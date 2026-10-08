# Laporan Perubahan Backend — `BE-RWI-193`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-193` |
| Judul | Fondasi baca dan cetak |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-193` |
| Trace | `FR-RWA-053`, `126`, `127`, `128`; `RWI-DEC-240` butir 7, `247`, `257`, `264`; `INV-RWA-08`, `09`, `14`; G-33; API 12.2, 12.5; validation `VAL-RWA-01`, `02`, `09`, `40`, `41`, `46`; permission 10.1, 10.2; `INT-RWA-01` s.d. `12`; data 20.13.1 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-187` ✅, `BE-RWI-188` ✅, `BE-RWI-189` ✅, `BE-RWI-192` ✅ |
| Klasifikasi | `HEAVY` — controller baru dengan sepuluh aksi hak akses, pintu baca lintas modul, cetak beralasan |
| Task mode | `BACKEND` |
| Target tulis | `InPatientManagement/{Controllers,Services,Helpers,DTOs}`, `InpPatientLocationQuery.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS`, migration diterapkan ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi registry dan API runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` (`InpPatientLocationQuery`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001` |
| Wewenang | Source: ya |
| Temuan preflight | Kontrak approved memakai `PATCH` untuk `lock`, `unlock`, `discard`, `cancel` (perubahan status), berbeda dari standar transaksi proyek yang memakai `POST` untuk aksi. Kode mengikuti kontrak; dicatat sebagai temuan untuk pemilik kontrak |

---

## 1. Kebutuhan

Seluruh layar Workspace PPRI butuh:
- satu controller dengan hak akses per tindakan;
- penjaga status episode;
- satu pintu baca data modul lain yang gagal aman;
- kop surat dari profil rumah sakit;
- log cetak yang mewajibkan alasan untuk cetak ulang.

## 2. Proses bisnis

1. **Penjaga tulis** (`InpAdmissionWriteGuard`):
   - episode `Draft` → `409 INP-ADM-DOC-002`, kecuali ringkasan dan kop;
   - menulis pada episode `Closed`/`Cancelled` → `409 INP-ADM-DOC-001`;
   - membaca dan mencetak ulang tetap boleh.
2. **Pintu baca** (`InpAdmissionSourceReader`) menjadi satu-satunya jalur ke data modul lain, dan
   tiap sumber berstatus `Available`, `Failed`, atau `NotYetAvailable`, tidak pernah `500`.
   - Sumber yang dibaca: identitas dan calon pihak (`BE-RWI-187`), profil rumah sakit (`188`),
     penjamin, surat pengantar, alergi (`189`), deposit lewat adapter Billing yang ada, dan kasus OK.
   - Dua sumber masih `NotYetAvailable` sampai gerbangnya turun: dokter perujuk luar (`RWI-OQ-128`)
     dan tarif kamar harian (`RWI-OQ-129`).
3. **Kop surat**: `GET /letterhead` boleh untuk episode status apa pun. Profil tidak tersedia →
   kop tanpa identitas, tanpa nilai bawaan.
4. **Catat cetak**: `POST /print-logs`. Salinan wajib 1–10; cetak dokumen wajib menunjuk dokumen
   milik episode.
   - Cetakan pertama untuk satu kunci (jenis + dokumen) tidak butuh alasan.
   - Cetakan berikutnya, atau cetakan apa pun pada episode `Closed`/`Cancelled`, wajib beralasan
     (`422 INP-ADM-PRT-001`).
   - Alasan `Other` wajib keterangan 1–200 karakter (`400`).
   - `Idempotency-Key` yang sama mengembalikan baris yang sama.
5. **Riwayat cetak**: `GET /print-logs` menghitung "cetakan ke-*n*" per kunci dari urutan baris.
6. **Zona waktu dan teks**: diambil dari profil rumah sakit, bawaan `Asia/Jakarta`; disimpan UTC.
   Tanggal berbahasa Indonesia dan rupiah dibentuk `InpAdmissionText`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpPatientLocationQuery`, `AccessPermissionService`, `PermissionRegistryDescriptor` (pola alias tanpa `[AccessAction]`),
pola `Idempotency-Key` `InpatientSurgeryBookingController`, `InpatientEpisodeController` (pola atribut dan logger),
`IInpBillingDepositAdapter`, `OperatingRoomCaseService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Controllers/InpatientAdmissionDocumentController.cs` | Baru — 28 endpoint, sepuluh aksi hak akses, logger non-`GET` |
| `InPatientManagement/Services/InpAdmissionWriteGuard.cs` | Baru |
| `InPatientManagement/Services/InpAdmissionSourceReader.cs` | Baru — pintu baca tunggal; `InpAdmissionEpisodeContext` |
| `InPatientManagement/Services/InpAdmissionResult.cs` | Baru — kode alasan `INP-ADM-DOC-*`, `INP-ADM-PRT-*`; hasil berkode status |
| `InPatientManagement/Services/InpAdmissionPrintService.cs` | Baru — catat cetak, riwayat cetak, data cetak dokumen (bagian dokumen milik `BE-RWI-194`) |
| `InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs` | Baru — kop surat (bagian lain milik `195` s.d. `200`) |
| `InPatientManagement/Helpers/InpAdmissionText.cs` | Baru — zona waktu, tanggal Indonesia, rupiah, normalisasi telepon |
| `InPatientManagement/DTOs/InpatientAdmissionWorkspaceDtos.cs` | Baru — kop, ringkasan, cetakan tanpa siklus, log cetak |
| `InPatientManagement/Services/InpPatientLocationQuery.cs` | `HasActivePlacementAsync` |
| `Program.cs` | Registrasi sembilan service Workspace PPRI |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 12.2 `Health Services / Inpatient Management / Inpatient Admission Workspace` |
| Database | Memakai tabel `BE-RWI-192`; tidak ada skema baru |
| Keamanan/Auth | Resource baru `InpatientAdmissionDocument` (`SortOrder = 19`) dengan sepuluh aksi:<br>• `AccessType = Read`: `Read`, `ViewAmount`, `Print`<br>• `AccessType = Create`: `Create`<br>• `AccessType = Update`: `Update`, `Sign`, `SignAsCro`, `SignAsNurse`, `SignAsHeadNurse`, `Cancel`<br>`/patient-rights` memakai pola alias `InpatientEpisode : Read`. Tidak ada nama peran di kode. Payload logger hanya `EntityId`, controller, aksi, status |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`

| Method | Path | Kegunaan | Hak akses | Task |
| --- | --- | --- | --- | --- |
| `GET` | `/summary` | Header, menu, kelengkapan, peringatan tanpa rupiah | `InpatientAdmissionDocument : Read` | `195` |
| `GET` | `/summary/amounts` | Status deposit berupiah, jatuh tempo terlewati | `: ViewAmount` | `195`, `202` |
| `GET` | `/letterhead` | Kop surat; episode status apa pun | `: Read` | `193` |
| `GET` | `/general-consent/print-data` | Data cetak General Consent V1 dan Surat Persetujuan | `: Read` | `198` |
| `GET` | `/prefill/{documentType}` | Isian bawaan sebelum dokumen dibuat | `: Read` | `194` |
| `GET` | `/documents` | Daftar dokumen; query `type?`, `includeHistory` | `: Read` | `194` |
| `GET` | `/documents/{documentId}` | Satu dokumen tanpa rupiah | `: Read` | `194` |
| `GET` | `/documents/{documentId}/amounts` | Angka Pelunasan Deposit | `: ViewAmount` | `202` |
| `GET` | `/documents/{documentId}/print` | Data cetak tanpa rupiah | `: Print` | `194` |
| `GET` | `/documents/{documentId}/amount-print` | Data cetak berupiah; service juga memeriksa `Print` | `: ViewAmount` | `202` |
| `GET` | `/identity-labels` | Gelang dan label | `: Print` | `196` |
| `GET` | `/base-data` | IPD tanpa rupiah | `: Read` | `197` |
| `GET` | `/base-data/amounts` | "Rencana @ Kamar (Rp)" | `: ViewAmount` | `197` |
| `GET` | `/print-logs` | Riwayat cetak; query `kind?`, `documentId?` | `: Read` | `193` |
| `GET` | `/patient-rights` | Ringkasan nilai kepercayaan dan privasi `Completed` | `InpatientEpisode : Read` (alias) | `200` |
| `POST` | `/documents` | Simpan konsep; header `Idempotency-Key` | `: Create` | `194` |
| `PUT` | `/documents/{documentId}` | Ubah konsep | `: Update` | `194` |
| `PATCH` | `/documents/{documentId}/lock` | Kunci, bentuk salinan beku | `: Update` | `194` |
| `PATCH` | `/documents/{documentId}/unlock` | Buka kunci tanpa tanda tangan | `: Update` | `194` |
| `PATCH` | `/documents/{documentId}/discard` | Buang konsep sendiri | `: Update` | `194` |
| `POST` | `/documents/{documentId}/revisions` | Versi koreksi; header `Idempotency-Key` | `: Update` | `194` |
| `PATCH` | `/documents/{documentId}/cancel` | Batalkan beralasan | `: Cancel` | `194` |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` | Catatan tanda tangan kertas | `: Sign` | `194` |
| `POST` | `/documents/{documentId}/signatures/admission-officer` | Atestasi slot petugas admisi | `: Sign` | `194` |
| `POST` | `/documents/{documentId}/signatures/cro` | Atestasi slot CRO | `: SignAsCro` | `199` |
| `POST` | `/documents/{documentId}/signatures/receiving-nurse` | Atestasi slot perawat penerima | `: SignAsNurse` | `199` |
| `POST` | `/documents/{documentId}/signatures/head-nurse` | Atestasi slot kepala ruangan | `: SignAsHeadNurse` | `194` |
| `POST` | `/print-logs` | Catat cetak atau cetak ulang; header `Idempotency-Key` | `: Print` | `193` |

Tidak ada `DELETE` (`INV-RWA-03`). `PUT /procedure-plan-mark` tidak dibuat (di luar gelombang, `EPIC-RWA-09`). Respons gagal: `ApiResponse<object>` dengan `errors = { Code, Details[] }`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight; atribut hak akses | `[AccessAction]` dan `[AccessPermission]` sama nama; alias didukung `PermissionRegistryDescriptor` (dicatat per kunci); `SortOrder = 19` belum dipakai | `PASS` | Review source |
| Pencarian source identitas rumah sakit, kota V1, pola kode formulir di 24 berkas Workspace PPRI | 0 hasil; satu-satunya "Jakarta" adalah id zona waktu `Asia/Jakarta` (5 kemunculan) | `PASS` | Pencarian 8 Oktober 2026 |
| Dependency melingkar DI | Evaluator → source reader → service pemilik; tidak ada yang memakai `InpEpisodeService` | `PASS` | Pemeriksaan konstruktor |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| Registry sesudah aplikasi dinyalakan; API kop dan log cetak | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Registry memuat `InpatientAdmissionDocument` dengan sepuluh aksi | Terpenuhi (source) | Sepuluh nama `[AccessAction]`; pemeriksaan registry runtime dikecualikan |
| 2. `/letterhead` episode status apa pun; pencarian source nol hasil | Terpenuhi | `GetLetterheadAsync` memakai `LoadAsync` tanpa saringan status; pencarian di atas |
| 3. Aturan `POST /print-logs` | Terpenuhi (source) | `RecordPrintAsync`: `HasPreviousPrintAsync`, `IsClosedOrCancelled`, `PRT-001`, catatan `Other`, `ReplayAsync` |
| 4. "Cetakan ke-*n*" dari urutan | Terpenuhi (source) | `GetPrintLogsAsync` → `PrintSequence` |
| 5. Sumber gagal → `Failed`/`NotYetAvailable`, tidak `500` | Terpenuhi (source) | Setiap method source reader dibungkus `try/catch` kecuali pembatalan |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penyimpangan `PATCH` vs standar `POST` mengikuti kontrak approved; menunggu keputusan pemilik kontrak bila ingin diselaraskan |
| Masalah yang diketahui | `InpatientActorClaims` (sudah ada sebelum task ini) memuat pemeriksaan nama peran. Workspace PPRI hanya memakai `GetUserId` dari helper itu; pemeriksaan peran tidak dipakai. Helper tidak disentuh |
| Risiko tersisa | Pemberian sepuluh aksi ke peran adalah konfigurasi admin (`RWI-OQ-124`) |
| Perubahan sampingan | `InpPatientLocationQuery.HasActivePlacementAsync` (aditif) |
| Interupsi | `NONE` |
| Status Git | `??` controller, `InpAdmission*` service, helper, DTO; `M` `InpPatientLocationQuery.cs`, `Program.cs` |
| Langkah berikutnya | Admin memberi aksi `InpatientAdmissionDocument` ke peran uji; verifikasi registry dan API oleh pengguna |
