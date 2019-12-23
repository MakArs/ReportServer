#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
ENV TZ=Europe/Moscow
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build


WORKDIR /src
COPY ["ReportService/ReportService.csproj", "ReportService/"]
RUN dotnet restore "ReportService/ReportService.csproj"

RUN apk add tzdata \
        && cp /usr/share/zoneinfo/${TZ} /etc/localtime \
        && echo "${TZ}" > /etc/timezone

COPY . .
WORKDIR "/src/ReportService"
RUN dotnet build "ReportService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportService.dll"]