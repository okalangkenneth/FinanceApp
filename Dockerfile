# Debian-based runtime kept deliberately: QuestPDF's bundled natives
# (libQuestPdfSkia.so/libqpdf.so) resolve fully against it (verified via ldd,
# no extra apt packages needed); alpine would need the musl variant validated.
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
# DataProtection key directory must exist app-owned BEFORE the compose named
# volume first mounts there — Docker copies ownership from the image dir; an
# absent dir yields a root-owned mountpoint the non-root user cannot write.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet
# Non-root user shipped with the .NET 8 images; 8080 needs no privileges
USER app
ENTRYPOINT ["dotnet", "FinanceApp.dll"]
