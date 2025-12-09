using UnityEngine;

public class MonjeAnimationProxy : MonoBehaviour
{
    private Monje monje;

    private void Awake()
    {
        monje = GetComponentInParent<Monje>();
    }

    public void Teletransport()
    {
        monje?.Teletransport();
    }

    public void TeletransportToFlee()
    {
        monje?.TeletransportToFlee();
    }
    public void OnTeletransportAttackImpact()
    {
        monje?.OnTeletransportAttackImpact();
    }

    public void OnTeletransportAttackImpactEnd()
    {
        monje?.OnTeletransportAttackImpactEnd();
    }

    public void ThrowGas()
    {
        monje?.ThrowGas();
    }
    public void ThrowGasEnd()
    {
        monje?.OnThrowGasEnd();
    }

    public void OnThrowRayShakeCam()
    {
        monje?.OnThrowRayShakeCam();
    }

    public void OnThrowRay()
    {
        monje?.OnThrowRay();
    }

    public void OnThrowRayEnd()
    {
        monje?.OnThrowRayEnd();
    }
}
