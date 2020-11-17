using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.ViewModels;
using TSClient.Enums;
using TSClient.Events;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Groups;
using TSClient.Exceptions;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class ChatCommandHandler {

        public MainViewModel Parent { get; set; }
        public MySettings Settings { get; set; }

        public List<ChatCommand> ChatCommands { get; set; } = new List<ChatCommand>() {
            new TimeCommand(),
            new YouTubeCommand(),
            new CoinFlipCommand(),
            new GroupsCommand(),
        };


        public ChatCommandHandler(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }

        public void HandleTextMessage(NotifyTextMessageEvent evt) {
            if (!Settings.ChatCommandsEnabled || string.IsNullOrEmpty(evt.Message) || !evt.Message.StartsWith(Settings.ChatCommandPrefix)) return;


            NotifyTextMessageEvent tempEvt = new NotifyTextMessageEvent();
            evt.CopyProperties(tempEvt);
            evt = tempEvt;

            evt.Message = evt.Message.Substring(Settings.ChatCommandPrefix.Length);


            Action<string> sendMessageCallback = null;
            if (evt.TargetMode == MessageMode.Private) {
                sendMessageCallback = (s) => Parent.Client.SendPrivateMessage(s, evt.InvokerId);
            } else if (evt.TargetMode == MessageMode.Channel) {
                sendMessageCallback = Parent.Client.SendChannelMessage;
            } else if (evt.TargetMode == MessageMode.Server) {
                sendMessageCallback = Parent.Client.SendServerMessage;
            }


            bool hasAdmin = Settings.AdminUniqueIds.Contains(evt.InvokerUniqueId);
            if (Parent.RateLimiter.CheckRateLimit("chat_command", evt.InvokerUniqueId, hasAdmin) == false) {
                sendMessageCallback.Invoke(ColorCoder.Error("Slow down a little, you are sending too many commands!"));
                return;
            }

            string[] messageSplit = evt.Message.Split(new char[] { ' ' });
            string command = messageSplit[0].ToLower();
            List<string> parameters = messageSplit.Skip(1).ToList();

            ChatCommand commandToExecute = null;
            try {
                commandToExecute = GetCommandForMessage(command, parameters, evt.InvokerUniqueId);
            } catch (ChatCommandNotFoundException) {
                sendMessageCallback.Invoke(ColorCoder.Error("Command was not found"));
                return;
            } catch (ChatCommandInvalidSyntaxException ex) {
                sendMessageCallback.Invoke(ColorCoder.Error($"Invalid syntax. Usage of command:\n{Settings.ChatCommandPrefix}{ex.Message}"));
                return;
            } catch (NoPermissionException) {
                sendMessageCallback.Invoke(ColorCoder.Error($"You don't have access to this command, {ColorCoder.Username(evt.InvokerName)}"));
                return;
            }

            try {
                commandToExecute.HandleCommand(evt, command, parameters, sendMessageCallback);

            } catch (MultipleTargetsFoundException ex) {
                string joined = string.Join(", ", ex.AllFoundTargets.Select(client => ColorCoder.Bold($"'{client.Nickname}'")));
                sendMessageCallback.Invoke(ColorCoder.Error($"Too many targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name ({joined})"));

            } catch (NoTargetsFoundException ex) {
                sendMessageCallback.Invoke(ColorCoder.Error($"No targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name..."));
            }
        }

        public ChatCommand GetCommandForMessage(string command, List<string> parameters, string uniqueId) { //command param1 param2 param3
            foreach (ChatCommand cmd in ChatCommands) {
                if (cmd.CommandPrefix == command || cmd.CommandAliases.Contains(command)) {
                    if (cmd.IsValidCommandSyntax(command, parameters)) {
                        if (!AccessManager.UserHasAccessToCommand(uniqueId, cmd.GetType())) {
                            throw new NoPermissionException();
                        }

                        ChatCommand chatCommand = (ChatCommand)Activator.CreateInstance(cmd.GetType());
                        chatCommand.Parent = Parent;
                        chatCommand.Settings = Settings;
                        return chatCommand;
                    } else {
                        throw new ChatCommandInvalidSyntaxException(cmd.GetUsageHelp(command, parameters));
                    }
                }
            }

            throw new ChatCommandNotFoundException();
        }
    }
}
