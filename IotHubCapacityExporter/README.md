# IoTHubCapacityExporter

## Overview

The `IoTHubCapacityExporter` is an Azure Function that retrieves IoT Hub capacity metrics and posts them to Dynatrace for monitoring. This function is designed to run in the In-Process model of Azure Functions.

## Features

- Retrieves IoT Hub capacity metrics from Azure.
- Posts capacity metrics to Dynatrace.
- Validates Azure AD tokens to ensure secure access.

## Prerequisites

- Azure Subscription
- Dynatrace Account
- Azure Function App
- Azure AD App Registration

## Environment Variables

The function relies on the following environment variables:

- `DYNATRACE_URL`: The URL of your Dynatrace environment.
- `DYNATRACE_TOKEN`: The API token for accessing Dynatrace.
- `AZURE_TENANT_ID`: The Azure AD tenant ID.
- `AZURE_CLIENT_ID`: The client ID of the Azure AD app registration.
- `AZURE_CLIENT_SECRET`: The client secret of the Azure AD app registration.
- `AZURE_SUBSCRIPTION_ID`: The Azure subscription ID.

### Storing Secrets in Azure Key Vault

It is recommended to store sensitive information such as `DYNATRACE_TOKEN`, `AZURE_CLIENT_SECRET`, and other secrets in Azure Key Vault. You can then configure your Azure Function to access these secrets securely.

## Setup

1. **Clone the repository:**

    git clone https://github.com/your-repo/IoTHubCapacityExporter.git
    cd IoTHubCapacityExporter

2. **Configure environment variables:**

    Set the required environment variables in the Azure Function App settings.

3. **Deploy the Azure Function:**

    Deploy the function to your Azure Function App using your preferred method (Azure CLI, Visual Studio, etc.).

## Usage

### Triggering the Function

The function can be triggered via HTTP GET or POST requests. Ensure that the request includes a valid Azure AD token in the `Authorization` header.

Example:
curl -X GET "https://IoTHubCapacityExporter.azurewebsites.net/api/IoTHubCapacityExporter" -H "Authorization: Bearer "


### GitHub Actions Workflow

A sample GitHub Actions workflow is provided to automate the process of acquiring an Azure AD token and triggering the function.


## Function Details

### `IoTHubCapacityExporter`

- **Trigger**: HTTP GET/POST
- **Authorization**: Function level
- **Description**: Retrieves IoT Hub capacity metrics and posts them to Dynatrace.

### Methods

- `Run(HttpRequest req, ILogger log)`: Main function entry point.
- `ValidateTokenAsync(string token)`: Validates the Azure AD token.
- `GetIoTHubCapacityMetricsAsync(string accessToken, ILogger log)`: Retrieves IoT Hub capacity metrics.
- `ListIoTHubsAsync(string accessToken)`: Lists all IoT Hubs in the subscription.
- `PostCapacityMetricsToDynatrace(Dictionary<string, Dictionary<string, int>> capacityMetrics, ILogger log)`: Posts capacity metrics to Dynatrace.
- `PostMetricMetadataAsync(string metricKey, ILogger log)`: Posts metric metadata to Dynatrace.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes.


