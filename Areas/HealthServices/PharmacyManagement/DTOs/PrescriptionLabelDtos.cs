namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

/// <summary>
/// Bahan cetak etiket obat untuk satu resep.
/// </summary>
/// <remarks>
/// Yang disediakan di sini adalah datanya, bukan tata letaknya. Ukuran label dan susunannya
/// milik layar cetak, karena keduanya bergantung pada kertas dan pencetak yang dipakai
/// masing-masing instalasi.
/// </remarks>
public class PrescriptionLabelResponse
{
    public Guid PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateTime PrescriptionDateTime { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    /// <summary>Identitas penerbit, diambil dari master lokasi rumah sakit.</summary>
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? SitePhoneNumber { get; set; }

    public List<PrescriptionLabelItemResponse> Items { get; set; } = [];
}

public class PrescriptionLabelItemResponse
{
    public Guid Id { get; set; }
    public int LineNumber { get; set; }

    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DrugForm { get; set; }

    /// <summary>
    /// Rute pemberian, bila tercatat pada master obat.
    /// </summary>
    /// <remarks>
    /// Sering kosong: sebagian besar baris master obat belum mengisinya. Karena itu warna
    /// etiket tidak diturunkan dari sini melainkan dipilih petugas, dengan nilai ini sebagai
    /// petunjuk bila tersedia.
    /// </remarks>
    public string? Route { get; set; }

    public decimal Quantity { get; set; }
    public string? DispenseUnit { get; set; }

    /// <summary>Aturan pakai sebagaimana ditulis pemberi resep.</summary>
    public string? Signa { get; set; }
    public string? FrequencyText { get; set; }

    /// <summary>Petunjuk pemberian dari master obat, misalnya "sesudah makan".</summary>
    public string? AdministrationInstruction { get; set; }

    /// <summary>
    /// Penanda yang menuntut kehati-hatian tambahan saat penyerahan.
    /// </summary>
    /// <remarks>
    /// Dibawa apa adanya dari snapshot resep, bukan dibaca ulang dari master: etiket harus
    /// mencerminkan obat sebagaimana diresepkan saat itu.
    /// </remarks>
    public bool IsNarcotic { get; set; }
    public bool IsPsychotropic { get; set; }
    public bool IsHighAlert { get; set; }
    public bool IsAntibiotic { get; set; }
}
