using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeAdapterFacade_v2.Models
{
    public class FileItem : FileSystemItem
    {
        public long Size { get; set; }

        public FileItem(string name, long size) : base(name) 
        { 
            Size = size; 
        }

        public override long GetSize()
        {
            return Size;
        }

        public override string ToString()
        {
            return $"[FILE]: {Name} : {Size} bytes";
        }
    }
}
