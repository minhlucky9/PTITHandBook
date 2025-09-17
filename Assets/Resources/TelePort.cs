using PlayerController;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class TelePort : MonoBehaviour
{
    public static TelePort instance;
    [SerializeField] private Button targetButton;
    public Transform Player;
    public Transform Cam;
    public GameObject LoadingScreen;

    [Header("Teleport")]
    public Transform Safezone;
    public Transform HoiTruongA2;
    public Transform PhongHocA2;
    public Transform BackToHoiTruongA2;
    public Transform BackToPhongHocA2;
    public Transform Tutorial;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // Kiểm tra nếu phím Z được nhấn xuống
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    // Kích hoạt sự kiện onClick của button
        //    targetButton.onClick.Invoke();
        //}
    }

    /// <summary>
    /// Reset rotation của camera và xóa các giá trị góc xoay cũ trong CameraHandle (lookAngle, pivotAngle).
    /// </summary>
    private void ResetCameraHandleRotation(CameraHandle camHandle)
    {
        // 1) Reset biến private lookAngle & pivotAngle
        var type = typeof(CameraHandle);
        var lookField = type.GetField("lookAngle", BindingFlags.NonPublic | BindingFlags.Instance);
        var pivotField = type.GetField("pivotAngle", BindingFlags.NonPublic | BindingFlags.Instance);
        lookField.SetValue(camHandle, 0f);
        pivotField.SetValue(camHandle, 0f);

        // 2) Reset trực tiếp transform và pivot local rotation
        camHandle.transform.rotation = Quaternion.identity;
        camHandle.cameraPivotTransform.localRotation = Quaternion.identity;
    }


    #region SafeZone

    public void ReturnToSafeZone()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(TeleportSafeZone(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator TeleportSafeZone(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = Safezone.position;
        player.transform.rotation = Safezone.rotation; 
        camHandle.transform.position = Safezone.position + camOffset;

        camHandle.ClearLockOnTarget();
        ResetCameraHandleRotation(camHandle);
        // 5) Bật lại controller đúng cách (bao gồm input)
        locomotion.enabled = true;
        camHandle.enabled = true;
        player.ActivateController(); // Bật lại input và UI
        PlayerManager.instance.isInteract = false;
    }

    #endregion

    #region Tutorial

    public void ReturnToTutorial()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Tutorial == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;
        //
        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        

        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(TeleportTutorial(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator TeleportTutorial(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = Tutorial.position;
        player.transform.rotation = Tutorial.rotation;
        camHandle.transform.position = Tutorial.position + camOffset;
        camHandle.ClearLockOnTarget();
        //
        
    }

    public void ReturnToTutorial_EndPhase1()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(TeleportTutorial_EndPhase1(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator TeleportTutorial_EndPhase1(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = Safezone.position;
        player.transform.rotation = Safezone.rotation;
        camHandle.transform.position = Safezone.position + camOffset;

        camHandle.ClearLockOnTarget();
        ResetCameraHandleRotation(camHandle);



    }

    #endregion

    #region HoiTruongA2

    public void ReturnToHoiTruongA2()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(TeleportHoiTruongA2(player, camHandle, locomotion, camOffset));
    }
     
    private IEnumerator TeleportHoiTruongA2(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = HoiTruongA2.position;
        camHandle.transform.position = HoiTruongA2.position + camOffset;
        camHandle.ClearLockOnTarget();

        // 5) Bật lại controller đúng cách (bao gồm input)
        locomotion.enabled = true;
        camHandle.enabled = true;
        player.ActivateController(); // Bật lại input và UI
        PlayerManager.instance.isInteract = false;
    }

    #endregion

    #region BackToHoiTruongA2

    public void BackToReturnToHoiTruongA2()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(BackToTeleportHoiTruongA2(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator BackToTeleportHoiTruongA2(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = BackToHoiTruongA2.position;
        camHandle.transform.position = BackToHoiTruongA2.position + camOffset;
        camHandle.ClearLockOnTarget();

        // 5) Bật lại controller đúng cách (bao gồm input)
        locomotion.enabled = true;
        camHandle.enabled = true;
        player.ActivateController(); // Bật lại input và UI
        PlayerManager.instance.isInteract = false;
    }

    #endregion

    #region BackToPhongHocA2

    public void BackToReturnToPhongHocA2()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(BackToTeleportPhongHocA2(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator BackToTeleportPhongHocA2(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = BackToPhongHocA2.position;
        camHandle.transform.position = BackToPhongHocA2.position + camOffset;
        camHandle.ClearLockOnTarget();

        // 5) Bật lại controller đúng cách (bao gồm input)
        locomotion.enabled = true;
        camHandle.enabled = true;
        player.ActivateController(); // Bật lại input và UI
        PlayerManager.instance.isInteract = false;
    }

    #endregion

    #region PhongHocA2

    public void ReturnToPhongHocA2()
    {
        var player = PlayerManager.instance;
        var camHandle = CameraHandle.singleton;
        var locomotion = player.GetComponent<PlayerLocomotion>();
        if (player == null || Safezone == null || camHandle == null || locomotion == null)
            return;

        // 1) Tính offset camera (3m sau + 2m cao)
        Vector3 camOffset = -player.transform.forward * 3f + Vector3.up * 2f;

        // 2) Tắt controller đúng cách (bao gồm input)
        player.DeactivateController();
        camHandle.enabled = false;
        locomotion.enabled = false;
        LoadingScreen.SetActive(true);
        // 3) Start coroutine: đợi 1s → teleport → bật lại
        StartCoroutine(TeleportPhongHocA2(player, camHandle, locomotion, camOffset));
    }

    private IEnumerator TeleportPhongHocA2(
        PlayerManager player,
        CameraHandle camHandle,
        PlayerLocomotion locomotion,
        Vector3 camOffset)
    {
        // đợi 1 giây trước khi dịch chuyển
        yield return new WaitForSeconds(1f);
        LoadingScreen.SetActive(false);
        // 4) Teleport player và rig‐root camera
        player.transform.position = PhongHocA2.position;
        camHandle.transform.position = PhongHocA2.position + camOffset;
        camHandle.ClearLockOnTarget();

        // 5) Bật lại controller đúng cách (bao gồm input)
        locomotion.enabled = true;
        camHandle.enabled = true;
        player.ActivateController(); // Bật lại input và UI
        PlayerManager.instance.isInteract = false;
    }

    #endregion
}
