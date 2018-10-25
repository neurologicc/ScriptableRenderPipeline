using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [Serializable]
    public class RenderSetup
    {
        [SerializeField]
        public List<RenderPassInfo> m_RenderPassList;   // This contains the data to create instances of render passes

        [SerializeField]
        public List<string> m_ExternalOutputs;

        // Runtime Only Members
        public List<IRenderPass> m_RenderPassInstances; // This is basically the list above but an instanced version.

        public static RenderSetup CreateRenderSetup(Type[] passes, string[] externalOutputs, LightweightRenderPipelineAsset asset)
        {
            RenderSetup retRenderSetup = new RenderSetup();
            retRenderSetup.m_RenderPassList = new List<RenderPassInfo>();
            retRenderSetup.m_ExternalOutputs = new List<string>();

            foreach (var pass in passes)
            {
                RenderPassInfo info = new RenderPassInfo();
                RenderPassReflectionUtilities.GetClassAndAssemblyFromType(pass, out info.className, out info.assemblyName);

                //Type classType;
                //RenderPassReflectionUtilities.GetTypeFromClassAndAssembly(info.className, info.assemblyName, out classType);
                var passObject = (ScriptableRenderPass)Activator.CreateInstance(pass);
                passObject.name = pass.ToString();
                AssetDatabase.AddObjectToAsset(passObject, asset);
                info.passObject = passObject;

                retRenderSetup.m_RenderPassList.Add(info);
            }

            foreach(var output in externalOutputs)
                retRenderSetup.m_ExternalOutputs.Add(output);

            return retRenderSetup;
        }

        public void CheckForErrors()
        {
            HashSet<string> definedOutputs = new HashSet<string>();


            for (int i = 0; i < m_RenderPassList.Count; i++)
            {
                RenderPassInfo passInfo = m_RenderPassList[i];

                Type classType;
                RenderPassReflectionUtilities.GetTypeFromClassAndAssembly(passInfo.className, passInfo.assemblyName, out classType);


                passInfo.errorMessage = "";
                FieldInfo[] fieldInfo = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo info in fieldInfo)
                {
                    RenderPassInput input = info.GetCustomAttribute<RenderPassInput>();
                    if (input != null && !input.HasDefaultValue && !definedOutputs.Contains(input.Name) && !m_ExternalOutputs.Contains(input.Name))
                        passInfo.errorMessage += "Uninitialized input " + input.Name + "\n";

                    RenderPassOutput output = info.GetCustomAttribute<RenderPassOutput>();
                    if (output != null && !definedOutputs.Contains(output.Name))
                        definedOutputs.Add(output.Name);
                }

                m_RenderPassList[i] = passInfo;
            }
        }
    }
}
