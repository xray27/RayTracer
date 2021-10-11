using System.Numerics;

namespace RayTracer
{
    public struct Material
    {
        public Vector3 Kd;  //diffuse color
        public Vector3 Ks;  //specular color
        public Vector3 Ka;  //ambient color
        public Vector3 Km;  //reflection color

        public float glossiness;
        public bool isMirror;

        public Material(
            Vector3 Kd = new Vector3(),
            Vector3 Ks = new Vector3(),
            Vector3 Ka = new Vector3(),
            Vector3 Km = new Vector3(),
            float glossiness = 1)
        {
            this.Kd = Kd;
            this.Ks = Ks;
            this.Ka = Ka;
            this.Km = Km;
            this.glossiness = glossiness;

            if (Km.Length() > 0) this.isMirror = true;
            else this.isMirror = false;
        }
    }
}
