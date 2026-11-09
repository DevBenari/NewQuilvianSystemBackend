# Laporan Perubahan Backend — `BE-RWI-256`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-256` |
| Judul | Rencana kontrol terstruktur pada resume medis |
| Slice | Repair 043 episode rawat inap; Slice 2 pada [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-043.md](../../../roadmap/roadmap-repair-043.md), kartu `BE-RWI-256` |
| Trace | `FIX-EPS-043-02` menutup `ISS-EPS-043-02` (tahap 1); [plan repair 043](../../../docs/plan-repair/plan-repair-043-patah-alur-admisi-discharge-dan-koordinasi-bedah.md) bagian 3.B dan kartu `FIX-EPS-043-02` |
| Contract version | Delta additive di luar `api-contract.md`; bentuknya ditetapkan plan repair 043 yang disetujui pemilik 2026-11-02. Dicatat pada bagian 3.3 |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — model, DTO, validasi, konfigurasi EF, dan satu migration |
| Task mode | `BACKEND` (ditambah wewenang dokumen blueprint) |
| Target tulis | `NewQuilvianSystemBackend` branch `MHamzah` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Source masuk pada `da737fb9` (2026-11-02); diverifikasi pada HEAD `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai di sisi server: kelima acceptance criteria sisi server terpetakan ke source, `dotnet build` lulus, migration sudah diterapkan di `QuilvianNewDevHamzah` |

---

## 1. Masalah yang diperbaiki

Bagian 7 "Rencana Kontrol" pada resume medis hanya berupa satu kotak teks bebas. Dokter menulis
"kontrol 1 minggu ke poli dalam", tetapi sistem tidak tahu tanggal, poli, dan dokternya.
Akibatnya jadwal kontrol tidak bisa dibaca mesin, tidak bisa dicetak rapi, dan tidak bisa
dipakai untuk janji temu otomatis di tahap berikutnya (`FIX-EPS-043-06`).

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Pasien pulang membawa rencana kontrol yang jelas: tanggal, poliklinik, dan dokter |
| Pelaku | DPJP (dokter penanggung jawab pelayanan) menyusun dan menandatangani resume; supervisor membuka sesi koreksi bila resume yang sudah ditandatangani perlu diubah |
| Pemicu | DPJP menyimpan atau memfinalisasi resume pulang |
| Prasyarat | Episode berstatus `DischargePending`, artinya DPJP sudah menyatakan pasien boleh pulang, sehingga tanggal keputusan pulang selalu ada |

Langkah utama:

1. DPJP mengisi tanggal kontrol, poliklinik, dan dokter (opsional), serta catatan tindak lanjut.
2. Server memvalidasi rencana kontrol **sebelum** apa pun disimpan (`InpDischargeService.cs`,
   method `ValidateFollowUpPlanAsync`).
3. Bila lolos, ketiga nilai disimpan pada `InpDischargeSummary`; catatan lama `FollowUpInstruction`
   tetap disimpan apa adanya.
4. Bila resume yang sudah ditandatangani diubah lewat sesi koreksi, nilai sebelum perubahan
   disalin ke `InpDischargeSummaryRevision`.
5. Detail resume mengembalikan ketiga nilai beserta nama poliklinik dan nama dokter.

Aturan validasi beserta contoh (keputusan pulang 2 November 2026 pukul 06.30 WIB):

| Isian | Hasil | Pesan |
| --- | --- | --- |
| Semua kosong | Diterima, tanpa rencana kontrol | — |
| 9 November, Poli Penyakit Dalam, dr. F | Diterima | — |
| 9 November, tanpa poli | Ditolak (kode 400, isian tidak lengkap) | "Poliklinik kontrol wajib dipilih bila tanggal kontrol diisi." |
| Poli dipilih, tanpa tanggal | Ditolak | "Tanggal kontrol wajib diisi bila poliklinik atau dokter kontrol dipilih." |
| 1 November, Poli Penyakit Dalam | Ditolak | "Tanggal kontrol tidak boleh sebelum tanggal keputusan pulang." |
| 2 November (hari yang sama dengan keputusan pulang) | Diterima | — |
| Poli yang bukan unit rawat jalan aktif | Ditolak | "Poliklinik kontrol tidak ditemukan atau bukan unit rawat jalan yang aktif." |
| Dokter tanpa jadwal praktik aktif di poli itu | Ditolak | "Dokter kontrol tidak punya jadwal praktik aktif di poliklinik yang dipilih." |

Tanggal keputusan pulang dibandingkan menurut jam dinding rumah sakit (zona `Asia/Jakarta`).
Contoh: keputusan pulang 2 November pukul 06.30 WIB tersimpan sebagai 1 November 23.30 UTC.
Server tetap menganggap tanggalnya 2 November, sehingga kontrol 2 November diterima.

Jalur tidak normal: resume lama yang hanya berisi teks tetap terbaca, karena ketiga kolom baru
boleh kosong; resume yang sudah ditandatangani hanya dapat diubah supervisor lewat sesi koreksi
(aturan lama, tidak berubah).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Plan repair 043 bagian 3.B dan kartu `FIX-EPS-043-02`; roadmap repair 043; governance backend
(`AGENTS.md`, `TASK_RULES`, `TEST_POLICY`, `REVIEW_RULES`, kontrak engineering, registry prefix);
source pada tabel 3.2; migration `20261102050639_AddStructuredFollowUpToInpDischargeSummary`.

### 3.2 Berkas yang berubah

Source sudah masuk commit `da737fb9`. Verifikasi 9 November 2026 tidak menemukan kekurangan sisi
server, sehingga **tidak ada perubahan source baru** pada sesi ini.

| Berkas (isi commit `da737fb9`) | Perubahan |
| --- | --- |
| `InPatientManagement/Models/InpDischargeSummary.cs` | Kolom `FollowUpDate` (`DateOnly?`), `FollowUpServiceUnitId`, `FollowUpDoctorId` beserta navigasi ke `MstServiceUnit` dan `MstDoctor` |
| `InPatientManagement/Models/InpDischargeSummaryRevision.cs` | Salinan tiga kolom yang sama, tanpa relasi |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpDischargeSummaryConfiguration.cs` | Index `FollowUpDate`; FK ke poli dan dokter dengan `DeleteBehavior.Restrict` |
| `InPatientManagement/DTOs/InpatientDischargeDtos.cs` | Request menerima tiga isian; respons detail mengembalikan nama poli dan nama dokter; respons revisi membawa salinannya |
| `InPatientManagement/Services/InpDischargeService.cs` | `ValidateFollowUpPlanAsync`, penyimpanan tiga nilai, penyalinan ke revisi, dan proyeksi detail |
| `Migrations/20261102050639_AddStructuredFollowUpToInpDischargeSummary.cs` | Enam kolom nullable, tiga index, dua FK; `Down()` membalik semuanya |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Body simpan resume menerima `followUpDate`, `followUpServiceUnitId`, `followUpDoctorId` (opsional). Respons detail menambah `followUpServiceUnitName` dan `followUpDoctorName`. Semua additive; pemanggil lama tidak rusak. `api-contract.md` dan `data-dictionary.md` sub-modul belum memuatnya (Slice 6 MVP) |
| Database | Migration `20261102050639_AddStructuredFollowUpToInpDischargeSummary` **sudah tercatat** di `__EFMigrationsHistory` DB `QuilvianNewDevHamzah`; enam kolom terbukti ada lewat `information_schema` (9 November 2026, baca-saja). Database lain belum diperiksa |
| Keamanan/Auth | Endpoint resume yang ada tidak berubah hak aksesnya. Aturan "hanya DPJP aktif yang menyusun" dan "hanya supervisor yang mengubah resume bertanda tangan" tetap berlaku |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah endpoint. Endpoint simpan dan baca resume yang sudah ada mendapat
isian tambahan seperti pada bagian 3.3.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` (`ACTIVE`), `TOUCHED LEGACY`. QBE yang berlaku: QBE-ENT-002 (nullability mengikuti domain: data lama boleh kosong), QBE-CFG-001/002 (index dan FK pada configuration), QBE-VAL-001, QBE-DTO-001, QBE-TXN-001 (penyimpanan resume dan revisi dalam satu transaksi yang sudah ada) | `PASS` | Review source |
| `dotnet build QuilvianSystemBackend.csproj -o <scratch>` (9 November 2026) | `0 Error(s)`, `12 Warning(s)` (peringatan paket NuGet lama) | `PASS` | Keluaran build |
| Kolom di DB dev | `InpDischargeSummary` dan `InpDischargeSummaryRevision` masing-masing punya `FollowUpDate` (date), `FollowUpServiceUnitId` (uuid), `FollowUpDoctorId` (uuid) | `PASS` | Query `information_schema.columns` |
| AC 1 sisi server — tiga nilai tersimpan dan tampil di detail | Disimpan pada upsert; proyeksi detail membawa nama poli dan nama dokter | `PASS` (penelusuran source) | `InpDischargeService.cs`, proyeksi detail |
| AC 2 — tanggal kontrol sebelum keputusan pulang ditolak | Pesan persis "Tanggal kontrol tidak boleh sebelum tanggal keputusan pulang." | `PASS` (penelusuran source) | `ValidateFollowUpPlanAsync` |
| AC 3 sisi server — poli kosong tidak lolos | Tanggal tanpa poli ditolak. Bila "Kontrol ke poliklinik" dipilih tetapi tanggal dan poli sama-sama kosong, server tidak bisa mengetahuinya (pilihan itu tidak disimpan), jadi layar yang wajib menahannya — bagian `FE-RWI-286` | `PASS` (penelusuran source) | `ValidateFollowUpPlanAsync` |
| AC 4 sisi server — resume lama tetap terbuka | Ketiga kolom nullable; `FollowUpInstruction` tidak berubah | `PASS` (penelusuran source) | Model dan migration |
| AC 5 — revisi menyimpan nilai sebelum perubahan | Tiga nilai disalin ke `InpDischargeSummaryRevision` | `PASS` (penelusuran source) | Blok pembuatan revisi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — satu resume baru dan satu resume lama, dijalankan bersama `FE-RWI-286`
pada sesi uji langsung; hasilnya ditambahkan ke laporan ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1–5 `FIX-EPS-043-02` dari sisi server | Terpenuhi | Bagian 5 |
| DoD — `dotnet build` pada project aplikasi | Terpenuhi | `0 Error(s)` |
| DoD — file migration disiapkan; penerapan oleh pemilik | Terpenuhi; migration sudah diterapkan di DB dev Hamzah | Bagian 3.3 |
| DoD — laporan `task/report/backend/BE-RWI-256.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Working tree backend memuat perubahan modul HR milik pekerjaan lain; tidak disentuh |
| Masalah yang diketahui | Pilihan "Tidak perlu kontrol" tidak disimpan sebagai nilai tersendiri. Resume baru tanpa rencana kontrol dan resume lama sama-sama tersimpan dengan tiga kolom kosong. Ini sesuai langkah 2 plan repair (hanya tiga kolom) dan sesuai AC 4, tetapi layar tidak bisa membedakan "dokter memilih tidak perlu kontrol" dari "belum diisi" |
| Risiko tersisa | Migration belum diperiksa pada database selain `QuilvianNewDevHamzah` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada perubahan source; hanya dokumen blueprint rawat inap |
| Langkah berikutnya | `FE-RWI-286` beserta uji manual resume baru dan resume lama |
