using System;
using System.Collections.Generic;
using System.Text;

namespace ChaosExpert
{
    public class MathProcess
    {
        /// <summary>
        /// Вычисление разницы в процентах между двумя величинами
        /// </summary>
        /// <param name="a">Первоначальное значение (относительно которого изменяется)</param>
        /// <param name="b">Второе значение</param>
        /// <returns>Разница в процентах относительно 1-го аргумента</returns>
        public static decimal DeltaProcent(decimal a, decimal b)
        {
            return (b - a) / a * 100;
        }

        public static float DeltaProcent(float a, float b)
        {
            return (float)(b - a) / a * 100;
        }

        /// <summary>
        /// Простая линейная регрессия y=Ax+B
        /// </summary>
        /// <param name="A">Массив наблюдений</param>
        /// <returns>Коэффициенты A и B</returns>
        public static KRegression SimpleRegression(PointRS[] A)
        {
            double sumX = 0;
            double sumY = 0;
            double sumX2 = 0;
            double sumXY = 0;
            CalculateStart.CreateEvent(A.Length, "Simple regression ...");
            for (int i = 0; i < A.Length; i++)
            {
                sumX += A[i].n;
                sumY += A[i].rs;
                sumX2 += A[i].n * A[i].n;
                sumXY += A[i].n * A[i].rs;
                Calculus.CreateEvent(i);
            }
            double nSumX2 = A.Length * sumX2;
            KRegression res;
            res.A = (A.Length * sumXY - sumX * sumY) / (nSumX2 - sumX * sumX);
            res.B = (sumX2 * sumY - sumX * sumXY) / (nSumX2 - sumX2 * sumX2);
            return res;
        }

        /// <summary>
        /// Простая линейная регрессия y=Ax+B
        /// </summary>
        /// <param name="A">Массив наблюдений</param>
        /// <returns>Коэффициенты A и B</returns>
        public static KRegression SimpleRegression(PointRS[] A, int startIndex, int endIndex)
        {
            double sumX = 0;
            double sumY = 0;
            double sumX2 = 0;
            double sumXY = 0;
            int len = endIndex - startIndex + 1;
            CalculateStart.CreateEvent(A.Length, "Simple regression ...");
            for (int i = startIndex; i < len; i++)
            {
                sumX += A[i].n;
                sumY += A[i].rs;
                sumX2 += A[i].n * A[i].n;
                sumXY += A[i].n * A[i].rs;
                Calculus.CreateEvent(i);
            }
            double nSumX2 = len * sumX2;
            KRegression res;
            res.A = (len * sumXY - sumX * sumY) / (nSumX2 - sumX * sumX);
            res.B = (sumX2 * sumY - sumX * sumXY) / (nSumX2 - sumX2 * sumX2);
            return res;
        }


        public static KRegression SimpleRegression(double[,] A, int startIndex, int endIndex)
        {
            double sumX = 0;
            double sumY = 0;
            double sumX2 = 0;
            double sumXY = 0;
            int len = endIndex - startIndex + 1;
            CalculateStart.CreateEvent(len, "Simple regression ...");
            for (int i = startIndex; i < len; i++)
            {
                sumX += A[i, 0];
                sumY += A[i, 1];
                sumX2 += A[i, 0] * A[i, 0];
                sumXY += A[i, 0] * A[i, 1];
                Calculus.CreateEvent(i);
            }
            double nSumX2 = len * sumX2;
            KRegression res;
            res.A = (len * sumXY - sumX * sumY) / (nSumX2 - sumX * sumX);
            res.B = (sumX2 * sumY - sumX * sumXY) / (nSumX2 - sumX2 * sumX2);
            return res;
        }

        public static KRegressionFloat SimpleRegression(float[,] A, int startIndex, int endIndex)
        {
            float sumX = 0;
            float sumY = 0;
            float sumX2 = 0;
            float sumXY = 0;
            int len = endIndex - startIndex + 1;
            //CalculateStart.CreateEvent(len, "Simple regression ...");
            for (int i = startIndex; i <= endIndex; i++)
            {
                sumX += A[i, 0];
                sumY += A[i, 1];
                sumX2 += A[i, 0] * A[i, 0];
                sumXY += A[i, 0] * A[i, 1];
                //Calculus.CreateEvent(i);
            }
            float nSumX2 = len * sumX2;
            KRegressionFloat res;
            res.A = (len * sumXY - sumX * sumY) / (nSumX2 - sumX * sumX);
            res.B = (sumX2 * sumY - sumX * sumXY) / (nSumX2 - sumX2 * sumX2);
            return res;
        }

        /// <summary>
        /// Получение индекса для одномерного массива по координатам многомерного
        /// </summary>
        /// <param name="A">Координаты в многомерном массиве</param>
        /// <param name="B">Количество элементов по каждой координате</param>
        /// <returns>Индекс в одномерном с таким же количеством элементов</returns>
        public static ulong getIndexByMultidimArray(int[] A, int[] B)
        {
            int v;
            ulong ind = 0;            
            for (int i = 0; i < A.Length; i++)
            {
                v = 1;
                for (int j = i+1; j < B.Length; j++)
                {
                    v *= B[j];
                }
                ind += (ulong) A[i] * (ulong) v;
            }
            return ind;
        }

        public static int getIndexByMultidimArray(int[] A, int lastIndex)
        {
            int v;
            int ind = 0;
            for (int i = 0; i < A.Length; i++)
            {
                v = 1;
                for (int j = i + 1; j < A.Length; j++)
                {
                    v *= lastIndex;
                }                
                ind += A[i] * v;
            }
            return ind;
        }

        /// <summary>
        /// Вычисляет манхеновское расстояние между точками в многомерном про-ве
        /// </summary>
        /// <param name="p1">Координаты 1-й точки</param>
        /// <param name="p2">Координаты 2-й точки</param>
        /// <returns>Расстояние как сумма модулей разностей соответствующих координат</returns>
        public static float getPointSpacingManhaten(float[] p1, float[] p2)
        {
            float b = 0;
            float a = 0;
            float max = 0;
            for (int i = 0; i < p1.Length; i++)
            {
                b = Math.Abs(p1[i] - p2[i]);
                a += b;
                //if (b > max) max = b;
            }
            return a;
        }

        public static float sigmoid(float x)
        {
            float res = (float)1 / (1 + x);
            return res;
        }
        
        public static float sigmoid2(float x)
        {
            float res = (float)1 / (1 + (float) Math.Exp((double)-x));
            return res;
        }

        /// <summary>
        /// Перемножение матриц A и B
        /// </summary>
        /// <param name="A">1-я матрица</param>
        /// <param name="col1A">Индекс столбца с какого</param>
        /// <param name="col2A">Индекс столбца по какой</param>
        /// <param name="row1A">Индекс строки с какой</param>
        /// <param name="row2A">Индекс строки по какую</param>
        /// <param name="B">Аналогично</param>
        /// <param name="col1B"></param>
        /// <param name="col2B"></param>
        /// <param name="row1B"></param>
        /// <param name="row2B"></param>
        /// <returns>C размером число строк в A x число столбцов B</returns>
        public static float[,] mulMatrix(float[,] A, int col1A, int col2A, int row1A, int row2A, float[,] B, int col1B, int col2B, int row1B, int row2B)
        {
            float[,] C = new float[row2A - row1A + 1, col2B - col1A + 1];
            for (int i = row1A; i <= row2A; i++)
                for(int j = col1B; j <= col2B; j++)
                {
                    C[i - row1A,j - col1B] = 0;
                    for (int k = 0; k < col2A - col1A + 1; k++)
                    {
                        C[i - row1A,j - col1B] += (A[i,k+col1A] * B[k+row1B,j]);
                    }
                }
            return C;
        }        
    }
}
