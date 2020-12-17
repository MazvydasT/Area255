using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProcessSimulateImportConditioner
{
    public class Asteroid
    {
        public event PropertyChangedEventHandler PropertyChanged;


        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly double rotationTo = Utils.RandomBool ? 360d : -360d;
        public double RotationTo { get { return rotationTo; } }

        private Duration oneRevolutionDurantion = new Duration(new TimeSpan(0, 0, 0, 0, Utils.RandomInt(2000, 10000)));
        public Duration OneRevolutionDurantion { get { return oneRevolutionDurantion; } }

        private Duration trajectoryDurantion = new Duration(new TimeSpan(0, 0, 0, 0, Utils.RandomInt(5000, 30000)));
        public Duration TrajectoryDurantion { get { return trajectoryDurantion; } }

        private Point trajectoryStartPoint = new Point(Utils.RandomDouble, Utils.RandomDouble);
        public Point TrajectoryStartPoint
        {
            get
            {
                if (Math.Min(trajectoryStartPoint.X, trajectoryStartPoint.Y) > 0)
                {
                    if (trajectoryStartPoint.X < trajectoryStartPoint.Y) trajectoryStartPoint.X = 0;
                    else trajectoryStartPoint.Y = 0;
                }

                return trajectoryStartPoint;
            }
        }

        private Point trajectoryEndPoint = new Point(Utils.RandomDouble, Utils.RandomDouble);
        public Point TrajectoryEndPoint
        {
            get
            {
                if (Math.Max(trajectoryEndPoint.X, trajectoryEndPoint.Y) < 1)
                {
                    if (trajectoryEndPoint.X > trajectoryEndPoint.Y) trajectoryEndPoint.X = 1;
                    else trajectoryEndPoint.Y = 1;
                }

                return trajectoryEndPoint;
            }
        }

        private readonly double trajectoryPassThroughPointRatio = Utils.RandomDouble; //0.5;
        public double TrajectoryPassThroughPointRatio { get { return trajectoryPassThroughPointRatio; } }

        private Point trajectoryPassThroughPoint = /*new Point(0.5, 0.5);*/ new Point(Utils.RandomDouble, Utils.RandomDouble);
        public Point TrajectoryPassThroughPoint { get { return trajectoryPassThroughPoint; } }

        private Nullable<Point> trajectoryControlPoint = null;
        public Nullable<Point> TrajectoryControlPoint
        {
            get
            {
                if (trajectoryControlPoint == null)
                {
                    var p0 = new Vector(TrajectoryStartPoint.X, TrajectoryStartPoint.Y);
                    var p2 = new Vector(TrajectoryEndPoint.X, TrajectoryEndPoint.Y);

                    var pc = new Vector(TrajectoryPassThroughPoint.X, TrajectoryPassThroughPoint.Y);

                    var t = trajectoryPassThroughPointRatio;

                    var p1 = (pc - p0 * Math.Pow(t, 2) - p2 * Math.Pow(1 - t, 2)) / t;

                    trajectoryControlPoint = new Point(p1.X, p1.Y);
                }

                return trajectoryControlPoint.Value;
            }
        }


        public Asteroid()
        {

        }
    }
}
