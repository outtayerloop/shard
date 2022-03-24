FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY *.sln ./
COPY Shard.Shared.Core/*.csproj ./Shard.Shared.Core/
COPY Shard.Shared.Web.IntegrationTests/*.csproj ./Shard.Shared.Web.IntegrationTests/
COPY Shard.WiemEtBrunelle.IntegrationTests/*.csproj ./Shard.WiemEtBrunelle.IntegrationTests/
COPY Shard.WiemEtBrunelle.Web/*.csproj ./Shard.WiemEtBrunelle.Web/
COPY Shard.WiemEtBrunelle.UnitaireTests/*.csproj ./Shard.WiemEtBrunelle.UnitaireTests/

RUN dotnet restore
COPY . .
WORKDIR /src/Shard.Shared.Core
RUN dotnet build -c Release -o /app

WORKDIR /src/Shard.Shared.Web.IntegrationTests
RUN dotnet build -c Release -o /app

WORKDIR /src/Shard.WiemEtBrunelle.IntegrationTests
RUN dotnet build -c Release -o /app

WORKDIR /src/Shard.WiemEtBrunelle.Web
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Shard.WiemEtBrunelle.Web.dll"]