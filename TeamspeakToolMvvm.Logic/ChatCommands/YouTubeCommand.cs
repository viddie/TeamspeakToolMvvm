using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Groups;
using TeamspeakToolMvvm.Logic.Misc;
using TSClient.Events;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    class YouTubeCommand : ChatCommand {
        public override string CommandPrefix { get; set; } = "youtube";
        public override List<string> CommandAliases { get; set; } = new List<string>() { "yt" };
        public override bool HasExceptionWhiteList { get; set; } = true;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return parameters.Count == 1;
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return "youtube <url|id>";
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return $"Fetches basic meta data for a linked youtube video";
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string url = parameters[0];

            if (url.ToLower().StartsWith("[url]")) {
                url = url.Substring("[url]".Length, url.Length - ("[/url]".Length + "[url]".Length));
            }

            url = url.Contains("v=") ? url : $"https://youtube.com/watch?v={url}";

            YoutubeClient youtube = new YoutubeClient();
            Video video = youtube.Videos.GetAsync(url).Result;

            string title = video.Title;
            string author = video.Author;
            long likeCount = video.Engagement.LikeCount;
            long dislikeCount = video.Engagement.DislikeCount;
            long views = video.Engagement.ViewCount;
            TimeSpan duration = video.Duration;

            double ratio = double.NaN;
            if (likeCount != 0 || dislikeCount != 0) {
                ratio = (double)likeCount / ((double)likeCount + dislikeCount);
            }

            string yt = ColorCoder.ColorText(Color.DarkRed, "YouTube");
            string likes = ColorCoder.ColorText(Color.DarkGreen, $"↑{likeCount}");
            string dislikes = ColorCoder.ColorText(Color.Red, $"↓{dislikeCount}");

            string ratioStr = $"{ratio:0%}";
            if (ratio == 1) {
                ratioStr = ColorCoder.ColorText(Color.FromHex("22ee22"), $"↑{ratioStr}↑");
            } else if (ratio >= 0.95) {
                ratioStr = ColorCoder.ColorText(Color.DarkGreen, ratioStr);
            } else if (ratio >= 0.85) {
                ratioStr = ColorCoder.ColorText(Color.Yellow, ratioStr);
            } else {
                ratioStr = ColorCoder.ColorText(Color.Red, ratioStr);
            }

            Parent.Settings.StatisticYouTubeLinksFetched++;

            string response = $"{yt} ⇒ [url={url}]{title}[/url] [{duration}] from [B]{author}[/B] | {likes} | {dislikes} | {ratioStr}";

            Parent.IgnoreSelfTextMessage(response);
            messageCallback(response);
        }
    }
}
