using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using lenticulis_gui.src.App;
using lenticulis_gui.src.SupportLib;

namespace lenticulis_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // TODO: load stored language
            if (!LangProvider.Initialize())
                Shutdown();

            // initialize support library - to create mappings for memory it needs, etc.
            SupportLib.initializeMagick();

            base.OnStartup(e);
        }
    }
}
