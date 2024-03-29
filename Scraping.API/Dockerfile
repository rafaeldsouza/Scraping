#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Scraping.API/Scraping.API.csproj", "Scraping.API/"]
COPY ["Scraping.Application/Scraping.Application.csproj", "Scraping.Application/"]
COPY ["Scraping.Core/Scraping.Core.csproj", "Scraping.Core/"]
RUN dotnet restore "Scraping.API/Scraping.API.csproj"
COPY . .
WORKDIR "/src/Scraping.API"
RUN dotnet build "Scraping.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scraping.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scraping.API.dll"]