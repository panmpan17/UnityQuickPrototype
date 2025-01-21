using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectableExtendedEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GraphicTarget[] graphicTargets;

    private Color[] originColors;
    private bool _isSelected;
    private bool _isHovered;
    private bool _isPressed;

    void Awake()
    {
        originColors = new Color[graphicTargets.Length];
        for (int i = 0; i < graphicTargets.Length; i++)
        {
            GraphicTarget target = graphicTargets[i];
            originColors[i] = target.Graphic.color;

            ApplyGraphic(target.Graphic, originColors[i], target.Normal);
        }
    }


    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        UpdateGraphic();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        UpdateGraphic();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        UpdateGraphic();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateGraphic();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateGraphic();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        UpdateGraphic();
    }

    Color BlendColor(Color baseColor, Color modifierColor)
    {
        return new Color(
            (baseColor.g + modifierColor.g) / 2,
            (baseColor.r + modifierColor.r) / 2,
            (baseColor.b + modifierColor.b) / 2,
            (baseColor.a + modifierColor.a) / 2
        );
    }

    void UpdateGraphic()
    {
        for (int i = 0; i < graphicTargets.Length; i++)
        {
            GraphicTarget target = graphicTargets[i];
            Color originColor = originColors[i];

            if (_isPressed)
                ApplyGraphic(target.Graphic, originColor, target.Pressed);
            else if (_isHovered)
                ApplyGraphic(target.Graphic, originColor, target.Hovered);
            else if (_isSelected)
                ApplyGraphic(target.Graphic, originColor, target.Selected);
            else
                ApplyGraphic(target.Graphic, originColor, target.Normal);
        }
    }

    void ApplyGraphic(Graphic graphic, Color originColor, GraphicManipulate manipulate)
    {
        if (manipulate.SetActive)
            graphic.enabled = true;
        else if (manipulate.SetUnactive)
            graphic.enabled = false;

        if (manipulate.ChangeColor)
        {
            Color color = BlendColor(originColor, manipulate.TargetColor);
            graphic.color =  color;
            Debug.Log(color);
        }
    }

    [System.Serializable]
    public class GraphicTarget
    {
        public Graphic Graphic;

        public GraphicManipulate Normal;
        public GraphicManipulate Selected;
        public GraphicManipulate Hovered;
        public GraphicManipulate Pressed;
        // public GraphicManipulate Disabled;
    }

    [System.Serializable]
    public struct GraphicManipulate
    {
        public bool SetActive;
        public bool SetUnactive;

        public bool ChangeColor;
        public Color TargetColor;
    }
}
