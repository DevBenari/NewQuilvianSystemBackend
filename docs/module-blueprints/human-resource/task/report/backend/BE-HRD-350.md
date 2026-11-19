# Laporan Perubahan Backend — `BE-HRD-350`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-350` |
| Judul | Ekspor `.xlsx` dan log ekspor |
| Slice | Amandemen `A52` — `EPIC HRD-114`, gelombang `MVP-68` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B87 |
| Trace | `FR-PK-018`; `HRD-DEC-206` (butir 1, 3, 4, 5); `api-contract.md` Attendance Report `GET /{reportCode}/export`; `permission-audit-matrix.md` 30.1 (`AttendanceReport : Export`, log **Ya**); `AT-HRD-PK-37`, `-38` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-349` ✅ |
| Klasifikasi | `STANDARD` — skor 5: satu repository (0), sekitar 8 berkas diperiksa (1), 2 berkas source diubah (0), pembentukan berkas biner (1), hak akses baru (1), log audit wajib (1), privasi data sakit pada berkas (1), tanpa schema (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceReportService.cs,Controllers/AttendanceReportController.cs}`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `349` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 114/114 (termasuk kontrol negatif); `AT-HRD-PK-37` dan ekspor tujuh laporan lewat API devYoga; `AT-HRD-PK-38` lewat harness |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `NEW CODE` pada service dan controller dari `BE-HRD-349` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Laporan kehadiran hanya dapat dilihat di layar | Kepala Unit dan HRD tidak dapat mengunduh laporan ke Excel (`HRD-DEC-206` butir 1) |
| Belum ada jejak ekspor | Tidak diketahui siapa mengunduh data kehadiran unit apa (butir 4) |

---

## 2. Proses bisnis

**Ekspor = layar (butir 3).** Sari, Kepala Unit ICU, memfilter Laporan Keterlambatan ICU Februari lalu menekan Ekspor.
Server membentuk laporan dengan fungsi yang sama seperti tabel layar, hanya tanpa paginasi, sehingga cakupan unit, filter,
kolom, dan jumlah barisnya identik. Berkas berisi:

| Baris | Isi |
| --- | --- |
| 1–4 | Nama laporan; unit (atau "Semua unit dalam cakupan"); rentang dan status yang disaring; "Periode sudah ditutup — data final" atau "Periode masih terbuka — data dapat berubah" |
| 6 | Kepala kolom = label `Columns` layar |
| 7 dst. | Satu baris per baris laporan; tanggal `yyyy-MM-dd`, jam dalam WIB `yyyy-MM-dd HH:mm` |

Nama berkas `laporan-<kode>_<mulai>_<selesai>.xlsx`. Bila Sari memilih IGD, permintaan ditolak `403` tanpa berkas.
Cuti sakit tetap hanya "Sakit" (butir 5) karena berkas memakai baris laporan yang sama.

**Log (butir 4).** Setiap ekspor yang berhasil menulis tepat satu log audit `AttendanceReport.Export`, contoh:
"Ekspor Laporan Ketidakhadiran (absence); unit: Unit Perawatan Intensif (ICU); rentang 2026-10-01 s.d. 2026-10-07;
status: -; periode: ditutup; 4 baris; berkas laporan-absence_2026-10-01_2026-10-07.xlsx." — dengan pelaku, waktu, dan
alamat permintaan dari sesi. Isi ditulis di teks pesan karena `LoggerService` tidak menyimpan payload. Ekspor yang ditolak
(401/403/400/404) tidak menghasilkan berkas maupun log.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `api-contract.md` Attendance Report; `permission-audit-matrix.md` 30.1 baris ekspor; `acceptance-test-matrix.md` `AT-HRD-PK-37`–`39`
- Pola ekspor: `LabCancellationReportController.cs` (aksi `Export`, berkas dari fungsi yang sama dengan daftar), `CashierShiftRevenueReportService.cs` (ClosedXML); `LoggerService.cs`
- `AttendanceReportService.cs`, `AttendanceReportController.cs`, `AttendanceReportDtos.cs` (`BE-HRD-349`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceReportService.cs` | Baru: `ExportAsync` (memanggil `BuildAsync(paging:false)`, menulis `.xlsx` dengan ClosedXML, nama unit untuk kepala berkas dan log), `XlsxContentType`, kelas `AttendanceReportExportResult` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendanceReportController.cs` | Baru: `GET /{reportCode}/export` (`AccessAction("Export")`, `AccessPermission("AttendanceReport", "Export")`) dan satu log `AuditAsync` per ekspor berhasil; konstruktor menerima `LoggerService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint ekspor sesuai kontrak |
| Database | Tanpa migration; hanya baca |
| Keamanan/Auth | Aksi baru `AttendanceReport : Export` (belum diberikan ke jabatan mana pun); cakupan unit sama dengan layar |
| Audit | Log `[AUD]` per ekspor berhasil |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/reports`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{reportCode}/export` | `AttendanceReport : Export` | Query sama dengan layar: `organizationUnitId`, `fromDate`, `toDate`, `status` (paginasi diabaikan) | Berkas `.xlsx` (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) | `200`, `400` (`VR-PK-28`), `401`, `403` (izin; `VR-PK-27`), `404` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (114 uji: 111 sebelumnya + 3 baru di `AttendanceReportTests.cs`) | 114/114 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-37`: ketujuh laporan × tiga pengguna (Kepala Unit ICU, Kepala Bidang, Admin HRD), layar berukuran halaman 2 | Jumlah baris berkas = `TotalData` layar dan = `RowCount`; kepala kolom = label layar; ekspor Sari ICU hanya berisi ICU; nama berkas, judul, baris "periode masih terbuka", jam masuk "2027-02-10 07:15" (WIB) | `PASS` | `Ekspor_sama_dengan_layar_untuk_ketujuh_laporan` |
| — `AT-HRD-PK-38`: Sari mengekspor HCU | `403`, isi berkas kosong | `PASS` | `Ekspor_lintas_unit_ditolak_dan_berkas_tanpa_data_sakit` |
| — `AT-HRD-PK-39` pada berkas | Sel berisi "Sakit"; tidak ada diagnosis, alasan, nama jenis cuti sakit, kolom lampiran atau diagnosis | `PASS` | sama |
| — Log | Lewat controller dengan logger penangkap: satu ekspor berhasil = satu log berisi nama dan kode laporan, unit, rentang, jumlah baris, dan id pelaku dari sesi; ekspor 403 tidak menambah log | `PASS` | `Satu_log_per_ekspor` |
| Kontrol negatif: ekspor memakai paging layar, dan log ekspor dihapus, backend dibangun ulang | Tepat dua uji yang menjaganya gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 114/114 lagi | `PASS` | Keluaran `dotnet test` |
| API devYoga (16 November 2026), superadmin: 7 laporan × 2 filter (ICU 1–7 Okt 2026; semua unit Nov 2026), layar berukuran halaman 2 | 14/14 berkas `200`; jumlah baris berkas = `TotalData` layar (0–6 baris); kepala kolom = label layar | `PASS` | `at-350.mjs`, `at-log.jsonl` (entri "350 …"), berkas di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig\ekspor-350` |
| — Berkas Ketidakhadiran ICU Okt | Kepala: nama laporan, "Unit: Unit Perawatan Intensif (ICU)", rentang, "Periode sudah ditutup — data final"; baris Bagas Alpa, **Raka Sakit**, Raka Alpa, Arif Alpa; tidak ada "Z99", "diagnosis", "demam", "Surat Dokter" | `PASS` | `at-log.jsonl` |
| — Log ekspor | 28 baris `[AUD]` `AttendanceReport.Export` untuk 28 ekspor berhasil (dua kali jalan); isi lengkap, pelaku `superadmin` | `PASS` | `be350.log` |
| — 93 hari / kode tak dikenal / tanpa sesi | `400` / `404` / `401`; tidak ada log untuk ketiganya | `PASS` | `at-log.jsonl`, `be350.log` |
| — `gilang.mahendra` tanpa izin `Export` | `403`, bukan berkas | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-38` lewat API dengan Kepala Unit sungguhan | Tidak dijalankan: devYoga belum punya akun Kepala Unit berizin `AttendanceReport : Export` (perlu akun dan sandi, mis. Cahyo); jalur 403 `VR-PK-27` ekspor memakai fungsi yang sama dengan layar yang sudah diuji di `BE-HRD-349` | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Jumlah baris berkas = jumlah baris layar untuk filter sama | Terpenuhi | Harness (7 × 3) dan devYoga (7 × 2) |
| Satu log per ekspor berisi pelaku, waktu, kode laporan, filter, jumlah baris | Terpenuhi | Harness dan `be350.log` |
| `AT-HRD-PK-37`, `-38` | `-37` terpenuhi lewat API; `-38` terpenuhi di harness | Bagian 5 |
| Risiko kartu: `AuditAsync` tidak menyimpan payload | Ditangani — isi di teks pesan | Bagian 2 |
| DoD umum | Terpenuhi | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | `AttendanceReport : Export` belum diberikan ke jabatan mana pun; berikan lewat Role Access bersama `Read` |
| Risiko tersisa | Berkas dibentuk di memori; untuk cakupan semua unit dengan 92 hari, ukurannya sebanding dengan jumlah baris laporan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Pengurai `.xlsx` skrip uji sempat salah membaca berkas (awalan namespace `x:` dan data descriptor zip); skrip uji diperbaiki, berkas tidak berubah |
| Status Git | 2 berkas source diubah (dari `BE-HRD-349`); laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` s.d. `349` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-351` — verifikasi MVP `A52` (proyek uji HR di `Tests/`, akun non-superadmin untuk uji 403) |
