using Core;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStatsController
{
    public class PlayerStats : CharacterStats
    {
        public static PlayerStats instance;
        PlayerManager playerManager;
        public HealthBar healthBar;
        StaminaBar staminaBar;
        PlayerAnimatorHandle animatorHandle;

        public float staminaRegenerationAmount = 66.67f;
        public float regenerationStaminaTimer = 0;

        public int soulCount;
        private void Awake()
        {
            instance = this;
            staminaBar = FindAnyObjectByType<StaminaBar>();
            playerManager = GetComponent<PlayerManager>();
            animatorHandle = GetComponentInChildren<PlayerAnimatorHandle>();
        }

        private void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetCurrentHealth(currentHealth);

            maxStamina = SetMaxStaminaFromStaminaLevel();
            currentStamina = maxStamina;
            staminaBar.SetMaxStamina(maxStamina);
            staminaBar.SetCurrentStamina(currentStamina);
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        private float SetMaxStaminaFromStaminaLevel()
        {
            maxStamina = staminaLevel * 20;
            return maxStamina;
        }

        public void TakeDamageNoAnimation(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            healthBar.SetCurrentHealth(currentHealth);
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
            }  
        }

        public void TakeDamage(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            healthBar.SetCurrentHealth(currentHealth);

            if(currentHealth <= 0)
            {
                HandleDeath();
            } else
            {
                animatorHandle.PlayTargetAnimation("Damage_01", true);
            }
        }

        private void HandleDeath()
        {
            currentHealth = 0;
            animatorHandle.PlayTargetAnimation("Death_01", true);
            isDead = true;
        }

        public void IncreaseSpeedMultiplier(float amount)
        {
            speedMultiplier = speedMultiplier + amount;
        }

        public void ReduceSpeedMultiplier(float amount)
        {
            speedMultiplier -= amount;
            speedMultiplier = Mathf.Clamp(speedMultiplier, 1, float.PositiveInfinity);
        }

        public void ReduceStamina(int amount)
        {
            if (isLockStamina) return;

            regenerationStaminaTimer = 0;
            currentStamina -= amount;
            staminaBar.SetCurrentStamina((int)currentStamina);
        }

        public void RegenerateStamina()
        {
 
            if (playerManager.isInteracting)
            {
                regenerationStaminaTimer = 0;
            }
            else
            {
                regenerationStaminaTimer += Time.deltaTime;

                if (currentStamina < maxStamina && regenerationStaminaTimer > 1f)
                {
                    currentStamina += staminaRegenerationAmount * Time.deltaTime;
                    staminaBar.SetCurrentStamina((int)currentStamina);
                }
            }

        }

        public void HealPlayer(int healAmount)
        {
            currentHealth = currentHealth + healAmount;

            if(currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            healthBar.SetCurrentHealth(currentHealth);
        }
    }
}

