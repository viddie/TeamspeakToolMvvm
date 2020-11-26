using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {
    public class CooldownNotExpiredException : Exception {

        public TimeSpan Duration { get; set; }

        public CooldownNotExpiredException(TimeSpan duration) {
            Duration = duration;
        }
    }
}
