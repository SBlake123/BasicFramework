using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform joystickRect;
    private RectTransform handleRect;
    private Player targetPlayer;

    public static VirtualJoystick Create(Transform parent)
    {
        GameObject joystickObject = new GameObject(
            "VirtualJoystick",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(VirtualJoystick));

        joystickObject.transform.SetParent(parent, false);

        RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
        joystickRect.anchorMin = Vector2.zero;
        joystickRect.anchorMax = Vector2.zero;
        joystickRect.pivot = new Vector2(0.5f, 0.5f);
        joystickRect.anchoredPosition = new Vector2(150f, 150f);
        joystickRect.sizeDelta = new Vector2(220f, 220f);

        Image joystickImage = joystickObject.GetComponent<Image>();
        joystickImage.color = new Color(1f, 1f, 1f, 0.2f);

        GameObject handleObject = new GameObject(
            "Handle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        handleObject.transform.SetParent(joystickObject.transform, false);

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(100f, 100f);

        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 0.45f);
        handleImage.raycastTarget = false;

        VirtualJoystick joystick = joystickObject.GetComponent<VirtualJoystick>();
        joystick.joystickRect = joystickRect;
        joystick.handleRect = handleRect;
        return joystick;
    }

    public void SetTarget(Player player)
    {
        ClearInput();
        targetPlayer = player;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateInput(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateInput(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ClearInput();
    }

    private void OnDisable()
    {
        ClearInput();
    }

    private void UpdateInput(PointerEventData eventData)
    {
        if (targetPlayer == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        float radius = joystickRect.rect.width * 0.5f;
        Vector2 direction = Vector2.ClampMagnitude(localPoint / radius, 1f);
        float handleRange = radius - (handleRect.rect.width * 0.5f);

        handleRect.anchoredPosition = direction * handleRange;
        targetPlayer.SetVirtualJoystickInput(direction);
    }

    private void ClearInput()
    {
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }

        if (targetPlayer != null)
        {
            targetPlayer.ClearVirtualJoystickInput();
        }
    }
}
