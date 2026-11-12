using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public interface ITrainingCertificationService
    {
        Task<TrainingCertificateDto> IssueCertificateAsync(IssueCertificateRequestDto request, string currentUserId);
        Task<TrainingCertificateDto?> GetCertificateByNumberAsync(string certificateNumber);
        Task<List<TrainingCertificateDto>> GetCertificatesByParticipantAsync(Guid participantId);
        Task<List<TrainingCertificateDto>> GetCertificatesByStaffAsync(Guid workforceProfileId);
    }
}
