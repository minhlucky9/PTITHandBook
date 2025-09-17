using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerController;  // nếu bạn có cần tắt/bật input người chơi

public class ObjectController : TalkInteraction
{
    [Header("UI Sequence Settings")]
    [Tooltip("Danh sách các panel UI sẽ hiển thị tuần tự")]
    public List<GameObject> uiSequencePanels;
    [Tooltip("Phím bấm để chuyển panel (mặc định E)")]
    public KeyCode advanceKey = KeyCode.E;

    private int sequenceIndex = 0;

    public override void Awake()
    {
        base.Awake();
        interactableText = "Press E to interact";
    }

    public override void Interact()
    {
        base.Interact();           // dừng movement, xoay nhân vật…
        sequenceIndex = 0;
        StartCoroutine(UISequenceCoroutine());
    }

    private IEnumerator UISequenceCoroutine()
    {
        // Nếu cần, tắt input của người chơi trong lúc show UI
        // PlayerManager.instance.DeactivateController();

        while (sequenceIndex < uiSequencePanels.Count)
        {
            GameObject panel = uiSequencePanels[sequenceIndex];
            panel.SetActive(true);

            // Đợi tới khi nhấn phím advanceKey
            yield return new WaitUntil(() => Input.GetKeyDown(advanceKey));

            panel.SetActive(false);
            sequenceIndex++;
        }

        StopInteract();  // cho phép nhân vật di chuyển lại

        // Nếu đã tắt input, bật lại
        // PlayerManager.instance.ActivateController();
    }
}
