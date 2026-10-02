namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Amplop kejadian yang dikeluarkan modul Operasi ke outbox-nya (`BE-OPR-009`, `OPR-INT-001`
/// dan `OPR-INT-002`).
/// </summary>
/// <remarks>
/// <para>
/// Kontrak ini <b>internal</b>. Ia menyatakan apa yang terjadi di kamar operasi memakai
/// istilah modul Operasi sendiri, dan berhenti di situ. Penerjemahan ke Billing, Inventory,
/// SATUSEHAT, atau consumer lain adalah pekerjaan integration layer, bukan pekerjaan domain
/// Operasi. Karena itu tidak ada satu pun nama, kode, atau struktur milik consumer tertentu di
/// dalam berkas ini, dan modul Operasi tidak pernah bergantung pada ketersediaan mereka.
/// </para>
/// <para>
/// Amplopnya disimpan apa adanya pada baris outbox. Itu disengaja: kejadian harus terbaca
/// sebagaimana keadaannya saat itu, bukan sebagaimana keadaan basis data ketika seseorang
/// akhirnya membacanya. Jadwal berubah, tim berganti, tindakan ditambah — dan pesan yang sudah
/// terlanjur dikirim tidak boleh ikut berubah bersama mereka.
/// </para>
/// </remarks>
public class OprIntegrationEvent
{
    /// <summary>Identitas kejadian. Consumer memakainya untuk membuang kiriman ganda.</summary>
    public Guid EventId { get; set; }

    /// <summary>Nama kejadian bisnisnya, misalnya `operating-room.material-usage.recorded`.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Versi bentuk amplop ini. Bertambah hanya bila bentuknya berubah tidak kompatibel.</summary>
    public string EventVersion { get; set; } = string.Empty;

    /// <summary>Waktu kejadiannya, UTC. Bukan waktu pengirimannya.</summary>
    public DateTime OccurredAt { get; set; }

    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }

    /// <summary>
    /// Permintaan tindakan yang melahirkan kasus ini, yaitu `TrxPatientProcedure` milik tindakan
    /// utamanya.
    /// </summary>
    /// <remarks>
    /// Modul Operasi tidak memiliki entitas "service request" tersendiri; ia memakai ulang
    /// tindakan pasien yang sudah ada, sebagaimana ditetapkan blueprint. Kosong bila kasusnya
    /// belum memiliki tindakan utama.
    /// </remarks>
    public Guid? ServiceRequestId { get; set; }

    public OprCaseStatusSnapshot Case { get; set; } = new();

    /// <summary>Tindakan yang tercatat pada kasus, urut sesuai urutannya.</summary>
    public List<OprIntegrationEventProcedure> Procedures { get; set; } = [];

    /// <summary>Pelaksana yang sedang berlaku pada kasus saat kejadian ini terjadi.</summary>
    public List<OprIntegrationEventPerformer> Performers { get; set; } = [];

    /// <summary>Isi khusus kejadian ini. Terisi hanya untuk kejadian pemakaian material.</summary>
    public OprIntegrationEventMaterial? Material { get; set; }

    /// <summary>Isi khusus kejadian komponen tagihan.</summary>
    public OprIntegrationEventCharge? Charge { get; set; }
}

/// <summary>Keadaan kasus saat kejadian terjadi, secukupnya untuk dibaca consumer.</summary>
public class OprCaseStatusSnapshot
{
    public string Status { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string CaseType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Laterality { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class OprIntegrationEventProcedure
{
    public Guid PatientProcedureId { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int Sequence { get; set; }
}

public class OprIntegrationEventPerformer
{
    public Guid WorkforceId { get; set; }

    /// <summary>Peran dalam tim operasi, misalnya `PrimarySurgeon`.</summary>
    public string Role { get; set; } = string.Empty;

    public bool IsLead { get; set; }
}

public class OprIntegrationEventMaterial
{
    public Guid UsageId { get; set; }
    public Guid ExternalItemId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public Guid? UnitMeasurementId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public int Revision { get; set; }
    public Guid RecordedBy { get; set; }
}

public class OprIntegrationEventCharge
{
    /// <summary>Komponen yang ditagihkan, misalnya `procedure`.</summary>
    public string Component { get; set; } = string.Empty;

    public int Revision { get; set; }
}
