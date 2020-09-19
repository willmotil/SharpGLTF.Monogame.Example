﻿//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float2 AlphaTransform;
float AlphaCutoff;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODO: check https://github.com/KhronosGroup/glTF-Sample-Viewer/issues/267

float ProcessAlphaChannel(float alpha)
{
    // alpha cutoff
    clip((alpha < AlphaCutoff) ? -1 : 1);

    // alpha blend
    return mad(alpha, AlphaTransform.x, AlphaTransform.y);
}