using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    internal sealed record OnboardingContractSnapshot(
        Guid ContractId,
        string ContractNumber,
        string? ContractTypeName,
        string HistoryType,
        string ContractStatus,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? ProbationEndDate,
        bool IsProbationApplicable,
        string? DocumentPath,
        string? DocumentFileName,
        decimal? BaseSalaryAmount);

    internal sealed record OnboardingLiveFacts(
        OnboardingContractSnapshot? FirstContract,
        bool HasValidSignedContract,
        McuClearanceResult? McuResult,
        bool HasValidMcu,
        HashSet<Guid> ValidCertificationTypeIds,
        bool DirectSupervisorValid,
        string? DirectSupervisorInvalidReason,
        bool HasManagerConflict,
        string? ManagerConflictNote,
        Guid? ActiveManagerProfileId,
        string? ActiveManagerName,
        bool PreceptorValid,
        string? PreceptorInvalidReason,
        Guid? PreceptorshipId,
        PreceptorshipStatus? PreceptorshipStatusValue,
        Guid? CurrentPreceptorProfileId,
        string? CurrentPreceptorName,
        DateOnly? AccompaniedSince,
        DateOnly? IndependentOn,
        bool HasCredentialLicense,
        int EmployeeDataCompletedGroups,
        int EmployeeDataApplicableGroups);

    internal static class OnboardingCasePolicy
    {
        public static readonly TimeZoneInfo JakartaTimeZone = ResolveJakartaTimeZone();

        public static DateOnly OperationalToday()
        {
            var wibNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, JakartaTimeZone);
            return DateOnly.FromDateTime(wibNow.Date);
        }

        public static string MaskIdentityNumber(string? value)
        {
            var clean = value?.Trim();
            if (string.IsNullOrWhiteSpace(clean)) return "-";
            if (clean.Length <= 6) return new string('*', clean.Length);
            return string.Concat(clean.AsSpan(0, 4), new string('*', clean.Length - 8), clean.AsSpan(clean.Length - 4));
        }

        public static string MaskBankAccount(string? value)
        {
            var clean = value?.Trim();
            if (string.IsNullOrWhiteSpace(clean)) return "-";
            if (clean.Length <= 4) return new string('*', clean.Length);
            return string.Concat(new string('*', Math.Min(8, clean.Length - 4)), clean.AsSpan(clean.Length - 4));
        }

        public static string MaskPhone(string? value)
        {
            var clean = value?.Trim();
            if (string.IsNullOrWhiteSpace(clean)) return "-";
            if (clean.Length <= 6) return new string('*', clean.Length);
            return string.Concat(clean.AsSpan(0, 3), new string('*', clean.Length - 6), clean.AsSpan(clean.Length - 3));
        }

        public static string MaskEmail(string? value)
        {
            var clean = value?.Trim();
            if (string.IsNullOrWhiteSpace(clean)) return "-";
            var at = clean.IndexOf('@');
            if (at <= 1) return "***" + (at >= 0 ? clean[at..] : string.Empty);
            return string.Concat(clean.AsSpan(0, 2), "***", clean.AsSpan(at));
        }

        public static string FormatSalaryOrMask(decimal? amount, bool canViewAmount)
        {
            if (!canViewAmount) return "Disembunyikan (Perlu Hak ViewAmount Kontrak)";
            if (!amount.HasValue) return "Belum ditetapkan";
            return $"Rp {amount.Value.ToString("N0", CultureInfo.GetCultureInfo("id-ID"))}";
        }

        public static string SanitizeAccountFailureMessage(string? rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return "Pembuatan akun pengguna belum selesai. Gunakan Coba Lagi pada tahap Akses IT.";
            }

            var trimmed = rawMessage.Trim();
            if (trimmed.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Sql", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                return "Pembuatan akun otomatis terkendala konfigurasi/validasi akun. Silakan periksa email profil lalu tekan Coba Lagi.";
            }

            return trimmed.Length > 240 ? trimmed[..240] : trimmed;
        }

        public static string GetSafeMcuLabel(McuClearanceResult? result) => result switch
        {
            McuClearanceResult.Fit => "Laik Bekerja (Fit)",
            McuClearanceResult.FitWithRestrictions => "Laik Bekerja dengan Catatan Pembatasan",
            McuClearanceResult.Unfit => "Tidak Laik Bekerja (Unfit)",
            McuClearanceResult.Pending => "Menunggu Hasil MCU",
            _ => "Data MCU Belum Tercatat"
        };

        public static bool HasValidAttachmentSignature(string extension, Stream stream)
        {
            if (!stream.CanRead) return false;

            Span<byte> header = stackalloc byte[8];
            var origPos = stream.CanSeek ? stream.Position : 0;
            if (stream.CanSeek) stream.Position = 0;
            var read = stream.Read(header);
            if (stream.CanSeek) stream.Position = origPos;

            return extension switch
            {
                ".pdf" => read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46, // %PDF
                ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                                    && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                _ => false
            };
        }

        public static bool IsPostReadyOptionalEditableItem(HrdOnboardingCaseItem item)
        {
            return !item.IsRequired
                && item.ItemKind != OnboardingItemKind.Automatic
                && string.IsNullOrWhiteSpace(item.SystemItemCode);
        }

        public static OnboardingCaseProgressDto ComputeProgress(HrdOnboardingCase c)
        {
            var activeItems = c.Items.Where(i => !i.IsDelete).ToList();
            var requiredItems = activeItems.Where(i => i.IsRequired).ToList();
            var optionalItems = activeItems.Where(i => !i.IsRequired).ToList();
            var total = activeItems.Count;
            var completed = activeItems.Count(i => i.ItemStatus == OnboardingItemStatus.Completed);
            var reqCompleted = requiredItems.Count(i => i.ItemStatus == OnboardingItemStatus.Completed);
            var optCompleted = optionalItems.Count(i => i.ItemStatus == OnboardingItemStatus.Completed);

            var today = OperationalToday();
            var daysPast = c.CaseStatus == OnboardingCaseStatus.InProgress && !c.FirstDayAttendedOn.HasValue && today > c.PlannedStartDate
                ? today.DayNumber - c.PlannedStartDate.DayNumber
                : 0;

            var pct = requiredItems.Count > 0
                ? (int)Math.Round(reqCompleted * 100.0 / requiredItems.Count, MidpointRounding.AwayFromZero)
                : (total > 0 ? (int)Math.Round(completed * 100.0 / total, MidpointRounding.AwayFromZero) : 0);

            return new OnboardingCaseProgressDto
            {
                TotalItems = total,
                CompletedItems = completed,
                RequiredTotal = requiredItems.Count,
                RequiredCompleted = reqCompleted,
                OptionalTotal = optionalItems.Count,
                OptionalCompleted = optCompleted,
                ProgressPercent = pct,
                ProgressLabel = $"{reqCompleted} dari {requiredItems.Count} butir wajib selesai ({completed}/{total} total butir)",
                LateStartFlagged = daysPast > 0,
                DaysPastPlannedStart = daysPast
            };
        }

        public static OnboardingReadinessDto EvaluateReadiness(
            HrdOnboardingCase c,
            Guid? actorUserId,
            OnboardingLiveFacts facts)
        {
            var activeItems = c.Items.Where(i => !i.IsDelete).ToList();
            var requiredItems = activeItems.Where(i => i.IsRequired).ToList();
            var blockers = new List<OnboardingBlockerDto>();
            var warnings = new List<OnboardingBlockerDto>();

            if (c.CaseStatus == OnboardingCaseStatus.Cancelled)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "CASE_CANCELLED",
                    StageKey = "acceptance",
                    Severity = "blocking",
                    Message = $"Kasus onboarding telah dibatalkan ({c.CancelReason?.ToString() ?? "Cancelled"})."
                });
            }

            if (!c.HasGeneralTemplate)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "GENERAL_TEMPLATE_MISSING",
                    StageKey = "orientation",
                    Severity = "blocking",
                    Message = "Template onboarding umum aktif belum terbentuk saat kasus dibuat; kelengkapan checklist wajib harus dipastikan.",
                    ActionHint = "Periksa master Template Onboarding Lapis Umum."
                });
            }

            if (!c.FirstDayAttendedOn.HasValue)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "FIRST_DAY_ATTENDANCE_MISSING",
                    StageKey = "acceptance",
                    Severity = "blocking",
                    Message = "Kehadiran hari pertama belum dicatat oleh HRD.",
                    ActionHint = "Catat kehadiran hari pertama pada tahap Konfirmasi Penerimaan."
                });
            }

            // Validasi keberadaan butir sistem wajib (mencegah vacuous truth bila checklist kosong)
            var requiredSysCodes = new List<string>
            {
                EmploymentLifecycleValueConstants.ItemFirstDayAttendance,
                EmploymentLifecycleValueConstants.ItemContractSigned,
                EmploymentLifecycleValueConstants.ItemFitnessClearance,
                EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned
            };
            if (c.IsClinical)
            {
                requiredSysCodes.Add(EmploymentLifecycleValueConstants.ItemPreceptorAssigned);
            }

            foreach (var sysCode in requiredSysCodes)
            {
                if (!activeItems.Any(i => string.Equals(i.SystemItemCode, sysCode, StringComparison.OrdinalIgnoreCase)))
                {
                    var stageKey = sysCode switch
                    {
                        EmploymentLifecycleValueConstants.ItemFirstDayAttendance => "acceptance",
                        EmploymentLifecycleValueConstants.ItemContractSigned => "contract",
                        EmploymentLifecycleValueConstants.ItemFitnessClearance => "acceptance",
                        EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned => "placement",
                        EmploymentLifecycleValueConstants.ItemPreceptorAssigned => "orientation",
                        _ => "orientation"
                    };

                    blockers.Add(new OnboardingBlockerDto
                    {
                        Code = $"SYSTEM_ITEM_MISSING_{sysCode}",
                        StageKey = stageKey,
                        Severity = "blocking",
                        Message = $"Butir wajib sistem '{sysCode}' tidak ditemukan pada kasus ini."
                    });
                }
            }

            if (requiredItems.Count == 0)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "EMPTY_REQUIRED_CHECKLIST",
                    StageKey = "orientation",
                    Severity = "blocking",
                    Message = "Checklist wajib tidak boleh kosong sebelum pernyataan Siap Kerja."
                });
            }

            // Validasi setiap butir wajib
            var pendingRequired = requiredItems.Where(i => i.ItemStatus != OnboardingItemStatus.Completed).ToList();
            foreach (var item in pendingRequired)
            {
                var stageKey = item.SystemItemCode switch
                {
                    EmploymentLifecycleValueConstants.ItemFirstDayAttendance => "acceptance",
                    EmploymentLifecycleValueConstants.ItemContractSigned => "contract",
                    EmploymentLifecycleValueConstants.ItemFitnessClearance => "acceptance",
                    EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned => "placement",
                    EmploymentLifecycleValueConstants.ItemPreceptorAssigned => "orientation",
                    _ => "orientation"
                };

                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "PENDING_REQUIRED_ITEM",
                    StageKey = stageKey,
                    Severity = "blocking",
                    Message = $"Butir wajib '{item.ItemName}' ({item.ResponsibleRole}) belum berstatus Selesai."
                });
            }

            // Validasi integritas bukti fisik untuk item wajib bukti yang sudah selesai
            var missingEvidenceItems = requiredItems
                .Where(i => i.ItemKind == OnboardingItemKind.EvidenceRequired
                            && i.ItemStatus == OnboardingItemStatus.Completed
                            && string.IsNullOrWhiteSpace(i.EvidenceStoragePath))
                .ToList();
            foreach (var item in missingEvidenceItems)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "EVIDENCE_MISSING",
                    StageKey = "orientation",
                    Severity = "blocking",
                    Message = $"Butir wajib bukti '{item.ItemName}' belum memiliki lampiran bukti yang valid di server."
                });
            }

            // Validasi fakta nyata kontrak kerja pertama (termasuk mencegah data lama rehire/kontrak tanpa scan)
            if (!facts.HasValidSignedContract)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "CONTRACT_NOT_SIGNED",
                    StageKey = "contract",
                    Severity = "blocking",
                    Message = facts.FirstContract == null
                        ? "Draf perjanjian kerja pertama untuk penawaran kandidat ini belum terbentuk."
                        : $"Perjanjian kerja '{facts.FirstContract.ContractNumber}' berstatus '{facts.FirstContract.ContractStatus}' atau belum memiliki pindaian dokumen bertanda tangan.",
                    ActionHint = "Unggah pindaian kontrak & tandai sudah ditandatangani pada modul Perjanjian Kerja.",
                    TargetRoute = facts.FirstContract != null ? $"/hr/lifecycle/employment-contracts/{facts.FirstContract.ContractId}" : "/hr/lifecycle/employment-contracts"
                });
            }

            // Validasi MCU kelaikan kerja
            if (!facts.HasValidMcu)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "MCU_NOT_FIT",
                    StageKey = "acceptance",
                    Severity = "blocking",
                    Message = $"Hasil MCU pra-kerja belum laik bekerja ({GetSafeMcuLabel(facts.McuResult)})."
                });
            }
            else if (facts.McuResult == McuClearanceResult.FitWithRestrictions)
            {
                warnings.Add(new OnboardingBlockerDto
                {
                    Code = "MCU_FIT_WITH_RESTRICTIONS",
                    StageKey = "acceptance",
                    Severity = "warning",
                    Message = "Kandidat berstatus Laik dengan Pembatasan; pastikan penempatan unit memperhatikan batas kerja."
                });
            }

            // Validasi sertifikasi wajib otomatis terhadap data live WfpCertification
            var requiredAutoCerts = requiredItems
                .Where(i => i.ItemKind == OnboardingItemKind.Automatic && i.CertificationTypeId.HasValue)
                .ToList();
            foreach (var certItem in requiredAutoCerts)
            {
                if (!facts.ValidCertificationTypeIds.Contains(certItem.CertificationTypeId!.Value))
                {
                    blockers.Add(new OnboardingBlockerDto
                    {
                        Code = "MANDATORY_CERTIFICATION_INVALID",
                        StageKey = "orientation",
                        Severity = "blocking",
                        Message = $"Sertifikasi wajib pada butir '{certItem.ItemName}' belum terverifikasi atau sudah kedaluwarsa.",
                        ActionHint = "Lengkapi dan verifikasi sertifikasi di profil tenaga kerja, lalu klik Periksa Butir Otomatis."
                    });
                }
            }

            // Validasi atasan langsung
            var supervisorItem = activeItems.FirstOrDefault(i => i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            if (!facts.DirectSupervisorValid)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "DIRECT_SUPERVISOR_INVALID",
                    StageKey = "placement",
                    Severity = "blocking",
                    Message = facts.DirectSupervisorInvalidReason ?? "Atasan langsung belum ditetapkan secara valid oleh Kepala Unit.",
                    ActionHint = "Minta Kepala Unit menetapkan atasan langsung pada checklist tahap Penempatan."
                });
            }

            if (facts.HasManagerConflict && !string.IsNullOrWhiteSpace(facts.ManagerConflictNote))
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "MANAGER_CONFLICT",
                    StageKey = "placement",
                    Severity = "blocking",
                    Message = facts.ManagerConflictNote
                });
            }

            // Validasi preceptor klinis
            if (c.IsClinical && !facts.PreceptorValid)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "PRECEPTOR_INVALID",
                    StageKey = "orientation",
                    Severity = "blocking",
                    Message = facts.PreceptorInvalidReason ?? "Preceptor klinis aktif belum ditetapkan untuk tenaga kesehatan ini."
                });
            }

            // Peringatan SPK/RKK non-blokir sesuai HRD-DEC-087
            if (c.IsClinical && !facts.HasCredentialLicense)
            {
                warnings.Add(new OnboardingBlockerDto
                {
                    Code = "CREDENTIAL_WARNING_ONLY",
                    StageKey = "orientation",
                    Severity = "warning",
                    Message = "Kewenangan klinis / kredensial (SPK/RKK) belum tercatat pada profil pegawai (peringatan non-blokir sesuai HRD-DEC-087)."
                });
            }

            if (c.CaseStatus == OnboardingCaseStatus.InProgress && !c.FirstDayAttendedOn.HasValue && OperationalToday() > c.PlannedStartDate)
            {
                warnings.Add(new OnboardingBlockerDto
                {
                    Code = "LATE_START",
                    StageKey = "acceptance",
                    Severity = "warning",
                    Message = $"Pegawai belum tercatat hadir melewati tanggal rencana mulai ({c.PlannedStartDate:yyyy-MM-dd})."
                });
            }

            // Validasi VR-ONB-12: Four-Eyes (penyata siap kerja != pemilih atasan langsung)
            var fourEyesViolation = supervisorItem != null
                && actorUserId.HasValue
                && actorUserId.Value != Guid.Empty
                && supervisorItem.CompletedByUserId.HasValue
                && supervisorItem.CompletedByUserId.Value == actorUserId.Value;

            var fourEyesMessage = fourEyesViolation
                ? "Akun Anda menetapkan atasan langsung pada kasus ini; sesuai aturan empat mata (VR-ONB-12), Pernyataan Siap Kerja harus dilakukan petugas HRD lain."
                : null;

            if (fourEyesViolation)
            {
                blockers.Add(new OnboardingBlockerDto
                {
                    Code = "FOUR_EYES_VIOLATION",
                    StageKey = "readiness",
                    Severity = "blocking",
                    Message = fourEyesMessage!
                });
            }

            var accessFailure = c.AccessRequests.Any(a => !a.IsDelete && a.RequestStatus == OnboardingAccessRequestStatus.Failed);
            if (accessFailure)
            {
                warnings.Add(new OnboardingBlockerDto
                {
                    Code = "ACCOUNT_CREATION_FAILED",
                    StageKey = "access",
                    Severity = "warning",
                    Message = "Pembuatan akun portal layanan mandiri sebelumnya gagal. Anda tetap dapat mengulang di tahap Permintaan Akses IT."
                });
            }

            var isAlreadyReady = c.CaseStatus == OnboardingCaseStatus.Completed;
            var eligible = c.CaseStatus == OnboardingCaseStatus.InProgress && blockers.Count == 0;

            return new OnboardingReadinessDto
            {
                Eligible = eligible,
                CanDeclare = eligible,
                AlreadyReady = isAlreadyReady,
                FourEyesViolation = fourEyesViolation,
                FourEyesMessage = fourEyesMessage,
                EvaluatedAtUtc = DateTime.UtcNow,
                SupervisorAssignedByUserId = supervisorItem?.CompletedByUserId,
                SupervisorAssignedByName = supervisorItem?.CompletedByUser?.DisplayName ?? supervisorItem?.CompletedByUser?.UserName,
                SupervisorSelectedProfileId = supervisorItem?.SelectedWorkforceProfileId,
                SupervisorSelectedName = supervisorItem?.SelectedWorkforceProfile?.DisplayName,
                Blockers = blockers,
                Warnings = warnings
            };
        }

        public static List<OnboardingWizardStageDto> BuildStages(
            HrdOnboardingCase c,
            OnboardingCaseProgressDto progress,
            OnboardingReadinessDto readiness,
            OnboardingAcceptanceSummaryDto acceptance,
            OnboardingEmployeeDataSummaryDto employeeData,
            OnboardingContractSummaryDto contract,
            OnboardingPlacementSummaryDto placement,
            OnboardingPreceptorshipSummaryDto? preceptorship,
            OnboardingActorPermissionsDto actor)
        {
            var isCancelled = c.CaseStatus == OnboardingCaseStatus.Cancelled;
            var isCompleted = c.CaseStatus == OnboardingCaseStatus.Completed;
            var blockersByStage = readiness.Blockers.Concat(readiness.Warnings)
                .GroupBy(b => b.StageKey)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            List<OnboardingBlockerDto> ForStage(string key) =>
                blockersByStage.TryGetValue(key, out var list) ? list : new List<OnboardingBlockerDto>();

            // Stage 1: Konfirmasi Penerimaan
            var stage1Actions = new List<string>();
            if (!isCancelled && !isCompleted)
            {
                if (!c.FirstDayAttendedOn.HasValue && actor.CanRecordAttendance) stage1Actions.Add("RecordFirstDayAttendance");
                if (c.FirstDayAttendedOn.HasValue && !contract.IsContractValidForOnboarding && actor.CanCancelAttendance) stage1Actions.Add("CancelFirstDayAttendance");
                if (!c.FirstDayAttendedOn.HasValue && actor.CanCancelJoin) stage1Actions.Add("CancelJoin");
                if (actor.CanReassignCoordinator) stage1Actions.Add("ReassignCoordinator");
            }
            var stage1Status = isCancelled
                ? "cancelled"
                : c.FirstDayAttendedOn.HasValue && acceptance.McuClearanceStatus is "Fit" or "FitWithRestrictions"
                    ? "completed"
                    : "in_progress";

            var stage1 = new OnboardingWizardStageDto
            {
                Order = 1,
                Key = "acceptance",
                Title = "Konfirmasi Penerimaan",
                Subtitle = "Serah terima rekrutmen, MCU pra-kerja & kehadiran hari pertama",
                Status = stage1Status,
                ResponsibleRoleLabel = "HRD Rekrutmen & Kepegawaian",
                Summary = isCancelled
                    ? $"Kasus dibatalkan: {c.CancelReason?.ToString() ?? "-"}."
                    : c.FirstDayAttendedOn.HasValue
                        ? $"Hadir hari pertama tercatat tanggal {c.FirstDayAttendedOn.Value:dd-MM-yyyy}. MCU: {acceptance.McuClearanceLabel}."
                        : $"Menunggu kehadiran hari pertama (rencana {c.PlannedStartDate:dd-MM-yyyy}). MCU: {acceptance.McuClearanceLabel}.",
                NextActionLabel = c.FirstDayAttendedOn.HasValue ? "Kehadiran Tercatat" : "Catat Hadir Hari Pertama",
                NextActionHint = c.FirstDayAttendedOn.HasValue
                    ? "Kehadiran hari pertama dan aktivasi portal awal sudah dicatat."
                    : "Catat kehadiran fisik hari pertama atau proses Batal Bergabung jika kandidat mengundurkan diri sebelum masuk.",
                AllowedActions = stage1Actions,
                Blockers = ForStage("acceptance"),
                DeepLinks = string.IsNullOrWhiteSpace(acceptance.CandidateRoute)
                    ? new List<OnboardingDeepLinkDto>()
                    : new List<OnboardingDeepLinkDto>
                    {
                        new() { Key = "candidate-journey", Label = "Buka Recruitment Journey Kandidat", Route = acceptance.CandidateRoute }
                    }
            };

            // Stage 2: Kelengkapan Data Pegawai
            var empPrimaryRoute = employeeData.PrimaryWorkforceRoute ?? employeeData.Employee360Route;
            var stage2Status = isCancelled
                ? "cancelled"
                : employeeData.ApplicableGroupCount > 0 && employeeData.CompletedGroupCount >= employeeData.ApplicableGroupCount
                    ? "completed"
                    : employeeData.CompletedGroupCount > 0 ? "in_progress" : "warning";
            var stage2 = new OnboardingWizardStageDto
            {
                Order = 2,
                Key = "employeeData",
                Title = "Kelengkapan Data Pegawai",
                Subtitle = "Audit read-only 9 kelompok berkas & profil tenaga kerja",
                Status = stage2Status,
                ResponsibleRoleLabel = "HRD / Staf Administrasi Kepegawaian",
                Summary = employeeData.HasMasterRecord
                    ? $"{employeeData.CompletedGroupCount} dari {employeeData.ApplicableGroupCount} kelompok data terisi pada profil pegawai."
                    : $"Profil tenaga kerja aktif ({employeeData.ProfileCode}) tercatat; {employeeData.CompletedGroupCount} kelompok data terisi. Master data pegawai resmi dilengkapi terpisah.",
                NextActionLabel = "Lengkapi Data Pegawai",
                NextActionHint = "Lengkapi alamat, keluarga, kontak darurat, pendidikan, rekening payroll, dan sertifikasi pada modul Master Data Pegawai / Employee 360.",
                Blockers = ForStage("employeeData"),
                DeepLinks = string.IsNullOrWhiteSpace(empPrimaryRoute)
                    ? new List<OnboardingDeepLinkDto>()
                    : new List<OnboardingDeepLinkDto>
                    {
                        new() { Key = "workforce-profile", Label = employeeData.HasMasterRecord ? "Buka Data Induk & Kelengkapan Pegawai" : "Lengkapi Profil via Data Pegawai / 360", Route = empPrimaryRoute }
                    }
            };

            // Stage 3: Perjanjian Kerja
            var stage3Actions = new List<string>();
            if (!isCancelled && !isCompleted && actor.CanCheckAutomaticItems)
            {
                stage3Actions.Add("CheckAutomaticItems");
            }
            var stage3Status = isCancelled
                ? "cancelled"
                : contract.IsContractValidForOnboarding
                    ? "completed"
                    : contract.ContractId.HasValue ? "in_progress" : "blocked";
            var stage3 = new OnboardingWizardStageDto
            {
                Order = 3,
                Key = "contract",
                Title = "Perjanjian Kerja",
                Subtitle = "Verifikasi kontrak pertama bertanda tangan & lampiran scan",
                Status = stage3Status,
                ResponsibleRoleLabel = "HRD Administrasi Kontrak",
                Summary = contract.ContractId.HasValue
                    ? $"Kontrak {contract.ContractNumber} ({contract.ContractStatus ?? "-"}) — {(contract.HasSignedDocument ? "Pindaian terunggah" : "Pindaian belum diunggah")}."
                    : "Belum ada kontrak kerja yang terhubung ke penawaran serah terima ini.",
                NextActionLabel = contract.IsContractValidForOnboarding ? "Kontrak Sah" : "Buka & Sahkan Kontrak",
                NextActionHint = contract.IsContractValidForOnboarding
                    ? "Kontrak pertama telah sah dan siap menjadi dasar penjadwalan serta payroll."
                    : "Unggah scan kontrak bertanda tangan pada modul Perjanjian Kerja lalu klik Periksa Butir Otomatis.",
                AllowedActions = stage3Actions,
                Blockers = ForStage("contract"),
                DeepLinks = string.IsNullOrWhiteSpace(contract.ContractRoute)
                    ? new List<OnboardingDeepLinkDto>()
                    : new List<OnboardingDeepLinkDto>
                    {
                        new() { Key = "contract-detail", Label = "Buka Detail Kontrak Kerja", Route = contract.ContractRoute }
                    }
            };

            // Stage 4: Penempatan Pegawai
            var supItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            var stage4Actions = new List<string>();
            if (!isCancelled && !isCompleted && supItem != null && supItem.ItemStatus != OnboardingItemStatus.Completed && actor.CanCompleteUnitItems)
            {
                stage4Actions.Add("AssignDirectSupervisor");
            }
            var stage4Status = isCancelled
                ? "cancelled"
                : placement.DirectSupervisorProfileId.HasValue && !placement.HasManagerConflict && supItem?.ItemStatus == OnboardingItemStatus.Completed
                    ? "completed"
                    : "in_progress";
            var stage4 = new OnboardingWizardStageDto
            {
                Order = 4,
                Key = "placement",
                Title = "Penempatan Pegawai",
                Subtitle = "Unit kerja, jabatan, uraian tugas & penetapan atasan langsung",
                Status = stage4Status,
                ResponsibleRoleLabel = "Kepala Unit Tujuan & HRD",
                Summary = placement.DirectSupervisorName != null
                    ? $"Unit {placement.OrganizationUnitName}, Jabatan {placement.PositionTitle}. Atasan langsung ditetapkan: {placement.DirectSupervisorName}."
                    : $"Unit {placement.OrganizationUnitName}, Jabatan {placement.PositionTitle}. Menunggu Kepala Unit menetapkan atasan langsung.",
                NextActionLabel = supItem?.ItemStatus == OnboardingItemStatus.Completed ? "Atasan Langsung Ditetapkan" : "Tetapkan Atasan Langsung",
                NextActionHint = supItem?.ItemStatus == OnboardingItemStatus.Completed
                    ? "Penugasan atasan langsung (WfpManagerAssignment) akan diterbitkan otomatis saat Siap Kerja dinyatakan."
                    : "Hanya Kepala Unit bersangkutan yang berwenang menetapkan atasan langsung pada butir ini.",
                AllowedActions = stage4Actions,
                Blockers = ForStage("placement"),
                DeepLinks = new List<OnboardingDeepLinkDto>
                {
                    new() { Key = "org-assignments", Label = "Penugasan Organisasi", Route = "/hr/workforce-core/organization-assignments" },
                    new() { Key = "job-descriptions", Label = "Uraian Tugas Jabatan", Route = "/hr/workforce-core/employee-job-descriptions" },
                    new() { Key = "manager-assignments", Label = "Penugasan Atasan Langsung", Route = "/hr/workforce-core/manager-assignments" }
                }
            };

            // Stage 5: Orientasi & Pendampingan
            var stage5Actions = new List<string>();
            if (!isCancelled)
            {
                if (!isCompleted && actor.CanCheckAutomaticItems) stage5Actions.Add("CheckAutomaticItems");
                if (actor.CanCompleteHrItems) stage5Actions.Add("CompleteHrItems");
                if (actor.CanCompleteUnitItems) stage5Actions.Add("CompleteUnitItems");
            }
            var stage5Status = isCancelled
                ? "cancelled"
                : progress.RequiredTotal > 0 && progress.RequiredCompleted >= progress.RequiredTotal && (!c.IsClinical || (preceptorship != null && preceptorship.PreceptorshipStatus != "NotAssigned"))
                    ? (progress.OptionalTotal > progress.OptionalCompleted ? "in_progress" : "completed")
                    : "in_progress";
            var stage5 = new OnboardingWizardStageDto
            {
                Order = 5,
                Key = "orientation",
                Title = "Orientasi & Pendampingan",
                Subtitle = "Checklist Umum + Klinis + Unit & penunjukan preceptor klinis",
                Status = stage5Status,
                ResponsibleRoleLabel = "HRD & Kepala Unit (Terpisah per Butir)",
                Summary = c.IsClinical
                    ? $"{progress.ProgressLabel}. Pendampingan klinis: {preceptorship?.PreceptorshipStatus ?? "NotAssigned"}."
                    : $"{progress.ProgressLabel}.",
                NextActionLabel = isCompleted ? "Kelola Butir Opsional" : "Selesaikan Checklist Orientasi",
                NextActionHint = isCompleted
                    ? "Setelah Siap Kerja, butir opsional masih dapat dilengkapi dan status pendampingan klinis dikelola Kepala Unit."
                    : "HRD menyelesaikan butir Umum/Administratif dan Kepala Unit menyelesaikan butir Unit/Klinis sesuai peran masing-masing.",
                AllowedActions = stage5Actions,
                Blockers = ForStage("orientation"),
                DeepLinks = string.IsNullOrWhiteSpace(preceptorship?.PreceptorshipRoute)
                    ? new List<OnboardingDeepLinkDto>()
                    : new List<OnboardingDeepLinkDto>
                    {
                        new() { Key = "preceptorship", Label = "Buka Detail Pendampingan Klinis", Route = preceptorship!.PreceptorshipRoute! }
                    }
            };

            // Stage 6: Permintaan Akses IT
            var accReq = c.AccessRequests.Where(a => !a.IsDelete).OrderByDescending(a => a.RequestedAt).ToList();
            var hasFailedAcc = accReq.Any(a => a.RequestStatus == OnboardingAccessRequestStatus.Failed);
            var hasPendingAcc = accReq.Any(a => a.RequestStatus == OnboardingAccessRequestStatus.Requested);
            var stage6Actions = new List<string>();
            if (!isCancelled)
            {
                if (hasFailedAcc && actor.CanRetryAccountCreation) stage6Actions.Add("RetryAccountCreation");
                if (accReq.Any(a => a.RequestType == OnboardingAccessRequestType.WorkAccess && a.RequestStatus == OnboardingAccessRequestStatus.Requested) && actor.CanCompleteWorkAccess)
                {
                    stage6Actions.Add("CompleteWorkAccess");
                }
            }
            var stage6Status = isCancelled
                ? "cancelled"
                : hasFailedAcc ? "warning"
                : accReq.Count > 0 && !hasPendingAcc ? "completed"
                : "in_progress";
            var stage6 = new OnboardingWizardStageDto
            {
                Order = 6,
                Key = "access",
                Title = "Permintaan Akses IT",
                Subtitle = "Akun portal mandiri (ESS) & tiket akses kerja SIMRS penuh",
                Status = stage6Status,
                ResponsibleRoleLabel = "IT / Administrator Sistem & HRD",
                Summary = accReq.Count == 0
                    ? "Belum ada permintaan akses tercatat."
                    : hasFailedAcc
                        ? "Pembuatan akun otomatis gagal; perlukan Coba Lagi."
                        : hasPendingAcc
                            ? "Tiket akses kerja SIMRS menunggu penyelesaian oleh IT/Administrator."
                            : "Akun layanan mandiri dan akses kerja SIMRS telah diproses.",
                NextActionLabel = hasFailedAcc ? "Ulangi Pembuatan Akun" : hasPendingAcc ? "Proses di Antrean Akses IT" : "Akses Terpenuhi",
                NextActionHint = "Pembukaan akses penuh SIMRS diterbitkan otomatis menjadi tiket kerja setelah Siap Kerja dinyatakan.",
                AllowedActions = stage6Actions,
                Blockers = ForStage("access"),
                DeepLinks = new List<OnboardingDeepLinkDto>
                {
                    new() { Key = "access-queue", Label = "Buka Antrean Permintaan Akses IT", Route = "/hr/lifecycle/onboarding-access-requests" }
                }
            };

            // Stage 7: Verifikasi Siap Kerja
            var stage7Actions = new List<string>();
            if (!isCancelled && !isCompleted && readiness.CanDeclare && actor.CanDeclareReady)
            {
                stage7Actions.Add("DeclareReady");
            }
            var stage7Status = isCancelled
                ? "cancelled"
                : isCompleted ? "completed"
                : readiness.Eligible ? "in_progress" : "blocked";
            var stage7 = new OnboardingWizardStageDto
            {
                Order = 7,
                Key = "readiness",
                Title = "Verifikasi Siap Kerja",
                Subtitle = "Pemeriksaan akhir aturan bisnis, empat mata & aktivasi penuh",
                Status = stage7Status,
                ResponsibleRoleLabel = "HRD Berwenang (Bukan Pemilih Atasan)",
                Summary = isCompleted
                    ? $"Pegawai telah dinyatakan Siap Kerja pada {c.ReadyForWorkAt?.ToString("dd-MM-yyyy HH:mm") ?? "-"}."
                    : readiness.Eligible
                        ? "Seluruh syarat Siap Kerja terpenuhi. Menunggu pengesahan HRD."
                        : $"Tertahan oleh {readiness.Blockers.Count} penghalang sebelum Siap Kerja dapat disahkan.",
                NextActionLabel = isCompleted ? "Sudah Siap Kerja" : "Nyatakan Siap Kerja",
                NextActionHint = isCompleted
                    ? "Profil aktif, atasan langsung terbentuk, serta pegawai dapat masuk penjadwalan dan payroll."
                    : readiness.Eligible
                        ? "Konfirmasi pernyataan Siap Kerja untuk mengaktifkan pegawai secara penuh."
                        : "Selesaikan seluruh item penghalang pada daftar di bawah sebelum menyatakan Siap Kerja.",
                AllowedActions = stage7Actions,
                Blockers = readiness.Blockers.Concat(readiness.Warnings).ToList()
            };

            return new List<OnboardingWizardStageDto>
            {
                stage1, stage2, stage3, stage4, stage5, stage6, stage7
            };
        }

        private static TimeZoneInfo ResolveJakartaTimeZone()
        {
            foreach (var id in new[] { "Asia/Jakarta", "SE Asia Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { }
            }
            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }
    }
}
