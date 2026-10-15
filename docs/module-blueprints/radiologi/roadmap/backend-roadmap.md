# Backend Roadmap — Modul Radiologi

| Field | Value |
|---|---|
| Roadmap ID | `RAD-RM-BE-001` |
| Revision | `13` — katalog Radiologi v1 disalin ke devYoga (481 pemeriksaan, 450 tarif), 2026-10-15. Sebelumnya `12` — migration `AddRadReportTemplateAndFilmPrint` diterapkan, izin Kepala Instalasi/Radiografer diberikan, 12 aturan keselamatan disahkan, 2026-10-15. Sebelumnya `11` — `BE-RAD-24`/`25`/`26` dibuka dan dibangun atas instruksi pemilik modul (fitur v1 bertanda yang dapat dibangun), 2026-10-15. Sebelumnya `6` — `RAD-DEC-021`/`022` (ikut v1): `BE-RAD-20` selesai, `BE-RAD-21` dibatalkan, `BE-RAD-23` tanpa backend, `BE-RAD-22` bertanda belum tersedia, 2026-10-15. Sebelumnya `5` — `BE-RAD-19` selesai; `BE-RAD-21` dan `BE-RAD-22` tertahan keputusan, 2026-10-15. Sebelumnya `4` — paritas v1 (`RAD-EVD-001`): `BE-RAD-18`..`BE-RAD-23` dibuka, `BE-RAD-18` selesai, 2026-10-15. Sebelumnya `3` — `BE-RAD-17` dibuka dan selesai, 2026-10-14 |
| Status | `approved` |
| Blueprint | `RAD-BP-001` revision 9, status `approved` |
| Backend SHA baseline | `0e2eb105` |
| Kontrak terkunci | `RAD-ARCH-BE-001`, `RAD-API-001`, `RAD-STATE-001`, `RAD-VAL-001`, `RAD-INT-001`, `RAD-PERM-001`, `RAD-TEST-001` — seluruhnya `approved` 2026-09-10 |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 2026-09-10 |
| Tanggal | 2026-09-10 |

> **Batas dokumen ini.** Roadmap memecah pekerjaan menjadi task kecil yang dapat diuji. Ia
> **bukan** izin menulis kode. Setiap task memerlukan wewenang `TASK MODE: BACKEND` tersendiri
> saat dikerjakan.
>
> Pembuatan migration dan penjalanannya terhadap database mana pun tetap merupakan wewenang
> terpisah, walaupun task-nya sudah tercantum di sini.

---

## 0. Ringkasan Task Backend dan Status — per 2026-10-14

> **Mulai baca status dari sini.** Status diambil dari laporan di
> [`task/report/backend/`](../task/report/backend/) lalu dicocokkan dengan source backend branch
> `yoga` per 2026-10-14. Rincian tiap task tetap ada pada blok task dan laporannya.

**Arti status:**

| Status | Arti |
|---|---|
| **Selesai** | Kode selesai dan DoD terbukti |
| **Selesai — batas verifikasi** | Kode dan build selesai. Satu butir pembuktian masih menunggu lingkungan, akun, atau data. Kodenya tidak perlu disentuh lagi |
| **Selesai sebagian — sisa non-kode** | Bagian kode selesai. Sisa DoD menunggu keputusan atau tindakan di luar tim rekayasa |

### 0.1 Angka pokok

| Golongan | Jumlah | Task |
|---|---:|---|
| Nomor task backend | **26** | `BE-RAD-01`..`BE-RAD-15` dari roadmap (15), `BE-RAD-16` dan `BE-RAD-17` dari instruksi pemilik modul 2026-09-14 dan 2026-10-14 (2), `BE-RAD-18`..`BE-RAD-23` dari paritas v1 `RAD-EVD-001` (6), `BE-RAD-24`..`BE-RAD-26` dari fitur bertanda `RAD-DEC-022` (3) |
| **Selesai** | **24** | Seluruh nomor lain, termasuk `BE-RAD-23` (tanpa kode backend) dan `BE-RAD-18`/`19`/`20` (2026-10-15: kedua migration diterapkan ke devYoga atas instruksi pemilik modul, riwayat 318 → 320; GET terhadap devYoga 10/10) |
| Selesai — batas verifikasi | **0** | — (`BE-RAD-24`..`26` ditutup 2026-10-15: migration diterapkan, GET devYoga 10/10; `BE-RAD-16` ditutup oleh `RadReportPendingTests`) |
| Selesai sebagian — sisa non-kode | **1** | `BE-RAD-15` |
| Dibatalkan | **1** | `BE-RAD-21` — v1 tidak punya kunci Lunas (`RAD-EVD-001` 7.1) |
| **Siap dikerjakan** | **0** | — |

24 + 1 + 1 = **26** (`BE-RAD-22` dan `BE-RAD-16` masuk *Selesai* 2026-10-15; `BE-RAD-24`..`26` dibuka dan dibangun 2026-10-15).

### 0.2 Seluruh task menurut gelombang

| Task | Judul | Gelombang | Status | Catatan |
|---|---|---|---|---|
| `BE-RAD-01` | Siklus pengesahan pada aturan keselamatan | `MVP-0` | **Selesai** | Migration diterapkan ke `QuilvianNewDevYoga` 2026-09-10 — [laporan](../task/report/backend/BE-RAD-01.md) |
| `BE-RAD-02` | Service siklus pengesahan | `MVP-0` | **Selesai** | [laporan](../task/report/backend/BE-RAD-02.md) |
| `BE-RAD-03` | Endpoint aturan keselamatan | `MVP-0` | **Selesai** | [laporan](../task/report/backend/BE-RAD-03.md) |
| `BE-RAD-04` | Kelola alat pencitraan | `MVP-0` | **Selesai** | [laporan](../task/report/backend/BE-RAD-04.md) |
| `BE-RAD-05` | Kelola butir keselamatan | `MVP-0` | **Selesai** | [laporan](../task/report/backend/BE-RAD-05.md) |
| `BE-RAD-06` | Gerbang keselamatan menilai `RuleStatus` | `MVP-0` | **Selesai** | [laporan](../task/report/backend/BE-RAD-06.md) |
| `BE-RAD-15` | Rencana dan pengisian data master awal | `MVP-1` | **Selesai sebagian — sisa non-kode** | Enam alat, empat butir, dan dua belas draf aturan terisi. "Setiap alat punya aturan aktif" menunggu pengesahan klinis (`DEC-RAD-005`) dan penunjukan pengaju aturan (`RAD-OPEN-012`) — [laporan](../task/report/backend/BE-RAD-15.md) |
| `BE-RAD-07` | Model hasil bacaan berversi | `MVP-2` | **Selesai** | Migration `AddRadReport` ikut terapan devYoga 2026-10-14 (nol migration tertunda) — [laporan](../task/report/backend/BE-RAD-07.md) |
| `BE-RAD-08` | Service penulisan dan pengesahan bacaan | `MVP-2` | **Selesai** | Penghalang `RadReport : ActAsRadiologist` ternyata sudah tertutup sejak 2026-09-11 (`BE-RAD-16` bagian 1.1); penanda melekat pada posisi Dokter Radiologi (`RAD-DEC-017`) — [laporan](../task/report/backend/BE-RAD-08.md) |
| `BE-RAD-09` | Endpoint hasil bacaan | `MVP-2` | **Selesai** | [laporan](../task/report/backend/BE-RAD-09.md) |
| `BE-RAD-10` | Koreksi berversi | `MVP-2` | **Selesai** | [laporan](../task/report/backend/BE-RAD-10.md) |
| `BE-RAD-11` | Penyajian hasil ke rekam medis | `MVP-4` | **Selesai** | Dilengkapi `BE-RAD-16` (`by-patient`, `Impression`) — [laporan](../task/report/backend/BE-RAD-11.md) |
| `BE-RAD-12` | Penanda cito pada pesanan | `MVP-4` | **Selesai** | Migration `AddRadOrderUrgency` ikut terapan devYoga 2026-10-14 (nol migration tertunda) — [laporan](../task/report/backend/BE-RAD-12.md) |
| `BE-RAD-13` | Daftar kerja per alat | `MVP-4` | **Selesai** | [laporan](../task/report/backend/BE-RAD-13.md) |
| `BE-RAD-14` | Uji kontrak hak akses radiologi | Lintas | **Selesai** | Nol cacat hak akses pada kelima controller — [laporan](../task/report/backend/BE-RAD-14.md) |
| `BE-RAD-16` | Kelahiran bacaan otomatis dan penyajian hasil ke rekam medis | di luar roadmap | **Selesai** | `EnsurePendingReportAsync` kini dipanggil saat mutu citra diterima; `RAD-API-001` r11. Batas verifikasi ditutup 2026-10-15: `RadReportPendingTests` (3 uji — `Pending` lahir, idempoten, ditolak bila mutu belum dinilai/ditolak) — [laporan](../task/report/backend/BE-RAD-16.md) |
| `BE-RAD-17` | Empat transisi study: tandai perlu diulang, tahan, lanjutkan, batalkan | susulan, bagian 6b | **Selesai** | Dibuka atas instruksi pemilik modul 2026-10-14; 34/34 uji lulus; HTTP lokal 5/5 tanpa tulis. Hak akses `RadStudy : Hold`/`Cancel` belum diberikan ke jabatan mana pun — [laporan](../task/report/backend/BE-RAD-17.md) |
| `BE-RAD-18` | Kolom, tab, dan penyaring Daftar Pasien v1 pada `GET /rad-orders` | paritas v1, bagian 6c | **Selesai — batas verifikasi** | 41/41 uji Radiologi lulus (7 baru). Migration `AddRadOrderRequiresPreparation` **tertunda** di devYoga — menunggu instruksi — [laporan](../task/report/backend/BE-RAD-18.md) |
| `BE-RAD-19` | Rekap Beranda Radiologi | paritas v1, bagian 6c | **Selesai — batas verifikasi** | `GET /rad-orders/dashboard`; 46/46 uji Radiologi (5 baru) — [laporan](../task/report/backend/BE-RAD-19.md) |
| `BE-RAD-20` | Petugas Radiologi dan Dokter Pemeriksa per pesanan | paritas v1, bagian 6c | **Selesai — batas verifikasi** | Ikut v1 (`RAD-DEC-021`); endpoint assignment + dua endpoint pilihan; WhatsApp dan tarif di baris daftar; 53/53 uji — [laporan](../task/report/backend/BE-RAD-20.md) |
| `BE-RAD-21` | Kunci Lunas sebelum proses | paritas v1, bagian 6c | **Dibatalkan** | v1 tidak mengunci: `ubahStatusPembayaran` tak pernah dipanggil, pembayaran hanya kolom tampilan |
| `BE-RAD-22` | Unggah gambar dan jumlah film | paritas v1, bagian 6c | **Selesai** — migration diterapkan ke devYoga, `GET /images` 200 | `RadOrderImage` + 4 endpoint, penyimpanan privat; 60/60 uji. Jumlah film = bahan terpakai `Film` — [laporan](../task/report/backend/BE-RAD-22.md) |
| `BE-RAD-23` | Kirim hasil lewat WhatsApp | paritas v1, bagian 6c | **Selesai — tanpa backend** | Ikut v1: `wa.me` dari peramban; nomor dari `PatientPhoneNumber` (`BE-RAD-20`) |
| `BE-RAD-24` | Laporan operasional dan Tarif Tindakan (menu v1 Laporan/Pemeriksaan/Setup) | fitur bertanda, bagian 6c | **Selesai** | 6 endpoint `GET /rad-orders/reports/*`, tanpa tabel baru; 71/71 uji — [laporan](../task/report/backend/BE-RAD-24.md) |
| `BE-RAD-25` | Ubah Pemeriksaan dan Cetak Film | fitur bertanda, bagian 6c | **Selesai** | `PUT /{id}/procedure`, `PUT /{id}/film-print`; kolom `RadOrder.PrintFilm` — [laporan](../task/report/backend/BE-RAD-25.md) |
| `BE-RAD-26` | Template Hasil | fitur bertanda, bagian 6c | **Selesai** | Tabel `RadReportTemplate` + CRUD `/rad-reports/templates` — [laporan](../task/report/backend/BE-RAD-26.md) |

### 0.3 Yang masih terbuka di luar task bernomor

Per 2026-10-15 **tidak ada pekerjaan kode Radiologi yang terbuka**. Seluruh sisa di bawah menunggu tindakan atau
keputusan di luar tim rekayasa, urut menurut dampaknya pada pemakaian:

1. ~~**Aturan keselamatan aktif**~~ — **12 aturan disahkan 2026-10-15** atas instruksi pemilik modul: diajukan Bayu
   Ramadhan (Radiografer), disahkan dr. Arya (Kepala Instalasi Radiologi) — `AC-13` terpenuhi, penyusun/pengaju ≠
   pengesah. **Sisa:** tiga alat — DX (Digital Radiography), XA (X-Ray Angiography), NM (Nuclear Medicine) — belum
   punya aturan sama sekali, sehingga pemeriksaan pada ketiganya tetap ditolak (*fail-closed*). Isi aturannya keputusan
   klinis pemilik modul.
2. ~~**Hak akses jabatan**~~ — **diberikan 2026-10-15** atas persetujuan pemilik modul: *Kepala Instalasi Radiologi*
   0 → 16 izin (termasuk `RadSafetyRule` Approve/Reject/Deactivate, `RadReport` Validate/Release/Amend/
   `ActAsRadiologist`); *Radiografer* 13 → 22 (termasuk `RadSafetyRule` Create/Update/Submit). Set lama dikirim utuh;
   tidak ada jabatan yang memegang Create dan Approve sekaligus.
3. ~~**Katalog pemeriksaan dan tarif Radiologi kosong**~~ — **diisi 2026-10-15** atas instruksi pemilik modul, disalin dari
   v1 tayang (`LabPemeriksaan`, `namaLab` = Radiologi) lewat API aplikasi: 523 baris v1 → **481 pemeriksaan unik** (42
   duplikat identik dibuang; X-Ray 142, CT Scan 88, MRI 207, USG 44; grup `Radiologi`, tipe `Radiology`, kode v1 di
   `ExternalProcedureCode`) dan **450 tarif** kategori `RADIOLOGY`, semua kelas, berlaku 15 Okt 2026 — harga cocok
   v1 450/450. 31 pemeriksaan berharga 0 di v1 dibiarkan tanpa tarif ("Belum diatur"). Katalog v1 memuat butir yang
   bukan pemeriksaan pencitraan ("OUTSIDE TEST", jasa dokter Cathlab/Operasi, film tambahan) — ikut disalin; perapiannya
   keputusan pemilik master data.
4. ~~**Migration `AddRadReportTemplateAndFilmPrint`**~~ — **diterapkan 2026-10-15** atas instruksi pemilik modul (uji
   kering naik→turun→`ROLLBACK`, lalu `COMMIT`; riwayat 321 → 322, nol tertunda, `RadOrder` tak berubah). GET
   Radiologi terhadap devYoga 10/10 tanpa `42703`.
5. **Fitur v1 yang masih bertanda** "Fitur belum tersedia" (`RAD-DEC-022`): menu Setup → Kategori dan Parameter
   ("Coming Soon" di v1 tayang), tab Pasien Luar (data contoh di v1), Foto PACS (`RAD-OPEN-013`), AI
   (`RAD-OPEN-014`) — butuh sistem/penyedia luar atau modul lain. Tujuh menu Laporan/Pemeriksaan/Setup lainnya,
   Cetak Film, Ubah Pemeriksaan, dan Template Hasil **dicabut dari daftar 2026-10-15** (`BE-RAD-24`..`26`).

- ~~**Empat transisi study tanpa endpoint**~~ — **ditutup 2026-10-14** oleh `BE-RAD-17`.
- ~~**Batas verifikasi `BE-RAD-16`**~~ — **ditutup 2026-10-15** oleh `RadReportPendingTests` (63/63 uji Radiologi).

---

## 1. Ringkasan

15 task backend, dipecah menurut irisan vertikal — setiap task menghasilkan sesuatu yang dapat
diuji, bukan potongan lapisan yang baru berguna setelah semuanya selesai.

| Gelombang | Task | Epic | Hasil yang dapat diuji |
|---|---|---|---|
| `MVP-0` | `BE-RAD-01` s/d `BE-RAD-06` | `EPIC RAD-01` | Aturan keselamatan dapat disusun, disahkan, dan menentukan boleh-tidaknya pemeriksaan |
| `MVP-1` | `BE-RAD-15` | `EPIC RAD-01` | Data master awal terisi sehingga modul dapat dipakai |
| `MVP-2` | `BE-RAD-07` s/d `BE-RAD-10` | `EPIC RAD-02`, `RAD-03` | Hasil bacaan dapat ditulis, disahkan, dirilis, dan dikoreksi berversi |
| `MVP-4` | `BE-RAD-11` s/d `BE-RAD-13` | `EPIC RAD-04`, `RAD-07` | Hasil terbaca dari rekam medis; daftar kerja per alat berjalan |
| Lintas | `BE-RAD-14` | Seluruhnya | Hak akses terbukti tidak dapat bergeser diam-diam |

**Urutan yang tidak boleh dibalik.** `MVP-0` mendahului segalanya. Gerbang keselamatan bersifat
*fail-closed*: tanpa aturan yang dapat disahkan, tidak satu pun pemeriksaan berjalan, sehingga
tidak akan pernah ada citra layak untuk dibaca.

---

## 2. Gelombang `MVP-0` — Aturan Keselamatan

### `BE-RAD-01` — Siklus pengesahan pada aturan keselamatan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-001`, `FR-RAD-003` |
| Decision | `RAD-DEC-005`, `RJ-BIL-DEC-014` |
| Contract | `RAD-ARCH-BE-001` bagian 4.8 dan 7; `RAD-ERD-SAF-001` |
| Yang dikerjakan | Enum `RadSafetyRuleStatus`; enam kolom baru pada `MstRadModalitySafetyRule`; penyesuaian `MstRadModalitySafetyRuleConfiguration`; migration `AddRadSafetyRuleApprovalLifecycle` |
| Acceptance criteria | AC-12, AC-14, AC-16 pada decision log |
| Test | Migration dijalankan pada database berisi aturan yang berlaku; seluruh baris `IsActive = true` menjadi `RuleStatus = Active`; pemeriksaan tetap dapat berjalan |
| Dependency | Registry `Rad` berstatus `ACTIVE` — **sudah terpenuhi** 2026-09-10 |
| Risiko | **Tertinggi di seluruh roadmap.** Bila `RuleStatus` salah diisi bawaan, seluruh pemeriksaan langsung tertolak begitu migration dijalankan. Urutan wajib: isi kolom dulu, baru ubah filter index |
| Owner | Backend |
| Definition of Done | Migration dapat dijalankan dan dimundurkan; uji pengisian data lama lulus; tidak ada aturan yang berubah arti |
| **Keadaan 2026-09-10** | **SELESAI.** Source dan migration selesai, build lulus 0 error, migration **diterapkan** ke `QuilvianNewDevYoga` atas izin pemilik modul. `RadiologySafetyGateTests` 13 lulus. Sisa: jumlah baris per status belum terhitung karena tidak ada klien database. Laporan: `task/report/backend/BE-RAD-01.md` |

### `BE-RAD-02` — Service siklus pengesahan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-002`, `FR-RAD-003`, `FR-RAD-004` |
| Decision | `RAD-DEC-005`, `RAD-DEC-015` |
| Contract | `RAD-STATE-001` bagian 5; `RAD-VAL-001` bagian 2 |
| Yang dikerjakan | `RadSafetyPolicyService` — susun draf, ajukan, sahkan, tolak, nonaktifkan; penomoran versi; penjagaan tabrakan aturan aktif |
| Acceptance criteria | AC-13, AC-14, AC-15 |
| Test | Admin mengesahkan aturannya sendiri ditolak `403`; pengesahan menaikkan versi tepat satu kali; penolakan tanpa alasan ditolak `400`; pengesahan kedua untuk kombinasi sama ditolak `409` |
| Dependency | `BE-RAD-01` |
| Risiko | Pemisahan wewenang menyusun dan mengesahkan wajib ditegakkan di service, bukan hanya di atribut endpoint |
| Owner | Backend |
| Definition of Done | Seluruh transisi pada `RAD-STATE-001` bagian 5 terbukti; transisi tidak sah ditolak dengan kode yang benar |
| **Keadaan 2026-09-10** | **SELESAI.** `RadSafetyPolicyService` dengan enam tindakan; versi naik tepat satu kali pada pengesahan; pengesahan sendiri ditolak `403` dengan memeriksa penyusun **dan** pengaju. Build lulus 0 error; `RadSafetyPolicyServiceTests` **18 lulus** tanpa database. Yang **tidak** tercakup: pemeriksaan "pelakunya penanggung jawab klinis" tetap bersandar pada penanda `[AccessPermission("RadSafetyRule", "Approve")]` yang dipasang `BE-RAD-03`. Laporan: `task/report/backend/BE-RAD-02.md` |

### `BE-RAD-03` — Endpoint aturan keselamatan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-005` |
| Decision | `RAD-DEC-005` |
| Contract | `RAD-API-001` bagian 2 grup *Master Data / Rad Safety Rule*; `RAD-PERM-001` bagian 4 |
| Yang dikerjakan | `RadSafetyRuleController` — 8 endpoint, termasuk `GET /coverage` |
| Acceptance criteria | AC-17 |
| Test | `GET /coverage` menampilkan alat tanpa aturan aktif; string `[AccessPermission]` persis seperti kontrak |
| Dependency | `BE-RAD-02` |
| Risiko | Rendah |
| Owner | Backend |
| Definition of Done | Delapan endpoint tersedia sesuai kontrak; kode status sesuai `RAD-API-001` bagian 4 |
| **Keadaan 2026-09-10** | **SELESAI.** `RadSafetyRuleController` dengan **sebelas** endpoint — sembilan dari kontrak revision 3, ditambah `filters/metadata` dan `summary` yang diwajibkan standar endpoint. String `[AccessPermission]` cocok baris demi baris dengan `RAD-PERM-001` bagian 4. Build lulus 0 error; **42 uji radiologi lulus**, 15 di antaranya baru. `GET /coverage` hanya memuat alat yang belum tercakup, sehingga daftar kosong berarti siap dipakai — bentuk yang dituntut `BE-RAD-15`. Laporan: `task/report/backend/BE-RAD-03.md` |

### `BE-RAD-04` — Kelola alat pencitraan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-051` |
| Decision | `RAD-DEC-001` butir 14, `RAD-DEC-002` |
| Contract | `RAD-API-001` grup *Master Data / Rad Modality*; `RAD-VAL-001` bagian 3 |
| Yang dikerjakan | `RadModalityController` — CRUD. CRUD sederhana, memakai `ApplicationDbContext` langsung sesuai pola project |
| Acceptance criteria | AC-5 |
| Test | Kode alat kembar ditolak `409`; menonaktifkan alat yang masih dipakai aturan aktif ditolak `409` |
| Dependency | Tidak ada |
| Risiko | Rendah |
| Owner | Backend |
| Definition of Done | Alat baru dapat didaftarkan tanpa menyentuh database langsung |
| **Keadaan 2026-09-11** | **SELESAI.** `RadModalityController` dengan **sembilan** endpoint baseline master data — lima dari kontrak, ditambah `filters/metadata`, `summary`, `options`, dan `PATCH /{id}/status`. Kode alat kembar ditolak `409`, termasuk yang hanya berbeda huruf besar-kecil; alat yang masih dipakai aturan berlaku tidak dapat dinonaktifkan maupun dihapus. Build lulus 0 error; **61 uji radiologi lulus**, 19 di antaranya baru. Delta: akses data ditempatkan di service, **tidak** memakai `ApplicationDbContext` langsung seperti tertulis pada baris "Yang dikerjakan" — `QBE-SVC-001` mengalahkan pola legacy untuk `NEW CODE`. Laporan: `task/report/backend/BE-RAD-04.md` |

### `BE-RAD-05` — Kelola butir keselamatan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-051` |
| Decision | `RAD-DEC-005` |
| Contract | `RAD-API-001` grup *Master Data / Rad Safety Requirement` |
| Yang dikerjakan | `RadSafetyRequirementController` — CRUD |
| Acceptance criteria | Turunan AC-5 |
| Test | Kode butir kembar ditolak `409`; menonaktifkan butir yang masih dipakai ditolak `409` |
| Dependency | Tidak ada |
| Risiko | Rendah |
| Owner | Backend |
| Definition of Done | Butir keselamatan dapat dikelola lewat endpoint |
| **Keadaan 2026-09-11** | **SELESAI.** `RadSafetyRequirementController` dengan **sembilan** endpoint baseline master data. Kode butir kembar ditolak `409` termasuk yang berbeda huruf besar-kecil; butir yang masih dipakai aturan berlaku tidak dapat dinonaktifkan maupun dihapus, sedangkan butir yang hanya dipakai draf boleh. Kelompok butir diturunkan dari isinya, bukan daftar tetap. Build lulus 0 error; **78 uji radiologi lulus**, 17 di antaranya baru. Laporan: `task/report/backend/BE-RAD-05.md` |

### `BE-RAD-06` — Gerbang keselamatan menilai `RuleStatus`

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | `FR-RAD-001` |
| Decision | `RAD-DEC-005` |
| Contract | `RAD-STATE-001` bagian 5; `RAD-VAL-001` bagian 4 |
| Yang dikerjakan | Penyesuaian `RadSafetyGateEvaluator` dan `RadStudyService` agar hanya aturan `RuleStatus = Active` yang dinilai |
| Acceptance criteria | AC-12 |
| Test | Study dengan satu aturan `Draft` saja tetap ditolak; sifat fail-closed tidak berubah; dua berkas uji radiologi yang sudah ada tetap lulus |
| Dependency | `BE-RAD-01` |
| Risiko | **Menyentuh logika yang sudah berjalan.** Uji yang ada wajib tetap lulus tanpa diubah |
| Owner | Backend |
| Definition of Done | `RadiologySafetyGateTests` dan `RadiologyStudyLifecycleTests` lulus tanpa perubahan |
| **Keadaan 2026-09-10** | **SELESAI.** Penyaring gerbang, penilaian dalam `RadSafetyGateEvaluator`, dan penanda kesiapan alat kini memakai `RuleStatus = Active`. Build lulus 0 error; `RadiologySafetyGateTests` **20 lulus** (13 lama tanpa perubahan perilaku + 7 baru untuk AC-12). `RadiologyStudyLifecycleTests` **tidak dapat dijalankan** — `QUILVIAN_BILLING_TEST_DB` belum diisi, keadaan lingkungan yang sudah ada sebelum task ini. Satu penyimpangan dari Definition of Done dicatat: dua berkas uji **diubah pada data perancahnya**, bukan pada yang diuji. Laporan: `task/report/backend/BE-RAD-06.md` |

---

## 3. Gelombang `MVP-1` — Data Master Awal

### `BE-RAD-15` — Rencana dan pengisian data master awal

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-01` |
| Requirement | Definition of Done butir "Seluruh tabel master MVP sudah terisi" |
| Decision | `RJ-BIL-DEC-014`, `RAD-DEC-002`, `RAD-DEC-005` |
| Contract | `RAD-ARCH-BE-001` bagian 9 |
| Yang dikerjakan | Enam alat sesuai `RAD-DEC-002`; sekurang-kurangnya empat butir keselamatan; sekurang-kurangnya satu aturan `Active` untuk **setiap** alat yang dipakai |
| Acceptance criteria | AC-6, AC-17 |
| Test | `GET /coverage` mengembalikan daftar kosong — tidak ada alat tanpa aturan aktif |
| Dependency | `BE-RAD-03`, `BE-RAD-04`, `BE-RAD-05` |
| Risiko | **Isi awalnya belum ditetapkan klinis** (`DEC-RAD-005`). Usulan pemetaan pada `RAD-ARCH-BE-001` bagian 9 bersifat usulan, wajib diverifikasi terhadap SOP rumah sakit |
| Owner | Backend + penanggung jawab klinis |
| Definition of Done | Setiap alat punya aturan aktif; **termasuk USG**, yang butirnya tidak wajib tetapi tetap perlu satu baris aturan agar tidak tertolak fail-closed |
| **Keadaan 2026-09-11** | **SELESAI SEBAGIAN — sisanya bukan wewenang backend.** Seeder `RadiologyMasterDataSeeder` mengisi enam alat dan empat butir keselamatan, lalu menyusun dua belas usulan aturan **berstatus `Draft`** sesuai usulan baseline `DEC-RAD-005`. Tidak satu pun `Active`: aturan yang menentukan kapan pasien disinari hanya sah setelah disahkan penanggung jawab klinis. Build lulus 0 error; **88 uji radiologi lulus**, 10 di antaranya baru. **`GET /coverage` sengaja TETAP berisi enam alat** — dibuktikan menjadi kosong begitu drafnya disahkan. Penahan: `DEC-RAD-005` masih `OPEN`, pemilik tata kelola klinis. Laporan: `task/report/backend/BE-RAD-15.md` |

> **Jebakan yang paling mudah terlewat.** USG tidak punya satu pun butir keselamatan wajib.
> Karena gerbang bersifat fail-closed, USG tetap **membutuhkan** sedikitnya satu aturan `Active`
> walau aturan itu menandai butirnya tidak wajib. Tanpa satu baris pun, seluruh pemeriksaan USG
> akan tertolak.

---

## 4. Gelombang `MVP-2` — Hasil Bacaan dan Koreksi

### `BE-RAD-07` — Model hasil bacaan berversi

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-010`, `FR-RAD-014` |
| Decision | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003` |
| Contract | `RAD-ARCH-BE-001` bagian 4.9 dan 4.10; `RAD-ERD-REP-001`; `RAD-ERD-DICT-001` bagian 1, 2, dan 7 |
| Yang dikerjakan | Tiga enum `RadReport*`; model `RadReport` dan `RadReportVersion`; dua configuration; migration `AddRadReport` |
| Acceptance criteria | AC-18 |
| Test | Index unik satu bacaan per study berlaku; migration dapat dimundurkan |
| Dependency | Registry `Rad` `ACTIVE` — **sudah terpenuhi** |
| Risiko | Sedang. Dua tabel baru; **jangan** menambahkan kunci asing dari induk ke versi berlaku, karena melingkar |
| Owner | Backend |
| Definition of Done | Kedua tabel terbentuk sesuai DDL pada kamus data; tabel lain tidak tersentuh |
| **Keadaan 2026-09-11** | **SELESAI untuk source dan migration.** Tiga enum, dua model, dua configuration, dan migration `20260911025734_AddRadReport`. Build lulus 0 error; **121 uji radiologi lulus**, 15 di antaranya baru. Migration **dibuat, TIDAK dijalankan** — eksekusi database wewenang terpisah yang belum diberikan, sehingga butir "migration dapat dimundurkan" **belum terbukti**. Snapshot bertambah tepat dua entity, tidak ada yang hilang; hanya dua tabel baru yang disentuh. Laporan: `task/report/backend/BE-RAD-07.md` |

### `BE-RAD-08` — Service penulisan dan pengesahan bacaan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-011`, `FR-RAD-012`, `FR-RAD-013`, `FR-RAD-015` |
| Decision | `RAD-DEC-003`, `RAD-DEC-015` |
| Contract | `RAD-STATE-001` bagian 3 dan 4; `RAD-VAL-001` bagian 1; `RAD-PERM-001` bagian 5.1 dan 6 |
| Yang dikerjakan | `RadReportService` — tulis draf, ubah draf, sahkan, rilis; pengisian `AuthorRoleSnapshot` dari hak akses penulis; penjagaan pengesahan sendiri |
| Acceptance criteria | AC-1, AC-2, AC-3, AC-4 |
| Test | Tujuh baris tabel contoh pada `RAD-STATE-001` bagian 3 seluruhnya terbukti; residen yang kemudian menjadi radiolog tetap ditolak atas draf lamanya |
| Dependency | `BE-RAD-07` |
| Risiko | **Inti keselamatan modul.** Pemeriksaan `ActAsRadiologist` wajib di service, bukan hanya di atribut endpoint |
| Owner | Backend |
| Definition of Done | Seluruh transisi sah dan tidak sah pada `RAD-STATE-001` bagian 3 terbukti lewat uji |
| **Keadaan 2026-09-11** | **SELESAI untuk source dan uji, dengan satu penghalang terbuka.** `RadReportService` beserta `RadReportNumberService` dan `RadReportDtos`; pemeriksaan `ActAsRadiologist` dan penjagaan pengesahan sendiri berada di service. Build lulus 0 error; **162 uji radiologi lulus**, 41 di antaranya baru; seluruh 1.367 uji in-memory lulus. Tujuh baris tabel pengesahan `RAD-STATE-001` bagian 3 seluruhnya terbukti, termasuk residen yang kemudian menjadi radiolog tetap ditolak atas draf lamanya. **Penghalang**: penanda `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun karena `AccessMenuSeeder` hanya mendaftarkan pasangan yang menempel pada endpoint, sedangkan penanda ini sengaja tanpa endpoint — aturan `RAD-DEC-003` ada di kode tetapi belum berjalan di sistem sebenarnya. Perlu diselesaikan pada `BE-RAD-09`. **Belum tersambung**: `RadStudyService` belum memanggil kelahiran bacaan saat study menjadi `QualityAccepted`. Tidak ada perubahan schema, entity, maupun migration. Laporan: `task/report/backend/BE-RAD-08.md` |

### `BE-RAD-09` — Endpoint hasil bacaan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-010` s/d `FR-RAD-015` |
| Decision | `RAD-DEC-003` |
| Contract | `RAD-API-001` grup *Rad Report*; `RAD-PERM-001` bagian 3 |
| Yang dikerjakan | `RadReportController` — endpoint baca, tulis draf, ubah draf, sahkan, rilis |
| Acceptance criteria | AC-1 s/d AC-4 lewat jalur HTTP |
| Test | Kode status sesuai `RAD-API-001` bagian 4; `403` dan `422` sesuai matriks validasi |
| Dependency | `BE-RAD-08` |
| Risiko | Rendah |
| Owner | Backend |
| Definition of Done | Endpoint sesuai kontrak; pesan kesalahan memakai bahasa pada `RAD-VAL-001` |
| **Keadaan 2026-09-11** | **SELESAI, penghalang `BE-RAD-08` masih terbuka.** `RadReportController` dengan **sepuluh** endpoint — delapan dari kontrak, ditambah `filters/metadata` dan `summary` yang diwajibkan baseline `transaction-endpoint-standard.md`. `GET /{id}/versions` dan `POST /{id}/amendments` **sengaja tidak dibuat**, keduanya milik `BE-RAD-10`, dan ada ujinya yang memastikan tidak terbawa. Build lulus 0 error; **203 uji radiologi lulus**, 41 di antaranya baru; seluruh 1.408 uji in-memory lulus. AC-1 sampai AC-4 dibuktikan **lewat jalur HTTP**, bukan hanya di service. Kontrak diamandemen menjadi `RAD-API-001` rev 6 dan `RAD-PERM-001` rev 6 — **menunggu konfirmasi pemilik modul**. Delta: `POST /by-study/{id}/draft` menjawab `200`, bukan `201` seperti contoh `RAD-API-001` bagian 4, mengikuti seluruh endpoint create modul yang sudah berjalan; `RadReportValidateRequest` tidak dibuat karena tidak ada isian maupun kolom untuknya. **Penghalang tetap**: `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun. Laporan: `task/report/backend/BE-RAD-09.md` |

### `BE-RAD-10` — Koreksi berversi

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-03` |
| Requirement | `FR-RAD-020`, `FR-RAD-021`, `FR-RAD-022`, `FR-RAD-023` |
| Decision | `RJ-BIL-GATE-DEC-004` |
| Contract | `RAD-STATE-001` bagian 3 dan 4; `RAD-API-001` endpoint `/amendments` dan `/versions` |
| Yang dikerjakan | Amandemen berversi di `RadReportService`; endpoint `POST /{id}/amendments` dan `GET /{id}/versions` |
| Acceptance criteria | AC-18 sampai AC-21 |
| Test | Versi 1 **tidak berubah satu huruf pun** setelah amandemen; versi 1 menjadi `Superseded`; amandemen atas amandemen menghasilkan versi 3 |
| Dependency | `BE-RAD-09` |
| Risiko | **Tidak boleh ada satu pun jalur yang menimpa versi rilis.** Tidak ada endpoint hapus versi |
| Owner | Backend |
| Definition of Done | Riwayat versi utuh dan dapat ditelusuri mundur lewat `PreviousVersionId` |
| **Keadaan 2026-09-11** | **SELESAI.** `CreateAmendmentAsync` dan `GetVersionsAsync` beserta endpoint `POST /{id}/amendments` dan `GET /{id}/versions`; grup *Rad Report* kini **lengkap**. Build lulus 0 error; **233 uji radiologi lulus**, 30 di antaranya baru; seluruh 1.438 uji in-memory lulus. Versi lama terbukti tidak berubah satu huruf pun, rantai `PreviousVersionId` benar-benar ditelusuri sampai bacaan aslinya, dan **tidak ada satu pun jalur yang dapat mengubah atau menghapus versi** — dibuktikan dua uji ketiadaan jalur pada controller dan service. **Selisih kontrak ditemukan**: `RAD-STATE-001` bagian 3 menulis versi lama menjadi `Superseded` saat draf koreksi ditulis, sedangkan bagian 4 dan `FR-RAD-020` menyatakan saat koreksi **dirilis**. Yang dikerjakan mengikuti bagian 4 karena bacaan itu yang aman; kalimat bagian 3 perlu diperbaiki pemilik modul. Kontrak diamandemen menjadi `RAD-API-001` rev 7 dan `RAD-PERM-001` rev 7 — **menunggu konfirmasi**. Tidak ada perubahan schema, entity, maupun migration. Laporan: `task/report/backend/BE-RAD-10.md` |

---

## 5. Gelombang `MVP-4` — Rekam Medis, Daftar Kerja, dan Cito

### `BE-RAD-11` — Penyajian hasil ke rekam medis

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-04` |
| Requirement | `FR-RAD-030`, `FR-RAD-032` |
| Decision | `RAD-DEC-006` |
| Contract | `RAD-INT-001` bagian 2; `RAD-API-001` endpoint `GET /by-encounter/{encounterId}` |
| Yang dikerjakan | Endpoint pembacaan hasil per kunjungan; memastikan bacaan yang belum dirilis **tidak** ikut terbawa |
| Acceptance criteria | AC-19, AC-21 |
| Test | Bacaan berstatus `Drafted` tidak muncul; koreksi langsung terlihat tanpa penyalinan |
| Dependency | `BE-RAD-10` |
| Risiko | Sedang. **Jangan** menyediakan jalur apa pun yang memungkinkan modul lain menyimpan salinan |
| Owner | Backend |
| Definition of Done | Uji arsitektur membuktikan tidak ada tabel di luar Radiologi yang menyimpan isi bacaan |
| **Keadaan 2026-09-11** | **SELESAI untuk sisi backend.** `GET /by-encounter/{encounterId}` kini menyaring bacaan yang belum pernah dirilis — draf **dan** bacaan yang sudah disahkan tetapi belum dirilis tidak muncul sama sekali. Build lulus 0 error; **245 uji radiologi lulus**, 12 di antaranya baru; seluruh 1.450 uji in-memory lulus. DoD dibuktikan **empat uji arsitektur** yang memeriksa dari empat arah — entity di luar Radiologi, source seluruh `Areas/`, source modul Radiologi terhadap slot dokumen rekam medis, dan ketergantungan constructor service; ketiga uji pemindaian dijaga terhadap lulus semu. **Perubahan perilaku** pada endpoint yang sudah berjalan, dicatat sebagai `RAD-API-001` rev 8 — **menunggu konfirmasi**. `FR-RAD-031` (gangguan dibedakan dari kekosongan) **tidak tersentuh** — kriteria layar, milik `FE-RAD-13`. Tidak ada perubahan schema, entity, maupun migration. Laporan: `task/report/backend/BE-RAD-11.md` |

### `BE-RAD-12` — Penanda cito pada pesanan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-07` |
| Requirement | `FR-RAD-064`, `FR-RAD-065` |
| Decision | `RAD-DEC-013` |
| Contract | `RAD-ARCH-BE-001` bagian 4.1 dan 7; `RAD-ERD-DICT-001` bagian 4 |
| Yang dikerjakan | Tiga kolom pada `RadOrder`; index `ModalityId` + `IsUrgent` + `OrderStatus`; migration `AddRadOrderUrgency` |
| Acceptance criteria | AC-42 |
| Test | Pesanan lama seluruhnya bernilai `IsUrgent = false`; pemanggil lama yang tidak mengirim field tetap berhasil |
| Dependency | Tidak ada. **Dapat dimajukan ke gelombang mana pun** |
| Risiko | Rendah. Menyentuh tabel yang sudah ada, tetapi hanya menambah kolom bernilai bawaan |
| Owner | Backend |
| Definition of Done | Kompatibilitas mundur terbukti; tidak ada pemanggil lama yang rusak |
| **Keadaan 2026-09-11** | **SELESAI untuk source dan migration.** Tiga kolom pada `RadOrder`, index gabungan `ModalityId` + `IsUrgent` + `OrderStatus`, dan migration `20260911045053_AddRadOrderUrgency`. Build lulus 0 error; **259 uji radiologi lulus**, 14 di antaranya baru; 1.464 uji in-memory **dan 481 uji Sqlite** lulus tanpa satu pun disunting — inilah bukti terkuat DoD kompatibilitas mundurnya, karena project Sqlite memuat uji modul Rawat Inap yang memanggil `RadOrderService`. Migration **dibuat, TIDAK dijalankan**; hanya `RadOrder` yang disentuh, snapshot bertambah tanpa satu baris pun hilang. Penanda cito **tidak** disalin ke `RadStudy` — diturunkan dari pesanannya, dijaga uji. Kontrak diamandemen menjadi `RAD-API-001` rev 9 — **menunggu konfirmasi**; dua field jejak pada rincian berada di luar teks kontrak dan dicatat beserta alasannya. Laporan: `task/report/backend/BE-RAD-12.md` |

### `BE-RAD-13` — Daftar kerja per alat

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-07` |
| Requirement | `FR-RAD-060`, `FR-RAD-061`, `FR-RAD-062` |
| Decision | `RAD-DEC-012` |
| Contract | `RAD-API-001` endpoint `GET /worklist` dan `PUT /{id}/urgency` |
| Yang dikerjakan | Endpoint daftar kerja berupa **penyaringan** atas pesanan dan study yang sudah ada; endpoint ubah penanda cito |
| Acceptance criteria | AC-36, AC-37, AC-39, AC-41 |
| Test | Uji arsitektur membuktikan **tidak ada tabel daftar kerja**; pesanan cito berada di urutan pertama; `GET /worklist` tanpa `modalityId` ditolak `400` |
| Dependency | `BE-RAD-12` |
| Risiko | Rendah |
| Owner | Backend |
| Definition of Done | Daftar kerja hanya menyentuh `RadOrder` dan `RadStudy` |
| **Keadaan 2026-09-11** | **SELESAI.** `GET /worklist` dan `PUT /{id}/urgency`; **dengan ini seluruh 15 task backend roadmap ini selesai.** Build lulus 0 error; **283 uji radiologi lulus**, 24 di antaranya baru; 1.488 uji in-memory dan 481 uji Sqlite lulus. **Tidak ada tabel daftar kerja** — dibuktikan dua uji arsitektur, salah satunya membaca source methodnya sendiri dan memastikan satu-satunya DbSet yang disentuh adalah `RadOrders`. Penanda cito ikut terbaca pada baris study **dan ikut berubah** ketika penanda pesanannya dicabut — bukti penurunan lebih baik daripada penyalinan. Hari kerja dihitung menurut **Waktu Indonesia Barat**, karena shift pagi 06.00 WIB berjalan pada 23.00 UTC hari sebelumnya. Setiap perubahan penanda masuk `RadTransitionHistory` beserta pelakunya, termasuk pencabutannya. **Selisih**: `transaction-endpoint-standard.md` melarang `PUT /{id}/<aksi>`, tetapi kontrak menuliskannya `PUT` dan seluruh dua belas endpoint aksi controller ini memakai `PUT` — dicatat pada `RAD-API-001` rev 10 untuk diputuskan pemilik modul. Tidak ada perubahan schema, entity, maupun migration. Laporan: `task/report/backend/BE-RAD-13.md` |

---

## 6. Task Lintas Gelombang

### `BE-RAD-14` — Uji kontrak hak akses radiologi

| Field | Isi |
|---|---|
| Epic | Seluruhnya |
| Requirement | Menutup `RAD-CAP-025` |
| Decision | `RAD-DEC-003`, `RAD-DEC-005`, `RAD-DEC-015` |
| Contract | `RAD-PERM-001` bagian 9; `RAD-TEST-001` bagian 8 |
| Yang dikerjakan | Uji kontrak mengikuti pola `LaboratoryAuthorityTests` dan `BloodBankRoleAccessContractTests` |
| Acceptance criteria | Empat butir pada `RAD-PERM-001` bagian 9 |
| Test | Setiap endpoint memuat `[AccessPermission]` dengan string persis; tidak ada endpoint tanpa atribut; pemisahan wewenang benar-benar ditegakkan service |
| Dependency | Dijalankan ulang setiap kali endpoint bertambah |
| Risiko | Rendah, tetapi ketiadaannya berisiko tinggi |
| Owner | Backend |
| Definition of Done | Uji lulus dan dijalankan pada setiap gelombang |
| **Keadaan 2026-09-11** | **SELESAI.** `RadiologyRoleAccessContractTests` membuktikan keempat butir `RAD-PERM-001` bagian 9 atas 26 endpoint di lima controller. Build lulus 0 error; **106 uji radiologi lulus**, 18 di antaranya baru. **Tidak ditemukan satu pun cacat** — cacat `403` permanen yang menimpa sembilan endpoint Rawat Inap tidak terjadi di Radiologi. Uji ditempatkan di project in-memory, bukan Postgres, supaya benar-benar dijalankan. Laporan: `task/report/backend/BE-RAD-14.md` |

---

## 6b. Task Susulan

### `BE-RAD-17` — Empat transisi study: tandai perlu diulang, tahan, lanjutkan, batalkan

| Field | Isi |
|---|---|
| Asal | Temuan `FE-RAD-09` bagian 4.1: empat transisi `RAD-STATE-001` bagian 2 tidak punya endpoint. Dibuka atas instruksi pemilik modul Yoga Aji Pratama, 2026-10-14 |
| Epic | `EPIC RAD-05` |
| Decision | `RAD-STATE-001` bagian 2 |
| Contract | `RAD-API-001` revision 12; `RAD-PERM-001` revision 10 |
| Yang dikerjakan | `POST /{id}/mark-repeat-required`, `/hold`, `/resume`, `/cancel` pada `RadStudyController`; empat method pada `RadStudyService`; `RadStudyTransitionRequest`; `StatusBeforeHold` dan `ClosureReason` pada `RadStudyResponse` |
| Acceptance criteria | (1) `QualityRejected` → `RepeatRequired`. (2) Tahan dari status sebelum acquisition, status asal disimpan. (3) Lanjutkan kembali ke status asal. (4) Batal sebelum acquisition, alasan wajib, ditolak bila acquisition pernah dimulai. (5) Transisi tidak sah `409`, alasan kosong `400`. (6) Setiap transisi tercatat di `RadTransitionHistory` beserta pelaku |
| Test | `Tests/QuilvianSystemBackend.RadiologyTests/RadStudyTransitionTests.cs` |
| Dependency | Tidak ada. Kolom `StatusBeforeHold` dan `ClosureReason` sudah ada sejak migration awal — **tanpa migration** |
| Risiko | Rendah. Dua hak akses baru (`RadStudy : Hold`, `RadStudy : Cancel`) perlu diberikan admin |
| Owner | Backend |
| Definition of Done | Endpoint berjalan, uji lulus, kontrak dan matriks izin diperbarui, `FE-RAD-09` dapat menyambungkannya |
| **Keadaan 2026-10-14** | **SELESAI.** Build lulus; 34/34 uji lulus. Disambungkan ke layar oleh sisa `FE-RAD-09` pada hari yang sama — [laporan](../task/report/backend/BE-RAD-17.md) |

---

## 6c. Paritas v1 — `RAD-EVD-001`

Dibuka atas instruksi pemilik modul Yoga Aji Pratama, 2026-10-14: layar Radiologi V2 mengikuti v1 tayang, pengaman
V2 tetap (`RAD-DEC-018`..`RAD-DEC-020`). Urutan fase ada di [`RAD-EVD-001`](../evidence/2026-10-14-keputusan-paritas-v1.md)
bagian 6.

### `BE-RAD-18` — Kolom, tab, dan penyaring Daftar Pasien v1 pada `GET /rad-orders`

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 3, Fase 1 |
| Decision | `RAD-DEC-018`, `RAD-DEC-020` |
| Contract | `RAD-API-001` revision 13 |
| Yang dikerjakan | Kolom `RadOrder.RequiresPreparation` + isian `CreateRadOrderRequest.RequiresPreparation`; penyaring `onlyPreparation`, `examinationDay` (`today`/`notToday`, WIB), `excludeCancelled`; baris daftar bertambah `ExaminationDate`, `RequiresPreparation`, `StudyNumbers`, `DiagnosisText`, `RequestedByName`, `ConfirmedAt`, `ConfirmedByName`, `PaymentStatus`, `IsPaymentCleared` |
| Acceptance criteria | (1) Tab RJ/RI = jenis kunjungan + hari ini WIB. (2) Tab Persiapan = penanda persiapan. (3) Tab Perjanjian = tanggal pemeriksaan bukan hari ini. (4) Daftar Pasien menyembunyikan batal/ditolak, Riwayat tidak. (5) Kolom v1 dibaca dari tabel pemiliknya, satu kueri berkelompok per kolom. (6) Pembayaran memakai aturan Lab r43 apa adanya |
| Test | `Tests/QuilvianSystemBackend.RadiologyTests/RadOrderListV1ColumnsTests.cs` (7 uji) |
| Migration | `20261014094349_AddRadOrderRequiresPreparation` — satu kolom `boolean` bawaan `false` |
| Risiko | Rendah; seluruhnya tambahan |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** Build lulus; 41/41 uji Radiologi lulus. Migration **belum diterapkan** ke devYoga: tanpa kolom itu `GET /rad-orders` di devYoga akan gagal begitu backend baru berjalan — [laporan](../task/report/backend/BE-RAD-18.md) |

### `BE-RAD-19` — Rekap Beranda Radiologi

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 2.1, Fase 3 |
| Yang dikerjakan | `GET /rad-orders/dashboard` (`RadOrder : Read`, baca saja): kartu Total Booking/Rawat Jalan/Rawat Inap/IGD/Pemeriksaan Lunas, status Menunggu/Diproses/Selesai/Dibatalkan/CITO, tren 30 hari (tepat 30 butir), komposisi Rajal/Ranap/IGD/Rujukan, lima booking terbaru |
| Contract | `RAD-API-001` revision 14 |
| Test | `Tests/QuilvianSystemBackend.RadiologyTests/RadDashboardTests.cs` (5 uji, "sekarang" dipatok) |
| Dependency | `BE-RAD-18` (status pembayaran) |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** 46/46 uji Radiologi lulus; tanpa schema. Belum dijalankan terhadap devYoga — [laporan](../task/report/backend/BE-RAD-19.md) |

### `BE-RAD-20` — Petugas Radiologi dan Dokter Pemeriksa per pesanan

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 2.3, Fase 4 |
| Decision | `RAD-DEC-021` — ikut v1. Daftar kerja per alat `RAD-DEC-012` tetap; penunjukan ini tidak memberi hak mengesahkan bacaan (`RAD-DEC-003`) |
| Contract | `RAD-API-001` revision 15; `RAD-PERM-001` (tanpa string izin baru) |
| Yang dikerjakan | `RadOrder.RadiographerUserId`, `ExaminingDoctorId` (migration `AddRadOrderAssignment`); `PUT /{id}/assignment`; `GET /examiner-doctor-options`, `/radiographer-options`; baris daftar + rincian: nama petugas/dokter, `PatientPhoneNumber`, `TariffAmount` |
| Test | `Tests/QuilvianSystemBackend.RadiologyTests/RadOrderAssignmentTests.cs` (7 uji) |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** 53/53 uji Radiologi. Satu cacat ditemukan uji dan diperbaiki: tarif yang belum diatur sempat terbaca Rp0 (`GetValueOrDefault` atas `decimal`) — [laporan](../task/report/backend/BE-RAD-20.md) |

### `BE-RAD-21` — Kunci Lunas sebelum proses

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 3, Fase 5 |
| Dependency | `BE-RAD-18` (`IsPaymentCleared`) |
| Temuan | Kunci pola Lab akan mengunci semua pasien tunai, karena tagihan Radiologi terbit sesudah mutu citra (`RWI-DEC-195`) |
| Jawaban v1 (`RAD-DEC-021`) | **v1 tidak punya kunci Lunas** — `ubahStatusPembayaran` didefinisikan tetapi tidak pernah dipanggil; status pembayaran hanya kolom tampilan, dan kolom itu sudah ada (`BE-RAD-18`) |
| **Keadaan 2026-10-15** | **DIBATALKAN** |

### `BE-RAD-22` — Unggah gambar dan jumlah film

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 2.3, Fase 6 |
| Jumlah film | **Sudah tersedia** — `RadAcquisitionConsumption` jenis `Film` (jumlah, nama butir, satuan) sudah ada di skema, dicatat dari konsol study `FE-RAD-09`. Ukuran Kecil/Sedang/Besar dan Hitam/Berwarna v1 cukup menjadi nama butir; pilihan bakunya butuh daftar film dari pemilik persediaan |
| Alur v1 | Petugas mengunggah satu atau lebih gambar pada Form Hasil; disimpan di server (`photoLabFiles`, `/uploads/HasilLabPhoto`) |
| Yang dikerjakan | Pola `ReferralDocumentStorageService`: tabel `RadOrderImage`, berkas privat `radiology-images/{orderId}/`, JPG/PNG dari isi, ≤10 MB, ≤20 per pesanan; `GET/POST /{id}/images`, `GET …/content`, `DELETE …/{imageId}`; jejak `Order.ImageAdded/Removed` |
| Contract | `RAD-API-001` revision 16 |
| Test | `Tests/QuilvianSystemBackend.RadiologyTests/RadOrderImageTests.cs` (7 uji) |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** 60/60 uji. Migration `AddRadOrderImage` menunggu instruksi untuk devYoga. Penanda "Unggah Gambar" di layar dicabut — [laporan](../task/report/backend/BE-RAD-22.md) |

### `BE-RAD-23` — Kirim hasil lewat WhatsApp

| Field | Isi |
|---|---|
| Asal | `RAD-EVD-001` bagian 3, Fase 6 |
| Jawaban v1 (`RAD-DEC-021`) | `wa.me/<62…>?text=…` dari peramban — tanpa backend |
| **Keadaan 2026-10-15** | **SELESAI — TANPA BACKEND.** Dikerjakan `FE-RAD-17`; nomor dari `PatientPhoneNumber` (`BE-RAD-20`) |

### `BE-RAD-24` — Laporan operasional dan Tarif Tindakan

| Field | Isi |
|---|---|
| Asal | Instruksi pemilik modul 2026-10-15: bangun fitur bertanda yang dapat dibangun (`RAD-DEC-022`) |
| Alur v1 | Menu Laporan (Statistik Pemeriksaan, Dokter Pengirim, Pasien Perusahaan), Pemeriksaan (Harian, Per Item), Setup → Tarif Tindakan. Di v1 tayang kelimanya "Coming Soon"; isi diturunkan dari pesanan V2 |
| Yang dikerjakan | `RadOperationalReportService`: rentang `startDate`/`endDate` WIB (kosong = 30 hari, maksimal 366 hari); statistik per pemeriksaan, dokter pengirim (dokter pemberi instruksi, atau akun pemesan), pasien berpenjamin perusahaan, harian (termasuk hari kosong), per item (≤ 5.000 baris, `IsTruncated`), tarif tindakan (`MstProcedure` `IsRadiology` + `MstTariff`) |
| Contract | `RAD-API-001` revision 17 |
| Test | `RadV1FeatureTests.cs` — 4 uji (statistik dan harian dalam rentang WIB; pasien perusahaan; rentang tidak sah; tarif hanya pemeriksaan radiologi) |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** 71/71 uji Radiologi. Tanpa tabel baru, tetapi ikut menunggu migration `BE-RAD-25` karena berbagi entitas `RadOrder` — [laporan](../task/report/backend/BE-RAD-24.md) |

### `BE-RAD-25` — Ubah Pemeriksaan dan Cetak Film

| Field | Isi |
|---|---|
| Asal | Instruksi pemilik modul 2026-10-15 (`RAD-DEC-022`) |
| Alur v1 | Detail Pasien: aksi baris *Ubah Pemeriksaan*; kolom *Cetak Film* berupa centang |
| Yang dikerjakan | `PUT /{id}/procedure` — hanya `Requested`/`Accepted`/`Scheduled`, tanpa study yang belum batal, alasan wajib, pemeriksaan harus `IsRadiology` aktif; jejak `Order.ChangeProcedure`. `PUT /{id}/film-print` — kolom `RadOrder.PrintFilm`; jejak `Order.FilmPrint` hanya bila nilainya berubah. Jumlah film tetap dicatat sebagai bahan terpakai `Film` |
| Contract | `RAD-API-001` revision 17 |
| Test | `RadV1FeatureTests.cs` — 3 uji |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** Migration `20261015054457_AddRadReportTemplateAndFilmPrint` belum diterapkan — [laporan](../task/report/backend/BE-RAD-25.md) |

### `BE-RAD-26` — Template Hasil

| Field | Isi |
|---|---|
| Asal | Instruksi pemilik modul 2026-10-15 (`RAD-DEC-022`) |
| Alur v1 | Menu Setup → Template Hasil ("Coming Soon" di v1 tayang) |
| Yang dikerjakan | Tabel `RadReportTemplate` (kode unik di antara yang belum dihapus, alat/pemeriksaan opsional, Temuan/Kesan/Saran); `GET /rad-reports/templates` (`RadReport : Read`), `POST/PUT/DELETE` (`RadReport : Validate` — dokter radiolog). Template hanya isian awal draf; pengesahan `RAD-DEC-003` tidak berubah |
| Contract | `RAD-API-001` revision 17; `RAD-PERM-001` — tanpa string izin baru |
| Test | `RadV1FeatureTests.cs` — 1 uji (buat, ubah, hapus, kode unik, saringan alat) |
| **Keadaan 2026-10-15** | **SELESAI — BATAS VERIFIKASI.** Migration sama dengan `BE-RAD-25` — [laporan](../task/report/backend/BE-RAD-26.md) |

---

## 7. Peta Dependency

```text
BE-RAD-04 ─┐
BE-RAD-05 ─┤
BE-RAD-01 ─┴─> BE-RAD-02 ──> BE-RAD-03 ──> BE-RAD-15
     └────────> BE-RAD-06

BE-RAD-07 ──> BE-RAD-08 ──> BE-RAD-09 ──> BE-RAD-10 ──> BE-RAD-11

BE-RAD-12 ──> BE-RAD-13

BE-RAD-14 ── dijalankan ulang setiap gelombang
```

`BE-RAD-07` sampai `BE-RAD-11` **tidak bergantung** pada `MVP-0`, sehingga secara teknis dapat
berjalan paralel. Tetapi tanpa `MVP-0` tidak akan ada citra layak untuk dibaca, sehingga
pengujiannya dengan data nyata mustahil.

---

## 8. Risiko yang Perlu Diketahui Sejak Awal

| Risiko | Task | Cara menekan |
|---|---|---|
| Migration 1 mematikan seluruh pemeriksaan | `BE-RAD-01` | Nilai bawaan `RuleStatus` adalah `Active`; isi kolom sebelum mengubah index; uji pada database berisi data |
| Aturan pengesahan bacaan tidak ditegakkan | `BE-RAD-08` | Pemeriksaan di service, bukan atribut; dibuktikan `BE-RAD-14` |
| Versi bacaan tertimpa | `BE-RAD-10` | Tidak ada endpoint ubah maupun hapus versi rilis |
| Modul lain menyalin hasil bacaan | `BE-RAD-11` | Uji arsitektur, memperluas uji rawat inap yang sudah ada |
| Data master awal belum disahkan klinis | `BE-RAD-15` | Data bawaan berstatus `Draf`; tetap wajib disahkan sebelum berlaku |

---

## 9. Yang Tidak Ada di Roadmap Ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Pelewatan gerbang keselamatan darurat | `S5` menunggu `DEC-RAD-001`; tanda tangan klinis lewat `RAD-REQ-002` |
| Temuan kritis dan pemberitahuannya | `S11` menunggu `DEC-RAD-002` |
| Pemantauan keterlambatan pesanan cito | Ditunda `RAD-DEC-013`; `RAD-OPEN-009` |
| Penyambungan pemesanan radiologi dari IGD | Milik modul IGD; menunggu frontend Radiologi Rilis 1 |
| Integrasi PACS dan DICOM | Di luar scope `RAD-DEC-001` — **diubah `RAD-DEC-020` (2026-10-14)**: masuk cakupan, tetapi bentuknya menunggu `RAD-OPEN-013`. Begitu pula AI (`RAD-OPEN-014`) |
| Penghapusan dua endpoint data induk lama di `RadStudyController` | Task tersendiri setelah seluruh konsumen berpindah |

---

## 10. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-10 | Roadmap backend pertama. 15 task, 5 gelombang, dari 7 epic yang disetujui. | `draft` |
| 2 | 2026-10-14 | Bagian **0** *Ringkasan Task Backend dan Status* ditambahkan untuk pelacakan: 16 nomor task — 14 selesai, 1 selesai dengan batas verifikasi (`BE-RAD-16`), 1 selesai sebagian dengan sisa non-kode (`BE-RAD-15`). `BE-RAD-16` dicatat walaupun lahir di luar roadmap. Blok task tidak diubah. | `approved` |
| 3 | 2026-10-14 | `BE-RAD-17` dibuka atas instruksi pemilik modul dan selesai hari yang sama — bagian 6b. Angka bagian 0 menjadi 17 nomor task: 15 selesai, 1 batas verifikasi, 1 selesai sebagian dengan sisa non-kode. | `approved` |
| 4 | 2026-10-15 | Paritas v1 (`RAD-EVD-001`): bagian 6c membuka `BE-RAD-18`..`BE-RAD-23`. `BE-RAD-18` selesai dengan batas verifikasi (migration tertunda). `BE-RAD-20` tertahan karena bertentangan dengan `RAD-DEC-012`; `BE-RAD-23` tertahan `RAD-OPEN-015`. Baris PACS bagian 9 mencatat perubahan `RAD-DEC-020`. Angka bagian 0: 23 nomor task. | `approved` |
| 5 | 2026-10-15 | `BE-RAD-19` selesai dengan batas verifikasi (`RAD-API-001` r14). `BE-RAD-21` tertahan `RAD-OPEN-016` (waktu terbit tagihan Radiologi); `BE-RAD-22` tertahan `RAD-OPEN-013` — bagian jumlah film ternyata sudah tersedia. Angka bagian 0: 15 selesai, 3 batas verifikasi, 1 sebagian, 4 tertahan. | `approved` |
| 6 | 2026-10-15 | Pemilik modul: semua keputusan ikut v1, fitur v1 yang belum ada dipasang bertanda (`RAD-DEC-021`, `RAD-DEC-022`). `BE-RAD-20` selesai (batas verifikasi); `BE-RAD-21` dibatalkan (v1 tanpa kunci); `BE-RAD-23` selesai tanpa backend; `BE-RAD-22` bertanda. Angka: 16 selesai, 4 batas verifikasi, 1 sebagian, 1 dibatalkan, 1 bertanda. | `approved` |
| 7 | 2026-10-15 | Migration `AddRadOrderRequiresPreparation` + `AddRadOrderAssignment` diterapkan ke devYoga atas instruksi pemilik modul (uji kering naik→turun→`ROLLBACK`, lalu `COMMIT`; riwayat 318 → 320, nol tertunda, baris `RadOrder` tak berubah). GET terhadap devYoga 10/10 tanpa `42703`. Batas verifikasi `BE-RAD-18`/`19`/`20` ditutup. Angka: 19 selesai, 1 batas verifikasi, 1 sebagian, 1 dibatalkan, 1 bertanda. | `approved` |
| 8 | 2026-10-15 | `BE-RAD-22` Unggah Gambar dibangun (`RAD-API-001` r16), 60/60 uji; migration `AddRadOrderImage` belum diterapkan. Angka: 19 selesai, 2 batas verifikasi, 1 sebagian, 1 dibatalkan. | `approved` |
| 9 | 2026-10-15 | Migration `20261015043017_AddRadOrderImage` diterapkan ke devYoga atas instruksi pemilik modul (uji kering naik→turun→`ROLLBACK`, lalu `COMMIT`; riwayat 320 → 321, nol tertunda, `RadOrder` tak berubah). GET Radiologi terhadap devYoga 11/11. Angka: 20 selesai, 1 batas verifikasi, 1 sebagian, 1 dibatalkan. | `approved` |
| 10 | 2026-10-15 | Perapian penutup: batas verifikasi `BE-RAD-16` ditutup (`RadReportPendingTests`, 63/63); bagian 0.3 ditulis ulang menjadi satu daftar sisa non-kode bernomor. Angka: 21 selesai, 1 sebagian, 1 dibatalkan. | `approved` |
| 11 | 2026-10-15 | Instruksi pemilik modul: bangun fitur bertanda yang dapat dibangun. `BE-RAD-24` (laporan operasional + tarif tindakan), `BE-RAD-25` (Ubah Pemeriksaan, Cetak Film), `BE-RAD-26` (Template Hasil) dibuka dan dibangun; `RAD-API-001` r17; 71/71 uji. Migration `AddRadReportTemplateAndFilmPrint` belum diterapkan. Bagian 0.3 butir 4–5 ditulis ulang. Angka: 26 nomor — 21 selesai, 3 batas verifikasi, 1 sebagian, 1 dibatalkan. | `approved` |
| 12 | 2026-10-15 | Atas instruksi pemilik modul: migration `20261015054457_AddRadReportTemplateAndFilmPrint` diterapkan ke devYoga (riwayat 321 → 322), GET 10/10; izin Kepala Instalasi Radiologi (+16) dan Radiografer (+9) diberikan; 12 aturan keselamatan diajukan Bayu dan disahkan dr. Arya — seluruhnya `Active`. Tiga alat (DX, XA, NM) belum punya aturan. Angka: 24 selesai, 1 sebagian, 1 dibatalkan. | `approved` |
| 13 | 2026-10-15 | Katalog pemeriksaan dan tarif Radiologi disalin dari v1 tayang atas instruksi pemilik modul: 481 pemeriksaan, 450 tarif (harga cocok 450/450), 31 tanpa tarif karena v1 berharga 0. Bagian 0.3 butir 3 ditutup. Angka tidak berubah. | `approved` |
