-- Migration: Room Features (Music, Files, Settings)
-- Created: 2026-08-30

-- Table: room_music_tracks
CREATE TABLE IF NOT EXISTS room_music_tracks (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL,
    title VARCHAR(255) NOT NULL,
    artist VARCHAR(255),
    source_type VARCHAR(20) NOT NULL DEFAULT 'upload',
    file_path VARCHAR(500),
    external_url VARCHAR(1000),
    duration_seconds INTEGER DEFAULT 0,
    uploaded_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_music_space FOREIGN KEY (space_id) REFERENCES study_spaces(id) ON DELETE CASCADE,
    CONSTRAINT fk_room_music_user FOREIGN KEY (uploaded_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_room_music_space_id ON room_music_tracks(space_id);
CREATE INDEX IF NOT EXISTS idx_room_music_uploaded_by ON room_music_tracks(uploaded_by);

-- Table: room_shared_files
CREATE TABLE IF NOT EXISTS room_shared_files (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    original_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    file_type VARCHAR(50) NOT NULL,
    uploaded_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_files_space FOREIGN KEY (space_id) REFERENCES study_spaces(id) ON DELETE CASCADE,
    CONSTRAINT fk_room_files_user FOREIGN KEY (uploaded_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_room_files_space_id ON room_shared_files(space_id);
CREATE INDEX IF NOT EXISTS idx_room_files_uploaded_by ON room_shared_files(uploaded_by);

-- Table: room_settings
CREATE TABLE IF NOT EXISTS room_settings (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL UNIQUE,
    background_type VARCHAR(20) NOT NULL DEFAULT 'theme',
    background_value VARCHAR(100),
    background_image_path VARCHAR(500),
    accent_color VARCHAR(20),
    updated_by BIGINT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_room_settings_space FOREIGN KEY (space_id) REFERENCES study_spaces(id) ON DELETE CASCADE,
    CONSTRAINT fk_room_settings_user FOREIGN KEY (updated_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_room_settings_space_id ON room_settings(space_id);

-- Grant permissions (adjust as needed)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_app_user;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO your_app_user;
