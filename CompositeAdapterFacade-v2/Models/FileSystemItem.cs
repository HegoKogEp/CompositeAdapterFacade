using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeAdapterFacade_v2.Models
{
    public abstract class FileSystemItem
    {
        public string Name { get; set; }
        
        public FileSystemItem(string name)
        {
            Name = name;
        }

        public abstract long GetSize();

        public virtual void Add(FileSystemItem item)
        {
            throw new InvalidOperationException("Файл не может содержать потомков.");
        }

        public virtual void Remove(FileSystemItem item)
        {
            throw new InvalidOperationException("Файл не может содержать потомков.");
        }

        public virtual FileSystemItem? GetChild(int index)
        {
            throw new InvalidOperationException("Файл не может содержать потомков.");
        }

        public virtual List<FileSystemItem> GetChildren()
        {
            throw new InvalidOperationException("Файл не может содержать потомков.");
        }
    }
}
