using System;
using System.Collections.Generic;
using System.Text;

namespace ChaosExpert
{
class Classification
{
    //����� �������
    private static int numClasses = 2;
    //���������� �������� ��������� ����� (�� 1-� ����������)
    private static int cellNum = 40;
    //����� ������ �� ����� (����� ��������������� �������)
    private static int amountCells = cellNum;
    //����������� ������ ��� ���������� �������� �����
    private static float kLen = 0.1f;
    //��� ������������ ������ ������ �������
    private static int[] _coord_inds;
    private static int[] _coord_current;
    private static int[] _coord_new;
    private static int dimensionGrid;
    private static int[] _biases = new int[3];
    private static List<ulong> _neighbours = new List<ulong>();
    private static bool _isSelfPoint;

    //��������� � �������� �� ������ ����������
    private static float[] mins;
    private static float[] maxs;
    //����� ������ �� ������ ���������� �����
    private static int[] numCells;


    /// <summary>
    /// ����������� ����� ������ ������� ������ (����/���������) � ����� �� �������� � ������
    /// ������ ����������� ������ ������� ��������� � ������������:
    /// 
    /// for x -> _bias
    ///     for y -> _bias
    ///         for z -> _bias
    ///             ...
    ///                 addList(points[x+_bias[i]],y+_bias[i]],z+_bias[i]]...);
    /// 
    /// </summary>
    /// <param name="coord_ind">������ ��������� ����������</param>
    public static void traverseNeighbour(int coord_ind, int numLayer)
    {
        coord_ind++;
        //���� ����� �� ��������� ����������
        if (coord_ind == dimensionGrid - 1)
        {
            //��������� ������ � ������� ��������� �� ������ ����������            
            _coord_new[coord_ind] = _coord_current[coord_ind];
            for (int t = 0; t < _coord_inds.Length; t++)
            {                    
                //���� ������ ������� �� ������� ����� �� �� ���������
                if (_coord_new[t] < 0)
                {
                    _isSelfPoint = false;
                }                
            }
            if (_isSelfPoint)
            {
                //���������� ������ ������ � ���������� �������                    
                ulong neighbourInd = MathProcess.getIndexByMultidimArray(_coord_new, numCells);
                //��������� � ������ �������
                _neighbours.Add(neighbourInd);
            }
            else
            {
                _isSelfPoint = true;
            }
            //��� ��������� ���������� ����� ��������� ��� ��������            
            for (int j = 1; j <= numLayer; j++)
            {
                //�������� ����� � 1 ����� ������ �� ��������� ������� �������� - ��� ������� ��� �����
                for (int i = 1; i < _biases.Length; i++)
                {
                    //�������  ���������� � ������ ��������� ������ (��������� �������� � ������� ����������)
                    _coord_new[coord_ind] = _coord_current[coord_ind] + _biases[i] * j;
                    //��������� �� ����� �� �� ������� ����� (�� ������� �� ������ ����������)
                    if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
                    {
                        //���������� ������ ������ � ���������� �������                    
                        ulong neighbourInd = MathProcess.getIndexByMultidimArray(_coord_new, numCells);
                        //��������� � ������ �������
                        _neighbours.Add(neighbourInd);
                    }
                    else
                    {
                        //���� ���� �� ���� ���������� ������ ������� �� ������� ����������� - �������
                        _coord_new[coord_ind] = -1;
                        return;
                    }
                }
            }
        }
        //�� ��������� ����������� ������
        else
        {
            //�������  ���������� � ������ ��������� ������ c ������� ���������
            _coord_new[coord_ind] = _coord_current[coord_ind];
            //��������� �� ����� �� �� ������� ����� (�� ������� �� ������ ����������)
            if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
            {
                traverseNeighbour(coord_ind, numLayer);
            }
            else
            {
                //���� ���� �� ���� ���������� ������ ������� �� ������� ����������� - �������
                _coord_new[coord_ind] = -1;
                return;
            }

            //��������������� ����������� � ����� ����� ��������� ��� ��������
            for (int j = 1; j <= numLayer; j++)
            {
                for (int i = 1; i < _biases.Length; i++)
                {
                    //�������  ���������� � ������ ��������� ������ (��������� �������� � ������� ����������)
                    _coord_new[coord_ind] = _coord_current[coord_ind] + _biases[i] * j;
                    //��������� �� ����� �� �� ������� ����� (�� ������� �� ������ ����������)
                    if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
                    {
                        traverseNeighbour(coord_ind, numLayer);
                    }
                    else
                    {
                        //���� ���� �� ���� ���������� ������ ������� �� ������� ����������� - �������
                        _coord_new[coord_ind] = -1;
                        return;
                    }
                }
            }            
        }
    }
    
    /// <summary>
    /// K nearest neighbour � ������ ������� � �������
    /// </summary>
    /// <param name="A">������ �����</param>
    /// <param name="startTrain">������ � �������� ���������� ��������</param>
    /// <param name="endTrain">������ ������� ������������� ��������</param>
    /// <param name="startTest">������ ������ ������������</param>
    /// <param name="endTest">������ ����� ������������</param>
    public static string KNNwithTimeR(PointClassification[] A, int startTrain, int endTrain, int startTest, int endTest, string filename)
    {
        //����������� �����
        dimensionGrid = A[0].coord.Length;
        //���� ��������� � �������� �� ������ ����������
        //����� ���������� ������� �����
        mins = new float[dimensionGrid];
        maxs = new float[dimensionGrid];
        //�������������� ������ ��������� 
        for (int i = 0; i < dimensionGrid; i++)
        {
            mins[i] = A[startTrain].coord[i];
            maxs[i] = A[startTrain].coord[i];
        }
        for (int i = startTrain; i <= endTrain; i++)
        {
            for (int j = 0; j < dimensionGrid; j++)
            {
                if (A[i].coord[j] < mins[j]) mins[j] = A[i].coord[j];
                if (A[i].coord[j] > maxs[j]) maxs[j] = A[i].coord[j];
            }
        }
        //������������� ������� ����� � �������
        /*
        for (int j = 0; j < dimensionGrid; j++)
        {
            mins[j] *= (1-kLen);
            maxs[j] *= (1 + kLen);
        }
         */ 
        //���������� ����� ����� �� ������ ����������
        
        //����� ������ �����
        //���������� ����� �������� ����������
        float[] lens = new float[dimensionGrid];
        int minLenInd = 0;
        for (int i = 0; i < dimensionGrid; i++)
        {
            lens[i] = (maxs[i] - mins[i]);
            if (lens[i] < lens[minLenInd]) minLenInd = i;
        }


        //����� ������ �� ������ ���������� �����
        numCells = new int[dimensionGrid];        
        //�� ������ ������
        numCells[minLenInd] = cellNum;
        ulong amountCells = 1;
        //����� ������� ������ �� 1-� ���������� - ��� ���� �����. ����� �� �������� (������)
        float lenCell = lens[minLenInd] / cellNum;
        //���������� ������� ����� ������ �� ����� ������� ������
        for (int i = 0; i < dimensionGrid; i++)
        {
            numCells[i] = (int) Math.Ceiling((double)lens[i] / lenCell);            
            //����� ������ (� ���������� �������)
            amountCells *= (ulong)numCells[i];
        }

        //������� ����� ����������� dimensionGrid
        //��� �������� ����� ������������ ����������������� ������ - ���������� � ����������� ����������
        
        //���������� ������, � ������� ����� ������� ����������� �����
        CellClassification[] grid = new CellClassification[amountCells];
        //���������� ����� �� ����� (������� ������)
        int[] coord = new int[dimensionGrid];
        //��������� ��� ����� �� �����
        for (int i = startTrain; i <= endTrain; i++)
        {
            //������ �� ������ ����������
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j]-mins[j])/lenCell));
            }
            //������ � ���������� �������
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }
             
            //� ������ ����� ��������� ������ �����,                                        
            grid[ind].points.Add(i);
            //� ����� ����������� �������� �������
            grid[ind].classCounter[A[i].classNum]++;
        }
        //������� ����������� - ��� ������ ���������������� ����� - �� ��������� ����� � 
        //��������� ������������� �� ������� � ���� ������������ ������������� 
        float[,] res = new float[endTest - startTest + 1, numClasses + 7];

        //�������������� �����        
        //������� ��� ����� ���������� �������������
        
        //���������� 
        string[] header = new string[numClasses + 7];
        header[0] = "Class point";
        header[1] = "Power 0";
        header[2] = "Neubohoods 0";
        header[3] = "Power 1";
        header[4] = "Neubohoods 1";
        header[5] = "Procent 1";
        header[6] = "Procent 0";
        header[7] = "True detect";
        header[8] = "Class detect as";

        int c = 0;        
        for (int i = startTest; i <= endTest; i++)
        {
            //���������� � ����� ������ ����� ��������
            //������ �� ������ ����������
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //������ � ���������� �������
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            //���� ������ ������� �� ������� ����� ���������� ��
            if (ind < 0 || ind >= amountCells)
            {
                continue;
            }

            //���������� ������, ������ �������� ����� ����� ����� (������� �� ��������� ����� � ������)

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[2];
            }

            int amountPointsCell = 0;
            //��������� ����� �� ���� �������
            for (int j = 0; j < numClasses; j++)
            {
                amountPointsCell += grid[ind].classCounter[j];
            }
            //������ ������� �� ���������. ������� ����� ��������� �����������
            float r = lenCell * ((float) 1 / (amountPointsCell+1)+1);

            //���������� ������ ������ ������� ����� ������ ����� �������� � ����
            ///��� ����� ����� ������ �� ���� ������, ���������� ��� ������, � ����� ����������� ��������� (����)
            ///������� ��� ������ ���������� ����������. ��������� ����� ����� �������� ������ ������� ����� ������� 
            /// ��� ������, ��� ���� ��������� ���������
            /// 

            //���������� ����� ������ ������ �������
            int numLayersAround = (int)Math.Floor((double)(r / lenCell));

            //���������� ��� ������: �� ������ ���������� � ��� ������� (����� � �����)
            
            //�������� � ����������
            _biases[0] = 0;
            _biases[1] = 1;
            _biases[2] = -1;

            _coord_inds = new int[dimensionGrid];
            
            //������� ������� ���� ������� � ������ _neighbours
            _neighbours.Clear();
            //��� ��� ����� ������ �����������, ���������� ���������� ����������
            //���� ��������
            _isSelfPoint = true;
            //���������� ������� ������
            _coord_current = new int[coord.Length];
            _coord_new = new int[coord.Length];
            for (int t = 0; t < coord.Length; t++)
            {
                _coord_current[t] = coord[t];
                _coord_new[t] = coord[t];
            }                           
            traverseNeighbour(-1, numLayersAround);
            //���� ������� �����-������ �� ������� ������
            float[,] classes = new float[numClasses,2];
            //������ ������ ������-������ ���������� ��� ����� � ���������� ���������� �� ��� � ��������
            for (int n = 0; n < _neighbours.Count; n++)
            {
                //������ ������-������ � ������� ������
                ulong neighbour_ind = _neighbours[n];
                //���� � ������ ���� �����
                if ((int)neighbour_ind < grid.Length && grid[neighbour_ind].points != null)
                {
                    //������� ��� ����� � ������-������ � ���������� ���������� �� ��� � ��������
                    for (int p = 0; p < grid[neighbour_ind].points.Count; p++)
                    {
                        //������ ����� � ������-������
                        int point_ind = grid[neighbour_ind].points[p];
                        //���������� ����� ������ � ������ ������ � ������� ������
                        float spacing = MathProcess.getPointSpacingManhaten(A[point_ind].coord, A[i].coord);
                        //���� ����� ������ � �������� ������ - �������� �� � ������ ������������� �����-�������
                        if (spacing < r)
                        {
                            //���������� ������� �� ������� ������
                            
                            //
                            //TimeSpan diff = A[i].time.Subtract(A[point_ind].time);
                            //float timeSpacing = (float) diff.TotalMinutes;
                            //spacing *= spacing;
                            //timeSpacing *= timeSpacing;
                            //��������� ��������� �� ������� � ��������� � ����������������,
                            //�.�. ��� ������ ������ ����������� �������� ��� �������� ����� �����
                            //float timeComponent = Math.Abs(timeSpacing) / 100;
                            //float spacingComponent = 1/(spacing + timeSpacing);
                            //float timeComponent = 100000/(timeSpacing+1);
                            float spacingComponent = 1 / (spacing);
                            classes[A[point_ind].classNum, 0] += spacingComponent;
                            classes[A[point_ind].classNum, 1]++;
                        }
                    }
                }
            }
            //�������������� ����� ����� �������� ���� �������
            //������ �������� ���������� ������
            int maxClassInd = 0;
            res[c, 0] = A[i].classNum;
            int m = 1;
            for (int z = 0; z < numClasses; z++)
            {
                //�������������� ��������� ����� ������ � ������
                res[c, m] = classes[z, 0];
                //���������� ������� ������� ������
                res[c, m + 1] = classes[z, 1];
                m = m + 2;
                //���� ���������� � ������������ � ������� ������, �� ��� ����� ��������� �����
                if ( classes[maxClassInd,0] < classes[z,0])
                {
                    maxClassInd = z;
                }
            }
            float procent1 = 0;
            if (res[c, 3] > 0)
            {
                procent1 = (float)res[c, 3] * 100 / (res[c, 1] + res[c, 3]);
            }
            res[c, numClasses + 3] = procent1;

            float procent0 = 0;
            if (res[c, 1] > 0)
            {
                procent0 = (float)res[c, 1] * 100 / (res[c, 1] + res[c, 3]);
            }
            res[c, numClasses + 4] = procent0;

            
            if (procent1 > procent0)
            {
                maxClassInd = 1;
            }
            else
            {
                maxClassInd = 0;
            }
            //Random rnd = new Random();
            //int randp = rnd.Next(0, 100);
            
            
            if (procent0 > 95 && res[c, 2] > 3)
            {
                maxClassInd = 0;
            }
            else
            {
                maxClassInd = 1;
            }
            
            //���� ����� ������� �� ���������� ��������� � ������� �����
            if (A[i].classNum == maxClassInd)
            {
                res[c, numClasses+5] = 1;
            }
            else
            {
                res[c, numClasses+5] = 0;
            }
            
            res[c, numClasses + 6] = maxClassInd;
            c++;

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }

            //� ������ ����� ��������� ������ �����,                                        
            grid[ind].points.Add(i);
            //� ����� ����������� �������� �������
            grid[ind].classCounter[A[i].classNum]++;
        }
        DataProcess.ExportArray(res, filename, header);
        string report = "Done. count = " + _neighbours.Count.ToString();
        return report;         
    }


    /// <summary>
    /// K nearest neighbour � ������ ������� � �������
    /// </summary>
    /// <param name="A">������ �����</param>
    /// <param name="startTrain">������ � �������� ���������� ��������</param>
    /// <param name="endTrain">������ ������� ������������� ��������</param>
    /// <param name="startTest">������ ������ ������������</param>
    /// <param name="endTest">������ ����� ������������</param>
    public static string KNNSimple(PointClassificationSimple[] A, int startTrain, int endTrain, int startTest, int endTest, string filename)
    {
        //����������� �����
        dimensionGrid = A[0].coord.Length;
        //���� ��������� � �������� �� ������ ����������
        //����� ���������� ������� �����
        mins = new float[dimensionGrid];
        maxs = new float[dimensionGrid];
        //�������������� ������ ��������� 
        for (int i = 0; i < dimensionGrid; i++)
        {
            mins[i] = A[startTrain].coord[i];
            maxs[i] = A[startTrain].coord[i];
        }
        for (int i = startTrain; i <= endTrain; i++)
        {
            for (int j = 0; j < dimensionGrid; j++)
            {
                if (A[i].coord[j] < mins[j]) mins[j] = A[i].coord[j];
                if (A[i].coord[j] > maxs[j]) maxs[j] = A[i].coord[j];
            }
        }
        //������������� ������� ����� � �������
        /*
        for (int j = 0; j < dimensionGrid; j++)
        {
            mins[j] *= (1-kLen);
            maxs[j] *= (1 + kLen);
        }
         */
        //���������� ����� ����� �� ������ ����������

        //����� ������ �����
        //���������� ����� �������� ����������
        float[] lens = new float[dimensionGrid];
        int minLenInd = 0;
        for (int i = 0; i < dimensionGrid; i++)
        {
            lens[i] = (maxs[i] - mins[i]);
            if (lens[i] < lens[minLenInd]) minLenInd = i;
        }


        //����� ������ �� ������ ���������� �����
        numCells = new int[dimensionGrid];
        //�� ������ ������
        numCells[minLenInd] = cellNum;
        ulong amountCells = 1;
        //����� ������� ������ �� 1-� ���������� - ��� ���� �����. ����� �� �������� (������)
        float lenCell = lens[minLenInd] / cellNum;
        //���������� ������� ����� ������ �� ����� ������� ������
        for (int i = 0; i < dimensionGrid; i++)
        {
            numCells[i] = (int)Math.Ceiling((double)lens[i] / lenCell);
            //����� ������ (� ���������� �������)
            amountCells *= (ulong)numCells[i];
        }

        //������� ����� ����������� dimensionGrid
        //��� �������� ����� ������������ ����������������� ������ - ���������� � ����������� ����������

        //���������� ������, � ������� ����� ������� ����������� �����
        CellClassification[] grid = new CellClassification[amountCells];
        //���������� ����� �� ����� (������� ������)
        int[] coord = new int[dimensionGrid];
        //��������� ��� ����� �� �����
        for (int i = startTrain; i <= endTrain; i++)
        {
            //������ �� ������ ����������
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //������ � ���������� �������
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }

            //� ������ ����� ��������� ������ �����,                                        
            grid[ind].points.Add(i);
            //� ����� ����������� �������� �������
            grid[ind].classCounter[A[i].classNum]++;
        }
        //������� ����������� - ��� ������ ���������������� ����� - �� ��������� ����� � 
        //��������� ������������� �� ������� � ���� ������������ ������������� 
        float[,] res = new float[endTest - startTest + 1, numClasses + 7];

        //�������������� �����        
        //������� ��� ����� ���������� �������������

        //���������� 
        string[] header = new string[numClasses + 7];
        header[0] = "Class point";
        header[1] = "Power 0";
        header[2] = "Neubohoods 0";
        header[3] = "Power 1";
        header[4] = "Neubohoods 1";
        header[5] = "Procent 1";
        header[6] = "Procent 0";
        header[7] = "True detect";
        header[8] = "Class detect as";

        int c = 0;
        for (int i = startTest; i <= endTest; i++)
        {
            //���������� � ����� ������ ����� ��������
            //������ �� ������ ����������
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //������ � ���������� �������
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            //���� ������ ������� �� ������� ����� ���������� ��
            if (ind < 0 || ind >= amountCells)
            {
                continue;
            }

            //���������� ������, ������ �������� ����� ����� ����� (������� �� ��������� ����� � ������)

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[2];
            }

            int amountPointsCell = 0;
            //��������� ����� �� ���� �������
            for (int j = 0; j < numClasses; j++)
            {
                amountPointsCell += grid[ind].classCounter[j];
            }
            //������ ������� �� ���������. ������� ����� ��������� �����������
            float r = lenCell * ((float)1 / (amountPointsCell + 1) + 1);

            //���������� ������ ������ ������� ����� ������ ����� �������� � ����
            ///��� ����� ����� ������ �� ���� ������, ���������� ��� ������, � ����� ����������� ��������� (����)
            ///������� ��� ������ ���������� ����������. ��������� ����� ����� �������� ������ ������� ����� ������� 
            /// ��� ������, ��� ���� ��������� ���������
            /// 

            //���������� ����� ������ ������ �������
            int numLayersAround = (int)Math.Floor((double)(r / lenCell));

            //���������� ��� ������: �� ������ ���������� � ��� ������� (����� � �����)

            //�������� � ����������
            _biases[0] = 0;
            _biases[1] = 1;
            _biases[2] = -1;

            _coord_inds = new int[dimensionGrid];

            //������� ������� ���� ������� � ������ _neighbours
            _neighbours.Clear();
            //��� ��� ����� ������ �����������, ���������� ���������� ����������
            //���� ��������
            _isSelfPoint = true;
            //���������� ������� ������
            _coord_current = new int[coord.Length];
            _coord_new = new int[coord.Length];
            for (int t = 0; t < coord.Length; t++)
            {
                _coord_current[t] = coord[t];
                _coord_new[t] = coord[t];
            }
            traverseNeighbour(-1, numLayersAround);
            //���� ������� �����-������ �� ������� ������
            float[,] classes = new float[numClasses, 2];
            //������ ������ ������-������ ���������� ��� ����� � ���������� ���������� �� ��� � ��������
            for (int n = 0; n < _neighbours.Count; n++)
            {
                //������ ������-������ � ������� ������
                ulong neighbour_ind = _neighbours[n];
                //���� � ������ ���� �����
                if ((int)neighbour_ind < grid.Length && grid[neighbour_ind].points != null)
                {
                    //������� ��� ����� � ������-������ � ���������� ���������� �� ��� � ��������
                    for (int p = 0; p < grid[neighbour_ind].points.Count; p++)
                    {
                        //������ ����� � ������-������
                        int point_ind = grid[neighbour_ind].points[p];
                        //���������� ����� ������ � ������ ������ � ������� ������
                        float spacing = MathProcess.getPointSpacingManhaten(A[point_ind].coord, A[i].coord);
                        //���� ����� ������ � �������� ������ - �������� �� � ������ ������������� �����-�������
                        if (spacing < r)
                        {
                            //���������� ������� �� ������� ������

                            //
                            //TimeSpan diff = A[i].time.Subtract(A[point_ind].time);
                            //float timeSpacing = (float) diff.TotalMinutes;
                            //spacing *= spacing;
                            //timeSpacing *= timeSpacing;
                            //��������� ��������� �� ������� � ��������� � ����������������,
                            //�.�. ��� ������ ������ ����������� �������� ��� �������� ����� �����
                            //float timeComponent = Math.Abs(timeSpacing) / 100;
                            //float spacingComponent = 1/(spacing + timeSpacing);
                            //float timeComponent = 100000/(timeSpacing+1);
                            float spacingComponent = 1 / (spacing);
                            classes[A[point_ind].classNum, 0] += spacingComponent;
                            classes[A[point_ind].classNum, 1]++;
                        }
                    }
                }
            }
            //�������������� ����� ����� �������� ���� �������
            //������ �������� ���������� ������
            int maxClassInd = 0;
            res[c, 0] = A[i].classNum;
            int m = 1;
            for (int z = 0; z < numClasses; z++)
            {
                //�������������� ��������� ����� ������ � ������
                res[c, m] = classes[z, 0];
                //���������� ������� ������� ������
                res[c, m + 1] = classes[z, 1];
                m = m + 2;
                //���� ���������� � ������������ � ������� ������, �� ��� ����� ��������� �����
                if (classes[maxClassInd, 0] < classes[z, 0])
                {
                    maxClassInd = z;
                }
            }
            float procent1 = 0;
            if (res[c, 3] > 0)
            {
                procent1 = (float)res[c, 3] * 100 / (res[c, 1] + res[c, 3]);
            }
            res[c, numClasses + 3] = procent1;

            float procent0 = 0;
            if (res[c, 1] > 0)
            {
                procent0 = (float)res[c, 1] * 100 / (res[c, 1] + res[c, 3]);
            }
            res[c, numClasses + 4] = procent0;


            if (procent1 > procent0)
            {
                maxClassInd = 1;
            }
            else
            {
                maxClassInd = 0;
            }
            //Random rnd = new Random();
            //int randp = rnd.Next(0, 100);


            if (procent0 > 95 && res[c, 2] > 3)
            {
                maxClassInd = 0;
            }
            else
            {
                maxClassInd = 1;
            }

            //���� ����� ������� �� ���������� ��������� � ������� �����
            if (A[i].classNum == maxClassInd)
            {
                res[c, numClasses + 5] = 1;
            }
            else
            {
                res[c, numClasses + 5] = 0;
            }

            res[c, numClasses + 6] = maxClassInd;
            c++;

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }

            //� ������ ����� ��������� ������ �����,                                        
            grid[ind].points.Add(i);
            //� ����� ����������� �������� �������
            grid[ind].classCounter[A[i].classNum]++;
        }
        DataProcess.ExportArray(res, filename, header);
        string report = "Done. count = " + _neighbours.Count.ToString();
        return report;
    }
}
}
