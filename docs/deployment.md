# Деплой

## Docker
- `Server.Api/Dockerfile` собирает и публикует ASP.NET API.
- Контейнер слушает `0.0.0.0:8080`.
- `Host.Agent/Dockerfile` собирает и публикует host-агент.
- Контейнер агента слушает `0.0.0.0:8090`.

## Локальный запуск compose
- `docker compose up --build -d`
- Проверка:
  - `GET http://localhost:8080/api/system/info` (с токеном).
- Для связки server->agent используются:
  - `Agent__BaseUrl` (например `http://host-agent:8090/`)
  - `Agent__ApiKey`

## Tailscale
- Подключите ПК и Android к одной tailnet.
- На Android клиент обращается к API по tailnet-имени/адресу ПК и порту `8080`.
- Для production использовать HTTPS и безопасные сертификаты.
