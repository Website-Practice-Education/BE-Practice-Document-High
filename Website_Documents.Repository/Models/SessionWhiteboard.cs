using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_whiteboard")]
public class SessionWhiteboard
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("element_type")]
    [StringLength(20)]
    public string ElementType { get; set; } = string.Empty;

    [Column("content")]
    public string? Content { get; set; }

    [Column("position_x")]
    public int PositionX { get; set; } = 0;

    [Column("position_y")]
    public int PositionY { get; set; } = 0;

    [Column("width")]
    public int? Width { get; set; }

    [Column("height")]
    public int? Height { get; set; }

    [Column("color")]
    [StringLength(20)]
    public string? Color { get; set; }

    [Column("font_size")]
    public int? FontSize { get; set; }

    [Column("layer_index")]
    public int LayerIndex { get; set; } = 0;

    [Column("is_locked")]
    public bool IsLocked { get; set; } = false;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
