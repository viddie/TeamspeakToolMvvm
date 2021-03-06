﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace TeamspeakToolMvvm.Logic.Misc {
    public static class AudioHelper {

        public static Tuple<string, string> LoadYoutubeVideo(string videoId, int startSeconds, int duration, IProgress<double> progressWriter =null) {
            MySettings settings = MySettings.Instance;
            string youtubeFolder = settings.PlaysoundsYoutubeFolder;
            int durationSecondsLimit = settings.PlaysoundsYoutubeMaxVideoDurationSeconds;
            double maxFileSizeMb = settings.PlaysoundsYoutubeMaxVideoDurationSeconds;

            if (youtubeFolder == null) {
                youtubeFolder = Utils.GetProjectFolderPath("youtube");
            } else {
                //youtubeFolder = Path.GetFullPath(youtubeFolder);
                if (!Directory.Exists(youtubeFolder)) {
                    Directory.CreateDirectory(youtubeFolder);
                }
            }

            string existingFile = null;
            try {
                existingFile = Directory.GetFiles(youtubeFolder).First((s) => Path.GetFileNameWithoutExtension(s) == videoId);
            } catch (Exception) { }


            string cachedCutFile = $"{videoId}_{startSeconds}_{duration}";
            try {
                cachedCutFile = Directory.GetFiles(youtubeFolder).First((s) => s.Contains(cachedCutFile));
            } catch (Exception) {
                if (existingFile != null) {
                    cachedCutFile = Path.Combine(youtubeFolder, $"{cachedCutFile}{Path.GetExtension(existingFile)}");
                }
            }


            YoutubeClient youtube = new YoutubeClient();
            Video video = youtube.Videos.GetAsync($"https://youtube.com/watch?v={videoId}").Result;
            string title = video.Title;

            if (!string.IsNullOrEmpty(existingFile)) {
                Console.WriteLine("Audio was cached");
            } else {
                if (durationSecondsLimit != -1 && video.Duration.TotalSeconds > durationSecondsLimit) {
                    throw new ArgumentException($"The video is too long to download (> {durationSecondsLimit} seconds)...");
                }

                StreamManifest streamManifest = youtube.Videos.Streams.GetManifestAsync(videoId).Result;
                IStreamInfo streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

                string extension = streamInfo.Container.ToString().ToLower();
                string fileName = $"{videoId}.{extension}";
                existingFile = Path.Combine(youtubeFolder, fileName);
                string fileNameOut = $"{videoId}_{startSeconds}_{duration}.{extension}";
                cachedCutFile = Path.Combine(youtubeFolder, fileNameOut);

                // Get the actual stream
                Stream stream = youtube.Videos.Streams.GetAsync(streamInfo).Result;

                if (maxFileSizeMb != -1 && stream.Length / 1024 / 1024 > maxFileSizeMb) {
                    throw new ArgumentException($"The video is too large (>{maxFileSizeMb}MB) to download...");
                }

                Console.WriteLine($"Stream length (MB): {stream.Length / 1024 / 1024}");
                stream.Dispose();

                progressWriter = progressWriter ?? new ProgressWriter();
                Console.Write($"Downloading stream: {streamInfo.Size} bytes  ({streamInfo.Container.Name})");
                youtube.Videos.Streams.DownloadAsync(streamInfo, existingFile, new ProgressWriter()).Wait();

                if (existingFile.EndsWith(".webm")) {
                    RepackageExistingWebm(existingFile);
                }
            }

            if (File.Exists(cachedCutFile)) {
                Console.WriteLine("Audio also was cut");
            } else {
                CutAudio(existingFile, cachedCutFile, startSeconds, duration);
            }


            return Tuple.Create(cachedCutFile, title);
        }

        public static async Task PlayYoutubeAudio(string videoId, int startSeconds, int duration, double speed = 1.0, double pitch = 1.0, double volume = 1.0, string audioDevice = null, int durationSecondsLimit = -1, int maxFileSizeMb = -1) {
            if (!Directory.Exists("playsounds")) {
                Directory.CreateDirectory("playsounds");
            }
            if (!Directory.Exists(@"playsounds\cut")) {
                Directory.CreateDirectory(@"playsounds\cut");
            }

            string existingFile = null;

            try {
                existingFile = Directory.GetFiles("playsounds").First((s) => s.Contains(videoId));
            } catch (Exception) { }

            if (!string.IsNullOrEmpty(existingFile)) {
                string existingExtension = Path.GetExtension(existingFile);
                Console.WriteLine("Audio was cached");
                GetAudioDurationInSeconds(existingFile);

                string cachedCutFile = $"playsounds\\cut\\{videoId}_{startSeconds}_{duration}{existingExtension}";
                if (File.Exists(cachedCutFile)) {
                    Console.WriteLine("Audio also was cut");
                } else {
                    CutAudio(existingFile, cachedCutFile, startSeconds, duration);
                }

                PlayAudio(cachedCutFile, volume, speed, pitch, audioDevice: audioDevice);
                return;
            }



            YoutubeClient youtube = new YoutubeClient();

            // You can specify video ID or URL
            Video video = await youtube.Videos.GetAsync($"https://youtube.com/watch?v={videoId}");

            string title = video.Title;
            string author = video.Author;

            if (durationSecondsLimit != -1 && video.Duration.TotalSeconds > durationSecondsLimit) {
                throw new ArgumentException("The video is too long to download...");
            }

            Console.WriteLine($"Downloading audio from '{title}' by '{author}'");

            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            IStreamInfo streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();


            if (streamInfo != null) {
                string extension = streamInfo.Container.ToString().ToLower();
                string fileName = $"playsounds\\{videoId}.{extension}";
                string fileNameOut = $"playsounds\\cut\\{videoId}_{startSeconds}_{duration}.{extension}";

                // Get the actual stream
                Stream stream = youtube.Videos.Streams.GetAsync(streamInfo).Result;

                if (maxFileSizeMb != -1 && stream.Length / 1024 / 1024 > maxFileSizeMb) {
                    throw new ArgumentException($"The video is too large (>{maxFileSizeMb}MB) to download...");
                }

                Console.WriteLine($"Stream length (MB): {stream.Length / 1024 / 1024}");
                stream.Dispose();


                Console.Write($"Downloading stream: {streamInfo.Size} bytes  ({streamInfo.Container.Name})");
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, new ProgressWriter());

                CutAudio(fileName, fileNameOut, startSeconds, duration);
                PlayAudio(fileNameOut, volume, speed, pitch, audioDevice: audioDevice);
            }
        }


        public static void CutAudio(string inFile, string outFile, int startSeconds, int duration) {
            Console.WriteLine($"Cutting audio: '{inFile}' to '{outFile}' section ({TimeSpan.FromSeconds(startSeconds)}-{TimeSpan.FromSeconds(startSeconds + duration)})");

            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -ss {startSeconds} -i {inFile} -t {duration} {outFile}",
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process p = Process.Start(psi);
            p.WaitForExit();
        }

        public static Process PlayAudio(string file, double volume = 1, double speed = 1, double pitch = 1, string audioDevice = null) {
            Console.WriteLine($"Playing audio: '{file}'");

            string audioDeviceParam = audioDevice == null ? "" : $"-audio-device={audioDevice}";

            string volumeStr = volume.ToString().Replace(",", ".");
            string speedStr = speed.ToString().Replace(",", ".");
            string pitchStr = pitch.ToString().Replace(",", ".");

            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "mpv.exe",
                Arguments = $"--af=volume={volumeStr},scaletempo=scale={speedStr},rubberband=pitch-scale={pitchStr} {audioDeviceParam} {file}",
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return Process.Start(psi);
        }

        //./ffprobe.exe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 playsounds/jqmm1xG2HcM.webm
        public static double GetAudioDurationInSeconds(string file) {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "ffprobe.exe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 {file}",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            Process p = Process.Start(psi);
            string line = p.StandardOutput.ReadLine();

            if (line == null) return double.NaN;

            line = line.Trim();
            p.WaitForExit();

            return double.Parse(line.Replace(".", ","));
        }

        private static void RepackageExistingWebm(string existingFile) {
            string absolutePath = Path.GetFullPath(existingFile);
            string folder = Path.GetDirectoryName(absolutePath);
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(absolutePath);
            string extension = Path.GetExtension(absolutePath);


            string existingFileCopy = $"{fileNameNoExtension}_copy{extension}";
            string copyAbsolutePath = Path.Combine(folder, existingFileCopy);

            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{absolutePath}\" -vcodec copy -acodec copy \"{copyAbsolutePath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            Process p = Process.Start(psi);
            p.WaitForExit();

            File.Delete(absolutePath);
            File.Move(copyAbsolutePath, absolutePath);
        }




        //./ffprobe.exe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 playsounds/jqmm1xG2HcM.webm
        public static List<Tuple<string, string>> GetAudioDevices() {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "mpv.exe",
                Arguments = $"-audio-device=help",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            Process p = Process.Start(psi);
            string input = p.StandardOutput.ReadToEnd();
            input = input.Trim();
            p.WaitForExit();

            string delimiter = "detected audio devices:";
            int startIndex = input.IndexOf(delimiter);
            input = input.Substring(startIndex + delimiter.Length);
            input = input.Trim();

            List<Tuple<string, string>> toRet = new List<Tuple<string, string>>();

            string[] lines = input.Split(new char[] { '\n' });
            foreach (string line in lines) {
                string lineTrimmed = line.Trim();
                string[] deviceInfo = lineTrimmed.Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

                string deviceId = deviceInfo[0];
                string deviceName = deviceInfo[1].Trim();
                deviceName = deviceName.Substring(1, deviceName.Length - 2);

                toRet.Add(Tuple.Create(deviceId, deviceName));
            }

            return toRet;
        }


        public static string IsYouTubeVideoUrl(string url) {
            if (url.ToLower().StartsWith("[url]")) {
                url = Utils.RemoveTag(url, "url");
            }

            if (!url.Contains("youtube") && !url.Contains("youtu.be")) {
                return null;
            }

            if (url.Contains("youtu.be")) {
                string watchId = url.Substring(url.IndexOf("youtu.be/")+9, 11);
                url = $"https://youtube.com/watch?v={watchId}";
            } else {
                url = url.Contains("v=") ? url : $"https://youtube.com/watch?v={url}";
            }

            try {
                YoutubeClient youtube = new YoutubeClient();
                Video video = youtube.Videos.GetAsync(url).Result;
                return video.Url;
            } catch (Exception) {
                return null;
            }
            return null;
        }
    }
    public class ProgressWriter : IProgress<double> {
        public void Report(double value) {
            Console.WriteLine($"Download Progress: {value:0.##%}");
        }
    }
}
