using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform touchAreaRect;
    public RectTransform joystickRect;
    public RectTransform handleRect;
    private Player targetPlayer;
    private int? activePointerId;
    private Vector2 defaultJoystickPosition;

    //public static VirtualJoystick Create(Transform parent)
    //{
    //    GameObject joystickObject = new GameObject(
    //        "VirtualJoystick",
    //        typeof(RectTransform),
    //        typeof(CanvasRenderer),
    //        typeof(Image),
    //        typeof(VirtualJoystick));

    //    joystickObject.transform.SetParent(parent, false);

    //    RectTransform joystickRect = joystickObject.GetComponent<RectTransform>();
    //    joystickRect.anchorMin = Vector2.zero;
    //    joystickRect.anchorMax = Vector2.zero;
    //    joystickRect.pivot = new Vector2(0.5f, 0.5f);
    //    joystickRect.anchoredPosition = new Vector2(150f, 150f);
    //    joystickRect.sizeDelta = new Vector2(220f, 220f);

    //    Image joystickImage = joystickObject.GetComponent<Image>();
    //    joystickImage.color = new Color(1f, 1f, 1f, 0.2f);

    //    GameObject handleObject = new GameObject(
    //        "Handle",
    //        typeof(RectTransform),
    //        typeof(CanvasRenderer),
    //        typeof(Image));

    //    handleObject.transform.SetParent(joystickObject.transform, false);

    //    RectTransform handleRect = handleObject.GetComponent<RectTransform>();
    //    handleRect.anchorMin = new Vector2(0.5f, 0.5f);
    //    handleRect.anchorMax = new Vector2(0.5f, 0.5f);
    //    handleRect.pivot = new Vector2(0.5f, 0.5f);
    //    handleRect.anchoredPosition = Vector2.zero;
    //    handleRect.sizeDelta = new Vector2(100f, 100f);

    //    Image handleImage = handleObject.GetComponent<Image>();
    //    handleImage.color = new Color(1f, 1f, 1f, 0.45f);
    //    handleImage.raycastTarget = false;

    //    VirtualJoystick joystick = joystickObject.GetComponent<VirtualJoystick>();
    //    joystick.joystickRect = joystickRect;
    //    joystick.handleRect = handleRect;
    //    return joystick;
    //}

    private void Awake()
    {
        if (joystickRect != null)
        {
            defaultJoystickPosition = joystickRect.anchoredPosition;
        }
    }

    public void SetTarget(Player player)
    {
        ClearInput();
        targetPlayer = player;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId.HasValue || touchAreaRect == null || joystickRect == null || handleRect == null)
        {
            return;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(
                touchAreaRect,
                eventData.position,
                eventData.pressEventCamera))
        {
            return;
        }

        MoveJoystickBase(eventData);
        activePointerId = eventData.pointerId;
        UpdateInput(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activePointerId == eventData.pointerId)
        {
            UpdateInput(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activePointerId == eventData.pointerId)
        {
            ClearInput();
        }
    }

    private void OnDisable()
    {
        ClearInput();
    }

    private void UpdateInput(PointerEventData eventData)
    {
        if (joystickRect == null || handleRect == null ||
            !TryGetOffset(eventData, out Vector2 offset))
        {
            return;
        }

        // Clamp the center independently of the visual image size.
        Vector2 position = Vector2.ClampMagnitude(offset, JoystickMoveRadius());
        Vector2 direction = position.normalized;

        // The scene handle is a centered, direct child of the joystick background.
        handleRect.anchoredPosition = position;
        if (targetPlayer != null)
        {
            targetPlayer.SetVirtualJoystickInput(direction);
        }
    }

    private void MoveJoystickBase(PointerEventData eventData)
    {
        RectTransform parentRect = joystickRect.parent as RectTransform;
        if (parentRect == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        // anchoredPosition is measured from the RectTransform's anchor reference point.
        Vector2 anchorReference = new Vector2(
            Mathf.Lerp(parentRect.rect.xMin, parentRect.rect.xMax,
                (joystickRect.anchorMin.x + joystickRect.anchorMax.x) * 0.5f),
            Mathf.Lerp(parentRect.rect.yMin, parentRect.rect.yMax,
                (joystickRect.anchorMin.y + joystickRect.anchorMax.y) * 0.5f));

        joystickRect.anchoredPosition = localPoint - anchorReference;
    }

    private float JoystickMoveRadius()
    {
        return Mathf.Min(joystickRect.rect.width, joystickRect.rect.height) * 0.2f;
    }

    private bool TryGetOffset(PointerEventData eventData, out Vector2 offset)
    {
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        offset = localPoint - joystickRect.rect.center;
        return success;
    }

    private void ClearInput()
    {
        activePointerId = null;
        if (handleRect != null)
        {
            handleRect.anchoredPosition = Vector2.zero;
        }

        if (joystickRect != null)
        {
            joystickRect.anchoredPosition = defaultJoystickPosition;
        }

        if (targetPlayer != null)
        {
            targetPlayer.ClearVirtualJoystickInput();
        }
    }
}
