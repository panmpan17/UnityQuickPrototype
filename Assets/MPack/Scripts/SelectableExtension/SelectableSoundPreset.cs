using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName="MPack/Selectable Sound Preset")]
public class SelectableSoundPreset : ScriptableObject
{
    public AudioClip OnPointerEnter;
    [Range(0, 1)]
    public float OnPointerEnterVolume = 1;
    public AudioClip OnPointerExit;
    [Range(0, 1)]
    public float OnPointerExitVolume = 1;

    [Space(8)]
    public AudioClip OnSelect;
    [Range(0, 1)]
    public float OnSelectVolume = 1;
    public AudioClip OnDeselect;
    [Range(0, 1)]
    public float OnDeselectVolume = 1;

    [Space(8)]
    public AudioClip OnSubmit;
    [Range(0, 1)]
    public float OnSubmitVolume = 1;
}
