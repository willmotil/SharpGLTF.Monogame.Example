using System;

//https://github.com/vpenades/SharpGLTF
//https://github.com/vpenades/SharpGLTF/tree/master/examples
//https://threejs.org/docs/#examples/en/loaders/GLTFLoader
//https://github.com/KhronosGroup/glTF/tree/master/specification/2.0
//https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Platform/Graphics/Effect/Resources/SkinnedEffect.fx
//https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Graphics/Effect/SkinnedEffect.cs
// the git link for the gltf viewer - pixel shader https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag
//
// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/material.normalTextureInfo.schema.json gltf has normalmap scalars in the model humm pretty thourogh.

namespace SharpGLTF.Runtime
{
    public static class Msg
    {
        public static bool Tracking { set; get; } = true;
        public static int track0 = 0;
        public static int track1 = 0;
        public static void TrackStructureLog(string s) { TrackStructureLog(0, s, 0); }
        public static void TrackStructureLog(int linesDown, string s) { TrackStructureLog(linesDown, s, 0); }
        public static void TrackStructureLog(string s, int linesDownAfter) { TrackStructureLog(0, s, linesDownAfter); }
        public static void TrackStructureLog(int linesDownBefore, string s, int linesDownAfter)
        {
            if (Tracking)
            {

                string lines = "";
                for (int i = 0; i < linesDownBefore; i++)
                    lines += "\n";

                string linesAfter = "";
                for (int i = 0; i < linesDownAfter; i++)
                    linesAfter += "\n";

                Console.WriteLine(lines + "(" + track0 + ") " + s + linesAfter);
                track0++;

            }
        }
        public static void TrackStructure1Log(string s) { TrackStructure1Log(0, s, 0); }
        public static void TrackStructure1Log(int linesDown, string s) { TrackStructure1Log(linesDown, s, 0); }
        public static void TrackStructure1Log(string s, int linesDownAfter) { TrackStructure1Log(0, s, linesDownAfter); }
        public static void TrackStructure1Log(int linesDownBefore, string s, int linesDownAfter)
        {
            if (Tracking)
            {

                string lines = "";
                for (int i = 0; i < linesDownBefore; i++)
                    lines += "\n";

                string linesAfter = "";
                for (int i = 0; i < linesDownAfter; i++)
                    linesAfter += "\n";

                Console.WriteLine(lines + "(" + track0 + ") " + "(" + track1 + ") " + s + linesAfter);
                track0++;
                track1++;

            }
        }
    }
}

