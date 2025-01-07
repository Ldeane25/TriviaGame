using System.Text.Json;



namespace LukeDeaneTrivia;

public partial class ResultsPage : ContentPage
{
	public string playersScoreString ;
    public List<KeyValuePair<string,int>> PlayersScore { get; set; } 

    public ResultsPage()
	{
		InitializeComponent();
        playersScoreString = Preferences.Get("results", "");
        var playersScore = string.IsNullOrEmpty(playersScoreString) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(playersScoreString) ?? new Dictionary<string, int>();
        PlayersScore = playersScore.ToList();
		BindingContext = this;
    }


}
