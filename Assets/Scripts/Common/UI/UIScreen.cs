
using DG.Tweening;
using UnityEngine;
[RequireComponent(typeof(CanvasGroup))]
public class UIScreen : MonoBehaviour
{
    
    private CanvasGroup canvasGroup;
    private Sequence openAnimSeq;
    private Sequence closeAnimSeq;


    public void Init()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Close();
    }
    

    public void Open()
    {
        OpenAnimation();
    }


    public void Close()
    {
        CloseAnimation();
    }


    private void OpenAnimation()
    {
        if (closeAnimSeq.IsPlaying()) closeAnimSeq.Pause();
        SetInteractable(true);
        canvasGroup.alpha = 0;
        if (openAnimSeq == null)
        {
            openAnimSeq = DOTween.Sequence();
            openAnimSeq.Append(canvasGroup.DOFade(1, 0.2f)).SetEase(Ease.OutQuart);
            openAnimSeq.SetAutoKill(false);
        }
        else
        {
            openAnimSeq.Restart();
        }
    }

    private void CloseAnimation()
    { 
        SetInteractable(false);
        if (closeAnimSeq == null)
        {
            closeAnimSeq = DOTween.Sequence();
            closeAnimSeq.Append(canvasGroup.DOFade(0, 0.2f)).SetEase(Ease.InQuart).OnComplete(()=>SetInteractable(false));
            closeAnimSeq.SetAutoKill(false);
        }
        else
        {
            closeAnimSeq.Restart();
        }
    }

    private void SetInteractable(bool isInteractable)
    {
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;
    }
    
}