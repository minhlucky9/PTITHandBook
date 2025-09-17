using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ImageLoader : MonoBehaviour
{
    [SerializeField] private Image image;
    private string imageLink = "https://slink.ptit.edu.vn/sukien/public/qr/aDcFTzZSDoKL-zmYHhEUvKVlbujng_Uzl7jKVF_8iLWFje8v5dpoIxbsUlw0IabjElo41OILDz2vLYDjoF_NtPRigFmGEMZHU83luWn5Kr9D4GDiClJO_yuLf4a6otqs";

    private void Start()
    {
        StartCoroutine(LoadImage(imageLink));
    }

    IEnumerator LoadImage(string link)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(link);
        yield return request.SendWebRequest();

        if(request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);   
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite newsprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            image.sprite = newsprite;
        }
    }
}