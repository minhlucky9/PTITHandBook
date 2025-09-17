using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalResponseData_Login;

public class Character3D_Manager : MonoBehaviour
{
    private GameObject[] characterList;
    public int index;

    private void Start()
    {
        //  index = PlayerPrefs.GetInt("CharacterSelected");
        index = 0;

        characterList = new GameObject[transform.childCount];

         for (int i = 0; i < transform.childCount; i++)
        {
            characterList[i] = transform.GetChild(i).gameObject;

        }
         foreach (GameObject go in characterList)
        {
            go.SetActive(false);
        }


        if (characterList[index])
        {
            characterList[index].SetActive(true);
        }
    }

    public void ToggleLeft()
    {
        characterList[index].SetActive(false);

        index--;
        if (index < 0)
        {
            index = characterList.Length - 1;
        }

        characterList[index].SetActive(true);

    }

    public void ToggleRight()
    {
        characterList[index].SetActive(false);

        index++;
        if (index == characterList.Length)
        {
            index = 0;
        }

        characterList[index].SetActive(true);

    }

    public void Play()
    {
        PlayerPrefs.SetInt("CharacterSelected", index);
        //GlobalResponseData.gender = index;
        SceneManager.LoadScene("MainScene");
    }
}
