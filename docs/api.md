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
- Событие: `NotificationReceived` (тело: `title`, `message`, `receivedAtUtc`)
- Подключение к hub требует JWT с ролью `Operator` или `Admin` (как у остального API).

## Входящие оповещения по HTTP
- `POST /api/notifications/inbound`
  - Заголовок: `X-Notification-Key` — должен совпадать с `Notifications:InboundApiKey` в конфигурации сервера.
  - Body (JSON): `title` (обязательно), `message` (необязательно).
  - Не использует JWT; при пустом или не заданном `Notifications:InboundApiKey` возвращается `503`.
  - Успешный ответ сохраняет оповещение на сервере и рассылает событие `NotificationReceived` подписчикам SignalR.
  - Если задан `Notifications:ToastListenerBaseUrl`, сервер дополнительно вызывает HTTP toast-слушатель на этой машине (см. ниже).
- `GET /api/notifications/recent?sinceUtc={ISO-8601}`
  - `Bearer` JWT, роль `Operator` или `Admin`.
  - Возвращает JSON-массив оповещений с `receivedAtUtc` строго позже `sinceUtc` (для опроса из UI).

## Host.Agent (на управляемом ПК)
- `POST /api/agent/execute` — выполнение команды (см. архитектуру), заголовок `X-Agent-Key` (совпадает с `Agent:ApiKey` на агенте и секретом на сервере при вызове агента).

## Toast Listener (Avalonia, на ПК с десктопом)
Отдельное приложение `Client.ToastListener.Desktop` поднимает HTTP и показывает toast по запросу сервера.

- `POST {ToastListenerBaseUrl}api/toast/show`
  - Заголовок: `X-Toast-Listener-Key` — должен совпадать с `Notifications:ToastListenerKey` на сервере и с `ToastListener:ApiKey` в `appsettings.json` слушателя.
  - Body (JSON): `title` (обязательно), `message` (необязательно).
  - На сервере: задайте `Notifications:ToastListenerBaseUrl` (например `http://127.0.0.1:8787/`) и тот же ключ в `Notifications:ToastListenerKey`. Если `ToastListenerBaseUrl` пустой, пересылка на десктоп отключена.

## Требования доступа
- Для `commands` и `system` требуется `Bearer` токен и роль `Operator` или `Admin`.
