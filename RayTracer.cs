using System;
using System.Numerics;

namespace RayTracer
{
	public class RayTracer
	{
        public Camera cam;
        public Scene scene;
        public Surface dbScreen, rtScreen;
        public Vector3[] pixels;
        public float dbScreenScale = 20f;
        public bool calculateShading = true;
        public int recursionDepth = 2;
        public Vector3 defaultColor = new Vector3(0, 0, 0);
        public int SSAA = 1; // anti-aliasing


        private int dbScreenSizeX = 512;
        private int dbScreenSizeY = 512;
        private int rtScreenSizeX = 512;
        private int rtScreenSizeY = 512;
        private bool dbScreenOn = true;
        public bool DbScreenOn
        {
            get { return dbScreenOn; }

            set 
            { 
                dbScreenOn = value;
                ResizeScreen(rtScreenSizeX + dbScreenSizeX, rtScreenSizeY);
            }

        }

        public RayTracer(Camera cam, Scene scene)
		{
            //init 
            this.cam = cam;
            this.scene = scene;
        }

        public void ResizeScreen(int width, int height)
        {
            //set new screenSizes
            if(dbScreenOn == false)
            {
                rtScreenSizeX = width;
                rtScreenSizeY = height;
                dbScreenSizeX = 0;
                dbScreenSizeY = 0;
            }
            else
            {
                dbScreenSizeX = width / 2;
                rtScreenSizeX = width - dbScreenSizeX;
                dbScreenSizeY = height;
                rtScreenSizeY = height;
            }

            //update screen
            dbScreen = new Surface(dbScreenSizeX, dbScreenSizeY);
            rtScreen = new Surface(rtScreenSizeX, rtScreenSizeY);
            pixels = new Vector3[rtScreenSizeX * rtScreenSizeY];

            //update cam
            cam.ChangeAspectRatio((float)rtScreenSizeX / rtScreenSizeY);
        }

        // renders one frame
        public void Render(Surface screen)
        {
            dbScreen.Clear(0x00007f);
            pixels = new Vector3[rtScreenSizeX * rtScreenSizeY];    //reset pixels
            for (int y = 0; y < rtScreenSizeY*SSAA; y++)
            {
                for (int x = 0; x < rtScreenSizeX*SSAA; x++)
                {
                    //only true if current ray must be drawn to debug screen
                    bool debugRay = dbScreenOn && y == (rtScreenSizeY*SSAA) / 2 && x % (40*SSAA) == 0;

                    Vector3 color = defaultColor;   //default color
                    float u = (float)x / (rtScreenSizeX*SSAA);
                    float v = (float)y / (rtScreenSizeY*SSAA);
                    Vector3 P = cam.GetP(u, v);

                    // primary ray from camera position through pixel
                    Ray primaryRay = new Ray(cam.pos, Vector3.Normalize(P - cam.pos), float.MaxValue);

                    // first intersection
                    Intersection I = scene.Intersect(primaryRay);

                    //calculate color
                    if (calculateShading)
                    {
                        foreach (Light L in scene.lightSources)
                            color += Trace(primaryRay, I, L, recursionDepth, debugRay);
                    }

                    else if (I.primitive != null)    //for fast rendering
                        color = I.material.Kd;

                    //Debug
                    if (debugRay)
                    {
                        //Draw rays to debug screen
                        primaryRay.t = I.t;
                        DrawRayToDebug(primaryRay, 0xffff00);
                        DrawRayToDebug(new Ray(I.point, I.normal, 1), 0xff0000);    //normal vector
                    }
                    //Add color to correct pixel
                    pixels[y/SSAA * rtScreenSizeX + x/SSAA] += color;
                }
            }

            //Divide pixel colors by SSAA^2 (otherwise every pixel has to much brightness.)
            for (int y = 0; y < rtScreenSizeY; y++)
                for (int x = 0; x < rtScreenSizeX; x++)
                    pixels[y * rtScreenSizeX + x] /= (SSAA*SSAA);

            if (dbScreenOn)
            {
                //Draw Camera to debug
                int[] position = Vector3ToScreenCords(cam.pos);
                dbScreen.Plot(position[0], position[1], 0xff0000);

                //Draw screenplane to debug
                position = Vector3ToScreenCords(cam.p0);
                int[] position1 = Vector3ToScreenCords(cam.p1);
                dbScreen.Line(position[0], position[1], position1[0], position1[1], 0xff0000);

                //Draw spheres to debug
                foreach (IPrimitive primitive in scene.primitives)
                {
                    if (primitive is Sphere)
                        DrawSphereToDebug((Sphere)primitive, 0x00ff00);
                }
            }


            //Finalize screen
            for (int i = 0; i <pixels.Length; i++)
            {
                Vector3 hdr = pixels[i];
                int red = Math.Min((int)(hdr.X * 256), 255);
                int green = Math.Min((int)(hdr.Y * 256), 255);
                int blue = Math.Min((int)(hdr.Z * 256), 255);
                rtScreen.pixels[i] = (red << 16) + (green << 8) + blue;
            }

            if (dbScreenOn)
            {
                dbScreen.CopyTo(screen, 0, 0);
                rtScreen.CopyTo(screen, dbScreenSizeX, 0);
            }
            else
                rtScreen.CopyTo(screen, 0, 0);
        }

        // Calculate diffuse and specular color
        Vector3 DirectIllumination(Ray ray, Intersection I, Light L, bool debugRay = false, bool ommitSpecular = false)
        {
            bool doesExist;
            Ray shadowRay = L.ShadowRay(I, out doesExist);

            //if the shadowRay doesn't exist (for example, the point is not lit by a spotlight), return default color;
            if (!doesExist)  
                return defaultColor;
            if (scene.DoesIntersect(shadowRay))
                return defaultColor;

            Vector3 R = -shadowRay.dir - 2 * Vector3.Dot(-shadowRay.dir, I.normal) * I.normal; //Reflection vector;

            float distanceFactor = 1 / (shadowRay.t * shadowRay.t);

            //Diffuse shading
            Vector3 diffuse = I.material.Kd * Math.Max(0, Vector3.Dot(I.normal, shadowRay.dir));

            //Add glossy shading (ommit if specified for partial reflectivity)
            Vector3 specular = new Vector3(0, 0, 0);
            if (!ommitSpecular)
                specular = I.material.Ks * (float)Math.Pow(Math.Max(0, Vector3.Dot(-ray.dir, R)), I.material.glossiness);

            if (debugRay)
            {
                DrawRayToDebug(shadowRay, 0xffffff);    //draw shadow ray
                Ray reflectionRay = new Ray(I.point, R, 1f);
                DrawRayToDebug(reflectionRay, 0xff9933);    //draw reflection ray
            }

            return distanceFactor * L.color * (diffuse + specular);
        }

        // Calculate color of intersection. Recursive if mirror object.
        Vector3 Trace(Ray primaryRay, Intersection I, Light L, int depth, bool debugRay)
        {
            Vector3 color = new Vector3(0, 0, 0);

            //If ray doesn't hit anything, return color from skydome
            if(I.primitive == null)
                return scene.skydome.GetColor(primaryRay.dir);

            //If reflective, shoot ray again and calculate color.
            if (I.primitive != null && I.material.isMirror && depth > 0)
            {
                // new ray with same angle with respect to the normal
                Vector3 dir = Vector3.Normalize(primaryRay.dir - 2 * Vector3.Dot(primaryRay.dir, I.normal) * I.normal);
                Ray ray = new Ray(I.point + dir * 1e-3f, dir, float.MaxValue);

                // calculate new intersection
                Intersection I0 = scene.Intersect(ray);
                color += I.material.Km * Trace(ray, I0, L, depth - 1, debugRay);

                if (debugRay)
                {
                    //show reflected rays
                    ray.t = I0.t;
                    DrawRayToDebug(ray, 0xff00ff);
                }
            }

            Vector3 ambient = I.material.Ka * scene.ambientLight;
            color += DirectIllumination(primaryRay, I, L, debugRay, I.material.isMirror) + ambient;
            return color;
        }


        // FROM THIS POINT: DEBUG UTILITY METHODS
        private Vector3 TranslateVector3ToDebugBasis(Vector3 v)
        {
            return LinearAlgebra.ChangeBasisFromVector(v - cam.pos, -1 * Vector3.Cross(cam.dir, cam.up), cam.dir, cam.up);
        }

        
        private int[] DebugBasisToScreenCords(Vector3 v)
        {
            return new int[] { (int)(v.X * dbScreenScale + dbScreenSizeX / 2), (int)(-v.Y * dbScreenScale + dbScreenSizeY - 10) };
        }

        //Calculate on which screen-coordinates a vector must be drawn on debug screen.
        private int[] Vector3ToScreenCords(Vector3 v)
        {
            v = TranslateVector3ToDebugBasis(v);
            return DebugBasisToScreenCords(v);
        }

        //Draw a ray to the debug screen.
        private void DrawRayToDebug(Ray r, int color)
        {
            int[] startPos = Vector3ToScreenCords(r.O);
            int[] endPos;
            if (dbScreenScale * r.t > dbScreenSizeX*dbScreenSizeY * 2)
                endPos = Vector3ToScreenCords(r.O + r.dir * 512 * 2); //If ray is to long to fit screen, make it shorter
            else
                endPos = Vector3ToScreenCords(r.O + r.dir * r.t);     //Else, use own length
            dbScreen.Line(startPos[0], startPos[1], endPos[0], endPos[1], color);
        }

        //Draw a spehre to the debug.
        private void DrawSphereToDebug(Sphere s, int color)
        {
            Vector3 pos = TranslateVector3ToDebugBasis(s.pos);

            //Don't draw sphere if not on middle of screen.
            if (Math.Abs(pos.Z) > s.r)
                return;

            //Calculate r of circle
            float r = (float) (Math.Sqrt(s.r * s.r - pos.Z * pos.Z));

            //Draw circle
            int precision = 100;
            float angle1, angle2;
            Vector3 a, b;
            int[] pos1, pos2;
            for (int angle = 0; angle <= precision; angle++)
            {
                angle1 = (float)angle / precision * 2 * (float)Math.PI;
                angle2 = (float)(angle + 1) / precision * 2 * (float)Math.PI;
                a = new Vector3((float)(Math.Cos(angle1) * r + pos.X), (float)(Math.Sin(angle1) * r + pos.Y), 0);
                b = new Vector3((float)(Math.Cos(angle2) * r + pos.X), (float)(Math.Sin(angle2) * r + pos.Y), 0);
                pos1 = DebugBasisToScreenCords(a);
                pos2 = DebugBasisToScreenCords(b);
                dbScreen.Line(pos1[0], pos1[1], pos2[0], pos2[1], color);
            }

        }
    }
}
