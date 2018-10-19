using System;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Render all opaque forward objects into the given color and depth target 
    ///
    /// You can use this pass to render objects that have a material and/or shader
    /// with the pass names LightweightForward or SRPDefaultUnlit. The pass only
    /// renders objects in the rendering queue range of Opaque objects.
    /// </summary>
    /// 
    [RenderPassGroup("LWRP")]
    public class RenderOpaqueForwardPass : LWRPRenderPass
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

        const string k_RenderOpaquesTag = "Render Opaques";
        
        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context)
        {
            ClearShaderPassNames();
            RegisterShaderPassName("LightweightForward");
            RegisterShaderPassName("SRPDefaultUnlit");

            Camera camera = m_RenderingData.Value.cameraData.camera;

            ClearFlag clearFlag = GetCameraClearFlag(camera);
            Color clearColor = CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);

            FilterRenderersSettings opaqueFilterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque,
            };

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderOpaquesTag);
            using (new ProfilingSample(cmd, k_RenderOpaquesTag))
            {
                RenderBufferLoadAction loadOp = RenderBufferLoadAction.DontCare;
                RenderBufferStoreAction storeOp = RenderBufferStoreAction.Store;
                SetRenderTarget(cmd, m_ColorAttachmentHandle.Value.Identifier(), loadOp, storeOp,
                    m_DepthAttachmentHandle.Value.Identifier(), loadOp, storeOp, clearFlag, clearColor, m_BaseRTDescriptor.Value.dimension);

                // TODO: We need a proper way to handle multiple camera/ camera stack. Issue is: multiple cameras can share a same RT
                // (e.g, split screen games). However devs have to be dilligent with it and know when to clear/preserve color.
                // For now we make it consistent by resolving viewport with a RT until we can have a proper camera management system
                //if (colorAttachmentHandle == -1 && !cameraData.isDefaultViewport)
                //    cmd.SetViewport(camera.pixelRect);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                XRUtils.DrawOcclusionMesh(cmd, camera, m_RenderingData.Value.cameraData.isStereoEnabled);
                var sortFlags = m_RenderingData.Value.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawRendererSettings(camera, sortFlags, m_RendererConfiguration.Value, m_RenderingData.Value.supportsDynamicBatching);
                context.DrawRenderers(m_RenderingData.Value.cullResults.visibleRenderers, ref drawSettings, opaqueFilterSettings);

                // Render objects that did not match any shader pass with error shader
                //TODO: FIX THIS
                //renderer.RenderObjectsWithError(context, ref renderingData.cullResults, camera, opaqueFilterSettings, SortFlags.None);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
