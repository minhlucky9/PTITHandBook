using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class CoinLootInteraction : LootInteraction
{
    [Header("UI Controller")]
    public UIAnimationController uiController;

    private static CoinLootInteraction currentActiveCoin;
    private static Coroutine deactivateCoroutine;

    string questId = "";
    public void SetupCoinMinigame(string questId)
    {
        this.questId = questId;
    }

    public override void OnEnterCollider()
    {

        base.OnEnterCollider();
        HandleUIAnimation();
        if (questId != "")
        {
            CollectQuestManager.instance.MarkCoinAsCollected(this);
            CollectQuestManager.instance.OnCollectQuestChange(questId);
        }
        AudioManager.instance.PlayCorrectSound();
        gameObject.SetActive(false);
    }

    private void HandleUIAnimation()
    {
        // Kiểm tra nếu không có UIController được gán
        if (uiController == null)
        {
            Debug.LogWarning($"UIAnimationController is not assigned on {gameObject.name}");
            return;
        }

        // Nếu đang có coin khác đang active
        if (currentActiveCoin != null && currentActiveCoin != this)
        {
            // Deactivate UI của coin trước đó
            if (currentActiveCoin.uiController != null)
            {
                currentActiveCoin.uiController.Deactivate();
            }

            // Hủy coroutine auto-deactivate cũ nếu có
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }
        }

        // Set coin hiện tại là coin active
        currentActiveCoin = this;

        // Activate UI của coin hiện tại
        uiController.Activate();

        // Bắt đầu coroutine để tự động deactivate sau 15 giây
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }
        deactivateCoroutine = StartCoroutine(AutoDeactivateUI(uiController));
    }

    private IEnumerator AutoDeactivateUI(UIAnimationController uiController)
    {
        // Đợi 15 giây
        yield return new WaitForSeconds(5f);
        Debug.Log("aaa");
        uiController.Deactivate();
        // Deactivate UI nếu coin này vẫn là coin active hiện tại
        if (uiController != null)
        {
            uiController.Deactivate();
            currentActiveCoin = null;
            deactivateCoroutine = null;
        }
    }

    // Optional: Thêm method để force deactivate UI (có thể gọi từ nơi khác)
    public void ForceDeactivateUI()
    {
        if (currentActiveCoin == this)
        {
            if (uiController != null)
            {
                uiController.Deactivate();
            }

            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }

            currentActiveCoin = null;
        }
    }

    // Clean up khi object bị destroy
    private void OnDestroy()
    {
        if (currentActiveCoin == this)
        {
            currentActiveCoin = null;
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }
        }
    }
}
