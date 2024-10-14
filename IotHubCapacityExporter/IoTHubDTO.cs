using Newtonsoft.Json;
using System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IoTHubCapacity
{
    public class IoTHubDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string Etag { get; set; }
        public Properties Properties { get; set; }
        public Sku Sku { get; set; }
        public Identity Identity { get; set; }
        public SystemData SystemData { get; set; }
    }

    public class Properties
    {
        public List<Location> Locations { get; set; }
        public string State { get; set; }
        public string ProvisioningState { get; set; }
        public List<object> IpFilterRules { get; set; }
        public string HostName { get; set; }
        public EventHubEndpoints EventHubEndpoints { get; set; }
        public Routing Routing { get; set; }
        public StorageEndpoints StorageEndpoints { get; set; }
        public MessagingEndpoints MessagingEndpoints { get; set; }
        public bool EnableFileUploadNotifications { get; set; }
        public CloudToDevice CloudToDevice { get; set; }
        public string Features { get; set; }
        public string MinTlsVersion { get; set; }
        public List<PrivateEndpointConnection> PrivateEndpointConnections { get; set; }
        public bool DisableLocalAuth { get; set; }
        public List<object> AllowedFqdnList { get; set; }
        public bool EnableDataResidency { get; set; }
    }

    public class Location
    {
        public string LocationName { get; set; }
        public string Role { get; set; }
    }

    public class EventHubEndpoints
    {
        public Events Events { get; set; }
    }

    public class Events
    {
        public int RetentionTimeInDays { get; set; }
        public int PartitionCount { get; set; }
        public List<string> PartitionIds { get; set; }
        public string Path { get; set; }
        public string Endpoint { get; set; }
    }

    public class Routing
    {
        public Endpoints Endpoints { get; set; }
        public List<object> Routes { get; set; }
        public FallbackRoute FallbackRoute { get; set; }
    }

    public class Endpoints
    {
        public List<object> ServiceBusQueues { get; set; }
        public List<object> ServiceBusTopics { get; set; }
        public List<object> EventHubs { get; set; }
        public List<object> StorageContainers { get; set; }
        public List<object> CosmosDBSqlContainers { get; set; }
    }

    public class FallbackRoute
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Condition { get; set; }
        public List<string> EndpointNames { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class StorageEndpoints
    {
        [JsonProperty("$default")]
        public Default Default { get; set; }
    }

    public class Default
    {
        public string SasTtlAsIso8601 { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }

    public class MessagingEndpoints
    {
        public FileNotifications FileNotifications { get; set; }
    }

    public class FileNotifications
    {
        public string LockDurationAsIso8601 { get; set; }
        public string TtlAsIso8601 { get; set; }
        public int MaxDeliveryCount { get; set; }
    }

    public class CloudToDevice
    {
        public int MaxDeliveryCount { get; set; }
        public string DefaultTtlAsIso8601 { get; set; }
        public Feedback Feedback { get; set; }
    }

    public class Feedback
    {
        public string LockDurationAsIso8601 { get; set; }
        public string TtlAsIso8601 { get; set; }
        public int MaxDeliveryCount { get; set; }
    }

    public class PrivateEndpointConnection
    {
        public PrivateEndpointConnectionProperties Properties { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class PrivateEndpointConnectionProperties
    {
        public PrivateEndpoint PrivateEndpoint { get; set; }
        public PrivateLinkServiceConnectionState PrivateLinkServiceConnectionState { get; set; }
    }

    public class PrivateEndpoint
    {
        public string Id { get; set; }
    }

    public class PrivateLinkServiceConnectionState
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public string ActionsRequired { get; set; }
    }

    public class Sku
    {
        public string Name { get; set; }
        public string Tier { get; set; }
        public int Capacity { get; set; }
    }

    public class Identity
    {
        public string Type { get; set; }
    }

    public class SystemData
    {
        public DateTime CreatedAt { get; set; }
    }

}