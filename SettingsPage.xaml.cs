using LukeDeaneTrivia.Resources.Styles;
using Plugin.Maui.Audio;
using System.ComponentModel;
using Microsoft.Maui.Controls;
namespace LukeDeaneTrivia;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged
{
    
    private string topic;
    private string difficulty;
    private int numOfQuestions;
    private int time;

    public int Time
    {
        get => time;
        set
        {
            
            if (value < 5 || value > 60)
            {
                time = 10;
                Console.WriteLine("Time value out of range, setting to 10");
                //for some reason it always sets time to 10
            }
            else
            {
                time = value;
                Console.WriteLine(value);

            }

            Preferences.Set("time", time);
            OnPropertyChanged(nameof(Time));
        }
    }

    public static Dictionary<string, int> TopicToNumber = new()
    {
        { "General", 9},
        {"Computers", 18 },
        {"Books", 10 },
        {"Film", 11 },
        {"Music", 12 },
        {"Video Games", 15 },
        {"Sports", 21 },
    };

    public string Topic
    {
        get => topic;
        set
        {
           
            topic = value;
            OnPropertyChanged(nameof(Topic));
            Preferences.Set("topic", topic);
        }
    }

    public string Difficulty
    {
        get => difficulty;
        set
        {
            difficulty = value;
            OnPropertyChanged(nameof(Difficulty));
            Preferences.Set("difficulty", difficulty);
        }
    }

    public int NumOfQuestions { get => numOfQuestions; set 
            {
            if (value < 1 || value > 20)
            {
                numOfQuestions = 10;
            } else
            {
                numOfQuestions = value;
            }
            OnPropertyChanged(nameof(NumOfQuestions));
            Preferences.Set("numOfQuestions", numOfQuestions);
        } }
    public bool IsDarkTheme { get; set; }
    private IAudioPlayer audioPlayer;


    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this;
        // Retrieve the saved theme preference and set the property
        IsDarkTheme = Preferences.Get("isDarkTheme", false); // Default to LightTheme
        Difficulty = Preferences.Get("difficulty", "Easy"); // Default to "Easy" if not set
        Topic = Preferences.Get("topic", "General"); // Default to "General" if not set
        NumOfQuestions = Preferences.Get("numOfQuestions", 10);
        // Apply the saved theme on page load
        ApplyTheme(IsDarkTheme);
        //For some reason does not work on Android but does on Windows and Mac
        InitializeAudioPlayer();
        
    }

    void DarkTheme_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        IsDarkTheme = e.Value; // Update the property from the Switch
        Preferences.Set("isDarkTheme", IsDarkTheme); // Save the updated preference

        // Apply the theme based on the updated toggle state
        ApplyTheme(IsDarkTheme);
    }

    

    private async void InitializeAudioPlayer()
    {
        try
        {
            // Load the audio file from the Resources folder
            audioPlayer = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("music.mp3"));
            audioPlayer.Loop = true; //Add loop for music
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load music: {ex.Message}", "OK");
        }
    }

    private void ApplyTheme(bool isDark)
    {
        //Clearing dictionary and adding theme based on argument
        Application.Current.Resources.MergedDictionaries.Clear();
        if (isDark)
        {
            Application.Current.Resources.MergedDictionaries.Add(new DarkTheme());
        }
        else
        {
            Application.Current.Resources.MergedDictionaries.Add(new LightTheme());
        }
    }

    void Music_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        if (audioPlayer == null)
            return;

        if (e.Value)
        {
            // Play the music when the switch is turned on
            audioPlayer.Play();
        }
        else
        {
            // Stop the music when the switch is turned off
            audioPlayer.Stop();

        }
    }

    
}
