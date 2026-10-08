# Laporan Perubahan Backend — `BE-RWI-198`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-198` |
| Judul | Data cetak General Consent dan Surat Persetujuan |
| Slice | Slice A; gelombang `RWA-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-198` |
| Trace | `FR-RWA-020` s.d. `022` (bagian cetak), `140`, `142`; `RWI-DEC-230`, `233`, `246`, `251`, `252`; `RWI-AC-347`, `348`, `372`, `373`; API 12.2, 12.3 `GeneralConsentPrintDataResponse` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-193` ✅ |
| Klasifikasi | `LIGHT` — baca saja |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionWorkspaceQueryService.cs`, controller |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API pasien samaran **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

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

Formulir General Consent V1 dan Surat Persetujuan 12 butir harus terisi dari server tanpa satu pun jalur
tulis. General Consent tetap *fail-closed* (`RWI-DEC-230`, `233`).

## 2. Proses bisnis

1. **Data cetak.** Respons memuat:
   - kop dari profil rumah sakit;
   - kode formulir dan kota dari pengaturan, serta tanggal cetak menurut zona waktu rumah sakit;
   - identitas pasien beserta sapaan dan umur;
   - episode (nomor, kunjungan, masuk, kelas, unit, kamar, bed, DPJP);
   - penjamin.
2. **Tipe kamar** (`RWI-DEC-251`). "Khusus" bila episode membutuhkan isolasi, atau bed/kamar
   penempatan berjalan bertanda isolasi atau perawatan intensif. Selain itu "Umum". Nama kamar dan kelas
   tidak pernah dipakai menebak. Alasannya ikut dikembalikan.
3. **Calon penanda tangan** (`RWI-DEC-252`). Pasien sendiri, seluruh relasi aktif dengan jenis terstruktur
   (`Spouse`, `Child`, `Mother`, `Father`, …), lalu kontak darurat sebagai daftar dengan teks hubungannya.
   Server tidak memilih diam-diam. Data wali gagal dimuat → peringatan "Tidak ditemukan di data
   wali/kontak darurat. Isi manual.".
4. **Tidak ada jalur tulis.** Tidak ada endpoint tulis General Consent, dan `patient-consents` tidak dipanggil.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstRoom`/`MstBed` (penanda isolasi/intensif), `InpEpisode.RequiresIsolation`, relasi dan kontak darurat (`BE-RWI-187`), pengaturan (`BE-RWI-185`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `GetGeneralConsentPrintDataAsync`, `ResolveRoomType` |
| `Controllers/InpatientAdmissionDocumentController.cs` | `GET /general-consent/print-data` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.2 satu endpoint baca; aditif `Warnings[]`, `Guarantor.CardNumber/MemberNumber/PolicyNumber` |
| Database | Tidak ada |
| Keamanan/Auth | `InpatientAdmissionDocument : Read`; tidak butuh hak master pasien |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/general-consent/print-data` | Data cetak General Consent V1 dan Surat Persetujuan 12 butir | `InpatientAdmissionDocument : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Tidak ada jalur tulis | Method `AsNoTracking`; tidak ada endpoint tulis GC; `patient-consents` tidak dirujuk | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Pasien samaran dengan relasi dan kontak darurat | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bed intensif atau episode isolasi → "Khusus"; kamar bernama "Melati Khusus" tanpa penanda → "Umum" | Terpenuhi (source) | `ResolveRoomType` hanya membaca penanda |
| 2. Relasi terstruktur seluruhnya; kontak darurat hanya daftar beserta teks | Terpenuhi (source) | `GetGeneralConsentPrintDataAsync` |
| 3. Tidak ada endpoint tulis GC; `patient-consents` tidak dipanggil | Terpenuhi | Review controller dan service |
| 4. Kode formulir dan kota dari pengaturan | Terpenuhi (source) | `setting.GeneralConsentFormCode`, `DocumentSigningCity` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | General Consent tetap *fail-closed*; penyimpanan menunggu `DEC-INP-003` |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | Berkas Workspace PPRI bersama (`??`) |
| Langkah berikutnya | Frontend mengganti bacaan `use-inpatient-consent-print.js` ke endpoint ini (`FE-RWI-*`) |
