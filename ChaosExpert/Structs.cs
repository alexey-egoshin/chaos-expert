using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace ChaosExpert
{
    [System.Serializable()]
    public struct Point
    {
        public DateTime dateTime;
        public string dateTimeStr;
        public float val;//значение величины
        public float qval;//значение качества (объем сделок например)
    }

    public struct PointTimeThresholdChange
    {
        public float time;//
        public string dateTimeStr;
        public float val;
        public float qval;
        public float original;//значение из исходного массива A
    }

    public struct PointTimeThresholdChangeFloat
    {
        public float time;//
        public string dateTimeStr;
        public float val;
        public float qval;
        public float original;//значение из исходного массива A
    }

    public struct PointTimeThresholdChangeSimple
    {
        public int ind;//индекс значения
        public int time;//число отсчетов
        public float val;//Значение сигнала
    }

    //Локальные экстремумы, которые отличаются друг от друга минимум на определенный порог
    public struct PointLocalExtremum
    {
        public float val;//цена в данной точке
        public string dateTimeStr;//дата в данной точке
        public float qval;//накопленный объем от предпоследней точки до этой
        public int count;//кол-во сделок между этой и предпоследней точками
        public float time;//время после последней точки (мин)
        public float timeClear;//чистое время после последней точки (не считая перерывов между торгами)
        public float timeThrehold;//время, за которое достигается порог
        public float timeThreholdClear;//время, за которое достигается порог (не считая перерывов между торгами)
        //максимальный возврат цены при движении от одного локального экстремума до другого.
        //Считается в % от первого локального экстремума
        public float maxReturn;
        public float averageReturn;//средний откат в % от первого локальн. экстр.
        public int indexOrg;//индекс точки в исходном массиве
    }

    public struct ReachThresholds
    {
        public List<OptimumThreshold> list;
        public float maxProbability;
        public int sign;
    }

    public struct OptimumThreshold
    {
        public int indStatistic;
        public float price;
    }

    public struct LocalExtremumStatistic
    {
        public byte type;//1 - не ушли ниже открытия позиции (точка стартового порога), 0 - ушли ниже

        public float startThreshold;//стартовый порог
        public float endThreshold;//конечный порог
        public float lengthChanging;//длина изменения (конечный порог - стартовый порог) %
        public float probability;//вероятность достижения конечного порога после преодоления стартового порога в %
        public float probabilityWithoutRecoil;//вероятность достижения конечного порога после преодоления стартового порога в % без откатов ниже точки стартового порога
        public float minTime;//минимальное время, которое требуется для достижения конечного порога после преодоления стартового (в отсчетах)
        public float averageTime;//среднее время        
        public float maxTime;//максимальное время
        public int amount;//кол-во таких случаев
        
        public int amountUp;//кол-во случаев достижения u при движении вверх
        public int amountDUp;//кол-во случаев НЕ достижения u при движении вверх

        public float maxReturnUp;//максимальный возврат цены при движении от стартового порога до конечного вверх. Считается в % от первого локального экстремума
        public float averageReturnUp;//средний откат в % от первого локальн. экстр. при движениях вверх
        
        public float minTimeDUp;//минимальное время достижения порога d при движении вверх
        public float averageTimeDUp;//среднее время достижения порога d при движении вверх
        public float maxTimeDUp;//максимальное время достижения порога d при движении вверх        
        
        public float averageTimeDUpWithoutRecoil;//среднее время достижения порога d при движении вверх при котором достигается u без откатов ниже d
        public float averageTimeDUpRecoil;//среднее время достижения порога d при движении вверх при котором НЕ достигается u без откатов ниже d или не достигается вообще
        public float dispersionTimeDUpWithoutRecoil;//дисперсия времени достижения порога d при движении вверх с последующим достижением u без откатов ниже стартового порога (точки открытия позиции)
        public float dispersionTimeDUpRecoil;//дисперсия времени достижения порога d при движении вверх с последующим НЕ достижением u без откатов ниже стартового порога (точки открытия позиции). Т.е. либо не достижение, либо откаты ниже стартового порога
        public int[] timeDUpRecoil;//значения времени достижения d при достижении u без откатов ниже d
        public int[] timeDUpWithoutRecoil;//значения времени достижения d при достижении u (и не достижении) с откатами ниже d
        public int amountUpWithoutRecoil;//число достижений u без откатов ниже d при движении вверх
        public int amountUpRecoil;//число не достижений u и достижений u с откатами ниже d при движении вверх

        public float minTimeUUp;//минимальное время достижения порога u при движении вверх (от d)
        public float averageTimeUUp;//среднее время достижения порога u при движении вверх
        public float maxTimeUUp;//максимальное время достижения порога u при движении вверх                

        public int amountGetUAfterReturnUp;//Кол-во случаев достижения от d до u при которых цена вначале откатывается ниже d при движении вверх
        public int amountReturnDUp;//кол-во откатов ниже d при движении вверх
        public int amountReturnUp;//кол-во откатов при движении вверх

        public int amountDown;//кол-во случаев достижения u при движении вниз
        public int amountDDown;//кол-во случаев НЕ достижения u при движении вниз
        public float maxReturnDown;//максимальный откат при движении вниз (т.е. откат вверх)
        public float averageReturnDown;//средний откат в % от первого локальн. экстр. при движениях вниз

        public float minTimeDDown;//максимальное время достижения порога d при движении вниз
        public float averageTimeDDown;//среднее время достижения порога d при движении вниз
        public float maxTimeDDown;//максимальное время достижения порога d при движении вниз
        public float averageTimeDDownWithoutRecoil;//среднее время достижения порога d при движении вниз при котором достигается u без откатов ниже d
        public float averageTimeDDownRecoil;//среднее время достижения порога d при движении вниз при котором НЕ достигается u без откатов ниже d или не достигается вообще
        public float dispersionTimeDDownWithoutRecoil;//дисперсия времени достижения порога d при движении вниз  с последующим достижением u без откатов ниже стартового порога (точки открытия позиции)
        public float dispersionTimeDDownRecoil;//дисперсия времени достижения порога d при движении вниз с последующим НЕ достижением u без откатов ниже стартового порога (точки открытия позиции). Т.е. либо не достижение, либо откаты ниже стартового порога
        public int[] timeDDownRecoil;//значения времени достижения d при достижении u без откатов ниже d
        public int[] timeDDownWithoutRecoil;//значения времени достижения d при достижении u (и не достижении) с откатами ниже d
        public int amountDownWithoutRecoil;//число достижений u без откатов ниже d при движении вниз
        public int amountDownRecoil;//число не достижений u и достижений u с откатами ниже d при движении вниз

        public float minTimeUDown;//максимальное время достижения порога u при движении вниз (от d)
        public float averageTimeUDown;//среднее время достижения порога u при движении вниз
        public float maxTimeUDown;//максимальное время достижения порога u при движении вниз
                
        public int amountGetUAfterReturnDown;//Кол-во случаев достижения от d до u при которых цена вначале откатывается выше d при движении вниз        
    }

    /// <summary>
    /// Точка пробития подтверждающего порога с соответствующей инфой (см. статью мою)
    /// </summary>
    public struct DisruptionPoint
    {
        public int type;//0 - ложный, 1 - настоящий порог
        public int indConfirmativeThreshold;//Индекс подтверждающего порога       
        public int indExtremum;//Индекс экстремума (истинного/ложного)
        public int indPreviousExtremum;//Индекс предпоследнего экстремума (истинного)
        public int indThreshold;//Индекс основного порога
        public int indPreviousPseudoExtremum;//индекс предпоследнего экстремума (истинного/ложного)
    }

    public struct PointRS
    {
        public double rs;// Log(R/S)
        public double n;// Log(количество отсчетов (длина ряда))
        public double h;// показатель Херста
        public double h0;// показатель Херста без логарифмов
    }

    /// <summary>
    /// Коэффициенты простой регрессии y = Ax+B
    /// </summary>
    public struct KRegression
    {
        public double A;
        public double B;
    }

    public struct KRegressionFloat
    {
        public float A;
        public float B;
    }

    public struct ResultRS
    {
        public PointRS[] points;
        public KRegression regression;
    }

    //Результаты расчета корр. размерности 
    public struct CorrelatingDimensionResult
    {
        public float[,] points;
        //Оценка корр. размерности
        public float correlatingDimension;
        //Максималльная размерность системы
        public int maxDimension;
        //Стартовая размерность
        public int startDimension;
        //Конечная размерность
        public int endDimension;
        //Размерность вложения, при которой количество самопересечений траектории = 0
        public int dimension_zero;
        //Последняя размерность вложения
        public int lastDimension;
        //Глубина вложения, при которой достигается максимум корр. размерности
        public int maxCorrDimension;
        //Количество пересечений для d
        public int crossers_count;
        //Величина первого шага
        public float step;
        //Минимальное расстояние между точками на траектории при dimension=1
        public float minR;
        //Максимальное расстояние между точками при dimension=1
        public float maxR;
        //Оценка снизу колмогоровской энтропии дин. системы
        public float entropyK;
        public long errorCode;
    }

    /// <summary>
    /// Структура для параметров входных данных
    /// </summary>
    public struct DataParams
    {
        //Имя файла
        public string fileName;
        //Используемый столбец
        public int columnIndex;
        //Период времени в формате строки: 03.07.2006.10.40-27.02.2007.17.30
        public string timePeriod;
        //Максимальное количество отсчетов - используется для прореживания
        public int maxCount;
        //Разделитель столбцов
        public char delimiter;
        //Предопределенный формат файла: 
        //classic: 16.06.2004,12:51,132.48,132.49,132.48,132.48,10
        //finam: 20060703;104100;48.00000;48.00000;48.00000;48.00000;800
        public string format;
        //"Очищать" ли файл 
        public bool isDoClearFile;
    }

    /// <summary>
    /// Параметры для расчета корр. размерности
    /// </summary>
    public struct CorrelationDimParams
    {
        //Начальное расстояние, используется только один раз для 1-го расчета
        public float r;
        //Стартовая размерность
        public int startDim;
        //Конечная размерность
        public int endDim;
        //количество точек для проведения регрессии (используется 70% последних меньше)
        public int stepCount;
        //Кол-во потоков
        public int thredCount;
    }

    /// <summary>
    /// Параметры для расчета фракт. размерности (показателя Херста)
    /// </summary>
    public struct FractalDimParams
    {        
    }

    /// <summary>
    /// Параметры для расчета автовзаимной информации
    /// </summary>
    public struct AutoMutualInfoParams
    {
        //Начальный лаг (задержка смещения ряда друг относительно друга)
        public int startDelay;
        //Конечный лаг
        public int endDelay;
        //Количество уровней разбиения по вероятностям достижения
        public int levelsCount;
        //Кол-во потоков
        public int thredCount;
    }

    //Результаты расчета корр. размерности 
    public struct AutoMutualInfoResult
    {
        //автовзаимная информация для различных задержек (лагов) ряда 
        public double[] AMI;
        //Оптимальный лаг для реконструкции аттрактора
        public int optimalLag;
    }
    /// <summary>
    /// Параметры для оценки максимального показателя Ляпунова
    /// </summary>
    public struct LyapunovParams
    {                
        //Глубина вложения
        public int embedingDimension;
        //Лаг (задержка)
        public int lag;
        //Количество оценок показателя для усреднения
        public int count;        
    }
    
    /// <summary>
    /// Параметры для расчета индекса вариации
    /// </summary>
    public struct VariationIndParams
    {
        //Стартовый индекс в массиве, с которого беруться значения для расчетов
        public int startIndex;
        //Конечный индекс в массиве
        public int endIndex;
        //Стартовая длина отрезка разбиения
        public int startSegmentLength;
        //Конечная длина отрезка разбиения 
        public int endSegmentLength;
        //Длина окна (кол-во отсчетов) для расчета индекса
        public int windowLength;
        //Количество точек для регрессии
        public int numPointsRegression;
    }

    /// <summary>
    /// Параметры для многослойного персептрона
    /// </summary>
    public struct PerseptronParams
    {        
        public int numInputSignals;
        public int numLayers;
        public int[] numNeuronsInLayer;
        public int learningSet;
        public int predictionStep;
        public int numEpochs;
        public int delayInput;
    }

    /// <summary>
    /// Параметры для классификации
    /// </summary>
    public struct ClassificationParams
    {
    }



    public struct InputDateFormat
    {
        public string formatName;
        public int year;
        public int month;
        public int day;
        public int hour;
        public int min;
        public string format;
    }

    public struct NormalizationByBoundResultOneChannel
    {
        public float[] A;
        public float amplituda;
        public float offset;
    }

    public struct NormalizationByBoundResult
    {
        public float[,] A;
        public float[] amplituda;
        public float[] offset;
    }

    public struct MlpTrainResult
    {
        public float k;
        public float b;
        public int sign;
        public float[] checkSignal;
        public float[] realSignal;
        public float[] predictSignal;
        public float[] realSignal2;
        public float[] predictSignal2;
        public string report;
    }


    //Trade
    //лимитированная заявка (обычная)
    public struct OrderLimit
    {
        public int number;
        public int timeNum;//индекс момента времени срабатывания
        public DateTime date;//исполнения
        public float price;
        public float volume;
        public int type;//1 - покупка, -1 - продажа
        public int kind;//1 - профит, -1 - стоп
        public byte status;//1 - активна, 2 - исполнена, 0 - снята 
        public int numOrderCondition;
    }

    //условная стоп-заявка
    public struct OrderStop
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float price;//цена порождаемой лимитированной заявки
        public float priceCondition;//цена срабатывания заявки
        public int volume;
        public int type;//1 - покупка, -1 - продажа
        public byte status;//1 - активна, 2 - исполнена, 0 - снята
    }

    //условная тейк-профит заявка
    public struct OrderProfit
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float priceCondition;
        public int volume;
        public int type;//покупка/продажа
        public float maxReturnProcent;//максимальный позволяемый откат назад в процентах
        public float spred;//защитный спред в абсолютных единицах от последней сделки после срабатывания условия
        public float maxPrice;//максимальная цена достигнутая ценой по данной заявке
        public byte status;
    }

    //условная свзанная заявка
    public struct OrderCondition
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float price;
        public int volume;
        public int type;//1 - покупка, -1 - продажа
        public byte status;//1 - активна, 2 - исполнена, 0 - снята
        public OrderLimit orderLimitStop; //ссылка на стоп-заявку
        public OrderLimit orderLimitProfit;//ссылка на тейк-профит заявку
    }

    public struct TradeResults
    {
        public string report;
        public OrderLimit[] executedOrders;
        public OrderCondition[] executedOrdersCond;
        public float[] cashVals;
        public float[,] MAs;
    }

    ////////////////////////////////////////////
    //старые структуры
    ////////////////////////////////////////////

    //активная заявка (обычная)
    public struct ActiveOrder
    {
        public int number;
        public DateTime date;
        public float price;
        public int volume;
        public int type;
        public byte kind;//0 - обычная, 1 - стоп, 2 - тейк-профит
        public int numerLinkOrder;
        public bool isActive;
        public float priceCondition;
    }

    //активная стоп-заявка
    public struct ActiveStopOrder
    {
        public int number;
        public DateTime date;
        public float price;//цена порождаемой заявки
        public float priceCondition;//цена срабатывания заявки
        public int volume;
        public int type;
        public int numerLinkOrder;
    }

    //активная тейк-профит заявка
    public struct ActiveProfitOrder
    {
        public int number;
        public DateTime date;
        public float priceCondition;
        public int volume;
        public int type;//покупка/продажа
        public float maxReturnProcent;//максимальный позволяемый откат назад в процентах
        public float spred;//защитный спред в абсолютных единицах
        public float maxPrice;//максимальная цена достигнутая ценой по данной заявке
        public byte status;//0 - стоп-условие еще не выполнено, 1 - стоп-условие выполнено
        public int numerLinkOrder;
    }

    //исполнненая заявка
    public struct ExecutedOrder
    {
        public int number;
        public int indSec;//индекс секунды из массива секундных отсчетов цены, когда сработала заявка
        public DateTime dateSet;
        public DateTime dateExecuted;
        public float price;
        public float volume;
        public int type;
        public float profit;
        public byte kind;//0 - обычная, 1 - стоп, 2 - тейк-профит
        public float sumCredit;
        public int numerLinkOrder;
    }

    //отмененная заявка
    public struct CanceledOrder
    {
        public int number;
        public DateTime dateSet;
        public DateTime dateCancel;
        public float price;
        public float volume;
        public int type;
    }

    //ЦБ в портфеле
    public struct Security
    {
        public string name;        
        public float volume;//число акций
        public int type;//покупка/продажа
    }

    //Точка на сетке при классификации
    public struct PointClassification
    {
        public float[] coord;//массив координат точки
        public DateTime time;//время точки (для разделения по актуальности)
        public int classNum;//номер класса
    }

    //Точка на сетке при классификации (полученные по сигналу float[])
    public struct PointClassificationSimple
    {
        public float[] coord;//массив координат точки        
        public int classNum;//номер класса
    }

    //Клетка сетки при классификации
    public struct CellClassification
    {
        //public float[] position;//положение клетки в сетке (индекс ряды, столбца и пр.)
        //public float[] leftTop;//координата левого верхнего угла клетки
        public List<int> points;//список индексов входящих точек (в одномерном массиве)        
        public int[] classCounter;//счетчик входящих точек для каждого класса
    }

    public struct listGraphs
    {
        public ZedGraph.GraphPane myGraphPane;
        public ZedGraph.PointPairList[] list;
        public ZedGraph.LineItem[] curves;     
    }

    /// <summary>
    /// Результаты обнаружения разладки в сигнале
    /// </summary>
    public struct DisorderResult
    {
        public float[] A;//Решающая функция
        public int indMax;//индекс максимума решающей функции
    }

    /// <summary>
    /// Параметры для расчета статистика достижения порогов изменения по ряду локальных экстремумов
    /// заданного порога
    /// <param name="startThreshold">Начальный порог</param>
    /// <param name="endThreshold">Конечный порог</param>
    /// <param name="deltaThreshold">Минимальное расстояние между d и u (в %)</param>
    /// <param name="deltaMaxThreshold">Максимальное расстояние между d и u</param>
    /// <param name="stepThreshold">Шаг изменения порога</param>
    /// <param name="probabilityLimit">Лимит вероятности, который должен быть набран для сохранения</param>
    /// </summary>
    public struct LocalExtremumStatParams
    {
        public float startThreshold;
        public float endThreshold;
        public float deltaThreshold;
        public float deltaMaxThreshold;
        public float stepThreshold;
        public float probabilityLimit;
    }

    //Статистика достжений порогов изменения при достижении заданных порогов без откатов ниже открытия позиии
    public struct LocalExtremumStatisticWithoutRecoil
    {
        public byte type;//1 - не ушли ниже открытия позиции (точка стартового порога), 0 - ушли ниже
        public float startThreshold;//стартовый порог в %
        public float thresholdAchievement;//порог достижения в % от точки достижения стартового порога (т.е. пройдя startThreshold прошли еще thresholdAchievement)
        public float probability;//вероятность достижения порога после преодоления стартового порога в %
        public float averageTime;
        public float dispersionTime;
        public float averageVolume;
        public float dispersionVolume;
    }

    public struct AggregateToSec
    {
        public char delimiter;
        public int startNum;
        public int endNum;
    }
}