# Sub-modul `dokter-rawat-inap` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `dokter-rawat-inap` |
| `judul` | Dokter Rawat Inap |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | `5` — satu angka, dipegang tingkat modul |
| `status` | **`draft`** — belum disetujui manusia; status sub-modul tidak berubah oleh impact scan |
| `artifact_readiness` | **`CURRENT`** — sembilan artefak diamendemen ke revision `0.3` / kontrak `0.3.0` pada 2026-09-02 menyerap `RWI-DEC-086` s.d. `RWI-DEC-088`; empat berkas lain sengaja tetap `0.2` karena isinya tidak bergerak. Terikat `BE@93b3227` dan `FE@863f24b` |
| `delivery_readiness` | **`PARTIAL`** — amendment desain selesai dan **nol pertanyaan memblokir tersisa**. Yang tertinggal hanya approval manusia; planning boleh berjalan begitu approval turun |
| `last_focused_impact_scan` | `2026-09-02`; [`../01-existing-capability-map.md`](../01-existing-capability-map.md) bagian 15 |
| `impact_scan_source_sha` | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `last_focused_requirement_gate` | Revision `1.3`, 2026-09-02; seluruh tujuh capability `READY_FOR_DOMAIN_DESIGN`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085` |
| `domain_architecture` | Revision `0.2`, 2026-09-02, **`DOMAIN_ARCHITECTURE_READY`** untuk ketujuh capability. [`../evidence/03-hospital-domain-architecture.md`](../evidence/03-hospital-domain-architecture.md) Bagian Kedua, bagian O s.d. AB |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###`, deret bersama seluruh modul |
| `approved_by` | — belum ada yang disetujui |
| `approved_at` | — |
| `rumpun kemampuan` | Dokumentasi dokter — kajian medis, SOAP, CPPT, tindakan, visite, resep, dan penunjang |
| `kemampuan` | **7** — `CAP-015`, `CAP-020` s.d. `CAP-025`, sesuai `RWI-DEC-083` |
| `uji pemecahan` | **3/5** syarat `bentuk-blueprint.md` bagian 4.1, sebagaimana dicatat `RWI-DEC-082` |
| `peran pemilik` | DPJP dan dokter jaga ruangan |

---

## 0. Kenapa `draft` dan bukan `BLOCKED`

`bentuk-blueprint.md` bagian 6 gerakan ③ menyatakan sub-modul yang **batas kepemilikan datanya belum
diputuskan** lahir berstatus `BLOCKED`. Sub-modul ini **tidak** dalam keadaan itu.

| Hal | Keadaannya | Sumbernya |
|---|---|---|
| Kepemilikan tabel dokumentasi klinis | **Sudah diputuskan** — milik `ClinicalManagement`. Rawat Inap tidak membuat tabel tandingan | `RWI-DEC-081` |
| Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` | **Sudah diberikan** 2026-08-21; menutup `RWI-OQ-032` dan `DEC-INP-001` | `RWI-DEC-062` |
| Kemampuan yang menjadi jatah sub-modul ini | **Sudah dipetakan**, nol kemampuan yatim | `RWI-DEC-083` |
| Masuknya dokumentasi klinis ke dalam scope modul | **Sudah diputuskan** 2026-09-02 | `RWI-DEC-080` |
| Yang benar-benar tersisa | Pekerjaan desain yang belum dikerjakan, ditambah satu penghalang **teknis**: *shared inpatient clinical context resolver* | `PRD-RWI-FINAL-001` bagian 30.3 |

**`BLOCKED` berarti menunggu orang. `draft` berarti menunggu pekerjaan.**

> **Diperbarui setelah impact scan 2026-09-02.** Desainnya sudah pernah dikerjakan, tetapi
> **seluruh artefaknya kini `STALE`** terhadap source terbaru. Yang tersisa bukan sekadar approval:
> amendment wajib menyerap keberadaan Radiologi, konflik consumer frontend berbasis antrean rawat
> jalan, defect jalur konsultasi tanpa antrean, serta status reuse/adapter/extension/missing pada
> tujuh kemampuan. `INT-DOK-01` masih `Missing` dan `INT-DOK-02` masih `Extend`.
>
> **Berbeda dari `keperawatan`, sub-modul ini tidak menyisakan satu pun `OPEN DECISION`
> kepemilikan.** Ketujuh kemampuannya punya pemilik data yang tegas pada `PRD-RWI-FINAL-001`
> bagian 23.1.

---

## 1. Kemampuan yang dimiliki sub-modul ini

| Kemampuan | ID | Nama pada `PRD-RWI-FINAL-001` |
|---|---|---|
| Pemeriksaan penunjang — laboratorium dan radiologi | `CAP-015` | Supporting Services |
| Dokumentasi SOAP | `CAP-020` | Clinical Documentation — SOAP |
| CPPT | `CAP-021` | Clinical Documentation — CPPT |
| Kajian medis awal | `CAP-022` | Medical Assessment |
| Resep rawat inap dan obat pulang | `CAP-023` | Medication Management |
| Tindakan dokter | `CAP-024` | Physician Procedures |
| Pencatatan visite dokter | `CAP-025` | Physician Visit |

`CAP-021` CPPT memang ditulis lintas profesi bersama `keperawatan`. Itu **sifat** CPPT, bukan tabrakan kepemilikan: pemilik kontraknya tetap satu, yaitu sub-modul ini, sesuai `RWI-DEC-083`.

Pemetaan lengkap ke-28 kemampuan modul ada di
[`../02-module-map.md`](../02-module-map.md) bagian 4.

---

## 2. Kepemilikan data

**Sub-modul ini tidak memiliki satu tabel pun.** `RWI-DEC-081` menetapkan seluruh tabel dokumentasi
klinis rawat inap — pengkajian, CPPT, SOAP, kajian medis, resep, dan tindakan — dimiliki
`ClinicalManagement`. Rawat Inap hanya menyediakan **workspace, konteks episode, dan kontrak**.

| Yang dimiliki sub-modul ini | Yang dipakai dari modul lain |
|---|---|
| Nol tabel. Nol migration. Nol `DbSet` | `ClinicalManagement` untuk kajian medis, SOAP, CPPT, tindakan, dan visite; `PharmacyManagement` untuk resep; modul Laboratory dan Radiology untuk penunjang |

Baris kepemilikan datanya dibaca di [`../02-module-map.md`](../02-module-map.md) bagian 2.3, bukan
di `02-backend-architecture.md` sub-modul ini.

> **Larangan yang mengikat sejak hari pertama:** sub-modul ini **MUST NOT** membuat tabel tandingan
> untuk kemampuan di atas. Bila kelak desainnya terasa menuntut tabel baru, yang benar adalah
> kembali ke `/qv-grill`, bukan membuatnya diam-diam. Aturan ini diwariskan `RWI-DEC-081`.

---

## 3. Daftar artefak dan hash

Seluruh artefak sudah **diamendemen** pada 2026-09-02 menyerap arsitektur domain revision `0.2`,
keberadaan modul Radiologi, keputusan visite `RWI-DEC-084` dan `RWI-DEC-085`, defect jalur tanpa
antrean, serta konflik consumer frontend. Revision artefak naik `0.1` → `0.2` dan seluruh kontrak
naik `0.1.0` → `0.2.0`. Revision blueprint tingkat modul **tidak** ikut naik; itu wewenang manifest
modul.

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.3` | `draft` | `5f8abd017537bbd70a686355e1658d290ad659c9ff1a5678adc50d3deaaf5cce` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.3` | `draft` | `d71573a191629027fa5a7ee4ac73fabc6568973f07e676dcffdb54684d793c90` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.3` | `draft` | `b361ce6c7175fb4f8bbb49340c1814424cf07df71f30629a740e4ee540ed36dc` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.2` | `draft` | `d0638fd918123d1b84c75afbd8658abb1df080a5bbb8a7e6cfd18df4e212d831` |
| [`flowcharts/01-catatan-harian-dan-cppt.md`](./flowcharts/01-catatan-harian-dan-cppt.md) | `0.2` | `draft` | `cf523ca2079c4750b66cf248e5feac2bddbe802e37ea216d562d03555424b105` |
| [`flowcharts/02-visite-dokter.md`](./flowcharts/02-visite-dokter.md) | `0.2` | `draft` — **berkas baru** | `0aac5416e7e137975679432145d34e9cf2b0bdb32111307d4d6576814f693502` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.2` | `draft` | `f1a21de72b5ceb40dec2750c83732d9e77f2d91cf02d2efa4fd803ebdea8b302` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.3.0` | `draft` | `7239e375dd2d5f1d63b0336f696c9c3fd21dbfbd96a642b77927bca066d7e7cb` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.3.0` | `draft` | `97664b2829c46e9d1006fa265a5b09b22c8254054637c2d3b50c2a94d35e4e68` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.3.0` | `draft` | `1d3405eaeb1b7095dccdc369dfaf9ea4a1e12d1e5d15ab4810d4a48133349541` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.3.0` | `draft` | `6dadaed67ada54cfdf2bf5fdff66b1065da297c9bd6a2a105cbb84d18b730939` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.3.0` | `draft` | `5980b6b72a7bf6f16e077e8b1bae68ed1a12b8cabea2ef9c22fd1ec995855d9d` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.3.0` | `draft` | `b3f31aaa1c39708e9dcbfcf701ed47e0827fab07f9b5084ff2e263cc6d011d59` |

**Empat berkas sengaja tetap pada revision `0.2`** — ketiga flowchart dan kamus data — karena
`RWI-DEC-086` s.d. `RWI-DEC-088` tidak menggerakkan isinya: alurnya sudah menyebut koreksi lewat
penulis atau penulis pengganti, dan ketiga keputusan itu **nol perubahan bentuk data**. Menyuntingnya
hanya untuk menaikkan angka justru menyesatkan pembaca berikutnya.

Berkas per proses pada `flowcharts/` bertambah dari satu menjadi dua. `01-catatan-harian-dan-visite.md`
**berganti nama** menjadi `01-catatan-harian-dan-cppt.md`, dan proses visite dipindahkan ke berkas
tersendiri `02-visite-dokter.md` karena `RWI-DEC-084` menjadikannya kejadian yang berdiri sendiri
dengan pemicu, pelaku, jalur koreksi, dan cara menghitung sendiri.

`roadmap/` dan `task/report/` belum ada, dan **itu bukan penyimpangan struktur**: keduanya ditulis
`/qv-plan` dan kedua skill build, bukan oleh skill desain.

### 3.1 Yang diserap amendment `0.2`

| Kelompok artefak | Temuan yang diserap | Bagaimana diserap |
|---|---|---|
| Backend, data, API, integration | Radiologi tersedia; konteks klinis episode belum ada; jalur tanpa antrean gagal; multiplicity belum sesuai; visite belum ada | Grup Radiologi masuk kontrak; `INT-DOK-01` beserta perbaikan jalur tanpa antrean menjadi gelombang nol; entity visite lahir sebagai `CliPhysicianVisit` |
| Penamaan entity | `TrxPhysicianVisit` melanggar `QBE-NAM-001` | Diganti `CliPhysicianVisit` memakai prefix registry `Cli` yang berstatus `ACTIVE` |
| Koreksi dokumen | Kolom amandemen per tabel diusulkan padahal mesinnya sudah ada | Enam kolom dicabut; koreksi memakai mesin addendum `MedicalRecordManagement` |
| Frontend dan flow | Ruang kerja ter-commit memakai antrean rawat jalan, aksi panggil/lewati/tidak hadir, dan butir menu tingkat dua | Layar `FE-DOK-01` berstatus `Conflict`; sumber daftar dipindah ke census episode; butir menu di bawah "Dokter" dicabut; gelombang `DOK-MVP-FE` menahan rilis |
| Keputusan visite | `RWI-DEC-084` dan `RWI-DEC-085` turun setelah `0.1` ditulis | Mesin status `Recorded`/`Cancelled`, kunci permintaan wajib, koreksi berbentuk batal-lalu-catat-ulang, dan hitungan per kejadian |
| PRD dan acceptance | Radiologi dinyatakan tidak ada; bukti otomatis nihil | Radiologi naik menjadi `MUST HAVE`; tujuh acceptance `RWI-AC-150` s.d. `RWI-AC-156` dan regresi `RWI-AC-143` masuk matriks |

### 3.2 Yang masih sama seperti `0.1`

| Hal | Kenapa tidak berubah |
|---|---|
| Nol tabel milik Rawat Inap | `RWI-DEC-081` tidak berubah |
| Pilihan berbagi tabel untuk kajian medis | Tetap dipilih, tetapi keberatannya diperkuat bukti isi tabel yang bercorak keperawatan |
| Batas `RUL-DOK-01` dan `RUL-DOK-02` | Batas kepemilikan tidak bergerak; hanya penomorannya yang dirapikan |

### 3.3 Amendment Pass koreksi dokumen — **sudah diserap revision `0.3`**

`RWI-DEC-086` s.d. `RWI-DEC-088` turun **sesudah** ketiga belas artefak `0.2` selesai ditulis.
Seluruh dampaknya sudah diserap revision `0.3` pada hari yang sama; tabel di bawah dipertahankan
sebagai jejak apa yang berubah dan kenapa.

| Artefak | Yang diselaraskan | Sifat |
|---|---|---|
| `02-backend-architecture.md` bagian 4.9 | Menyebut mesin koreksi dokumen "dipakai apa adanya" dan menyimpulkan **nol** perubahan. Yang benar: mesinnya memang tidak berubah, tetapi **tiga jenis dokumen wajib didaftarkan** ke sana saat finalisasi — `RWI-DEC-087` | Tambahan pekerjaan, bukan pembatalan rancangan |
| `contracts/state-transition-matrix.md` bagian 1 dan 6 | Perpindahan ke `Completed` kini **sekaligus** mendaftarkan dokumen ke mesin keutuhan dan menguncinya sebagai tertanda tangan | Penajaman, bukan perubahan nilai status |
| `contracts/validation-matrix.md` | Perlu satu aturan: koreksi pada catatan yang **belum** final ditolak, dengan pesan yang mengarahkan menyunting langsung — `RWI-AC-159` | Aturan baru |
| `contracts/api-contract.md` bagian 9 dan `contracts/permission-audit-matrix.md` | Endpoint koreksi **atas nama penulis lain** beserta hak aksesnya belum tercatat, padahal sudah ada di source. Ditambah pemetaan peran baru dari `RWI-DEC-088`: DPJP aktif memegang kewenangan pengganti, kepala unit rawat inap menerbitkan penetapan berhalangan | Kelengkapan yang terlewat |
| `contracts/permission-audit-matrix.md` bagian 3 | **Satu baris kewenangan per pasien yang baru.** Penetapan berhalangan pada mesin yang ada bersifat **milik penulis**, bukan milik penggantinya — ia tidak menyebut siapa yang boleh menggantikan. Pembatasan "hanya DPJP aktif episode itu" karena itu **tidak dapat dijaga** mesin hak akses maupun mesin penetapan, dan wajib menjadi penjaga kewenangan per pasien sesuai `INV-DOK-13` | **Batas baru yang wajib dirancang penempatannya** |
| `04-prd-to-mvp.md` bagian 20.2 | Pertanyaan memblokir nomor 5 **tertutup**, dan arah jawabannya berbeda dari dugaan semula. Gelombang pengiriman perlu memuat pendaftaran tiga jenis dokumen | Penutupan blocker |
| `testing/acceptance-test-matrix.md` | Enam acceptance baru `RWI-AC-157` s.d. `RWI-AC-162` belum masuk matriks | Tambahan skenario |

**Tidak satu pun dampak di atas membatalkan keputusan desain `0.2`.** Seluruhnya menambah atau
menajamkan. Pencabutan enam kolom amandemen tetap benar; yang keliru hanya anggapan bahwa mesinnya
sudah tersambung untuk keempat jenis dokumen.

**Satu batas baru lahir dari penyerapan ini**, dan ia tidak dapat dijaga mesin mana pun: penetapan
berhalangan bersifat milik penulis, sehingga pembatasan `RWI-DEC-088` bahwa hanya DPJP aktif episode
itu yang boleh mengoreksi wajib menjadi penjaga kewenangan per pasien. Tercatat sebagai `VAL-DOK-35`
dan diuji `RWI-AC-167`, dengan catatan tegas bahwa pemeriksaan hak aksesnya justru **lolos** — yang
menolak adalah aturan bisnis.

---

## 4. Contract version

`contract_versions` sub-modul ini: **`0.2.0`**.

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.2.0` | `0.2.0` | `draft` — grup Radiologi ditambah; tiga endpoint amandemen dicabut; endpoint sunting visite diganti batalkan dan tautkan |
| State transition | `0.2.0` | `0.2.0` | `draft` — mesin event visite lahir; status `Amended` dicabut; nilai status tindakan diselaraskan dengan enum sebenarnya |
| Validation | `0.2.0` | `0.2.0` | `draft` — enam aturan baru `VAL-DOK-26` s.d. `VAL-DOK-31`; nol aturan dicabut |
| Integration | `0.2.0` | `0.2.0` | `draft` — penomoran mengikuti arsitektur domain; `INT-DOK-07` integritas dokumen lahir; perbaikan jalur tanpa antrean masuk `INT-DOK-01` |
| Permission dan audit | `0.2.0` | `0.2.0` | `draft` — Action `Cancel` ditambah, Action `Amend` dicabut, Resource `RadOrder` masuk peta peran |
| Acceptance test | `0.2.0` | `0.2.0` | `draft` — 43 skenario, 17 di antaranya jalur gagal; bukti otomatisnya sendiri masih nol dan itulah `ARCH-GAP-016` |

Angka ini bergerak **sendiri**, terpisah dari `contract_versions` milik `episode-rawat-inap` yang
sudah berada di `0.4.0`. Itulah gunanya bentuk `COMPOSITE`: satu sub-modul boleh maju tanpa menunggu
yang lain.

---

## 5. Dependency sub-modul ini

| Bergantung pada | Untuk apa | Keadaan |
|---|---|---|
| `episode-rawat-inap` | Episode sebagai **konteks** setiap dokumen: siapa pasiennya, di mana dirawat, siapa penanggung jawabnya, dan apakah episodenya masih hidup | **Tersedia** — `approved` 2026-08-24. Sub-modul ini **membaca**, tidak menulis |
| `ClinicalManagement` | Tabel dan mesin dokumentasi klinis | **Disetujui** `RWI-DEC-062`; **belum dikerjakan** — butuh *shared inpatient clinical context resolver* |
| `Corporate HR Workforce` | Identitas penulis dokumen | Tersedia |

Arah ketergantungannya satu arah: sub-modul ini butuh `episode-rawat-inap`, tetapi
`episode-rawat-inap` **tidak** butuh sub-modul ini. Karena itu tidak ada satu pun task
`episode-rawat-inap` yang tertahan menunggu folder ini terisi.

### 5.1 Dependency gate setelah impact scan

Status berikut memakai taksonomi audit kanonis dan terikat pada SHA impact scan di metadata.

| Dependency / capability | Evidence | Status | Dampak | Kelanjutan independen |
|---|---|---|---|---|
| Konteks episode, census, dan DPJP | `DOK-TRC-CTX-01` | `Ready to reuse` | Fondasi tersedia | Dapat langsung diserap dalam amendment |
| `INT-DOK-01` resolver konteks klinis | `DOK-TRC-INT-01` | `Missing` | Menahan create SOAP/kajian medis rawat inap | Desain kontrak dan acceptance masih dapat diamendemen |
| Defect konsultasi tanpa antrean | `DOK-TRC-DEF-01` | `Repair` | Risiko HTTP 500 dan regresi IGD | Amendment dapat menentukan batas repair |
| `INT-DOK-02` multiplicity konsultasi/resep | `DOK-TRC-INT-02` | `Extend` | Menahan catatan dan resep berulang sepanjang episode | Amendment dapat mengunci scope Inpatient/Emergency |
| Lab dan Radiologi | `DOK-TRC-CAP015` | `Extend` | Kontrak penunjang target tidak lagi benar | Fondasi order dapat dipakai ulang setelah adapter episode ditetapkan |
| SOAP | `DOK-TRC-CAP020` | `Repair` | Create rawat inap belum aman | Infrastruktur SOAP/integrity/addendum dapat dipakai ulang |
| CPPT | `DOK-TRC-CAP021` | `Extend` | Verifikasi DPJP dan konteks episode belum ada | Model dan integrity/addendum dapat dipakai ulang |
| Kajian medis awal | `DOK-TRC-CAP022` | `Reuse with adapter` | Membutuhkan resolver, discriminator, SLA, dan authority | Pilihan reuse perlu dipastikan sebelum approval |
| Resep | `DOK-TRC-CAP023` | `Extend` | Resep kedua dan obat pulang belum terwakili | Mesin Pharmacy yang ada tetap menjadi fondasi |
| Tindakan dokter | `DOK-TRC-CAP024` | `Extend` | Belum terikat episode/visite/DPJP | Planned/executed dan billing fact dapat dipakai ulang |
| Visite dokter | `DOK-TRC-CAP025` | `Missing` | Capability belum punya persistence, API, permission, consumer, atau test | Desain target tetap dapat diamendemen |
| Workspace frontend | `DOK-TRC-FE-01` | `Conflict` | **Menahan sign-off, planning frontend, dan rilis** | Base component `DOK-TRC-FE-BASE` dapat dipakai dengan adapter |
| Authorization episode/DPJP | `DOK-TRC-AUTH-01` | `Extend` | Permission generik belum membuktikan kewenangan pasien | Permission engine dan `IsActiveDoctorAsync` dapat dipakai ulang |
| Bukti otomatis | `DOK-TRC-VER-01` | `Missing` | Menahan klaim kesiapan implementasi/rilis | 26 test fondasi terarah lulus; bukan bukti end-to-end dokter |

---

## 6. Yang harus dilakukan sebelum sub-modul ini dapat disetujui atau direncanakan

Kolom **Keadaan** memakai tiga nilai: `Selesai` bila amendment desain sudah menyelesaikannya,
`Terbuka` bila masih menunggu orang, dan `Implementasi` bila desainnya selesai tetapi kodenya
belum ada.

| No | Butir | Pemilik | Keadaan | Memblokir? |
|---:|---|---|---|:---:|
| 1 | Bentuk konteks klinis episode — bagaimana dokumen klinis menemukan perawatan yang benar tanpa antrean semu | Pemilik `ClinicalManagement` lewat `RWI-DEC-062` | **`Implementasi`** — bentuknya sudah dirancang sebagai satu service bersama, `INT-DOK-01` | **Ya untuk planning/build**; tidak lagi menahan desain |
| 2 | Batas waktu klinis `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | `Terbuka` | **Tidak untuk desain** — parameter tersedia tanpa angka; tetap menahan sign-off produksi |
| 3 | Konflik ruang kerja frontend ter-commit | Frontend authority | **`Implementasi`** — bentuk targetnya dikunci `03-frontend-architecture.md` bagian 0 dan 3.1.1, dan gelombang `DOK-MVP-FE` menahan rilis | **Ya** — tidak boleh di-sign-off atau dirilis dalam bentuk sekarang |
| 4 | Pelonggaran batas satu catatan per kunjungan dan satu resep aktif (`INT-DOK-02`) | `ClinicalManagement`, `PharmacyManagement` | **`Implementasi`** — `approved` sejak `RWI-DEC-038`, kodenya belum ada | **Ya** — tanpanya dokter hanya dapat menulis satu catatan dan satu resep untuk seluruh masa perawatan |
| 5 | Kajian medis memakai ulang tabel pengkajian atau bentuk penyimpanan tersendiri | Product/Domain bersama pemilik `ClinicalManagement` | `Terbuka` — desain memilih pakai ulang beserta keberatannya | **Tidak** — bila berubah, yang bergerak hanya arsitektur dan kamus data |
| 6 | Serap keberadaan modul Radiologi ke seluruh artefak | Pemilik desain sub-modul | **`Selesai`** — arsitektur, kontrak API, integrasi, kamus data, flow, PRD, dan acceptance sudah memuatnya | Tidak |
| 7 | Definisi dan hitungan Physician Visit | Product/Domain owner | **`Selesai`** — `RWI-DEC-084`, `RWI-DEC-085`, `DEC-INP-008 CLOSED`, dan seluruhnya sudah diserap desain | Tidak |
| 8 | **Perbaikan jalur tanpa antrean** yang hari ini berujung kegagalan sistem | `ClinicalManagement` | **`Implementasi`** — dirancang sebagai gelombang `DOK-MVP-0` | **Ya untuk rilis** — menyentuh pasien rawat inap dan IGD sekaligus |
| 9 | ~~Jaminan bahwa dokumen terkunci tetap menerima koreksi~~ | Pemilik `MedicalRecordManagement` | **`Selesai`** — dijawab source: koreksi **hanya** diterima pada dokumen terkunci. Sambil menjawabnya ditemukan celah yang lebih serius, ditutup `RWI-DEC-086` dan `RWI-DEC-087` | ~~Ya~~ → Tidak |
| 11 | **Pendaftaran tiga jenis dokumen ke mesin keutuhan saat finalisasi** | `ClinicalManagement` | **`Implementasi`** — dirancang sebagai gelombang `DOK-MVP-0b` | **Ya untuk rilis** — tanpanya catatan final tidak dapat dikoreksi sama sekali |
| 10 | Baris registry `RadiologyManagement / Rad` masih `PLANNED` padahal entity-nya sudah ada | Pemilik registry | `Terbuka` | Tidak — penambahan kolom pada entity yang sudah ada tidak terhalang |

Butir 1, 2, dan 8 **tidak** menahan `episode-rawat-inap`.

**Butir 9 adalah satu-satunya pertanyaan memblokir yang lahir dari amendment ini**, dan ia lahir
justru karena desain berhenti membuat jalur koreksi sendiri lalu memakai mesin yang sudah ada.

---

## 7. Fase lifecycle dan langkah berikutnya

| Fase | Status | Bukti / syarat keluar |
|---|---|---|
| Capability audit terbaru | `DONE` | Peta kemampuan revision `1.3`, bagian 15 |
| Scoped impact review Dokter Rawat Inap | `DONE` | Seluruh klaster target dan consumer frontend sudah diklasifikasikan |
| Focused requirement gate | `DONE` | Evidence revision `1.3`; seluruh capability dokter siap domain design |
| Domain amendment tujuh capability | **`DONE`** | Arsitektur domain revision `0.2`, Bagian Kedua bagian O s.d. AB; hasil `DOMAIN_ARCHITECTURE_READY` untuk `CAP-015` dan `CAP-020`–`CAP-025` |
| Keputusan `CAP-025` | **`DONE`** | `RWI-DEC-084` dan `RWI-DEC-085`; `DEC-INP-008 CLOSED` |
| Amendment blueprint penuh | **`DONE`** | Revision `0.2` menyerap arsitektur domain; revision `0.3` menyerap `RWI-DEC-086` s.d. `RWI-DEC-088`. Sembilan artefak pada `0.3`, empat sengaja tetap `0.2` |
| Approval manusia | **`READY`** | Artefak sudah `CURRENT`. Pemilik dapat meninjau dan menyetujui; approval tetap tindakan manusia |
| Delivery planning | `BLOCKED` | Menunggu **satu** hal: approval pemilik. Pertanyaan memblokir sudah nol |
| Build / release | `BLOCKED` | Belum ada roadmap/task yang disetujui; source capability dan bukti otomatis belum lengkap |

| Kondisi | Skill |
|---|---|
| ~~Requirement gate focused dan keputusan visite selesai~~ | ~~`hospital-domain-architect` amendment~~ — **selesai 2026-09-02** |
| ~~Domain architecture ketujuh capability siap~~ | ~~`design-business-module` amendment~~ — **selesai 2026-09-02** |
| ~~Pertanyaan memblokir butir 9 ingin ditutup~~ | ~~`/qv-grill` Amendment Pass~~ — **selesai 2026-09-02**, ditutup `RWI-DEC-086` s.d. `RWI-DEC-088` |
| Pemilik klinis sudah ditunjuk dan `RWI-RULE-021` ingin ditutup | `/qv-grill` Amendment Pass |
| Amendment selesai, kontrak current, dan owner menyetujui | `plan-module-delivery` untuk sub-modul ini |

Sub-modul ini **MUST NOT** diteruskan ke `plan-module-delivery` dalam keadaan sekarang, dan kini
tinggal **satu** alasannya: approval pemilik belum ada. Artefaknya `CURRENT`, kontraknya `0.3.0`,
dan pertanyaan memblokirnya nol. Handoff yang sah sekarang adalah **peninjauan dan approval
manusia**.
Revision blueprint dan versi kontrak baru ditentukan setelah perubahan target material benar-benar
diserap; pembaruan status ini sendiri tidak menaikkan keduanya.

---

## 8. Handoff contract — amendment desain setelah arsitektur domain siap

| Field | Nilai |
|---|---|
| `next_owner_ready_slice` | **Peninjauan dan approval pemilik** atas 13 artefak desain revision `0.2`; setelah disetujui dan butir 9 dijawab, `plan-module-delivery` |
| `next_owner_blocked_slice` | — tidak ada blocker keputusan bisnis pada scope Dokter Rawat Inap |
| `blueprint_id` / `revision` | `RWI-BP-001` / `5` |
| `current_phase` | Requirement gate `DONE`; domain amendment `DONE`; amendment blueprint penuh `DONE` pada revision `0.3`; **approval manusia `READY`, nol pertanyaan memblokir** |
| `ready_capability_scope` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024`, `CAP-025` |
| `blocked_capability_scope` | — |
| `requirement_readiness` | **`READY_FOR_DOMAIN_DESIGN`** untuk seluruh scope Dokter Rawat Inap |
| `decision_readiness` | Terkunci: `RWI-DEC-038`, `062`, `070`, `080`–`085`; `DEC-INP-001 CLOSED`; `DEC-INP-008 CLOSED` |
| `contract_readiness` | Seluruh kontrak naik ke **`0.3.0`** berstatus `draft` dan `CURRENT` terhadap `BE@93b3227` serta `FE@863f24b`; belum boleh dipakai planning sebelum disetujui |
| `dependency_readiness` | `PARTIAL`; rincian dan status kanonis ada pada bagian 5.1 |
| `approval_status` | `draft`, `approved_by: null`, `approved_at: null` |
| `input_hash_decisions` | `00-interview-decisions.md` **revision `10`**: `de786bebc169636c0d7bd254d429a0209809890d78a7f1dcd8220d303fcbecc0` — inilah revisi yang diserap artefak desain `0.3`. Artefak `0.2` sebelumnya berdiri di atas revision `8` |
| `decisions_revision_terbaru` | Revision `10`: `de786bebc169636c0d7bd254d429a0209809890d78a7f1dcd8220d303fcbecc0`. **Sudah diserap** artefak revision `0.3` — rinciannya pada bagian 3.3 |
| `input_hash_capability_map` | `01-existing-capability-map.md` revision `1.3`: `0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03` |
| `input_hash_requirement_gate` | `evidence/02-requirement-completeness-gate.md` revision `1.3`: `883ed59b48bc10cb2ee9b2e09900c470a63bad9d06a339613aa871d308a70ade` |
| `input_hash_domain_architecture` | `evidence/03-hospital-domain-architecture.md` revision `0.2`: `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| `source_sha` | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `expected_output` | Keputusan approval pemilik atas revision `0.2`, ditambah jawaban pertanyaan memblokir bagian 6 butir 9. Setelah keduanya ada, keluaran berikutnya adalah roadmap dan task dari `plan-module-delivery` |

### 8.1 Acceptance handoff

**Handoff menuju `design-business-module` sudah dijalankan dan selesai pada 2026-09-02.** Kelima
syaratnya terpenuhi: arsitektur domain mencakup ketujuh capability tanpa tabel tandingan, ownership
dan lifecycle-nya lengkap, Physician Visit mengikuti `RWI-DEC-084` dan `RWI-DEC-085` termasuk
pemisahan dari agregasi Billing, readiness-nya dinyatakan eksplisit, dan approval blueprint tetap
kosong.

### 8.2 Acceptance handoff berikutnya

Handoff menuju `plan-module-delivery` dianggap siap hanya bila **seluruh** butir berikut terpenuhi:

1. pemilik meninjau dan menyetujui ketiga belas artefak revision `0.2`, dan `approved_by` beserta
   `approved_at` terisi;
2. ~~pertanyaan memblokir pada bagian 6 butir 9~~ — **sudah tertutup** 2026-09-02 oleh
   `RWI-DEC-086` s.d. `RWI-DEC-088`;
3. `INT-DOK-01` beserta perbaikan jalur tanpa antrean punya pemilik pengerjaan yang jelas, karena
   keduanya menyentuh modul lain;
4. konflik ruang kerja frontend punya keputusan delivery — dirework lebih dulu atau dikarantina
   dari rilis;
5. kontrak `0.2.0` tidak berubah lagi setelah approval; bila berubah, revision baru dan impact scan
   kedua repository menyusul.

**Butir 1 kini satu-satunya yang tersisa.** Butir 2 tertutup dengan hasil yang tidak terduga:
pertanyaannya ternyata salah arah, dan jawabannya justru memunculkan pekerjaan baru — mendaftarkan
tiga jenis dokumen ke mesin keutuhan supaya koreksi mungkin sama sekali. Itulah gunanya memeriksa
source sebelum bertanya.
