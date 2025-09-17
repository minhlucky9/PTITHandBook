using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterInformation
{
    public string name;
    public float health;
    public float stamina;
    public float posX;
    public float posY;
    public float posZ;


    public CharacterInformation(string name, float health, float stamina, float posX, float posY, float posZ)
    {
        this.name = name;
        this.health = health;
        this.stamina = stamina;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
    }
}
public class CharacterSave : MonoBehaviour
{
    public TextMeshProUGUI nameInput;
    public Slider HealthSlider;
    public Slider StaminaSlider;
    public GameObject player;


    public CharacterInformation ReturnClass()
    {
        Vector3 position = player.transform.position;
        return new CharacterInformation(nameInput.text, HealthSlider.value, StaminaSlider.value, position.x, position.y, position.z); 
    }

    public void SetUI(CharacterInformation characterInformation)
    {
        nameInput.text = characterInformation.name;
        HealthSlider.value = characterInformation.health;
        StaminaSlider .value = characterInformation.stamina;
        Vector3 newPosition = new Vector3(characterInformation.posX, characterInformation.posY, characterInformation.posZ);
        player.transform.position = newPosition;
    }
}
