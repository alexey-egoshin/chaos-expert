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

///Часть содержит методы для вычисления корреляционной размерности
namespace ChaosExpert
{
    /// <remarks>
    /// Класс для определения "разладки" в сигнале
    /// </remarks>

    public class DisorderSignal
    {
        /// <summary>
        /// Апостериорное обнаружение разладки в сигнале. Общий случай Дарховского-Бродского
        /// ! Диагностирование
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <param name="v">Параметр. Асимптотически минимаксный метод при 0.5</param>
        /// <returns>Решающая функция. Максимум по абсолютному значению из доверительного интервала (не считая краев) - точка разладки</returns>
        public static DisorderResult aposteriorBrodskyDarhovskyOverall(float[] A, float v)
        {
            float[] B = new float[A.Length];
            int indMax = 0;
            for (int n = 0; n < A.Length; n++)
            {
                float sum1 = 0;
                for (int i = 0; i < n + 1; i++)
                {
                    sum1 += A[i];
                }
                float sum2 = 0;
                for (int i = n; i < A.Length; i++)
                {
                    sum2 += A[i];
                }
                float o = (float)n / (float)A.Length;
                B[n] = (float)Math.Pow((double)(o * (1 - o)), (double)v) * (sum1 / (float)(n + 1) - 1 / (float)(A.Length - n + 1) * sum2);
                indMax = (Math.Abs(B[n]) > Math.Abs(B[indMax])) ? n : indMax;
            }
            DisorderResult res;
            res.A = B;
            res.indMax = indMax;
            return res;            
        }


        /// <summary>
        /// Апостериорное обнаружение разладки по методу Васильченко. Не до конца понял.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="e"></param>
        /// <param name="d"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static int[] aposteriorVasilchenko(float[] A, float e, float d, float h)
        {
            float[] B = new float[A.Length];
            int eN = (int)Math.Floor(A.Length * e);
            float cn = 3 * e * e * h / 8;
            int[] n = new int[A.Length];
            float v = _vasilchenkoStatistic(A, eN, e, eN);
            int nc = 0;
            if (Math.Abs(v) > cn)
            {
                n[nc] = eN;
                float rightCond = B.Length - 2 * eN;
                for (int i = eN + 1; i < rightCond; i++)
                {
                    v = _vasilchenkoStatistic(A, i, e, eN);
                    int leftCond = n[nc] + (int)Math.Floor(d * A.Length / 2);
                    if (Math.Abs(v) > cn && leftCond <= rightCond)
                    {
                        nc++;
                        n[nc] = leftCond;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //List<float> n;

            return n;
        }

        public static float _vasilchenkoStatistic(float[] A, int n, float e, int eN)
        {
            int s1 = n - eN;
            int s2 = n + 2 * eN;
            int s3 = n + eN;
            int s4 = n;
            float sum1 = DataProcess.sumArray(A, 0, s1);
            float sum2 = DataProcess.sumArray(A, 0, s2);
            float sum3 = DataProcess.sumArray(A, 0, s3);
            float sum4 = DataProcess.sumArray(A, 0, s4);
            float res = 1f / A.Length * (sum1 - sum2 + 3 * (sum3 - sum4));
            return res;
        }

        /// <summary>
        /// Последовательный метод Сегена-Сандерсона
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <param name="v">Порог чувствительности алгоритма (> 0)</param>
        /// <returns>Решающая функция</returns>
        public static float[] successiveSegenaSanderson(float[] A, float v)
        {
            float s1 = A[0];
            float s2 = s1;
            float g1 = 0;
            float[] B = new float[A.Length];
            for (int i = 1; i < A.Length; i++)
            {
                s1 += A[i];
                s2 = (s2 < s1) ? s2 : s1;
                float ds = s1 - s2;
                g1 = (g1 > ds) ? g1 : ds;
                float d = (v > i) ? v : i;
                B[i] = g1 / d;
            }
            return B;
        }

        /// <summary>
        /// Алгоритм кумулятивных сумм. Описание (! Ratio tests for change point detection 2008.pdf)
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <returns>Решающая функция и индекс ее максимума</returns>
        public static DisorderResult CUSUM(float[] A)
        {
            float average = 0;
            float[] B = new float[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                average += A[i];
            }
            average /= A.Length;
            float dispersion_sum = 0;
            for (int i = 0; i < A.Length; i++)
            {
                float delta = A[i] - average;
                dispersion_sum += delta * delta;
            }
            dispersion_sum = (float)Math.Pow(dispersion_sum*A.Length, 0.5f);
            int indMax = 0;
            for (int i = 0; i < A.Length; i++)
            {
                float sum = 0;
                for (int k = 0; k <= i; k++)
                {
                    sum += A[k] - average;
                }
                B[i] = sum / dispersion_sum;
                indMax = (Math.Abs(B[i]) > Math.Abs(B[indMax])) ? i : indMax;
            }
            DisorderResult res;
            res.A = B;
            res.indMax = indMax;
            return res;
        }

        public static DisorderResult CUSUM(Point[] A, int startInd, int endInd)
        {
            float average = 0;
            int len = endInd - startInd + 1;
            float[] B = new float[len];
            for (int i = startInd; i < len; i++)
            {
                average += A[i].val;
            }
            average /= len;
            float dispersion_sum = 0;
            for (int i = startInd; i < len; i++)
            {
                float delta = A[i].val - average;
                dispersion_sum += delta * delta;
            }
            dispersion_sum = (float)Math.Pow(dispersion_sum * len, 0.5f);
            int indMax = 0;
            for (int i = startInd; i < len; i++)
            {
                float sum = 0;
                for (int k = startInd; k <= i; k++)
                {
                    sum += A[k].val - average;
                }
                B[i] = sum / dispersion_sum;
                indMax = (Math.Abs(B[i]) > Math.Abs(B[indMax])) ? i : indMax;
            }
            DisorderResult res;
            res.A = B;
            res.indMax = indMax;
            return res;
        }


        /// <summary>
        /// Подобие R/S статистики на основе CUSUM. Описание (! Ratio tests for change point detection 2008.pdf)
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <returns>Решающая функция</returns>
        public static float[] CUSUM_RS(float[] A)
        {
            float average = 0;
            float[] B = new float[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                average += A[i];
            }
            average /= A.Length;
            float dispersion_sum = 0;
            for (int i = 0; i < A.Length; i++)
            {
                float delta = A[i] - average;
                dispersion_sum += delta * delta;
            }
            dispersion_sum = (float)Math.Pow(dispersion_sum, 0.5f);
            for (int i = 0; i < A.Length; i++)
            {
                float sum = 0;
                float max = 0;
                float min = 0;
                for (int k = 0; k <= i; k++)
                {
                    sum += A[k] - average;
                    if (sum > max) max = sum;
                    if (sum < min) min = sum;
                }

                B[i] = (max - min) / dispersion_sum;
            }

            return B;
        }

        /// <summary>
        /// Модификация CUSUM_RS. Описание (! Ratio tests for change point detection 2008.pdf) 
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <returns>Решающая функция</returns>
        public static float[] CUSUM_Giraitis(float[] A)
        {
            float[] B = new float[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                float average = 0;

                for (int k = 0; k <= i; k++)
                {
                    average += A[k];
                }
                average /= i + 1;
                float dispersion_sum = 0;
                for (int k = 0; k <= i; k++)
                {
                    float delta = A[k] - average;
                    dispersion_sum += delta * delta;
                }
                dispersion_sum *= (i + 1);
                float sum_left = 0;
                float sum_right = 0;
                for (int k = 0; k <= i; k++)
                {
                    float sum1 = 0;
                    for (int j = 0; j <= k; j++)
                    {
                        float d = A[j] - average;
                        sum1 += d;
                        sum_right += d;

                    }
                    sum1 *= sum1;
                    sum_left += sum1;

                }
                sum_right *= sum_right / (i + 1);
                B[i] = (sum_left - sum_right) / dispersion_sum;
            }

            return B;
        }

        /// <summary>
        /// На основе CUSUM. Описание (! Ratio tests for change point detection 2008.pdf)  
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <returns>Решающая функция</returns>
        public static float[] ratioCUSUM(float[] A)
        {
            float average = 0;
            float[] B = new float[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                average += A[i];
            }
            average /= A.Length;

            for (int i = 0; i < A.Length; i++)
            {
                float sum = 0;
                float max = 0;
                float min = 0;
                for (int k = 0; k <= i; k++)
                {
                    for (int j = 0; k <= i; k++)
                    {

                    }
                    sum += A[k] - average;
                    if (sum > max) max = sum;
                    if (sum < min) min = sum;
                }

                B[i] = (max - min);
            }

            return B;
        }

        /// <summary>
        /// Последовательный метод обнаружения разладки
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <param name="v">Параметр чувствительности (сравним со средним значением сигнала)</param>
        /// <returns>Решающая функция</returns>
        public static float[] GirshikRubinShiryaev(float[] A, float v)
        {
            float[] B = new float[A.Length];
            float last_val = 0;
            for (int i = 0; i < A.Length; i++)
            {
                last_val = (float)Math.Exp(A[i] - v) * (1 + last_val);
                B[i] = last_val;
            }
            return B;
        }

        /// <summary>
        /// Тест Петтита. Описание (An integrated approach using change-point detection and ANN for interest rates forecasting 2000.pdf)  
        /// </summary>
        /// <param name="A">Сигнал</param>     
        /// <returns>Решающая функция</returns>
        public static float[] testPettit(float[] A)
        {
            float[] B = new float[A.Length];
            for (int i = 0; i < A.Length - 1; i++)
            {
                B[i] = 2 * DataProcess.sumArray(A, 0, i) - (i + 1) * (A.Length + 1);
            }
            return B;
        }

        /// <summary>
        /// Обнаружение разладки случайного процесса по выборке на основе принципа минимума информационного рассогласования (Обнаружение разладки случайного процесса по выборке на основе принципа минимума информационного рассогласования.doc)
        /// </summary>
        /// <param name="A">Сигнал</param>     
        /// <returns>Решающая функция и индекс ее максимума</returns>
        public static DisorderResult minInfoError(float[] A)
        {
            float[] B = new float[A.Length];
            float c = (float)(Math.Log(2 * Math.PI) + 1);
            int indMax = 0;
            for (int i = 0; i < A.Length - 2; i++)
            {
                float c1 = -(i + 1) * (float)(Math.Log(DataProcess.averageSquare(A, 0, i)) + c) / 2;
                float c2 = -(A.Length - i - 1) * (float)(Math.Log(DataProcess.averageSquare(A, i + 1, A.Length - 1)) + c) / 2;
                float c3 = (A.Length) * (float)(Math.Log(DataProcess.averageSquare(A, 0, A.Length - 1)) + c) / 2;
                B[i] = c1 + c2 + c3;
                indMax = (Math.Abs(B[i]) > Math.Abs(B[indMax])) ? i : indMax;
            }
            DisorderResult res;
            res.A = B;
            res.indMax = indMax;
            return res;
        }

        public static float[] generateTradeSignal(float p0, float average, float dispersion, float herst, int length)
        {
            //случайный сигнал (гаусов процесс)
            float[] B = new float[length];
            //конечный сигнал (торговый сигнал - цена)
            float[] P = new float[length];
            float p_previos = p0;
            //генерим случайный сигнал заданной длины, с заданной дисперсией
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                B[i] = ((float)random.NextDouble() - 0.5f) * dispersion;

            }

            float gconst = herst - 0.5f;
            float r = average + B[0];
            float p = p_previos * (float)Math.Exp(r);
            p_previos = p;
            P[0] = p;

            //получаем случайный процесс с "памятью"
            for (int i = 1; i < length; i++)
            {
                gconst = herst - 0.5f;
                int j = 1;
                float e = 0;
                float psi = 1f;
                do
                {
                    psi = psi * (gconst + j - 1) / j;
                    if (psi == 0) psi = 1;
                    e += psi * B[i - j];
                    j++;
                }
                while (j <= i && j<400);
                r = average + e;
                p = p_previos * (float)Math.Exp(r);
                p_previos = p;
                P[i] = p;
            }
            return P;
        }
    }
}