using UnityEngine;
using UnityEngine.UI;

public class UIGameScreen : UIScreen
{
    public SimpleButton _drawSoilModeButton;
    public SimpleButton _drawPlantsModeButton;
    public SimpleButton _drawPlants2ModeButton;
    public SimpleButton _PlayMeteoriteEventButton;
    public RawImage _mapImage;
    
    private Color _activeColor = Color.white;
    private Color _inactiveColor = new Color(1, 1, 1, 0.7f);

    
    private Image _drawSoilButtonImage;
    private Image _drawPlantsButtonImage;
    private Image _drawPlants2ButtonImage;

    private void Awake()
    {
        _drawSoilButtonImage = _drawSoilModeButton.GetComponent<Image>();
        _drawPlants2ButtonImage = _drawPlants2ModeButton.GetComponent<Image>();
        _drawPlantsButtonImage = _drawPlantsModeButton.GetComponent<Image>();
    }

    public void SetMap(Texture2D mapImage)
    {
        _mapImage.texture = mapImage;
    }
    
    public void SetDrawSoilMode()
    {
        _drawSoilButtonImage.color = _activeColor;
        _drawPlantsButtonImage.color = _inactiveColor;
        _drawPlants2ButtonImage.color =  _inactiveColor;
    }

    public void SetDrawPlantsMode()
    {
        _drawSoilButtonImage.color = _inactiveColor;
        _drawPlantsButtonImage.color = _activeColor;
        _drawPlants2ButtonImage.color = _inactiveColor;
    }

    public void SetDrawPlants2Mode()
    {
        _drawSoilButtonImage.color = _inactiveColor;
        _drawPlantsButtonImage.color = _inactiveColor;
        _drawPlants2ButtonImage.color = _activeColor;
    }

    public void SetInactiveDrawingMode()
    {
        _drawSoilButtonImage.color = _inactiveColor;
        _drawPlantsButtonImage.color = _inactiveColor;
        _drawPlants2ButtonImage.color = _inactiveColor;
    }
}