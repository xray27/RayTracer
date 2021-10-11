using System;
using System.Numerics;

namespace RayTracer
{
    public class Camera
    {
        public Vector3 pos;     // Camera position
        public Vector3 dir;     // Camera direction
        public Vector3 up;     // Upwards direction of camera, has direction from screen center to top of screen.
        private float fov;      //View angle in degrees

        public float width = 2;
        public float height = 2;

        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 SC;      //Screen center

        //property for FOV
        public float FOV { 
            get { return fov; } 
            set 
            { 
                fov = value; 
                CalculateScreenPlane();
            } 
        }

        public Camera(Vector3 pos, Vector3 dir, float fov)
        {
            this.pos = pos;
            this.dir = dir;
            this.fov = fov;

            // calulate the vector orthogonal to the plane that this.dir and [0, -1, 0] makes
            // calculate the vector orthogonal to the plane that the last vector and this.dir makes
            this.up = Vector3.Cross(-this.dir, new Vector3(0,  1,  0));
            this.up = Vector3.Cross(this.dir, this.up);

            CalculateScreenPlane();
        }

        //Calculate the postitions of the screen plane
        private void CalculateScreenPlane()
        {
            dir = Vector3.Normalize(dir);
            up = Vector3.Normalize(up);
            if (Math.Abs(Vector3.Dot(dir, up)) > 0.0001f)
                throw new ArgumentException("Up vector is not orthogonal to direction vector." + Vector3.Dot(dir, up).ToString());

            float d = 1 / (float)Math.Tan(((Math.PI/180) * fov) / 2);   //distance to screen plane.
            SC = this.pos + d * dir;
            Vector3 left = Vector3.Cross(dir, up);   //Vector facing to the left of screen plane;
            p0 = SC + up * height / 2 + left * width / 2;
            p1 = SC + up * height / 2 - left * width / 2;
            p2 = SC - up * height / 2 + left * width / 2;
        }

        //Get postion on surface screen
        public Vector3 GetP(float u, float v)
        {
            return p0 + u * (p1 - p0) + v * (p2 - p0);
        }

        //Move camera relative to the current direction
        public void MoveCamera(float x = 0, float y = 0, float z = 0, float pitch = 0, float yaw = 0, float roll = 0)
        {
            Vector3 left = Vector3.Cross(dir, up);   //Vector facing to the left of screen plane;
            pos += left * -x + up * y + dir * z;
            dir = LinearAlgebra.RodriguesRotation(dir, left, pitch);
            up = LinearAlgebra.RodriguesRotation(up, left, pitch);

            up = LinearAlgebra.RodriguesRotation(up, dir, -roll);
            dir = LinearAlgebra.RodriguesRotation(dir, up, yaw);
            CalculateScreenPlane();
        }

        public void ChangeAspectRatio(float aspectRatio)
        {
            height = width / aspectRatio;
            CalculateScreenPlane();
        }
    }
}
