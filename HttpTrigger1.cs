using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShoppingCartList.Models;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
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
        string gmessage="Retrieved all items successfully";
        dynamic gmydata = new ExpandoObject();
        gmydata.message = gmessage;
        gmydata.Data=shp;
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(gmydata);

        return new OkObjectResult(json);
    }


    [FunctionName("GetShoppingCartItemById")]
        public async Task<IActionResult> GetShoppingCartItemById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getshoppingcartitembyid/{id}/{category}")]
             HttpRequest req, ILogger log,string id,string category)
                   
        {
            log.LogInformation($"Getting Shopping Cart Item with ID: {id}");
            try
            {
                var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
                string getmessage="Retrived  an item successfully by Id";
                dynamic gmydata = new ExpandoObject();
                gmydata.message = getmessage;
                gmydata.Data=item.Resource;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(gmydata);
                return new OkObjectResult(json);
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string responseMessage="Invalid input params,Please check";
                return new NotFoundObjectResult(responseMessage);
            }
        }
        

    [FunctionName("CreateShoppingCartItem")]
        public async Task<IActionResult> CreateShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createshoppingcartitem")] HttpRequest req,
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
            string responsemessage="Created an item successfully";
            dynamic cmydata = new ExpandoObject();
            cmydata.message = responsemessage;
            cmydata.Data=item;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(cmydata);
            return new OkObjectResult(json);
        }

        [FunctionName("UpdateShoppingCartItem")]
        public async Task<IActionResult> UpdateShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateshoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id,string category)
        {
            log.LogInformation($"Updating Shopping Cart Item with ID: {id}");

            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<UpdateShoppingCartItem>(requestData);
            var item = await documentContainer.ReadItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));

            if (item.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string responseMessage="There is no item with the mentioned id";
                return new NotFoundObjectResult(responseMessage);
            }

            item.Resource.Collected = data.Collected;
            await documentContainer.UpsertItemAsync(item.Resource);
            string updatemessage="Updated successfully";
            dynamic upmydata = new ExpandoObject();
            upmydata.message = updatemessage;
            upmydata.Data=item.Resource;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(upmydata);
            return new OkObjectResult(json);
        }

        [FunctionName("DeleteShoppingCartItem")]
        public async Task<IActionResult> DeleteShoppingCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delshoppingcartitem/{id}/{category}")] HttpRequest req,
            ILogger log, string id,string category)
        {
            log.LogInformation($"Deleting Shopping Cart Item with ID: {id}");

            await documentContainer.DeleteItemAsync<ShoppingCartItem>(id, new Microsoft.Azure.Cosmos.PartitionKey(category));
            string responseMessage="Deleted sucessfully";         
            return new OkObjectResult(responseMessage);
        }
    }
}