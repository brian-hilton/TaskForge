using System.Runtime.CompilerServices;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            string dbConnection = app.Configuration.GetConnectionString("DbConnection")!;

            // Return user from user table based off passed id in the url
            app.MapGet("/get-user", (int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var user = repo.GetUser(userId);
                return user;
            });

            app.MapGet("/get-top-users", (int top) =>
            {
                var repo = new UserRepository(dbConnection);
                var users = repo.GetTopUsers(top);
                return users;
            });

            // This one set to root for testing purposes
            app.MapGet("/", () =>
            {
                var repo = new UserRepository(dbConnection);
                var users = repo.GetTopUsers(20);
                return users;
            });

            // Get all roles associated with user
            app.MapGet("/user/roles", (int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var userRoles = repo.GetUserRole(userId);
                return userRoles;
            });

            // Add a role into role table; return role object
            app.MapPost("/user/add-role", (CreateRoleRequest roleRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var newRole = repo.CreateUserRole(roleRequest.UserId, roleRequest.RoleId);
                return newRole;
            });

            // Create a user using request model; return user object
            app.MapPost("/create-user", (CreateUserRequest createUserRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var newUser = repo.CreateUser(createUserRequest.Username, createUserRequest.Password, createUserRequest.Email);
                return newUser;
            });

            app.MapPatch("/users/update-user", (UpdateUserRequest userRequest, int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var updatedUser = repo.UpdateUser(userRequest, userId);
                return updatedUser;
            });

            // Delete user with their id
            app.MapDelete("/users/delete-user", (int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                repo.DeleteUser(userId);
            });


        }
    }
}
