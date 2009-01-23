using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RayTracer;

namespace WaterPolygonizerDemo
{
    class Game2 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        WaterBody waterbody;
        Polygonizer polygonizer;

        RTManager rayTracer;

        public Game2()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rayTracer = new RTManager(this);

            rayTracer.CameraPosition = new Vector3(30, 10, 30);
            rayTracer.CameraTarget = new Vector3(0, -10, 0);

            rayTracer.NearPlaneDistance = 0.001f;
            rayTracer.FarPlaneDistance = 1000f;

            Components.Add(rayTracer);

            waterbody = new WaterBody();
            polygonizer = new Polygonizer(waterbody);
        }

        protected override void Initialize()
        {
            InitializeWorld();
            InitializeLights();

            base.Initialize();
        }

        private void InitializeWorld()
        {
            Quad q = new Quad(
                new Vector3(waterbody.PositionMin.X, waterbody.PositionMin.Y, waterbody.PositionMin.Z),
                new Vector3(waterbody.PositionMin.X, waterbody.PositionMin.Y, waterbody.PositionMax.Z),
                new Vector3(waterbody.PositionMax.X, waterbody.PositionMin.Y, waterbody.PositionMax.Z),
                new Vector3(waterbody.PositionMax.X, waterbody.PositionMin.Y, waterbody.PositionMin.Z));
            Material qMat = new Material();
            qMat.AmbientStrength = 1f;
            qMat.DiffuseStrength = 1f;
            qMat.setAmbientColor(new Vector4(1f, 0f, 0f, 1f));
            qMat.setDiffuseColor(new Vector4(1f, 0f, 0f, 1f));
            q.Material1 = qMat;

            rayTracer.RayTraceables.Add(q);

            Material water = new Material();
            water.AmbientStrength = 1f;
            water.DiffuseStrength = 1f;
            water.setAmbientColor(new Vector4(.1f, .1f, .1f, .1f));
            water.setDiffuseColor(new Vector4(.1f, .2f, .7f, .4f));
            polygonizer.Material1 = water;
            rayTracer.RayTraceables.Add(polygonizer);
        }

        private void InitializeLights()
        {
            Light l1 = new Light();
            l1.LightColor = new Vector4(1f, 1f, 1f, 1f);
            l1.Position = new Vector3(5f, 8f, 15f);
            rayTracer.Lights.Add(l1);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            waterbody.Update();
            polygonizer.Update();

            base.Draw(gameTime);
#if DEBUG
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Grid Time: " + polygonizer.GridTime, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Poly Time: " + polygonizer.PolyTime, new Vector2(0, 24), Color.White);
            spriteBatch.End();
#endif
        }
    }
}
