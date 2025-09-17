using UnityEngine;

namespace Interaction.Minigame
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Timed Image Quiz Conservation Data")]
    public class TimedImageQuizConservationSO : QuizConservationSO
    {
        [Header("Timed Image Settings")]
        [Tooltip("Số giây sẽ chỉ show hình trước khi vào câu hỏi")]
        public float imageDisplayDuration = 15f;

        public override void Init(GameObject targetGameObject)
        {
            // 1) Bật chế độ timed-image và truyền duration
            ConservationManager.instance.EnableTimedImageMode(imageDisplayDuration);
            // 2) Gọi base.Init để khởi QuizManager như bình thường
            base.Init(targetGameObject);
        }
    }
}
