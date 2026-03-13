using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Match_3.GameObject.Interface;
using Match_3.Tools;

namespace Match_3.GameObject;

public abstract class GameObjectClass : GameObjectInterface
{
    private static string DefaultSpriteName = "rune";

    private string SpriteType;

    protected Grid Element;

    protected string ImagesDefaultPath = "Match_3.GameObject.Sprites";

    private int X;
    private int Y;
    private int pixelX;
    private int pixelY;

    private static int OffSet = 2;
    
    private int linesOrientation;
    private bool isBomb;

    public event EventHandler ClickedEvent;

    public static GameObjectClass SelectedObject;

    public GameObjectClass(string spriteType, Canvas canvas, int x, int y)
    {
        X = x;
        Y = y;

        SpriteType = spriteType;

        var resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_n.png";

        Element = new Grid
        {
            Width = GameConfig.CellSize,
            Height = GameConfig.CellSize
        };

        var ElementImage = new Image
        {
            Width = GameConfig.CellSize,
            Height = GameConfig.CellSize,
            Stretch = Stretch.Uniform,
            Source = GameHelper.LoadImageFromResource(resourceName)
        };

        Element.Children.Add(ElementImage);

        Element.MouseLeftButtonDown += OnElementClicked;
        Element.Cursor = Cursors.Hand;

        pixelX = X * GameConfig.CellSize + OffSet;
        pixelY = Y * GameConfig.CellSize + OffSet;
        Canvas.SetLeft(Element, pixelX);
        Canvas.SetTop(Element, pixelY);
        canvas.Children.Add(Element);
    }

    private void OnElementClicked(object sender, MouseButtonEventArgs e)
    {
        Clicked();
        ClickedEvent?.Invoke(this, EventArgs.Empty);
    }

    public void FixPosition()
    {
        pixelX = X * GameConfig.CellSize + OffSet;
        pixelY = Y * GameConfig.CellSize + OffSet;
        Canvas.SetLeft(Element, pixelX);
        Canvas.SetTop(Element, pixelY);
    }

    public void MoveY(int y)
    {
        pixelY += y;
        Canvas.SetTop(Element, pixelY);
    }

    public void MoveX(int x)
    {
        pixelX += x;
        Canvas.SetLeft(Element, pixelX);
    }

    public void ChangeCoords(int x, int y)
    {
        X = x;
        Y = y;
    }

    public (int x, int y) GetCoords()
    {
        return (X, Y);
    }

    public UIElement GetElement()
    {
        return Element;
    }

    public void Clicked()
    {
        if (GameObjectFactory.State != 0) return;

        if (SelectedObject == null)
        {
            SelectObject();
        }
        else if (SelectedObject == this)
        {
            DeselectObject();
        }
        else
        {
            GameObjectFactory.CheckAndSwapWithSelected(SelectedObject, this);
            DeselectObject();
        }
    }

    private void SelectObject()
    {
        SelectedObject = this;

        Element.Effect = new DropShadowEffect
        {
            Color = Colors.Yellow,
            ShadowDepth = 0,
            BlurRadius = 10,
            Opacity = 1
        };
    }

    private void DeselectObject()
    {
        if (SelectedObject != null)
        {
            SelectedObject.Element.Effect = null;
            SelectedObject = null;
        }
    }

    public void PlayIdleAnimation()
    {
        throw new NotImplementedException();
    }

    public void PlayDieAnimation(int type)
    {
        if (isBomb) return;

        if (Element.Children.OfType<Image>().FirstOrDefault() == null) return;

        var typeString = "h";
        if (type == 0) typeString = "h";
        if (type == 1) typeString = "v";

        var resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_{typeString}.png";
        Element.Children.OfType<Image>().FirstOrDefault().Source = GameHelper.LoadImageFromResource(resourceName);
    }
    
    private Color[] colorMassive =
    {
        Colors.White,
        Colors.Blue,
        Colors.Green,
        Colors.Orange,
        Colors.Pink,
        Colors.Red
    };

    public void ChangeToBomb(int colorIndex)
    {
        if (Element.Children.OfType<Image>().FirstOrDefault() == null) return;

        isBomb = true;
        var image = Element.Children.OfType<Image>().FirstOrDefault();
        var resourceName = "Match_3.GameObject.Sprites.bonuses.bomb.png";
        image.Source = GameHelper.LoadImageFromResource(resourceName);
        
        var textBlock = new TextBlock();
        textBlock.Text = "Color";
        textBlock.Foreground = new SolidColorBrush(colorMassive[colorIndex]);
        textBlock.FontSize = 20;
        textBlock.FontWeight = FontWeights.Bold;
        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.TextWrapping = TextWrapping.Wrap;

        Element.Children.Add(textBlock);
    }

    public int GetOreintationType()
    {
        return linesOrientation;
    }

    public void ChangeToLine(int orientationType)
    {
        if (Element.Children.OfType<Image>().FirstOrDefault() == null) return;

        var typeString = "h";
        if (orientationType == 0) typeString = "h";
        if (orientationType == 1) typeString = "v";

        linesOrientation = orientationType;

        var resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_{typeString}.png";
        Element.Children.OfType<Image>().FirstOrDefault().Source = GameHelper.LoadImageFromResource(resourceName);

        var textBlock = new TextBlock();
        textBlock.Text = "Line";
        textBlock.Foreground = new SolidColorBrush(Colors.White);
        textBlock.FontSize = 24;
        textBlock.FontWeight = FontWeights.Bold;
        textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.TextWrapping = TextWrapping.Wrap;

        Element.Children.Add(textBlock);
    }

    public void MoveAtCell(int x, int y)
    {
        throw new NotImplementedException();
    }
}