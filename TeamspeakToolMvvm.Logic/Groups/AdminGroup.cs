using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Groups {
    public class AdminGroup : Group {
        public override string DisplayName { get; set; } = "Admin";
        public override string Name { get; set; } = "admin";
        public override string Description { get; set; } = "The admin group has control over all features";
        public override bool AccessAll { get; set; } = true;
        public override bool AccessNone { get; set; } = false;
        public override bool IsDefault { get; set; } = false;
        public override Group InheritGroup { get; set; }
        public override Dictionary<Type, bool> CommandAccesses { get; set; } = new Dictionary<Type, bool>();
        public override Dictionary<string, bool> SubCommandAccesses { get; set; } = new Dictionary<string, bool>();
    }
}
