using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Modularity;
using Prism.Regions;
using RoxorBot.Data.Constants;
using RoxorBot.Modules.Output.Views;

namespace RoxorBot.Modules.Output
{
    public class OutputModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public OutputModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(Regions.Output, typeof(OutputView));
        }
    }
}
