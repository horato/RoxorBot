using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogger _logger = new Logger();

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
            catch (Exception ea)
            {
                _logger.Log(ea.ToString());
            }
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            _logger.Log("A first chance exception was thrown", LogType.Exception);
            _logger.Log(e.Exception.Message, LogType.Exception);
            _logger.Log(e.ToString(), LogType.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Log("An unhandled exception was thrown", LogType.UnhandledException);
            var ex = (Exception)e.ExceptionObject;
            _logger.Log(ex.Message, LogType.UnhandledException);
            _logger.Log(ex.ToString(), LogType.UnhandledException);
        }
    }
}
