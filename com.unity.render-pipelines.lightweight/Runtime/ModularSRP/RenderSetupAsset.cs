using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [CreateAssetMenu(fileName = "RenderSetup", menuName = "SRP/Render Setup", order = 1)]
    public class RenderSetupAsset : ScriptableObject
    {
        public RenderSetup RenderSetup;
    }
}