using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("Assign ở Inspector")]
    [SerializeField] private GameObject maleModel;
    [SerializeField] private GameObject femaleModel;

    private void Awake()
    {
        // Đọc lựa chọn (mặc định là 1 nếu chưa có)
        int sel = PlayerPrefs.GetInt("SelectedCharacter", 1);

        // Nếu sel == 1 → female, sel == 2 → male
        femaleModel.SetActive(sel == 1);
        maleModel.SetActive(sel == 2);
    }
}
