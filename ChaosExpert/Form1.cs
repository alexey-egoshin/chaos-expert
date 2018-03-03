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

namespace ChaosExpert
{
    public partial class Form1 : Form
    {
        FormProcess form2;
        ReportForm formReport = new ReportForm();

        GraphForm graphForm = new GraphForm();
        GraphForm graphForm2 = new GraphForm();

        GraphForm newGraphForm = new GraphForm();
        
        int threadsCount = 2;
        string[][] BatchLines = new string[0][];
        int doBatchLine=0;
        private DataParams dataParams;

        public Form1(string[] args)
        {
            InitializeComponent();            
            if (args.Length > 0)
            {
                //Пакетный файл
                if (args[0] == "batch")
                {
                    FileStream fInput = File.Open(args[1], FileMode.Open, FileAccess.Read);
                    StreamReader srInput = new StreamReader(fInput);
                    string allFile = srInput.ReadToEnd();
                    string[] Lines = allFile.Split('\n');
                    BatchLines = new string[Lines.Length][];
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        string[] param = Lines[i].Split(' ');
                        BatchLines[i] = param;
                    }
                    doBatchLine = 0;
                    Run(BatchLines[0]);
                }
                else
                {
                    Run(args);
                }
            }
        }

        delegate object ProcessDelegate(string processName, DataParams dataParam, object param);
        private object asyncProcess(string processName, DataParams dataParam, object param)
        {
            // Начинаем асинхронное вычисление
            ProcessDelegate process = new ProcessDelegate(ProcessRun);
            AsyncCallback cb = new AsyncCallback(MyAsyncCallback);
            //IAsyncResult ar = process.BeginInvoke(processName, dataParam, param, cb, process);
            IAsyncResult ar = process.BeginInvoke(processName, dataParam, param, null, null);
            //ar.AsyncWaitHandle.WaitOne();
            //object res = process.EndInvoke(ar);

            return null;
        }

        public void MyAsyncCallback(IAsyncResult ar)
        {
	        object s;
	        int iExecThread = 12;
	        // Because you passed your original delegate in the asyncState parameter
	        // of the Begin call, you can get it back here to complete the call.
	        ProcessDelegate process = (ProcessDelegate) ar.AsyncState;

	        // Complete the call.
	        s = process.EndInvoke (ar) ;
            //formReport.richTextBox1.AppendText("Async done!");
	        //MessageBox.Show (string.Format ("The delegate call returned the string:   \"{0}\", and the number {1}", s, iExecThread.ToString() ) );
        }

        public void Run(string[] args)
        {
            form2 = new FormProcess();
            formReport.MdiParent = this;
            formReport.Show();
            form2.Activate();
            graphForm = new GraphForm();
            graphForm.MdiParent = this;

            //Парсим командную строку
            /*
             * Параметры для запрашиваемых данных:
             * -df имя файла
             * -dm максимальное число принимаемых записей (если превышение, будет произведена редукция)
             * -dci индекс принимаемого столбца
             * -dd разделитель столбцов
             * -dtp строка с временным периодом
             * -dfr формат принимаемых данных (finam/classic)
             */
            Hashtable param = new Hashtable();
            string processName = args[0];
            for (int i = 1; i < args.Length - 1; i+=2) param.Add(args[i], args[i+1]);
            dataParams = new DataParams();
            //param["dxxx"] - параметр относится к обработке даннных
            //param["pxxx"] - параметр относится к вычислительной процедуре
            dataParams.fileName = (string)param["dfn"];            
            dataParams.timePeriod = (string)param["dtp"];
            dataParams.maxCount = Convert.ToInt32(param["dmc"]);
            dataParams.columnIndex = Convert.ToInt32(param["dci"]);
            dataParams.delimiter = Convert.ToChar(param["dd"]);
            dataParams.format = (string)param["dfr"];
            dataParams.isDoClearFile = (bool)Convert.ToBoolean(param["didcf"]);
            //Параметры для вычислительной процедуры
            Object paramProcedure = null;
            switch (processName)
            {                                
                //Корр. интеграл
                case "cd":
                    CorrelationDimParams paramCD = new CorrelationDimParams();
                    paramCD.r = (float)Convert.ToDouble(param["pr"]);
                    paramCD.startDim = Convert.ToInt32(param["psd"]);
                    paramCD.endDim = Convert.ToInt32(param["ped"]);
                    paramCD.stepCount = Convert.ToInt32(param["psc"]);
                    paramProcedure = paramCD;
                    break;
                //RS-анализ, вычисление показателя Херста
                case "rs":                    
                    form2.CreateProgressBars(threadsCount);
                    form2.Show();
                    FractalDimParams paramRS = new FractalDimParams();
                    paramProcedure = paramRS;
                    break;
                //AutoMutual Info
                case "ami":
                    AutoMutualInfoParams paramAUI = new AutoMutualInfoParams();                    
                    paramAUI.startDelay = Convert.ToInt32(param["psd"]);
                    paramAUI.endDelay = Convert.ToInt32(param["ped"]);
                    paramAUI.levelsCount = Convert.ToInt32(param["plc"]);
                    paramAUI.thredCount = Convert.ToInt32(param["ptc"]);
                    paramProcedure = paramAUI;                    
                    break;
                //Оценка максимального показателя Ляпунова
                case "lyapunov":
                    form2.CreateProgressBars(threadsCount);
                    form2.Show();
                    LyapunovParams paramLyap = new LyapunovParams();
                    paramLyap.embedingDimension = Convert.ToInt32(param["ped"]);
                    paramLyap.lag = Convert.ToInt32(param["pl"]);
                    paramLyap.count = Convert.ToInt32(param["pc"]);
                    paramProcedure = paramLyap;
                    break;
                //Оценка индекса вариации
                case "varind":
                    paramProcedure = getVarIndParam(param);
                    break;
                //Многслойный персептрон
                case "mlp":                    
                    PerseptronParams paramMLP = new PerseptronParams();
                    paramMLP.numInputSignals = 1;                    
                    int[] numNeuronsInLayer = {3,2,1};
                    paramMLP.numLayers = numNeuronsInLayer.Length;
                    paramMLP.numNeuronsInLayer = numNeuronsInLayer;
                    paramMLP.learningSet = Convert.ToInt32(param["pls"]);
                    paramMLP.delayInput = Convert.ToInt32(param["pdi"]);
                    paramMLP.predictionStep = 1;
                    paramMLP.numEpochs = 4000;
                    paramProcedure = paramMLP;
                    break;
                case "data_process":
                    paramProcedure = null;
                    break;
                case "trade":
                    paramProcedure = null;
                    GraphForm newGraphForm = new GraphForm();
                    break;
                case "classification":
                    graphForm2 = new GraphForm();
                    graphForm2.MdiParent = this;
                    paramProcedure = null;
                    break;
                case "disorder_brodsky_darhovsky_overall":
                    paramProcedure = (float) Convert.ToDouble(param["pv"]);
                    break;
                case "time_threshold_change":
                    paramProcedure = (float)Convert.ToDouble(param["pt"]);
                    break;
                case "generate_trade_signal":
                    paramProcedure = null;
                    break;
                case "aggregate_to_secs":
                    AggregateToSec paramATS = new AggregateToSec();
                    paramATS.delimiter = Convert.ToChar(param["pdelim"]);
                    paramATS.startNum = Convert.ToInt32(param["pstart_num"]);
                    paramATS.endNum = Convert.ToInt32(param["pend_num"]);
                    paramProcedure = paramATS;
                    break;
                /// <param name="startThreshold">Начальный порог</param>
                /// <param name="endThreshold">Конечный порог</param>
                /// <param name="deltaThreshold">Минимальное расстояние между d и u (в %)</param>
                /// <param name="deltaMaxThreshold">Максимальное расстояние между d и u</param>
                /// <param name="stepThreshold">Шаг изменения порога</param>
                /// <param name="probabilityLimit">Лимит вероятности, который должен быть набран для сохранения</param>
                case "local_extremum_statistic":
                    LocalExtremumStatParams paramLES = new LocalExtremumStatParams();
                    paramLES.startThreshold = (float)Convert.ToDouble(param["p_startThreshold"]);
                    paramLES.endThreshold = (float)Convert.ToDouble(param["p_endThreshold"]);
                    paramLES.deltaThreshold = (float)Convert.ToDouble(param["p_deltaThreshold"]);
                    paramLES.deltaMaxThreshold = (float)Convert.ToDouble(param["p_deltaMaxThreshold"]);
                    paramLES.stepThreshold = (float)Convert.ToDouble(param["p_stepThreshold"]);
                    paramLES.probabilityLimit = (float)Convert.ToDouble(param["p_probabilityLimit"]);
                    paramProcedure = paramLES;
                    break;                    
            }
            Object res = asyncProcess(processName, dataParams, paramProcedure);
            switch (processName)
            {                
                case "trade":
                    //TradeResults r = new CorrelationDimParams();                    
                    break;
            }
            
        }

        private object getVarIndParam(Hashtable param)
        {
            VariationIndParams paramVarInd = new VariationIndParams();
            paramVarInd.startIndex = Convert.ToInt32(param["psi"]);
            paramVarInd.endIndex = Convert.ToInt32(param["pei"]);
            paramVarInd.startSegmentLength = Convert.ToInt32(param["psl"]);
            paramVarInd.endSegmentLength = Convert.ToInt32(param["pel"]);
            paramVarInd.windowLength = Convert.ToInt32(param["pwl"]);
            paramVarInd.numPointsRegression = Convert.ToInt32(param["pnum_reg"]);
            
            return paramVarInd;
        }

        private float[] LoadArrayByTime(DataParams prm, ref string periodStr, ref int discharging)
        {                        
            float[] C;
            float[] A = DataProcess.LoadDataTimePeriod(prm.fileName, prm.columnIndex, 4, prm.timePeriod, prm.delimiter, prm.format, ref periodStr);
            if (A.Length > prm.maxCount)
            {
                //Прореживание
                double step = (double)A.Length / prm.maxCount;
                discharging = (int)Math.Ceiling(step);
            }
            string[] S = DataProcess.LoadDataWithTimePeriod(prm.fileName, prm.columnIndex, prm.timePeriod, prm.delimiter, prm.format, ref periodStr);
            if (discharging > 1)
            {
                //float[] B = DataProcess.GetDischargingArray(A, discharging);
                float[] B = DataProcess.GetMovingAverageArray(A, discharging);
                string[] SS = DataProcess.GetDischargingArray(S, discharging);
                DataProcess.ExportArray(B, prm.fileName + "_org.csv");
                DataProcess.ExportArray(SS, "_time_" + prm.fileName);
                GC.Collect(GC.GetGeneration(A));
                //DataProcess.ExportArray(B, fileName+"_dis"+discharging.ToString()+".csv");
                //C = DataProcess.GetLnDifference(B);
                C = DataProcess.MovingAverage(B, 12);
                //C = DataProcess.NormalizationByAverage(C);
                GC.Collect(GC.GetGeneration(B));
            }
            else
            {
                //C = DataProcess.GetLnDifference(A);
                C = DataProcess.MovingAverage(A, 12);
                //C = DataProcess.NormalizationByAverage(C);
                GC.Collect(GC.GetGeneration(A));
            }
            DataProcess.ExportArray(C, prm.fileName + "_norm.csv");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return C;
        }

        private float[] LoadArraySimple(DataParams dataParams, ref int discharging)
        {            
            float[] C;
            float[] A = DataProcess.LoadDataFloat(dataParams.fileName, dataParams.columnIndex, ";");
            if (A.Length > dataParams.maxCount)
            {
                //Прореживание
                double step = (double)A.Length / dataParams.maxCount;
                discharging = (int)Math.Ceiling(step);
            }
            if (discharging > 1)
            {
                float[] B = DataProcess.GetDischargingArray(A, discharging);
                DataProcess.ExportArray(B, "data.csv");
                GC.Collect(GC.GetGeneration(A));                
                //DataProcess.ExportArray(B, fileName+"_dis"+discharging.ToString()+".csv");
                //C = DataProcess.GetLnDifference(B);
                C = DataProcess.NormalizationByAverage(B);                
                GC.Collect(GC.GetGeneration(B));                
            }
            else
            {
                //C = DataProcess.GetLnDifference(A);
                //C = DataProcess.NormalizationByAverage(A);                
                //GC.Collect(GC.GetGeneration(A));                
            }
            //C = DataProcess.MovingAverage(C, 2);
            //DataProcess.ExportArray(C, dataParams.fileName + "_norm.csv");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return A;
        }

        private string AggregateToSecs(DataParams dataParam, object param)
        {
            AggregateToSec prm = (AggregateToSec)param;
            DataProcess.AggregateToSecondsFromTicksFiles(dataParam.fileName, prm.startNum, prm.endNum, prm.delimiter);
            /*
            string path = Path.GetDirectoryName(dataParam.fileName)+@"\";
            string fname = path+Path.GetFileNameWithoutExtension(dataParam.fileName);
            string ext = Path.GetExtension(dataParam.fileName);
            Point[] A = DataProcess.AggregateToSecondsFromTicks(fname + prm.startNum + "_" + prm.endNum + ".out", prm.delimiter);
            DataProcess.SaveObject(A, fname + "_tics_" + prm.startNum + "_" + prm.endNum + ".bin");
            */
            return "Done";
        }       

        private string LocalExtremumStat(DataParams dataParam, object param)
        {
            LocalExtremumStatParams prm = (LocalExtremumStatParams)param;
            Point[] A = (Point[]) DataProcess.LoadObject(dataParam.fileName);
            //DataProcess.ExportArray(A, "1.csv");
            LocalExtremumStatistic[] B = DataProcess.GetLocalExtremumStatisticWithoutRecoil(A, 0, A.Length, prm.startThreshold, prm.endThreshold, prm.deltaThreshold, prm.deltaMaxThreshold, prm.stepThreshold, prm.probabilityLimit);
            DataProcess.ExportArray(B, "_stat.csv");
            return "Done";
        }

        private string Trade(GraphForm newGraphForm)
        {
            /*
            listGraphs listGraphics = new listGraphs();
            ShowGraph("test", listGraphics, newGraphForm);
            return "0";
             */

            //Point[] A = (Point[])DataProcess.LoadObject(@"sec\GAZP_10_1_5.bin");
            //Point[] A = (Point[])DataProcess.LoadObject(@"sec\LKOH_10_1_5.bin");
            Point[] A = (Point[])DataProcess.LoadObject(@"sec\FGAZP_10_1_5.bin");

            //Point[] A = (Point[])DataProcess.LoadFromTicks(@"sec\FGAZP_1_5.out");
            //DataProcess.SaveObject(A, "FGAZP_1_5.bin");
            Trader trader = new Trader(A);
            int startInd = 1098000;
            int endInd = 1200000;
            TradeResults resTrade = trader.StrategySimpleOrangeAcrossRed(0, startInd, endInd);
            float[,] MAs = resTrade.MAs;
            DataProcess.ExportArray(resTrade.cashVals, "cash_vals.txt");
            listGraphs listGraphics = new listGraphs();
            listGraphics.myGraphPane = new GraphPane();

            int fontSize = 5;
            listGraphics.myGraphPane.YAxis.Title.FontSpec.Size = fontSize;
            listGraphics.myGraphPane.YAxis.Scale.FontSpec.Size = fontSize;
            listGraphics.myGraphPane.XAxis.Scale.FontSpec.Size = fontSize;
            listGraphics.myGraphPane.XAxis.Title.FontSpec.Size = fontSize;
            listGraphics.myGraphPane.Title.FontSpec.Size = fontSize;
            listGraphics.list = new PointPairList[8];
            listGraphics.curves = new LineItem[8];

            PointPairList listGraphPrice = new PointPairList();
            //PointPairList listGraphVolume = new PointPairList();
            PointPairList listGraphMA0 = new PointPairList();
            PointPairList listGraphMA1 = new PointPairList();
            PointPairList listGraphMA2 = new PointPairList();
            PointPairList listGraphMA3 = new PointPairList();
            PointPairList listGraphMA4 = new PointPairList();
            PointPairList listGraphBuy = new PointPairList();
            PointPairList listGraphSell = new PointPairList();

            float maxPrice = 0;
            float minPrice = 999999999999999999;
            for (int i = startInd; i < endInd; i++)
            {
                listGraphPrice.Add(i, A[i].val);
                if (A[i].val > maxPrice)
                {
                    maxPrice = A[i].val;
                }
                else if (A[i].val < minPrice)
                {
                    minPrice = A[i].val;
                }                
            }
            /*
            NormalizationByBoundResultOneChannel newVol = DataProcess.NormalizationByBound(A, startInd, endInd - 1, minPrice, maxPrice);
            for (int i = 0; i < newVol.A.Length; i++)
            {
                listGraphVolume.Add(i + startInd, newVol.A[i]);
            }
            */
            float[] income = new float[resTrade.executedOrders.Length];
            for (int i = 0; i < resTrade.executedOrders.Length; i++)
            {
                if (resTrade.executedOrders[i].type == Trader.BUY)
                {
                    listGraphBuy.Add(resTrade.executedOrders[i].timeNum, resTrade.executedOrders[i].price);
                }
                else
                {
                    listGraphSell.Add(resTrade.executedOrders[i].timeNum, resTrade.executedOrders[i].price);
                }

            }

            for (int i = 0; i < resTrade.executedOrdersCond.Length; i++)
            {
                listGraphBuy.Add(resTrade.executedOrdersCond[i].timeNum, resTrade.executedOrdersCond[i].price);
            }

            for (int i = 0; i < MAs.GetLength(1); i++)
            {
                listGraphMA1.Add(i+startInd, MAs[0,i]);
                listGraphMA2.Add(i + startInd, MAs[1, i]);
                listGraphMA3.Add(i + startInd, MAs[2, i]);
                listGraphMA0.Add(i + startInd, MAs[3, i]);
                listGraphMA4.Add(i + startInd, MAs[4, i]);
            }

            LineItem curve = new LineItem("Sell");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = false;
            curve.Color = Color.Red;
            curve.Symbol.Type = SymbolType.TriangleDown;
            curve.Symbol.Fill = new Fill(Color.Red);
            curve.Symbol.Size = 3;
            curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[0] = curve;
            listGraphics.list[0] = listGraphSell;

            curve = new LineItem("Buy");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = false;
            curve.Color = Color.Green;
            curve.Symbol.Type = SymbolType.Triangle;
            curve.Symbol.Fill = new Fill(Color.Green);
            curve.Symbol.Size = 4;
            curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[1] = curve;
            listGraphics.list[1] = listGraphBuy;

            curve = new LineItem("Price");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Blue;
            curve.Symbol.Type = SymbolType.Circle;
            curve.Symbol.Fill = new Fill(Color.Blue);
            curve.Symbol.Size = 1;
            curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[2] = curve;
            listGraphics.list[2] = listGraphPrice;

            curve = new LineItem("MA1");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Black;
            curve.Symbol.Type = SymbolType.Circle;
            //curve.Symbol.Fill = new Fill(Color.Black);
            curve.Symbol.Size = 1;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[3] = curve;
            listGraphics.list[3] = listGraphMA1;

            curve = new LineItem("MA2");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Green;
            curve.Symbol.Type = SymbolType.Circle;
            //curve.Symbol.Fill = new Fill(Color.Black);
            curve.Symbol.Size = 1;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[4] = curve;
            listGraphics.list[4] = listGraphMA2;


            curve = new LineItem("MA3");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Red;
            curve.Symbol.Type = SymbolType.Circle;            
            curve.Symbol.Size = 1;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[5] = curve;
            listGraphics.list[5] = listGraphMA3;

            curve = new LineItem("MA0");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Orange;
            curve.Symbol.Type = SymbolType.Circle;
            //curve.Symbol.Fill = new Fill(Color.Black);
            curve.Symbol.Size = 1;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[6] = curve;
            listGraphics.list[6] = listGraphMA0;

            curve = new LineItem("MA4");
            curve.IsY2Axis = false;
            curve.Line.IsVisible = true;
            curve.Color = Color.Purple;
            curve.Symbol.Type = SymbolType.Circle;
            //curve.Symbol.Fill = new Fill(Color.Black);
            curve.Symbol.Size = 1;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[7] = curve;
            listGraphics.list[7] = listGraphMA4;


            /*
            curve = new LineItem("Volume");
            curve.IsY2Axis = true;
            curve.Line.IsVisible = true;
            curve.Color = Color.DarkOrange;
            curve.Symbol.Type = SymbolType.Circle;
            //curve.Symbol.Fill = new Fill(Color.DarkOrange);
            curve.Symbol.Size = 2;
            //curve.Symbol.Border.IsVisible = false;
            listGraphics.curves[6] = curve;
            listGraphics.list[6] = listGraphVolume;
            */
            ShowGraphPrice("test", listGraphics, newGraphForm, minPrice, maxPrice);

            return resTrade.report;             
        }

        private object ProcessRun(string processName, DataParams dataParam, object param)
        {
            ResultRS RS;
            float[] C;                        
            string periodStr = "";
            //Шаг выборки
            int discharging = 2;
            string report = "Процедура: " + processName;
            if (dataParam.fileName != null)
            {
                report += "\nФайл: " + dataParam.fileName;
                report += "\nИндекс столбца: " + dataParam.columnIndex.ToString();
                if (dataParam.timePeriod != null)
                {
                    report += "\nЗаданный временной интервал: " + dataParam.timePeriod;
                    report += "\nРеальный временной интервал: " + periodStr;
                }
            }
            int errors = 0;
            Point[] A = new Point[1];
            Point[] MA = new Point[1];
            DisruptionPoint[] B = new DisruptionPoint[1];
            switch (processName)
            {
                //Correlating Dimension
                case "cd":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();
                
                    //Преобразуем параметры
                    CorrelationDimParams prm = (CorrelationDimParams)param;                                         
                    CorrelatingDimensionResult cd;
                    cd = ChaosLogic.GetCorrelatingDimension((float)prm.r, prm.stepCount, C, prm.startDim, prm.endDim, threadsCount);
                    RS = ChaosLogic.RS_Analyse(C, 3, 0, C.Length - 1, C.Length, threadsCount);
                    DataProcess.ExportArray(RS.points, "rs.csv");                                      
                    if (cd.correlatingDimension != ChaosLogic.ERR_CORR_DIMENSION)
                    {
                        DataProcess.ExportArray(cd.points, 2, "cd.csv");                                                                        
                        report += "\nstep: " + cd.step.ToString();
                        report += "\nminR: " + cd.minR.ToString();
                        report += "\nmaxR: " + cd.maxR.ToString();
                        report += "\nСтартовая размерность: " + cd.startDimension.ToString();
                        report += "\nКонечная размерность: " + cd.endDimension.ToString();
                        report += "\nПоследняя размерность вложения: " + cd.lastDimension.ToString();
                        report += "\nРазмерность вложения для максимальной корр. размерности: " + cd.maxCorrDimension.ToString();
                        report += "\nГлубина вложения траектории без самопересечения: " + cd.dimension_zero.ToString();
                        report += "\nОценка снизу колмогоровской энтропии: " + cd.entropyK.ToString();
                        if (cd.errorCode != 0) report += "\nОшибка: недостаточно данных для оценки размерности.";
                    }
                    else
                    {
                        report = "Корреляционная размерность не определена.";
                    }
                    report += "\nПоказатель Херста: " + RS.regression.A.ToString() + " + " + RS.regression.B.ToString();
                    break;                 
                //RS-анализ, вычисление показателя Херста (фрактальной размерности)
                case "rs":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);
                    /*
                    float[] Rev = new float[C.Length];

                    for (int t = 0; t < C.Length; t++)
                    {
                        Rev[t] = C[C.Length - t - 1];
                    }
                    C = Rev;
                     */ 
                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();

                    FractalDimParams paramRS = (FractalDimParams)param;
                    
                    RS = ChaosLogic.RS_Analyse(C, 3, 0, C.Length - 1, C.Length, threadsCount);
                    DataProcess.ExportArray(RS.points, "rs.csv");
                    DataProcess.ExportArray(C, "_orig_file.csv");
                    report += "\nПоказатель Херста: " + RS.regression.A.ToString() + " + " + RS.regression.B.ToString();

                    //float[] D = DisorderSignal.aposteriorBrodskyDarhovskyOverall(C, 1);
                    //DataProcess.ExportArray(D, "_disorder.csv");

                    /*
                    float[] Rev = new float[C.Length];

                    for (int t = 0; t < C.Length; t++)
                    {
                        Rev[t] = C[C.Length - t - 1];
                    }
                    C = Rev;

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();
                    
                    RS = ChaosLogic.RS_Analyse(C, 3, 0, C.Length - 1, C.Length, threadsCount);
                    DataProcess.ExportArray(RS.points, "rs_res.csv");
                    DataProcess.ExportArray(C, "_orig_file_res.csv");
                    report += "\nПоказатель Херста: " + RS.regression.A.ToString() + " + " + RS.regression.B.ToString();

                    //D = DisorderSignal.aposteriorBrodskyDarhovskyOverall(C, 1);
                    //DataProcess.ExportArray(D, "_disorder_res.csv");
                    */
                    break; 
                //AutoMutual Info
                case "ami":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();

                    AutoMutualInfoParams paramAUI = (AutoMutualInfoParams)param;
                    AutoMutualInfoResult res;
                    if (paramAUI.endDelay == 0)
                    {
                        paramAUI.endDelay = C.Length - 1;
                    }
                    res = ChaosLogic.AutoMutualInformation(C, paramAUI.startDelay, paramAUI.endDelay, paramAUI.levelsCount);                    
                    report += "\nОптимальный лаг: " + res.optimalLag.ToString();
                    DataProcess.ExportArray(res.AMI, "_ami.csv");
                    break;
                //Lyapunov
                case "lyapunov":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();

                    LyapunovParams paramLyap = (LyapunovParams)param;
                    float lyapunov = ChaosLogic.GetMaxLyapunovExponent(C, paramLyap.embedingDimension, paramLyap.lag, paramLyap.count);                    
                    report += "\nОценка максимального показателя Ляпунова: " + lyapunov.ToString();
                    break;
                //Оценка индекса вариации
                case "varind":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();

                    VariationIndParams paramVarInd = (VariationIndParams) param;
                    int endIndex = (paramVarInd.endIndex > 0) ? paramVarInd.endIndex : C.Length - 1;
                    float[] vi = ChaosLogic.VariationIndex(C, paramVarInd.startIndex, endIndex, paramVarInd.startSegmentLength, paramVarInd.endSegmentLength, paramVarInd.windowLength, paramVarInd.numPointsRegression);
                    DataProcess.ExportArray(vi, "_vig.csv");
                    double sum = 0;
                    for (int i = 0; i < vi.Length; i++) sum += vi[i];
                    sum /= vi.Length;
                    report += "\nУсредненный индекс вариации по окнам: " + sum.ToString();
                    int len = endIndex - paramVarInd.startIndex + 1;
                    //Индекс вариации рассчитанный для всего массива
                    //vi = ChaosLogic.VariationIndex(C, paramVarInd.startIndex, endIndex, paramVarInd.startSegmentLength * 3, (int)(len / 2), len,paramVarInd.numPointsRegression);
                    //report += "\nИндекс вариации: " + vi[0].ToString();
                    break;
                case "mlp":                    
                    if (dataParam.isDoClearFile) errors = DataProcess.ClearFile(dataParam.fileName);
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    report += "\nДлина обрабатываемого ряда: " + C.Length.ToString();
                    report += "\nДлина исходного ряда: " + (C.Length * discharging).ToString();
                    report += "\nШаг выборки: " + discharging.ToString();

                    PerseptronParams paramMLP = (PerseptronParams)param;
                    string[][] functionsActivation = new string[3][];
                    for (int i = 0; i < paramMLP.numLayers; i++)
                    {
                        functionsActivation[i] = new string [paramMLP.numNeuronsInLayer[i]];
                    }
                    int learningLength = C.Length * paramMLP.learningSet / 100;
                    int signalLen = learningLength - paramMLP.predictionStep;
                    float[,] signal = new float[signalLen, 1];
                    float[,] desiredSignal = new float[signalLen, 1];
                    for (int i = 0; i < signalLen; i++)
                    {
                        /*
                        for (int j = 0; j < paramMLP.numNeuronsInLayer[paramMLP.numLayers - 1]; j++)
                        {
                            signal[i, j] = C[i];
                            desiredSignal[i, j] = C[i + paramMLP.predictionStep];
                        }
                         */
                        signal[i, 0] = C[i];
                        desiredSignal[i, 0] = C[i + paramMLP.predictionStep];
                    }
                    //vi = ChaosLogic.VariationIndex(C, 0, C.Length-1, 25, 100, 500,5);
                    //DataProcess.ExportArray(vi, "_vig.csv");
                    //signal = DataProcess.MovingAverage(signal, 12);
                    //desiredSignal = DataProcess.MovingAverage(desiredSignal, 12);
                    float upperBound = 0.99999F;
                    float lowerBound = -0.99999F;
                    NormalizationByBoundResult signalData = DataProcess.NormalizationByBound(signal, lowerBound, upperBound);
                    NormalizationByBoundResult desiredSignalData = DataProcess.NormalizationByBound(desiredSignal, lowerBound, upperBound);
                    signal = signalData.A;
                    desiredSignal = desiredSignalData.A;

                    PointPairList list = new PointPairList();
                    int lengthList = signalData.A.Length;
                    for (int i = 0; i < lengthList; i++)
                    {
                        list.Add(i, signalData.A[i,0]);
                    }
                    //ShowGraph("Test", list);
                    DataProcess.ExportArray(signal, "_signal.csv");
                    DataProcess.ExportArray(desiredSignal, "_desiredSignal.csv");
                    MultilayerPerseptron mlp = new MultilayerPerseptron(paramMLP.numInputSignals, paramMLP.numLayers, paramMLP.numNeuronsInLayer, functionsActivation, signal, desiredSignal, paramMLP.delayInput);
                    paramMLP.numEpochs = 4000;
                    mlp.train(paramMLP.numEpochs, learningLength, signalData.amplituda, signalData.offset);
                    int testLength = C.Length - learningLength;
                    float[,] testSignal = new float[testLength - paramMLP.predictionStep, 1];
                    float[,] testDesiredSignal = new float[testLength - paramMLP.predictionStep, 1];
                    for (int i = 0; i < testLength - paramMLP.predictionStep; i++)
                    {
                        testSignal[i, 0] = C[i + learningLength];
                        testDesiredSignal[i, 0] = C[i + learningLength + paramMLP.predictionStep];
                    }
                    
                    mlp.upperBound = upperBound;
                    mlp.lowerBound = lowerBound;
                    NormalizationByBoundResult testSignalData = DataProcess.NormalizationByBound(testSignal, lowerBound, upperBound);
                    NormalizationByBoundResult testDesiredSignalData = DataProcess.NormalizationByBound(testDesiredSignal, lowerBound, upperBound);

                    //DataProcess.ExportArray(testSignalData.A, "_testSignal.csv");
                    //DataProcess.ExportArray(testDesiredSignalData.A, "_testDesiredSignal.csv");

                    //DataProcess.SaveObject(testSignalData.A, "_testSignal.obj");
                    //testSignalData.A = (float[,])DataProcess.LoadObject("_testSignal.obj");

                    //DataProcess.SaveObject(testDesiredSignalData.A, "_testDesiredSignal.obj");
                    //testDesiredSignalData.A = (float[,])DataProcess.LoadObject("_testDesiredSignal.obj");

                    //report += mlp.recursivePrediction(5, signalData.amplituda, signalData.offset);
                    //DataProcess.ExportArray3(testResult, "_test.csv");
                    
                    float[, ,] testResult2 = mlp.test(testSignalData.A, testDesiredSignalData.A, desiredSignalData.offset, desiredSignalData.amplituda);
                    DataProcess.ExportArray3(testResult2, "_test2.csv");
                    break;
                case "data_process":
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);

                    //A = DataProcess.LoadDataFullTimePeriod(dataParam.fileName, 2, dataParam.timePeriod, dataParam.delimiter, dataParam.format, ref periodStr);
                    //NormalizationByBoundResultOneChannel rs = DataProcess.NormalizationByBound(C, -1f, 1f);
                    //DataProcess.ExportArray(rs.A,"_norm.csv");
                    break;
                case "trade":
                    {
                        report += Trade(newGraphForm);
                    }
                    break;
                case "classification":
                                        
                    //DataProcess.ClearFile(dataParam.fileName);
                    
                    if (dataParam.timePeriod != null)
                    {                        
                        //A = DataProcess.LoadDataFullTimePeriod(dataParam.fileName, 2, dataParam.timePeriod, dataParam.delimiter, dataParam.format, ref periodStr);
                        A = (Point[])DataProcess.LoadObject(dataParam.fileName);

                        LocalExtremumStatistic[] D = DataProcess.GetLocalExtremumStatisticWithoutRecoil(A, 0, A.Length-1, 0.05f, 1.5f, 0.15f, 0.2f, 0.05f, 1f);
                        DataProcess.ExportArray(D, "_stat.csv");

                        B = DataProcess.GetThresholdStatisticDisruption(A, 0, A.Length - 1, 0.35f, 0.1f);
                        DataProcess.ExportArray(B, A, "_statistic.csv");
                        
                        PointClassification[] points = DataProcess.convertDisruptionPointToPointClassification2(B, A);                        
                        //DataProcess.ExportArray(points, "_points.csv");
                        
                        int startTrainInd = 5;
                        int endTrainInd = points.Length - 20;
                        //report += Classification.KNNwithTimeR(points, startTrainInd, endTrainInd, endTrainInd + 1, points.Length - 1, "_class_pt.csv");
                        
                        /*
                        points = DataProcess.convertDisruptionPointToPointClassification(B);
                        DataProcess.ExportArray(points, "_points2.csv");
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_pt.csv");
                    
                        points = DataProcess.convertDisruptionPointToPointClassification3(B);
                        DataProcess.ExportArray(points, "_points3.csv");
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_up-uv.csv");
                    
                        PointClassification[] points = DataProcess.convertDisruptionPointToPointClassification4(B);
                        DataProcess.ExportArray(points, "_points4.csv");
                     
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_full.csv");
                        */


                        //Отрисовывем графики
                        
                        listGraphs listGraphics = new listGraphs();
                        listGraphics.myGraphPane = new GraphPane();

                        int fontSize = 5;
                        listGraphics.myGraphPane.YAxis.Title.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.YAxis.Scale.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.XAxis.Scale.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.XAxis.Title.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.Title.FontSpec.Size = fontSize;
                        listGraphics.list = new PointPairList[5];
                        listGraphics.curves = new LineItem[5];

                        PointPairList listGraph = new PointPairList();
                        PointPairList listGraphExtremum = new PointPairList();
                        PointPairList listGraphTrue = new PointPairList();
                        PointPairList listGraphFalse = new PointPairList();
                        PointPairList listGraphThreshold = new PointPairList();

                        for (int i = 0; i < B.Length; i++)
                        {
                            if (B[i].type == 1)
                            {
                                listGraphTrue.Add(B[i].indConfirmativeThreshold, A[B[i].indConfirmativeThreshold].val);
                                listGraphExtremum.Add(B[i].indExtremum, A[B[i].indExtremum].val);
                                listGraphThreshold.Add(B[i].indThreshold, A[B[i].indThreshold].val);
                            }
                            else
                            {
                                listGraphFalse.Add(B[i].indConfirmativeThreshold, A[B[i].indConfirmativeThreshold].val);
                            }
                        }

                        LineItem curve = new LineItem("Основные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Gray;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Empty);
                        curve.Symbol.Size = 6;
                        listGraphics.curves[0] = curve;
                        listGraphics.list[0] = listGraphThreshold;

                        curve = new LineItem("Ложные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Black;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Black);
                        curve.Symbol.Size = 4;
                        listGraphics.curves[1] = curve;
                        listGraphics.list[1] = listGraphFalse;

                        curve = new LineItem("Истинные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Green;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Green);
                        curve.Symbol.Size = 5;
                        listGraphics.curves[2] = curve;
                        listGraphics.list[2] = listGraphTrue;

                        curve = new LineItem("Экстремумы");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Blue;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Blue);
                        curve.Symbol.Size = 5;
                        listGraphics.curves[3] = curve;
                        listGraphics.list[3] = listGraphExtremum;

                        //Исходный сигнал                    
                        listGraph = new PointPairList();
                        int amountPoints = A.Length;
                        for (int i = 0; i < amountPoints; i++)
                        {
                            listGraph.Add(i, A[i].val);
                        }

                        curve = new LineItem("Исходный сигнал");
                        curve.Line.IsVisible = true;
                        curve.Line.Width = 1f;
                        curve.Color = Color.Red;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Red);
                        curve.Symbol.Size = 2;
                        listGraphics.curves[4] = curve;
                        listGraphics.list[4] = listGraph;

                        ShowGraph("test", listGraphics, graphForm);

                        //классы
                        listGraphics.myGraphPane = new GraphPane();
                        listGraphics.list = new PointPairList[2];
                        listGraphics.curves = new LineItem[2];

                        listGraphTrue = new PointPairList();
                        listGraphFalse = new PointPairList();
                        for (int i = 10; i < points.Length; i++)
                        {
                            if (points[i].classNum == 1)
                            {
                                listGraphTrue.Add(points[i].coord[0], points[i].coord[1]);                                
                            }
                            else
                            {
                                listGraphFalse.Add(points[i].coord[0], points[i].coord[1]);
                            }
                        }
                        PointPair[] trueArr = listGraphTrue.ToArray();
                        PointPair[] falseArr = listGraphFalse.ToArray();

                        curve = new LineItem("Ложные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Black;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Black);
                        //curve.Symbol.Fill.Type = FillType.GradientByZ;
                        //curve.Symbol.Fill.RangeMin = falseArr[0].Z;
                        //curve.Symbol.Fill.RangeMax = falseArr[falseArr.Length-1].Z;
                        curve.Symbol.Size = 2;
                        curve.Symbol.Border.IsVisible = false;
                        listGraphics.curves[0] = curve;
                        listGraphics.list[0] = listGraphFalse;

                        curve = new LineItem("Истинные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Green;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Green);
                        //curve.Symbol.Fill.Type = FillType.GradientByZ;
                        //curve.Symbol.Fill.RangeMin = trueArr[0].Z;
                        //curve.Symbol.Fill.RangeMax = trueArr[trueArr.Length-1].Z;
                        curve.Symbol.Size = 2;
                        curve.Symbol.Border.IsVisible = false;
                        listGraphics.curves[1] = curve;
                        listGraphics.list[1] = listGraphTrue;

                        ShowGraph("test", listGraphics, graphForm2);
                    }
                    //просто по столбцу данных
                    else
                    {
                        C = LoadArraySimple(dataParam, ref discharging);
                        B = DataProcess.GetThresholdStatisticDisruption(C, 0, C.Length - 1, 15f, 5f);
                        DataProcess.ExportArray(B, C, "_statistic.csv");
                        PointClassificationSimple[] points = DataProcess.convertDisruptionPointToPointClassification3(B, C);
                        DataProcess.ExportArray(points, "_pointsF.csv");

                        int startTrainInd = 5;
                        int endTrainInd = points.Length - 20;
                        //report += Classification.KNNSimple(points, startTrainInd, endTrainInd, endTrainInd + 1, points.Length - 1, "_class_pt.csv");

                        /*
                        points = DataProcess.convertDisruptionPointToPointClassification(B);
                        DataProcess.ExportArray(points, "_points2.csv");
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_pt.csv");
                    
                        points = DataProcess.convertDisruptionPointToPointClassification3(B);
                        DataProcess.ExportArray(points, "_points3.csv");
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_up-uv.csv");
                    
                        PointClassification[] points = DataProcess.convertDisruptionPointToPointClassification4(B);
                        DataProcess.ExportArray(points, "_points4.csv");
                     
                        report += Classification.KNNwithTimeR(points, 5, points.Length - 300, points.Length - 299, points.Length - 1, "_class_full.csv");
                        */


                        //Отрисовывем графики
                        listGraphs listGraphics = new listGraphs();
                        listGraphics.myGraphPane = new GraphPane();

                        int fontSize = 5;
                        listGraphics.myGraphPane.YAxis.Title.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.YAxis.Scale.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.XAxis.Scale.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.XAxis.Title.FontSpec.Size = fontSize;
                        listGraphics.myGraphPane.Title.FontSpec.Size = fontSize;
                        listGraphics.list = new PointPairList[5];
                        listGraphics.curves = new LineItem[5];

                        PointPairList listGraph = new PointPairList();
                        PointPairList listGraphExtremum = new PointPairList();
                        PointPairList listGraphTrue = new PointPairList();
                        PointPairList listGraphFalse = new PointPairList();
                        PointPairList listGraphThreshold = new PointPairList();

                        for (int i = 0; i < B.Length; i++)
                        {
                            if (B[i].type == 1)
                            {
                                listGraphTrue.Add(B[i].indConfirmativeThreshold, C[B[i].indConfirmativeThreshold]);
                                listGraphExtremum.Add(B[i].indExtremum, C[B[i].indExtremum]);
                                listGraphThreshold.Add(B[i].indThreshold, C[B[i].indThreshold]);
                            }
                            else
                            {
                                listGraphFalse.Add(B[i].indConfirmativeThreshold, C[B[i].indConfirmativeThreshold]);
                            }
                        }

                        LineItem curve = new LineItem("Основные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Gray;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Empty);
                        curve.Symbol.Size = 6;
                        listGraphics.curves[0] = curve;
                        listGraphics.list[0] = listGraphThreshold;

                        curve = new LineItem("Ложные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Black;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Black);
                        curve.Symbol.Size = 3;
                        listGraphics.curves[1] = curve;
                        listGraphics.list[1] = listGraphFalse;

                        curve = new LineItem("Истинные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Green;
                        curve.Symbol.Type = SymbolType.Diamond;
                        curve.Symbol.Fill = new Fill(Color.Green);
                        curve.Symbol.Size = 4;
                        listGraphics.curves[2] = curve;
                        listGraphics.list[2] = listGraphTrue;

                        curve = new LineItem("Экстремумы");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Blue;
                        curve.Symbol.Type = SymbolType.TriangleDown;
                        curve.Symbol.Fill = new Fill(Color.Empty);
                        curve.Symbol.Size = 6;
                        listGraphics.curves[3] = curve;
                        listGraphics.list[3] = listGraphExtremum;

                        //Исходный сигнал                    
                        listGraph = new PointPairList();
                        int amountPoints = C.Length;
                        for (int i = 0; i < amountPoints; i++)
                        {
                            listGraph.Add(i, C[i]);
                        }

                        curve = new LineItem("Исходный сигнал");
                        curve.Line.IsVisible = true;
                        curve.Line.Width = 1f;
                        curve.Color = Color.Red;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Red);
                        curve.Symbol.Size = 2;
                        listGraphics.curves[4] = curve;
                        listGraphics.list[4] = listGraph;

                        ShowGraph("test", listGraphics, graphForm);

                        //классы
                        listGraphics.myGraphPane = new GraphPane();
                        listGraphics.list = new PointPairList[2];
                        listGraphics.curves = new LineItem[2];

                        listGraphTrue = new PointPairList();
                        listGraphFalse = new PointPairList();
                        for (int i = 10; i < points.Length; i++)
                        {
                            if (points[i].classNum == 1)
                            {
                                listGraphTrue.Add(points[i].coord[0], points[i].coord[1]);
                            }
                            else
                            {
                                listGraphFalse.Add(points[i].coord[0], points[i].coord[1]);
                            }
                        }
                        PointPair[] trueArr = listGraphTrue.ToArray();
                        PointPair[] falseArr = listGraphFalse.ToArray();

                        curve = new LineItem("Ложные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Black;
                        curve.Symbol.Type = SymbolType.Circle;
                        curve.Symbol.Fill = new Fill(Color.Black);
                        //curve.Symbol.Fill.Type = FillType.GradientByZ;
                        //curve.Symbol.Fill.RangeMin = falseArr[0].Z;
                        //curve.Symbol.Fill.RangeMax = falseArr[falseArr.Length-1].Z;
                        curve.Symbol.Size = 2;
                        curve.Symbol.Border.IsVisible = false;
                        listGraphics.curves[0] = curve;
                        listGraphics.list[0] = listGraphFalse;

                        curve = new LineItem("Истинные пороги");
                        curve.Line.IsVisible = false;
                        curve.Color = Color.Green;
                        curve.Symbol.Type = SymbolType.Diamond;
                        curve.Symbol.Fill = new Fill(Color.Green);
                        //curve.Symbol.Fill.Type = FillType.GradientByZ;
                        //curve.Symbol.Fill.RangeMin = trueArr[0].Z;
                        //curve.Symbol.Fill.RangeMax = trueArr[trueArr.Length-1].Z;
                        curve.Symbol.Size = 2;
                        curve.Symbol.Border.IsVisible = false;
                        listGraphics.curves[1] = curve;
                        listGraphics.list[1] = listGraphTrue;

                        ShowGraph("test", listGraphics, graphForm2);
                    }                    
                                        
                    break;
                case "disorder_brodsky_darhovsky_overall":
                    if (dataParam.timePeriod != null)
                    {
                        C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        //Экспортируем в файл _time_filename значения с датой
                    }
                    //просто по столбцу данных
                    else C = LoadArraySimple(dataParam, ref discharging);
                    
                    float v = (float)param;
                    //float[] disorder = DisorderSignal.aposteriorBrodskyDarhovskyOverall(C,v);                    
                    //DataProcess.ExportArray(disorder, "_disorder.csv");
                    break;
                case "disorder_vasilchenko":
                    C = LoadArraySimple(dataParam, ref discharging);
                    int[] E = DisorderSignal.aposteriorVasilchenko(C, 0.0001f, 0.8f, 0.0001f);
                    DataProcess.ExportArray(E, "_disorder_vasilchenko.csv");
                    break;

                case "disorder_segena_sanderson":
                    C = LoadArraySimple(dataParam, ref discharging);
                    float[] ASEG = DisorderSignal.successiveSegenaSanderson(C, 10);
                    DataProcess.ExportArray(ASEG, "_disorder_segena_sanderson.csv");
                    break;                
                case "cusum":

                    Point[] cusumPrice = (Point[])DataProcess.LoadObject(@"sec\GAZP_1.bin");
                    DisorderResult resCUSUM = DisorderSignal.CUSUM(cusumPrice,0,30000);
                    DataProcess.ExportArray(resCUSUM.A, "_cusum.csv");
                    report += "\n\nCUSUM: indexMax=" + resCUSUM.indMax + " (" + resCUSUM.A[resCUSUM.indMax] + ")";

                    /*
                    C = LoadArraySimple(dataParam, ref discharging);                    
                    DisorderResult resCUSUM = DisorderSignal.CUSUM(C);
                    DataProcess.ExportArray(resCUSUM.A, "_cusum.csv");
                    report += "\n\nCUSUM: indexMax="+resCUSUM.indMax+" ("+resCUSUM.A[resCUSUM.indMax]+")";

                    DisorderResult MinError = DisorderSignal.minInfoError(C);
                    DataProcess.ExportArray(MinError.A, "_MINERROR.csv");
                    report += "\n\nMinError: indexMax=" + MinError.indMax + " (" + MinError.A[MinError.indMax] + ")";

                    DisorderResult disorder = DisorderSignal.aposteriorBrodskyDarhovskyOverall(C, 1);
                    DataProcess.ExportArray(disorder.A, "_disorder.csv");
                    report += "\n\ndarhovsky: indexMax=" + disorder.indMax + " (" + disorder.A[disorder.indMax] + ")";
                    
                    int endIndex1 = C.Length - 1;
                    float[] vi1 = ChaosLogic.VariationIndex(C, 0, endIndex1, 5, 30, 60, 5);
                    DataProcess.ExportArray(vi1, "_vig.csv");


                    DisorderResult resCUSUM1 = DisorderSignal.CUSUM(vi1);
                    DataProcess.ExportArray(resCUSUM1.A, "_cusum_vi.csv");
                    report += "\n\nCUSUM(vi): indexMax=" + resCUSUM1.indMax + " (" + resCUSUM1.A[resCUSUM1.indMax] + ")";

                    DisorderResult MinError1 = DisorderSignal.minInfoError(vi1);
                    DataProcess.ExportArray(MinError1.A, "_MINERROR_vi.csv");
                    report += "\n\nMinError (vi): indexMax=" + MinError1.indMax + " (" + MinError1.A[MinError1.indMax] + ")";

                    DisorderResult disorder1 = DisorderSignal.aposteriorBrodskyDarhovskyOverall(vi1, 1);
                    DataProcess.ExportArray(disorder1.A, "_disorder_vi.csv");
                    report += "\n\ndarhovsky(vi): indexMax=" + disorder1.indMax + " (" + disorder1.A[disorder1.indMax] + ")";
                    */


                    //float[] Shiryaev = DisorderSignal.GirshikRubinShiryaev(C, 5f);
                    //float[] CUSUMRS = DisorderSignal.CUSUM_RS(C);
                    //float[] CUSUM_G = DisorderSignal.CUSUM_Giraitis(C);                    
                    //DataProcess.ExportArray(Shiryaev, "_shiryaev.csv");
                    //DataProcess.ExportArray(CUSUM_G, "_cusum_g.csv");
                    break;
                case "cusum_generate":
                    //C = LoadArraySimple(dataParam, ref discharging);                    
                    int lengthSeries = 500;
                    C = new float[lengthSeries];
                    int countTest = 100;
                    float[,] table = new float[countTest, 6];
                    int point_disorder = 300;
                    //float[] randnums = new float[2500];

                    int[] cusum_err_count = new int[countTest];
                    int[] max_err_count = new int[countTest];
                    int[] avr_err_count = new int[countTest];
                    int[] minimax_err_count = new int[countTest];

                    float cusum_sum0 = 0;
                    float cusum_sum1 = 0;
                    float max_sum0 = 0;
                    float max_sum1 = 0;
                    float avr_sum0 = 0;
                    float avr_sum1 = 0;
                    float minimax_sum0 = 0;
                    float minimax_sum1 = 0;
                    for (int j = 0; j < countTest; j++)
                    {
                        float x = 0.2f;
                        Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
                        for (int i = 0; i < point_disorder; i++)
                        {
                            float rand1 = (float)rand.NextDouble();
                            float rand2 = (float)rand.NextDouble();
                            //float rand3 = (float)rand.NextDouble();
                            //float rand4 = (float)rand.NextDouble();
                            //float rand5 = (float)rand.NextDouble();
                            //float rand6 = (float)rand.NextDouble();
                            
                            C[i] = x * 0.9f + 0.1f + (rand1 - rand2) * 0.6f;                                                        
                            //C[i] = x * 0.8f + 0.1f + (rand1 - rand2) * (rand3 - rand4) * 0.5f;
                            x = C[i];
                            //randnums[r] = rand1 - rand2;                            
                        }
                        for (int i = point_disorder; i < lengthSeries; i++)
                        {
                            float rand1 = (float)rand.NextDouble();
                            float rand2 = (float)rand.NextDouble();
                            //float rand3 = (float)rand.NextDouble();
                            //float rand4 = (float)rand.NextDouble();
                            //float rand5 = (float)rand.NextDouble();
                            //float rand6 = (float)rand.NextDouble();

                            C[i] = x * 0.9f + 0.2f + (rand1 - rand2)* 0.6f;
                            x = C[i];
                        }
                        //float[] D = DataProcess.GetDifference(C);
                        float[] D = C;                        
                        //C = DataProcess.reverseArray(C);
                        DisorderResult resCUSUMg = DisorderSignal.CUSUM(D);
                        
                        //DataProcess.ExportArray(resCUSUM.A, "_cusum.csv");
                        //report += "\n\nCUSUM: indexMax=" + resCUSUM.indMax + " (" + resCUSUM.A[resCUSUM.indMax] + ")";
                        /*
                        DisorderResult MinErrorg = DisorderSignal.minInfoError(D);
                        
                        //DataProcess.ExportArray(MinError.A, "_MINERROR.csv");
                        //report += "\n\nMinError: indexMax=" + MinError.indMax + " (" + MinError.A[MinError.indMax] + ")";

                        DisorderResult disorderg = DisorderSignal.aposteriorBrodskyDarhovskyOverall(D, 1);
                        */
                        //DataProcess.ExportArray(disorder.A, "_disorder.csv");
                        //report += "\n\ndarhovsky: indexMax=" + disorder.indMax + " (" + disorder.A[disorder.indMax] + ")";

                        int endIndex1g = D.Length - 1;
                        float[] vi1g = ChaosLogic.VariationIndex(D, 0, endIndex1g, 5, 30, 60, 5);
                        //DataProcess.ExportArray(vi1, "_vig.csv");


                        DisorderResult resCUSUM1g = DisorderSignal.CUSUM(vi1g);
                        resCUSUM1g.indMax += 60;
                        //DataProcess.ExportArray(resCUSUM1.A, "_cusum_vi.csv");
                        //report += "\n\nCUSUM(vi): indexMax=" + resCUSUM1.indMax + " (" + resCUSUM1.A[resCUSUM1.indMax] + ")";
                        
                        /*
                        DisorderResult MinError1g = DisorderSignal.minInfoError(vi1g);
                        MinError1g.indMax += 60;
                        //DataProcess.ExportArray(MinError1.A, "_MINERROR_vi.csv");
                        //report += "\n\nMinError (vi): indexMax=" + MinError1.indMax + " (" + MinError1.A[MinError1.indMax] + ")";

                        DisorderResult disorder1g = DisorderSignal.aposteriorBrodskyDarhovskyOverall(vi1g, 1);
                        disorder1g.indMax += 60;
                         */ 
                        //DataProcess.ExportArray(disorder1.A, "_disorder_vi.csv");
                        //report += "\n\ndarhovsky(vi): indexMax=" + disorder1.indMax + " (" + disorder1.A[disorder1.indMax] + ")";
                        int column_num = 0;
                        table[j, column_num] = resCUSUMg.indMax;
                        column_num++;
                        float cusum_err = (float)(point_disorder - resCUSUMg.indMax) / (float)lengthSeries;
                        float sign_cusum_err = Math.Sign(cusum_err);
                        if (sign_cusum_err > 0)
                        {
                            cusum_err_count[0]++;
                            cusum_sum0 += cusum_err;
                        }
                        else
                        {
                            cusum_err_count[1]++;
                            cusum_sum1 += cusum_err;
                        }
                        table[j, column_num] = cusum_err;
                        column_num++;
                        /*
                        table[j, column_num] = MinErrorg.indMax;
                        column_num++;
                        float minError_err = (float)(point_disorder - MinErrorg.indMax) / (float)lengthSeries;
                        table[j, column_num] = Math.Abs(minError_err);                        
                        column_num++;

                        table[j, column_num] = disorderg.indMax;
                        column_num++;
                        float disorder_err = (float)(point_disorder - disorderg.indMax) / (float)lengthSeries;
                        table[j, column_num] = Math.Abs(disorder_err);
                        column_num++;
                         */ 

                        table[j, column_num] = resCUSUM1g.indMax;
                        column_num++;
                        float max_ind_estimation = (resCUSUM1g.indMax > resCUSUMg.indMax) ? resCUSUM1g.indMax : resCUSUMg.indMax;
                        float max_err = (float)(point_disorder - max_ind_estimation) / (float)lengthSeries;
                        table[j, column_num] = max_err;
                        float sign_max_err = Math.Sign(max_err);
                        if (sign_max_err > 0)
                        {
                            max_err_count[0]++;
                            max_sum0 += max_err;
                        }
                        else
                        {
                            max_err_count[1]++;
                            max_sum1 += max_err;
                        }
                        column_num++;

                        float avr = (resCUSUM1g.indMax + resCUSUMg.indMax) / 2f;
                        float avr_err = (float)(point_disorder - avr) / (float)lengthSeries;
                        table[j, column_num] = avr_err;

                        float sign_avr_err = Math.Sign(avr_err);
                        if (sign_avr_err > 0)
                        {
                            avr_err_count[0]++;
                            avr_sum0 += avr_err;
                        }
                        else
                        {
                            avr_err_count[1]++;
                            avr_sum1 += avr_err;
                        }
                        column_num++;


                        float minimax_err = (float)(point_disorder - (avr + max_ind_estimation) / 2f) / (float)lengthSeries;
                        table[j, column_num] = minimax_err;

                        float sign_minimax_err = Math.Sign(minimax_err);
                        if (sign_minimax_err > 0)
                        {
                            minimax_err_count[0]++;
                            minimax_sum0 += minimax_err;
                        }
                        else
                        {
                            minimax_err_count[1]++;
                            minimax_sum1 += minimax_err;
                        }
                        column_num++;

                        /*
                        table[j, column_num] = MinError1g.indMax;
                        column_num++;
                        table[j, column_num] = (float)(point_disorder - MinError1g.indMax) / (float)lengthSeries;
                        
                        column_num++;
                        table[j, column_num] = (table[j, column_num-1] + minError_err) / 2f;                        
                        table[j, column_num] = Math.Abs(table[j, column_num]);
                        table[j, column_num-1] = Math.Abs(table[j, column_num - 1]);
                        column_num++;

                        table[j, column_num] = disorder1g.indMax;
                        column_num++;
                        table[j, column_num] = (float)(point_disorder - disorder1g.indMax) / (float)lengthSeries;
                        column_num++;
                        table[j, column_num] = (table[j, column_num-1] + disorder_err) / 2f;                        
                        table[j, column_num] = Math.Abs(table[j, column_num]);
                        table[j, column_num-1] = Math.Abs(table[j, column_num - 1]);
                         */

                        if (j == countTest - 1)
                        {
                            DataProcess.ExportArray(D, "_D.csv");
                            DataProcess.ExportArray(vi1g, "_vi1g.csv");
                        }
                        /*
                        if ((fa > 0.05) && (fa<0.08))
                        {
                            DataProcess.ExportArray(D, "_D.csv");
                            DataProcess.ExportArray(vi1g, "_vi1g.csv");
                            report += "\n\nCUSUM: indexMax=" + resCUSUMg.indMax.ToString() + " (" + fa.ToString() + ")";
                            report += "\n\nCUSUM(vi): indexMax=" + resCUSUM1g.indMax.ToString() + " (" + faerr.ToString() + ")";
                        }
                         */ 
                    }
                    report += "\n\ncusum=" + cusum_err_count[0] + "-" + cusum_err_count[1] + " " + cusum_sum0 / (float)cusum_err_count[0] + " " + cusum_sum1 / (float)cusum_err_count[1];
                    report += "\n\nmax=" + max_err_count[0] + "-" + max_err_count[1] + " " + max_sum0 / (float)max_err_count[0] + " " + max_sum1 / (float)max_err_count[1];
                    report += "\n\navr=" + avr_err_count[0] + "-" + avr_err_count[1] + " " + avr_sum0 / (float)avr_err_count[0] + " " + avr_sum1 / (float)avr_err_count[1];
                    report += "\n\nminimax=" + minimax_err_count[0] + "-" + minimax_err_count[1] + " " + minimax_sum0 / (float)minimax_err_count[0] + " " + minimax_sum1 / (float)minimax_err_count[1];

                    DataProcess.ExportArray(table,"_table.csv");
                    break;

                case "time_threshold_change":
                    float threshold = (float)param;
                    discharging = 1;
                    if (dataParam.timePeriod != null)
                    {                        
                        //C = LoadArrayByTime(dataParam, ref periodStr, ref discharging);
                        A = DataProcess.LoadDataFullTimePeriod(dataParam.fileName, 2, dataParam.timePeriod, dataParam.delimiter, dataParam.format, ref periodStr);
                        PointTimeThresholdChangeFloat[] T = DataProcess.GetTimeThresholdChangeClear(threshold, A, 0, 0);

                        DataProcess.ExportArray(A, "_A.csv");
                        DataProcess.ExportArray(T, "_T.csv");
                    }
                    else
                    {
                        C = LoadArraySimple(dataParam, ref discharging);
                        PointTimeThresholdChangeSimple[] TS = DataProcess.GetTimeThresholdChangeSimple(threshold, C, 0, 0);                        
                        DataProcess.ExportArray(C, "_C.csv");
                        DataProcess.ExportArray(TS, "_TS.csv");

                        float[] IPS = DataProcess.GetPointsInformation(TS, 0, 0);
                        DataProcess.ExportArray(IPS, "_IPS.csv");
                    }
                    
                    //PointTimeThresholdChangeFloat[] T = DataProcess.GetTimeThresholdChangeClear(threshold, C, 0, 0);                    
                    //NormalizationByBoundResultOneChannel rs = DataProcess.NormalizationByBound(C, -1f, 1f);
                    //DataProcess.ExportArray(rs.A,"_norm.csv");                    

                    break;       
                case "generate_trade_signal":
                    float[] TradeSignal = DisorderSignal.generateTradeSignal(8f, 0f, 0.0016f, 0.575f, 10000);
                    DataProcess.ExportArray(TradeSignal, "_trade_signal.csv");
                    break;

                case "aggregate_to_secs":
                    report += AggregateToSecs(dataParam, param);
                    break;

                case "local_extremum_statistic":
                    report += LocalExtremumStat(dataParam, param);
                    break;
                    
            }            
            
            report += "\n\n\n";
            ShowText(report);
            
            FileStream fOutput = new FileStream("_report.txt", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fOutput);
            //Освобождаем системные ресурсы (пытаемся по крайне мере)
            writer.Write(report);
            writer.Close();
            fOutput.Close();
            //System.GC.Collect(GC.GetGeneration(C));
            System.GC.Collect(GC.GetGeneration(fOutput));
            System.GC.Collect(GC.GetGeneration(writer));
            System.GC.Collect(GC.GetGeneration(report));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            ProcessEnd();

            return report;
        }

        delegate void ShowTextDelegate(string text);
        private void ShowText(string text)
        {
            // Проверяем, не вызывается ли метод из UI-потока
            if (formReport.richTextBox1.InvokeRequired == false)
            {
                formReport.richTextBox1.AppendText(text);
            }
            else
            {
                ShowTextDelegate showText = new ShowTextDelegate(ShowText);
                this.BeginInvoke(showText, new object[] { text });
            }
        }

        delegate void ShowGraphDelegate(string text, listGraphs list, GraphForm graphForm);
        private void ShowGraph(string text, listGraphs list, GraphForm graphForm)
        {
            // Проверяем, не вызывается ли метод из UI-потока
            if (graphForm.zedGraphControl.InvokeRequired == false)
            {
                
                    graphForm.zedGraphControl.GraphPane = list.myGraphPane;
                    graphForm.zedGraphControl.IsScrollY2 = true;
                    graphForm.zedGraphControl.IsShowHScrollBar = true;
                    graphForm.zedGraphControl.IsShowVScrollBar = true;
                    graphForm.zedGraphControl.IsAutoScrollRange = true;
                    graphForm.zedGraphControl.GraphPane.IsBoundedRanges = true;
                    graphForm.zedGraphControl.IsSynchronizeXAxes = true;
                    graphForm.zedGraphControl.IsSynchronizeYAxes = true;
                    graphForm.zedGraphControl.IsEnableVPan = true;
                    graphForm.zedGraphControl.IsEnableVZoom = true;
                    GraphPane myPane = graphForm.zedGraphControl.GraphPane;
                    myPane = list.myGraphPane;

                    LineItem[] curves = new LineItem[list.list.Length];
                    for (int i = 0; i < list.list.Length; i++)
                    {
                        //myPane.CurveList.                    
                        curves[i] = myPane.AddCurve(list.curves[i].Label.Text, list.list[i], list.curves[i].Color, list.curves[i].Symbol.Type);
                        curves[i].Symbol.Size = list.curves[i].Symbol.Size;
                        curves[i].Symbol.Fill = new Fill(list.curves[i].Symbol.Fill);
                        curves[i].Line.Width = list.curves[i].Line.Width;
                        curves[i].Line.IsVisible = list.curves[i].Line.IsVisible;
                    }
                    myPane.YAxis.Title.Text = "Price";
                    myPane.YAxis.Title.FontSpec.Size = 5;
                    myPane.YAxis.IsVisible = true;
                    myPane.YAxis.Scale.FontSpec.FontColor = Color.Black;
                    myPane.YAxis.Scale.IsVisible = true;
                    //myPane.YAxis.Scale.MaxAuto = false;
                    

                    myPane.Y2Axis.Title.Text = "Volume";
                    myPane.Y2Axis.Title.FontSpec.Size = 5;
                    myPane.Y2Axis.IsVisible = true;
                    myPane.Y2Axis.MinorTic.IsOpposite = false;
                    myPane.Y2Axis.MajorTic.IsOpposite = false;
                    myPane.Y2Axis.Scale.FontSpec.FontColor = Color.Black;
                    myPane.Y2Axis.Scale.IsVisible = true;
                    
                    graphForm.zedGraphControl.AxisChange();
                    graphForm.Show();
                

                /*
                ZedGraphControl zgc = graphForm.zedGraphControl;
                // Get a reference to the GraphPane
                GraphPane myPane = zgc.GraphPane;

                // Set the titles and axis labels
                myPane.Title.Text = "Demonstration of Multi Y Graph";
                myPane.XAxis.Title.Text = "Time, s";
                myPane.YAxis.Title.Text = "Velocity, m/s";
                myPane.Y2Axis.Title.Text = "Acceleration, m/s2";

                // Make up some data points based on the Sine function
                PointPairList vList = new PointPairList();
                PointPairList aList = new PointPairList();
                PointPairList dList = new PointPairList();
                PointPairList eList = new PointPairList();

                // Fabricate some data values
                for (int i = 0; i < 30; i++)
                {
                    double time = (double)i;
                    double acceleration = 2.0;
                    double velocity = acceleration * time;
                    double distance = acceleration * time * time / 2.0;
                    double energy = 100.0 * velocity * velocity / 2.0;
                    aList.Add(time, acceleration);
                    vList.Add(time, velocity);
                    eList.Add(time, energy);
                    dList.Add(time, distance);
                }

                // Generate a red curve with diamond symbols, and "Velocity" in the legend
                LineItem myCurve = myPane.AddCurve("Velocity",
                   vList, Color.Red, SymbolType.Diamond);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);

                // Generate a blue curve with circle symbols, and "Acceleration" in the legend
                myCurve = myPane.AddCurve("Acceleration",
                   aList, Color.Blue, SymbolType.Circle);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the Y2 axis
                myCurve.IsY2Axis = true;

                // Generate a green curve with square symbols, and "Distance" in the legend
                myCurve = myPane.AddCurve("Distance",
                   dList, Color.Green, SymbolType.Square);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the second Y axis
                myCurve.YAxisIndex = 1;

                // Generate a Black curve with triangle symbols, and "Energy" in the legend
                myCurve = myPane.AddCurve("Energy",
                   eList, Color.Black, SymbolType.Triangle);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the Y2 axis
                myCurve.IsY2Axis = true;
                // Associate this curve with the second Y2 axis
                myCurve.YAxisIndex = 1;

                // Show the x axis grid
                myPane.XAxis.MajorGrid.IsVisible = true;

                // Make the Y axis scale red
                myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
                myPane.YAxis.Title.FontSpec.FontColor = Color.Red;
                // turn off the opposite tics so the Y tics don't show up on the Y2 axis
                myPane.YAxis.MajorTic.IsOpposite = false;
                myPane.YAxis.MinorTic.IsOpposite = false;
                // Don't display the Y zero line
                myPane.YAxis.MajorGrid.IsZeroLine = false;
                // Align the Y axis labels so they are flush to the axis
                myPane.YAxis.Scale.Align = AlignP.Inside;
                myPane.YAxis.Scale.Max = 100;

                // Enable the Y2 axis display
                myPane.Y2Axis.IsVisible = true;
                // Make the Y2 axis scale blue
                myPane.Y2Axis.Scale.FontSpec.FontColor = Color.Blue;
                myPane.Y2Axis.Title.FontSpec.FontColor = Color.Blue;
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                myPane.Y2Axis.MajorTic.IsOpposite = false;
                myPane.Y2Axis.MinorTic.IsOpposite = false;
                // Display the Y2 axis grid lines
                myPane.Y2Axis.MajorGrid.IsVisible = true;
                // Align the Y2 axis labels so they are flush to the axis
                myPane.Y2Axis.Scale.Align = AlignP.Inside;
                myPane.Y2Axis.Scale.Min = 1.5;
                myPane.Y2Axis.Scale.Max = 3;

                // Create a second Y Axis, green
                YAxis yAxis3 = new YAxis("Distance, m");
                myPane.YAxisList.Add(yAxis3);
                yAxis3.Scale.FontSpec.FontColor = Color.Green;
                yAxis3.Title.FontSpec.FontColor = Color.Green;
                yAxis3.Color = Color.Green;
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                yAxis3.MajorTic.IsInside = false;
                yAxis3.MinorTic.IsInside = false;
                yAxis3.MajorTic.IsOpposite = false;
                yAxis3.MinorTic.IsOpposite = false;
                // Align the Y2 axis labels so they are flush to the axis
                yAxis3.Scale.Align = AlignP.Inside;

                Y2Axis yAxis4 = new Y2Axis("Energy");
                yAxis4.IsVisible = true;
                myPane.Y2AxisList.Add(yAxis4);
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                yAxis4.MajorTic.IsInside = false;
                yAxis4.MinorTic.IsInside = false;
                yAxis4.MajorTic.IsOpposite = false;
                yAxis4.MinorTic.IsOpposite = false;
                // Align the Y2 axis labels so they are flush to the axis
                yAxis4.Scale.Align = AlignP.Inside;
                yAxis4.Type = AxisType.Log;
                yAxis4.Scale.Min = 100;

                // Fill the axis background with a gradient
                myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);

                zgc.AxisChange();
                graphForm.Show();
                 */ 
            }
            else
            {
                ShowGraphDelegate showGraph = new ShowGraphDelegate(ShowGraph);
                this.BeginInvoke(showGraph, new object[] { text, list, graphForm});
            }            
        }

        delegate void ShowGraphPriceDelegate(string text, listGraphs list, GraphForm graphForm, float minPrice, float maxPrice);
        private void ShowGraphPrice(string text, listGraphs list, GraphForm graphForm, float minPrice, float maxPrice)
        {
            // Проверяем, не вызывается ли метод из UI-потока
            if (graphForm.zedGraphControl.InvokeRequired == false)
            {

                graphForm.zedGraphControl.GraphPane = list.myGraphPane;
                graphForm.zedGraphControl.IsScrollY2 = true;
                graphForm.zedGraphControl.IsShowHScrollBar = true;
                graphForm.zedGraphControl.IsShowVScrollBar = true;
                graphForm.zedGraphControl.IsAutoScrollRange = true;
                graphForm.zedGraphControl.GraphPane.IsBoundedRanges = true;
                graphForm.zedGraphControl.IsSynchronizeXAxes = true;
                graphForm.zedGraphControl.IsSynchronizeYAxes = true;
                graphForm.zedGraphControl.IsEnableVPan = true;
                graphForm.zedGraphControl.IsEnableVZoom = true;
                GraphPane myPane = graphForm.zedGraphControl.GraphPane;
                myPane = list.myGraphPane;

                LineItem[] curves = new LineItem[list.list.Length];
                for (int i = 0; i < list.list.Length; i++)
                {
                    //myPane.CurveList.                    
                    curves[i] = myPane.AddCurve(list.curves[i].Label.Text, list.list[i], list.curves[i].Color, list.curves[i].Symbol.Type);
                    curves[i].Symbol.Size = list.curves[i].Symbol.Size;
                    curves[i].Symbol.Fill = new Fill(list.curves[i].Symbol.Fill);
                    curves[i].Line.Width = list.curves[i].Line.Width;
                    curves[i].Line.IsVisible = list.curves[i].Line.IsVisible;
                }
                myPane.YAxis.Title.Text = "Price";
                myPane.YAxis.Title.FontSpec.Size = 5;
                myPane.YAxis.IsVisible = true;
                myPane.YAxis.Scale.FontSpec.FontColor = Color.Black;
                myPane.YAxis.Scale.IsVisible = true;
                //myPane.YAxis.Scale.MaxAuto = false;
                myPane.YAxis.Scale.Min = minPrice;
                myPane.YAxis.Scale.Max = maxPrice;

                /*
                myPane.Y2Axis.Title.Text = "Volume";
                myPane.Y2Axis.Title.FontSpec.Size = 5;
                myPane.Y2Axis.IsVisible = true;
                myPane.Y2Axis.MinorTic.IsOpposite = false;
                myPane.Y2Axis.MajorTic.IsOpposite = false;
                myPane.Y2Axis.Scale.FontSpec.FontColor = Color.Black;
                myPane.Y2Axis.Scale.IsVisible = true;
                myPane.Y2Axis.Scale.Min = minPrice;
                myPane.Y2Axis.Scale.Max = maxPrice;
                 */ 

                graphForm.zedGraphControl.AxisChange();
                graphForm.Show();


                /*
                ZedGraphControl zgc = graphForm.zedGraphControl;
                // Get a reference to the GraphPane
                GraphPane myPane = zgc.GraphPane;

                // Set the titles and axis labels
                myPane.Title.Text = "Demonstration of Multi Y Graph";
                myPane.XAxis.Title.Text = "Time, s";
                myPane.YAxis.Title.Text = "Velocity, m/s";
                myPane.Y2Axis.Title.Text = "Acceleration, m/s2";

                // Make up some data points based on the Sine function
                PointPairList vList = new PointPairList();
                PointPairList aList = new PointPairList();
                PointPairList dList = new PointPairList();
                PointPairList eList = new PointPairList();

                // Fabricate some data values
                for (int i = 0; i < 30; i++)
                {
                    double time = (double)i;
                    double acceleration = 2.0;
                    double velocity = acceleration * time;
                    double distance = acceleration * time * time / 2.0;
                    double energy = 100.0 * velocity * velocity / 2.0;
                    aList.Add(time, acceleration);
                    vList.Add(time, velocity);
                    eList.Add(time, energy);
                    dList.Add(time, distance);
                }

                // Generate a red curve with diamond symbols, and "Velocity" in the legend
                LineItem myCurve = myPane.AddCurve("Velocity",
                   vList, Color.Red, SymbolType.Diamond);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);

                // Generate a blue curve with circle symbols, and "Acceleration" in the legend
                myCurve = myPane.AddCurve("Acceleration",
                   aList, Color.Blue, SymbolType.Circle);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the Y2 axis
                myCurve.IsY2Axis = true;

                // Generate a green curve with square symbols, and "Distance" in the legend
                myCurve = myPane.AddCurve("Distance",
                   dList, Color.Green, SymbolType.Square);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the second Y axis
                myCurve.YAxisIndex = 1;

                // Generate a Black curve with triangle symbols, and "Energy" in the legend
                myCurve = myPane.AddCurve("Energy",
                   eList, Color.Black, SymbolType.Triangle);
                // Fill the symbols with white
                myCurve.Symbol.Fill = new Fill(Color.White);
                // Associate this curve with the Y2 axis
                myCurve.IsY2Axis = true;
                // Associate this curve with the second Y2 axis
                myCurve.YAxisIndex = 1;

                // Show the x axis grid
                myPane.XAxis.MajorGrid.IsVisible = true;

                // Make the Y axis scale red
                myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
                myPane.YAxis.Title.FontSpec.FontColor = Color.Red;
                // turn off the opposite tics so the Y tics don't show up on the Y2 axis
                myPane.YAxis.MajorTic.IsOpposite = false;
                myPane.YAxis.MinorTic.IsOpposite = false;
                // Don't display the Y zero line
                myPane.YAxis.MajorGrid.IsZeroLine = false;
                // Align the Y axis labels so they are flush to the axis
                myPane.YAxis.Scale.Align = AlignP.Inside;
                myPane.YAxis.Scale.Max = 100;

                // Enable the Y2 axis display
                myPane.Y2Axis.IsVisible = true;
                // Make the Y2 axis scale blue
                myPane.Y2Axis.Scale.FontSpec.FontColor = Color.Blue;
                myPane.Y2Axis.Title.FontSpec.FontColor = Color.Blue;
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                myPane.Y2Axis.MajorTic.IsOpposite = false;
                myPane.Y2Axis.MinorTic.IsOpposite = false;
                // Display the Y2 axis grid lines
                myPane.Y2Axis.MajorGrid.IsVisible = true;
                // Align the Y2 axis labels so they are flush to the axis
                myPane.Y2Axis.Scale.Align = AlignP.Inside;
                myPane.Y2Axis.Scale.Min = 1.5;
                myPane.Y2Axis.Scale.Max = 3;

                // Create a second Y Axis, green
                YAxis yAxis3 = new YAxis("Distance, m");
                myPane.YAxisList.Add(yAxis3);
                yAxis3.Scale.FontSpec.FontColor = Color.Green;
                yAxis3.Title.FontSpec.FontColor = Color.Green;
                yAxis3.Color = Color.Green;
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                yAxis3.MajorTic.IsInside = false;
                yAxis3.MinorTic.IsInside = false;
                yAxis3.MajorTic.IsOpposite = false;
                yAxis3.MinorTic.IsOpposite = false;
                // Align the Y2 axis labels so they are flush to the axis
                yAxis3.Scale.Align = AlignP.Inside;

                Y2Axis yAxis4 = new Y2Axis("Energy");
                yAxis4.IsVisible = true;
                myPane.Y2AxisList.Add(yAxis4);
                // turn off the opposite tics so the Y2 tics don't show up on the Y axis
                yAxis4.MajorTic.IsInside = false;
                yAxis4.MinorTic.IsInside = false;
                yAxis4.MajorTic.IsOpposite = false;
                yAxis4.MinorTic.IsOpposite = false;
                // Align the Y2 axis labels so they are flush to the axis
                yAxis4.Scale.Align = AlignP.Inside;
                yAxis4.Type = AxisType.Log;
                yAxis4.Scale.Min = 100;

                // Fill the axis background with a gradient
                myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);

                zgc.AxisChange();
                graphForm.Show();
                 */
            }
            else
            {
                ShowGraphPriceDelegate showGraphPrice = new ShowGraphPriceDelegate(ShowGraphPrice);
                this.BeginInvoke(showGraphPrice, new object[] { text, list, graphForm, minPrice, maxPrice });
            }
        }

        private void ShowGraphSimple(string text, listGraphs list, GraphForm graphForm)
        {
            
            graphForm.zedGraphControl.GraphPane = list.myGraphPane;
            GraphPane myPane = graphForm.zedGraphControl.GraphPane;
            myPane = list.myGraphPane;
            LineItem[] curves = new LineItem[list.list.Length];
            for (int i = 0; i < list.list.Length; i++)
            {
                //myPane.CurveList.                    
                curves[i] = myPane.AddCurve(list.curves[i].Label.Text, list.list[i], list.curves[i].Color, list.curves[i].Symbol.Type);
                curves[i].Symbol.Size = list.curves[i].Symbol.Size;
                curves[i].Symbol.Fill = new Fill(list.curves[i].Symbol.Fill);
                curves[i].Line.Width = list.curves[i].Line.Width;
                curves[i].Line.IsVisible = list.curves[i].Line.IsVisible;
            }
            graphForm.zedGraphControl.AxisChange();
            graphForm.Show();            
        }     

        /*
        delegate void ShowGraphDelegate(string text, PointPairList list);
        private void ShowGraph(string text, PointPairList list)
        {            
            // Проверяем, не вызывается ли метод из UI-потока
            if (graphForm.zedGraphControl.InvokeRequired == false)
            {                
                GraphPane myPane = graphForm.zedGraphControl.GraphPane;
                int fontSize = 5;
                myPane.YAxis.Title.FontSpec.Size = 10;
                myPane.YAxis.Scale.FontSpec.Size = 10;
                myPane.XAxis.Scale.FontSpec.Size = 10;
                myPane.XAxis.Title.FontSpec.Size = 10;
                myPane.Title.FontSpec.Size = 10;
                LineItem curve = myPane.AddCurve("", list, Color.Red, SymbolType.Circle);
                curve.Line.Width = 1F;
                curve.Symbol.Fill = new Fill(Color.Red);
                curve.Symbol.Size = 1;
                graphForm.zedGraphControl.AxisChange();
                graphForm.Show();
            }
            else
            {
                ShowGraphDelegate showGraph = new ShowGraphDelegate(ShowGraph);
                this.BeginInvoke(showGraph, new object[] { text, list });
            }             
        }        
        */
        delegate void ProcessEndDelegate();
        private void ProcessEnd()
        {
            // Проверяем, не вызывается ли метод из UI-потока
            if (this.InvokeRequired == false)
            {
                form2.Hide();
                form2.Dispose();
                if (doBatchLine < BatchLines.GetLength(0)-1)
                {
                    doBatchLine++;
                    Run(BatchLines[doBatchLine]);
                }
            }
            else
            {
                ProcessEndDelegate d = new ProcessEndDelegate(ProcessEnd);
                this.BeginInvoke(d);
            }
        }

        private void variationIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VariationIndexParamsForm formParams = new VariationIndexParamsForm();
            formParams.varIndStartbutton.Click +=new EventHandler(varIndStartbutton_Click);
            formParams.Show();
        }

        private void varIndStartbutton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            VariationIndexParamsForm formParams = (VariationIndexParamsForm)btn.Parent;
            //MessageBox.Show("Start compute Variation Index " + formParams.param[0]);

            Hashtable param = new Hashtable();
            for (int i = 0; i < formParams.param.Length - 1; i += 2) param.Add(formParams.param[i], formParams.param[i + 1]);
            object procedureParam = getVarIndParam(param);
            formParams.Close();
            //asyncProcess("varind", dataParams, procedureParam);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            LoadDataParamForm dataParamForm = new LoadDataParamForm();
            dataParamForm.btnLoadData.Click += new EventHandler(btnLoadData_Click);            
            dataParamForm.ShowDialog();
            //dataParamForm.Activate();                       
        }

        /// <summary>
        /// Загрузка данных и пострение графика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadData_Click(object sender, EventArgs e)
        {
            form2 = new FormProcess();
            formReport.MdiParent = this;
            formReport.Show();
            form2.Activate();

            GraphForm graphForm = new GraphForm();
            graphForm.MdiParent = this;
            
            Button btn = (Button)sender;
            LoadDataParamForm dataParamForm = (LoadDataParamForm)btn.Parent;
            string[] paramVal = dataParamForm.textBoxDataParams.Text.Split();
            Hashtable param = new Hashtable();
            for (int i = 1; i < paramVal.Length - 1; i += 2) param.Add(paramVal[i], paramVal[i + 1]);
            dataParams = new DataParams();
            dataParams.fileName = openFileDialog1.FileName;
            dataParams.timePeriod = (string)param["dtp"];
            dataParams.maxCount = Convert.ToInt32(param["dmc"]);
            dataParams.columnIndex = Convert.ToInt32(param["dci"]);
            dataParams.delimiter = Convert.ToChar(param["dd"]);
            dataParams.format = (string)param["dfr"];
            dataParams.isDoClearFile = (bool)Convert.ToBoolean(param["didcf"]);
            dataParamForm.Close();

            int errors;
            if (dataParams.isDoClearFile) errors = DataProcess.ClearFile(dataParams.fileName);
            float[] C;
            string periodStr = "";
            int discharging = 0;
            if (dataParams.timePeriod != null)
            {
                C = LoadArrayByTime(dataParams, ref periodStr, ref discharging);
                //Экспортируем в файл _time_filename значения с датой
            }
            //просто по столбцу данных
            else C = LoadArraySimple(dataParams, ref discharging);

            PointPairList list = new PointPairList();
            int lengthList = C.Length;
            for (int i = 0; i < lengthList; i++)
            {
                list.Add(i, C[i]);
            }

            GraphPane myPane = graphForm.zedGraphControl.GraphPane;
            int fontSize = 5;
            myPane.YAxis.Title.FontSpec.Size = 10;
            myPane.YAxis.Scale.FontSpec.Size = 10;
            myPane.XAxis.Scale.FontSpec.Size = 10;
            myPane.XAxis.Title.FontSpec.Size = 10;
            myPane.Title.FontSpec.Size = 10;
            LineItem curve = myPane.AddCurve("", list, Color.Red, SymbolType.Circle);
            curve.Line.Width = 1F;
            curve.Symbol.Fill = new Fill(Color.Red);
            curve.Symbol.Size = 1;            
            graphForm.zedGraphControl.AxisChange();
            graphForm.Show();
            //graphForm.CreateZedGraphControl();
        }

        private void бродскогоДарховскогоToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}