using UnityEngine;
using TMPro;
using System.Collections;

public class KillFeedUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform feedParent; // UI parent with VerticalLayoutGroup
    [SerializeField] private GameObject killFeedMessagePrefab;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float fadeStartDelay = 2f;

    [Header("Events")]
    [SerializeField] private PlayerKillEvent _playerKilledEvent;

    private void Awake()
    {
        _playerKilledEvent.OnRaised.AddListener(OnPlayerKilled);
    }

    private void OnDestroy()
    {
        _playerKilledEvent.OnRaised.RemoveListener(OnPlayerKilled);
    }

    private void OnPlayerKilled(PlayerKillInfo info)
    {
        string killerName = InGamePlayerManager.Instance.GetPlayerData(info.Killer)?.GetNickname();
        string victimName = InGamePlayerManager.Instance.GetPlayerData(info.Victim)?.GetNickname();
        if (killerName == null) killerName = info.Killer.ToString();
        if (victimName == null) victimName = info.Victim.ToString();

        GameObject msgObj = Instantiate(killFeedMessagePrefab, feedParent);
        TMP_Text msgText = msgObj.GetComponentInChildren<TMP_Text>();
        msgText.text = $"{killerName} killed {victimName}";

        StartCoroutine(RemoveAfterDelay(msgObj));
    }

    private IEnumerator RemoveAfterDelay(GameObject obj)
    {
        TMP_Text msgText = obj.GetComponentInChildren<TMP_Text>();
        float timer = 0f;

        // Wait until fade should start
        yield return new WaitForSeconds(fadeStartDelay);

        Color originalColor = msgText.color;

        // Fade out
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            Color c = originalColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            msgText.color = c;
            yield return null;
        }

        Destroy(obj);
    }
}