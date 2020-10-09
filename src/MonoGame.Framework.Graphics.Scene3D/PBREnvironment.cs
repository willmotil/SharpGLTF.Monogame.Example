using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines all athmospheric and lighting properties for a given scene setup.
    /// </summary>
    public class PBREnvironment
    {
        #region constants

        public static PBREnvironment CreateDefault()
        {
            var env = new PBREnvironment();
            env.SetDirectLight(0, (20, 30), Color.White, 3.5f);
            env.SetDirectLight(1, (-70, 60), Color.DeepSkyBlue, 1.5f);
            env.SetDirectLight(2, (50, -50), Color.OrangeRed, 0.5f);
            return env;
        }

        #endregion

        #region data

        private float _Exposure = 2.5f;
        private Vector3 _AmbientLight = Vector3.Zero;
        private readonly List<PBRPunctualLight> _PunctualLights = new List<PBRPunctualLight>();

        #endregion

        #region cubemap for enviroment.

        private static TextureCube _iblCubeMap;
        private static Texture2D _iblLutMap;
        public void SetEnviromentalCubeMap(TextureCube iblCubeMap) { _iblCubeMap = iblCubeMap; }
        public void SetEnviromentalLUTMap(Texture2D iblLutMap) { _iblLutMap = iblLutMap; }

        private static float _testValue;
        public void SetTestingValue(float testValue) { _testValue = testValue; }

        #endregion

        #region API

        public void SetExposure(float exposure) { _Exposure = exposure; }

        public void SetAmbientLight(Vector3 color) { _AmbientLight = color; }

        public void SetDirectLight(int idx, (int direction, int elevation) degrees, Color color, float intensity)
        {
            _SetPunctualLight(idx, PBRPunctualLight.Directional(degrees, color.ToVector3(), intensity));
        }

        public void SetDirectLight(int idx, Vector3 direction, Color color, float intensity)
        {
            _SetPunctualLight(idx, PBRPunctualLight.Directional(direction, color.ToVector3(), intensity));
        }

        private void _SetPunctualLight(int idx, PBRPunctualLight l)
        {
            while (_PunctualLights.Count <= idx) _PunctualLights.Add(default);
            _PunctualLights[idx] = l;
        }

        public void ApplyTo(Effect effect)
        {
            if (effect is IEffectFog fog) { fog.FogEnabled = false; }

            //if (effect is IEffectFog)
            //{
            if (_iblCubeMap != null && effect.Name != "Unlit")
            {
                //Console.WriteLine("_iblCubeMap ok, " + effect + " , " + effect.Name + " , " + effect.CurrentTechnique.Name);
                effect.Parameters["envCubeMap"].SetValue(_iblCubeMap);  // u_GGXEnvSampler   u_EnvCubeSampler
                effect.Parameters["u_GGXLUT"].SetValue(_iblLutMap); // u_GGXLUT  u_CharlieLUT
                effect.Parameters["testValue"].SetValue(_testValue);
            }
            else
            {
                //Console.WriteLine("_iblCubeMap  null " + effect + " , " + effect.Name + " , " + effect.CurrentTechnique.Name);
            }
            //}

            PBRPunctualLight.ApplyLights(effect, _Exposure, _AmbientLight, _PunctualLights);
        }

        #endregion
    }
}
