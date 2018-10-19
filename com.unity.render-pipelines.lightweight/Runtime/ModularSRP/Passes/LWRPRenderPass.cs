using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    public abstract class LWRPRenderPass : ScriptableRenderPass
    {
        public static bool useStructuredBufferForLights
        {
            get
            {
                // TODO: Graphics Emulation are breaking StructuredBuffers disabling it for now until
                // we have a fix for it
                return false;
                // return SystemInfo.supportsComputeShaders &&
                //        SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore &&
                //        !Application.isMobilePlatform &&
                //        Application.platform != RuntimePlatform.WebGLPlayer;
            }
        }

        protected RenderTextureDescriptor CreateRenderTextureDescriptor(ref CameraData cameraData, float scaler = 1.0f)
        {
            Camera camera = cameraData.camera;
            RenderTextureDescriptor desc;
            float renderScale = cameraData.renderScale;

            if (cameraData.isStereoEnabled)
            {
                return XRGraphicsConfig.eyeTextureDesc;
            }
            else
            {
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
            }
            desc.colorFormat = cameraData.isHdrEnabled ? RenderTextureFormat.DefaultHDR :
                RenderTextureFormat.Default;
            desc.enableRandomWrite = false;
            desc.sRGB = true;
            desc.width = (int)((float)desc.width * renderScale * scaler);
            desc.height = (int)((float)desc.height * renderScale * scaler);
            return desc;
        }

        public ClearFlag GetCameraClearFlag(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            ClearFlag clearFlag = ClearFlag.None;
            CameraClearFlags cameraClearFlags = camera.clearFlags;
            if (cameraClearFlags != CameraClearFlags.Nothing)
            {
                clearFlag |= ClearFlag.Depth;
                if (cameraClearFlags == CameraClearFlags.Color || cameraClearFlags == CameraClearFlags.Skybox)
                    clearFlag |= ClearFlag.Color;
            }

            return clearFlag;
        }

        public RendererConfiguration GetRendererConfiguration(int additionalLightsCount)
        {
            RendererConfiguration configuration = RendererConfiguration.PerObjectReflectionProbes | RendererConfiguration.PerObjectLightmaps | RendererConfiguration.PerObjectLightProbe;
            if (additionalLightsCount > 0)
            {
                if (useStructuredBufferForLights)
                    configuration |= RendererConfiguration.ProvideLightIndices;
                else
                    configuration |= RendererConfiguration.PerObjectLightIndices8;
            }

            return configuration;
        }

        public void RenderPostProcess(PostProcessRenderContext postProcessingContext, CommandBuffer cmd, ref CameraData cameraData, RenderTextureFormat colorFormat, RenderTargetIdentifier source, RenderTargetIdentifier dest, bool opaqueOnly)
        {
            Camera camera = cameraData.camera;
            postProcessingContext.Reset();
            postProcessingContext.camera = camera;
            postProcessingContext.source = source;
            postProcessingContext.sourceFormat = colorFormat;
            postProcessingContext.destination = dest;
            postProcessingContext.command = cmd;
            postProcessingContext.flip = !cameraData.isStereoEnabled && camera.targetTexture == null;

            if (opaqueOnly)
                cameraData.postProcessLayer.RenderOpaqueOnly(postProcessingContext);
            else
                cameraData.postProcessLayer.Render(postProcessingContext);
        }
    }
}