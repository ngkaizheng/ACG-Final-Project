using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameEvent _onWeaponFired;
    [SerializeField] private GameEvent _onWeaponReloaded;
    [SerializeField] private GameEvent _onWeaponSwitched;
    [SerializeField] private GameEvent<int> _onDamageDealt;

    public void Fire()
    {
        // Logic for firing the weapon
        Debug.Log("Weapon fired!");
        _onWeaponFired.Raise();
    }

    public void Reload()
    {
        // Logic for reloading the weapon
        Debug.Log("Weapon reloaded!");
        _onWeaponReloaded.Raise();
    }

    public void Switch()
    {
        // Logic for switching the weapon
        Debug.Log("Weapon switched!");
        _onWeaponSwitched.Raise();
    }

    public void Fire(int damage)
    {
        // Logic for firing the weapon with damage
        Debug.Log($"Weapon fired with {damage} damage!");
        _onDamageDealt.Raise(damage);
    }
}