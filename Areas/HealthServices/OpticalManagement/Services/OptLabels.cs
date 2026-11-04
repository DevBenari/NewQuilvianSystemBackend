using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Label Indonesia nilai enum Optik untuk metadata layar dan kalimat log. Satu-satunya tempat label ini
    /// ditulis, sehingga frontend tidak meng-hardcode label (<c>03-frontend-architecture.md</c> bagian 3).
    /// </summary>
    public static class OptLabels
    {
        public static string ItemCategory(OptItemCategory value) => value switch
        {
            OptItemCategory.Frame => "Frame",
            OptItemCategory.SpectacleLens => "Lensa Kacamata",
            OptItemCategory.ContactLens => "Lensa Kontak",
            OptItemCategory.Accessory => "Aksesoris & Cairan",
            OptItemCategory.ReadyMadeGlasses => "Kacamata Jadi",
            _ => "Tidak diketahui"
        };

        public static string LensDesign(OptLensDesign value) => value switch
        {
            OptLensDesign.SingleVision => "Single Vision",
            OptLensDesign.Bifocal => "Bifokal",
            OptLensDesign.Progressive => "Progresif",
            _ => "Tidak diketahui"
        };

        public static string LensSupply(OptLensSupply value) => value switch
        {
            OptLensSupply.Stock => "Lensa Stok",
            OptLensSupply.Custom => "Lensa Custom",
            _ => "Tidak diketahui"
        };

        public static string WorkshopType(OptWorkshopType value) => value switch
        {
            OptWorkshopType.InHouse => "Bengkel Sendiri",
            OptWorkshopType.ExternalLab => "Lab Luar",
            _ => "Tidak diketahui"
        };

        public static string OrderKind(OptOrderKind value) => value switch
        {
            OptOrderKind.Fabrication => "Pesanan Berpengerjaan",
            OptOrderKind.DirectSale => "Penjualan Langsung",
            OptOrderKind.Remake => "Remake",
            _ => "Tidak diketahui"
        };

        public static string OrderStatus(OptOrderStatus value) => value switch
        {
            OptOrderStatus.Draft => "Draf",
            OptOrderStatus.AwaitingDownPayment => "Menunggu Uang Muka",
            OptOrderStatus.InFabrication => "Dalam Pengerjaan",
            OptOrderStatus.ReadyForPickup => "Siap Diambil",
            OptOrderStatus.HandedOver => "Diserahkan",
            OptOrderStatus.Cancelled => "Batal",
            OptOrderStatus.NotCollected => "Tidak Diambil",
            _ => "Tidak diketahui"
        };

        public static string OrderItemStatus(OptOrderItemStatus value) => value switch
        {
            OptOrderItemStatus.Active => "Aktif",
            OptOrderItemStatus.Cancelled => "Batal",
            _ => "Tidak diketahui"
        };

        public static string RefractionExamStatus(OptRefractionExamStatus value) => value switch
        {
            OptRefractionExamStatus.Draft => "Draf",
            OptRefractionExamStatus.Final => "Final",
            OptRefractionExamStatus.Cancelled => "Batal",
            _ => "Tidak diketahui"
        };

        public static string PrescriptionType(OptPrescriptionType value) => value switch
        {
            OptPrescriptionType.Spectacle => "Kacamata",
            OptPrescriptionType.ContactLens => "Lensa Kontak",
            _ => "Tidak diketahui"
        };

        public static string PrescriptionOrigin(OptPrescriptionOrigin value) => value switch
        {
            OptPrescriptionOrigin.Internal => "Resep Internal",
            OptPrescriptionOrigin.External => "Resep Luar",
            _ => "Tidak diketahui"
        };

        public static string CancellationStatus(OptCancellationStatus value) => value switch
        {
            OptCancellationStatus.Requested => "Diajukan",
            OptCancellationStatus.Approved => "Disetujui",
            OptCancellationStatus.Rejected => "Ditolak",
            _ => "Tidak diketahui"
        };

        public static string ComplaintType(OptComplaintType value) => value switch
        {
            OptComplaintType.FitDiscomfort => "Ketidaknyamanan/Ukuran",
            OptComplaintType.LensDefect => "Cacat Lensa/Lapisan",
            OptComplaintType.FrameDefect => "Cacat Frame",
            OptComplaintType.Damage => "Kerusakan (Benturan/Pemakaian)",
            _ => "Tidak diketahui"
        };

        public static string WarrantyClaimStatus(OptWarrantyClaimStatus value) => value switch
        {
            OptWarrantyClaimStatus.Submitted => "Diajukan",
            OptWarrantyClaimStatus.Examined => "Diperiksa",
            OptWarrantyClaimStatus.Approved => "Disetujui",
            OptWarrantyClaimStatus.Rejected => "Ditolak",
            _ => "Tidak diketahui"
        };
    }
}
