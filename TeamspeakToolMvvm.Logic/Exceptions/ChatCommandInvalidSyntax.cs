using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {
    [Serializable]
    public class ChatCommandInvalidSyntaxException : Exception {
        public ChatCommandInvalidSyntaxException() { }
        public ChatCommandInvalidSyntaxException(string message) : base(message) { }
        public ChatCommandInvalidSyntaxException(string message, Exception innerEx) : base(message, innerEx) { }
    }
}
