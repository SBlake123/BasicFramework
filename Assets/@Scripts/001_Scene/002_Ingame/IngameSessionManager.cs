using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns one raid session. It receives meaningful player changes and creates
/// checkpoints; it deliberately does not own player movement or combat logic.
/// </summary>
public class IngameSessionManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private string mapId = "03_Ingame";
    [SerializeField, Min(1f)] private float checkpointIntervalSeconds = 30f;

    public bool IsRaidActive { get; private set; }
    public event Action<RaidSessionData> OnRaidCheckpointed;
    public event Action OnRaidExtracted;
    public event Action OnRaidFailed;

    private void Awake()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
    }

    public void BeginRaid()
    {
        if (IsRaidActive)
        {
            return;
        }

        PlayerDataManager.Instance.LoadOrCreate();
        PlayerDataManager.Instance.BeginRaid(mapId);
        IsRaidActive = true;

        CapturePlayerTransform();
        CheckpointLoop().Forget();
    }

    /// <summary>
    /// Call this from future PlayerVitals when health or stamina changes.
    /// </summary>
    public void ReportPlayerVitals(float health, float stamina)
    {
        if (!IsRaidActive)
        {
            return;
        }

        PlayerDataManager.Instance.UpdateRaidVitals(health, stamina);
        PlayerDataManager.Instance.RequestCheckpoint();
    }

    /// <summary>
    /// Call this from future inventory code after loot, drop, or equipment changes.
    /// </summary>
    public void ReportRaidInventory(IReadOnlyList<ItemStackData> items)
    {
        if (!IsRaidActive)
        {
            return;
        }

        PlayerDataManager.Instance.ReplaceRaidInventory(items);
        PlayerDataManager.Instance.RequestCheckpoint();
    }

    public async UniTask CompleteExtraction()
    {
        if (!IsRaidActive)
        {
            return;
        }

        CapturePlayerTransform();
        PlayerDataManager.Instance.CommitExtraction();
        IsRaidActive = false;
        OnRaidExtracted?.Invoke();
        await UniTask.CompletedTask;
    }

    public async UniTask CompleteDeath()
    {
        if (!IsRaidActive)
        {
            return;
        }

        CapturePlayerTransform();
        PlayerDataManager.Instance.CommitDeath();
        IsRaidActive = false;
        OnRaidFailed?.Invoke();
        await UniTask.CompletedTask;
    }

    private async UniTask CheckpointLoop()
    {
        while (IsRaidActive)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(checkpointIntervalSeconds), cancellationToken: destroyCancellationToken);

            if (!IsRaidActive)
            {
                break;
            }

            CapturePlayerTransform();
            PlayerDataManager.Instance.SaveCheckpointNow();
            OnRaidCheckpointed?.Invoke(PlayerDataManager.Instance.ActiveRaid);
        }
    }

    private void CapturePlayerTransform()
    {
        if (player == null)
        {
            return;
        }

        PlayerDataManager.Instance.UpdateRaidPosition(player.transform.position);
    }
}
