using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Identity;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
//using Microsoft.Azure.Cosmos.Table;

//https://azurefunctionsfrequencycounter.azurewebsites.net/api/FrequencyFunction
namespace AzureFunctionsApp
{
    //public static class FileUpload
    //{
    //    [FunctionName("FileUpload")]
    //    public static async Task<IActionResult> Run(
    //        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    //    {
    //        string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    //        string containerName = Environment.GetEnvironmentVariable("ContainerName");
    //        Stream myBlob = new MemoryStream();
    //        var file = req.Form.Files["File"];
    //        myBlob = file.OpenReadStream();
    //        var blobClient = new BlobContainerClient(Connection, containerName);
    //        var blob = blobClient.GetBlobClient(file.FileName);
    //        await blob.UploadAsync(myBlob);
    //        return new OkObjectResult("file uploaded successfylly");
    //    }
    //}









    public static class FrequencyFunction
    {


        public static IDictionary<char, int> LetterScores;


        public static string fileInPath;
        public static string fileOutPath;
        public static string fileExcludePath;
        public static string fileArchivePath;
        public static string resultOutput;







        [FunctionName("FrequencyFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            //string name = req.Query["name"];
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;
            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";
            //return new OkObjectResult(responseMessage);

            try
            {
                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("azurefunctionsapp2022061"));


                // Create a BlobServiceClient that will authenticate through Active Directory
                Uri accountUri = new Uri("https://azurefunctionsapp2022061.blob.core.windows.net/");
                BlobServiceClient client = new BlobServiceClient(accountUri, new DefaultAzureCredential(), null);

                //UploadBlob();
                //DownloadBlob();



                //var formdata = await req.ReadFormAsync();
                var file = req.Form.Files[0]; //https://soltisweb.com/blog/detail/2020-11-10-howtopostafiletoazurefunctionin3minutes

                CreateDictionary();
                GetStringFromFile(file);

                //return new OkObjectResult(file.FileName + " - " + file.Length.ToString());


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

            //String tableName = "people";
            //tableClient.createTableIfNotExists(tableName);


            CloudTable table = tableClient.GetTableReference("people");
            table.CreateIfNotExistsAsync();







            // Create a new customer entity.
            CustomerEntity customer1 = new CustomerEntity("Harp", "Walter");
            customer1.Email = "Walter@contoso.com";
            customer1.PhoneNumber = "425-555-0101";

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert((Microsoft.WindowsAzure.Storage.Table.ITableEntity)customer1);

            // Execute the insert operation.
            table.ExecuteAsync(insertOperation);
        }





        public static string DownloadTable(string partitionKey, string rowKey)
        {
            string resultOutput = string.Empty;

            var connectionString = "DefaultEndpointsProtocol=https;AccountName=freqcounter-cosmosdb;AccountKey=niwn9MFBfWKJTtf4KpmhRefljAVtPcMf1qzV1g9G0pn3HhoRT4R5u9PACAotyfPxTZbVlExSY6c4Ch2GlWIUDw==;TableEndpoint=https://freqcounter-cosmosdb.table.cosmos.azure.com:443/;";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //String tableName = "people";
            //tableClient.createTableIfNotExists(tableName);


            CloudTable table = tableClient.GetTableReference("people");
            table.CreateIfNotExistsAsync();



            TableOperation tableOperation = TableOperation.Retrieve<CustomerEntity>(partitionKey, rowKey);
            Task<TableResult> tableResult = table.ExecuteAsync(tableOperation);

            var customerEntity = tableResult.Result;
            
                var entities = new CustomerEntity();


            entities.Email = ((AzureFunctionsApp.CustomerEntity)customerEntity.Result).Email;
            entities.PhoneNumber = ((AzureFunctionsApp.CustomerEntity)customerEntity.Result).PhoneNumber;


            //((Microsoft.WindowsAzure.Storage.Table.TableResult)customerEntity).Result




            //TableResult result = tableResult.Result;


            //entities.Add(result);

            //resultOutput = ((CustomerEntity)tableResult.Result).Email;
            //Console.WriteLine(((CustomerEntity)tableResult.Result).Email);


            resultOutput = entities.Email;


            return resultOutput;





            //// Create a new customer entity.
            //CustomerEntity customer1 = new CustomerEntity("Harp", "Walter");
            //customer1.Email = "Walter@contoso.com";
            //customer1.PhoneNumber = "425-555-0101";

            // Create the TableOperation object that inserts the customer entity.
            //TableOperation insertOperation = TableOperation.Insert((Microsoft.WindowsAzure.Storage.Table.ITableEntity)customer1);





            //TableOperation selectOperation = TableOperation.Retrieve();
            //// Execute the insert operation.
            //table.ExecuteAsync(selectOperation);
        }





        public static void UploadBlob()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=azurefunctionsapp2022061;AccountKey=3HFQ0g4DeMc8I2gPiA+iVXI/Mnrg5dXOsyU9PQZn25P35GKmvAjQKydzdXefdFOCwTec242FsbeO+ASt/7lJCw==;EndpointSuffix=core.windows.net";
            string containerName = "alchemycontainer";
            string blobName = "alchemyblob";
            string filePath = "elements.txt";




            // Get a reference to a container named "sample-container" and then create it
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);


            if (!container.Exists())
                container.Create();


            Uri accountUri = new Uri("https://azurefunctionsapp2022061.blob.core.windows.net/");
            BlobContainerClient _container = new BlobContainerClient(accountUri, null);


            //await new BlobClient(new Uri("https://aka.ms/bloburl")).DownloadToAsync(downloadPath);




            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient(blobName);

            // Upload local file
            blob.Upload(filePath);
        }


        //public static void UploadBlob()
        //{
        //    string connectionString = "DefaultEndpointsProtocol=https;AccountName=azurefunctionsapp2022061;AccountKey=3HFQ0g4DeMc8I2gPiA+iVXI/Mnrg5dXOsyU9PQZn25P35GKmvAjQKydzdXefdFOCwTec242FsbeO+ASt/7lJCw==;EndpointSuffix=core.windows.net";
        //    string containerName = "alchemycontainer";
        //    string blobName = "alchemyblob";
        //    string filePath = "elements.txt";

        //    // Get a reference to a container named "sample-container" and then create it
        //    BlobContainerClient container = new BlobContainerClient(connectionString, containerName);


        //    if (!container.Exists())
        //    container.Create();


        //    Uri accountUri = new Uri("https://azurefunctionsapp2022061.blob.core.windows.net/");
        //    BlobContainerClient _container = new BlobContainerClient(accountUri, null);


        //    //await new BlobClient(new Uri("https://aka.ms/bloburl")).DownloadToAsync(downloadPath);


        //    // Get a reference to a blob named "sample-file" in a container named "sample-container"
        //    BlobClient blob = container.GetBlobClient(blobName);

        //    // Upload local file
        //    blob.Upload(filePath);
        //}


        public static void DownloadBlob()
        {
            // Get a temporary path on disk where we can download the file
            string downloadPath = "hello.jpg";

            // Download the public blob at https://aka.ms/bloburl
            new BlobClient(new Uri("https://aka.ms/bloburl")).DownloadTo(downloadPath);
        }





        public static void CreateDictionary()
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




        private static void GetStringFromFile(IFormFile _file)
        {
            string Filename = _file.FileName;

            if (!object.Equals(Filename, null))
            {
                string stringData = ReadFileAsString(_file);
                IEnumerable<string> WordList = GetWordsFromString(stringData);
                List<Tuple<string, int>> WordTupleList = CreateCountedWordList(WordList);

                resultOutput += ReturnMostFrequentWord(WordTupleList) + "\r\n";
                resultOutput += ReturnMostFrequentWord(WordTupleList, 7) + "\r\n";
                resultOutput += ReturnHighestScoreWord(WordTupleList) + "\r\n";

            }
        }

        public static string ReadFileAsString(IFormFile _file)
        {
            string stringData;



            using (Stream stream = _file.OpenReadStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                stringData = reader.ReadToEnd(); //.ReadToEndAsync();

                // Do something with file data
            };




            //using (Stream fileStream = GetFileStream(fileName))
            //{
            //    StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
            //    stringData = streamReader.ReadToEnd();
            //}

            return stringData;
        }


        //public static string ReadFileAsString(string fileName)
        //{
        //    string stringData;

        //    using (Stream fileStream = GetFileStream(fileName))
        //    {
        //        StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);
        //        stringData = streamReader.ReadToEnd();
        //    }

        //    return stringData;
        //}






        public static Stream GetFileStream(string fileName)
        {
            try
            {
                return new FileStream(fileName, FileMode.Open);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private static IEnumerable<string> GetWordsFromString(string stringData)
        {
            IEnumerable<string> stringList2 = new List<string>();

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

            stringList2 = stringList;
            //stringList2 = stringList2.OrderBy(e => e.First()).ToList();
            stringList.Sort();

            return stringList;
        }


















        public static string CleanString(string stringIn, bool setToLower = true)
        {
            //string pattern = "[^a-zA-Z0-9]";

            //Regex regex = new Regex("[^a-zA-Z0-9]");

            //return regex.Replace(stringIn, "");








            string stringOut = stringIn;
            //stringOut = stringIn.Replace("\r\n", "");
            //stringOut = stringOut.Replace(" ", "");

            //if (setToLower)
            //{
            //    stringOut = stringOut.ToLowerInvariant();
            //}

            //return stringOut;




            NumberFormatInfo nfi = NumberFormatInfo.CurrentInfo;

            // Define the regular expression pattern.
            string pattern;
            pattern = @"^\s*[";
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

            Regex rgx = new Regex(pattern);


            string filelinesclean = Regex.Replace(stringIn, pattern, "");




            //stringOut = stringIn.Replace(pattern, "");


            return filelinesclean;



        }


        public static List<Tuple<string, int>> CreateCountedWordList(IEnumerable<string> WordList)
        {
            List<Tuple<string, int>> WordTupleList = new List<Tuple<string, int>>();

            IEnumerable<string> DistinctWordList = WordList.Distinct();


            foreach (string word in DistinctWordList)
            {

                int count = WordList.Count(e => e == word);

                // <word, frequency, score>
                Tuple<string, int> WordTuple = new Tuple<string, int>(word, count); //https://www.tutorialsteacher.com/csharp/csharp-tuple#:~:text=The%20following%20example%20creates%20a,passed%20values%20to%20the%20constructor. & https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples#tuples-vs-systemtuple
                WordTupleList.Add(WordTuple); //could keep max count at this level to provide output without looping again in function below for highest frequency & score word
            }

            return WordTupleList;
        }


    }



    public class CustomerEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public CustomerEntity(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public CustomerEntity() { } // the parameter-less constructor must be provided

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        //public static explicit operator CustomerEntity(TableResult v)
        //{
        //    //throw new NotImplementedException();

        //    return this; // ((CustomerEntity)v.Result).Email;
        //}

        //public static explicit operator CustomerEntity(TableResult v)
        //{
        //    //throw new NotImplementedException();


        //}
    }
}
