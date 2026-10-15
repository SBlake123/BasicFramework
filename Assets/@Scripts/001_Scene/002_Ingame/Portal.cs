using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private float escapeDelay = 6f;

    private readonly HashSet<Collider> playerColliders = new();
    private float elapsedTime;
    private bool isEscaping;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Player>() == null) return;

        playerColliders.Add(other);
        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        playerColliders.Remove(other);
        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(false);


        if (playerColliders.Count == 0) elapsedTime = 0f;
    }

    private void Update()
    {
        if (isEscaping || playerColliders.Count == 0) return;

        //IngameUIManager.Instance.

        elapsedTime += Time.unscaledDeltaTime;
        IngameUIManager.Instance.escapeSlider.value = elapsedTime / escapeDelay;
        if (elapsedTime < escapeDelay) return;
        IngameUIManager.Instance.escapeSliderWindow.gameObject.SetActive(false);

        isEscaping = true;
        Escape();
    }

    private void Escape()
    {
        // Å»Ãâ Ã³¸®
    }

    private void OnDisable()
    {
        playerColliders.Clear();
        elapsedTime = 0f;
        isEscaping = false;
    }
}
