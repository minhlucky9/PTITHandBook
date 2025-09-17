using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    REQUIREMENTS_NOT_MET,
    CAN_START,
    IN_PROGRESS,
    FINISHED
}

public class Quest
{
    // static info
    public QuestInfoSO info;

    // state info
    public QuestState state;
    public int currentQuestStepIndex;
    public QuestStepState[] questStepStates;

    public Action OnQuestFinish;

    public event Action OnStateChanged;

    public bool isCurrentStepExists() => (currentQuestStepIndex < info.questSteps.Count);

    public Quest(QuestInfoSO questInfo)
    {
        info = questInfo;
        state = QuestState.REQUIREMENTS_NOT_MET;
        currentQuestStepIndex = 0;
        questStepStates = new QuestStepState[info.questSteps.Count];
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.info = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;

        // if the quest step states and prefabs are different lengths,
        // something has changed during development and the saved data is out of sync.
        if (this.questStepStates.Length != this.info.questSteps.Count)
        {
            Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
                + "of different lengths. This indicates something changed "
                + "with the QuestInfo and the saved data is now out of sync. "
                + "Reset your data - as this might cause issues. QuestId: " + this.info.id);
        }
    }

    public bool TryNextStep()
    {
        currentQuestStepIndex++;
        //check if there is step behind
        if (isCurrentStepExists())
        {
            ChangeQuestState(QuestState.IN_PROGRESS);
            return true;
        } 
        else
        {
            ChangeQuestState(QuestState.FINISHED);
            OnQuestFinish?.Invoke();            
            return false;
        }
    }

    public void ChangeQuestState(QuestState state)
    {
        this.state = state;
        OnStateChanged?.Invoke();      
    }

}

[System.Serializable]
public class QuestData
{
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;

    public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates)
    {
        this.state = state;
        this.questStepIndex = questStepIndex;
        this.questStepStates = questStepStates;
    }
}

[System.Serializable]
public class QuestStepState
{
    public string state;
    public string status;


    public QuestStepState(string state, string status)
    {
        this.state = state;
        this.status = status;
    }

    public QuestStepState()
    {
        this.state = "";
        this.status = "";
    }
}
