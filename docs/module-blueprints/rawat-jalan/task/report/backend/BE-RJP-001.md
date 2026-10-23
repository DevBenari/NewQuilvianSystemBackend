# Laporan Perubahan Backend — `BE-RJP-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-001` |
| Judul | Nomor prioritas kelipatan N |
| Slice | `MVP-0`, `EPIC RP-01` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-001` |
| Trace | `FR-RP-001`, `002`; `RJ-PRI-DEC-004` (menggantikan `RJ-DOC-DEC-056`); `INV-RP-09`; `VAL-RP-21`; `02-backend-architecture.md` RP.4.1 |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 4: berkas diperiksa 9–20 (1), diubah 3 (0), logika sedang (1), kontrak memakai yang ada (1), database perilaku query (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueNumberAllocator.cs`, `Program.cs`, `appsettings.json`, dokumen `rawat-jalan` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`) |
| Tanggal | 2026-10-16 |
| Status | ✅ Selesai — runtime 6/6 + uji start gagal `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — alokator yang sudah ada |
| QBE yang berlaku | `QBE-CODE-001`, `QBE-CODE-003` (alokasi tetap di service, di bawah advisory lock), `QBE-SVC-001`, `QBE-VAL-001` |

---

## 1. Masalah yang diperbaiki

Nomor cadangan prioritas memakai daftar tetap `1, 3, 5, 10, 15`. PDF poin 2G meminta kelipatan 5. Dengan daftar tetap,
pasien prioritas keenam dan seterusnya mendapat nomor biasa yang bercampur dengan nomor Reguler.

## 2. Proses bisnis

1. Kiosk atau petugas mendaftarkan kunjungan. Klasifikasi menentukan prioritas atau Reguler (`BE-RJP-002`).
2. Di bawah kunci per tanggal dan service unit, alokator membaca nomor yang sudah pernah terbit pada cakupan itu,
   termasuk yang batal atau dihapus.
3. **Prioritas** mendapat kelipatan N terkecil yang belum terpakai. **Reguler** mendapat nomor terkecil yang belum
   terpakai dan bukan kelipatan N.
4. N diambil dari `HealthServices:Registration:QueueNumber:PriorityNumberMultiple` (bawaan 5). Nilai di bawah 2 membuat
   aplikasi gagal mulai.

Contoh dari uji hari ini: nomor 1, 2, 4, 6–9, 11–20 sudah terpakai. Pendaftaran berikutnya mendapat Reguler 3,
Prioritas 5, Reguler 21, Prioritas 10, Reguler 22.

Jalur tidak normal: nomor aturan lama yang sudah terbit hari itu (misalnya 1 sebagai prioritas) tetap dihormati, karena
alokator hanya melewati nomor terpakai. Tidak ada nomor ganda.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OutpatientQueueNumberAllocator.cs` (penuh), `Program.cs` (registrasi opsi), `appsettings.json`, pemakai
`IsReservedPriorityNumber` (`PatientEncounterController.cs:1181`, `PatientEncounterDtos.cs:539`), `rules/backend/*`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueNumberAllocator.cs` | `ReservedPriorityNumbers` dan `DefaultReservedPriorityNumbers` diganti `PriorityNumberMultiple` (bawaan 5, minimal 2). Pemilihan nomor dipindah ke `PickNumber` statis (kelipatan N untuk prioritas, bukan kelipatan N untuk Reguler). Kunci, cakupan, dan larangan pakai ulang tidak berubah |
| `Program.cs` | `AddOptions<OutpatientQueueNumberOptions>().Bind(...).Validate(... >= 2).ValidateOnStart()` |
| `appsettings.json` | `QueueNumber.ReservedPriorityNumbers` diganti `QueueNumber.PriorityNumberMultiple: 5` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. `isReservedPriorityNumber` tetap `true` untuk prioritas. Nilai nomor yang terbit berubah sesuai aturan baru |
| Database | Tidak ada perubahan schema; `has-pending-model-changes` bersih |
| Keamanan/Auth | `NOT APPLICABLE` |
| Konfigurasi | Kunci `ReservedPriorityNumbers` tidak dibaca lagi. Lingkungan lain yang mengisinya perlu diganti `PriorityNumberMultiple` |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin`, `/`, `/kiosk` | Daftar kunjungan. Nomor antrean mengikuti aturan kelipatan N | Tidak berubah |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -p:UseSharedCompilation=false -o <scratchpad>/be-build` | Build succeeded, 0 error, 286 warning (lama) | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Keluaran perintah |
| QBE Strict (7 berkas `MVP-0`) | VIOLATION 0, Final result PASS | `PASS` | Keluaran QBE |
| `R-RP-01` REG-A / PRI-A / REG-B / PRI-B / REG-C | 3 / 5 / 21 / 10 / 22, masing-masing sama dengan nomor yang diharapkan dari isi DB | `PASS` | `rt_mvp0.py` |
| `R-RP-02` — hari yang sudah memuat nomor aturan lama (1) | Nomor baru tidak memakai ulang nomor terpakai | `PASS` | Nomor lama `[1]`, baru `[3, 5, 21, 10, 22]` |
| `UAT-RP-02` — `PriorityNumberMultiple=1` lewat variabel lingkungan | Aplikasi berhenti saat mulai: `OptionsValidationException: HealthServices:Registration:QueueNumber:PriorityNumberMultiple wajib bernilai minimal 2.` | `PASS` | `be7186-invalid.log` |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`, tidak ada project test.

**Keadaan DB sesudah run (`QuilvianNewDevSukma`, sufiks `1C75`):** pasien `RJP0 UJI * 1C75` beserta kunjungan dan
antrean hari ini di Poli `0614bbe8…`; membership Gold uji `UJI-RJP0-1C75-*`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-01` | Terpenuhi | Lima pendaftaran berurutan |
| `R-RP-02` | Terpenuhi | Nomor lama dihormati |
| `UAT-RP-01` (Reguler, Reguler, prioritas → 1, 2, 5 pada poli kosong) | Terpenuhi secara aturan | Diuji pada hari yang sudah berisi nomor: aturan yang sama menghasilkan nomor yang diharapkan di setiap langkah |
| `UAT-RP-02` | Terpenuhi | Uji start gagal |
| Laporan task | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | Rilis di luar jam layanan (`GATE-RP-02`). Lingkungan yang masih mengisi `ReservedPriorityNumbers` perlu diberi tahu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat ringkasan sesi |
| Langkah berikutnya | `BE-RJP-003`, `BE-RJP-004` |
