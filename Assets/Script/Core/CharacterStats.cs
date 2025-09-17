using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CharacterStats : MonoBehaviour
    {
        public int characterLevel;
        public int characterExp = 0;

        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        [Header("Movement Stats")]
        public int staminaLevel = 10;
        public float maxStamina;
        public float currentStamina;
        public float speedMultiplier = 1;
        public bool isLockStamina = false;


        public bool isDead;

        public void ExpGain(int amount)
        {
            characterExp += amount;
            if(characterExp >= 100)
            {
                characterLevel++;
                characterExp = characterExp % 100;
            }
        }
    }

}
