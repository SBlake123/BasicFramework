/// [스크립트 명세]-----------------------------------------------------------------------------------------------------------------------
///  유니티 IAP를 처리하는 매니저이다
///  로고 씬에서 GameObject 컴포넌트에 등록
/// --------------------------------------------------------------------------------------------------------------------------------------

using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

public class gIapManager : MonoBehaviour, IDetailedStoreListener
{
    private static IStoreController m_StoreController; // The Unity Purchasing system.
    private static int nowState = (int)IAP_STATE.START;
    private static ConfigurationBuilder builder;
    public static string logMsg = "";
    // 리턴 액션
    private static Action returnAction;

    public static class iapPrefs
    {
        //식별 정보
        public const string iapUserId   = "iapUserId";
        public const string iapToken    = "iapToken";
        public const string iapServer   = "iapServer";
        public const string iapMarket   = "iapMarket";        //[10 = 구글, 20 = iOS, 30 = 원스토어]
        
        //영수증
        public const string iapTxid     = "iapTxid";
        public const string iapSign     = "iapSign";
        public const string iapSku      = "iapSku";
    }

    enum IAP_STATE
    {
        START = 0,          //준비 상태
        READY,              //준비 상태
        INIT,               //인잇
        ACTIVATE,           //대기 중
        REQUEST,            //요청 처리 중
        WAITING_REGIST,     //저장된 상품 등록 대기 중
        NETWORKING,         //네트워크 응답 대기 중
        REWARD,             //보상 대기 중
        POPUP,              //팝업 대기 중
    }

    //서버에서 인앱 결제인지 구독인지 보내주는 키 값을 해당 Enum에 적용한다.
    public enum IAP_KIND
    {
        CONSUMABLE = 10,    //일반 인앱 결제
        SUBSCRIPTION = 20   //구독
    }

    //리턴코드
    public struct receiptStr
    {
        public string userId;
        public string token;
        public string server;
        public string market;

        public string txid;
        public string sign;
        public string sku;
    }


    // Awake is called before the first frame update
    void Awake()
    {
        var obj = FindObjectsOfType<gIapManager>();
        if (obj.Length <= 1)
        {
            gBase.setEnKey();
            DontDestroyOnLoad(gameObject);
            nowState = (int)IAP_STATE.READY;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (nowState)
        {
            case (int)IAP_STATE.INIT:
                initProduct();
                break;
            case (int)IAP_STATE.ACTIVATE:
                if (checkHaveBuyInfo() == true)
                {
                    netIap(getBuyInfo());
                }
                break;
            case (int)IAP_STATE.WAITING_REGIST:
                netIap(getBuyInfo());
                break;
            case (int)IAP_STATE.NETWORKING:
                getNetworkResult();
                break;
        }
    }

    //[이하 network 매니저 및 팝업 매니저와 연동하는 함수]=============================================

    //인 앱 결제를 요청한다.
    void netIap(receiptStr receipt)
    {
        if (nowState != (int)IAP_STATE.NETWORKING && gNetworkManager.bCheckUse() == true)
        {
            string[] inputPost = new string[9];
            string[] inputStr = new string[9];

            nowState = (int)IAP_STATE.NETWORKING;

            inputPost[0] = "uIndex";
            inputPost[1] = "token";
            inputPost[2] = "server";
            inputPost[3] = "market";        //[10 = 구글, 20 = iOS, 30 = 원스토어]
            inputPost[4] = "product";       //sdk에서 획득한 상품명
            inputPost[5] = "txid";          //sdk에서 획득한 영수증
            inputPost[6] = "sign";          //sdk에서 획득한 시그니처
            inputPost[7] = "udid";          // 디바이스 고유값
            inputPost[8] = "timeZoneVal";   //로컬 타임계 오프셋

            //inputStr[0] = Convert.ToString(gBase.getUserIndex());
            //inputStr[1] = gBase.getLoginToken();
            //inputStr[2] = Convert.ToString(gBase.getServerNum());
            //inputStr[3] = Convert.ToString((int)LoginManager.Instance.GetMarketType());
            inputStr[0] = receipt.userId;
            inputStr[1] = receipt.token;
            inputStr[2] = receipt.server;
            inputStr[3] = receipt.market;
            inputStr[4] = receipt.sku;
            inputStr[5] = receipt.txid;
            inputStr[6] = receipt.sign;
            inputStr[7] = SystemInfo.deviceUniqueIdentifier;
            inputStr[8] = Convert.ToString(TimeZoneInfo.Local.BaseUtcOffset.Hours * 100 + TimeZoneInfo.Local.BaseUtcOffset.Minutes);
            gNetworkManager.setRequest("0342_buyIap.php", inputPost, inputStr, true, true);
        }
    }

    //네트워킹 결과값이 떨어지면 해당 API에 따라 처리한다.
    void getNetworkResult()
    {
        //네트워크 매니저에서 결과값이 떨어지면
        if (gNetworkManager.getResult() == true)
        {
            int yesState = (int)IAP_STATE.ACTIVATE;
            int noState = (int)IAP_STATE.ACTIVATE;
            //JSON 데이터를 전달받는다.
            if (gNetworkManager.getErrorMsg() != "")
            {
                //통신 에러를 처리한다.
                switch (gNetworkManager.getApi())
                {
                    default:
                        setPopUp(yesState, noState, false, 99008);
                        break;
                }
            }
            else
            {
                JsonData json = gNetworkManager.getResponseJson();
                //리턴 코드를 처리한다
                gNetworkManager.returnStr reVal = gNetworkManager.checkReturnSuccessCode(json);
                if (reVal.bSuccess == true)
                {
                    //API에 따라 결과를 처리한다.
                    switch (gNetworkManager.getApi())
                    {
                        case "0342_buyIap.php":
                            receiveIap(json);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (reVal.code >= 10000)
                    {
                        setPopUp(yesState, noState, false, reVal.code);
                    }
                    else
                    {
                        //타임 아웃
                        setPopUp(yesState, noState, false, 99007);
                    }
                    clearIapInfo(-1);
                }
            }
            //API값을 클리어한다.
            gNetworkManager.clearApi();
        }
    }

    async void receiveIap(JsonData json)
    {
        if (json != null)
        {
            clearIapInfo(0);
            gNetworkManager.buyIAPReturn = JsonConvert.DeserializeObject<BuyIAPReturn>(gNetworkManager.getResponseMsg());
            if (gNetworkManager.buyIAPReturn.get.Count > 0)
            {
                nowState = (int)IAP_STATE.REWARD;
                await RewardManager.Instance.RewardSetting(gNetworkManager.buyIAPReturn.get);
                await RewardManager.Instance.ReceiveRewardAfterSetting(async () => await clickRewardBtnClose(true));
            }
            else
            {
                await clickRewardBtnClose(false);
            }    
        }
        else
        {
            clearIapInfo(-1);
        }
    }

    async UniTask clickRewardBtnClose(bool usingAction)
    {
        switch (usingAction)
        {
            case true:
                {
                    if (returnAction != null)
                    {
                        returnAction.Invoke();
                        returnAction = null;
                    }
                    break;
                }
            case false:
                {
                    if (returnUniTask != null)
                    {
                        await returnUniTask.Invoke();
                        returnUniTask = null;
                    }
                    break;
                }
        }

        nowState = (int)IAP_STATE.ACTIVATE;
    }

    private static int yesSceneState = -1;
    private static int noSceneState = -1;

    //코드 기반으로 씬을 바꾸고 팝업을 요청한다.
    private static void setPopUp(int ySceneState, int nSceneState, bool bChoose, int textCode)
    {
        string yesStr = "";
        string noStr = "";
        string bodyStr = "";
        yesStr = LanguageManager.Instance.GetLangScript(90013, LanguageManager.Instance.languageScriptDic);
        noStr = LanguageManager.Instance.GetLangScript(90014, LanguageManager.Instance.languageScriptDic);
        bodyStr = LanguageManager.Instance.GetLangScript(textCode, LanguageManager.Instance.languageScriptDic);

        yesSceneState = ySceneState;
        noSceneState = nSceneState;

        nowState = (int)IAP_STATE.POPUP;
        //gPopUpManager.setPopUpCode(bChoose, yesStr, noStr, "", bodyStr);
        if (gPopUpManager.setPopUpCode(bChoose, yesStr, noStr, "", bodyStr) == true)
        {
            if (bChoose == true)
            {
                gPopUpManager.AddMethodToBtn(setYesState, setNoState);
            }
            else
            {
                gPopUpManager.AddMethodToBtn(setYesState, null);
            }
        }
    }

    //확인 또는 네를 눌렀을 때 해당 상태를 리턴한다.
    private static void setYesState()
    {
        //테스트용 코드
        //if (returnAction != null)
        //{
        //    returnAction.Invoke();
        //    returnAction = null;
        //}
        Debug.Log("Fucking Yeeees!");
        nowState = yesSceneState;
    }

    //아니요를 눌렀을 때 해당 상태를 리턴한다.

    private static void setNoState()
    {
        Debug.Log("Fucking Nooooo!");
        nowState = noSceneState;
    }

    //--------------------------------------------------------------------------------------------



    //[이하 IAP 사용 관련 함수]=========================================================

    public static bool checkIAPReady()
    {
        //iap가 제대로 돌아가려면 Unity Game Service라는 게 이니셜라이즈되어야 한다.
        if (nowState == (int)IAP_STATE.READY && Samples.Purchasing.Core.InitializeGamingServices.InitializeGamingServices.bInitialized() == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //액티브 된 상태인가?
    public static bool checkIAPActive()
    {
        if (nowState != (int)IAP_STATE.START && nowState != (int)IAP_STATE.READY && nowState != (int)IAP_STATE.INIT && m_StoreController != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //IAP 상품을 등록한다.
    public static void setIAP(List<string> iapList, List<int> kind)
    {
        if (nowState == (int)IAP_STATE.READY)
        {
            builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            for (int i = 0; i < iapList.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(iapList[i]) == false)
                {
                    switch (kind[i])
                    {
                        case (int)IAP_KIND.CONSUMABLE:    //일회성 인 앱 결제
                            Debug.Log(string.Format("IAP Consumable: {0}", iapList[i]));
                            builder.AddProduct(iapList[i], ProductType.Consumable);//, new IDs() {{ iapList[i], GooglePlay.Name }, { iapList[i], AppleAppStore.Name }});
                            break;
                        case (int)IAP_KIND.SUBSCRIPTION:    //구독형 인 앱 결제
                            Debug.Log(string.Format("IAP Subscription: {0}", iapList[i]));
                            builder.AddProduct(iapList[i], ProductType.Subscription);//, new IDs() { { iapList[i], GooglePlay.Name }, { iapList[i], AppleAppStore.Name } });
                            break;
                        default:
                            break;
                    }
                }
            }
            nowState = (int)IAP_STATE.INIT;
        }
    }

    //상태를 이니셜라이즈한다.
    void initProduct()
    {
        if (nowState == (int)IAP_STATE.INIT && builder != null)
        {
            try
            {
                UnityPurchasing.Initialize(this, builder);
                //상태를 액티브로 전환한다.
                nowState = (int)IAP_STATE.ACTIVATE;
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("IAP Initialization Failed: {0}", e.Message));
                nowState = (int)IAP_STATE.READY;
            }
        }
    }

    //상품 이름을 알아온다
    public static string getProductName(string productId)
    {
        string str = "";
#if UNITY_EDITOR
        str = "Name";
#endif
        if (nowState != (int)IAP_STATE.START && nowState != (int)IAP_STATE.READY && nowState != (int)IAP_STATE.INIT)
        {
            if (m_StoreController != null)
            {
                Product product = m_StoreController.products.WithID(productId); //상품 정의
                if (product != null && product.availableToPurchase) //상품이 존재하면서 구매 가능하면
                {
                    str = product.metadata.localizedTitle;
                }
            }
        }
        return str;
    }

    //상품 가격을 알아온다
    public static string getProductPrice(string productId)
    {
        string str = "";
#if UNITY_EDITOR
        str = "Cash";
#endif
        if (nowState != (int)IAP_STATE.START && nowState != (int)IAP_STATE.READY && nowState != (int)IAP_STATE.INIT)
        {
            if (m_StoreController != null)
            {
                Product product = m_StoreController.products.WithID(productId); //상품 정의
                if (product != null && product.availableToPurchase) //상품이 존재하면서 구매 가능하면
                {
                    str = product.metadata.localizedPriceString;
                }
            }
        }
        return str;
    }

    //상품 구매를 요청한다.
    public static int setProductPurchase(string productId, Action setAction = null)
    {
        int reVal = 0; //상품을 결제할 수 있는 상태가 아닙니다.
        if (nowState == (int)IAP_STATE.ACTIVATE)
        {
            returnAction = null;
            if (checkHaveBuyInfo() == false)
            {
                Product product = m_StoreController.products.WithID(productId); //상품 정의
                if (product != null) //상품이 존재하면서 구매 가능하면
                {
                    if (product.availableToPurchase == true)
                    {
                        //상태를 결제 요청으로 변경
                        nowState = (int)IAP_STATE.REQUEST;
                        m_StoreController.InitiatePurchase(productId); //구매가 가능하면 진행
                        //액션 등록
                        returnAction = setAction;
                        reVal = 1;

                    }
                    else //상품이 존재하지 않거나 구매 불가능하면
                    {
                        //Debug.Log("Product could not be Purchase.");
                        reVal = -3;
                        logMsg = "Product could not be Purchase.";
                    }
                }
                else
                {
                    //Debug.Log("Product could not be found.");
                    reVal = -2;
                    logMsg = "Product could not be found.";
                }
            }
            else
            {
                //Debug.Log("처리할 결제가 존재합니다.");
                reVal = -1;
                logMsg = "처리할 결제가 존재합니다.";
            }
        }
        else
        {
            logMsg = $"결제 불가. 현재 상태: {nowState}";
        }
        return reVal;
    }

    public static Func<UniTask> returnUniTask { get; set; }

    public static UniTask setProductPurchaseWithUniTask(string productId, Func<UniTask> setUniTask = null)
    {
        int reVal = 0; //상품을 결제할 수 있는 상태가 아닙니다.
        if (nowState == (int)IAP_STATE.ACTIVATE)
        {
            returnAction = null;
            if (checkHaveBuyInfo() == false)
            {
                Product product = m_StoreController.products.WithID(productId); //상품 정의
                if (product != null) //상품이 존재하면서 구매 가능하면
                {
                    if (product.availableToPurchase == true)
                    {
                        //상태를 결제 요청으로 변경
                        nowState = (int)IAP_STATE.REQUEST;
                        m_StoreController.InitiatePurchase(productId); //구매가 가능하면 진행
                        //액션 등록
                        returnUniTask = setUniTask;
                        reVal = 1;

                    }
                    else //상품이 존재하지 않거나 구매 불가능하면
                    {
                        //Debug.Log("Product could not be Purchase.");
                        reVal = -3;
                        logMsg = "Product could not be Purchase.";
                    }
                }
                else
                {
                    //Debug.Log("Product could not be found.");
                    reVal = -2;
                    logMsg = "Product could not be found.";
                }
            }
            else
            {
                //Debug.Log("처리할 결제가 존재합니다.");
                reVal = -1;
                logMsg = "처리할 결제가 존재합니다.";
            }
        }
        else
        {
            logMsg = $"결제 불가. 현재 상태: {nowState}";
        }

        return new UniTask();
    }

    //등록할 상품이 존재하는 경우, 구매 등록 정보를 전달한다.
    public static receiptStr getBuyInfo()
    {
        receiptStr reVal = new receiptStr();
        reVal.userId = "";
        reVal.token = "";
        reVal.server = "";
        reVal.market = "";
        reVal.sign = "";
        reVal.txid = "";
        reVal.sku = "";
        if (nowState == (int)IAP_STATE.WAITING_REGIST || nowState == (int)IAP_STATE.ACTIVATE)
        {
            if (EncryptedPlayerPrefs.HasKey(iapPrefs.iapTxid) == true)
            {
                reVal.userId = EncryptedPlayerPrefs.GetString(iapPrefs.iapUserId, "");
                reVal.token = EncryptedPlayerPrefs.GetString(iapPrefs.iapToken, "");
                reVal.server = EncryptedPlayerPrefs.GetString(iapPrefs.iapServer, "");
                reVal.market = EncryptedPlayerPrefs.GetString(iapPrefs.iapMarket, "");

                reVal.sign = EncryptedPlayerPrefs.GetString(iapPrefs.iapSign, "");
                reVal.txid = EncryptedPlayerPrefs.GetString(iapPrefs.iapTxid, "");
                reVal.sku = EncryptedPlayerPrefs.GetString(iapPrefs.iapSku, "");
            }
        }
        logMsg = string.Format("Save Purchase - {0}\nSave Receipt - {1}", reVal.sku, reVal.txid);
        return reVal;
    }

    //성공적으로 처리된 경우 데이터를 변경하고, 컨슘을 처리한 다음 액티브 상태로 바꾼다.
    public static void clearIapInfo(int sCode = 0)
    {
        if (sCode == 0)
        {
            string sku = "";
            if (EncryptedPlayerPrefs.HasKey(iapPrefs.iapSku) == true)
            {
                sku = EncryptedPlayerPrefs.GetString(iapPrefs.iapSku, "");
                //컨슘을 처리한다.
                try
                {
                    confirmPendingPurchase(sku);
                }
                catch
                {
                    logMsg = string.Format("No consume product");
                }
            }
        }
        //저장값을 초기화한다.
        clearIapSave();
        //상태를 다시 활성화한다.
        nowState = (int)IAP_STATE.ACTIVATE;
        logMsg = string.Format("Clear iAP save data");
    }

    //현재 처리할 구매 등록 정보를 확인한다.
    private static bool checkHaveBuyInfo()
    {
        bool reVal = false;
        /*
        string txid = "";
        if (EncryptedPlayerPrefs.HasKey(iapPrefs.iapTxid) == true)
        {
            txid = EncryptedPlayerPrefs.GetString(iapPrefs.iapTxid, "");
        }
        if (txid.Length > 0)
        {
            //처리할 결제가 존재한다.
            logMsg = string.Format("처리할 영수증이 남아있습니다({0})", txid);
            reVal = true;
        }
        */
        string sku = "";
        if (EncryptedPlayerPrefs.HasKey(iapPrefs.iapSku) == true)
        {
            sku = EncryptedPlayerPrefs.GetString(iapPrefs.iapSku, "");
        }
        if (sku.Length > 0)
        {
            //처리할 상품이 존재한다.
            logMsg = string.Format("처리할 상품이 남아있습니다({0})", sku);
            reVal = true;
        }
        return reVal;
    }

    //결제 검증 저장값을 초기화한다.
    private static void clearIapSave()
    {
        //상태를 저장한다.
        EncryptedPlayerPrefs.SetString(iapPrefs.iapUserId, "");
        EncryptedPlayerPrefs.SetString(iapPrefs.iapToken, "");
        EncryptedPlayerPrefs.SetString(iapPrefs.iapServer, "");
        EncryptedPlayerPrefs.SetString(iapPrefs.iapMarket, "");

        EncryptedPlayerPrefs.SetString(iapPrefs.iapSign, "");
        EncryptedPlayerPrefs.SetString(iapPrefs.iapTxid, "");
        EncryptedPlayerPrefs.SetString(iapPrefs.iapSku, "");

        PlayerPrefs.Save();
    }

    //결제 검증할 정보를 저장한다.
    private static void saveIapInfo(string receipt = "", string sign = "", string sku = "")
    {
        logMsg = string.Format("Purchase - {0}\nReceipt - {1}", sku, receipt);
        //상태를 저장한다.
        EncryptedPlayerPrefs.SetString(iapPrefs.iapUserId, Convert.ToString(gBase.getUserIndex()));
        EncryptedPlayerPrefs.SetString(iapPrefs.iapToken, gBase.getLoginToken());
        EncryptedPlayerPrefs.SetString(iapPrefs.iapServer, Convert.ToString(gBase.getServerNum()));
        EncryptedPlayerPrefs.SetString(iapPrefs.iapMarket, Convert.ToString((int)LoginManager.Instance.GetMarketType()));

        EncryptedPlayerPrefs.SetString(iapPrefs.iapSign, sign);
        EncryptedPlayerPrefs.SetString(iapPrefs.iapTxid, receipt);
        EncryptedPlayerPrefs.SetString(iapPrefs.iapSku, sku);

        PlayerPrefs.Save();
    }


    //구매 정보를 플레이어프렙스에 등록한다.
    private void registBuyInfo(Product product)
    {
        string receipt = "";
        string sign = "";
        string sku = "";
#if UNITY_ANDROID
        JsonData json = JsonMapper.ToObject(product.receipt);
        try
        {
            string payload = gSystem.getStringFromJson(json, "Payload");
            JsonData json2 = JsonMapper.ToObject(payload);
            receipt = gSystem.getStringFromJson(json2, "json");
            sign = gSystem.getStringFromJson(json2, "signature");
        }
        catch
        {
            receipt = "Fail to get a fucking receipt";
            //Debug.Log("Fail to get a fucking receipt");
        }
        sku = product.definition.id;
        //JsonData json2 = JsonMapper.ToObject(json["Payload"].ToString());
        //string receipt = json2["json"].ToString();
        //string sign = json2["signature"].ToString();
        //string sku = product.definition.id;
#elif UNITY_IOS
        //JsonData json = JsonMapper.ToObject(product.receipt);
        //try
        //{
            //JsonData val = json["Payload"];
            //receipt = val.ToString();
        //}
        //catch
        //{
        //    receipt = "Fail to get a fucking receipt";
            //Debug.Log("Fail to get a fucking receipt");
        //}
        receipt = product.receipt;
        sku = product.definition.id;
        sign = "";
#endif
        //상태를 저장한다.
        saveIapInfo(receipt, sign, sku);
        //상태를 등록 대기 상태로 변경한다.
        nowState = (int)IAP_STATE.WAITING_REGIST;
    }
    //구매 실패 시 이유를 알아내고 필요한 경우 재주문한다.
    private void purchaseFailedCheck(Product product, PurchaseFailureReason failureReason)
    {
        if (failureReason == PurchaseFailureReason.ExistingPurchasePending || failureReason == PurchaseFailureReason.DuplicateTransaction)
        {
            //완료되지 않은 주문의 경우 재주문한다.
            registBuyInfo(product);
        }
        else if (failureReason == PurchaseFailureReason.UserCancelled)
        {
            //상태를 다시 활성화한다.
            nowState = (int)IAP_STATE.ACTIVATE;

        }
        else
        {
            //상태를 다시 활성화한다.
            nowState = (int)IAP_STATE.ACTIVATE;
        }
    }

    //구매 완료 시 컨슘을 처리한다.
    private static void confirmPendingPurchase(string productID)
    {
        Product product = m_StoreController.products.WithID(productID);
        if (product != null)
        {
            if (product.definition.type == ProductType.Consumable)
            {
                m_StoreController.ConfirmPendingPurchase(product);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------



    //[이하 IDetailedStoreListener 리스너 함수]=========================================================

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        m_StoreController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null)
        {
            errorMessage += $" More details: {message}";
        }
        Debug.Log(errorMessage);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        Product product = args.purchasedProduct;

        Debug.Log($"Purchase Complete - Product: {product.definition.id}");
        logMsg = $"Purchase Complete - Product: {product.definition.id}";

        //추가 처리 함수
        registBuyInfo(product);

        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.

        if (product.definition.type == ProductType.Consumable)
        {
            return PurchaseProcessingResult.Pending;
        }
        else
        {
            return PurchaseProcessingResult.Complete;
        }
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        //Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
        //    $" Purchase failure reason: {failureDescription.reason}," +
        //    $" Purchase failure details: {failureDescription.message}");
        logMsg = $"Purchase failed - Product: '{product.definition.id}'," + $" Purchase failure reason: {failureDescription.reason}," + $" Purchase failure details: {failureDescription.message}";

        //추가 처리 함수
        purchaseFailedCheck(product, failureDescription.reason);
    }

    //--------------------------------------------------------------------------------------------------------------------------





}
