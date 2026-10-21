# Laporan Perubahan Backend — `BE-RJP-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-003` |
| Judul | Display API tanpa antrean prioritas |
| Slice | `MVP-0`, `EPIC RP-02` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-003` |
| Trace | `FR-RP-010`; `RJ-PRI-DEC-005`; `INV-RP-08`; `CAP-RP-02`; `api-contract.md` Queue Display Runtime (diperbarui) |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` |
| Dependency | — |
| Klasifikasi | `LIGHT` — skor 3: satu berkas diubah, logika sederhana, kontrak memakai yang ada (1), query (1), keamanan berkaitan (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `Areas/HealthServices/RegistrationManagement/Controllers/QueueDisplayRuntimeController.cs`, dokumen `rawat-jalan` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`) |
| Tanggal | 2026-10-16 |
| Status | ✅ Selesai — runtime 3/3 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` |
| Keberlakuan | `TOUCHED LEGACY` — controller lama yang memakai `ApplicationDbContext` langsung (utang teknis, tidak diubah polanya) |
| QBE yang berlaku | `QBE-API-001` (bentuk respons tetap). `QBE-SVC-001` tidak dipicu karena tidak ada logika baru di luar satu syarat filter pada query yang sudah ada |

---

## 1. Masalah yang diperbaiki

Antrean prioritas tampil di layar display publik, diurutkan di atas, dan dipanggil lewat suara. PDF 2G meminta nomor
prioritas tidak ditampilkan.

## 2. Proses bisnis

Layar display memuat ringkasan, daftar, dan panggilan lewat tiga endpoint. Ketiganya kini memakai satu penyaring yang
menyingkirkan antrean `IsPriorityQueue = true`, berapa pun mode audiens perangkatnya. Suara panggilan dibuat dari antrean
yang lolos penyaring, sehingga antrean prioritas juga tidak bersuara. Ruang kerja perawat dan dokter memakai controller
lain dan tidak terdampak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`QueueDisplayRuntimeController.cs` (seluruh endpoint dan `BuildDisplayQueueQuery`), `QueueVoiceService` (pemakai),
`NurseStationQueueController.cs:238`, `DoctorQueueController.cs:488`, frontend `use-queue-display-screen.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/QueueDisplayRuntimeController.cs` | `BuildDisplayQueueQuery` menambah `.Where(x => !x.IsPriorityQueue)` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons sama. Isi `summary`, `items`, `called` tidak lagi memuat atau menghitung antrean prioritas |
| Database | Tidak ada perubahan schema; `has-pending-model-changes` bersih |
| Keamanan/Auth | Privasi: nomor prioritas tidak lagi terlihat di layar publik. Izin tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Queue Display Runtime

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/summary` | Ringkasan tanpa antrean prioritas | Policy `QueueDisplayRuntimeRead` |
| `GET` | `/items` | Daftar tanpa antrean prioritas | Policy `QueueDisplayRuntimeRead` |
| `GET` | `/called` | Panggilan dan suara tanpa antrean prioritas | Policy `QueueDisplayRuntimeRead` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Build | 0 error | `PASS` | Keluaran build |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Keluaran perintah |
| QBE Strict | VIOLATION 0 | `PASS` | Keluaran QBE |
| `R-RP-03` items — akun display uji `QDD-RSMMC-00013` (cluster Rawat Jalan Spesialis) | 200, 7 item, Reguler uji 3/21/22 tampil, prioritas uji 5/10 tidak tampil, tidak ada item `isPriorityQueue` | `PASS` | `rt_mvp0.py` |
| `R-RP-03` summary | 200, `totalQueue` 7 (tanpa prioritas) | `PASS` | `rt_mvp0.py` |
| `R-RP-03` called | 200, tanpa nomor prioritas | `PASS` | `rt_mvp0.py` |

Uji manual: `NOT FEASIBLE` di tahap backend — suara pada perangkat fisik diuji saat UAT (`GAP-RP-06`).

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

**Keadaan DB sesudah run:** satu perangkat display uji `UJI RJP DISPLAY 1C75` (`QDD-RSMMC-00013`) beserta akun loginnya
dibuat lewat API admin.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-03` | Terpenuhi | Tiga endpoint |
| `UAT-RP-03` (sisi API) | Terpenuhi | Nomor 5/10 tidak tampil, 3/21/22 tampil |
| Laporan task | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Display berstatus `MemberOnly` akan kosong untuk tingkat prioritas (`GATE-RP-01`) |
| Masalah yang diketahui | — |
| Risiko tersisa | Admin perangkat display perlu diberi tahu soal `GATE-RP-01` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat ringkasan sesi |
| Langkah berikutnya | `BE-RJP-004` + `FE-RJP-001` (rilis bersama) |
