using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// Assets/@Resources/ 아래의 기본 Address를 상대 주소로 정리합니다.
/// 예: Assets/@Resources/GameObject/Sword.prefab -> GameObject/Sword.prefab
/// </summary>
[InitializeOnLoad]
public static class AddressableAddressTool
{
    private const string ResourceRoot = "Assets/@Resources/";
    private static bool normalizationScheduled;

    static AddressableAddressTool()
    {
        AddressableAssetSettings.OnModificationGlobal += OnAddressablesModified;
    }

    [MenuItem("Tools/Addressables/Normalize Resource Addresses")]
    public static void NormalizeResourceAddresses()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("Addressables 설정을 찾지 못했습니다.");
            return;
        }

        int changedCount = Normalize(settings);
        if (changedCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"Addressables 주소 정리 완료: {changedCount}개 변경");
    }

    private static void OnAddressablesModified(
        AddressableAssetSettings settings,
        AddressableAssetSettings.ModificationEvent modificationEvent,
        object eventData)
    {
        if (normalizationScheduled ||
            (modificationEvent != AddressableAssetSettings.ModificationEvent.EntryCreated &&
             modificationEvent != AddressableAssetSettings.ModificationEvent.EntryAdded &&
             modificationEvent != AddressableAssetSettings.ModificationEvent.EntryMoved))
        {
            return;
        }

        // 등록 작업이 끝난 뒤 정리합니다. 직접 지정한 커스텀 주소는 유지합니다.
        normalizationScheduled = true;
        EditorApplication.delayCall += () =>
        {
            normalizationScheduled = false;
            if (settings != null && Normalize(settings) > 0)
            {
                AssetDatabase.SaveAssets();
            }
        };
    }

    private static int Normalize(AddressableAssetSettings settings)
    {
        int changedCount = 0;
        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null || group.ReadOnly)
            {
                continue;
            }

            // 주소 변경 이벤트가 발생해도 순회 중인 컬렉션은 유지합니다.
            var entries = new System.Collections.Generic.List<AddressableAssetEntry>(group.entries);
            foreach (AddressableAssetEntry entry in entries)
            {
                if (entry == null || entry.ReadOnly ||
                    !entry.AssetPath.StartsWith(ResourceRoot, StringComparison.Ordinal) ||
                    !entry.address.StartsWith(ResourceRoot, StringComparison.Ordinal))
                {
                    continue;
                }

                Undo.RecordObject(group, "Normalize Addressable Address");
                entry.SetAddress(entry.AssetPath.Substring(ResourceRoot.Length));
                changedCount++;
            }
        }

        return changedCount;
    }
}
