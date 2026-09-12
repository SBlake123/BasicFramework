using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIManager : MonoSingleton<IngameUIManager>
{
    public event Func<UniTask> InventoryRequested;
    public event Func<UniTask> AttackRequested;
    public event Func<UniTask> OptionRequested;

    public Canvas hudCanvas;

    public RectTransform InputUIParentRect;
    public RectTransform JoystickParent;

    public Button attackBtn;
    public Button inventoryBtn;
    public Button optionBtn;

    public async UniTask Init()
    {
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

}
