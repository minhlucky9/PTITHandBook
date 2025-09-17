using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Interaction;    // để dùng Quest, QuestState
using System;
using static QuestLogManager;

public class QuestLogItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image questImage;
    [SerializeField] private GameObject requirementNotMetUI;
    [SerializeField] private Transform stateContainer; // chứa các icon/status child

    private Transform npcTarget;

    private Quest boundQuest;
    private Action onStateChangedHandler;

    private QuestState _lastState;

    public void Bind(int index, Quest quest)
    {
        Unbind();

        boundQuest = quest;
        _lastState = boundQuest.state;


        // 1. Text + description (clamp index)
        nameText.text = $"Nhiệm vụ {index + 1}";
        UpdateDescription();

        // 2. Ảnh
        questImage.sprite = boundQuest.info.questIcon;

        // 3. REQUIREMENTS_NOT_MET
        requirementNotMetUI.SetActive(boundQuest.state == QuestState.REQUIREMENTS_NOT_MET);

        // 4. Hiển thị state icon đúng
        UpdateStateIcons(boundQuest.state);

        AddGotoNPCButtons();

        // 5. Đăng ký lắng nghe khi state thay đổi
        onStateChangedHandler = () =>
        {
            UpdateDescription();
            requirementNotMetUI.SetActive(boundQuest.state == QuestState.REQUIREMENTS_NOT_MET);
            UpdateStateIcons(boundQuest.state);
            if (QuestLogManager.instance != null)
                QuestLogManager.instance.RefreshCurrentQuestUI();

           

        };
        boundQuest.OnStateChanged += onStateChangedHandler;
    }

    private void UpdateDescription()
    {
        int count = boundQuest.info.questSteps.Count;
        if (count > 0 && boundQuest.state != QuestState.FINISHED)
        {
            int idx = Mathf.Clamp(boundQuest.currentQuestStepIndex, 0, count - 1);
            descText.text = boundQuest.info.questSteps[idx].stepDescription;
        }
        else
        {
            descText.text = "Nhiệm vụ đã hoàn thành";
        }
    }

    private void UpdateStateIcons(QuestState state)
    {
        int idx = (int)state;
        for (int i = 0; i < stateContainer.childCount; i++)
        {
            stateContainer.GetChild(i).gameObject.SetActive(i == idx);
        }
    }

    private void Unbind()
    {
        if (boundQuest != null && onStateChangedHandler != null)
        {
            boundQuest.OnStateChanged -= onStateChangedHandler;
            onStateChangedHandler = null;
        }
    }

    public void SetNPC(Transform t)
    {
        npcTarget = t;
    }

    private void AddGotoNPCButtons()
    {
        // chỉ 3 icon đầu, clear listener cũ rồi add mới
        int max = Mathf.Min(3, stateContainer.childCount);
        for (int i = 0; i < max; i++)
        {
            var icon = stateContainer.GetChild(i);
            var btn = icon.GetComponent<Button>();
            if (btn == null) btn = icon.gameObject.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            if (TutorialManager.Instance.isRunning) TutorialManager.Instance.UpdateButtonNextTutoriorEvent(btn, false);
            btn.onClick.AddListener(() =>
            {
                if (npcTarget != null && MoveToNPC.instance != null)
                {
                    MoveToNPC.instance.npcTarget = npcTarget;
                    MoveToNPC.instance.StartAutoMove();
                    MouseManager.instance.CloseQuestUI();
                    
                }
            });
        }
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
