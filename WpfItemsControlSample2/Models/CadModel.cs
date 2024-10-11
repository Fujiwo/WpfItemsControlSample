using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfItemsControlSample2.Models
{
    static class PointExtensions
    {
        public static bool IsEqual(this Point @this, Point point) => @this.X == point.X && @this.Y == point.Y;
        public static Point Plus(this Point @this, Point point) => new(x: @this.X + point.X, y: @this.Y + point.Y);
        public static Point Minus(this Point @this, Point point) => new(x: @this.X - point.X, y: @this.Y - point.Y);
        public static Point Multiply(this Point @this, double value) => new(x: @this.X * value, y: @this.Y * value);
        public static Point Divide(this Point @this, double value) => new(x: @this.X / value, y: @this.Y / value);
        public static double AbsoluteValue(this Point @this) => Math.Sqrt(@this.X.Square() + @this.Y.Square());
        public static double Distance(this Point @this, Point point) => @this.Minus(point).AbsoluteValue();
        public static Point Sum(this IEnumerable<Point> points) => points.Count() == 0 ? new Point() : points.Aggregate((sum, point) => sum.Plus(point));
        public static double Square(this double @this) => @this * @this;
        public static bool IsInRange(this Point @this, Size size) => @this.X.IsInRange(0.0, size.Width) && @this.Y.IsInRange(0.0, size.Height);
        public static bool IsInRange(this double @this, double minimum, double maximum) => @this >= minimum && @this <= maximum;
    }

    abstract class Body
    {
        public event Action? Update;

        public const double GravitationalConstant = 100.0;

        public double Mass { get; set; } = 1.0;
        public double Density { get; set; } = 0.001;
        public double Area => Mass / Density;
        public double Radius => Math.Sqrt(Area / (2.0 * Math.PI));

        Point position;
        public Point Position {
            get => position;
            set {
                if (!position.IsEqual(value)) {
                    position = value;
                    Update?.Invoke();
                }
            }
        }

        public Point Velocity { get; set; }
        public Point Acceleration => Force.Divide(Mass);
        public Point Force { get; set; }

        public Body Clone() => (Body)MemberwiseClone();

        public void Move(IEnumerable<Body> others)
        {
            Force = GetForce(others);
            Move();
        }

        Point GetForce(IEnumerable<Body> others) => others.Select(GetForce).Sum();

        Point GetForce(Body another)
        {
            var distance = Position.Distance(another.Position);
            return another.Position.Minus(Position).Divide(distance).Multiply(GravitationalConstant * Mass * another.Mass).Divide(distance.Square());
        }

        void Move()
        {
            Velocity = Velocity.Plus(Acceleration);
            Position = Position.Plus(Velocity    );
        }
    }

    class BodyFigure : Body, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Point TopLeft => Position.Minus(new Point(x: Radius, y: Radius));
        public Size Size => new(width: Radius * 2.0, height: Radius * 2.0);
         
        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.White;
        public double Thickness { get; set; } = 1.0;

        public BodyFigure()
            =>  Update += () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName: nameof(TopLeft)));
    }

    class BodyFigureSet : IEnumerable<BodyFigure>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        readonly ObservableCollection<BodyFigure> bodies = [];

        public int Count => bodies.Count;

        public BodyFigureSet() => bodies.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);

        public void Add(BodyFigure body) => bodies.Add(body);
        public void Remove(BodyFigure body) => bodies.Remove(body);
        public void RemoveAt(int index) => bodies.RemoveAt(index);

        public void Move()
        {
            var clones = bodies.Select(body => body.Clone()).ToList();

            bodies.Select((body, index) => (body, index))
                  .ToList()
                  .ForEach(tuple => tuple.body.Move(clones.Where((_, index) => index != tuple.index)));
        }

        public IEnumerator<BodyFigure> GetEnumerator() => bodies.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class CadModel : IEnumerable<BodyFigure>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        const int  maximumCount         = 100;
        const long intervalMilliseconds = 100L;

        readonly BodyFigureSet bodyFigureSet = [];
        readonly DispatcherTimer timer = new() { Interval = new TimeSpan(ticks: intervalMilliseconds * 10000L) };

        public Size MaximumSize { get; set; } = new Size(width: 800, height: 600);

        public CadModel()
        {
            bodyFigureSet.CollectionChanged += (_, e) => CollectionChanged?.Invoke(this, e);

            timer.Tick += (_, _) => AddRandomFigure();
            timer.Start();
        }

        public void Add(BodyFigure bodyFigure) => bodyFigureSet.Add(bodyFigure);

        public IEnumerator<BodyFigure> GetEnumerator() => bodyFigureSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void AddRandomFigure()
        {
            RemoveOutOfRangeBodyFigures();
            if (bodyFigureSet.Count < maximumCount)
                Add(CreateRandomBodyFigure(MaximumSize));
            bodyFigureSet.Move();

            void RemoveOutOfRangeBodyFigures()
                => bodyFigureSet.Where(bodyFigure => !bodyFigure.Position.IsInRange(MaximumSize))
                                .ToList()
                                .ForEach(bodyFigureSet.Remove);
        }

        static readonly Random random = new();

        static BodyFigure CreateRandomBodyFigure(Size maximumSize)
        {
            const double minimumMass     =  1.0;
            const double maximumMass     = 10.0;
            const double maximumVelocity =  1.0;

            return new BodyFigure() {
                Position    = GetRandomPosition(),
                Velocity    = GetRandomVelocity(),
                Mass        = GetRandomMass    (),
                FillColor   = GetRandomColor   (),
                StrokeColor = GetRandomColor   ()
            };

            Point GetRandomPosition() => GetRandomPoint(maximumSize);
            static Point GetRandomVelocity() => GetRandomPoint(new(maximumVelocity, maximumVelocity)).Minus(GetRandomPoint(new(maximumVelocity, maximumVelocity)));
            static double GetRandomMass() => random.NextDouble() * (maximumMass - minimumMass) + minimumMass;

            static Point GetRandomPoint(Size maximumSize)
                => new (x: random.NextDouble() * maximumSize.Width ,
                        y: random.NextDouble() * maximumSize.Height);

            static Color GetRandomColor()
                => Color.FromArgb((byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100), (byte)random.Next(0x00, 0x100));
        }
    }
}
