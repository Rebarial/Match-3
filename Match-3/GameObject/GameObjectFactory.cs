using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Match_3.Tools;

namespace Match_3.GameObject;

public class BonusData
{
    public GameObjectClass ElementToBonus { get; set; }
    public bool IsHorizontalBonus { get; set; }
    public bool IsVerticalBonus { get; set; }

    public bool IsVerticalLineDestroy { get; set; }

    public bool IsHorizontalLineDestroy { get; set; }
}

public static class GameObjectFactory
{
    private static Canvas gameCanvas;
    private static GameObjectClass[,] gameObjectBoard;
    private static int[,] intBoard; //Столбец, Строка
    private static List<GameObjectClass> dieGameObjects;
    private static List<(int x, int y, int size, int orientation_type, int element_type)> bonusGameObjects;

    public static Dictionary<string, int> OrientationTypeDict = new() { { "horizontal", 0 }, { "vertical", 1 } };

    private static int tickAnimationEnd;

    public static Canvas GenerateDesk()
    {
        State = GameState.Idle;
        dieGameObjects = new List<GameObjectClass>();
        bonusGameObjects = new List<(int x, int y, int size, int orientation_type, int element_type)>();

        var size = GameConfig.BoardSize;

        gameCanvas = new Canvas
        {
            Width = GameConfig.CellSize * GameConfig.BoardSize,
            Height = GameConfig.CellSize * GameConfig.BoardSize,
            Background = Brushes.DarkSlateGray
        };

        intBoard = GameHelper.InitializeBoard(size);

        gameObjectBoard = GenerateGameObjectByIndexArray(intBoard);

        CheckDestroy();
        if (!dieGameObjects.Any()) tickAnimationEnd = -1;

        return gameCanvas;
    }

    public enum GameState
    {
        Idle = 0,
        Destroying = 1,
        Dropping = 2,
        Swapping = 3,
        AfterSwap = 4,
        BombAnimating = 5,
        LineDetonation = 6
    }

    public static GameState State;

    public static void DeskController(int tickCounter)
    {
        if (State == GameState.Idle)
        {
            CheckDestroy();
            bonusGameObjects.Clear();
            if (dieGameObjects.Any()) StartDestroy(tickCounter);
            if (linesActive.Any()) StartLineDetonation(tickCounter);
            if (bombInGame) StartBomb(tickCounter);
        }

        if (State == GameState.Destroying) DestroyObject(tickCounter);
        if (State == GameState.Dropping) DropObjects(tickCounter);
        if (State == GameState.Swapping) SwapObjects(tickCounter);
        if (State == GameState.AfterSwap) StartDestroy(tickCounter);
        if (State == GameState.BombAnimating) BombAnimation(tickCounter);
        if (State == GameState.LineDetonation) LineDetionationAnimation(tickCounter);
    }

    private static void LineDetionationAnimation(int tickCounter)
    {
        if (tickCounter >= tickAnimationEnd)
        {
            foreach (var lineActive in linesActive)
                if (lineActive.GetOreintationType() == OrientationTypeDict["horizontal"])
                {
                    var y = lineActive.GetCoords().y;
                    GameObjectClass element;
                    for (var i = 0; i < GameConfig.BoardSize; i++)
                    {
                        element = gameObjectBoard[i, y];
                        if (!dieGameObjects.Contains(element)) dieGameObjects.Add(element);
                    }
                }
                else
                {
                    var x = lineActive.GetCoords().x;
                    GameObjectClass element;
                    for (var i = 0; i < GameConfig.BoardSize; i++)
                    {
                        element = gameObjectBoard[x, i];
                        if (!dieGameObjects.Contains(element)) dieGameObjects.Add(element);
                    }
                }

            linesActive.Clear();
            State = GameState.AfterSwap;
            tickAnimationEnd = -1;
        }
        else
        {
            var progress = 1.0 - (double)(tickAnimationEnd - tickCounter) / (tickAnimationEnd -
                                                                             (tickAnimationEnd -
                                                                              MyMathHelper.OneSecTicks(GameConfig
                                                                                  .GameLoopIntervalMs) / 2));

            foreach (var lineActive in linesActive)
            {
                var orientation = lineActive.GetOreintationType();
                var coords = lineActive.GetCoords();

                AnimateWave(orientation, coords, progress, lineActive);
            }
        }
    }


    private static void AnimateWave(int orientation, (int x, int y) coords, double progress, GameObjectClass lineActive)
    {
        var waveRadius = (int)(progress * GameConfig.BoardSize);
        var pulseScale = 1 + 0.2 * Math.Sin(progress * Math.PI * 4);
        var lineScale = 1 + progress * 0.5;
        var lineOpacity = 1 - progress * 0.3;

        // Анимация элементов в радиусе волны
        for (var i = 0; i < GameConfig.BoardSize; i++)
        {
            double distance;
            GameObjectClass element;

            if (orientation == OrientationTypeDict["horizontal"])
            {
                distance = Math.Abs(i - coords.x);
                element = gameObjectBoard[i, coords.y];
            }
            else // Vertical
            {
                distance = Math.Abs(i - coords.y);
                element = gameObjectBoard[coords.x, i];
            }

            if (element != null && element != lineActive && distance <= waveRadius)
            {
                element.GetElement().RenderTransform = new ScaleTransform(pulseScale, pulseScale);
                element.GetElement().RenderTransformOrigin = new Point(0.5, 0.5);

                var opacity = 1 - distance / GameConfig.BoardSize * 0.5;
                element.GetElement().Opacity = opacity;
            }
        }

        // Анимация самой линии (масштабирование по нужной оси)
        var scaleX = orientation == OrientationTypeDict["horizontal"] ? lineScale : 1;
        var scaleY = orientation == OrientationTypeDict["vertical"] ? lineScale : 1;

        lineActive.GetElement().RenderTransform = new ScaleTransform(scaleX, scaleY);
        lineActive.GetElement().RenderTransformOrigin = new Point(0.5, 0.5);
        lineActive.GetElement().Opacity = lineOpacity;
    }

    private static void StartLineDetonation(int tickCounter)
    {
        scale = 1;
        var animationTime = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 2;
        animationSpeed = GameConfig.BoardSize * GameConfig.CellSize / animationTime;
        tickAnimationEnd = MyMathHelper.TickAddition(tickCounter, animationTime);
        State = GameState.LineDetonation;
    }

    private static void BombAnimation(int tickCounter)
    {
        if (tickCounter >= tickAnimationEnd)
        {
            foreach (var bomb in bombs)
            {
                var (x, y) = bomb.GetCoords();

                var startX = Math.Max(0, x - 1);
                var endX = Math.Min(GameConfig.BoardSize - 1, x + 1);
                var startY = Math.Max(0, y - 1);
                var endY = Math.Min(GameConfig.BoardSize - 1, y + 1);

                for (var i = startX; i <= endX; i++)
                for (var j = startY; j <= endY; j++)
                {
                    var element = gameObjectBoard[i, j];

                    if (element != null && !dieGameObjects.Contains(element))
                    {
                        dieGameObjects.Add(element);
                        if (lines.Contains(element)) linesActive.Add(element);
                    }
                }

                DeleteElement(bomb);
            }

            bombs.Clear();
            bombInGame = false;
            if (linesActive.Any())
                StartLineDetonation(tickCounter);
            else
                State = GameState.AfterSwap;
        }
        else
        {
            foreach (var bomb in bombs)
            {
                scale += 0.05;
                bomb.GetElement().RenderTransform = new ScaleTransform(scale, scale);
                bomb.GetElement().RenderTransformOrigin = new Point(1 / scale, 1 / scale);
            }
        }
    }

    private static void StartBomb(int tickCounter)
    {
        scale = 1;
        var animationTime = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 4;
        tickAnimationEnd = MyMathHelper.TickAddition(tickCounter, animationTime);
        State = GameState.BombAnimating;
    }

    private static void StartDestroy(int tickCounter)
    {
        scale = 1;
        var animationTime = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 4;
        tickAnimationEnd = MyMathHelper.TickAddition(tickCounter, animationTime);
        State = GameState.Destroying;
    }

    private static bool checkCoordInLine(int coord, int coordEndLine, int lineSize)
    {
        return coordEndLine - lineSize + 1 <= coord && coord <= coordEndLine;
    }

    private static GameObjectClass selectedObject;
    private static GameObjectClass targetObject;
    private static bool bombInGame;
    private static readonly List<GameObjectClass> bombs = new();

    private static readonly List<GameObjectClass> lines = new();
    private static readonly List<GameObjectClass> linesActive = new();

    private static void SwapObjects(int tickCounter)
    {
        if (tickAnimationEnd < 0)
        {
            var animationTime = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 4;
            animationSpeed = (GameConfig.CellSize + 2) / animationTime;
            tickAnimationEnd = MyMathHelper.TickAddition(tickCounter, animationTime);
        }
        else if (tickCounter >= tickAnimationEnd)
        {
            var selectedObjectX = selectedObject.GetCoords().x;
            var selectedObjectY = selectedObject.GetCoords().y;

            var targetObjectX = targetObject.GetCoords().x;
            var targetObjectY = targetObject.GetCoords().y;

            (intBoard[selectedObjectX, selectedObjectY], intBoard[targetObjectX, targetObjectY]) = (
                intBoard[targetObjectX, targetObjectY], intBoard[selectedObjectX, selectedObjectY]);

            (gameObjectBoard[selectedObjectX, selectedObjectY], gameObjectBoard[targetObjectX, targetObjectY]) = (
                gameObjectBoard[targetObjectX, targetObjectY], gameObjectBoard[selectedObjectX, selectedObjectY]);

            dieGameObjects.Clear();
            CheckDestroy();
            if (dieGameObjects.Any())
            {
                selectedObject.ChangeCoords(targetObjectX, targetObjectY);
                targetObject.ChangeCoords(selectedObjectX, selectedObjectY);

                selectedObject.FixPosition();
                targetObject.FixPosition();

                var bonusStructureData = new List<BonusData>
                {
                    new()
                    {
                        ElementToBonus = targetObject,
                        IsHorizontalBonus = false,
                        IsVerticalBonus = false,
                        IsHorizontalLineDestroy = false,
                        IsVerticalLineDestroy = false
                    },
                    new()
                    {
                        ElementToBonus = selectedObject,
                        IsHorizontalBonus = false,
                        IsVerticalBonus = false,
                        IsHorizontalLineDestroy = false,
                        IsVerticalLineDestroy = false
                    }
                };

                (targetObjectX, targetObjectY) = targetObject.GetCoords();
                (selectedObjectX, selectedObjectY) = selectedObject.GetCoords();

                if (bonusGameObjects.Any())
                {
                    foreach (var bonus in bonusGameObjects)
                        if (bonus.orientation_type == OrientationTypeDict["horizontal"])
                        {
                            if (checkCoordInLine(targetObjectX, bonus.x, bonus.size) && bonus.y == targetObjectY)
                            {
                                if (bonus.size == 4)
                                {
                                    bonusStructureData[0].IsHorizontalBonus = true;
                                }
                                else if (bonus.size >= 5)
                                {
                                    bonusStructureData[0].IsHorizontalLineDestroy = true; //для условия бомбы
                                    bonusStructureData[0].IsVerticalLineDestroy = true;
                                }

                                bonusStructureData[0].IsHorizontalLineDestroy = true;
                            }

                            if (checkCoordInLine(selectedObjectX, bonus.x, bonus.size) && bonus.y == selectedObjectY)
                            {
                                if (bonus.size == 4)
                                {
                                    bonusStructureData[1].IsHorizontalBonus = true;
                                }
                                else if (bonus.size >= 5)
                                {
                                    bonusStructureData[1].IsHorizontalLineDestroy = true; //для условия бомбы
                                    bonusStructureData[1].IsVerticalLineDestroy = true;
                                }

                                bonusStructureData[1].IsHorizontalLineDestroy = true;
                            }
                        }
                        else if (bonus.orientation_type == OrientationTypeDict["vertical"])
                        {
                            if (checkCoordInLine(targetObjectY, bonus.y, bonus.size) && bonus.x == targetObjectX)
                            {
                                if (bonus.size == 4)
                                {
                                    bonusStructureData[0].IsVerticalBonus = true;
                                }
                                else if (bonus.size >= 5)
                                {
                                    bonusStructureData[0].IsHorizontalLineDestroy = true; //для условия бомбы
                                    bonusStructureData[0].IsVerticalLineDestroy = true;
                                }

                                bonusStructureData[0].IsVerticalLineDestroy = true;
                            }

                            if (checkCoordInLine(selectedObjectY, bonus.y, bonus.size) && bonus.x == selectedObjectX)
                            {
                                if (bonus.size == 4)
                                {
                                    bonusStructureData[1].IsVerticalBonus = true;
                                }
                                else if (bonus.size >= 5)
                                {
                                    bonusStructureData[1].IsHorizontalLineDestroy = true; //для условия бомбы
                                    bonusStructureData[1].IsVerticalLineDestroy = true;
                                }

                                bonusStructureData[1].IsVerticalLineDestroy = true;
                            }
                        }

                    foreach (var bonusData in bonusStructureData)
                        if (bonusData.IsHorizontalLineDestroy && bonusData.IsVerticalLineDestroy)
                        {
                            dieGameObjects.Remove(bonusData.ElementToBonus);
                            bonusData.ElementToBonus.ChangeToBomb();
                            bombs.Add(bonusData.ElementToBonus);
                            bombInGame = true;
                        }
                        else if (bonusData.IsVerticalBonus)
                        {
                            dieGameObjects.Remove(bonusData.ElementToBonus);
                            bonusData.ElementToBonus.ChangeToLine(OrientationTypeDict["vertical"]);
                            lines.Add(bonusData.ElementToBonus);
                        }
                        else if (bonusData.IsHorizontalBonus)
                        {
                            dieGameObjects.Remove(bonusData.ElementToBonus);
                            bonusData.ElementToBonus.ChangeToLine(OrientationTypeDict["horizontal"]);
                            lines.Add(bonusData.ElementToBonus);
                        }
                }
            }
            else
            {
                (intBoard[selectedObjectX, selectedObjectY], intBoard[targetObjectX, targetObjectY]) = (
                    intBoard[targetObjectX, targetObjectY], intBoard[selectedObjectX, selectedObjectY]);

                (gameObjectBoard[selectedObjectX, selectedObjectY], gameObjectBoard[targetObjectX, targetObjectY]) = (
                    gameObjectBoard[targetObjectX, targetObjectY], gameObjectBoard[selectedObjectX, selectedObjectY]);

                selectedObject.FixPosition();
                targetObject.FixPosition();
            }

            tickAnimationEnd = -1;
            if (linesActive.Any())
                StartLineDetonation(tickCounter);
            else
                State = GameState.AfterSwap;
        }
        else
        {
            var xdif = selectedObject.GetCoords().x - targetObject.GetCoords().x;
            var ydif = selectedObject.GetCoords().y - targetObject.GetCoords().y;

            if (xdif > 0)
            {
                selectedObject.MoveX(-animationSpeed);
                targetObject.MoveX(animationSpeed);
            }
            else if (xdif < 0)
            {
                selectedObject.MoveX(animationSpeed);
                targetObject.MoveX(-animationSpeed);
            }

            if (ydif > 0)
            {
                selectedObject.MoveY(-animationSpeed);
                targetObject.MoveY(animationSpeed);
            }
            else if (ydif < 0)
            {
                selectedObject.MoveY(animationSpeed);
                targetObject.MoveY(-animationSpeed);
            }
        }
    }

    public static void CheckAndSwapWithSelected(GameObjectClass firstElement, GameObjectClass SecondElement)
    {
        if (State != 0 || tickAnimationEnd != -1) return;

        selectedObject = firstElement;
        targetObject = SecondElement;

        var coordsFirstElement = firstElement.GetCoords();
        var coordsSecondElement = SecondElement.GetCoords();

        var relative = false;

        if (Math.Abs(coordsFirstElement.x - coordsSecondElement.x) == 1 &&
            Math.Abs(coordsFirstElement.y - coordsSecondElement.y) == 0)
            relative = true;

        if (Math.Abs(coordsFirstElement.x - coordsSecondElement.x) == 0 &&
            Math.Abs(coordsFirstElement.y - coordsSecondElement.y) == 1)
            relative = true;

        if (!relative) return;

        State = GameState.Swapping;
    }

    private static List<List<(int, int)>> dropMatrix;
    private static int currentDropIds;
    private static int animationSpeed;

    private static void DropObjects(int tickCounter)
    {
        if (dropMatrix.Count > 0)
        {
            if (tickAnimationEnd < 0)
            {
                GameObjectClass currentGameObject;
                foreach (var gameObjectCoords in dropMatrix[0])
                {
                    for (var i = gameObjectCoords.Item2; i > 0; i--)
                    {
                        currentGameObject = gameObjectBoard[gameObjectCoords.Item1, i - 1];
                        gameObjectBoard[gameObjectCoords.Item1, i] = currentGameObject;
                        intBoard[gameObjectCoords.Item1, i] = intBoard[gameObjectCoords.Item1, i - 1];
                        currentGameObject.ChangeCoords(gameObjectCoords.Item1, i);
                    }

                    var newGameObjectClass = GameHelper.getRandomNumber();

                    intBoard[gameObjectCoords.Item1, 0] = newGameObjectClass;

                    gameObjectBoard[gameObjectCoords.Item1, 0] =
                        CreateGameObjectByIndexType(newGameObjectClass, gameCanvas, gameObjectCoords.Item1, -1);
                    gameObjectBoard[gameObjectCoords.Item1, 0].ChangeCoords(gameObjectCoords.Item1, 0);
                }

                var animationTime = MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 4;
                animationSpeed = (GameConfig.CellSize + 2) / animationTime;
                tickAnimationEnd = MyMathHelper.TickAddition(tickCounter, animationTime);
            }
            else if (tickCounter >= tickAnimationEnd)
            {
                foreach (var gameObjectCoords in dropMatrix[0])
                    for (var i = 0; i <= gameObjectCoords.Item2; i++)
                        gameObjectBoard[gameObjectCoords.Item1, i].FixPosition();

                dropMatrix.RemoveAt(0);
                tickAnimationEnd = -1;
            }
            else
            {
                foreach (var gameObjectCoords in dropMatrix[0])
                    for (var i = 0; i <= gameObjectCoords.Item2; i++)
                        gameObjectBoard[gameObjectCoords.Item1, i].MoveY(animationSpeed);
            }
        }
        else
        {
            State = GameState.Idle;
        }
    }

    private static void GenerateDropMatrix()
    {
        dropMatrix = new List<List<(int, int)>>();

        int currentColumnEmptyCount;
        for (var i = 0; i < GameConfig.BoardSize; i++)
        {
            currentColumnEmptyCount = 0;
            for (var j = 0; j < GameConfig.BoardSize; j++)
                if (intBoard[i, j] == -1)
                {
                    currentColumnEmptyCount++;
                    if (currentColumnEmptyCount > dropMatrix.Count) dropMatrix.Add(new List<(int, int)>());
                    dropMatrix[currentColumnEmptyCount - 1].Add((i, j));
                }
        }
    }

    private static double scale = 1;

    private static void DestroyObject(int tickCounter)
    {
        if (tickCounter % (MyMathHelper.OneSecTicks(GameConfig.GameLoopIntervalMs) / 20) == 0)
        {
            if (!dieGameObjects.Any()) return;

            scale += 0.01;
            foreach (var gameObject in dieGameObjects)
            {
                gameObject.GetElement().RenderTransform = new ScaleTransform(scale, scale);
                gameObject.GetElement().RenderTransformOrigin = new Point(1 / scale, 1 / scale);
            }
        }

        if (tickCounter >= tickAnimationEnd)
        {
            tickAnimationEnd = -1;
            scale = 1;
            foreach (var gameObject in dieGameObjects)
            {
                Game.Score += 1;
                DeleteElement(gameObject);
            }

            dieGameObjects.Clear();
            State = GameState.Dropping;
            GenerateDropMatrix();
        }
    }

    private static void DeleteElement(GameObjectClass element)
    {
        gameCanvas.Children.Remove(element.GetElement());
        intBoard[element.GetCoords().x, element.GetCoords().y] = -1;
    }

    /// <summary>
    ///     Заполняет dieGameObjects, bonusGameObjects
    /// </summary>
    /// <param name="tickCouner"></param>
    private static void CheckDestroy()
    {
        if (gameObjectBoard == null) return;

        bonusGameObjects.Clear();
        dieGameObjects.Clear();

        CheckOrientationDestroy(true);

        CheckOrientationDestroy(false);
    }

    private static void CheckOrientationDestroy(bool isHorizontal)
    {
        for (var i = 0; i < GameConfig.BoardSize; i++)
        {
            var lastElementType = -1;
            var currentTypeCount = 0;

            for (var j = 0; j < GameConfig.BoardSize; j++)
            {
                int currentValue;
                if (isHorizontal)
                    currentValue = intBoard[j, i];
                else
                    currentValue = intBoard[i, j];

                if (currentValue == lastElementType)
                {
                    currentTypeCount++;
                }
                else
                {
                    if (currentTypeCount >= 3)
                    {
                        if (isHorizontal)
                            bonusGameObjects.Add((j - 1, i, currentTypeCount,
                                OrientationTypeDict["horizontal"], lastElementType));
                        else
                            bonusGameObjects.Add((i, j - 1, currentTypeCount,
                                OrientationTypeDict["vertical"], lastElementType));
                    }

                    currentTypeCount = 1;
                }

                if (currentTypeCount >= 3)
                    for (var k = j; k > j - currentTypeCount; k--)
                    {
                        GameObjectClass gameObjectToDie;
                        if (isHorizontal)
                            gameObjectToDie = gameObjectBoard[k, i];
                        else
                            gameObjectToDie = gameObjectBoard[i, k];

                        var orientation = isHorizontal ? "horizontal" : "vertical";
                        gameObjectToDie.PlayDieAnimation(OrientationTypeDict[orientation]);

                        if (!dieGameObjects.Contains(gameObjectToDie))
                        {
                            dieGameObjects.Add(gameObjectToDie);
                            if (lines.Contains(gameObjectToDie))
                                linesActive.Add(gameObjectToDie);
                        }
                    }

                lastElementType = currentValue;
            }


            if (currentTypeCount >= 3)
            {
                if (isHorizontal)
                    bonusGameObjects.Add((GameConfig.BoardSize - 1, i, currentTypeCount,
                        OrientationTypeDict["horizontal"], lastElementType));
                else
                    bonusGameObjects.Add((i, GameConfig.BoardSize - 1, currentTypeCount,
                        OrientationTypeDict["vertical"], lastElementType));
            }
        }
    }

    private static GameObjectClass[,] GenerateGameObjectByIndexArray(int[,] indexBoard)
    {
        var result = new GameObjectClass[GameConfig.BoardSize, GameConfig.BoardSize];

        for (var i = 0; i < GameConfig.BoardSize; i++)
        for (var j = 0; j < GameConfig.BoardSize; j++)
        {
            var indexValue = indexBoard[i, j];
            result[i, j] = CreateGameObjectByIndexType(indexValue, gameCanvas, i, j);
        }

        return result;
    }

    private static readonly Dictionary<int, string> spriteTypeDict = new()
    {
        { 1, "blue" },
        { 2, "green" },
        { 3, "orange" },
        { 4, "pink" },
        { 5, "red" }
    };

    public static GameObjectClass CreateGameObjectByIndexType(int spriteTypeIndex, Canvas canvas, int x, int y)
    {
        return new GameElement(spriteTypeDict[spriteTypeIndex], canvas, x, y);
    }

    public static GameObjectClass CreateGameObjectByTextType(string spriteType, Canvas canvas, int x, int y)
    {
        return new GameElement(spriteType, canvas, x, y);
    }
}