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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using ILNumerics.Drawing.Shapes; 
using ILNumerics.Drawing.Controls; 
using ILNumerics.Exceptions; 
using ILNumerics.Drawing.Misc; 
using System.Drawing; 
using ILNumerics.BuiltInFunctions; 
using ILNumerics.Drawing.Interfaces;
using ILNumerics.Drawing.Labeling;
using ILNumerics.Drawing.Plots;

namespace ILNumerics.Drawing.Plots {
    /// <summary>
    /// simple updatable bar graph implementation, fixed number of bars, simple example implementation derived from ILSceneGraph
    /// </summary>
    public class ILBarGraph2D : ILPlot, IILLegendRenderer, IILPanelConfigurator {

        #region attributes 
        float m_barWidth; 
        ILQuad[] m_quads;
        ILLabel m_legendSampleLabel;
        ILLabel m_legendTextLabel; 
        int m_oldestBarIndex;
        int m_oldestOpacity; 
        #endregion 

        #region properties
        /// <summary>
        /// Sets the opacity for the 'oldest' bar. May be used as 'Fading-Out' effect. (Default: 255)
        /// </summary>
        int OpacityOldest {
            get { return m_oldestOpacity; }
            set { m_oldestOpacity = value; }
        }
        /// <summary>
        /// padding between graphs 
        /// </summary>
        public float BarWidth {
            get {
                return m_barWidth; 
            }
            set {
                m_barWidth = value;
                Invalidate(); 
            }
        }
        /// <summary>
        /// number of bars in the graph
        /// </summary>
        public override int Count {
            get {
                return m_quads.Length; 
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// create new 2D bar graph
        /// </summary>
        /// <param name="panel">hosting panel</param>
        /// <param name="data">numeric vector data, heights of bars</param>
        public ILBarGraph2D (ILPanel panel, ILBaseArray data) 
            : base (panel) {
            // spacing between + width of bars
            m_barWidth = 0.6f;
            m_oldestOpacity = 255; 
            // create the bars
            createQuads(data); 
            m_legendSampleLabel = new ILLabel(m_panel);
            m_legendSampleLabel.Text = "Bars";

            m_legendTextLabel = new ILLabel(m_panel);
            m_legendTextLabel.Text = "3D";

            m_oldestBarIndex = 0; 
        }
        #endregion

        #region public interface
        /// <summary>
        /// return reference to single bar (ILQuad) by index
        /// </summary>
        /// <param name="index">index of the bar</param>
        /// <returns>the bar shape as ILQuad</returns>
        public new ILQuad this [int index] {
            get { return m_quads[index]; }
        }
        /// <summary>
        /// add 'new' bar with new value and remove oldest bar
        /// </summary>
        /// <param name="value">height of new bar</param>
        /// <returns>discarded value</returns>
        public float Queue(float value) {
            // we move all shapes by 1 left
            ILPoint3Df offset = new ILPoint3Df(-1f,0,0);
            for (int i = 0; i < m_quads.Length; i++) {
                m_quads[i].Translate(offset);
                // FADE OUt old barsss .... 
                m_quads[(i + m_oldestBarIndex) % (m_quads.Length - 1)].Opacity
                    = (byte)(m_oldestOpacity + i * (255 - m_oldestOpacity) / m_quads.Length);
                // tell the scene to update its size
                m_quads[i].Invalidate();
            }
            // configure oldest graph
            ILQuad newestQuad = m_quads[m_oldestBarIndex];
            offset.X = m_quads.Length; 
            newestQuad.Translate(offset);
            float ret = newestQuad.Vertices[2].YPosition; 
            newestQuad.Vertices[2].YPosition = value;
            newestQuad.Vertices[3].YPosition = value;
            newestQuad.Opacity = 255; 

            if (++m_oldestBarIndex >= m_quads.Length) 
                m_oldestBarIndex = 0; 
            return ret;        
        }
        #endregion

        #region IILLegendRenderer Member
        /* In order to show up in the legend, the plot needs to 
         * implement the IILLegendRenderer interface. Here this is 
         * done very simple via a ILShapeLabel. The first one simple draws 
         * the text "Bars" in the sample area of the legend. The original 
         * ILLabel of the graph is then used to draw its text into the 
         * label area of the legend. 
         * Better implementations should check for 
         * ** the drawing not to exceed the sampleArea and labelArea rectangles
         * ** use 2 distinct labels instead of one (performance) 
         */ 
        public void DrawToLegend(ILRenderProperties p, Rectangle sampleArea, Rectangle labelArea) {
            m_legendSampleLabel.Position = sampleArea.Location;
            m_legendSampleLabel.Draw(p);

            m_legendTextLabel.Position = labelArea.Location;
            m_legendTextLabel.Draw(p); 
        }
        public Size LabelSize { get { return m_legendTextLabel.Size; } }
        #endregion

        #region private helper 
        protected void createQuads(ILBaseArray data) {
            if (data == null || data.Length == 0) {
                Clear(); 
                m_quads = new ILQuad[0];
            } else {
                Clear(); 
                m_quads = new ILQuad[data.Length]; 
                ILColorEnumerator colors = new ILColorEnumerator(); 
                ILArray<float> fData = null; 
                if (data is ILArray<float>) {
                    fData = (ILArray<float>)data; 
                } else {
                    fData = ILMath.tosingle(data);
                }
                for (int i = 0; i < m_quads.Length; i++) {
                    m_quads[i] = new ILQuad(m_panel); 
                    m_quads[i].Border.Visible = true; 
                    m_quads[i].FillColor = colors.NextColor(); 
                    ILPoint3Df pos = new ILPoint3Df(); 
                    pos.X = i - m_barWidth / 2; 
                    pos.Y = 0; 
                    pos.Z = -0.5f; 
                    m_quads[i].Vertices[0].Position = pos; 
                    pos.X += m_barWidth; 
                    m_quads[i].Vertices[1].Position = pos; 
                    pos.Y += fData.GetValue(i); 
                    m_quads[i].Vertices[2].Position = pos; 
                    pos.X -= m_barWidth; 
                    m_quads[i].Vertices[3].Position = pos; 
                    // label the bar
                    m_quads[i].Label.Text = i.ToString(); 
                    m_quads[i].Label.Anchor = new PointF(.5f,-1);   

                    // bars will be transparent, oldest fading to OpacityOldest
                    m_quads[i].Opacity = (byte)(m_oldestOpacity + i * (255 - m_oldestOpacity) / m_quads.Length); 
                    // add the bar to the scene graph node (base)
                    Add(m_quads[i]); 
                }
            }
        }
        #endregion

        #region IILPanelConfigurator Members

        public void  ConfigurePanel(ILPanel panel) {
            panel.Axes.XAxis.Visible = false;
            panel.InteractiveMode = InteractiveModes.ZoomRectangle;
            panel.Axes.YAxis.FarLines.Visible = false;
            panel.PlotBoxScreenSizeMode = PlotBoxScreenSizeMode.Optimal; 
        }

        #endregion
    }
}
