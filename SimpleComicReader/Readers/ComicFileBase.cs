using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SimpleComicReader.Annotations;

namespace SimpleComicReader.Readers
{
    public abstract class ComicFileBase : ICollection<ComicPageBase>, INotifyPropertyChanged
    {
        protected readonly List<ComicPageBase> Elements = new List<ComicPageBase>();

        private bool _isLoaded;
        private bool _isThumbnailLoaded = true;
        private ImageSource _thumbnail;

        public ImageSource Thumbnail
        {
            get
            {
                EnsureLoaded();
                if (_thumbnail == null)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        _thumbnail = LoadThumbnail();
                        if (_thumbnail != null)
                        {
                            IsThumbnailLoaded = true;
                            OnPropertyChanged(nameof(Thumbnail));
                        }
                    }));
                }
                return _thumbnail;
            }
        }

        private static string CacheFolder = ".";
        static ComicFileBase()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, Application.Current.MainWindow.GetType().Assembly.GetName().Name);
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            var cacheFolder = Path.Combine(appFolder, "Thumbnails");
            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            CacheFolder = cacheFolder;
        }

        private string GetThumbnailFilename()
        {
            var sourceName = SourceName + "_" + DisplayName + ".png";

            return Path.Combine(CacheFolder, sourceName);
        }

        private ImageSource LoadThumbnail()
        {
            var imageName = GetThumbnailFilename();
            if (imageName == null)
                return null;

            if (File.Exists(imageName))
            {
                var bmp1 = new BitmapImage();
                bmp1.BeginInit();
                bmp1.UriSource = new Uri(imageName);
                bmp1.CacheOption = BitmapCacheOption.OnLoad;
                bmp1.EndInit();
                bmp1.Freeze();
                return BitmapFrame.Create(bmp1);
            }

            BitmapFrame bmp = null;

            var first = Elements.FirstOrDefault();
            if (first != null)
            {
                bmp = ResizeThumbnail(first.Image);
            }

            if (bmp != null)
            {
                SaveFrameToFile(bmp, imageName);
            }

            return bmp;
        }

        public static void SaveFrameToFile(BitmapFrame image, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(image);
                encoder.Save(fileStream);
            }
        }

        public bool IsThumbnailLoaded
        {
            get { return _isThumbnailLoaded; }
            set
            {
                if (value == _isThumbnailLoaded) return;
                _isThumbnailLoaded = value;
                OnPropertyChanged();
            }
        }

        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height)
        {
            var rect = new Rect(0, 0, width, height);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }

        // ReSharper disable once NotAccessedField.Local
        private BitmapImage _imTemp;
        private BitmapFrame ResizeThumbnail(BitmapImage im)
        {
            if (im.IsDownloading)
            {
                _imTemp = im;
                im.Changed += (s, a) =>
                {
                    _thumbnail = null;
                    OnPropertyChanged(nameof(Thumbnail));
                };
                return null;
            }

            _imTemp = null;

            int w = 128;
            int h = 128;
            if (im.Width > im.Height)
            {
                h = (int)(im.Height * 128 / im.Width);
            }
            else
            {
                w = (int)(im.Width * 128 / im.Height);
            }

            return CreateResizedImage(im, w, h);
        }

        public abstract string DisplayName { get; }
        public string SourceName { get; set; }

        protected ComicFileBase(string sourceName) { SourceName = sourceName; }

        private void EnsureLoaded()
        {
            if(!_isLoaded)
                LoadElements();
            _isLoaded = true;
        }

        protected abstract void LoadElements();

        public ComicPageBase this[int index]
        {
            get
            {
                EnsureLoaded();
                return Elements[index];
            }
        }

        public int Count
        {
            get
            {
                EnsureLoaded();
                return Elements.Count;
            }
        }

        public IEnumerator<ComicPageBase> GetEnumerator()
        {
            EnsureLoaded();
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureLoaded();
            return ((IEnumerable)Elements).GetEnumerator();
        }

        public void Add(ComicPageBase item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ComicPageBase item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(ComicPageBase[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(ComicPageBase item)
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly => true;

        public static List<ComicAcceptor> Acceptors = new List<ComicAcceptor>();

        public abstract class ComicAcceptor
        {
            public abstract bool Accepts(FileInfo file);
            public abstract ComicFileBase Load(string sourceName, FileInfo file);
        }

        public virtual void Unload()
        {
            foreach (var p in Elements)
            {
                p.Unload();
            }
            Elements.Clear();
            _isLoaded = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
