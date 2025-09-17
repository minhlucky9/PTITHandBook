using Core;
using Interaction;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region TalkInteraction được sử dụng để làm gì
/*--------------------------------------------------------------------------------------------------------------------------------------------*/

/* TalkInteraction được thiết kế để làm cho NPC quay mặt về phía người chơi khi trò chuyện và trở lại hướng ban đầu khi hoàn thành. */

/*--------------------------------------------------------------------------------------------------------------------------------------------*/

#endregion

public class TalkInteraction : Interactable
{
    public static TalkInteraction instance;
    float rotationSpeed = 5f;
    PlayerManager playerManager;
    [HideInInspector] public ConservationManager conservationManager;
    Quaternion originalRotation;

    public virtual void Awake()
    {
        instance = this;
        gameObject.layer = LayerMask.NameToLayer("TalkableObject");
        gameObject.tag = "Talkable";
        interactableText = "Nhấn E để trò chuyện";

        originalRotation = transform.rotation;
        playerManager = FindObjectOfType<PlayerManager>();
        conservationManager = FindObjectOfType<ConservationManager>();
    }

    #region Phương thức Updated()

    #region Quaternion.LookRotation là gì

    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    /* Quaternion.LookRotation() là một hàm trong Unity dùng để tạo ra một rotation (quaternion) sao cho một đối tượng sẽ "nhìn về hướng chỉ định". */

    /* 💡 Dễ hiểu hơn: nó giúp bạn xoay một GameObject để hướng về phía một hướng cụ thể (thường là một Vector3 direction). */

    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    #endregion

    #region playerManager.transform.position - transform.position có ý nghĩa gì

    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    /* Đây là phép trừ giữa hai vector vị trí trong không gian 3D. Kết quả là một vector hướng đi từ NPC (transform) tới chủ thể (target).

        - target.position: vị trí của chủ thể (ví dụ, người chơi).

        - transform.position: vị trí hiện tại của NPC.

        - target.position - transform.position: vector chỉ từ NPC đến chủ thể.

      Ví dụ đơn giản:

        - Nếu target.position = (5, 0, 0)

        - Và transform.position = (2, 0, 0)

        - Thì target.position - transform.position = (3, 0, 0)

     → Vector này trỏ từ NPC tới chủ thể.

     Tổng thể:

       👉 NPC là A, chủ thể là B.

           - B - A → hướng từ A đến B → NPC quay về phía B.

           - A - B → hướng từ B đến A → NPC quay ngược (lưng quay về B).


    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    #endregion

    #endregion

    public virtual void Update()
    {
        if (isInteracting)
        {
            Quaternion tr = Quaternion.LookRotation(playerManager.transform.position - transform.position);
            HandleRotation(tr);
        }
        else
        {
            HandleRotation(originalRotation);
        }
    }

    public override void Interact()
    {
        base.Interact();
        // KHÔNG set isInteracting = true ngay lập tức
        isInteracting = true;
        PlayerManager.instance.isInteract = true;

        // Bắt đầu quá trình transition mượt mà
        StartCoroutine(SmoothTransitionToConversation());
    }

    public IEnumerator SmoothTransitionToConversation()
    {
        // Bước 1: Ngừng input nhưng vẫn để animation chạy về idle
        playerManager.StartTransitionToIdle();

        // Bước 2: Đợi animation transition về idle (khoảng 0.3-0.5 giây)
        yield return new WaitForSeconds(0.4f);

        // Bước 3: Bây giờ mới set isInteracting = true để NPC xoay về phía player
     //   isInteracting = true;

        // Bước 4: Hoàn tất deactivate controller và bắt đầu conversation
        playerManager.CompleteDeactivateController();

        // Bước 5: Bắt đầu conversation
        yield return StartConservation();
    }

    public IEnumerator SmoothTransitionToTraceMiniGame()
    {
        // Bước 1: Ngừng input nhưng vẫn để animation chạy về idle
        playerManager.StartTransitionToIdle();

        // Bước 2: Đợi animation transition về idle (khoảng 0.3-0.5 giây)
        yield return new WaitForSeconds(0.4f);

        // Bước 3: Bây giờ mới set isInteracting = true để NPC xoay về phía player
        //   isInteracting = true;

        // Bước 4: Hoàn tất deactivate controller và bắt đầu conversation
        playerManager.CompleteDeactivateController();

        
    }

    public override void StopInteract()
    {
        base.StopInteract();
        isInteracting = false;
        PlayerManager.instance.isInteract = false;
        //Stop conservation
        StartCoroutine(StopConservation());
    }

    public void StopToTeleport()
    {
        StartCoroutine(ConservationManager.instance.DeactivateConservationDialog());
    }


    #region Giải thích phương thức HandleRotation()
    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    /* Phương thức HandleRotation() nhận một parameter là một quaternion và thực hiện việc xoay NPC một cách mượt mà:

        Kiểm tra cần xoay không:

         - Quaternion.Angle(quaternion, transform.rotation) tính góc (bằng độ) giữa góc xoay hiện tại của NPC và góc xoay đích.

         - Nếu góc này > 0.01 độ, nghĩa là NPC cần xoay để đạt được hướng đích.

                  ----------------------------------------------------------------------------------------------

        Xoay mượt mà:

         - Quaternion.Slerp() (Spherical Linear Interpolation - nội suy tuyến tính cầu) là hàm quan trọng tạo ra sự chuyển động mượt mà khi xoay.

         - Nó nhận ba tham số:

           + Xoay hiện tại (transform.rotation)

           + Xoay đích (tr)

           + Hệ số nội suy (rs * Time.deltaTime)

         - rs * Time.deltaTime đảm bảo việc xoay diễn ra với tốc độ đồng nhất bất kể tốc độ khung hình (frame rate) là bao nhiêu.

         - Hàm này trả về một quaternion nằm "giữa" góc xoay hiện tại và góc xoay đích, tạo ra chuyển động mượt mà theo thời gian.

                   ----------------------------------------------------------------------------------------------

       Hoàn thành xoay:

         - Nếu góc giữa hiện tại và đích rất nhỏ (≤ 0.01 độ), NPC đã gần như đạt đến hướng mục tiêu nên nó gán trực tiếp góc xoay đích.

    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    #endregion

    void HandleRotation(Quaternion quaternion)
    {
        if (Quaternion.Angle(quaternion, transform.rotation) > 0.01f)
        {
            float rs = rotationSpeed;
            Quaternion tr = quaternion;
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rs * Time.deltaTime);
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = quaternion;
        }
    }


    #region Các Coroutine
    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    /* StartConservation():

        - Vô hiệu hóa điều khiển người chơi thông qua playerManager.DeactivateController().

        - Chờ 0.5 giây để tạo độ trễ.

        - Kích hoạt hộp thoại thông qua conservationManager.ActivateConservationDialog().

                  ----------------------------------------------------------------------------------------------

        StopConservation():

         - Vô hiệu hóa hộp thoại thông qua conservationManager.DeactivateConservationDialog().

         - Chờ 0.7 giây để tạo độ trễ.

         - Kích hoạt lại điều khiển người chơi thông qua playerManager.ActivateController().

                   ----------------------------------------------------------------------------------------------

       Coroutine cho phép các hành động xảy ra theo tuần tự với các khoảng thời gian chờ đợi mà không làm đóng băng game.

    /*--------------------------------------------------------------------------------------------------------------------------------------------*/

    #endregion

    IEnumerator StartConservation()
    {
        // Không cần gọi DeactivateController() nữa vì đã xử lý trong SmoothTransitionToConversation()
        yield return new WaitForSeconds(0.2f); // Thời gian ngắn để UI hiện mượt mà
        yield return conservationManager.ActivateConservationDialog();
      //  isInteracting = false;
    }

    IEnumerator StopConservation()
    {
        yield return conservationManager.DeactivateConservationDialog();
        yield return new WaitForSeconds(0.7f);
        playerManager.ActivateController();

    }
}
