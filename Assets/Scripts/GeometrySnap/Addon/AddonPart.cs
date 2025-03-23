using UnityEngine;


[CreateAssetMenu(fileName = "AddonPart", menuName = "Scriptable Objects/Addon Part")]
public class AddonPart : ScriptableObject
{
    public GameObject Prefab;
    public float OverrideColor;
}
