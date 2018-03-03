using System;
using System.Collections.Generic;
using System.Text;

namespace ChaosExpert
{
class Classification
{
    //Число классов
    private static int numClasses = 2;
    //Количество участков разбиения сетки (по 1-й координате)
    private static int cellNum = 40;
    //Всего клеток на сетке (длина соответствущего массива)
    private static int amountCells = cellNum;
    //Коэффициент запаса для увеличения размеров сетки
    private static float kLen = 0.1f;
    //Для рекурсивного метода обхода соседей
    private static int[] _coord_inds;
    private static int[] _coord_current;
    private static int[] _coord_new;
    private static int dimensionGrid;
    private static int[] _biases = new int[3];
    private static List<ulong> _neighbours = new List<ulong>();
    private static bool _isSelfPoint;

    //максимумы и минимумы по каждой координате
    private static float[] mins;
    private static float[] maxs;
    //Число клеток по каждой координате сеток
    private static int[] numCells;


    /// <summary>
    /// Рекурсивный метод обхода соседей клетки (куба/гиперкуба) и занос их индексов в список
    /// Должен реализовать полный перебор координат в пространстве:
    /// 
    /// for x -> _bias
    ///     for y -> _bias
    ///         for z -> _bias
    ///             ...
    ///                 addList(points[x+_bias[i]],y+_bias[i]],z+_bias[i]]...);
    /// 
    /// </summary>
    /// <param name="coord_ind">Индекс начальной координаты</param>
    public static void traverseNeighbour(int coord_ind, int numLayer)
    {
        coord_ind++;
        //Если дошли до последней координаты
        if (coord_ind == dimensionGrid - 1)
        {
            //Добавляем соседа с нулевым смещением по данной координате            
            _coord_new[coord_ind] = _coord_current[coord_ind];
            for (int t = 0; t < _coord_inds.Length; t++)
            {                    
                //Если клетка выходит за размеры сетки ее не учитываем
                if (_coord_new[t] < 0)
                {
                    _isSelfPoint = false;
                }                
            }
            if (_isSelfPoint)
            {
                //Определяем индекс соседа в одномерном массиве                    
                ulong neighbourInd = MathProcess.getIndexByMultidimArray(_coord_new, numCells);
                //Добавляем в список соседей
                _neighbours.Add(neighbourInd);
            }
            else
            {
                _isSelfPoint = true;
            }
            //Для последней координаты также применяем все смещения            
            for (int j = 1; j <= numLayer; j++)
            {
                //Смещение берем с 1 чтобы дважды не учитывать нулевое смещение - его добавим вне цикла
                for (int i = 1; i < _biases.Length; i++)
                {
                    //Заносим  координаты в массив координат соседа (применяем смещение к текущей координате)
                    _coord_new[coord_ind] = _coord_current[coord_ind] + _biases[i] * j;
                    //Проверяем не вышли ли за пределы сетки (за пределы по данной координате)
                    if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
                    {
                        //Определяем индекс соседа в одномерном массиве                    
                        ulong neighbourInd = MathProcess.getIndexByMultidimArray(_coord_new, numCells);
                        //Добавляем в список соседей
                        _neighbours.Add(neighbourInd);
                    }
                    else
                    {
                        //Если хотя бы одна координата клетки выходит за пределы допустимого - выходим
                        _coord_new[coord_ind] = -1;
                        return;
                    }
                }
            }
        }
        //Не последняя размерность клетки
        else
        {
            //Заносим  координаты в массив координат соседа c нулевым смещением
            _coord_new[coord_ind] = _coord_current[coord_ind];
            //Проверяем не вышли ли за пределы сетки (за пределы по данной координате)
            if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
            {
                traverseNeighbour(coord_ind, numLayer);
            }
            else
            {
                //Если хотя бы одна координата клетки выходит за пределы допустимого - выходим
                _coord_new[coord_ind] = -1;
                return;
            }

            //Последовательно погружаемся в циклы чтобы перебрать все варианты
            for (int j = 1; j <= numLayer; j++)
            {
                for (int i = 1; i < _biases.Length; i++)
                {
                    //Заносим  координаты в массив координат соседа (применяем смещение к текущей координате)
                    _coord_new[coord_ind] = _coord_current[coord_ind] + _biases[i] * j;
                    //Проверяем не вышли ли за пределы сетки (за пределы по данной координате)
                    if (_coord_new[coord_ind] < numCells[coord_ind] && _coord_new[coord_ind] >= 0)
                    {
                        traverseNeighbour(coord_ind, numLayer);
                    }
                    else
                    {
                        //Если хотя бы одна координата клетки выходит за пределы допустимого - выходим
                        _coord_new[coord_ind] = -1;
                        return;
                    }
                }
            }            
        }
    }
    
    /// <summary>
    /// K nearest neighbour с учетом времени и радиуса
    /// </summary>
    /// <param name="A">Массив точек</param>
    /// <param name="startTrain">Индекс с которого начинается обучение</param>
    /// <param name="endTrain">Индекс которым заканчивается обучение</param>
    /// <param name="startTest">Индекс начала тестирования</param>
    /// <param name="endTest">Индекс конца тестирования</param>
    public static string KNNwithTimeR(PointClassification[] A, int startTrain, int endTrain, int startTest, int endTest, string filename)
    {
        //размерность сетки
        dimensionGrid = A[0].coord.Length;
        //ищем максимумы и минимумы по каждой координате
        //чтобы определить размеры сетки
        mins = new float[dimensionGrid];
        maxs = new float[dimensionGrid];
        //инициализируем первым значением 
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
        //Устанавливаем размеры сетки с запасом
        /*
        for (int j = 0; j < dimensionGrid; j++)
        {
            mins[j] *= (1-kLen);
            maxs[j] *= (1 + kLen);
        }
         */ 
        //Определяем длину сетки по каждой координате
        
        //Длины сторон сетки
        //Определяем самую короткую координату
        float[] lens = new float[dimensionGrid];
        int minLenInd = 0;
        for (int i = 0; i < dimensionGrid; i++)
        {
            lens[i] = (maxs[i] - mins[i]);
            if (lens[i] < lens[minLenInd]) minLenInd = i;
        }


        //Число клеток по каждой координате сеток
        numCells = new int[dimensionGrid];        
        //По первой задаем
        numCells[minLenInd] = cellNum;
        ulong amountCells = 1;
        //длина стороны клетки по 1-й координате - для всех равна. Делим на квадраты (кубики)
        float lenCell = lens[minLenInd] / cellNum;
        //Определяем размеры сетки исходя из длины стороны клетки
        for (int i = 0; i < dimensionGrid; i++)
        {
            numCells[i] = (int) Math.Ceiling((double)lens[i] / lenCell);            
            //Всего клеток (в одномерном массиве)
            amountCells *= (ulong)numCells[i];
        }

        //Создаем сетку размерности dimensionGrid
        //Для хранения будем использовать псевдомногомерный массив - одномерный с многомерной адресацией
        
        //Одномерный массив, в котором будем хранить многомерную сетку
        CellClassification[] grid = new CellClassification[amountCells];
        //Координаты точки на сетке (индексы клеток)
        int[] coord = new int[dimensionGrid];
        //Размещаем все точки на сетке
        for (int i = startTrain; i <= endTrain; i++)
        {
            //индекс по каждой координате
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j]-mins[j])/lenCell));
            }
            //индекс в одномерном массиве
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }
             
            //В клетку сетки добавляем индекс точки,                                        
            grid[ind].points.Add(i);
            //а также увеличиваем счетчики классов
            grid[ind].classCounter[A[i].classNum]++;
        }
        //Таблица ресультатов - для каждой классифицируемой точки - ее настоящий класс и 
        //найденное распределение по классам и флаг правильности распознавания 
        float[,] res = new float[endTest - startTest + 1, numClasses + 7];

        //Классифицируем точки        
        //Обходим все точки подлежащие классификации
        
        //Результаты 
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
            //Определяем в какую клетку сетки попадает
            //индекс по каждой координате
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //индекс в одномерном массиве
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            //Если индекс выходит за пределы сетки пропускаем ее
            if (ind < 0 || ind >= amountCells)
            {
                continue;
            }

            //Определяем радиус, внутри которого будем брать точки (зависит от плотности точек в клетке)

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[2];
            }

            int amountPointsCell = 0;
            //Суммируем точки по всем классам
            for (int j = 0; j < numClasses; j++)
            {
                amountPointsCell += grid[ind].classCounter[j];
            }
            //радиус зависит от плотности. Формулу можно подбирать эмпирически
            float r = lenCell * ((float) 1 / (amountPointsCell+1)+1);

            //Определяем клетки внутри которых будем искать точки входящие в круг
            ///Для этого берем клетки со всех сторон, попадающие под радиус, а также диагнольные квадранты (кубы)
            ///Считаем что клетки достаточно квадратные. Оцениваем число слоев соседних клеток которые могут попасть 
            /// под радиус, при этом проверяем граничные
            /// 

            //Количество слоев клеток вокруг текущей
            int numLayersAround = (int)Math.Floor((double)(r / lenCell));

            //Перебираем все клетки: по каждой координате в обе стороны (перед и после)
            
            //Смещения к координате
            _biases[0] = 0;
            _biases[1] = 1;
            _biases[2] = -1;

            _coord_inds = new int[dimensionGrid];
            
            //Заносим индексы всех соседей в список _neighbours
            _neighbours.Clear();
            //Так как метод обхода рекурсивный, используем глобальные переменные
            //Флаг контроля
            _isSelfPoint = true;
            //Координаты текущей клетки
            _coord_current = new int[coord.Length];
            _coord_new = new int[coord.Length];
            for (int t = 0; t < coord.Length; t++)
            {
                _coord_current[t] = coord[t];
                _coord_new[t] = coord[t];
            }                           
            traverseNeighbour(-1, numLayersAround);
            //Сюда считаем точки-соседи по каждому классу
            float[,] classes = new float[numClasses,2];
            //Внутри каждой клетки-соседе перебираем все точки и сравниваем расстояние до нее с радиусом
            for (int n = 0; n < _neighbours.Count; n++)
            {
                //индекс клетки-соседа в массиве клеток
                ulong neighbour_ind = _neighbours[n];
                //Если в клетке есть точки
                if ((int)neighbour_ind < grid.Length && grid[neighbour_ind].points != null)
                {
                    //Обходим все точки в клетке-соседе и сравниваем расстояние до нее с радиусом
                    for (int p = 0; p < grid[neighbour_ind].points.Count; p++)
                    {
                        //индекс точки в клетке-соседе
                        int point_ind = grid[neighbour_ind].points[p];
                        //расстояние между точкой в клетке соседе и текущей точкой
                        float spacing = MathProcess.getPointSpacingManhaten(A[point_ind].coord, A[i].coord);
                        //Если точка входит в заданный радиус - включаем ее в список анализируемых точек-соседей
                        if (spacing < r)
                        {
                            //Прибавляем влияние по каждому классу
                            
                            //
                            //TimeSpan diff = A[i].time.Subtract(A[point_ind].time);
                            //float timeSpacing = (float) diff.TotalMinutes;
                            //spacing *= spacing;
                            //timeSpacing *= timeSpacing;
                            //Временной компонент не большой в сравнении с пространственным,
                            //т.к. его фактор должен учитываться суммарно для большого числа точек
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
            //Классифицируем точку после проверки всех соседей
            //Индекс наиболее вероятного класса
            int maxClassInd = 0;
            res[c, 0] = A[i].classNum;
            int m = 1;
            for (int z = 0; z < numClasses; z++)
            {
                //инициализируем найденный класс первым в списке
                res[c, m] = classes[z, 0];
                //количество соседей данного класса
                res[c, m + 1] = classes[z, 1];
                m = m + 2;
                //Если расстояние в пространстве и времени меньше, то это более вероятный класс
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
            
            //Если класс который мы определили совпадает с классом точки
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

            //В клетку сетки добавляем индекс точки,                                        
            grid[ind].points.Add(i);
            //а также увеличиваем счетчики классов
            grid[ind].classCounter[A[i].classNum]++;
        }
        DataProcess.ExportArray(res, filename, header);
        string report = "Done. count = " + _neighbours.Count.ToString();
        return report;         
    }


    /// <summary>
    /// K nearest neighbour с учетом времени и радиуса
    /// </summary>
    /// <param name="A">Массив точек</param>
    /// <param name="startTrain">Индекс с которого начинается обучение</param>
    /// <param name="endTrain">Индекс которым заканчивается обучение</param>
    /// <param name="startTest">Индекс начала тестирования</param>
    /// <param name="endTest">Индекс конца тестирования</param>
    public static string KNNSimple(PointClassificationSimple[] A, int startTrain, int endTrain, int startTest, int endTest, string filename)
    {
        //размерность сетки
        dimensionGrid = A[0].coord.Length;
        //ищем максимумы и минимумы по каждой координате
        //чтобы определить размеры сетки
        mins = new float[dimensionGrid];
        maxs = new float[dimensionGrid];
        //инициализируем первым значением 
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
        //Устанавливаем размеры сетки с запасом
        /*
        for (int j = 0; j < dimensionGrid; j++)
        {
            mins[j] *= (1-kLen);
            maxs[j] *= (1 + kLen);
        }
         */
        //Определяем длину сетки по каждой координате

        //Длины сторон сетки
        //Определяем самую короткую координату
        float[] lens = new float[dimensionGrid];
        int minLenInd = 0;
        for (int i = 0; i < dimensionGrid; i++)
        {
            lens[i] = (maxs[i] - mins[i]);
            if (lens[i] < lens[minLenInd]) minLenInd = i;
        }


        //Число клеток по каждой координате сеток
        numCells = new int[dimensionGrid];
        //По первой задаем
        numCells[minLenInd] = cellNum;
        ulong amountCells = 1;
        //длина стороны клетки по 1-й координате - для всех равна. Делим на квадраты (кубики)
        float lenCell = lens[minLenInd] / cellNum;
        //Определяем размеры сетки исходя из длины стороны клетки
        for (int i = 0; i < dimensionGrid; i++)
        {
            numCells[i] = (int)Math.Ceiling((double)lens[i] / lenCell);
            //Всего клеток (в одномерном массиве)
            amountCells *= (ulong)numCells[i];
        }

        //Создаем сетку размерности dimensionGrid
        //Для хранения будем использовать псевдомногомерный массив - одномерный с многомерной адресацией

        //Одномерный массив, в котором будем хранить многомерную сетку
        CellClassification[] grid = new CellClassification[amountCells];
        //Координаты точки на сетке (индексы клеток)
        int[] coord = new int[dimensionGrid];
        //Размещаем все точки на сетке
        for (int i = startTrain; i <= endTrain; i++)
        {
            //индекс по каждой координате
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //индекс в одномерном массиве
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[numClasses];
            }

            //В клетку сетки добавляем индекс точки,                                        
            grid[ind].points.Add(i);
            //а также увеличиваем счетчики классов
            grid[ind].classCounter[A[i].classNum]++;
        }
        //Таблица ресультатов - для каждой классифицируемой точки - ее настоящий класс и 
        //найденное распределение по классам и флаг правильности распознавания 
        float[,] res = new float[endTest - startTest + 1, numClasses + 7];

        //Классифицируем точки        
        //Обходим все точки подлежащие классификации

        //Результаты 
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
            //Определяем в какую клетку сетки попадает
            //индекс по каждой координате
            for (int j = 0; j < dimensionGrid; j++)
            {
                coord[j] = (int)Math.Floor((double)((A[i].coord[j] - mins[j]) / lenCell));
            }
            //индекс в одномерном массиве
            ulong ind = MathProcess.getIndexByMultidimArray(coord, numCells);

            //Если индекс выходит за пределы сетки пропускаем ее
            if (ind < 0 || ind >= amountCells)
            {
                continue;
            }

            //Определяем радиус, внутри которого будем брать точки (зависит от плотности точек в клетке)

            if (grid[ind].points == null)
            {
                grid[ind].points = new List<int>();
                grid[ind].classCounter = new int[2];
            }

            int amountPointsCell = 0;
            //Суммируем точки по всем классам
            for (int j = 0; j < numClasses; j++)
            {
                amountPointsCell += grid[ind].classCounter[j];
            }
            //радиус зависит от плотности. Формулу можно подбирать эмпирически
            float r = lenCell * ((float)1 / (amountPointsCell + 1) + 1);

            //Определяем клетки внутри которых будем искать точки входящие в круг
            ///Для этого берем клетки со всех сторон, попадающие под радиус, а также диагнольные квадранты (кубы)
            ///Считаем что клетки достаточно квадратные. Оцениваем число слоев соседних клеток которые могут попасть 
            /// под радиус, при этом проверяем граничные
            /// 

            //Количество слоев клеток вокруг текущей
            int numLayersAround = (int)Math.Floor((double)(r / lenCell));

            //Перебираем все клетки: по каждой координате в обе стороны (перед и после)

            //Смещения к координате
            _biases[0] = 0;
            _biases[1] = 1;
            _biases[2] = -1;

            _coord_inds = new int[dimensionGrid];

            //Заносим индексы всех соседей в список _neighbours
            _neighbours.Clear();
            //Так как метод обхода рекурсивный, используем глобальные переменные
            //Флаг контроля
            _isSelfPoint = true;
            //Координаты текущей клетки
            _coord_current = new int[coord.Length];
            _coord_new = new int[coord.Length];
            for (int t = 0; t < coord.Length; t++)
            {
                _coord_current[t] = coord[t];
                _coord_new[t] = coord[t];
            }
            traverseNeighbour(-1, numLayersAround);
            //Сюда считаем точки-соседи по каждому классу
            float[,] classes = new float[numClasses, 2];
            //Внутри каждой клетки-соседе перебираем все точки и сравниваем расстояние до нее с радиусом
            for (int n = 0; n < _neighbours.Count; n++)
            {
                //индекс клетки-соседа в массиве клеток
                ulong neighbour_ind = _neighbours[n];
                //Если в клетке есть точки
                if ((int)neighbour_ind < grid.Length && grid[neighbour_ind].points != null)
                {
                    //Обходим все точки в клетке-соседе и сравниваем расстояние до нее с радиусом
                    for (int p = 0; p < grid[neighbour_ind].points.Count; p++)
                    {
                        //индекс точки в клетке-соседе
                        int point_ind = grid[neighbour_ind].points[p];
                        //расстояние между точкой в клетке соседе и текущей точкой
                        float spacing = MathProcess.getPointSpacingManhaten(A[point_ind].coord, A[i].coord);
                        //Если точка входит в заданный радиус - включаем ее в список анализируемых точек-соседей
                        if (spacing < r)
                        {
                            //Прибавляем влияние по каждому классу

                            //
                            //TimeSpan diff = A[i].time.Subtract(A[point_ind].time);
                            //float timeSpacing = (float) diff.TotalMinutes;
                            //spacing *= spacing;
                            //timeSpacing *= timeSpacing;
                            //Временной компонент не большой в сравнении с пространственным,
                            //т.к. его фактор должен учитываться суммарно для большого числа точек
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
            //Классифицируем точку после проверки всех соседей
            //Индекс наиболее вероятного класса
            int maxClassInd = 0;
            res[c, 0] = A[i].classNum;
            int m = 1;
            for (int z = 0; z < numClasses; z++)
            {
                //инициализируем найденный класс первым в списке
                res[c, m] = classes[z, 0];
                //количество соседей данного класса
                res[c, m + 1] = classes[z, 1];
                m = m + 2;
                //Если расстояние в пространстве и времени меньше, то это более вероятный класс
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

            //Если класс который мы определили совпадает с классом точки
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

            //В клетку сетки добавляем индекс точки,                                        
            grid[ind].points.Add(i);
            //а также увеличиваем счетчики классов
            grid[ind].classCounter[A[i].classNum]++;
        }
        DataProcess.ExportArray(res, filename, header);
        string report = "Done. count = " + _neighbours.Count.ToString();
        return report;
    }
}
}
