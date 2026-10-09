# Laporan Perubahan Backend — `BE-RWI-197`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-197` |
| Judul | Data Dasar Rawat Inap (IPD) |
| Slice | Slice A; gelombang `RWA-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-197` |
| Trace | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `253`, `254`, `258`; `RWI-AC-365`, `375`, `386`, `387`; API 12.2, 12.3; validation `VAL-RWA-44`; flowchart `07` bagian 2 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-193` ✅ |
| Klasifikasi | `MEDIUM` — banyak sumber, larangan isian tebakan |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionWorkspaceQueryService.cs`, `InpAdmissionPrintService.cs`, controller |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS` ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API tiga jenis kunjungan **dikecualikan atas keputusan pengguna 8 Oktober 2026**. Dokter perujuk luar dan tarif kamar memakai cadangan aman sampai `BE-RWI-190`/`191` ⛔ |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Lembar IPD "DATA DASAR RAWAT INAP/ODC" harus terangkai dari sumber resmi. Isian tanpa sumber dicetak
garis kosong, bukan tebakan (misalnya keluhan utama tidak boleh dipakai sebagai diagnosis masuk).

## 2. Proses bisnis

1. **Kiri:**
   - identitas pasien;
   - alamat KTP beserta wilayah;
   - agama, kecuali `Unknown` yang dicetak kosong;
   - tanggal masuk, kamar/bed, kelas;
   - dokter perujuk, DPJP.
2. **Kanan:**
   - penjamin dan kategori pembayaran (Perorangan/Perusahaan/Asuransi);
   - nomor peserta dan polis;
   - penanggung jawab: relasi atau kontak darurat bertanda penanggung jawab;
   - diagnosis dan rencana dari surat pengantar terbit terbaru;
   - petugas yang mengonfirmasi admisi, dari riwayat status `Draft` → `Admitted`;
   - perawat penanggung jawab aktif;
   - riwayat pindah ruangan;
   - butir Nilai Kepercayaan dan ringkasan Privasi dari dokumen `Completed`.
3. **`BlankFields` selalu** memuat:
   - `KtpRtRw`, `KtpVillage`, `DomicileAddress`;
   - `Occupation`, `OfficeAddress`, `OfficePhone`, `Nationality`;
   - `MutationNumber`, `InformationRecipient` (General Consent *fail-closed*);
   - `DirectorApproval`, `SpecialAttention`, `Cashier`;
   - `ResponsiblePersonDomicileAddress`.
   Isian bersumber yang kebetulan kosong ikut masuk `BlankFields`.
4. **Surat pengantar `Issued` terbaru** → diagnosis — rencana dan dokter penerbit. Surat `Cancelled`
   diabaikan. Tanpa surat, dokter perujuk luar dipakai sesudah `BE-RWI-190`; saat ini `NotYetAvailable`,
   sehingga dicetak garis kosong.
5. **Rupiah.** `RoomRateDisplay = "lihat kasir"`. `/base-data/amounts` mengembalikan `NotYetAvailable`
   sampai `BE-RWI-191`, dan `403` tanpa `ViewAmount`.
6. **Gagal baca.** Identitas atau episode gagal terbaca → `CanPrint = false`, "Cetak ditahan sampai data
   wajib terbaca lengkap.". Catat cetak IPD saat itu → `422 INP-ADM-PRT-004`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

PRD Lampiran A.6, API 12.3 `InpatientBaseDataResponse`, `InpStatusHistory`, `InpBedPlacement`, penugasan DPJP dan perawat, surat pengantar (`BE-RWI-189`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionWorkspaceQueryService.cs` | `GetBaseDataAsync`, `GetBaseDataAmountsAsync` |
| `Services/InpAdmissionSourceReader.cs` | `GetEpisodeAsync` (penempatan, DPJP, perawat, petugas konfirmasi); adapter `NotYetAvailable` dokter perujuk luar dan tarif kamar |
| `Services/InpAdmissionPrintService.cs` | Penahan cetak IPD (`PRT-004`) |
| `Controllers/InpatientAdmissionDocumentController.cs` | `GET /base-data`, `/base-data/amounts` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.2 dua endpoint; respons `Read` tanpa rupiah |
| Database | Tidak ada |
| Keamanan/Auth | `Read` untuk IPD; `ViewAmount` untuk tarif |

## 4. Dokumentasi endpoint

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/admission-workspace/base-data` | IPD terangkai tanpa rupiah, `BlankFields`, `CanPrint` | `InpatientAdmissionDocument : Read` |
| `GET` | `…/admission-workspace/base-data/amounts` | Tarif kamar per hari (`NotYetAvailable` sampai `BE-RWI-191`) | `InpatientAdmissionDocument : ViewAmount` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; tidak ada isian tebakan | Isian tanpa sumber selalu `BlankFields` | `PASS` | Review source |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Kunjungan dengan surat `Issued`, hanya `Cancelled`, tanpa surat | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pekerjaan, kewarganegaraan, RT/RW, kelurahan, domisili, kantor, mutasi, persetujuan direktur, perhatian khusus, kasir di `BlankFields` | Terpenuhi (source) | `GetBaseDataAsync` |
| 2. Surat `Issued` terbaru → diagnosis, rencana, dokter; `Cancelled` diabaikan; tanpa surat → kosong | Terpenuhi (source) | `GetLatestIssuedInpatientReferralAsync` (`BE-RWI-189`) |
| 3. `/base-data/amounts` → `NotYetAvailable`; tanpa `ViewAmount` → `403` | Terpenuhi (source) | `GetDailyRoomRateAsync` adapter; atribut |
| 4. Identitas/episode gagal → `CanPrint = false`; catat cetak `422 PRT-004` | Terpenuhi (source) | `GetBaseDataAsync`; `ValidatePrintableAsync` |
| 5. Penerima informasi kosong selama GC *fail-closed* | Terpenuhi (source) | `InformationRecipient` di `BlankFields` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dokter perujuk luar (`RWI-OQ-128`) dan tarif kamar (`RWI-OQ-129`) masih cadangan aman; IPD boleh dirilis lebih dulu (`RWI-AC-386`, `387`) |
| Masalah yang diketahui | — |
| Risiko tersisa | — |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | Berkas Workspace PPRI bersama (`??`) |
| Langkah berikutnya | `BE-RWI-190`, `191` setelah gerbang turun |
