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
        public User CreateUser(string username, string password, string email)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            var newId = db.ExecuteScalar<int>(@"
            insert into dbo.Users (name, password, created_date, updated_date, email)
            output inserted.id
            values (@Name, @Password, @createdDate, @createdDate, @Email)",
            new { Name = username, Password = password, createdDate = currentDate, Email = email });

            return new User { Id = newId, Name = username, Password = password, CreatedDate = currentDate, UpdatedDate = currentDate, Email = email };
        }

        public User? GetUser(int id)
        {
            using var db = new SqlConnection(_databaseConnection);

            var user = db.QueryFirstOrDefault<User>("select * from users where id = @Id", new { Id = id });

            return user;
        }

        public List<User>? GetUsers(int pageSize, int pageNum)
        {
            using var db = new SqlConnection(_databaseConnection);

            var users = db.Query<User>("select * from users OFFSET @skipCount FETCH @PageSize ROWS ONLY", new { skipCount = pageNum * pageSize, PageSize = pageSize }).ToList();

            return users;
        }

        public List<User>? GetTopUsers(int top)
        {
            using var db = new SqlConnection(_databaseConnection);
            var users = db.Query<User>(@"select top(@Top) * from users", new { Top = top }).ToList();
            return users;
        }

        // Can return a List of Roles associated with userId
        public List<Role> GetUserRole(int id)
        {
            using var db = new SqlConnection(_databaseConnection);

            var roles = db.Query<Role>(@"select R.* 
                                        from dbo.UserRoles as UR 
                                        join dbo.Roles as R on UR.role_id = R.id 
                                        where UR.[user_id] = @UserId",
                                        new { UserId = id }).ToList();

            return roles;

        }

        public User UpdateUser(UpdateUserRequest updatedUser, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

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

            // If we did not find any fields in the passed job request object
            if (firstField)
            {
                throw new Exception("Update attempted with empty job request");
            }

            // Append WHERE clause to query
            query += " where Id = @Id";
            parameters.Add("Id", userId);

            int rowsAffected = db.Execute(query, parameters, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                throw new Exception("Could not update job.");
            }


            transaction.Commit();

            UpdateDateModified("Users", userId);

            var user = GetUser(userId);

            if (user != null) { return user; }

            return null;

        }

        public void DeleteUser(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute(@"
                                            delete from dbo.Users 
                                            where id = @UserId", new { UserId = userId }, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback(); throw new Exception($"Could not delete user with id: {userId}");
            }

            transaction.Commit();
        }

        public UserRole CreateUserRole(int userId, int roleId)
        {
            // Make sure role exists by checking Role table
            // Run query to insert Role into UserRole table
            // Call update method to modify last_updated in User table

            var role = GetRoleById(roleId);
            if (role == null)
            {
                throw new Exception($"Could not get user role for roleId {roleId}");
            }

            using var db = new SqlConnection(_databaseConnection);

            var currentDate = DateTime.UtcNow;

            int newId = db.ExecuteScalar<int>(@"
                                insert into dbo.UserRoles (user_id, role_id)
                                output inserted.id
                                values (@Id, @RoleId)",
                                new { Id = userId, RoleId = role.Id });

            if (newId == 0)
            {
                throw new Exception("Could not assign role for user.");
            }

            var userRole = new UserRole { Id = newId, UserId = userId, RoleId = role.Id };

            // Update User.UpdatedDate to show that we have made a change
            UpdateDateModified("Users", userId);

            return userRole;
        }
    }
}
