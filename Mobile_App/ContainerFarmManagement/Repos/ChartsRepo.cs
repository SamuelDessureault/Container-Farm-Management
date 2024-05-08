using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Repos
{
    public static class ChartsRepo
    {
        private static ObservableCollection<float> historyList = new ObservableCollection<float>();

        /// <summary>
        /// Gets the series list of a reading value history for a LiveChart2 chart.
        /// </summary>
        /// <param name="history">The history of reading values.</param>
        /// <returns>A series list of LiveCharts2</returns>
        public static List<ISeries> GetSeries(IEnumerable<float> history)
        {
            historyList.Clear();
            foreach (float value in history)
                historyList.Add(value);
            return new List<ISeries>()
            {
                new LineSeries<float>()
                {
                    Values = historyList,
                    Fill = new SolidColorPaint(SKColors.Azure),
                    LineSmoothness = 0
                }
            };
        }

        /// <summary>
        /// Gets the x-axis for the LiveCharts2 chart.
        /// </summary>
        /// <param name="name">The name of the x-axis</param>
        /// <returns>The x-axis for the LiveCharts2 chart.</returns>
        public static Axis[] GetYAxis(string name)
        {
            return new Axis[]
            {
                new Axis
                {
                    TextSize = 32
                }
            };
        }

        /// <summary>
        /// Gets the y-axis for the LiveCharts2 chart.
        /// </summary>
        /// <returns>The y-axis for the LiveCharts2 chart.</returns>
        public static Axis[] GetXAxis()
        {
            return new Axis[]
            {
                new Axis
                {
                    Name = "Time (By telemetry interval)",
                    TextSize = 25
                }
            };
        }
    }
}