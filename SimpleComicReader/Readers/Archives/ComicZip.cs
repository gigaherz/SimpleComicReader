using System.IO;
using SharpCompress.Archive.Zip;

namespace SimpleComicReader.Readers.Archives
{
    public class ComicZip : ComicArchive<ZipArchive>
    {
        public ComicZip(string sourceName, FileInfo file) : base(sourceName, file)
        {
        }

        protected override ZipArchive OpenArchive()
        {
            return ZipArchive.Open(File.FullName);
        }

        public static void Initialize()
        {
            Acceptors.Add(new Acceptor(".cbz", (s, i) => new ComicZip(s, i)));
        }
    }
}
