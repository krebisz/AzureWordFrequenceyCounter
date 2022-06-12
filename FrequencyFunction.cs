using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Identity;
using Microsoft.AspNetCore.Http;


//https://azurefunctionsfrequencycounter.azurewebsites.net/api/FrequencyFunction
namespace AzureFunctionsApp
{
    public static class FrequencyFunction
    {
        public static IDictionary<char, int> LetterScores;
        public static string resultOutput;

        [FunctionName("FrequencyFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var file = req.Form.Files[0]; //https://soltisweb.com/blog/detail/2020-11-10-howtopostafiletoazurefunctionin3minutes

                PopulateLetterScoreDictionary();
                GetStringFromFile(file);

                UploadTable();
                resultOutput += DownloadTable("Harp", "Walter") + "\r\n";

                return new OkObjectResult(resultOutput);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }



        //https://stackoverflow.com/questions/51730269/how-to-upload-a-single-variable-to-azure-table-storage-c-sharp
        public static void UploadTable()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=freqcounter-cosmosdb;AccountKey=niwn9MFBfWKJTtf4KpmhRefljAVtPcMf1qzV1g9G0pn3HhoRT4R5u9PACAotyfPxTZbVlExSY6c4Ch2GlWIUDw==;TableEndpoint=https://freqcounter-cosmosdb.table.cosmos.azure.com:443/;";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("entities");
            Task t1 = table.CreateIfNotExistsAsync();
            t1.Wait();

            // Create a new customer entity.
            WordEntity wordEntity = new WordEntity("Harp", "Walter");
            wordEntity.Word = "Walter@contoso.com";
            wordEntity.Frequency = 22;

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert((Microsoft.WindowsAzure.Storage.Table.ITableEntity)wordEntity);

            // Execute the insert operation.
            Task t = table.ExecuteAsync(insertOperation);
            t.Wait();


        }





        public static string DownloadTable(string partitionKey, string rowKey)
        {
            string resultOutput = string.Empty;

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=freqcounter-cosmosdb;AccountKey=niwn9MFBfWKJTtf4KpmhRefljAVtPcMf1qzV1g9G0pn3HhoRT4R5u9PACAotyfPxTZbVlExSY6c4Ch2GlWIUDw==;TableEndpoint=https://freqcounter-cosmosdb.table.cosmos.azure.com:443/;";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("entities");
            //table.CreateIfNotExistsAsync();

            TableOperation tableOperation = TableOperation.Retrieve<WordEntity>(partitionKey, rowKey);
            Task<TableResult> tableResult = table.ExecuteAsync(tableOperation);



            var entity = tableResult.Result;

            var entities = new WordEntity();


            entities.Word = ((AzureFunctionsApp.WordEntity)entity.Result).Word;
            entities.Frequency = ((AzureFunctionsApp.WordEntity)entity.Result).Frequency;

            resultOutput = entities.Word;
            return resultOutput;
        }

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


        public static string ReturnMostFrequentWord(List<Tuple<string, int>> WordTupleList, int wordLength = 0)
        {
            string message = string.Empty;
            string word = string.Empty;
            int amount = 0;

            foreach (Tuple<string, int> t in WordTupleList)
            {
                if (wordLength == 0 || t.Item1.Length == wordLength)
                {
                    if (t.Item2 > amount)
                    {
                        word = t.Item1;
                        amount = t.Item2;
                    }
                }
            }

            if (wordLength > 0)
            {
                message = "Most frequent " + wordLength.ToString() + " word: " + word + " occurred " + amount.ToString() + " times";
            }
            else
            {
                message = "Most frequent word: " + word + " occurred " + amount.ToString() + " times";
            }

            return message;
        }


        public static string ReturnHighestScoreWord(List<Tuple<string, int>> WordTupleList)
        {
            string message = string.Empty;
            string word = string.Empty;
            int amount = 0;

            foreach (Tuple<string, int> t in WordTupleList)
            {
                int score = GetWordScore(t.Item1);

                if (score > amount)
                {
                    word = t.Item1;
                    amount = score;
                }
            }

            //AddNotification(new Notification("Highest scoring word(s)(according to Scrabble): " + word + " with a score of " + amount.ToString(), Notification.MessageType.Information));
            message = "Highest scoring word(s)(according to Scrabble): " + word + " with a score of " + amount.ToString();

            return message;
        }

        public static int GetWordScore(string word)
        {
            int score = 0;

            foreach (char c in word)
            {
                bool keyExists = LetterScores.ContainsKey(c); //https://www.techiedelight.com/determine-key-exists-dictionary-csharp/

                if (keyExists)
                {
                    score += LetterScores[c];
                }
            }

            return score;
        }

        private static void GetStringFromFile(IFormFile file)
        {
            string Filename = file.FileName;

            if (!object.Equals(Filename, null))
            {
                string stringData = ReadFileAsString(file);
                IEnumerable<string> WordList = GetWordsFromString(stringData);
                List<Tuple<string, int>> WordTupleList = CreateCountedWordList(WordList);

                resultOutput += ReturnMostFrequentWord(WordTupleList) + "\r\n";
                resultOutput += ReturnMostFrequentWord(WordTupleList, 7) + "\r\n";
                resultOutput += ReturnHighestScoreWord(WordTupleList) + "\r\n";

            }
        }

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

        private static IEnumerable<string> GetWordsFromString(string stringData)
        {
            stringData = CleanString(stringData);
            string[] stringArray = stringData.Split(new char[] { ' ', });

            List<string> stringList = new List<string>();

            foreach (string word in stringArray)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    stringList.Add(word);
                }
            }

            stringList.Sort();

            return stringList;
        }

        public static string CleanString(string stringIn, bool setToLower = true)
        {
            NumberFormatInfo nfi = NumberFormatInfo.CurrentInfo;

            // Define the regular expression pattern.
            string pattern = @"^\s*[";
            // Get the positive and negative sign symbols.
            pattern += Regex.Escape(nfi.PositiveSign + nfi.NegativeSign) + @"]?\s?";
            // Get the currency symbol.
            pattern += Regex.Escape(nfi.CurrencySymbol) + @"?\s?";
            // Add integral digits to the pattern.
            pattern += @"(\d*";
            // Add the decimal separator.
            pattern += Regex.Escape(nfi.CurrencyDecimalSeparator) + "?";
            // Add the fractional digits.
            pattern += @"\d{";
            // Determine the number of fractional digits in currency values.
            pattern += nfi.CurrencyDecimalDigits.ToString() + "}?){1}$";

            string filelinesclean = Regex.Replace(stringIn, pattern, "");
            return filelinesclean;
        }


        public static List<Tuple<string, int>> CreateCountedWordList(IEnumerable<string> WordList)
        {
            List<Tuple<string, int>> WordTupleList = new List<Tuple<string, int>>();

            IEnumerable<string> DistinctWordList = WordList.Distinct();

            foreach (string word in DistinctWordList)
            {
                int count = WordList.Count(e => e == word);

                // <word, frequency>
                Tuple<string, int> WordTuple = new Tuple<string, int>(word, count); //https://www.tutorialsteacher.com/csharp/csharp-tuple#:~:text=The%20following%20example%20creates%20a,passed%20values%20to%20the%20constructor. & https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples#tuples-vs-systemtuple
                WordTupleList.Add(WordTuple); //could keep max count at this level to provide output without looping again in function below for highest frequency & score word
            }

            return WordTupleList;
        }
    }



    public class WordEntity : TableEntity
    {
        public WordEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        public WordEntity() { }

        public string Word { get; set; }

        public int Frequency { get; set; }
    }
}