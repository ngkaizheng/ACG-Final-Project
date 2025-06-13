// GameEventListenerT.cs
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener<T> : MonoBehaviour
{
    [SerializeField] GameEvent<T> _event;
    [SerializeField] UnityEvent<T> _response;

    private void OnEnable()
    {
        _event.OnRaised.AddListener(TriggerResponse);
    }

    private void OnDisable()
    {
        _event.OnRaised.RemoveListener(TriggerResponse);
    }

    public void TriggerResponse(T value)
    {
        _response.Invoke(value);
    }
}