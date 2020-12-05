using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Config;

namespace TeamspeakToolMvvm.Logic.Misc {
    public static class Scrapers {
        public static string ScrapeGeneral(string url) {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HtmlWeb webGet = new HtmlWeb();
            HtmlDocument document = webGet.Load(url);
            string title = document.DocumentNode.SelectSingleNode("//title")?.InnerText;
            HtmlNodeCollection metaNodes = document.DocumentNode.SelectNodes("html/head/meta");
            if (metaNodes == null) return null;

            Dictionary<string, string> metaInfo = new Dictionary<string, string>() {
                ["author"] = null,
                ["description"] = null,
                ["keywords"] = null,

                ["twitter:site"] = null,
                ["twitter:title"] = null,
                ["twitter:description"] = null,
                ["twitter:creator"] = null,

                ["og:title"] = null,
                ["og:description"] = null,
            };

            foreach (HtmlNode node in metaNodes) {
                string attributeName = node.GetAttributeValue("name", null);
                if (attributeName == null) attributeName = node.GetAttributeValue("property", null);

                if (attributeName != null && metaInfo.ContainsKey(attributeName)) {
                    metaInfo[attributeName] = node.GetAttributeValue("content", null);
                }
            }

            title = title ?? metaInfo["twitter:title"] ?? metaInfo["og:title"];
            string author = metaInfo["author"] ?? metaInfo["twitter:creator"];
            string description = metaInfo["description"] ?? metaInfo["twitter:description"] ?? metaInfo["og:description"];
            string keywords = metaInfo["keywords"];

            if (title == null) return null;

            keywords = keywords == null ? null : string.Join(", ", keywords.Split(new string[] { ", " }, StringSplitOptions.None).Take(3));
            string authorAddition = author == null ? "" : $" by {author}";

            string toPrint = $"{ColorCoder.ColorText(Color.LightBlue, "Link")} ⇒ [url={url}]{title}[/url]{authorAddition}";

            if (MySettings.Instance.ImmediateGeneralDescriptionEnabled && description != null)
                toPrint += $"\n\"{description}\"";
            if (MySettings.Instance.ImmediateGeneralKeywordsEnabled && keywords != null)
                toPrint += $"\nKeywords: {keywords}";

            return $"{toPrint}";
        }
    }
}
