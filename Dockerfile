# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS src
WORKDIR /src
COPY src .
RUN dotnet publish PasswordManager.Web -c Release -r linux-x64 -o /out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=src /out .
EXPOSE 80/tcp 443/tcp

ENTRYPOINT ["dotnet", "PasswordManager.Web.dll"]
