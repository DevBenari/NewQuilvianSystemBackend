# Laporan Perubahan Backend — `BE-IGD-083`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-083` |
| Judul | Kunjungan IGD dipromosikan ke `Disposed` saat tindak lanjut dilaksanakan di luar IGD (_admit_ Rawat Inap), lalu tertutup lewat mekanisme yang sudah ada |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 — perbaikan cacat B6-06 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18, kartu `BE-IGD-083` |
| Requirement | `FR-IGD-113` (`AT-IGD-237`), `FR-IGD-120` (`AT-IGD-240`); DoD PRD §12.7 butir 13 |
| Keputusan | `IGD-DEC-323` (sejalur dengan blueprint Rawat Inap), `IGD-DEC-324` (delta matriks state), `IGD-DEC-308`, `304`, `311`, `106`; fakta `IGD-FACT-128`, `130`…`132` |
| Contract version | State **`0.15.0`** §9.2 baris 5 dan §11.8; validation **`0.19.0`** §6 aturan 1, 3a dan §14.10 aturan 32; API **`0.19.0`** §12.8 (tidak berubah) — `approved` `IGD-DEC-324`, manifest bagian 0s |
| Dependency | `BE-IGD-074` ✅, `BE-IGD-078` ✅ |
| Klasifikasi | `MEDIUM` — lima berkas di satu modul, jalur penutupan kunjungan yang dipakai lima titik pemicu; nol schema, nol endpoint baru |
| Task mode | `BACKEND` — atas perintah pemilik _"kerjakan sekarang"_ (Rizki Gunawan, 18 Oktober 2026) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): lima berkas di `Areas/HealthServices/EmergencyInstallationManagement/`, laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb18fe6` (`rizkiG`, ahead 1 dari origin) + working tree dokumen blueprint 18 Oktober 2026 |
| Tanggal | 18 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — implementasi selesai, QBE Strict `PASS`, build Rizki berhasil (0 error, 286 warning = baseline); menunggu Putaran B7.** Acceptance 4, 5, 8, 9 terpenuhi; 6 sebagian; 1, 2, 3, 7 menunggu B7 |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` — *Emergency*, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `TOUCHED LEGACY` — controller lama yang memakai `ApplicationDbContext` langsung tidak di-refactor; logika baru ditempatkan di service |
| Entity / migration baru | Tidak ada — QBE-MOD-002/003, QBE-ENT-001, QBE-CFG-001 tidak terpicu |
| QBE ID yang berlaku | QBE-SVC-001 (logika promosi dan penahan di service, bukan controller), QBE-API-001 (pola `409` + `ApiResponse` yang sudah ada), QBE-VAL-001 (invarian penutupan), QBE-TXN-001 (promosi ikut transaksi aksi pemicu), QBE-LOG-001 (log penutupan susulan yang sudah ada tetap memuat aktor), QBE-PERM-001 (nol endpoint baru, metadata akses tidak disentuh) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/TASK_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `TEST_POLICY.md`; `BACKEND_ENGINEERING_CONTRACT.md` (salinan suite dan repo identik); registry repo. Registry suite dan repo berbeda isi (selisih lama `ACC-DEP-007`, milik lead) — entri `Emg` ada di repo |
| Sisa folder yang dicabut | Tidak ditemukan `agents/rules/` maupun `.codex/` |

---

## 1. Masalah yang diperbaiki

Sejak `IGD-DEC-304`, tindak lanjut rawat inap tidak lagi dijalankan dari tombol _Jalankan_ IGD. Petugas admisi Rawat Inap
yang menjalankannya saat meng-_admit_ pasien dari Antrean Transfer IGD/Poli, dan saat itu tindak lanjut IGD menjadi
_Executed_ (`BE-RWI-207`). Tiga hal di IGD tidak ikut menyesuaikan:

1. **Kunjungan tidak pernah dipindahkan ke `Disposed`.** Langkah itu hanya ada di jalur _Jalankan_. Penjaga penutupan
   mensyaratkan `Disposed`, jadi kunjungan tidak pernah tertutup dan lencananya tetap "Menunggu keputusan" (`IGD-FACT-128`).
2. **_Catat Tiba_ tidak mencoba menutup kunjungan.** Hanya _Terima_/_Tolak Dokumen_, batal kepergian, dan sikap pesanan
   yang mencoba. Pada urutan _Terima Dokumen_ lalu _Catat Tiba_, penutupan dicoba saat pasien belum tiba lalu tidak pernah
   dicoba lagi (`IGD-FACT-130`).
3. **Fungsi evaluasi tidak melihat _Executed_ yang belum disimpan.** Panggilan Rawat Inap yang disepakati terjadi di
   transaksi _admit_ sebelum `SaveChangesAsync` (`IGD-REQ-003` butir 1), padahal fungsi IGD hanya mencari di basis data
   (`IGD-FACT-131`).

Bila hanya butir 1 yang diperbaiki, timbul masalah baru: begitu Rawat Inap memanggil evaluasi, kunjungan yang pasiennya masih
di IGD langsung tertutup saat _admit_ karena belum ada kepergian yang menahan (`IGD-FACT-132`).

_Contoh nyata (Putaran B6, 18 Oktober 2026)._ P-B6-A di-_admit_ pukul 08.00.47. Perawat IGD membuat kepergian dan mencatat
berangkat pukul 08.22; perawat ruangan menerima dokumen pukul 08.22.21 dan mencatat tiba pukul 08.22.26. Kunjungan tetap
`AwaitingDisposition` (`6`), tidak pernah `Completed`.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Kunjungan IGD pasien yang dipindahkan ke rawat inap tertutup sendiri saat pasien tiba di bangsal — tidak lebih cepat, tidak tertahan |
| Pelaku | Petugas admisi (Rawat Inap), perawat IGD, perawat ruangan, petugas yang menekan _Selesaikan_ |
| Aturan | State `0.15.0` §11.8 baris 1–3; validation `0.19.0` §14.10 aturan 32 |

**Alur normal — admisi dulu, lalu antar:**

1. Petugas admisi meng-_admit_ pasien. Tindak lanjut IGD menjadi _Executed_. Bila Rawat Inap sudah memasang panggilan
   evaluasi, kunjungan langsung dipromosikan ke `Disposed` tetapi **tetap terbuka**: pesan penahannya _"Pasien belum
   dicatat tiba di unit rawat inap."_ Tanpa panggilan itu, kunjungan tetap `AwaitingDisposition` sampai langkah 3.
2. Perawat IGD membuat kepergian SBAR, mengajukan serah terima, dan mencatat berangkat.
3. Perawat ruangan menekan _Terima Dokumen_. Penutupan dicoba: kunjungan dipromosikan ke `Disposed` bila belum, lalu
   tertahan _"Masih ada proses kepergian pasien yang belum selesai."_ Promosinya **disimpan**.
4. Perawat ruangan menekan _Catat Tiba_. Penutupan dicoba lagi (titik pemicu baru) dan berhasil: kunjungan `Completed`,
   encounter IGD ikut ditutup, log `EmergencyVisit.CompleteByDisposition` tercatat dengan aktor perawat ruangan.

Urutan langkah 3 dan 4 boleh terbalik; hasil akhirnya sama.

**Alur pasien tiba dulu, admisi kemudian:** kepergian dibuat, pasien dicatat tiba, dokumen diterima selagi tindak lanjut
masih _Dikonfirmasi_ — tidak ada yang ditutup. Saat _admit_, Rawat Inap memanggil evaluasi; fungsi IGD menemukan tindak
lanjut yang baru menjadi _Executed_ (walau belum disimpan), mempromosikan, lalu menutup kunjungan dalam penyimpanan _admit_
yang sama.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Petugas menekan _Selesaikan_ sesudah _admit_, sebelum pasien tiba | `409` _"Pasien belum dicatat tiba di unit rawat inap."_ (atau _"Masih ada proses kepergian pasien yang belum selesai."_ bila kepergiannya sudah dibuat); status kunjungan tidak berubah |
| Kunjungan masih `UnderObservation` dengan observasi aktif saat penutupan dicoba | Promosi ditolak penjaga; tidak ada yang berubah; alasan _"Masih ada observasi yang belum diselesaikan."_ |
| Kunjungan lama yang terlanjur tertahan (P-B6-A) | Ditutup lewat _Selesaikan_: dipromosikan, lalu `Completed`, karena kepergiannya sudah tiba |
| Tindak lanjut non-ranap lewat _Jalankan_ | Tidak berubah. Bila punya kepergian, kunjungan kini tertutup saat _Catat Tiba_, tidak menunggu _Terima Dokumen_ |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EmergencyVisitService.cs` (`CanTransition`, `TryApplyVisitStatus`, `TryCloseAfterDispositionAsync`,
`TryCloseAfterBlockerResolvedAsync`, `AmbilAlasanMenungguPenutupanAsync`), `EmergencyDispositionService.cs`
(`ValidateVisitClosureAsync`, konstruktor), `EmergencyDispositionService.AdmissionRequest.cs`
(`EvaluasiPenutupanKunjunganSusulanAsync`, `ButuhFormPermintaanRawatInapAsync`), `EmergencyDispositionController.cs`
(`UpdateDispositionStatus`), `EmergencyDepartureController.cs` (`Arrive`, `ExecuteDeparture`, `DenganPenutupanSusulanAsync`),
`EmergencyDepartureService.cs` (`ArriveAsync`, `UbahFisikAsync`, validasi buat kepergian), `EmergencyObservationController.cs`
(pemicu observasi), `EmergencyVisitController.cs` (`Complete`), `Models/EmgDisposition.cs`, `Enums/EmergencyVisitStatus.cs`,
`DTOs/EmergencyVisitDtos.cs`, `InpAdmissionTransferService.cs` dan `.BedManagement.cs` (baca saja), `Program.cs` (registrasi
DI, baca saja), `Filters/`, `Middlewares/`, `Attributes/` (tidak ada `SaveChanges` tersirat).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/EmergencyVisitService.cs` | (1) Method baru `TryPromoteToDisposed` — blok promosi bertahap `BE-IGD-078` yang dipindah dari controller, beserta komentar `IGD-DEC-311` apa adanya. (2) `PromosikanBilaTindakLanjutTerlaksanaAsync` (publik, dipakai _Selesaikan_) dan `PromosikanKeDisposedAsync` (privat): promosi; bila ditolak dan masih ada observasi aktif/dieskalasi, pesannya validation §6 aturan 2. (3) `HasilPenutupanSusulan` mendapat properti `Dipromosikan` (`init`), tanpa mengubah konstruktor posisional. (4) `TryCloseAfterDispositionAsync` mencari tindak lanjut lewat `CariTindakLanjutTerlaksanaAsync` dan mempromosikan kunjungan yang belum `Disposed` sebelum penjaga penutupan. (5) `TryCloseAfterBlockerResolvedAsync` menyimpan bila ditutup **atau** dipromosikan. +67/−12 |
| `Services/EmergencyDispositionService.cs` | (1) Konstanta `PesanObservasiBelumSelesai` dan `PesanPasienBelumTibaDiRawatInap`. (2) `AdaObservasiBelumSelesaiAsync` — kueri observasi yang dipindah dari `ValidateVisitClosureAsync` supaya aturannya tetap satu. (3) `CariTindakLanjutTerlaksanaAsync` — tindak lanjut _Executed_ terbaru, lebih dulu dari entity yang dilacak `DbContext` (`DbSet.Local`), baru dari basis data. (4) Penahan aturan 32 (`MenungguPasienTibaDiRawatInapAsync`) diperiksa sesudah aturan 3. +69/−11 |
| `Controllers/EmergencyDispositionController.cs` | Jalur _Jalankan_ memanggil `TryPromoteToDisposed`; urutan, pesan `409`, dan pengecualian pengulangan tidak berubah. +2/−12 |
| `Controllers/EmergencyDepartureController.cs` | `Arrive` memakai jalur penutupan susulan yang sama dengan _Terima Dokumen_ (`cobaPenutupanSusulan: true`). +1/−1 |
| `Controllers/EmergencyVisitController.cs` | `Complete` mempromosikan kunjungan bertindak lanjut _Executed_ sebelum penjaga penutupan; `now` dan `actorUserId` dipindah ke atas. Penolakan apa pun tetap `409` tanpa penyimpanan. +8/−3 |

Total: 5 berkas, **+147 / −39** (`git diff --stat`). Nol berkas Rawat Inap, nol `Program.cs`, nol migration, nol DTO, route,
atau enum baru. Komentar: 4 baris dipindah, nol baris komentar baru. Akhiran baris kelima berkas tetap CRLF tanpa BOM
(dihitung lewat Node).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk permintaan dan respons tidak berubah. `PATCH /emergency-visits/{id}/complete` dapat mengembalikan satu pesan `409` baru (aturan 32), dan kini `200` untuk kunjungan bertindak lanjut _Executed_ yang sebelumnya ditolak aturan 1. `POST /emergency-departures/{id}/arrive` kini dapat menutup kunjungan sebagai efek samping, sama seperti `accept-handover` |
| Database | `NOT APPLICABLE` — nol schema, nol migration. Data lama tidak diubah; kunjungan yang terlanjur tertahan dirapikan lewat _Selesaikan_ (B7-05) |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint baru; `[AccessAction]`/`[AccessPermission]` tidak disentuh; tidak ada pemeriksaan peran yang di-hardcode |

---

## 4. Dokumentasi endpoint

Nol endpoint baru. Perilaku dua endpoint yang sudah ada berubah:

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}/complete` | Menyelesaikan kunjungan. **Baru:** bila tindak lanjutnya sudah _Executed_ tetapi kunjungan belum `Disposed`, kunjungan dipromosikan lebih dulu; tindak lanjut rawat inap ditolak `409` _"Pasien belum dicatat tiba di unit rawat inap."_ selama belum ada kepergian yang tiba | `EmergencyVisit : Update` |

#### Health Services / Emergency Installation Management / Emergency Departure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-departures/{id}/arrive` | Mencatat pasien tiba di unit tujuan. **Baru:** sesudah tersimpan, penutupan kunjungan dicoba dalam transaksi yang sama | `EmergencyDeparture : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <lima berkas>` | Files evaluated 5; VIOLATION 0, REVIEW 0, INFO 0; Final result `PASS` | `PASS` | Keluaran perintah, 18 Oktober 2026 |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (working tree) | Files evaluated 5; VIOLATION 0, REVIEW 0, INFO 0; `PASS` | `PASS` | Keluaran perintah |
| Review diff dan scope | Hanya lima berkas Cakupan; tidak ada berkas `.cs` lain berubah | `PASS` | `git diff --stat`, `git status --short` |
| Review nama member | Nama baru tidak bentrok dengan berkas `partial` `EmergencyDispositionService.AdmissionRequest.cs` | `PASS` | Pencarian nama di `Areas/` |
| Penyimpanan tersirat pada `409` | Tidak ada filter, middleware, atau atribut yang memanggil `SaveChanges`; promosi di memori pada _Selesaikan_ yang ditolak tidak tersimpan | `PASS` | Pencarian di `Filters/`, `Middlewares/`, `Attributes/` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental` (Rizki) | `Build succeeded with 286 warning(s)` dalam 724,0 detik; 0 error. 286 sama dengan baseline `BE-IGD-078`/`079`, jadi nol warning baru. DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 18 Oktober 2026 10.51.30 WIB, sesudah suntingan source terakhir 09.54 | `PASS` | Tangkapan layar keluaran build dari Rizki; jam berkas DLL |
| Putaran B7 bagian 1 (B7-00…05) | Belum dijalankan — menunggu build | `NOT RUN` | Panduan uji sisa §3D |
| Putaran B7 bagian 2 (B7-06, 07) | Belum dijalankan — menunggu panggilan Rawat Inap (`IGD-REQ-003` butir 1) | `NOT RUN` | — |

Uji manual: `REQUIRED` — Putaran B7.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:** uji API dan layar (Putaran B7, Antigravity); kueri basis data apa pun.

**Langkah build untuk Rizki** (jalur ringan, dari folder backend):

```powershell
dotnet build-server shutdown
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false -m:1 -nodeReuse:false --no-incremental
```

`--no-incremental` dipakai supaya jumlah warning yang dilaporkan adalah angka sebenarnya.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Alur normal tanpa panggilan Rawat Inap, urutan B6-06 persis → kunjungan `Completed`; `visitCompletedAt` terisi; log `EmergencyVisit.CompleteByDisposition` pada _Catat Tiba_ memuat `DispositionId` dan `EncounterDitutup` `true` (`AT-IGD-237`) | Belum terpenuhi — source ada (perubahan 3.2 baris 1, 4) | Menunggu B7-03 |
| 2 | Sesudah _Terima Dokumen_ dan sebelum _Catat Tiba_, `visitStatus` `7`; lencana bukan "Menunggu keputusan" | Belum terpenuhi — source ada (`Dipromosikan` + simpan di `TryCloseAfterBlockerResolvedAsync`) | Menunggu B7-03 |
| 3 | `PATCH …/complete` sebelum pasien tiba → `409` aturan 32 (atau aturan 3), status tidak berubah | Belum terpenuhi — source ada (`ValidateVisitClosureAsync`, `Complete`) | Menunggu B7-02 |
| 4 | Evaluasi dalam `DbContext` yang sama sebelum `SaveChangesAsync` mengenali _Executed_ yang belum disimpan dan mempromosikan; pasien tiba dulu → `Completed`; alur normal → `Disposed` dan tetap terbuka | **Terpenuhi lewat diff** | `CariTindakLanjutTerlaksanaAsync` membaca `DbSet.Local` lebih dulu; `EvaluasiPenutupanKunjunganSusulanAsync` meneruskan ke `TryCloseAfterDispositionAsync` (tidak berubah); penahan aturan 32 memakai pencarian yang sama. Runtime B7-06/07 menjadi bukti `BE-IGD-077` #4 |
| 5 | Promosi selalu lewat `TryApplyVisitStatus`; ditolak → tidak ada perubahan, alasan jelas; nol penulisan status di jalur baca | **Terpenuhi lewat diff** | `TryPromoteToDisposed` hanya memanggil `TryApplyVisitStatus`; langkah pertama hanya dari `Triaged`/`InTreatment`, sehingga penolakan langkah kedua tidak meninggalkan perubahan; `AmbilAlasanMenungguPenutupanAsync` dan `SaringMenungguPenutupan` tidak disentuh |
| 6 | Jalur _Jalankan_ IGD tidak berubah | Sebagian — diff terpenuhi, regresi runtime belum | Blok dipindah utuh; kunjungan sudah `Disposed` di memori saat `TryCloseAfterDispositionAsync` dipanggil, jadi promosi tidak berjalan dua kali; aturan 32 tidak menyentuh jenis tanpa `RequiresAdmissionRequest`. Menunggu B7-04 |
| 7 | P-B6-A dirapikan lewat `PATCH …/complete` → `200` | Belum terpenuhi — source ada | Menunggu B7-05 |
| 8 | Diff hanya berkas Cakupan; nol Rawat Inap, `Program.cs`, migration, DTO/route/enum; nol komentar baru; CRLF tanpa BOM; QBE `PASS` | **Terpenuhi** | Bagian 3.2 dan 5 |
| 9 | Build 0 error; jumlah warning dilaporkan | **Terpenuhi** | Build Rizki 18 Oktober 2026: 0 error, 286 warning (baseline) |

**DoD.** Delta masuk kontrak sebelum implementasi — terpenuhi (`IGD-DEC-324`, manifest 0s). Laporan ini — ada. Register,
grafik R3.18.2, dan traceability — diperbarui ke 🟡. **Belum:** acceptance 1, 2, 3, 6 (runtime), 7 — semuanya Putaran B7 bagian 1.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada dari checker. Build: 286 warning, sama dengan baseline (antara lain CS1573 pada `Filters/AccessPermissionFilter.cs`, di luar task ini) |
| Masalah yang diketahui | (1) Kunjungan `UnderObservation` yang observasinya dibatalkan (bukan diselesaikan) tetap tidak dapat dipromosikan — perilaku penjaga yang sudah ada, sama seperti jalur _Jalankan_. (2) Cabang cadangan `EvaluasiPenutupanKunjunganSusulanAsync` tanpa `IServiceProvider` tidak diubah; tidak terpakai di runtime DI. (3) Daftar "menunggu penutupan" tetap menampilkan alasan aturan 1 untuk kunjungan yang belum dipromosikan, karena jalur baca tidak menulis status |
| Risiko tersisa | Perubahan perilaku di luar transfer: kunjungan non-ranap yang punya kepergian kini tertutup saat _Catat Tiba_ (disetujui `IGD-DEC-324`). Satu sampai tiga kueri tambahan per penjagaan penutupan kunjungan `Disposed` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lima berkas source di atas `M`; dokumen blueprint IGD `M`/`??` dari pass blueprint hari yang sama; `Storage/uploads/patient-qrcodes/` `??` (data runtime, bukan dari task ini). Tidak ada stage, commit, atau push |
| Langkah berikutnya | (1) Rizki: build backend — **selesai** 18 Oktober 2026 10.51. (2) Antigravity: Putaran B7 bagian 1 (prompt A.1d panduan uji sisa). (3) Agent: penilaian B7, lalu kartu ini ✅ bila lulus. (4) Sesudah Rawat Inap memasang panggilan `IGD-REQ-003` butir 1: B7 bagian 2 |
