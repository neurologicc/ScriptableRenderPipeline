using System;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class RenderPassOutput : System.Attribute
    {
        string m_Name;

        public RenderPassOutput(string input)
        {
            m_Name = input;
        }

        public string Name { get { return m_Name; } }

    }
}