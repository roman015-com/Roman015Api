# https://hub.docker.com/_/microsoft-dotnet-sdk/
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_USERNAME=test
ARG BUILD_TOKEN=test
WORKDIR /source

# Copy source and build App
COPY . ./Roman015API/
WORKDIR /source/Roman015API
RUN dotnet nuget add source --username $BUILD_USERNAME --password $BUILD_TOKEN --store-password-in-clear-text --name "Roman015Github" "https://nuget.pkg.github.com/roman015-com/index.json"
RUN dotnet restore
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 80
ENTRYPOINT ["dotnet", "Roman015API.dll"]