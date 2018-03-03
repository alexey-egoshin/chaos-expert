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
        public float val;//�������� ��������
        public float qval;//�������� �������� (����� ������ ��������)
    }

    public struct PointTimeThresholdChange
    {
        public float time;//
        public string dateTimeStr;
        public float val;
        public float qval;
        public float original;//�������� �� ��������� ������� A
    }

    public struct PointTimeThresholdChangeFloat
    {
        public float time;//
        public string dateTimeStr;
        public float val;
        public float qval;
        public float original;//�������� �� ��������� ������� A
    }

    public struct PointTimeThresholdChangeSimple
    {
        public int ind;//������ ��������
        public int time;//����� ��������
        public float val;//�������� �������
    }

    //��������� ����������, ������� ���������� ���� �� ����� ������� �� ������������ �����
    public struct PointLocalExtremum
    {
        public float val;//���� � ������ �����
        public string dateTimeStr;//���� � ������ �����
        public float qval;//����������� ����� �� ������������� ����� �� ����
        public int count;//���-�� ������ ����� ���� � ������������� �������
        public float time;//����� ����� ��������� ����� (���)
        public float timeClear;//������ ����� ����� ��������� ����� (�� ������ ��������� ����� �������)
        public float timeThrehold;//�����, �� ������� ����������� �����
        public float timeThreholdClear;//�����, �� ������� ����������� ����� (�� ������ ��������� ����� �������)
        //������������ ������� ���� ��� �������� �� ������ ���������� ���������� �� �������.
        //��������� � % �� ������� ���������� ����������
        public float maxReturn;
        public float averageReturn;//������� ����� � % �� ������� �������. �����.
        public int indexOrg;//������ ����� � �������� �������
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
        public byte type;//1 - �� ���� ���� �������� ������� (����� ���������� ������), 0 - ���� ����

        public float startThreshold;//��������� �����
        public float endThreshold;//�������� �����
        public float lengthChanging;//����� ��������� (�������� ����� - ��������� �����) %
        public float probability;//����������� ���������� ��������� ������ ����� ����������� ���������� ������ � %
        public float probabilityWithoutRecoil;//����������� ���������� ��������� ������ ����� ����������� ���������� ������ � % ��� ������� ���� ����� ���������� ������
        public float minTime;//����������� �����, ������� ��������� ��� ���������� ��������� ������ ����� ����������� ���������� (� ��������)
        public float averageTime;//������� �����        
        public float maxTime;//������������ �����
        public int amount;//���-�� ����� �������
        
        public int amountUp;//���-�� ������� ���������� u ��� �������� �����
        public int amountDUp;//���-�� ������� �� ���������� u ��� �������� �����

        public float maxReturnUp;//������������ ������� ���� ��� �������� �� ���������� ������ �� ��������� �����. ��������� � % �� ������� ���������� ����������
        public float averageReturnUp;//������� ����� � % �� ������� �������. �����. ��� ��������� �����
        
        public float minTimeDUp;//����������� ����� ���������� ������ d ��� �������� �����
        public float averageTimeDUp;//������� ����� ���������� ������ d ��� �������� �����
        public float maxTimeDUp;//������������ ����� ���������� ������ d ��� �������� �����        
        
        public float averageTimeDUpWithoutRecoil;//������� ����� ���������� ������ d ��� �������� ����� ��� ������� ����������� u ��� ������� ���� d
        public float averageTimeDUpRecoil;//������� ����� ���������� ������ d ��� �������� ����� ��� ������� �� ����������� u ��� ������� ���� d ��� �� ����������� ������
        public float dispersionTimeDUpWithoutRecoil;//��������� ������� ���������� ������ d ��� �������� ����� � ����������� ����������� u ��� ������� ���� ���������� ������ (����� �������� �������)
        public float dispersionTimeDUpRecoil;//��������� ������� ���������� ������ d ��� �������� ����� � ����������� �� ����������� u ��� ������� ���� ���������� ������ (����� �������� �������). �.�. ���� �� ����������, ���� ������ ���� ���������� ������
        public int[] timeDUpRecoil;//�������� ������� ���������� d ��� ���������� u ��� ������� ���� d
        public int[] timeDUpWithoutRecoil;//�������� ������� ���������� d ��� ���������� u (� �� ����������) � �������� ���� d
        public int amountUpWithoutRecoil;//����� ���������� u ��� ������� ���� d ��� �������� �����
        public int amountUpRecoil;//����� �� ���������� u � ���������� u � �������� ���� d ��� �������� �����

        public float minTimeUUp;//����������� ����� ���������� ������ u ��� �������� ����� (�� d)
        public float averageTimeUUp;//������� ����� ���������� ������ u ��� �������� �����
        public float maxTimeUUp;//������������ ����� ���������� ������ u ��� �������� �����                

        public int amountGetUAfterReturnUp;//���-�� ������� ���������� �� d �� u ��� ������� ���� ������� ������������ ���� d ��� �������� �����
        public int amountReturnDUp;//���-�� ������� ���� d ��� �������� �����
        public int amountReturnUp;//���-�� ������� ��� �������� �����

        public int amountDown;//���-�� ������� ���������� u ��� �������� ����
        public int amountDDown;//���-�� ������� �� ���������� u ��� �������� ����
        public float maxReturnDown;//������������ ����� ��� �������� ���� (�.�. ����� �����)
        public float averageReturnDown;//������� ����� � % �� ������� �������. �����. ��� ��������� ����

        public float minTimeDDown;//������������ ����� ���������� ������ d ��� �������� ����
        public float averageTimeDDown;//������� ����� ���������� ������ d ��� �������� ����
        public float maxTimeDDown;//������������ ����� ���������� ������ d ��� �������� ����
        public float averageTimeDDownWithoutRecoil;//������� ����� ���������� ������ d ��� �������� ���� ��� ������� ����������� u ��� ������� ���� d
        public float averageTimeDDownRecoil;//������� ����� ���������� ������ d ��� �������� ���� ��� ������� �� ����������� u ��� ������� ���� d ��� �� ����������� ������
        public float dispersionTimeDDownWithoutRecoil;//��������� ������� ���������� ������ d ��� �������� ����  � ����������� ����������� u ��� ������� ���� ���������� ������ (����� �������� �������)
        public float dispersionTimeDDownRecoil;//��������� ������� ���������� ������ d ��� �������� ���� � ����������� �� ����������� u ��� ������� ���� ���������� ������ (����� �������� �������). �.�. ���� �� ����������, ���� ������ ���� ���������� ������
        public int[] timeDDownRecoil;//�������� ������� ���������� d ��� ���������� u ��� ������� ���� d
        public int[] timeDDownWithoutRecoil;//�������� ������� ���������� d ��� ���������� u (� �� ����������) � �������� ���� d
        public int amountDownWithoutRecoil;//����� ���������� u ��� ������� ���� d ��� �������� ����
        public int amountDownRecoil;//����� �� ���������� u � ���������� u � �������� ���� d ��� �������� ����

        public float minTimeUDown;//������������ ����� ���������� ������ u ��� �������� ���� (�� d)
        public float averageTimeUDown;//������� ����� ���������� ������ u ��� �������� ����
        public float maxTimeUDown;//������������ ����� ���������� ������ u ��� �������� ����
                
        public int amountGetUAfterReturnDown;//���-�� ������� ���������� �� d �� u ��� ������� ���� ������� ������������ ���� d ��� �������� ����        
    }

    /// <summary>
    /// ����� �������� ��������������� ������ � ��������������� ����� (��. ������ ���)
    /// </summary>
    public struct DisruptionPoint
    {
        public int type;//0 - ������, 1 - ��������� �����
        public int indConfirmativeThreshold;//������ ��������������� ������       
        public int indExtremum;//������ ���������� (���������/�������)
        public int indPreviousExtremum;//������ �������������� ���������� (���������)
        public int indThreshold;//������ ��������� ������
        public int indPreviousPseudoExtremum;//������ �������������� ���������� (���������/�������)
    }

    public struct PointRS
    {
        public double rs;// Log(R/S)
        public double n;// Log(���������� �������� (����� ����))
        public double h;// ���������� ������
        public double h0;// ���������� ������ ��� ����������
    }

    /// <summary>
    /// ������������ ������� ��������� y = Ax+B
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

    //���������� ������� ����. ����������� 
    public struct CorrelatingDimensionResult
    {
        public float[,] points;
        //������ ����. �����������
        public float correlatingDimension;
        //������������� ����������� �������
        public int maxDimension;
        //��������� �����������
        public int startDimension;
        //�������� �����������
        public int endDimension;
        //����������� ��������, ��� ������� ���������� ��������������� ���������� = 0
        public int dimension_zero;
        //��������� ����������� ��������
        public int lastDimension;
        //������� ��������, ��� ������� ����������� �������� ����. �����������
        public int maxCorrDimension;
        //���������� ����������� ��� d
        public int crossers_count;
        //�������� ������� ����
        public float step;
        //����������� ���������� ����� ������� �� ���������� ��� dimension=1
        public float minR;
        //������������ ���������� ����� ������� ��� dimension=1
        public float maxR;
        //������ ����� �������������� �������� ���. �������
        public float entropyK;
        public long errorCode;
    }

    /// <summary>
    /// ��������� ��� ���������� ������� ������
    /// </summary>
    public struct DataParams
    {
        //��� �����
        public string fileName;
        //������������ �������
        public int columnIndex;
        //������ ������� � ������� ������: 03.07.2006.10.40-27.02.2007.17.30
        public string timePeriod;
        //������������ ���������� �������� - ������������ ��� ������������
        public int maxCount;
        //����������� ��������
        public char delimiter;
        //���������������� ������ �����: 
        //classic: 16.06.2004,12:51,132.48,132.49,132.48,132.48,10
        //finam: 20060703;104100;48.00000;48.00000;48.00000;48.00000;800
        public string format;
        //"�������" �� ���� 
        public bool isDoClearFile;
    }

    /// <summary>
    /// ��������� ��� ������� ����. �����������
    /// </summary>
    public struct CorrelationDimParams
    {
        //��������� ����������, ������������ ������ ���� ��� ��� 1-�� �������
        public float r;
        //��������� �����������
        public int startDim;
        //�������� �����������
        public int endDim;
        //���������� ����� ��� ���������� ��������� (������������ 70% ��������� ������)
        public int stepCount;
        //���-�� �������
        public int thredCount;
    }

    /// <summary>
    /// ��������� ��� ������� �����. ����������� (���������� ������)
    /// </summary>
    public struct FractalDimParams
    {        
    }

    /// <summary>
    /// ��������� ��� ������� ������������ ����������
    /// </summary>
    public struct AutoMutualInfoParams
    {
        //��������� ��� (�������� �������� ���� ���� ������������ �����)
        public int startDelay;
        //�������� ���
        public int endDelay;
        //���������� ������� ��������� �� ������������ ����������
        public int levelsCount;
        //���-�� �������
        public int thredCount;
    }

    //���������� ������� ����. ����������� 
    public struct AutoMutualInfoResult
    {
        //������������ ���������� ��� ��������� �������� (�����) ���� 
        public double[] AMI;
        //����������� ��� ��� ������������� ����������
        public int optimalLag;
    }
    /// <summary>
    /// ��������� ��� ������ ������������� ���������� ��������
    /// </summary>
    public struct LyapunovParams
    {                
        //������� ��������
        public int embedingDimension;
        //��� (��������)
        public int lag;
        //���������� ������ ���������� ��� ����������
        public int count;        
    }
    
    /// <summary>
    /// ��������� ��� ������� ������� ��������
    /// </summary>
    public struct VariationIndParams
    {
        //��������� ������ � �������, � �������� �������� �������� ��� ��������
        public int startIndex;
        //�������� ������ � �������
        public int endIndex;
        //��������� ����� ������� ���������
        public int startSegmentLength;
        //�������� ����� ������� ��������� 
        public int endSegmentLength;
        //����� ���� (���-�� ��������) ��� ������� �������
        public int windowLength;
        //���������� ����� ��� ���������
        public int numPointsRegression;
    }

    /// <summary>
    /// ��������� ��� ������������� �����������
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
    /// ��������� ��� �������������
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
    //�������������� ������ (�������)
    public struct OrderLimit
    {
        public int number;
        public int timeNum;//������ ������� ������� ������������
        public DateTime date;//����������
        public float price;
        public float volume;
        public int type;//1 - �������, -1 - �������
        public int kind;//1 - ������, -1 - ����
        public byte status;//1 - �������, 2 - ���������, 0 - ����� 
        public int numOrderCondition;
    }

    //�������� ����-������
    public struct OrderStop
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float price;//���� ����������� �������������� ������
        public float priceCondition;//���� ������������ ������
        public int volume;
        public int type;//1 - �������, -1 - �������
        public byte status;//1 - �������, 2 - ���������, 0 - �����
    }

    //�������� ����-������ ������
    public struct OrderProfit
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float priceCondition;
        public int volume;
        public int type;//�������/�������
        public float maxReturnProcent;//������������ ����������� ����� ����� � ���������
        public float spred;//�������� ����� � ���������� �������� �� ��������� ������ ����� ������������ �������
        public float maxPrice;//������������ ���� ����������� ����� �� ������ ������
        public byte status;
    }

    //�������� �������� ������
    public struct OrderCondition
    {
        public int number;
        public int timeNum;
        public DateTime date;
        public float price;
        public int volume;
        public int type;//1 - �������, -1 - �������
        public byte status;//1 - �������, 2 - ���������, 0 - �����
        public OrderLimit orderLimitStop; //������ �� ����-������
        public OrderLimit orderLimitProfit;//������ �� ����-������ ������
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
    //������ ���������
    ////////////////////////////////////////////

    //�������� ������ (�������)
    public struct ActiveOrder
    {
        public int number;
        public DateTime date;
        public float price;
        public int volume;
        public int type;
        public byte kind;//0 - �������, 1 - ����, 2 - ����-������
        public int numerLinkOrder;
        public bool isActive;
        public float priceCondition;
    }

    //�������� ����-������
    public struct ActiveStopOrder
    {
        public int number;
        public DateTime date;
        public float price;//���� ����������� ������
        public float priceCondition;//���� ������������ ������
        public int volume;
        public int type;
        public int numerLinkOrder;
    }

    //�������� ����-������ ������
    public struct ActiveProfitOrder
    {
        public int number;
        public DateTime date;
        public float priceCondition;
        public int volume;
        public int type;//�������/�������
        public float maxReturnProcent;//������������ ����������� ����� ����� � ���������
        public float spred;//�������� ����� � ���������� ��������
        public float maxPrice;//������������ ���� ����������� ����� �� ������ ������
        public byte status;//0 - ����-������� ��� �� ���������, 1 - ����-������� ���������
        public int numerLinkOrder;
    }

    //����������� ������
    public struct ExecutedOrder
    {
        public int number;
        public int indSec;//������ ������� �� ������� ��������� �������� ����, ����� ��������� ������
        public DateTime dateSet;
        public DateTime dateExecuted;
        public float price;
        public float volume;
        public int type;
        public float profit;
        public byte kind;//0 - �������, 1 - ����, 2 - ����-������
        public float sumCredit;
        public int numerLinkOrder;
    }

    //���������� ������
    public struct CanceledOrder
    {
        public int number;
        public DateTime dateSet;
        public DateTime dateCancel;
        public float price;
        public float volume;
        public int type;
    }

    //�� � ��������
    public struct Security
    {
        public string name;        
        public float volume;//����� �����
        public int type;//�������/�������
    }

    //����� �� ����� ��� �������������
    public struct PointClassification
    {
        public float[] coord;//������ ��������� �����
        public DateTime time;//����� ����� (��� ���������� �� ������������)
        public int classNum;//����� ������
    }

    //����� �� ����� ��� ������������� (���������� �� ������� float[])
    public struct PointClassificationSimple
    {
        public float[] coord;//������ ��������� �����        
        public int classNum;//����� ������
    }

    //������ ����� ��� �������������
    public struct CellClassification
    {
        //public float[] position;//��������� ������ � ����� (������ ����, ������� � ��.)
        //public float[] leftTop;//���������� ������ �������� ���� ������
        public List<int> points;//������ �������� �������� ����� (� ���������� �������)        
        public int[] classCounter;//������� �������� ����� ��� ������� ������
    }

    public struct listGraphs
    {
        public ZedGraph.GraphPane myGraphPane;
        public ZedGraph.PointPairList[] list;
        public ZedGraph.LineItem[] curves;     
    }

    /// <summary>
    /// ���������� ����������� �������� � �������
    /// </summary>
    public struct DisorderResult
    {
        public float[] A;//�������� �������
        public int indMax;//������ ��������� �������� �������
    }

    /// <summary>
    /// ��������� ��� ������� ���������� ���������� ������� ��������� �� ���� ��������� �����������
    /// ��������� ������
    /// <param name="startThreshold">��������� �����</param>
    /// <param name="endThreshold">�������� �����</param>
    /// <param name="deltaThreshold">����������� ���������� ����� d � u (� %)</param>
    /// <param name="deltaMaxThreshold">������������ ���������� ����� d � u</param>
    /// <param name="stepThreshold">��� ��������� ������</param>
    /// <param name="probabilityLimit">����� �����������, ������� ������ ���� ������ ��� ����������</param>
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

    //���������� ��������� ������� ��������� ��� ���������� �������� ������� ��� ������� ���� �������� ������
    public struct LocalExtremumStatisticWithoutRecoil
    {
        public byte type;//1 - �� ���� ���� �������� ������� (����� ���������� ������), 0 - ���� ����
        public float startThreshold;//��������� ����� � %
        public float thresholdAchievement;//����� ���������� � % �� ����� ���������� ���������� ������ (�.�. ������ startThreshold ������ ��� thresholdAchievement)
        public float probability;//����������� ���������� ������ ����� ����������� ���������� ������ � %
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