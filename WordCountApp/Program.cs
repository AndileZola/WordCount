using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Linq.Expressions;

namespace WordCountApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                //The stop watch to tracke how long it took the app to run
                Stopwatch exectuionTimeTracker = new Stopwatch();
                //Start the stopwatch
                exectuionTimeTracker.Start();

                var url = "http://www.gutenberg.org/files/2600/2600-0.txt";
                //Get the text from online
                string warAndPeachBook = await DownloadTheBookFromOnline(url);
                //string warAndPeachBook = await client.GetStringAsync("http://www.gutenberg.org/files/2600/2600-0.txt");

                //Convert the text to an array of words
                var AllWordsArray = SplitWords(warAndPeachBook).ToList();

                //Write all grouped words to a file
                WriteToFile("AllGroupedWords.txt", AllWordsArray);

                //Get top 50 words ordeerd by descending 
                var TopFiftyWordsByCount = OrderWordsByCount(AllWordsArray, 50);
                //Write TopFiftyWordsByCount words to a file
                WriteToFile("50WordsOrderedByCount.txt", TopFiftyWordsByCount);

                //Get top 50 words ordeerd by descending
                var TopFiftyWordsByLength = OrderWordsByLength(AllWordsArray, 50);
                //Write TopFiftyWordsByCount words to a file
                WriteToFile("50WordsOrderedByLength.txt", TopFiftyWordsByLength);

                //Stop the stopwatch
                exectuionTimeTracker.Stop();

                //Record the time elapsed.
                TimeSpan timeTaken = exectuionTimeTracker.Elapsed;

                //Display the time lapsed
                Console.WriteLine($"Time taken is : {timeTaken}");

                //Hold the screen
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                //Log the error, to help with the investigation
                File.WriteAllText(@"..\..\..\ErrorLog.txt",ex.Message);
            }
        }
        /// <summary>
        /// This fuction accepts a string and split it into a list of strings
        /// </summary>
        /// <param name="book">Book text</param>
        /// <returns>List of BookWords Object</returns>
        static List<BookWords> SplitWords(string book)
        {
            //Regex pattern to find multiple spaces and replace them with one space
            var RegexPartternToFindSpaces = @"([\s]{1,})";
            Regex regex = new Regex(RegexPartternToFindSpaces);

            //Remove newline and carriage return form the text
            book = book.Replace("\r\n", string.Empty);

            //if there are more one spaces that follow each other, replace them with one space
            book = regex.Replace(book, " ");

            //Convert the text to a list of words
            string[] _wordsArray = book.Split(' ');            

            //Return the grouped words
            return GroupWordsToListOfObject(_wordsArray);
        }

        /// <summary>
        /// This fuction accepts array of strings and modify it to a list of BookWords Object
        /// </summary>
        /// <param name="book">List of book words</param>
        /// <returns>List of BookWords Object</returns>
        static List<BookWords> GroupWordsToListOfObject(string[] words)
        {
            var GroupedWords = words.GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .Select(group => new BookWords
            {
                Word = group.Key.Trim(),
                Counter = group.Count(),
                Length = group.Key.Trim().Length
            }).OrderByDescending(x=>x.Counter).ToList();

            return GroupedWords;
        }

        /// <summary>
        /// This fuction queries the list of BookWords and return the required count
        /// </summary>
        /// <param name="words">list of BookWords</param>
        /// <param name="take">required count of words</param>
        /// <returns>filtered list of BookWords</returns>
        static List<BookWords> OrderWordsByCount(List<BookWords> words,int take=50)
        {
            var OrderedWords = words.OrderByDescending(x => x.Counter).Take(take).ToList();
            return OrderedWords;
        }

        /// <summary>
        /// This fuction queries the list of BookWords and return the required count
        /// </summary>
        /// <param name="words">list of BookWords</param>
        /// <param name="take">required count of words</param>
        /// <returns>filtered list of BookWords</returns>
        static List<BookWords> OrderWordsByLength(List<BookWords> words, int take = 50)
        {
            var OrderedWords = words.Where(x => x.Length > 6).Take(take).ToList();
            return OrderedWords;
        }

        /// <summary>
        /// This function calls the url to get the book from
        /// </summary>
        /// <param name="url">the url wehere the book is found</param>
        /// <returns>content of the book in string</returns>
        static async Task<string> DownloadTheBookFromOnline(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                //Get the text from online
                return  await client.GetStringAsync("http://www.gutenberg.org/files/2600/2600-0.txt");
            }
        }

        /// <summary>
        /// This function writes a list of bookWords to a file
        /// </summary>
        /// <param name="fileName">Name of a file to write to</param>
        /// <param name="words">List of books words to write </param>
        static void WriteToFile(string fileName,List<BookWords> words)
        {
            //List of concatenated word and count to write to a file
            var wordAndCountList = new List<string>();

            //Concatenate the word and count into one line
            words.ForEach(SingleWord => wordAndCountList.Add($"Word:{SingleWord.Word} ===== Count:{SingleWord.Counter}"));

            //File path to write into
            var path = $@"..\..\..\{fileName}";

            //Add all the concatenated list to the file
            File.AppendAllLines(path, wordAndCountList.AsEnumerable());
        }

        class BookWords
        {
            public string Word { get; set; }
            public int Counter { get; set; }
            public int Length { get; set; }
        }
    }
}
     