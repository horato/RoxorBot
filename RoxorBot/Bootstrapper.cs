using System.Globalization;
using System.Threading;
using Prism.Unity;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Mvvm;
using RoxorBot.Data;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Implementations.Chat;
using RoxorBot.Data.Implementations.Database;
using RoxorBot.Data.Implementations.Factories.Entities;
using RoxorBot.Data.Implementations.Factories.Wrapper;
using RoxorBot.Data.Implementations.Providers;
using RoxorBot.Data.Implementations.Repositories;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Logic.Managers;
using RoxorBot.Modules.Main;
using RoxorBot.Modules.Output;
using RoxorBot.Modules.Toolbar;
using RoxorBot.Data.Interfaces.Repositories;

namespace RoxorBot
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            var sessionFactory = DatabaseConfigurator.Configure();
            Container.RegisterInstance(sessionFactory, new ContainerControlledLifetimeManager());

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
            
            RegisterTypeIfMissing(typeof(IChatMessageHandler), typeof(ChatMessageHandler), true);
            RegisterTypeIfMissing(typeof(ITwitchLibTranslationService), typeof(TwitchLibTranslationService), false);
            RegisterTypeIfMissing(typeof(IChatHandlersProvider), typeof(ChatHandlersProvider), false);
            RegisterTypeIfMissing(typeof(IDialogService), typeof(DialogService), false);

            RegisterManagers();
            RegisterRepositories();
            RegisterProviders();
            RegisterFactories();
        }


        private void RegisterManagers()
        {
            RegisterTypeIfMissing(typeof(IRaffleManager), typeof(RaffleManager), true);
            RegisterTypeIfMissing(typeof(IChatManager), typeof(ChatManager), true);
            RegisterTypeIfMissing(typeof(IRewardTimerManager), typeof(RewardTimerManager), true);
            RegisterTypeIfMissing(typeof(IAutomatedMessagesManager), typeof(AutomatedMessagesManager), true);
            RegisterTypeIfMissing(typeof(IFilterManager), typeof(FilterManager), true);
            RegisterTypeIfMissing(typeof(IPointsManager), typeof(PointsManager), true);
            RegisterTypeIfMissing(typeof(IUserCommandsManager), typeof(UserCommandsManager), true);
            RegisterTypeIfMissing(typeof(IFollowersManager), typeof(FollowersManager), true);
            RegisterTypeIfMissing(typeof(IUsersManager), typeof(UsersManager), true);
            RegisterTypeIfMissing(typeof(IYoutubeManager), typeof(YoutubeManager), true);
        }

        private void RegisterRepositories()
        {
            RegisterTypeIfMissing(typeof(IAutomatedMessagesRepository), typeof(AutomatedMessagesRepository), false);
            RegisterTypeIfMissing(typeof(IFilterRepository), typeof(FilterRepository), false);
            RegisterTypeIfMissing(typeof(IUsersRepository), typeof(UsersRepository), false);
            RegisterTypeIfMissing(typeof(IUserCommandsRepository), typeof(UserCommandsRepository), false);
        }

        private void RegisterProviders()
        {
            RegisterTypeIfMissing(typeof(IAutomatedMessagesProvider), typeof(AutomatedMessagesProvider), false);
            RegisterTypeIfMissing(typeof(IFilterProvider), typeof(FilterProvider), false);
            RegisterTypeIfMissing(typeof(IUsersProvider), typeof(UsersProvider), false);
            RegisterTypeIfMissing(typeof(IUserCommandsProvider), typeof(UserCommandsProvider), false);
        }

        private void RegisterFactories()
        {
            RegisterTypeIfMissing(typeof(IUserFactory), typeof(UserFactory), false);
            RegisterTypeIfMissing(typeof(IUserCommandFactory), typeof(UserCommandFactory), false);

            RegisterTypeIfMissing(typeof(IAutomatedMessageWrapperFactory), typeof(AutomatedMessageWrapperFactory), false);
            RegisterTypeIfMissing(typeof(IFilterWrapperFactory), typeof(FilterWrapperFactory), false);
            RegisterTypeIfMissing(typeof(IUserWrapperFactory), typeof(UserWrapperFactory), false);
            RegisterTypeIfMissing(typeof(IUserCommandWrapperFactory), typeof(UserCommandWrapperFactory), false);
        }
    }
}
