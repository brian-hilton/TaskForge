using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

namespace TaskForge
{
    public class DbRepository
    {
        private string _databaseConnection;

        public DbRepository(string databaseConnection) 
        { 
            _databaseConnection = databaseConnection; 
        }


        public void UpdateDateModified(string tableName, int id)
        {
            var validTableNames = new List<string> { "Users", "Jobs", "Crews" };

            if (!validTableNames.Contains(tableName))
            {
                throw new ArgumentException("Invalid table name.");
            }

            using var db = new SqlConnection(_databaseConnection);
            db.Open();

            var transaction = db.BeginTransaction();
            var currentDate = DateTime.UtcNow;

            int rowsAffected = db.Execute($@"
                                            update {tableName} SET updated_date = @CurrentDate
                                            where id = @Id" ,
                                            new { CurrentDate = currentDate, Id = id },
                                            transaction);

            if (rowsAffected != 1) {
                throw new Exception($"Could not update record from {tableName}");
            }

            transaction.Commit();

        }

    }
}

/*
 select R.* from dbo.UserRoles as UR 
join dbo.Roles as R on UR.role_id = R.id where UR.[user_id] = 1
 */
