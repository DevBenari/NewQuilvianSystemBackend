# Laporan Perubahan Backend — `BE-RWI-201`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-201` |
| Judul | Selisih Biaya |
| Slice | Slice B; gelombang `RWA-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-201` |
| Trace | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `RWI-AC-377`; G-43, G-44; validation `VAL-RWA-10`, `14`, `24`; data 20.4, 20.9 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-194` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionDocumentService.cs`, `…Content.cs`, `InpAdmissionPrefillService.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi pasien asuransi dan tunai samaran **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Surat pernyataan selisih biaya hanya untuk pasien berpenjamin asuransi atau perusahaan, dengan subjek
berkode dan data pasien dari master.

## 2. Proses bisnis

1. **Syarat penjamin.** Pasien tunai → buat dan kunci ditolak `422 INP-ADM-DOC-010`. Penjamin yang
   melarang selisih dibebankan tetap boleh membuat surat (`RWI-DEC-256`). Penjamin gagal dibaca → `422`
   "coba lagi" tanpa menebak.
2. **Subjek** tersimpan sebagai kode: Diri sendiri, Istri, Suami, Anak, Saudara kandung lainnya. Pilihan
   terakhir wajib keterangan (`CK_InpAdmissionCostDifferenceStatement_Other`).
3. **Isian bawaan.** "Diri saya sendiri" dari identitas pasien (`PatientAsDeclarer`); tipe ID dipetakan
   dari teks master pasien.
4. **Kunci.** Nama, alamat, tipe ID, No. ID deklarer, serta kota dan tanggal wajib (`422 INP-ADM-DOC-024`).
5. **Telepon.** Dinormalkan ("0812-3456-7890" → "081234567890"); lebih dari 13 digit → `400`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Konteks penjamin (`BE-RWI-189`), validation `VAL-RWA-10`, `14`, `24`, data 20.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionDocumentService.cs` | `CheckCostDifferencePayerAsync` saat buat dan kunci |
| `Services/InpAdmissionDocumentService.Content.cs` | `ApplyCostDifference`; validasi kunci `024`; normalisasi telepon pihak |
| `Services/InpAdmissionPrefillService.cs` | `BuildPatientAsDeclarer`, `MapIdentityType`; alasan `NotRequiredForPayer`/`GuarantorUnavailable` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint dokumen umum |
| Database | `InpAdmissionCostDifferenceStatement`, `InpAdmissionDocumentParty` |
| Keamanan/Auth | Nomor identitas deklarer sensitif; tidak masuk logger |

## 4. Dokumentasi endpoint

Endpoint dokumen umum ([`BE-RWI-193` bagian 4](BE-RWI-193.md#4-dokumentasi-endpoint)), jenis `CostDifferenceStatement`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Aturan penjamin dibaca lewat source reader | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Pasien asuransi dan tunai samaran | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien tunai: buat dan kunci `422 INP-ADM-DOC-010` | Terpenuhi (source) | `CheckCostDifferencePayerAsync` di `CreateAsync` dan `LockAsync` |
| 2. Penjamin pelarang selisih tetap dapat membuat surat | Terpenuhi (source) | Hanya `PaymentType` yang diperiksa |
| 3. Subjek berkode; saudara lainnya wajib keterangan | Terpenuhi (source + DB) | `InpCostDifferenceSubject`; `CHECK` |
| 4. Nama, alamat, tipe ID, No. ID wajib; HP dinormalkan, 14 digit `400` | Terpenuhi (source) | `ValidateForLock`; `NormalizePhone`/`IsValidPhone` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nomor identitas deklarer sensitif (G-35) |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | Berkas Workspace PPRI bersama (`??`) |
| Langkah berikutnya | Uji pengguna pasien asuransi dan tunai |
