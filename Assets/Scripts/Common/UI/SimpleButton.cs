using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButton : BaseEventButton, IPointerClickHandler
{
    private Sequence clickSeq;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    protected override void OnClick()
    {
        Vector3 oldPosition = transform.position;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        if (clickSeq == null)
        {
            clickSeq = DOTween.Sequence();
            clickSeq.SetUpdate(true);
            clickSeq.Append(transform.DOMove(newPosition, 0.1f).SetEase(Ease.InFlash));
            clickSeq.AppendCallback(OnClickEvent);
            clickSeq.Append(transform.DOMove(oldPosition, 0.5f).SetEase(Ease.OutFlash));

            clickSeq.SetAutoKill(false);
        }
        else
        {
            clickSeq.Restart();
        }
    }

    protected virtual void OnClickEvent()
    {
        Clicked?.Invoke();
    }

    public void Show(bool isShowing)
    {
        gameObject.SetActive(isShowing);
    }

    private void OnDisable()
    {
        clickSeq?.Kill();
    }
}