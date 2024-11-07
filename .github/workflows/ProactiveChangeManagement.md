# Proactive Change Management GitHub Workflow
This repository contains a GitHub Actions workflow for **Proactive Change Management**. This workflow is designed to streamline the DevOps and SRE change management process by integrating GitHub Actions with ServiceNow. It automates the management of configuration items (CIs), initiates standard change requests, and facilitates deployments on an Azure Kubernetes Service (AKS) cluster.
## Overview
This workflow automates:
- **CI Discovery and Creation**: Ensuring the microservice CI exists in ServiceNow CMDB.
- **Change Request Management**: Creating and progressing a ServiceNow change request through standard states (e.g., Scheduled, Implement, Review).
- **Deployment to AKS**: Deploying the specified microservice to an AKS cluster, with rollback on failure.
## Workflow Trigger
This workflow is triggered manually via `workflow_dispatch`, allowing you to input necessary parameters such as ServiceNow instance details, AKS configurations, and the microservice name.
## Inputs
The workflow requires the following inputs:
### ServiceNow Details
- `servicenow_instance` (required): ServiceNow instance URL.
- `change_model` (required): sys_id of the change model.
- `std_chg_prod_ver` (required): Version of the standard change producer.
- `assign_group` (required): Assignment group for the change request in ServiceNow.
### Microservice and AKS Details
- `microservice_name`: Name of the microservice.
- `RESOURCE_GROUP`: Azure Resource Group Name.
- `aks_cluster_name`: AKS Cluster Name.
- `aks_resource_group`: AKS Resource Group.
- `aks_credentials`: AKS credentials for deployment access.
## Workflow Jobs
The workflow consists of four main jobs:
### 1. Build Job
Builds and pushes the Docker image of the microservice to GitHub Container Registry.
- **Checkout Repository**: Clones the code repository.
- **Login to GitHub Container Registry**: Authenticates to GitHub Container Registry.
- **Build Docker Image**: Builds the Docker image using the microservice name as the tag.
- **Push Docker Image**: Publishes the Docker image.
### 2. CI-Management Job
Discovers or creates the microservice CI in ServiceNow.
- **Discover/Create CI in ServiceNow**: Checks if the CI exists; if not, creates it with details such as category and location.
- **Print CMDB Item Details**: Outputs the discovered or created CI details.
### 3. Change-Request Job
Creates and manages a change request in ServiceNow.
- **Retrieve First Member of Assignment Group**: Fetches the first member of the assignment group.
- **Create Change Request**: Creates a standard change request linked to the CI.
- **Change State Transitions**: Moves the change request through the states Scheduled and Implement.
- **Print Change Request Details**: Outputs the change request number and sys_id for reference.
### 4. Deploy Job
Deploys the microservice to the AKS cluster and manages deployment outcomes.
- **Deploy to AKS**: Uses the Docker image from GitHub Container Registry to deploy to AKS.
- **Rollback Deployment**: Initiates a rollback if the deployment fails.
- **Move Change Request to Review State**: Updates the change request to "Review" if the deployment is successful.
## Benefits
- **Automated Change Management**: Integrates GitHub Actions with ServiceNow for a streamlined change management workflow.
- **Consistency**: Standardized deployment process for DevOps and SRE teams.
- **Audit Trail**: Provides a clear history of changes and deployments in ServiceNow.
- **Rollback Mechanism**: Ensures resilience by rolling back failed deployments.
## Usage
To use this workflow:
1. Navigate to the **Actions** tab in your GitHub repository.
2. Select the **Proactive Change Management** workflow.
3. Click **Run workflow**, then provide the required parameters.
### Required GitHub Secrets
Ensure the following secrets are stored in GitHub:
- `SERVICENOW_USER`: Username for ServiceNow authentication.
- `SERVICENOW_PASSWORD`: Password for ServiceNow authentication.
- `GITHUB_TOKEN`: GitHub token for authenticating to GitHub Container Registry.
## Customization
- Modify input parameters such as `RESOURCE_GROUP`, `aks_cluster_name`, or `assign_group` as required.
- Update rollback commands to match your deployment and rollback configurations.
---
This workflow provides DevOps and SRE teams with a proactive and automated approach to managing change requests and deployments.
