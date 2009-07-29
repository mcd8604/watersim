float4x4 World;
float4x4 View;
float4x4 Projection;

float3 lightPos;
float4 lightColor;
float3 cameraPos;

float4 materialColor;
float4 ambientColor;

float diffusePower;
float specularPower;
float exponent;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal	: NORMAL0;
    float4 Color    : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float3 WPosition: TEXCOORD0;
    float3 Normal	: TEXCOORD1;
    float3 Light	: TEXCOORD2;
    float3 Reflected: TEXCOORD3;
    float3 View		: TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    	
    float4x4 wvp = mul(mul(World,View),Projection);
    //output.Position = mul(input.Position, wvp);
    output.Position = input.Position;
    output.WPosition = mul(input.Position, World).xyz;
    
	output.Normal = mul(input.Normal, World);
    
    output.Light = normalize(lightPos - output.WPosition);
    output.Reflected = normalize(reflect(output.Light, output.Normal));
    output.View = normalize(cameraPos - output.WPosition);
        
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{    
    float4 ambient = ambientColor * materialColor;
    float4 surfaceColor = lightColor * materialColor;
    float4 diffuse = diffusePower * surfaceColor * dot(input.Light, input.Normal); 
    
    float4 specular;
    float dot = dot(input.View, input.Reflected);
    
    if(dot >= 0) {
		specular = 0;
	} else {   
		specular = specularPower * lightColor * pow(dot, exponent);
	}
	
    return ambient + diffuse + specular;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
