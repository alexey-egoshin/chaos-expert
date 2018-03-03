/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using System.Text.RegularExpressions;

///����� �������� ������ ��� ���������� �������������� �����������
namespace ChaosExpert
{
    /// <remarks>
    /// ����� ��� ���������� ����������� ������������� �������
    /// </remarks>    
    public partial class ChaosLogic
    {
        /// <summary>
        /// ������ ������� ��������������� ��������� ��� ����������� > 1. 
        /// </summary>
        /// <param name="r">���������� ����� �������, ������������ �������� �������</param>        
        /// <param name="RD">������ ���������� ����� �������</param>
        /// <param name="len">����� ������� � �������</param>
        /// <param name="B">������ ����������� ��������</param>
        /// <returns>������ ��������������� ��������� ��� ������� ����������</returns>
        public static float GetCorrelatingIntegralFirst2(float r, ref float[] RD, int len, float[][] B)
        {
            int l = RD.Length;
            float sum = 0;
            int z = 0;
            float d;
            CalculateStart.CreateEvent(l, "r = " + r.ToString(), 0);
            for (int i = 0; i < len - 1; i++)
            {
                for (int j = i + 1; j < len; j++)
                {
                    d = GetPointDistance(B[i], B[j]);
                    if (d < r && d > 0) sum++;
                    RD[z] = d;
                    z++;
                    Calculus.CreateEvent(z, 0);
                }
            }
            //sum = (sum > 0) ? sum : 1;
            return sum / l;
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� ���������� ��� ������ �����������
        /// </summary>
        /// <param name="r">����������, ������������ �������� ���� ����</param>
        /// <param name="RD">������ � ������������ ����� ������� ��� ������ �����������</param>
        /// <returns>������ ��������������� ��������� ��� ������� ����������</returns>
        public static float GetCorrelatingIntegralContinious(float r, float[] RD)
        {
            float sum = 0;
            CalculateStart.CreateEvent(RD.Length, "r = " + r.ToString(), 0);
            for (int i = 0; i < RD.Length - 1; i++)
            {
                if (RD[i] < r && RD[i] > 0) sum++;
                Calculus.CreateEvent(i, 0);
            }
            sum = (sum > 0) ? sum : 1;
            return sum / RD.Length;
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� �������� = 1 
        /// </summary>
        /// <param name="r">���. ���������� ����� �������</param>
        /// <param name="step">�������� ����</param>
        /// <param name="stepCount">���������� ����� (����� ��� ���������� ���������)</param>
        /// <param name="B">������ � �������</param>
        /// <param name="minR">����������� ���������� ����� ������� (�������� ������������ � ���������� � ���������� ���������)</param>        
        /// <param name="k">����������� ����������� ��� ����������� ���� (�������� ������������ � ���������� � ���������� ���������. ������� �� ���������� ������� ���������� �� ����������)</param>
        /// <returns>������ ��������������� ��������� ��� ������ �����������</returns>
        public static float GetCorrelatingDimensionEstimationFirst(float r, ref float step, int stepCount, float[] B, ref float minR, ref float k)
        {
            float[,] C = new float[stepCount, 2];
            CalculateStart.CreateEvent(stepCount, "Dimension = 1", 1);
            Calculus.CreateEvent(0, 1);

            int len = B.Length;
            len = len * (len - 1) / 2;

            //���������� ����� ����� ������ ����� �� ���������
            float[] RD = new float[len];
            //����. ���������� ����� ����� �������
            float maxR = 0;
            //���. ���������� ����� ����� �������
            minR = 1;
            int zeros = 0;
            C[0, 0] = (float)Math.Log10(r);
            C[0, 1] = (float)Math.Log10(GetCorrelatingIntegralFirst(r, B, ref RD, ref maxR, ref minR, ref zeros));
            Calculus.CreateEvent(1, 1);

            //step = 0.1F;            
            float sum = 0;
            float sum2 = 0;
            for (int i = 0; i < RD.Length; i++)
            {
                sum += RD[i];
                sum2 += RD[i] * RD[i];
            }
            sum = (float)(sum - maxR - minR) / (RD.Length - 2);
            sum2 = (float)Math.Sqrt(sum2 - maxR * maxR - minR * minR) / (RD.Length - 2);

            float zr = (float)zeros / RD.Length;
            
            step = (sum - sum2) / stepCount;
            for (int i = 1; i < stepCount; i++)
            {
                C[i, 0] = (float)0.8 * k * (step * i + minR + step);
                C[i, 1] = (float)Math.Log10(GetCorrelatingIntegralContinious(C[i, 0], RD));
                C[i, 0] = (float)Math.Log10(C[i, 0]);
                Calculus.CreateEvent(i + 1, 1);
            }
            System.GC.Collect(GC.GetGeneration(RD));
            KRegressionFloat rg = MathProcess.SimpleRegression(C, 0, stepCount - 1);
            DataProcess.ExportArray(C, 2, "0.csv");
            System.GC.Collect(GC.GetGeneration(C));
            return rg.A;
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� �������� > 1
        /// </summary>
        /// <param name="step">�������� ����</param>
        /// <param name="stepCount">���������� ����� (����� ��� ���������� ���������)</param>
        /// <param name="A">������ � �������</param>
        /// <param name="minR">����������� ���������� ����� ������� (�������� ������ ������������)</param>
        /// <param name="k">����������� ����������� ��� ����������� ���� (�������� ������ ������������)</param>
        /// <returns>������ ��������������� ��������� ��� ������ �����������</returns>
        public static float GetCorrelatingDimensionEstimation(float step, int stepCount, float[] A, int dimension, float minR, float k)
        {
            float[,] C = new float[stepCount, 2];
            float[][] B = GetTimeSeriesDimension(A, dimension);
            CalculateStart.CreateEvent(stepCount, "Dimension = " + dimension.ToString(), 1);
            Calculus.CreateEvent(0, 1);

            //����� ������ ������� B
            int newLen = A.Length - dimension + 1;
            //����� ���� ��� �����
            int len = newLen * (newLen - 1) / 2;
            //���������� ����� ������� ��� ������� ���������
            float[] RD = new float[len];

            C[0, 0] = step;
            C[0, 1] = (float)Math.Log10(GetCorrelatingIntegralFirst2(C[0, 0], ref RD, newLen, B));
            C[0, 0] = (float)Math.Log10(C[0, 0]);
            Calculus.CreateEvent(1, 1);
            for (int i = 1; i < stepCount; i++)
            {
                C[i, 0] = (float)0.8 * k * ((i + 1) * step);
                C[i, 1] = (float)Math.Log10(GetCorrelatingIntegralContinious(C[i, 0], RD));
                C[i, 0] = (float)Math.Log10(C[i, 0]);
                Calculus.CreateEvent(i + 1, 1);
            }
            System.GC.Collect(GC.GetGeneration(RD));
            System.GC.Collect(GC.GetGeneration(B));
            KRegressionFloat rg = MathProcess.SimpleRegression(C, 0, stepCount - 1);
            DataProcess.ExportArray(C, 2, dimension.ToString() + ".csv");
            System.GC.Collect(GC.GetGeneration(C));
            return rg.A;
        }

        /// <summary>
        /// ����������� ��������������� ��������� �� ���������� ����. 
        /// </summary>
        /// <param name="r">��������� ���</param>        
        /// <param name="stepCount">���������� �����</param>
        /// <param name="A">������� ������</param>
        /// <param name="startDimension">��������� ����������� �������� (1-� ������������� �����������)</param>
        /// <param name="endDimension">�������� ����������� ��������</param>
        /// <returns>���������: ������������� ��������, ������������ ����������� �������, ������ ����� ������ ��������. ��������� ��� ��������� ������������ ��������</returns>
        public static CorrelatingDimensionResult GetCorrelatingDimension(float r, int stepCount, float[] A, int startDimension, int endDimension)
        {
            int dimLen = endDimension - startDimension + 2;
            float[,] C = new float[dimLen, 2];
            CalculateStart.CreateEvent(dimLen, "Correlating Integral = null", 2);
            float minR = 1;
            float k = 1;
            C[0, 0] = 1;
            A = DataProcess.NormalizationByAverage(A);
            float step = 0;
            C[0, 1] = GetCorrelatingDimensionEstimationFirst(r, ref step, stepCount, A, ref minR, ref k);
            C[0, 2] = 0;
            Calculus.CreateEvent(1, 2, "Correlating Integral = " + C[0, 1].ToString());
            CorrelatingDimensionResult res = new CorrelatingDimensionResult();            
            for (int dimension = startDimension; dimension <= endDimension; dimension++)
            {
                int d = dimension - startDimension + 1;
                C[d, 0] = dimension;
                C[d, 1] = GetCorrelatingDimensionEstimation(step, stepCount, A, dimension, minR, k);
                Calculus.CreateEvent(d, 2, "Correlating Integral = " + C[d, 1].ToString());               
                if (d >= 2 && C[d - 1, 1] < C[d - 2, 1] && (C[d, 1] - C[d - 1, 1]) < 0.01)
                {
                    float max = C[d - 2, 1];
                    if (C[d - 1, 1] > max) max = C[d - 1, 1];
                    if (C[d, 1] > max) max = C[d, 1];
                    res.correlatingDimension = max;
                    res.maxDimension = (int)(2 * Math.Floor(res.correlatingDimension) + 1);
                    res.points = C;
                    return res;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            res.correlatingDimension = C[endDimension - startDimension + 1, 1];
            res.maxDimension = (int)(2 * Math.Floor(res.correlatingDimension) + 1);
            res.points = C;
            return res;
        }
      
    }      
}
*/