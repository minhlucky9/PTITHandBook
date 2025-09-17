using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interaction.Minigame
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Change Scene Event Data")]
    public class ChangeSceneEvent : MinigameDataSO
    {
        public string targetSceneName;
        Vector3 playerPosition;
        public override void Init(GameObject targetGameObject)
        {
            base.Init(targetGameObject);
            //
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Additive);
            //
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name.Equals(targetSceneName))
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                //
                SubSceneGameManager.instance.InitGame(targetNPC, this, targetSceneName);
            }
        }

        public void OnSceneUnload(Scene scene)
        {
            if (scene.name.Equals(targetSceneName))
            {
                SceneManager.sceneUnloaded -= OnSceneUnload;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().position = playerPosition;
            }
        }
    }
}
