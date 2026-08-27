-- =====================================================
-- Migration: Thêm bảng Collaborative Study Rooms
-- Feature: Học cùng nhau trong thời gian thực
-- =====================================================

-- =====================================================
-- Table: live_study_sessions (Phòng học trực tuyến)
-- =====================================================
CREATE TABLE IF NOT EXISTS live_study_sessions (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    session_type VARCHAR(30) DEFAULT 'practice',
    subject_id INTEGER,
    topic_id INTEGER,
    difficulty_level SMALLINT DEFAULT 1,
    question_count INTEGER DEFAULT 10,
    time_limit_minutes INTEGER DEFAULT 30,
    status VARCHAR(20) DEFAULT 'waiting',
    max_participants INTEGER DEFAULT 20,
    current_participants INTEGER DEFAULT 0,
    invite_code VARCHAR(20),
    host_id BIGINT,
    started_at TIMESTAMP WITH TIME ZONE,
    ended_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: live_session_members (Thành viên phòng học)
-- =====================================================
CREATE TABLE IF NOT EXISTS live_session_members (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    role VARCHAR(20) DEFAULT 'participant',
    status VARCHAR(20) DEFAULT 'joined',
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    left_at TIMESTAMP WITH TIME ZONE,
    questions_answered INTEGER DEFAULT 0,
    correct_answers INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    current_streak INTEGER DEFAULT 0,
    is_ready BOOLEAN DEFAULT FALSE,
    last_activity_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_activities (Hoạt động trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_activities (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    user_id BIGINT,
    activity_type VARCHAR(30) NOT NULL,
    description TEXT,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_whiteboard (Bảng vẽ chia sẻ)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_whiteboard (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    element_type VARCHAR(20) NOT NULL,
    content TEXT,
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
    session_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    content TEXT NOT NULL,
    message_type VARCHAR(20) DEFAULT 'text',
    reply_to_id BIGINT,
    is_pinned BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_shared_questions (Câu hỏi được chia sẻ)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_shared_questions (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    question_id BIGINT NOT NULL,
    shared_by BIGINT,
    shared_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    order_index INTEGER DEFAULT 0,
    is_current BOOLEAN DEFAULT FALSE
);

-- =====================================================
-- Table: session_participant_answers (Câu trả lời trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_participant_answers (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    question_id BIGINT NOT NULL,
    selected_option_id BIGINT,
    selected_letter CHAR(1),
    answer_text TEXT,
    is_correct BOOLEAN,
    time_spent_seconds INTEGER DEFAULT 0,
    points_earned INTEGER DEFAULT 0,
    answered_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_leaderboard (Bảng xếp hạng trong session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_leaderboard (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    rank_position INTEGER DEFAULT 0,
    total_score INTEGER DEFAULT 0,
    questions_correct INTEGER DEFAULT 0,
    total_questions INTEGER DEFAULT 0,
    average_time_seconds INTEGER DEFAULT 0,
    fastest_answer_seconds INTEGER,
    longest_streak INTEGER DEFAULT 0,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- =====================================================
-- Table: session_invitations (Lời mời tham gia session)
-- =====================================================
CREATE TABLE IF NOT EXISTS session_invitations (
    id BIGSERIAL PRIMARY KEY,
    session_id BIGINT NOT NULL,
    invited_by BIGINT NOT NULL,
    invited_user_id BIGINT,
    invite_code VARCHAR(20),
    status VARCHAR(20) DEFAULT 'pending',
    expires_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    responded_at TIMESTAMP WITH TIME ZONE
);

-- =====================================================
-- Add Foreign Keys
-- =====================================================
ALTER TABLE live_study_sessions ADD CONSTRAINT fk_live_sessions_space 
    FOREIGN KEY (space_id) REFERENCES study_spaces(id) ON DELETE CASCADE;
ALTER TABLE live_study_sessions ADD CONSTRAINT fk_live_sessions_subject 
    FOREIGN KEY (subject_id) REFERENCES subjects(id) ON DELETE SET NULL;
ALTER TABLE live_study_sessions ADD CONSTRAINT fk_live_sessions_topic 
    FOREIGN KEY (topic_id) REFERENCES topics(id) ON DELETE SET NULL;
ALTER TABLE live_study_sessions ADD CONSTRAINT fk_live_sessions_host 
    FOREIGN KEY (host_id) REFERENCES users(id) ON DELETE SET NULL;
ALTER TABLE live_study_sessions ADD CONSTRAINT chk_live_sessions_difficulty 
    CHECK (difficulty_level >= 1 AND difficulty_level <= 5);

ALTER TABLE live_session_members ADD CONSTRAINT fk_session_members_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE live_session_members ADD CONSTRAINT fk_session_members_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
ALTER TABLE live_session_members ADD CONSTRAINT uq_session_members 
    UNIQUE (session_id, user_id);

ALTER TABLE session_activities ADD CONSTRAINT fk_session_activities_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_activities ADD CONSTRAINT fk_session_activities_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL;

ALTER TABLE session_whiteboard ADD CONSTRAINT fk_whiteboard_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_whiteboard ADD CONSTRAINT fk_whiteboard_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

ALTER TABLE session_chat_messages ADD CONSTRAINT fk_session_chat_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_chat_messages ADD CONSTRAINT fk_session_chat_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
ALTER TABLE session_chat_messages ADD CONSTRAINT fk_session_chat_reply 
    FOREIGN KEY (reply_to_id) REFERENCES session_chat_messages(id) ON DELETE SET NULL;

ALTER TABLE session_shared_questions ADD CONSTRAINT fk_shared_questions_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_shared_questions ADD CONSTRAINT fk_shared_questions_question 
    FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE;
ALTER TABLE session_shared_questions ADD CONSTRAINT fk_shared_questions_shared_by 
    FOREIGN KEY (shared_by) REFERENCES users(id) ON DELETE SET NULL;
ALTER TABLE session_shared_questions ADD CONSTRAINT uq_shared_questions 
    UNIQUE (session_id, question_id);

ALTER TABLE session_participant_answers ADD CONSTRAINT fk_participant_answers_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_participant_answers ADD CONSTRAINT fk_participant_answers_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
ALTER TABLE session_participant_answers ADD CONSTRAINT fk_participant_answers_question 
    FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE;
ALTER TABLE session_participant_answers ADD CONSTRAINT fk_participant_answers_option 
    FOREIGN KEY (selected_option_id) REFERENCES question_options(id) ON DELETE SET NULL;
ALTER TABLE session_participant_answers ADD CONSTRAINT uq_participant_answers 
    UNIQUE (session_id, user_id, question_id);

ALTER TABLE session_leaderboard ADD CONSTRAINT fk_leaderboard_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_leaderboard ADD CONSTRAINT fk_leaderboard_user 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
ALTER TABLE session_leaderboard ADD CONSTRAINT uq_leaderboard 
    UNIQUE (session_id, user_id);

ALTER TABLE session_invitations ADD CONSTRAINT fk_session_invitations_session 
    FOREIGN KEY (session_id) REFERENCES live_study_sessions(id) ON DELETE CASCADE;
ALTER TABLE session_invitations ADD CONSTRAINT fk_session_invitations_invited_by 
    FOREIGN KEY (invited_by) REFERENCES users(id) ON DELETE CASCADE;
ALTER TABLE session_invitations ADD CONSTRAINT fk_session_invitations_invited_user 
    FOREIGN KEY (invited_user_id) REFERENCES users(id) ON DELETE CASCADE;

-- =====================================================
-- Add Indexes
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
