using NLog;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGoSlackService.Models;
using System;
using System.Threading.Tasks;

namespace PokemonGoSlackService.Services
{
    public sealed class AccountService
    {
        private static readonly AccountService _instance = new AccountService();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Client _client;
        private static ISettings _clientSettings;

        private AccountService()
        {
            _clientSettings = new Settings();
            _client = new Client(_clientSettings);
        }

        public static AccountService Instance
        {
            get
            {
                return _instance;
            }
        }

        public Client Client
        {
            get
            {
                return _client;
            }
        }

        public ISettings Settings
        {
            get
            {
                return _clientSettings;
            }
        }

        public async Task PtcLogin()
        {
            try
            {
                await _client.Login.DoPtcLogin(_clientSettings.PtcUsername, _clientSettings.PtcPassword);
            }
            catch (Exception ex) when (ex is AccessTokenExpiredException || ex is PtcOfflineException || ex is InvalidResponseException || ex is AccountNotVerifiedException)
            {
                // an error occurred, let's try logging in again in ten seconds
                await Task.Delay(10000);

                await PtcLogin();
            }
            catch(Exception ex)
            {
                logger.Error(string.Format("LOGGING IN ON: {0}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));
                logger.Error(ex.Message);
                logger.Error(string.Empty);
                logger.Error(ex.StackTrace);
                logger.Error("---------------------------------------------------------------------");
                logger.Error(string.Empty);
                logger.Error(string.Empty);

                // maybe set something up for generic error handling, and for sending out an email as well
            }
        }
    }
}
