using System;
using UnityEngine;

[Serializable]
public class KawaseBlurSettings
{
    public int DownSample = 1;
    public int PassesCount = 1;
    public float BlurOffset = 6f;
}