using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("room_music_tracks")]
public class RoomMusicTrack
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("artist")]
    [StringLength(255)]
    public string? Artist { get; set; }

    [Column("source_type")]
    [StringLength(20)]
    public string SourceType { get; set; } = "upload"; // "upload" or "link"

    [Column("file_path")]
    [StringLength(500)]
    public string? FilePath { get; set; }

    [Column("external_url")]
    [StringLength(1000)]
    public string? ExternalUrl { get; set; }

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("uploaded_by")]
    public long UploadedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("UploadedBy")]
    public virtual User? Uploader { get; set; }
}

[Table("room_shared_files")]
public class RoomSharedFile
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Column("original_name")]
    [StringLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    [Column("file_path")]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("content_type")]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Column("file_type")]
    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;

    [Column("uploaded_by")]
    public long UploadedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("UploadedBy")]
    public virtual User? Uploader { get; set; }
}

[Table("room_settings")]
public class RoomSetting
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("background_type")]
    [StringLength(20)]
    public string BackgroundType { get; set; } = "theme"; // "theme" or "custom"

    [Column("background_value")]
    [StringLength(100)]
    public string? BackgroundValue { get; set; } // Theme name or hex color

    [Column("background_image_path")]
    [StringLength(500)]
    public string? BackgroundImagePath { get; set; }

    [Column("accent_color")]
    [StringLength(20)]
    public string? AccentColor { get; set; }

    [Column("updated_by")]
    public long UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("UpdatedBy")]
    public virtual User? Updater { get; set; }
}
