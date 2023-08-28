variable "release_version" {
  type        = string
  description = "Docker Image version for the cloud run API instance"
  default     = "latest"
}

variable "zitadel_auth" {
  type        = string
  sensitive   = true
  description = "JSON Value of the private JWT key to authenticate against ZITADEL"
}
