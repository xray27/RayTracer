using System;
using System.Numerics;

namespace RayTracer
{
    public interface IPrimitive
    {
        Material GetMaterial(Vector3 point);    //returns the material at this point on primitive.
        Ray Intersect(Ray ray);                 // Change the distance from the ray if it intersects with the primtive.
        Vector3 Normal(Vector3 point);          // returns the normal vector though the given point for the primitive 
    }

    public class Sphere : IPrimitive
    {
        public Vector3 pos;
        public float r;
        public Material material;

        public Sphere(Material material, Vector3 pos, float r)
        {
            this.pos = pos;
            this.r = r;
            this.material = material;
        }

        public Ray Intersect(Ray ray)
        {
            if ((ray.O - pos).Length() > r)   //use fast intersect formula when outside sphere
            {
                //Source: Lecture 3
                Vector3 c = this.pos - ray.O;
                float t = Vector3.Dot(c, ray.dir);

                Vector3 q = c - t * ray.dir;
                float pp = Vector3.Dot(q, q);
                float rr = this.r * this.r;
                if (pp > rr)
                    return ray;
                t -= (float)Math.Sqrt(rr - pp);
                if ((t < ray.t) && (t > 0))
                {
                    ray.t = t;
                    return ray;
                }
                else return ray;
            }
            //Solid when inside sphere.
            ray.t = 0;
            return ray;

        }
        public Vector3 Normal(Vector3 point)
        {
            return Vector3.Normalize(point - this.pos);
        }

        public Material GetMaterial(Vector3 point)
        {
            return material;
        }
    }

    public class Plane : IPrimitive
    {
        public Vector3 norm;
        public Vector3 pos;
        public Material material;
        bool checkerBoard;          //checkerboard pattern only implemented on floor plane.
        public Material material2;

        public Plane(Material material, Vector3 norm, Vector3 pos, bool checkerBoard = false, Material material2 = new Material())
        {
            this.norm = Vector3.Normalize(norm);
            this.pos = pos;
            this.material = material;
            this.checkerBoard = checkerBoard;
            this.material2 = material2;
        }

        public Ray Intersect(Ray ray)
        {
            //Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
            float denom = Vector3.Dot(this.norm, ray.dir);
            if (denom > 1e-6)
            {
                Vector3 p0l0 = this.pos - ray.O;
                float t = Vector3.Dot(p0l0, this.norm) / denom;
                if (t < ray.t)
                    ray.t = t;
                return ray;
            }
            return ray;
        }
        public Vector3 Normal(Vector3 point)
        {
            return -this.norm;
        }

        public Material GetMaterial(Vector3 point)
        {
            if (checkerBoard)
                //generate checkerboard pattern by alternating material.
                if ((Math.Floor(point.X / 2) + Math.Floor(point.Z / 2)) % 2 == 0)
                    return material2;
            return material;
        }
    }
    public class Triangle : IPrimitive
    {
        public Vector3 A;   //Corners of triangle.
        public Vector3 B;
        public Vector3 C;
        public Vector3 normal; //normal vector

        public Material material;
        public Triangle(Material material, Vector3 A, Vector3 B, Vector3 C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.material = material;

            //calculate normal vector
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            this.normal = Vector3.Normalize(Vector3.Cross(AB, AC));
        }
        public Ray Intersect(Ray ray)
        {
            //Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
            //Check if ray and triangle are parallel.
            if (Vector3.Dot(normal, ray.dir) < 1e-6)
                return ray;

            //Distance from origin to plane.
            float D = Vector3.Dot(normal, A);
            float t = (Vector3.Dot(normal, -ray.O) + D) / Vector3.Dot(normal, ray.dir);
            Vector3 P = ray.O + t * ray.dir;

            //Check if triangle is behind the camera;
            if (t < 0)
                return ray;


            //Do inside-out test
            //AB
            if (Vector3.Dot(normal, Vector3.Cross(B - A, P - A)) < 0)
                return ray;

            //BC
            if (Vector3.Dot(normal, Vector3.Cross(C - B, P - B)) < 0)
                return ray;

            //CA
            if (Vector3.Dot(normal, Vector3.Cross(A - C, P - C)) < 0)
                return ray;

            if(t < ray.t)
                ray.t = t;
            return ray;
        }

        public Vector3 Normal(Vector3 point)
        {
            return -normal;
        }

        public Material GetMaterial(Vector3 point)
        {
            return material;
        }
    }
}