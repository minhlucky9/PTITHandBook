using GameManager;
using Interaction;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_Scene_3 : MonoBehaviour
{
    public Button NextScene_Btn;
    public UIAnimationController Scene;
    void Start()
    {
        NextScene_Btn.onClick.AddListener(() =>
        {
            Scene.Deactivate();
            TutorialManager.Instance.ShowNextStepDelayed();
            NextStepPrepare();

        });
    }

    private void NextStepPrepare()
    {
        // Reset transition flag
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
