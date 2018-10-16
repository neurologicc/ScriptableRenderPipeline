namespace UnityEngine.Experimental.Rendering.LightweightPipeline
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class LWRPAdditionalLightData : MonoBehaviour
    {
        [Tooltip("Controls the distance by which the shadows will be pushed away from the light. Useful for avoiding false self-shadowing artifacts.")]
        [SerializeField] float m_DepthBias = 1.0f;

        [Tooltip("Controls distance by which the shadow casting surfaces will be shrunk along the surface normal. Useful for avoiding false self-shadowing artifacts.")]
        [SerializeField] float m_NormalBias = 1.0f;

        [Tooltip("Controls the usage of pipeline settings.")]
        [SerializeField] bool m_UsePipelineSettings = true;

        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }

        public float depthBias
        {
            get { return m_DepthBias; }
            set { m_DepthBias = value; }
        }

        public float normalBias
        {
            get { return m_NormalBias; }
            set { m_NormalBias = value; }
        }
    }
}
