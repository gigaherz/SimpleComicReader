using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using GDDL;
using GDDL.Config;
using GDDL.Structure;
using SimpleComicReader.Annotations;

namespace SimpleComicReader
{
    public class ConfigManager : INotifyPropertyChanged
    {
        public static ConfigManager Instance { get; } = new ConfigManager();

        private string _lastFolder;
        private string _lastBook;
        private int _lastPage;

        public ObservableCollection<string> RecentFolders { get; } = new ObservableCollection<string>();

        public string LastFolder
        {
            get { return _lastFolder; }
            set
            {
                if (value == _lastFolder) return;
                _lastFolder = value;
                OnPropertyChanged();
            }
        }

        public string LastBook
        {
            get { return _lastBook; }
            set
            {
                if (value == _lastBook) return;
                _lastBook = value;
                OnPropertyChanged();
            }
        }

        public int LastPage
        {
            get { return _lastPage; }
            set
            {
                if (value == _lastPage) return;
                _lastPage = value;
                OnPropertyChanged();
            }
        }

        public Visibility RecentVisibility => RecentFolders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public void AddToRecent(string folder)
        {
            RecentFolders.Remove(folder);
            RecentFolders.Add(folder);
            while(RecentFolders.Count > 9)
                RecentFolders.RemoveAt(0);
            OnPropertyChanged(nameof(RecentVisibility));
        }

        public void LoadConfig()
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

                    if (root.ContainsKey("RecentFolders"))
                    {
                        RecentFolders.Clear();
                        foreach (var s in root["RecentFolders"].AsList().Select(e => e.AsValue().String))
                        {
                            RecentFolders.Add(s);
                        }
                    }
                }
            }
        }

        public void SaveConfig()
        {
            var settingsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsFile = Path.Combine(settingsFolder ?? ".", "Config.gddl");

            var root = new Set();
            if(!string.IsNullOrEmpty(LastFolder))
                root.Add(Element.NamedElement("LastFolder", Element.StringValue(LastFolder)));
            if(!string.IsNullOrEmpty(LastBook))
                root.Add(Element.NamedElement("LastBook", Element.StringValue(LastBook)));
            root.Add(Element.NamedElement("LastPage", Element.IntValue(LastPage)));

            if (RecentFolders.Count > 0)
            {
                var list = new Set();
                foreach (var s in RecentFolders)
                {
                    list.Add(Element.StringValue(s));
                }
                root.Add(Element.NamedElement("RecentFolders", list));
            }

            File.WriteAllText(settingsFile, root.ToString(new StringGenerationContext(StringGenerationOptions.Nice)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
