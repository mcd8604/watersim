// Change to #define to enforce minimum memory usage - kills CPU
#undef FORCE_MINIMUM_MEMORY_USAGE

// Remember to set in Polygonizer too
#define USE_ARRAY

using System;
using AviAccess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WaterPolygonizerDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
		private readonly GraphicsDeviceManager graphics;
#if DEBUG
        private SpriteBatch spriteBatch;
        private SpriteFont font;
		private int framecount = 0;
#endif

		private WaterBody waterbody;
		private VertexPositionColor[] waterPoints;
		private VertexDeclaration vpcDeclaration;
		private VertexDeclaration vpnDeclaration;
		private VertexBuffer waterVertexBuffer;
		private Polygonizer polygonizer;

		private VertexPositionNormal[] floorVertices;

		private bool hasdrawn;
		private bool paused;

        //BasicEffect effect;
		private Effect effect;

		private Vector3 camPosition;
		private Vector3 camTarget;

		private Matrix world;
		private Matrix view;
		private Matrix projection;

    	private readonly AviWriter aviWriter;

    	private MouseState lastState;
		private MouseState curState;
		private float rotation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            waterbody = new WaterBody();
            polygonizer = new Polygonizer(waterbody);
            InitializeFloor();

            aviWriter = new AviWriter(this, "test.avi");

            Components.Add(aviWriter);
        }

        private void InitializeFloor()
        {
            floorVertices = new VertexPositionNormal[6];

			Vector3 PositionMin = waterbody.Min;
			Vector3 PositionMax = waterbody.Max;

            floorVertices[0] = new VertexPositionNormal(new Vector3(PositionMax.X, PositionMin.Y, PositionMax.Z), Vector3.Up);
            floorVertices[1] = new VertexPositionNormal(new Vector3(PositionMin.X, PositionMin.Y, PositionMax.Z), Vector3.Up);
            floorVertices[2] = new VertexPositionNormal(new Vector3(PositionMin.X, PositionMin.Y, PositionMin.Z), Vector3.Up);

            floorVertices[3] = new VertexPositionNormal(new Vector3(PositionMax.X, PositionMin.Y, PositionMax.Z), Vector3.Up);
            floorVertices[4] = new VertexPositionNormal(new Vector3(PositionMin.X, PositionMin.Y, PositionMin.Z), Vector3.Up);
            floorVertices[5] = new VertexPositionNormal(new Vector3(PositionMax.X, PositionMin.Y, PositionMin.Z), Vector3.Up);
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

            waterVertexBuffer.SetData(waterPoints);

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
#if DEBUG
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
			font = Content.Load<SpriteFont>("font");
#endif

            GraphicsDevice.RenderState.PointSize = 5f;
            vpcDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            vpnDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormal.VertexElements);

            InitializeMatrices();
            InitializeEffect();
            InitializeVertices();
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
            camPosition = new Vector3(120, 40, 120);
            //camTarget = waterbody.PositionMin + ((waterbody.PositionMax - waterbody.PositionMin) / 2);
            camTarget = new Vector3(0, -40, 0);
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
            effect.Parameters["materialColor"].SetValue(new Vector4(.8f, .8f, .9f, .2f));
            effect.Parameters["diffusePower"].SetValue(1f);
            effect.Parameters["specularPower"].SetValue(.45f);
            effect.Parameters["exponent"].SetValue(8);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if FORCE_MINIMUM_MEMORY_USAGE
			GC.Collect();
#endif

            curState = Mouse.GetState();

            if (curState.LeftButton == ButtonState.Pressed)
            {
                // Update camera rotation
                rotation += (curState.X - lastState.X) / (float)GraphicsDevice.Viewport.Width * MathHelper.TwoPi;
                //float y = (curState.Y - lastState.Y) / (float)GraphicsDevice.Viewport.Height * MathHelper.TwoPi;
                //camRotation = Quaternion.CreateFromYawPitchRoll(x, y, 0);
                resetCamera();
            }

            lastState = curState;

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
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
                try
                {
                    waterbody.Update();
                    polygonizer.Update();

					for (int i = 0; i < waterbody.water.Length; i++)
					{
						waterPoints[i] = new VertexPositionColor(waterbody.water[i].Position, waterbody.water[i].color);
					}

					waterVertexBuffer = new VertexBuffer(
						graphics.GraphicsDevice,
						VertexPositionColor.SizeInBytes * waterPoints.Length, BufferUsage.None
					);

					waterVertexBuffer.SetData(waterPoints);

					hasdrawn = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    aviWriter.Close();
					Exit();
					return;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // draw floor

            graphics.GraphicsDevice.VertexDeclaration = vpnDeclaration;

            effect.Parameters["materialColor"].SetValue(Color.SkyBlue.ToVector4());
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, floorVertices, 0, 2);
                pass.End();
            }
            effect.End();

            if (polygonizer.Paused)
            {
                // draw vertices
                graphics.GraphicsDevice.VertexDeclaration = vpcDeclaration;
                graphics.GraphicsDevice.Vertices[0].SetSource(waterVertexBuffer, 0, VertexPositionColor.SizeInBytes);
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, waterbody.water.Length);
                    pass.End();
                }
                effect.End();
            }
            effect.Parameters["materialColor"].SetValue(new Vector4(.5f, .6f, .9f, .2f));

#if !USE_ARRAY
			if (polygonizer.vertexList.Count > 0 && !polygonizer.Paused)
            {
                graphics.GraphicsDevice.VertexDeclaration = vpnDeclaration;
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
					GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, polygonizer.vertexList.ToArray() , 0, polygonizer.vertexList.Count / 3);
                    pass.End();
                }
                effect.End();
            }
#else
			if (polygonizer.currentframeprimatives > 0 && !polygonizer.Paused)
            {
                graphics.GraphicsDevice.VertexDeclaration = vpnDeclaration;
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
					GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, polygonizer.VertexList, 0, polygonizer.currentframeprimatives);
                    pass.End();
                }
                effect.End();
            }
#endif

#if DEBUG
            spriteBatch.Begin();
			spriteBatch.DrawString(font, "Phys Time: " + waterbody.timer.Elapsed.TotalSeconds, Vector2.Zero, Color.White);
			spriteBatch.DrawString(font, "Grid Time: " + polygonizer.GridTime, new Vector2(0, 24), Color.White);
            spriteBatch.DrawString(font, "Poly Time: " + polygonizer.PolyTime, new Vector2(0, 48), Color.White);
			spriteBatch.DrawString(font, "Frames:    " + (++framecount), new Vector2(0, 72), Color.White);
            spriteBatch.End();
#endif

			hasdrawn = true;

			base.Draw(gameTime);
        }
    }
}