# Backend Roadmap — Modul Radiologi

| Field | Value |
|---|---|
| Roadmap ID | `RAD-RM-BE-001` |
| Revision | `1` |
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
| Integrasi PACS dan DICOM | Di luar scope `RAD-DEC-001` |
| Penghapusan dua endpoint data induk lama di `RadStudyController` | Task tersendiri setelah seluruh konsumen berpindah |

---

## 10. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-10 | Roadmap backend pertama. 15 task, 5 gelombang, dari 7 epic yang disetujui. | `draft` |
