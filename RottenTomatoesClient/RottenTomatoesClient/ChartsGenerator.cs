using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;
using RottenTomatoesClient.Models;
using ScottPlot;
using ScottPlot.WPF;
namespace RottenTomatoesClient
{
    public static class ChartsGenerator
    {
        internal static ViewModel ViewModel
        {
            get => default;
            set
            {
            }
        }

        internal static void ShowLinePlot(string opt1, string opt2)
        {
            Plot myPlot = CreateLineChart(opt1, opt2);
            new ScottPlot.WpfPlotViewer(myPlot).ShowDialog();
        }
        internal static string SaveLinePLot(string opt1, string opt2)
        {
            Plot plot = CreateLineChart(opt1, opt2);
            string filepath = String.Format("{0}/Charts/{1}.png", Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, Guid.NewGuid());
            plot.SaveFig(filepath);
            return filepath;
        }

        private static Plot CreateLineChart(string opt1, string opt2)
        {
            List<double> dataX = new(), dataY = new();
            LoadDataRange(ref dataX, opt1);
            LoadDataRange(ref dataY, opt2);
            ClearDataset(ref dataX, ref dataY);

            var myPlot = new ScottPlot.Plot(600, 600);
            myPlot.AddScatter(dataX.ToArray(), dataY.ToArray(), lineWidth: 0);
            myPlot.XLabel(opt1);
            myPlot.YLabel(opt2);
            return myPlot;
        }

        internal static void ShowBarPlot(string option)
        {
            Plot plt = CreateBarChart(option);

            new ScottPlot.WpfPlotViewer(plt).ShowDialog();
        }
        internal static string SaveBarPLot(string option)
        {
            Plot plot = CreateBarChart(option);
            string filepath = String.Format("{0}/Charts/{1}.png", Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, Guid.NewGuid());
            plot.SaveFig(filepath);
            return filepath;
        }

        private static Plot CreateBarChart(string option)
        {
            var plt = new ScottPlot.Plot(600, 600);
            List<double> values = new();
            List<string> labels = new();
            LoadDataRange(ref values, ref labels, option);
            plt.AddBar(values.ToArray());
            plt.XTicks(labels.ToArray());
            plt.SetAxisLimits(yMin: 0);
            plt.XAxis.Label(option);
            plt.XAxis.TickLabelStyle(rotation: 90);
            return plt;
        }

        private static void ClearDataset(ref List<double> dataX, ref List<double> dataY)
        {
            for (int i = 0; i < dataX.Count; i++)
            {
                if (dataX[i] == 0 || dataY[i] == 0)
                {
                    dataX.RemoveAt(i);
                    dataY.RemoveAt(i);
                }
                    
            }
        }
        private static void LoadDataRange(ref List<double> values, ref List<string> labels, string option)
        {
            using (var db = new _8i11CherezovRtContext())
            {
                List<Tuple<string, int>> data = new();
                if (option == "Родина режиссёра")
                {
                    data = db.Directors
                        .GroupBy(x => x.Birthplace)
                        .Select(x => new Tuple<string, int>(x.Key, x.Count()))
                        .ToList();

                }
                if (option == "Жанр фильма")
                {
                    data = db.Genres
                        .Include(x => x.Films)
                        .Select(x => new Tuple<string, int>(x.Genre1, x.Films.Count()))
                        .ToList();
                }

                foreach (var elem in data)
                {
                    values.Add(elem.Item2);
                    labels.Add(elem.Item1);
                }
            }
        }
        private static void LoadDataRange(ref List<double> data, string opt)
        {
            using (var db = new _8i11CherezovRtContext())
            {
                switch (opt)
                {
                    case "Оценки критиков":
                        data = db.Films
                            .Select(x => Convert.ToDouble(x.CriticsReviewRating))
                            .ToList();
                        break;
                    case "Оценки пользователей":
                        data = db.Films
                            .Select(x => Convert.ToDouble(x.AudienceReviewRating))
                            .ToList();
                        break;
                    case "Количество оценок пользователей":
                        data = db.Films
                            .Select(x => Convert.ToDouble(x.AudienceReviewCount))
                            .ToList();
                        break;
                    case "Количество оценок критиков":
                        data = db.Films
                            .Select(x => Convert.ToDouble(x.CriticsReviewCount))
                            .ToList();
                        break;
                    case "Кассовый сбор":
                        data = db.Films
                            .Select(x => Convert.ToDouble(x.BoxOffice))
                            .ToList();
                        break;
                }
            }
        }
    }
}
