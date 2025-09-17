using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubSceneGameManager : MonoBehaviour
{
    public static SubSceneGameManager instance;

    public string sceneName;
    public Transform spawnPlayerTransform;
    public Transform spawnCameraPivotTransform;
    [HideInInspector] public GameObject player;

    private void Awake()
    {
        instance = this;
        //
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<Rigidbody>().position = spawnPlayerTransform.position;
        //
        FindObjectOfType<CameraHandle>().cameraPivotTransform.rotation = spawnCameraPivotTransform.rotation;
        //
        player.GetComponent<PlayerManager>().ActivateController();
    }

    public virtual void InitGame(GameObject targetNPC, MinigameDataSO minigameData, string sceneName)
    {
        this.sceneName = sceneName;
    }
}
