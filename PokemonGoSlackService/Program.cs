using PokemonGoSlackService.Services;
using PokemonGoSlackService.Services.Interfaces;
using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace PokemonGoSlackService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // manual dependency injection!
            IBearingService bearingService = new BearingService();
            IMapService mapService = new MapService(bearingService);

            #if (!DEBUG)
                if (System.Environment.UserInteractive)
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                }
                else
                {
                    ServiceBase[] ServicesToRun = new ServiceBase[]
                    {
                        new PokemonGoSlackService(mapService)
                    };

                    ServiceBase.Run(ServicesToRun);
                }
            #else
                PokemonGoSlackService service = new PokemonGoSlackService(mapService);
                service.Start();
            #endif
        }
    }
}
