using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JabilCore.Utilities.Enumeration;
using JabilCore.Utilities.Model;

namespace NetApp.PreParser
{
    public class TruncateSerialNumber : ICustomAction
    {
        private const string EndLine = "\r\n";

        public FileResult Run(IFolderData origin, IFolderData destination)
        {
            try
            {
                var fileOrigin = new StreamReader(origin.DirectoryBase + "\\" + origin.FileName);
                var outputContent = new List<string>();
                while (fileOrigin.Peek() >= 0)
                {
                    var line = (fileOrigin.ReadLine() ?? throw new InvalidOperationException()).ToArray();
                    outputContent.Add(line[0] == 'S' && line.Length >= 9
                        ? string.Join("", line).Substring(0, 9)
                        : string.Join("", line));
                }

                var fileDestination = new StreamWriter(destination.DirectoryBase + "\\" + destination.FileName);
                fileDestination.Write(string.Join(EndLine, outputContent));

                fileOrigin.Close();
                fileDestination.Close();

                File.Delete(origin.DirectoryBase + "\\" + origin.FileName);

                return new FileResult
                {
                    Status = FileStatus.Success
                };
            }
            catch (Exception e)
            {
                return new FileResult
                {
                    Status = FileStatus.Error,
                    Exception = e
                };
            }
        }
    }
}