using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {

    [Serializable]
    public class GroupNotFoundException : Exception {
        public GroupNotFoundException() { }
        public GroupNotFoundException(string message) : base(message) { }
        public GroupNotFoundException(string message, Exception innerEx) : base(message, innerEx) { }
    }
}
