using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro.Examples;
using TMPro;
using static GlobalResponseData_Login;
using UnityEngine.Events;
public class ChooseCharacter : MonoBehaviour
{
    [Header("UI Panels (hoặc Buttons)")]
    [SerializeField] private GameObject character1UI;
    [SerializeField] private GameObject character2UI;

    [SerializeField] private TextMeshProUGUI CharacterName;

   

    // 1 = chọn nhân vật 1 (female), 2 = chọn nhân vật 2 (male)
    private int selectedCharacter = 0;
    private GameObject currentUI;

   


private void Start()
    {
        // Ban đầu cả 2 UI đều hiển thị để người chơi chọn
        character1UI.SetActive(false);
        character2UI.SetActive(true);
        currentUI = character1UI;
        selectedCharacter = 1;
    }

    public void SelectCharacter1()
    {
        if (currentUI != null)
            currentUI.SetActive(true);

        character1UI.SetActive(false);
        currentUI = character1UI;
        selectedCharacter = 1;
    }

    public void SelectCharacter2()
    {
        if (currentUI != null)
            currentUI.SetActive(true);

        character2UI.SetActive(false);
        currentUI = character2UI;
        selectedCharacter = 2;
    }

    // Gắn hàm này vào nút Xác nhận
    public void ConfirmSelection()
    {
        if (selectedCharacter == 0)
        {
            Debug.LogWarning("Bạn chưa chọn nhân vật!");
            return;
        }

        // Lưu lựa chọn vào PlayerPrefs
        GlobalResponseData.CharacterName = CharacterName.text; 
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacter);
        PlayerPrefs.Save();
        GlobalResponseData.gender = selectedCharacter;

    }
}
