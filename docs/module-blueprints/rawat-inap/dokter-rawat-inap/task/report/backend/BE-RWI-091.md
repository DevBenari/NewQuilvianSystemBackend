# Laporan Perubahan Backend — `BE-RWI-091`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-091` |
| Judul | Registrasi keutuhan sejak konsep (migration R2) |
| Slice | Gelombang 2 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-091` |
| Trace | `FR-DOK-074`, `FR-DOK-077`; `INV-DOK-18`; `RWI-DEC-138`, `RWI-DEC-144`, `RWI-DEC-151`; `VAL-DOK-43`; state matrix 0.6.0 bagian 8.1; `INT-DOK-15` |
| Contract version | `0.6.0` (status `draft`, blueprint revision 7 disetujui `RWI-DEC-150`) |
| Dependency | `BE-RWI-088` ✅; `BE-RWI-079` [BE-INP] ✅ |
| Klasifikasi | `MEDIUM` — kode saja (R2), menyentuh jalur tulis catatan dokter dan kajian medis serta mesin keutuhan milik `MedicalRecordManagement` |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Areas/HealthServices/MedicalRecordManagement/Services`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 16 September 2026 |
| Status | ✅ Selesai; validasi statis source. **`dotnet build` PASS 17 September 2026** (bagian 8); uji runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` (pemanggil), `HealthServices / MedicalRecordManagement` (mesin keutuhan) |
| Registry/prefix | `Cli` dan `Mrc` terdaftar `ACTIVE`; nol entity baru |
| Keberlakuan | `TOUCHED LEGACY` — `DoctorConsultationController` dan `PatientAssessmentController` masih mengakses `ApplicationDbContext` langsung; tidak dirapikan |
| Persetujuan pemilik mesin | Yoga Aji Pratama menyetujui registrasi sejak konsep — `RWI-DEC-151` |
| QBE relevan | `QBE-API-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-PERM-001` |
| Database | Nol perubahan bentuk data; nol migration |

## 1. Masalah yang diperbaiki

Mesin keutuhan dokumen sebelumnya baru mencatat catatan dokter dan kajian medis saat ditandatangani,
sehingga konsep tidak punya identitas: tidak dapat dikunci saat perawatan ditutup dan tidak muncul di
"Catatan Saya". Pada commit `a0a710db` sebagian besar pendaftaran sejak konsep sudah mendarat, tetapi
satu celah tertinggal: **jalur Selesai tidak menolak konsep yang sudah terkunci "Tidak
Ditandatangani"**. Akibatnya konsep yang dikunci penutupan perawatan tetap dapat diselesaikan
belakangan — catatan menjadi `Completed` sementara registrasinya tetap `LockedUnsigned`, dua keadaan
yang saling membantah dan sama dengan menandatangani mundur (dilarang `RWI-DEC-138` butir 2).

## 2. Proses bisnis

1. dr. Yoga menyimpan konsep SOAP Joko pukul 06.30 → catatan dan registrasi `Draft` lahir dalam satu
   transaksi. Gagal mendaftar = catatan ikut batal (tidak ada registrasi yatim).
2. Simpan otomatis (`PATCH /{id}/soap`) pukul 06.31 dua kali → **tidak** mendaftarkan apa pun; tetap
   satu registrasi. `RegisterAsync` juga idempoten terhadap baris yang sudah ada di basis data maupun
   baris yang baru ditambahkan pada unit kerja yang sama.
3. Pukul 08.00 dr. Yoga menekan Selesai walau penugasannya berakhir 07.00 → diterima, karena jalur
   penyelesaian memakai penjaga penulis (`ResolveForAuthorEditAsync`, tanpa syarat penugasan aktif)
   sesuai `FR-DOK-077`. `RegisterSignedAsync` mengambil registrasi yang **sama** lalu menaikkannya
   `Draft → Signed`.
4. Waktu klinis tetap di `TrxDoctorConsultation.ClinicalDateTime`; waktu tanda tangan di
   `MrcClinicalDocumentIntegrity.SignedAt` — dua nilai terpisah.
5. **Jalur tidak normal (yang diperbaiki task ini):** episode Joko ditutup 13.00 dan konsep dikunci
   `LockedUnsigned`. Pukul 15.00 dr. Yoga menekan Selesai atau menyunting → `409` "Catatan ini terkunci
   karena perawatan pasien sudah ditutup. Lengkapi lewat addendum dari Catatan Saya."
6. Membatalkan konsep → registrasi menjadi `Cancelled`, barisnya tidak dihapus.

Poliklinik, medical check-up, dan IGD tidak berubah: konsep mereka tidak didaftarkan sejak draf,
sehingga tidak pernah berstatus `LockedUnsigned` dan penjaga baru melewatkannya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ClinicalDocumentIntegrityService.cs`, `DoctorConsultationController.cs`, `PatientAssessmentController.cs`,
`ConsultationFinalizationService.cs`, `InpatientClinicalContextService.cs`, `InpDischargeService.Closure.cs`,
state matrix 0.6.0 bagian 8.1, validation matrix 10.1, integration contract 12.5.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Method baru `EnsureDraftStillOpenAsync`: registrasi `LockedUnsigned` → `409` VAL-DOK-43; keadaan lain diteruskan ke `EnsureMutableAsync` tanpa mengubah pesan lama |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | `PUT /{id}` dan `PATCH /{id}/soap` memakai penjaga baru; `PATCH /{id}/complete` kini memanggil penjaga baru sebelum finalisasi |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `PUT /{id}` memakai penjaga baru; `PATCH /{id}/complete` memanggilnya sebelum penyelesaian |

Kode pendaftaran sejak konsep, pembatalan registrasi, dan idempotensi lapis kedua sudah ada sejak
commit `a0a710db` dan dipetakan apa adanya sebagai bukti kriteria 1–3.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint baru. Perilaku `PUT`, `PATCH /soap`, `PATCH /complete` pada catatan dokter dan kajian medis rawat inap: konsep `LockedUnsigned` dijawab `409` beserta arahan addendum |
| Database | `NOT APPLICABLE` — R2 kode saja; registrasi `Draft` yang telanjur terbentuk tidak dihapus bila mundur |
| Keamanan/Auth | Nol hak akses baru; penjaga penulis `BE-RWI-088` tetap berlaku |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter; rawat inap → registrasi `Draft` dalam transaksi yang sama | `DoctorConsultation : Create` |
| `PUT` | `/{id}` | Mengubah konsep; `LockedUnsigned` → `409` | `DoctorConsultation : Update` |
| `PATCH` | `/{id}/soap` | Simpan otomatis tanpa registrasi tambahan; `LockedUnsigned` → `409` | `DoctorConsultation : Update` |
| `PATCH` | `/{id}/complete` | Menyelesaikan = tanda tangan pada registrasi yang sama; `LockedUnsigned` → `409` | `DoctorConsultation : Update` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah konsep kajian; `LockedUnsigned` → `409` | `PatientAssessment : Update` |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian pada registrasi yang sama; `LockedUnsigned` → `409` | `PatientAssessment : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–5 | Seluruhnya terpetakan ke source | `PASS` | Tabel bagian 6 |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang pada seluruh berkas berubah | Nol ambiguitas; nol `using` hilang pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` (perintah ringan pemilik: `-m:1`, tanpa shared compilation, tanpa analyzer) | Build ke-5 pada 17 September 2026: **0 error, 212 warning** | `PASS` | Bagian 8 |
| Uji idempoten dua kiriman simpan otomatis berturut-turut (runtime) | Tidak dijalankan | `NOT RUN` | Idempotensi dibuktikan secara statis: `PATCH /soap` tidak memanggil `RegisterAsync`; `RegisterAsync` memeriksa basis data dan `ChangeTracker` |
| Verifikasi proses bisnis runtime | Tidak dijalankan | `NOT RUN` | Menunggu build dan lingkungan uji |

Uji manual: `NOT FEASIBLE` pada sesi ini (tanpa build).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Konsep SOAP dan kajian medis rawat inap membentuk tepat satu registrasi `Draft` sejak simpan pertama | Terpenuhi (statis) | `DoctorConsultationController.CreateConsultation` dan `PatientAssessmentController` blok `BE-RWI-091` memanggil `RegisterAsync` sebelum `SaveChanges`/`Commit` |
| 2. Simpan otomatis tidak menggandakan registrasi; `RegisterAsync` idempoten | Terpenuhi (statis) | `PATCH /soap` tanpa pendaftaran; `RegisterAsync` memeriksa `FindAsync` lalu `ChangeTracker` |
| 3. Tanda tangan memakai registrasi yang sama | Terpenuhi (statis) | `RegisterSignedAsync` → `RegisterAsync` mengembalikan baris yang ada, lalu `Draft → Signed` |
| 4. Penulis menyelesaikan konsep setelah penugasan berakhir | Terpenuhi (statis) | Jalur Selesai memakai `EnsureSoleAuthorAsync` → `ResolveForAuthorEditAsync` (tanpa syarat penugasan aktif) |
| 5. Waktu klinis dan waktu tanda tangan terpisah | Terpenuhi (statis) | `ClinicalDateTime` pada dokumen; `SignedAt` pada `MrcClinicalDocumentIntegrity` |
| Laporan merujuk `RWI-DEC-151` | Terpenuhi | Metadata dan preflight |
| `dotnet build` | Terpenuhi | `PASS` 17 September 2026 — bagian 8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Keunikan `(DocumentKind, DocumentId)` pada basis data tetap usulan kepada pemilik `MedicalRecordManagement` (`INT-DOK-15`); dua transaksi paralel pada dokumen yang sama secara teoretis masih dapat membuat dua baris |
| Masalah yang diketahui | Kontrak 12.2 menyebut `Idempotency-Key` wajib pada `POST` catatan dokter rawat inap; kolomnya tidak ada di kamus data dan R2 kode saja, sehingga tidak dikerjakan. Dicatat sebagai delta kontrak |
| Risiko tersisa | Build lolos; bukti runtime (uji API dan proses bisnis) belum ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan gabungan pada jawaban sesi; berkas task ini berstatus `M` |
| Langkah berikutnya | Uji `409` pada konsep `LockedUnsigned` setelah penutupan episode |

## 8. Verifikasi susulan — 17 September 2026

Atas permintaan pemilik, build dan migration dijalankan setelah laporan ini ditulis. Perintah build
yang dipakai persis perintah pemilik:
`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`.

| Langkah | Hasil | Tindakan |
| --- | --- | --- |
| Build ke-1 | Gagal dalam 12 detik — `CS1002` pada `ApplicationDbContextModelSnapshot.cs`: relasi `ConsultationId` milik `TrxPatientProcedure` kehilangan `;` (sisa suntingan `BE-RWI-097`) | `;` ditambahkan |
| Build ke-2 | Pemeriksaan tipe penuh: **1 error** — `CS1931` pada `CpptVerificationService.cs`: variabel query `episode` bentrok dengan variabel lokal `episode` (kode `BE-RWI-096`) | Variabel query diganti nama `episodeBerjalan`, logika tidak berubah |
| Build ke-3 | 0 error. `dotnet ef` menolak model: FK `SupersedesDecisionId` (merujuk tabelnya sendiri) dan FK `ReconciliationItemId` pada `PhmMedicationReconciliationDecision` bernama sama setelah dipotong 63 karakter — EF Core tidak memberi akhiran unik pada FK yang merujuk tabelnya sendiri. **Galat ini juga akan membuat aplikasi gagal saat `DbContext` pertama dipakai** | Nama constraint eksplisit `FK_PhmMedicationReconciliationDecision_SupersedesDecisionId` pada configuration, migration R5, dan snapshot |
| Build ke-4 | 0 error. Snapshot gagal dibaca EF: lima navigasi koleksi (`Decisions`, `Versions`, `Ranges`) ditulis di blok relasi, sebelum relasinya dideklarasikan | Dipindahkan ke bagian navigasi snapshot; diperiksa tanpa build dengan simulasi urutan navigasi (0 galat), perbandingan DDL snapshot vs model runtime di 632 tabel, dan SQL migration vs model runtime (0 temuan) |
| Build ke-5 | **0 error, 212 warning** (garis dasar 211) | — |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | — |
| `dotnet ef database update --no-build` ke `QuilvianNewDevHamzah` | `Done.` Delapan migration diterapkan: `20260916000000` s.d. `20260916007000` (termasuk R3, R7 milik task sebelumnya, lalu R4, R5, R6, R8) | `migrations list` sesudahnya: nol `Pending` |

**Yang masih `NOT RUN`:** uji kontrak API dan proses bisnis runtime, uji jalur mundur migration
(`Down`), serta regresi yang disyaratkan kartu roadmap. Database yang disentuh hanya
`QuilvianNewDevHamzah` milik pemilik; database tim, staging, dan production tidak disentuh.
