using System.Numerics;

namespace RayTracer
{
    public struct Ray
    {
        public Vector3 O;           // Ray Origin
        public Vector3 dir;         // Ray Direction
        public float t;             // distance

        public Ray(Vector3 O, Vector3 dir, float t)
        {
            this.O = O;
            this.dir = dir;
            this.t = t;
        }
    }
}
