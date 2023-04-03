using JabilCore.Utilities.Enumeration;
using JabilCore.Utilities.Model;
using System;
using System.IO;

namespace Cadence.Sftp
{
    public class MoveSftp : ICustomAction
    {

        public FileResult Run(IFolderData origin, IFolderData destination)
        {
            try
            {
                string pathProcess = "Processed";

                var sftpData = destination as SFtpData;
                var fileByte = System.IO.File.ReadAllBytes($"{origin.DirectoryBase}\\{Path.GetFileName(origin.FileName)}");
                sftpData.File = new System.IO.MemoryStream(fileByte);
                JabilCore.Utilities.Network.SFtp sFtp = new JabilCore.Utilities.Network.SFtp(sftpData);
                sFtp.Upload();

                System.IO.File.Move($"{origin.DirectoryBase}\\{Path.GetFileName(origin.FileName)}", $"{origin.DirectoryBase}\\{pathProcess}\\{Path.GetFileName(origin.FileName)}");

                return new FileResult
                {
                    Status = FileStatus.Success
                };
            }
            catch (Exception exception)
            {
                return new FileResult
                {
                    Status = FileStatus.Error,
                    Exception = exception
                };
            }
        }
    }
}
