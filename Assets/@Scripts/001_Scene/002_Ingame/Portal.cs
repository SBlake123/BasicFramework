using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private float escapeDelay = 6f;

    private readonly HashSet<Collider> playerColliders = new();
    private float elapsedTime;
    private bool isEscaping;

    public event Func<UniTask> EscapeSuccess;

    public async UniTask Init()
    {
        EscapeCheck().Forget();
    }

    public async UniTask EscapeCheck()
    {
        try
        {

            while (true)
            {
                if (isEscaping) return;

                playerColliders.RemoveWhere(col => col == null || !col.enabled || !col.gameObject.activeInHierarchy);

                if (playerColliders.Count > 0)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    IngameUIManager.Instance.escapeSlider.normalizedValue = Mathf.Clamp01(elapsedTime / escapeDelay);

                    if (elapsedTime >= escapeDelay)
                    {
                        isEscaping = true;
                        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(false);
                        await Escape();
                        return;
                    }
                }
                else
                {
                    elapsedTime = 0f;
                    IngameUIManager.Instance.escapeSlider.value = 0f;
                    IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(false);
                }

                await UniTask.NextFrame();
            }
        }
        catch (NullReferenceException)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<Player>();
        if (player == null) return;
        if (Array.IndexOf(player.playerHitBoxArr, other) < 0) return;

        playerColliders.Add(other);
        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        playerColliders.Remove(other);
        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(false);


        if (playerColliders.Count == 0) elapsedTime = 0f;
    }

    private async UniTask Escape()
    {
        EscapeSuccess?.Invoke();
        // Å»Ãâ Ã³¸®
    }

    private void OnDisable()
    {
        playerColliders.Clear();
        elapsedTime = 0f;
        isEscaping = false;
    }
}
