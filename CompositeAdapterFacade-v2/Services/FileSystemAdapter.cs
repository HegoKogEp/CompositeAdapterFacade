using System;
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