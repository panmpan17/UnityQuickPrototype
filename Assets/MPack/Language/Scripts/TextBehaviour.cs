using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace MPack
{
    public class TextBehaviour : MonoBehaviour
    {
        protected TextMeshPro textMeshPro;
        protected TextMeshProUGUI textMeshProUI;
        protected bool initialed = false;

        public Color Color {
            get => textMeshPro != null ? textMeshPro.color : textMeshProUI != null ? textMeshProUI.color : Color.white;
            set {
                if (!initialed) InitialGetTextComponent();

                if (textMeshPro != null) textMeshPro.color = value;
                else if (textMeshProUI != null) textMeshProUI.color = value;
                else Debug.LogError("missing", gameObject);
            }
        }

        public float Alpha {
            get => textMeshPro != null ? textMeshPro.alpha : textMeshProUI != null ? textMeshProUI.alpha : 1;
            set {
                if (!initialed) InitialGetTextComponent();

                if (textMeshPro != null) textMeshPro.alpha = value;
                else if (textMeshProUI != null) textMeshProUI.alpha = value;
                else Debug.LogError("missing", gameObject);
            }
        }

        public string Text {
            get => textMeshPro != null ? textMeshPro.text : textMeshProUI != null ? textMeshProUI.text : string.Empty;
        }


        protected virtual void Awake()
        {
            if (!initialed) InitialGetTextComponent();
        }

        /// <summary>
        /// Initial the text component
        /// </summary>
        protected void InitialGetTextComponent() {
            initialed = true;
            
            textMeshPro = GetComponent<TextMeshPro>();
            textMeshProUI = GetComponent<TextMeshProUGUI>();

            if (textMeshPro == null && textMeshProUI == null) {
            #if UNTIY_EDITOR
                Debug.LogError("LanguageText require Text or TextMesh or TextMeshPro or TextMeshProUGUI", gameObject);
            #endif
                enabled = false;
                return;
            }
        }


        public virtual void SetText(string newValue)
        {
            if (!initialed) InitialGetTextComponent();

            if (textMeshPro != null)
                textMeshPro.text = newValue;
            else if (textMeshProUI != null)
                textMeshProUI.text = newValue;
    #if UNITY_EDITOR
            else Debug.LogError("missing", gameObject);
    #endif
        }
    }
}