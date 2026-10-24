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
        transform.DOKill();
        damageText.DOKill();

        transform.localScale = Vector3.one;
        damageText.alpha = 1f;
        gameObject.SetActive(true);

        damageText.DOFade(0f, 0.7f).SetDelay(0.3f).SetEase(Ease.Linear);

        transform.DOMoveY(transform.position.y + 20f, 1f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                damageText.DOKill();
                ObjectPool.Instance.PushToPool(gameObject);
            });

    }
}

