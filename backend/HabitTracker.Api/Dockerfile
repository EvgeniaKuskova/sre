# Сборка (п. 5): из исходников получаем два артефакта:
#   * runtime — готовый к запуску образ приложения
#   * migrate — образ одноразового процесса наката миграций (п. 12)
# Конфигурация среды в образ НЕ зашивается — она передаётся только
# переменными окружения при запуске (п. 3, Release).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Сначала только манифест пакетов -> слой с восстановленными зависимостями
# переиспользуется между сборками (п. 2)
COPY HabitTracker.Api/HabitTracker.Api.csproj HabitTracker.Api/
RUN dotnet restore HabitTracker.Api/HabitTracker.Api.csproj

COPY HabitTracker.Api/ HabitTracker.Api/
RUN dotnet publish HabitTracker.Api/HabitTracker.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Артефакт для одноразового процесса миграций ---------------------------
FROM build AS migrate
RUN dotnet tool install --global dotnet-ef --version 9.0.20
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src/HabitTracker.Api
# Собираем заранее, чтобы на этапе запуска миграции накатывались быстро
RUN dotnet build --configuration Release
CMD ["dotnet", "ef", "database", "update", "--configuration", "Release", "--no-build"]

# --- Артефакт приложения ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Порт задаётся переменной окружения (п. 7), значение по умолчанию — 8080
ENV PORT=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "HabitTracker.Api.dll"]
