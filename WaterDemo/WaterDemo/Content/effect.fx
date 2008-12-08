
float4x4 world;
float4x4 view;
float4x4 projection;

struct VertexToPixel
{
    float4 Position   	: POSITION;
    float4 Color    	: COLOR0;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

VertexToPixel ParticleVS (float4 Position : POSITION, float4 Color : COLOR0)
{
    VertexToPixel Output = (VertexToPixel)0;
     
    float4x4 preViewProjection = mul (view, projection);
	float4x4 preWorldViewProjection = mul (world, preViewProjection); 
    
    Output.Position = mul(Position, preWorldViewProjection);    
    Output.Color = Color;
    
    return Output;    
}

PixelToFrame ParticlePS(VertexToPixel PSIn, float4 Color : COLOR0)
{ 
    PixelToFrame Output = (PixelToFrame)0;

    Output.Color = Color;
    
    return Output;
}

technique Particle
{
	pass Pass0
    {   
    	PointSpriteEnable = true;
    	VertexShader = compile vs_1_1 ParticleVS();
        PixelShader  = compile ps_1_1 ParticlePS();
    }
}

