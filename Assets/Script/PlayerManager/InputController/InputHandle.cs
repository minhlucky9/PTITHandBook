
using GameManager;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class InputHandle : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        public bool sprint_input;
        public bool roll_input;
        public bool jump_input;
        public bool inventory_input;
        public bool pick_up;
        public bool talk_input;
        public bool lock_on_input;

        public bool rollFlag;
        public bool lockOnFlag;
        public bool sprintFlag;
        public float sprintInputTimer;

        public bool enableJumpInput = false;

        [HideInInspector] public PlayerControls inputActions;
        CameraHandle cameraHandle;
        UIWeatherManager uiManager;
        PlayerManager playerManager;
        PlayerStats playerStats;
        PlayerAnimatorHandle animatorHandle;

        Vector2 movementInput;
        Vector2 cameraInput;

        public Transform criticalAttackRayCastStartPoint;

        public static InputHandle instance;

        private void Awake()
        {
            instance = this;

        }

        private void Start()
        {
            uiManager = FindObjectOfType<UIWeatherManager>();
            playerManager = GetComponent<PlayerManager>();
            cameraHandle = FindObjectOfType<CameraHandle>();
            animatorHandle = GetComponentInChildren<PlayerAnimatorHandle>();
            playerStats = GetComponent<PlayerStats>();
        }

        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                //bind input values to parameters
                inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();

                inputActions.PlayerAction.PickUp.performed += i => pick_up = true;
                inputActions.PlayerAction.Talk.performed += i => talk_input = true;
                inputActions.PlayerAction.Jump.performed += i => jump_input = true && enableJumpInput;
                inputActions.PlayerAction.Inventory.performed += i => inventory_input = true;

                inputActions.PlayerAction.Roll.performed += i => roll_input = true;
                inputActions.PlayerAction.Roll.canceled += i => roll_input = false;

                inputActions.PlayerAction.Sprint.performed += i => sprint_input = true;
                inputActions.PlayerAction.Sprint.canceled += i => sprint_input = false;


                inputActions.PlayerAction.LockOn.performed += i => lock_on_input = true;

            }
            inputActions.Enable();
        }

        public void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            HandleMoveInput(delta);
            HandleRollInput(delta);
            HandleLockOnInput();
        }

        public bool isMoveInputsPressed()
        {
            if (movementInput != Vector2.zero || jump_input || roll_input)
                return true;

            return false;
        }

        private void HandleMoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;

        }

        private void HandleRollInput(float delta)
        {
            rollFlag = roll_input;

            if (sprint_input)
            {
                if (playerStats.currentStamina <= 0)
                {
                    sprint_input = false;
                    sprintFlag = false;
                }

                if (moveAmount > 0.5f && playerStats.currentStamina > 0)
                {
                    sprintFlag = true;
                }
            }
            else
            {
                sprintFlag = false;
            }
        }

        private void HandleLockOnInput()
        {
            if (lock_on_input && lockOnFlag == false)
            {
                lock_on_input = false;
                cameraHandle.HandleLockOn();

                if (cameraHandle.nearestLockOnTarget != null)
                {
                    cameraHandle.currentLockOnTarget = cameraHandle.nearestLockOnTarget;
                    lockOnFlag = true;
                }

            }
            else if (lock_on_input && lockOnFlag)
            {
                lock_on_input = false;
                lockOnFlag = false;
                //Clear lock on target
                cameraHandle.ClearLockOnTarget();
            }

            cameraHandle.SetCameraHeight();
        }

        public void ResetAllInputValues()
        {
            // Reset movement input vectors
            movementInput = Vector2.zero;
            cameraInput = Vector2.zero;

            // Reset processed input values
            horizontal = 0f;
            vertical = 0f;
            moveAmount = 0f;
            mouseX = 0f;
            mouseY = 0f;

            // Reset input flags
            sprint_input = false;
            roll_input = false;
            jump_input = false;
            inventory_input = false;
            pick_up = false;
            talk_input = false;
            lock_on_input = false;

            rollFlag = false;
            lockOnFlag = false;
            sprintFlag = false;
            sprintInputTimer = 0f;
        }


        // DISABLE CHỈ MOVEMENT INPUT, GIỮ CAMERA INPUT
        public void DisableMovementOnly()
        {
            if (inputActions != null)
            {
                // Tắt chỉ Movement input (WASD), giữ nguyên Camera input (mouse)
                inputActions.PlayerMovement.Movement.Disable();

                // Tắt luôn các action input như Sprint, Roll, Jump
                inputActions.PlayerAction.Sprint.Disable();
                inputActions.PlayerAction.Roll.Disable();
                inputActions.PlayerAction.Jump.Disable();

                // Reset movement values về 0 để đảm bảo không còn input dính
                movementInput = Vector2.zero;
                horizontal = 0f;
                vertical = 0f;
                moveAmount = 0f;
                sprint_input = false;
                roll_input = false;
                jump_input = false;
                sprintFlag = false;
                rollFlag = false;
            }
        }

        // ENABLE LẠI MOVEMENT INPUT
        public void EnableMovementOnly()
        {
            if (inputActions != null)
            {
                // Bật lại Movement input (WASD)
                inputActions.PlayerMovement.Movement.Enable();

                // Bật lại các action input
                inputActions.PlayerAction.Sprint.Enable();
                inputActions.PlayerAction.Roll.Enable();
                inputActions.PlayerAction.Jump.Enable();

                // Reset để đảm bảo sạch sẽ
                ResetAllInputValues();
            }
        }

    }
}


