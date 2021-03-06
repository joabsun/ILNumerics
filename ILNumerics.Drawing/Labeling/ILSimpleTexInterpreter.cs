#region Copyright GPLv3
//
//  This file is part of ILNumerics.Net. 
//
//  ILNumerics.Net supports numeric application development for .NET 
//  Copyright (C) 2007, H. Kutschbach, http://ilnumerics.net 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
//  Non-free licenses are also available. Contact info@ilnumerics.net 
//  for details.
//
#endregion

using System;
using System.Text;
using System.Drawing; 
using System.Collections.Generic;
using ILNumerics.Drawing; 
using ILNumerics.Drawing.Interfaces; 

namespace ILNumerics.Drawing.Labeling {
    /// <summary>
    /// Simple, (partial) tex symbol interpreter 
    /// </summary>
    /// <remarks>this is the default interpreter for all ILLabelingElements</remarks>
    public class ILSimpleTexInterpreter : ILSimpleInterpreter {

#region attributes
        public static readonly ILKeywords Keywords = new ILKeywords(); 
#endregion

#region constructor
        
        /// <summary>
        /// create a new instance of a simple text interpreter
        /// </summary>
        public ILSimpleTexInterpreter () : base () { }

#endregion

#region helper functions 

        protected override void parseString (string expression, Font font, Point offset, Color color, 
                                    IILTextRenderer renderer, ref Size size, 
                                    ref List<ILRenderQueueItem> queue) {
            int pos = 0;
            string key, itemText; 
            RectangleF bmpSize = new RectangleF(); 
            int curHeigth = 0, curWidth = 0; 
            Bitmap itemBMP = null; 
            int lineHeight = 0, lineWidth = 0; 
            Size itemSize = Size.Empty; 
            while (pos < expression.Length) {
                itemText = expression.Substring(pos++,1);
                #region special position control sequences
                if (itemText == "\r") {
                    queue.Add(new ILRenderQueueItem(itemText,0,0, color)); 
                    if (curWidth < lineWidth) 
                        curWidth = lineWidth; 
                    lineWidth = 0; 
                    continue; 
                } else if (itemText == "\n") {
                    queue.Add(new ILRenderQueueItem(itemText,0,0, color));
                    curHeigth += lineHeight; 
                    lineHeight = 0;
                    if (curWidth < lineWidth) 
                        curWidth = lineWidth; 
                    lineWidth = 0; 
                    continue;
                #endregion
                } else if (itemText == "\\") {   
                    #region font control
                    if (pos < expression.Length - 2) {
                        #region test for font control sequences: \it,\bf,\rm
                        if (expression[pos] == 'i' && expression[pos+1] == 't') {
                            font = new Font(font,font.Style | FontStyle.Italic); 
                            pos += 2; continue; 
                        } else if (expression[pos] == 'b' && expression[pos+1] == 'f') {
                            font = new Font(font,font.Style | FontStyle.Bold); 
                            pos += 2; continue; 
                        } else if (expression[pos] == 'r' && expression[pos+1] == 'm') {
                            font = new Font(font,FontStyle.Regular); 
                            pos += 2; continue;
                        }
                        #endregion
                        #region fontname,-size,-color
                        if (parseKeyword(expression,ref pos,Keywords.Reset)) {
                            color = Color.Empty; 
                            font = m_normalFont; 
                            offset = new Point(0,0);
                            continue; 
                        }
                        string parameter = ""; 
                        if (parseKeywordArgumented(expression,ref pos,Keywords.Fontname,ref parameter)) {
                            font = new Font(parameter,font.Size,font.Style,font.Unit); 
                            continue; 
                        }
                        if (parseKeywordArgumented(expression,ref pos,Keywords.Fontsize,ref parameter)) {
                            int newSize;
                            if (!int.TryParse(parameter,out newSize)) 
                                continue; 
                            if (parameter.StartsWith("+") && newSize > 0 ) {
                                newSize += (int)font.Size; 
                                if (newSize > 40) newSize = 40;    
                            } else if (parameter.StartsWith("-") && -newSize < font.Size) {
                                newSize = (int)font.Size + newSize; 
                            } 
                            if (newSize > 0 && newSize < 40) {
                                offset.Y += (int)Math.Round(font.Size - newSize);  
                                font = new Font(font.Name,newSize,font.Style,font.Unit); 
                            }
                            continue;
                        }
                        if (parseKeywordArgumented(expression,ref pos,Keywords.Color,ref parameter)) {
                            parseColor(parameter, ref color);   
                            continue; 
                        }
                        #endregion
                    }
                    #endregion
                    //if (pos < expression.Length - "size".Length)
                    #region handle predefined symbols 
                    TextSymbols symbol = matchSymbol(expression,ref pos); 
                    if (symbol != TextSymbols.nothing) {
                        itemText = TranslateSymbol(symbol); 
                        if (String.IsNullOrEmpty (itemText)) continue; 
                    }
                    #endregion
                    #region lower- upper indices
                } else if (pos < expression.Length && itemText == "_") {
                    int end; 
                    if (pos < expression.Length-1 && expression[pos] == '{') {
                        pos ++; 
                        // find end brace & remove
                        end = expression.IndexOf('}',pos)-1;
                        if (end > 0 && end < expression.Length) {
                            parseString(
                                expression.Substring(pos,end-pos+1),
                                new Font(font.Name,font.Size * 0.7f,font.Style,font.Unit),
                                new Point(offset.X,offset.Y + (int)(0.3f * font.Height)),
                                color, renderer, ref size, ref queue);
                            pos = end+2;
                            continue; 
                        }
                    } 
                    // cache next char only
                    parseString(
                        expression.Substring(pos++,1),
                        new Font(font.Name,font.Size * 0.7f,font.Style,font.Unit),
                        new Point(offset.X,offset.Y + (int)(0.3f * font.Height)),
                        color, renderer, ref size, ref queue);
                    continue; 
                } else if (pos < expression.Length && itemText == "^") {
                    int end; 
                    //offset.Y += 0.8f * font.Height; 
                    if (pos < expression.Length-1 && expression[pos] == '{') {
                        pos ++; 
                        // find end brace & remove
                        end = expression.IndexOf('}',pos)-1;
                        if (end > 0 && end < expression.Length) { 
                            parseString(
                                expression.Substring(pos,end-pos+1),
                                new Font(font.Name,font.Size * 0.7f,font.Style,font.Unit),
                                new Point(offset.X,offset.Y - (int)(0.2f * font.Height)),
                                color, renderer, ref size, ref queue);
                            pos = end+2;
                            continue; 
                        }
                    } 
                    // cache next char only
                    parseString(
                        expression.Substring(pos++,1),
                        new Font(font.Name,font.Size * 0.7f,font.Style,font.Unit),
                        new Point(offset.X,offset.Y - (int)(0.2f * font.Height))
                        ,color, renderer, ref size, ref queue);
                    continue; 
                    #endregion
                }
                key = ILHashCreator.Hash(font,itemText); 
                
                if (renderer.TryGetSize(key, ref itemSize)) {
                    queue.Add(new ILRenderQueueItem(key,offset,color));
                    if (itemSize.Height > lineHeight) lineHeight = itemSize.Height; 
                    lineWidth += (int)itemSize.Width;
                } else {
                    lock (this) {
                        itemBMP = transformItem(itemText,font,out bmpSize); 
                        renderer.Cache(key,itemBMP,bmpSize); 
                        queue.Add(new ILRenderQueueItem(key,offset,color)); 
                        // update size
                        if (bmpSize.Height > lineHeight) 
                            lineHeight = (int)bmpSize.Height; 
                        lineWidth += (int)bmpSize.Width;
                    }
                }
            }
            size.Width += ((curWidth>lineWidth)?curWidth:lineWidth);
            size.Height = curHeigth + lineHeight; 
        }

        private bool parseKeyword(string text, ref int pos, string keyword) {
            if (pos < text.Length - keyword.Length                 
                && text.Substring(pos,keyword.Length) == keyword) {
                pos += keyword.Length; 
                return true; 
            }
            return false; 
        }

        private void parseColor(string parameter, ref Color color) {
            if (!String.IsNullOrEmpty(parameter) && parameter[0] == '#') {
                // try to parse for rgb value. Format: #D2E540
                if (parameter.Length == 7) {
                    IFormatProvider format = System.Threading.Thread.CurrentThread.CurrentCulture; 
                    byte r,g,b; 
                    // R - value
                    if (!byte.TryParse(parameter.Substring(1,2),
                                      System.Globalization.NumberStyles.HexNumber,
                                      format,out r)) return; 
                    // G - value
                    if (!byte.TryParse(parameter.Substring(3,2),
                                      System.Globalization.NumberStyles.HexNumber,
                                      format,out g)) return;
                    // B - value
                    if (!byte.TryParse(parameter.Substring(5,2),
                                      System.Globalization.NumberStyles.HexNumber,
                                      format,out b)) return;
                    color = Color.FromArgb(r,g,b); 
                } 
            } else {
                // try to parse for known color name
                try {
                    color = Color.FromName(parameter); 
                } catch {}
            }
        }
        private bool parseKeywordArgumented(string text, ref int pos, string keyword, ref string parameter) {
            int tmpPos = pos; 
            if (pos < text.Length - keyword.Length                 
                && text.Substring(pos,keyword.Length) == keyword) {
                tmpPos = pos+keyword.Length; 
                if (text[tmpPos] != '{' 
                    || tmpPos == text.Length-1) return false; 
                int end = text.IndexOf('}',++tmpPos); 
                if (end < tmpPos) return false; 
                parameter = text.Substring(tmpPos,end-tmpPos).Trim(); 
                pos = end + 1;
                return true; 
            }
            return false; 
        }
        /// <summary>
        /// extract TextSymbol from text 
        /// </summary>
        /// <param name="text">text to extract symbol from</param>
        /// <param name="pos">current text character position</param>
        /// <returns>one of TextSymbol enumeration values</returns>
        /// <remarks>if one symbol was found, its enum representation is 
        /// returned and pos is increased by the corresponding number of 
        /// characters. If no matching symbol was found, pos is not altered 
        /// and TextSymbols.nothing will be returned.</remarks>
        internal static TextSymbols matchSymbol (string text, ref int pos) {
            foreach (string sym in Enum.GetNames(typeof(TextSymbols))) {
                if (pos <= text.Length - sym.Length) {
                    if (text.Substring(pos,sym.Length) != sym) {
                        continue; 
                    }
                    if (sym == "nothing") continue; 
                    pos += sym.Length; 
                    return (TextSymbols)Enum.Parse(typeof(TextSymbols),sym); 
                }
            }
            return TextSymbols.nothing; 
        }
        /// <summary>
        ///  translates TextSymbol enum value to unicode character
        /// </summary>
        /// <param name="symbol">enum representation</param>
        /// <returns>unicode character</returns>
        /// <remarks>refers to: http://www.decodeunicode.org/ (f.e.)</remarks>
        public static string TranslateSymbol (TextSymbols symbol) {
            #region symbol matching
            switch (symbol) {
                case TextSymbols.alpha:
                    return "α";
                case TextSymbols.beta:
                    return "β";
                case TextSymbols.gamma:
                    return "γ";
                case TextSymbols.delta:
                    return "δ";
                case TextSymbols.epsilon:
                    return "ε";
                case TextSymbols.zeta:
                    return "ζ";
                case TextSymbols.eta:
                    return "η";
                case TextSymbols.theta:
                    return "θ";
                case TextSymbols.vartheta:
                    return "\u03d1";
                case TextSymbols.iota:
                    return "ι";
                case TextSymbols.kappa:
                    return "κ";
                case TextSymbols.lambda:
                    return "λ";
                case TextSymbols.mu:
                    return "μ";
                case TextSymbols.nu:
                    return "ν";
                case TextSymbols.xi:
                    return "ξ";
                case TextSymbols.pi:
                    return "π";
                case TextSymbols.rho:
                    return "ρ";
                case TextSymbols.sigma:
                    return "σ";
                case TextSymbols.varsigma:
                    return "ς";
                case TextSymbols.tau:
                    return "τ";
                case TextSymbols.upsilon:
                    return "υ";
                case TextSymbols.phi:
                    return "φ";
                case TextSymbols.chi:
                    return "χ";
                case TextSymbols.psi:
                    return "ψ";
                case TextSymbols.omega:
                    return "ω";
                case TextSymbols.Gamma: 
                    return "Γ"; 
                case TextSymbols.Delta:
                    return "Δ";
                case TextSymbols.Theta:
                    return "Θ";
                case TextSymbols.Lambda:
                    return "Λ";
                case TextSymbols.Xi: 
                    return "Ξ";
                case TextSymbols.Pi:
                    return "\u03a0"; //"Π"; 
                case TextSymbols.Sigma:
                    return "Σ";
                case TextSymbols.Upsilon:
                    return "Υ";
                case TextSymbols.Phi:
                    return "Φ";
                case TextSymbols.Psi:
                    return "Ψ";
                case TextSymbols.Omega:
                    return "Ω"; 
                case TextSymbols.forall:
                    return "\u2200";
                case TextSymbols.exists:
                    return "\u2203";  
                case TextSymbols.ni:
                    return "\u220b"; 
                case TextSymbols.cong:
                    return "\u2205"; 
                case TextSymbols.neq:
                    return "\u2260"; 
                case TextSymbols.equiv:
                    return "\u2261"; 
                case TextSymbols.approx:
                    return "\u2240"; 
                case TextSymbols.aleph:
                    return "\u2235"; 
                case TextSymbols.Im:
                    return "\u2111"; 
                case TextSymbols.Re:
                    return "\u211b"; 
                case TextSymbols.wp:
                    return "\u2118"; 
                case TextSymbols.otimes:
                    return "\u2297"; 
                case TextSymbols.oplus:
                    return "\u2295"; 
                case TextSymbols.oslash:
                    return "\u2205"; 
                case TextSymbols.cap:
                    return "\u22c2"; 
                case TextSymbols.cup:
                    return "\u22c3"; 
                case TextSymbols.supseteq:
                    return "\u2287"; 
                case TextSymbols.supset:
                    return "\u2283"; 
                case TextSymbols.subseteq:
                    return "\u2286"; 
                case TextSymbols.subset:
                    return "\u2282"; 
                case TextSymbols.int_:
                    return "\u222b"; 
                case TextSymbols.in_:
                    return "\u2208"; 
                case TextSymbols.o:
                    return "\u25cb"; 
                case TextSymbols.rfloor:
                    return "\u230b"; 
                case TextSymbols.lceil:
                    return "\u2308"; 
                case TextSymbols.nabla:
                    return "\u2207"; 
                case TextSymbols.lfloor:
                    return "\u230a"; 
                case TextSymbols.cdot:
                    return "\u2219"; 
                case TextSymbols.ldots:
                    return "\u2026"; 
                case TextSymbols.cdots:
                    return "\u220f"; 
                case TextSymbols.perp:
                    return "\u22a5"; 
                case TextSymbols.neg:
                    return "\u2511"; 
                case TextSymbols.prime:
                    return "\u2032"; 
                case TextSymbols.wedge:
                    return "\u22c0"; 
                case TextSymbols.times:
                    return "\u2a09"; 
                case TextSymbols.Null:
                    return "\u2205"; 
                case TextSymbols.rceil:
                    return "\u2309"; 
                case TextSymbols.surd:
                    return "\u221a"; 
                case TextSymbols.mid:
                    return "|"; 
                case TextSymbols.vee:
                    return "\u22c1"; 
                case TextSymbols.varpi:
                    return "\u03d6"; 
                case TextSymbols.copyright:
                    return "\u00a9"; 
                case TextSymbols.langle:
                    return "\u2329"; 
                case TextSymbols.rangle:
                    return "\u232a"; 
                case TextSymbols.sim: 
                    return "\u223c"; 
                case TextSymbols.leq: 
                    return "\u2264"; 
                case TextSymbols.infty: 
                    return "\u221e"; 
                case TextSymbols.leftrightarrow: 
                    return "\u21d4"; 
                case TextSymbols.leftarrow: 
                    return "\u21d0"; 
                case TextSymbols.uparrow: 
                    return "\u21d1"; 
                case TextSymbols.rightarrow: 
                    return "\u21d2"; 
                case TextSymbols.downarrow:         
                    return "\u21d3"; 
                case TextSymbols.circ: 
                    return "\u25cc"; 
                case TextSymbols.pm: 
                    return "\u00b1"; 
                case TextSymbols.geq: 
                    return "\u2265"; 
                case TextSymbols.propto: 
                    return "\u221d"; 
                case TextSymbols.partial: 
                    return "\u2202"; 
                case TextSymbols.div: 
                    return "\u00f7"; 
                default:
                    break;
            }
            return ""; 
            #endregion
        }

#endregion

#region symbol definitions

        /// <summary>
        /// available keywords, interpretable by this IILInterpreter
        /// </summary>
        /// <remarks><para>The static instance ILSimpleTexInterpreter.Keywords can be 
        /// used to alter the keywords used to switch to different font styles etc.</para></remarks>
        public class ILKeywords {
            private string m_fontname = "fontname";
            /// <summary>
            /// placeholder for font name control sequence
            /// </summary>
            public string Fontname {
                get { return m_fontname; }
                set { if (!string.IsNullOrEmpty(value)) m_fontname = value; }
            }     
            private string m_fontsize = "fontsize";
            /// <summary>
            /// placeholder for font size control sequence
            /// </summary>
            public string Fontsize {
                get { return m_fontsize; }
                set { if (!string.IsNullOrEmpty(value)) m_fontsize = value; }
            }     
            private string m_italic = "it";
            /// <summary>
            /// placeholder for italic font control sequence
            /// </summary>
            public string Italic {
                get { return m_italic; }
                set { if (!string.IsNullOrEmpty(value)) m_italic = value; }
            }     
            private string m_bold = "bf";
            /// <summary>
            /// placeholder for bold font control sequence
            /// </summary>
            public string Bold {
                get { return m_bold; }
                set { if (!string.IsNullOrEmpty(value)) m_bold = value; }
            }     
            private string m_color = "color";
            /// <summary>
            /// placeholder for font color control sequence
            /// </summary>
            public string Color {
                get { return m_color; }
                set { if (!string.IsNullOrEmpty(value)) m_color = value; }
            }     
            private string m_reset = "reset";
            /// <summary>
            /// placeholder for text control sequence 'reset to initial value' 
            /// </summary>
            public string Reset {
                get { return m_reset; }
                set { if (!string.IsNullOrEmpty(value)) m_reset = value; }
            }     
        }
        /// <summary>
        /// all available TextSymbols (like \\Alpha etc.) this IILTextInterpreter understands
        /// </summary>
        public enum TextSymbols {
            alpha,
            beta,
            gamma,
            delta,
            epsilon,
            zeta,
            eta,
            theta,
            vartheta,
            iota,
            kappa,
            lambda,
            mu,
            nu,
            xi,
            pi,
            rho,
            sigma,
            varsigma,
            tau, 
            upsilon,
            phi,
            chi,
            psi,
            omega,
            Gamma,
            Delta,
            Theta,
            Lambda,
            Xi,
            Pi,
            Sigma,
            Upsilon,
            Phi,
            Psi,
            Omega,
            forall,
            exists,
            ni,
            cong,
            neq,
            equiv,
            approx,
            aleph,
            Im,
            Re,
            wp,
            otimes,
            oplus,
            oslash,
            cap,
            cup,
            supseteq,
            supset,
            subseteq,
            subset,
            int_,
            in_,
            o,
            rfloor,
            lceil,
            nabla,
            lfloor,
            cdot,
            ldots,
            perp,
            neg,
            prime,
            wedge,
            times,
            Null,
            rceil,
            surd,
            mid,
            vee,
            varpi,
            copyright,
            langle,
            rangle,
            nothing,
            sim,
            leq,
            infty,
            clubsuit,
            diamondsuit,
            heartsuit,
            spadesuit,
            leftrightarrow,
            leftarrow,
            uparrow,
            rightarrow,
            downarrow,
            circ,
            pm,
            geq,
            propto,
            partial,
            div,
            cdots
        }

#endregion

    }
}
