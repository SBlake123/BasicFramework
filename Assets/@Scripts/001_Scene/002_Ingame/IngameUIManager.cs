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

    public Canvas hudCanvas;

    public RectTransform InputUIParentRect;
    public RectTransform JoystickParent;

    public Button attackBtn;
    public Button inventoryBtn;

    public async UniTask Init()
    {
        attackBtn.onClick.AddListener(async () => await OnAttackClicked());
        inventoryBtn.onClick.AddListener(async () => await OnInventoryClicked());
    }

    private async UniTask OnInventoryClicked()
    {
        InventoryRequested?.Invoke();
    }

    private async UniTask OnAttackClicked()
    {
        AttackRequested?.Invoke();
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
