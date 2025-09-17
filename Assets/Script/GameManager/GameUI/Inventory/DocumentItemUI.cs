using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocumentItemUI : MonoBehaviour
{
    public string docName;
    public Transform docContentPrefab;  // chứa layout + text image ... của tài liệu
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
     //   InventoryUIManager.instance.ShowDocumentDetail(docName, docContentPrefab);
    }
}
