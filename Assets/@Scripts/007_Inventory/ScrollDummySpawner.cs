using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Attached to the crafting panel. Can also be driven from the Inspector during Play Mode.
public class ScrollDummySpawner : MonoBehaviour
{
    public Inventory_000_Base inventory;
    public int itemId = 80001;

    public UniTask Spawn(int id) => inventory.AddDummyScroll(id);

    [ContextMenu("선택한 주문서 인벤토리에 1개 생성")]
    private async void SpawnSelected()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Play Mode에서 사용해 주세요.", this); return; }
        try { await Spawn(itemId); }
        catch (Exception error) { Debug.LogException(error, this); }
    }
}
