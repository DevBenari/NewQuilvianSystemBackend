namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Pembentuk isi QR pasien dan nomor rekam medis terformat — diekstrak dari
    /// <c>PatientController.BuildPatientQrPayload</c> tanpa mengubah perilakunya (<c>BE-RWI-187</c>,
    /// kontrak <c>episode-rawat-inap</c> <c>0.11.0</c> backend 13.8.4, <c>RWI-DEC-259</c>, <c>266</c>).
    /// </summary>
    /// <remarks>
    /// Isi QR hanya No. RM terformat — tanpa nama, tanggal lahir, maupun ID acak — dan dirender
    /// ulang dari No. RM setiap kali dicetak, sehingga pasien lama tanpa berkas QR tetap dapat
    /// dicetak gelangnya.
    ///
    /// <para>
    /// Contoh: No. RM tersimpan <c>00123456</c> → isi QR <c>00-12-34-56</c>; tersimpan
    /// <c>00-12-34-56</c> → tetap <c>00-12-34-56</c>; enam angka <c>123456</c> → <c>12-34-56</c>.
    /// Panjang angka lain dikembalikan apa adanya.
    /// </para>
    /// </remarks>
    public static class PatientQrPayloadBuilder
    {
        /// <summary>Isi QR pasien. Melempar galat bila nomor rekam medis kosong, sama seperti method lama.</summary>
        public static string Build(string medicalRecordNumber)
        {
            if (string.IsNullOrWhiteSpace(medicalRecordNumber))
            {
                throw new InvalidOperationException("Nomor rekam medis tidak tersedia untuk payload QR.");
            }

            var rawNumber = NormalizeToRawDigits(medicalRecordNumber);

            if (!string.IsNullOrWhiteSpace(rawNumber))
            {
                return FormatMedicalRecordNumber(rawNumber);
            }

            return medicalRecordNumber.Trim();
        }

        /// <summary>Angka saja dari nomor rekam medis; kosong bila tidak ada angka sama sekali.</summary>
        public static string? NormalizeToRawDigits(string? medicalRecordNumber)
        {
            if (string.IsNullOrWhiteSpace(medicalRecordNumber))
            {
                return null;
            }

            var digits = new string(medicalRecordNumber.Where(char.IsDigit).ToArray());

            return string.IsNullOrWhiteSpace(digits)
                ? null
                : digits;
        }

        /// <summary>Format tampilan nomor rekam medis: delapan angka <c>00-00-00-00</c>, enam angka <c>00-00-00</c>.</summary>
        public static string FormatMedicalRecordNumber(string rawNumber)
        {
            if (string.IsNullOrWhiteSpace(rawNumber))
            {
                return rawNumber;
            }

            var digits = new string(rawNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 8)
            {
                return $"{digits[..2]}-{digits.Substring(2, 2)}-{digits.Substring(4, 2)}-{digits.Substring(6, 2)}";
            }

            if (digits.Length == 6)
            {
                return $"{digits[..2]}-{digits.Substring(2, 2)}-{digits.Substring(4, 2)}";
            }

            return rawNumber;
        }
    }
}
