using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChaosExpert
{
    public partial class FormProcess : Form
    {
        private ProgressBar[] progressBars;
        private Label[] labels;
        private int[] threadsProgress;
        private string[] threadsMsgs;
        public int[] msgThreads;

        public FormProcess()
        {
            InitializeComponent();
        }

        private void CreateProgressBar(int num)
        {
            Label label = new Label();
            label.Location = new System.Drawing.Point(5, 10 + 60 * num);
            label.Name = "process" + num.ToString();
            label.Text = "Process " + num.ToString();
            label.Size = new Size(600, 15);
            this.Controls.Add(label);
            labels[num] = label;            

            ProgressBar progressBar1 = new ProgressBar();
            progressBar1.Location = new System.Drawing.Point(5, 38 + 60 * num);
            progressBar1.Maximum = 100;
            progressBar1.Name = "progressBar"+num.ToString();
            progressBar1.Size = new Size(700, 20);
            progressBar1.Step = 1;
            progressBar1.Style = ProgressBarStyle.Continuous;            
            this.Controls.Add(progressBar1);
            progressBars[num] = progressBar1;
            this.Height += 55;
        }

        public void CreateProgressBars(int count) 
        {
            progressBars = new ProgressBar[count];
            labels = new Label[count];
            threadsProgress = new int[count];
            threadsMsgs = new string[count];            
            for (int i=0;i<count;i++)
            {
                CreateProgressBar(i);
                threadsMsgs[i] = "";
            }
            Subscribe();
        }

        private void Subscribe()
        {
            Calculus.OnCalculusChange += new Calculus.CalculusChangeHandler(Calculus_OnCalculusChange);
            CalculateStart.OnCalculateStart += new CalculateStart.CalculateStartHandler(CalculateStart_OnCalculateStart);
        }

        private void CalculateStart_OnCalculateStart(MaxLengthEventArgs info)
        {
            ShowTask(info.task, info.length, info.threadNum);
            Application.DoEvents();
        }

        delegate void ShowTaskThreadDelegate(string task, int maxValue, int threadNum);
        /// <summary>
        /// Отображение на лейбле выполняемой задачи и установка максимального значения для прогрессбара
        /// </summary>
        /// <param name="task">Название задачи</param>
        /// <param name="maxValue">Максимальное значение прогрессбара</param>
        /// <param name="threadNum">Номер потока</param>
        private void ShowTask(string task, int maxValue, int threadNum)
        {
            // Проверяем, не вызывается ли метод из UI-потока                                    
            if (progressBars[threadNum].InvokeRequired == false)
            {                
                labels[threadNum].Text = task;
                progressBars[threadNum].Maximum = maxValue;
            }
            else
            {
                // Синхронно показываем информацию о ходе выполнения
                ShowTaskThreadDelegate showTask = new ShowTaskThreadDelegate(ShowTask);
                this.BeginInvoke(showTask, new object[] { task, maxValue, threadNum });
            }
        }

        private void Calculus_OnCalculusChange(CalculusInfoEventArgs calculusInfo)
        {            
            threadsProgress[calculusInfo.threadNum] = calculusInfo.current;
            if (calculusInfo.msg != "") threadsMsgs[calculusInfo.threadNum] = calculusInfo.msg;
        }
 
        private void timer1_Tick(object sender, EventArgs e)
        {                        
            for (int i = 0; i < progressBars.Length; i++)
            {
                progressBars[i].Value = threadsProgress[i];
                if (threadsMsgs[i].Length > 1) labels[i].Text = threadsMsgs[i];
                threadsMsgs[i] = "";
                threadsProgress[i] = 0;
            }
        }

        private void FormProcess_VisibleChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            for (int i = 0; i < progressBars.Length; i++)
            {
                progressBars[i].Value = 0;
                threadsMsgs[i] = "";
            }
        }

        private void FormProcess_Activated(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }    
}