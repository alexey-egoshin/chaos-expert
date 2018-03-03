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
using ZedGraph;

///����� �������� ������ ��� ���������� �������������� �����������
namespace ChaosExpert
{
    /// <remarks>
    /// ����� ��� ���������� ����������� ������������� �������
    /// </remarks>
        
    public partial class ChaosLogic
    {
        //���������� ���������� ����. ��������, �.�. = 0
        public const long ERR_CORR_INTEGRAL = 999999999999999999;
        //���������� ���������� ����. �����������, �.�. ����. �������� = 0
        public const long ERR_CORR_DIMENSION = -999999999999999999;

        //������������ ������������� ���������� ��� �����, ����� ������� �� ������� �� ������
        public const float E_DEV = 0.6f;

        /// <summary>
        /// ���������� ����� 2-�� ������� � ������������ ����������� x1.Length=x2.Length
        /// </summary>
        /// <param name="x">������ � ������������ 1-� �����</param>
        /// <param name="y">������ � ������������ 2-� �����</param>
        /// <returns>���������� ����� 2-�� �������</returns>
        public static double GetPointDistance(double[] x, double[] y)
        {
            double a = 0;
            double b = 0;
            for (int i = 0; i < x.Length; i++)
            {
                b = x[i] - y[i];
                a += b * b;
            }
            return a;
        }

        public static float GetPointDistance(float[] x, float[] y)
        {
            float b = 0;
            //float a = 0;
            float max = 0;
            for (int i = 0; i < x.Length; i++)
            {
                b = Math.Abs(x[i] - y[i]);
                //a += b;
                if (b > max) max = b;
            }
            return max;
        }

        public static float GetPointDistance(float x, float y)
        {
            float b;
            b = Math.Abs(x - y);
            return b;
        }

        /// <summary>
        /// ������������ ���������� �� ���������� ����. ���������� ������ ��������� ������� �� ������� ���-�� ���������� - ��� (��������)
        /// </summary>
        /// <param name="A">������ � �������</param>
        /// <param name="startDelay">��������� ��� ��������</param>
        /// <param name="endDelay">�������� ��� ��������</param>
        /// <param name="levelsCount">���������� ������� �������� �� ����������� ���������� (��� ������ ��� ������)</param>
        /// <returns>������ � ����������� �������� ���������� ��� ������ �������� ���� � ����������� ��� (������ ��������� �������)</returns>
        public static AutoMutualInfoResult AutoMutualInformation(float[] A, int startDelay, int endDelay, int levelsCount)
        {
            ///�������� ������������� ������������ ��� ��������.
            ///��� ����� ��������� �� levels ������� ���������, � ������� ����������� ��� ������� ���������
            
            //����������� ��� ������� ������
            double[] Levels = new double[levelsCount];
            double[] AMI = new double[endDelay - startDelay + 1];                       
            float max = 0;
            double[,] L12 = new double[levelsCount, levelsCount];
            int[] L = new int[A.Length];
            //������� �������� � ������� ����� ������� ������ ���� ��������� �� ������
            max = A[0];
            float min = A[0];
            for (int i = 0; i < A.Length; i++)
            {
                if (A[i] > max) max = A[i];
                if (A[i] < min) min = A[i];
            }
            float levelStep = (max-min) / levelsCount;
            float leftBound = min;
            float rightBound = leftBound + levelStep;
            //�������� �������
            //double H = 0;
            //����������� ��� ������� ���������
            for (int i = 0; i < levelsCount-1; i++)
            {
                Levels[i] = 0;                
                for (int j = 0; j < A.Length; j++)
                {
                    if (A[j] >= leftBound && A[j] < rightBound)
                    {
                        Levels[i]++;
                        L[j] = i;
                    }
                }                
                Levels[i] /= A.Length;
                //H -= Levels1[i] * Math.Log(Levels1[i], 2);
                leftBound += levelStep;
                rightBound += levelStep;
            }
            int k = levelsCount - 1;
            Levels[k] = 0;
            for (int j = 0; j < A.Length; j++)
            {
                if (A[j] >= leftBound && A[j] <= rightBound)
                {
                    Levels[k]++;
                    L[j] = k;
                }
            }
            Levels[k] /= A.Length;
            //H -= Levels1[k] * Math.Log(Levels1[k], 2);            
            for (int delay = startDelay; delay <= endDelay; delay++)
            {                
                //����������� �������� �� L1 � L2                
                for (int i = 0; i < levelsCount; i++)
                {
                    for (int j = 0; j < levelsCount; j++)
                    {
                        L12[i, j] = 0;
                    }
                }
                for (int i = 0; i < L.Length - delay; i++)
                {
                    L12[L[i], L[i+delay]]++;
                }
                double mutual = 0;
                for (int i = 0; i < levelsCount; i++)
                {
                    for (int j = 0; j < levelsCount; j++)
                    {
                        L12[i, j] /= A.Length-delay;
                        if (Levels[i] > 0 && Levels[j] > 0 && L12[i, j] > 0)
                        {
                            mutual += L12[i, j] * Math.Log(L12[i, j] / (Levels[i] * Levels[j]), 2);
                        }
                    }
                }
                System.GC.Collect(GC.GetGeneration(L12));
                System.GC.Collect(GC.GetGeneration(L));
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                AMI[delay - startDelay] = mutual;
            }
            AutoMutualInfoResult res = new AutoMutualInfoResult();
            res.AMI = AMI;
            //���� 1-� ��������� �������
            res.optimalLag = 0;
            for (int i = 1; i < AMI.Length-1; i++) 
            {
                if (AMI[i] < AMI[i - 1] && AMI[i] < AMI[i + 1])
                {
                    res.optimalLag = i;
                    break;
                }
            }
            return res;
        }



        //************************************************************************************************
        //                                 Correlating Integral Float
        //
        //************************************************************************************************

        /// <summary>
        /// ������ ������ ��������������� ��������� ��� ����������� 1. ��������� ������������ � ������������ ���������� 
        /// ����� ������� �� ����������. �������������� ���������� 0-� ����������. ��� ���������� ����� ������� �����������.
        /// </summary>
        /// <param name="r">���������� ����� �������, ������������ �������� �������</param>
        /// <param name="B">������ � �������</param>
        /// <param name="RD">������ ���������� ����� �������</param>
        /// <param name="maxR">������������ ���������� ����� �������</param>
        /// <param name="minR">����������� ���������� ����� �������</param>
        /// <param name="zeros">���-�� ������� ����������</param>
        /// <returns>������ ��������������� ��������� ��� ������� ����������</returns>
        public static float GetCorrelatingIntegralFirst(float r, float[] B, ref float[] RD, ref float maxR, ref float minR, ref int zeros)
        {
            int len = B.Length;
            int l = RD.Length;
            int sum = 0;
            int z = 0;
            CalculateStart.CreateEvent(l, "r = " + r.ToString(), 0);
            float d;
            //�������������� ����������� ���������� ������� ������� ���������� ���������� ����� �������
            do
            {
                minR = GetPointDistance(B[0], B[z + 1]);
                z++;
            } while (minR == 0);
            z = 0;
            for (int i = 0; i < len - 1; i++)
            {
                for (int j = i + 1; j < len; j++)
                {
                    d = GetPointDistance(B[i], B[j]);
                    if (d == 0) zeros++;                    
                    if (d < r) sum++;
                    RD[z] = d;
                    maxR = (d > maxR) ? d : maxR;
                    minR = (d < minR && d > 0) ? d : minR;
                    z++;
                    Calculus.CreateEvent(z, 0);
                }
            }            
            return (float) sum / l;
        }

        /// <summary>
        /// ������ ������� ��������������� ��������� ��� ����������� > 1 (������������� ������) 
        /// </summary>
        /// <param name="r">���������� ����� �������, ������������ �������� �������</param>
        /// <param name="dimension">����������� ��������</param>
        /// <param name="RD">������ ���������� ����� �������</param>
        /// <param name="len">����� ������� � �������</param>
        /// <param name="B">������ ����������� ��������</param>
        /// <returns>������ ��������������� ��������� ��� ������� ����������</returns>
        public static float GetCorrelatingIntegralFirst2(float r, ref float[] RD, int len, float[] A, int dimension, ref int zeros, int threadCount)
        {
            int lenRD = len * (len - 1) / 2;
            int segmentRD = lenRD / threadCount;
            int reminderRD = lenRD - segmentRD * threadCount;
            int endInd = (int)(2 * len - 1 - Math.Sqrt((1 - 2 * len) * (1 - 2 * len) - 8 * (segmentRD + reminderRD))) / 2;
            endInd -= 1;
            //������
            Thread[] threads = new Thread[threadCount];
            //��������� ����������
            CorrelatingIntegralFirst2Thread[] cis = new CorrelatingIntegralFirst2Thread[threadCount];
            int startInd = 0;
            //������� ��������������� �������
            int[] boundary = new int[threadCount];
            boundary[0] = endInd;
            for (int i = 1; i < threadCount - 1; i++)
            {
                startInd = endInd + 1;
                int a = startInd + 1;
                endInd = (int)(len - 0.5 - Math.Sqrt(len * len + len + 0.25 - 2 * a * len + a * a - a - 2 * segmentRD));
                endInd -= 1;
                boundary[i] = endInd;
            }
            boundary[threadCount - 1] = len - 1;
            //������ ����� �����            
            int segmentLen = (boundary[0] + 1) * len - ((boundary[0] + 2) * (boundary[0] + 1) / 2);
            CalculateStart.CreateEvent(segmentLen + 1, "r = " + r.ToString(), 0);
            cis[0] = new CorrelatingIntegralFirst2Thread(r, ref A, dimension, 0, boundary[0], 0);
            cis[0].RD = new float[segmentLen];
            threads[0] = new Thread(new ThreadStart(cis[0].GetCorrelatingIntegralFirst2));
            threads[0].Start();
            for (int i = 1; i < threadCount; i++)
            {
                startInd = boundary[i - 1] + 1;
                endInd = boundary[i];
                cis[i] = new CorrelatingIntegralFirst2Thread(r, ref A, dimension, startInd, endInd, i);
                int a = startInd + 1;
                int b = endInd + 1;
                int p1 = b - a + 1;
                float p2 = (float)(a + b) / 2;
                p2 = len - p2;
                segmentLen = (int)(p1 * p2);
                cis[i].RD = new float[segmentLen];
                CalculateStart.CreateEvent(segmentLen + 1, "r = " + r.ToString(), i);
                threads[i] = new Thread(new ThreadStart(cis[i].GetCorrelatingIntegralFirst2));
                threads[i].Start();
            }
            for (int i = 0; i < threadCount; i++) threads[i].Join();
            int sum = 0;
            //������� ��� ���������� � ���� ������
            int indexResult = 0;
            zeros = 0;
            for (int i = 0; i < threadCount; i++)
            {
                sum += cis[i].sum;
                zeros += cis[i].zeros;
                cis[i].RD.CopyTo(RD, indexResult);
                indexResult += cis[i].RD.Length;
            }
            //sum = (sum > 0) ? sum : 1;
            return (float)sum / RD.Length;
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� ���������� ��� ������ ����������� (������������� ������)
        /// </summary>
        /// <param name="r">����������, ������������ �������� ���� ����</param>
        /// <param name="RD">������ � ������������ ����� ������� ��� ������ �����������</param>
        /// <param name="threadCount">���-�� �������</param>
        /// <returns>������ ��������������� ��������� ��� ������� ����������</returns>
        public static float GetCorrelatingIntegralContinious(float r, float[] RD, int threadCount)
        {
            int periodIntervalLength = RD.Length / threadCount;
            //������� (�.�. ����� ����� �� ��������� �� ���� �� ����� �������)
            int remainder = RD.Length - periodIntervalLength * threadCount;
            //������ ��� ������� ������ = ����� �������� + �������
            int periodLengthLocal = periodIntervalLength + remainder;
            //������
            Thread[] threads = new Thread[threadCount];
            //��������� ����������
            CorrelatingIntegralContiniousThread[] cis = new CorrelatingIntegralContiniousThread[threadCount];
            int startInd = 0;
            int endInd = periodLengthLocal - 1;
            //������ ����� �����
            CalculateStart.CreateEvent(endInd + 1, "r = " + r.ToString(), 0);
            cis[0] = new CorrelatingIntegralContiniousThread(r, RD, startInd, endInd, 0);
            threads[0] = new Thread(new ThreadStart(cis[0].GetCorrelatingIntegralContinious));
            threads[0].Start();
            for (int i = 1; i < threadCount; i++)
            {
                startInd = endInd + 1;
                endInd = startInd + periodIntervalLength - 1;
                int len = endInd - startInd + 1;
                CalculateStart.CreateEvent(len + 1, "r = " + r.ToString(), i);
                cis[i] = new CorrelatingIntegralContiniousThread(r, RD, startInd, endInd, i);
                threads[i] = new Thread(new ThreadStart(cis[i].GetCorrelatingIntegralContinious));
                threads[i].Start();
            }
            for (int i = 0; i < threadCount; i++) threads[i].Join();
            int sum = 0;
            for (int i = 0; i < threadCount; i++) sum += cis[i].sum;
            //sum = (sum > 0) ? sum : 1;
            return (float)sum / RD.Length;
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� �������� = 1 (������������� ������)
        /// ���������� ������������ ������ ��� ���� ����� ���������� �������� ���� � ���� ��� ���������, ����������� � �������.       
        /// </summary>
        /// <param name="r">���. ���������� ����� �������</param>
        /// <param name="step">�������� ����</param>
        /// <param name="stepCount">���������� ����� (����� ��� ���������� ���������)</param>
        /// <param name="A">������ � �������</param>
        /// <param name="minR">����������� ���������� ����� ������� (�������� ������������ � ���������� � ���������� ���������)</param>                
        /// <param name="threadCount">���-�� �������</param>
        /// <returns>������ ��������������� ��������� ��� ������ �����������</returns>
        public static float GetCorrelatingDimensionEstimationFirst(float r, ref float step, ref int stepCount, float[] A, ref float minR, ref float maxR, int threadCount, out float baseM)
        {
            int len = A.Length;
            len = len * (len - 1) / 2;
            //���������� ����� ����� ������ ����� �� ���������
            float[] RD = new float[len];
            //����. ���������� ����� ����� �������
            maxR = 0;
            //���. ���������� ����� ����� �������
            minR = 1;
            int zeros = 0;                        
            float ci = GetCorrelatingIntegralFirst(r, A, ref RD, ref maxR, ref minR, ref zeros);            
            int startRegres = 0;
            Calculus.CreateEvent(1, 0);            
            //step = minR*1.5F;            
            step = maxR / 3000;
            //���������� ����� ��� ������� ����, ������������� ���������� � ���� ��� ���������� � �������
            //������������ ���������� ��� �������� = 1/20 -� �� ������������� ���������� �� ����������
            baseM = 1.2F;
            int stepCount2 = (int)Math.Floor(Math.Log10(maxR / (20 * step)) / Math.Log10(baseM));
            float[,] C = new float[stepCount2 + 2, 3];
            CalculateStart.CreateEvent(stepCount2, "Dimension = 1", 0);
            Calculus.CreateEvent(0, 0);
            int st = 1;
            for (int i = 0; i < stepCount2-1; i++)
            {
                C[i, 0] = step * (float)Math.Pow(1.2, i);
                C[i, 2] = C[i, 0];
                ci = GetCorrelatingIntegralContinious(C[i, 0], RD, threadCount);                
                if (ci != 0)
                {
                    C[i, 1] = (float)Math.Log10(ci);
                    C[i, 0] = (float)Math.Log10(C[i, 0]);                    
                }
                else
                {
                    startRegres++;                    
                }
                st++;
                Calculus.CreateEvent(i + 1, 0);
            }
            //���� ����� > 1
            if (st > 1)
            {                
                System.GC.Collect(GC.GetGeneration(RD));
                float ci1 = 0;
                float ci2 = 0;
                float ci3 = 0;
                float ci4 = 0;
                float ci_res = 0;
                ci = 1000;
                float eMin = 1000;
                int j = 1;
                for (int i = startRegres+1; i+3 < stepCount2-1; i++)
                {
                    ci1 = (C[i, 1] - C[i - 1, 1]) / (C[i, 0] - C[i - 1, 0]);
                    ci2 = (C[i+1, 1] - C[i, 1]) / (C[i+1, 0] - C[i, 0]);
                    ci3 = (C[i + 2, 1] - C[i + 1, 1]) / (C[i + 2, 0] - C[i + 1, 0]);
                    ci4 = (C[i + 3, 1] - C[i + 2, 1]) / (C[i + 3, 0] - C[i + 2, 0]);
                    ci = (float)(ci1 + ci2 + ci3 + ci4) / 4;
                    ci = ((ci - ci1) * (ci - ci1) + (ci - ci2) * (ci - ci2) + (ci - ci3) * (ci - ci3) + (ci - ci4) * (ci - ci4)) / 4;
                    //������� ����� ������������
                    if (ci < eMin && (C[i, 1] - C[i - 1, 1]) > 0.001 && (C[i + 1, 1] - C[i, 1]) > 0.001 && (C[i + 3, 1] - C[i+2, 1]) > 0.001 && ci4 > 0.1)
                    {
                        ci_res = ci;
                        eMin = ci;
                        j = i;
                    }                                           
                }
                
                KRegressionFloat rg = MathProcess.SimpleRegression(C, j - 1, j + 3);
                C[stepCount2 - 1, 0] = -1;
                C[stepCount2 - 1, 1] = j;
                C[stepCount2, 0] = 0;
                C[stepCount2, 1] = zeros;
                //������ ���� - � ����, � �������� ���������� �������� ������� (��� ��� ������ - ���)
                step = C[j - 1, 2] * 0.9f;
                //����������� ���� ��� ���������� � ������� ��� ���������� ���� ��� ��������� ���-�� �����
                baseM = (float)Math.Exp(Math.Log(maxR / (20 * step), Math.E) / stepCount);
                //��������� ���-�� �����, �.�. ��������� ����� �������� "�����������"
                stepCount = (int)(stepCount * 5 / 6);
                DataProcess.ExportArray(C, 2, "0.csv");
                System.GC.Collect(GC.GetGeneration(C));
                return rg.A;                                
            }
            else
            {                
                //��������� �������� ������
                return ERR_CORR_INTEGRAL;
            }
        }

        /// <summary>
        /// ������ ��������������� ��������� ��� ����������� �������� > 1 (������������� ������)
        /// </summary>
        /// <param name="step">�������� ����</param>
        /// <param name="stepCount">���������� ����� (����� ��� ���������� ���������)</param>
        /// <param name="A">������ � �������</param>
        /// <param name="dimension">�����������, ��� ������� ������������ ������</param>
        /// <param name="minR">����������� ���������� ����� ������� (�������� ������ ������������)</param>        
        /// <param name="threadCount">���-�� �������</param>
        /// <param name="allCD">��� ����. ��������� ��� ������� �������� ����������� (�������������� �������� ��� ������������� ����� ������)</param>
        /// <param name="zeros">���������� ����� �����������, �.�. ������� ����������(�������������� �������� ��� ������������� ����� ������)</param>
        /// <param name="stepNum">� ����, ��������� ��� ������� ��������</param>
        /// <param name="baseM">��������� ��� ���������� � ������� ��� ��������� ����</param>
        /// <returns>������ ��������������� ��������� ��� ������ �����������</returns>
        public static float GetCorrelatingDimensionEstimation(float step, int stepCount, float[] A, int dimension, float minR, int threadCount, ref float[,] allCD, ref int zeros, int stepNum, float baseM)
        {
            float[,] C = new float[stepCount+1, 2];            
            CalculateStart.CreateEvent(stepCount, "Estimation" + dimension.ToString(), 0);
            Calculus.CreateEvent(0, 0);
            //����� ������ ������� B
            int newLen = A.Length - dimension + 1;
            //����� ���� ��� �����
            int len = newLen * (newLen - 1) / 2;
            //���������� ����� ������� ��� ������� ���������
            float[] RD = new float[len];
            C[0, 0] = step;
            zeros = 0;
            float ci = GetCorrelatingIntegralFirst2(C[0, 0], ref RD, newLen, A, dimension, ref zeros, threadCount);
            int startRegres = 0;
            if (ci != 0)
            {
                C[0, 0] = (float)Math.Log10(C[0, 0]);
                C[0, 1] = (float)Math.Log10(ci);                
            }
            else
            {
                //������
                //return ERR_CORR_INTEGRAL;
                startRegres = 1;                
            }
            allCD[stepNum, 0] = ci;
            Calculus.CreateEvent(1, 0);
            int st = 1;
            for (int i = 1; i < stepCount; i++)
            {
                C[i, 0] = step * (float)Math.Pow(baseM, i);
                ci = GetCorrelatingIntegralContinious(C[i, 0], RD, threadCount);
                if (ci != 0)
                {
                    C[i, 1] = (float)Math.Log10(ci);
                    C[i, 0] = (float)Math.Log10(C[i, 0]);                    
                }
                else
                {
                    startRegres++;                    
                }
                allCD[stepNum, i] = ci;
                st++;
                Calculus.CreateEvent(i + 1, 0);
            }
            //���� ����� > 1
            if (st > 1)
            {
                System.GC.Collect(GC.GetGeneration(RD));                
                //����. ����������� �� 5-�� ������
                float cr1 = 0;
                float cr2 = 0;
                float cr3 = 0;
                float cr4 = 0;
                float cr = 1000;
                float eMin = 1000;
                int j = 0;
                if (startRegres == 0) startRegres++;
                for (int i = startRegres; i + 3 < stepCount; i++)
                {                    
                    float ci1 = C[i, 1] - C[i - 1, 1];
                    float ci2 = C[i + 1, 1] - C[i, 1];
                    float ci3 = C[i + 2, 1] - C[i + 1, 1];
                    float ci4 = C[i + 3, 1] - C[i + 2, 1];
                    cr1 = ci1 / (C[i, 0] - C[i - 1, 0]);
                    cr2 = ci2 / (C[i + 1, 0] - C[i, 0]);
                    cr3 = ci3 / (C[i + 2, 0] - C[i + 1, 0]);
                    cr4 = ci4 / (C[i + 3, 0] - C[i + 2, 0]);
                    cr = (float)(cr1 + cr2 + cr3 + cr4) / 4;                    
                    //������������� ����������
                    float e1 = Math.Abs(cr - cr1) / cr;
                    float e2 = Math.Abs(cr - cr2) / cr;
                    float e3 = Math.Abs(cr - cr3) / cr;
                    float e4 = Math.Abs(cr - cr4) / cr;

                    if (e1 > E_DEV || e2 > E_DEV || e3 > E_DEV || e4 > E_DEV)
                    {
                        //���� �� ����� �� ����� �� ������
                        //��������� �������� ������, ����� ��������� ���������� �����
                        //return ERR_CORR_INTEGRAL;
                    }
                    else
                    {
                        //���� ����. ����� = 0, ������ ������� ��������� ����������
                        if (C[i - 1, 1] != 0)
                        {
                            float e_sum = e1 + e2 + e3 + e4;
                            ///���������� �����, � ������� ��� ���� ������ ����������� ������������� ����������
                            ///� ��� ����� ��������������� ���� ���� ����� �� ��������� �������� (�.�. ��� ���������� ������� ���� ������� ��������������
                            ///�������)
                            float minH1 = Math.Abs(ci1 / C[i - 1, 1]);
                            float minH2 = Math.Abs(ci2 / C[i, 1]);
                            float minH3 = Math.Abs(ci3 / C[i + 1, 1]);
                            float minH4 = Math.Abs(ci3 / C[i + 2, 1]);
                            if (e_sum < eMin && ci1 > 0 && minH1 > 0.01 && ci2 > 0 && minH2 > 0.01 && ci3 > 0 && minH3 > 0.01 && ci4 > 0 && minH4 > 0.01)
                            {
                                eMin = e_sum;
                                j = i;
                            }
                        }
                    }
                }                
                //��������� �������� ������, ����� ��������� ���������� �����
                if (j == 0) {
                    DataProcess.ExportArray(C, 2, dimension.ToString() + ".csv");
                    return ERR_CORR_INTEGRAL;
                }
                //�������� ��������� ����� �����, � ������� ������������� ���������� �� �������� ����������
                KRegressionFloat rg = MathProcess.SimpleRegression(C, j - 1, j + 3);
                C[stepCount, 0] = -1;
                C[stepCount, 1] = j;
                DataProcess.ExportArray(C, 2, dimension.ToString() + ".csv");
                System.GC.Collect(GC.GetGeneration(C));
                return rg.A;
            }
            else
            {
                //��������� �������� ������
                return ERR_CORR_INTEGRAL;
            }            
        }
        
        /// <summary>
        /// ����������� ��������������� ��������� �� ���������� ���� (������������� ������).
        /// ����� ����� ����������:
        /// 1. GetCorrelatingDimensionEstimationFirst:
        /// ���������� ����������� � ������������ ���������� �� ����������. ���������� ������� ���� [������ �. ����������������� ����, �. 126] 
        /// � �������� ��������� ��� ������������� ��������� ����. ������������ ���������� �� ��������� 
        /// ������������_����������_��_����������/15. ����� ��������� ������ ����, � ���� ��� ���������, ����������� � �������.
        /// ���������� ����� �������� �������. ����������� ��������: 12-25. ���������� ������������� ����������, �� ���� ��������� ���.
        /// ����� ���������� � ����� ����������� ����. �����������, �� ��� ��� ���� ���� ��� �� ����� ����������� � ������ 6 ��� 
        /// ��� ����������������� (���� ��� ����������, ������ ����� ��������� ���������� �����, �� ������� �������� ���������).
        /// 
        /// ��� �������� ������� � ����������� ����������� ���������� ����� �� �������� ������� ����� ����������� (���������� ������)
        /// </summary>
        /// <param name="r">��������� ���</param>                
        /// <param name="stepCount">���������� �����</param>
        /// <param name="A">������� ������</param>
        /// <param name="startDimension">��������� ����������� �������� (1-� ������������� �����������)</param>
        /// <param name="endDimension">�������� ����������� ��������</param>
        /// <param name="threadCount">���-�� �������</param>
        /// <returns>���������: ������������� ��������, ������������ ����������� �������, ������ ����� ������ ��������. ��������� ��� ��������� ������������ ��������</returns>
        public static CorrelatingDimensionResult GetCorrelatingDimension(float r, int stepCount, float[] A, int startDimension, int endDimension, int threadCount)
        {
            int dimLen = endDimension - startDimension + 2;
            float[,] C = new float[dimLen, 3];
            CalculateStart.CreateEvent(dimLen, "Correlating Dimension = 0", threadCount);
            float minR = 1;
            float maxR = 0;
            C[0, 0] = 1;
            //������ ���� ������������ � GetCorrelatingDimensionEstimationFirst
            float step = 0;
            CorrelatingDimensionResult res = new CorrelatingDimensionResult();
            //��������� ��� ���������� � ������� ��� ��������� ���������� ���� ������������ � GetCorrelatingDimensionEstimationFirst
            float baseM=0;
            C[0, 1] = GetCorrelatingDimensionEstimationFirst(r, ref step, ref stepCount, A, ref minR, ref maxR, threadCount, out baseM);
            if (C[0, 1] == ERR_CORR_INTEGRAL)
            {
                res.correlatingDimension = ERR_CORR_DIMENSION;
            }
            Calculus.CreateEvent(1, threadCount, "Correlating Dimension = " + C[0, 1].ToString()+" minR="+minR.ToString()+" maxR="+maxR.ToString());
            float[,] allCD = new float[dimLen, stepCount];
            int d=0;
            int i = 0;
            int zeros=0;
            res.dimension_zero = 0;
            int g = 0;
            float maxCorrDim = 0;
            int dimension;
            int notZero = -1;
            int dimForMaxCorr = 1;
            for (dimension = startDimension; dimension <= endDimension; dimension+=1)
            {
                zeros = 0;
                d = dimension - startDimension + 1;
                C[i, 0] = dimension;
                C[i, 1] = GetCorrelatingDimensionEstimation(step, stepCount, A, dimension, minR, threadCount, ref allCD, ref zeros, i, baseM);
                C[i, 2] = zeros;
                if (C[i, 1] == ERR_CORR_INTEGRAL) break;
                Calculus.CreateEvent(i, threadCount, "Correlating Dimension = " + C[i, 1].ToString() + " Dimension: " + dimension.ToString()+" Zeros: "+zeros.ToString());
                //���� ���������� ��������������� �� ���������� = 0
                if (zeros == 0 && res.dimension_zero == 0)
                {
                    res.dimension_zero = dimension;
                    notZero = 1;
                }
                if (C[i, 1] > maxCorrDim)
                {
                    maxCorrDim = C[i, 1];
                    dimForMaxCorr = dimension;
                }
                
                if ((res.dimension_zero > 0 && d > 1 && (C[i, 1] < C[i - 1, 1])) || (res.dimension_zero > 0 && d > 1 && (C[i, 1] - C[i - 1, 1]) < 0.0001))
                {
                    g++;
                    //���� ���������� ���������� ��� ��������������� ������ 6 �������
                    if (g > 6) break;                    
                }
                else
                {
                    if (g > 0) g--;
                }
                i++;
                //����������� ������, ������� � ����������� �������
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            if (C[i, 1] == ERR_CORR_INTEGRAL)
            {
                d -= 1;
                res.errorCode = ERR_CORR_INTEGRAL;
            }
            else
            {
                res.errorCode = 0;
                i--;
            }
                        
            //������ ����. �����������
            res.correlatingDimension = maxCorrDim;
            //������������ ����������� �������
            res.maxDimension = (int)(2 * Math.Floor(res.correlatingDimension) + 1);
            //��������� �����������
            res.startDimension = startDimension;
            //�������� �����������
            res.endDimension = endDimension;
            //�� ����� ������� �������� �����
            res.lastDimension = dimension-1;
            //����������� ��������, ��� ������� ����������� �������� ����. �����������
            res.maxCorrDimension = dimForMaxCorr;
            //����� ����. �����������
            res.points = C;
            res.step = step;
            //���. ���������� �� ����������
            res.minR = minR;
            //����. ���������� �� ����������
            res.maxR = maxR;
            //���������� ����������� �� ��������� ��������
            res.crossers_count = (notZero > 0) ? zeros : notZero;
            //������� ����. ��������
            float[,] allK = new float[i, stepCount];                        
            float[] KE = new float[i-1];
            //�� ��������� ����� ����� ��������� ���
            int stepEntNum = 0;
            for (d = 1; d < i; d++)
            {
                for (int z = 0; z < stepCount; z++) 
                {
                    if (allCD[d, z] > 0)
                    {
                        allK[d, z] = (float)Math.Log(allCD[d - 1, z] / allCD[d, z]);
                    }
                    else
                    {
                        allK[d, z] = -1;
                        stepEntNum = z + 1;
                    }
                }                
            }
            for (d = 1; d < i; d++) KE[d - 1] = allK[d, stepEntNum];
            DataProcess.ExportArray(allK, stepCount, "allK.csv");
            //������ �������� �����������
            //������� �������� �������� �������������� �������
            float minE = 1000;
            int indE = 0;
            for (d = 2; d < i-3; d++)
            {
                float avr = (KE[d]+KE[d+1]+KE[d+2])/3;
                float e = (Math.Abs(KE[d] - avr) + Math.Abs(KE[d + 1] - avr) + Math.Abs(KE[d + 2] - avr)) / avr;
                if (e < minE && KE[d] > KE[d + 1] && KE[d+2] > KE[d + 1])
                {
                    minE = e;
                    indE = d;
                }
            }
            //��� ����� ��������� ��������, ���� ��������� ������� ���
            if (i>=2) res.entropyK = (indE>0) ? (float)(KE[indE] + KE[indE + 1] + KE[indE + 2]) / 3 : KE[i-2];
            System.GC.Collect(GC.GetGeneration(KE));
            System.GC.Collect(GC.GetGeneration(allCD));
            return res;
        }

        /// <summary>
        /// ����������� ������������� ���������� ��������
        /// </summary>
        /// <param name="A">�������� ���</param>
        /// <param name="embedingDimension">����������� �������� (��� ������������� ���������� ��)</param>
        /// <param name="lag">��������� ��� (��� ������������� ���������� ��)</param>
        /// <param name="count">���������� ��� ������ ���������� (�� ������ ��������� �����)</param>
        /// <returns>����������� ������ ���������� ��������</returns>
        public static float GetMaxLyapunovExponent(float[] A, int embedingDimension, int lag, int count)
        {
            //�������������� ���������
            //����� ���������� ������������������� ����������
            int lenAll = (int)Math.Floor((double)(A.Length / (lag + 1)));
            int len = lenAll - embedingDimension + 1;
            int countLag = lenAll / (lag + 1);
            //���������� ���������� � ������� ������������
            float[] B = new float[lenAll];            
            int i;
            int z = 0;
            //��������������� ��������� � ������� ������������ �� ��������� ���� (�����������)
            for (i = 0; i < A.Length - lag -1; i += lag + 1)
            {
                B[z] = A[i];
                z++;
            }
            //���������� ����������� � ������������ ���������� ����� ������� �� ����������
            float minR = 10000000;
            float maxR = 0;            
            float d = 0;
            float max;
            CalculateStart.CreateEvent(len - 2, "Lyapunov = 0", 0);
            for (int k = 0; k < len - 1; k++)
            {
                for (int j = k + 1; j < len; j++)
                {
                    //���������� ���������� ����� �������
                    max = 0;
                    for (int dim = 0; dim < embedingDimension; dim++)
                    {
                        d = Math.Abs(B[k+dim] - B[j+dim]);
                        if (d > max) max = d;
                    }
                    d = max;                    
                    if (d > maxR) maxR = d;
                    if (d < minR) minR = d;                    
                }
                Calculus.CreateEvent(k, 0);
            }

            //���������� ����������� ����������, ������ �������� ������ ���� ���������� ����� ���������� �������
            //��� 1/3000-� �� ������� ����������
            //float minD = maxR /3000;            
            float minD = maxR / 500;
            float maxD = maxR / 10;            
                                    
            //������ ���������� ������, �� �������� ���������� ������ minD;
            int nearestInd = -1;            
            int startInd = 0;
            i = 0;
            z = 0;
            CalculateStart.CreateEvent(len, "Lyapunov = 0", 1);
            float[,] S = new float[count, 5];
            int c = 0;
            float sLyapunov;
            float sTime;
            float tmp;
            bool isBreak;
            //������� ��� ��������� �����
            float sumLyap = 0;
            float[,] Lyapunovs = new float[len,3];                        
            while (i < len - 100)
            {
                //������ ��������� ����� ����������, ���������� �� ����������� ����������,
                //����� ��� ������� ���� ����� ���������
                int indLastPoint = -1;
                //���� ����� ��������� (������������� ������ maxD)
                float angle = 1;
                float minDeltaAngel = 4;

                startInd = i;
                tmp = 0;
                c = 0;
                //���� ���������� ����� - ����� ����� ��� ���
                isBreak = false;
                //���� �� ��������� ����� �� ��������� ��������
                double sumL = 0;
                while (c < count && startInd < len - 100)
                {
                    nearestInd = -1;
                    sLyapunov = 0;
                    sTime = 0;                                        
                    //���� ������ ������
                    for (int j = startInd + (embedingDimension * embedingDimension); j < len - (embedingDimension * embedingDimension); j++)
                    {
                        //���������� ���������� ����� ������� (��� �������� ����� ��������� ������ �������)
                        max = 0;
                        for (int dim = 0; dim < embedingDimension; dim++)
                        {
                            d = Math.Abs(B[startInd + dim] - B[j + dim]);
                            if (d > max) max = d;
                        }
                        d = max;
                        if (d < minD)
                        {
                            //���� ���� ������, ����� �������� �� ����������,
                            //�� ���������� ������� � ���������� ����������� ������� �� ����
                            if (indLastPoint > -1)
                            {                                
                                //��������� ������������ ����� ���������
                                float scalarProduct = 0;
                                float moduleVectorJ = 0;
                                float moduleVectorLast = 0;
                                for (int dim = 0; dim < embedingDimension; dim++)
                                {
                                    scalarProduct += B[j + dim] * B[indLastPoint + dim];
                                    moduleVectorJ += B[j + dim] * B[j + dim];
                                    moduleVectorLast += B[indLastPoint + dim] * B[indLastPoint + dim];
                                }
                                //���� ����� ���������
                                float newAngle = (float)Math.Acos(scalarProduct / Math.Sqrt(moduleVectorJ * moduleVectorLast));
                                float deltaAngle = Math.Abs(angle - newAngle);
                                if (deltaAngle < minDeltaAngel && j < len - embedingDimension - 7)
                                {
                                    minDeltaAngel = deltaAngle;
                                    nearestInd = j;
                                }
                            }
                            //������ ����� �� ����������
                            else
                            {
                                nearestInd = j;
                                //�������� ����� ������
                                goto find_neighbor;
                            }
                        }
                    }                    
                find_neighbor:
                    //��������� ����� ������
                    if (nearestInd > 0)
                    {
                        //����������� ���������� ����� ������� � ��������� startInd � nearestInd
                        //���� ��� ������ maxD
                        //��������� ���������� ����� �������
                        S[c, 0] = d;
                        //����� (���-�� �����), ����� �������� ��� ����� ���������� �� ����������� ������������ ����������
                        int timeCount = 0;
                        int beginStart = startInd;
                        while (d < maxD && timeCount < 5000 && startInd < len - (embedingDimension * embedingDimension) && nearestInd < len - (embedingDimension * embedingDimension))
                        {
                            startInd++;
                            nearestInd++;
                            //���������� ���������� ����� �������
                            max = 0;
                            for (int dim = 0; dim < embedingDimension; dim++)
                            {
                                d = Math.Abs(B[startInd + dim] - B[nearestInd + dim]);
                                if (d > max) max = d;
                            }
                            d = max;
                            timeCount++;
                        }
                        //���� ������� ������� �����������
                        if (timeCount > 7)
                        {
                            //���� ��� ������ ������������ ����� ���������� ���� ����� ���������
                            if (indLastPoint == -1)
                            {
                                //��������� ������������ ����� ���������
                                float scalarProduct = 0;
                                float moduleVectorJ = 0;
                                float moduleVectorLast = 0;
                                for (int dim = 0; dim < embedingDimension; dim++)
                                {
                                    scalarProduct += B[startInd + dim] * B[nearestInd + dim];
                                    moduleVectorJ += B[startInd + dim] * B[startInd + dim];
                                    moduleVectorLast += B[nearestInd + dim] * B[nearestInd + dim];
                                }
                                //���� ����� ���������
                                angle = (float)Math.Acos(scalarProduct / Math.Sqrt(moduleVectorJ * moduleVectorLast));
                            }
                            //���������� ������ �����
                            indLastPoint = nearestInd;
                            //�������� ���������� ����� �������
                            S[c, 1] = d;
                            //�����, ������� ������������� ��� ���������� ����������
                            S[c, 2] = timeCount;
                            S[c, 3] = beginStart;
                            S[c, 4] = nearestInd;                            
                            //����� ��������� ����� - ��������� �����
                            //startInd = nearestInd;                        
                            c++;
                        }
                        else if (timeCount == 0)
                        {
                            startInd++;
                        }
                        //DataProcess.ExportArray(S, "_S"+c.ToString()+".csv");
                    }
                    //��������� ����� �� ������
                    else
                    {
                        //���� ���� �� ���� �������� ��������
                        if (c > 0)
                        {
                            //DataProcess.ExportArray(S, "_lyapunovs" + i + ".csv");
                            sLyapunov = 0;
                            sTime = 0;
                            sumL = 0;
                            //��������� ���������� �������� ��� ������ ��������� �����
                            for (int j = 0; j < c; j++)
                            {
                                tmp = (float)Math.Log((double)(S[j, 1] / S[j, 0]));
                                //tmp = (float)Math.Log((double)(S[j, 1] / S[j, 0]));
                                sLyapunov += tmp;
                                sTime += S[j, 2];
                                sumL += S[j, 1];
                                S[j, 0] = 0;
                                S[j, 1] = 0;
                                S[j, 2] = 0;
                                S[j, 3] = 0;
                                S[j, 4] = 0;
                            }
                            Lyapunovs[z,0] = sLyapunov / sTime;
                            Lyapunovs[z, 1] = sTime;
                            Lyapunovs[z, 2] = (float)Math.Log(sumL);
                            sumLyap += Lyapunovs[z,0];
                            z++;
                        }
                        isBreak = true;
                        //������� � ��������� � ��������� ���. �����
                        break;
                    }
                }
                //���� ���� ��������� �� �����
                if (!isBreak)
                {
                    //���� ���� �� ���� �������� ��������
                    if (c > 0)
                    {
                        //DataProcess.ExportArray(S, "_lyapunovs" + i + ".csv");                        
                        sLyapunov = 0;
                        sTime = 0;
                        sumL = 0;
                        //��������� ���������� �������� ��� ������ ��������� �����
                        for (int j = 0; j < c; j++)
                        {
                            tmp = (float)Math.Log((double)(S[j, 1] / S[j, 0]));
                            sLyapunov += tmp;
                            sTime += S[j, 2];
                            sumL += S[j, 1];
                            S[j, 0] = 0;
                            S[j, 1] = 0;
                            S[j, 2] = 0;
                            S[j, 3] = 0;
                            S[j, 4] = 0;
                        }
                        Lyapunovs[z, 0] = sLyapunov / sTime;
                        Lyapunovs[z, 1] = sTime;
                        Lyapunovs[z, 2] = (float)Math.Log(sumL);
                        sumLyap += Lyapunovs[z,0];
                        z++;
                    }
                }
                Calculus.CreateEvent(i, 1);
                i++;                
            }            
            DataProcess.ExportArray(Lyapunovs, "_lyapunovs.csv");
            float lyapunovExponent = sumLyap / z;
            return lyapunovExponent;
        }

        /// <summary>
        /// ������ ������� �������� �� ���������. ������������� ������� � ������� ����������� �� ���� � ��� ������� ���� 
        /// �������������� ������ ��������. 
        /// </summary>
        /// <param name="A">������ � �������</param>
        /// <param name="startGInd">������ � �������, � �������� ���������� ������������� ������� � ������� A</param>
        /// <param name="endGInd">������ � �������, ������� ��������� ������������� ������� � ������� A</param>
        /// <param name="startSegmentLength">��������� ����� ��������, �� �������� ������������� ������. �� ������ ���� ������� ���������, ����� �� ����� ��������������� ��������� �������</param>
        /// <param name="endSegmentLength">�������� ����� ��������, �� �������� ������������� ������. ������ ���� ������ ��� ����� ����� ����</param>
        /// <param name="windowLength">����� ����. ������ ���� ������ ��� ����� ����� ������� A</param>
        /// <returns></returns>
        public static float[] VariationIndex(float[] A, int startGInd, int endGInd, int startSegmentLength, int endSegmentLength, int windowLength, int numRegressionPoints)
        {
            //����� �������� ��������� �������
            int lenGlobal = endGInd - startGInd + 1;
            //���-�� ����
            int countResults = (int)lenGlobal - windowLength + 1;
            //���-�� ��� ������� ���� + ������ ��������, ������������ ��� ����� ��������� (�.�. ����� ����� ���� = ����� ����� ���������)
            float[] resGlobal = new float[countResults];
            //������� ����
            int g = 0;            
            float lnStartSegmentLength = (int)Math.Log(startSegmentLength);
            //��������� ������� ��� ��������������� �����, � ������ ������� �������� ���������
            float powerVal = (float)(Math.Log(endSegmentLength) - lnStartSegmentLength) / (numRegressionPoints - 1);
            //��������� ������ �������� ��� ������� "����"
            for (int startInd = startGInd; startInd <= endGInd - (windowLength - 1); startInd++)
            {
                int endInd = startInd + (windowLength - 1);
                //������ ��� ���������  - ������� ������� �������� ��� �������������� ����                
                float[,] res = new float[numRegressionPoints, 2];
                //������� ��� ���������
                int i = 0;
                //������� ��� ���������
                int ii = 0;
                //��������� ����� �������� ��������� �� startN �� endN 
                for (int currentSegmentLength = startSegmentLength; currentSegmentLength <= endSegmentLength; currentSegmentLength = (int)Math.Exp(lnStartSegmentLength + powerVal * ii))
                {
                    //������� ����� �������� (�������� ����� ���������� � ����������) �� ��������
                    float sum = 0;
                    for (int j = startInd; j <= endInd - currentSegmentLength; j = j + currentSegmentLength)
                    {
                        float min = A[j];
                        float max = A[j];
                        //���� �������� � ������� �� ������ �������
                        for (int k = j; k <= j + currentSegmentLength; k++)
                        {
                            if (A[k] < min) min = A[k];
                            if (A[k] > max) max = A[k];
                        }
                        //���������� �������
                        sum += Math.Abs(max - min);
                    }
                    if (sum > 0)
                    {
                        res[i, 0] = (float)Math.Log((float)currentSegmentLength);
                        res[i, 1] = (float)Math.Log((float)sum);
                        i++;
                    }
                    ii++;
                }
                if (g==5) DataProcess.ExportArray(res, "a_vi" + g + ".csv");
                KRegressionFloat regres = MathProcess.SimpleRegression(res, 0, i-1);
                resGlobal[g] = -1 * regres.A;
                g++;
                System.GC.Collect(GC.GetGeneration(A));
                System.GC.Collect(GC.GetGeneration(res));
            }
            return resGlobal;
        }        

    }

    /// <summary>
    /// ��������������� �����, ��� ����������� ������������ ���������� ��������������� ���������
    /// </summary>
    public class CorrelatingIntegralContiniousThread
    {
        //���������� ����� �������
        public float r;
        //������ ���������� ����� ����� �������
        public float[] RD;
        //��������� ������ � RD
        public int startIndex;
        //�������� ������ � RD
        public int endIndex;
        //���-�� ���������� ����������� r
        public int sum;
        //���-�� �������
        public int threadNum;

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="r">���������� ����� �������, ������������ �������� �������</param>        
        /// <param name="RD">������ ���������� ����� �������</param>
        /// <param name="startIndex">��������� ������ � RD</param>
        /// <param name="endIndex">�������� ������ � RD</param>
        /// <param name="threadNum">���-�� �������</param>
        public CorrelatingIntegralContiniousThread(float r, float[] RD, int startIndex, int endIndex, int threadNum)
        {
            this.r = r;
            this.RD = RD;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.sum = 0;
            this.threadNum = threadNum;
        }

        /// <summary>
        /// ��������� ���-�� ���������� ������ r
        /// </summary>
        public void GetCorrelatingIntegralContinious()
        {
            //int j = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                //if (RD[i] < r && RD[i] > 0) sum++;
                if (RD[i] < r) sum++;
                
                //����������� ����������                                
                //������� ������� ������� � �� ������ ��� ������� > 2
                //j++;
                //Calculus.CreateEvent(j, threadNum);
            }
        }
    }

    /// <summary>
    /// ��������������� �����, ��� ����������� ������������ ���������� ��������������� ��������� 
    /// (��� ����������� �������� > 2)
    /// </summary>
    public class CorrelatingIntegralFirst2Thread
    {
        public float r;
        public float[] RD;
        public float[] A;
        public int dimension;
        public int startIndex;
        public int endIndex;
        public int sum;
        public int zeros;
        public int threadNum;

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="r">���������� ����� �������, ������������ �������� �������</param>
        /// <param name="B">����������� ������ ����� � ������������ �������� ������ 2</param>
        /// <param name="startIndex">��������� ������ � B</param>
        /// <param name="endIndex">�������� ������ � B</param>
        /// <param name="threadNum">���-�� �������</param>
        public CorrelatingIntegralFirst2Thread(float r, ref float[] A, int dimension, int startIndex, int endIndex, int threadNum)
        {
            this.r = r;
            this.A = A;
            this.dimension = dimension;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.sum = 0;
            this.zeros = 0;
            this.threadNum = threadNum;            
        }

        /// <summary>
        /// ��������� ���-�� ���������� ������ r � ���������� ��� ���������� � RD
        /// </summary>
        public void GetCorrelatingIntegralFirst2()
        {
            int l = A.Length - dimension + 1;
            sum = 0;
            int z = 0;
            float d;
            for (int i = startIndex; i < endIndex; i++)
            {
                for (int j = i + 1; j < l; j++)
                {
                    //d = ChaosLogic.GetPointDistance(B[i], B[j]);
                    float max = 0;
                    for (int dim = 0; dim < dimension; dim++)
                    {
                        d = Math.Abs(A[i + dim] - A[j + dim]);
                        if (d > max) max = d;
                    }
                    //if (d < r && d > 0) sum++;
                    if (max < r) sum++;
                    if (max == 0) zeros++;
                    this.RD[z] = max;
                    z++;
                    //������� ������� ������� � �� ������ ��� ������� > 2 ���� ������������
                    //Calculus.CreateEvent(z, threadNum);
                }
            }
        }
    }
}