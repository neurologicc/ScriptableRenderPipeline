using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    public class RenderPassReflectionUtilities
    {
        public static RenderPassInfo[] QueryRenderPasses()
        {
            List<RenderPassInfo> allRenderPassInfo = new List<RenderPassInfo>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(x => typeof(IRenderPass).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                foreach (Type type in types)
                {
                    RenderPassInfo info = new RenderPassInfo();
                    GetClassAndAssemblyFromType(type, out info.className, out info.assemblyName);
                    allRenderPassInfo.Add(info);
                }
            }

            return allRenderPassInfo.ToArray();
        }

        public static void GetClassAndAssemblyFromType(Type type, out string className, out string assemblyName)
        {
            Assembly asm = type.Assembly;
            assemblyName = asm.FullName;
            className = type.FullName;
        }

        public static void GetTypeFromClassAndAssembly(string className, string assemblyName, out Type type)
        {
            Assembly asm = Assembly.Load(assemblyName);
            type = asm.GetType(className);
        }


    }
}