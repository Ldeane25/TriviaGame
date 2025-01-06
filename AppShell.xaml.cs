namespace LukeDeaneTrivia;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
		Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
		Routing.RegisterRoute(nameof(ResultsPage), typeof(ResultsPage));
    }
}

