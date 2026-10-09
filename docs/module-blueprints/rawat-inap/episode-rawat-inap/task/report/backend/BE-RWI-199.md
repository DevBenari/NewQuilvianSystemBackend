# Laporan Perubahan Backend — `BE-RWI-199`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-199` |
| Judul | Serah Terima Pasien Baru |
| Slice | Slice B; gelombang `RWA-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-199` |
| Trace | `FR-RWA-030` s.d. `034`; `RWI-DEC-239`, `241`, `255`, `262`; `INV-RWA-04`, `11`, `12`; `RWI-AC-353`, `354`, `359`, `360`, `376`, `383`; validation `VAL-RWA-20`, `21`, `30` s.d. `34`; data 20.5; flowchart `06` |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-194` ✅, `BE-RWI-186` ✅ |
| Klasifikasi | `HEAVY` — tiga tanda tangan berbeda akun, syarat bed, saran sistem |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionPrefillService.cs`, `InpAdmissionDocumentService.Content.cs`, `InpAdmissionSignatureService.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi tiga akun dan penempatan bed **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-PERM-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Serah terima pasien baru memakai butir master `STPB-*` yang dibekukan saat dokumen dibuat, menampilkan
saran sistem tanpa memilih otomatis, dan selesai dengan tiga tanda tangan petugas berbeda sesudah pasien
menempati bed.

## 2. Proses bisnis

1. **Pembekuan butir.** Dokumen dibuat → butir aktif jenis serah terima dibekukan menurut `SortOrder`.
   Sub-butir 02A–02C langsung di bawah induknya, dan sub-butir yatim menjadi butir utama. Kode, nama,
   nomor butir, dan nomor induk disalin ke `InpAdmissionHandoverItem`; nama master yang berubah kemudian
   tidak mengubah dokumen. Tanpa butir aktif → tidak dapat dibuat (`HandoverItemsMissing`).
2. **Saran sistem.** Hanya tampil selama `Draft`, dan petugas tetap memilih Sudah/Belum:
   - `ReferralLetter`: "Sudah — saran sistem (surat pengantar *dr*, *dd-MM-yyyy*)", hanya bila surat
     `Issued` ada;
   - `DepositStatementCompleted`;
   - `BaseDataPrinted`: sesudah IPD dicetak;
   - `WristbandPrinted`: sesudah gelang dicetak, menyebut pencetak;
   - `CostEstimateCompleted` diam sampai `BE-RWI-203`;
   - `LabelAndGeneralConsent` tanpa saran selama *fail-closed*.
3. **Kunci.** Seluruh kekurangan ditolak dalam satu `422`, misalnya "Butir 5 belum dipilih Sudah atau
   Belum." (`INP-ADM-DOC-020`) dan "Butir 11 berstatus Belum wajib diberi keterangan." (`021`).
4. **Tanda tangan.** Tiga slot wajib: Admission/Petugas PPRI, CRO, Perawat penerima.
   - Satu akun dua slot → `422 INP-ADM-DOC-033`; dua permintaan bersamaan ditahan
     `UX_InpAdmissionDocumentSignature_Document_Signer`.
   - Slot Perawat saat pasien belum menempati bed aktif → `422 INP-ADM-DOC-034`; slot CRO tidak
     terpengaruh.
   - Tanda tangan sebelum kunci → `409 INP-ADM-DOC-030`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Butir `STPB-*` (`BE-RWI-186`), log cetak (`BE-RWI-193`), surat pengantar (`BE-RWI-189`), `InpPatientLocationQuery`, backend 13.9 (slot wajib), 13.13.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionPrefillService.cs` | `BuildHandoverLinesAsync`, `ComputeSuggestionsAsync` |
| `Services/InpAdmissionDocumentService.Content.cs` | `ApplyHandover` (bekukan saat buat, pilihan/keterangan saat ubah), validasi kunci `020`/`021` |
| `Services/InpAdmissionSignatureService.cs` | `033` satu akun dua slot; `034` syarat bed lewat `HasActivePlacementAsync` |
| `Services/InpAdmissionDocumentRules.cs` | Slot wajib Serah Terima, label slot V1 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Memakai endpoint dokumen umum; slot `cro` (`SignAsCro`) dan `receiving-nurse` (`SignAsNurse`) |
| Database | Tabel `BE-RWI-192` |
| Keamanan/Auth | Aturan satu akun satu slot di tingkat bisnis dan index (`GUARD-RWA-01`) |

## 4. Dokumentasi endpoint

Lihat [`BE-RWI-193` bagian 4](BE-RWI-193.md#4-dokumentasi-endpoint). Khusus task ini:
- `GET /prefill/NewPatientHandover`;
- `POST …/signatures/cro`;
- `POST …/signatures/receiving-nurse`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Saran tidak memilih otomatis | `Choice` hanya dari request; saran hanya teks pada respons `Draft` | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Tiga akun dan penempatan bed pada data samaran | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | Butuh butir `STPB-*` dari seeder |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Butir 5 kosong dan butir 11 Belum tanpa keterangan → satu `422` dua pesan bernomor | Terpenuhi (source) | `ValidateForLock` |
| 2. Nama butir master diubah → dokumen tetap nama lama | Terpenuhi (source) | `ItemNameSnapshot` dibekukan saat buat |
| 3. Saran butir 1 hanya bila surat `Issued`; butir 9 sesudah IPD dicetak; butir 13 sesudah gelang; butir 12 tanpa saran | Terpenuhi (source) | `ComputeSuggestionsAsync` |
| 4. Akun sama dua slot → `033`; bersamaan ditolak index | Terpenuhi (source + DB) | `SignatureService`; `UX_…_Document_Signer` |
| 5. Slot Perawat saat bed dipesan → `034`; CRO tidak terpengaruh | Terpenuhi (source) | `AttestAsync` |
| 6. Sebelum kunci, tanda tangan CRO → `030` | Terpenuhi (source) | `PrepareAsync` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Data uji butuh seeder menyala sekali (`Seeders__RunInpatientMasterDataSeed=true`) agar 18 butir `STPB-*` ada |
| Masalah yang diketahui | Tidak ada data keanggotaan perawat per unit; perawat unit lain pemegang `SignAsNurse` dapat menandatangani (`GUARD-RWA-02`, risiko diterima) |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | Berkas Workspace PPRI bersama (`??`) |
| Langkah berikutnya | Uji pengguna dengan tiga akun |
