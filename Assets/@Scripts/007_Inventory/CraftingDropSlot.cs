using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingDropSlot : MonoBehaviour, IPointerClickHandler
{
    public LobbyCraftingUI owner;
    public bool isScroll;
    public void Accept(InventoryGrid_000_Base source) => owner.AssignSlot(source, isScroll);
    public void OnPointerClick(PointerEventData eventData) => owner.AssignSlot(null, isScroll);
}
