# Laporan Perubahan Backend — `BE-RWI-194`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-194` |
| Judul | Siklus dokumen admisi, dengan Permintaan Privasi sebagai jenis pertama |
| Slice | Slice B — Ruang kerja dan dokumen bertanda tangan; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-194` |
| Trace | `FR-RWA-060` s.d. `062`, `120` s.d. `125`, `128`; `RWI-DEC-237` s.d. `240`, `263`; `INV-RWA-01` s.d. `06`, `10`; `RWI-AC-351`, `352`, `355` s.d. `358`, `384`; API 12.2, 12.3; state 10.1; validation 15.1, 15.3 s.d. 15.6; data 20.2 s.d. 20.7 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-193` ✅ |
| Klasifikasi | `HEAVY` — task terbesar; siklus status, konkurensi, transaksi versi, salinan beku |
| Task mode | `BACKEND` |
| Target tulis | `InPatientManagement/Services/InpAdmission{DocumentService*,SignatureService,SnapshotBuilder,PrefillService,PrintService,DocumentRules}.cs`, `DTOs/InpatientAdmissionDocumentDtos.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS`, migration diterapkan ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi proses bisnis dua akun dan logger runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DEL-001` |
| Wewenang | Source: ya |

---

## 1. Kebutuhan

Setiap jenis dokumen admisi harus berperilaku sama dari konsep sampai lengkap, dengan salinan beku saat
dikunci (`RWI-DEC-263`), tanda tangan per slot, versi koreksi, dan cetakan bertanda status. Permintaan
Privasi dibangun sebagai jenis pertama untuk membuktikan siklus generik.

## 2. Proses bisnis

1. **Simpan konsep** (`POST /documents`).
   - Satu dokumen aktif per jenis per episode: kedua → `409 INP-ADM-DOC-003`. Dua simpan bersamaan
     ditahan index unik, dan pelanggarannya dipetakan ke kode yang sama.
   - `Idempotency-Key` (≤ 80) sama → dokumen yang sama.
2. **Ubah konsep** (`PUT`). `RowVersion` basi → `409 INP-ADM-DOC-004`. Dokumen bukan `Draft` →
   `409 INP-ADM-DOC-005`.
3. **Kunci** (`PATCH lock`).
   - Seluruh isian yang kurang ditolak dalam **satu** `422` berisi daftar `Details`.
   - Pihak bersumber relasi/kontak dibaca ulang dari service pemilik.
   - Salinan beku (`SnapshotJson` format 1) dibentuk dari kop, identitas pasien, episode, dan penjamin.
     Sumber wajib gagal → `422 INP-ADM-DOC-027`.
4. **Buka kunci** hanya tanpa tanda tangan (`409 INP-ADM-DOC-006`).
5. **Tanda tangan.**
   - Slot kertas pasien/keluarga dicatat petugas: nama 1–200, hubungan wajib, waktu tidak sebelum
     kunci dan tidak lebih dari 5 menit ke depan (`400`).
   - Slot petugas diatestasi akun yang masuk; nama dan jabatan dibekukan.
   - Penolakan:
     - dokumen masih `Draft` → `409 INP-ADM-DOC-030`;
     - slot bukan wajib → `422 INP-ADM-DOC-031`;
     - slot sudah terisi → `409 INP-ADM-DOC-032` "Kolom ini sudah ditandatangani *nama* pada *waktu*.";
     - satu akun dua slot → `422 INP-ADM-DOC-033`.
   - Slot wajib terakhir terisi → `Completed`.
6. **Versi koreksi** (`POST revisions`) hanya dari `Completed`, alasan 10–500. Dalam satu transaksi,
   versi lama menjadi `Superseded` lalu versi baru `Draft` `VersionNo + 1` lahir, sehingga index aktif
   tidak pernah dilanggar.
7. **Buang konsep** hanya oleh pembuatnya (`422 INP-ADM-DOC-008`). **Batal** oleh pemegang `Cancel`,
   alasan ≥ 10. Keduanya meninggalkan jejak, tanpa `DELETE`.
8. **Cetak per status** (`GET …/print`):
   - `Draft`: data hidup, penanda "KONSEP — BELUM DITANDATANGANI".
   - `AwaitingSignature`: "Lembar untuk ditandatangani — versi *n*".
   - `Completed`: final.
   - `Superseded`: "DIGANTIKAN VERSI *n+1*".
   - `Cancelled`: "DIBATALKAN".
   - Selain `Draft`, data diambil dari salinan beku.
   - Episode batal → "ADMISI DIBATALKAN".
   - `SourceChangedSinceLock` menandai data pasien yang berubah sesudah dikunci.
9. **Privasi.** Privasi transportasi Ya/Tidak; kerabat yang boleh menjenguk dan permintaan khusus
   masing-masing tersimpan per baris, maksimal 3, ≤ 200 karakter. Slot wajib: pasien/keluarga dan
   kepala ruangan.
10. **Penjaga episode** (`BE-RWI-193`): tulis pada `Closed`/`Cancelled` → `409 INP-ADM-DOC-001`; pada
    `Draft` → `409 INP-ADM-DOC-002`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

State 10.1, validation 15.1–15.6, kamus data 20.2–20.7, flowchart `05`; pola `RowVersion` `InpAdmissionReferral`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpAdmissionDocumentService.cs` | Baru:<br>• daftar, detail, rupiah;<br>• buat, ubah, kunci, buka kunci, buang, batal, versi koreksi;<br>• `SaveAsync` (konkurensi → `004`); pemetaan pelanggaran unique `23505` |
| `Services/InpAdmissionDocumentService.Content.cs` | Baru — penerapan isi per jenis, pihak, validasi kunci `VAL-RWA-20` s.d. `25` |
| `Services/InpAdmissionDocumentService.Response.cs` | Baru — respons dokumen, slot, `AvailableActions` dari status dan hak pengguna |
| `Services/InpAdmissionSignatureService.cs` | Baru — catatan kertas dan atestasi per slot |
| `Services/InpAdmissionSnapshotBuilder.cs` | Baru — salinan beku format 1 |
| `Services/InpAdmissionPrefillService.cs` | Baru — isian bawaan dan alasan tidak dapat dibuat |
| `Services/InpAdmissionDocumentRules.cs` | Baru — slot wajib, label V1, nama status |
| `Services/InpAdmissionPrintService.cs` | Bagian data cetak dokumen dan penanda status |
| `DTOs/InpatientAdmissionDocumentDtos.cs` | Baru — seluruh request/response dokumen |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.2/12.3 bagian dokumen (lihat tabel endpoint [`BE-RWI-193`](BE-RWI-193.md#4-dokumentasi-endpoint)) |
| Database | Tabel `BE-RWI-192`; tidak ada skema baru |
| Keamanan/Auth | Slot dijaga aksi berbeda; `AvailableActions` dihitung lewat `AccessPermissionService`. Logger hanya `EntityId`/controller/aksi/status: nama pihak, nomor identitas, isi privasi, dan alasan tidak pernah masuk |

## 4. Dokumentasi endpoint

Lihat tabel lengkap pada [`BE-RWI-193` bagian 4](BE-RWI-193.md#4-dokumentasi-endpoint). Endpoint task ini:
- `GET /prefill/{documentType}`, `/documents`, `/documents/{id}`, `/documents/{id}/print`;
- `POST /documents`, `PUT /documents/{id}`;
- `PATCH lock`, `unlock`, `discard`, `cancel`; `POST revisions`;
- `POST signatures/patient-or-family`, `admission-officer`, `head-nurse`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; transaksi versi koreksi | `BeginTransaction` → lama `Superseded` → simpan → baru `Draft` → simpan → commit | `PASS` | Review `ReviseAsync` |
| Payload logger | Hanya `EntityId`, `Controller`, `Action`, `StatusCode` | `PASS` | `LogAndReturnAsync` di controller |
| Tidak ada `DELETE` | Tidak ada `[HttpDelete]` | `PASS` | Review controller |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Proses bisnis dua akun pada data samaran | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Privasi `Draft` → kunci → kertas → atestasi Kepala Ruangan → `Completed`; cetakan per status dari salinan beku | Terpenuhi (source) | `LockAsync`, `SignatureService`, `GetDocumentPrintAsync` |
| 2. Alamat diubah sesudah kunci → cetakan lama, `SourceChangedSinceLock = true` | Terpenuhi (source) | `PatientChanged` pada respons |
| 3. Buka kunci hanya tanpa tanda tangan (`006`) | Terpenuhi (source) | `UnlockAsync` |
| 4. Versi koreksi dari `Completed`, alasan 10–500, satu transaksi | Terpenuhi (source) | `ReviseAsync` |
| 5. Batal ≥ 10 oleh `Cancel`; buang hanya pembuat (`008`) | Terpenuhi (source) | `CancelAsync`, `DiscardAsync`, atribut `Cancel` |
| 6. Dokumen aktif kedua `003`; simpan bersamaan → satu baris | Terpenuhi (source + DB) | Pemeriksaan + `UX_InpAdmissionDocument_Episode_Type_Active` |
| 7. `RowVersion` basi `004` | Terpenuhi (source) | `IsConcurrencyToken`, `SaveAsync` |
| 8. `031`, `032`, `030`; tanpa `SignAsHeadNurse` `403` | Terpenuhi (source) | `PrepareAsync`; atribut endpoint |
| 9. Waktu kertas sebelum kunci atau masa depan ditolak | Terpenuhi (source) | `RecordPaperSignatureAsync` (`400`) |
| 10. Tulis `Closed`/`Cancelled` `001`, `Draft` `002` | Terpenuhi (source) | `InpAdmissionWriteGuard` |
| 11. Tidak ada `DELETE` | Terpenuhi | Review |
| 12. Kerabat dan permintaan khusus per baris, maks. 3 | Terpenuhi (source + DB) | `ApplyPrivacy`; `CK_InpAdmissionPrivacyEntry_LineNo` |
| 13. `Idempotency-Key` sama → hasil sama | Terpenuhi (source + DB) | Replay + index unik |
| 14. Payload logger tanpa kolom sensitif | Terpenuhi (source) | Controller |
| DoD build | Terpenuhi | Build pengguna 0 error |

Seluruh butir "Terpenuhi (source)" belum dibuktikan runtime; verifikasi API **dikecualikan atas keputusan pengguna 8 Oktober 2026**.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta kontrak (aditif):<br>• `CannotCreateReasonCode` menambah `GuarantorUnavailable`, `EpisodeNotWritable`, `HandoverItemsMissing`;<br>• beberapa respons memuat `Warnings[]` |
| Masalah yang diketahui | Baris anak dokumen `Draft` yang dibuang petugas dihapus fisik (dokumen terkunci tidak pernah) |
| Risiko tersisa | Belum ada bukti runtime dua akun |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `??` berkas service dokumen, signature, snapshot, prefill, rules, DTO dokumen |
| Langkah berikutnya | Uji pengguna: alur Privasi lengkap dua akun |
