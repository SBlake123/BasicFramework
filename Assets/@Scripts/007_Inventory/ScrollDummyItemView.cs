using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Procedural placeholder until scroll sprites/prefabs are authored.
public class ScrollDummyItemView : MonoBehaviour
{
    public static UniTask<GameObject> Create(string key, Transform parent)
    {
        if (!ScrollDummyRawData.TryViewKey(key, out int id)) return ObjectPool.Instance.PopFromPool(key, parent);
        var raw = ScrollDummyRawData.Find(id);
        var obj = new GameObject(key, typeof(RectTransform), typeof(Image), typeof(ScrollDummyItemView));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.sizeDelta = new Vector2(95, 105);
        var image = obj.GetComponent<Image>();
        image.color = new Color(.88f, .76f, .52f);
        image.raycastTarget = false;
        var label = new GameObject("Scroll Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(obj.transform, false);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        var source = parent.GetComponentInParent<Inventory_000_Base>();
        if (source != null && source.damageMainTxt != null) label.font = source.damageMainTxt.font;
        label.text = $"{(raw.equipmentType == EquipmentType.Weapon ? "W" : "S")}\n{raw.successRate}%";
        label.fontSize = 28;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(.2f, .13f, .08f);
        label.raycastTarget = false;
        return UniTask.FromResult(obj);
    }

    public static void Release(GameObject obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<ScrollDummyItemView>() == null) { ObjectPool.Instance.PushToPool(obj); return; }
        obj.transform.SetParent(null);
        obj.SetActive(false);
        Destroy(obj);
    }
}
