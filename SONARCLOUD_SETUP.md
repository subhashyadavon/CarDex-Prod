# SonarCloud Setup Guide

This guide will walk you through setting up SonarCloud for the CarDex project.

## Prerequisites

- GitHub account with access to the CarDex repository
- Repository must be public (or you need a SonarCloud paid plan for private repos)

## Step-by-Step Setup

### Step 1: Create SonarCloud Account

1. Go to [https://sonarcloud.io](https://sonarcloud.io)
2. Click **"Log in"** in the top right
3. Select **"Log in with GitHub"**
4. Authorize SonarCloud to access your GitHub account

### Step 2: Import Your Repository

1. After logging in, click the **"+"** button in the top right
2. Select **"Analyze new project"**
3. You'll see a list of your GitHub organizations
4. Select the organization that contains the CarDex repository
5. If this is your first time:
   - Click **"Install SonarCloud"** on GitHub
   - Select which repositories SonarCloud can access
   - Choose **"Only select repositories"** and select **CarDex**
   - Click **"Install"**
6. Back on SonarCloud, you should now see the CarDex repository
7. Check the box next to **CarDex**
8. Click **"Set Up"**

### Step 3: Configure Analysis Method

1. SonarCloud will ask how you want to analyze your project
2. Select **"With GitHub Actions"** (recommended)
3. SonarCloud will show you instructions, but we've already created the workflow!

### Step 4: Create Projects for Backend and Frontend

Since we have separate jobs for backend and frontend, we need to create two projects:

#### Create Backend Project

1. In SonarCloud, click **"+"** → **"Create new project"**
2. Select **"Manually"**
3. Fill in the details:
   - **Project key**: `cardex-backend` (must match the workflow)
   - **Organization**: Your organization name
   - **Display name**: CarDex Backend
4. Click **"Set Up"**
5. Choose **"With GitHub Actions"**
6. You'll see a token - **SAVE THIS TOKEN** (you'll need it in Step 5)

#### Create Frontend Project

1. Click **"+"** → **"Create new project"** again
2. Select **"Manually"**
3. Fill in the details:
   - **Project key**: `cardex-frontend` (must match the workflow)
   - **Organization**: Your organization name
   - **Display name**: CarDex Frontend
4. Click **"Set Up"**
5. Choose **"With GitHub Actions"**
6. You can use the same token from the backend project

### Step 5: Add Secrets to GitHub Repository

Now you need to add the SonarCloud credentials to your GitHub repository:

1. Go to your GitHub repository: [https://github.com/subhashyadavon/CarDex](https://github.com/subhashyadavon/CarDex)
2. Click **"Settings"** (top menu)
3. In the left sidebar, click **"Secrets and variables"** → **"Actions"**
4. Click **"New repository secret"**

#### Add SONAR_TOKEN

1. **Name**: `SONAR_TOKEN`
2. **Secret**: Paste the token you saved from Step 4
3. Click **"Add secret"**

#### Add SONAR_HOST_URL

1. Click **"New repository secret"** again
2. **Name**: `SONAR_HOST_URL`
3. **Secret**: `https://sonarcloud.io`
4. Click **"Add secret"**

#### Add SONAR_ORGANIZATION

1. Click **"New repository secret"** again
2. **Name**: `SONAR_ORGANIZATION`
3. **Secret**: Your SonarCloud organization key
4. Click **"Add secret"**

**To find your organization key:**
- Go to SonarCloud
- Click on your organization name (top left)
- The organization key is shown in the URL: `https://sonarcloud.io/organizations/YOUR-ORG-KEY`
- Or go to **My Organizations** and copy the key from there

### Step 6: Trigger the Analysis

Now that everything is set up, trigger the workflow:

1. Make a small change to your code (or just trigger manually)
2. Commit and push:
   ```bash
   git add .
   git commit -m "Trigger SonarCloud analysis"
   git push
   ```
3. Go to GitHub → **Actions** tab
4. You should see the **"SonarQube Analysis"** workflow running
5. Wait for it to complete

### Step 7: View Results in SonarCloud

1. Go back to [https://sonarcloud.io](https://sonarcloud.io)
2. You should see both projects:
   - **CarDex Backend**
   - **CarDex Frontend**
3. Click on each to view:
   - Code coverage
   - Bugs and vulnerabilities
   - Code smells
   - Security hotspots
   - Quality gate status

## Troubleshooting

### "Project not found" Error

- Make sure the project keys in the workflow (`cardex-backend` and `cardex-frontend`) exactly match the project keys in SonarCloud
- Check that you've created both projects

### "Invalid token" Error

- Verify the `SONAR_TOKEN` secret is correctly set in GitHub
- Make sure you copied the entire token without extra spaces
- Try regenerating the token in SonarCloud (My Account → Security → Generate Token)

### "Organization not found" Error

- Make sure you've added the organization key to the workflow (Step 6)
- Verify the organization key matches your SonarCloud organization

### Analysis Not Running

- Check that the workflow file is in `.github/workflows/sonarqube.yml`
- Verify the secrets are added to the repository (not your personal account)
- Check the Actions tab for any error messages

## Next Steps

Once set up, SonarCloud will:

✅ Automatically analyze code on every push  
✅ Analyze pull requests and add comments  
✅ Track code quality over time  
✅ Show quality gate status in GitHub  
✅ Send notifications for new issues  

You can customize:
- Quality gates (what passes/fails)
- New code definition
- Issue severity
- Notification settings

## Useful Links

- [SonarCloud Dashboard](https://sonarcloud.io)
- [SonarCloud Documentation](https://docs.sonarcloud.io)
- [Quality Gates](https://docs.sonarcloud.io/improving/quality-gates/)
- [Pull Request Decoration](https://docs.sonarcloud.io/enriching/pull-request-analysis/)
