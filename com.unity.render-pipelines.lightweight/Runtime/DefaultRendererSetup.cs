//using System;
//using UnityEngine.Rendering;

//namespace UnityEngine.Experimental.Rendering.LightweightPipeline
//{
//    public class DefaultRendererSetup : IRendererSetup
//    {
//        private SetupForwardRenderingPass m_SetupForwardRenderingPass;
//        private CreateLightweightRenderTexturesPass m_CreateLightweightRenderTexturesPass;
//        private SetupLightweightConstanstPass m_SetupLightweightConstants;
//        private RenderOpaqueForwardPass m_RenderOpaqueForwardPass;
//        private DrawSkyboxPass m_DrawSkyboxPass;
//        private RenderTransparentForwardPass m_RenderTransparentForwardPass;
//        private TransparentPostProcessPass m_TransparentPostProcessPass;
//        private EndXRRenderingPass m_EndXrRenderingPass;

//#if UNITY_EDITOR
//        private SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
//#endif


//        private RenderTargetHandle ColorAttachment;
//        private RenderTargetHandle DepthAttachment;
//        private RenderTargetHandle DepthTexture;
//        private RenderTargetHandle OpaqueColor;

//        [NonSerialized]
//        private bool m_Initialized = false;

//        private void Init()
//        {
//            if (m_Initialized)
//                return;

//            m_SetupForwardRenderingPass = new SetupForwardRenderingPass();
//            m_CreateLightweightRenderTexturesPass = new CreateLightweightRenderTexturesPass();
//            m_SetupLightweightConstants = new SetupLightweightConstanstPass();
//            m_RenderOpaqueForwardPass = new RenderOpaqueForwardPass();
//            m_DrawSkyboxPass = new DrawSkyboxPass();
//            m_RenderTransparentForwardPass = new RenderTransparentForwardPass();
//            m_TransparentPostProcessPass = new TransparentPostProcessPass();

//#if UNITY_EDITOR
//            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass();
//#endif

//            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
//            // Samples (MSAA) depend on camera and pipeline
//            ColorAttachment.Init("_CameraColorTexture");
//            DepthAttachment.Init("_CameraDepthAttachment");
//            DepthTexture.Init("_CameraDepthTexture");
//            OpaqueColor.Init("_CameraOpaqueTexture");

//            m_Initialized = true;
//        }

       
//        public void Setup(ScriptableRenderer renderer, ref ScriptableRenderContext context,
//            ref CullResults cullResults, ref RenderingData renderingData)
//        {
//            Init();

//            renderer.Clear();

//            Camera camera = renderingData.cameraData.camera;

//            renderer.SetupPerObjectLightIndices(ref cullResults, ref renderingData.lightData);
//            RenderTextureDescriptor baseDescriptor = ScriptableRenderer.CreateRTDesc(ref renderingData.cameraData);

//            renderer.EnqueuePass(m_SetupForwardRenderingPass);

//            bool requiresRenderToTexture =
//                ScriptableRenderer.RequiresIntermediateColorTexture(
//                    ref renderingData.cameraData,
//                    baseDescriptor);
            
//            RenderTargetHandle colorHandle = RenderTargetHandle.CameraTarget;
//            RenderTargetHandle depthHandle = RenderTargetHandle.CameraTarget;

//            if (requiresRenderToTexture)
//            {
//                colorHandle = ColorAttachment;
//                depthHandle = DepthAttachment;
                
//                var sampleCount = (SampleCount)renderingData.cameraData.msaaSamples;
//                m_CreateLightweightRenderTexturesPass.Setup(baseDescriptor, colorHandle, depthHandle, sampleCount);
//                renderer.EnqueuePass(m_CreateLightweightRenderTexturesPass);
//            }          

//            bool dynamicBatching = renderingData.supportsDynamicBatching;
//            RendererConfiguration rendererConfiguration = ScriptableRenderer.GetRendererConfiguration(renderingData.lightData.totalAdditionalLightsCount);

//            m_SetupLightweightConstants.Setup(renderer.maxVisibleLocalLights, renderer.perObjectLightIndices);
//            renderer.EnqueuePass(m_SetupLightweightConstants);

//            m_RenderOpaqueForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, ScriptableRenderer.GetCameraClearFlag(camera), camera.backgroundColor, rendererConfiguration, dynamicBatching);
//            renderer.EnqueuePass(m_RenderOpaqueForwardPass);

//            if (camera.clearFlags == CameraClearFlags.Skybox)
//            {
//                m_DrawSkyboxPass.Setup(colorHandle, depthHandle);
//                renderer.EnqueuePass(m_DrawSkyboxPass);
//            }

//            m_RenderTransparentForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, ClearFlag.None, camera.backgroundColor, rendererConfiguration, dynamicBatching);
//            renderer.EnqueuePass(m_RenderTransparentForwardPass);

//            if (!renderingData.cameraData.isStereoEnabled && renderingData.cameraData.postProcessEnabled)
//            {
//                m_TransparentPostProcessPass.Setup(renderer.postProcessRenderContext, baseDescriptor, colorHandle, BuiltinRenderTextureType.CameraTarget);
//                renderer.EnqueuePass(m_TransparentPostProcessPass);
//            }
//        }
//    }
//}
