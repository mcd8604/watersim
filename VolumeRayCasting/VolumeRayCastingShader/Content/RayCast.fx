#define POS_INF 2147483648;
#define NEG_INF -2147483647;

// Render parameters

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 InvWorld;
float4x4 InvView;
float4x4 InvProjection;

float2 ScreenResolution;

// Volume parameters

float CastingStepSize;
float IsoValue;
float3 MinBound, MaxBound;
float3 GridCellSize;

// Volume data

texture VolumeData;
sampler3D VolumeSampler =
sampler_state
{
	Texture = <VolumeData>;
};

// Structures

struct Ray
{
	float3 Origin;
	float3 Direction;
};

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
		(2 * screenCoords.x / ScreenResolution.x) - 1, 
		1 - (2 * screenCoords.y / ScreenResolution.y), 
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
	
// Returns true if a ray intersects with the parallel pairs of planes of a bounding box.
// tNear - distance along the ray of the near intersection.
// tFar - distance along the ray of the far intersection.
bool RayAABBIntersection(Ray ray, float3 min, float3 max, out float tNear, out float tFar)
{		
	bool intersects = true;

	tNear = NEG_INF;
	tFar = POS_INF;		
	
	// http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
	// if ray is parallel to the axis, then	
	// if ray is not between the pair of planes, it does not intersect
	// else, compute the intersection distance of the planes	
	// If T1 > T2, swap (T1, T2) /* since T1 intersection with near plane */
	// If T1 > Tnear, set Tnear =T1 /* want largest Tnear */	
	// If T2 < Tfar, set Tfar="T2" /* want smallest Tfar */	
	// If Tnear > Tfar, box is missed so return false	
	// If Tfar < 0, box is behind ray return false		
	
	// X Axis
	
	if(ray.Direction.x == 0) {
		if(ray.Origin.x < min.x || ray.Origin.x > max.x) intersects = false;
	} else {		
		float tx1 = (min.x - ray.Origin.x) / ray.Direction.x;
		float tx2 = (max.x - ray.Origin.x) / ray.Direction.x;
		if(tx1 > tx2) {
			float swap = tx1;
			tx1 = tx2;
			tx2 = swap;
		}			
		if(tx1 > tNear) tNear = tx1;				
		if(tx2 < tFar) tFar = tx2;				
		if(tNear > tFar) intersects = false;				
		if(tFar < 0) intersects = false;
	}	
	
	// Y Axis
	
	if(intersects) {
		if(ray.Direction.y == 0) {
			if(ray.Origin.y < min.y || ray.Origin.y > max.y) intersects = false;
		} else {		
			float ty1 = (min.y - ray.Origin.y) / ray.Direction.y;
			float ty2 = (max.y - ray.Origin.y) / ray.Direction.y;
			if(ty1 > ty2) {
				float swap = ty1;
				ty1 = ty2;
				ty2 = swap;
			}							
			if(ty1 > tNear) tNear = ty1;				
			if(ty2 < tFar) tFar = ty2;				
			if(tNear > tFar) intersects = false;				
			if(tFar < 0) intersects = false;
		}
	}
	
	// Z Axis
	
	if(intersects) {
		if(ray.Direction.z == 0) {
			if(ray.Origin.z < min.z || ray.Origin.z > max.z) intersects = false;
		} else {		
			float tz1 = (min.z - ray.Origin.z) / ray.Direction.z;
			float tz2 = (max.z - ray.Origin.z) / ray.Direction.z;	
			if(tz1 > tz2) {
				float swap = tz1;
				tz1 = tz2;
				tz2 = swap;
			}					
			if(tz1 > tNear) tNear = tz1;				
			if(tz2 < tFar) tFar = tz2;				
			if(tNear > tFar) intersects = false;				
			if(tFar < 0) intersects = false;
		}
	}
		
	return intersects;
}

// Returns true if a valid value of the volume is intersected by the Ray.
bool VolumeIntersection(Ray ray, out float4 intersectData)
{    
    float nearDist = 0, farDist = 0;  	
	bool intersected = false;

	// Check the bounding box first	
	
	if(RayAABBIntersection(ray, MinBound, MaxBound, nearDist, farDist))
	{	
		// Process volume - step along the ray until either 
		// a value is found or farDist is reached.	
		
		float curDist = nearDist;
		float3 curPt;
		float3 index;
        int xIndex = 0;
        int yIndex = 0;
        int zIndex = 0;	
        
        int i = 0;
		
		while(curDist < farDist && !intersected && ++i < 128) 
		{
			curPt = ray.Origin + (ray.Direction * curDist);
			
			index.x = floor(((curPt.x - MinBound.x) / GridCellSize.x));
			index.y = floor(((curPt.y - MinBound.y) / GridCellSize.y));
			index.z = floor(((curPt.z - MinBound.z) / GridCellSize.z));
            
			float3 index000 = { xIndex, yIndex, zIndex };
			float3 index001 = { xIndex, yIndex, zIndex + 1 };
			float3 index010 = { xIndex, yIndex + 1, zIndex };
			float3 index011 = { xIndex, yIndex + 1, zIndex + 1 };
			float3 index100 = { xIndex + 1, yIndex, zIndex };
			float3 index101 = { xIndex + 1, yIndex, zIndex + 1 };
			float3 index110 = { xIndex + 1, yIndex + 1, zIndex };
			float3 index111 = { xIndex + 1, yIndex + 1, zIndex + 1 };
                                    
			// trilinear interpolation
            
			float3 c000 = MinBound + (GridCellSize * index000);
			float3 c111 = c000 + GridCellSize;
            
			float3 delta = (curPt - c000) / (c111 - c000);

			float4 d000 = tex3D(VolumeSampler, index000);
			float4 d001 = tex3D(VolumeSampler, index001);
			float4 d010 = tex3D(VolumeSampler, index010);
			float4 d011 = tex3D(VolumeSampler, index011);
			float4 d100 = tex3D(VolumeSampler, index100);
			float4 d101 = tex3D(VolumeSampler, index101);
			float4 d110 = tex3D(VolumeSampler, index110);
			float4 d111 = tex3D(VolumeSampler, index111);
       
			float4 d00 = lerp(d000, d100, delta.x);
			float4 d01 = lerp(d001, d101, delta.x);
			float4 d11 = lerp(d011, d111, delta.x);
			float4 d10 = lerp(d010, d110, delta.x);

			float4 d0 = lerp(d00, d10, delta.y);
			float4 d1 = lerp(d01, d11, delta.y);
			
			float4 d = lerp(d0, d1, delta.z);

			// (d.xyz contains the gradient normal of the current point)
			// (d.w contains the density of the current point)

			// If the density value exceeds the IsoValue, the ray intersects

			if (d.z > IsoValue)
			{
				intersected = true;
				//normalize(d);
				intersectData = d;
			} else			
				curDist += CastingStepSize;
		}	
	}
	
	return intersected;
}

float4 VolumePixelShader(VertexShaderOutput input, float2 screenCoords : VPOS) : COLOR0
{
    // Get ray
    
    Ray ray = GetRay(screenCoords);
    
    // Test ray intersection    
    
    float4 intersectData = 0;
    bool intersected = VolumeIntersection(ray, intersectData);		
        
    if(intersected)
		return 1;
	else 
		return 0; //background color
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
