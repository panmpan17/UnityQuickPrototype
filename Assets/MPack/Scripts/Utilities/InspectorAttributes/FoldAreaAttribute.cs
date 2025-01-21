using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPack
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=false)]
    public class FoldAreaAttribute : PropertyAttribute
    {
        public string Title;

        public FoldAreaAttribute()
        {
            Title = "Fold Area";
        }
        public FoldAreaAttribute(string title)
        {
            Title = title;
        }
    }
}