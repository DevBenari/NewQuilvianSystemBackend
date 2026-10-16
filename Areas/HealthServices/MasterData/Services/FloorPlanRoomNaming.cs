using System.Text.RegularExpressions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>
/// Aturan murni nama kamar, label lantai/lokasi, label bed, dan penanda turunan jenis kelas untuk
/// kamar yang tergambar di denah (backend 15.6). Tidak mengakses database dan tidak membuka
/// transaksi, sehingga editor master, aktivasi ruang cadangan, dan importer memakai satu aturan.
/// </summary>
/// <remarks>
/// Contoh: kamar bernomor <c>329</c> berkelas "Kelas 2" di lantai <c>3</c>, wing Barat Laut,
/// bernama "Kelas 2 329", berlantai "Lantai 3", berlokasi "Lantai 3 · Barat Laut", dan bed
/// berikutnya sesudah 329-A, 329-B, 329-C berlabel "329-D".
/// </remarks>
public static class FloorPlanRoomNaming
{
    public const int FloorCodeMaxLength = 10;
    public const int RoomKeyMaxLength = 100;
    private const int RoomNameMaxLength = 150;

    private static readonly Regex FloorCodePattern = new("^[A-Za-z0-9]+$", RegexOptions.CultureInvariant);
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant);
    private static readonly Regex NumericRoomNumberPattern = new("^[0-9]+$", RegexOptions.CultureInvariant);
    private static readonly Regex BedLetterPattern = new("^[A-Z]+$", RegexOptions.CultureInvariant);

    /// <summary>Kode lantai: angka atau huruf, maksimum 10 karakter, mis. <c>3</c>.</summary>
    public static bool IsValidFloorCode(string? floorCode) =>
        !string.IsNullOrWhiteSpace(floorCode) &&
        floorCode.Trim().Length <= FloorCodeMaxLength &&
        FloorCodePattern.IsMatch(floorCode.Trim());

    /// <summary>
    /// Memeriksa kunci denah <c>lantai-&lt;FloorCode&gt;/&lt;slug&gt;</c> (VAL-DNH-01, VAL-DNH-03).
    /// Mengembalikan <c>null</c> bila sah, atau kode dan pesan penolakan.
    /// </summary>
    /// <example><c>lantai-3/k-329</c> sah untuk lantai <c>3</c>; <c>Lantai-3/K-329</c> dan <c>lantai-2/k-329</c> ditolak.</example>
    public static (string Code, string Message)? ValidateRoomKey(string? key, string? floorCode)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (string.IsNullOrWhiteSpace(floorCode))
            return ("FLOOR_CODE_REQUIRED", "Kunci denah wajib disertai kode lantai.");
        if (!IsValidFloorCode(floorCode))
            return ("INVALID_INPUT", "Kode lantai hanya boleh angka atau huruf, maksimum 10 karakter.");

        var trimmed = key.Trim();
        var prefix = "lantai-" + floorCode.Trim().ToLowerInvariant() + "/";
        if (trimmed.Length > RoomKeyMaxLength || !trimmed.StartsWith(prefix, StringComparison.Ordinal) ||
            !SlugPattern.IsMatch(trimmed[prefix.Length..]))
            return ("INVALID_INPUT",
                $"Kunci denah harus berbentuk '{prefix}<nama>' dengan huruf kecil, angka, atau tanda hubung, maksimum {RoomKeyMaxLength} karakter.");
        return null;
    }

    /// <summary>"Lantai 3".</summary>
    public static string FloorName(string floorCode) => "Lantai " + floorCode.Trim();

    /// <summary>"Lantai 3 · Barat Laut"; tanpa arah wing cukup "Lantai 3".</summary>
    public static string LocationName(string floorCode, RoomWingDirection? wingDirection) =>
        wingDirection.HasValue && Enum.IsDefined(wingDirection.Value)
            ? FloorName(floorCode) + " · " + WingLabel(wingDirection.Value)
            : FloorName(floorCode);

    public static string WingLabel(RoomWingDirection wingDirection) => wingDirection switch
    {
        RoomWingDirection.Utara => "Utara",
        RoomWingDirection.TimurLaut => "Timur Laut",
        RoomWingDirection.Timur => "Timur",
        RoomWingDirection.Tenggara => "Tenggara",
        RoomWingDirection.Selatan => "Selatan",
        RoomWingDirection.BaratDaya => "Barat Daya",
        RoomWingDirection.Barat => "Barat",
        RoomWingDirection.BaratLaut => "Barat Laut",
        _ => wingDirection.ToString()
    };

    /// <summary>Nomor kamar berupa angka saja, mis. <c>329</c>. Kode ruang seperti <c>ICU-CK</c> bukan nomor angka.</summary>
    public static bool IsNumericRoomNumber(string? roomNumber) =>
        !string.IsNullOrWhiteSpace(roomNumber) && NumericRoomNumberPattern.IsMatch(roomNumber.Trim());

    /// <summary>
    /// Nama kamar berkunci: kamar bernomor angka bernama <c>&lt;nama kelas&gt; &lt;nomor&gt;</c>;
    /// kamar tanpa nomor angka memakai nama ruang fisik yang tetap (RWI-DEC-308).
    /// </summary>
    /// <example>("Kelas 2", "329", null) → "Kelas 2 329"; ("Kelas 1", "ICU-CK", "R. ICU CK") → "R. ICU CK".</example>
    public static string RoomName(string? patientClassName, string? roomNumber, string? fixedRoomName)
    {
        string name;
        if (IsNumericRoomNumber(roomNumber) && !string.IsNullOrWhiteSpace(patientClassName))
            name = patientClassName.Trim() + " " + roomNumber!.Trim();
        else if (!string.IsNullOrWhiteSpace(fixedRoomName))
            name = fixedRoomName.Trim();
        else if (!string.IsNullOrWhiteSpace(patientClassName) && !string.IsNullOrWhiteSpace(roomNumber))
            name = patientClassName.Trim() + " " + roomNumber.Trim();
        else
            name = roomNumber?.Trim() ?? string.Empty;
        return name.Length <= RoomNameMaxLength ? name : name[..RoomNameMaxLength];
    }

    /// <summary>Label bed yang dibaca petugas, mis. "329-C".</summary>
    public static string BedLabel(string roomNumber, string bedLetter) => roomNumber.Trim() + "-" + bedLetter;

    /// <summary>Huruf bed bila nilainya huruf besar saja (A, B, …, Z, AA).</summary>
    public static string? NormalizeBedLetter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var upper = value.Trim().ToUpperInvariant();
        return BedLetterPattern.IsMatch(upper) ? upper : null;
    }

    /// <summary>Huruf bed dari label berawalan nomor kamar, mis. ("329-C", "329") → "C".</summary>
    public static string? LetterFromLabel(string? bedName, string? roomNumber)
    {
        if (string.IsNullOrWhiteSpace(bedName) || string.IsNullOrWhiteSpace(roomNumber)) return null;
        var prefix = roomNumber.Trim() + "-";
        var trimmed = bedName.Trim();
        return trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? NormalizeBedLetter(trimmed[prefix.Length..]) : null;
    }

    /// <summary>
    /// Huruf berikutnya sesudah huruf terbesar yang sudah dipakai awalan yang sama di lantai itu,
    /// termasuk bed nonaktif; A–Z lalu AA, AB.
    /// </summary>
    /// <example>Kamar 321 pertama memakai A–C, maka kamar 321 kedua mulai dari "D".</example>
    public static string NextBedLetter(IEnumerable<string?> usedLetters)
    {
        var max = usedLetters.Select(NormalizeBedLetter).Where(x => x != null).Select(x => LetterToNumber(x!)).DefaultIfEmpty(0).Max();
        return NumberToLetter(max + 1);
    }

    public static int LetterToNumber(string letters)
    {
        var value = 0;
        foreach (var c in letters) value = value * 26 + (c - 'A' + 1);
        return value;
    }

    public static string NumberToLetter(int number)
    {
        var result = string.Empty;
        while (number > 0)
        {
            var remainder = (number - 1) % 26;
            result = (char)('A' + remainder) + result;
            number = (number - 1) / 26;
        }
        return result;
    }

    /// <summary>
    /// Menulis ulang penanda kamar turunan jenis kelas (tabel backend 15.6): ICU → ruang intensif,
    /// Isolasi → ruang isolasi, Perinatologi → ruang bayi, ODC → ruang ODC, kelas biasa → kamar rawat inap.
    /// </summary>
    public static void ApplyRoomFlags(MstRoom room, PatientClassType? classType)
    {
        room.RoomType = classType switch
        {
            PatientClassType.ICU => RoomType.IntensiveCareRoom,
            PatientClassType.Isolation => RoomType.IsolationRoom,
            PatientClassType.Perinatology => RoomType.BabyRoom,
            PatientClassType.Odc => RoomType.OdcRoom,
            _ => RoomType.InpatientRoom
        };
        room.IsIntensiveCare = classType == PatientClassType.ICU;
        room.IsIsolationRoom = classType == PatientClassType.Isolation;
        room.IsForNewborn = classType == PatientClassType.Perinatology;
        room.IsOdcRoom = classType == PatientClassType.Odc;
    }

    /// <summary>
    /// Menulis ulang penanda bed turunan jenis kelas. Boks rawat gabung (<paramref name="isCrib"/>)
    /// tetap boks setelah kelas kamar berubah; kamar Perinatologi selalu untuk bayi.
    /// </summary>
    public static void ApplyBedFlags(MstBed bed, PatientClassType? classType, bool isCrib)
    {
        bed.IsIntensiveCareBed = classType == PatientClassType.ICU;
        bed.IsIsolationBed = classType == PatientClassType.Isolation;
        bed.IsOdcBed = classType == PatientClassType.Odc;
        bed.IsForNewborn = isCrib || classType == PatientClassType.Perinatology;
    }
}
