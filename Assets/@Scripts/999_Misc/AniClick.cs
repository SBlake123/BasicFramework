using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class AniClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public float standardScale { get; set; } = 0.6f;
    public float expendingScale { get; set; } = 0.66f;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(expendingScale, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(expendingScale, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(standardScale, 0.15f);
    }

    public void ScaleSetting(float standardScale, float expendingScale)
    {
        this.standardScale = standardScale;
        this.expendingScale = expendingScale;
    }
}
