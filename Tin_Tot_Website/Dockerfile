FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache restore
COPY ["Tin_Tot_Website/Tin_Tot_Website.csproj", "Tin_Tot_Website/"]
COPY ["TinTot.Application/TinTot.Application.csproj", "TinTot.Application/"]
COPY ["TinTot.Infrastructure/TinTot.Infrastructure.csproj", "TinTot.Infrastructure/"]
COPY ["TinTot.Domain/TinTot.Domain.csproj", "TinTot.Domain/"]

RUN dotnet restore "Tin_Tot_Website/Tin_Tot_Website.csproj"

# Copy toàn bộ source code
COPY . .

WORKDIR /src
RUN dotnet publish "Tin_Tot_Website/Tin_Tot_Website.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tin_Tot_Website.dll"]