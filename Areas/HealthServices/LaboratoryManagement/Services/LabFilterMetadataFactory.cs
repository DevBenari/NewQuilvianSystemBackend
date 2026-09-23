using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Penyusun metadata penyaring untuk kelima grup endpoint Laboratorium.
    ///
    /// Isinya murni keterangan bentuk layar — daftar pilihan enum, kolom yang boleh diurutkan,
    /// ukuran halaman, dan parameter query yang benar-benar diterima endpointnya. Tidak ada
    /// satu pun kueri database di sini, sehingga metadata dapat dijawab tanpa menyentuh data
    /// pasien.
    ///
    /// <b>Satu aturan yang menentukan seluruh isi berkas ini:</b> ruas yang dideklarasikan wajib
    /// benar-benar didukung endpointnya. Metadata yang menjanjikan penyaring yang tidak diproses
    /// daftar adalah cacat kontrak, bukan sekadar dokumentasi yang usang. Karena itu grup yang
    /// daftarnya memang belum menerima parameter apa pun — Lab Order dan Lab Specimen —
    /// menyatakannya terbuka lewat <c>SupportsServerSideFiltering</c> bernilai salah, bukan
    /// mengarang daftar penyaring yang tidak ada penegakannya.
    /// </summary>
    public static class LabFilterMetadataFactory
    {
        private static readonly List<int> UkuranHalaman = new() { 10, 25, 50, 100 };
        private static readonly List<string> ArahUrut = new() { "asc", "desc" };

        // =================================================================
        // Lab Order
        // =================================================================

        public static LabOrderFilterMetadataResponse LabOrder() =>
            new()
            {
                OrderStatuses = Opsi<LabOrderStatus>(LabelOrderStatus),
                Disciplines = Opsi<LabDiscipline>(LabelDisiplin),
                SortOptions = new()
                {
                    new() { Value = "createDateTime", Label = "Waktu dibuat" },
                    new() { Value = "orderStatus", Label = "Status pesanan" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "encounterId",
                        Type = "guid",
                        Description = "Menyaring per kunjungan pasien. Dipakai layar yang hanya menampilkan pesanan satu pasien.",
                        Example = "3f2a4c60-0001-4a10-9f01-6b1d0a5e7c01"
                    },
                    new() { Name = "orderStatus", Type = "integer", Description = "Menyaring per status operasional pesanan.", Example = "2" },
                    new() { Name = "discipline", Type = "integer", Description = "Menyaring per disiplin laboratorium.", Example = "1" },
                    new() { Name = "startDate", Type = "date", Description = "Pesanan yang dibuat sejak tanggal ini.", Example = "2026-09-01" },
                    new() { Name = "endDate", Type = "date", Description = "Pesanan yang dibuat sampai tanggal ini.", Example = "2026-09-30" },
                    new() { Name = "search", Type = "string", Description = "Pencarian bebas pada kode dan nama jenis pemeriksaan.", Example = "hemoglobin" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan: createDateTime atau orderStatus.", Example = "createDateTime" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan: asc atau desc. Bawaannya desc.", Example = "desc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true
            };

        // =================================================================
        // Lab Specimen
        // =================================================================

        public static LabSpecimenFilterMetadataResponse LabSpecimen() =>
            new()
            {
                SpecimenStatuses = Opsi<LabSpecimenStatus>(LabelSpecimenStatus),
                RecollectionCauses = Opsi<LabRecollectionCause>(LabelSebabAmbilUlang),
                SortOptions = new()
                {
                    new() { Value = "specimenSequence", Label = "Urutan wadah" },
                    new() { Value = "specimenStatus", Label = "Status wadah" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "labOrderId",
                        Type = "guid",
                        Required = "Yes",
                        Description = "Pesanan yang wadahnya hendak dilihat. Dikirim sebagai bagian route, bukan query string.",
                        Example = "3f2a4c60-0001-4a10-9f01-6b1d0a5e7c01"
                    }
                },
                SupportsServerSideFiltering = false,
                SupportsServerSidePaging = false,
                IsDeletable = false
            };

        // =================================================================
        // Lab Value Bound
        // =================================================================

        public static LabValueBoundFilterMetadataResponse LabValueBound() =>
            new()
            {
                ResultForms = Opsi<LabResultForm>(LabelBentukHasil),
                GenderScopes = Opsi<LabGenderScope>(LabelJenisKelamin),
                SortOptions = new()
                {
                    new() { Value = "procedureName", Label = "Nama pemeriksaan" },
                    new() { Value = "genderScope", Label = "Jenis kelamin" },
                    new() { Value = "ageCategory", Label = "Kelompok umur" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "procedureId",
                        Type = "guid",
                        Description = "Menyaring per jenis pemeriksaan.",
                        Example = "3f2a4c60-0001-4a10-9f01-6b1d0a5e7c01"
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama pemeriksaan.",
                        Example = "hemoglobin"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false,
                CriticalBoundRequiresApproval = true
            };

        // =================================================================
        // Lab Critical Bound Approval
        // =================================================================

        public static LabCriticalBoundApprovalFilterMetadataResponse LabCriticalBoundApproval() =>
            new()
            {
                RequestStatuses = Opsi<LabBoundChangeStatus>(LabelStatusPengajuan),
                SortOptions = new()
                {
                    new() { Value = "submittedAt", Label = "Waktu diajukan" },
                    new() { Value = "requestStatus", Label = "Status pengajuan" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "valueBoundId",
                        Type = "guid",
                        Required = "Yes",
                        Description = "Batas nilai yang pengajuannya hendak dilihat. Dikirim sebagai bagian route, bukan query string.",
                        Example = "3f2a4c60-0001-4a10-9f01-6b1d0a5e7c01"
                    }
                },
                SupportsServerSideFiltering = false,
                SupportsServerSidePaging = false,
                IsScopedToSingleValueBound = true,
                SelfApprovalForbidden = true,
                SinglePendingRequestOnly = true
            };

        // =================================================================
        // Lab Rejection Reason
        // =================================================================

        public static LabRejectionReasonFilterMetadataResponse LabRejectionReason() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "reasonCode", Label = "Kode alasan" },
                    new() { Value = "reasonName", Label = "Nama alasan" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode, nama, dan keterangan alasan.",
                        Example = "label"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false,
                SystemFlagFields = new() { "isInternalHospitalError", "requiresNote" }
            };

        public static LabSpecimenTypeFilterMetadataResponse LabSpecimenType() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "specimenTypeCode", Label = "Kode jenis" },
                    new() { Value = "specimenTypeName", Label = "Nama jenis" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode, nama, dan keterangan jenis.",
                        Example = "darah"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false,
                IsOtherBucketEditable = false
            };

        /// <summary>
        /// Bentuk layar data induk breakpoint (<c>FE-LAB-34</c>).
        ///
        /// Seluruh penyaring yang diumumkan di sini <b>benar-benar diproses</b>
        /// <c>GetListAsync</c>. Metadata yang menjanjikan penyaring yang tidak diproses adalah
        /// cacat kontrak, bukan dokumentasi yang usang.
        /// </summary>
        public static LabSusceptibilityBreakpointFilterMetadataResponse LabSusceptibilityBreakpoint() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "organismName", Label = "Nama kuman" },
                    new() { Value = "antibioticName", Label = "Nama antibiotik" },
                    new() { Value = "guidelineVersion", Label = "Versi pedoman" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "labOrganismId",
                        Type = "uuid",
                        Description = "Menyaring satu kuman.",
                        Example = "db086f41-bbef-4e06-bd86-00dffa95f058"
                    },
                    new()
                    {
                        Name = "labAntibioticId",
                        Type = "uuid",
                        Description = "Menyaring satu antibiotik.",
                        Example = "e66c2432-9fab-42d8-b1e7-348576b77183"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada nama kuman, nama antibiotik, dan versi pedoman.",
                        Example = "catarrhalis"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = true
            };

        /// <summary>Bentuk layar pemetaan profil Mikrobiologi katalog (<c>FE-LAB-34</c>).</summary>
        public static LabProcedureMicrobiologyProfileFilterMetadataResponse LabProcedureMicrobiologyProfile() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "procedureName", Label = "Nama pemeriksaan" },
                    new() { Value = "procedureCode", Label = "Kode pemeriksaan" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "usesSusceptibilitySet",
                        Type = "boolean",
                        Description = "Menyaring pemeriksaan yang memakai set bakteri atau tidak.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama pemeriksaan katalog.",
                        Example = "MO KUL"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                CultureTypeOptions = Opsi<LabCultureType>(x => x switch
                {
                    LabCultureType.Bacterial => "Bakteri",
                    LabCultureType.Fungal => "Jamur",
                    _ => x.ToString()
                }),
                SusceptibilityMethodOptions = Opsi<LabSusceptibilityMethod>(x => x switch
                {
                    LabSusceptibilityMethod.DiscDiffusion => "Difusi Cakram",
                    LabSusceptibilityMethod.Dilution => "Dilusi",
                    _ => x.ToString()
                }),
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = true
            };

        // =================================================================
        // =================================================================
        // Pembantu
        // =================================================================

        private static List<LabEnumOptionResponse> Opsi<TEnum>(Func<TEnum, string> label)
            where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(x => new LabEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = label(x)
                })
                .ToList();

        private static string LabelOrderStatus(LabOrderStatus x) => x switch
        {
            LabOrderStatus.Draft => "Draf",
            LabOrderStatus.Requested => "Diminta",
            LabOrderStatus.Accepted => "Diterima",
            LabOrderStatus.InProcess => "Sedang dikerjakan",
            LabOrderStatus.Completed => "Selesai",
            LabOrderStatus.OnHold => "Ditahan",
            LabOrderStatus.CancelRequested => "Pembatalan diminta",
            LabOrderStatus.Cancelled => "Dibatalkan",
            LabOrderStatus.Confirmed => "Dikonfirmasi",
            _ => x.ToString()
        };

        private static string LabelDisiplin(LabDiscipline x) => x switch
        {
            LabDiscipline.ClinicalPathology => "Patologi Klinik",
            LabDiscipline.AnatomicalPathology => "Patologi Anatomi",
            LabDiscipline.Microbiology => "Mikrobiologi",
            _ => x.ToString()
        };

        private static string LabelSpecimenStatus(LabSpecimenStatus x) => x switch
        {
            LabSpecimenStatus.Planned => "Direncanakan",
            LabSpecimenStatus.Collected => "Sudah diambil",
            LabSpecimenStatus.Received => "Tiba di laboratorium",
            LabSpecimenStatus.Accepted => "Dinyatakan layak",
            LabSpecimenStatus.Rejected => "Ditolak",
            LabSpecimenStatus.RecollectionRequired => "Perlu ambil ulang",
            LabSpecimenStatus.Cancelled => "Dibatalkan",
            LabSpecimenStatus.OnHold => "Ditahan",
            _ => x.ToString()
        };

        private static string LabelSebabAmbilUlang(LabRecollectionCause x) => x switch
        {
            LabRecollectionCause.InternalHospitalError => "Kesalahan internal rumah sakit",
            LabRecollectionCause.PatientOrSpecimenCondition => "Kondisi pasien atau sampel",
            LabRecollectionCause.ExternalCause => "Sebab eksternal",
            _ => x.ToString()
        };

        private static string LabelBentukHasil(LabResultForm x) => x switch
        {
            LabResultForm.Numeric => "Angka",
            LabResultForm.Choice => "Pilihan",
            _ => x.ToString()
        };

        private static string LabelJenisKelamin(LabGenderScope x) => x switch
        {
            LabGenderScope.All => "Semua",
            LabGenderScope.Male => "Laki-laki",
            LabGenderScope.Female => "Perempuan",
            _ => x.ToString()
        };

        private static string LabelStatusPengajuan(LabBoundChangeStatus x) => x switch
        {
            LabBoundChangeStatus.Submitted => "Diajukan",
            LabBoundChangeStatus.Approved => "Disetujui",
            LabBoundChangeStatus.Rejected => "Ditolak",
            LabBoundChangeStatus.Withdrawn => "Ditarik",
            _ => x.ToString()
        };

        /// <summary>Bentuk layar data induk organisme Mikrobiologi (<c>FE-LAB-24</c>).</summary>
        public static LabOrganismFilterMetadataResponse LabOrganism() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "organismName", Label = "Nama organisme" },
                    new() { Value = "organismCode", Label = "Kode organisme" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama organisme.",
                        Example = "catarrhalis"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false
            };

        /// <summary>Bentuk layar panel uji antibiotik (<c>FE-LAB-24</c>).</summary>
        public static LabAntibioticFilterMetadataResponse LabAntibiotic() =>
            new()
            {
                SortOptions = new()
                {
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "antibioticName", Label = "Nama antibiotik" },
                    new() { Value = "antibioticCode", Label = "Kode antibiotik" }
                },
                SortDirections = new(ArahUrut),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama antibiotik.",
                        Example = "ampicillin"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false
            };

        // =================================================================
        // Tiga data induk Patologi Anatomi — `r32`, `BE-LAB-66`, dipakai `FE-LAB-27`.
        //
        // KETIGANYA MENGIRIM `SortOptions` KOSONG, DAN ITU DISENGAJA. Ketiga jalur daftarnya
        // mengurutkan secara TETAP di dalam service — parameter dan golongan pada
        // `SortOrder` lalu nama, pemetaan pada nama golongan lalu nama pemeriksaan — dan
        // ketiga query DTO-nya nol punya ruas `SortBy` maupun `SortDirection`. Mengirim
        // daftar pilihan urutan di sini berarti layar merender pemilih yang nol mengubah satu
        // baris pun: kelas "terlihat bekerja padahal tidak" yang justru paling mahal pada
        // layar data induk, sebab kepala instalasi akan menyimpulkan urutannya sudah diatur.
        //
        // `LabOrganism` dan `LabAntibiotic` di atas MENGIRIMNYA padahal keadaannya sama —
        // `LabOrganismPagedQuery` dan kembarannya juga nol punya ruas sort. Itu selisih yang
        // DILAPORKAN, bukan ditambal di sini: memperbaikinya berarti menyentuh cakupan
        // `BE-LAB-65`, dan menambah ruas sort pada endpoint daftar yang sudah berjalan
        // bertentangan dengan `r32` bagian 27.6 yang menyatakan kesepuluh endpoint lama nol
        // berubah bentuk.
        // =================================================================

        /// <summary>Bentuk layar data induk parameter Patologi Anatomi (<c>FE-LAB-27</c>).</summary>
        public static LabPathologyParameterFilterMetadataResponse LabPathologyParameter() =>
            new()
            {
                SortOptions = new(),
                SortDirections = new(),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan label parameter.",
                        Example = "makroskopik"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false
            };

        /// <summary>Bentuk layar data induk golongan Patologi Anatomi (<c>FE-LAB-27</c>).</summary>
        public static LabPathologyCategoryFilterMetadataResponse LabPathologyCategory() =>
            new()
            {
                SortOptions = new(),
                SortDirections = new(),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama golongan.",
                        Example = "sitologi"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false
            };

        /// <summary>
        /// Bentuk layar penggolongan jenis pemeriksaan Patologi Anatomi (<c>FE-LAB-27</c>).
        ///
        /// Dua penanda terakhir menyatakan ketiadaan yang disengaja, supaya layar membacanya
        /// alih-alih menyimpulkannya dari endpoint yang menjawab <c>404</c>.
        /// </summary>
        public static LabProcedurePathologyCategoryFilterMetadataResponse LabProcedurePathologyCategory() =>
            new()
            {
                SortOptions = new(),
                SortDirections = new(),
                PageSizeOptions = new(UkuranHalaman),
                QueryParameters = new()
                {
                    new()
                    {
                        Name = "labPathologyCategoryId",
                        Type = "uuid",
                        Description = "Menyaring pada satu golongan. Kosong berarti seluruh golongan.",
                        Example = "0f1d2c3b-4a59-6878-9706-b5c4d3e2f1a0"
                    },
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Pencarian bebas pada kode dan nama jenis pemeriksaan.",
                        Example = "biopsi"
                    },
                    new() { Name = "pageNumber", Type = "integer", Description = "Halaman ke berapa, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "20" }
                },
                SupportsServerSideFiltering = true,
                SupportsServerSidePaging = true,
                IsDeletable = false,
                SupportsStatusToggle = false,
                HasOptionsEndpoint = false
            };
    }
}
