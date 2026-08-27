-- =====================================================
-- Migration: Thêm bảng Collaborative Study Rooms
-- Feature: Học cùng nhau trong thời gian thực
-- =====================================================

-- =====================================================
-- Table: live_study_sessions (Phòng học trực tuyến)
-- =====================================================
CREATE TABLE IF NOT EXISTS live_study_sessions (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT REFERENCES study_spaces(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    session_type VARCHAR(30) DEFAULT 'practice', -- practice, quiz_battle, review
    subject_id INTEGER REFERENCES subjects(id) ON DELETE SET NULL,
    topic_id INTEGER REFERENCES topics(id) ON DELETE SET NULL,
    difficulty_level SMALLINT DEFAULT 1,
    question_count INTEGER DEFAULT 10,
    time_limit_minutes INTEGER DEFAULT 30,
    status VARCHAR(20) DEFAULT 'waiting', -- waiting, in_progress, completed, cancelled
    max_participants INTEGER DEFAULT 20,
    current_participants INTEGER DEFAULT 0,
    invite_code VARCHAR(20),
    host_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    started_at TIMESTAMP WITH TIME ZONE,
    ended_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CONSTRAINT live_study_sessions_difficulty_check CHECK (difficulty_level >= 1 AND difficulty_level <= 5)
);

-- =====================================================
-- Table: live_session_members (Thành viên phòng học)
-- =====================================================
CREATE TABLE IF NOT EXISTS live_session_members (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role VARCHAR(20) DEFAULT 'participant', -- host, moderator, participant
    status VARCHAR(20) DEFAULT 'joined', -- joined, active, idle, left
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    left_at TIMESTAMP WITH TIME ZONE,
    questions_answered INTEGER DEFAULT 0,
    correct_answers INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    current_streak INTEGER DEFAULT 0,
    is_ready BOOLEAN DEFAULT FALSE,
    last_activity_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(session_id, user_id)
);

-- =====================================================
-- Table: session_activities (Hoạt động trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_activities (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    activity_type VARCHAR(30) NOT NULL, -- question_shown, answer_submitted, correct_answer, streak, achievement
    description TEXT,
    metadata JSONB, -- Dữ liệu bổ sung dạng JSON
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_whiteboard (Bảng vẽ chia sẻ)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_whiteboard (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    element_type VARCHAR(20) NOT NULL, -- text, drawing, shape, image, math
    content TEXT, -- Nội dung text hoặc JSON cho drawing
    position_x INTEGER DEFAULT 0,
    position_y INTEGER DEFAULT 0,
    width INTEGER,
    height INTEGER,
    color VARCHAR(20),
    font_size INTEGER,
    layer_index INTEGER DEFAULT 0,
    is_locked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_chat_messages (Chat trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_chat_messages (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    message_type VARCHAR(20) DEFAULT 'text', -- text, emoji, image, system
    reply_to_id BIGINT REFERENCES session_chat_messages(id) ON DELETE SET NULL,
    is_pinned BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_shared_questions (Câu hỏi được chia sẻ)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_shared_questions (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    shared_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    shared_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    order_index INTEGER DEFAULT 0,
    is_current BOOLEAN DEFAULT FALSE,
    UNIQUE(session_id, question_id)
);

-- =====================================================
-- Table: session_participant_answers (Câu trả lời trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_participant_answers (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    selected_option_id BIGINT REFERENCES question_options(id) ON DELETE SET NULL,
    selected_letter CHAR(1),
    answer_text TEXT,
    is_correct BOOLEAN,
    time_spent_seconds INTEGER DEFAULT 0,
    points_earned INTEGER DEFAULT 0,
    answered_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(session_id, user_id, question_id)
);

-- =====================================================
-- Table: session_leaderboard (Bảng xếp hạng trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_leaderboard (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    rank_position INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    questions_correct INTEGER DEFAULT 0,
    total_questions INTEGER DEFAULT 0,
    average_time_seconds INTEGER DEFAULT 0,
    fastest_answer_seconds INTEGER,
    longest_streak INTEGER DEFAULT 0,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(session_id, user_id)
);

-- =====================================================
-- Table: session_invitations (Lời mời tham gia session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_invitations (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL REFERENCES live_study_sessions(id) ON DELETE CASCADE,
    invited_by BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    invited_user_id BIGINT REFERENCES users(id) ON DELETE CASCADE,
    invite_code VARCHAR(20),
    status VARCHAR(20) DEFAULT 'pending', -- pending, accepted, declined, expired
    expires_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    responded_at TIMESTAMP WITH TIME ZONE
);

-- =====================================================
-- INDEXES
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_live_sessions_space ON live_study_sessions(space_id);
CREATE INDEX IF NOT EXISTS idx_live_sessions_status ON live_study_sessions(status);
CREATE INDEX IF NOT EXISTS idx_live_sessions_host ON live_study_sessions(host_id);
CREATE INDEX IF NOT EXISTS idx_live_sessions_invite_code ON live_study_sessions(invite_code);
CREATE INDEX IF NOT EXISTS idx_live_sessions_subject ON live_study_sessions(subject_id);

CREATE INDEX IF NOT EXISTS idx_session_members_session ON live_session_members(session_id);
CREATE INDEX IF NOT EXISTS idx_session_members_user ON live_session_members(user_id);
CREATE INDEX IF NOT EXISTS idx_session_members_status ON live_session_members(status);

CREATE INDEX IF NOT EXISTS idx_session_activities_session ON session_activities(session_id);
CREATE INDEX IF NOT EXISTS idx_session_activities_user ON session_activities(user_id);
CREATE INDEX IF NOT EXISTS idx_session_activities_type ON session_activities(activity_type);

CREATE INDEX IF NOT EXISTS idx_whiteboard_session ON session_whiteboard(session_id);
CREATE INDEX IF NOT EXISTS idx_whiteboard_user ON session_whiteboard(user_id);

CREATE INDEX IF NOT EXISTS idx_session_chat_session ON session_chat_messages(session_id);
CREATE INDEX IF NOT EXISTS idx_session_chat_user ON session_chat_messages(user_id);

CREATE INDEX IF NOT EXISTS idx_shared_questions_session ON session_shared_questions(session_id);
CREATE INDEX IF NOT EXISTS idx_shared_questions_question ON session_shared_questions(question_id);

CREATE INDEX IF NOT EXISTS idx_participant_answers_session ON session_participant_answers(session_id);
CREATE INDEX IF NOT EXISTS idx_participant_answers_user ON session_participant_answers(user_id);

CREATE INDEX IF NOT EXISTS idx_leaderboard_session ON session_leaderboard(session_id);
CREATE INDEX IF NOT EXISTS idx_leaderboard_user ON session_leaderboard(user_id);

CREATE INDEX IF NOT EXISTS idx_session_invitations_session ON session_invitations(session_id);
CREATE INDEX IF NOT EXISTS idx_session_invitations_code ON session_invitations(invite_code);

-- =====================================================
-- DONE!
-- =====================================================
