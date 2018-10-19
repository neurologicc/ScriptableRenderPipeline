using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public class RenderPassInput : System.Attribute
{
    string m_Name;
    object m_DefaultValue;
    bool   m_HasDefaultValue = true;

    public RenderPassInput(string input)
    {
        m_Name = input;
        m_DefaultValue = null;
        m_DefaultValue = false;
    }


    public RenderPassInput(string input, bool defaultValue)
    {
        m_Name = input;
        m_DefaultValue = defaultValue;
    }

    public RenderPassInput(string input, string defaultValue)
    {
        m_Name = input;
        m_DefaultValue = defaultValue;
    }

    public RenderPassInput(string input, int defaultValue)
    {
        m_Name = input;
        m_DefaultValue = defaultValue;
    }

    public string Name { get { return m_Name; } }
    public object DefaultValue { get { return m_DefaultValue; } }
    public bool HasDefaultValue { get { return m_HasDefaultValue; } }

}
