variable "location" {
  description = "The Azure region to deploy to."
  type        = string
  default     = "East US"
}

variable "unique_id" {
  description = "A unique identifier to avoid name collisions."
  type        = string
  default     = "prod"
}

variable "github_username" {
  description = "GitHub username for GHCR."
  type        = string
}

variable "github_pat" {
  description = "GitHub Personal Access Token with read:packages scope."
  type        = string
  sensitive   = true
}

variable "supabase_connection_string" {
  description = "Connection string for the Supabase database."
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "Secret key for JWT authentication."
  type        = string
  sensitive   = true
}
