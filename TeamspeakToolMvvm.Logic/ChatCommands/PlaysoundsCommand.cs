using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using TeamspeakToolMvvm.Logic.Economy;
using TeamspeakToolMvvm.Logic.Exceptions;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TeamspeakToolMvvm.Logic.Models;
using TSClient.Events;
using TSClient.Models;

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
    // !ps add-file <name> <price> <filename> [source]                    - Saves a playsound by local file
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

        public static string SoundsFolder = "playsounds";
        public static List<string> AdminCommands = new List<string>() {
            "add-file", "add-yt", "remove", "add-modifier", "list-devices", "stop-sounds"
        };

        public static List<Process> AllStartedSoundProcesses = new List<Process>();
        public static double LoadingPercentDone { get; set; } = -1;

        public static object YoutubeDownloadLock { get; set; } = new object();
        public static bool IsLoadingYoutubeAudio { get; set; } = false;



        public override string CommandPrefix { get; set; } = "playsounds";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "ps" };
        public override bool HasExceptionWhiteList { get; set; } = false;

        public bool IsAdministrativeCommand = false;
        public string AdminAction;
        public string AdminPlaysoundName;
        public uint AdminPlaysoundPrice;
        public string AdminPlaysoundFileName;
        public bool AdminPlaysoundFileNameIsYoutube = false;
        public string AdminPlaysoundSource;

        public bool HasRequestedPagination = false;
        public uint RequestedPage = uint.MaxValue;

        public string SelectedModifier;
        public string SoundSource;
        public bool SourceIsYoutube = false;
        public uint YoutubeStartTime = 0;
        public uint YoutubeEndTime = (uint)int.MaxValue - 1;
        public Dictionary<string, double> ManualModifiers;

        public uint PriceLookupDuration;

        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            if (parameters.Count == 0) {
                HasRequestedPagination = true;
                RequestedPage = 1;
                return true;
            }
            if (parameters[0].StartsWith("-mods")) return true;  //Mods list

            if (AdminCommands.Contains(parameters[0])) {
                AdminAction = parameters[0];
                parameters = parameters.Skip(1).ToList();

                if (parameters.Count == 0) {
                    if (AdminAction == "stop-sounds" || AdminAction == "list-devices") {
                        IsAdministrativeCommand = true;
                        return true;
                    }
                    return false;
                }

                IsAdministrativeCommand = true;
                AdminPlaysoundName = parameters[0];
                parameters = parameters.Skip(1).ToList();



                if (AdminAction == "add-file" || AdminAction == "add-yt") {
                    if (!uint.TryParse(parameters[0], out AdminPlaysoundPrice)) {
                        throw new CommandParameterInvalidFormatException(3, parameters[0], "price", typeof(uint), GetUsageSyntax(command, new List<string>()));
                    }
                    parameters = parameters.Skip(1).ToList();


                    AdminPlaysoundFileName = parameters[0];
                    parameters = parameters.Skip(1).ToList();

                    if (AdminAction == "add-yt") {
                        AdminPlaysoundFileNameIsYoutube = true;
                        AdminPlaysoundSource = AdminPlaysoundFileName;
                        if (parameters.Count >= 1) {
                            try {
                                YoutubeStartTime = GetYoutubeTimeInSeconds(parameters[0]);
                            } catch (Exception) {
                                throw new CommandParameterInvalidFormatException(5, parameters[0], "startTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                            }
                        }
                        if (parameters.Count == 2) {
                            try {
                                YoutubeEndTime = GetYoutubeTimeInSeconds(parameters[1]);
                            } catch (Exception) {
                                throw new CommandParameterInvalidFormatException(6, parameters[1], "endTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                            }
                        }
                    } else {
                        if (parameters.Count > 0) {
                            AdminPlaysoundSource = parameters[0];
                        }
                    }

                } else if (AdminAction == "remove" && parameters.Count != 0) {
                    return false;

                } else if (AdminAction == "add-modifier") {
                    if (parameters.Count == 0) return false;

                    ManualModifiers = ParseModifierParameters(parameters);
                }

                return true;
            }


            if (parameters.Count == 2 && parameters[0] == "price" && uint.TryParse(parameters[1], out PriceLookupDuration)) return true;


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
                if (parameters.Count > 4) return false;
                if (parameters.Count >= 1) {
                    try {
                        YoutubeStartTime = GetYoutubeTimeInSeconds(parameters[0]);
                        parameters = parameters.Skip(1).ToList();
                    } catch (Exception) {
                        //throw new CommandParameterInvalidFormatException(SelectedModifier == null ? 2 : 3, parameters[0], "startTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                    }
                }
                if (parameters.Count >= 1) {
                    try {
                        YoutubeEndTime = GetYoutubeTimeInSeconds(parameters[0]);
                        parameters = parameters.Skip(1).ToList();
                    } catch (Exception) {
                        //throw new CommandParameterInvalidFormatException(SelectedModifier == null ? 3 : 4, parameters[1], "endTime", typeof(uint), GetUsageSyntax(command, new List<string>()));
                    }
                }
            }

            if (parameters.Count >= 1) {
                ManualModifiers = ParseModifierParameters(parameters);
            }


            return true;
        }

        public static Dictionary<string, double> ParseModifierParameters(List<string> parameters) {
            Dictionary<string, double> toRet = new Dictionary<string, double>();
            for (int i = 0; i < parameters.Count; i++) {
                if (!parameters[i].Contains("=")) return toRet;
                string[] split = parameters[i].Split(new char[] { '=' });
                string modName = split[0];
                string modValue = split[1].Replace(".", ",");

                if (!ModifierRanges.ContainsKey(modName)) return toRet;
                if (!double.TryParse(modValue, out double modValueParsed)) return toRet;

                toRet.Add(modName, modValueParsed);
            }

            return toRet;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            if (parameters.Count > 0) {
                if (parameters[0] == "add-file") {
                    return $"{command} add-file <name> <price> <filename>";
                } else if (parameters[0] == "add-yt") {
                    return $"{command} add-yt <name> <price> <youtube-url> [startTime] [endTime]";
                } else if (parameters[0] == "remove") {
                    return $"{command} remove <name>";
                } else if (parameters[0] == "add-modifier") {
                    return $"{command} add-modifier <name> <speed|pitch|volume>=<value>...";
                }
            }
            return command;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "Play a sound for some of your balance";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            if (parameters.Count > 0 &&
                (AdminCommands.Contains(parameters[0])) &&
                !AccessManager.UserHasAccessToSubCommand(uniqueId, "command:playsounds_manage")) {
                return false;
            }
            return true;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            // +----------------------+
            // | Administrative Block |
            // +----------------------+
            if (IsAdministrativeCommand) {
                if (AdminAction == "add-file") {
                    AddPlaysound(AdminPlaysoundName, AdminPlaysoundPrice, AdminPlaysoundFileName, AdminPlaysoundSource, messageCallback);

                } else if (AdminAction == "add-yt") {

                } else if (AdminAction == "remove") {
                    RemovePlaysound(AdminPlaysoundName, messageCallback);

                } else if (AdminAction == "add-modifier") {

                } else if (AdminAction == "list-devices") {
                    string devices = string.Join("\n\t", AudioHelper.GetAudioDevices());
                    messageCallback.Invoke($"All audio devices:\n\t{devices}");

                } else if (AdminAction == "stop-sounds") {
                    int killed = 0;
                    foreach (Process p in AllStartedSoundProcesses) {
                        if (!p.HasExited) {
                            p.Kill();
                            killed++;
                        }
                    }
                    AllStartedSoundProcesses.Clear();

                    messageCallback.Invoke(ColorCoder.ErrorBright($"Killed {ColorCoder.Bold($"'{killed}'")} audio process(es)"));
                }
                return;
            }


            if (parameters.Count == 2 && parameters[0] == "price") {
                int price = CalculateYoutubePlaysoundCost(PriceLookupDuration);
                messageCallback.Invoke($"Cost for {ColorCoder.Bold($"'{PriceLookupDuration}'")} seconds of a youtube video: {ColorCoder.Currency(price)}");
                return;
            }



            // +----------------------+
            // |      Pagination      |
            // +----------------------+
                if (HasRequestedPagination) {
                int entriesPerPage = Settings.PlaysoundsSoundsPerPage;
                int maxPage = (int) Math.Ceiling((double) Settings.PlaysoundsSavedSounds.Count / entriesPerPage);

                if (RequestedPage < 1 || RequestedPage > maxPage) {
                    messageCallback.Invoke($"The page '{RequestedPage}' is not in the valid range of [1 to {maxPage}]!");
                    return;
                }

                int pageIndex = (int)RequestedPage - 1;
                int skipValues = pageIndex * entriesPerPage;
                List<Playsound> thisPage = Settings.PlaysoundsSavedSounds.OrderBy(ps => ps.BasePrice).ThenBy(ps => ps.Name).Skip(skipValues).Take(entriesPerPage).ToList();

                string toPrint = "";
                foreach (Playsound listSound in thisPage) {
                    toPrint += $"\n'{listSound.Name}'\t({listSound.BasePrice} {Settings.EcoPointUnitName})";
                }
                toPrint += $"\n\t\t\t{ColorCoder.Bold($"-\tPage ({RequestedPage}/{maxPage})\t-")}";
                toPrint += $"\n\nTo switch pages write '{Settings.ChatCommandPrefix}{command} <1/2/3/...>'";

                messageCallback.Invoke(toPrint);

                return;
            }


            if (evt.TargetMode != TSClient.Enums.MessageMode.Channel) {
                messageCallback.Invoke(ColorCoder.Error("Playsounds can only be used in channel chats!"));
                return;
            }

            Client myClient = Parent.Client.GetClientById(Parent.MyClientId);
            if (Parent.Client.IsClientOutputMuted(myClient)) {
                messageCallback.Invoke(ColorCoder.Error("This doesn't work right now..."));
                return;
            }
            if (Parent.Client.IsClientInputMuted(myClient)) {
                messageCallback.Invoke(ColorCoder.Error("Host is muted, playsound can still be played but others won't hear it (@Panther)"));
            }

            CooldownManager.ThrowIfCooldown(evt.InvokerUniqueId, "command:playsounds");


            // +-------------------------+
            // | Playback of local files |
            // +-------------------------+



            if (!CheckValidModifiers(ManualModifiers, messageCallback)) {
                return;
            }

            double speed = 1.0, pitch = 1.0, volume = 1.0;

            if (SelectedModifier != null) {
                Dictionary<string, double> modifiers = Settings.PlaysoundsModifiers[SelectedModifier];
                if (modifiers.ContainsKey(ModifierNameSpeed)) {
                    speed = modifiers[ModifierNameSpeed];
                }
                if (modifiers.ContainsKey(ModifierNamePitch)) {
                    pitch = modifiers[ModifierNamePitch];
                }
                if (modifiers.ContainsKey(ModifierNameVolume)) {
                    volume = modifiers[ModifierNameVolume];
                }
            }

            if (ManualModifiers != null) {
                if (ManualModifiers.ContainsKey(ModifierNameSpeed)) {
                    speed = ManualModifiers[ModifierNameSpeed];
                }
                if (ManualModifiers.ContainsKey(ModifierNamePitch)) {
                    pitch = ManualModifiers[ModifierNamePitch];
                }
                if (ManualModifiers.ContainsKey(ModifierNameVolume)) {
                    volume = ManualModifiers[ModifierNameVolume];
                }
            }

            Dictionary<string, double> actualModifiers = new Dictionary<string, double>() {
                [ModifierNameSpeed] = speed,
                [ModifierNamePitch] = pitch,
                [ModifierNameVolume] = volume,
            };

            double priceFactor = GetPriceFactorForModifiers(actualModifiers);


            int basePrice;
            string filePath;
            double baseDuration;
            string playingSoundStr;

            if (SourceIsYoutube) {
                // +----------------------+
                // |   Youtube fetching   |
                // +----------------------+
                if (SoundSource.ToLower().StartsWith("[url]")) {
                    SoundSource = Utils.RemoveTag(SoundSource, "url");
                }
                string title = "";
                string youtubeUrl = AudioHelper.IsYouTubeVideoUrl(SoundSource);
                if (youtubeUrl != null) {
                    lock (YoutubeDownloadLock) {
                        if (IsLoadingYoutubeAudio) {
                            string loadingStr = LoadingPercentDone == -1 ? "download not yet started" : $"{LoadingPercentDone:0.##}%";
                            messageCallback.Invoke(ColorCoder.ErrorBright($"Chill, I'm still loading another clip... ({loadingStr})"));
                            return;
                        }
                        IsLoadingYoutubeAudio = true;
                    }

                    string videoId = youtubeUrl.Substring(youtubeUrl.IndexOf($"v=") + 2, 11);
                    try {
                        (filePath, title) = AudioHelper.LoadYoutubeVideo(videoId, (int)YoutubeStartTime, (int)(YoutubeEndTime - YoutubeStartTime) + 1, new ProgressSaver());
                        Parent.UpdateYoutubeFolderSize();
                    } catch (ArgumentException ex) {
                        messageCallback.Invoke(ColorCoder.ErrorBright($"Error with youtube video: {ex.Message}"));
                        lock (YoutubeDownloadLock) {
                            IsLoadingYoutubeAudio = false;
                            LoadingPercentDone = -1;
                        }
                        return;
                    }

                    lock (YoutubeDownloadLock) {
                        IsLoadingYoutubeAudio = false;
                        LoadingPercentDone = -1;
                    }
                } else {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"The URL is not a youtube link..."));
                    return;
                }

                baseDuration = AudioHelper.GetAudioDurationInSeconds(filePath);
                if (double.IsNaN(baseDuration)) {
                    messageCallback.Invoke(ColorCoder.ErrorBright($"Couldn't get audio duration from file '{filePath}'"));
                    return;
                }

                basePrice = CalculateYoutubePlaysoundCost(baseDuration);
                if (baseDuration <= 60 && Settings.PlaysoundsYoutubeOnePointEvent) {
                    basePrice = 1;
                }

                playingSoundStr = ColorCoder.SuccessDim($" Playing YouTube clip {ColorCoder.Bold($"'{title}'")} [{TimeSpan.FromSeconds((int)baseDuration)}].");

            } else {
                Playsound sound;
                if (SoundSource == "random") {
                    Random r = new Random();
                    sound = Settings.PlaysoundsSavedSounds.FindAll(ps => Math.Ceiling(ps.BasePrice * priceFactor) <= EconomyManager.GetBalanceForUser(evt.InvokerUniqueId)).OrderBy(ps => r.Next()).First();
                } else {
                    List<Playsound> sounds = Settings.PlaysoundsSavedSounds.FindAll(ps => ps.Name.ToLower().Contains(SoundSource.ToLower()));

                    //Fix for matching the exact sound name
                    Playsound exactSound = Settings.PlaysoundsSavedSounds.Find(ps => ps.Name == SoundSource);
                    if (exactSound != null) {
                        sounds.Clear();
                        sounds.Add(exactSound);
                    }

                    if (sounds.Count == 0) {
                        messageCallback.Invoke(ColorCoder.Error($"A playsound with the name {ColorCoder.Bold($"'{SoundSource}'")} wasn't found!"));
                        return;

                    } else if (sounds.Count > 1) {
                        string soundsJoined = string.Join(", ", sounds.Select(ps => ColorCoder.Bold($"'{ps.Name}'")));
                        messageCallback.Invoke(ColorCoder.Error($"Multiple sounds with {ColorCoder.Bold($"'{SoundSource}'")} in their name were found: ({soundsJoined})"));
                        return;

                    } else {
                        sound = sounds[0];
                    }
                }

                basePrice = sound.BasePrice;
                filePath = Utils.GetProjectFilePath($"{SoundsFolder}\\{sound.FileName}");
                baseDuration = AudioHelper.GetAudioDurationInSeconds(filePath);
                playingSoundStr = ColorCoder.SuccessDim($" Playing sound {ColorCoder.Bold($"'{sound.Name}'")}.");
            }

            

            int modifiedPrice = Math.Max(1, (int)Math.Round(basePrice * priceFactor));
            double modifiedDuration = baseDuration / speed;

            int balanceAfter = EconomyManager.GetUserBalanceAfterPaying(evt.InvokerUniqueId, modifiedPrice);
            if (balanceAfter < 0) {
                messageCallback.Invoke(ColorCoder.Error($"You dont have enough cash for that sound, {ColorCoder.Username(evt.InvokerName)}. Price: {ColorCoder.Currency(modifiedPrice, Settings.EcoPointUnitName)}, Needed: {ColorCoder.Currency(-balanceAfter, Settings.EcoPointUnitName)}"));
                return;
            }

            CooldownManager.SetCooldown(evt.InvokerUniqueId, "command:playsounds", CalculateCooldown(modifiedDuration));

            EconomyManager.ChangeBalanceForUser(evt.InvokerUniqueId, -modifiedPrice);

            string usernameStr = ColorCoder.Username(evt.InvokerName);
            string priceAdditionStr = priceFactor == 1 ? "" : $" x{priceFactor:0.##} = -{modifiedPrice}";
            string balanceStr = $"Your balance: {EconomyManager.GetBalanceForUser(evt.InvokerUniqueId)} {Settings.EcoPointUnitName} {ColorCoder.ErrorBright($"(-{basePrice}{priceAdditionStr})")}";
            messageCallback.Invoke($"{usernameStr} {playingSoundStr} {balanceStr}");

            Process audioProcess = AudioHelper.PlayAudio(filePath, volume, speed, pitch, audioDevice:Settings.PlaysoundsSoundDevice);
            AllStartedSoundProcesses.Add(audioProcess);
        }


        private bool CheckValidModifiers(Dictionary<string, double> manualModifiers, Action<string> messageCallback) {
            if (manualModifiers == null) {
                return true;
            }

            foreach (string currentMod in manualModifiers.Keys) {
                if (!ModifierRanges.ContainsKey(currentMod)) {
                    messageCallback.Invoke(ColorCoder.Error($"The modifier '{currentMod}' doesn't exist!"));
                }
                double manualVal = manualModifiers[currentMod];
                double min = ModifierRanges[currentMod].Item1;
                double max = ModifierRanges[currentMod].Item2;

                if (manualVal < min || manualVal > max) {
                    messageCallback.Invoke(ColorCoder.Error($"The modifier '{currentMod}' is not in the valid range of [{min} to {max}]!"));
                    return false;
                }
            }

            return true;
        }

        private void AddPlaysound(string name, uint price, string fileName, string source, Action<string> messageCallback) {
            if (Settings.PlaysoundsSavedSounds.FirstOrDefault(ps => ps.Name == name) != null) {
                messageCallback.Invoke(ColorCoder.Error($"A playsound with the name {ColorCoder.Bold($"'{name}'")} already exists!"));
                return;
            }

            string filePath = Utils.GetProjectFilePath($"{SoundsFolder}\\{fileName}");
            if(!File.Exists(filePath)) {
                messageCallback.Invoke(ColorCoder.Error($"No file with the name {ColorCoder.Bold($"'{fileName}'")} was found in the playsounds folder!"));
                return;
            }

            Settings.PlaysoundsSavedSounds.Add(new Playsound() {
                Name = name,
                FileName = fileName,
                BasePrice = (int) price,
                Source = source,
            });
            Settings.DelayedSave();

            messageCallback.Invoke(ColorCoder.Success($"Added playsound {ColorCoder.Bold($"'{name}'")}!"));
        }

        private void RemovePlaysound(string name, Action<string> messageCallback) {
            if (Settings.PlaysoundsSavedSounds.FirstOrDefault(ps => ps.Name == name) == null) {
                messageCallback.Invoke(ColorCoder.Error($"A playsound with the name {ColorCoder.Bold($"'{name}'")} wasn't found!"));
                return;
            }

            Playsound sound = Settings.PlaysoundsSavedSounds.First(ps => ps.Name == name);
            Settings.PlaysoundsSavedSounds.Remove(sound);
            Settings.DelayedSave();

            messageCallback.Invoke(ColorCoder.Success($"Removed playsound {ColorCoder.Bold($"'{name}'")}!"));
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

        public static TimeSpan CalculateCooldown(double soundDuration) {
            return TimeSpan.FromSeconds(soundDuration * MySettings.Instance.PlaysoundsDurationCooldownMultiplier);
        }

        public static int CalculateYoutubePlaysoundCost(double duration) {
            //0.00085x^{2}\ +\ 0.41x
            double coefficientA = MySettings.Instance.PlaysoundsYoutubePriceCoefficientA;
            double coefficientB = MySettings.Instance.PlaysoundsYoutubePriceCoefficientB;
            double cost = (coefficientA * (duration * duration)) + (coefficientB * duration);
            return Math.Max(1, (int)Math.Round(cost));
        }
        #endregion
    }

    public class ProgressSaver : IProgress<double> {
        public void Report(double value) {
            PlaysoundsCommand.LoadingPercentDone = value;
        }
    }
}
