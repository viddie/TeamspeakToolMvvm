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
            new TickCommand(),
            new ScrapeCommand(),
            new ReplayCommand(),
            new PlaysoundsCommand(),
            new AoeCommand(),
            new ResetCooldown(),
        };


        public ChatCommandHandler(MainViewModel parent) {
            Parent = parent;
            Settings = Parent.Settings;
        }

        public bool HandleTextMessage(NotifyTextMessageEvent evt, Action<string> messageCallback) {
            if (!Settings.ChatCommandsEnabled || string.IsNullOrEmpty(evt.Message) || !evt.Message.StartsWith(Settings.ChatCommandPrefix)) return false;

            Parent.LogMessage($"{evt.InvokerName} requested command: \"{evt.Message}\"");

            //Copy event as to not mess up other event handlers
            NotifyTextMessageEvent tempEvt = new NotifyTextMessageEvent();
            evt.CopyProperties(tempEvt);
            evt = tempEvt;

            evt.Message = evt.Message.Substring(Settings.ChatCommandPrefix.Length);

            bool hasAdmin = Settings.AdminUniqueIds.Contains(evt.InvokerUniqueId);
            if (Parent.RateLimiter.CheckRateLimit("chat_command", evt.InvokerUniqueId, hasAdmin) == false) {
                messageCallback.Invoke(ColorCoder.ErrorBright("Slow down a little, you are sending too many commands!"));
                return true;
            }

            string[] messageSplit = evt.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string command = messageSplit[0].ToLower();
            List<string> parameters = messageSplit.Skip(1).ToList();
            parameters = ParseParameterEscapes(parameters);

            ChatCommand commandToExecute = null;
            try {
                commandToExecute = GetCommandForMessage(command, parameters, evt.InvokerUniqueId);
            } catch (ChatCommandNotFoundException) {
                messageCallback.Invoke(ColorCoder.ErrorBright("Command was not found"));
                return true;
            } catch (ChatCommandInvalidSyntaxException ex) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"Invalid syntax. Usage of command:\n{Settings.ChatCommandPrefix}{ex.Message}"));
                return true;
            } catch (NoPermissionException) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have access to this command, {ColorCoder.Username(evt.InvokerName)}"));
                return true;
            } catch (CommandParameterInvalidFormatException ex) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"The {ex.GetParameterPosition()} parameter's format was invalid ({ex.ParameterName} = '{ex.ParameterValue}'). It has to be {ColorCoder.Bold(ex.GetNeededType())}!\nUsage: {Settings.ChatCommandPrefix}{ex.UsageHelp}"));
                return true;
            }

            try {
                commandToExecute.HandleCommand(evt, command, parameters, messageCallback);

            } catch (MultipleTargetsFoundException ex) {
                string joined = string.Join(", ", ex.AllFoundTargets.Select(client => ColorCoder.Bold($"'{client.Nickname}'")));
                messageCallback.Invoke(ColorCoder.ErrorBright($"Too many targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name ({joined})"));

            } catch (CooldownNotExpiredException ex) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"That command is still on cooldown. ({ColorCoder.Bold($"{CooldownManager.FormatCooldownTime(ex.Duration)}")} cooldown)"));

            } catch (NoTargetsFoundException ex) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"No targets were found with {ColorCoder.Bold($"'{ex.Message}'")} in their name..."));

            } catch (Exception ex) {
                Parent.LogMessage($"Encountered exception in command '{commandToExecute.GetType().Name}': {ex}");
            }

            return true;
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

        public static List<string> ParseParameterEscapes(List<string> parameters) {
            List<string> toRet = new List<string>();

            bool isEscaping = false;
            string joinedParameter = "";
            foreach (string currentParameter in parameters) {
                if (currentParameter.StartsWith("\"")) {
                    isEscaping = true;
                    joinedParameter = "";
                }

                if (isEscaping) {
                    if (joinedParameter == "")
                        joinedParameter = currentParameter.Substring(1);
                    else
                        joinedParameter = string.Join(" ", joinedParameter, currentParameter);

                    if (currentParameter.EndsWith("\"")) {
                        isEscaping = false;
                        joinedParameter = joinedParameter.Substring(0, joinedParameter.Length - 1);
                        toRet.Add(joinedParameter);
                    }
                } else {
                    toRet.Add(currentParameter);
                }
            }

            if (isEscaping) {
                throw new Exception("Still escaping!");
            }

            return toRet;
        }
        /*
         Python code to escape the character (") in parameters:

            i = 0

            while True:
                if(i >= len(parameters)):
                    break
                param = parameters[i]

                if(param.startswith("\"")):
                    startAt = i
                    while not parameters[i].endswith("\""):
                        i += 1
                        if(i == len(parameters)):
                            self.sendTextMessage(schid, targetMode, "[color=#bb0000]Syntax error: Found opening \" but no closing \", [b]@'{}'[/b][/color]".format(fromName), targetID)
                            return
                    joinedParam = " ".join(parameters[startAt:i+1])
                    joinedParam = joinedParam[1:-1]

                    for index in range(startAt, i+1):
                        parameters.pop(startAt)

                    parameters.insert(startAt, joinedParam)

                i += 1
         */
    }
}
