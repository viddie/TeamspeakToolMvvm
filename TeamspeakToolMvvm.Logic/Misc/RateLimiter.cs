using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.ViewModels;

namespace TeamspeakToolMvvm.Logic.Misc {
    public class RateLimiter {

        public Dictionary<string, Dictionary<string, List<DateTime>>> RateLimitTracker = new Dictionary<string, Dictionary<string, List<DateTime>>>();

        public MySettings Settings;

        public RateLimiter(MySettings settings) {
            Settings = settings;
        }

        public bool CheckRateLimit(string cmd, string uniqueId, bool isAdmin=false) {
            if (!RateLimitTracker.ContainsKey(cmd)) {
                RateLimitTracker.Add(cmd, new Dictionary<string, List<DateTime>>());
            }

            Dictionary<string, List<DateTime>> cmdRateLimits = RateLimitTracker[cmd];

            if (!cmdRateLimits.ContainsKey(uniqueId)) {
                cmdRateLimits.Add(uniqueId, new List<DateTime>());
            }

            List<DateTime> cmdUserCalls = cmdRateLimits[uniqueId];
            cmdUserCalls.RemoveAll((dt) => (DateTime.Now - dt).TotalSeconds > 60);

            int limit = Settings.DefaultRateLimitPerMinute;
            if (isAdmin) limit = Settings.DefaultRateLimitPerMinuteAdmin;

            bool canCall = cmdUserCalls.Count < limit;
            if (canCall) {
                cmdUserCalls.Add(DateTime.Now);
            }
            return canCall;
        }
    }
}
