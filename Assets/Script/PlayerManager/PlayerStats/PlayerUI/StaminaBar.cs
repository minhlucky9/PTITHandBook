using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerStatsController
{
    public class StaminaBar : MonoBehaviour
    {
        public Slider slider;
        public Gradient gradient;
        public Image fill;

        private void Awake()
        {
            slider = GetComponent<Slider>();    
        }

        public void SetMaxStamina(float maxStamina)
        {
            slider.maxValue = maxStamina;
            slider.value = maxStamina;

            fill.color = gradient.Evaluate(1f);
        }

        public void SetCurrentStamina(float currentStamina)
        {
            slider.value = currentStamina;

            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }
}



