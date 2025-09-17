using GameManager;
using Interaction;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using static MouseManager;

public class Tutorial_Scene : MonoBehaviour
{
    public Button NextScene_Btn;
    public UIAnimationController Scene;

    [Header("Flag")]
    public bool isDefaultTutorial;
    public bool isMouseTutorial;
    public bool isKeyboardTutorial;
    public bool isShowHUDTutorial;
    public bool isEndPhase1Tutorial;
    public bool isQuizPhaseTutorial;
    public bool isEndEnterPressTutorial;

    void Start()
    {
        NextScene_Btn.onClick.AddListener(() =>
        {
            Scene.Deactivate();
            if (!isQuizPhaseTutorial)
            {
                TutorialManager.Instance.ShowNextStepDelayed();
            }
            else
            {
                if (TutorialManager.Instance.currentUI != null)
                    Destroy(TutorialManager.Instance.currentUI.gameObject);
            }
            NextStepPrepare();
        });
    }

    private void NextStepPrepare()
    {
        if (isDefaultTutorial)
        {
            PlayerManager.instance.DeactivateController();
        }

        if (isMouseTutorial)
        {
            PlayerManager.instance.isTransitioningToIdle = false;

            // Đảm bảo input được reset trước khi activate
            if (PlayerManager.instance.inputHandle != null)
            {
                PlayerManager.instance.inputHandle.ResetAllInputValues();
            }

            PlayerManager.instance.inputHandle.enabled = true;
            PlayerManager.instance.inputHandle.inputActions.PlayerMovement.Movement.Disable();
            PlayerManager.instance.playerLocomotion.enabled = true;
            PlayerManager.instance.cameraHandle.enabled = true;
            PlayerManager.instance.isTalkingWithNPC = false;
            MouseManager.instance.HideCursor();
            MouseManager.instance.permission = MousePermission.EnterPress; 
        }

        if (isKeyboardTutorial)
        {
            PlayerManager.instance.isTransitioningToIdle = false;

            // Đảm bảo input được reset trước khi activate
            if (PlayerManager.instance.inputHandle != null)
            {
                PlayerManager.instance.inputHandle.ResetAllInputValues();
            }

            PlayerManager.instance.inputHandle.enabled = true;
            PlayerManager.instance.inputHandle.inputActions.PlayerMovement.Movement.Enable();
            PlayerManager.instance.playerLocomotion.enabled = true;
            PlayerManager.instance.cameraHandle.enabled = true;
            PlayerManager.instance.isTalkingWithNPC = false;
        }

        if (isShowHUDTutorial)
        {
            PlayerManager.instance.inputHandle.enabled = false;
            PlayerManager.instance.playerLocomotion.enabled = false;  // KHÔNG gọi HandleMovement nữa
            PlayerManager.instance.cameraHandle.enabled = false;
            PlayerManager.instance.playerLocomotion.rigidbody.velocity = Vector3.zero;

            // Reset camera velocity để ngăn camera tiếp tục xoay do quán tính
            if (PlayerManager.instance.cameraHandle != null)
            {
                PlayerManager.instance.cameraHandle.ResetCameraVelocity();
            }

            // Reset animator parameters để ngăn animation tiếp tục chạy
            if (PlayerManager.instance.anim != null)
            {
                PlayerManager.instance.anim.SetFloat("Vertical", 0f);
                PlayerManager.instance.anim.SetFloat("Horizontal", 0f);
            }

            // Reset tất cả input values để ngăn character tiếp tục di chuyển
            if (PlayerManager.instance.inputHandle != null)
            {
                PlayerManager.instance.inputHandle.ResetAllInputValues();
            }

            // Reset player manager flags
            PlayerManager.instance.isSprinting = false;

            PlayerManager.instance.uiManager.hudWindow.Activate();
            PlayerManager.instance.mouseManager.ShowCursor();
            PlayerManager.instance.isTalkingWithNPC = true;
        }

        if (isEndPhase1Tutorial)
        {
            TelePort.instance.ReturnToTutorial_EndPhase1();
            PlayerManager.instance.uiManager.hudWindow.Activate();
            MouseManager.instance.permission = MousePermission.OnlyQuest;
            if (TutorialManager.Instance.tutorialFieldInstance != null)
                Destroy(TutorialManager.Instance.tutorialFieldInstance.gameObject);
        }
        if (isEndEnterPressTutorial)
        {          
            MouseManager.instance.permission = MousePermission.None;
            MouseManager.instance.ShowCursor();
        }


    }

    // Update is called once per frame
    void Update()
    {
        if ((MouseManager.instance.permission == MousePermission.All || MouseManager.instance.permission == MousePermission.EnterPress)
           && Input.GetKeyDown(KeyCode.E) && !PlayerManager.instance.isInteract)
        {
            Scene.Deactivate();
            if (!isQuizPhaseTutorial)
            {
                TutorialManager.Instance.ShowNextStepDelayed();
            }
            else
            {
                if (TutorialManager.Instance.currentUI != null)
                    Destroy(TutorialManager.Instance.currentUI.gameObject);
            }
            NextStepPrepare();
        }
    }
}
