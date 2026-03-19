# CompositeAdapterFacade-v2 - Документация по проекту

## Содержание
1. [Compositeadapterfacade-V2](#compositeadapterfacade-v2)
   1. [CompositeAdapterFacade-v2.csproj](#compositeadapterfacadev2csproj)
   2. [Program.cs](#programcs)
2. [Compositeadapterfacade-V2/Models](#compositeadapterfacade-v2-models)
   1. [FileItem.cs](#fileitemcs)
   2. [FileSystemItem.cs](#filesystemitemcs)
   3. [FolderItem.cs](#folderitemcs)
   4. [IFileSystem.cs](#ifilesystemcs)
3. [Compositeadapterfacade-V2/Services](#compositeadapterfacade-v2-services)
   1. [FileSystemAdapter.cs](#filesystemadaptercs)
   2. [SyncFacade.cs](#syncfacadecs)

## FILE 1: CompositeAdapterFacade-v2.csproj

<a id='compositeadapterfacadev2csproj'></a>

```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>CompositeAdapterFacade_v2</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

---

## FILE 2: FileItem.cs

<a id='fileitemcs'></a>

```csharp
﻿using System;
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
```

---

## FILE 3: FileSystemItem.cs

<a id='filesystemitemcs'></a>

```csharp
﻿using System;
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
```

---

## FILE 4: FolderItem.cs

<a id='folderitemcs'></a>

```csharp
﻿using System;
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
```

---

## FILE 5: IFileSystem.cs

<a id='ifilesystemcs'></a>

```csharp
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CompositeAdapterFacade_v2.Models
{
    public interface IFileSystem
    {
        List<string> ListItems(string path);
        byte[] ReadFile(string path);
        void WriteFile(string path, byte[] data);
        void DeleteItem(string path);
    }
}
```

---

## FILE 6: Program.cs

<a id='programcs'></a>

```csharp
﻿using System;
using System.IO;
using CompositeAdapterFacade_v2.Models;
using CompositeAdapterFacade_v2.Services;

namespace CompositeAdapterFacade_v2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Паттерн Composite: создание иерархии файловой системы");

            FolderItem root = new FolderItem("Root");
            root.Add(new FileItem("file1.txt", 100));
            root.Add(new FileItem("image.png", 250));

            FolderItem docs = new FolderItem("Docs");
            docs.Add(new FileItem("report.pdf", 500));
            docs.Add(new FileItem("notes.txt", 75));

            FolderItem work = new FolderItem("Work");
            work.Add(new FileItem("project.doc", 1200));
            docs.Add(work);

            root.Add(docs);

            Console.WriteLine($"Размер корневой директории: {root.GetSize()} байт\n");


            Console.WriteLine("Паттерн Adapter: работа через интерфейс IFileSystem");

            IFileSystem fileSystem = new FileSystemAdapter(root);

            try
            {
                var items = fileSystem.ListItems("Root");
                Console.WriteLine($"Содержимое корня: {string.Join(", ", items)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка ListItems: {ex.Message}");
            }

            try
            {
                byte[] data = fileSystem.ReadFile("Root/Docs/report.pdf");
                Console.WriteLine($"Прочитано: {data.Length} байт");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Ошибка ReadFile: {ex.Message}");
            }

            try
            {
                fileSystem.WriteFile("Root/Docs/new.txt", new byte[200]);
                var updated = fileSystem.ListItems("Root/Docs");
                Console.WriteLine($"После записи: {string.Join(", ", updated)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка WriteFile: {ex.Message}");
            }

            try
            {
                fileSystem.DeleteItem("Root/file1.txt");
                var afterDelete = fileSystem.ListItems("Root");
                Console.WriteLine($"После удаления: {string.Join(", ", afterDelete)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка DeleteItem: {ex.Message}");
            }

            Console.WriteLine();

            Console.WriteLine("Паттерн Facade: высокоуровневые операции");

            var localRoot = new FolderItem("Local");
            localRoot.Add(new FileItem("notes.txt", 200));

            var workFolder = new FolderItem("Work");
            workFolder.Add(new FileItem("report.doc", 1500));

            var notesSub = new FolderItem("Notes");
            notesSub.Add(new FileItem("todo.txt", 50));
            workFolder.Add(notesSub);

            localRoot.Add(workFolder);

            var cloudRoot = new FolderItem("Cloud");
            var backupRoot = new FolderItem("Backup");

            IFileSystem localFileSystem = new FileSystemAdapter(localRoot);
            IFileSystem cloudFileSystem = new FileSystemAdapter(cloudRoot);
            IFileSystem backupFileSystem = new FileSystemAdapter(backupRoot);

            var facade = new SyncFacade(localFileSystem, cloudFileSystem);

            Console.WriteLine("Синхронизация: Local -> Cloud");
            try
            {
                facade.SyncFolder("Local", "Cloud");
                Console.WriteLine($"Размер Cloud: {cloudRoot.GetSize()} байт");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Файл не найден: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Папка не найдена: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка синхронизации: {ex.Message}");
            }

            facade = new SyncFacade(localFileSystem, backupFileSystem);

            Console.WriteLine("Резервное копирование: Local -> Backup");
            try
            {
                facade.Backup("Local", "Backup");
                Console.WriteLine($"Размер Backup: {backupRoot.GetSize()} байт");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Файл не найден: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Папка не найдена: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка бэкапа: {ex.Message}");
            }

            // Итог
            Console.WriteLine($"\nИтоговые размеры:");
            Console.WriteLine($"Local:  {localRoot.GetSize()} байт");
            Console.WriteLine($"Cloud:  {cloudRoot.GetSize()} байт");
            Console.WriteLine($"Backup: {backupRoot.GetSize()} байт");
        }
    }
}
```

---

## FILE 7: FileSystemAdapter.cs

<a id='filesystemadaptercs'></a>

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using CompositeAdapterFacade_v2.Models;

namespace CompositeAdapterFacade_v2.Services
{
    public class FileSystemAdapter : IFileSystem
    {
        private FileSystemItem _root;

        public FileSystemAdapter(FileSystemItem root)
        {
            _root = root;
        }

        private FileSystemItem? FindByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == _root.Name)
                return _root;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            FileSystemItem? current = _root;
            int startIndex = (parts.Length > 0 && parts[0] == _root.Name) ? 1 : 0;

            for (int i = startIndex; i < parts.Length; i++)
            {
                if (current is FolderItem folder)
                {
                    current = folder.GetChildren().FirstOrDefault(c => c.Name == parts[i]);
                    if (current == null) return null;
                }
                else
                {
                    return null;
                }
            }
            return current;
        }

        private FileSystemItem? FindOrCreatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == _root.Name)
                return _root;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            FileSystemItem? current = _root;
            int startIndex = (parts.Length > 0 && parts[0] == _root.Name) ? 1 : 0;

            for (int i = startIndex; i < parts.Length; i++)
            {
                if (current is FolderItem folder)
                {
                    var child = folder.GetChildren().FirstOrDefault(c => c.Name == parts[i]);
                    if (child == null)
                    {
                        child = new FolderItem(parts[i]);
                        folder.Add(child);
                    }
                    current = child;
                }
                else
                {
                    return null;
                }
            }
            return current;
        }

        public List<string> ListItems(string path)
        {
            var item = FindByPath(path);
            var result = new List<string>();

            if (item is FolderItem folder)
            {
                foreach (var child in folder.GetChildren())
                {
                    result.Add(child.Name);
                }
            }
            return result;
        }

        public byte[] ReadFile(string path)
        {
            var item = FindByPath(path);
            if (item is FileItem file)
            {
                return new byte[file.Size];
            }
            throw new FileNotFoundException($"Файл не найден: {path}");
        }

        public void WriteFile(string path, byte[] data)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new ArgumentException($"Невалидный путь: {path}");

            string fileName = parts[^1];
            string dirPath = string.Join("/", parts.Take(parts.Length - 1));
            var parent = FindOrCreatePath(dirPath);

            if (parent is FolderItem folder)
            {
                var existing = folder.GetChildren().FirstOrDefault(c => c.Name == fileName);
                if (existing is FileItem existingFile)
                {
                    existingFile.Size = data.Length;
                }
                else
                {
                    folder.Add(new FileItem(fileName, data.Length));
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"Папка не найдена: {dirPath}");
            }
        }

        public void DeleteItem(string path)
        {
            var item = FindByPath(path);
            if (item == null)
                throw new FileNotFoundException($"Элемент не найден: {path}");

            if (item == _root)
                throw new InvalidOperationException("Нельзя удалить корневой элемент");

            if (!RemoveFromParent(_root, item))
                throw new InvalidOperationException($"Не удалось удалить: {path}");
        }

        private bool RemoveFromParent(FileSystemItem current, FileSystemItem target)
        {
            if (current is FolderItem folder)
            {
                if (folder.GetChildren().Contains(target))
                {
                    folder.Remove(target);
                    return true;
                }
                foreach (var child in folder.GetChildren())
                {
                    if (RemoveFromParent(child, target))
                        return true;
                }
            }
            return false;
        }
    }
}
```

---

## FILE 8: SyncFacade.cs

<a id='syncfacadecs'></a>

```csharp
﻿using System;
using System.Collections.Generic;
using CompositeAdapterFacade_v2.Models;

namespace CompositeAdapterFacade_v2.Services
{
    public class SyncFacade
    {
        private IFileSystem _sourceFS;
        private IFileSystem _targetFS;

        public SyncFacade(IFileSystem source, IFileSystem target)
        {
            _sourceFS = source;
            _targetFS = target;
        }

        public void SyncFolder(string sourcePath, string targetPath)
        {
            var items = _sourceFS.ListItems(sourcePath);

            foreach (var itemName in items)
            {
                string sourceItemPath = $"{sourcePath}/{itemName}";
                string targetItemPath = $"{targetPath}/{itemName}";

                try
                {
                    byte[] data = _sourceFS.ReadFile(sourceItemPath);
                    _targetFS.WriteFile(targetItemPath, data);
                }
                catch (FileNotFoundException)
                {
                    SyncFolder(sourceItemPath, targetItemPath);
                }
            }
        }

        public void Backup(string sourcePath, string backupPath)
        {
            SyncFolder(sourcePath, backupPath);
        }
    }
}
```

---

