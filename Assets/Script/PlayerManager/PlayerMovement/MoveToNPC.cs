using GameManager;
using PlayerController;
using PlayerStatsController;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveToNPC : MonoBehaviour
{
    public Transform npcTarget;
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerAnimatorHandle animatorHandle;
    private InputHandle inputHandle; // Thêm reference để disable input
    private PlayerLocomotion playerLocomotion; // Reference để lấy speed values
    private PlayerStats playerStats; // Reference để lấy speedMultiplier
    private CameraHandle cameraHandle; // Reference để tìm CameraHandle (chỉ để kiểm tra)

    public float stopDistance = 1.5f; // Khoảng cách an toàn để dừng lại
    public float navMeshCheckDistance = 5f;
    private bool hasStoppedNearNPC = false;
    public bool isAutoMoving = false; // Flag để xác định có đang tự động di chuyển
    public static MoveToNPC instance; // Để PlayerLocomotion truy cập nhanh

    void Start()
    {
        instance = this;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animatorHandle = GetComponentInChildren<PlayerAnimatorHandle>();
        inputHandle = GetComponent<InputHandle>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        playerStats = GetComponent<PlayerStats>();
        cameraHandle = FindObjectOfType<CameraHandle>(); // Tìm CameraHandle trong scene (chỉ để kiểm tra)
        
        // ĐẢM BẢO TRẠNG THÁI BAN ĐẦU ĐÚNG
        agent.enabled = false;
        isAutoMoving = false;
        
        // Đảm bảo InputHandle được bật khi bắt đầu
        if (inputHandle != null)
        {
            inputHandle.enabled = true;
            inputHandle.EnableMovementOnly(); // Đảm bảo movement input được bật
        }
        
        SetOptimalSpeed();
        //agent.acceleration = 100f; // Đạt tốc độ tối đa siêu nhanh
        //agent.angularSpeed = 360f;
        agent.stoppingDistance = 0.1f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    void Update()
    {
        // KIỂM TRA XUNG ĐỘT CHỈ VỚI MOVEMENT - Camera input vẫn được phép
        if (agent.enabled && inputHandle != null && 
            (inputHandle.horizontal != 0 || inputHandle.vertical != 0 || inputHandle.moveAmount > 0))
        {
            Debug.LogWarning("XUNG ĐỘT MOVEMENT PHÁT HIỆN! Reset movement input");
            inputHandle.DisableMovementOnly();
        }
        
        // Bấm H để bật/tắt auto move
        //if (Input.GetKeyDown(KeyCode.H) && npcTarget != null)
        //{
        //    if (isAutoMoving)
        //    {
        //        StopAutoMove();
        //        hasStoppedNearNPC = false;
        //    }
        //    else
        //    {
        //        StartAutoMove();
        //    }
        //}

        // Nếu đã gần NPC thì dừng và tự động tắt NavMeshAgent
        if (npcTarget != null && isAutoMoving && agent.enabled)
        {
            float distance = Vector3.Distance(transform.position, npcTarget.position);
            playerLocomotion.rigidbody.Sleep();
            // Nếu đã gần NPC thì dừng
            if (distance <= stopDistance)
            {
                // Ngay lập tức dừng NavMeshAgent để tránh xung đột
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                
                // Delay việc tắt hoàn toàn để tránh xung đột
                StartCoroutine(DelayedStopAutoMove());
                hasStoppedNearNPC = true;
            }
            else
            {
                agent.isStopped = false;
                hasStoppedNearNPC = false;
            }
        }

        // Khi dừng lại gần NPC, xoay mặt về phía NPC
        if (hasStoppedNearNPC && npcTarget != null)
        {
            Vector3 dir = npcTarget.position - transform.position;
            dir.y = 0; // Không xoay theo trục Y
            if (dir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }

        // Vô hiệu hóa WASD input khi đang auto move - logic đã có trong PlayerLocomotion
        // Không cần reset input values vì PlayerLocomotion đã chặn khi isAutoMoving = true

        // Cập nhật animation tương tự như system di chuyển thường
        if (animatorHandle != null && isAutoMoving)
        {
            float speed = agent.velocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / agent.speed);

            // Sử dụng hệ thống animation giống như PlayerLocomotion với sprint = true để có animation chạy nhanh
            bool isSprinting = speed > 5f; // Nếu tốc độ > 5 thì coi như đang sprint
            animatorHandle.UpdateAnimatorValues(normalizedSpeed, 0, isSprinting);
        }

        // Không cần AUTO CAMERA nữa vì người chơi có thể tự do điều khiển camera bằng chuột
    }

    // Method để dừng tự động di chuyển và bật lại điều khiển WASD
    public void StopAutoMove()
    {
        // Tắt NavMeshAgent và reset hoàn toàn
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.enabled = false;
        }
        
        isAutoMoving = false;
        hasStoppedNearNPC = false;
        
        // Đảm bảo Rigidbody được reset và sẵn sàng
        if (playerLocomotion != null && playerLocomotion.rigidbody != null)
        {
            playerLocomotion.rigidbody.velocity = Vector3.zero;
            playerLocomotion.rigidbody.angularVelocity = Vector3.zero;
            playerLocomotion.rigidbody.isKinematic = false;
        }
        
        // Delay việc bật lại InputHandle để tránh xung đột frame cuối
        StartCoroutine(DelayedEnableInputHandle());
    }

    // Method để bắt đầu auto move đơn giản
    public void StartAutoMove()
    {
        // KIỂM TRA NAVMESH THÔNG MINH TRƯỚC KHI BẬT AUTO MOVE
        if (!CanStartAutoMove())
        {
            ShowNavMeshWarning();
            return; // Dừng lại không thực hiện auto move
        }

        isAutoMoving = true;
        agent.enabled = true;
        SetOptimalSpeed();

        // Nếu không ở đúng trên NavMesh, warp đến vị trí gần nhất
        Vector3 validPosition = GetNearestNavMeshPosition();
        if (validPosition != transform.position)
        {
            agent.Warp(validPosition);
            Debug.Log($"Warped player to valid NavMesh position: {validPosition}");
        }

        agent.SetDestination(npcTarget.position);
        PlayerInventory.instance.SubtractGold(20);
        hasStoppedNearNPC = false;

        // CHỈ TẮT MOVEMENT INPUT, GIỮ CAMERA INPUT
        if (inputHandle != null)
        {
            inputHandle.DisableMovementOnly();
        }

        // KHÔNG TẮT CAMERAHANDLE - Giữ nguyên để có thể dùng chuột điều khiển camera
    }

    bool CanStartAutoMove()
    {
        // Kiểm tra khoảng cách đến NavMesh gần nhất
        //NavMeshHit navHit;
        Vector3 nearestPoint = GetNearestNavMeshPosition();
        float distanceToNavMesh = Vector3.Distance(transform.position, nearestPoint);

        // Nếu khoảng cách <= navMeshCheckDistance thì cho phép
        return distanceToNavMesh <= navMeshCheckDistance;
    }

    Vector3 GetNearestNavMeshPosition()
    {
        NavMeshHit navHit;
        // Tìm điểm NavMesh gần nhất trong phạm vi rộng hơn
        if (NavMesh.SamplePosition(transform.position, out navHit, navMeshCheckDistance * 200f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        // Nếu không tìm thấy, trả về vị trí hiện tại
        return transform.position;
    }

    void ShowNavMeshWarning()
    {
        Debug.LogWarning("Player quá xa NavMesh! Hiển thị UI cảnh báo.");

        TalkInteraction.instance.StartCoroutine(TalkInteraction.instance.SmoothTransitionToTraceMiniGame());
        MouseManager.instance.OpenNavMeshUI();


    }

    // Coroutine để delay việc bật lại InputHandle
    System.Collections.IEnumerator DelayedEnableInputHandle()
    {
        Debug.Log("Bắt đầu delay để bật lại InputHandle...");
        
        // Đợi 3 frames để đảm bảo NavMeshAgent hoàn toàn ngừng hoạt động
        yield return null;
        yield return null;
        yield return null;
        
        Debug.Log("Bật lại Movement Input và giữ nguyên Camera Input");
        // Bật lại Movement Input - Camera input vẫn luôn hoạt động
        if (inputHandle != null)
        {
            inputHandle.EnableMovementOnly();
        }
        
        // Không cần bật lại CameraHandle vì không tắt nó
    }
    
    // Coroutine để delay việc tắt auto khi đến NPC
    System.Collections.IEnumerator DelayedStopAutoMove()
    {
        Debug.Log("Bắt đầu quá trình tắt auto-move...");
        
        // Đợi 2 frames để đảm bảo NavMeshAgent dừng hoàn toàn
        yield return null;
        yield return null;
        
        Debug.Log("Tắt auto-move hoàn toàn và bật lại WASD");
        // Giờ mới tắt hoàn toàn và bật lại WASD
        StopAutoMove();
    }
    
    // Method tiện ích để kiểm tra xem player có ở trong NavMesh không
    public bool IsPlayerOnNavMesh()
    {
        NavMeshHit navHit;
        return NavMesh.SamplePosition(transform.position, out navHit, 1f, NavMesh.AllAreas);
    }
    
    // Method để hiển thị thông tin debug NavMesh
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Vẽ sphere tại vị trí player với màu khác nhau tùy theo trạng thái NavMesh
            if (IsPlayerOnNavMesh())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Vẽ đường đến NPC nếu có
            if (npcTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, npcTarget.position);
            }
        }
    }
    
    // Coroutine để delay việc bật lại input
    System.Collections.IEnumerator DelayedEnableInput()
    {
        yield return null; // Đợi 1 frame
        // Reset input để đảm bảo sạch sẽ
        if (inputHandle != null)
        {
            inputHandle.ResetAllInputValues();
        }
    }
    
    // Tính toán và set tốc độ tối đa dựa trên stamina system  
    void SetOptimalSpeed()
    {
        if (playerStats != null)
        {
            // Gấp đôi tốc độ sprint thực tế
            float sprintSpeed = 5f;
            float maxSpeed = sprintSpeed * playerStats.speedMultiplier;
            agent.speed = maxSpeed;
        }
        else
        {
            agent.speed = 14f;
        }
    }
    
    // Vô hiệu hóa movement input
    void DisableMovementInput()
    {
        if (inputHandle != null)
        {
            inputHandle.ResetAllInputValues(); // Reset tất cả input values về 0
        }
    }
    
    // Bật lại movement input  
    void EnableMovementInput()
    {
        if (inputHandle != null)
        {
            inputHandle.ResetAllInputValues(); // Reset để đảm bảo không có input nào bị "dính"
        }
    }
}

