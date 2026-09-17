# Laporan Perubahan Backend — `BE-RWI-125`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-125` |
| Judul | Obat bawaan dan pesanan tindakan dari sisi perawat |
| Slice | Gelombang 1 — `KEP-V2-3` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-125` |
| Trace | `FR-KEP-078`, `FR-KEP-079`; `INT-DOK-19`; api-contract `dokter-rawat-inap` 0.6.0 bagian 12.5 dan 12.8; state matrix `dokter-rawat-inap` 8.3 dan 8.5 |
| Contract version | `0.6.0` [DOK] |
| Dependency | `BE-RWI-101` [BE-DOK] ✅; `BE-RWI-097` [BE-DOK] ✅ (dan `BE-RWI-098` ✅ untuk daftar verifikasi) |
| Klasifikasi | `LIGHT` — verifikasi reuse, nol perubahan source |
| Task mode | `BACKEND` |
| Target tulis | Tidak ada source; hanya laporan dan register |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — `EXISTING / REUSE`; kelima kriteria sudah ditopang source `dokter-rawat-inap`; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement` (rekonsiliasi), `HealthServices / ClinicalManagement` (pesanan tindakan) |
| Registry / prefix | `Phm`, `Cli` — `ACTIVE` |
| Keberlakuan | `NEW CODE` milik `BE-RWI-097`/`101` — dibaca, tidak diubah |
| QBE relevan | `QBE-PERM-001`, `QBE-VAL-001` (diverifikasi) |
| Hak akses baru | Tidak ada |
| Database | Nol migration |

## 1. Masalah yang dijawab

Perawatlah yang mendata obat bawaan saat pasien masuk dan yang memasukkan pesanan tindakan atas instruksi
dokter. Task ini memastikan kedua jalur itu sudah terbuka bagi perawat **tanpa** memberinya wewenang dokter:
perawat membaca keputusan rekonsiliasi tetapi tidak memutuskannya.

## 2. Proses bisnis

1. Ns. Siti mencatat "Amlodipine 5 mg sekali sehari" sebagai obat bawaan Budi →
   `POST /pharmacy-management/medication-reconciliations`. Pencatat dari akun login dan wajib bertugas di
   unit episode.
2. Ns. Siti membaca keputusan dr. Ahmad per obat → `GET /{id}/decisions`.
3. Ns. Siti mencoba memutuskan "Lanjut" → `POST /{id}/decisions` → `403` (bukan dokter berpenugasan aktif).
   Butir hak akses `Decide` juga terpisah dari `Create`, sehingga admin tidak keliru memberikannya.
4. dr. Ahmad memberi instruksi lisan "rawat luka hari ini". Ns. Siti memesan tindakan →
   `POST /clinical-management/patient-procedures/inpatient-orders` dengan `InstructingDoctorId = dr. Ahmad`.
   dr. Ahmad wajib sedang bertugas atas Budi; tanpa itu `403`. Penginput tercatat dari akun Ns. Siti
   (`OrderedByUserId`), status verifikasi `Pending`.
5. Pesanan muncul pada daftar verifikasi instruksi dr. Ahmad dan ia memverifikasinya (`BE-RWI-098`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MedicationReconciliationController.cs`, `MedicationReconciliationService.cs`
(`RecordHomeMedicationAsync`, `DecideAsync`, `GetDecisionsAsync`), `PatientProcedureOrderService.cs`
(`CreateInpatientOrderAsync`, `GetInstructionVerificationWorklistAsync`, `VerifyInstructionAsync`),
laporan `BE-RWI-097`, `BE-RWI-098`, `BE-RWI-101`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Nol berkas source. Seluruh kriteria sudah ditopang source yang ada |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — dipakai apa adanya |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` — penjaga yang ada diverifikasi, tidak diubah |

## 4. Dokumentasi endpoint yang dipakai perawat

#### Health Services / Pharmacy Management / Medication Reconciliation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Perawat mencatat obat bawaan | `MedicationReconciliation : Create` |
| `GET` | `/episodes/{episodeId}` | Daftar obat bawaan satu episode | `MedicationReconciliation : Read` |
| `GET` | `/{id}/decisions` | Riwayat keputusan dokter — dibaca perawat | `MedicationReconciliation : Read` |
| `POST` | `/{id}/decisions` | **Ditolak `403` bagi perawat** | `MedicationReconciliation : Decide` |

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/inpatient-orders` | Perawat memesan tindakan dengan dokter pemberi instruksi | `PatientProcedure : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran source kriteria 1–5 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | Tidak dijalankan; task ini nol perubahan source | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| Verifikasi proses bisnis runtime (perawat mencoba memutuskan rekonsiliasi) | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Perawat mencatat obat bawaan | Terpenuhi | `MedicationReconciliationService.RecordHomeMedicationAsync` — penjaga unit perawat (`403` unit lain/tanpa pegawai) |
| 2. Perawat membaca keputusan, tidak dapat mengubah (`403`) | Terpenuhi | `GetDecisionsAsync` (Read); `DecideAsync` → `ResolveActorDoctorIdAsync` kosong → `403 PenolakanBukanDokterPerawat` |
| 3. Pesanan dengan dokter pemberi instruksi yang bertugas | Terpenuhi | `CreateInpatientOrderAsync` — cabang perawat: `InstructingDoctorId` wajib + `IsDoctorAssignedAsync` |
| 4. Penginput dari akun login | Terpenuhi | `OrderedByUserId = actorUserId` |
| 5. Pesanan muncul pada daftar verifikasi dokter | Terpenuhi | `GetInstructionVerificationWorklistAsync` — `InstructingDoctorId == doctorId` dan status `Pending` |
| DoD: `dotnet build` | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Uji runtime percobaan perawat memutuskan rekonsiliasi belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Hanya laporan ini `??` |
| Langkah berikutnya | Frontend menu Obat bawaan dan Tindakan |
