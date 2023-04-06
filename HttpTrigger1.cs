using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShoppingCartList.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;



namespace ShoppingCartList
{
    public class ShoppingCartApi
    {
    private const string DatabaseName = "ShoppingCartItems";
    private const string CollectionName = "Items";
        private readonly CosmosClient _cosmosClient;
        private Container documentContainer;
       
        public ShoppingCartApi(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = _cosmosClient.GetContainer("ShoppingCartItems", "Items");
        }
    [FunctionName("Getallshoppingcartitems")]
    public static IActionResult Getallemps(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getallshoppingcartitems")] HttpRequest req,
        [CosmosDB(
            DatabaseName,
                CollectionName,
                Connection ="CosmosDBConnectionString",
                SqlQuery = "SELECT * FROM c")]
               System.Collections.Generic.IEnumerable<ShoppingCartItem> shp,
        ILogger log)
    {
        log.LogInformation("Getting list of all employees ");
        return new OkObjectResult(shp);
    }


    [FunctionName("GetShoppingCartItemById")]
        public async Task<IActionResult> GetShoppingCartItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shoppingcartitem/{id}/{category}")]

            //[CosmosDB(
             //   DatabaseName,
              //  CollectionName,
              //  Connection ="CosmosDBConnectionString",
               // Id = "{id}",
               // PartitionKey = "{category}")]ShoppingCartItem shoppingCartItem,

            HttpRequest req, ILogger log, string id,string category)
        {
            log.LogInformation($"Getting Shopping Cart Item with ID: {id}");

            try
            {
                var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
                return new OkObjectResult(item.Resource);
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("CreateShoppingCartItem")]
        public async Task<IActionResult> CreateShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createshoppingcartitem")] HttpRequest req,
           // [CosmosDB(
            //DatabaseName,
            //CollectionName,
            //Connection ="CosmosDBConnectionString")] IAsyncCollector<ShoppingCartItem> shoppingCartItemsOut,
            ILogger log)
        {
            log.LogInformation("Creating Shopping Cart Item");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreateShoppingCartItem>(requestData);

            var item = new ShoppingCartItem
            {
                ItemName = data.ItemName,
                 Category = data.Category
            };

            await documentContainer.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.Category));

            ////await shoppingCartItemsOut.AddAsync(item);

            return new OkObjectResult(item);
        }

        [FunctionName("PutShoppingCartItem")]
        public async Task<IActionResult> PutShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateshoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id,string category)
        {
            log.LogInformation($"Updating Shopping Cart Item with ID: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateShoppingCartItem>(requestData);

            var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));

            if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }

            item.Resource.Collected = data.Collected;

            await documentContainer.UpsertItemAsync(item.Resource);

            return new OkObjectResult(item.Resource);
        }

        [FunctionName("DeleteShoppingCartItem")]
        public async Task<IActionResult> DeleteShoppingCartItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delshoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id,string category)
        {
            log.LogInformation($"Deleting Shopping Cart Item with ID: {id}");

            await documentContainer.DeleteItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
            return new OkResult();
        }
    }
}