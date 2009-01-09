float4x4 World;
float4x4 View;
float4x4 Projection;

float4 lightPos;
float4 lightColor;
float4 cameraPos;

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
    float4 Position : POSITION0;
    float4 Color	: COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection); 
    
    float4 lightVector = normalize(lightPos - input.Position);
    //float4 reflectedVector = normalize(reflect(lightVector, input.Normal));
    //float4 viewVector = normalize(cameraPos - input.Position);
    
    float4 ambient = ambientColor * materialColor;
    float4 surfaceColor = lightColor * materialColor;
    float4 diffuse = diffusePower * surfaceColor * saturate(dot(lightVector, input.Normal)); 
    //float4 specular = specularPower * surfaceColor * pow(dot(viewVector, reflectedVector), exponent);

    //output.Color = ambient + diffuse + specular;
	output.Color = ambient + diffuse;
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
