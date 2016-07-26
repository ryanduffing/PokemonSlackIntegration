using System.Collections.Generic;

namespace PokemonGoSlackService.Models
{
    public static class PokemonStorage
    {
        private static List<NearbyPokemon> _nearbyPokemon;

        public static List<NearbyPokemon> PokemonNearby
        {
            get
            {
                if (_nearbyPokemon == null)
                {
                    _nearbyPokemon = new List<NearbyPokemon>();
                }

                return _nearbyPokemon;
            }
            set
            {
                _nearbyPokemon = value;
            }
        }
    }
}
