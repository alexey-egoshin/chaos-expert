/*************************************************************************
Copyright (c) 2007, Sergey Bochkanov (ALGLIB project).

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

- Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.

- Redistributions in binary form must reproduce the above copyright
  notice, this list of conditions and the following disclaimer listed
  in this license in the documentation and/or other materials
  provided with the distribution.

- Neither the name of the copyright holders nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*************************************************************************/

using System;

class descriptivestatistics
{
    /*************************************************************************
    Подпрограмма расчета моментов распределения: матожидания, дисперсии,
    скоса и эксцесса.

    Входные параметры:
        X       -   массив значений с нумерацией элементов [0..N-1]
        N       -   число значений в массиве
        
    Выходные параметры
        Mean    -   математическое ожидание
        Variance-   дисперсия
        Skewness-   скос (если дисперсия не равна 0)
        Kurtosis-   эксцесс (если дисперсия не равна 0)

      -- ALGLIB --
         Copyright 06.09.2006 by Bochkanov Sergey
    *************************************************************************/
    public static void calculatemoments(ref double[] x,
        int n,
        ref double mean,
        ref double variance,
        ref double skewness,
        ref double kurtosis)
    {
        int i = 0;
        double v = 0;
        double v1 = 0;
        double v2 = 0;
        double stddev = 0;

        mean = 0;
        variance = 0;
        skewness = 0;
        kurtosis = 0;
        stddev = 0;
        if( n<=0 )
        {
            return;
        }
        
        //
        // Mean
        //
        for(i=0; i<=n-1; i++)
        {
            mean = mean+x[i];
        }
        mean = mean/n;
        
        //
        // Variance (using corrected two-pass algorithm)
        //
        if( n!=1 )
        {
            v1 = 0;
            for(i=0; i<=n-1; i++)
            {
                v1 = v1+AP.Math.Sqr(x[i]-mean);
            }
            v2 = 0;
            for(i=0; i<=n-1; i++)
            {
                v2 = v2+(x[i]-mean);
            }
            v2 = AP.Math.Sqr(v2)/n;
            variance = (v1-v2)/(n-1);
            if( variance<0 )
            {
                variance = 0;
            }
            stddev = Math.Sqrt(variance);
        }
        
        //
        // Skewness and kurtosis
        //
        if( stddev!=0 )
        {
            for(i=0; i<=n-1; i++)
            {
                v = (x[i]-mean)/stddev;
                v2 = AP.Math.Sqr(v);
                skewness = skewness+v2*v;
                kurtosis = kurtosis+AP.Math.Sqr(v2);
            }
            skewness = skewness/n;
            kurtosis = kurtosis/n-3;
        }
    }


    /*************************************************************************
    Подпрограмма расчета среднего отклонения

    Входные параметры:
        X       -   массив значений с нумерацией элементов [0..N-1]
        N       -   число значений в массиве

    Выходные параметры
        ADev    -   среднее отклонение

      -- ALGLIB --
         Copyright 06.09.2006 by Bochkanov Sergey
    *************************************************************************/
    public static void calculateadev(ref double[] x,
        int n,
        ref double adev)
    {
        int i = 0;
        double mean = 0;

        mean = 0;
        adev = 0;
        if( n<=0 )
        {
            return;
        }
        
        //
        // Mean
        //
        for(i=0; i<=n-1; i++)
        {
            mean = mean+x[i];
        }
        mean = mean/n;
        
        //
        // ADev
        //
        for(i=0; i<=n-1; i++)
        {
            adev = adev+Math.Abs(x[i]-mean);
        }
        adev = adev/n;
    }


    /*************************************************************************
    Подпрограмма расчета медианы

    Входные параметры:
        X       -   массив значений с нумерацией элементов [0..N-1]
        N       -   число значений в массиве

    Выходные параметры
        Median  -   медиана
        
    ЗАМЕЧАНИЕ: медиана ищется путем частичной сортировки переданного  массива,
    поэтому массив передается по значению (создается копия), а не  по  ссылке,
    что несколько увеличивает время работы алгоритма - затраты на копирование.
    Зато при этом не меняется переданный массив.

      -- ALGLIB --
         Copyright 06.09.2006 by Bochkanov Sergey
    *************************************************************************/
    public static void calculatemedian(double[] x,
        int n,
        ref double median)
    {
        int i = 0;
        int ir = 0;
        int j = 0;
        int l = 0;
        int midp = 0;
        int k = 0;
        double a = 0;
        double temp = 0;
        double tval = 0;

        x = (double[])x.Clone();

        
        //
        // Some degenerate cases
        //
        median = 0;
        if( n<=0 )
        {
            return;
        }
        if( n==1 )
        {
            median = x[0];
            return;
        }
        if( n==2 )
        {
            median = 0.5*(x[0]+x[1]);
            return;
        }
        
        //
        // Common case, N>=3.
        // Choose X[(N-1)/2]
        //
        l = 0;
        ir = n-1;
        k = (n-1)/2;
        while( true )
        {
            if( ir<=l+1 )
            {
                
                //
                // 1 or 2 elements in partition
                //
                if( ir==l+1 & x[ir]<x[l] )
                {
                    tval = x[l];
                    x[l] = x[ir];
                    x[ir] = tval;
                }
                break;
            }
            else
            {
                midp = (l+ir)/2;
                tval = x[midp];
                x[midp] = x[l+1];
                x[l+1] = tval;
                if( x[l]>x[ir] )
                {
                    tval = x[l];
                    x[l] = x[ir];
                    x[ir] = tval;
                }
                if( x[l+1]>x[ir] )
                {
                    tval = x[l+1];
                    x[l+1] = x[ir];
                    x[ir] = tval;
                }
                if( x[l]>x[l+1] )
                {
                    tval = x[l];
                    x[l] = x[l+1];
                    x[l+1] = tval;
                }
                i = l+1;
                j = ir;
                a = x[l+1];
                while( true )
                {
                    do
                    {
                        i = i+1;
                    }
                    while( x[i]<a );
                    do
                    {
                        j = j-1;
                    }
                    while( x[j]>a );
                    if( j<i )
                    {
                        break;
                    }
                    tval = x[i];
                    x[i] = x[j];
                    x[j] = tval;
                }
                x[l+1] = x[j];
                x[j] = a;
                if( j>=k )
                {
                    ir = j-1;
                }
                if( j<=k )
                {
                    l = i;
                }
            }
        }
        
        //
        // If N is odd, return result
        //
        if( n%2==1 )
        {
            median = x[k];
            return;
        }
        a = x[n-1];
        for(i=k+1; i<=n-1; i++)
        {
            if( x[i]<a )
            {
                a = x[i];
            }
        }
        median = 0.5*(x[k]+a);
    }


    /*************************************************************************
    Подпрограмма расчета перцентиля

    Входные параметры:
        X       -   массив значений с нумерацией элементов [0..N-1]
        N       -   число значений в массиве
        P       -   перцентиль, 0<=P<=1

    Выходные параметры
        V       -   значение перцентиля (при необходимости для расчета
                    используется интерполяция)


      -- ALGLIB --
         Copyright 01.03.2008 by Bochkanov Sergey
    *************************************************************************/
    public static void calculatepercentile(double[] x,
        int n,
        double p,
        ref double v)
    {
        int i1 = 0;
        double t = 0;

        x = (double[])x.Clone();

        System.Diagnostics.Debug.Assert(n>1, "CalculatePercentile: N<=1!");
        System.Diagnostics.Debug.Assert(p>=0 & p<=1, "CalculatePercentile: incorrect P!");
        internalstatheapsort(ref x, n);
        if( p==0 )
        {
            v = x[0];
            return;
        }
        if( p==1 )
        {
            v = x[n-1];
            return;
        }
        t = p*(n-1);
        i1 = (int)Math.Floor(t);
        t = t-(int)Math.Floor(t);
        v = x[i1]*(1-t)+x[i1+1]*t;
    }


    /*************************************************************************
    Процедура для упорядочивания массива пирамидальной сортировкой

    Входные параметры:
        Arr -   сортируемый массив.
                Нумерация элементов от 0 до N-1
        N   -   размер массива

    Выходные параметры:
        Arr -   массив, упорядоченный по возрастанию.
                Нумерация элементов от 0 до N-1
    *************************************************************************/
    private static void internalstatheapsort(ref double[] arr,
        int n)
    {
        int i = 0;
        int j = 0;
        int k = 0;
        int t = 0;
        double tmp = 0;

        if( n==1 )
        {
            return;
        }
        i = 2;
        do
        {
            t = i;
            while( t!=1 )
            {
                k = t/2;
                if( arr[k-1]>=arr[t-1] )
                {
                    t = 1;
                }
                else
                {
                    tmp = arr[k-1];
                    arr[k-1] = arr[t-1];
                    arr[t-1] = tmp;
                    t = k;
                }
            }
            i = i+1;
        }
        while( i<=n );
        i = n-1;
        do
        {
            tmp = arr[i];
            arr[i] = arr[0];
            arr[0] = tmp;
            t = 1;
            while( t!=0 )
            {
                k = 2*t;
                if( k>i )
                {
                    t = 0;
                }
                else
                {
                    if( k<i )
                    {
                        if( arr[k]>arr[k-1] )
                        {
                            k = k+1;
                        }
                    }
                    if( arr[t-1]>=arr[k-1] )
                    {
                        t = 0;
                    }
                    else
                    {
                        tmp = arr[k-1];
                        arr[k-1] = arr[t-1];
                        arr[t-1] = tmp;
                        t = k;
                    }
                }
            }
            i = i-1;
        }
        while( i>=1 );
    }
}
