using Interaction;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeSceneManager : SubSceneGameManager
{
    [Header("UI Timer")]
    public TextMeshProUGUI timerText;
    private float timeRemaining;
    private Coroutine uiTimerCoroutine;
    public UIAnimationController Fail;
    public UIAnimationController Exit;
    public UIAnimationController Success;
    public GameObject LoadingScreen;

    public CoinLootInteraction[] targets;
    private const float QuestTimeoutSeconds = 300f;

    private GameObject __targetNPC;
    private MinigameDataSO __minigamedata;


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            OnPauseButtonPressed();
        }
    }

    public override void InitGame(GameObject targetNPC, MinigameDataSO minigameData, string sceneName)
    {

        __targetNPC = targetNPC;
        __minigamedata = minigameData;
        base.InitGame(targetNPC, minigameData, sceneName);

        timeRemaining = QuestTimeoutSeconds;
        if (uiTimerCoroutine != null) StopCoroutine(uiTimerCoroutine);
        uiTimerCoroutine = StartCoroutine(UpdateTimerUI());

        foreach (CoinLootInteraction coin in targets)
        {
            coin.SetupCoinMinigame(minigameData.minigameId);
        }

        //setup collect quest
        CollectQuest collectQuest = new CollectQuest();
        collectQuest.numberToCollect = targets.Length;
        collectQuest.OnFinishQuest = () => {
            targetNPC.SendMessage("OnQuestMinigameSuccess");
            //
            StartCoroutine(ExitScene(3f));
        };
        CollectQuestManager.instance.collectQuests.Add(minigameData.minigameId, collectQuest);
        //  StartCoroutine(QuestTimeoutCoroutine(minigameData.questId, minigameData.minigameId, targetNPC));
    }

    private IEnumerator QuestTimeoutCoroutine(string questId, string minigameId, GameObject targetNPC)
    {
        yield return new WaitForSeconds(QuestTimeoutSeconds);

        if (CollectQuestManager.instance.collectQuests.ContainsKey(minigameId))
        {


            // reset quest về HAVE_QUEST
            GameManager.QuestManager.instance.UpdateQuestStep(
               QuestState.CAN_START,
                questId
            );

            targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);

            CollectQuestManager.instance.collectQuests.Remove(minigameId);
            // rời scene ngay

            PlayerManager.instance.DeactivateController();
            CameraHandle cameraHandle = FindObjectOfType<CameraHandle>();
            if (cameraHandle != null)
            {
                cameraHandle.ResetCameraVelocity();
            }
            Fail?.Activate();

        }
    }

    public void GameExit()
    {
        StartCoroutine(ExitScene(3f));
    }

    public void GameSuccess()
    {
        PlayerManager.instance.DeactivateController();
        CameraHandle cameraHandle = FindObjectOfType<CameraHandle>();
        if (cameraHandle != null)
        {
            cameraHandle.ResetCameraVelocity();
        }
        Success?.Activate();
    }

    public void ExitGame()
    {
        GameManager.QuestManager.instance.UpdateQuestStep(
              QuestState.CAN_START,
           __minigamedata.questId
           );
        __targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);

        CollectQuestManager.instance.collectQuests.Remove(__minigamedata.minigameId);

        StopCoroutine(uiTimerCoroutine);
        PauseMenuUI.Instance.Resume();
        StartCoroutine(ExitScene(3f));
    }

    private IEnumerator UpdateTimerUI()
    {
        // mỗi frame cập nhật
        while (timeRemaining > 0f)
        {
            // hiển thị mm:ss
            int m = Mathf.FloorToInt(timeRemaining / 60f);
            int s = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{m:00}:{s:00}";

            // giảm
            if (!PauseMenuUI.GameIsPaused)
            {
                timeRemaining -= Time.deltaTime;
            }
            yield return null;
        }

        // khi về 0
        timerText.text = "00:00";

        // Timeout xảy ra - gọi GameFail
        if (CollectQuestManager.instance.collectQuests.ContainsKey(__minigamedata.minigameId))
        {
            // reset quest về HAVE_QUEST
            GameManager.QuestManager.instance.UpdateQuestStep(
               QuestState.CAN_START,
                __minigamedata.questId
            );

            __targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);

            CollectQuestManager.instance.collectQuests.Remove(__minigamedata.minigameId);

            PlayerManager.instance.DeactivateController();
            Fail?.Activate();
            StartCoroutine(ExitScene(3f));
        }
    }

    IEnumerator ExitScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.UnloadSceneAsync(sceneName);
        LoadingScreen.SetActive(false);
        PlayerManager.instance.ActivateController();
    }

    public void OnPauseButtonPressed()
    {
        PauseMenuUI.Instance.Pause();
        PlayerManager.instance.DeactivateController();

        // Reset camera velocity để ngăn camera tiếp tục xoay do quán tính
        CameraHandle cameraHandle = FindObjectOfType<CameraHandle>();
        if (cameraHandle != null)
        {
            cameraHandle.ResetCameraVelocity();
        }

        Exit?.Activate();
    }

    public void OnResumeButtonPressed()
    {

        Exit?.Deactivate();
        PlayerManager.instance.ActivateController();
        PauseMenuUI.Instance.Resume();
    }
}
