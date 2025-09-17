using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PlayerLocomotion : MonoBehaviour
    {
        CameraHandle cameraHandle;
        PlayerManager playerManager;
        PlayerStats playerStats;
        Transform cameraObject;
        InputHandle inputHandle;
        public Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public PlayerAnimatorHandle animatorHandle;

        public new Rigidbody rigidbody;

        public GameObject normalCamera;

        public CapsuleCollider characterCollider;
        public CapsuleCollider characterCollisionBlockerCollider;

        [Header("Ground & Air Detection Stats")]
        [SerializeField]
        float groundDetectionRayStartPoint = 0.5f;
        [SerializeField]
        float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField]
        float groundDirectionRayDistance = 0.2f;
        LayerMask ignoreForGroundCheck;
        public float inAirTimer;

        [Header("Jump and Falling")]
        public float fallingVelocity = 0;
        public float gravity = 15f;
        public float stepHeight = 0.4f;
        public float stepVelocity = 1.5f;
        public float jumpVelocity = 7f;
        Vector3 wallNormal;
        float steppingTimer = 0;
        bool isStepping => steppingTimer > 0f;

        [Header("Stamina Costs")]
        [SerializeField]
        int rollStaminaCost = 15;
        int backStepStaminaCost = 12;
        int sprintStaminaCost = 1;

        [Header("Movement Stats")]
        [SerializeField]
        float walkingSpeed = 3;
        [SerializeField]
        float movementSpeed = 5;
        [SerializeField]
        float sprintSpeed = 7;
        [SerializeField]
        float rotationSpeed = 10;
        [SerializeField]
        float currentRunningSpeed = 0;

        private void Awake()
        {
            cameraHandle = FindObjectOfType<CameraHandle>();
            playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            inputHandle = GetComponent<InputHandle>();
            animatorHandle = GetComponentInChildren<PlayerAnimatorHandle>();
            playerStats = GetComponent<PlayerStats>();

        }


        void Start()
        {
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandle.Initialized();

            playerManager.isGrounded = true;
            playerManager.isClimbable = true;
            ignoreForGroundCheck = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("CharacterColliderBlocker"));

            Physics.IgnoreCollision(characterCollider, characterCollisionBlockerCollider, true);
        }

        #region Movement
        Vector3 normalVector = Vector3.up;
        Vector3 targetPosition;

        public void HandleRotation(float delta)
        {
            if (animatorHandle.canRotate)
            {
                if (inputHandle.lockOnFlag && inputHandle.sprintFlag == false)
                {
                    if (inputHandle.sprintFlag || inputHandle.rollFlag)
                    {
                        Vector3 targetDirection = Vector3.zero;
                        targetDirection = cameraHandle.cameraTransform.forward * inputHandle.vertical;
                        targetDirection += cameraHandle.cameraTransform.right * inputHandle.horizontal;
                        targetDirection.Normalize();
                        targetDirection.y = 0;

                        if (targetDirection == Vector3.zero)
                        {
                            targetDirection = transform.forward;
                        }

                        Quaternion tr = Quaternion.LookRotation(targetDirection);
                        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);

                        transform.rotation = targetRotation;
                    }
                    else
                    {
                        Vector3 rotationDirection = moveDirection;
                        rotationDirection = cameraHandle.currentLockOnTarget.position - transform.position;
                        rotationDirection.y = 0;

                        rotationDirection.Normalize();

                        Quaternion tr = Quaternion.LookRotation(rotationDirection);
                        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * Time.deltaTime);
                        transform.rotation = targetRotation;
                    }

                }
                else
                {
                    Vector3 targetDir = Vector3.zero;
                    float moveOverride = inputHandle.moveAmount;

                    targetDir = cameraObject.forward * inputHandle.vertical;
                    targetDir += cameraObject.right * inputHandle.horizontal;

                    targetDir.Normalize();
                    targetDir.y = 0;

                    if (targetDir == Vector3.zero)
                        targetDir = myTransform.forward;

                    float rs = rotationSpeed;

                    Quaternion tr = Quaternion.LookRotation(targetDir);
                    Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
                    myTransform.rotation = targetRotation;
                }
            }

            if (playerManager.isInAir)
            {
                if (moveDirection.magnitude > 0.1f)
                {
                    float rs = rotationSpeed;
                    Quaternion tr = Quaternion.LookRotation(moveDirection.normalized);
                    Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
                    myTransform.rotation = targetRotation;
                }
            }

            if (playerManager.isGrounded)
            {
                float rs = rotationSpeed;
                Quaternion tr = Quaternion.Euler(0, myTransform.rotation.eulerAngles.y, 0);
                Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);
                myTransform.rotation = targetRotation;
            }
        }

        public void HandleMovement(float delta)
        {
            if (playerManager.usingAnimationMove)
                return;

            if (inputHandle.rollFlag)
                return;

            // Cho phép HandleMovement chạy khi đang transition để animator mượt mà
            if (playerManager.isInteracting && !playerManager.isTransitioningToIdle)
                return;

            var moveToNPC = GetComponent<MoveToNPC>();
            if (moveToNPC != null && moveToNPC.isAutoMoving)
                return;

            moveDirection = cameraObject.forward * inputHandle.vertical;
            moveDirection += cameraObject.right * inputHandle.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            currentRunningSpeed = movementSpeed;

            if (inputHandle.sprintFlag && inputHandle.moveAmount > 0.5)
            {
                currentRunningSpeed = sprintSpeed;
                playerManager.isSprinting = true;
                playerStats.ReduceStamina(sprintStaminaCost);
            }
            else
            {
                if (inputHandle.moveAmount < 0.5f)
                {
                    currentRunningSpeed = walkingSpeed;
                }
                else
                {
                    currentRunningSpeed = movementSpeed;
                }
                playerManager.isSprinting = false;
            }

            moveDirection *= CalculateRunningSpeed(currentRunningSpeed);

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector).normalized * CalculateRunningSpeed(currentRunningSpeed);
            SetRigidbodyVelocity(projectedVelocity);

            if (inputHandle.lockOnFlag && inputHandle.sprintFlag == false)
            {
                animatorHandle.UpdateAnimatorValues(inputHandle.vertical, inputHandle.horizontal, playerManager.isSprinting);
            }
            else
            {
                animatorHandle.UpdateAnimatorValues(inputHandle.moveAmount, 0, playerManager.isSprinting);
            }
        }

        public void HandleStepping(float delta)
        {
            //isStepping = false;
            steppingTimer -= Time.deltaTime;
            if (playerManager.usingAnimationMove)
                return;

            if (inputHandle.rollFlag)
                return;

            if (playerManager.isInteracting)
                return;

            if (playerManager.isOnWall)
                return;

            if (playerManager.isInAir || playerManager.isJumping)
                return;

            if (moveDirection.magnitude == 0) return;

            RaycastHit hitLower45;
            Vector3 startTestStepPosition = transform.position;
            float maxDistance = 0.6f;

            Vector3 foward = moveDirection.normalized;
            Collider[] hitLowers = Physics.OverlapBox(startTestStepPosition, new Vector3(maxDistance, 0, maxDistance), Quaternion.identity, cameraHandle.environmentLayer);

            if (hitLowers.Length > 0)
            {
                Collider[] hitUppers = Physics.OverlapBox(startTestStepPosition + new Vector3(0, stepHeight, 0), new Vector3(maxDistance, 0, maxDistance), Quaternion.identity, cameraHandle.environmentLayer);
                if (hitUppers.Length == 0)
                {
                    //fix calculate raycast with all collider
                    bool a = hitLowers[0].Raycast(new Ray(startTestStepPosition, foward), out hitLower45, 0.5f);
                    Debug.Log("stepping " + a + " " + Vector3.Dot(hitLower45.normal, transform.up));
                    bool canClimbSlope = Vector3.Dot(hitLower45.normal, transform.up) < 0.1f;

                    if (a && canClimbSlope)
                    {
                        Vector3 move = moveDirection.normalized + Vector3.up;
                        SetRigidbodyVelocity(CalculateRunningSpeed(currentRunningSpeed) * move.normalized);
                        steppingTimer = 0.1f;
                    }

                    return;
                }
            }

            
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (playerManager.usingAnimationMove)
                return;

            if (playerManager.isInteracting)
                return;

            if (playerStats.currentStamina <= 0)
                return;



            if (inputHandle.rollFlag)
            {
                moveDirection += cameraObject.forward * inputHandle.vertical;
                moveDirection += cameraObject.right * inputHandle.horizontal;


                if (inputHandle.moveAmount > 0)
                {
                    animatorHandle.PlayTargetAnimation("Rolling", true);
                    animatorHandle.anim.SetBool("usingAnimationMove", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;

                    playerStats.ReduceStamina(rollStaminaCost);
                }
                //else
                //{
                //    animatorHandle.PlayTargetAnimation("Backstep", true);
                //    animatorHandle.anim.SetBool("usingAnimationMove", true);
                //    playerStats.ReduceStamina(backStepStaminaCost);
                //}
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            if (playerManager.usingAnimationMove)
                return;

            if (playerManager.isOnWall)
                return;

            //if (isStepping) return;

            playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDetectionRayStartPoint;

            //check wall to climb

            //if (playerManager.isJumping && playerManager.isInAir && playerManager.isClimbable)
            //{
            //    if (Physics.SphereCast(origin, groundDetectionRayStartPoint, moveDirection.normalized, out hit, 0.3f, cameraHandle.environmentLayer))
            //    {
            //        moveDirection = Vector3.zero;

            //        bool climbable = (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"));
            //        float dotTest = Mathf.Abs(Vector3.Dot(hit.normal, transform.up));

            //        if (climbable && dotTest < 0.2f)
            //        {
            //            //start climp wall
            //            Debug.Log("asd");
            //            animatorHandle.StartClimbing();
            //            playerManager.isClimbable = false;
            //            targetPosition = hit.point + hit.normal * 0.31f;
            //            wallNormal = hit.normal;
            //            return;
            //        }
            //    }
            //}

            //change velocity of rigidbody
            if (moveDirection.magnitude < 0.05f)
            {
                moveDirection = Vector3.zero;
            }
            Vector3 velocity = moveDirection.normalized * CalculateRunningSpeed(currentRunningSpeed) - fallingVelocity * Vector3.up;

            if (playerManager.isJumping)
            {
                //velocity += moveDirection.normalized * currentRunningSpeed;
                velocity += Vector3.up * jumpVelocity;
            }

            if(isStepping)
            {
                
                velocity += Vector3.up * stepVelocity;
                
                //if(normalVector != Vector3.up)
                //{
                //    velocity = Vector3.ProjectOnPlane(moveDirection, normalVector).normalized * CalculateRunningSpeed(currentRunningSpeed);
                //}
                
                //SetRigidbodyVelocity(CalculateRunningSpeed(currentRunningSpeed) * move.normalized);
            }

            SetRigidbodyVelocity(velocity);

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + Vector3.up * groundDirectionRayDistance;
            targetPosition = myTransform.position;

            //start falling
            if (velocity.y < 0.1f)
            {
                if (Physics.SphereCast(origin, 0.15f, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
                {
                    normalVector = hit.normal;
                    Vector3 tp = hit.point;
                    targetPosition.y = tp.y + 0.02f;
                    playerManager.isGrounded = true;

                    if (playerManager.isInAir)
                    {
                        if (inAirTimer > 0.2f)
                        {
                            Debug.Log("You were in the air for " + inAirTimer);
                            animatorHandle.PlayTargetAnimation("Land", true);
                        }
                        else
                        {
                            animatorHandle.PlayTargetAnimation("Empty", false);
                        }

                        inAirTimer = 0;
                        playerManager.isInAir = false;
                        playerManager.isClimbable = true;
                        this.moveDirection = Vector3.zero;
                        animatorHandle.StopJumping();
                        animatorHandle.StopDoubleJump();
                    }
                }
                else
                {

                    if (playerManager.isGrounded)
                    {
                        playerManager.isGrounded = false;
                    }

                    if (playerManager.isInAir == false)
                    {

                        if (playerManager.isInteracting == false)
                        {
                            animatorHandle.PlayTargetAnimation("Falling", true);
                        }

                        playerManager.isInAir = true;
                    }
                }
            } else if(velocity.y > 0.01f)
            {
                playerManager.isInAir = true && !isStepping;
            }


            if (playerManager.isGrounded)
            {
                if(!isStepping) 
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime / 0.1f);
                }

                if (!playerManager.isJumping)
                    fallingVelocity = 0;
            }
            else
            {
                fallingVelocity += gravity * Time.deltaTime;
            }

        }

        public void HandleOnWall(float delta)
        {
            if (playerManager.isOnWall)
            {
                //remove gravity
                SetRigidbodyVelocity(Vector3.zero);

                //move close to wall
                if (playerManager.isClimbable == false)
                {
                    if (Vector3.Distance(myTransform.position, targetPosition) < 0.02f)
                    {
                        myTransform.position = targetPosition;
                        playerManager.isClimbable = true;
                    }
                    else
                    {
                        myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime * 5f);
                    }
                    return;
                }

                //handle rotation
                RaycastHit hit;
                Debug.DrawRay(transform.position, -wallNormal);
                if (Physics.SphereCast(transform.position, 0.3f, -wallNormal, out hit, 0.6f))
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-hit.normal, transform.up);
                    wallNormal = wallNormal + hit.normal;
                    wallNormal.Normalize();
                    transform.forward = -wallNormal;

                    //
                    //wallNormal = hit.normal;
                }
                else
                {
                    //if(Vector3.Angle(transform.forward)
                    animatorHandle.StopClimbing();
                    return;
                }

                if (inputHandle.moveAmount > 0)
                {
                    Vector3 right = Vector3.Cross(wallNormal, transform.up);
                    Vector3 forward = Vector3.Cross(right, wallNormal);
                    Vector3 climbDirection = forward * inputHandle.vertical;
                    climbDirection += right * inputHandle.horizontal;
                    climbDirection.Normalize();

                    Vector3 projectedVelocity = Vector3.ProjectOnPlane(climbDirection, wallNormal).normalized;
                    SetRigidbodyVelocity(projectedVelocity);
                    animatorHandle.anim.SetBool("isClimbing", true);
                    inAirTimer = 0f;
                }
                else
                {
                    animatorHandle.anim.SetBool("isClimbing", false);
                }

                if (inAirTimer > 1.6f)
                {
                    //animatorHandle.StopClimbing();
                }
            }
        }

        public void HandleJumping(float delta)
        {
            if (playerManager.usingAnimationMove)
                return;

            if (playerManager.isInteracting && !playerManager.isOnWall && !playerManager.isInAir)
                return;

            //if (playerStats.currentStamina <= 0)
            //    return;

            if (inputHandle.jump_input)
            {
                if (playerManager.isOnWall)
                {
                    animatorHandle.PlayTargetAnimation("JumpFromWall", true);
                    animatorHandle.anim.SetBool("usingAnimationMove", true);

                    fallingVelocity = 0;
                    return;
                }

                if (playerManager.canDoubleJump)
                {
                    animatorHandle.PlayTargetAnimation("DoubleJump", true);
                    animatorHandle.StartJumping();
                    playerManager.isClimbable = false;
                    fallingVelocity = 0;
                    inAirTimer = 0f;
                    return;
                }

                if (playerManager.isInAir) return;

                if (inputHandle.moveAmount > 0 && rigidbody.velocity.magnitude > 0.1f)
                {
                    //moveDirection = cameraObject.forward * inputHandle.vertical;
                    //moveDirection += cameraObject.right * inputHandle.horizontal;
                    //moveDirection.y = 0;
                    //moveDirection.Normalize();
                    animatorHandle.PlayTargetAnimation("Jump", true);

                    Quaternion jumpRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = jumpRotation;
                    animatorHandle.StartJumping();
                }
                else
                {
                    animatorHandle.PlayTargetAnimation("Jump", true);
                    moveDirection = transform.forward * 0.01f;
                    animatorHandle.StartJumping();
                }
            }
        }

        public void ReflectMoveDirection()
        {
            moveDirection = wallNormal * CalculateRunningSpeed(currentRunningSpeed);
            //Vector3.Reflect(moveDirection, wallNormal);
        }

        #endregion

        public float CalculateRunningSpeed(float speed)
        {
            return speed * playerStats.speedMultiplier;
        }

        public void SetRigidbodyVelocity(Vector3 velocity)
        {
            if (!rigidbody.isKinematic)
                rigidbody.velocity = velocity;
        }
    }

}
