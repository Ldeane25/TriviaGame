using LukeDeaneTrivia.Resources.Styles;

namespace LukeDeaneTrivia;

public partial class App : Application
{
	
	public App()
	{
		InitializeComponent();
        bool isDarkTheme = Preferences.Get("isDarkTheme", false);

        if (isDarkTheme)
        {
            Current.Resources.MergedDictionaries.Add(new DarkTheme());
        }
        else
        {
            Current.Resources.MergedDictionaries.Add(new LightTheme());
        }
        MainPage = new AppShell();
	}
}

