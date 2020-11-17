using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Groups {
    public class BannedGroup : Group {
        public override string DisplayName { get; set; } = "Banned";
        public override string Name { get; set; } = "banned";
        public override string Description { get; set; } = "A banned user can not use any commands.";
        public override bool AccessAll { get; set; } = false;
        public override bool IsDefault { get; set; } = false;
        public override Group InheritGroup { get; set; } = null;
        public override Dictionary<Type, bool> CommandAccesses { get; set; } = new Dictionary<Type, bool>();
        public override Dictionary<string, bool> SubCommandAccesses { get; set; } = new Dictionary<string, bool>();
    }
}
