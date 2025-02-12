using Microsoft.AspNetCore.Http.HttpResults;
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
            app.MapGet("/get-user", async (int userId) =>
            {
                try
                {
                    var repo = new UserRepository(dbConnection);
                    var user = await repo.GetUserAsync(userId);

                    if (user == null) { return Results.NotFound($"User with ID: {userId} not found."); }

                    return Results.Ok(user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user {userId}: {ex.Message}");
                    return Results.Problem("An error occured while retrieving the user.");
                }
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
            app.MapGet("/user/roles", async (int userId) =>
            {
                try
                {
                    var repo = new UserRepository(dbConnection);
                    var roles = await repo.GetUserRolesAsync(userId);

                    if (roles.Count == 0)
                        return Results.NotFound($"No roles found for user with ID {userId}");

                    return Results.Ok(roles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user roles: {ex.Message}");
                    return Results.Problem("An error occurred while retrieving user roles.");
                }
            });

            app.MapPost("/login", async (LoginRequest loginRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var user = await repo.LoginAsync(loginRequest);
                return user;
            });

            // Add a role into role table; return role object
            app.MapPost("/user/add-role", async (CreateRoleRequest roleRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var newRole = await repo.CreateUserRoleAsync(roleRequest.UserId, roleRequest.RoleId);
                return newRole;
            });

            // Create a user using request model; return user object
            app.MapPost("/create-user", async (CreateUserRequest createUserRequest) =>
            {                
                var repo = new UserRepository(dbConnection);
                var newUser = await repo.CreateUserAsync(createUserRequest.Username, createUserRequest.Password, createUserRequest.Email);
                return Results.Created($"/get-user?userId={newUser.Id}", newUser);
            });

            app.MapPatch("/users/update-user", async (UpdateUserRequest userRequest, int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var updatedUser = await repo.UpdateUserAsync(userRequest, userId);

                if (updatedUser == null)
                {
                    return Results.NotFound(new { message = $"User with ID {userId} not found."});
                }
                return Results.Ok(updatedUser);
            });

            // Delete user with their id
            app.MapDelete("/users/delete-user", async (int id) =>
            {
                var repo = new UserRepository(dbConnection);
                bool deleted = await repo.DeleteUserAsync(id);

                if (!deleted)
                {
                    return Results.NotFound(new { message = $"User with ID {id} not found." });
                }

                return Results.NoContent(); // 204 No Content for successful deletion
            });


        }
    }
}
