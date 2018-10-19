using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEngine.Experimental.Rendering.ModularSRP
{
    [RenderPassGroup("LWRP")]
    class SetupLightsAndTexturesPass : LWRPRenderPass
    {
        // Inputs
        [RenderPassInput("RenderingData")]
        RenderPassReference<RenderingData> m_RenderingData;

        // Outputs
        [RenderPassOutput("MaxVisibleAdditionalLights")]
        RenderPassReference<int> m_MaxVisibleAdditionalLights;
        [RenderPassOutput("PerObjectLightIndices")]
        RenderPassReference<ComputeBuffer> m_PerObjectLightIndices;
        [RenderPassOutput("BaseRTDescriptor")]
        RenderPassReference<RenderTextureDescriptor> m_BaseRTDescriptor;
        [RenderPassOutput("DepthAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_DepthAttachmentHandle;
        [RenderPassOutput("ColorAttachmentHandle")]
        RenderPassReference<RenderTargetHandle> m_ColorAttachmentHandle;
        [RenderPassOutput("RendererConfiguration")]
        RenderPassReference<RendererConfiguration> m_RendererConfiguration;


        const int k_MaxVisibleAdditionalLightsNoStructuredBuffer = 16;
        const int k_MaxVisibleAdditioanlLightsStructuredBuffer = 4096;

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

        public void SetupPerObjectLightIndices(ref CullResults cullResults, ref LightData lightData)
        {
            if (lightData.additionalLightsCount == 0)
                return;

            List<VisibleLight> visibleLights = lightData.visibleLights;
            int[] perObjectLightIndexMap = cullResults.GetLightIndexMap();

            int directionalLightsCount = 0;
            int localLightsCount = 0;

            // Disable all directional lights from the perobject light indices
            // Pipeline handles them globally.
            for (int i = 0; i < visibleLights.Count && localLightsCount < lightData.additionalLightIndices.Count; ++i)
            {
                VisibleLight light = visibleLights[i];
                if (light.lightType == LightType.Directional)
                {
                    perObjectLightIndexMap[i] = -1;
                    ++directionalLightsCount;
                }
                else
                {
                    perObjectLightIndexMap[i] -= directionalLightsCount;
                    ++localLightsCount;
                }
            }

            // Disable all remaining lights we cannot fit into the global light buffer.
            for (int i = directionalLightsCount + localLightsCount; i < visibleLights.Count; ++i)
                perObjectLightIndexMap[i] = -1;

            cullResults.SetLightIndexMap(perObjectLightIndexMap);

            // if not using a compute buffer, engine will set indices in 2 vec4 constants
            // unity_4LightIndices0 and unity_4LightIndices1
            if (useStructuredBufferForLights)
            {
                int lightIndicesCount = cullResults.GetLightIndicesCount();
                if (lightIndicesCount > 0)
                {
                    if (m_PerObjectLightIndices == null)
                    {
                        m_PerObjectLightIndices.Value = new ComputeBuffer(lightIndicesCount, sizeof(int));
                    }
                    else if (m_PerObjectLightIndices.Value.count < lightIndicesCount)
                    {
                        m_PerObjectLightIndices.Value.Release();
                        m_PerObjectLightIndices.Value = new ComputeBuffer(lightIndicesCount, sizeof(int));
                    }

                    cullResults.FillLightIndices(m_PerObjectLightIndices.Value);
                }
            }
        }

        public override void Initialize()
        {
            //m_MaxVisibleAdditionalLights.Value = useStructuredBufferForLights ? k_MaxVisibleAdditioanlLightsStructuredBuffer : k_MaxVisibleAdditionalLightsNoStructuredBuffer;
            //SetupPerObjectLightIndices(ref m_RenderingData.Value.cullResults, ref m_RenderingData.Value.lightData);

            //m_BaseRTDescriptor.Value = CreateRenderTextureDescriptor(ref m_RenderingData.Value.cameraData);
            //m_DepthAttachmentHandle.Value.Init("_CameraDepthAttachment");
            //m_ColorAttachmentHandle.Value.Init("_CameraColorTexture");
        }

        public override void Execute(ScriptableRenderContext context)
        {
            m_MaxVisibleAdditionalLights.Value = useStructuredBufferForLights ? k_MaxVisibleAdditioanlLightsStructuredBuffer : k_MaxVisibleAdditionalLightsNoStructuredBuffer;
            m_BaseRTDescriptor.Value = CreateRenderTextureDescriptor(ref m_RenderingData.Value.cameraData);
            m_DepthAttachmentHandle.Value.Init("_CameraDepthAttachment");
            m_ColorAttachmentHandle.Value.Init("_CameraColorTexture");

            SetupPerObjectLightIndices(ref m_RenderingData.Value.cullResults, ref m_RenderingData.Value.lightData);

            m_RendererConfiguration.Value = ScriptableRenderer.GetRendererConfiguration(m_RenderingData.Value.lightData.additionalLightsCount);
        }
    }
}