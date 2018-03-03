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

///Часть класса, содержащая RS-методы

namespace ChaosExpert
{
    public partial class ChaosLogic
    {
        /// <summary>
        /// R/S для одного периода
        /// </summary>
        /// <param name="A">Массив данных</param>
        /// <param name="start_index">Нач. индекс массива (используемые данные)</param>
        /// <param name="end_index">Конечный индекс массива</param>
        /// <returns>R/S - отношения размаха к стандартному отклонению</returns>
        public static float GetRS(PointTimeThresholdChange[] A, int start_index, int end_index)
        {
            //среднее
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
                //сумма накопленных отклонений
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
                //стандартное отклоненение
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
        /// R/S для одного периода
        /// </summary>
        /// <param name="A">Массив данных</param>
        /// <param name="start_index">Нач. индекс массива (используемые данные)</param>
        /// <param name="end_index">Конечный индекс массива</param>
        /// <returns>R/S - отношения размаха к стандартному отклонению</returns>
        public static float GetRS(float[] A, int start_index, int end_index)
        {
            //среднее
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
                //сумма накопленных отклонений
                sum += deviation[j];
                if (sum > max) max = sum;
                if (sum < min) min = sum;
            }

            if (len > 1)
            {
                //стандартное отклоненение
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
        /// R/S анализ ряда (последовательные вычисления)
        /// </summary>
        /// <param name="A">Массив входных данных</param>
        /// <param name="periodLength">Стартовая длина периода</param>
        /// <param name="startGIndex">Начальный индекс в массиве, с которого берутся данные</param>
        /// <param name="endGIndex">Конечный индекс в массиве, до которого берутся данные (включительно)</param>
        /// <param name="maxPeriodLength">Максимальная длина периода, до которого ведется расчет</param>
        /// <returns>Таблицу полученных R/S-точек и коэффициенты регрессии (A - соответствет H)</returns>
        public static ResultRS RS_Analyse(float[] A, int periodLength, int startGIndex, int endGIndex, int maxPeriodLength)
        {
            //кол-во RS-результатов
            int resLength = maxPeriodLength - periodLength + 1;
            //результаты RS-анализа
            PointRS[] result = new PointRS[resLength];
            //последний индекс в массиве
            int lastIndex = endGIndex;
            //первый индекс в массиве
            //конечный локальный индекс
            int startIndex, endIndex, startIndexNow;
            //ср. значение RS для данного периода
            float averageRS;
            int j = 0;
            //текущая длина периода            
            int currentPeriodLength = periodLength;
            //Кол-во периодов
            int periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
            //Устанавливаем длину прогресбара
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
                //Кол-во периодов
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //Увеличиваем прогресбар
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
        /// R/S анализ ряда (последовательные вычисления)
        /// </summary>
        /// <param name="A">Массив входных данных</param>
        /// <param name="periodLength">Стартовая длина периода</param>
        /// <param name="startGIndex">Начальный индекс в массиве, с которого берутся данные</param>
        /// <param name="endGIndex">Конечный индекс в массиве, до которого берутся данные (включительно)</param>
        /// <param name="maxPeriodLength">Максимальная длина периода, до которого ведется расчет</param>
        /// <returns>Таблицу полученных R/S-точек и коэффициенты регрессии (A - соответствет H)</returns>
        public static ResultRS RS_Analyse(PointTimeThresholdChange[] A, int periodLength, int startGIndex, int endGIndex, int maxPeriodLength)
        {
            //кол-во RS-результатов
            int resLength = maxPeriodLength - periodLength + 1;
            //результаты RS-анализа
            PointRS[] result = new PointRS[resLength];
            //последний индекс в массиве
            int lastIndex = endGIndex;
            //первый индекс в массиве
            //конечный локальный индекс
            int startIndex, endIndex;
            //ср. значение RS для данного периода
            float averageRS;
            int j = 0;
            //текущая длина периода            
            int currentPeriodLength = periodLength;
            //Кол-во периодов
            int periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
            //Устанавливаем длину прогресбара
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
                //Кол-во периодов
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //Увеличиваем прогресбар
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
        /// R/S анализ ряда (параллельные вычисления)
        /// </summary>
        /// <param name="A">Массив входных данных</param>
        /// <param name="periodLength">Стартовая длина периода</param>
        /// <param name="startGIndex">Начальный индекс в массиве, с которого берутся данные</param>
        /// <param name="endGIndex">Конечный индекс в массиве, до которого берутся данные (включительно)</param>
        /// <param name="maxPeriodLength">Максимальная длина периода, до которого ведется расчет</param>
        /// <param name="threadCount">Количество потоков (распараллеливание вычислений)</param>
        /// <returns>Таблицу полученных R/S-точек и коэффициенты регрессии (A - соответствет H)</returns>
        public static ResultRS RS_Analyse(float[] A, int currentPeriodLength, int startGIndex, int endGIndex, int maxPeriodLength, int threadCount)
        {
            //кол-во RS-результатов
            int resLength = maxPeriodLength - currentPeriodLength + 1;
            //результаты RS-анализа
            PointRS[] result = new PointRS[resLength];
            //Длина периода на поток
            int periodIntervalLength = resLength / threadCount;
            //Остаток (т.к. длина может не поделится на цело на число потоков)
            int remainder = resLength - periodIntervalLength * threadCount;
            //Максимальный период для первого потока = общий интервал + остаток
            int maxPeriodLengthLocal = periodIntervalLength + remainder + currentPeriodLength - 1;
            //Потоки
            Thread[] threads = new Thread[threadCount];
            //Потоковые экземпляры, проводящие R/S анализ
            RS_AnalyseThread[] rsat = new RS_AnalyseThread[threadCount];
            //Первый поток пошел
            rsat[0] = new RS_AnalyseThread(A, currentPeriodLength, startGIndex, endGIndex, maxPeriodLengthLocal, 0);
            threads[0] = new Thread(new ThreadStart(rsat[0].RS_Analyse));
            //Устанавливаем длину прогресбара для 1 процесса
            resLength = maxPeriodLengthLocal - currentPeriodLength + 1;
            CalculateStart.CreateEvent(resLength, "R/S analyse 1", 0);
            threads[0].Start();
            //Запускаем остальные потоки
            int j = 2;
            for (int i = 1; i < threadCount; i++)
            {
                //с какого периода начинать считать
                currentPeriodLength = maxPeriodLengthLocal + 1;
                //до какого периода считать
                maxPeriodLengthLocal += periodIntervalLength;
                //Запуск потока
                rsat[i] = new RS_AnalyseThread(A, currentPeriodLength, startGIndex, endGIndex, maxPeriodLengthLocal, i);
                threads[i] = new Thread(new ThreadStart(rsat[i].RS_Analyse));
                //Устанавливаем длину прогресбара для i процесса
                resLength = maxPeriodLengthLocal - currentPeriodLength + 1;
                CalculateStart.CreateEvent(resLength, "R/S analyse " + j.ToString(), i);
                threads[i].Start();
                j++;
            }
            //Ждем выполнения всех потоков
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
            //Сшиваем все результаты в один массив
            int indexResult = 0;
            for (int i = 0; i < threadCount; i++)
            {
                rsat[i].result.CopyTo(result, indexResult);
                indexResult += rsat[i].result.Length;
            }
            //Регрессия
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
    /// Вспомогательный класс, для организации паралелльных вычислений в R\S анализе
    /// </summary>
    public class RS_AnalyseThread
    {
        //Входной массив
        public float[] A;
        //Текущая длина периода
        public int currentPeriodLength;
        //С какого индекса в А считать (включительно)
        public int startGIndex;
        //До какого индекса в А считать (включительно)
        public int endGIndex;
        //Макс. длина периода
        public int maxPeriodLength;
        //Результаты R/S анализа для данного потока
        public PointRS[] result;
        //Номер потока
        public int threadNum;

        /// <summary>
        /// Конструктор, подготавливает поток для вычислений R/S анализа ряда
        /// </summary>
        /// <param name="A">Массив входных данных</param>
        /// <param name="currentPeriodLength">Стартовая длина периода</param>
        /// <param name="startGIndex">Начальный индекс в массиве, с которого берутся данные</param>
        /// <param name="endGIndex">Конечный индекс в массиве, до которого берутся данные (включительно)</param>
        /// <param name="maxPeriodLength">Максимальная длина периода, до которого ведется расчет</param>
        /// <param name="threadNum">Номер потока (распараллеливание вычислений)</param>        
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
        /// Вычисление R/S анализа в потоке
        /// </summary>
        public void RS_Analyse()
        {
            //кол-во RS-результатов
            int resLength = maxPeriodLength - currentPeriodLength + 1;
            //результаты RS-анализа
            result = new PointRS[resLength];
            //последний индекс в массиве
            int lastIndex = endGIndex;
            //первый индекс в массиве
            //конечный локальный индекс
            int startIndex, endIndex;
            //ср. значение RS для данного периода
            double averageRS;
            int j = 0;
            //Кол-во периодов
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
                //Кол-во периодов
                periodCount = (endGIndex - startGIndex + 1) / currentPeriodLength;
                //Увеличиваем прогресбар                
                Calculus.CreateEvent(j, threadNum);
            }
            while (currentPeriodLength <= maxPeriodLength);
        }
    }
}
