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
                new CommandModel() { DisplayName = "Test Command", Command = new RelayCommand(TestCommand), IconName = "Wrench" },
                new CommandModel() { DisplayName = "Reload Client List", Command = new RelayCommand(ReloadClientList), IconName = "Refresh" },
            };

            RegisterMessages();
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
        #endregion

        #region TS3 Actions
        public void MassPokeInMyChannel() {
            if (!CheckConnection()) return;

            int myChannelId = GetMyChannelId();

            LogMessage($"Mass poking all clients in your channel...");
            List<Client> clients = Client.GetClientsInChannel(myChannelId);
            foreach (Client client in clients) {
                LogMessage($"  > Poking '{client.Nickname}'");
                Client.SendCommand($"clientpoke clid={client.Id} msg=Poke\\sTest");
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

            string data = @"1;AYAYA;AYAYA;https://www.youtube.com/watch?v=mwNQr_kMNis
1;disappointed;Disappointed;https://www.youtube.com/watch?v=Ncgv7ruZ6HU
1;doot_doot;Doot Doot;https://www.youtube.com/watch?v=WTWyosdkx44
1;ey_deutschland;Deutschland;https://www.youtube.com/watch?v=n-iMnBqYFyA
1;fuck_you;FUCK YOU;
1;ich_liebe_sie;Ich liebe Sie;https://www.youtube.com/watch?v=h6oS2-V0OUk
1;im_gay;Im gay;https://www.youtube.com/watch?v=P-_GWUw8LwM
1;kirby;kirby;https://twitter.com/DitzyFlama/status/974655111015284739
1;nochmal;Nochmal;https://www.youtube.com/watch?v=HVwVAt88ZhI
1;oof;Oof;https://www.youtube.com/watch?v=_4vQ6ZQGdnE
1;profanity;Profanity;https://www.youtube.com/watch?v=hpigjnKl7nI
1;why_are_you_running;Why are you running;https://www.youtube.com/watch?v=W6oQUDFV2C0
1;bruh;Bruh;-> Maik
1;aha;Aha;
1;anderername;kys;
2;Lena;Lena;https://youtu.be/s-2uAc6_xAQ?t=714
2;drd_cheating;forsenCD;https://www.youtube.com/watch?v=_enuY4vFvWc
2;huenchen_mit_reis;Huehnchen mit Reis;https://www.youtube.com/watch?v=-d0_loBKXmQ
2;monte;Monte;-> JayD
2;pewds;PewDiePie;https://www.youtube.com/watch?v=J_bMArMJ-f8
2;smb_gameover;Game Over;
2;sooogoood;So Good;https://www.youtube.com/watch?v=HiN6siMYy1M
2;ohyeah;Oh Yeah!;-> Maik
2;alfredo;Alfredo;
2;ohoho;Ohoho;
3;YEAH;YEAH;https://www.youtube.com/watch?v=6YMPAH67f4o
3;blyat;Blyat;-> Maik
3;for_the_damaged_coda;Damaged Coda;https://www.youtube.com/watch?v=JBd0ERZhCis&feature=youtu.be&t=19s
3;iphone_alarm;iPhone Alarm;https://www.youtube.com/watch?v=VOp8bB0IZQs
3;nein;NEIN;https://www.youtube.com/watch?v=xoMgnJDXd3k
3;stupid_fucking_mistakes_man;Stupid fucking mistakes;https://www.youtube.com/watch?v=PfCxixhdHa0
3;Autism2;Autism 2;-> Nick
3;icallmain;I call main;-> Arthur aka. 'Maverick' aka. 'Skill Legende'
3;daddy;Daddy;
3;alte_bilder;Alte Bilder;
3;alte_bilder_jayd;JayD Bilder;
3;ehre;Ehre;
4;apes;Apes;https://www.youtube.com/watch?v=ncrrM4uKLFk
4;ocean_man;Ocean Man;https://www.youtube.com/watch?v=tkzY_VwNIek
4;patrick;Patrick;https://www.youtube.com/watch?v=e5suwZ5C6to
4;yee;Yee;https://www.youtube.com/watch?v=q6EoRBvdVPQ
4;Soos;Soos;
5;roundabout;Roundabout;https://www.youtube.com/watch?v=cPCLFtxpadE
5;Gaense;Gaense;https://www.youtube.com/watch?v=ClYY7-zILXE
5;andreas;Halt stop;https://www.youtube.com/watch?v=TDEpLi-9WU8
5;big_dick;Big dick;https://www.youtube.com/watch?v=i63cgUeSsY0
5;gay_frogs;Gay Frogs;https://www.youtube.com/watch?v=_yR84yorg0U
5;nitenite;nitenite;https://www.youtube.com/watch?v=rVD1v7j0HIQ
5;oh_whoops;Oh whoops;https://www.youtube.com/watch?v=us5MGEL5W34
5;Crab;Crab rave;https://youtu.be/LDU_Txk06tM
5;444;Trier;https://www.youtube.com/watch?v=h4QUC0aXN8M
6;Dance;Dance;
6;flute;Flute;https://www.youtube.com/watch?v=ootqCFoJMdo
6;i_still_love_you;I still love you;https://www.youtube.com/watch?v=2lsjdUUha4s
6;running_in_the_90s;Running in the 90s;https://www.youtube.com/watch?v=BJ0xBCwkg3E
6;surprise_buttsecks;Surprise;https://www.youtube.com/watch?v=J5MZyJZSN3k
6;woran_hats_gelegen;Woran hats gelegen;https://www.youtube.com/watch?v=eHKZlXlqcS4
6;Autism1;Autism 1;-> Nick
6;Fische;Fische;
6;magnetprobe;Magnetprobe;https://www.youtube.com/watch?v=noSUZfjq3f4
6;hiit_or_miss;Hiit or miss;
7;ANELE;ANELE;https://www.youtube.com/watch?v=oxtJnR8wzYc
8;fortnite;Fortnite;
8;victory_screech;Victory Screech;https://www.youtube.com/watch?v=MdN0NXgjsn8
8;muffin_song;Muffin Time;https://www.youtube.com/watch?v=LACbVhgtx9I
10;t1_machine_gun;T1;https://www.youtube.com/watch?v=gIKf_IL1DB8
12;hitormiss;Hit or miss;-> Zed der hurensohn
12;tracer_fotze;Tracer;https://youtu.be/d0RmRJsgP28?t=95
100;Hearingtest;Hearing Test;No >:(";

            string[] lines = data.Split('\n');
            foreach (string line in lines) {
                string[] lineSplit = line.Split(';');
                int price = int.Parse(lineSplit[0].Trim());
                string fileName = lineSplit[1].Trim() + ".wav";
                string name = lineSplit[2].Trim();
                string source = lineSplit[3].Trim();

                Playsound sound = new Playsound() {
                    Name = name,
                    FileName = fileName,
                    BasePrice = price,
                    Source = source
                };

                if (Settings.PlaysoundsSavedSounds.FirstOrDefault(ps => ps.Name == name) == null) {
                    Settings.PlaysoundsSavedSounds.Add(sound);
                }
            }

            Settings.DelayedSave();
        }

        public void ReloadClientList() {
            Client.GetClientList(false);
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
            if (Settings.ImmediateYoutubeEnabled && AudioHelper.IsYouTubeVideoUrl(evt.Message)) {
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

            CommandHandler.HandleTextMessage(evt, messageCallback);
            OnImmediateChatReaction(evt, messageCallback);
        }
        #endregion
    }
}
