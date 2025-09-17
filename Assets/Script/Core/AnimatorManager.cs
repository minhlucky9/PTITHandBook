using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class AnimatorManager : MonoBehaviour
    {
        public Animator anim;
        public bool canRotate;

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, float delay = 0f)
        {
            anim.applyRootMotion = isInteracting;
            anim.SetBool("canRotate", false);
            anim.SetBool("isInteracting", isInteracting);

            PlayerManager.instance.isInteracting = isInteracting;

            StartCoroutine(RunAnimation(targetAnim, delay));
        }

        public virtual void TakeCriticalDamageAnimationEvent()
        {

        }

        private IEnumerator RunAnimation(string targetAnim, float delay)
        {
            yield return new WaitForSeconds(delay);
            anim.CrossFade(targetAnim, 0.2f);

            yield return null;
        }
    }
}

