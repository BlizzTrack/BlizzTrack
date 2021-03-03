#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Worker/Worker.csproj", "Worker/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["BNetLib/BNetLib.csproj", "BNetLib/"]
RUN dotnet restore "Worker/Worker.csproj"
COPY . .
WORKDIR "/src/Worker"
RUN dotnet build "Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]