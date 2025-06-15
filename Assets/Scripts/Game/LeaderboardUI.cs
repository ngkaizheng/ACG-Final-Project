using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text leaderboardText;

    public static LeaderboardUI Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateLeaderboard();
    }

    public void UpdateLeaderboard()
    {
        if (leaderboardText == null) return;
        var manager = InGamePlayerManager.Instance;
        if (manager == null) return;

        // Gather all player data
        List<InGamePlayerData> players = new List<InGamePlayerData>();
        foreach (var kv in manager.playerDataDict)
        {
            if (kv.Value != null)
                players.Add(kv.Value);
        }

        // Sort by kills descending, then deaths ascending
        players.Sort((a, b) =>
        {
            int cmp = b.Kills.CompareTo(a.Kills);
            if (cmp == 0) cmp = a.Deaths.CompareTo(b.Deaths);
            return cmp;
        });

        // // Build leaderboard string
        // StringBuilder sb = new StringBuilder();
        // sb.AppendLine("<b>Leaderboard</b>");
        // sb.AppendLine($"{"Name",-12}{"Kills",8}{"Deaths",8}");
        // foreach (var p in players)
        // {
        //     sb.AppendLine($"{p.GetNickname(),-12}{p.Kills,8}{p.Deaths,8}");
        // }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>LEADERBOARD</b>");

        var mspace = "0.8";

        // Column headers with fixed-width spacing
        sb.AppendLine($"<mspace={mspace}em>{"NAME".PadRight(12)}</mspace>" +
                     $"<mspace={mspace}em>{"KILLS".PadLeft(6)}</mspace>" +
                     $"<mspace={mspace}em>{"DEATHS".PadLeft(8)}</mspace>");

        // Player entries
        // Player entries
        foreach (var p in players)
        {
            string name = p.GetNickname();
            if (name.Length > 12)
                name = name.Substring(0, 9) + "...";
            name = name.PadRight(12);

            sb.AppendLine($"<mspace={mspace}em>{name.PadRight(12)}</mspace>" +
                         $"<mspace={mspace}em>{p.Kills.ToString().PadLeft(6)}</mspace>" +
                         $"<mspace={mspace}em>{p.Deaths.ToString().PadLeft(8)}</mspace>");
        }

        leaderboardText.text = sb.ToString();
    }
}