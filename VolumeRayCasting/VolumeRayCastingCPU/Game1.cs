#undef WRITE_AVI

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;
using RayTracer;
using AviAccess;

namespace VolumeRayCasting
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        WaterBody waterBody;
        Volume volume;

        readonly Vector3 cameraPos = new Vector3(75f, 50f, 75f);
        readonly Vector3 cameraTarget = new Vector3(0f, -25f, 0f);

        readonly Vector4 ambientLight = new Vector4(.2f, .2f, .2f, 1f);

        RTManager rayTracer;
#if WRITE_AVI
        AviWriter aviWriter;
#endif

#if DEBUG
        double fps;
        int frameCount;
        int totalFrames = 0;
        const double SAMPLE_TIME_FRAME = 1f;
        double sampleTime;
        SpriteFont font;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rayTracer = new RTManager(this);

            rayTracer.CameraPosition = cameraPos;
            rayTracer.CameraTarget = cameraTarget;

            rayTracer.NearPlaneDistance = 0.1f;
            rayTracer.FarPlaneDistance = 1000f;

            rayTracer.RecursionDepth = 2;

            this.Components.Add(rayTracer);
#if WRITE_AVI
            int i = 0;
            while (System.IO.File.Exists("Render_" + i + ".avi")) { ++i; }

            aviWriter = new AviWriter(this, "Render_" + i + ".avi");

            this.Components.Add(aviWriter);
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitializeWorld();
            InitializeLighting();

            base.Initialize();
        }

        private void InitializeWorld()
        {
            waterBody = new WaterBody();
            
            volume = new Volume(waterBody);
            Material mw = new Material();
            mw.AmbientStrength = 0f;
            mw.DiffuseStrength = 1f;
            mw.SpecularStrength = 0.75f;
            //mw.Transparency = 0.5f;
            mw.Exponent = 32;
            mw.setAmbientColor(Color.RoyalBlue.ToVector4());
            mw.setDiffuseColor(Color.SkyBlue.ToVector4());
            mw.setSpecularColor(Color.Azure.ToVector4());
            volume.Material1 = mw;

            rayTracer.WorldObjects.Add(volume);

            //RayTraceable floor = new Quad(new Vector3(8, 0, 16), new Vector3(-8, 0, -16), new Vector3(8, 0, -16), new Vector3(-8, 0, -16));
            RayTraceable floor = new Quad(
                new Vector3(waterBody.PositionMax.X, waterBody.PositionMin.Y, waterBody.PositionMax.Z), 
                waterBody.PositionMin, 
                new Vector3(waterBody.PositionMax.X, waterBody.PositionMin.Y, waterBody.PositionMin.Z),
                waterBody.PositionMin
                );
            Material floorMat = new MaterialCheckered();
            //Material floorMat = new MaterialCircleGradient(.5f, Color.White.ToVector4(), Color.Green.ToVector4());
            //Material floorMat = new MaterialBitmap((System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"mtgcard.jpg"));
            floorMat.AmbientStrength = 1f;
            floorMat.DiffuseStrength = 1f;
            floor.Material1 = floorMat;
            floor.MaxU = 10;
            floor.MaxV = 10;

            rayTracer.WorldObjects.Add(floor);
        }

        private void InitializeLighting()
        {
            rayTracer.AmbientLight = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            Light l1 = new Light();
            l1.LightColor = new Vector4(1f, 1f, 1f, 1f);
            l1.Position = new Vector3(100f, 100f, 100f);
            rayTracer.Lights.Add(l1);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
#if DEBUG
            font = Content.Load<SpriteFont>(@"font");
#endif
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        
#if DEBUG
        double waterTime;
        Stopwatch sw = new Stopwatch();
#endif

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Z))
            {
                volume.Paused = true;
                rayTracer.Visible = false;
            }

            if (keyboard.IsKeyDown(Keys.X))
            {
                volume.Paused = false;
                rayTracer.Visible = true;
            }

#if DEBUG
            sampleTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (sampleTime >= SAMPLE_TIME_FRAME)
            {
                fps = sampleTime / frameCount;
                sampleTime = 0;
                frameCount = 0;
            }
#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            sw.Reset();
            sw.Start();
            waterBody.Update();
            sw.Stop();
            waterTime = sw.Elapsed.TotalSeconds;
            volume.Update();

#if DEBUG
            ++frameCount;
            ++totalFrames;
#endif

            base.Draw(gameTime);

#if DEBUG
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Raytracer Time: " + rayTracer.DrawTime, Vector2.Zero, Color.White);
            spriteBatch.End();
#endif

#if WRITE_AVI
            if (totalFrames > 300)
            {
                aviWriter.Close();
                Exit();
            }
#endif
        }
        
    }
}
