using System;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [Serializable]
    public struct RenderPassInfo
    {
        public string assemblyName;
        public string className;
        public string errorMessage;
        public ScriptableRenderPass passObject;
    }
}
