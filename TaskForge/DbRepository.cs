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

        public User CreateUser(string username, string password, string email) 
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            var newId = db.ExecuteScalar<int>(@"
            insert into dbo.Users (name, password, created_date, updated_date, email)
            output inserted.id
            values (@Name, @Password, @createdDate, @createdDate, @Email)",
            new { Name = username, Password = password, createdDate = currentDate, Email = email });

            return new User { Id = newId, Name = username, Password = password, CreatedDate = currentDate, UpdatedDate = currentDate, Email = email}; 
        }

        public User? GetUser(int id)
        {
            using var db = new SqlConnection( _databaseConnection);

            var user = db.QueryFirstOrDefault<User>("select * from users where id = @Id", new { Id = id });

            return user;
        }

        public List<User>? GetUsers(int pageSize, int pageNum)
        {
            using var db = new SqlConnection(_databaseConnection);

            var users = db.Query<User>("select * from users OFFSET @skipCount FETCH @PageSize ROWS ONLY", new { skipCount = pageNum * pageSize, PageSize = pageSize }).ToList();

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

        public Role? GetRoleById(int roleId)
        {
            // Return a Role object by the id of the role we want in the Roles table
            using var db = new SqlConnection(_databaseConnection);
            var role = db.QueryFirstOrDefault<Role>("select * from dbo.Roles where id = @RoleId", new { RoleId = roleId});

            return role;

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

            int newId  = db.ExecuteScalar<int>(@"
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

        public void UpdateDateModified(string tableName, int id)
        {
            var validTableNames = new List<string> { "Users", "Jobs" };

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

        public Job CreateJob(string name, string status, string location)
        {
            using var db = new SqlConnection(_databaseConnection);
            var currentDate = DateTime.UtcNow;

            var newJob = new Job() { Name = name, Status = status, Location = location, CreatedDate = currentDate, UpdatedDate = currentDate };

            var newId = db.ExecuteScalar<int>(@"
            insert into dbo.Jobs (name, status, location, created_date, updated_date)
            output inserted.id
            values (@Name, @Status, @Location, @CreatedDate, @CreatedDate)", newJob);

            newJob.Id = newId;
            return newJob;

        }

        public void AssignJob(int jobId, int userId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = db.Execute(@"
                        update dbo.Jobs SET user_id = @UserId
                        where id = @JobId",
                        new { JobId = jobId, UserId = userId }, transaction);

            if (rowsAffected != 1) 
            { 
                transaction.Rollback();
                throw new Exception("Invalid job ID");
            }

            transaction.Commit();

        }

        public List<Job> GetUserJobs(int userId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var userJobs = db.Query<Job>(@" select * from dbo.Jobs where id = @UserId", new { UserId = userId }).ToList();

            return userJobs;
        }

        public Job GetJobById(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);

            var job = db.QueryFirstOrDefault<Job>(@"
                                                    SELECT 
                                                        id AS Id,
                                                        name AS Name,
                                                        status AS Status,
                                                        location AS Location,
                                                        user_id AS UserId,
                                                        created_date AS CreatedDate,
                                                        updated_date AS UpdatedDate,
                                                        due_date AS DueDate
                                                    FROM dbo.Jobs
                                                    WHERE id = @JobId", new { JobId = jobId });

            if (job == null)
            {
                throw new Exception($"Job with ID {jobId} could not be found");
            }

            return job;
        }

        public Job ClearJob(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            var currentDate = DateTime.UtcNow;

            int rowsAffected = db.Execute(@"UPDATE Jobs set 
                                            name = '',
                                            status = '',
                                            location = '',
                                            updated_date = @CurrentDate,
                                            due_date = NULL
                                            where id = @JobId" , new { CurrentDate = currentDate, JobId = jobId }, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback(); throw new Exception("Could not clear job");
            }

            transaction.Commit();
            return GetJobById(jobId);
        }
        public Job UpdateJob(UpdateJobRequest updatedJob, int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            // Begin query and initialize empty parameters object
            var query = "update Jobs set ";
            var parameters = new DynamicParameters();
            bool firstField = true;

            // Check for each field and append to query & parameters
            if (!string.IsNullOrEmpty(updatedJob.Name))
            {
                query += $"{(firstField ? "" : ", ")}name = @Name";
                parameters.Add("Name", updatedJob.Name);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedJob.Location))
            {
                query += $"{(firstField ? "" : ", ")}location = @Location";
                parameters.Add("Location", updatedJob.Location);
                firstField = false;
            }

            if (!string.IsNullOrEmpty(updatedJob.Status))
            {
                query += $"{(firstField ? "" : ", ")}status = @Status";
                parameters.Add("Status", updatedJob.Status);
                firstField = false;
            }

            if (updatedJob.DueDate.HasValue) 
            {
                query += $"{(firstField ? "" : ", ")}due_date = @DueDate";
                parameters.Add("DueDate", updatedJob.DueDate);
                firstField = false;
            }

            // If we did not find any fields in the passed job request object
            if (firstField)
            {
                throw new Exception("Update attempted with empty job request");
            }

            // Append WHERE clause to query
            query += " where Id = @Id";
            parameters.Add("Id", jobId);

            int rowsAffected = db.Execute(query, parameters, transaction);

            if (rowsAffected != 1)
            {
                transaction.Rollback();
                throw new Exception("Could not update job.");
            }
                        

            transaction.Commit();

            UpdateDateModified("Jobs", jobId);
            
            var job = GetJobById(jobId);

            return job;
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

        public void DeleteJob(int jobId)
        {
            using var db = new SqlConnection(_databaseConnection);
            db.Open();
            var transaction = db.BeginTransaction();

            int rowsAffected = db.Execute(@"delete from dbo.Jobs 
                                            where id = @JobId", new { @JobId = jobId}, transaction);

            if (rowsAffected != 0)
            {
                transaction.Rollback();
                throw new Exception($"Could not delete job with id {jobId}");
            }

            transaction.Commit();
        }



    }
}

/*
 select R.* from dbo.UserRoles as UR 
join dbo.Roles as R on UR.role_id = R.id where UR.[user_id] = 1
 */
