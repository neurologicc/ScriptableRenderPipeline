using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.LightweightPipeline;


namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    /// <summary>
    /// Inherit from this class to perform custom rendering in the Lightweight Render Pipeline. 
    /// </summary>
    public abstract class ScriptableRenderPass : IRenderPass
    {
        private List<ShaderPassName> m_ShaderPassNames = new List<ShaderPassName>();

        /// <summary>
        /// Cleanup any allocated data that was created during the execution of the pass.
        /// </summary>
        /// <param name="cmd">Use this CommandBuffer to cleanup any generated data</param>
        public virtual void FrameCleanup(CommandBuffer cmd)
        {}

        
        protected void ClearShaderPassNames()
        {
            m_ShaderPassNames.Clear();
        }

        protected void RegisterShaderPassName(string passName)
        {
            m_ShaderPassNames.Add(new ShaderPassName(passName));
        }


        public virtual void Initialize() { }

        public abstract void Execute(ScriptableRenderContext context);

        protected DrawRendererSettings CreateDrawRendererSettings(Camera camera, SortFlags sortFlags, RendererConfiguration rendererConfiguration, bool supportsDynamicBatching)
        {
            DrawRendererSettings settings = new DrawRendererSettings(camera, m_ShaderPassNames[0]);
            for (int i = 1; i < m_ShaderPassNames.Count; ++i)
                settings.SetShaderPassName(i, m_ShaderPassNames[i]);
            settings.sorting.flags = sortFlags;
            settings.rendererConfiguration = rendererConfiguration;
            settings.flags = DrawRendererFlags.EnableInstancing;
            if (supportsDynamicBatching)
                settings.flags |= DrawRendererFlags.EnableDynamicBatching;
            return settings;
        }

        protected static void SetRenderTarget(
            CommandBuffer cmd,
            RenderTargetIdentifier colorAttachment,
            RenderBufferLoadAction colorLoadAction,
            RenderBufferStoreAction colorStoreAction,
            ClearFlag clearFlags,
            Color clearColor,
            TextureDimension dimension)
        {
            if (dimension == TextureDimension.Tex2DArray)
                CoreUtils.SetRenderTarget(cmd, colorAttachment, clearFlags, clearColor, 0, CubemapFace.Unknown, -1);
            else
                CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, clearFlags, clearColor);
        }

        protected static void SetRenderTarget(
            CommandBuffer cmd,
            RenderTargetIdentifier colorAttachment,
            RenderBufferLoadAction colorLoadAction,
            RenderBufferStoreAction colorStoreAction,
            RenderTargetIdentifier depthAttachment,
            RenderBufferLoadAction depthLoadAction,
            RenderBufferStoreAction depthStoreAction,
            ClearFlag clearFlag,
            Color clearColor,
            TextureDimension dimension)
        {
            if (dimension == TextureDimension.Tex2DArray)
                CoreUtils.SetRenderTarget(cmd, colorAttachment, depthAttachment,
                    clearFlag, clearColor, 0, CubemapFace.Unknown, -1);
            else
                CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction,
                    depthAttachment, depthLoadAction, depthStoreAction, clearFlag, clearColor);
        }
    }
}
