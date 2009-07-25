#define POS_INF 2147483648;
#define NEG_INF -2147483647;

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 InvWorld;
float4x4 InvView;
float4x4 InvProjection;

float2 Resolution;

float3 CameraPosition;

// AABB data in order of { xMin, xMax, yMin, yMax, zMin, zMax }

static float3 AABBNormals[6] =
{
	{ -1, 0, 0 },
	{ 1, 0, 0 },
	{ 0, -1, 0 },
	{ 0, 1, 0 },
	{ 0, 0, -1 },
	{ 0, 0, 1 }
};

float AABBDistances[6];

float3 MinBound, MaxBound;

struct Ray
{
	float3 Origin;
	float3 Direction;
};

// Volume data

texture tex0;
sampler3D s_3D;

float4 getValue(float3 tex : TEXCOORD)
{
    return tex3D(s_3D, tex);
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    //float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);

	output.Position = input.Position;	
	output.Color = input.Color;

    // TODO: add your vertex shader code here.

    return output;
}

float4 aT = 0;
float4 bT = 0;

Ray GetRay(float2 screenCoords : VPOS)
{       
	// Convert screen coords to range [-1, 1]	
	// Create two points for the ray
	
	float4 a = 
	{ 
		(2 * screenCoords.x / Resolution.x) - 1, 
		1 - (2 * screenCoords.y / Resolution.y), 
		0, 
		1 
	};
		
	float4 b = a;
	b.z = 1;
	
	// Transform points to world space
	
	float4x4 invWVP = mul(mul(InvProjection, InvView), InvWorld);
	
	float4 rayStart = mul(a, invWVP);	
	float4 rayEnd = mul(b, invWVP);
		      
	rayStart /= rayStart.w;
	rayEnd /= rayEnd.w;
		      
	// Create ray in world space
		      
	Ray ray;
	ray.Origin = rayStart;
	ray.Direction = normalize(rayEnd - rayStart);
		
	return ray;
}

float RayPlaneIntersection(Ray ray, float planeOrigDist, float3 planeNormal) 
{	
	// t = -( N.O + d ) / ( N.D )	
	return -( dot( planeNormal, ray.Origin) + planeOrigDist ) / dot( planeNormal, ray.Direction );
}

bool hitbox(Ray r, float3 m1, float3 m2, out float tmin, out float tmax) 
 {
   float tymin, tymax, tzmin, tzmax; 
   float flag = 1.0; 
 
    if (r.Direction.x >= 0) 
    {
       tmin = (m1.x - r.Origin.x) / r.Direction.x;
         tmax = (m2.x - r.Origin.x) / r.Direction.x;
    }
    else 
    {
       tmin = (m2.x - r.Origin.x) / r.Direction.x;
       tmax = (m1.x - r.Origin.x) / r.Direction.x;
    }
    if (r.Direction.y >= 0) 
    {
       tymin = (m1.y - r.Origin.y) / r.Direction.y; 
       tymax = (m2.y - r.Origin.y) / r.Direction.y; 
    }
    else 
    {
       tymin = (m2.y - r.Origin.y) / r.Direction.y; 
       tymax = (m1.y - r.Origin.y) / r.Direction.y; 
    }
     
    if ((tmin > tymax) || (tymin > tmax)) flag = -1.0; 
    if (tymin > tmin) tmin = tymin; 
    if (tymax < tmax) tmax = tymax; 
      
    if (r.Direction.z >= 0) 
    {
       tzmin = (m1.z - r.Origin.z) / r.Direction.z; 
       tzmax = (m2.z - r.Origin.z) / r.Direction.z; 
    }
    else 
    {
       tzmin = (m2.z - r.Origin.z) / r.Direction.z; 
       tzmax = (m1.z - r.Origin.z) / r.Direction.z; 
    }
    if ((tmin > tzmax) || (tzmin > tmax)) flag = -1.0; 
    if (tzmin > tmin) tmin = tzmin; 
    if (tzmax < tmax) tmax = tzmax; 
      
    return (flag > 0); 
 }

//http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
bool RayAABBIntersection(Ray ray, float originDist[6], out float dist)
{
	bool intersects = true;
		
	float tNear = NEG_INF;
	float tFar = POS_INF;
	
	// For each pair of planes associated with X, Y, and Z do:
	
	for(int i = 0, k = 0; i < 3; ++i, k = i * 2)
	{
		// if ray is parallel to the pair of planes
		
		if(ray.Direction[i] == 0)
		{
			// if ray is not between the pair of planes, it does not intersect
			
			if(ray.Origin[i] < originDist[k] || ray.Origin[i] > originDist[k])
			{
				//break;
				intersects = false;
			}
		} 
		else 
		{
			// compute the intersection distance of the planes
			
			// T1 = (Xl - Xo) / Xd
			// T2 = (Xh - Xo) / Xd
			
			float t1 = (originDist[k] - ray.Origin[i]) / ray.Direction[i];
			float t2 = (originDist[k + 1] - ray.Origin[i]) / ray.Direction[i];
						
			//float t1 = RayPlaneIntersection(ray, originDist[k], AABBNormals[k]);
			//float t2 = RayPlaneIntersection(ray, originDist[k + 1], AABBNormals[k + 1]);
			
			// If T1 > T2 swap (T1, T2) /* since T1 intersection with near plane */
			
			if(t1 > t2) 
			{
				float swap = t1;
				t1 = t2;
				t2 = swap;
			}
			
			// If T1 > Tnear set Tnear =T1 /* want largest Tnear */
			
			if(t1 > tNear) 
			{
				tNear = t1;
			}
			
			// If T2 < Tfar set Tfar="T2" /* want smallest Tfar */
			
			if(t2 < tFar)
			{
				tFar = t2;
			}
			
			// If Tnear > Tfar box is missed so return false
			
			if(tNear > tFar) 
			{
				//return false;
				intersects = false;
			}
			
			// If Tfar < 0 box is behind ray return false end
			
			if(tFar < 0)
			{
				//return false;
				intersects = false;
			}
		}
	}
	
	if(!intersects)
	{
		dist = NEG_INF;
	}
	else 
	{
		dist = tNear;
	}
		
	return intersects;
}

float GetClosestIntersection(Ray ray)
{
	float dist, maxDist;
	
	//if(RayAABBIntersection(ray, AABBDistances, dist))
	if(hitbox(ray, MinBound, MaxBound, dist, maxDist))
	{
		// TODO: child AABB intersection
	}
	
	return dist;
}

float4 VolumePixelShader(VertexShaderOutput input, float2 screenCoords : VPOS) : COLOR0
{
    // Get ray
    
    Ray ray = GetRay(screenCoords);
    
    // Test ray intersection    
    
    float dist = GetClosestIntersection(ray);
    
    float4 retColor = input.Color;
    
    if(dist < 0 || dist > 1000)
		retColor = 0;

    return retColor;
}

technique Volume
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 VolumePixelShader();
    }
}
