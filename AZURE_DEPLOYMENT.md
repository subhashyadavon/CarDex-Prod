# Azure Deployment Setup Guide

This guide will walk you through deploying the CarDex application to Azure using Docker containers and GitHub Actions.

## Prerequisites

- Azure account ([Sign up for free](https://azure.microsoft.com/free/))
- GitHub account with access to the CarDex repository
- Docker images published to GitHub Container Registry (GHCR)

## Overview

The deployment uses:
- **Azure Web App for Containers** - Hosts the Docker containers
- **Docker Compose** - Orchestrates frontend and backend containers
- **GitHub Actions** - Automates deployment on push/manual trigger

## Step-by-Step Setup

### Step 1: Create Azure Account

1. Go to [https://portal.azure.com](https://portal.azure.com)
2. Sign in or create a new account
3. If new, you'll get $200 free credits for 30 days

### Step 2: Create a Resource Group

1. In the Azure Portal, search for **"Resource groups"**
2. Click **"+ Create"**
3. Fill in the details:
   - **Subscription**: Select your subscription
   - **Resource group**: `cardex-rg` (or your preferred name)
   - **Region**: Choose closest to your users (e.g., `East US`, `West Europe`)
4. Click **"Review + create"** → **"Create"**

### Step 3: Create Azure Web App for Containers

1. In the Azure Portal, search for **"Web App"**
2. Click **"+ Create"**
3. Fill in the **Basics** tab:
   - **Subscription**: Your subscription
   - **Resource Group**: `cardex-rg` (created in Step 2)
   - **Name**: `cardex-app` (must be globally unique - try `cardex-app-yourname`)
   - **Publish**: **Docker Container**
   - **Operating System**: **Linux**
   - **Region**: Same as resource group
   - **Pricing Plan**: 
     - Click **"Create new"**
     - Name: `cardex-plan`
     - **Sku and size**: Click **"Change size"**
     - For testing: Select **B1** (Basic, ~$13/month)
     - For production: Select **P1V2** or higher
     - Click **"Apply"**

4. Click **"Next: Docker"**

5. Fill in the **Docker** tab:
   - **Options**: **Docker Compose (Preview)**
   - **Image Source**: **Docker Hub or other registries**
   - **Access Type**: **Public**
   - **Configuration**: Upload or paste your `docker-compose.prod.yml` content
   
   > **Note**: You can skip this for now and configure it via GitHub Actions later

6. Click **"Review + create"** → **"Create"**
7. Wait for deployment to complete (2-3 minutes)
8. Click **"Go to resource"**

### Step 4: Configure Web App Settings

1. In your Web App, go to **"Configuration"** (left sidebar under Settings)
2. Click **"Application settings"** tab
3. Add the following settings (click **"+ New application setting"** for each):

   | Name | Value |
   |------|-------|
   | `WEBSITES_ENABLE_APP_SERVICE_STORAGE` | `false` |
   | `WEBSITES_PORT` | `80` |
   | `DOCKER_ENABLE_CI` | `true` |

4. Click **"Save"** at the top
5. Click **"Continue"** when prompted

### Step 5: Get Publish Profile

The publish profile contains credentials for GitHub Actions to deploy to Azure:

1. In your Web App overview page, click **"Get publish profile"** (top toolbar)
2. This downloads a `.PublishSettings` XML file
3. Open the file in a text editor
4. **Copy the entire contents** (you'll need this in Step 6)

### Step 6: Add Azure Secrets to GitHub

1. Go to your GitHub repository: [https://github.com/subhashyadavon/CarDex](https://github.com/subhashyadavon/CarDex)
2. Click **"Settings"** → **"Secrets and variables"** → **"Actions"**
3. Click **"New repository secret"**

#### Add AZURE_WEBAPP_PUBLISH_PROFILE

1. **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
2. **Secret**: Paste the entire contents of the publish profile file from Step 5
3. Click **"Add secret"**

### Step 7: Update Azure Deployment Workflow

Update the workflow with your actual Azure Web App name:

1. Open `.github/workflows/azure-deploy.yml`
2. Find line 24: `app-name: 'cardex-app'`
3. Replace `cardex-app` with your actual Web App name from Step 3
4. Save the file
5. Commit and push:
   ```bash
   git add .github/workflows/azure-deploy.yml
   git commit -m "Update Azure Web App name"
   git push
   ```

### Step 8: Configure Environment Variables in Azure

Your application needs database and JWT secrets. Add them to Azure:

1. In Azure Portal, go to your Web App
2. Click **"Configuration"** → **"Application settings"**
3. Add these settings:

   | Name | Value | Notes |
   |------|-------|-------|
   | `SUPABASE_CONNECTION_STRING` | Your Supabase connection string | From your Supabase project |
   | `JWT_SECRET_KEY` | Your JWT secret | Use a strong random string (32+ chars) |
   | `ASPNETCORE_ENVIRONMENT` | `Production` | Already in docker-compose |

4. Click **"Save"**

**To get your Supabase connection string:**
1. Go to [https://supabase.com](https://supabase.com)
2. Open your project
3. Go to **Settings** → **Database**
4. Copy the **Connection string** (URI format)
5. Replace `[YOUR-PASSWORD]` with your database password

### Step 9: Deploy to Azure

You have two options to deploy:

#### Option A: Manual Deployment (Recommended for first time)

1. Go to GitHub repository → **Actions** tab
2. Click **"Deploy to Azure"** workflow (left sidebar)
3. Click **"Run workflow"** dropdown
4. Click **"Run workflow"** button
5. Wait for the workflow to complete (5-10 minutes)

#### Option B: Automatic Deployment

The workflow automatically triggers when:
- Backend Docker Build workflow completes successfully
- Frontend Docker Build workflow completes successfully

So just push code changes and it will auto-deploy!

### Step 10: Verify Deployment

1. Go to Azure Portal → Your Web App
2. Click **"Browse"** (top toolbar)
3. Your app should open in a new tab
4. If you see errors, check:
   - **Logs**: Web App → **Log stream** (left sidebar)
   - **Container logs**: Web App → **Container settings** → **Logs**

### Step 11: Configure Custom Domain (Optional)

1. In your Web App, go to **"Custom domains"**
2. Click **"+ Add custom domain"**
3. Follow the instructions to add your domain
4. Configure DNS records with your domain provider
5. Enable HTTPS with a free managed certificate

## Troubleshooting

### Deployment Fails

**Check GitHub Actions logs:**
1. Go to GitHub → Actions tab
2. Click on the failed workflow run
3. Expand the failed step to see error details

**Common issues:**
- Invalid publish profile → Re-download from Azure (Step 5)
- Wrong app name → Update workflow file (Step 7)
- Missing secrets → Verify in GitHub Settings (Step 6)

### Application Not Starting

**Check Azure logs:**
1. Azure Portal → Your Web App
2. **Log stream** (left sidebar)
3. Look for error messages

**Common issues:**
- Missing environment variables → Add in Configuration (Step 8)
- Database connection failed → Check Supabase connection string
- Port mismatch → Verify `WEBSITES_PORT` is set to `80`

### Container Pull Failed

**Issue**: Azure can't pull Docker images from GHCR

**Solution**:
1. Verify images are public in GitHub
2. Go to GitHub → Packages
3. Click on each package (cardex-frontend, cardex-backend)
4. **Package settings** → **Change visibility** → **Public**

### 502 Bad Gateway

**Possible causes:**
- Container is starting (wait 2-3 minutes)
- Application crashed (check logs)
- Port configuration wrong (verify WEBSITES_PORT)

## Monitoring and Maintenance

### View Application Logs

1. Azure Portal → Your Web App
2. **Log stream** → View real-time logs
3. **Diagnose and solve problems** → Advanced tools

### Scale Your Application

1. Azure Portal → Your Web App
2. **Scale up (App Service plan)** → Choose bigger tier
3. **Scale out (App Service plan)** → Add more instances

### Set Up Alerts

1. Azure Portal → Your Web App
2. **Alerts** → **+ Create** → **Alert rule**
3. Configure alerts for:
   - High CPU usage
   - High memory usage
   - HTTP errors (5xx)
   - Response time

### Enable Application Insights (Recommended)

1. Azure Portal → Your Web App
2. **Application Insights** → **Turn on Application Insights**
3. Create new or use existing Application Insights resource
4. Get detailed telemetry, performance metrics, and error tracking

## Cost Optimization

### Development/Testing
- Use **B1 Basic** tier (~$13/month)
- Stop the app when not in use (saves costs)

### Production
- Use **P1V2 Premium** tier (~$96/month)
- Includes auto-scaling, custom domains, SSL
- Better performance and reliability

### Free Tier Option
- Azure offers **F1 Free** tier
- Limited to 60 CPU minutes/day
- Good for demos, not production

## Useful Commands

### View deployment status
```bash
# In GitHub Actions tab
# Or use Azure CLI:
az webapp show --name cardex-app --resource-group cardex-rg
```

### Restart Web App
```bash
az webapp restart --name cardex-app --resource-group cardex-rg
```

### View logs
```bash
az webapp log tail --name cardex-app --resource-group cardex-rg
```

## Next Steps

✅ Set up custom domain  
✅ Enable Application Insights  
✅ Configure auto-scaling  
✅ Set up staging slots for zero-downtime deployments  
✅ Configure backup and disaster recovery  

## Useful Links

- [Azure Portal](https://portal.azure.com)
- [Azure Web Apps Documentation](https://docs.microsoft.com/azure/app-service/)
- [Docker Compose on Azure](https://docs.microsoft.com/azure/app-service/configure-custom-container)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
