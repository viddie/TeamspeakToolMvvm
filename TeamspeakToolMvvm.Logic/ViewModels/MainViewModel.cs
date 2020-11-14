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

namespace TeamspeakToolMvvm.Logic.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Properties
        public MySettings Settings { get; set; }
        public TeamspeakClient Client { get; set; }
        #endregion

        #region View Properties
        public string Title { get; set; } = "Teamspeak Tool - " + Assembly.GetExecutingAssembly().GetName().Version;
        public bool IsConnected { get; set; } = false;

        public string ConnectButtonText { get; set; } = "Connect";

        public List<CommandModel> Commands { get; set; }
        #endregion

        #region Commands
        public RelayCommand OpenSettingsCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }

        #endregion

        #region Teamspeak Properties
        private int MyClientId { get; set; } = -1;
        private int MyChannelId { get; set; } = -1;
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

            if (Settings.ClientAuthKey != "<your-api-key-here>") {
                HandleConnectCommand();
            }

            Commands = new List<CommandModel>() {
                new CommandModel() { DisplayName = "Mass-Poke My Channel", Command = new RelayCommand(MassPokeInMyChannel), IconName = "HandOutlineUp" },
                new CommandModel() { DisplayName = "Open chat with yourself", Command = new RelayCommand(OpenChatWithMyself), IconName = "Commenting" },
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
            (int myId, int myChannelId) = Client.GetWhoAmI();

            List<Client> clients = Client.GetClientsInChannel(myChannelId);
            foreach (Client client in clients) {
                Client.SendCommand($"clientpoke clid={client.Id} msg=Poke\\sTest");
            }
        }

        public void OpenChatWithMyself() {
            (int myId, int myChannelId) = Client.GetWhoAmI();
            Client.SendCommand($"sendtextmessage targetmode=1 target={myId} msg=Hola");
        }

        
        #endregion
    }
}
