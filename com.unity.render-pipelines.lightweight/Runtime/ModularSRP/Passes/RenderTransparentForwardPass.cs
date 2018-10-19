using System;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Render all transparent forward objects into the given color and depth target 
    ///
    /// You can use this pass to render objects that have a material and/or shader
    /// with the pass names LightweightForward or SRPDefaultUnlit. The pass only renders
    /// objects in the rendering queue range of Transparent objects.
    /// </summary>
    /// 
    [RenderPassGroup("LWRP")]
    public class RenderTransparentForwardPass : ScriptableRenderPass
    {
        [RenderPassInput("RendererConfiguration")]
        RenderPassReference<RendererConfiguration> m_RendererConfiguration;
        [RenderPassInput("DepthAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_DepthAttachmentHandle;
        [RenderPassInput("ColorAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_ColorAttachmentHandle;
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;
        [RenderPassInput("BaseRTDescriptor")]
        RenderPassReference<RenderTextureDescriptor> m_BaseRTDescriptor;

        const string k_RenderTransparentsTag = "Render Transparents";

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            ClearShaderPassNames();
            RegisterShaderPassName("LightweightForward");
            RegisterShaderPassName("SRPDefaultUnlit");

            FilterRenderersSettings transparentFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.transparent,
            };

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTransparentsTag);
            using (new ProfilingSample(cmd, k_RenderTransparentsTag))
            {
                RenderBufferLoadAction loadOp = RenderBufferLoadAction.Load;
                RenderBufferStoreAction storeOp = RenderBufferStoreAction.Store;
                SetRenderTarget(cmd, m_ColorAttachmentHandle.Value.Identifier(), loadOp, storeOp,
                    m_DepthAttachmentHandle.Value.Identifier(), loadOp, storeOp, ClearFlag.None, Color.black, m_BaseRTDescriptor.Value.dimension);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Camera camera = m_RenderingData.Value.cameraData.camera;
                var drawSettings = CreateDrawRendererSettings(camera, SortFlags.CommonTransparent, m_RendererConfiguration.Value, m_RenderingData.Value.supportsDynamicBatching);
                context.DrawRenderers(m_RenderingData.Value.cullResults.visibleRenderers, ref drawSettings, transparentFilterSettings);

                // Render objects that did not match any shader pass with error shader

                //TODO: FIX THIS
                //renderer.RenderObjectsWithError(context, ref m_RenderingData.Value.cullResults, camera, transparentFilterSettings, SortFlags.None);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
