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

///����� ������, ���������� RS-������

namespace ChaosExpert
{
    public partial class ChaosLogic
    {
        /// <summary>
        /// R/S ��� ������ �������
        /// </summary>
        /// <param name="A">������ ������</param>
        /// <param name="start_index">���. ������ ������� (������������ ������)</param>
        /// <param name="end_index">�������� ������ �������</param>
        /// <returns>R/S - ��������� ������� � ������������ ����������</returns>
        public static float GetRS(PointTimeThresholdChange[] A, int start_index, int end_index)
        {
            //�������
            float average = 0;
            for (int i = start_index; i <= end_index; i++) average += A[i].time;
            int len = end_index - start_index + 1;
            average /= len;
            float[] deviation = new float[len];
            float sum = 0;
            float sum_sq = 0;
            float min = 0;
            float max = 0;
            for (int i = start_index; i <= end_index; i++)
            {
                int j = i - start_index;
                deviation[j] = A[i].time - average;
                sum_sq += deviation[j] * deviation[j];
                //����� ����������� ����������
                sum += deviation[j];
                if (sum > max)
                {
                    max = sum;
                }
                if (sum < min)
                {
                    min = sum;
                }
            }
            if (len > 1)
            {
                //����������� ������������
                float s = (float)Math.Sqrt(sum_sq / len);
                if (s == 0) s = 1;
                return (max - min) / s;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// R/S ��� ������ �������
        /// </summary>
        /// <param name="A">������ ������</param>
        /// <param name="start_index">���. ������ ������� (������������ ������)</param>
        /// <param name="end_index">�������� ������ �������</param>
        /// <returns>R/S - ��������� ������� � ������������ ����������</returns>
        public static float GetRS(float[] A, int start_index, int end_index)
        {
            //�������
            float average = 0;
            for (int i = start_index; i <= end_index; i++) average += A[i];
            int len = end_index - start_index + 1;
            average /= len;
            float[] deviation = new float[len];
            float sum = 0;
            float sum_sq = 0;
            float min = 0;
            float max = 0;
            for (int i = start_index; i <= end_index; i++)
            {
                int j = i - start_index;
                deviation[j] = A[i] - average;
                sum_sq += deviation[j] * deviation[j];
                //����� ����������� ����������
                sum += deviation[j];
                if (sum > max) max = sum;
                if (sum < min) min = sum;
            }

            if (len > 1)
            {
                //����������� ������������
                float s = (float)Math.Sqrt(sum_sq / len);
                if (s == 0) s = 1;
                return (max - min) / s;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// R/S ������ ���� (���������������� ����������)
        /// </summary>
        /// <param name="A">������ ������� ������</param>
        /// <param name="periodLength">��������� ����� �������</param>
        /// <param name="startGIndex">��������� ������ � �������, � �������� ������� ������</param>
        /// <param name="endGIndex">�������� ������ � �������, �� �������� ������� ������ (������������)</param>
        /// <param name="maxPeriodLength">������������ ����� �������, �� �������� ������� ������</param>
        /// <returns>������� ���������� R/S-����� � ������������ ��������� (A - ������������ H)</returns>
        public static ResultRS RS_Analyse(float[] A, int periodLength, int startGIndex, int endGIndex, int maxPeriodLength)
        {
            //���-�� RS-�����������
            int resLength = maxPeriodLength - periodLength + 1;
            //���������� RS-�������
            PointRS[] result = new PointRS[resLength];
            //��������� ������ � �������
            int lastIndex = endGIndex;
            //������ ������ � �������
            //�������� ��������� ������
            int startIndex, endIndex, startIndexNow;
            //��. �������� RS ��� ������� �������
            float averageRS;
            int j = 0;
            //������� ����� �������            
            int currentPeriodLength = periodLength;
            //���-�� ��������
            int periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
            //������������� ����� �����������
            CalculateStart.CreateEvent(resLength + 1, "R/S analyse...");
            do
            {
                startIndex = lastIndex;
                endIndex = lastIndex;
                averageRS = 0;
                for (int i = 1; i <= periodCount; i++)
                {
                    startIndex = endIndex - currentPeriodLength + 1;
                    averageRS += GetRS(A, startIndex, endIndex);
                    endIndex -= startIndex - 1;
                }
                startIndexNow = startIndex;
                averageRS /= periodCount;
                result[j].n = Math.Log10((float)currentPeriodLength / 2);
                result[j].rs = Math.Log10(averageRS);
                result[j].h = result[j].rs / result[j].n;
                j++;
                currentPeriodLength++;
                //���-�� ��������
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //����������� ����������
                Calculus.CreateEvent(j);
            }
            while (currentPeriodLength <= maxPeriodLength);
            KRegression regression = MathProcess.SimpleRegression(result);
            ResultRS res;
            res.points = result;
            res.regression = regression;
            return res;
        }

        /// <summary>
        /// R/S ������ ���� (���������������� ����������)
        /// </summary>
        /// <param name="A">������ ������� ������</param>
        /// <param name="periodLength">��������� ����� �������</param>
        /// <param name="startGIndex">��������� ������ � �������, � �������� ������� ������</param>
        /// <param name="endGIndex">�������� ������ � �������, �� �������� ������� ������ (������������)</param>
        /// <param name="maxPeriodLength">������������ ����� �������, �� �������� ������� ������</param>
        /// <returns>������� ���������� R/S-����� � ������������ ��������� (A - ������������ H)</returns>
        public static ResultRS RS_Analyse(PointTimeThresholdChange[] A, int periodLength, int startGIndex, int endGIndex, int maxPeriodLength)
        {
            //���-�� RS-�����������
            int resLength = maxPeriodLength - periodLength + 1;
            //���������� RS-�������
            PointRS[] result = new PointRS[resLength];
            //��������� ������ � �������
            int lastIndex = endGIndex;
            //������ ������ � �������
            //�������� ��������� ������
            int startIndex, endIndex;
            //��. �������� RS ��� ������� �������
            float averageRS;
            int j = 0;
            //������� ����� �������            
            int currentPeriodLength = periodLength;
            //���-�� ��������
            int periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
            //������������� ����� �����������
            CalculateStart.CreateEvent(resLength + 1, "R/S analyse...");
            do
            {
                startIndex = lastIndex;
                endIndex = lastIndex;
                averageRS = 0;
                for (int i = 1; i <= periodCount; i++)
                {
                    startIndex = endIndex - currentPeriodLength + 1;
                    averageRS += GetRS(A, startIndex, endIndex);
                    endIndex -= startIndex - 1;
                }
                averageRS /= periodCount;
                result[j].n = Math.Log10(currentPeriodLength);
                result[j].rs = Math.Log10(averageRS);
                result[j].h = result[j].rs / result[j].n;
                j++;
                currentPeriodLength = periodLength + j;
                //���-�� ��������
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //����������� ����������
                Calculus.CreateEvent(j);
            }
            while (currentPeriodLength <= maxPeriodLength);
            KRegression regression = MathProcess.SimpleRegression(result);
            ResultRS res;
            res.points = result;
            res.regression = regression;
            return res;
        }

        /// <summary>
        /// R/S ������ ���� (������������ ����������)
        /// </summary>
        /// <param name="A">������ ������� ������</param>
        /// <param name="periodLength">��������� ����� �������</param>
        /// <param name="startGIndex">��������� ������ � �������, � �������� ������� ������</param>
        /// <param name="endGIndex">�������� ������ � �������, �� �������� ������� ������ (������������)</param>
        /// <param name="maxPeriodLength">������������ ����� �������, �� �������� ������� ������</param>
        /// <param name="threadCount">���������� ������� (����������������� ����������)</param>
        /// <returns>������� ���������� R/S-����� � ������������ ��������� (A - ������������ H)</returns>
        public static ResultRS RS_Analyse(float[] A, int currentPeriodLength, int startGIndex, int endGIndex, int maxPeriodLength, int threadCount)
        {
            //���-�� RS-�����������
            int resLength = maxPeriodLength - currentPeriodLength + 1;
            //���������� RS-�������
            PointRS[] result = new PointRS[resLength];
            //����� ������� �� �����
            int periodIntervalLength = resLength / threadCount;
            //������� (�.�. ����� ����� �� ��������� �� ���� �� ����� �������)
            int remainder = resLength - periodIntervalLength * threadCount;
            //������������ ������ ��� ������� ������ = ����� �������� + �������
            int maxPeriodLengthLocal = periodIntervalLength + remainder + currentPeriodLength - 1;
            //������
            Thread[] threads = new Thread[threadCount];
            //��������� ����������, ���������� R/S ������
            RS_AnalyseThread[] rsat = new RS_AnalyseThread[threadCount];
            //������ ����� �����
            rsat[0] = new RS_AnalyseThread(A, currentPeriodLength, startGIndex, endGIndex, maxPeriodLengthLocal, 0);
            threads[0] = new Thread(new ThreadStart(rsat[0].RS_Analyse));
            //������������� ����� ����������� ��� 1 ��������
            resLength = maxPeriodLengthLocal - currentPeriodLength + 1;
            CalculateStart.CreateEvent(resLength, "R/S analyse 1", 0);
            threads[0].Start();
            //��������� ��������� ������
            int j = 2;
            for (int i = 1; i < threadCount; i++)
            {
                //� ������ ������� �������� �������
                currentPeriodLength = maxPeriodLengthLocal + 1;
                //�� ������ ������� �������
                maxPeriodLengthLocal += periodIntervalLength;
                //������ ������
                rsat[i] = new RS_AnalyseThread(A, currentPeriodLength, startGIndex, endGIndex, maxPeriodLengthLocal, i);
                threads[i] = new Thread(new ThreadStart(rsat[i].RS_Analyse));
                //������������� ����� ����������� ��� i ��������
                resLength = maxPeriodLengthLocal - currentPeriodLength + 1;
                CalculateStart.CreateEvent(resLength, "R/S analyse " + j.ToString(), i);
                threads[i].Start();
                j++;
            }
            //���� ���������� ���� �������
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
            //������� ��� ���������� � ���� ������
            int indexResult = 0;
            for (int i = 0; i < threadCount; i++)
            {
                rsat[i].result.CopyTo(result, indexResult);
                indexResult += rsat[i].result.Length;
            }
            //���������
            KRegression regression = MathProcess.SimpleRegression(result, (int) (result.Length/2.5), result.Length-1);
            //KRegression regression = MathProcess.SimpleRegression(result, 0, result.Length - 1);
            ResultRS res;
            res.points = result;
            res.regression = regression;
            GC.Collect(GC.GetGeneration(A));
            GC.Collect(GC.GetGeneration(rsat));
            GC.Collect(GC.GetGeneration(threads));
            GC.Collect(GC.GetGeneration(result));
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return res;
        }
    }

    /// <summary>
    /// ��������������� �����, ��� ����������� ������������ ���������� � R\S �������
    /// </summary>
    public class RS_AnalyseThread
    {
        //������� ������
        public float[] A;
        //������� ����� �������
        public int currentPeriodLength;
        //� ������ ������� � � ������� (������������)
        public int startGIndex;
        //�� ������ ������� � � ������� (������������)
        public int endGIndex;
        //����. ����� �������
        public int maxPeriodLength;
        //���������� R/S ������� ��� ������� ������
        public PointRS[] result;
        //����� ������
        public int threadNum;

        /// <summary>
        /// �����������, �������������� ����� ��� ���������� R/S ������� ����
        /// </summary>
        /// <param name="A">������ ������� ������</param>
        /// <param name="currentPeriodLength">��������� ����� �������</param>
        /// <param name="startGIndex">��������� ������ � �������, � �������� ������� ������</param>
        /// <param name="endGIndex">�������� ������ � �������, �� �������� ������� ������ (������������)</param>
        /// <param name="maxPeriodLength">������������ ����� �������, �� �������� ������� ������</param>
        /// <param name="threadNum">����� ������ (����������������� ����������)</param>        
        public RS_AnalyseThread(float[] A, int currentPeriodLength, int startGIndex, int endGIndex, int maxPeriodLength, int threadNum)
        {
            this.A = A;
            this.currentPeriodLength = currentPeriodLength;
            this.startGIndex = startGIndex;
            this.endGIndex = endGIndex;
            this.maxPeriodLength = maxPeriodLength;
            this.threadNum = threadNum;
        }

        /// <summary>
        /// ���������� R/S ������� � ������
        /// </summary>
        public void RS_Analyse()
        {
            //���-�� RS-�����������
            int resLength = maxPeriodLength - currentPeriodLength + 1;
            //���������� RS-�������
            result = new PointRS[resLength];
            //��������� ������ � �������
            int lastIndex = endGIndex;
            //������ ������ � �������
            //�������� ��������� ������
            int startIndex, endIndex;
            //��. �������� RS ��� ������� �������
            double averageRS;
            int j = 0;
            //���-�� ��������
            int periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
            do
            {
                startIndex = lastIndex;
                endIndex = lastIndex;
                averageRS = 0;
                for (int i = 1; i <= periodCount; i++)
                {
                    startIndex = endIndex - currentPeriodLength + 1;
                    averageRS += ChaosLogic.GetRS(A, startIndex, endIndex);
                    endIndex -= startIndex - 1;
                }
                averageRS /= periodCount;
                result[j].n = Math.Log10((float)currentPeriodLength/2);
                result[j].rs = Math.Log10(averageRS);
                result[j].h = result[j].rs / result[j].n;
                result[j].h0 = averageRS / currentPeriodLength;
                j++;
                currentPeriodLength++;
                //���-�� ��������
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //����������� ����������                
                Calculus.CreateEvent(j, threadNum);
            }
            while (currentPeriodLength <= maxPeriodLength);
        }
    }
}
