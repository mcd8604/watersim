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

		VertexPositionColor[] waterPoints;
		VertexDeclaration basicEffectVertexDeclaration;
		VertexBuffer vertexBuffer;
        Effect effect;

		WaterBody waterbody;

		Vector3 gravity = new Vector3(0f, -1f, 0f);

		bool hasdrawn = false;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			waterbody = new WaterBody();
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

			graphics.GraphicsDevice.RenderState.PointSize = 10;

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

			viewMatrix = Matrix.CreateLookAt(new Vector3(40, 30, 20), Vector3.Zero, Vector3.Up);

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
            effect = Content.Load<Effect>("effect");

            basicEffectVertexDeclaration = new VertexDeclaration(
                            graphics.GraphicsDevice, VertexPositionColor.VertexElements);

            effect.Parameters["world"].SetValue(worldMatrix);
            effect.Parameters["view"].SetValue(viewMatrix);
            effect.Parameters["projection"].SetValue(projectionMatrix);

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

			if (hasdrawn)
			{
				waterbody.Update();
				hasdrawn = false;
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
			graphics.GraphicsDevice.VertexDeclaration = basicEffectVertexDeclaration;
			graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);

			effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, waterbody.water.Length);

				pass.End();
			}
            effect.End();

			base.Draw(gameTime);
		}
	}
}
