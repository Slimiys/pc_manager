# PcManager

Клиент-серверное приложение для удалённого управления ПК через `ASP.NET Core`, `Avalonia`, `Host.Agent` и защищённый сетевой доступ по `Tailscale`.

## Быстрый старт
1. Установить .NET SDK (актуальная стабильная версия).
2. Запустить host-агент:
   - `dotnet run --project Host.Agent/Host.Agent.csproj`
3. Запустить API:
   - `dotnet run --project Server.Api/Server.Api.csproj`
4. Получить токен:
   - `POST /api/auth/token?role=Operator`
5. Вызвать системный endpoint:
   - `GET /api/system/info` с `Bearer` токеном.

## Запуск с портом через аргументы
- API:
  - `dotnet run --project Server.Api/Server.Api.csproj -- --port 8080`
- Host.Agent:
  - `dotnet run --project Host.Agent/Host.Agent.csproj -- --port 8090`

## Документация
- [Архитектура](docs/architecture.md)
- [API](docs/api.md)
- [Безопасность](docs/security.md)
- [Деплой](docs/deployment.md)
- [Тестирование (TDD + NUnit)](docs/testing.md)
