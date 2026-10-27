# Структура проєкту MyCloudSync

Рішення `MyCloudSync.sln` на .NET 10 відповідає багатошаровій архітектурі з [docs/architecture/architecture.md](architecture/architecture.md).

```text
MyCloudSync/
├── MyCloudSync.sln
├── src/
│   ├── MyCloudSync.App/              WinForms: вікна, трей, Program.cs
│   │   ├── Forms/                головне вікно, вибір папки на Drive
│   │   ├── Pages/                сторінки «Синхронізація», «Журнал», «Налаштування»
│   │   ├── Controls/             кольори, картка з рамкою
│   │   ├── Tray/
│   │   └── Resources/
│   ├── MyCloudSync.Core/             моделі, інтерфейси, логіка синхронізації
│   │   ├── Models/
│   │   ├── Abstractions/
│   │   └── Sync/
│   └── MyCloudSync.Infrastructure/   Google Drive, файлова система, SQLite, Windows
│       ├── GoogleDrive/
│       ├── FileSystem/
│       ├── Data/
│       └── Platform/
└── tests/
    └── MyCloudSync.Tests/            xUnit-тести
```

## Проєкти

| Проєкт | Платформа | Залежить від | Зміст |
|---|---|---|---|
| `MyCloudSync.App` | net10.0-windows, WinForms | Core, Infrastructure | `MainForm` (заголовок, бокова панель), сторінки `SyncPage`, `LogPage`, `SettingsPage`, `DriveFolderPickerForm`, `TrayIconController`, налаштування залежностей у `Program.cs` |
| `MyCloudSync.Core` | net10.0 | — | Моделі таблиць бази, інтерфейси сервісів і репозиторіїв, класи логіки синхронізації |
| `MyCloudSync.Infrastructure` | net10.0-windows | Core | Реалізації інтерфейсів Core: Google Drive, файлова система, SQLite, DPAPI, автозапуск |
| `MyCloudSync.Tests` | net10.0-windows, xUnit | Core, Infrastructure | Модульні тести: база даних, файлова система, логіка синхронізації, повний цикл синхронізації |

## Бібліотеки

| Пакет | Проєкт | Призначення |
|---|---|---|
| `Google.Apis.Drive.v3` | Infrastructure | Google Drive API і OAuth 2.0 |
| `Microsoft.Data.Sqlite` | Infrastructure | Доступ до SQLite |
| `System.Security.Cryptography.ProtectedData` | Infrastructure | Шифрування токенів через DPAPI |
| `Microsoft.Extensions.DependencyInjection` | App | Контейнер залежностей |
| `xunit` | Tests | Модульні тести |

## Компоненти та відповідальні

| Папка | Класи | Відповідальний |
|---|---|---|
| `App/Forms`, `App/Pages`, `App/Controls`, `App/Tray` | `MainForm`, `SyncPage`, `LogPage`, `SettingsPage`, `DriveFolderPickerForm`, `CardPanel`, `Theme`, `TrayIconController` | Задорожний Максим |
| `Infrastructure/Platform` | `AutostartService` | Задорожний Максим |
| `Infrastructure/GoogleDrive` | `GoogleAuthService`, `AccountTokenStore`, `GoogleDriveStorage` | Іванюк Олексій |
| `Infrastructure/Platform` | `DpapiTokenProtector` | Іванюк Олексій |
| `Core/Sync` | `SyncCoordinator`, `ChangeDetector`, `SyncPlanner`, `SyncScheduler`, `ExcludeMatcher`, `ConflictResolver` | Самсін Владислав |
| `Infrastructure/Data`, `Infrastructure/FileSystem` | репозиторії, `DatabaseInitializer`, `LocalFileSystem`, `FolderWatcher`, `HashCalculator` | Самсін Владислав |

## Стан реалізації

| Частина | Стан |
|---|---|
| Структура рішення, моделі, інтерфейси, контейнер залежностей | Готово |
| Інтерфейс за макетами етапу 3, підключений до синхронізації; сповіщення про конфлікти | Готово |
| База даних: створення схеми та всі репозиторії | Готово |
| Файлова система: `LocalFileSystem`, `FolderWatcher`, `HashCalculator` | Готово |
| Логіка: `ChangeDetector`, `SyncPlanner`, `ExcludeMatcher`, `ConflictResolver`, `SyncScheduler` | Готово |
| Повний цикл синхронізації: `SyncCoordinator` | Готово |
| Google Drive: `GoogleAuthService`, `AccountTokenStore`, `GoogleDriveStorage` | Готово |
| `DpapiTokenProtector`, `AutostartService` | Готово |

## Тести

| Клас | Що перевіряє |
|---|---|
| `DatabaseTests` | Створення бази та збереження налаштувань |
| `RepositoryTests` | Збереження і читання акаунта, пари папок, масок, стану файлів, конфліктів |
| `LocalFileSystemTests` | Сканування папки, хеші MD5, копіювання та перейменування файлів |
| `FolderWatcherTests` | Події про зміни в папці |
| `ChangeDetectorTests` | Усі правила трьохстороннього порівняння |
| `SyncPlannerTests` | План дій, маски виключень, режими синхронізації, зайняті файли |
| `ExcludeMatcherTests` | Маски `*`, `?` і маски папок |
| `ConflictResolverTests` | Назва конфліктної копії |
| `SyncCoordinatorTests` | Повний цикл синхронізації з тестовим сховищем у пам'яті: вивантаження, завантаження, видалення, конфлікт, пауза, відсутність мережі |
| `SyncSchedulerTests` | Таймер перевірки та об'єднання подій (debounce) |

## Запуск

Відкрити `MyCloudSync.sln` у Visual Studio 2022 або новішій (з компонентом «.NET desktop development»), зробити `MyCloudSync.App` стартовим проєктом і запустити (F5). Під час першого запуску створюється база `%LOCALAPPDATA%\MyCloudSync\mycloudsync.db`.

Для входу через Google потрібен файл `client_secret.json` — OAuth-клієнт типу «Desktop app», створений у Google Cloud Console для проєкту з увімкненим Google Drive API. Файл кладеться в папку `src/MyCloudSync.App` і під час збирання копіюється поруч із програмою. У репозиторій він не потрапляє (`.gitignore`).

Тести: **Test → Run All Tests** або `dotnet test`. Тестам Google-акаунт не потрібен: замість Google Drive використовується сховище в пам'яті.
