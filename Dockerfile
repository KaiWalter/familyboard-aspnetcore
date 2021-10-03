FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim-arm32v7

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
ENV ASPNETCORE_URLS="https://+:5001"
ENV ASPNETCORE_HTTPS_PORT=5001
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$CERTPASS
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=$CERTNAME

WORKDIR /app

COPY ./bin/Debug/net5.0/linux-arm/publish .

ENTRYPOINT ["dotnet", "familyboard-aspnetcore.dll"]