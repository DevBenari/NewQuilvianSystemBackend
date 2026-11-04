# Optik — Arsitektur Domain

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Architecture ID | `OPT-HDA-001` |
| Revision | `1` |
| Status | `draft` — belum disetujui pemilik |
| Scope | Slice `OPT-S1`..`OPT-S10`. **`OPT-S11` Kacamata BPJS di luar scope** (`OPT-DEC-030`) |
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN` untuk S1–S10 — [02-requirement-completeness-assessment.md](02-requirement-completeness-assessment.md) revision 2 (`OPT-RCG-001`) |
| Keputusan yang dipakai | `OPT-DEC-001`..`OPT-DEC-046` (kecuali `OPT-DEC-014`, `superseded`) — [00-interview-decisions.md](00-interview-decisions.md) revision 3 |
| Bukti keadaan saat ini | [01-existing-capability-map.md](01-existing-capability-map.md) revision 1 (`OPT-CAP-01`..`31`) |
| Source SHA | Backend `2a5094ca`, frontend `b6dd6c304` — **sama** dengan capability map, sehingga tidak perlu impact scan |
| Baseline rujukan | Tidak dipakai (`indonesia-hospital-domain-reference` tanpa cakupan optik) |
| Kesiapan arsitektur | **`DOMAIN_ARCHITECTURE_READY`** untuk S1–S10 (bagian N) |

> **Cara membaca dokumen ini.** Dokumen ini menjelaskan *konsep bisnis* Optik, siapa pemiliknya,
> bagaimana konsep itu berhubungan dengan modul lain, dan bagaimana statusnya berubah. Dokumen
> ini **bukan** rancangan tabel, endpoint, atau layar. Satu konsep di sini belum tentu menjadi
> satu tabel; keputusan itu dibuat pada tahap `/design-business-module`.

---

## A. Ringkasan untuk pembaca umum

Optik punya empat "benda bisnis" utama yang masing-masing menjaga aturannya sendiri:

| No | Benda bisnis | Menjaga aturan apa | Contoh |
| ---: | --- | --- | --- |
| 1 | **Pemeriksaan Refraksi** | Hanya RO/Sp.M ber-SIP berlaku yang memeriksa; saat final, pemeriksaan ditagih sekali | Rina (RO) memeriksa Ibu Sari pukul 09.10; tarif pemeriksaan Rp 75.000 masuk tagihan saat disimpan final |
| 2 | **Resep Optik** (kacamata atau lensa kontak) | Ukuran sah dan bernotasi minus; revisi berversi; terkunci begitu lensa dikerjakan; berlaku 12 bulan | Resep v1 OD −1,50 direvisi menjadi v2; setelah pesanan dikirim ke bengkel, revisi ditolak |
| 3 | **Pesanan Optik** | Alur status, syarat uang muka, potong stok sekali, batal dengan persetujuan, dan masa garansi | ORD-0001 dikirim ke bengkel setelah uang muka Rp 525.000; stok frame berkurang 1 |
| 4 | **Keluhan Garansi** | Keluhan diperiksa ulang lalu diputuskan Kepala Optik; remake menjadi pesanan turunan Rp 0 | Keluhan pusing 10 hari sesudah diserahkan → remake ORD-0001-R1 |

Semua yang **bukan** milik Optik dipakai lewat pemiliknya: pasien (Patient Management),
kunjungan (Registrasi), STR/SIP (HR), barang dan tarif (Master Data), stok (Farmasi), tagihan,
pembayaran, dan diskon (Billing), jasa medis (Medical Fee), serta tampilan riwayat (Rekam Medis).
Optik tidak menyimpan salinan tandingan atas data-data itu.

---

## B. Ubiquitous language

| Istilah | Arti di Optik | Bukan |
| --- | --- | --- |
| **Kunjungan Optik** | Kunjungan resmi pasien pada unit layanan Optik, dibuat Registrasi. Satu kunjungan pemesanan tetap terbuka sampai pesanan terakhirnya selesai (`OPT-DEC-043`) | Bukan "kunjungan poli" yang ditutup harian |
| **Pemeriksaan refraksi** | Pengukuran tajam penglihatan dan ukuran koreksi oleh RO/Sp.M dalam satu kunjungan Optik | Bukan konsultasi dokter Poli Mata |
| **Resep Optik** | Ukuran koreksi yang sah untuk dibuatkan kacamata atau lensa kontak. Asalnya internal (RO/Sp.M RS) atau luar (disalin RO/Sp.M) | Bukan resep obat Farmasi (`PhmPrescription`) |
| **Versi resep** | Satu isi resep pada satu waktu. Revisi menambah versi; versi lama tidak pernah dihapus | — |
| **Resep terkunci** | Resep yang versinya sudah dipakai pesanan berstatus `Dalam Pengerjaan` atau sesudahnya | Bukan resep kedaluwarsa |
| **Resep berlaku** | Resep yang tanggal terbitnya belum lewat batas usia (bawaan 12 bulan, `OPT-DEC-040`) | Berlaku ≠ tidak terkunci; resep terkunci tetap bisa berlaku untuk pesanan baru |
| **Notasi minus** | Cara menulis silinder dengan nilai negatif. Resep bernotasi plus diubah (*transposisi*) saat disalin | — |
| **Pesanan berpengerjaan** | Pesanan kacamata/lensa yang harus dipotong dan dipasang di bengkel sendiri atau lab luar | — |
| **Penjualan langsung** | Pesanan barang jadi tanpa pengerjaan dan tanpa uang muka (cairan, kacamata baca jadi, aksesoris, lensa kontak berresep) | — |
| **Uang muka** | Pembayaran sebagian atas tagihan pesanan (`OPT-DEC-028`) | Bukan deposit Billing (`BilDepositPaymentOrder`) |
| **Bagian pasien** | Nilai tagihan yang dibayar pasien sesudah tanggungan penjamin; dasar hitung minimum uang muka (`OPT-DEC-044`) | — |
| **Lensa custom** | Lensa yang dipesan khusus ke lab luar dan tidak bisa dijual ke pasien lain | — |
| **Lensa stok** | Lensa dari stok depo Optik; bisa kembali ke stok bila belum dipotong (`OPT-DEC-038`) | — |
| **Remake** | Pembuatan ulang kacamata tanpa biaya karena keluhan dalam masa garansi yang disetujui | Bukan pesanan baru berbayar |
| **Depo Optik** | Satu lokasi penyimpanan Farmasi untuk unit Optik | Bukan gudang milik Optik |

---

## C. Peta bounded context

| ID | Context | Tanggung jawab | Konsep yang dimiliki | Hubungan |
| --- | --- | --- | --- | --- |
| `BC-OPT-01` | **Optik — Klinis** | Pemeriksaan refraksi dan resep | `CON-OPT-01`..`04` | Upstream bagi `BC-OPT-02`, Rekam Medis, Billing (tarif pemeriksaan), Medical Fee |
| `BC-OPT-02` | **Optik — Pesanan & Layanan Purna** | Pesanan, pengerjaan, penyerahan, keluhan garansi, remake | `CON-OPT-05`..`10` | Downstream `BC-OPT-01`; upstream bagi Farmasi (stok), Billing (tagihan barang), Registrasi (penutupan kunjungan) |
| `BC-OPT-03` | **Optik — Profil Barang & Pengaturan** | Atribut terstruktur barang Optik dan angka bawaan | `CON-OPT-11`, `12` | Menempel pada barang milik Master Data |
| `BC-PAT` | Patient Management | Pasien induk | Pasien | Upstream (Existing) |
| `BC-REG` | Registrasi | Kunjungan dan unit layanan | Kunjungan, unit layanan | Upstream; **Extend**: jenis unit Optik, kunjungan terbuka lintas hari ditutup atas permintaan Optik |
| `BC-HR` | Human Resource | Pegawai, STR/SIP | `WfpCredentialLicense`, profesi | Upstream (Existing, dibaca lewat adapter) |
| `BC-MD` | Master Data | Barang, kategori, tarif, supplier | `MstDrug`, kategori, `MstTariff`, `MstSupplier` | Upstream; **Extend**: kategori barang Optik |
| `BC-PHM` | Farmasi | Depo dan kartu stok | Lokasi, saldo, mutasi | Downstream penerima perintah stok; **Extend**: jenis dokumen sumber Optik |
| `BC-BIL` | Billing | Tagihan, pembayaran, tanggungan, diskon, piutang | Tagihan, pembayaran, kebijakan diskon | Downstream penerima fakta tagihan; **Extend**: sumber Optik, alokasi per baris |
| `BC-MDF` | Medical Fee | Jasa medis | — | Downstream penerima jasa pemeriksaan (**Extend**) |
| `BC-MRC` | Rekam Medis | Tampilan riwayat klinis | — | Downstream lewat pembaca (**Adapter/View**) |
| `BC-OUT` | Rawat Jalan (Poli Mata) | Kunjungan Poli Mata | — | Upstream rujukan (Existing; tautan kunjungan asal) |

```text
 Patient ─┐        HR (STR/SIP) ──┐
          ▼                       ▼
 Registrasi ──► [BC-OPT-01 Klinis] ──event──► Rekam Medis (baca)
   ▲   │             │  │                └──► Billing (tarif periksa) / Medical Fee
   │   │             ▼  │
   │   └──────► [BC-OPT-02 Pesanan] ──perintah stok──► Farmasi (depo Optik)
   │                 │   └──fakta tagihan barang──► Billing ──status bayar──┐
   └──tutup kunjungan┘                                                    │
                     ▲────────────────────────────────────────────────────┘
 Master Data (barang, tarif) ◄── [BC-OPT-03 Profil barang & pengaturan]
```

---

## D. Katalog konsep domain

| ID | Nama | Klasifikasi | Pemilik | Ownership | Identitas | Peran lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `CON-OPT-01` | Pemeriksaan Refraksi | `AGGREGATE_ROOT` | `BC-OPT-01` | New | Nomor pemeriksaan + kunjungan | `Draft` → `Final` | Pemeriksa RO/Sp.M ber-SIP berlaku; final sekali menerbitkan satu tagihan | `OPT-DEC-005`, `012`, `042` |
| `CON-OPT-02` | Hasil Ukur | `VALUE_OBJECT` | `BC-OPT-01` | New | — | Bagian pemeriksaan | Visus jauh/dekat per mata, autorefraktometer, koreksi subjektif, PD (rincian `n1`, `PROPOSED`) | `OPT-DEC-001`, RCG `n1` |
| `CON-OPT-03` | Resep Optik | `AGGREGATE_ROOT` | `BC-OPT-01` | New | Nomor resep | Aktif ↔ terkunci (turunan) | Jenis kacamata/lensa kontak; asal internal/luar; satu versi terakhir; terkunci bila dipakai pesanan berpengerjaan | `OPT-DEC-007`, `011`, `017`, `022`, `023`, `045`, `046` |
| `CON-OPT-04` | Versi Resep + Ukuran Lensa per Mata | `ENTITY` + `VALUE_OBJECT` | `BC-OPT-01` | New | Nomor versi dalam resep | Tidak berubah setelah dibuat | Rentang dan kelipatan `OPT-DEC-045`; silinder minus; nilai asli resep plus tersimpan; lewat rentang hanya Sp.M dengan alasan | `OPT-DEC-045` |
| `CON-OPT-05` | Pesanan Optik | `AGGREGATE_ROOT` | `BC-OPT-02` | New | Nomor pesanan | Bagian G.2 | Seluruh aturan status, uang muka, stok, dan garansi | `OPT-DEC-016`, `020`, `028`, `034`, `035`, `044` |
| `CON-OPT-06` | Item Pesanan | `ENTITY` | `BC-OPT-02` | New | Nomor baris dalam pesanan | Aktif → dibatalkan | Barang wajib kategori Optik dan bertarif berlaku; lensa kontak wajib resep lensa kontak berlaku; tanda lensa custom/stok dan "sudah dipotong" | `OPT-DEC-019`, `021`, `031`, `038`, `046` |
| `CON-OPT-07` | Tujuan Pengerjaan | `VALUE_OBJECT` | `BC-OPT-02` | New (merujuk `MstSupplier`) | — | Diisi saat kirim | Bengkel sendiri atau supplier lab lensa, ditambah perkiraan jadi | `OPT-DEC-013` |
| `CON-OPT-08` | Pengajuan Batal | `ENTITY` | `BC-OPT-02` | New | Dalam pesanan | `Diajukan` → `Disetujui`/`Ditolak` | Wajib alasan; sesudah pengerjaan wajib persetujuan Kepala Optik dan tanda "lensa sudah dipotong" | `OPT-DEC-010`, `038` |
| `CON-OPT-09` | Keluhan Garansi | `AGGREGATE_ROOT` | `BC-OPT-02` | New | Nomor keluhan | Bagian G.3 | Tertaut ke pesanan asal dan kunjungan keluhan baru; jenis keluhan menentukan masa garansi | `OPT-DEC-015`, `041`, `043` |
| `CON-OPT-10` | Remake | (peran `CON-OPT-05`) | `BC-OPT-02` | New | Pesanan turunan bertanda remake | Seperti pesanan | Rp 0; tertaut ke pesanan asal dan keluhan; memotong stok | `OPT-DEC-015` |
| `CON-OPT-11` | Profil Barang Optik | `REFERENCE_DATA` | `BC-OPT-03` | New, menempel pada barang Master Data (**Extend** kategori) | Satu profil per barang | Aktif/nonaktif mengikuti barang | Atribut per kategori (`n2`, `CONFIGURABLE_DEFAULT`); tidak menggandakan nama/harga/stok barang | `OPT-DEC-031` |
| `CON-OPT-12` | Pengaturan Optik | `REFERENCE_DATA` | `BC-OPT-03` | New | Satu per unit Optik | Berlaku ke depan | Persentase uang muka, batas diskon, hari pengingat/tutup, usia resep, masa garansi, rentang BC/DIA; nilai dibekukan ke pesanan/resep saat dibuat | `OPT-DEC-009`, `040`, `041`, `045` |
| `CON-EXT-01` | Pasien | `EXTERNAL_CONTRACT` | `BC-PAT` | Existing | — | — | Wajib terdaftar lebih dulu | `OPT-CAP-01` |
| `CON-EXT-02` | Kunjungan Optik | `EXTERNAL_CONTRACT` | `BC-REG` | Existing + Extend | Nomor kunjungan | Terbuka sampai Optik meminta tutup | Unit layanan berjenis Optik; tautan opsional ke kunjungan Poli Mata asal | `OPT-DEC-006`, `032`, `037`, `043`, `OPT-CAP-02` |
| `CON-EXT-03` | Izin praktik (STR/SIP) | `EXTERNAL_CONTRACT` | `BC-HR` | Existing (Adapter) | — | — | Dibandingkan per tanggal kalender WIB | `OPT-DEC-012`, `OPT-CAP-06` |
| `CON-EXT-04` | Barang + tarif | `EXTERNAL_CONTRACT` | `BC-MD` | Existing + Extend (kategori) | Kode barang, kode tarif | — | Harga selalu dari tarif berlaku | `OPT-DEC-021`, `OPT-CAP-14`, `19` |
| `CON-EXT-05` | Kartu stok depo Optik | `EXTERNAL_CONTRACT` | `BC-PHM` | Existing + Extend (jenis dokumen) | — | — | Hanya `DrugStockService` yang menulis saldo; kunci idempoten per tindakan | `OPT-CAP-15`..`17` |
| `CON-EXT-06` | Tagihan + pembayaran + diskon | `EXTERNAL_CONTRACT` | `BC-BIL` | Extend | — | — | Satu fakta tagihan per item pesanan dan per pemeriksaan final | `OPT-DEC-027`..`029`, `042` |

---

## E. Model aggregate

### `AGG-OPT-01` Pemeriksaan Refraksi

| Butir | Isi |
| --- | --- |
| Root | `CON-OPT-01` |
| Batas | Satu pemeriksaan dalam satu kunjungan Optik, beserta hasil ukurnya (`CON-OPT-02`) |
| Invariant | (1) Pemeriksa adalah RO/Sp.M yang STR/SIP-nya berlaku pada tanggal pemeriksaan (WIB). (2) Hanya satu kali menjadi `Final`. (3) Kunjungannya adalah kunjungan Optik yang belum ditutup |
| Tindakan | Mulai, isi hasil ukur, simpan final |
| Event | `EVT-OPT-01 PemeriksaanFinal` → Billing (fakta tarif pemeriksaan, idempoten per pemeriksaan), Medical Fee (jasa pemeriksa), Rekam Medis (dapat dibaca) |
| Catatan | Koreksi hasil ukur sesudah final belum diputuskan (`GAP-OPT-01`) |

### `AGG-OPT-02` Resep Optik

| Butir | Isi |
| --- | --- |
| Root | `CON-OPT-03` |
| Batas | Satu resep beserta seluruh versinya (`CON-OPT-04`). Tertaut opsional ke `AGG-OPT-01` (resep internal hasil pemeriksaan) |
| Invariant | (1) Pembuat dan perevisi adalah RO/Sp.M dengan izin berlaku. (2) Isi mengikuti rentang `OPT-DEC-045`; lewat rentang hanya oleh Sp.M dengan alasan. (3) Silinder disimpan minus; nilai asli resep plus tersimpan apa adanya. (4) Revisi ditolak bila resep terkunci. (5) Resep luar menyimpan penulis asli, asal fasilitas, dan tanggal resep. (6) Jenis lensa kontak wajib memuat BC, DIA, power per mata, dan merk/jenis |
| Tindakan | Buat (internal/luar), revisi (menambah versi), kunci (dipicu pesanan) |
| Event | `EVT-OPT-02 VersiResepDibuat` → Rekam Medis selalu membaca versi terakhir |
| Mengapa terpisah dari pemeriksaan | Resep luar tidak punya pemeriksaan, satu resep dipakai banyak pesanan (`OPT-DEC-023`), dan lifecycle kunci/versinya berbeda dari pemeriksaan |

### `AGG-OPT-03` Pesanan Optik

| Butir | Isi |
| --- | --- |
| Root | `CON-OPT-05` |
| Batas | Pesanan, item (`CON-OPT-06`), tujuan pengerjaan (`CON-OPT-07`), pengajuan batal (`CON-OPT-08`), dan riwayat status |
| Invariant | (1) Pesanan berukuran merujuk **satu versi resep tertentu** yang berlaku saat dibuat; versi itu yang dikirim ke bengkel/lab. (2) Masuk `Dalam Pengerjaan` hanya bila terbayar ≥ minimum (persentase dibekukan saat pesanan dibuat) dari bagian pasien. (3) Aturan sementara `OPT-DEC-035`: satu kunjungan hanya punya satu pesanan berpengerjaan yang belum lunas, sampai alokasi per baris tersedia. (4) Stok frame/lensa stok dipotong tepat sekali saat `Dalam Pengerjaan`; penjualan langsung dipotong saat `Diserahkan`. (5) `Diserahkan` hanya bila lunas. (6) Diskon tidak disimpan di pesanan (milik Billing). (7) Remake tertaut ke pesanan asal dan keluhan, bernilai Rp 0, dan tidak menerbitkan fakta tagihan |
| Tindakan | Buat, simpan & tagihkan, kirim ke pengerjaan, tandai siap, serahkan (dengan *fitting*), ajukan batal, setujui/tolak batal, tutup tidak diambil |
| Event | `EVT-OPT-03 PesananDitagihkan` (fakta tagihan per item) · `EVT-OPT-04 PesananDikirimKePengerjaan` (perintah potong stok, kunci resep) · `EVT-OPT-05 PesananDibatalkan` (batal baris tagihan frame dan lensa belum dipotong, kembalikan stok) · `EVT-OPT-06 PesananDiserahkan` (mulai garansi, potong stok penjualan langsung, cek penutupan kunjungan) · `EVT-OPT-07 PesananTidakDiambil` (cek penutupan kunjungan) |

### `AGG-OPT-04` Keluhan Garansi

| Butir | Isi |
| --- | --- |
| Root | `CON-OPT-09` |
| Batas | Satu keluhan atas satu pesanan yang sudah `Diserahkan` |
| Invariant | (1) Dibuat pada kunjungan Optik baru (`OPT-DEC-043`). (2) Status garansi dihitung dari tanggal `Diserahkan` dan masa garansi menurut jenis keluhan yang berlaku saat pesanan diserahkan. (3) Pemeriksaan ulang oleh RO/Sp.M. (4) Keputusan oleh Kepala Optik. (5) Satu keluhan menghasilkan paling banyak satu remake |
| Tindakan | Catat keluhan, periksa ulang, putuskan (setujui remake / tolak / di luar garansi) |
| Event | `EVT-OPT-08 RemakeDisetujui` → membuat pesanan remake (`CON-OPT-10`) |

---

## F. Model relasi

| Dari | Ke | Makna | Kardinalitas | Wajib? | Arah ownership | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| Kunjungan Optik | Pasien | Kunjungan milik satu pasien | N : 1 | Wajib | Registrasi → Patient | `OPT-CAP-01` |
| Kunjungan Optik | Kunjungan Poli Mata | Asal rujukan | N : 0..1 | Opsional | Registrasi | `OPT-DEC-037` |
| Pemeriksaan Refraksi | Kunjungan Optik | Diperiksa dalam kunjungan | N : 1 | Wajib | Optik merujuk Registrasi | `OPT-DEC-006` |
| Pemeriksaan Refraksi | Pemeriksa (pegawai) | Pemeriksa bertanggung jawab | N : 1 | Wajib | Optik merujuk HR | `OPT-DEC-005` |
| Resep | Pemeriksaan Refraksi | Resep internal lahir dari pemeriksaan | 0..N : 0..1 | Opsional (resep luar tanpa pemeriksaan) | Optik | `OPT-DEC-017` |
| Resep | Pasien | Resep milik pasien | N : 1 | Wajib | Optik merujuk Patient | — |
| Resep | Kunjungan Optik | Kunjungan saat resep dibuat/disalin | N : 1 | Wajib | Optik merujuk Registrasi | `OPT-DEC-039` |
| Versi Resep | Resep | Isi resep pada satu waktu | N : 1 | Wajib, ≥ 1 versi | Optik | `OPT-DEC-007` |
| Pesanan | Kunjungan Optik | Pesanan dibuat dan ditagih di kunjungan itu | N : 1 | Wajib | Optik merujuk Registrasi | `OPT-DEC-043` |
| Pesanan | Versi Resep | Ukuran yang dikerjakan | N : 0..1 | Wajib untuk pesanan berukuran; kosong untuk penjualan langsung non-lensa-kontak | Optik | `OPT-DEC-016`, `023` |
| Item Pesanan | Barang | Barang yang dijual | N : 1 | Wajib | Optik merujuk Master Data | `OPT-DEC-021`, `031` |
| Item Pesanan | Baris tagihan | Fakta tagihan item | 1 : 0..1 | Wajib kecuali remake | Billing memiliki baris; Optik memiliki fakta | `OPT-DEC-029` |
| Pesanan | Tujuan pengerjaan (supplier) | Lab luar | N : 0..1 | Opsional (bengkel sendiri) | Optik merujuk Master Data | `OPT-DEC-013` |
| Pesanan remake | Pesanan asal | Remake atas pesanan | 0..1 : 1 | Wajib untuk remake | Optik | `OPT-DEC-015` |
| Keluhan Garansi | Pesanan asal | Keluhan atas pesanan | N : 1 | Wajib | Optik | `OPT-DEC-015` |
| Keluhan Garansi | Kunjungan Optik (keluhan) | Kunjungan baru saat keluhan | 1 : 1 | Wajib | Optik merujuk Registrasi | `OPT-DEC-043` |
| Profil Barang Optik | Barang | Atribut Optik untuk barang | 0..1 : 1 | Opsional (barang Optik wajib punya) | Optik menempel Master Data | `OPT-DEC-031` |

**Penghapusan dan pengarsipan:** tidak ada konsep Optik yang dihapus permanen. Resep, versi,
pesanan, dan keluhan bersifat catatan klinis dan finansial. Yang ada hanya batal (dengan alasan)
atau nonaktif.

---

## G. Model lifecycle/status

### G.1 Pemeriksaan Refraksi

| Dari | Tindakan | Ke | Siapa | Syarat | Audit |
| --- | --- | --- | --- | --- | --- |
| — | Mulai | `Draft` | RO/Sp.M | Kunjungan Optik terbuka; izin berlaku | pelaku, waktu |
| `Draft` | Simpan final | `Final` | RO/Sp.M | Hasil ukur minimum terisi | pelaku, waktu; menerbitkan `EVT-OPT-01` sekali |
| `Draft` | Batalkan | `Batal` | RO/Sp.M | Alasan wajib; tidak ada tagihan karena belum final | pelaku, waktu, alasan |

Koreksi sesudah `Final`: revisi berversi tanpa tagih ulang (`OPT-DEC-047`).

### G.2 Pesanan Optik (`OPT-DEC-016`, diperluas `OPT-DEC-020`, `028`, `035`, `038`, `044`)

| Dari | Tindakan | Ke | Siapa | Syarat / pemeriksaan invariant |
| --- | --- | --- | --- | --- |
| — | Buat | `Draft` | Petugas penjualan | Kunjungan Optik terbuka; resep berlaku untuk pesanan berukuran dan untuk lensa kontak |
| `Draft` | Simpan & tagihkan | `Menunggu Uang Muka` | Petugas penjualan | Item terisi, semua barang bertarif berlaku; fakta tagihan per item terbit (`EVT-OPT-03`); persentase minimum dibekukan |
| `Menunggu Uang Muka` | Kirim ke pengerjaan | `Dalam Pengerjaan` | Petugas penjualan | Terbayar ≥ minimum dari bagian pasien; tujuan pengerjaan dan perkiraan jadi terisi; aturan sementara satu pesanan belum lunas per kunjungan; stok cukup (`n4`). **Resep terkunci, stok dipotong** (`EVT-OPT-04`) |
| `Dalam Pengerjaan` | Tandai siap | `Siap Diambil` | Petugas penjualan | Kacamata jadi diterima |
| `Siap Diambil` | Serahkan | `Diserahkan` | Petugas penjualan | Lunas; *fitting* dicatat; garansi mulai (`EVT-OPT-06`) |
| `Draft` (penjualan langsung) | Tagihkan & serahkan | `Diserahkan` | Petugas penjualan | Lunas; lensa kontak wajib resep berlaku; stok dipotong saat diserahkan |
| `Draft` / `Menunggu Uang Muka` | Batalkan | `Batal` | Petugas penjualan | Alasan wajib; baris tagihan dibatalkan; tidak ada stok yang perlu dikembalikan |
| `Dalam Pengerjaan` / `Siap Diambil` | Ajukan batal → disetujui | `Batal` | Petugas mengajukan; **Kepala Optik** menyetujui | Alasan; tanda per lensa stok "sudah/belum dipotong". Frame dan lensa stok belum dipotong: baris dibatalkan, stok kembali. Lensa custom dan lensa sudah dipotong: baris tetap ditagih (`EVT-OPT-05`) |
| `Siap Diambil` | Tutup tidak diambil | `Tidak Diambil` | Kepala Optik | Melewati batas akhir (bawaan 90 hari); item mengikuti aturan pembatalan sesudah pengerjaan (`OPT-DEC-018` → `010`). Karena kacamata sudah jadi, frame terpasang dan lensa terpotong tetap ditagih, dan kacamata disimpan; penyerahan susulan setelah lunas (`OPT-DEC-049`) (`EVT-OPT-07`) |

Status terminal: `Diserahkan`, `Batal`, `Tidak Diambil`. Pembukaan kembali tidak ada; keluhan
sesudah `Diserahkan` lewat `AGG-OPT-04`.

**Contoh perjalanan lengkap:** pesanan ORD-0002 (frame Rp 400.000 + lensa stok Rp 300.000,
tunai) disimpan 09.45 → `Menunggu Uang Muka`. Terbayar Rp 350.000 (50%) → dikirim ke bengkel
10.00 → `Dalam Pengerjaan`, stok frame dan lensa masing-masing −1. Pasien membatalkan pukul 11.00
saat lensa belum dipotong. Kepala Optik menyetujui → `Batal`, kedua baris tagihan dibatalkan,
stok kembali +1, dan uang muka Rp 350.000 menjadi kelebihan bayar yang diproses Billing.

### G.3 Keluhan Garansi

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| — | Catat keluhan | `Diajukan` | Petugas penjualan / RO | Pesanan asal `Diserahkan`; kunjungan keluhan baru; jenis keluhan dipilih; status "dalam/di luar garansi" dihitung otomatis |
| `Diajukan` | Periksa ulang | `Diperiksa` | RO/Sp.M | Temuan dicatat (boleh disertai pemeriksaan refraksi baru) |
| `Diperiksa` | Setujui remake | `Disetujui` | Kepala Optik | Dalam masa garansi; menerbitkan `EVT-OPT-08` |
| `Diperiksa` | Tolak | `Ditolak` | Kepala Optik | Alasan wajib (misalnya kerusakan karena benturan) |

### G.4 Resep Optik

Resep tidak punya status bertingkat. Ia punya dua sifat turunan:

| Sifat | Kapan benar | Akibat |
| --- | --- | --- |
| **Terkunci** | Ada pesanan yang merujuk versinya dan berstatus `Dalam Pengerjaan` atau sesudahnya | Revisi ditolak |
| **Berlaku** | Tanggal terbit + batas usia (dibekukan dari pengaturan saat dibuat) ≥ hari ini (WIB) | Pesanan baru berukuran dan penjualan lensa kontak diterima |

Perubahan isi selalu menambah versi baru. Pembatalan resep tidak termasuk keputusan
(`OPT-DEC-007` memilih revisi berversi), jadi tidak dimodelkan.

---

## H. Tanggung jawab authorization

Semua peran berasal dari keputusan pemilik. Pemetaan ke departemen-jabatan dilakukan lewat
kebijakan akses yang ada (`OPT-CAP-08`).

| Tindakan | RO | Sp.M | Petugas penjualan | Kepala Optik | Admin Optik | Kasir (Billing) | Dasar |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Lihat pemeriksaan dan resep | ✔ | ✔ | ✔ (baca, untuk membuat pesanan) | ✔ | — | — | `OPT-DEC-005` |
| Buat/finalkan pemeriksaan, buat/revisi/salin resep | ✔ | ✔ | ✘ | — | — | — | `OPT-DEC-005`, `011`, `017`, `039` |
| Lewati rentang isian resep | ✘ | ✔ (dengan alasan) | ✘ | — | — | — | `OPT-DEC-045` |
| Buat, tagihkan, kirim, tandai siap, serahkan pesanan | — | — | ✔ | ✔ | — | — | `OPT-DEC-016` |
| Batal sebelum pengerjaan | — | — | ✔ | ✔ | — | — | `OPT-DEC-016` |
| Setujui batal sesudah pengerjaan, tutup tidak diambil | — | — | ✘ | ✔ | — | — | `OPT-DEC-010`, `018` |
| Ajukan diskon | — | — | ✔ | ✔ | — | — | `OPT-DEC-027` |
| Setujui diskon di atas batas | — | — | ✘ | ✔ (sebagai penyetuju kebijakan Billing) | — | — | `OPT-DEC-027` |
| Periksa ulang keluhan | ✔ | ✔ | — | — | — | — | `OPT-DEC-015` |
| Putuskan keluhan/remake | — | — | — | ✔ | — | — | `OPT-DEC-015` |
| Ubah pengaturan Optik | — | — | — | — | ✔ | — | `OPT-DEC-009`, `040`, `041` |
| Terima uang muka dan pelunasan | — | — | — | — | — | ✔ | `OPT-DEC-003` |

"—" berarti tidak diatur keputusan. Usulan desain: ditolak secara bawaan (*fail-closed*).

---

## I. Model audit/histori

| Kejadian | Yang wajib tercatat | Dasar |
| --- | --- | --- |
| Pemeriksaan final | Pemeriksa, nomor izin yang berlaku saat itu, waktu | `OPT-DEC-005`, `012` |
| Versi resep | Pembuat/perevisi, waktu, alasan revisi, nilai asli resep plus, alasan lewat rentang | `OPT-DEC-007`, `011`, `045` |
| Salinan resep luar | Penulis asli, fasilitas, tanggal resep, penyalin | `OPT-DEC-017` |
| Perpindahan status pesanan | Status asal, status tujuan, pelaku, waktu, alasan (wajib untuk batal/tutup) | RCG `n8` |
| Pengajuan dan persetujuan batal | Pengaju, penyetuju, alasan, tanda "sudah dipotong" per lensa | `OPT-DEC-010`, `038` |
| Nilai dibekukan | Persentase uang muka, usia resep, masa garansi yang berlaku saat dibuat | `OPT-DEC-009`, `040`, `041` |
| Keputusan keluhan | Pemeriksa ulang, temuan, penyetuju, alasan | `OPT-DEC-015` |
| Perubahan pengaturan | Nilai lama, nilai baru, pelaku, waktu | `OPT-DEC-040` |

Catatan proyek: `LoggerService.AuditAsync` tidak menulis objek payload, jadi isi audit harus
berada di teks pesan atau di jejak milik aggregate sendiri.

---

## J. Model integrasi

| ID | Produsen → Konsumen | Tujuan | Sumber kebenaran | Sinkron? | Idempotensi | Gagal / rekonsiliasi |
| --- | --- | --- | --- | --- | --- | --- |
| `INT-OPT-01` | Optik → Registrasi | Daftar kunjungan Optik (jalur unit penunjang) | Registrasi | Sinkron | Satu permintaan pendaftaran per kedatangan | Gagal = kunjungan tidak terbentuk, Optik tidak membuat apa pun |
| `INT-OPT-02` | Optik → Registrasi | Tutup kunjungan saat pesanan terakhir terminal, atau saat sapuan akhir hari untuk kunjungan tanpa pesanan berpengerjaan | Registrasi | Asinkron (event) | Per kunjungan | Gagal tutup → coba ulang; kunjungan terbuka tidak merusak data |
| `INT-OPT-03` | HR → Optik | Izin berlaku per tanggal (adapter baca) | HR | Sinkron baca | — | HR tidak tersedia → simpan resep ditolak (*fail-closed*) |
| `INT-OPT-04` | Optik → Billing | Fakta tagihan: tarif pemeriksaan (per pemeriksaan final) dan barang (per item pesanan) sebagai sumber resmi baru | Billing (harga dari tarif) | Asinkron dengan penerimaan tercatat | Kunci: pemeriksaan/item + versi fakta | Ditolak (tarif hilang) → antrean rekonsiliasi Billing; pesanan tetap `Draft` sampai fakta diterima |
| `INT-OPT-05` | Optik → Billing | Pembatalan baris tagihan (frame, lensa belum dipotong) | Billing | Asinkron | Per item | Seperti di atas |
| `INT-OPT-06` | Billing → Optik | Bagian pasien dan jumlah terbayar per pesanan (per baris bila tersedia; per tagihan dengan aturan sementara) | Billing | Sinkron baca | — | Billing tidak tersedia → kirim ke pengerjaan dan penyerahan ditolak |
| `INT-OPT-07` | Optik → Farmasi | Potong stok, kembalikan stok, potong untuk remake | Farmasi (`DrugStockService`) | Sinkron dalam satu tindakan | Kunci tetap per pesanan per tindakan, misalnya `ORD-0001:issue` | Stok kurang → tindakan Optik ditolak utuh, status tidak berpindah |
| `INT-OPT-08` | Optik → Medical Fee | Jasa medis pemeriksa saat pemeriksaan final | Medical Fee | Asinkron | Per pemeriksaan | Antrean Medical Fee |
| `INT-OPT-09` | Rekam Medis ← Optik | Pembaca visus, refraksi, dan resep versi terakhir | Optik | Baca sesuai permintaan | — | — |
| `INT-OPT-10` | Rawat Jalan → Optik | Rujukan Poli Mata (kunjungan asal + dokter perujuk) | Registrasi/Rawat Jalan | Sinkron | — | Tanpa tautan → kunjungan tetap bisa dibuat sebagai datang sendiri |

Tidak ada integrasi eksternal di MVP: lab lensa luar dicatat manual, dan kanal pesan di luar scope.

---

## K. Dampak billing

**Klasifikasi: berdampak pada charge.** Semua kebijakan berikut sudah diputuskan pemilik Optik.
Bagian Billing menunggu persetujuan pemiliknya (`OPT-APR-001`..`004`, `011`).

| Kejadian | Dampak | Dasar |
| --- | --- | --- |
| Pemeriksaan final | Satu baris tarif pemeriksaan, berapa pun nasib pesanannya | `OPT-DEC-042` |
| Pesanan disimpan & ditagihkan | Satu baris per item, harga dari tarif berlaku | `OPT-DEC-028`, `029` |
| Uang muka | Pembayaran sebagian atas tagihan, tanpa deposit | `OPT-DEC-028` |
| Diskon | Diajukan dari Optik, dicatat di baris tagihan oleh kebijakan Billing | `OPT-DEC-027` |
| Batal sebelum pengerjaan | Semua baris pesanan dibatalkan | `OPT-DEC-016` |
| Batal sesudah pengerjaan / tidak diambil | Baris frame dan lensa stok belum dipotong dibatalkan; lensa custom dan lensa sudah dipotong tetap; sisa kurang bayar = piutang pasien (wewenang Billing) | `OPT-DEC-010`, `018`, `028`, `038` |
| Remake | Tidak ada fakta tagihan; stok tetap dipotong | `OPT-DEC-015` |
| Penjamin | Minimum uang muka dari bagian pasien; perhitungan tanggungan milik Billing | `OPT-DEC-044` |

---

## L. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan** untuk `AGG-OPT-01`/`02` dan transisi pesanan
yang memakai resep.

| Risiko | Pengaman arsitektur | Dasar |
| --- | --- | --- |
| Resep dibuat orang tak berwenang | Hanya RO/Sp.M; izin berlaku per tanggal WIB; *fail-closed* bila HR tak tersedia | `OPT-DEC-005`, `012` |
| Salah tanda silinder | Disimpan minus; transposisi tercatat beserta aslinya | `OPT-DEC-045` |
| Ukuran tidak wajar | Rentang ketat; lewat rentang hanya Sp.M dengan alasan | `OPT-DEC-045` |
| Lensa dikerjakan dari ukuran yang berubah diam-diam | Pesanan merujuk versi tertentu; resep terkunci saat pengerjaan | `OPT-DEC-007` |
| Kacamata dari ukuran usang | Resep berlaku ≤ 12 bulan | `OPT-DEC-023`, `040` |
| Lensa kontak tanpa evaluasi | Semua lensa kontak wajib resep lensa kontak berlaku | `OPT-DEC-046` |
| Kacamata diserahkan ke pasien yang salah | Tanda "identitas dicocokkan" wajib; pengambil wakil dicatat | `OPT-DEC-048` |

---

## M. Gap arsitektur

| ID | Isu | Status bukti | Dampak | Arah |
| --- | --- | --- | --- | --- |
| `GAP-OPT-01` | Koreksi hasil ukur pemeriksaan sesudah `Final` — **ditutup `OPT-DEC-047`** (revisi berversi, tanpa tagih ulang) | `CONFIRMED` | `NON_BLOCKING_STANDARD`: tidak mengubah aggregate, hanya aturan koreksi | Usulan: revisi berversi seperti resep, tanpa menagih ulang. Dikonfirmasi pemilik saat desain (`/grill-me` singkat bila perlu) |
| `GAP-OPT-02` | Verifikasi identitas pasien saat penyerahan kacamata — **ditutup `OPT-DEC-048`** (wajib dicocokkan, boleh diwakilkan) | `CONFIRMED` | `NON_BLOCKING_STANDARD`: satu syarat pada transisi `Diserahkan` | Usulan: petugas mencocokkan nama + tanggal lahir atau nomor RM, lalu tercatat. Butuh konfirmasi pemilik |
| `GAP-OPT-03` | Pembayaran sebagian pada kunjungan terbuka — **terjawab fakta di tahap desain**: tender diterima pada tagihan `Open` dan tagihan difinalisasi otomatis saat lunas, sehingga dibutuhkan kebijakan finalisasi sumber Optik (`OPT-APR-012`, `02-backend-architecture.md` bagian 9) | `CONFIRMED` (fakta) | Ditangani di desain | — |
| `GAP-OPT-04` | Rincian isian hasil ukur (`n1`) dan atribut per kategori (`n2`) | `PROPOSED` | `NON_BLOCKING_STANDARD` / `CONFIGURABLE_DEFAULT` | Dikonfirmasi di screen brief |
| `GAP-OPT-06` | Kacamata yang `Tidak Diambil` — **ditutup `OPT-DEC-049`** (tetap ditagih penuh, disimpan; penyerahan susulan setelah lunas) | `CONFIRMED` | `NON_BLOCKING_STANDARD`: hanya cabang pada transisi `Tidak Diambil`, tidak mengubah aggregate | Usulan: tetap ditagih (frame sudah terpasang, setara `OPT-DEC-010`). Dikonfirmasi pemilik |
| `GAP-OPT-05` | Persetujuan modul lain `OPT-APR-001`..`011` | — | Menahan task, bukan arsitektur | Diminta ke masing-masing pemilik |

Tidak ada Decision ID bisnis berstatus `BLOCKING` yang terbuka.

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY`** untuk `OPT-S1`..`OPT-S10`.

- Bounded context dapat dipertahankan: tiga context Optik dan delapan context pemilik lain dipakai lewat pemiliknya.
- Ownership material sudah tuntas: tidak ada konsep master bersama yang digandakan.
- Lifecycle dan invariant penting sudah terwakili di empat aggregate.
- Tidak ada keputusan pemblokir yang terbuka.
- Dampak billing dan keselamatan klinis eksplisit.
- Gap yang tersisa tidak memblokir dan diberi arah.

---

## O. Handoff ke `/design-business-module`

| Butir | Nilai |
| --- | --- |
| Scope | `OPT-S1`..`OPT-S10`; `OPT-S11` (BPJS) tetap di luar |
| Kesiapan requirement | `OPT-RCG-001` r2 — `READY_FOR_DOMAIN_DESIGN` |
| Kesiapan arsitektur | `OPT-HDA-001` r1 — `DOMAIN_ARCHITECTURE_READY` |
| Keputusan | `OPT-DEC-001`..`046` (`014` superseded); `DEC-OPT-001`..`005` ditutup |
| Persetujuan menunggu | `OPT-APR-001`..`011` |
| Prefix | `Opt` (`OPT-DEC-036`), wajib didaftarkan sebelum model pertama |
| Source SHA | Backend `2a5094ca`, frontend `b6dd6c304` |
| Jejak requirement → domain | Kolom "Dasar" dan "Bukti" di bagian D, G, H, K, dan L |
| Yang harus dipertajam di desain | Nama fisik, ERD, kontrak API, kunci idempotensi, rentang BC/DIA, `GAP-OPT-01`..`04`, `06` |
| Yang tidak boleh diubah di desain | Makna versi/kunci resep, pemisahan pemeriksaan–resep–pesanan–keluhan, uang muka sebagai bayar sebagian, satu fakta tagihan per item, dan stok hanya lewat Farmasi |
