// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace FastQuant.Quant
{
    public class Matrix
    {
        private double[,] elms;
        private int m;
        private int n;
        private MatrixDiag diagonal;

        public MatrixDiag Diagonal => this.diagonal;

        public int M => this.m;

        public int N => this.n;

        public int Rows => this.m;

        public int Cols => this.n;

        public double[,] Elements => this.elms;

        public double this[int row, int col]
        {
            get
            {
                return 0 <= row && row < this.M && 0 <= col && col < this.N ? this.elms[row, col] : 0;
            }
            set
            {
                if (0 <= row && row < this.M && 0 <= col && col < this.N)
                    this.elms[row, col] = value;
            }
        }

        public Matrix Inverted => new Matrix(this).Invert();

        public Matrix Transposed => new Matrix(this).Transpose();

        public bool IsSymmetric
        {
            get
            {
                if (this.n != this.m)
                    return false;
                for (int i = 0; i < this.m; ++i)
                    for (int j = 0; j < i; ++j)
                        if (this.elms[i, j] != this.elms[j, i])
                            return false;
                return true;
            }
        }

        public Matrix()
        {
        }

        public Matrix(Matrix matrix)
            : this(matrix.M, matrix.N)
        {
            for (int i = 0; i < this.M; ++i)
                for (int j = 0; j < this.N; ++j)
                    this.elms[i, j] = matrix.elms[i, j];
        }

        public Matrix(int m, int n)
        {
            this.m = m;
            this.n = n;
            this.elms = new double[m, n];
            this.diagonal = new MatrixDiag(this);
        }

        public Matrix(int size)
            : this(size, size)
        {
        }

        public Matrix(int m, int n, double val)
            : this(m, n)
        {
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < n; ++j)
                    this.elms[i, j] = val;
        }

        public Matrix(double[] values)
            : this(1, values.Length)
        {
            for (int i = 0; i < this.N; ++i)
                this.elms[0, i] = values[i];
        }

        public Matrix(double[,] values)
            : this(values.GetLength(0), values.GetLength(1))
        {
            for (int i = 0; i < this.m; ++i)
                for (int j = 0; j < this.n; ++j)
                    this.elms[i, j] = values[i, j];
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (m1.M != m2.M || m1.N != m2.N)
                return new Matrix();
            var matrix = new Matrix(m1.M, m1.N);
            for (int i = 0; i < m1.M; ++i)
                for (int j = 0; j < m1.N; ++j)
                    matrix[i, j] = m1[i, j] + m2[i, j];
            return matrix;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.M != m2.M || m1.N != m2.N)
                return new Matrix();
            var matrix = new Matrix(m1.M, m1.N);
            for (int i = 0; i < m1.M; ++i)
                for (int j = 0; j < m1.N; ++j)
                    matrix[i, j] = m1[i, j] - m2[i, j];
            return matrix;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.N != m2.M)
                return m1;
            var matrix = new Matrix(m1.M, m2.N);
            for (int i = 0; i < m1.M; ++i)
            {
                for (int j = 0; j < m2.N; ++j)
                {
                    double num = 0.0;
                    for (int k = 0; k < m1.N; ++k)
                        num += m1[i, k] * m2[k, j];
                    matrix[i, j] = num;
                }
            }
            return matrix;
        }

        public static Matrix operator /(Matrix m1, Matrix m2)
        {
            if (m1.M != m2.M || m1.N != m2.N)
                return m1;
            var matrix = new Matrix(m1.M, m1.N);
            for (int i = 0; i < m1.M; ++i)
                for (int j = 0; j < m1.N; ++j)
                    matrix[i, j] = m1[i, j] / m2[i, j];
            return matrix;
        }

        public static Matrix operator +(Matrix matrix, double scalar)
        {
            var m1 = new Matrix(matrix.M, matrix.N);
            for (int i = 0; i < matrix.M; ++i)
                for (int j = 0; j < matrix.N; ++j)
                    m1[i, j] = matrix[i, j] + scalar;
            return m1;
        }

        public static Matrix operator -(Matrix matrix, double scalar)
        {
            return matrix + -scalar;
        }

        public static Matrix operator *(Matrix matrix, double Scalar)
        {
            var m1 = new Matrix(matrix.M, matrix.N);
            for (int i = 0; i < matrix.M; ++i)
            {
                for (int j = 0; j < matrix.N; ++j)
                    m1[i, j] = matrix[i, j] * Scalar;
            }
            return m1;
        }

        public static Matrix operator /(Matrix matrix, double Scalar)
        {
            return matrix * (1.0 / Scalar);
        }

        public static bool operator ==(Matrix m1, Matrix m2)
        {
            if (!AreComparable(m1, m2))
                return false;
            for (int i = 0; i < m1.m; ++i)
                for (int j = 0; j < m1.n; ++j)
                    if (m1.elms[i, j] != m2.elms[i, j])
                        return false;
            return true;
        }

        public static bool operator !=(Matrix m1, Matrix m2)
        {
            if (!AreComparable(m1, m2))
                return false;
            for (int i = 0; i < m1.m; ++i)
                for (int j = 0; j < m1.n; ++j)
                    if (m1.elms[i, j] == m2.elms[i, j])
                        return false;
            return true;
        }

        public static bool operator ==(Matrix matrix, double scalar)
        {
            for (int i = 0; i < matrix.m; ++i)
                for (int j = 0; j < matrix.n; ++j)
                    if (matrix.elms[i, j] != scalar)
                        return false;
            return true;
        }

        public static bool operator !=(Matrix matrix, double scalar)
        {
            for (int i = 0; i < matrix.m; ++i)
                for (int j = 0; j < matrix.n; ++j)
                    if (matrix.elms[i, j] == scalar)
                        return false;
            return true;
        }

        public static bool operator <(Matrix matrix, double scalar)
        {
            for (int i = 0; i < matrix.m; ++i)
                for (int j = 0; j < matrix.n; ++j)
                    if (matrix.elms[i, j] >= scalar)
                        return false;
            return true;
        }

        public static bool operator <=(Matrix matrix, double scalar)
        {
            for (int i = 0; i < matrix.m; ++i)
                for (int j = 0; j < matrix.n; ++j)
                    if (matrix.elms[i, j] > scalar)
                        return false;
            return true;
        }

        public static bool operator >(Matrix matrix, double scalar)
        {
            for (int i = 0; i < matrix.m; ++i)
                for (int j = 0; j < matrix.n; ++j)
                    if (matrix.elms[i, j] <= scalar)
                        return false;
            return true;
        }

        public static bool operator >=(Matrix matrix, double Scalar)
        {
            for (int index1 = 0; index1 < matrix.m; ++index1)
                for (int index2 = 0; index2 < matrix.n; ++index2)
                    if (matrix.elms[index1, index2] < Scalar)
                        return false;
            return true;
        }

        public void MakeEigenVectors(Vector d, Vector e, Matrix z)
        {
            int rows = z.Rows;
            double[] elements1 = d.Elements;
            double[] elements2 = e.Elements;
            double[] numArray = new double[rows * rows];
            for (int index = 0; index < rows * rows; ++index)
                numArray[index] = z.Elements[index / rows, index % rows];
            for (int index = 1; index < rows; ++index)
                elements2[index - 1] = elements2[index];
            elements2[rows - 1] = 0.0;
            for (int index1 = 0; index1 < rows; ++index1)
            {
                int num1 = 0;
                int index2;
                do
                {
                    for (index2 = index1; index2 < rows - 1; ++index2)
                    {
                        double num2 = Math.Abs(elements1[index2]) + Math.Abs(elements1[index2 + 1]);
                        if (Math.Abs(elements2[index2]) + num2 == num2)
                            break;
                    }
                    if (index2 != index1)
                    {
                        if (num1++ != 30)
                        {
                            double num2 = (elements1[index1 + 1] - elements1[index1]) / (2.0 * elements2[index1]);
                            double num3 = Math.Sqrt(num2 * num2 + 1.0);
                            double num4 = elements1[index2] - elements1[index1] + elements2[index1] / (num2 + (num2 >= 0.0 ? Math.Abs(num3) : -Math.Abs(num3)));
                            double num5 = 1.0;
                            double num6 = 1.0;
                            double num7 = 0.0;
                            int index3;
                            for (index3 = index2 - 1; index3 >= index1; --index3)
                            {
                                double num8 = num5 * elements2[index3];
                                double num9 = num6 * elements2[index3];
                                num3 = Math.Sqrt(num8 * num8 + num4 * num4);
                                elements2[index3 + 1] = num3;
                                if (num3 != 0.0)
                                {
                                    num5 = num8 / num3;
                                    num6 = num4 / num3;
                                    double num10 = elements1[index3 + 1] - num7;
                                    num3 = (elements1[index3] - num10) * num5 + 2.0 * num6 * num9;
                                    num7 = num5 * num3;
                                    elements1[index3 + 1] = num10 + num7;
                                    num4 = num6 * num3 - num9;
                                    for (int index4 = 0; index4 < rows; ++index4)
                                    {
                                        double num11 = numArray[index4 + (index3 + 1) * rows];
                                        numArray[index4 + (index3 + 1) * rows] = num5 * numArray[index4 + index3 * rows] + num6 * num11;
                                        numArray[index4 + index3 * rows] = num6 * numArray[index4 + index3 * rows] - num5 * num11;
                                    }
                                }
                                else
                                {
                                    elements1[index3 + 1] -= num7;
                                    elements2[index2] = 0.0;
                                    break;
                                }
                            }
                            if (num3 != 0.0 || index3 < index1)
                            {
                                elements1[index1] -= num7;
                                elements2[index1] = num4;
                                elements2[index2] = 0.0;
                            }
                        }
                        else
                            goto label_31;
                    }
                }
                while (index2 != index1);
                continue;
                label_31:
                Error("MakeEigenVectors", "too many iterationsn");
                return;
            }
            for (int index = 0; index < rows * rows; ++index)
                z.Elements[index / rows, index % rows] = numArray[index];
        }

        public void EigenSort(Matrix eigenVectors, Vector eigenValues)
        {
            int rows = eigenVectors.Rows;
            double[] numArray = new double[rows * rows];
            for (int index = 0; index < rows * rows; ++index)
                numArray[index] = eigenVectors.Elements[index / rows, index % rows];
            double[] elements = eigenValues.Elements;
            for (int index1 = 0; index1 < rows; ++index1)
            {
                int index2 = index1;
                double num1 = elements[index1];
                for (int index3 = index1 + 1; index3 < rows; ++index3)
                {
                    if (elements[index3] >= num1)
                    {
                        index2 = index3;
                        num1 = elements[index3];
                    }
                }
                if (index2 != index1)
                {
                    elements[index2] = elements[index1];
                    elements[index1] = num1;
                    for (int index3 = 0; index3 < rows; ++index3)
                    {
                        double num2 = numArray[index3 + index1 * rows];
                        numArray[index3 + index1 * rows] = numArray[index3 + index2 * rows];
                        numArray[index3 + index2 * rows] = num2;
                    }
                }
            }
            for (int index = 0; index < rows * rows; ++index)
                eigenVectors.Elements[index / rows, index % rows] = numArray[index];
        }

        public Matrix EigenVectors(Vector eigenValues)
        {
            if (!this.IsSymmetric)
                throw new NotImplementedException("Not yet implemented for non-symmetric matrix");
            var matrix = new Matrix(this.Rows, this.Cols);
            for (int index1 = 0; index1 < this.Rows; ++index1)
                for (int index2 = 0; index2 < this.Cols; ++index2)
                    matrix[index1, index2] = this[index1, index2];
            eigenValues.ResizeTo(this.Rows);
            var e = new Vector(this.Rows);
            MakeTridiagonal(matrix, eigenValues, e);
            MakeEigenVectors(eigenValues, e, matrix);
            EigenSort(matrix, eigenValues);
            return matrix;
        }

        public void MakeTridiagonal(Matrix a, Vector d, Vector e)
        {
            int num1 = a.m;
            if (a.m != a.n)
                throw new ArgumentException("Matrix to tridiagonalize must be square");
            if (!a.IsSymmetric)
                throw new ArgumentException("Can only tridiagonalise symmetric matrix");
            var numArray1 = new double[this.M * this.N];
            for (int index = 0; index < this.M * this.N; ++index)
                numArray1[index] = a.Elements[index / this.M, index % this.N];
            var elements1 = d.Elements;
            var elements2 = e.Elements;
            for (int index1 = num1 - 1; index1 > 0; --index1)
            {
                int num2 = index1 - 1;
                double d1 = 0.0;
                double num3 = 0.0;
                if (num2 > 0)
                {
                    for (int index2 = 0; index2 <= num2; ++index2)
                        num3 += Math.Abs(numArray1[index1 + index2 * num1]);
                    if (num3 == 0.0)
                    {
                        elements2[index1] = numArray1[index1 + num2 * num1];
                    }
                    else
                    {
                        for (int index2 = 0; index2 <= num2; ++index2)
                        {
                            numArray1[index1 + index2 * num1] /= num3;
                            d1 += numArray1[index1 + index2 * num1] * numArray1[index1 + index2 * num1];
                        }
                        double num4 = numArray1[index1 + num2 * num1];
                        double num5 = num4 < 0.0 ? Math.Sqrt(d1) : -Math.Sqrt(d1);
                        elements2[index1] = num3 * num5;
                        d1 -= num4 * num5;
                        numArray1[index1 + num2 * num1] = num4 - num5;
                        double num6 = 0.0;
                        for (int index2 = 0; index2 <= num2; ++index2)
                        {
                            numArray1[index2 + index1 * num1] = numArray1[index1 + index2 * num1] / d1;
                            double num7 = 0.0;
                            for (int index3 = 0; index3 <= index2; ++index3)
                                num7 += numArray1[index2 + index3 * num1] * numArray1[index1 + index3 * num1];
                            for (int index3 = index2 + 1; index3 <= num2; ++index3)
                                num7 += numArray1[index3 + index2 * num1] * numArray1[index1 + index3 * num1];
                            elements2[index2] = num7 / d1;
                            num6 += elements2[index2] * numArray1[index1 + index2 * num1];
                        }
                        double num8 = num6 / (d1 + d1);
                        for (int index2 = 0; index2 <= num2; ++index2)
                        {
                            double num7 = numArray1[index1 + index2 * num1];
                            double num9;
                            elements2[index2] = num9 = elements2[index2] - num8 * num7;
                            for (int index3 = 0; index3 <= index2; ++index3)
                                numArray1[index2 + index3 * num1] -= num7 * elements2[index3] + num9 * numArray1[index1 + index3 * num1];
                        }
                    }
                }
                else
                    elements2[index1] = numArray1[index1 + num2 * num1];
                elements1[index1] = d1;
            }
            elements1[0] = 0.0;
            elements2[0] = 0.0;
            for (int index1 = 0; index1 < num1; ++index1)
            {
                int num2 = index1 - 1;
                if (elements1[index1] != 0.0)
                {
                    for (int index2 = 0; index2 <= num2; ++index2)
                    {
                        double num3 = 0.0;
                        for (int index3 = 0; index3 <= num2; ++index3)
                            num3 += numArray1[index1 + index3 * num1] * numArray1[index3 + index2 * num1];
                        for (int index3 = 0; index3 <= num2; ++index3)
                            numArray1[index3 + index2 * num1] -= num3 * numArray1[index3 + index1 * num1];
                    }
                }
                elements1[index1] = numArray1[index1 + index1 * num1];
                numArray1[index1 + index1 * num1] = 1.0;
                for (int index2 = 0; index2 <= num2; ++index2)
                {
                    double[] numArray2 = numArray1;
                    int index3 = index2 + index1 * num1;
                    double[] numArray3 = numArray1;
                    int index4 = index1 + index2 * num1;
                    double num3 = 0.0;
                    double num4 = 0.0;
                    numArray3[index4] = num3;
                    double num5 = num4;
                    numArray2[index3] = num5;
                }
            }
            for (int index = 0; index < this.M * this.N; ++index)
                a.Elements[index / this.M, index % this.N] = numArray1[index];
        }

        public static bool AreComparable(Matrix m1, Matrix m2) => m1.m == m2.m && m1.n == m2.n;

        public Matrix Abs()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j => this.elms[i, j] = Math.Abs(this.elms[i, j])));
            return this;
        }

        public Matrix Sqr()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j => this.elms[i, j] = Math.Pow(this.elms[i, j], 2)));
            return this;
        }

        public Matrix Sqrt()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j => this.elms[i, j] = Math.Sqrt(this.elms[i, j])));
            return this;
        }

        public Matrix Apply(TElementPosAction action)
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j => this.elms[i, j] = action.Operation(this.elms[i, j])));
            return this;
        }

        public double RowNorm()
        {
            double val2 = 0.0;
            for (int index1 = 0; index1 < this.m; ++index1)
            {
                double val1 = 0.0;
                for (int index2 = 0; index2 < this.n; ++index2)
                    val1 += Math.Abs(this.elms[index1, index2]);
                val2 = Math.Max(val1, val2);
            }
            return val2;
        }

        public double ColNorm()
        {
            double val2 = 0.0;
            for (int index1 = 0; index1 < this.n; ++index1)
            {
                double val1 = 0.0;
                for (int index2 = 0; index2 < this.m; ++index2)
                    val1 += Math.Abs(this.elms[index2, index1]);
                val2 = Math.Max(val1, val2);
            }
            return val2;
        }

        public double E2Norm()
        {
            double num = 0.0;
            for (int index1 = 0; index1 < this.m; ++index1)
            {
                for (int index2 = 0; index2 < this.n; ++index2)
                    num += Math.Pow(this.elms[index1, index2], 2.0);
            }
            return num;
        }

        public double E2Norm(Matrix m1, Matrix m2)
        {
            double num = 0.0;
            for (int index1 = 0; index1 < this.m; ++index1)
            {
                for (int index2 = 0; index2 < this.n; ++index2)
                    num += Math.Pow(m1.elms[index1, index2] - m2.elms[index1, index2], 2.0);
            }
            return num;
        }

        public Matrix NormByDiag()
        {
            if (this.m != this.n)
            {
                this.Error("NormByDiag", "Not square");
                return this;
            }

            double[] numArray = new double[this.Diagonal.NDiag];
            for (int index = 0; index < this.Diagonal.NDiag; ++index)
            {
                numArray[index] = this.Diagonal[index];
                if (numArray[index] == 0.0)
                {
                    this.Error("NormByDiag", "Zeros in diagonal");
                    return this;
                }
            }
            for (int index1 = 0; index1 < this.m; ++index1)
            {
                for (int index2 = 0; index2 < this.n; ++index2)
                    this[index1, index2] = this[index1, index2] / Math.Sqrt(numArray[index1] * numArray[index2]);
            }
            return this;

        }

        public Matrix Transpose()
        {
            double[,] numArray = new double[this.n, this.m];
            for (int i = 0; i < this.m; ++i)
            {
                for (int j = 0; j < this.n; ++j)
                    numArray[j, i] = this.elms[i, j];
            }
            this.elms = numArray;
            int num = this.n;
            this.n = this.m;
            this.m = num;
            return this;
        }

        public Matrix Invert()
        {
            double det;
            return this.Invert(out det);
        }

        public Matrix Invert(out double det)
        {
            det = 1.0;
            if (this.Rows != this.Cols)
                this.Error("Invert", "#columns != #rows");
            int rows = this.Rows;
            Matrix matrix = new Matrix(rows, 2 * rows);
            for (int index1 = 0; index1 < rows; ++index1)
            {
                for (int index2 = 0; index2 < rows; ++index2)
                    matrix[index1, index2] = this[index1, index2];
            }
            for (int index1 = 0; index1 < rows; ++index1)
            {
                for (int index2 = rows; index2 < 2 * rows; ++index2)
                    matrix[index1, index2] = index1 == index2 - rows ? 1.0 : 0.0;
            }
            for (int index1 = 0; index1 < rows; ++index1)
            {
                double num1 = 0.0;
                int index2 = -1;
                for (int index3 = index1; index3 < rows; ++index3)
                {
                    if (Math.Abs(matrix[index3, index1]) > num1)
                    {
                        num1 = Math.Abs(matrix[index3, index1]);
                        index2 = index3;
                    }
                }
                if (num1 < 1E-35)
                    throw new ArgumentException("Element max value less than required minimum.");
                if (index2 != index1)
                {
                    for (int index3 = index1; index3 < 2 * rows; ++index3)
                    {
                        double num2 = matrix[index1, index3];
                        matrix[index1, index3] = matrix[index2, index3];
                        matrix[index2, index3] = num2;
                    }
                    det = -det;
                }
                for (int index3 = 0; index3 < rows; ++index3)
                {
                    if (index3 != index1)
                    {
                        double num2 = matrix[index3, index1] / matrix[index1, index1];
                        for (int index4 = index1; index4 < 2 * rows; ++index4)
                            matrix[index3, index4] = matrix[index3, index4] - matrix[index1, index4] * num2;
                    }
                }
                double num3 = matrix[index1, index1];
                for (int index3 = index1; index3 < 2 * rows; ++index3)
                    matrix[index1, index3] = matrix[index1, index3] / num3;
                det = det * num3;
            }
            for (int index1 = 0; index1 < rows; ++index1)
            {
                for (int index2 = 0; index2 < rows; ++index2)
                    this[index1, index2] = matrix[index1, rows + index2];
            }
            return this;
        }

        public Matrix MakeSymetric()
        {
            if (this.n != this.m)
            {
                this.Error("MakeSymetric", "Matrix to symmetrize must be square");
                return this;
            }

            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j =>
            {
                this.elms[i, j] = (this.elms[i, j] + this.elms[j, i]) / 2;
                this.elms[j, i] = this.elms[i, j];
            }));
            return this;
        }

        public Matrix UnitMatrix()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j => this.elms[i, j] = i != j ? 0 : 1));
            return this;
        }

        public Matrix HilbertMatrix()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j =>
            {
                if (i == j)
                    this.elms[i, j] = 1.0 / (i + j + 1);
            }));
            return this;
        }

        public Matrix HilbertMatrix2()
        {
            Parallel.For(0, this.m, i => Parallel.For(0, this.n, j =>
            {
                if (i == j)
                    this.elms[i, j] = 1.0 / (i + j + 1);
            }));
            return this;
        }

        public void Print()
        {
            this.Print("F2");
        }

        public void Print(string format)
        {
            for (int i = 0; i < this.M; ++i)
            {
                for (int j = 0; j < this.N; ++j)
                    Console.Write(" " + this[i, j].ToString(format));
                Console.WriteLine("");
            }
        }

        public override bool Equals(object matrix) => this == (Matrix)matrix;

        public override int GetHashCode() => base.GetHashCode();

        protected void Error(string where, string what)
        {
            Console.WriteLine("Matrix: " + where + " : " + what);
        }

        public class TElementPosAction
        {
            public virtual double Operation(double element) => element;
        }
    }


    public class MatrixDiag
    {
        private Matrix matrix;

        public double this[int i]
        {
            get
            {
                if (0 <= i && i < this.NDiag)
                    return this.matrix.Elements[i, i];
                Error("this[]", "Out of boundry");
                return 0.0;
            }
            set
            {
                if (0 <= i && i < this.NDiag)
                    this.matrix.Elements[i, i] = value;
                else
                    Error("this[]", "Out of boundry");
            }
        }

        public int NDiag => Math.Min(this.matrix.N, this.matrix.M);

        public MatrixDiag(Matrix matrix)
        {
            this.matrix = matrix;
        }

        public void Assign(MatrixDiag matrixDiag)
        {
            if (!Matrix.AreComparable(this.matrix, matrixDiag.matrix))
                return;
            Parallel.For(0, NDiag, i => this[i] = matrixDiag[i]);
        }

        protected void Error(string Where, string What)
        {
        }

        public override bool Equals(object matrixDiag) => this.matrix.Equals(((MatrixDiag)matrixDiag).matrix);

        public override int GetHashCode() => base.GetHashCode();
    }
}