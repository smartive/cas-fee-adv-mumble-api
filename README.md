# Mumble API

This is the API for "Mumble". A simple API that supports creating
posts, replies and attaching media to them. Furthermore, the API
supports liking and unliking posts and replies.

This API is a demo API for the Certificate of Advanced Studies (CAS)
in Frontend Engineering Advanced at the University of Applied Sciences
OST in Switzerland (CAS FEE ADV OST).

Most calls to the API are required to be authenticated by
[ZITADEL](https://zitadel.com). Accessing the list of "posts" or
the "search" is possible without authentication. To create, like, unlike,
or delete a post (or a reply), the user must be authenticated. The authentication
is done via OIDC and the API expects a valid JWT/Opaque token in the
`HTTP Authorization` header.

## Access

The API is deployed on Google Cloud Run.
Swagger / OpenAPI Documentation is available.

Access the documentation on the following URL:
[https://mumble-api-prod-4cxdci3drq-oa.a.run.app/index.html](https://mumble-api-prod-4cxdci3drq-oa.a.run.app/index.html)

## Using own frontend

All authenticated calls require a valid opaque OIDC token. To get a token,
you receive a ZITADEL organization and a project grant to your organization.
You then may configure your application by yourself and use the API with the
fetched tokens.
