resource "google_storage_bucket" "gcs_bucket" {
  name                        = "${local.name}-data"
  location                    = local.gcp_region
  uniform_bucket_level_access = false
  force_destroy               = true
}

resource "google_storage_bucket_iam_member" "gcs_bucket" {
  bucket = google_storage_bucket.gcs_bucket.name
  role   = "roles/storage.objectViewer"
  member = "allUsers"
}

resource "google_service_account" "gcs_access" {
  account_id   = "${local.name}-gcs"
  display_name = "Storage Access ${local.name}"
  description  = "Account to access the ${local.name} gcs bucket."
}

resource "google_storage_bucket_iam_member" "gcs_iam" {
  bucket = google_storage_bucket.gcs_bucket.name
  role   = "roles/storage.admin"
  member = "serviceAccount:${google_service_account.gcs_access.email}"
}

resource "google_service_account_key" "gcs_access_key" {
  service_account_id = google_service_account.gcs_access.id
}
