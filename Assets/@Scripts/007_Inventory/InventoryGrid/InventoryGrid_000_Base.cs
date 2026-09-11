using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum GridCategory
{
    Stash,
    Weapon,
    Armor,
    Acc
}


[RequireComponent(typeof(InvenSpacePointerHandler))]
public partial class InventoryGrid_000_Base : MonoBehaviour
{
    public Inventory_000_Base Inventory_000_Base;
    public ItemData itemData { get; set; }

    public RectTransform itemImgParent;
}
