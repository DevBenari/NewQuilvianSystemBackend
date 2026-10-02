# Laporan Perubahan Backend — `BE-RWI-093`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-093` |
| Judul | Addendum oleh penulis asli tanpa penugasan aktif |
| Slice | Gelombang 3 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-093` |
| Trace | `FR-DOK-080`; `RWI-DEC-127`; `RWI-FACT-013` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-091` — registrasi keutuhan tersedia pada working tree aktif |
| Klasifikasi | `MEDIUM` — reuse penuh pada mesin keamanan addendum; tidak membutuhkan source behavior baru |
| Task mode | `BACKEND` |
| Model | GPT-5 |
| Commit backend saat dikerjakan | `36db5e6d1f1e6ee8b3a3dce8aa6c9d4adcc8060a`, branch `MHamzah` |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** sebagai `EXISTING / REUSE`; ditandai selesai tanpa `dotnet build` sesuai instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / MedicalRecordManagement` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001` |
| Disposisi | Reuse `ClinicalNoteAddendumService.ResolveAuthorityAsync` dan route author biasa; tidak membuat service atau policy tandingan |
| Keamanan | Kewenangan ditentukan dari `AuthorUserId` server-side dan ID pengguna login; bukan dari payload |

## 1. Hasil audit capability

Capability yang diminta task ini sudah tersedia utuh:

- `ResolveAuthorityAsync` menerima penulis asli untuk dokumen `Signed` maupun `LockedUnsigned`.
- Pemeriksaan penulis asli tidak membaca penugasan dokter dan tidak membaca status episode.
- Dokter lain ditolak kecuali memakai route pengganti yang berbeda dan memiliki kewenangan khusus.
- Addendum menambah baris baru dan penghitung, tidak mengubah isi maupun status dokumen induk.
- Jalur ini tidak memiliki operasi membuat catatan klinis baru.

Karena disposisinya memang `EXISTING / REUSE`, tidak ada alasan yang sah untuk menyalin atau
memodifikasi mesin addendum. Perubahan `BE-RWI-092` hanya menambahkan `CanAddAddendum` pada daftar
Catatan Saya agar frontend dapat menemukan capability yang sudah ada ini.

## 2. Dokumentasi endpoint yang direuse

Base path: `/api/v1/health-services/medical-record-management/clinical-note-addendums`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa kewenangan koreksi | `ClinicalNoteAddendum : Read` |
| `POST` | `/by-document/{documentKind}/{documentId}` | Penulis asli menambah addendum | `ClinicalNoteAddendum : Create` |
| `POST` | `/by-document/{documentKind}/{documentId}/as-substitute` | Jalur pengganti terpisah; tidak dipakai oleh pengecualian penulis asli | `ClinicalNoteAddendum : CreateAsSubstitute` |

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Status dokumen | **PASS statis** — `Draft` dan `Cancelled` ditolak; dua status terkunci diteruskan |
| Penulis asli tanpa penugasan | **PASS statis** — perbandingan langsung `AuthorUserId == actorUserId`, tanpa query assignment |
| Episode `Closed` | **PASS statis** — jalur author addendum tidak menolak berdasarkan status episode |
| Dokter lain | **PASS statis** — route author mengirim `actorHasSubstituteAuthority: false`, sehingga bukan penulis menerima `403` |
| Catatan baru pada episode tertutup | **PASS statis** — service hanya membuat `MrcClinicalNoteAddendum`; tidak membentuk dokumen klinis |
| QBE checker `Strict`, scope working tree | **PASS** — nol finding |
| `dotnet build` | **NOT RUN — instruksi eksplisit pengguna** |
| Uji runtime | **NOT RUN** |

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — author dapat addendum setelah assignment berakhir | Terpenuhi secara statis | Authority author tidak bergantung assignment |
| AC-2 — author dapat addendum pada episode `Closed` | Terpenuhi secara statis | Tidak ada episode guard pada jalur author |
| AC-3 — dokter lain ditolak | Terpenuhi secara statis | Perbandingan author server-side dan route substitute terpisah |
| AC-4 — tidak membuka pembuatan catatan baru | Terpenuhi secara statis | Entity yang dibuat hanya addendum |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan dokumen delivery |

## 5. Catatan penutup

Task ini sengaja selesai tanpa perubahan source khusus karena capability yang diminta sudah ada dan
sesuai kontrak. Build dan uji runtime belum dilakukan; tidak ada operasi Git atau database.
