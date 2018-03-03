/*************************************************************************
Copyright (c) 2005-2007, Sergey Bochkanov (ALGLIB project).

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

class lq
{
    /*************************************************************************
    LQ-разложение прямоугольной матрицы размером M x N

    Входные параметры:
        A   -   матрица A. Нумерация элементов: [0..M-1, 0..N-1]
        M   -   число строк в матрице A
        N   -   число столбцов в матрице A

    Выходные параметры:
        A   -   матрицы L и Q в компактной форме (см. ниже)
        Tau -   массив скалярных множителей, участвующих в формировании
                матрицы Q. Нумерация элементов [0..Min(M,N)-1]

    Матрица A представляется, как A = LQ, где Q ортогональная матрица размером
    M x M, а R  нижнетреугольная  (или  нижнетрапецоидальная) матрица размером
    M x N.

    После завершения работы подпрограммы на главной диагонали матрицы A и ниже
    располагаются элементы матрицы L. В массиве Tau и над  главной  диагональю
    матрицы A располагаются элементы, формирующие матрицу Q, следующим способом:

    Матрица Q представляется, как произведение элементарных отражений

    Q = H(k-1)*H(k-2)*...*H(1)*H(0),

    где k = min(m,n), а каждое H(i) имеет вид

    H(i) = 1 - tau * v * (v^T)

    где tau скалярная величина, хранящаяся в Tau[I], а v - вещественный вектор
    у которого v(0:i-1)=0, v(i)=1, v(i+1:n-1) хранится в элементах A(i,i+1:n-1).

      -- ALGLIB --
         Copyright 2005-2007 by Bochkanov Sergey
    *************************************************************************/
    public static void rmatrixlq(ref double[,] a,
        int m,
        int n,
        ref double[] tau)
    {
        double[] work = new double[0];
        double[] t = new double[0];
        int i = 0;
        int k = 0;
        int minmn = 0;
        int maxmn = 0;
        double tmp = 0;
        int i_ = 0;
        int i1_ = 0;

        minmn = Math.Min(m, n);
        maxmn = Math.Max(m, n);
        work = new double[m+1];
        t = new double[n+1];
        tau = new double[minmn-1+1];
        k = Math.Min(m, n);
        for(i=0; i<=k-1; i++)
        {
            
            //
            // Generate elementary reflector H(i) to annihilate A(i,i+1:n-1)
            //
            i1_ = (i) - (1);
            for(i_=1; i_<=n-i;i_++)
            {
                t[i_] = a[i,i_+i1_];
            }
            reflections.generatereflection(ref t, n-i, ref tmp);
            tau[i] = tmp;
            i1_ = (1) - (i);
            for(i_=i; i_<=n-1;i_++)
            {
                a[i,i_] = t[i_+i1_];
            }
            t[1] = 1;
            if( i<n )
            {
                
                //
                // Apply H(i) to A(i+1:m,i:n) from the right
                //
                reflections.applyreflectionfromtheright(ref a, tau[i], ref t, i+1, m-1, i, n-1, ref work);
            }
        }
    }


    /*************************************************************************
    Частичная "распаковка" матрицы Q из LQ-разложения матрицы A.

    Входные параметры:
        A       -   матрицы L и Q в упакованной форме.
                    Результат работы RMatrixLQ.
        M       -   число строк в оригинальной матрице A. M>=0
        N       -   число столбцов в оригинальной матрице A. N>=0
        Tau     -   скалярные множители, формирующие Q.
                    Результат работы RMatrixLQ.
        QRows   -   требуемое число строк матрицы Q. N>=QRows>=0.

    Выходные параметры:
        Q       -   первые QRows строк матрицы Q. Массив с нумерацией
                    элементов [0..QRows-1, 0..N-1]. Если QRows=0, то массив
                    не изменяется.

      -- ALGLIB --
         Copyright 2005 by Bochkanov Sergey
    *************************************************************************/
    public static void rmatrixlqunpackq(ref double[,] a,
        int m,
        int n,
        ref double[] tau,
        int qrows,
        ref double[,] q)
    {
        int i = 0;
        int j = 0;
        int k = 0;
        int minmn = 0;
        double[] v = new double[0];
        double[] work = new double[0];
        int i_ = 0;
        int i1_ = 0;

        System.Diagnostics.Debug.Assert(qrows<=n, "RMatrixLQUnpackQ: QRows>N!");
        if( m<=0 | n<=0 | qrows<=0 )
        {
            return;
        }
        
        //
        // init
        //
        minmn = Math.Min(m, n);
        k = Math.Min(minmn, qrows);
        q = new double[qrows-1+1, n-1+1];
        v = new double[n+1];
        work = new double[qrows+1];
        for(i=0; i<=qrows-1; i++)
        {
            for(j=0; j<=n-1; j++)
            {
                if( i==j )
                {
                    q[i,j] = 1;
                }
                else
                {
                    q[i,j] = 0;
                }
            }
        }
        
        //
        // unpack Q
        //
        for(i=k-1; i>=0; i--)
        {
            
            //
            // Apply H(i)
            //
            i1_ = (i) - (1);
            for(i_=1; i_<=n-i;i_++)
            {
                v[i_] = a[i,i_+i1_];
            }
            v[1] = 1;
            reflections.applyreflectionfromtheright(ref q, tau[i], ref v, 0, qrows-1, i, n-1, ref work);
        }
    }


    /*************************************************************************
    Распаковка матрицы L из LQ-разложения матрицы A.

    Входные параметры:
        A       -   матрицы Q и L в упакованной форме.
                    Результат работы подпрограммы RMatrixLQ
        M       -   число строк в оригинальной матрице A. M>=0
        N       -   число столбцов в оригинальной матрице A. N>=0

    Выходные параметры:
        L       -   матрица L. Массив с нумерацией элементов [0..M-1, 0..N-1].

      -- ALGLIB --
         Copyright 2005-2007 by Bochkanov Sergey
    *************************************************************************/
    public static void rmatrixlqunpackl(ref double[,] a,
        int m,
        int n,
        ref double[,] l)
    {
        int i = 0;
        int k = 0;
        int i_ = 0;

        if( m<=0 | n<=0 )
        {
            return;
        }
        l = new double[m-1+1, n-1+1];
        for(i=0; i<=n-1; i++)
        {
            l[0,i] = 0;
        }
        for(i=1; i<=m-1; i++)
        {
            for(i_=0; i_<=n-1;i_++)
            {
                l[i,i_] = l[0,i_];
            }
        }
        for(i=0; i<=m-1; i++)
        {
            k = Math.Min(i, n-1);
            for(i_=0; i_<=k;i_++)
            {
                l[i,i_] = a[i,i_];
            }
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine
    See RMatrixLQ for 0-based replacement.
    *************************************************************************/
    public static void lqdecomposition(ref double[,] a,
        int m,
        int n,
        ref double[] tau)
    {
        double[] work = new double[0];
        double[] t = new double[0];
        int i = 0;
        int k = 0;
        int nmip1 = 0;
        int minmn = 0;
        int maxmn = 0;
        double tmp = 0;
        int i_ = 0;
        int i1_ = 0;

        minmn = Math.Min(m, n);
        maxmn = Math.Max(m, n);
        work = new double[m+1];
        t = new double[n+1];
        tau = new double[minmn+1];
        
        //
        // Test the input arguments
        //
        k = Math.Min(m, n);
        for(i=1; i<=k; i++)
        {
            
            //
            // Generate elementary reflector H(i) to annihilate A(i,i+1:n)
            //
            nmip1 = n-i+1;
            i1_ = (i) - (1);
            for(i_=1; i_<=nmip1;i_++)
            {
                t[i_] = a[i,i_+i1_];
            }
            reflections.generatereflection(ref t, nmip1, ref tmp);
            tau[i] = tmp;
            i1_ = (1) - (i);
            for(i_=i; i_<=n;i_++)
            {
                a[i,i_] = t[i_+i1_];
            }
            t[1] = 1;
            if( i<n )
            {
                
                //
                // Apply H(i) to A(i+1:m,i:n) from the right
                //
                reflections.applyreflectionfromtheright(ref a, tau[i], ref t, i+1, m, i, n, ref work);
            }
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine
    See RMatrixLQUnpackQ for 0-based replacement.
    *************************************************************************/
    public static void unpackqfromlq(ref double[,] a,
        int m,
        int n,
        ref double[] tau,
        int qrows,
        ref double[,] q)
    {
        int i = 0;
        int j = 0;
        int k = 0;
        int minmn = 0;
        double[] v = new double[0];
        double[] work = new double[0];
        int vm = 0;
        int i_ = 0;
        int i1_ = 0;

        System.Diagnostics.Debug.Assert(qrows<=n, "UnpackQFromLQ: QRows>N!");
        if( m==0 | n==0 | qrows==0 )
        {
            return;
        }
        
        //
        // init
        //
        minmn = Math.Min(m, n);
        k = Math.Min(minmn, qrows);
        q = new double[qrows+1, n+1];
        v = new double[n+1];
        work = new double[qrows+1];
        for(i=1; i<=qrows; i++)
        {
            for(j=1; j<=n; j++)
            {
                if( i==j )
                {
                    q[i,j] = 1;
                }
                else
                {
                    q[i,j] = 0;
                }
            }
        }
        
        //
        // unpack Q
        //
        for(i=k; i>=1; i--)
        {
            
            //
            // Apply H(i)
            //
            vm = n-i+1;
            i1_ = (i) - (1);
            for(i_=1; i_<=vm;i_++)
            {
                v[i_] = a[i,i_+i1_];
            }
            v[1] = 1;
            reflections.applyreflectionfromtheright(ref q, tau[i], ref v, 1, qrows, i, n, ref work);
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine
    *************************************************************************/
    public static void lqdecompositionunpacked(double[,] a,
        int m,
        int n,
        ref double[,] l,
        ref double[,] q)
    {
        int i = 0;
        int j = 0;
        double[] tau = new double[0];

        a = (double[,])a.Clone();

        if( n<=0 )
        {
            return;
        }
        q = new double[n+1, n+1];
        l = new double[m+1, n+1];
        
        //
        // LQDecomposition
        //
        lqdecomposition(ref a, m, n, ref tau);
        
        //
        // L
        //
        for(i=1; i<=m; i++)
        {
            for(j=1; j<=n; j++)
            {
                if( j>i )
                {
                    l[i,j] = 0;
                }
                else
                {
                    l[i,j] = a[i,j];
                }
            }
        }
        
        //
        // Q
        //
        unpackqfromlq(ref a, m, n, ref tau, n, ref q);
    }
}
