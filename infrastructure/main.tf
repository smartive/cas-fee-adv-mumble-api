locals {
  name             = "mumble-api"
  gcp_region       = "europe-west6"
  zitadel_instance = "cas-fee-adv-ed1ide.zitadel.cloud"
}

provider "google" {
  project = "ost-cas-adv-fee"
  region  = local.gcp_region
}

provider "random" {
}

provider "zitadel" {
  domain           = local.zitadel_instance
  jwt_profile_json = var.zitadel_auth
}

data "google_project" "project" {
}

terraform {
  backend "gcs" {
    bucket = "cas-fee-adv-mumble-api-terraform"
  }

  required_providers {
    zitadel = {
      source  = "zitadel/zitadel"
      version = "2.3.0"
    }
  }
}
