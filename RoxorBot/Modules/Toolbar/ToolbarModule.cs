using Prism.Modularity;
using Prism.Regions;
using RoxorBot.Data.Constants;
using RoxorBot.Modules.Toolbar.Views;

namespace RoxorBot.Modules.Toolbar
{
    public class ToolbarModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ToolbarModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(Regions.Output, typeof(ToolbarView));
        }
    }
}
