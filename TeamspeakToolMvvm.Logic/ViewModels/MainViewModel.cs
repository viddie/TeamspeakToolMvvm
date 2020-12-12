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
using TSClient.Helpers;
using TeamspeakToolMvvm.Logic.ChatCommands;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Economy;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.IO;

namespace TeamspeakToolMvvm.Logic.ViewModels {
    public class MainViewModel : ViewModelBase {

        #region Properties
        public MySettings Settings { get; set; }
        public AoeSettings EloSettings { get; set; }
        public StatisticSettings StatSettings { get; set; }

        public TeamspeakClient Client { get; set; }
        public Timer KeepAliveTimer { get; set; }
        public int KeepAliveSeconds { get; set; } = 180;

        public ChatCommandHandler CommandHandler { get; set; }
        public RateLimiter RateLimiter { get; set; }

        public List<string> SelfMessagesToIgnore { get; set; } = new List<string>();
        #endregion

        #region View Properties
        public string Title { get; set; } = "Teamspeak Tool - " + Assembly.GetExecutingAssembly().GetName().Version;
        public bool IsConnected { get; set; } = false; 
        public bool ConnectButtonVisible { get; set; } = false;
        public string ConnectButtonText { get; set; } = "Connect";


        public bool HasSelectedClient { get; set; } = true;
        public bool HasSelectedChannel { get; set; } = false;

        public List<CommandModel> GlobalCommands { get; set; }
        public List<CommandModel> ClientCommands { get; set; }
        public List<CommandModel> ChannelCommands { get; set; }

        public string LastRouletteResult { get; set; } = "";
        public string YoutubePlaysoundsFileSize { get; set; } = "0 bytes";
        public long LastYoutubeFileSizeBytes { get; set; } = 0;

        public ObservableCollection<string> LogTexts { get; set; } = new ObservableCollection<string>();
        #endregion

        #region Commands
        public RelayCommand OpenSettingsCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }

        #endregion

        #region Teamspeak Properties
        public int MySchandlerId { get; set; } = -1;
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

            #region Aoe Elo Settings
            EloSettings = AoeSettings.Instance;
            EloSettings.Load<AoeSettings>("aoe_elos.json");
            EloSettings.SaveTimerDelay = 1000;
            #endregion

            #region Statistics Settings
            StatSettings = StatisticSettings.Instance;
            StatSettings.Load<StatisticSettings>("statistics.json");
            StatSettings.SaveTimerDelay = 1000;
            #endregion


            CommandHandler = new ChatCommandHandler(this);
            RateLimiter = new RateLimiter(Settings);
            AccessManager.Instance = new AccessManager(this);
            EconomyManager.Instance = new EconomyManager(this);
            CooldownManager.Instance = new CooldownManager(this);

            //Auto connect if API key is set
            if (Settings.ClientAuthKey != "<your-api-key-here>") {
                HandleConnectCommand();
            }

            //Initialize Commands
            GlobalCommands = new List<CommandModel>() {
                new CommandModel() { DisplayName = "Mass-Poke My Channel", Command = new RelayCommand(MassPokeInMyChannel), IconName = "HandOutlineUp" },
                new CommandModel() { DisplayName = "Open chat with yourself", Command = new RelayCommand(OpenChatWithMyself), IconName = "Commenting" },
                new CommandModel() { DisplayName = "Toggle no-move", Command = new RelayCommand(ToggleNoMove), IconName = "Ban" },
                new CommandModel() { DisplayName = "Toggle door", Command = new RelayCommand(ToggleDoorChannel), IconName = "ArrowRight" },
                new CommandModel() { DisplayName = "Reload Client List", Command = new RelayCommand(ReloadClientList), IconName = "Refresh" },
                new CommandModel() { DisplayName = "Toggle YT 1-Point Event", Command = new RelayCommand(ToggleYtOnePointEvent), IconName = "None" },
                new CommandModel() { DisplayName = "Purge YouTube playsounds folder", Command = new RelayCommand(PurgeYouTubePlaysoundsFolder), IconName = "Trash" },
                new CommandModel() { DisplayName = "Test Command", Command = new RelayCommand(TestCommand), IconName = "Wrench" },
            };

            RegisterMessages();


            UpdateYoutubeFolderSize();
        }

        private void RegisterMessages() {
            MessengerInstance.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);
        }

        #region Command Handlers
        public void HandleOpenSettingsCommand() {
            MySettingsViewModel svm = new MySettingsViewModel(Settings);
            svm.LoadSettings();
            MessengerInstance.Send(new OpenSettingsViewMessage<MySettingsViewModel>(svm, null, () => {
                svm.OnSave();
            }));
        }

        public async void HandleConnectCommand() {
            if (IsConnected) return;

            string authKey = Settings.ClientAuthKey;

            if (Client != null) Client.CloseClient();

            Client = new TeamspeakClient(Settings.ClientHost, Settings.ClientPort);
            try {
                ConnectButtonText = "Connecting...";
                await Task.Run(() => {
                    try {
                        Client.ConnectClient();
                        Client.AuthorizeClient(authKey);
                        Client.ConnectionClosedCallback = OnConnectionClosed;
                        AfterConnectionInit(true);
                    } catch (Exception) {
                        AfterConnectionInit(false);
                    }
                });
            } catch (TeamspeakConnectionException) {
                AfterConnectionInit(false);
            } catch (Exception) {
                AfterConnectionInit(false);
            }
        }

        public void AfterConnectionInit(bool success) {
            if (!success) {
                IsConnected = false;
                ConnectButtonVisible = true;
                ConnectButtonText = "Error";
                Task.Run(() => {
                    Thread.Sleep(3000);
                    ConnectButtonText = "Connect";
                });
                return;
            }

            IsConnected = true;
            ConnectButtonText = "Connected!";
            Task.Run(() => {
                Thread.Sleep(3000);
                ConnectButtonVisible = false;
            });

            //Initialize KeepAlive timer
            if (KeepAliveTimer != null) KeepAliveTimer.Dispose();
            KeepAliveTimer = new Timer(new TimerCallback((o) => {
                Client.SendCommand("whoami");
            }), null, 1000 * KeepAliveSeconds, 1000 * KeepAliveSeconds);

            //Start economy timer
            EconomyManager.Instance.StartTickTimer();

            (MyClientId, MyChannelId) = Client.GetWhoAmI();
            Client.GetClientList(false);
            Client.GetChannelList(false);

            //Register for: No-Move, Door
            Client.RegisterEventCallback(typeof(NotifyClientMovedEvent), OnClientMoved);

            //Register for: Chat Commands
            Client.RegisterEventCallback(typeof(NotifyTextMessageEvent), OnTextMessageEvent);
        }

        public void OnConnectionClosed() {
            IsConnected = false;
            ConnectButtonVisible = true;
            ConnectButtonText = "Connect";
        }

        public bool CheckConnection() {
            if (!IsConnected) {
                MessengerInstance.Send(new DisplayErrorMessage() { Caption = "Connection lost", Message = "The client is currently not connected..." });
            }

            return IsConnected;
        }
        #endregion

        #region Message Handlers
        public void HandleApplicationClosingMessage(ApplicationClosingMessage msg) {
            Client?.CloseClient();
            Client?.CleanupClient();
        }
        #endregion

        #region Methods
        public void LogMessage(string text) {
            MessengerInstance.Send(new AddLogMessage(text));
        }

        public void IgnoreSelfTextMessage(string text) {
            SelfMessagesToIgnore.Add(text);
        }

        public void UpdateYoutubeFolderSize() {
            long totalBytes = 0;
            foreach (string file in Directory.GetFiles(Settings.PlaysoundsYoutubeFolder)) {
                totalBytes += new FileInfo(file).Length;
            }

            string plusSign = totalBytes - LastYoutubeFileSizeBytes > 0 ? "+" : "";
            string addedAddition = LastYoutubeFileSizeBytes == 0 ? "" : $" ({plusSign}{Misc.Utils.FormatBytes(totalBytes - LastYoutubeFileSizeBytes)})";

            YoutubePlaysoundsFileSize = $"{Misc.Utils.FormatBytes(totalBytes)}{addedAddition}";
            LastYoutubeFileSizeBytes = totalBytes;
        }
        #endregion

        #region TS3 Actions
        public void MassPokeInMyChannel() {
            if (!CheckConnection()) return;

            int myChannelId = GetMyChannelId();

            LogMessage($"Mass poking all clients in your channel...");
            List<Client> clients = Client.GetClientsInChannel(myChannelId);
            foreach (Client client in clients) {
                LogMessage($"  > Poking '{client.Nickname}'");
                Client.SendCommand($"clientpoke clid={client.Id} msg");
            }
        }

        public void OpenChatWithMyself() {
            if (!CheckConnection()) return;

            LogMessage($"Opened chat with yourself");
            int myId = GetMyClientId();
            Client.SendCommand($"sendtextmessage targetmode=1 target={myId} msg=Hola");
        }

        public void ToggleNoMove() {
            Settings.NoMoveEnabled = !Settings.NoMoveEnabled;
        }
        public void ToggleDoorChannel() {
            Settings.DoorChannelEnabled = !Settings.DoorChannelEnabled;
        }

        public void TestCommand() {
            LogMessage("Executing test command...");
        }

        public void ReloadClientList() {
            Client.GetClientList(false);
        }

        public void ToggleYtOnePointEvent() {
            if (!Settings.PlaysoundsYoutubeOnePointEvent) {
                Client.SendChannelMessage(ColorCoder.SuccessDim($"YouTube playsounds 1-Point event!\n\t{ColorCoder.Bold($"- All youtube playsounds shorter than 60 seconds only cost 1 point! -")}"));
            } else {
                Client.SendChannelMessage($"1-Point event stopped.");
            }

            Settings.PlaysoundsYoutubeOnePointEvent = !Settings.PlaysoundsYoutubeOnePointEvent;
        }

        public void PurgeYouTubePlaysoundsFolder() {
            foreach (string file in Directory.GetFiles(Settings.PlaysoundsYoutubeFolder)) {
                File.Delete(file);
            }

            LogMessage($"Purged '{YoutubePlaysoundsFileSize}' of playsound cache data");
            UpdateYoutubeFolderSize();
        }
        #endregion


        #region Client Move Handlers
        public void OnNoMoveEvent(NotifyClientMovedEvent evt) {
            if (!Settings.NoMoveEnabled) return;
            if (evt.Reason == ClientMoveReason.SelfMove || evt.ClientId == evt.InvokerId) return; //Ignore channel switching

            int myId = GetMyClientId();
            if (myId == evt.ClientId) {
                Client me = Client.GetClientById(myId);
                Client.SendCommand($"clientmove cid={me.ChannelId} clid={myId}");
                LogMessage($"No-Move triggered for yourself");
                StatSettings.MovesDenied++;
                return;
            }


            if (!Settings.HasAdmin) return; //Others can only be protected with admin privileges
            if (string.IsNullOrEmpty(Settings.NoMoveUsername)) return;

            string otherExcludeNames = Settings.NoMoveUsername;
            List<string> namesList = otherExcludeNames.Split(new char[] { ',' }).ToList();

            Client otherClient = Client.GetClientById(evt.ClientId);
            if (otherClient != null && namesList.Contains(otherClient.Nickname)) {
                Client.SendCommand($"clientmove cid={otherClient.ChannelId} clid={otherClient.Id}");
                LogMessage($"No-Move triggered for '{otherClient.Nickname}'");
                StatSettings.MovesDenied++;
                return;
            }
        }

        private void OnDoorMoveEvent(NotifyClientMovedEvent evt) {
            if (!Settings.HasAdmin) return;
            if (!Settings.DoorChannelEnabled) return;
            if (string.IsNullOrEmpty(Settings.DoorChannelName)) return;

            string doorName = Settings.DoorChannelName;
            string doorMessage = string.IsNullOrEmpty(Settings.DoorMessage) ? "Left the house." : Settings.DoorMessage;
            doorMessage = ModelParser.ToStringAttribute(doorMessage);

            Channel channel = Client.GetChannelById(evt.ToChannelId);
            if (channel != null && channel.Name == doorName) {
                LogMessage($"{Client.GetClientById(evt.ClientId).Nickname} used the door channel");
                Client.SendCommand($"clientkick reasonid=5 reasonmsg={doorMessage} clid={evt.ClientId}");
                StatSettings.DoorUsed++;
            }
        }
        #endregion

        #region Client Text Message Handlers
        public void OnImmediateChatReaction(NotifyTextMessageEvent evt, Action<string> messageCallback) {
            if (Settings.ImmediateYoutubeEnabled && AudioHelper.IsYouTubeVideoUrl(evt.Message) != null) {
                evt.Message = $"!yt {evt.Message}";
                CommandHandler.HandleTextMessage(evt, messageCallback);
            } else if(Settings.ImmediateGeneralEnabled) {
                if (evt.Message.ToLower().StartsWith("[url]") && evt.Message.ToLower().EndsWith("[/url]")) {
                    string url = evt.Message.Substring(5, evt.Message.Length - 11);
                    string generalInfo = Scrapers.ScrapeGeneral(url);
                    if (generalInfo != null) {
                        messageCallback.Invoke(generalInfo);
                    }
                }
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


        public void RefreshClientList() {
            Client.GetClientList(false);

            foreach (Client client in Client.GetClientList()) {
                Settings.LastSeenUsernames[client.UniqueId] = client.Nickname;
            }

            Settings.DelayedSave();
        }
        #endregion


        #region TS3 Events
        public void OnClientMoved(Event e) {
            NotifyClientMovedEvent evt = (NotifyClientMovedEvent)e;

            OnNoMoveEvent(evt);
            OnDoorMoveEvent(evt);

            if (evt.ClientId == MyClientId) {
                MyChannelId = evt.ToChannelId;
            }

            RefreshClientList();
        }

        public void OnTextMessageEvent(Event e) {
            NotifyTextMessageEvent evt = (NotifyTextMessageEvent)e;

            int myId = GetMyClientId();

            if (evt.InvokerId == myId && SelfMessagesToIgnore.Contains(evt.Message)) {
                SelfMessagesToIgnore.Remove(evt.Message);
                return;
            }

            Action<string> messageCallback = null;
            if (evt.TargetMode == MessageMode.Private) {
                if (evt.InvokerId == myId) {
                    messageCallback = (s) => Client.SendPrivateMessage(s, evt.Target);
                } else {
                    messageCallback = (s) => Client.SendPrivateMessage(s, evt.InvokerId);
                }
            } else if (evt.TargetMode == MessageMode.Channel) {
                messageCallback = Client.SendChannelMessage;
            } else if (evt.TargetMode == MessageMode.Server) {
                messageCallback = Client.SendServerMessage;
            }

            bool handled = CommandHandler.HandleTextMessage(evt, messageCallback);
            if (!handled) {
                OnImmediateChatReaction(evt, messageCallback);
            }
        }
        #endregion
    }
}
