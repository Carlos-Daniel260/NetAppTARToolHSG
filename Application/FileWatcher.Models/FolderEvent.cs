using System.Collections.Generic;

namespace FileWatcher.Models
{
    /// <summary>
    /// Event Definition
    /// </summary>
    public class FolderEvent
    {
        public EventType EventType { get; set; }
        public ActionType ActionType  { get; set; }
        public List<string> DistributionList { get; set; }
        public int StandByFilesLimit = 0; // TODO: Create StandByFiles List and Process
        public int StandByAction = 0;
        public int DelayInSeconds = 60;
        public int Attempts = 3;
    }
}
