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
                var repo = new UserRepository(dbConnection);
                var user = await repo.GetUserAsync(userId);

                if (user == null)
                {
                    return Results.NotFound($"User with ID: {userId} not found.");
                }

                return Results.Ok(user);
            });

            // Get top users
            app.MapGet("/get-top-users", (int top) =>
            {
                var repo = new UserRepository(dbConnection);
                var users = repo.GetTopUsers(top);

                if (users == null || !users.Any())
                {
                    return Results.NotFound("No users found.");
                }

                return Results.Ok(users);
            });

            app.MapGet("/get-all-user-roles", async () =>
            {
                var repo = new UserRepository(dbConnection);
                var userRoles = await repo.GetAllUserRolesAsync();
                if (userRoles == null)
                {
                    return Results.NotFound("no user roles found.");
                }
                return Results.Ok(userRoles);
            });

            // This one set to root for testing purposes
            app.MapGet("/", () =>
            {
                var repo = new UserRepository(dbConnection);
                var users = repo.GetTopUsers(20);
                return users.Any() ? Results.Ok(users) : Results.NotFound("No users found.");
            });

            // Get all roles associated with user
            app.MapGet("/user/roles", async (int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var roles = await repo.GetUserRolesAsync(userId);

                if (roles.Count == 0)
                    return Results.NotFound($"No roles found for user with ID {userId}");

                return Results.Ok(roles);
            });

            // Get all Roles from the table so the frontend can match role id to the role name
            app.MapGet("/roles", async () =>
            {
                var repo = new UserRepository(dbConnection);
                var roles = await repo.GetAllRolesAsync();
                if (roles.Count == 0 || roles == null)
                {
                    return Results.NotFound("Could not acquire role data.");
                }

                else
                {
                    return Results.Ok(roles);
                }
            });

            // Login endpoint
            app.MapPost("/login", async (LoginRequest loginRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var user = await repo.LoginAsync(loginRequest);

                if (user == null)
                {
                    return Results.NotFound("Invalid login credentials.");
                }

                return Results.Ok(user);
            });

            // Add a role into role table; return role object
            app.MapPost("/user/add-role", async (CreateRoleRequest roleRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var newRole = await repo.CreateUserRoleAsync(roleRequest.UserId, roleRequest.RoleId);

                if (newRole == null)
                {
                    return Results.BadRequest("Failed to create role.");
                }

                return Results.Created($"/user/roles/{newRole.Id}", newRole);
            });

            // Create a user using request model; return user object
            app.MapPost("/create-user", async (CreateUserRequest createUserRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var newUser = await repo.CreateUserAsync(createUserRequest.Username, createUserRequest.Password, createUserRequest.Email);

                if (newUser == null)
                {
                    return Results.BadRequest("Failed to create user.");
                }

                return Results.Created($"/get-user?userId={newUser.Id}", newUser);
            });

            app.MapPost("/register-user", async (RegisterUserRequest registerUserRequest) =>
            {
                var repo = new UserRepository(dbConnection);
                var user = await repo.PostUserAndUserRole(registerUserRequest.Username, registerUserRequest.Password, registerUserRequest.Email, registerUserRequest.Role);

                if (user == null)
                {
                    return Results.BadRequest("Failed to create user.");
                }
                return Results.Created($"/get-user?userId={user.Id}", user);
            });

            app.MapPatch("/users/update-user", async (UpdateUserRequest userRequest, int userId) =>
            {
                var repo = new UserRepository(dbConnection);
                var updatedUser = await repo.UpdateUserAsync(userRequest, userId);

                if (updatedUser == null)
                {
                    return Results.NotFound(new { message = $"User with ID {userId} not found." });
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
