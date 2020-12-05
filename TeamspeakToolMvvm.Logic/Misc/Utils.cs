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


        public static string FormatTimeSpanShort(TimeSpan ts) {
            int seconds = ts.Seconds;
            if (ts.TotalSeconds < 1) {
                seconds = 1;
            }

            return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{seconds:00}";
        }
    }
}
