FROM mcr.microsoft.com/dotnet/aspnet:6.0.7-bullseye-slim-arm32v7

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

# application
COPY ./bin/Debug/net6.0/linux-arm/publish .

ENTRYPOINT ["dotnet", "familyboard-aspnetcore.dll"]