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

namespace VolumeRayCasting
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<Primitive> primitives;
        WaterBody waterBody;
        Volume volume;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        float nearDist = 0.1f;
        float farDist = 50.0f;
        
        Ray[,] rayTable;
        Texture2D projection;

        List<Light> lights;

        readonly Vector3 cameraPos = new Vector3(75f, 75f, 75f);
        readonly Vector3 cameraTarget = new Vector3(0f, 0f, 0f);

        readonly Vector4 ambientLight = new Vector4(.2f, .2f, .2f, 1f);

#if DEBUG
        double fps;
        int frameCount;
        const double SAMPLE_TIME_FRAME = 1f;
        double sampleTime;
        SpriteFont font;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            primitives = new List<Primitive>();
            waterBody = new WaterBody();
            volume = new Volume(waterBody);
            Material mw = new Material();
            mw.AmbientStrength = 1f;
            mw.setAmbientColor(Color.Blue.ToVector4());
            volume.Material1 = mw;

            primitives.Add(volume);
        }

        private void InitializeLighting()
        {
            lights = new List<Light>();

            Light l1 = new Light();
            l1.LightColor = new Vector4(1f, 1f, 1f, 1f);
            l1.Position = new Vector3(100f, 100f, 100f);
            lights.Add(l1);
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

            viewMatrix = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearDist, farDist);
            worldMatrix = Matrix.Identity;
            populateRayTable();

            projection = new Texture2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        private void populateRayTable()
        {
            rayTable = new Ray[GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight];

            Ray ray = new Ray(cameraPos, Vector3.Zero);
            Vector3 viewPlaneVec = Vector3.Zero;

            for (int x = 0; x < rayTable.GetLength(0); ++x)
            {
                for (int y = 0; y < rayTable.GetLength(1); ++y)
                {
                    viewPlaneVec = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), projectionMatrix, viewMatrix, worldMatrix);
                    ray.Direction = Vector3.Normalize(viewPlaneVec - cameraPos);
                    rayTable[x, y] = ray;
                }
            }
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
        double rayTime;
        Stopwatch sw = new Stopwatch();
#endif

        MouseState lastState = Mouse.GetState();
        MouseState curState;

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

            lastState = curState;

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Z))
            {
                volume.Paused = true;
            }

            if (keyboard.IsKeyDown(Keys.X))
            {
                volume.Paused = false;
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            sw.Reset();
            sw.Start();
            waterBody.Update();
            sw.Stop();
            waterTime = sw.Elapsed.TotalSeconds;
            volume.Update();
            
            if(!volume.Paused)
                rayTracerDraw();

#if DEBUG
            ++frameCount;
#endif

            base.Draw(gameTime);
        }
        
        private void rayTracerDraw()
        {
            Color[] colorData = new Color[rayTable.Length];
            sw.Reset();
            sw.Start();
            for (int y = 0; y < rayTable.GetLength(1); ++y)
            {
                for (int x = 0; x < rayTable.GetLength(0); ++x)
                {
                    Ray ray = rayTable[x, y];
                    int pixelIndex = (y * rayTable.GetLength(0)) + x;

                    Vector3 intersectPoint;
                    Primitive p = getClosestIntersection(ray, out intersectPoint);

                    if (p != null)
                    {
                        // find polygon intersection
                        /*VertexPositionNormalTexture[] vertexData = intersected.VertexData;
                        float? closestDist = float.PositiveInfinity;
                        Vector3 polyNormal = Vector3.Zero;
                        for (int i = 0; i < vertexData.Length; i += 3)
                        {
                            Plane p = new Plane(vertexData[i].Position, vertexData[i + 1].Position, vertexData[i + 2].Position);
                            float? polyDist = ray.Intersects(p);
                            if (polyDist < closestDist)
                            {
                                polyNormal = p.Normal;
                                closestDist = polyDist; 
                            }
                        }*/
                        Vector4 totalLight = GetLighting(ref intersectPoint, p);
                        colorData[pixelIndex] = new Color(totalLight);
                    }
                    else
                    {
                        // use background color
                        colorData[pixelIndex] = Color.CornflowerBlue;
                    }
                }
            }
            sw.Stop();
            rayTime = sw.Elapsed.TotalSeconds;

            projection = new Texture2D(GraphicsDevice, rayTable.GetLength(0), rayTable.GetLength(1));
            projection.SetData<Color>(colorData);

            spriteBatch.Begin();
            spriteBatch.Draw(projection, Vector2.Zero, Color.White);
#if DEBUG
            spriteBatch.DrawString(font, "FPS: " + fps, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Water Time: " + waterTime, new Vector2(0, 24), Color.White);
            spriteBatch.DrawString(font, "Grid Time: " + volume.GridTime, new Vector2(0, 48), Color.White);
            spriteBatch.DrawString(font, "Ray Time: " + rayTime, new Vector2(0, 72), Color.White);
#endif
            spriteBatch.End();

        }

        private Vector4 GetLighting(ref Vector3 intersectPoint, Primitive p)
        {
            return Color.Black.ToVector4();
            Vector4 totalLight = p.calculateAmbient(ambientLight, intersectPoint);
            //Vector4 diffuseTotal = Vector4.Zero;
            //Vector4 specularTotal = Vector4.Zero;
            //Vector3 intersectNormal = p.GetIntersectNormal(intersectPoint);
            //Vector3 viewVector = Vector3.Normalize(cameraPos - intersectPoint);

            //foreach (Light light in lights)
            //{
                // Spawn a shadow ray from the intersection point to the light source
                //Vector3 lightVector = Vector3.Normalize(light.Position - intersectPoint);

                // but only if the intersection is facing the light source
                //float facing = Vector3.Dot(intersectNormal, lightVector);
                //if (facing < 0)
                //{
                //    Ray shadowRay = new Ray(intersectPoint, lightVector);

                //    // Check if the shadow ray reaches the light before hitting any other object
                //    float dist = Vector3.Distance(intersectPoint, light.Position);
                //    bool shadowed = false;

                //    foreach (Primitive primitive in primitives)
                //    {
                //        if (primitive != p)
                //        {
                //            float? curDist = primitive.Intersects(shadowRay);
                //            if (curDist != null && curDist < dist)
                //            {
                //                dist = (float)curDist;
                //                shadowed = true;
                //                break;
                //            }
                //        }
                //    }

                //    if (!shadowed)
                //    {
                //        diffuseTotal += p.calculateDiffuse(intersectPoint, intersectNormal, light, lightVector);
                //        specularTotal += p.calculateSpecular(intersectPoint, intersectNormal, light, lightVector, viewVector);
                //    }
                //}
            //}

            //totalLight +=
            //    Vector4.Multiply(diffuseTotal, p.Material1.DiffuseStrength) +
            //    Vector4.Multiply(specularTotal, p.Material1.SpecularStrength);

            return totalLight;
        }

        /// <summary>
        /// Finds the closest intersected Primitive and sets the intersectPoint Vector3.
        /// </summary>
        /// <param name="ray">The ray to test Primitive intersections.</param>
        /// <param name="intersectPoint">The Vector3 to hold the intersection data.</param>
        /// <returns>The closest intersected Primitive, or null if no Primitive is intersected.</returns>
        private Primitive getClosestIntersection(Ray ray, out Vector3 intersectPoint)
        {
            float? dist = float.PositiveInfinity;
            float? curDist = null;
            Primitive intersected = null;

            foreach (Primitive primitive in primitives)
            {
                curDist = primitive.Intersects(ray);
                if (curDist < dist)
                {
                    dist = curDist;
                    intersected = primitive;
                }
            }

            intersectPoint = ray.Position + Vector3.Multiply(ray.Direction, (float)dist);

            return intersected;
        }

    }
}
