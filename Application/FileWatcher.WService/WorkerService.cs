using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms.VisualStyles;
using FileWatcher.Models;
using FileWatcher.Notification;
using FileWatcher.Service;
using FileWatcher.WService.Settings;
using Humanizer;
using JabilCore.Utilities.Enumeration;
using JabilCore.Utilities.Model;
using static JabilCore.Utilities.IO.File;

namespace FileWatcher.WService
{
    /// <summary>
    /// Worker Service Execution
    /// </summary>
    public partial class WorkerService : ServiceBase
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private static FileWatcherSettings _config;
        private static string destination;
        private static string SearchFolder;
        private static ConnectionDb  _connectionDB ;
        private string _baseMessage;
        private static HttpClient _httpClient = new HttpClient();
        private const string url = "https://prd.apps.zap.corp.jabil.org/meswebapi/";
        private static string paramsUrl;
        private static DateTime date = DateTime.Now;

        /// <summary>
        /// initialize constructor only in debug
        /// </summary>
        public void OnDebug()
        {
            _baseMessage = $"[FilerWatcher.WService<{Environment.MachineName}>]";
            OnStart(null);

        }

        /// <summary>
        /// Worker Service Construct 
        /// </summary>
        public WorkerService()
        {
            InitializeComponent();
            _baseMessage = $"[FilerWatcher.WService<{Environment.MachineName}>]";
        }

        /// <summary>
        /// OnStop Method
        /// Notifies the user and support of the stoppage
        /// </summary>
        protected override void OnStop() 
        {
            try
            {
                File.AppendAllText(destination + "\\log.txt", $"{Environment.NewLine}[{date}] The service has stopped: FileWatcherService to MESsystem {Environment.NewLine}");
                SendErrorToSupport($"{_baseMessage} The FileWatcher Service was stopped.");
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(
                    $"[{_baseMessage}] Email delivery failed: When the FileWatcher Service was stopped > {e.Message} > {e.StackTrace}",
                    EventLogEntryType.Error);
                File.AppendAllText(destination + "\\log.txt", $"{Environment.NewLine}{date} Email delivery failed: When the FileWatcher Service was stopped > {{e.Message}} > {{e.StackTrace}}");

            }
        }
        /// <summary>
        /// Sending Email to Support
        /// </summary>
        /// <param name="message">Message information about the stoppage of the service</param>

        private static void SendErrorToSupport(string message)
        {           
            EmailNotification.Send(
                @from: "filewatcher@jabil.com",
                to: _config.Support.DistributionList,
                subject: "FileWatcherWService",
                body: message);
        }

        /// <summary>
        /// OnStart Method
        /// Automatic method called when the service is started
        /// Access to the configured folders with the credentials
        /// And Filewatcher configuration
        /// </summary>
        /// <param name="args">generic initialization parameter</param>
        /// <exception cref="Exception">That was thrown when the error occurred.</exception>
        protected override void OnStart(string[] args)
        {
            try
            {
                _config = new FileWatcherSettings();
                destination = _config.Log;
                foreach (var folder in _config.Folders)
                {
                    
                    // destination = folder.Destination.Path;
                    //SearchFolder = folder.SearchFolder.Path;
                    // _connectionDB = new ConnectionDb(folder.DBConfig.source, folder.DBConfig.Catalog, folder.DBConfig.User, folder.DBConfig.Password);
                    // paramsUrl = $"?sqlServer={folder.DBMES.source}&dataBase={folder.DBMES.Catalog}";
                    // _httpClient.DefaultRequestHeaders.Add("ApiKey", folder.DBMES.TokenApi);
                    ConnectToOrigin(folder);

                    folder.Watcher = new FileSystemWatcher
                    {
                        Path = folder.Origin.Path,
                        // Filter = string.IsNullOrEmpty(folder.Origin.Filter) ? "*.*" : folder.Origin.Filter,
                        EnableRaisingEvents = true,
                        IncludeSubdirectories=true,
                        InternalBufferSize = 1024 * 64,
                    };

                    folder.Watcher.Error += WatcherOnError;
                    folder.Watcher.Created +=OnChanged;
                }
                    var serviceLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    File.AppendAllText(destination + "\\log.txt", $"{Environment.NewLine}[{date}] The service is running in: {serviceLocation} {Environment.NewLine}");
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(
                    $"{date} Servicio Detenido , Error: {e.Message} {Environment.NewLine}",
                    EventLogEntryType.Error);
                SendErrorToSupport($"{_baseMessage} {e.Message} > {e.StackTrace}");
                File.AppendAllText(destination + "\\log.txt", $"{date}Error on start: {e.Message} ");
            }
        }

        /// <summary>
        /// The serial number data and connection to the MES service are obtained in this function
        /// </summary>
        /// <param name="source"> The object used to serialize event handler calls issued as a result of a directory change</param>
        /// <param name="eventArgs">Provides data for directory events</param>
        private static void OnChanged(object source, FileSystemEventArgs eventArgs)
        {
            int tries = 0;
            int numberOfRetries = 3; 
            
            File.AppendAllText(destination + "\\log.txt", $"Changed in Path {Environment.NewLine}{date} ");
            while (tries <= numberOfRetries)
            {
                try
                {
                    var text = ReadTar(eventArgs.FullPath);
                    if (text == null) return;
                    if (text?.Count() != 0)
                    {
                        string copyFilePath = "C:\\Users\\3601346\\Documents\\PathWatcher\\CopyDestination\\" + eventArgs.Name;
                        File.Copy(eventArgs.FullPath, copyFilePath, true);
                        string[] textArray = File.ReadAllLines(copyFilePath);
                        string serial = textArray[0].Remove(0,1);
                        var SerialCount = serial.Count();                           
                        if (SerialCount == 20)
                        {
                            textArray[0] = $"S{serial.Substring(0, 7)}";
                            File.WriteAllLines(copyFilePath, textArray);
                            
                        }
                        File.AppendAllText(destination + "\\log.txt", $"changed in {eventArgs.FullPath} {Environment.NewLine}");
                        return;
                     }

                }
                catch (Exception e)
                {
                    tries++;
                    string message = $"Error '{e.Message}' in {eventArgs.FullPath} {System.Environment.NewLine}";
                    File.AppendAllText(destination+"\\log.txt", $" {Environment.NewLine}{date}Error:  {message} {Environment.NewLine}");
                    var err = e.Message;
                }
            }
            throw new Exception($"{date} Error after {tries} tries");

        }

        /// <summary>
        /// Scan the Origin folder to use the configured credentials if you have them
        /// </summary>
        /// <param name="folder">All paths configured in JSON</param>
        private static void ConnectToOrigin(FolderWatcher folder)
        {
            var folderDataOrigin = folder.Origin.Credentials != null
                ? new SharedFolderData
                {
                    DirectoryBase = folder.Origin.Path,
                    Domain = folder.Origin.Credentials.Domain,
                    User = folder.Origin.Credentials.User,
                    Password = folder.Origin.Credentials.Password
                }
                : new FolderData
                {
                    DirectoryBase = folder.Origin.Path,
                };

            if (!CheckConnection(folderDataOrigin))
                throw new Exception("The connection to the origin folder failed.");
        } 

        /// <summary>
        /// Use and implement the credentials of each folder established in the JSON
        /// </summary>
        /// <param name="folder">Serial number obtained in the search of the reference test log file</param>
        /// <param name="fileName">The scanned file name of origin folder</param>
        private void ExecuteAction(FolderWatcher folder, string fileName)
        {
            var attempts = 1;
            var toDo = true;
            var totalTime = 0;
            while (toDo)
            {

                var folderDataOrigin = folder.Origin.Credentials != null
                    ? new SharedFolderData
                    {
                        DirectoryBase = folder.Origin.Path,
                        FileName = fileName,
                        Domain = folder.Origin.Credentials.Domain,
                        User = folder.Origin.Credentials.User,
                        Password = folder.Origin.Credentials.Password
                    }
                    : new FolderData
                    {
                        DirectoryBase = folder.Origin.Path,
                        FileName = fileName
                    };

                var newFileName = (folder.Destination.Prefix ?? string.Empty) +
                                  Path.GetFileNameWithoutExtension(fileName) +
                                  (string.IsNullOrEmpty(folder.Destination.Suffix)
                                      ? string.Empty
                                      : DateTime.Now.ToString(folder.Destination.Suffix)) +
                                  Path.GetExtension(fileName);

                IFolderData folderDataDestination = null;
                switch (folder.Destination.FolderType)
                {
                    case FolderType.Normal:
                        folderDataDestination = new FolderData
                        {
                            DirectoryBase = folder.Destination.Path,
                            FileName = newFileName
                        };
                        break;
                    case FolderType.SharedFolder:
                        folderDataDestination = new SharedFolderData
                        {
                            DirectoryBase = folder.Destination.Path,
                            FileName = newFileName,
                            Domain = folder.Destination.Credentials.Domain,
                            User = folder.Destination.Credentials.User,
                            Password = folder.Destination.Credentials.Password
                        };
                        break;
                    case FolderType.Sftp:
                        folderDataDestination = new SFtpData
                        {
                            DirectoryBase = folder.Destination.Path,
                            Domain = folder.Destination.Credentials.Domain,
                            UserName = folder.Destination.Credentials.User,
                            Password = folder.Destination.Credentials.Password,
                            Port = folder.Destination.Credentials.Port,
                            FileName = newFileName
                        };
                        break;
                }

                if (folderDataDestination.GetType() == typeof(SharedFolderData))
                {
                    if (!CheckConnection(folderDataDestination))
                        throw new Exception("The connection to the destination folder failed.");

                }
                FileResult fileResult;
                switch (folder.Event.ActionType)
                {
                    case ActionType.Custom:
                        fileResult = CustomActionCaller(origin: folderDataOrigin,
                            destination: folderDataDestination,
                            customAction: folder.Destination.CustomAction);
                        break;
                    case ActionType.Copy:
                        fileResult = Copy(folderDataOrigin: folderDataOrigin,
                            folderDataTarget: folderDataDestination);
                        break;
                    case ActionType.Move:
                        fileResult = Move(folderDataOrigin: folderDataOrigin,
                            folderDataTarget: folderDataDestination);
                        break;
                    case ActionType.Notify:
                        fileResult = new FileResult {Status = FileStatus.Success};
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (fileResult.Status != FileStatus.Success)
                {
                    EventLog.WriteEntry(
                        $"{folder.BaseMessage} This is the {attempts.ToOrdinalWords()} attempt to file {fileName}: {fileResult.Exception.Message}");

                    System.Threading.Thread.Sleep(1000 * attempts * folder.Event.DelayInSeconds);
                    totalTime += attempts * folder.Event.DelayInSeconds;
                    attempts++;

                    if (attempts > folder.Event.Attempts)
                        throw new Exception(
                            $"The file {fileName} is still locked after {attempts - 1} attempts and {totalTime} seconds waiting. ");
                }
                else
                {
                    toDo = false;
                }
            }
        }

        /// <summary>
        /// Generation of a new specific instance in case of a specific action of the path
        /// </summary>
        /// <param name="origin">Path obtained from JSON</param>
        /// <param name="destination">Path obtained from JSON</param>
        /// <param name="customAction">Action specifically listed and within the JSON parameters</param>
        private FileResult CustomActionCaller(IFolderData origin, IFolderData destination, string customAction)
        {
            var type = _assemblies.FirstOrDefault(a => a.GetTypes().Any(t => t.FullName == customAction))
                ?.GetType(customAction);

            var newInstance = (ICustomAction) Activator.CreateInstance(type ??
                                                                       throw new InvalidOperationException(
                                                                           "CustomAction | Type not match (The class cannot be found)."));

            return newInstance.Run(origin, destination);
        }

        /// <summary>
        /// Informs about an error found on the execution of the Filewatcher
        /// </summary>
        /// <param name="sender"> The object used to serialize event handler calls issued as a result of a directory change</param>
        /// <param name="errorEventArgs">Provides data for directory events</param>
        private void WatcherOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            var folder = _config.Folders.FirstOrDefault(f => f.Watcher == (FileSystemWatcher) sender);
            try
            {
                if (folder == null)
                    throw new Exception(
                        $"An error was detected in the file watcher and could not identify your parent controller.");

                var message = $"{folder.BaseMessage} {errorEventArgs.GetException().Message}";
                EventLog.WriteEntry(message + $" > {errorEventArgs.GetException().StackTrace}", EventLogEntryType.Error);

                EmailNotification.Send(
                    @from: "filewatcher@jabil.com",
                    to: folder.Event.DistributionList,
                    subject: "FileWatcherWService",
                    body: message
                );
            }
            catch (Exception e)
            {
                SendErrorToSupport($"{_baseMessage} {e.Message} > {e.StackTrace}");
            }
            finally
            {
                if (folder != null)
                {
                    ConnectToOrigin(folder);
                    FilesStopped(folder);
                }
            }
        }
        
        /// <summary>
        /// Get the path files of origin and execute its default action
        /// </summary>
        ///  <param name="folder">Serial number obtained in the search of the reference test log file</param>
        private void FilesStopped(FolderWatcher folder)
        {
            var files = Directory.EnumerateFiles(folder.Origin.Path, folder.Origin.Filter).ToList();

            foreach (var file in files)
            {
                ExecuteAction(folder, file);
            }
        }

        public static IEnumerable<string> ReadTar(string pathTar)
        {
            try
            {
                var text = File.ReadLines(pathTar).SkipWhile(p => !p.StartsWith("TP") && !p.EndsWith("TP"));
                return text;

            }
            catch (Exception ex)
            {
                File.AppendAllText(destination + "\\log.txt", $"{Environment.NewLine}[{date}]Error:  {ex.Message} {Environment.NewLine}");
                return null;
            }

        }
    }
}

