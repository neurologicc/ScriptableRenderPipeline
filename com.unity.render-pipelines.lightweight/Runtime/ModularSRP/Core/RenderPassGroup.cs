using System;
using UnityEngine;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RenderPassGroup : System.Attribute
    {
        public RenderPassGroup(string group)
        {
            Group = group;
        }

        public string Group;
    }
}