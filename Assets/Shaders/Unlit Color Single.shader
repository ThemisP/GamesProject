Shader "Unlit/Unlit Color Single" {
 
Properties {
    _Color ("Color", Color) = (1.000000,1.000000,1.000000,1.000000)
}
 
SubShader {
    Color [_Color]
    Pass {}
}
 
}