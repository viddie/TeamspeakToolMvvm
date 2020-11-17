using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Groups {
    public class ModeratorGroup : Group {
        public override string DisplayName { get; set; } = "Moderator";
        public override string Name { get; set; } = "moderator";
        public override string Description { get; set; } = "Moderators have access to some advanced commands to allow for moderation of the application";
        public override bool AccessAll { get; set; } = false;
        public override bool IsDefault { get; set; } = false;

        public override Group InheritGroup { get; set; }

        public override Dictionary<Type, bool> CommandAccesses { get; set; } = new Dictionary<Type, bool>() {

        };
        public override Dictionary<string, bool> SubCommandAccesses { get; set; } = new Dictionary<string, bool>() {
            ["command:groups_list"] = true,
        };
    }
}
