using System;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Draw the skybox into the given color buffer using the given depth buffer for depth testing.
    ///
    /// This pass renders the standard Unity skybox.
    /// </summary>

    [RenderPassGroup("LWRP")]
    public class DrawSkyboxPass : ScriptableRenderPass
    {
        [RenderPassInput("DepthAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_DepthAttachmentHandle;
        [RenderPassInput("ColorAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_ColorAttachmentHandle;
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Draw Skybox (Set RT's)");
            if (m_RenderingData.Value.cameraData.isStereoEnabled && XRGraphicsConfig.eyeTextureDesc.dimension == TextureDimension.Tex2DArray)
            {
                cmd.SetRenderTarget(m_ColorAttachmentHandle.Value.Identifier(), m_DepthAttachmentHandle.Value.Identifier(), 0, CubemapFace.Unknown, -1);
            }
            else
            {
                cmd.SetRenderTarget(m_ColorAttachmentHandle.Value.Identifier(), m_DepthAttachmentHandle.Value.Identifier());
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            context.DrawSkybox(m_RenderingData.Value.cameraData.camera);
        }
    }
}
