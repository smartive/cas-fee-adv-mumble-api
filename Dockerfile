### Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet publish \
  --configuration Release \
  -r linux-x64 \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -p:SelfContained=true \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -o ./artifacts \
  MumbleApi

### Deploy
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

ARG BUILD_VERSION=unknown
ARG COMMIT_SHA=unknown

ENV BUILD_VERSION=${BUILD_VERSION} \
  COMMIT_SHA=${COMMIT_SHA} \
  DOTNET_RUNNING_IN_CONTAINER=true \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  DOTNET_EnableDiagnostics=0 \
  ASPNETCORE_ENVIRONMENT=Production \
  LC_ALL=en_US.UTF-8 \
  LANG=en_US.UTF-8 \
  TZ=Europe/Zurich

LABEL org.opencontainers.image.source="https://github.com/smartive/cas-fee-adv-mumble-api" \
    org.opencontainers.image.authors="education@smartive.ch" \
    org.opencontainers.image.url="https://github.com/smartive/cas-fee-adv-mumble-api" \
    org.opencontainers.image.documentation="https://github.com/smartive/cas-fee-adv-mumble-api/blob/main/README.md" \
    org.opencontainers.image.source="https://github.com/smartive/cas-fee-adv-mumble-api/blob/main/Dockerfile" \
    org.opencontainers.image.version="${BUILD_VERSION}" \
    org.opencontainers.image.revision="${COMMIT_SHA}" \
    org.opencontainers.image.licenses="Apache-2.0" \
    org.opencontainers.image.title="Mumble API" \
    org.opencontainers.image.description="Demo API for Mumble. API that is used in the CAS FEE Advanced."

WORKDIR /app

COPY --from=build /app/artifacts .

CMD [ "/app/MumbleApi" ]
