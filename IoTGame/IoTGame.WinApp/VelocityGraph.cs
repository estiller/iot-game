using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace IoTGame.WinApp
{
    public sealed class VelocityGraph : Control
    {
        private Line _vector;

        public VelocityGraph()
        {
            DefaultStyleKey = typeof(VelocityGraph);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var axisX = new Line
            {
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeThickness = 3,
                X1 = 0,
                X2 = Width,
                Y1 = Height / 2.0,
                Y2 = Height / 2.0
            };

            var axisY = new Line
            {
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeThickness = 3,
                X1 = Width / 2.0,
                X2 = Width / 2.0,
                Y1 = 0,
                Y2 = Height
            };

            var deadZone = new Ellipse
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2,
                Width = Width * 0.2,
                Height = Height * 0.2
                
            };

            _vector = new Line
            {
                Stroke = Background,
                StrokeThickness = 5,
                X1 = Width / 2.0,
                Y1 = Height / 2.0,
                X2 = Width / 2.0,
                Y2 = Height / 2.0
            };

            Canvas canvas = (Canvas)GetTemplateChild("Canvas");
            canvas.Children.Add(axisX);
            canvas.Children.Add(axisY);
            canvas.Children.Add(_vector);

            canvas.Children.Add(deadZone);
            Canvas.SetLeft(deadZone, Width / 2.0 - deadZone.Width / 2.0);
            Canvas.SetTop(deadZone, Height / 2.0 - deadZone.Height / 2.0);
        }

        public double VectorX
        {
            get => (double)GetValue(VectorXProperty);
            set => SetValue(VectorXProperty, value);
        }

        // Using a DependencyProperty as the backing store for VectorX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VectorXProperty =
            DependencyProperty.Register("VectorX", typeof(double), typeof(VelocityGraph), new PropertyMetadata(0.0, VectorXChangedCallback));

        private static void VectorXChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (VelocityGraph) d;
            graph.UpdateArrowX((double)e.NewValue);

        }

        private void UpdateArrowX(double newValue)
        {
            if (_vector == null) return;
            _vector.X2 = newValue * Width / 2.0 + Width / 2.0;
        }

        public double VectorY
        {
            get => (double)GetValue(VectorYProperty);
            set => SetValue(VectorYProperty, value);
        }

        // Using a DependencyProperty as the backing store for VectorY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VectorYProperty =
            DependencyProperty.Register("VectorY", typeof(double), typeof(VelocityGraph), new PropertyMetadata(0.0, VectorYChangedCallback));

        private static void VectorYChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (VelocityGraph) d;
            graph.UpdateArrowY((double)e.NewValue);
        }

        private void UpdateArrowY(double newValue)
        {
            if (_vector == null) return;
            _vector.Y2 = -newValue * Height / 2.0 + Height / 2.0;
        }
        public StatisticsValue[] StatisticsGrid
        {
            get => (StatisticsValue[])GetValue(StatisticsGridProperty);
            set => SetValue(StatisticsGridProperty, value);
        }

        // Using a DependencyProperty as the backing store for StatisticsGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatisticsGridProperty =
            DependencyProperty.Register("StatisticsGrid", typeof(StatisticsValue[]), typeof(VelocityGraph), new PropertyMetadata(null));
    }

    public class StatisticsValue : INotifyPropertyChanged
    {
        private double _value;
        private int _count;

        public double Value
        {
            get => _value;
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public Brush Color { get; set; }

        public int Count
        {
            get => _count;
            set
            {
                if (value == _count) return;
                _count = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}