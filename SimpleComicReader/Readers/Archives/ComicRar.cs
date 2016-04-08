using System.IO;
using SharpCompress.Archive.Rar;

namespace SimpleComicReader.Readers.Archives
{
    class ComicRar : ComicArchive<RarArchive>
    {
        public ComicRar(string sourceName, FileInfo file) : base(sourceName, file)
        {
        }

        protected override RarArchive OpenArchive()
        {
            return RarArchive.Open(File.FullName);
        }

        public static void Initialize()
        {
            Acceptors.Add(new Acceptor(".cbr", (s, i) => new ComicRar(s, i)));
        }
    }
}