using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Groups {
    [Serializable]
    public abstract class Group {
        public abstract string DisplayName { get; set; }
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract bool AccessAll { get; set; }
        public abstract bool AccessNone { get; set; }
        public abstract bool IsDefault { get; set; }
        public abstract Group InheritGroup { get; set; }

        public abstract Dictionary<Type, bool> CommandAccesses { get; set; }
        public abstract Dictionary<string, bool> SubCommandAccesses { get; set; }

    }
}
