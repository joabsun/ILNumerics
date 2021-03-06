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
    public static unsafe int dlas2(double f, double g, double h__, ref double ssmin, ref double ssmax)
    {
        /* System generated locals */
        double d__1, d__2;

        /* Local variables */
        double c__, fa, ga, ha, _as, at, au, fhmn, fhmx;

        fa = abs(f);
        ga = abs(g);
        ha = abs(h__);
        fhmn = min(fa,ha);
        fhmx = max(fa,ha);
        if (fhmn == 0.0) {
	    ssmin = 0.0;
	    if (fhmx == 0.0) {
	        ssmax = ga;
	    } else {
    /* Computing 2nd power */
	        d__1 = min(fhmx,ga) / max(fhmx,ga);
	        ssmax = max(fhmx,ga) * sqrt(d__1 * d__1 + 1.0);
	    }
        } else {
	    if (ga < fhmx) {
	        _as = fhmn / fhmx + 1.0;
	        at = (fhmx - fhmn) / fhmx;
    /* Computing 2nd power */
	        d__1 = ga / fhmx;
	        au = d__1 * d__1;
	        c__ = 2.0 / (sqrt(_as * _as + au) + sqrt(at * at + au));
	        ssmin = fhmn * c__;
	        ssmax = fhmx / c__;
	    } else {
	        au = fhmx / ga;
	        if (au == 0.0) {

    /*              Avoid possible harmful underflow if exponent range */
    /*              asymmetric (true SSMIN may not underflow even if */
    /*              AU underflows) */

		    ssmin = fhmn * fhmx / ga;
		    ssmax = ga;
	        } else {
		    _as = fhmn / fhmx + 1.0;
		    at = (fhmx - fhmn) / fhmx;
    /* Computing 2nd power */
		    d__1 = _as * au;
    /* Computing 2nd power */
		    d__2 = at * au;
		    c__ = 1.0 / (sqrt(d__1 * d__1 + 1.0) + sqrt(d__2 * d__2 + 1.0));
		    ssmin = fhmn * c__ * au;
		    ssmin += ssmin;
		    ssmax = ga / (c__ + c__);
	        }
	    }
        }
        return 0;
    } 
}

