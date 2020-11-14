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

        public static string GetProjectFilePath(string fileName) {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return Path.Combine(folder, fileName);
        }
    }
}
