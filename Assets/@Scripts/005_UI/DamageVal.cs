using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageVal : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;


    private void OnEnable()
    {
        gameObject.SetActive(true);

        SetDamage(223337);

        transform.DOMoveY(transform.position.y + 20f, 0.8f).SetEase(Ease.Linear).OnComplete(()=> gameObject.SetActive(false));      
    }

    public void SetDamage(int damage)
    {
        string value = damage.ToString();
        string result = "";

        foreach (char c in value)
            result += $"<sprite=\"DamageNumber\" index={c - '0'}>";

        damageText.text = result;
    }
}

