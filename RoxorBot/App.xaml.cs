using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;
using RoxorBot.Logic.Logging;

namespace RoxorBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogger _logger = LoggerProvider.GetLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var culture = CultureInfo.GetCultureInfo("cs-CZ");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            try
            {
                base.OnStartup(e);
                var bs = new Bootstrapper();
                bs.Run();
            }
            catch
            {
                //
            }
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            _logger.Error("A first chance exception was thrown", e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("An unhandled exception was thrown", e.ExceptionObject as Exception);
        }
    }
}
