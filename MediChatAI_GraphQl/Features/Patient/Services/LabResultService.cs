using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Patient.Services;

public class LabResultService : ILabResultService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LabResultService> _logger;

    public LabResultService(ApplicationDbContext context, ILogger<LabResultService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<LabResult>> GetPatientLabResultsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.LabResults
                .Where(lr => lr.PatientId == patientId);

            if (startDate.HasValue)
                query = query.Where(lr => lr.ResultDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(lr => lr.ResultDate <= endDate.Value);

            return await query
                .OrderByDescending(lr => lr.ResultDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab results for patient {PatientId}", patientId);
            throw;
        }
    }

    public Task<LabResult> UploadLabResultAsync(string patientId, UploadLabResultInput input)
    {
        // TODO: Implement when UploadLabResultInput DTO is properly defined
        throw new NotImplementedException("Lab result upload not yet implemented");
    }

    public async Task<bool> DeleteLabResultAsync(Guid labResultId, string patientId)
    {
        try
        {
            var labResult = await _context.LabResults
                .FirstOrDefaultAsync(lr => lr.Id == labResultId && lr.PatientId == patientId);

            if (labResult == null)
                return false;

            _context.LabResults.Remove(labResult);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab result {LabResultId}", labResultId);
            throw;
        }
    }

    public async Task<LabResult?> GetLabResultByIdAsync(Guid labResultId)
    {
        return await _context.LabResults
            .FirstOrDefaultAsync(lr => lr.Id == labResultId);
    }
}
