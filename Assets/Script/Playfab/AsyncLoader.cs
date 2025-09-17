using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GlobalResponseData_Login;
using UnityEngine.Events;
public class AsyncLoader : MonoBehaviour
{
    public static AsyncLoader Instance;

    [Header("Menu")]
    [SerializeField] private GameObject loadingscreen;
    [SerializeField] private GameObject Menu;

    public PlayerDataLoader playerDataLoader;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    private UnityEventCallState[] _originalStates;
    [SerializeField] private Button button;

    private void Awake()
    {
        Instance = this;

        int count = button.onClick.GetPersistentEventCount();
        _originalStates = new UnityEventCallState[count];
        for (int i = 0; i < count; i++)
        {
            _originalStates[i] = button.onClick.GetPersistentListenerState(i);
        }
    }

    public void ApplyToggle(bool condition)
    {
        for (int i = 0; i < _originalStates.Length; i++)
        {
            if (condition)
            {
                // Giữ nguyên như lúc Awake
                button.onClick.SetPersistentListenerState(i, _originalStates[i]);
            }
            else
            {
                // Đảo Off <-> RuntimeOnly; nếu listener gốc là các trạng thái khác thì bạn có thể bổ sung thêm
                var orig = _originalStates[i];
                var flipped = (orig == UnityEventCallState.Off)
                    ? UnityEventCallState.RuntimeOnly
                    : (orig == UnityEventCallState.RuntimeOnly)
                        ? UnityEventCallState.Off
                        : orig;
                button.onClick.SetPersistentListenerState(i, flipped);
            }
        }
    }

    public void LoadlevelBtn(string leveltoload)
    {
        Menu.SetActive(false);
        loadingscreen.SetActive(true);
        StartCoroutine(LoadLevelAsync(leveltoload));
    }


    public void LoadSaveData()
    {
        playerDataLoader.LoadPlayerData();

    }

    public void CheckLoadSaveData()
    {
        if(GlobalResponseData.FirstTimeQuest == 1)
        {
            LoadlevelBtn("MainScene");  
        }
    }

    IEnumerator LoadLevelAsync(string leveltoload)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(leveltoload);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = progressValue;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }
}
