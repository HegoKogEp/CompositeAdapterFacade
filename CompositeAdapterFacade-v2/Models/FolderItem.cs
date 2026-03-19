using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace CompositeAdapterFacade_v2.Models
{
    public class FolderItem : FileSystemItem
    {
        private List<FileSystemItem> _children = new List<FileSystemItem>();

        public FolderItem(string name) : base(name) { }

        public override void Add(FileSystemItem item)
        {
            _children.Add(item);
        }

        public override void Remove(FileSystemItem item)
        {
            _children.Remove(item);
        }

        public override FileSystemItem? GetChild(int index)
        {
            return _children[index];
        }

        public override List<FileSystemItem> GetChildren()
        {
            return _children;
        }

        public override long GetSize()
        {
            long size = 0;
            foreach (FileSystemItem item in _children)
            {
                size += item.GetSize();
            }
            return size;
        }
    }
}
