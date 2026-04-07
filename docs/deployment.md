# Деплой

## Docker
- `docker-compose.yml` поднимает **только** `Server.Api` (образ из `Server.Api/Dockerfile`).
- Контейнер слушает `0.0.0.0:8080` (снаружи обычно `http://localhost:8080`).
- **Host.Agent** в compose не входит: агент нужно запускать на **управляемом ПК** (например `dotnet run --project Host.Agent/Host.Agent.csproj -- --port 8090`), чтобы команды выполнялись в реальной ОС, а не в контейнере.

### Связка API (Docker) → агент (хост)
- В переменных окружения сервера задайте `Agent__BaseUrl` на адрес агента, доступный **из контейнера**:
  - Docker Desktop (Windows/Mac): часто `http://host.docker.internal:8090/` — агент на той же машине на порту 8090.
  - Linux: `host.docker.internal` может быть недоступен — укажите IP хоста или машины с агентом в LAN/Tailscale, например `http://192.168.1.10:8090/`.
- `Agent__ApiKey` на сервере и `Agent:ApiKey` у агента должны **совпадать**.

### Образ агента без compose
- `Host.Agent/Dockerfile` по-прежнему можно собирать вручную (`docker build -f Host.Agent/Dockerfile .`), если нужен контейнерный агент для тестов — но для управления реальным рабочим столом Windows используйте нативный запуск агента.

## Конфигурация окружения
- Для связки `Server.Api -> Host.Agent` используются:
  - `Agent__BaseUrl`
  - `Agent__ApiKey`
- Для авторизации API используется `Jwt__Secret`.

## Tailscale
- Сетевой доступ между устройствами обеспечивается через tailnet.
- Клиент обращается к API по tailnet-имени/адресу хоста.
- Для production требуется HTTPS и доверенные сертификаты.
