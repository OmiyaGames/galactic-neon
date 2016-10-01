Shader "Omiya Games/Blend 2 Textures"
{ 
    Properties
    {
        _Color ("Color", Color) = (1,1,1)
        _Blend ("Blend", Range (0, 1) ) = 0.5 
        _MainTex ("Texture 1", 2D) = "" 
        _Texture2 ("Texture 2", 2D) = ""
    }
 
    SubShader
    {
        Pass
        {
            Material
            {
                Ambient[_Color]
                Diffuse[_Color]
            }
            
            SetTexture[_MainTex]
            SetTexture[_Texture2]
            { 
                ConstantColor(0, 0, 0, [_Blend])
                Combine texture Lerp(constant) previous
            }       
        }
    }
}
