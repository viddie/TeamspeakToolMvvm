using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Messages {
    public class AddLogMessage {
        public string Message { get; set; }

        public AddLogMessage(string s) {
            Message = s;
        }
    }
}
