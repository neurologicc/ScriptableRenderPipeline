using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [Serializable]
    public class RenderSetup
    {
        [SerializeField]
        public List<RenderPassInfo> m_RenderPassList;   // This contains the data to create instances of render passes

        // Runtime Only Members
        public List<IRenderPass> m_RenderPassInstances; // This is basically the list above but an instanced version.

        public static RenderSetup CreateRenderSetup(Type[] passes)
        {
            RenderSetup retRenderSetup = new RenderSetup();
            retRenderSetup.m_RenderPassList = new List<RenderPassInfo>();

            foreach (var pass in passes)
            {
                RenderPassInfo info = new RenderPassInfo();
                RenderPassReflectionUtilities.GetClassAndAssemblyFromType(pass, out info.className, out info.assemblyName);
                retRenderSetup.m_RenderPassList.Add(info);
            }

            return retRenderSetup;
        }
    }
}