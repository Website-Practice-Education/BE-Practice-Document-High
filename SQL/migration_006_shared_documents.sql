-- Migration: Create shared_documents table
-- Description: Bảng lưu trữ tài liệu chia sẻ công khai (file hoặc link)
-- Fixed: Use BIGINT for user_id columns to match User.Id type

BEGIN;

CREATE TABLE IF NOT EXISTS shared_documents (
    id BIGSERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    document_type VARCHAR(50) NOT NULL DEFAULT 'link', -- 'file' or 'link'
    file_url VARCHAR(1000),
    file_type VARCHAR(50),
    file_size BIGINT,
    
    -- Phân loại theo môn học, chủ đề, số lượng câu
    subject_id INTEGER,
    topic_id INTEGER,
    question_count INTEGER,
    grade_level INTEGER,
    
    -- Link metadata
    link_url VARCHAR(1000),
    link_source VARCHAR(100),
    
    -- Người chia sẻ - Changed to BIGINT
    shared_by_user_id BIGINT,
    shared_by_name VARCHAR(255),
    
    -- Thống kê
    view_count INTEGER DEFAULT 0,
    download_count INTEGER DEFAULT 0,
    like_count INTEGER DEFAULT 0,
    
    -- Trạng thái
    is_active BOOLEAN DEFAULT TRUE,
    is_verified BOOLEAN DEFAULT FALSE,
    
    -- Moderation fields - Added
    moderation_status VARCHAR(20) DEFAULT 'pending',
    moderation_notes TEXT,
    moderated_by_user_id BIGINT,
    moderated_by_name VARCHAR(255),
    moderated_at TIMESTAMP WITH TIME ZONE,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    -- Foreign keys
    CONSTRAINT fk_shared_documents_subject 
        FOREIGN KEY (subject_id) REFERENCES subjects(id) ON DELETE SET NULL,
    CONSTRAINT fk_shared_documents_topic 
        FOREIGN KEY (topic_id) REFERENCES topics(id) ON DELETE SET NULL,
    CONSTRAINT fk_shared_documents_user 
        FOREIGN KEY (shared_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT fk_shared_documents_moderator 
        FOREIGN KEY (moderated_by_user_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Indexes for better query performance
CREATE INDEX idx_shared_documents_subject ON shared_documents(subject_id);
CREATE INDEX idx_shared_documents_topic ON shared_documents(topic_id);
CREATE INDEX idx_shared_documents_question_count ON shared_documents(question_count);
CREATE INDEX idx_shared_documents_grade_level ON shared_documents(grade_level);
CREATE INDEX idx_shared_documents_document_type ON shared_documents(document_type);
CREATE INDEX idx_shared_documents_created_at ON shared_documents(created_at DESC);
CREATE INDEX idx_shared_documents_is_active ON shared_documents(is_active);
CREATE INDEX idx_shared_documents_shared_by ON shared_documents(shared_by_user_id);
CREATE INDEX idx_shared_documents_moderation ON shared_documents(moderation_status);

-- Add comments for documentation
COMMENT ON TABLE shared_documents IS 'Bảng lưu trữ tài liệu chia sẻ công khai (file hoặc link)';
COMMENT ON COLUMN shared_documents.document_type IS 'Loại: file (tải lên) hoặc link (chia sẻ đường dẫn)';
COMMENT ON COLUMN shared_documents.question_count IS 'Số lượng câu hỏi trong tài liệu';
COMMENT ON COLUMN shared_documents.link_source IS 'Nguồn link: Google Drive, Facebook, Zalo,...';
COMMENT ON COLUMN shared_documents.moderation_status IS 'Trạng thái kiểm duyệt: pending, approved, rejected';

COMMIT;
