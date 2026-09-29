# Діаграми MyCloudSync

Діаграми вбудовані в Markdown, тому GitHub відображає їх без окремих програм чи розширень. Їхній текст можна редагувати прямо в цьому файлі.

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
        UI --> Core[Координатор синхронізації]
        Watcher[FileSystemWatcher і контрольне сканування] --> Core
        Core --> Adapter[Google Drive Adapter]
        Core --> Repo[SQLite Repository]
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
    participant Core as Координатор
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

```mermaid
erDiagram
    ACCOUNT ||--o{ SYNC_PAIR : має
    SYNC_PAIR ||--o{ EXCLUDE_RULE : налаштовує
    SYNC_PAIR ||--o{ FILE_STATE : відстежує
    FILE_STATE ||--o{ CONFLICT : фіксує
    SYNC_PAIR ||--o{ SYNC_LOG : журналює

    ACCOUNT {
        int account_id PK
        string email
        blob token_ciphertext
        datetime token_expires_at
    }
    SYNC_PAIR {
        int pair_id PK
        int account_id FK
        string local_path
        string drive_folder_id
        string sync_mode
        boolean enabled
    }
    EXCLUDE_RULE {
        int rule_id PK
        int pair_id FK
        string pattern
    }
    FILE_STATE {
        int file_state_id PK
        int pair_id FK
        string relative_path
        string drive_file_id
        string local_hash
        datetime remote_modified_at
        string sync_status
    }
    CONFLICT {
        int conflict_id PK
        int file_state_id FK
        datetime detected_at
        string local_copy_path
        string remote_file_id
        string resolution_status
    }
    SYNC_LOG {
        int log_id PK
        int pair_id FK
        datetime occurred_at
        string relative_path
        string action
        string result
    }
    APP_SETTINGS {
        int settings_id PK
        int check_interval_minutes
        boolean autostart
    }
```

`APP_SETTINGS` — глобальні налаштування, тому в схемі вона не пов’язана з конкретною парою папок. OAuth токен потрібно захистити засобами Windows DPAPI.
