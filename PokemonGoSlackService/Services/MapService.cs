using md = PokemonGoSlackService.Models;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using PokemonGoSlackService.Services.Interfaces;
using System.Text;
using PokemonGoSlackService.Helpers;
using System.Linq;
using POGOProtos.Map;
using POGOProtos.Map.Fort;
using System.Collections.Generic;
using POGOProtos.Map.Pokemon;
using GeoCoordinatePortable;
using PokemonGo.RocketAPI.Exceptions;
using NLog;

namespace PokemonGoSlackService.Services
{
    public class MapService : IMapService
    {
        private IBearingService BearingService { get; set; }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MapService(IBearingService bearingService)
        {
            this.BearingService = bearingService;
        }

        public async Task GetLureActivity()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.PyramidSchemePokestopId))
                {
                    var mapObjects = await AccountService.Instance.Client.Map.GetMapObjects();

                    // now search for the pyramid scheme pokestop
                    MapCell pyramidSchemeCell = mapObjects.MapCells.FirstOrDefault(x => x.Forts.Any(t => t.Id == Properties.Settings.Default.PyramidSchemePokestopId));
                    FortData pyramidScheme = pyramidSchemeCell.Forts.FirstOrDefault(y => y.Id == Properties.Settings.Default.PyramidSchemePokestopId);

                    // there's a lure going
                    if (pyramidScheme.LureInfo != null)
                    {
                        // set up the lure information
                        if (md.CurrentLure.ExpirationTime == 0 || (GetCurrentTimeMilliseconds() > md.CurrentLure.ExpirationTime))
                        {
                            // set the lure expiration time to 30 minutes past the last modified timestamp
                            md.CurrentLure.ExpirationTime = pyramidScheme.LastModifiedTimestampMs + 1800000;

                            SendLureData();
                        }
                    }
                    else
                    {
                        // reset lure expiration time
                        md.CurrentLure.ExpirationTime = 0;
                    }
                }
            }
            catch(Exception ex) when (ex is AccessTokenExpiredException || ex is PtcOfflineException || ex is InvalidResponseException || ex is AccountNotVerifiedException)
            {
                // these exception are PokemonGo Exceptions
                // probably related to auth issue or server side issues

                // try logging in again
                await AccountService.Instance.PtcLogin();

                // now try this call again
                await GetLureActivity();
            }
            catch(Exception ex)
            {
                logger.Error(string.Format("RETRIEVING LURE DATA ON: {0}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));
                logger.Error(ex.Message);
                logger.Error(string.Empty);
                logger.Error(ex.StackTrace);
                logger.Error("---------------------------------------------------------------------");
                logger.Error(string.Empty);
                logger.Error(string.Empty);

                // maybe set something up for generic error handling, and for sending out an email as well
            }
        }

        public async Task GetNearbyPokemon()
        {
            try
            {
                if (Properties.Settings.Default.DefaultLatitude != 0 && Properties.Settings.Default.DefaultLongitude != 0)
                {
                    var mapObjects = await AccountService.Instance.Client.Map.GetMapObjects();
                    var ignoredPokemon = Properties.Settings.Default.IgnoredPokemon;

                    // extract all uncommon wild pokemon
                    var wildPokemon = mapObjects.MapCells.SelectMany(x => x.WildPokemons)
                        .Where(x => !ignoredPokemon.Contains(x.PokemonData.PokemonId.ToString().ToUpper()));

                    foreach (var pokemon in wildPokemon)
                    {
                        ProcessPokemon(pokemon);
                    }

                    // remove any pokemon that has expired to keep the list trimmed down
                    List<md.NearbyPokemon> updatedList = md.PokemonStorage.PokemonNearby.Where(x => GetCurrentTimeMilliseconds() <= x.ExpirationTime).ToList();
                    md.PokemonStorage.PokemonNearby = updatedList;
                }
            }
            catch (Exception ex) when (ex is AccessTokenExpiredException || ex is PtcOfflineException || ex is InvalidResponseException || ex is AccountNotVerifiedException)
            {
                // these exception are PokemonGo Exceptions
                // probably related to auth issue or server side issues

                // try logging in again
                await AccountService.Instance.PtcLogin();

                // now try this call again
                await GetNearbyPokemon();
            }
            catch(Exception ex)
            {
                logger.Error(string.Format("RETRIEVING NEARBY POKEMON ON: {0}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));
                logger.Error(ex.Message);
                logger.Error(string.Empty);
                logger.Error(ex.StackTrace);
                logger.Error("---------------------------------------------------------------------");
                logger.Error(string.Empty);
                logger.Error(string.Empty);

                // maybe set something up for generic error handling, and for sending out an email as well
            }
        }

        private DateTime ConvertToEasternTime(DateTime timeToConvert)
        {
            // set date time to eastern
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeToConvert, easternZone);

            return easternTime;
        }

        private double GetCurrentTimeMilliseconds()
        {
            return (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private string GetShortenedGoogleUrl(string url)
        {
            string googleResponse = WebRequestHelper.WebRequestCall(
                string.Format(Properties.Settings.Default.GoogleShortenerUrl, Properties.Settings.Default.GoogleShortenerKey),
                "{\"longUrl\": \"" + url + "\"}");


            return JsonConvert.DeserializeObject<md.GoogleShortenerResponse>(googleResponse).id;
        }

        private void ProcessPokemon(WildPokemon pokemon)
        {
            // prevents duplicates from being added to the list
            if (!md.PokemonStorage.PokemonNearby.Any(x => x.EncounterId == pokemon.EncounterId))
            {
                GeoCoordinate defaultCoordinates = new GeoCoordinate(AccountService.Instance.Settings.DefaultLatitude, AccountService.Instance.Settings.DefaultLongitude);
                GeoCoordinate pokemonCoordinates = new GeoCoordinate(pokemon.Latitude, pokemon.Longitude);
                int timeToDespawn = TimeSpan.FromMilliseconds(pokemon.TimeTillHiddenMs).Minutes;

                if (timeToDespawn > 0)
                {
                    var nearbyPokemon = new md.NearbyPokemon
                    {
                        DegreeBearing = this.BearingService.DegreeBearing(defaultCoordinates, pokemonCoordinates),
                        DistanceInFeet = Convert.ToInt32(defaultCoordinates.GetDistanceTo(pokemonCoordinates) * 3.28084),
                        EncounterId = pokemon.EncounterId,
                        ExpirationTime = pokemon.TimeTillHiddenMs + GetCurrentTimeMilliseconds(),
                        GoogleLink = GetShortenedGoogleUrl(string.Format(Properties.Settings.Default.GoogleLocationUrl, pokemonCoordinates.Latitude, pokemonCoordinates.Longitude)),
                        Name = pokemon.PokemonData.PokemonId.ToString(),
                        TimeToDespawn = timeToDespawn.ToString()
                    };

                    // add it to list for record keeping
                    md.PokemonStorage.PokemonNearby.Add(nearbyPokemon);

                    // this is a new pokemon, so alert slack of its presence
                    SendPokemonData(nearbyPokemon);
                }
            }
        }

        private void SendLureData()
        {
            TimeSpan expirationTime = TimeSpan.FromMilliseconds(md.CurrentLure.ExpirationTime);
            DateTime expirationDate = ConvertToEasternTime(new DateTime(1970, 1, 1) + expirationTime);

            string lureJson = string.Format("{{\"text\": \":pokemongo-luremodule: Lure Activated at Pyramid Scheme! Expires at: {0}\"}}", expirationDate.ToString("h:mm"));

            WebRequestHelper.WebRequestCall(Properties.Settings.Default.IncomingSlackHook, lureJson);
        }

        private void SendPokemonData(md.NearbyPokemon pokemon)
        {
            StringBuilder pokeJsonBuilder = new StringBuilder();

            pokeJsonBuilder.Append("{{\"text\":\"There is a <http://pokemondb.net/pokedex/{0}|{1}> nearby!");
            pokeJsonBuilder.Append(" <{2}|({3}\' {4})>");
            pokeJsonBuilder.Append(" - despawns in {5} minutes. :pokemon-{6}:\"}}");

            string pokeJson = string.Format(
                pokeJsonBuilder.ToString(),
                pokemon.Name.ToLower(), 
                pokemon.Name.ToLower(), 
                pokemon.GoogleLink, 
                pokemon.DistanceInFeet, 
                pokemon.DegreeBearing,
                pokemon.TimeToDespawn, 
                pokemon.Name.ToLower()
            );

            WebRequestHelper.WebRequestCall(Properties.Settings.Default.IncomingSlackHook, pokeJson);
        }
    }
}
