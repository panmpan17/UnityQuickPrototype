using UnityEngine;


[CreateAssetMenu(fileName = "RigibodyAttractParameter", menuName = "Scriptable Objects/Rigibody Attract Parameter")]
public class RigibodyAttractParameter : ScriptableObject
{
    public float ShakeTimer;
    public AnimationCurve ShakeCurve;
    public RangeStruct ShakeSpeedRange;
    public RangeStruct ShakeAmplitudeRange;

    public float TimerProgress(float runTime)
    {
        return runTime / ShakeTimer;
    }

    public void GetShakeValues(float progress, out float speed, out float amplitude)
    {
        speed = ShakeSpeedRange.Lerp(ShakeCurve.Evaluate(progress));
        amplitude = ShakeAmplitudeRange.Lerp(ShakeCurve.Evaluate(progress));
    }
}
