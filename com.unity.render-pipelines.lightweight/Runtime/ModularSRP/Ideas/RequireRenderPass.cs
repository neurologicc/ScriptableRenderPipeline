using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RequireRenderPass : System.Attribute
{
    public RequireRenderPass(string passName)
    {
        requiredPass = passName;
    }

    public string requiredPass;
}
