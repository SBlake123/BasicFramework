/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// 
///  1. 정의 : 인앱 결제를 위한 MonoBehaviour 라이브러리 클래스
///  2. 기능
///     A. 인 앱 결제 처리      [이원진, 2017년 5월 19일]
///     B. 구매 요청            [이원진, 2017년 5월 19일]
///     C. 상품 정보 획득       [이원진, 2017년 5월 19일]
///     D. 영수증 정보 획득     [이원진, 2017년 5월 19일]
/// --------------------------------------------------------------------------------------------------------------------------------------
//#define USE_IAP

using LitJson;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_IAP 
using UnityEngine.Purchasing;
#endif

#if USE_IAP 
public class InAppPurchaser : MonoBehaviour, IStoreListener
#else
public class InAppPurchaser : MonoBehaviour
#endif
{
    /// [IAP 연동 셋팅 가이드]------------------------------------------------------------------------------------------------------------
    /// 
    ///  1. 유니티의 window 메뉴에서 service 탭을 선택해서 열고 In App Purchaser를 선택해서 임포트합니다.
    ///  2. 해당 파일을 프로젝트에 삽입합니다.
    ///  3. 시작 씬에 빈 게임 오브젝트를 생성하여 삽입한 다음 이 스크립트를 컴포넌트로 연결합니다.
    ///  4. 상품 아이디와 라이선스 키를 스태틱 변수에 등록해줍니다.
    ///  
    ///  ※ 해당 IAP 구현은 컨슘되는 일회성 결제만을 다루고 있습니다. 구독 타입의 결제를 구현하는 경우, 추가로 구현해야 합니다.
    ///  ※ 해당 IAP 구현에서 함수이름은 해당 예제 및 SDK 연동에 따라 기본적으로 첫글자 파스칼 표기법을 따르고 있습니다. 
    ///    단, 개발자가 해당 클래스 아래에 추가한 함수와 전역 변수 이름은 코딩 스타일에 따라 SKU를 제외하고 카멜 표기법을 따릅니다. 
    /// 
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// [전역 변수 선언]------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// [상품ID 등록]
    ///  1. 상품ID는 개발자 콘솔에 등록한 상품ID와 동일하게 해주세요.
    ///  2. 해당 상품ID는 개발자 콘솔에 가서 앱 정보의 인앱 제품에서 확인할 수 있습니다. 이름 말고 ID를 넣으세요
    ///     ※ 구글은 인앱 상품 ID가 번들ID에 종속되지만 iOS는 번들ID와 무관하게 독립적입니다.
    ///       이 말 뜻은, 구글은 각 번들에 따라서 인앱 ID가 변경되지 않지만 iOS는 번들 별로 인앱 ID를 바꿔줘야 한다는 뜻입니다. 
    ///       이 경우, iOS는 [번들 아이디].[제품 아이디]로 스토어에 등록하고 상품 아이디 앞에 번들 아이디를 붙여서 상품아이디를 만들어주면 깔끔합니다.   
    /// </summary>
    public static string[] SKU = new string[0];

    /// <summary>
    /// [구글 라이선스 키 등록]
    /// 1. 구글 결제를 위해서는 애플리케이션 용 라이선스 키가 필요합니다. iOS는 필요하지 않습니다.
    /// 2. 구글 라이선스 공개 키는 개발자 콘솔에 가서 개발 도구 항목의 "서비스 및 API"에서 확인할 수 있습니다. 
    ///    (라이선스 및 인앱 결제 항목에 있는 "Base64 인코딩 RSA 공개 키")
    /// 3. 구글 결제 서버 구현 시에도 해당 키의 등록은 필요합니다.
    /// </summary>
    public static string googleKey = gData.purchaseKey.key;
#if USE_IAP
    public static IStoreController storeController;         //IAP 제어 변수입니다
    public static IExtensionProvider extensionProvider;     //IAP 제어 변수입니다
#endif
    public static int callIAP = 0;          //업데이트 함수 동작을 제어하기 위한 전역 변수입니다.
    public static string callSKU = "";      //결제할 특정 상품의 ID를 알아내기 위한 전역 변수입니다.

    //public static float krwAdd = 1.0f;      //한국 부가세처리. 2017년 11월 이후 필요없음

    public static bool bBuyItemOrder;

    public static double iapTimer;
    public static bool bNowInitializing;

    enum IAP_ERROR
    {
        NON_COMPLETE = 0,
        USER_CANCEL
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [MonoBehaviour 함수]
    /// 
    /// 1. Awake()  : 구동시 불리우며 초기 설정 및 InitializePurchasing 함수를 호출합니다.
    /// 2. Update() : callIAP가 1인 경우, BuyProductID 함수를 호출합니다.
    /// </summary>
    void Start()
    {
    }
    void Awake()
    {
        bBuyItemOrder = true;
        gBase.setEnKey();
        gData.setIAPKey();
        DontDestroyOnLoad(gameObject);
        gBase.setIapInit();
        InitializePurchasing();
        callIAP = 0;
        callSKU = "";
        iapTimer = 0.0f;
    }
    void Update()
    {
        if(callIAP == 1)
        {
            callIAP = 0;
            BuyProductID(callSKU);
            callSKU = "";
        }
#if !UNITY_EDITOR && USE_IAP 
        iapTimer += Time.deltaTime;
        //인잇 여부 확인
        if (IsInitialized() == false)
        {
            //인잇이 안된 경우 다시 인잇을 처리
            if (iapTimer > 15.0f && bNowInitializing == false)
            {   //15초가 지났고 인잇을 시도하는 상황이 아니라면 다시 인잇을 시도한다
                Debug.Log("iapModule : Initialize retry");
                InitializePurchasing();
                iapTimer = 0.0f;
            }
            else if(iapTimer > 300.0f)
            {   //300초가 넘었다면 초기화한다. 오버플로우를 방지하기 위한 예외처리
                iapTimer = 0.0f;
            }
        }
        else
        {
            //인잇이 된 경우 타이며는 0
            iapTimer = 0.0f;
        }
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [이니셜라이즈 관련 라이브러리 함수]
    /// 
    /// 1. IsInitialized()          : 이니셜라이즈가 되어 있으면 true, 되어 있지 않으면 false를 리턴합니다.
    /// 2. InitializePurchasing()   : 상품ID와 라이선스 키를 IAP 모듈에 등록하여 이니셜라이즈시킵니다. 
    /// 3. OnInitialized()          : 이니셜라이즈 성공 시 처리하는 콜백 함수입니다. storeController와 extensionProvider를 활성화시킵니다
    /// 4. OnInitializeFailed()     : 이니셜라이즈 실패 시 처리하는 콜백 함수입니다.
    /// </summary>

    //이니셜라이즈 체크함수
    /// <returns> true or false </returns>
    public static bool IsInitialized()
    {
#if USE_IAP
        return (storeController != null && extensionProvider != null);
#else
        return true;
#endif
    }
    //이니셜라이즈 메인함수
    public void InitializePurchasing()
    {
#if !UNITY_EDITOR && USE_IAP
        if (IsInitialized())
            return;

        bNowInitializing = true;

        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        for (int i = 0; i < SKU.Length; i++)
        {
            builder.AddProduct(SKU[i], ProductType.Consumable, new IDs
            {
                { SKU[i], AppleAppStore.Name },
                { SKU[i], GooglePlay.Name },
            });
        }
#if UNITY_ANDROID
        builder.Configure<IGooglePlayConfiguration>().SetPublicKey(googleKey);
#endif
        UnityPurchasing.Initialize(this, builder);
#endif

    }

#if USE_IAP
    //이니셜라이즈 성공
    public void OnInitialized(IStoreController sc, IExtensionProvider ep)
    {
        Debug.Log("OnInitialized : PASS");
        storeController = sc;
        extensionProvider = ep;
        bBuyItemOrder = true;
        //RestorePurchase();
        bNowInitializing = false;
    }
    //이니셜라이즈 실패
    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + reason);
        bBuyItemOrder = false;
        bNowInitializing = false;
    }
#endif
    /// ---------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// [구매 관련 라이브러리 함수]
    /// 
    /// 1. BuyProductID() : 업데이트 함수에서 호출되어 넘겨받은 상품ID로 구매를 진행합니다.
    /// 2. RestorePurchase() : iOS 구매복원에 대한 함수입니다.
    /// 3. ProcessPurchase() : 구매 성공 시 해당 함수로 구매 결과 데이터(영수증)가 넘어오고 컴플리트 처리가 됩니다.
    /// 4. OnPurchaseFailed() : 구매 실패 시 상품과 실패 이유가 넘어옵니다.
    /// </summary>
    //구매 메인 함수
    public void BuyProductID(string productId)
    {
#if USE_IAP
        try
        {
            if (IsInitialized())
            {
                Product p = storeController.products.WithID(productId);

                if (p != null && p.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", p.definition.id));
                    storeController.InitiatePurchase(p);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        catch (Exception e)
        {
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
            purchaseFailedCheck((int)IAP_ERROR.NON_COMPLETE);
        }
#endif
    }

    //리스토어 함수
    public void RestorePurchase()
    {
#if USE_IAP
        extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result => {
            if (result)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
            }
            else
            {
                // Restoration failed.
            }
        });
#endif
    }

/*
    //iOS용 리스토어 함수
    public void RestorePurchase()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions
            (
                (result) => { Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore."); }
            );
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }
*/

#if USE_IAP
    //구매 성공 시 콜백 함수
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
#if UNITY_ANDROID
        JsonData json = JsonMapper.ToObject(args.purchasedProduct.receipt);
        JsonData json2 = JsonMapper.ToObject(json["Payload"].ToString());
        string receipt = json2["json"].ToString();
        string sign = json2["signature"].ToString();
        string sku = args.purchasedProduct.definition.id;
        purchaseResultData(receipt, sign, sku);
#elif UNITY_IOS
        JsonData json = JsonMapper.ToObject(args.purchasedProduct.receipt);
        JsonData val = json["Payload"];
        string receipt = val.ToString();
        string sku = args.purchasedProduct.definition.id;
        string sign = "";
        purchaseResultData(receipt, sign, sku);
#endif
        //return PurchaseProcessingResult.Complete;
        return PurchaseProcessingResult.Pending;
    }
    //구매 실패 시 콜백 함수
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        if (failureReason == PurchaseFailureReason.UserCancelled)
            purchaseFailedCheck((int)IAP_ERROR.USER_CANCEL);
        else if (failureReason == PurchaseFailureReason.ExistingPurchasePending || failureReason == PurchaseFailureReason.DuplicateTransaction)
        {
#if UNITY_ANDROID
            JsonData json = JsonMapper.ToObject(product.receipt);
            JsonData json2 = JsonMapper.ToObject(json["Payload"].ToString());
            string receipt = json2["json"].ToString();
            string sign = json2["signature"].ToString();
            string sku = product.definition.id;
            purchaseResultData(receipt, sign, sku);
#elif UNITY_IOS
            JsonData json = JsonMapper.ToObject(product.receipt);
            JsonData val = json["Payload"];
            string receipt = val.ToString();
            string sku = product.definition.id;
            string sign = "";
            purchaseResultData(receipt, sign, sku);
#endif
            purchaseFailedCheck((int)IAP_ERROR.NON_COMPLETE);
        }
        else
            purchaseFailedCheck(-1);
    }
#endif
    /// ---------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// [구매 관련 개발자 처리 추가 함수]
    /// 
    /// 1. buyProduct() : 외부 클래스에서 해당 함수를 상품ID와 함께 호출하면, 넘어온 pId를 받아서 Update 함수가 동작하도록 callIAP 값과 callSKU 값을 처리합니다.
    ///                   ※ 외부 클래스 및 컴포넌트에서 상품ID를 셋팅할 때는 InAppPurchaser.SKU를 찾아서 해당 값으로 셋팅해야 합니다.
    /// 2. getInitPurchasing() : 외부 클래스에서 현재 이니셜라이즈가 되었는지를 파악할 수 있는 함수입니다.
    /// 3. getProductPrice() : 외부 클래스에서 상품의 가격을 문자열로 파악할 수 있는 함수입니다.
    ///                        ※ 상품의 가격은 스토어에 등록되어 있는, 환율에 따라 해당 마켓의 통화로 변환된 값이 통화기호와 함께 전달됩니다.
    /// 4. purchaseFailedCheck() : 상품 결제가 실패한 경우, 해당 이유에 대해 파악할 수 있습니다.
    /// 5. purchaseResultData()  : 영수증 정보를 받아서 처리하는 함수입니다. 만약 서버 검증을 하려면 해당 영수증, 시그니쳐, sku 정보를 서버에 전송하는 외부 클래스 함수를 
    ///                            여기에서 해당 값을 넣어 호출하면 됩니다.
    /// </summary>
    //구매 요청
    public static void buyProduct(string pId)
    {
        if (callIAP == 0)
        {
            callIAP = 1;
            callSKU = pId;
            bBuyItemOrder = false;
        }
    }

    //이니셜라이즈 여부로 게임을 시작해도 되는지를 전달하는 함수
    //단, 아예 게임을 못 즐기는 일은 없어야 하기 때문에, 일정 시간이 지나면 게임은 스타트해도 된다고 알리고 업데이트에서 이니셜라이즈를 다시 요청한다.
    public static bool checkInitPurchasingStart()
    {
#if USE_IAP
        if (gBase.gameVersion.marketType != (int)ENUM_MARKET_TYPE.GOOGLE && gBase.gameVersion.marketType != (int)ENUM_MARKET_TYPE.APPLE)
        {
            return true;
        }
        else
        {
//#if UNITY_EDITOR
//          return true;
//#else
            if (storeController != null && extensionProvider != null)
                return true;
            else if(iapTimer > 10.0f)
                return true;
            else
                return false;
//#endif
        }
#else
        return true;
#endif
    }
    //상품 가격을 알아오는 함수
    public static string getProductPrice(string productId)
    {
        string str = "Cash";
#if USE_IAP
        if (storeController != null && extensionProvider != null)
        {
            str = storeController.products.WithID(productId).metadata.localizedPriceString;
            if (str != null)
                return str;
        }
#endif
        return str;
    }
    //구매 실패 시 이유를 알아내는 함수
    public static void purchaseFailedCheck(int reason)
    {
        switch(reason)
        {
            case (int)IAP_ERROR.NON_COMPLETE:
                //만약 무슨 일이 있어서 컴플리트가 되지 않은 상품의 재구매를 요청할 경우 해당 에러가 발생합니다.
                break;
            case (int)IAP_ERROR.USER_CANCEL:
                //유저가 상품 구매를 취소할 경우 해당 에러가 발생합니다.
                break;
            default:
                //해당되지 않는 에러 처리.
                break;
        }
        bBuyItemOrder = true;
    }
    //구매 정보를 서버에 전송하기 위한 값을 받아오는 함수. 여기에서 받아온 해당 값들을 서버에 보내면 됩니다.
    public static void purchaseResultData(string receipt, string sign, string sku)
    {
        bBuyItemOrder = true;
        gBase.setIapPurchase(receipt, sign, sku);
    }

    //구매 완료 시 컨슘처리
    public static void confirmPendingPurchase(string productID)
    {
#if USE_IAP
        var p = storeController.products.WithID(productID);
        storeController.ConfirmPendingPurchase(p);
#endif
    }
    /// ---------------------------------------------------------------------------------------------------------------------------------

}
