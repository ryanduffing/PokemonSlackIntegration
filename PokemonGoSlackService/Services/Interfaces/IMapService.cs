using System.Threading.Tasks;

namespace PokemonGoSlackService.Services.Interfaces
{
    public interface IMapService
    {
        Task GetLureActivity();

        Task GetNearbyPokemon();
    }
}
