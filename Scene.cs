using System.Collections.Generic;
using System.Numerics;

namespace RayTracer
{
    public class Scene
    {
        public List<IPrimitive> primitives;
        public List<Light> lightSources;
        public Vector3 ambientLight;    //color of the ambient light
        public Skydome skydome;

        public Scene(List<IPrimitive> primitives, List<Light> lightSources, Vector3 ambientLight, Skydome skydome)
        {
            this.primitives = primitives;
            this.lightSources = lightSources;
            this.ambientLight = ambientLight;
            this.skydome = skydome;
        }

        //Return the closest intersection with the ray and a primtive in the scene.
        //Intersection primitive is null when there is no intersection
        public Intersection Intersect(Ray ray)
        {
            IPrimitive primitive = null;
            float old_t = ray.t;
            foreach (IPrimitive p in primitives)
            {
                ray = p.Intersect(ray);
                if(ray.t < old_t)   //only update primitive when ray is shorter then before.
                {
                    old_t = ray.t;
                    primitive = p;
                }
            }
            return new Intersection(ray.t, ray.O + ray.dir * ray.t, primitive);
        }

        //Return true if a ray intersect something, otherwhise false
        public bool DoesIntersect(Ray ray)
        {
            float old_t = ray.t;
            foreach (IPrimitive p in primitives)
            {
                ray = p.Intersect(ray);
                if (ray.t < old_t)
                    return true;

            }
            return false;
        }
    }
}
