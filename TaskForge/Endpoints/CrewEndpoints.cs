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

            app.MapGet("crews/get-crew", (int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                var crew = repo.GetCrew(crewId);
                return crew;
            });

            app.MapGet("crews/all-crews", () =>
            {
                var repo = new CrewRepository(dbConnection);
                var crews = repo.GetAllCrews();
                return crews;
            });

            app.MapPost("crews/create-crew", (string name) =>
            {
                var repo = new CrewRepository(dbConnection);
                var newCrew = repo.CreateCrew(name);
                return newCrew;
            });

            app.MapPatch("crews/update-crew", (UpdateCrewRequest updateCrewRequest, int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                var updatedCrew = repo.UpdateCrew(updateCrewRequest, crewId);
                return updatedCrew;
            });

            app.MapDelete("crews/delete-crew", (int crewId) =>
            {
                var repo = new CrewRepository(dbConnection);
                repo.DeleteCrew(crewId);
            });
        }


    }
}
