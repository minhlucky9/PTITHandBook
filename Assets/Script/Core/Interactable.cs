using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Interactable : MonoBehaviour
    {
        public float radius = 0.6f;
        public string interactableText;
        public bool isInteracting = false;


        #region OnDrawGizmosSelected() là gì
        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        /* OnDrawGizmosSelected() là một hàm đặc biệt trong Unity được gọi bởi Editor (trong Scene View) để vẽ gizmos chỉ khi GameObject đang được chọn (selected). */

        /* OnDrawGizmosSelected() chỉ có tác dụng trong Unity Editor và không hiển thị trong bản build của game, chỉ dùng cho debug, không ảnh hưởng đến gameplay */

        /* OnDrawGizmosSelected() ở dưới giúp chúng vẽ vùng tròn màu xanh dương xung quanh nhân vật để thể hiện khoảng không gian có thể tương tác với NPC */

        /*--------------------------------------------------------------------------------------------------------------------------------------------*/

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, radius);
        }

        public virtual void Interact()
        {
      
        }

        public virtual void StopInteract()
        {
    
        }
    }
}

