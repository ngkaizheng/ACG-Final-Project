using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] GameEvent _event;
    [SerializeField] UnityEvent _response;

    private void OnEnable()
    {
        _event.OnRaised.AddListener(TriggerResponse);
    }

    private void OnDisable()
    {
        _event.OnRaised.RemoveListener(TriggerResponse);
    }

    public void TriggerResponse()
    {
        _response.Invoke();
    }
}