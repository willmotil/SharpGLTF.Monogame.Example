using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// need to take a look at this later i want to create a skycube ect not require a user to load one.
//https://github.com/vpenades/MonoGame.AnimAndPBR/blob/master/src/MonoGame.Framework.Graphics.Toolkit3D/Graphics/ModelArchitecture.md

namespace WillDxSharpGltf
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game2 : Game
    {
        SpriteBatch _spriteBatch;
        SpriteFont font;
        public static Effect primitivesEffect;

        SpherePNTT cube, cube2, sky;
        float TestValue1 = 0;
        float TestValue2 = 1;
        string msg = "";

        private Texture2D _texture;
        private Texture2D _generatedTexture;

        private Texture2D _ldrTexture;
        private Texture2D _premadeLut;
        private TextureCube _textureCubeMap;
        private Texture2D[] _ldrTextureFaces;
        private Texture2D _cmLeft;
        private Texture2D _cmRight;
        private Texture2D _cmFront;
        private Texture2D _cmBack;
        private Texture2D _cmTop;
        private Texture2D _cmBottom;

        #region camera variables.

        DemoCamera _camera;
        bool _useDemoWaypoints = true;
        static float _targetScale = 1f;
        static Vector3 _testTarget = new Vector3(35, 0, -30);

        
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


        public static Vector3 CamposFromTarget(Vector3 p, Vector3 t, float d){    return Vector3.Normalize(p - t) * d + p;}

        public static float DefaultDist = 8f;
        public static float DefaultDist2 = 12f;

        Vector3[] _wayPoints = new Vector3[]
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

        #endregion

        #region resources

        private readonly GraphicsDeviceManager _Graphics;

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


        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        // these are the scene instances we create for every glTF model we want to render on screen.
        // Instances are designed to be as lightweight as possible, so it should not be a problem to
        // create as many of them as you need at runtime.
        private ModelInstance _Test;
        private ModelInstance _VertexColorTest;
        private ModelInstance _AlphaBlendModeTest;
        private ModelInstance _MultiUvTest;
        private ModelInstance _TextureCoordinateTest;
        private ModelInstance _NormalTangentMirrorTest;
        private ModelInstance _TextureSettingsTest;
        private ModelInstance _UnlitTest;
        private ModelInstance _InterpolationTest;
        //private ModelInstance _AnimatedMorphCube;
        private ModelInstance _TextureTransformMultiTest;
        private ModelInstance _TextureTransformTest;
        private ModelInstance _ClearCoatTest;
        private ModelInstance _SpecGlossVsMetalRough;


        #endregion

        #region lifecycle

        public Game2()
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            this.Window.Title = "SharpGLTF - MonoGame Scene";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;
            this.IsMouseVisible = true;

            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion



        #region content loading

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _texture = Content.Load<Texture2D>("MG_Logo_Small_exCanvs");

            _LightsAndFog = new PBREnvironment();

            LoadGenerateCpuSideEnvStuff();

            LoadPrimitives();

            SelectTestModel();

            LoadStandardTestingModels();

            SetupCamera();

            msg =
           " cube size " + _textureCubeMap.Size +
           " \n mip Level: " + TestValue1 + "  /  " + _textureCubeMap.LevelCount +
           " \n mip CallType: texCubeLod ";
        }

        public void SetupCamera()
        {
            _camera = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(2, 2, 10), new Vector3(0, 0, 0), Vector3.UnitY, 0.1f, 10000f, 1f, true, false);
            _camera.TransformCamera(_camera.World.Translation, _testTarget, _camera.World.Up);
            _camera.Up = Vector3.Up;
            _camera.WayPointCycleDurationInTotalSeconds = 50f;
            _camera.MovementSpeedPerSecond = 3f;
            _camera.SetWayPoints(_wayPoints, true, 200);
        }

        public void LoadGenerateCpuSideEnvStuff()
        {
            LoadIndivdualFaces();
            // This creates a new equaRectangularMap.
            //_generatedTexture = CubeMapHelper.GetEquaRectangularMapFromSixImageFaces(GraphicsDevice, 800, 400, _cmLeft, _cmBottom, _cmBack, _cmRight, _cmTop, _cmFront);
            _generatedTexture = CubeMapHelper.GetEquaRectangularMapFromSixImageFaces(GraphicsDevice, 2048, 1024, _cmLeft, _cmBottom, _cmBack, _cmRight, _cmTop, _cmFront);
            //_textureCubeMap = CubeMapHelper.SetIndividualFacesToCubeMap(GraphicsDevice, 256, _textureCubeMap, _cmLeft, _cmBottom, _cmBack, _cmRight, _cmTop, _cmFront);
            //_textureCubeMap = CubeMapHelper.SetIndividualFacesToCubeMap(GraphicsDevice, 2048, _textureCubeMap, _cmLeft, _cmBottom, _cmBack, _cmRight, _cmTop, _cmFront);

            _premadeLut = Content.Load<Texture2D>("ibl_brdf_lut"); // need to probably generate this instead of just loading a premade one.
            _ldrTexture = Content.Load<Texture2D>("ibl_ldr_generatedWater");
            //_ldrTexture = Content.Load<Texture2D>("ibl_ldr_gentestWaterLabeledFaces");
            //_ldrTexture = Content.Load<Texture2D>("ibl_ldr_generatedStPeters");
            //_ldrTexture = Content.Load<Texture2D>("ibl_ldr_radiance");

            _ldrTextureFaces = CubeMapHelper.GetMapFacesTextureArrayFromEquaRectangularMap(GraphicsDevice, _ldrTexture, 256); // this is sphereical map to a texture array.
            //_textureCubeMap = CubeMapHelper.GetCubeMapFromEquaRectangularMap(GraphicsDevice, _ldrTexture, 256);
            _textureCubeMap = CubeMapHelper.GetCubeMapFromEquaRectangularMap(GraphicsDevice, _ldrTexture, 2048);

            // need to generate the irradiance maps and mips and all that also.
            _LightsAndFog.SetEnviromentalCubeMap(_textureCubeMap);
            _LightsAndFog.SetEnviromentalLUTMap(_premadeLut);

            // save the created spherical map to disk.
            CubeMapHelper.SaveTexture2D(Path.Combine(Environment.CurrentDirectory, "ibl_ldr_generatedWater.png"), _generatedTexture);
        }

        public void LoadPrimitives()
        {
            font = new HardCodedSpriteFont().LoadHardCodeSpriteFont(GraphicsDevice);
            primitivesEffect = Content.Load<Effect>("MipLevelTestEffect");
            sky = new SpherePNTT(true, false, false, 10, 1000f, true, false);  // since i ussally map this using a left cross texture, i need the extra flip normal option on this time.
            cube = new SpherePNTT(false, false, false, 2, 1f);
            cube2 = new SpherePNTT(false, false, false, 5, 2f);
            primitivesEffect.Parameters["CubeMap"].SetValue(_textureCubeMap);
        }

        public void LoadIndivdualFaces()
        {
            //_cmLeft = Content.Load<Texture2D>("CubeFaces/_left256");
            //_cmRight = Content.Load<Texture2D>("CubeFaces/_right256");
            //_cmFront = Content.Load<Texture2D>("CubeFaces/_front256");
            //_cmBack = Content.Load<Texture2D>("CubeFaces/_back256");
            //_cmTop = Content.Load<Texture2D>("CubeFaces/_top256");
            //_cmBottom = Content.Load<Texture2D>("CubeFaces/_bottom256");

            _cmLeft = Content.Load<Texture2D>("CubeFaces/left");
            _cmRight = Content.Load<Texture2D>("CubeFaces/right");
            _cmFront = Content.Load<Texture2D>("CubeFaces/front");
            _cmBack = Content.Load<Texture2D>("CubeFaces/back");
            _cmTop = Content.Load<Texture2D>("CubeFaces/top");
            _cmBottom = Content.Load<Texture2D>("CubeFaces/bottom");

            //_cmLeft = Content.Load<Texture2D>("CubeFaces/negx");
            //_cmRight = Content.Load<Texture2D>("CubeFaces/posx");
            //_cmFront = Content.Load<Texture2D>("CubeFaces/posz");
            //_cmBack = Content.Load<Texture2D>("CubeFaces/negz");
            //_cmTop = Content.Load<Texture2D>("CubeFaces/posy");
            //_cmBottom = Content.Load<Texture2D>("CubeFaces/negy");
        }

        public void LoadStandardTestingModels()
        {
            ModelCollectionContent _load(string filePath)
            {
                return Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel(filePath, this.GraphicsDevice);
            }

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

        public void SelectTestModel()
        {

            ModelCollectionContent _load(string filePath)
            {
                return Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel(filePath, this.GraphicsDevice);
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
            _TestTemplate = _load("Models\\MetalRoughSpheresNoTextures.glb");        _targetScale = 500;

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

        protected override void UnloadContent()
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

        #endregion

        #region game loop




        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

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

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                _useDemoWaypoints = false;
            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                _useDemoWaypoints = true;

            _camera.Update(_testTarget, _useDemoWaypoints, gameTime);

            // test mip maps press the 1 key.
            if (Keyboard.GetState().IsKeyDown(Keys.D1) && Pause(gameTime))
                UpdateTestingUiShaderVariables(gameTime);

            base.Update(gameTime);
        }

        public void UpdateTestingUiShaderVariables(GameTime gameTime)
        {
                TestValue1++;
                if (TestValue1 > _textureCubeMap.LevelCount)
                    TestValue1 = 0;

                msg =
                    " cube size " + _textureCubeMap.Size +
                    " \n mip Level: " + TestValue1 + "  /  " + _textureCubeMap.LevelCount +
                    " \n mip CallType: texCubeLod ";
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // draw primitives

            DrawPrimitives(gameTime);

            // draw all the instances.

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;
            
            var dir = new Vector3((float)Math.Cos(animTime), 0, -(float)Math.Sin(animTime));
            _LightsAndFog.SetDirectLight(0, dir, Color.White, 1.0f);
            
            dir = Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1, 10, 5)), animTime).Right;
            _LightsAndFog.SetDirectLight(1, dir, Color.White, 1.0f);
            
            dir = Vector3.Normalize(new Vector3(1, 1, -25));
            _LightsAndFog.SetDirectLight(2, dir, Color.White, 1.0f);

            _LightsAndFog.SetTestingValue(TestValue1);

            //StackTraceToStringBuilder.Tracing = false;

            var ctx = new ModelDrawingContext(_Graphics.GraphicsDevice);

            ctx.SetCamera(_camera.World);

            // draw all the instances.            

            ctx.DrawSceneInstances
            (
                // environment lights and fog
                _LightsAndFog,
                // all model instances
                _Test, _VertexColorTest, _TextureCoordinateTest, _TextureSettingsTest, _MultiUvTest, _TextureTransformMultiTest, _TextureTransformTest, _AlphaBlendModeTest, _NormalTangentMirrorTest, _UnlitTest, _InterpolationTest, _ClearCoatTest, _SpecGlossVsMetalRough  /* , _AnimatedMorphCube */
             );

            _spriteBatch.Begin();

            _spriteBatch.Draw(_ldrTexture, new Rectangle(0, 0, 300, 150), Color.White);
            _spriteBatch.Draw(_generatedTexture, new Rectangle(350, 0, 300, 150), Color.White);

            int x = 0; int y = 150;
            _spriteBatch.Draw(_cmLeft, new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_cmBottom, new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_cmBack, new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_cmRight, new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_cmTop, new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_cmFront, new Rectangle(x, y, 100, 100), Color.White); x += 100;

            x = 0; y = 250;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_LEFT], new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_BOTTOM], new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_BACK], new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_RIGHT], new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_TOP], new Rectangle(x, y, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(_ldrTextureFaces[CubeMapHelper.FACE_FRONT], new Rectangle(x, y, 100, 100), Color.White); x += 100;

            _camera.DrawCurveThruWayPointsWithSpriteBatch(2f, new Vector3(100,100, 100), 1 ,gameTime);

            _spriteBatch.DrawString(font, msg, new Vector2(10, 20), Color.Red);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        protected void DrawPrimitives(GameTime gameTime)
        {
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            primitivesEffect.Parameters["View"].SetValue(_camera.View);   // just add defaults here or dont add anything.
            primitivesEffect.Parameters["CameraPosition"].SetValue( Vector3.Zero); //_camera.Position);
            primitivesEffect.Parameters["Projection"].SetValue(projectionMatrix);
            primitivesEffect.Parameters["testValue1"].SetValue((int)TestValue1);

            primitivesEffect.Parameters["World"].SetValue(Matrix.Identity);
            sky.Draw(GraphicsDevice, primitivesEffect, _textureCubeMap);

            //primitivesEffect.Parameters["World"].SetValue(Matrix.Identity);
            //cube.Draw(GraphicsDevice, primitivesEffect, _textureCubeMap);
            //cube2.Draw(GraphicsDevice, primitivesEffect, _textureCubeMap);

            //primitivesEffect.Parameters["testValue2"].SetValue((int)TestValue2);
            //effect.Parameters["TextureA"].SetValue(front);
            //sky.Draw(GraphicsDevice, primitivesEffect, cmFront, cmBack, cmLeft, cmRight, cmTop, cmBottom);
            //cube.Draw(GraphicsDevice, primitivesEffect, cmFront, cmBack, cmLeft, cmRight, cmTop, cmBottom);
            //cube2.Draw(GraphicsDevice, primitivesEffect, cmFront, cmBack, cmLeft, cmRight, cmTop, cmBottom);
        }

        void ReflectionRenderToSceneFaces(Vector3 reflectionCameraPosition, RenderTargetCube renderTargetReflectionCube)
        {
            Matrix view = new Matrix();
            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeX);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Left, Vector3.Up);
           // ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeY);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Down, Vector3.Backward);
           // ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.NegativeZ);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Forward, Vector3.Up);
           // ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveX);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Right, Vector3.Up);
          //  ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveY);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Up, Vector3.Forward);
           // ReflectionRenderScene();

            GraphicsDevice.SetRenderTarget(renderTargetReflectionCube, CubeMapFace.PositiveZ);
            view = CubeMapHelper.CreateAndSetCubeFaceView(reflectionCameraPosition, Vector3.Backward, Vector3.Up);
           // ReflectionRenderScene();
        }


        float pause = 0f;
        bool Pause(GameTime gametime)
        {
            if (pause < 0)
            {
                pause = .5f;
                return true;
            }
            else
            {
                pause -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }
    }

}

