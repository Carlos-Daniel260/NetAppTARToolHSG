using System.ServiceProcess;

namespace FileWatcher.WService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            var myService = new WorkerService();
            myService.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            var servicesToRun = new ServiceBase[]
            {
                new WorkerService(),
            };

            ServiceBase.Run(servicesToRun);
#endif
        }
    }
}