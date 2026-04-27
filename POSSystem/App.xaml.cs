using System.Windows;
using POSSystem.Data;

namespace POSSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database (creates pos.db file & tables on first run, seeds data)
            DatabaseHelper.Initialize();
        }
    }
}
