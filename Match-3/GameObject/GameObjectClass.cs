using System.IO;
using System.Reflection;
using System.Resources;
using Match_3.GameObject.Interface;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using Match_3.Tools;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Match_3.GameObject;

public abstract class GameObjectClass: GameObjectInterface
{
    protected static string DefaultSpriteName = "rune";
    
    protected string SpriteType;
    
    protected Grid Element;
    
    protected string ImagesDefaultPath = "Match_3.GameObject.Sprites";

    protected int X;
    protected int Y;
    protected int pixelX;
    protected int pixelY;
    
    protected static int OffSet = 2;

    public event EventHandler ClickedEvent;
    
    public static GameObjectClass selectedObject = null;

    public GameObjectClass(string spriteType, Canvas canvas, int x, int y)
    {
        
        X = x;
        Y = y;
        
        SpriteType = spriteType;

        string resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_n.png";

        Element = new Grid
        {
            Width = GameConfig.CellSize,
            Height = GameConfig.CellSize
        };
        
        var ElementImage = new Image
        {
            Width = GameConfig.CellSize, 
            Height = GameConfig.CellSize,
            Stretch = System.Windows.Media.Stretch.Uniform,
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
        
        if (selectedObject == null)
        {
            SelectObject();
        }
        else if (selectedObject == this)
        {
            DeselectObject();
        }
        else
        {
            GameObjectFactory.CheckAndSwapWithSelected(selectedObject, this);
            DeselectObject();
        }
    }
    
    private void SelectObject()
    {
        selectedObject = this;

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
        if (selectedObject != null)
        {
            selectedObject.Element.Effect = null;
            selectedObject = null;
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
        
        string typeString = "h";
        if (type == 0) typeString = "h";
        if (type == 1) typeString = "v";
        
        string resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_{typeString}.png";
        Element.Children.OfType<Image>().FirstOrDefault().Source =  GameHelper.LoadImageFromResource(resourceName);
    }

    public void ChangeToBomb()
    {
        if (Element.Children.OfType<Image>().FirstOrDefault() == null) return;

        isBomb = true;
        string resourceName = $"Match_3.GameObject.Sprites.bonuses.bomb.png";
        Element.Children.OfType<Image>().FirstOrDefault().Source =  GameHelper.LoadImageFromResource(resourceName);
    }

    private int linesOrientation;
    private bool isBomb = false;

    public int GetOreintationType()
    {
        if (linesOrientation == null) return -1;
        return linesOrientation;
    }
    public void ChangeToLine(int orientationType)
    {
        if (Element.Children.OfType<Image>().FirstOrDefault() == null) return;
        
        string typeString = "h";
        if (orientationType == 0) typeString = "h";
        if (orientationType == 1) typeString = "v";

        linesOrientation = orientationType;
        
        string resourceName = $"Match_3.GameObject.Sprites.{SpriteType}.{DefaultSpriteName}_{SpriteType}_{typeString}.png";
        Element.Children.OfType<Image>().FirstOrDefault().Source =  GameHelper.LoadImageFromResource(resourceName);
        
        TextBlock textBlock = new TextBlock();
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

