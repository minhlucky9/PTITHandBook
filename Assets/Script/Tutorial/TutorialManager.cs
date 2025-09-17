using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static MouseManager;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Danh sách các prefab UIAnimationController (sort)")]
    [SerializeField] private List<UIAnimationController> steps;

    [Header("UI prefab container")]
    [SerializeField] private Transform uiContainer;
    public GameObject TutorialField;
    public GameObject LoadingScreen;
    public GameObject tutorialFieldInstance;

    [Header("Delay prefab time")]
    [SerializeField] private float delayBetweenSteps = 1f;

    [Header("For Scene 17 & 18")]
    public Button button17;
    public Button button18;

    private int currentIndex = -1;
    public UIAnimationController currentUI;
    public bool isRunning = false;
    bool isUpdatingTutorior = false;
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //update tutorior button event


        UpdateButtonNextTutoriorEvent(button17);
        UpdateButtonNextTutoriorEvent(button18);
    }

    public void UpdateButtonNextTutoriorEvent(Button button, bool nextStep = true)
    {
        UnityAction showNextTutoriorOnButton = () =>
        {
            if(nextStep)
            {
                ShowNextStepDelayed();
            } else
            {
                if (currentUI != null)
                    Destroy(currentUI.gameObject);
            }
            
        };
        button.onClick.AddListener(showNextTutoriorOnButton);
        button.onClick.AddListener(delegate
        {
            button.onClick.RemoveListener(showNextTutoriorOnButton);
        });
    }

    private void Start()
    {
        if (GlobalResponseData_Login.GlobalResponseData.FirstTimeQuest == 0)
        {
            currentIndex = -1;
            LoadingScreen.SetActive(true);
            StartCoroutine(StartTutorial());
        }
        Debug.Log(GlobalResponseData_Login.GlobalResponseData.FirstTimeQuest == 0);
    }

    private IEnumerator StartTutorial()
    {
        yield return new WaitUntil(() => TelePort.instance != null);
        LoadingScreen.SetActive(false);
        isRunning = true;
        tutorialFieldInstance = Instantiate(TutorialField);
        TelePort.instance.ReturnToTutorial();
        MouseManager.instance.permission = MousePermission.None;
        ShowNextStep();
    }

    public void ShowNextStep()
    {
        if (!isRunning) return;

        if (currentUI != null)
            Destroy(currentUI.gameObject);





        currentIndex++;
        if (currentIndex < steps.Count)
        {
            currentUI = Instantiate(steps[currentIndex], uiContainer);
            currentUI.Activate();
          
        //    PlayerManager.instance.DeactivateController();
        }
        else
        {
            EndTutorial();
        }
    }

    public void ShowNextStepDelayed()
    {
        if (!isRunning) return;

        if (isUpdatingTutorior) return;
        isUpdatingTutorior = true;

        StartCoroutine(ShowNextStepCoroutine());
    }

    private IEnumerator ShowNextStepCoroutine()
    {
    //   PlayerManager.instance.DeactivateController();
        yield return new WaitForSeconds(delayBetweenSteps);

        if (currentUI != null)
            Destroy(currentUI.gameObject);

        currentIndex++;
        isUpdatingTutorior = false;
        if (currentIndex < steps.Count)
        {
            currentUI = Instantiate(steps[currentIndex], uiContainer);
            currentUI.Activate();
          
        }
        else
        {
            EndTutorial();
        }
    }

    private void EndTutorial()
    {
        isRunning = false;
        GlobalResponseData_Login.GlobalResponseData.FirstTimeQuest = 1;
        PlayerManager.instance.ActivateController();
        MouseManager.instance.permission = MousePermission.All;
        Debug.Log("[Tutorial] Đã hoàn thành.");
    }

}
