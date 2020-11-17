using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.ViewModels;

namespace TeamspeakToolMvvm.Logic.Groups {
    public class AccessManager {

        public static AccessManager Instance;
        public static Group DefaultGroup = new UserGroup();

        public static Group UserGroup = DefaultGroup;
        public static Group ModeratorGroup = new ModeratorGroup() { InheritGroup = UserGroup };
        public static Group AdminGroup = new AdminGroup();
        public static Group BannedGroup = new BannedGroup();

        public static List<Group> AllGroups { get; set; } = new List<Group>() {
            AdminGroup,
            ModeratorGroup,
            UserGroup,
            BannedGroup,
        };

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }


        public AccessManager(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }

        public static bool UserHasAccessToCommand(string uniqueId, Type chatCommand) {
            CheckUserHasGroups(uniqueId);

            foreach (Group group in GetUserGroups(uniqueId)) {
                if (group.AccessAll || group.CommandAccesses.ContainsKey(chatCommand)) {
                    return true;
                }
                if (group.InheritGroup != null && group.InheritGroup.CommandAccesses.ContainsKey(chatCommand)) {
                    return true;
                }
            }

            return false;
        }
        public static bool UserHasAccessToSubCommand(string uniqueId, string subCommand) {
            CheckUserHasGroups(uniqueId);

            foreach (Group group in GetUserGroups(uniqueId)) {
                if (group.AccessAll || group.SubCommandAccesses.ContainsKey(subCommand)) {
                    return true;
                }
                if (group.InheritGroup != null && group.InheritGroup.SubCommandAccesses.ContainsKey(subCommand)) {
                    return true;
                }
            }

            return false;
        }

        public static void CheckUserHasGroups(string uniqueId) {
            if (Instance.Settings.UserGroups.ContainsKey(uniqueId)) return;
            Instance.Settings.UserGroups.Add(uniqueId, new List<string>() { DefaultGroup.Name });
            Instance.Settings.DelayedSave();
        }

        public static bool AddUserGroupByName(string uniqueId, string groupName) {
            Group group = GetGroupByName(groupName);
            if (group == null) {
                throw new GroupNotFoundException($"Group '{groupName}' was not found");
            }

            return AddUserGroup(uniqueId, group);
        }
        public static bool AddUserGroup(string uniqueId, Group group) {
            if (UserHasGroup(uniqueId, group)) {
                return false;
            }

            Instance.Settings.UserGroups[uniqueId].Add(group.Name);

            if (UserHasGroup(uniqueId, DefaultGroup)){
                RemoveUserGroup(uniqueId, DefaultGroup);
            }

            Instance.Settings.DelayedSave();
            return true;
        }

        public static bool RemoveUserGroupByName(string uniqueId, string groupName) {
            Group group = GetGroupByName(groupName);
            if (group == null) {
                throw new GroupNotFoundException($"Group '{groupName}' was not found");
            }

            return RemoveUserGroup(uniqueId, group);
        }
        public static bool RemoveUserGroup(string uniqueId, Group group) {
            if (!UserHasGroup(uniqueId, group)) {
                return false;
            }

            Instance.Settings.UserGroups[uniqueId].Remove(group.Name);

            if (Instance.Settings.UserGroups[uniqueId].Count == 0) {
                Instance.Settings.UserGroups[uniqueId].Add(DefaultGroup.Name);
            }

            Instance.Settings.DelayedSave();
            return true;
        }

        public static List<Group> GetUserGroups(string uniqueId) {
            CheckUserHasGroups(uniqueId);
            List<string> groups = Instance.Settings.UserGroups[uniqueId];
            List<Group> convertedGroups = new List<Group>();

            foreach (Group group in AllGroups) {
                if (groups.Contains(group.Name)) {
                    convertedGroups.Add(group);
                }
            }

            return convertedGroups;
        }

        public static Group GetGroupByName(string groupName) {
            groupName = groupName.ToLower();

            foreach (Group group in AllGroups) {
                if (group.Name == groupName) {
                    return group;
                }
            }
            return null;
        }

        public static bool UserHasGroup(string uniqueId, Group checkGroup) {
            CheckUserHasGroups(uniqueId);

            foreach (Group group in GetUserGroups(uniqueId)) {
                if (group == checkGroup) {
                    return true;
                }
            }
            return false;
        }
    }
}
