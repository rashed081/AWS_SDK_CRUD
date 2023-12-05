using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

public class DynamoDbItem
{
    private IAmazonDynamoDB _dynamoDbClient;
    private string _tableName;

    public DynamoDbItem() { }
    public DynamoDbItem(IAmazonDynamoDB dynamoDbClient, string tableName)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = tableName;
    }

    [DynamoDBHashKey]
    public string Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; }

    [DynamoDBProperty]
    public string Description { get; set; }

    internal async Task UpdateItem()
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = this.Id } }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#N", "Name" },
                { "#D", "Description" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":newName", new AttributeValue { S = this.Name } },
                { ":newDescription", new AttributeValue { S = this.Description } }
            },
            UpdateExpression = "SET #N = :Name, #D = :Description",
            ReturnValues = "ALL_NEW"
        };

        await _dynamoDbClient.UpdateItemAsync(request);
    }
}