# Reactive Change Management GitHub Workflow
This repository contains a **Reactive Change Management** GitHub Actions workflow. It is designed to enhance DevOps and SRE processes by enabling reactive change management through integration with ServiceNow and Azure. The workflow automates change analysis, deployment logging, activity monitoring, and configuration item (CI) management, creating a comprehensive approach to tracking and documenting changes in Azure environments.
## Overview
The workflow performs the following actions:
- **Change Analysis**: Uses Azure Monitor to perform a change analysis for a specified time range.
- **Deployment and Activity Logging**: Retrieves Azure App Service deployment and activity logs.
- **CI Discovery and Creation**: Ensures that relevant configuration items exist in ServiceNow, creating new ones if needed.
- **Change Request Management**: Automatically creates a change request in ServiceNow and manages it through states such as Scheduled, Implement, and Review.
## Workflow Trigger
The workflow is triggered manually through `workflow_dispatch`, which requires inputs such as Azure credentials, ServiceNow details, and time range for change analysis.
## Inputs
The following inputs are required to trigger the workflow:
### Azure Details
- `azure_client_id`: Azure Client ID (required).
- `azure_client_secret`: Azure Client Secret (required).
- `azure_tenant_id`: Azure Tenant ID (required).
- `azure_subscription_id`: Azure Subscription ID (required).
### ServiceNow Details
- `servicenow_instance`: ServiceNow instance URL (required).
- `change_model`: Sys_id of the change model (required).
- `std_chg_prod_ver`: Version of the standard change producer (required).
- `assign_group`: Assignment group for the change request in ServiceNow (required).
### Change Analysis Time Range
- `start_time`: Start time for change analysis (format: HH:MM:SS).
- `end_time`: End time for change analysis (format: HH:MM:SS).
## Workflow Jobs
The workflow consists of three main jobs:
### 1. Azure Change Analysis Job
Analyzes changes in Azure for the specified time range, retrieves deployment and activity logs, and maps Azure resources to ServiceNow CIs.
- **Setup Azure CLI**: Logs into Azure using provided credentials.
- **Change Analysis**: Retrieves changes in the subscription for the specified time range and parses the results.
- **Deployment Logs**: Fetches and parses Azure App Service deployment logs.
- **Activity Logs**: Retrieves Azure activity logs for App Service deployments.
- **Combine Logs**: Merges change analysis, deployment logs, and activity logs for a comprehensive view.
- **Map Azure Resources to ServiceNow CIs**: Maps resources and app versions to configuration items in ServiceNow.
### 2. CI Management in ServiceNow
Discovers or creates the relevant CI in ServiceNow based on the mapped Azure resources.
- **Discover/Create CI**: Checks if the CI exists in ServiceNow; if not, creates it with relevant details.
- **Print CMDB Item**: Outputs the CI details (name and sys_id) for reference.
### 3. Change Request Management
Creates and manages a change request in ServiceNow based on the gathered change analysis and log data.
- **Retrieve Assignment Group Member**: Identifies the first member of the assignment group for the change request.
- **Create Change Request**: Initiates a change request in ServiceNow and includes detailed change information.
- **Change Request State Transitions**: Progresses the change request through states such as Scheduled, Implement, and Review.
- **Print Change Request Details**: Outputs the change request number and sys_id for tracking.
## Benefits
- **Enhanced Reactive Change Tracking**: Automates the tracking of reactive changes using Azure Monitor and ServiceNow.
- **Streamlined CI and Change Request Management**: Integrates with ServiceNow for automatic CI discovery and change request creation.
- **Comprehensive Audit Trail**: Merges change analysis, deployment, and activity logs into a single, easy-to-track output.
- **Automated State Management**: Progresses the change request through defined states, ensuring complete visibility.
## Usage
To use this workflow:
1. Navigate to the **Actions** tab in your GitHub repository.
2. Select the **Reactive Change Management** workflow.
3. Click **Run workflow**, then provide the required parameters.
### Required GitHub Secrets
Ensure the following secrets are stored securely in GitHub:
- `SERVICENOW_USER`: Username for ServiceNow authentication.
- `SERVICENOW_PASSWORD`: Password for ServiceNow authentication.
- `GITHUB_TOKEN`: GitHub token for accessing GitHub resources.
- `AZURE_CREDENTIALS`: JSON object containing Azure credentials for login.
## Customization
- Modify the time range, resource groups, and assignment group settings based on your organizational needs.
- Adjust rollback or CI mapping logic as required for specific deployment configurations.
---
This workflow provides a reactive and automated approach to change management, integrating Azure monitoring data with ServiceNow to enhance DevOps and SRE visibility.
has context menu
