resource "google_service_account" "cloud_runner" {
  account_id   = "${local.name}-cloud-runner"
  display_name = "Google Cloud Run Service Account"
  description  = "Account to deploy applications to google cloud run and access SQL instance."
}

resource "google_project_iam_member" "cloud_runner" {
  for_each = toset([
    "roles/run.serviceAgent",
    "roles/viewer",
    "roles/storage.objectViewer",
    "roles/run.admin",
    "roles/cloudsql.client"
  ])
  role    = each.key
  member  = "serviceAccount:${google_service_account.cloud_runner.email}"
  project = data.google_project.project.id
}

resource "google_project_iam_member" "cloud_runner_svc" {
  role    = "roles/run.serviceAgent"
  member  = "serviceAccount:service-${data.google_project.project.number}@serverless-robot-prod.iam.gserviceaccount.com"
  project = data.google_project.project.id
}

locals {
  run_env = {
    "ASPNETCORE_URLS" : "http://+:8080",
    "DATABASE__HOST" : "/cloudsql/${google_sql_database_instance.pgsql_db.connection_name}",
    "DATABASE__PORT" : "5432",
    "DATABASE__DATABASE" : google_sql_database.db.name,
    "DATABASE__USERNAME" : google_sql_user.db_user.name,
    "AUTHENTICATION__ISSUER" : "https://${local.zitadel_instance}",
    "SWAGGER__CLIENTID" : zitadel_application_oidc.swagger.client_id,
    "STORAGE__BUCKET" : google_storage_bucket.gcs_bucket.name,
  }
  run_secrets = {
    "DATABASE_PASSWORD" : "db-pass",
    "AUTHENTICATION_JWTKEY" : "zitadel-api-app-key",
    "STORAGE_SERVICEACCOUNTKEY" : "storage-access",
  }
}

resource "google_cloud_run_service" "api" {
  name                       = "${local.name}-prod"
  location                   = local.gcp_region
  autogenerate_revision_name = true

  template {
    metadata {
      annotations = {
        "run.googleapis.com/cloudsql-instances" = google_sql_database_instance.pgsql_db.connection_name
      }
    }

    spec {
      containers {
        image = "europe-west6-docker.pkg.dev/ost-cas-adv-fee/cas-fee-adv-mumble/mumble-api:${var.release_version}"

        resources {
          limits = {
            "memory" = "512Mi"
          }
        }

        ports {
          name           = "http1"
          container_port = 8080
        }

        dynamic "env" {
          for_each = local.run_env

          content {
            name  = env.key
            value = env.value
          }
        }

        dynamic "env" {
          for_each = local.run_secrets

          content {
            name = env.key
            value_from {
              secret_key_ref {
                key  = "latest"
                name = google_secret_manager_secret.secrets[env.value].secret_id
              }
            }
          }
        }
      }

      service_account_name = google_service_account.cloud_runner.email
    }
  }

  traffic {
    percent         = 100
    latest_revision = true
  }

  depends_on = [
    google_secret_manager_secret.secrets,
    google_secret_manager_secret_version.versions,
    google_secret_manager_secret_iam_member.access,
  ]
}

data "google_iam_policy" "noauth" {
  binding {
    role = "roles/run.invoker"
    members = [
      "allUsers",
    ]
  }
}

resource "google_cloud_run_service_iam_policy" "noauth" {
  location = google_cloud_run_service.api.location
  project  = google_cloud_run_service.api.project
  service  = google_cloud_run_service.api.name

  policy_data = data.google_iam_policy.noauth.policy_data
}
