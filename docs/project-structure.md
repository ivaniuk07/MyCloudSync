# Структура проєкту MyCloudSync

Рішення `MyCloudSync.sln` на .NET 10 відповідає багатошаровій архітектурі з [docs/architecture/architecture.md](architecture/architecture.md).

```text
MyCloudSync/
├── MyCloudSync.sln
├── src/
│   ├── MyCloudSync.App/              WinForms: вікна, трей, Program.cs
│   │   ├── Forms/
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
| `MyCloudSync.App` | net10.0-windows, WinForms | Core, Infrastructure | `MainForm`, `SettingsForm`, `LogForm`, `DriveFolderPickerForm`, `TrayIconController`, налаштування залежностей у `Program.cs` |
| `MyCloudSync.Core` | net10.0 | — | Моделі таблиць бази, інтерфейси сервісів і репозиторіїв, класи логіки синхронізації |
| `MyCloudSync.Infrastructure` | net10.0-windows | Core | Реалізації інтерфейсів Core: Google Drive, файлова система, SQLite, DPAPI, автозапуск |
| `MyCloudSync.Tests` | net10.0-windows, xUnit | Core, Infrastructure | Тести бази даних і налаштувань |

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
| `App/Forms`, `App/Tray` | `MainForm`, `SettingsForm`, `LogForm`, `DriveFolderPickerForm`, `TrayIconController` | Задорожний Максим |
| `Infrastructure/Platform` | `AutostartService` | Задорожний Максим |
| `Infrastructure/GoogleDrive` | `GoogleAuthService`, `GoogleDriveStorage` | Іванюк Олексій |
| `Infrastructure/Platform` | `DpapiTokenProtector` | Іванюк Олексій |
| `Core/Sync` | `SyncCoordinator`, `ChangeDetector`, `SyncPlanner`, `SyncScheduler`, `ExcludeMatcher`, `ConflictResolver` | Самсін Владислав |
| `Infrastructure/Data`, `Infrastructure/FileSystem` | репозиторії, `DatabaseInitializer`, `LocalFileSystem`, `FolderWatcher`, `HashCalculator` | Самсін Владислав |

## Стан реалізації

| Частина | Стан |
|---|---|
| Структура рішення, моделі, інтерфейси, контейнер залежностей | Готово |
| Вікна та іконка в треї | Розмітка й обробники подій |
| Створення бази даних (`schema.sql` як вбудований ресурс) | Готово |
| `SettingsRepository`, `SyncLogRepository` | Готово |
| `DpapiTokenProtector`, `AutostartService`, `HashCalculator` | Готово |
| Робота з Google Drive, файлова система, логіка синхронізації, інші репозиторії | Оголошено методи, реалізація — на наступних етапах |

## Запуск

Відкрити `MyCloudSync.sln` у Visual Studio 2022 (з компонентом «.NET desktop development»), зробити `MyCloudSync.App` стартовим проєктом і запустити (F5). Під час першого запуску створюється база `%LOCALAPPDATA%\MyCloudSync\mycloudsync.db`.

Тести: **Test → Run All Tests** або `dotnet test`.
