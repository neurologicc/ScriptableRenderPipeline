using System;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Set up camera properties for the current pass.
    ///
    /// This pass is used to configure shader uniforms and other unity properties that are required for rendering.
    /// * Setup Camera RenderTarget and Viewport
    /// * VR Camera Setup and SINGLE_PASS_STEREO props
    /// * Setup camera view, projection and their inverse matrices.
    /// * Setup properties: _WorldSpaceCameraPos, _ProjectionParams, _ScreenParams, _ZBufferParams, unity_OrthoParams
    /// * Setup camera world clip planes properties
    /// * Setup HDR keyword
    /// * Setup global time properties (_Time, _SinTime, _CosTime)
    /// </summary>
    /// 
    [RenderPassGroup("LWRP")]
    public class SetupForwardRenderingPass : ScriptableRenderPass
    {
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            context.SetupCameraProperties(m_RenderingData.Value.cameraData.camera, m_RenderingData.Value.cameraData.isStereoEnabled);
        }
    }
}
