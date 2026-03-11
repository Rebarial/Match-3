namespace Match_3.GameObject.Interface;

public interface ClicableObjectInterface
{
    void Clicked();
}
public interface DefaultAnimationInterface
{
    void PlayIdleAnimation();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">Type of animation: 0 – horizontal, 1 – verticale</param>
    void PlayDieAnimation(int type);
}

public interface MovingObjectInterface
{
    void MoveAtCell(int x, int y);
}

public interface GameObjectInterface : DefaultAnimationInterface, MovingObjectInterface, ClicableObjectInterface
{
    
}

