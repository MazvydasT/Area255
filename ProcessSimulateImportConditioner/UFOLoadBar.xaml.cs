using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProcessSimulateImportConditioner
{
    /// <summary>
    /// Interaction logic for UFOLoadBar.xaml
    /// </summary>
    public partial class UFOLoadBar : UserControl
    {
        public UFOLoadBar()
        {
            InitializeComponent();

            this.IsVisibleChanged += UFOLoadBar_IsVisibleChanged;
        }

        void UFOLoadBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            else
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private double? previousProgressValue = null;
        private double animationStartTimeMS = 0;
        // private double previousRenderingTimeS = -1;
        private PathGeometry pathGeometry = null;
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var renderingEventArgs = (RenderingEventArgs)e;
            var service = ApplicationData.Service;

            var progressAnimationDurationMS = service.ProgressAnimationDuration.TimeSpan.TotalMilliseconds;
            var accelerationRatio = 0.2;
            var decelerationRatio = 0.7;

            var renderingTimeMS = renderingEventArgs.RenderingTime.TotalMilliseconds;

            /*var renderingTimeS = renderingEventArgs.RenderingTime.TotalSeconds;

            if (previousRenderingTimeS == -1)
            {
                previousRenderingTimeS = renderingTimeS;
                return;
            }

            var timeElapsedBetweenRendersS = renderingTimeS - previousRenderingTimeS;

            var acceleration = 1; // units/s

            barContainer.Width += acceleration * timeElapsedBetweenRendersS;*/

            if (previousProgressValue != service.ProgressValue)
            {
                previousProgressValue = service.ProgressValue;
                animationStartTimeMS = renderingTimeMS;

                var initialBarContainerPosition = barContainer.Width / ActualWidth;
                var targetBarContainerPosition = service.ProgressValue / service.MaxValue;
                var barContainerPathLength = targetBarContainerPosition - initialBarContainerPosition;

                var accelerationPathLength = barContainerPathLength * accelerationRatio;
                var decelerationPathLength = barContainerPathLength * decelerationRatio;
                var constantPathLength = barContainerPathLength - accelerationPathLength - decelerationPathLength;

                var startingPoint = new Point(initialBarContainerPosition, 0);
                var accelerationEndPoint = Point.Add(startingPoint, new Vector(accelerationPathLength, accelerationPathLength));
                var accelerationControlPoint = new Point(startingPoint.X, accelerationEndPoint.Y);

                var decelerationStartPoint = Point.Add(accelerationEndPoint, new Vector(constantPathLength, 0));
                var decelerationEndPoint = Point.Add(decelerationStartPoint, new Vector(decelerationPathLength, decelerationPathLength));
                var decelerationControlPoint = new Point(decelerationEndPoint.X, decelerationStartPoint.Y);

                pathGeometry = new PathGeometry(new PathFigure[]
                {
                    new PathFigure(startingPoint, new PathSegment[]
                    {
                        new QuadraticBezierSegment(accelerationControlPoint, accelerationEndPoint, true),
                        new LineSegment(decelerationStartPoint, true),
                        new QuadraticBezierSegment(decelerationControlPoint, decelerationEndPoint, true)
                    }, false)
                });
            }

            var animationTimeRatio = Math.Min((renderingTimeMS - animationStartTimeMS) / progressAnimationDurationMS, 1);

            Point point = new Point();

            if (pathGeometry.Bounds.Width > 0)
            {
                pathGeometry.GetPointAtFractionLength(animationTimeRatio, out point, out _);
            }

            barContainer.Width = ActualWidth * point.X;
        }

        private void LoaderRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            loaderRoot.MouseMove += LoaderRoot_MouseMove;
            ApplicationData.Service.MouseIsInsideLoader = true;
        }

        private void LoaderRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            ApplicationData.Service.MouseIsInsideLoader = false;
            loaderRoot.MouseMove -= LoaderRoot_MouseMove;
        }

        void LoaderRoot_MouseMove(object sender, MouseEventArgs e)
        {
            ApplicationData.Service.MousePositionInsideLoader = Mouse.GetPosition((IInputElement)sender);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.Service.ProgressValue > 1)
                ApplicationData.Service.ProgressValue--;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.Service.ProgressValue < ApplicationData.Service.MaxValue)
                ApplicationData.Service.ProgressValue++;
        }
    }
}
