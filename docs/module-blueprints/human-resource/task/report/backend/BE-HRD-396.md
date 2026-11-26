# Laporan Perubahan Backend — `BE-HRD-396`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-396` |
| Judul | Histori kepegawaian internal read-only dari SK |
| Slice | `S-FIN-M04` / Riwayat Pekerjaan (alias `FIN-HRD-04-BE-010`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-396` |
| Trace | `HRD-DEC-048`; instruksi Menu 04 `FIN-HRD-04-BE-010` |
| Contract version | Kontrak riwayat kepegawaian Workforce Core existing |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpEmploymentHistoryController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-DEL-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Riwayat internal yang bersumber dari SK atau sudah disetujui adalah catatan resmi (mutasi, promosi, terminasi). `PUT` dan `DELETE` sudah menolaknya `409`. Tiga jalan memutar masih terbuka:

1. **Cabut approval lalu ubah.** `PATCH /{id}/approval` dengan `IsApproved = false` menghapus tanda resmi pada riwayat yang hanya resmi karena disetujui; sesudah itu `PUT`/`DELETE` lolos.
2. **Sembunyikan.** `PATCH /{id}/status` menonaktifkan riwayat resmi sehingga hilang dari tampilan.
3. **Hapus bukti.** `DELETE /{id}/file` menghapus berkas SK pada riwayat resmi.

## 2. Proses bisnis

1. Riwayat internal dianggap resmi bila punya `ReferenceType`/`ReferenceId` (bersumber SK/transaksi) atau `ApprovedByUserId`.
2. Pada riwayat resmi: `PUT`, `DELETE`, mencabut approval, mengubah status aktif, dan menghapus berkas → `409 OFFICIAL_INTERNAL_HISTORY_READONLY`.
3. Perubahan riwayat resmi hanya lewat SK pemiliknya (Menu 07) atau Menu 12.
4. Pengalaman eksternal (`ExternalPrior`) tidak terpengaruh.

**Contoh.** Riwayat "Promosi Kepala Ruang 1 Jan 2026" (SK `SK/2026/001`) → `PATCH /status` `IsActive = false` → `409`; riwayat tetap tampil di daftar internal.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpEmploymentHistoryController.cs` (semua aksi tulis).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpEmploymentHistoryController.cs` | Helper `IsOfficialInternalHistory`; penjaga baru pada `PATCH /approval` (cabut), `PATCH /status`, `DELETE /file`; `PUT`/`DELETE` memakai helper yang sama; cek profil tidak mensyaratkan profil aktif (`BE-HRD-401`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; tiga aksi kini dapat membalas `409` pada riwayat resmi |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup manipulasi catatan resmi |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Employment History

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` / `DELETE` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employment-histories/{id}` | Ditolak `409` untuk riwayat resmi | `WfpEmploymentHistory : Update` / `Delete` |
| `PATCH` | `…/{id}/approval` | Mencabut approval riwayat resmi ditolak `409` | `WfpEmploymentHistory : Update` |
| `PATCH` | `…/{id}/status` | Ditolak `409` untuk riwayat resmi | `WfpEmploymentHistory : Update` |
| `DELETE` | `…/{id}/file` | Ditolak `409` untuk riwayat resmi | `WfpEmploymentHistory : Delete` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: cabut approval riwayat disetujui | `409` (dulu lolos, lalu `PUT` bebas) | `PASS` (penelusuran) | `Approve` |
| Penelusuran: nonaktifkan riwayat ber-SK | `409` | `PASS` (penelusuran) | `UpdateStatus` |
| Penelusuran: daftar `source=internal` | Riwayat ber-SK tetap tampil | `PASS` (penelusuran) | `GetEmploymentHistories` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) PUT/DELETE peristiwa resmi ditolak `409` | Terpenuhi | `IsOfficialInternalHistory` pada `PUT`, `DELETE`, dan tiga jalan memutar |
| (2) Peristiwa SK tetap tampil di riwayat internal | Terpenuhi | Filter `internal` + status aktif tidak dapat diubah |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | `POST` masih menerima `ReferenceId` dari klien tanpa memeriksa SK rujukannya; riwayat seperti itu langsung terkunci (tidak dapat dimanipulasi), tetapi asal rujukannya belum divalidasi |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-397` |
