terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "cardex" {
  name     = "rg-cardex-prod"
  location = var.location
}

resource "azurerm_service_plan" "plan" {
  name                = "plan-cardex-prod"
  resource_group_name = azurerm_resource_group.cardex.name
  location            = azurerm_resource_group.cardex.location
  os_type             = "Linux"
  sku_name            = "S1"
}

resource "azurerm_app_service" "app" {
  name                = "app-cardex-prod-${var.unique_id}"
  resource_group_name = azurerm_resource_group.cardex.name
  location            = azurerm_resource_group.cardex.location
  app_service_plan_id = azurerm_service_plan.plan.id

  site_config {
    always_on        = true
    linux_fx_version = "COMPOSE|${filebase64("/Users/subhashyadav/Desktop/forked/CarDex/docker-compose.prod.yml")}"
  }

  app_settings = {
    "DOCKER_REGISTRY_SERVER_URL"          = "https://ghcr.io"
    "DOCKER_REGISTRY_SERVER_USERNAME"     = var.github_username
    "DOCKER_REGISTRY_SERVER_PASSWORD"     = var.github_pat
    "SUPABASE_CONNECTION_STRING"          = var.supabase_connection_string
    "JWT_SECRET_KEY"                      = var.jwt_secret_key
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
  }
}
