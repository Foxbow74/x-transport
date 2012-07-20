using System.IO;
using System.Windows;
using AlphaXServer;
using AvalonDock;

namespace AlfaStudio
{
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs _e)
		{
		    InitInternalServer();
            base.OnStartup(_e);

			ThemeFactory.ChangeTheme("luna.normalcolor");

            var main = new MainWindow();
			main.Show();
		}

        void InitInternalServer()
        {
			if (!File.Exists(Server.DB_NAME))
			{
				Uploader.Upload(Server.DB_NAME);
			}
        	new Server().Reset();
        }

	}
}