using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageVal : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    private void Start()
    {
        SetDamage(227);
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

