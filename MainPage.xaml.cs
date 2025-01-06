using System.Windows.Input;

namespace LukeDeaneTrivia;

public partial class MainPage : ContentPage
{
    


    public MainPage()
    {
        InitializeComponent();
        BindingContext = this; 
    }

    async void PlayBtn_Clicked(System.Object sender, System.EventArgs e)
    {
        var button = (Button)sender;
        var players = button.Text;
        
        var difficulty = Preferences.Get("difficulty", "medium");
        var topic = Preferences.Get("topic", "general");
        var numOfQuestions = Preferences.Get("numOfQuestions", 10);
        var time = Preferences.Get("time", 20);
        await Navigation.PushAsync(new GamePage(players,numOfQuestions,topic,difficulty,time));
        
    }
}
