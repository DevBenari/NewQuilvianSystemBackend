using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstQueueVoiceProfile", Schema = "public")]
    public class MstQueueVoiceProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(80)]
        public string VoiceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string VoiceName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = "Female";

        [MaxLength(20)]
        public string Language { get; set; } = "id-ID";

        [Required]
        [MaxLength(500)]
        public string ModelPath { get; set; } = string.Empty;

        public decimal LengthScale { get; set; } = 1.08m;
        public decimal NoiseScale { get; set; } = 0.65m;
        public decimal NoiseW { get; set; } = 0.80m;
        public decimal Volume { get; set; } = 1.15m;
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

}
