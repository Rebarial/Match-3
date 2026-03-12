using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Match_3.GameObject;
using Match_3.Tools;

namespace Match_3;

public partial class Game : Window
{
    private bool gameStarted;
    private DispatcherTimer gameTimer = new();

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

        var gameCanvas = new Canvas();
        gameCanvas.Background = Brushes.LightGray;
        gameCanvas.Width = 400;
        gameCanvas.Height = 400;

        GameConfig.CellSize = 60;
        GameConfig.BoardSize = 8;
        GameArea.Children.Add(GameObjectFactory.GenerateDesk());
        Grid.SetZIndex(CenterStackPanel, 10);
    }


    private void StartGame()
    {
        gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(GameConfig.GameLoopIntervalMs)
        };

        gameTimer.Tick += Update;
        gameTimer.Start();
    }

    private int tickCounter;
    private int timeOfGameLeft = 60;

    public static int Score = 0;

    private void Update(object sender, EventArgs e)
    {
        if (!gameStarted)
        {
            if (tickCounter % GameConfig.StartTimerSpeed == 0 && tickCounter != 0)
            {
                GameConfig.TimerUntilStart--;
                if (GameConfig.TimerUntilStart >= 0)
                {
                    BigTimerToStart.FontSize = 20;
                    var labelText = $"{GameConfig.TimerUntilStart}!";
                    if (GameConfig.TimerUntilStart == 0) labelText = "Go!";
                    BigTimerToStart.Content = labelText;
                }
                else
                {
                    CenterStackPanel.Children.Remove(BigTimerToStart);
                    BigTimerToStart = null;
                    gameStarted = true;
                }
            }
            else
            {
                var fontSizeUpSpeed = 40 / GameConfig.StartTimerSpeed;
                BigTimerToStart.FontSize = BigTimerToStart.FontSize + fontSizeUpSpeed;
            }
        }
        else
        {
            if (tickCounter % MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) == 0)
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

            GameObjectFactory.DeskController(tickCounter);
        }

        tickCounter++;
        if (tickCounter >= GameConfig.TicksLimit) tickCounter = 1;
    }

    private void ShowGameOver()
    {
        MessageBox.Show($"Game Over!\n\nВаш счёт: {Score}",
            "Game Over",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        var mainWindow = new MainWindow();
        mainWindow.Show();

        Close();
    }
}