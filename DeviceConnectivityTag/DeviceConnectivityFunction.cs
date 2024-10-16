using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Devices.Shared;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DeviceConnectivity
{
    public static class DeviceConnectivityFunction
    {
        private static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("IOT_HUB_CONNECTION_STRING"));
        private static string dynatraceUrl = Environment.GetEnvironmentVariable("DYNATRACE_URL"); // Replace with your Dynatrace environment
        private static string dynatraceApiToken = Environment.GetEnvironmentVariable("DYNATRACE_TOKEN");

        [FunctionName("DeviceConnectivityFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function executed at: {DateTime.Now}");
            var deviceIds = await GetDeviceIdsAsync();

            foreach (var deviceId in deviceIds)
            {
                var twin = await GetDeviceConnectivityAsync(deviceId);
                log.LogInformation($"Device {deviceId} connectivity status: {twin.ConnectionState.ToString()}");
                await SendConnectivityToDynatrace(deviceId, twin, log);
            }

            return new OkObjectResult("Device connectivity data processed successfully.");
        }

        private static async Task<List<string>> GetDeviceIdsAsync()
        {
            var deviceIds = new List<string>();
            var query = registryManager.CreateQuery("SELECT deviceId FROM devices", 100);

            while (query.HasMoreResults)
            {
                var jsonResults = await query.GetNextAsJsonAsync();
                foreach (var json in jsonResults)
                {
                    var twin = JsonConvert.DeserializeObject<Twin>(json);
                    deviceIds.Add(twin.DeviceId);
                }
            }

            return deviceIds;
        }

        private static async Task<Twin> GetDeviceConnectivityAsync(string deviceId)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                CheckAdditionalContent = false
            };
            var twin = await registryManager.GetTwinAsync(deviceId);
            twin.Tags["D2C-CONNECTION-STATUS"] = twin.ConnectionState.ToString();
            await registryManager.UpdateTwinAsync(deviceId, twin, twin.ETag);
            return twin;
        }

        private static async Task SendConnectivityToDynatrace(string deviceId, Twin twin, ILogger log)
        {
            string metricKey = "custom.device.connectivity";
            bool metadataPosted = await PostMetricMetadataAsync(metricKey, log, twin);
            if (metadataPosted)
            {
                int metricValue = twin.ConnectionState.ToString().Equals("Connected", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                string sanitizedDeviceId = deviceId.Replace("-", "");
                string connectionState = twin.ConnectionState.ToString();
                string metricLine = $"custom.device.connectivity,deviceid={("uuid" + sanitizedDeviceId).Trim()},status={connectionState},tags={connectionState} gauge,{metricValue}";
                var client = new RestClient(dynatraceUrl);
                RestRequest request = new RestRequest("/api/v2/metrics/ingest", Method.Post);
                request.AddHeader("Authorization", $"Api-Token {dynatraceApiToken}");
                request.AddHeader("Content-Type", "text/plain; charset=utf-8");
                request.AddParameter("text/plain", metricLine, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    log.LogInformation($"Successfully sent connectivity data for {deviceId} to Dynatrace.");
                }
                else
                {
                    log.LogError($"Failed to send data to Dynatrace: {response.Content}");
                }
            }
        }

        private static async Task<bool> PostMetricMetadataAsync(string metricKey, ILogger log, Twin twin)
        {
            var url = $"{dynatraceUrl}/api/v2/settings/objects";
            var client = new HttpClient();
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
            ""displayName"": ""Device Connectivity"",
            ""tags"": [""{"D2C-" + twin.ConnectionState.ToString()}""],
            ""unit"": ""None""
        }}
    }}]";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                return false;
            }
        }
    }

}
