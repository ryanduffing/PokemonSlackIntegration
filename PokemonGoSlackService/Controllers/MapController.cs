using PokemonGoSlackService.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Http;

namespace PokemonGoSlackService.Controllers
{
    public class MapController : ApiController
    {
        private IMapService MapService { get; set; }

        public MapController(IMapService mapService)
        {
            this.MapService = mapService;
        }

        [HttpGet]
        public async Task GetLureActivity()
        {
            await this.MapService.GetLureActivity();
        }

        [HttpGet]
        public async Task GetNearbyPokemon()
        {
            await this.MapService.GetNearbyPokemon();
        }
    }
}
