using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {
    public class TimeStringParseException : Exception {
        public TimeStringParseException() { }
        public TimeStringParseException(string message) : base(message) { }
        public TimeStringParseException(string message, Exception innerEx) : base(message, innerEx) { }

    }
}
