using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Match_3.Tools;
using Match_3.GameObject;

namespace Match_3;


public partial class Game : Window
{
    bool gameStarted = false;
    private DispatcherTimer gameTimer = new DispatcherTimer();
    public Game()
    {
        InitializeComponent();
        InitGame();
        StartGame();
    }

    

    private void InitGame()
    {
        GameConfig.GameLoopIntervalMs = 25;
        GameConfig.TimerUntilStart = 3;
        GameConfig.StartTimerSpeed = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 2;

        GameConfig.TicksLimit = 100001;
        
        Canvas gameCanvas = new Canvas();
        gameCanvas.Background = Brushes.LightGray;
        gameCanvas.Width = 400;
        gameCanvas.Height = 400;
        
        GameConfig.CellSize = 60;
        GameConfig.BoardSize = 8;
        GameArea.Children.Add(GameObjectFactory.GenerateDesk());
        Grid.SetZIndex(CenterStackPanel, 10);
        //var go1 = GameObjectFactory.CreateGameObjectByIndexType(1, 20, 20);
        //CenterStackPanel.Children.Add(go1.GetElement());
    }
    
    
    private void StartGame()
    {
        gameTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(GameConfig.GameLoopIntervalMs) 
        };

        gameTimer.Tick += Update;
        gameTimer.Start();
    }

    private int tickCouner = 0;
    private int timeOfGameLeft = 60;
    
    public static int Score = 0;
    
    private void Update(object sender, EventArgs e)
    {
        if (!gameStarted)
        {
            if (tickCouner % GameConfig.StartTimerSpeed == 0 && tickCouner != 0)
            {
                GameConfig.TimerUntilStart--;
                if (GameConfig.TimerUntilStart >= 0)
                {
                    BigTimerToStart.FontSize = 20;
                    string labelText = $"{GameConfig.TimerUntilStart}!";
                    if (GameConfig.TimerUntilStart == 0) labelText = "Go!";
                    BigTimerToStart.Content = labelText;
                }
                else
                {
                    this.CenterStackPanel.Children.Remove(BigTimerToStart);
                    BigTimerToStart = null;
                    gameStarted = true;
                }

            }
            else
            {
                int fontSizeUpSpeed = 40 / GameConfig.StartTimerSpeed;
                BigTimerToStart.FontSize = BigTimerToStart.FontSize + fontSizeUpSpeed;
            }
            
        }
        else 
        {
            if (tickCouner % MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) == 0)
            {
                timeOfGameLeft--;
                TimerText.Text = timeOfGameLeft.ToString();
                if (timeOfGameLeft <= 0)
                {
                    gameTimer.Stop(); 
                    ShowGameOver(); 
                }
            }
            
            ScoreText.Text = Score.ToString();

            GameObjectFactory.DeskController(tickCouner);
        }
        tickCouner++;
        if (tickCouner >= GameConfig.TicksLimit) tickCouner = 1;
    }
    
    private void ShowGameOver()
    {
        MessageBox.Show($"Game Over!\n\nВаш счёт: {Score}", 
            "Game Over", 
            MessageBoxButton.OK, 
            MessageBoxImage.Information);
        
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        
        this.Close();
    }
}