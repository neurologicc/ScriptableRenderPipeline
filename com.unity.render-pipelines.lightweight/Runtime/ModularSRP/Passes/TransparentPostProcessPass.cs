using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Perform final frame post-processing using the given color attachment
    /// as the source and the current camera target as the destination.
    ///
    /// You can use this pass to apply post-processing to the given color
    /// buffer. The pass uses the currently configured post-process stack,
    /// and it copies the result to the Camera target.
    /// </summary>
    /// 
    [RenderPassGroup("LWRP")]
    public class TransparentPostProcessPass : LWRPRenderPass
    {
        [RenderPassInput("ColorAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_ColorAttachmentHandle;
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;
        [RenderPassInput("BaseRTDescriptor")]
        RenderPassReference<RenderTextureDescriptor> m_BaseRTDescriptor;
        [RenderPassInput("PostProcessRenderContext")]
        RenderPassReference<PostProcessRenderContext> m_PostProcessingRenderContext;

        const string k_PostProcessingTag = "Render PostProcess Effects";

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_PostProcessingTag);
            RenderPostProcess(m_PostProcessingRenderContext.Value, cmd, ref m_RenderingData.Value.cameraData, m_BaseRTDescriptor.Value.colorFormat, m_ColorAttachmentHandle.Value.Identifier(), BuiltinRenderTextureType.CameraTarget, false);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
