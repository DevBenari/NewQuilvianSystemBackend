# Rawat Inap — Peta Modul

| Field | Value |
|---|---|
| Dokumen | `02-module-map.md` — hanya lahir pada `blueprint_shape: COMPOSITE` |
| Revision | `1` |
| Status | `draft` |
| Tanggal | 2026-09-02 |
| Modul | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| Bentuk blueprint | `COMPOSITE`, ditetapkan `RWI-DEC-082`, `shape_decided_by: USER_CONFIRMED` |
| Masukan keputusan | [`00-interview-decisions.md`](./00-interview-decisions.md) revision `7` — `RWI-DEC-080` s.d. `RWI-DEC-083`, `RWI-OQ-047` |
| Masukan keadaan saat ini | [`01-existing-capability-map.md`](./01-existing-capability-map.md) revision `1.2` |
| Baseline requirement | `PRD_Final_Rawat_Inap_100_Persen.md` v1.0.0 (`PRD-RWI-FINAL-001`), `docs/Modul-RS/Rawat-Inap/` — menggantikan batas scope revision `4` lewat `RWI-DEC-080` |
| Owner | Product/Domain: **Muhammad Hamzah**, ditunjuk `RWI-DEC-061` |
| Ditulis oleh | `/qv-design` gerakan ① `bentuk-blueprint.md` bagian 6 |

---

## 0. Kenapa berkas ini ada

Modul Rawat Inap dipecah menjadi **tiga sub-modul** lewat `RWI-DEC-082`. Begitu sebuah modul
dipecah, ada tiga hal yang **tidak boleh ada tiga salinannya**, karena tidak akan ada satu berkas
pun yang memergoki bila salinannya berbeda-beda:

| Yang dijaga | Kalau tidak dijaga di satu tempat |
|---|---|
| Tabel kepemilikan data seluruh modul | Dua sub-modul membuat tabel yang sama, dan tidak ada yang tahu |
| Peta butir menu seluruh modul | Tiga sub-modul merancang sidebar sendiri-sendiri |
| Urutan migration lintas sub-modul | Tiga migration sama-sama merasa urutan pertama |
| Pemetaan kemampuan ke sub-modul | **Kemampuan yatim** — kemampuan tanpa sub-modul pemilik tidak diperiksa siapa pun, karena `requirement-traceability.md` menjadi milik masing-masing sub-modul dan hanya memeriksa jatahnya sendiri |

Berkas ini berisi **persis empat bagian** itu. Arsitektur, kontrak, kamus data, PRD, roadmap, dan
laporan task **tidak** ada di sini; semuanya tinggal di dalam folder sub-modul masing-masing.

> **Cara mengenali folder sub-modul:** folder yang memuat `blueprint-manifest.md` adalah sub-modul.
> `evidence/` bukan sub-modul.

---

## 1. Registry sub-modul

Tiga sub-modul, hasil uji pemecahan `bentuk-blueprint.md` bagian 4.1 yang dicatat `RWI-DEC-082`.

| Slug | Rumpun kemampuan | Uji pemecahan | Jumlah kemampuan | Status | Pemilik | Approval |
|---|---|:---:|:---:|---|---|---|
| [`episode-rawat-inap/`](./episode-rawat-inap/) | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan | **5/5** | 16 | `approved` | Muhammad Hamzah | **Muhammad Hamzah, 2026-08-24** lewat `RWI-DEC-074` |
| [`keperawatan/`](./keperawatan/) | Pengkajian, asuhan, tindakan keperawatan, gizi, pemakaian alat | **3/5** | 5 | `draft` | Muhammad Hamzah | Belum — **dirancang 2026-09-02**, menunggu approval |
| [`dokter-rawat-inap/`](./dokter-rawat-inap/) | SOAP, CPPT, kajian medis, resep, tindakan, visite, penunjang | **3/5** | 7 | `draft` | Muhammad Hamzah | Belum — **dirancang 2026-09-02**, menunggu approval |

**Status modul diturunkan, bukan ditulis tangan.** Satu `approved` + dua `draft` = **`partial`**,
mengikuti `bentuk-blueprint.md` bagian 7. Modul ini **tidak boleh** terlihat `approved` selama dua
sub-modulnya belum disetujui.

> **Diperbarui 2026-09-02 sore.** `keperawatan` **sudah dirancang** dan kesebelas berkasnya sudah
> berisi. Statusnya tetap `draft` karena approval adalah tindakan manusia; status modul karena itu
> tetap `partial`.

### 1.1 Kenapa dua sub-modul baru lahir `draft`, bukan `BLOCKED`

`bentuk-blueprint.md` bagian 6 gerakan ③ menyatakan sub-modul yang **batas kepemilikan datanya
belum diputuskan** lahir `BLOCKED`. Di sini keadaannya bukan itu:

| Hal | Keadaannya |
|---|---|
| Kepemilikan tabel pengkajian, CPPT, SOAP, kajian medis, resep, tindakan | **Sudah diputuskan** — milik `ClinicalManagement`, `RWI-DEC-081` |
| Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` | **Sudah diberikan** 2026-08-21, `RWI-DEC-062`, menutup `RWI-OQ-032` dan `DEC-INP-001` |
| Yang benar-benar tersisa | Untuk `keperawatan`: **approval pemilik** ditambah satu penghalang **teknis** `INT-KEP-01`, yang ternyata hanya satu cabang validasi — bukan subsistem baru. Untuk `dokter-rawat-inap`: pekerjaan desain yang belum dikerjakan |

Karena tidak ada satu pun keputusan bisnis yang menggantung, keduanya berstatus **`draft`**:
belum dirancang, bukan terhalang. Bedanya penting — `BLOCKED` berarti menunggu orang, `draft`
berarti menunggu pekerjaan.

### 1.2 Rumpun yang dinilai lalu **ditolak** menjadi sub-modul

| Rumpun yang dipertimbangkan | Hasil uji | Keputusannya |
|---|:---:|---|
| Integrasi penunjang — laboratorium, radiologi, kamar operasi, billing, gizi | **1/5** | **Bukan sub-modul.** Ia tidak punya bounded context, mesin status, master data, maupun pemilik peran sendiri di dalam modul ini. Kontraknya dibagikan ke rumpun yang memakainya, sesuai `RWI-DEC-082` |

Dicatat supaya tidak diusulkan ulang di kemudian hari.

### 1.3 Prefix ID task

| Sub-modul | Prefix task | Keterangan |
|---|---|---|
| `episode-rawat-inap` | `BE-RWI-###`, `FE-RWI-###` | Sudah berjalan; 36 laporan backend dan 41 laporan frontend memakai deret ini |
| `keperawatan` | `BE-RWI-###`, `FE-RWI-###` | **Deret yang sama, dilanjutkan** |
| `dokter-rawat-inap` | `BE-RWI-###`, `FE-RWI-###` | **Deret yang sama, dilanjutkan** |

Pembedanya adalah **folder**, bukan ID. Sebuah task selalu berasal dari satu berkas roadmap, dan
letak roadmap itu sudah menjawab `<blueprint-root>`-nya — `bentuk-blueprint.md` bagian 2. Deret ID
sengaja **tidak** dipecah supaya 77 laporan yang sudah ada tidak perlu dinomori ulang.

> **Yang perlu dijaga:** karena deretnya satu, penomoran task lintas sub-modul **wajib** diambil
> dari nomor tertinggi di seluruh modul, bukan tertinggi di dalam folder sendiri. Bila kelak dua
> roadmap ditulis berbarengan, pemisahan prefix menjadi keputusan tersendiri lewat `/qv-grill`.

---

## 2. Tabel kepemilikan data seluruh modul

Ini pertahanan paling langsung terhadap duplikasi entity. Kolom **Dibuat ulang** yang berisi "Ya"
wajib punya alasan.

Tabel ini **naik** dari `episode-rawat-inap/02-backend-architecture.md` bagian 2. Yang tinggal di
sana sekarang hanya kelompok data milik sub-modul itu sendiri, beserta rujukan balik ke sini.

### 2.1 Data milik modul lain — dipakai, tidak dibuat ulang

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang |
|---|---|---|---|
| Pasien | Patient Management (modul lain) | Ketiganya | Tidak — dirujuk lewat kunjungan |
| Kunjungan pasien | Registration Management (modul lain) | Ketiganya | Tidak — `InpEpisode.EncounterId` |
| Penjamin kunjungan | Registration Management (modul lain) | `episode-rawat-inap` | Tidak — dibaca lewat kunjungan |
| Tempat tidur, kamar, unit layanan, kelas pasien | Master Data HealthServices (modul lain) | `episode-rawat-inap` | Tidak — dirujuk lewat Id |
| Status ketersediaan tempat tidur | Master Data HealthServices (modul lain) | `episode-rawat-inap` | Tidak — **ditulis** sebagai salinan; satu-satunya penulisan lintas modul, lihat `episode-rawat-inap/02-backend-architecture.md` §2.1 |
| Dokter | Corporate HR Workforce (modul lain) | `episode-rawat-inap`, `dokter-rawat-inap` | Tidak — `InpDoctorAssignment.DoctorId` |
| Pegawai dan profil tenaga kerja | Corporate HR Workforce (modul lain) | `episode-rawat-inap`, `keperawatan` | Tidak — `InpNurseAssignment.EmployeeId` |
| Surat keterangan medis | Clinical Management (modul lain) | `episode-rawat-inap` | Tidak — dipakai apa adanya untuk lembar yang diserahkan pasien |
| Faktur, tagihan, pembayaran | Billing Management (modul lain) | Tidak dipakai pada MVP | Tidak |
| Disposisi IGD | Emergency Installation Management (modul lain) | `episode-rawat-inap`, hanya jalur serah terima | Tidak — di luar scope MVP; `DEC-INP-002` menunggu Rizki Gunawan |
| Catatan kepergian pasien dari IGD | Emergency Installation Management (modul lain) | `episode-rawat-inap`, **dibaca** | Tidak — waktu tiba dibaca dari event `Tiba` di sana, `RWI-DEC-072` |
| Rangkaian kedatangan antar kunjungan | Registration Management (modul lain) | `episode-rawat-inap`, **dibaca** | Tidak — kolom `TrxPatientEncounter.OriginEncounterId` dibuat dan diisi modul IGD, `RWI-DEC-073` |

### 2.2 Data milik modul ini

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang |
|---|---|---|---|
| Episode rawat inap | **`episode-rawat-inap`** | **Ketiganya** — keperawatan dan dokter membacanya sebagai konteks, tidak menulisnya | **Ya** — konsep baru, tidak ada pemiliknya di mana pun |
| Pemesanan dan penempatan tempat tidur | **`episode-rawat-inap`** | `episode-rawat-inap` | **Ya** — konsep baru; hari ini tidak ada satu pun catatan penghunian di dalam sistem |
| Penanggung jawab episode — DPJP dan perawat | **`episode-rawat-inap`** | Ketiganya — dibaca sebagai penentu kewenangan menulis dokumentasi | **Ya** — berbentuk riwayat berperiode, berbeda dari kolom dokter pada kunjungan |
| Resume pulang beserta versinya | **`episode-rawat-inap`** | `episode-rawat-inap`, ditulis DPJP | **Ya** — catatan resmi episode, berbeda dari surat keterangan milik Clinical Management. `CAP-026` tetap milik episode, `RWI-DEC-083` |
| Daftar periksa administrasi dan penandaannya | **`episode-rawat-inap`** | `episode-rawat-inap` | **Ya** — butir per rumah sakit, dapat diubah admin |
| Riwayat status episode | **`episode-rawat-inap`** | Ketiganya, **dibaca** | **Ya** — jejak yang tidak dapat dihapus |
| Sesi koreksi episode | **`episode-rawat-inap`** | `episode-rawat-inap` | **Ya** — konsep tersendiri, bukan status episode keenam |
| Pengaturan Rawat Inap yang dapat diubah admin | **`episode-rawat-inap`** | Ketiganya, **dibaca** | **Ya** — mengikuti pola `MstEmergencySetting` |

### 2.3 Dokumentasi klinis — dimiliki `ClinicalManagement`, `RWI-DEC-081`

`RWI-DEC-081` menegaskan: Rawat Inap **tidak** membuat tabel tandingan untuk dokumentasi klinis. Ia
hanya menyediakan **workspace, konteks episode, dan kontrak**. Kedelapan baris berikut karena itu
seluruhnya berkolom "Tidak" pada **Dibuat ulang** — dan itu bukan kelalaian, melainkan keputusan.

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang |
|---|---|---|---|
| Pengkajian awal dan pengkajian ulang keperawatan | Clinical Management (modul lain) | `keperawatan` | **Tidak** — `RWI-DEC-081`. Tabelnya **sudah ada** (`TrxPatientAssessment`, 85 kolom); yang diminta enam kolom tambahan |
| Diagnosis, rencana asuhan, dan evaluasi keperawatan | Clinical Management (modul lain) | `keperawatan` | **Tidak** — `RWI-DEC-081`. Tabelnya **belum ada di mana pun**; diminta tiga tabel baru milik `ClinicalManagement` |
| Catatan dan tindakan keperawatan | Clinical Management (modul lain) | `keperawatan` | **Tidak** — `RWI-DEC-081`. `TrxPatientProcedure` **tidak dapat dipakai ulang**: ia mewajibkan konsultasi dan dokter. Diminta satu tabel baru milik `ClinicalManagement` |
| Kajian medis awal | Clinical Management (modul lain) | `dokter-rawat-inap` | **Tidak** — `RWI-DEC-081` |
| Catatan SOAP | Clinical Management (modul lain) | `dokter-rawat-inap` | **Tidak** — `RWI-DEC-081` |
| CPPT | Clinical Management (modul lain) | `dokter-rawat-inap` dan `keperawatan` bersama | **Tidak** — `RWI-DEC-081`. CPPT memang terintegrasi lintas profesi; itu sifatnya, bukan tabrakan kepemilikan |
| Resep rawat inap dan obat pulang | Pharmacy Management (modul lain) | `dokter-rawat-inap` | **Tidak** — `RWI-DEC-046`, penanda obat pulang adalah **jenis resep**, bukan daftar terpisah |
| Tindakan dokter | Clinical Management (modul lain) | `dokter-rawat-inap` | **Tidak** — `RWI-DEC-081` |

> **Penghalang yang tersisa bersifat teknis, bukan keputusan.** *Shared inpatient clinical context
> resolver* pada `PRD-RWI-FINAL-001` bagian 30.3: mesin klinis hari ini berputar pada antrean dan
> konsultasi, sedangkan pasien rawat inap tidak punya antrean. Pelonggarannya sudah disetujui
> pemiliknya lewat `RWI-DEC-062` dan dirinci pada `RWI-RULE-026`; yang belum ada adalah kodenya.

### 2.4 Satu baris yang **belum diputuskan** — `RWI-OQ-047`

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang |
|---|---|---|---|
| Kelayakan keuangan (*financial clearance*) | **Belum diputuskan** — `PRD-RWI-FINAL-001` bagian 23.1 menaruhnya pada **Billing Management**; `RWI-RULE-028` aturan 7 memilikinya **sementara** lewat `InpFinancialClearance` milik Rawat Inap | `episode-rawat-inap` | **Belum diputuskan** — hari ini `Ya, sementara`; dicabut atau dipertahankan bergantung jawaban `RWI-OQ-047` |

| Catatan pemakaian alat pada pasien | **Belum diputuskan** — `PRD-RWI-FINAL-001` bagian 23.1 memuat 28 baris *source of truth* dan **tidak satu pun** menyebut Equipment Usage. `RWI-DEC-081` juga tidak. Calonnya: modul persediaan, `ClinicalManagement`, atau `InPatientManagement` | `keperawatan` | **`OPEN DECISION`** — ditemukan 2026-09-02 saat merancang `keperawatan`; usulan `RWI-OQ-048` |

**Kedua baris sengaja ditulis apa adanya.** `blueprint-output-contract.md` bagian 3.0 menyatakan
kolom `Pemilik` yang berisi "belum diputuskan" adalah keadaan yang **sah**; yang tidak sah adalah
membiarkan barisnya kosong lalu sub-modul diam-diam membuat tabelnya sendiri.

| Hal | Keadaannya |
|---|---|
| Pertentangannya | PRD bagian 23.1 vs `RWI-RULE-028` aturan 7 |
| Ditemukan | 2026-09-02, saat Amendment Pass |
| Pemilik jawaban | Product/Domain bersama pemilik `BillingManagement` |
| Yang ditahannya | **Hanya baris ini.** Tidak menahan desain sub-modul mana pun |
| Yang **tidak** ditahannya | `InpFinancialClearance` sudah ada di kode dan sudah dipakai `BE-RWI-024`. Bila `RWI-OQ-047` kelak memilih Billing, yang berubah adalah **sumber bacaannya**, bukan seluruh alur penutupan — kelima syarat penutupan `RWI-RULE-028` tetap berlaku apa adanya |

---

## 3. Peta butir menu dan urutan migration

### 3.1 Peta butir menu seluruh modul — sidebar hanya satu

Peta ini **naik** dari `episode-rawat-inap/03-frontend-architecture.md` bagian 2B dan
`episode-rawat-inap/05-skema-tampilan.md` bagian 2. Nama route, nama menu, dan urutannya tetap
`DEV_DISCRETION`; **keterjangkauan tidak**.

Aturan `IA-INP-05` mengikat seluruh modul: menu tingkat dua Rawat Inap dibatasi **paling banyak
sembilan butir**. Karena itu sub-modul `keperawatan` dan `dokter-rawat-inap` **tidak boleh**
menambah butir menu tingkat dua tanpa lebih dulu mengubah `IA-INP-05` — sembilan butir itu sudah
habis dipakai `episode-rawat-inap`.

| # | Butir menu | Tingkat | Induk | Route usulan | Layar | Sub-modul | Hak akses penjaga |
|---:|---|:---:|---|---|---|---|---|
| 1 | Beranda Rawat Inap | 2 | Pelayanan Kesehatan → Rawat Inap | `…/inpatient-management` | `FE-INP-19` | `episode-rawat-inap` | `InpatientEpisode : Read` |
| 2 | Admisi Rawat Inap | 2 | Pelayanan Kesehatan → Rawat Inap | `…/admissions` | `FE-INP-03` | `episode-rawat-inap` | `InpatientEpisode : Create` |
| 3 | Papan Tempat Tidur | 2 | Pelayanan Kesehatan → Rawat Inap | `…/bed-board` | `FE-INP-02` | `episode-rawat-inap` | `InpatientBedOccupancy : Read` |
| 4 | Daftar Kerja Episode | 2 | Pelayanan Kesehatan → Rawat Inap | `…/episodes` | `FE-INP-16` | `episode-rawat-inap` | `InpatientEpisode : Read` |
| 5 | Pasien Sedang Dirawat (census) | 2 | Pelayanan Kesehatan → Rawat Inap | `…/census` | `FE-INP-01` | `episode-rawat-inap` | `InpatientEpisode : Read` |
| 6 | Daftar Pantau | 2 | Pelayanan Kesehatan → Rawat Inap | `…/monitoring` | `FE-INP-09` | `episode-rawat-inap` | `InpatientEpisode : Read` |
| 7 | Selisih Tempat Tidur | 2 | Pelayanan Kesehatan → Rawat Inap | `…/bed-drift` | `FE-INP-10` | `episode-rawat-inap` | `InpatientBedOccupancy : Read` |
| 8 | Pengaturan Rawat Inap | 2 | Pelayanan Kesehatan → **Master Data** | `…/settings` | `FE-INP-12` | `episode-rawat-inap` | `MstInpatientSetting : Update` |
| 9 | Butir Administrasi Rawat Inap | 2 | Pelayanan Kesehatan → **Master Data** | `…/clearance-items` | `FE-INP-13` | `episode-rawat-inap` | `MstInpatientClearanceItem : Read` |

**Sembilan butir. Kuota `IA-INP-05` sudah penuh.**

### 3.2 Layar anak — tidak mendapat butir menu, wajib punya jalan masuk

`bentuk-blueprint.md` menuntut setiap layar muncul sebagai butir menu **atau** dinyatakan sebagai
layar anak beserta layar induknya. Berikut yang kedua.

| Layar | Nama | Induk yang menjadi jalan masuk | Sub-modul |
|---|---|---|---|
| `FE-INP-04` | Detail Episode | Daftar Kerja Episode / Census | `episode-rawat-inap` |
| `FE-INP-05` | Perpindahan Pasien | Detail Episode | `episode-rawat-inap` |
| `FE-INP-06` | Keputusan Pulang dan Resume | Detail Episode | `episode-rawat-inap` |
| `FE-INP-07` | Penutupan Episode | Detail Episode | `episode-rawat-inap` |
| `FE-INP-08` | Kelayakan Keuangan | Detail Episode | `episode-rawat-inap` |
| `FE-INP-11` | Sesi Koreksi | Detail Episode berstatus `Closed` | `episode-rawat-inap` |
| `FE-INP-14` | Pencatatan Kepergian | Detail Episode | `episode-rawat-inap` |
| `FE-INP-15` | Kebutuhan Isolasi | Detail Episode / alur admisi | `episode-rawat-inap` |
| `FE-INP-17` | Pembatalan Admisi | Daftar Kerja Episode / Detail Episode | `episode-rawat-inap` |
| `FE-INP-18` | Cetak Persetujuan | Alur admisi / Detail Episode | `episode-rawat-inap` |

### 3.3 Butir menu dua sub-modul yang belum dirancang

| Sub-modul | Butir menu tingkat dua | Rencana keterjangkauan |
|---|:---:|---|
| `keperawatan` | **Nol — ditetapkan 2026-09-02** | **Diputuskan saat sub-modul dirancang.** Keenam layarnya menjadi layar anak: `FE-KEP-01` s.d. `FE-KEP-05` dicapai dari Detail Episode `FE-INP-04` dan Census `FE-INP-01`; `FE-KEP-06` menjadi daftar ketiga di dalam Daftar Pantau `FE-INP-09`. Alasannya bukan sekadar kuota `IA-INP-05` yang penuh: pekerjaan perawat berputar pada satu pasien, bukan pada daftar dokumen. Rincian di `keperawatan/03-frontend-architecture.md` bagian 2 |
| `dokter-rawat-inap` | **Nol — ditetapkan 2026-09-02** | **Diputuskan saat sub-modul dirancang.** Kedelapan layarnya menjadi layar anak: `FE-DOK-01` s.d. `FE-DOK-07` dicapai dari Detail Episode `FE-INP-04` dan Census `FE-INP-01`; `FE-DOK-08` menjadi daftar tambahan di dalam Daftar Pantau `FE-INP-09`. Rincian di `dokter-rawat-inap/03-frontend-architecture.md` bagian 2 |

Keduanya **wajib** ditetapkan saat sub-modulnya dirancang, dan hasilnya **wajib** dituliskan
kembali ke berkas ini — bukan ke `03-frontend-architecture.md` sub-modulnya. Sidebar hanya satu.

### 3.4 Urutan migration lintas sub-modul

Yang dipegang di sini hanya **urutan antar sub-modul**. Urutan langkah **di dalam** sebuah
sub-modul tetap tinggal di `02-backend-architecture.md` sub-modul itu.

| Gelombang | Sub-modul | Yang dibuat | Prasyarat | Keadaan |
|---:|---|---|---|---|
| **M1** | `episode-rawat-inap` | 2 tabel master + 11 tabel transaksi berawalan `Inp`, index dan 4 unique index parsial, 13 `DbSet`, 6 service | Tidak ada | **Sudah dirancang**, rinciannya di `episode-rawat-inap/02-backend-architecture.md` §7 |
| **M2** | `episode-rawat-inap` | Perubahan **perilaku** `BedController.UpdateBedAvailability` | M1 selesai | Sudah dirancang. Sengaja paling akhir di dalam M1 karena satu-satunya perubahan perilaku pada modul lain |
| **M3** | `keperawatan` | **Nol tabel milik Rawat Inap.** Yang diminta kepada `ClinicalManagement`: 6 kolom pada `TrxPatientAssessment`, 4 tabel transaksi + 1 master baru, dan **satu pelonggaran validasi** (`INT-KEP-01`) | M1 selesai — pengkajian butuh episode sebagai konteks | **Dirancang 2026-09-02**, `draft` |
| **M4** | `dokter-rawat-inap` | **Nol tabel milik Rawat Inap.** Yang diminta: **satu** tabel baru `TrxPhysicianVisit` milik `ClinicalManagement`, ditambah kolom pada konsultasi, CPPT, tindakan, resep (`PharmacyManagement`), dan pesanan lab (`LaboratoryManagement`); serta **dua** pelonggaran validasi (`INT-DOK-01`, `INT-DOK-02`) | M1 selesai. **`INT-DOK-01` wajib dikerjakan bersama `INT-KEP-01` milik M3** | **Dirancang 2026-09-02**, `draft` |

**Temuan yang perlu dibaca sebelum menjadwalkan M3 dan M4.** Keduanya **tidak menambah satu tabel
pun ke modul ini**. Yang mereka butuhkan adalah perubahan di dalam `ClinicalManagement` dan
`PharmacyManagement` — *shared inpatient clinical context resolver*, `PRD-RWI-FINAL-001` bagian
30.3. Migration-nya karena itu **bukan milik modul Rawat Inap**, dan urutannya ditentukan pemilik
kedua modul itu. Persetujuannya sudah ada sejak `RWI-DEC-062`; penjadwalannya belum.

Akibat praktisnya: **tidak ada satu pun migration Rawat Inap yang tertahan** menunggu `keperawatan`
atau `dokter-rawat-inap`. Ketiga sub-modul dapat berjalan sendiri-sendiri, dan itulah gunanya
bentuk `COMPOSITE`.

---

## 4. Pemetaan kemampuan ke sub-modul

Sumbernya `RWI-DEC-083`. **28 kemampuan, 28 baris, nol kemampuan yatim.**

Kemampuan tanpa sub-modul pemilik dihitung sebagai **gap**, bukan sebagai pekerjaan yang belum
dijadwalkan — dan gap semacam itu tidak akan diperiksa berkas traceability mana pun, karena pada
bentuk `COMPOSITE` setiap `requirement-traceability.md` hanya memeriksa jatah sub-modulnya sendiri.

### 4.1 `episode-rawat-inap` — 16 kemampuan

| Kemampuan | ID | Nama pada PRD final | Keadaan pada blueprint |
|---|---|---|---|
| Rujukan dan daftar tunggu masuk | `CAP-001` | Referral & Waiting List | `DEFERRED` — ditunda setelah MVP, `04-prd-to-mvp.md` bagian 8 |
| Memilih atau mendaftarkan pasien | `CAP-002` | Admission Intake — Patient Identification | Dirancang, `EPIC RI-21` |
| Menentukan penjamin atau cara bayar | `CAP-003` | Admission Intake — Guarantor & Payment Context | Dirancang, `EPIC RI-21` |
| Menentukan DPJP | `CAP-004` | Admission Intake — DPJP Assignment | Dirancang, `EPIC RI-21` |
| Mencari tempat tidur tersedia | `CAP-005` | Bed Management | Dirancang, `EPIC RI-22`, `EPIC RI-34` |
| Mengunci, menempatkan, mengaktifkan episode | `CAP-006` | Admission Activation | Dirancang, `EPIC RI-22`, `EPIC RI-23` |
| Cetak kartu, gelang, label | `CAP-007` | Admission Documents | `DEFERRED` — ditunda setelah MVP |
| Census pasien dirawat | `CAP-008` | Inpatient Census | Dirancang, `EPIC RI-24` |
| Dokumen persetujuan, serah terima, edukasi | `CAP-009` | Patient Documents | `DEFERRED` — `DEC-INP-003`, pemilik privasi belum ditunjuk. Cetak tanpa simpan tersedia lewat `RWI-DEC-077` |
| Deposit, estimasi biaya, cek manfaat | `CAP-010` | Financial Preparation | `DEFERRED` — menunggu Billing operasional |
| Penugasan perawat penanggung jawab | `CAP-011` | Nurse Assignment | Dirancang, `EPIC RI-25` |
| Pindah kamar, tempat tidur, kelas | `CAP-017` | Patient Transfer | Dirancang, `EPIC RI-26` |
| Permintaan dan serah terima kamar operasi | `CAP-018` | Surgical Handoff | `DEFERRED` — `OperatingRoomManagement` berstatus `PLANNED` |
| Tagihan berjalan | `CAP-019` | Charge Review | `DEFERRED` — `BillingManagement` belum punya kemampuan transaksi |
| Resume medis atau resume pulang | `CAP-026` | Discharge Documentation | Dirancang, `EPIC RI-27`. **Tetap milik episode** walaupun ditulis DPJP — `AC-CAP026-02` mengikat koreksinya pada invariant episode, dan `InpDischargeSummary` sudah dimiliki Rawat Inap |
| Penutupan episode dan pelepasan tempat tidur | `CAP-028` | Episode Closure | Dirancang, `EPIC RI-28` |

### 4.2 `keperawatan` — 5 kemampuan

| Kemampuan | ID | Nama pada PRD final | Keadaan pada blueprint |
|---|---|---|---|
| Pengkajian awal dan pengkajian ulang keperawatan | `CAP-012` | Nursing Assessment | **Dirancang** 2026-09-02, `draft`. `MUST HAVE`, `EPIC KEP-01` dan `KEP-02` |
| Diagnosis, rencana asuhan, evaluasi keperawatan | `CAP-013` | Nursing Care | **Dirancang**, `draft`. `MUST HAVE`, `EPIC KEP-03`. Katalog SDKI tetap `OPEN DECISION` karena pemakaiannya belum dinyatakan |
| Catatan dan tindakan keperawatan | `CAP-014` | Nursing Interventions | **Dirancang**, `draft`. `MUST HAVE`, `EPIC KEP-04` |
| Pencatatan pemakaian alat | `CAP-016` | Equipment Usage | **`OPEN DECISION`** — kepemilikan tabelnya belum diputuskan; `EPIC KEP-06` **MUST NOT** masuk gelombang pengiriman. Lihat bagian 2.4 |
| Asuhan gizi | `CAP-027` | Nutrition Care | **Dirancang sebagian**, `draft`. `DEFERRED` ke `POST-MVP`: skrining gizi berjalan penuh, rujukan menunggu modul Gizi berdiri |

### 4.3 `dokter-rawat-inap` — 7 kemampuan

| Kemampuan | ID | Nama pada PRD final | Keadaan pada blueprint |
|---|---|---|---|
| Pemeriksaan penunjang — laboratorium dan radiologi | `CAP-015` | Supporting Services | **Dirancang sebagian**, `draft`. **Laboratorium masuk MVP** — modulnya ada dan `LabOrder` tidak punya gerbang antrean. **Radiologi `DEFERRED`** — modulnya tidak ada di repository |
| Dokumentasi SOAP | `CAP-020` | Clinical Documentation — SOAP | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-03`. SOAP **sudah ada** di dalam `TrxDoctorConsultation` |
| CPPT | `CAP-021` | Clinical Documentation — CPPT | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-04`. Ditulis bersama `keperawatan`; kontraknya milik sub-modul ini |
| Kajian medis awal | `CAP-022` | Medical Assessment | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-02`. Struktur tabelnya menunggu persetujuan pemilik |
| Resep rawat inap dan obat pulang | `CAP-023` | Medication Management | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-06`. Mesin Farmasi **sudah lengkap** |
| Tindakan dokter | `CAP-024` | Physician Procedures | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-06` |
| Pencatatan visite dokter | `CAP-025` | Physician Visit | **Dirancang**, `draft`. `MUST HAVE`, `EPIC DOK-05`. **Satu-satunya tabel yang benar-benar baru** di seluruh dua sub-modul klinis. `RWI-AC-032` s.d. `034`, `047`, `048` menemukan tempatnya di sini |

### 4.4 Pemeriksaan kemampuan yatim

| Pemeriksaan | Hasil |
|---|---|
| Jumlah kemampuan pada baseline `PRD-RWI-FINAL-001` | 28 — `CAP-001` s.d. `CAP-028` |
| Jumlah baris pada bagian 4.1 + 4.2 + 4.3 | 16 + 5 + 7 = **28** |
| Kemampuan tanpa sub-modul pemilik | **Nol** |
| Kemampuan yang dimiliki lebih dari satu sub-modul | **Nol.** `CAP-021` dibaca dan ditulis dua profesi, tetapi pemilik kontraknya tetap satu: `dokter-rawat-inap` |
| Kemampuan `DEFERRED` | 6 — `CAP-001`, `CAP-007`, `CAP-009`, `CAP-010`, `CAP-018`, `CAP-019`. Seluruhnya **punya sub-modul pemilik** dan karena itu bukan gap |

**Selisih penomoran yang perlu diketahui pembaca.** Blueprint revision `1` s.d. `4` memakai deret
`RWI-CAP-###` dari `01-existing-capability-map.md`, sedangkan baseline baru memakai `CAP-###` dari
PRD final. Keduanya **bukan** deret yang sama dan tidak boleh disamakan begitu saja. Berkas ini
memakai deret `CAP-###` karena `RWI-DEC-080` menjadikan PRD final sebagai baseline; rujukan
`RWI-CAP-###` pada dokumen sub-modul tetap sah untuk membaca dokumen itu sendiri.

---

## 5. Traceability

| Bagian berkas ini | Sumbernya |
|---|---|
| Registry sub-modul | `RWI-DEC-082` |
| Kepemilikan data — modul lain dan modul ini | `episode-rawat-inap/02-backend-architecture.md` bagian 2 revision `0.4` |
| Kepemilikan data — dokumentasi klinis | `RWI-DEC-081`, `RWI-RULE-026` aturan 1 |
| Kepemilikan data — kelayakan keuangan | `RWI-OQ-047` — **terbuka**, ditulis apa adanya |
| Peta butir menu | `episode-rawat-inap/03-frontend-architecture.md` bagian 2B, `episode-rawat-inap/05-skema-tampilan.md` bagian 2 |
| Urutan migration | `episode-rawat-inap/02-backend-architecture.md` bagian 7.1 |
| Pemetaan kemampuan | `RWI-DEC-083`, `PRD-RWI-FINAL-001` bagian 10 dan 23.1 |

## 6. Yang menahan berkas ini menjadi `approved`

| Butir | Menahan | Pemilik jawaban |
|---|---|---|
| `RWI-OQ-047` sumber kebenaran kelayakan keuangan | **Satu baris** pada bagian 2.4 | Product/Domain bersama pemilik `BillingManagement` |
| **`RWI-OQ-048`** kepemilikan catatan pemakaian alat — **baru 2026-09-02** | **Satu baris** pada bagian 2.4, dan `EPIC KEP-06` pada `keperawatan/04-prd-to-mvp.md` | Product/Domain bersama pemilik persediaan |
| ~~Butir menu `keperawatan`~~ | **Tertutup 2026-09-02** — nol butir menu tingkat dua; keenam layarnya menjadi layar anak | — |
| ~~Butir menu `dokter-rawat-inap`~~ | **Tertutup 2026-09-02** — nol butir menu tingkat dua |
| **Urutan daftar di dalam `FE-INP-09` Daftar Pantau** — baru 2026-09-02 | Satu layar kini dipakai tiga sub-modul: 4 daftar `episode-rawat-inap`, 1 `keperawatan`, 1 `dokter-rawat-inap` | Ditetapkan saat salah satu daftar baru dikerjakan; **tidak boleh** diputuskan sendiri-sendiri |

Tidak satu pun menahan pekerjaan `episode-rawat-inap`, dan `RWI-OQ-048` tidak menahan ketiga
kemampuan `MUST HAVE` milik `keperawatan`.
