# Laporan Perubahan Backend — `BE-OPT-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-013` |
| Judul | Pembaca Rekam Medis dan kunci RM per versi |
| Slice | `MVP-1` — `EPIC OPT-09` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-013` |
| Trace | `FR-OPT-090`; `OPT-DEC-024`, `OPT-DEC-050`; `INT-OPT-09`; `OPT-AC-014`; `UAT-20` |
| Contract version | `OPT-INT-v1` `INT-OPT-09` — `approved`; persetujuan Rekam Medis `OPT-APR-007` ✅ (Yoga Aji Pratama, `RM-DEC-027`) |
| Dependency | `BE-OPT-011` ✅, `BE-OPT-012` ✅ |
| Klasifikasi | `MEDIUM` — satu pembaca baru, pendaftaran keutuhan di dua service Optik, tiga suntingan kecil di Rekam Medis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/Services/`; `MedicalRecordManagement/Enums/ClinicalDocumentKind.cs`, `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` (`JenisYangDitegakkan`), `MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` (`NamaJenis`); `Program.cs`; `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement`; menyentuh `MedicalRecordManagement` atas persetujuan pemiliknya (`OPT-APR-007`) |
| Registry | `Opt` / `ACTIVE`; Rekam Medis `Mrc` / `ACTIVE` |
| Keberlakuan | `NEW CODE` (pembaca, pendaftaran keutuhan); `TOUCHED LEGACY` untuk tiga berkas Rekam Medis dan `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001`, `QBE-AUD-001`, `QBE-OPT-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan (enum disimpan sebagai `int`, tabel keutuhan sudah ada) · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Hasil pemeriksaan dan resep Optik belum terlihat di berkas Rekam Medis. Dokumen itu juga belum tunduk pada kunci
RM, sehingga pemeriksaan final pun masih bisa diubah tanpa jejak di Rekam Medis.

---

## 2. Proses bisnis

1. **Rina memulai pemeriksaan Ibu Sari.** Revisi 1 terdaftar di Rekam Medis sebagai **draf**.
2. **Rina menyimpan final.** Revisi 1 menjadi **bertanda tangan atas nama Rina dan terkunci**.
3. **Rina merevisi visus OS 6/15 → 6/12** dengan alasan "salah ketik". Revisi 2 terdaftar sebagai **dokumen RM baru** yang langsung bertanda tangan. Dokumen revisi 1 tetap terkunci dan tidak berubah.
4. **Setiap versi resep** — baru dibuat, disalin dari resep luar, atau direvisi — terdaftar sebagai dokumen RM bertanda tangan.
5. **dr. Bima membuka berkas Ibu Sari (`UAT-20`).** Yang tampil:
   - "Pemeriksaan Refraksi — Visus OD 6/12, OS 6/12; Koreksi OD −1,50, OS −1,75 (Rina)", bertanda "pernah dikoreksi";
   - "Resep Kacamata — OD −1,50; OS −1,75; PD 62 (Rina)".

   Rincian menunjukkan versi 2 dari 2 beserta riwayat "v1 …; v2 … — salah ketik". Versi 1 tetap bisa dibuka, dengan keterangan "Versi lama. Versi terakhir: 2.". Tidak ada harga, diskon, atau pembayaran (`OPT-DEC-024`).
6. **Resep luar** tampil "Resep Kacamata (resep luar)" dengan "Resep luar — Siti, Optik Sinar", teks asli sebelum transposisi, dan nama penyalinnya.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Kunjungan ditutup saat pemeriksaan masih draf, sehingga Rekam Medis mengunci draf itu tanpa tanda tangan | Isi dan final ditolak `409 OPT-RM-LOCKED` "Pemeriksaan ini terkunci di rekam medis karena kunjungan sudah ditutup. Batalkan draf ini dan mulai pemeriksaan pada kunjungan baru."; batal tetap boleh dan tanda kunci RM tidak terhapus |
| Draf dibatalkan | Dokumen RM-nya ditandai batal |
| Pendaftaran ke Rekam Medis ditolak | `409 OPT-RM-REGISTRATION`; pemeriksaan atau resep tidak tersimpan |
| Pemeriksaan draf atau batal | Tidak tampil di berkas |
| Id dokumen milik pasien lain | Rincian `null` (tidak bocor) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`IMedicalRecordDocumentSource`, `MedicalRecordSourceDtos`, `McuMedicalRecordDocumentSource` (pola), `ClinicalDocumentIntegrityService` (`RegisterAsync`, `RegisterSignedAsync`, `EnsureMutableAsync`, `EnsureDraftStillOpenAsync`, `MarkCancelledAsync`, `LockOpenDocumentsForEncounterAsync`), `MedicalRecordTimelineService.NamaJenis` dan `SeluruhJenis`, `LabResultValidationService` (pola penanganan gagal daftar), registrasi pembaca di `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptMedicalRecordDocumentSource.cs` | **Baru.** Pembaca dua jenis dokumen: satu baris per pemeriksaan final dan per resep berisi versi terakhir; rincian menerima Id versi mana pun |
| `Areas/HealthServices/OpticalManagement/Services/OptRefractionExamService.cs` | **Diperbarui.** Daftar draf saat mulai; tolak isi/final bila draf dikunci RM; tanda tangan saat final dan per revisi; tandai batal. Konstruktor menerima `ClinicalDocumentIntegrityService` dan `IHttpContextAccessor` (perangkat dan IP tanda tangan) |
| `Areas/HealthServices/OpticalManagement/Services/OptPrescriptionService.cs` | **Diperbarui.** Tanda tangan RM untuk setiap versi resep, pada `SaveChanges` yang sama |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** `OPT-RM-LOCKED`, `OPT-RM-REGISTRATION` |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | **Diperbarui.** `OpticalRefraction = 45`, `OpticalPrescription = 46` |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | **Diperbarui.** Dua jenis itu masuk `JenisYangDitegakkan` (persetujuan dicatat di komentar) |
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | **Diperbarui.** Label "Pemeriksaan Refraksi" dan "Resep Optik" pada `NamaJenis` |
| `Program.cs` | **Diperbarui.** `AddScoped<IMedicalRecordDocumentSource, OptMedicalRecordDocumentSource>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be013MedicalRecordTests.cs` | **Baru** (diabaikan Git). 9 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/Be011RefractionExamTests.cs`, `Be012PrescriptionTests.cs` | **Diperbarui.** Penyesuaian konstruktor |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint Optik baru. Berkas Rekam Medis kini memuat dua jenis dokumen Optik |
| Database | Tanpa migration. Menulis baris `MrcClinicalDocumentIntegrity` (tabel Rekam Medis yang sudah ada) per versi |
| Keamanan/Auth | Pembaca tidak memeriksa izin Optik; izin berkas milik Rekam Medis (`RM-DEC-040`). Rincian menolak Id dokumen pasien lain. Pembaca tidak menyentuh tabel pesanan, sehingga data harga dan bayar tidak mungkin terbaca |

**Delta terhadap rencana:** label `NamaJenis` di `MedicalRecordTimelineService` tidak tercantum di cakupan roadmap. Tanpa label itu, layar berkas menampilkan nama enum mentah ("OpticalRefraction"). Perubahannya dua baris dan masih di bawah persetujuan `OPT-APR-007`.

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Dokumen Optik dibaca lewat endpoint berkas Rekam Medis yang sudah ada, dengan jenis
`OpticalRefraction` (45) dan `OpticalPrescription` (46).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 123 lulus, 0 gagal (9 baru) | `PASS` | Keluaran `dotnet test` |
| `dotnet test Tests/QuilvianSystemBackend.MedicalRecordTests -p:RunAnalyzers=False` (regresi Rekam Medis) | 225 lulus, 0 gagal | `PASS` | Keluaran `dotnet test` |
| Angka 45/46, ditegakkan, berlabel | Lulus | `PASS` | Uji |
| `OPT-DEC-050`: revisi 1 draf → bertanda tangan saat final; revisi 2 dokumen baru bertanda tangan; dokumen revisi 1 ditolak `EnsureMutableAsync` | Lulus | `PASS` | Uji |
| Draf batal → keutuhan `Cancelled` | Lulus | `PASS` | Uji |
| Draf dikunci RM: isi/final `409 OPT-RM-LOCKED`; batal boleh; tanda kunci tetap | Lulus | `PASS` | Uji |
| Setiap versi resep bertanda tangan | Lulus | `PASS` | Uji |
| `UAT-20`/`OPT-AC-014`: versi terakhir tampil, ringkasan dan rincian, tanpa label harga/bayar atau "Rp" | Lulus | `PASS` | Uji |
| Versi lama terbaca berketerangan; draf tidak terbaca; pasien lain `null`; resep luar bertanda; `Limit = 0` hanya menghitung | Lulus | `PASS` | Uji |
| QBE checker atas 8 berkas + kontrol multi-baris | Kontrol terkena 5 temuan; 8 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan:**
- Membuka berkas Rekam Medis di backend yang menyala. Tabel Optik belum ada di devYoga, dan wewenang database tidak diberikan.
- `UAT-21` (izin berkas Rekam Medis). Penjaganya milik Rekam Medis dan baru bisa dibuktikan lewat endpoint berkas yang menyala bersama pemilik Rekam Medis.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-014` | Terpenuhi | Uji pembaca |
| (2) `UAT-20` | Terpenuhi di harness | Uji pembaca |
| (3) Tidak ada kolom harga/bayar | Terpenuhi | Pembaca tidak menyentuh tabel pesanan; uji memeriksa label dan nilai |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bila kunjungan ditutup sementara pemeriksaan masih draf, draf itu terkunci selamanya di Rekam Medis. Petugas harus membatalkannya dan memeriksa ulang di kunjungan baru. Ini akibat langsung kunci RM yang disetujui (`OPT-DEC-050`) |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Angka enum 45/46 mengikuti `02-backend-architecture.md` (angka bebas berikutnya); bila Rekam Medis menetapkan angka lain, harus diubah **sebelum** ada data. Layar berkas Rekam Medis (FE) belum diperiksa untuk dua jenis baru ini |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`, `M Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs`, `M …/ClinicalDocumentIntegrityService.cs`, `M …/MedicalRecordTimelineService.cs`; `?? Areas/HealthServices/OpticalManagement/Services/OptMedicalRecordDocumentSource.cs` |
| Langkah berikutnya | `MVP-1` tinggal `BE-OPT-003` (rujukan bertaut di Registrasi). Sesudah itu `MVP-2`: `BE-OPT-030` (perluasan Billing) dan `BE-OPT-033` boleh lebih awal, lalu `031`, `032`, `034` |
