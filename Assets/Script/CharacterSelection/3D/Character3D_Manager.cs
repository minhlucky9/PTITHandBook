using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalResponseData_Login;

public class Character3D_Manager_Ingame : MonoBehaviour
{
    private GameObject[] characterList;
    public int index;
    public GameObject character1;
    public GameObject character2;

    public GameObject Logo1;
    public GameObject Logo2;

    private void Update()
    {
        if(index == 0)
        {
            transform.position = character1.transform.position;
            Logo1.SetActive(true);
        }
        else
        {
            transform.position = character2.transform.position;
            Logo2.SetActive(true);
        }
    }
    private void Start()
    {
      
        index = PlayerPrefs.GetInt("CharacterSelected");
      

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
        SceneManager.LoadScene("SampleScene");
    }
}
