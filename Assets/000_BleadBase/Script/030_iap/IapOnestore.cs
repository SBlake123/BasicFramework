//#define SERVICE_ONESTORE

using UnityEngine;
#if SERVICE_ONESTORE
using OneStore;
#endif


public class IapOnestore : MonoBehaviour {

    //public static string[] inapp_products = new string[] { "one.ozaak.ar.cash001", "one.ozaak.ar.cash003", "one.ozaak.ar.cash005", "one.ozaak.ar.cash010", "one.ozaak.ar.cash030", "one.ozaak.ar.cash050", "one.ozaak.ar.cash100", "one.ozaak.ar.daily006", "one.ozaak.ar.daily012", "one.ozaak.ar.package001", "one.ozaak.ar.package002", "one.ozaak.ar.package003" };
    //public static string[] inapp_name = new string[] { "룬 80", "룬 250", "룬 440", "룬 1000", "룬 3600", "룬 7000", "룬 16000", "월정액 40", "월정액 120", "시작 패키지", "강화 특가 패키지", "크리스타냐 특가팩" };
    //public static string[] price = new string[] { "1,100원", "3,300원", "5,500원", "11,000원", "33,000원", "55,000원", "110,000원", "6,600원", "13,200원", "5,900원", "8,900원", "14,900원" };

    public static string[] inapp_products = new string[0];
    public static string[] inapp_name = new string[0];
    public static string[] price = new string[0];

    //Main화면 > 상점 > 룬 구매 > 해당 상품 터치하여 구매


    public static string inapp_type = "inapp";

    private string devPayload = "this is test payload!";
    public static string paylord = "This is Ozaak lt In-App Purchase paylord";
    public static string base64EncodedPublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCwO7f1zv0EO6kZIjh835nEJ52xFpjY9iq/N2WSXBimaN623WDEUDOF8TX8DMQ98Uw56L52Mwf0zRDaxEbnEhO2MEPJIGPh9tLwETCWad7AIpUtORg0oTM4WL5n4+XNQClyvlGx6yfE1liLTlrHp/VT3yXLwhrPjJ2KSYTQN69VxwIDAQAB";

    public static bool cReady;
    public static bool bBuyItemOrder;

    public static double iapTimer;

    void Start()
    {
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gBase.setIapInit();
        bBuyItemOrder = true;
        iapTimer = 0.0f;
#if !UNITY_EDITOR && SERVICE_ONESTORE
        cReady = false;
        if (gBase.gameVersion.marketType == (int)ENUM_MARKET_TYPE.ONESTORE)
        {
            IapOnestore.connectService();
        }
#else
        cReady = true;
#endif
    }

    void Update()
    {
        if (iapTimer < 20.0f)
        {
            iapTimer += Time.deltaTime;
        }
    }

    public static void connectService()
    {
#if SERVICE_ONESTORE
        Onestore_IapCallManager.connectService(base64EncodedPublicKey);
#endif
    }

    public static void isBillingSupported()
    {
#if SERVICE_ONESTORE
        Onestore_IapCallManager.isBillingSupported();
#endif
    }

    public static void getPurchases()
    {
#if SERVICE_ONESTORE
        Onestore_IapCallManager.getPurchases();
#endif
    }

    public void getProductDetails()
    {
#if SERVICE_ONESTORE
        Onestore_IapCallManager.getProductDetails(inapp_products, inapp_type);
#endif
    }

    public static void buyProduct(int num)
    {
#if !UNITY_EDITOR && SERVICE_ONESTORE
        if (cReady == true && bBuyItemOrder == true)
        {
            if (num < 0 || num > inapp_products.Length - 1)
                return;
            else
            {
                //bBuyItemOrder = false;
                Onestore_IapCallManager.buyProduct(inapp_products[num], inapp_type, paylord);
            }
        }
#endif
    }

    public static void consume(string pId)
    {
        string inapp_json = pId;
        if (inapp_json.Length > 0)
        {
#if SERVICE_ONESTORE
            Onestore_IapCallManager.consume(inapp_json);
#endif
        }
        else
        {
        }
    }
}
