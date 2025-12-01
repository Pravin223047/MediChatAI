using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum LabResultStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    Rejected
}

public enum LabResultAbnormality
{
    Normal,
    SlightlyAbnormal,
    Abnormal,
    Critical
}

public class LabResult
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string PatientId { get; set; } = string.Empty;

    public ApplicationUser? Patient { get; set; }

    [MaxLength(450)]
    public string? OrderedByDoctorId { get; set; }

    public ApplicationUser? OrderedByDoctor { get; set; }

    [Required]
    [MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TestType { get; set; }

    [MaxLength(200)]
    public string? LabName { get; set; }

    [MaxLength(500)]
    public string? LabAddress { get; set; }

    public LabResultStatus Status { get; set; } = LabResultStatus.Pending;

    public DateTime? OrderedAt { get; set; }

    public DateTime? CollectionDate { get; set; }

    public DateTime? ResultDate { get; set; }

    [MaxLength(100)]
    public string? Value { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(100)]
    public string? ReferenceRange { get; set; }

    public LabResultAbnormality Abnormality { get; set; } = LabResultAbnormality.Normal;

    public bool IsCritical { get; set; } = false;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(2000)]
    public string? Interpretation { get; set; }

    /// <summary>
    /// URL to the PDF or image of the lab report
    /// </summary>
    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(255)]
    public string? FileName { get; set; }

    [MaxLength(100)]
    public string? FileMimeType { get; set; }

    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Category like CBC, Lipid Panel, Liver Function, etc.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsReviewed { get; set; } = false;

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(450)]
    public string? ReviewedByDoctorId { get; set; }

    public ApplicationUser? ReviewedByDoctor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Additional test parameters stored as JSON
    /// For complex tests with multiple values
    /// </summary>
    [MaxLength(4000)]
    public string? TestParameters { get; set; }
}
