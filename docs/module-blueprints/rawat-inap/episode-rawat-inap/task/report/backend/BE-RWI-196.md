# Laporan Perubahan Backend — `BE-RWI-196`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-196` |
| Judul | Data gelang dan label pasien |
| Slice | Slice A; gelombang `RWA-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-196` |
| Trace | `FR-RWA-050` s.d. `053`; `RWI-DEC-243`, `253`, `259`; `RWI-AC-363`, `364`, `374`, `380`; API 12.2, 12.3 `IdentityLabelResponse`; validation `VAL-RWA-45`; flowchart `07` bagian 1 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-193` ✅ |
| Klasifikasi | `MEDIUM` — identifikasi pasien (keselamatan) |
| Task mode | `BACKEND` |
| Target tulis | `InpWristbandRules.cs`, `InpAdmissionWorkspaceQueryService.cs`, `InpAdmissionPrintService.cs`, controller |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API empat pasien samaran **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Gelang dewasa atau bayi dengan sapaan yang benar, dan label pasien tanpa nomor karangan, dengan QR berisi
No. RM saja (`RWI-DEC-259`).

## 2. Proses bisnis

1. **Jenis gelang.** Gelang Bayi bila pasien bertanda bayi baru lahir atau umurnya ≤
   `InfantWristbandMaxAgeYears` (bawaan 5). Selain itu Gelang Dewasa. Tanpa tanggal lahir umur tidak
   dapat dibuktikan, sehingga jenisnya Dewasa.
2. **Sapaan.**
   - Bayi baru lahir dengan ibu tercatat → "BY. NY. *NAMA IBU*" plus dua label kecil.
   - Di bawah 17 tahun → "An.".
   - Pria → "Tn.".
   - Wanita menikah, cerai, janda, atau berpisah → "Ny."; wanita belum menikah → "Nn.".
   - Status nikah `Unknown` → tanpa sapaan.
3. **Contoh nama gelang.** "BUDI SANTOSO, Tn." · "12 Mar 1981" · "45 th". Teks umur di bawah satu tahun
   ditulis "4 bln" atau "3 hr".
4. **Label.**
   - Kode RS dari pengaturan; kosong → kode situs.
   - Baris nama "*NAMA* / *KODE*"; tanggal lahir "dd/MM/yy"; "L / 45 th".
   - No. Kartu hanya `CardNumberSnapshot`; kosong tetap kosong.
5. **Hitungan cetak.** Respons memuat hitungan cetak per jenis.
6. **Validasi catat cetak.** Mencatat cetak gelang yang jenisnya berbeda dari hitungan server →
   `422 INP-ADM-PRT-005`; identitas tidak terbaca → `422 INP-ADM-PRT-004`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientQrPayloadBuilder` (`BE-RWI-187`), konteks penjamin (`BE-RWI-189`), `InpatientSettingValues`; validation 15.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpWristbandRules.cs` | Baru — `ResolveKind`, `ResolveSalutation`, `BuildDisplayName`, `AgeInYears`, `AgeText` |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `GetIdentityLabelsAsync` |
| `Services/InpAdmissionPrintService.cs` | Validasi jenis gelang dan identitas pada catat cetak (`PRT-004`, `PRT-005`) |
| `Controllers/InpatientAdmissionDocumentController.cs` | `GET /identity-labels` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.2 `/identity-labels`; delta aditif `Wristband.PrintKind` (jenis cetakan yang wajib dicatat) |
| Database | Tidak ada |
| Keamanan/Auth | `InpatientAdmissionDocument : Print` |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/identity-labels` | Data gelang dewasa/bayi dan label pasien | `InpatientAdmissionDocument : Print` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Fungsi murni; tidak ada nomor karangan | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Empat pasien samaran (pria 45 th, bayi baru lahir, anak 4 th, wanita status nikah `Unknown`) | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pria 45 th "*NAMA*, Tn."; bayi "BY. NY. *ibu*" + 2 label; anak 4 th Gelang Bayi "An."; wanita `Unknown` tanpa sapaan | Terpenuhi (source) | `InpWristbandRules` |
| 2. QR sama dengan isi QR pasien; pasien tanpa berkas QR tetap terlayani | Terpenuhi (source) | Isi QR dari builder; tidak membaca berkas `QrCodePath` |
| 3. Kode RS dari pengaturan, kosong → `SiteCode`; No. Kartu hanya `CardNumberSnapshot` | Terpenuhi (source) | `GetIdentityLabelsAsync` |
| 4. Jenis gelang berbeda → `422 INP-ADM-PRT-005` | Terpenuhi (source) | `InpAdmissionPrintService.ValidatePrintableAsync` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aturan sapaan dan batas umur wajib diverifikasi tim keselamatan pasien sebelum produksi (G-38) |
| Masalah yang diketahui | Bayi baru lahir tanpa ibu tercatat dicetak nama sendiri tanpa "BY. NY." (tidak dikarang) |
| Risiko tersisa | G-38 |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` `InpWristbandRules.cs` dan berkas Workspace PPRI bersama |
| Langkah berikutnya | Uji empat pasien samaran oleh pengguna; tinjauan G-38 |
