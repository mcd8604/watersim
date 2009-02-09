using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RayTracer;
using AviAccess;

namespace RayTracePolygonizerDemo
{
    class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        RTManager rayTracer;
        RayTraceable floor;
        WaterBody waterbody;
        Polygonizer polygonizer;

        Vector3 camPosition = new Vector3(120, 40, 120);
        Vector3 camTarget = new Vector3(0, -40, 0);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            InitializeRayTracer();

            waterbody = new WaterBody();
            polygonizer = new Polygonizer(waterbody);

            Components.Add(rayTracer);

            //AviWriter aviWriter = new AviWriter(this, "test.avi");
            //Components.Add(aviWriter);
        }

        private void InitializeRayTracer()
        {
            rayTracer = new RayTracer.RTManager(this);

            rayTracer.NearPlaneDistance = 0.1f;
            rayTracer.FarPlaneDistance = 100.0f;

            rayTracer.CameraPosition = camPosition;
            rayTracer.CameraTarget = camTarget;

            rayTracer.RecursionDepth = 4;

            rayTracer.BackgroundColor = Color.CornflowerBlue.ToVector4();
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
            InitializeLighting();
            InitializeWorld();

            base.Initialize();
        }

        private void InitializeLighting()
        {
            Light l1 = new Light();
            l1.LightColor = new Vector4(1f, 1f, 1f, 1f);
            l1.Position = new Vector3(5f, 8f, 15f);
            rayTracer.Lights.Add(l1);
        }

        private void InitializeWorld()
        {
            floor = new Quad(
                new Vector3(waterbody.PositionMax.X, waterbody.PositionMin.Y, waterbody.PositionMax.Z), 
                new Vector3(waterbody.PositionMin.X, waterbody.PositionMin.Y, waterbody.PositionMin.Z), 
                new Vector3(waterbody.PositionMax.X, waterbody.PositionMin.Y, waterbody.PositionMin.Z), 
                new Vector3(waterbody.PositionMax.X, waterbody.PositionMin.Y, waterbody.PositionMin.Z)
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
            
            Material water = new Material();
            water.AmbientStrength = 0.075f;
            water.DiffuseStrength = 0.075f;
            water.SpecularStrength = 0.2f;
            water.Exponent = 20;
            water.setAmbientColor(new Vector4(1f, 1f, 1f, 1f));
            water.setDiffuseColor(new Vector4(1f, 1f, 1f, 1f));
            water.setSpecularColor(Vector4.One);
            water.ReflectionCoef = .2f;
            water.Transparency = .99f;
            water.RefractionIndex = .99f;
            polygonizer.Material1 = water;

            rayTracer.WorldObjects.Add(polygonizer);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("font");
        }

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

            // TODO: Add your update logic here   

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            waterbody.Update();
            polygonizer.Update();

            base.Draw(gameTime);
#if DEBUG
            //spriteBatch.Begin();
            //spriteBatch.DrawString(font, "Grid Time: " + polygonizer.GridTime, Vector2.Zero, Color.White);
            //spriteBatch.DrawString(font, "Poly Time: " + polygonizer.PolyTime, new Vector2(0, 24), Color.White);
            //spriteBatch.DrawString(font, "Raytrace Time: " + rayTracer.RayTime, new Vector2(0, 48), Color.White);
            //spriteBatch.End();
#endif
        }

    }

}
