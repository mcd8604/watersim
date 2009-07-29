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
using WaterLib;

#if DEBUG
using System.Diagnostics;
#endif

namespace VolumeRayCastingShader
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Effect effect;
        Texture3D tex;
        VertexDeclaration vertexDeclaration;
        VertexBuffer vertexBuffer;

        float rotation;
        Matrix world;
        Matrix view;
        Matrix projection;
        readonly Vector3 cameraPos = new Vector3(150f, 150f, 150f);
        readonly Vector3 cameraTarget = new Vector3(0f, 0f, 0f);

        Volume volume;

#if DEBUG
        Stopwatch timer = new Stopwatch();
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            volume = new Volume();
            volume.Update();
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

            // Init Matrices

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 
                GraphicsDevice.Viewport.AspectRatio, 
                1f, 
                1000f);

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

            // TODO: use this.Content to load your game content here

            // Init Effect

            InitEffect();

            // Init Texture

            // Eye rays, geometry, scene grid and grid traversal data 
            // are all transformed into float arrays and are saved as textures

            tex = new Texture3D(GraphicsDevice,
                Volume.GRID_DIMENSION,
                Volume.GRID_DIMENSION,
                Volume.GRID_DIMENSION,
                0,
                TextureUsage.None,
                SurfaceFormat.Vector4);

            UpdateTexture();

            //effect.Parameters["tex0"].SetValue(tex);

            // Init VertexDeclaration

            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Init VertexBuffer

            vertexBuffer = new VertexBuffer(GraphicsDevice, 6 * VertexPositionColor.SizeInBytes, BufferUsage.None);
            VertexPositionColor[] data = 
            { 
                new VertexPositionColor(new Vector3(-1f, -1f, 0), Color.White),
                new VertexPositionColor(new Vector3(-1f, 1f, 0), Color.White),
                new VertexPositionColor(new Vector3(1f, 1f, 0), Color.White),

                new VertexPositionColor(new Vector3(-1f, -1f, 0), Color.White),
                new VertexPositionColor(new Vector3(1f, 1f, 0), Color.White),
                new VertexPositionColor(new Vector3(1f, -1f, 0), Color.White) 
            };
            vertexBuffer.SetData<VertexPositionColor>(data);
            GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
        }

        private void InitEffect()
        {
            effect = Content.Load<Effect>("RayCast");

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            effect.Parameters["InvWorld"].SetValue(Matrix.Invert(world));
            effect.Parameters["InvView"].SetValue(Matrix.Invert(view));
            effect.Parameters["InvProjection"].SetValue(Matrix.Invert(projection));

            effect.Parameters["ScreenResolution"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            effect.Parameters["MinBound"].SetValue(volume.VolumeBB.Min);
            effect.Parameters["MaxBound"].SetValue(volume.VolumeBB.Max);

            effect.Parameters["CastingStepSize"].SetValue(volume.GridCellSize.X); // Temporary
            effect.Parameters["IsoValue"].SetValue(volume.IsoLevel);
            effect.Parameters["GridCellSize"].SetValue(volume.GridCellSize);
        }

        private void UpdateTexture()
        {
            Vector4[] data = 
                new Vector4[ 
                    Volume.GRID_DIMENSION * 
                    Volume.GRID_DIMENSION * 
                    Volume.GRID_DIMENSION ];
            int count = 0;
            for (int x = 0; x < Volume.GRID_DIMENSION; ++x)
            {
                for (int y = 0; y < Volume.GRID_DIMENSION; ++y)
                {
                    for (int z = 0; z < Volume.GRID_DIMENSION; ++z)
                    {
                        data[count++] = new Vector4(volume.Gradient[x, y, z], volume.GridValues[x, y, z]);
                    }
                }
            }
            tex.SetData<Vector4>(data);

            if (effect != null)
                effect.Parameters["VolumeData"].SetValue(tex);
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            world = Matrix.CreateRotationY(MathHelper.ToRadians(++rotation));           
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["InvWorld"].SetValue(Matrix.Invert(world));

            effect.Begin();
            foreach (EffectPass p in effect.CurrentTechnique.Passes) //Techniques["Volume"]
            {
                p.Begin();  
#if DEBUG
                timer.Start();
#endif
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
#if DEBUG
                timer.Stop();
                Console.WriteLine("Draw Time:" + timer.Elapsed.TotalSeconds);
                timer.Reset();
#endif
                p.End();
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
