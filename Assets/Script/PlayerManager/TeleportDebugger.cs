using UnityEngine;
using System.Collections.Generic;

public static class TeleportDebugger
{
    private static List<string> debugLogs = new List<string>();
    private static float startTime;

    public static void StartDebugging()
    {
        debugLogs.Clear();
        startTime = Time.time;
        Debug.Log("========== TELEPORT DEBUG STARTED ==========");
    }

    public static void Log(string message, Object context = null)
    {
        float timeStamp = Time.time - startTime;
        string logMessage = $"[{timeStamp:F3}s] {message}";
        debugLogs.Add(logMessage);
        Debug.Log(logMessage, context);
    }

    public static void LogPosition(string label, Vector3 position, Object context = null)
    {
        Log($"{label}: {position}", context);
    }

    public static void EndDebugging()
    {
        Debug.Log("========== TELEPORT DEBUG SUMMARY ==========");
        foreach (string log in debugLogs)
        {
            Debug.Log(log);
        }
        Debug.Log("========== DEBUG END ==========");
    }
}