using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.ViewModels;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.Economy {
    public class EconomyManager {

        public static EconomyManager Instance;

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }

        public Timer EcoTimer { get; set; }

        public EconomyManager(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }


        public void StartTickTimer() {
            EcoTimer = new Timer(new TimerCallback(ProcessEcoTick), null, 0, Timeout.Infinite);
        }

        public void StopTickTimer() {
            EcoTimer.Change(0, Timeout.Infinite);
        }

        public void ProcessEcoTick(object state) {
            if (!Settings.EcoTicksEnabled || !Parent.IsConnected) {
                Parent.LogMessage("Triggered eco tick but the client was not connected...");
                return;
            }

            Parent.LogMessage("Triggered eco tick");

            if (Settings.LastTick != null) {
                DateTime dueTime = Settings.LastTick + TimeSpan.FromSeconds(Settings.EcoTickTimeSeconds);
                if (dueTime > DateTime.Now) {
                    EcoTimer.Change(dueTime - DateTime.Now, TimeSpan.FromMilliseconds(-1));
                    return;
                }
            }

            foreach (Client client in Parent.Client.GetClientList(fromCache:false)) {
                if (GetBalanceForUser(client.UniqueId) < Settings.EcoSoftBalanceLimit) {
                    ChangeBalanceForUser(client.UniqueId, Settings.EcoTickGain);
                }
            }

            //Implicitly saves the file
            Settings.LastTick = DateTime.Now;
            EcoTimer.Change(TimeSpan.FromSeconds(Settings.EcoTickTimeSeconds), TimeSpan.FromMilliseconds(-1));
        }


        public static int GetBalanceForUser(string uid) {
            CheckUserHasData(uid);
            return Instance.Settings.UserBalances[uid];
        }

        public static void SetBalanceForUser(string uid, int value) {
            MySettings settings = Instance.Settings;
            CheckUserHasData(uid);
            value = Math.Max(0, Math.Min(settings.EcoHardBalanceLimit, value));
            settings.UserBalances[uid] = value;
            settings.DelayedSave();
        }

        public static void ChangeBalanceForUser(string uid, int change) {
            CheckUserHasData(uid);
            SetBalanceForUser(uid, GetBalanceForUser(uid) + change);
        }


        public static int GetUserBalanceAfterPaying(string uid, int price) {
            CheckUserHasData(uid);
            return GetBalanceForUser(uid) - price;
        }


        private static void CheckUserHasData(string uid) {
            MySettings settings = Instance.Settings;
            if (!settings.UserBalances.ContainsKey(uid)) {
                settings.UserBalances.Add(uid, 0);
                settings.DelayedSave();
            }
        }
    }
}
