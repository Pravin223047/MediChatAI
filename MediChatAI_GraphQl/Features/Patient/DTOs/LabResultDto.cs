namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class LabResultDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = "";
    public string TestName { get; set; } = "";
    public string TestCategory { get; set; } = ""; // Blood Test, Urine Test, Imaging, etc.
    public DateTime TestDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string Status { get; set; } = ""; // Pending, Completed, Reviewed
    public string? OrderedByDoctorId { get; set; }
    public string? OrderedByDoctorName { get; set; }
    public List<LabTestParameter> Parameters { get; set; } = new();
    public string? DoctorComments { get; set; }
    public string? DocumentUrl { get; set; } // PDF/Image of lab report
    public bool IsAbnormal { get; set; }
}

public class LabTestParameter
{
    public string ParameterName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Unit { get; set; } = "";
    public string? ReferenceRange { get; set; }
    public string Status { get; set; } = "Normal"; // Low, Normal, High, Critical
    public bool IsCritical { get; set; }
}

public class GetLabResultsInput
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TestCategory { get; set; }
    public string? Status { get; set; }
    public int? Limit { get; set; } = 50;
    public int? Skip { get; set; } = 0;
}

public class UploadLabResultInput
{
    public string TestName { get; set; } = "";
    public string TestCategory { get; set; } = "";
    public DateTime TestDate { get; set; }
    public string DocumentUrl { get; set; } = "";
    public List<LabTestParameter>? Parameters { get; set; }
}
