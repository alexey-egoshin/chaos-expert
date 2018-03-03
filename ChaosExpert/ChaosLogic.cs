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
    /// Класс для вычисления хаотических характеристик сигнала
    /// </remarks>
        
    public partial class ChaosLogic
    {
        //невозможно рассчитать корр. интеграл, т.к. = 0
        public const long ERR_CORR_INTEGRAL = 999999999999999999;
        //невозможно рассчитать корр. размерность, т.к. корр. интеграл = 0
        public const long ERR_CORR_DIMENSION = -999999999999999999;

        //максимальное относительное отклонение для точки, чтобы считать ее лежащей на прямой
        public const float E_DEV = 0.6f;

        /// <summary>
        /// Расстояние между 2-мя точками в пространстве размерности x1.Length=x2.Length
        /// </summary>
        /// <param name="x">Массив с координатами 1-й точки</param>
        /// <param name="y">Массив с координатами 2-й точки</param>
        /// <returns>Расстояние между 2-мя точками</returns>
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
        /// Автовзаимная информация по временному ряду. Интересует первый локальный минимум на графике кол-во информации - лаг (задержка)
        /// </summary>
        /// <param name="A">Массив с данными</param>
        /// <param name="startDelay">Начальный лаг смещения</param>
        /// <param name="endDelay">Конечный лаг смещения</param>
        /// <param name="levelsCount">Количество уровней разбиеня по вероятности достижения (чем больше тем точнее)</param>
        /// <returns>Массив с количеством взаимной информации для каждой величины лага и оптимальный лаг (первый локальный минимум)</returns>
        public static AutoMutualInfoResult AutoMutualInformation(float[] A, int startDelay, int endDelay, int levelsCount)
        {
            ///Получаем распределение вероятностей для величины.
            ///Для этого разбиваем на levels уровней состояний, и считаем вероятность для каждого состояния
            
            //Вероятности для каждого уровня
            double[] Levels = new double[levelsCount];
            double[] AMI = new double[endDelay - startDelay + 1];                       
            float max = 0;
            double[,] L12 = new double[levelsCount, levelsCount];
            int[] L = new int[A.Length];
            //Находим максимум и минимум чтобы выбрать размер шага разбиения на уровни
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
            //Энтропия сигнала
            //double H = 0;
            //Вероятности для каждого состояния
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
                //Вероятности перехода из L1 в L2                
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
            //Ищем 1-й локальный минимум
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
        /// Первая оценка корреляционного интеграла для размерности 1. Находятся максимальное и мимнимальное расстояние 
        /// между точками на аттракторе. Подсчитывается количество 0-х расстояний. Все расстояния между точками сохраняются.
        /// </summary>
        /// <param name="r">Расстояние между точками, относительно которого считают</param>
        /// <param name="B">Массив с точками</param>
        /// <param name="RD">Массив расстояний между точками</param>
        /// <param name="maxR">Максимальное расстояние между точками</param>
        /// <param name="minR">Минимальное расстояние между точками</param>
        /// <param name="zeros">Кол-во нулевых расстояний</param>
        /// <returns>Оценка корреляционного интеграла для данного расстояния</returns>
        public static float GetCorrelatingIntegralFirst(float r, float[] B, ref float[] RD, ref float maxR, ref float minR, ref int zeros)
        {
            int len = B.Length;
            int l = RD.Length;
            int sum = 0;
            int z = 0;
            CalculateStart.CreateEvent(l, "r = " + r.ToString(), 0);
            float d;
            //Инициализируем минимальное расстояние поиском первого ненулевого расстояния между точками
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
        /// Оценка первого корреляционного интеграла для размерности > 1 (многопоточная версия) 
        /// </summary>
        /// <param name="r">Расстояние между точками, относительно которого считают</param>
        /// <param name="dimension">Размерность вложения</param>
        /// <param name="RD">Массив расстояний между точками</param>
        /// <param name="len">Длина массива с точками</param>
        /// <param name="B">Массив размерности вложения</param>
        /// <returns>Оценка корреляционного интеграла для данного расстояния</returns>
        public static float GetCorrelatingIntegralFirst2(float r, ref float[] RD, int len, float[] A, int dimension, ref int zeros, int threadCount)
        {
            int lenRD = len * (len - 1) / 2;
            int segmentRD = lenRD / threadCount;
            int reminderRD = lenRD - segmentRD * threadCount;
            int endInd = (int)(2 * len - 1 - Math.Sqrt((1 - 2 * len) * (1 - 2 * len) - 8 * (segmentRD + reminderRD))) / 2;
            endInd -= 1;
            //Потоки
            Thread[] threads = new Thread[threadCount];
            //Потоковые экземпляры
            CorrelatingIntegralFirst2Thread[] cis = new CorrelatingIntegralFirst2Thread[threadCount];
            int startInd = 0;
            //Границы ответственности потоков
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
            //Первый поток пошел            
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
            //Сшиваем все результаты в один массив
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
        /// Оценка корреляционного интеграла для последующих расстояний для данной размерности (многопоточная версия)
        /// </summary>
        /// <param name="r">Расстояние, относительно которого идет счет</param>
        /// <param name="RD">Массив с расстояниями между точками для данной размерности</param>
        /// <param name="threadCount">Кол-во потоков</param>
        /// <returns>Оценка корреляционного интеграла для данного расстояния</returns>
        public static float GetCorrelatingIntegralContinious(float r, float[] RD, int threadCount)
        {
            int periodIntervalLength = RD.Length / threadCount;
            //Остаток (т.к. длина может не поделится на цело на число потоков)
            int remainder = RD.Length - periodIntervalLength * threadCount;
            //Период для первого потока = общий интервал + остаток
            int periodLengthLocal = periodIntervalLength + remainder;
            //Потоки
            Thread[] threads = new Thread[threadCount];
            //Потоковые экземпляры
            CorrelatingIntegralContiniousThread[] cis = new CorrelatingIntegralContiniousThread[threadCount];
            int startInd = 0;
            int endInd = periodLengthLocal - 1;
            //Первый поток пошел
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
        /// Оценка корреляционного интеграла для размерности вложения = 1 (многопоточная версия)
        /// Фактически используется только для того чтобы определить величину шага и базу для множителя, возводимого в степень.       
        /// </summary>
        /// <param name="r">Нач. расстояние между точками</param>
        /// <param name="step">Величина шага</param>
        /// <param name="stepCount">Количество шагов (точек для построения регрессии)</param>
        /// <param name="A">Массив с данными</param>
        /// <param name="minR">Минимальное расстояние между точками (значение определяется и изменяется в глобальном контексте)</param>                
        /// <param name="threadCount">Кол-во потоков</param>
        /// <returns>Оценка корреляционного интеграла для данной размерности</returns>
        public static float GetCorrelatingDimensionEstimationFirst(float r, ref float step, ref int stepCount, float[] A, ref float minR, ref float maxR, int threadCount, out float baseM)
        {
            int len = A.Length;
            len = len * (len - 1) / 2;
            //Расстояния между всеми парами точек на атракторе
            float[] RD = new float[len];
            //Макс. расстояние между двумя точками
            maxR = 0;
            //Мин. расстояние между двумя точками
            minR = 1;
            int zeros = 0;                        
            float ci = GetCorrelatingIntegralFirst(r, A, ref RD, ref maxR, ref minR, ref zeros);            
            int startRegres = 0;
            Calculus.CreateEvent(1, 0);            
            //step = minR*1.5F;            
            step = maxR / 3000;
            //количество точек для данного шага, максимального расстояния и базы для возведения в степень
            //максимальное расстояние для покрытия = 1/20 -я от максимального расстояния на аттракторе
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
            //если точек > 1
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
                    //Условия можно пересмотреть
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
                //Размер шага - с шага, с которого начинается линейный участок (все что меньше - шум)
                step = C[j - 1, 2] * 0.9f;
                //определение базы для возведения в степень при увеличении шага для заданного кол-ва точек
                baseM = (float)Math.Exp(Math.Log(maxR / (20 * step), Math.E) / stepCount);
                //Уменьшаем кол-во точек, т.к. последние точки начинают "спешиваться"
                stepCount = (int)(stepCount * 5 / 6);
                DataProcess.ExportArray(C, 2, "0.csv");
                System.GC.Collect(GC.GetGeneration(C));
                return rg.A;                                
            }
            else
            {                
                //регрессию провести нельзя
                return ERR_CORR_INTEGRAL;
            }
        }

        /// <summary>
        /// Оценка корреляционного интеграла для размерности вложения > 1 (многопоточная версия)
        /// </summary>
        /// <param name="step">Величина шага</param>
        /// <param name="stepCount">Количество шагов (точек для построения регрессии)</param>
        /// <param name="A">Массив с данными</param>
        /// <param name="dimension">Размерность, для которой производится расчет</param>
        /// <param name="minR">Минимальное расстояние между точками (значение только используется)</param>        
        /// <param name="threadCount">Кол-во потоков</param>
        /// <param name="allCD">Все корр. интегралы для расчета энтропии Колмогорова (переопределяем значение для использование после выхода)</param>
        /// <param name="zeros">Количество точек пересечения, т.е. нулевых расстояний(переопределяем значение для использование после выхода)</param>
        /// <param name="stepNum">№ шага, требуется для расчета энтропии</param>
        /// <param name="baseM">Основание для возведения в степень для множителя шага</param>
        /// <returns>Оценка корреляционного интеграла для данной размерности</returns>
        public static float GetCorrelatingDimensionEstimation(float step, int stepCount, float[] A, int dimension, float minR, int threadCount, ref float[,] allCD, ref int zeros, int stepNum, float baseM)
        {
            float[,] C = new float[stepCount+1, 2];            
            CalculateStart.CreateEvent(stepCount, "Estimation" + dimension.ToString(), 0);
            Calculus.CreateEvent(0, 0);
            //Длина нового массива B
            int newLen = A.Length - dimension + 1;
            //Число всех пар точек
            int len = newLen * (newLen - 1) / 2;
            //Расстояния между точками для данного измерения
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
                //Ошибка
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
            //если точек > 1
            if (st > 1)
            {
                System.GC.Collect(GC.GetGeneration(RD));                
                //Корр. размерности по 5-ти точкам
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
                    //относительные отклонения
                    float e1 = Math.Abs(cr - cr1) / cr;
                    float e2 = Math.Abs(cr - cr2) / cr;
                    float e3 = Math.Abs(cr - cr3) / cr;
                    float e4 = Math.Abs(cr - cr4) / cr;

                    if (e1 > E_DEV || e2 > E_DEV || e3 > E_DEV || e4 > E_DEV)
                    {
                        //одна из точек не лежит на прямой
                        //регрессию провести нельзя, нужно увеличить количество точек
                        //return ERR_CORR_INTEGRAL;
                    }
                    else
                    {
                        //Если корр. сумма = 0, значит слишком маленькое расстояние
                        if (C[i - 1, 1] != 0)
                        {
                            float e_sum = e1 + e2 + e3 + e4;
                            ///Запоминаем точку, у которой для всей линиии минимальное относительное отклонение
                            ///и все точки последовательно выше друг друга на некоторую величину (т.к. для случайного сигнала есть длинный горизонтальный
                            ///участок)
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
                //регрессию провести нельзя, нужно увеличить количество точек
                if (j == 0) {
                    DataProcess.ExportArray(C, 2, dimension.ToString() + ".csv");
                    return ERR_CORR_INTEGRAL;
                }
                //проводим регрессию через точки, у которых относительное отклонение от среднего минимально
                KRegressionFloat rg = MathProcess.SimpleRegression(C, j - 1, j + 3);
                C[stepCount, 0] = -1;
                C[stepCount, 1] = j;
                DataProcess.ExportArray(C, 2, dimension.ToString() + ".csv");
                System.GC.Collect(GC.GetGeneration(C));
                return rg.A;
            }
            else
            {
                //регрессию провести нельзя
                return ERR_CORR_INTEGRAL;
            }            
        }
        
        /// <summary>
        /// Определение корреляционного интеграла по временному ряду (многопоточная версия).
        /// Общая схема вычислений:
        /// 1. GetCorrelatingDimensionEstimationFirst:
        /// Определяем минимальное и максимальное расстояние на аттракторе. Определяем уровень шума [Шустер Г. Детерминированный хаос, с. 126] 
        /// и выбираем начальный шаг перекрывающий амплитуду шума. Максимальное расстояние не превышает 
        /// максимальное_расстояние_на_аттракторе/15. Затем вычисляем размер шага, и базу для множителя, возводимого в степень.
        /// Количество шагов задается вручную. Оптимальное значение: 12-25. Необходимо автоматически определять, но пока непонятно как.
        /// Затем собственно в цикле вычисляется корр. размерность, до тех пор пока либо она не будет уменьшаться в подряд 6 раз 
        /// или микроприбавляться (если это происходит, значит нужно уменьшить количество точек, по которым строится регрессия).
        /// 
        /// Для шумового сигнала с увеличением размерности количество точек на линейном участке будет сокращаться (достаточно быстро)
        /// </summary>
        /// <param name="r">Начальный шаг</param>                
        /// <param name="stepCount">Количество шагов</param>
        /// <param name="A">Входной массив</param>
        /// <param name="startDimension">Стартовая размерность вложения (1-я расчитывается обязательно)</param>
        /// <param name="endDimension">Конечная размерность вложения</param>
        /// <param name="threadCount">Кол-во потоков</param>
        /// <returns>Структуру: кореляционный интеграл, максимальная размерность системы, массив точек оценок корреляц. интеграла для различных размерностей вложения</returns>
        public static CorrelatingDimensionResult GetCorrelatingDimension(float r, int stepCount, float[] A, int startDimension, int endDimension, int threadCount)
        {
            int dimLen = endDimension - startDimension + 2;
            float[,] C = new float[dimLen, 3];
            CalculateStart.CreateEvent(dimLen, "Correlating Dimension = 0", threadCount);
            float minR = 1;
            float maxR = 0;
            C[0, 0] = 1;
            //размер шага определяется в GetCorrelatingDimensionEstimationFirst
            float step = 0;
            CorrelatingDimensionResult res = new CorrelatingDimensionResult();
            //Основание для возведения в степень для получения следующего шага определяется в GetCorrelatingDimensionEstimationFirst
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
                //Если количество самопересечений на траектории = 0
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
                    //Если количество уменьшений или микроувеличений больше 6 выходим
                    if (g > 6) break;                    
                }
                else
                {
                    if (g > 0) g--;
                }
                i++;
                //Освобождаем память, занятую в выполненных методах
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
                        
            //Оценка корр. размерности
            res.correlatingDimension = maxCorrDim;
            //Максимальная размерность системы
            res.maxDimension = (int)(2 * Math.Floor(res.correlatingDimension) + 1);
            //Стартовая размерность
            res.startDimension = startDimension;
            //Конечная размерность
            res.endDimension = endDimension;
            //До какой глубины вложения вошли
            res.lastDimension = dimension-1;
            //Размерность вложения, при которой достигается максимум корр. размерности
            res.maxCorrDimension = dimForMaxCorr;
            //Точки корр. размерности
            res.points = C;
            res.step = step;
            //Мин. расстояние на аттракторе
            res.minR = minR;
            //Макс. расстояние на аттракторе
            res.maxR = maxR;
            //Количество пересечений на последнем вложении
            res.crossers_count = (notZero > 0) ? zeros : notZero;
            //Считаем корр. энтропию
            float[,] allK = new float[i, stepCount];                        
            float[] KE = new float[i-1];
            //По умолчанию берем самый маленький шаг
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
            //Оценка энтропии Колмогорова
            //Находим наиболее линейный горизонтальный участок
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
            //или берем последнее значение, если линейного участка нет
            if (i>=2) res.entropyK = (indE>0) ? (float)(KE[indE] + KE[indE + 1] + KE[indE + 2]) / 3 : KE[i-2];
            System.GC.Collect(GC.GetGeneration(KE));
            System.GC.Collect(GC.GetGeneration(allCD));
            return res;
        }

        /// <summary>
        /// Определение максимального показателя Ляпунова
        /// </summary>
        /// <param name="A">Исходный ряд</param>
        /// <param name="embedingDimension">Размерность вложения (для реконструкции аттрактора ДС)</param>
        /// <param name="lag">Временной лаг (для реконструкции аттрактора ДС)</param>
        /// <param name="count">Количество раз оценки показателя (из разных начальных точек)</param>
        /// <returns>Усредненная оценка показателя Ляпунова</returns>
        public static float GetMaxLyapunovExponent(float[] A, int embedingDimension, int lag, int count)
        {
            //Реконструируем аттрактор
            //Длина траектории реконструированного аттрактора
            int lenAll = (int)Math.Floor((double)(A.Length / (lag + 1)));
            int len = lenAll - embedingDimension + 1;
            int countLag = lenAll / (lag + 1);
            //Траектория аттрактора в фазовом пространстве
            float[] B = new float[lenAll];            
            int i;
            int z = 0;
            //Восстанавливаем аттрактор в лаговом пространстве из исходного ряда (прореживаем)
            for (i = 0; i < A.Length - lag -1; i += lag + 1)
            {
                B[z] = A[i];
                z++;
            }
            //Определяем минимальное и максимальное расстояние между точками на аттракторе
            float minR = 10000000;
            float maxR = 0;            
            float d = 0;
            float max;
            CalculateStart.CreateEvent(len - 2, "Lyapunov = 0", 0);
            for (int k = 0; k < len - 1; k++)
            {
                for (int j = k + 1; j < len; j++)
                {
                    //Определяем расстояние между точками
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

            //Определяем минимальное расстояние, меньше которого должно быть расстояние между ближайшими точками
            //как 1/3000-я от размера аттарктора
            //float minD = maxR /3000;            
            float minD = maxR / 500;
            float maxD = maxR / 10;            
                                    
            //Индекс ближайшего соседа, до которого расстояние меньше minD;
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
            //Обходим все начальные точки
            float sumLyap = 0;
            float[,] Lyapunovs = new float[len,3];                        
            while (i < len - 100)
            {
                //Индекс последней точки траектории, разошедшей на превышающее расстояние,
                //нужна для расчета угла между векторами
                int indLastPoint = -1;
                //Угол между векторами (разошедшимися больше maxD)
                float angle = 1;
                float minDeltaAngel = 4;

                startInd = i;
                tmp = 0;
                c = 0;
                //Флаг завершения цикла - через бреак или нет
                isBreak = false;
                //Идем от начальной точки по алгоритму Бенетина
                double sumL = 0;
                while (c < count && startInd < len - 100)
                {
                    nearestInd = -1;
                    sLyapunov = 0;
                    sTime = 0;                                        
                    //Ищем соседа справа
                    for (int j = startInd + (embedingDimension * embedingDimension); j < len - (embedingDimension * embedingDimension); j++)
                    {
                        //Определяем расстояние между точками (как максимум среди координат нового вектора)
                        max = 0;
                        for (int dim = 0; dim < embedingDimension; dim++)
                        {
                            d = Math.Abs(B[startInd + dim] - B[j + dim]);
                            if (d > max) max = d;
                        }
                        d = max;
                        if (d < minD)
                        {
                            //Если ищем соседа, после движения по траектории,
                            //то необходимо выбрать с наименьшим отклонением вектора по углу
                            if (indLastPoint > -1)
                            {                                
                                //Скалярное произведение между векторами
                                float scalarProduct = 0;
                                float moduleVectorJ = 0;
                                float moduleVectorLast = 0;
                                for (int dim = 0; dim < embedingDimension; dim++)
                                {
                                    scalarProduct += B[j + dim] * B[indLastPoint + dim];
                                    moduleVectorJ += B[j + dim] * B[j + dim];
                                    moduleVectorLast += B[indLastPoint + dim] * B[indLastPoint + dim];
                                }
                                //Угол между векторами
                                float newAngle = (float)Math.Acos(scalarProduct / Math.Sqrt(moduleVectorJ * moduleVectorLast));
                                float deltaAngle = Math.Abs(angle - newAngle);
                                if (deltaAngle < minDeltaAngel && j < len - embedingDimension - 7)
                                {
                                    minDeltaAngel = deltaAngle;
                                    nearestInd = j;
                                }
                            }
                            //первый заход по траектории
                            else
                            {
                                nearestInd = j;
                                //Ближйший сосед найден
                                goto find_neighbor;
                            }
                        }
                    }                    
                find_neighbor:
                    //Ближайший сосед найден
                    if (nearestInd > 0)
                    {
                        //Отслеживаем расстояние между точками с индексами startInd и nearestInd
                        //пока оно меньше maxD
                        //Начальное расстояние между точками
                        S[c, 0] = d;
                        //Время (кол-во шагов), после которого две точки расходятся на максимально определенное расстояние
                        int timeCount = 0;
                        int beginStart = startInd;
                        while (d < maxD && timeCount < 5000 && startInd < len - (embedingDimension * embedingDimension) && nearestInd < len - (embedingDimension * embedingDimension))
                        {
                            startInd++;
                            nearestInd++;
                            //Определяем расстояние между точками
                            max = 0;
                            for (int dim = 0; dim < embedingDimension; dim++)
                            {
                                d = Math.Abs(B[startInd + dim] - B[nearestInd + dim]);
                                if (d > max) max = d;
                            }
                            d = max;
                            timeCount++;
                        }
                        //Если удалось сделать продвижение
                        if (timeCount > 7)
                        {
                            //Если это первая разошедшееся точка запоминаем угол между векторами
                            if (indLastPoint == -1)
                            {
                                //Скалярное произведение между векторами
                                float scalarProduct = 0;
                                float moduleVectorJ = 0;
                                float moduleVectorLast = 0;
                                for (int dim = 0; dim < embedingDimension; dim++)
                                {
                                    scalarProduct += B[startInd + dim] * B[nearestInd + dim];
                                    moduleVectorJ += B[startInd + dim] * B[startInd + dim];
                                    moduleVectorLast += B[nearestInd + dim] * B[nearestInd + dim];
                                }
                                //Угол между векторами
                                angle = (float)Math.Acos(scalarProduct / Math.Sqrt(moduleVectorJ * moduleVectorLast));
                            }
                            //Запоминаем индекс точки
                            indLastPoint = nearestInd;
                            //Конечное расстояние между точками
                            S[c, 1] = d;
                            //Время, которое потребовалось для разбегания траекторий
                            S[c, 2] = timeCount;
                            S[c, 3] = beginStart;
                            S[c, 4] = nearestInd;                            
                            //Новая стартовая точка - последний сосед
                            //startInd = nearestInd;                        
                            c++;
                        }
                        else if (timeCount == 0)
                        {
                            startInd++;
                        }
                        //DataProcess.ExportArray(S, "_S"+c.ToString()+".csv");
                    }
                    //Ближайший сосед не найден
                    else
                    {
                        //Если хотя бы одно значение получили
                        if (c > 0)
                        {
                            //DataProcess.ExportArray(S, "_lyapunovs" + i + ".csv");
                            sLyapunov = 0;
                            sTime = 0;
                            sumL = 0;
                            //Оцениваем показатель Ляпунова для данной начальной точки
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
                        //Выходим и переходим к следующей нач. точке
                        break;
                    }
                }
                //Если цикл отработал до конца
                if (!isBreak)
                {
                    //Если хотя бы одно значение получили
                    if (c > 0)
                    {
                        //DataProcess.ExportArray(S, "_lyapunovs" + i + ".csv");                        
                        sLyapunov = 0;
                        sTime = 0;
                        sumL = 0;
                        //Оцениваем показатель Ляпунова для данной начальной точки
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
        /// Расчет индекса вариации по Старченко. Анализируемый участок в массиве разбивается на окна и для каждого окна 
        /// рассчитывается индекс вариации. 
        /// </summary>
        /// <param name="A">Массив с данными</param>
        /// <param name="startGInd">Индекс в массиве, с которого начинается анализируемый участок в массиве A</param>
        /// <param name="endGInd">Индекс в массиве, которым кончается анализируемый участок в массиве A</param>
        /// <param name="startSegmentLength">Стартовая длина сегмента, по которому расчитывается индекс. Не должна быть слишком маленькой, иначе не будет ярковыраженного линейного участка</param>
        /// <param name="endSegmentLength">Конечная длина сегмента, по которому расчитывается индекс. Должна быть меньше или равна длине окна</param>
        /// <param name="windowLength">Длина окна. Должна быть меньше или равна длине массива A</param>
        /// <returns></returns>
        public static float[] VariationIndex(float[] A, int startGInd, int endGInd, int startSegmentLength, int endSegmentLength, int windowLength, int numRegressionPoints)
        {
            //Длина рабочего фрагмента массива
            int lenGlobal = endGInd - startGInd + 1;
            //Кол-во окон
            int countResults = (int)lenGlobal - windowLength + 1;
            //Рез-ты для каждого окна + индекс вариации, рассчитанный для всего фрагмента (т.е. когда длина окна = длине всего фрагмента)
            float[] resGlobal = new float[countResults];
            //Счетчик окон
            int g = 0;            
            float lnStartSegmentLength = (int)Math.Log(startSegmentLength);
            //Основание степени для логарифмической шкалы, в рамках которой строится регрессия
            float powerVal = (float)(Math.Log(endSegmentLength) - lnStartSegmentLength) / (numRegressionPoints - 1);
            //Вычисляем индекс вариации для каждого "окна"
            for (int startInd = startGInd; startInd <= endGInd - (windowLength - 1); startInd++)
            {
                int endInd = startInd + (windowLength - 1);
                //Массив для регрессии  - расчета индекса вариации для анализируемого окна                
                float[,] res = new float[numRegressionPoints, 2];
                //счетчик для регрессии
                int i = 0;
                //счетчик для сегментов
                int ii = 0;
                //Пробегаем длину отрезков разбиения от startN до endN 
                for (int currentSegmentLength = startSegmentLength; currentSegmentLength <= endSegmentLength; currentSegmentLength = (int)Math.Exp(lnStartSegmentLength + powerVal * ii))
                {
                    //Находим сумму размахов (разность между наибольшим и наименьшим) по отрезкам
                    float sum = 0;
                    for (int j = startInd; j <= endInd - currentSegmentLength; j = j + currentSegmentLength)
                    {
                        float min = A[j];
                        float max = A[j];
                        //Ищем максимум и минимум на данном отрезке
                        for (int k = j; k <= j + currentSegmentLength; k++)
                        {
                            if (A[k] < min) min = A[k];
                            if (A[k] > max) max = A[k];
                        }
                        //Складываем размахи
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
    /// Вспомогательный класс, для организации параллельных вычислений корреляционного интеграла
    /// </summary>
    public class CorrelatingIntegralContiniousThread
    {
        //Расстояние между точками
        public float r;
        //Массив расстояний между всеми точками
        public float[] RD;
        //Стартовый индекс в RD
        public int startIndex;
        //Конечный индекс в RD
        public int endIndex;
        //Кол-во расстояний превышающих r
        public int sum;
        //Кол-во потоков
        public int threadNum;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="r">Расстояние между точками, относительно которого считают</param>        
        /// <param name="RD">Массив расстояний между точками</param>
        /// <param name="startIndex">Стартовый индекс в RD</param>
        /// <param name="endIndex">Конечный индекс в RD</param>
        /// <param name="threadNum">Кол-во потоков</param>
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
        /// Вычисляет кол-во расстояний больше r
        /// </summary>
        public void GetCorrelatingIntegralContinious()
        {
            //int j = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                //if (RD[i] < r && RD[i] > 0) sum++;
                if (RD[i] < r) sum++;
                
                //Увеличиваем прогресбар                                
                //Считает гораздо быстрее и не виснет при потоках > 2
                //j++;
                //Calculus.CreateEvent(j, threadNum);
            }
        }
    }

    /// <summary>
    /// Вспомогательный класс, для организации параллельных вычислений корреляционного интеграла 
    /// (при размерности вложения > 2)
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
        /// Конструктор
        /// </summary>
        /// <param name="r">Расстояние между точками, относительно которого считают</param>
        /// <param name="B">Многомерный массив точек в пространстве вложения больше 2</param>
        /// <param name="startIndex">Стартовый индекс в B</param>
        /// <param name="endIndex">Конечный индекс в B</param>
        /// <param name="threadNum">Кол-во потоков</param>
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
        /// Вычисляет кол-во расстояний больше r и запоминает все расстояния в RD
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
                    //Считает гораздо быстрее и не виснет при потоках > 2 если закомментить
                    //Calculus.CreateEvent(z, threadNum);
                }
            }
        }
    }
}