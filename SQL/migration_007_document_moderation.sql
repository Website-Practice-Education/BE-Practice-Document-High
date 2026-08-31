-- Migration: Add moderation fields to shared_documents table
-- Description: Thêm các trường kiểm duyệt cho tài liệu chia sẻ

BEGIN;

-- Add moderation status field (pending = chờ duyệt, approved = đã duyệt, rejected = từ chối)
ALTER TABLE shared_documents ADD COLUMN IF NOT EXISTS moderation_status VARCHAR(20) DEFAULT 'pending';
ALTER TABLE shared_documents ADD COLUMN IF NOT EXISTS moderation_notes TEXT;
ALTER TABLE shared_documents ADD COLUMN IF NOT EXISTS moderated_by_user_id INTEGER;
ALTER TABLE shared_documents ADD COLUMN IF NOT EXISTS moderated_by_name VARCHAR(255);
ALTER TABLE shared_documents ADD COLUMN IF NOT EXISTS moderated_at TIMESTAMP WITH TIME ZONE;

-- Add foreign key for moderator
ALTER TABLE shared_documents 
    ADD CONSTRAINT fk_shared_documents_moderator 
    FOREIGN KEY (moderated_by_user_id) REFERENCES users(id) ON DELETE SET NULL;

-- Add constraint for moderation status values
ALTER TABLE shared_documents 
    ADD CONSTRAINT chk_moderation_status 
    CHECK (moderation_status IN ('pending', 'approved', 'rejected'));

-- Update existing records to set default status
UPDATE shared_documents SET moderation_status = 'approved' WHERE is_verified = TRUE AND moderation_status = 'pending';
UPDATE shared_documents SET moderation_status = 'pending' WHERE is_verified = FALSE AND moderation_status = 'pending';

-- Indexes for moderation queries
CREATE INDEX IF NOT EXISTS idx_shared_documents_moderation_status ON shared_documents(moderation_status);
CREATE INDEX IF NOT EXISTS idx_shared_documents_moderation_created ON shared_documents(created_at DESC) WHERE moderation_status = 'pending';

-- Comments for documentation
COMMENT ON COLUMN shared_documents.moderation_status IS 'Trạng thái kiểm duyệt: pending (chờ duyệt), approved (đã duyệt), rejected (từ chối)';
COMMENT ON COLUMN shared_documents.moderation_notes IS 'Ghi chú từ người kiểm duyệt (lý do từ chối, phản hồi, v.v.)';
COMMENT ON COLUMN shared_documents.moderated_by_user_id IS 'ID người kiểm duyệt';
COMMENT ON COLUMN shared_documents.moderated_by_name IS 'Tên người kiểm duyệt';
COMMENT ON COLUMN shared_documents.moderated_at IS 'Thời điểm kiểm duyệt';

COMMIT;
