namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IMedicalTermService
{
    string? GetExplanation(string term);
    string AnnotateMessage(string message);
}

public class MedicalTermService : IMedicalTermService
{
    private readonly Dictionary<string, string> _medicalTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common medical terms
        { "hypertension", "High blood pressure - when blood pushes against artery walls too hard" },
        { "diabetes", "Condition where blood sugar levels are too high" },
        { "prescription", "Doctor's written order for medication" },
        { "symptoms", "Physical or mental signs that indicate a disease or condition" },
        { "diagnosis", "Identification of a disease or condition" },
        { "mg", "Milligrams - a unit of measurement for medication" },
        { "ml", "Milliliters - a unit of liquid measurement" },
        { "CBC", "Complete Blood Count - blood test measuring different blood cells" },
        { "ECG", "Electrocardiogram - test measuring heart's electrical activity" },
        { "MRI", "Magnetic Resonance Imaging - detailed body imaging scan" },
        { "CT scan", "Computed Tomography - detailed X-ray imaging" },
        { "inflammation", "Body's response to injury or infection, causing redness and swelling" },
        { "chronic", "Long-lasting or recurring medical condition" },
        { "acute", "Sudden and severe onset of symptoms" },
        { "benign", "Not harmful or cancerous" },
        { "malignant", "Cancerous or harmful" },
        { "antibiotic", "Medicine that fights bacterial infections" },
        { "vaccine", "Substance that helps body build immunity against diseases" },
        { "allergy", "Body's immune system overreaction to a substance" },
        { "asthma", "Lung condition causing breathing difficulties" },
        { "arthritis", "Joint inflammation causing pain and stiffness" },
        { "migraine", "Severe headache often with nausea and light sensitivity" },
        { "cholesterol", "Waxy substance in blood that can clog arteries if too high" },
        { "glucose", "Blood sugar - main energy source for body" },
        { "insulin", "Hormone that regulates blood sugar levels" },
        { "biopsy", "Removal of tissue sample for examination" },
        { "anemia", "Low red blood cell count causing fatigue" },
        { "BP", "Blood Pressure" },
        { "HR", "Heart Rate" },
        { "temp", "Temperature" }
    };

    public string? GetExplanation(string term)
    {
        return _medicalTerms.TryGetValue(term.Trim(), out var explanation) ? explanation : null;
    }

    public string AnnotateMessage(string message)
    {
        foreach (var term in _medicalTerms.Keys)
        {
            var pattern = $@"\b{term}\b";
            message = System.Text.RegularExpressions.Regex.Replace(
                message,
                pattern,
                $"<span class='medical-term' data-term='{term}' title='{_medicalTerms[term]}'>{term}</span>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }
        return message;
    }
}
