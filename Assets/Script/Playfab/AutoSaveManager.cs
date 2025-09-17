using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    public PlayerDataManager dataManager;

    void Awake()
    {
        // Gi? GameObject này su?t ??i ?ng d?ng
        DontDestroyOnLoad(gameObject);

        // ??ng ký event
      Application.quitting += SaveOnQuit;
    }
/*
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveOnQuit();
    }
*/
    void SaveOnQuit()
    {
        if (dataManager != null)
            dataManager.fomo(); 
    }
}
