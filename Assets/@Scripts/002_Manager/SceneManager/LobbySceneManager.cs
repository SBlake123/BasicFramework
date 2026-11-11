using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum LobbySceneState
{
    NONE,
    LOADING,
    EXPLORE,
    PORTAL_CONFIRM,
    DIALOGUE,
    INVENTORY,
    UPGRADE,
    OPTION,
    ENTERING_GAME,
    GO_TO_TITLE
}

public class LobbySceneManager : StateBaseSceneManager
{
    [SerializeField] private Ingame_002_Inventory inventoryPage;
    [SerializeField] private Ingame_003_Option optionPage;
    [SerializeField] private GameObject screenGuard;
    [SerializeField] private Image transitionOverlay;
    [SerializeField] private Camera roomCamera;
    [SerializeField] private PopupManager popupPrefab;
    [SerializeField] private SoundManager soundPrefab;
    [SerializeField] private TMP_FontAsset lobbyFont;
    [SerializeField] private Transform portal;
    [SerializeField] private SpriteRenderer blacksmith;

    private const float PortalRadius = 1.3f;
    private const float NpcInteractionDistance = 2.35f;
    private const float TransitionDuration = 0.4f;
    private static readonly Color PanelColor = new(0.10f, 0.14f, 0.16f, 0.98f);
    private static readonly Color ButtonColor = new(0.19f, 0.33f, 0.31f, 1f);

    private LobbySceneState state;
    private IngameSessionManager session;
    private IngameUIManager hud;
    private GameObject dialogue;
    private GameObject upgradePanel;
    private TextMeshProUGUI hint;
    private PopupManager popup;
    private bool busy;
    private string saveError;
    private readonly List<RaycastResult> pointerHits = new();
    private PointerEventData pointerEvent;

    private void Start() => ChangeState((int)LobbySceneState.LOADING).Forget();

    public override void SceneAllocate()
    {
        inventoryPage.stateBaseSceneManager = this;
        optionPage.stateBaseSceneManager = this;
        inventoryPage.pageMain.SetActive(false);
        optionPage.pageMain.SetActive(false);
    }

    public override async UniTask ChangeState(int next)
    {
        var nextState = (LobbySceneState)next;
        if (state == nextState || busy) return;

        state = nextState;
        await OnStateChange();
    }

    public override async UniTask OnStateChange()
    {
        if (state == LobbySceneState.LOADING)
        {
            await InitializeLobby();
            return;
        }

        bool exploring = state == LobbySceneState.EXPLORE;
        session.player.SetInputBlocked(!exploring);
        hud.InputUISetActive(exploring);
        dialogue.SetActive(state == LobbySceneState.DIALOGUE);
        upgradePanel.SetActive(state == LobbySceneState.UPGRADE);
        inventoryPage.pageMain.SetActive(false);
        optionPage.pageMain.SetActive(false);
        hint.gameObject.SetActive(exploring);

        switch (state)
        {
            case LobbySceneState.INVENTORY:
            case LobbySceneState.UPGRADE:
                inventoryPage.pageMain.SetActive(true);
                break;
            case LobbySceneState.OPTION:
                await OpenPage(optionPage);
                break;
            case LobbySceneState.ENTERING_GAME:
                await LeaveLobby(GSceneName.INGAME_SCENE);
                break;
            case LobbySceneState.GO_TO_TITLE:
                await LeaveLobby(GSceneName.TITLE_SCENE);
                break;
        }
    }

    private async UniTask InitializeLobby()
    {
        busy = true;
        screenGuard.SetActive(true);
        SceneAllocate();
        session = IngameSessionManager.Instance;
        hud = IngameUIManager.Instance;
        popup = FindFirstObjectByType<PopupManager>();
        if (popup == null)
            popup = Instantiate(popupPrefab);
        if (FindFirstObjectByType<SoundManager>() == null)
            Instantiate(soundPrefab).SoundInit();
        session.player.SetInputBlocked(true);
        hud.InputUISetActive(false);
        var lookAhead = roomCamera.GetComponentInParent<PlayerCameraLookAhead>();
        if (lookAhead != null)
            lookAhead.player = session.player;

        BuildPanels();
        try
        {
            // Present the opaque overlay before loading or building any item views.
            await UniTask.NextFrame(destroyCancellationToken);
            await EnsureGameData();
            await hud.Init();
            await session.Init();
            hud.SetStaminaSlider();
            await PrepareLobbyPage(inventoryPage);
            await PrepareLobbyPage(optionPage);
            hud.ConsumeAttackInput = TryPortalInteraction;
            hud.InventoryRequested += OpenInventory;
            hud.OptionRequested += OpenOptions;
        }
        catch (Exception exception) when (!(exception is OperationCanceledException))
        {
            if (this == null) return;
            Debug.LogException(exception, this);
            hint.text = "로비를 준비하지 못했습니다.";
            await FadeOverlay(0f);
            return;
        }

        hud.InputUISetActive(true);
        Canvas.ForceUpdateCanvases();
        await FadeOverlay(0f);
        if (this == null) return;
        screenGuard.SetActive(false);
        busy = false;
        await ChangeState((int)LobbySceneState.EXPLORE);
        BackKeySetting().Forget();
    }

    private async UniTask PrepareLobbyPage(StateBasePage page)
    {
        page.pageMain.SetActive(true);
        try
        {
            await page.Init();
            Canvas.ForceUpdateCanvases();
        }
        finally
        {
            if (page != null)
                page.pageMain.SetActive(false);
        }
    }

    private async UniTask FadeOverlay(float alpha)
    {
        transitionOverlay.gameObject.SetActive(true);
        await transitionOverlay.DOFade(alpha, TransitionDuration)
            .SetUpdate(true)
            .SetLink(transitionOverlay.gameObject)
            .AsyncWaitForCompletion();
        if (alpha == 0f && transitionOverlay != null)
            transitionOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (state != LobbySceneState.EXPLORE) return;

        Vector2 position = session.player.transform.position;
        bool nearNpc = Vector2.Distance(position, blacksmith.transform.position) <= NpcInteractionDistance;
        bool onPortal = Vector2.Distance(position, portal.position) <= PortalRadius;
        hint.text = saveError ?? (onPortal ? "포탈 입장: 공격 버튼 또는 Z" :
            nearNpc ? "대장장이를 눌러 대화" : string.Empty);

        if (onPortal && Input.GetKeyDown(KeyCode.Z))
        {
            TryPortalInteraction();
            return;
        }

        if (nearNpc && TryGetPointer(out Vector2 pointer))
        {
            Ray ray = roomCamera.ScreenPointToRay(pointer);
            Bounds target = blacksmith.bounds;
            target.Expand(new Vector3(0.5f, 0.5f, 0.2f));
            if (target.IntersectRay(ray) && !HitsHudControl(pointer))
                ChangeState((int)LobbySceneState.DIALOGUE).Forget();
        }
    }

    private bool TryGetPointer(out Vector2 pointer)
    {
        pointer = default;
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began) continue;
                pointer = touch.position;
                return true;
            }
            return false;
        }

        if (!Input.GetMouseButtonDown(0)) return false;
        pointer = Input.mousePosition;
        return true;
    }

    private bool HitsHudControl(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        pointerEvent ??= new PointerEventData(eventSystem);
        pointerEvent.Reset();
        pointerEvent.position = position;
        eventSystem.RaycastAll(pointerEvent, pointerHits);
        foreach (RaycastResult hit in pointerHits)
        {
            if (hit.module is GraphicRaycaster && hit.gameObject.GetComponentInParent<Selectable>() != null)
                return true;
        }
        return false;
    }

    private bool TryPortalInteraction()
    {
        if (state != LobbySceneState.EXPLORE) return true;
        if (Vector2.Distance(session.player.transform.position, portal.position) > PortalRadius) return false;

        if (popup.setPopUpCode(true, "게임에 입장하시겠습니까?", "입장", "취소"))
        {
            ChangeState((int)LobbySceneState.PORTAL_CONFIRM).Forget();
            popup.AddMethodToBtn(
                () => ChangeState((int)LobbySceneState.ENTERING_GAME).Forget(),
                ReturnToExplore, ReturnToExplore);
        }
        return true;
    }

    private void ReturnToExplore() => ChangeState((int)LobbySceneState.EXPLORE).Forget();

    private UniTask OpenInventory() => state == LobbySceneState.EXPLORE
        ? ChangeState((int)LobbySceneState.INVENTORY) : UniTask.CompletedTask;

    private UniTask OpenOptions() => state == LobbySceneState.EXPLORE
        ? ChangeState((int)LobbySceneState.OPTION) : UniTask.CompletedTask;

    private async UniTask OpenPage(StateBasePage page)
    {
        busy = true;
        screenGuard.SetActive(true);
        try
        {
            await page.Init();
            page.pageMain.SetActive(true);
        }
        finally
        {
            screenGuard.SetActive(false);
            busy = false;
        }
    }

    public async UniTask CloseInventory()
    {
        if (busy || (state != LobbySceneState.INVENTORY && state != LobbySceneState.UPGRADE)) return;

        busy = true;
        screenGuard.SetActive(true);
        try
        {
            await inventoryPage.Inventory_000_Base.InventoryClosed();
            SaveProfile();
        }
        finally
        {
            screenGuard.SetActive(false);
            busy = false;
        }
        await ChangeState((int)LobbySceneState.EXPLORE);
    }

    private async UniTask LeaveLobby(string destination)
    {
        if (!SaveProfile())
        {
            await ChangeState((int)LobbySceneState.EXPLORE);
            return;
        }

        busy = true;
        screenGuard.SetActive(true);
        try
        {
            await FadeOverlay(1f);
            if (this == null) return;
            await SceneLoadManager.Instance.LoadScene(destination);
        }
        catch (Exception exception)
        {
            if (this == null) return;
            Debug.LogException(exception, this);
            saveError = "씬을 열지 못했습니다. 다시 시도해 주세요.";
            await FadeOverlay(0f);
            if (this == null) return;
            busy = false;
            screenGuard.SetActive(false);
            await ChangeState((int)LobbySceneState.EXPLORE);
        }
    }

    private async UniTask EnsureGameData()
    {
        var language = LanguageManager.Instance;
        var data = DataManager.Instance;
        bool needsLanguage = language.languageScriptDic.Count == 0;
        bool needsData = data.monsterDataDic.Count == 0;
        if (!needsLanguage && !needsData) return;

        await Addressables.InitializeAsync();
        if (needsLanguage)
            await language.OnInitialize();
        if (needsData)
            await data.OnInitialize();
    }

    private bool SaveProfile()
    {
        bool saved = PlayerDataManager.Instance.SaveLobbyProfile(session.playerIngameData);
        saveError = saved ? null : "저장하지 못했습니다. 다시 시도해 주세요.";
        return saved;
    }

    protected override async UniTask BackKeySetting()
    {
        while (!destroyCancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
            if (!Input.GetKeyDown(KeyCode.Escape) || busy) continue;

            switch (state)
            {
                case LobbySceneState.DIALOGUE:
                case LobbySceneState.OPTION:
                    await ChangeState((int)LobbySceneState.EXPLORE);
                    break;
                case LobbySceneState.PORTAL_CONFIRM:
                    popup.setPopUpClose();
                    await ChangeState((int)LobbySceneState.EXPLORE);
                    break;
                case LobbySceneState.INVENTORY:
                case LobbySceneState.UPGRADE:
                    await CloseInventory();
                    break;
            }
        }
    }

    private void BuildPanels()
    {
        Transform screen = screenGuard.transform.parent;
        hint = CreateLabel(screen, "Interaction Hint", string.Empty, 34f);
        Place(hint.rectTransform, new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(900, 58));

        dialogue = new GameObject("Blacksmith Dialogue", typeof(RectTransform));
        dialogue.transform.SetParent(screen, false);
        Stretch((RectTransform)dialogue.transform);
        Image backdrop = CreatePanel(dialogue.transform, "Dialogue Dismiss Area", Color.clear);
        Stretch(backdrop.rectTransform);
        var dismiss = backdrop.gameObject.AddComponent<Button>();
        dismiss.transition = Selectable.Transition.None;
        dismiss.onClick.AddListener(ReturnToExplore);

        var tooltipFrame = inventoryPage.Inventory_000_Base.itemConfigWindow.GetComponent<Image>();
        Image speech = CreatePanel(dialogue.transform, "Dialogue", tooltipFrame.color);
        ApplyTooltipFrame(speech, tooltipFrame);
        Place(speech.rectTransform, new Vector2(0.5f, 0.08f), new Vector2(0, 100), new Vector2(1250, 200));
        var speaker = CreateLabel(speech.transform, "Speaker", "대장장이", 34f);
        Place(speaker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -58), new Vector2(1100, 44));
        speaker.alignment = TextAlignmentOptions.MidlineLeft;
        speaker.color = new Color(1f, 0.82f, 0.53f);
        var text = CreateLabel(speech.transform, "Dialogue Text", "장비를 손보고 갈 텐가?", 43f);
        Place(text.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 67), new Vector2(1100, 62));
        text.alignment = TextAlignmentOptions.MidlineLeft;
        speech.raycastTarget = false;
        Image choices = CreatePanel(speech.transform, "Dialogue Choices", tooltipFrame.color);
        ApplyTooltipFrame(choices, tooltipFrame);
        Place(choices.rectTransform, new Vector2(1f, 1f), new Vector2(-180, 58), new Vector2(360, 100));
        AddButton(choices.transform, "강화", new Vector2(0.27f, 0.5f), Vector2.zero, () =>
            ChangeState((int)LobbySceneState.UPGRADE).Forget());
        AddButton(choices.transform, "취소", new Vector2(0.73f, 0.5f), Vector2.zero, ReturnToExplore);
        dialogue.SetActive(false);

        Image upgrade = CreatePanel(inventoryPage.pageMain.transform, "Upgrade Panel", PanelColor);
        Place(upgrade.rectTransform, new Vector2(1f, 0.5f), new Vector2(-1070, 0), new Vector2(500, 540));
        upgradePanel = upgrade.gameObject;
        var heading = CreateLabel(upgrade.transform, "Title", "장비 강화", 52f);
        Place(heading.rectTransform, new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(430, 70));
        Image slot = CreatePanel(upgrade.transform, "Equipment Slot", ButtonColor);
        Place(slot.rectTransform, new Vector2(0.5f, 0.57f), Vector2.zero, new Vector2(165, 165));
        Stretch(CreateLabel(slot.transform, "Slot Label", "대상 장비", 26f).rectTransform);
        var apply = AddButton(upgrade.transform, "강화", new Vector2(0.32f, 0.17f), Vector2.zero, null);
        apply.interactable = false;
        AddButton(upgrade.transform, "닫기", new Vector2(0.68f, 0.17f), Vector2.zero, () => CloseInventory().Forget());
        upgradePanel.SetActive(false);
    }

    private void ApplyTooltipFrame(Image panel, Image tooltipFrame)
    {
        panel.sprite = tooltipFrame.sprite;
        panel.type = tooltipFrame.type;
        panel.pixelsPerUnitMultiplier = tooltipFrame.pixelsPerUnitMultiplier;
    }

    private Image CreatePanel(Transform parent, string name, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        panel.transform.SetParent(parent, false);
        panel.color = color;
        return panel;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string name, string value, float size)
    {
        var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(parent, false);
        label.font = lobbyFont != null ? lobbyFont : hud.hpTmp.font;
        label.text = value;
        label.fontSize = size;
        label.color = new Color(0.96f, 0.93f, 0.82f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private Button AddButton(Transform parent, string label, Vector2 anchor, Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Image image = CreatePanel(parent, label, ButtonColor);
        Place(image.rectTransform, anchor, position, new Vector2(144, 64));
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (action != null) button.onClick.AddListener(action);
        Stretch(CreateLabel(image.transform, "Label", label, 33f).rectTransform);
        return button;
    }

    private void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

}
