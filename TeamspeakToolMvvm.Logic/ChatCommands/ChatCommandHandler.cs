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

        public static List<ChatCommand> ChatCommands { get; set; } = new List<ChatCommand>() {
            new TimeCommand(),
            new YouTubeCommand(),
            new CoinFlipCommand(),
            new GroupsCommand(),
            new RollCommand(),
            new SayCommand(),
            new TeamsCommand(),
            new DynamicCommand(),
            new BalanceCommand(),
            new HelpCommand(),
            new StopCommand(),
            new RouletteCommand(),
            new DailyCommand(),
        };


        public ChatCommandHandler(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }

        public void HandleTextMessage(NotifyTextMessageEvent evt) {
            if (!Settings.ChatCommandsEnabled || string.IsNullOrEmpty(evt.Message) || !evt.Message.StartsWith(Settings.ChatCommandPrefix)) return;

            Parent.LogMessage($"{evt.InvokerName} requested command: \"{evt.Message}\"");

            //Copy event as to not mess up other event handlers
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
            } catch (CommandParameterInvalidFormatException ex) {
                sendMessageCallback.Invoke(ColorCoder.Error($"The {ex.GetParameterPosition()} parameter's format was invalid ({ex.ParameterName} = '{ex.ParameterValue}'). It has to be {ColorCoder.Bold(ex.GetNeededType())}!\nUsage: {Settings.ChatCommandPrefix}{ex.UsageHelp}"));
                return;
            }

            try {
                commandToExecute.HandleCommand(evt, command, parameters, sendMessageCallback);

            } catch (MultipleTargetsFoundException ex) {
                string joined = string.Join(", ", ex.AllFoundTargets.Select(client => ColorCoder.Bold($"'{client.Nickname}'")));
                sendMessageCallback.Invoke(ColorCoder.Error($"Too many targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name ({joined})"));

            } catch (CooldownNotExpiredException ex) {
                sendMessageCallback.Invoke(ColorCoder.Error($"That command is still on cooldown. ({ColorCoder.Bold($"{CooldownManager.FormatCooldownTime(ex.Duration)}")} cooldown)"));

            } catch (NoTargetsFoundException ex) {
                sendMessageCallback.Invoke(ColorCoder.Error($"No targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name..."));

            } catch (Exception ex) {
                Parent.LogMessage($"Encountered exception in command '{commandToExecute.GetType().Name}': {ex}");
            }
        }

        public ChatCommand GetCommandForMessage(string command, List<string> parameters, string uniqueId) { //command param1 param2 param3
            foreach (ChatCommand cmd in ChatCommands) {
                if (cmd.CommandPrefix == command || cmd.CommandAliases.Contains(command)) {
                    ChatCommand chatCommand = (ChatCommand)Activator.CreateInstance(cmd.GetType());
                    chatCommand.Parent = Parent;
                    chatCommand.Settings = Settings;

                    try {
                        if (!chatCommand.CanExecute(uniqueId, command, parameters)) {
                            throw new NoPermissionException();
                        }
                        if (!chatCommand.IsValidCommandSyntax(command, parameters)) {
                            throw new ChatCommandInvalidSyntaxException(chatCommand.GetUsageSyntax(command, parameters));
                        }

                        return chatCommand;

                    } catch (CommandParameterInvalidFormatException ex) {
                        ex.UsageHelp = chatCommand.GetUsageSyntax(command, parameters);
                        throw ex;
                    }
                }
            }

            throw new ChatCommandNotFoundException();
        }
    }
}
