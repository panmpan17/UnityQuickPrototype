using UnityEngine;

public abstract class AbstractWeaponAddon : MonoBehaviour
{
    public abstract void ApplyPowerUp(AbstractPowerUpAddon[] powerUps);
}
