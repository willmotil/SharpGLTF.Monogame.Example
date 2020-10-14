using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WillDxSharpGltf
{

    /*
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

        RenderTargetCube renderTargetCubeEnviroment;
        TextureCube textureCubeEnviroment;

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

            renderTargetCubeEnviroment = new RenderTargetCube(GraphicsDevice, 256, true, SurfaceFormat.Vector4, DepthFormat.None);

            _LightsAndFog = new PBREnvironment();

            _mts.LoadStandardTestingModels(GraphicsDevice);

            LoadPrimitives();

            SetupCamera();

            CreateEnviromentalCubeMap();
        }

        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && Pause(gameTime))
                _useDemoWaypoints = ! _useDemoWaypoints;

            if (Keyboard.GetState().IsKeyDown(Keys.D3) && Pause(gameTime))
                _camera.TransformCamera(new Vector3(0, 0, 0), _camera.Forward, Vector3.Up);

            if (Keyboard.GetState().IsKeyDown(Keys.D4) && Pause(gameTime))
                wireframe = !wireframe;

            if (Keyboard.GetState().IsKeyDown(Keys.D5) && Pause(gameTime))
                CreateEnviromentalCubeMap();

            // test mip maps press the 1 key.
            if (Keyboard.GetState().IsKeyDown(Keys.D1) && Pause(gameTime))
                UpdateTestingUiShaderVariables(gameTime);

            _camera.Update(ModelTestSuiteExecution._testTarget, _useDemoWaypoints, gameTime);

            _mts.UpdateModels(gameTime);

            base.Update(gameTime);
        }

        public void CreateEnviromentalCubeMap()
        {
            RenderToSceneFaces(_hdrIblEffect, renderTargetCubeEnviroment);
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

        void RenderToSceneFaces(Effect effect, RenderTargetCube renderTargetCube)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int i = 0; i < 6; i++) 
            {
                switch (i)
                {
                    case 2: // FACE_BACK:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                    case 4: //FACE_TOP:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeY);
                        break;
                    case 0: //FACE_LEFT:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeZ);
                        break;
                    case 5: //FACE_FRONT:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveX);
                        break;
                    case 1: //FACE_BOTTOM:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveY);
                        break;
                    case 3: //FACE_RIGHT:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.PositiveZ);
                        break;
                    default:
                        GraphicsDevice.SetRenderTarget(renderTargetCube, CubeMapFace.NegativeX);
                        break;
                }
                // render screenquad to face.
                _hdrIblEffect.Parameters["Texture"].SetValue(_texture);
                _hdrIblEffect.Parameters["FaceToMap"].SetValue(i);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, screenQuad, 0, 2);
                }
            }
            textureCubeEnviroment = (TextureCube)renderTargetCube;

            GraphicsDevice.SetRenderTarget(null);

            Console.WriteLine($" Rendered to scene.  textureCubeEnviroment.Format {textureCubeEnviroment.Format}  textureCubeEnviroment.LevelCount {textureCubeEnviroment.LevelCount}" );
        }

        // K now i need a new primitive cube to verify that the env is set up right.
        protected void DrawPrimitives(GameTime gameTime)
        {
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(90 * (float)((3.14159265358f) / 180f), GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height, 0.01f, 1000f);
            //primitivesEffect.Parameters["View"].SetValue(_camera.View);   // just add defaults here or dont add anything.
            //primitivesEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero); //_camera.Position);
            //primitivesEffect.Parameters["Projection"].SetValue(projectionMatrix);
            //primitivesEffect.Parameters["testValue1"].SetValue((int)TestValue1);
            //primitivesEffect.Parameters["World"].SetValue(Matrix.Identity);
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
            //TestValue1++;
            //if (TestValue1 > _textureCubeMapSpecular.LevelCount)
            //    TestValue1 = 0;
        }

        public void DrawSpriteBatches(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_texture, new Rectangle(0, 0, 200, 100), Color.White);

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
            CreateScreenQuad();
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
