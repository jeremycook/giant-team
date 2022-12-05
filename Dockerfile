# See https://github.com/dotnet/dotnet-docker/tree/main/samples/complexapp


# Start build_web_app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build_web_app
WORKDIR /source
# Copy csproj and restore as distinct layers
COPY GiantTeam/*.csproj GiantTeam/
COPY GiantTeam.Asp/*.csproj GiantTeam.Asp/
COPY GiantTeam.DataProtection/*.csproj GiantTeam.DataProtection/
COPY GiantTeam.Authentication.Api/*.csproj GiantTeam.Authentication.Api/
COPY GiantTeam.Data.Api/*.csproj GiantTeam.Data.Api/
COPY WebApp/*.csproj WebApp/
RUN dotnet restore WebApp/WebApp.csproj
# Copy project code
COPY GiantTeam/ GiantTeam/
COPY GiantTeam.Asp/ GiantTeam.Asp/
COPY GiantTeam.DataProtection/ GiantTeam.DataProtection/
COPY GiantTeam.Authentication.Api/ GiantTeam.Authentication.Api/
COPY GiantTeam.Data.Api/ GiantTeam.Data.Api/
COPY WebApp/ WebApp/
# Build WebApp
WORKDIR /source/WebApp
RUN dotnet build -c release --no-restore


# Start publish_web_app
FROM build_web_app AS publish_web_app
RUN dotnet publish -c release --no-build -o /app


# Start build_solid_ui
FROM node:slim AS build_solid_ui
WORKDIR /src
COPY SolidUI .
RUN npm install
RUN npm run build


# Build the main entrypoint
#FROM mcr.microsoft.com/dotnet/aspnet:7.0
FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0-jammy-chiseled
WORKDIR /app
COPY --from=publish_web_app /app .
COPY --from=build_solid_ui /src/dist wwwroot/
ENV IN_CONTAINER=true
ENTRYPOINT ["dotnet", "WebApp.dll"]
