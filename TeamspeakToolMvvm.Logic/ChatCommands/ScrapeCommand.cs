using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSClient.Events;

namespace TeamspeakToolMvvm.Logic.ChatCommands {
    public class ScrapeCommand : ChatCommand {

        public static object ScraperLock = new object();
        public static bool IsScraperBusy = false;


        public static Dictionary<string, Action<NotifyTextMessageEvent, string, List<string>, Action<string>>> Scrapers { get; set; } = new Dictionary<string, Action<NotifyTextMessageEvent, string, List<string>, Action<string>>>() {
            ["2v2"] = Handle2v2CupScrape,
        };

        public override string CommandPrefix { get; set; } = "scrape";
        public override List<string> CommandAliases {
            get {
                return Scrapers.Keys.ToList();
            }
            set {

            }
        }
        public override bool HasExceptionWhiteList { get; set; } = true;


        public override bool IsValidCommandSyntax(string command, List<string> parameters) {
            return parameters.Count == 0;
        }

        public override string GetUsageDescription(string command, List<string> parameters) {
            return "A scraping tool to fetch some info from the web";
        }

        public override string GetUsageSyntax(string command, List<string> parameters) {
            return command;
        }

        public override bool CanExecuteSubCommand(string uniqueId, string command, List<string> parameters) {
            return false;
        }

        public override void HandleCommand(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            if (command == "scrape") {
                string toPrint = $"All available scrapers ({Scrapers.Count}):";
                foreach (string scraper in Scrapers.Keys) {
                    toPrint += $"\n\t- {Settings.ChatCommandPrefix}{scraper}";
                }
                toPrint += $"\n\nScraper usage in chat: {Settings.ChatCommandPrefix}{{scraperName}}";
                messageCallback.Invoke(toPrint);

            } else {
                if (IsScraperBusy) {
                    messageCallback.Invoke($"The scraper is currently busy. Try again later!");
                    return;
                }

                try {
                    lock (ScraperLock) {
                        IsScraperBusy = true;
                        Scrapers[command].Invoke(evt, command, parameters, messageCallback);
                        IsScraperBusy = false;
                    }
                } catch (Exception ex) {
                    Parent.LogMessage($"Encountered error in scraping of '{command}': {ex}");
                    messageCallback.Invoke($"Could not fetch information. Error: {ex.GetType().Name}");
                }
            }
        }


        public static void Handle2v2CupScrape(NotifyTextMessageEvent evt, string command, List<string> parameters, Action<string> messageCallback) {
            string url = "https://liquipedia.net/ageofempires/2v2_World_Cup/2020";
            HtmlWeb web = new HtmlWeb();

            HtmlDocument doc = web.Load(url);

            //HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'wikitable wikitable-striped infobox_matches_content')]");
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//table[@class='wikitable wikitable-striped infobox_matches_content']");

            if (nodes.Count == 0) {
                messageCallback.Invoke("There are no upcoming matches scheduled.");
                return;
            }

            List<Tuple<string, string>> matches = new List<Tuple<string, string>>();

            foreach (HtmlNode tableNode in nodes) {
                HtmlNode tbody = tableNode.ChildNodes[0];
                matches.Add(Tuple.Create(tbody.ChildNodes[0].InnerText, tbody.ChildNodes[1].InnerText));
            }

            string toPrint = "All upcoming matches:";

            foreach (Tuple<string, string> match in matches) {
                toPrint += $"\n\t- {match.Item1} @ {match.Item2}";
            }

            messageCallback.Invoke(toPrint);
        }
    }
}
