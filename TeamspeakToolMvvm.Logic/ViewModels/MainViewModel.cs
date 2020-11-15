using TSClient;
using AdvancedSettings.Logic.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TeamspeakToolMvvm.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TSClient.Exceptions;
using System.Threading;
using TeamspeakToolMvvm.Logic.Messages;
using TeamspeakToolMvvm.Logic.Models;
using TSClient.Models;
using TSClient.Events;
using TSClient.Enums;

namespace TeamspeakToolMvvm.Logic.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Properties
        public MySettings Settings { get; set; }
        public TeamspeakClient Client { get; set; }
        public int KeepAliveSeconds { get; set; } = 300;
        #endregion

        #region View Properties
        public string Title { get; set; } = "Teamspeak Tool - " + Assembly.GetExecutingAssembly().GetName().Version;
        public bool IsConnected { get; set; } = false;

        public string ConnectButtonText { get; set; } = "Connect";


        public bool HasSelectedClient { get; set; } = true;
        public bool HasSelectedChannel { get; set; } = false;

        public List<CommandModel> GlobalCommands { get; set; }
        public List<CommandModel> ClientCommands { get; set; }
        public List<CommandModel> ChannelCommands { get; set; }
        #endregion

        #region Commands
        public RelayCommand OpenSettingsCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }

        #endregion

        #region Teamspeak Properties
        public int MyClientId { get; set; } = -1;
        public int MyChannelId { get; set; } = -1;
        #endregion

        public MainViewModel() {
            OpenSettingsCommand = new RelayCommand(HandleOpenSettingsCommand);
            ConnectCommand = new RelayCommand(HandleConnectCommand);

            #region Settings
            Settings = MySettings.Instance;
#if DEBUG
            Settings.Load<MySettings>("test_settings.json");
#else
            Settings.Load<MySettings>("settings.json");
#endif
            Settings.SaveTimerDelay = 1000;
            #endregion

            //Auto connect if API key is set
            if (Settings.ClientAuthKey != "<your-api-key-here>") {
                HandleConnectCommand();
            }

            //Initialize Commands
            GlobalCommands = new List<CommandModel>() {
                new CommandModel() { DisplayName = "Mass-Poke My Channel", Command = new RelayCommand(MassPokeInMyChannel), IconName = "HandOutlineUp" },
                new CommandModel() { DisplayName = "Open chat with yourself", Command = new RelayCommand(OpenChatWithMyself), IconName = "Commenting" },
                new CommandModel() { DisplayName = "Toggle no-move", Command = new RelayCommand(ToggleNoMove), IconName = "Ban" },
            };

            RegisterMessages();
        }

        private void RegisterMessages() {
            MessengerInstance.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);
        }

        #region Command Handlers
        public void HandleOpenSettingsCommand() {
            MySettingsViewModel svm = new MySettingsViewModel(Settings);
            MessengerInstance.Send(new OpenSettingsViewMessage<MySettingsViewModel>(svm, null, () => {
                svm.OnSave();
            }));
        }

        public async void HandleConnectCommand() {
            string authKey = Settings.ClientAuthKey;

            Client = new TeamspeakClient(Settings.ClientHost, Settings.ClientPort);
            try {
                ConnectButtonText = "Connecting...";
                await Task.Run(() => {
                    Client.ConnectClient();
                    Client.AuthorizeClient(authKey);
                    AfterConnectionInit();
                });
                IsConnected = true;
                ConnectButtonText = "Connected!";

            } catch (TeamspeakConnectionException) {
                IsConnected = false;
                ConnectButtonText = "Error";
            } catch (Exception) {
                IsConnected = false;
                ConnectButtonText = "Error";
            }
        }

        public void AfterConnectionInit() {
            //Initialize KeepAlive timer
            Timer t = new Timer(new TimerCallback((o) => {
                Client.SendCommand("whoami");
            }), null, 1000 * KeepAliveSeconds, 1000 * KeepAliveSeconds);


            Client.GetClientList(false);

            //Register No-Move
            Client.RegisterEventCallback(typeof(NotifyClientMovedEvent), OnClientMoved);
        }
        #endregion

        #region Message Handlers
        public void HandleApplicationClosingMessage(ApplicationClosingMessage msg) {
            Client?.CloseClient();
            Client?.CleanupClient();
        }
        #endregion

        #region Methods

        #endregion

        #region TS3 Actions
        public void MassPokeInMyChannel() {
            int myChannelId = GetMyChannelId();

            List<Client> clients = Client.GetClientsInChannel(myChannelId);
            foreach (Client client in clients) {
                Client.SendCommand($"clientpoke clid={client.Id} msg=Poke\\sTest");
            }
        }

        public void OpenChatWithMyself() {
            int myId = GetMyClientId();
            Client.SendCommand($"sendtextmessage targetmode=1 target={myId} msg=Hola");
        }

        public void ToggleNoMove() {
            Settings.NoMoveEnabled = !Settings.NoMoveEnabled;
        }

        public void OnNoMoveEvent(NotifyClientMovedEvent evt) {
            if (!Settings.NoMoveEnabled) return;
            if (evt.Reason == ClientMoveReason.SelfMove || evt.ClientId == evt.InvokerId) return; //Ignore channel switching

            int myId = GetMyClientId();
            if (myId == evt.ClientId) {
                Client me = GetClientById(myId);
                Client.SendCommand($"clientmove cid={me.ChannelId} clid={myId}");
                return;
            }


            string otherExcludeName = Settings.NoMoveUsername;
            Client otherClient = GetClientById(evt.ClientId);
            if (otherExcludeName == otherClient.Nickname) {
                Client.SendCommand($"clientmove cid={otherClient.ChannelId} clid={otherClient.Id}");
                return;
            }
        }
        #endregion


        #region TS3 Calls
        public int GetMyClientId() {
            if (MyClientId == -1) {
                (MyClientId, MyChannelId) = Client.GetWhoAmI();
            }

            return MyClientId;
        }

        public int GetMyChannelId() {
            (MyClientId, MyChannelId) = Client.GetWhoAmI();
            return MyChannelId;
        }

        public Client GetClientById(int id) {
            List<Client> clients = Client.GetClientList();

            foreach (Client client in clients) {
                if (client.Id == id) {
                    return client;
                }
            }

            return null;
        }
        #endregion


        #region TS3 Events
        public void OnClientMoved(Event e) {
            NotifyClientMovedEvent evt = (NotifyClientMovedEvent)e;

            OnNoMoveEvent(evt);

            Client.GetClientList(false);
            //Client.SendCommand("clientlist");
        }
        #endregion
    }
}
