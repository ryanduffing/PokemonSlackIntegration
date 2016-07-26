using GeoCoordinatePortable;

namespace PokemonGoSlackService.Services.Interfaces
{
    public interface IBearingService
    {
        string DegreeBearing(GeoCoordinate pointOne, GeoCoordinate pointTwo);
    }
}
