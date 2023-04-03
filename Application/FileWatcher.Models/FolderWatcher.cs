using System;
using System.IO;

namespace FileWatcher.Models
{
    public class FolderWatcher
    {
        public string BaseMessage => $"[FilerWatcher.WService<{Environment.MachineName}>] Folder Name {GetId()},";
        public string Id => GetId();

        private string GetId()
        {
            return string.IsNullOrEmpty(Name)
                ? Origin.Path.Replace("\\", string.Empty).Replace(":", string.Empty)
                : Name;
        }

        public string Name { get; set; }
        public FolderEvent Event { get; set; }
        public FolderSource Origin { get; set; }
        public FolderSource Destination { get; set; }
        public FolderSource SearchFolder { get; set; }
        public FolderSource DBConfig { get; set; }
        public FolderSource DBMES { get; set; }

        public FileSystemWatcher Watcher { get; set; }
        
    }
}