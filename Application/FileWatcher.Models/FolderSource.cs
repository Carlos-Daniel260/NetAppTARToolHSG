namespace FileWatcher.Models
{
    public class FolderSource
    {
        public string Path { get; set; }
        public string Filter { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public FolderCredential Credentials { get; set; }
        public string CustomAction { get; set; }
        public FolderType FolderType { get; set; } = FolderType.Normal;
        /// <summary>
        /// DB Credentials 
        /// </summary>
        public string source { get; set; }
        public string Catalog { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string TokenApi { get; set; }
        public string Search { get; set;}
        public string Dbo { get; set; }

    }
}