namespace PokemonGoSlackService.Models
{
    public class NearbyPokemon
    {
        public string DegreeBearing { get; set; }

        public int DistanceInFeet { get; set; }

        public ulong EncounterId { get; set; }

        public double ExpirationTime { get; set; }

        public string GoogleLink { get; set; }

        public string Name { get; set; }

        public string TimeToDespawn { get; set; }
    }
}
