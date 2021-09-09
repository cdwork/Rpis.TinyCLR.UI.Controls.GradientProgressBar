using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;

namespace Rpis.TinyCLR.UI.Controls
{
    public partial class GradientProgressBar : UIElement
    {
        private object _locker = new object();
        private ArrayList _gradientPoints;

        public Color BorderColor
        {
            get => this._borderColor;
            set
            {
                this._borderColor = value;
                this._borderBrush = new SolidColorBrush(this._borderColor);
            }
        }


        public int MinValue
        {
            get => this._minValue;
            set
            {
                if (value == this._minValue) return;
                this._minValue = value;
                if (this._minValue > this._maxValue) this._maxValue = this._minValue;
                if (this._value < this._minValue) this._value = this._minValue;
                GetCutOffValue();
            }
        }

        public int MaxValue
        {
            get => this._maxValue;
            set
            {
                if (value == this._maxValue) return;
                this._maxValue = value;
                if (this._minValue > this._maxValue) this._minValue = this._maxValue;
                if (this._value > this._maxValue) this._value = this._maxValue;
                GetCutOffValue();
            }
        }


        public int Value
        {
            get => this._value;
            set
            {
                if (value == this._value) return;
                this._value = value;
                if (this._value < this._minValue) this._value = this._minValue;
                if (this._value > this._maxValue) this._value = this._maxValue;
                GetCutOffValue();
            }
        }

        public Color BackgroundColor
        {
            get => this._backgroundBrush.Color;
            set => this._backgroundBrush = new SolidColorBrush(value);
        }

        private SolidColorBrush _backgroundBrush;

        private SolidColorBrush _borderBrush = new SolidColorBrush(Colors.Black);


        private int _fillWidth;
        private int _fillHeigth;
        private int _borderLeft;
        private int _borderTop;
        private int _borderRight;
        private int _borderBottom;
        private Color _borderColor;
        private int _minValue = 0;
        private int _maxValue = 100;
        private int _value = 50;

        private int _cutOffValue;

        public GradientProgressBar() : this(
            Colors.Yellow,
            new GradientPoint(3 / 15.0, Color.FromRgb(0x00, 0x64, 0x00), Color.FromRgb(0x00, 0xFF, 0x00)),
            new GradientPoint(10 / 15.0, Color.FromRgb(0x00, 0xFF, 0x00), Color.FromRgb(0x00, 0xFF, 0x00)),
            new GradientPoint(2 / 15.0, Color.FromRgb(0x00, 0xFF, 0x00), Color.FromRgb(0xFF, 0x00, 0x00)))
        { }
        public GradientProgressBar(Color backgroundColor, params GradientPoint[] points)
        {
            this._backgroundBrush = new SolidColorBrush(backgroundColor);
            SetGradientPoints(points);
        }

        public void SetGradientPoints(params GradientPoint[] points)
        {
            var gradientPoints = new ArrayList();
            if (points.Length > 0)
            {
                var weight = 0.0;
                foreach (var gradientPoint in points)
                {
                    weight += gradientPoint.Weight;
                }

                var scale = 1 / weight;

                for (var index = 0; index < points.Length; index++)
                {
                    points[index].Scale = scale;
                    gradientPoints.Add(points[index]);
                }
            }

            this._gradientPoints = gradientPoints;
            InvalidateMeasure();
        }

        private void GetCutOffValue()
        {
            int cutOffValue = 0;
            double value = this._value - this._minValue;
            double maxValue = this._maxValue - this._minValue;
            if (value != 0 && maxValue != 0) cutOffValue = (int)System.Math.Round(value / maxValue * this._fillWidth);

            if (this._cutOffValue == cutOffValue) return;
            lock (this._locker) this._cutOffValue = cutOffValue + this._borderRight;
            Invalidate();
        }

        public void GetBorderThickness(out int left, out int top, out int right, out int bottom)
        {
            left = this._borderLeft;
            top = this._borderTop;
            right = this._borderRight;
            bottom = this._borderBottom;
        }
        public void SetBorderThickness(int length) => SetBorderThickness(length, length, length, length);
        public void SetBorderThickness(int left, int top, int right, int bottom)
        {
            VerifyAccess();
            if (left < 0 || right < 0 || top < 0 || bottom < 0)
                throw new ArgumentException($"'{left},{top},{right},{bottom}' is not a valid value 'BorderThickness'");
            lock (this._locker)
            {
                this._borderLeft = left;
                this._borderTop = top;
                this._borderRight = right;
                this._borderBottom = bottom;
            }

            CreateFills();
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight)
        {
            base.MeasureOverride(availableWidth, availableHeight, out desiredWidth, out desiredHeight);
            CreateFills();
        }

        private void CreateFills()
        {
            var startX = this._borderLeft;

            lock (this._locker)
            {
                this._fillWidth = this.ActualWidth - this._borderLeft - this._borderRight;
                this._fillHeigth = this.ActualHeight - this._borderTop - this._borderBottom;
                if (this._fillWidth < 0) this._fillWidth = 0;
                if (this._fillHeigth < 0) this._fillHeigth = 0;

                var availableWidth = this._fillWidth;
                for (var i = 0; i < this._gradientPoints.Count; i++)
                {
                    var point = (GradientPoint)this._gradientPoints[i];
                    point.CreateBrush(startX, this._fillWidth, availableWidth, this._borderTop, this._fillHeigth);
                    availableWidth -= point.Width;
                    startX += point.Width;
                }
            }

            GetCutOffValue();
        }

        public override void OnRender(DrawingContext dc)
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            if (this._borderLeft > 0)
                dc.DrawRectangle(this._borderBrush, null, 0, 0, this._borderLeft, height);
            if (this._borderRight > 0)
                dc.DrawRectangle(this._borderBrush, null, width - this._borderRight, 0, this._borderRight, height);
            if (this._borderTop > 0)
                dc.DrawRectangle(this._borderBrush, null, 0, 0, width, this._borderTop);
            if (this._borderBottom > 0)
                dc.DrawRectangle(this._borderBrush, null, 0, height - this._borderBottom, width, this._borderBottom);

            lock (this._locker)
            {
                if (this._cutOffValue < 1)
                {
                    dc.DrawRectangle(this._backgroundBrush, null, this._borderLeft, this._borderTop, this._fillWidth, this._fillHeigth);
                }
                else
                {
                    for (var i = 0; i < this._gradientPoints.Count; i++)
                    {
                        var point = (GradientPoint)this._gradientPoints[i];
                        point.DrawRectangleInContext(dc);
                        if (this._cutOffValue <= point.EndX) break;
                    }

                    if (this._cutOffValue == this._fillWidth + this._borderRight) return;
                    dc.DrawRectangle(this._backgroundBrush, null,
                        this._cutOffValue, this._borderTop,
                        width - this._cutOffValue - this._borderRight, this._fillHeigth);
                }
            }
        }
    }
}
