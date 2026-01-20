using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using zoplannerservice.Models;

namespace zoplannerservice.Services;

public class NotificationService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotificationService(IAmazonSQS sqsClient, string queueUrl)
    {
        _sqsClient = sqsClient;
        _queueUrl = queueUrl;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task SendEventAsync<T>(T @event)
    {
        // Debug för att kolla url.
        // Console.WriteLine($"DEBUG QueueUrl: {_queueUrl}");

        if (string.IsNullOrWhiteSpace(_queueUrl))
            throw new InvalidOperationException("QueueUrl is not configured");

        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var json = JsonSerializer.Serialize(@event, _jsonOptions);

        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = json
        };

        await _sqsClient.SendMessageAsync(request);
    }

    public async Task<List<NewAssignmentEvent>> ReceiveAssignmentEventsAsync()
    {

        var request = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 5,
            VisibilityTimeout = 30
        };

        var response = await _sqsClient.ReceiveMessageAsync(request);
        var result = new List<NewAssignmentEvent>();

        foreach (var message in response.Messages)
        {
            try
            {
                var assignmentEvent =
                    JsonSerializer.Deserialize<NewAssignmentEvent>(message.Body, _jsonOptions)
                    ?? throw new Exception("Deserialization returned null");


                result.Add(assignmentEvent);

                // Delete AFTER successful
                await _sqsClient.DeleteMessageAsync(
                    _queueUrl,
                    message.ReceiptHandle
                );
            }

            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to preocess message: {message.MessageId}: {ex.Message}");
            }
        }
        return result;
    }
}
