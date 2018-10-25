using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    public class RenderSetupManager
    {
        public static Dictionary<string, object> m_OutputObjectData = new Dictionary<string, object>();


        public static void ClearOutputs()
        {
            m_OutputObjectData.Clear();
        }

        public static object CreateOutput(string identifier, object value)
        {
            if (!m_OutputObjectData.ContainsKey(identifier))
            {
                m_OutputObjectData.Add(identifier, value);
                return m_OutputObjectData[identifier];
            }

            return null;
        }

        public static object GetValue(string identifier)
        {
            if (m_OutputObjectData.ContainsKey(identifier))
            {
                return m_OutputObjectData[identifier];
            }

            return null;
        }


        public static void Initialize(RenderSetup setup)
        {
            setup.m_RenderPassInstances = new List<IRenderPass>();
            Dictionary<string, object> outputObjectData = new Dictionary<string, object>(m_OutputObjectData);

            if (setup.m_RenderPassList != null)
            {
                for (int i = 0; i < setup.m_RenderPassList.Count; i++)
                {
                    //RenderPassInfo passInfo = setup.m_RenderPassList[i];
                    //Type classType;

                    //RenderPassReflectionUtilities.GetTypeFromClassAndAssembly(passInfo.className, passInfo.assemblyName, out classType);

                    // Create an instance of our render pass. Render pass should have no arguments
                    //IRenderPass pass = (IRenderPass)Activator.CreateInstance(classType);
                    IRenderPass pass = setup.m_RenderPassList[i].passObject;

                    if (pass == null)
                        continue;

                    setup.m_RenderPassInstances.Add(pass);
                    Type passType = pass.GetType();

                    // Create objects for all of our IRenderPassReferences. This is a little bit in efficient
                    FieldInfo[] fieldInfo = pass.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo info in fieldInfo)
                    {
                        Type fieldType = info.FieldType;
                        if (typeof(IRenderPassReference).IsAssignableFrom(fieldType))
                        {
                            IRenderPassReference renderPassReference = (IRenderPassReference)Activator.CreateInstance(fieldType);
                            info.SetValue(pass, renderPassReference);
                        }
                    }

                    foreach (FieldInfo info in fieldInfo)
                    {
                        RenderPassInput input = info.GetCustomAttribute<RenderPassInput>();

                        if (input != null)
                        {
                            // If we dont have an output defined
                            if (!outputObjectData.ContainsKey(input.Name))
                            {
                                // First check the default value
                                if (input.HasDefaultValue)
                                {
                                    Type fieldType = info.FieldType;
                                    if (typeof(IRenderPassReference).IsAssignableFrom(fieldType))
                                    {
                                        IRenderPassReference renderPassReference = (IRenderPassReference)info.GetValue(pass);
                                        renderPassReference.SetValue(input.DefaultValue);
                                        info.SetValue(pass, renderPassReference);
                                    }
                                    else
                                        info.SetValue(pass, input.DefaultValue);
                                }
                                else
                                    Debug.LogError("Undefined input for " + info.Name + "in renderPass " + passType.FullName + ". Missing matching RenderPassOutput");
                            }
                            else
                            {
                                // Since we have an output field defined check to make sure our types match
                                if (outputObjectData[input.Name].GetType() == info.FieldType)
                                {
                                    info.SetValue(pass, outputObjectData[input.Name]);
                                }
                                else
                                    Debug.LogError("Render pass " + passType.ToString() + " has a type mismatch for output " + input.Name);
                            }
                        }
                    }

                    pass.Initialize(); // This needs to initialize any data we want to share in our outputs

                    foreach (FieldInfo info in fieldInfo)
                    {
                        RenderPassOutput output = (RenderPassOutput)info.GetCustomAttribute(typeof(RenderPassOutput), false);

                        if (output != null)
                        {
                            if (!outputObjectData.ContainsKey(output.Name))
                            {
                                outputObjectData.Add(output.Name, info.GetValue(pass));
                            }
                            else
                            {
                                outputObjectData[output.Name] = info.GetValue(pass);
                            }
                        }
                    }
                }
            }
        }

        public static void Execute(RenderSetup setup, ScriptableRenderContext context)
        {
            if (setup != null && setup.m_RenderPassInstances != null)
            {
                for (int i = 0; i < setup.m_RenderPassInstances.Count; i++)
                    setup.m_RenderPassInstances[i].Execute(context);
            }
        }
    }
}
