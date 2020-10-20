using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WillDxSharpGltf
{

    /*https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#appendix-a-tangent-space-recalculation
    Implemented...
    load the hdr map and gpu render capture it to a enviromental cubemap.


     TODO 
    Verify the enviroment is correctly set to the cubemap thru the rendertarget cube.  
    (For this i need a new primitive cube or to redo the old one just for this but id rather just have something simple for now id be nice to upgrade my old class but not right now).
    Depending on if i can create variable sized mip levels (doubtful).
    Create the whole thing either at once or step wise to a single rendertarget cube 
    
    otherwise do them seperately (this will probably be necesssary).
    
    (The below is the hard part it will also require i get the hammersly formula and a couple others, working on the shader and i may need the alternate non bit math version of which will probably need to be reworked).
    Create and set the mip levels to a Specular  pre-filter roughness reflection map.
    Create and set the mip levels to a Diffuse pre-filter global illumination map.
    
    The Lut To be honest ill need to do more research on the lut i really can't see much need for it as far as i can tell its a lot extra for little gain and its hacky but i need to read a bit more on it.
    Need to modify the shaders to handle the linear srgb conversion and tone mapping.
    Need to then test it against the primitive cube and add in the add hock material data to a appy basic texture to ensure the gltf algorithm looks right as the backface or lighting on the current shader is hosed.
    Once im fairly confident i have them lined up then i need to go into the pbr shader add this stuff and then find that bug.
    (might be better to just test a good chunk of this against a primitive once i get this far)
    Then i need to add the extra optional stuff which i can probably just do as i go as its pretty trivial and most of it just option stuff that can go in the enviromental class or a helper.
    Ill need a aligned view matrix to the current facing though for dynamic captures that's pretty trivial, though im not 100% these algorithms are properly aligned so that could be a bit tricky.
    arrrggg lots of stuff but a pbr shader is basically crippled without enviromental lighting.
     */

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game_HdrGpuSideTest : Game
    {
        private readonly GraphicsDeviceManager _Graphics;
        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        ModelTestSuiteExecution _mts = new ModelTestSuiteExecution();

        SpriteBatch _spriteBatch;
        SpriteFont _font;
        public static Effect _primitivesEffect;
        public static Effect _hdrIblEffect;

        DemoCamera _camera;
        bool _useDemoWaypoints = true;

        //SpherePNTT _skySphere;
        CubePrimitive skyCube = new CubePrimitive(100, false, false, true);
        CubePrimitive[] cubes = new CubePrimitive[5];

        VertexPositionTexture[] screenQuad;
        RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };

        float _mipLevelTestValue = 0;
        float TestValue2 = 0;
        bool _wireframe = false;
        string msg = "";

        private Texture2D _textureHdrEnvMap;
        private Texture2D _generatedTexture;
        private Texture2D _premadeLut;

        int _envMapToDraw = 0; // env =1,  envdif = 2, 

        TextureCube _textureCubeEnviroment;
        TextureCube _textureCubeIblDiffuseIllumination;
        TextureCube _textureCubeIblSpecularIllumination;

        public Game_HdrGpuSideTest()
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.Window.Title = "SharpGLTF - MonoGame Scene - Hdr Effects test";
            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;
            this.IsMouseVisible = true;
            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region content loading

        public void CreateIblCubeMaps()
        {
            Console.WriteLine($"\n Rendered to scene.");
            RenderToSceneFaces(_textureHdrEnvMap, ref _textureCubeEnviroment, "HdrToEnvCubeMap", true, true, 512);
            RenderToSceneFaces(_textureCubeEnviroment, ref _textureCubeIblDiffuseIllumination, "EnvCubemapToDiffuseIlluminationCubeMap", false, true, 128);
        }

        protected override void LoadContent()
        {
            // https://hdrihaven.com    https://texturehaven.com
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = new HardCodedSpriteFont().LoadHardCodeSpriteFont(GraphicsDevice);
            _primitivesEffect = Content.Load<Effect>("MipLevelTestEffect");
            _hdrIblEffect = Content.Load<Effect>("HdrIBLEffectTest");

            _premadeLut = Content.Load<Texture2D>("ibl_brdf_lut");
            _textureHdrEnvMap = Content.Load<Texture2D>("colorful_studio_2k");   //("hdr_01");

            Console.WriteLine($" hdri info  \n Format {_textureHdrEnvMap.Format} \n Bounds {_textureHdrEnvMap.Bounds}   IlluminationMapSampleSize {FigureOutSampleSize()} ");

            _mts.LoadStandardTestingModels(GraphicsDevice);

            LoadPrimitives();

            SetupCamera();

            CreateIblCubeMaps();

            _LightsAndFog = new PBREnvironment();
            _LightsAndFog.SetEnviromentalCubeMap(_textureCubeIblDiffuseIllumination); //_textureCubeEnviroment _textureCubeIblDiffuseIllumination _textureCubeIblSpecularIllumination
            _LightsAndFog.SetEnviromentalLUTMap(_premadeLut);
        }

        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && Pause(gameTime))
                _useDemoWaypoints = ! _useDemoWaypoints;

            if (Keyboard.GetState().IsKeyDown(Keys.D1) && Pause(gameTime))
                _envMapToDraw = 1;

            if (Keyboard.GetState().IsKeyDown(Keys.D2) && Pause(gameTime))
                _envMapToDraw = 2;

            if (Keyboard.GetState().IsKeyDown(Keys.D4) && Pause(gameTime))
                _wireframe = !_wireframe;

            if (Keyboard.GetState().IsKeyDown(Keys.D5) && Pause(gameTime))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                CreateIblCubeMaps();
                stopwatch.Stop();
                Console.WriteLine($" Time elapsed: { stopwatch.Elapsed.TotalMilliseconds}ms   {stopwatch.Elapsed.TotalSeconds}sec ");
            }

            // test mip maps press the 1 key.
            if (Keyboard.GetState().IsKeyDown(Keys.F1) && Pause(gameTime))
                UpdateTestingUiShaderVariables(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.F2) && Pause(gameTime))
                Console.WriteLine("numbOfSamples: " + FigureOutSampleSize() );

            if (Keyboard.GetState().IsKeyDown(Keys.F3) && Pause(gameTime))
                _camera.TransformCamera(new Vector3(0, 0, 0), _camera.Forward, Vector3.Up);

            _camera.Update(ModelTestSuiteExecution._testTarget, _useDemoWaypoints, gameTime);

            _mts.UpdateModels(gameTime);

            msg = $"Cullmode {GraphicsDevice.RasterizerState.CullMode}  \n Camera.Forward  { _camera.Forward }  \n _mipLevelTestValue {_mipLevelTestValue} ";

            base.Update(gameTime);
        }

        public int FigureOutSampleSize()
        {
            float ToRadians = 0.0174532925f;
            float numberOfSamplesHemisphere = 10;//6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
            float numberOfSamplesAround = 16;//10.0f;
            float hemisphereMaxAngle = 45.0f;
            float minimumAdjustment = 2.1f;
            float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // 30;  1.57f // hemisphere
            float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //2.5f * ToRadians;  // 2.5f     // y dist
            //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //2.85f * ToRadians; // 2.85f  // z roll

            var normal = new Vector3(0, 0, 1);

            Vector3 up = new Vector3(normal.Z, normal.Z, normal.Z);// float3(0,1,0);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, normal));
            up = Vector3.Cross(normal, right);

            Console.WriteLine($"\n normal: {normal}");

            int numbOfSamples = 0;
            for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y 
            {
                float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
                float stepPhi = (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut; // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
                if (stepPhi > minimumAdjustment)
                    stepPhi = minimumAdjustment;
                int numbOfSamplesThisPass = 0;

                Console.WriteLine($"\n  theta:{theta * 57.295779513f} ");
                for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
                {
                    Vector3 temp = ((float)Math.Cos(phi) * right + (float)Math.Sin(phi) * up);
                    Vector3 sampleVector = ((float)Math.Cos(theta) * normal + (float)Math.Sin (theta) * temp);

                    numbOfSamples++;
                    numbOfSamplesThisPass++;

                    Console.WriteLine($"   phi:{phi * 57.295779513f}   sampleVector: { sampleVector}");
                }
                Console.WriteLine($"  numbOfSamplesThisPass: {numbOfSamplesThisPass} theta degree span: {theta * 57.295779513f *2f} ");
            }
            Console.WriteLine($"  numbOfSamples: {numbOfSamples} ");
            return numbOfSamples;
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // draw primitives

            if (_wireframe)
                GraphicsDevice.RasterizerState = rs_wireframe;

            DrawPrimitives(gameTime);

            // draw all the model instances.

            DrawModelInstances(gameTime);

            // draw regular spritebatches.

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        // K now i need a new primitive cube to verify that the env is set up right.
        protected void DrawPrimitives(GameTime gameTime)
        {
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            _primitivesEffect.Parameters["Projection"].SetValue(projectionMatrix);
            _primitivesEffect.Parameters["View"].SetValue(_camera.View);   // just add defaults here or dont add anything.
            _primitivesEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero); //_camera.Position);
            _primitivesEffect.Parameters["testValue1"].SetValue((int)_mipLevelTestValue);
            //_primitivesEffect.Parameters["World"].SetValue(Matrix.Identity);



            _primitivesEffect.Parameters["World"].SetValue(_camera.World);
            if (_envMapToDraw == 1)
                skyCube.DrawPrimitiveCube(GraphicsDevice, _primitivesEffect, _textureCubeEnviroment);
            else
                skyCube.DrawPrimitiveCube(GraphicsDevice, _primitivesEffect, _textureCubeIblDiffuseIllumination);


            _primitivesEffect.Parameters["Projection"].SetValue(_camera.Projection);

            for (int i = 0; i < 5;  i++)
            {
                _primitivesEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(i *2 + 70, i * 2, i * -2.5f + -8)));
                if (_envMapToDraw == 1)
                    cubes[i].DrawPrimitiveCube(GraphicsDevice, _primitivesEffect, _textureCubeEnviroment);
                else
                    cubes[i].DrawPrimitiveCube(GraphicsDevice, _primitivesEffect, _textureCubeIblDiffuseIllumination);
            }


            //if (_envMapToDraw == 0)
            //   _skySphere.Draw(GraphicsDevice, _primitivesEffect, _textureCubeEnviroment);
            //else
            //   _skySphere.Draw(GraphicsDevice, _primitivesEffect, _textureCubeIblDiffuseIllumination);
        }

        public void DrawModelInstances(GameTime gameTime)
        {

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var dir = new Vector3((float)Math.Cos(animTime), 0, -(float)Math.Sin(animTime));
            _LightsAndFog.SetDirectLight(0, -dir, Color.White, 1.0f);

            dir = Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1, 10, 5)), animTime).Right;
            _LightsAndFog.SetDirectLight(1, -dir, Color.White, 1.0f);

            dir = Vector3.Normalize(new Vector3(1, 1, -25));
            _LightsAndFog.SetDirectLight(2, -dir, Color.White, 1.0f);

            _LightsAndFog.SetTestingValue(_mipLevelTestValue);

            if (_envMapToDraw == 1)
                _LightsAndFog.SetEnviromentalCubeMap(_textureCubeEnviroment);
            else
                _LightsAndFog.SetEnviromentalCubeMap(_textureCubeIblDiffuseIllumination);

            var ctx = new ModelDrawingContext(_Graphics.GraphicsDevice);

            ctx.SetCamera(_camera.World);
            ctx.SetProjectionMatrix(_camera.Projection);

            // draw all the instances.            

            ctx.DrawSceneInstances
            (
                // environment lights and fog
                _LightsAndFog,
                // all model instances
                _mts._Test, 
                _mts._VertexColorTest, 
                _mts._TextureCoordinateTest, 
                _mts._TextureSettingsTest, 
                _mts._MultiUvTest, 
                _mts._TextureTransformTest, 
                _mts._AlphaBlendModeTest, 
                _mts._NormalTangentMirrorTest, 
                _mts._UnlitTest, 
                _mts._InterpolationTest, 
                _mts._SpecGlossVsMetalRough
             /*                 _mts._ClearCoatTest , _mts._TextureTransformMultiTest , _AnimatedMorphCube , _mts._ClearCoatTest */
             );
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_textureHdrEnvMap, new Rectangle(0, 0, 200, 100), Color.White);

            _camera.DrawCurveThruWayPointsWithSpriteBatch(2f, new Vector3(300, 100, 100), 1, gameTime);

            _spriteBatch.DrawString(_font, msg, new Vector2(10, 0), Color.Red);

            _spriteBatch.End();
        }

        /// <summary>
        /// The ref was used to pass the ref variable directly thru here not a ref copy i guess.
        /// </summary>
        void RenderToSceneFaces(Texture2D sourceHdrLdrEquaRectangularMap, ref TextureCube textureCubeDestinationMap, string Technique, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            var renderTargetCube = new RenderTargetCube(GraphicsDevice, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrIblEffect.CurrentTechnique = _hdrIblEffect.Techniques[Technique];
            _hdrIblEffect.Parameters["Texture"].SetValue(sourceHdrLdrEquaRectangularMap);
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case (int)CubeMapFace.NegativeX: // FACE_LEFT
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                    case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        break;
                    case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        break;
                    case (int)CubeMapFace.PositiveZ: // FACE_BACK
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        break;

                    case (int)CubeMapFace.PositiveY: // FACE_TOP
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        break;
                    case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        break;
                }
                _hdrIblEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrIblEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube; // set the render to the specified texture cube.
            GraphicsDevice.SetRenderTarget(null);

            Console.WriteLine($" SphericalTex2D as source ... Technique: {Technique}  resulting TextureCube.Format: {textureCubeDestinationMap.Format}  Mip LevelCount: {textureCubeDestinationMap.LevelCount}");
        }

        /// <summary>
        /// The ref was used to pass the ref variable directly thru here not a ref copy i guess.
        /// </summary>
        void RenderToSceneFaces(TextureCube sourceHdrLdrEnvMap, ref TextureCube textureCubeDestinationMap, string Technique, bool generateMips, bool useHdrFormat, int sizeSquarePerFace)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            var pixelformat = SurfaceFormat.Color;
            if (useHdrFormat)
                pixelformat = SurfaceFormat.Vector4;
            var renderTargetCube = new RenderTargetCube(GraphicsDevice, sizeSquarePerFace, generateMips, pixelformat, DepthFormat.None);
            _hdrIblEffect.CurrentTechnique = _hdrIblEffect.Techniques[Technique];
            _hdrIblEffect.Parameters["CubeMap"].SetValue(sourceHdrLdrEnvMap);
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case (int)CubeMapFace.NegativeX: // FACE_LEFT
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue( CubePrimitive.matrixNegativeX );
                        break;
                    case (int)CubeMapFace.NegativeZ: // FACE_FORWARD
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue(CubePrimitive.matrixNegativeZ);
                        break;
                    case (int)CubeMapFace.PositiveX: // FACE_RIGHT
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue(CubePrimitive.matrixPositiveX);
                        break;
                    case (int)CubeMapFace.PositiveZ: // FACE_BACK
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue(CubePrimitive.matrixPositiveZ);
                        break;

                    case (int)CubeMapFace.PositiveY: // FACE_TOP
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue(CubePrimitive.matrixPositiveY);
                        break; 
                    case (int)CubeMapFace.NegativeY: // FACE_BOTTOM
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        _hdrIblEffect.Parameters["CubeMapFaceNormal"].SetValue(CubePrimitive.matrixNegativeY);
                        break;

                        //default:
                        //    GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        //    break;
                }
                _hdrIblEffect.Parameters["FaceToMap"].SetValue(i); // render screenquad to face.
                foreach (EffectPass pass in _hdrIblEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad, 0, 2);
                }
            }
            textureCubeDestinationMap = renderTargetCube; // set the render to the specified texture cube.
            GraphicsDevice.SetRenderTarget(null);

            Console.WriteLine($" Cubemap as source ... Technique: {Technique}  resulting TextureCube.Format: {textureCubeDestinationMap.Format}  Mip LevelCount: {textureCubeDestinationMap.LevelCount}");
        }








        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // .
        // . Move all this junk out of the way so i can see what im doing, this stuff isn't going to change much for this test.
        // .
        // .
        // .
        // .
        // .
        // .


        public void UpdateTestingUiShaderVariables(GameTime gameTime)
        {
            _mipLevelTestValue++;
            if (_mipLevelTestValue > _textureCubeEnviroment.LevelCount)
                _mipLevelTestValue = 0;
        }

        public void LoadPrimitives()
        {
            CreateScreenQuad();
            for (int i = 0; i < 5; i++)
                cubes[i] = new CubePrimitive(1, true, false, true);
            //_skySphere = new SpherePNTT(true, false, false, 25, 1000, true, false);
        }

        protected override void UnloadContent()
        {
            _mts.UnloadContent();
        }

        public void SetupCamera()
        {
            _camera = new DemoCamera(GraphicsDevice, _spriteBatch, null, new Vector3(2, 2, 10), new Vector3(0, 0, 0), Vector3.UnitY, 0.1f, 10000f, 1f, true, false);
            _camera.TransformCamera(_camera.World.Translation, ModelTestSuiteExecution._testTarget, _camera.World.Up);
            _camera.Up = Vector3.Up;
            _camera.WayPointCycleDurationInTotalSeconds = 50f;
            _camera.MovementSpeedPerSecond = 3f;
            _camera.SetWayPoints(_mts._wayPoints, true, 200);
        }

        public void CreateScreenQuad()
        {
            var r = new Rectangle(-1, -1, 2, 2);
            screenQuad = new VertexPositionTexture[6];
            //
            if (GraphicsDevice.RasterizerState == RasterizerState.CullClockwise)
            {
                screenQuad[0] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f));  // p1
                screenQuad[1] = new VertexPositionTexture(new Vector3(r.Left, r.Bottom, 0f), new Vector2(0f, 1f)); // p0
                screenQuad[2] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3

                screenQuad[3] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3
                screenQuad[4] = new VertexPositionTexture(new Vector3(r.Right, r.Top, 0f), new Vector2(1f, 0f));// p2
                screenQuad[5] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f)); // p1
            }
            else
            {
                screenQuad[0] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f));  // p1
                screenQuad[2] = new VertexPositionTexture(new Vector3(r.Left, r.Bottom, 0f), new Vector2(0f, 1f)); // p0
                screenQuad[1] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3

                screenQuad[4] = new VertexPositionTexture(new Vector3(r.Right, r.Bottom, 0f), new Vector2(1f, 1f));// p3
                screenQuad[3] = new VertexPositionTexture(new Vector3(r.Right, r.Top, 0f), new Vector2(1f, 0f));// p2
                screenQuad[5] = new VertexPositionTexture(new Vector3(r.Left, r.Top, 0f), new Vector2(0f, 0f)); // p1
            }
        }

        float pause = 0f;
        bool Pause(GameTime gametime)
        {
            if (pause < 0)
            {
                pause = .2f;
                return true;
            }
            else
            {
                pause -= (float)gametime.ElapsedGameTime.TotalSeconds;
                return false;
            }
        }

    }

    public class BasicFps
    {
        private double frames = 0;
        private double updates = 0;
        private double elapsed = 0;
        private double last = 0;
        private double now = 0;
        public double msgFrequency = 1.0f;
        public string msg = "";

        /// <summary>
        /// The msgFrequency here is the reporting time to update the message.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Short Answer can do it like this time elapsed per frame. 
            // This is delta time to keep things straight for movement.
            // elapsedThisFrame = (double)(gameTime.ElapsedGameTime.TotalSeconds); 

            // You can do this if your adding up elapsed time to do something like a timer or countdown or up till you display a msg.
            // elapsedTimeCumulative += (double)(gameTime.ElapsedGameTime.TotalSeconds); 

            // I do this just because i usually want to get the time now as well.
            now = gameTime.TotalGameTime.TotalSeconds;
            elapsed = (double)(now - last);
            if (elapsed > msgFrequency)
            {
                msg = " Fps: " + (frames / elapsed).ToString() + "\n Elapsed time: " + elapsed.ToString() + "\n Updates: " + updates.ToString() + "\n Frames: " + frames.ToString();
                elapsed = 0;
                frames = 0;
                updates = 0;
                last = now;
            }
            updates++;
        }

        public void DrawFps(SpriteBatch spriteBatch, SpriteFont font, Vector2 fpsDisplayPosition, Color fpsTextColor)
        {
            spriteBatch.DrawString(font, msg, fpsDisplayPosition, fpsTextColor);
            frames++;
        }
    }
}
