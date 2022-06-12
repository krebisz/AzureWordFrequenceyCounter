using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights;

namespace AzureFunctionsApp
{
    public static class FrequencyFunction
    {
        public static string resultOutput;
        public static string partitionKey;
        public static string rowKey;

        public static string TableReference = "wordentities";
        public static string CconnectionString = "DefaultEndpointsProtocol=https;AccountName=freqcounter-cosmosdb;AccountKey=niwn9MFBfWKJTtf4KpmhRefljAVtPcMf1qzV1g9G0pn3HhoRT4R5u9PACAotyfPxTZbVlExSY6c4Ch2GlWIUDw==;TableEndpoint=https://freqcounter-cosmosdb.table.cosmos.azure.com:443/;";
        public static IDictionary<char, int> LetterScores;
        private static readonly TelemetryClient telemetryClient;

        [FunctionName("FrequencyFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            PopulateLetterScoreDictionary();

            try
            {
                IFormFile file = req.Form.Files[0]; //RESOURCE UTILIZED: https://soltisweb.com/blog/detail/2020-11-10-howtopostafiletoazurefunctionin3minutes
                RunWordCounters(file);

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CconnectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(TableReference);

                DataHelper.CreateCloudTable(table);
                DataHelper.InsertTableEntity(table, partitionKey, rowKey);
                resultOutput += DataHelper.SelectTableEntity(table, partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            watch.Stop();
            resultOutput += "Total time elapsed: " + watch.ElapsedMilliseconds + " ms \r\n";

            return new OkObjectResult(resultOutput);
        }

        /// <summary>
        /// Populates the scrabble letter score dictionary
        /// </summary>
        public static void PopulateLetterScoreDictionary()
        {
            LetterScores = new Dictionary<char, int>();
            LetterScores.Add('a', 1);
            LetterScores.Add('b', 3);
            LetterScores.Add('c', 3);
            LetterScores.Add('d', 2);
            LetterScores.Add('e', 1);
            LetterScores.Add('f', 4);
            LetterScores.Add('g', 2);
            LetterScores.Add('h', 4);
            LetterScores.Add('i', 1);
            LetterScores.Add('j', 8);
            LetterScores.Add('k', 5);
            LetterScores.Add('l', 1);
            LetterScores.Add('m', 3);
            LetterScores.Add('n', 1);
            LetterScores.Add('o', 1);
            LetterScores.Add('p', 3);
            LetterScores.Add('q', 10);
            LetterScores.Add('r', 1);
            LetterScores.Add('s', 1);
            LetterScores.Add('t', 1);
            LetterScores.Add('u', 1);
            LetterScores.Add('v', 4);
            LetterScores.Add('w', 4);
            LetterScores.Add('x', 8);
            LetterScores.Add('y', 4);
            LetterScores.Add('z', 10);
        }

        public static string ReturnMostFrequentWord(List<WordEntity> WordEntityList, int wordLength = 0)
        {
            string message = string.Empty;
            string word = string.Empty;
            int frequency = 0;

            foreach (WordEntity t in WordEntityList)
            {
                if (wordLength == 0 || t.Word.Length == wordLength)
                {
                    if (t.Frequency > frequency)
                    {
                        word = t.Word;
                        frequency = t.Frequency;
                    }
                }
            }

            if (wordLength > 0)
            {
                message = "Most frequent " + wordLength.ToString() + " word: " + word + " occurred " + frequency.ToString() + " times";
            }
            else
            {
                //These two variables are here to provided the "highest frequency" word found. It is very basic, but proves the functionality requested as to what is stored within Azure
                partitionKey = word;
                rowKey = frequency.ToString();
                message = "Most frequent word: " + word + " occurred " + frequency.ToString() + " times";
            }

            return message;
        }

        /// <summary>
        /// Returns the highest scoring word in scrabble from the word list
        /// </summary>
        /// <param name="WordEntityList">Takes a list of entities that inherit from the table entity object</param>
        /// <returns></returns>
        public static string ReturnHighestScoreWord(List<WordEntity> WordEntityList)
        {
            string word = string.Empty;
            int amount = 0;

            foreach (WordEntity t in WordEntityList)
            {
                int score = GetWordScore(t.Word);

                if (score > amount)
                {
                    word = t.Word;
                    amount = score;
                }
            }

            //AddNotification(new Notification("Highest scoring word(s)(according to Scrabble): " + word + " with a score of " + amount.ToString(), Notification.MessageType.Information));
            string message = "Highest scoring word(s)(according to Scrabble): " + word + " with a score of " + amount.ToString();

            return message;
        }

        /// <summary>
        /// Computes scrabble score for an individual word
        /// </summary>
        /// <param name="word">string to be scored</param>
        /// <returns></returns>
        public static int GetWordScore(string word)
        {
            int score = 0;

            foreach (char c in word)
            {
                bool keyExists = LetterScores.ContainsKey(c); 

                if (keyExists)
                {
                    score += LetterScores[c];
                }
            }

            return score;
        }

        /// <summary>
        /// The calling method that retrieves the posted file and the subsequent word counts
        /// </summary>
        /// <param name="file">a form file that is posted to the azure function app</param>
        private static void RunWordCounters(IFormFile file)
        {
            string Filename = file.FileName;

            if (!object.Equals(Filename, null))
            {
                string stringData = ReadFileAsString(file);
                IEnumerable<string> WordList = GetWordsFromString(stringData);
                List<WordEntity> WordEntityList = CreateWordEntityList(WordList);

                resultOutput += ReturnMostFrequentWord(WordEntityList) + "\r\n";
                resultOutput += ReturnMostFrequentWord(WordEntityList, 7) + "\r\n";
                resultOutput += ReturnHighestScoreWord(WordEntityList) + "\r\n";
            }
        }

        /// <summary>
        /// Takes a posted file and reads it as a string
        /// </summary>
        /// <param name="file">a form file that is posted to the azure function app</param>
        /// <returns></returns>
        public static string ReadFileAsString(IFormFile file)
        {
            string stringData;

            using (Stream stream = file.OpenReadStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                stringData = reader.ReadToEnd();
            };

            return stringData;
        }

        /// <summary>
        /// An input string is broken into individual words in a string list
        /// </summary>
        /// <param name="stringData">a string to be partitioned</param>
        /// <returns></returns>
        private static IEnumerable<string> GetWordsFromString(string stringData)
        {
            stringData = CleanString(stringData);
            string[] wordArray = stringData.Split(new char[] { ' ', });
            List<string> wordList = new List<string>();

            foreach (string word in wordArray)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    wordList.Add(word);
                }
            }

            wordList.Sort(); //The sort allows for optimization of counting through words
            return wordList;
        }

        /// <summary>
        /// Used for filtering out characters that do not make up words within a provided input string
        /// </summary>
        /// <param name="stringIn">the string to clean special characters from</param>
        /// <param name="setToLower">sets input to lower case</param>
        /// <returns></returns>
        public static string CleanString(string stringIn, bool setToLower = true)
        {
            NumberFormatInfo nfi = NumberFormatInfo.CurrentInfo;
         
            string pattern = @"^\s*["; //Define the regular expression
            pattern += Regex.Escape(nfi.PositiveSign + nfi.NegativeSign) + @"]?\s?"; //Get the positive and negative sign symbols
            pattern += Regex.Escape(nfi.CurrencySymbol) + @"?\s?"; //Get the currency symbol
            pattern += @"(\d*"; //Get integral digits
            pattern += Regex.Escape(nfi.CurrencyDecimalSeparator) + "?"; //Get the decimal separator
            pattern += @"\d{"; //Get the fractional digits
            pattern += nfi.CurrencyDecimalDigits.ToString() + "}?){1}$"; //Get the number of fractional digits in currency values

            return Regex.Replace(stringIn, pattern, "");
        }

        /// <summary>
        /// Accepts a string list as input and outputs a WordEntityList containing the distinct words and occurances found
        /// </summary>
        /// <param name="wordList">a string list of non-distinct words</param>
        /// <returns></returns>
        public static List<WordEntity> CreateWordEntityList(IEnumerable<string> wordList)
        {
            List<WordEntity> wordEntityList = new List<WordEntity>();
            IEnumerable<string> distinctWordList = wordList.Distinct();

            foreach (string word in distinctWordList) //MAIN performance bottleneck
            {
                int count = 0;
                string currentword = wordList.ElementAt(0); 

                while (word == currentword && wordList.Count() > count + 1)
                {
                    count++;

                    if (wordList.Count() > count + 1)
                    currentword = wordList.ElementAt(count);
                }

                wordList = wordList.Skip(count); //Work with an increasingly smaller list


                WordEntity wordEntity = new WordEntity();
                wordEntity.Word = word;
                wordEntity.Frequency = count;

                wordEntityList.Add(wordEntity); //could keep max count at this level to provide output without looping again in functions built to find highest frequency & score word
            }

            return wordEntityList;
        }
    }
}