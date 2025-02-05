using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskForge.UI.Services;

namespace TaskForge.UI.Pages
{
    public class IndexModel : PageModel
    {
        public List<(string,string)>Crews { get; set; } = new List<(string,string)> ();
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            var client = new TaskForgeClient(new HttpClient());
            var crews = await client.GetAllCrewsForUserAsync(9);

            foreach(var crewId in crews)
            {
                var crew = await client.GetCrewAsync(int.Parse(crewId));
                Crews.Add((crewId, crew.Name));
            }
        }
    }
}
