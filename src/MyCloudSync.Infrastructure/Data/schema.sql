CREATE TABLE IF NOT EXISTS Account (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    email            TEXT    NOT NULL UNIQUE,
    display_name     TEXT,
    access_token     TEXT    NOT NULL,
    refresh_token    TEXT    NOT NULL,
    token_expires_at TEXT,
    created_at       TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ','now'))
);

CREATE TABLE IF NOT EXISTS SyncPair (
    id                INTEGER PRIMARY KEY AUTOINCREMENT,
    account_id        INTEGER NOT NULL REFERENCES Account(id) ON DELETE CASCADE,
    local_path        TEXT    NOT NULL,
    drive_folder_id   TEXT    NOT NULL,
    drive_folder_name TEXT,
    sync_mode         TEXT    NOT NULL DEFAULT 'TwoWay'
                      CHECK (sync_mode IN ('TwoWay', 'UploadOnly', 'DownloadOnly')),
    is_active         INTEGER NOT NULL DEFAULT 1 CHECK (is_active IN (0, 1)),
    page_token        TEXT,
    last_sync_at      TEXT,
    created_at        TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ','now')),
    UNIQUE (account_id, local_path)
);

CREATE TABLE IF NOT EXISTS FileState (
    id                INTEGER PRIMARY KEY AUTOINCREMENT,
    sync_pair_id      INTEGER NOT NULL REFERENCES SyncPair(id) ON DELETE CASCADE,
    relative_path     TEXT    NOT NULL,
    drive_file_id     TEXT    UNIQUE,
    is_folder         INTEGER NOT NULL DEFAULT 0 CHECK (is_folder IN (0, 1)),
    size_bytes        INTEGER,
    local_hash        TEXT,
    drive_md5         TEXT,
    local_modified_at TEXT,
    drive_modified_at TEXT,
    last_synced_at    TEXT,
    status            TEXT    NOT NULL DEFAULT 'Pending'
                      CHECK (status IN ('Synced', 'Pending', 'Error', 'Conflict', 'Skipped')),
    UNIQUE (sync_pair_id, relative_path)
);

CREATE TABLE IF NOT EXISTS Conflict (
    id                 INTEGER PRIMARY KEY AUTOINCREMENT,
    file_state_id      INTEGER NOT NULL REFERENCES FileState(id) ON DELETE CASCADE,
    conflict_copy_name TEXT    NOT NULL,
    local_modified_at  TEXT    NOT NULL,
    drive_modified_at  TEXT    NOT NULL,
    detected_at        TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ','now')),
    is_resolved        INTEGER NOT NULL DEFAULT 0 CHECK (is_resolved IN (0, 1))
);

CREATE TABLE IF NOT EXISTS SyncLog (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    sync_pair_id  INTEGER NOT NULL REFERENCES SyncPair(id) ON DELETE CASCADE,
    file_state_id INTEGER REFERENCES FileState(id) ON DELETE SET NULL,
    action        TEXT    NOT NULL
                  CHECK (action IN ('Upload', 'Download', 'Delete', 'Rename', 'Conflict', 'Skip', 'Error', 'Info')),
    direction     TEXT    CHECK (direction IN ('ToCloud', 'FromCloud')),
    result        TEXT    NOT NULL CHECK (result IN ('Success', 'Warning', 'Error')),
    message       TEXT,
    created_at    TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ','now'))
);

CREATE TABLE IF NOT EXISTS ExcludeRule (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    sync_pair_id INTEGER NOT NULL REFERENCES SyncPair(id) ON DELETE CASCADE,
    pattern      TEXT    NOT NULL,
    is_enabled   INTEGER NOT NULL DEFAULT 1 CHECK (is_enabled IN (0, 1)),
    UNIQUE (sync_pair_id, pattern)
);

CREATE TABLE IF NOT EXISTS AppSettings (
    id                 INTEGER PRIMARY KEY CHECK (id = 1),
    check_interval_sec INTEGER NOT NULL DEFAULT 300 CHECK (check_interval_sec BETWEEN 60 AND 3600),
    autostart          INTEGER NOT NULL DEFAULT 0 CHECK (autostart IN (0, 1)),
    start_minimized    INTEGER NOT NULL DEFAULT 1 CHECK (start_minimized IN (0, 1)),
    show_notifications INTEGER NOT NULL DEFAULT 1 CHECK (show_notifications IN (0, 1)),
    language           TEXT    DEFAULT 'uk'
);

INSERT OR IGNORE INTO AppSettings (id) VALUES (1);
CREATE INDEX IF NOT EXISTS ix_FileState_status   ON FileState (sync_pair_id, status);
CREATE INDEX IF NOT EXISTS ix_SyncLog_created    ON SyncLog (sync_pair_id, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_Conflict_open      ON Conflict (is_resolved, detected_at DESC);
CREATE INDEX IF NOT EXISTS ix_SyncPair_account   ON SyncPair (account_id);
CREATE INDEX IF NOT EXISTS ix_ExcludeRule_pair   ON ExcludeRule (sync_pair_id);

PRAGMA user_version = 1;
