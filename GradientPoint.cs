using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace Rpis.TinyCLR.UI.Controls
{
    public partial class GradientProgressBar
    {
        public class GradientPoint
        {
            public readonly Color StartColor;
            public readonly Color EndColor;

            internal Brush Brush;
            internal int StartX;
            internal int EndX;
            internal int StartY;
            internal int EndY;

            internal int Width;
            private int _height;

            internal double Scale
            {
                get => this._scale;
                set => this._scale = value;
            }

            public double Weight => this._weight;
            private double _weight;
            private double _scale = 1;

            /// <summary>
            /// Create SolidColorBrush for selected color.
            /// </summary>
            /// <param name="weightCoefficient">
            /// Must take values greater than 0. Cannot take not a number (NaN), (-/+)Infinity, and less than and equal to 0.
            /// </param>
            /// <param name="color">
            /// Brush color.
            /// </param>
            public GradientPoint(double weightCoefficient, Color color) : this(weightCoefficient, color, color) { }
            /// <summary>
            /// Create LinearGradientBrush for selected colors.
            /// </summary>
            /// <param name="weightCoefficient">
            /// Must take values greater than 0. Cannot take not a number (NaN), (-/+)Infinity, and less than and equal to 0.
            /// </param>
            /// <param name="startColor">
            /// Start color brush.
            /// </param>
            /// <param name="endColor">
            /// End color brush.
            /// </param>
            public GradientPoint(double weightCoefficient, Color startColor, Color endColor)
            {
                if (double.IsNaN(weightCoefficient) || double.IsInfinity(weightCoefficient) || weightCoefficient <= 0) throw new ArgumentOutOfRangeException(nameof(weightCoefficient));
                this._weight = weightCoefficient;
                this.StartColor = startColor;
                this.EndColor = endColor;
            }

            internal void CreateBrush(int startX, int maxWidth, int availableWidth, int startY, int maxHeight)
            {
                var width = Math.Min((int)Math.Round(maxWidth * this._weight* this._scale), availableWidth);
                var endX = width + startX - 1;
                if (endX < startX) endX = startX;
                if (this.StartColor.ColorEquals(this.EndColor))
                {
                    this.Brush =
                        new SolidColorBrush(this.StartColor);
                }
                else
                {
                    this.Brush =
                        new LinearGradientBrush(this.StartColor, this.EndColor, startX, startY, endX, startY)
                        { MappingMode = BrushMappingMode.Absolute };
                }

                this.StartX = startX;
                this.EndX = endX;

                this.StartY = startY;
                this.EndY = maxHeight + startY;

                this.Width = width;
                this._height = maxHeight;
                if (this.Width < 0) this.Width = 0;
                if (this._height < 0) this._height = 0;
            }

            internal void DrawRectangleInContext(DrawingContext dc)
            {
                dc.DrawRectangle(this.Brush, null, this.StartX, this.StartY, this.Width, this._height);
            }
        }
    }
}