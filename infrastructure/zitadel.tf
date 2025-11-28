locals {
  zitadel_org_id     = "338558858827942346"
  zitadel_project_id = "348701753820117818"
}

resource "zitadel_application_api" "api" {
  org_id           = local.zitadel_org_id
  project_id       = local.zitadel_project_id
  name             = "API"
  auth_method_type = "API_AUTH_METHOD_TYPE_PRIVATE_KEY_JWT"
}

resource "zitadel_application_key" "api_app_key" {
  org_id     = local.zitadel_org_id
  project_id = local.zitadel_project_id
  app_id     = zitadel_application_api.api.id

  key_type        = "KEY_TYPE_JSON"
  expiration_date = "2100-01-01T00:00:00Z"
}

resource "zitadel_application_oidc" "swagger" {
  org_id     = local.zitadel_org_id
  project_id = local.zitadel_project_id

  name = "Swagger UI"
  redirect_uris = [
    "https://mumble-api-prod-714602723919.europe-west6.run.app/oauth2-redirect.html",
  ]
  app_type          = "OIDC_APP_TYPE_WEB"
  response_types    = ["OIDC_RESPONSE_TYPE_CODE"]
  grant_types       = ["OIDC_GRANT_TYPE_AUTHORIZATION_CODE"]
  auth_method_type  = "OIDC_AUTH_METHOD_TYPE_NONE"
  clock_skew        = "0s"
  dev_mode          = true
  version           = "OIDC_VERSION_1_0"
  access_token_type = "OIDC_TOKEN_TYPE_BEARER"
}
