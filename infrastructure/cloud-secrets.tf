locals {
  secrets = {
    "db-pass" : random_password.database.result,
    "storage-access" : base64decode(google_service_account_key.gcs_access_key.private_key),
    "zitadel-api-app-key" : zitadel_application_key.api_app_key.key_details
  }
}

resource "google_secret_manager_secret" "secrets" {
  for_each = local.secrets

  secret_id = "${local.name}-${each.key}"

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret_version" "versions" {
  for_each = local.secrets

  secret      = google_secret_manager_secret.secrets[each.key].name
  secret_data = each.value
}

resource "google_secret_manager_secret_iam_member" "access" {
  for_each = local.secrets

  secret_id = google_secret_manager_secret_version.versions[each.key].id
  role      = "roles/secretmanager.secretAccessor"
  member    = "serviceAccount:${google_service_account.cloud_runner.email}"
  depends_on = [
    google_secret_manager_secret.secrets,
  ]
}
