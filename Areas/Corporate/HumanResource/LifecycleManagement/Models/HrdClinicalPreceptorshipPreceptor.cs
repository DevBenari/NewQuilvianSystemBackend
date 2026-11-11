using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("HrdClinicalPreceptorshipPreceptor", Schema = "public")]
    public class HrdClinicalPreceptorshipPreceptor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PreceptorshipId { get; set; }

        [Required]
        public Guid PreceptorWorkforceProfileId { get; set; }

        [Required]
        public DateOnly AssignedFrom { get; set; }

        public DateOnly? AssignedUntil { get; set; }

        [Required]
        public Guid AssignedByUserId { get; set; }

        [MaxLength(500)]
        public string? ChangeReason { get; set; }

        public HrdClinicalPreceptorship? Preceptorship { get; set; }
        public MstWorkforceProfile? PreceptorWorkforceProfile { get; set; }
        public ApplicationUser? AssignedByUser { get; set; }
    }
}
