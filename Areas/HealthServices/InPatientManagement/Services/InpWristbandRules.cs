using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>Jenis gelang identitas pasien.</summary>
    public enum InpWristbandKind
    {
        Adult = 1,
        Infant = 2
    }

    /// <summary>
    /// Jenis gelang, sapaan, dan teks umur — <c>BE-RWI-196</c>, <c>RWI-DEC-243</c>, validation 15.9.
    /// Fungsi murni: hasil hanya ditentukan data pasien, tanggal cetak, dan batas umur pengaturan.
    /// </summary>
    /// <remarks>
    /// <b>Jenis gelang.</b> Gelang Bayi bila pasien bertanda bayi baru lahir atau umurnya paling
    /// tinggi <c>InfantWristbandMaxAgeYears</c> tahun (bawaan 5); selain itu Gelang Dewasa.
    ///
    /// <para>
    /// <b>Sapaan.</b> "BY. NY. <i>nama ibu</i>" untuk bayi baru lahir; "An." di bawah 17 tahun;
    /// "Tn." pria 17 tahun ke atas; "Ny." wanita 17 tahun ke atas berstatus menikah, cerai, janda,
    /// atau berpisah; "Nn." wanita 17 tahun ke atas belum menikah; tanpa sapaan bila status nikah
    /// tidak diketahui.
    /// </para>
    ///
    /// <para>
    /// Contoh (dicetak 7 Oktober 2026): Budi Santoso, pria, lahir 12 Maret 1981 → Gelang Dewasa
    /// "BUDI SANTOSO, Tn." · "12 Mar 1981" · "45 th". Bayi Ny. Rina umur 3 hari → Gelang Bayi
    /// "BY. NY. RINA SANTOSO" dan dua label kecil. Anak 4 tahun → Gelang Bayi bersapaan "An.".
    /// Wanita 30 tahun status nikah tidak diketahui → Gelang Dewasa tanpa sapaan.
    /// </para>
    ///
    /// <para>
    /// Aturan ini menyangkut identifikasi pasien dan wajib diverifikasi tim keselamatan pasien
    /// sebelum produksi (gerbang G-38).
    /// </para>
    /// </remarks>
    public static class InpWristbandRules
    {
        /// <summary>Ambang dewasa untuk sapaan, dari keputusan <c>RWI-DEC-243</c>.</summary>
        public const int AdultAgeYears = 17;

        /// <summary>Jumlah label kecil yang ikut Gelang Bayi.</summary>
        public const int InfantSmallLabelCount = 2;

        public static InpWristbandKind ResolveKind(
            bool isNewborn,
            DateTime? birthDate,
            DateTime today,
            int infantMaxAgeYears)
        {
            if (isNewborn)
            {
                return InpWristbandKind.Infant;
            }

            var age = AgeInYears(birthDate, today);

            // Tanpa tanggal lahir umur tidak dapat dibuktikan ≤ batas bayi; tidak ada tebakan.
            return age.HasValue && age.Value <= infantMaxAgeYears
                ? InpWristbandKind.Infant
                : InpWristbandKind.Adult;
        }

        /// <summary>Sapaan cetakan; kosong bila tidak dapat ditentukan dari data.</summary>
        public static string? ResolveSalutation(
            bool isNewborn,
            Gender? gender,
            MaritalStatus maritalStatus,
            DateTime? birthDate,
            DateTime today)
        {
            if (isNewborn)
            {
                return "By.";
            }

            var age = AgeInYears(birthDate, today);

            if (!age.HasValue)
            {
                return null;
            }

            if (age.Value < AdultAgeYears)
            {
                return "An.";
            }

            if (gender == Gender.Male)
            {
                return "Tn.";
            }

            if (gender == Gender.Female)
            {
                return maritalStatus switch
                {
                    MaritalStatus.Married or MaritalStatus.Divorced or MaritalStatus.Widowed or MaritalStatus.Separated => "Ny.",
                    MaritalStatus.Single => "Nn.",
                    _ => null
                };
            }

            return null;
        }

        /// <summary>
        /// Nama pada gelang. Bayi baru lahir yang ibunya tercatat: "BY. NY. <i>NAMA IBU</i>"; selain
        /// itu "<i>NAMA</i>, <i>sapaan</i>" atau nama saja bila sapaan tidak dapat ditentukan.
        /// </summary>
        public static string BuildDisplayName(
            string fullName,
            bool isNewborn,
            string? motherName,
            Gender? gender,
            MaritalStatus maritalStatus,
            DateTime? birthDate,
            DateTime today)
        {
            if (isNewborn && !string.IsNullOrWhiteSpace(motherName))
            {
                return $"BY. NY. {motherName.Trim().ToUpperInvariant()}";
            }

            var name = (fullName ?? string.Empty).Trim().ToUpperInvariant();

            // Bayi baru lahir tanpa ibu tercatat tidak diberi "BY. NY." karangan.
            var salutation = isNewborn
                ? null
                : ResolveSalutation(false, gender, maritalStatus, birthDate, today);

            return salutation == null ? name : $"{name}, {salutation}";
        }

        public static int? AgeInYears(DateTime? birthDate, DateTime today)
        {
            if (!birthDate.HasValue || birthDate.Value.Date > today.Date)
            {
                return null;
            }

            var birth = birthDate.Value.Date;
            var age = today.Year - birth.Year;

            if (birth > today.Date.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        /// <summary>Teks umur: "45 th", "4 bln", atau "3 hr" pada tanggal cetak.</summary>
        public static string? AgeText(DateTime? birthDate, DateTime today)
        {
            var years = AgeInYears(birthDate, today);

            if (!years.HasValue)
            {
                return null;
            }

            if (years.Value >= 1)
            {
                return $"{years.Value} th";
            }

            var birth = birthDate!.Value.Date;
            var months = ((today.Year - birth.Year) * 12) + today.Month - birth.Month;

            if (birth.AddMonths(months) > today.Date)
            {
                months--;
            }

            if (months >= 1)
            {
                return $"{months} bln";
            }

            return $"{(today.Date - birth).Days} hr";
        }
    }
}
