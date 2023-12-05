using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;

public class DynamoDbController : Controller
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient(new BasicAWSCredentials("_____", "____"), RegionEndpoint.USEast1);
    private readonly string tableName = "rashedb8table1";

    public async Task<IActionResult> Index()
    {
        List<DynamoDbItem> items = await ReadAllItemAsync(_dynamoDbClient, tableName);
        return View(items);
    }

    public async Task<List<DynamoDbItem>> ReadAllItemAsync(IAmazonDynamoDB _dynamoDbClient, string tableName)
    {
        var table = Table.LoadTable(_dynamoDbClient, tableName);
        var scanFilter = new ScanFilter();
        var search = table.Scan(scanFilter);
        List<Document> documents = await search.GetRemainingAsync();

        List<DynamoDbItem> items = documents.Select(document =>
        {
            return new DynamoDbItem
            {
                Id = document["id"].AsString(),
                Name = document["Name"].AsString(),
                Description = document["Description"].AsString()
            };
        }).ToList();

        return items;
    }


    [HttpGet]
    public IActionResult Create()
    {
        var model = new DynamoDbItem();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DynamoDbItem item)
    {
        await AddItemAsync(item);
        return RedirectToAction("Index");

    }
    private async Task AddItemAsync(DynamoDbItem item)
    {
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>()
            {
                { "id", new AttributeValue {
                      S = item.Id
                  }},
                { "Name", new AttributeValue {
                      S = item.Name
                  }},
                { "Description", new AttributeValue {
                      S = item.Description
                  }
                }
            }
        };
        await _dynamoDbClient.PutItemAsync(request);
    }



    [HttpGet]
    public async Task<IActionResult> Update(string id)
    {
        var items = await ReadAllItemAsync(_dynamoDbClient, tableName);
        var item = items.FirstOrDefault(item => item.Id == id);
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Update(DynamoDbItem model)
    {
        await model.UpdateItem();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var items = await ReadAllItemAsync(_dynamoDbClient, tableName);
        var item = items.FirstOrDefault(item => item.Id == id);
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await DeleteItemAsync(id);
        return RedirectToAction("Index");
    }

    private async Task DeleteItemAsync(string id)
    {
        var request = new DeleteItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>()
            {
                { "id", new AttributeValue {
                      S = id
                  } }
            }
        };
        await _dynamoDbClient.DeleteItemAsync(request);
    }
}
