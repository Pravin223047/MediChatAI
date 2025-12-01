namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class MedicationReminderDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = "";
    public int PrescriptionId { get; set; }
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public List<string> ReminderTimes { get; set; } = new(); // e.g., ["08:00", "14:00", "20:00"]
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<MedicationAdherenceLog> AdherenceLogs { get; set; } = new();
    public int TotalDoses { get; set; }
    public int DosesTaken { get; set; }
    public int DosesMissed { get; set; }
    public double AdherencePercentage { get; set; }
    public bool RefillAlertEnabled { get; set; }
    public int RefillAlertDaysBefore { get; set; } = 3;
}

public class MedicationAdherenceLog
{
    public int Id { get; set; }
    public int ReminderId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? ActualTakenTime { get; set; }
    public string Status { get; set; } = ""; // Taken, Missed, Skipped
    public string? Notes { get; set; }
}

public class CreateMedicationReminderInput
{
    public int PrescriptionId { get; set; }
    public List<string> ReminderTimes { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool RefillAlertEnabled { get; set; } = true;
    public int RefillAlertDaysBefore { get; set; } = 3;
}

public class UpdateMedicationAdherenceInput
{
    public int ReminderId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = ""; // Taken, Missed, Skipped
    public DateTime? ActualTakenTime { get; set; }
    public string? Notes { get; set; }
}

public class GetMedicationRemindersInput
{
    public bool? IsActive { get; set; }
    public DateTime? Date { get; set; } // Get reminders for specific date
}
