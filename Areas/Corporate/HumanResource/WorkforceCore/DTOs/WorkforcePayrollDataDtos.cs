namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WorkforcePayrollDataQuery
    {
        /// <summary>Nama atau nomor pegawai, dokter, atau perawat.</summary>
        public string? Search { get; set; }

        /// <summary>Karyawan, Perawat, atau Dokter. Kosong = semua.</summary>
        public string? PersonType { get; set; }

        /// <summary>"Lengkap" atau "Belum lengkap". Kosong = semua.</summary>
        public string? Completeness { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu orang pada daftar monitoring read-only Data Penggajian Pegawai (HRD). Seluruh data dibaca dari Master Karyawan,
    /// Master Dokter, WfpSalaryAssignment, dan WfpBankAccount yang berlaku pada tanggal acuan. Nominal gaji hanya terisi bagi
    /// pemegang izin ViewAmount (null bila tidak berwenang atau belum ada penetapan gaji yang berlaku; tidak pernah diganti 0).
    /// Nomor rekening selalu disamarkan.
    /// </summary>
    public class WorkforcePayrollDataItemResponse
    {
        public Guid WorkforceProfileId { get; set; }

        /// <summary>Id MstEmployee untuk tautan menuju Master Karyawan (null untuk Dokter).</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>Id MstDoctor untuk tautan menuju Master Dokter (null untuk Karyawan/Perawat).</summary>
        public Guid? DoctorId { get; set; }

        /// <summary>Karyawan, Perawat (karyawan yang profesinya perawat), atau Dokter.</summary>
        public string PersonType { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }

        public bool HasApprovedSalaryAssignment { get; set; }
        public bool HasPendingSalaryAssignment { get; set; }
        public bool HasSalaryConflict { get; set; }

        public Guid? SalaryAssignmentId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public string? SalaryStructureName { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public string? SalaryGradeCode { get; set; }
        public string? SalaryGradeName { get; set; }

        /// <summary>Benar bila pemanggil berwenang melihat nominal (WorkforcePayrollData : ViewAmount).</summary>
        public bool AmountsVisible { get; set; }

        /// <summary>Nominal gaji pokok dari penetapan gaji utama yang berlaku; null bila tanpa izin ViewAmount atau belum tersedia.</summary>
        public decimal? BaseSalary { get; set; }

        public string? CurrencyCode { get; set; }
        public string? PaymentFrequency { get; set; }

        /// <summary>Disetujui, Menunggu Persetujuan HRD, Konflik Penetapan Gaji, Belum Berlaku, Sudah Berakhir, atau Belum Ada.</summary>
        public string SalaryStatus { get; set; } = string.Empty;

        public DateTime? SalaryEffectiveStartDate { get; set; }
        public DateTime? SalaryEffectiveEndDate { get; set; }

        public Guid? BankAccountId { get; set; }

        /// <summary>"Belum ada", "Belum terverifikasi", "Terverifikasi", "Belum berlaku", atau "Kedaluwarsa".</summary>
        public string BankStatus { get; set; } = string.Empty;

        public string? BankName { get; set; }

        /// <summary>Nomor rekening disamarkan di Backend; hanya empat karakter terakhir yang tampak.</summary>
        public string? AccountNumberMasked { get; set; }

        public bool BankIsVerified { get; set; }
        public DateTime? BankVerifiedAt { get; set; }

        public bool IsComplete { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class WorkforcePayrollDataSummaryResponse
    {
        public int TotalPeople { get; set; }
        public int WithApprovedSalaryAssignment { get; set; }
        public int WithVerifiedBankAccount { get; set; }
        public int Complete { get; set; }
    }

    /// <summary>
    /// Rincian read-only data penggajian satu orang. Bagian penetapan gaji dan rekening bernilai null bila belum ada yang berlaku
    /// pada tanggal acuan. Nominal hanya dikirim bagi pemegang izin ViewAmount dan nomor rekening selalu disamarkan.
    /// </summary>
    public class WorkforcePayrollDataDetailResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public string PersonType { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }

        public bool AmountsVisible { get; set; }
        public string SalaryStatus { get; set; } = string.Empty;
        public string BankStatus { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public List<string> Issues { get; set; } = new();

        public WorkforcePayrollSalaryDetail? Salary { get; set; }
        public WorkforcePayrollBankDetail? BankAccount { get; set; }
    }

    public class WorkforcePayrollSalaryDetail
    {
        public Guid Id { get; set; }
        public Guid SalaryStructureId { get; set; }
        public string SalaryStructureCode { get; set; } = string.Empty;
        public string SalaryStructureName { get; set; } = string.Empty;
        public Guid SalaryGradeId { get; set; }
        public string SalaryGradeCode { get; set; } = string.Empty;
        public string? SalaryGradeName { get; set; }

        /// <summary>Null bila pemanggil tidak berwenang ViewAmount.</summary>
        public decimal? GradeMinimum { get; set; }

        /// <summary>Null bila pemanggil tidak berwenang ViewAmount.</summary>
        public decimal? GradeMaximum { get; set; }

        /// <summary>Nominal gaji pokok dari WfpSalaryAssignment; null bila pemanggil tidak berwenang ViewAmount.</summary>
        public decimal? BaseSalary { get; set; }

        public string CurrencyCode { get; set; } = "IDR";
        public string PaymentFrequency { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class WorkforcePayrollBankDetail
    {
        public Guid Id { get; set; }
        public Guid? BankId { get; set; }
        public string? BankName { get; set; }

        /// <summary>Nomor rekening disamarkan di Backend; endpoint monitoring tidak pernah mengirim nomor rekening lengkap.</summary>
        public string AccountNumberMasked { get; set; } = string.Empty;

        public string AccountHolderName { get; set; } = string.Empty;
        public bool IsPayrollAccount { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    /// <summary>Galat bisnis pembacaan monitoring (404 untuk orang yang tidak ditemukan atau tidak aktif).</summary>
    public sealed class WorkforcePayrollDataException : Exception
    {
        public int StatusCode { get; }

        public WorkforcePayrollDataException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
