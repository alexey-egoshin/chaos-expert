/*************************************************************************
Copyright (c) 1992-2007 The University of Tennessee.  All rights reserved.

Contributors:
    * Sergey Bochkanov (ALGLIB project). Translation from FORTRAN to
      pseudocode.

See subroutines comments for additional copyrights.

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

class qr
{
    /*************************************************************************
    QR-разложение прямоугольной матрицы размером M x N

    Входные параметры:
        A   -   матрица A. Нумерация элементов: [0..M-1, 0..N-1]
        M   -   число строк в матрице A
        N   -   число столбцов в матрице A

    Выходные параметры:
        A   -   матрицы Q и R в компактной форме (см. ниже)
        Tau -   массив скалярных множителей, участвующих в формировании
                матрицы Q. Нумерация элементов [0.. Min(M-1,N-1)]

    Матрица A представляется, как A = QR, где Q ортогональная матрица размером
    M x M, а R  верхнетреугольная (или верхнетрапецоидальная) матрица размером
    M x N.

    После завершения работы подпрограммы на главной диагонали матрицы A и выше
    располагаются элементы матрицы R. В массиве Tau и под  главной  диагональю
    матрицы A располагаются элементы, формирующие матрицу Q, следующим способом:

    Матрица Q представляется, как произведение элементарных отражений

    Q = H(0)*H(2)*...*H(k-1),

    где k = min(m,n), а каждое H(i) имеет вид

    H(i) = 1 - tau * v * (v^T)

    где tau скалярная величина, хранящаяся в Tau[I], а v - вещественный вектор
    у которого v(0:i-1)=0, v(i)=1, v(i+1:m-1) хранится в элементах A(i+1:m-1,i).

      -- LAPACK routine (version 3.0) --
         Univ. of Tennessee, Univ. of California Berkeley, NAG Ltd.,
         Courant Institute, Argonne National Lab, and Rice University
         February 29, 1992.
         Translation from FORTRAN to pseudocode (AlgoPascal)
         by Sergey Bochkanov, ALGLIB project, 2005-2007.
    *************************************************************************/
    public static void rmatrixqr(ref double[,] a,
        int m,
        int n,
        ref double[] tau)
    {
        double[] work = new double[0];
        double[] t = new double[0];
        int i = 0;
        int k = 0;
        int minmn = 0;
        double tmp = 0;
        int i_ = 0;
        int i1_ = 0;

        if( m<=0 | n<=0 )
        {
            return;
        }
        minmn = Math.Min(m, n);
        work = new double[n-1+1];
        t = new double[m+1];
        tau = new double[minmn-1+1];
        
        //
        // Test the input arguments
        //
        k = minmn;
        for(i=0; i<=k-1; i++)
        {
            
            //
            // Generate elementary reflector H(i) to annihilate A(i+1:m,i)
            //
            i1_ = (i) - (1);
            for(i_=1; i_<=m-i;i_++)
            {
                t[i_] = a[i_+i1_,i];
            }
            reflections.generatereflection(ref t, m-i, ref tmp);
            tau[i] = tmp;
            i1_ = (1) - (i);
            for(i_=i; i_<=m-1;i_++)
            {
                a[i_,i] = t[i_+i1_];
            }
            t[1] = 1;
            if( i<n )
            {
                
                //
                // Apply H(i) to A(i:m-1,i+1:n-1) from the left
                //
                reflections.applyreflectionfromtheleft(ref a, tau[i], ref t, i, m-1, i+1, n-1, ref work);
            }
        }
    }


    /*************************************************************************
    Частичная "распаковка" матрицы Q из QR-разложения матрицы A.

    Входные параметры:
        A       -   матрицы Q и R в упакованной форме.
                    Результат работы RMatrixQR
        M       -   число строк в оригинальной матрице A. M>=0
        N       -   число столбцов в оригинальной матрице A. N>=0
        Tau     -   скалярные множители, формирующие Q.
                    Результат работы RMatrixQR.
        QColumns-   требуемое число столбцов матрицы Q. M>=QColumns>=0.

    Выходные параметры:
        Q       -   первые QColumns столбцов матрицы Q. Массив с нумерацией
                    элементов [0..M-1, 0..QColumns-1]. Если QColumns=0, то массив
                    не изменяется.

      -- ALGLIB --
         Copyright 2005 by Bochkanov Sergey
    *************************************************************************/
    public static void rmatrixqrunpackq(ref double[,] a,
        int m,
        int n,
        ref double[] tau,
        int qcolumns,
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

        System.Diagnostics.Debug.Assert(qcolumns<=m, "UnpackQFromQR: QColumns>M!");
        if( m<=0 | n<=0 | qcolumns<=0 )
        {
            return;
        }
        
        //
        // init
        //
        minmn = Math.Min(m, n);
        k = Math.Min(minmn, qcolumns);
        q = new double[m-1+1, qcolumns-1+1];
        v = new double[m+1];
        work = new double[qcolumns-1+1];
        for(i=0; i<=m-1; i++)
        {
            for(j=0; j<=qcolumns-1; j++)
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
            for(i_=1; i_<=m-i;i_++)
            {
                v[i_] = a[i_+i1_,i];
            }
            v[1] = 1;
            reflections.applyreflectionfromtheleft(ref q, tau[i], ref v, i, m-1, 0, qcolumns-1, ref work);
        }
    }


    /*************************************************************************
    Распаковка матрицы R из QR-разложения матрицы A.

    Входные параметры:
        A       -   матрицы Q и R в упакованной форме.
                    Результат работы подпрограммы RMatrixQR
        M       -   число строк в оригинальной матрице A. M>=0
        N       -   число столбцов в оригинальной матрице A. N>=0

    Выходные параметры:
        R       -   матрица R. Массив с нумерацией элементов [0..M-1, 0..N-1].

      -- ALGLIB --
         Copyright 2005 by Bochkanov Sergey
    *************************************************************************/
    public static void rmatrixqrunpackr(ref double[,] a,
        int m,
        int n,
        ref double[,] r)
    {
        int i = 0;
        int k = 0;
        int i_ = 0;

        if( m<=0 | n<=0 )
        {
            return;
        }
        k = Math.Min(m, n);
        r = new double[m-1+1, n-1+1];
        for(i=0; i<=n-1; i++)
        {
            r[0,i] = 0;
        }
        for(i=1; i<=m-1; i++)
        {
            for(i_=0; i_<=n-1;i_++)
            {
                r[i,i_] = r[0,i_];
            }
        }
        for(i=0; i<=k-1; i++)
        {
            for(i_=i; i_<=n-1;i_++)
            {
                r[i,i_] = a[i,i_];
            }
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine. See RMatrixQR for 0-based replacement.
    *************************************************************************/
    public static void qrdecomposition(ref double[,] a,
        int m,
        int n,
        ref double[] tau)
    {
        double[] work = new double[0];
        double[] t = new double[0];
        int i = 0;
        int k = 0;
        int mmip1 = 0;
        int minmn = 0;
        double tmp = 0;
        int i_ = 0;
        int i1_ = 0;

        minmn = Math.Min(m, n);
        work = new double[n+1];
        t = new double[m+1];
        tau = new double[minmn+1];
        
        //
        // Test the input arguments
        //
        k = Math.Min(m, n);
        for(i=1; i<=k; i++)
        {
            
            //
            // Generate elementary reflector H(i) to annihilate A(i+1:m,i)
            //
            mmip1 = m-i+1;
            i1_ = (i) - (1);
            for(i_=1; i_<=mmip1;i_++)
            {
                t[i_] = a[i_+i1_,i];
            }
            reflections.generatereflection(ref t, mmip1, ref tmp);
            tau[i] = tmp;
            i1_ = (1) - (i);
            for(i_=i; i_<=m;i_++)
            {
                a[i_,i] = t[i_+i1_];
            }
            t[1] = 1;
            if( i<n )
            {
                
                //
                // Apply H(i) to A(i:m,i+1:n) from the left
                //
                reflections.applyreflectionfromtheleft(ref a, tau[i], ref t, i, m, i+1, n, ref work);
            }
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine. See RMatrixQRUnpackQ for 0-based replacement.
    *************************************************************************/
    public static void unpackqfromqr(ref double[,] a,
        int m,
        int n,
        ref double[] tau,
        int qcolumns,
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

        System.Diagnostics.Debug.Assert(qcolumns<=m, "UnpackQFromQR: QColumns>M!");
        if( m==0 | n==0 | qcolumns==0 )
        {
            return;
        }
        
        //
        // init
        //
        minmn = Math.Min(m, n);
        k = Math.Min(minmn, qcolumns);
        q = new double[m+1, qcolumns+1];
        v = new double[m+1];
        work = new double[qcolumns+1];
        for(i=1; i<=m; i++)
        {
            for(j=1; j<=qcolumns; j++)
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
            vm = m-i+1;
            i1_ = (i) - (1);
            for(i_=1; i_<=vm;i_++)
            {
                v[i_] = a[i_+i1_,i];
            }
            v[1] = 1;
            reflections.applyreflectionfromtheleft(ref q, tau[i], ref v, i, m, 1, qcolumns, ref work);
        }
    }


    /*************************************************************************
    Obsolete 1-based subroutine. See RMatrixQR for 0-based replacement.
    *************************************************************************/
    public static void qrdecompositionunpacked(double[,] a,
        int m,
        int n,
        ref double[,] q,
        ref double[,] r)
    {
        int i = 0;
        int k = 0;
        double[] tau = new double[0];
        double[] work = new double[0];
        double[] v = new double[0];
        int i_ = 0;

        a = (double[,])a.Clone();

        k = Math.Min(m, n);
        if( n<=0 )
        {
            return;
        }
        work = new double[m+1];
        v = new double[m+1];
        q = new double[m+1, m+1];
        r = new double[m+1, n+1];
        
        //
        // QRDecomposition
        //
        qrdecomposition(ref a, m, n, ref tau);
        
        //
        // R
        //
        for(i=1; i<=n; i++)
        {
            r[1,i] = 0;
        }
        for(i=2; i<=m; i++)
        {
            for(i_=1; i_<=n;i_++)
            {
                r[i,i_] = r[1,i_];
            }
        }
        for(i=1; i<=k; i++)
        {
            for(i_=i; i_<=n;i_++)
            {
                r[i,i_] = a[i,i_];
            }
        }
        
        //
        // Q
        //
        unpackqfromqr(ref a, m, n, ref tau, m, ref q);
    }
}
