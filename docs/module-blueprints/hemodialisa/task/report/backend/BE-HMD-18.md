# Laporan Perubahan Backend — `BE-HMD-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-18` |
| Judul | Serah Terima Tagihan ke Bounded Context Billing, Penanganan Sesi Dihentikan, dan Percobaan Ulang |
| Slice | `MVP-5` — Penutupan Sesi, Pengesahan Medis, Rekam Medis, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.6 |
| Trace | `FR-HMD-080` s.d. `FR-HMD-083`, `CAP-10`, `CAP-11`, `HMD-DEC-009`, `HMD-DEC-012`, `NFR-003`, `NFR-008`; `contracts/api-contract.md` grup Session (`billing-handoff`); `integration-contract.md` bagian 3 dan 6 |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-17` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; memakai `ClinicalMilestoneFactProducer` dan `BillingSourceContract` yang sudah ada tanpa mengubahnya |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

Tindakan HD yang sudah disahkan harus muncul di tagihan pasien tepat sekali. Bila penyerahan ke
Billing dijadikan satu transaksi dengan pengesahan, gangguan Billing akan membatalkan pengesahan
catatan medis; bila penyerahan bisa terkirim dua kali, pasien tertagih ganda. Sesi yang dihentikan
karena komplikasi tidak boleh menagih jasa HD sama sekali.

---

## 2. Proses bisnis

1. Transaksi pengesahan (`BE-HMD-17`) selesai lebih dulu. Baru **sesudah** commit,
   `HmdBillingHandoffService.HandOffAsync` dijalankan — di luar transaksi, karena
   `ClinicalMilestoneFactProducer` menolak berjalan di dalam transaksi.
2. Keputusan penyerahan:

| Keadaan sesi | Yang terjadi | `BillingHandoffStatus` |
| --- | --- | --- |
| Dihentikan (`StopReason` terisi) atau tindakan `IsBillable = false` | Tidak ada fakta tagihan diserahkan | `NotRequired` |
| Tindakan tidak ditemukan | Dicatat sebabnya | `Failed` |
| Billing menerima (`Emitted`) atau sudah pernah menerima (`Replayed`) | `BillingHandoffAt` diisi | `Succeeded` |
| Hasil belum pasti (`OutcomeUnknown`) | Pesan disimpan | `Pending` |
| Billing menolak atau tidak dapat dihubungi | Pesan disimpan; sesi **tetap** `Finalized` | `Failed` |

3. Fakta yang diserahkan memakai jalur resmi `Procedure` / `ProcedureCharge` dengan bentuk yang
   sama seperti tindakan lain: `SourceAggregateId` = id `TrxPatientProcedure`, kunjungan, jumlah
   dan satuan, serta cuplikan kode/nama/harga tindakan.
4. **Tanpa tagihan ganda.** `OccurredAt` diambil dari waktu pengesahan yang tersimpan
   (`SignedAt`), bukan waktu sekarang. Pengulangan menghasilkan sidik jari yang sama, sehingga
   Billing menjawab `Replayed`, bukan membuat tagihan kedua.
5. **Dua dokter pada tindakan** diisi sejak sesi dimulai (`BE-HMD-13`): `DoctorId` = dokter
   penanggung jawab sesi, `InstructingDoctorId` = dokter pembuat resep (`HMD-DEC-009`).
6. Koordinator melihat status (`GET /{id}/billing-handoff`) dan mengulang
   (`POST /{id}/billing-handoff/retry`). Pengulangan hanya untuk sesi `Finalized` dengan status
   `Pending` atau `Failed`; status lain dijawab apa adanya. Pengulangan tidak pernah menyentuh
   isi catatan medis.

**Contoh Billing sedang mati.** dr. A mengesahkan sesi pukul 12.00. Billing tidak dapat dihubungi.
Sesi tetap `Finalized` dan terkunci; `BillingHandoffStatus = Failed` dengan pesan galat. Pukul
14.00 koordinator menekan Ulangi; fakta terkirim dengan `OccurredAt` = 12.00, status menjadi
`Succeeded`. Menekan Ulangi sekali lagi tidak mengirim apa pun, karena status sudah `Succeeded`.

**Contoh sesi dihentikan.** Sesi Bapak Darma dihentikan karena komplikasi. Tindakannya
`IsBillable = false` dengan catatan alasan penghentian. Saat disahkan, status penyerahan
`NotRequired`; tidak ada tagihan jasa HD di billing pasien.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `integration-contract.md` bagian 3 (pemetaan dokter) dan 6 (Billing), `api-contract.md` grup Session
- `ClinicalMilestoneFactProducer`, `ClinicalMilestoneFactRequest`, `ClinicalFactEmissionKind`, `BillingSourceContract`, `PatientProcedureController` (bentuk fakta tindakan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdBillingHandoffService.cs` | Baru. Status, pengulangan, penyerahan, penyusunan fakta |
| `Services/HmdSessionFinalizationService.cs` | Memanggil penyerahan sesudah commit |
| `Controllers/HmdSessionController.cs` | Endpoint status dan pengulangan penyerahan |
| `DTOs/HmdSessionDtos.cs` | `HmdBillingHandoffResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 2 endpoint kontrak terpenuhi |
| Database | Tidak ada perubahan schema. Menulis fakta tagihan lewat producer Billing yang sudah ada |
| Keamanan/Auth | `HemodialysisSession : Read/RetryBillingHandoff` |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/billing-handoff` | Status penyerahan tindakan sesi ke Billing beserta sebab kegagalannya | `HemodialysisSession : Read` |
| `POST` | `/{id}/billing-handoff/retry` | Mengulang penyerahan yang tertunda atau gagal tanpa tagihan ganda | `HemodialysisSession : RetryBillingHandoff` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source pemisahan transaksi | Penyerahan hanya dipanggil sesudah commit pengesahan | `PASS` | `HmdSessionFinalizationService.cs` baris 66–83 |
| Pemeriksaan source idempotensi | `OccurredAt = SignedAt`; `Succeeded` tidak diserahkan ulang | `PASS` | `HmdBillingHandoffService.cs` baris 87–160 |
| Pemeriksaan source sesi dihentikan | `NotRequired`; `IsBillable = false` sejak `stop` dan dipertahankan saat pengesahan | `PASS` | `HmdBillingHandoffService.cs` baris 104–110; `HmdSessionService.cs` baris 959–968 |
| Uji dengan Billing dimatikan | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `BillingHandoffAndResilienceTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji kegagalan Billing dan runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Billing mati saat finalisasi → sesi tetap `Finalized`, status `Failed`, pengulangan berhasil tanpa tagihan ganda | Terpenuhi | Penyerahan sesudah commit; `OccurredAt` tetap; `Replayed` diperlakukan sukses |
| 2. Sesi dihentikan → `IsBillable = false`, tidak ada tagihan jasa HD | Terpenuhi | `NotRequired`; tidak ada fakta diserahkan |
| 3. Dua dokter berbeda tercatat pada tindakan pasien | Terpenuhi | `DoctorId` dan `InstructingDoctorId` diisi saat Mulai (`HmdSessionService.cs` baris 558–559) |
| DoD: handoff, isolasi kegagalan, retry, non-billable | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bila proses berhenti tepat di antara commit pengesahan dan penyerahan, status tetap `Pending` dan harus diulang koordinator; tidak ada pekerja latar yang mengulang otomatis |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Pendapatan tertunda bila baris `Pending`/`Failed` tidak dipantau; status tersedia di `GET /{id}/billing-handoff` |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji dengan Billing dimatikan oleh pemilik; pertimbangkan ringkasan sesi `Pending`/`Failed` di worklist |
