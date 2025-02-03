using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge.Repositories
{
    public class JobRepository
    {
        private string _databaseConnection;

        public JobRepository(string databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }
    }
}
