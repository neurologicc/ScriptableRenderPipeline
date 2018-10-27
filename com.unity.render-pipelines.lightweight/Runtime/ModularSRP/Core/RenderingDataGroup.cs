using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RenderingDataGroup : System.Attribute
    {
        public RenderingDataGroup(string group)
        {
            Group = group;
        }

        public string Group;

    }
}
