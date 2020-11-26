using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Exceptions {
    [Serializable]
    class CommandParameterInvalidFormatException : Exception {

        public int ParameterPosition { get; set; }
        public object ParameterValue { get; set; }
        public string ParameterName { get; set; }
        public Type ParameterType { get; set; }
        public string UsageHelp { get; set; }

        public CommandParameterInvalidFormatException(int pos, object val, string name, Type type, string help) {
            ParameterPosition = pos;
            ParameterValue = val;
            ParameterName = name;
            ParameterType = type;
            UsageHelp = help;
        }
        public CommandParameterInvalidFormatException(string message) : base(message) { }
        public CommandParameterInvalidFormatException(string message, Exception innerEx) : base(message, innerEx) { }

        public string GetParameterPosition() {
            if (ParameterPosition % 10 == 1 && ParameterPosition != 11) {
                return $"{ParameterPosition}st";
            }
            if (ParameterPosition % 10 == 2 && ParameterPosition != 12) {
                return $"{ParameterPosition}nd";
            }
            if (ParameterPosition % 10 == 3 && ParameterPosition != 13) {
                return $"{ParameterPosition}rd";
            }

            return $"{ParameterPosition}th";
        }

        public string GetNeededType() {
            //The returned string will be lead by 'The parameter has to be {returnedValue}'

            if (ParameterType == typeof(bool)) {
                return "true or false";

            } else if (ParameterType == typeof(int)) {
                return "a whole number";

            } else if (ParameterType == typeof(uint)) {
                return "a positive whole number";

            } else if (ParameterType == typeof(double)) {
                return "a number";

            }

            return "idk";
        }
    }
}
