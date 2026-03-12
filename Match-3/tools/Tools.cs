using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace Match_3.Tools;

public static class MyMathHelper
{
    public static int OneSecTicks(int gameLoopIntervalMs)
    {
        const int msInSecond = 1000;
        if (gameLoopIntervalMs == 0) return msInSecond; 
        return msInSecond / gameLoopIntervalMs;
    }

    public static int TickAddition(int firstTick, int tickCount)
    {
        return (firstTick + tickCount) % GameConfig.TicksLimit;
    }
}

public static class GameHelper
{
    public static BitmapImage LoadImageFromResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
    
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null) return null;
        
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }

    public static int getRandomNumber()
    {
        Random random = new Random();
        return random.Next(1,6);
    }
    
    public static int[,] InitializeBoard(int size)
    {
        Random random = new Random();
        
        
        /*
        return new int[8,8]
        {
            { 1, 2, 1, 1, 5, 1, 2, 3 },
            { 2, 2, 1, 2, 1, 2, 3, 4 },
            { 2, 1, 4, 3, 2, 2, 4, 5 },
            { 1, 2, 3, 5, 3, 4, 5, 1 },
            { 2, 2, 3, 5, 4, 5, 1, 2 },
            { 1, 5, 1, 1, 5, 1, 2, 3 },
            { 2, 3, 4, 5, 1, 2, 3, 4 },
            { 3, 4, 5, 1, 2, 3, 4, 5 }
        };
        */
        
   
        int[,] board = new int[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                board[i, j] = random.Next(1,5);
            }
        }

        return board;
    }
}