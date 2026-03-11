output "app_url" {
  value = "https://${azurerm_app_service.app.default_site_hostname}"
}

output "resource_group_name" {
  value = azurerm_resource_group.cardex.name
}
