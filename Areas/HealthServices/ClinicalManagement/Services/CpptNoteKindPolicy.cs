using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Pemetaan antara <b>profesi penulis</b> dan <b>jenis catatan</b> yang sah baginya pada
    /// lembar terpadu — <c>BE-RWI-094</c>, <c>VAL-DOK-59</c>, <c>RWI-DEC-141</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa aturannya dikumpulkan di satu tempat.</b> Pemeriksaan yang sama dipakai pada
    /// jalur buat dan jalur ubah catatan terpadu, dan akan dipakai juga oleh sub-modul
    /// <c>keperawatan</c> lewat <c>FR-KEP-077</c>. Menyalinnya ke setiap jalur berarti beberapa
    /// salinan yang dapat berbeda isi, dan perbedaan sekecil apa pun di sini berujung pada
    /// catatan perawat yang tersimpan sebagai catatan dokter.
    /// </para>
    /// <para>
    /// <b>Yang dijaga aturan ini, dengan contoh.</b> Ns. Siti menulis SOAP keperawatan Tn. Budi
    /// dan mengirim <c>NoteKind = NursingSoap</c> dengan <c>ProfessionType = "Nurse"</c> →
    /// diterima. Bila ia mengirim <c>NoteKind = PhysicianNote</c> → ditolak <c>400</c>, karena
    /// catatan perkembangan dokter bukan bentuk catatan yang ia tulis. Sebaliknya, dr. Yoga yang
    /// mengirim <c>NursingSoap</c> juga ditolak.
    /// </para>
    /// <para>
    /// <b><see cref="CpptNoteKind.Unspecified"/> selalu sah, dan itu disengaja.</b> Entri lama
    /// bernilai demikian, dan permintaan ubah atas entri lama yang tidak menyebutkan jenis tidak
    /// boleh gagal hanya karena penulisnya dahulu tidak pernah menyatakannya. Menaikkannya
    /// menjadi jenis tertentu berdasarkan profesi <b>bukan</b> pekerjaan aturan ini — itu akan
    /// menjadi tebakan atas data klinis lama.
    /// </para>
    /// <para>
    /// Kelas statis, tanpa interface, mengikuti pola pembantu pada repository ini.
    /// </para>
    /// </remarks>
    public static class CpptNoteKindPolicy
    {
        /// <summary>
        /// Jenis catatan yang sah bagi sebuah profesi.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Profesi dibandingkan apa adanya terhadap nilai <c>ProfessionType</c> yang sudah
        /// dipakai tabel CPPT: <c>Doctor</c>, <c>Nurse</c>, <c>Midwife</c>, <c>Pharmacist</c>,
        /// <c>Nutritionist</c>, <c>Physiotherapist</c>, <c>Laboratory</c>, <c>Radiology</c>,
        /// <c>Other</c>.
        /// </para>
        /// <para>
        /// <b>Bidan disandingkan dengan perawat, bukan dengan dokter.</b> Dokumentasi
        /// kebidanan di bangsal berbentuk SOAP dan catatan naratif, sama seperti keperawatan;
        /// menyandingkannya dengan dokter akan membuka jenis catatan perkembangan dokter bagi
        /// profesi yang tidak memegang tanggung jawab itu.
        /// </para>
        /// </remarks>
        /// <param name="professionType">Profesi penulis, apa adanya seperti tersimpan.</param>
        public static IReadOnlyList<CpptNoteKind> JenisYangSah(string? professionType)
        {
            var profesi = (professionType ?? string.Empty).Trim();

            if (string.Equals(profesi, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    CpptNoteKind.Unspecified,
                    CpptNoteKind.PhysicianNote
                };
            }

            if (string.Equals(profesi, "Nurse", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(profesi, "Midwife", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    CpptNoteKind.Unspecified,
                    CpptNoteKind.NursingSoap,
                    CpptNoteKind.NursingNarrative
                };
            }

            // Farmasi, gizi, fisioterapi, laboratorium, radiologi, dan profesi lain. Satu jenis
            // untuk seluruhnya; pembedanya kolom ProfessionType yang sudah ada.
            return new[]
            {
                CpptNoteKind.Unspecified,
                CpptNoteKind.OtherProfessionNote
            };
        }

        /// <summary>
        /// Jenis bawaan bagi sebuah profesi, dipakai ketika permintaan tidak menyebutkan jenis.
        /// </summary>
        /// <remarks>
        /// Bukan tebakan atas isi catatan: profesi penulisnya dinyatakan sendiri pada permintaan
        /// yang sama, dan yang dipilih di sini adalah satu-satunya jenis yang wajar bagi profesi
        /// itu. Perawat menjadi pengecualian — ia punya dua bentuk catatan yang sah, sehingga
        /// bawaan bagi perawat adalah <see cref="CpptNoteKind.NursingNarrative"/>, bentuk yang
        /// paling tidak mengklaim struktur yang mungkin tidak ada.
        /// </remarks>
        /// <param name="professionType">Profesi penulis.</param>
        public static CpptNoteKind JenisBawaan(string? professionType)
        {
            var profesi = (professionType ?? string.Empty).Trim();

            if (string.Equals(profesi, "Doctor", StringComparison.OrdinalIgnoreCase))
                return CpptNoteKind.PhysicianNote;

            if (string.Equals(profesi, "Nurse", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(profesi, "Midwife", StringComparison.OrdinalIgnoreCase))
            {
                return CpptNoteKind.NursingNarrative;
            }

            return CpptNoteKind.OtherProfessionNote;
        }

        /// <summary>
        /// Menjawab apakah pasangan profesi dan jenis catatan itu sah.
        /// </summary>
        public static bool Sah(string? professionType, CpptNoteKind noteKind) =>
            JenisYangSah(professionType).Contains(noteKind);

        /// <summary>
        /// Menjawab apakah jenis boleh dipakai pada catatan <b>baru</b>. Unspecified hanya
        /// disediakan bagi baris legacy hasil migration dan tidak dapat dipilih untuk data baru.
        /// </summary>
        public static bool SahUntukCatatanBaru(string? professionType, CpptNoteKind noteKind) =>
            noteKind != CpptNoteKind.Unspecified && Sah(professionType, noteKind);

        /// <summary>
        /// Kalimat penolakan <c>VAL-DOK-59</c>, menyebut jenis apa saja yang sah bagi profesi
        /// itu — bukan sekadar menyatakan nilainya tidak valid.
        /// </summary>
        /// <remarks>
        /// Pengguna yang ditolak di sini sedang salah memilih pada satu daftar pendek. Menyebut
        /// pilihan yang benar membuat penolakannya dapat langsung diperbaiki; menuliskan "jenis
        /// catatan tidak valid" memaksanya menebak.
        /// </remarks>
        public static string Penolakan(string? professionType, CpptNoteKind noteKind)
        {
            var sah = JenisYangSah(professionType)
                .Where(x => x != CpptNoteKind.Unspecified)
                .Select(NamaJenis);

            return $"Jenis catatan \"{NamaJenis(noteKind)}\" tidak sah bagi profesi " +
                   $"\"{(professionType ?? string.Empty).Trim()}\". Jenis yang dapat dipilih: " +
                   $"{string.Join(", ", sah)}.";
        }

        /// <summary>
        /// Label jenis catatan yang siap ditampilkan. Ditentukan server supaya layar tidak
        /// menerjemahkan nilai enum sendiri-sendiri.
        /// </summary>
        public static string NamaJenis(CpptNoteKind noteKind) => noteKind switch
        {
            CpptNoteKind.PhysicianNote => "Catatan Perkembangan Dokter",
            CpptNoteKind.NursingSoap => "SOAP Keperawatan",
            CpptNoteKind.NursingNarrative => "Catatan Keperawatan",
            CpptNoteKind.OtherProfessionNote => "Catatan Profesi Lain",
            _ => "Tidak Ditentukan"
        };
    }
}
