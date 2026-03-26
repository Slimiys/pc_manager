# Тестирование

## Стандарт
- Используем `NUnit`.
- Применяем TDD: `Red -> Green -> Refactor`.

## Набор тестов
- `tests/Server.Domain.Tests` — доменные модели и правила.
- `tests/Server.Application.Tests` — use-cases и orchestration.
- `tests/Server.Api.IntegrationTests` — API, авторизация, базовые сценарии.

## Команды
- Запуск всех тестов:
  - `dotnet test PcManager.slnx`

## PR-checklist
- Есть тесты для нового поведения.
- Нет регрессий существующих тестов.
- Рефакторинг после Green не нарушает SOLID/KISS/DRY/YAGNI.
