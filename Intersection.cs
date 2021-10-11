using System.Numerics;

namespace RayTracer
{
    public struct Intersection
    {
        public float t;                 //distance from camera eye
        public Vector3 point;           //point of intersection
        public IPrimitive primitive;    //primitive from intersection
        public Vector3 normal;          //Normal vector on primitive
        public Material material;       //material at intersection

        public Intersection(float t, Vector3 point, IPrimitive primitive)
        {
            this.t = t;
            this.point = point;
            this.primitive = primitive;
            if (primitive != null)
            {
                this.normal = primitive.Normal(point);
                this.material = primitive.GetMaterial(point);
            }
            else
            {
                this.normal = new Vector3(0, 0, 0);
                this.material = new Material();
            }

        }
    }
}
