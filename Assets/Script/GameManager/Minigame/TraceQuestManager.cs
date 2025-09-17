// TraceQuestManager.cs
using Interaction;
using Interaction.Minigame;
using PlayerController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraceQuestManager : MonoBehaviour
{
    public static TraceQuestManager instance;

    private GameObject targetNPC;
    private TraceEventSO traceEvent;
    private int currentIndex = 0;
    public GameObject currentObject;
    private TraceQuest traceQuest;
    private TraceObjectInteraction DragAndDropPuzzle;

    [Header("UI Progress")]
    [SerializeField] private UIAnimationController progressUI;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Slots & UI")]
    public List<InventorySlot> dropSlots;
    private int correctCount;

    public List<Sprite> sprites;

    private Coroutine CollectTimerRoutine;
    private float timeRemaining;
    private const float COLLECT_DURATION = 1080f;


    private List<ItemInitialState> initialStates = new List<ItemInitialState>();

    [Header("SlidingPuzzle Minigame")]
    public Texture2D SlidingpuzzleTexture_1;
    public Texture2D SlidingpuzzleTexture_2;
    public Texture2D SlidingpuzzleTexture_3;

    [Header("DragAndDrop Minigame")]
    public Texture2D puzzleTexture_1;
    public Texture2D puzzleTexture_2;

    public Texture2D[] ptitImages;
    private List<int> showImages;

    [Header("PassWordMinigame_1")]
    [SerializeField] UIAnimationController passwordContainer_1;
    [SerializeField] Transform hintContainer;
    [SerializeField] TMP_InputField passwordInput_1;
    [SerializeField] Button checkBtn_1;
    [SerializeField] TextMeshProUGUI titleTxt_1;
    [SerializeField] TextMeshProUGUI feedbackTxt_1;
    [SerializeField] Button closeBtnPass1;

    [Header("PassWordMinigame_2")]
    [SerializeField] UIAnimationController passwordContainer_2;
    [SerializeField] TMP_InputField passwordInput_2;
    [SerializeField] TMP_Text password2;
    [SerializeField] Button checkBtn_2;
    [SerializeField] TextMeshProUGUI titleTxt_2;
    [SerializeField] TextMeshProUGUI feedbackTxt_2;
    [SerializeField] Button closeBtnPass2;

    [Header("PassWordMinigame_3")]
    [SerializeField] UIAnimationController passwordContainer_3;
    [SerializeField] TMP_InputField passwordInput_3;
    [SerializeField] Button checkBtn_3;
    [SerializeField] TextMeshProUGUI titleTxt_3;
    [SerializeField] TextMeshProUGUI feedbackTxt_3;
    private GameObject PuzzleObject;
    public GameObject PuzzelModel;

    [System.Serializable]
    public class ItemInitialState
    {
        public DraggableItem item;
        public Transform originalParent;
        public Vector3 originalPosition;
        public bool wasEnabled;
    }

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        showImages = new List<int>();
    }

    public void InitTraceQuest(GameObject targetNPC, TraceEventSO traceEvent)
    {
        this.targetNPC = targetNPC;
        this.traceEvent = traceEvent;
        currentIndex = 0;
        showImages = new List<int>();
        timeRemaining = COLLECT_DURATION;

        // ===== Khởi tạo TraceQuest =====
        traceQuest = new TraceQuest();
        traceQuest.numberToTrace = traceEvent.traceObjects.Count;
               // Khi xong hết thì báo NPCController hoàn thành quest
        traceQuest.OnFinishQuest = () =>
        {
            targetNPC.SendMessage("OnQuestMinigameSuccess");
            ConservationManager.instance.StarContainer.Deactivate();
            if (CollectTimerRoutine != null)
            {
                StopCoroutine(CollectTimerRoutine);
                CollectTimerRoutine = null;
            }
            ConservationManager.instance.timerContainer.Deactivate();
        };
               // Hiển thị bar giống CollectQuestManager
        ConservationManager.instance.StarContainer.Activate();
        ConservationManager.instance.StarText.text = "Số vật phẩm đã thu thập: " + $"0/{traceQuest.numberToTrace}";
        SpawnNextObject();
        Invoke(nameof(StartCollectTimer), 0.5f);
    }

    private void StartCollectTimer()
    {
        // Tránh gọi nhiều lần
        if (CollectTimerRoutine != null)
        {
            StopCoroutine(CollectTimerRoutine);
        }

        CollectTimerRoutine = StartCoroutine(CollectCountdown(timeRemaining));
    }

    private IEnumerator CollectCountdown(float duration)
    {
        float t = duration;
        ConservationManager.instance.timerContainer.Activate();


        while (t > 0f)
        {
            // tính phút và giây
            int minutes = (int)(t / 60);
            int seconds = (int)(t % 60);
            // format “MM:SS”
            ConservationManager.instance.timerText.text = $"{minutes:00}:{seconds:00}";

            t -= Time.deltaTime;
            timeRemaining = t;
            yield return null;
        }

        // khi hết giờ
        ConservationManager.instance.timerText.text = "00:00";
        OnCollectTimerExpired();
    }

    private void OnCollectTimerExpired()
    {
        // dừng coroutine nếu còn chạy
        if (CollectTimerRoutine != null)
        {
            StopCoroutine(CollectTimerRoutine);
            CollectTimerRoutine = null;
        }

        // ẩn UI timer
        ConservationManager.instance.timerContainer.Deactivate();
        ConservationManager.instance.StarContainer.Deactivate();
        // reset quest về CAN_START
        GameManager.QuestManager.instance.UpdateQuestStep(
         QuestState.CAN_START,
          traceEvent.questId
      );
        EndGame(false);

        ForceCloseAllActiveUI(false);


        targetNPC.SendMessage("ChangeNPCState", NPCState.HAVE_QUEST);

        DialogConservation correctDialog = new DialogConservation();
        DialogResponse response = new DialogResponse();

        correctDialog.message = "Thời gian đã hết. Bạn đã không thể hoàn thành nhiệm vụ. Hãy thử lại vào lần tới";
        response.executedFunction = DialogExecuteFunction.OnQuestMinigameFail;

        response.message = "Đã hiểu";
        correctDialog.possibleResponses.Add(response);
        TalkInteraction.instance.StartCoroutine(TalkInteraction.instance.SmoothTransitionToTraceMiniGame());
        StartCoroutine(ConservationManager.instance.UpdateConservation(correctDialog));

        targetNPC.SendMessage("OnQuizTimerFail");
    }

    /// <summary>
    /// Tắt tất cả UIAnimationController đang active trong scene
    /// </summary>
    private void ForceCloseAllActiveUI(bool activate = true)
    {
        // Tìm tất cả TraceObjectInteraction trong scene
        TraceObjectInteraction[] allTraceObjects = FindObjectsOfType<TraceObjectInteraction>();

        if(PasswordMinigame.instance.passwordContainer != null)
        {
            PasswordMinigame.instance.passwordContainer.UpdateObjectChange();
            PasswordMinigame.instance.passwordContainer.Deactivate();
        }

        if (LogoDropMinigame.instance.Container != null)
        {
            LogoDropMinigame.instance.Container.UpdateObjectChange();
            LogoDropMinigame.instance.Container.Deactivate();
        }

        if (DragAndDropMinigame.instance.MiniGame != null)
        {
            DragAndDropMinigame.instance.MiniGame.UpdateObjectChange();
            DragAndDropMinigame.instance.MiniGame.Deactivate();
        }

        if (UISlidingPuzzleManager.instance.Container != null)
        {
            UISlidingPuzzleManager.instance.Container.UpdateObjectChange();
            UISlidingPuzzleManager.instance.Container.Deactivate();
        }

        foreach (TraceObjectInteraction traceObj in allTraceObjects)
        {
            if (traceObj != null && traceObj.uiControllers != null)
            {
                // Tắt tất cả UI controllers của object này
                foreach (UIAnimationController uiController in traceObj.uiControllers)
                {
                    if (uiController != null)
                    {
                        uiController.Deactivate();
                    }
                }
            }
          
        }

        // Kích hoạt lại player controller nếu bị disable
        if (PlayerManager.instance != null && activate)
        {
            PlayerManager.instance.ActivateController();
        }
    }

    private void SpawnNextObject()
    {
        if (currentIndex >= traceEvent.traceObjects.Count)
            return;

        var prefab = traceEvent.traceObjects[currentIndex].prefab;
        currentObject = Instantiate(prefab);

        // Chỉ thêm nếu prefab chưa có TraceObjectInteraction
        if (!currentObject.TryGetComponent<TraceObjectInteraction>(out var ti))
        {
            ti = currentObject.AddComponent<TraceObjectInteraction>();
        }

        ti.Initialize(currentIndex);
    }

    /// <summary>
    /// Khi interaction được “xác nhận” sau UIAnimationController đóng
    /// </summary>
    public void ConfirmTrace(int index)
    {
        if (currentObject) Destroy(currentObject);

        // Tăng counter và cập nhật UI
        traceQuest.OnTracedChange();
        if(currentIndex+1 >= traceEvent.traceObjects.Count)
        {
            targetNPC.SendMessage("FinishQuestStep");
            ConservationManager.instance.StarContainer.Deactivate();
        }
        else
        {
            targetNPC.SendMessage("FinishQuestStep");
        }
      
        currentIndex++;
        SpawnNextObject();
       
    }

    public void OnTraceObjectInteracted(string questId, int index)
    {
        // destroy object vừa tương tác
        if (currentObject) Destroy(currentObject);

        // thông báo NPCController hoàn thành step
        targetNPC.SendMessage("OnQuestMinigameSuccess");

        // tăng index và spawn vật tiếp theo
        currentIndex++;
        SpawnNextObject();
    }

    #region Drag and Drop Puzzle

    private void ResetGameState()
    {
        // Lưu trạng thái ban đầu nếu chưa có
        if (initialStates.Count == 0)
        {
            SaveInitialStates();
        }

        // Reset tất cả items về vị trí ban đầu
        foreach (var state in initialStates)
        {
            if (state.item != null)
            {
                state.item.transform.SetParent(state.originalParent);
                state.item.transform.localPosition = state.originalPosition;
                state.item.transform.localRotation = Quaternion.identity;
                state.item.enabled = state.wasEnabled;
                state.item.image.raycastTarget = true;
            }
        }

        correctCount = 0;
    }

    // Lưu trạng thái ban đầu của tất cả draggable items
    private void SaveInitialStates()
    {
        initialStates.Clear();

        // Tìm tất cả DraggableItem trong scene
        DraggableItem[] allItems = FindObjectsOfType<DraggableItem>();

        foreach (var item in allItems)
        {
            ItemInitialState state = new ItemInitialState
            {
                item = item,
                originalParent = item.transform.parent,
                originalPosition = item.transform.localPosition,
                wasEnabled = item.enabled
            };
            initialStates.Add(state);
        }
    }


    public void OnCorrectDrop(DraggableItem item)
    {
       // correctCount++;
       //// item.enabled = false;
        DragAndDropMinigame.instance.OnCorrectPlacement(item.itemID);
       // if (correctCount >= DragAndDropMinigame.instance.totalPieces)
       //     EndGame(true);
    }


    public void OnWrongDrop(DraggableItem item)
    {
        Debug.Log($"Wrong {item.itemID}");
    }


    private void EndGame(bool win)
    {
 

        // Vô hiệu hoá kéo thả toàn bộ
        foreach (var slot in dropSlots)
            foreach (Transform child in slot.transform)
                if (child.TryGetComponent<DraggableItem>(out var di))
                    di.enabled = false;

        if (win)
        {
            Debug.Log("Win");
            
           

            StartCoroutine(DragAndDropUIManager.instance.DeActivateMiniGameUI());

            StartCoroutine(DelayAfterFinishPuzzle());

        }
        else
        {
            Debug.Log("Lose");
           
           
          
            StartCoroutine(DragAndDropUIManager.instance.DeActivateMiniGameUI());
           
        }
    }

    public void InitPuzzle_1()
    {
        //ResetGameState();

        //foreach (var slot in dropSlots)
        //    slot.gameManager = this;

        TraceQuestManager.instance.InitDragDropPuzzle(GetRandImage(), () =>
        {
            Debug.Log("Puzzle completed!");
        });

      
    }

    public void InitPuzzle_2()
    {
        //ResetGameState();

        //foreach (var slot in dropSlots)
        //    slot.gameManager = this;
        TraceQuestManager.instance.InitDragDropPuzzle(GetRandImage(), () =>
        {
            Debug.Log("Puzzle completed!");
        });


    }

    private Texture2D GetRandImage()
    {
        int randImage = UnityEngine.Random.Range(0, ptitImages.Length);
        if(showImages.Count == ptitImages.Length) showImages.Clear();

        while (showImages.Count < ptitImages.Length && showImages.Contains(randImage))
        {
            randImage = UnityEngine.Random.Range(0, ptitImages.Length);
        }

        Texture2D selectedImage = ptitImages[randImage];
        showImages.Add(randImage);

        return selectedImage;
    }

    public void InitLogo_1()
    {
        List<LogoDropMinigame.LogoItem> logos = new List<LogoDropMinigame.LogoItem>();
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "1",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[0]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "2",
            name = "Đáp án chính xác",
            description = "Cấu trúc logo mở thể hiện Học viện gắn liền với thực tiễn, với xã hội và luôn phát triển không ngừng. Ba vòng tròn quyện vào nhau và chuyển hóa sang nhau thể hiện 3 gắn kết: Đào tạo – Nghiên cứu – Sản xuất Kinh doanh. Hình ảnh quyển sách mở rộng và mô hình cấu trúc nguyên tử: biểu tượng 2 hoạt động chính của Học viện là đào tạo và nghiên cứu Chữ PTIT (tên viết tắt tiếng Anh của Học viện – Posts & Telecoms Institute of Technology) đồng thời là Bưu chính (P), Viễn thông (T) và Công nghệ thông tin (IT) – 3 lĩnh vực nghiên cứu và đào tạo của Học viện",
            sprite = sprites[1]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "3",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[2]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "4",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[3]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "5",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[4]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "6",
            name = "Đáp án chính xác",
            description = "Cấu trúc logo mở thể hiện Học viện gắn liền với thực tiễn, với xã hội và luôn phát triển không ngừng. Ba vòng tròn quyện vào nhau và chuyển hóa sang nhau thể hiện 3 gắn kết: Đào tạo – Nghiên cứu – Sản xuất Kinh doanh. Hình ảnh quyển sách mở rộng và mô hình cấu trúc nguyên tử: biểu tượng 2 hoạt động chính của Học viện là đào tạo và nghiên cứu Chữ PTIT (tên viết tắt tiếng Anh của Học viện – Posts & Telecoms Institute of Technology) đồng thời là Bưu chính (P), Viễn thông (T) và Công nghệ thông tin (IT) – 3 lĩnh vực nghiên cứu và đào tạo của Học viện",
            sprite = sprites[5]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "7",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[6]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "8",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[7]
        });
        logos.Add(new LogoDropMinigame.LogoItem
        {
            id = "9",
            name = "Unity Logo",
            description = "Game Engine phổ biến",
            sprite = sprites[8]
        });
        TraceQuestManager.instance.InitLogoDropMinigame(logos, "6");


    }

    public void InitSlidingPuzzle_1()
    {
        UISlidingPuzzleManager.instance.SetSize(3);
        UISlidingPuzzleManager.instance.PuzzleCompleted += () =>
        {
            UISlidingPuzzleManager.instance.Container.Deactivate();
            StartCoroutine(DelayAfterFinishPuzzle());
        };
        UISlidingPuzzleManager.instance.InitializePuzzle(GetRandImage(), () =>
        {
            OnMinigameFail();
        });
    }

    public void InitSlidingPuzzle_2()
    {
        UISlidingPuzzleManager.instance.SetSize(3);
        UISlidingPuzzleManager.instance.InitializePuzzle(GetRandImage(), () =>
        {
            OnMinigameFail();
        });
    }

    public void InitSlidingPuzzle_3()
    {
        UISlidingPuzzleManager.instance.SetSize(4);
        UISlidingPuzzleManager.instance.InitializePuzzle(GetRandImage(), () =>
        {
            OnMinigameFail();
        });
    }

    public int GetRandomFromList(List<int> numList, List<int> except = null)
    {
        int rand = UnityEngine.Random.Range(0, numList.Count);
        while(except != null && except.Contains(numList[rand]))
        {
            rand = UnityEngine.Random.Range(0, numList.Count);
        }
        return numList[rand];
    }
        

    public void InitPassWordMiniGame_1()
    {
        //setup password
        List<int> correctNumbers = new List<int>();
        List<int> wrongNumbers = new List<int>();
        //random password
        while(correctNumbers.Count < 3)
        {
            int rand = UnityEngine.Random.Range(0, 10);
            while(correctNumbers.Contains(rand))
            {
                rand = UnityEngine.Random.Range(0, 10);
            }

            correctNumbers.Add(rand);
        }
        //
        for(int i = 0; i < 10; i++)
        {
            if (!correctNumbers.Contains(i)) wrongNumbers.Add(i);
        }
       

        for (int index = 0; index < hintContainer.childCount; index++)
        {
            TMP_Text[] hintTexts = hintContainer.GetChild(index).GetComponentsInChildren<TMP_Text>();
            int fillCorrect = 0;
            int correctNumber = -1;
      
            switch (index)
            {
                case 0:
                    int correctPos = UnityEngine.Random.Range(0, correctNumbers.Count);
                    hintTexts[correctPos].text = correctNumbers[correctPos].ToString();
                    //
                    for (int i = 0; i < 3; i++)
                    {
                        if (i == correctPos) continue;
                        int wrongNum = GetRandomFromList(wrongNumbers);
                        hintTexts[i].text = wrongNum.ToString();
                    }
                    hintTexts[3].text = "1 số đúng và ở đúng vị trí";
                    break;

                case 1:
                    fillCorrect = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        if (fillCorrect > 0)
                        {
                            float ratio = UnityEngine.Random.Range(0f, 1f);
                            if (ratio < 0.6f || i == 2)
                            {
                                int correctNum = GetRandomFromList(correctNumbers, new List<int> { correctNumbers[i] });
                                hintTexts[i].text = correctNum.ToString();
                                fillCorrect--;
                                correctNumber = correctNum;
                                continue;
                            }
                        }
                        int wrongNum = GetRandomFromList(wrongNumbers);
                        hintTexts[i].text = wrongNum.ToString();
                    }
                    hintTexts[3].text = "1 số đúng nhưng ở sai vị trí";
                    break;

                case 2:
                    fillCorrect = 1;
                    for (int i = 0; i < 3; i++)
                    {
                        if (fillCorrect > 0)
                        {
                            float ratio = UnityEngine.Random.Range(0f, 1f);
                            if (ratio < 0.6f || i == 2)
                            {
                                int wrongNum = GetRandomFromList(wrongNumbers);
                                hintTexts[i].text = wrongNum.ToString();
                                fillCorrect--;
                                continue;
                            }
                        }
                        int correctNum = GetRandomFromList(correctNumbers, new List<int> { correctNumbers[i], correctNumber });
                        correctNumber = correctNum;
                        hintTexts[i].text = correctNum.ToString();
                    }
                    hintTexts[3].text = "2 số đúng nhưng ở sai vị trí";
                    break;

                case 3:
                    List<int> except = new List<int>();
                    for (int i = 0; i < 3; i++)
                    {
                        int wrongNum = GetRandomFromList(wrongNumbers, except);
                        except.Add(wrongNum);
                        hintTexts[i].text = wrongNum.ToString();
                    }
                    hintTexts[3].text = "Không có gì đúng";
                    break;

                case 4:
                    int correctPos2 = UnityEngine.Random.Range(0, correctNumbers.Count);
                    hintTexts[correctPos2].text = correctNumbers[correctPos2].ToString();
                    bool canfillCorrect = true ;
                    for (int i = 0; i < 3; i++)
                    {
                        if (i == correctPos2) continue;
                        if (canfillCorrect == true)
                        {
                            float ratio = UnityEngine.Random.Range(0f, 1f);
                            if (ratio < 0.6f || i == 2)
                            {
                                int correctNum = GetRandomFromList(correctNumbers, new List<int> { correctNumbers[i], correctNumbers[correctPos2] });
                                hintTexts[i].text = correctNum.ToString();
                                canfillCorrect = false;
                                continue;
                            }
                        }
                        int wrongNum = GetRandomFromList(wrongNumbers);
                        hintTexts[i].text = wrongNum.ToString();
                        canfillCorrect = true;
                    }
                    hintTexts[3].text = "2 số đúng, 1 đúng vị trí, 1 sai vị trí";
                    break;
            }
        }
            
        
        
        Debug.Log($"{correctNumbers[0]}{correctNumbers[1]}{correctNumbers[2]}");
        //
        TraceQuestManager.instance.InitPasswordMinigameForInterger(
        passwordContainer_1,
        passwordInput_1,
        checkBtn_1,
        titleTxt_1,
        feedbackTxt_1,
        closeBtnPass1,
        $"{correctNumbers[0]}{correctNumbers[1]}{correctNumbers[2]}",
        "Mật khẩu cấp 1"
        );
    }

    public void InitPassWordMiniGame_2()
    {
        string passString = "";
        for(int i = 0; i < 6; i++)
        {
            passString += UnityEngine.Random.Range(0, 10);
        }

        password2.text = passString;

        TraceQuestManager.instance.InitPasswordMinigame(
        passwordContainer_2,
        passwordInput_2,
        checkBtn_2,
        titleTxt_2,
        feedbackTxt_2,
        closeBtnPass2,
        passString,
        "Mật khẩu cấp 2"
        );
    }

    public void InitPassWordMiniGame_3()
    {
        TraceQuestManager.instance.InitPasswordMinigame(
        passwordContainer_3,
        passwordInput_3,
        checkBtn_3,
        titleTxt_3,
        feedbackTxt_3,
        closeBtnPass2,
        "5239",
        "Mật khẩu cấp 3"
        );

        PuzzleObject = Instantiate(PuzzelModel);
    }


    private IEnumerator DelayAfterFinishPuzzle()
    {
       yield return new WaitForSeconds(1f);

       DragAndDropPuzzle = FindAnyObjectByType<TraceObjectInteraction>();



        if (DragAndDropPuzzle != null)
        {
            Debug.Log("Call Next UI");
            DragAndDropPuzzle.OnClick_NextUI();
        }
    }

    #endregion

    public class TraceQuest
    {
        public int numberToTrace;
        public int currentTraced;
        public Action OnFinishQuest;

        /// <summary>
        /// Gọi mỗi khi một vật phẩm được “xác nhận” tương tác
        /// sẽ tăng counter và cập nhật UI giống CollectQuest.OnCollectedChange()
        /// </summary>
        public void OnTracedChange()
        {
            currentTraced++;
            // Cập nhật text dạng "1/6", "2/6", …
            ConservationManager.instance.StarText.text = "Số vật phẩm đã thu thập: " + $"{currentTraced}/{numberToTrace}";
            Debug.Log($"{currentTraced}/{numberToTrace}");
            // Nếu đã tương tác đủ
            //if (currentTraced == numberToTrace)
            //    OnFinishQuest?.Invoke();
        }
    }

    #region Enhanced Drag and Drop Minigame

    /// <summary>
    /// Khởi tạo Drag and Drop minigame với texture
    /// </summary>
    public void InitDragDropPuzzle(Texture2D puzzleTexture, System.Action onComplete = null)
    {
        // Tìm hoặc tạo component DragAndDropMinigame
        if (DragAndDropMinigame.instance == null)
        {
            GameObject minigameObj = GameObject.Find("DragDropMinigame");
            if (minigameObj == null)
            {
                minigameObj = new GameObject("DragDropMinigame");
                minigameObj.transform.SetParent(DragAndDropUIManager.instance.transform);
            }
            DragAndDropMinigame.instance = minigameObj.GetComponent<DragAndDropMinigame>();
            if (DragAndDropMinigame.instance == null)
            {
                DragAndDropMinigame.instance = minigameObj.AddComponent<DragAndDropMinigame>();
            }
        }

        // Callback khi hoàn thành
        System.Action onCompleteCallback = () =>
        {
            StartCoroutine(DragAndDropUIManager.instance.DeActivateMiniGameUI());
            StartCoroutine(DelayAfterFinishPuzzle());
            onComplete?.Invoke();
        };

        // Khởi tạo minigame
        DragAndDropMinigame.instance.Init(puzzleTexture, onCompleteCallback, () =>
        {
            OnMinigameFail();
        });

        // Hiển thị UI
        StartCoroutine(DragAndDropUIManager.instance.ActivateMiniGameUI());
    }

    /// <summary>
    /// Khởi tạo Drag and Drop với các settings custom
    /// </summary>
    public void InitDragDropPuzzleWithSettings(Texture2D puzzleTexture, int rows, int columns, System.Action onComplete = null)
    {
        if (DragAndDropMinigame.instance == null)
        {
            GameObject minigameObj = new GameObject("DragDropMinigame");
            DragAndDropMinigame.instance = minigameObj.AddComponent<DragAndDropMinigame>();
        }

        // Set custom settings trước khi init
#if UNITY_EDITOR
        var serializedObject = new UnityEditor.SerializedObject(DragAndDropMinigame.instance);
        serializedObject.FindProperty("rows").intValue = rows;
        serializedObject.FindProperty("columns").intValue = columns;
        serializedObject.ApplyModifiedProperties();
#endif
        InitDragDropPuzzle(puzzleTexture, onComplete);
    }

    #endregion

    #region Logo Drop Minigame

    /// <summary>
    /// Khởi tạo Logo Drop minigame
    /// </summary>
    public void InitLogoDropMinigame(List<LogoDropMinigame.LogoItem> logoItems, string correctID, System.Action onComplete = null, System.Action onFail = null)
    {
        // Tìm hoặc tạo component LogoDropMinigame
        if (LogoDropMinigame.instance == null)
        {
            GameObject minigameObj = GameObject.Find("LogoDropMinigame");
            if (minigameObj == null)
            {
                minigameObj = new GameObject("LogoDropMinigame");
                Canvas canvas = minigameObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                minigameObj.AddComponent<CanvasScaler>();
                minigameObj.AddComponent<GraphicRaycaster>();
            }

            LogoDropMinigame.instance = minigameObj.GetComponent<LogoDropMinigame>();
            if (LogoDropMinigame.instance == null)
            {
                LogoDropMinigame.instance = minigameObj.AddComponent<LogoDropMinigame>();
            }

            // Tạo containers nếu chưa có
            CreateLogoDropContainers(minigameObj.transform);
        }

        // Callbacks
        System.Action onCompleteCallback = () =>
        {
            Debug.Log("Logo Drop completed!");
            LogoDropMinigame.instance.Container.Deactivate();
            StartCoroutine(DelayAfterFinishPuzzle());
            onComplete?.Invoke();
        };

        System.Action onFailCallback = () =>
        {
            Debug.Log("Logo Drop failed!");
            // Reset quest hoặc xử lý thất bại
            onFail?.Invoke();
            OnMinigameFail();


        };

        // Khởi tạo minigame
        LogoDropMinigame.instance.Init(logoItems, correctID, onCompleteCallback, onFailCallback);
    }

    public void OnMinigameFail()
    {
        ForceCloseAllActiveUI(false);

        DialogConservation correctDialog = new DialogConservation();
        DialogResponse response = new DialogResponse();

        correctDialog.message = "Thử thách thất bại!!!";
        response.executedFunction = DialogExecuteFunction.StopInteract;

        response.message = "Đã hiểu";
        correctDialog.possibleResponses.Add(response);
        TalkInteraction.instance.StartCoroutine(TalkInteraction.instance.SmoothTransitionToTraceMiniGame());
        StartCoroutine(ConservationManager.instance.UpdateConservation(correctDialog));
    }

    /// <summary>
    /// Tạo containers cho Logo Drop minigame
    /// </summary>
    private void CreateLogoDropContainers(Transform parent)
    {
        // Drag Container
        GameObject dragContainer = new GameObject("DragContainer");
        dragContainer.transform.SetParent(parent);
        RectTransform dragRect = dragContainer.AddComponent<RectTransform>();
        dragRect.anchorMin = new Vector2(0, 0.5f);
        dragRect.anchorMax = new Vector2(0.4f, 1);
        dragRect.offsetMin = new Vector2(50, 50);
        dragRect.offsetMax = new Vector2(-50, -50);

        // Drop Container
        GameObject dropContainer = new GameObject("DropContainer");
        dropContainer.transform.SetParent(parent);
        RectTransform dropRect = dropContainer.AddComponent<RectTransform>();
        dropRect.anchorMin = new Vector2(0.6f, 0.5f);
        dropRect.anchorMax = new Vector2(1, 1);
        dropRect.offsetMin = new Vector2(50, 50);
        dropRect.offsetMax = new Vector2(-50, -50);

        // UI Elements Container
        GameObject uiContainer = new GameObject("UIContainer");
        uiContainer.transform.SetParent(parent);
        RectTransform uiRect = uiContainer.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0, 0);
        uiRect.anchorMax = new Vector2(1, 0.5f);
        uiRect.offsetMin = new Vector2(50, 50);
        uiRect.offsetMax = new Vector2(-50, -50);

        // Assign to component
#if UNITY_EDITOR
        var serializedObject = new UnityEditor.SerializedObject(LogoDropMinigame.instance);
        serializedObject.FindProperty("dragContainer").objectReferenceValue = dragContainer.transform;
        serializedObject.FindProperty("dropContainer").objectReferenceValue = dropContainer.transform;
        serializedObject.ApplyModifiedProperties();
#endif
    }

    #endregion

    #region Password Minigame

    /// <summary>
    /// Khởi tạo Password minigame với container và UI elements đầy đủ
    /// </summary>
    public void InitPasswordMinigameForInterger(UIAnimationController passwordContainer,
                                    TMP_InputField inputField,
                                    Button checkButton,
                                    TextMeshProUGUI titleText,
                                    TextMeshProUGUI feedbackText,
                                    Button closebtn,
                                    string correctPassword,
                                    string title = "Nhập mật mã",
                                    System.Action onComplete = null,
                                    System.Action onFail = null)
    {
        // Tìm hoặc tạo component PasswordMinigame
        if (PasswordMinigame.instance == null)
        {
            GameObject minigameObj = GameObject.Find("PasswordMinigameManager");
            if (minigameObj == null)
            {
                minigameObj = new GameObject("PasswordMinigameManager");
            }

            PasswordMinigame.instance = minigameObj.GetComponent<PasswordMinigame>();
            if (PasswordMinigame.instance == null)
            {
                PasswordMinigame.instance = minigameObj.AddComponent<PasswordMinigame>();
            }
        }

        // Callbacks
        System.Action onCompleteCallback = () =>
        {
            Debug.Log("Password minigame completed!");
            PasswordMinigame.instance.passwordContainer.Deactivate();
            StartCoroutine(DelayAfterFinishPuzzle());
            onComplete?.Invoke();
        };

        System.Action onFailCallback = () =>
        {
            Debug.Log("Password minigame failed!");
            OnMinigameFail();
            onFail?.Invoke();
        };

        closebtn.onClick.RemoveAllListeners();
        closebtn.onClick.AddListener(delegate
        {
            onFailCallback?.Invoke();
        });

        // Khởi tạo minigame với đầy đủ UI elements
        PasswordMinigame.instance.InitForInterger(passwordContainer, inputField,  checkButton, titleText, feedbackText,
                            correctPassword, title, onCompleteCallback, onFailCallback);
    }

    public void InitPasswordMinigame(UIAnimationController passwordContainer,
                                TMP_InputField inputField,
                                Button checkButton,
                                TextMeshProUGUI titleText,
                                TextMeshProUGUI feedbackText,
                                Button closeBtn,
                                string correctPassword,
                                string title = "Nhập mật mã",
                                System.Action onComplete = null,
                                System.Action onFail = null)
    {
        // Tìm hoặc tạo component PasswordMinigame
        if (PasswordMinigame.instance == null)
        {
            GameObject minigameObj = GameObject.Find("PasswordMinigameManager");
            if (minigameObj == null)
            {
                minigameObj = new GameObject("PasswordMinigameManager");
            }

            PasswordMinigame.instance = minigameObj.GetComponent<PasswordMinigame>();
            if (PasswordMinigame.instance == null)
            {
                PasswordMinigame.instance = minigameObj.AddComponent<PasswordMinigame>();
            }
        }

        // Callbacks
        System.Action onCompleteCallback = () =>
        {
            Debug.Log("Password minigame completed!");
            PasswordMinigame.instance.passwordContainer.Deactivate();
            StartCoroutine(DelayAfterFinishPuzzle());
            onComplete?.Invoke();
        };

        System.Action onFailCallback = () =>
        {
            Debug.Log("Password minigame failed!");
            // Có thể reset quest hoặc cho phép thử lại
            onFail?.Invoke();
        };

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(delegate
        {
            onFailCallback?.Invoke();
        });

        // Khởi tạo minigame với đầy đủ UI elements
        PasswordMinigame.instance.Init(passwordContainer, inputField, checkButton, titleText, feedbackText,
                            correctPassword, title, onCompleteCallback, onFailCallback);
    }

    /// <summary>
    /// Khởi tạo Password minigame với container (tự động tìm UI elements)
    /// </summary>
    public void InitPasswordMinigame(UIAnimationController passwordContainer,
                                    string correctPassword,
                                    string title = "Nhập mật mã",
                                    System.Action onComplete = null,
                                    System.Action onFail = null)
    {
        // Tìm hoặc tạo component PasswordMinigame
        if (PasswordMinigame.instance == null)
        {
            GameObject minigameObj = GameObject.Find("PasswordMinigameManager");
            if (minigameObj == null)
            {
                minigameObj = new GameObject("PasswordMinigameManager");
            }

            PasswordMinigame.instance = minigameObj.GetComponent<PasswordMinigame>();
            if (PasswordMinigame.instance == null)
            {
                PasswordMinigame.instance = minigameObj.AddComponent<PasswordMinigame>();
            }
        }

        // Callbacks
        System.Action onCompleteCallback = () =>
        {
            Debug.Log("Password minigame completed!");
           
            onComplete?.Invoke();
        };

        System.Action onFailCallback = () =>
        {
            Debug.Log("Password minigame failed!");
            onFail?.Invoke();
        };

        // Khởi tạo minigame (sẽ tự động tìm UI elements trong container)
        PasswordMinigame.instance.Init(passwordContainer, correctPassword, title, onCompleteCallback, onFailCallback);
    }

    /// <summary>
    /// Khởi tạo Password minigame với nhiều containers khác nhau
    /// </summary>
    public void InitPasswordMinigameWithMultipleContainers(UIAnimationController[] passwordContainers,
                                                          string[] passwords,
                                                          string[] titles = null,
                                                          System.Action onAllComplete = null)
    {
        if (passwordContainers == null || passwords == null ||
            passwordContainers.Length != passwords.Length)
        {
            Debug.LogError("Invalid parameters for multiple password containers!");
            return;
        }

        int currentContainerIndex = 0;

        System.Action onSingleComplete = null;
        onSingleComplete = () =>
        {
            currentContainerIndex++;
            if (currentContainerIndex < passwordContainers.Length)
            {
                string title = titles != null && currentContainerIndex < titles.Length ?
                              titles[currentContainerIndex] : "Nhập mật mã";

                InitPasswordMinigame(passwordContainers[currentContainerIndex],
                                   passwords[currentContainerIndex],
                                   title,
                                   onSingleComplete,
                                   null);
            }
            else
            {
                Debug.Log("All password minigames completed!");
                onAllComplete?.Invoke();
            }
        };

        // Bắt đầu với container đầu tiên
        string firstTitle = titles != null && titles.Length > 0 ? titles[0] : "Nhập mật mã";
        InitPasswordMinigame(passwordContainers[0], passwords[0], firstTitle, onSingleComplete, null);
    }

    #endregion
}
