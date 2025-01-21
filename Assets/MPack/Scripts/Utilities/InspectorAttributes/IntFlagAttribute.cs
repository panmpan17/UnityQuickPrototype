using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace MPack
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public class IntFlagAttribute : PropertyAttribute
    {
    }
}