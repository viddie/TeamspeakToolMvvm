using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;
using TSClient.Models;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class TimedCommand : ChatCommand {

        public static Dictionary<string, Tuple<Timer, DateTime, string, List<string>>> TimerInfos = new Dictionary<string, Tuple<Timer, DateTime, string, List<string>>>();


        public override string CommandPrefix { get; set; } = "timed";
        public override List<string> CommandAliases { get; set; } = new List<string>() { };
        public override bool HasExceptionWhiteList { get; set; } = true;

        private string Action;

        private TimeSpan TimeDelta;
        private string TargetCommand;
        private List<string> TargetParameters;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) parameters.Add("status");

            if (parameters[0].ToLower() == "status" || parameters[0].ToLower() == "cancel") {
                Action = parameters[0].ToLower();
                return true;
            }

            if (parameters.Count < 2) return false;
            TimeDelta = Utils.TimeStringToTimeSpan(parameters[0]);
            TargetCommand = parameters[1];
            TargetParameters = new List<string>();

            if (parameters.Count > 2) {
                TargetParameters = parameters.Skip(2).ToList();
            }

            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return $"{command} <status|cancel|timedelta> [<command>] [[parameters]]";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Delays a command invocation";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            if (Action == "status") {
                if (TimerInfos.ContainsKey(evt.InvokerUniqueId)) {
                    (Timer targetTimer, DateTime timeDue, string targetCommand, List<string> targetParameters) = TimerInfos[evt.InvokerUniqueId];

                    TimeSpan timeLeft = timeDue - DateTime.Now;
                    timeLeft = TimeSpan.FromSeconds((int)timeLeft.TotalSeconds);

                    messageCallback.Invoke($"{ColorCoder.Username(evt.InvokerName)}: Time left for {FormatCommandString(command, parameters)}: [{timeLeft}]");

                } else {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have an active timed command, {ColorCoder.Username(evt.InvokerName)}"));
                }
                return;


            } else if (Action == "cancel") {
                if (TimerInfos.ContainsKey(evt.InvokerUniqueId)) {
                    (Timer targetTimer, DateTime timeDue, string targetCommand, List<string> targetParameters) = TimerInfos[evt.InvokerUniqueId];

                    targetTimer.Dispose();
                    TimerInfos.Remove(evt.InvokerUniqueId);

                    messageCallback.Invoke($"{ColorCoder.Username(evt.InvokerName)}: Your timer for {FormatCommandString(command, parameters)} was cancelled!");

                } else {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"You don't have an active timed command, {ColorCoder.Username(evt.InvokerName)}"));
                }
                return;
            }




            if (TimerInfos.ContainsKey(evt.InvokerUniqueId)) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"You already have an active timed command, {ColorCoder.Username(evt.InvokerName)}"));
                return;
            }

            TimeDelta = TimeDelta.Duration();
            ChatCommand commandToExecute;
            
            try {
                commandToExecute = Parent.CommandHandler.GetCommandForMessage(TargetCommand, TargetParameters.AsEnumerable().ToList(), evt.InvokerUniqueId);
            } catch (Exception) {
                messageCallback.Invoke(ColorCoder.ErrorBright($"Could not parse the desired command. Make sure you have access to the command and the syntax is valid!"));
                return;
            }

            Timer timer = new Timer(new TimerCallback((state) => {
                HandleDelayedCommand(evt.InvokerUniqueId, TargetCommand, TargetParameters, messageCallback);
            }), null, TimeDelta, TimeSpan.FromMilliseconds(-1));

            TimerInfos.Add(evt.InvokerUniqueId, Tuple.Create(timer, DateTime.Now+TimeDelta, TargetCommand, TargetParameters));

            messageCallback.Invoke(ColorCoder.SuccessDim($"Invoking command {FormatCommandString(TargetCommand, TargetParameters)} in [{ColorCoder.Bold(TimeDelta)}], {ColorCoder.Username(evt.InvokerName)}"));
        }

        public void HandleDelayedCommand(string clientUniqueId, string command, List<string> parameters, Action<string> messageCallback) {
            TimerInfos.Remove(clientUniqueId);

            Client invoker = Parent.Client.GetClientByUniqueId(clientUniqueId);
            if (invoker == null) {
                string lastSeenName = Parent.Settings.LastSeenUsernames[clientUniqueId];
                messageCallback.Invoke(ColorCoder.ErrorBright($"Delayed command for {FormatCommandString(command, parameters)} was not executed as invoker {ColorCoder.Username(lastSeenName)} is no longer online"));
                return;
            }



            string commandMessage = FormatCommandStringRaw(command, parameters);
            NotifyTextMessageEvent evt = new NotifyTextMessageEvent() {
                InvokerId = invoker.Id,
                InvokerName = invoker.Nickname,
                InvokerUniqueId = invoker.UniqueId,
                Message = commandMessage,
            };

            messageCallback.Invoke($"Invoking command {FormatCommandString(command, parameters)} for {ColorCoder.Username(invoker.Nickname)}");
            Parent.CommandHandler.HandleTextMessage(evt, messageCallback);
        }



        public static string FormatCommandStringRaw(string cmd, List<string> parameters) {
            string targetParametersJoined = string.Join(" ", parameters.Select((s) => s.Contains(" ") ? $"\"{s}\"" : s));
            return $"{MySettings.Instance.ChatCommandPrefix}{cmd} {targetParametersJoined}";
        }
        public static string FormatCommandString(string cmd, List<string> parameters) {
            return ColorCoder.Bold($"'{FormatCommandStringRaw(cmd, parameters)}'");
        }
    }
}
