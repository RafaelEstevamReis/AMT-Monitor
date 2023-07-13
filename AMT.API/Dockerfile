FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Git checkout step
RUN apt-get update && apt-get install -y git
RUN git clone https://github.com/RafaelEstevamReis/AMT-Monitor.git amtSrc && \
    cd amtSrc && \
    git checkout main

WORKDIR "/src/amtSrc/AMT.API"
RUN dotnet build "AMT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AMT.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AMT.API.dll"]
