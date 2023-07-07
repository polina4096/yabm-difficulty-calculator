FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["yabm-difficulty-calculator.csproj", "./"]
RUN dotnet restore "yabm-difficulty-calculator.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "yabm-difficulty-calculator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "yabm-difficulty-calculator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "yabm-difficulty-calculator.dll"]
