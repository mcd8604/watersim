using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracer
{
    public class RTManager : DrawableGameComponent
    {
        private int width;
        private int height;

        private Matrix worldMatrix;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;

        private Vector3 cameraPos;
        public Vector3 CameraPosition
        {
            get { return cameraPos; }
            set { cameraPos = value; }
        }
        private Vector3 cameraTarget;
        public Vector3 CameraTarget
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }

        private float nearDist;
        public float NearPlaneDistance
        {
            get { return nearDist; }
            set { nearDist = value; }
        }
        private float farDist;
        public float FarPlaneDistance
        {
            get { return farDist; }
            set { farDist = value; }
        }

        private Ray[,] rayTable;
        private Texture2D projection;
        private SpriteBatch spriteBatch;

        private Vector4 ambientLight = new Vector4(.2f, .2f, .2f, 1f);

        private List<Light> lights = new List<Light>();
        public List<Light> Lights
        {
            get { return lights; }
            set { lights = value; }
        }

        private List<RayTraceable> rayTraceables = new List<RayTraceable>();
        public List<RayTraceable> RayTraceables
        {
            get { return rayTraceables; }
            set { rayTraceables = value; }
        }

        public RTManager(Game game)
            : base(game) { }

        public override void Initialize()
        {
            worldMatrix = Matrix.Identity;
        
            base.Initialize();
        }

        protected override void LoadContent()
        {
            width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            height = GraphicsDevice.PresentationParameters.BackBufferHeight;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            InitializeViewProjection();

            base.LoadContent();
        }

        private void InitializeViewProjection()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearDist, farDist);

            populateRayTable();
            projection = new Texture2D(GraphicsDevice, width, height);
        }

        private void populateRayTable()
        {
            rayTable = new Ray[width, height];

            Ray ray = new Ray(cameraPos, Vector3.Zero);
            Vector3 viewPlaneVec = Vector3.Zero;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    viewPlaneVec = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), projectionMatrix, viewMatrix, worldMatrix);
                    ray.Direction = Vector3.Normalize(viewPlaneVec - cameraPos);
                    rayTable[x, y] = ray;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            trace();

            spriteBatch.Begin();
            spriteBatch.Draw(projection, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void trace()
        {
            Color[] colorData = new Color[width * height];

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Ray ray = rayTable[x, y];
                    int pixelIndex = (y * width) + x;

                    Vector3 intersectPoint;
                    RayTraceable p = getClosestIntersection(ray, out intersectPoint);

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
            projection = new Texture2D(GraphicsDevice, width, height);
            projection.SetData<Color>(colorData);
        }

        private Vector4 GetLighting(ref Vector3 intersectPoint, RayTraceable p)
        {
            Vector4 totalLight = p.calculateAmbient(ambientLight, intersectPoint);
            Vector4 diffuseTotal = Vector4.Zero;
            Vector4 specularTotal = Vector4.Zero;
            Vector3 intersectNormal = p.GetIntersectNormal(intersectPoint);
            Vector3 viewVector = Vector3.Normalize(cameraPos - intersectPoint);

            foreach (Light light in lights)
            {
                // Spawn a shadow ray from the intersection point to the light source
                Vector3 lightVector = Vector3.Normalize(light.Position - intersectPoint);

                // but only if the intersection is facing the light source
                float facing = Vector3.Dot(intersectNormal, lightVector);
                if (facing < 0)
                {
                    Ray shadowRay = new Ray(intersectPoint, lightVector);

                    // Check if the shadow ray reaches the light before hitting any other object
                    float dist = Vector3.Distance(intersectPoint, light.Position);
                    bool shadowed = false;

                    foreach (RayTraceable primitive in rayTraceables)
                    {
                        if (primitive != p)
                        {
                            float? curDist = primitive.Intersects(shadowRay);
                            if (curDist != null && curDist < dist)
                            {
                                dist = (float)curDist;
                                shadowed = true;
                                break;
                            }
                        }
                    }

                    if (!shadowed)
                    {
                        diffuseTotal += p.calculateDiffuse(intersectPoint, intersectNormal, light, lightVector);
                        specularTotal += p.calculateSpecular(intersectPoint, intersectNormal, light, lightVector, viewVector);
                    }
                }
            }

            totalLight +=
                Vector4.Multiply(diffuseTotal, p.Material1.DiffuseStrength) +
                Vector4.Multiply(specularTotal, p.Material1.SpecularStrength);

            return totalLight;
        }

        /// <summary>
        /// Finds the closest intersected Primitive and sets the intersectPoint Vector3.
        /// </summary>
        /// <param name="ray">The ray to test Primitive intersections.</param>
        /// <param name="intersectPoint">The Vector3 to hold the intersection data.</param>
        /// <returns>The closest intersected Primitive, or null if no Primitive is intersected.</returns>
        private RayTraceable getClosestIntersection(Ray ray, out Vector3 intersectPoint)
        {
            float? dist = float.PositiveInfinity;
            float? curDist = null;
            RayTraceable intersected = null;

            foreach (RayTraceable primitive in rayTraceables)
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
