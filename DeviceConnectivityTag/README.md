# Device Connectivity Function

This Azure Function monitors device connectivity status and sends the data to Dynatrace.

## Prerequisites

- Azure IoT Hub
- Dynatrace account
- Azure Function App

## Environment Variables

The following environment variables need to be set for the function to work correctly:

- `IOT_HUB_CONNECTION_STRING`: Connection string for the Azure IoT Hub.
- `DYNATRACE_URL`: URL for the Dynatrace API.
- `DYNATRACE_TOKEN`: API token for Dynatrace.

### Storing Secrets in Azure Key Vault

It is recommended to store sensitive information such as `DYNATRACE_TOKEN`, `AZURE_CLIENT_SECRET`, and other secrets in Azure Key Vault. You can then configure your Azure Function to access these secrets securely.

## Functionality

The function performs the following tasks:

1. **Retrieve Device IDs**: Queries the IoT Hub to get a list of device IDs.
2. **Get Device Connectivity**: Retrieves the connectivity status of each device.
3. **Send Data to Dynatrace**: Sends the connectivity status to Dynatrace.

## Code Overview

### DeviceConnectivityFunction.cs

The `DeviceConnectivityFunction` class contains the main logic for the Azure Function. Below is a detailed description of its components:

#### Environment Variables

- `iotHubConnectionString`: Connection string for the Azure IoT Hub.
- `registryManager`: Instance of `RegistryManager` to interact with the IoT Hub.
- `dynatraceUrl`: URL for the Dynatrace API.
- `dynatraceApiToken`: API token for Dynatrace.

#### Function Method

This method is triggered by an HTTP request. It performs the following steps:
1. Logs the function execution.
2. Checks if the IoT Hub connection string is set.
3. Retrieves the list of device IDs.
4. For each device ID, retrieves the connectivity status and sends it to Dynatrace.
5. Returns a success message.

#### Helper Methods

- `GetDeviceIdsAsync()`: Retrieves the list of device IDs from the IoT Hub.
- `GetDeviceConnectivityAsync(string deviceId)`: Retrieves the connectivity status of a specific device.
- `SendConnectivityToDynatrace(string deviceId, Twin twin, ILogger log)`: Sends the connectivity status to Dynatrace.
- `PostMetricMetadataAsync(string metricKey, ILogger log, Twin twin)`: Posts metric metadata to Dynatrace.

## Running the Function

To run the function locally, use the following command:
func start


Ensure that the required environment variables are set in your local environment.

## Deployment

To deploy the function to Azure, follow these steps:

1. Create a Function App in the Azure portal.
2. Set the required environment variables in the Function App settings.
3. Deploy the function code to the Function App.
