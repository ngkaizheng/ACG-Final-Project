using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Game Event (Generic)")]
public class GameEvent<T> : ScriptableObject
{
    public UnityEvent<T> OnRaised = new UnityEvent<T>();

    public void Raise(T value)
    {
        OnRaised.Invoke(value);
    }
}