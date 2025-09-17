using Core;
using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PlayerAnimatorHandle : AnimatorManager
    {
        PlayerManager playerManager;
        PlayerStats playerStats;
        PlayerLocomotion playerLocomotion;
        InputHandle inputHandle;
        int vertical;
        int horizontal;

        public void Initialized()
        {
            anim = GetComponent<Animator>();
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            inputHandle = GetComponentInParent<InputHandle>();
            playerManager = GetComponentInParent<PlayerManager>();
            playerStats = GetComponentInParent<PlayerStats>();
            vertical = Animator.StringToHash("Vertical");
            horizontal = Animator.StringToHash("Horizontal");
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting)
        {

            #region Vertical
            float v = 0;
            if(verticalMovement > 0 && verticalMovement < 0.55f)
            {
                v = 0.5f;
            } else if(verticalMovement > 0.55f)
            {
                v = 1;
            } else if(verticalMovement < 0 && verticalMovement > -0.55f)
            {
                v = -1;
            } else if(verticalMovement < -0.55f) {
                v = -1;
            } else {
                v = 0;
            }
            #endregion

            #region Horizontal
            float h = 0;
            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
            {
                h = 0.5f;
            }
            else if (horizontalMovement > 0.55f)
            {
                h = 1;
            }
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
            {
                h = -1;
            }
            else if (horizontalMovement < -0.55f)
            {
                h = -1;
            }
            else
            {
                h = 0;
            }
            #endregion

            if(isSprinting)
            {
                v = 2;
                h = horizontalMovement;
            }

            anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
            anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
        }

        public void StartClimbing()
        {
            anim.SetBool("isOnWall", true);
            PlayTargetAnimation("StickWall", true);
            playerLocomotion.rigidbody.Sleep();
            playerLocomotion.inAirTimer = 0;
            StopJumping();
            StopDoubleJump();
        }

        public void StopClimbing()
        {
            anim.SetBool("isOnWall", false);
            playerManager.isClimbable = false;
            playerLocomotion.rigidbody.WakeUp();
        }

        public void StartJumpingFromWall()
        {
            anim.SetBool("usingAnimationMove", false);
            StopClimbing();
            StartJumping();
            playerLocomotion.ReflectMoveDirection();
        }

        public void StartJumping()
        {
            anim.SetBool("isJumping", true);
        }

        public void StopJumping()
        {
            anim.SetBool("isJumping", false);
        }

        public void CanDoubleJump()
        {
            anim.SetBool("canDoubleJump", true);
        }
        public void StopDoubleJump()
        {
            anim.SetBool("canDoubleJump", false);
        }

        public void CanRotate()
        {
            anim.SetBool("canRotate", true);
        }

        public void StopRotate()
        {
            anim.SetBool("canRotate", false);
        }

        public void EnableCombo()
        {
            anim.SetBool("canDoCombo", true);
        }

        public void DisableCombo()
        {
            anim.SetBool("canDoCombo", false);
        }

        public void EnableIsInvulnerable() 
        {
            anim.SetBool("isInvulnerable", true);
        }

        public void DisableIsInvulnerable() 
        {
            anim.SetBool("isInvulnerable", false);
        }

        public override void TakeCriticalDamageAnimationEvent()
        {
            playerStats.TakeDamageNoAnimation(playerManager.pendingCriticalDamage);
            playerManager.pendingCriticalDamage = 0;
        }

        private void OnAnimatorMove()
        {
            if (playerManager.isInteracting == false)
                return;

            if (playerManager.usingAnimationMove == false)
                return;

            float delta = Time.deltaTime;
            playerLocomotion.rigidbody.drag = 0;

            Vector3 deltaPosition = anim.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition * playerStats.speedMultiplier / delta;
            playerLocomotion.SetRigidbodyVelocity(velocity);
        }
    }
}

