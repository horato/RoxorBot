using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prism.Unity;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Implementations.Chat;
using RoxorBot.Data.Implementations.Providers;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Logic.Managers;
using RoxorBot.Modules.Main;
using RoxorBot.Modules.Output;
using RoxorBot.Modules.Toolbar;

namespace RoxorBot
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("cs-CZ");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("cs-CZ");

                base.InitializeShell();
                ViewModelLocationProvider.SetDefaultViewModelFactory(type =>
                {
                    var vmType = ViewModelProvider.CreateViewModel(type);
                    return Container.Resolve(vmType);
                });

                Application.Current.MainWindow = (Window)Shell;
                Application.Current.MainWindow.Show();
            }
            catch
            {
                // ignored
            }
        }

        protected override void ConfigureModuleCatalog()
        {
            var catalog = (ModuleCatalog)ModuleCatalog;
            catalog.AddModule(typeof(OutputModule));
            catalog.AddModule(typeof(MainModule));
            catalog.AddModule(typeof(ToolbarModule));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            RegisterTypeIfMissing(typeof(ILogger), typeof(Logger), true);
            RegisterTypeIfMissing(typeof(IRaffleManager), typeof(RaffleManager), true);
            RegisterTypeIfMissing(typeof(IChatManager), typeof(ChatManager), true);
            RegisterTypeIfMissing(typeof(IRewardTimerManager), typeof(RewardTimerManager), true);
            RegisterTypeIfMissing(typeof(IAutomatedMessagesManager), typeof(AutomatedMessagesManager), true);
            RegisterTypeIfMissing(typeof(IFilterManager), typeof(FilterManager), true);
            RegisterTypeIfMissing(typeof(IPointsManager), typeof(PointsManager), true);
            RegisterTypeIfMissing(typeof(IUserCommandsManager), typeof(UserCommandsManager), true);
            RegisterTypeIfMissing(typeof(IDatabaseManager), typeof(DatabaseManager), true);
            RegisterTypeIfMissing(typeof(IFollowersManager), typeof(FollowersManager), true);
            RegisterTypeIfMissing(typeof(IUsersManager), typeof(UsersManager), true);
            RegisterTypeIfMissing(typeof(IYoutubeManager), typeof(YoutubeManager), true);
            RegisterTypeIfMissing(typeof(IChatMessageHandler), typeof(ChatMessageHandler), true);
            RegisterTypeIfMissing(typeof(ITwitchLibTranslationService), typeof(TwitchLibTranslationService), false);
            RegisterTypeIfMissing(typeof(IChatHandlersProvider), typeof(ChatHandlersProvider), false);
        }
    }
}
