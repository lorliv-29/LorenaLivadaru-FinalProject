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
        if (string.IsNullOrEmpty(all)) return new List<string>();

        string[] entries = all.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<(string name, int time)> sortedList = new List<(string, int)>();

        foreach (var entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int t))
                sortedList.Add((parts[0], t));
        }

        // Sort: Smallest time (fastest) at the top
        sortedList.Sort((a, b) => a.time.CompareTo(b.time));

        List<string> formattedScores = new List<string>();
        for (int i = 0; i < Mathf.Min(maxEntries, sortedList.Count); i++)
        {
            // Convert the raw seconds (e.g. 75) into MM:SS (e.g. 01:15)
            int m = sortedList[i].time / 60;
            int s = sortedList[i].time % 60;
            string timeStr = string.Format("{0:00}:{1:00}", m, s);

            formattedScores.Add($"{sortedList[i].name} — {timeStr}");
        }
        return formattedScores;
    }
}