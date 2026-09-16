# PRD ke MVP — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement` |
| Sub-modul | `dokter-rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | **`0.6.0`** untuk bagian 22; `0.3.0` untuk bagian 1 s.d. 20; `0.5.0` untuk bagian 21 |
| Revision artefak | **`0.4`** — bagian 22 penyelarasan `PRD-RWI-V2-001` |
| Status | **`draft`** untuk `0.4`. `0.3` `approved` — disetujui Muhammad Hamzah, 2026-09-03 |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-03** |
| Repository target | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Commit SHA baseline | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Baseline requirement | `PRD-RWI-FINAL-001` v1.0.0 bagian 18, 19, 23.1, 30.3 |
| Decision log | Revision `10`, SHA-256 `de786bebc169636c0d7bd254d429a0209809890d78a7f1dcd8220d303fcbecc0` — memuat `RWI-DEC-086` s.d. `RWI-DEC-088` dan `RWI-RULE-038` |
| Arsitektur domain | revision `0.2`, `DOMAIN_ARCHITECTURE_READY`, SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Ringkasan cakupan | Tujuh kemampuan dokumentasi dokter rawat inap, dari kajian medis awal sampai pemesanan penunjang, **tanpa satu tabel tandingan pun** |
| Ditulis paling akhir | Ya — menurunkan dari arsitektur dan kelima kontrak |

---

## 2. Ringkasan eksekutif

Sub-modul ini memberi dokter **satu tempat untuk mengerjakan seluruh dokumentasi pasien rawat
inap**: kajian medis awal, catatan perkembangan harian, catatan terpadu beserta verifikasinya,
visite, resep, tindakan, dan pemeriksaan penunjang.

**Yang membuatnya murah:** enam dari tujuh kemampuan berdiri di atas mesin yang **sudah ada dan
sudah dipakai** poliklinik serta IGD. Isi SOAP sudah berada di dalam catatan dokter; resep punya
mesin lengkap di Farmasi; laboratorium dan **radiologi** sudah berjalan; mesin koreksi dokumen
sudah tersedia di rekam medis. Hanya **satu** tabel yang benar-benar baru: kejadian visite.

**Yang menahannya:** satu perbaikan dan dua pelonggaran pada mesin klinis. Kedua pelonggaran itu
keputusannya **sudah turun sejak 2026-08-21** tetapi kodenya belum ada; perbaikannya baru ditemukan
pada pemindaian 2 September 2026.

---

## 3. Masalah produk

| Masalah | Bukti kode | Akibatnya hari ini |
| --- | --- | --- |
| Jalur tanpa antrean **gagal** | `DoctorConsultationController.cs` baris 258–265 dan 360–366 | Setiap percobaan menulis catatan untuk pasien tanpa antrean berujung kegagalan sistem, dan itu menyentuh pasien rawat inap maupun IGD |
| Catatan rawat inap tidak dapat dibuat tanpa nomor antrean | Pencarian episode pada dua controller klinis nihil | Dokter tidak dapat menulis catatan, diagnosis, resep, maupun tindakan untuk pasien rawat inap |
| Batas satu catatan per kunjungan | Penolakan pada validasi konsultasi | Bahkan bila pintunya dibuka, dokter hanya dapat menulis **satu** catatan untuk seluruh masa perawatan |
| Batas satu resep aktif per catatan | Penolakan pada validasi resep | Pasien yang dirawat sepuluh hari hanya dapat menerima satu resep |
| Tidak ada catatan visite di mana pun | Pencarian `PhysicianVisit` nihil | Visite dokter tidak dapat dibuktikan, dan `RWI-DEC-084` yang sudah mendefinisikannya tidak punya tempat menyimpan |
| Verifikasi catatan terpadu tidak dapat dicatat | Model CPPT tidak punya satu kolom verifikasi pun | Catatan terpadu ada, tetapi tidak ada cara menandai DPJP sudah memeriksanya |
| Ruang kerja dokter memakai antrean rawat jalan | `doctor-inpatient-view.jsx` mengimpor hook antrean | Layar dapat menampilkan pasien rawat jalan berlabel "Rawat Inap" beserta aksi antrean yang salah |
| **Catatan yang sudah diselesaikan tidak dapat disunting maupun dikoreksi** | Penyuntingan setelah selesai sudah dilarang, tetapi catatan dokter tidak pernah terdaftar pada mesin keutuhan | Salah ketik menjadi permanen di rekam medis. Satu-satunya jalan yang tersisa bagi dokter adalah menulis catatan baru yang membantah catatan lama |

---

## 4. Visi produk

1. DPJP membuka daftar pasiennya dari census rawat inap, bukan dari antrean.
2. Ia memilih satu pasien; konteks perawatan tampil beserta penanda alergi.
3. Ia menulis kajian medis awal tanpa nomor antrean.
4. Setiap hari ia mencatat visitenya sebagai kejadian tersendiri, lalu menulis perkembangan.
5. Ia meresepkan obat berkali-kali sepanjang perawatan, memesan laboratorium dan radiologi, serta
   mencatat tindakan.
6. Ia memverifikasi catatan profesi lain, dan keterlambatan verifikasi terpantau tanpa menahan
   pelayanan.
7. Seluruhnya tersimpan di rekam medis yang sama dengan catatan poliklinik pasien itu — **satu
   pasien, satu tempat rekam medis**.

---

## 5. Batas MVP

**Titik mulai**

1. Pasien sudah dikonfirmasi tiba di kamar.
2. Penanggung jawab pasien sudah ditetapkan.
3. Jalur tanpa antrean sudah diperbaiki sehingga tidak lagi gagal.

**Titik akhir**

1. Kajian medis, catatan harian, catatan terpadu beserta verifikasinya, visite, resep, tindakan,
   serta pesanan laboratorium dan radiologi tercatat pada rekam medis pasien.
2. Riwayat visite dapat dibaca beserta kejadian yang dibatalkan dan alasannya.
3. Supervisor dapat melihat verifikasi yang tertunggak.
4. Perilaku poliklinik dan medical check-up terbukti tidak berubah.

**Di luar batas:** resume pulang, seluruh dokumentasi keperawatan, dan penagihan visite.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
| --- | --- |
| DPJP | Seluruhnya, **termasuk verifikasi catatan terpadu** |
| Dokter jaga ruangan | Seluruhnya kecuali verifikasi dan kajian medis awal |
| Dokter konsulen | Membaca, menulis catatan terpadu, mencatat visite |
| Perawat | Membaca catatan dokter; menulis catatan terpadu dari ruang kerjanya sendiri |
| Supervisor klinis | Membaca seluruhnya; membatalkan kejadian visite yang salah catat |
| Petugas Farmasi, Laboratorium, Radiologi | Bekerja di modulnya sendiri; sub-modul ini hanya mengirim pesanan dan membaca hasil |

---

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Kajian medis awal | `CAP-022` | Wajib; tanpa ini tidak ada dasar rencana perawatan |
| Dokumentasi catatan harian | `CAP-020` | Wajib; tanpa ini dokter menulis di kertas dan riwayat digital bolong |
| Catatan terpadu beserta verifikasi | `CAP-021` | Wajib; lembar lintas profesi adalah tempat perawat dan dokter bertemu |
| Resep rawat inap dan obat pulang | `CAP-023` | Wajib; tanpa resep berulang, pasien sepuluh hari hanya dapat satu resep |
| Tindakan dokter | `CAP-024` | Wajib; menjadi dasar fakta klinis untuk penagihan |
| Pencatatan visite | `CAP-025` | Wajib; satu-satunya bukti kunjungan dokter, dan tidak ada tempat lain menyimpannya |
| Pemeriksaan penunjang laboratorium **dan radiologi** | `CAP-015` | Wajib; keduanya modulnya sudah ada, dan keputusan terapi bergantung pada hasilnya |

**Tujuh kemampuan `MUST HAVE`** — naik dari enam pada `0.1.0`, karena radiologi tidak lagi ditunda.
Kepemilikan datanya seluruhnya tegas; tidak ada `OPEN DECISION` kepemilikan pada sub-modul ini.

---

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Nilai batas waktu verifikasi catatan terpadu | bagian `CAP-021` | Angkanya wajib berasal dari Clinical Governance, dan itu belum turun | **Mekanismenya tetap dibangun.** Kebijakan kosong berarti verifikasi tidak diwajibkan; pencatatan berjalan penuh |
| Nilai batas waktu kajian medis | bagian `CAP-022` | `RWI-RULE-021` belum `approved` dan pemilik klinisnya belum ditunjuk | Sama — mekanismenya siap, angkanya menyusul |
| Pencatatan visite oleh petugas atas nama dokter | bagian `CAP-025` | `RWI-RULE-017` current menyatakan kemampuan itu tidak tersedia sampai ada kebijakan eksplisit | **Bawaan yang aman:** hanya dokter yang dapat mencatat visite |
| Penagihan dan agregasi tarif visite | bagian `CAP-025` | Kebijakan agregasi milik pemilik Billing dan belum ada | Kejadian klinis tetap dicatat lengkap, sehingga aturan apa pun yang kelak disetujui dapat dijalankan mundur |
| Pembacaan balik status penyerahan obat pulang | bagian `CAP-023` | Kontrak status final Farmasi belum disetujui pemiliknya — `RWI-DOK-RQG-003` | Butir daftar periksa administrasi tetap **ditandai manual** petugas admisi, seperti `RWI-DEC-033` |
| Pemberitahuan otomatis kepada pengguna | lintas kemampuan | Tidak ada requirement-nya — `RWI-DOK-RQG-001` | Daftar pantau dan daftar percobaan ulang |

> **Radiologi tidak lagi berada di tabel ini.** Pada `0.1.0` ia ditunda dengan alasan "modulnya
> tidak ada". Alasan itu terbukti keliru pada `BE@93b3227`.

---

## 9. Alur bisnis target

`FLOW-DOK-MVP-001`, diturunkan dari [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md):

1. Pasien dikonfirmasi tiba di kamar dan penanggung jawabnya ditetapkan.
2. Dokter membuka daftar pasien rawat inap miliknya dari census.
3. Dokter memilih satu pasien; konteks perawatan dan penanda alergi tampil.
4. Dokter menulis kajian medis awal lalu menyelesaikannya.
5. Setiap hari dokter mencatat visitenya sebagai kejadian tersendiri.
6. Dokter menulis catatan perkembangan dengan waktu pemeriksaan yang sebenarnya.
7. Bila perlu, dokter memesan pemeriksaan laboratorium atau radiologi.
8. Dokter membuat resep; saat pasien pulang, resepnya ditandai sebagai obat pulang.
9. Dokter mencatat tindakan; fakta klinisnya diterbitkan ke Billing setelah catatan tersimpan.
10. DPJP memverifikasi catatan terpadu yang ditulis profesi lain.
11. Keputusan pulang diambil — **milik `episode-rawat-inap`**, bukan sub-modul ini.

---

## 10. Epic dan functional requirement

### `EPIC DOK-01` — Pintu masuk dokumentasi dokter dibuka

**Tujuan.** Dokter dapat menulis untuk pasien rawat inap tanpa antrean, tanpa merusak alur lain.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-001` | Catatan dokter dapat dibuat bagi kunjungan yang punya perawatan rawat inap berjalan, tanpa antrean dan tanpa kunjungan IGD | **EXTEND** |
| `FR-DOK-002` | Catatan **kedua dan seterusnya** pada satu kunjungan rawat inap diterima | **EXTEND** |
| `FR-DOK-003` | Resep **kedua dan seterusnya** sepanjang perawatan diterima | **EXTEND** |
| `FR-DOK-004` | Perilaku rawat jalan dan medical check-up **tidak berubah sedikit pun** | **EXISTING / REUSE** — dijaga test regresi |
| `FR-DOK-005` | Jalur catatan dokter IGD terbukti tidak rusak | **EXISTING / REUSE** — dijaga test regresi |
| `FR-DOK-037` ★ | **Jalur tanpa antrean tidak lagi menghasilkan kegagalan sistem** | **MISSING / NEW** — perbaikan |
| `FR-DOK-038` ★ | Penanda perawatan yang tidak cocok dengan kunjungannya ditolak | **MISSING / NEW** |

> **`FR-DOK-037` — perbaikan jalur tanpa antrean**
>
> Sistem menyimpan catatan dokter untuk pasien yang tidak punya baris antrean, tanpa menyentuh data
> antrean mana pun.
>
> **Contoh:** dr. Andi membuka Tn. Budi yang dirawat di Melati 3B, lalu menyimpan catatan. Hari ini
> permintaan itu berujung kegagalan sistem (kode 500) karena data antrean yang kosong tetap
> ditulis. Setelah perbaikan, catatan tersimpan dengan kode 201, dan jumlah baris antrean sebelum
> dan sesudahnya **identik**.

### `EPIC DOK-02` — Kajian medis awal

**Tujuan.** Pemeriksaan menyeluruh pertama tercatat dan berbeda dari catatan harian.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-006` | Dokter membuat kajian medis pada perawatan yang berjalan | **EXTEND** |
| `FR-DOK-007` | Kajian medis dan catatan harian punya record serta lifecycle **berbeda** | **EXTEND** |
| `FR-DOK-008` | Catatan harian **tidak menimpa** kajian medis final | **EXTEND** |
| `FR-DOK-009` | Diagnosis dan daftar masalah tersimpan terstruktur, bukan teks di dalam catatan | **EXISTING / REUSE** |
| `FR-DOK-010` | Koreksi kajian medis mempertahankan versi asli beserta alasannya | **EXISTING / REUSE** — memakai mesin koreksi rekam medis yang sudah ada |
| `FR-DOK-011` | Perawat **tidak dapat** membuat kajian medis | **MISSING / NEW** |

### `EPIC DOK-03` — Catatan perkembangan harian

**Tujuan.** Perkembangan pasien terbaca berurutan menurut waktu pemeriksaan yang sebenarnya.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-012` | Beberapa catatan sepanjang perawatan tersimpan sebagai lini masa | **EXTEND** |
| `FR-DOK-013` | Waktu pemeriksaan terpisah dari waktu penulisan, dan lini masa terurut waktu pemeriksaan | **MISSING / NEW** |
| `FR-DOK-014` | Catatan dapat dibuat walaupun pengkajian awal keperawatan **belum selesai** | **MISSING / NEW** |
| `FR-DOK-015` | Perawatan yang sudah ditutup menolak catatan baru tetapi **menerima koreksi** catatan lama | **MISSING / NEW** |
| `FR-DOK-016` | Koreksi tidak mengaktifkan kembali perawatan, tidak membuka tempat tidur, tidak mengubah lama dirawat | **MISSING / NEW** |
| `FR-DOK-044` ★ | **Memfinalkan catatan sekaligus mendaftarkannya ke mesin keutuhan sebagai dokumen tertanda tangan**, dalam transaksi yang sama | **MISSING / NEW** |
| `FR-DOK-045` ★ | Koreksi pada catatan yang **belum** final ditolak, dengan arahan menyunting langsung | **MISSING / NEW** |
| `FR-DOK-046` ★ | Kajian medis dan tindakan yang sudah diselesaikan dapat dikoreksi dengan cara yang sama seperti catatan dokter | **MISSING / NEW** |
| `FR-DOK-047` ★ | **Koreksi atas nama dokter yang berhalangan hanya oleh DPJP yang aktif pada episode itu**, setelah kepala unit menerbitkan penetapan berbatas waktu | **MISSING / NEW** |
| `FR-DOK-048` ★ | Koreksi atas nama dokter lain **tidak mengubah penulis catatan aslinya** | **EXISTING / REUSE** — mesinnya sudah menyimpannya |

> **`FR-DOK-044` — finalisasi sekaligus mendaftarkan**
>
> Sistem mendaftarkan catatan ke mesin keutuhan pada saat yang sama dengan finalisasinya. Bila
> pendaftaran gagal, finalisasi ikut batal.
>
> **Contoh:** dr. Andi menekan Selesai pada catatan Tn. Budi pukul 11.00. Catatan itu langsung
> berstatus final **dan** tercatat sebagai dokumen tertanda tangan atas nama dr. Andi. Pukul 11.02
> ia sadar salah menulis tekanan darah; percobaan menyuntingnya ditolak, dan ia membetulkannya lewat
> koreksi beralasan. Bila pendaftaran keutuhan gagal karena gangguan, catatan itu **tidak jadi**
> difinalkan — sehingga tidak pernah ada catatan final yang tidak dapat dibetulkan.

> **`FR-DOK-047` — koreksi atas nama dokter yang berhalangan**
>
> **Contoh:** dr. Andi menulis catatan 3 September lalu cuti sepuluh hari. Kepala unit rawat inap
> menerbitkan penetapan berhalangan atas nama dr. Andi, berlaku 5 sampai 13 September. dr. Sinta,
> yang menjadi DPJP Tn. Budi selama dr. Andi cuti, menambahkan koreksi pada 5 September. dr. Rina
> yang juga memegang butir hak akses pengganti tetapi **bukan** DPJP Tn. Budi ditolak dengan kode
> 403, walaupun penetapan yang sama berlaku baginya.

> **`FR-DOK-013` — waktu pemeriksaan**
>
> **Contoh:** dr. Andi memeriksa Tn. Budi pukul **07.40**, lalu menulis catatannya pukul **11.00**.
> Lini masa menempatkan catatan itu pada urutan pukul 07.40, bukan 11.00. Catatan lain yang ditulis
> pukul 09.00 untuk pemeriksaan pukul 08.50 tetap berada **di bawah** catatan pukul 07.40.

### `EPIC DOK-04` — Catatan terpadu dan verifikasi

**Tujuan.** Catatan lintas profesi terbaca sebagai satu lembar, dan DPJP menyatakan sudah
memeriksanya.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-017` | Catatan dokter dan perawat tampil sebagai entry terpisah beserta penulis dan profesinya | **EXISTING / REUSE** |
| `FR-DOK-018` | DPJP dapat memverifikasi catatan; **penulis aslinya tidak berubah** | **MISSING / NEW** |
| `FR-DOK-019` | Verifikasi hanya oleh DPJP yang **aktif pada saat verifikasi** | **MISSING / NEW** |
| `FR-DOK-020` | Keterlambatan verifikasi terpantau menurut kebijakan aktif dan **tidak menahan** pekerjaan | **MISSING / NEW** |
| `FR-DOK-021` | Kebijakan verifikasi kosong berarti tidak ada yang menunggu verifikasi | **MISSING / NEW** |
| `FR-DOK-022` | Koreksi catatan terverifikasi mengembalikannya ke menunggu verifikasi | **MISSING / NEW** |

### `EPIC DOK-05` — Visite sebagai kejadian

**Tujuan.** Kunjungan dokter tercatat sebagai fakta yang berdiri sendiri, dapat dikoreksi tanpa
kehilangan jejak, dan dihitung apa adanya.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-023` | Visite tercatat beserta perawatan, dokter, peran, waktu kedatangan, dan pencatatnya | **MISSING / NEW** |
| `FR-DOK-024` | **Catatan tanpa kejadian visite tidak menambah hitungan visite** | **MISSING / NEW** |
| `FR-DOK-025` | Visite muncul di riwayat walaupun catatannya ditulis kemudian, atau tidak ditulis sama sekali | **MISSING / NEW** |
| `FR-DOK-026` | Pengiriman berulang dengan kunci yang sama tidak melahirkan kejadian ganda | **MISSING / NEW** |
| `FR-DOK-027` | Visite kedua pada jam berdekatan **diperingatkan, bukan ditolak** | **MISSING / NEW** |
| `FR-DOK-028` | Hanya pengguna berkewenangan dokter yang dapat mencatat visite | **MISSING / NEW** |
| `FR-DOK-039` ★ | **Dua visite nyata pada tanggal yang sama menghasilkan dua kejadian dan hitungan dua** | **MISSING / NEW** |
| `FR-DOK-040` ★ | **Kejadian salah catat dibatalkan beralasan, tetap tersimpan, dan tidak ikut dihitung** | **MISSING / NEW** |
| `FR-DOK-041` ★ | **Agregasi tagihan tidak mengubah, menggabungkan, maupun menghapus kejadian klinis** | **MISSING / NEW** |

> **`FR-DOK-039` — dua visite pada hari yang sama**
>
> **Contoh:** 12 September, dr. Andi visite pukul **07.40**. Pukul **16.10** kondisi Tn. Budi
> memburuk dan dr. Andi datang lagi, lalu mencatat visite kedua. Riwayat menampilkan **dua** baris,
> dan hitungan klinis serta operasional hari itu **dua**. Bila kelak penjamin hanya membayar satu
> visite per dokter per tanggal, Billing boleh menerbitkan satu tagihan — dan kedua baris riwayat
> **tetap utuh**.

> **`FR-DOK-040` — koreksi visite**
>
> **Contoh:** dr. Andi mengisi jam 17.40 padahal ia datang pukul 07.40. Ia membatalkan kejadian itu
> dengan alasan "salah ketik jam", lalu mencatat kejadian baru pukul 07.40. Riwayat menampilkan dua
> baris: satu dibatalkan beserta alasannya, satu berlaku. Hitungan hari itu **1**.

### `EPIC DOK-06` — Resep, tindakan, dan penunjang

**Tujuan.** Obat, tindakan, dan pemeriksaan penunjang berjalan sepanjang perawatan tanpa salah
episode.

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-029` | Resep dibuat dari konteks rawat inap | **EXTEND** |
| `FR-DOK-030` | **Obat pulang menjadi jenis resep eksplisit** yang dapat dibedakan dari resep harian | **EXTEND** |
| `FR-DOK-031` | Status pemenuhan resep dapat **dibaca**; sub-modul ini tidak pernah menulisnya | **EXISTING / REUSE** |
| `FR-DOK-032` | Pengiriman resep berulang tidak melahirkan resep ganda | **EXTEND** |
| `FR-DOK-033` | Tindakan membedakan yang direncanakan dari yang dikerjakan | **EXISTING / REUSE** — status yang ada sudah memuatnya |
| `FR-DOK-034` | Kegagalan penagihan **tidak menghilangkan** catatan tindakan | **EXTEND** |
| `FR-DOK-035` | Pemesanan laboratorium membawa konteks perawatan, dan pesanan perawatan A tidak dapat diproses sebagai milik perawatan B | **EXTEND** |
| `FR-DOK-036` | Hasil laboratorium final terbaca **tanpa baris salinan** yang menjadi kebenaran baru | **EXISTING / REUSE** |
| `FR-DOK-042` ★ | **Pemesanan radiologi membawa konteks perawatan**, dan hasilnya terbaca tanpa baris salinan | **EXTEND** |
| `FR-DOK-043` ★ | **Hasil yang belum final ditampilkan dengan penanda**, tidak sebagai hasil sah | **MISSING / NEW** |

**Tidak ada epic berstatus `OPEN DECISION` pada sub-modul ini.**

### 10.1 Functional requirement yang dibatalkan

| No | Bunyinya pada `0.1.0` | Alasan dibatalkan |
| --- | --- | --- |
| — | Tidak ada FR yang dibatalkan | Seluruh FR `0.1.0` tetap berlaku; sepuluh FR baru ditambahkan, dan disposisi `FR-DOK-010` serta `FR-DOK-033` berubah menjadi `EXISTING / REUSE` karena mesinnya ternyata sudah ada |

---

## 11. Model status yang diusulkan

| Mesin | Nilai | Invariant utama |
| --- | --- | --- |
| Catatan dokter | `Draft`, `InProgress`, `Completed`, `Cancelled` | Final tidak disunting di tempat — `INV-DOK-10` |
| Kajian medis | Sama — tabel yang sama, pembedanya jenis kajian | Berbeda record dan lifecycle dari catatan harian |
| Verifikasi catatan terpadu | `NotRequired`, `Pending`, `Verified`, `Overdue` | Verifikator bukan penulis asli — `INV-DOK-11` |
| Tindakan | `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled` | Kegagalan penagihan tidak mengubahnya — `INV-DOK-09` |
| **Kejadian visite** | `Recorded`, `Cancelled` | Satu kunci permintaan satu kejadian; yang batal tetap tersimpan — `INV-DOK-06`, `INV-DOK-08` |
| Integritas dokumen | `Draft`, `Signed`, `LockedUnsigned`, `Cancelled` | Koreksi **hanya** diterima pada dokumen terkunci; dokumen konsep justru ditolak — `RWI-FACT-013`. Memfinalkan catatan sekaligus mendaftarkannya sebagai tertanda tangan — `RWI-DEC-086` |
| Jenis resep | `Routine`, `Daily`, `Discharge` | Obat pulang adalah jenis, bukan daftar terpisah |

**Nol status episode baru** — `RWI-DEC-009`.

---

## 12. Sasaran arsitektur

| Sasaran | Isinya |
| --- | --- |
| Tabel baru milik Rawat Inap | **Nol** |
| Tabel baru milik modul lain | **Satu** — `CliPhysicianVisit`, milik `ClinicalManagement`, memakai prefix registry `Cli` |
| Kolom baru | 3 pada catatan dokter, 5 pada catatan terpadu, 3 pada tindakan, 3 pada resep, 1 pada pesanan laboratorium, 1 pada pesanan radiologi |
| Yang dipakai ulang apa adanya | Mesin koreksi dokumen rekam medis, mesin fakta klinis ke Billing, komponen dasar klinis frontend |
| Perubahan perilaku pada modul lain | **Tiga** — perbaikan jalur tanpa antrean, `INT-DOK-01`, dan `INT-DOK-02` |
| Endpoint baru | **14 rencana**, turun dari 17 karena jalur koreksi memakai endpoint yang sudah ada. `0.3` **tidak menambah endpoint baru**: koreksi atas nama penulis lain dan penetapan berhalangan keduanya sudah ada di source |
| Perubahan perilaku tambahan pada `0.3` | **Satu** — pendaftaran tiga jenis dokumen ke mesin keutuhan saat finalisasi. Nol perubahan bentuk data |
| Prasyarat registry | Baris `RadiologyManagement / Rad` dinaikkan dari `PLANNED` menjadi `ACTIVE` |

---

## 13. Sasaran kemampuan API

Daftar lengkap ada di [`contracts/api-contract.md`](./contracts/api-contract.md) `0.2.0`. Di bawah
ini hanya endpoint yang **belum ada** dan menjadi sasaran MVP.

### Health Services / Clinical Management / Physician Visit

Base URL: `api/v1/health-services/clinical-management/physician-visits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat visite | `PhysicianVisit : Create` | `CreatePhysicianVisitRequest` | `ApiResponse<PhysicianVisitResponse>` | `EPIC DOK-05` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Riwayat visite satu perawatan | `PhysicianVisit : Read` | — | `ApiResponse<PagedResult<PhysicianVisitListItem>>` | `EPIC DOK-05` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Satu kejadian beserta tautannya | `PhysicianVisit : Read` | — | `ApiResponse<PhysicianVisitResponse>` | `EPIC DOK-05` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan kejadian salah catat | `PhysicianVisit : Cancel` | `CancelPhysicianVisitRequest` | `ApiResponse<PhysicianVisitResponse>` | `EPIC DOK-05` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/links` | Menautkan dokumen | `PhysicianVisit : Update` | `UpdatePhysicianVisitLinksRequest` | `ApiResponse<PhysicianVisitResponse>` | `EPIC DOK-05` | **Rencana (belum tersedia)** |

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/verify` | DPJP memverifikasi | `PatientIntegratedProgressNote : Verify` | — | `ApiResponse<ProgressNoteResponse>` | `EPIC DOK-04` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Lini masa lintas profesi satu perawatan | `PatientIntegratedProgressNote : Read` | Query | `ApiResponse<PagedResult<...>>` | `EPIC DOK-04` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/verification-status` | Yang menunggu dan yang terlambat | `PatientIntegratedProgressNote : Read` | — | `ApiResponse<VerificationStatusResponse>` | `EPIC DOK-04` | **Rencana (belum tersedia)** |

### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/soap-timeline` | Lini masa catatan terurut waktu pemeriksaan | `DoctorConsultation : Read` | Query | `ApiResponse<SoapTimelineResponse>` | `EPIC DOK-03` | **Rencana (belum tersedia)** |

### Lainnya

| Method | Path | Kegunaan | Hak akses | Epic | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/patient-assessments/episodes/{episodeId}` | Kajian satu perawatan | `PatientAssessment : Read` | `EPIC DOK-02` | **Rencana (belum tersedia)** |
| `GET` | `/patient-procedures/episodes/{episodeId}` | Tindakan satu perawatan | `PatientProcedure : Read` | `EPIC DOK-06` | **Rencana (belum tersedia)** |
| `GET` | `/prescriptions/episodes/{episodeId}` | Resep satu perawatan | `Prescription : Read` | `EPIC DOK-06` | **Rencana (belum tersedia)** |
| `GET` | `/lab-orders/episodes/{episodeId}` | Pesanan laboratorium satu perawatan | `LabOrder : Read` | `EPIC DOK-06` | **Rencana (belum tersedia)** |
| `GET` | `/rad-orders/episodes/{episodeId}` | Pesanan radiologi satu perawatan | `RadOrder : Read` | `EPIC DOK-06` | **Rencana (belum tersedia)** |

### Endpoint yang dipakai ulang tanpa dibuat baru

| Method | Path | Kegunaan | Hak akses | Epic | Status |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/clinical-note-addendums/by-document/{documentKind}/{documentId}` | Mengoreksi catatan sendiri yang sudah final | `ClinicalNoteAddendum : Create` | `EPIC DOK-03` | **Tersedia** |
| `POST` | `/clinical-note-addendums/by-document/{documentKind}/{documentId}/as-substitute` | Mengoreksi atas nama dokter yang berhalangan | `ClinicalNoteAddendum : CreateAsSubstitute` | `EPIC DOK-03` | **Tersedia** |
| `POST` | `/clinical-note-author-delegations` | Menerbitkan penetapan berhalangan | `ClinicalNoteAuthorDelegation : Create` | `EPIC DOK-03` | **Tersedia** |

---

## 14. Matriks kewenangan

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md)
`0.2.0`. Resource baru: `PhysicianVisit`. Action baru: `Verify` dan `Cancel`.

| Peran | Butir hak akses yang menentukan |
| --- | --- |
| DPJP | `PatientIntegratedProgressNote : Verify`, `PhysicianVisit : Create`, `PhysicianVisit : Cancel` |
| Dokter jaga | Sama, **kecuali** `PatientIntegratedProgressNote : Verify` |
| Supervisor klinis | `PhysicianVisit : Cancel` |
| Perawat | `PatientIntegratedProgressNote : Create`; tanpa `Verify` |
| Petugas admisi dan kasir | **Tidak ada** |

> **Peringatan `BE-RWI-034`.** Resource dan Action baru wajib memakai nama yang sama persis pada
> `[AccessAction]` dan `[AccessPermission]`, dan wajib diuji dengan peran non-SuperAdmin. Kelalaian
> yang sama pernah mengunci sembilan endpoint dan menahan tujuh task frontend.

---

## 15. Batas integrasi dan billing

| Batas | Yang **MUST NOT** dibuat sendiri oleh sub-modul ini |
| --- | --- |
| Farmasi | Menulis status pemenuhan atau menandai obat sudah diserahkan — `RUL-DOK-01` |
| Laboratorium dan Radiologi | Menulis atau menyalin hasil — `RUL-DOK-02` |
| Billing | Menghitung nominal, menetapkan tarif, atau mengagregasikan kejadian visite |
| Rekam medis | Jalur koreksi tandingan di luar mesin addendum yang sudah ada |
| Antrean | Baris antrean apa pun untuk pasien rawat inap — `RWI-RULE-026` aturan 2 |

Arah fakta klinis ke Billing **satu arah**: modul klinis menyatakan peristiwa, Billing menerimanya.
Kegagalan Billing tidak menghapus catatan klinis, dan agregasi Billing tidak menyentuh riwayat.

---

## 16. Guardrail regulasi

| Kewajiban | Yang dipenuhi MVP | Yang belum |
| --- | --- | --- |
| Rekam medis elektronik | Kajian medis, catatan harian, catatan terpadu, visite, resep, tindakan, dan pesanan penunjang tersimpan lengkap beserta pelaku dan waktunya | — |
| Keterlacakan | Setiap koreksi dan setiap verifikasi menyimpan pelaku, waktu, dan alasan | — |
| Pemisahan penulis dan penyetuju | Verifikator disimpan terpisah dari penulis asli | — |
| Koreksi rekam medis | Koreksi beralasan; versi lama tidak pernah hilang; perawatan yang sudah ditutup tetap dapat dikoreksi tanpa dibuka kembali | Jaminan bahwa dokumen terkunci menerima koreksi — `INT-DOK-07` |
| Bukti kunjungan dokter | Kejadian visite lengkap beserta pembatalan beralasan | — |
| Masa simpan | — | `RWI-OQ-035` menunggu pemilik hukum |
| Batas waktu klinis | Mekanisme pemantauannya siap dan dapat dikonfigurasi | Angkanya — `RWI-RULE-021` dan kebijakan verifikasi |

---

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Sasaran |
| --- | --- | --- |
| `NFR-001` | Keselamatan konteks | Konteks pasien dan penanda alergi tampil sebelum satu pun tombol tulis aktif |
| `NFR-002` | Idempotency | Wajib pada visite, resep, dan tindakan. Pada visite kuncinya **wajib terisi** |
| `NFR-003` | Concurrency | Seluruh unique index diuji terhadap **PostgreSQL sungguhan**, bukan provider InMemory |
| `NFR-004` | Atomicity | Catatan klinis dan jejak auditnya tersimpan dalam satu transaksi |
| `NFR-005` | Otorisasi | Kewenangan atas **pasien tertentu** diperiksa di dalam setiap perintah klinis |
| `NFR-006` | Privasi | Kolom sensitif tidak masuk logger dan tidak tampil pada daftar ringkas |
| `NFR-007` | Regresi | Setiap task yang menyentuh mesin klinis membawa test regresi poliklinik dan IGD — `RWI-DEC-051` |
| `NFR-008` | Penanganan waktu | Waktu klinis disimpan terpisah dari waktu pencatatan, dan divalidasi terhadap waktu masuk kamar |

---

## 18. Skenario UAT

### `EPIC DOK-01`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-01` | **Berhasil** | Tn. Budi sudah di kamar | dr. Andi membuka Tn. Budi lalu menulis catatan | Tersimpan tanpa diminta nomor antrean |
| `UAT-DOK-02` | **Berhasil** | Catatan hari pertama sudah ada | dr. Andi menulis catatan kedua keesokan harinya | Dua catatan pada satu kunjungan, keduanya tersimpan |
| `UAT-DOK-23` ★ | **Berhasil** | Pasien tanpa baris antrean | Menyimpan catatan | Tersimpan; **tidak** muncul kegagalan sistem, dan jumlah baris antrean tidak berubah |
| `UAT-DOK-03` | **Gagal** | Pasien poliklinik | Dokter poliklinik menulis catatan tanpa antrean | Ditolak dengan pesan **sama persis** seperti sebelumnya |

### `EPIC DOK-02`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-04` | **Berhasil** | Kajian medis sudah selesai | dr. Andi menulis catatan harian selama tiga hari | Isi kajian medis awal **sama persis** seperti hari pertama |
| `UAT-DOK-05` | **Gagal** | Kajian medis belum berisi diagnosis | Menyelesaikan kajian medis | Ditolak; bagian yang kosong disebut satu per satu |
| `UAT-DOK-06` | **Gagal** | Pengguna berperan perawat | Ns. Sari mencoba menulis kajian medis | Ditolak: hanya dokter |

### `EPIC DOK-03`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-07` | **Berhasil** | Pemeriksaan pukul 07.40 | dr. Andi menulis catatannya pukul 11.00 | Lini masa menempatkannya pada urutan pukul 07.40 |
| `UAT-DOK-08` | **Berhasil** | Pengkajian perawat belum selesai | dr. Andi menulis catatan | Tersimpan; tidak ada penolakan |
| `UAT-DOK-09` | **Berhasil** | Perawatan sudah ditutup | Catatan lama dibetulkan lewat koreksi beralasan | Tersimpan; perawatan **tetap** tertutup, tempat tidur tidak berubah |
| `UAT-DOK-10` | **Gagal** | Perawatan sudah ditutup | Menulis catatan **baru** | Ditolak, dan pesannya menyebutkan koreksi tetap bisa |
| `UAT-DOK-30` ★ | **Berhasil** | Catatan baru saja diselesaikan | dr. Andi sadar salah ketik dua menit kemudian, lalu menambah koreksi beralasan | Isi asli tetap terbaca; koreksi tampil sebagai baris tersendiri beserta alasan dan waktunya |
| `UAT-DOK-31` ★ | **Berhasil** | dr. Andi sedang cuti dan kepala unit sudah menerbitkan penetapan | dr. Sinta selaku DPJP pengganti menambah koreksi pada catatan dr. Andi | Tersimpan; **nama dr. Andi tetap sebagai penulis catatan**, dr. Sinta tercantum pada baris koreksi sebagai pengganti |
| `UAT-DOK-32` ★ | **Gagal** | Catatan masih berstatus konsep | Mencoba menambah koreksi | Ditolak, dan pesannya mengarahkan menyunting langsung |
| `UAT-DOK-33` ★ | **Gagal** | Penetapan berhalangan dikirim tanpa tanggal berakhir | Kepala unit menyimpan penetapan | Ditolak; penetapan tidak terbentuk |
| `UAT-DOK-34` ★ | **Gagal** | Penetapan atas nama dr. Andi berlaku | dr. Rina, yang memegang butir hak akses pengganti tetapi **bukan** DPJP pasien itu, mencoba mengoreksi | Ditolak. Pemeriksaan hak aksesnya **lolos**; yang menolak adalah aturan kewenangan atas pasien |

### `EPIC DOK-04`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-11` | **Berhasil** | Ns. Sari menulis catatan terpadu | dr. Andi memverifikasinya | Terverifikasi; **nama Ns. Sari tetap sebagai penulis**, dr. Andi sebagai verifikator |
| `UAT-DOK-12` | **Berhasil** | Kebijakan verifikasi belum diisi | Supervisor membuka daftar pantau | Berbunyi "verifikasi tidak diwajibkan", **bukan** daftar kosong yang menyesatkan |
| `UAT-DOK-13` | **Gagal** | dr. Rina bukan DPJP pasien itu | dr. Rina mencoba memverifikasi | Ditolak |

### `EPIC DOK-05`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-14` | **Berhasil** | Belum ada visite hari itu | dr. Andi mencatat visite pukul 07.40, menulis catatannya pukul 07.52 | Riwayat menampilkan visite pukul **07.40** sejak dicatat |
| `UAT-DOK-15` | **Berhasil** | Belum ada visite | dr. Andi menulis tiga catatan tanpa mencatat visite | Riwayat visite **tetap kosong** — dan itu benar |
| `UAT-DOK-16` | **Berhasil** | Koneksi lambat | Tombol catat visite tertekan dua kali | **Satu** kejadian tercatat |
| `UAT-DOK-24` ★ | **Berhasil** | Visite pagi sudah tercatat | Pukul 16.10 dr. Andi datang lagi lalu mencatat visite kedua | **Dua** baris riwayat; hitungan hari itu **dua** |
| `UAT-DOK-25` ★ | **Berhasil** | Jam terisi keliru | dr. Andi membatalkan beralasan lalu mencatat ulang | Dua baris: satu batal beserta alasannya, satu berlaku; hitungan **satu** |
| `UAT-DOK-26` ★ | **Berhasil** | Dua kejadian pada tanggal yang sama | Billing menerbitkan satu tagihan harian | Riwayat klinis **tetap menampilkan dua kejadian** tanpa perubahan waktu maupun dokter |
| `UAT-DOK-17` | **Gagal** | Pengguna berperan perawat | Ns. Sari mencoba mencatat visite atas nama dr. Andi | Ditolak |
| `UAT-DOK-27` ★ | **Gagal** | Kejadian sudah dibatalkan | Membatalkannya sekali lagi | Ditolak dengan keterangan sudah dibatalkan |

### `EPIC DOK-06`

| ID | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-DOK-18` | **Berhasil** | Perawatan berjalan lima hari | dr. Andi meresepkan setiap hari | Lima resep tersimpan pada satu perawatan |
| `UAT-DOK-19` | **Berhasil** | Pasien akan pulang | dr. Andi membuat resep obat pulang | Tersaring tersendiri sebagai obat pulang |
| `UAT-DOK-20` | **Berhasil** | Sistem tagihan sedang mati | Tindakan dicatat | Catatan klinis tersimpan; penerbitan fakta ditandai gagal |
| `UAT-DOK-28` ★ | **Berhasil** | Perawatan berjalan | dr. Andi memesan pemeriksaan radiologi lalu membaca hasil finalnya | Pesanan tercatat di modul Radiologi; hasil terbaca tanpa baris salinan |
| `UAT-DOK-29` ★ | **Berhasil** | Hasil laboratorium belum disahkan | Dokter membuka daftar hasil | Ditampilkan dengan penanda **"belum final"**, bukan sebagai hasil sah |
| `UAT-DOK-21` | **Gagal** | Resep sudah dikirim | dr. Andi mencoba menandai obat sudah diserahkan | Ditolak: hanya petugas Farmasi |
| `UAT-DOK-22` | **Gagal** | Dua perawatan berbeda | Pesanan perawatan A dibuka dari perawatan B | Ditolak |

---

## 19. Definition of Done

| No | Butir | Bukti |
| ---: | --- | --- |
| 1 | **Jalur tanpa antrean tidak lagi gagal**, dan tidak menyentuh data antrean | `UAT-DOK-23`, matriks acceptance bagian 0 hijau |
| 2 | Jalur IGD dan poliklinik terbukti tidak rusak | Test regresi bagian 0 hijau |
| 3 | Konteks klinis terpasang; catatan rawat inap dapat dibuat tanpa antrean | `AC-CAP022-01`, `AC-CAP023-01` hijau |
| 4 | Catatan dan resep kedua diterima untuk rawat inap, **dan tetap ditolak untuk rawat jalan** | `RWI-AC-143` hijau |
| 5 | Kajian medis dan catatan harian terbukti record serta lifecycle berbeda | `AC-CAP022-02` hijau |
| 6 | Waktu pemeriksaan terpisah dari waktu penulisan, lini masa terurut benar | `FR-DOK-013` hijau |
| 7 | Perawatan tertutup menolak catatan baru **dan** menerima koreksi tanpa membuka tempat tidur | `AC-CAP020-03` hijau |
| 8 | Verifikasi tidak mengubah penulis asli | `AC-CAP021-03` hijau |
| 9 | Verifikasi hanya oleh DPJP yang aktif saat itu | `VAL-DOK-07`, `INV-DOK-11` hijau |
| 10 | Catatan tanpa kejadian visite tidak menambah hitungan visite | `RWI-AC-151` hijau |
| 11 | **Dua visite nyata pada tanggal sama menghasilkan hitungan dua** | `RWI-AC-154` hijau |
| 12 | **Kiriman ulang berkunci sama tetap satu kejadian**, terbukti pada PostgreSQL sungguhan | `RWI-AC-152`, `RWI-AC-155` hijau |
| 13 | **Kejadian yang dibatalkan tetap tersimpan dan tidak dihitung** | `INV-DOK-08` hijau |
| 14 | **Agregasi tagihan tidak mengubah riwayat klinis** | `RWI-AC-156` hijau |
| 15 | Kegagalan penagihan tidak menghilangkan catatan tindakan | `UAT-DOK-20` hijau |
| 16 | Obat pulang terbedakan dari resep harian | `AC-CAP023-03` hijau |
| 17 | Pesanan laboratorium **dan radiologi** tidak dapat dipakai lintas perawatan | `AC-CAP015-01` hijau untuk keduanya |
| 18 | Hasil penunjang terbaca tanpa baris salinan, dan yang belum final ditandai | `AC-CAP015-02`, `VAL-DOK-30` hijau |
| 19 | **Nol jalur tulis** menuju status pemenuhan resep dan hasil penunjang | Architecture test hijau |
| 20 | **Nol tabel `Inp*`** untuk dokumentasi dokter | Architecture test hijau |
| 21 | **Nol entity baru berawalan `Trx*`**; entity visite bernama `CliPhysicianVisit` | Architecture test hijau |
| 22 | **Nol baris antrean** dibuat untuk pasien rawat inap | Architecture test hijau |
| 23 | Resource dan Action baru berfungsi bagi peran non-SuperAdmin | Test hak akses per peran hijau |
| 24 | **Ruang kerja dokter membaca census episode**, tanpa aksi antrean | Test frontend bagian 8 hijau |
| 25 | Delapan layar terjangkau sesuai `IA-INP-01` dan `IA-INP-05` | Bukti navigasi |
| 26 | Kolom sensitif tidak muncul di logger | Pemeriksaan payload log |
| 27 | Baris registry `Rad` sudah `ACTIVE` | Registry kepemilikan prefix |
| 28 | **Memfinalkan catatan sekaligus mendaftarkannya ke mesin keutuhan**, dan kegagalan pendaftaran membatalkan finalisasi | `RWI-AC-157` hijau |
| 29 | Catatan final dapat dikoreksi, dan catatan konsep menolak koreksi | `RWI-AC-158`, `RWI-AC-159` hijau |
| 30 | Kajian medis dan tindakan ikut dapat dikoreksi | `RWI-AC-162` hijau |
| 31 | Koreksi atas nama dokter lain **tidak mengubah penulis aslinya** | `RWI-AC-164` hijau |
| 32 | Penetapan berhalangan tanpa masa berlaku ditolak | `RWI-AC-165` hijau |
| 33 | **Hanya DPJP aktif episode itu** yang dapat mengoreksi atas nama dokter lain | `RWI-AC-167` hijau |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Urutan pengiriman

| Gelombang | Isinya | Syarat mulai |
| --- | --- | --- |
| **`DOK-MVP-0`** | **Perbaikan jalur tanpa antrean** beserta test regresi IGD dan poliklinik | Blueprint disetujui |
| **`DOK-MVP-0b`** ★ | **Pendaftaran catatan dokter, kajian medis, dan tindakan ke mesin keutuhan saat finalisasi.** Nol perubahan bentuk data | `DOK-MVP-0` selesai. Menutup celah catatan final yang tidak dapat dikoreksi |
| **`DOK-MVP-1`** | `INT-DOK-01` dan `INT-DOK-02`; kolom baru pada enam tabel; enum baru; `CliPhysicianVisit`; baris registry `Rad` | `DOK-MVP-0` selesai; **dikerjakan bersama `KEP-MVP-0`** |
| **`DOK-MVP-2`** | `EPIC DOK-01`, `EPIC DOK-02` — pintu masuk dan kajian medis | `DOK-MVP-1` |
| **`DOK-MVP-3`** | `EPIC DOK-03` — catatan harian, waktu pemeriksaan, koreksi pada perawatan tertutup | `DOK-MVP-2` |
| **`DOK-MVP-4`** | `EPIC DOK-05` — visite beserta pembatalan dan hitungannya | `DOK-MVP-2` |
| **`DOK-MVP-5`** | `EPIC DOK-06` — resep, tindakan, laboratorium, radiologi | `DOK-MVP-2` |
| **`DOK-MVP-6`** | `EPIC DOK-04` — catatan terpadu dan verifikasi | `DOK-MVP-3`; **paling akhir** karena butuh catatan yang sudah ada untuk diverifikasi |
| **`DOK-MVP-FE`** | **Rework ruang kerja dokter** ke census episode, pencabutan aksi antrean, dan pemindahan butir menu | `DOK-MVP-2`; **wajib selesai sebelum rilis apa pun** |
| **`POST-MVP`** | Nilai batas waktu verifikasi dan kajian; pencatatan visite atas nama dokter; penagihan visite; pembacaan balik penyerahan obat pulang | Clinical Governance; pemilik klinis; pemilik Billing; pemilik Farmasi |

**Nol epic `OPEN DECISION`, sehingga nol epic yang tertahan di luar gelombang.**

> **`DOK-MVP-0` berada di urutan nol dan tidak boleh dilewati.** Ia tidak menambah satu pun
> kemampuan, tetapi tanpanya seluruh gelombang berikutnya dibangun di atas jalur yang sudah
> diketahui gagal.

### 20.2 Pertanyaan terbuka sebelum development lock

| No | Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| ---: | --- | --- | --- | :---: |
| 1 | **Kajian medis memakai ulang tabel pengkajian dengan pembeda jenis, atau bentuk penyimpanan tersendiri?** Blueprint memilih pakai ulang; keberatannya ada di `02-backend-architecture.md` bagian 4.2 | Product/Domain bersama pemilik `ClinicalManagement` | Bila dipilih bentuk tersendiri, yang berubah hanya arsitektur dan kamus data; kontrak API, kewenangan, dan alur tetap | Tidak |
| 2 | Apakah verifikasi DPJP atas catatan terpadu **diwajibkan** di rumah sakit ini, dan berapa batas waktunya? | Clinical Governance | Mekanismenya tetap dibangun; bawaan tidak diwajibkan | Tidak |
| 3 | Berapa batas waktu kajian medis awal? `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | Daftar pantau berjalan tanpa angka | Tidak |
| 4 | Apakah pencatatan visite atas nama dokter oleh petugas diizinkan? | Clinical Governance | Bawaan aman: hanya dokter | Tidak |
| ~~5~~ | ~~Apakah dokumen klinis yang sudah terkunci tetap menerima koreksi?~~ | Pemilik `MedicalRecordManagement` | **TERTUTUP 2026-09-02.** Jawabannya lebih tegas dari dugaan: koreksi **hanya** diterima pada dokumen terkunci. Sambil menjawabnya ditemukan celah yang lebih serius, ditutup `RWI-DEC-086` dan `RWI-DEC-087` | ~~Ya~~ → **Tidak** |
| 6 | Kapan baris registry `RadiologyManagement / Rad` dinaikkan menjadi `ACTIVE`? | Pemilik registry | Selisih registry terhadap source tetap terbuka | Tidak — penambahan kolom pada entity yang sudah ada tidak terhalang |

> **Nol pertanyaan memblokir.** Pertanyaan 5 tertutup pada Amendment Pass 2026-09-02, dan
> jawabannya justru memperluas pekerjaan: bukan hanya memastikan dokumen terkunci menerima koreksi,
> melainkan **mendaftarkan tiga jenis dokumen** ke mesin keutuhan supaya koreksinya mungkin sama
> sekali. Dokumen ini tetap berstatus `draft` dan approval manusia belum tergantikan, tetapi tidak
> ada lagi pertanyaan yang menahannya diteruskan ke `/plan-module-delivery`.

## 21. Gelombang 1A — Rawat Inap Safety Corrections

**Ditambahkan 11 September 2026**, menyerap `RWI-DEC-098` dan `RWI-DEC-099`. Bagian ini menambahkan
satu gelombang perbaikan di atas MVP yang sudah dirancang, dan menurunkan dari
`contracts/api-contract.md` `0.5.0` beserta kedua matriks `0.5.0`.

### 21.1 Dua penyimpangan yang dibereskan

| Penyimpangan | Keadaan hari ini di source | Kenapa berbahaya |
| --- | --- | --- |
| Catatan terpadu dapat dihapus | `DELETE /{id}` melakukan soft delete tanpa pemeriksaan status, tanpa alasan, dan tanpa memeriksa penulis | Catatan hilang dari seluruh pembacaan normal **tanpa menyisakan keadaan**. Pembaca berikutnya tidak punya cara mengetahui pernah ada sesuatu di sana |
| Penulis klinis diambil dari payload | `DoctorId` berasal dari antrean, permintaan, atau kunjungan | Dokter mana pun yang memegang butir hak akses dapat menulis untuk pasien mana pun, dan catatan dapat tersimpan atas nama dokter lain |

### 21.2 Batas gelombang ini

**Titik mulai.** Kontrak `0.5.0` `draft`; keputusan `RWI-DEC-098` dan `RWI-DEC-099` `approved`;
kolom peran pada `InpDoctorAssignment` sudah dirancang `episode-rawat-inap`.

**Titik akhir.**

1. `DELETE` pada catatan terpadu tidak tersedia; jawabannya `404`.
2. Pembatalan memanggil pemeriksaan keutuhan dokumen lebih dulu.
3. Kelima grup jalur tulis mengirim dokter pelaku ke resolver.
4. Seluruh acceptance criteria `AC-DOK-060` s.d. `AC-DOK-075` lulus.

### 21.3 Epic dan functional requirement

| ID | Epic | Prioritas | Disposisi |
| --- | --- | --- | --- |
| `EPIC DOK-08` | Catatan klinis final tidak dapat disembunyikan | `P0` | `REPAIR` — mesin keutuhan sudah ada, hanya tidak dipanggil |
| `EPIC DOK-09` | Penulis klinis berasal dari pengguna terautentikasi | `P0` | `REPAIR` — penjaga sudah ada, hanya mati |

| FR | Bunyi requirement | Epic | Disposisi |
| --- | --- | --- | --- |
| `FR-DOK-060` | Jalur `DELETE` pada catatan terpadu tidak tersedia | `DOK-08` | `EXTEND` |
| `FR-DOK-061` | Pembatalan menolak catatan `Final` dan `Terverifikasi`, dan mengarahkannya ke addendum | `DOK-08` | `REPAIR` |
| `FR-DOK-062` | Catatan yang dibatalkan tetap terbaca pada rekam medis beserta alasan dan pelakunya | `DOK-08` | `EXISTING / REUSE` |
| `FR-DOK-063` | Penulis diambil dari `ApplicationUser.DoctorId`; tanpa pemetaan aktif, penulisan ditolak | `DOK-09` | `MISSING / NEW` |
| `FR-DOK-064` | `DoctorId` pada payload tidak menentukan penulis; mismatch ditolak | `DOK-09` | `REPAIR` |
| `FR-DOK-065` | Penulis wajib punya penugasan aktif pada episode, dinilai pada waktu klinis | `DOK-09` | `REPAIR` |
| `FR-DOK-066` | Kelima grup jalur tulis memanggil resolver dengan dokter pelaku | `DOK-09` | `REPAIR` |
| `FR-DOK-067` | Verifikasi CPPT tidak mengubah penulis aslinya, dan verifikator tunduk aturan yang sama | `DOK-09` | `EXTEND` |
| `FR-DOK-068` | Kewenangan konsulen atas keputusan pulang ditolak selama kebijakannya belum ada | `DOK-09` | `OPEN DECISION` — perilaku fail-closed, tidak masuk gelombang pengiriman mana pun |

### 21.4 Skenario UAT

**Jalur berhasil.** DPJP menulis catatan untuk pasiennya. Konsulen dengan penugasan aktif menulis
catatan konsultasi. Dokter membatalkan catatan draf yang salah, beserta alasannya, dan catatan itu
tetap terbaca sebagai dibatalkan. Dokter mengoreksi catatan final lewat addendum, dan isi aslinya
tetap utuh.

**Jalur gagal.** Dokter tanpa penugasan menulis dan ditolak. Dokter mengirim identitas dokter lain
dan ditolak tanpa satu pun baris tersimpan. Dokter membatalkan catatan yang sudah terverifikasi dan
ditolak dengan arahan ke addendum. Pemanggilan `DELETE` dijawab sebagai route yang tidak ada.

### 21.5 Definition of Done gelombang ini

| Butir | Cara menjawabnya |
| --- | --- |
| `DELETE` hilang dari controller catatan terpadu | Pencarian source mengembalikan nol `HttpDelete` pada berkas itu |
| Pembatalan memanggil pemeriksaan keutuhan | `AC-DOK-061` dan `AC-DOK-062` lulus |
| Kelima grup mengirim dokter pelaku | `AC-DOK-072` lulus, **lima test terpisah** |
| Penilaian memakai waktu klinis | `AC-DOK-070` dan `AC-DOK-071` lulus |
| Regresi Rawat Jalan lulus | `AC-DOK-066` lulus. **Wajib**, karena controller dipakai bersama |
| Skenario negatif memakai peran nyata | `AC-DOK-075` lulus; peran yang dipakai tercatat pada laporan task |
| Nol butir hak akses baru | Daftar permission sebelum dan sesudah sama persis |

**Satu jebakan yang dinyatakan terbuka.** Seluruh test hak akses lama akan tetap lulus tanpa
disentuh, baik sebelum maupun sesudah perbaikan, karena gelombang ini tidak melahirkan Resource
maupun Action baru. Test hak akses hijau **bukan** bukti bahwa penjaga baru bekerja.

### 21.6 Yang sengaja di luar gelombang ini

| Yang ditunda | Alasan |
| --- | --- |
| Delapan jalur hapus lain pada `ClinicalManagement` | `RWI-DEC-098` memilih menutup dua saja. Konsekuensinya diterima pemilik dan ketegangannya dilacak `RWI-OQ-055` |
| Menaikkan sembilan `ClinicalDocumentKind` yang belum ditegakkan | Keputusan pemilik `MedicalRecordManagement`; dilacak `V2-UNK-01` |
| Kewenangan konsulen memutuskan pulang | `OPEN-MVP-004` belum dijawab; perilaku fail-closed berlaku |

---

## 22. Penyelarasan `PRD-RWI-V2-001` — revision `0.4` ★ 15 September 2026

Bagian ini **menambah** rilis pertama dengan isi ruang kerja dokter V2, mengikuti dua puluh bagian wajib dokumen PRD ke
MVP. Bagian 1 s.d. 21 tetap berlaku kecuali yang disebut digantikan pada 22.1. Seluruh entity, status, hak akses, dan
endpoint di bawah **diturunkan** dari `02-backend-architecture.md` `0.5` bagian 11 dan kontrak `0.6.0`; tidak ada yang
diciptakan di sini.

### 22.1 Identitas dokumen

| Field | Nilai |
| --- | --- |
| Revision artefak | **`0.4`** |
| `contract_version` | **`0.6.0`** |
| Status | **`draft`** — approval manusia belum ada |
| Commit SHA baseline | Backend `df3679c0d5b2f08106702153eb242d3a6cb2929b`; frontend `1ce219b40f8e411f3c4e66975626ab33ae81616a` |
| Masukan hulu | `PRD-RWI-V2-001` v`2.0`, SHA-256 `2b3b2f29…0a679f`, dibaca berlapis `RWI-DEC-110` |
| Decision log | Revision `21`, SHA-256 `1c55c80a…2d45102a` |
| Gate | `1.6`, SHA-256 `f31d207a…e49b5300` — `INP-S17` s.d. `INP-S21` `READY_FOR_DOMAIN_DESIGN` |
| Arsitektur domain | `0.2` `DOMAIN_ARCHITECTURE_READY` untuk tujuh capability lama; `DOMAIN_ARCHITECTURE_NOT_RUN` untuk isi baru dengan alasan gate `1.6` bagian 15.15 |
| Ringkasan cakupan | Ruang kerja dokter satu halaman sama dengan Rawat Jalan, delapan tab, kewenangan penulis, Catatan Saya, verifikasi DPJP, Resep Harian, template, rekonsiliasi, sliding scale, pesanan dengan instruksi, Resume Medis, penunjang enam layanan |

| Bagian lama | Digantikan menjadi |
| --- | --- |
| 5 "Di luar batas: resume pulang" | Resume ditulis dan ditandatangani dari tab Resume Medis; datanya tetap milik `episode-rawat-inap` |
| 6 Pelaku sasaran | 22.6 |
| 7 Tujuh kemampuan | Tetap wajib; ditambah 22.7 |
| 9 `FLOW-DOK-MVP-001` langkah 2 "dari census" | Dari Dokter → Rawat Inap berdasarkan penugasan dokter login — 22.9 |
| 20.1 `DOK-MVP-FE` "pemindahan butir menu" | Butir menu **dipertahankan** — `RWI-DEC-107` |
| 21.3 `FR-DOK-065` "dinilai pada waktu klinis" | Ditambah syarat penugasan aktif saat disimpan — `FR-DOK-076` |

### 22.2 Ringkasan eksekutif

Dokter rawat inap hari ini membuka dua halaman yang tidak mirip ruang kerja Rawat Jalan, dapat menyelesaikan konsep
catatan dokter lain atas nama penulisnya, dapat memverifikasi CPPT walaupun bukan DPJP, tidak punya Resep Harian,
rekonsiliasi obat, maupun sliding scale, dan tidak punya jalan menambah koreksi setelah penugasannya berakhir.

Penyelarasan ini mengejar tiga hasil: **dokter bekerja di layar yang sama dengan poliklinik** sehingga tidak perlu
belajar ulang; **setiap tanda tangan klinis benar-benar milik penulisnya** dan tidak ada catatan yang terbuka setelah
perawatan ditutup; **siklus obat rawat inap utuh** dari obat bawaan, resep, protokol insulin, sampai penghentian obat
yang langsung membatalkan dosis berikutnya.

### 22.3 Masalah produk

| Kondisi hari ini | Bukti | Akibat |
| --- | --- | --- |
| Ruang kerja dua halaman, pustaka komponen berbeda dari Rawat Jalan | `RLN3-CAP-04` | Melanggar `UI-AC-DOK-001` s.d. `012` |
| Konsep SOAP, kajian medis, dan CPPT dapat diselesaikan orang lain | `RLN3-CAP-33` s.d. `36`, `RWI-FACT-029` | Catatan tertanda tangan atas nama orang yang tidak menandatanganinya |
| Verifikasi CPPT menerima peran apa pun | `RLN3-CAP-25` | Konsulen memverifikasi atas nama tanggung jawab DPJP |
| Tindakan baru diterima pada episode tertutup | `RLN3-CAP-35` | Rekam medis bertambah setelah perawatan ditutup |
| Konsep SOAP dan kajian tidak ikut terkunci saat penutupan | `RWI-FACT-039`, `RLN3-CAP-38` | Konsep dapat disunting selamanya |
| Template resep dapat dibuat atas nama dokter lain; pemakaiannya gagal total bila satu obat bermasalah; tanpa pemeriksaan alergi | `RWI-FACT-035`, `RLN3-CAP-39` s.d. `44` | Risiko obat dan kepemilikan |
| Rekonsiliasi obat, sliding scale, Resep Harian belum ada | `RWI-FACT-032`, `V2-CAP-10` | Kemunduran fungsi dibanding V1 |

**Yang sudah ada dan dipakai ulang:** mesin keutuhan dan addendum, registrasi idempoten, daftar konsep milik penulis,
diagnosis terstruktur, visite, pesanan laboratorium dan radiologi, resep berbutir wajib, master obat non-formularium.

### 22.4 Visi produk

1. Kepala ruangan menetapkan siapa dokter yang bertugas atas pasien.
2. Dokter membuka satu halaman dan hanya melihat pasien yang boleh ia dokumentasikan.
3. Setiap catatan lahir sebagai konsep yang terdaftar, hanya diselesaikan penulisnya, dan terkunci saat perawatan ditutup.
4. Obat yang dibawa pasien dicatat perawat, diputuskan dokter, dan menjadi resep bila dilanjutkan.
5. Insulin berdosis skala dipesan dari protokol yang disahkan, dan dilaksanakan perawat dari angka GDS bangsal.
6. Pesanan yang dibuat perawat atas instruksi telepon langsung berjalan dan diverifikasi dokternya belakangan.
7. DPJP memverifikasi, menandatangani resume, dan dokter mana pun dapat melengkapi catatannya sendiri lewat addendum.

### 22.5 Batas MVP penyelarasan ini

**Titik mulai**

1. Episode `Admitted` dan penugasan dokter tersedia, termasuk jalur tulis penugasan konsulen dan dokter jaga milik `episode-rawat-inap` `0.9.0`.
2. Komponen tata letak Rawat Jalan sudah diekstraksi atas persetujuan pemilik `rawat-jalan` — `INT-DOK-21`.
3. Sekurang-kurangnya satu protokol sliding scale berversi `Approved` untuk lingkungan uji.

**Titik akhir**

1. Dokter menjalankan satu pasien dari daftar miliknya sampai resume bertanda tangan pada satu halaman.
2. Tidak ada konsep catatan dokter rawat inap yang dapat diselesaikan selain penulisnya, dan tidak ada konsep yang terbuka setelah penutupan.
3. Verifikasi CPPT hanya oleh DPJP aktif atau DPJP terakhir.
4. Rekonsiliasi obat, Resep Harian, template, dan sliding scale tersedia pada rilis yang sama dengan MAR `keperawatan`.
5. Pesanan perawat atas instruksi terverifikasi dokter pemberi instruksi.
6. Perilaku poliklinik dan IGD terbukti tidak berubah, kecuali perubahan template yang sudah diberitahukan.

**Di luar batas:** handover shift, transfusi, integrasi Gizi/Hemodialisa/Bank Darah/Rehab Medik, Resume ODC, notifikasi
aktif, template bersama tingkat SMF.

### 22.6 Pelaku sasaran

| Pelaku | Tanggung jawab dalam penyelarasan |
| --- | --- |
| DPJP | Seluruh dokumentasi; verifikasi CPPT; tanda tangan resume; membatalkan pesanan orang lain |
| Konsulen, dokter jaga | Dokumentasi, resep, rekonsiliasi, sliding scale, pesanan; tanpa verifikasi CPPT dan tanpa tanda tangan resume |
| Dokter dengan penugasan singkat | Menulis catatan terlambat; tanpa kewenangan DPJP |
| Kepala ruangan, supervisor | Membuat penugasan, termasuk penugasan singkat |
| Perawat | Mencatat obat bawaan; memesan atas instruksi dari ruang kerja keperawatan; membaca Resep Harian |
| Pengubah dan pengesah konfigurasi farmasi-klinis | Menyiapkan dan mengesahkan protokol sliding scale, dua orang berbeda |
| Apoteker | Membaca protokol dan keputusan rekonsiliasi |
| Pengguna berhak mendaftarkan obat | Mendaftarkan obat bawaan non-formularium |

### 22.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Ruang kerja satu halaman dan daftar pasien dari penugasan | `CAP-020` s.d. `CAP-025` permukaan | Wajib; tanpa ini dokter tidak punya jalan masuk yang benar dan layar melanggar PRD |
| Kewenangan penulis, registrasi sejak konsep, penguncian, Catatan Saya | `CAP-020`, `CAP-021`, `CAP-022` | Wajib; tanpanya tanda tangan klinis tidak dapat dipercaya — celah keselamatan `RWI-FACT-029` |
| Verifikasi CPPT hanya DPJP dan jenis catatan | `CAP-021` | Wajib; `AC-RWI-011` |
| Resep Harian, penghentian butir, template, keputusan rekonsiliasi | `CAP-023-RSP` | Wajib; `RWI-DEC-133` menjadikan rekonsiliasi `P0` bersama MAR, dan penghentian butir satu-satunya cara menghentikan dosis |
| Protokol dan order sliding scale | `CAP-023-RSP` | Wajib; `RWI-DEC-145` memasukkannya ke batas rilis MAR |
| Pesanan tindakan dan penunjang dengan instruksi | `CAP-024`, `CAP-015-LAB`, `CAP-015-RAD` | Wajib; `RWI-DEC-114`, tanpanya instruksi telepon malam tidak dapat dicatat |
| Tab Resume Medis | `CAP-026` permukaan | Wajib; PRD bagian 21 dan `RWI-DEC-112` |
| Penunjang enam layanan sebagai permukaan | `CAP-015-GIZ`, `CAP-015-HD`, `CAP-015-BDR`, `CAP-015-RHB` | Wajib sebagai **permukaan jujur** "Integrasi belum tersedia"; tanpa backend |

### 22.8 Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Integrasi Gizi dan Bank Darah | `CAP-015-GIZ`, `CAP-015-BDR` | Keputusan pemilik 15 September 2026 mempertahankan `RWI-DEC-108`/`113` walaupun modulnya sudah ada di source; butuh kontrak integrasi yang belum dirancang | Pemesanan lewat modul Gizi dan Bank Darah langsung, di luar ruang kerja dokter |
| Hemodialisa, Rehab Medik | `CAP-015-HD`, `CAP-015-RHB` | Modul pemiliknya belum ada | Alur manual yang berlaku di rumah sakit |
| Resume ODC | `CAP-026` permukaan | Jenis kunjungan ODC di V2 belum dibuktikan — `RWI-DEC-123` | Alur Kasir V1 yang berlaku |
| Notifikasi aktif lapor dokter dan dugaan reaksi obat | `CAP-023-RSP` | Penerima belum diputuskan — gate `G-15`, `G-24` | Instruksi tampil di layar perawat; perawat menelepon dokter |
| Batas waktu verifikasi instruksi dan CPPT | `CAP-021`, `CAP-024` | Keputusan klinis `RWI-RULE-021` | Daftar Perlu Review tanpa label terlambat untuk instruksi |
| Template bersama tingkat SMF/rumah sakit | `CAP-023-RSP` | Di luar scope `RWI-DEC-122` | Template pribadi |
| Handover shift dan transfusi | Gate `INP-S19` | `DEFERRED` oleh `RWI-DEC-145` | Prosedur kertas yang berlaku |

### 22.9 Alur bisnis target

`FLOW-DOK-MVP-002`, diturunkan dari `flowcharts/00-alur-utama.md` bagian 4:

1. Kepala ruangan menetapkan DPJP, konsulen, atau dokter jaga.
2. Dokter membuka Dokter → Rawat Inap dan memilih pasien dari daftarnya.
3. Dokter menulis dan menyelesaikan kajian medis; registrasi keutuhan lahir sejak konsep.
4. Dokter mencatat visit dan menulis SOAP harian.
5. Dokter memutuskan obat bawaan yang dicatat perawat.
6. Dokter membuat resep; untuk insulin berdosis skala, memesan protokol dari versi sah.
7. Dokter memesan tindakan dan penunjang; perawat dapat memesan atas instruksi dokter yang bertugas.
8. Dokter menghentikan obat dari Resep Harian bila terapi berubah.
9. DPJP memverifikasi CPPT dan dokter pemberi instruksi memverifikasi pesanan perawat pada Perlu Review.
10. DPJP menulis dan menandatangani resume dari tab Resume Medis.
11. Episode ditutup oleh `episode-rawat-inap`; konsep terkunci dan pesanan tertunda batal.
12. Dokter melengkapi catatannya lewat Catatan Saya bila perlu.

### 22.10 Epic dan functional requirement

| ID | Epic | Prioritas | Disposisi backend |
| --- | --- | --- | --- |
| `EPIC DOK-10` | Ruang kerja satu halaman dan daftar pasien dari penugasan | `P0` | `EXTEND` — census diperluas; komponen Rawat Jalan diekstraksi |
| `EPIC DOK-11` | Kewenangan penulis, registrasi sejak konsep, penguncian, Catatan Saya | `P0` | `EXTEND` — mesin keutuhan dipakai ulang; `MISSING / NEW` untuk `my-authored` |
| `EPIC DOK-12` | Verifikasi CPPT hanya DPJP dan jenis catatan | `P0` | `EXTEND` |
| `EPIC DOK-13` | Resep Harian, penghentian butir, template, keputusan rekonsiliasi | `P0` | `EXTEND` dan `MISSING / NEW` |
| `EPIC DOK-14` | Protokol dan order sliding scale | `P0` | `MISSING / NEW` |
| `EPIC DOK-15` | Pesanan tindakan dan penunjang dengan instruksi | `P0` | `EXTEND` |
| `EPIC DOK-16` | Tab Resume Medis | `P1` | `EXTEND` — dirancang `episode-rawat-inap` |
| `EPIC DOK-17` | Penunjang enam layanan sebagai permukaan | `P1` | `EXISTING / REUSE` untuk Lab/Rad; nol backend untuk empat layanan |

| FR | Bunyi requirement yang dapat diuji | Epic | Disposisi |
| --- | --- | --- | --- |
| `FR-DOK-069` | Daftar Pasien Rawat Inap hanya memuat pasien dengan penugasan aktif dokter login. **Contoh:** dr. Ahmad DPJP Budi dan konsulen Sari di antara 122 pasien → dua kartu, Total Pasien 2 | `DOK-10` | `EXTEND` |
| `FR-DOK-070` | `doctorId` pada query diabaikan bila `assignedToMe=true`. **Contoh:** akun dr. Ahmad mengirim id dr. Rina → tetap dua pasien dr. Ahmad | `DOK-10` | `EXTEND` |
| `FR-DOK-071` | Metrik kepala dihitung dari daftar yang sama dan disembunyikan bila gagal dimuat, bukan ditampilkan nol | `DOK-10` | `EXTEND` |
| `FR-DOK-072` | Tata letak sama dengan Dokter Rawat Jalan menurut `UI-AC-DOK-001` s.d. `012`; tangkapan layar bertopeng pada tiga lebar layar identik kotaknya | `DOK-10` | Frontend |
| `FR-DOK-073` | Tidak ada aksi atau status antrean pada ruang kerja rawat inap | `DOK-10` | Frontend |
| `FR-DOK-074` | Konsep SOAP dan kajian medis rawat inap membentuk tepat satu registrasi `Draft` sejak disimpan pertama kali. **Contoh:** simpan otomatis dua kali pukul 06.31 → satu registrasi | `DOK-11` | `EXTEND` |
| `FR-DOK-075` | Hanya penulis yang menyunting dan menyelesaikan konsep rawat inap; DPJP aktif pun `403` | `DOK-11` | `REPAIR` |
| `FR-DOK-076` | Dokumen baru mensyaratkan penugasan pada waktu klinis dan aktif saat disimpan. **Contoh:** catatan 05.00 dikirim 08.10 tanpa penugasan → `403`; 08.40 dengan penugasan singkat → `201` | `DOK-11` | `REPAIR` |
| `FR-DOK-077` | Penulis menyelesaikan konsepnya setelah penugasan berakhir bila konsep dibuat saat penugasan aktif; waktu klinis 06.30 dan tanda tangan 08.00 tersimpan terpisah | `DOK-11` | `REPAIR` |
| `FR-DOK-078` | Saat episode ditutup, konsep rawat inap menjadi `LockedUnsigned` dalam transaksi penutupan | `DOK-11` | `MISSING / NEW` — dipicu `episode-rawat-inap` |
| `FR-DOK-079` | Catatan Saya menampilkan konsep dan catatan terkunci milik penulis di bawah episode rawat inap, dengan identitas minimum saja | `DOK-11` | `EXTEND` + `MISSING / NEW` (`my-authored`, menunggu Yoga Aji Pratama) |
| `FR-DOK-080` | Penulis menambah addendum pada catatan final atau terkunci miliknya tanpa penugasan aktif | `DOK-11` | `EXISTING / REUSE` |
| `FR-DOK-081` | Tindakan baru dari catatan dokter pada episode `Closed` ditolak `422` | `DOK-11` | `REPAIR` |
| `FR-DOK-082` | Verifikasi CPPT hanya oleh penugasan berperan DPJP aktif pada detik verifikasi; konsulen dan dokter jaga `403` | `DOK-12` | `REPAIR` |
| `FR-DOK-083` | Pada episode `Closed`, DPJP terakhir memverifikasi entri yang ditulis sebelum penutupan; keterlambatan tetap tercatat. **Contoh:** entri Sabtu 21.00 diverifikasi Senin 16.00 → tercatat terlambat 19 jam | `DOK-12` | `EXTEND` |
| `FR-DOK-084` | Daftar tunggu verifikasi memuat episode `Closed` milik DPJP terakhir sampai seluruh entrinya terverifikasi | `DOK-12` | `MISSING / NEW` |
| `FR-DOK-085` | CPPT menyimpan jenis catatan yang diperiksa terhadap profesi penulis; entri lama `Unspecified` tanpa pengisian tebakan | `DOK-12` | `EXTEND` |
| `FR-DOK-086` | Resep Harian menyaring periode hari ini, minggu ini, bulan ini, dan rentang, termasuk racikan dan obat pulang | `DOK-13` | `EXTEND` |
| `FR-DOK-087` | Penghentian butir beralasan oleh dokter bertugas membatalkan dosis MAR `Due` sesudahnya dalam satu transaksi. **Contoh:** Ceftriaxone dihentikan 09.10 → dosis 20.00 `Cancelled`, dosis 08.00 `Administered` utuh | `DOK-13` | `MISSING / NEW` |
| `FR-DOK-088` | Template dibuat, diubah, dan dihapus hanya atas nama dokter login — berlaku umum termasuk poliklinik | `DOK-13` | `REPAIR` |
| `FR-DOK-089` | Ruang kerja rawat inap hanya menampilkan dan memakai template milik dokter login; perawat tidak memakai template | `DOK-13` | `REPAIR` |
| `FR-DOK-090` | Pemakaian template menandai butir bentrok alergi atau obat tidak tersedia tanpa menggagalkan butir lain; draft bertanda ditolak saat disimpan | `DOK-13` | `MISSING / NEW` |
| `FR-DOK-091` | Template kosong ditolak | `DOK-13` | `REPAIR` |
| `FR-DOK-092` | Dokter memutuskan obat bawaan Lanjut Sama, Lanjut Ubah, atau Hentikan; "Lanjut" membentuk butir draft resep; perawat `403` | `DOK-13` | `MISSING / NEW` |
| `FR-DOK-093` | Keputusan dapat diganti selama butir hasilnya masih draft, dengan riwayat keputusan tersimpan; setelah resep aktif `409` | `DOK-13` | `MISSING / NEW` |
| `FR-DOK-094` | Versi protokol menyimpan rentang yang menutup seluruh nilai tanpa tumpuk. **Contoh:** 200–260 dan 250–299 ditolak; versi mulai 150 tanpa rentang di bawahnya ditolak | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-095` | Pengesah protokol bukan pengubah terakhir; versi sah sebelumnya pensiun pada transaksi yang sama | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-096` | Order hanya dari versi `Approved`; rentang tersalin; penyesuaian wajib beralasan. **Contoh:** dosis separuh "pasien sensitif insulin" → GDS 280 menghasilkan 3 unit, bukan 6 | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-097` | Versi template baru tidak mengubah order yang sudah berjalan | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-098` | Penyesuaian membuat versi order baru dan menolak nomor versi basi | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-099` | Penghentian order atau butir insulinnya menghentikan pelaksanaan berikutnya | `DOK-14` | `MISSING / NEW` |
| `FR-DOK-100` | Perawat membuat pesanan tindakan dengan dokter pemberi instruksi yang bertugas; tanpa `ConsultationId`; penginput dari akun login | `DOK-15` | `EXTEND` |
| `FR-DOK-101` | Pesanan tindakan poliklinik tanpa `ConsultationId` tetap ditolak dengan pesan lama | `DOK-15` | `EXISTING / REUSE` |
| `FR-DOK-102` | Pesanan hanya diubah penginput; dibatalkan penginput atau DPJP aktif dengan alasan | `DOK-15` | `REPAIR` |
| `FR-DOK-103` | Pelaksana menjadi penulis catatan pelaksanaan; addendum hanya oleh pelaksana | `DOK-15` | `EXISTING / REUSE` |
| `FR-DOK-104` | Hanya dokter pemberi instruksi yang memverifikasi; verifikasi tidak mengubah penginput dan isi | `DOK-15` | `MISSING / NEW` |
| `FR-DOK-105` | Penutupan episode membatalkan pesanan tertunda yang belum tertagih dengan alasan tetap; pesanan tertagih dibiarkan dan tampil di daftar pantau | `DOK-15` | `MISSING / NEW` — dipicu `episode-rawat-inap` |
| `FR-DOK-106` | Pesanan laboratorium dan radiologi perawat membawa pemberi instruksi dan status verifikasi | `DOK-15` | `EXTEND` — **tertahan persetujuan pemilik Lab/Rad** |
| `FR-DOK-107` | Tab Resume Medis membaca, menyimpan, dan menandatangani resume yang sama dengan Detail Episode, beserta tiga isian baru | `DOK-16` | `EXTEND` |
| `FR-DOK-108` | Isian usulan resume menampilkan label sumber dan tidak tersimpan tanpa tindakan dokter | `DOK-16` | `MISSING / NEW` |
| `FR-DOK-109` | Resume ODC tampil "Integrasi belum tersedia" tanpa form | `DOK-16` | Frontend |
| `FR-DOK-110` | Kartu Laboratorium dan Radiologi menampilkan jumlah pesanan, hasil final baru, dan detail hasil | `DOK-17` | `EXISTING / REUSE` |
| `FR-DOK-111` | Empat kartu lain menampilkan konteks pasien dan "Integrasi belum tersedia" tanpa permintaan jaringan ke modul itu | `DOK-17` | Frontend |

### 22.11 Model status yang diusulkan

| Objek | Status | Invariant utama |
| --- | --- | --- |
| Registrasi keutuhan SOAP dan kajian rawat inap | `Draft`, `Signed`, `LockedUnsigned`, `Cancelled` | `INV-DOK-18`; `LockedUnsigned` tidak pernah kembali |
| Verifikasi CPPT | `NotRequired`, `Pending`, `Verified`, `Overdue` | `INV-DOK-16` |
| Pesanan tindakan | `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled`; verifikasi instruksi `NotRequired`, `Pending`, `Verified` | `INV-DOK-17` |
| Butir resep | aktif, dihentikan | Tidak dapat dihidupkan kembali |
| Keputusan rekonsiliasi | `Pending`, `ContinueSame`, `ContinueModified`, `Stopped` | Riwayat keputusan tidak diubah |
| Versi protokol | `Draft`, `Approved`, `Retired` | Satu `Approved` per template |
| Order sliding scale | `Active`, `Stopped` | Satu order per butir insulin |

### 22.12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
| --- | --- | --- |
| Mesin keutuhan, addendum, `my-unsigned`, `MstPrescriptionTemplate`, `MstDrug`, visite, diagnosis | Census (`assignedToMe`), `TrxPatientProcedure`, `TrxPatientIntegratedProgressNote`, `PhmPrescriptionItem`, `LabOrder`, `RadOrder`, `CpptVerificationService`, `InpatientClinicalContextService`, `PrescriptionTemplateService` | Tujuh tabel `PharmacyManagement`: rekonsiliasi dua tabel, sliding scale lima tabel; `InpatientClinicalDocumentRegistrationService`, `PatientProcedureOrderService`, `InpatientPrescriptionService`; tiga controller Farmasi; `my-authored` di `MedicalRecordManagement` |

Nol tabel baru di `ClinicalManagement`, `InPatientManagement`, dan `MedicalRecordManagement` dari sub-modul ini.

### 22.13 Sasaran kemampuan API

Subset dari `contracts/api-contract.md` bagian 12; label status mengikuti kontrak.

| Grup `[Tags]` | Method | Path | Hak akses | Epic | Status |
| --- | --- | --- | --- | --- | --- |
| Health Services / Inpatient Management / Inpatient Census | `GET` | `/?assignedToMe=true` | `InpatientCensus : Read` | `DOK-10` | **Tersedia**, penyaring **Rencana** |
| Health Services / Clinical Management / Doctor Consultation | `PUT`, `PATCH` | `/{id}`, `/{id}/soap`, `/{id}/complete`, `/{id}/cancel` | `DoctorConsultation : Update` | `DOK-11` | **Tersedia**, perilaku **Rencana** |
| Health Services / Medical Record Management / Clinical Document Integrity | `GET` | `/my-authored` | `ClinicalDocumentIntegrity : Read` | `DOK-11` | **Rencana (belum tersedia)** |
| Health Services / Clinical Management / Patient Integrated Progress Note | `PATCH`, `GET` | `/{id}/verify`, `/verification-worklist` | `PatientIntegratedProgressNote : Verify`, `: Read` | `DOK-12` | Perilaku **Rencana**; worklist **Rencana (belum tersedia)** |
| Health Services / Pharmacy Management / Prescription | `PATCH` | `/items/{itemId}/stop` | `Prescription : Stop` | `DOK-13` | **Rencana (belum tersedia)** |
| Health Services / Pharmacy Management / Prescription Template | `POST` | `/{id}/apply` | `PrescriptionTemplate : Create` | `DOK-13` | **Tersedia**, perilaku **Rencana** |
| Health Services / Pharmacy Management / Medication Reconciliation | `POST` | `/{id}/decisions` | `MedicationReconciliation : Decide` | `DOK-13` | **Rencana (belum tersedia)** |
| Health Services / Pharmacy Management / Sliding Scale Template | `POST` | `/versions/{versionId}/approve` | `SlidingScaleTemplate : Approve` | `DOK-14` | **Rencana (belum tersedia)** |
| Health Services / Pharmacy Management / Sliding Scale Order | `POST` | `/`, `/{id}/versions` | `SlidingScaleOrder : Create`, `: Update` | `DOK-14` | **Rencana (belum tersedia)** |
| Health Services / Clinical Management / Patient Procedure | `POST`, `PATCH` | `/inpatient-orders`, `/{id}/verify-instruction` | `PatientProcedure : Create`, `: Verify` | `DOK-15` | **Rencana (belum tersedia)** |
| Health Services / Inpatient Management / Inpatient Discharge | `GET` | `/{episodeId}/summary-prefill` | `InpatientDischarge : Read` | `DOK-16` | **Rencana (belum tersedia)** |

### 22.14 Matriks kewenangan

| Aksi | String hak akses | DPJP | Konsulen / dokter jaga | Perawat |
| --- | --- | :---: | :---: | :---: |
| Menyelesaikan konsep sendiri | `[AccessPermission("DoctorConsultation", "Update")]` | ✔ | ✔ | — |
| Verifikasi CPPT | `[AccessPermission("PatientIntegratedProgressNote", "Verify")]` | ✔ aktif/terakhir | Hak ada, ditolak aturan bisnis | — |
| Menghentikan butir resep | `[AccessPermission("Prescription", "Stop")]` | ✔ | ✔ | — |
| Keputusan rekonsiliasi | `[AccessPermission("MedicationReconciliation", "Decide")]` | ✔ | ✔ | — |
| Mencatat obat bawaan | `[AccessPermission("MedicationReconciliation", "Create")]` | — | — | ✔ |
| Memesan sliding scale | `[AccessPermission("SlidingScaleOrder", "Create")]` | ✔ | ✔ | — |
| Mengesahkan protokol | `[AccessPermission("SlidingScaleTemplate", "Approve")]` | Bila diberi peran pengesah | Sama | — |
| Memesan tindakan atas instruksi | `[AccessPermission("PatientProcedure", "Create")]` | ✔ | ✔ | ✔ |
| Verifikasi instruksi | `[AccessPermission("PatientProcedure", "Verify")]` | Bila pemberi instruksi | Bila pemberi instruksi | — |
| Menandatangani resume | `[AccessPermission("InpatientDischarge", "Sign")]` | ✔ aktif | — | — |

### 22.15 Batas integrasi dan billing

| Sub-modul ini **MUST NOT** | Karena |
| --- | --- |
| Membuat tabel resep, template, rekonsiliasi, sliding scale di `ClinicalManagement` atau `InPatientManagement` | `RUL-DOK-04` |
| Membuat daftar catatan-milik-saya tandingan | `RWI-DEC-142` |
| Menghitung dosis dari hasil laboratorium | `RUL-DOK-03` |
| Mengintegrasikan Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-108`, `113` |
| Membatalkan pesanan yang sudah tertagih lewat penutupan | `RWI-DEC-143` (c); ditangani bersama Billing |
| Menandai obat diserahkan | `RUL-DOK-01` |

Pesanan dan pelaksanaan tetap bukan tagihan — `BR-RWI-013`.

### 22.16 Guardrail regulasi

| Kewajiban | Bentuknya pada MVP |
| --- | --- |
| Rekam medis tidak dihapus dan tidak ditimpa | Konsep terkunci "Tidak Ditandatangani" tetap bagian rekam medis; koreksi lewat addendum |
| Tanda tangan milik penulis | `INV-DOK-14` |
| Verifikasi CPPT oleh DPJP | `INV-DOK-16`; batas waktu menunggu pemilik klinis |
| Isi protokol klinis disahkan pemilik klinis | Gerbang produksi `RWI-DEC-146` — pemilik belum ditunjuk |
| Daftar obat high-alert termasuk insulin disahkan | Gerbang produksi `RWI-DEC-116` |
| Privasi Catatan Saya | Identitas minimum `RWI-DEC-127` butir (2); pemilik keamanan/privasi belum ditunjuk tetap gerbang produksi |

### 22.17 Kebutuhan non-fungsional

| ID | Kebutuhan | Contoh bukti |
| --- | --- | --- |
| `NFR-012` | Registrasi, penguncian, pembatalan pesanan, dan penghentian butir berjalan dalam satu transaksi dengan perintah pemicunya | Kegagalan buatan di tengah → nol perubahan parsial |
| `NFR-013` | Kiriman ulang dengan kunci sama tidak menggandakan pesanan, obat bawaan, order sliding scale, maupun registrasi | Dua permintaan identik → satu baris |
| `NFR-014` | Benturan penyesuaian protokol ditolak dengan nomor versi | Dua dokter bersamaan → satu `201`, satu `409` |
| `NFR-015` | Seluruh penjaga per pasien diuji dengan peran nyata non-SuperAdmin | Laporan task mencatat peran uji |
| `NFR-016` | Waktu klinis dan waktu tanda tangan tersimpan terpisah dalam UTC dan ditampilkan zona `Asia/Jakarta` | SOAP 06.30 ditandatangani 08.00 tampil keduanya |
| `NFR-017` | Kolom sensitif baru tidak masuk custom logger | Pemeriksaan payload log |

### 22.18 Skenario UAT

| ID | Epic | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- | --- |
| `UAT-DOK-35` | `DOK-10` | Berhasil | dr. Ahmad DPJP Budi, konsulen Sari | Membuka Dokter → Rawat Inap | Dua kartu; kepala dan tab identik tata letaknya dengan Rawat Jalan |
| `UAT-DOK-36` | `DOK-10` | Gagal | Akun perawat membuka route ruang kerja dokter | Membuka halaman | "Akun Anda belum terhubung ke data dokter"; nol daftar |
| `UAT-DOK-37` | `DOK-11` | Berhasil | Konsep SOAP dr. Yoga 06.30, penugasan berakhir 07.00 | 08.00 menyelesaikan dari Catatan Saya | Diterima; waktu klinis dan tanda tangan berbeda tampil |
| `UAT-DOK-38` | `DOK-11` | Gagal | Konsep dr. Yoga | dr. Rina menekan Selesai | Ditolak; konsep tetap milik dr. Yoga |
| `UAT-DOK-39` | `DOK-11` | Gagal | Episode Joko ditutup dengan konsep terbuka | dr. Yoga mencoba menyunting | Ditolak dengan arahan addendum; addendum dari Catatan Saya diterima |
| `UAT-DOK-40` | `DOK-12` | Berhasil | Episode Budi ditutup; dua entri tertinggal | DPJP terakhir memverifikasi dari Perlu Review | Keduanya terverifikasi; tetap tercatat terlambat |
| `UAT-DOK-41` | `DOK-12` | Gagal | Konsulen dengan penugasan aktif | Memverifikasi CPPT | Ditolak "hanya DPJP yang sedang bertugas" |
| `UAT-DOK-42` | `DOK-13` | Berhasil | Budi membawa Amlodipin, alergi Paracetamol | Dokter Lanjut Sama; memakai template berisi Paracetamol; menghentikan Ceftriaxone | Draft terisi; Paracetamol bertanda; dosis Ceftriaxone berikutnya batal |
| `UAT-DOK-43` | `DOK-13` | Gagal | Template "ISPA anak" Bersama milik dr. Ahmad | dr. Rina memakai dari rawat inap lewat alamat langsung | Ditolak; tidak ada butir masuk draft |
| `UAT-DOK-44` | `DOK-14` | Berhasil | Versi v2 disahkan pengguna kedua | dr. Rina memesan dengan dosis separuh beralasan | Order `Active`; perawat melihat 3 unit untuk GDS 280 |
| `UAT-DOK-45` | `DOK-14` | Gagal | Pengubah terakhir v3 | Mengesahkan v3 sendiri | Ditolak; v2 tetap berlaku |
| `UAT-DOK-46` | `DOK-15` | Berhasil | 23.00 instruksi telepon dr. Yoga | Ns. Siti memesan cek GDS; pagi dr. Yoga memverifikasi | Pesanan berjalan malam itu; terverifikasi pagi; penginput tetap Siti |
| `UAT-DOK-47` | `DOK-15` | Gagal | Pesanan kateter dr. Rina | Ns. Siti mengubah jumlah sebelum memasang | Ditolak; hanya penginput |
| `UAT-DOK-48` | `DOK-15` | Gagal | Episode ditutup dengan pesanan tertunda tertagih | Menutup episode | Penutupan berhasil; pesanan tertagih tidak dibatalkan dan tampil di daftar pantau |
| `UAT-DOK-49` | `DOK-16` | Berhasil | DPJP Budi | Mengisi Kondisi Saat Pulang dari tab Resume Medis lalu menandatangani | Detail Episode menampilkan resume yang sama; episode belum tertutup |
| `UAT-DOK-50` | `DOK-16` | Gagal | Konsulen Budi | Menandatangani resume | Ditolak |
| `UAT-DOK-51` | `DOK-17` | Berhasil | Hasil Darah Lengkap `FINAL` | Membuka kartu Laboratorium | Nilai Hb dan leukosit bertanda di atas/di bawah rujukan |
| `UAT-DOK-52` | `DOK-17` | Gagal | — | Membuka kartu Hemodialisa | "Integrasi belum tersedia"; tidak ada data contoh dan tidak ada tombol yang tampak menyimpan |

### 22.19 Definition of Done penyelarasan

| Butir | Bukti |
| --- | --- |
| Satu pasien berjalan dari daftar dokter sampai resume bertanda tangan pada satu halaman | `UAT-DOK-35`, `UAT-DOK-49` |
| Tata letak lolos `UI-AC-DOK-001` s.d. `012` | Tangkapan layar bertopeng tiga lebar layar pada laporan task frontend |
| Tidak ada konsep rawat inap yang diselesaikan selain penulis | `RWI-AC-186` lulus pada SOAP, kajian medis, dan CPPT |
| Tidak ada konsep terbuka setelah penutupan | `RWI-AC-199`, `RWI-AC-201` |
| Verifikasi CPPT hanya DPJP aktif atau terakhir | `RWI-AC-190`, `UAT-DOK-40`, `UAT-DOK-41` |
| Rekonsiliasi, Resep Harian, template, sliding scale berjalan pada rilis yang sama dengan MAR | `RWI-AC-192` s.d. `195`, `RWI-AC-222` s.d. `226`, `RWI-AC-193` |
| Pesanan perawat terverifikasi pemberi instruksi | `UAT-DOK-46`; untuk Lab/Rad: persetujuan pemilik tercatat |
| Pesanan tertunda batal saat penutupan tanpa menahannya | `RWI-AC-213`, `RWI-AC-214`, `UAT-DOK-48` |
| Perilaku poliklinik tidak berubah kecuali template | Test regresi `RWI-AC-143`, `RWI-AC-218`; pemberitahuan kepada pemilik `rawat-jalan` tercatat |
| Nol tabel tandingan | `RWI-AC-211`, `RWI-AC-225`, pencarian `RWI-AC-192` |
| Seluruh master dan konfigurasi awal terisi pada lingkungan uji | `02-backend-architecture.md` bagian 11.9 |

### 22.20 Urutan pengiriman dan pertanyaan terbuka

Nama gelombang memakai awalan `DOK-V2-` supaya tidak bertabrakan dengan `DOK-MVP-*` dan `Gelombang 1A` — temuan `RLN-13`.

| Gelombang | Isinya | Syarat mulai |
| --- | --- | --- |
| `DOK-V2-0` | Perbaikan penjaga tanpa bentuk data: `FR-DOK-075`, `076`, `081`, `082` beserta regresi | Blueprint revision `7` disetujui |
| `DOK-V2-1` | `EPIC DOK-11` registrasi dan penguncian; `EPIC DOK-12` jenis catatan dan daftar verifikasi; kolom `TrxPatientProcedure` | `DOK-V2-0`; `episode-rawat-inap` `0.9.0` penutupan dikerjakan bersama |
| `DOK-V2-2` | `EPIC DOK-13` dan `EPIC DOK-14` | `DOK-V2-1`; **dirilis bersama MAR `KEP-V2-2`** |
| `DOK-V2-3` | `EPIC DOK-15`; bagian Lab/Rad setelah persetujuan pemiliknya | `DOK-V2-1` |
| `DOK-V2-4` | `EPIC DOK-10`, `EPIC DOK-16`, `EPIC DOK-17` — ruang kerja frontend | Persetujuan ekstraksi komponen `rawat-jalan`; `DOK-V2-1` |
| `POST-MVP` | Seluruh baris 22.8 | Keputusan pemilik masing-masing |

**Nol epic `OPEN DECISION` di dalam gelombang.** `FR-DOK-068` tetap di luar gelombang sebagaimana bagian 21.

| No | Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| ---: | --- | --- | --- | :---: |
| 7 | Jadwal pemeriksaan GDS pada order sliding scale dan status dosis bila hasil jatuh pada rentang 0 unit (usulan gate `G-22`: dosis terjadwal, `Held` beralasan) | Muhammad Hamzah | Kolom `CheckFrequencyCode` dan pembentukan dosis `Due` dirancang sebagai usulan; bila ditolak, hanya pembentukan dosis yang berubah | Tidak — dikonfirmasi saat approval |
| 8 | Satuan GDS rumah sakit (usulan gate `G-25`: disimpan eksplisit, pencocokan menolak satuan berbeda) | Muhammad Hamzah / pemilik klinis | Satuan wajib dipilih per versi | Tidak |
| 9 | Persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` atas kolom instruksi | Pemilik kedua modul | `FR-DOK-106` tertahan implementasinya | Tidak untuk desain; ya untuk task Lab/Rad |
| 10 | ~~Persetujuan Yoga Aji Pratama atas `my-authored` dan `serviceContext`~~ — **`closed` 2026-09-16** oleh `RWI-DEC-151` | Yoga Aji Pratama | ~~Tab Terkunci Catatan Saya tertahan~~ — `BE-RWI-092` dan `FE-RWI-077` bebas | Tidak |
| 11 | Persetujuan pemilik `rawat-jalan` atas ekstraksi komponen tata letak | Pemilik blueprint `rawat-jalan` | `DOK-V2-4` tertahan | Tidak untuk desain; ya untuk `DOK-V2-4` |
| 12 | Apakah integrasi Gizi dan Bank Darah dibuka sekarang karena modulnya sudah ada di source | Muhammad Hamzah lewat `grill-me` | Tetap "Integrasi belum tersedia" | Tidak |
| 13 | Penunjukan pemilik klinis pengesah isi protokol sliding scale | Manajemen rumah sakit | Sliding scale tidak dipakai pasien sungguhan | Gerbang produksi |
