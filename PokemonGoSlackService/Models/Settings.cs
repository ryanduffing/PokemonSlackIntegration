using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;

namespace PokemonGoSlackService.Models
{
    public class Settings : ISettings
    {
        public AuthType AuthType => AuthType.Ptc;

        public string PtcUsername => Properties.Settings.Default.PtcUsername;

        public string PtcPassword => Properties.Settings.Default.PtcPassword;

        public double DefaultAltitude => 0;

        public double DefaultLatitude => Properties.Settings.Default.DefaultLatitude;

        public double DefaultLongitude => Properties.Settings.Default.DefaultLongitude;

        public string GoogleRefreshToken
        {
            get
            {
                return Properties.Settings.Default.GoogleRefreshToken;
            }
            set
            {
                Properties.Settings.Default.GoogleRefreshToken = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
