using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[Serializable]
public class MonsterDropData
{
    public int monsterId { get; set; }
    public string prefabName { get; set; }
    public int itemId { get; set; }
    public bool isWeight { get; set; }
    public float weightRate { get; set; }
    public float dropRate { get; set; }
    public int minCount { get; set; }
    public int maxCount { get; set; }
}

public class ItemLootingManager : MonoSingleton<ItemLootingManager>
{
    public async UniTask MakingItemLootingBox(string objName, int monsterIdx, Transform transform)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(objName);

        if (obj.TryGetComponent<LootingBox>(out var lootingBox))
        {

        }

        obj.transform.position = transform.position;
        obj.SetActive(true);

    }

    public async UniTask OpenItemLootingBox(LootingBox lootingBox)
    {

    }

    //public Dictionary<int, List<ItemData>> lootingBoxDic = new Dictionary<int, List<ItemData>>();

    //너는 무슨 일을 해아하니?
    //몬스터가 죽었을 때, 상자를 만들어달라고 요청함,
    //그 몬스터가 어떤 몬스터인지에 따라 무슨 아이템을 줄 지 결정해야 함.
    //상자가 항상 있는 건 아님.
    //몬스터가 드롭할 수 있는 아이템 리스트를 따로 만들 것
    //몬스터 사망 => 요청 => 그 몬스터의 정보에 따른 루팅 정보가 들어있는 상자 드롭
    //상자 클릭 시 인벤토리에 그 상자의 내용 들어가기.
    //내 생각에는 번호를 붙여서 싱자 리스트에 맞는 아이템 들어가는 것이 필요.
}
