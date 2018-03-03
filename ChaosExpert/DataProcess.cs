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

namespace ChaosExpert
{
    /// <summary>
    /// Класс для работы с данными: загрузка из файлов, обработка, очистка
    /// </summary>
    public class DataProcess
    {
        public const long INFINITY = 999999999999999999;


        /// <summary>
        /// Очищает файл от повторных записей и с нулевыми объемами. Также удаляет знак ":" в строках.
        /// </summary>
        /// <param name="fileName">имя файла</param>
        /// <returns>количество удаленных некорректных строк</returns>        
        public static int ClearFile(string fileName)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string clearFile = allFile.Replace(":", "");
            string[] lines = clearFile.Split('\n');
            string prev_time = "";
            string time_str = "";
            fInput.Close();
            fInput = File.Open(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fInput);
            int errors = 0;
            writer.Write(lines[0]);
            StringBuilder str = new StringBuilder();

            int i = 1;
            string[] str_tmp = lines[i].Split(';');
            time_str = str_tmp[0] + str_tmp[1];
            if (time_str == prev_time || Convert.ToDouble(str_tmp[5]) == 0)
            {
                errors++;
            }
            else
            {
                lines[i] = lines[i];
                writer.Write(lines[i]);
            }
            prev_time = time_str;

            for (i = 2; i < lines.Length - 1; i++)
            {
                str_tmp = lines[i].Split(';');
                time_str = str_tmp[0] + str_tmp[1];
                if (time_str == prev_time || Convert.ToDouble(str_tmp[5]) == 0)
                {
                    errors++;
                }
                else
                {
                    if (lines[i].Length > 3)
                    {
                        lines[i] = "\n" + lines[i];
                        writer.Write(lines[i]);
                    }
                }
                prev_time = time_str;
            }
            writer.Close();
            fInput.Close();
            return errors;
        }

        /// <summary>
        /// Загрузка одного столбца с данными из файла. Первая строка может содержать текст. Разделитель ";"
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="column_index">Индекс столбца (начиная с 0), подлежащего загрузке</param>
        /// <returns>Массив double</returns>
        public static double[] LoadData(string fileName, int column_index)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0)
            {
                iStart = 1;
            }
            else
            {
                iStart = 0;
            }

            double[] A = new double[Lines.Length - iStart];
            //CalculateStart.CreateEvent(A.Length, "Load data from " + fileName + "...");            
            int j;

            //Если есть несколько столбцов 
            if (Lines[1].IndexOf(";") > -1)
            {
                for (int i = 0; i < Lines.Length - iStart; i++)
                {
                    j = i + iStart;
                    string[] str_tmp = Lines[j].Split(';');
                    A[i] = (str_tmp[column_index] != "") ? Convert.ToDouble(str_tmp[column_index]) : 0;
                    //Calculus.CreateEvent(j, Lines.Length);
                }
            }
            //иначе один столбец
            else
            {
                for (int i = 0; i < Lines.Length - iStart; i++)
                {
                    j = i + iStart;
                    A[i] = (Lines[j] != "") ? Convert.ToDouble(Lines[j]) : 0;
                    //Calculus.CreateEvent(j, Lines.Length);
                }
            }
            fInput.Close();
            return A;
        }

        /// <summary>
        /// Загрузка одного столбца с данными из файла. Первая строка может содержать текст. Разделитель ";"
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="column_index">Индекс столбца (начиная с 0), подлежащего загрузке</param>
        /// <returns>Массив float</returns>
        public static float[] LoadDataFloat(string fileName, int column_index, string delimiter)
        {
            delimiter = (delimiter == "") ? ";" : delimiter;
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            float[] A = new float[Lines.Length - iStart];
            //CalculateStart.CreateEvent(A.Length, "Load data from " + fileName + "...");
            int j;
            //Если есть несколько столбцов 
            if (Lines[1].IndexOf(delimiter) > -1)
            {
                for (int i = 0; i < Lines.Length - iStart; i++)
                {
                    j = i + iStart;
                    string[] str_tmp = Lines[j].Split(';');
                    A[i] = (str_tmp[column_index] != "") ? (float)Convert.ToDouble(str_tmp[column_index]) : 0;
                    //Calculus.CreateEvent(j, Lines.Length);
                }
            }
            //иначе один столбец
            else
            {
                int k = 0;
                for (int i = 0; i < Lines.Length - iStart; i++)
                {
                    j = i + iStart;
                    //Если не пустая строка
                    if (String.Compare(Lines[j], "\r") != 0)
                    {
                        A[k] = (float)Convert.ToDouble(Lines[j]);
                        k++;
                    }
                    //Calculus.CreateEvent(j, Lines.Length);
                }
                float[] B = new float[k];
                for (int i = 0; i < k; i++) B[i] = A[i];
                A = B;
            }
            fInput.Close();
            return A;
        }


        /// <summary>
        /// Загрузка данных
        /// Формат файла:
        /// DATE;TIME;OPEN;HIGH;LOW;CLOSE
        /// 19990601;110000;1.43000;1.43200;1.40300;1.43000
        /// 19990601;1101;1.42700;1.42700;1.42700;1.42700
        /// </summary>
        /// <returns>Массив с данными</returns>
        public static Point[] LoadData(string fileName)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            Point[] A = new Point[Lines.Length - 1];
            string prev_val = "";
            string prev_qval = "";
            int i = 1;
            while (i < Lines.Length)
            {
                string[] str_tmp = Lines[i].Split(';');
                //Дата                                
                string year = str_tmp[0].Substring(0, 4);
                string month = str_tmp[0].Substring(4, 2);
                string day = str_tmp[0].Substring(6, 2);

                //Время
                string hour = str_tmp[1].Substring(0, 2);
                string min = str_tmp[1].Substring(2, 2);

                //Дата-время
                string myDateTimeValue = day + "." + month + "." + year + " " + hour + ":" + min + ":00";
                //string myDateTimeValue = "06.02.1999 12:15:00";
                IFormatProvider culture = new CultureInfo("ru-RU", true);

                A[i - 1].dateTime = DateTime.Parse(myDateTimeValue, culture, DateTimeStyles.NoCurrentDateDefault);
                A[i - 1].dateTimeStr = myDateTimeValue;
                //A[i - 1].candle[OPEN] = Convert.ToDecimal(str_tmp[2]);
                //High
                A[i - 1].val = (float)Convert.ToDouble(str_tmp[3]);
                //A[i - 1].candle[LOW] = Convert.ToDecimal(str_tmp[4]);
                //Volume
                A[i - 1].qval = (float)Convert.ToDouble(str_tmp[5]);
                prev_val = str_tmp[3];
                prev_qval = str_tmp[5];
                i++;
            }
            fInput.Close();
            GC.Collect(GC.GetGeneration(fInput));
            GC.Collect(GC.GetGeneration(srInput));
            GC.Collect(GC.GetGeneration(Lines));
            GC.Collect(GC.GetGeneration(allFile));
            return A;
        }

        /// <summary>
        /// Загрузка данных вместе со значением времени и объема в формате
        /// DATE;TIME;OPEN;HIGH;LOW;CLOSE;VOLUME
        /// 19990601;110000;1.43000;1.43200;1.40300;1.43000
        /// 19990601;1101;1.42700;1.42700;1.42700;1.42700
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="index">Загружаемый столбец значений (Open-0, Close-3)</param>
        /// <returns></returns>
        public static Point[] LoadDataTimeFloat(string fileName, int index)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            Point[] A = new Point[Lines.Length - 1];
            string prev_val = "";
            string prev_qval = "";
            int i = 1;
            CalculateStart.CreateEvent(A.Length + 1, "Load data from " + fileName + "...", 0);
            while (i < Lines.Length)
            {
                string[] str_tmp = Lines[i].Split(',');
                //Дата                                
                string year = str_tmp[0].Substring(0, 4);
                string month = str_tmp[0].Substring(4, 2);
                string day = str_tmp[0].Substring(6, 2);

                //Время
                string hour = str_tmp[1].Substring(0, 2);
                string min = str_tmp[1].Substring(2, 2);

                //Дата-время
                string myDateTimeValue = day + "." + month + "." + year + " " + hour + ":" + min + ":00";
                //string myDateTimeValue = "06.02.1999 12:15:00";
                IFormatProvider culture = new CultureInfo("ru-RU", true);

                A[i - 1].dateTime = DateTime.Parse(myDateTimeValue, culture, DateTimeStyles.NoCurrentDateDefault);
                A[i - 1].dateTimeStr = myDateTimeValue;
                //A[i - 1].candle[OPEN] = Convert.ToDecimal(str_tmp[2]);
                //High
                A[i - 1].val = (float)Convert.ToDouble(str_tmp[index]);
                //A[i - 1].candle[LOW] = Convert.ToDecimal(str_tmp[4]);
                //Volume
                A[i - 1].qval = (float)Convert.ToDouble(str_tmp[5]);
                prev_val = str_tmp[index];
                prev_qval = str_tmp[5];
                Calculus.CreateEvent(i, 0);
                i++;
            }
            fInput.Close();
            return A;
        }


        /// <summary>
        /// Возвращает формат даты входной строки в файле с данными
        /// </summary>
        /// <param name="format">
        /// Название формата:
        /// classic: 02.11.2006;12:34
        /// finam: 20060703;1040
        /// </param>
        /// <returns></returns>
        private static InputDateFormat _setInputFormat(string format)
        {
            InputDateFormat input = new InputDateFormat();
            if (format == "classic")
            {
                input.formatName = format;
                input.year = 6;
                input.month = 3;
                input.day = 0;
                input.hour = 11;
                input.min = 14;
                input.format = "d.m.y,h:min";
            }
            else if (format == "finam")
            {
                input.formatName = format;
                input.format = "ymd;hmin";
                input.year = 0;
                input.month = 4;
                input.day = 6;
                input.hour = 9;
                input.min = 11;
            }
            return input;
        }

        /// <summary>
        /// Получение индексов начала и конца по датам начала и конца
        /// </summary>
        /// <param name="dateStr">Строка с датой в формате 02.12.1994.10.40</param>
        /// <param name="startInd">Стартовый индекс в массиве строк</param>
        /// <param name="startInd">Стартовый индекс в массиве строк</param>
        /// <param name="inputFormat">Формат файла (finam/classic) </param>
        /// <param name="date">Дата, соответсвующая данному индексу </param>
        /// <returns>Индекс в масиве для данной даты</returns>       
        private static int _getIndexByDate(string dateStr, int startInd, string[] Lines, InputDateFormat inputFormat, ref string date)
        {
            string[] dateComponents = dateStr.Split('.');
            int startDay = Convert.ToInt32(dateComponents[0]);
            int startMonth = Convert.ToInt32(dateComponents[1]);
            int startYear = Convert.ToInt32(dateComponents[2]);
            int startHour = Convert.ToInt32(dateComponents[3]);
            int startMin = Convert.ToInt32(dateComponents[4]);
            int i = startInd;
            int len = Lines.Length;
            //04.04.2007,5:19,158.34,158.38,158.34,158.37,23
            string hourStr;
            string minStr;

            while (Convert.ToInt32(Lines[i].Substring(inputFormat.year, 4)) < startYear && i + 10 < len) i += 10;
            while (Convert.ToInt32(Lines[i].Substring(inputFormat.month, 2)) < startMonth && i + 8 < len) i += 8;
            while (Convert.ToInt32(Lines[i].Substring(inputFormat.day, 2)) < startDay && i + 5 < len) i += 5;

            //classic
            if (inputFormat.hour == 11)
            {
                string[] comps = Lines[i].Split(',');
                string[] times = comps[1].Split(':');
                hourStr = times[0];
                minStr = times[1];

                while (Convert.ToInt32(hourStr) < startHour && i + 3 < len)
                {
                    i += 3;
                    comps = Lines[i].Split(',');
                    times = comps[1].Split(':');
                    hourStr = times[0];
                }
                while (Convert.ToInt32(minStr) < startMin && i + 1 < len)
                {
                    i += 1;
                    comps = Lines[i].Split(',');
                    times = comps[1].Split(':');
                    minStr = times[1];
                }
            }
            //finam
            else
            {
                hourStr = Lines[i].Substring(inputFormat.hour, 2);
                minStr = Lines[i].Substring(inputFormat.min, 2);

                while (Convert.ToInt32(hourStr) < startHour && i + 3 < len)
                {
                    i += 3;
                    hourStr = Lines[i].Substring(inputFormat.hour, 2);
                }
                while (Convert.ToInt32(minStr) < startMin && i + 1 < len)
                {
                    i += 1;
                    minStr = Lines[i].Substring(inputFormat.min, 2);
                }
            }
            date = Lines[i].Substring(inputFormat.day, 2) + "." + Lines[i].Substring(inputFormat.month, 2) + "." + Lines[i].Substring(inputFormat.year, 4) + " " + hourStr + ":" + minStr;
            return i;
        }

        /// <summary>
        /// Загрузка данных за период
        /// DATE;TIME;OPEN;HIGH;LOW;CLOSE;VOLUME
        /// 01.06.1999;11:00;1.43000;1.43200;1.40300;1.43000        
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="index">Загружаемый столбец значений (Open-0, Close-3)</param>
        /// <param name="timePeriod">02.12.1994.10.40-26.11.2006.14.35</param>        
        /// <param name="delimiter">Разделитель столбцов</param>
        /// <param name="format">Формат файла (finam/classic)</param>
        /// <param name="periodStr">Реально охватываемый период (переопределяемое значение)</param>
        /// <returns></returns>
        public static float[] LoadDataTimePeriod(string fileName, int index, string timePeriod, char delimiter, string format, ref string periodStr)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            string[] dateComponents = timePeriod.Split('-');
            string date = "";
            InputDateFormat inputFormat = _setInputFormat(format);
            int start = _getIndexByDate(dateComponents[0], iStart, Lines, inputFormat, ref date);
            periodStr = date;
            int end = _getIndexByDate(dateComponents[1], start, Lines, inputFormat, ref date);
            periodStr += " - " + date;
            float[] A = new float[end - start + 1];
            int j = 0;
            for (int i = start; i <= end; i++)
            {
                string[] str_tmp = Lines[i].Split(delimiter);
                A[j] = (str_tmp[index] != "") ? (float)Convert.ToDouble(str_tmp[index]) : 0;
                j++;
            }
            fInput.Close();
            GC.Collect(GC.GetGeneration(fInput));
            GC.Collect(GC.GetGeneration(srInput));
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return A;
        }

        /// <summary>
        /// Загрузка данных за период - средние значения по 2-м столбцам
        /// DATE;TIME;OPEN;HIGH;LOW;CLOSE;VOLUME
        /// 01.06.1999;11:00;1.43000;1.43200;1.40300;1.43000        
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="index">Загружаемый столбец значений (Open-0, Close-3)</param>
        /// <param name="timePeriod">02.12.1994.10.40-26.11.2006.14.35</param>
        /// <returns></returns>
        public static float[] LoadDataTimePeriod(string fileName, int index1, int index2, string timePeriod, char delimiter, string format, ref string periodStr)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            string[] dateComponents = timePeriod.Split('-');
            string date = "";
            InputDateFormat inputFormat = _setInputFormat(format);
            int start = _getIndexByDate(dateComponents[0], iStart, Lines, inputFormat, ref date);
            periodStr = date;
            int end = _getIndexByDate(dateComponents[1], start, Lines, inputFormat, ref date);
            periodStr += " - " + date;
            float[] A = new float[end - start + 1];
            int j = 0;
            for (int i = start; i <= end; i++)
            {
                string[] str_tmp = Lines[i].Split(delimiter);
                float val1 = (str_tmp[index1] != "") ? (float)Convert.ToDouble(str_tmp[index1]) : 0;
                float val2 = (str_tmp[index2] != "") ? (float)Convert.ToDouble(str_tmp[index2]) : 0;
                A[j] = (val1 + val2) / 2;
                j++;
            }
            fInput.Close();
            GC.Collect(GC.GetGeneration(fInput));
            GC.Collect(GC.GetGeneration(srInput));
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return A;
        }

        /// <summary>
        /// Загрузка цены  за период (средние значения по 4-м столбцам), объема и даты
        /// </summary>

        public static Point[] LoadDataFullTimePeriod(string fileName, int indexFirst, string timePeriod, char delimiter, string format, ref string periodStr)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            allFile.Replace("\n\n", "");
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            string[] dateComponents = timePeriod.Split('-');
            string date = "";
            InputDateFormat inputFormat = _setInputFormat(format);
            int start = _getIndexByDate(dateComponents[0], iStart, Lines, inputFormat, ref date);
            periodStr = date;
            int end = _getIndexByDate(dateComponents[1], start, Lines, inputFormat, ref date);
            periodStr += " - " + date;
            Point[] A = new Point[end - start + 1];
            int j = 0;
            for (int i = start; i <= end; i++)
            {
                //Дата
                string year = Lines[i].Substring(inputFormat.year, 4);
                string month = Lines[i].Substring(inputFormat.month, 2);
                string day = Lines[i].Substring(inputFormat.day, 2);
                //Время

                string[] comps = Lines[i].Split(delimiter);                
                string hour = "";
                string min = "";
                if (inputFormat.formatName == "classic")
                {
                    string[] times = comps[1].Split(':');
                    hour = times[0];
                    min = times[1];
                }
                else
                {
                    hour = Lines[i].Substring(inputFormat.hour, 2);
                    min = Lines[i].Substring(inputFormat.min, 2);
                }
                
                //string hour = Lines[i].Substring(inputFormat.hour, 2);
                //string min = Lines[i].Substring(inputFormat.min, 2);
                //Дата-время
                string myDateTimeValue = day + "." + month + "." + year + " " + hour + ":" + min + ":00";
                //string myDateTimeValue = "06.02.1999 12:15:00";
                IFormatProvider culture = new CultureInfo("ru-RU", true);
                A[j].dateTime = DateTime.Parse(myDateTimeValue, culture, DateTimeStyles.NoCurrentDateDefault);
                A[j].dateTimeStr = myDateTimeValue;
                string[] str_tmp = Lines[i].Split(delimiter);
                float val1 = (str_tmp[indexFirst] != "") ? (float)Convert.ToDouble(str_tmp[indexFirst]) : 0;
                float val2 = (str_tmp[indexFirst + 1] != "") ? (float)Convert.ToDouble(str_tmp[indexFirst + 1]) : 0;
                float val3 = (str_tmp[indexFirst + 2] != "") ? (float)Convert.ToDouble(str_tmp[indexFirst + 2]) : 0;
                float val4 = (str_tmp[indexFirst + 3] != "") ? (float)Convert.ToDouble(str_tmp[indexFirst + 3]) : 0;
                A[j].val = (val1 + val2 + val3 + val4) / 4;
                A[j].qval = (float)Convert.ToDouble(str_tmp[indexFirst + 4]);
                j++;
            }
            fInput.Close();
            GC.Collect(GC.GetGeneration(fInput));
            GC.Collect(GC.GetGeneration(srInput));
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return A;
        }

        /// <summary>
        /// Загрузка данных за период вместе с датой и временем
        /// DATE;TIME;OPEN;HIGH;LOW;CLOSE;VOLUME
        /// 01.06.1999;11:00;1.43000;1.43200;1.40300;1.43000        
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="index">Загружаемый столбец значений (Open-0, Close-3)</param>
        /// <param name="timePeriod">02.12.1994.10.40-26.11.2006.14.35</param>        
        /// <param name="delimiter">Разделитель столбцов</param>
        /// <param name="format">Формат файла (finam/classic)</param>
        /// <param name="periodStr">Реально охватываемый период (переопределяемое значение)</param>
        /// <returns></returns>
        public static string[] LoadDataWithTimePeriod(string fileName, int index, string timePeriod, char delimiter, string format, ref string periodStr)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            string[] dateComponents = timePeriod.Split('-');
            string date = "";
            InputDateFormat inputFormat = _setInputFormat(format);
            int start = _getIndexByDate(dateComponents[0], iStart, Lines, inputFormat, ref date);
            periodStr = date;
            int end = _getIndexByDate(dateComponents[1], start, Lines, inputFormat, ref date);
            periodStr += " - " + date;
            string[] A = new string[end - start + 1];
            int j = 0;
            for (int i = start; i <= end; i++)
            {
                string[] str_tmp = Lines[i].Split(delimiter);
                A[j] = str_tmp[0] + " " + str_tmp[1] + ";" + str_tmp[index];
                j++;
            }
            fInput.Close();
            GC.Collect(GC.GetGeneration(fInput));
            GC.Collect(GC.GetGeneration(srInput));
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return A;
        }


        /// <summary>
        /// Бинарная сериализация массива
        /// </summary>
        /// <param name="A">Массив</param>
        /// <param name="fileName">Имя файла</param>
        public static void SaveObject(float[] A, string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(f, A);
            f.Close();
        }

        public static void SaveObject(double[] A, string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(f, A);
            f.Close();
        }

        public static void SaveObject(object A, string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(f, A);
            f.Close();
        }
        /*
        public static float[] LoadObject(string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            float[] A = (float[])bf.Deserialize(f);
            f.Close();
            return A;
        }
        */
        public static object LoadObject(string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            object A = bf.Deserialize(f);
            f.Close();
            return A;
        }

        public static void ExportArray(float[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(int[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(string[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i]);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(float[,] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    tmp += A[i, j].ToString() + ";";
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(float[,] A, string fileName, string[] header)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < header.Length; i++)
            {
                tmp += header[i] + ";";
            }
            writer.WriteLine(tmp);
            for (int i = 0; i < A.GetLength(0); i++)
            {
                tmp = "";
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    tmp += A[i, j].ToString() + ";";
                }
                writer.WriteLine(tmp);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(string[,] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    tmp += A[i, j] + ";";
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(double[,] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    tmp += A[i, j].ToString() + ";";
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(float[, ,] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < A.GetLength(1); i++)
            {
                for (int j = 0; j < A.GetLength(0); j++)
                {

                    for (int k = 0; k < A.GetLength(2); k++)
                    {
                        tmp += A[i, j, k].ToString() + ";";
                    }
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray3(float[, ,] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {

                    for (int k = 0; k < A.GetLength(2); k++)
                    {
                        tmp += A[i, j, k].ToString() + ";";
                    }
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(float[][] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            int l1 = A[0].Length;
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < l1; j++)
                {
                    tmp += A[i][j].ToString() + ";";
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(double[][] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string tmp = "";
            int l1 = A[0].Length;
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < l1; j++)
                {
                    tmp += A[i][j].ToString() + ";";
                }
                writer.WriteLine(tmp);
                tmp = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(double[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(double[,] A, int colCount, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    writer.Write(A[i, j].ToString() + ";");
                }
                writer.Write("\n");
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(float[,] A, int colCount, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    writer.Write(A[i, j].ToString() + ";");
                }
                writer.Write("\n");
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(PointRS[] rs, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("N;RS;H;H not Log");
            for (int i = 0; i < rs.Length; i++)
            {
                writer.WriteLine(rs[i].n.ToString() + ";" + rs[i].rs.ToString() + ";" + rs[i].h.ToString() + ";" + rs[i].h0.ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(PointTimeThresholdChangeFloat[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("Time;Date;Val;VOL;");
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].time.ToString() + ";" + A[i].dateTimeStr + ";" + A[i].val.ToString() + ";" + A[i].qval.ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(PointTimeThresholdChangeSimple[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("Ind;Time;Val;");
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].ind.ToString() + ";" + A[i].time.ToString() + ";" + A[i].val.ToString());
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(PointLocalExtremum[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("Time;Ind;Date;Val;QVaL;ClearTime;TimeClearThreshold;TimeThreshold;");
            string str = "";
            for (int i = 0; i < A.Length; i++)
            {
                str += A[i].time.ToString() + ";";
                str += A[i].indexOrg.ToString() + ";";
                str += A[i].dateTimeStr + ";";
                str += A[i].val.ToString() + ";";
                str += A[i].qval.ToString() + ";";
                str += A[i].timeClear.ToString() + ";";
                str += A[i].timeThreholdClear.ToString() + ";";
                str += A[i].timeThrehold + ";";
                writer.WriteLine(str);
                str = "";
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(Point[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            //writer.WriteLine("Date;Val;QVaL;");
            for (int i = 0; i < A.Length; i++)
            {
                writer.WriteLine(A[i].dateTimeStr + ";" + A[i].val.ToString() + ";" + A[i].qval.ToString() + ";");
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(LocalExtremumStatistic[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string header = "Probability;D;U;Delta;Amount;";
            
            header += "averageTimeDUpRecoil;";
            header += "averageTimeDUpWithoutRecoil;";
            header += "amountUpRecoil;";
            header += "amountUpWithoutRecoil;";
            header += "averageTimeDDownRecoil;";            
            header += "averageTimeDDownWithoutRecoil;";
            header += "amountDownRecoil;";
            header += "amountDownWithoutRecoil;";
            /*
            header += "amountUp;";
            header += "amountDUp;";
            header += "amountReturnUp;";
            header += "amountReturnDUp;";
            header += "averageTimeUUp;";
            header += "maxReturnUp;";
            header += "averageReturnUp;";
            header += "amountGetUAfterReturnUp;";

            header += "amountDown;";
            header += "amountDDown;";
            header += "maxReturnDown;";
            header += "averageReturnDown;";
            header += "averageTimeUDown;";
            header += "amountGetUAfterReturnDown;";
            */
            writer.WriteLine(header);
            string line = "";
            for (int i = 0; i < A.Length; i++)
            {                
                line = A[i].probability.ToString() + ";";
                line += A[i].startThreshold.ToString() + ";";
                line += A[i].endThreshold.ToString() + ";";
                line += A[i].lengthChanging.ToString() + ";";
                line += A[i].amount.ToString() + ";";
                
                line += A[i].averageTimeDUpRecoil.ToString() + ";";
                line += A[i].averageTimeDUpWithoutRecoil.ToString() + ";";
                line += A[i].amountUpRecoil.ToString() + ";";
                line += A[i].amountUpWithoutRecoil.ToString() + ";";
                line += A[i].averageTimeDDownRecoil.ToString() + ";";                
                line += A[i].averageTimeDDownWithoutRecoil.ToString() + ";";
                line += A[i].amountDownRecoil.ToString() + ";";
                line += A[i].amountDownWithoutRecoil.ToString() + ";";
                /*
                line += A[i].amountUp.ToString() + ";";
                line += A[i].amountDUp.ToString() + ";";
                line += A[i].amountReturnUp.ToString() + ";";
                line += A[i].amountReturnDUp.ToString() + ";";
                line += A[i].averageTimeUUp.ToString() + ";";
                line += A[i].maxReturnUp.ToString() + ";";
                line += A[i].averageReturnUp.ToString() + ";";
                line += A[i].amountGetUAfterReturnUp.ToString() + ";";

                line += A[i].amountDown.ToString() + ";";
                line += A[i].amountDDown.ToString() + ";";
                line += A[i].maxReturnDown.ToString() + ";";
                line += A[i].averageReturnDown.ToString() + ";";
                line += A[i].averageTimeUDown.ToString() + ";";
                line += A[i].amountGetUAfterReturnDown.ToString() + ";";
                */
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(DisruptionPoint[] A, Point[] B, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string header = "Type;Date;Price;";
            writer.WriteLine(header);
            string line = "";
            for (int i = 0; i < A.Length; i++)
            {
                line = A[i].type.ToString() + ";";
                line += B[A[i].indConfirmativeThreshold].dateTime.ToString() + ";";
                line += B[A[i].indConfirmativeThreshold].val + ";";
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(DisruptionPoint[] A, float[] B, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string header = "Type;№;Value;";
            writer.WriteLine(header);
            string line = "";
            for (int i = 0; i < A.Length; i++)
            {
                line = A[i].type.ToString() + ";";
                line += A[i].indConfirmativeThreshold.ToString() + ";";
                line += B[A[i].indConfirmativeThreshold] + ";";
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }


        public static void ExportArray(PointClassification[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string header = "ClassNum;Date;";
            for (int i = 0; i < A[0].coord.Length; i++)
            {
                header += "Coord" + i.ToString() + ";";
            }
            writer.WriteLine(header);
            string line = "";
            for (int i = 0; i < A.Length; i++)
            {
                line = A[i].classNum.ToString() + ";";
                line += A[i].time.ToString() + ";";
                for (int j = 0; j < A[i].coord.Length; j++)
                {
                    line += A[i].coord[j].ToString() + ";";
                }
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArray(PointClassificationSimple[] A, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            string header = "ClassNum;";
            for (int i = 0; i < A[0].coord.Length; i++)
            {
                header += "Coord" + i.ToString() + ";";
            }
            writer.WriteLine(header);
            string line = "";
            for (int i = 0; i < A.Length; i++)
            {
                line = A[i].classNum.ToString() + ";";                
                for (int j = 0; j < A[i].coord.Length; j++)
                {
                    line += A[i].coord[j].ToString() + ";";
                }
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }


        public static void ExportList(List<ExecutedOrder> L, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("N;dateSet;dateExecuted;price;volume;type;kind;profit;sumCredit");
            string line;
            for (int i = 0; i < L.Count; i++)
            {
                line = L[i].number.ToString() + ";";
                line += L[i].dateSet.ToString() + ";";
                line += L[i].dateExecuted.ToString() + ";";
                line += L[i].price.ToString() + ";";
                line += L[i].volume.ToString() + ";";
                line += L[i].type.ToString() + ";";
                line += L[i].kind.ToString() + ";";
                line += L[i].profit.ToString() + ";";
                line += L[i].sumCredit.ToString() + ";";
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportList(List<PointLocalExtremum> L, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("Val;");
            string line;
            for (int i = 0; i < L.Count; i++)
            {
                line = L[i].val.ToString() + ";";
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        public static void ExportArrayList(ArrayList L, string fileName)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.WriteLine("Val;");
            string line;
            for (int i = 0; i < L.Count; i++)
            {
                line = L[i].ToString() + ";";
                writer.WriteLine(line);
            }
            writer.Close();
            fOutput.Close();
        }

        /// <summary>
        /// Нормализация сигнала по среднему
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <returns>Нормированный сигнал</returns>
        public static float[] NormalizationByAverage(float[] A)
        {
            float average = 0;
            float sum = 0;
            for (int i = 0; i < A.Length; i++)
            {
                sum += A[i];
            }
            average = sum / A.Length;
            sum = 0;
            float sq;
            for (int i = 0; i < A.Length; i++)
            {
                sq = A[i] - average;
                sum += sq * sq;
            }
            float average_sq = (float)Math.Sqrt(sum / A.Length);
            for (int i = 0; i < A.Length; i++)
            {
                A[i] = (A[i] - average) / average_sq;
            }
            return A;
        }

        /// <summary>
        /// Нормализация одноканального сигнала в заданном диапазоне (по амплитуде)
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <param name="lowerBound">Минимально допустимое значние</param>
        /// <param name="upperBound">Максимально допустимое значние</param>
        /// <returns>Нормализованный сигнал со значениями амплитуды и смещения</returns>
        public static NormalizationByBoundResultOneChannel NormalizationByBound(float[] A, float lowerBound, float upperBound)
        {
            int length = A.Length;
            float deltaBound = upperBound - lowerBound;
            float max;
            float min;
            float amplituda;
            float offset;
            max = -100000000000;
            min = 1000000000000;
            for (int i = 0; i < length; i++)
            {
                if (A[i] > max) max = A[i];
                if (A[i] < min) min = A[i];
            }
            amplituda = deltaBound / (max - min);
            offset = upperBound - amplituda * max;
            for (int i = 0; i < length; i++)
            {
                A[i] = amplituda * A[i] + offset;
            }
            NormalizationByBoundResultOneChannel res = new NormalizationByBoundResultOneChannel();
            res.A = A;
            res.amplituda = amplituda;
            res.offset = offset;
            return res;
        }


        public static NormalizationByBoundResultOneChannel NormalizationByBound(Point[] A, int startInd, int endInd, float lowerBound, float upperBound)
        {            
            int length = endInd-startInd+1;
            float[] B = new float[length];
            float deltaBound = upperBound - lowerBound;
            float max;
            float min;
            float amplituda;
            float offset;
            max = -100000000000;
            min = 1000000000000;
            for (int i = startInd; i <= endInd; i++)
            {
                if (A[i].qval > max) max = A[i].qval;
                if (A[i].qval < min) min = A[i].qval;
            }
            amplituda = deltaBound / (max - min);
            offset = upperBound - amplituda * max;
            for (int i = startInd; i <= endInd; i++)
            {
                B[i-startInd] = amplituda * A[i].qval + offset;
            }
            NormalizationByBoundResultOneChannel res = new NormalizationByBoundResultOneChannel();
            res.A = B;
            res.amplituda = amplituda;
            res.offset = offset;
            return res;
        }

        /// <summary>
        /// Нормализация многоканального сигнала в заданном диапазоне (по амплитуде)
        /// </summary>
        /// <param name="A">Сигнал</param>
        /// <param name="lowerBound">Минимально допустимое значние</param>
        /// <param name="upperBound">Максимально допустимое значние</param>
        /// <returns>Нормализованный сигнал со значениями амплитуды и смещения для каждого канала</returns>
        public static NormalizationByBoundResult NormalizationByBound(float[,] A, float lowerBound, float upperBound)
        {
            int length = A.GetLength(0);
            int numChanals = A.GetLength(1);
            float deltaBound = upperBound - lowerBound;
            float[] max = new float[numChanals];
            float[] min = new float[numChanals];
            float[] amplituda = new float[numChanals];
            float[] offset = new float[numChanals];

            for (int c = 0; c < numChanals; c++)
            {
                max[c] = -100000000000;
                min[c] = 1000000000000;
                for (int i = 0; i < length; i++)
                {
                    if (A[i, c] > max[c]) max[c] = A[i, c];
                    if (A[i, c] < min[c]) min[c] = A[i, c];
                }
                amplituda[c] = deltaBound / (max[c] - min[c]);
                offset[c] = upperBound - amplituda[c] * max[c];
            }
            for (int c = 0; c < numChanals; c++)
            {
                for (int i = 0; i < length; i++)
                {
                    A[i, c] = amplituda[c] * A[i, c] + offset[c];
                }
            }
            NormalizationByBoundResult res = new NormalizationByBoundResult();
            res.A = A;
            res.amplituda = amplituda;
            res.offset = offset;
            return res;
        }

        public static double[,] shuffleArray(double[,] A)
        {
            Thread.Sleep(5);
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            int length = A.GetLength(0);
            int numChanals = A.GetLength(1);
            double[] tmp = new double[numChanals];
            int i = 0;
            while (i < length / 20)
            {
                int ind1 = rand.Next(0, length);
                int ind2 = rand.Next(0, length);
                for (int j = 0; j < numChanals; j++)
                {
                    tmp[j] = A[ind2, j];
                    A[ind2, j] = A[ind1, j];
                    A[ind1, j] = tmp[j];
                }
                i++;
            }
            return A;
        }

        public static void shuffleArrays(ref double[,] A, ref double[,] B, int trainLength)
        {
            Thread.Sleep(5);
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            int numChanals = A.GetLength(1);
            double[] tmp1 = new double[numChanals];
            double[] tmp2 = new double[numChanals];
            int i = 0;
            int numRepeat = 100;
            while (i < numRepeat)
            {
                int ind1 = rand.Next(0, trainLength);
                int ind2 = rand.Next(0, trainLength);
                for (int j = 0; j < numChanals; j++)
                {
                    tmp1[j] = A[ind2, j];
                    A[ind2, j] = A[ind1, j];
                    A[ind1, j] = tmp1[j];

                    tmp2[j] = B[ind2, j];
                    B[ind2, j] = B[ind1, j];
                    B[ind1, j] = tmp2[j];
                }
                i++;
            }
        }



        /// <summary>
        /// Преобразует ряд в логарифмическую разность LOG(x)/LOG(x+1)
        /// </summary>
        /// <param name="A">Массив входных данных длины n</param>
        /// <returns>Массив длины n-1Начальный индекс в массиве, с которого беруться данные</returns>
        public static double[] GetLnDifference(double[] A)
        {
            double[] B = new double[A.Length - 1];
            for (int i = 1; i < A.Length; i++)
            {
                B[i - 1] = Math.Log(A[i] / A[i - 1], Math.Exp(1));
            }
            return B;
        }

        public static float[] GetLnDifference(float[] A)
        {
            float[] B = new float[A.Length - 1];
            for (int i = 1; i < A.Length; i++)
            {
                B[i - 1] = (float)(Math.Log(A[i] / A[i - 1], Math.Exp(1)));
            }
            return B;
        }

        public static float[] GetDifference(float[] A)
        {
            float[] B = new float[A.Length - 1];
            for (int i = 1; i < A.Length; i++)
            {
                B[i - 1] = A[i] - A[i - 1];
            }
            return B;
        }


        /// <summary>
        /// Вычесляет время достижения порога изменения величины
        /// </summary>
        /// <param name="threshold">Порог изменения (%)</param>
        /// <param name="A">Массив данных</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив с новым рядом времени достижения.
        /// Положительное значение - увеличение на порог изменения, отрицательное - уменьшение. 
        /// </returns>
        public static PointTimeThresholdChange[] GetTimeThresholdChange(float threshold, Point[] A, int startIndex, int endIndex)
        {
            startIndex = 0;
            endIndex = A.Length - 1;
            int lastIndex = 0;
            int j = 0;
            int len = endIndex - startIndex + 1;
            PointTimeThresholdChange b = new PointTimeThresholdChange();
            ArrayList list = new ArrayList(1);
            int i = 0;
            float qval_sum = 0;
            for (i = startIndex; i <= endIndex; i++)
            {
                float delta = MathProcess.DeltaProcent(A[lastIndex].val, A[i].val);
                qval_sum += A[i].qval;
                if (Math.Abs(delta) > threshold)
                {
                    TimeSpan diff = A[i].dateTime.Subtract(A[lastIndex].dateTime);
                    b.time = (float)(Math.Sign(delta) * diff.TotalSeconds / 60);//minutes
                    b.dateTimeStr = A[i].dateTimeStr;
                    b.qval = qval_sum;
                    b.original = A[i].val;
                    //richTextBox1.AppendText(delta.ToString() + " - " + b.time.ToString() + " " + A[i].dateTimeStr+ "\n");
                    list.Add(b);
                    j++;
                    lastIndex = i;
                    qval_sum = 0;
                }
            }
            PointTimeThresholdChange[] d = (PointTimeThresholdChange[])list.ToArray(typeof(PointTimeThresholdChange));
            return d;
        }
        /*
        public static PointTimeThresholdChangeFloat[] GetTimeThresholdChange(float threshold, Point[] A, int startIndex, int endIndex)
        {
            //startIndex = 0;
            endIndex = A.Length - 1;
            int lastIndex = 0;
            int j = 0;
            int len = endIndex - startIndex + 1;
            PointTimeThresholdChangeFloat b = new PointTimeThresholdChangeFloat();
            ArrayList list = new ArrayList(1);
            int i = 0;
            int z = 0;
            float qval_sum = 0;
            CalculateStart.CreateEvent(len + 1, "Threshold " + threshold.ToString());
            for (i = startIndex; i <= endIndex; i++)
            {
                float delta = MathProcess.DeltaProcent(A[lastIndex].val, A[i].val);
                qval_sum += A[i].qval;
                if (Math.Abs(delta) > threshold)
                {
                    TimeSpan diff = A[i].dateTime.Subtract(A[lastIndex].dateTime);
                    b.time = (float)(Math.Sign(delta) * diff.TotalSeconds / 60);//minutes                    
                    b.dateTimeStr = A[i].dateTimeStr;
                    b.qval = qval_sum;
                    b.original = A[i].val;
                    list.Add(b);
                    j++;
                    lastIndex = i;
                }
                z++;
                Calculus.CreateEvent(z, 0);
            }
            PointTimeThresholdChangeFloat[] d = (PointTimeThresholdChangeFloat[])list.ToArray(typeof(PointTimeThresholdChangeFloat));
            return d;
        }
        */
        /// <summary>
        /// Вычесляет время достижения порога изменения величины не считая перерывов между торгами
        /// </summary>
        /// <param name="threshold">Порог изменения (%)</param>
        /// <param name="A">Массив данных</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив с новым рядом времени достижения.
        /// Положительное значение - увеличение на порог изменения, отрицательное - уменьшение. 
        /// </returns>
        public static PointTimeThresholdChangeFloat[] GetTimeThresholdChangeClear(float threshold, Point[] A, int startIndex, int endIndex)
        {
            startIndex = 0;
            if (endIndex == 0)
            {
                endIndex = A.Length - 1;
            }
            int len = endIndex - startIndex + 1;
            PointTimeThresholdChangeFloat b = new PointTimeThresholdChangeFloat();
            ArrayList list = new ArrayList(1);
            int i = 0;
            int z = 0;        
            CalculateStart.CreateEvent(len + 1, "Threshold " + threshold.ToString());            
            z = 0;
            int lastIndex = startIndex;
            float qvalSum = A[startIndex].qval;
            for (i = startIndex + 1; i <= endIndex; i++)
            {
                float delta = MathProcess.DeltaProcent(A[lastIndex].val, A[i].val);
                qvalSum += A[i].qval;                
                if (Math.Abs(delta) > threshold)
                {
                    b.time = Math.Sign(delta) * (float)Math.Log10(_getClearTime(A[lastIndex].dateTime, A[i].dateTime));
                    b.dateTimeStr = A[i].dateTimeStr;
                    b.qval = (float)Math.Log10(qvalSum);
                    b.val = A[i].val;
                    list.Add(b);
                    lastIndex = i;
                    qvalSum = 0;                    
                }
                z++;
                Calculus.CreateEvent(z, 0);
            }
            PointTimeThresholdChangeFloat[] d = (PointTimeThresholdChangeFloat[])list.ToArray(typeof(PointTimeThresholdChangeFloat));
            return d;
        }

        /// <summary>
        /// Вычисляет информативность точки в массиве времени достижения порога изменения
        /// </summary>
        /// <param name="A">Массив времени достижения порога изменения</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив где для каждой точки указана ее информативность</returns>
        public static float[] GetPointsInformation(PointTimeThresholdChangeSimple[] A, int startIndex, int endIndex)
        {
            if (endIndex == 0)
            {
                endIndex = A.Length - 1;
            }
            int len = endIndex - startIndex + 1;
            float[] B = new float[len];
            B[0] = 0;
            float dl = 0;
            float dr = 0;
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                //A[i-1].val
                float[] pl = new float[2];
                pl[0] = A[i-1].val;
                pl[1] = A[i-1].time;

                float[] p = new float[2];
                p[0] = A[i].val;
                p[1] = A[i].time;

                float[] pr = new float[2];
                pr[0] = A[i + 1].val;
                pr[1] = A[i + 1].time;
                dl = MathProcess.getPointSpacingManhaten(p, pl);
                dr = MathProcess.getPointSpacingManhaten(p, pr);
                B[i] = (dl + dr) / len;
            }
            B[len - 1] = dr / len;
            return B;
        }

        /// <summary>
        /// Вычесляет время достижения порога изменения (число отсчетов) величины для простого массива
        /// </summary>
        /// <param name="threshold">Порог изменения (%)</param>
        /// <param name="A">Массив данных</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив с новым рядом времени достижения.
        /// Положительное значение - увеличение на порог изменения, отрицательное - уменьшение. 
        /// </returns>
        public static PointTimeThresholdChangeSimple[] GetTimeThresholdChangeSimple(float threshold, float[] A, int startIndex, int endIndex)
        {
            startIndex = 0;
            if (endIndex == 0)
            {
                endIndex = A.Length - 1;
            }
            int len = endIndex - startIndex + 1;
            PointTimeThresholdChangeSimple b = new PointTimeThresholdChangeSimple();
            ArrayList list = new ArrayList(1);
            int i = 0;
            int z = 0;
            CalculateStart.CreateEvent(len + 1, "Threshold " + threshold.ToString());
            z = 0;
            int lastIndex = startIndex;            
            for (i = startIndex + 1; i <= endIndex; i++)
            {
                float delta = MathProcess.DeltaProcent(A[lastIndex], A[i]);                
                if (Math.Abs(delta) > threshold)
                {
                    b.time = Math.Sign(delta) * (i - lastIndex);
                    b.val = A[i];
                    b.ind = i;
                    list.Add(b);
                    lastIndex = i;
                }
                z++;
                Calculus.CreateEvent(z, 0);
            }
            PointTimeThresholdChangeSimple[] d = (PointTimeThresholdChangeSimple[])list.ToArray(typeof(PointTimeThresholdChangeSimple));
            return d;
        }

        /// <summary>
        /// Загружает точки локальных экстремумов, где разица между сосоедними точками не менее threshold %
        /// </summary>
        /// <param name="threshold">Порог изменения (%)</param>
        /// <param name="A">Массив данных</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив с новым рядом времени достижения.
        public static PointLocalExtremum[] GetLocalExtremums(float threshold, Point[] A, int startIndex, int endIndex)
        {
            //PointLocalExtremum[] res;
            ArrayList list = new ArrayList(0);

            int lastIndex = startIndex;
            int lastIndex2 = startIndex;
            int lastIndexExtremum = startIndex;
            float qvalSum2 = 0;
            PointLocalExtremum b = new PointLocalExtremum();
            PointLocalExtremum b2 = new PointLocalExtremum();
            int j;
            b.time = 0;
            b.dateTimeStr = A[startIndex].dateTimeStr;
            b.val = A[startIndex].val;
            b.qval = A[startIndex].qval;
            b.count = 1;
            b.indexOrg = startIndex;
            list.Add(b);
            float delta;
            int i = lastIndexExtremum;
            while (i < endIndex)
            {
                delta = MathProcess.DeltaProcent(A[lastIndexExtremum].val, A[i].val);
                qvalSum2 += A[i].qval;
                //Если порог достигнут
                if (Math.Abs(delta) > threshold)
                {
                    lastIndex2 = i;
                    //Время достижения порога
                    TimeSpan diff = A[lastIndex2].dateTime.Subtract(A[lastIndexExtremum].dateTime);
                    delta = MathProcess.DeltaProcent(A[lastIndexExtremum].val, A[lastIndex2].val);
                    b2.timeThrehold = (float)(Math.Sign(delta) * diff.TotalSeconds / 60);//minutes
                    int minus_min = 0;
                    if (diff.TotalMinutes <= 495) minus_min = 0;
                    else if (diff.TotalMinutes > 495 && diff.Days == 0) minus_min = 945;
                    else if (diff.Days > 0)
                    {
                        DateTime curDate = A[lastIndexExtremum].dateTime.AddDays(1);
                        int weekEndCount = 0;
                        while (curDate < A[lastIndex2].dateTime)
                        {
                            if (curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday) weekEndCount++;
                            curDate = curDate.AddDays(1);
                        }
                        minus_min = (diff.Days - weekEndCount) * 945;
                    }
                    b2.timeThreholdClear = (float)diff.TotalMinutes - minus_min;
                    j = lastIndex2 + 1;
                    int signPoint = Math.Sign(delta);
                    //ищем точку которая отлична на порог от найденной в противоположную сторону                   
                    while (j < endIndex)
                    {
                        delta = MathProcess.DeltaProcent(A[lastIndex2].val, A[j].val);
                        int signDelta = Math.Sign(delta);
                        qvalSum2 += A[j].qval;
                        //Если нашли следующую точку отличающуюся на порог или больше
                        if (signDelta != signPoint && Math.Abs(delta) > threshold)
                        {
                            //lastIndex2 - локальный экстремум

                            //Ищем максимальный и средний откат между текущим и последним локальн. экстре-ми
                            //как локальный экстремум противоположный движению
                            float localExtr;
                            float localExtrAverage = 0;
                            int amountLocalExtr = 0;
                            //если цена выросла
                            if (signDelta > 0)
                            {
                                localExtr = INFINITY;
                                //ищем локальные минимумы
                                for (int r = lastIndexExtremum + 1; r < lastIndex2; r++)
                                {
                                    if (A[r].val < A[r - 1].val && A[r].val < A[r + 1].val && A[r].val < localExtr)
                                    {
                                        localExtr = A[r].val;
                                        localExtrAverage += localExtr;
                                        amountLocalExtr++;
                                    }
                                }
                            }
                            //если цена упала
                            else
                            {
                                localExtr = 0;
                                //ищем локальные максимумы
                                for (int r = lastIndexExtremum + 1; r < lastIndex2; r++)
                                {
                                    if (A[r].val > A[r - 1].val && A[r].val > A[r + 1].val && A[r].val > localExtr)
                                    {
                                        localExtr = A[r].val;
                                        localExtrAverage += localExtr;
                                        amountLocalExtr++;
                                    }
                                }
                            }

                            b2.maxReturn = MathProcess.DeltaProcent(A[lastIndexExtremum].val, localExtr);
                            b2.averageReturn = MathProcess.DeltaProcent(A[lastIndexExtremum].val, (localExtrAverage / amountLocalExtr));
                            diff = A[lastIndex2].dateTime.Subtract(A[lastIndexExtremum].dateTime);
                            delta = MathProcess.DeltaProcent(A[lastIndexExtremum].val, A[lastIndex2].val);
                            b2.time = (float)(Math.Sign(delta) * diff.TotalSeconds / 60);//minutes
                            if (diff.TotalMinutes <= 495) minus_min = 0;
                            else if (diff.TotalMinutes > 495 && diff.Days == 0) minus_min = 945;
                            else if (diff.Days > 0)
                            {
                                DateTime curDate = A[lastIndexExtremum].dateTime.AddDays(1);
                                int weekEndCount = 0;
                                while (curDate < A[lastIndex2].dateTime)
                                {
                                    if (curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday) weekEndCount++;
                                    curDate = curDate.AddDays(1);
                                }
                                minus_min = (diff.Days - weekEndCount) * 945;
                            }
                            b2.timeClear = (float)diff.TotalMinutes - minus_min;
                            b2.dateTimeStr = A[lastIndex2].dateTimeStr;
                            b2.val = A[lastIndex2].val;
                            b2.qval = qvalSum2;
                            b2.count = lastIndex2 - lastIndexExtremum;
                            b2.indexOrg = lastIndex2;
                            list.Add(b2);
                            lastIndexExtremum = lastIndex2;
                            qvalSum2 = 0;
                            i = lastIndexExtremum;
                            break;
                        }
                        else
                        {
                            if (signPoint > 0 && A[j].val > A[lastIndex2].val || signPoint < 0 && A[j].val < A[lastIndex2].val)
                            {
                                lastIndex2 = j;
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
            PointLocalExtremum[] res = (PointLocalExtremum[])list.ToArray(typeof(PointLocalExtremum));
            return res;
        }

        /// <summary>
        /// Получаем локальные экстремумы, где разица между сосоедними точками не менее threshold %
        /// </summary>
        /// <param name="threshold">Порог изменения (%)</param>
        /// <param name="A">Массив данных</param>
        /// <param name="startIndex">Нач. индекс массива</param>
        /// <param name="endIndex">Конечный индекс массива</param>
        /// <returns>Массив с новым рядом времени достижения.
        public static PointLocalExtremum[] GetLocalExtremumsSimple(float threshold, Point[] A, int startIndex, int endIndex)
        {
            //PointLocalExtremum[] res;
            ArrayList list = new ArrayList(0);

            int lastIndex = startIndex;
            int lastIndex2 = startIndex;
            int lastIndexExtremum = startIndex;
            float qvalSum2 = 0;
            PointLocalExtremum b = new PointLocalExtremum();
            PointLocalExtremum b2 = new PointLocalExtremum();
            int j;
            b.time = 0;
            b.dateTimeStr = A[startIndex].dateTimeStr;
            b.val = A[startIndex].val;
            b.qval = A[startIndex].qval;
            b.count = 1;
            b.indexOrg = startIndex;
            list.Add(b);
            float delta;
            int i = lastIndexExtremum;
            while (i < endIndex)
            {
                delta = MathProcess.DeltaProcent(A[lastIndexExtremum].val, A[i].val);
                qvalSum2 += A[i].qval;
                //Если порог достигнут
                if (Math.Abs(delta) > threshold)
                {
                    lastIndex2 = i;                    
                    j = lastIndex2 + 1;
                    int signPoint = Math.Sign(delta);
                    //ищем точку которая отлична на порог от найденной в противоположную сторону                   
                    while (j < endIndex)
                    {
                        delta = MathProcess.DeltaProcent(A[lastIndex2].val, A[j].val);
                        int signDelta = Math.Sign(delta);
                        qvalSum2 += A[j].qval;
                        //Если нашли следующую точку отличающуюся на порог или больше
                        if (signDelta != signPoint && Math.Abs(delta) > threshold)
                        {
                            //lastIndex2 - локальный экстремум
                            b2.val = A[lastIndex2].val;
                            b2.qval = qvalSum2;
                            b2.count = lastIndex2 - lastIndexExtremum;
                            b2.indexOrg = lastIndex2;
                            list.Add(b2);
                            lastIndexExtremum = lastIndex2;
                            qvalSum2 = 0;
                            i = lastIndexExtremum;
                            break;
                        }
                        else
                        {
                            if (signPoint > 0 && A[j].val > A[lastIndex2].val || signPoint < 0 && A[j].val < A[lastIndex2].val)
                            {
                                lastIndex2 = j;
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
            PointLocalExtremum[] res = (PointLocalExtremum[])list.ToArray(typeof(PointLocalExtremum));
            return res;
        }



        /// <summary>
        /// Возвращает статистику по вероятности достижения порога изменения сигнала после достижения 
        /// определенного порога изменения
        /// </summary>
        /// <param name="A"></param>
        /// <param name="startThreshold">Начальный порог</param>
        /// <param name="endThreshold">Конечный порог</param>
        /// <param name="deltaThreshold">Минимальное расстояние между d и u (в %)</param>
        /// <param name="deltaMaxThreshold">Максимальное расстояние между d и u</param>
        /// <param name="stepThreshold">Шаг изменения порога</param>
        /// <param name="probabilityLimit">Лимит вероятности, который должен быть набран для сохранения</param>
        /// <returns>Массив со статистикой для каждой пары d-u</returns>
        public static LocalExtremumStatistic[] GetLocalExtremumStatistic(Point[] A, int startInd, int endInd, float startThreshold, float endThreshold, float deltaThreshold, float deltaMaxThreshold, float stepThreshold, float probabilityLimit)
        {
            ArrayList list = new ArrayList(0);
            int stepCountD = (int)((float)(endThreshold - deltaThreshold - startThreshold) / stepThreshold);
            int lengthResult = (stepCountD) * (stepCountD - 1) / 2;
            LocalExtremumStatistic b = new LocalExtremumStatistic();
            for (int i = 0; i < stepCountD; i++)
            {
                //порог изменения (нижний), который преодолевается
                float d = startThreshold + stepThreshold * i;
                float winLength = endThreshold - d;
                winLength = (winLength > deltaMaxThreshold) ? deltaMaxThreshold : winLength;
                int stepLevelCount = (int)((float)winLength / stepThreshold);
                PointLocalExtremum[] R = GetLocalExtremums(d, A, startInd, endInd);
                /*
                FileStream fOutput = new FileStream("_compare.csv", FileMode.Create, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fOutput);
                writer.WriteLine("O;E;");
                int tt = 0;
                int p = 0;
                float vv;
                while (p < A.Length && tt < R.Length)
                {
                    if (R[tt].indexOrg == p)
                    {
                        vv = A[p].val;
                        tt++;
                    }
                    else
                    {
                        vv = 0;
                    }
                    writer.WriteLine(A[p].val.ToString() + ";" + vv.ToString() + ";");
                    p++;
                }
                writer.Close();
                fOutput.Close();
                return B;
                 */
                int lengthExtremums = R.Length;
                for (int j = 0; j < stepLevelCount; j++)
                {
                    //Верхний порог изменения, который достигается (или нет) после преодоления d 
                    float u = d + deltaThreshold + stepThreshold * j;

                    //d = 0.5f;
                    //u = 0.925f;


                    //кол-во случаев достижения u после преодоления d
                    int amount = 0;
                    //минимальное время достижения u после преодоления d
                    //float minTime = 100000000;
                    //максимальное время 
                    //float maxTime = 0;
                    //подсчет всего времени для расчета среднего
                    //float fullTime = 0;

                    //Движение вверх (от левого экстремума к правому)
                    //кол-во движений вверх
                    int amountUp = 0;
                    //наибольший откат при движении вверх (% от левого экстремума)
                    float maxReturnUp = INFINITY;
                    //средний откат при движении вверх
                    float averageReturnUp = 0;
                    //кол-во откатов при движении вверх
                    int amountReturnUp = 0;
                    //минимальное время для достижения порога d при движении вверх
                    float minTimeDUp = INFINITY;
                    //максимальное время для достижения порога d при движении вверх
                    float maxTimeDUp = 0;
                    //среднее время для достижения порога d при движении вверх
                    float averageTimeDUp = 0;
                    //минимальное время для достижения порога u при движении вверх (после преодоления d)
                    float minTimeUUp = INFINITY;
                    //максимальное время для достижения порога u при движении вверх
                    float maxTimeUUp = 0;
                    //среднее время для достижения порога u при движении вверх
                    float averageTimeUUp = 0;
                    //Кол-во случаев достижения от d до u при которых цена вначале откатывается ниже d
                    //при движении вверх
                    int amountGetUAfterReturnUp = 0;
                    //кол-во откатов ниже d
                    int amountReturnDUp = 0;
                    //кол-во НЕ достижений u при движении вверх
                    int amountDUp = 0;

                    //Движение вниз (от левого экстремума к правому)
                    //кол-во движений вниз при которых достигается u
                    int amountDown = 0;
                    //кол-во движений вниз при которых НЕ достигается u
                    int amountDDown = 0;
                    //наибольший откат при движении вниз (% от левого экстремума)
                    float maxReturnDown = INFINITY;
                    //средний откат при движении вниз
                    float averageReturnDown = 0;
                    //кол-во откатов при движении вниз
                    int amountReturnDown = 0;
                    //минимальное время для достижения порога d при движении вниз
                    float minTimeDDown = INFINITY;
                    //максимальное время для достижения порога d при движении вниз
                    float maxTimeDDown = 0;
                    //среднее время для достижения порога d при движении вниз
                    float averageTimeDDown = 0;
                    //минимальное время для достижения порога u при движении вниз (после преодоления d)
                    float minTimeUDown = INFINITY;
                    //максимальное время для достижения порога u при движении вниз
                    float maxTimeUDown = 0;
                    //среднее время для достижения порога u при движении вниз
                    float averageTimeUDown = 0;
                    //Кол-во случаев достижения от d до u при которых цена вначале откатывается выше d
                    //при движении вниз
                    int amountGetUAfterReturnDown = 0;
                    //кол-во достижений u при движении вниз
                    int amountUDown = 0;

                    //Рассматриваем все экстремумы попарно
                    for (int k = 1; k < lengthExtremums; k++)
                    {
                        float delta = MathProcess.DeltaProcent(R[k - 1].val, R[k].val);
                        int signDelta = Math.Sign(delta);
                        //Если достигается нужный порог изменения
                        if (Math.Abs(delta) >= u)
                        {
                            //Анализируем внутренний участок между экстремумами k-1 и k                            

                            //с левого экстремума  идем до границы d                                
                            int r = R[k - 1].indexOrg + 1;
                            while (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[r].val)) <= d)
                            {
                                r++;
                            }
                            int indexD = r;
                            //Ищем максимальный и средний откат между d и u
                            //как локальный экстремум противоположный движению

                            //если цена выросла между экстремумами k-1 и k

                            //максимальный откат между d и u
                            float localReturnMax = INFINITY;
                            int localReturnMaxIndex = -1;
                            if (signDelta > 0)
                            {
                                //ищем локальные минимумы и самый минимальный сохраняем
                                //идем с границы d по u                                
                                while (r + 1 <= R[k].indexOrg)
                                {
                                    //если точка - локальный минимум
                                    if (A[r].val < A[r - 1].val && A[r].val < A[r + 1].val)
                                    {
                                        //дельта относительно левого экстремума
                                        float localDelta = MathProcess.DeltaProcent(R[k - 1].val, A[r].val);
                                        //если откат самый максимальный
                                        if (localDelta < localReturnMax)
                                        {
                                            localReturnMax = localDelta;
                                            //индекс максимального отката
                                            localReturnMaxIndex = r;
                                        }
                                    }
                                    if (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[r].val)) >= u) break;
                                    r++;
                                }
                                //проверяем, дойдет ли цена до u после отката, опустившегося ниже d
                                if (localReturnMax < d)
                                {
                                    int ind = localReturnMaxIndex;
                                    while (ind <= R[k].indexOrg)
                                    {
                                        ind++;
                                        //Цена дошла
                                        if (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[ind].val)) >= u)
                                        {
                                            amountGetUAfterReturnUp++;
                                            break;
                                        }
                                    }
                                    amountReturnDUp++;
                                }
                                //время для достижения d
                                //TimeSpan diffD = A[indexD].dateTime.Subtract(A[R[k - 1].indexOrg].dateTime);
                                //время для достижения u от d
                                //TimeSpan diffU = A[r].dateTime.Subtract(A[indexD].dateTime);
                                float dTime = indexD - R[k - 1].indexOrg + 3.5f;
                                float uTime = r - indexD + 3.5f;
                                if (dTime > maxTimeDUp) maxTimeDUp = dTime;
                                if (uTime > maxTimeUUp) maxTimeUUp = uTime;
                                averageTimeDUp += dTime;
                                averageTimeUUp += uTime;
                                //накапливаем суммарно откаты при движении вверх
                                if (localReturnMax < INFINITY && localReturnMax <= u)
                                {
                                    averageReturnUp += localReturnMax;
                                    if (localReturnMax < maxReturnUp) maxReturnUp = localReturnMax;
                                    amountReturnUp++;
                                }
                                amountUp++;
                            }
                            //если цена упала
                            else
                            {
                                //ищем локальные максимумы и самый максимальный сохраняем
                                //с левого экстремума (d) идем до границы u                                
                                while (r + 1 <= R[k].indexOrg)
                                {
                                    //если точка - локальный максимум
                                    if (A[r].val > A[r - 1].val && A[r].val > A[r + 1].val)
                                    {
                                        //дельта относительно левого экстремума
                                        float localDelta = Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[r].val));
                                        //если откат самый максимальный
                                        if (localDelta < localReturnMax)
                                        {
                                            localReturnMax = localDelta;
                                            //индекс максимального отката
                                            localReturnMaxIndex = r;
                                        }
                                    }
                                    if (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[r].val)) >= u) break;
                                    r++;
                                }
                                //проверяем, дойдет ли цена до u после отката, опустившегося выше d
                                if (localReturnMax < d)
                                {
                                    int ind = localReturnMaxIndex;
                                    while (ind <= R[k].indexOrg)
                                    {
                                        ind++;
                                        //Цена дошла
                                        if (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[ind].val)) >= u)
                                        {
                                            amountGetUAfterReturnDown++;
                                            break;
                                        }
                                    }
                                }
                                //время для достижения d
                                //TimeSpan diffD = A[indexD].dateTime.Subtract(A[R[k - 1].indexOrg].dateTime);
                                //время для достижения u от d
                                //TimeSpan diffU = A[r].dateTime.Subtract(A[indexD].dateTime);
                                float dTime = indexD - R[k - 1].indexOrg + 3.5f;
                                float uTime = r - indexD + 3.5f;
                                if (dTime > maxTimeDDown) maxTimeDDown = dTime;
                                if (uTime > maxTimeUDown) maxTimeUDown = uTime;
                                averageTimeDDown += dTime;
                                averageTimeUDown += uTime;
                                //накапливаем суммарно откаты при движении вверх
                                if (localReturnMax < INFINITY && localReturnMax <= u)
                                {
                                    averageReturnDown += localReturnMax;
                                    if (localReturnMax < maxReturnDown) maxReturnDown = localReturnMax;
                                    amountReturnDown++;
                                }
                                amountDown++;
                            }
                            //int indexU = r;
                            amount++;
                        }
                        //если не дошли до u
                        else
                        {
                            if (signDelta > 0) amountDUp++;
                            else amountDDown++;
                        }
                    }
                    float probability = (float)amount / lengthExtremums * 100;
                    if (probability > probabilityLimit)
                    {

                        //list
                        b.startThreshold = d;
                        b.endThreshold = u;
                        b.lengthChanging = u - d;
                        b.probability = probability;

                        b.amountUp = amountUp;
                        b.amountDUp = amountDUp;

                        b.minTimeUUp = minTimeUUp;
                        b.averageTimeUUp = averageTimeUUp / amountUp;
                        b.maxTimeUUp = maxTimeUUp;

                        b.minTimeUDown = minTimeUDown;
                        b.averageTimeUDown = averageTimeUDown / amountDown;
                        b.maxTimeUDown = maxTimeUDown;

                        b.amountGetUAfterReturnDown = amountGetUAfterReturnDown;

                        b.amount = amount;

                        if (maxReturnUp == INFINITY) maxReturnUp = 0;
                        if (maxReturnDown == INFINITY) maxReturnDown = 0;

                        if (amountReturnUp > 0)
                        {
                            b.maxReturnUp = maxReturnUp;
                            b.averageReturnUp = averageReturnUp / amountReturnUp;
                        }
                        else
                        {
                            b.maxReturnUp = -1;
                            b.averageReturnUp = -1;
                        }


                        b.minTimeDUp = minTimeDUp;
                        b.averageTimeDUp = averageTimeDUp / amountUp;
                        b.maxTimeDUp = maxTimeDUp;

                        b.minTimeUUp = minTimeUUp;
                        b.averageTimeUUp = averageTimeUUp / amountUp;
                        b.maxTimeUUp = maxTimeUUp;

                        b.amountDown = amountDown;
                        b.amountDDown = amountDDown;

                        if (amountReturnDown > 0)
                        {
                            b.maxReturnDown = maxReturnDown;
                            b.averageReturnDown = averageReturnDown / amountReturnDown;
                        }
                        else
                        {
                            b.maxReturnDown = -1;
                            b.averageReturnDown = -1;
                        }

                        b.minTimeDDown = minTimeDDown;
                        b.averageTimeDDown = averageTimeDDown / amountDown;
                        b.maxTimeDDown = maxTimeDDown;

                        b.minTimeUDown = minTimeUDown;
                        b.averageTimeUDown = averageTimeUDown / amountDown;
                        b.maxTimeUDown = maxTimeUDown;

                        b.amountGetUAfterReturnUp = amountGetUAfterReturnUp;

                        b.amountReturnDUp = amountReturnDUp;

                        b.amountReturnUp = amountReturnUp;
                        list.Add(b);
                        //z++;
                    }
                }
            }
            LocalExtremumStatistic[] B = (LocalExtremumStatistic[])list.ToArray(typeof(LocalExtremumStatistic));
            return B;
        }


        /// <summary>
        /// Возвращает статистику по вероятности достижения порога изменения сигнала после достижения 
        /// определенного порога изменения, без откатов ниже/выше порога открытия позиция
        /// </summary>
        /// <param name="A"></param>
        /// <param name="startThreshold">Начальный порог</param>
        /// <param name="endThreshold">Конечный порог</param>
        /// <param name="deltaThreshold">Минимальное расстояние между d (нижний порог) и u (верхний порог) (в %)</param>
        /// <param name="deltaMaxThreshold">Максимальное расстояние между d и u</param>
        /// <param name="stepThreshold">Шаг изменения порога</param>
        /// <param name="probabilityLimit">Лимит вероятности, который должен быть набран для сохранения</param>
        /// <returns>Массив со статистикой для каждой пары d-u</returns>
        public static LocalExtremumStatistic[] GetLocalExtremumStatisticWithoutRecoil(Point[] A, int startInd, int endInd, float startThreshold, float endThreshold, float deltaThreshold, float deltaMaxThreshold, float stepThreshold, float probabilityLimit)
        {
            ArrayList list = new ArrayList(0);
            int stepCountD = (int)((float)(endThreshold - deltaThreshold - startThreshold) / stepThreshold);
            int lengthResult = (stepCountD) * (stepCountD - 1) / 2;
            LocalExtremumStatistic b = new LocalExtremumStatistic();
            for (int i = 0; i < stepCountD; i++)
            {
                //порог изменения (нижний), который преодолевается
                float d = startThreshold + stepThreshold * i;
                float winLength = endThreshold - d;
                winLength = (winLength > deltaMaxThreshold) ? deltaMaxThreshold : winLength;
                int stepLevelCount = (int)((float)winLength / stepThreshold);
                
                for (int j = 0; j < stepLevelCount; j++)
                {
                    //Верхний порог изменения, который достигается (или нет) после преодоления d 
                    float u = d + deltaThreshold + stepThreshold * j;
                    PointLocalExtremum[] R = GetLocalExtremumsSimple(u, A, startInd, endInd);
                    //ExportArray(R, "_local_e_" + d+"-" + u + ".csv");
                    int lengthExtremums = R.Length;
                    //d = 0.5f;
                    //u = 0.925f;

                    //кол-во случаев достижения u после преодоления d
                    int amount = 0;
                    //минимальное время достижения u после преодоления d
                    //float minTime = 100000000;
                    //максимальное время 
                    //float maxTime = 0;
                    //подсчет всего времени для расчета среднего
                    //float fullTime = 0;

                    //Движение вверх (от левого экстремума к правому)
                    //кол-во движений вверх
                    int amountUp = 0;
                    //наибольший откат при движении вверх (% от левого экстремума)
                    float maxReturnUp = INFINITY;
                    //средний откат при движении вверх
                    float averageReturnUp = 0;
                    //кол-во откатов при движении вверх
                    int amountReturnUp = 0;
                    //минимальное время для достижения порога d при движении вверх
                    float minTimeDUp = INFINITY;
                    //максимальное время для достижения порога d при движении вверх
                    float maxTimeDUp = 0;
                    //среднее время для достижения порога d при движении вверх
                    float averageTimeDUp = 0;
                    float averageTimeDUpWithoutRecoil = 0;//среднее время достижения порога d при движении вверх при котором достигается u без откатов ниже d
                    float averageTimeDUpRecoil = 0;//среднее время достижения порога d при движении вверх при котором НЕ достигается u без откатов ниже d или не достигается вообще
                    //минимальное время для достижения порога u при движении вверх (после преодоления d)
                    float minTimeUUp = INFINITY;
                    //максимальное время для достижения порога u при движении вверх
                    float maxTimeUUp = 0;
                    //среднее время для достижения порога u при движении вверх
                    float averageTimeUUp = 0;
                    //Кол-во случаев достижения от d до u при которых цена вначале откатывается ниже d
                    //при движении вверх
                    int amountGetUAfterReturnUp = 0;
                    //кол-во откатов ниже d
                    int amountReturnDUp = 0;
                    //кол-во НЕ достижений u при движении вверх
                    int amountDUp = 0;
                    //количество достижений u без откатов ниже d при движении вверх
                    int amountUpWithoutRecoil = 0;
                    //количество не достижений u и достижений u с откатами ниже d при движении вверх
                    int amountUpRecoil = 0;
                    ArrayList timeDUpRecoil = new ArrayList(0);
                    ArrayList timeDUpWithoutRecoil = new ArrayList(0);

                    //Движение вниз (от левого экстремума к правому)
                    //кол-во движений вниз при которых достигается u
                    int amountDown = 0;
                    //кол-во движений вниз при которых НЕ достигается u
                    int amountDDown = 0;
                    //наибольший откат при движении вниз (% от левого экстремума)
                    float maxReturnDown = INFINITY;
                    //средний откат при движении вниз
                    float averageReturnDown = 0;
                    //кол-во откатов при движении вниз
                    int amountReturnDown = 0;
                    //минимальное время для достижения порога d при движении вниз
                    float minTimeDDown = INFINITY;
                    //максимальное время для достижения порога d при движении вниз
                    float maxTimeDDown = 0;
                    //среднее время для достижения порога d при движении вниз
                    float averageTimeDDown = 0;
                    //среднее время достижения порога d при движении вниз при котором достигается u без откатов ниже d
                    float averageTimeDDownWithoutRecoil = 0;
                    float averageTimeDDownRecoil = 0;//среднее время достижения порога d при движении вниз при котором НЕ достигается u без откатов ниже d или не достигается вообще
                    
                    //количество достижений u без откатов ниже d при движении вниз
                    int amountDownWithoutRecoil = 0;
                    //количество не достижений u и достижений u с откатами ниже d при движении вниз
                    int amountDownRecoil = 0;
                    ArrayList timeDDownRecoil = new ArrayList(0);
                    ArrayList timeDDownWithoutRecoil = new ArrayList(0);


                    //минимальное время для достижения порога u при движении вниз (после преодоления d)
                    float minTimeUDown = INFINITY;
                    //максимальное время для достижения порога u при движении вниз
                    float maxTimeUDown = 0;
                    //среднее время для достижения порога u при движении вниз
                    float averageTimeUDown = 0;
                    //Кол-во случаев достижения от d до u при которых цена вначале откатывается выше d
                    //при движении вниз
                    int amountGetUAfterReturnDown = 0;
                    //кол-во достижений u при движении вниз
                    int amountUDown = 0;
                    

                    //тип прохода до u: 1 - без отката ниже d, 0 - с откатами или даже без дохода до u                    
                    byte type = 0;
                    
                    //Рассматриваем все экстремумы попарно
                    for (int k = 1; k < lengthExtremums; k++)
                    {                        
                        float delta = MathProcess.DeltaProcent(R[k - 1].val, R[k].val);
                        int signDelta = Math.Sign(delta);

                        //Анализируем внутренний участок между экстремумами k-1 и k                            

                        //с левого экстремума  идем до границы d                                
                        int r = R[k - 1].indexOrg + 1;                        
                        while (Math.Abs(MathProcess.DeltaProcent(R[k - 1].val, A[r].val)) <= d)
                        {
                            r++;
                        }
                        int indexD = r;
                        //время которое потребовалось чтобы дойти от левого экстремума до порога d
                        float dTime = indexD - R[k - 1].indexOrg;

                        //Если достигается нужный порог изменения
                        if (Math.Abs(delta) >= u)
                        {                            
                            //от d идем до u
                            r = indexD+1;
                            while (Math.Abs(MathProcess.DeltaProcent(R[k-1].val, A[r].val)) <= u)
                            {
                                r++;
                            }
                            int indexU = r;
                            //время чтобы дойти до u
                            float uTime = r - indexD;
                            
                            //если цена выросла между экстремумами k-1 и k
                            if (signDelta > 0)
                            {
                                //ищем локальные минимумы и самый минимальный сохраняем
                                //идем с границы d по u
                                r = indexD + 1;
                                type = 1;
                                while (r < indexU)
                                {
                                    //если цена опустилась ниже d при изменении до u
                                    if ((A[r].val - A[indexD].val)<=0)
                                    {
                                        type = 0;
                                        amountUpRecoil++;
                                        averageTimeDUpRecoil += dTime;
                                        timeDUpRecoil.Add(dTime);
                                        break;
                                    }                                                                       
                                    r++;
                                }
                                if (type == 1)
                                {                                    
                                    amountUpWithoutRecoil++;
                                    averageTimeDUpWithoutRecoil += dTime;
                                    timeDUpWithoutRecoil.Add(dTime);
                                }
                                
                                if (dTime > maxTimeDUp) maxTimeDUp = dTime;
                                if (uTime > maxTimeUUp) maxTimeUUp = uTime;
                                averageTimeDUp += dTime;
                                averageTimeUUp += uTime;                                
                                amountUp++;
                            }
                            //если цена упала
                            else
                            {                                
                                //с левого экстремума (d) идем до границы u
                                r = indexD + 1;
                                type = 1;
                                while (r < indexU)
                                {                                    
                                    //если откаты превышают d
                                    if ((A[r].val - A[indexD].val) >= 0)
                                    {
                                        type = 0;
                                        amountDownRecoil++;
                                        averageTimeDDownRecoil += dTime;
                                        timeDDownRecoil.Add(dTime);
                                        break;
                                    }                                                                        
                                    r++;
                                }
                                if (type == 1)
                                {                                    
                                    amountDownWithoutRecoil++;
                                    averageTimeDDownWithoutRecoil += dTime;
                                    timeDDownWithoutRecoil.Add(dTime);
                                }
                                if (dTime > maxTimeDDown) maxTimeDDown = dTime;
                                if (uTime > maxTimeUDown) maxTimeUDown = uTime;
                                averageTimeDDown += dTime;
                                averageTimeUDown += uTime;
                                amountDown++;
                            }
                            //int indexU = r;
                            amount++;
                        }
                        //если не дошли до u
                        else
                        {                            
                            //цена выросла
                            if (signDelta > 0)
                            {
                                amountUpRecoil++;
                                averageTimeDUpRecoil += dTime;
                                
                            }
                            //цена упала
                            else
                            {
                                amountDownRecoil++;
                                averageTimeDDownRecoil += dTime;
                            }
                        }
                    }
                    float probability = (float)(amountUpWithoutRecoil + amountDownWithoutRecoil) / (float)(amountUpWithoutRecoil + amountDownWithoutRecoil + amountUpRecoil + amountDownRecoil) * 100;
                    if (probability > probabilityLimit)
                    {
                        //list                    

                        b.startThreshold = d;
                        b.endThreshold = u;
                        b.lengthChanging = u - d;
                        b.probability = probability;

                        b.amountUp = amountUp;
                        b.amountDUp = amountDUp;

                        b.minTimeUUp = minTimeUUp;
                        b.averageTimeUUp = averageTimeUUp / amountUp;
                        b.maxTimeUUp = maxTimeUUp;

                        b.minTimeUDown = minTimeUDown;
                        b.averageTimeUDown = averageTimeUDown / amountDown;
                        b.maxTimeUDown = maxTimeUDown;

                        b.amountGetUAfterReturnDown = amountGetUAfterReturnDown;

                        b.amount = amount;

                        if (maxReturnUp == INFINITY) maxReturnUp = 0;
                        if (maxReturnDown == INFINITY) maxReturnDown = 0;

                        if (amountReturnUp > 0)
                        {
                            b.maxReturnUp = maxReturnUp;
                            b.averageReturnUp = averageReturnUp / amountReturnUp;
                        }
                        else
                        {
                            b.maxReturnUp = -1;
                            b.averageReturnUp = -1;
                        }


                        b.minTimeDUp = minTimeDUp;
                        b.averageTimeDUp = averageTimeDUp / amountUp;
                        b.maxTimeDUp = maxTimeDUp;

                        b.minTimeUUp = minTimeUUp;
                        b.averageTimeUUp = averageTimeUUp / amountUp;
                        b.maxTimeUUp = maxTimeUUp;

                        b.amountDown = amountDown;
                        b.amountDDown = amountDDown;

                        if (amountReturnDown > 0)
                        {
                            b.maxReturnDown = maxReturnDown;
                            b.averageReturnDown = averageReturnDown / amountReturnDown;
                        }
                        else
                        {
                            b.maxReturnDown = -1;
                            b.averageReturnDown = -1;
                        }

                        b.minTimeDDown = minTimeDDown;
                        b.averageTimeDDown = averageTimeDDown / amountDown;
                        b.maxTimeDDown = maxTimeDDown;

                        b.minTimeUDown = minTimeUDown;
                        b.averageTimeUDown = averageTimeUDown / amountDown;
                        b.maxTimeUDown = maxTimeUDown;

                        b.amountGetUAfterReturnUp = amountGetUAfterReturnUp;

                        b.amountReturnDUp = amountReturnDUp;

                        b.amountReturnUp = amountReturnUp;

                        b.averageTimeDDownRecoil = averageTimeDDownRecoil / amountDownRecoil;
                        b.averageTimeDUpRecoil = averageTimeDUpRecoil / amountUpRecoil;

                        b.amountDownWithoutRecoil = amountDownWithoutRecoil;
                        b.amountUpWithoutRecoil = amountUpWithoutRecoil;
                        b.averageTimeDDownWithoutRecoil = averageTimeDDownWithoutRecoil / amountDownWithoutRecoil;
                        b.averageTimeDUpWithoutRecoil = averageTimeDUpWithoutRecoil / amountUpWithoutRecoil;

                        b.amountDownRecoil = amountDownRecoil;
                        b.amountUpRecoil = amountUpRecoil;
                        b.averageTimeDDownRecoil = averageTimeDDownRecoil / amountDownRecoil;
                        b.averageTimeDUpRecoil = averageTimeDUpRecoil / amountUpRecoil;
                        
                        ExportArrayList(timeDUpRecoil, "timeDUpRecoil" + d + "-" + u + ".csv");
                        ExportArrayList(timeDUpWithoutRecoil, "timeDUpWithoutRecoil" + d + "-" + u + ".csv");
                        ExportArrayList(timeDDownRecoil, "timeDDownRecoil" + d + "-" + u + ".csv");
                        ExportArrayList(timeDDownWithoutRecoil, "timeDDownWithoutRecoil" + d + "-" + u + ".csv");

                        list.Add(b);
                        //z++;
                    }
                }
            }
            LocalExtremumStatistic[] B = (LocalExtremumStatistic[])list.ToArray(typeof(LocalExtremumStatistic));
            return B;
        }


        /// <summary>
        /// Чистое время торговли (без ночи и выходных)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float _getClearTime(DateTime a, DateTime b)
        {
            TimeSpan diff = b.Subtract(a);
            int minus_min = 0;
            if (diff.TotalMinutes <= 495) minus_min = 0;
            else if (diff.TotalMinutes > 495 && diff.Days == 0) minus_min = 945;
            else if (diff.Days > 0)
            {
                DateTime curDate = a.AddDays(1);
                int weekEndCount = 0;
                while (curDate < b)
                {
                    if (curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday) weekEndCount++;
                    curDate = curDate.AddDays(1);
                }
                minus_min = (diff.Days - weekEndCount) * 945;
            }
            return (float)diff.TotalMinutes - minus_min;
        }

        /// <summary>
        /// /// Чистое время торговли (без выходных)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float _getClearTimeWeekEnd(DateTime a, DateTime b)
        {
            TimeSpan diff = b.Subtract(a);
            int minus_min = 0;
            if (diff.Days > 0)
            {
                DateTime curDate = a.AddDays(1);
                int weekEndCount = 0;
                while (curDate < b)
                {
                    if (curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday) weekEndCount++;
                    curDate = curDate.AddDays(1);
                }
                minus_min = (diff.Days - weekEndCount) * 1440;
            }
            return (float)diff.TotalMinutes - minus_min;
        }

        /// <summary>
        /// Статистика по подтверждающим порогам после экстремумов с порогом threshold и подтверждающим порогом thresholdDisruption, с целью классификации 
        /// их на ложные и истинные.
        /// </summary>
        /// <param name="A">Времеменной ряд</param>
        /// <param name="startIndex">Начальный индекс из ряда</param>
        /// <param name="endIndex">Конечный индекс</param>
        /// <param name="threshold">Порог изменения в %</param>
        /// <param name="thresholdDisruption">Подверждающий порог в %</param>
        /// <returns>Подтверждающие пороги, с признаком ложного или истиного.
        /// Истиный порог признак того что наравление движения сигнала действительно изменилось на противоположное,
        /// и сигнал прошел порог threshold. Ложный означает что после пробития thresholdDisruption, threshold достиигнут не был,
        /// т.е. сигнал продолжил движение в прежнем направлении.
        /// </returns>        
        public static DisruptionPoint[] GetThresholdStatisticDisruption(Point[] A, int startIndex, int endIndex, float threshold, float thresholdDisruption)
        {
            //Общий список экстремумов
            ArrayList list = new ArrayList(0);
            //список истинных экстремумов            
            List<DisruptionPoint> listTrue = new List<DisruptionPoint>();
            //Индекс последнего экстремума - в начале не определен
            int lastIndExtremum = startIndex;
            //Индекс предпоследнего экстремума - в начале не определен
            int lastIndPreviousExtremum = startIndex;
            //Индекс последнего псевдоэкстремума - в начале = 1-й точке
            int lastIndPseudExtremum = startIndex;
            //индекс предпоследнего экстремума (истинного/ложного)
            int indPreviousPseudoExtremum = startIndex;
            float qvalSum = 0;
            //Точка пробития подтверждающего порога
            DisruptionPoint b = new DisruptionPoint();
            //Знак пробития подтверждающего порога 
            int signConfirmativeThreshold = 0;
            //Счетчик истинных порогов
            int countTrueThresholds = 0;
            //Флаг того что мы ищем следующий экстремум, в противоположном направлении от предыдущего
            bool isFindNextExtremum = false;
            //Знак последнего пробоя
            int lastSignThreshold = 0;
            float delta;
            //Условие по знаку - в начале направление пробоя нас не интересует
            bool signCondition = true;
            //Устанавливаем флаг истнности последнего порога в нейтраль            
            int flagVerityThreshold = 0;
            //Индекс последнего подвтерждающего порога
            int indConfirmativeThreshold = 0;
            //Фактический подтверждающий порог 
            float confirmativeThreshold = 0;
            //максимальное изменение сигнала относительно псевдоэкстремума
            float maxDeltaAbs = 0;
            int i = startIndex + 1;
            //Начинаем обход всех точек
            while (i < endIndex)
            {
                //Изменение сигнала от последнего псевдоэкстремума                
                delta = MathProcess.DeltaProcent(A[lastIndPseudExtremum].val, A[i].val);
                //Модуль изменения сигнала 
                float deltaAbs = Math.Abs(delta);
                //Знак изменения сигнала
                int signDelta = Math.Sign(delta);
                //Накопленные объемы от последнего экстремума до пробития подтверждающего порога
                qvalSum += A[i].qval;

                //Если ищем следующий экстремум, противоположный предыдущему
                if (isFindNextExtremum)
                {
                    //Условие по знаку - не должно совпадать с прошлым пробоем 
                    //т.е. после минимума - максимум
                    signCondition = (signDelta != lastSignThreshold);
                }
                //Если сигнал уже пробил подтверждающий порог, следим, не вернулся ли он обратно
                if (flagVerityThreshold == 1)
                {
                    //Если сигнал вернулся ниже точки последнего псевдоэкстремума (в противоположную сторону от 
                    //пробития подтверждающего порога)
                    if (signDelta != signConfirmativeThreshold)
                    {
                        //Значит порог ложный                        
                        b.type = 0;
                        b.indConfirmativeThreshold = indConfirmativeThreshold;
                        b.indExtremum = lastIndPseudExtremum;
                        b.indPreviousExtremum = lastIndExtremum;
                        b.indPreviousPseudoExtremum = indPreviousPseudoExtremum;
                        indPreviousPseudoExtremum = lastIndPseudExtremum;
                        list.Add(b);
                        flagVerityThreshold = 0;

                    }
                }
                //Если подтверждающий порог пробит и выполнено условие по знаку пробоя
                else if (deltaAbs > thresholdDisruption && signCondition)
                {
                    //Т.к. подтверждающий порог пробит - пока полагам что последний псевдоэкстремум - истинный
                    //т.е. что мы в этом же направлении пробьем основной порог изменения
                    flagVerityThreshold = 1;
                    //Знак изменения сигнала при пробитии подтверждающего порога
                    signConfirmativeThreshold = signDelta;
                    //Запоминаем индекс пробития порога
                    indConfirmativeThreshold = i;
                    confirmativeThreshold = deltaAbs;

                }
                //Если подтверждающий порог пробит и изменение больше основного порога и совпадает по знаку 
                //с подтверждающим порогом
                if (flagVerityThreshold == 1 && deltaAbs > threshold && signDelta == signConfirmativeThreshold)
                {
                    //Добавляем подтверждающий порог как истинный
                    b.type = 1;

                    //Индекс подтверждающего порога
                    b.indConfirmativeThreshold = indConfirmativeThreshold;

                    //Если не первый экстремум
                    if (lastIndExtremum != lastIndPreviousExtremum)
                    {
                        //Предпоследний экстремум становиться последним
                        lastIndPreviousExtremum = lastIndExtremum;
                    }
                    //Псевдоэкстремум становиться реальным экстремумом
                    lastIndExtremum = lastIndPseudExtremum;

                    //Индекс экстремума
                    b.indExtremum = lastIndExtremum;
                    b.indPreviousExtremum = lastIndPreviousExtremum;
                    //Индекс порога
                    b.indThreshold = i;
                    //индекс предпоследнего экстремума (истинного/ложного) 
                    b.indPreviousPseudoExtremum = indPreviousPseudoExtremum;
                    indPreviousPseudoExtremum = lastIndPseudExtremum;
                    //Добавляем в список общих порогов
                    list.Add(b);
                    //Добавляем в список истинных порогов
                    listTrue.Add(b);
                    //Увеличиваем счетчик истинных порогов
                    countTrueThresholds++;
                    //Псевдоэкстремум становиться реальным экстремумом
                    lastIndExtremum = lastIndPseudExtremum;
                    //Псевдоэкстремум устанавливаем в первую точку после пробития порога
                    //Для поиска нового экстремума
                    lastIndPseudExtremum = i;
                    //Смещение от последнего псевдоэкстремума
                    maxDeltaAbs = deltaAbs;
                    //Сбрасываем флаг истинности подтверждающего порога
                    flagVerityThreshold = 0;
                    //Знак последнего пробоя порога
                    lastSignThreshold = signConfirmativeThreshold;
                }
                //Если псевдоэкстремум не совпадает c последним экстремумом, значит ищем новый экстремум 
                //смещая псевдоэкстремум в сторону максимума/минимума
                if (lastIndPseudExtremum != lastIndExtremum)
                {
                    isFindNextExtremum = true;
                    float deltaExtremum = MathProcess.DeltaProcent(A[lastIndExtremum].val, A[i].val);
                    int signDeltaExtremum = Math.Sign(deltaExtremum);
                    float deltaExtremumAbs = Math.Abs(deltaExtremum);
                    //Смещаем псевдоэкстремум в направлении экстремума
                    if (signDeltaExtremum == lastSignThreshold && deltaExtremumAbs > maxDeltaAbs)
                    {
                        lastIndPseudExtremum = i;
                        //Запоминаем максимальное смещение относительно псевдоэкстремума
                        maxDeltaAbs = deltaExtremumAbs;
                    }
                }
                else
                {
                    isFindNextExtremum = false;
                }
                i++;
            }
            DisruptionPoint[] res = (DisruptionPoint[])list.ToArray(typeof(DisruptionPoint));
            return res;
        }

        /// <summary>
        /// Статистика по подтверждающим порогам после экстремумов с порогом threshold и подтверждающим порогом thresholdDisruption, с целью классификации 
        /// их на ложные и истинные.
        /// </summary>
        /// <param name="A">Времеменной ряд (просто float значения)</param>
        /// <param name="startIndex">Начальный индекс из ряда</param>
        /// <param name="endIndex">Конечный индекс</param>
        /// <param name="threshold">Порог изменения в %</param>
        /// <param name="thresholdDisruption">Подверждающий порог в %</param>
        /// <returns>Подтверждающие пороги, с признаком ложного или истиного.
        /// Истиный порог признак того что наравление движения сигнала действительно изменилось на противоположное,
        /// и сигнал прошел порог threshold. Ложный означает что после пробития thresholdDisruption, threshold достиигнут не был,
        /// т.е. сигнал продолжил движение в прежнем направлении.
        /// </returns>
        public static DisruptionPoint[] GetThresholdStatisticDisruption(float[] A, int startIndex, int endIndex, float threshold, float thresholdDisruption)
        {
            //Общий список экстремумов
            ArrayList list = new ArrayList(0);
            //список истинных экстремумов       
            List<DisruptionPoint> listTrue = new List<DisruptionPoint>();
            //Индекс последнего экстремума - в начале не определен
            int lastIndExtremum = startIndex;
            //Индекс предпоследнего экстремума - в начале не определен
            int lastIndPreviousExtremum = startIndex;
            //Индекс последнего псевдоэкстремума - в начале = 1-й точке
            int lastIndPseudExtremum = startIndex;
            //индекс предпоследнего экстремума (истинного/ложного)
            int indPreviousPseudoExtremum = startIndex;            
            //Точка пробития подтверждающего порога
            DisruptionPoint b = new DisruptionPoint();
            //Знак пробития подтверждающего порога 
            int signConfirmativeThreshold = 0;
            //Счетчик истинных порогов
            int countTrueThresholds = 0;
            //Флаг того что мы ищем следующий экстремум, в противоположном направлении от предыдущего
            bool isFindNextExtremum = false;
            //Знак последнего пробоя
            int lastSignThreshold = 0;
            float delta;
            //Условие по знаку - в начале направление пробоя нас не интересует
            bool signCondition = true;
            //Устанавливаем флаг истнности последнего порога в нейтраль            
            int flagVerityThreshold = 0;
            //Индекс последнего подвтерждающего порога
            int indConfirmativeThreshold = 0;
            //Фактический подтверждающий порог 
            float confirmativeThreshold = 0;
            //максимальное изменение сигнала относительно псевдоэкстремума
            float maxDeltaAbs = 0;
            int i = startIndex + 1;
            //Начинаем обход всех точек
            while (i < endIndex)
            {
                //Изменение сигнала от последнего псевдоэкстремума                
                delta = MathProcess.DeltaProcent(A[lastIndPseudExtremum], A[i]);
                //Модуль изменения сигнала 
                float deltaAbs = Math.Abs(delta);
                //Знак изменения сигнала
                int signDelta = Math.Sign(delta);                

                //Если ищем следующий экстремум, противоположный предыдущему
                if (isFindNextExtremum)
                {
                    //Условие по знаку - не должно совпадать с прошлым пробоем 
                    //т.е. после минимума - максимум
                    signCondition = (signDelta != lastSignThreshold);
                }
                //Если сигнал уже пробил подтверждающий порог, следим, не вернулся ли он обратно
                if (flagVerityThreshold == 1)
                {
                    //Если сигнал вернулся ниже точки последнего псевдоэкстремума (в противоположную сторону от 
                    //пробития подтверждающего порога)
                    if (signDelta != signConfirmativeThreshold)
                    {
                        //Значит порог ложный                        
                        b.type = 0;
                        b.indConfirmativeThreshold = indConfirmativeThreshold;
                        b.indExtremum = lastIndPseudExtremum;
                        b.indPreviousExtremum = lastIndExtremum;
                        b.indPreviousPseudoExtremum = indPreviousPseudoExtremum;
                        indPreviousPseudoExtremum = lastIndPseudExtremum;
                        list.Add(b);
                        flagVerityThreshold = 0;

                    }
                }
                //Если подтверждающий порог пробит и выполнено условие по знаку пробоя
                else if (deltaAbs > thresholdDisruption && signCondition)
                {
                    //Т.к. подтверждающий порог пробит - пока полагам что последний псевдоэкстремум - истинный
                    //т.е. что мы в этом же направлении пробьем основной порог изменения
                    flagVerityThreshold = 1;
                    //Знак изменения сигнала при пробитии подтверждающего порога
                    signConfirmativeThreshold = signDelta;
                    //Запоминаем индекс пробития порога
                    indConfirmativeThreshold = i;
                    confirmativeThreshold = deltaAbs;

                }
                //Если подтверждающий порог пробит и изменение больше основного порога и совпадает по знаку 
                //с подтверждающим порогом
                if (flagVerityThreshold == 1 && deltaAbs > threshold && signDelta == signConfirmativeThreshold)
                {
                    //Добавляем подтверждающий порог как истинный
                    b.type = 1;

                    //Индекс подтверждающего порога
                    b.indConfirmativeThreshold = indConfirmativeThreshold;

                    //Если не первый экстремум
                    if (lastIndExtremum != lastIndPreviousExtremum)
                    {
                        //Предпоследний экстремум становиться последним
                        lastIndPreviousExtremum = lastIndExtremum;
                    }
                    //Псевдоэкстремум становиться реальным экстремумом
                    lastIndExtremum = lastIndPseudExtremum;

                    //Индекс экстремума
                    b.indExtremum = lastIndExtremum;
                    b.indPreviousExtremum = lastIndPreviousExtremum;
                    //Индекс порога
                    b.indThreshold = i;
                    //индекс предпоследнего экстремума (истинного/ложного) 
                    b.indPreviousPseudoExtremum = indPreviousPseudoExtremum;
                    indPreviousPseudoExtremum = lastIndPseudExtremum;
                    //Добавляем в список общих порогов
                    list.Add(b);
                    //Добавляем в список истинных порогов
                    listTrue.Add(b);
                    //Увеличиваем счетчик истинных порогов
                    countTrueThresholds++;
                    //Псевдоэкстремум становиться реальным экстремумом
                    lastIndExtremum = lastIndPseudExtremum;
                    //Псевдоэкстремум устанавливаем в первую точку после пробития порога
                    //Для поиска нового экстремума
                    lastIndPseudExtremum = i;
                    //Смещение от последнего псевдоэкстремума
                    maxDeltaAbs = deltaAbs;
                    //Сбрасываем флаг истинности подтверждающего порога
                    flagVerityThreshold = 0;
                    //Знак последнего пробоя порога
                    lastSignThreshold = signConfirmativeThreshold;
                }
                //Если псевдоэкстремум не совпадает c последним экстремумом, значит ищем новый экстремум 
                //смещая псевдоэкстремум в сторону максимума/минимума
                if (lastIndPseudExtremum != lastIndExtremum)
                {
                    isFindNextExtremum = true;
                    float deltaExtremum = MathProcess.DeltaProcent(A[lastIndExtremum], A[i]);
                    int signDeltaExtremum = Math.Sign(deltaExtremum);
                    float deltaExtremumAbs = Math.Abs(deltaExtremum);
                    //Смещаем псевдоэкстремум в направлении экстремума
                    if (signDeltaExtremum == lastSignThreshold && deltaExtremumAbs > maxDeltaAbs)
                    {
                        lastIndPseudExtremum = i;
                        //Запоминаем максимальное смещение относительно псевдоэкстремума
                        maxDeltaAbs = deltaExtremumAbs;
                    }
                }
                else
                {
                    isFindNextExtremum = false;
                }
                i++;
            }
            DisruptionPoint[] res = (DisruptionPoint[])list.ToArray(typeof(DisruptionPoint));
            return res;
        }

        public static PointClassification[] convertDisruptionPointToPointClassification0(DisruptionPoint[] A, Point[] B)
        {
            PointClassification[] res = new PointClassification[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[2];
                res[i].coord[0] = (B[A[i].indPreviousExtremum].val - B[A[i].indExtremum].val) / B[A[i].indPreviousExtremum].val;
                //res[i].coord[1] = _getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indExtremum].dateTime) / (_getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indConfirmativeThreshold].dateTime)+1) / 100 ;
                res[i].coord[1] = _getClearTime(B[A[i].indPreviousPseudoExtremum].dateTime, B[A[i].indExtremum].dateTime) / 10000;
                res[i].time = B[A[i].indConfirmativeThreshold].dateTime;
            }
            return res;
        }

        public static PointClassification[] convertDisruptionPointToPointClassification(DisruptionPoint[] A, Point[] B)
        {
            PointClassification[] res = new PointClassification[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[3];
                res[i].coord[0] = (B[A[i].indPreviousExtremum].val - B[A[i].indExtremum].val) / B[A[i].indPreviousExtremum].val;
                //res[i].coord[1] = _getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indExtremum].dateTime) / (_getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indConfirmativeThreshold].dateTime)+1) / 100 ;
                res[i].coord[1] = _getClearTime(B[A[i].indPreviousPseudoExtremum].dateTime, B[A[i].indExtremum].dateTime) / 10000;
                
                float volume = 0;
                float volume2 = 0;
                for (int j = A[i].indPreviousExtremum; j < A[i].indExtremum; j++)
                {
                    volume += B[j].qval;
                }
                int t = i;
                int countFalseThreshold = 0;
                while (t >= 1 && A[t - 1].type == 0)
                {
                    countFalseThreshold++;
                    t--;
                }
                volume2 = 0;
                for (int j = A[i].indExtremum; j <= A[i].indConfirmativeThreshold; j++)
                {
                    volume2 += B[j].qval;
                }
                res[i].coord[2] = volume2 / (volume + 1);
                //res[i].coord[2] = volume2 / (volume + 1) / (countFalseThreshold + 1) * 10;
                //res[i].coord[3] = (float) (countFalseThreshold / 1000f);

                res[i].time = B[A[i].indConfirmativeThreshold].dateTime;
            }
            return res;
        }

        public static PointClassification[] convertDisruptionPointFToPointClassification(DisruptionPoint[] A, Point[] B)
        {
            PointClassification[] res = new PointClassification[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[3];
                res[i].coord[0] = (B[A[i].indPreviousExtremum].val - B[A[i].indExtremum].val) / B[A[i].indPreviousExtremum].val;
                //res[i].coord[1] = _getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indExtremum].dateTime) / (_getClearTimeWeekEnd(B[A[i].indPreviousExtremum].dateTime, B[A[i].indConfirmativeThreshold].dateTime)+1) / 100 ;
                res[i].coord[1] = _getClearTime(B[A[i].indPreviousPseudoExtremum].dateTime, B[A[i].indExtremum].dateTime) / 10000;

                float volume = 0;
                float volume2 = 0;
                for (int j = A[i].indPreviousExtremum; j < A[i].indExtremum; j++)
                {
                    volume += B[j].qval;
                }
                int t = i;
                int countFalseThreshold = 0;
                while (t >= 1 && A[t - 1].type == 0)
                {
                    countFalseThreshold++;
                    t--;
                }
                volume2 = 0;
                for (int j = A[i].indExtremum; j <= A[i].indConfirmativeThreshold; j++)
                {
                    volume2 += B[j].qval;
                }
                res[i].coord[2] = volume2 / (volume + 1);
                //res[i].coord[2] = volume2 / (volume + 1) / (countFalseThreshold + 1) * 10;
                //res[i].coord[3] = (float) (countFalseThreshold / 1000f);

                res[i].time = B[A[i].indConfirmativeThreshold].dateTime;
            }
            return res;
        }


        public static PointClassification[] convertDisruptionPointToPointClassification2(DisruptionPoint[] A, Point[] B)
        {
            PointClassification[] res = new PointClassification[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[7];
                res[i].coord[0] = (B[A[i].indPreviousExtremum].val - B[A[i].indExtremum].val) / B[A[i].indPreviousExtremum].val;
                res[i].coord[1] = _getClearTime(B[A[i].indPreviousExtremum].dateTime, B[A[i].indExtremum].dateTime);
                res[i].coord[2] = _getClearTime(B[A[i].indPreviousPseudoExtremum].dateTime, B[A[i].indExtremum].dateTime);

                float volume = 0;
                float volume2 = 0;
                for (int j = A[i].indPreviousExtremum; j < A[i].indExtremum; j++)
                {
                    volume += B[j].qval;
                }
                int t = i;
                int countFalseThreshold = 0;
                while (t >= 1 && A[t - 1].type == 0)
                {
                    countFalseThreshold++;
                    t--;
                }
                volume2 = 0;
                for (int j = A[i].indExtremum; j <= A[i].indConfirmativeThreshold; j++)
                {
                    volume2 += B[j].qval;
                }
                res[i].coord[3] = volume2;
                res[i].coord[4] = volume;
                res[i].coord[5] = countFalseThreshold;
                res[i].coord[6] = _getClearTime(B[A[i].indPreviousExtremum].dateTime, B[A[i].indConfirmativeThreshold].dateTime);
                res[i].time = B[A[i].indConfirmativeThreshold].dateTime;
            }
            return res;
        }

        /// <summary>
        /// Конвертация статистики по точкам экстремумов и подтверждающим порогам в точки для классификации подверждающих порогов на истинные и ложные
        /// </summary>
        /// <param name="A">Массив порогов</param>
        /// <param name="B">Исходный сигнал</param>
        /// <returns>Массив порогов пригодный для классификации</returns>
        public static PointClassificationSimple[] convertDisruptionPointToPointClassification2(DisruptionPoint[] A, float[] B)
        {
            PointClassificationSimple[] res = new PointClassificationSimple[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[5];
                res[i].coord[0] = (B[A[i].indPreviousExtremum] - B[A[i].indExtremum]) / B[A[i].indPreviousExtremum];
                res[i].coord[1] = B[A[i].indExtremum] - B[A[i].indPreviousExtremum];
                res[i].coord[2] = B[A[i].indExtremum] - B[A[i].indPreviousPseudoExtremum];
                int t = i;
                int countFalseThreshold = 0;
                while (t >= 1 && A[t - 1].type == 0)
                {
                    countFalseThreshold++;
                    t--;
                }
                res[i].coord[3] = countFalseThreshold;
                res[i].coord[4] = B[A[i].indConfirmativeThreshold] - B[A[i].indPreviousExtremum];                
            }
            return res;
        }

        /// <summary>
        /// Конвертация статистики по точкам экстремумов и подтверждающим порогам в точки для классификации подверждающих порогов на истинные и ложные
        /// </summary>
        /// <param name="A">Массив порогов</param>
        /// <param name="B">Исходный сигнал</param>
        /// <returns>Массив порогов пригодный для классификации</returns>
        public static PointClassificationSimple[] convertDisruptionPointToPointClassification3(DisruptionPoint[] A, float[] B)
        {
            PointClassificationSimple[] res = new PointClassificationSimple[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[4];
                res[i].coord[0] = (B[A[i].indPreviousExtremum] - B[A[i].indExtremum]);                
                res[i].coord[1] = (A[i].indExtremum - A[i].indPreviousExtremum);
                res[i].coord[2] = (B[A[i].indExtremum] - B[A[i].indConfirmativeThreshold]);
                res[i].coord[3] = (A[i].indExtremum - A[i].indConfirmativeThreshold);
            }
            return res;
        }

        /// <summary>
        /// Конвертация статистики по точкам экстремумов и подтверждающим порогам в точки для классификации подверждающих порогов на истинные и ложные
        /// </summary>
        /// <param name="A">Массив порогов</param>
        /// <param name="B">Исходный сигнал</param>
        /// <returns>Массив порогов пригодный для классификации</returns>
        public static PointClassificationSimple[] convertDisruptionPointToPointClassification4(DisruptionPoint[] A, float[] B)
        {
            PointClassificationSimple[] res = new PointClassificationSimple[A.Length];
            for (int i = 0; i < A.Length; i++)
            {
                res[i].classNum = A[i].type;
                res[i].coord = new float[5];
                res[i].coord[0] = B[A[i].indPreviousExtremum];
                res[i].coord[1] = B[A[i].indExtremum];
                res[i].coord[2] = A[i].indExtremum;
                res[i].coord[3] = A[i].indPreviousExtremum;
                res[i].coord[4] = B[A[i].indConfirmativeThreshold];
            }
            return res;
        }

        public static double[,] convertPointClassificationToDouble(PointClassification[] A)
        {
            double[,] B = new double[A.Length, A[0].coord.Length];
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < A[0].coord.Length; j++)
                {
                    B[i, j] = A[i].coord[j];
                }
            }
            return B;
        }

        /// <summary>
        /// Простое прореживание массива
        /// </summary>
        /// <param name="A">Массив с данными</param>
        /// <param name="num">Номер каждого забираемого</param>
        /// <returns>Массив содержащий каждый num-й элемент из A</returns>
        public static float[] GetDischargingArray(float[] A, int num)
        {
            int len = (int)Math.Floor((double)A.Length / num);
            float[] B = new float[len];
            int j = 0;
            int z = 0;
            for (int i = 0; i < A.Length; i++)
            {
                j++;
                if (num == j)
                {
                    B[z] = A[i];
                    j = 0;
                    z++;
                }
            }
            return B;
        }

        public static string[] GetDischargingArray(string[] A, int num)
        {
            int len = (int)Math.Floor((double)A.Length / num);
            string[] B = new string[len];
            int j = 0;
            int z = 0;
            for (int i = 0; i < A.Length; i++)
            {
                j++;
                if (num == j)
                {
                    B[z] = A[i];
                    j = 0;
                    z++;
                }
            }
            return B;
        }

        /// <summary>
        /// Прореживание массива скользящим средним
        /// </summary>
        /// <param name="A">Массив с данными</param>
        /// <param name="num">Длина окна скользящего среднего</param>
        /// <returns>Массив содержащий каждое по num элементов из A</returns>
        public static float[] GetMovingAverageArray(float[] A, int num)
        {
            int len = (int)Math.Floor((double)A.Length / num);
            float[] B = new float[len];
            int j = 0;
            int z = 0;
            float sum = 0;
            for (int i = 0; i < A.Length; i++)
            {
                j++;
                sum += A[i];
                if (num == j)
                {
                    B[z] = sum / num;
                    sum = 0;
                    j = 0;
                    z++;
                }
            }
            return B;
        }

        public static float[,] MovingAverage(float[,] A, int num)
        {
            int numChanals = A.GetLength(1);
            int newLength = A.GetLength(0) - num + 1;
            float[,] B = new float[newLength, numChanals];
            float sum;
            for (int i = 0; i < numChanals; i++)
            {
                for (int j = num - 1; j < A.GetLength(0); j++)
                {
                    sum = 0;
                    for (int k = 0; k < num; k++)
                    {
                        sum += A[j - k, i];
                    }
                    B[j - num + 1, i] = sum / num;
                }
            }
            return B;
        }

        public static float[] MovingAverage(float[] A, int num)
        {
            int newLength = A.Length - num + 1;
            float[] B = new float[newLength];
            float sum;
            int z = 0;
            for (int j = num - 1; j < A.Length; j++)
            {
                sum = 0;
                for (int k = 0; k < num; k++)
                {
                    sum += A[j - k];
                }
                B[z] = sum / num;
                z++;
            }
            return B;
        }

        public static Point[] MovingAverage(Point[] A, int num)
        {
            int newLength = A.Length - num + 1;
            Point[] B = new Point[newLength];
            float val_sum;
            int z = 0;
            for (int j = num - 1; j < A.Length; j++)
            {
                val_sum = 0;
                for (int k = 0; k < num; k++)
                {
                    val_sum += A[j - k].val;
                }
                B[z] = A[j];
                B[z].val = val_sum / num;
                z++;
            }
            return B;
        }

        
        public static float AveragePrice(Point[] A, int startInd, int lenBack)
        {                       
            float val_sum = 0;
            int z = 0;
            for (int j = 0; j < lenBack; j++)
            {
                val_sum += A[startInd-j].val;
            }
            val_sum /= lenBack;
            return val_sum;
        }

        public static float AveragePriceNext(Point[] A, int startInd, int lenBack, float previousAvr)
        {
            return previousAvr + (A[startInd - lenBack + 1].val - A[startInd+1].val) / lenBack;
        }

        public static float AveragePriceNextReverse(Point[] A, int startInd, int lenBack, float previousAvr)
        {
            return previousAvr + (A[startInd].val - A[startInd - lenBack].val) / lenBack;
        }

        public static void WriteToFile(string fileName, string record)
        {
            FileStream fOutput = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            writer.Write(record);
            writer.Close();
            fOutput.Close();
        }

        /// <summary>
        /// Сумма элементов массива
        /// </summary>
        /// <param name="A">Массив</param>
        /// <param name="startInd">начальный индекс</param>
        /// <param name="endInd">конечный индекс</param>
        /// <returns>Сумма элементов</returns>
        public static float sumArray(float[] A, int startInd, int endInd)
        {
            float sum = 0;
            for (int i = startInd; i <= endInd; i++)
            {
                sum += A[i];
            }
            return sum;
        }

        /// <summary>
        /// Среднее квадратическое элементов массива
        /// </summary>
        /// <param name="A">Массив</param>
        /// <param name="startInd">начальный индекс</param>
        /// <param name="endInd">конечный индекс</param>
        /// <returns>Среднее квадратическое</returns>
        public static float averageSquare(float[] A, int startInd, int endInd)
        {            
            float sum = 0;
            for (int i = startInd; i <= endInd; i++)
            {
                sum += A[i] * A[i];
            }
            sum /= (float)(endInd - startInd + 1);
            return sum;
        }

        /// <summary>
        /// Делает реверс массива
        /// </summary>
        /// <param name="C">Массив</param>
        /// <returns>Массив, где элементы переставлены в обратном порядке</returns>
        public static float[] reverseArray(float[] C)
        {
            float[] Rev = new float[C.Length];

            for (int t = 0; t < C.Length; t++)
            {
                Rev[t] = C[C.Length - t - 1];
            }
            return Rev;
        }

        public static float[,] convertArrayFromDoubleToFloat(double[,]A)
        {
            float[,] B = new float[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    B[i, j] = (float)A[i, j];
                }
            }
            return B;
        }

        /// <summary>
        /// Агрегируем файл с тиками получеными с Финама до секундных значений
        /// <DATE>;<TIME>;<LAST>;<VOL>
        /// 20090202;102959;113.00000;1
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="delimiter">Разделитель между столбцами</param>
        /// <returns>Массив значений</returns>
        public static Point[] AggregateToSecondsFromTicks(string fileName, char delimiter)
        {            
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            allFile = allFile.Replace("LKOH;0;", "");
            allFile = allFile.Replace(" ", "");
            allFile = allFile.Replace("\r\n\r\n", "\r\n");
            allFile = allFile.Replace("\r\r", "\r");
            fInput.Close();
            //String delim = "\n";
            //allFile = allFile.Trim(delim.ToCharArray());
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            ArrayList list = new ArrayList(1);
            Point b = new Point();            
            int j = 0;
            IFormatProvider culture = new CultureInfo("ru-RU", true);
            float valSum = 0;
            float qvalSum = 0;
            string dateTimeStr = "";
            for (int i = iStart; i < Lines.Length; i++)
            {
                if (Lines[i].Length > 5)
                {
                    string[] str_tmp = Lines[i].Split(delimiter);
                    string date = str_tmp[0];
                    string time = str_tmp[1];

                    string year = date.Substring(0, 4);
                    string month = date.Substring(4, 2);
                    string day = date.Substring(6, 2);

                    string hour = time.Substring(0, 2);
                    string min = time.Substring(2, 2);
                    string sec = time.Substring(4, 2);

                    //string myDateTimeValue = "06.02.1999 12:15:45";
                    string myDateTimeValue = day + "." + month + "." + year + " " + hour + ":" + min + ":" + sec;
                    //цена
                    float val = (str_tmp[2] != "") ? (float)Convert.ToDouble(str_tmp[2]) : 0;
                    float qval = (str_tmp[3] != "") ? (float)Convert.ToInt64(str_tmp[3]) : 0;

                    if (j > 0)
                    {
                        if (myDateTimeValue == dateTimeStr)
                        {
                            valSum += val * qval;
                            qvalSum += qval;
                        }
                        else
                        {
                            b.dateTimeStr = myDateTimeValue;
                            b.dateTime = DateTime.Parse(myDateTimeValue, culture, DateTimeStyles.NoCurrentDateDefault);
                            b.val = (float)Math.Round(valSum / qvalSum, 2);
                            b.qval = qvalSum;
                            list.Add(b);
                            j = 0;
                            valSum = val * qval;
                            qvalSum = qval;
                        }
                    }
                    else
                    {
                        valSum += val * qval;
                        qvalSum += qval;
                    }
                    dateTimeStr = myDateTimeValue;
                    j++;
                }
            }
            string fname = Path.GetFileNameWithoutExtension(fileName);
            string path = Path.GetDirectoryName(fileName) + @"\";
            Point[] B = (Point[])list.ToArray(typeof(Point));
            ExportArray(B, path+fname + "_sec.csv");
            //SaveObject(B, fname+".bin");
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return B;
        }

        public static string AggregateToSecondsFromTicksFiles(string fileName, int startNum, int endNum, char delimiter)
        {
            string path = Path.GetDirectoryName(fileName)+@"\";
            string fname = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            for (int i = startNum; i <= endNum; i++)
            {
                AggregateToSecondsFromTicks(path+fname + i + ext, delimiter);
            }
            for (int i = startNum; i <= endNum; i++)
            {
                FileStream fInput = File.Open(path + fname + i + "_sec" + ext, FileMode.Open, FileAccess.Read);
                StreamReader srInput = new StreamReader(fInput);
                string allFile = srInput.ReadToEnd();
                fInput.Close();
                File.AppendAllText(path+fname + startNum + "_" + endNum +".out", allFile);
            }
            return "Done;";
        }
        /// <summary>
        /// 11.01.2009;10:30:00;115;289167;
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Point[] LoadFromTicks(string fileName)
        {
            FileStream fInput = File.Open(fileName, FileMode.Open, FileAccess.Read);
            StreamReader srInput = new StreamReader(fInput);
            string allFile = srInput.ReadToEnd();
            //allFile = allFile.Replace(" ", "");
            allFile = allFile.Replace("\r\n\r\n", "\r\n");
            allFile = allFile.Replace("\r\r", "\r");
            fInput.Close();
            //String delim = "\n";
            //allFile = allFile.Trim(delim.ToCharArray());
            string[] Lines = allFile.Split('\n');
            //Если первая строка содержит текст - пропускаем ее
            Match m = Regex.Match(Lines[0], @"[A-z]");
            int iStart;
            if (m.Length > 0) iStart = 1;
            else iStart = 0;
            CalculateStart.CreateEvent(Lines.Length + 1, "Load data from " + fileName + "...", 0);
            ArrayList list = new ArrayList(1);
            Point b = new Point();
            int j = 0;
            IFormatProvider culture = new CultureInfo("ru-RU", true);
            float valSum = 0;
            float qvalSum = 0;
            string dateTimeStr = "";
            for (int i = iStart; i < Lines.Length; i++)
            {
                if (Lines[i].Length > 5)
                {                    
                    string[] str_tmp = Lines[i].Split(';');
                    /*
                    string[] date_time = str_tmp[0].Split(' ');
                    string[] data = str_tmp[1].Split(';');

                    string[] date_c = date_time[0].Split('.');
                    string[] time_c = date_time[1].Split(':');

                    string day = date[0];
                    string month = date[1];
                    string year = date[2];
                    
                    string hour = time_c[0];
                    string min = time_c[1];
                    string sec = time_c[2];
                    */
                    //string myDateTimeValue = "06.02.1999 12:15:45";
                    //myDateTimeValue = (string) str_tmp[0];
                    //string myDateTimeValue = day + "." + month + "." + year + " " + hour + ":" + min + ":" + sec;
                    //цена
                    float val = (str_tmp[1] != "") ? (float)Convert.ToDouble(str_tmp[1]) : 0;
                    float qval = (str_tmp[2] != "") ? (float)Convert.ToInt64(str_tmp[2]) : 0;

                    b.dateTimeStr = str_tmp[0];
                    b.dateTime = DateTime.Parse(str_tmp[0], culture, DateTimeStyles.NoCurrentDateDefault);
                    b.val = val;
                    b.qval = qval;
                    list.Add(b);
                }
            }
            Point[] B = (Point[])list.ToArray(typeof(Point));
            string fname = Path.GetFileNameWithoutExtension(fileName);
            string path = Path.GetDirectoryName(fileName) + @"\";
            SaveObject(B, path+fname + ".bin");
            GC.Collect(GC.GetGeneration(allFile));
            GC.Collect(GC.GetGeneration(Lines));
            return B;
        }
    }
}
