using AngleSharp.Dom;
using RottenTomatoesParser.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Reflection;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RottenTomatoesParser
{
    public class Parser
    {
        static void Main()
        {
            // Ссылка на список фильмов
            string url = @"https://www.rottentomatoes.com/browse/movies_at_home/sort:popular?page=5";
            
            // Настройки драйвера браузера. Загрузка страницы - жадная, чтобы не ожидать полной заргузки страницы.
            var chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var customProfile = new ChromeOptions();
            customProfile.LeaveBrowserRunning = false;
            customProfile.PageLoadStrategy = PageLoadStrategy.Eager;

            using IWebDriver driver = new ChromeDriver(chromeDriverService, customProfile);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.Navigate().GoToUrl(url);

            ReadOnlyCollection<IWebElement> list = driver.FindElements(By.CssSelector("a[data-qa='discovery-media-list-item-caption']"));
            List<Tuple<string, string>> titlesAndRefs= new(); 
            foreach ( var element in list)
            {
                string reflink = element.GetAttribute("href");
                string title = element.FindElement(By.TagName("span")).Text;
                titlesAndRefs.Add(Tuple.Create(title, reflink));
            }
            

            using (var db = new _8i11CherezovRtContext()) 
            {
                List<Genre> genres = db.Genres.ToList();
                List<string> filmtitles = db.Films.Select(x => x.Title).ToList();
                List<Director> directors = db.Directors.ToList();
                List<Film> newFilms = new List<Film>();                
                

                foreach (Tuple<string, string> titleAndRef in titlesAndRefs)
                {
                    if (!filmtitles.Contains(titleAndRef.Item1))
                    {
                        driver.Navigate().GoToUrl(titleAndRef.Item2);
                        driver.FindElement(By.TagName("a"));
                        string rawHtml = driver.PageSource;
                        HtmlParser htmlParser = new HtmlParser();
                        IHtmlDocument document = htmlParser.ParseDocument(rawHtml);

                        IElement? ratings = document.QuerySelector("#topSection > div.thumbnail-scoreboard-wrap > div + score-board-deprecated");
                        string? critRating = ratings!.GetAttribute("tomatometerscore");
                        string? audRating = ratings!.GetAttribute("audiencescore");
                        IElement? critQuantity = document.QuerySelector("#scoreboard > a:nth-child(3)");
                       
                        IElement? audQuantity = document.QuerySelector("#scoreboard > a:nth-child(4)");
                        IElement? boxOffice = null;
                        IElement? director = null;
                        IElement? genre = null;
                        
                        IElement? info = document.QuerySelector("#info");
                        for (int i = 1; i < 13; i++)
                        {
                            IElement? head = document.QuerySelector(string.Format("#info > li:nth-child({0}) > p > b", i));
                            if (head == null)
                            {
                                break;
                            }
                            else
                            {
                                string headstr = head!.TextContent;
                                if (headstr.Contains("Box Office"))
                                {
                                    boxOffice = document.QuerySelector(string.Format("#info > li:nth-child({0}) > p > span", i));
                                }
                                else if (headstr.Contains("Genre"))
                                {
                                    genre = document.QuerySelector(string.Format("#info > li:nth-child({0}) > p > span", i));
                                }
                                else if (headstr.Contains("Director"))
                                {
                                    director = document.QuerySelector(string.Format("#info > li:nth-child({0}) > p > span > a", i));
                                }
                            }    
                        }



                        int criticsReviewRating = 0;
                        try
                        {
                            if (critRating != null)
                                criticsReviewRating = Convert.ToInt32(Regex.Replace(critRating!, @"\D", ""));
                        }
                        catch { criticsReviewRating = 0; }
                        int criticsReviewCount = 0;
                        try
                        {
                            if (critQuantity != null)
                                criticsReviewCount = Convert.ToInt32(Regex.Replace(critQuantity!.TextContent, @"\D", ""));
                        }
                        catch { criticsReviewCount = 0; }
                        int audienceReviewRating = 0;
                        try
                        {
                            if (audRating != null)
                                audienceReviewRating = Convert.ToInt32(Regex.Replace(audRating!, @"\D", ""));
                        }
                        catch {  audienceReviewRating = 0; }
                        int audienceReviewCount = 0;
                        try
                        {
                            if (audQuantity != null)
                                audienceReviewCount = Convert.ToInt32(Regex.Replace(audQuantity!.TextContent, @"\D", ""));
                        }
                        catch { audienceReviewCount = 0; }
                        double? _boxOffice = 0;
                        try
                        {
                            if (boxOffice != null || boxOffice.TextContent != "")
                                _boxOffice = double.Parse(boxOffice!.TextContent.Substring(1, boxOffice!.TextContent.Length - 2), CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            _boxOffice = 0;
                        }
                        Guid directorId = GetDirectorID(director, db, driver, ref directors);
                        Film newFilm = new Film()
                        {
                            FilmId = Guid.NewGuid(),
                            Title = titleAndRef.Item1,
                            CriticsReviewRating = criticsReviewRating,
                            CriticsReviewCount = criticsReviewCount,
                            AudienceReviewRating = audienceReviewRating,
                            AudienceReviewCount = audienceReviewCount,
                            BoxOffice = _boxOffice,
                            DirectorId = directorId
                        };
                        

                        
                        string[] genreNames = genre!.TextContent.Split(',');
                        foreach (string genreName in genreNames)
                        {
                            string tempName = Regex.Replace(genreName, @"\t|\n|\r", "");
                            tempName = tempName.Trim();
                            Genre? tempGenre = genres
                                .Where(x => x.Genre1 == tempName)
                                .FirstOrDefault();
                            if (tempGenre is null)
                            {
                                tempGenre = new Genre() { 
                                    GenreId = genres.Count() + 1,
                                    Genre1 = tempName
                                };
                                db.Genres.Add(tempGenre);
                                genres.Add(tempGenre);
                            }
                            newFilm.Genres.Add(tempGenre);
                        }
                        db.Films.Add(newFilm);

                    }

                }
                
                db.SaveChanges();
                driver.Dispose();
            } 
            
        }

        private static Guid GetDirectorID(IElement? director, _8i11CherezovRtContext db, IWebDriver driver, ref List<Director> directors)
        {
            if (director is not null)
            {

                Director? dir = directors.Where(x => x.Name == director!.TextContent).FirstOrDefault();
                if (dir is null)
                {
                    string baseurl = @"https://www.rottentomatoes.com";
                    string link = baseurl + director!.GetAttribute("href");
                    driver.Navigate().GoToUrl(link);
                    driver.FindElement(By.TagName("h1"));
                    string rawHtml = driver.PageSource;
                    HtmlParser htmlParser = new HtmlParser();
                    IHtmlDocument document = htmlParser.ParseDocument(rawHtml);
                    IElement? birthplace = document.QuerySelector("#celebrity > article > section:nth-child(1) > div > div > div > p:nth-child(4)");
                    IElement? birthday = document.QuerySelector("#celebrity > article > section:nth-child(1) > div > div > div > p[data-qa=\"celebrity-bio-bday\"]");

                    Director newDir = new Director()
                    {
                        DirectorId = Guid.NewGuid(),
                        Name = director!.TextContent,
                        Birthplace = GetBirthplace(birthplace),
                        Birthday = GetBirthdate(birthday)
                    };
                    directors.Add(newDir);
                    db.Directors.Add(newDir);
                    return newDir.DirectorId;
                }
                else return dir!.DirectorId;
            }
            else return Guid.Empty;
        }

        private static DateTime GetBirthdate(IElement? birthday)
        {
            if (birthday is not null)
            {
                if (!birthday!.TextContent.Contains("Not Available"))
                {
                    string dateString = birthday!.TextContent.Replace("\n", "");
                    dateString = dateString.Replace("Birthday:", "").Trim();
                    DateTime date = DateTime.ParseExact(dateString, "MMM d, yyyy", CultureInfo.InvariantCulture);
                    return date;
                }
                else return DateTime.MinValue;
            }
            else return DateTime.MinValue;
        }

        private static string GetBirthplace(IElement? birthplace)
        {
            if (birthplace is not null)
            { 
                if (!birthplace!.TextContent.Contains("Not Available"))
                {
                    string loc = birthplace!.TextContent.Replace("\n", "");
                    string[] temp = loc.Split(',');
                    return temp.Last().Trim();
                }
                else
                    return "Unknown";
            }
            else
                return "Unknown";
        }
    }
}