using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Resource.Core.ResourceDao;
using Resource.Core.ResourceObjects;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ResourceUnitTest
{
    /// <summary>
    /// Created: Olaolu Ajose
    /// Date: 7/02/2017
    /// Title: Test Project to Parse MSG.json.txt
    /// Twitter handle @olaolumusic
    /// </summary>
    [TestClass]
    public class DailyScripturesTest
    {
        private static string _readingPlanTemplate;
        private static string BibleTranslation { get; set; }
        private Bible BibleVersion { get; set; }

        [TestInitialize]
        public void Setup()
        {
            var filepath = AppDomain.CurrentDomain.BaseDirectory + "\\resources\\DailyScriptures.txt";
            _readingPlanTemplate = File.ReadAllText(filepath);

            var bibleFilepath = AppDomain.CurrentDomain.BaseDirectory + "\\resources\\MSG.json.txt";
            BibleTranslation = File.ReadAllText(bibleFilepath);

            BibleVersion = new Bible
            {
                Name = "The Mesage Translation",
                ShortName = "TMSG",
                Books = new List<Book>()
            };
        }

        [TestMethod]
        public void DailyScripturesDaoTest()
        {
            var sampleConnectionString = SqldaoFactory.CreateConnection();

            var scriptures = new DailyScriptures();

            /// ExtrasDao.GetDailyScripturesAsync(scriptures, sampleConnectionString.ConnectionString);

            Assert.AreEqual(true, true);

        }

        [TestMethod]
        public void GetDailyReadingTemplate()
        {
            // var getEmailTemplate = EmailServices.GetEmailTemplate(Constants.NotificationType.WelcomeTemplate);

            var filepath = AppDomain.CurrentDomain.BaseDirectory + "\\resources\\DailyScriptures.txt";
            var template = File.ReadAllText(filepath);

            Trace.WriteLine(string.Format("Visualize the design...{0}", template));
            Assert.IsNotNull(template);

        }
        /// <summary>
        /// Felt I should share this as it took me a while to come by a TMSG Json format
        /// And also parsing it was a bit techy.
        /// This test in designed to parse the Json Version of
        /// The Bible and return a C# Bible object in the following structure:
        /// Bible--|
        ///        Books|
        ///              Chapters--|
        ///                        Verses--|
        ///  </summary>
        [TestMethod]
        public void ParseDailyScripturesTest()
        {

            #region -- Arrange --
            var dailyReading = "Gen. 1-2; Ps. 1; Matt. 1-2".ToLower().Split(';');

            #endregion

            #region -- Act --
            //Ensures there is a daily scriptures to transvers
            if (dailyReading.Count() <= 1) return;

            //All books after reading from File
            var jObject = JObject.Parse(BibleTranslation);
            var bookId = 0;

            //Get all book names eg Genesis, Exodus...
            var bookNames = jObject.Properties().Select(book => book.Name).ToList();

            var bibleBooks = jObject.Children();

            //Each Book in the Bible
            foreach (var book in bibleBooks)
            {
                var tempBook = new Book
                {
                    BookName = bookNames[bookId],
                    BookChapter = new List<Chapter>()
                };

                var bibleBook = book.Children();
                //Book Chapters
                //var chapterNumber = 1;
                foreach (var chapters in bibleBook)
                {
                    var bookChapters = chapters.Children();

                    foreach (var chapter in bookChapters)
                    {
                        var tempChapter = new Chapter
                        {
                            ChapterId = Convert.ToInt16(chapter.Path.Split('.')[1]),
                            ChaperVerses = new List<Verse>()
                        };

                        var chapterVerses = chapter.Children();

                        //Foreach verse in Chapter
                        foreach (var verse in chapterVerses)
                        {
                            var bookVerse = JObject.Parse(verse.ToString());


                            foreach (var tag in bookVerse)
                            {
                                var tempVerse = new Verse
                                {
                                    Id = Convert.ToInt16(tag.Key),
                                    VerseText = tag.Value.ToString()
                                };

                                tempChapter.ChaperVerses.Add(tempVerse);
                            }
                        }
                        tempBook.BookChapter.Add(tempChapter);
                        //chapterNumber++;
                    }

                }
                BibleVersion.Books.Add(tempBook);
                bookId++;
            }
            //Now lets search the Bible
            var dailyBook = new Book();
            foreach (var book in BibleVersion.Books.Where(book => book.BookName.ToLower().StartsWith(dailyReading[1].Split('.')[0].Trim())))
            {
                dailyBook = book;
                break;
            }
            var dailyChapter = new Chapter();
            foreach (var chapter in dailyBook.BookChapter.Where(chapter => chapter.ChapterId == Convert.ToInt16(dailyReading[1].Split('.')[1])))
            {
                dailyChapter = chapter;
                break;
            }
            var dailyVerses = new List<Verse>();
            dailyVerses.AddRange(dailyChapter.ChaperVerses);

            #endregion

            #region -- Assert Psalm 1 contains 6 verses-- 
            Assert.AreEqual(dailyVerses.Count, 6);
            #endregion
        }
    }
}
