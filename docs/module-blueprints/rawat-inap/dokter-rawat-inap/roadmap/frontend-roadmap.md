# Roadmap Delivery Frontend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
roadmap_revision: 1
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
blueprint_shape: COMPOSITE
submodule: dokter-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: keputusan rupa tetap DEV_DISCRETION sesuai 03-frontend-architecture.md §7"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), approval desain 2026-09-03"
approved_at: "2026-09-03"
source_sha:
  backend: "93b3227c431401d8f586dec4e1fb25fbf41766e3"
  frontend: "863f24b0d1617069310c04e5770b47fd1b518b5b"
contract_versions: "0.3.0"
input_revisions:
  03-frontend-architecture.md: 0.3
  02-module-map.md: 1
  04-prd-to-mvp.md: 0.3
input_hashes:
  03-frontend-architecture.md: "3e8c04ed74e117d629c678d71f6a63d6e87c69f9fc32fad30d9bbb7b11c4a5a8"
  02-module-map.md: "29c761eed6a3fdc3a4d76c2803fde6e956a19784c4b3a14fc27d30e81e5a5d08"
  04-prd-to-mvp.md: "a0d5cc0c998fea5d7c23c587eeec1718e456320c6191eb5ffb752e0f3d79f9cb"
artifact_hashes:
  contracts/api-contract.md: "bbfa035a6607710f1b2bf30f50b7d8899adcc4b214b28734bc04dba19124bbc3"
  contracts/permission-audit-matrix.md: "7790bbc230e3a39bdfda93a0862cd81004bb035e0614077a48710cc9f99db5b2"
task_id_series: "FE-RWI-042 s.d. FE-RWI-050 — deret bersama seluruh modul, dilanjutkan dari FE-RWI-041"
```

---

## 0. Peringatan yang menentukan seluruh roadmap ini

> **Ruang kerja dokter sudah ada di source, dan memakai kontrak yang salah.** Berbeda dari
> `episode-rawat-inap` yang dibangun dari nol, sub-modul ini **memperbaiki layar yang sudah
> ter-commit**. Statusnya `Conflict` pada `DOK-TRC-FE-01`, dan ia **menahan rilis apa pun** —
> bukan menahan pengembangan.
>
> Yang salah adalah **sumber data dan pintu masuknya**, bukan seluruh layar. Komponen dasar klinis
> dan tab SOAP, catatan terpadu, resep, serta tindakan **isinya benar** dan dipakai ulang. Membuang
> semuanya lalu menulis dari nol adalah pemborosan; membiarkannya apa adanya adalah risiko salah
> pasien.

> **Akibat yang harus dinyatakan terus terang.** Dalam bentuk sekarang, layar itu dapat menampilkan
> pasien **rawat jalan** dengan label "Rawat Inap", lalu mengirim aksi antrean terhadap mereka.
> `FE-RWI-042` dan `FE-RWI-043` ada untuk menutup itu, dan keduanya berada paling depan.

> **Keputusan rupa tetap milik pelaksana.** Warna, jarak, ikon, bentuk tab, drawer, dan pilihan
> component library adalah `DEV_DISCRETION`. Yang mengikat hanyalah keterjangkauan layar, sumber
> data tiap wilayah, butir hak akses tiap tombol, dan makna keadaan kosong serta gagal.

---

## 1. Batas kewenangan dokumen ini

| Mengikat | `DEV_DISCRETION` |
| --- | --- |
| Layar mana yang mendapat butir menu, dan layar mana yang menjadi layar anak | Nama butir menu dan urutannya |
| Sumber data setiap wilayah layar | Susunan visual, jarak, dan warna |
| Butir hak akses yang menjaga setiap tombol | Ikon dan penempatan tombol |
| Makna keadaan kosong, gagal, dan sedang memuat | Kalimat persisnya |
| Ketiadaan aksi antrean pada ruang kerja rawat inap | Bentuk tab, drawer, atau accordion |

---

## 2. Slice dan milestone

Seluruh task frontend berada pada gelombang **`DOK-MVP-FE`**, dan gelombang itu **wajib selesai
sebelum rilis apa pun**. Urutan di dalamnya mengikuti kesiapan endpoint backend.

| Urutan | Task | Bergantung pada backend | Yang dapat diverifikasi bisnis |
| ---: | --- | --- | --- |
| 1 | `FE-RWI-042` | — | Butir menu yang menyesatkan hilang; pintu masuk pindah ke census |
| 2 | `FE-RWI-043` | `BE-RWI-044` | Dokter membuka pasiennya sendiri, bukan antrean poliklinik |
| 3 | `FE-RWI-044` | `BE-RWI-045` | Kajian medis awal dapat ditulis dan diselesaikan |
| 4 | `FE-RWI-045` | `BE-RWI-046`, `BE-RWI-047` | Catatan harian ditulis, diurut waktu pemeriksaan, dan dikoreksi |
| 5 | `FE-RWI-047` | `BE-RWI-048`, `BE-RWI-049` | Visite dicatat, dibatalkan, dan riwayatnya terbaca |
| 6 | `FE-RWI-048` | `BE-RWI-050`, `BE-RWI-051` | Resep dan tindakan dikerjakan dari satu layar |
| 7 | `FE-RWI-049` | `BE-RWI-052` | Pemeriksaan laboratorium dan radiologi dipesan, hasilnya dibaca |
| 8 | `FE-RWI-046` | `BE-RWI-053` | Catatan terpadu dibaca dan diverifikasi DPJP |
| 9 | `FE-RWI-050` | `BE-RWI-053` | Supervisor melihat verifikasi yang tertunggak |

---

## 3. Task

### `FE-RWI-042` — Pintu masuk dokter berpindah dari antrean ke pasien yang dirawat

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter tidak lagi menemukan dua layar bersebelahan yang tampak sama padahal satu berbasis antrean dan satu berbasis perawatan; pintu masuknya kini dari daftar pasien yang benar-benar dirawat |
| **Trace** | `DOK-TRC-FE-01`; `03-frontend-architecture.md` §0, §2.1, §3.1.1; `02-module-map.md` §3.3; `IA-INP-01`, `IA-INP-05` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Daftar pasien dirawat yang **sudah tersedia** di backend beserta penyaring dokternya |
| **Scope** | Pencabutan butir menu "Dokter → Rawat Inap"; pemindahan pintu masuk ke baris pasien pada census dan detail perawatan; pelepasan penyaring tanggal bawaan antrean |
| **Dependency** | — |
| **Acceptance criteria** | 1. Butir menu "Dokter → Rawat Inap" **tidak ada lagi** pada sidebar. 2. Ruang kerja dokter dapat dicapai dari baris pasien pada census dan dari detail perawatan. 3. Daftar pasien berasal dari daftar pasien dirawat yang disaring dokter yang sedang masuk, **bukan** dari antrean. 4. **Nol** pemanggilan layanan antrean tersisa pada berkas ruang kerja. 5. Sub-modul ini menambah **nol** butir menu baru |
| **Verification** | Test state komponen yang membuktikan permintaan menuju daftar pasien dirawat; pemindaian berkas ruang kerja yang menemukan nol impor hook antrean; bukti navigasi tiga klik dari Beranda |
| **Risk/blocker** | Butir menu yang dicabut sudah ter-commit dan sudah terlihat pengguna; pencabutannya perlu diumumkan supaya tidak dianggap fitur hilang. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti; nol impor antrean; laporan mencantumkan jalur navigasi baru |

---

### `FE-RWI-043` — Ruang kerja dokter berdiri di atas konteks pasien yang pasti

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Sebelum dokter dapat menulis apa pun, layar sudah memastikan pasien, lokasi, penanggung jawab, dan riwayat alerginya benar-benar tampil |
| **Trace** | `FE-DOK-01`; `03-frontend-architecture.md` §3.1; `INV-DOK-01`, `INV-DOK-02` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Komponen dasar klinis pada `src/components/ui/doctor-clinical-base/` — kepala halaman, ringkasan, kartu pasien, konteks, tab, tabel, panel, badge, keadaan kosong |
| **Scope** | Kepala konteks; penanda alergi; diagnosis kerja; penanda kewenangan; penonaktifan tombol tulis saat konteks gagal; pelepasan aksi panggil, lewati, dan tidak hadir |
| **Dependency** | `FE-RWI-042`, `BE-RWI-044` |
| **Acceptance criteria** | 1. Kepala konteks menampilkan nomor perawatan, nama pasien, lokasi, hari rawat, dan penanggung jawab. 2. Bila kepala konteks gagal dimuat, **seluruh tombol tulis nonaktif** beserta pesan dan tombol coba lagi. 3. Kegagalan memuat riwayat alergi **ditampilkan menonjol**, tidak disembunyikan. 4. Pengguna yang tidak berwenang atas pasien itu melihat tombol tulis nonaktif beserta keterangan siapa yang berwenang. 5. **Nol** aksi panggil, lewati, dan tidak hadir tersisa di seluruh ruang kerja |
| **Verification** | Test komponen untuk keempat keadaan — memuat, kosong, gagal, berisi; test yang membuktikan tombol tulis nonaktif saat konteks gagal; test yang membuktikan pesan kegagalan alergi terlihat |
| **Risk/blocker** | Ketiadaan penanda alergi terbaca sebagai "tidak ada alergi", dan bagi peresepan itu berbahaya. Kegagalannya **wajib** terlihat. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti; empat keadaan layar tergambar dan teruji; nol aksi antrean |

---

### `FE-RWI-044` — Layar kajian medis awal, terpisah kasatmata dari pengkajian keperawatan

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | DPJP mengisi pemeriksaan menyeluruh pertama, dan tidak seorang pun dapat mengira dokumen itu adalah pengkajian keperawatan |
| **Trace** | `FE-DOK-02`; `03-frontend-architecture.md` §3.2; `AC-CAP022-02` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Komponen dasar klinis; pola formulir bertahap yang sudah dipakai modul lain |
| **Scope** | Formulir kajian medis; daftar masalah terstruktur; rujukan hanya-baca ke pengkajian keperawatan; tombol Selesaikan |
| **Dependency** | `FE-RWI-043`, `BE-RWI-045` |
| **Acceptance criteria** | 1. Layar menampilkan kajian medis dan pengkajian keperawatan sebagai **dua hal yang jelas berbeda**. 2. Pengkajian keperawatan tampil hanya-baca dan **bukan penghalang** bila belum ada. 3. Kegagalan menyimpan **tidak menghilangkan isian**. 4. Menyelesaikan kajian yang belum lengkap menampilkan bagian kosong **satu per satu**. 5. Tombol Selesaikan hanya muncul bagi peran yang berhak |
| **Verification** | Test komponen pemisahan tampilan; test isian bertahan saat gagal; test daftar bagian kosong |
| **Risk/blocker** | Keduanya tersimpan pada tabel yang sama; pemisahan di layar adalah harga yang dibayar atas keputusan itu, dan **mengikat**. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti; pemisahan tampilan tergambar pada skema layar |

---

### `FE-RWI-045` — Catatan perkembangan beserta waktu pemeriksaan dan koreksinya

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter menulis catatan harian dengan waktu pemeriksaan yang sebenarnya, dan membetulkan yang sudah final tanpa dapat menyuntingnya diam-diam |
| **Trace** | `FE-DOK-03`; `03-frontend-architecture.md` §3.3; `RWI-DEC-086`, `RWI-DEC-088` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Tab SOAP yang sudah ada, setelah sumber datanya diganti ke konteks perawatan |
| **Scope** | Lini masa terurut waktu pemeriksaan; formulir empat bagian; pengisian waktu pemeriksaan; tombol Koreksi; penanda koreksi beserta penulisnya |
| **Dependency** | `FE-RWI-043`, `BE-RWI-046`, `BE-RWI-047` |
| **Acceptance criteria** | 1. Lini masa terurut **waktu pemeriksaan**, bukan waktu penulisan. 2. Waktu pemeriksaan dapat diisi mundur; di luar batas wajar ditolak beserta keterangannya. 3. **Tidak ada tombol Sunting** pada catatan yang sudah diselesaikan. 4. Layar mengatakan bahwa menekan Selesai mengunci catatan **sebelum** tombol itu ditekan. 5. Penanda koreksi menampilkan **penulis asli sebagai penulis catatan**, dan dokter pengganti hanya pada baris koreksinya. 6. Tombol Koreksi disembunyikan bila pengguna tidak berwenang |
| **Verification** | Test urutan lini masa memakai tiga catatan berwaktu berbeda; test ketiadaan tombol sunting; test tampilan penulis asli versus penulis pengganti |
| **Risk/blocker** | Memberi tahu penguncian **setelah** tombol ditekan akan membuat dokter merasa dijebak; penempatan pemberitahuannya mengikat, kalimatnya `DEV_DISCRETION`. Owner: Frontend authority |
| **DoD** | Keenam acceptance criteria terbukti; test urutan dan test penulis hijau |

---

### `FE-RWI-046` — Catatan terpadu dan verifikasi DPJP

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | DPJP membaca catatan seluruh profesi pada satu lembar dan menyatakan sudah memeriksanya, tanpa nama penulis aslinya tergantikan |
| **Trace** | `FE-DOK-04`; `03-frontend-architecture.md` §3.4; `AC-CAP021-03` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Tab catatan terpadu yang sudah ada, setelah sumber datanya diganti |
| **Scope** | Lini masa lintas profesi; penanda verifikasi; tombol Verifikasi; penanda penulis dan verifikator terpisah |
| **Dependency** | `FE-RWI-043`, `BE-RWI-053` |
| **Acceptance criteria** | 1. Setiap catatan menampilkan penulis dan profesinya. 2. Setelah diverifikasi, **nama penulis asli tetap tampil sebagai penulis**; verifikator tampil terpisah. 3. Tombol Verifikasi **disembunyikan** bagi yang tidak berhak, bukan ditampilkan lalu ditolak. 4. Saat kebijakan verifikasi tidak aktif, penanda berbunyi "verifikasi tidak diwajibkan" — bukan daftar kosong. 5. Keterlambatan tampil tanpa menahan penulisan catatan berikutnya |
| **Verification** | Test tampilan penulis versus verifikator; test tombol tersembunyi bagi peran tidak berhak; test keadaan kebijakan kosong |
| **Risk/blocker** | Menampilkan satu nama saja membuat rekam medis tidak dapat menunjukkan siapa menulis dan siapa menyetujui. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti |

---

### `FE-RWI-047` — Riwayat visite beserta pencatatan dan pembatalannya

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter mencatat kunjungannya, membatalkan yang salah catat, dan melihat riwayat yang jujur — termasuk baris yang dibatalkan |
| **Trace** | `FE-DOK-05`; `03-frontend-architecture.md` §3.5; `RWI-DEC-084`, `RWI-DEC-085`; `RWI-AC-150` s.d. `RWI-AC-156` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Komponen dasar klinis; pola pencegahan pengiriman ganda yang sudah dipakai modul lain |
| **Scope** | Riwayat visite; tombol Catat Visite beserta kunci permintaan; peringatan visite berdekatan; tombol Batalkan beserta alasan wajib; tautan dokumen opsional |
| **Dependency** | `FE-RWI-043`, `BE-RWI-048`, `BE-RWI-049` |
| **Acceptance criteria** | 1. Riwayat menampilkan kejadian yang **dibatalkan beserta alasannya**, tidak disembunyikan. 2. Keadaan kosong berbunyi "belum ada visite tercatat" **walaupun sudah ada tiga catatan perkembangan**. 3. Tombol Catat Visite nonaktif selama permintaan berjalan, dan penekanan dua kali menghasilkan satu kejadian. 4. Visite pada jam berdekatan **diperingatkan, bukan ditolak**, dan dapat dilanjutkan. 5. Tombol Batalkan menuntut alasan; tombol simpan nonaktif selama alasan kosong. 6. **Tidak ada tombol Sunting** pada kejadian visite |
| **Verification** | Test keadaan kosong saat catatan sudah ada; test penekanan ganda; test peringatan yang dapat dilanjutkan; test ketiadaan tombol sunting |
| **Risk/blocker** | Menyembunyikan baris yang dibatalkan berarti menghapus jejak yang justru dibutuhkan auditor. Menolak visite kedua **dilarang** `RWI-DEC-085`. Owner: Frontend authority |
| **DoD** | Keenam acceptance criteria terbukti; skema layar menggambarkan baris batal |

---

### `FE-RWI-048` — Resep dan tindakan dari satu layar

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter meresepkan dan mencatat tindakan tanpa berpindah layar, dan melihat status pemenuhan tanpa dapat mengubahnya |
| **Trace** | `FE-DOK-06`; `03-frontend-architecture.md` §3.6; `RUL-DOK-01` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Tab resep dan tindakan yang sudah ada, setelah sumber datanya diganti |
| **Scope** | Daftar resep beserta jenis dan status pemenuhan; tombol Buat Resep; daftar tindakan; penanda keadaan penerbitan tagihan |
| **Dependency** | `FE-RWI-043`, `BE-RWI-050`, `BE-RWI-051` |
| **Acceptance criteria** | 1. Resep obat pulang **terbedakan** dari resep harian pada daftar. 2. **Tidak ada tombol menandai obat sudah diserahkan** di seluruh layar. 3. Status pemenuhan tampil hanya-baca. 4. Kegagalan penerbitan tagihan tampil sebagai **penanda pada barisnya**, bukan galat halaman. 5. Pengiriman resep berulang tidak melahirkan resep ganda di layar |
| **Verification** | Test ketiadaan tombol tandai-diserahkan; test penanda kegagalan tagihan; test pengiriman ganda |
| **Risk/blocker** | Menambahkan tombol tandai-diserahkan kelak berarti melanggar batas kepemilikan, bukan melengkapi layar. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti |

---

### `FE-RWI-049` — Pemeriksaan penunjang laboratorium dan radiologi

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter memesan pemeriksaan dan membaca hasil yang sudah disahkan, dengan hasil yang belum final tidak pernah terlihat seperti hasil sah |
| **Trace** | `FE-DOK-07`; `03-frontend-architecture.md` §3.7; `INV-DOK-12` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Komponen dasar klinis; pola daftar dan penyaring yang sudah ada |
| **Scope** | Daftar pesanan laboratorium; daftar pesanan radiologi beserta modalitas dan jadwalnya; tombol Pesan; tampilan hasil final; penanda hasil belum final |
| **Dependency** | `FE-RWI-043`, `BE-RWI-052` |
| **Acceptance criteria** | 1. Laboratorium dan radiologi tampil sebagai dua daftar yang jelas. 2. Hasil yang belum final tampil dengan penanda dan **tidak** terlihat sama dengan hasil final. 3. Hasil milik perawatan lain **tidak ikut tampil**. 4. Kalimat "pemeriksaan radiologi belum tersedia di sistem" **tidak ada lagi** di mana pun. 5. Tidak ada tombol mengisi hasil |
| **Verification** | Test penanda hasil belum final; test penyaringan per perawatan; pemindaian teks yang membuktikan kalimat lama sudah hilang |
| **Risk/blocker** | Kalimat lama berasal dari anggapan bahwa modul radiologi belum ada; anggapan itu sudah terbukti keliru. Owner: Frontend authority |
| **DoD** | Kelima acceptance criteria terbukti |

---

### `FE-RWI-050` — Daftar pantau verifikasi catatan terpadu

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Supervisor klinis menemukan catatan yang menunggu atau lewat batas verifikasi, dan tidak salah membaca daftar kosong sebagai kebijakan yang tidak aktif |
| **Trace** | `FE-DOK-08`; `03-frontend-architecture.md` §3.8 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Layar daftar pantau `FE-INP-09` yang sudah ada; komponen daftar yang sudah dipakai |
| **Scope** | Daftar tambahan di dalam daftar pantau; tiga keadaan kosong yang dibedakan; tautan ke catatan terpadu pasien |
| **Dependency** | `FE-RWI-046`, `BE-RWI-053` |
| **Acceptance criteria** | 1. Daftar muncul sebagai daftar tambahan di dalam daftar pantau yang sudah ada, **bukan** layar baru. 2. Tiga keadaan dibedakan tegas: sudah terverifikasi, tidak diwajibkan, dan gagal dimuat. 3. Setiap baris membuka catatan terpadu pasien itu. 4. Daftar menampilkan nama pasien, penulis, dan keterlambatan — **tanpa isi klinis**. 5. Urutan daftar di dalam daftar pantau mengikuti ketetapan `02-module-map.md`, bukan diputuskan sendiri |
| **Verification** | Test ketiga keadaan kosong; test bahwa isi klinis tidak muncul pada daftar; bukti navigasi dua klik dari Beranda |
| **Risk/blocker** | Satu layar kini dipakai tiga sub-modul; urutan dan pengelompokannya **ditetapkan tingkat modul**. Owner: Frontend authority bersama pemilik `02-module-map.md` |
| **DoD** | Kelima acceptance criteria terbukti; urutan daftar mengikuti peta modul |

---

## 4. Gerbang yang menahan rilis

| Gerbang | Sifat | Menahan apa |
| --- | --- | --- |
| Gelombang `DOK-MVP-FE` belum selesai | Ruang kerja masih memakai kontrak antrean | **Rilis apa pun** pada sub-modul ini |
| Data master rawat inap yang layak — `RWI-UI-GAP-007` | Masih terbuka | Uji end-to-end yang bermakna |
| Peran DPJP, dokter jaga, konsulen, perawat, dan supervisor terpisah di lingkungan uji | Belum dipastikan | Pengujian matriks kewenangan, terutama verifikasi dan pembatalan visite |
| Kebijakan verifikasi catatan terpadu | Belum ada | Isi daftar pantau; mekanismenya tetap dapat diuji dengan kebijakan kosong |

---

## 5. Yang sengaja tidak ada di roadmap ini

| Yang tidak ada | Alasan |
| --- | --- |
| Task menambah butir menu baru | Sub-modul ini menambah **nol** butir menu; kedelapan layarnya menjadi layar anak |
| Layar penerbitan penetapan berhalangan | Milik kepala unit lewat `MedicalRecordManagement`, bukan ruang kerja dokter |
| Layar cetak | Tidak ada pada sub-modul ini; resume pulang milik `episode-rawat-inap` |
| Task menentukan warna, ikon, atau component library | `DEV_DISCRETION` |
| Task untuk sub-modul `keperawatan` | Statusnya masih `draft` |
