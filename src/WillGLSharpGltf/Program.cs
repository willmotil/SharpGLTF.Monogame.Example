﻿using System;

namespace WillGLSharpGltf
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            //using (var game = new Game1()) game.Run();
            using (var game = new Game2()) game.Run();
        }
    }
}
