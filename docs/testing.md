# Тестирование

## Стандарт
- Используем `NUnit`.
- Применяем TDD: `Red -> Green -> Refactor`.

## Набор тестов
- `tests/Server.Domain.Tests` — доменные модели и правила.
- `tests/Server.Application.Tests` — use-cases и orchestration.
- `tests/Server.Api.IntegrationTests` — API, авторизация, базовые сценарии.
