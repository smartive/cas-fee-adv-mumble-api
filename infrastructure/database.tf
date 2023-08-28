locals {
  database_access_ips = {
    "smartive AG ZH" : "85.195.221.58/32",
    "smartive AG SG" : "85.195.251.214/32",
  }
}

resource "google_sql_database_instance" "pgsql_db" {
  name                = "${local.name}-pgsql-15"
  database_version    = "POSTGRES_15"
  region              = local.gcp_region
  deletion_protection = true

  settings {
    tier              = "db-g1-small"
    availability_type = "ZONAL"
    disk_autoresize   = true
    disk_type         = "PD_SSD"

    insights_config {
      query_insights_enabled = true
    }

    backup_configuration {
      enabled                        = true
      point_in_time_recovery_enabled = true
      start_time                     = "00:00"
      location                       = local.gcp_region
    }

    maintenance_window {
      day  = 7
      hour = 0
    }

    ip_configuration {
      dynamic "authorized_networks" {
        for_each = local.database_access_ips

        content {
          name  = authorized_networks.key
          value = authorized_networks.value
        }
      }
    }
  }
}

resource "random_password" "database" {
  length  = 16
  special = false
}

resource "google_sql_user" "db_user" {
  name     = "${local.name}-prod"
  instance = google_sql_database_instance.pgsql_db.name
  password = random_password.database.result
}

resource "google_sql_database" "db" {
  name     = "${local.name}-prod"
  instance = google_sql_database_instance.pgsql_db.name
}
