-- Migration: Create call_sessions and call_participants tables
-- Date: 2026-08-30

-- Create call_sessions table
CREATE TABLE IF NOT EXISTS call_sessions (
    id BIGSERIAL PRIMARY KEY,
    space_id BIGINT NOT NULL REFERENCES study_spaces(id) ON DELETE CASCADE,
    initiator_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    call_type VARCHAR(20) DEFAULT 'audio',
    room_id VARCHAR(100) NOT NULL,
    status VARCHAR(20) DEFAULT 'active',
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL,
    max_participants INTEGER DEFAULT 10,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_call_sessions_space_id ON call_sessions(space_id);
CREATE INDEX IF NOT EXISTS idx_call_sessions_status ON call_sessions(status);
CREATE INDEX IF NOT EXISTS idx_call_sessions_room_id ON call_sessions(room_id);

-- Create call_participants table
CREATE TABLE IF NOT EXISTS call_participants (
    id BIGSERIAL PRIMARY KEY,
    call_session_id BIGINT NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    join_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    leave_time TIMESTAMP NULL,
    is_muted BOOLEAN DEFAULT FALSE,
    is_video_off BOOLEAN DEFAULT FALSE,
    is_screen_sharing BOOLEAN DEFAULT FALSE,
    connection_status VARCHAR(20) DEFAULT 'connected',
    peer_id VARCHAR(100) NULL
);

-- Create indexes for call_participants
CREATE INDEX IF NOT EXISTS idx_call_participants_session_id ON call_participants(call_session_id);
CREATE INDEX IF NOT EXISTS idx_call_participants_user_id ON call_participants(user_id);
CREATE INDEX IF NOT EXISTS idx_call_participants_active ON call_participants(call_session_id, leave_time) WHERE leave_time IS NULL;

-- Add comments
COMMENT ON TABLE call_sessions IS 'Stores audio/video call sessions for study spaces';
COMMENT ON TABLE call_participants IS 'Stores participants in call sessions';

COMMENT ON COLUMN call_sessions.call_type IS 'Type of call: audio or video';
COMMENT ON COLUMN call_sessions.status IS 'Call status: active, ended, missed';
COMMENT ON COLUMN call_sessions.room_id IS 'Unique room identifier for WebRTC signaling';

COMMENT ON COLUMN call_participants.is_muted IS 'Whether participant is muted';
COMMENT ON COLUMN call_participants.is_video_off IS 'Whether participant video is turned off';
COMMENT ON COLUMN call_participants.is_screen_sharing IS 'Whether participant is sharing screen';
COMMENT ON COLUMN call_participants.peer_id IS 'Unique peer identifier for WebRTC';
