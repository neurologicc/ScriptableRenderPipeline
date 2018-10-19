using System;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Generate rendering attachments that can be used for rendering.
    ///
    /// You can use this pass to generate valid rendering targets that
    /// the Lightweight Render Pipeline can use for rendering. For example,
    /// when you render a frame, the LWRP renders into a valid color and
    /// depth buffer.
    /// </summary>
    /// 
    [RenderPassGroup("LWRP")]
    public class CreateLightweightRenderTexturesPass : ScriptableRenderPass
    {
        [RenderPassInput("DepthAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_DepthAttachmentHandle;
        [RenderPassInput("ColorAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_ColorAttachmentHandle;
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;
        [RenderPassInput("BaseRTDescriptor")]
        RenderPassReference<RenderTextureDescriptor> m_BaseRTDescriptor;

        const string k_CreateRenderTexturesTag = "Create Render Textures";
        const int k_DepthStencilBufferBits = 32;

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            int samples = m_RenderingData.Value.cameraData.msaaSamples;

            CommandBuffer cmd = CommandBufferPool.Get(k_CreateRenderTexturesTag);
            if (m_ColorAttachmentHandle.Value != RenderTargetHandle.CameraTarget)
            {
                var colorDescriptor = m_BaseRTDescriptor.Value;
                colorDescriptor.depthBufferBits = 0;
                colorDescriptor.sRGB = true;
                colorDescriptor.msaaSamples = samples;
                cmd.GetTemporaryRT(m_ColorAttachmentHandle.Value.id, colorDescriptor, FilterMode.Bilinear);
            }

            if (m_DepthAttachmentHandle.Value != RenderTargetHandle.CameraTarget)
            {
                var depthDescriptor = m_BaseRTDescriptor.Value;
                depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                depthDescriptor.msaaSamples = (int)samples;
                depthDescriptor.bindMS = (int)samples > 1 && !SystemInfo.supportsMultisampleAutoResolve;
                cmd.GetTemporaryRT(m_DepthAttachmentHandle.Value.id, depthDescriptor, FilterMode.Point);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            
            if (m_ColorAttachmentHandle.Value != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ColorAttachmentHandle.Value.id);
                m_ColorAttachmentHandle.Value = RenderTargetHandle.CameraTarget;
            }

            if (m_DepthAttachmentHandle.Value != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_DepthAttachmentHandle.Value.id);
                m_DepthAttachmentHandle.Value = RenderTargetHandle.CameraTarget;
            }
        }
    }
}
