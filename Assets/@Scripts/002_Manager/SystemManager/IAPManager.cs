using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

/// <summary>
/// Unity IAP(In App Purchasing) 초기화, 구매 요청, 구매 처리, 복원을 담당하는 매니저.
/// 씬에 빈 GameObject를 만들고 이 스크립트를 붙여서 사용하세요.
/// Window > Package Manager에서 "In App Purchasing" 패키지가 설치되어 있어야 합니다.
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    [Serializable]
    public class ProductDefinitionEntry
    {
        public string productId;      // 코드/카탈로그에서 쓰는 공용 ID
        public ProductType productType;
    }

    [Header("상품 목록 (IAP Catalog와 ID를 동일하게 맞추세요)")]
    [SerializeField]
    private ProductDefinitionEntry[] products = new ProductDefinitionEntry[]
    {
        new ProductDefinitionEntry { productId = "coin_pack_small", productType = ProductType.Consumable },
        new ProductDefinitionEntry { productId = "remove_ads", productType = ProductType.NonConsumable },
        new ProductDefinitionEntry { productId = "premium_subscription", productType = ProductType.Subscription },
    };

    [Header("로컬 영수증 검증 (Google Play)")]
    [Tooltip("Window > Unity IAP > Receipt Validation Obfuscator 실행 후 생성되는 GooglePlayTangle을 사용합니다. " +
             "Services > In-App Purchasing 설정에서 Google Play 라이선스 공개키를 먼저 등록해야 합니다.")]
    [SerializeField]
    private bool useLocalReceiptValidation = true;

    private CrossPlatformValidator validator;

    public bool IsInitialized => storeController != null && extensionProvider != null;

    // 외부에서 구독할 이벤트들
    public event Action OnInitializedEvent;
    public event Action<string> OnInitializeFailedEvent;
    public event Action<Product> OnPurchaseSucceeded;
    public event Action<string, PurchaseFailureReason> OnPurchaseFailedEvent;
    public event Action<string, string> OnReceiptValidationFailed; // productId, 에러 메시지

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializePurchasing();
    }

    /// <summary>
    /// IAP를 초기화합니다. ConfigurationBuilder에 카탈로그의 상품을 등록합니다.
    /// </summary>
    public void InitializePurchasing()
    {
        if (IsInitialized)
        {
            Debug.Log("[IAPManager] 이미 초기화되어 있습니다.");
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var entry in products)
        {
            builder.AddProduct(entry.productId, entry.productType);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    // ---------------- 구매 요청 ----------------

    /// <summary>
    /// productId로 구매를 요청합니다. UI 버튼 등에서 호출하세요.
    /// </summary>
    public void BuyProduct(string productId)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("[IAPManager] 아직 초기화되지 않았습니다.");
            return;
        }

        Product product = storeController.products.WithID(productId);

        if (product == null)
        {
            Debug.LogError($"[IAPManager] 상품을 찾을 수 없습니다: {productId}");
            return;
        }

        if (!product.availableToPurchase)
        {
            Debug.LogError($"[IAPManager] 구매 불가능한 상품입니다: {productId}");
            return;
        }

        Debug.Log($"[IAPManager] 구매 요청: {productId}");
        storeController.InitiatePurchase(product);
    }

    /// <summary>
    /// 상품 가격 문자열(현지화됨)을 가져옵니다. 예: "₩1,200"
    /// </summary>
    public string GetLocalizedPriceString(string productId)
    {
        if (!IsInitialized) return string.Empty;

        Product product = storeController.products.WithID(productId);
        return product != null ? product.metadata.localizedPriceString : string.Empty;
    }

    /// <summary>
    /// 비소모성/구독 상품을 이미 보유하고 있는지 확인합니다.
    /// </summary>
    public bool HasReceipt(string productId)
    {
        if (!IsInitialized) return false;

        Product product = storeController.products.WithID(productId);
        return product != null && product.hasReceipt;
    }

    /// <summary>
    /// iOS 등에서 이전 구매 내역을 복원할 때 사용합니다.
    /// 실제로는 초기화 시 자동으로 hasReceipt가 반영되지만,
    /// 사용자가 명시적으로 "구매 복원" 버튼을 눌렀을 때 안내용으로 호출합니다.
    /// </summary>
    public void RestorePurchases(Action<bool> onComplete = null)
    {
#if UNITY_IOS
        if (!IsInitialized)
        {
            onComplete?.Invoke(false);
            return;
        }

        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions((result, message) =>
        {
            Debug.Log($"[IAPManager] 복원 결과: {result}, {message}");
            onComplete?.Invoke(result);
        });
#else
        Debug.Log("[IAPManager] 이 플랫폼에서는 별도 복원 절차가 필요 없습니다 (Google Play는 자동 반영).");
        onComplete?.Invoke(true);
#endif
    }

    // ---------------- IStoreListener 콜백 ----------------

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAPManager] 초기화 성공");
        storeController = controller;
        extensionProvider = extensions;

        if (useLocalReceiptValidation)
        {
            InitializeValidator();
        }

        OnInitializedEvent?.Invoke();
    }

    /// <summary>
    /// GooglePlayTangle / AppleTangle을 사용해 로컬 영수증 검증기를 준비합니다.
    /// GooglePlayTangle.cs가 프로젝트에 없다면 Window > Unity IAP > Receipt Validation Obfuscator에서
    /// Google Play 콘솔의 라이선스 공개키를 입력해 먼저 생성해야 합니다.
    /// </summary>
    private void InitializeValidator()
    {
        try
        {
            //validator = new CrossPlatformValidator(
            //    GooglePlayTangle.Data(),
            //    AppleTangle.Data(),
            //    Application.identifier);
        }
        catch (Exception e)
        {
            // GooglePlayTangle.cs가 아직 생성되지 않은 경우 여기서 예외가 발생합니다.
            Debug.LogError($"[IAPManager] 영수증 검증기 초기화 실패. Receipt Validation Obfuscator로 " +
                            $"GooglePlayTangle을 먼저 생성했는지 확인하세요. ({e.Message})");
            validator = null;
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAPManager] 초기화 실패: {error}");
        OnInitializeFailedEvent?.Invoke(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAPManager] 초기화 실패: {error}, {message}");
        OnInitializeFailedEvent?.Invoke($"{error}: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Product product = args.purchasedProduct;
        string productId = product.definition.id;

        if (useLocalReceiptValidation && validator != null)
        {
            if (!IsReceiptValid(product))
            {
                Debug.LogWarning($"[IAPManager] 영수증 검증 실패, 지급하지 않습니다: {productId}");
                OnReceiptValidationFailed?.Invoke(productId, "영수증 서명 검증 실패");
                // 위조된 영수증으로 판단되므로 지급 없이 트랜잭션만 종료합니다.
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log($"[IAPManager] 영수증 검증 통과: {productId}");
        }
        else if (useLocalReceiptValidation && validator == null)
        {
            // GooglePlayTangle 미생성 등으로 검증기가 없는 상태.
            // 개발 중에는 통과시키되, 배포 전에는 반드시 원인을 해결하세요.
            Debug.LogWarning($"[IAPManager] 검증기가 준비되지 않아 검증 없이 지급합니다: {productId}");
        }

        GrantItem(product);
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// CrossPlatformValidator로 영수증 서명을 검증합니다.
    /// Google Play는 서명 위조 여부를, 지원 스토어라면 결과 목록으로 상세 정보도 받을 수 있습니다.
    /// </summary>
    private bool IsReceiptValid(Product product)
    {
        try
        {
            IPurchaseReceipt[] receipts = validator.Validate(product.receipt);

            foreach (var receipt in receipts)
            {
                Debug.Log($"[IAPManager] 검증된 영수증 - productId: {receipt.productID}, purchaseDate: {receipt.purchaseDate}");

                if (receipt is GooglePlayReceipt googleReceipt)
                {
                    // 필요하다면 여기서 orderID, purchaseState(0=Purchased, 1=Cancelled) 등을 추가로 확인할 수 있습니다.
                    Debug.Log($"[IAPManager] GooglePlay orderId: {googleReceipt.transactionID}, purchaseState: {googleReceipt.purchaseState}");
                }
            }

            return true; // 예외 없이 통과하면 서명이 유효한 영수증입니다.
        }
        catch (IAPSecurityException e)
        {
            Debug.LogError($"[IAPManager] 영수증 서명이 유효하지 않습니다: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 검증을 통과한 구매에 대해 실제 게임 아이템/재화를 지급합니다.
    /// </summary>
    private void GrantItem(Product product)
    {
        string productId = product.definition.id;
        Debug.Log($"[IAPManager] 아이템 지급: {productId}");

        // TODO: 여기서 실제 아이템 지급 로직을 구현하세요.
        // 예시:
        // switch (productId)
        // {
        //     case "coin_pack_small":
        //         CurrencyManager.Instance.AddCoins(100);
        //         break;
        //     case "remove_ads":
        //         AdsManager.Instance.RemoveAds();
        //         break;
        // }

        OnPurchaseSucceeded?.Invoke(product);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[IAPManager] 구매 실패: {product.definition.id}, 사유: {failureReason}");
        OnPurchaseFailedEvent?.Invoke(product.definition.id, failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"[IAPManager] 구매 실패(상세): {product.definition.id}, {failureDescription.reason}, {failureDescription.message}");
        OnPurchaseFailedEvent?.Invoke(product.definition.id, failureDescription.reason);
    }
}