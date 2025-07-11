using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : NetworkBehaviour
{
    [SerializeField] private Canvas _healthCanvas;
    [SerializeField] private Image _healthFill;
    [SerializeField] private Vector3 _offset = new Vector3(0, 2f, 0); // Height above player
    [SerializeField] private float _smoothSpeed = 5f;

    private NetworkHealth _playerHealth;
    private float _targetFillAmount;

    public override void Spawned()
    {
        _playerHealth = transform.root.GetComponentInParent<NetworkHealth>();
        _healthCanvas.worldCamera = Camera.main;
    }

    public override void Render()
    {
        if (_playerHealth == null) return;

        // Update health bar fill smoothly
        _targetFillAmount = (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
        _healthFill.fillAmount = Mathf.Lerp(_healthFill.fillAmount, _targetFillAmount, _smoothSpeed * Runner.DeltaTime);

        // Change color based on health percentage
        UpdateHealthColor();

        // Face the health bar toward the main camera
        if (Camera.main != null)
        {
            _healthCanvas.transform.LookAt(Camera.main.transform);
            _healthCanvas.transform.Rotate(0, 180f, 0); // Flip so it's not mirrored
        }

        // Position above player
        _healthCanvas.transform.position = transform.parent.position + _offset;
    }

    private void UpdateHealthColor()
    {
        float healthPercent = _healthFill.fillAmount;

        if (healthPercent > 0.6f)
            _healthFill.color = Color.green;
        else if (healthPercent > 0.3f)
            _healthFill.color = Color.yellow;
        else
            _healthFill.color = Color.red;
    }
}