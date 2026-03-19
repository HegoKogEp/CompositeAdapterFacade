using System;
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