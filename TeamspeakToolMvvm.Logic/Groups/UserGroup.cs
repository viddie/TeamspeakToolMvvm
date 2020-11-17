﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.ChatCommands;

namespace TeamspeakToolMvvm.Logic.Groups {
    public class UserGroup : Group {
        public override string DisplayName { get; set; } = "User";
        public override string Name { get; set; } = "user";
        public override string Description { get; set; } = "The default user group. Has Access to most commands";
        public override bool AccessAll { get; set; } = false;
        public override bool IsDefault { get; set; } = true;
        public override Group InheritGroup { get; set; } = null;
        public override Dictionary<Type, bool> CommandAccesses { get; set; } = new Dictionary<Type, bool>() {
            [typeof(YouTubeCommand)] = true,
            [typeof(TimeCommand)] = true,
            [typeof(CoinFlipCommand)] = true,
        };
        public override Dictionary<string, bool> SubCommandAccesses { get; set; } = new Dictionary<string, bool>() {

        };
    }
}
