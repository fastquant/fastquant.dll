// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartQuant.Quant
{
    public class Vector
    {
        public int NRows { get; private set; }

        public double[] Elements { get; private set; }

        public double this[int index]
        {
            get
            {
                return Elements[index];
            }
            set
            {
                Elements[index] = value;
            }
        }

        public Vector()
        {
            NRows = -1;
        }

        public Vector(int nrows)
        {
            EnsureNumberPositive(nrows, "Number of rows");
            NRows = nrows;
            Elements = new double[NRows];
        }

        public static double operator *(Vector v1, Vector v2)
        {
            EnsureCompatible(v1, v2);
            return Enumerable.Range(0, v1.NRows).Sum(i => v1[i] * v2[i]);
        }

        public static Vector operator *(Vector vector, double val)
        {
            EnsureValid(vector);
            var v = new Vector(vector.NRows);
            Parallel.For(0, v.NRows, i => v[i] = vector[i] * val);
            return v;
        }

        public static Vector operator +(Vector vector, double val)
        {
            EnsureValid(vector);
            var v = new Vector(vector.NRows);
            Parallel.For(0, v.NRows, i => v[i] = vector[i] + val);
            return v;
        }

        public static Vector operator -(Vector vector, double val)
        {
            EnsureValid(vector);
            var v = new Vector(vector.NRows);
            Parallel.For(0, v.NRows, i => v[i] = vector[i] - val);
            return v;
        }

        public static Vector operator +(Vector target, Vector source)
        {
            EnsureValid(source, "Source vector");
            EnsureValid(target, "Target vector");
            EnsureCompatible(target, source);

            var v = new Vector(target.NRows);
            Parallel.For(0, v.NRows, i => v[i] = target[i] + source[i]);
            return v;
        }

        public static Vector operator -(Vector target, Vector source)
        {
            EnsureValid(source, "Source vector");
            EnsureValid(target, "Target vector");
            EnsureCompatible(target, source);

            var v = new Vector(target.NRows);
            Parallel.For(0, v.NRows, i => v[i] = target[i] - source[i]);
            return v;
        }

        public bool IsValid() => NRows != -1;

        public static bool AreCompatible(Vector v1, Vector v2)
        {
            EnsureValid(v1, "Vector 1");
            EnsureValid(v2, "Vector 2");
            return v1.NRows == v2.NRows;
        }

        public void Zero()
        {
            Parallel.For(0, Elements.Length, i => Elements[i] = 0);
        }

        public void ResizeTo(int newNRows)
        {
            EnsureNumberPositive(newNRows, "Number of rows");
            var newArray = new double[newNRows];
            var num = Math.Min(this.NRows, newNRows);
            Parallel.For(0, Math.Min(NRows, newNRows), i => newArray[i] = Elements[i]);
            NRows = newNRows;
            Elements = newArray;
        }

        public double Norm1()
        {
            EnsureValid(this);
            return Elements.Sum();
        }

        public double Norm2Sqr()
        {
            EnsureValid(this);
            return Elements.Sum(e => e * e);
        }

        public double NormInf()
        {
            EnsureValid(this);
            return Elements.Max(e => Math.Abs(e));
        }

        public Vector Abs()
        {
            EnsureValid(this);
            var v = new Vector(NRows);
            Parallel.For(0, this.NRows, i => v[i] = Math.Abs(this.Elements[i]));
            return v;
        }

        public Vector Sqr()
        {
            EnsureValid(this);
            var v = new Vector(NRows);
            Parallel.For(0, this.NRows, i => v[i] = this.Elements[i] * this.Elements[i]);
            return v;
        }

        public Vector Sqrt()
        {
            EnsureValid(this);
            var v = new Vector(NRows);
            Parallel.For(0, NRows, i => v[i] = Math.Sqrt(this.Elements[i]));
            return v;
        }

        public Vector ElementMult(Vector target, Vector source)
        {
            EnsureValid(source, "Source vector");
            EnsureValid(target, "Target vector");
            EnsureCompatible(target, source);

            var v = new Vector(target.NRows);
            Parallel.For(0, NRows, i => v[i] = target[i] * source[i]);
            return v;
        }

        public Vector ElementDiv(Vector target, Vector source)
        {
            EnsureValid(source, "Source vector");
            EnsureValid(target, "Target vector");
            EnsureCompatible(target, source);

            var v = new Vector(target.NRows);
            Parallel.For(0, NRows, i => v[i] = target[i] / source[i]);
            return v;
        }

        public override bool Equals(object vector)
        {
            var that = (Vector)vector;
            if (NRows != that.NRows)
                return false;
            return Enumerable.Range(0, NRows).All(i => this[i] == that[i]);
        }

        public override int GetHashCode() => base.GetHashCode();

        public void Print() => Print("F2");

        public void Print(string format)
        {
            Console.WriteLine(string.Join(Environment.NewLine, Elements.Select(e => e.ToString(format))));
        }

        private static void EnsureValid(Vector v, string name = "Vector")
        {
            if (!v.IsValid())
                throw new ArgumentException($"{name} is not initialized");
        }

        private static void EnsureNumberPositive(int n, string name)
        {
            if (n <= 0)
                throw new ArgumentException($"{name} has to be positive");
        }

        private static void EnsureCompatible(Vector v1, Vector v2)
        {
            if (!AreCompatible(v1, v2))
                throw new ArgumentException("Vectors are not compatible");
        }
    }
}