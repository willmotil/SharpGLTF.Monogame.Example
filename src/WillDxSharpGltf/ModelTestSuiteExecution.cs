using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WillDxSharpGltf
{
    public class ModelTestSuiteExecution
    {

        public static float _targetScale = 1f;
        public static Vector3 _testTarget = new Vector3(35, 0, -30);

        public static float DefaultDist = 8f;
        public static float DefaultDist2 = 12f;

        public static Vector3 CamposFromTarget(Vector3 p, Vector3 t, float d) { return Vector3.Normalize(p - t) * d + p; }

        public static Vector3 _VertexColorTestPos = new Vector3(0, 0, -6);
        public static Vector3 _TextureCoordinateTestPos = new Vector3(5, 0, -6);
        public static Vector3 _TextureSettingsTestPos = new Vector3(11, 0, -6);
        public static Vector3 _MultiUvTestPos = new Vector3(16, 0, -6);
        public static Vector3 _TextureTransformMultiTestPos = new Vector3(21, 0, -6);
        public static Vector3 _TextureTransformTestPos = new Vector3(28, 0, -6);
        public static Vector3 _AlphaBlendModeTestPos = new Vector3(38, 0, -6);
        public static Vector3 _NormalTangentMirrorTestPos = new Vector3(48, 0, -6);
        public static Vector3 _UnlitTestPos = new Vector3(55, 0, -6);
        public static Vector3 _InterpolationTestPos = new Vector3(60, 0, -6);
        public static Vector3 _AnimatedMorphCubePos = new Vector3(65, 0, -6);
        public static Vector3 _ClearCoatTestPos = new Vector3(70, 0, -6);
        public static Vector3 _SpecGlossVsMetalRoughPos = new Vector3(75, 0, -6);

        public Vector3[] _wayPoints = new Vector3[]
        {
            new Vector3(35, 20, 25), new Vector3(0, 20, 25), new Vector3(70, 20, 25), new Vector3(0, 15, 25),
            CamposFromTarget(_VertexColorTestPos, _testTarget, DefaultDist2),    // _VertexColorTest
            CamposFromTarget(_TextureCoordinateTestPos, _testTarget, DefaultDist2),    // _TextureCoordinateTest
            CamposFromTarget(_TextureSettingsTestPos, _testTarget, DefaultDist2),    // _TextureSettingsTest
            CamposFromTarget(_MultiUvTestPos, _testTarget, DefaultDist2),   // _MultiUvTest
            CamposFromTarget(_TextureTransformMultiTestPos, _testTarget, DefaultDist2), // _TextureTransformMultiTest
            CamposFromTarget(_TextureTransformTestPos, _testTarget, DefaultDist2), // _TextureTransformTest
            CamposFromTarget(_AlphaBlendModeTestPos, _testTarget, DefaultDist2),   // _AlphaBlendModeTest
            CamposFromTarget(_NormalTangentMirrorTestPos, _testTarget, DefaultDist2), // _NormalTangentMirrorTest
            CamposFromTarget(_UnlitTestPos, _testTarget, DefaultDist2),    // _UnlitTest
            CamposFromTarget(_InterpolationTestPos, _testTarget, DefaultDist2),   // _InterpolationTest
            CamposFromTarget(_AnimatedMorphCubePos, _testTarget, DefaultDist2),  // _AnimatedMorphCube
            CamposFromTarget(_ClearCoatTestPos, _testTarget, DefaultDist2),  // _ClearCoatTestPos
            CamposFromTarget(_SpecGlossVsMetalRoughPos, _testTarget, DefaultDist2),  // _SpecGlossVsMetalRough
            

            CamposFromTarget(_SpecGlossVsMetalRoughPos, _testTarget, DefaultDist),  // _SpecGlossVsMetalRough
            CamposFromTarget(_ClearCoatTestPos, _testTarget, DefaultDist),  // _ClearCoatTestPos
            CamposFromTarget(_AnimatedMorphCubePos, _testTarget, DefaultDist),  // _AnimatedMorphCube
            CamposFromTarget(_InterpolationTestPos, _testTarget, DefaultDist),   // _InterpolationTest
            CamposFromTarget(_UnlitTestPos, _testTarget, DefaultDist),    // _UnlitTest
            CamposFromTarget(_NormalTangentMirrorTestPos, _testTarget, DefaultDist),
            CamposFromTarget(_AlphaBlendModeTestPos, _testTarget, DefaultDist),   // _AlphaBlendModeTest
            CamposFromTarget(_TextureTransformTestPos, _testTarget, DefaultDist), // _TextureTransformTest
            CamposFromTarget(_TextureTransformMultiTestPos, _testTarget, DefaultDist), // _TextureTransformMultiTest
            CamposFromTarget(_MultiUvTestPos, _testTarget, DefaultDist),   // _MultiUvTest
            CamposFromTarget(_TextureSettingsTestPos, _testTarget, DefaultDist),    // _TextureSettingsTest
            CamposFromTarget(_TextureCoordinateTestPos, _testTarget, DefaultDist),    // _TextureCoordinateTest
            CamposFromTarget(_VertexColorTestPos, _testTarget, DefaultDist),    // _VertexColorTest

            _VertexColorTestPos + new Vector3(0, 10, 0),
            _testTarget + new Vector3(0, 2, 10),

            new Vector3(8, 0, 8) + _testTarget, new Vector3(8, 0, -8) + _testTarget, new Vector3(-8, 0, -8) + _testTarget, new Vector3(-8, 0, 8) + _testTarget,
            new Vector3(4, -1, 4) + _testTarget, new Vector3(4, -1, -4) + _testTarget, new Vector3(-3, -1, -3) + _testTarget, new Vector3(-2, -1, 2) + _testTarget,
            new Vector3(5, 20, 5) + _testTarget, new Vector3(5, -10, -5) + _testTarget, new Vector3(-5, -20, -5) + _testTarget, new Vector3(-5, 10, 30) + _testTarget
     };


        #region resources

        // these are the actual hardware resources that represent every model's geometry.        

        ModelCollectionContent _TestTemplate;
        ModelCollectionContent _VertexColorTestTemplate;
        ModelCollectionContent _TextureCoordinateTestTemplate;
        ModelCollectionContent _TextureSettingsTestTemplate;
        ModelCollectionContent _MultiUvTestTemplate;
        ModelCollectionContent _TextureTransformMultiTestTemplate;
        ModelCollectionContent _TextureTransformTestTemplate;
        ModelCollectionContent _AlphaBlendModeTestTemplate;
        ModelCollectionContent _NormalTangentMirrorTestTemplate;
        ModelCollectionContent _UnlitTestTemplate;
        ModelCollectionContent _InterpolationTestTemplate;
        //ModelCollectionContent _AnimatedMorphCubeTemplate;
        ModelCollectionContent _ClearCoatTestTemplate;
        ModelCollectionContent _SpecGlossVsMetalRoughTemplate;

        // these are the scene instances we create for every glTF model we want to render on screen.
        // Instances are designed to be as lightweight as possible, so it should not be a problem to
        // create as many of them as you need at runtime.
        public ModelInstance _Test;
        public ModelInstance _VertexColorTest;
        public ModelInstance _AlphaBlendModeTest;
        public ModelInstance _MultiUvTest;
        public ModelInstance _TextureCoordinateTest;
        public ModelInstance _NormalTangentMirrorTest;
        public ModelInstance _TextureSettingsTest;
        public ModelInstance _UnlitTest;
        public ModelInstance _InterpolationTest;
        //private ModelInstance _AnimatedMorphCube;
        public ModelInstance _TextureTransformMultiTest;
        public ModelInstance _TextureTransformTest;
        public ModelInstance _ClearCoatTest;
        public ModelInstance _SpecGlossVsMetalRough;


        #endregion

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            SelectTestModel(graphicsDevice);

            LoadStandardTestingModels(graphicsDevice);
        }

        public void LoadStandardTestingModels(GraphicsDevice graphicsDevice)
        {
            ModelCollectionContent _load(string filePath)
            {
                return Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel(filePath, graphicsDevice);
            }
            
            SelectTestModel(graphicsDevice);

            _VertexColorTestTemplate = _load("Models\\VertexColorTest.glb");
            _TextureCoordinateTestTemplate = _load("Models\\TextureCoordinateTest.glb");
            _TextureSettingsTestTemplate = _load("Models\\TextureSettingsTest.glb");
            _MultiUvTestTemplate = _load("Models\\MultiUvTest.glb");
            _TextureTransformMultiTestTemplate = _load("Models\\TextureTransformMultiTest.glb");
            _TextureTransformTestTemplate = _load("Models\\TextureTransformTest.glb");
            _AlphaBlendModeTestTemplate = _load("Models\\AlphaBlendModeTest.glb");
            _NormalTangentMirrorTestTemplate = _load("Models\\NormalTangentMirrorTest.glb");
            _UnlitTestTemplate = _load("Models\\UnlitTest.glb");
            _InterpolationTestTemplate = _load("Models\\InterpolationTest.glb");
            //_AnimatedMorphCubeTemplate = _load("Models\\AnimatedMorphCube.glb");
            _ClearCoatTestTemplate = _load("Models\\ClearCoatTest.glb");
            _SpecGlossVsMetalRoughTemplate = _load("Models\\SpecGlossVsMetalRough.glb");
        }
        public void SelectTestModel(GraphicsDevice graphicsDevice)
        {

            ModelCollectionContent _load(string filePath)
            {
                return Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel(filePath, graphicsDevice);
            }

            // pass i think.
            //_TestTemplate = _load("Models\\OrientationTest.glb"); _targetScale = .3f;

            //  passes https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/TextureCoordinateTest/screenshot/screenshot.png
            //_TestTemplate = _load("Models\\TextureCoordinateTest.glb");                 _targetScale = 1;

            //  Passes for dir lights.  fails for enviromental lighting Not yet implemented.
            //_TestTemplate = _load("Models\\MetalRoughSpheres.glb");                     _targetScale = 1;

            // passes vertex colors,  passes for texture samplers. https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/VertexColorTest/screenshot/screenshot.png
            //_TestTemplate = _load("Models\\VertexColorTest.glb");                            _targetScale = 3;

            //  passes.  https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/BoxVertexColors/screenshot/screenshot.png
            //_TestTemplate = _load("Models\\BoxVertexColors.glb");                          _targetScale = 3;

            // mostly works as far as i can tell. https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/ClearCoatTest/screenshot/screenshot_large.jpg
            //_TestTemplate = _load("Models\\ClearCoatTest.glb");                            _targetScale = 1;


            //Msg.TrackStructureLog(1, $"_TestTemplate = loader.LoadDeviceModel( Models\\AlphaBlendModeTest.glb );                   _targetScale = 3;", 1);
            //  passes  
            //  https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/AlphaBlendModeTest/screenshot/screenshot_large.jpg
            //_TestTemplate = _load("Models\\AlphaBlendModeTest.glb"); _targetScale = 2;

            //Msg.TrackStructureLog(1, $"", 1);

            //  some still fail https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/TextureTransformMultiTest/screenshot/screenshot.jpg
            //_TestTemplate = _load("Models\\TextureTransformMultiTest.glb"); targetScale = 4;

            //  some still fail. https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/TextureSettingsTest/screenshot/screenshot.png
            //_TestTemplate = _load("Models\\TextureSettingsTest.glb");                     _targetScale = 1;

            // some still fail
            //_TestTemplate = _load("Models\\_TextureTransformMultiTest.glb");                     _targetScale = 1;

            // not yet implemented.  https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/MultiUVTest/screenshot/screenshot.jpg
            //_TestTemplate = _load("Models\\MultiUvTest.glb");                                _targetScale = 3;

            // Half passes half fails.  
            // https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/NormalTangentMirrorTest/screenshot/screenshot.png
            //_TestTemplate = _load("Models\\NormalTangentMirrorTest.glb");            _targetScale = 3;

            //_TestTemplate = _load("Models\\NormalTangentTest.glb");            _targetScale = 3;

            //  passes
            //_TestTemplate = _load("Models\\vertex_colors.glb");                              _targetScale = 1;

            //  fail partial success.
            //_TestTemplate = _load("Models\\BoxAnimated.glb");                              _targetScale = 2;

            // Fails Vertex Colors in this test need to ignore light shading to pass ... aka... simply draw the color that is present in either the vertex or texture.
            // https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/UnlitTest/screenshot/unlit_test_fail.jpg
            // this test is for a part attribute that tells the mesh part to skip lighting techniques even if present, and only use vertex colors or texture colors i think
            //_TestTemplate = _load("Models\\UnlitTest.glb");                                    _targetScale = 1;

            // 8 fails 1 passed test  https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/InterpolationTest/screenshot/screenshot.gif
            // i dunno even what they mean by linear. 
            // their are tons of cubic spline formulas (probably a besier curve) will have to find their specific implementation.
            // two of the regular step interpolations aren't working either though.
            //_TestTemplate = _load("Models\\InterpolationTest.glb");                         _targetScale = 1;

            // Fixed   typo bug. line 159  != instead of ==
            // This caused a application crashing null reference exception.  
            // This appears to have a null material not sure if that needs to be null checked too.
            // https://github.com/vpenades/SharpGLTF.Monogame.Example/blob/master/src/SharpGLTF.Runtime.MonoGame/LoaderContext.cs#L159
            _TestTemplate = _load("Models\\MetalRoughSpheresNoTextures.glb"); _targetScale = 500;

            //  appears to pass, need a better test model inconclusive, need a clear test to check both normal and bump types especially with this double sided thing going on.
            //_TestTemplate = _load("Models\\NormalTangentTest.glb");                     _targetScale = 1;

            //  Not yet implemented
            //_TestTemplate =_load("Models\\AnimatedMorphCube.glb");                  _targetScale = 1;

            //   Not yet implemented
            //_TestTemplate = _load("Models\\AnimatedMorphSphere.glb");                _targetScale = 1;

            // umm dunno if i like this DGF energy scheme.
            //_TestTemplate = _load("Models\\sceneMaterialBallA.glb");                     _targetScale = 0.05f;

            //  looks ok
            //_TestTemplate =_load("Models\\shark.glb");                                        _targetScale = 1.0f;

            //  looks ok
            //_TestTemplate = _load("Models\\BoomBox.glb");                                  _targetScale = 100.0f;

            //  passes normal map inconclusive.
            //_TestTemplate = _load("Models\\WaterBottle.glb");                               _targetScale = 15;

            //  looks ok
            //_TestTemplate = _load("Models\\DamagedHelmet.glb");                        _targetScale = 4;

            //  looks ok
            //_TestTemplate = _load("Models\\flightHelmet.glb");                        _targetScale = .1f;

            //  passes
            //_TestTemplate = _load("Models\\SpecGlossVsMetalRough.glb"); _targetScale = 15;

            //_TestTemplate = _load("Models\\dragon.glb"); _targetScale = 15;
            //_TestTemplate = _load("Models\\duck.glb"); _targetScale = 1;
            //_TestTemplate = _load("Models\\underwaterScene.glb"); _targetScale = 1;
            //_TestTemplate = _load("Models\\underwaterSceneRocksBarnaclesMussels.glb"); _targetScale = 1;

            //_TestTemplate = _load("Models\\BrainStem.glb"); _targetScale = 1;

            // The energy output of this DGF shader for point lights appears to be too high but that is trivial.
            // Already starting to get stutters im almost afraid to put my counter back in and look at the collections now.
        }

        public void UpdateModels(GameTime gameTime)
        {
            // create as many instances as we need from the templates

            if (_Test == null) _Test = _TestTemplate.DefaultModel.CreateInstance();
            if (_VertexColorTest == null) _VertexColorTest = _VertexColorTestTemplate.DefaultModel.CreateInstance();
            if (_TextureCoordinateTest == null) _TextureCoordinateTest = _TextureCoordinateTestTemplate.DefaultModel.CreateInstance();
            if (_TextureSettingsTest == null) _TextureSettingsTest = _TextureSettingsTestTemplate.DefaultModel.CreateInstance();
            if (_MultiUvTest == null) _MultiUvTest = _MultiUvTestTemplate.DefaultModel.CreateInstance();
            if (_TextureTransformMultiTest == null) _TextureTransformMultiTest = _TextureTransformMultiTestTemplate.DefaultModel.CreateInstance();
            if (_TextureTransformTest == null) _TextureTransformTest = _TextureTransformTestTemplate.DefaultModel.CreateInstance();
            if (_AlphaBlendModeTest == null) _AlphaBlendModeTest = _AlphaBlendModeTestTemplate.DefaultModel.CreateInstance();
            if (_NormalTangentMirrorTest == null) _NormalTangentMirrorTest = _NormalTangentMirrorTestTemplate.DefaultModel.CreateInstance();
            if (_UnlitTest == null) _UnlitTest = _UnlitTestTemplate.DefaultModel.CreateInstance();
            if (_InterpolationTest == null) _InterpolationTest = _InterpolationTestTemplate.DefaultModel.CreateInstance();
            //if (_AnimatedMorphCube == null) _AnimatedMorphCube = _AnimatedMorphCubeTemplate.DefaultModel.CreateInstance();
            if (_ClearCoatTest == null) _ClearCoatTest = _ClearCoatTestTemplate.DefaultModel.CreateInstance();
            if (_SpecGlossVsMetalRough == null) _SpecGlossVsMetalRough = _SpecGlossVsMetalRoughTemplate.DefaultModel.CreateInstance();

            // animate each instance individually.

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            _Test.WorldMatrix = Matrix.CreateScale(_targetScale) * Matrix.CreateTranslation(_testTarget);
            _VertexColorTest.WorldMatrix = Matrix.CreateScale(2.0f) * Matrix.CreateTranslation(_VertexColorTestPos);
            _TextureCoordinateTest.WorldMatrix = Matrix.CreateScale(2.0f) * Matrix.CreateTranslation(_TextureCoordinateTestPos);
            _TextureSettingsTest.WorldMatrix = Matrix.CreateScale(.5f) * Matrix.CreateTranslation(_TextureSettingsTestPos);
            _MultiUvTest.WorldMatrix = Matrix.CreateScale(1f) * Matrix.CreateTranslation(_MultiUvTestPos);
            _TextureTransformMultiTest.WorldMatrix = Matrix.CreateScale(2f) * Matrix.CreateTranslation(_TextureTransformMultiTestPos);
            _TextureTransformTest.WorldMatrix = Matrix.CreateScale(2f) * Matrix.CreateTranslation(_TextureTransformTestPos);
            _AlphaBlendModeTest.WorldMatrix = Matrix.CreateScale(1.0f) * Matrix.CreateTranslation(_AlphaBlendModeTestPos);
            _NormalTangentMirrorTest.WorldMatrix = Matrix.CreateScale(2f) * Matrix.CreateTranslation(_NormalTangentMirrorTestPos);
            _UnlitTest.WorldMatrix = Matrix.CreateScale(1f) * Matrix.CreateTranslation(_UnlitTestPos);
            _InterpolationTest.WorldMatrix = Matrix.CreateScale(.5f) * Matrix.CreateTranslation(_InterpolationTestPos);// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0/#animation-sampler
            //_AnimatedMorphCube.WorldMatrix = Matrix.CreateScale(.5f) * Matrix.CreateTranslation(_AnimatedMorphCubePos);
            _ClearCoatTest.WorldMatrix = Matrix.CreateScale(.4f) * Matrix.CreateTranslation(_ClearCoatTestPos);
            _SpecGlossVsMetalRough.WorldMatrix = Matrix.CreateScale(15f) * Matrix.CreateTranslation(_SpecGlossVsMetalRoughPos);

            _Test.Armature.SetAnimationFrame(0, 0.5f * animTime);
            _MultiUvTest.Armature.SetAnimationFrame(0, 0.7f * animTime);
            _TextureSettingsTest.Armature.SetAnimationFrame(0, 1.5f * animTime);
            _NormalTangentMirrorTest.Armature.SetAnimationFrame(0, 1.5f * animTime);
            _InterpolationTest.Armature.SetAnimationFrame(0, 1.0f * animTime);
            //_AnimatedMorphCube.Armature.SetAnimationFrame(0, 1.5f * animTime);
            _TextureTransformTest.Armature.SetAnimationFrame(0, 1.5f * animTime);
        }
    

        public void UnloadContent()
        {
            _TestTemplate?.Dispose();
            _TestTemplate = null;

            _AlphaBlendModeTestTemplate?.Dispose();
            _AlphaBlendModeTestTemplate = null;

            _VertexColorTestTemplate?.Dispose();
            _VertexColorTestTemplate = null;

            _TextureCoordinateTestTemplate?.Dispose();
            _TextureCoordinateTestTemplate = null;

            _MultiUvTestTemplate?.Dispose();
            _MultiUvTestTemplate = null;

            _TextureSettingsTestTemplate?.Dispose();
            _TextureSettingsTestTemplate = null;

            _NormalTangentMirrorTestTemplate?.Dispose();
            _NormalTangentMirrorTestTemplate = null;

            _UnlitTestTemplate?.Dispose();
            _UnlitTestTemplate = null;

            _InterpolationTestTemplate?.Dispose();
            _InterpolationTestTemplate = null;

            //_AnimatedMorphCubeTemplate?.Dispose();
            //_AnimatedMorphCubeTemplate = null;

            _TextureTransformMultiTestTemplate?.Dispose();
            _TextureTransformMultiTestTemplate = null;

            _TextureTransformTestTemplate?.Dispose();
            _TextureTransformTestTemplate = null;

            _ClearCoatTestTemplate?.Dispose();
            _ClearCoatTestTemplate = null;

            _SpecGlossVsMetalRoughTemplate?.Dispose();
            _SpecGlossVsMetalRoughTemplate = null;
        }

    }
}
