
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerDataManager;

public class GlobalResponseData_Login : MonoBehaviour
{
    public static class GlobalResponseData
    {
        public static int code;
        public static string desc;
        public static int user_id;
        public static string student_id;
        public static string fullname;
        public static string session_id;
        public static int role;
        public static float HealthSlider;
        public static string CharacterName; 

        public static float x;
        public static float y;
        public static float z;
        public static int FirstTimeQuest = 0;
        public static int Medal;
        public static int bonus_medal;
        public static int gold = 5;
        public static int level = 1;
        public static int gender = 1;

    //    public static List<SerializableQuest> savedQuests; // Add this to store the loaded quest data

        public static Dictionary<string, Quest> quests;



        public static List<InventoryItem> inventoryItems;
    }
}
