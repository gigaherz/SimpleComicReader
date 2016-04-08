using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleComicReader.Readers;

namespace SimpleComicReader.Collections
{
    class ComicFolder : ComicCollectionBase
    {
        public DirectoryInfo Dir { get; }

        public ComicFolder(DirectoryInfo dir)
        {
            Dir = dir;
        }

        protected override void LoadElements()
        {
            var sourceName = Dir.FullName.Replace(':', '_').Replace('\\', '_').Replace('/', '_');

            foreach (var file in from file in Dir.EnumerateFiles()
                   from acceptor in ComicFileBase.Acceptors
                   where acceptor.Accepts(file)
                   select acceptor.Load(sourceName, file))
            {
                Elements.Add(file);
            }
        }
    }
}
