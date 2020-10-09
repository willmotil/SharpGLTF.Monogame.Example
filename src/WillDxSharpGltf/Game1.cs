using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SharpGLTF.Runtime;

namespace WillDxSharpGltf
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region lifecycle

        //private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _texture;
        private Texture2D _generatedTexture;

        private Texture2D _ldrTexture;
        private Texture2D[] _ldrTextureFaces;

        private Texture2D _premadeLut;

        private TextureCube _textureCubeMap;
        private Texture2D cmLeft;
        private Texture2D cmRight;
        private Texture2D cmFront;
        private Texture2D cmBack;
        private Texture2D cmTop;
        private Texture2D cmBottom;

        public Game1()
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

            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        #region resources

        private readonly GraphicsDeviceManager _Graphics;

        // these are the actual hardware resources that represent every model's geometry.        

        ModelCollectionContent _AvodadoTemplate;
        ModelCollectionContent _BrainStemTemplate;
        ModelCollectionContent _CesiumManTemplate;
        ModelCollectionContent _HauntedHouseTemplate;
        ModelCollectionContent _SharkTemplate;

        #endregion

        #region content loading

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _texture = Content.Load<Texture2D>("MG_Logo_Small_exCanvs");

            _premadeLut = Content.Load<Texture2D>("ibl_brdf_lut"); // need to probably generate this instead of just loading a premade one.

            _ldrTexture = Content.Load<Texture2D>("ibl_ldr_radiance");

            //_ldrTextureFaces = CubeMapHelper.SphericalMapToTextureFaces(GraphicsDevice, 256, _ldrTexture); // prototype test need to fix this now that i see what i did wrong.
            _ldrTextureFaces = CubeMapHelper.GetMapFacesTextureArrayFromEquaRectangularMap(GraphicsDevice, _ldrTexture, 200); // this is sphereical map to a texture array.

            LoadIndivdualFaces();

           // _textureCubeMap = CubeMapHelper.SetIndividualFacesToCubeMap(GraphicsDevice, 256, _textureCubeMap, cmLeft, cmBottom, cmBack, cmRight, cmTop, cmFront);

            //// This needs work mip maps aren't handled correctly.
            _textureCubeMap = CubeMapHelper.GetCubeMapFromEquaRectangularMap(GraphicsDevice, _ldrTexture, 200);

            // This creates a new equaRectangularMap.
            _generatedTexture = CubeMapHelper.GetEquaRectangularMapFromSixImageFaces(GraphicsDevice, 400, 200, cmLeft, cmBottom, cmBack, cmRight, cmTop, cmFront);

            // need to generate the irradiance maps and mips and all that also.

            _LightsAndFog.SetEnviromentalCubeMap(_textureCubeMap);
            _LightsAndFog.SetEnviromentalLUTMap(_premadeLut);


            ModelCollectionContent _load(string filePath)
            {
                return Microsoft.Xna.Framework.Content.Pipeline.Graphics.FormatGLTF.LoadModel(filePath, this.GraphicsDevice);
            }

            _AvodadoTemplate = _load("Models\\Avocado.glb");
            _BrainStemTemplate = _load("Models\\BrainStem.glb");
            _CesiumManTemplate = _load("Models\\CesiumMan.glb");
            _HauntedHouseTemplate = _load("Models\\haunted_house.glb");
            _SharkTemplate = _load("Models\\shark.glb");
        }

        public void LoadIndivdualFaces()
        {
            cmLeft = Content.Load<Texture2D>("CubeFaces/_left256");
            cmRight = Content.Load<Texture2D>("CubeFaces/_right256");
            cmFront = Content.Load<Texture2D>("CubeFaces/_front256");
            cmBack = Content.Load<Texture2D>("CubeFaces/_back256");
            cmTop = Content.Load<Texture2D>("CubeFaces/_top256");
            cmBottom = Content.Load<Texture2D>("CubeFaces/_bottom256");
        }

        protected override void UnloadContent()
        {
            _AvodadoTemplate?.Dispose();
            _AvodadoTemplate = null;

            _BrainStemTemplate?.Dispose();
            _BrainStemTemplate = null;

            _CesiumManTemplate?.Dispose();
            _CesiumManTemplate = null;

            _HauntedHouseTemplate?.Dispose();
            _HauntedHouseTemplate = null;

            _SharkTemplate?.Dispose();
            _SharkTemplate = null;
        }

        #endregion

        #region game loop

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        // these are the scene instances we create for every glTF model we want to render on screen.
        // Instances are designed to be as lightweight as possible, so it should not be a problem to
        // create as many of them as you need at runtime.
        private ModelInstance _HauntedHouse;
        private ModelInstance _BrainStem;
        private ModelInstance _Avocado;
        private ModelInstance _CesiumMan1;
        private ModelInstance _CesiumMan2;
        private ModelInstance _CesiumMan3;
        private ModelInstance _CesiumMan4;
        private ModelInstance _Shark;

        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // create as many instances as we need from the templates

            if (_Avocado == null) _Avocado = _AvodadoTemplate.DefaultModel.CreateInstance();
            if (_HauntedHouse == null) _HauntedHouse = _HauntedHouseTemplate.DefaultModel.CreateInstance();
            if (_BrainStem == null) _BrainStem = _BrainStemTemplate.DefaultModel.CreateInstance();
            if (_CesiumMan1 == null) _CesiumMan1 = _CesiumManTemplate.DefaultModel.CreateInstance();
            if (_CesiumMan2 == null) _CesiumMan2 = _CesiumManTemplate.DefaultModel.CreateInstance();
            if (_CesiumMan3 == null) _CesiumMan3 = _CesiumManTemplate.DefaultModel.CreateInstance();
            if (_CesiumMan4 == null) _CesiumMan4 = _CesiumManTemplate.DefaultModel.CreateInstance();

            if (_Shark == null) _Shark = _SharkTemplate.DefaultModel.CreateInstance();

            // animate each instance individually.

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            _Avocado.WorldMatrix = Matrix.CreateScale(30) * Matrix.CreateRotationY(animTime * 0.3f) * Matrix.CreateTranslation(-4, 4, 1);
            _HauntedHouse.WorldMatrix = Matrix.CreateScale(20) * Matrix.CreateRotationY(1);

            _BrainStem.WorldMatrix = Matrix.CreateTranslation(0, 0.5f, 8);
            _BrainStem.Armature.SetAnimationFrame(0, 0.7f * animTime);

            _CesiumMan1.WorldMatrix = Matrix.CreateTranslation(-3, 0, 5);
            _CesiumMan1.Armature.SetAnimationFrame(0, 0.3f);

            _CesiumMan2.WorldMatrix = Matrix.CreateTranslation(-2, 0, 5);
            _CesiumMan2.Armature.SetAnimationFrame(0, 0.5f * animTime);

            _CesiumMan3.WorldMatrix = Matrix.CreateTranslation(2, 0, 5);
            _CesiumMan3.Armature.SetAnimationFrame(0, 1.0f * animTime);

            _CesiumMan4.WorldMatrix = Matrix.CreateTranslation(3, 0, 5);
            _CesiumMan4.Armature.SetAnimationFrame(0, 1.5f * animTime);

            _Shark.WorldMatrix = Matrix.CreateTranslation(5, 3, -6);
            _Shark.Armature.SetAnimationFrame(0, 1.0f * animTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            base.Draw(gameTime);

            // setup drawing context

            var animTime = (float)gameTime.TotalGameTime.TotalSeconds;

            var lookAt = new Vector3(0, 2, 0);
            var camPos = new Vector3((float)Math.Sin(animTime * 0.5f) * 2, 2, 12);
            var camera = Matrix.CreateWorld(camPos, lookAt - camPos, Vector3.UnitY);

            //var ctx = new MonoGameDrawingContext(_Graphics.GraphicsDevice);

            var ctx = new ModelDrawingContext(_Graphics.GraphicsDevice);

            ctx.SetCamera(camera);

            // draw all the instances.            

            ctx.DrawSceneInstances
                (
                _LightsAndFog, // environment lights and fog
                _Avocado, _HauntedHouse, _BrainStem, _CesiumMan1, _CesiumMan2, _CesiumMan3, _CesiumMan4, _Shark // all model instances
                );

            _spriteBatch.Begin();

            _spriteBatch.Draw(_texture, new Rectangle(0, 0, 100, 100), Color.White);

            int x = 0;
            _spriteBatch.Draw(cmLeft, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(cmRight, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(cmFront, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(cmBack, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(cmTop, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;
            _spriteBatch.Draw(cmBottom, new Rectangle(100 + x, 0, 100, 100), Color.White); x += 100;

            x = 0;
            _spriteBatch.Draw(_premadeLut, new Rectangle(0 + x, 100, 100, 100), Color.White); x += 100;

            for (int i = 0; i < _ldrTextureFaces.Length; i++)
            {
                _spriteBatch.Draw(_ldrTextureFaces[i], new Rectangle(0 + x, 100, 100, 100), Color.White);
                x += 100;
            }

            x = 0;
            _spriteBatch.Draw(_generatedTexture, new Rectangle(0 + x, 200, 400, 200), Color.White);


            _spriteBatch.End();
        }

        #endregion
    }
}

/* Hlsl stuff that is related gonna have to figure out how to fit this in as well as other stuff.
  
        // were to put this.

        #region cubemap for enviroment.

        private TextureCube _iblCubeMap;
        public void SetEnviromentalCubeMap(TextureCube iblCubeMap) => _iblCubeMap = iblCubeMap;

        //Parameters["u_LambertianEnvSampler"].SetValue(_iblCubeMap);  // not even sure i want to set it to this initially.

        #endregion


 TextureCube CubeMap;
 sampler CubeMapSampler = sampler_state
 {
     texture = <CubeMap>;
     magfilter = linear; /// set it to point ect
     minfilter = linear;
     mipfilter = linear;
     AddressU = clamp;
     AddressV = clamp;
     //AddressW = clamp;
 };

  // Placed after point lights.

 //____________________________________________________________________
 // Enviromental reflections and refraction.
 //____________________________________________________________________

 float maxMipLevel = 9;
 float mipLevel = roughness * maxMipLevel;
 float scaledRoughness = (roughness * 0.5f + 0.5f);
 float3 F = xfresnelSchlickRoughness(max(dot(N, V), 0.0f), F0, roughness);
 float3 InvF = (1.0f - F);
 float3 kS = F;
 float3 kD = InvF * dielectricity;
 float3 NR = N * roughness + R * smoothness;
 float3 irradiance = texCUBElod(CubeMapSampler, float4(NR, maxMipLevel * roughness)).rgb;
 float3 diffuse = (irradiance.r + irradiance.g + irradiance.b) * 0.3333f * albedo; //albedo * irradiance;
 float3 specular = irradiance * albedo; 
 float3 ambient = (kD * diffuse + kS * specular); // * occlusion;
 // add env light and point lights
 float3 outColor = ambient + Lo;


 // used to determine the inflection placement position from point a to a normal opposite the plane. \|/
 float3 InflectionPositionFromPlane(float3 anyPositionOnPlaneP, float3 theSurfaceNormalN, float3 theCameraPostionC)
 {
     // also gives the length when placed againsts a unit normal so any unit n * a distance is the distance to that normals plane no matter the normals direction. 
     float camToPlaneDist = dot(theSurfaceNormalN, theCameraPostionC - anyPositionOnPlaneP);
     return theCameraPostionC - theSurfaceNormalN * camToPlaneDist * 2;
 }

 // thanks to pumkin pudding for this function.
 float3 Function2dSphericalUvCoordinatesTo3dCubeMapNormal(float2 uvCoords)
 {
     float pi = 3.14159265358f;
     float3 v = float3(0.0f, 0.0f, 0.0f);
     float2 uv = uvCoords;
     uv *= float2(2.0f * pi, pi);
     float siny = sin(uv.y);
     v.x = -sin(uv.x) * siny;
     v.y = cos(uv.y);
     v.z = -cos(uv.x) * siny;
     return v;
 }

 // thanks to pumkin pudding for this function.
 float2 Function3dCubeMapNormalTo2dSphericalUvCoordinates(float3 normal)
 {
     const float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
     float2 uv = float2(atan2(normal.z, normal.x), asin(normal.y));
     uv *= INVERT_ATAN;
     uv += 0.5f;
     return uv;
 }

  */




//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;

//namespace WillDxSharpGltf
//{
//    public class Game1 : Game
//    {
//        private GraphicsDeviceManager _graphics;
//        private SpriteBatch _spriteBatch;

//        public Game1()
//        {
//            _graphics = new GraphicsDeviceManager(this);
//            Content.RootDirectory = "Content";
//            IsMouseVisible = true;
//        }

//        protected override void Initialize()
//        {
//            // TODO: Add your initialization logic here

//            base.Initialize();
//        }

//        protected override void LoadContent()
//        {
//            _spriteBatch = new SpriteBatch(GraphicsDevice);

//            // TODO: use this.Content to load your game content here
//        }

//        protected override void Update(GameTime gameTime)
//        {
//            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
//                Exit();

//            // TODO: Add your update logic here

//            base.Update(gameTime);
//        }

//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.CornflowerBlue);

//            // TODO: Add your drawing code here

//            base.Draw(gameTime);
//        }
//    }
//}
