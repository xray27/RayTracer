using System;
using ImageMagick;  //used for reading hdr image
using System.Numerics;

namespace RayTracer
{
    public class Skydome
    {
        private IPixelCollection<float> pixels;
        float exposure;
        int width;
        int height;

        public Skydome(string file, float exposure)
        {
            this.exposure = exposure;

            var image = new MagickImage(file);
            pixels = image.GetPixels();
            width = image.Width;
            height = image.Height;
        }

        //Return color from Vector pointing to skydome
        //Source: https://en.wikipedia.org/wiki/UV_mapping for a detailed explanation
        public Vector3 GetColor(Vector3 vec)
        {
            vec = Vector3.Normalize(vec);

            float u = 0.5f + (float)(Math.Atan2(vec.X, vec.Z) / (2 * Math.PI));
            float v = 0.5f - (float)(Math.Asin(vec.Y) / Math.PI);

            // check if u or v >= 1. If so, then loop around to 0
            u = u >= 1 ? 0 : u;
            v = v >= 1 ? 0 : v;

            int xPixel = (int)(u * width);
            int yPixel = (int)(v * height);

            IMagickColor<float> pixel = pixels.GetPixel(xPixel, yPixel).ToColor();
            return new Vector3(pixel.R / exposure, pixel.G / exposure, pixel.B / exposure);
        }

    }
}
