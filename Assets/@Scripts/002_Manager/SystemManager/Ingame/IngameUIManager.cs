using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUIManager : MonoSingleton<IngameUIManager>
{
    public event Func<UniTask> InventoryRequested;
    public event Func<UniTask> AttackRequested;
    public event Func<UniTask> DodgeRequested;
    public event Func<UniTask> OptionRequested;

    public Canvas hudCanvas;

    public RectTransform InputUIParentRect;
    public RectTransform JoystickParent;

    public Button attackBtn;
    public Button dodgeBtn;
    public Button inventoryBtn;
    public Button optionBtn;

    public Slider hpSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI hpTmp;

    public async UniTask Init()
    {
        dodgeBtn.onClick.AddListener(async () => await OnDodgeClicked());
        attackBtn.onClick.AddListener(async () => await OnAttackClicked());
        inventoryBtn.onClick.AddListener(async () => await OnInventoryClicked());
        optionBtn.onClick.AddListener(async () => await OnOptionClicked());
    }

    private async UniTask OnInventoryClicked()
    {
        InventoryRequested?.Invoke();
    }

    private async UniTask OnAttackClicked()
    {
        AttackRequested?.Invoke();
    }

    private async UniTask OnDodgeClicked()
    {
        DodgeRequested?.Invoke();
    }

    private async UniTask OnOptionClicked()
    {
        OptionRequested?.Invoke();
    }


    public void InputUISetActive(bool isActive)
    {
        JoystickParent.gameObject.SetActive(isActive);
        InputUIParentRect.gameObject.SetActive(isActive);
    }

    public void JoyStickUISetActive(bool isActive)
    {
        JoystickParent.gameObject.SetActive(isActive);
    }

    public void SetHpSliderText()
    {
        hpTmp.text = $"{IngameSessionManager.Instance.playerStats.currentHp}/{IngameSessionManager.Instance.playerStats.maxHp}";
    }

}
