using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AWS;
using Microsoft.AspNetCore.Mvc;

public class SQSController : Controller
{
    private readonly IAmazonSQS _sqsClient = new AmazonSQSClient(new BasicAWSCredentials("_____", "_____"), RegionEndpoint.USEast1);
    private readonly string _queueName = "rashed_b8_test";

    public async Task<string> GetQueueUrl()
    {
        var request = new GetQueueUrlRequest
        {
            QueueName = _queueName,
        };

        GetQueueUrlResponse response = await _sqsClient.GetQueueUrlAsync(request);
        return response.QueueUrl;
    }

    [HttpPost]
    public async Task<ActionResult> AddMessage(IFormCollection formCollection)
    {
        var newMessage = formCollection["newMessage"];

        if (!string.IsNullOrWhiteSpace(newMessage))
        {
            using (var client = new AmazonSQSClient(RegionEndpoint.USEast1))
            {
                var queueUrl = await GetQueueUrl();

                var request = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = newMessage
                };

                await client.SendMessageAsync(request);
            }
        }

        return RedirectToAction("Messages");
    }


    public async Task<ActionResult> Messages()
    {
        using (var client = new AmazonSQSClient(RegionEndpoint.USEast1))
        {
            var queueUrl = await GetQueueUrl();
            var numberOfMessages = 10; 

            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = numberOfMessages
            };

            var response = await client.ReceiveMessageAsync(request);

            var messageBodies = response.Messages.Select(message => message.Body).ToList();
            var receiptHandles = response.Messages.Select(message => message.ReceiptHandle).ToList();

            var messageCount = await GetMessageCountInQueueAsync(queueUrl);

            var viewModel = new MessagesViewModel
            {
                Messages = messageBodies,
                ReceiptHandles = receiptHandles,
                MessageCount = messageCount
            };

            return View(viewModel);
        }
    }

    [HttpPost]
    public async Task<ActionResult> DeleteMessages(List<string> receiptHandles)
    {
        using (var client = new AmazonSQSClient(RegionEndpoint.USEast1))
        {
            var queueUrl = await GetQueueUrl();

            var request = new DeleteMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = receiptHandles.Select((receiptHandle, index) => new DeleteMessageBatchRequestEntry
                {
                    Id = index.ToString(),
                    ReceiptHandle = receiptHandle
                }).ToList()
            };

            await client.DeleteMessageBatchAsync(request);
        }

        return RedirectToAction("Messages");
    }

    private async Task<int> GetMessageCountInQueueAsync(string queueUrl)
    {
        using (var client = new AmazonSQSClient(RegionEndpoint.USEast1))
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
            };

            var response = await client.GetQueueAttributesAsync(request);

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessages", out string messageCount))
            {
                return int.Parse(messageCount);
            }
            else
            {
                return -1;
            }
        }
    }
}
