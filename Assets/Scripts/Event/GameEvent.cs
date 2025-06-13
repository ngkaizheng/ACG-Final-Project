using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public UnityEvent OnRaised = new UnityEvent();

    public void Raise()
    {
        OnRaised.Invoke();
    }
}