using System;
using System.Numerics;

namespace RayTracer
{
    public class Light
    {
        public Vector3 pos; // light position
        public Vector3 color; //color of light

        public Light(Vector3 pos, Vector3 color)
        {
            this.pos = pos;
            this.color = color;
        }

        //Construct the shadowray from Intersection.
        public virtual Ray ShadowRay(Intersection I, out bool doesExist)
        {
            float t = (this.pos - I.point).Length(); //length from point to light source
            Vector3 dir = (this.pos - I.point);

            //fix for shadow acne
            t -= 2 * 1E-3f;
            dir = Vector3.Normalize(dir);

            doesExist = true;
            
            return new Ray(I.point += I.normal*1E-3f, dir, t);
        }
    }

    public class SpotLight : Light
    {
        public float angle;
        public Vector3 dir;

        public SpotLight(Vector3 pos, Vector3 color, Vector3 dir, float angle) : base(pos, color)
        {
            this.angle = angle; // the width of the cone
            this.pos = pos;
            this.color = color;
            this.dir = Vector3.Normalize(dir);
        }

        public override Ray ShadowRay(Intersection I, out bool doesExist)
        {
            Vector3 v1 = Vector3.Normalize(pos - I.point);
            float theta = (float)Math.Acos(Vector3.Dot(dir, v1)) * 180 / (float)Math.PI;

            // If the angle between the intersection point and light position is 
            // greater than the spotlight angle, the shadowray doesn't exist.
            if (theta > angle)
            {
                doesExist = false;
                return new Ray();
            }

            else
                return base.ShadowRay(I, out doesExist);
        }
    }
}
