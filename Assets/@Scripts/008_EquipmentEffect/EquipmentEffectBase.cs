using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentEffectBase
{
    public Player player { get; set; }
    public ItemOptionData option { get; set; }
    public Transform objTrf { get; set; }
    public abstract UniTask Initialize(Player owner, ItemOptionData option);
    public abstract UniTask TriggerAsync();
    public abstract void Release();
}


public class BurningAuraEffect : EquipmentEffectBase
{
    public override async UniTask Initialize(Player owner, ItemOptionData option)
    {
        player = owner;
        this.option = option;

        GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.BURNING_AURA, owner.specialAttributeTrf);

        objTrf = obj.transform;

        // 범위, 피해량 설정하고 주변 적에게 주기적으로 피해
    }

    public override async UniTask TriggerAsync()
    {
        //없음.
    }


    public override void Release()
    {
        if (objTrf == null)
            return;

        ObjectPool.Instance.PushToPool(objTrf.gameObject);
        objTrf = null;
        // 피해 처리 중단, 감지 대상 목록 초기화
    }
}

public class FlameOnHitEffect : EquipmentEffectBase
{
    public override UniTask Initialize(Player owner, ItemOptionData option)
    {
        player = owner;
        this.option = option;

        return UniTask.CompletedTask;
    }

    public override async UniTask TriggerAsync()
    {
        int randomRange = UnityEngine.Random.Range(0, 100);

        if (randomRange < option.value)
        {
            GameObject obj = await ObjectPool.Instance.PopFromPool(GPrefabName.CRAWLING_FLAME, null, false);
            obj.transform.position = player.transform.position;
            obj.GetComponent<CrawlingFlame>().Init(player.lastLookDirection);
            obj.SetActive(true);
            //발동
        }
        //30퍼센트 확률로
    }


    public override void Release()
    {

    }
}

