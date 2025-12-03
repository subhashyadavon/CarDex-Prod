# Azure Deployment Guide

This guide explains how to deploy the CarDex application to Azure Web App for Containers using the created GitHub Actions workflow.

## Prerequisites

1.  **Azure Account**: You need an active Azure subscription.
2.  **Azure CLI** (Optional but recommended): For easier resource creation.

## Step 1: Create Azure Web App for Containers

1.  Log in to the [Azure Portal](https://portal.azure.com).
2.  Search for **"App Services"** and click **"Create"** -> **"Web App"**.
3.  **Basics Tab**:
    *   **Subscription**: Select your subscription.
    *   **Resource Group**: Create a new one (e.g., `cardex-rg`) or select an existing one.
    *   **Name**: Enter a unique name (e.g., `cardex-app`). **Note this name**.
    *   **Publish**: Select **"Docker Container"**.
    *   **Operating System**: Select **"Linux"**.
    *   **Region**: Select a region close to you.
    *   **Pricing Plan**: Select a plan (e.g., `B1` or `F1` for testing, though `F1` might have limitations with multi-container).
4.  **Docker Tab**:
    *   **Options**: Select **"Docker Compose (Preview)"**.
    *   **Configuration File**: `docker-compose.prod.yml`
    *   **Image Source**: Select **"Docker Hub"** (even though we use GHCR, we will override this with our deployment).
    *   **Access Type**: Public.
    *   **Command**: Leave empty.
5.  Click **"Review + create"** and then **"Create"**.

## Step 2: Configure Environment Variables

1.  Go to your newly created Web App in the Azure Portal.
2.  In the left menu, under **"Settings"**, click **"Environment variables"**.
3.  Add the following **App settings**:
    *   `SUPABASE_CONNECTION_STRING`: Your Supabase connection string.
    *   `JWT_SECRET_KEY`: Your JWT secret key.
    *   `WEBSITES_PORT`: `80` (This tells Azure which port to route traffic to).
4.  Click **"Apply"**.

## Step 3: Get Publish Profile

1.  In the Web App overview page (or top menu), click **"Get publish profile"**.
2.  This will download a file named something like `cardex-app.PublishSettings`.
3.  Open this file with a text editor and copy the **entire content**.

## Step 4: Configure GitHub Secrets

1.  Go to your GitHub repository.
2.  Click **"Settings"** -> **"Secrets and variables"** -> **"Actions"**.
3.  Click **"New repository secret"**.
4.  **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
5.  **Secret**: Paste the content of the publish profile file you copied in Step 3.
6.  Click **"Add secret"**.

## Step 5: Update Workflow (If needed)

1.  Open `.github/workflows/azure-deploy.yml`.
2.  Ensure the `app-name` matches the name you created in Step 1 (default is `cardex-app`).

## Step 6: Deploy

You can trigger the deployment in two ways:

1.  **Manual**: Go to the **"Actions"** tab in GitHub, select **"Deploy to Azure"**, and click **"Run workflow"**.
2.  **Automatic**: The workflow is configured to run automatically after the `Backend Docker Build` and `Frontend Docker Build` workflows complete successfully.

## Troubleshooting

*   **Logs**: You can view the container logs in the Azure Portal under **"Deployment Center"** -> **"Logs"** or **"Log Stream"**.
*   **Startup Failure**: If the app fails to start, check the `Log Stream` to see if there are database connection errors or missing environment variables.
