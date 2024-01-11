using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RottenTomatoesClient.Models;
using System.Windows.Controls;

namespace RottenTomatoesClient
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #region ViewBindings
        private ObservableCollection<FilmVM> filmsList;
        public ObservableCollection<FilmVM> FilmsList
        {
            get
            {
                return this.filmsList;
            }
            set
            {
                this.filmsList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> LineChartOptions { get; set; }
        public ObservableCollection<string> BarChartOptions { get; set; }
        
        private int dataGridIndex;
        public int DataGridIndex
        {
            get
            {
                return this.dataGridIndex;
            }
            set
            {
                this.dataGridIndex = value;
                OnPropertyChanged();
            }
        }
        


        private string comboX;
        public string ComboX
        {
            get
            {
                return this.comboX;
            }
            set
            {
                this.comboX = value;
                OnPropertyChanged();
            }
        }

        private string comboY;
        public string ComboY
        {
            get
            {
                return this.comboY;
            }
            set
            {
                this.comboY = value;
                OnPropertyChanged();
            }
        }

        private string comboBar;
        public string ComboBar
        {
            get
            {
                return this.comboBar;
            }
            set
            {
                this.comboBar = value;
                OnPropertyChanged();
            }
        }

        private FilmVM dataGridItem;
        public FilmVM DataGridItem
        {
            get
            {
                return this.dataGridItem;
            }
            set
            {
                this.dataGridItem = value;
                OnPropertyChanged();
            }
        }

        #endregion

        
        public ViewModel() 
        {
            filmsList = new ();
            dataGridIndex = -1;
            LineChartOptions = new ObservableCollection<string>(new string[] 
            { 
                "Оценки критиков", 
                "Оценки пользователей", 
                "Количество оценок пользователей", 
                "Количество оценок критиков",
                "Кассовый сбор"
            });
            BarChartOptions = new ObservableCollection<string>(new string[]
            {
                "Родина режиссёра",
                "Жанр фильма"
            });
        }


        private Command? loadALLCommand;
        public Command LoadALLCommand
        {
            get => loadALLCommand ??= new Command(obj =>
            {
                FilmsList.Clear();
                try
                {
                    using (var db = new _8i11CherezovRtContext())
                    {
                        
                        FilmsList = new ObservableCollection<FilmVM>(db.Films
                            .Include(x => x.Director)
                            .Include(x => x.Genres)
                            .OrderByDescending(x => x.AudienceReviewRating)
                            .ThenByDescending(x => x.CriticsReviewRating)
                            .Select(x => new FilmVM
                            {
                                Title = x.Title,
                                DirectorName = x.Director.Name,
                                CriticsReviewRating = x.CriticsReviewRating,
                                CriticsReviewCount = x.CriticsReviewCount,
                                AudienceReviewRating = x.AudienceReviewRating,
                                AudienceReviewCount = x.AudienceReviewCount,
                                BoxOffice = x.BoxOffice,
                                Genres = GetGenresString(x.Genres)
                            })
                            .ToList());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
        }

        private static string GetGenresString(ICollection<Genre> genres)
        {
            List<string> res = new();
            foreach (var genre in genres)
            {
                res.Add(genre.Genre1);
            }
            return String.Join(", ", res);
        }

        private Command? deleteCommand;
        public Command DeleteCommand
        {
            get => deleteCommand ??= new Command(obj =>
            {
                try
                {
                    using (var db = new _8i11CherezovRtContext())
                    {
                        Film? filmToDelete = db.Films
                        .Where(x => x.Title == DataGridItem.Title
                        && x.AudienceReviewRating == DataGridItem.AudienceReviewRating
                        && x.CriticsReviewRating == DataGridItem.CriticsReviewRating)
                        .FirstOrDefault();
                        if (filmToDelete != null)
                        {
                            db.Films.Remove(filmToDelete);
                            db.SaveChanges();
                            FilmsList.Remove(DataGridItem);
                        }
                        else { throw new Exception("Ошибка при удаленни записи: запись не найдена в БД!"); }
                    }
                    DataGridIndex = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageBox.Show(ex.InnerException.Message);
                }
            });
        }

        private Command? updateCommand;
        public Command UpdateCommand
        {
            get => updateCommand ??= new Command(obj =>
            {
                try
                {
                    using (var db = new _8i11CherezovRtContext())
                    {
                        Film? filmToUpdate = db.Films
                        .OrderByDescending(x => x.AudienceReviewRating)
                        .ThenByDescending(x => x.CriticsReviewRating)
                        .Include(x => x.Director)
                        .ToList()
                        .ElementAt(DataGridIndex);
                        db.Attach(filmToUpdate);
                        if (filmToUpdate != null)
                        {
                            filmToUpdate.Title = DataGridItem.Title;
                            filmToUpdate.Director.Name = DataGridItem.DirectorName;
                            filmToUpdate.CriticsReviewCount = DataGridItem.CriticsReviewCount;
                            filmToUpdate.CriticsReviewRating = DataGridItem.CriticsReviewRating;
                            filmToUpdate.AudienceReviewCount = DataGridItem.AudienceReviewCount;
                            filmToUpdate.AudienceReviewRating = DataGridItem.AudienceReviewRating;
                            filmToUpdate.BoxOffice = DataGridItem.BoxOffice;
                            db.SaveChanges();
                            MessageBox.Show("Изменения сохранены в базе данных.");
                        }
                        else { throw new Exception("Ошибка при обновлении записи: запись не найдена в БД!"); }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
        }


        private Command? buildLineChart;
        public Command BuildLineChart
        {
            get => buildLineChart ??= new Command(obj =>
            {
                try
                {
                    ChartsGenerator.ShowLinePlot(comboX, comboY);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        private Command? buildBarChart;
        public Command BuildBarChart
        {
            get => buildBarChart ??= new Command(obj =>
            {
                try
                {
                    ChartsGenerator.ShowBarPlot(comboBar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private Command? generateReport;
        public Command GenerateReport
        {
            get => generateReport ??= new Command(obj =>
            {
                try
                {
                    ReportGenerator.GenerateReport(ComboX, ComboY, ComboBar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    
                }
            });
        }

        private Command? updateParsing;
        public Command UpdateParsing
        {
            get => updateParsing ??= new Command(obj =>
            {
                try
                {
                    Process.Start("C:\\Учёба\\Архитектура ИС\\Курсовая\\RottenTomatoesParser\\RottenTomatoesParser\\bin\\Debug\\net6.0\\RottenTomatoesParser.exe");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    
                }
            });
        }
    }
}
