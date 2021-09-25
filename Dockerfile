FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim-arm32v7

WORKDIR /app

COPY ./bin/Debug/net5.0/linux-arm .

EXPOSE 80

ENTRYPOINT ["dotnet", "familyboard-aspnetcore.dll"]