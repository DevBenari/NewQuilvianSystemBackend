namespace QuilvianSystemBackend.Enum
{
    public enum EmployeeProfessionType
    {
        GeneralStaff = 1,              // Staff umum / administrasi umum rumah sakit

        Nurse = 2,                     // Perawat
        Midwife = 3,                   // Bidan

        Pharmacist = 4,                // Apoteker
        PharmacyAssistant = 5,         // Tenaga teknis kefarmasian / asisten apoteker

        LaboratoryAnalyst = 6,         // Analis laboratorium / ATLM
        Radiographer = 7,              // Radiografer / petugas radiologi
        Physiotherapist = 8,           // Fisioterapis
        Nutritionist = 9,              // Ahli gizi / nutrisionis
        MedicalRecorder = 10,          // Perekam medis / rekam medis
        BiomedicalEngineer = 11,       // Teknisi elektromedis / biomedical engineer
        Sanitarian = 12,               // Sanitarian / kesehatan lingkungan rumah sakit

        InformationTechnology = 20,    // Staff IT / SIMRS / infrastruktur teknologi
        Finance = 21,                  // Keuangan / akuntansi
        HumanResource = 22,            // HRD / SDM
        Procurement = 23,              // Pengadaan / purchasing
        Inventory = 24,                // Gudang / inventory / logistik
        Legal = 25,                    // Legal / hukum
        Marketing = 26,                // Marketing / hubungan pelanggan
        PublicRelation = 27,           // Humas / public relation
        QualityManagement = 28,        // Mutu rumah sakit / quality management
        RiskManagement = 29,           // Manajemen risiko
        InternalAudit = 30,            // Audit internal

        Registration = 40,             // Pendaftaran pasien / admission
        Cashier = 41,                  // Kasir
        Billing = 42,                  // Billing / penagihan
        InsuranceClaim = 43,           // Klaim asuransi / BPJS / penjamin
        CustomerService = 44,          // Customer service / front office
        CallCenter = 45,               // Call center / contact center

        Security = 60,                 // Security / satuan pengamanan
        CleaningService = 61,          // Cleaning service / petugas kebersihan
        Driver = 62,                   // Supir / driver ambulans atau operasional
        Laundry = 63,                  // Laundry rumah sakit
        KitchenStaff = 64,             // Petugas dapur / kitchen / food service
        Maintenance = 65,              // Maintenance / teknisi gedung
        Electrician = 66,              // Teknisi listrik
        Plumber = 67,                  // Teknisi plumbing / saluran air
        Gardener = 68,                 // Petugas taman
        ParkingStaff = 69,             // Petugas parkir

        Management = 80,               // Manajemen rumah sakit
        Director = 81,                 // Direktur
        Manager = 82,                  // Manager
        Supervisor = 83,               // Supervisor / koordinator
        HeadOfUnit = 84,               // Kepala unit / kepala instalasi / kepala ruangan

        StudentIntern = 90,            // Mahasiswa magang / praktik kerja lapangan
        Volunteer = 91,                // Relawan

        Other = 99                     // Lainnya
    }
}
