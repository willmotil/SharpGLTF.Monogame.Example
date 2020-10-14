using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WillDxSharpGltf
{

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
        public static Effect primitivesEffect;
        public static Effect _hdrIblEffect;

        VertexPositionTexture[] screenQuad;
        RasterizerState rs_wireframe = new RasterizerState() { FillMode = FillMode.WireFrame };

        float TestValue1 = 0;
        float TestValue2 = 1;
        bool wireframe = false;
        string msg = "";

        private Texture2D _texture;
        private Texture2D _generatedTexture;

        DemoCamera _camera;
        bool _useDemoWaypoints = true;


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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = new HardCodedSpriteFont().LoadHardCodeSpriteFont(GraphicsDevice);
            _hdrIblEffect = Content.Load<Effect>("HdrIBLEffectTest");
            _texture = Content.Load<Texture2D>("hdr_01");
            Console.WriteLine($" hdri info  \n Format {_texture.Format} \n Bounds {_texture.Bounds}");

            _LightsAndFog = new PBREnvironment();

            _mts.LoadStandardTestingModels(GraphicsDevice);

            LoadPrimitives();

            SetupCamera();
        }

        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            _mts.UpdateModels(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                _useDemoWaypoints = false;
            if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                _useDemoWaypoints = true;

            _camera.Update(ModelTestSuiteExecution._testTarget, _useDemoWaypoints, gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.D3) && Pause(gameTime))
                _camera.TransformCamera(new Vector3(0, 0, 0), _camera.Forward, Vector3.Up);

            if (Keyboard.GetState().IsKeyDown(Keys.D4) && Pause(gameTime))
                wireframe = !wireframe;

            // test mip maps press the 1 key.
            if (Keyboard.GetState().IsKeyDown(Keys.D1) && Pause(gameTime))
                UpdateTestingUiShaderVariables(gameTime);

            base.Update(gameTime);
        }

        public void UpdateTestingUiShaderVariables(GameTime gameTime)
        {
            //TestValue1++;
            //if (TestValue1 > _textureCubeMapSpecular.LevelCount)
            //    TestValue1 = 0;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // draw primitives

            if (wireframe)
                GraphicsDevice.RasterizerState = rs_wireframe;

            DrawPrimitives(gameTime);

            // draw all the model instances.

            DrawModelInstances(gameTime);

            // draw regular spritebatches.

            DrawSpriteBatches(gameTime);

            base.Draw(gameTime);
        }

        protected void DrawPrimitives(GameTime gameTime)
        {
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            //primitivesEffect.Parameters["View"].SetValue(_camera.View);   // just add defaults here or dont add anything.
            //primitivesEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero); //_camera.Position);
            //primitivesEffect.Parameters["Projection"].SetValue(projectionMatrix);
            //primitivesEffect.Parameters["testValue1"].SetValue((int)TestValue1);
            //primitivesEffect.Parameters["World"].SetValue(Matrix.Identity);

            //hdrIblEffect

        }

        public void RenderScreenQuad(Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad, 0, 2);
            }
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









        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // .
        // . Move all this crap out of the way, this stuff isn't going to change much for this test.
        // .
        // .
        // .
        // .
        // .
        // .







        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_texture, new Rectangle(0, 0, 300, 300), Color.White);

            _camera.DrawCurveThruWayPointsWithSpriteBatch(2f, new Vector3(300, 100, 100), 1, gameTime);

            _spriteBatch.DrawString(_font, msg, new Vector2(10, 0), Color.Red);

            _spriteBatch.End();
        }

        public void DrawModelInstances(GameTime gameTime)
        {

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var dir = new Vector3((float)Math.Cos(animTime), 0, -(float)Math.Sin(animTime));
            _LightsAndFog.SetDirectLight(0, dir, Color.White, 1.0f);

            dir = Matrix.CreateFromAxisAngle(Vector3.Normalize(new Vector3(1, 10, 5)), animTime).Right;
            _LightsAndFog.SetDirectLight(1, dir, Color.White, 1.0f);

            dir = Vector3.Normalize(new Vector3(1, 1, -25));
            _LightsAndFog.SetDirectLight(2, dir, Color.White, 1.0f);

            _LightsAndFog.SetTestingValue(TestValue1);

            var ctx = new ModelDrawingContext(_Graphics.GraphicsDevice);

            ctx.SetCamera(_camera.World);

            // draw all the instances.            

            ctx.DrawSceneInstances
            (
                // environment lights and fog
                _LightsAndFog,
                // all model instances
                _mts._Test, _mts._VertexColorTest, _mts._TextureCoordinateTest, _mts._TextureSettingsTest, _mts._MultiUvTest, _mts._TextureTransformMultiTest, _mts._TextureTransformTest, _mts._AlphaBlendModeTest, _mts._NormalTangentMirrorTest, _mts._UnlitTest, _mts._InterpolationTest, _mts._ClearCoatTest, _mts._SpecGlossVsMetalRough  /* , _AnimatedMorphCube */
             );
        }

        protected override void UnloadContent()
        {
            _mts.UnloadContent();
        }

        public void LoadPrimitives()
        {
            CreateScreenQuad(GraphicsDevice.Viewport.Bounds);
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

        public void CreateScreenQuad(Rectangle ViewBounds)
        {
            screenQuad = new VertexPositionTexture[6];
            //
            if (GraphicsDevice.RasterizerState == RasterizerState.CullClockwise)
            {
                screenQuad[0] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Top, 0f), new Vector2(0f, 0f));  // p1
                screenQuad[1] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Bottom, 0f), new Vector2(0f, 1f)); // p0
                screenQuad[2] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Bottom, 0f), new Vector2(1f, 1f));// p3

                screenQuad[3] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Bottom, 0f), new Vector2(1f, 1f));// p3
                screenQuad[4] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Top, 0f), new Vector2(1f, 0f));// p2
                screenQuad[5] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Top, 0f), new Vector2(0f, 0f)); // p1
            }
            else
            {
                screenQuad[0] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Top, 0f), new Vector2(0f, 0f));  // p1
                screenQuad[2] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Bottom, 0f), new Vector2(0f, 1f)); // p0
                screenQuad[1] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Bottom, 0f), new Vector2(1f, 1f));// p3

                screenQuad[4] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Bottom, 0f), new Vector2(1f, 1f));// p3
                screenQuad[3] = new VertexPositionTexture(new Vector3(ViewBounds.Right, ViewBounds.Top, 0f), new Vector2(1f, 0f));// p2
                screenQuad[5] = new VertexPositionTexture(new Vector3(ViewBounds.Left, ViewBounds.Top, 0f), new Vector2(0f, 0f)); // p1
            }
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
