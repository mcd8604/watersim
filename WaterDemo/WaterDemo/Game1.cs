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

namespace WaterDemo
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		//graphics stuff
		Matrix worldMatrix;
		Matrix viewMatrix;
		Matrix projectionMatrix;

        Vector3 cameraPos = new Vector3(40, 30, 20);

		VertexPositionColor[] waterPoints;
		VertexDeclaration basicEffectVertexDeclaration;
		VertexBuffer vertexBuffer;
        Effect effect;
        Model model;
        Matrix scale;

		WaterBody waterbody;

		Vector3 gravity = new Vector3(0f, -1f, 0f);

		bool hasdrawn = false;
		bool paused = false;

		VertexPositionNormalTexture[] floorVertices;
		VertexDeclaration vpntDeclaration;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			waterbody = new WaterBody();
			InitializeFloor();
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

		private void InitializeFloor()
		{
			floorVertices = new VertexPositionNormalTexture[6];

			floorVertices[0] = new VertexPositionNormalTexture(new Vector3(waterbody.Max.X, waterbody.Min.Y, waterbody.Max.Z), Vector3.Up, Vector2.Zero);
			floorVertices[1] = new VertexPositionNormalTexture(new Vector3(waterbody.Min.X, waterbody.Min.Y, waterbody.Max.Z), Vector3.Up, Vector2.Zero);
			floorVertices[2] = new VertexPositionNormalTexture(new Vector3(waterbody.Min.X, waterbody.Min.Y, waterbody.Min.Z), Vector3.Up, Vector2.Zero);

			floorVertices[3] = new VertexPositionNormalTexture(new Vector3(waterbody.Max.X, waterbody.Min.Y, waterbody.Max.Z), Vector3.Up, Vector2.Zero);
			floorVertices[4] = new VertexPositionNormalTexture(new Vector3(waterbody.Min.X, waterbody.Min.Y, waterbody.Min.Z), Vector3.Up, Vector2.Zero);
			floorVertices[5] = new VertexPositionNormalTexture(new Vector3(waterbody.Max.X, waterbody.Min.Y, waterbody.Min.Z), Vector3.Up, Vector2.Zero);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// graphics stuff?
			InitializeTransform();
			InitializeEffect();
			InitializeVertices();

			vpntDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

			graphics.GraphicsDevice.RenderState.PointSize = 10;

            model = Content.Load<Model>("sphere");
            scale = Matrix.CreateScale(0.5f);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// Initializes the transforms used for the 3D model.
		/// </summary>
		private void InitializeTransform()
		{
			//float tilt = MathHelper.ToRadians(22.5f);  // 22.5 degree angle
			// Use the world matrix to tilt the cube along x and y axes.
			//worldMatrix = Matrix.CreateRotationX(tilt) *
			//	Matrix.CreateRotationY(tilt);
			worldMatrix = Matrix.CreateRotationX(0);

			viewMatrix = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);

			projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(45),  // 45 degree angle
				(float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height,
				1.0f, 1000.0f);
		}

		/// <summary>
		/// Initializes the basic effect (parameter setting and technique selection)
		/// used for the 3D model.
		/// </summary>
		private void InitializeEffect()
		{
            effect = Content.Load<Effect>("Basic");

            basicEffectVertexDeclaration = new VertexDeclaration(
                            graphics.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            effect.Parameters["World"].SetValue(worldMatrix);
            effect.Parameters["View"].SetValue(viewMatrix);
            effect.Parameters["Projection"].SetValue(projectionMatrix);

            effect.Parameters["lightPos"].SetValue(new Vector4(20f, 20f, 20f, 1f));
            effect.Parameters["lightColor"].SetValue(Vector4.One);
            effect.Parameters["cameraPos"].SetValue(new Vector4(cameraPos, 1f));

            effect.Parameters["ambientColor"].SetValue(new Vector4(.2f, .2f, .2f, 1f));
            //effect.Parameters["materialColor"].SetValue(new Vector4(.0f, .5f, .8f, 1f));
            effect.Parameters["diffusePower"].SetValue(1f);
            effect.Parameters["specularPower"].SetValue(1);
            effect.Parameters["exponent"].SetValue(8);

			/*basicEffectVertexDeclaration = new VertexDeclaration(
				graphics.GraphicsDevice, VertexPositionColor.VertexElements);

			basicEffect = new BasicEffect(graphics.GraphicsDevice, null);
			//basicEffect.Alpha = 1.0f;
			//basicEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 1.0f);
			//basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
			//basicEffect.SpecularPower = 5.0f;
			basicEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);

			//basicEffect.DirectionalLight0.Enabled = true;
			//basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
			//basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1.0f, -1.0f, -1.0f));
			//basicEffect.DirectionalLight0.SpecularColor = Vector3.One;

			//basicEffect.DirectionalLight1.Enabled = true;
			//basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
			//basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.0f, 1.0f));
			//basicEffect.DirectionalLight1.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);

			basicEffect.LightingEnabled = true;

			basicEffect.VertexColorEnabled = true;

			basicEffect.World = worldMatrix;
			basicEffect.View = viewMatrix;
			basicEffect.Projection = projectionMatrix;*/
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

			vertexBuffer = new VertexBuffer(
				graphics.GraphicsDevice,
				VertexPositionColor.SizeInBytes * waterPoints.Length, BufferUsage.None
			);

			vertexBuffer.SetData<VertexPositionColor>(waterPoints);

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

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();

			if (keyboard.IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			if (keyboard.IsKeyDown(Keys.Space))
			{
				waterbody = new WaterBody();
			}

			if (keyboard.IsKeyDown(Keys.P))
			{
				paused = true;
			}

			if (keyboard.IsKeyDown(Keys.R))
			{
				paused = false;
			}

			waterbody.control = keyboard.IsKeyDown(Keys.C);

			if (hasdrawn && !paused)
			{
				waterbody.Update();
				hasdrawn = false;
				Count++;
			}


			for (int i = 0; i < waterbody.water.Length; i++)
			{
				waterPoints[i] = new VertexPositionColor(waterbody.water[i].Position, waterbody.water[i].color);
			}

			vertexBuffer = new VertexBuffer(
				graphics.GraphicsDevice,
				VertexPositionColor.SizeInBytes * waterPoints.Length, BufferUsage.None
			);

			vertexBuffer.SetData<VertexPositionColor>(waterPoints);

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

			graphics.GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here
            //graphics.GraphicsDevice.VertexDeclaration = basicEffectVertexDeclaration;
            //graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);

            //effect.Begin();
            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //{
            //    pass.Begin();

            //    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, waterbody.water.Length);

            //    pass.End();
            //}
            //effect.End();

			effect.Parameters["World"].SetValue(worldMatrix);

			graphics.GraphicsDevice.VertexDeclaration = vpntDeclaration;

			effect.Parameters["materialColor"].SetValue(Color.Blue.ToVector4());
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, floorVertices, 0, 2);
				pass.End();
			}
			effect.End();

            graphics.GraphicsDevice.VertexDeclaration = basicEffectVertexDeclaration;
            foreach (Water w in waterbody.water)
            {
            	effect.Parameters["materialColor"].SetValue(w.color.ToVector4());
                effect.Parameters["World"].SetValue(Matrix.Multiply(scale, Matrix.CreateTranslation(w.Position)));
                foreach (ModelMesh mesh in model.Meshes)
                {
                    mesh.Draw();
                }
            }

			foreach (Solid solid in waterbody.solids)
			{
				effect.Parameters["materialColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["World"].SetValue(Matrix.Multiply(Matrix.CreateScale(2), Matrix.CreateTranslation(solid.Position)));
				foreach (ModelMesh mesh in model.Meshes)
				{
					mesh.Draw();
				}
			}


			base.Draw(gameTime);
		}
	}
}
