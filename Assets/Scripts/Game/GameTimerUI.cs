using UnityEngine;
using TMPro;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void Awake()
    {
    }

    private void Update()
    {
        if (GameController.Instance == null || GameController.Instance.Runner == null)
        {
            timerText.text = "00:00";
            return;
        }

        var timer = GameController.Instance.GameTimer;
        float secondsLeft = 0f;

        if (timer.IsRunning)
        {
            secondsLeft = timer.RemainingTime(GameController.Instance.Runner) ?? 0f;
            if (secondsLeft < 0) secondsLeft = 0;
        }

        int minutes = Mathf.FloorToInt(secondsLeft / 60f);
        int seconds = Mathf.FloorToInt(secondsLeft % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}