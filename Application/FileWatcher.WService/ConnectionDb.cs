using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FileWatcher.Service
{
    /// <summary>
    /// Configuration Connection to DB Class
    /// </summary>
    internal class ConnectionDb
    {
        public SqlConnection ConnectDb = new SqlConnection(); 
        private readonly string _ConnectionString;
        private readonly string _param;
        private readonly string _dbo;
        /// <summary>
        /// String Connection settings
        /// </summary>
        /// <param name="source">Server Name</param>
        /// <param name="catalog">Database Name</param>
        /// <param name="userId">User Name</param>
        /// <param name="password">Password </param>
        /// <param name="param">Search parameter </param>
        /// <param name="dbo">Table name</param>
        public ConnectionDb(string source, string catalog, string userId, string password )
        {
            _ConnectionString = $"data source = {source}; initial catalog ={catalog}; user id = {userId}; password = {password}; Encrypt=false";
            ConnectDb.ConnectionString = _ConnectionString;
        }
        /// <summary>
        /// Request to the Part diks database dbo.AR_Steps
        /// </summary>
        /// <param name="DiskSerialNumber">Serial number obtained in the search of the reference test log file</param>
        public List<string> GetDiskPartNumberFromDB(string DiskSerialNumber) {
            List<string> PartNumbers = new List<string>();
            string query = "up_GetPartNumberBySerialNumber"; 
            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            {

                SqlCommand command = new SqlCommand(query, connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@sn", DiskSerialNumber));

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) 
                {

                    PartNumbers.Add(reader.GetString(0));

                }
                reader.Close();
                Console.WriteLine("Correct Request to DB");
                connection.Close();
            }
            return PartNumbers.ToList();
           

        }

    }
}
