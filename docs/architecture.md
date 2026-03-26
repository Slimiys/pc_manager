# Архитектура

## Слои
- `Server.Api` — HTTP API, JWT, SignalR, rate limiting.
- `Host.Agent` — локальный агент на хосте для выполнения OS-команд (`LockWorkstation` и др.).
- `Server.Application` — orchestration и бизнес-поток команд.
- `Server.Domain` — доменные модели и состояния.
- `Server.Infrastructure` — in-memory хранилище, исполнители, аудит, системная информация.
- `Client.Avalonia` — кросс-платформенный клиент для Desktop/Android.

## Принципы
- SOLID обязателен во всех слоях.
- KISS/DRY/YAGNI используются как критерии проектирования.
- Публичные классы и методы имеют XML-документацию.
- Нейминг в C# следует соглашениям `ktaranov naming convention` (PascalCase/camelCase, `I` для интерфейсов, `_camelCase` для private fields).

## Поток команды
1. Клиент отправляет `POST /api/commands/execute`.
2. `CommandService` проверяет allowlist и идемпотентность.
3. Команда проходит статусы `Queued -> Started -> Completed|Failed`.
4. `Server.Api` передает выполнение в `Host.Agent` (при настроенном `Agent:BaseUrl`) или исполняет локально.
5. Состояние сохраняется в `ICommandRepository`.
6. Событие статуса отправляется через SignalR Hub.
