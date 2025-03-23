ARG VARIANT=bookworm-slim-arm32v7
ARG DOTNETVERSION=8.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNETVERSION}-${VARIANT} AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNETVERSION}-${VARIANT}

# certificate creation and configuration
ENV CERTNAME=/tmp/familyboard.pfx
ENV CERTPASS=$(pwgen)
RUN openssl req -x509 \
    -passout env:CERTPASS \
    -subj "/CN=familyboard.my.net" \
    -newkey rsa:4096 \
    -keyout /tmp/familyboardkey.pem \
    -out /tmp/familyboardcert.pem \
    -days 365
RUN openssl pkcs12 -export \
    -out $CERTNAME \
    -inkey /tmp/familyboardkey.pem \
    -in /tmp/familyboardcert.pem \
    -passin env:CERTPASS \
    -passout env:CERTPASS

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="https://+:5001;http://+:5000"
ENV ASPNETCORE_HTTPS_PORT=5001
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$CERTPASS
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=$CERTNAME

# token cache configuration
WORKDIR /app
RUN mkdir .tokenkeycache
ENV TOKENKEYCACHEPATH=/app/.tokenkeycache
ENV IMAGESPLAYEDPATH=/app/.imagesplayed.json

COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "familyboard-aspnetcore.dll"]
