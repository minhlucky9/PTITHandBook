using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Minigame/DragAndDrop Event Data")]
public class CryptogramEventSO : MinigameDataSO
{
    public override void Init(GameObject targetNPC)
    {
        base.Init(targetNPC);
        // Start the drag&drop UI for this quest step
        CryptoGramManager.instance.InitCryptoQuest(targetNPC, this);
    }
}


