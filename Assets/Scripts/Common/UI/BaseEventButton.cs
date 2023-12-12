using UnityEngine;
using UnityEngine.Events;

public abstract class BaseEventButton : MonoBehaviour
{
    public UnityEvent Clicked;

    public bool isInteractable { get; private set; } = true;

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
    }

    protected virtual void OnClick() { }

    
}