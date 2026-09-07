using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "BasicFramework/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Classification")]
    [SerializeField] private ItemCategory category;
    [SerializeField] private EquipmentSlot equipmentSlot = EquipmentSlot.None;

    [Header("Inventory")]
    [SerializeField] private bool canStack = true;
    [SerializeField, Min(1)] private int maxStackAmount = 99;

    [Header("Consumable")]
    [SerializeField, Min(0)] private int healAmount;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public ItemCategory Category => category;
    public EquipmentSlot EquipSlot => equipmentSlot;
    public bool CanStack => canStack;
    public int MaxStackAmount => maxStackAmount;
    public int HealAmount => healAmount;
    public bool CanBeEquipped => category == ItemCategory.Equipment;

    private void OnValidate()
    {
        if (category != ItemCategory.Equipment)
        {
            equipmentSlot = EquipmentSlot.None;
        }

        if (category == ItemCategory.Equipment)
        {
            canStack = false;
            maxStackAmount = 1;
        }

        if (!canStack)
        {
            maxStackAmount = 1;
        }
    }
}
