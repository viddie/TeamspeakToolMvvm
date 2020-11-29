using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {

    // !ps                                                                - Lists all saved playsounds
    // !ps <number>                                                       - View page {number} of the playsounds
    // !ps <sound>                                                        - Play a saved sound
    // !ps random                                                         - Play a random saved sound
    // !ps -<nc|demon|earrape> <sound>                                    - Play a saved sound with a modifier
    // !ps -mods                                                          - List all modifiers
    // !ps <sound> <speed|pitch|volume>=<value>...                        - Play a saved sound with manual modifiers
    // !ps <youtube-url> [startTime] [endTime]                            - Play a sound from youtube at for the given time interval
    // !ps -<nc|demon|earrape> <youtube-url> [startTime] [endTime]        - Play a sound from youtube at for the given time interval with a modifier


    // === Manage Parameters ===
    // These parameters don't have a syntax check yet
    // 
    // !ps add-file <name> <price> <filename>                             - Saves a playsound by local file
    // !ps add-yt <name> <price> <youtube-url> [startTime] [endTime]      - Saves a playsound by youtube video with time interval
    // !ps remove <name>                                                  - Removes a saved playsound by name

    // !ps add-modifier <name> <speed|pitch|volume>=<value>...            - Removes a saved playsound by name


    public class PlaysoundsCommand : ChatCommand {

        public static string ModifierNameVolume = "volume";
        public static string ModifierNameSpeed = "speed";
        public static string ModifierNamePitch = "pitch";

        public static Dictionary<string, Tuple<double, double>> ModifierRanges { get; set; } = new Dictionary<string, Tuple<double, double>>() {
            [ModifierNameVolume] = Tuple.Create(0.1, 10.0),
            [ModifierNameSpeed] = Tuple.Create(0.2, 3.0),
            [ModifierNamePitch] = Tuple.Create(0.1, 3.0),
        };


        public override string CommandPrefix { get; set; } = "playsounds";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "ps" };
        public override bool HasExceptionWhiteList { get; set; } = false;


        public bool HasRequestedPagination = false;
        public uint RequestedPage = uint.MaxValue;

        public string SelectedModifier;
        public string SoundSource;
        public bool SourceIsYoutube = false;
        public uint YoutubeStartTime = uint.MaxValue;
        public uint YoutubeEndTime = uint.MaxValue;
        public Dictionary<string, double> ManualModifiers;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) return true;
            if (parameters[0].StartsWith("-mods")) return true;  //Mods list
            if (parameters[0] == "add-file" || parameters[0] == "add-yt" || parameters[0] == "remove" || parameters[0] == "add-modifier") return true;

            if (parameters[0].StartsWith("-")) { //Remove mods from parameters
                SelectedModifier = parameters[0].Substring(1);

                if (!ModifierExists(SelectedModifier)) {
                    return false;
                }

                parameters = parameters.Skip(1).ToList();
            }

            if (parameters.Count == 1) { //Pagination
                if (uint.TryParse(parameters[0], out uint page)) {
                    HasRequestedPagination = true;
                    RequestedPage = page;
                    return true;
                }
            }


            if (parameters.Count >= 1) { //Extract sound source
                if (parameters[0].ToLower().StartsWith("[url]") && parameters[0].ToLower().EndsWith("[/url]")) {
                    SoundSource = parameters[0].Substring(5, parameters[0].Length - 11);
                    SourceIsYoutube = true;
                } else {
                    SoundSource = parameters[0];
                }

                parameters = parameters.Skip(1).ToList();
            } else { //No sound source was given
                return false;
            }


            if (parameters.Count >= 1 && SourceIsYoutube) { //Youtube: start and end time
                if (parameters.Count > 2) return false;
                if (parameters.Count >= 1) {
                    try {
                        YoutubeStartTime = GetYoutubeTimeInSeconds(parameters[0]);
                    } catch (Exception) {
                        throw new CommandParameterInvalidFormatException(SelectedModifier == null ? 2 : 3, parameters[0], "startTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                    }
                }
                if (parameters.Count == 2) {
                    try {
                        YoutubeEndTime = GetYoutubeTimeInSeconds(parameters[1]);
                    } catch (Exception) {
                        throw new CommandParameterInvalidFormatException(SelectedModifier == null ? 3 : 4, parameters[1], "endTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                    }
                }

            } else if(parameters.Count >= 1) {
                ManualModifiers = new Dictionary<string, double>();
                for (int i = 0; i < parameters.Count; i++) {
                    if (!parameters[i].Contains("=")) return false;
                    string[] split = parameters[i].Split(new char[] { '=' });
                    string modName = split[0];
                    string modValue = split[1].Replace(".", ",");

                    if (!ModifierRanges.ContainsKey(modName)) return false;
                    if (!double.TryParse(modValue, out double modValueParsed)) return false;

                    ManualModifiers.Add(modName, modValueParsed);
                }
            }


            return true;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return command;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Play a sound for some of your balance";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            if (parameters.Count > 0 &&
                (parameters[0] == "add-file" || parameters[0] == "add-yt" || parameters[0] == "remove" || parameters[0] == "add-modifier") &&
                !AccessManager.UserHasAccessToSubCommand(uniqueId, "command:playsounds_manage")) {
                return false;
            }
            return true;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string mod = SelectedModifier ?? "<none>";
            string modCost = SelectedModifier == null ? "1.0" : GetPriceFactorForModifiers(Settings.PlaysoundsModifiers[SelectedModifier]).ToString();
            string source = SoundSource ?? "<none>";
            string manualModifiers = ManualModifiers == null ? "<none>" : string.Join(", ", ManualModifiers.Keys.ToList());

            messageCallback.Invoke($"Playsound: (HasRequestedPagination: {HasRequestedPagination}, RequestedPage: {RequestedPage}, Modifier: {mod}, ModifierPriceFactor: {modCost}, SoundSource: {source}, SourceIsYoutube: {SourceIsYoutube}, YoutubeStartSecs: {YoutubeStartTime}, YoutubeEndSecs: {YoutubeEndTime}, ManualModifiers: {manualModifiers}, ManualModifierPriceFactor: {GetPriceFactorForModifiers(ManualModifiers)})");

            if (parameters[0] == "add-yt") {
                string devices = string.Join("\n\t", AudioHelper.GetAudioDevices());
                messageCallback.Invoke($"All audio devices:\n\t{devices}");
            }
        }


        #region Modifier Methods
        public static bool ModifierExists(string name) {
            return MySettings.Instance.PlaysoundsModifiers.ContainsKey($"{name}");
        }

        public static double GetPriceFactorForModifiers(Dictionary<string, double> modifiers) {
            if (modifiers == null) return 1.0;

            double factor = 1.0;

            if (modifiers.ContainsKey(ModifierNameSpeed)) {
                factor /= Math.Pow(modifiers[ModifierNameSpeed], 0.7);
            }

            if (modifiers.ContainsKey(ModifierNameVolume)) {
                factor *= Math.Pow(modifiers[ModifierNameVolume], 0.3);
            }

            return factor;
        }

        public static uint GetYoutubeTimeInSeconds(string ytTime) {
            if (ytTime.Contains(":")) {
                string[] split = ytTime.Split(new char[] { ':' });
                string minutePart = split[0];
                string secondsPart = split[1];

                return uint.Parse(minutePart) * 60 + uint.Parse(secondsPart);
            } else {
                return uint.Parse(ytTime);
            }
        }
        #endregion
    }
}
