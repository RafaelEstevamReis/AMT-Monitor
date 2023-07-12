#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG ip=10.0.0.1
ARG pwd=123456

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AMT.API/AMT.API.csproj", "AMT.API/"]
RUN dotnet restore "AMT.API/AMT.API.csproj"
COPY . .
WORKDIR "/src/AMT.API"
RUN dotnet build "AMT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AMT.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AMT.API.dll"]