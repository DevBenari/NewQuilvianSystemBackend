# Laporan Perubahan Backend — `BE-RWI-124`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-124` |
| Judul | SOAP dan catatan keperawatan sebagai CPPT berjenis |
| Slice | Gelombang 1 — `KEP-V2-3` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-124` |
| Trace | `FR-KEP-077`; `RWI-AC-204`, `RWI-AC-205`; `RWI-DEC-115`, `RWI-DEC-140`; api-contract `keperawatan` 0.5.0 bagian 7.15; api-contract `dokter-rawat-inap` 0.6.0 bagian 12.4 |
| Contract version | `0.5.0` + `0.6.0` [DOK] |
| Dependency | `BE-RWI-094` [BE-DOK] ✅ 16 September 2026 |
| Klasifikasi | `LIGHT` — reuse; satu saringan aditif pada dua endpoint baca |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — `EXISTING / REUSE` ditambah saringan aditif; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tabel `TrxPatientIntegratedProgressNote` legacy |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE relevan | `QBE-API-001`, `QBE-PAGE-001` |
| Hak akses baru | Tidak ada |
| Database | Nol migration — kolom `NoteKind` lahir di migration R3 (`BE-RWI-094`) |

## 1. Masalah yang diperbaiki

Menu SOAP dan menu Catatan Keperawatan harus menulis ke CPPT yang sama dengan catatan dokter, dibedakan
jenisnya. `BE-RWI-094` sudah membuat kolom `NoteKind`, kebijakan profesi-jenis, dan saringan pada
`GET /episodes/{episodeId}`. Yang belum ada: saringan jenis pada daftar umum `GET /` dan timeline, sehingga
menu yang memakai kedua permukaan itu tetap menerima campuran seluruh jenis.

## 2. Proses bisnis

1. Ns. Siti menulis SOAP keperawatan Budi → `POST /patient-integrated-progress-notes` dengan
   `NoteKind = NursingSoap` (2). Profesi penulis dibaca dari **tautan akun**, bukan dari payload.
2. Ns. Siti menulis catatan naratif → `NoteKind = NursingNarrative` (3).
3. Menu SOAP membaca `?noteKind=2`; menu Catatan Keperawatan membaca `?noteKind=3` — pada
   `GET /episodes/{episodeId}`, `GET /`, maupun `GET /timeline`.
4. **Jalur tidak normal:** Ns. Siti mengirim `NoteKind = PhysicianNote` (1) → `400` "Jenis catatan
   \"Catatan Perkembangan Dokter\" tidak sah bagi profesi \"Nurse\". Jenis yang dapat dipilih: SOAP
   Keperawatan, Catatan Keperawatan." Akun tanpa profesi klinis → `403`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientIntegratedProgressNoteController.cs` (buat, ubah, daftar, timeline, per episode),
`CpptNoteKindPolicy.cs`, `CpptNoteKind.cs`, laporan `BE-RWI-094`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Query opsional `noteKind` pada `GET /` dan `GET /timeline` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif — tanpa `noteKind` perilaku sama seperti sebelumnya |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` — penjaga profesi-jenis milik `BE-RWI-094` tidak diubah |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menulis CPPT; `NoteKind` diperiksa terhadap profesi akun (sudah ada) | `PatientIntegratedProgressNote : Create` |
| `GET` | `/episodes/{episodeId}` | Lini masa per episode, saring `noteKind` (sudah ada) | `PatientIntegratedProgressNote : Read` |
| `GET` | `/` | Daftar umum — **saringan baru** `noteKind` | `PatientIntegratedProgressNote : Read` |
| `GET` | `/timeline` | Timeline — **saringan baru** `noteKind` | `PatientIntegratedProgressNote : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–4 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| Verifikasi kontrak API runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. SOAP keperawatan tersimpan sebagai `NursingSoap` | Terpenuhi | `CreateProgressNote` → `jenisCatatan`, `CpptNoteKindPolicy.SahUntukCatatanBaru` |
| 2. Catatan Keperawatan tersimpan sebagai `NursingNarrative` | Terpenuhi | Idem; bawaan perawat `JenisBawaan("Nurse") = NursingNarrative` |
| 3. Menu menyaring jenisnya sendiri | Terpenuhi | `GetByEpisode(noteKind)` (`BE-RWI-094`) + `GetProgressNotes(noteKind)` + `GetTimeline(noteKind)` (task ini) |
| 4. Jenis diperiksa terhadap profesi penulis | Terpenuhi | `ResolveCpptAuthorProfessionAsync` + `CpptNoteKindPolicy.Penolakan` → `400` |
| Catatan kartu: enum tidak ditambah di sini | Terpenuhi | `CpptNoteKind` tidak disentuh |
| DoD: `dotnet build` | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` pada controller di atas |
| Langkah berikutnya | Frontend menu SOAP dan Catatan Keperawatan memakai saringan `noteKind` |
