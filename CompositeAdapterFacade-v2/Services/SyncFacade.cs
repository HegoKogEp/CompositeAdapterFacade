using System;
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