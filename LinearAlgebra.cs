using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace RayTracer
{
    class LinearAlgebra
    {
        //return rotated v around k with angle a
        public static Vector3 RodriguesRotation(Vector3 v, Vector3 k, float a)
        {
            //Source: https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
            return v * (float)Math.Cos(a) + Vector3.Cross(k, v) * (float)Math.Sin(a) + k * Vector3.Dot(k, v) * (1 - (float)Math.Cos(a));

        }

        //Return vector v changed from normal basis to basis {a,b,c}.
        public static Vector3 ChangeBasisFromVector(Vector3 v, Vector3 a, Vector3 b, Vector3 c)
        {
            //Make Transformation Matrix
            Matrix<float> M = Matrix<float>.Build.DenseIdentity(3,3);
            M[0, 0] = a.X; M[0, 1] = b.X; M[0, 2] = c.X;
            M[1, 0] = a.Y; M[1, 1] = b.Y; M[1, 2] = c.Y;
            M[2, 0] = a.Z; M[2, 1] = b.Z; M[2, 2] = c.Z;
            M = M.Inverse();

            //Turn vector to matrix
            Matrix<float> J = Matrix<float>.Build.DenseIdentity(3, 1);
            J[0, 0] = v.X;
            J[1, 0] = v.Y;
            J[2, 0] = v.Z;

            //Do matrix multiplication
            M = M * J;
            
            //Turn back to vector
            return new Vector3(M[0,0],M[1,0],M[2,0]);
        }
    }
}
