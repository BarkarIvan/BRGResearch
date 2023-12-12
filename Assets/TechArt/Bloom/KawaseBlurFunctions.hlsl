#ifndef  KAWASE_BLUR_FUNCTIONS
#define  KAWASE_BLUR_FUNCTIONS

half4 downsample(half2 uv, half2 halfpixel, sampler2D tex, half offset )
{
    half4 sum = tex2D(tex, uv) * 4.0;
    sum += tex2D(tex, uv - halfpixel.xy * offset);
    sum += tex2D(tex, uv + halfpixel.xy * offset);
    sum += tex2D(tex, uv + half2(halfpixel.x, - halfpixel.y) * offset);
    sum += tex2D(tex, uv - half2(halfpixel.x, - halfpixel.y) * offset);
    return sum/8.0;
}

half4 downsampleWithStep(half2 uv, half2 halfpixel, sampler2D tex, half offset, half stepamount)
{
    half4 sum = step(stepamount, tex2D(tex, uv)) * 4.0;
    sum += step(stepamount, tex2D(tex, uv - halfpixel.xy * offset));
    sum += step(stepamount, tex2D(tex, uv + halfpixel.xy * offset));
    sum += step(stepamount, tex2D(tex, uv + half2(halfpixel.x, - halfpixel.y) * offset));
    sum += step(stepamount, tex2D(tex, uv - half2(halfpixel.x, - halfpixel.y) * offset));
    return sum/8.0;
}

half4 upsample(half2 uv, half2 halfpixel, sampler2D tex, half offset)
{
    half4 sum = tex2D(tex, uv + half2(-halfpixel.x * 2.0, 0.0) * offset);
    sum += tex2D(tex, uv + half2(-halfpixel.x, halfpixel.y) * offset) * 2.0;
    sum += tex2D(tex, uv + half2(0.0, halfpixel.y * 2.0) * offset);
    sum += tex2D(tex, uv + half2(halfpixel.x, halfpixel.y) * offset) * 2.0;
    sum += tex2D(tex, uv + half2(halfpixel.x * 2.0, 0.0) * offset);
    sum += tex2D(tex, uv + half2(halfpixel.x, -halfpixel.y) * offset) * 2.0;
    sum += tex2D(tex, uv + half2(0.0, - halfpixel.y * 2.0) * offset);
    sum += tex2D(tex, uv + half2 (-halfpixel.x, -halfpixel.y) * offset) * 2.0;
    return sum/12;
}


/*
half4 downsampleDepth(half2 uv, half2 halfpixel, sampler2D tex, float offset )
{
    
    half4 sum = DecodeDepth(tex2D(tex, uv) * 4.0);
    sum += DecodeDepth(tex2D(tex, uv - halfpixel.xy * offset));
    sum += DecodeDepth(tex2D(tex, uv + halfpixel.xy * offset));
    sum += DecodeDepth(tex2D(tex, uv + half2(halfpixel.x, - halfpixel.y) * offset));
    sum += DecodeDepth(tex2D(tex, uv - half2(halfpixel.x, - halfpixel.y) * offset));
    return half4(EncodeDepth(sum/8.0),1);
}

half4 upsampleDepth(half2 uv, half2 halfpixel, sampler2D tex, float offset)
{
   
    half4 sum = DecodeDepth(tex2D(tex, uv + half2(-halfpixel.x * 2.0, 0.0) * offset));
    sum += DecodeDepth(tex2D(tex, uv + half2(-halfpixel.x, halfpixel.y) * offset) * 2.0);
    sum += DecodeDepth(tex2D(tex, uv + half2(0.0, halfpixel.y * 2.0) * offset));
    sum += DecodeDepth(tex2D(tex, uv + half2(halfpixel.x, halfpixel.y) * offset) * 2.0);
    sum += DecodeDepth(tex2D(tex, uv + half2(halfpixel.x * 2.0, 0.0) * offset));
    sum += DecodeDepth(tex2D(tex, uv + half2(halfpixel.x, -halfpixel.y) * offset) * 2.0);
    sum += DecodeDepth(tex2D(tex, uv + half2(0.0, - halfpixel.y * 2.0) * offset));
    sum += DecodeDepth(tex2D(tex, uv + half2 (-halfpixel.x, -halfpixel.y) * offset) * 2.0);
    return half4(EncodeDepth(sum/12),1);
    
}
*/
#endif