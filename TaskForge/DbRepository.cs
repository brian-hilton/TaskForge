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


        public async Task UpdateDateModifiedAsync(string tableName, int id)
        {
            var validTableNames = new List<string> { "Users", "Jobs", "Crews" };

            if (!validTableNames.Contains(tableName))
            {
                //throw new ArgumentException("Invalid table name.");
            }

            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();

            var transaction = db.BeginTransaction();
            var currentDate = DateTime.UtcNow;

            int rowsAffected = await db.ExecuteAsync($@"
                                            update {tableName} SET updated_date = @CurrentDate
                                            where id = @Id" ,
                                            new { CurrentDate = currentDate, Id = id },
                                            transaction);

            if (rowsAffected != 1) {
                //throw new Exception($"Could not update record from {tableName}");
            }

            await transaction.CommitAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            // Return a Role object by the id of the role we want in the Roles table
            using var db = new SqlConnection(_databaseConnection);
            return await db.QueryFirstOrDefaultAsync<Role>("select * from dbo.Roles where id = @RoleId", new { RoleId = roleId });
        }

    }
}

/*
 select R.* from dbo.UserRoles as UR 
join dbo.Roles as R on UR.role_id = R.id where UR.[user_id] = 1
 */
