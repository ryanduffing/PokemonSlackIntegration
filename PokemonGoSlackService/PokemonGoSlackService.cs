using Microsoft.Owin.Hosting;
using PokemonGoSlackService.Services;
using PokemonGoSlackService.Services.Interfaces;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace PokemonGoSlackService
{
    public partial class PokemonGoSlackService : ServiceBase
    {
        private IDisposable Server { get; set; }

        private IMapService MapService { get; set; }

        public PokemonGoSlackService(IMapService mapService)
        {
            this.MapService = mapService;

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.Server = WebApp.Start<Startup>(url: Properties.Settings.Default.ServiceApiAddress);

            // do a quick call to start up the authentication
            Task.Run(async () =>
            {
                await AccountService.Instance.PtcLogin();

                while (true)
                {
                    await this.MapService.GetNearbyPokemon();
                    await this.MapService.GetLureActivity();

                    // looks for new pokemon every three minutes
                    await Task.Delay(180000);
                }
            });
        }

        protected override void OnStop()
        {
            if (this.Server != null)
            {
                this.Server.Dispose();
            }

            base.OnStop();
        }

        public void Start()
        {
            OnStart(new string[0]);
        }
    }
}
