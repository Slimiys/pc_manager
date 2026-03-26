# API

## Auth
- `POST /api/auth/token?role=Operator|Admin`
  - Возвращает JWT токен.

## Commands
- `POST /api/commands/execute`
  - Body:
    - `type`: `LockWorkstation` | `GetUptime`
    - `payload`: optional string
- `GET /api/commands/{id}`
  - Возвращает текущий статус команды.
  - Исполнение идет через `Host.Agent`, если настроен `Agent:BaseUrl`.

## System
- `GET /api/system/info`
  - Возвращает имя машины, ОС, runtime и UTC время.

## Realtime
- SignalR Hub: `/hubs/events`
- Событие: `CommandStatusChanged`

## Требования доступа
- Для `commands` и `system` требуется `Bearer` токен и роль `Operator` или `Admin`.
