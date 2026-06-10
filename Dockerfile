FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceApp/FinanceApp.csproj", "FinanceApp/"]
RUN dotnet restore "FinanceApp/FinanceApp.csproj"
COPY . .
WORKDIR "/src/FinanceApp"
RUN dotnet build "FinanceApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceApp.dll"]
