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
﻿using CompositeAdapterFacade_v2.Models;
using CompositeAdapterFacade_v2.Services;

namespace CompositeAdapterFacade_v2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Создаём структуру
            FolderItem root = new FolderItem("Root");
            root.Add(new FileItem("file1.txt", 100));
            FolderItem docs = new FolderItem("Docs");
            docs.Add(new FileItem("report.pdf", 500));
            root.Add(docs);

            // Создаём адаптер
            IFileSystem fs = new FileSystemAdapter(root);

            // Тестируем методы
            Console.WriteLine("Содержимое корня: " + string.Join(", ", fs.ListItems("Root")));
            // Вывод: file1.txt, Docs

            byte[] data = fs.ReadFile("Root/Docs/report.pdf");
            Console.WriteLine($"Прочитано {data.Length} байт");

            fs.WriteFile("Root/Docs/new.txt", new byte[200]);
            Console.WriteLine("Содержимое Docs после записи: " + string.Join(", ", fs.ListItems("Root/Docs")));
            // Вывод: report.pdf, new.txt

            fs.DeleteItem("Root/file1.txt");
            Console.WriteLine("Содержимое корня после удаления: " + string.Join(", ", fs.ListItems("Root")));
            // Вывод: Docs

            Console.WriteLine("=== Демонстрация паттерна Facade ===\n");

            // 1. Создаём структуры Composite (источник и цель)
            var localRoot = new FolderItem("Local");
            localRoot.Add(new FileItem("notes.txt", 200));
            var subFolder = new FolderItem("Work");
            subFolder.Add(new FileItem("report.doc", 1500));
            localRoot.Add(subFolder);

            var cloudRoot = new FolderItem("Cloud"); // пустое облако

            // 2. Создаём адаптеры (преобразуют Composite → IFileSystem)
            IFileSystem localFS = new FileSystemAdapter(localRoot);
            IFileSystem cloudFS = new FileSystemAdapter(cloudRoot);

            // 3. Создаём фасад и выполняем операции
            var facade = new SyncFacade(localFS, cloudFS);

            // Синхронизация
            facade.SyncFolder("Local", "Cloud");

            // Проверяем результат: размер облака должен стать таким же, как у локальной
            Console.WriteLine($"\nРазмер локальной: {localRoot.GetSize()} байт");
            Console.WriteLine($"Размер облачной:  {cloudRoot.GetSize()} байт");

            // Резервное копирование (в другую "папку")
            var backupRoot = new FolderItem("Backup");
            IFileSystem backupFS = new FileSystemAdapter(backupRoot);
            var backupFacade = new SyncFacade(localFS, backupFS);

            backupFacade.Backup("Local", "Backup");
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
using System.Text;
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
                Console.WriteLine($"[Adapter] Чтение файла: {path}");
                return new byte[file.Size];
            }
            throw new FileNotFoundException($"Файл не найден: {path}");
        }

        public void WriteFile(string path, byte[] data)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Console.WriteLine("[Adapter] Невалидный путь для записи");
                return;
            }

            string fileName = parts[^1];
            string dirPath = string.Join("/", parts.Take(parts.Length - 1));

            var parent = FindByPath(dirPath);

            if (parent is FolderItem folder)
            {
                var existing = folder.GetChildren().FirstOrDefault(c => c.Name == fileName);
                if (existing is FileItem existingFile)
                {
                    existingFile.Size = data.Length;
                    Console.WriteLine($"[Adapter] Обновлён файл: {path}");
                }
                else
                {
                    folder.Add(new FileItem(fileName, data.Length));
                    Console.WriteLine($"[Adapter] Создан файл: {path}");
                }
            }
            else
            {
                Console.WriteLine($"[Adapter] Папка не найдена: {dirPath}");
            }
        }

        public void DeleteItem(string path)
        {
            var item = FindByPath(path);
            if (item == null)
            {
                Console.WriteLine($"[Adapter] Элемент не найден для удаления: {path}");
                return;
            }

            if (item == _root)
            {
                Console.WriteLine("[Adapter] Нельзя удалить корневой элемент");
                return;
            }

            if (RemoveFromParent(_root, item))
            {
                Console.WriteLine($"[Adapter] Удалено: {path}");
            }
            else
            {
                Console.WriteLine($"[Adapter] Не удалось удалить: {path}");
            }
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
using System.Text;
using CompositeAdapterFacade_v2.Models;

namespace CompositeAdapterFacade_v2.Services
{
    public class SyncFacade
    {
        private IFileSystem? _sourceFileSystem;
        private IFileSystem? _destinationFileSystem;

        public SyncFacade(IFileSystem? sourceFileSystem, IFileSystem? destinationFileSystem)
        {
            _sourceFileSystem = sourceFileSystem;
            _destinationFileSystem = destinationFileSystem;
        }

        public void SyncFolder(string sourcePath, string destinationPath)
        {
            Console.WriteLine($"[FACADE] Начало синхронизации: {sourcePath} -> {destinationPath}");

            var items = _sourceFileSystem.ListItems(sourcePath);

            foreach (var item in items)
            {
                string sourceItemPath = $"{sourcePath}/{item}";
                string destinationItemPath = $"{destinationPath}/{item}";

                try
                {
                    byte[] data = _sourceFileSystem.ReadFile(sourceItemPath);
                    _destinationFileSystem.WriteFile(destinationItemPath, data);
                    Console.WriteLine($"Файл {item} синхронизирован");
                }
                catch(FileNotFoundException)
                {
                    SyncFolder(sourceItemPath, destinationItemPath);
                }
            }

            Console.WriteLine($"[FACADE] Синхронизация выполнена");
        }

        public void Backup(string sourcePath, string backupPath)
        {
            Console.WriteLine($"[FACADE] Начало резервного копирования {sourcePath} -> {backupPath}");

            try
            {
                SyncFolder(sourcePath, backupPath);
                Console.WriteLine("[FACADE] Копирование завершено успешно");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"[FACADE] Файл не найден: {ex.Message}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[FACADE] Неизвестная ошибка: {ex.Message}");
            }
        }
    }
}
```

---

