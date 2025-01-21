using UnityEngine;


namespace MPack
{
    [ExecuteInEditMode]
    public class ButtonPaletteSwitcher : MonoBehaviour
    {
        public ButtonPaletteReference paletteReference;

        void Awake()
        {
            ApplyColor();
        }

        #if UNITY_EDITOR
        void Update()
        {
            ApplyColor();
        }
        #endif

        void ApplyColor()
        {
            if (!paletteReference)
                return;

            var selectable = GetComponent<UnityEngine.UI.Selectable>();
            if (!selectable)
                return;

            selectable.transition = paletteReference.Transition;
            selectable.colors = paletteReference.Colors;
            selectable.spriteState = paletteReference.SpriteState;
            selectable.animationTriggers = paletteReference.AnimationTriggers;
        }
    }
}
