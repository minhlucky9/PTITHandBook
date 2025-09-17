using Core;
using GameManager;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GlobalResponseData_Login;

namespace PlayerController
{

    public class PlayerManager : CharacterManager
    {
        public static PlayerManager instance;


        [HideInInspector] public InputHandle inputHandle;
        [HideInInspector] public UIManager uiManager;
        [HideInInspector] public MouseManager mouseManager;
        [HideInInspector] public Animator anim;
        [HideInInspector] public CameraHandle cameraHandle;
        [HideInInspector] public PlayerAnimatorHandle animatorHandle;
        [HideInInspector] public PlayerStats playerStats;
        [HideInInspector] public PlayerLocomotion playerLocomotion;
        [HideInInspector] public UIAnimationController interactionPopup;
        [HideInInspector] public TMP_Text interactionText;

        public GameObject itemPopup;

        public bool isInteracting;
        public bool isInteract;
        public bool usingAnimationMove;
        public bool isTalkingWithNPC = false;

        public bool isTransitioningToIdle = false;

        [Header("Player Flags")]
        public bool isSprinting;
        public bool isInAir;
        public bool isJumping;
        public bool isGrounded;
        public bool isOnWall;
        public bool canDoubleJump;
        public bool isClimbable;
        public bool isMouseTutor = false;
        LayerMask lootableMask;
        LayerMask talkableMask;

        [Header("Assign ở Inspector")]
        [SerializeField] private GameObject maleModel;
        [SerializeField] private GameObject femaleModel;
        [SerializeField] private GameObject maleAvatar;
        [SerializeField] private GameObject femaleAvatar;

        private void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;
            // Đọc lựa chọn (mặc định là 1 nếu chưa có)
            int sel = GlobalResponseData.gender;

            // Nếu sel == 1 → female, sel == 2 → male
            if (sel == 1)
            {
                femaleModel.SetActive(true);
                femaleAvatar.SetActive(true);

                maleModel.SetActive(false);
                maleAvatar.SetActive(false);
            }
            else
            {
                maleAvatar.SetActive(true);
                maleModel.SetActive(true);

                femaleModel.SetActive(false);
                femaleAvatar.SetActive(false);
            }
            

            inputHandle = GetComponent<InputHandle>();
            anim = GetComponentInChildren<Animator>();
            uiManager = FindObjectOfType<UIManager>();
            mouseManager = FindObjectOfType<MouseManager>();

            cameraHandle = FindObjectOfType<CameraHandle>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            playerStats = GetComponent<PlayerStats>();
            animatorHandle = GetComponentInChildren<PlayerAnimatorHandle>();

            //get component
            interactionText = interactionPopup.gameObject.GetComponentInChildren<TMP_Text>();

            //setup raycast mask
            lootableMask = 1 << LayerMask.NameToLayer("LootableObject");
            talkableMask = 1 << LayerMask.NameToLayer("TalkableObject");
        }

        private bool returnRequested = false;
        private Vector3 returnPosition;
        private Vector3 returnCameraOffset;



        public void RequestReturnToSafeZone(Vector3 playerPos, Vector3 camOffset)
        {
            returnRequested = true;
            returnPosition = playerPos;
            returnCameraOffset = camOffset;
        }
        private void Update()
        {
            float delta = Time.deltaTime;

            //get player flag from animator
            isInteracting = anim.GetBool("isInteracting");
            usingAnimationMove = anim.GetBool("usingAnimationMove");
            isJumping = anim.GetBool("isJumping");
            isOnWall = anim.GetBool("isOnWall");
            canDoubleJump = anim.GetBool("canDoubleJump");
            animatorHandle.canRotate = anim.GetBool("canRotate");

            //set player flag in animator 
            anim.SetBool("isInAir", isInAir);
            anim.SetBool("isDead", playerStats.isDead);

            // Chỉ xử lý input và movement khi inputHandle được enable hoặc đang transition
            if (inputHandle.enabled || isTransitioningToIdle)
            {
                // Trong lúc transition, không capture input mới nhưng vẫn xử lý logic
                if (inputHandle.enabled && !isMouseTutor)
                {
                    //capture input press event
                    inputHandle.TickInput(delta);
                    CheckForInteractableObject();
                }

                //disable rigidbody when not moving
                playerLocomotion.rigidbody.isKinematic = !(inputHandle.isMoveInputsPressed() || isInAir || isJumping || isOnWall || usingAnimationMove);

                //handle input event (với input = 0 trong lúc transition)
                playerLocomotion.HandleJumping(delta);
                playerLocomotion.HandleRollingAndSprinting(delta);
            }

            // Stamina regeneration luôn chạy
            playerStats.RegenerateStamina();
        }

        private void FixedUpdate()
        {
            if (playerLocomotion.enabled == false) return;

            if (!inputHandle.enabled && !isTransitioningToIdle)
            {
                // Chỉ đảm bảo rigidbody dừng lại khi hoàn toàn deactivated
                if(!playerLocomotion.rigidbody.isKinematic) playerLocomotion.rigidbody.velocity = Vector3.zero;
                return;
            }

            float delta = Time.deltaTime;

            // Trong lúc transition, cho phép movement logic chạy với input = 0 để animator transition tự nhiên
            if (isTransitioningToIdle)
            {
                // Đảm bảo rigidbody không di chuyển
                if (!playerLocomotion.rigidbody.isKinematic) playerLocomotion.rigidbody.velocity = Vector3.zero;

                // Vẫn gọi HandleMovement để animator được cập nhật với moveAmount = 0
                // Điều này sẽ làm animator transition từ running về idle tự nhiên
                playerLocomotion.HandleMovement(delta);
                playerLocomotion.HandleRotation(delta);
                return;
            }

            // Logic bình thường khi input enabled
            playerLocomotion.HandleMovement(delta);
            playerLocomotion.HandleFalling(delta, playerLocomotion.moveDirection);
            playerLocomotion.HandleStepping(delta);
            //   playerLocomotion.HandleOnWall(delta);
            playerLocomotion.HandleRotation(delta);
  
        }



        private void LateUpdate()
        {
            float delta = Time.deltaTime;

            // Kiểm tra nếu game bị pause thì không xử lý camera
            if (PauseMenuUI.GameIsPaused)
            {
                goto SkipCameraUpdate;
            }

            // Xử lý teleport
            if (returnRequested && cameraHandle != null)
            {
                // Teleport player
                transform.position = returnPosition;

                // Tính toán vị trí camera mới
                Vector3 newCameraPosition = returnPosition + returnCameraOffset;

                // FORCE reset camera position và velocity
                cameraHandle.ForceResetCamera(newCameraPosition);

                // Reset camera components
                cameraHandle.cameraPivotTransform.localPosition = new Vector3(0f, cameraHandle.unlockPivotPosition, 0f);
                cameraHandle.cameraPivotTransform.localRotation = Quaternion.identity;
                cameraHandle.cameraTransform.localPosition = new Vector3(0f, 0f, cameraHandle.defaultPosition);

                returnRequested = false;

                // SKIP camera update trong frame này
                goto SkipCameraUpdate;
            }

            // Camera logic bình thường
            if (cameraHandle != null)
            {
                cameraHandle.FollowTarget(delta);

                if (!isInteract)
                {
                    cameraHandle.HandleCameraRotation(delta, inputHandle.mouseX, inputHandle.mouseY);
                }
            }

        SkipCameraUpdate:

            // Reset input flags
            inputHandle.rollFlag = false;
            inputHandle.pick_up = false;
            inputHandle.talk_input = false;
            inputHandle.jump_input = false;
            inputHandle.inventory_input = false;

            if (isInAir)
            {
                playerLocomotion.inAirTimer += Time.deltaTime;
            }
        }

        public void ActivateController()
        {
            // Reset transition flag
            isTransitioningToIdle = false;
            isTalkingWithNPC = false;

            // Đảm bảo input được reset trước khi activate
            if (inputHandle != null)
            {
                inputHandle.ResetAllInputValues();
            }

            inputHandle.enabled = true;
            playerLocomotion.enabled = true;
            cameraHandle.enabled = true;
            uiManager.hudWindow.Activate();
            mouseManager.HideCursor();
            
        }
      
        public void DeactivateController()
        {

            inputHandle.enabled = false;
            playerLocomotion.enabled = false;  // KHÔNG gọi HandleMovement nữa
            cameraHandle.enabled = false;
            playerLocomotion.rigidbody.velocity = Vector3.zero;

            // Reset camera velocity để ngăn camera tiếp tục xoay do quán tính
            if (cameraHandle != null)
            {
                cameraHandle.ResetCameraVelocity();
            }

            // Reset animator parameters để ngăn animation tiếp tục chạy
            if (anim != null)
            {
                anim.SetFloat("Vertical", 0f);
                anim.SetFloat("Horizontal", 0f);
            }

            // Reset tất cả input values để ngăn character tiếp tục di chuyển
            if (inputHandle != null)
            {
                inputHandle.ResetAllInputValues();
            }

            // Reset player manager flags
            isSprinting = false;

            uiManager.hudWindow.Deactivate();
            mouseManager.ShowCursor();
            isTalkingWithNPC = true;
        }

        public void StartTransitionToIdle()
        {
            // Đánh dấu đang trong quá trình transition
            isTransitioningToIdle = true;

            // Ngừng input nhưng KHÔNG reset animator parameters ngay lập tức
            inputHandle.enabled = false;

            // Reset input values trong InputHandle để không có input mới
            if (inputHandle != null)
            {
                inputHandle.ResetAllInputValues();
            }

            // Ngừng rigidbody movement
            playerLocomotion.rigidbody.velocity = Vector3.zero;

            // KHÔNG reset animator parameters ở đây - để nó tự transition
            // Reset player flags
            isSprinting = false;
        }

        public void CompleteDeactivateController()
        {
            // Kết thúc quá trình transition
            isTransitioningToIdle = false;

            // Hoàn thành việc deactivate các components còn lại
            playerLocomotion.enabled = false;
            cameraHandle.enabled = false;

            // Reset camera velocity
            if (cameraHandle != null)
            {
                cameraHandle.ResetCameraVelocity();
            }

            // UI changes
            uiManager.hudWindow.Deactivate();
            mouseManager.ShowCursor();
            isTalkingWithNPC = true;
        }



        #region Player Interaction

        public void CheckForInteractableObject()
        {
            RaycastHit hit;
            //sphere cast cannot detect collider in overlap sphere
            if (Physics.SphereCast(transform.position - transform.forward * 0.5f, 0.5f, transform.forward, out hit, 1f, talkableMask))
            {
                if (hit.collider.tag == "Talkable")
                {
                    Interactable interactableObject = hit.collider.GetComponent<TalkInteraction>();

                    if (interactableObject != null)
                    {
                        string interactableText = interactableObject.interactableText;
                        //
                        interactionText.text = interactableText;
                        interactionPopup.Activate();

                        if (inputHandle.talk_input)
                        {
                            interactableObject.Interact();

                        }
                    }
                }
            }
            else
            {
                if (interactionPopup != null)
                {
                    interactionPopup.Deactivate();
                }

                if (itemPopup != null && inputHandle.pick_up)
                {
                    //itemPopup.SetActive(false);
                }

            }
        }

        public void OpenChestInteraction(Transform playerStandingPosition)
        {
            //playerLocomotion.rigidbody.velocity = Vector3.zero;
            transform.position = playerStandingPosition.position;
            //

        }
        #endregion
    }

}
