﻿#region ORIGINS, COPYRIGHTS, AND LICENSE
/*

This C# version of LAPACK is derivied from http://www.netlib.org/clapack/,
and the original copyright and license is as follows:

Copyright (c) 1992-2008 The University of Tennessee.  All rights reserved.
$COPYRIGHT$ Additional copyrights may follow $HEADER$

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
 */
#endregion

public partial class ManagedLapack
{
    public static unsafe double dnrm2(int n, double* x, int incx)
    {
        /* System generated locals */
        int i__1, i__2;
        double ret_val, d__1;

        /* Local variables */
        int ix;
        double ssq, norm, scale, absxi;

        /* Parameter adjustments */
        --x;

        /* Function Body */
        if (n < 1 || incx < 1) {
	    norm = 0;
        } else if (n == 1) {
	    norm = abs(x[1]);
        } else {
	    scale = 0;
	    ssq = 1;
    /*        The following loop is equivalent to this call to the LAPACK */
    /*        auxiliary routine: */
    /*        CALL DLASSQ( N, X, INCX, SCALE, SSQ ) */

	    i__1 = (n - 1) * incx + 1;
	    i__2 = incx;
	    for (ix = 1; i__2 < 0 ? ix >= i__1 : ix <= i__1; ix += i__2) {
	        if (x[ix] != 0) {
                d__1 = x[ix]; absxi = abs(d__1);
		    if (scale < absxi) {
    /* Computing 2nd power */
		        d__1 = scale / absxi;
		        ssq = ssq * (d__1 * d__1) + 1;
		        scale = absxi;
		    } else {
    /* Computing 2nd power */
		        d__1 = absxi / scale;
		        ssq += d__1 * d__1;
		    }
	        }
    /* L10: */
	    }
	    norm = scale * sqrt(ssq);
        }

        ret_val = norm;
        return ret_val;
    } 
}

