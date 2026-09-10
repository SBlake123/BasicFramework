using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InventoryEmpty
{
    Empty,
    NotEmpty
}


public class InvenSpacePointerHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {

        // 아이템 존재 or 안존재
        // 아이템 이미지 생성
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 아이템 존재 or 안존재
        // 아이템 움직이기.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 아이템 존재 or 안존재
        // 떨어지는 위치 파악한 뒤 아이템 교체 or 장착
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 아이템 존재 or 안존재
        // 더블클릭시 장착
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 아이템 존재 or 안존재

        // 상세정보
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 아이템 존재 or 안존재
        //상세정보 해제
    }
}
