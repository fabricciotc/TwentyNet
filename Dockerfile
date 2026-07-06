# Multi-stage build for TwentyNet BFF
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY TwentyNet.slnx ./
COPY src/TwentyNet.Domain/TwentyNet.Domain.csproj src/TwentyNet.Domain/
COPY src/TwentyNet.Contracts/TwentyNet.Contracts.csproj src/TwentyNet.Contracts/
COPY src/TwentyNet.Application/TwentyNet.Application.csproj src/TwentyNet.Application/
COPY src/TwentyNet.Persistence/TwentyNet.Persistence.csproj src/TwentyNet.Persistence/
COPY src/TwentyNet.BFF/TwentyNet.BFF.csproj src/TwentyNet.BFF/
COPY tests/TwentyNet.Application.Tests/TwentyNet.Application.Tests.csproj tests/TwentyNet.Application.Tests/

RUN dotnet restore src/TwentyNet.BFF/TwentyNet.BFF.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish src/TwentyNet.BFF/TwentyNet.BFF.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TwentyNet.BFF.dll"]
