using DS.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class ConservationManager : MonoBehaviour
    {
        public static ConservationManager instance;

        [Header("NPC Message")]

        public UIAnimationController messageContainer;

        public TMP_Text npcMessage;

        public TMP_Text npcName;

        public TMP_Text npcRole;

       

        [Header("Player Response")]

        public UIAnimationController responseController;

        public GameObject responsePrefab;

        public Sprite defautIcon;

        GameObject targetNPC;

        NPCConservationSO conservationData;

        [Header("Image")]

        public UIAnimationController ImageContainer;

        public Image questionIllustration = null;

        [Header("Timed Image Quiz UI")]

        public UIAnimationController imageQuizContainer; 

        public Image quizImage;                          

        public Slider quizTimerSlider;


        [Header("Time Countdown Quiz")]

        public UIAnimationController timerContainer;

        public TMP_Text timerText;

        [Header("Star")]

        public UIAnimationController StarContainer;

        public TMP_Text StarText;

        [Header("Mission Complete")]

        public UIAnimationController missionCompleteContainer;

        public TMP_Text missionCompleteText;

        [Header("Mission Fail")]

        public UIAnimationController missionFailContainer;


        #region ImageTimedQuiz

        [HideInInspector]
        public bool isTimedImageMode;
        [HideInInspector]
        public float timedImageDuration;


        public void EnableTimedImageMode(float duration)
        {
            isTimedImageMode = true;
            timedImageDuration = duration;
        }


        private void DisableTimedImageMode()
        {
            isTimedImageMode = false;
        }
        private Coroutine quizImageTimerRoutine;

      
        public void ShowQuizImage(Sprite sprite, float duration = 15f)
        {
            quizImage.sprite = sprite;
            quizTimerSlider.maxValue = duration;
            quizTimerSlider.value = duration;
            imageQuizContainer.Activate();
            quizImageTimerRoutine = StartCoroutine(QuizImageCountdown(duration));
        }

        
        public void HideQuizImage()
        {
            if (quizImageTimerRoutine != null)
                StopCoroutine(quizImageTimerRoutine);
            imageQuizContainer.Deactivate();
        }

        private IEnumerator QuizImageCountdown(float duration)
        {
            float t = duration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                quizTimerSlider.value = Mathf.Max(t, 0f);
                yield return null;
            }
        }

        #endregion

        private void Awake()
        {

            instance = this;

        }

        public void InitConservation(GameObject npc, DialogConservation dialog)
        {        
            targetNPC = npc;

            var npcController = npc.GetComponent<IDialogueHandler>();

            if (npcController != null)
            {
                npcName.text = npcController.NpcInfo.npcName;

                npcRole.text = npcController.NpcInfo.npcRole;
            }

            SetupDialogInConservation(dialog);

            StartCoroutine(ActivateConservationDialog());
        }

        public void InitConservation(GameObject npc, NPCConservationSO conservation)
        {
            targetNPC = npc;

            conservationData = conservation;

            //update name and role text
            NPCController npcController = npc.GetComponent<NPCController>();

            if (npcController)
            {
                npcName.text = npcController.npcInfo.npcName;

                npcRole.text = npcController.npcInfo.npcRole;
            }
            

            //setup first conservation
            DialogConservation firstDialog;

            if(conservationData.startFromFirstDialog)
            {
                firstDialog = conservation.dialogs[0];
            }

            else
            {
                //can get a random dialog
                firstDialog = conservation.dialogs[0];
            }

            SetupQuizDialogInConservation(firstDialog);
        }

        public void ChangeTargetNPC(GameObject npc) { targetNPC = npc; }

        #region SetupDialog

        void SetupDialogInConservation(DialogConservation dialog)
        {
           
            npcMessage.text = dialog.message;

            var npcCtrl = targetNPC.GetComponent<IDialogueHandler>();

            if (npcCtrl != null)
            {
                npcCtrl.PausedDialog = dialog;

                Debug.Log($"[PauseResume] saved pausedDialog = {dialog.dialogId}");

                npcCtrl.IsConversationPaused = false;
            }

            //Setup player response
            //Clear old responses in container
            foreach (Transform child in responseController.transform)
            {
                child.gameObject.SetActive(false);
            }

            //Add new response
            for(int i = 0; i < dialog.possibleResponses.Count; i++)
            {
                DialogResponse responseData = dialog.possibleResponses[i];

                //find or create response object
                GameObject responseObject;

                if (i < responseController.transform.childCount)
                {
                    responseObject = responseController.transform.GetChild(i).gameObject;
                } 

                else
                {
                    responseObject = Instantiate(responsePrefab, responseController.transform);
                }

                //setup response data
                responseObject.GetComponentInChildren<TMP_Text>().text = responseData.message;

                responseObject.transform.GetChild(1).GetComponent<Image>().sprite = responseData.icon != null? responseData.icon : defautIcon;

                responseObject.GetComponent<Button>().onClick.AddListener(delegate () 
                {
                    if (!string.IsNullOrEmpty(responseData.nextDialogId))
                    {
                        Debug.Log($"Looking for next node with ID: {responseData.nextDialogId}");

                        // Lấy adapter từ targetNPC, nếu chưa có thì thử dùng DialogueManager
                        var npcController = targetNPC.GetComponent<IDialogueHandler>();

                        DSDialogueAdapter dialogueAdapter = targetNPC.GetComponent<DSDialogueAdapter>();                     

                        DSDialogueSO nextNode = dialogueAdapter != null ? dialogueAdapter.GetNodeByName(responseData.nextDialogId) : null;

                        if (nextNode != null)
                        {
                            Debug.Log($"Found next node: {nextNode.DialogueName}");

                            DialogConservation nextCons = dialogueAdapter.ConvertDSDialogueToConservation(nextNode);

                            StartCoroutine(UpdateConservation(nextCons));

                            // Gửi lệnh dựa trên ExecutedFunction của node tiếp theo, không của response hiện tại.
                            if (nextNode.NextExecutedFunction != DialogExecuteFunction.None)
                            {
                                targetNPC.SendMessage(nextNode.NextExecutedFunction.ToString());

                                Debug.Log(nextNode.NextExecutedFunction.ToString());
                            }
                        }                        
                       
                        else
                        {
                            Debug.LogError($"Could not find next node with ID: {responseData.nextDialogId}");

                            Debug.Log($"Current nodeLookup count: {(dialogueAdapter != null ? dialogueAdapter.GetNodeLookupCount() : 0)}");

                            if (dialogueAdapter != null)
                            {
                                dialogueAdapter.LogNodeNames();
                            }
                        }
                    }
                    else if (responseData.executedFunction == DialogExecuteFunction.NextQuiz ||
                            responseData.executedFunction == DialogExecuteFunction.AnswerCorrect ||
                            responseData.executedFunction == DialogExecuteFunction.AnswerWrong
                           )
                    {
                        // Gọi trực tiếp hàm của QuizManager
                        QuizManager.instance.Invoke(responseData.executedFunction.ToString(), 0f);
                    }

                    else if (responseData.executedFunction == DialogExecuteFunction.OnQuestMinigameSuccess ||
                             responseData.executedFunction == DialogExecuteFunction.OnQuestMinigameFail ||
                             responseData.executedFunction == DialogExecuteFunction.OpenShopFunctionalWindow)
                    {
                        targetNPC.SendMessage(responseData.executedFunction.ToString());
                    }
                    else if (string.IsNullOrEmpty(responseData.nextDialogId) && responseData.executedFunction != DialogExecuteFunction.None)
                    {/*
                        Debug.Log("No nextDialogId; switching to quiz conversation.");
                        // Dừng hội thoại cũ
                        StopAllCoroutines();
                        responseController.Deactivate();
                        messageContainer.Deactivate();

                        // Lấy NPCController của targetNPC để lấy tham chiếu quiz conversation
                        NPCController npcController = targetNPC.GetComponent<NPCController>();
                        if (npcController != null && npcController.quizConversation != null)
                        {
                            // Gọi trực tiếp QuizManager để khởi tạo hội thoại quiz
                            QuizManager.instance.InitAndStartQuizData(targetNPC, npcController.quizConversation);
                        }
                        else
                        {
                            Debug.LogError("Quiz conversation asset not assigned in NPCController or NPCController not found.");
                        }
                    */

                        targetNPC.SendMessage(responseData.executedFunction.ToString());
                    }
                   

                    else
                    {
                        // Get the TalkInteraction component from the target NPC
                        TalkInteraction talkInteraction = targetNPC.GetComponent<TalkInteraction>();
                        if (talkInteraction != null)
                        {
                            talkInteraction.StopInteract();
                        }
                        else
                        {
                            Debug.LogError($"No TalkInteraction component found on NPC: {targetNPC.name}");
                        }
                    }
                });




                responseObject.SetActive(true);
            }
            responseController.UpdateObjectChange();
        }

        void SetupQuizDialogInConservation(DialogConservation dialog)
        {
            //Setup npc message
            npcMessage.text = dialog.message;

            //Setup player response
            //Clear old responses in container
            foreach (Transform child in responseController.transform)
            {
                child.gameObject.SetActive(false);
            }

            //Add new response
            for (int i = 0; i < dialog.possibleResponses.Count; i++)
            {
                DialogResponse responseData = dialog.possibleResponses[i];

                //find or create response object
                GameObject responseObject;
                if (i < responseController.transform.childCount)
                {
                    responseObject = responseController.transform.GetChild(i).gameObject;
                }
                else
                {
                    responseObject = Instantiate(responsePrefab, responseController.transform);
                }

                //setup response data
                responseObject.GetComponentInChildren<TMP_Text>().text = responseData.message;
                responseObject.transform.GetChild(1).GetComponent<Image>().sprite = responseData.icon != null ? responseData.icon : defautIcon;
                responseObject.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    if (responseData.nextDialogId != "")
                    {
                        DialogConservation nextDialog = conservationData.GetDialog(responseData.nextDialogId);
                        if (nextDialog != null) { StartCoroutine(UpdateConservation(nextDialog)); }
                    }
                    if (responseData.executedFunction != DialogExecuteFunction.None) { targetNPC.SendMessage(responseData.executedFunction.ToString()); }
                });

                responseObject.SetActive(true);
            }
            responseController.UpdateObjectChange();
        }

        #endregion

        void ClearAllButtonEvent()
        {
            Button[] btns = responseController.GetComponentsInChildren<Button>();
            foreach(Button b in btns)
            {
                b.onClick.RemoveAllListeners();
            }
            //
            DisableAllButton();
        }

        void EnableAllButton()
        {
            Button[] btns = responseController.GetComponentsInChildren<Button>();
            foreach (Button b in btns)
            {
                b.interactable = true;
            }
        }

        void DisableAllButton()
        {
            Button[] btns = responseController.GetComponentsInChildren<Button>();
            foreach (Button b in btns)
            {
                b.interactable = false;
            }
        }
        public IEnumerator UpdateConservation(DialogConservation nextDialog, Sprite questionImage = null)
        {
            ClearAllButtonEvent();
            responseController.Deactivate();
            ImageContainer.Deactivate();

            yield return new WaitForSeconds(0.4f);
            messageContainer.Deactivate();

            if (quizTimerSlider != null)
                quizTimerSlider.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.6f);

            bool useTimedImage = isTimedImageMode && questionImage != null;

            if (useTimedImage)
            {
                // ——————————————
                // 1) Show timed-image container
                // ——————————————
                // đảm bảo ImageContainer vẫn tắt:
                ImageContainer.Deactivate();

                // bật đúng container cho timed image
                imageQuizContainer.Activate();

                // gán sprite và reset slider
                quizImage.sprite = questionImage;
                quizImage.enabled = true;

                quizTimerSlider.maxValue = timedImageDuration;
                quizTimerSlider.value = timedImageDuration;
                quizTimerSlider.gameObject.SetActive(true);

                // đếm ngược
                float t = timedImageDuration;
                while (t > 0f)
                {
                    t -= Time.deltaTime;
                    quizTimerSlider.value = Mathf.Max(t, 0f);
                    yield return null;
                }

                // ẩn timed-image container
                quizTimerSlider.gameObject.SetActive(false);
                imageQuizContainer.Deactivate();
                quizImage.enabled = false;

                DisableTimedImageMode();
            }

            // ——————————————
            // 2) Tiếp tục phần câu hỏi như bình thường
            // ——————————————
            SetupDialogInConservation(nextDialog);

            // chỉ show questionIllustration nếu không phải timed-image
            bool showWithQuestion = questionImage != null && !useTimedImage;
            questionIllustration.enabled = showWithQuestion;
            if (showWithQuestion)
            {
                questionIllustration.sprite = questionImage;
                questionIllustration.color = Color.white;
            }

            messageContainer.Activate();
            yield return new WaitForSeconds(0.2f);
            responseController.Activate();
            ImageContainer.Activate();
            EnableAllButton();

            yield return null;
        }

        public IEnumerator ClearConservation()
        {
            ClearAllButtonEvent();
            responseController.Deactivate();
            ImageContainer.Deactivate();

            yield return new WaitForSeconds(0.4f);
            messageContainer.Deactivate();
        }

        public IEnumerator ActivateConservationDialog()
        {
            messageContainer.Activate();
            yield return new WaitForSeconds(0.3f);
            responseController.Activate();
            ImageContainer.Activate();
            EnableAllButton();
        }

        public IEnumerator DeactivateConservationDialog()
        {
            ClearAllButtonEvent();
            responseController.Deactivate();
            ImageContainer.Deactivate();
            yield return new WaitForSeconds(0.3f);
            messageContainer.Deactivate();
        }

        public IEnumerator DeactivateConservationChoice()
        {
            ClearAllButtonEvent();
            responseController.Deactivate();
            yield return new WaitForSeconds(0.3f);
           
        }

        public IEnumerator ActivateConservationChoice()
        {
           
            yield return new WaitForSeconds(0.3f);
            responseController.Activate();
            EnableAllButton();
        }
    }
}

