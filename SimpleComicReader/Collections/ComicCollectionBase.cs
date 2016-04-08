using System;
using System.Collections;
using System.Collections.Generic;
using SimpleComicReader.Readers;

namespace SimpleComicReader.Collections
{
    public abstract class ComicCollectionBase : ICollection<ComicFileBase>
    {
        protected readonly List<ComicFileBase> Elements = new List<ComicFileBase>();

        bool _isLoaded;
        private void EnsureLoaded()
        {
            if (!_isLoaded)
                LoadElements();
            _isLoaded = true;
        }

        protected abstract void LoadElements();

        public ComicFileBase this[int index]
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

        public IEnumerator<ComicFileBase> GetEnumerator()
        {
            EnsureLoaded();
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureLoaded();
            return ((IEnumerable) Elements).GetEnumerator();
        }

        public void Add(ComicFileBase item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ComicFileBase item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(ComicFileBase[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(ComicFileBase item)
        {
            throw new NotSupportedException();
        }

        public bool IsReadOnly => true;
    }
}
