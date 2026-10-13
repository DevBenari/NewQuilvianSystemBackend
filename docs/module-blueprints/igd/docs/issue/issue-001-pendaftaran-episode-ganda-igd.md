# ISSUE-001 — Validasi Episode Ganda IGD Harus Muncul pada Langkah 2 Data Emergency Visit, Bukan Menunggu Langkah Verifikasi

```yaml
issue_id: ISSUE-IGD-001
module_id: igd
submodule: igd
layar: "Pendaftaran Pasien IGD — Langkah 2 Data Emergency Visit (FE-IGD-034/038, emergency-visit-step.jsx)"
sumber_laporan: "Laporan pelapor 12-10-2026: 1 keluhan utama disertai 1 tangkapan layar Langkah 2 Data Emergency Visit"
tanggal_issue: "2026-10-12"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "6a1c6cf9 (rizkiG)"
source_sha_frontend: "701ddfb5d (RizkiV2)"
rencana_perbaikan: ../plan-repair/plan-repair-001-pendaftaran-episode-ganda-igd.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Laporan ini menyoroti alur kerja pemeriksaan kunjungan aktif (*active episode / encounter*) pada pendaftaran pasien Instalasi Gawat Darurat (IGD). Saat ini, sistem baru menjalankan pemeriksaan episode aktif ketika petugas menekan tombol **Selesaikan Pendaftaran** di Langkah Verifikasi (langkah paling akhir). Hal ini mengakibatkan petugas yang mendaftarkan pasien dengan kunjungan aktif terhenti di akhir proses dan terpaksa mundur kembali ke Langkah 2 untuk mengisi kolom **Alasan Pendaftaran Episode Ganda**.

Di sisi lain, pada Langkah 2 (*Data Emergency Visit*), kolom isian **Alasan Pendaftaran Episode Ganda** saat ini selalu tampil aktif/terbuka untuk semua pasien, termasuk pasien baru atau pasien yang belum pernah memiliki kunjungan sama sekali. Jika petugas yang salah paham mengisi kolom tersebut pada pasien tanpa episode aktif, server backend justru akan menolak pendaftaran dengan galat *HTTP 400 Bad Request* karena alasan tidak boleh diisi jika tidak ada episode aktif.

Pelapor meminta agar peringatan (*alert*) kunjungan aktif dipindahkan ke Langkah 2, di mana kolom isian alasan pendaftaran episode ganda hanya aktif jika pasien memang memiliki kunjungan/encounter yang masih berjalan. Bila pasien tidak memiliki kunjungan aktif, kolom tersebut dinonaktifkan (*disabled*) atau dihilangkan agar tidak membingungkan petugas loket IGD.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "di pendaftaran pasien, harus nya ada alert dibagian ini jika pasien kunjungan ecounter nyaa masih berjalan sehingga dan text pengisian area ini nyala tapi jika ini pasien belum sama sekali ada kunjungan atau ecounters pasien maka text area ganda ini disabled atau diilangin lagi, jadi alert nyaa bukan ketika klik selesaikan pendaftaran pada section verification" | Lampiran 1: Screenshot Langkah 2 "Data Emergency Visit" (`media_1791798022453_3632a021.png`) |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-001-01` | 1 | Pra-cek episode berjalan terlambat di Langkah Verifikasi dan isian episode ganda selalu terbuka di Langkah 2 | `DESIGN_CHANGE`, `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-001-01` |
| `ISS-IGD-001-T1` | — | Potensi penolakan *HTTP 400 Bad Request* backend bila kolom alasan episode ganda terisi pada pasien tanpa episode aktif | `BUG` | Frontend | High | SUDAH-VERIFIKASI | `FIX-IGD-001-02` |
| `ISS-IGD-001-T2` | — | Kotak peringatan episode ganda pada Langkah Verifikasi perlu diselaraskan sebagai jaring pengaman sekunder | `DESIGN_CHANGE` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-IGD-001-03` |

Radius dampak umum: Perubahan berpusat pada komponen pendaftaran IGD di frontend:
1. `src/components/view/health-services/registration-management/emergency-registration/emergency-visit-step.jsx`
2. `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js`
3. `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-page.jsx`

Backend `EmergencyVisitController.cs` sudah menyediakan endpoint pra-cek baca-saja `GET /v1/health-services/emergency-installation-management/emergency-visits/active-episode?patientId=` (`BE-IGD-050`), sehingga **tidak memerlukan perubahan backend**.

---

## 4. Rincian per temuan

### ISS-IGD-001-01 — Pra-cek episode berjalan terlambat di Langkah Verifikasi dan isian episode ganda selalu terbuka di Langkah 2

| Field | Nilai |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` + `BUG` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source code frontend dan backend |
| **Perbaikan** | `FIX-IGD-001-01` |

**Apa yang terjadi.**
Ketika petugas loket IGD mendaftarkan pasien yang sudah memiliki kunjungan IGD berjalan (misalnya pasien yang sudah terdaftar 10 menit lalu dan sedang menunggu triage), petugas tidak melihat tanda peringatan apa pun pada Langkah 2 (*Data Emergency Visit*). Petugas terus mengisi keluhan utama, metode pembayaran, hingga tiba di Langkah 4/5 (*Verifikasi & Konfirmasi*). Ketika petugas mencentang konfirmasi dan mengklik tombol **Selesaikan Pendaftaran**, pendaftaran tiba-tiba gagal dengan pesan galat merah: *"Pendaftaran dihentikan sebelum encounter dibuat karena pasien ini masih punya kunjungan IGD yang berjalan..."*. Petugas bingung karena untuk mengatasi hal tersebut, petugas harus menekan tombol **Kembali** berulang kali ke Langkah 2 untuk mengisi kolom **Alasan Pendaftaran Episode Ganda**. Sebaliknya, untuk pasien baru atau pasien tanpa kunjungan aktif, kolom teks alasan tersebut tetap dapat diketik bebas tanpa ada indikator apakah kolom itu diperlukan atau tidak.

**Kenapa terjadi.**
Rantai penyebab teknis:
1. Di `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:1160-1181`, pemanggilan `lookupActiveEmergencyEpisode(patientId)` hanya diletakkan di dalam fungsi `handleSubmitRegistration`. Fungsi ini baru dieksekusi saat tombol *Selesaikan Pendaftaran* di `verification-step.jsx` ditekan:
   ```javascript
   // use-emergency-registration.js:1160-1181
   if (
     shouldRunDuplicateEpisodeCheck({
       patientId,
       encounterId,
       duplicateEpisodeOverrideReason: values?.duplicateEpisodeOverrideReason,
     })
   ) {
     const check = await lookupActiveEmergencyEpisode(patientId);
     if (check?.hasActiveEpisode) {
       return {
         success: false,
         stage: "duplicateCheck",
         existingVisit: check.visit,
         existingEncounter: check.encounter,
         message: !check.visit && check.encounter
           ? "Pendaftaran dihentikan sebelum encounter dibuat karena pasien ini sudah terdaftar di IGD..."
           : "Pendaftaran dihentikan sebelum encounter dibuat karena pasien ini masih punya kunjungan IGD...",
       };
     }
   }
   ```
2. Pada komponen Langkah 2 (`emergency-visit-step.jsx:116-125`), kolom teks alasan pendaftaran ganda dirender secara statis tanpa pemeriksaan apakah pasien memiliki episode berjalan:
   ```jsx
   // emergency-visit-step.jsx:116-125
   <div className={styles.emergencyVisitFieldSpacing}>
     <EmergencyTextField
       name="duplicateEpisodeOverrideReason"
       label="Alasan Pendaftaran Episode Ganda"
       placeholder="Kosongkan untuk pendaftaran normal. Isi hanya bila pasien benar-benar datang kembali sementara episode lama belum ditutup."
       multiline
       rows={3}
       maxLength={DUPLICATE_EPISODE_OVERRIDE_REASON_MAX_LENGTH}
     />
   </div>
   ```
3. Komponen `EmergencyVisitStep` tidak menerima informasi apakah pasien memiliki episode aktif, tidak memicu pemanggilan pra-cek saat langkah dibuka, dan tidak memiliki komponen `EmergencyInlineAlert` untuk memberi tahu petugas mengenai kunjungan yang sedang berjalan.

**Apakah ini menyimpang dari desain?**
Pada task `FE-IGD-034` dan `FE-IGD-038`, desain awal memang memusatkan pra-cek pada `handleSubmitRegistration` dengan tujuan teknis sempit yaitu mencegah pembuatan `patient-encounter` yatim sebelum `POST emergency-visits`. Namun, peletakan pra-cek di gerbang verifikasi akhir bertentangan dengan prinsip kegunaan antarmuka (*UI/UX principle*): isian yang dibutuhkan untuk mengatasi penolakan (`duplicateEpisodeOverrideReason`) berada di Langkah 2, sementara penolakan baru diberitahukan di Langkah 4/5. Oleh karena itu, temuan ini diklasifikasikan sebagai `DESIGN_CHANGE` atas permintaan pemilik modul dan `BUG` operasional alur kerja formulir.

**Dampak nyata.**
Petugas loket IGD (misalnya Sdri. Siti) melayani pasien darurat Tn. Budi Santoso yang datang kembali karena sesak napas bertambah parah setelah sebelumnya terdaftar 1 jam lalu. Sdri. Siti mengisi data hingga langkah verifikasi, lalu pendaftaran ditolak. Sdri. Siti harus mundur dua langkah ke belakang, mengetik alasan *"Pasien datang kembali dengan perburukan sesak napas"*, lalu maju lagi ke langkah verifikasi untuk menyimpan ulang. Di IGD di mana setiap detik berharga, alur bolak-balik ini memperlambat penanganan klinis pasien.

**Rekomendasi.**
1. Jalankan pra-cek episode aktif segera setelah pasien teridentifikasi pada Langkah 1 atau saat Langkah 2 dimuat.
2. Jika pasien memiliki episode/encounter IGD aktif:
   - Tampilkan kotak peringatan (`EmergencyInlineAlert tone="warning"`) di Langkah 2 yang menyebutkan nomor kunjungan/encounter berjalan dan statusnya.
   - Aktifkan kolom teks **Alasan Pendaftaran Episode Ganda** dan jadikan wajib diisi bila ingin melanjutkan.
3. Jika pasien tidak memiliki episode/encounter aktif (termasuk pasien baru):
   - Nonaktifkan (*disabled*) kolom teks **Alasan Pendaftaran Episode Ganda** dengan penjelasan helper text bahwa pasien tidak memiliki kunjungan aktif.
   - Kosongkan nilainya agar tidak memicu penolakan backend.

---

## 5. Tanya-jawab pelapor

> **T:** Di pendaftaran pasien, harusnya ada alert di bagian ini jika pasien kunjungan encounternya masih berjalan sehingga text pengisian area ini nyala, tapi jika pasien belum sama sekali ada kunjungan/encounter maka text area ganda ini disabled atau dihilangkan lagi, jadi alertnya bukan ketika klik selesaikan pendaftaran pada section verification?
>
> **J:** Benar sekali. Penempatan alert dan pengecekan saat ini memang terlambat karena baru berjalan saat tombol "Selesaikan Pendaftaran" diklik di Langkah Verifikasi. Rekomendasi yang akan diterapkan adalah memindahkan pemeriksaan ke awal (saat pasien dipilih/saat membuka Langkah 2):
> 1. Jika pasien memiliki kunjungan/encounter aktif, sistem langsung menampilkan kotak peringatan kuning (*warning*) di Langkah 2 dan menyalakan kolom isian "Alasan Pendaftaran Episode Ganda" (serta mewajibkan pengisiannya).
> 2. Jika pasien tidak memiliki kunjungan aktif (atau pasien baru), kotak peringatan tidak tampil dan kolom isian alasan dinonaktifkan (*disabled*) serta nilainya dikosongkan. Dengan demikian, proses di Langkah Verifikasi akan berjalan mulus tanpa penolakan mendadak.

---

## 6. Temuan tambahan

### ISS-IGD-001-T1 — Potensi penolakan HTTP 400 Bad Request backend bila kolom alasan terisi pada pasien tanpa episode aktif

| Field | Nilai |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat penelusuran aturan validasi backend `EmergencyVisitController.cs:385-391` |
| **Jenis** | `BUG` |
| **Area** | Frontend + Backend Consistency |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `EmergencyVisitController.cs:385-391` |
| **Perbaikan** | `FIX-IGD-001-02` |

**Apa yang terjadi.**
Pada kode backend `EmergencyVisitController.cs:385-391`, terdapat aturan validasi tegas:
```csharp
// EmergencyVisitController.cs:383-391
// Alasan tanpa episode aktif berarti petugas salah paham keadaannya. Menyimpannya
// membuat daftar pantau memuat penembusan yang tidak pernah terjadi.
if (episodeAktif == null && !string.IsNullOrWhiteSpace(alasanEpisodeGanda))
{
    return BadRequest(ApiResponse<object>.Fail(
        StatusCodes.Status400BadRequest,
        "Pasien ini tidak punya kunjungan IGD yang masih berjalan, sehingga alasan " +
        "pendaftaran ganda tidak perlu diisi. Kosongkan kolom itu lalu simpan lagi."));
}
```
Jika kolom teks di Langkah 2 dibiarkan aktif untuk pasien normal tanpa episode berjalan, dan petugas yang tidak paham mengetik teks di dalamnya (misalnya mengetik "tidak ada" atau "kunjungan pertama"), backend akan langsung menolak pendaftaran dengan status *400 Bad Request*.

**Rekomendasi.**
Mematikan (*disabled*) kolom teks dan mengosongkan nilainya secara otomatis saat pasien tidak memiliki episode aktif secara langsung menutup risiko galat *400 Bad Request* ini.

---

### ISS-IGD-001-T2 — Kotak peringatan episode ganda pada Langkah Verifikasi perlu diselaraskan sebagai jaring pengaman sekunder

| Field | Nilai |
| --- | --- |
| **Kenapa dicantumkan** | Terkait langsung dengan alur di `verification-step.jsx` |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di `verification-step.jsx` dan `use-emergency-registration.js` |
| **Perbaikan** | `FIX-IGD-001-03` |

**Apa yang terjadi.**
Kotak peringatan kuning dan merah pada `verification-step.jsx` saat ini dirancang untuk menangani penolakan episode aktif pada tahap akhir. Jika pra-cek sudah dipindahkan ke Langkah 2, kotak ini tetap dipertahankan sebagai jaring pengaman sekunder (*fail-safe*) untuk kondisi khusus (misalnya pendaftaran serentak oleh dua loket berbeda), namun tidak lagi menjadi jalur utama interaksi pengguna.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor:
*Tidak ada pertanyaan yang menahan perbaikan. Kebutuhan pelapor sudah jelas dan spesifik.*

### Untuk pemilik modul:

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bentuk tampilan kolom "Alasan Pendaftaran Episode Ganda" saat pasien TIDAK memiliki episode aktif | (A) Dibuat `disabled` dengan placeholder/helper text penjelas;<br>(B) Dihilangkan sama sekali (*hidden/unmounted*) dari tampilan Langkah 2 | **Pilihan A (disabled)**: Menjaga stabilitas tata letak form dan memberi edukasi visual kepada petugas loket bahwa fitur episode ganda memang ada namun tidak berlaku untuk kunjungan normal. | `FIX-IGD-001-01` |
| K-02 | Waktu eksekusi pra-cek episode berjalan | (A) Saat pasien dipilih di Langkah 1 & disimpan di state controller;<br>(B) Saat komponen Langkah 2 (*EmergencyVisitStep*) di-mount | **Pilihan A**: Lebih responsif karena saat petugas tiba di Langkah 2, data episode aktif sudah siap tanpa jeda pemuatan (*loading spinner*). | `FIX-IGD-001-01` |

---

## 8. Catatan pola

Pola masalah ini merupakan contoh klasik dari **"Validasi Server yang Terlambat Dicerminkan di Antarmuka Pengguna"**:
1. Aturan validasi bisnis (tidak boleh mendaftarkan pasien dengan episode aktif tanpa alasan) ditegakkan di backend.
2. Frontend awalnya hanya menerapkan penolakan di gerbang submit paling ujung (`handleSubmitRegistration`), bukan di langkah formulir tempat data tersebut dimasukkan (`EmergencyVisitStep`).
3. Pencegahan ke depan: Setiap kali sebuah formulir multi-langkah (*wizard*) memiliki isian yang bergantung pada kondisi pra-syarat server (*conditional requirement*), pemeriksaan pra-syarat wajib dilakukan sebelum atau tepat saat langkah isian tersebut dibuka, bukan menunggu tombol simpan akhir.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Dokumen issue dibuat dari laporan keluhan pelapor dan analisis kode frontend/backend | `diagnose-module-issue` |
