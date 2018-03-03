using System;
using System.Collections.Generic;
using System.Text;
using ChaosExpert;

namespace ChaosExpert
{
    public class Trader
    {
        public const int SELL = -1;
        public const int BUY = 1;

        public const int PROFIT = 1;
        public const int STOP = -1;

        public const byte ACTIVE = 1;
        public const byte EXECUTED = 2;
        public const byte CANCELED = 0;
        public const byte WAIT = 3;        

        private Point[] DATA;
        private LocalExtremumStatistic[] STATISTICS;
        private List<PointLocalExtremum> EXTREMUMS;
        //точка, с которой начинается торговля (индекс в DATA)
        private int startTradeInd;

        private List<OrderLimit> ordersLimit;
        private List<OrderLimit> ordersLimitExecuted;
        private List<OrderStop> ordersStop;
        private List<OrderProfit> ordersProfit;
        private List<OrderCondition> ordersCondition;
        private List<OrderCondition> ordersConditionExecuted;

        private int lastExecutedOrderCondition = -1;
        //текущий стоп
        private int currentStopId = -1;
        //текущий профит
        private int currentProfitId = -1;
        
        /*
        //Активные заявки
        private List<ActiveOrder> activeOrders;
        //Активные стоп-заявки
        private List<ActiveStopOrder> activeStopOrders;
        //Активные тейк-профит заявки
        private List<ActiveProfitOrder> activeProfitOrders;
         */ 
        //Исполненные заявки
        private List<ExecutedOrder> executedOrders;
        private List<int> deletedOrders;
        private List<float> cashVals;
        //Портфель акций
        private float securityPortfolio = 0;
        //private List<Security> securityPortfolio;
        private int countOpenOrders = 0;
        //Кошелек с деньгами
        private float cash = 30000f;
        private float commission = 0;
        private int countOrder;
        //минимальный шаг изменения цены.
        private float minPriceStep = 1f;//абсолютное значение
        private bool isPositionOpen = false;
        private bool isLongOpen = false;
        private bool isShortOpen = false;
        private bool is_allow_long = true;
        bool stopRaise = false;
        private DateTime startCredit;
        float sumCredit = 0;
        private int countOrdersConditionExecuted = 0;
        int indLevel = -1;
        int indLevel2 = -1;
        bool isOrderConditionSet = false;
        public Trader(Point[] DATA)
        {
            this.DATA = DATA;

            ordersLimit = new List<OrderLimit>();
            ordersLimitExecuted = new List<OrderLimit>();
            ordersStop = new List<OrderStop>();
            ordersProfit = new List<OrderProfit>();
            ordersCondition = new List<OrderCondition>();
            ordersConditionExecuted = new List<OrderCondition>();
            cashVals = new List<float>();
            
            /*
            EXTREMUMS = new List<PointLocalExtremum>();                       
            activeOrders = new List<ActiveOrder>();
            activeStopOrders = new List<ActiveStopOrder>();
            activeProfitOrders = new List<ActiveProfitOrder>();
            executedOrders = new List<ExecutedOrder>();
            deletedOrders = new List<int>();
            securityPortfolio = new List<Security>();
             */ 
        }

        /*
         * Если оранжевая пересекает красную - открываем позицию
         */
        public TradeResults StrategySimpleOrangeAcrossRed(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[5];

            periodsMA[0] = 360;//черная
            periodsMA[1] = 720;//зеленая
            periodsMA[2] = 1440;//красная

            periodsMA[3] = 20;//оранжевая
            periodsMA[4] = 120;//фиолетовая

            /*
            periodsMA[0] = 720;//черная
            periodsMA[1] = 1440;//зеленая
            periodsMA[2] = 2880;//красная

            periodsMA[3] = 40;//оранжевая
            periodsMA[4] = 240;//фиолетовая

            
            periodsMA[0] = 180;//черная
            periodsMA[1] = 360;//зеленая
            periodsMA[2] = 720;//красная

            periodsMA[3] = 10;//оранжевая
            periodsMA[4] = 60;//фиолетовая
            */


            //по 10 последних значений
            float[,] lastMA = new float[periodsMA.Length, 10];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];


            float priceLevel = 0;
            int isDownLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            indLevel = 0;
            indLevel2 = 0;

            //индекс когда Ф пересекла Ч сверху вниз
            int indVioletDownBlack = 0;
            //индекс когда Ф пересекла Ч снизу
            int indVioletUpBlack = 0;
            //индекс когда Ч пересекла З снизу
            int indBlackUpGreen = 0;
            //индекс когда О пересекла К снизу
            int indOrangeUpRed = 0;
            //индекс когда О пересекла К сверху
            int indOrangeDownRed = 0;
            //индекс когда О пересекла Ф снизу
            int indOrangeUpViolet = 0;
            //индекс когда О пересекла Ф сверху
            int indOrangeDownViolet = 0;
            //Последняя цена по которой покупали (при отслеживании позиции на росте)
            float lastBuyPrice = 0;
            bool buyCondition = false;
            bool buyCondition2 = false;            
            string log = "";
            float deltaOrangeRedMax = 0;
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }

                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                //Если условная заявка выставлена
                if (isOrderConditionSet)
                {
                    //Отслеживаем условия
                }
                else if (!isLongOpen && !isOrderConditionSet)
                {
                    //TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                    //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                    /*
                    log += "Check (" + currentInd.ToString() + "): 1=" +
                        (lastMA[4, 0] < lastMA[2, 0] && lastMA[0, 0] < lastMA[2, 0]).ToString() + " \n"
                    + " 2=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 3=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 4=" + (DATA[currentInd].val < lastMA[2, 0] && DATA[currentInd - 1].val < lastMA[2, 1] && DATA[currentInd - 2].val < lastMA[2, 2]).ToString() + " \n"
                    //+ " 5=" + (DATA[currentInd].val < lastMA[1, 0] && DATA[currentInd - 1].val < lastMA[1, 1] && DATA[currentInd - 2].val < lastMA[1, 2]).ToString() + " \n"
                    ;
                    */

                    buyCondition = false;
                    buyCondition2 = false;

                    //Ф пересекла Ч сверху вниз
                    if (lastMA[4, 0] < lastMA[0, 0] && lastMA[4, 1] < lastMA[0, 1]
                     && lastMA[4, 2] >= lastMA[0, 2] && lastMA[4, 3] > lastMA[0, 3]
                       )
                    {
                        indVioletDownBlack = currentInd;                        
                    }

                    //Ф пересекла Ч снизу
                    if (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] > lastMA[0, 1]
                     && lastMA[4, 2] <= lastMA[0, 2] && lastMA[4, 3] < lastMA[0, 3]
                       )
                    {
                        indVioletDownBlack = 0;                        
                    }

                    //Ч пересекла З снизу вверх
                    if (lastMA[0, 0] > lastMA[1, 0] && lastMA[0, 1] > lastMA[1, 1]
                     && lastMA[0, 2] <= lastMA[1, 2] && lastMA[0, 3] < lastMA[1, 3]
                       )
                    {
                        indBlackUpGreen = currentInd;                        
                    }

                    //Ч пересекла З сверху
                    if (lastMA[0, 0] < lastMA[1, 0] && lastMA[0, 1] < lastMA[1, 1]
                     && lastMA[0, 2] >= lastMA[1, 2] && lastMA[0, 3] > lastMA[1, 3]
                       )
                    {
                        indBlackUpGreen = 0;
                        //indVioletDownBlack = 0;
                    }

                    //О пересекла К снизу вверх
                    if (lastMA[3, 0] > lastMA[2, 0] && lastMA[3, 1] > lastMA[2, 1]
                     && lastMA[3, 2] <= lastMA[2, 2] && lastMA[3, 3] < lastMA[2, 3])
                    {
                        indOrangeUpRed = currentInd;
                        indOrangeDownRed = 0;
                    }

                    //О пересекла К сверху
                    if (lastMA[3, 0] < lastMA[2, 0] && lastMA[3, 1] < lastMA[2, 1]
                     && lastMA[3, 2] >= lastMA[2, 2] && lastMA[3, 3] > lastMA[2, 3])
                    {
                        indOrangeDownRed = currentInd;
                        indOrangeUpRed = 0;
                    }

                    //О пересекла Ф снизу вверх
                    if (lastMA[3, 0] > lastMA[4, 0] && lastMA[3, 1] > lastMA[4, 1]
                     && lastMA[3, 2] <= lastMA[4, 2] && lastMA[3, 3] < lastMA[4, 3])
                    {
                        indOrangeUpViolet = currentInd;
                        indOrangeDownViolet = 0;
                    }

                    //О пересекла Ф сверху
                    if (lastMA[3, 0] < lastMA[4, 0] && lastMA[3, 1] < lastMA[4, 1]
                     && lastMA[3, 2] >= lastMA[4, 2] && lastMA[3, 3] > lastMA[4, 3])
                    {
                        indOrangeDownViolet = currentInd;
                        indOrangeUpViolet = 0;
                    }

                    if (
                        (((currentInd-indOrangeUpRed) > 0
                        && (currentInd - indOrangeUpRed) < 50
                        && (lastMA[3, 0] - lastMA[2, 0]) > 5
                        && (lastMA[3, 0] - lastMA[2, 0]) < 15
                        )
                        ||
                        ((currentInd - indOrangeUpRed) > 50
                        && (currentInd - indOrangeUpRed) < 5000
                        && (lastMA[3, 0] - lastMA[2, 0]) > 5
                        && (lastMA[3, 0] - lastMA[2, 0]) < 200
                        )
                        )
                        //О > Ф
                        && lastMA[3, 0] > lastMA[4, 0]
                        //Ф > Ч
                        && lastMA[4, 0] > lastMA[0, 0]
                        //Ф растет
                         && lastMA[4, 0] >= lastMA[4, 1]
                         && lastMA[4, 1] >= lastMA[4, 2]
                         && MathProcess.DeltaProcent(lastMA[4, 2], lastMA[4, 0]) >= 0.001f
                        
                         //О растет
                         && lastMA[3, 0] >= lastMA[3, 1]
                         && lastMA[3, 1] >= lastMA[3, 2]
                         && MathProcess.DeltaProcent(lastMA[3, 2], lastMA[3, 0]) >= 0.0008f
                        
                        )
                    {
                        buyCondition2 = true;
                    }

                    buyCondition =
                     //Ч пересекла З снизу вверх
                     indBlackUpGreen > 0 
                     //&& indOrangeUpRed > indBlackUpGreen
                     //О недавно пересекла К снизу
                     && (currentInd-indOrangeUpRed) < 3000
                     //О пересекла Ф снизу
                     && indOrangeUpViolet>0
                     //О растет
                     && lastMA[3, 0] >= lastMA[3, 1]
                     && lastMA[3, 1] >= lastMA[3, 2]
                     && lastMA[3, 0] >= lastMA[3, 2]
                     //Ф растет
                     && lastMA[4, 0] >= lastMA[4, 1]
                     && lastMA[4, 1] >= lastMA[4, 2]                     
                     && MathProcess.DeltaProcent(lastMA[4, 2], lastMA[4, 0]) >= 0.001f
                     //Ч растет
                     && lastMA[0, 0] >= lastMA[0, 1]
                     && lastMA[0, 1] >= lastMA[0, 2]
                     && MathProcess.DeltaProcent(lastMA[0, 2], lastMA[0, 0]) >= 0.0008f
                     //З растет
                     && lastMA[1, 0] >= lastMA[1, 1]
                     && lastMA[1, 1] >= lastMA[1, 2]                     
                     && MathProcess.DeltaProcent(lastMA[1, 2], lastMA[1, 0]) >= 0.0007f
                     && 
                     //К растет
                     ((lastMA[2, 0] >= lastMA[2, 1] 
                      && lastMA[2, 1] >= lastMA[2, 2]
                      && MathProcess.DeltaProcent(lastMA[2, 2], lastMA[2, 0]) >= 0.0006f
                     )                     
                     /*||
                     //или К не растет но О больше К на опр. порог
                     (MathProcess.DeltaProcent(lastMA[2, 0], lastMA[3, 0]) >= 0.2f)
                      */ 
                     )
                     
                     //Цена выше К
                     && DATA[currentInd].val > lastMA[2, 0]
                     //О > Ф
                     && lastMA[3, 0] > lastMA[4, 0]
                     //Ф > Ч
                     && lastMA[4, 0] > lastMA[0, 0]
                     //Ч > З
                     && lastMA[0, 0] > lastMA[1, 0]
                     //З > К
                     && lastMA[1, 0] > lastMA[2, 0]
                     //&& MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)<0.3f

                     ;
                    /*
                    if (isLevel)
                    {

                    }
                    */
                    if (buyCondition2)
                    {
                        //log += "+ (" + currentInd.ToString() + "): 1=" +
                        //(MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+"\n";
                        //float deltaPrice = minPriceStep;
                        // - minPriceStep*10
                        float price = lastMA[3, 0] + minPriceStep * 5;
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            //
                            newOrderLimitStop.price = price - minPriceStep * 30;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price + minPriceStep * 120;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            isOrderConditionSet = true;
                            //is_allow_long = false;
                        }
                    }
                }
                //isLongOpen
                else if (isLongOpen)
                {
                    
                    /*Отслеживаем открытую позицию чтобы оперативно изменить стоп                    
                     *Когда О пересекает Ф (или близко), меняем стоп чтобы быстро закрыться
                     * 
                     */
                    float profit = DATA[currentInd].val - ordersConditionExecuted[ordersConditionExecuted.Count - 1].price;
                    //Если текущий профит больше заданного порога
                    if (!stopRaise && profit > 12)
                    {
                        //поднимаем стоп
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentStopId];
                        orderLimitTmp.price = ordersConditionExecuted[ordersConditionExecuted.Count - 1].price + minPriceStep * 5;
                        ordersLimit[currentStopId] = orderLimitTmp;
                        stopRaise = true;
                    }
                    //Если Ф пересекла Ч сверху начала падать
                    if (profit > 10 &&
                        lastMA[4, 0] < lastMA[0, 0] && lastMA[4, 1] < lastMA[0, 1]
                     && lastMA[4, 2] >= lastMA[0, 2] && lastMA[4, 3] > lastMA[0, 3]                        
                        )
                    {
                        //меняем профит к текущей цене
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentProfitId];
                        orderLimitTmp.price = DATA[currentInd].val - minPriceStep * 2;
                        ordersLimit[currentProfitId] = orderLimitTmp;
                    }
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }


        /*
         * Если оранжевая пересекает красную больше определенного порога - открываем позицию
         */
        public TradeResults StrategyOrangeAcrossRed(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[5];
            
            periodsMA[0] = 360;//черная
            periodsMA[1] = 720;//зеленая
            periodsMA[2] = 1440;//красная

            periodsMA[3] = 20;//оранжевая
            periodsMA[4] = 120;//фиолетовая

            /*
            periodsMA[0] = 180;//черная
            periodsMA[1] = 360;//зеленая
            periodsMA[2] = 720;//красная

            periodsMA[3] = 10;//оранжевая
            periodsMA[4] = 60;//фиолетовая
            */


            //по 10 последних значений
            float[,] lastMA = new float[periodsMA.Length, 10];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];


            float priceLevel = 0;
            int isDownLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            indLevel = 0;
            indLevel2 = 0;
            string log = "";
            float deltaOrangeRedMax = 0;            
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }

                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }

                //оранжевая пересекла красную снизу вверх
                if (lastMA[3, 0] > lastMA[2, 0] && lastMA[3, 1] > lastMA[2, 1]
                 && lastMA[3, 2] <= lastMA[2, 2] && lastMA[3, 3] < lastMA[2, 3]
                   )
                {
                    //indLevel = currentInd;
                    //indLevel2 = 0;                        
                }

                //оранжевая пересекла красную сверху вниз
                if (lastMA[3, 0] < lastMA[2, 0] && lastMA[3, 1] < lastMA[2, 1]
                 && lastMA[3, 2] >= lastMA[2, 2] && lastMA[3, 3] > lastMA[2, 3]
                   )
                {
                    indLevel = -1;
                    indLevel2 = -1;
                    deltaOrangeRedMax = 0;
                }


                if (indLevel == -1)
                {
                    //если оранжевая под красной - запоминаем максимальное отклонение ее от красной
                    float deltaOrangeRed = MathProcess.DeltaProcent(lastMA[3, 0], lastMA[2, 0]);
                    if (deltaOrangeRed > deltaOrangeRedMax)
                    {
                        deltaOrangeRedMax = deltaOrangeRed;
                    }
                }

                //оранжевая пересекла красную снизу вверх и К растет
                if (indLevel < 0 && lastMA[3, 0] > lastMA[2, 0] && lastMA[3, 1] > lastMA[2, 1]
                 && lastMA[3, 2] >= lastMA[2, 2] && lastMA[3, 3] < lastMA[2, 3]
                 && lastMA[3, 4] < lastMA[2, 4] && lastMA[3, 5] < lastMA[2, 5]
                    //красная растет
                 && lastMA[2, 0] >= lastMA[2, 1]
                 && deltaOrangeRedMax > 0.03f
                 )
                {
                    //индекс что О пересекла К снизу вверх когда К растет и при этом О была ниже К до опр. порога
                    indLevel = currentInd;
                    indLevel2 = -1;
                }

                //оранжевая пересекла красную снизу вверх и К падает
                if (indLevel < 0 && lastMA[3, 0] > lastMA[2, 0] && lastMA[3, 1] > lastMA[2, 1]
                 && lastMA[3, 2] >= lastMA[2, 2] && lastMA[3, 3] < lastMA[2, 3]
                 && lastMA[3, 4] < lastMA[2, 4] && lastMA[3, 5] < lastMA[2, 5]
                    //красная падает
                 && lastMA[2, 0] <= lastMA[2, 1]
                 )
                {
                    indLevel = -1;
                    //индекс что О пересекла К снизу вверх когда К падает
                    indLevel2 = currentInd;
                    priceLevel = lastMA[3, 0]*1.003f;
                    isDownLevel = 0;
                }

                //если опустились ниже зафиксированного уровня
                if (indLevel2 > 0
                    && lastMA[3, 0] < priceLevel
                    && lastMA[3, 1] >= priceLevel
                    && lastMA[3, 2] >= priceLevel
                    )
                {
                    isDownLevel = currentInd;
                }

                if (!isLongOpen && !isOrderConditionSet)
                {
                    //TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                    //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                    /*
                    log += "Check (" + currentInd.ToString() + "): 1=" +
                        (lastMA[4, 0] < lastMA[2, 0] && lastMA[0, 0] < lastMA[2, 0]).ToString() + " \n"
                    + " 2=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 3=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 4=" + (DATA[currentInd].val < lastMA[2, 0] && DATA[currentInd - 1].val < lastMA[2, 1] && DATA[currentInd - 2].val < lastMA[2, 2]).ToString() + " \n"
                    //+ " 5=" + (DATA[currentInd].val < lastMA[1, 0] && DATA[currentInd - 1].val < lastMA[1, 1] && DATA[currentInd - 2].val < lastMA[1, 2]).ToString() + " \n"
                    ;
                    */
                    
                    //условие когда К растет
                    bool buyCondition =
                     indLevel > 0 && (currentInd - indLevel) < 100
                        && MathProcess.DeltaProcent(lastMA[2, 0], lastMA[3, 0]) > 0.05f
                        && DATA[currentInd].val >= DATA[currentInd - 1].val
                    ;

                    if (!buyCondition)
                    {
                        //условие когда К падает
                        buyCondition =
                        indLevel2 > 0 
                        && isDownLevel > 0 
                        && (currentInd - indLevel2) < 1500
                        && MathProcess.DeltaProcent(priceLevel, lastMA[3, 0]) > 0.08f
                        && DATA[currentInd].val >= DATA[currentInd - 1].val
                    ;
                    }
                    if (buyCondition)
                    {
                        //log += "+ (" + currentInd.ToString() + "): 1=" +
                        //(MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+"\n";
                        float deltaPrice = minPriceStep;
                        /*
                        if (DATA[currentInd].val >= DATA[currentInd - 1].val
                            && DATA[currentInd - 1].val >= DATA[currentInd - 2].val
                            && DATA[currentInd - 2].val >= DATA[currentInd - 3].val
                            )
                        {
                            deltaPrice = (-1)*minPriceStep * 10;
                        }
                        else 
                        {
                            deltaPrice = minPriceStep * 5;
                        }
                        */
                        float price=0;
                        if (priceLevel > 0)
                        {
                            price = DATA[currentInd].val;
                        }
                        else
                        {
                            price = DATA[currentInd].val - minPriceStep * 2;
                        }
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price - minPriceStep*20;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price + minPriceStep * 120;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            isOrderConditionSet = true;
                            //is_allow_long = false;
                        }
                    }
                    
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        indLevel = -1;
                        //условие на закрытие длинной позиции
                        /*
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                         */ 
                    }
                    
                }
                //isLongOpen
                else
                {
                    /*
                    //Отслеживаем открытую позицию чтобы оперативно изменить стоп                    
                    float delta_profit = MathProcess.DeltaProcent(ordersConditionExecuted[ordersConditionExecuted.Count - 1].price, DATA[currentInd].val);
                    if (delta_profit > 0.04f && !stopRaise)
                    {
                        //поднимаем стоп                        
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentStopId];
                        orderLimitTmp.price = DATA[currentInd].val - minPriceStep * 4;
                        ordersLimit[currentStopId] = orderLimitTmp;
                        stopRaise = true;
                    }

                    //Если Ф начала падать
                    if (delta_profit > 0.1f && lastMA[4, 0] < lastMA[4, 1] && lastMA[4, 1] < lastMA[4, 2])
                    {
                        //поднимаем меняем профит к текущей цене
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentProfitId];
                        orderLimitTmp.price = DATA[currentInd].val;
                        ordersLimit[currentProfitId] = orderLimitTmp;
                    }
                     */
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }


        /*
         * Если цена растет 2 (N) сделки в подряд - покупаем с коротким профитом
         */
        public TradeResults StrategySimpleInertion(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[5];
            periodsMA[0] = 360;
            periodsMA[1] = 720;
            periodsMA[2] = 1440;

            periodsMA[3] = 20;
            periodsMA[4] = 120;
            //по 10 последних значений
            float[,] lastMA = new float[periodsMA.Length, 10];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];

            int indLevel = -1;
            float priceLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;

            string log = "";
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }
                
                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                isLongOpen = true;
                if (!isLongOpen)
                {
                    //TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                    //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                    /*
                    log += "Check (" + currentInd.ToString() + "): 1=" +
                        (lastMA[4, 0] < lastMA[2, 0] && lastMA[0, 0] < lastMA[2, 0]).ToString() + " \n"
                    + " 2=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 3=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 4=" + (DATA[currentInd].val < lastMA[2, 0] && DATA[currentInd - 1].val < lastMA[2, 1] && DATA[currentInd - 2].val < lastMA[2, 2]).ToString() + " \n"
                    //+ " 5=" + (DATA[currentInd].val < lastMA[1, 0] && DATA[currentInd - 1].val < lastMA[1, 1] && DATA[currentInd - 2].val < lastMA[1, 2]).ToString() + " \n"
                    ;
                    */
                    bool buyCondition =
                        
                        DATA[currentInd].val >= DATA[currentInd - 1].val
                        && DATA[currentInd-1].val >= DATA[currentInd - 2].val
                        && DATA[currentInd].val > DATA[currentInd - 2].val
                    ;
                    if (buyCondition)
                    {
                        //log += "+ (" + currentInd.ToString() + "): 1=" +
                        //(MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+"\n";
                        float deltaPrice = minPriceStep;
                        /*
                        if (DATA[currentInd].val >= DATA[currentInd - 1].val
                            && DATA[currentInd - 1].val >= DATA[currentInd - 2].val
                            && DATA[currentInd - 2].val >= DATA[currentInd - 3].val
                            )
                        {
                            deltaPrice = (-1)*minPriceStep * 10;
                        }
                        else 
                        {
                            deltaPrice = minPriceStep * 5;
                        }
                        */
                        float price = DATA[currentInd].val-minPriceStep*4;
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price - minPriceStep*2;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price + minPriceStep*5;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            //is_allow_long = false;
                        }
                    }
                    /*
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        //условие на закрытие длинной позиции
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                    }
                     */
                }
                //isLongOpen
                else
                {
                    /*
                    //Отслеживаем открытую позицию чтобы оперативно изменить стоп                    
                    float delta_profit = MathProcess.DeltaProcent(ordersConditionExecuted[ordersConditionExecuted.Count - 1].price, DATA[currentInd].val);
                    if (delta_profit > 0.04f && !stopRaise)
                    {
                        //поднимаем стоп                        
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentStopId];
                        orderLimitTmp.price = DATA[currentInd].val - minPriceStep * 4;
                        ordersLimit[currentStopId] = orderLimitTmp;
                        stopRaise = true;
                    }

                    //Если Ф начала падать
                    if (delta_profit > 0.1f && lastMA[4, 0] < lastMA[4, 1] && lastMA[4, 1] < lastMA[4, 2])
                    {
                        //поднимаем меняем профит к текущей цене
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentProfitId];
                        orderLimitTmp.price = DATA[currentInd].val;
                        ordersLimit[currentProfitId] = orderLimitTmp;
                    }
                     */ 
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }


        /*
         * Фиолетовая пересекает черную снизу вверх под красной, при этом и Ф, Ч, К - в росте. Покупка если цена под К.
         */
        public TradeResults StrategyMA_PurpleAcrossBlack(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[5];
            periodsMA[0] = 360;
            periodsMA[1] = 720;
            periodsMA[2] = 1440;

            periodsMA[3] = 20;
            periodsMA[4] = 120;
            //по 10 последних значений
            float[,] lastMA = new float[periodsMA.Length, 10];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];

            int indLevel = -1;
            float priceLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            
            string log = "";
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }
                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                if (!isLongOpen)
                {
                    //TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                    //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                    /*
                    log += "Check (" + currentInd.ToString() + "): 1=" +
                        (lastMA[4, 0] < lastMA[2, 0] && lastMA[0, 0] < lastMA[2, 0]).ToString() + " \n"
                    + " 2=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 3=" + (lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 0] && lastMA[4, 2] < lastMA[0, 0]).ToString() + " \n"
                    //+ " 4=" + (DATA[currentInd].val < lastMA[2, 0] && DATA[currentInd - 1].val < lastMA[2, 1] && DATA[currentInd - 2].val < lastMA[2, 2]).ToString() + " \n"
                    //+ " 5=" + (DATA[currentInd].val < lastMA[1, 0] && DATA[currentInd - 1].val < lastMA[1, 1] && DATA[currentInd - 2].val < lastMA[1, 2]).ToString() + " \n"
                    ;
                    */
                    bool buyCondition =
                        //Ф и Ч под К
                    lastMA[4, 0] < lastMA[2, 0] && lastMA[0, 0] < lastMA[2, 0]
                        //Ф пересекает Ч снизу вверх
                    && lastMA[4, 0] > lastMA[0, 0] && lastMA[4, 1] >= lastMA[0, 1] && lastMA[4, 2] < lastMA[0, 2]
                        //цена должна быть не слишком быстро к К
                        //&& MathProcess.DeltaProcent(DATA[currentInd].val, lastMA[2, 0]) > 0.1f
                        //когда Ф близко к К
                    && (MathProcess.DeltaProcent(lastMA[2, 0], lastMA[2, 1]) < 0.0001f
                        //К не падает слишком быстро    
                        && MathProcess.DeltaProcent(lastMA[4, 0], lastMA[2, 0]) < 0.2f
                        //З не падает слишком бстро
                        && MathProcess.DeltaProcent(lastMA[1, 0], lastMA[1, 1]) < 0.0003f                        
                        )
                        //Ф растет быстро
                    && MathProcess.DeltaProcent(lastMA[4, 1], lastMA[4, 0]) > 0.001f
                        //Ч в росте
                        //&& lastMA[0, 0] > lastMA[0, 1] && lastMA[0, 1] > lastMA[0, 2] && lastMA[0, 2] > lastMA[0, 3]
                        //К в росте
                        //&& lastMA[2, 0] > lastMA[2, 1] && lastMA[2, 1] > lastMA[2, 2] && lastMA[2, 2] > lastMA[2, 3]
                        //З в росте
                        //&& lastMA[1, 0] > lastMA[1, 1] && lastMA[1, 1] > lastMA[1, 2] && lastMA[1, 2] > lastMA[1, 3]

                    //Цена ниже К                    
                        //&& DATA[currentInd].val < lastMA[2, 0] && DATA[currentInd - 1].val < lastMA[2, 1] && DATA[currentInd - 2].val < lastMA[2, 2]

                    //Цена ниже З
                        //&& DATA[currentInd].val < lastMA[1, 0] && DATA[currentInd - 1].val < lastMA[1, 1] && DATA[currentInd - 2].val < lastMA[1, 2]
                    //Цена не падает
                    && !(DATA[currentInd].val <= DATA[currentInd - 1].val 
                        && DATA[currentInd - 1].val <= DATA[currentInd-2].val
                        && DATA[currentInd - 2].val <= DATA[currentInd - 3].val
                        )
                    ;
                    if (buyCondition)
                    {
                        //log += "+ (" + currentInd.ToString() + "): 1=" +
                        //(MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+"\n";
                        float deltaPrice = (-1) * minPriceStep * 15;
                        /*
                        if (DATA[currentInd].val >= DATA[currentInd - 1].val
                            && DATA[currentInd - 1].val >= DATA[currentInd - 2].val
                            && DATA[currentInd - 2].val >= DATA[currentInd - 3].val
                            )
                        {
                            deltaPrice = (-1)*minPriceStep * 10;
                        }
                        else 
                        {
                            deltaPrice = minPriceStep * 5;
                        }
                        */
                        float price = DATA[currentInd].val + deltaPrice;
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price * 0.99999f;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price * 1.01f;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            //is_allow_long = false;
                        }
                    }
                    /*
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        //условие на закрытие длинной позиции
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                    }
                     */
                }
                //isLongOpen
                else
                {
                    //Отслеживаем открытую позицию чтобы оперативно изменить стоп                    
                    float delta_profit = MathProcess.DeltaProcent(ordersConditionExecuted[ordersConditionExecuted.Count - 1].price, DATA[currentInd].val);                    
                    if (delta_profit > 0.04f && !stopRaise)
                    {                        
                        //поднимаем стоп                        
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentStopId];
                        orderLimitTmp.price = DATA[currentInd].val - minPriceStep * 4;
                        ordersLimit[currentStopId] = orderLimitTmp;
                        stopRaise = true;                                                  
                    }

                    //Если Ф начала падать
                    if (delta_profit > 0.1f && lastMA[4, 0] < lastMA[4, 1] && lastMA[4, 1] < lastMA[4, 2])
                    {
                        //поднимаем меняем профит к текущей цене
                        OrderLimit orderLimitTmp = new OrderLimit();
                        orderLimitTmp = ordersLimit[currentProfitId];
                        orderLimitTmp.price = DATA[currentInd].val;
                        ordersLimit[currentProfitId] = orderLimitTmp;                                          
                    }
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }

        public TradeResults StrategyMA_AllGrow(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[5];
            periodsMA[0] = 180;
            periodsMA[1] = 360;
            periodsMA[2] = 720;

            periodsMA[3] = 10;
            periodsMA[4] = 60;
            //по 10 последних значений
            float[,] lastMA = new float[periodsMA.Length, 10];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];

            int indLevel = -1;
            float priceLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            string log = "";            
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }
                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                if (!isLongOpen)
                {                                        

                    bool buyCondition = false;

                    if (lastMA[1, 0] <= lastMA[2, 0] && lastMA[1, 1] > lastMA[2, 1])
                    {
                        is_allow_long = true;
                    }
                    //TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                    //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                    /*
                    log += "Check (" + currentInd.ToString() + "): 1=" +
                        (MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+ " \n";
                    */
                    
                    buyCondition =
                    //если все средние растут
                    DATA[currentInd].val > lastMA[3, 0] && DATA[currentInd-1].val > lastMA[3, 1]
                    && DATA[currentInd-2].val > lastMA[3, 2] && DATA[currentInd - 3].val > lastMA[3, 3]
                    && DATA[currentInd - 4].val > lastMA[3, 4] && DATA[currentInd - 5].val > lastMA[3, 5]
                    && DATA[currentInd - 6].val > lastMA[3, 6] && DATA[currentInd - 7].val > lastMA[3, 7]

                    && lastMA[3, 0] >= lastMA[3, 1] && lastMA[3, 1] >= lastMA[3, 2]
                    && lastMA[3, 2] >= lastMA[3, 3] && lastMA[3, 3] >= lastMA[3, 4]
                    && lastMA[3, 4] >= lastMA[3, 5] && lastMA[3, 5] >= lastMA[3, 6] 
                    && lastMA[3, 6] >= lastMA[3, 7]

                    && lastMA[4, 0] >= lastMA[4, 1] && lastMA[4, 1] >= lastMA[4, 2]
                    && lastMA[4, 2] >= lastMA[4, 3] && lastMA[4, 3] >= lastMA[4, 4]
                    && lastMA[4, 4] >= lastMA[4, 5] && lastMA[4, 5] >= lastMA[4, 6]
                    && lastMA[4, 6] >= lastMA[4, 7]

                    && lastMA[3, 0] >= lastMA[4, 0] && lastMA[3, 1] >= lastMA[4, 1]
                    && lastMA[3, 2] >= lastMA[4, 2] && lastMA[3, 3] >= lastMA[4, 3]
                    && lastMA[3, 4] >= lastMA[4, 4] && lastMA[3, 5] >= lastMA[4, 5]
                    && lastMA[3, 6] >= lastMA[4, 6] && lastMA[3, 7] >= lastMA[4, 7]

                    && lastMA[0, 0] >= lastMA[0, 1] && lastMA[0, 1] >= lastMA[0, 2]
                    && lastMA[1, 0] >= lastMA[1, 1] && lastMA[1, 1] >= lastMA[1, 2]
                    && lastMA[2, 0] >= lastMA[2, 1] && lastMA[2, 1] >= lastMA[2, 2]
                    
                    && MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val) < 0.25f
                    && MathProcess.DeltaProcent(lastMA[0, 0], DATA[currentInd].val) < 0.25f
                    && MathProcess.DeltaProcent(lastMA[2, 0], lastMA[0, 0]) < 0.25f
                    && MathProcess.DeltaProcent(lastMA[1, 0], lastMA[0, 0]) > 0.01f

                    
                    && MathProcess.DeltaProcent(lastMA[4, 1], lastMA[4, 0]) > 0.00025f
                    && MathProcess.DeltaProcent(lastMA[1, 1], lastMA[1, 0]) > 0.00025f
                    && MathProcess.DeltaProcent(lastMA[2, 1], lastMA[2, 0])> 0.00025f
                     
                    //оранжевая выше черной, черная выше зеленой и красной
                    && lastMA[3, 0] > lastMA[0, 0]&& lastMA[0, 0] > lastMA[1, 0]&& lastMA[0, 0] > lastMA[2, 0]
                    && lastMA[4, 0] > lastMA[0, 0] && lastMA[1, 0] > lastMA[2, 0]
                    ;

                    if (buyCondition && is_allow_long)
                    {
                        //log += "+ (" + currentInd.ToString() + "): 1=" +
                        //(MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val)).ToString()+"\n";
                        float price = DATA[currentInd].val+minPriceStep;
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price * 0.993f;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price * 1.01f;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка                            
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка                            
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            is_allow_long = false;
                        }
                    }
                    /*
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        //условие на закрытие длинной позиции
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                    }
                     */
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }


        public TradeResults StrategyMAFutures(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd < endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[4];
            periodsMA[0] = 180;
            periodsMA[1] = 360;
            periodsMA[2] = 720;

            periodsMA[3] = 10;
            //по 5 последних значений
            float[,] lastMA = new float[periodsMA.Length, 5];
            float[,] MAs = new float[periodsMA.Length, endInd - startTradeInd];

            int indLevel = -1;
            float priceLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            string log = "";
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }
                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                if (!isLongOpen)
                {

                    /*
                     * Черная пересекает зеленую - Уровень. 
                     * Покупаем когда цена пробивает Уровень снизу вверх и Красная если выше не падает 
                     * и прошло не менее 100 (N) от момента фиксации Уровня. Уровень снимается через 1500 (M) отсчетов.
                    */

                    //Фиксация уровня
                    bool levelFixation = lastMA[0, 0] > lastMA[1, 0]
                    && lastMA[0, 1] > lastMA[1, 1]                    
                    && lastMA[0, 2] <= lastMA[1, 2]
                    && lastMA[0, 3] < lastMA[1, 3]
                    //черная растет
                    && lastMA[0, 0] >= lastMA[0, 1]
                    //черная должна пересекать не горизонтально
                    && MathProcess.DeltaProcent(lastMA[0, 2], lastMA[0, 0]) >= 0.0005f
                    ;
                    if (levelFixation)
                    {
                        indLevel = currentInd - 3;
                        priceLevel = lastMA[0, 0];
                        //запоминаем значение MA2 для определения скорости падения
                        valFixMA2 = lastMA[2, 1];
                        //log += "indLevel=" + indLevel.ToString() + " priceLevel=" + priceLevel.ToString() + " \n";
                    }

                    //проверяем не в падающем ли тренде оказались после фиксации
                    //если да, то снимаем Уровень
                    if (indLevel > 0)
                    {
                        bool isDown =
                        lastMA[1, 0] < lastMA[2, 0] && lastMA[1, 1] >= lastMA[2, 1] && lastMA[1, 2] > lastMA[2, 2];
                        if (isDown)
                        {
                            indLevel = -1;
                            priceLevel = 0;
                        }
                    }

                    bool buyCondition = false;
                    //Если уровень зафиксирован, проверяем условие на покупку.                    

                    if (indLevel > 0)
                    {
                        TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                        //float minutes = (float)(diff.TotalSeconds / 60);//minutes
                        if ((currentInd - indLevel) > 100)
                        {
                            /*
                            log += "Check (" + currentInd.ToString() + "): 1=" +
                                (DATA[currentInd].val > priceLevel).ToString()
                                + " 2=" + (DATA[currentInd - 1].val >= priceLevel).ToString()
                                + " 3=" + (DATA[currentInd - 2].val < priceLevel).ToString()
                                + " 4=" + (MathProcess.DeltaProcent(DATA[currentInd].val, lastMA[2, 0]) > 0.02f).ToString()
                                + " 5=" + (Math.Abs(MathProcess.DeltaProcent(valFixMA2, lastMA[2, 0])) < 0.05f).ToString()
                                + " \n";
                            */ 
                            buyCondition =
                            DATA[currentInd].val > priceLevel
                            && DATA[currentInd - 1].val >= priceLevel
                            && DATA[currentInd - 2].val < priceLevel                            
                            //красная должна быть выше
                            && MathProcess.DeltaProcent(DATA[currentInd].val, lastMA[2, 0]) > 0.03f
                            //цена должна быть ниже или чуть выше зеленой
                            && MathProcess.DeltaProcent(lastMA[1, 0], DATA[currentInd].val) < 0.001f
                            //не падающий тренд - скорость падения MA2 небольшая
                            && Math.Abs(MathProcess.DeltaProcent(valFixMA2, lastMA[2, 0])) < 0.05f
                            ;
                        }
                        //Проверка на снятие Уровня
                        if (currentInd - indLevel > 2000)
                        {
                            indLevel = -1;
                            priceLevel = 0;
                        }
                    }

                    if (buyCondition)
                    {
                        indLevel = -1;
                        float price = DATA[currentInd].val + minPriceStep;
                        int volume = (int)Math.Floor(this.cash * 0.9f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price * 0.9995f;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price * 1.002f;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count - 2];//стоп установиться как только сработает заявка                            
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка                            
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            isLongOpen = true;
                        }
                    }
                    /*
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        //условие на закрытие длинной позиции
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                    }
                     */
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }


        public TradeResults StrategyMA(int startLearnInd, int endLearnInd, int endInd)
        {
            if (endInd <endLearnInd) endInd = this.DATA.Length;
            //DataProcess.MovingAverage(
            //3 скользящих средних различного периода
            int[] periodsMA = new int[3];
            periodsMA[0] = 180;
            periodsMA[1] = 360;
            periodsMA[2] = 720;
            //по 5 последних значений
            float[,] lastMA = new float[periodsMA.Length,5];
            float[,] MAs = new float[periodsMA.Length, endInd-startTradeInd];

            int indLevel = -1;
            float priceLevel = 0;
            float valFixMA2 = 0;

            //пошла торговля
            startTradeInd = endLearnInd + 1;
            string log = "";
            for (int currentInd = startTradeInd; currentInd < endInd; currentInd++)
            {
                if (cash <= 0 && securityPortfolio <= 0)
                {
                    break;
                }                
                //вычисляем последние значения скользящих средних
                for (int i = 0; i < lastMA.GetLength(0); i++)
                {
                    for (int j = 0; j < lastMA.GetLength(1); j++)
                    {
                        //при первом вычислении используем обычный метод
                        if (currentInd == startTradeInd && j==0)
                        {
                            lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                        }
                        //если уже есть вычисленные значения - используем для оптимизации
                        else if (j > 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNext(DATA, currentInd - j, periodsMA[i], lastMA[i, j - 1]);
                            //lastMA[i, j] = DataProcess.AveragePrice(DATA, currentInd - j, periodsMA[i]);
                            //
                        }
                        //на новом шаге используем предыдущее значение
                        else if (currentInd != startTradeInd && j == 0)
                        {
                            lastMA[i, j] = DataProcess.AveragePriceNextReverse(DATA, currentInd - j, periodsMA[i], lastMA[i, 0]);                            
                        }

                    }
                    MAs[i, currentInd - startTradeInd] = lastMA[i, 0];
                }
                if (!isLongOpen)
                {

                    /*
                     * Черная пересекает зеленую - Уровень. 
                     * Покупаем когда цена пробивает Уровень снизу вверх и Красная если выше не падает 
                     * и прошло не менее 500 (N) от момента фиксации Уровня. Уровень снимается через 1500 (M) отсчетов.
                    */

                    //Фиксация уровня
                    bool levelFixation = lastMA[0, 0] > lastMA[1, 0]
                    && lastMA[0, 1] > lastMA[1, 1]
                    //черная должна пересекать не горизонтально
                    && MathProcess.DeltaProcent(lastMA[0, 2], lastMA[0, 0])>=0.0005f
                    && lastMA[0, 2] <= lastMA[1, 2]
                    && lastMA[0, 3] < lastMA[1, 3]
                    && lastMA[0, 0] >= lastMA[0, 1]
                    //&& lastMA[0, 1] >= lastMA[0, 2]
                    //&& lastMA[0, 0] > lastMA[0, 2]
                    ;                    
                    if (levelFixation)
                    {
                        indLevel = currentInd - 3;                        
                        priceLevel = lastMA[0, 0] * 0.9995f;
                        //запоминаем значение MA2 для определения скорости падения
                        valFixMA2 = lastMA[2,1];
                        //log += "indLevel=" + indLevel.ToString() + " priceLevel=" + priceLevel.ToString() + " \n";
                    }

                    //проверяем не в падающем ли тренде оказались после фиксации
                    //если да, то снимаем Уровень
                    if (indLevel > 0)
                    {
                        bool isDown =
                            //(lastMA[0, 0] < lastMA[1, 0] && lastMA[0, 1] >= lastMA[1, 1] && lastMA[0, 2] > lastMA[1, 2])
                        //||
                        //(lastMA[0, 0] < lastMA[2, 0] && lastMA[0, 1] >= lastMA[2, 1] && lastMA[0, 2] > lastMA[2, 2]);
                        lastMA[1, 0] < lastMA[2, 0] && lastMA[1, 1] >= lastMA[2, 1] && lastMA[1, 2] > lastMA[2, 2];
                        if (isDown)
                        {
                            indLevel = -1;
                            priceLevel = 0;
                        }
                    }

                    bool buyCondition = false;
                    //Если уровень зафиксирован, проверяем условие на покупку.                    

                    if (indLevel > 0) {
                        TimeSpan diff = DATA[currentInd].dateTime.Subtract(DATA[indLevel].dateTime);
                        float minutes = (float)(diff.TotalSeconds / 60);//minutes
                        if ((minutes > 10 || (currentInd - indLevel) > 350) && minutes < 30) {
                            /*
                            log += "Check (" + currentInd.ToString() + "): 1=" +
                                (DATA[currentInd].val > priceLevel).ToString()
                                + " 2=" + (DATA[currentInd - 1].val >= priceLevel).ToString()
                                + " 3=" + (DATA[currentInd - 2].val < priceLevel).ToString()
                                + " 4=" + (lastMA[2, 0] > lastMA[0, 0]).ToString()
                                + " 5=" + (Math.Abs(MathProcess.DeltaProcent(valFixMA2, lastMA[2, 0])) < 0.1f).ToString()
                                + " \n";
                             */ 
                            buyCondition =
                            DATA[currentInd].val > priceLevel
                            && DATA[currentInd - 1].val >= priceLevel
                            && DATA[currentInd - 2].val < priceLevel
                            //красная должна быть выше
                            && MathProcess.DeltaProcent(DATA[currentInd].val, lastMA[2, 0]) > 0.08f
                            //цена должна быть ниже или чуть выше зеленой
                            && MathProcess.DeltaProcent(lastMA[1, 0], DATA[currentInd].val) < 0.01f
                            //не падающий тренд - скорость падения MA2 небольшая
                            && Math.Abs(MathProcess.DeltaProcent(valFixMA2, lastMA[2, 0])) < 0.05f
                            ;
                        }
                        //Проверка на снятие Уровня
                        if (currentInd - indLevel > 2000)
                        {
                            indLevel = -1;
                            priceLevel = 0;
                        }
                    }




                    //условие на покупку
                    /*
                     * Цена пересекает К, но не далеко от нее
                     * Все в росте
                     
                    bool addon_cond1 = true;
                    if (lastMA[1, 0]>lastMA[2, 0]&&lastMA[0, 0]>lastMA[2, 0]) {
                        addon_cond1 = lastMA[1, 0] >= lastMA[1, 1] &&  lastMA[0, 0] >= lastMA[0, 1];
                    }
                    bool buyCondition =
                        (DATA[currentInd].val>lastMA[2, 0]
                        && (MathProcess.DeltaProcent(lastMA[2, 0], DATA[currentInd].val) < 0.03f
                        && DATA[currentInd - 1].val > lastMA[2, 1]
                        && DATA[currentInd - 2].val > lastMA[2, 2]
                        && DATA[currentInd - 3].val > lastMA[2, 3]
                        && DATA[currentInd - 4].val < lastMA[2, 4])
                                                                                                  
                        && lastMA[2, 0] > lastMA[2, 1]
                        && lastMA[2, 1] > lastMA[2, 2]

                        && Math.Abs(MathProcess.DeltaProcent(lastMA[0, 0], lastMA[0, 1]))>0.00009f
                        && Math.Abs(MathProcess.DeltaProcent(lastMA[0, 1], lastMA[0, 2])) > 0.00009f
                        && Math.Abs(MathProcess.DeltaProcent(lastMA[1, 0], lastMA[1, 1])) > 0.00009f
                        && Math.Abs(MathProcess.DeltaProcent(lastMA[1, 1], lastMA[1, 2])) > 0.00009f

                        && !(MathProcess.DeltaProcent(lastMA[1, 0], DATA[currentInd].val)>0.001f
                            && lastMA[1, 0] < lastMA[1, 1]
                            && lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 0] < lastMA[0, 1])
                        
                        && addon_cond1

                        //&& DATA[currentInd].val > DATA[currentInd-1].val
                        //&& DATA[currentInd-1].val >= DATA[currentInd - 2].val                        
                        );                        
                    */

                    if (buyCondition) 
                    {
                        indLevel = -1;
                        float price = DATA[currentInd].val + minPriceStep;
                        int volume = (int)Math.Floor(this.cash * 0.9999f / price);
                        //float amountMoney = price * volume;
                        if (volume > 0)
                        {
                            //Стоп                            
                            OrderLimit newOrderLimitStop = new OrderLimit();
                            newOrderLimitStop.number = countOrder;
                            newOrderLimitStop.type = SELL;
                            newOrderLimitStop.price = price * 0.9965f;
                            newOrderLimitStop.volume = volume;
                            newOrderLimitStop.status = WAIT;
                            newOrderLimitStop.kind = STOP;
                            newOrderLimitStop.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitStop);
                            countOrder++;

                            //Профит                            
                            OrderLimit newOrderLimitProfit = new OrderLimit();
                            newOrderLimitProfit.number = countOrder;                            
                            newOrderLimitProfit.type = SELL;
                            newOrderLimitProfit.price = price * 1.002f;
                            newOrderLimitProfit.volume = volume;
                            newOrderLimitProfit.status = WAIT;
                            newOrderLimitProfit.kind = PROFIT;
                            newOrderLimitProfit.numOrderCondition = ordersCondition.Count;
                            ordersLimit.Add(newOrderLimitProfit);
                            countOrder++;

                            //Подготавливаем заявку
                            OrderCondition newOrderCondition = new OrderCondition();
                            newOrderCondition.number = countOrder;
                            newOrderCondition.timeNum = currentInd;
                            newOrderCondition.type = BUY;
                            newOrderCondition.price = price;
                            newOrderCondition.volume = volume;
                            newOrderCondition.date = DATA[currentInd].dateTime;                            
                            newOrderCondition.status = ACTIVE;
                            newOrderCondition.orderLimitStop = ordersLimit[ordersLimit.Count-2];//стоп установиться как только сработает заявка                            
                            newOrderCondition.orderLimitProfit = ordersLimit[ordersLimit.Count - 1];//стоп установиться как только сработает заявка                            
                            //добавляем заявку в список активных
                            ordersCondition.Add(newOrderCondition);
                            countOrder++;
                            isLongOpen = true;
                        }
                    }
                    /*
                    //если открыта длинная позиция
                    if (isLongOpen)
                    {
                        //условие на закрытие длинной позиции
                        bool closeLongPositionCondition = lastMA[0, 0] < DATA[currentInd].val
                            && lastMA[0, 1] >= DATA[currentInd].val
                            && lastMA[0, 2] > DATA[currentInd].val
                            && lastMA[0, 3] > DATA[currentInd].val;
                        if (closeLongPositionCondition)
                        {
                            
                        }
                    }
                     */                     
                }
                exchangeProcess(currentInd);
            }
            //DataProcess.ExportList(ordersLimitExecuted, "_executedOrders.csv");
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            float res_cash = cash + securityPortfolio * DATA[endInd - 1].val;
            report += "\nСчет:" + res_cash.ToString();
            report += log;
            OrderLimit[] executedOrders = (OrderLimit[])ordersLimitExecuted.ToArray();
            OrderCondition[] executedOrdersCond = (OrderCondition[])ordersConditionExecuted.ToArray();
            float[] cashValsArr = (float[])cashVals.ToArray();
            TradeResults res = new TradeResults();
            res.report = report;
            res.executedOrders = executedOrders;
            res.executedOrdersCond = executedOrdersCond;
            res.cashVals = cashValsArr;
            res.MAs = MAs;
            return res;
        }

        private void exchangeProcess(int currentInd)
        {            
            //обрабатываем все активные условные заявки
            for (int o = lastExecutedOrderCondition+1; o < ordersCondition.Count; o++)
            {
                if (ordersCondition[o].status == ACTIVE)
                {
                    //условие сделки                
                    if (ordersCondition[o].type == BUY 
                        && DATA[currentInd].val >= ordersCondition[o].price
                        && DATA[currentInd-1].val <= ordersCondition[o].price
                        )
                    {
                        //ordersCondition[o].type == BUY &&                                            
                        float volume = (int)Math.Floor(this.cash * 0.9999f / DATA[currentInd].val);
                        volume = DATA[currentInd].qval < volume ? DATA[currentInd].qval : volume;
                        float amountMoney = volume * DATA[currentInd].val;
                        if (volume > 0)
                        {
                            float commissionBroker = 0.5f;
                            //Вычитаем денежки из кошелька                            
                            this.cash -= ordersCondition[o].type * amountMoney;
                            this.securityPortfolio += ordersCondition[o].type * volume;
                            this.cash -= commissionBroker;

                            cashVals.Add(this.cash);

                            //ставим стоп и профит
                            OrderLimit orderLimitTmp = new OrderLimit();
                            int indOrder = ordersLimit.IndexOf(ordersCondition[o].orderLimitStop);
                            orderLimitTmp = ordersLimit[indOrder];
                            orderLimitTmp.status = ACTIVE;
                            ordersLimit[indOrder] = orderLimitTmp;
                            currentStopId = indOrder;

                            OrderCondition orderConditionTmp = new OrderCondition();
                            orderConditionTmp = ordersCondition[o];
                            orderConditionTmp.orderLimitStop = orderLimitTmp;
                            orderConditionTmp.status = EXECUTED;
                            orderConditionTmp.timeNum = currentInd;
                            orderConditionTmp.price = DATA[currentInd].val;


                            indOrder = ordersLimit.IndexOf(ordersCondition[o].orderLimitProfit);
                            orderLimitTmp = ordersLimit[indOrder];
                            orderLimitTmp.status = ACTIVE;
                            ordersLimit[indOrder] = orderLimitTmp;
                            currentProfitId = indOrder;

                            orderConditionTmp.orderLimitProfit = orderLimitTmp;
                            ordersCondition[o] = orderConditionTmp;

                            ordersConditionExecuted.Add(orderConditionTmp);

                            lastExecutedOrderCondition = o;
                            
                            isLongOpen = true;
                            isOrderConditionSet = false;
                            stopRaise = false;

                        }
                    }
                    //если условие покупки не настало - снимаем
                    else if (ordersCondition[o].type == BUY && (currentInd-ordersCondition[o].timeNum)>100)
                    {
                        OrderCondition orderConditionTmp = new OrderCondition();
                        orderConditionTmp = ordersCondition[o];
                        orderConditionTmp.status = CANCELED;
                        ordersCondition[o] = orderConditionTmp;
                        lastExecutedOrderCondition = o;

                        //и снимаем установленные лимитные ордера
                        //стоп
                        OrderLimit orderLimitTmp = new OrderLimit();
                        int indOrder = ordersLimit.IndexOf(ordersCondition[o].orderLimitStop);
                        orderLimitTmp = ordersLimit[indOrder];
                        orderLimitTmp.status = CANCELED;
                        ordersLimit[indOrder] = orderLimitTmp;
                        currentStopId = 0;

                        //профит
                        indOrder = ordersLimit.IndexOf(ordersCondition[o].orderLimitProfit);
                        orderLimitTmp = ordersLimit[indOrder];
                        orderLimitTmp.status = CANCELED;
                        ordersLimit[indOrder] = orderLimitTmp;
                        currentProfitId = 0;

                        isLongOpen = false;
                        isOrderConditionSet = false;
                        //is_allow_long = true;
                    }                    
                }
            }
            //обрабатываем все обычные лимитные заявки
            for (int o = 0; o < ordersLimit.Count; o++)
            {
                if (ordersLimit[o].status == ACTIVE)
                {
                    //условие сделки                
                    if (ordersLimit[o].type == SELL && 
                            (
                                (ordersLimit[o].kind == STOP 
                                && DATA[currentInd].val <= ordersLimit[o].price
                                && DATA[currentInd-1].val >= ordersLimit[o].price
                                )
                                ||
                                (ordersLimit[o].kind == PROFIT 
                                && DATA[currentInd].val >= ordersLimit[o].price
                                && DATA[currentInd-1].val <= ordersLimit[o].price
                                )
                            )
                        )
                    {
                        float volume = DATA[currentInd].qval < this.securityPortfolio ? DATA[currentInd].qval : this.securityPortfolio;
                        if (volume > 0)
                        {
                            float amountMoney = volume * DATA[currentInd].val;
                            /*
                            if (ordersLimit[o].kind == PROFIT)
                            {
                                is_allow_long = false;
                            }
                            else
                            {
                                is_allow_long = true;
                            }
                            */
                            float commissionBroker = 0.5f;
                            //Вычитаем денежки из кошелька                            
                            this.cash -= ordersLimit[o].type * amountMoney;
                            this.securityPortfolio += ordersLimit[o].type * volume;
                            this.cash -= commissionBroker;
                            
                            //Снимаем парную заявку
                            int indExecuted = -1;
                            int indNotExecuted = -1;
                            if (ordersLimit[o].kind == STOP)
                            {
                                indExecuted = currentStopId;
                                indNotExecuted = currentProfitId;
                            }
                            else
                            {
                                indExecuted = currentProfitId;
                                indNotExecuted = currentStopId;
                            }

                            OrderLimit orderLimitTmp = new OrderLimit();
                            orderLimitTmp = ordersLimit[indExecuted];
                            orderLimitTmp.timeNum = currentInd;
                            orderLimitTmp.price = DATA[currentInd].val;
                            ordersLimit[indExecuted] = orderLimitTmp;

                            ordersLimitExecuted.Add(ordersLimit[indExecuted]);
                            ordersLimit.Remove(ordersLimit[indExecuted]);
                            int correctInd = 0;
                            if (indExecuted < indNotExecuted)
                            {
                                correctInd = 1;
                            }
                            ordersLimit.Remove(ordersLimit[indNotExecuted - correctInd]);                            
                            cashVals.Add(this.cash);
                            isLongOpen = false;
                            indLevel = 0;
                            indLevel2 = 0;
                        }
                    }
                }
            }
        }


        /*
        /// <summary>
        /// Стратегия основана на статистике уровней досижения при пороговом анализе изменения цены
        /// </summary>
        /// <param name="startLearnInd"></param>
        /// <param name="endLearnInd"></param>
        /// <returns></returns>
        public string Strategy1(int startLearnInd, int endLearnInd)
        {
            PointLocalExtremum[] Extremums0 = DataProcess.GetLocalExtremums(2f, DATA, startLearnInd, endLearnInd);
            DataProcess.ExportArray(Extremums0, "_extremumStatistic.csv");
            return "Done";
            //Получаем статистику по окну [startLearnInd; endLearnInd]
            STATISTICS = DataProcess.GetLocalExtremumStatistic(DATA, startLearnInd, endLearnInd, 0.6f, 2f, 0.2f, 0.4f, 0.05f, 85f);
            DataProcess.ExportArray(STATISTICS, "_statistic.csv");
            int maxProbabilityInd = 0;
            for (int i = 0; i < STATISTICS.Length; i++)
            {
                if (STATISTICS[i].probability > STATISTICS[maxProbabilityInd].probability) maxProbabilityInd = i;
            }
            LocalExtremumStatistic[] STATISTICS2 = DataProcess.GetLocalExtremumStatistic(DATA, endLearnInd + 1, DATA.Length - 1, 0.6f, 2f, 0.2f, 0.4f, 0.05f, 85f);
            DataProcess.ExportArray(STATISTICS2, "_statistic2.csv");
            //экстремумы с самым маленьким порогом
            float threshold = STATISTICS[maxProbabilityInd].startThreshold;
            float lengthChanging = STATISTICS[maxProbabilityInd].lengthChanging / 100;
            PointLocalExtremum[] Extremums = DataProcess.GetLocalExtremums(threshold, DATA, startLearnInd, endLearnInd);
            for (int i = 0; i < Extremums.Length; i++)
            {
                EXTREMUMS.Add(Extremums[i]);
            }
            
            sumCredit = 0;
            startCredit = DateTime.Now;

            float qvalSum = 0;
            PointLocalExtremum b = new PointLocalExtremum();                        
            float delta;

            int lastIndex = EXTREMUMS.Count - 1;
            int lastSign = (int) (EXTREMUMS[lastIndex].val - EXTREMUMS[lastIndex - 1].val);
            float localExtremum = EXTREMUMS[lastIndex].val;
            int localExtremumInd = lastIndex;

            //ищем локальный экстремум (или макс/мин точку) между lastIndex и currentInd, 
            //противоположную по направлению lastSign
            //Если последний был максимум, ищем минимум
            if (lastSign > 0)
            {
                for (int l = localExtremumInd; l <= endLearnInd; l++)
                {
                    if (DATA[l].val < localExtremum)
                    {
                        localExtremum = DATA[l].val;
                        localExtremumInd = l;
                    }
                }
            }
            else
            {
                for (int l = localExtremumInd; l <= endLearnInd; l++)
                {
                    if (DATA[l].val > localExtremum)
                    {
                        localExtremum = DATA[l].val;
                        localExtremumInd = l;
                    }
                }
            }            
            
            //пошла торговля
            startTradeInd = endLearnInd + 1;
            for (int currentInd = startTradeInd; currentInd < this.DATA.Length; currentInd++)
            {
                if (lastSign > 0)
                {
                    if (DATA[currentInd].val < localExtremum)
                    {
                        localExtremum = DATA[currentInd].val;
                        localExtremumInd = currentInd;
                    }
                }
                else
                {
                    if (DATA[currentInd].val > localExtremum)
                    {
                        localExtremum = DATA[currentInd].val;
                        localExtremumInd = currentInd;
                    }
                }
                delta = MathProcess.DeltaProcent(localExtremum, DATA[currentInd].val);
                int signDelta = Math.Sign(delta);
                if (Math.Abs(delta) >= threshold)
                {
                    b.val = localExtremum;
                    b.indexOrg = localExtremumInd;
                    EXTREMUMS.Add(b);
                    lastIndex = EXTREMUMS.Count - 1;
                    lastSign = (int)(EXTREMUMS[lastIndex].val - EXTREMUMS[lastIndex - 1].val);                    
                    localExtremum = DATA[currentInd].val;
                    localExtremumInd = currentInd;
                }                
                if (!isPositionOpen && Math.Abs(MathProcess.DeltaProcent(Math.Abs(delta), threshold)) <= 2 && Math.Abs(delta) <= threshold)
                {                    
                    float price = DATA[currentInd].val + minPriceStep * 2 * signDelta;
                    //float thresholdOpen = (signDelta > 0) ? STATISTICS[0].averageReturnUp : STATISTICS[0].averageReturnDown;
                    ////float priceCondition = localExtremum + signDelta * (1 + thresholdOpen / 100 - minPriceStep * 5);
                    //float priceCondition = price - signDelta * minPriceStep * 5;
                    int volume = (int)Math.Floor(this.cash * 0.9999f / price);
                    float amountMoney = price * volume;
                    if (countOpenOrders == 0 && volume > 0 && (signDelta > 0 && cash > 0 && amountMoney <= cash || signDelta < 0 && sumCredit <= 1 && (securityPortfolio * price) + cash >= amountMoney))
                    {
                        //Подготавливаем заявку
                        ActiveOrder newOrder = new ActiveOrder();
                        newOrder.number = countOrder;
                        newOrder.type = signDelta;
                        newOrder.price = price;
                        //на все деньги
                        newOrder.volume = volume;
                        newOrder.date = DATA[currentInd].dateTime;
                        newOrder.kind = 0;
                        newOrder.numerLinkOrder = -1;
                        //newOrder.priceCondition = newOrder.price - signDelta * minPriceStep * 10;
                        newOrder.priceCondition = localExtremum;
                        newOrder.isActive = true;
                        //добавляем заявку в список активных
                        orders .Add(newOrder);
                        countOpenOrders++;
                        countOrder++;
                    }
                }
                serverProcess(currentInd);
            }
            DataProcess.ExportList(executedOrders, "_executedOrders.csv");
            DataProcess.ExportList(EXTREMUMS, "_EXTREMUMS.csv");            
            string report = "Done. countOrder = " + countOrder.ToString();
            report += "\n Начало торговли: " + DATA[startTradeInd].dateTimeStr;
            return report;
        }


        /// <summary>
        /// Возвращает список индексов (в EXTREMUMS) достигутых уровней точкой с индексом ind
        /// </summary>
        /// <param name="ind">Индекс точки</param>
        /// <returns></returns>
        private ReachThresholds _getReachThresholds(int ind, int maxAmountThresholds)
        {
            List<OptimumThreshold> list = new List<OptimumThreshold>();            
            int i = EXTREMUMS.Count - 1;
            int countThresholds = 0;
            float maxProbability = 0;
            int sign = 0;
            while (i > 0)
            {
                float delta = MathProcess.DeltaProcent(DATA[ind].val, EXTREMUMS[i].val);
                sign = Math.Sign(delta);
                //проверяем изменение на все пороги
                for (int e = 0; e < STATISTICS.Length; e++)
                {
                    float deltaThreshold = Math.Abs(delta) - STATISTICS[e].startThreshold;
                    if (Math.Abs(deltaThreshold) < 0.0005 && i > 0)
                    {
                        OptimumThreshold ot = new OptimumThreshold();
                        ot.indStatistic = e;
                        ot.price = EXTREMUMS[i].val;
                        list.Add(ot);                        
                        maxProbability = STATISTICS[e].probability > maxProbability ? STATISTICS[e].probability : maxProbability;
                        countThresholds++;
                        if (countThresholds == maxAmountThresholds) goto ExitLabel;
                        i -= 2;
                    }                    
                }                
                i -= 1;
            }
            ExitLabel:
            ReachThresholds res = new ReachThresholds();
            res.list = list;            
            res.maxProbability = maxProbability;
            res.sign = sign;
            return res;
        }

        /// <summary>
        /// Определяет индекс используемого порога из STATISTICS
        /// Оптимум ищется среди порогов с максимальной вероятностью, с наибольшей длиной 
        /// дальнейшего движения
        /// </summary>
        /// <param name="listReachThresholds">Список достигнутых порогов</param>
        /// <returns>индекс используемого порога из STATISTICS</returns>
        private OptimumThreshold _getOptimumThreshold(ReachThresholds reachThresholds)
        {            
            //Ищем максимальную длину изменения среди максимумов
            float maxDelta = 0;
            for (int i = 0; i < reachThresholds.list.Count; i++)
            {
                int ind = reachThresholds.list[i].indStatistic;
                if (STATISTICS[ind].probability == reachThresholds.maxProbability)
                {
                    maxDelta = STATISTICS[ind].lengthChanging > maxDelta ? STATISTICS[ind].lengthChanging : maxDelta;
                }
            }
            //ищем среди максимально вероятных и максимально длинных изменений
            //минимальное среднее время достижения u
            float minTime = 1000000000000;
            //вспомогательный список для индексов максимально вероятных и максимально длинных изменений
            List<OptimumThreshold> list = new List<OptimumThreshold>();
            OptimumThreshold res = new OptimumThreshold();
            //Если точка на палающем тренде            
            if (0 < 0)
            {
                for (int i = 0; i < reachThresholds.list.Count; i++)
                {                    
                    int ind = reachThresholds.list[i].indStatistic;
                    if (STATISTICS[ind].probability == reachThresholds.maxProbability && STATISTICS[ind].lengthChanging == maxDelta)
                    {
                        minTime = STATISTICS[ind].averageTimeUDown < minTime ? STATISTICS[ind].averageTimeUDown : minTime;
                        OptimumThreshold ot = new OptimumThreshold();
                        ot.indStatistic = ind;
                        ot.price = reachThresholds.list[i].price;
                        list.Add(ot);
                    }
                }
                foreach (OptimumThreshold ot in list)
                {
                    if (STATISTICS[ot.indStatistic].averageTimeUDown == minTime)
                    {
                        return ot;
                    }
                }
            }
            //если точка на растущем тренде
            else
            {
                for (int i = 0; i < reachThresholds.list.Count; i++)
                {
                    int ind = reachThresholds.list[i].indStatistic;
                    if (STATISTICS[ind].probability == reachThresholds.maxProbability && STATISTICS[ind].lengthChanging == maxDelta)
                    {
                        minTime = STATISTICS[ind].averageTimeUUp < minTime ? STATISTICS[ind].averageTimeUUp : minTime;
                        OptimumThreshold ot = new OptimumThreshold();
                        ot.indStatistic = ind;
                        ot.price = reachThresholds.list[i].price;
                        list.Add(ot);
                    }
                }
                foreach (OptimumThreshold ot in list)
                {
                    if (STATISTICS[ot.indStatistic].averageTimeUUp == minTime)
                    {
                        return ot;
                    }
                }
            }
            return res;
        }

        private void serverProcess(int currentInd)
        {
            //обрабатываем все активные заявки
            for (int o = 0; o < activeOrders.Count; o++)
            {
                if (!activeOrders[o].isActive)
                {
                    if (activeOrders[o].type * (DATA[currentInd].val - activeOrders[o].priceCondition) < 0)
                    {
                        ActiveOrder activeOrderTmp = new ActiveOrder();
                        activeOrderTmp = activeOrders[o];
                        activeOrderTmp.isActive = true;
                        activeOrders[o] = activeOrderTmp;
                    }
                    else
                    {
                        TimeSpan diff = DATA[currentInd].dateTime.Subtract(activeOrders[o].date);
                        if (diff.TotalDays > 3)
                        {
                            //countOpenOrders--;
                            //deletedOrders.Add(o);                                
                        }
                    }
                }
                bool condition = activeOrders[o].kind == 0 && activeOrders[o].isActive && activeOrders[o].type * (DATA[currentInd].val - activeOrders[o].price) >= 0;
                condition = condition || (activeOrders[o].kind == 2 && activeOrders[o].type * (activeOrders[o].price - DATA[currentInd].val) >= 0);
                condition = condition || (activeOrders[o].kind == 1 && activeOrders[o].type * (activeOrders[o].price - DATA[currentInd].val) <= 0);
                if (condition)
                {
                    float volume = DATA[currentInd].qval < activeOrders[o].volume ? DATA[currentInd].qval : activeOrders[o].volume;
                    float amountMoney = volume * DATA[currentInd].val;
                    if (volume > 0)
                    {
                        //Оформляем выполненную заявку
                        ExecutedOrder executedOrder = new ExecutedOrder();
                        executedOrder.number = activeOrders[o].number;
                        executedOrder.price = DATA[currentInd].val;
                        executedOrder.dateSet = activeOrders[o].date;
                        executedOrder.dateExecuted = DATA[currentInd].dateTime;
                        executedOrder.type = activeOrders[o].type;
                        executedOrder.volume = volume;
                        executedOrder.kind = activeOrders[o].kind;
                        //если продаем акции которых нет, значит берем кредит
                        if (executedOrder.type < 0 && securityPortfolio < volume)
                        {
                            startCredit = executedOrder.dateSet;
                            //сумма кредита
                            sumCredit = (volume - securityPortfolio) * executedOrder.price;
                        }
                        float commissionBroker = amountMoney * 0.00033f;
                        //Вычитаем денежки из кошелька                            
                        this.cash -= executedOrder.type * amountMoney;
                        this.securityPortfolio += executedOrder.type * volume;
                        //0.028% в день за пользование кредита
                        this.commission += commissionBroker;
                        this.cash -= commissionBroker;

                        //если мы брали акции в кредит и теперь должны за них вернуть
                        if (securityPortfolio == 0 && executedOrder.type > 0)
                        {
                            DateTime timeCredit = DATA[currentInd].dateTime;
                            TimeSpan diff = DATA[currentInd].dateTime.Subtract(startCredit);
                            commissionBroker = (float)Math.Ceiling(diff.TotalDays) * 0.00028f * sumCredit;
                            sumCredit = 0;
                            commission += commissionBroker;
                            cash -= commissionBroker;
                        }
                        executedOrder.profit = cash;
                        executedOrder.sumCredit = sumCredit;

                        if (activeOrders[o].numerLinkOrder >= 0)
                        {
                            for (int z = 0; z < activeOrders.Count; z++)
                            {
                                if (activeOrders[o].numerLinkOrder == activeOrders[z].number) deletedOrders.Add(activeOrders[z].number);
                            }
                            List<int> deletedProfitOrders = new List<int>();
                            for (int z = 0; z < activeProfitOrders.Count; z++)
                            {
                                if (activeProfitOrders[z].number == activeOrders[o].numerLinkOrder)
                                {
                                    deletedProfitOrders.Add(activeProfitOrders[z].number);
                                }
                            }
                            for (int i = 0; i < deletedProfitOrders.Count; i++)
                            {
                                for (int j = 0; j < activeProfitOrders.Count; j++)
                                {
                                    if (activeProfitOrders[j].number == deletedProfitOrders[i])
                                    {
                                        activeProfitOrders.RemoveAt(j);
                                        break;
                                    }
                                }
                            }
                        }
                        //если обычная заявка ставим стоп и профит
                        if (executedOrder.kind == 0)
                        {
                            countOpenOrders--;
                            isPositionOpen = true;
                            //подготавливаем соответствующую профит-заявку

                            /*
                            ActiveProfitOrder newProfitOrder = new ActiveProfitOrder();
                            newProfitOrder.number = countOrder;
                            newProfitOrder.type = -1 * executedOrder.type;
                            //стоп-цена при которой начнет проверяться условие                        
                            newProfitOrder.priceCondition = executedOrder.price * (1 + lengthChanging * executedOrder.type);
                            //STATISTICS[optimalThresholdIndex].lengthChanging
                            newProfitOrder.date = executedOrder.dateExecuted;
                            newProfitOrder.volume = (int)executedOrder.volume;
                            //максимальный откат в процентах
                            newProfitOrder.maxReturnProcent = 0.05f;
                            //защитный спред в абсолютных значениях цены
                            newProfitOrder.spred = minPriceStep;
                            //служебная запись для отслеживания максимума
                            newProfitOrder.maxPrice = newProfitOrder.priceCondition;
                            //стоп-условие еще не выполнено
                            newProfitOrder.status = 0;                                
                            countOrder++;
                            

                            ActiveOrder newProfitOrder = new ActiveOrder();
                            newProfitOrder.number = countOrder;
                            newProfitOrder.type = -1 * executedOrder.type;
                            newProfitOrder.price = executedOrder.price * (1 + 0.005f * executedOrder.type);
                            newProfitOrder.date = executedOrder.dateExecuted;
                            newProfitOrder.volume = (int)executedOrder.volume;
                            newProfitOrder.kind = 2;
                            newProfitOrder.numerLinkOrder = -1;
                            newProfitOrder.isActive = true;
                            countOrder++;

                            //Подготавливаем стоп-заявку
                            ActiveOrder stopOrder = new ActiveOrder();
                            stopOrder.number = countOrder;
                            //float maxReturn = executedOrder.type > 0 ? STATISTICS[maxProbabilityInd].maxReturnUp / 100 : STATISTICS[maxProbabilityInd].maxReturnDown / 100;
                            //stopOrder.price = activeOrders[o].priceCondition * (1 + maxReturn * executedOrder.type);
                            stopOrder.price = executedOrder.price - minPriceStep * 4 * executedOrder.type;
                            //stopOrder.price = localExtremum + (minPriceStep * 5 * executedOrder.type);
                            stopOrder.volume = (int)executedOrder.volume;
                            stopOrder.type = -1 * executedOrder.type;
                            stopOrder.date = executedOrder.dateExecuted;
                            stopOrder.numerLinkOrder = newProfitOrder.number;
                            stopOrder.kind = 1;
                            stopOrder.isActive = true;
                            countOrder++;
                            newProfitOrder.numerLinkOrder = stopOrder.number;
                            //добавляем стоп-заявку в очередь
                            activeOrders.Add(stopOrder);
                            //добавляем заявку в список активных
                            //activeProfitOrders.Add(newProfitOrder);
                            activeOrders.Add(newProfitOrder);
                            executedOrder.numerLinkOrder = stopOrder.number;
                        }
                        else
                        {
                            isPositionOpen = false;
                        }
                        //Добавляем в список выполненных заявок
                        executedOrders.Add(executedOrder);
                        //Удаляем активную заявку
                        deletedOrders.Add(activeOrders[o].number);
                    }
                }
            }
            for (int i = 0; i < deletedOrders.Count; i++)
            {
                for (int j = 0; j < activeOrders.Count; j++)
                {
                    if (activeOrders[j].number == deletedOrders[i])
                    {
                        activeOrders.RemoveAt(j);
                        break;
                    }
                }
            }
            deletedOrders.Clear();

            /*
            //обрабатываем все активные стоп-заявки
            for (int o = 0; o < activeStopOrders.Count; o++)
            {
                ActiveStopOrder activeStopOrder = activeStopOrders[o];
                //Если выполняется условие стопа
                if ((DATA[currentInd].val - activeStopOrder.priceCondition) * activeStopOrder.type >= 0)
                {
                    //Генерим новую заявку
                    ActiveOrder newOrder = new ActiveOrder();
                    newOrder.number = countOrder;
                    newOrder.type = activeStopOrder.type;
                    newOrder.price = activeStopOrder.price;
                    newOrder.date = DATA[currentInd].dateTime;
                    newOrder.volume = activeStopOrder.volume;
                    newOrder.kind = 1;
                    newOrder.numerLinkOrder = activeStopOrder.numerLinkOrder;
                    //добавляем заявку в список активных
                    activeOrders.Add(newOrder);
                    countOrder++;
                    //Удаляем активную стоп-заявку
                    activeStopOrders.Remove(activeStopOrder);
                }
            }
            
            //обрабатываем все активные тейк-профит заявки
            for (int o = 0; o < activeProfitOrders.Count; o++)
            {
                ActiveProfitOrder activeProfitOrder = activeProfitOrders[o];
                //Если выполняется условие стопа
                if ((DATA[currentInd].val - activeProfitOrder.priceCondition) * activeProfitOrder.type < 0)
                {
                    float maxPrice = activeProfitOrder.type * (DATA[currentInd].val - activeProfitOrder.maxPrice) < 0 ? DATA[currentInd].val : activeProfitOrder.maxPrice;
                    ActiveProfitOrder activeProfitOrderTmp = new ActiveProfitOrder();
                    activeProfitOrderTmp = activeProfitOrders[o];
                    activeProfitOrderTmp.status = 1;
                    activeProfitOrderTmp.maxPrice = maxPrice;
                    activeProfitOrders[o] = activeProfitOrderTmp;
                    activeProfitOrder = activeProfitOrderTmp;
                }
                //Если стоп-условие сработало и сработало условие установки заявки на профит
                float delta = MathProcess.DeltaProcent(activeProfitOrder.maxPrice, DATA[currentInd].val);
                if (activeProfitOrder.status == 1 && Math.Sign(delta) == activeProfitOrder.type && Math.Abs(delta) >= activeProfitOrder.maxReturnProcent)
                {
                    //Генерим новую заявку
                    ActiveOrder newOrder = new ActiveOrder();
                    newOrder.number = activeProfitOrder.number;
                    newOrder.type = activeProfitOrder.type;
                    newOrder.price = DATA[currentInd].val + activeProfitOrder.type * activeProfitOrder.spred * 2;
                    newOrder.date = DATA[currentInd].dateTime;
                    newOrder.volume = activeProfitOrder.volume;
                    newOrder.kind = 2;
                    newOrder.isActive = true;
                    newOrder.numerLinkOrder = activeProfitOrder.numerLinkOrder;
                    //добавляем заявку в список активных
                    activeOrders.Add(newOrder);
                    countOrder++;
                    //удаляем тейк-профит заявку
                    //activeProfitOrders.Remove(activeProfitOrder);
                    deletedOrders.Add(activeProfitOrder.number);
                }
            }
            for (int i = 0; i < deletedOrders.Count; i++)
            {
                for (int j = 0; j < activeProfitOrders.Count; j++)
                {
                    if (activeProfitOrders[j].number == deletedOrders[i])
                    {
                        activeProfitOrders.RemoveAt(j);
                        break;
                    }
                }
            }
            deletedOrders.Clear();
        }
         */
    }
}
