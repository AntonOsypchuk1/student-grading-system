# Student Grading System

**Student Grading System** — платформа для автоматизованого аналізу студентських письмових робіт із використанням AI.

---

## Структура репозиторію

```/
├── SGS/ # ASP.NET Core Web API
│ ├── Tests/
│ │ ├── SGS.IntegrationTests/
│ │ ├── SGS.SemanticAnalysisTests/
│ │ └── SGS.ServicesTests/
│ ├── SGS.API/
│ │ ├── Controllers/ # API-контролери
│ │ ├── appsettings*.json # Конфіги (ConnectionStrings, JwtSettings, SemanticKernelSettings, Storage)
│ │ └── Program.cs # Startup сервісів та конфігурацій
│ ├── SGS.Application/
│ │ ├── DTOs/
│ │ ├── Services/ # Сервіси бізнес-моделі
│ │ ├── Interfaces
│ │ └── ApplicationModule.cs # Підключення модулю в DI
│ ├── SGS.Domain/
│ │ ├── Entities/ # Основні сутності бази даних
│ │ └── Enums/
│ ├── SGS.Infrastructure/ # Репозиторії + Unit of Work
│ │ ├── Migrations # EF Core міграції
│ │ ├── Interfaces
│ │ ├── Repositories # Репозиторії для EF
│ │ └── InfrastructureModule.cs
│ ├── SGS.SemanticAnalysis/ # AI-модуль на базі Semantic Kernel
│ │ ├── Kernels # Конфігурація Semantic Kernel
│ │ ├── Prompts # Опис промтів
│ │ ├── Services # Сервіси створення кернелів
│ │ └── SemanticAnalysisModule.cs
├── SGS.Web/ # React + Vite фронтенд
│ ├── src/
│ │ ├── api/ # Axios-клієнт
│ │ ├── hooks/ # React-Query hooks
│ │ ├── components/ # UI-компоненти
│ │ ├── features/ # Сторінки для студентів і викладачів
│ │ ├── types/ # Типи для TypeScript
│ │ ├── context/ # Конекст для навігації
│ │ └── App.tsx # Основний Router
│ └── vite.config.ts # Проксі налаштування для /api
```
---

## Початок роботи

### 1. Передумови

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+ & npm](https://nodejs.org/)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- Для роботи AI модуля [AzureOpenAI](https://ai.azure.com/) (або будь-який інший ресурс), чи локально розгортати AI модель (наприклад, [Ollama](https://ollama.com/search))
- (опціонально) Azure Blob Storage або локальний диск

### 2. Клонування

```bash
git clone https://git.ztu.edu.ua/ipz/2021-2025/ipz-21-3/anton-osypchuk/student-grading-system.git
cd student-grading-system
```

### 3. Серверна частина
Встановіть рядки підключення та ключі у Server/appsettings.json і appsettings.Development.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sgs_db;Username=sgs;Password=secret"
  },
  "JwtSettings": {
    "Secret": "<your-jwt-secret>",
    "Issuer": "SGSServer",
    "Audience": "SGSClient",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "SemanticKernelSettings": {
    "Endpoint": "<your-azure-endpoint>",
    "ApiKey": "<your-azure-key>",
    "DeploymentName": "gpt-4",
    "Language": "en"
  },
  "Storage": {
    "Provider": "Disk",
    "BasePath": "<path-to-store-files>"
  }
}
```

Виконайте міграції БД:

```bash
cd SGS.Infrastructure/
dotnet ef database update
```

Запустіть API:
```bash
dotnet run
```

За замовчуванням доступно на https://localhost:7248/api.

### 4. Клієнтська частина
Перейдіть до клієнтської папки та встановіть залежності:

```bash
cd ../Client
npm install
```

Запустіть Vite-сервер:

```bash
npm run dev
```

Відкрийте у браузері http://localhost:5173.

## Основні технології
Backend:
- ASP.NET Core Web API
- Entity Framework Core (Repository + Unit of Work)
- Microsoft Semantic Kernel (LLM-інтеграція)
- PostgreSQL
- JWT-автентифікація

Frontend:
- React + TypeScript
- Vite
- React Query (кешування)
- Axios (API-клієнт із перехоплювачами)
- Styled components

Тестування:
- xUnit + Moq (юніт-тести)
- WebApplicationFactory (інтеграційні тести)

Юніт-тести:
```bash
cd Tests/SGS.ServicesTests/
dotnet test
```

Інтеграційні тести:
```bash
cd Tests/SGS.IntegrationTests/
dotnet test
```

Тести SemanticAnalysis модулю:
```bash
cd Tests/SGS.SemanticAnalysisTests/
dotnet test
```

Ліцензія
[MIT © Осипчук Антон Олексійович](https://git.ztu.edu.ua/ipz/2021-2025/ipz-21-3/anton-osypchuk/student-grading-system/-/blob/0aaedaa2e4bc5825857a4c40e59de0013de5b946/LICENSE)