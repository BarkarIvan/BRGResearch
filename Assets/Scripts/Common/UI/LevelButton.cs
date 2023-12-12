public class LevelButton : SimpleButton
{
    public int LevelIndex;

    public delegate void LevelButtonClickedDelegate(int levelIndex);
    public event LevelButtonClickedDelegate LevelButtonClicked;

    protected override void OnClickEvent()
    {
        //base.OnClickEvent(); // вызываем базовую реализацию, если это необходимо
        LevelButtonClicked?.Invoke(LevelIndex);
    }
}