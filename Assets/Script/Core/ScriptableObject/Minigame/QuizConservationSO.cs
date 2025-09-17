using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Interaction.Minigame {
    [CreateAssetMenu(menuName = "Scriptable Objects/Minigame/Quiz Conservation Data")]
    public class QuizConservationSO : MinigameDataSO
    {
        public int numberOfQuestion;
        public int numberOfMaxWrong;
        public List<QuestionAndAnswer> qnas;
        public DialogConservation quizIntro;

        public override void Init(GameObject targetGameObject)
        {
            QuizManager.instance.InitAndStartQuizData(targetGameObject, this);
        }
    }

    [System.Serializable]
    public class QuestionAndAnswer
    {
        [TextArea(2,4)]
        public string Question;
        public string[] Answers;
        public int CorrectAnswer;
        [TextArea(2, 4)]
        public string Explaination;

        [Header("Image")]
        public Sprite QuestionImage;
    }
}

