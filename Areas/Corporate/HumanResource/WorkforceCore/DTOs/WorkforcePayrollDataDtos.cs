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
    /// Satu orang pada daftar. Sengaja tanpa gaji pokok dan tanpa nomor rekening lengkap: nominal dan nomor rekening hanya dimuat lewat
    /// detail untuk pemegang hak ubah.
    /// </summary>
    public class WorkforcePayrollDataItemResponse
    {
        public Guid WorkforceProfileId { get; set; }

        /// <summary>Karyawan, Perawat (karyawan yang profesinya perawat), atau Dokter.</summary>
        public string PersonType { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }

        public bool HasApprovedSalaryAssignment { get; set; }
        public bool HasPendingSalaryAssignment { get; set; }
        public string? SalaryStructureName { get; set; }
        public DateTime? SalaryEffectiveStartDate { get; set; }

        /// <summary>"Belum ada", "Belum terverifikasi", atau "Terverifikasi".</summary>
        public string BankStatus { get; set; } = string.Empty;

        public string? BankName { get; set; }

        /// <summary>Nomor rekening disamarkan, hanya empat karakter terakhir yang tampak.</summary>
        public string? AccountNumberMasked { get; set; }

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

    /// <summary>Isian satu orang untuk diubah. Bagian yang belum ada bernilai null.</summary>
    public class WorkforcePayrollDataDetailResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string PersonType { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public WorkforcePayrollSalaryDetail? Salary { get; set; }
        public WorkforcePayrollBankDetail? BankAccount { get; set; }
    }

    public class WorkforcePayrollSalaryDetail
    {
        public Guid Id { get; set; }
        public Guid SalaryStructureId { get; set; }
        public string SalaryStructureCode { get; set; } = string.Empty;
        public string SalaryStructureName { get; set; } = string.Empty;
        public string SalaryGradeCode { get; set; } = string.Empty;
        public decimal GradeMinimum { get; set; }
        public decimal GradeMaximum { get; set; }
        public decimal BaseSalary { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class WorkforcePayrollBankDetail
    {
        public Guid Id { get; set; }
        public Guid? BankId { get; set; }
        public string? BankName { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    /// <summary>
    /// Simpan data penggajian satu orang. Bagian <see cref="Salary"/> dan <see cref="BankAccount"/> bersifat sendiri-sendiri: bagian
    /// yang null tidak disentuh. Persetujuan dan verifikasi hanya diterima dari pemegang hak yang sesuai (diputuskan controller).
    /// </summary>
    public class UpdateWorkforcePayrollDataRequest
    {
        public UpdateWorkforcePayrollSalaryRequest? Salary { get; set; }
        public UpdateWorkforcePayrollBankRequest? BankAccount { get; set; }

        /// <summary>Menyetujui penetapan gaji utama sesudah disimpan. Tanpa ini, perubahan atas penetapan yang sudah disetujui menunggu persetujuan ulang.</summary>
        public bool ApproveSalaryAssignment { get; set; }

        /// <summary>Menandai rekening payroll terverifikasi sesudah disimpan. Tanpa ini, perubahan nomor atau pemilik rekening menunggu verifikasi ulang.</summary>
        public bool MarkBankAccountVerified { get; set; }
    }

    public class UpdateWorkforcePayrollSalaryRequest
    {
        public Guid SalaryStructureId { get; set; }
        public decimal BaseSalary { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
    }

    public class UpdateWorkforcePayrollBankRequest
    {
        public Guid? BankId { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
    }

    public class WorkforcePayrollDataSaveResponse
    {
        public Guid WorkforceProfileId { get; set; }

        /// <summary>"Tidak diubah", "Dibuat", "Diperbarui", atau "Disetujui".</summary>
        public string SalaryAssignment { get; set; } = "Tidak diubah";

        /// <summary>"Tidak diubah", "Dibuat", "Diperbarui", atau "Terverifikasi".</summary>
        public string BankAccount { get; set; } = "Tidak diubah";

        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>Galat bisnis yang dijelaskan ke petugas. <see cref="StatusCode"/> 400 untuk isian yang ditolak, 404 untuk orang yang tidak ditemukan.</summary>
    public sealed class WorkforcePayrollDataException : Exception
    {
        public int StatusCode { get; }

        public WorkforcePayrollDataException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
