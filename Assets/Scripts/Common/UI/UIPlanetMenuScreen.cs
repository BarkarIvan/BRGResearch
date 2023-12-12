
public class UIPlanetMenuScreen : UIScreen
{

    public LevelButton[] LevelButtons;


    public void Init()
    {
        LevelButtons = GetComponentsInChildren<LevelButton>();
    }
    
}
