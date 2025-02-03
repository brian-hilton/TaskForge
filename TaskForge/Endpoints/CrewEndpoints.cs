using System.Runtime.CompilerServices;

namespace TaskForge.Endpoints
{
    public static class CrewEndpoints
    {
        private static void MapCrewEndpoints(this WebApplication app)
        {
            string dbConnection = app.Configuration.GetConnectionString("DbConnection")!;
        }
    }
}
