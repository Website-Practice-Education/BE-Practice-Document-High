-- Migration: 007_forum_posts.sql
-- Description: Create Forum tables for community posts with images, likes, and comments

-- Create Forum Posts table
CREATE TABLE IF NOT EXISTS forum_posts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content VARCHAR(500) NOT NULL,
    image_url VARCHAR(2000),
    like_count INTEGER DEFAULT 0,
    comment_count INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Create Forum Comments table
CREATE TABLE IF NOT EXISTS forum_comments (
    id SERIAL PRIMARY KEY,
    post_id INTEGER NOT NULL REFERENCES forum_posts(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content VARCHAR(1000) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_deleted BOOLEAN DEFAULT FALSE
);

-- Create Forum Likes table
CREATE TABLE IF NOT EXISTS forum_likes (
    id SERIAL PRIMARY KEY,
    post_id INTEGER NOT NULL REFERENCES forum_posts(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(post_id, user_id)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_forum_posts_user_id ON forum_posts(user_id);
CREATE INDEX IF NOT EXISTS idx_forum_posts_created_at ON forum_posts(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_forum_posts_is_deleted ON forum_posts(is_deleted);
CREATE INDEX IF NOT EXISTS idx_forum_posts_like_count ON forum_posts(like_count DESC);

CREATE INDEX IF NOT EXISTS idx_forum_comments_post_id ON forum_comments(post_id);
CREATE INDEX IF NOT EXISTS idx_forum_comments_user_id ON forum_comments(user_id);
CREATE INDEX IF NOT EXISTS idx_forum_comments_created_at ON forum_comments(created_at DESC);

CREATE INDEX IF NOT EXISTS idx_forum_likes_post_id ON forum_likes(post_id);
CREATE INDEX IF NOT EXISTS idx_forum_likes_user_id ON forum_likes(user_id);

-- Add foreign key constraint name for forum_posts (if not already exists via EF)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_posts_user_id_fkey'
    ) THEN
        ALTER TABLE forum_posts 
        ADD CONSTRAINT forum_posts_user_id_fkey 
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
    END IF;
END $$;

-- Add foreign key constraints for comments
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_comments_post_id_fkey'
    ) THEN
        ALTER TABLE forum_comments 
        ADD CONSTRAINT forum_comments_post_id_fkey 
        FOREIGN KEY (post_id) REFERENCES forum_posts(id) ON DELETE CASCADE;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_comments_user_id_fkey'
    ) THEN
        ALTER TABLE forum_comments 
        ADD CONSTRAINT forum_comments_user_id_fkey 
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
    END IF;
END $$;

-- Add foreign key constraints for likes
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_likes_post_id_fkey'
    ) THEN
        ALTER TABLE forum_likes 
        ADD CONSTRAINT forum_likes_post_id_fkey 
        FOREIGN KEY (post_id) REFERENCES forum_posts(id) ON DELETE CASCADE;
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_likes_user_id_fkey'
    ) THEN
        ALTER TABLE forum_likes 
        ADD CONSTRAINT forum_likes_user_id_fkey 
        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
    END IF;
END $$;

-- Add unique constraint for likes
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'forum_likes_post_id_user_id_key'
    ) THEN
        ALTER TABLE forum_likes 
        ADD CONSTRAINT forum_likes_post_id_user_id_key 
        UNIQUE (post_id, user_id);
    END IF;
END $$;

COMMENT ON TABLE forum_posts IS 'Community forum posts with text and image content';
COMMENT ON TABLE forum_comments IS 'Comments on forum posts';
COMMENT ON TABLE forum_likes IS 'Like records for forum posts';
