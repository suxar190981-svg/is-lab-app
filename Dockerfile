# Этап 1: сборка (build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY IsLabApp.csproj .
RUN dotnet restore

# Копируем остальной код и собираем
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Этап 2: запуск (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем опубликованное приложение из этапа build
COPY --from=build /app/publish .

# Устанавливаем порт, который будет слушать контейнер
EXPOSE 8080

# Команда запуска
ENTRYPOINT ["dotnet", "IsLabApp.dll"]
