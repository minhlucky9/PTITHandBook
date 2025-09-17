using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerDataLoader;

public class GameData : MonoBehaviour
{
    public static GameData instance { get; private set; }

    public PlayerState playerState;
    public Dictionary<string, Quest> questMap;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
