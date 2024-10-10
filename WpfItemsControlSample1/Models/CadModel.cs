using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfItemsControlSample1.Models
{
    class CadModel : IEnumerable<Figure>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        const int  maximumCount         = 100 ;
        const long intervalMilliseconds =  10L;

        ObservableCollection<Figure> figures = new();
        DispatcherTimer timer = new() { Interval = new TimeSpan(ticks: intervalMilliseconds * 10000L) };

        public Size MaximumSize { get; set; } = new Size(width: 800, height: 600);

        public CadModel()
        {
            figures.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);

            timer.Tick += (_, _) => AddRandomFigure();
            timer.Start();
        }

        public void Add(Figure figure) => figures.Add(figure);

        public IEnumerator<Figure> GetEnumerator() => figures.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void AddRandomFigure()
        {
            if (figures.Count >= maximumCount)
                figures.RemoveAt(0);
            Add(CreateRandomFigure(MaximumSize));
        }

        static Random random = new();

        static Figure CreateRandomFigure(Size maximumSize)
        {
            const int    maximumPolygonPointCount = 7;
            const double maximumThickness         = 3.0;

            Figure figure = random.Next(Figure.KindCount) switch {
                0 => new LineFigure      { Start    = GetRandomPoint(maximumSize), End = GetRandomPoint(maximumSize) },
                1 => new RectangleFigure { Position = GetRandomPoint(maximumSize), End = GetRandomPoint(maximumSize) },
                2 => new EllipseFigure   { Position = GetRandomPoint(maximumSize), End = GetRandomPoint(maximumSize) },
                3 => new PolygonFigure   { Points   = GetRandomPointCollection(maximumSize)                          },
                _ => throw new InvalidOperationException()
            };
            (figure.FillColor, figure.StrokeColor, figure.Thickness)
                = (GetRandomColor(), GetRandomColor(), GetRandomThickness(maximumThickness));
            return figure;

            static PointCollection GetRandomPointCollection(Size maximumSize)
                => new PointCollection(GetRandomPoints(maximumPolygonPointCount, maximumSize));

            static IEnumerable<Point> GetRandomPoints(int maximumPointCount, Size maximumSize)
                => Enumerable.Range(0, random.Next(3, maximumPointCount + 1)).Select(_ => GetRandomPoint(maximumSize));

            static Point GetRandomPoint(Size maximumSize)
                => new Point(x: random.NextDouble() * maximumSize.Width ,
                             y: random.NextDouble() * maximumSize.Height);

            static Color GetRandomColor()
                => Color.FromArgb((byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100));

            static double GetRandomThickness(double maximumThickness)
                => random.NextDouble() * maximumThickness;
        }
    }

    static class PointExtensions
    {
        public static Point Add(this Point @this, Size size) => new Point(x: @this.X + size.Width, y: @this.Y + size.Height);
        public static Size Subtract(this Point @this, Point point) => new Size(width: @this.X - point.X, height: @this.Y - point.Y);

    }

    abstract class Figure
    {
        public const int KindCount = 4;

        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor   { get; set; } = Colors.White;
        public double Thickness  { get; set; } = 1.0;
    }

    class LineFigure : Figure
    {
        public Point Start { get; set; }
        public Point End   { get; set; }
    }

    class RectangleFigure : Figure
    {
        public Point Position { get; set; }
        public Size  Size     { get; set; }

        public Point End {
            get => new Point(Position.X + Size.Width, Position.Y + Size.Height);
            set {
                var (minimum, maximum) = (new Point(Math.Min(Position.X, value.X), Math.Min(Position.Y, value.Y)),
                                          new Point(Math.Max(Position.X, value.X), Math.Max(Position.Y, value.Y)));
                Position               = minimum;
                Size                   = maximum.Subtract(minimum);
            }
        }
    }

    class EllipseFigure : RectangleFigure
    {}

    class PolygonFigure : Figure
    {
        public PointCollection Points { get; set; } = new();
    }
}
