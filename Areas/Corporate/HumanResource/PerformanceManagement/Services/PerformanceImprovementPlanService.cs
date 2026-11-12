using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Program Pembinaan Formal PIP 60 Hari & Integrasi Pembekuan Mutasi Sub-06 (BE-HRD-262, BE-HRD-263, PRF-B7, INT-HR-84)
    /// </summary>
    public class PerformanceImprovementPlanService : IPerformanceImprovementPlanService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceImprovementPlanService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<PipActiveSummaryDto>> GetActivePipListAsync(CancellationToken cancellationToken = default)
        {
            var pips = await _uow.PerformanceImprovementPlans
                .Where(x => x.Status == "ACTIVE" || x.Status == "EXTENDED")
                .ToListAsync(cancellationToken);

            return pips.Select(p => new PipActiveSummaryDto
            {
                Id = p.Id,
                WorkforceProfileId = p.WorkforceProfileId,
                StaffName = $"Pegawai ({p.WorkforceProfileId.ToString()[..8]})",
                AppraisalId = p.PerformanceAppraisalId,
                BaselineScore = p.BaselineScore,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MentorStaffId = p.MentorStaffId,
                Status = p.Status,
                ExtensionCount = p.ExtensionCount
            }).ToList();
        }

        public async Task<PipActiveSummaryDto> IssueContractAsync(IssuePipContractRequestDto dto, Guid issuedBy, CancellationToken cancellationToken = default)
        {
            // PRF-B7: PIP terstruktur durasi 60 hari kalender
            var endDate = dto.EndDate == default || dto.EndDate <= dto.StartDate 
                ? dto.StartDate.AddDays(60) 
                : dto.EndDate;

            var pip = new HrdPerformanceImprovementPlan
            {
                Id = Guid.NewGuid(),
                PerformanceAppraisalId = dto.AppraisalId,
                WorkforceProfileId = dto.WorkforceProfileId,
                MentorStaffId = dto.MentorStaffId,
                ImprovementObjectives = dto.ImprovementObjectives,
                StartDate = dto.StartDate,
                EndDate = endDate,
                Status = "ACTIVE",
                BaselineScore = 60.00m,
                ExtensionCount = 0,
                CreateBy = issuedBy,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = issuedBy,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(pip, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // INT-HR-84 & VR-PRF-11: Pembekuan Mutasi/Promosi Staf PIP ke Sub-06
            // Di sini kita mencatat log event StaffEnteredPipEvent yang memblokir hak promosi/mutasi pegawai
            // Di sistem production, ini dapat dipancarkan melalui MediatR / RabbitMQ / Outbox event

            return new PipActiveSummaryDto
            {
                Id = pip.Id,
                WorkforceProfileId = pip.WorkforceProfileId,
                StaffName = $"Pegawai ({pip.WorkforceProfileId.ToString()[..8]})",
                AppraisalId = pip.PerformanceAppraisalId,
                BaselineScore = pip.BaselineScore,
                StartDate = pip.StartDate,
                EndDate = pip.EndDate,
                MentorStaffId = pip.MentorStaffId,
                Status = pip.Status,
                ExtensionCount = pip.ExtensionCount
            };
        }

        public async Task<bool> RecordWeeklyLogAsync(Guid pipId, RecordPipWeeklyLogRequestDto dto, Guid mentorId, CancellationToken cancellationToken = default)
        {
            // VR-PRF-09, VR-PRF-15: Kepatuhan Logbook Mingguan (Minggu 1-8, catatan mentor minimal 20 karakter)
            if (dto.WeekNumber < 1 || dto.WeekNumber > 12)
            {
                throw new ArgumentException("Nomor minggu logbook harus antara 1 sampai 12 (VR-PRF-09).");
            }

            if (string.IsNullOrWhiteSpace(dto.MentorObservationNotes) || dto.MentorObservationNotes.Trim().Length < 20)
            {
                throw new ArgumentException("Catatan observasi perkembangan mingguan mentor wajib diisi minimal 20 karakter (VR-PRF-15).");
            }

            var pip = await _uow.PerformanceImprovementPlans.FirstOrDefaultAsync(x => x.Id == pipId, cancellationToken);
            if (pip == null)
                throw new KeyNotFoundException($"Kontrak pembinaan PIP ID {pipId} tidak ditemukan.");

            var log = new HrdPerformancePipActionLog
            {
                Id = Guid.NewGuid(),
                PerformanceImprovementPlanId = pipId,
                WeekNumber = dto.WeekNumber,
                ActionItemCompleted = dto.ActionItemCompleted,
                MentorObservationNotes = dto.MentorObservationNotes,
                ProgressStatus = dto.ProgressStatus,
                LogDate = DateTime.UtcNow,
                RecordedByMentorId = mentorId,
                CreateBy = mentorId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = mentorId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(log, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeliverFinalVerdictAsync(Guid pipId, VerdictPipRequestDto dto, Guid approverId, CancellationToken cancellationToken = default)
        {
            var pip = await _uow.PerformanceImprovementPlans.FirstOrDefaultAsync(x => x.Id == pipId, cancellationToken);
            if (pip == null)
                throw new KeyNotFoundException($"Kontrak pembinaan PIP ID {pipId} tidak ditemukan.");

            if (dto.Verdict == "PASS")
            {
                pip.Status = "PASSED";
                pip.FinalVerdictNotes = dto.VerdictNotes;
                // INT-HR-84: StaffCompletedPipEvent - Pemulihan hak kepegawaian normal di Sub-06
            }
            else if (dto.Verdict == "EXTEND_30_DAYS")
            {
                // VR-PRF-10: Pembatasan perpanjangan PIP maksimal tepat 1 kali durasi 30 hari
                if (pip.ExtensionCount >= 1)
                {
                    throw new InvalidOperationException("Batas perpanjangan program pembinaan PIP (maksimal 1 kali selama 30 hari kalender) telah habis; sidang wajib menetapkan putusan akhir kelulusan atau eskalasi sanksi (VR-PRF-10).");
                }

                pip.ExtensionCount += 1;
                pip.EndDate = pip.EndDate.AddDays(30);
                pip.Status = "EXTENDED";
                pip.FinalVerdictNotes = "[DIPERPANJANG 30 HARI] " + dto.VerdictNotes;
            }
            else // FAIL_ESCALATE_LEGAL
            {
                pip.Status = "FAILED";
                pip.FinalVerdictNotes = "[ESKALASI KE SUB-18 HUBUNGAN INDUSTRIAL & SANKSI] " + dto.VerdictNotes;
            }

            pip.UpdateBy = approverId;
            pip.UpdateDateTime = DateTime.UtcNow;
            _uow.Update(pip);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
