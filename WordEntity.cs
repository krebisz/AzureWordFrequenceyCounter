using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctionsApp
{
    /// <summary>
    /// The class that contains the words and frequency which are also table entities to use within a non-relational database
    /// </summary>
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