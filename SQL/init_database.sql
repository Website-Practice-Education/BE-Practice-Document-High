-- =====================================================
-- SQL Script tạo Database và Tables cho Website_Documents
-- Chạy trong pgAdmin hoặc psql
-- =====================================================

-- Tạo Database (nếu chưa có)
CREATE DATABASE bookstore_db;

-- Kết nối vào database
\c bookstore_db;

-- =====================================================
-- Table: subjects (Môn học)
-- =====================================================
CREATE TABLE IF NOT EXISTS subjects (
    id SERIAL PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: users (Người dùng)
-- =====================================================
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(150),
    role VARCHAR(20) DEFAULT 'student',
    grade SMALLINT,
    avatar_url TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP
);

-- =====================================================
-- Table: exams (Đề thi)
-- =====================================================
CREATE TABLE IF NOT EXISTS exams (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    subject_id INTEGER REFERENCES subjects(id) ON DELETE SET NULL,
    description TEXT,
    duration_minutes INTEGER DEFAULT 60,
    total_questions INTEGER DEFAULT 0,
    year SMALLINT,
    exam_type VARCHAR(50),
    is_timed BOOLEAN DEFAULT TRUE,
    allow_pause BOOLEAN DEFAULT FALSE,
    show_timer BOOLEAN DEFAULT TRUE,
    is_public BOOLEAN DEFAULT FALSE,
    created_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: questions (Câu hỏi)
-- =====================================================
CREATE TABLE IF NOT EXISTS questions (
    id SERIAL PRIMARY KEY,
    subject_id INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
    topic_id INTEGER,
    lesson_id INTEGER,
    question_type VARCHAR(20) NOT NULL DEFAULT 'multiple_choice',
    content TEXT NOT NULL,
    explanation TEXT,
    difficulty SMALLINT DEFAULT 1,
    year SMALLINT,
    source VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: exam_questions (Liên kết Đề thi - Câu hỏi)
-- =====================================================
CREATE TABLE IF NOT EXISTS exam_questions (
    id SERIAL PRIMARY KEY,
    exam_id BIGINT NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    order_number INTEGER DEFAULT 0
);

-- =====================================================
-- Table: question_options (Các lựa chọn cho câu hỏi)
-- =====================================================
CREATE TABLE IF NOT EXISTS question_options (
    id SERIAL PRIMARY KEY,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    option_key VARCHAR(5) NOT NULL,
    option_text TEXT NOT NULL,
    is_correct BOOLEAN DEFAULT FALSE
);

-- =====================================================
-- Table: user_attempts (Lượt thi của người dùng)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_attempts (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    exam_id BIGINT NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    subject_id INTEGER REFERENCES subjects(id) ON DELETE SET NULL,
    score DECIMAL(5,2),
    total_correct INTEGER DEFAULT 0,
    total_questions INTEGER DEFAULT 0,
    status VARCHAR(20) DEFAULT 'in_progress',
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    submitted_at TIMESTAMP,
    time_spent_seconds INTEGER DEFAULT 0
);

-- =====================================================
-- Table: user_answers (Câu trả lời của người dùng)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_answers (
    id SERIAL PRIMARY KEY,
    attempt_id BIGINT NOT NULL REFERENCES user_attempts(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    selected_option_id BIGINT REFERENCES question_options(id),
    is_correct BOOLEAN,
    answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: notifications (Thông báo)
-- =====================================================
CREATE TABLE IF NOT EXISTS notifications (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: achievements (Thành tích)
-- =====================================================
CREATE TABLE IF NOT EXISTS achievements (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    icon_url TEXT,
    criteria_type VARCHAR(50),
    criteria_value INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: user_achievements (Thành tích của người dùng)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_achievements (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    achievement_id BIGINT NOT NULL REFERENCES achievements(id) ON DELETE CASCADE,
    earned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, achievement_id)
);

-- =====================================================
-- Table: topics (Chủ đề)
-- =====================================================
CREATE TABLE IF NOT EXISTS topics (
    id SERIAL PRIMARY KEY,
    subject_id INTEGER NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    order_number INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: lessons (Bài học)
-- =====================================================
CREATE TABLE IF NOT EXISTS lessons (
    id SERIAL PRIMARY KEY,
    topic_id INTEGER NOT NULL REFERENCES topics(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    content TEXT,
    order_number INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: user_daily_progress (Tiến độ hàng ngày)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_daily_progress (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    questions_answered INTEGER DEFAULT 0,
    correct_answers INTEGER DEFAULT 0,
    time_spent_minutes INTEGER DEFAULT 0,
    UNIQUE(user_id, date)
);

-- =====================================================
-- Table: user_lesson_progress (Tiến độ bài học)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_lesson_progress (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    lesson_id INTEGER NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'not_started',
    completed_at TIMESTAMP,
    UNIQUE(user_id, lesson_id)
);

-- =====================================================
-- Table: user_topic_progress (Tiến độ chủ đề)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_topic_progress (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    topic_id INTEGER NOT NULL REFERENCES topics(id) ON DELETE CASCADE,
    status VARCHAR(20) DEFAULT 'not_started',
    UNIQUE(user_id, topic_id)
);

-- =====================================================
-- Table: user_bookmarks (Đánh dấu của người dùng)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_bookmarks (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    note TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, question_id)
);

-- =====================================================
-- Table: question_comments (Bình luận câu hỏi)
-- =====================================================
CREATE TABLE IF NOT EXISTS question_comments (
    id SERIAL PRIMARY KEY,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: user_answer_history (Lịch sử trả lời)
-- =====================================================
CREATE TABLE IF NOT EXISTS user_answer_history (
    id SERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    question_id BIGINT NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    selected_option_id BIGINT REFERENCES question_options(id),
    is_correct BOOLEAN,
    answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Table: lesson_resources (Tài nguyên bài học)
-- =====================================================
CREATE TABLE IF NOT EXISTS lesson_resources (
    id SERIAL PRIMARY KEY,
    lesson_id INTEGER NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    resource_type VARCHAR(50),
    title VARCHAR(255),
    url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================
-- Tạo Indexes để tăng tốc truy vấn
-- =====================================================
CREATE INDEX IF NOT EXISTS idx_questions_difficulty ON questions(difficulty);
CREATE INDEX IF NOT EXISTS idx_questions_lesson ON questions(lesson_id);
CREATE INDEX IF NOT EXISTS idx_questions_subject ON questions(subject_id);
CREATE INDEX IF NOT EXISTS idx_questions_topic ON questions(topic_id);
CREATE INDEX IF NOT EXISTS idx_questions_year ON questions(year);
CREATE INDEX IF NOT EXISTS idx_user_attempts_user ON user_attempts(user_id);
CREATE INDEX IF NOT EXISTS idx_user_attempts_exam ON user_attempts(exam_id);

-- =====================================================
-- Insert dữ liệu mẫu
-- =====================================================

-- Insert Subjects
INSERT INTO subjects (code, name, description) VALUES
('MATH', 'Toán học', 'Môn Toán dành cho học sinh'),
('PHY', 'Vật lý', 'Môn Vật lý dành cho học sinh'),
('CHEM', 'Hóa học', 'Môn Hóa học dành cho học sinh'),
('ENG', 'Tiếng Anh', 'Môn Tiếng Anh dành cho học sinh')
ON CONFLICT (code) DO NOTHING;

-- Insert Users (password: 123456)
INSERT INTO users (email, password_hash, full_name, role, grade) VALUES
('admin@example.com', '$2a$11$abcdefghijklmnopqrstuv', 'Admin User', 'admin', NULL),
('student@example.com', '$2a$11$abcdefghijklmnopqrstuv', 'Student User', 'student', 10)
ON CONFLICT (email) DO NOTHING;

-- =====================================================
-- DONE!
-- =====================================================
