using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using SimpleComicReader.Annotations;

namespace SimpleComicReader.Readers
{
    public abstract class ComicPageBase : INotifyPropertyChanged
    {
        private BitmapImage _image;
        public string Name { get; }

        public virtual BitmapImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = LoadImage();
                }

                return _image;
            }
            protected set
            {
                _image = null;
                OnPropertyChanged();
            }
        }

        protected ComicPageBase(string name)
        {
            Name = name;
        }

        public void Unload()
        {
            _image = null;
        }

        protected BitmapImage LoadImage()
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = OpenStream();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            return bmp;
        }

        protected abstract Stream OpenStream();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
