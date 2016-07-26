using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PokemonGoSlackService
{
    [RunInstaller(true)]
    public class PokemonGoSlackServiceInstaller : Installer
    {
        public PokemonGoSlackServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = "Pokemon Go Slack Service";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = "Pokemon Go Slack Service";
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
