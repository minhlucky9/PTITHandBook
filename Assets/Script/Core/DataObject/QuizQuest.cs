using Interaction;
using UnityEngine;

public class QuizQuest : Quest
{
    // Các bi?n l?u ti?n trình quiz
    public int currentQuestion;
    public int correctAnswers;
    public int wrongAnswers;

    // Constructor cho t?o quest m?i t? QuestInfoSO
    public QuizQuest(QuestInfoSO questInfo) : base(questInfo)
    {
        currentQuestion = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
    }

    // Constructor khi load d? li?u ?ã l?u (deserialized)
    public QuizQuest(QuestInfoSO questInfo, QuestState state, int currentQuestStepIndex, QuestStepState[] questStepStates, int currentQuestion, int correctAnswers, int wrongAnswers)
        : base(questInfo, state, currentQuestStepIndex, questStepStates)
    {
        this.currentQuestion = currentQuestion;
        this.correctAnswers = correctAnswers;
        this.wrongAnswers = wrongAnswers;
    }

    // Ph??ng th?c h? tr? serialization (n?u mu?n l?u thêm d? li?u quiz)
    public string SerializeData()
    {
        QuizQuestData data = new QuizQuestData();
        data.state = this.state;
        data.currentQuestStepIndex = this.currentQuestStepIndex;
        data.questStepStates = this.questStepStates;
        data.currentQuestion = this.currentQuestion;
        data.correctAnswers = this.correctAnswers;
        data.wrongAnswers = this.wrongAnswers;
        return JsonUtility.ToJson(data);
    }

    // Ph??ng th?c h? tr? kh?i t?o t? d? li?u ?ã l?u
    public static QuizQuest Deserialize(QuestInfoSO questInfo, string serializedData)
    {
        QuizQuestData data = JsonUtility.FromJson<QuizQuestData>(serializedData);
        return new QuizQuest(questInfo, data.state, data.currentQuestStepIndex, data.questStepStates, data.currentQuestion, data.correctAnswers, data.wrongAnswers);
    }
}

// L?p d? li?u cho QuizQuest (dùng cho serialization/deserialization)
[System.Serializable]
public class QuizQuestData
{
    public QuestState state;
    public int currentQuestStepIndex;
    public QuestStepState[] questStepStates;
    public int currentQuestion;
    public int correctAnswers;
    public int wrongAnswers;
}
