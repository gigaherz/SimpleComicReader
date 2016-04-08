using System.IO;
using SharpCompress.Archive.Tar;

namespace SimpleComicReader.Readers.Archives
{
    public class ComicTar : ComicArchive<TarArchive>
    {
        public ComicTar(string sourceName, FileInfo file) : base(sourceName, file)
        {
        }

        protected override TarArchive OpenArchive()
        {
            return TarArchive.Open(File.FullName);
        }

        public static void Initialize()
        {
            Acceptors.Add(new Acceptor(".cbt", (s, i) => new ComicTar(s, i)));
        }
    }
}