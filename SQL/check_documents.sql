-- ================================================
-- SQL CHECK: Kiểm tra tài liệu trong database
-- ================================================

-- 1. Kiểm tra cấu trúc bảng shared_documents (xem có thiếu cột moderation không)
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'shared_documents'
ORDER BY ordinal_position;

-- 2. Kiểm tra tất cả tài liệu (cả đã duyệt và chưa duyệt)
SELECT 
    id,
    title,
    document_type,
    file_url,
    is_active,
    is_verified,
    moderation_status,
    shared_by_name,
    created_at
FROM shared_documents
WHERE is_active = true
ORDER BY created_at DESC
LIMIT 50;

-- 3. Kiểm tra tài liệu theo trạng thái kiểm duyệt
SELECT 
    moderation_status,
    COUNT(*) as total
FROM shared_documents
WHERE is_active = true
GROUP BY moderation_status;

-- 4. Kiểm tra chi tiết tài liệu chờ duyệt (pending)
SELECT 
    id,
    title,
    description,
    document_type,
    file_url,
    file_type,
    file_size,
    subject_id,
    topic_id,
    question_count,
    grade_level,
    shared_by_user_id,
    shared_by_name,
    is_verified,
    moderation_status,
    moderation_notes,
    moderated_by_name,
    moderated_at,
    created_at,
    updated_at
FROM shared_documents
WHERE is_active = true 
    AND (moderation_status = 'pending' OR moderation_status IS NULL)
ORDER BY created_at ASC;

-- 5. Đếm tài liệu chờ duyệt
SELECT COUNT(*) as pending_count
FROM shared_documents
WHERE is_active = true 
    AND (moderation_status = 'pending' OR moderation_status IS NULL);

-- 6. Kiểm tra tài liệu của một user cụ thể
-- Thay 'admin@studyhub.com' bằng email cần check
SELECT 
    sd.id,
    sd.title,
    sd.document_type,
    sd.moderation_status,
    sd.created_at
FROM shared_documents sd
JOIN users u ON sd.shared_by_user_id = u.id
WHERE u.email = 'admin@studyhub.com'
ORDER BY sd.created_at DESC;
