using UnityEngine;
using System.Collections.Generic;

public static class ScoreManager
{
    private const string ScoresKey = "Scores";

    // Add a new score entry
    public static void AddScore(string playerName, int score)
    {
        string newEntry = $"{playerName}:{score}";
        string existing = PlayerPrefs.GetString(ScoresKey, "");

        // Fix new score at the top
        string updated = newEntry + "|" + existing;
        PlayerPrefs.SetString(ScoresKey, updated);
        PlayerPrefs.Save();
    }

    // Retrieve scores as a list of strings
    public static List<string> GetScoreStrings(int maxEntries = 10)
    {
        string all = PlayerPrefs.GetString(ScoresKey, "");
        string[] entries = all.Split('|');

        List<string> scoreList = new List<string>();
        for (int i = 0; i < Mathf.Min(maxEntries, entries.Length); i++)
        {
            if (!string.IsNullOrWhiteSpace(entries[i]))
                scoreList.Add(entries[i]);
        }

        return scoreList;
    }
}