# Laporan Perubahan Backend — `BE-HMD-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-17` |
| Judul | Pengesahan Dokter, Pendaftaran Keutuhan Rekam Medis, dan Penguncian Catatan Sesi |
| Slice | `MVP-5` — Penutupan Sesi, Pengesahan Medis, Rekam Medis, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.6 |
| Trace | `FR-HMD-071` s.d. `FR-HMD-074`, `CAP-07`, `CAP-08`, `NFR-001`, `NFR-008`, Temuan Kritis 1; `contracts/api-contract.md` grup Session (`finalize`); `state-transition-matrix.md` bagian 3; `integration-contract.md` bagian 7; `HMD-VAL-070` s.d. `HMD-VAL-075` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-02` ✅, `BE-HMD-16` ✅ |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 1, logika 2, kontrak API 1, database 1, keamanan 2, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/`; method aditif pada `ClinicalDocumentIntegrityService` (lihat `BE-HMD-02`) |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE`; `Mrc` untuk method aditif Rekam Medis |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` pada `ClinicalDocumentIntegrityService` (izin pemilik 22 September 2026) |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

Catatan sesi HD adalah dokumen hukum. Tanpa pengesahan yang terikat pada dokter penanggung jawab
sesi, dokter lain yang kebetulan punya hak umum bisa mengesahkan catatan yang bukan tanggung
jawabnya. Tanpa penguncian yang benar-benar ditegakkan, catatan yang sudah disahkan masih bisa
diubah diam-diam — Temuan Kritis 1.

---

## 2. Proses bisnis

**Pengesahan** (`POST /{id}/finalize`) — satu transaksi berkunci per sesi:

1. Sesi harus `AwaitingFinalization`; yang sudah `Finalized` → `409`.
2. **Hanya dokter penanggung jawab sesi ini.** Dokter pelaku diturunkan dari relasi akun ke
   `MstDoctor`, lalu dibandingkan dengan `ResponsibleDoctorId` sesi. Tidak cocok →
   `403 HMD-VAL-072` "Pengesahan catatan hemodialisa hanya dapat dilakukan dokter penanggung
   jawab sesi." — walaupun pelaku memegang hak `HemodialysisRecord : Finalize`.
3. **Dua pelaku berbeda** bila `RequireDifferentSigner = true` (bawaan): penyusun dokumentasi
   tidak boleh mengesahkan sendiri → `422 HMD-VAL-074`.
4. Isian minimum dokumentasi diperiksa ulang → `422 HMD-VAL-070`.
5. Dokumen didaftarkan ke daftar keutuhan Rekam Medis sebagai `HemodialysisSession = 14` lewat
   `RegisterCountersignedAsync`: **penulis** = perawat (`DocumentedByUserId`), **penanda tangan** =
   dokter, perangkat dan alamat IP penanda tangan tercatat, status `Signed`, terkunci karena
   tanda tangan. Gagal → `422 HMD-VAL-073`.
6. `RecordHash` = SHA-256 isi klinis sesi (checklist, penilaian, observasi, obat, komplikasi, dan
   data inti sesi) — sidik jari untuk membuktikan catatan tidak berubah.
7. Sesi → `Finalized`, `SignedByUserId`/`SignedAt` (waktu server). Status serah terima Billing
   `Pending` untuk sesi selesai, `NotRequired` untuk sesi dihentikan.
8. `TrxPatientProcedure` → `Completed`, `IsExecuted`, waktu selesai = waktu akhir sesi; sesi
   dihentikan tetap `IsBillable = false`.
9. Commit. Bila penyimpanan atau pendaftaran gagal, **seluruh** langkah dibatalkan dan sesi tetap
   `AwaitingFinalization`.
10. **Sesudah** commit, serah terima Billing dijalankan (`BE-HMD-18`) — kegagalannya tidak
    menyentuh status `Finalized`.

**Pengembalian** (`POST /{id}/return-for-completion`): dokter penanggung jawab mengembalikan
dokumentasi kepada perawat dengan alasan wajib (`400 HMD-VAL-071`). Sesi kembali ke `Completed`
atau `Stopped`, `DocumentedBy` dikosongkan, alasan tersimpan pada `ReturnReason`.

**Penguncian dua tempat.** Setiap aksi tulis pada isi sesi memeriksa status `Finalized` **dan**
`ClinicalDocumentIntegrityService.EnsureMutableAsync(HemodialysisSession, …)`. Keduanya menolak
dengan `423 HMD-VAL-075` "Catatan sesi ini sudah disahkan dan tidak dapat diubah. Gunakan koreksi
rekam medis untuk memperbaikinya."

**Koreksi lewat addendum.** Endpoint Rekam Medis yang sudah ada,
`POST /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}`
dengan `documentKind = HemodialysisSession`, kini menerima catatan sesi HD. Karena penulis yang
tercatat adalah perawat, perawat itu sendiri yang berhak menambah koreksi. Data asli tidak
berubah; addendum tersimpan berdampingan beserta alasan dan waktunya.

**Contoh.** DPJP sesi Pasien C adalah dr. A. dr. B — memegang hak `Finalize` — mencoba
mengesahkan → `403`. dr. A mengesahkan → `Finalized`. Dua hari kemudian seseorang memanggil
`PUT …/checklist` → `423`. Perawat yang menyusun dokumentasi menyadari salah ketik berat badan
akhir dan membuat addendum; berat badan asli tetap tersimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Session, `state-transition-matrix.md` bagian 3, `integration-contract.md` bagian 7, `validation-matrix.md` `HMD-VAL-070` s.d. `075`
- `ClinicalDocumentIntegrityService`, `ClinicalNoteAddendumService`, `ClinicalNoteAddendumController`, `MrcClinicalDocumentIntegrity`, `InpatientClinicalContextService`, `TrxPatientProcedure`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdSessionFinalizationService.cs` | Baru. Pengesahan transaksional, pengembalian, `RecordHash` |
| `Controllers/HmdRecordController.cs` | Baru, 2 endpoint; meneruskan `User-Agent` dan alamat IP ke pendaftaran keutuhan |
| `Services/HmdSessionService.cs` | `GuardWritableAsync` dipanggil 16 method penulis sesi |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | `RegisterCountersignedAsync` (dilaporkan pada `BE-HMD-02`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 1 endpoint kontrak terpenuhi. **Delta**: `POST /{id}/return-for-completion`, memakai hak `HemodialysisRecord : Finalize` |
| Database | Tidak ada perubahan schema. Menulis `MrcClinicalDocumentIntegrity` dan mengubah `TrxPatientProcedure` di dalam transaksi pengesahan |
| Keamanan/Auth | `HemodialysisRecord : Finalize`. Kewenangan DPJP diperiksa dari data, bukan dari nama role. Penguncian dua tempat |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Session

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/finalize` | Dokter penanggung jawab mengesahkan dan mengunci catatan sesi | `HemodialysisRecord : Finalize` |
| `POST` | `/{id}/return-for-completion` | Dokter penanggung jawab mengembalikan dokumentasi untuk dilengkapi | `HemodialysisRecord : Finalize` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source pengesahan | Kunci → DPJP → penanda tangan berbeda → isian minimum → keutuhan → hash → status → tindakan → commit; rollback pada kegagalan | `PASS` | `HmdSessionFinalizationService.cs` baris 138–246 |
| Pemeriksaan source Billing sesudah commit | `HandOffAsync` dipanggil setelah transaksi selesai | `PASS` | `HmdSessionFinalizationService.cs` baris 66–83 |
| Pemeriksaan source penguncian | `GuardWritableAsync` status + `EnsureMutableAsync` | `PASS` | `HmdSessionService.cs` baris 1104–1111 |
| Pemeriksaan source addendum | `ClinicalNoteAddendumService.ResolveAuthorityAsync` membuka koreksi bagi penulis asli; `documentKind` diterima dari route | `PASS` | `ClinicalNoteAddendumService.cs` baris 53–95 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `SessionFinalizationAndLockingTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP dan uji rollback terpaksa — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Dokter bukan DPJP yang memegang hak `Finalize` → `403` dengan pesan DPJP | Terpenuhi, **dengan delta teks** | `403 HMD-VAL-072`; teks mengikuti `validation-matrix.md` |
| 2. Sesudah finalisasi, `PUT …/observations` atau `PUT …/checklist` → `423` | Terpenuhi | Penguncian dua tempat. Catatan: observasi ditambah lewat `POST`, bukan `PUT`; keduanya melewati penjaga yang sama |
| 3. Koreksi berat badan akhir lewat addendum; data asli tetap dan addendum tercatat berdampingan | Terpenuhi | Endpoint addendum Rekam Medis yang sudah ada + `HemodialysisSession` ditegakkan + penulis = perawat |
| DoD: transaksi pengesahan atomik, catatan terkunci, addendum berfungsi | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aksi penjadwalan (ubah jadwal, batal, penugasan) pada sesi final ditolak `423` lewat pemeriksaan status saja, tanpa `EnsureMutableAsync`, karena itu data penjadwalan, bukan isi dokumen |
| Masalah yang diketahui | Linimasa Rekam Medis belum mengenal `HemodialysisSession` untuk nama dan isi dokumen — lihat `BE-HMD-02` |
| Risiko tersisa | Kewenangan DPJP bergantung pada tautan akun pengguna ke `MstDoctor`; akun dokter tanpa tautan tidak dapat mengesahkan |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik (DPJP vs bukan DPJP, `423` sesudah final, addendum); task lanjutan linimasa Rekam Medis |
