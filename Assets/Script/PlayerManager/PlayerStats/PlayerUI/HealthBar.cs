using Interaction;
using PlayerController;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerStatsController
{
    public class HealthBar : MonoBehaviour
    {
        [Header("UI Components")]
        public Slider slider;
        public Gradient gradient;
        public Image fill;

        [Header("Warning & Death UIs")]
        public UIAnimationController lowHealthUI;
        public UIAnimationController deadUI;
        public Button resumeDrainButton;  // nút trên UI cảnh báo máu thấp

        [Header("Auto Drain Settings")]
        [Tooltip("Interval (in seconds) between each health tick")]
        [SerializeField] private float tickInterval = 12f;
        [Tooltip("How much health to subtract each tick")]
        [SerializeField] private float damagePerTick = 1f;

        [Header("Respawn Settings")]
        public Transform respawnPoint;

        [SerializeField]private float currentHealth;
        private Coroutine drainCoroutine;
        private bool lowHealthAlerted = false;
        private bool isDeadHandled = false;

        public static HealthBar instance;

        private void Awake()
        {
            instance = this;
            if (slider == null)
                slider = GetComponent<Slider>();

            // Đăng ký nút resume
            if (resumeDrainButton != null)
                resumeDrainButton.onClick.AddListener(ResumeDrain);
        }

        private void Start()
        {
            currentHealth = slider.maxValue;
            drainCoroutine = StartCoroutine(DrainHealthOverTime());
        }

        public void SetMaxHealth(float maxHealth)
        {
            slider.maxValue = maxHealth;
            slider.value = maxHealth;
            fill.color = gradient.Evaluate(1f);
            currentHealth = maxHealth;
        }

        public void SetCurrentHealth(float newHealth)
        {
            currentHealth = newHealth;
          
            currentHealth = Mathf.Clamp(newHealth, 0f, slider.maxValue);
            slider.value = currentHealth;
            fill.color = gradient.Evaluate(slider.normalizedValue);
            if (PlayerManager.instance != null && PlayerManager.instance.isTalkingWithNPC)
            {
                // Không hiện cảnh báo máu yếu hoặc chết khi đang nói chuyện
                return;
            }
            // Nếu đã hồi phục trên ngưỡng thì cho phép cảnh báo lần sau
            if (currentHealth >= 30f)
            {
                lowHealthAlerted = false;
            }

            // Xử lý cảnh báo máu thấp chỉ lần đầu khi vượt ngưỡng
            if (currentHealth < 30f && !lowHealthAlerted && currentHealth > 0f)
            {
                lowHealthUI?.Activate();
                lowHealthAlerted = true;

                // Dừng drain khi cảnh báo hiện
                if (drainCoroutine != null)
                {
                    StopCoroutine(drainCoroutine);
                    drainCoroutine = null;
                }

                PlayerManager.instance.isInteract = true;
                PlayerManager.instance.DeactivateController();
            }

            // Xử lý chết
            if (currentHealth <= 0f && !isDeadHandled)
            {
                deadUI?.Activate();
                isDeadHandled = true;

                // Dừng Drain hoàn toàn
                if (drainCoroutine != null)
                {
                    StopCoroutine(drainCoroutine);
                    drainCoroutine = null;
                }

                PlayerManager.instance.isInteract = true;

                PlayerManager.instance.DeactivateController();
            }
        }

        private IEnumerator DrainHealthOverTime()
        {
            while (currentHealth > 0f)
            {
               
                // Nếu đang nói chuyện với NPC thì không trừ máu
                if (PlayerManager.instance != null && PlayerManager.instance.isTalkingWithNPC)
                {
                    yield return null; // Đợi frame tiếp theo, không trừ máu
                    continue;
                }
                SetCurrentHealth(currentHealth - damagePerTick);

                yield return new WaitForSeconds(tickInterval);
            }
        }

        /// <summary>
        /// Resume drain khi người chơi confirm trên UI cảnh báo
        /// </summary>
        /// 
        private void ResumeDrain()
        {
            PlayerManager.instance.isInteract = false;
            PlayerManager.instance.ActivateController();
            // Khởi động lại drain nếu đang dừng và vẫn còn máu
            if (drainCoroutine == null && currentHealth > 0f)
            {
                lowHealthUI?.Deactivate();
                drainCoroutine = StartCoroutine(DrainHealthOverTime());
            }
        }

        /// <summary>
        /// Hồi sinh nhân vật: trừ vàng, reset máu, vị trí, camera
        /// </summary>
        public void RespawnPlayer()
        {
            // Trừ 30% vàng
            int goldToLose = Mathf.FloorToInt(PlayerInventory.instance.gold * 0.3f);
            PlayerInventory.instance.SubtractGold(goldToLose);

            // Hồi lại máu
            SetCurrentHealth(slider.maxValue);
            isDeadHandled = false;

            // Tắt UI chết nếu đang hiện
            deadUI?.Deactivate();

            // Đưa nhân vật và camera về respawn
            //var player = PlayerManager.instance;
            //var cam = CameraHandle.singleton;
            //if (player != null && respawnPoint != null)
            //    player.transform.position = respawnPoint.position;

            //if (cam != null)
            //{
            //    cam.transform.position = respawnPoint.position - player.transform.forward * 3 + Vector3.up * 2;
            //    cam.ClearLockOnTarget();
            //}

            PlayerManager.instance.ActivateController();
            ConservationManager.instance.DeactivateConservationDialog();

            // Tự động resume drain
            ResumeDrain();
        }
    }
}
