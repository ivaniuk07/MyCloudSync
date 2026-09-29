# Діаграми MyCloudSync

## Варіанти використання

```mermaid
flowchart LR
    User[Користувач]
    Drive[Google Drive]
    subgraph App[MyCloudSync]
        Auth([Підключити акаунт])
        Local([Вибрати локальну папку])
        Remote([Вибрати папку Google Drive])
        Sync([Запустити або призупинити синхронізацію])
        Status([Переглянути статус і прогрес])
        Log([Переглянути журнал операцій])
    end
    User --- Auth
    User --- Local
    User --- Remote
    User --- Sync
    User --- Status
    User --- Log
    Drive --- Auth
    Drive --- Remote
    Drive --- Sync
```

## Компоненти й потік взаємодії

```mermaid
flowchart LR
    User[Користувач] --> UI[WinForms UI]
    subgraph App[MyCloudSync на Windows]
        UI --> Core[SyncCoordinator]
        Watcher[FolderWatcher і контрольне сканування] --> Core
        Core --> Adapter[GoogleDriveStorage]
        Core --> Repo[SQLite-репозиторії]
    end
    Core --> Files[Локальні файли]
    Adapter --> Drive[Google Drive API v3]
    Repo --> DB[(SQLite)]
```

## Послідовність синхронізації

```mermaid
sequenceDiagram
    actor User as Користувач
    participant UI as WinForms UI
    participant Core as SyncCoordinator
    participant DB as SQLite
    participant Local as Локальна папка
    participant Drive as Google Drive API
    User->>UI: Натиснути «Синхронізувати зараз»
    UI->>Core: Запустити синхронізацію
    Core->>DB: Завантажити конфігурацію та стан файлів
    DB-->>Core: Останній збережений стан
    Core->>Local: Перевірити локальні файли
    Local-->>Core: Список локальних змін
    Core->>Drive: Отримати зміни хмарної папки
    Drive-->>Core: Список хмарних змін
    alt Зміни лише локально
        Core->>Drive: Вивантажити нові або змінені файли
        Drive-->>Core: Результат і Google Drive ID
    else Зміни лише в хмарі
        Core->>Local: Завантажити нові або змінені файли
    else Зміни з обох боків
        Note over Core,Drive: Після MVP зберегти обидві версії та записати конфлікт
    end
    Core->>DB: Оновити стан файлів і журнал
    Core-->>UI: Передати статус, прогрес і помилки
    UI-->>User: Показати результат
```

## Модель даних

Спрощена ER-діаграма з ключовими полями. Повний опис усіх полів, типів, індексів і SQL-скрипт створення бази — у [docs/architecture/data-model.md](../architecture/data-model.md).

```mermaid
erDiagram
    Account ||--o{ SyncPair : "має"
    SyncPair ||--o{ FileState : "відстежує"
    SyncPair ||--o{ ExcludeRule : "налаштовує"
    SyncPair ||--o{ SyncLog : "журналює"
    FileState ||--o{ SyncLog : "стосується"
    FileState ||--o{ Conflict : "фіксує"

    Account {
        int id PK
        string email UK
        string access_token "DPAPI"
        string refresh_token "DPAPI"
        datetime token_expires_at
    }
    SyncPair {
        int id PK
        int account_id FK
        string local_path
        string drive_folder_id
        string sync_mode
        bool is_active
        string page_token
    }
    FileState {
        int id PK
        int sync_pair_id FK
        string relative_path
        string drive_file_id UK
        string local_hash "MD5"
        string drive_md5
        datetime drive_modified_at
        string status
    }
    Conflict {
        int id PK
        int file_state_id FK
        string conflict_copy_name
        datetime detected_at
        bool is_resolved
    }
    SyncLog {
        int id PK
        int sync_pair_id FK
        int file_state_id FK "може бути NULL"
        string action
        string result
        datetime created_at
    }
    ExcludeRule {
        int id PK
        int sync_pair_id FK
        string pattern
        bool is_enabled
    }
    AppSettings {
        int id PK "завжди 1"
        int check_interval_sec
        bool autostart
    }
```

`AppSettings` — глобальні налаштування, тому в схемі вона не пов’язана з конкретною парою папок. OAuth-токени шифруються засобами Windows DPAPI.
