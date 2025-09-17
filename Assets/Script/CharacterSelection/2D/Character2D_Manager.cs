using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character_Manager : MonoBehaviour
{
    public Character_database characterDB;
    public Text nameText;
    public Image artworkSprite;
    public Character3D_Manager manager;
    public int selectedOption = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdatedCharacter(selectedOption);
    }

    public void NextOption()
    {
        selectedOption++;

        if (selectedOption >= characterDB.CharacterCount)
        {
            selectedOption = 0;
        }
        UpdatedCharacter(selectedOption);
    }

    public void BackOption()
    {
        selectedOption--;

        if(selectedOption < 0)
        {
            selectedOption = characterDB.CharacterCount - 1;
        }
        UpdatedCharacter(selectedOption);
    }
    private void UpdatedCharacter(int selectedOption)
    {
        Character_atribute character = characterDB.GetCharacter(selectedOption);

        artworkSprite.sprite = character.characterSprite;

        nameText.text = character.characterName;
    }
}
