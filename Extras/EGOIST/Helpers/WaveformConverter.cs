using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EGOIST.Helpers;

public class WaveformConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ObservableCollection<double> waveform || waveform.Count == 0)
            return Geometry.Empty;

        const int samplesPerPoint = 100;
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, 50) };

        for (var i = 0; i < waveform.Count; i += samplesPerPoint)
        {
            double sum = 0;
            var samplesTaken = 0;

            for (var j = i; j < i + samplesPerPoint && j < waveform.Count; j++)
            {
                sum += waveform[j];
                samplesTaken++;
            }

            var average = sum / samplesTaken;
            figure.Segments.Add(new LineSegment(new Point(i / samplesPerPoint, 50 - (average * 50)), true));
        }

        geometry.Figures.Add(figure);
        return geometry;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
