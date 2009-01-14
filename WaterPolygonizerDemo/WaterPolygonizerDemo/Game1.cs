using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;
using AviAccess;

namespace WaterPolygonizerDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        WaterBody waterbody;
        VertexPositionColor[] waterPoints;
        VertexDeclaration vpcDeclaration;
        VertexDeclaration vpntDeclaration;
        VertexBuffer waterVertexBuffer;
        Polygonizer polygonizer;

        Vector3 gravity = new Vector3(0f, -1f, 0f);

        bool hasdrawn = false;
        bool paused = false;

        //BasicEffect effect;
        Effect effect;

        Vector3 camPosition;
        Vector3 camTarget;
        Quaternion camRotation = Quaternion.Identity;

        Matrix world;
        Matrix view;
        Matrix projection;

        SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            waterbody = new WaterBody();
            polygonizer = new Polygonizer(waterbody);

            //AviWriter aviWriter = new AviWriter(this, "test.avi");

            //Components.Add(aviWriter);
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

            base.Initialize();
        }

        /// <summary>
        /// Initializes the vertices and indices of the 3D model.
        /// </summary>
        private void InitializeVertices()
        {
            waterPoints = new VertexPositionColor[waterbody.water.Length];

            for (int i = 0; i < waterbody.water.Length; i++)
            {
                waterPoints[i] = new VertexPositionColor(waterbody.water[i].Position, waterbody.water[i].color);
            }

            waterVertexBuffer = new VertexBuffer(
                graphics.GraphicsDevice,
                VertexPositionColor.SizeInBytes * waterPoints.Length, BufferUsage.None
            );

            waterVertexBuffer.SetData<VertexPositionColor>(waterPoints);

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
            GraphicsDevice.RenderState.PointSize = 5f;
            vpcDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            vpntDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            InitializeMatrices();
            InitializeEffect();
            InitializeVertices();

            font = Content.Load<SpriteFont>("font");
        }

        private void InitializeMatrices()
        {
            world = Matrix.Identity;
            resetCamera();
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.001f, 1000f);
        }

        private void resetCamera()
        {
            //camPosition = waterbody.PositionMax * 2;
            camPosition = new Vector3(30, 10, 30);
            //camTarget = waterbody.PositionMin + ((waterbody.PositionMax - waterbody.PositionMin) / 2);
            camTarget = new Vector3(0, -10, 0);
            view = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
            if (effect != null)
            {
                //effect.View = view;
                effect.Parameters["View"].SetValue(view);
            }
        }

        private void InitializeEffect()
        {
            //effect = new BasicEffect(GraphicsDevice, new EffectPool());

            //effect.World = world;
            //effect.View = view;
            //effect.Projection = projection;

            //effect.EnableDefaultLighting();

            //effect.AmbientLightColor = new Vector3(0.0f, 0.0f, 0.2f);

            //effect.DirectionalLight0.Enabled = true;
            //effect.DirectionalLight0.DiffuseColor = new Vector3(0.25f, .25f, .25f);
            //effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, 2, 1));
            //effect.DirectionalLight0.SpecularColor = new Vector3(0.75f, 0.75f, 75f);

            effect = Content.Load<Effect>("Basic");

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            effect.Parameters["lightPos"].SetValue(new Vector3(20f, 20f, 20f));
            effect.Parameters["lightColor"].SetValue(Vector4.One);
            effect.Parameters["cameraPos"].SetValue(camPosition);

            effect.Parameters["ambientColor"].SetValue(new Vector4(.2f, .2f, .2f, 1f));
            effect.Parameters["materialColor"].SetValue(new Vector4(.0f, .5f, .8f, .4f));
            effect.Parameters["diffusePower"].SetValue(1f);
            effect.Parameters["specularPower"].SetValue(1);
            effect.Parameters["exponent"].SetValue(4);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private float Time;
        private int Count;

        MouseState lastState = Mouse.GetState();
        MouseState curState;
        float rotation;

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

            curState = Mouse.GetState();

            if (curState.LeftButton == ButtonState.Pressed)
            {
                // Update camera rotation
                rotation += (curState.X - lastState.X) / (float)GraphicsDevice.Viewport.Width * MathHelper.TwoPi;
                float y = (curState.Y - lastState.Y) / (float)GraphicsDevice.Viewport.Height * MathHelper.TwoPi;
                //camRotation = Quaternion.CreateFromYawPitchRoll(x, y, 0);
                resetCamera();
            }

            lastState = curState;

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (keyboard.IsKeyDown(Keys.Space))
            {
                waterbody = new WaterBody();
                polygonizer = new Polygonizer(waterbody);
                resetCamera();
            }

            if (keyboard.IsKeyDown(Keys.P))
            {
                paused = true;
            }

            if (keyboard.IsKeyDown(Keys.R))
            {
                paused = false;
            }

            if (keyboard.IsKeyDown(Keys.Z))
            {
                polygonizer.Paused = true;
            }

            if (keyboard.IsKeyDown(Keys.X))
            {
                polygonizer.Paused = false;
            }

            if (hasdrawn && !paused)
            {
                waterbody.Update();                
                polygonizer.Update();
                hasdrawn = false;
                Count++;
            }

            for (int i = 0; i < waterbody.water.Length; i++)
            {
                waterPoints[i] = new VertexPositionColor(waterbody.water[i].Position, waterbody.water[i].color);
            }

            waterVertexBuffer = new VertexBuffer(
                graphics.GraphicsDevice,
                VertexPositionColor.SizeInBytes * waterPoints.Length, BufferUsage.None
            );

            waterVertexBuffer.SetData<VertexPositionColor>(waterPoints);

            Time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Time > 1f)
            {
                Console.WriteLine(Count);
                Count = 0;
                Time -= 1f;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            hasdrawn = true;

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            graphics.GraphicsDevice.VertexDeclaration = vpcDeclaration;
            graphics.GraphicsDevice.Vertices[0].SetSource(waterVertexBuffer, 0, VertexPositionColor.SizeInBytes);

            if (polygonizer.Paused)
            {
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, waterbody.water.Length);
                    pass.End();
                }
                effect.End();
            }

            if (polygonizer.Vertices.Length > 0 && !polygonizer.Paused)
            {
                graphics.GraphicsDevice.VertexDeclaration = vpntDeclaration;
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, polygonizer.Vertices, 0, polygonizer.Vertices.Length / 3);
                    pass.End();
                }
                effect.End();
            }

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
