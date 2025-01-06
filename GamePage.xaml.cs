using System.ComponentModel;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Web;
using System.Timers;
namespace LukeDeaneTrivia;
//dotnet build -t:Run -f net8.0-maccatalyst



public partial class GamePage : ContentPage, INotifyPropertyChanged
{
    private System.Timers.Timer timer;
    private string players;
    public string Players
    {
        get => players;
        set
        {
            players = value;
            OnPropertyChanged(nameof(Players));
        }
    }

    private string currPlayer;
    public string CurrPlayer
    {
        get => currPlayer;
        set
        {
            currPlayer = value;
            OnPropertyChanged(nameof(CurrPlayer));
        }
    }

    private ObservableCollection<Question> _questions;
    public ObservableCollection<Question> Questions { get => _questions; }
    private int currentQuestionIndex;
    List<Question> questionList;
    HttpClient httpClient;
    private int score;
    //implement dictionary with names and scores with saving to preferences
    Dictionary<string, int> playersScore = new Dictionary<string, int>();
    public List<string> names;
    private int currentPlayerIndex;


    private string topic;
    public string Topic
    {
        get => topic;
        set
        {
            topic = value;
            OnPropertyChanged(nameof(Topic));
        }
    }

    public List<string> Answers
    {
        get
        {
            if (CurrentQuestion == null) return new List<string>();
            List<string> allAnswers = new List<string>(CurrentQuestion.incorrect_answers) { CurrentQuestion.correct_answer };
            return allAnswers;
        }
    }

    private Question currentQuestion;
    public Question CurrentQuestion
    {
        get => currentQuestion;
        set
        {
            currentQuestion = value;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(Answers));
            
        }
    }


    private string selectedAnswer;
    public string SelectedAnswer
    {
        get => selectedAnswer;
        set
        {
            selectedAnswer = value;
            OnPropertyChanged(nameof(SelectedAnswer));
        }
    }


    private int numOfQuestions;
    public int NumOfQuestions
    {
        get => numOfQuestions;
        set
        {
            numOfQuestions = value;
            OnPropertyChanged(nameof(NumOfQuestions));
        }
    }

    private bool isBusy;
    public bool IsBusy
    {
        get
        {
            return isBusy;
        }
        set
        {
            if (isBusy != value)
            {
                isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }
    public bool IsNotBusy => !IsBusy;

    private string difficulty;
    public string Difficulty
    {
        get => difficulty;
        set
        {
            difficulty = value;
            OnPropertyChanged(nameof(Difficulty));
        }
    }


    private int time;
    public int Time
    {
        get => time;
        set
        {
            time = value;
            OnPropertyChanged(nameof(Time));
        }

    }
    private int showTime;
    public int ShowTime
    {
        get => showTime;
        set
        {
            showTime = value;
            OnPropertyChanged(nameof(ShowTime));
        }
    }

	public GamePage(string players, int numOfQuestions, string topic, string difficulty,int time)
	{
		InitializeComponent();
        Players = players;
        Difficulty = difficulty;
        NumOfQuestions = numOfQuestions;
        Topic = topic;
        Time = time;
        httpClient = new HttpClient();
        questionList = new List<Question>();
        _questions = new ObservableCollection<Question>();
        currentQuestionIndex = 0;
        currentPlayerIndex = 0;
        BindingContext = this;
        names = new List<string>();
        ShowTime = Time;
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += Timer_Elapsed;
        
	}

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (showTime == 0)
        {
            StopTimer();
            NextQuestion();

        } else
        {

            showTime--;
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ShowTime = showTime; // Set the updated time to your property
        });

    }

    private void StartTimer()
    {
        timer.Start();
    }

    private void StopTimer()
    {
        timer.Stop();
    }


    //Method to get users name
    private async Task CollectPlayerNames()
    {
        Int32.TryParse(Players, out int num);
        try
        {

        
        names.Clear(); // clear any existing names
        for (int i = 0; i < num; i++)
        {
            string name = await DisplayPromptAsync("Player Name", $"Enter the name of player {i + 1}:", "OK");
            if (playersScore.ContainsKey(name))
            {
                await DisplayAlert("Error", "Player names must be unique.", "OK");
                i--; // ask the same player again
                continue;
            }
            
            playersScore.Add(name, 0);
            names.Add(name);
        }
        CurrPlayer = names[0]; // Set the first player as the current player
        }
        catch (Exception e)
        {
            await DisplayAlert("Error", e.Message, "Ok");
            await Shell.Current.GoToAsync(".."); // Navigate back to the mainpage

        }
    }





    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (names.Count == 0)
        {

        await CollectPlayerNames(); 
        }
        
        await MakeCollection();
        StartTimer();
    }


    // questions from API
    public async Task GetQuestions(string difficulty, int numOfQuestions, string topic)
    {
        int topicNumber;
        if (SettingsPage.TopicToNumber.TryGetValue(topic, out int numberOfTopic))
        {
            topicNumber = numberOfTopic;
        } else
        {
            topicNumber = 9; // Default to general knowledge if no valid topics
        }

        if (questionList.Count > 0) return;

        string url = $"https://opentdb.com/api.php?amount={numOfQuestions}&category={topicNumber}&difficulty={difficulty.ToLower()}";

        try
        {
            //Console.WriteLine($"Making API call to: {url}");

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string contents = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("API Response: " + contents);  

                var questionResponse = JsonSerializer.Deserialize<QuestionResponse>(contents);
                //Console.WriteLine("QUESTION RESPONSE RESULTS: " + questionResponse?.Results);
                //Console.WriteLine("Deserialized API Response: " + JsonSerializer.Serialize(questionResponse));  // Check the deserialized object

                if (questionResponse?.results != null)
                {
                    questionList = questionResponse.results;
                    //Console.WriteLine($"Fetched {questionList.Count} questions.");
                }
                else
                {
                    //Console.WriteLine("No results found in API response.");
                }
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error loading questions: {ex.Message}");
            await Shell.Current.DisplayAlert("Error loading questions", ex.Message, "OK");
        }
    }


    //Making a questions list from data 
    public async Task MakeCollection()
    {
        if (isBusy)
            return;

        IsBusy = true;

        try
        {
            //Console.WriteLine("Calling GetQuestions...");  // Debug log
            await GetQuestions(Difficulty, NumOfQuestions, Topic);

            if (questionList.Count == 0)
            {
                //Console.WriteLine("No questions loaded!");  // Debug log
                return;
            }

            _questions.Clear();
            //Formatting question because you get those html things
            foreach (var question in questionList)
            {
                question.question = HttpUtility.HtmlDecode(question.question);
                question.correct_answer = HttpUtility.HtmlDecode(question.correct_answer);
                for (int i = 0; i < question.incorrect_answers.Count; i++)
                {
                    question.incorrect_answers[i] = HttpUtility.HtmlDecode(question.incorrect_answers[i]);
                }
                _questions.Add(question);
            }
            Console.WriteLine($"Loaded {questionList.Count} questions.");  // Debug log

            NextQuestion();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MakeCollection: {ex.Message}");  // Debug log
            await Shell.Current.DisplayAlert("Error in loading questions", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    //Moving to the next question and switching between players
    public void NextQuestion()
    {
        
        if (currentQuestionIndex < questionList.Count)
        {
            showTime = time;
            StartTimer();
            CurrentQuestion = questionList[currentQuestionIndex];
            currentQuestionIndex++;
            CurrPlayer = names[currentPlayerIndex];
            currentPlayerIndex = (currentPlayerIndex + 1) % names.Count;
        } else
        {
            QuestionLabel.Text = "Game Over!";
            GameFinished();
            StopTimer();
        }
    }

    
    //Answering question
    async void AnswBtn_Clicked(System.Object sender, System.EventArgs e)
    {
        var button = (Button)sender;
        var answer = button.Text;
        var correctAnsw = CurrentQuestion.correct_answer;
        if (answer.Equals(correctAnsw))
        {
            button.BackgroundColor = Colors.Green;
            button.Text = "😎";
           
            if (playersScore.ContainsKey(CurrPlayer))
            {
                playersScore[CurrPlayer]++;
                Console.WriteLine("PLAYER SCORE: " + playersScore[CurrPlayer]);
            }
        }
        else
        {
            button.BackgroundColor = Colors.Red;
            button.Text = "😢";
        }
        await Task.Delay(500);
        NextQuestion();
        button.BackgroundColor = (Color)Application.Current.Resources["TextColor"]; // Reset to the default color
        button.Text = answer;
    }


    
    void GameFinished()
    {
        foreach (var pop in playersScore)
        {
            Console.WriteLine($"SCORE: {pop.Key} - {pop.Value}");
        }

        QuestionsView.IsVisible = false;
        CurrPlayerStack.IsVisible = false;

        

        var resultsCollectionView = new CollectionView
        {
            ItemsSource = playersScore.ToList(),
            ItemTemplate = new DataTemplate(() =>
            {
                var frame = new Frame
                {
                    BackgroundColor = (Color)Application.Current.Resources["TextColor"],
                    Padding = 10,
                    CornerRadius = 10,
                    Margin = 5

                };
                var grid = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
                    RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
                    Padding = 10
                };

                var keyLabel = new Label
                {
                    FontSize = 24,
                    TextColor = (Color)Application.Current.Resources["BackgroundColor"],
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };
                keyLabel.SetBinding(Label.TextProperty, "Key");

                var valueLabel = new Label
                {
                    FontSize = 24,
                    TextColor = (Color)Application.Current.Resources["BackgroundColor"],
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                valueLabel.SetBinding(Label.TextProperty, "Value");


                grid.Children.Add(keyLabel);
                Grid.SetColumn(keyLabel, 0);
                Grid.SetRow(keyLabel, 0);

                grid.Children.Add(valueLabel);
                Grid.SetColumn(valueLabel, 1);
                Grid.SetRow(valueLabel, 0);

                frame.Content = grid;

                return frame;
            })
        };

        GeneralLayout.Children.Add(resultsCollectionView);
        //GeneralLayout.Children.Add(resultListView);
        SaveResult(playersScore); // Save the results to preferences
    }

    async void SaveResult(Dictionary<string,int> results) 
    {
        try
        {
            //Check if there are already some results saved
            if (Preferences.Get("results", "") != "")
            {
                var stringResults = Preferences.Get("results", "");
                Dictionary<string, int> playersResults = JsonSerializer.Deserialize<Dictionary<string, int>>(stringResults);
                //Add new results and then save 
                foreach (var result in results)
                {
                    if (!playersResults.ContainsKey(result.Key))
                    {
                        playersResults.Add(result.Key, result.Value);
                    }
                }
                var jsonResults = JsonSerializer.Serialize(playersResults);
                Preferences.Set("results", jsonResults);
            } else
            //Just save the results
            {
                var json = JsonSerializer.Serialize(results);
                Preferences.Set("results", json);
            }
            //await DisplayAlert("Saving", "Results have been saved", "Ok");
        }
        catch (Exception e)
        {
            await DisplayAlert("Saving", $"{e.Message}", "Ok");
        }
    }
}
