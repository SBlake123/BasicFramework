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
    public event Func<UniTask> LootingBoxOpenRequested;
    public event Func<UniTask> LootingBoxClosedRequested;

    public LootingBox nowLootingBox { get; set; }

    public async UniTask MakingItemLootingBox(string objName, int monsterIdx, Transform transform)
    {
        GameObject obj = await ObjectPool.Instance.PopFromPool(objName);

        if (obj.TryGetComponent<LootingBox>(out var lootingBox))
        {
            lootingBox.Init();
            SettingLootingItem(monsterIdx, lootingBox);
        }

        if (lootingBox.itemDataList.Count == 0)
        {
            ObjectPool.Instance.PushToPool(obj);
            return;
        }

        obj.transform.position = transform.position;
        obj.SetActive(true);
    }

    public async UniTask OpenItemLootingBox(LootingBox lootingBox)
    {
        nowLootingBox = lootingBox;

        LootingBoxOpenRequested?.Invoke();
        //인벤토리에서 열어야 함.

        //가지고 있는 
    }

    public void SettingLootingItem(int monsterIdx, LootingBox lootingBox)
    {
        if (!DataManager.Instance.monsterDropDataDic.TryGetValue(monsterIdx, out var drops)) return;

        float totalWeight = 0f;

        foreach (MonsterDropData drop in drops)
        {
            if (drop.isWeight)
            {
                if (drop.weightRate > 0f) totalWeight += drop.weightRate;
            }
            else
            {
                TryDropItem(drop);
            }
        }
        if (totalWeight > 0f)
        {
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;
            MonsterDropData selectedDrop = null;

            foreach (MonsterDropData drop in drops)
            {
                if (!drop.isWeight || drop.weightRate <= 0f) continue;

                selectedDrop = drop;
                cumulativeWeight += drop.weightRate;
                if (roll < cumulativeWeight) break;
            }

            if (selectedDrop != null) TryDropItem(selectedDrop);
        }

        lootingBox.isSetting = true;

        void TryDropItem(MonsterDropData drop)
        {
            if (drop.dropRate <= 0f) return;
            if (drop.dropRate < 1f && UnityEngine.Random.value >= drop.dropRate) return;

            if (drop.minCount < 0 || drop.maxCount < drop.minCount)
            {
                Debug.LogError($"드롭 수량 설정 오류: monsterId={monsterIdx}, itemId={drop.itemId}");
                return;
            }

            if (!DataManager.Instance.itemDataDic.TryGetValue(drop.itemId, out var template))
            {
                Debug.LogError($"아이템 데이터 없음: {drop.itemId}");
                return;
            }

            int count = UnityEngine.Random.Range(drop.minCount, drop.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                ItemData item = template.DeepCopy();
                item.isEquip = 0;
                item.gridIdx = lootingBox.itemDataList.Count;
                SetDropItemOption(item);
                lootingBox.itemDataList.Add(item);
            }
        }

        void SetDropItemOption(ItemData item)
        {
            int roll = UnityEngine.Random.Range(0, 200);
            int section = 0;

            if (roll < 100)
                section = 0;
            else if (roll < 195)
                section = 1;
            else if (roll < 200)
                section = 2;

            item.itemRarity = section;


            
            //section 값에 따라 붙을 수 있는 옵션 조사하기
        }
    }

    public async UniTask CreatedItem(int itemId, ItemRarity itemRarity)
    {
        switch (itemRarity)
        {
            case ItemRarity.COMMON:
                {

                }
                break;
            case ItemRarity.UNCOMMON:
                {

                }
                break;
            case ItemRarity.RARE:
                {

                }
                break;
            case ItemRarity.UNIQUE:
                {

                }
                break;
        }
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
