using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    public interface IRenderPass
    {
        void Initialize();   // This needs to initialize all outputs

        /// <summary>
        /// Execute the pass. This is where custom rendering occurs. Specific details are left to the implementation
        /// </summary>
        /// <param name="renderer">The currently executing renderer. Contains configuration for the current execute call.</param>
        /// <param name="context">Use this render context to issue any draw commands during execution</param>
        /// <param name="renderingData">Current rendering state information</param>
        void Execute(ScriptableRenderContext context);
    }

}