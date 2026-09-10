using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InvenSpacePointerHandler))]
public class InventoryGrid_000_Base : MonoBehaviour
{
    public ItemData itemData { get; set; }

    public RectTransform itemImgParent;
}
