using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsApp
{
    public static class DataHelper
    {
        /// <summary>
        /// Creates the table to hold the word entities
        /// </summary>
        /// <param name="table"></param>
        public static void CreateCloudTable(CloudTable table)
        {
            Task taskCreate = table.CreateIfNotExistsAsync();
            taskCreate.Wait();
        }

        /// <summary>
        /// Insert the word entity (table entity) into the Azure non-relational table
        /// </summary>
        /// <param name="table">a cloud table</param>
        /// <param name="partitionKey">the first identifier for the entity</param>
        /// <param name="rowKey">the second ifentifier for the entity</param>
        public static void InsertTableEntity(CloudTable table, string partitionKey, string rowKey) //RESOURCE UTILIZED: https://stackoverflow.com/questions/51730269/how-to-upload-a-single-variable-to-azure-table-storage-c-sharp
        {
            // Create a new word entity
            WordEntity wordEntity = new WordEntity(partitionKey, rowKey);
            wordEntity.Word = partitionKey;
            int.TryParse(rowKey, out int frequency);
            wordEntity.Frequency = frequency;

            // Create the TableOperation object that inserts the word entity
            TableOperation insertOperation = TableOperation.InsertOrReplace(wordEntity);

            // Execute the insert operation
            Task taskInsert = table.ExecuteAsync(insertOperation);
            taskInsert.Wait();
        }

        /// <summary>
        /// Selects a table (word) entity from the non-relational Azure table
        /// </summary>
        /// <param name="table">a cloud table</param>
        /// <param name="partitionKey">the first identifier for the entity</param>
        /// <param name="rowKey">the second ifentifier for the entity</param>
        /// <returns></returns>
        public static string SelectTableEntity(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<WordEntity>(partitionKey, rowKey);
            Task<TableResult> tableResult = table.ExecuteAsync(tableOperation);
            tableResult.Wait();
            var tableEntity = tableResult.Result;

            WordEntity wordEntity = new WordEntity();
            wordEntity.Word = ((WordEntity)tableEntity.Result).Word;
            wordEntity.Frequency = ((WordEntity)tableEntity.Result).Frequency;

            return ("Word in Azure: " + wordEntity.Word + " occurred " + wordEntity.Frequency.ToString() + " times \r\n");
        }

    }
}