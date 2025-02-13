using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaskForge.Models;
using TaskForge.Repositories;

namespace TaskForge.Endpoints
{
    public static class CrewEndpoints
    {
        public static void MapCrewEndpoints(this WebApplication app)
        {
            string dbConnection = app.Configuration.GetConnectionString("DbConnection")!;

            app.MapGet("crews/get-crew", async (int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                var crew = await repo.GetCrewAsync(crewId);
                return crew != null ? Results.Ok(crew) : Results.NotFound($"Failed to get crew with ID: {crewId}");
            });

            app.MapGet("crews/all-crews", async () =>
            {
                var repo = new CrewRepository(dbConnection);
                var crews = await repo.GetAllCrewsAsync();
                return crews != null && crews.Any() ? Results.Ok(crews) : Results.NotFound("No crews found.");
            });

            app.MapPost("crews/create-crew", async (string name) =>
            {
                var repo = new CrewRepository(dbConnection);
                var newCrew = await repo.CreateCrewAsync(name);
                return newCrew != null ? Results.Ok(newCrew) : Results.BadRequest("Failed to create crew.");
            });

            app.MapPatch("crews/update-crew", async (UpdateCrewRequest updateCrewRequest, int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                var updatedCrew = await repo.UpdateCrewAsync(updateCrewRequest, crewId);
                return updatedCrew != null ? Results.Ok(updatedCrew) : Results.NotFound($"Crew with ID {crewId} not found.");
            });

            app.MapDelete("crews/delete-crew", async (int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                var deleted = await repo.DeleteCrewAsync(crewId);
                return deleted ? Results.NoContent() : Results.NotFound($"Failed to delete crew with ID: {crewId}");
            });
        }
    }

}
