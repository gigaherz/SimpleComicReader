using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using SimpleComicReader.Annotations;
using SimpleComicReader.Collections;
using SimpleComicReader.Readers;
using SimpleComicReader.Readers.Archives;

namespace SimpleComicReader
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private int _currentImage;
        private int _currentSource;
        private ComicCollectionBase _sourceCollection;

        public ComicCollectionBase SourceCollection
        {
            get { return _sourceCollection; }
            set
            {
                if (Equals(value, _sourceCollection)) return;
                _sourceCollection = value;
                OnPropertyChanged();
            }
        }

        public ICommand RecentFolderCommand { get; private set; }

        public ComicFileBase CurrentSource => _currentSource >= 0 ? _sourceCollection?[_currentSource] : null;

        public ComicPageBase CurrentImage => CurrentSource?[_currentImage];

        private bool _menuMode;
        public bool MenuMode
        {
            get { return _menuMode; }
            set
            {
                if (_menuMode != value)
                {
                    _menuMode = value;
                    OnPropertyChanged(nameof(MenuVisible));
                    OnPropertyChanged(nameof(PageVisible));
                    OnPropertyChanged(nameof(ReturnVisible));
                }
            }
        }

        public Visibility MenuVisible => _menuMode ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PageVisible => _menuMode ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ReturnVisible => _currentSource >= 0 ? MenuVisible : Visibility.Collapsed;

        public MainWindow()
        {
            RecentFolderCommand = new RelayCommand(RecentFolder_Click);

            InitializeComponent();

            ComicZip.Initialize();
            ComicRar.Initialize();
            Comic7Zip.Initialize();
            ComicTar.Initialize();

            _swipeTimer = new DispatcherTimer() {Interval = TimeSpan.FromMilliseconds(100), IsEnabled = false};
            _swipeTimer.Tick += SwipeTimerTick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source?.AddHook(WndProc);

            ConfigManager.Instance.LoadConfig();

            if (!string.IsNullOrEmpty(ConfigManager.Instance.LastFolder) &&
                Directory.Exists(ConfigManager.Instance.LastFolder))
            {
                var dir = new DirectoryInfo(ConfigManager.Instance.LastFolder);

                SourceCollection = new ComicFolder(dir);

                ConfigManager.Instance.AddToRecent(dir.FullName);
                ConfigManager.Instance.SaveConfig();
                
                if (!string.IsNullOrEmpty(ConfigManager.Instance.LastBook)
                    && SourceCollection.TryFindIndex(ConfigManager.Instance.LastBook, out _currentSource))
                {
                    _currentImage = ConfigManager.Instance.LastPage;

                    if (_currentImage < 0)
                    {
                        _currentImage = 0;
                    }

                    if (_currentImage >= CurrentSource?.Count)
                    {
                        _currentImage = CurrentSource.Count - 1;
                    }

                    OnPropertyChanged(nameof(CurrentImage));
                }
                else
                {
                    _currentSource = -1;
                    _currentImage = 0;
                    MenuMode = true;

                    OnPropertyChanged(nameof(CurrentImage));
                }
            }
            else
            {
                OpenFolder();
            }

            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = CurrentSource != null ? $"{CurrentSource.DisplayName} - {_currentImage + 1}/{CurrentSource.Count} - Simple Comic Reader" : "Simple Comic Reader";
        }

        private void OpenFolder()
        {
            var folder = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (folder.ShowDialog() == true)
            {
                var path = folder.SelectedPath;

                OpenFolder(path);
            }
        }

        private void OpenFolder(string path)
        {
            ConfigManager.Instance.LastFolder = path;
            var dir = new DirectoryInfo(path);

            SourceCollection = new ComicFolder(dir);
            _currentSource = -1;
            _currentImage = 0;
            OnPropertyChanged(nameof(CurrentSource));
            OnPropertyChanged(nameof(CurrentImage));
            MenuMode = true;

            ConfigManager.Instance.AddToRecent(dir.FullName);
            ConfigManager.Instance.SaveConfig();
        }

        readonly DispatcherTimer _swipeTimer;
        int _swipeAcc;

        private void SwipeTimerTick(object sender, EventArgs e)
        {
            if (_swipeAcc > 0)
            {
                _swipeAcc = Math.Max(_swipeAcc - 10, 0);
            }
            else if (_swipeAcc < 0)
            {
                _swipeAcc = Math.Max(_swipeAcc + 10, 0);
            }

            if (_swipeAcc == 0)
                _swipeTimer.IsEnabled = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == 0x20e) // WM_MOUSEHWHEEL
            {
                _swipeAcc += wparam.ToInt32() >> 16;

                if (_swipeAcc >= 120)
                {
                    NextPage_Click(null, null);

                    _swipeAcc = 0;

                    _swipeTimer.IsEnabled = false;
                }
                else if (_swipeAcc <= -120)
                {
                    PreviousPage_Click(null, null);

                    _swipeAcc = 0;

                    _swipeTimer.IsEnabled = false;
                }
                else
                {
                    _swipeTimer.IsEnabled = true;
                }
            }

            return IntPtr.Zero;
        }

        private ComicFileBase _currentComic;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(CurrentSource) && _currentComic != CurrentSource)
            {
                _currentComic?.Unload();
                _currentComic = CurrentSource;

                ConfigManager.Instance.LastBook = CurrentSource?.DisplayName ?? "";
            }

            if (propertyName == nameof(CurrentImage))
            {
                PageViewer.ScrollToTop();
                ConfigManager.Instance.LastPage = _currentImage;
            }

            ConfigManager.Instance.SaveConfig();

            UpdateTitle();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PreviousSource_Click(object sender, RoutedEventArgs e)
        {
            _currentImage = 0;
            if (_currentSource > 0)
            {
                _currentSource--;
                OnPropertyChanged(nameof(CurrentSource));
            }
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage > 0)
            {
                _currentImage--;
            }
            else if (_currentSource > 0)
            {
                _currentSource--;
                _currentImage = CurrentSource.Count - 1;
                OnPropertyChanged(nameof(CurrentSource));
            }
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void Library_Click(object sender, RoutedEventArgs e)
        {
            MenuMode = true;
        }

        private void Reading_Click(object sender, RoutedEventArgs e)
        {
            MenuMode = false;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            _currentImage++;
            if (_currentImage >= CurrentSource.Count)
            {
                _currentSource++;
                _currentImage = 0;
                if (_currentSource >= SourceCollection.Count)
                {
                    _currentSource = SourceCollection.Count - 1;
                    _currentImage = CurrentSource.Count - 1;
                }
                OnPropertyChanged(nameof(CurrentSource));
                OnPropertyChanged(nameof(CurrentImage));
            }
            else
            {
                OnPropertyChanged(nameof(CurrentImage));
            }
        }

        private void NextSource_Click(object sender, RoutedEventArgs e)
        {
            _currentSource++;
            _currentImage = 0;
            if (_currentSource >= SourceCollection.Count)
            {
                _currentSource = SourceCollection.Count - 1;
                _currentImage = CurrentSource.Count - 1;
            }
            else
            {
                OnPropertyChanged(nameof(CurrentSource));
            }
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageUp:
                case Key.Left:
                case Key.K:
                    PreviousPage_Click(null, null);
                    e.Handled = true;
                    break;
                case Key.PageDown:
                case Key.Right:
                case Key.J:
                    NextPage_Click(null, null);
                    e.Handled = true;
                    break;
                case Key.Space:
                    ScrollPage();
                    e.Handled = true;
                    break;
                case Key.Down:
                    ScrollBitDown();
                    e.Handled = true;
                    break;
                case Key.Up:
                    ScrollBitUp();
                    e.Handled = true;
                    break;
            }
        }

        private void ScrollBitUp()
        {
            PageViewer.ScrollToVerticalOffset(Math.Max(0, PageViewer.VerticalOffset - PageViewer.ViewportHeight * 0.25f));
        }

        private void ScrollBitDown()
        {
            PageViewer.ScrollToVerticalOffset(PageViewer.VerticalOffset + PageViewer.ViewportHeight * 0.25f);
        }

        private void ScrollPage()
        {
            if (PageViewer.VerticalOffset >= PageViewer.ScrollableHeight)
            {
                NextPage_Click(null, null);
            }
            else
            {
                PageViewer.ScrollToVerticalOffset(PageViewer.VerticalOffset + PageViewer.ViewportHeight * 0.5f);
            }
        }

        private void Book_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _currentSource = BookViewer.SelectedIndex;
            _currentImage = 0;
            OnPropertyChanged(nameof(CurrentSource));
            OnPropertyChanged(nameof(CurrentImage));
            MenuMode = false;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void Recent_Click(object sender, RoutedEventArgs e)
        {
            var obj = sender as Button;
            var menu = (ContextMenu)Resources["RecentMenu"];
            menu.PlacementTarget = obj;
            menu.IsOpen = true;
        }

        private void RecentFolder_Click(object obj)
        {
            var item = (string)obj;
            OpenFolder(item);
        }
    }
}
