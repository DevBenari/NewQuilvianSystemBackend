namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums
{
    // OPT-BP-001, BE-OPT-002. Seluruh enum disimpan sebagai int (HasConversion<int>) —
    // 02-backend-architecture.md bagian 4.4. Nilai tidak boleh digeser; nilai baru selalu di ujung.

    /// <summary>Status pemeriksaan refraksi (<c>OPT-STATE-v1</c> bagian 1).</summary>
    public enum OptRefractionExamStatus
    {
        Draft = 1,
        Final = 2,
        Cancelled = 3
    }

    /// <summary>Jenis resep Optik.</summary>
    public enum OptPrescriptionType
    {
        Spectacle = 1,
        ContactLens = 2
    }

    /// <summary>Asal resep: dibuat RO/Sp.M rumah sakit, atau disalin dari resep luar (<c>OPT-DEC-017</c>).</summary>
    public enum OptPrescriptionOrigin
    {
        Internal = 1,
        External = 2
    }

    /// <summary>Jenis pesanan (<c>OPT-DEC-015</c>, <c>OPT-DEC-020</c>).</summary>
    public enum OptOrderKind
    {
        Fabrication = 1,
        DirectSale = 2,
        Remake = 3
    }

    /// <summary>Status pesanan (<c>OPT-DEC-016</c>, <c>OPT-STATE-v1</c> bagian 3).</summary>
    public enum OptOrderStatus
    {
        Draft = 1,
        AwaitingDownPayment = 2,
        InFabrication = 3,
        ReadyForPickup = 4,
        HandedOver = 5,
        Cancelled = 6,
        NotCollected = 7
    }

    /// <summary>Status satu baris barang pesanan.</summary>
    public enum OptOrderItemStatus
    {
        Active = 1,
        Cancelled = 2
    }

    /// <summary>Kategori barang Optik (<c>OPT-DEC-031</c>).</summary>
    public enum OptItemCategory
    {
        Frame = 1,
        SpectacleLens = 2,
        ContactLens = 3,
        Accessory = 4,
        ReadyMadeGlasses = 5
    }

    /// <summary>Asal lensa kacamata: stok depo Optik atau pesanan khusus lab luar (<c>OPT-DEC-013</c>, <c>OPT-DEC-038</c>).</summary>
    public enum OptLensSupply
    {
        Stock = 1,
        Custom = 2
    }

    /// <summary>Desain lensa kacamata.</summary>
    public enum OptLensDesign
    {
        SingleVision = 1,
        Bifocal = 2,
        Progressive = 3
    }

    /// <summary>Tujuan pengerjaan lensa (<c>OPT-DEC-013</c>).</summary>
    public enum OptWorkshopType
    {
        InHouse = 1,
        ExternalLab = 2
    }

    /// <summary>Status pengajuan batal sesudah pengerjaan (<c>OPT-DEC-010</c>).</summary>
    public enum OptCancellationStatus
    {
        Requested = 1,
        Approved = 2,
        Rejected = 3
    }

    /// <summary>Jenis keluhan garansi (<c>OPT-DEC-041</c>). <see cref="Damage"/> tidak pernah bergaransi.</summary>
    public enum OptComplaintType
    {
        FitDiscomfort = 1,
        LensDefect = 2,
        FrameDefect = 3,
        Damage = 4
    }

    /// <summary>Status keluhan garansi (<c>OPT-STATE-v1</c> bagian 4).</summary>
    public enum OptWarrantyClaimStatus
    {
        Submitted = 1,
        Examined = 2,
        Approved = 3,
        Rejected = 4
    }
}
