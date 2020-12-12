using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Misc
{
    public static class Utils
    {
        public static string GetProjectFilePath(string fileName, string folder=null) {
            string baseFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (folder != null) {
                baseFolder = Path.Combine(baseFolder, folder);
                if (!Directory.Exists(baseFolder)) {
                    Directory.CreateDirectory(baseFolder);
                }
            }

            return Path.Combine(baseFolder, fileName);
        }
        public static string GetProjectFolderPath(string folderName) {
            string baseFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string folder = Path.Combine(baseFolder, folderName);
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }


        public static string FormatTimeSpanShort(TimeSpan ts) {
            int seconds = ts.Seconds;
            if (ts.TotalSeconds < 1) {
                seconds = 1;
            }

            return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{seconds:00}";
        }

        public static string RemoveTag(string baseString, string tag) {
            string openTag = $"[{tag}]";
            string closeTag = $"[/{tag}]";

            return baseString.Substring(openTag.Length, baseString.Length - (closeTag.Length + openTag.Length));
        }

        public static string FormatBytes(long bytes) {
            string negativeSign = "";
            if (bytes < 0) {
                negativeSign = "-";
                bytes = Math.Abs(bytes);
            }

            if (bytes >= 1073741824) {
                return $"{negativeSign}{bytes / 1073741824:0.##} GB";
            } else if (bytes >= 1048576) {
                return $"{negativeSign}{bytes / 1048576:0.##} MB";
            } else if (bytes >= 1024) {
                return $"{negativeSign}{bytes / 1024:0.##} KB";
            } else if (bytes > 1) {
                return $"{negativeSign}{bytes} bytes";
            } else if (bytes == 1) {
                return $"{negativeSign}1 byte";
            } else {
                return $"0 bytes";
            }
        }
    }
}
