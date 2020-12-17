using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProcessSimulateImportConditioner
{
    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class OddEvenBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value % 2 == 0 ? null : new SolidColorBrush(Utils.GreyColour);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadingBarColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value == 0 ? Utils.LimeGreen : Utils.OrangeRed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SysRootToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var sysRootPath = (string)value;
            var sysRootPathIsEmpty = sysRootPath == String.Empty;

            var invert = parameter != null && (bool)parameter;

            if (invert)
                sysRootPathIsEmpty = !sysRootPathIsEmpty;

            return sysRootPathIsEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var moreThanZero = System.Convert.ToDouble(value) > 0;
            var invert = parameter != null && (bool)parameter;

            if (invert) moreThanZero = !moreThanZero;

            return moreThanZero ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = (bool)value;
            var invert = parameter != null && (bool)parameter;

            if (invert) val = !val;

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OutputDirectoryValidityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Brushes.White : new SolidColorBrush(Utils.WarningColour);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OutputDirectoryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var autoOutputDirectory = (bool)values[0];
            var autoOutputBaseDirectory = (string)values[1];
            var index = (int)values[2];
            var outputDirectory = (string)values[3];
            var usePartName = (bool)values[4];
            var partName = (string)values[5];
            var input = (Input)values[6];

            string path;

            if (autoOutputDirectory)
            {
                var duplicatePathIndex = usePartName ? 0 : index;

                path = Path.Combine(autoOutputBaseDirectory, usePartName ? partName : index.ToString());

                while (ApplicationData.Service.InputsContainMoreThanOneOutputPath(path, input) || !Utils.DirectoryIsEmpty(path))
                {
                    var newDuplicatePathIndex = (++duplicatePathIndex).ToString();

                    path = Path.Combine(autoOutputBaseDirectory, usePartName ? partName + " " + newDuplicatePathIndex : newDuplicatePathIndex);
                }
            }

            else
                path = outputDirectory;

            return Utils.PathIsValid(path) || !autoOutputDirectory ? path : "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class BaseOutputDirectoryValidityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)values[0] == 0 || (bool)values[1] ? Brushes.White : new SolidColorBrush(Utils.WarningColour);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class GoButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var inputsCount = (int)values[0];
            var invalidOutputDirectoryCount = (int)values[1];
            var baseOutputDirectoryIsValid = (bool)values[2];
            var autoOutputCount = (int)values[3];

            return inputsCount > 0 && invalidOutputDirectoryCount == 0 && (baseOutputDirectoryIsValid || autoOutputCount == 0) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    /*public class UFOProgressBarConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var progressBarWidth = ((double)values[0]) * (((double)values[2]) / ((double)values[1]));

            progressBarWidth = Double.IsNaN(progressBarWidth) || Double.IsInfinity(progressBarWidth) ? 0 : progressBarWidth;

            return progressBarWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }*/

    public class ProgressBarYTransformConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mouseIsInsideLoader = (bool)values[0];

            if (!mouseIsInsideLoader) return 0d;

            var yMousePositionInsideLoader = (double)values[1];
            var loaderRootHalfHeight = (double)values[2] / 2d;

            return yMousePositionInsideLoader - loaderRootHalfHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class PointPercentageToPixelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var percentagePoint = (Point)values[0];
            var referenceWidth = (double)values[1];
            var referenceHeight = (double)values[2];

            var pixelPoint = new Point(referenceWidth * percentagePoint.X, referenceHeight * percentagePoint.Y);

            return pixelPoint;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}