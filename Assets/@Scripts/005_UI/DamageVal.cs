using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageVal : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    public void SetDamage(int damage)
    {
        string value = damage.ToString();
        string result = "";

        foreach (char c in value)
            result += $"<sprite=\"DamageNumber\" index={c - '0'}>";

        damageText.text = result;
    }

    public void Play()
    {
        gameObject.SetActive(true);
        transform.DOKill();
        transform.DOMoveY(transform.position.y + 20f, 0.8f).SetEase(Ease.Linear).OnComplete(() => ObjectPool.Instance.PushToPool(gameObject));
    }
}

