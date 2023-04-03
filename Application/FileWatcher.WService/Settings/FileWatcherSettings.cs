using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileWatcher.Models;
using Newtonsoft.Json;

namespace FileWatcher.WService.Settings
{
    /// <summary>
    /// FileWatcherSettings Class
    /// </summary>
    public class FileWatcherSettings
    {
        /// <summary>
        /// FileWatcherSettings Class
        /// </summary>
        public string SettingsPath => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings\\";
        /// <summary>
        /// FileWatcherSettings Class
        /// </summary>
        public IEnumerable<FolderWatcher> Folders { get; }
        /// <summary>
        /// FileWatcherSettings Class
        /// </summary>
        public SupportSettings Support { get; }
        /// <summary>
        /// FileWatcherSettings Class
        /// </summary>
        public List<string> Plugins { get; } = new List<string>();
        public string Log;


        /// <summary>
        /// Process the Initial Settings 
        /// </summary>
        public FileWatcherSettings()
        {
            string fileJson= null;

            try
            {
                fileJson = File.ReadAllText(SettingsPath + "FileWatcherSettings.json");
             
            }
            catch {
                File.AppendAllText(SettingsPath +"\\StartError.txt" , "FileWatcher can not init because file Json: 'FileWatcherSettings.json' not found or there are errors ");
            }

            
            var configuration = JsonConvert.DeserializeObject<dynamic>(fileJson);

            if (configuration.Folders != null)
                Folders = JsonConvert.DeserializeObject<List<FolderWatcher>>(configuration.Folders.ToString());

            if (configuration.Support != null)
                Support = JsonConvert.DeserializeObject<SupportSettings>(configuration.Support.ToString());

            if(configuration.Plugins != null)
                Plugins = JsonConvert.DeserializeObject<List<string>>(configuration.Plugins.ToString());

            if(configuration.Log != null )
                Log = configuration.Log.ToString();

        }
    }
}