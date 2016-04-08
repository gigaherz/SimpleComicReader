using System;
using System.IO;
using SharpCompress.Archive;

namespace SimpleComicReader.Readers.Archives
{
    public abstract class ComicArchive<T> : ComicFileBase
        where T : IArchive
    {
        public FileInfo File { get; }

        public T Archive;

        protected ComicArchive(string sourceName, FileInfo file)
            : base(sourceName)
        {
            File = file;
        }

        public override string DisplayName => File.Name;

        protected abstract T OpenArchive();

        protected override void LoadElements()
        {
            Archive = OpenArchive();

            foreach (var entry in Archive.Entries)
            {
                Elements.Add(new Page(entry));
            }
        }

        public override void Unload()
        {
            base.Unload();
            Elements.Clear();
            Archive.Dispose();
            Archive = default(T);
        }

        public class Page : ComicPageBase
        {
            private readonly IArchiveEntry _entry;

            public Page(IArchiveEntry entry) : base(entry.Key)
            {
                _entry = entry;
            }

            protected override Stream OpenStream()
            {
                return _entry.OpenEntryStream();
            }
        }

        public class Acceptor : ComicAcceptor
        {
            private readonly string _expextedExtension;
            private readonly Func<string, FileInfo, ComicFileBase> _factory;

            public Acceptor(string extension, Func<string, FileInfo, ComicFileBase> factory)
            {
                _expextedExtension = extension;
                _factory = factory;
            }

            public override bool Accepts(FileInfo file)
            {
                return string.Compare(file.Extension, _expextedExtension, StringComparison.OrdinalIgnoreCase) == 0;
            }

            public override ComicFileBase Load(string sourceName, FileInfo file)
            {
                return _factory(sourceName, file);
            }
        }
    }
}