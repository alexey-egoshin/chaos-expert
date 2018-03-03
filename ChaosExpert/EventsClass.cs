using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ChaosExpert
{
    /// <summary>
    /// Сообщение для прогрессбара 
    /// </summary>
    public class CalculusInfoEventArgs : EventArgs
    {
        public readonly int current;
        public readonly int threadNum;
        public readonly string msg;

        public CalculusInfoEventArgs(int current)
        {
            this.current = current;
            this.threadNum = 0;
            this.msg = "";
        }

        public CalculusInfoEventArgs(int current, int threadNum)
        {
            this.current = current;
            this.threadNum = threadNum;
            this.msg = "";
        }

        public CalculusInfoEventArgs(int current, int threadNum, string msg)
        {
            this.current = current;
            this.threadNum = threadNum;
            this.msg = msg;
        }
    }


    /// <summary>
    /// Создание события при проведении вычислений для прогрессбара
    /// </summary>
    public class Calculus
    {
        public delegate void CalculusChangeHandler(CalculusInfoEventArgs calculusInfo);
        public static event CalculusChangeHandler OnCalculusChange;

        public static void CreateEvent(int current)
        {
            CalculusInfoEventArgs e = new CalculusInfoEventArgs(current);
            if (OnCalculusChange != null)
            {
                OnCalculusChange(e);
            }
        }

        public static void CreateEvent(int current, int threadNum)
        {
            CalculusInfoEventArgs e = new CalculusInfoEventArgs(current, threadNum);
            if (OnCalculusChange != null)
            {
                OnCalculusChange(e);
            }
        }

        public static void CreateEvent(int current, int threadNum, string msg)
        {
            CalculusInfoEventArgs e = new CalculusInfoEventArgs(current, threadNum, msg);
            if (OnCalculusChange != null)
            {
                OnCalculusChange(e);
            }
        }
    }

    /// <summary>
    /// Событие, возникающее перед началом длительных вычислений: установка максимума прогрессбара и вывод названия задачи
    /// </summary>
    public class MaxLengthEventArgs : EventArgs
    {
        //Длина прогрессбара
        public readonly int length;
        //Название запускаемой задачи
        public readonly string task;
        //Номер потока
        public readonly int threadNum;

        public MaxLengthEventArgs(int length, string task)
        {
            this.length = length;
            this.task = task;
        }

        public MaxLengthEventArgs(int length, string task, int threadNum)
        {
            this.length = length;
            this.task = task;
            this.threadNum = threadNum;
        }

    }

    /// <summary>
    /// Запуск события перед стартом вычислений
    /// </summary>
    public class CalculateStart
    {
        public delegate void CalculateStartHandler(MaxLengthEventArgs info);
        public static event CalculateStartHandler OnCalculateStart;

        public static void CreateEvent(int length, string task)
        {
            MaxLengthEventArgs e = new MaxLengthEventArgs(length, task);
            if (OnCalculateStart != null)
            {
                OnCalculateStart(e);
                Application.DoEvents();
            }
        }

        public static void CreateEvent(int length, string task, int threadNum)
        {
            MaxLengthEventArgs e = new MaxLengthEventArgs(length, task, threadNum);
            if (OnCalculateStart != null)
            {
                OnCalculateStart(e);
                Application.DoEvents();
            }
        }
    }
}
