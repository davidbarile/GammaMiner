#if LETAI_TRUESHADOW
using LeTai.TrueShadow.PluginInterfaces;

namespace UnityEngine.UI.ProceduralImage
{
    public partial class ProceduralImage : ITrueShadowRendererNormalMaterialProvider
    {
        public Material GetTrueShadowRendererNormalMaterial()
        {
            return defaultMaterial;
        }
    }
}
#endif