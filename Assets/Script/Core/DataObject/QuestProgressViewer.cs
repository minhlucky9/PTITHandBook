using UnityEngine;
using GameManager; // Gi? s? QuestManager và Quest n?m trong namespace GameManager

// N?u b?n ?ã t?o l?p QuizQuest k? th?a t? Quest, hãy ??m b?o namespace c?a nó ???c import
// Ví d?: using YourNamespace; 

public class QuestProgressViewer : MonoBehaviour
{
    // Các thông s? hi?n th?
    public int startX = 10;
    public int startY = 10;
    public int lineHeight = 30;

    void OnGUI()
    {
        // Ki?m tra QuestManager ?ã kh?i t?o ch?a
        if (QuestManager.instance == null)
        {
            GUI.Label(new Rect(startX, startY, 300, lineHeight), "QuestManager ch?a ???c kh?i t?o.");
            return;
        }

        // L?y questMap t? QuestManager
        var questMap = QuestManager.instance.questMap;
        if (questMap == null || questMap.Count == 0)
        {
            GUI.Label(new Rect(startX, startY, 300, lineHeight), "Không có quest nào ???c load.");
            return;
        }

        int currentY = startY;
        GUI.Label(new Rect(startX, currentY, 600, lineHeight), "=== Quest Progress Viewer ===");
        currentY += lineHeight;

        // Duy?t qua t?ng quest trong questMap
        foreach (var kvp in questMap)
        {
            Quest quest = kvp.Value;
            // Hi?n th? thông tin c? b?n: QuestID, tr?ng thái, b??c hi?n t?i và t?ng s? b??c
            string questInfo = string.Format("Quest ID: {0} | State: {1} | Step: {2}/{3}",
                                             quest.info.id, quest.state.ToString(),
                                             quest.currentQuestStepIndex, quest.info.questSteps.Count);

            // N?u quest là QuizQuest (l?p con c?a Quest dùng cho quiz), ép ki?u và hi?n th? thêm thông tin quiz
            QuizQuest quizQuest = quest as QuizQuest;
            if (quizQuest != null)
            {
                questInfo += string.Format(" | Quiz: Q={0} Correct={1} Wrong={2}",
                                             quizQuest.currentQuestion, quizQuest.correctAnswers, quizQuest.wrongAnswers);
            }

            GUI.Label(new Rect(startX, currentY, 600, lineHeight), questInfo);
            currentY += lineHeight;
        }
    }
}
