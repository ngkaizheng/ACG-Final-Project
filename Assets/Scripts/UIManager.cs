using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameEvent _onWeaponFired;

    private void OnEnable()
    {
        _onWeaponFired.OnRaised.AddListener(UpdateAmmoUI);
    }

    private void OnDisable()
    {
        _onWeaponFired.OnRaised.RemoveListener(UpdateAmmoUI);
    }

    private void UpdateAmmoUI()
    {
        Debug.Log("Ammo UI Updated!");
    }
}