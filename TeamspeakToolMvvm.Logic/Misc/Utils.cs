using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeamspeakToolMvvm.Logic.Exceptions;

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


        public static string TimeSpanToTimeString(TimeSpan span) {
            string toRet = "";

            return toRet;
        }



        /// <summary></summary>
        /// <param name="timeString">The time string.</param>
        /// <returns></returns>
        ///
        /* Code from Python
      def getTimesFromTimeString(self, timeString):#5h60s2m3500f
        fromIndex = 0
        hasTimeUnit = False

        dictTimes = { }

        for toIndex in range(1, len(timeString)+1) :
            try:
                num = int (timeString[fromIndex:toIndex])
                 hasTimeUnit = False
            except:
                if(fromIndex == toIndex):
                    return -69  #what in the world happened here
                else:
                    try:
                        num = int (timeString[fromIndex:toIndex - 1])
                         unit = timeString[toIndex - 1:toIndex]
                        fromIndex = toIndex
                        if(unit in dictTimes):
                            dictTimes[unit] += num
                        else:
                            dictTimes[unit] = num
                        hasTimeUnit = True
                    except:
                        return -1

        if(hasTimeUnit == False):
            return -2

        weeks = 0
        days = 0
        hours = 0
        minutes = 0
        seconds = 0
        miliseconds = 0

        for key in dictTimes:
            if(key == "f"):
                miliseconds = dictTimes[key]
            elif(key == "s"):
                seconds = dictTimes[key]
            elif(key == "m"):
                minutes = dictTimes[key]
            elif(key == "h"):
                hours = dictTimes[key]
            elif(key == "d"):
                days = dictTimes[key]
            elif(key == "w"):
                weeks = dictTimes[key]
            else:
                return key

        return [miliseconds, seconds, minutes, hours, days, weeks]
        */

        public static TimeSpan TimeStringToTimeSpan(string timeString) {
            int fromIndex = 0;
            bool hasTimeUnit = false;
            Dictionary<string, int> dictTimes = new Dictionary<string, int>();

            for (int toIndex = 1; toIndex < timeString.Length + 1; toIndex++) {
                int parsedNum;
                if (int.TryParse(timeString.Substring(fromIndex, toIndex - fromIndex), out _)){
                    hasTimeUnit = false;
                } else {
                    if (fromIndex == toIndex) {
                        throw new TimeStringParseException("This should never occur...");
                    }

                    if (int.TryParse(timeString.Substring(fromIndex, toIndex - fromIndex - 1), out parsedNum)) {
                        string unit = timeString.Substring(toIndex - 1, 1);
                        fromIndex = toIndex;
                        if (dictTimes.ContainsKey(unit)) {
                            dictTimes[unit] += parsedNum;
                        } else {
                            dictTimes.Add(unit, parsedNum);
                        }
                        hasTimeUnit = true;
                    } else {
                        throw new TimeStringParseException();
                    }
                }
            }

            if (!hasTimeUnit) {
                throw new TimeStringParseException("There was a missing time unit for a number");
            }

            int weeks = 0, days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

            foreach (string unit in dictTimes.Keys) {
                switch (unit) {
                    case "f":
                        milliseconds = dictTimes[unit];
                        break;
                    case "s":
                        seconds = dictTimes[unit];
                        break;
                    case "m":
                        minutes = dictTimes[unit];
                        break;
                    case "h":
                        hours = dictTimes[unit];
                        break;
                    case "d":
                        days = dictTimes[unit];
                        break;
                    case "w":
                        weeks = dictTimes[unit];
                        break;
                }
            }

            TimeSpan toRet = new TimeSpan();
            toRet = toRet.Add(TimeSpan.FromMilliseconds(milliseconds));
            toRet = toRet.Add(TimeSpan.FromSeconds(seconds));
            toRet = toRet.Add(TimeSpan.FromMinutes(minutes));
            toRet = toRet.Add(TimeSpan.FromHours(hours));
            toRet = toRet.Add(TimeSpan.FromDays(days));
            toRet = toRet.Add(TimeSpan.FromDays(weeks * 7));

            return toRet;
        }
    }
}
