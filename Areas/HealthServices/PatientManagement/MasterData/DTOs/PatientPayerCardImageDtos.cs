using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class UpdatePatientPayerCardImageRequest
    {
        /// <summary>
        /// Gambar kartu hasil scan (base64, boleh berawalan data URL).
        /// </summary>
        [Required]
        public string CardImageBase64 { get; set; } = string.Empty;
    }

    public class PatientPayerCardImageResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string? CardImagePath { get; set; }
    }
}
