using Prism.Modularity;
using Prism.Regions;
using RoxorBot.Data.Constants;
using RoxorBot.Modules.Main.Views;

namespace RoxorBot.Modules.Main
{
    public class MainModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public MainModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(Regions.MainRegion, typeof(MainView));
        }
    }
}