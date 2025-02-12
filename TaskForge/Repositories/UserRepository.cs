using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Data.Common;
using System.Data.SqlClient;
using TaskForge.Models;

/*
 Queries related to the role table have been moved into the user repository
*/

namespace TaskForge.Repositories
{
    public class UserRepository : DbRepository
    {
        private string _databaseConnection;

        public UserRepository(string databaseConnection) : base(databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<User> LoginAsync(LoginRequest loginRequest)
        {
            using var db = new SqlConnection(_databaseConnection);
            return await db.QueryFirstOrDefaultAsync<User>(@"
                                                            select * from dbo.users 
                                                            where email = @Email and password = @Password", 
                                                            new { Email = loginRequest.Email, Password = loginRequest.Password }).ConfigureAwait(false);
        }
        public async Task<User> CreateUserAsync(string username, string password, string email)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync().ConfigureAwait(false);

            var currentDate = DateTime.UtcNow;

            var newId = await db.ExecuteScalarAsync<int>(@"
            insert into dbo.Users (name, password, created_date, updated_date, email)
            output inserted.id
            values (@Name, @Password, @createdDate, @createdDate, @Email)",
            new { Name = username, Password = password, createdDate = currentDate, Email = email });

            return new User { Id = newId, Name = username, Password = password, CreatedDate = currentDate, UpdatedDate = currentDate, Email = email };
        }

        public async Task<User?> GetUserAsync(int id)
        {
            using var db = new SqlConnection(_databaseConnection);
            return await db.QueryFirstOrDefaultAsync<User>("select * from users where id = @Id", new { Id = id });
        }

        public async Task<List<User>> GetUsers(int pageSize, int pageNum)
        {
            using var db = new SqlConnection(_databaseConnection);
            var users = await db.QueryAsync<User>("select * from users OFFSET @skipCount FETCH @PageSize ROWS ONLY", new { skipCount = pageNum * pageSize, PageSize = pageSize });     
            return users.ToList();
        }

        public List<User>? GetTopUsers(int top)
        {
            using var db = new SqlConnection(_databaseConnection);
            var users = db.Query<User>(@"select top(@Top) * from users", new { Top = top }).ToList();
            return users;
        }

        // Can return a List of Roles associated with userId
        public async Task<List<Role>> GetUserRolesAsync(int id)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();

            var roles = await (db.QueryAsync<Role>(@"
                                        select R.* 
                                        from dbo.UserRoles as UR 
                                        join dbo.Roles as R on UR.role_id = R.id 
                                        where UR.[user_id] = @UserId",
                                        new { UserId = id }));
            return roles.ToList();
        }

        public async Task<User> UpdateUserAsync(UpdateUserRequest updatedUser, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            // Begin query and initialize empty parameters object
            var query = "update Users set ";
            var parameters = new DynamicParameters();
            bool firstField = true;

            // Check for each field and append to query & parameters
            if (!string.IsNullOrEmpty(updatedUser.Name))
            {
                query += $"{(firstField ? "" : ", ")}name = @Name";
                parameters.Add("Name", updatedUser.Name);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                query += $"{(firstField ? "" : ", ")}password = @Password";
                parameters.Add("Password", updatedUser.Password);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedUser.Email))
            {
                query += $"{(firstField ? "" : ", ")}email = @Email";
                parameters.Add("Email", updatedUser.Email);
                firstField = false;
            }

            // Append WHERE clause to query
            query += " where Id = @Id";
            parameters.Add("Id", userId);

            int rowsAffected = await db.ExecuteAsync(query, parameters, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                return null;
            }


            await transaction.CommitAsync();
            await UpdateDateModifiedAsync("Users", userId);

            return await GetUserAsync(userId);

            

        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            await db.OpenAsync();
            var transaction = await db.BeginTransactionAsync();

            int rowsAffected = await db.ExecuteAsync(@"
                                            delete from dbo.Users 
                                            where id = @UserId", new { UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await transaction.CommitAsync();
            return true;
        }

        public async Task<UserRole> CreateUserRoleAsync(int userId, int roleId)
        {
            // Make sure role exists by checking Role table
            // Run query to insert Role into UserRole table
            // Call update method to modify last_updated in User table

            var role = await GetRoleByIdAsync(roleId);

            using var db = new SqlConnection(_databaseConnection);

            var currentDate = DateTime.UtcNow;

            int newId = await db.ExecuteScalarAsync<int>(@"
                                insert into dbo.UserRoles (user_id, role_id)
                                output inserted.id
                                values (@Id, @RoleId)",
                                new { Id = userId, RoleId = role.Id });

            var userRole = new UserRole { Id = newId, UserId = userId, RoleId = role.Id };

            // Update User.UpdatedDate to show that we have made a change
            await UpdateDateModifiedAsync("Users", userId);

            return userRole;
        }
    }
}
