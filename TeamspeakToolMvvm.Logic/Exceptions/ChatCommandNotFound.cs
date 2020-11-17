using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {

    [Serializable]
    public class ChatCommandNotFoundException : Exception {
        public ChatCommandNotFoundException() { }
        public ChatCommandNotFoundException(string message) : base(message) { }
        public ChatCommandNotFoundException(string message, Exception innerEx) : base(message, innerEx) { }
    }
}
