using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.ViewModels;

namespace TeamspeakToolMvvm.Logic.Misc {
    public class CooldownManager {

        public static CooldownManager Instance;

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }

        public CooldownManager(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }

        public static void SetCooldown(string uid, string command, TimeSpan duration) {
            CheckHasValue(uid, command);
            Instance.Settings.CooldownEarliestPossibleCalls[uid][command] = DateTime.Now + duration;
            Instance.Settings.DelayedSave();
        }
        public static void SetCooldown(string uid, string command, DateTime earliestPossibleCall) {
            CheckHasValue(uid, command);
            Instance.Settings.CooldownEarliestPossibleCalls[uid][command] = earliestPossibleCall;
            Instance.Settings.DelayedSave();
        }

        public static void ThrowIfCooldown(string uid, string command) {
            if (HasCooldown(uid, command)) {
                DateTime earliestCall = Instance.Settings.CooldownEarliestPossibleCalls[uid][command];
                throw new CooldownNotExpiredException(earliestCall - DateTime.Now);
            }
        }

        public static bool HasCooldown(string uid, string command) {
            CheckHasValue(uid, command);
            DateTime earliestCall = Instance.Settings.CooldownEarliestPossibleCalls[uid][command];
            return earliestCall >= DateTime.Now;
        }

        public static void ResetCooldown(string uid, string command) {
            SetCooldown(uid, command, DateTime.Now);
        }
        public static void ResetCooldowns(string uid) {
            if (Instance.Settings.CooldownEarliestPossibleCalls.ContainsKey(uid)) {
                Instance.Settings.CooldownEarliestPossibleCalls.Remove(uid);
            }
        }


        public static string FormatCooldownTime(TimeSpan ts) {
            return Utils.FormatTimeSpanShort(ts);
        }


        private static void CheckHasValue(string uid, string command) {
            if (!Instance.Settings.CooldownEarliestPossibleCalls.ContainsKey(uid)) {
                Instance.Settings.CooldownEarliestPossibleCalls.Add(uid, new Dictionary<string, DateTime>());
            }
            if (!Instance.Settings.CooldownEarliestPossibleCalls[uid].ContainsKey(command)) {
                Instance.Settings.CooldownEarliestPossibleCalls[uid].Add(command, DateTime.MinValue);
            }
        }
    }
}
