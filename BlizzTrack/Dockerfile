#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get update && apt-get -y install nodejs
COPY ["BlizzTrack/BlizzTrack.csproj", "BlizzTrack/"]
RUN dotnet restore "BlizzTrack/BlizzTrack.csproj"
COPY . .
WORKDIR "/src/BlizzTrack"
RUN dotnet build "BlizzTrack.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlizzTrack.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlizzTrack.dll"]