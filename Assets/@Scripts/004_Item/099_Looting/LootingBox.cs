using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PooledObject))]
public class LootingBox : MonoBehaviour, IPointerClickHandler
{
    public List<ItemData> itemDataList { get; set; } = new List<ItemData>();

    private float minDistance = 1f;

    public bool isSetting { get; set; } = false;

    public void Init()
    {
        itemDataList.Clear();
        isSetting = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var player = IngameSessionManager.Instance.player;
        if (player == null) return;

        var boxCollider = GetComponent<Collider>();
        Vector3 closestPoint =
            boxCollider.ClosestPoint(player.transform.position);

        if (Vector2.Distance(closestPoint, player.transform.position) > minDistance)
            return;

        ItemLootingManager.Instance.OpenItemLootingBox(this);
    }
}
