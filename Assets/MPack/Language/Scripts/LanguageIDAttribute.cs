using System;
using UnityEngine;

namespace MPack
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public class LanguageIDAttribute : PropertyAttribute
    {
        public bool IsTextArea;
        public bool CanCollapse;

        public LanguageIDAttribute(bool isTextArea=false, bool canCollapse=false)
        {
            IsTextArea = isTextArea;
            CanCollapse = canCollapse;
        }
    }
}