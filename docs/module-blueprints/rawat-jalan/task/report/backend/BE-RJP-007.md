# Laporan Perubahan Backend — `BE-RJP-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-007` |
| Judul | Perintah petugas |
| Slice | `MVP-1`, `EPIC RP-05` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-007` |
| Trace | `FR-RP-033` s.d. `035`, `037`, `FR-RP-052`; `RJ-PRI-DEC-012`, `019`, `022`, `023`, `028`, `029`; `VAL-RP-10` s.d. `14`; `INV-RP-06`, `07` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md`; `state-transition-matrix.md` RP-B, RP-C; `permission-audit-matrix.md` RP-1 |
| Dependency | `BE-RJP-006` ✅ |
| Klasifikasi | `HIGH` — skor 8: diperiksa 9–20 (1), diubah 2 (0), logika tinggi (2: matriks status, konkurensi), kontrak baru (2), keamanan berkaitan (1), workflow (2) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `OutpatientPriorityReservationController.cs`, `OutpatientPriorityReservationService.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-19 |
| Status | ✅ Selesai — runtime 18/18 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `NEW CODE` |
| Arketipe endpoint | Transaksi ber-lifecycle: perpindahan status lewat `POST /{id}/<aksi>`, suntingan atribut lewat `PUT /{id}/screening`, pembatalan lewat `cancel` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001` |
| Role access | Tiga butir baru: `Screen` (`AccessTypes.Update`), `Complete` (`AccessTypes.Update`), `Cancel` (`AccessTypes.Update`). Kirim ulang memakai `Create`; tandai tiba dan kembalikan memakai `Complete` (`permission-audit-matrix.md` RP-1). Argumen `[AccessPermission]` sama persis dengan `[AccessAction]`. Tidak ada pemeriksaan nama peran |

---

## 1. Masalah yang diselesaikan

Sesudah reservasi dibuat (`BE-RJP-006`), petugas belum bisa menindaklanjutinya: menandai pasien tiba, mengoreksi tanda
tiba yang salah, mencatat skrining awal, mengirim ulang link, atau membatalkan reservasi.

## 2. Proses bisnis

| Tindakan | Siapa | Dari → ke | Syarat |
| --- | --- | --- | --- |
| Tandai tiba | Admisi (`Complete`) | Menunggu Kedatangan → Sudah Tiba | Hari ini = tanggal kunjungan |
| Kembalikan ke Belum Tiba | Admisi (`Complete`) | Sudah Tiba → Menunggu Kedatangan | Alasan 10–500 karakter. Link yang sama aktif lagi |
| Simpan skrining | Petugas skrining (`Screen`) | Sudah Tiba → Sudah Tiba | Batuk, demam, risiko jatuh diisi semua. Boleh diulang |
| Kirim ulang link | Admisi (`Create`) | Menunggu Kedatangan → Menunggu Kedatangan | Kode baru terbit; kode lama mati |
| Batalkan | Admisi (`Cancel`) | Menunggu Kedatangan / Sudah Tiba → Dibatalkan | Alasan 10–500 karakter |

Setiap tindakan wajib membawa `expectedRowVersion` dari data terakhir yang dilihat petugas. Bila petugas lain sudah lebih
dulu mengubah reservasi, tindakan ditolak `409` dan petugas memuat ulang.

Contoh dari uji: dua petugas menekan Batalkan pada reservasi yang sama hampir bersamaan. Satu berhasil, yang lain
mendapat "Reservasi baru saja diubah petugas lain. Muat ulang lalu coba lagi." Riwayat `CANCELLED` hanya satu.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientPriorityReservationController.cs` | Diperbarui | `POST /{id}/arrival-code`, `/{id}/arrival`, `/{id}/arrival-reversal`, `PUT /{id}/screening`, `POST /{id}/cancel`; penyusun respons detail bersama |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `MarkArrivalAsync`, `RevertArrivalAsync`, `SaveScreeningAsync`, `ReissueArrivalCodeAsync`, `CancelAsync`, dan kerangka `ExecuteStaffCommandAsync` (cek versi → status → hari kunjungan → ubah → satu riwayat dalam satu transaksi). `NewHistory` diringkas memakai `HistoryEntry` |

### 3.2 Aturan yang diterapkan

| Aturan | Penerapan |
| --- | --- |
| `VAL-RP-10` | Tandai tiba di luar hari kunjungan: `409` "Kedatangan hanya bisa ditandai pada hari kunjungan, {tanggal}." |
| `VAL-RP-11` | Status tidak sesuai matriks RP-C: `409` "Reservasi sedang berstatus {status}, sehingga tindakan ini tidak bisa dilakukan." |
| `VAL-RP-12` | Alasan kosong, < 10, atau > 500 karakter: `400`. Diperiksa sebelum data dibaca |
| `VAL-RP-13` | Salah satu isian skrining kosong: `400` |
| `VAL-RP-14` | `expectedRowVersion` berbeda: `409`. Lapis kedua: `RowVersion` adalah concurrency token, sehingga penyimpanan bersamaan menghasilkan `DbUpdateConcurrencyException` yang dijawab `409` |
| `INV-RP-06` | Tepat satu baris riwayat per tindakan, dalam transaksi yang sama. Riwayat skrining tidak memuat isi checklist |
| `INV-RP-07` | Status akhir menolak semua tindakan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru, aditif |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Keamanan | Log hanya `EntityId`, nomor, status, controller, aksi. Alasan, checklist, kode, dan hash tidak dicatat |

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/registration-management/outpatient-priority-reservations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/arrival-code` | Kirim ulang link | `OutpatientPriorityReservation : Create` | `{ "expectedRowVersion": "…" }` | `ApiResponse<PriorityReservationIssuedResponse>` | Tersedia |
| `POST` | `/{id}/arrival` | Tandai tiba | `OutpatientPriorityReservation : Complete` | `{ "expectedRowVersion": "…" }` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `POST` | `/{id}/arrival-reversal` | Kembalikan ke Belum Tiba | `OutpatientPriorityReservation : Complete` | `{ "expectedRowVersion": "…", "reason": "Pasien tidak ditemukan di ruang tunggu" }` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `PUT` | `/{id}/screening` | Simpan skrining | `OutpatientPriorityReservation : Screen` | `{ "expectedRowVersion": "…", "cough": true, "fever": false, "fallRisk": true }` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `POST` | `/{id}/cancel` | Batalkan | `OutpatientPriorityReservation : Cancel` | `{ "expectedRowVersion": "…", "reason": "Pasien minta ganti tanggal kunjungan" }` | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |

Kode status: `200`; `400` alasan/skrining/versi tidak sah; `403` tanpa hak; `404` reservasi tidak ada; `409` status, hari
kunjungan, atau versi tidak cocok.

Delta: kontrak tidak menyebut `400` bila `expectedRowVersion` kosong. Service menolaknya dengan "Versi data reservasi wajib
dikirim." karena tanpa versi pemeriksaan `VAL-RP-14` tidak bermakna.

## 5. Verifikasi

Data samaran: enam pasien `RJPR UJI P1`..`P6`, membership Gold disisipkan SQL data uji. Untuk `R-RP-24`, membership P5
diubah SQL menjadi Kedaluwarsa. Untuk `R-RP-19`, satu akun uji dibuat lewat `POST /external-users` pada pasangan
departemen+jabatan yang tidak dipakai siapa pun (Logistik dan Gudang / Staff Pengadaan), diberi hanya `Read` dan `Screen`
lewat `POST /role-access/policies`, lalu login dengan koordinat geofence rumah sakit. Sesudah uji, kebijakan pasangan itu
dikosongkan lewat API dan akun dinonaktifkan.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Build, `has-pending-model-changes`, QBE Strict (2 berkas) | 0 error; bersih; VIOLATION 0 | `PASS` |
| `R-RP-15` a | Tandai tiba: Sudah Tiba, `arrivalSource = Staff`, riwayat `ARRIVED` sumber Staff | `PASS` |
| `R-RP-15` b | Kembalikan: Menunggu Kedatangan, `arrivedAt` kosong, riwayat `ARRIVAL_REVERTED` beralasan, hash link tidak berubah | `PASS` |
| `VAL-RP-12` | Alasan "pendek": `400` | `PASS` |
| `VAL-RP-14` | `rowVersion` lama: `409` | `PASS` |
| `VAL-RP-11` a, b | Skrining saat Menunggu Kedatangan `409`; kirim ulang saat Sudah Tiba `409` | `PASS` |
| `VAL-RP-13` | Skrining tanpa risiko jatuh: `400` | `PASS` |
| Skrining berulang | Nilai terakhir berlaku, `screenedByName` terisi, dua riwayat `SCREENED` tanpa isi checklist | `PASS` |
| `VAL-RP-10` | Tandai tiba reservasi besok: `409` menyebut 20 Oktober 2026 | `PASS` |
| `R-RP-23` (kirim ulang) | Kode baru 43 karakter, hash diganti dan sama dengan SHA-256 kode baru, riwayat `CODE_REISSUED`, teks WA memuat link baru | `PASS` |
| `R-RP-26` | Dua pembatalan bersamaan: `[200, 409]`, satu riwayat `CANCELLED` | `PASS` |
| `INV-RP-07` | Batalkan reservasi Dibatalkan: `409` | `PASS` |
| Detail Dibatalkan | Alasan dan `closedAt` terisi; semua `can*` `false` | `PASS` |
| `R-RP-19` a | Batal tanpa alasan: `400` | `PASS` |
| `R-RP-24` | Batal "ganti tanggal" berhasil; reservasi baru sesudah membership Kedaluwarsa `409` `VAL-RP-02` | `PASS` |
| `R-RP-19` b | Akun `Read` + `Screen`: buat `403`, tandai tiba `403`, batal `403`; daftar `200`, skrining `200` | `PASS` |
| Pembersihan | Kebijakan pasangan uji 0, akun uji nonaktif | `PASS` |
| Log | Kode, hash, dan ketiga alasan uji tidak ada di log berkas maupun konsol | `PASS` |

Skrip: `rt_rjp007.py` dan `rjp_users.py` di scratchpad sesi. Total 18/18 `PASS`.

Belum tercakup di task ini: "link lama 404, link baru `READY`" (`R-RP-23`) dan "link aktif lagi" sesudah dikembalikan
(`R-RP-15`) diuji lewat endpoint publik di `BE-RJP-012`. Bagian "lengkapi = 403" dari `R-RP-19` diuji di `BE-RJP-008`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-15` | Terpenuhi untuk sisi petugas; konfirmasi link sesudah dikembalikan menyusul `BE-RJP-012` | a, b |
| `R-RP-19` | Terpenuhi untuk buat, tandai tiba, batal; lengkapi menyusul `BE-RJP-008` | a, b |
| `R-RP-23` (kirim ulang) | Terpenuhi untuk hash; link lama/baru menyusul `BE-RJP-012` | Uji kirim ulang |
| `R-RP-24` | Terpenuhi | Uji |
| `R-RP-26` | Terpenuhi | Uji paralel |

Bagian yang menyusul memakai endpoint task lain dan tercatat pada kriteria task itu, bukan kekurangan task ini.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | — |
| Risiko tersisa | Kebijakan akses peran nyata (Admisi, Petugas skrining) belum diatur; admin perlu mencentang butir baru di layar Akses Role sebelum UAT |
| Perubahan sampingan | `NONE`. Akun uji `TEST-RJP Uji screen` tersisa dalam keadaan nonaktif |
| Interupsi | Sesi terhenti karena limit sesudah refactor service. Dilanjutkan dari status Git dan isi berkas; tidak ada suntingan ganda |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-008` (pelengkapan dan jalur lain) |
