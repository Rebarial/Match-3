using Match_3.GameObject;
using System.Windows;
using System.Windows.Controls;

namespace Match_3.GameObject;

public class GameElement: GameObjectClass
{
    public GameElement(string spriteType, Canvas canvas, int x, int y) : base(spriteType, canvas, x, y)
    {
        
    }
    
    public UIElement GetElement()  // Добавьте этот метод
    {
        return Element;
    }

}