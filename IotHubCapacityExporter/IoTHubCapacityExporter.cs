using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace IoTHubCapacity
{

    public static class IoTHubCapacityExporter
    {
        private static string dynatraceUrl = Environment.GetEnvironmentVariable("DYNATRACE_URL"); // Replace with your Dynatrace environment
        private static string dynatraceApiToken = Environment.GetEnvironmentVariable("DYNATRACE_TOKEN");
        private static readonly string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
        private static readonly string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        private static readonly string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
        private static readonly string subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        private static readonly HttpClient client = new HttpClient();
        

        [FunctionName("IoTHubCapacityExporter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Authenticate with Azure
            // string accessToken = await GetAzureAccessTokenAsync();
            string accessToken = req.Headers["ACCESS_TOKEN"];
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return new BadRequestObjectResult("ACCESS_TOKEN is missing");
            }

            // Get IoT Hub capacity metrics
            var capacityMetrics = await GetIoTHubCapacityMetricsAsync(accessToken, log);

            // Post capacity metrics to Dynatrace
            await PostCapacityMetricsToDynatrace(capacityMetrics, log);

            return new OkObjectResult("IoTHubCapacityExporter function executed successfully.");
        }

        private static async Task<string> GetAzureAccessTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            string[] scopes = { "https://management.azure.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }

        private static async Task<Dictionary<string, Dictionary<string, int>>> GetIoTHubCapacityMetricsAsync(string accessToken, ILogger log)
        {
            var capacityMetrics = new Dictionary<string, Dictionary<string, int>>();

            // List all IoT Hubs in the subscription
            var iotHubs = await ListIoTHubsAsync(accessToken);

            foreach (var iotHub in iotHubs)
            {
                var iotHubName = iotHub["name"].ToString();
                var resourceGroupName = iotHub["resourcegroup"].ToString();
                log.LogInformation($"Fetching capacity metrics for IoT Hub: {iotHubName}");

                var requestUrl = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Devices/IotHubs/{iotHubName}?api-version=2023-06-30";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                IoTHubDTO data = JsonConvert.DeserializeObject<IoTHubDTO>(responseBody);


                var iotHubMetrics = new Dictionary<string, int>();
                
                string name = data.Name;
                int dailyMessageLimit = GetDailyMessageLimit(data);

                int currentValue = dailyMessageLimit;
                iotHubMetrics[name] = currentValue;

                capacityMetrics[iotHubName] = iotHubMetrics;
            }

            return capacityMetrics;
        }

        private static async Task<List<dynamic>> ListIoTHubsAsync(string accessToken)
        {
            var iotHubs = new List<dynamic>();

            var requestUrl = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/ETN-IT-DSET-DEV-INFRA-RG/providers/Microsoft.Devices/IotHubs?api-version=2023-06-30";            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(responseBody);

            foreach (var item in data.value)
            {
                iotHubs.Add(item);
            }

            return iotHubs;
        }

        private static async Task PostCapacityMetricsToDynatrace(Dictionary<string, Dictionary<string, int>> capacityMetrics, ILogger log)
        {
            string accessToken = await GetAzureAccessTokenAsync();
            var subscriptionName = await GetSubscriptionNameAsync(subscriptionId, accessToken);
            // Remove spaces and allow only alphanumeric characters
            subscriptionName = Regex.Replace(subscriptionName, @"[^a-zA-Z0-9]", "").ToLower();
            


            foreach (var iotHub in capacityMetrics)
            {
                string iotHubName = iotHub.Key;
                var metrics = iotHub.Value;
                int capacity = 0;

                foreach (var metric in metrics)
                {
                    string metricName = metric.Key;
                    capacity = metric.Value;

                    // Now you have the metric name and its integer value
                    Console.WriteLine($"IoT Hub: {iotHubName}, Metric: {metricName}, Value: {capacity}");
                }
                iotHubName = Regex.Replace(iotHubName, @"[^a-zA-Z0-9]", "").ToLower();
                string metricKey = $"{subscriptionName}.{iotHubName}.{capacity}";
                bool metadataPosted = await PostMetricMetadataAsync(metricKey, log);
                if (metadataPosted)
                {
                    foreach (var metric in iotHub.Value)
                    {
                        string metricLine = $"{metricKey},metric={metric.Key} gauge,{metric.Value}";
                        var client = new RestClient(dynatraceUrl);
                        RestRequest request = new RestRequest("/api/v2/metrics/ingest", Method.Post);
                        request.AddHeader("Authorization", $"Api-Token {dynatraceApiToken}");
                        request.AddHeader("Content-Type", "text/plain; charset=utf-8");
                        request.AddParameter("text/plain", metricLine, ParameterType.RequestBody);
                        var response = await client.ExecuteAsync(request);
                        if (response.IsSuccessful)
                        {
                            log.LogInformation($"Successfully sent capacity metric {metric.Key} for {iotHubName} to Dynatrace.");
                        }
                        else
                        {
                            log.LogError($"Failed to send data to Dynatrace: {response.Content}");
                        }
                    }
                }
            }
        }

        private static async Task<bool> PostMetricMetadataAsync(string metricKey, ILogger log)
        {
            var url = $"{dynatraceUrl}/api/v2/settings/objects";

            // Check if the Authorization header already exists
            if (!client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Api-Token {dynatraceApiToken}");
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = $@"
[{{
    ""schemaId"": ""builtin:metric.metadata"",
    ""scope"": ""metric-{metricKey}"",
    ""value"": {{
        ""displayName"": ""IoT Hub Capacity"",
        ""tags"": [""IoTHub-Capacity-{metricKey}""],
        ""unit"": ""None""
    }}
}}]";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
        private static int GetDailyMessageLimit(IoTHubDTO iotHub)
        {
            int result = 0;
            switch (iotHub.Sku.Tier)
            {
                case "Free":
                    return 8000; // Free tier has a fixed limit
                case "Basic":
                    if (int.TryParse(Environment.GetEnvironmentVariable("IOT_BASIC_TIER"), out int basicTierLimit))
                    {
                        return iotHub.Sku.Capacity * basicTierLimit; // Use the parsed value
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid IOT_BASIC_TIER environment variable");
                    }
                case "Standard":
                    if (int.TryParse(Environment.GetEnvironmentVariable("IOT_STD_TIER"), out int stdTierLimit))
                    {
                        return iotHub.Sku.Capacity * stdTierLimit; // Use the parsed value
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid IOT_STD_TIER environment variable");
                    }
                default:
                    throw new InvalidOperationException("Unknown SKU tier");
            }
        }

        public static async Task<string> GetSubscriptionNameAsync(string subscriptionId, string accessToken)
        {
            var requestUrl = $"https://management.azure.com/subscriptions/{subscriptionId}?api-version=2020-01-01";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            return json["displayName"].ToString();
        }
    }
}
