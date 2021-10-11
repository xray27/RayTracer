using System.Numerics;
using System.Collections.Generic;
using OpenTK.Input;
using System;

namespace RayTracer
{
    class MyApplication
    {
        Camera cam;
        List<IPrimitive> primitives;
        List<Light> lightSources;
        RayTracer rt;
        public Surface screen;
        private bool disableRender = false;

        private bool needsRender = true;

        // initialize the scene
        public void Init()
        {
            cam = new Camera(new Vector3(12.9f, 6.8f, 36.4f), new Vector3(-0.5f, -0.3f, 0.8f), 60);

            /* You can specify a material based on the following optional parameters.
             * Kd = the color of the diffuse material
             * Ks = the color of the specular reflection
             * Ka = ambient color
             * Km = color of the mirror reflection
             * 
             * For example, in order to get a fully reflective material, 
             * we do not specify Kd and Ks, as the object itself will not have a color
             * We do specify Km and Ka.
             * 
             * To get partial reflectivity, we specify Km, Ka AND Ks, as we want the object
             * to have both a reflection and a color.
             */
            Material m1 = new Material(
                Km: new Vector3(1, 1, 1),
                Ka: new Vector3(0.1f, 0.1f, 0.1f)
                );

            Material m2 = new Material(
                Kd: new Vector3(0.4f, 0.4f, 0),
                Ks: new Vector3(1, 1, 0),
                Ka: new Vector3(0.1f, 0.1f, 0.1f),
                glossiness: 10);

            Material m3 = new Material(
                Kd: new Vector3(1f, 1f, 0),
                Ks: new Vector3(1, 1, 0),
                Ka: new Vector3(0.1f, 0.1f, 0.1f),
                glossiness: 10);

            Material m4 = new Material(
                Kd: new Vector3(0.4f, 0.4f, 0.4f),
                Ks: new Vector3(2f, 2f, 2f),
                Ka: new Vector3(0.1f, 0.1f, 0.1f),
                glossiness: 10);

            //partial reflection
            Material m5 = new Material(
                Kd: new Vector3(235f / 255f, 155f / 255f, 52f / 255f) * 0.6f,
                Km: new Vector3(235f / 255f, 155f / 255f, 52f / 255f),
                Ka: new Vector3(0.1f, 0.1f, 0.1f)
                ); ;

            Material m6 = new Material(
                Kd: new Vector3(0,0,1),
                Ks: new Vector3(0,0, 0.8f),
                Ka: new Vector3(0.1f, 0.1f, 0.1f),
                glossiness: 10);

            Material m7 = new Material(
                Ka: new Vector3(0.1f, 0.1f, 0.1f));


            //pyramid cords
            Vector3 A = new Vector3(-3, -3f, 60);
            Vector3 B = new Vector3(3, -3f, 60);
            Vector3 C = new Vector3(3, -3f, 56);
            Vector3 D = new Vector3(-3, -3f, 56);

            Vector3 T = new Vector3(0, 3, 58);

            Vector3 V1 = new Vector3(-10, -3f, 80);
            Vector3 V2 = new Vector3(10, -3f, 80);
            Vector3 V3 = new Vector3(0, 20, 80);

            // input arbitrary amount of primitives
            primitives = new List<IPrimitive>() {
                new Sphere(m5, new Vector3(10f, 0f, 50f), 3f),
                new Sphere(m2, new Vector3(-8f, 0f, 50f), 3f),
                new Sphere(m1, new Vector3(0, 0f, 67f), 3f),
                new Sphere(m4, new Vector3(0, 4, 58), 1f),

                new Plane(m3, new Vector3(0f, -1f, 0), new Vector3(-0, -3f, 0f), true, m7),

                new Triangle(m1, V1, V2, V3),

                new Triangle(m6, T, B, A),
                new Triangle(m6, B, T, C),
                new Triangle(m6, C, T, D),
                new Triangle(m6, D, T, A)
            };

            // input arbitrary amount of lightsources
            lightSources = new List<Light>(){
                new Light(
                    new Vector3(15, 8, 35f),
                    new Vector3(255f, 0, 255f)
                ),

                new Light(
                    new Vector3(-15, 8, 65f),
                    new Vector3(0,255f,255f)
                ),

                new SpotLight(
                    new Vector3(0, 10, 45f),
                    new Vector3(255f, 255f, 255f) * 2,
                    new Vector3(0, 1.2f, -0.8f),
                    30)
            };

            Scene s = new Scene(primitives, lightSources, new Vector3(0.1f, 0.1f, 0.1f), new Skydome("../../assets/skydomes/sunflowers_8k.hdr", 600000f));
            rt = new RayTracer(cam, s);
        }
        // renders one frame
        public void Tick()
        {
            //only render raytracer if it needs to.
            if (needsRender)
            {

                //draw info to console
                Console.Clear();
                Console.WriteLine("Rendering Frame...");
                Console.WriteLine("Camera Position   | HNJKLI  | {0}", rt.cam.pos);
                Console.WriteLine("Camera Dir        | WASD QE | {0}", rt.cam.dir);
                Console.WriteLine("Camera FOV        | Z C     | {0}", rt.cam.FOV);
                Console.WriteLine("RecusionDepth     | 0 to 9  | {0}", rt.recursionDepth);
                Console.WriteLine("Anti-Aliasing     | - +     | {0}", rt.SSAA);
                Console.WriteLine("Debug screen      | X       | {0}", rt.DbScreenOn);
                Console.WriteLine("Calculate Shading | P       | {0}", rt.calculateShading);
                Console.WriteLine("Disable Rendering | F V     | {0}", disableRender);
                if(!disableRender)
                    rt.Render(screen);
                Console.WriteLine("Done!");
            }
            needsRender = false;
        }

        // catches key presses
        public void KeyboardPress(KeyboardState keyboard)
        {
            needsRender = true;
            //Camera movement
            float speed = 0.7f;
            float rspeed = 0.05f;
            if (keyboard[OpenTK.Input.Key.H])
                rt.cam.MoveCamera(z: speed);
            if (keyboard[OpenTK.Input.Key.N])
                rt.cam.MoveCamera(z: -speed);
            if (keyboard[OpenTK.Input.Key.J])
                rt.cam.MoveCamera(x: -speed);
            if (keyboard[OpenTK.Input.Key.L])
                rt.cam.MoveCamera(x: speed);
            if (keyboard[OpenTK.Input.Key.I])
                rt.cam.MoveCamera(y: speed);
            if (keyboard[OpenTK.Input.Key.K])
                rt.cam.MoveCamera(y: -speed);


            if (keyboard[OpenTK.Input.Key.Q])
                rt.cam.MoveCamera(roll: -rspeed);
            if (keyboard[OpenTK.Input.Key.E])
                rt.cam.MoveCamera(roll: rspeed);
            if (keyboard[OpenTK.Input.Key.A])
                rt.cam.MoveCamera(yaw: -rspeed);
            if (keyboard[OpenTK.Input.Key.D])
                rt.cam.MoveCamera(yaw: rspeed);
            if (keyboard[OpenTK.Input.Key.W])
                rt.cam.MoveCamera(pitch: rspeed);
            if (keyboard[OpenTK.Input.Key.S])
                rt.cam.MoveCamera(pitch: -rspeed);

            if (keyboard[OpenTK.Input.Key.P])
                rt.calculateShading = !rt.calculateShading;

            //change recusion depth
            if (keyboard[OpenTK.Input.Key.Number0])
                rt.recursionDepth = 0;
            if (keyboard[OpenTK.Input.Key.Number1])
                rt.recursionDepth = 1;
            if (keyboard[OpenTK.Input.Key.Number2])
                rt.recursionDepth = 2;
            if (keyboard[OpenTK.Input.Key.Number3])
                rt.recursionDepth = 3;
            if (keyboard[OpenTK.Input.Key.Number4])
                rt.recursionDepth = 4;
            if (keyboard[OpenTK.Input.Key.Number5])
                rt.recursionDepth = 5;
            if (keyboard[OpenTK.Input.Key.Number6])
                rt.recursionDepth = 6;
            if (keyboard[OpenTK.Input.Key.Number7])
                rt.recursionDepth = 7;
            if (keyboard[OpenTK.Input.Key.Number8])
                rt.recursionDepth = 8;
            if (keyboard[OpenTK.Input.Key.Number9])
                rt.recursionDepth = 9;

            //Change dbScreen.
            if (keyboard[OpenTK.Input.Key.X])
                rt.DbScreenOn = !rt.DbScreenOn;

            //Change FOV
            if (keyboard[OpenTK.Input.Key.C])
                rt.cam.FOV += 5;
            if (keyboard[OpenTK.Input.Key.Z])
                rt.cam.FOV -= 5;

            //anti aliasing
            if (keyboard[OpenTK.Input.Key.Minus])
                rt.SSAA = Math.Min(1, rt.SSAA-1);
            if (keyboard[OpenTK.Input.Key.Plus])
                rt.SSAA++;

            if (keyboard[OpenTK.Input.Key.F])
                disableRender = true;
            if (keyboard[OpenTK.Input.Key.V])
                disableRender = false;
        }

        public void OnResize(int width, int height)
        {
            rt.ResizeScreen(width, height);
            needsRender = true;
        }
    }
}
