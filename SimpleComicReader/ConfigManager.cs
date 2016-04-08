using System.IO;
using System.Reflection;
using GDDL;
using GDDL.Config;
using GDDL.Structure;

namespace SimpleComicReader
{
    class ConfigManager
    {
        public static string LastFolder { get; set; }
        public static string LastBook { get; set; }
        public static int LastPage { get; set; }

        public static void LoadConfig()
        {
            var settingsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsFile = Path.Combine(settingsFolder ?? ".", "Config.gddl");

            if (File.Exists(settingsFile))
            {
                using (var parser = Parser.FromFile(settingsFile))
                {
                    var data = parser.Parse(true);

                    var root = data.AsSet();
                    LastFolder = root.ContainsKey("LastFolder") ? root["LastFolder"].AsValue().String : null;
                    LastBook = root.ContainsKey("LastBook") ? root["LastBook"].AsValue().String : null;
                    LastPage = root.ContainsKey("LastPage") ? (int) root["LastPage"].AsValue().Integer : 0;
                }
            }
        }

        public static void SaveConfig()
        {
            var settingsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsFile = Path.Combine(settingsFolder ?? ".", "Config.gddl");

            var root = new Set();
            if(!string.IsNullOrEmpty(LastFolder))
                root.Add(Element.NamedElement("LastFolder", Element.StringValue(LastFolder)));
            if(!string.IsNullOrEmpty(LastBook))
                root.Add(Element.NamedElement("LastBook", Element.StringValue(LastBook)));
            root.Add(Element.NamedElement("LastPage", Element.IntValue(LastPage)));

            File.WriteAllText(settingsFile, root.ToString(new StringGenerationContext(StringGenerationOptions.Nice)));
        }
    }
}
