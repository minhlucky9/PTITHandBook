using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Character_database : ScriptableObject
{
    public Character_atribute[] character;

    public int CharacterCount
    {
        get
        {
            return character.Length;
        }
    }

    public Character_atribute GetCharacter(int index)
    {
        return character[index];
    }
}
