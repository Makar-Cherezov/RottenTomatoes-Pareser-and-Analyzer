using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System;
using System.IO;
using RottenTomatoesClient.Models;
using System.Linq;
using System.Windows;

namespace RottenTomatoesClient
{
    public static class ReportGenerator
    {
        internal static ViewModel ViewModel
        {
            get => default;
            set
            {
            }
        }

        internal static void Replace(string toFind, string toReplace, Word.Document wordDocument)
        {
            Word.Range range = wordDocument.StoryRanges[Word.WdStoryType.wdMainTextStory];
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: toFind, ReplaceWith: toReplace);
        }
        public static void GenerateReport(string opt1, string opt2, string opt3)
        {
            Word.Application wordWorker = new Word.Application();
            wordWorker.Visible = true;

            object wordFile = String.Format("{0}/ReportTemplate.docx", Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
            Word.Document wordDocument = wordWorker.Documents.Add(ref wordFile,
                false,
                Word.WdNewDocumentType.wdNewBlankDocument,
                true);

            int filmsCount = 0, directorsCount = 0, critRating = 0, audRating = 0;
            string critTitle = "", audTitle = "";

            using (var db = new _8i11CherezovRtContext())
            {
                filmsCount = db.Films.Count();
                directorsCount = db.Directors.Count();
                var critTop = db.Films
                    .OrderByDescending(x => x.CriticsReviewRating)
                    .Select(x => new { x.Title, x.CriticsReviewRating })
                    .FirstOrDefault();
                critRating = critTop!.CriticsReviewRating;
                critTitle = critTop!.Title;
                var audTop = db.Films
                    .OrderByDescending(x => x.AudienceReviewRating)
                    .Select(x => new { x.Title, x.AudienceReviewRating })
                    .FirstOrDefault();
                audRating = audTop!.AudienceReviewRating;
                audTitle = audTop!.Title;
            }

            Replace("[число_фильмов]", filmsCount.ToString(), wordDocument);
            Replace("[число_режиссёров]", directorsCount.ToString(), wordDocument);
            Replace("[фильм_критик]", critTitle, wordDocument);
            Replace("[фильм_зритель]", audTitle, wordDocument);
            Replace("[оценка_критик]", critRating.ToString(), wordDocument);
            Replace("[оценка_зритель]", audRating.ToString(), wordDocument);

            string lineChartSourcePath = ChartsGenerator.SaveLinePLot(opt1, opt2);
            string barChartSourcePath = ChartsGenerator.SaveBarPLot(opt3);

            // Move the cursor to the end of the document
            object moveEnd = WdUnits.wdStory;
            object what = WdGoToItem.wdGoToLine;
            object which = WdGoToDirection.wdGoToLast;
            object count = null; // This parameter is optional
            wordWorker.Selection.GoTo(what, which, count, moveEnd);

            wordDocument.InlineShapes.AddPicture(lineChartSourcePath);
            wordDocument.InlineShapes.AddPicture(barChartSourcePath);
            try
            {
                wordDocument.SaveAs2(String.Format("{0}/Reports/Report {1}.docx", 
                    Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, 
                    DateTime.Now.ToString("mm-HH-dd-MM-yy")));
                wordDocument.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            wordWorker.Quit(Word.WdSaveOptions.wdPromptToSaveChanges);

        }
    }
}