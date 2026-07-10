# Stage 1: Build Environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution file and project descriptors to restore dependencies (leverages Docker cache layers)
COPY BalanceFlow.sln ./
COPY src/BalanceFlow.Domain/BalanceFlow.Domain.csproj src/BalanceFlow.Domain/
COPY src/BalanceFlow.Application/BalanceFlow.Application.csproj src/BalanceFlow.Application/
COPY src/BalanceFlow.Infrastructure/BalanceFlow.Infrastructure.csproj src/BalanceFlow.Infrastructure/
COPY src/BalanceFlow.Api/BalanceFlow.Api.csproj src/BalanceFlow.Api/
COPY src/BalanceFlow.UnitTests/BalanceFlow.UnitTests.csproj src/BalanceFlow.UnitTests/

RUN dotnet restore BalanceFlow.sln

# Copy the entire source tree and compile in Release mode
COPY . ./
RUN dotnet publish src/BalanceFlow.Api/BalanceFlow.Api.csproj -c Release -o out

# Stage 2: Lightweight ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Bind to port 8080 (standard port for Render, Koyeb, Railway, and Fly.io container platforms)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BalanceFlow.Api.dll"]
