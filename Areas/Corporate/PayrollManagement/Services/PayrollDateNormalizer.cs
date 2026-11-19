namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Menormalkan tanggal kalender periode payroll supaya aman ditulis atau dibandingkan dengan kolom <c>timestamp with time zone</c>.
///
/// Masalahnya: tanggal periode (<c>MstPayrollPeriod.StartDate</c>, <c>EndDate</c>, <c>AttendanceCutoffDate</c>,
/// <c>VariableInputCutoffDate</c>, <c>PaymentDate</c>) berkolom <c>date</c>, dan Npgsql membacanya sebagai <see cref="DateTimeKind.Unspecified"/>.
/// Npgsql menolak menulis nilai berkind itu ke <c>timestamp with time zone</c>: snapshot tanggal pada <c>PrlPayrollRun</c>, dan
/// perbandingan dengan kolom berlaku <c>PrlPayroll</c>/<c>PrlTransportAllowance</c> (juga <c>timestamptz</c>), gagal dengan
/// "Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'".
///
/// Konvensi: tanggal kalender dicap UTC tengah malam (<c>2026-11-01</c> menjadi <c>2026-11-01T00:00:00Z</c>), BUKAN dikonversi dari zona
/// waktu aplikasi. Pembaca hilir (<c>.Month</c>, <c>.Year</c>, <c>DateOnly.FromDateTime</c> pada snapshot run, tanggal berlaku profil
/// yang disiapkan otomatis) membaca nilai itu sebagai tanggal kalender; konversi WIB akan menggeser tanggal satu hari ke belakang
/// (<c>2026-10-31T17:00Z</c>) dan mengubah bulan periode. Karena itu <c>AppDateTimeHelper.ToUtc</c> (konversi instan waktu dari zona
/// aplikasi) sengaja tidak dipakai untuk tanggal kalender periode.
/// </summary>
public static class PayrollDateNormalizer
{
    /// <summary>Tanggal kalender <paramref name="value"/> pada UTC tengah malam. Bagian jam dan Kind asal diabaikan.</summary>
    public static DateTime ToUtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    /// <summary>Versi nullable: <c>null</c> tetap <c>null</c>.</summary>
    public static DateTime? ToUtcDate(DateTime? value) => value.HasValue ? ToUtcDate(value.Value) : null;
}
