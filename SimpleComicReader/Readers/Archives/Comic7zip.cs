using System.IO;
using SharpCompress.Archive.SevenZip;

namespace SimpleComicReader.Readers.Archives
{
    public class Comic7Zip : ComicArchive<SevenZipArchive>
    {
        public Comic7Zip(string sourceName, FileInfo file) : base(sourceName, file)
        {
        }

        protected override SevenZipArchive OpenArchive()
        {
            return SevenZipArchive.Open(File.FullName);
        }

        public static void Initialize()
        {
            Acceptors.Add(new Acceptor(".cb7", (s, i) => new Comic7Zip(s, i)));
        }
    }
}