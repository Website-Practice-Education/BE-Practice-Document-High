-- =====================================================
-- Migration: Thêm bảng Study Spaces (Phòng học nhóm)
-- Created: 2026-08-30
-- =====================================================

-- =====================================================
-- Table: study_spaces (Phòng học nhóm)
-- =====================================================
CREATE TABLE IF NOT EXISTS study_spaces (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    space_type VARCHAR(20) DEFAULT 'public',
    invite_code VARCHAR(20) UNIQUE,
    max_members INTEGER DEFAULT 50,
    is_active BOOLEAN DEFAULT TRUE,
    created_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_study_spaces_created_by ON study_spaces(created_by);
CREATE INDEX IF NOT EXISTS idx_study_spaces_invite_code ON study_spaces(invite_code);

-- =====================================================
-- Table: study_space_members (Thành viên phòng học)
-- =====================================================
CREATE TABLE IF NOT EXISTS study_space_members (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL REFERENCES study_spaces(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role VARCHAR(20) DEFAULT 'member',
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(space_id, user_id)
);

CREATE INDEX IF NOT EXISTS idx_study_space_members_space_id ON study_space_members(space_id);
CREATE INDEX IF NOT EXISTS idx_study_space_members_user_id ON study_space_members(user_id);

-- =====================================================
-- Table: chat_messages (Tin nhắn chat)
-- =====================================================
CREATE TABLE IF NOT EXISTS chat_messages (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL REFERENCES study_spaces(id) ON DELETE CASCADE,
    user_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    content TEXT NOT NULL,
    message_type VARCHAR(20) DEFAULT 'text',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_chat_messages_space_id ON chat_messages(space_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_user_id ON chat_messages(user_id);

-- =====================================================
-- Table: friendships (Bạn bè)
-- =====================================================
CREATE TABLE IF NOT EXISTS friendships (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    friend_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, friend_id)
);

CREATE INDEX IF NOT EXISTS idx_friendships_user_id ON friendships(user_id);
CREATE INDEX IF NOT EXISTS idx_friendships_friend_id ON friendships(friend_id);
CREATE INDEX IF NOT EXISTS idx_friendships_status ON friendships(status);

-- =====================================================
-- DONE!
-- =====================================================
