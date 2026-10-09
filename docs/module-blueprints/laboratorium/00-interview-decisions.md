# Laboratorium — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Revision | `94` |
| Status | `draft` |
| Pass | `Scope pass` selesai; `Closure pass` selesai (tiga putaran); `Amendment pass` putaran 1 selesai; **putaran 2 selesai** (Penerimaan Sampling/Specimen, 2026-09-14); **putaran 3 selesai** (metode pembayaran, 2026-09-14); **putaran 4 selesai** (pendaftaran lewat kiosk dan pemecahan pesanan per disiplin, 2026-09-15); **putaran 5 selesai sebagian** (Menu Hasil, pencetakan, dan pengiriman hasil ke pasien, 2026-09-16 — sepuluh klarifikasi diadopsi `LAB-DEC-064`..`LAB-DEC-073`; **empat dari tujuh hal yang dibuka ditutup pada hari yang sama**; sisanya `LAB-OPEN-029`, `LAB-OPEN-030`, `LAB-OPEN-032`); **putaran 9 selesai** (halaman Hasil Pemeriksaan Mikrobiologi, 2026-09-21 — enam belas keputusan `LAB-DEC-095`..`LAB-DEC-110`; **enam pertentangan dengan keputusan terkunci diselesaikan seluruhnya ke arah blueprint**; `LAB-OPEN-017` dan `LAB-OPEN-030` ditutup); **putaran 10 selesai** (penutupan closure question impact scan capability map revision 4, 2026-09-21 — `LAB-DEC-111`..`LAB-DEC-113`; `LAB-CONFLICT-010` dan ketiga `LAB-CLOSE-010/011/012` ditutup; `LAB-COORD-014` dibuka dan **tidak memblokir**); **putaran 11 selesai** (bukti cetak tiga disiplin `LAB-EVD-005`, 2026-09-21 — `LAB-DEC-114`..`LAB-DEC-121`; **empat keputusan berumur beberapa jam diperbaiki**; `LAB-OPEN-039` sebagian ditutup; `LAB-OPEN-029` **sengaja tetap terbuka**); **putaran 12 selesai** (cetakan Mikrobiologi varian bakteri `LAB-EVD-006`, 2026-09-21 — `LAB-DEC-122`..`LAB-DEC-128`; **`LAB-DEC-116` yang berumur kurang dari satu jam dikoreksi**; interpretasi `S`/`I`/`R` menjadi terhitung); **putaran 13 selesai** (dataset specimen SNOMED CT `LAB-EVD-007`, 2026-09-21 — `LAB-DEC-129`..`LAB-DEC-132`; **`LAB-OPEN-040` DITUTUP**; `LAB-DEC-098` dikoreksi pada pilihan kolomnya); **putaran 14 selesai** (PRD Hasil Pemeriksaan PK & Mikrobiologi `LAB-EVD-008`, 2026-09-24 — `LAB-DEC-133` menetapkan PRD sebagai bukti untuk direkonsiliasi; `LAB-DEC-134`..`LAB-DEC-145` beserta `LAB-FE-015`/`LAB-FE-016` menutup **keempat belas pertentangan dan keenam butir baru**, nol keputusan terkunci dicabut; 45 koreksi PRD dicatat; `PRD1-CLIN-01` terbuka bagi pihak klinis, `LAB-COORD-015` bagi pemilik keamanan/platform, `PRD1-OPEN-01` tidak memblokir); **closure pass putaran 15 selesai** (closure question capability map revision 5, 2026-09-24 — `LAB-DEC-146`..`LAB-DEC-149` serta `LAB-FE-017` menutup `LAB-CLOSE-013`..`LAB-CLOSE-016` dan `LAB-CONFLICT-012`/`LAB-CONFLICT-013`, nol keputusan terkunci dicabut; `LAB-COORD-016` dibuka); **putaran 16 selesai** (jawaban dr. Bima `LAB-EVD-009`, 2026-09-24 — `LAB-DEC-150` menjawab sebagian `DEC-LAB-011` dan **menggantikan butir 2 `LAB-DEC-022`**; `LAB-DEC-151` menutup `PRD1-CLIN-01`; sisa `DEC-LAB-011` diajukan `LAB-REQ-014`); **putaran 17 selesai** (jawaban tertulis dr. Bima `LAB-EVD-010`, 2026-09-25 — `LAB-DEC-152` menutup sisa `DEC-LAB-011` dan `LAB-OPEN-034`, `LAB-DEC-153` menutup `DEC-LAB-018`; `LAB-OPEN-044` dibuka); **putaran 18 selesai** (keputusan pemilik modul `LAB-EVD-011`, 2026-09-25 — `LAB-DEC-154` menutup `LAB-CONFLICT-014`, `LAB-DEC-155` menjawab sebagian pertanyaan pemakaian sebelum `S6`, `LAB-DEC-156` mengamandemen label BR-88; `LAB-OPEN-045` dibuka); **putaran 19 selesai** (enam penahan gerbang `LAB-RCG-001-r10`, 2026-09-25 — `LAB-DEC-157`..`LAB-DEC-164` menutup `DEC-LAB-023`, `DEC-LAB-024`, `DEC-LAB-026`, `DEC-LAB-027` dan mempersempit `DEC-LAB-022`, `DEC-LAB-025`; `LAB-COORD-018` dibuka; satu pertentangan dengan bukti lapangan diselesaikan ke arah blueprint); **putaran 21 selesai** (keputusan tertulis pemilik modul `LAB-EVD-012`, 2026-10-06 — `LAB-DEC-168`..`LAB-DEC-187` menutup dua belas penahan dan menunda `S16b` ke Phase 2; enam klarifikasi, seluruhnya pilihan yang direkomendasikan; `LAB-SIGN-002` dibuka bagi ratifikasi klinis sebelum go-live); **putaran 22 selesai** (menu dan aksi alur kerja FE v1 pada daftar pasien lab, 2026-10-06 — `LAB-DEC-188`..`LAB-DEC-192` beserta `LAB-FE-030`/`LAB-FE-031`; lima pertanyaan, seluruhnya pilihan yang direkomendasikan; `LAB-DEC-048`, `LAB-DEC-060`, dan `LAB-DEC-062` diamandemen; nol kontrak backend berubah; `LAB-OPEN-051` dibuka); **putaran 23 selesai** (urutan Terima Sampling dan Konfirmasi mengikuti FE v1, 2026-10-07 — `LAB-DEC-193`..`LAB-DEC-197` beserta BR-138; pencatatan `Q-P22-01`/`02` sebagai `LAB-DEC-198`/`199`; `LAB-FE-032`/`033`; lima pertanyaan, seluruhnya pilihan yang direkomendasikan; `LAB-DEC-061`, `188`, `189`, `190` diamandemen; `LAB-OPEN-027` dan `LAB-CONFLICT-017` ditutup; `LAB-OPEN-052` dibuka; **menuntut amandemen empat kontrak**); **putaran 24 selesai** (dokter pemeriksa bagi pengonfirmasi dan jawaban detail konfirmasi, 2026-10-07 — dua temuan verifikasi `FE-LAB-50`; `LAB-DEC-200`..`LAB-DEC-203` beserta BR-139; `LAB-FE-034`; `AC-289`..`AC-293`; empat pertanyaan, seluruhnya pilihan yang direkomendasikan; **menuntut amandemen `LAB-API-v1`** dan urutan rilis `MVP-12c`); **putaran 25 selesai** (Beranda Lab mengikuti FE v1, 2026-10-07 — `LAB-DEC-204`..`LAB-DEC-211` beserta BR-140; `LAB-FE-035`; `AC-294`..`AC-301`; sebelas pertanyaan, seluruhnya pilihan yang direkomendasikan; butir *grafik Beranda versi v1* `LAB-DEC-188` diputuskan; **menuntut amandemen `LAB-API-v1`**); **putaran 26 selesai** (alur Lab dari kiosk sampai hasil mengikuti FE v1, 2026-10-08 — mencatat `LAB-EVD-013`; `LAB-DEC-212`..`LAB-DEC-221` beserta BR-141; `LAB-FE-036`; `AC-302`..`AC-313`; batas scope dan tiga pertanyaan, seluruhnya pilihan yang direkomendasikan; `LAB-DEC-191` digantikan; `AC-45` tetap berlaku; **menuntut amandemen `LAB-API-v1`**); **putaran 27 selesai** (lingkup kunci Lunas Proses Pemeriksaan, 2026-10-08 — `LAB-DEC-222`..`LAB-DEC-224`; `AC-314`..`AC-317`; batas scope dan tiga pertanyaan, seluruhnya pilihan yang direkomendasikan; `LAB-DEC-218` diamandemen; `LAB-CONFLICT-018` diselesaikan sisi Laboratorium; `LAB-COORD-020` dibuka; **menuntut amandemen `LAB-API-v1` dan `LAB-VAL-v1`**); **putaran 28 selesai** (CITO dari pemesanan hilang saat wadah direncanakan, 2026-10-09 — `LAB-DEC-225`..`LAB-DEC-228`; `AC-318`..`AC-322`; batas scope dan lima pertanyaan, seluruhnya pilihan yang direkomendasikan; `LAB-CONFLICT-019` dibuka dan ditutup; **menuntut amandemen `02-backend-architecture.md` 28.8**); **putaran 29 selesai** (rencana ulang sesudah wadah dibatalkan, temuan T1 `BE-LAB-94`, 2026-10-09 — `LAB-DEC-229`; `AC-323`..`AC-325`; satu pertanyaan, pilihan yang direkomendasikan) |
| Product/domain owner | **Yoga Aji Pratama** (`yogaaji452@gmail.com`), ditetapkan 2026-09-01 |
| Backend SHA | Dipindai pada `466a7127` (298 commit sejak `c87d9c0`). **`HEAD` bergeser ke `9067fa73` pada 2026-09-14 saat sesi berjalan** — diperiksa: source Laboratorium dan seluruh berkas yang menjadi dasar temuan **tidak berubah**, sehingga scan tetap sahih. **Putaran 4 dipindai pada `9067fa73`; `HEAD` kembali bergeser ke `e2152709` pada 2026-09-15 saat sesi berjalan** karena pekerjaan `BE-LAB-20`..`BE-LAB-25` di-commit (`7cd82c26`) lalu di-merge dari `origin/QuilvianIntegrationBackend` — diperiksa: merge itu **tidak menyentuh satu pun berkas Laboratorium** di luar commit tersebut, dan seluruh perubahan `BE-LAB-21`, `BE-LAB-22`, serta `BE-LAB-25` terverifikasi utuh sesudahnya |
| Frontend SHA | `9cd4cd03f`, dipindai 2026-09-14 (155 commit sejak `688daff90`). **Putaran 14 dibaca pada BE `ddeb5ed8` (branch `yoga`) + FE `72607a087`, 2026-09-24** |
| Tanggal sesi | 2026-09-01; dilanjutkan 2026-09-14, 2026-09-15, dan seterusnya; putaran 14 dibuka 2026-09-24 |
| Capability map | `01-existing-capability-map.md` **revision 5**, impact scan pada BE `ddeb5ed8` + FE `72607a087`, 2026-09-24 — **segar**, dipakai closure pass putaran 15. *(Sebelumnya revision 4 pada BE `981e002c`, 2026-09-21; semula revision 3 pada BE `466a7127` + FE `9cd4cd03f`, 2026-09-14, yang menutup `LAB-OPEN-022` dan membuka `CONF-02`.)* |

> **Catatan penting soal cara membaca dokumen ini.**
> Dokumen ini adalah catatan wawancara, bukan desain dan bukan izin menulis kode.
> Isinya memisahkan empat hal: **Fact** (fakta yang dibuktikan dari kode atau dokumen yang
> sudah disetujui), **Decision** (keputusan manusia yang berwenang), **Assumption** (dugaan
> yang belum dikonfirmasi), dan **Open Question** (pertanyaan yang masih menunggu jawaban
> pemilik proses).

---

## Peringatan Prasyarat

1. **Scope dikunci tanpa audit kemampuan existing yang formal.**
   Berkas `docs/module-blueprints/laboratorium/01-existing-capability-map.md` belum ada.
   Artinya kemungkinan tumpang tindih (duplikasi) dengan modul lain belum diperiksa secara
   menyeluruh. Bukti yang dipakai di dokumen ini adalah pembacaan langsung terhadap source
   code pada SHA di atas, bukan hasil audit `/qv-trace`.

2. **Sebagian keputusan Laboratorium sudah dikunci lebih dulu di modul lain.**
   Amendment `RJ-BIL-GATE-DEC-003` pada blueprint `rawat-jalan` sudah mengunci siklus hidup
   pesanan lab, spesimen, hasil, pembatalan, dan kelayakan tagih. Wawancara ini **tidak boleh
   membuka ulang** keputusan tersebut; yang digali adalah bagian Laboratorium yang belum
   pernah ditanyakan.

3. **Dokumen tata kelola backend canonical ada dan tetap berlaku, tetapi bukan di `docs/engineering/`.**
   `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tidak dicabut.
   Keduanya tinggal di repository suite Skill `QuilvianEngineeringSkills`, bukan di dalam
   repository backend. Path `docs/engineering/` yang masih disebut `AGENTS.md` sudah usang.
   `LAB-OPEN-002` **ditutup** 2026-09-01 oleh `LAB-FACT-007`. Penutupannya membuka dua
   penghambat implementasi yang baru dan nyata: rules root yang terpasang belum memuat kedua
   dokumen itu (`LAB-OPEN-018`), dan registry masih mencatat modul Laboratorium berstatus
   `PLANNED` sehingga `QBE-MOD-002` menahan pembuatan entity `Lab*` (`LAB-OPEN-019`).

---

## Batas Scope Modul (disetujui pemilik modul 2026-09-01)

**Modul:** Laboratorium (`laboratorium`).

**Satu kalimat batas scope:**
Modul Laboratorium mengurus perjalanan pemeriksaan laboratorium mulai dari pesanan dokter,
pengambilan dan penerimaan sampel, proses pemeriksaan, sampai hasil dinyatakan sah dan
dirilis — beserta bukti siapa melakukan apa dan kapan.

### Di dalam scope

| No | Kemampuan | Penjelasan singkat | Rilis |
|---:|---|---|---|
| 1 | Pesanan laboratorium (*lab order*) | Dokter memesan pemeriksaan untuk satu kunjungan pasien | Sudah ada |
| 2 | Siklus hidup sampel | Rencana ambil, pengambilan, penerimaan di lab, penerimaan/penolakan sampel, ambil ulang | Sudah ada |
| 3 | Daftar kerja petugas lab (*worklist*) | Antrian pekerjaan lab per hari atau per bagian | **Rilis 1** |
| 4 | Hasil pemeriksaan | Pengisian nilai hasil, verifikasi, validasi, dan rilis hasil | **Rilis 1** |
| 5 | Nilai kritis | Hasil yang berbahaya bagi pasien dan wajib dikabarkan segera ke dokter | **Rilis 1** |
| 6 | Koreksi hasil setelah rilis | Perbaikan hasil dengan tetap menyimpan riwayat versi lama | **Rilis 1** |
| 7 | Penyajian hasil ke dokter dan pasien | Melihat dan mencetak hasil | **Rilis 1** |
| 8 | Seluruh tampilan frontend Laboratorium | Dibangun dari nol, tidak ada satu pun yang bisa dipakai ulang | **Rilis 1** |
| 9 | Pengiriman fakta ke Billing | Laboratorium hanya **mengirim fakta**, misalnya "sampel diterima", tanpa menghitung uang | **Rilis 1** |
| 10 | Riwayat perubahan status yang tidak bisa diubah | Jejak audit untuk seluruh perpindahan status | Sudah ada |
| 11 | Tabel batas nilai per pemeriksaan | Satuan hasil, batas normal bawah/atas, batas kritis bawah/atas, batas waktu cito, pembeda jenis kelamin dan kelompok umur | **Rilis 1** |
| 12 | Pemberitahuan tersimpan untuk dokter | Kotak pemberitahuan berisi nilai kritis dan koreksi hasil, dengan status sudah dibaca atau belum | **Rilis 1** |
| 13 | Penandaan cito dan daftar pantau keterlambatan | Dokter menandai cito, sistem memantau pesanan cito yang lewat batas waktu | **Rilis 1** |
| 14 | Sisa katalog pemeriksaan laboratorium mandiri | Jenis sampel, wadah, volume minimal, metode, paket/panel | Rilis 2 |

### Di luar scope — milik modul lain

| No | Kemampuan | Pemilik modul | Alasan |
|---:|---|---|---|
| 1 | Perhitungan tarif, tagihan, void, refund, pembayaran | `billing-kasir` | Sudah dikunci: Laboratorium tidak punya wewenang finansial (`RJ-BIL-GATE-DEC-003`) |
| 2 | Pemeriksaan radiologi | Modul Radiologi | Sudah dikunci terpisah di `RJ-BIL-GATE-DEC-004` |
| 3 | Pendaftaran pasien dan pembentukan kunjungan (*encounter*) | `registration-management` | Laboratorium hanya menempel pada kunjungan yang sudah ada |
| 4 | Resep dan obat | `pharmacy` | Alur berbeda |
| 5 | Penyimpanan dokumen rekam medis dan penomoran berkas | `rekam-medis` | Laboratorium hanya menyerahkan hasil sebagai isi rekam medis |
| 6 | Data induk umum: pasien, dokter, unit layanan, tarif | `master-data` | Dipakai bersama, bukan dimiliki Laboratorium |
| 7 | Bank darah dan transfusi | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 8 | Mikrobiologi (kultur dan uji kepekaan antibiotik) | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 9 | Patologi Anatomi (blok parafin, slide, sitologi) | Belum ada modul | Dikeluarkan oleh `LAB-DEC-002` |
| 10 | Stok, pembelian, dan pencatatan pemakaian reagen | `pharmacy`/`inventory` | Dikeluarkan oleh `LAB-DEC-014` |

> Daftar di atas sudah dikonfirmasi lewat `LAB-DEC-001`, `LAB-DEC-002`, dan `LAB-DEC-014`
> pada 2026-09-01.

### Di luar scope — untuk modul lain

| Kebutuhan | Alasan dikeluarkan | Diserahkan kepada |
|---|---|---|
| Mikrobiologi: kultur bakteri dan uji kepekaan antibiotik | Hasilnya bertahap, bukan sekali jadi. Hasil sementara keluar hari ke-2, hasil akhir hari ke-5. Alur hasil `Pending → InProcess → Completed → Validated → Released` yang sudah dikunci `LAB-INH-003` tidak cukup untuk pola ini dan akan memerlukan amendment tersendiri | Slice/modul Mikrobiologi berikutnya |
| Patologi Anatomi: blok parafin, pemotongan slide, sitologi | Alur fisik sampelnya berbeda total dari sampel cair, dan hasilnya berupa narasi diagnosis, bukan angka | Slice/modul Patologi Anatomi berikutnya |
| Bank Darah: uji cocok serasi (*crossmatch*), penelusuran kantong darah, pelaporan reaksi transfusi | Punya aturan keselamatan pasien tersendiri yang berakibat fatal bila salah tangani. Tidak boleh ditumpangkan pada alur sampel biasa | Modul Bank Darah tersendiri |
| Katalog pemeriksaan lab mandiri | Tetap milik Laboratorium, tetapi ditunda ke Rilis 2 oleh `LAB-DEC-001` | Rilis 2 modul ini |

---

## Amendment Pass Putaran 2 — Penerimaan Sampling/Specimen (dibuka 2026-09-14)

**Bukti pemicu.** Pemilik modul melampirkan artifact **`Penerimaan Sampling Specimen Lab.md`**
pada sesi 2026-09-14. Artifact itu memuat 16 capability (`CAP-001`..`CAP-016`) dan 29 rule
(`RULE-001`..`RULE-029`), hasil 14 klarifikasi bisnis tertanggal 12 September 2026.

Mengikuti konvensi modul ini, berkas bukti **tidak disalin** ke dalam folder blueprint —
sama seperti `Analisis_Konsolidasi_Modul_Laboratorium.md` yang menjadi bukti `LAB-DEC-025`
sampai `LAB-DEC-031`. Bukti dikutip dengan namanya. Dicatat sebagai `LAB-EVD-001`.

> **Peringatan yang harus dibaca lebih dulu.** Artifact ini **bukan** dokumen yang lahir dari
> blueprint Laboratorium. Ia menggambarkan satu layar sebagaimana dipahami di lapangan, dan
> sebagian isinya bertentangan dengan keputusan yang sudah dikunci di sini maupun di modul
> lain. Pertentangan itu diselesaikan pada bagian ini, **bukan** dengan menganggap artifact
> otomatis menang dan **bukan** dengan membuang artifact.

### Yang ternyata bukan pertentangan

Pembacaan ulang `BR-25` (`LAB-DEC-029`) menunjukkan satu dugaan pertentangan gugur:

| Isi artifact | Keadaan sebenarnya |
|---|---|
| Layar menghitung dan menampilkan Subtotal serta Grand Total | **Sudah diizinkan.** `BR-25` menyebut Laboratorium boleh menampilkan "harga satuan, jumlah, subtotal, dan total saat memesan", dan `AC-43` menegaskannya |
| `Cito` tidak mengubah harga | **Tidak bertentangan.** `LAB-DEC-026` menaruh cito pada baris pemeriksaan sebagai penanda urutan kerja, bukan pengubah tarif |

Yang tetap dilarang oleh `BR-25` hanya dua: **memutuskan apakah pasien membayar**, dan
**membentuk tagihan**. Jadi kalkulasi di layar aman; penentuan metode bayar dan penerimaan
uang tidak.

**Celah yang justru terbongkar.** `BR-25` sudah menyebut kata **"jumlah"** sejak 2026-09-01,
tetapi `LabExamination@466a7127` tidak punya kolom Qty sama sekali — yang ada hanya
`IsDuplo`. Keputusan dan model tidak sejalan. Dicatat sebagai `LAB-GAP-001`.

### Batas scope amendment ini (dikonfirmasi pemilik modul 2026-09-14)

**Di dalam scope — milik Laboratorium**

| No | Butir yang diamandemen | Asal di `LAB-EVD-001` | Sifat |
|---:|---|---|---|
| 1 | Jenis specimen menjadi pilihan terkendali, menggantikan `SpecimenDescription` teks bebas | `CAP-009` | Tambahan data |
| 2 | Volume specimen menjadi kolom tersendiri | `CAP-009`, `RULE-021` | Tambahan data |
| 3 | ~~Qty pada baris pemeriksaan~~ — **dicabut `LAB-DEC-050`** 2026-09-14; kolom Jumlah tidak dibuat | `CAP-011`, `RULE-025` | ~~Menutup celah~~ |
| 4 | Penguncian **penambahan baris** setelah penetapan kelayakan; pembatalan tidak ikut terkunci (`LAB-DEC-049`) | `CAP-012`, `RULE-026` | Aturan baru |
| 5 | Tanggal Penerimaan Specimen dapat diisi petugas, tidak hanya jam server | `RULE-019` bagian specimen | Aturan baru |
| 6 | Bentuk layar: satu menu gabungan atau tetap tiga layar terpisah | seluruh §5 artifact | Kewenangan UI |
| 7 | Titik sentuh ke Billing: apa yang dikirim agar Billing tahu perujuknya PKS atau bukan | `CAP-007` | Kontrak antarmodul |
| 8 | Titik sentuh ke Master Data: apa yang dilakukan petugas saat RS perujuk belum ada di daftar | `CAP-008`, `RULE-014` | Kontrak antarmodul |

**Di luar scope — diserahkan ke modul pemiliknya**

| Butir `LAB-EVD-001` | Pemilik | Keputusan yang menahan |
|---|---|---|
| Petugas lab mendaftarkan RS perujuk baru sebagai data induk | `master-data` | `LAB-DEC-035` butir 4; `AC-50`; `VAL-43` |
| Menentukan `Piutang Mitra`/`Tunai` dan menerima uang tunai | `billing-kasir` | `RJ-BIL-GATE-DEC-003`; `LAB-DEC-029`; `AC-43` |
| Promo dan pengaruhnya pada Grand Total | `billing-kasir` / `master-data` | `LAB-DEC-033` |
| Status PKS rumah sakit perujuk | `insurance-management` / `master-data` | `LAB-DEC-034` |
| Menyimpan pasien baru, mengubah Tanggal Registrasi, Kategori Pasien dan Title dari usia | `registration-management` | `LAB-DEC-032` butir 5; `AC-45` |
| Pemindaian KTP dan barcode No. RM sebagai kewajiban di **seluruh** titik pendaftaran RS | `registration-management` / platform | Lebih besar dari modul ini; belum pernah diputuskan |

> **Yang perlu disadari dari pembagian ini.** Menu Penerimaan Sampling/Specimen **tidak akan
> bisa** menerima uang tunai maupun menambah RS perujuk baru dari layarnya sendiri. Keduanya
> bukan penyederhanaan teknis, melainkan konsekuensi langsung dari keputusan yang sudah
> disetujui: satu sumber harga, satu daftar perujuk terkendali, dan wewenang uang yang tidak
> dipegang laboratorium.

---

## Glossary

| Istilah | Arti dalam bahasa sehari-hari |
|---|---|
| *Lab order* / pesanan lab | Permintaan resmi dari dokter agar pasien diperiksa laboratorium |
| Spesimen / sampel | Bahan yang diambil dari pasien untuk diperiksa, misalnya darah, urin, dahak |
| *Barcode* spesimen | Label unik yang ditempel di tabung sampel agar tidak tertukar |
| *Received* (diterima) | Sampel sudah sampai secara fisik di laboratorium |
| *Accepted* (dinyatakan layak) | Sampel sudah diperiksa kelayakannya dan dinyatakan boleh dikerjakan |
| *Rejected* (ditolak) | Sampel tidak layak diperiksa, misalnya darah menggumpal atau jumlahnya kurang |
| *Recollection* | Pengambilan ulang sampel karena sampel sebelumnya tidak bisa dipakai |
| *Validated* (divalidasi) | Hasil sudah diperiksa kebenarannya oleh petugas yang berwenang |
| *Released* (dirilis) | Hasil sudah boleh dibaca dokter dan pasien |
| Nilai kritis | Hasil yang menandakan bahaya dan wajib dilaporkan segera, misalnya kalium sangat tinggi |
| Nilai rujukan | Batas normal suatu pemeriksaan, misalnya hemoglobin dewasa pria 13–17 g/dL |
| *Turnaround time* (TAT) | Lama waktu dari sampel diterima sampai hasil keluar |
| *Charge eligibility* | Titik saat suatu pemeriksaan sudah sah untuk ditagihkan |

---

## Fakta dari Source Code (bukti, bukan keputusan)

Seluruh fakta di bawah dibaca langsung dari backend pada SHA `c87d9c0`.

### F1 — Modul Laboratorium backend sudah ada sebagian

Folder `Areas/HealthServices/LaboratoryManagement` berisi 12 berkas: 2 controller, 2 service,
4 model, 1 enum, 2 DTO, dan 1 konfigurasi Entity Framework.

Model yang sudah ada:

| Model | Isi pokok |
|---|---|
| `LabOrder` | Pesanan lab; punya `EncounterId`, `ProcedureId`, `OrderStatus`, `StatusBeforeHold`, `Version` |
| `TrxLabSpecimen` | Sampel; punya `SpecimenBarcode`, `SpecimenSequence`, salinan tarif, jejak siapa mengambil/menerima/memutuskan, sebab ambil ulang, dan tautan ke sampel yang digantikan |
| `MstLabRejectionReason` | Daftar alasan penolakan sampel yang terkendali |
| `TrxLabTransitionHistory` | Riwayat perpindahan status yang tidak bisa diubah |

### F2 — Status yang sudah dikodekan

| Enum | Nilai |
|---|---|
| `LabOrderStatus` | `Draft`, `Requested`, `Accepted`, `InProcess`, `Completed`, `OnHold`, `CancelRequested`, `Cancelled` |
| `LabSpecimenStatus` | `Planned`, `Collected`, `Received`, `Accepted`, `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold` |
| `LabRecollectionCause` | `InternalHospitalError`, `PatientOrSpecimenCondition`, `ExternalCause` |
| `LabTransitionScope` | `LabOrder`, `LabSpecimen` |

### F3 — Endpoint yang sudah tersedia

#### `[Tags("Health Services / Laboratory Management / Lab Order")]`

Base route: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Ringkasan |
|---|---|---|
| `GET` | `/` | Menampilkan daftar pesanan lab |
| `GET` | `/{id}` | Menampilkan satu pesanan lab |
| `POST` | `/` | Membuat pesanan lab baru |
| `PUT` | `/{id}/start-process` | Menandai pesanan mulai dikerjakan |
| `PUT` | `/{id}/complete` | Menandai pesanan selesai |
| `PUT` | `/{id}/hold` | Menahan sementara pesanan |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan yang ditahan |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan |

#### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

Base route: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Ringkasan |
|---|---|---|
| `GET` | `/rejection-reasons` | Daftar alasan penolakan sampel |
| `GET` | `/by-order/{labOrderId}` | Daftar sampel milik satu pesanan |
| `GET` | `/by-order/{labOrderId}/history` | Riwayat perpindahan status satu pesanan |
| `POST` | `/by-order/{labOrderId}` | Menambah sampel pada pesanan |
| `POST` | `/{id}/collect` | Mencatat pengambilan sampel |
| `POST` | `/{id}/receive` | Mencatat sampel diterima di lab |
| `POST` | `/{id}/accept` | Menyatakan sampel layak diperiksa |
| `POST` | `/{id}/reject` | Menolak sampel |
| `POST` | `/{id}/request-recollection` | Meminta pengambilan ulang sampel |
| `POST` | `/{id}/hold` | Menahan sementara sampel |
| `POST` | `/{id}/resume` | Melanjutkan sampel yang ditahan |
| `POST` | `/{id}/cancel` | Membatalkan sampel |

### F4 — Yang belum ada sama sekali di backend

| Kemampuan | Bukti |
|---|---|
| Hasil pemeriksaan (`LabResult`) | Tidak ada model, service, controller, maupun enum status hasil |
| Nilai kritis dan notifikasinya | Tidak ada |
| Nilai rujukan normal | Tidak ada tabel penyimpan batas normal |
| Katalog pemeriksaan lab khusus | Tidak ada. Yang dipakai adalah `MstProcedure` dengan penanda `IsLaboratory` |
| Paket/panel pemeriksaan | Tidak ada |
| Daftar kerja (*worklist*) petugas lab | Tidak ada endpoint khusus |
| Integrasi alat laboratorium | Tidak ada |
| Pengukuran waktu penyelesaian (TAT) | Tidak ada |

### F5 — Frontend belum punya modul Laboratorium sama sekali

Pada SHA `c79bb6ee4`, folder `src/components/view/health-services/` berisi
`billing-management`, `emergency-installation-management`, `inpatient-management`,
`master-data`, `medical-record-management`, `operating-room-management`,
`patient-management`, `pharmacy-management`, dan `registration-management`.
**Tidak ada** `laboratory-management`. Tidak ada pula route `src/app/health-services/laboratory-*`.
Artinya seluruh tampilan Laboratorium harus dibangun dari nol.

### F6 — Katalog pemeriksaan lab saat ini menumpang pada `MstProcedure`

`MstProcedure` memiliki kolom penanda `IsLaboratory`. Tidak ada kolom untuk jenis sampel,
wadah, volume minimal, metode, satuan hasil, atau nilai rujukan. Semua atribut khas
laboratorium belum punya tempat.

### F7 — Platform belum punya sarana notifikasi umum

Diperiksa pada 2026-09-01 untuk menjawab `LAB-OPEN-008`.

| Yang dicari | Hasil |
|---|---|
| Layanan notifikasi umum (`INotificationService` atau sejenisnya) | **Tidak ada** |
| Tabel notifikasi tersimpan (kotak masuk pemberitahuan per pengguna) | **Tidak ada** |
| Surel (SMTP/MailKit) | **Tidak ada** |
| Pesan singkat atau WhatsApp (Twilio, Fonnte, dan sejenisnya) | **Tidak ada** |
| Sarana realtime | **Ada, tetapi khusus antrean.** `Hubs/QueueHub.cs` dipetakan ke `/hubs/queues` lewat SignalR, dan pengelompokannya per *nurse station cluster* untuk kebutuhan antrean nurse station serta doctor queue |

Artinya: teknologi realtime sudah terpasang dan terbukti jalan di produksi, tetapi **belum ada
pemberitahuan yang tersimpan**. Pemberitahuan lewat SignalR saja hanya sampai kepada dokter
yang kebetulan sedang membuka aplikasi. Dokter yang sedang menutup aplikasi tidak akan pernah
tahu ada nilai kritis. Fakta ini menjadi dasar pilihan pada `LAB-OPEN-008`.

---

## Keputusan yang Sudah Dikunci di Modul Lain (diwarisi, tidak dibuka ulang)

Sumber: `docs/module-blueprints/rawat-jalan/00-interview-decisions.md`,
amendment `RJ-BIL-GATE-DEC-003` tanggal 2026-08-19, status `locked-draft`,
bukti approval SHA-256 `4d44472028622f6a9c460f78c4ff61c6fa29d57b6ce5f46f8c6d2b820373c2cc`.

> Status tata kelola formal amendment itu masih `OPEN`: tanda tangan Laboratorium, Clinical
> Governance, dan Billing/Finance belum dilampirkan.

| Kode | Isi yang sudah dikunci |
|---|---|
| `LAB-INH-001` | Alur pesanan: `Draft → Requested → Accepted → InProcess → Completed`, dengan pengecualian `OnHold`, `CancelRequested`, `Cancelled` |
| `LAB-INH-002` | Alur sampel: `Planned → Collected → Received → Accepted`, dengan pengecualian `Rejected`, `RecollectionRequired`, `Cancelled`, `OnHold` |
| `LAB-INH-003` | Alur hasil: `Pending → InProcess → Completed → Validated → Released`; koreksi lewat `Corrected/Amended → Revalidated → Released` dan tetap menyimpan riwayat rilis lama |
| `LAB-INH-004` | `Validated` dan `Released` adalah status **hasil**, bukan status pesanan, dan bukan pemicu awal penagihan |
| `LAB-INH-005` | Satu pesanan boleh punya banyak sampel. Ambil ulang membuat identitas sampel **baru** dan menyimpan sampel lama beserta tautan sebabnya |
| `LAB-INH-006` | Dokter boleh mengubah langsung hanya sampai status `Requested`. Setelah itu dokter hanya melihat, menambah keterangan klinis, atau **mengajukan** pembatalan/koreksi |
| `LAB-INH-007` | Pengambilan, penerimaan, penerimaan/penolakan, pemrosesan, validasi, dan rilis memakai kewenangan yang berbeda-beda. Jabatan tidak otomatis memberi kewenangan |
| `LAB-INH-008` | "Sampel sampai di lab" (`Received`) **tidak sama dengan** "sampel dinyatakan layak" (`Accepted`) |
| `LAB-INH-009` | Titik sah untuk ditagihkan adalah `Accepted`. `Requested`, `Collected`, dan `Received` **bukan** pemicu tagihan pemeriksaan |
| `LAB-INH-010` | Laboratorium hanya mengirim fakta klinis dan pemakaian bahan. Billing adalah satu-satunya pemilik akibat finansial |
| `LAB-INH-011` | Ambil ulang karena kesalahan internal rumah sakit **tidak boleh** otomatis menambah tanggungan pasien |
| `LAB-INH-012` | Laboratorium tidak punya `Paid`, penyelesaian pembayaran, persetujuan penjamin, void, refund, maupun pembalikan transaksi |
| `LAB-INH-013` | Setiap perpindahan status yang penting menghasilkan riwayat yang tidak bisa diubah, memuat identitas, status asal/tujuan, tindakan, alasan, pelaku, waktu, dan ID korelasi |

**Contoh penerapan `LAB-INH-009` dan `LAB-INH-011` agar jelas:**

> Pasien Budi dipesankan pemeriksaan Hemoglobin. Perawat mengambil darah pukul 08.00
> (`Collected`), sampel sampai di lab pukul 08.20 (`Received`), lalu petugas lab memeriksa
> kelayakannya dan menyatakan layak pukul 08.25 (`Accepted`). **Baru pada pukul 08.25** itu
> pemeriksaan Hemoglobin sah untuk ditagihkan.
>
> Bila ternyata pukul 08.30 tabung sampel pecah karena kelalaian petugas lab, sampel harus
> diambil ulang dengan sebab `InternalHospitalError`. Pemeriksaan Hemoglobin **tetap satu
> kali** ditagihkan kepada Budi. Biaya pengambilan ulang menjadi tanggungan rumah sakit,
> bukan Budi.

---

## Aktor dan Tanggung Jawab

Daftar sementara, menunggu konfirmasi pemilik proses.

| Aktor | Tanggung jawab yang diperkirakan | Status |
|---|---|---|
| Dokter pemesan | Membuat pesanan lab, membaca hasil, mengajukan pembatalan | `assumption` |
| Perawat / flebotomis | Mengambil sampel dari pasien | `assumption` |
| Petugas penerimaan lab | Menerima sampel, memutuskan layak atau ditolak | `assumption` |
| Analis laboratorium | Mengerjakan pemeriksaan dan mengisi hasil | `assumption` |
| Penanggung jawab teknis / dokter patologi klinik | Memvalidasi dan merilis hasil | `assumption` |
| Kepala instalasi laboratorium | Menyetujui pengecualian dan kebijakan lokal | `assumption` |
| Petugas Billing | Menerima fakta dari Laboratorium dan menentukan tagihan | `fact` (`LAB-INH-010`) |

---

## Business Rules dan Invariants

Selain warisan `LAB-INH-001` sampai `LAB-INH-013`, sesi ini mengunci aturan berikut.

### BR-01 — Prinsip empat mata pada validasi hasil (`LAB-DEC-003`)

**Aturan:** Petugas yang mengetik angka hasil **tidak boleh** menjadi petugas yang
memvalidasi dan merilis hasil yang sama. Sistem menolak percobaan itu pada keadaan normal.

**Jalur pengecualian:** Bila keadaan memaksa, misalnya shift malam hanya ada satu analis,
sistem tetap mengizinkan dengan tiga syarat wajib:

1. Petugas mengisi alasan pengecualian dari daftar alasan yang terkendali.
2. Hasil diberi penanda permanen "divalidasi oleh pengisi sendiri".
3. Penanda itu ikut tercetak pada lembar hasil dan tersimpan di riwayat yang tidak bisa diubah.

**Contoh:**

> Analis Sari bertugas malam sendirian. Pukul 23.40 ia mengetik hasil Natrium pasien Andi
> sebesar 128 mmol/L. Ia menekan tombol Validasi. Sistem menahan dan menampilkan pesan:
> "Anda yang mengisi hasil ini. Validasi oleh orang yang sama memerlukan alasan."
> Sari memilih alasan "Shift tunggal, tidak ada validator lain bertugas" dan menekan Lanjut.
> Hasil dirilis pukul 23.42, dan pada lembar hasil tercetak keterangan
> "Divalidasi oleh pengisi sendiri — Sari — Shift tunggal". Keesokan paginya kepala
> instalasi melihat hasil ini di daftar pantau pengecualian.

> **⚠ Catatan 2026-09-24 — aturan BR-01 tidak berubah, contohnya berubah.** Sejak `LAB-DEC-150`
> hanya dokter berkewenangan laboratorium yang memvalidasi, sehingga analis Sari **tidak dapat**
> memvalidasi hasil apa pun. Pada Patologi Klinik pengisi (analis) dan pemvalidasi (dokter)
> selalu dua orang berbeda; jalur pengecualian ini tetap berlaku bagi disiplin yang pengisinya
> dokter, misalnya Patologi Anatomi. Akibatnya bagi shift malam dibaca pada BR-101.

**Konsekuensi bila dilanggar:** Kesalahan ketik satu digit pada Kalium — misalnya 3,5 diketik
menjadi 7,5 — bisa membuat dokter memberikan terapi yang salah. Karena itu aturan ini
diperlakukan sebagai invariant keselamatan, bukan sekadar preferensi.

### BR-02 — Nilai kritis wajib dilaporkan dan dicatat (`LAB-DEC-004`)

**Aturan:** Hasil bernilai kritis **tetap dirilis** agar dokter segera melihatnya. Namun
pemeriksaan itu **belum dianggap tuntas** sampai catatan pelaporan terisi lengkap:

| Isian wajib | Contoh |
|---|---|
| Siapa yang melapor | Analis Sari |
| Kepada siapa | dr. Rina, DPJP pasien Andi |
| Kapan | 2026-09-01 pukul 23.45 |
| Lewat apa | Telepon |
| Bukti pembacaan ulang | dr. Rina mengulang "Kalium tujuh koma dua", dicentang Sari |

**Aturan turunan:** Sistem menyediakan daftar pantau berisi hasil kritis yang **belum**
dilaporkan, agar kepala instalasi bisa menegur sebelum menjadi insiden.

**Contoh:**

> Hasil Kalium pasien Andi keluar 7,2 mmol/L pukul 23.44, sedangkan batas kritis atas yang
> ditetapkan adalah 6,0 mmol/L. Sistem langsung merilis hasil sehingga dr. Rina bisa
> membukanya, sekaligus memunculkan formulir pelaporan yang harus diisi Sari. Selama formulir
> itu kosong, pemeriksaan Kalium tetap muncul di daftar "nilai kritis belum dilaporkan".

**Alasan pilihan ini:** Menahan hasil kritis justru memperlambat penanganan pasien, sedangkan
pelaporan lisan tanpa catatan tidak bisa dibuktikan saat ditelusuri auditor atau saat terjadi
sengketa.

### BR-03 — Hasil Rilis 1 diketik manual (`LAB-DEC-005`)

**Aturan:** Pada Rilis 1, seluruh angka hasil diketik oleh analis. Tidak ada sambungan
otomatis ke alat laboratorium.

**Risiko yang harus disadari:** Pengguna memilih pengetikan manual **tanpa** menyiapkan tempat
data untuk hasil yang dikirim alat. Artinya sistem Rilis 1 tidak menyimpan asal hasil, nomor
seri alat, maupun waktu kirim alat. Bila kelak alat disambungkan, struktur data hasil harus
diubah dan riwayat hasil lama tidak akan bisa membedakan mana yang diketik orang dan mana yang
dikirim alat. Risiko ini dicatat sebagai `LAB-RISK-001`.

**Pengaman yang sudah ada:** Risiko salah ketik ditutup oleh BR-01 (prinsip empat mata).

### BR-04 — Batas nilai disimpan sebagai data induk sejak Rilis 1 (`LAB-DEC-006`)

**Latar belakang conflict:** `LAB-DEC-001` menunda katalog lab mandiri ke Rilis 2, sedangkan
`LAB-DEC-004` mewajibkan sistem mengenali nilai kritis. `MstProcedure` tidak punya kolom
satuan, batas normal, maupun batas kritis (F6). Tanpa penyelesaian, Rilis 1 tidak akan pernah
tahu bahwa Kalium 7,2 mmol/L itu berbahaya.

**Aturan:** Rilis 1 tetap mendapat **satu tabel batas nilai** per jenis pemeriksaan. Isinya
hanya yang benar-benar dibutuhkan untuk menilai hasil:

| Isian | Contoh Kalium | Contoh Hemoglobin |
|---|---|---|
| Satuan hasil | mmol/L | g/dL |
| Batas normal bawah | 3,5 | 13,0 (pria dewasa) |
| Batas normal atas | 5,1 | 17,0 (pria dewasa) |
| Batas kritis bawah | 2,5 | 7,0 |
| Batas kritis atas | 6,0 | 20,0 |
| Pembeda jenis kelamin | Tidak | Ya |
| Pembeda kelompok umur | Tidak | Ya |

**Yang tetap ditunda ke Rilis 2:** jenis sampel, jenis wadah/tabung, volume minimal, metode
pemeriksaan, dan paket/panel.

**Contoh:**

> Analis Sari mengetik hasil Kalium 7,2 mmol/L. Sistem membandingkannya dengan tabel batas
> nilai: 7,2 lebih besar dari batas kritis atas 6,0. Sistem langsung menandai hasil sebagai
> nilai kritis dan memunculkan formulir pelaporan BR-02. Bila hasilnya 5,3 — di atas normal
> tetapi belum kritis — sistem hanya menandainya "di atas nilai rujukan" tanpa formulir
> pelaporan.

### BR-05 — Koreksi hasil setelah rilis (`LAB-DEC-007`)

**Aturan:**

1. Koreksi hasil yang sudah dirilis hanya boleh dilakukan petugas yang punya kewenangan
   validasi atau rilis. Analis biasa tidak boleh.
2. Begitu hasil perbaikan dirilis ulang, dokter pemesan **otomatis** mendapat pemberitahuan
   bahwa hasil pasiennya berubah.
3. Versi hasil yang lama **tetap terlihat**, diberi tanda "sudah diperbaiki", dan tidak
   dihapus. Ini melanjutkan `LAB-INH-003`.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis 9,4 g/dL pukul 10.00. Pukul 14.00 ketahuan angka yang
> benar adalah 4,9 g/dL karena tertukar dengan sampel lain. Tono, yang berwenang validasi,
> membuat koreksi. Sistem menyimpan versi lama 9,4 dengan tanda "sudah diperbaiki", merilis
> versi baru 4,9, lalu mengirim pemberitahuan ke dr. Rina. dr. Rina yang tadinya menganggap
> pasien tidak perlu transfusi kini tahu keadaan sebenarnya.

**Alasan pilihan ini:** Dokter mungkin sudah terlanjur memberi terapi berdasarkan angka yang
salah. Tanpa pemberitahuan, koreksi tidak ada gunanya bagi pasien.

### BR-06 — Hasil boleh dirilis sebagian (`LAB-DEC-008`)

**Aturan:** Setiap pemeriksaan dalam satu pesanan punya status hasilnya sendiri dan dirilis
begitu selesai divalidasi, tanpa menunggu pemeriksaan lain. Lembar hasil **wajib** menandai
mana yang sudah keluar dan mana yang masih diproses.

**Contoh:**

> dr. Rina memesan Hemoglobin, Leukosit, dan Kalium sekaligus untuk pasien Andi. Kalium selesai
> pukul 09.10 dengan nilai kritis 7,2 mmol/L, sedangkan Hemoglobin dan Leukosit baru selesai
> pukul 10.30. Kalium dirilis pukul 09.12 sehingga dr. Rina bisa langsung bertindak. Lembar
> hasil saat itu menampilkan Kalium 7,2 (kritis) dan keterangan "Hemoglobin: masih diproses,
> Leukosit: masih diproses".

**Aturan turunan yang wajib:** Selama masih ada pemeriksaan yang belum keluar, lembar hasil
harus memuat peringatan "hasil belum lengkap" agar dokter tidak salah mengira pemeriksaan
sudah tuntas.

### BR-07 — Laboratorium melayani seluruh unit sejak Rilis 1 (`LAB-DEC-009`)

**Aturan:** Modul Laboratorium menerima pesanan dari Rawat Jalan, Rawat Inap, dan IGD sekaligus
sejak Rilis 1. Tidak ada pembedaan alur kerja berdasarkan unit asal.

**Dasar teknis:** `LabOrder` sudah memakai `EncounterId` yang mengacu ke `TrxPatientEncounter`
umum, sehingga tidak ada penghalang di sisi data (F1).

**Aturan turunan yang wajib:** Karena IGD ikut dilayani, pesanan harus punya **penanda tingkat
kesegeraan** — sekurang-kurangnya "biasa" dan "cito/segera". Pesanan cito tidak boleh mengantre
di belakang pesanan rawat jalan rutin pada daftar kerja petugas.

**Contoh:**

> Pukul 10.00 ada 14 pesanan rawat jalan rutin menunggu dikerjakan. Pukul 10.05 masuk pesanan
> Kalium cito dari IGD untuk pasien tidak sadar. Pesanan IGD itu harus muncul di urutan paling
> atas daftar kerja analis, bukan di urutan ke-15.

Definisi resmi "cito" dan batas waktunya ditutup oleh BR-09.

### BR-08 — Pemberitahuan disimpan, bukan sekadar dikirim (`LAB-DEC-012`)

**Latar belakang:** `LAB-DEC-004` mewajibkan dokter tahu ada nilai kritis, dan `LAB-DEC-007`
mewajibkan dokter tahu hasilnya dikoreksi. Fakta F7 menunjukkan platform belum punya sarana
apa pun untuk itu, kecuali SignalR khusus antrean.

**Aturan:**

1. Setiap pemberitahuan disimpan sebagai baris data milik dokter tujuan, bukan sekadar dikirim
   sekilas lalu hilang.
2. Setiap pemberitahuan menyimpan sekurang-kurangnya: dokter tujuan, jenis pemberitahuan
   (nilai kritis atau koreksi hasil), pesanan dan pemeriksaan yang dimaksud, pasien, waktu
   dibuat, serta status sudah dibaca atau belum beserta waktu dibacanya.
3. Pemberitahuan muncul di kotak pemberitahuan dokter saat ia membuka aplikasi, tanpa
   memandang apakah ia sedang online ketika pemberitahuan itu dibuat.
4. SignalR dipakai sebagai pelengkap agar pemberitahuan muncul seketika bila dokter kebetulan
   sedang membuka aplikasi. SignalR **bukan** satu-satunya jalur.

**Contoh:**

> Hasil Kalium pasien Andi keluar 7,2 mmol/L pukul 23.44. dr. Rina sedang tidak membuka
> aplikasi. Pemberitahuan tetap tersimpan. Pukul 05.30 dr. Rina membuka aplikasi dan langsung
> melihat satu pemberitahuan belum dibaca: "Nilai kritis — Kalium 7,2 mmol/L — pasien Andi —
> 2026-09-01 23.44". Sistem mencatat pemberitahuan itu dibaca pukul 05.31.
>
> Bandingkan bila hanya memakai SignalR: pemberitahuan pukul 23.44 itu hilang begitu saja, dan
> rumah sakit tidak punya bukti apa pun bahwa dokter pernah dikabari.

**Hubungan dengan BR-02:** Pemberitahuan otomatis ini **tidak menggantikan** kewajiban
pelaporan lisan pada BR-02. Keduanya berjalan bersama: pelaporan lisan untuk penanganan
segera, pemberitahuan tersimpan sebagai bukti dan jaring pengaman.

### BR-09 — Aturan cito dan batas waktu penyelesaian (`LAB-DEC-013`)

**Aturan:**

1. Dokter pemesan menandai sendiri pesanan sebagai cito ketika membuat pesanan. Tidak ada
   penandaan otomatis berdasarkan unit asal.
2. Setiap jenis pemeriksaan punya **batas waktu penyelesaian cito** sendiri, disimpan bersama
   tabel batas nilai pada `LAB-DEC-006`.
3. Batas waktu dihitung sejak sampel dinyatakan layak (`Accepted`) sampai hasil dirilis
   (`Released`).
4. Sistem menampilkan pesanan cito yang sudah melewati batas waktunya sebagai daftar pantau
   tersendiri.

**Contoh:**

| Pemeriksaan | Batas waktu cito | Contoh perhitungan |
|---|---|---|
| Kalium | 60 menit | Sampel layak pukul 09.00, hasil dirilis pukul 09.45 → **memenuhi**, sisa 15 menit |
| Kalium | 60 menit | Sampel layak pukul 09.00, hasil dirilis pukul 10.20 → **lewat batas** 20 menit, masuk daftar pantau |
| Hemoglobin | 45 menit | Sampel layak pukul 14.00, belum dirilis sampai pukul 14.50 → **lewat batas**, muncul di daftar pantau meski hasil belum keluar |

**Alasan pilihan ini:** Dokter yang paling tahu kondisi pasiennya, sehingga penandaan cito
diserahkan kepadanya. Namun tanpa batas waktu yang terukur, "cito" hanya menjadi label — dan
lama-lama semua dokter menandai cito sehingga prioritas kehilangan arti. Batas waktu membuat
kepatuhan bisa dibuktikan.

### BR-10 — Reagen bukan urusan Laboratorium (`LAB-DEC-014`)

**Aturan:** Modul Laboratorium **tidak** mengelola stok, pembelian, maupun pencatatan
pemakaian reagen. Seluruhnya menjadi urusan modul Farmasi atau Inventory.

**Akibat yang harus disadari:** Laporan biaya bahan per pemeriksaan belum akan tersedia sampai
modul Inventory menanganinya. Bila kelak Billing membutuhkan angka pemakaian bahan nyata untuk
Actual Consumption Rule, kebutuhan itu harus diajukan ke modul Inventory, bukan ditambahkan ke
Laboratorium.

### BR-11 — Ruang koreksi dokter tanpa tahap draf (`LAB-DEC-015`)

**Latar belakang conflict `CONF-01`.** Capability map menemukan pertentangan: keputusan warisan
`LAB-INH-001` menyebut pesanan dimulai dari status `Draft`, tetapi kode tidak pernah membuat
status itu. Bukti: `Services/LabOrderService.cs:136@c87d9c0` selalu menetapkan `Requested` saat
pesanan dibuat, dan tidak ada satu pun kode yang menetapkan `Draft`. Akibatnya `LAB-INH-006` —
yang memberi dokter ruang menyunting "sampai `Requested`" — menjadi kosong, karena pesanan
sudah `Requested` sejak detik pertama.

**Aturan yang menutup conflict ini:**

1. Status `Draft` **dihapus** dari siklus hidup pesanan laboratorium. Pesanan tetap dibuat
   langsung berstatus `Requested`, persis seperti perilaku kode saat ini.
2. Dokter pemesan **boleh menyunting** pesanannya — menambah, mengurangi, atau mengganti
   pemeriksaan — **selama belum ada satu pun sampel yang diambil** pada pesanan itu.
3. Begitu sampel pertama berpindah ke status `Collected`, seluruh pesanan terkunci bagi dokter.
   Sejak titik itu dokter hanya boleh melihat, menambah keterangan klinis, atau **mengajukan**
   pembatalan sesuai `LAB-INH-006`.
4. Setiap penyuntingan pesanan tetap menghasilkan baris riwayat yang tidak bisa diubah, sama
   seperti perpindahan status lain.

**Contoh:**

> dr. Rina memesan Hemoglobin, Leukosit, dan Kalium untuk pasien Andi pukul 08.00. Pukul 08.03
> ia sadar seharusnya Natrium, bukan Kalium. Perawat belum mengambil darah. dr. Rina mengganti
> Kalium menjadi Natrium, dan sistem mengizinkannya sambil mencatat perubahan itu di riwayat.
>
> Bandingkan: pukul 08.10 perawat Dewi sudah mengambil darah dan sampel berstatus `Collected`.
> Pukul 08.12 dr. Rina ingin menambah pemeriksaan Trombosit. Sistem menolak penyuntingan.
> dr. Rina harus membuat pesanan baru, atau mengajukan pembatalan bila memang seluruh pesanan
> keliru.

**Kenapa batas "belum ada sampel diambil" yang dipilih.** Batas ini jelas, mudah diperiksa
mesin, dan masuk akal secara nyata: begitu darah pasien sudah diambil, mengubah daftar
pemeriksaan berarti tabung yang sudah terisi bisa tidak cocok lagi dengan yang dipesan.

**Akibat ke blueprint lain.** `LAB-INH-001` dan `LAB-INH-006` diwarisi dari
`RJ-BIL-GATE-DEC-003` milik blueprint `rawat-jalan`. Keduanya perlu diamandemen di sana:
`Draft` dihapus dari siklus hidup, dan batas kewenangan dokter diubah dari "sampai `Requested`"
menjadi "sampai sampel pertama diambil". Dicatat sebagai `LAB-AMD-001`.

### BR-12 — Pemberitahuan adalah kemampuan platform, bukan milik Laboratorium (`LAB-DEC-016`)

**Latar belakang.** Capability map `CAP-18` membuktikan platform belum punya sarana
pemberitahuan apa pun: tidak ada tabel penyimpan pemberitahuan, tidak ada surel, tidak ada
pesan singkat. Yang ada hanya `Hubs/QueueHub.cs@c87d9c0` yang khusus melayani antrean.

**Aturan:**

1. Pemberitahuan tersimpan dibangun sebagai **kemampuan platform bersama**, bukan milik modul
   Laboratorium. Modul mana pun boleh memakainya: Farmasi, Radiologi, Billing, dan lainnya.
2. Laboratorium menjadi **pemakai pertama** yang membuktikan bentuknya, dengan dua jenis
   pemberitahuan: nilai kritis dan koreksi hasil.
3. Dokter memiliki **satu kotak masuk** untuk seluruh pemberitahuan dari semua modul, bukan
   satu kotak per modul.
4. Bentuk data pemberitahuan harus cukup umum sejak awal: pengguna tujuan, jenis, judul, isi,
   penunjuk ke data sumber, waktu dibuat, status sudah dibaca, dan waktu dibaca. Istilah khas
   laboratorium tidak boleh masuk ke struktur umum itu.

**Contoh kenapa ini penting.**

> Bila pemberitahuan dibangun khusus Laboratorium, lalu Farmasi membangun versinya sendiri, dan
> Radiologi membangun versinya sendiri lagi, maka dr. Rina harus memeriksa tiga tempat berbeda
> untuk tahu apakah ada hal mendesak. Untuk pemberitahuan biasa itu merepotkan; untuk nilai
> kritis itu berbahaya.

**Akibat pada pelaksanaan.** Karena kemampuan ini milik platform, pembangunannya memerlukan
kesepakatan dengan pemilik platform sebelum masuk roadmap Laboratorium. Dicatat sebagai
`LAB-COORD-001`.

### BR-13 — Hasil lab terdaftar sebagai dokumen klinis di rekam medis (`LAB-DEC-017`)

**Latar belakang.** Modul `rekam-medis` tidak menyimpan isi dokumen klinis. Ia mencatat
**keutuhan** dokumen milik modul lain lewat `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs@c87d9c0`,
yang menunjuk dokumen memakai pasangan `DocumentKind` dan `DocumentId`. Daftar jenis dokumen
pada `ClinicalDocumentKind@c87d9c0` berisi 13 nilai — `ProgressNote`, `Consultation`,
`Assessment`, `Diagnosis`, `Procedure`, `VitalSign`, `Allergy`, `MedicalHistory`,
`FamilyHistory`, `ClinicalDocument`, `NoteAttachment`, `MedicalCertificate`, dan `Consent` —
dan **tidak ada** nilai untuk hasil laboratorium.

**Aturan:**

1. Isi hasil pemeriksaan **tetap disimpan di tabel Laboratorium**. Tidak ada penggandaan angka
   hasil ke tabel rekam medis.
2. Setiap hasil yang dirilis **didaftarkan** ke rekam medis sebagai jenis dokumen klinis baru,
   sehingga ikut memiliki catatan penulis, waktu penandatanganan, dan status penguncian.
3. Rekam medis menjadi tempat menelusuri "dokumen apa saja yang dimiliki pasien ini", termasuk
   hasil laboratorium.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis pukul 10.00 oleh Tono. Angka 9,4 g/dL tersimpan di tabel
> hasil milik Laboratorium. Bersamaan dengan itu, rekam medis mencatat satu baris keutuhan:
> jenis dokumen "hasil laboratorium", penunjuk ke hasil tersebut, penulis Tono, ditandatangani
> pukul 10.00. Ketika petugas rekam medis menelusuri berkas pasien Andi, hasil lab itu ikut
> terlihat sebagai bagian berkasnya, tanpa angka 9,4 pernah disalin ke mana pun.

**Titik yang harus dicocokkan dan belum diputuskan.** Rekam medis mengunci dokumen ketika
kunjungan pasien ditutup, lihat `ClinicalDocumentLockTrigger.EncounterClosed@c87d9c0`.
Sementara `LAB-DEC-007` mengizinkan koreksi hasil setelah dirilis. Kedua aturan ini bertemu
ketika hasil perlu dikoreksi **setelah** kunjungan pasien ditutup. Perilaku yang benar untuk
keadaan itu belum diputuskan dan dicatat sebagai `LAB-OPEN-011`.

**Akibat pada pelaksanaan.** Daftar `ClinicalDocumentKind` adalah milik modul `rekam-medis`.
Penambahan nilai baru memerlukan kesepakatan dengan pemiliknya. Dicatat sebagai `LAB-COORD-002`.

### BR-14 — Batas nilai menjadi tabel tersendiri milik Laboratorium (`LAB-DEC-018`)

**Latar belakang.** Katalog pemeriksaan laboratorium menumpang
`Areas/HealthServices/MasterData/Models/MstProcedure.cs@c87d9c0` lewat penanda `IsLaboratory`.
Tabel itu dipakai bersama seluruh tindakan rumah sakit, termasuk bedah, terapi, dan radiologi.

**Aturan:**

1. Batas nilai disimpan pada **tabel tersendiri milik modul Laboratorium**, bukan sebagai kolom
   tambahan pada `MstProcedure`.
2. Setiap baris batas nilai menunjuk ke satu jenis pemeriksaan di `MstProcedure`.
3. **Satu jenis pemeriksaan boleh memiliki lebih dari satu baris batas**, dibedakan menurut
   jenis kelamin dan kelompok umur.
4. Isi setiap baris sekurang-kurangnya: penunjuk ke jenis pemeriksaan, satuan hasil, batas
   normal bawah dan atas, batas kritis bawah dan atas, batas waktu penyelesaian cito, pembatas
   jenis kelamin, dan pembatas kelompok umur.
5. Kepala instalasi laboratorium dapat mengubah isinya lewat layar pengelolaan, tanpa
   menerbitkan versi aplikasi baru.

**Contoh kenapa bentuk tabel terpisah yang dipilih:**

| Pemeriksaan | Jenis kelamin | Kelompok umur | Normal bawah | Normal atas | Kritis bawah | Kritis atas |
|---|---|---|---:|---:|---:|---:|
| Hemoglobin | Pria | Dewasa | 13,0 | 17,0 | 7,0 | 20,0 |
| Hemoglobin | Wanita | Dewasa | 12,0 | 15,0 | 7,0 | 20,0 |
| Hemoglobin | Semua | Anak | 11,0 | 14,0 | 6,0 | 18,0 |
| Kalium | Semua | Semua | 3,5 | 5,1 | 2,5 | 6,0 |

> Perhatikan Hemoglobin punya **tiga baris**. Bila batas nilai ditaruh sebagai kolom pada
> `MstProcedure`, Hemoglobin hanya punya satu baris sehingga ketiga batas itu tidak mungkin
> disimpan sekaligus. Itulah alasan pokok bentuk tabel terpisah dipilih, di luar soal menjaga
> `MstProcedure` tetap bersih.

**Batas kepemilikan.** Tabel batas nilai milik Laboratorium. `MstProcedure` tetap milik
`master-data` dan **tidak diubah**. Laboratorium hanya menunjuk ke sana.

### BR-15 — Pengelolaan alasan penolakan sampel dengan dua tingkat kewenangan (`LAB-DEC-019`)

**Latar belakang.** Tabel `Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs@c87d9c0`
sudah ada dan dipakai, tetapi hanya punya endpoint baca
(`Controllers/LabSpecimenController.cs#GetRejectionReasons@c87d9c0`). Tidak ada layar
pengelolaan dan tidak ditemukan pengisian data awal.

Yang membuat tabel ini tidak sesederhana daftar biasa: kolom `IsInternalHospitalError@c87d9c0`
menentukan apakah pengambilan ulang ditanggung rumah sakit atau boleh dibebankan kepada pasien,
sesuai `LAB-INH-011`.

**Aturan:**

| Kolom | Boleh diubah kepala instalasi lab | Alasan |
|---|:---:|---|
| Kode alasan | Ya, saat membuat baru | Penanda teknis, tidak berdampak biaya |
| Nama alasan | Ya | Sekadar penamaan |
| Keterangan | Ya | Sekadar penjelasan |
| Urutan tampil | Ya | Kenyamanan pemakaian |
| Aktif atau tidak | Ya | Alasan yang tidak dipakai boleh disembunyikan |
| **Penanda kesalahan internal rumah sakit** | **Tidak** | Menentukan siapa menanggung biaya. Menurut `LAB-INH-010`, akibat finansial bukan wewenang Laboratorium |
| **Penanda wajib disertai catatan** | **Tidak** | Menentukan kelengkapan bukti saat penolakan; melemahkannya berarti melemahkan jejak audit |

Dua kolom terakhir hanya dapat disetel admin sistem.

**Contoh:**

> Kepala instalasi Pak Hendra menemukan alasan penolakan baru yang sering terjadi: "Sampel
> tidak diberi label". Ia menambahkannya sendiri lewat layar pengelolaan, memberi nama dan
> urutan tampil. Tetapi kolom "kesalahan internal rumah sakit" pada alasan itu tampil terkunci
> dan bertanda gembok — pengisiannya harus lewat admin sistem, karena jawabannya menentukan
> apakah pengambilan darah ulang gratis bagi pasien atau tidak.

**Yang tetap harus disiapkan.** Karena tidak ditemukan pengisian data awal, daftar alasan
penolakan harus terisi sebelum modul dipakai. Bila kosong, petugas tidak bisa menolak sampel
sama sekali. Kebutuhan data awal ini dicatat sebagai bagian pekerjaan Rilis 1.

### BR-16 — Koreksi hasil setelah kunjungan ditutup memakai addendum (`LAB-DEC-020`)

**Latar belakang pertemuan dua aturan.** `LAB-DEC-017` mendaftarkan hasil lab ke rekam medis.
Rekam medis mengunci dokumen ketika kunjungan pasien ditutup, lihat
`ClinicalDocumentLockTrigger.EncounterClosed@c87d9c0`. Sementara `LAB-DEC-007` mengizinkan
koreksi hasil kapan pun. Ketiganya bertemu ketika hasil ketahuan salah setelah kunjungan
ditutup.

**Kemampuan yang sudah ada dan dipakai ulang.** Modul rekam medis sudah punya mekanisme
koreksi untuk dokumen terkunci:
`Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalNoteAddendum.cs@c87d9c0`,
memuat `CorrectionReason`, `AuthorUserId`, `SignedAt`, `Sequence`, dan menempel pada
`IntegrityId`. Mekanisme ini berlaku untuk **semua** jenis dokumen, bukan hanya catatan
perkembangan — dibuktikan oleh
`Controllers/ClinicalNoteAddendumController.cs@c87d9c0` yang menerima `ClinicalDocumentKind`
sebagai parameter.

**Aturan:**

1. Dokumen hasil yang asli **tetap terkunci dan tidak diubah isinya**.
2. Hasil perbaikan didaftarkan sebagai **addendum** pada dokumen asli, wajib menyebutkan alasan
   koreksi dan ditandatangani petugas yang berwenang.
3. Tidak ada pembukaan kunci dokumen. Tidak ada kemampuan baru yang perlu ditambahkan ke modul
   rekam medis.
4. Aturan ini melengkapi, bukan menggantikan, `LAB-DEC-007`: di dalam modul Laboratorium hasil
   tetap berjalan lewat `Corrected/Amended → Revalidated → Released`, dan dokter pemesan tetap
   otomatis diberi tahu.

**Contoh:**

> Hasil Hemoglobin pasien Andi dirilis 9,4 g/dL pada 1 September. Kunjungan Andi ditutup pada
> 2 September, sehingga dokumen hasil terkunci. Pada 4 September ketahuan angka yang benar
> adalah 4,9 g/dL. Tono, yang berwenang validasi, membuat koreksi.
>
> Yang terjadi: dokumen hasil 1 September tetap ada apa adanya, terkunci, memuat 9,4. Di
> atasnya menempel satu addendum bertanda tangan Tono tertanggal 4 September, beralasan
> "tertukar dengan sampel pasien lain", memuat angka 4,9. dr. Rina menerima pemberitahuan
> otomatis bahwa hasil pasiennya berubah. Siapa pun yang membuka berkas Andi melihat keduanya
> beserta urutan waktunya.

**Kenapa pilihan ini yang terbaik.** Tidak ada satu pun kemampuan baru yang perlu dibangun,
janji "terkunci berarti terkunci" tetap utuh, dan bentuknya persis sesuai `LAB-INH-003` yang
menyebut jalur koreksi harus mempertahankan riwayat rilis lama.

### BR-17 — Hasil punya dua bentuk: angka dan pilihan terbatas (`LAB-DEC-021`)

**Latar belakang gap `DEC-LAB-002`.** Gerbang kelengkapan requirement menemukan bahwa seluruh
tabel batas nilai pada BR-04 berbentuk angka — satuan, batas bawah, batas atas. Padahal
`LAB-DEC-002` membatasi modul pada Patologi Klinik, dan Patologi Klinik **tidak seluruhnya
berupa angka**.

**Aturan:**

1. Setiap jenis pemeriksaan ditetapkan **bentuk hasilnya** sejak awal, tepat satu dari dua:
   **hasil angka** atau **hasil pilihan terbatas**.
2. Pemeriksaan berhasil angka memakai batas normal bawah dan atas serta batas kritis bawah dan
   atas, persis seperti BR-04.
3. Pemeriksaan berhasil pilihan menyimpan **daftar pilihan yang sah**, beserta penanda pilihan
   mana yang dianggap **di luar rujukan** dan mana yang dianggap **kritis**.
4. Analis tidak boleh mengetik bebas pada pemeriksaan berhasil pilihan. Ia hanya memilih dari
   daftar yang sah.
5. `LAB-DEC-004` tentang nilai kritis berlaku untuk **kedua bentuk**, bukan hanya bentuk angka.

**Contoh bentuk angka:**

| Pemeriksaan | Satuan | Normal | Kritis bawah | Kritis atas |
|---|---|---|---:|---:|
| Kalium | mmol/L | 3,5 – 5,1 | 2,5 | 6,0 |

**Contoh bentuk pilihan terbatas:**

| Pemeriksaan | Pilihan sah | Di luar rujukan | Kritis |
|---|---|---|---|
| Protein urin | Negatif, +1, +2, +3, +4 | +1, +2 | +3, +4 |
| Glukosa urin | Negatif, +1, +2, +3 | +1 | +2, +3 |
| Tes kehamilan | Positif, Negatif | — | — |
| Golongan darah | A, B, AB, O | — | — |

**Contoh penerapan:**

> Protein urin pasien Andi keluar +4. Analis memilih "+4" dari daftar, bukan mengetiknya.
> Sistem mencocokkan pilihan itu dengan daftar kritis, menemukan +4 termasuk kritis, lalu
> memunculkan formulir pelaporan BR-02 persis seperti pada Kalium 7,2 mmol/L.
>
> Bandingkan bila hasil diketik bebas: analis pertama menulis "+4", analis kedua menulis
> "Positif kuat (4+)", analis ketiga menulis "protein +4". Sistem tidak akan bisa mengenali
> ketiganya sebagai hal yang sama, sehingga nilai kritis tidak pernah terdeteksi.

**Catatan untuk golongan darah dan tes kehamilan.** Keduanya berbentuk pilihan tetapi tidak
punya nilai kritis — tidak ada golongan darah yang "berbahaya". Kolom kritisnya dibiarkan
kosong, dan itu sah.

### BR-18 — Kewenangan validasi dan rilis diberikan per orang (`LAB-DEC-022`)

**Latar belakang gap `DEC-LAB-001`.** `LAB-INH-007` menyatakan validasi dan rilis memakai
kewenangan berbeda, dan jabatan organisasi tidak otomatis memberi kewenangan. `BR-01`
menyatakan pengisi hasil tidak boleh memvalidasi hasil yang sama. Keduanya mengatur **hubungan
antar kewenangan**, tetapi tidak satu pun menyebut **siapa yang memegangnya**.

**Aturan:**

1. Kewenangan **validasi** dan kewenangan **rilis** tetap dua hal terpisah, sesuai
   `LAB-INH-007`.
2. Keduanya diberikan **kepada orang per orang**, bukan melekat pada jabatan. Seorang analis
   senior boleh memegangnya, seorang kepala ruangan boleh tidak.
3. Rumah sakit menjamin **setiap shift memiliki sekurang-kurangnya dua orang pemegang
   kewenangan validasi**.
4. Sistem menampilkan peringatan kepada kepala instalasi bila suatu shift hanya memiliki satu
   pemegang kewenangan validasi, karena pada shift itu prinsip empat mata pasti akan gagal.
5. Sistem tidak menetapkan siapa yang berwenang. Ia hanya menegakkan aturan atas penetapan yang
   dibuat rumah sakit.

**Kenapa butir 3 dan 4 penting.**

> Bila sebuah shift hanya punya satu pemegang kewenangan validasi, maka setiap hasil yang ia
> kerjakan sendiri akan lewat jalur pengecualian `BR-01`. Dalam sebulan, "pengecualian" itu
> berubah menjadi kebiasaan, dan prinsip empat mata berhenti berarti apa pun — padahal
> pengujiannya tetap lulus dan tidak ada aturan yang dilanggar.
>
> Peringatan pada butir 4 membuat keadaan itu terlihat sebelum menjadi kebiasaan.

**Contoh penerapan:**

> Shift malam Sabtu dijadwalkan berisi analis Sari dan analis Budi. Keduanya memegang
> kewenangan validasi, Budi juga memegang kewenangan rilis. Sari mengerjakan Kalium pasien
> Andi dan mengisi hasilnya. Budi memvalidasi — `BR-01` terpenuhi karena Budi bukan Sari — lalu
> merilisnya.
>
> Bila Budi mendadak berhalangan dan Sari bertugas sendirian, kepala instalasi mendapat
> peringatan bahwa shift itu hanya punya satu pemegang kewenangan validasi. Sari tetap dapat
> bekerja lewat jalur pengecualian `BR-01`, tetapi keadaannya sudah diketahui, bukan
> tersembunyi.
>
> **⚠ Catatan 2026-09-24:** butir 2 aturan ini — analis senior boleh memegang kewenangan —
> **superseded** oleh `LAB-DEC-150`. Baca Sari dan Budi sebagai **dokter** berkewenangan
> laboratorium. Butir 3 dan 4 — minimal dua pemegang per shift dan peringatannya — tetap berlaku.

**Yang tetap milik rumah sakit, bukan sistem.** Penetapan siapa saja yang layak memegang
kewenangan validasi adalah keputusan kepegawaian dan kompetensi. Sistem tidak ikut menilainya.

### BR-19 — Batas kritis lebih terlindungi daripada batas normal (`LAB-DEC-023`)

**Latar belakang gap `DEC-LAB-003`.** `LAB-DEC-019` mengunci kolom penanda kesalahan internal
pada tabel alasan penolakan agar hanya dapat disetel admin sistem, karena kolom itu menentukan
siapa menanggung biaya. Sementara `LAB-DEC-018` membiarkan kepala instalasi mengubah seluruh
isi tabel batas nilai dengan bebas — termasuk batas kritis, yang menentukan kapan seorang
pasien dinyatakan dalam bahaya. Perlindungan atas angka keselamatan justru lebih longgar
daripada perlindungan atas angka biaya.

**Aturan:**

| Yang diubah | Siapa yang boleh | Perlu persetujuan | Riwayat disimpan |
|---|---|:---:|:---:|
| Satuan hasil | Kepala instalasi | Tidak | Ya |
| Batas normal bawah dan atas | Kepala instalasi | Tidak | Ya |
| Daftar pilihan sah dan penanda di luar rujukan | Kepala instalasi | Tidak | Ya |
| **Batas kritis bawah dan atas** | Kepala instalasi mengajukan | **Ya, persetujuan klinis** | Ya |
| **Penanda pilihan yang dianggap kritis** | Kepala instalasi mengajukan | **Ya, persetujuan klinis** | Ya |
| Batas waktu penyelesaian cito | Kepala instalasi | Tidak | Ya |

Riwayat perubahan menyimpan sekurang-kurangnya: kolom apa yang berubah, nilai lama, nilai baru,
siapa yang mengubah atau mengajukan, siapa yang menyetujui bila diperlukan, waktu, dan alasan.

**Kenapa batas normal dibedakan dari batas kritis.**

> Batas normal memang wajar berubah. Ketika laboratorium mengganti alat atau metode
> pemeriksaan, rentang normal bisa bergeser sedikit, dan itu penyesuaian teknis biasa yang
> memang menjadi keahlian kepala instalasi.
>
> Batas kritis berbeda sifatnya. Ia bukan soal metode, melainkan soal pada angka berapa seorang
> pasien dianggap terancam. Itu penilaian klinis, bukan penilaian teknis laboratorium.

**Contoh yang dicegah aturan ini:**

> Kepala instalasi merasa terlalu banyak peringatan nilai kritis mengganggu pekerjaan harian,
> lalu menaikkan batas kritis atas Kalium dari 6,0 menjadi 8,0. Sejak saat itu pasien dengan
> Kalium 7,2 mmol/L tidak lagi memicu kewajiban pelaporan `BR-02`. Tidak ada aturan yang
> dilanggar dan tidak ada yang menyadarinya.
>
> Dengan `BR-19`, perubahan itu berhenti sebagai pengajuan sampai pihak klinis menyetujuinya,
> dan seluruh jejaknya tersimpan.

**Hubungan dengan keputusan sebelumnya.** Aturan ini **mempersempit** `LAB-DEC-018`, tidak
membatalkannya. Janji "kepala instalasi dapat mengubah tanpa menerbitkan versi aplikasi baru"
tetap berlaku untuk seluruh kolom kecuali dua kolom keselamatan di atas.

### BR-20 — Wadah fisik dipisahkan dari pemeriksaan terpesan (`LAB-DEC-024`)

**Latar belakang gap `DEC-LAB-008`.** Arsitektur domain menemukan bahwa model yang berjalan
menyatukan dua hal yang berbeda. Satu baris sampel membawa tepat satu jenis pemeriksaan, satu
barcode, satu keputusan layak atau tolak, dan satu baris tagihan — bukti pada
`TrxLabSpecimen.ProcedureId@c87d9c0`.

Selama tiap pemeriksaan memang memakai wadah berbeda, model itu tampak benar. Masalahnya muncul
ketika dua pemeriksaan berbagi satu wadah yang sama, misalnya fungsi hati dan fungsi ginjal yang
keduanya diperiksa dari satu tabung serum hasil sekali tusuk.

**Aturan:**

1. **Wadah Fisik** menjadi konsep tersendiri. Satu wadah berarti satu tabung atau satu pot
   nyata: satu barcode, satu peristiwa pengambilan, dan **satu** keputusan layak atau tolak.
2. **Pemeriksaan Terpesan** menjadi konsep tersendiri. Satu pemeriksaan berarti satu jenis
   pemeriksaan yang diminta: satu tarif, satu baris tagihan, dan kelak satu hasil.
3. **Satu wadah dapat melayani beberapa pemeriksaan.** Satu pemeriksaan ditopang tepat satu
   wadah.
4. Keputusan layak atau tolak diambil atas **wadah**, dan berlaku serentak bagi seluruh
   pemeriksaan yang ditopangnya. Menolak sebagian tidak lagi mungkin.
5. Kelayakan tagih tetap terbit **per pemeriksaan**, dipicu oleh dinyatakan layaknya wadah yang
   menopangnya.
6. Ambil ulang menciptakan **wadah** baru, dan seluruh pemeriksaan yang ditopangnya ikut
   berpindah ke wadah baru itu. Wadah lama tetap terlihat beserta tautan sebabnya.

**Contoh:**

> dr. Rina memesan Fungsi hati Rp150.000 dan Fungsi ginjal Rp120.000 untuk pasien Andi.
> Keduanya diperiksa dari satu tabung serum. Perawat Dewi menusuk sekali, mengisi satu tabung,
> menempel **satu** barcode.
>
> Petugas Budi memeriksa tabung itu dan menyatakannya layak. Pada saat itu terbit **dua**
> kejadian kelayakan tagih: Fungsi hati Rp150.000 dan Fungsi ginjal Rp120.000. Satu wadah, dua
> tagihan — dan itu memang benar, karena pasien memang menjalani dua pemeriksaan.
>
> Bandingkan bila tabung itu ternyata keruh dan Budi menolaknya. **Kedua** pemeriksaan gugur
> serentak, karena memang tidak ada bahan yang bisa dikerjakan. Model lama mengizinkan Budi
> menolak Fungsi hati sambil menerima Fungsi ginjal — sesuatu yang tidak mungkin terjadi di
> meja kerja.

**Kenapa diputuskan sekarang.** Hasil pemeriksaan melekat pada **pemeriksaan**, bukan pada
wadah. Slice hasil belum ditulis sebaris pun. Memutuskan pemisahan ini setelah tabel hasil
terbentuk dan terisi angka pasien jauh lebih mahal dan lebih berisiko.

**Kesesuaian dengan keputusan terkunci.** Pemisahan ini **tidak** melanggar `LAB-INH-005`
(satu pesanan boleh punya banyak sampel — tetap berlaku), `LAB-INH-009` (titik kelayakan tagih
tetap pada dinyatakan layak), maupun `LAB-INH-010` (Billing tetap satu-satunya pemilik akibat
finansial). Yang berubah adalah **satuan** tempat kelayakan tagih menempel, bukan aturannya.

**Yang wajib diperiksa sebelum dikerjakan.** Perubahan ini menyentuh struktur data yang sudah
berjalan. Sebelum dikerjakan, wajib dipastikan berapa banyak data laboratorium yang benar-benar
sudah terisi di basis data produksi. Bukti `01-existing-capability-map.md#CAP-21` menunjukkan
frontend Laboratorium masih nol, sehingga kemungkinan besar belum ada data pasien sungguhan —
tetapi itu **dugaan, bukan bukti**. Dicatat sebagai `LAB-OPEN-012`.

---

### BR-21 — Cakupan diperluas menjadi tiga disiplin (`LAB-DEC-025`)

**Menggantikan `LAB-DEC-002`.**

**Latar belakang.** `LAB-DEC-002` membatasi modul pada Patologi Klinik. Analisis konsolidasi
bukti lapangan menunjukkan laboratorium rumah sakit ini menjalankan **tiga disiplin sejajar**,
masing-masing dengan daftar pasien, alur hasil, dan laporan tersendiri.

**Aturan:**

| Disiplin | Status | Bukti |
|---|---|---|
| Patologi Klinik | **Di dalam scope** | Monitoring pada aplikasi baru; alur hasil pada workstation HCLAB |
| Patologi Anatomi | **Di dalam scope** | Daftar pasien tersendiri; nomor PA/Sitologi/FNAB; makroskopik, mikroskopik, kesimpulan |
| Mikrobiologi | **Di dalam scope** | Daftar pasien tersendiri; organisme, sensitivitas antibiotik, laporan R/I/S |
| **Bank Darah** | **Tetap di luar scope** | Analisis konsolidasi memisahkannya secara eksplisit. Kemunculannya sebagai pilihan workstation HCLAB **tidak** memasukkannya ke scope |

**Akibat.** Ketiga disiplin berbagi konsep yang sama untuk pesanan, wadah, dan kelayakan tagih,
tetapi **berbeda pada bentuk hasilnya**. Perbedaan itu diatur BR-23.

**Contoh perbedaan yang harus ditampung:**

> Satu pasien menjalani Hemoglobin (Patologi Klinik), kultur darah (Mikrobiologi), dan biopsi
> kulit (Patologi Anatomi). Ketiganya berangkat dari pesanan dan wadah yang bentuknya sama,
> tetapi hasilnya berbeda total: angka bersatuan, daftar bakteri beserta kepekaan antibiotik,
> dan uraian naratif beserta gambar.

### BR-22 — Cito dan Duplo melekat pada pemeriksaan, bukan pesanan (`LAB-DEC-026`)

**Mengubah `LAB-DEC-013` dan BR-09.** Aturan cito tetap berlaku; yang berubah adalah **letaknya**.

**Latar belakang.** BR-09 menaruh penanda kesegeraan pada pesanan. Bukti lapangan menunjukkan
Cito adalah kolom **per baris pemeriksaan**, sejajar dengan harga dan subtotal, dan muncul pula
pada form hasil. Duplo mengikuti pola yang sama.

**Aturan:**

1. Penanda **Cito** melekat pada **pemeriksaan terpesan**, bukan pada pesanan.
2. Penanda **Duplo** juga melekat pada pemeriksaan terpesan.
3. Satu pesanan boleh memuat pemeriksaan cito dan pemeriksaan biasa sekaligus.
4. Batas waktu penyelesaian cito tetap disimpan pada tabel batas nilai per jenis pemeriksaan,
   sesuai `LAB-DEC-013`.
5. Daftar kerja mendahulukan **pemeriksaan** bertanda cito, bukan seluruh isi pesanannya.

**Contoh:**

> dr. Rina memesan Kalium cito bersama Kolesterol rutin dalam satu pesanan. Dengan aturan lama,
> seluruh pesanan menjadi cito sehingga Kolesterol ikut menyita antrean prioritas. Dengan
> BR-22, hanya Kalium yang naik ke urutan atas daftar kerja; Kolesterol tetap di antrean biasa.

**Yang belum diputuskan:** apakah penanda Cito dan Duplo berdampak pada tarif. Bukti
menempatkan keduanya pada baris yang sama dengan harga, tetapi dampaknya tidak diperagakan.
Dicatat sebagai `LAB-OPEN-013`.

### BR-23 — Hasil punya empat bentuk (`LAB-DEC-027`)

**Menggantikan `LAB-DEC-021`.** Dua bentuk yang sudah diputuskan tetap berlaku; dua bentuk
ditambahkan.

| Bentuk | Dipakai oleh | Isi | Dapat dinilai kritis otomatis |
|---|---|---|:---:|
| **Angka bersatuan** | Patologi Klinik | Hasil, satuan, batas normal, batas kritis, penanda rendah/tinggi | **Ya** |
| **Pilihan terbatas** | Patologi Klinik, Mikrobiologi | Daftar pilihan sah beserta penanda di luar rujukan dan penanda kritis | **Ya** |
| **Mikrobiologi berstruktur** | Mikrobiologi | Penanda definitif, status Normal/Positif/Negatif, organisme per bakteri, antibiotik, kadar, zona dalam mm, dan hasil `R`/`I`/`S` | **Tidak** — lihat catatan |
| **Narasi Patologi Anatomi** | Patologi Anatomi | Makroskopik, mikroskopik, dan kesimpulan — ketiganya wajib — beserta gambar contoh | **Tidak** — lihat catatan |

**Arti kode kepekaan antibiotik**, sesuai bukti lapangan:

| Kode | Arti |
|---|---|
| `R` | *Resistent* — bakteri kebal terhadap antibiotik itu |
| `I` | *Intermediate* — kepekaan berada di antara, perlu pertimbangan dosis |
| `S` | *Sensitive* — bakteri peka, antibiotik itu diperkirakan bekerja |

**Aturan turunan:**

1. Setiap hasil bakteri disimpan sebagai baris tersendiri yang dapat ditambah dan dikurangi.
2. Gambar pada hasil Patologi Anatomi dibatasi ukurannya; batas yang terlihat pada bukti adalah
   2 MB.
3. Makroskopik, mikroskopik, dan kesimpulan **wajib** terisi untuk Patologi Anatomi.

**Catatan penting tentang penilaian kritis.** Bentuk ketiga dan keempat **tidak dapat** dinilai
kritis dengan mekanisme batas nilai. Bakteri resisten dan kesimpulan patologi yang mengkhawatirkan
adalah penilaian klinis, bukan perbandingan angka. Bagaimana keduanya masuk alur nilai kritis
**belum diputuskan** dan dicatat sebagai `LAB-OPEN-014`.

### BR-24 — Laboratorium memiliki jalur pendaftaran pasien sendiri (`LAB-DEC-028`)

**Mengubah batas scope yang sebelumnya menyerahkan seluruh pendaftaran ke Registrasi.**

**Latar belakang.** Blueprint mengasumsikan setiap pesanan menempel pada kunjungan yang sudah
dibuat modul Registrasi. Bukti lapangan menunjukkan laboratorium menerima **pasien datang
langsung** dan **pasien rujukan dari luar** yang belum punya kunjungan sama sekali.

**Aturan:**

| Jalur | Asal pasien | Yang dilakukan Laboratorium |
|---|---|---|
| Kunjungan yang sudah ada | Rawat Jalan, Rawat Inap, IGD | Menempel pada kunjungan itu, seperti rancangan semula |
| **Pasien datang langsung** | Datang sendiri ke laboratorium | Laboratorium mendaftarkan pasien dan membuat konteks kunjungannya |
| **Pasien rujukan luar** | Dikirim dokter atau institusi lain | Laboratorium mendaftarkan pasien beserta data perujuknya |

**Data rujukan yang wajib ditampung:** dokter perujuk, instansi atau rumah sakit perujuk,
kontak instansi, surat rujukan, dan diagnosis awal.

**Batas yang tetap dipegang.** Identitas pasien tetap **milik** modul Patient Management, dan
kunjungan tetap **milik** modul Registrasi. Laboratorium **tidak** membuat salinan pasien.
Bagaimana tepatnya Laboratorium membuat kunjungan tanpa mengambil alih kepemilikannya
**belum diputuskan** dan dicatat sebagai `LAB-OPEN-015`.

### BR-28 — Laboratorium meminta Registrasi membuat kunjungan (`LAB-DEC-032`)

**Menutup `LAB-OPEN-015`.** Melengkapi BR-24, tidak menggantikannya.

**Bukti yang menentukan.** Modul Registrasi **sudah memiliki** seluruh yang dibutuhkan pada
`c87d9c0`:

| Yang sudah ada | Lokasi |
|---|---|
| Nilai `WalkIn` pada sumber pendaftaran | `EncounterRegistrationSource.WalkIn = 5` |
| Penanda pasien datang langsung | `TrxPatientEncounter.IsWalkIn` |
| Penanda dan nomor rujukan | `TrxPatientEncounter.IsReferral`, `ReferralNumber`, `IsReferralRequired`, `IsReferralVerified` |
| Pembuatan kunjungan datang langsung | `PatientEncounterController@c87d9c0` |

**Aturan:**

1. **Layar pendaftaran tetap milik Laboratorium.** Petugas lab tidak berpindah aplikasi untuk
   menerima pasien datang langsung atau pasien rujukan luar.
2. Saat pendaftaran disimpan, Laboratorium **memanggil Registrasi** dengan mengirim identitas
   pasien, penanda datang langsung, dan data rujukan.
3. Registrasi yang **membuat** kunjungan, menjalankan aturannya sendiri, lalu mengembalikan
   penunjuk kunjungan yang baru dibuat.
4. Laboratorium menyimpan penunjuk itu pada pesanan, persis seperti pesanan dari poliklinik.
5. Laboratorium **tidak menulis** satu baris pun ke tabel milik Registrasi maupun Patient
   Management.

**Akibat bagi invariant.** `INV-01` — setiap pesanan terikat pada tepat satu kunjungan yang
sudah ada — **tetap utuh**. Tidak ada pengecualian yang dibuat untuk pasien datang langsung.

**Contoh:**

> Ibu Sari datang sendiri ke laboratorium membawa surat rujukan dari klinik luar, tanpa pernah
> mendaftar di loket. Petugas lab membuka layar pendaftaran di aplikasi Laboratorium, mengisi
> identitas Ibu Sari beserta nama klinik dan dokter perujuknya, lalu menyimpan.
>
> Di balik layar, Laboratorium meminta Registrasi membuat kunjungan bertanda datang langsung
> dan bertanda rujukan. Registrasi menjalankan aturannya — penomoran kunjungan, pemeriksaan
> kelengkapan — lalu mengembalikan penunjuk kunjungan. Pesanan lab Ibu Sari menempel pada
> kunjungan itu, sama seperti pesanan pasien poliklinik.
>
> Ketika kelak hasilnya perlu ditelusuri, atau Billing perlu menagihkannya, konteks kunjungan
> sudah ada dan tidak ada yang berbeda.

**Akibat pada pelaksanaan.** Kontrak pemanggilan antarmodul memerlukan kesepakatan dengan
pemilik `registration-management`. Dicatat sebagai `LAB-COORD-003`.

### BR-30 — Penempatan data induk mengikuti cakupan pemakaiannya (`LAB-DEC-034`)

> **Aturan ini berlaku untuk backend saja.** Frontend **tidak mengikutinya** — menu data induk
> di frontend tetap memakai konvensi yang sudah berjalan, yaitu seluruhnya berada di
> `health-services/master-data/`. Rinciannya ada pada `03-frontend-architecture.md` bagian 2.1.

| Cakupan data induk | Letaknya di backend |
|---|---|
| **Khusus Laboratorium** — hanya dipakai modul ini | `Areas/HealthServices/LaboratoryManagement/Models/` |
| **Global** — dipakai lebih dari satu modul | `Areas/HealthServices/MasterData/Models/` |

**Penerapan pada modul ini:**

| Data induk | Cakupan | Letaknya |
|---|---|---|
| `MstLabRejectionReason` | Khusus Laboratorium | `LaboratoryManagement/Models/` — **sudah benar**, tetap di sana |
| `LabValueBound` | Khusus Laboratorium | `LaboratoryManagement/Models/` |
| `LabValueOption` | Khusus Laboratorium | `LaboratoryManagement/Models/` |
| `MstProcedure` | Global — dipakai seluruh layanan | `MasterData/Models/` — **tidak disentuh** |
| `MstTariff`, `MstInsuranceTariff` | Global | `MasterData/Models/` — **tidak disentuh** |
| `MstAgeCategory` | Global | `MasterData/Models/` — **tidak disentuh** |

**Bukti bahwa aturan ini memang pola yang berlaku.** Pada `c87d9c0` terdapat **20 data induk
khusus modul** yang sudah berada di folder modulnya masing-masing:

| Modul | Contoh |
|---|---|
| HR Service Management | `MstEmployeeDocumentType`, `MstHrServiceCategory`, `MstHrServiceType` |
| Lifecycle Management | `MstOnboardingTemplate`, `MstOffboardingTemplate` |
| Recruitment Management | `MstCandidateStatus`, `MstInterviewTemplate`, `MstRecruitmentStage` |
| Workforce Planning | `MstStaffingRatio`, `MstStaffingStandard`, `MstWorkforceRequirement` |
| Pharmacy Management | `MstPrescriptionReviewCriterion`, `MstPrescriptionTemplate` |
| **Laboratory Management** | `MstLabRejectionReason` |

Sementara `Areas/HealthServices/MasterData/Models/` berisi 61 data induk yang memang dipakai
lintas modul.

**Koreksi terhadap dokumen aturan.** `backend-structure-rules.md` menyatakan seluruh data induk
berada di `Areas/HealthServices/MasterData/Models/`, dengan contoh `MstEmergencyTriageLevel`.
Dua hal keliru pada pernyataan itu:

1. `MstEmergencyTriageLevel` **tidak ditemukan** di source pada `c87d9c0`.
2. Pola nyata yang berlaku adalah pemisahan menurut cakupan, bukan penyeragaman ke satu folder.

Karena itu penempatan `MstLabRejectionReason` di folder Laboratorium **bukan utang teknis**.
Catatan utang teknis pada `02-backend-architecture.md` revision 1 dicabut.

**Cara menilai cakupan sebuah data induk:**

> Pertanyaannya sederhana: *apakah modul selain Laboratorium akan pernah membacanya?*
>
> Alasan penolakan sampel — hanya Laboratorium. Batas nilai pemeriksaan — hanya Laboratorium.
> Keduanya khusus.
>
> Jenis tindakan, tarif, kategori umur — dibaca Rawat Jalan, Rawat Inap, IGD, Farmasi, dan
> Billing. Ketiganya global, dan Laboratorium **tidak boleh** memindahkannya.

### BR-31 — Sumber rujukan menjadi data induk global (`LAB-DEC-035`)

**Menutup `DEC-LAB-009`.** Membuka slice `S13b` pendaftaran pasien rujukan luar.

**Latar belakang.** `TrxPatientEncounter@c87d9c0` hanya menyimpan **penanda dan nomor**
rujukan — `IsReferral`, `ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`. Tidak ada
nama dokter perujuk, nama instansi, alamat, maupun telepon. Tidak ada pula data induk instansi
perujuk; `MstHospitalSite` adalah lokasi milik rumah sakit ini sendiri, bukan institusi luar.

**Aturan:**

1. **Instansi perujuk** menjadi data induk **global** di bawah Master Data: nama klinik atau
   rumah sakit, alamat, telepon, dan penanda aktif.
2. **Dokter perujuk** juga menjadi data induk global, tertaut ke instansinya.
3. **Kunjungan menunjuk** ke keduanya. Nama tidak disimpan sebagai teks bebas pada kunjungan.
4. Laboratorium **tidak memiliki** dan **tidak menyalin** keduanya. Ia hanya memilih dari daftar
   saat mendaftarkan pasien rujukan.

**Kenapa global, bukan khusus Laboratorium.** Menurut `LAB-DEC-034`, penempatan mengikuti
cakupan pemakaian. Rujukan **bukan** hal khusus laboratorium:

| Bukti | Isi |
|---|---|
| Kunjungan sudah punya penanda rujukan sejak awal | `TrxPatientEncounter.IsReferral@c87d9c0` — dipakai seluruh jenis kunjungan |
| Rawat Jalan dan IGD juga menerima pasien rujukan | Penanda itu tidak dibatasi pada kunjungan laboratorium |

Karena itu instansi dan dokter perujuk berada di `Areas/HealthServices/MasterData/Models/`,
bukan di folder Laboratorium.

**Contoh yang dicegah aturan ini:**

> Klinik Sehat Sentosa mengirim rata-rata 40 pasien per bulan. Bila namanya diketik bebas,
> tiga petugas berbeda akan menulis "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan
> "sehat sentosa". Laporan dokter pengirim akan menghitungnya sebagai tiga institusi dengan
> masing-masing belasan pasien, dan kerja sama dengan klinik itu tidak akan pernah terlihat
> nilainya.
>
> Dengan daftar terkendali, ketiganya menunjuk satu baris yang sama.

**Akibat pada pelaksanaan:**

| Yang diperlukan | Pemiliknya |
|---|---|
| Dua data induk baru: instansi perujuk dan dokter perujuk | Master Data |
| Kolom penunjuk pada kunjungan | Registrasi |
| Layar pemilihan saat mendaftarkan pasien rujukan | Laboratorium |
| Pengisian daftar instansi perujuk sebelum dipakai | Master Data bersama Laboratorium |

Karena dua di antaranya milik modul lain, diperlukan kesepakatan. Dicatat sebagai
`LAB-COORD-004`.

### BR-32 — Disiplin melekat pada jenis pemeriksaan di katalog (`LAB-DEC-036`)

**Menutup `DEC-LAB-010`.** Mengamandemen AC-25.

**Latar belakang.** `MstProcedure@c87d9c0` sudah memiliki penanda jenis tindakan —
`IsLaboratory`, `IsRadiology`, `IsSurgery`, `IsTherapy` — tetapi **tidak ada pembeda** antara
Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Yang tersedia hanya `ProcedureGroupName`
dan `ProcedureCategoryName` berupa teks bebas, yang tidak dapat diandalkan.

**Aturan:**

1. Satu kolom penanda disiplin ditambahkan pada `MstProcedure`, hanya bermakna bila
   `IsLaboratory` bernilai benar.
2. Nilainya: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi.
3. Sistem menolak pemeriksaan yang disiplinnya tidak sesuai dengan disiplin pesanan
   (`INV-22`).
4. Kolom itu **satu-satunya** tambahan Laboratorium pada `MstProcedure`. Satuan hasil, batas
   nilai, jenis wadah, dan atribut operasional lain **tetap tidak boleh** masuk ke sana —
   seluruhnya berada di tabel milik Laboratorium sesuai `LAB-DEC-018`.

**Kenapa ini boleh, sementara batas nilai tidak boleh.**

| Yang ditambahkan | Sifatnya | Boleh di `MstProcedure`? |
|---|---|---|
| Penanda disiplin | **Klasifikasi** jenis tindakan, sejenis `IsLaboratory` dan `IsRadiology` yang sudah ada | **Ya** |
| Satuan hasil, batas normal, batas kritis | **Data operasional** yang berbeda menurut jenis kelamin dan umur, sehingga tidak muat satu baris per pemeriksaan | **Tidak** |

**Amandemen AC-25.** Bunyi lama: *"`MstProcedure` tidak bertambah satu kolom pun akibat
pekerjaan modul Laboratorium."* Bunyi baru ada pada AC-25 yang diperbarui — yang dilarang
adalah kolom **operasional**, bukan kolom klasifikasi.

**Contoh penerapan `INV-22`:**

> Petugas membuat pesanan berdisiplin Mikrobiologi, lalu mencoba menambahkan Hemoglobin.
> Hemoglobin bertanda disiplin Patologi Klinik pada katalog, sehingga sistem menolaknya dengan
> pesan bahwa pemeriksaan itu bukan bagian Mikrobiologi.
>
> Tanpa kolom ini, sistem tidak punya cara mengetahuinya, dan pesanan campur aduk baru
> ketahuan saat petugas laboratorium kebingungan di meja kerja.

**Akibat pada pelaksanaan.** `MstProcedure` milik Master Data. Penambahan kolom memerlukan
izin pemiliknya, dan pengisian nilainya untuk seluruh pemeriksaan berpenanda `IsLaboratory`
yang sudah ada. Dicatat sebagai `LAB-COORD-005`.

### BR-25 — Tarif ditampilkan, keputusan uang tetap milik Billing (`LAB-DEC-029`)

**Menegaskan `LAB-INH-010` dan `LAB-INH-012`, tidak melemahkannya.**

**Aturan:**

| Yang **boleh** dilakukan Laboratorium | Yang **tetap dilarang** |
|---|---|
| Menampilkan harga satuan, jumlah, subtotal, dan total saat memesan | Membuat, mengubah, atau membatalkan tagihan |
| Menampilkan status cakupan penjamin, termasuk penanda tidak tercakup | Memutuskan apakah pasien membayar |
| Menyimpan salinan tarif saat kejadian pada baris pemeriksaan | Menyimpan status pembayaran sebagai kebenaran |
| Mengelola daftar tarif laboratorium sebagai data induk modul | Menghitung tagihan akhir |

**Contoh yang menjelaskan batas ini:**

> Saat memesan, petugas melihat Hemoglobin Rp50.000, Kultur darah Rp350.000, total
> Rp400.000, dan penanda bahwa kultur darah tidak tercakup penjamin. Angka itu **membantu
> petugas dan pasien mengambil keputusan sebelum pemeriksaan dimulai**. Ia bukan tagihan.
> Tagihan tetap dibentuk Billing berdasarkan fakta kelayakan tagih yang dikirim Laboratorium.

**Kepemilikan tarif ditutup `LAB-DEC-033`** — lihat BR-29.

### BR-29 — Tarif tetap milik Master Data; Laboratorium hanya menyajikannya (`LAB-DEC-033`)

**Menutup `LAB-OPEN-016`.** Mempertajam BR-25.

**Bukti yang menentukan.** `MstTariff@c87d9c0` adalah tabel tarif **bersama seluruh rumah
sakit**, bukan tabel khusus satu modul:

| Yang sudah ditampung `MstTariff` | Keterangan |
|---|---|
| `ProcedureId` | Menunjuk jenis tindakan atau pemeriksaan |
| `DrugId` | Tabel yang sama juga melayani obat |
| `ServiceUnitId`, `ClinicId`, `PatientClassId` | Tarif dapat berbeda menurut unit, klinik, dan kelas pasien |
| `EffectiveStartDate`, `EffectiveEndDate` | Masa berlaku tarif |
| `IsRoomCharge`, `IsAdministrationFee`, `IsRegistrationFee`, `IsConsultationFee` | Tabel yang sama juga melayani biaya kamar, administrasi, pendaftaran, dan konsultasi |

Laboratorium sudah membacanya lewat `LabSpecimenService#ResolveTariffAsync@c87d9c0`, yang
memilih tarif berlaku menurut `ProcedureId` dan masa berlakunya.

**Aturan:**

1. Tarif pemeriksaan laboratorium **tetap milik** Master Data. Laboratorium **tidak** membuat
   tabel tarif sendiri.
2. Menu **`Tarif Laboratorium`** pada modul Laboratorium adalah **tampilan tersaring**: ia
   memperlihatkan baris `MstTariff` yang menunjuk pemeriksaan berpenanda `IsLaboratory`, agar
   kepala instalasi dapat memeriksanya tanpa berpindah modul.
3. Menu itu bersifat **baca saja**. Perubahan tarif tetap dilakukan lewat Master Data.
4. Salinan tarif saat kejadian tetap disimpan pada baris pemeriksaan, sesuai `LAB-DEC-024`.

**Kenapa satu sumber harga itu penting.**

> Bila Laboratorium punya tabel tarifnya sendiri, rumah sakit akan punya dua harga untuk
> pemeriksaan yang sama. Petugas lab menyebut Hemoglobin Rp50.000 kepada pasien, sementara
> Billing menagihkan Rp65.000 dari tabel yang berbeda. Yang dirugikan bukan sistemnya,
> melainkan kepercayaan pasien di loket.

**Akibat yang harus disadari.** Kepala instalasi laboratorium **tidak dapat** mengubah tarif
sendiri. Setiap penyesuaian tarif pemeriksaan melewati petugas Master Data. Ini konsekuensi
yang disengaja dari memilih satu sumber harga.

**Perbedaan dengan sistem yang berjalan.** Aplikasi laboratorium yang ada memperlihatkan
`Tarif Laboratorium` sebagai menu tersendiri. Pada Quilvian, menu itu tetap ada tetapi
**maknanya berubah** — dari pengelolaan menjadi penyajian.

### BR-26 — Kemampuan tambahan yang masuk scope modul (`LAB-DEC-030`)

Sebelas kemampuan yang terlihat pada bukti lapangan dinyatakan **milik modul Laboratorium**,
dengan pembagian rilis sebagai berikut.

| Kemampuan | Rilis | Alasan penempatan |
|---|---|---|
| Pendaftaran pasien datang langsung dan rujukan luar | **Rilis 1** | Tanpa ini, sebagian pasien tidak dapat dilayani sama sekali |
| Katalog pemeriksaan beserta tarif dan cakupan | **Rilis 1** | Dibutuhkan sejak layar pemesanan pertama |
| Monitoring per disiplin | **Rilis 1** | Tiga daftar sejajar, sesuai `LAB-DEC-025` |
| Penanda Duplo | **Rilis 1** | Melekat pada pemeriksaan, sekalian dengan Cito |
| Penanda Definitif pada Mikrobiologi | Rilis 2 | Maknanya belum diputuskan, lihat `LAB-OPEN-017` |
| Nota Lab, Label Lab, Label Golongan Darah | ~~Rilis 2~~ → **Rilis 1** | **Diamandemen `LAB-DEC-075`, 2026-09-17.** Seluruh datanya ternyata sudah berdiri, sehingga biayanya mendekati nol dan alasan penundaan tidak lagi sebanding |
| Kirim hasil ke pasien | Rilis 2 | Menyentuh privasi, perlu keputusan tersendiri |
| Ekspor Excel daftar order dan nilai kritis | Rilis 2 | Kenyamanan, bukan prasyarat |
| Laporan operasional laboratorium | Rilis 2 | Sebelas jenis laporan; besar dan tidak memblokir alur |
| Penautan hasil laboratorium eksternal berupa berkas PDF | Rilis 2 | Bergantung `LAB-COORD-002` |
| Order dari MCU | Rilis 3 | Sumber pesanan tambahan; MCU belum dibahas sama sekali |

**Yang tetap di luar scope modul:** Bank Darah, dan seluruh kemampuan Quality Control serta
pemantauan beban kerja alat yang terlihat pada perangkat lunak pihak ketiga.

### BR-27 — Baseline alur ujung ke ujung (`LAB-DEC-031`)

Alur berikut diadopsi sebagai baseline resmi modul, mengikuti kesimpulan analisis konsolidasi:

> Registrasi dan Pemesanan → Monitoring → Penerimaan Wadah → Pemeriksaan → Pengisian Hasil
> sesuai disiplin → Validasi, Rilis, dan Otorisasi → Nilai Kritis dan Komunikasi bila
> diperlukan → Riwayat dan Distribusi Hasil → Pelaporan → Penautan Hasil Eksternal.

**Peringatan yang wajib dibawa.** Analisis konsolidasi menyatakan sendiri bahwa baseline ini
**belum boleh dijadikan kontrak implementasi final** sebelum delapan hal berikut diputuskan:
matriks kewenangan per peran, urutan status resmi, aturan pembatalan dan koreksi hasil, alur
dan batas waktu nilai kritis, integrasi teknis alat laboratorium, kebijakan jejak audit, aturan
tagihan dan cakupan, serta mekanisme penyelarasan antaraplikasi.

Kedelapannya dicatat sebagai `LAB-P0-001` sampai `LAB-P0-008` pada bagian Open Questions.

### ~~BR-33~~ — Qty adalah alat bantu layar, bukan kolom penyimpanan (`LAB-DEC-038`)

> ## ⛔ DICABUT `LAB-DEC-050` pada 2026-09-14 — jangan dipakai
>
> Seluruh isi bagian ini **tidak lagi berlaku**. `BE-LAB-23` membuktikan `LabExamination`
> memiliki index unik `(SpecimenId, ProcedureId)` di tingkat database, dipasang atas dasar
> `BR-20` dan `AC-35` pada 2026-09-01. Qty bernilai lebih dari satu untuk jenis pemeriksaan yang
> sama pada wadah yang sama karena itu **mustahil**.
>
> **Contoh Glukosa di bawah ini keliru**: Glukosa Puasa dan Glukosa 2 Jam PP adalah dua
> `MstProcedure` yang berbeda, sehingga petugas memilih dua butir katalog dan Qty tidak
> diperlukan.
>
> Yang berlaku sekarang adalah **BR-45**: kolom Jumlah tidak dibuat sama sekali.
>
> Isinya dipertahankan apa adanya sebagai riwayat keputusan, bukan sebagai aturan.

**Menutup `LAB-GAP-001`.** Mempertegas `LAB-DEC-024` dan `LAB-DEC-027`, tidak melemahkannya.

**Aturan:**

1. Kolom **`Jumlah/Qty`** ada di layar Penerimaan Sampling/Specimen sebagai **alat bantu
   pengisian cepat**. Petugas tidak perlu memilih pemeriksaan yang sama tiga kali.
2. Saat disimpan, Qty **memecah diri**. Qty bernilai 3 menghasilkan **tiga baris**
   `LabExamination`, masing-masing dengan salinan tarifnya sendiri, statusnya sendiri, dan
   tempat hasilnya sendiri.
3. **Tidak ada kolom Qty** pada tabel mana pun. Yang tersimpan adalah jumlah barisnya.
4. Penguncian yang dimaksud `RULE-025` karena itu berarti **baris tidak dapat ditambah atau
   dihapus**, bukan sebuah kolom berubah menjadi baca-saja.
5. `IsDuplo` **tidak** digantikan Qty. Keduanya berbeda maksud: Qty adalah beberapa
   pemeriksaan yang masing-masing menghasilkan angkanya sendiri, sedangkan duplo adalah satu
   pemeriksaan yang diulang untuk meyakinkan satu angka yang sama (`LAB-DEC-026`).

**Contoh yang menjelaskan kenapa begini:**

> Pasien diminta Glukosa puasa **dan** Glukosa 2 jam setelah makan. Petugas mengetik Qty 2
> pada Glukosa. Yang tersimpan adalah dua baris pemeriksaan.
>
> Dua jam kemudian analis mengisi angka pertama 96 mg/dL, lalu angka kedua 143 mg/dL. Kedua
> angka punya barisnya sendiri, waktu pemeriksaannya sendiri, dan penilaian normal/tidaknya
> sendiri.
>
> Seandainya Qty disimpan sebagai satu kolom pada satu baris, angka 143 tidak punya tempat.
> Yang paling mungkin terjadi di lapangan: analis menimpa angka 96, dan hasil puasa pasien
> hilang tanpa jejak.

**Akibat pada uang.** `AC-37` tetap berlaku apa adanya — satu wadah yang dinyatakan layak
menerbitkan kelayakan tagih sebanyak baris pemeriksaan yang ditopangnya. Dua baris Glukosa
menerbitkan dua kelayakan tagih, masing-masing dengan tarifnya sendiri. Laboratorium tetap
tidak menghitung tagihan; ia hanya mengirim faktanya.

**Yang tidak berubah.** `LAB-DEC-027` tetap utuh: satu baris pemeriksaan menampung satu bentuk
hasil.

### BR-34 — Titik kunci daftar pemeriksaan berbeda menurut jalur masuknya (`LAB-DEC-039`)

**Mengamandemen `AC-20`.** `BR-11` dan `LAB-DEC-015` **tidak dicabut**; yang diperjelas adalah
bahwa titik kuncinya tidak sama untuk kedua jalur masuk pesanan.

**Masalah yang ditemukan.** `AC-20` mengunci daftar pemeriksaan saat sampel pertama berstatus
`Collected`. Aturan itu benar untuk pesanan dokter dari poliklinik: dokter memesan lebih dulu,
petugas mengambil darah kemudian, dan setelah jarum masuk daftar tidak boleh berubah.

Pada jalur penerimaan sampling, urutannya terbalik. **Sampel sudah di tangan sejak detik
pertama** — pasien rujukan membawanya sendiri, atau sampel dikirim rumah sakit perujuk. Bila
`AC-20` diterapkan apa adanya, daftar pemeriksaan terkunci sebelum petugas sempat memilih satu
pun pemeriksaan.

**Aturan:**

| Jalur masuk | Daftar pemeriksaan terkunci saat |
|---|---|
| Pesanan dokter dari poliklinik, rawat inap, atau IGD | Sampel pertama berstatus `Collected` — tidak berubah, sesuai `AC-20` |
| **Penerimaan sampling/specimen langsung di laboratorium** | **Kelayakan wadah ditetapkan** `Layak` atau `Tidak Layak` |

1. Aksi **`Pemeriksaan Diproses`** yang disebut `RULE-026` pada `LAB-EVD-001` **dipetakan ke
   aksi penetapan kelayakan yang sudah ada**. Tidak ada tombol ketiga yang dibuat.
2. Selama kelayakan belum ditetapkan, petugas bebas menambah, mengubah jumlah, dan menghapus
   baris pemeriksaan.
3. Sesudah kelayakan ditetapkan, daftar terkunci: baris tidak dapat ditambah maupun dihapus.
4. Wadah yang ditetapkan `Tidak Layak` menghentikan proses sesuai `RULE-020`; daftarnya tetap
   terkunci, dan jalan keluarnya adalah pengambilan ulang menurut `LAB-INH-005`, bukan
   menyunting daftar.

**Kenapa kelayakan, bukan tombol tersendiri.**

> Pada detik wadah dinyatakan `Layak`, `AC-37` menerbitkan kelayakan tagih sebanyak baris
> pemeriksaan yang ditopangnya, masing-masing dengan tarifnya sendiri. Fakta itu berangkat ke
> Billing dan tidak ditarik kembali.
>
> Bila daftar pemeriksaan masih bisa berubah sesudahnya, rumah sakit akan menagih pemeriksaan
> yang sudah dihapus, atau gagal menagih pemeriksaan yang baru ditambahkan. Titik kunci karena
> itu harus jatuh **persis** di kejadian yang menerbitkan uang, bukan pada tombol terpisah yang
> bisa lupa ditekan.

**Contoh:**

> Sampel darah Ibu Sari datang dari Klinik Sehat Sentosa pukul 08.10. Petugas mencatat
> wadahnya, lalu memilih Hemoglobin, Leukosit, dan Trombosit. Ia melihat tabungnya terlalu
> sedikit untuk tiga pemeriksaan, jadi Trombosit dihapus — masih boleh, karena kelayakan belum
> ditetapkan.
>
> Pukul 08.15 petugas menetapkan `Layak`. Dua kelayakan tagih terbit. Sejak detik itu daftar
> terkunci; permintaan menambah Trombosit ditolak sistem, dan bila memang diperlukan, yang
> ditempuh adalah pesanan baru dengan wadah baru.

### BR-35 — Jenis specimen menjadi data induk terkendali dengan jalan keluar yang terpantau (`LAB-DEC-040`)

**Menerapkan `LAB-DEC-034`.** Menegakkan alasan `LAB-DEC-035` tanpa menciptakan jalan buntu
yang sama seperti `VAL-43`.

**Keadaan sekarang.** `LabSpecimen.SpecimenDescription@466a7127` adalah teks bebas, contohnya
`"Darah vena"` atau `"Urin pagi"`.

**Batas terhadap `LAB-DEC-001`.** Keputusan itu menaruh "jenis sampel, wadah, volume minimal,
metode, paket/panel" di Rilis 2 sebagai bagian **katalog pemeriksaan mandiri** — yaitu
pertanyaan "pemeriksaan Hemoglobin membutuhkan sampel apa". Yang diputuskan di sini adalah
tingkat **transaksi** — "wadah yang datang pagi ini berisi apa". Keduanya berbeda, sehingga
`LAB-DEC-001` tidak dilanggar dan Rilis 2 tidak ditarik maju.

**Aturan:**

1. **Jenis specimen menjadi data induk milik Laboratorium**, diletakkan di folder Laboratorium
   sesuai `LAB-DEC-034` karena pemakaiannya khusus laboratorium.
2. Nilai awalnya tujuh, diambil dari `LAB-EVD-001`: Blood, Urine, Body Fluid, Sputum, Pus,
   Jaringan, dan Lainnya.
3. Pilihan **`Lainnya` tetap ada** dan **wajib** disertai keterangan. Sampel yang jenisnya
   belum terdaftar **tidak boleh** menghalangi penerimaan pasien.
4. Setiap pemakaian `Lainnya` beserta keterangannya masuk **daftar pantau** kepala instalasi.
5. Kepala instalasi dapat **menaikkan** keterangan yang sering muncul menjadi nilai tetap pada
   data induk.
6. `SpecimenDescription` **tidak dihapus**. Ia turun pangkat menjadi keterangan operasional
   bebas — misalnya "lengan kiri, tabung kedua" — dan berhenti menjadi tempat menyimpan jenis.

**Kenapa tidak ditutup rapat.**

> Menutup `Lainnya` sama sekali memang paling bersih bagi laporan. Tetapi akibatnya di
> lapangan: sampel cairan kista datang pukul 21.00, jenisnya belum ada di daftar, dan petugas
> tidak dapat menerimanya sampai kepala instalasi menambahkannya keesokan hari. Sampelnya
> sendiri tidak menunggu — ia rusak.
>
> Itu jalan buntu yang sama seperti `VAL-43` pada instansi perujuk, dengan satu perbedaan yang
> memberatkan: di sini yang tertahan adalah bahan yang sudah terlanjur diambil dari tubuh
> pasien.

**Kenapa tidak dibiarkan bebas.**

> Tanpa pemantauan, `Lainnya` akan menjadi tempat pembuangan. Dalam enam bulan kolom
> keterangannya memuat "cairan kista", "Cairan Kista", "c. kista", dan "kista" sebagai empat
> hal yang berbeda. Daftar pantau membuat kepala instalasi melihat bahwa keempatnya sebenarnya
> satu, lalu menaikkannya menjadi satu nilai tetap.

### BR-36 — Volume specimen selalu membawa satuannya (`LAB-DEC-041`)

**Menerima `RULE-021` sebagian.** Ketiadaan batas minimum dan maksimum **diterima apa adanya**
sebagai keputusan bisnis yang sah. Ketiadaan satuan **tidak** diterima.

**Aturan:**

1. Volume tersimpan sebagai **dua bagian**: angka, dan satuan yang dipilih dari daftar pendek.
2. Daftar satuan awalnya: `mL`, `µL`, `gram`, `blok`, dan `slide`.
3. **Tidak ada batas minimum maupun maksimum.** Petugas bebas mengisi berapa pun, sesuai
   `RULE-021`. Sistem tidak menolak volume yang kecil, dan tidak menghitung sendiri apakah
   sampelnya cukup.
4. Penilaian cukup atau tidaknya sampel tetap menjadi kewenangan petugas lewat penetapan
   kelayakan (`RULE-020`), bukan lewat perbandingan angka oleh sistem.
5. Batas volume minimal **per jenis pemeriksaan** tetap berada di Rilis 2 sebagai bagian
   katalog mandiri (`LAB-DEC-001`), dan tidak ditarik maju oleh keputusan ini.

**Kenapa satuan tidak boleh dilewatkan.**

> Angka `5` yang tersimpan sendirian tidak dapat dibaca ulang oleh siapa pun. Lima mililiter
> cukup untuk hitung darah lengkap; lima mikroliter tidak cukup untuk apa pun. Yang membuka
> catatan itu enam bulan kemudian tidak punya cara membedakannya, dan satu-satunya orang yang
> tahu maksudnya adalah petugas yang mengetiknya — yang ketika itu sudah tidak ingat.

**Kenapa daftar satuannya lebih dari mililiter.**

> `LAB-DEC-025` memasukkan Patologi Anatomi ke dalam cakupan, dan `LAB-DEC-040` mengesahkan
> Jaringan sebagai jenis specimen. Jaringan tidak diukur dalam mililiter. Bila satuannya
> dipaksa selalu mililiter, petugas akan mengisi angka yang tidak bermakna semata-mata supaya
> formulirnya bisa disimpan — dan angka yang tidak bermakna itu tetap tersimpan selamanya
> sebagai kalau-kalau data yang benar.

### BR-37 — Waktu sistem dan waktu nyata disimpan berdampingan (`LAB-DEC-042`)

**Menerima `RULE-019` untuk bagian specimen.** Bagian Tanggal Registrasi **tidak** diputuskan
di sini — itu milik Registrasi menurut `LAB-DEC-032` dan sudah dikeluarkan oleh `LAB-DEC-037`.

**Aturan:**

1. `ReceivedAt` **tetap diisi server** dan **tidak dapat diubah siapa pun**. Maknanya
   dipertegas: kapan datanya masuk ke sistem.
2. Ditambahkan satu waktu baru: **kapan specimen benar-benar diterima secara fisik**, diisi
   petugas.
3. Waktu nyata itu **tidak boleh berada di masa depan** dan **tidak boleh mendahului waktu
   pengambilan** specimen.
4. Laporan penerimaan dan pertanyaan "sampel ini sudah berapa lama di laboratorium" memakai
   **waktu nyata**.
5. Keduanya tersimpan dan selisihnya terlihat, sehingga keterlambatan pencatatan dapat
   ditelusuri tanpa menuduh siapa pun.
6. Perhitungan keterlambatan cito **tidak berubah** — `AC-17` tetap menghitungnya dari sampel
   `Accepted` sampai hasil `Released`.

**Kenapa fakta sistem tidak ditimpa.**

> Waktu yang diisi manusia adalah keterangan; waktu yang dicatat sistem adalah fakta. Keduanya
> berguna, dan keduanya menjawab pertanyaan yang berbeda. Bila yang satu menimpa yang lain,
> yang hilang justru kemampuan menjawab pertanyaan paling wajar di kemudian hari: apakah
> sampel ini terlambat dicatat, dan berapa lama.

**Arah penyimpangan yang perlu dicatat.** Berbeda dari kebanyakan pemunduran tanggal,
pemunduran di sini **merugikan laboratorium sendiri**. Sampel yang datang pukul 21.00 dan baru
dicatat pukul 08.00 keesokan hari akan terlihat menunggu **sebelas jam lebih lama** bila waktu
nyatanya diisi jujur. Itu justru alasan untuk mempercayainya — tidak ada yang diuntungkan dari
memundurkannya.

**Contoh:**

> Sampel dari Klinik Sehat Sentosa tiba di meja penerimaan pukul 21.10 hari Senin. Laboratorium
> sudah tutup untuk layanan umum, dan registrasinya dikerjakan Selasa pukul 08.05.
>
> Yang tersimpan: waktu nyata Senin 21.10, waktu sistem Selasa 08.05. Laporan penerimaan
> menempatkan sampel itu pada hari Senin. Kepala instalasi yang membaca selisih sebelas jam
> tahu bahwa itu bukan kelalaian, melainkan jam operasional — dan bila suatu hari selisih itu
> muncul pada sampel yang datang pukul 10.00, ia punya dasar untuk bertanya.

### BR-38 — Laboratorium mengusulkan instansi perujuk, Master Data yang mengesahkan (`LAB-DEC-043`)

**Menjawab `CAP-008` dan `RULE-014` pada `LAB-EVD-001`.** `LAB-DEC-035` butir 4 dan `AC-50`
**tidak dicabut** — Laboratorium tetap tidak memiliki data induk instansi perujuk.

**Masalah yang diselesaikan.** `VAL-43` menolak dengan `422` dan menyuruh petugas menghubungi
bagian data induk. Di meja penerimaan, artinya sampel sudah datang, pasien sudah menunggu, dan
petugas menelepon bagian yang mungkin sedang tidak di tempat — termasuk di luar jam kerjanya.

**Aturan:**

1. Dari layar penerimaan, petugas lab dapat **mengirim usulan** instansi perujuk baru berisi
   nama, alamat, dan telepon.
2. Usulan itu **langsung menjadi satu baris pada data induk** dengan status **menunggu
   persetujuan**. Ia **bukan** teks bebas, dan bukan tabel bayangan milik Laboratorium.
3. Kunjungan dapat **menunjuk** baris berstatus menunggu itu, sehingga penerimaan specimen
   berjalan terus tanpa tertahan.
4. **Master Data** yang menyetujui usulan, atau **menggabungkannya** dengan baris yang sudah
   ada bila ternyata institusinya sama.
5. Sebelum mengirim usulan, layar **wajib** memperlihatkan hasil pencarian daftar yang sudah
   ada, supaya usulan kembar tidak lahir dari kemalasan mencari.
6. Laboratorium **tetap tidak dapat** menyetujui usulannya sendiri, mengubah baris yang sudah
   disetujui, atau menetapkan status PKS.

**Kenapa usulan berstatus, bukan teks bebas.**

> Alasan `LAB-DEC-035` tidak gugur sedikit pun: bila namanya diketik bebas, Klinik Sehat
> Sentosa akan tercatat tiga kali dengan tiga ejaan, dan kerja sama dengan klinik itu tidak
> pernah terlihat nilainya.
>
> Usulan berstatus menghindarinya dengan cara lain: sejak detik pertama ia sudah **satu baris**
> yang dapat ditunjuk, dicari, dan digabungkan. Yang ditunda hanya pengesahannya, bukan
> identitasnya.

**Akibat pada pelaksanaan.** Status baru pada data induk instansi perujuk dan layar persetujuan
adalah **pekerjaan Master Data**, bukan Laboratorium. Diperlukan kesepakatan dengan pemiliknya.
Dicatat sebagai `LAB-COORD-006`.

**Yang masih harus diputuskan Master Data,** dan sengaja tidak diputuskan di sini: berapa lama
usulan boleh menggantung, apa yang terjadi pada kunjungan bila usulan akhirnya **ditolak**, dan
siapa yang berhak menggabungkan dua baris. Dicatat sebagai `LAB-OPEN-024`.

### BR-39 — Metode pembayaran dibaca dari Billing, bukan disimpulkan Laboratorium (`LAB-DEC-044`)

**Menjawab `CAP-007` dan `RULE-012` sampai `RULE-013` pada `LAB-EVD-001`.** Menerapkan `BR-25`,
tidak melemahkannya.

**Bedanya tipis dan penting.** `BR-25` melarang Laboratorium **memutuskan apakah pasien
membayar**. Ia tidak melarang Laboratorium **menampilkan** apa yang sudah diputuskan pihak
lain. Yang dilarang adalah menjalankan aturannya, bukan memperlihatkan hasilnya.

**Aturan:**

1. Layar penerimaan **memanggil Billing** dengan penunjuk kunjungan, lalu menampilkan jawaban
   yang dikembalikan — `Piutang Mitra`, `Tunai`, atau bentuk lain yang Billing kenal — beserta
   alasan singkatnya.
2. Tampilannya **baca-saja**. Tidak ada kotak pilihan, dan tidak ada cara menimpanya dari
   layar Laboratorium.
3. Laboratorium **tidak mengirim apa pun yang baru** ke Billing untuk keperluan ini. Kunjungan
   sudah menunjuk instansi perujuknya sejak `LAB-DEC-035`, dan kelayakan tagih sudah mengalir
   sejak `AC-37`.
4. Laboratorium **tidak membaca** penanda PKS dan **tidak menyimpulkan** sendiri. Penanda itu
   masukan bagi Billing, bukan bagi Laboratorium.
5. **Bila Billing tidak dapat dihubungi**, layar menulis *belum dapat ditentukan* dan
   **penerimaan specimen tetap berjalan**. Kegagalan membaca metode pembayaran tidak boleh
   menahan sampel.

**Kenapa penerimaan tetap jalan saat Billing diam.**

> Uang bukan syarat menerima sampel. Sampel yang tertahan akan rusak, dan pasien rujukan dari
> luar kota tidak dapat diminta datang lagi besok. Metode pembayaran dapat ditanyakan ulang
> kapan saja; darah yang sudah lisis tidak dapat.
>
> Ini pilihan **fail-open yang disengaja**, dan aman justru karena Laboratorium tidak memegang
> wewenang finansial apa pun. Tidak ada tagihan yang salah terbentuk akibat layar gagal membaca
> — yang terjadi hanya petugas tidak dapat memberi tahu pasien lebih awal.

**Kenapa tidak menyimpulkan dari penanda PKS.**

> Bila Laboratorium membaca penanda PKS lalu menyimpulkan sendiri, rumah sakit akan punya dua
> tempat yang menjalankan aturan uang yang sama. Ketika Billing suatu hari menambah syarat —
> misalnya plafon kerja sama yang sudah habis — layar laboratorium akan tetap menjawab dengan
> aturan lamanya, dan pasien mendengar dua jawaban berbeda di dua loket pada hari yang sama.

**Akibat pada pelaksanaan.** Endpoint baca metode pembayaran per kunjungan adalah **pekerjaan
Billing**, bukan Laboratorium. Diperlukan kesepakatan dengan pemiliknya. Dicatat sebagai
`LAB-COORD-007`.

**Yang sengaja tidak diputuskan di sini:** penerimaan uang tunai di laboratorium
(`RULE-018` pada `LAB-EVD-001`). Itu tetap di luar scope menurut `LAB-DEC-037`, dan tidak
berubah oleh keputusan ini.

### BR-40 — Penerimaan Sampling/Specimen berdiri sebagai menu tersendiri (`LAB-DEC-045`)

**Keputusan produk, bukan tata letak.** `LAB-FE-002` tetap berlaku: bentuk, tata letak, dan
pemilihan komponen di dalam layar tetap `DEV_DISCRETION`. Yang diputuskan di sini **jumlah
menunya**, bukan rupanya.

**Aturan:**

1. **Menu `Penerimaan Sampling/Specimen` berdiri sendiri**, melayani pasien rujukan luar dan
   pasien datang langsung. Satu rangkaian: identifikasi pasien, data pemeriksaan, pencatatan
   specimen, pemilihan pemeriksaan, sampai penetapan kelayakan.
2. Layar `lab-orders` dan layar specimen per pesanan yang sudah ada **tetap dipertahankan**,
   melayani pesanan dokter dari poliklinik, rawat inap, dan IGD.
3. Kedua jalur memakai **data dan aturan yang sama**. Yang berbeda hanya urutan penyajiannya
   dan titik kuncinya, sesuai `LAB-DEC-039`.
4. Menu baru ini **tidak** membuat pola penamaan route baru; ia mengikuti `LAB-FE-001`.

**Kenapa satu rangkaian, bukan tiga layar.**

> `LAB-DEC-039` menaruh titik kunci pada penetapan kelayakan. Aturan itu hanya masuk akal bila
> petugas melihat daftar pemeriksaan dan wadahnya berdampingan: ia memilih tiga pemeriksaan,
> melihat tabungnya hanya cukup untuk dua, menghapus satu, lalu menetapkan `Layak`.
>
> Bila ketiganya terpisah di tiga layar, penetapan kelayakan terjadi di layar terakhir —
> jauh dari tempat daftar pemeriksaan dipilih. Penguncian akan terasa datang tiba-tiba, dan
> petugas baru menyadarinya ketika sudah tidak bisa diperbaiki.

**Kenapa layar lama tidak dihapus.**

> Jalur pesanan dokter punya titik kunci yang berbeda — sampel pertama `Collected`, bukan
> penetapan kelayakan. Memaksa satu layar melayani dua titik kunci yang berbeda akan melahirkan
> layar yang perilakunya berubah-ubah tanpa alasan yang terlihat pengguna.

**Akibat yang harus disadari.** Ada **dua jalan** menuju hal yang mirip. Dokumen pelatihan dan
penamaan menunya harus membuat jelas siapa memakai yang mana, agar petugas tidak memilih jalur
yang salah lalu bingung mengapa tombolnya berbeda. Dicatat sebagai `LAB-FE-009`.

### BR-41 — Piutang mitra menjadi penjamin kunjungan tersendiri (`LAB-DEC-046`)

**Menutup sebagian `LAB-CONFLICT-003`.** Menjawab `Q-LAB-06` pada capability map revision 3.

**Keadaan yang ditemukan impact scan.**
`Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs@466a7127` hanya
mengenal tiga nilai, dan tidak satu pun mewakili piutang kepada rumah sakit perujuk:

| Nilai | Label | Artinya sekarang |
|---|---|---|
| `Cash = 1` | Tunai | Pasien membayar sendiri |
| `Insurance = 2` | Asuransi | Ditanggung asuransi |
| `CompanyGuarantor = 3` | Penjamin Perusahaan | Ditanggung **tempat pasien bekerja** |

**Aturan:**

1. `EncounterPaymentType` bertambah **satu nilai baru** untuk piutang mitra, ditambahkan
   **secara aditif**. `Cash`, `Insurance`, dan `CompanyGuarantor` **tidak boleh bergeser**.
2. Rumah sakit perujuk ber-PKS menjadi **penjamin kunjungan tersendiri**, sejajar dengan
   asuransi dan penjamin perusahaan — bukan dititipkan pada `CompanyGuarantor`.
3. Enum itu **milik Registrasi**. Laboratorium hanya memakainya, tidak memiliki dan tidak
   menambah nilainya sendiri.
4. Penetapan **status PKS** rumah sakit perujuk tetap di luar scope Laboratorium sesuai
   `LAB-DEC-037`.

**Preseden yang menguatkan.** Comment pada enum itu mencatat `CompanyGuarantor = 3` **sendiri**
ditambahkan lewat `BE-RWI-035` secara aditif, dengan peringatan agar dua nilai sebelumnya tidak
bergeser. Penambahan keempat mengikuti cara yang sama, bukan membuat pola baru.

**Kenapa tidak dititipkan pada `CompanyGuarantor`.**

> Penjamin perusahaan menjawab pertanyaan "di mana pasien ini bekerja". Piutang mitra menjawab
> pertanyaan "siapa yang mengirim pasien ini kepada kami". Keduanya kebetulan sama-sama berarti
> tagihannya tidak dibayar pasien di loket — tetapi sampai di situ saja kesamaannya.
>
> Bila disatukan, laporan penjamin perusahaan akan memuat PT tempat pasien bekerja dan Klinik
> Sehat Sentosa dalam satu daftar, seolah-olah keduanya hal yang sama. Rumah sakit kemudian
> tidak dapat menjawab dua pertanyaan yang berbeda-beda pemakainya: berapa besar tagihan
> korporat dari perusahaan mitra, dan berapa nilai kerja sama dengan klinik perujuk.

**Akibat pada pelaksanaan.** Perubahan enum berdampak pada **setiap modul** yang membaca
penjamin kunjungan, dan tata kelolanya (`RWI-ENC-PAYER-001`) dipegang Registrasi. Diperlukan
kesepakatan dengan pemilik `registration-management` dan `billing-kasir`. Ini yang dimaksud
`LAB-COORD-007` setelah dirumuskan ulang.

**Yang sengaja tidak diputuskan di sini:** nama literal nilai barunya, bagaimana Billing
menagihkan piutang mitra, dan bagaimana status PKS diperbarui. Ketiganya milik modul lain.

### BR-42 — Metode pembayaran diturunkan untuk rujukan, dinyatakan untuk datang langsung (`LAB-DEC-047`)

**Menutup sisa `LAB-CONFLICT-003`.** Menjawab `Q-LAB-07`. **Mengamandemen `LAB-DEC-044`**, yang
dasar faktualnya dibantah impact scan 2026-09-14.

**Apa yang salah pada `LAB-DEC-044`.** Keputusan itu menganggap metode pembayaran harus
**dibaca dari Billing** lewat endpoint baru. Kenyataannya tiga hal berbeda:

| Anggapan `LAB-DEC-044` | Kenyataan pada `466a7127` |
|---|---|
| Metode pembayaran milik Billing | Milik **Registrasi** — `RegPatientEncounterGuarantor` |
| Laboratorium perlu membacanya | Laboratorium sudah **mengirimkannya** — `LabPatientRegistrationDtos.cs:80,83,120,122` |
| Jawabannya `Piutang Mitra` atau `Tunai` | `Piutang Mitra` **belum ada**; baru dibuka `LAB-DEC-046` |

**Aturan:**

| Jalur masuk | Metode pembayaran |
|---|---|
| **Rujukan luar** | **Diturunkan** dari status PKS instansi perujuk. Ditampilkan **baca-saja**; petugas tidak memilih apa pun |
| **Datang langsung** | **Dinyatakan petugas**, karena tidak ada instansi perujuk yang dapat menurunkannya |

1. `PaymentType` dan `PaymentMethodId` **tetap ada** pada permintaan pendaftaran, tetapi
   maknanya dipersempit: keduanya hanya sah untuk jalur **datang langsung**.
2. Pada jalur **rujukan luar**, keduanya **diabaikan** bila dikirim. Laboratorium tidak dapat
   menimpa kesimpulan yang diturunkan dari status PKS.
3. Registrasi tetap yang **memvalidasi dan berhak menolak**, persis seperti `LAB-DEC-032`.
   Laboratorium meneruskan, tidak memutuskan.
4. Penurunan dari status PKS dikerjakan **Registrasi**, bukan Laboratorium. Laboratorium tidak
   membaca penanda PKS — `AC-73` tetap berlaku.

**Kenapa menyatakan penjamin bukan pelanggaran `BR-25`.**

> `BR-25` melarang Laboratorium **memutuskan apakah pasien membayar**. Menyatakan "pasien ini
> membawa kartu asuransi X" bukan keputusan itu — itu **mencatat identitas penjamin**, dan
> sahih atau tidaknya diperiksa Registrasi yang memang memiliki aturannya.
>
> Polanya sama persis dengan `LAB-DEC-032`: layar pendaftaran milik Laboratorium, tetapi yang
> membuat kunjungan dan menjalankan aturannya tetap Registrasi. Bila Registrasi menolak,
> penolakan itu diteruskan apa adanya.

**Kenapa jalur rujukan tidak boleh dipilih petugas.**

> Bila petugas lab dapat memilih sendiri, ia dapat memilih piutang mitra untuk klinik yang
> tidak pernah menandatangani PKS. Rumah sakit kemudian menagih pihak yang tidak pernah
> berjanji membayar, dan yang menanggung selisihnya adalah rumah sakit sendiri.
>
> Status PKS adalah fakta yang sudah tercatat. Yang diturunkan dari fakta tidak perlu — dan
> tidak boleh — ditanyakan ulang kepada orang yang tidak memegang faktanya.

**Kenapa tidak dicabut seluruhnya.**

> `LAB-DEC-028` memberi Laboratorium jalur pendaftaran sendiri supaya pasien yang hanya perlu
> satu pemeriksaan darah tidak mengantre di loket lebih dulu. Bila kemampuan menyatakan
> penjamin dicabut seluruhnya, pasien datang langsung yang membawa kartu asuransi tetap harus
> ke loket — dan jalur itu hanya berguna untuk separuh pasiennya.

**Akibat pada pelaksanaan.** `RegisterLabWalkInRequest` dan `RegisterLabExternalReferralRequest`
**tidak dipangkas**, tetapi aturan pemakaiannya berbeda per jalur. `FE-LAB-05` perlu penyesuaian
sebagian: ruas metode pembayaran pada formulir rujukan luar berubah menjadi tampilan baca-saja.

### BR-43 — Susunan menu Laboratorium dirapikan, disiplin berhenti ditanyakan (`LAB-DEC-048`)

**Mengamandemen `LAB-DEC-045`, `AC-76`, dan `LAB-FE-009`.** Arahan pemilik modul 2026-09-14,
setelah melihat layar yang sudah berjalan.

> **Catatan cara kerja yang harus jujur.** Perubahan ini **dikerjakan lebih dulu di frontend**
> atas arahan langsung pemilik modul, lalu dicatat di sini. Ia **tidak** melewati task roadmap
> `FE-LAB-*` seperti perubahan lain. Dicatat apa adanya agar tidak terbaca seolah lahir dari
> perencanaan.

**Aturan:**

| No | Yang berubah | Keadaan sebelumnya |
|---:|---|---|
| 1 | Butir menu **Pesanan Laboratorium dicabut** dari sidebar | Menu tersendiri menuju daftar pesanan |
| 2 | Tiga menu **Monitoring** dinamai ulang menjadi **Pemeriksaan** Patologi Klinik, Patologi Anatomi, dan Mikrobiologi | Berawalan "Monitoring" |
| 3 | Empat penyaring dicabut: jenis kunjungan pasien, unit layanan, status pesanan, dan status wadah | Sebelas penyaring |
| 4 | Penyaring **Kesegeraan** disederhanakan menjadi `Semua Data` dan `Cito` | "Semua Kesegeraan" dan "Hanya Memuat Cito" |
| 5 | Dua tombol pindah disiplin dicabut; tersisa **Muat ulang** | Tiga tombol pada kepala layar |
| 6 | **Disiplin pesanan diturunkan dari pemeriksaan yang dipilih**, tidak lagi dipilih petugas | Kotak pilihan tersendiri yang boleh dikosongkan |
| 7 | Baris pada ketiga layar Pemeriksaan **dapat dibuka** ke detail pesanannya | Tidak ada jalan masuk |

**Yang tetap berlaku dari `LAB-DEC-045`.** Layar `lab-orders`, `lab-orders/create`,
`lab-orders/[slug]`, dan `lab-orders/[slug]/specimens` **tidak dihapus dan tidak berubah
perilakunya**. Yang dicabut hanya butir menunya di sidebar. Jalan masuknya berpindah: pembuatan
pesanan dituju dari pendaftaran pasien laboratorium, dan detail pesanan dituju dari ketiga layar
Pemeriksaan.

**Kenapa butir 6 bukan sekadar penyederhanaan tampilan.**

> Sebelumnya disiplin adalah kotak pilihan tersendiri yang **boleh dikosongkan**. Pesanan yang
> disiplinnya kosong tidak pernah muncul di menu Patologi Klinik, Patologi Anatomi, maupun
> Mikrobiologi — pasiennya tersimpan dengan benar, tetapi **hilang dari layar yang justru
> dipakai petugas mengerjakannya**.
>
> Menurunkannya dari katalog juga menegakkan `LAB-DEC-036`: disiplin memang sudah melekat pada
> jenis pemeriksaan. Menanyakannya lagi kepada petugas berarti meminta ia mengulang jawaban
> yang sudah ada di data — dan memberi ia kesempatan menjawab keliru.

**Kenapa butir 7 wajib ada, bukan tambahan yang enak dimiliki.**

> Penelusuran menemukan tabel pada ketiga layar Pemeriksaan **tidak punya aksi buka**, dan
> Daftar Kerja pun tidak. Satu-satunya jalan ke detail pesanan adalah menu Pesanan Laboratorium
> yang dicabut butir 1.
>
> Tanpa butir 7, butir 1 akan membuat pasien muncul di menu yang benar tetapi **tidak dapat
> dibuka** — daftar yang hanya bisa dipandang. Keduanya karena itu satu keputusan, bukan dua.

**Akibat pada acceptance criteria.** `AC-76` diamandemen: yang dijanjikan tetap adalah
**perilaku layarnya**, bukan keberadaan butir menunya.

### BR-44 — Penguncian hanya berlaku pada penambahan, bukan pembatalan (`LAB-DEC-049`)

**Mengamandemen `AC-55` dan `AC-57`.** `BR-34` tetap berlaku untuk bagian penambahan.

**Kenapa diamandemen.** `BE-LAB-24` memeriksa jalur hapus pada 2026-09-14 dan menemukan
`AC-55` beserta `AC-57` tidak dapat dilaksanakan tanpa mencabut keputusan milik modul lain:

| Bukti | Isi |
|---|---|
| `validation-matrix.md` | `VAL-18` bertuliskan **"Berlaku pada: Menambah pemeriksaan"** — ia memang tidak pernah mencakup pembatalan |
| `LAB-INH-001` | Alur pesanan **memuat** `Cancelled` sebagai pengecualian sah |
| `LAB-INH-006` | Sesudah `Requested`, jalur pengajuan pembatalan **ada dan diatur**, bukan ditutup |
| `LAB-INH-010` | Billing satu-satunya pemilik akibat finansial |

**Aturan:**

1. Penguncian daftar pemeriksaan pada penetapan kelayakan berlaku untuk **penambahan baris**.
2. **Pembatalan tetap terbuka** sesudah kelayakan ditetapkan, lewat `POST /{id}/cancel` yang
   sudah ada, dan koreksi tagihannya adalah wewenang Billing (`LAB-INH-010`).
3. `VAL-18` **tidak berubah bunyinya** dan tidak diperluas ke jalur pembatalan.

**Akibat yang harus terlihat petugas.** Membatalkan pemeriksaan sesudah wadah dinyatakan
`Layak` **tidak serta-merta membatalkan tagihannya** — kelayakan tagihnya sudah terbit, dan
koreksinya dikerjakan Billing. Layar penerimaan wajib mengatakan itu pada saat pembatalan
ditekan, bukan membiarkannya menjadi kejutan di loket.

### BR-45 — Kolom Jumlah tidak dibuat; `LAB-DEC-038` dicabut (`LAB-DEC-050`)

**Mencabut `LAB-DEC-038` dan `BR-33`.** Menegakkan `BR-20` dan `AC-35` yang sudah disetujui
2026-09-01.

**Kenapa dicabut.** `BE-LAB-23` menemukan `LAB-DEC-038` tidak dapat dilaksanakan.
`LabExamination` memiliki **index unik di tingkat database** atas pasangan
`(SpecimenId, ProcedureId)`, dipasang atas dasar `BR-20` dan `AC-35`. `Quantity` bernilai 3
untuk satu jenis pemeriksaan pada satu wadah karena itu mustahil — baris kedua dan ketiga
ditolak service, dan bila lolos, ditolak database.

**Contoh pada `BR-33` sendiri keliru.** Ia memakai "Glukosa puasa dan Glukosa 2 jam setelah
makan" untuk membenarkan Qty memecah baris. Pada katalog yang tergolong benar, keduanya adalah
**dua `MstProcedure` yang berbeda** — masing-masing punya kode, tarif, dan batas nilai sendiri.
Petugas memilih dua butir katalog, dan Qty tidak diperlukan sama sekali.

**Aturan:**

1. Kolom **Jumlah/Qty tidak dibuat**, baik di layar maupun di permintaan API.
2. Petugas yang memerlukan dua pemeriksaan memilih **dua butir katalog** yang berbeda.
3. `BR-20` dan `AC-35` tetap utuh; index uniknya tidak dilepas.
4. `IsDuplo` tetap menjadi satu-satunya cara menyatakan pengerjaan ganda atas satu pemeriksaan
   (`LAB-DEC-026`).

**Apa yang sebenarnya ada di artifact.** Kolom Jumlah pada `LAB-EVD-001` diperlakukan sebagai
**bawaan tampilan layar transaksi umum**, bukan kebutuhan laboratorium yang terbukti. Menolak
membangunnya lebih jujur daripada membangun kolom yang tidak punya arti yang dapat dibedakan
saat hasilnya diisi.

---

### BR-46 — Kiosk menjadi titik masuk pendaftaran laboratorium (`LAB-DEC-051` .. `LAB-DEC-054`)

**Ditulis Amendment Pass putaran 4, 2026-09-15.** Menjawab permintaan pemilik modul: pendaftaran
dimulai dari kiosk, hasilnya ditampung pendaftaran pasien laboratorium, lalu petugas lab memilih
pemeriksaan.

> **Sebagian besar kemampuannya sudah ada, dan itu perlu ditulis lebih dulu supaya tidak
> dibangun ulang.** `TrxKioskScanSession` beserta `KioskScanSessionController` sudah memindai
> identitas dan mencocokkannya ke `PatientId` — **16 sesi sudah terjadi, 15 cocok ke pasien**.
> `MstKioskDevice` sudah mencatat perangkatnya beserta `KioskDeviceType.SelfServiceKiosk` dan
> `SessionExpireMinutes`. Yang belum ada hanya **sambungan dari sesi kiosk ke pendaftaran
> laboratorium**, dan pengetahuan kiosk tentang layanan yang dituju pasien.

**Aturan:**

1. Pasien **memilih Laboratorium sendiri di kiosk**. Sesi kiosk karena itu perlu membawa tujuan
   layanannya, yang hari ini tidak dimilikinya.
2. Kiosk membedakan **dua jalur** sejak awal: pasien yang **membawa permintaan dokter**, dan
   pasien yang **memeriksakan diri sendiri**. Keduanya berbeda perlakuan sampai ke penjamin dan
   tagihan.
3. **Kunjungan terbentuk begitu pasien selesai di kiosk**, bukan menunggu petugas lab. Pasien
   langsung terlihat pada antrean dan laporan.
4. Kunjungan dari kiosk yang **tidak pernah dilanjutkan** ditutup **otomatis oleh Registrasi**
   ketika batas waktunya lewat, dengan sebab "tidak dilanjutkan".

**Kenapa butir 4 tidak bisa dikerjakan Laboratorium.** `AC-45` menyatakan Laboratorium **nol
pembentukan maupun pengubahan kunjungan**; seluruh penulisannya lewat Registrasi. Menutup
kunjungan terbengkalai karena itu wewenang Registrasi, bukan Laboratorium — dan butir ini
dicatat sebagai permintaan lintas modul, bukan pekerjaan yang dapat dijadwalkan sendiri.

**Risiko butir 3, dicatat apa adanya karena pemilik modul memilihnya setelah diberi tahu.**
Pasien yang menekan selesai lalu pergi meninggalkan **kunjungan tanpa pemeriksaan**. Ia tidak
menghasilkan tagihan pemeriksaan laboratorium — fakta tagih baru terbit saat wadah dinyatakan
layak (`AC-37`) — tetapi kunjungannya sendiri, beserta biaya pendaftaran bila ada, tetap
menggantung sampai butir 4 berjalan. Selama butir 4 belum dijawab Registrasi, **angka kunjungan
harian akan melebihi jumlah pasien yang benar-benar diperiksa.**

### BR-47 — Pesanan dipecah otomatis menurut disiplin pemeriksaannya (`LAB-DEC-055`, `LAB-DEC-056`)

**Mengamandemen `LAB-DEC-048` butir 6 dan `AC-83`.** `LAB-DEC-048` sudah menetapkan disiplin
**diturunkan** dari pemeriksaan yang dipilih, tetapi tidak pernah menjawab satu hal: apa yang
terjadi bila pemeriksaan yang dipilih berasal dari **dua disiplin sekaligus**.

**Aturan:**

1. Petugas memilih pemeriksaan **satu kali** pada satu layar, tanpa memikirkan disiplin.
2. Sistem membentuk **satu pesanan per disiplin** dari pilihan itu. Hemoglobin dan kultur darah
   yang dipilih bersamaan menghasilkan **dua pesanan** — satu Patologi Klinik, satu
   Mikrobiologi.
3. Pemeriksaan yang **belum digolongkan** disiplinnya berkumpul menjadi **satu pesanan tanpa
   disiplin**, dan itu tetap sah — `AC-85` tidak dicabut.
4. Pemecahan dikerjakan **satu endpoint baru** yang menerima sesi kiosk beserta daftar
   pemeriksaan, dalam satu transaksi. `POST /lab-orders` yang sudah ada **tidak disentuh**.

**Kenapa dipecah, bukan dibiarkan satu pesanan lintas disiplin.** `LabOrder.Discipline` adalah
satu nilai dan terkunci setelah pesanan dibuat (`INV-21`); ketiga layar Pemeriksaan menyaring
tepat atas kolom itu; dan `VAL-46` sudah menolak pemeriksaan yang disiplinnya tidak cocok dengan
pesanannya. Memindahkan disiplin ke baris pemeriksaan berarti membongkar ketiganya sekaligus.
Pemecahan mencapai hasil yang sama — setiap pemeriksaan muncul di menu yang benar — **tanpa
menyentuh satu pun invariant yang sudah berjalan.**

**Kenapa endpoint baru, bukan memperluas yang lama.** `POST /lab-orders` mengembalikan **satu**
pesanan. Membuatnya kadang mengembalikan dua adalah perubahan bentuk respons bagi pemanggil yang
sudah ada. Pelajaran `specimenTypeId` pada `BE-LAB-21` masih segar: ruas wajib yang ditambahkan
ke endpoint berjalan membuat layar yang sudah dipakai menjawab `422` seketika. Endpoint baru
menghindari seluruh kelas persoalan itu.

> **Temuan yang dibuka pass ini, dan bukan bagian dari keputusan di atas.** `AC-83` menyatakan
> *"tidak ada jalan menyimpan pesanan berdisiplin kosong ketika pemeriksaannya sudah
> digolongkan"*. Pemeriksaan source menunjukkan janji itu **hanya ditegakkan frontend**:
> `LabOrderService` menyalin `request.Discipline` apa adanya tanpa pernah menurunkannya dari
> katalog, dan ruas itu tidak wajib. Pemanggil mana pun di luar layar tersebut masih dapat
> membuat pesanan tanpa disiplin. Terbukti pada data: **2 dari 5 pesanan tidak berdisiplin**,
> dan keduanya tidak muncul di satu pun dari tiga menu. Dicatat sebagai `LAB-CONFLICT-007`.

---

### BR-48 — Konfirmasi pesanan dan dokter pemeriksa (`LAB-DEC-061`)

> **Bagian ini ditulis 2026-09-16 untuk menutup selisih pembukuan, bukan untuk menambah aturan.**
> `AC-94` dan `AC-95` merujuk `BR-48` sejak 2026-09-15, tetapi bagiannya tidak pernah ditulis —
> putaran 2 menambahkan acceptance criteria-nya tanpa business rule-nya. Seluruh isi di bawah
> **disusun ulang dari keputusan dan kontrak yang sudah terkunci**: `LAB-DEC-061`,
> `LAB-STATE-v1` `r3`, `LAB-VAL-v1` `r6` (`VAL-70`..`VAL-73`), dan `LAB-API-v1` `r12`..`r14`.
> **Nol aturan baru diperkenalkan di sini.** Bila ada yang terbaca seperti aturan baru, itu cacat
> penulisan bagian ini dan yang berlaku tetap kontraknya.

**Aturan:**

1. **`Confirmed` adalah status pesanan tersendiri**, berada antara `Requested` dan `Accepted`.
2. **Konfirmasi hanya sah sekali.** Pesanan yang sudah dikonfirmasi menolak konfirmasi berikutnya
   (`VAL-70`, `409`), dan pesanan yang statusnya bukan `Requested` menolak konfirmasi sama sekali
   (`VAL-71`, `409`).
3. **Konfirmator mengikuti pengguna yang login**, bukan diisi manual. Nama dan tanggal/waktunya
   terekam pada `LabOrder.ConfirmedByUserId` dan `LabOrder.ConfirmedAt`.
4. **Dokter pemeriksa wajib dipilih sebelum konfirmasi disimpan.** Konfirmasi tanpa dokter
   pemeriksa ditolak (`VAL-72`, `422`), dan dokter yang dipilih wajib ada serta aktif (`VAL-73`,
   `422`). Nilainya tersimpan pada `LabOrder.ExaminerDoctorId`.
5. **Ketiganya terbaca kembali pada daftar dan ringkasan cetak** lewat ruas tampil
   `confirmedAt`, `confirmedByName`, dan `examinerDoctorName`.

**Kenapa nama, bukan penunjuk, yang dikirim ke daftar.** Daftar pantau adalah layar **baca**: ia
menampilkan antrean dan tidak melakukan aksi apa pun terhadap konfirmator maupun dokter pemeriksa.
Penunjuk hanya dibutuhkan aksi, dan aksi pada modul ini berjalan lewat **detail** pesanan — yang
sudah membawa `confirmedByUserId` dan `examinerDoctorId`. Mengirim penunjuk yang tidak dipakai
berarti mengirim nilai yang tidak boleh ditampilkan ke layar yang tidak membutuhkannya.

> **Satu hal sengaja TIDAK ditetapkan, dan bukan kelalaian.** Jalur `Requested` → `Accepted`
> **tidak dicabut** `LAB-STATE-v1` `r3`, sehingga konfirmasi **belum wajib** sebelum `Accepted`.
> Mewajibkannya menuntut perlakuan atas pesanan yang sedang berjalan dan kepastian bahwa setiap
> jalur pembuat pesanan melewati layar bertombol Konfirmasi. Dicatat `LAB-OPEN-027`.

---

### BR-49 — Pembatalan pesanan wajib beralasan, dan batas statusnya (`LAB-DEC-063`)

> **Sama seperti `BR-48`, bagian ini ditulis 2026-09-16 untuk menutup selisih pembukuan.**
> `AC-96` dan `AC-97` merujuknya sejak 2026-09-15. Disusun ulang dari `LAB-DEC-063`,
> `LAB-STATE-v1` `r3`, `LAB-VAL-v1` `r6` (`VAL-74`, `VAL-75`), dan `LAB-API-v1` `r12`.
> **Nol aturan baru.**

**Aturan:**

1. **Alasan pembatalan wajib.** Pembatalan tanpa alasan, atau beralasan yang hanya berisi spasi,
   ditolak (`VAL-74`, `422`).
2. **Pembatalan hanya sah pada `Requested` dan `Confirmed`.** Pesanan yang sudah `Accepted`,
   `InProcess`, `Completed`, atau `Cancelled` menolak pembatalan (`VAL-75`, `409`).
3. **Alasannya tersimpan pada jejak audit, bukan pada pesanan.** `LabOrderService.CancelAsync`
   sudah menyimpannya sebagai `ReasonNote` pada `LabTransitionHistory`, terbaca kembali per
   pesanan lewat `LabOrderId`. **Nol kolom baru dibuat untuk ini.**
4. **Pop-up alert konfirmasi akhir adalah kewenangan UI.** Backend menuntut alasannya ada, bukan
   menuntut berapa kali petugas ditanya.

**`VAL-75` adalah satu-satunya pengetatan, dan ia menyentuh endpoint yang sudah dipakai.**
`PUT /lab-orders/{id}/cancel` sebelumnya menerima pembatalan dari status yang lebih luas. Ini
dicatat terang pada `LAB-API-v1` `r12` sebagai perubahan **breaking**, bukan diselipkan sebagai
penyesuaian kecil.

**Apa yang TIDAK dijawab bagian ini.** `LAB-P0-003` menanyakan aturan pembatalan **dan koreksi**.
Yang ditetapkan di sini hanya pembatalan. **Koreksi hasil belum disinggung sama sekali** dan tetap
terbuka — ia melekat pada slice `S6`, yang tertahan `LAB-SIGN-001`.

---

### BR-50 — Menu Hasil tiga disiplin, pencetakan, dan pengiriman hasil ke pasien (`LAB-DEC-064` .. `LAB-DEC-069`)

**Sumber:** artifact `Laboratorium (4).md`, diserahkan pemilik modul 2026-09-16, berisi sepuluh
jawaban klarifikasi bertanggal sama. Rekonsiliasinya ada pada
[`05-evidence-reconciliation.md`](05-evidence-reconciliation.md) bagian 11.

> **Baca lebih dulu, sebelum satu baris pun diturunkan menjadi task.** Seluruh isi BR ini adalah
> slice **`S17`**, dan `S17` berdiri di atas hasil pemeriksaan yang **belum ada**:
> `LabExamination` hari ini nol kolom nilai hasil, nol waktu pemeriksaan, nol status verifikasi.
> Yang membentuknya slice `S4`, dan `S4` tertahan `LAB-SIGN-001`. BR ini **mencatat requirement**,
> **bukan** membuka izin membangun.

**Aturan yang ditetapkan pemilik:**

1. **Penyaring.** Keyword Search memakai kolom, relasi, dan pola query order pemeriksaan yang
   **sudah ada di backend**; mengarang nama kolom baru dilarang (`LAB-DEC-065`). Penyaring
   lainnya: NIK/No. RM, Kategori Periode, Tgl Awal, Tgl Akhir, dan Jenis Kunjungan.
2. **Rentang tanggal.** Tanggal hari ini boleh dipilih pada kedua ruas; tanggal masa depan tidak
   boleh; **tidak ada batas maksimum** panjang rentang; selama pemuatan wajib tampil animasi
   loading (`LAB-DEC-064`). Pembandingnya **inklusif** — `Tgl Awal <= Tgl Akhir` — sehingga
   pencarian satu hari tetap mungkin (`LAB-DEC-071`, mengoreksi `RULE-004` artifact).
3. **Perilaku datatable.** Pagination kelipatan 5, urutan default data terbaru paling atas, alert
   ketika hasil kosong, dan **alasan** ketika datatable gagal dimuat (`LAB-DEC-069`).
4. **Satu hasil final per nomor order.** Seluruh pemeriksaan pada satu order wajib sudah
   mempunyai hasil sebelum hasil final order dianggap lengkap, dan satu nomor order hanya
   menghasilkan **satu** dokumen final betapapun banyak pemeriksaannya (`LAB-DEC-067`).
5. **Syarat kirim.** Hasil hanya boleh dikirim sesudah memperoleh persetujuan **Profesor** dan
   **Dokter Lab**; nomor tujuannya diambil dari nomor WhatsApp pasien pada data induk pasien;
   berkasnya PDF atau laporan final yang **tidak dapat diedit pasien** (`LAB-DEC-067`).
6. **Counter `Terkirim ke Pasien`.** Angka integer, bukan rasio. Setiap pengiriman berhasil
   menambah `+1`; kegagalan **tidak** menambah. Tombol kirim ulang tetap tersedia, termasuk
   sesudah pengiriman sebelumnya gagal (`LAB-DEC-066`).
7. **Kewenangan.** Seluruh tombol aksi pada menu ini dipegang **Petugas Lab dan/atau Admin**
   (`LAB-DEC-068`).
8. **Bentuk menu.** Menu Hasil mengikuti pola **tiga daftar sejajar per disiplin**, bukan satu
   datatable gabungan. Disiplin ditentukan jalur yang dipanggil, bukan ruas yang dikirim
   (`LAB-DEC-070`).
9. **Nomor order.** `LabOrder` memperoleh kolom nomor order berupa **nomor urut yang terbaca
   manusia**, mengikuti pola `PatientEncounterNumberService`. Nomor ini mengisi kolom
   `No. Order` sekaligus menjadi sumber barcode Label Lab (`LAB-DEC-072`).
10. **Cakupan baris.** Menu Hasil hanya memuat pesanan Laboratorium; nilai Unit Layanan
   `Radiologi` tidak akan pernah muncul di sini (`LAB-DEC-073`).

**Yang sudah terpenuhi source hari ini — nol pekerjaan.** Urutan terbaru di atas sudah berjalan
(`LabMonitoringService` mengurutkan menurun atas `RequestedAt ?? CreateDateTime`); keempat pilihan
`pageSizeOptions` frontend (`10`, `25`, `50`, `100`) **semuanya kelipatan 5**; pola loading, empty,
dan error sudah dipakai layar pantau; dan nomor tujuan sudah punya tempat, yaitu
`MstPatient.WhatsAppNumber`.

**Yang belum ada sama sekali, dan ini bagian yang mahal.** `LabOrder` **nol kolom nomor order** —
bentuknya sudah diputuskan `LAB-DEC-072`, tetapi kolom, layanan alokasi, dan migration-nya belum
dibangun. Tanggal pemeriksaan — satu dari tiga pilihan Kategori Periode — belum punya kolom mana
pun. Counter pengiriman belum punya tabel. Gerbang pengiriman WhatsApp dan pembangkit berkas PDF
**keduanya nol pada platform**, dan keduanya bukan wewenang Laboratorium (`LAB-COORD-011`).

**Yang sengaja TIDAK diadopsi.** Ukuran cetak A4 / 100x50 mm / 50x25 mm **tidak** menjadi aturan.
Artifact menandainya sendiri `Confidence: Medium` dan menyebutnya *"default umum implementasi,
bukan ukuran klinis/regulasi yang ditetapkan oleh evidence"*. Menaikkannya menjadi keputusan
berarti mengubah tingkat bukti diam-diam. Dicatat sebagai `LAB-OPEN-032`.

> **Tujuh hal dibuka, empat ditutup pada hari yang sama.** Yang **ditutup** pemilik modul
> 2026-09-16: bentuk menu (`LAB-CONFLICT-009` → `LAB-DEC-070`), arah pembanding tanggal
> (`LAB-OPEN-028` → `LAB-DEC-071`), nomor order (`LAB-OPEN-033` → `LAB-DEC-072`), dan cakupan
> baris (`LAB-OPEN-031` → `LAB-DEC-073`). Yang **tetap terbuka dan tidak boleh ditebak
> implementer**: persetujuan Profesor dan Dokter Lab yang justru perkara `LAB-SIGN-001`
> (`LAB-OPEN-029`, bukan wewenang pemilik modul sendiri); jabatan `Dokter Lantai` pada kolom
> Dokter Konfirmator (`LAB-OPEN-030`, bertaut `LAB-P0-004`); dan ukuran cetak yang menunggu
> profil printer nyata (`LAB-OPEN-032`). Ketiganya diajukan lewat `LAB-REQ-007`.

> **Catatan pembukuan — ✅ ditutup 2026-09-16.** `AC-94` sampai `AC-97` merujuk `BR-48` dan
> `BR-49`, tetapi kedua BR itu tidak pernah ditulis sebagai bagian — putaran 2 menambahkan
> AC-nya tanpa BR-nya. **Keduanya kini ditulis** tepat di atas bagian ini, **disusun ulang dari
> keputusan dan kontrak yang sudah terkunci**, nol aturan baru diperkenalkan. Penomoran `BR-50`
> tetap seperti semula supaya rujukan yang sudah terlanjur dipakai tidak bergeser.

---

## Amendment Pass Putaran 9 — Halaman Hasil Pemeriksaan Mikrobiologi (2026-09-21)

**Sumber:** artifact `Laboratorium - Hasil Pemeriksaan Mikrobiologi` yang diserahkan pemilik
modul 2026-09-21, berisi 25 capability, 40 rule, dan 7 alur bisnis. Artifact itu sendiri
merangkum klarifikasi pengguna Q1-Q20 bertanggal 2026-09-18 beserta tujuh klarifikasi
lanjutan. Dibukukan sebagai **`LAB-EVD-004`**.

> **Baca lebih dulu.** Isi bagian ini adalah slice **`S4b`** (pengisian hasil Mikrobiologi),
> dan sebagian menyentuh **`S4d`** (validasi dan rilis Mikrobiologi). `S4b` sudah dibuka
> `LAB-DEC-084` pada 2026-09-18 dan data induk organisme serta antibiotiknya **sudah
> berdiri** di source. `S4d` masih tertahan `DEC-LAB-011`. Bagian ini **mencatat
> requirement**, bukan membuka izin membangun.

### Keadaan source saat wawancara ini berjalan

Diperiksa langsung pada backend `981e002c` dan frontend `ebef7ebe5`, 2026-09-21.

| Hal | Keadaan hari ini |
|---|---|
| `LabOrganism`, `LabAntibiotic` | **Sudah dibangun**, migration `AddLabMicrobiologyMasterData` 2026-09-18 |
| `LabExamination` | Punya `ResultNumeric`, `ResultOptionId`, `ResultValueBoundId`, `ExaminedAt`, `ResultEnteredAt`, `ResultEnteredByUserId`, `IsDuplo`, `Urgency` |
| Isolat dan baris kepekaan antibiotik | **Nol tabel.** Inilah pekerjaan inti `S4b` |
| `ValidatedAt` / `ReleasedAt` | **Belum dibangun.** Bentuknya sudah dikunci `LAB-DEC-080` |
| `LabSpecimenType` | Master **satu tingkat**: kode, nama, `IsOtherBucket`. Nol subjenis |
| `LabSpecimen` | `SpecimenTypeId` **tunggal**, `SpecimenTypeOtherNote`, `VolumeAmount` + `VolumeUnitId` |
| `MstDoctorSchedule` | **Ada**, dan menjadi calon sumber daftar dokter bertugas |
| `HL7` | **Nol kemunculan di seluruh backend**, bukan hanya di Laboratorium |

### Batas scope putaran ini (dikonfirmasi pemilik modul 2026-09-21)

**Di dalam scope:**

1. Halaman Hasil Pemeriksaan Mikrobiologi — pengisian (`S4b`) dan titik sentuhnya ke validasi/rilis (`S4d`).
2. Isolat organisme dan baris kepekaan antibiotik (antibiogram).
3. Penanda kritis khusus Mikrobiologi.
4. Penyuntingan Informasi Specimen pada halaman hasil.

**Di luar scope — sudah punya rumahnya sendiri:**

| Hal | Rumahnya |
|---|---|
| Patologi Klinik dan Patologi Anatomi | `S4a`+`S4`, `S4c`+`S4e`, terpisah sesuai `LAB-DEC-083` |
| Menu Penerimaan Sampling/Specimen sebagai layar | `S2`, sudah dikunci `LAB-DEC-045` |
| Gerbang WhatsApp dan pembangkit berkas PDF | `LAB-COORD-011`, pemilik platform |
| Sumber dan profil data `HL7` | `LAB-COORD-012`, pemilik platform |
| Layanan terjemahan otomatis beserta izin privasinya | `LAB-COORD-013`, pemilik platform + pemilik modul |
| Ukuran cetak | `LAB-OPEN-032`, menunggu profil printer nyata |
| Lokasi Specimen dan Metode Pengambilan Specimen | `S2b`, masih `BUSINESS_DECISION_REQUIRED` |

**Yang tidak ditanyakan ulang karena sudah terjawab:** `Diplo` sama dengan `Duplo` dan memakai
`IsDuplo` yang sudah berdiri (`LAB-DEC-089`); `Penanggung Jawab Analis` adalah ruas baru yang
sah (`LAB-DEC-093`); organisme dan antibiotik adalah data induk terkendali milik Laboratorium
(`LAB-DEC-084`); dan `Status Hasil PA` memang khas Patologi Anatomi sehingga ketiadaannya pada
Mikrobiologi bukan kehilangan (`LAB-DEC-094`).

### BR-51 — Hasil Mikrobiologi tetap melekat pada pemeriksaan (`LAB-DEC-095`)

**Menegakkan `LAB-DEC-085`, menolak `RULE-001` artifact.**

Artifact menuntut satu hasil pada tingkat **No. Order**. Itu ditolak. Yang berlaku tetap:

1. Setiap baris `LabExamination` berdisiplin Mikrobiologi memiliki hasilnya **sendiri**.
2. Yang berjumlah satu per order adalah **dokumen final cetaknya**, dan itu sudah dikunci
   `LAB-DEC-067` sejak 2026-09-16.
3. Layar hasil karena itu bertingkat dua: petugas memilih baris pemeriksaan lebih dulu, baru
   mengisi hasilnya.

**Kenapa bukan per order.**

> Satu pesanan memuat Kultur Darah dan Kultur Urin. Keduanya bahan berbeda dan sangat mungkin
> menumbuhkan kuman berbeda: darah tumbuh `Staphylococcus aureus`, urin tumbuh
> `Escherichia coli`. Bila keduanya dipaksa masuk satu kolom hasil, laporan pola kuman rumah
> sakit tidak lagi dapat menjawab "kuman apa yang paling sering tumbuh dari darah" — dan
> justru pertanyaan itulah yang menjadi dasar pemilihan antibiotik empiris.

**Contoh yang benar:**

> No. Order `LAB-2026-000871` berisi dua pemeriksaan. Petugas membuka baris Kultur Darah,
> mencatat isolat `Staphylococcus aureus`, lalu membuka baris Kultur Urin dan mencatat isolat
> `Escherichia coli`. Ketika keduanya selesai dan dirilis, yang tercetak **satu lembar**
> memuat dua bagian hasil.

### BR-52 — Waktu Issued dan Waktu Efektif Mikrobiologi diturunkan (`LAB-DEC-096`)

**Memperluas `LAB-DEC-092` dari Patologi Anatomi ke Mikrobiologi. Menolak `RULE-021` dan
`RULE-022` artifact.**

1. **Waktu Efektif** diambil dari `LabSpecimen.CollectedAt`, yaitu kapan bahan diambil dari
   pasien. Sudah tercatat sejak `S2`.
2. **Waktu Issued** diambil dari `FinalizedAt`, yaitu kapan hasil dinyatakan selesai ditulis.
3. Layar menampilkan keduanya **baca-saja**. Nol kolom baru, nol ketikan.
4. Koreksi waktu pengambilan bahan dilakukan pada data specimen, bukan pada halaman hasil.

**Contoh:**

> Bahan diambil Senin pukul 07.15 dan analis menyelesaikan penulisan hasil Kamis pukul 14.40.
> Waktu Efektif terbaca `Senin 07.15`, Waktu Issued terbaca `Kamis 14.40`. Nol keduanya
> diketik siapa pun, dan nol keduanya dapat melenceng dari kejadian yang dicatat sistem.

### BR-53 — Arti Simpan Final pada hasil Mikrobiologi (`LAB-DEC-097`)

**Menyalin pola `LAB-DEC-088` dari Patologi Anatomi. Menolak tafsir artifact bahwa Final
berarti hasil sah untuk pasien.**

| Tindakan | Artinya | Yang tercatat |
|---|---|---|
| Simpan Draft | Pengisian belum selesai | `FinalizedAt` masih kosong |
| Simpan Final | **Penulis hasil menyatakan selesai menulis** | `FinalizedAt` + `FinalizedByUserId` |
| Reopen | Penulisan dibuka kembali sebelum rilis | `FinalizedAt` dikosongkan beserta jejaknya |

**Yang tegas: `Simpan Final` BUKAN rilis.** Hasil yang sudah Final **belum boleh** dikirim ke
pasien. Pengiriman menunggu rilis, dan rilis Mikrobiologi adalah slice `S4d` yang tertahan
`DEC-LAB-011`.

**Kenapa bukan tafsir artifact.**

> Artifact menyerahkan tombol Final kepada Dokter Lab dan menyebut hasilnya langsung terkunci
> sebagai hasil pasien. Bila itu diterima, orang yang mengisi hasil sekaligus yang
> mengesahkannya — dan prinsip empat mata `LAB-DEC-003`, yang baru ditandatangani
> `DR-LAB-002` pada 2026-09-17, berhenti berlaku untuk Mikrobiologi saja tanpa satu pun
> pihak klinis menyatakannya.

**Akibat pembukuan:** `Reopen` sebelum rilis adalah penyuntingan biasa, **bukan** koreksi hasil
terrilis. Ia tidak menyentuh `S6` maupun `DEC-LAB-014`. Karena itu `BP-007` artifact — yang
menyatakan hasil Final hanya dapat dibuka baca-saja dan nol koreksi — **tidak diadopsi apa
adanya**: baca-saja berlaku sesudah **rilis**, bukan sesudah Final.

### BR-54 — Specimen memperoleh tingkat kedua yang terkendali (`LAB-DEC-098`, `LAB-DEC-099`)

**Menegakkan tata kelola `LAB-DEC-040`, menerima kebutuhan bentuknya dari artifact.**

1. `LabSpecimenType` **tetap satu tingkat** dan tidak diubah.
2. Berdiri **satu data induk baru** milik Laboratorium untuk Spesifik Specimen, yang menunjuk
   induknya pada `LabSpecimenType`.
3. Satu specimen boleh menunjuk **lebih dari satu** Spesifik Specimen.
4. Isi awalnya **bukan** 1.767 entri mentah, melainkan hasil penyaringan kepala instalasi atas
   nilai yang benar-benar dipakai Mikrobiologi (`LAB-DEC-099`).
5. Penambahan nilai baru tetap tunduk `LAB-DEC-040`: petugas boleh memakai jalan keluar
   `Lainnya` beserta keterangannya, pemakaian itu masuk **daftar pantau**, dan hanya **kepala
   instalasi** yang menaikkannya menjadi nilai tetap.

**Kenapa petugas tidak boleh langsung membuat nilai tetap.**

> Artifact mengusulkan pemeriksaan duplikat terhadap nama Indonesia maupun Inggris. Itu tidak
> cukup. `cairan kista`, `Cairan Kista`, `c. kista`, dan `kista` lolos dari pemeriksaan
> semacam itu sebagai empat nilai berbeda, dan dalam enam bulan daftar pilihannya menjadi
> tempat pembuangan. Alasannya sama persis dengan yang dipakai `LAB-DEC-040` tujuh hari lalu.

**Kenapa 1.767 entri tidak diimpor apa adanya.**

> Daftar pilihan berisi 1.767 baris bukan bantuan bagi petugas, melainkan hambatan. Dan
> dataset itu **nol kemunculan** di seluruh blueprint sebelum hari ini — ia belum pernah
> diperiksa siapa pun terhadap kebutuhan Mikrobiologi.

### BR-55 — Volume specimen tetap angka dan satuan (`LAB-DEC-100`)

**Menegakkan `LAB-DEC-041`, menerima sebagian kebutuhan bagian 5.5.4 artifact.**

1. Bentuk penyimpanan **tidak berubah**: satu angka, satu satuan.
2. Daftar satuan **diperpanjang** supaya bentuk specimen non-cair tertampung. Tambahan yang
   disepakati: `swab`, `preparat`, `potong`, `item`, `isolat`, `vial`.
3. Ukuran berdimensi — panjang kali lebar kali tinggi — **tidak** ditampung sebagai volume.
   Ia masuk `Keterangan Specimen`, karena ia tiga angka, bukan satu.

**Contoh:**

| Bahan | Tersimpan | Terbaca |
|---|---|---|
| Kultur darah | `5` + `mL` | `5 mL` |
| Swab tenggorok | `2` + `swab` | `2 swab` |
| Feses | `3` + `gram` | `3 gram` |
| Biopsi kulit | `1` + `potong`, keterangan `2 × 1 × 0,5 cm` | `1 potong`, dimensi di keterangan |

**Kenapa formatnya tidak dijadikan teks bebas.** Angka yang tersimpan sebagai kalimat tidak
dapat dijumlahkan, dibandingkan, maupun dilaporkan. Itu alasan yang sama dengan `LAB-DEC-041`.

### BR-56 — Isi baris kepekaan antibiotik (`LAB-DEC-101`, `LAB-DEC-102`)

**Mempertahankan BR-23, menolak penyempitan artifact.**

Satu baris kepekaan menyimpan:

| Ruas | Wajib | Keterangan |
|---|:---:|---|
| Antibiotik | **Ya** | Dari `LabAntibiotic` yang sudah berdiri; pengetikan bebas ditolak (`LAB-DEC-084`) |
| Interpretasi `S`/`I`/`R` | **Ya** | Arti kodenya sesuai BR-23 |
| Nilai MIC | Tidak | Terisi bila metode dilusi dipakai |
| Zona hambat (mm) | Tidak | Terisi bila metode difusi cakram dipakai |
| Keterangan | Tidak | Catatan analis |

Status `Normal`/`Positif`/`Negatif` tetap ada pada **tingkat isolat**, bukan pada baris
kepekaan.

**Kenapa MIC dan zona keduanya opsional, bukan salah satu diwajibkan.**

> Metode difusi cakram menghasilkan angka dalam milimeter. Metode dilusi menghasilkan nilai
> MIC. Laboratorium memakai keduanya bergantung antibiotik dan alat yang tersedia. Bila salah
> satunya diwajibkan, analis yang memakai metode lain akan mengisi angka karangan supaya
> formulirnya bisa disimpan — dan angka karangan itu tersimpan selamanya sebagai kalau-kalau
> data yang benar. Alasan yang sama dipakai `LAB-DEC-041` menolak memaksakan satuan mililiter.

**Subbakteri tidak dibangun (`LAB-DEC-102`).** Artifact memperkenalkan tingkat kedua di bawah
bakteri utama. Itu ditolak. `Escherichia coli` dan `Escherichia coli ESBL` masing-masing satu
baris tersendiri pada `LabOrganism` yang sudah berdiri. Nama kuman sudah memuat genus dan
spesies dalam satu nama, dan `Escherichia` sendirian bukan temuan yang dapat dilaporkan.
Artifact sendiri nol memberi satu pun contoh pasangan bakteri dan subbakteri.

### BR-57 — Penanda kritis Mikrobiologi berdiri sebagai data induk (`LAB-DEC-103`)

**Memisahkan bentuk dari isi. Menjawab bagian `LAB-OPEN-014` yang memang milik pemilik modul.**

1. Aturan kritis Mikrobiologi berdiri sebagai **data induk terkendali milik Laboratorium**.
2. Bentuk barisnya: kombinasi organisme, antibiotik, dan interpretasi.
3. Aturan dapat diubah **tanpa mengubah kode**. Nol angka atau kombinasi klinis boleh
   dihardcode.
4. **Isi barisnya diisi wewenang klinis Mikrobiologi (`DR-LAB-002`)**, bukan pemilik modul dan
   bukan implementer.
5. Selama barisnya masih kosong, penanda kritis **tidak menyala**, dan layar wajib menyatakan
   keadaan itu secara terbaca — bukan diam.

**Kenapa bukan "semua `R` berarti kritis".**

> Resistensi terhadap satu antibiotik cadangan bukan kegawatan. Sebaliknya MRSA yang masih
> peka terhadap banyak obat justru wajib dikabarkan segera. Aturan "semua `R` kritis" membuat
> penanda menyala hampir pada setiap hasil, dan alarm yang berbunyi terus-menerus berhenti
> dibaca orang. Artifact sendiri sudah menolaknya, dan penolakan itu benar.

**Yang tetap terbuka.** Alur pelaporannya — batas waktu, eskalasi, bukti pembacaan ulang —
tetap `LAB-P0-004` dan slice `S5`. Keputusan ini hanya menetapkan **dari mana penandanya
berasal**.

### BR-58 — Kewajiban ruas bergantung isi, bukan daftar nama (`LAB-DEC-104`)

**Menolak `RULE-011` artifact yang mewajibkan seluruh ruas.**

| Keadaan | Yang wajib |
|---|---|
| Selalu | Hasil pemeriksaan terisi |
| Bila ada isolat | Organisme wajib dipilih |
| Bila ada baris kepekaan | Antibiotik dan interpretasi `S`/`I`/`R` wajib |
| Selebihnya | MIC, zona mm, dan keterangan boleh kosong |

**Kenapa bukan "seluruh ruas wajib".**

> Kultur yang tidak menumbuhkan apa pun adalah hasil yang sah dan penting — ia menyingkirkan
> dugaan infeksi bakteri. Hasil seperti itu nol isolat dan nol baris kepekaan. Mewajibkan
> organisme membuat hasil negatif **mustahil disimpan**, dan petugas akan memilih organisme
> sembarang supaya formulirnya lolos.

Aturan sejenis pernah dicabut minggu ini: `VAL-88` dibatalkan justru karena ia mengunci tiga
nama ruas yang dihardcode. Keputusan ini mengikuti pola penggantinya, `VAL-95` — kewajiban
ditegakkan terhadap keadaan data, bukan terhadap daftar nama kolom.

### BR-59 — Ruas Analis diturunkan, bukan dipilih (`LAB-DEC-105`)

**Menolak artifact yang menjadikannya pilihan wajib.**

1. Nama analis pada form hasil **diturunkan** dari pengguna yang menekan simpan, yaitu
   `ResultEnteredByUserId` yang sudah dicatat sistem. Layar menampilkannya **baca-saja**.
2. `Penanggung Jawab Analis` tetap ruas tersendiri yang **dipilih**, sesuai `LAB-DEC-093` —
   dua orang, dua pekerjaan.

**Kenapa tidak boleh dipilih.**

> Bila analis dapat dipilih, petugas A dapat menyimpan hasil atas nama analis B. Prinsip empat
> mata `LAB-DEC-003` ditegakkan justru dengan membandingkan pengisi terhadap pemvalidasi, dan
> perbandingan itu kehilangan dasarnya begitu kolom pengisi boleh diketik. Alasan yang sama
> dipakai `LAB-DEC-092` tiga hari lalu terhadap Waktu Efektif: satu kejadian, satu jawaban.

### BR-60 — Penanda Definitif adalah fakta konsultasi (`LAB-DEC-106`)

**Menutup `LAB-OPEN-017` yang terbuka sejak 2026-09-02.**

1. `Definitif` menyimpan **fakta**: siapa mengonsultasikan, kepada siapa, dan kapan.
   Mengikuti pola `LAB-DEC-080` — mencatat apa yang terjadi, bukan menjanjikan apa berikutnya.
2. Ia **bukan status hasil**, dan **bukan izin apa pun**.
3. Hasil tetap dapat disunting sesudah `Definitif` menyala, sampai `Simpan Final`.
4. **`CAP-023` artifact tidak diadopsi.** `Definitif` tidak membuka pembagian hasil kepada
   dokter atau petugas medis lain sebelum rilis.

**Kenapa bukan izin berbagi.**

> Menjadikannya izin berarti hasil yang belum divalidasi beredar ke dokter lain sebagai dasar
> pengobatan, sementara gerbang pengirimannya sudah ditetapkan tersendiri oleh `LAB-DEC-067`.
> Dan menyamakan `Definitif` dengan "persetujuan Profesor" pada keputusan itu **bukan
> wewenang pemilik modul** — itu persis pertanyaan `LAB-OPEN-029`, yang justru menajam karena
> penetapan wewenang klinis 2026-09-17 nol memuat nama bergelar Profesor.

### BR-61 — Informasi Specimen dapat disunting sampai Final (`LAB-DEC-107`)

**Menerima `CAP-024` artifact, dengan syarat jejak.**

1. Petugas boleh melengkapi dan mengoreksi Informasi Specimen **pada halaman hasil**, termasuk
   jenis dan Spesifik Specimen.
2. Setiap perubahan **tercatat**: apa yang berubah, siapa mengubah, dan kapan. Nilai lamanya
   tetap terbaca.
3. Sesudah `Simpan Final`, seluruh bagian ini **baca-saja**.
4. Informasi Specimen tetap **bukan** area wajib.

**Kenapa tidak dibuat baca-saja.**

> Menutupnya mengulang jalan buntu yang persis dihindari `LAB-DEC-040`: satu kesalahan pilih
> saat penerimaan menahan pengisian hasil sampai petugas penerimaan tersedia — dan pada shift
> malam petugas itu mungkin tidak ada. Yang tertahan bukan formulir, melainkan pekerjaan atas
> bahan yang sudah terlanjur diambil dari tubuh pasien.

**Kenapa harus berjejak.** Mengubah jenis bahan sesudah pemeriksaan berjalan mengubah arti
hasilnya. Tanpa jejak, pembaca enam bulan kemudian nol punya cara tahu bahwa bahannya pernah
tercatat lain.

### BR-62 — Dokter Konfirmator: dua cara memilih, bukan dua jabatan (`LAB-DEC-108`)

**Menutup `LAB-OPEN-030` yang terbuka sejak 2026-09-16.**

1. Pilihan pertama: **DPJP**, diambil dari pesanan.
2. Pilihan kedua: **dokter yang sedang bertugas**, dibaca dari data induk jadwal dokter
   `MstDoctorSchedule` yang sudah berdiri.
3. Istilah **`Dokter Lantai`** menjadi **label layar** untuk pilihan kedua. Ia **bukan jabatan
   baru**, bukan peran pada matriks kewenangan, dan nol menambah tabel.

**Kenapa bukan jabatan baru.**

> Mendirikan `Dokter Lantai` sebagai peran berarti menambahkannya ke `LAB-PERM-v1` yang masih
> menunggu `LAB-P0-001`, dan menuntut proses baru untuk menetapkan siapa dokter lantai hari
> ini. Nol bukti lapangan menjelaskan siapa yang menetapkannya.

> ### ⚠️ BUTIR 2 BERDIRI DI ATAS FAKTA YANG TERBANTAH — impact scan 2026-09-21
>
> Pemeriksaan `/qv-trace` revision 4 pada `981e002c` menemukan **`MstDoctorSchedule` adalah
> jadwal praktik poliklinik, bukan daftar dokter jaga**: `ClinicId` wajib, ada
> `MaxPatientQuota`, `MaxWalkInQuota`, `IsAllowKioskRegistration`, dan `IsTelemedicineAvailable`,
> sedangkan `ScheduleType` hanya mengenal `WeeklyRecurring`, `SpecificDate`, dan `Temporary` —
> **nol nilai yang berarti "sedang jaga"**.
>
> **Akibatnya nyata, bukan soal kerapian.** Hasil kritis paling sering muncul pukul dua pagi,
> dan yang dicari petugas saat itu dokter yang **sedang berjaga di bangsal** — bukan dokter
> yang membuka praktik di Poli Penyakit Dalam setiap Selasa pukul 09.00. Tabel itu hanya bisa
> menjawab pertanyaan kedua.
>
> **Butir 1 — DPJP dari pesanan — tetap sahih dan nol terdampak.** Yang terbantah hanya
> sumber pilihan kedua. Dibuka sebagai `LAB-CONFLICT-010`, dan pertanyaan penggantinya
> `LAB-CLOSE-010`. Kandidat yang ditemukan scan: `TrxOnCallAssignment` milik Human Resource —
> penugasan jaga bertenggat waktu yang sesungguhnya, tetapi menunjuk `WorkforceProfileId`
> dan bukan `DoctorId`.
>
> **`LAB-DEC-108` belum dicabut** — mencabut keputusan bukan wewenang audit. Statusnya tetap
> `approved` sampai pemilik modul membukanya ulang pada putaran berikutnya.
>
> ### ✅ DISELESAIKAN pada hari yang sama — putaran 10
>
> **Butir 2 digantikan `LAB-DEC-111`:** dokter bertugas dibaca dari `TrxOnCallAssignment`,
> dengan jalur jatuh permanen ke daftar dokter aktif ketika jadwal jaga kosong. **Butir 1 —
> DPJP dari pesanan — tetap berlaku apa adanya.** Bentuk jawabannya pun tidak berubah: tetap
> **dua cara memilih, bukan dua jabatan**, sehingga `LAB-OPEN-030` tetap tertutup dan tidak
> dibuka kembali. Baca BR-65.

### BR-63 — Ruas HL7 tidak dibangun pada putaran ini (`LAB-DEC-109`)

**Menolak value set sementara `P`/`F`/`C`/`X` yang diusulkan artifact.**

1. Halaman hasil Mikrobiologi berdiri **tanpa** ruas HL7.
2. Ruas itu ditambahkan sesudah pemilik platform menyatakan profil HL7 mana yang dipakai
   (`LAB-COORD-012`).

**Kenapa nilai sementara ditolak.**

> Nilai sementara yang sudah terlanjur dipakai petugas selama berbulan-bulan tetap tersimpan
> sebagai data lama ketika nilai aslinya datang. Pemetaan ulangnya menjadi pekerjaan
> tersendiri yang biayanya jauh melebihi menunggu. Dan memilih value set adalah keputusan
> pemilik platform, bukan pemilik modul.

### BR-64 — Template cetak dicatat, bentuk akhirnya menunggu berkas (`LAB-DEC-110`)

**Menerima uraiannya sebagai requirement; menahan bentuk akhirnya.**

Yang dibukukan sekarang:

| Bagian | Isi |
|---|---|
| Kop | Logo rumah sakit di kiri; identitas, alamat, dan kontak rumah sakit di tengah; informasi akreditasi di kanan; judul **LABORATORIUM MIKROBIOLOGI** di tengah |
| Pengulangan | Kop dan informasi pasien **berulang pada setiap lembar** bila hasil lebih dari satu halaman |
| Footer | Kalimat anjuran menghubungi kembali dokter peminta; area **Catatan:**; kotak dua kolom berisi **Konsultan Mikrobiologi Klinik** di kiri, serta **Tgl. Cetak** dan **Petugas Otorisasi** di kanan |
| Isi dinamis | Tanggal cetak dan nama petugas otorisasi mengikuti transaksi |

**Celah bukti yang harus diisi.** Artifact menyebut dua screenshot — kop dan footer — tetapi
**kedua berkas itu tidak ikut diserahkan**. Yang dipakai bagian ini adalah **uraiannya**, bukan
gambarnya. Tata letak persisnya menunggu berkasnya, dan ukuran cetaknya tetap `LAB-OPEN-032`.
Dicatat sebagai `LAB-OPEN-039`.

---

## Amendment Pass Putaran 10 — Penutupan closure question impact scan (2026-09-21)

**Sumber:** tiga closure question yang dibuka `/qv-trace` capability map **revision 4** pada
hari yang sama — `LAB-CLOSE-010`, `LAB-CLOSE-011`, dan `LAB-CLOSE-012`. Putaran ini pendek dan
seluruhnya bersifat memperbaiki, bukan memperluas scope.

### BR-65 — Sumber dokter bertugas, beserta jalur jatuhnya (`LAB-DEC-111`)

**Menggantikan butir 2 `LAB-DEC-108`. Menutup `LAB-CLOSE-010` dan `LAB-CONFLICT-010`.**

Rantai yang dipakai, dan **seluruhnya sudah berdiri pada satu `ApplicationDbContext`**:

```
TrxOnCallAssignment (StartAt <= sekarang <= EndAt, AssignmentStatus aktif)
        -> WorkforceProfileId
        -> MstDoctor.WorkforceProfileId
        -> MstDoctor.FullName + MstDoctor.WhatsAppNumber
```

**Aturan:**

1. Sistem membaca penugasan jaga yang **aktif pada jam itu** dari `TrxOnCallAssignment`.
2. Bila hasilnya **nol baris**, layar **otomatis** menampilkan daftar dokter aktif yang dapat
   dicari, **disertai keterangan** bahwa jadwal jaga belum tersedia.
3. Jalur jatuh itu **bukan sementara**. Ia tetap berlaku di kemudian hari setiap kali jadwal
   jaga kosong pada jam tersebut.
4. `MstDoctorSchedule` **tidak dipakai** untuk keperluan ini.
5. Pilihan **DPJP** dari pesanan tetap berlaku apa adanya, nol berubah dari `LAB-DEC-108`.

**Kenapa jalur jatuhnya dibangun sekarang, bukan nanti.**

> `TrxOnCallAssignment` **nol punya controller** hari ini — tabelnya dapat dibaca, tetapi tidak
> ada satu pun cara mengisinya. Artinya pada hari pertama halaman ini hidup, jadwal jaga
> **pasti kosong**. Tanpa jalur jatuh, kolom Dokter Konfirmator lahir dalam keadaan tidak dapat
> dipakai, dan kewajiban pelaporan `LAB-DEC-004` tidak terpenuhi sejak hari pertama.
>
> Ini pola yang sudah dua kali memakan modul ini: `LAB-COORD-006` instansi perujuk dan
> `MST-POS-WRITE` — **daftarnya ada, cara mengisinya tidak**. Yang berbeda kali ini,
> Laboratorium **tidak** menunggu; ia menyediakan jalan lain sambil tetap membaca sumber yang
> benar begitu terisi.

**Contoh:**

> Pukul 02.10 hasil kultur darah menyalakan penanda kritis. Petugas membuka pilihan Dokter
> Konfirmator. Karena `TrxOnCallAssignment` belum terisi, layar menampilkan
> *"Jadwal jaga belum tersedia — pilih dokter dari daftar"* beserta pencarian nama dokter.
> Petugas memilih dr. Bagas, nomor WhatsApp-nya terbaca dari `MstDoctor.WhatsAppNumber`, dan
> pelaporan tetap tercatat. Enam bulan kemudian, sesudah Human Resource mengisi jadwal jaga,
> layar yang sama langsung menampilkan dr. Bagas sebagai dokter jaga malam itu **tanpa satu
> baris kode diubah**.

**Yang dibuka keputusan ini:** kebutuhan endpoint pengisi `TrxOnCallAssignment` pada modul
Human Resource, dicatat `LAB-COORD-014`. Ia **tidak memblokir** apa pun di Laboratorium.

### BR-66 — Jejak perubahan ruas berdiri sendiri (`LAB-DEC-112`)

**Melaksanakan `LAB-DEC-107`. Menutup `LAB-CLOSE-011`.**

1. Berdiri **satu tabel jejak perubahan ruas** milik Laboratorium. Isinya: baris apa yang
   berubah, ruas apa, **nilai lama**, nilai baru, siapa yang mengubah, dan kapan.
2. `LabTransitionHistory` **tidak disentuh** dan tetap mencatat perpindahan status saja.

**Kenapa tidak ditumpangkan.**

> Alasannya sama persis dengan yang dipakai `LAB-DEC-080` menolak menambahkan `Validated` ke
> `LabExaminationStatus`: **satu tabel, dua sumbu**. Perpindahan status dan perubahan nilai
> ruas menjawab dua pertanyaan yang berbeda. Digabungkan, ketiga kolom baru akan kosong pada
> seluruh baris status lama, dan setiap laporan riwayat status harus menyaring baris yang bukan
> miliknya — selamanya.

**Contoh:**

> Petugas keliru memilih `Urine` saat penerimaan, lalu memperbaikinya menjadi `Body Fluid` di
> halaman hasil pukul 09.40. Yang tersimpan: ruas `SpecimenTypeId`, nilai lama `Urine`, nilai
> baru `Body Fluid`, pelaku, dan waktunya. Pembaca enam bulan kemudian **tahu** bahwa bahan itu
> pernah tercatat lain — dan itu yang membuat hasilnya dapat dinilai ulang dengan jujur.

### BR-67 — Status temuan Mikrobiologi memakai daftar sendiri (`LAB-DEC-113`)

**Menutup `LAB-CLOSE-012`.**

| Disiplin | Daftar nilai | Tabelnya |
|---|---|---|
| Patologi Anatomi | `Normal`, `NeedsAttention`, `Critical` | `LabPathologyFindingStatus` yang sudah berdiri, **tidak disentuh** |
| **Mikrobiologi** | `Normal`, `Positif`, `Negatif` | **Daftar tersendiri**, pada tingkat isolat |

**Kenapa tidak dipakai ulang.**

> Keduanya menjawab pertanyaan yang berbeda. `Positif` berarti **ada pertumbuhan kuman** —
> itu temuan, bukan penilaian bahaya. `NeedsAttention` adalah **penilaian kegawatan**.
> Disatukan, Patologi Anatomi mendapat nilai `Positif` yang nol artinya bagi laporan jaringan,
> dan Mikrobiologi mendapat `NeedsAttention` yang **bertabrakan** dengan penanda kritis
> `LAB-DEC-103` — dua tempat menyatakan hal yang sama, dan nol aturan mengatakan mana yang
> menang.

**Kenapa juga bukan satu daftar gabungan lima nilai.** Setiap layar harus menyaring nilai yang
tidak berlaku bagi disiplinnya, dan penyaringan yang terlewat **satu kali saja** membuat
patolog dapat memilih `Positif` pada laporan jaringan tanpa satu pun galat terlihat.

---

## Amendment Pass Putaran 11 — Bukti cetak tiga disiplin (2026-09-21)

**Sumber:** `LAB-EVD-005` — dua PDF dan satu foto dokumen tercetak, diserahkan pemilik modul
2026-09-21. Rinciannya pada
[`evidence/2026-09-21-cetakan-tiga-disiplin.md`](evidence/2026-09-21-cetakan-tiga-disiplin.md).

> **Putaran ini memperbaiki keputusan yang berumur beberapa jam, dan itu perlu dinyatakan
> terus terang.** Bukti cetak datang **sesudah** `LAB-API-v1` `r26` disetujui pada hari yang
> sama. Empat keputusan putaran 9-10 terbukti berdiri di atas gambaran yang tidak lengkap.
> Nol di antaranya salah karena ceroboh — seluruhnya diambil dari uraian artifact, dan uraian
> itu memang tidak memuat bentuk cetak sebenarnya.

### BR-68 — `Definitif` adalah kualifikasi hasil yang dicetak (`LAB-DEC-114`)

**Memperluas `LAB-DEC-106`; tidak mencabutnya.**

Cetakan menulis `HASIL YANG DIPEROLEH : DEFINITIF` sebagai baris tersendiri yang menonjol.

1. Hasil Mikrobiologi memperoleh **satu ruas nilai kualifikasi**, daftar awalnya `Definitif`
   dan `Sementara`, dan daftar itu dapat diperluas.
2. Nilai itulah yang **dicetak** pada baris `HASIL YANG DIPEROLEH`.
3. Ketiga kolom fakta konsultasi `LAB-DEC-106` — siapa, kepada siapa, kapan — **tetap ada**
   sebagai dasar di balik nilai itu.

**Kenapa tidak diturunkan saja dari ada tidaknya konsultasi.**

> Menurunkannya berarti sistem menyimpulkan bahwa *dikonsultasikan* sama dengan *definitif*.
> Bukti nol menyatakan itu. Bila laboratorium pernah mencetak `Sementara` atas hasil yang
> sudah dikonsultasikan — dan pada biakan yang dibaca bertahap selama berhari-hari itu sangat
> mungkin — cetakannya berbohong, dan nol seorang pun akan tahu.

### BR-69 — Nilai MIC selalu membawa satuannya (`LAB-DEC-115`)

**Memperbaiki cacat pada `LAB-API-v1` `r26`.**

Cetakan menulis `Nystatin 1,25 ug/mL`. `r26` menyimpan `Concentration` sebagai angka **tanpa
satuan**.

1. Kadar tersimpan sebagai **dua bagian**: angka dan satuan.
2. Daftar satuan awalnya `ug/mL` dan `mg/L`, dan dapat diperluas.

**Kenapa ini cacat, bukan sekadar kurang rapi.** Alasannya sama persis dengan `LAB-DEC-041`
pada volume specimen: angka `1,25` yang tersimpan sendirian tidak dapat dibaca ulang siapa pun,
dan pola resistensi tidak dapat dihitung dari nilai yang satuannya berbeda-beda tanpa
tercatat.

**Kenapa tidak dikunci ke `ug/mL` saja.** Kesepuluh baris pada contoh memang `ug/mL`, tetapi
`mg/L` juga lazim. Mengunci satuan berarti nilai `mg/L` kelak tersimpan sebagai `ug/mL` **tanpa
satu pun galat terlihat** — kesalahan yang diam.

### BR-70 — Mikrobiologi mencakup biakan jamur (`LAB-DEC-116`)

**Memperluas cakupan `S4b`. Bukti pertama yang menunjukkannya.**

Contoh cetak yang diserahkan ternyata **kultur jamur**: `HASIL BIAKAN JAMUR : Candida albicans`
dan `HASIL RESISTENSI ANTIJAMUR` berisi Nystatin, Amphotericin, dan Fluconazole. Penelusuran
2026-09-21 menemukan **jamur nol kemunculan di seluruh backend**.

| Aturan | Isi |
|---|---|
| 1 | `LabOrganism` dan `LabAntibiotic` **tetap satu tabel masing-masing**. *Candida albicans* adalah organisme; Nystatin adalah agen antimikroba |
| 2 | Pemeriksaan memperoleh **penanda jenis biakan** — bakteri atau jamur |
| 3 | Penanda itu yang memilih **label cetak**: `BIAKAN JAMUR` / `RESISTENSI ANTIJAMUR` atau `BIAKAN BAKTERI` / `RESISTENSI ANTIBIOTIKA` |
| 4 | Dokumentasi `LabAntibiotic` **wajib diperbaiki** — hari ini ia tertulis khusus antibakteri, dan itu menyesatkan pemelihara berikutnya |

**Kenapa bukan dua model terpisah.** Struktur hasilnya **identik** — isolat, agen, interpretasi
`S`/`I`/`R`, kadar. Menggandakannya berarti dua tabel, dua layanan, dan dua layar untuk
perbedaan yang sebenarnya hanya label.

**Kenapa tidak dibiarkan tanpa penanda.** Cetakan akan menulis `HASIL RESISTENSI ANTIBIOTIKA`
untuk uji antijamur — dokumen klinis yang menyebut Nystatin sebagai antibiotik, dan itu salah
di hadapan dokter yang membacanya.

### BR-71 — Nomor cetak berdiri terpisah dari nomor order (`LAB-DEC-117`)

**Melengkapi `LAB-DEC-072`; tidak mengubahnya.**

Ketiga cetakan memakai nomor per disiplin per tahun — Mikrobiologi `26-1129`, Patologi Anatomi
`26.0919`, Patologi Klinik `25039254`. `OrderNumber` yang **sudah dibangun** berformat
`LAB-RSMMC-000000123`, nomor urut global.

1. Berdiri **satu nomor cetak** yang dialokasikan **per disiplin per tahun**.
2. `OrderNumber` **tetap** menjadi identitas internal dan sumber barcode Label Lab
   (`LAB-DEC-072` utuh).

**Kenapa dua nomor, bukan satu.** Keduanya menjawab pertanyaan berbeda: satu untuk sistem, satu
untuk dokumen yang dipegang pasien dan disebut lisan antarpetugas. Dan mengubah `OrderNumber`
berarti membongkar layanan alokasi, index unik, serta sumber barcode yang sudah berjalan — dan
pesanan lama terlanjur bernomor format lama.

### BR-72 — Tanggal pada cetakan punya pemetaannya sendiri (`LAB-DEC-118`)

**Menegaskan `LAB-DEC-096`, tidak mencabutnya.**

| Yang tercetak | Diturunkan dari |
|---|---|
| `Tanggal Terima` | Waktu **penerimaan fisik** specimen |
| `Tanggal Selesai` | `FinalizedAt` |
| `Tgl. Cetak` | Waktu pencetakan, dinamis |

Ruas `Waktu Efektif` dan `Waktu Issued` pada layar **tetap** seperti `LAB-DEC-096` — dari
`CollectedAt` dan `FinalizedAt`.

**Kenapa dua pemetaan berbeda dibiarkan hidup berdampingan.**

> Keduanya menjawab pertanyaan berbeda. Cetakan mencatat **lama pengerjaan laboratorium** —
> bahan masuk 3 Agustus, selesai 7 Agustus. `Waktu Efektif` menjawab **kapan bahan meninggalkan
> tubuh pasien**, dan itu yang bermakna klinis serta yang diminta ruas HL7. Menyamakan keduanya
> membuat bahan yang diambil Senin dan baru diterima Rabu tercatat efektif hari Rabu.

### BR-73 — Nama konsultan adalah data induk pengaturan (`LAB-DEC-119`)

Footer Mikrobiologi mencetak `Konsultan Mikrobiologi Klinik: Usman Chatib Warsa, PhD, SpMK-K,
Prof. dr.` — dan itu **bukan** `DR-LAB-002`, dr. Nabila Rahmawati Sp.MK, yang terdaftar sebagai
pemegang wewenang klinis Mikrobiologi.

1. Nama konsultan disimpan sebagai **pengaturan per disiplin**, dapat diubah kepala instalasi.
2. Ia **bukan** diturunkan dari pemegang wewenang klinis. Keduanya peran berbeda yang kebetulan
   sama-sama Mikrobiologi Klinik.
3. Patologi Anatomi memakai label **`Spesialis Patologi Anatomi`**, dan Patologi Klinik memakai
   **`Konsultan`** di bawah kop — **labelnya pun berbeda per disiplin**.

### BR-74 — `Petugas Otorisasi` adalah pihak yang merilis (`LAB-DEC-120`)

Cetakan Patologi Klinik mencetak **dua baris terpisah**: `Otorisasi oleh :` dan `Validasi oleh :`.

1. `Petugas Otorisasi` diisi pengguna yang melakukan **rilis**.
2. `Validasi oleh` diisi **pemvalidasi**, dan keduanya tidak boleh orang yang sama kecuali
   pengecualian `LAB-DEC-003` tercatat.
3. Ruasnya **dibangun sekarang** dan tampil kosong selama rilis belum ada.

> **Ini bukti lapangan langsung untuk prinsip empat mata.** Laboratorium ini **sudah**
> membedakan pemvalidasi dari pengotorisasi pada dokumen yang dicetak hari ini. Ia sekaligus
> menguatkan `LAB-DEC-097`: `Simpan Final` bukan rilis.

> ### ⚠ Akibat yang harus dibaca bersama keputusan ini
>
> **Cetakan Mikrobiologi BELUM DAPAT LENGKAP sampai `S4d` dibuka.** Rilis Mikrobiologi adalah
> `S4d`, dan ia tertahan `DEC-LAB-011`. Sampai penahan itu dijawab, kolom `Petugas Otorisasi`
> pada cetakan akan kosong. Itu **bukan** cacat yang perlu ditambal dengan mengisinya dari
> pencetak atau dari penulis hasil — keduanya akan membuat dokumen menyebut pihak yang salah
> sebagai pengesah.

### BR-75 — Temuan Profesor diajukan, bukan diputuskan sendiri (`LAB-DEC-121`)

`LAB-OPEN-029` tertahan justru karena penetapan wewenang klinis 2026-09-17 **nol memuat nama
bergelar Profesor**, sedangkan `LAB-DEC-067` mensyaratkan persetujuan Profesor sebelum hasil
dikirim kepada pasien. Bukti cetak ini menampilkan seorang **Profesor** sebagai Konsultan
Mikrobiologi Klinik.

1. Disusun **permintaan baru** kepada wewenang klinis yang melampirkan bukti cetak ini.
2. Isinya dua: apakah Konsultan Mikrobiologi Klinik adalah Profesor yang dimaksud
   `LAB-DEC-067`, dan bagaimana peran itu masuk `LAB-PERM-v1`.
3. **`LAB-OPEN-029` TIDAK ditutup di sini.**

**Kenapa tidak ditutup sendiri.** `LAB-OPEN-029` mencatat secara tegas bahwa ini **bukan
wewenang pemilik modul**. Menutupnya di sini berarti menetapkan siapa yang berhak menyetujui
pengiriman hasil kepada pasien tanpa satu pun pihak klinis menyatakannya. Yang berubah hari ini
bukan jawabannya, melainkan bahwa pertanyaannya **kini punya kandidat** yang sebelumnya tidak
ada.

---

## Amendment Pass Putaran 12 — Cetakan Mikrobiologi varian bakteri (2026-09-21)

**Sumber:** `LAB-EVD-006` — dua tangkapan layar cetakan `LABORATORIUM MIKROBIOLOGI` untuk
pemeriksaan `MO KUL SPUTUM KULTUR MO & RES`, No. Lab `26-1246`, diserahkan pemilik modul
2026-09-21. Ini **varian bakteri** yang `LAB-OPEN-039` catat sebagai kurang.

> **Putaran ini mengoreksi `LAB-DEC-116` yang berumur kurang dari satu jam.** Putaran 11
> menduga perbedaan bakteri dan jamur **hanya label**. Varian bakteri membuktikan bentuknya
> berbeda total. Dugaan itu diambil dari satu contoh cetak; contoh kedua membatalkannya.

### Bentuk yang terbaca

Label organismenya **`IDENTITAS : Branhamella catarrhalis`** — bukan `HASIL BIAKAN BAKTERI`
seperti dugaan `LAB-DEC-116`. Di bawahnya tabel **lima kolom**:

| ANTIBIOTIK | UG | R-S | Zona / mm | RESULT |
|---|---:|---|---:|---|
| AMPICILLIN | 10 | 13 - 17 | 20 | S |
| NETILMICIN | 30 | 12 - 15 | 13 | I |
| FOSFOMYCIN | 200 | 12 - 16 | 11 | R |
| GENTAMICIN | 10 | 12 - 15 | 0 | R |
| MEROPENEM | 10 | 13 - 16 | 31 | S |

Di atas tabel ada baris keterangan baku `Kode : R = Resistant, I = Intermediate, S = Sensitive`.

### BR-76 — `UG` dan rentang `R-S` berasal dari data induk (`LAB-DEC-122`)

**Menambal kekosongan `LAB-API-v1` `r26` yang nol mengenal keduanya.**

| Kolom cetak | Artinya | Sumbernya |
|---|---|---|
| `UG` | **Kandungan cakram** dalam mikrogram — Ampicillin 10, Cefoperazone 75, Fosfomycin 200 | Ruas baru pada `LabAntibiotic` |
| `R-S` | **Rentang breakpoint**, dua angka, misalnya `13 - 17` | Data induk breakpoint **per kombinasi organisme dan antibiotik** |

Keduanya **di-snapshot** ke baris hasil saat disimpan.

**Kenapa breakpoint per organisme, bukan per antibiotik saja.** CLSI menetapkannya begitu, dan
`LAB-DEC-039` sudah mewajibkan breakpoint **configurable dan tidak dihardcode**. Rentang
Ampicillin terhadap *Branhamella* tidak sama dengan terhadap *E. coli*.

**Kenapa di-snapshot.** Alasan yang sama dengan `OrganismNameSnapshot`: breakpoint diperbarui
ketika versi CLSI berganti, dan hasil tahun lalu tidak boleh berubah arti karenanya.

> **`LAB-DEC-115` tetap berlaku, dan `UG` bukan penggantinya.** Keduanya angka yang berbeda:
> `UG` adalah **kandungan cakram** pada metode difusi; MIC beserta satuannya adalah **nilai
> hasil** pada metode dilusi. Cetakan jamur menampilkan MIC; cetakan bakteri menampilkan `UG`.

### BR-77 — Interpretasi dihitung, boleh ditimpa beralasan (`LAB-DEC-123`)

**Mengubah `r26` yang mewajibkan analis mengetik `S`/`I`/`R`.**

Pemeriksaan terhadap **seluruh 22 baris** cetakan menunjukkan interpretasinya konsisten:

| Keadaan zona | Hasil | Contoh |
|---|---|---|
| Di **bawah** batas bawah | `R` | Fosfomycin zona 11 pada rentang 12-16 |
| **Di dalam** rentang | `I` | Netilmicin zona 13 pada rentang 12-15 |
| Di **atas** batas atas | `S` | Imipenem zona 32 pada rentang 13-16 |

**Aturan:**

1. Sistem mengisi `S`/`I`/`R` dari zona terhadap breakpoint.
2. Analis **boleh menimpanya**, dan alasannya **wajib** tercatat.
3. Bila breakpoint untuk kombinasi itu **belum terisi**, ruasnya kembali manual.

**Kenapa boleh ditimpa.** Resistensi intrinsik — kuman yang secara alami kebal walaupun zonanya
lebar — menuntut penilaian di luar rumus. Menutup penimpaan berarti melaporkan `Sensitive` pada
kuman yang pasti tidak akan merespons, dan itu berakhir pada antibiotik yang salah pilih.

**Kenapa tidak dibiarkan manual dengan peringatan saja.** Peringatan yang muncul 22 kali pada
satu layar berhenti dibaca. Dan mengetik ulang sesuatu yang dapat dihitung adalah 22 kesempatan
salah ketik pada setiap hasil.

### BR-78 — Dua penanda, bukan satu (`LAB-DEC-124`)

**Memperluas `LAB-DEC-116`. Dugaan "hanya label" pada putaran 11 terbukti salah.**

| Penanda | Menentukan | Nilai |
|---|---|---|
| **Jenis biakan** | Kata pada label | Bakteri → `IDENTITAS`; Jamur → `HASIL BIAKAN JAMUR` |
| **Metode uji** | **Bentuk tabel dan kolom mana yang berlaku** | Difusi cakram → `UG`, rentang, zona; Dilusi → MIC beserta satuannya |

Keduanya **bebas satu sama lain**: kultur jamur pun dapat diuji dengan difusi cakram.

**Kenapa dua sumbu, bukan satu.** Menggabungkannya berarti mengunci bakteri selalu difusi dan
jamur selalu dilusi. Bukti nol menyatakan itu, dan laboratorium yang menguji jamur dengan
difusi cakram akan mendapat bentuk tabel yang salah tanpa jalan keluar.

### BR-79 — Penanda set bakteri melekat pada pemetaan katalog (`LAB-DEC-125`)

**Menjawab pernyataan pemilik modul: tidak semua pemeriksaan Mikrobiologi memakai set bakteri.**

1. Berdiri **data induk pemetaan** milik Laboratorium yang menyatakan pemeriksaan mana memakai
   set bakteri, sejajar pola `LAB-DEC-087` pada kategori Patologi Anatomi.
2. Kepala instalasi mengelolanya.
3. Pemeriksaan tanpa set bakteri **tidak menampilkan** bagian isolat dan antibiogram sama
   sekali.

**Kenapa bukan diturunkan dari nama pemeriksaan.** Awalan `MO KUL` memang menandai kultur
mikroorganisme dan `JAM KUL` menandai kultur jamur — tetapi `LAB-DEC-087` **sudah menolak**
menurunkan kategori dari kata kunci nama, dan menurunkannya di sini mengulang persis hal yang
ditolak itu.

**Kenapa bukan kolom pada `MstProcedure`.** Tabel itu milik `master-data`. Menambah kolom di
sana menuntut koordinasi lintas modul, dan modul ini sudah dua kali tertahan pola yang sama
lewat `LAB-COORD-006` dan `MST-POS-WRITE`.

### BR-80 — Isolat boleh berdiri tanpa baris kepekaan (`LAB-DEC-126`)

Catatan pada cetakan menyebut **dua kuman** — *Streptococcus Alfa Haemolyticus* dan
*Branhamella catarrhalis* — tetapi hanya Branhamella yang punya tabel antibiogram.

1. **Kedua kuman tersimpan sebagai isolat.** Yang tidak diuji cukup nol baris kepekaan — bentuk
   ini sudah diizinkan ERD sejak awal (isolat 1 : 0..n kepekaan).
2. Isolat memperoleh **penanda apakah kepekaannya diuji**.
3. Cetakan tidak berubah: hanya isolat berantibiogram yang mendapat tabel `IDENTITAS`; sisanya
   muncul di area naratif.

**Kenapa tetap disimpan sebagai data.** Laporan pola kuman rumah sakit hanya dapat menghitung
kuman yang tersimpan sebagai data. Ditulis sebagai kalimat, pertanyaan *"berapa kali
Streptococcus Alfa Haemolyticus tumbuh tahun ini"* hanya dapat dijawab dengan membaca ribuan
catatan satu per satu.

### BR-81 — Kalimat penjelas baku tercetak otomatis (`LAB-DEC-127`)

Baris pertama Catatan berbunyi
`LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI.`

1. Kalimat itu disimpan sebagai **pengaturan**, dan tercetak sendiri pada setiap hasil
   ber-set-bakteri.
2. Kepala instalasi dapat mengubahnya.
3. **Catatan bebas milik analis tetap ada** di bawahnya, terpisah.

**Kenapa dipisahkan dari catatan bebas.** Ia penjelasan klinis yang berlaku bagi semua hasil
difusi cakram. Diketik ulang setiap kali, suatu hari ia terlupa pada hasil yang justru
membutuhkannya — dan salah ketik pada kalimat klinis nol akan tertangkap siapa pun.

### BR-82 — Zona bernilai nol adalah data, bukan kekosongan (`LAB-DEC-128`)

Sebelas dari 22 baris bernilai `0`, dan **seluruhnya** berhasil `R`.

| Nilai | Artinya |
|---|---|
| `0` | **Nol zona hambat terbentuk.** Pengukuran sah, dan temuan terkuat bahwa kuman kebal |
| Kosong | **Belum diukur**, atau metodenya memang tidak menghasilkan zona |

Keduanya disimpan berbeda dan dibedakan pada layar. Validasi **tidak boleh menolak** nilai `0`,
dan **tidak boleh mewajibkan** zona terisi — metode dilusi nol menghasilkan zona sama sekali.

---

## Amendment Pass Putaran 13 — Dataset specimen SNOMED CT (2026-09-21)

**Sumber:** `LAB-EVD-007` — `snomedct_specimen_dikelompokan_berdasarkan_jenis.xlsx`, diserahkan
pemilik modul 2026-09-21. **Menutup `LAB-OPEN-040`.** Analisis lengkapnya pada
[`evidence/2026-09-21-dataset-specimen-snomedct.md`](evidence/2026-09-21-dataset-specimen-snomedct.md).

> **Jumlah 1.767 terverifikasi apa adanya.** Yang tidak terverifikasi adalah **bentuknya**:
> datasetnya bertingkat **tiga**, sedangkan `LAB-DEC-098` merancang dua — dan tingkat yang
> dipilih artifact ternyata yang paling sedikit gunanya.

### BR-83 — Specimen tetap dua tingkat, tetapi dari kolom yang berbeda (`LAB-DEC-129`)

**Mengoreksi `LAB-DEC-098` pada pilihan kolomnya; bentuk dua tingkatnya dipertahankan.**

| Tingkat | Kolom sumber | Nilai | Tabel |
|---|---|---:|---|
| 1 — **Specimen** | `jenis_specimen` | **31** | `LabSpecimenType` **diperluas dari 7** |
| 2 — **Spesifik Specimen** | `display` | **1.767** | `LabSpecimenDetailType` |
| *(atribut)* | `subjenis_specimen` | 85 | Kolom pada baris detail, **bukan** tingkat pilihan |

**Kenapa `subjenis_specimen` turun menjadi atribut.**

> **21 dari 31 kelompok hanya punya satu subjenis.** Menjadikannya tingkat pilihan berarti
> pada dua pertiga data petugas melewati layar yang nol punya alternatif — klik yang tidak
> menanyakan apa pun. Ia tetap disimpan karena berguna bagi pelaporan: *"berapa banyak bahan
> dari kelompok cairan pleura"* hanya dapat dijawab dari kolom itu.

**Kenapa perluasan 7 → 31 aman.** Ketujuh nilai yang sudah ter-seed **seluruhnya punya
padanan** di antara 31 kelompok — Blood→Darah, Body Fluid→Cairan Tubuh, dan seterusnya. Ini
penambahan 24 baris beserta penggantian nama tujuh yang ada, bukan pembongkaran.

**Contoh pemakaian:**

> Petugas menerima tabung berisi cairan pleura. Ia memilih **Specimen** = `Cairan Tubuh`, lalu
> **Spesifik Specimen** = `Pleural fluid specimen`. Yang tersimpan juga
> `subjenis_specimen` = `Cairan pleura`, tanpa petugas pernah memilihnya.

### BR-84 — Seluruh 1.767 diimpor; yang berkonfidensi rendah dinonaktifkan (`LAB-DEC-130`)

**Menjalankan maksud `LAB-DEC-099` dengan dasar yang kini terlihat.**

1. **1.601 baris** berkonfidensi `Tinggi` dan `Sedang` diimpor dalam keadaan **aktif**.
2. **166 baris** berkonfidensi `Rendah` diimpor dalam keadaan **nonaktif**.
3. Kepala instalasi **mengaktifkan** yang ternyata dibutuhkan, bukan menambahkan dari nol.

**Kenapa yang `Rendah` dinonaktifkan, bukan dibuang.** Ke-166 baris itu menyebut **lokasi tanpa
menyebut bahan** — `Specimen from abdominal cavity`, `Specimen from anus`. Sebagai pilihan
rutin ia mengaburkan pelaporan bahan; tetapi membuangnya berarti kehilangan kode SNOMED-nya,
dan suatu hari salah satunya memang dibutuhkan.

**Kenapa tidak menunggu sesi penyaringan lebih dulu.** `LAB-DEC-099` menyerahkan penyaringan
kepada kepala instalasi, dan itu tetap berlaku — yang berubah, ia **memangkas daftar yang
sudah jalan** alih-alih menghadapi halaman kosong. Menunggu berarti `BE-LAB-55` membangun
tabel yang nol dapat diuji dengan data nyata, dan itu pola yang sudah dua kali menahan modul
ini lewat `LAB-COORD-006` dan `MST-POS-WRITE`.

### BR-85 — Nama Indonesia disediakan bertahap (`LAB-DEC-131`)

**Melaksanakan `LAB-DEC-008` tanpa menahan slice.**

1. Setiap baris punya **nama Indonesia** dan **nama Inggris**.
2. Nama Indonesia **boleh kosong**; layar menampilkan Inggris selama belum terisi.
3. Pencarian menerima **keduanya**, dan pemeriksaan duplikasi membandingkan keduanya sesuai
   `RULE-007`.
4. Harus ada cara melihat **mana yang belum diterjemahkan**.

**Kenapa tidak diterjemahkan lebih dulu.** 1.767 terjemahan istilah anatomi adalah pekerjaan
berminggu-minggu, dan menahan `BE-LAB-55` beserta `FE-LAB-32` karenanya tidak sebanding. Yang
lebih memberatkan: **terjemahan anatomi yang keliru lebih berbahaya daripada istilah Inggris
yang dibiarkan** — petugas yang ragu pada istilah asing akan bertanya, sedangkan terjemahan
yang salah tampak meyakinkan.

### BR-86 — Kode sendiri, SNOMED CT sebagai rujukan terpisah (`LAB-DEC-132`)

**Melengkapi `LAB-DEC-098`.**

1. `DetailTypeCode` tetap **milik Laboratorium**, mengikuti pola `LabOrganism`.
2. Kode SNOMED CT disimpan pada **kolom tersendiri** yang boleh kosong.

**Kenapa SNOMED tidak dijadikan kode utama.** Nilai yang kelak ditambahkan kepala instalasi
lewat jalan keluar `Lainnya` **nol punya kode SNOMED**. Memaksakannya sebagai kode utama
berarti baris lokal diberi kode karangan yang menyerupai standar internasional — dan kode
karangan di dalam kolom yang dianggap standar adalah kesalahan yang sangat sulit ditemukan
kemudian.

**Kenapa SNOMED tetap disimpan.** Datanya ada di tangan hari ini. Ketika rumah sakit kelak
bertukar data dengan sistem luar, pemetaan 1.767 baris tidak perlu dikerjakan ulang dari nol.

---

## Amendment Pass Putaran 14 — PRD Hasil Pemeriksaan Patologi Klinik & Mikrobiologi (dibuka dan selesai 2026-09-24)

> **Hasil putaran:** keempat belas pertentangan dan keenam butir baru tertutup lewat
> `LAB-DEC-133`..`LAB-DEC-145` serta `LAB-FE-015`/`LAB-FE-016`; `AC-196`..`AC-220` ditambahkan;
> 45 koreksi PRD dicatat pada bagian *Koreksi yang wajib masuk revisi PRD*. **Nol keputusan
> terkunci dicabut.** Yang tersisa di luar wewenang pemilik modul: `PRD1-CLIN-01` dan
> `LAB-COORD-015`.

**Sumber:** `LAB-EVD-008` — PRD *Modul Laboratorium - Hasil Pemeriksaan Patologi Klinik &
Mikrobiologi* berstatus `Draft Requirement`, ditempel pemilik modul pada sesi 2026-09-24.
**Disimpan verbatim** pada
[`evidence/2026-09-24-prd-hasil-patologi-klinik-mikrobiologi.md`](evidence/2026-09-24-prd-hasil-patologi-klinik-mikrobiologi.md).
Penulis PRD belum disebutkan.

> **Baca lebih dulu.** PRD ini ditulis seolah blueprint belum ada. Empat belas klaimnya
> bertentangan dengan keputusan yang sudah `approved`; empat di antaranya **sudah pernah
> ditolak** tiga hari sebelumnya pada putaran 9, dan dua menyentuh prinsip empat mata
> `LAB-DEC-003` yang **ditandatangani pihak klinis** 2026-09-17 (`LAB-DEC-079`). Bagian ini
> **mencatat rekonsiliasi**, bukan membuka izin membangun.

### Keadaan source saat putaran ini dibuka

Dibaca langsung pada backend `ddeb5ed8` (branch `yoga`) dan frontend `72607a087`, 2026-09-24.

| Hal | Keadaan hari ini |
|---|---|
| Pengisian hasil Patologi Klinik (`S4a`) | `LabExamination` mencatat `ResultEnteredAt` dan `ResultEnteredByUserId` — pengisi hasil adalah **pengguna yang menyimpan** |
| Draft/Final pada Patologi Klinik | **Tidak ada.** `FinalizedAt`/`FinalizedByUserId` pada `LabExamination` dibangun untuk Mikrobiologi (`S4b`, `LAB-DEC-097`) |
| `ValidatedAt` / `ReleasedAt` | **Belum dibangun.** `S4` dan `S4d` tertahan `DEC-LAB-011` |
| Gerbang WhatsApp dan pembangkit PDF | **Nol pada platform**, diverifikasi ulang 2026-09-23 (`LAB-COORD-011`) |
| Kode QR pada cetakan Patologi Klinik | Ada pada cetakan lapangan (`LAB-EVD-005` bagian 4); isi dan kegunaannya **nol diputuskan** |
| Capability map | Revision 4 dipindai pada `981e002c`; `HEAD` kini `ddeb5ed8`. **Berpotensi basi** — tidak menahan wawancara, tetapi impact scan `/qv-trace` dianjurkan sebelum desain |

### Kedudukan PRD (`LAB-DEC-133`)

**PRD dicatat sebagai bukti `LAB-EVD-008` dan direkonsiliasi butir per butir. Keputusan yang
sudah `approved` tetap berlaku kecuali pemilik modul membukanya ulang secara eksplisit per
butir, dan PRD direvisi mengikuti hasilnya.**

**Kenapa bukan baseline baru.**

> Menjadikan PRD baseline berarti menandai `superseded` belasan keputusan sekaligus —
> termasuk `LAB-DEC-003` yang hanya dapat diubah `DR-LAB-001` dan `DR-LAB-002`, bukan pemilik
> modul. Pekerjaan `S4a` dan `S4b` yang sudah berdiri harus dibongkar, sementara `S4` dan `S4d`
> tetap tertahan `DEC-LAB-011`. Pola butir per butir sudah dua kali dipakai (putaran 8 dan 9),
> dan dua kali menyelesaikan seluruh pertentangan tanpa satu pun keputusan terkunci dicabut
> diam-diam.

**Contoh penerapan:**

> PRD menulis *"Dokter Laboratorium merupakan satu-satunya role yang dapat melakukan Simpan
> Final"*. Keputusan yang berlaku, `LAB-DEC-022`, memberi kewenangan **per orang**, bukan per
> jabatan. Sampai pemilik modul membuka `LAB-DEC-022`, yang berlaku tetap per orang, dan
> kalimat PRD itu masuk daftar koreksi PRD.

### Batas scope putaran ini (dikonfirmasi pemilik modul 2026-09-24)

Pemilik modul memilih kedudukan di atas tanpa mengoreksi daftar berikut.

**Di dalam scope:**

1. Pengisian hasil serta Draft/Final Patologi Klinik (`S4a`) dan Mikrobiologi (`S4b`).
2. Titik sentuh ke validasi dan rilis (`S4`, `S4d`): siapa, dan kapan hasil terkunci.
3. Penandaan dan konfirmasi nilai kritis Patologi Klinik dan Mikrobiologi — titik sentuhnya ke `S5`.
4. Amendment hasil — titik sentuhnya ke `S6`.
5. Cetak, unduh, dan pengiriman hasil ke pasien lewat WhatsApp beserta counter-nya (`S17`).
6. QR Result Viewer.
7. Informasi specimen, set bakteri, dan antibiogram pada halaman hasil Mikrobiologi.

**Di luar scope — sudah punya rumahnya sendiri:**

| Hal | Rumahnya |
|---|---|
| Gerbang WhatsApp dan pembangkit berkas PDF | `LAB-COORD-011`, pemilik platform |
| Layanan terjemahan untuk preview bilingual | `LAB-COORD-013`, pemilik platform + pemilik modul |
| HL7 / SATUSEHAT `DiagnosticReport` | `LAB-COORD-012`, pemilik platform; PRD sendiri menaruhnya di *Future Enhancement* |
| Identitas, nomor telepon, dan email pasien; data dokter | `master-data`, **hanya dibaca** |
| Jadwal jaga dokter | Human Resource, `LAB-COORD-014` |
| Patologi Anatomi | Tidak disentuh PRD; keputusannya tetap |
| Integrasi LIS / alat | `LAB-DEC-005` — Rilis 1 diketik manual |
| Isi aturan klinis: angka batas kritis, kombinasi kritis Mikrobiologi, pemegang kewenangan validasi | `DR-LAB-001`, `DR-LAB-002`, kepala instalasi — `DEC-LAB-011`, `DEC-LAB-012`, `LAB-OPEN-041` |

### Pertentangan dengan keputusan yang berlaku

Arti status: `menunggu` belum ditanyakan; `ditanyakan` sedang menunggu jawaban;
`tetap-blueprint` keputusan lama tetap dan PRD dikoreksi; `diadopsi` PRD diterima dan keputusan
lama diamandemen; `bukan-wewenang-pemilik` hanya pihak klinis atau platform yang dapat
mengubahnya.

| ID | Bagian PRD | PRD menyatakan | Keputusan yang berlaku | Status |
|---|---|---|---|---|
| `PRD1-CONF-01` | 5.1, 5.2, FR-PK-002 | Dokter Lab mengisi hasil Patologi Klinik; Petugas Lab tidak dapat mengubahnya | `LAB-DEC-005`: hasil Patologi Klinik diketik **analis**. Dokter Lab menjadi pengisi hanya pada Patologi Anatomi (`LAB-DEC-090`) | `tetap-blueprint` — `LAB-DEC-134`, 2026-09-24 |
| `PRD1-CONF-02` | 5.2, BP-001 | Dokter Lab mengisi **dan** menekan Simpan Final sendiri | `LAB-DEC-003` empat mata, ditandatangani `DR-LAB-001`/`DR-LAB-002` (`LAB-DEC-079`); cetakan Patologi Klinik memisahkan *Validasi oleh* dan *Otorisasi oleh* (`LAB-DEC-120`) | `tetap-blueprint` — `LAB-DEC-134`: Dokter Lab dibaca sebagai pemvalidasi/pengotorisasi, bukan pengisi. Aturan empat mata sendiri tetap `bukan-wewenang-pemilik` |
| `PRD1-CONF-03` | 5.2, 12 | Hak final melekat pada role "Dokter Laboratorium"; *Role based access* | `LAB-DEC-022`: kewenangan validasi dan rilis **per orang**, bukan jabatan; pemegangnya `DEC-LAB-011` | `tetap-blueprint` — `LAB-DEC-142`: dua lapis, jabatan calon dan penunjukan per orang |
| `PRD1-CONF-04` | 2, FR-PK-002, 8 | Simpan Final = Selesai = hasil terkunci | `LAB-DEC-088`/`LAB-DEC-097`: Final berarti penulis selesai menulis, **bukan rilis**; baca-saja berlaku sesudah rilis. Patologi Klinik hari ini **nol** konsep Draft/Final | `tetap-blueprint` — `LAB-DEC-135`: pola Final = selesai menulis diperluas ke Patologi Klinik |
| `PRD1-CONF-05` | 8 | Lima status: Draft, Dalam Pemeriksaan, Definitif, Final/Selesai, Amendment | `LAB-DEC-080`: validasi dan rilis dicatat sebagai **fakta**, nol status baru; `LAB-DEC-106`/`LAB-DEC-114`: Definitif adalah fakta konsultasi dan kualifikasi cetak, bukan status | `tetap-blueprint` — `LAB-DEC-135`: kelima status menjadi label tampilan turunan; Definitif tidak berlaku bagi Patologi Klinik |
| `PRD1-CONF-06` | 2, FR-PK-002 | Status dipegang pada tingkat order | `LAB-DEC-008`: status per pemeriksaan dan rilis sebagian boleh; yang berjumlah satu per order adalah dokumen final (`LAB-DEC-067`) | `tetap-blueprint` — `LAB-DEC-135`: Final per pemeriksaan; *Dalam Pemeriksaan*/*Selesai* menjadi label turunan pada order |
| `PRD1-CONF-07` | 2, 6.2 | Satu hasil Mikrobiologi per No. Order | `LAB-DEC-095`: hasil per pemeriksaan | `tetap-blueprint` — `LAB-DEC-144`, penolakan 2026-09-21 ditegaskan ulang |
| `PRD1-CONF-08` | FR-MB-003 | `Lainnya` → ketik nama → cek duplikasi → langsung menjadi pilihan tetap | `LAB-DEC-040`/`LAB-DEC-098`: `Lainnya` masuk daftar pantau, hanya kepala instalasi yang menaikkannya; 1.767 entri SNOMED sudah diimpor (`LAB-DEC-130`) | `tetap-blueprint` — `LAB-DEC-144`, penolakan 2026-09-21 ditegaskan ulang |
| `PRD1-CONF-09` | FR-MB-004 | Ruas Analis diisi pada form | `LAB-DEC-105`: diturunkan dari pengguna yang menyimpan, baca-saja | `tetap-blueprint` — `LAB-DEC-144`, penolakan 2026-09-21 ditegaskan ulang |
| `PRD1-CONF-10` | FR-MB-005 | Subbakteri | `LAB-DEC-102`: tidak dibangun | `tetap-blueprint` — `LAB-DEC-144`, penolakan 2026-09-21 ditegaskan ulang |
| `PRD1-CONF-11` | BP-003 | Kritis → konfirmasi dokter → WhatsApp; sistem mencatat waktu kirim | `LAB-DEC-004`: wajib siapa, kepada siapa, kapan, lewat apa, **dan bukti pembacaan ulang**; `LAB-DEC-012`/`LAB-DEC-016`: pemberitahuan tersimpan pada kotak masuk platform; `LAB-P0-004` terbuka | `tetap-blueprint` — `LAB-DEC-136`: WhatsApp saluran pengantar; sah tidaknya balasan WA sebagai bukti baca ulang diajukan sebagai `PRD1-CLIN-01` |
| `PRD1-CONF-12` | FR-PK-004 | Amendment: alasan, nilai sebelum dan sesudah, user dan waktu | `LAB-DEC-007`: dokter pemesan **otomatis diberi tahu**; `LAB-DEC-082`: alasan dari daftar terkendali, versi bernomor; `LAB-DEC-020`: addendum bila kunjungan sudah ditutup; `DEC-LAB-014` terbuka | `tetap-blueprint` — bawaan `LAB-DEC-133`, tidak dibuka ulang pemilik modul 2026-09-24; koreksi PRD 18..21. Sisa klinisnya tetap `DEC-LAB-014` |
| `PRD1-CONF-13` | FR-PK-005 | Syarat kirim ke pasien cukup status Selesai | `LAB-DEC-067`: wajib persetujuan Profesor **dan** Dokter Lab; `LAB-OPEN-029` terbuka | `tetap-blueprint` — `LAB-DEC-139`: syarat `LAB-DEC-067` dipertahankan dan diperluas ke cetak serta unduh |
| `PRD1-CONF-14` | 13 (AC Mikrobiologi) | "Critical ditemukan → Notifikasi dokter aktif" | `LAB-DEC-103`: isi aturan kritis milik `DR-LAB-002`; selama `LAB-OPEN-041` kosong penanda **tidak menyala** | `bukan-wewenang-pemilik` — ditutup 2026-09-24 sebagai koreksi PRD 35; isi aturannya tetap `LAB-OPEN-041` |

**Contoh kenapa `PRD1-CONF-11` adalah celah keselamatan, bukan beda istilah.**

> Pukul 02.10 Kalium seorang pasien keluar 7,2 mmol/L. Petugas menekan Konfirmasi, sistem
> mengirim WhatsApp kepada dr. Bagas dan mencatat "terkirim 02.11". dr. Bagas sedang menangani
> pasien lain dan ponselnya tertinggal di ruang jaga. Menurut PRD, kewajiban pelaporan sudah
> tuntas pukul 02.11. Menurut `LAB-DEC-004`, belum: tidak ada bukti dr. Bagas menerima dan
> membaca ulang angkanya, sehingga hasil itu tetap muncul di daftar pantau *nilai kritis belum
> dilaporkan*.

### Butir baru yang belum pernah diputuskan

| ID | Bagian PRD | Hal | Status |
|---|---|---|---|
| `PRD1-NEW-01` | 4.1, 9 | **QR Result Viewer.** Cetakan Patologi Klinik lapangan memang berkode QR (`LAB-EVD-005`), tetapi isi QR dan siapa yang boleh membuka hasil lewat QR nol diputuskan. Menyangkut privasi data pasien | `diputuskan` — `LAB-DEC-140`: QR verifikasi keaslian dan status dokumen, bukan pembuka hasil |
| `PRD1-NEW-02` | BP-001 langkah 7 | **Konsultasi eksternal pada Patologi Klinik.** Fakta konsultasi hari ini hanya ada untuk Mikrobiologi (`LAB-DEC-106`) | `diputuskan` — `LAB-DEC-141`: fakta opsional, pola Mikrobiologi, tanpa Definitif |
| `PRD1-NEW-03` | 10 | **Email pasien** disebut pada integrasi Master Pasien, tanpa satu pun FR yang memakainya. Platform nol sarana surel (F7) | `diputuskan` — `LAB-DEC-145`: dihapus dari PRD |
| `PRD1-NEW-04` | FR-PK-003 | **Warna flag** — L/H kuning, kritis merah. Hari ini `DEV_DISCRETION` (`LAB-FE-002`); kewajiban kritis tampil menonjol sudah dikunci `LAB-FE-005` | `diputuskan` — `LAB-DEC-145`, `LAB-FE-015`: makna dikunci, penanda wajib huruf/teks |
| `PRD1-NEW-05` | 12 | **Larangan modal untuk input utama.** Hari ini `DEV_DISCRETION` (`LAB-FE-002`) | `diputuskan` — `LAB-DEC-145`, `LAB-FE-016`: diterima untuk isian hasil utama |
| `PRD1-NEW-06` | FR-MB-002 | **Lokasi Specimen dan Metode Pengambilan.** Masih `S2b`, `BUSINESS_DECISION_REQUIRED` | `diputuskan` — `LAB-DEC-145`: tetap `S2b` |
| `PRD1-OPEN-01` | FR-PK-001 | **Nomor Transaksi dan Nomor Mutasi.** Keduanya tercetak pada cetakan Patologi Klinik lapangan (`LAB-EVD-005` bagian 4) dan diminta PRD, tetapi arti dan sumber datanya belum pernah dipastikan. Ditemukan saat menyisir PRD, tidak ditanyakan pada putaran ini | `terbuka` — tidak memblokir |

### Yang sudah selaras — nol keputusan baru

Cito dan Diplo per pemeriksaan (`LAB-DEC-026`, `LAB-DEC-089`); counter `Terkirim ke Pasien`
bertambah hanya bila pengiriman berhasil (`LAB-DEC-066`); nomor WhatsApp pasien dari data induk
dan baca-saja (`LAB-DEC-067`); tombol cetak, unduh, dan kirim dipegang Petugas Lab
(`LAB-DEC-068`); Informasi Specimen dapat disunting pada halaman hasil (`LAB-DEC-107`);
breakpoint dapat diatur dan `S`/`I`/`R` dihitung sistem (`LAB-DEC-122`, `LAB-DEC-123`); Dokter
Konfirmator dari DPJP atau dokter bertugas dengan nomor baca-saja (`LAB-DEC-108`,
`LAB-DEC-111`); cetak multi halaman dengan kop berulang (`LAB-DEC-110`).

### BR-87 — Analis mengisi hasil Patologi Klinik; Dokter Lab mengesahkan (`LAB-DEC-134`)

**Menegakkan `LAB-DEC-005`. Menutup `PRD1-CONF-01` dan `PRD1-CONF-02` ke arah blueprint.**

**Aturan:**

1. Hasil Patologi Klinik tetap **diketik analis**, persis `LAB-DEC-005`. Pengisi tercatat
   otomatis dari pengguna yang menyimpan (`ResultEnteredByUserId`), tidak dipilih.
2. **Dokter Lab** pada PRD dibaca sebagai pihak yang **memvalidasi dan mengotorisasi** hasil
   Patologi Klinik — dua baris *Validasi oleh* dan *Otorisasi oleh* pada cetakan
   (`LAB-DEC-120`) — **bukan** sebagai pengisi hasil.
3. **Petugas Lab** pada PRD dibaca sebagai petugas **administrasi** laboratorium — pemegang
   tombol cetak, unduh, dan kirim sesuai `LAB-DEC-068` — dan **bukan** analis. Kalimat PRD
   *"Petugas Laboratorium tidak dapat mengubah hasil Patologi Klinik"* karena itu **tetap
   benar** dan dipertahankan.
4. Prinsip empat mata `LAB-DEC-003` tetap berlaku apa adanya, termasuk jalur pengecualiannya.
   Keputusan ini tidak mengubahnya dan memang tidak berwenang mengubahnya.
5. Butir 2 menetapkan **peran pada alur**, bukan siapa pemegangnya. Kewenangan validasi dan
   rilis tetap diberikan **per orang** (`LAB-DEC-022`), dan daftar pemegangnya tetap
   `DEC-LAB-011`. Apakah setiap Dokter Lab otomatis memegangnya adalah pertanyaan Q9
   (`PRD1-CONF-03`), bukan bagian keputusan ini.

**Contoh:**

> Analis Sari mengetik Kalium 7,5 mmol/L padahal alat menunjukkan 3,5. Ia menyimpan, dan
> sistem mencatat Sari sebagai pengisi. dr. Aditya membuka hasil untuk divalidasi, melihat
> angka yang tidak sesuai dengan gambaran klinis pasien, lalu mengembalikannya. Salah ketik itu
> tertangkap **karena** pengisi dan pengesah dua orang berbeda.
>
> Bila PRD diikuti apa adanya — Dokter Lab mengisi sekaligus memfinalkan — orang yang salah
> mengetik adalah orang yang sama yang mengesahkan, dan angka 7,5 keluar sebagai hasil pasien.

**Kenapa tidak mengizinkan Dokter Lab ikut mengisi.** Setiap kali Dokter Lab mengisi lalu
memvalidasi sendiri, hasil lewat jalur pengecualian `BR-01`. Bila itu sering, pengecualian
menjadi kebiasaan — persis yang diperingatkan `BR-18`, dan semua pengujiannya tetap lulus.

**Akibat pada source:** nol. Pengisian hasil Patologi Klinik yang sudah berdiri mencatat
pengisi dari pengguna yang menyimpan, dan itu tetap benar.

**Acceptance criteria:** tidak bertambah. `AC-01` dan `AC-02` sudah mengunci empat mata pada
titik validasi.

### BR-88 — Patologi Klinik memakai Draft dan Final seperti dua disiplin lain (`LAB-DEC-135`)

**Menyalin pola `LAB-DEC-088` (Patologi Anatomi) dan `LAB-DEC-097` (Mikrobiologi) ke Patologi
Klinik. Menutup `PRD1-CONF-04`, `PRD1-CONF-05`, dan `PRD1-CONF-06`.**

**Aturan:**

1. Analis menyimpan hasil Patologi Klinik sebagai **Draft** atau **Final**. Final dilakukan
   **per pemeriksaan** dan berarti **penulis selesai menulis** — dicatat sebagai fakta
   `FinalizedAt` dan `FinalizedByUserId`, dua kolom yang sudah ada pada `LabExamination`.
2. Hanya hasil yang sudah **Final** yang masuk antrean validasi dan dapat divalidasi.
3. Selama **belum divalidasi**, hasil yang sudah Final boleh dibuka kembali (**Reopen**):
   `FinalizedAt` dikosongkan, dan jejak siapa serta kapan membukanya tetap tercatat.
4. Hasil **terkunci sesudah dirilis**, bukan sesudah Final. Perubahan sesudah rilis hanya lewat
   koreksi (`LAB-DEC-007`, `LAB-DEC-082`).
5. **Nol status tersimpan baru** (`LAB-DEC-080`). Kelima status PRD menjadi **label tampilan
   turunan**:

| Label | Berlaku pada | Diturunkan dari |
|---|---|---|
| Draft | Pemeriksaan | Hasil sudah diisi, `FinalizedAt` kosong |
| Final | Pemeriksaan | `FinalizedAt` terisi, belum dirilis |
| Dalam Pemeriksaan | Order | Masih ada pemeriksaan yang tidak batal pada order itu yang belum dirilis |
| Selesai | Order | Seluruh pemeriksaan yang tidak batal sudah dirilis — saat itulah dokumen final order `LAB-DEC-067` lengkap |
| Amendment | Pemeriksaan | Sedang dikoreksi sesudah rilis — baru bermakna ketika `S6` berdiri |
| Definitif | — | **Tidak berlaku bagi Patologi Klinik.** Tetap kualifikasi hasil Mikrobiologi (`LAB-DEC-114`) |

PRD menggabungkan *Final/Selesai* menjadi satu baris. Keduanya dipisah di sini karena berlaku
pada tingkat berbeda: **Final** milik satu pemeriksaan, **Selesai** milik seluruh order.

> **Diamandemen 2026-09-25 oleh `LAB-DEC-156` (BR-107).** Label pemeriksaan **Final** kini
> ditampilkan **Menunggu Validasi**, dan tindakannya bernama **Pemeriksaan Selesai**; *Menunggu
> Hasil*, *Tervalidasi*, dan *Dirilis* ditambahkan. Tabel di atas dibiarkan sebagai jejak. Label
> order *Dalam Pemeriksaan* dan *Selesai* **tidak berubah**, dan sejak `LAB-DEC-154` status order
> `Completed` pun hanya dapat terjadi sesudah seluruh pemeriksaan tidak batal dirilis.

**Contoh 1 — kenapa perlu Final:**

> Pukul 09.05 analis Sari menyimpan Kalium 6,4 mmol/L sebagai **Draft**, lalu melihat
> sampelnya agak hemolisis dan mengulang pemeriksaan. Selama itu Kalium **tidak** muncul di
> antrean dr. Aditya. Pukul 09.30 hasil ulangnya 4,6; Sari menggantinya dan menekan **Final**.
> Baru saat itu dr. Aditya dapat memvalidasinya. Tanpa tahap Final, angka 6,4 dapat
> tervalidasi pukul 09.20.

**Contoh 2 — kenapa per pemeriksaan, bukan per order:**

> Order `LAB-RSMMC-000000123` berisi Kalium cito dan Hemoglobin. Kalium Final pukul 09.10,
> divalidasi dan dirilis pukul 09.15 — dokter jaga langsung dapat membacanya. Hemoglobin baru
> Final pukul 09.40 dan dirilis pukul 09.50. Order berlabel **Dalam Pemeriksaan** sampai 09.50,
> lalu **Selesai**. Bila Final dilakukan per order seperti tulisan PRD, Kalium cito tertahan
> 35 menit menunggu Hemoglobin, dan `LAB-DEC-008` dilanggar.

**Akibat pada source:** perluasan kecil `S4a`. Kolomnya sudah ada; yang belum ada adalah
tindakan Final dan Reopen untuk Patologi Klinik serta penyaring antrean validasi. Bentuk
endpoint-nya diputuskan pada tahap desain, bukan di sini.

**Yang dibuka keputusan ini — `PRD1-FOLLOW-01`.** Butir 3 membatasi Reopen sampai **sebelum
validasi**. Maka perlu diputuskan: bila pemvalidasi menemukan salah pada hasil yang **sudah
divalidasi tetapi belum dirilis**, bagaimana hasil dikembalikan kepada analis? Ditanyakan
bersama Q5.

### BR-89 — WhatsApp mengantar kabar hasil kritis, bukan membuktikan pelaporan (`LAB-DEC-136`)

**Menegakkan `LAB-DEC-004`. Menutup `PRD1-CONF-11` ke arah blueprint.**

**Aturan:**

1. WhatsApp kepada Dokter Konfirmator adalah **saluran pengantar**: ia mempercepat kabar
   sampai, tetapi **tidak menuntaskan** kewajiban pelaporan.
2. **Waktu kirim WhatsApp** dan **waktu dilaporkan** disimpan sebagai **dua fakta terpisah**.
   Pengiriman yang gagal tercatat sebagai gagal dan tidak mengubah kewajiban apa pun.
3. Pelaporan hasil kritis **tuntas hanya bila** petugas mencatat kelima isian `LAB-DEC-004`:
   siapa melapor, kepada siapa, kapan, lewat apa, dan **bukti pembacaan ulang**. Sampai itu
   lengkap, hasil tetap di daftar pantau *nilai kritis belum dilaporkan*.
4. Alur pelaporan kritis **tidak bergantung** pada gerbang WhatsApp. Selama `LAB-COORD-011`
   belum ada, pelaporan tetap dapat dituntaskan lewat telepon.
5. Berlaku untuk jalur kritis **Patologi Klinik dan Mikrobiologi**. Patologi Anatomi berada di
   luar putaran ini, tetapi tunduk pada `LAB-DEC-004` yang sama.
6. Apakah **balasan WhatsApp dokter yang menyebut ulang nilainya** boleh diterima sebagai bukti
   pembacaan ulang **bukan wewenang pemilik modul**. Diajukan kepada `DR-LAB-001` (Patologi
   Klinik) dan `DR-LAB-002` (Mikrobiologi) sebagai `PRD1-CLIN-01`, bertaut `LAB-P0-004`.
   Sampai dijawab, bukti baca ulang harus datang dari percakapan langsung, misalnya telepon.

**Contoh:**

> Pukul 02.10 Kalium pasien di Bangsal Melati keluar 7,2 mmol/L. Petugas memilih dr. Bagas
> sebagai Dokter Konfirmator; WhatsApp terkirim pukul 02.11 dan tercatat sebagai **waktu
> kirim**. Hasil tetap di daftar pantau. Pukul 02.18 dr. Bagas menelepon balik, petugas
> menyebut *"Kalium tujuh koma dua"*, dr. Bagas mengulanginya, dan petugas mencentang bukti
> baca ulang. Pukul 02.18 tercatat sebagai **waktu dilaporkan**, dan hasil keluar dari daftar
> pantau.
>
> Bila ponsel dr. Bagas tertinggal di ruang jaga dan ia tidak pernah menelepon balik, daftar
> pantau tetap menampilkan hasil itu — dan kepala instalasi dapat bertindak sebelum menjadi
> insiden. Menurut tulisan PRD, kasus yang sama sudah dianggap tuntas pukul 02.11.

**Kenapa tidak menerima tanda terbaca WhatsApp.** Centang biru hanya membuktikan pesannya
**dibuka**, bukan bahwa dokter **memahami angkanya**. Pembacaan ulang dimaksudkan persis untuk
menangkap salah dengar *"tujuh koma dua"* menjadi *"satu koma dua"*.

**Akibat pada source:** nol hari ini — alur kritis `S5` belum dibangun, dan gerbang WhatsApp
nol di platform. Keputusan ini mengunci bentuknya sebelum keduanya dirancang.

**Yang dibuka keputusan ini — `PRD1-FOLLOW-02`.** Isi pesan WhatsApp kepada dokter menentukan
data pasien apa yang keluar ke layanan pihak ketiga. Ditanyakan sebagai Q4b.

### BR-90 — Pesan WhatsApp hasil kritis tanpa data klinis (`LAB-DEC-137`)

**Menutup `PRD1-FOLLOW-02`.**

**Aturan:**

1. Pesan WhatsApp hasil kritis kepada dokter **hanya** memuat: bahwa ada hasil kritis, unit
   atau ruang perawatan pasien, waktu, dan nomor kontak Laboratorium.
2. Pesan **tidak boleh** memuat nama pasien, No. RM, NIK, nama pemeriksaan, maupun nilai hasil.
3. Larangan butir 2 ditegakkan pada **pembentuk pesan**, bukan pada disiplin petugas: tidak
   ada ruas pengganti (*placeholder*) untuk data pasien maupun data klinis, sehingga mengubah
   teks pesan pun tidak dapat memasukkannya.
4. Dokter mendapatkan angkanya lewat telepon balik ke Laboratorium — tempat pembacaan ulang
   `LAB-DEC-004` terjadi dengan sendirinya — atau lewat pemberitahuan tersimpan di aplikasi
   (`LAB-DEC-012`).

**Contoh pesan yang sah (data samaran):**

> *Laboratorium RS: ada HASIL KRITIS untuk pasien Anda di Bangsal Melati, 02.11. Mohon segera
> hubungi Laboratorium ext. 1234.*

**Contoh yang ditolak:**

> *Tn. A.S., RM 00-12-34-56 — Kalium 7,2 mmol/L (KRITIS).* — memuat nama, No. RM, pemeriksaan,
> dan nilai.

**Kenapa ini yang dipilih.** Pesan WhatsApp melewati layanan pihak ketiga dan tersimpan tanpa
batas waktu di ponsel pribadi. Data kesehatan termasuk data pribadi yang dilindungi secara
khusus, dan blueprint ini belum pernah memberi izin privasi untuk jalur itu. Pesan tanpa data
klinis membuat jalur kritis **tidak menunggu** izin privasi siapa pun — yang masih ditunggu
hanya gerbangnya sendiri (`LAB-COORD-011`). Isinya juga sejalan dengan `LAB-DEC-136`: pesan
yang tidak memuat angka mendorong dokter menelepon balik, dan telepon itulah yang menghasilkan
bukti baca ulang.

**Akibat bagi `PRD1-CLIN-01`.** Karena pesan keluar tidak memuat nilai, balasan WhatsApp dokter
yang menyebut ulang angkanya berarti **dokter sendiri** yang memasukkan data klinis ke
WhatsApp. Pertimbangan ini ditambahkan pada butir tersebut untuk `DR-LAB-001`/`DR-LAB-002`.

**Yang tetap terbuka bagi amandemen kelak.** Pesan berisi data klinis bukan dilarang
selamanya. Bila pemilik platform kelak menyatakan izin privasi untuk jalur ini, keputusan ini
dapat diamandemen — lewat keputusan baru, bukan lewat mengubah teks pesan.

### BR-91 — Hasil tervalidasi yang belum dirilis dikembalikan kepada analis (`LAB-DEC-138`)

**Menutup `PRD1-FOLLOW-01`. Mengisi celah antara Reopen (`LAB-DEC-135`, sebelum validasi)
dan koreksi resmi (`LAB-DEC-007`, sesudah rilis).**

**Aturan:**

1. Pemegang kewenangan **validasi atau rilis** dapat menekan **Kembalikan ke analis** pada hasil
   yang **sudah divalidasi tetapi belum dirilis**.
2. Alasan **wajib** dipilih dari **daftar alasan terkendali yang sama** dengan koreksi hasil
   (`LAB-DEC-082`).
3. Validasinya dibatalkan dan hasil kembali menjadi **Draft**. Fakta bahwa hasil **pernah
   divalidasi** — oleh siapa, kapan — **tetap tercatat** di riwayat beserta alasan
   pengembaliannya; tidak ada yang terhapus.
4. Analis membetulkan, menekan Final lagi, lalu hasil divalidasi ulang oleh orang yang **bukan**
   pengisinya (`LAB-DEC-003`).
5. **Tidak** menghasilkan versi bernomor dan **tidak** memberi tahu dokter pemesan, karena hasil
   itu belum pernah keluar dari laboratorium.
6. Hasil yang **sudah dirilis** tidak dapat dikembalikan dengan cara ini. Ia hanya dapat
   dikoreksi lewat `LAB-DEC-007` dan `LAB-DEC-082`.

**Contoh:**

> Pukul 10.00 dr. Aditya memvalidasi Hemoglobin 9,4 g/dL. Pukul 10.05, sebelum dirilis, petugas
> perilis menyadari tabungnya tertukar. Ia menekan **Kembalikan ke analis** dengan alasan
> *"Sampel tertukar"*. Hemoglobin kembali Draft; riwayat mencatat *divalidasi dr. Aditya 10.00,
> dikembalikan 10.05 — Sampel tertukar*. Analis Sari memeriksa tabung yang benar, mengisi 4,9,
> dan menekan Final pukul 10.20; dr. Aditya memvalidasinya lagi dan hasil dirilis pukul 10.25.
> dr. Rina hanya pernah melihat 4,9 — tidak ada kabar *"hasil berubah"* untuk angka yang tidak
> pernah ia baca.

**Kenapa memakai daftar alasan yang sama dengan koreksi.** Laporan mutu kelak dapat menjawab
*"berapa kali sampel tertukar bulan ini"* dengan menjumlahkan yang tertangkap **sebelum** dan
**sesudah** rilis dari satu daftar. Dua daftar berbeda membuat kejadian yang sama terhitung
dengan dua nama.

**Kenapa tidak memberi tahu dokter.** Kabar *"hasil pasien Anda berubah"* untuk hasil yang belum
pernah dilihat melatih dokter mengabaikan kabar semacam itu — termasuk ketika koreksinya
sungguhan.

**Akibat pada source:** nol hari ini — validasi dan rilis (`S4`) belum dibangun. Keputusan ini
mengunci bentuknya sebelum dirancang.

### BR-92 — Satu gerbang untuk seluruh penyerahan hasil final kepada pasien (`LAB-DEC-139`)

**Memperluas cakupan syarat `LAB-DEC-067` dari *kirim* menjadi seluruh penyerahan. Menutup
`PRD1-CONF-13`.**

**Aturan:**

1. Tiga aksi berikut tunduk pada **satu gerbang yang sama**: **kirim WhatsApp**, **cetak
   dokumen final untuk pasien**, dan **unduh** dokumen final.
2. Gerbang terbuka hanya bila **kedua** syarat terpenuhi:
   - order berlabel **Selesai** — seluruh pemeriksaan yang tidak batal sudah dirilis
     (`LAB-DEC-135`); **dan**
   - persetujuan **Profesor dan Dokter Lab** sesuai `LAB-DEC-067` sudah ada.
3. Gerbang ditegakkan **backend** pada ketiga jalur. Tidak ada satu jalur pun yang dapat
   menyerahkan dokumen final pasien tanpa melewatinya.
4. Selama gerbang tertutup, layar **menyebut syarat mana yang belum terpenuhi**, bukan sekadar
   menonaktifkan tombol tanpa penjelasan — sejalan dengan `LAB-DEC-069` yang melarang petugas
   dibiarkan menebak.
5. **Tidak terpengaruh gerbang ini:** Nota Lab, Label Lab, dan Label Golongan Darah
   (`LAB-DEC-075`), serta lembar hasil untuk dokter yang boleh dirilis sebagian
   (`LAB-DEC-008`).
6. Berlaku untuk Patologi Klinik **dan** Mikrobiologi pada putaran ini.
7. **Siapa Profesor yang dimaksud dan bagaimana bentuk persetujuannya bukan bagian keputusan
   ini.** Itu tetap `LAB-OPEN-029`, sudah diajukan kepada pihak klinis lewat `LAB-DEC-121`.

**Contoh:**

> Hemoglobin pasien Andi dirilis pukul 10.30, sehingga ordernya **Selesai**. Persetujuan
> Profesor belum ada. Pukul 11.00 Andi datang ke loket. Petugas membuka order: tombol Cetak,
> Unduh, dan Kirim WhatsApp tidak aktif, disertai keterangan *"Menunggu persetujuan Profesor
> dan Dokter Lab"*. Petugas tetap dapat mencetak **Nota Lab** untuk keperluan pembayaran.
> Begitu persetujuan tercatat, ketiga tombol aktif bersamaan.

**Kenapa satu gerbang, bukan satu per kanal.** Kertas di loket dan PDF di WhatsApp adalah
dokumen yang sama. Gerbang yang hanya menjaga WhatsApp dapat dilewati lewat tombol Cetak lalu
difoto — dan syarat `LAB-DEC-067` berubah menjadi formalitas pada satu kanal saja.

**Akibat yang perlu disadari.** Sampai `LAB-OPEN-029` dijawab, dokumen final pasien tertahan
di ketiga jalur. Dalam praktik ini **tidak menambah penundaan** dari keadaan hari ini: order
baru dapat berlabel Selesai sesudah rilis berdiri, dan rilis sendiri masih tertahan
`DEC-LAB-011`. Yang berubah adalah **urutan penahannya** — bila `DEC-LAB-011` terjawab lebih
dulu, `LAB-OPEN-029` menjadi penahan berikutnya bagi `S17`.

### BR-93 — Kode QR memeriksa keaslian dokumen, bukan membuka hasil (`LAB-DEC-140`)

**Menutup `PRD1-NEW-01`. *QR Result Viewer* pada PRD menjadi *QR verifikasi dokumen*.**

**Aturan:**

1. Kode QR tercetak pada **dokumen final pasien** — kertas maupun PDF — Patologi Klinik dan
   Mikrobiologi, yaitu dokumen yang melewati gerbang `LAB-DEC-139`.
2. Memindai QR membuka **halaman verifikasi publik** yang hanya menampilkan: nama rumah sakit,
   **nomor cetak** (`LAB-DEC-117`), tanggal rilis, dan **status dokumen**.
3. Status dokumen salah satu dari tiga:
   - **Asli dan berlaku**;
   - **Sudah digantikan** — beserta nomor versi dan tanggal versi penggantinya
     (`LAB-DEC-007`, `LAB-DEC-082`);
   - **Tidak dikenal**.
4. Halaman itu **tidak** menampilkan nilai hasil, nama pemeriksaan, nama pasien, No. RM,
   maupun NIK.
5. QR berisi **token acak** yang **tidak dapat diturunkan** dari nomor order, nomor cetak, atau
   nomor apa pun yang tercetak — nomor urut dapat ditebak lalu dipindai satu per satu.
6. Token melekat pada **satu versi dokumen**. Koreksi menerbitkan versi baru dengan token baru;
   token lama tetap hidup dan berganti status menjadi *sudah digantikan*.
7. Halaman publik tanpa login adalah **jalur baru bagi backend** dan memerlukan persetujuan
   pemilik keamanan/platform sebelum dibangun — `LAB-COORD-015`. Ini menahan
   **implementasi** halaman itu saja, bukan desainnya.

**Contoh:**

> 1 September, Hemoglobin pasien Andi 9,4 g/dL dicetak dengan nomor cetak `25039254` dan
> diserahkan. Andi membawanya ke perusahaan asuransi. 4 September, hasil dikoreksi menjadi
> 4,9 dan terbit versi 2. 10 September, petugas asuransi memindai QR pada kertas lama dan
> membaca: *"RS — No. 25039254 — dirilis 1 September — **SUDAH DIGANTIKAN** oleh versi 2
> tertanggal 4 September"*. Petugas asuransi tahu kertas di tangannya usang tanpa pernah
> melihat satu pun angka hasil Andi.

**Batas yang harus dinyatakan terus terang.** QR ini membuktikan bahwa **dokumen bernomor itu
pernah diterbitkan** dan **apa statusnya**. Ia **tidak** membuktikan bahwa angka pada kertas
tidak diubah — halaman verifikasi sengaja tidak menampilkan angka. Pembuktian keutuhan isi
menuntut tanda tangan digital pada PDF, dan itu urusan pembangkit PDF platform
(`LAB-COORD-011`), bukan kode QR.

**Kenapa bukan membuka hasil.** Kertas hasil berpindah tangan — keluarga, kantor, asuransi.
Siapa pun yang memotretnya dapat memindai QR. QR yang membuka hasil lengkap berarti setiap
foto kertas adalah kebocoran seluruh hasil pasien.

**Kenapa bukan hanya untuk pegawai.** QR yang hanya bekerja di dalam rumah sakit tidak berguna
di tempat dokumen usang paling berbahaya — di luar rumah sakit, di tangan pihak yang tidak
tahu hasilnya pernah dikoreksi.

### BR-94 — Konsultasi Patologi Klinik adalah fakta opsional (`LAB-DEC-141`)

**Memperluas pola `LAB-DEC-106` dari Mikrobiologi ke Patologi Klinik. Menutup `PRD1-NEW-02`.**

**Aturan:**

1. Konsultasi pada hasil Patologi Klinik dicatat sebagai **fakta**: siapa yang mengonsultasikan,
   kepada siapa, dan kapan — `ConsultedByUserId`, `ConsultedToName`, `ConsultedAt`, tiga kolom
   yang sudah ada pada `LabExamination`.
2. Konsultasi **opsional**. Ia **bukan** syarat Final, **bukan** status, dan **bukan** izin apa
   pun.
3. Patologi Klinik **tidak** memperoleh kualifikasi `Definitif` (`LAB-DEC-114` tetap khas
   Mikrobiologi).
4. Mengikuti pola Mikrobiologi, fakta konsultasi dicatat selama hasil belum Final; sesudah
   Final ia ikut baca-saja, dan dibuka lagi lewat Reopen bila perlu.
5. *Kepada siapa* berupa **nama tertulis**, sehingga konsultan dari luar rumah sakit yang tidak
   terdaftar pada data induk dokter tetap dapat dicatat.

**Contoh:**

> Analis menemukan sel blas pada hapusan darah tepi pasien Andi. Pukul 11.20 ia menelepon
> konsultan Sp.PK(K), lalu mencatat: *dikonsultasikan oleh Sari, kepada konsultan Sp.PK(K),
> 11.20*. Pukul 11.35 ia menekan Final. Enam bulan kemudian, ketika hasil itu ditinjau ulang,
> pertanyaan *"dulu dikonsultasikan ke siapa?"* terjawab dari data, bukan dari ingatan.
>
> Hasil Hemoglobin rutin pasien lain yang tidak dikonsultasikan tetap dapat Final seperti
> biasa — tidak ada kolom yang wajib diisi.

**Kenapa tidak dijadikan syarat.** Menjadikannya syarat menuntut daftar *pemeriksaan mana yang
wajib dikonsultasikan*, dan isi daftar itu keputusan klinis `DR-LAB-001`. Bila kelak dibutuhkan,
ia dapat ditambahkan di atas fakta yang sama tanpa membongkar apa pun.

**Akibat pada source:** nol kolom baru. Tindakan pencatatan konsultasi untuk Patologi Klinik
belum ada; bentuknya diputuskan desain `S4a`.

### BR-95 — Kewenangan validasi dan rilis berlapis dua: jabatan dan penunjukan (`LAB-DEC-142`)

**Menegakkan `LAB-DEC-022` dan menetapkan cara menegakkannya. Menutup `PRD1-CONF-03`.**

**Aturan:**

1. **Lapis jabatan** — hak akses per departemen dan jabatan yang sudah dipakai aplikasi
   menentukan **siapa yang boleh menjadi calon** pemvalidasi atau perilis. Jabatan calon dapat
   diatur, dan boleh mencakup analis senior. **⚠ Diamandemen 2026-09-24 oleh `LAB-DEC-150`:**
   jabatan calon **dibatasi pada dokter berkewenangan laboratorium**; analis, termasuk analis
   senior, tidak lagi dapat menjadi calon.
2. **Lapis orang** — **daftar penunjukan per orang** menentukan **siapa yang benar-benar
   berwenang**, sesuai `LAB-DEC-022`.
3. Validasi dan rilis hanya dapat dilakukan bila **kedua lapis lolos**.
4. Penunjukan **validasi** dan penunjukan **rilis** adalah dua hal terpisah (`LAB-INH-007`).
   Memegang satu tidak memberi yang lain.
5. Lapis orang **ditegakkan di dalam service**, sama seperti ketiga pembatasan pada
   `permission-audit-matrix` bagian 3 — sistem hak akses bekerja per aksi, bukan per orang.
6. **Siapa yang berhak mengisi daftar penunjukan bukan bagian keputusan ini.** Itu tetap
   `DEC-LAB-011`, diajukan kepada dr. Bima Prasetya, Sp.PK lewat `LAB-REQ-013`.

**Contoh:**

> Senin, rumah sakit menerima dr. Baru, Sp.PK, berjabatan Dokter Penanggung Jawab
> Laboratorium. Pukul 08.00 ia membuka antrean validasi — lapis jabatan lolos — lalu menekan
> Validasi pada hasil Kalium. Sistem menolak: *"Anda belum ditunjuk sebagai pemegang
> kewenangan validasi."* Rabu, sesudah kompetensinya dinilai, namanya masuk daftar penunjukan,
> dan validasinya diterima.
>
> Analis senior Budi berjabatan Analis Laboratorium Senior, jabatan yang diatur sebagai calon,
> dan namanya ada pada daftar penunjukan validasi. Ia dapat memvalidasi hasil Sari. Analis
> senior lain dengan jabatan yang sama tetapi tidak ditunjuk tetap tidak dapat.
>
> **⚠ Contoh paragraf ini tidak berlaku sejak 2026-09-24** (`LAB-DEC-150`): analis, termasuk
> analis senior, tidak dapat menjadi pemvalidasi. Pola dua lapisnya tetap berlaku bagi dokter.

**Kenapa dua lapis.** Lapis jabatan saja memberi kewenangan kepada setiap pemegang jabatan pada
hari pertamanya, tanpa penilaian kompetensi. Lapis orang saja tidak menahan kesalahan
pengisian daftar — staf administrasi yang keliru dimasukkan langsung dapat mengesahkan hasil
pasien.

**Yang dibuka keputusan ini — `PRD1-FOLLOW-03`.** Apakah penunjukan berlaku **per disiplin**?
Ditanyakan sebagai Q9b.

### BR-96 — Penunjukan dicatat per disiplin (`LAB-DEC-143`)

**Melengkapi `LAB-DEC-142`. Menutup `PRD1-FOLLOW-03`.**

**Aturan:**

1. Setiap penunjukan menyebut **tiga hal**: orangnya, jenis kewenangannya (validasi atau rilis),
   dan **disiplinnya** — Patologi Klinik, Mikrobiologi, atau Patologi Anatomi.
2. Penunjukan pada satu disiplin **tidak** memberi kewenangan pada disiplin lain.
3. Orang yang kompeten di lebih dari satu disiplin memperoleh penunjukan **terpisah** untuk
   masing-masing.

**Contoh:**

> Analis senior Budi ditunjuk untuk **validasi Patologi Klinik**. Pukul 14.00 ia membuka hasil
> antibiogram *Escherichia coli* ESBL di antrean Mikrobiologi dan menekan Validasi. Sistem
> menolak: *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."*
> **⚠ Sejak 2026-09-24 (`LAB-DEC-150`) baca "Budi" sebagai dokter berkewenangan laboratorium;
> analis tidak lagi dapat ditunjuk. Aturan per disiplinnya tidak berubah.**
>
> dr. Aditya ditunjuk untuk validasi dan rilis Patologi Klinik — dua baris. Bila rumah sakit
> kelak menetapkan ia juga memegang Mikrobiologi, ditambahkan dua baris lagi. Tidak ada yang
> perlu dibongkar.

**Kenapa per disiplin.** Kewenangan klinis modul ini sudah dipegang per disiplin
(`LAB-DEC-078`), dan validasi serta rilis tiap disiplin berdiri sebagai slice tersendiri
(`LAB-DEC-083`). Yang paling menentukan: bentuk per disiplin **tetap dapat mewakili** kebijakan
*satu orang untuk semua disiplin* — cukup tiga penunjukan — sedangkan bentuk lintas disiplin
**tidak dapat** mewakili sebaliknya. Apa pun jawaban `DEC-LAB-011` kelak, data penunjukan tidak
perlu dipecah ulang.

### Penegasan ulang empat penolakan Mikrobiologi (`LAB-DEC-144`)

**Menutup `PRD1-CONF-07`..`PRD1-CONF-10` ke arah blueprint.** PRD mengajukan kembali empat hal
yang ditolak pada putaran 9 (2026-09-21), **tanpa satu pun alasan atau bukti baru**. Pemilik
modul menegaskan keempat penolakan itu berlaku apa adanya.

| Butir | Yang ditegaskan | Keputusan asal |
|---|---|---|
| `PRD1-CONF-07` | Hasil Mikrobiologi melekat **per pemeriksaan**; yang berjumlah satu per order adalah **dokumen cetaknya** | `LAB-DEC-095`, `LAB-DEC-067` |
| `PRD1-CONF-08` | `Lainnya` beserta keterangannya masuk **daftar pantau**; hanya kepala instalasi yang menaikkannya menjadi pilihan tetap | `LAB-DEC-040`, `LAB-DEC-098`, `LAB-DEC-130` |
| `PRD1-CONF-09` | Nama analis **diturunkan** dari pengguna yang menyimpan dan tampil baca-saja; `Penanggung Jawab Analis` tetap ruas tersendiri yang dipilih | `LAB-DEC-105`, `LAB-DEC-093` |
| `PRD1-CONF-10` | **Subbakteri tidak dibangun**; varian seperti *E. coli ESBL* adalah baris organisme tersendiri | `LAB-DEC-102` |

**Kenapa dicatat sebagai keputusan, bukan dibiarkan sebagai bawaan `LAB-DEC-133`.** Ini kali
**kedua** keempat usulan yang sama datang lewat dokumen yang berbeda. Penegasan tertulis
membuat kali ketiga dapat dijawab dengan menunjuk satu baris, bukan mengulang wawancara.

**Akibat pada source:** nol. Keempatnya sudah dibangun sesuai keputusan asal pada `S4b`.

### Paket empat butir kecil (`LAB-DEC-145`)

**Menutup `PRD1-NEW-03`..`PRD1-NEW-06`.** Keempatnya diajukan sebagai satu paket karena
berisiko rendah dan saling lepas; setiap butir tetap disertai pilihan dan rekomendasinya
sendiri, dan pemilik modul menyetujui keempat rekomendasi.

| No | Butir | Keputusan | Contoh atau alasan |
|---:|---|---|---|
| 1 | `PRD1-NEW-03` Email pasien | **Dihapus dari PRD.** Nol FR memakainya; platform nol sarana surel; surel adalah kanal data kesehatan baru yang butuh izin tersendiri. Bila kelak dibutuhkan, dibuka sebagai butir baru dan tetap tunduk `LAB-DEC-139` | — |
| 2 | `PRD1-NEW-04` Warna flag | **Makna dikunci, kode warna tidak** — dicatat `LAB-FE-015`. Penanda wajib berupa huruf atau teks | Cetakan hitam-putih Kalium 5,3 tetap terbaca `H 5,3`, dan Kalium 7,2 terbaca `KRITIS 7,2` — tanpa huruf, keduanya abu-abu yang sama |
| 3 | `PRD1-NEW-05` Larangan modal | **Diterima untuk isian hasil utama** pada halaman hasil PK dan Mikrobiologi — dicatat `LAB-FE-016` | Antibiogram dapat sampai 22 baris; modal yang tertutup tak sengaja pada baris ke-20 menghapus seluruh isian |
| 4 | `PRD1-NEW-06` Lokasi dan metode pengambilan specimen | **Tetap `S2b`**, masih `BUSINESS_DECISION_REQUIRED`. PRD menandainya *menunggu keputusan `S2b`* | Daftar nilainya belum ada dari lapangan; memutuskan bentuk tanpa isi mengulang pola *daftarnya ada, cara mengisinya tidak* |

### Koreksi yang wajib masuk revisi PRD

Diperbarui setiap kali satu butir putaran ini diputuskan. Penulis PRD memakai daftar ini untuk
merevisi dokumennya.

| No | Bagian PRD | Tertulis sekarang | Seharusnya | Dasar |
|---:|---|---|---|---|
| 1 | 1 (Target User), 5 | Aktor Analis hanya disebut untuk Mikrobiologi | Tambahkan **Analis Laboratorium** sebagai pengisi hasil **Patologi Klinik** | `LAB-DEC-134` |
| 2 | 5.2 | Dokter Laboratorium: *Mengisi hasil pemeriksaan*, *Mengubah hasil sebelum final* | Dokter Laboratorium: **memvalidasi dan mengotorisasi** hasil Patologi Klinik | `LAB-DEC-134` |
| 3 | FR-PK-002 | *Dokter Laboratorium dapat: Mengisi hasil, Mengubah hasil, Menyimpan Draft* | **Analis** yang mengisi, mengubah, Simpan Draft, dan Simpan Final hasil — per pemeriksaan; Dokter Lab mengesahkannya | `LAB-DEC-134`, `LAB-DEC-135` |
| 4 | BP-001 langkah 4 | *Dokter Lab mengisi hasil* | **Analis** mengisi hasil; langkah validasi dan otorisasi oleh Dokter Lab ditambahkan sesudahnya | `LAB-DEC-134` |
| 5 | 13 (AC Patologi Klinik) | *Dokter Lab input hasil → Berhasil simpan draft* | **Analis** input hasil. Tambahkan skenario: *pengisi hasil menekan Validasi → ditolak kecuali alasan pengecualian diisi* | `LAB-DEC-134`, `AC-01` |
| 6 | 5.1 | *Petugas Laboratorium tidak dapat mengubah hasil Patologi Klinik* | **Tetap**, dengan penegasan bahwa Petugas Laboratorium adalah petugas administrasi, bukan analis | `LAB-DEC-134` butir 3 |
| 7 | 2 (Latar Belakang) | *... berubah menjadi Selesai setelah dilakukan Simpan Final oleh Dokter Laboratorium* | Order berlabel **Selesai** setelah **seluruh pemeriksaan yang tidak batal dirilis**. Simpan Final oleh analis berarti selesai menulis | `LAB-DEC-135` |
| 8 | FR-PK-002 (diagram) | `Dalam Pemeriksaan → Simpan Final → Selesai` | Per pemeriksaan: `Draft → Final (analis) → Validasi → Rilis`, dengan Reopen dari Final ke Draft selama belum divalidasi. Per order: `Dalam Pemeriksaan → Selesai` sebagai label turunan | `LAB-DEC-135` |
| 9 | 8 (Status Management) | Lima status sejajar: Draft, Dalam Pemeriksaan, Definitif, Final/Selesai, Amendment | Ganti dengan tabel label turunan BR-88: Draft dan Final pada pemeriksaan; Dalam Pemeriksaan dan Selesai pada order; Amendment sesudah rilis; **Definitif dihapus untuk Patologi Klinik** | `LAB-DEC-135`, `LAB-DEC-080` |
| 10 | 12 (Security) | *Data final tidak dapat diedit* | Data yang sudah **dirilis** tidak dapat diedit; hasil Final yang belum divalidasi dapat dibuka kembali | `LAB-DEC-135` butir 3 dan 4 |
| 11 | 13 (AC Patologi Klinik) | *Dokter Lab finalisasi → Status menjadi Selesai* | *Analis Simpan Final → hasil masuk antrean validasi*; *seluruh pemeriksaan order dirilis → order berlabel Selesai* | `LAB-DEC-135`, `AC-196`, `AC-199` |
| 12 | 13 (AC Patologi Klinik) | *User edit hasil final → Ditolak* | *Edit hasil yang sudah dirilis → ditolak*; *Reopen hasil Final yang belum divalidasi → berhasil dan tercatat* | `LAB-DEC-135`, `AC-197` |
| 13 | BP-003 (diagram) | `... → Konfirmasi Dokter → WhatsApp` sebagai akhir alur | Tambahkan langkah terakhir: **petugas mencatat pelaporan** — siapa, kepada siapa, kapan, lewat apa, bukti baca ulang — **baru tuntas**. WhatsApp menjadi langkah pengantar yang boleh dilewati | `LAB-DEC-136` |
| 14 | BP-003 | *Sistem mencatat waktu pengiriman* | Sistem mencatat **waktu kirim WhatsApp** dan **waktu dilaporkan** secara terpisah; hasil tetap di daftar pantau sampai bukti baca ulang tercatat | `LAB-DEC-136` |
| 15 | 10 (WhatsApp Gateway) | *Digunakan untuk: Notifikasi hasil kritis* | *Pengantar kabar hasil kritis — bukan bukti pelaporan*; tambahkan catatan ketergantungan pada gerbang WhatsApp platform (`LAB-COORD-011`) | `LAB-DEC-136` |
| 16 | 13 (AC Patologi Klinik) | Nol skenario hasil kritis | Tambahkan: *WhatsApp kritis terkirim, baca ulang belum dicatat → hasil tetap di daftar pantau* | `LAB-DEC-136`, `AC-200` |
| 17 | BP-003, 10 | Isi pesan WhatsApp kritis tidak diatur | Tambahkan ketentuan: pesan kritis **hanya** memuat adanya hasil kritis, unit/ruang, waktu, dan nomor kontak Laboratorium — **tanpa** nama pasien, No. RM, NIK, nama pemeriksaan, maupun nilai | `LAB-DEC-137` |
| 18 | FR-PK-004 | *Jika hasil sudah Final: tidak boleh edit langsung, harus melalui Amendment* | *Jika hasil sudah **dirilis***: hanya lewat koreksi. Sebelum rilis: **Reopen** oleh analis bila belum divalidasi; **Kembalikan ke analis** oleh pemegang kewenangan validasi/rilis bila sudah divalidasi | `LAB-DEC-135`, `LAB-DEC-138` |
| 19 | FR-PK-004 | *Wajib menyimpan alasan perubahan* | Alasan **dipilih dari daftar baku**; setiap koreksi menjadi **versi bernomor**, dan versi lama tetap terlihat bertanda *sudah diperbaiki* | `LAB-DEC-082`, `LAB-DEC-007` |
| 20 | FR-PK-004 | Nol ketentuan pemberitahuan dan kunjungan tertutup | Tambahkan: **dokter pemesan otomatis diberi tahu** setiap koreksi dirilis ulang; bila kunjungan sudah ditutup, koreksi didaftarkan sebagai **addendum** pada dokumen rekam medis | `LAB-DEC-007`, `LAB-DEC-020` |
| 21 | 5.2 | *Melakukan Amendment*, *Finalisasi hasil koreksi* pada peran yang sama | Hasil koreksi **divalidasi ulang oleh orang yang berbeda** dari pengoreksinya | `LAB-DEC-003` |
| 22 | FR-PK-002 (diagram) | Nol jalur balik sesudah validasi | Tambahkan jalur `Validasi → Kembalikan ke analis (beralasan) → Draft`, berlaku hanya sebelum rilis | `LAB-DEC-138` |
| 23 | FR-PK-005 (Prasyarat) | *Status hasil: Selesai* | Order **Selesai** (seluruh pemeriksaan yang tidak batal dirilis) **dan** persetujuan **Profesor dan Dokter Lab**. Berlaku sama untuk Print, Download, dan Kirim WhatsApp | `LAB-DEC-139`, `LAB-DEC-067` |
| 24 | 4.2, 6.2 | *Print hasil mikrobiologi* tanpa prasyarat | Tunduk pada gerbang yang sama dengan FR-PK-005 | `LAB-DEC-139` butir 6 |
| 25 | 13 (AC Patologi Klinik) | *Kirim hasil pasien → PDF terkirim* | Tambahkan: *order Selesai tanpa persetujuan → Cetak, Unduh, dan Kirim tidak aktif dan layar menyebut syarat yang kurang*; *Nota Lab tetap dapat dicetak* | `LAB-DEC-139`, `AC-208`, `AC-209` |
| 26 | 4.1 | *QR Result Viewer* | **QR verifikasi dokumen**: memeriksa keaslian dan status dokumen — asli, sudah digantikan, atau tidak dikenal — **bukan** membuka hasil | `LAB-DEC-140` |
| 27 | 9 (Header) | *QR Code* tanpa keterangan | QR melekat pada **dokumen final per versi**, berisi token acak menuju halaman verifikasi; tidak memuat data pasien | `LAB-DEC-140` butir 1, 5, 6 |
| 28 | 10 (Integration) | Nol halaman publik | Tambahkan halaman verifikasi publik tanpa login beserta ketergantungannya pada persetujuan pemilik keamanan/platform (`LAB-COORD-015`) | `LAB-DEC-140` butir 7 |
| 29 | 13 (AC Patologi Klinik) | Nol skenario QR | Tambahkan: *pindai QR dokumen yang sudah dikoreksi → tampil "sudah digantikan" beserta versi penggantinya, tanpa angka hasil* | `LAB-DEC-140`, `AC-212` |
| 30 | BP-001 langkah 7 | *Konsultasi eksternal jika diperlukan* | *Bila dikonsultasikan, catat siapa, kepada siapa, dan kapan.* Opsional, **bukan** syarat Simpan Final, dan **tidak** menghasilkan status Definitif pada Patologi Klinik | `LAB-DEC-141` |
| 31 | 5.2 | *Dokter Laboratorium merupakan satu-satunya role yang dapat melakukan Simpan Final* | Validasi dan rilis hanya oleh orang yang **berjabatan calon** dan **ditunjuk per orang**. Tidak setiap Dokter Lab otomatis berwenang. ~~Analis senior yang ditunjuk boleh~~ — **dicabut 2026-09-24**: validasi hanya oleh dokter berkewenangan laboratorium | `LAB-DEC-142`, `LAB-DEC-022`, `LAB-DEC-150` |
| 32 | 12 (Security) | *Role based access* | Hak akses per **departemen dan jabatan** untuk setiap aksi, **ditambah** penunjukan per orang untuk validasi dan rilis | `LAB-DEC-142` |
| 33 | BP-002 langkah 7; 13 (AC Mikrobiologi) | *Dokter Lab melakukan Final*; *Finalisasi → Hanya Dokter Lab* | **Simpan Final** oleh penulis hasil Mikrobiologi — berarti selesai menulis. **Validasi dan rilis** Mikrobiologi oleh pemegang penunjukan, pada `S4d` | `LAB-DEC-097`, `LAB-DEC-142` |
| 34 | 5.2, 12 | Kewenangan tidak dibedakan per disiplin | Penunjukan validasi dan rilis dicatat **per disiplin**; penunjukan Patologi Klinik tidak berlaku untuk Mikrobiologi | `LAB-DEC-143` |
| 35 | 13 (AC Mikrobiologi) | *Critical ditemukan → Notifikasi dokter aktif* | *Critical ditemukan **menurut aturan kritis Mikrobiologi yang diisi `DR-LAB-002`** → formulir pelaporan muncul*; *selama aturan itu kosong, layar menyatakan penanda kritis Mikrobiologi belum aktif* | `LAB-DEC-103`, `LAB-OPEN-041` |
| 36 | 2 (Latar Belakang), 6.2 | *Satu order ... menghasilkan satu hasil pemeriksaan mikrobiologi pada level order* | Setiap **pemeriksaan** Mikrobiologi punya hasilnya sendiri; yang berjumlah satu per order adalah **dokumen cetaknya** | `LAB-DEC-144`, `LAB-DEC-095` |
| 37 | FR-MB-003 | *Pilih Lainnya → input → validasi duplikasi → simpan → tampil sebagai checkbox → bisa digunakan kembali* | *Pilih Lainnya → isi keterangan → tersimpan pada specimen itu dan **masuk daftar pantau** → **kepala instalasi** yang menaikkannya menjadi pilihan tetap.* Pilihan tetap berasal dari data induk yang sudah memuat 1.767 entri SNOMED | `LAB-DEC-144`, `LAB-DEC-098` |
| 38 | FR-MB-004 | Ruas *Analis* di antara ruas isian | *Analis* **tampil baca-saja** dari pengguna yang menyimpan; tambahkan ruas *Penanggung Jawab Analis* yang dipilih | `LAB-DEC-144`, `LAB-DEC-105` |
| 39 | FR-MB-005, 5.3 | *Subbakteri*; Analis *mengelola organisme* | Hapus *Subbakteri*. *Mengelola organisme* diperjelas: analis **memilih** organisme pada hasil; **data induk** organisme dikelola lewat layar data induk | `LAB-DEC-144`, `LAB-DEC-102`, `LAB-DEC-084` |
| 40 | 10 (Master Pasien) | *Digunakan untuk: ... Email* | **Hapus** *Email* | `LAB-DEC-145` butir 1 |
| 41 | FR-PK-003 | Tabel *Low/High → Kuning, Critical → Merah* | Tiga tingkat dibedakan jelas, kritis paling menonjol, dan penanda **wajib berupa huruf atau teks** `L`, `H`, `KRITIS` di layar dan di cetakan. Warna mengikuti token desain | `LAB-DEC-145` butir 2, `LAB-FE-015` |
| 42 | 12 (Usability) | *Form tidak menggunakan modal untuk input utama* | **Tetap**, diperjelas: berlaku untuk isian hasil utama — nilai, isolat, antibiogram — pada halaman hasil PK dan Mikrobiologi; modal boleh untuk konfirmasi dan isian pendek | `LAB-DEC-145` butir 3, `LAB-FE-016` |
| 43 | FR-MB-002 | *Lokasi specimen*, *Metode pengambilan* sebagai ruas yang tersedia | Tandai **menunggu keputusan `S2b`** | `LAB-DEC-145` butir 4 |
| 44 | 4.2 | *Preview bilingual* sebagai cakupan | Tandai **bergantung `LAB-COORD-013`** — layanan terjemahan dan izin privasinya milik platform. Cetak Bahasa Indonesia tidak tertahan | Batas scope putaran 14 |
| 45 | 4.2 | *Diagnostic Report* sebagai cakupan | Perjelas maksudnya: bila **HL7/SATUSEHAT `DiagnosticReport`**, tidak dibangun sekarang (`LAB-DEC-109`, `LAB-COORD-012`) — PRD sendiri menaruhnya di *Future Enhancement*; bila **lembar hasil cetak**, sudah tercakup `LAB-DEC-110` | Batas scope putaran 14 |

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q1 | Kedudukan PRD | Baseline baru, bukti untuk direkonsiliasi, atau dokumen komunikasi saja? | ✅ Dijawab — `LAB-DEC-133` |
| Q2 | `PRD1-CONF-01`, `PRD1-CONF-02` | Siapa yang mengisi hasil Patologi Klinik | ✅ Dijawab — `LAB-DEC-134` |
| Q3 | `PRD1-CONF-04`..`PRD1-CONF-06` | Perlukah Patologi Klinik punya Draft/Final, dan bagaimana status PRD dipetakan | ✅ Dijawab — `LAB-DEC-135` |
| Q4 | `PRD1-CONF-11` | Sahkah WhatsApp sebagai bukti pelaporan nilai kritis | ✅ Dijawab — `LAB-DEC-136` |
| Q4b | `PRD1-FOLLOW-02` | Data pasien apa yang boleh dimuat pesan WhatsApp hasil kritis kepada dokter | ✅ Dijawab — `LAB-DEC-137` |
| Q5 | `PRD1-CONF-12`, `PRD1-FOLLOW-01` | Rincian amendment, termasuk pengembalian hasil yang sudah divalidasi tetapi belum dirilis | ✅ Dijawab — `LAB-DEC-138`; `PRD1-CONF-12` tetap blueprint |
| Q6 | `PRD1-CONF-13` | Apakah persetujuan `LAB-DEC-067` juga menjadi syarat cetak dan unduh hasil untuk pasien, bukan hanya kirim | ✅ Dijawab — `LAB-DEC-139` |
| Q7 | `PRD1-NEW-01` | QR Result Viewer: apa yang dibuka kode QR, dan bagi siapa | ✅ Dijawab — `LAB-DEC-140`; `LAB-COORD-015` dibuka |
| Q8 | `PRD1-NEW-02` | Konsultasi pada Patologi Klinik | ✅ Dijawab — `LAB-DEC-141` |
| Q9 | `PRD1-CONF-03` | Apakah setiap Dokter Lab otomatis berwenang memvalidasi dan merilis, atau hanya yang ditunjuk | ✅ Dijawab — `LAB-DEC-142` |
| Q9b | `PRD1-FOLLOW-03` | Apakah penunjukan validasi dan rilis berlaku per disiplin | ✅ Dijawab — `LAB-DEC-143` |
| — | `PRD1-CONF-14` | AC Mikrobiologi nilai kritis | ✅ Ditutup tanpa pertanyaan — bukan wewenang pemilik modul; koreksi PRD 35 |
| Q10 | `PRD1-CONF-07`..`PRD1-CONF-10` | Konfirmasi ulang empat butir Mikrobiologi yang pernah ditolak | ✅ Dijawab — `LAB-DEC-144`; **keempat belas pertentangan kini tertutup** |
| Q11 | `PRD1-NEW-03`..`PRD1-NEW-06` | Email, warna, modal, lokasi dan metode specimen | ✅ Dijawab — `LAB-DEC-145`, `LAB-FE-015`, `LAB-FE-016`. **Putaran 14 selesai** |

---

## Closure Pass Putaran 15 — Closure question capability map revision 5 (dibuka 2026-09-24)

**Sumber:** empat closure question `LAB-CLOSE-013`..`LAB-CLOSE-016` yang dibuka
[`01-existing-capability-map.md`](01-existing-capability-map.md) **revision 5** pada hari yang
sama, atas BE `ddeb5ed8` + FE `72607a087`. SHA kedua repository **tidak bergeser** sejak scan
itu, sehingga peta dipakai apa adanya.

### Batas scope putaran ini (dikonfirmasi pemilik modul 2026-09-24)

Pemilik modul menjawab pertanyaan pertama tanpa mengoreksi daftar berikut.

**Di dalam scope:** `LAB-CLOSE-013` penegakan `LAB-DEC-134`; `LAB-CLOSE-014` penolakan
penyimpanan ulang hasil Final; `LAB-CLOSE-015` wadah penunjukan per orang; `LAB-CLOSE-016`
letak isian hasil Patologi Klinik.

**Di luar scope:**

| Hal | Pemiliknya |
|---|---|
| Aturan internal kredensial Human Resource, misalnya siapa yang menyetujui grant | Pemilik `human-resource` — hanya titik sentuhnya yang dibahas |
| Jawaban `DEC-LAB-011` | dr. Bima Prasetya, Sp.PK |
| Isi data kebijakan hak akses di basis data (`UNK-P14-01`) | Admin sistem |
| Header kontrak yang tertinggal | Pembukuan, bukan keputusan bisnis |
| Tata letak halaman selain letak isian hasil | `DEV_DISCRETION` |

### BR-97 — Tindakan atas hasil memakai izin tersendiri (`LAB-DEC-146`)

**Menutup `LAB-CLOSE-013` dan `LAB-CONFLICT-012`. Menegakkan `LAB-DEC-134`.**

**Aturan:**

1. Tindakan **atas hasil** memakai **resource hak akses tersendiri**, terpisah dari
   `LabExamination`. Nama pastinya ditetapkan pada amandemen `LAB-PERM-v1`; contoh kerjanya
   `LabExaminationResult : Update`.
2. Yang pindah ke izin hasil — lima tindakan yang sudah berdiri:

   | Tindakan | Disiplin |
   |---|---|
   | Mengisi hasil | Patologi Klinik |
   | Mengisi hasil | Mikrobiologi |
   | Simpan Final | Keduanya |
   | Reopen | Keduanya |
   | Mencatat konsultasi | Keduanya |

   Tindakan hasil yang kelak dibangun untuk Patologi Klinik (`LAB-DEC-135`, `LAB-DEC-141`)
   ikut memakai izin yang sama.
3. **Tetap** pada `LabExamination : Update`: membatalkan pemeriksaan, menandai cito, dan
   menandai duplo.
4. **Tidak disentuh:** tindakan laporan Patologi Anatomi tetap memakai izinnya sekarang,
   sehingga `LAB-DEC-090` — Dokter Lab mengisi hasil Patologi Anatomi — utuh.
5. **Tidak termasuk:** validasi dan rilis. Keduanya berlapis dua menurut `LAB-DEC-142` dan
   memperoleh aksinya sendiri pada `S4`.
6. Izin hasil diberikan kepada **jabatan analis**. Dokter pemesan dan Petugas Lab administrasi
   **tidak** memperolehnya.
7. **Nol hari terkunci:** data kebijakan izin hasil bagi jabatan analis **wajib terpasang
   dalam rilis yang sama** dengan perubahan hak aksesnya.

**Contoh:**

> Sesudah perubahan ini, dr. Rina tetap memegang `LabExamination : Update` dan tetap dapat
> menandai Kalium pasiennya sebagai cito. Bila akunnya mengirim `PUT .../result`, sistem
> menjawab `403` — ia tidak memegang izin hasil.
>
> Analis Sari memegang izin hasil. Senin pagi sesudah rilis ia membuka Daftar Kerja dan
> mengisi hasil seperti hari Jumat, tanpa satu pun perubahan yang ia rasakan — karena data
> kebijakan jabatannya ikut terpasang bersama rilis itu.

**Kenapa bukan pemeriksaan jabatan di dalam kode.** Nama jabatan tertanam di kode tidak dapat
diatur admin, tidak terlihat pada layar pengelolaan hak akses, dan setiap jabatan baru —
misalnya *Analis Senior* — menuntut rilis kode.

**Kenapa bukan memindahkan cito saja.** Celah dokter pemesan memang tertutup, tetapi setiap
pemegang izin batal atau duplo — termasuk Petugas Lab — tetap dapat mengisi hasil, dan butir 3
`LAB-DEC-134` tetap tidak tertegakkan.

**Akibat pada source dan kontrak:**

| Yang terdampak | Akibatnya |
|---|---|
| `LAB-PERM-v1` | Amandemen: satu resource baru; bagian 10.1 dan baris 71-76 diperbarui |
| Lima endpoint yang sudah berdiri (`S4a`, `S4b`) | Perbaikan atribut hak akses — **task perbaikan**, bukan fitur baru |
| Data kebijakan | Satu langkah pemasangan izin hasil bagi jabatan analis, dirilis bersama |
| `UNK-P14-01` | Tidak lagi menentukan keamanan, tetapi tetap berguna untuk memetakan jabatan mana yang kini memegang `LabExamination : Update` sebelum pemasangan |

### BR-98 — Hasil yang sudah Final ditolak bila disimpan ulang (`LAB-DEC-147`)

**Menutup `LAB-CLOSE-014`. Mengukuhkan yang sudah tersirat pada `LAB-DEC-097`, BR-60 butir 3,
dan `LAB-DEC-135` butir 3, dengan pola `VAL-96` milik Patologi Anatomi.**

**Aturan:**

1. Selama hasil berstatus **Final**, seluruh penyimpanan atas **isi hasil** ditolak dengan kode
   `409` dan pesan yang menyuruh membuka kembali lebih dulu. Yang dijaga:

   | Isi | Disiplin |
   |---|---|
   | Nilai hasil | Patologi Klinik |
   | Status temuan, isolat, baris antibiogram, kualifikasi, jenis biakan, metode uji | Mikrobiologi |
   | Catatan konsultasi | Keduanya |

2. **Reopen** — beralasan, menaikkan `ReopenCount`, tercatat di riwayat — adalah **satu-satunya
   jalan** mengubah hasil yang sudah Final sebelum validasi.
3. Penjaga koreksi specimen yang sudah ada (`VAL-109`) **tidak berubah**.
4. Aturannya masuk `LAB-VAL-v1` sebagai amandemen; nomor `VAL`-nya ditetapkan saat amandemen.
5. **Mikrobiologi diperbaiki sekarang**, sebelum `S4b` masuk rilis — backend menolak, frontend
   menampilkan penolakannya secara terbaca tanpa menghilangkan isian yang sedang diketik.
   Patologi Klinik memperoleh penjaga yang sama saat `LAB-DEC-135` dibangun.

**Contoh:**

> Analis menekan Final pada hasil kultur urin Senin pukul 14.40. Selasa pukul 09.00 ia ingin
> mengganti *Escherichia coli* menjadi *Klebsiella pneumoniae*. Tombol Simpan menjawab: *"Hasil
> ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* Ia menekan Reopen
> dengan alasan *"Identifikasi ulang dari biakan hari kedua"*, mengganti organismenya, lalu
> menekan Final lagi pukul 09.20.
>
> Hasilnya: `ReopenCount` = 1, riwayat memuat alasannya, dan cetakan menulis **Tanggal Selesai:
> Selasa 09.20** — waktu isi yang benar-benar tercetak. Tanpa aturan ini, cetakan tetap menulis
> Senin 14.40 untuk isi yang ditulis Selasa, dan nol jejak bahwa hasil pernah dibuka.

**Kenapa diperbaiki sekarang, bukan menunggu `S4d`.** Waktu Final dipakai dua kali — dicetak
sebagai **Tanggal Selesai** (`LAB-DEC-118`) dan tampil sebagai **Waktu Issued**
(`LAB-DEC-096`) — sehingga Final yang dapat ditimpa langsung membuat dokumen menyebut waktu
yang salah. Dan `S4b` belum masuk Rilis 1 menurut manifest, sehingga perbaikan ini belum
menyentuh kebiasaan petugas mana pun.

**Akibat pada source dan kontrak:**

| Yang terdampak | Akibatnya |
|---|---|
| `LAB-VAL-v1` | Amandemen: satu aturan baru setara `VAL-96` |
| `S4b` backend | **Task perbaikan** — penjaga Final pada penyimpanan hasil Mikrobiologi dan pencatatan konsultasi |
| `S4b` frontend | **Task perbaikan** — penanganan `409` yang terbaca pada halaman hasil Mikrobiologi |
| Perluasan `S4a` | Penjaga yang sama menjadi bagian desainnya sejak awal |

### BR-99 — Penunjukan disimpan pada kredensial Human Resource; Laboratorium hanya membaca (`LAB-DEC-148`)

**Menutup `LAB-CLOSE-015`. Menetapkan wadah lapis orang `LAB-DEC-142` dan pembagian per disiplin
`LAB-DEC-143`.**

**Aturan:**

1. Penunjukan pemvalidasi dan perilis disimpan sebagai **kewenangan klinis per tenaga kerja**
   pada modul Human Resource (`WfpClinicalPrivilege`), bukan pada tabel milik Laboratorium.
2. Katalog kewenangan Human Resource (`MstClinicalPrivilegeCatalog`) memperoleh **enam kode**
   Laboratorium:

   | Disiplin | Validasi | Rilis |
   |---|---|---|
   | Patologi Klinik | ✔ | ✔ |
   | Mikrobiologi | ✔ | ✔ |
   | Patologi Anatomi | ✔ | ✔ |

   Nama dan format kodenya disepakati bersama pemilik Human Resource (`LAB-COORD-016`).
3. Laboratorium **hanya membaca**. Seseorang dianggap ditunjuk bila memegang kode yang sesuai
   dengan status **aktif** dan tanggal hari itu berada **dalam masa berlakunya** — pola
   `OperatingRoomCredentialResolver` milik Kamar Operasi, ditambah pencocokan **kode**.
4. Laboratorium **tidak menulis** ke tabel kredensial Human Resource dalam bentuk apa pun.
5. Status *suspended*, *revoked*, kedaluwarsa, atau di luar masa berlaku **seketika** menolak
   validasi dan rilis, tanpa satu pun perubahan pada data Laboratorium.
6. Pesan penolakan **menyebut sebabnya**: belum ditunjuk, masa berlaku habis, atau sedang
   ditangguhkan.
7. Lapis jabatan `LAB-DEC-142` butir 1 **tetap berlaku** di atasnya.
8. **Siapa yang berwenang menetapkan** tetap `DEC-LAB-011`. Keputusan ini hanya menyediakan
   wadah dan usulan untuk dr. Bima Prasetya, Sp.PK.

**Contoh:**

> Proses kredensial memberi dr. Baru, Sp.PK, kewenangan *Validasi Patologi Klinik* yang
> berlaku 1 Oktober 2026 sampai 30 September 2029. Sejak 1 Oktober ia dapat memvalidasi hasil
> Patologi Klinik — dan tetap **tidak** dapat memvalidasi Mikrobiologi maupun merilis apa pun.
>
> 12 Maret 2027 kewenangannya di-*suspend* karena sedang diinvestigasi. Pukul 12.01 ia menekan
> Validasi dan membaca: *"Kewenangan validasi Patologi Klinik Anda sedang ditangguhkan."*
> Kepala instalasi **tidak** perlu mengingat untuk menghapusnya dari daftar mana pun di
> Laboratorium.

**Kenapa bukan daftar milik Laboratorium.** Dua sumber penunjukan pasti suatu hari berbeda:
tenaga yang di-*suspend* di Human Resource tetap dapat memvalidasi sampai seseorang ingat
menghapusnya dari daftar Laboratorium. Masa berlaku dan status juga harus dibangun ulang,
padahal keduanya sudah berdiri.

**Kenapa ini tidak mengulang `LAB-COORD-006` dan `MST-POS-WRITE`.** Keduanya macet karena
**daftarnya ada, cara mengisinya tidak**. Di sini **keduanya ada**: katalog punya `POST`,
`PUT`, `PATCH status`, dan `DELETE`
(`Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Controllers/ClinicalPrivilegeCatalogController.cs@ddeb5ed8`),
dan kewenangan punya `grant`, `reject`, `suspend`, serta `revoke`.

**Akibat yang perlu disadari.** Validasi baru dapat berjalan setelah proses kredensial
**sungguh mengisi** kewenangannya. Sampai itu terjadi, sistem menolak validasi — dan itu
disengaja: jalan keluar yang melewati kredensial akan mengulang masalah `LAB-DEC-022` butir 3,
yaitu pengecualian yang diam-diam menjadi jalur utama.

### BR-100 — Hasil Patologi Klinik diisi pada halaman detail per order (`LAB-DEC-149`)

**Menutup `LAB-CLOSE-016` dan `LAB-CONFLICT-013`. Melaksanakan `LAB-FE-016` dan PRD
`FR-PK-001`.**

**Aturan:**

1. Hasil Patologi Klinik diisi pada **halaman detail per order**, satu halaman untuk satu order —
   sejajar halaman Mikrobiologi yang juga satu halaman per order.
2. Halaman itu menampilkan **seluruh pemeriksaan Patologi Klinik yang tidak batal** dalam **satu
   tabel isian**: parameter, hasil, satuan, nilai rujukan, dan penanda `L`/`H` (`LAB-FE-015`).
   Bedanya dari Mikrobiologi disengaja: pemeriksaan Patologi Klinik bernilai tunggal, sehingga
   tidak perlu dipilih satu per satu.
3. **Final tetap per pemeriksaan** (`LAB-DEC-135`). Menyimpan atau Final pada satu baris
   **tidak** mengunci dan **tidak** menghapus isian baris lain.
4. **Daftar Kerja tetap** sebagai antrean kerja dan **membuka** halaman ini. Dialog modal
   pengisian hasil pada Daftar Kerja **dicabut**.
5. Nama route mengikuti konvensi yang sudah ada (`LAB-FE-001`). Tata letak di dalam halaman
   tetap `DEV_DISCRETION` dalam batas `LAB-FE-015` dan `LAB-FE-016` — dicatat sebagai
   `LAB-FE-017`.

**Contoh:**

> Order `LAB-RSMMC-000000123` berisi 18 parameter Hematologi rutin dan Kalium cito. Analis Sari
> membuka order itu dari Daftar Kerja dan melihat 19 baris dalam satu tabel. Ia mengisi Kalium
> lebih dulu — 6,4 mmol/L, tampil dengan penanda `H` — dan menekan Final pada baris itu pukul
> 09.10. Lalu ia mengisi 18 baris Hematologi sambil sesekali menyimpan, dan menekan Final pada
> masing-masing hingga pukul 09.40.
>
> Dengan dialog lama, pekerjaan yang sama berarti membuka dan menutup dialog 19 kali, dan
> isian yang sedang diketik hilang begitu dialog tertutup tak sengaja.

**Kenapa bukan tetap di Daftar Kerja.** Satu order tidak pernah terlihat utuh di sana, sehingga
`FR-PK-001` tidak terpenuhi, dan pemvalidasi kelak harus merangkai sendiri gambaran order dari
baris-baris yang tercerai.

**Kenapa bukan dua tempat.** Penjaga Final (`LAB-DEC-147`) dan izin hasil (`LAB-DEC-146`) harus
diuji dan dijaga konsisten pada **dua** jalur selamanya — untuk kenyamanan yang dapat dicapai
dengan satu klik dari Daftar Kerja.

**Akibat pada source:** satu halaman frontend baru; dialog pengisian hasil pada
`lab-worklist-view.jsx:253-370` dicabut. Apakah halaman ini membutuhkan jalur baca hasil
tingkat order diputuskan pada tahap desain.

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q1 | `LAB-CLOSE-013` | Bagaimana `LAB-DEC-134` ditegakkan | ✅ Dijawab — `LAB-DEC-146` |
| Q2 | `LAB-CLOSE-014` | Hasil Final ditolak bila disimpan ulang, dan kapan `S4b` diperbaiki | ✅ Dijawab — `LAB-DEC-147` |
| Q3 | `LAB-CLOSE-015` | Penunjukan per orang: kredensial Human Resource atau daftar milik Laboratorium | ✅ Dijawab — `LAB-DEC-148`; `LAB-COORD-016` dibuka |
| Q4 | `LAB-CLOSE-016` | Letak isian hasil Patologi Klinik | ✅ Dijawab — `LAB-DEC-149`, `LAB-FE-017`. **Putaran 15 selesai** |

> **Hasil putaran:** keempat closure question dan kedua conflict capability map revision 5
> tertutup lewat `LAB-DEC-146`..`LAB-DEC-149` serta `LAB-FE-017`; `AC-221`..`AC-237`
> ditambahkan. **Nol keputusan terkunci dicabut.** Dibuka: `LAB-COORD-016` kepada pemilik
> `human-resource`. Menuntut amandemen `LAB-PERM-v1` dan `LAB-VAL-v1`, serta task perbaikan
> atas `S4a` dan `S4b` yang sudah berdiri.

---

## Amendment Pass Putaran 16 — Jawaban kepala instalasi atas butir terbuka (2026-09-24)

**Sumber:** `LAB-EVD-009` — jawaban **dr. Bima Prasetya, Sp.PK**, Kepala Instalasi Laboratorium,
disampaikan pemilik modul Yoga Aji Pratama pada sesi 2026-09-24. **Bukti tertulis dari dr. Bima
belum dilampirkan**; yang dicatat adalah jawaban sebagaimana disampaikan. Isinya apa adanya:

> **DEC-LAB-011:** Pemegang kewenangan validasi hasil laboratorium ditetapkan: dr. Bima Prasetya,
> Sp.PK. Validasi dilakukan oleh dokter yang memiliki kewenangan laboratorium. Sistem akan
> mencatat: nama validator, role validator, tanggal dan waktu validasi, audit trail perubahan.
>
> **LAB-OPEN-029:** Persetujuan Profesor menggunakan approval elektronik dengan audit trail.
> Detail mekanisme menunggu keputusan pihak klinis.
>
> **LAB-COORD-011 dan LAB-COORD-015:** Menunggu keputusan pemilik platform/security terkait
> implementasi gateway WA, PDF, dan QR verification.
>
> **PRD1-CLIN-01:** Balasan WA hanya sebagai konfirmasi komunikasi, bukan pengganti validasi
> klinis.
>
> **PRD1-OPEN-01:** Mohon klarifikasi konteks Nomor Transaksi dan Nomor Mutasi apakah untuk
> billing atau inventory.

**Kedudukan jawaban `DEC-LAB-011`** ditetapkan pemilik modul lewat pilihan **A**: dicatat sebagai
**jawaban sebagian**, bukan keputusan final, karena dibaca apa adanya ia bertentangan dengan
`LAB-DEC-022` butir 3-4 dan dengan pembagian kewenangan per disiplin `LAB-DEC-078`/`LAB-DEC-143`.

### BR-101 — Pemvalidasi pertama, penetapnya, dan kebijakan "hanya dokter" (`LAB-DEC-150`)

**Menjawab sebagian `DEC-LAB-011`. Menggantikan butir 2 `LAB-DEC-022`. Mengamandemen butir 1
`LAB-DEC-142`.**

**Aturan:**

1. **dr. Bima Prasetya, Sp.PK** adalah **pemegang pertama** kewenangan validasi hasil **Patologi
   Klinik**.
2. dr. Bima, selaku Kepala Instalasi Laboratorium, adalah **penetap** pemegang kewenangan
   validasi lainnya. Penetapannya dicatat lewat kredensial Human Resource (`LAB-DEC-148`).
3. **Kebijakan rumah sakit:** validasi hanya dilakukan **dokter yang memiliki kewenangan
   laboratorium**. Analis — termasuk analis senior — **tidak** dapat menjadi pemegang
   kewenangan validasi. Butir 2 `LAB-DEC-022` karena itu **superseded**, dan jabatan calon pada
   lapis jabatan `LAB-DEC-142` dibatasi pada jabatan dokter berkewenangan laboratorium.
4. Setiap validasi mencatat: **nama validator**, **peran/jabatan validator pada saat itu** —
   sebagai snapshot yang tidak ikut berubah bila jabatannya kelak berubah — **waktu validasi**,
   dan **jejak perubahan**. Nama dan waktu sudah dikunci `LAB-DEC-080`; yang **baru** adalah
   snapshot peran.
5. **Yang tetap terbuka** dan diajukan lewat `LAB-REQ-014`:
   - siapa pemegang **kedua** per shift — `LAB-DEC-022` butir 3 **tetap berlaku**;
   - siapa pemegang kewenangan validasi **Mikrobiologi** dan **Patologi Anatomi**.

**Contoh:**

> Pukul 10.00 analis Sari mengisi Kalium 6,4 dan menekan Final. dr. Bima membuka antrean validasi
> dan memvalidasinya pukul 10.05. Yang tercatat: *dr. Bima Prasetya, Sp.PK — Kepala Instalasi
> Laboratorium — 10.05*. Tahun depan dr. Bima berganti jabatan; catatan validasi pukul 10.05 itu
> **tetap** menulis Kepala Instalasi Laboratorium, sebab itulah peran yang ia pegang saat itu.
>
> Analis senior Budi — yang pada contoh `BR-95` dan `BR-96` ditunjuk memvalidasi — **tidak lagi**
> dapat menjadi pemvalidasi. Kedua contoh itu kini tidak berlaku.

**Akibat pada empat mata `LAB-DEC-003`.** Pada Patologi Klinik, pengisi hasil adalah analis
(`LAB-DEC-134`) dan pemvalidasi adalah dokter. Keduanya **tidak mungkin orang yang sama**, sehingga
jalur pengecualian *"divalidasi oleh pengisi sendiri"* praktis tidak lagi terpakai pada Patologi
Klinik. Jalur itu **tetap ada** pada disiplin yang pengisinya dokter, misalnya Patologi Anatomi
(`LAB-DEC-090`).

**Akibat yang harus dibaca bersama keputusan ini — jam malam.**

> Hasil Kalium kritis keluar pukul 02.00. Bila hanya dr. Bima yang memegang kewenangan validasi
> Patologi Klinik dan ia tidak bertugas, hasil itu **tidak dapat divalidasi maupun dirilis**
> sampai ia datang — dan jalur pengecualian juga tidak menolong, sebab analis tidak boleh
> memvalidasi. Itulah sebab `LAB-DEC-022` butir 3 tetap berlaku dan pemegang kedua ditanyakan
> lewat `LAB-REQ-014`.

**Akibat pada perencanaan.** `DEC-LAB-011` **tidak lagi memblokir desain `S4`** untuk Patologi
Klinik: pemvalidasinya bernama, penetapnya jelas, dan bentuk datanya — kredensial per orang per
disiplin — tidak bergantung pada berapa orang yang kelak ditetapkan. Yang **tetap tertahan**:
**rilis** `S4` ke pemakaian nyata (pemegang kedua), serta desain `S4d` dan `S4e` (pemegang
Mikrobiologi dan Patologi Anatomi).

### BR-102 — Balasan WhatsApp adalah konfirmasi komunikasi, bukan bukti baca ulang (`LAB-DEC-151`)

**Menutup `PRD1-CLIN-01`.**

1. Balasan WhatsApp dokter **boleh dicatat** sebagai **konfirmasi bahwa komunikasi terjadi**.
2. Balasan itu **tidak** diterima sebagai **bukti pembacaan ulang** `LAB-DEC-004`. Baca ulang tetap
   wajib lewat percakapan langsung, misalnya telepon — aturan sementara `LAB-DEC-136` butir 6
   menjadi aturan tetap.

**Tafsiran yang dicatat.** Jawaban memakai kata *"validasi klinis"*, sedangkan yang ditanyakan
adalah *"bukti baca ulang"*. Keduanya dibaca sebagai hal yang sama di sini karena jawabannya
**mempertahankan aturan yang lebih ketat** — nol kewenangan klinis dilonggarkan, sehingga
menutupnya tanpa konfirmasi `DR-LAB-001`/`DR-LAB-002` tidak mengambil wewenang siapa pun. Bila
kelak pihak klinis ingin **melonggarkannya**, itu keputusan baru milik mereka.

**Contoh:**

> Pukul 02.11 WhatsApp kabar kritis terkirim. Pukul 02.14 dr. Bagas membalas *"Oke, saya cek."*
> Petugas mencatatnya sebagai konfirmasi komunikasi. Hasil **tetap** di daftar pantau sampai
> pukul 02.18, ketika dr. Bagas menelepon dan membaca ulang *"Kalium tujuh koma dua"*.

### Butir lain dari `LAB-EVD-009`

| Butir | Yang dicatat | Keadaan |
|---|---|---|
| `LAB-OPEN-029` | **Bentuk** persetujuan Profesor: **persetujuan elektronik dengan audit trail** — bukan tanda tangan di kertas. Siapa Profesor yang dimaksud, perannya pada `LAB-PERM-v1`, dan mekanismenya **menunggu pihak klinis** | **Menyempit**, tetap terbuka. Gerbang `LAB-DEC-139` tetap tertutup |
| `LAB-COORD-011`, `LAB-COORD-015` | Masih menunggu pemilik platform/keamanan | **Tidak berubah** |
| `PRD1-OPEN-01` | Pertanyaan balik: billing atau inventory? **Bukti belum cukup untuk keduanya.** Satu-satunya sumber adalah kop cetakan Patologi Klinik sistem lama (`LAB-EVD-005` bagian 4) yang hanya memuat **label**, tanpa nilai. Inventory kecil kemungkinannya — lembar hasil pasien tidak lazim memuat pergerakan stok. Di backend, *mutasi* hanya muncul sebagai **mutasi kamar rawat inap** (`InpBedOccupancyService.cs:992`). Penutupnya: satu contoh cetakan berisi nilai (disamarkan) dan konfirmasi admin sistem lama tentang ruas asalnya | **Tetap terbuka**, tidak memblokir |

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q1 | `DEC-LAB-011` | Bagaimana jawaban dr. Bima diperlakukan | ✅ Dijawab — pilihan A, `LAB-DEC-150`; sisanya diajukan `LAB-REQ-014` |

---

## Amendment Pass Putaran 17 — Jawaban tertulis dr. Bima atas `LAB-REQ-014` (2026-09-25)

**Sumber:** `LAB-EVD-010` — surat **dr. Bima Prasetya, Sp.PK**, Kepala Instalasi Laboratorium,
diteruskan pemilik modul pada sesi 2026-09-25. Disimpan **apa adanya** pada
[`evidence/2026-09-25-jawaban-tertulis-dr-bima-lab-req-014.md`](evidence/2026-09-25-jawaban-tertulis-dr-bima-lab-req-014.md).
**Tertulis dan bernama**, sehingga catatan *"bukti tertulis belum dilampirkan"* pada `LAB-EVD-009`
kini terpenuhi. **Isi surat tidak bertanggal**; tanggal terima dicatat menurut pemilik modul, dan
tanggal kirim sebenarnya ada pada kepala surel aslinya.

**Scope putaran ini:** kewenangan validasi dan rilis hasil laboratorium — `S4`, `S4d`, `S4e`. Tidak
ada butir di luar modul.

### Jawaban diuji terhadap pertanyaan nota

| Pertanyaan `LAB-REQ-014` | Jawaban | Penilaian |
|---|---|---|
| 3 — konfirmasi tertulis bagian 1 | *"Ya, catatan yang disampaikan sudah sesuai"* | ✅ **Lengkap.** `LAB-DEC-150` kini berbukti tertulis, termasuk dr. Bima sebagai **penetap** — butir yang semula hanya kami simpulkan |
| 2 — pemegang Mikrobiologi dan Patologi Anatomi | dr. Nabila Rahmawati, Sp.MK; dr. Citra Maharani, Sp.PA | ✅ **Lengkap**, bernama |
| 1 — validasi di luar jam kerja | *"dokter lain yang telah ditetapkan… pada disiplin terkait"*; Patologi Klinik: *"mengikuti daftar dokter yang ditetapkan dalam kewenangan klinis laboratorium"* | ⚠ **Kebijakan lengkap, nama tidak.** Pilihan A nota — dokter lain — tanpa nama pemegang kedua Patologi Klinik. Diputuskan Q1 |
| 2a — siapa perilis, wajib dokter? | *"pejabat/dokter yang memiliki kewenangan otorisasi sesuai aturan laboratorium"* | ⚠ **Dapat dibaca dua arah**, dan *aturan laboratorium* yang dirujuk **tidak dilampirkan**. Diputuskan Q2 |

### BR-103 — Validasi di luar jam kerja dan pemegang per disiplin (`LAB-DEC-152`)

**Menutup sisa `DEC-LAB-011` dan `LAB-OPEN-034`. Melengkapi `LAB-DEC-150`.**

**Aturan:**

1. Di luar jam dokter utama bertugas, hasil divalidasi **dokter lain yang telah ditetapkan**
   sebagai pemegang kewenangan validasi **pada disiplin yang sama**. Hasil kritis **tidak**
   menunggu pagi — pilihan C nota tidak dipilih.
2. Pemegang kewenangan validasi:

   | Disiplin | Pemegang yang disebut | Pemegang tambahan |
   |---|---|---|
   | Patologi Klinik | dr. Bima Prasetya, Sp.PK | Menurut **daftar penetapan** pada kredensial Human Resource (`LAB-DEC-148`) |
   | Mikrobiologi | dr. Nabila Rahmawati, Sp.MK | Sama |
   | Patologi Anatomi | dr. Citra Maharani, Sp.PA | Sama |

3. Kewenangan validasi **tidak melintasi disiplin**: pemegang validasi Patologi Klinik tidak
   memvalidasi Mikrobiologi. Ini menjawab `LAB-OPEN-034` bagi validasi; bagi rilis, pembagian per
   disiplin sudah ditetapkan `LAB-DEC-143`.
4. **Nama pemegang tambahan adalah data, bukan keputusan.** Ia dicatat penetap lewat kredensial
   Human Resource — wadah yang sudah dirancang `LAB-DEC-148` — sehingga tidak ditunggu lewat surat.
5. `LAB-DEC-022` butir 3 **tetap berlaku**. Akibatnya menjadi **syarat rilis**, bukan syarat
   desain: suatu disiplin baru boleh dilepas ke pemakaian nyata bila sekurang-kurangnya **dua**
   pemegang validasi disiplin itu sudah tercatat — untuk Patologi Klinik, satu selain dr. Bima.

**Kenapa `LAB-OPEN-034` boleh ditutup tanpa pernyataan tersendiri `DR-LAB-001`..`003`.** Jawaban
ini **mempertahankan aturan yang lebih ketat** — kewenangan tidak melintas — sehingga tidak
melonggarkan wewenang klinis siapa pun; alasan yang sama dengan `LAB-DEC-151`. Kedua pemegang
Mikrobiologi dan Patologi Anatomi yang disebut adalah pemegang wewenang klinis disiplinnya sendiri
(`LAB-DEC-078`). Bila kelak rumah sakit ingin **melonggarkannya**, itu keputusan baru milik pihak
klinis per disiplin.

**Contoh:**

> Sabtu pukul 23.30 Kalium 6,9 pasien IGD sudah Final. dr. Bima tidak bertugas. dr. Contoh — yang
> penetapannya sebagai pemegang validasi Patologi Klinik sudah dicatat dr. Bima di kredensial
> Human Resource — memvalidasinya pukul 23.40. Pada malam yang sama, hasil kultur darah
> Mikrobiologi **tidak** dapat divalidasi dr. Contoh; ia menunggu pemegang validasi Mikrobiologi.

### BR-104 — Perilis tidak wajib dokter (`LAB-DEC-153`)

**Menutup `DEC-LAB-018`. Membuka `LAB-OPEN-044`.**

**Aturan:**

1. Hasil yang sudah divalidasi dirilis oleh **pemegang kewenangan otorisasi yang ditetapkan** —
   **dokter atau pejabat non-dokter**. Rilis **tidak** dibatasi pada dokter.
2. Penetapannya dicatat sebagai kode kewenangan **rilis** per disiplin pada kredensial Human
   Resource (`LAB-DEC-143`, `LAB-DEC-148`). Laboratorium tetap hanya membaca.
3. **Jabatan calon perilis** — lapis jabatan `LAB-DEC-142` — ditentukan *aturan laboratorium* yang
   dirujuk surat. Aturan itu **belum dilampirkan** (`LAB-OPEN-044`); admin sistem membacanya
   sebelum memberi aksi rilis (`UNK-P14-03`).
4. **Tetap berlaku:** perilis **bukan** pemvalidasi, kecuali pengecualian beralasan yang tercatat
   dan tercetak (`LAB-DEC-120`, `LAB-DEC-003`); validasi dan otorisasi adalah **dua aktivitas**
   yang masing-masing mencatat identitas, peran, waktu, dan jejak perubahan — dikonfirmasi surat
   butir 3.

**Tafsiran yang dicatat, dan dasarnya.** Surat menulis *"pejabat/dokter"*. Garis miring dibaca
**"atau"** — pilihan pemilik modul pada Q2 — karena surat yang sama memakai kata **dokter** hanya
untuk *Validasi oleh*, sedangkan *Otorisasi oleh* disebut *"pihak yang memberikan persetujuan
akhir"*; dan `LAB-DEC-150` membatasi **validasi** saja pada dokter. Bila dr. Bima bermaksud lain,
itu amandemen atas keputusan ini, bukan kesalahan pencatatan.

**Contoh:**

> Selasa pukul 10.05 dr. Contoh memvalidasi Hemoglobin 9,4. Pukul 10.15 penanggung jawab mutu
> laboratorium — bukan dokter, tetapi jabatannya disebut aturan laboratorium dan ia memegang kode
> rilis Patologi Klinik — merilisnya. Cetakan menulis *Validasi oleh: dr. Contoh* dan *Otorisasi
> oleh: {penanggung jawab mutu}* — dua orang, tanpa pengecualian.
>
> Pada malam hari dengan satu dokter bertugas dan tanpa perilis lain, dokter itu tetap dapat
> merilis hasil yang ia validasi sendiri **bila ia juga memegang kode rilis**, lewat jalur
> pengecualian bertanda *"Dirilis oleh pemvalidasi sendiri"*.

### Butir lain dari `LAB-EVD-010`

| Butir | Yang dicatat | Keadaan |
|---|---|---|
| `LAB-DEC-150` butir 2 — dr. Bima penetap | Dikonfirmasi tertulis: *"ditetapkan oleh Kepala Instalasi Laboratorium sesuai kewenangan klinis yang berlaku"* | ✅ Bukti tertulis melengkapi `LAB-EVD-009` |
| `LAB-DEC-150` butir 4 — snapshot peran | Dikonfirmasi tertulis: *"peran saat melakukan validasi"* | ✅ Tidak berubah |
| `S4d`, `S4e` | Penahan `DEC-LAB-011` dan `LAB-OPEN-034` kini tertutup | **Perlu dinilai ulang** gerbang requirement — bukan diputuskan di sini |

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q1 | Sisa `DEC-LAB-011`, `LAB-OPEN-034` | Bagaimana jawaban tanpa nama pemegang kedua dicatat | ✅ Dijawab — pilihan A *tutup sebagai kebijakan*, `LAB-DEC-152` |
| Q2 | `DEC-LAB-018` | Apakah perilis wajib dokter | ✅ Dijawab — pilihan A *tidak wajib dokter*, `LAB-DEC-153`; `LAB-OPEN-044` dibuka |

> **Hasil putaran:** `DEC-LAB-011` dan `DEC-LAB-018` **tertutup**, begitu juga `LAB-OPEN-034`.
> **Nol keputusan terkunci dicabut.** Satu butir terbuka baru, `LAB-OPEN-044` — isi *aturan
> laboratorium* tentang kewenangan otorisasi — dan ia menahan **langkah rilis** saja. `AC-241` dan
> `AC-242` ditambahkan.

---

## Amendment Pass Putaran 18 — Penyelesaian order dan pemakaian sebelum koreksi `S6` (2026-09-25)

**Sumber:** `LAB-EVD-011` — dua tangkapan layar keputusan pemilik modul, disalin apa adanya pada
[`evidence/2026-09-25-keputusan-penyelesaian-order-dan-pemakaian-sebelum-s6.md`](evidence/2026-09-25-keputusan-penyelesaian-order-dan-pemakaian-sebelum-s6.md),
beserta empat klarifikasi pilihan pada sesi yang sama. **Tangkapan kedua tampak terpotong** sesudah
contoh JSON; bila ada butir lanjutan, ia belum tercatat.

**Scope putaran ini:** penyelesaian order lewat `PUT /lab-orders/{id}/complete`, arti *hasil
resmi*, dan label keadaan hasil di layar. Tidak ada butir di luar modul.

### Keputusan diuji terhadap blueprint

| Butir tangkapan | Yang dikatakan | Terhadap blueprint | Penyelesaian |
|---|---|---|---|
| 1.1–1.2 | Sebelum `S6` selesai, hasil hanya untuk internal; tidak boleh menjadi hasil resmi pasien | Dapat dibaca dua cara: (a) hasil yang sudah dirilis sudah resmi; (b) tidak ada hasil resmi sampai `S6` dibangun | **Q1 — bacaan (a)**, `LAB-DEC-155` |
| 1.3 | Draft → Pemeriksaan Selesai → Menunggu Validasi → Tervalidasi → Dirilis | `LAB-DEC-080` melarang status tersimpan baru; `LAB-DEC-135` butir 2 memasukkan hasil Final **langsung** ke antrean validasi. *Pemeriksaan Selesai* dan *Menunggu Validasi* tampak seperti dua keadaan | **Q2 — satu keadaan**, `LAB-DEC-156` |
| 1.4 | Hanya Tervalidasi + Dirilis yang resmi | Selaras `LAB-DEC-120` dan `INT-08`: dokumen rekam medis lahir saat rilis | Diadopsi, `LAB-DEC-155` |
| 2, pembuka | *Selesai* tidak ditentukan langsung oleh endpoint penyelesaian order | Kode hari ini memindahkan `InProcess` → `Completed` **tanpa memeriksa hasil** (`LabOrderService.cs:1051-1059`) — inti `LAB-CONFLICT-014` | Diadopsi, `LAB-DEC-154` |
| 2.1 | Pemeriksaan tidak boleh *Dalam Pemeriksaan* atau *Menunggu Hasil* | Kedua kata dipakai sebagai keadaan **pemeriksaan**, sedangkan BR-88 memakai *Dalam Pemeriksaan* sebagai label **order** | **Q2** — *Menunggu Hasil* menjadi label pemeriksaan; *Dalam Pemeriksaan* tetap label order. Butir ini dibaca *"pemeriksaan sudah berhasil dan tidak lagi Draft"* |
| 2.2 | Pemeriksaan yang butuh validasi klinis sudah divalidasi, dengan nama, peran, dan waktu validator | Syarat **validasi**, sedangkan `AC-199` menyatakan *Selesai* = seluruhnya **dirilis** | **Q3 — dirilis.** Syarat validasi ikut terpenuhi karena rilis mensyaratkan validasi (`VAL-133`) |
| 2.2, tersirat | *"yang membutuhkan validasi klinis"* | Patologi Anatomi belum punya validasi (`S4e`), Mikrobiologi baru sesudah `MVP-10`, hasil `Sementara` tidak dapat divalidasi | **Q4 — tertahan** |
| 2.3 | `409` beserta rincian pemeriksaan | Respons proyek `ApiResponse` hanya punya `message` dan `errors`; preseden Farmasi `Fail(409, pesan, new { ex.Code })` | Dipetakan pada desain; **bunyi pesan disesuaikan** menjadi *belum dirilis* dan wajib disetujui bersama kontrak |

### BR-105 — Order *Selesai* hanya bila seluruh pemeriksaan tidak batal sudah dirilis (`LAB-DEC-154`)

**Menutup `LAB-CONFLICT-014`.**

**Aturan:**

1. `PUT /lab-orders/{id}/complete` tetap **tindakan manual** petugas, tetapi ia tidak lagi menentukan
   *Selesai* sendiri. Sebelum order berpindah `InProcess` → `Completed`, sistem memeriksa setiap
   pemeriksaan pada order itu.
2. Order boleh `Completed` **hanya bila setiap pemeriksaan yang tidak batal dan tidak gugur sudah
   dirilis**. Rilis mensyaratkan validasi (`VAL-133`), sehingga syarat tangkapan 2 butir 2 — divalidasi
   dokter berwenang, dengan nama, peran, dan waktu validator — **otomatis terpenuhi**: ketiganya
   dicatat `BE-LAB-73` sebagai `ValidatedByUserId`, snapshot jabatan, dan `ValidatedAt`.
3. Pemeriksaan **batal atau gugur tidak menahan** penyelesaian — anggapan dari `AC-199`, dicatat
   terbuka karena tangkapan tidak menyebutnya.
4. Satu saja pemeriksaan belum memenuhi syarat → **ditolak `409`**, order tetap `InProcess`, dan
   respons menyebut **setiap** pemeriksaan yang menahan beserta label keadaannya (BR-107).
5. Pemeriksaan yang **belum punya jalur validasi** — Patologi Anatomi sampai `S4e`, Mikrobiologi
   sampai `MVP-10`, hasil `Sementara` sampai `DEC-LAB-020` — **menahan** order sampai jalurnya ada.
6. Akibatnya `Completed` **tidak mungkin** terjadi selama ada pemeriksaan tidak batal yang belum
   dirilis. Kebalikannya tetap boleh: seluruh hasil sudah dirilis tetapi belum ada yang menekan
   Selesai. Label hasil order (`resultProgress`) lalu terbaca *Selesai* sementara status
   administrasinya masih diproses — dua hal berbeda yang **tidak lagi bertentangan**.

**Contoh:**

> Order `LAB-RSMMC-000000123` berisi Kalium, Hemoglobin, dan Glukosa. Glukosa dibatalkan pukul 08.30.
> Kalium dirilis 09.15, Hemoglobin dirilis 09.50. Pukul 10.00 petugas menekan Selesai → **diterima**.
>
> Bila pukul 10.00 Hemoglobin baru divalidasi dan belum dirilis → **`409`**: *"Order belum dapat
> diselesaikan karena masih terdapat pemeriksaan yang belum dirilis."* Rinciannya: *Hemoglobin —
> Tervalidasi*. Order tetap diproses.
>
> Order Patologi Anatomi berisi Histopatologi yang laporannya sudah Final → **`409`**, rincian
> *Histopatologi — Menunggu Validasi*, sampai `S4e` berdiri.

**Akibat pada source:** penjaga baru pada `LabOrderService.CompleteAsync`, beserta amandemen kontrak
API, validasi, dan state. **Hari ini nol layar frontend memanggil `PUT complete`** (diperiksa pada
`0bcd15724`), sehingga nol pengguna terhenti.

### BR-106 — Hanya hasil Tervalidasi dan Dirilis yang resmi (`LAB-DEC-155`)

**Menjawab bagian pertama pertanyaan *"Bolehkah `S4` dipakai sebelum koreksi `S6` berdiri"*
(`04-prd-to-mvp.md` 21.7). Bagian keduanya dibuka sebagai `LAB-OPEN-045`.**

**Aturan:**

1. **Hasil resmi pasien** adalah hasil yang **Tervalidasi dan Dirilis**. Hanya hasil resmi yang boleh
   dipakai untuk keputusan klinis dokter, dicetak sebagai hasil final, dikirim kepada pasien, atau
   diteruskan ke integrasi di luar Laboratorium — termasuk dokumen rekam medis `INT-08`, yang memang
   lahir saat rilis.
2. Hasil yang **belum dirilis** — *Draft*, *Menunggu Validasi*, *Tervalidasi* — hanya untuk
   pemeriksaan internal laboratorium, review analis, dan verifikasi teknis.
3. Validasi dan rilis **boleh dipakai sungguhan sebelum `S6` berdiri** (Q1). Pertanyaan ini **tidak
   lagi** diusulkan menahan langkah rilis `MVP-9d` maupun `MVP-10c`.
4. **Belum dijawab:** prosedur bila hasil yang **sudah dirilis** ternyata keliru selama `S6` belum ada.
   Sistem tidak dapat mengoreksinya — hasil terkunci sesudah rilis, dan pembatalan pemeriksaan
   ditolak `VAL-143`. Dibuka sebagai `LAB-OPEN-045`.
5. Pelaporan **nilai kritis** sebelum rilis **tidak** diputuskan di sini — tetap `DEC-LAB-017`.

**Contoh:**

> Senin pukul 09.10 Kalium 6,4 pasien rawat jalan berstatus *Menunggu Validasi*. Angka itu dipakai
> analis untuk memutuskan pemeriksaan ulang, tetapi **belum** boleh menjadi dasar resep dokter. Pukul
> 09.15 dr. Contoh memvalidasi dan perilis merilisnya; sejak saat itu hasilnya resmi dan tercatat di
> rekam medis.
>
> Pukul 11.00 ketahuan tabungnya milik pasien lain. Tanpa `S6` sistem tidak dapat menarik hasil itu,
> dan belum ada prosedur tertulis siapa memberi tahu dokter pemesan — itulah `LAB-OPEN-045`.

**Kenapa dicatat atas nama pemilik modul.** Pertanyaan induknya semula ditujukan juga kepada
`DR-LAB-001`. Butir 1-2 **memperketat** — hasil yang belum dirilis tidak resmi — sehingga tidak
melonggarkan wewenang klinis siapa pun. Butir 3, memakai hasil terrilis tanpa jalur koreksi,
menyentuh keselamatan klinis; **diusulkan** dikonfirmasi `DR-LAB-001` bersama `LAB-OPEN-045`.

**Akibat pada source:** nol. Backend sudah menahan pengiriman hasil yang belum dirilis
(`IsReleased`, `DeliveryBlockedReason`), dan frontend belum punya cetakan hasil sama sekali — yang
ada hanya label dan nota order. Aturan ini **mengikat `S17` dan `S18` kelak**.

### BR-107 — Label keadaan hasil di layar (`LAB-DEC-156`)

**Mengubah tabel label BR-88 butir 5 (`LAB-DEC-135`). Nol status tersimpan baru — `LAB-DEC-080`
utuh.**

**Aturan:**

1. Keadaan pemeriksaan ditampilkan dengan label berikut, diturunkan dari `resultStatus`:

   | `resultStatus` | Label layar | Kapan |
   |---|---|---|
   | `NotEntered` | **Menunggu Hasil** | Belum ada hasil tersimpan |
   | `Draft` | **Draft** | Hasil tersimpan, `FinalizedAt` kosong |
   | `Final` | **Menunggu Validasi** | `FinalizedAt` terisi, belum divalidasi |
   | `Validated` | **Tervalidasi** | `ValidatedAt` terisi, belum dirilis |
   | `Released` | **Dirilis** | `ReleasedAt` terisi — hasil resmi |

2. Tindakan analis yang memindahkan Draft ke Final bernama **Pemeriksaan Selesai**. Itu nama
   tombolnya, **bukan** keadaan tersendiri: hasil Final tetap langsung masuk antrean validasi
   (`LAB-DEC-135` butir 2), tanpa tindakan *ajukan ke validasi*.
3. Label **order** tetap BR-88: *Dalam Pemeriksaan* dan *Selesai*. *Dalam Pemeriksaan* **tidak**
   dipakai untuk pemeriksaan.
4. **Yang berubah dari BR-88:** label *Final* menjadi *Menunggu Validasi*; *Menunggu Hasil*,
   *Tervalidasi*, dan *Dirilis* ditambahkan. *Amendment* tetap milik `S6`.
5. **Batas:** layar laporan Patologi Anatomi tidak menampilkan lencana keadaan (`LAB-FE-029`) dan
   tombolnya *Selesaikan* (`LAB-FE-024`) — **tidak diubah** keputusan ini sampai `S4e`. Hasil
   Mikrobiologi `Sementara` tetap berlabel *Menunggu Validasi*, disertai keterangan *"Hasil sementara
   belum dapat divalidasi"*; kualifikasi bukan keadaan (`LAB-DEC-114`).

**Contoh:**

> Analis Sari menyimpan Kalium → layar menulis *Draft*. Ia menekan **Pemeriksaan Selesai** → *Menunggu
> Validasi*, dan Kalium muncul di antrean dr. Bima. dr. Bima memvalidasi → *Tervalidasi*. Perilis
> merilis → *Dirilis*. Hemoglobin pada order yang sama belum diisi → *Menunggu Hasil*. Ordernya
> berlabel *Dalam Pemeriksaan* sampai Hemoglobin juga dirilis.

**Akibat pada source:** frontend saja — pemetaan label dari `resultStatus` dan teks tombol Final.
Halaman Mikrobiologi hari ini memakai tombol *Simpan Final* dan peringatan *"Penulisan selesai —
belum dirilis"* (`lab-microbiology-completion-bar.jsx`). **Nol perubahan kontrak.**

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q1 | Pemakaian sebelum `S6` | Rilis sudah resmi, atau seluruh hasil internal sampai `S6` berdiri | ✅ Dijawab — **rilis sudah resmi**, `LAB-DEC-155`; `LAB-OPEN-045` dibuka |
| Q2 | Label keadaan | *Pemeriksaan Selesai* dan *Menunggu Validasi* satu keadaan atau dua | ✅ Dijawab — **satu: Final = Menunggu Validasi**, `LAB-DEC-156` |
| Q3 | `LAB-CONFLICT-014` | Order Selesai bila seluruhnya tervalidasi, atau dirilis | ✅ Dijawab — **dirilis**, `LAB-DEC-154` |
| Q4 | `LAB-CONFLICT-014` | Pemeriksaan tanpa jalur validasi menahan order atau cukup Final | ✅ Dijawab — **tertahan**, `LAB-DEC-154` butir 5 |

> **Hasil putaran:** `LAB-CONFLICT-014` **tertutup**; pertanyaan *pemakaian sebelum `S6`* terjawab
> **sebagian**. Satu keputusan terkunci diamandemen, pada tingkat label tampilan saja: tabel BR-88
> butir 5 (`LAB-DEC-135`). Satu butir terbuka baru, `LAB-OPEN-045`. `AC-243`..`AC-247` ditambahkan.

---

## Amendment Pass Putaran 19 — Enam penahan gerbang `LAB-RCG-001-r10` (2026-09-25)

**Sumber:** jawaban pemilik modul atas pertanyaan pilihan pada sesi 2026-09-25, menutup penahan
yang dicatat gerbang `LAB-RCG-001-r10` (`02-requirement-completeness-assessment.md` bagian 0E).
Bukti lapangan yang ditimbang: `LAB-EVD-003` (detail hasil PA, 2026-09-18) dan `LAB-EVD-005`
(cetakan tiga disiplin). Fakta kode dibaca pada backend `cfafad8d` dan frontend `0bcd15724`.

**Capability map rev 5 dipindai pada `ddeb5ed8`/`72607a087`** — berpotensi basi terhadap kedua SHA
itu. Dua kemampuan yang dipakai putaran ini (dokumen klinis pasien Clinical Management, pemesanan
dari kunjungan MCU) **belum ber-`CAP`**; jawaban di bawah bersandar pada pembacaan kode gerbang r10,
dan verifikasi berjalannya diteruskan ke `/trace-existing-capabilities`.

### Batas scope putaran ini (dikonfirmasi pemilik modul 2026-09-25)

| Di dalam scope | Di luar scope — untuk pemilik lain |
|---|---|
| `DEC-LAB-026` pemilik penautan berkas hasil eksternal (`S18`) | Penyimpanan berkas — `DEC-LAB-016`, keputusan privasi platform |
| `DEC-LAB-025` definisi laporan operasional (`S16`) | Penyediaan pemberitahuan — `LAB-COORD-017`, pemilik platform |
| `DEC-LAB-023` lokasi, pola, metode pengambilan (`S2b`) | Aturan internal dokumen klinis pasien — pemilik `clinical-management` |
| `DEC-LAB-022` seri Sitologi/FNAB, blok dan slide (`S2b`) | Paket, harga paket, dan rekap MCU — layanan MCU dan `billing-kasir` |
| `DEC-LAB-024` kejadian yang diberitahukan (`S8`) | |
| `DEC-LAB-027` order dari MCU (`S19`) | |

### BR-108 — Penautan PDF hasil eksternal: satu data, pintu di Laboratorium (`LAB-DEC-157`)

**Menjawab bagian pemilik `DEC-LAB-026`. Mengamandemen `LAB-DEC-030` pada baris *penautan hasil
laboratorium eksternal berupa berkas PDF*.**

**Aturan:**

1. **Datanya satu**, disimpan sebagai **dokumen klinis pasien milik Clinical Management**
   (`TrxPatientClinicalDocument`) dengan jenis `LaboratoryResult` dan sumber `ExternalHospital`.
   Laboratorium **tidak** menyimpan salinan maupun daftar tandingan.
2. **Laboratorium menyediakan pintu unggahnya** — layar tempat petugas lab menautkan PDF — sebab
   bukti lapangan menunjukkan petugas lab yang melakukannya (`Bagian Ketiga` `CAP-014`, `BP-003`).
   Pintu lain milik Clinical Management tetap sah; keduanya menulis ke data yang sama.
3. `LAB-DEC-030` **tetap** menempatkan kemampuan ini di Laboratorium — yang berubah adalah **tempat
   datanya**, bukan pemilik alur kerjanya.
4. Kesepakatan dengan pemilik `clinical-management` dibuka sebagai **`LAB-COORD-018`**: Laboratorium
   menulis ke dokumen klinis pasien, dan kemampuan itu hari ini **belum** mengunggah berkas —
   `POST` hanya menerima `FilePath` teks.
5. **Penyimpanan berkas tetap `DEC-LAB-016`.** Keputusan ini tidak menjawab di mana PDF tinggal.

**Contoh:**

> Pasien rujukan membawa PDF HbA1c dari laboratorium klinik lain. Petugas penerimaan lab
> menautkannya dari layar Laboratorium. Perawat poli yang kemudian membuka dokumen klinis pasien
> melihat **berkas yang sama** — bukan salinan kedua — lengkap dengan siapa yang mengunggahnya.

### BR-109 — Tautan, identitas, dan penarikan berkas eksternal (`LAB-DEC-158`)

**Menjawab sisa `DEC-LAB-026`.**

**Aturan:**

1. PDF ditautkan ke **pasien dan kunjungan** saat berkas diterima, sehingga tampil pada linimasa
   rekam medis kunjungan itu. Ruas keduanya sudah ada pada dokumen klinis pasien.
2. Sebelum menyimpan, pengunggah **wajib mencocokkan dua identitas pasien** yang tertera pada berkas
   — nama **dan** tanggal lahir atau No. RM — dengan pasien yang dipilih.
3. Berkas yang ternyata milik pasien lain ditandai **salah input** beralasan — status
   `EnteredInError` yang sudah ada — oleh **pengunggahnya atau kepala instalasi**. Berkas **tidak
   dihapus**; siapa, kapan, dan alasannya tetap terbaca.
4. Berkas eksternal **bukan** hasil resmi laboratorium ini (`LAB-DEC-155`): ia dokumen pendukung yang
   tidak divalidasi maupun dirilis pemvalidasi rumah sakit.

**Contoh:**

> Petugas memilih pasien Siti Aminah, No. RM 00123456, lalu membuka PDF bertuliskan *Siti Aminah,
> lahir 12-03-1980*. Tanggal lahir cocok → berkas disimpan. Sore harinya ketahuan PDF itu milik
> *Siti Aminah* lain dengan tanggal lahir 21-03-1980 yang salah terbaca. Kepala instalasi menandainya
> *salah input — tanggal lahir salah baca*; berkas tetap terlihat di riwayat beserta alasannya.

### BR-110 — Tiga laporan operasional pertama (`LAB-DEC-159`)

**Menjawab sebagian `DEC-LAB-025`. Mengesahkan `DEC-LAB-007`, yang sejak 2026-09-01 hanya usulan.**

**Aturan:**

1. `S16` **dipecah**. **`S16a`** berisi tiga laporan di bawah, seluruhnya dari data milik Laboratorium
   dan tanpa kontrak lintas modul. Delapan laporan lainnya — termasuk biaya, pasien perusahaan, dan
   kelompok penyakit — **tetap `DEC-LAB-025`**, menunggu daftar dan definisi dari kepala instalasi.
2. **Jumlah pemeriksaan** — dihitung **pada tanggal rilis**; hanya hasil resmi (`LAB-DEC-155`);
   pemeriksaan batal atau gugur tidak dihitung.
3. **Angka penolakan sampel** — wadah yang **dinyatakan tidak layak** ÷ **seluruh wadah yang
   diputuskan kelayakannya** pada periode itu, menurut **waktu keputusan** (`DecidedAt`), dirinci per
   disiplin dan per alasan penolakan.
4. **Waktu penyelesaian (TAT)** — dari wadah **dinyatakan layak** sampai hasil **dirilis**, dirinci
   per disiplin dengan cito dan rutin **terpisah**. Sama dengan definisi keterlambatan cito `AC-17`,
   sehingga laporan dan daftar pantau tidak berselisih.

**Contoh:**

> **Jumlah.** Kalium dipesan 30 September dan dirilis 1 Oktober → masuk hitungan **Oktober**.
>
> **Penolakan.** September: 400 wadah diputuskan, 12 ditolak (8 hemolisis, 4 volume kurang) →
> **3,0%**, dengan rincian per alasan.
>
> **TAT.** Kalium cito dinyatakan layak 08.00 dan dirilis 09.10 → **70 menit**, masuk kelompok
> *cito*. Pada batas cito 60 menit, pemeriksaan ini terhitung terlambat — **sama** dengan yang tampil
> di daftar pantau keterlambatan.

### BR-111 — Pembaca laporan (`LAB-DEC-160`)

**Aturan:** ketiga laporan `S16a` dibuka dan diunduh lewat **hak akses tersendiri**, yang diberikan
admin kepada jabatan tertentu — **kepala instalasi dan manajemen**. Hak baca daftar Laboratorium
**tidak** otomatis membukanya, sebab laporan merangkum seluruh pasien dan seluruh petugas.

**Contoh:** analis yang dapat membuka daftar Pemeriksaan tidak melihat menu laporan; kepala instalasi
dan direktur pelayanan melihatnya karena jabatannya diberi izin laporan.

### BR-112 — Lokasi, pola, dan metode pengambilan specimen (`LAB-DEC-161`)

**Menutup `DEC-LAB-023` dan `LAB-DEC-145` butir 4.**

**Aturan:**

1. Ketiga ruas **melekat pada wadah**, dan diisi di **halaman hasil** oleh **pengisi hasil
   disiplinnya** — Patologi Anatomi: Dokter Lab; Mikrobiologi: analis — **sampai Final**. Setiap
   perubahan berjejak, dan sesudah Final ketiganya baca-saja — pola Informasi Specimen Mikrobiologi
   (`LAB-DEC-107`, `LAB-DEC-112`).
2. **Lokasi** dan **metode** dipilih dari **daftar terkendali**. Bila tidak ada, pengisi memilih
   **`Lainnya`** dan **wajib** menulis keterangan; nilai baru masuk daftar **hanya** lewat kepala
   instalasi di layar data induk — pola jenis specimen dan `LAB-DEC-099`.
3. **Pola pengambilan** memakai tiga nilai dari bukti lapangan: **Tunggal**, **Serial**, **Hormonal**.
4. **Kewajiban** ketiga ruas **disetel kepala instalasi per kategori atau jenis pemeriksaan**, sejalan
   keberlakuan parameter PA (`VAL-95`); **bawaannya opsional**. Final ditolak bila ruas yang disetel
   wajib masih kosong.

**Pertentangan dengan bukti lapangan, dan arahnya.** `LAB-EVD-003` `CAP-006` menambahkan nilai
`lainnya` **langsung** ke daftar sesudah pemeriksaan duplikasi. Diputuskan **ke arah blueprint**:
pemeriksaan duplikasi tidak mengenali sinonim, sehingga *payudara kiri*, *mammae sinistra*, dan
*Payudara ki.* akan lolos sebagai tiga lokasi berbeda.

**Contoh:**

> Kepala instalasi menyetel *Pola* **wajib** bagi Sitologi Ginekologi dan **tidak berlaku** bagi
> Histologi. dr. Citra menulis laporan Pap smear tanpa mengisi Pola lalu menekan Final → ditolak,
> dengan pesan yang menyebut ruas Pola. Pada biopsi payudara, Pola kosong **tidak** menahan Final.
> Lokasi *kuadran lateral atas payudara kiri* belum ada di daftar → dr. Citra memilih *Lainnya* dan
> mengetik keterangannya; daftar tidak bertambah sampai kepala instalasi menambahkannya.

### BR-113 — Blok parafin dan slide tidak dilacak (`LAB-DEC-162`)

**Menjawab separuh `DEC-LAB-022`. Separuh lainnya — seri nomor Sitologi dan FNAB — tetap terbuka.**

**Aturan:**

1. Blok parafin dan slide **tidak** dicatat sebagai benda fisik — tanpa nomor blok, lokasi arsip,
   maupun riwayat peminjaman — sampai ada kebutuhan tertulis tentang arsip dan peminjaman.
2. Bila **jumlah** blok atau slide perlu tercetak pada laporan, ia menjadi **parameter laporan PA**
   (`LAB-DEC-086`) pada kategori yang membutuhkannya — **tanpa konsep baru**.
3. **Seri nomor Sitologi dan FNAB ditahan** sampai cetakan Sitologi, IHK, dan FNAB diserahkan
   (`LAB-OPEN-039`). Sampai itu, laporan PA memakai seri PA (`LAB-DEC-117`). Alasannya: nomor yang
   sudah terbit tidak dapat diubah mundur, sehingga menebak lebih mahal daripada menunggu.

**Contoh:** laporan histologi yang perlu mencetak *Jumlah blok: 3, Jumlah slide: 6* memperolehnya dari
dua parameter laporan PA; tidak ada daftar blok bernomor di mana pun.

### BR-114 — Pemberitahuan: tidak ada kejadian lain, `S8` dilebur (`LAB-DEC-163`)

**Menutup `DEC-LAB-024`.**

**Aturan:**

1. Di luar **nilai kritis** (`S5`) dan **koreksi hasil** (`S6`), **tidak ada** kejadian laboratorium
   yang wajib memberi tahu dokter. Hasil dirilis **tidak** mengirim pemberitahuan.
2. **`S8` tidak berdiri sendiri lagi.** Ia dilebur menjadi **dependency** `S5` dan `S6` atas
   kemampuan pemberitahuan platform (`LAB-COORD-017`).
3. Alasannya: nol bukti kebutuhan lain, dan setiap janji pemberitahuan membuat dokter
   mengandalkannya — padahal platform belum punya sarananya.

**Contoh:** Hemoglobin rawat jalan dirilis pukul 10.15. Dokter pemesan **tidak** menerima pemberitahuan;
ia membaca hasilnya dari halaman pasien. Bila Hemoglobin itu 5,1 — nilai kritis — pelaporannya
mengikuti `S5` (`LAB-DEC-004`), bukan `S8`.

### BR-115 — Order dari MCU memakai jalur umum (`LAB-DEC-164`)

**Menutup `DEC-LAB-027`. Mengamandemen `LAB-DEC-030` pada baris *order dari MCU*.**

**Aturan:**

1. Order dari kunjungan MCU memakai **jalur pemesanan biasa**. Modul Laboratorium **tidak**
   membangun sumber order MCU tersendiri.
2. **Paket pemeriksaan, harga paket, rekap untuk perusahaan, dan penerima hasil selain pasien**
   menjadi milik **layanan MCU** kelak — bukan Laboratorium.
3. **`S19` ditutup dari modul Laboratorium** sesudah pemesanan dari kunjungan `MedicalCheckup`
   **terbukti berjalan**. Sampai terbukti, `S19` berstatus *menunggu verifikasi*, bukan *tertahan
   keputusan*.

**Contoh:** PT Contoh mengirim 40 karyawan. Setiap karyawan terdaftar sebagai kunjungan MCU, dan
dokter MCU memesan darah lengkap lewat layar pemesanan biasa. Harga paket Rp 350.000 dan rekap
untuk perusahaan **bukan** urusan Laboratorium.

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---:|---|---|---|
| Q0 | Scope | Batas scope putaran 19 | ✅ Disetujui apa adanya |
| Q1 | `DEC-LAB-026` | Pemilik penautan | ✅ *Satu data, pintu di Lab* — `LAB-DEC-157`; `LAB-COORD-018` dibuka |
| Q2 | `DEC-LAB-026` | Ditautkan ke apa | ✅ *Pasien + kunjungan* — `LAB-DEC-158` |
| Q3 | `DEC-LAB-026` | Pasien salah | ✅ *Cek dua identitas + tanda salah input* — `LAB-DEC-158` |
| Q4 | `DEC-LAB-025` | Kelanjutan `S16` | ✅ *Tiga laporan dulu* — `LAB-DEC-159` |
| Q5 | `DEC-LAB-025` | Titik hitung jumlah pemeriksaan | ✅ *Saat dirilis* — `LAB-DEC-159` |
| Q6 | `DEC-LAB-025` | Rumus penolakan | ✅ *Per wadah* — `LAB-DEC-159` |
| Q7 | `DEC-LAB-025` | Definisi TAT | ✅ *Layak sampai dirilis* — `LAB-DEC-159` |
| Q8 | `DEC-LAB-025` | Pembaca laporan | ✅ *Hak akses tersendiri* — `LAB-DEC-160` |
| Q9 | `DEC-LAB-023` | Tempat dan pengisi | ✅ *Wadah, pengisi hasil* — `LAB-DEC-161` |
| Q10 | `DEC-LAB-023` | Nilai di luar daftar | ✅ *Lainnya + keterangan* — `LAB-DEC-161` |
| Q11 | `DEC-LAB-023` | Kewajiban | ✅ *Diatur per kategori* — `LAB-DEC-161` |
| Q12 | `DEC-LAB-022` | Seri Sitologi/FNAB | ⏸ *Tahan sampai cetakan ada* — tetap terbuka |
| Q13 | `DEC-LAB-022` | Blok dan slide | ✅ *Tidak dilacak dulu* — `LAB-DEC-162` |
| Q14 | `DEC-LAB-024` | Kejadian yang diberitahukan | ✅ *Tidak ada, `S8` dilebur* — `LAB-DEC-163` |
| Q15 | `DEC-LAB-027` | Perlakuan MCU | ✅ *Jalur umum saja* — `LAB-DEC-164` |

> **Hasil putaran:** `DEC-LAB-023`, `DEC-LAB-024`, `DEC-LAB-026`, dan `DEC-LAB-027` **tertutup**.
> `DEC-LAB-022` **menyempit** ke seri nomor Sitologi/FNAB saja; `DEC-LAB-025` **menyempit** ke
> delapan laporan lainnya. Satu koordinasi baru, `LAB-COORD-018`. `LAB-DEC-030` diamandemen pada
> dua baris — tempat data penautan PDF dan order MCU; `LAB-DEC-145` butir 4 terjawab. **Satu
> pertentangan dengan bukti lapangan** (`CAP-006`, daftar lokasi tumbuh otomatis) diselesaikan ke
> arah blueprint. `AC-248`..`AC-256` ditambahkan. **Kesiapan kelima slice perlu dinilai ulang
> gerbang** — bukan diputuskan di sini.

---

## Amendment Pass Putaran 20 — Urutan langkah kiosk Pasien Lama (2026-09-30)

**Sumber:** keputusan blueprint `kiosk` — `KSK-DEC-002` dan `KSK-DEC-014` (`kiosk/00-interview-decisions.md`),
`approved` oleh Sukma Giri Pratama, 30 Sep 2026. Dicatat di blueprint ini untuk menutup `KSK-OQ-004`
atas persetujuan Sukma Giri Pratama (pemilik `KSK-OQ-004`), 30 Sep 2026.

**Yang berubah hanya urutan langkah dan saat sesi kiosk dibentuk di alur kiosk Pasien Lama** — keduanya
milik alur kiosk `registration-management`, bukan keputusan Laboratorium.

| Hal | Bunyi lama (`FE-LAB-13`, 2026-09-16) | Bunyi baru (`KSK-DEC-002`/`014`, dikerjakan `FE-KSK-007`) |
|---|---|---|
| Letak pilihan Tujuan Layanan (Poliklinik / Laboratorium + surat dokter) | Langkah **pertama** alur Pasien Lama, sebelum identitas dipindai | Langkah **ketiga**, sesudah Identifikasi dan Review Data |
| Saat sesi kiosk (`scan-result`) dibentuk | Di langkah Identifikasi, saat kartu dipindai | **Tepat sekali**, saat Tujuan Layanan dipilih di langkah ketiga — untuk Poliklinik maupun Laboratorium. Identifikasi mengenali pasien lewat pencarian tanpa membentuk sesi; hasil pindai disimpan di memori layar |
| Urutan alur Laboratorium | Tujuan Layanan → Identifikasi → Selesai | Identifikasi → Review Data → Tujuan Layanan → Selesai |

**Yang tidak berubah, dan diperiksa satu per satu:**

1. Aturan BR-46 butir 1–4 serta `LAB-DEC-051`/`052`: pasien tetap memilih Laboratorium sendiri, dan dua jalur (membawa
   permintaan dokter / periksa mandiri) tetap ditanyakan di kiosk.
2. Tiga jawaban pemilik modul saat `FE-LAB-13`: **Pasien Lama saja**; alur Laboratorium **berhenti sesudah identitas
   terbaca** (kini sesudah identitas terbaca **dan** Review Data); **cabang Poliklinik tidak menuliskan ruas tujuan
   apa pun** — muatan `scan-result` cabang itu tetap objek tanpa `targetService`/`hasPhysicianRequest` (`KSK-DSN-007`).
3. Kontrak `scan-result` dan kedua ruas `TrxKioskScanSession` (`BE-EXT-04`/`04b`): tidak berubah. Sesi tetap ditulis
   sekali dan tidak punya jalur ubah.
4. Panel pendaftaran pasien laboratorium (`FE-LAB-14`): tetap membaca sesi bertujuan Laboratorium yang belum terpakai.

**Catatan tafsir `AC-93`.** Bunyi `AC-93` tidak diubah. "Perilaku sesi kiosk yang sudah ada" dibaca sebagai perilaku
**data dan kontrak** sesi (ruas, jalur baca, sesi lama tetap terbaca) — itulah yang dibuktikan `T-93a`..`T-93d`.
Pergeseran **saat** sesi dibentuk pada alur Pasien Lama adalah keputusan alur kiosk (`KSK-DEC-014`) dan dibuktikan
oleh acceptance criteria `FE-KSK-007` (tepat satu `scan-result` per perjalanan, sesudah Tujuan Layanan dipilih).

**Pemberitahuan.** Pemilik modul Laboratorium (Yoga Aji Pratama) perlu diberi tahu amendment ini. Tidak ada
keputusan Laboratorium yang berubah, sehingga tidak dibutuhkan keputusan baru dari pemilik modul.

---

## Amendment Pass Putaran 21 — Keputusan pembuka roadmap yang tertahan (2026-10-06)

**Sumber:** keputusan tertulis pemilik modul **`LAB-EVD-012`** — *Keputusan untuk Membuka Roadmap FE/BE
Laboratorium yang masih tertahan*, 14 butir ditambah dua bagian penutup — disimpan verbatim pada
[`evidence/2026-10-06-keputusan-pembuka-roadmap-tertahan.md`](evidence/2026-10-06-keputusan-pembuka-roadmap-tertahan.md).
Enam klarifikasi pilihan pada sesi yang sama (bagian B berkas itu) menyelesaikan titik yang bersinggungan
dengan keputusan terkunci; **keenamnya dijawab dengan pilihan yang direkomendasikan**. Fakta kode dibaca
pada backend `6924b689` dan frontend `a1404291d`.

**Capability map revision 6 dipindai 2026-09-25 dan berpotensi basi** terhadap kedua SHA itu. Putaran ini
hanya bersandar pada **satu** fakta kode baru, diperiksa 2026-10-06 pada `6924b689`: backend **nol** layanan
penyimpanan berkas bersama — nol `BlobServiceClient`, `IAmazonS3`, `MinioClient`, maupun antarmuka
penyimpanan umum. Unggahan yang ada tetap memakai `SaveFileAsync` privat di atas `IWebHostEnvironment`,
sama dengan temuan `DEC-LAB-016` 2026-09-18. Verifikasi rinci diteruskan ke `/trace-existing-capabilities`
sebelum desain.

### Batas scope putaran ini

Scope ditetapkan **oleh teks pemilik modul sendiri** — keempat belas butirnya dan daftar penggolongan
ulang penahan. Satu-satunya butir di luar teks yang diputuskan adalah **`DEC-LAB-019`**, sebab ia penahan
desain `S6` yang tidak disebut teks; tanpa jawabannya `S6` tetap tertahan walau `DEC-LAB-014` tertutup
(klarifikasi Q3).

| Di dalam scope | Di luar scope — untuk pemilik lain |
|---|---|
| Atribut wadah PA: lokasi (`DEC-LAB-029`), fiksasi (`DEC-LAB-028`), seri nomor Sitologi/FNAB (`DEC-LAB-022`) | **Isi** daftar klinis dan operasional — aturan kritis, SLA, kategori sensitif, jenis fiksasi, awalan seri: dokter disiplin dan Kepala Instalasi, sebelum go-live |
| Hasil Mikrobiologi `Sementara` (`DEC-LAB-020`) dan validasi PA (`DEC-LAB-021`) | **Penyediaan** pembangkit PDF, gerbang WhatsApp, pemberitahuan, dan penyimpanan berkas privat — pemilik platform |
| Nilai kritis: penyetuju perubahan (`DEC-LAB-012`) dan alurnya (`LAB-P0-004`, `LAB-OPEN-014`) | **Persetujuan** model akses tanpa login — pemilik keamanan/platform (`LAB-COORD-015`) |
| Koreksi sesudah rilis (`DEC-LAB-014`, `DEC-LAB-019`) | Aturan internal dokumen klinis pasien — pemilik `clinical-management` (`LAB-COORD-018`) |
| Penyerahan hasil ke pasien: kewenangan (`LAB-OPEN-029`), dokumen dan pengiriman (`LAB-COORD-011`), verifikasi QR (`LAB-COORD-015`) | Definisi delapan laporan `S16b` — menunggu katalog laporan |
| Penyimpanan berkas klinis (`DEC-LAB-016`) dan arti `Verified`/`Approved` (`LAB-COORD-018`) | |
| Penundaan `S16b` (`DEC-LAB-025`) dan penggolongan ulang penahan | |

### Keputusan diuji terhadap blueprint

Setiap butir dibaca terhadap keputusan yang sudah terkunci. **Empat bersinggungan**, dan keempatnya
diselesaikan dengan klarifikasi pilihan — bukan ditafsirkan diam-diam.

| Butir teks | Bersinggungan dengan | Bahayanya bila dibiarkan | Penyelesaian |
|---|---|---|---|
| 6 — Kepala Instalasi penyetuju akhir perubahan nilai kritis | BR-19 `LAB-DEC-023`: Kepala Instalasi **mengajukan**, pihak lain **menyetujui**. Contoh BR-19 justru Kepala Instalasi menaikkan batas Kalium 6,0 → 8,0 | Bila Kepala Instalasi mengusulkan sekaligus menyetujui, perlindungan BR-19 hilang tanpa satu aturan pun dilanggar | Q1: **pengusul ≠ penyetuju** — `LAB-DEC-175` |
| 7 — *Acknowledgement* menghentikan eskalasi | `LAB-DEC-004` (diteken `DR-LAB-001`..`003`): bukti pembacaan ulang; `LAB-DEC-151`: balasan WhatsApp bukan bukti baca ulang | Bila klik di aplikasi dianggap cukup, `LAB-DEC-004` melonggar tanpa tanda tangan ulang | Q2: **acknowledgement = laporan baca ulang** — `LAB-DEC-177` |
| 4 — `Sementara` dirilis | `LAB-DEC-154` (order Selesai bila semua dirilis), `LAB-DEC-139` (gerbang pasien saat Selesai), `LAB-DEC-163` (rilis tanpa pemberitahuan) | Order Selesai dan terkirim ke pasien saat biakan belum selesai; dokter tidak tahu hasil definitif berbeda | Q5 dan Q6 — `LAB-DEC-172`, `LAB-DEC-173` |
| 8 — koreksi | `DEC-LAB-019`: batal atau koreksi sesudah rilis — **tidak disebut** teks | `S6` tetap tertahan walau `DEC-LAB-014` tertutup | Q3: **selalu lewat koreksi** — `LAB-DEC-179`. Q4 menyusul untuk sisi pasien — `LAB-DEC-180` |

**Tiga butir mengubah keputusan terkunci, dan perubahan itu memang dikehendaki teksnya:**

| Butir | Keputusan yang berubah | Bentuk perubahan |
|---|---|---|
| 5 — validasi PA oleh penulisnya | `LAB-DEC-003` empat mata pada **validasi**, diteken `DR-LAB-003` bagi PA | **Bagi PA saja**, empat mata pindah ke **rilis**. Patologi Klinik dan Mikrobiologi tidak berubah. Ini **melonggarkan** aturan yang diteken dokter, sehingga pemakaian nyatanya menunggu tanda tangan `DR-LAB-003` (`LAB-SIGN-002`) — sesuai `LAB-DEC-079`: amandemen per disiplin diteken pemegang disiplinnya |
| 2 — seri Sitologi/FNAB | `LAB-DEC-162` butir 3 (seri ditahan sampai cetakan ada) dan `LAB-DEC-117` (nomor cetak per disiplin per tahun) | Seri tidak lagi menunggu cetakan; alokator nomor cetak PA berkunci **seri**, bukan disiplin |
| 10 — persetujuan sebelum dikirim ke pasien | `LAB-DEC-067` dan `LAB-DEC-139` (syarat persetujuan *Profesor dan Dokter Lab*); `LAB-DEC-121` (bukan wewenang pemilik modul sendirian) | Syarat bergelar diganti izin berbasis peran; hasil normal tanpa persetujuan tambahan. Konfirmasi Kepala Instalasi dicatat pada `LAB-SIGN-002` |

### BR-116 — Satu wadah, satu lokasi (`LAB-DEC-168`)

**Menutup `DEC-LAB-029`. Melengkapi `LAB-DEC-161`.**

**Aturan:**

1. Satu wadah specimen mewakili **tepat satu** lokasi anatomi atau sumber specimen.
2. Specimen dari beberapa lokasi dibuat sebagai **beberapa wadah**, masing-masing dengan identitas dan nomor
   wadahnya sendiri.
3. Satu order tetap boleh memiliki banyak wadah (`LAB-DEC-024`).
4. Lokasi bernomor pada cetakan PA (`LAB-EVD-005`) dibaca sebagai **daftar wadah** pada order itu, bukan
   daftar di dalam satu wadah.
5. `LAB-DEC-098` tidak berubah: satu wadah tetap boleh memuat lebih dari satu Spesifik Specimen. Spesifik
   Specimen menjawab *jaringan apa*; lokasi menjawab *diambil dari mana*.

**Contoh:**

> Order `LAB-001` biopsi saluran cerna memiliki tiga wadah: wadah 1 — Lambung, wadah 2 — Duodenum, wadah 3 —
> Ileum. Cetakan PA menulis *Lokasi Spesimen: 1. Lambung, 2. Duodenum, 3. Ileum* dari ketiga wadah itu.
> Petugas yang mencoba menyimpan *Lambung* dan *Duodenum* sekaligus pada wadah 1 ditolak, dengan pesan
> bahwa satu wadah hanya untuk satu lokasi.

### BR-117 — Fiksasi dicatat per wadah (`LAB-DEC-169`)

**Menutup `DEC-LAB-028`.**

**Aturan:**

1. Fiksasi dicatat **per wadah**, bila pemeriksaannya membutuhkan fiksasi.
2. Yang disimpan sekurang-kurangnya: **jenis cairan/metode fiksasi**, **tanggal dan jam mulai**, **petugas**
   yang melakukan atau mencatat, **wadah** terkait, dan **catatan** bila perlu.
3. Jenis fiksasi dipilih dari **data induk terkendali**; teks bebas bukan pilihan utama.
4. Bila specimen tidak membutuhkan fiksasi, sistem dapat mencatat **`Tidak Memerlukan Fiksasi`**.
5. Yang mencatat adalah **petugas yang melakukan fiksasi atau yang menerima informasinya** — misalnya
   perawat ruang tindakan saat pengambilan, atau petugas penerimaan laboratorium.
6. **Arah rancangan yang diturunkan, bukan keputusan baru:** pemeriksaan mana yang *membutuhkan* fiksasi
   disetel per kategori atau jenis pemeriksaan, sejalan pola kewajiban `LAB-DEC-161` butir 4. Isi daftar
   jenis fiksasi dan setelan kewajibannya adalah konten sebelum go-live (`LAB-OPEN-050`).

**Contoh:**

> Jaringan biopsi payudara dimasukkan ke wadah berisi *Formalin buffer 10%* pukul 09.20 oleh perawat kamar
> tindakan. Perawat mencatat jenis fiksasi dari daftar, jam mulai 09.20, dan namanya. Cetakan PA kelak
> menulis *Fiksasi: Formalin buffer 10%*. Pada swab tenggorok Mikrobiologi yang tidak membutuhkan fiksasi,
> petugas mencatat *Tidak Memerlukan Fiksasi* — bukan membiarkan ruasnya kosong tanpa arti.

### BR-118 — Seri nomor Sitologi dan FNAB berdiri sendiri (`LAB-DEC-170`)

**Menutup `DEC-LAB-022`. Mengamandemen `LAB-DEC-162` butir 3 dan `LAB-DEC-117`.**

**Aturan:**

1. **Sitologi** dan **FNAB** masing-masing memakai **seri nomor cetak sendiri**, terpisah dari seri
   Histopatologi/PA.
2. **Nomor order tetap satu identitas global** (`LAB-DEC-072`, `LAB-DEC-117`); yang berseri adalah **nomor
   cetak** laporan.
3. Awalan, pemisah, dan panjang nomor tiap seri **dapat disetel**, tidak tertanam di kode — melanjutkan pola
   `ReportNumberSeparator`/`ReportNumberLength` (`LAB-OPEN-043`).
4. Alokator nomor cetak PA kini berkunci **seri**, bukan disiplin. Patologi Klinik dan Mikrobiologi tetap
   satu seri per disiplin.
5. Penetapan kategori PA mana masuk seri mana — termasuk **IHK**, yang tidak disebut teks — adalah
   **setelan**, diisi sebelum go-live (`LAB-OPEN-049`).
6. **`LAB-OPEN-039` tidak lagi menahan seri ini.** Cetakan Sitologi, IHK, dan FNAB tetap dibutuhkan untuk
   **tata letak** cetak.
7. Bila cetakan lama kelak menunjukkan pola berbeda, **Kepala Instalasi** dapat memutuskan secara tertulis
   untuk mempertahankan pola lama. Karena bentuknya setelan, perubahan itu tidak menuntut perubahan kode.
8. **Peringatan `LAB-OPEN-043` tetap berlaku:** mengubah bentuk nomor di tengah tahun mengulang penghitung
   dari satu. Setelan karena itu **wajib final sebelum nomor Sitologi/FNAB pertama terbit**.

**Contoh:**

> Pada hari yang sama, biopsi payudara memperoleh nomor seri Histopatologi `26.0919`, sitologi urin
> memperoleh `S26.0041`, dan FNAB tiroid memperoleh `F26.0112`. Ketiganya tetap punya nomor order global
> masing-masing, misalnya `LAB-RSMMC-000000123`. Awalan `S` dan `F` hanya contoh; nilai sebenarnya disetel
> Kepala Instalasi.

### BR-119 — Hasil Mikrobiologi `Sementara` boleh dirilis; `Definitif` adalah tahap berikutnya (`LAB-DEC-171`)

**Menutup `DEC-LAB-020`. Melengkapi `LAB-DEC-114`; `LAB-DEC-106` utuh.**

**Aturan:**

1. Hasil Mikrobiologi berkualifikasi **`Sementara`** boleh **divalidasi dan dirilis** kepada pihak klinis,
   dengan empat mata yang sama seperti hasil lain (`LAB-DEC-003`, `LAB-DEC-120`).
2. Hasil itu **tampil dan tercetak jelas** sebagai **`HASIL SEMENTARA`**.
3. Ketika hasil **`Definitif`** tersedia, ia menjadi **tahap berikutnya dari pemeriksaan yang sama** —
   divalidasi dan dirilis lagi — dan **bukan** koreksi `S6`: tanpa alasan koreksi, tanpa persetujuan
   koreksi, dan versi `Sementara` **tidak** bertanda *sudah diperbaiki*.
4. Lintasan hasil: **`Draft` → `Sementara` → `Definitif`**. `S6` dipakai hanya bila hasil yang sudah terbit
   ternyata **salah**.
5. **Seluruh versi tetap tersimpan** dan terbaca pada riwayat; yang aktif adalah tahap terakhir yang
   dirilis.
6. **Hasil tanpa kualifikasi tetap dianggap bukan sementara** — arah `ARCH-GAP-LAB-10` dan `LAB-VAL-v1`
   `r10` 12.2 tidak diubah teks. Hasil itu sudah tahap akhir; perubahan sesudah rilisnya adalah koreksi
   `S6`.
7. Fakta konsultasi `LAB-DEC-106` tetap dicatat terpisah dari kualifikasi.

**Contoh:**

> Hari ke-1, pewarnaan Gram darah pasien sepsis menunjukkan kokus Gram positif. Analis menyimpan Final
> berkualifikasi `Sementara`, dokter Mikrobiologi memvalidasi, perilis lain merilisnya pukul 14.00. DPJP
> membaca *HASIL SEMENTARA — kokus Gram positif* dan memulai antibiotik empiris. Hari ke-3 biakan menumbuhkan
> *Staphylococcus aureus* MRSA. Analis menulis tahap `Definitif` pada pemeriksaan yang sama, lalu divalidasi
> dan dirilis lagi. Riwayat menampilkan dua versi — Sementara hari ke-1 dan Definitif hari ke-3 — dan **nol**
> alasan koreksi diminta.

### BR-120 — Order belum Selesai selama hasil Mikrobiologi masih `Sementara` (`LAB-DEC-172`)

**Klarifikasi Q5. Menajamkan `LAB-DEC-154`.**

**Aturan:**

1. Rilis `Sementara` menjadikan hasil **resmi** dan terbaca dokter (`LAB-DEC-155`), tetapi **tidak**
   menuntaskan pemeriksaannya.
2. Order baru dapat **Selesai** bila setiap pemeriksaan Mikrobiologinya sudah dirilis **`Definitif`** atau
   dirilis **tanpa kualifikasi**, di samping syarat `LAB-DEC-154` lainnya.
3. Karena gerbang penyerahan ke pasien terbuka saat order Selesai (`LAB-DEC-139`), pasien menerima dokumen
   final **sekali**, yaitu versi `Definitif`.

**Contoh:**

> Order berisi Darah Lengkap dan Kultur Darah. Darah Lengkap dirilis 09.00, Kultur dirilis `Sementara`
> 14.00. Order tetap *Dalam Proses* dan tombol Kirim ke Pasien belum aktif. Hari ke-3 Kultur dirilis
> `Definitif`; order menjadi *Selesai* dan dokumennya dapat dikirim.

### BR-121 — Dokter pemesan diberi tahu bila `Definitif` menyusul `Sementara` yang sudah dirilis (`LAB-DEC-173`)

**Klarifikasi Q6. Mengamandemen `LAB-DEC-163` di satu titik.**

**Aturan:**

1. Bila tahap `Definitif` dirilis atas pemeriksaan yang tahap `Sementara`-nya **sudah dirilis**, dokter
   pemesan menerima **satu pemberitahuan tersimpan** (pola `LAB-DEC-012`).
2. Rilis `Definitif` yang **tidak** didahului `Sementara` terrilis **tidak** memberi tahu siapa pun —
   `LAB-DEC-163` tetap berlaku di luar titik ini.
3. Alasannya sama dengan `LAB-DEC-007`: dokter mungkin sudah bertindak atas versi sebelumnya.
4. Sarananya kemampuan pemberitahuan platform yang memang sudah dibutuhkan `S5` dan `S6`
   (`LAB-COORD-017`); nol janji baru di luar itu.
5. Bila hasil `Definitif` memenuhi aturan kritis, alur `S5` berlaku **di samping** pemberitahuan ini.

**Contoh:**

> Lanjutan BR-119: begitu Definitif MRSA dirilis hari ke-3, dokter pemesan melihat di kotak
> pemberitahuannya *"Hasil definitif tersedia — Kultur Darah — pasien Andi"* dan mengganti antibiotik
> empiris sesuai kepekaan. Kultur urin pasien lain yang langsung dirilis Definitif tanpa tahap Sementara
> tidak menimbulkan pemberitahuan.

### BR-122 — Validasi PA oleh penulisnya; empat mata pada rilis (`LAB-DEC-174`)

**Menutup `DEC-LAB-021` — pilihan (c). Mengamandemen `LAB-DEC-003` bagi Patologi Anatomi saja.**

**Aturan:**

1. Dokter Sp.PA yang **menulis** laporan PA **boleh memvalidasi laporannya sendiri**. Ini **bukan** jalur
   pengecualian `LAB-DEC-003`, sehingga **tidak** diberi penanda *"divalidasi oleh pengisi sendiri"*.
2. **Rilis** dilakukan **dokter lain** yang memegang kewenangan rilis PA. Perilis ≠ pemvalidasi tetap
   ditegakkan (`LAB-DEC-120`); di PA artinya perilis ≠ penulis.
3. **Analis tidak pernah** menjadi pemvalidasi maupun perilis PA. Ini menyempitkan `LAB-DEC-153` (*perilis
   tidak wajib dokter*) **bagi PA saja**.
4. Patologi Klinik dan Mikrobiologi **tidak berubah**: pengisi tidak memvalidasi hasilnya sendiri.
5. **Akibat operasional yang wajib dibaca:** rilis PA membutuhkan **sekurang-kurangnya dua** Sp.PA
   berkewenangan. Bila hanya ada satu, laporan PA tidak dapat dirilis — sejalan syarat `LAB-DEC-152`
   (dua pemegang per disiplin sebelum disiplin dipakai nyata).
6. **Pemakaian nyata menunggu tanda tangan `DR-LAB-003`** (`LAB-SIGN-002`), sebab butir 1 melonggarkan
   aturan yang ia teken 2026-09-17. Desain dan pembangunan tidak menunggu.

**Contoh:**

> Sp.PA A menulis laporan biopsi payudara, menekan Final, lalu memvalidasinya sendiri pukul 11.00 — tanpa
> penanda pengecualian. Ia mencoba merilis dan ditolak, karena perilis harus orang lain. Sp.PA B yang
> berkewenangan rilis PA merilisnya pukul 13.00. Analis PA yang membuka laporan yang sama tidak melihat
> tombol Validasi maupun Rilis.

### BR-123 — Perubahan nilai kritis disetujui Kepala Instalasi; pengusul ≠ penyetuju (`LAB-DEC-175`)

**Menutup `DEC-LAB-012`, dengan klarifikasi Q1. Menajamkan `LAB-DEC-023` (BR-19).**

**Aturan:**

1. Penyetuju akhir perubahan **batas kritis** dan **penanda pilihan kritis** adalah **Kepala Instalasi
   Laboratorium**.
2. Kepala Instalasi dapat **mendelegasikan** kepada dokter penanggung jawab disiplin — Sp.PK bagi Patologi
   Klinik, dokter berkewenangan Mikrobiologi, Sp.PA bagi Patologi Anatomi — **hanya bila delegasinya
   tercatat resmi** di sistem: siapa, disiplin apa, sejak kapan.
3. **Pengusul ≠ penyetuju.** Sistem menolak penyetujuan oleh pengusulnya sendiri. Bila Kepala Instalasi
   yang mengusulkan, penyetujunya **dokter disiplin penerima delegasi**. Inilah yang menjaga contoh BR-19 —
   Kalium 6,0 → 8,0 — tetap tertangkap.
4. Setiap perubahan menyimpan: **nilai lama, nilai baru, alasan, pengusul, penyetuju, waktu persetujuan,
   tanggal mulai berlaku**, dan **riwayat versi**.
5. **Nilai baru tidak aktif sebelum disetujui**, dan baru berlaku pada **tanggal mulai berlakunya**. Sampai
   itu, nilai lama tetap dipakai menilai hasil.
6. Batas **normal** tidak berubah aturannya: Kepala Instalasi bebas mengubahnya dengan riwayat (BR-19).

**Contoh:**

> Kepala Instalasi mengusulkan batas kritis atas Kalium 6,0 → 6,5 mmol/L karena alat baru, berlaku 1
> November. Ia tidak dapat menyetujuinya sendiri; dokter Sp.PK yang tercatat menerima delegasi Patologi
> Klinik menyetujuinya 20 Oktober. Kalium 6,3 pada 25 Oktober **tetap** kritis karena nilai baru belum
> berlaku; pada 2 November, 6,3 tidak lagi kritis. Riwayat menyimpan 6,0 → 6,5, alasan *ganti alat*,
> pengusul, penyetuju, 20 Oktober, dan 1 November.

### BR-124 — Alur nilai kritis dan *Critical Notification SLA* (`LAB-DEC-176`)

**Menutup `LAB-P0-004` dan `LAB-OPEN-014` pada sisi ARSITEKTUR. Isi klinisnya konten sebelum go-live.**

**Aturan:**

1. Alur: **hasil pemeriksaan → aturan kritis terpicu → status kritis → pemberitahuan → acknowledgement →
   eskalasi bila belum ditanggapi**. `LAB-DEC-004` utuh: hasil kritis **tetap dirilis**.
2. **Penerima utama:** (a) **dokter pemesan**; dan (b) **DPJP/unit pelayanan** bila pasien masih dalam
   **episode perawatan aktif**. Pilihan Dokter Konfirmator `LAB-DEC-108`/`LAB-DEC-111` tetap menjadi cara
   memilih siapa yang dihubungi langsung untuk pelaporan lisan.
3. **Batas waktu tidak ditanam di kode.** Ia disimpan sebagai konfigurasi **`Critical Notification SLA`**
   yang dapat berbeda per disiplin atau per kebijakan rumah sakit.
4. **Arah rancangan yang diturunkan:** penerima tiap tingkat eskalasi ikut tersimpan pada konfigurasi yang
   sama, sebab teks hanya menyatakan *sistem melakukan eskalasi* tanpa menyebut kepada siapa. Isi tingkatnya
   konten sebelum go-live (`LAB-OPEN-047`).
5. Sistem **wajib menyimpan**: siapa yang dihubungi, waktu pengiriman, saluran, waktu acknowledgement, siapa
   yang melakukan acknowledgement, **setiap percobaan** pemberitahuan, eskalasi, dan hasil eskalasi.
6. **Mikrobiologi dan PA:** organisme, kondisi, atau nilai kritis dikelola sebagai **data induk aturan** oleh
   disiplinnya. Bagi Mikrobiologi bentuknya sudah `LAB-DEC-103`; isinya `LAB-OPEN-041`. **Bagi PA, arah
   rancangan yang diturunkan:** laporan PA naratif tidak dapat dicocokkan aturan secara otomatis, sehingga
   pemicunya tetap **`Status Hasil PA` = Kritis** yang diisi patolog (`LAB-DEC-094`), disertai **kondisi**
   yang dipilih dari daftar kondisi kritis PA. Isi daftarnya `LAB-OPEN-046`.
7. Pesan WhatsApp tetap **tanpa data klinis** (`LAB-DEC-137`) dan tetap **saluran pengantar**, bukan bukti
   pelaporan (`LAB-DEC-136`).
8. Pemberitahuan memakai kemampuan platform (`LAB-DEC-016`, `LAB-COORD-017`) — dependency, bukan keputusan
   Laboratorium.

**Catatan untuk arsitektur domain — dokter perujuk luar.** Pasien rujukan luar (`S13b`) tidak memiliki
episode aktif, dan dokter perujuknya umumnya tidak punya akun. Pemberitahuan tersimpan tidak sampai
kepadanya; pelaporan lisan `LAB-DEC-004` tetap berjalan dan eskalasi mengikuti SLA. Cara menanganinya
diteruskan ke arsitektur domain, bukan diputuskan di sini.

**Contoh:**

> Kalium pasien Andi (rawat inap, episode aktif) 7,2 mmol/L dirilis 23.44. Aturan kritis terpicu;
> pemberitahuan tersimpan dibuat bagi dokter pemesan dan DPJP, dan WhatsApp tanpa data klinis terkirim
> 23.45. SLA Patologi Klinik disetel 15 menit. Pukul 23.59 belum ada laporan baca ulang, maka sistem mencatat
> eskalasi ke tingkat berikutnya pada konfigurasi, misalnya dokter jaga. Pukul 00.05 analis menelepon DPJP,
> DPJP mengulang *"Kalium tujuh koma dua"*, dan analis mencatat kelima isian `LAB-DEC-004`. Eskalasi
> berhenti; hasilnya tercatat *ditanggapi DPJP 00.05*.

### BR-125 — *Acknowledgement* adalah laporan baca ulang `LAB-DEC-004` (`LAB-DEC-177`)

**Klarifikasi Q2. Menegakkan `LAB-DEC-004`, `LAB-DEC-136`, dan `LAB-DEC-151` tanpa mengubah bunyinya.**

**Aturan:**

1. *Acknowledgement* pada alur BR-124 **adalah** laporan pelaporan kritis yang **lengkap** — pelapor,
   penerima, waktu, saluran, dan **bukti pembacaan ulang** lewat percakapan langsung.
2. **Eskalasi berhenti hanya** ketika laporan itu tercatat lengkap.
3. Status *sudah dibaca* pada kotak pemberitahuan (`LAB-DEC-012`) dan balasan WhatsApp (`LAB-DEC-151`)
   **dicatat sebagai bukti komunikasi**, tetapi **tidak** menghentikan eskalasi dan **tidak** menuntaskan
   pelaporan.
4. Karena aturan yang diteken dokter tidak dilonggarkan, keputusan ini **tidak** menuntut tanda tangan
   ulang `DR-LAB-001`..`003`.

**Contoh:** DPJP membuka pemberitahuan pukul 23.50 dan membalas WhatsApp *"ok"*. Keduanya tercatat, tetapi
pukul 23.59 eskalasi tetap berjalan karena belum ada baca ulang. Eskalasi baru berhenti pukul 00.05 saat
baca ulang lewat telepon dicatat.

### BR-126 — Koreksi sesudah rilis: dua orang, versi bernomor, tanpa batas waktu (`LAB-DEC-178`)

**Menutup `DEC-LAB-014` dan sisa `LAB-P0-003`. Menajamkan `LAB-DEC-007`; `LAB-DEC-082` dan `LAB-DEC-020` utuh.**

**Aturan:**

1. Hasil yang sudah **dirilis** dapat dikoreksi.
2. **Pengaju** koreksi: petugas atau dokter yang memegang **kewenangan validasi** pada disiplin itu.
3. **Penyetuju** koreksi: **pengguna lain** yang memegang **kewenangan rilis** pada disiplin itu. Tidak harus
   Kepala Instalasi, tetapi **satu orang tidak boleh menjalankan seluruh proses sendiri**.
4. Alur: **hasil terrilis → pengajuan koreksi → alasan wajib (daftar terkendali `LAB-DEC-082`) → persetujuan
   orang kedua → versi koreksi → rilis versi koreksi**. Versi koreksi dirilis oleh penyetujunya, sehingga
   empat mata terpenuhi tanpa orang ketiga.
5. **Yang diberi tahu:** **dokter pemesan** dan **DPJP/unit pelayanan aktif** — memperluas `LAB-DEC-007`
   butir 2 yang hanya menyebut dokter pemesan.
6. Versi lama diberi status **`Superseded`** dan tetap terlihat bertanda *sudah diperbaiki* (`LAB-DEC-007`
   butir 3); versi koreksi menjadi **versi aktif**.
7. **Tidak ada batas waktu** yang membuat hasil lama tidak dapat dikoreksi. Penggantinya: alasan wajib,
   persetujuan, versi bernomor, dan jejak audit yang tidak dapat diubah.
8. Bila kunjungan sudah ditutup, mekanisme addendum `LAB-DEC-020` tetap berlaku.

**Contoh:**

> Hemoglobin pasien Andi dirilis 9,4 g/dL. Pukul 14.00 Tono (berkewenangan validasi Patologi Klinik)
> mengajukan koreksi menjadi 4,9 dengan alasan *salah ketik*. Tono mencoba menyetujui sendiri dan ditolak.
> Wati (berkewenangan rilis Patologi Klinik) menyetujui dan merilis versi 2 pukul 14.10. Versi 1 menjadi
> `Superseded` bertanda *sudah diperbaiki*; dokter pemesan dan DPJP rawat inap menerima pemberitahuan.
> Kesalahan serupa yang baru ketahuan tiga bulan kemudian tetap dapat dikoreksi dengan cara yang sama.

### BR-127 — Hasil terrilis tidak dibatalkan; salah pasien ditarik lewat koreksi (`LAB-DEC-179`)

**Klarifikasi Q3. Menutup `DEC-LAB-019`. Mengukuhkan arah `ARCH-GAP-LAB-09` dan `LAB-DEC-138`.**

**Aturan:**

1. Pemeriksaan yang hasilnya **sudah dirilis tidak dapat dibatalkan**. `LAB-DEC-049` (pembatalan sesudah
   kelayakan) berlaku **hanya sampai rilis**.
2. `S6` menyediakan **dua jenis koreksi**: **ganti nilai**, dan **tarik hasil** — untuk hasil yang ternyata
   bukan milik pasien itu (tabung tertukar, salah label).
3. Keduanya menempuh alur BR-126: alasan wajib, persetujuan orang kedua, dan pemberitahuan.
4. Pada **tarik hasil**, versi lama bertanda **`Invalidated`** dan tetap terlihat di riwayat; tidak ada versi
   pengganti bernilai.
5. Dampak tagihan dari penarikan **bukan** keputusan putaran ini; fakta kelayakan tagih (`S10`) diperiksa
   saat desain.

**Contoh:**

> Kalium pasien Andi 6,4 dirilis 10.00. Pukul 11.00 ketahuan tabungnya milik pasien Budi. Petugas mengajukan
> **tarik hasil** dengan alasan *sampel tertukar*; perilis lain menyetujuinya. Rekam medis Andi menampilkan
> Kalium 6,4 bercoret bertanda *ditarik — sampel tertukar*, bukan hilang tanpa jejak. Kalium Andi lalu
> diperiksa ulang dengan sampel baru.

### BR-128 — Versi koreksi masuk antrean kirim pasien (`LAB-DEC-180`)

**Klarifikasi Q4. Menghubungkan BR-126 dengan `S17`.**

**Aturan:**

1. Bila versi yang dikoreksi atau ditarik **pernah dikirim ke pasien**, dokumen lama diberi status
   **`Superseded`** (atau `Invalidated` pada tarik hasil) dan **token verifikasi QR-nya dicabut** (BR-132).
2. Saat versi koreksi dirilis, sistem **otomatis membuat tugas kirim berstatus `Pending`** bagi pasien itu.
3. Pengiriman tetap dilakukan petugas lewat **gerbang `S17`** — tidak terkirim otomatis.
4. Pada **tarik hasil** tidak ada dokumen pengganti bernilai; bentuk pesan kepada pasien diputuskan saat
   desain `S17`.

**Contoh:** Andi sudah menerima PDF Hemoglobin 9,4 lewat WhatsApp. Koreksi 4,9 dirilis 14.10, sehingga
daftar kirim memuat tugas *Pending — Andi — versi 2*. Pukul 14.30 petugas mengirimnya. Bila Andi memindai QR
pada PDF lama, halaman verifikasi menyatakan *sudah digantikan*.

### BR-129 — Delapan laporan `S16b` ditunda ke Phase 2 (`LAB-DEC-181`)

**Mengeluarkan `DEC-LAB-025` dari daftar penahan roadmap.**

**Aturan:**

1. Tiga laporan `S16a` (`LAB-DEC-159`, `LAB-DEC-160`) tetap pada roadmap berjalan.
2. Delapan laporan `S16b` menjadi **`PHASE 2 — DEFERRED PENDING REPORT CATALOG`**. **Tidak ada laporan yang
   dikarang.**
3. Satu laporan baru boleh menjadi task BE/FE hanya sesudah pemilik modul memberikan: **nama, tujuan,
   sumber data, definisi setiap kolom, rumus, periode, titik mulai/akhir, perlakuan pembatalan, penyaring,
   peran pembaca, dan format ekspor**.
4. `S16b` **tidak lagi menahan** roadmap Laboratorium lain.

**Contoh:** Kepala Instalasi meminta *laporan pemeriksaan per penjamin*. Sampai kesebelas butir definisinya
diserahkan, laporan itu tidak masuk roadmap; sesudahnya ia ditambahkan sebagai task baru tanpa mengganggu
task yang sedang berjalan.

### BR-130 — Persetujuan kirim ke pasien berbasis peran, bukan gelar (`LAB-DEC-182`)

**Menutup `LAB-OPEN-029`. Mengamandemen `LAB-DEC-067` dan `LAB-DEC-139` pada syarat persetujuan *Profesor dan Dokter Lab*.**

**Aturan:**

1. **Gelar "Profesor" tidak menjadi izin sistem.** Penggantinya izin berbasis peran atau kewenangan klinis,
   secara konsep **`LabResultPatientDeliveryApprover`**, yang diberikan kepada orang **yang ditunjuk Kepala
   Instalasi**.
2. Hasil **normal** yang sudah **Released** **tidak** memerlukan persetujuan tambahan. Alurnya: **Validated →
   Released → Eligible for Patient Delivery → Send**.
3. Bagi **kategori pemeriksaan sensitif**, kebutuhan **persetujuan kedua** dapat **disetel**. Daftar
   kategorinya konten sebelum go-live (`LAB-OPEN-048`).
4. Seluruh tindakan pengiriman **tercatat** pada jejak audit.
5. Bagian lain `LAB-DEC-139` **utuh**: satu gerbang untuk kirim, cetak untuk pasien, dan unduh; order harus
   Selesai.
6. `LAB-DEC-121` mencatat butir ini **bukan wewenang pemilik modul sendirian**. Karena itu penetapan *hasil
   normal tanpa persetujuan tambahan* dicatat untuk **dikonfirmasi Kepala Instalasi** sebelum go-live
   (`LAB-SIGN-002`).

**Contoh:**

> Order Darah Lengkap pasien Andi Selesai pukul 10.00 dan langsung *Eligible for Patient Delivery*; petugas
> mengirimnya tanpa menunggu siapa pun. Bila Kepala Instalasi menyetel kategori *HIV* sebagai sensitif,
> hasil HIV pasien Budi baru dapat dikirim sesudah pemegang izin `LabResultPatientDeliveryApprover`
> menyetujuinya, dan tombol Kirim menyebut syarat yang belum terpenuhi.

### BR-131 — Dokumen hasil PDF dan pengiriman WhatsApp (`LAB-DEC-183`)

**Menutup sisi arsitektur `LAB-COORD-011`. Penyediaan pembangkit PDF dan gerbang WhatsApp tetap milik platform.**

**Aturan:**

1. Hasil **Released** menghasilkan **dokumen PDF** yang memuat: **nomor dokumen, versi, waktu terbit,
   perilis, QR verifikasi**, dan **tautan ke order/hasil**.
2. Pengiriman WhatsApp memakai **layanan pemberitahuan platform yang terpisah**; Laboratorium tidak
   membangun gerbangnya sendiri.
3. Yang dikirim ke pasien adalah **tautan aman** menuju hasil, **bukan** URL berkas publik permanen. Model
   tautan aman — masa berlaku, sekali pakai atau tidak, verifikasi identitas tambahan — **menunggu
   persetujuan pemilik keamanan/platform** bersama `LAB-COORD-015`.
4. PDF tetap dapat diunduh dari Quilvian **sesuai hak akses**.
5. **Kegagalan WhatsApp tidak membatalkan status Released.** Status pengiriman berdiri sendiri: **`Pending` →
   `Sent` → `Delivered` / `Failed`**.
6. Kontrak FE/BE `S17` boleh dirancang sementara adapter WhatsApp dikerjakan platform.

**Contoh:** PDF hasil Andi bernomor cetak `26000123`, versi 1, terbit 10.05, perilis Wati. WhatsApp ke Andi
gagal karena nomornya tidak aktif, sehingga status kirim `Failed` sementara hasil tetap `Released`. Petugas
mengirim ulang sesudah nomornya diperbaiki.

### BR-132 — Verifikasi QR tanpa login memakai token yang dapat dicabut (`LAB-DEC-184`)

**Melengkapi `LAB-DEC-140`. `LAB-COORD-015` menyempit menjadi persetujuan model ini oleh pemilik keamanan/platform.**

**Aturan:**

1. Halaman verifikasi QR **boleh tanpa login**.
2. QR **tidak** membawa Patient ID, Order ID berurutan, maupun URL berkas publik — hanya **token
   opaque/bertanda tangan**.
3. Halaman publik menampilkan **informasi minimum**: status dokumen (berlaku / tidak berlaku / digantikan),
   nomor hasil, tanggal terbit, dan fasilitas penerbit.
4. **Nama pemeriksaan tetap tidak ditampilkan** (`LAB-DEC-140`) sampai kebijakan privasi menyetujuinya.
   Teks membuka kemungkinan itu, tetapi tidak memberikannya.
5. **Tidak ada pencarian pasien** dari halaman publik.
6. Token **dapat dicabut** ketika dokumen di-*supersede* atau dikoreksi.
7. **Bagian verifikasi publik baru menjadi task roadmap sesudah pemilik keamanan/platform menyetujui model
   ini**, sesuai teks butir 12.

**Contoh:** QR pada PDF lama Andi berisi token acak, bukan `LAB-RSMMC-000000123`. Sesudah koreksi BR-128,
token itu dicabut; pemindai melihat *Dokumen ini sudah digantikan* tanpa nama Andi maupun nama
pemeriksaannya.

### BR-133 — Berkas klinis tinggal di penyimpanan privat (`LAB-DEC-185`)

**Menutup `DEC-LAB-016` sebagai kebijakan. Penyediaan layanannya dibuka sebagai `LAB-COORD-019`.**

**Aturan:**

1. Berkas klinis — gambar PA dan PDF hasil eksternal — **tidak** disimpan sebagai berkas publik di web
   server.
2. Berkas tinggal di **penyimpanan berkas privat**; basis data menyimpan **metadata dan relasinya**.
3. Akses hanya lewat **endpoint terautentikasi** atau **tautan bertanda tangan sementara** yang kedaluwarsa.
4. Wajib ada: **izin akses, audit akses, validasi ukuran dan jenis berkas**, serta relasi ke **pasien,
   kunjungan, order, pemeriksaan, pengunggah, waktu unggah, dan versi/status**.
5. Berkas lama **tidak dihapus** saat koreksi; ia bertanda **`Superseded`/`Invalidated`**, dan berkas yang
   benar menjadi versi aktif.
6. **Fakta 2026-10-06:** backend **belum punya** layanan penyimpanan privat bersama, dan Laboratorium tidak
   berwenang mengadakannya sendiri — `LAB-COORD-019`.

**Contoh:** Patolog mengunggah foto mikroskopik JPEG 3,2 MB ke laporan PA. Berkas masuk penyimpanan privat;
basis data mencatat pasien, order, pemeriksaan, pengunggah, pukul 11.05, dan versi 1. Analis yang membuka
laporan melihat gambar lewat endpoint terautentikasi, dan aksesnya tercatat. Berkas `.exe` atau berkas
berukuran di atas batas ditolak. Mengganti gambar yang salah membuat versi 2 aktif dan versi 1 bertanda
`Superseded`.

### BR-134 — Arti `Verified` dan `Approved` pada dokumen lab eksternal (`LAB-DEC-186`)

**Menjawab butir (3) `LAB-COORD-018` dari sisi Laboratorium. Konfirmasi pemilik `clinical-management` tetap dibutuhkan.**

**Aturan:**

1. **`Verified`** = petugas memastikan dokumen **terbaca**, **milik pasien itu**, **terkait
   pemeriksaan/order/kunjungan yang dimaksud**, dan **berasal dari laboratorium eksternal yang dicatat**.
   `Verified` **bukan** persetujuan klinis atas isi hasil.
2. **`Approved`** = dokumen telah **ditinjau dan diterima secara klinis** oleh tenaga medis berwenang.
3. Lintasan: **`Uploaded` → `Verified` → `Approved`**.
4. **Laboratorium** bertanggung jawab atas **unggah dan verifikasi**; **Clinical Management/dokter
   berwenang** atas **penerimaan klinis**, bila memang dibutuhkan.
5. Pencocokan dua identitas saat unggah (`LAB-DEC-158`) adalah bagian dari verifikasi.
6. `LAB-CONFLICT-015` dan `LAB-CONFLICT-016` — jalan keluar berkas salah pasien dan satu kode izin
   `Update` — **tetap** bahan `LAB-COORD-018`.

**Contoh:** Petugas lab mengunggah PDF HbA1c dari klinik lain, mencocokkan nama dan tanggal lahir, lalu
menandainya `Verified`. Dokter poli penyakit dalam membacanya dan menandainya `Approved`. Petugas lab sendiri
tidak melihat tombol `Approved`.

### BR-135 — Penahan roadmap dipisahkan dari penahan go-live (`LAB-DEC-187`)

**Aturan:**

1. Butir berikut **tidak lagi** disebut sebagai alasan roadmap tidak dapat disusun:

   | ID | Keadaan |
   |---|---|
   | `DEC-LAB-011`, `DEC-LAB-018`, `DEC-LAB-023`, `DEC-LAB-024`, `DEC-LAB-026`, `DEC-LAB-027` | Sudah ditutup (`LAB-DEC-150`..`153`, `LAB-DEC-157`/`158`, `161`, `163`, `164`) |
   | `LAB-CONFLICT-014` | Sudah diputuskan (`LAB-DEC-154`) |
   | `LAB-OPEN-025`, `LAB-OPEN-026` | Bukan penahan kunjungan kiosk; keduanya tercatat pada [`approval-requests/2026-09-17-permintaan-kunjungan-dari-kiosk.md`](approval-requests/2026-09-17-permintaan-kunjungan-dari-kiosk.md) |

2. Butir berikut **tetap dipantau** sebagai **penahan rilis/go-live**, bukan penahan roadmap teknis:

   | ID | Yang ditahan |
   |---|---|
   | `DEC-LAB-017` | Pemakaian nyata `S4` sebelum `S5` berdiri |
   | `LAB-OPEN-044`, `UNK-P14-03` | Daftar jabatan calon perilis dan pemberian aksi rilis |
   | `LAB-OPEN-045` | Prosedur bila hasil terrilis keliru sebelum `S6` berdiri |
   | Validator kedua (`LAB-DEC-152`) | Dua pemegang validasi per disiplin; bagi PA juga dua Sp.PA agar rilis BR-122 mungkin |
   | Kewenangan HR (`LAB-COORD-016`) | Kode kewenangan Laboratorium pada katalog kredensial |
   | `LAB-SIGN-002` | Ratifikasi klinis keputusan putaran ini |
   | `LAB-OPEN-041`, `LAB-OPEN-046`..`LAB-OPEN-050` | Konten konfigurasi klinis dan operasional |

3. Dependency **penyediaan** milik platform — `LAB-COORD-011` (PDF, WhatsApp), `LAB-COORD-017`
   (pemberitahuan), `LAB-COORD-019` (penyimpanan privat) — **menahan implementasi** bagian yang memakainya,
   bukan penyusunan roadmap. Task yang memakainya ditulis dengan dependency eksplisit.
4. `LAB-COORD-015` menahan **task** verifikasi publik dan tautan aman sampai modelnya disetujui (BR-131
   butir 3, BR-132 butir 7).

**Contoh:** task backend nilai kritis `S5` boleh ditulis sekarang dengan dependency `LAB-COORD-017`. Ia baru
dapat dikerjakan ketika pemberitahuan platform tersedia, dan baru dipakai nyata ketika isi SLA
(`LAB-OPEN-047`) dan ratifikasi `LAB-SIGN-002` ada — tetapi tidak satu pun dari ketiganya menghalangi
task itu dituliskan.

### Kedudukan klinis keputusan putaran ini — `LAB-SIGN-002`

Pemilik modul menyatakan sejak `LAB-DEC-011` bahwa **wewenang klinis berada di pihak lain**, dan `LAB-DEC-079`
menetapkan amandemen aturan per disiplin diteken pemegang disiplinnya. Maka keputusan putaran ini **sah untuk
desain dan roadmap**, tetapi sebagian membutuhkan pernyataan klinis **sebelum go-live**:

| Keputusan | Kenapa perlu pernyataan klinis | Penanda tangan | Bentuk |
|---|---|---|---|
| `LAB-DEC-174` validasi PA oleh penulis | **Melonggarkan** `LAB-DEC-003` yang diteken bagi PA | `DR-LAB-003` dr. Citra Maharani, Sp.PA | **Wajib.** Tanpa tanda tangan, PA tidak dipakai nyata dengan aturan ini |
| `LAB-DEC-171`..`173` hasil `Sementara` | Pertanyaan asal `DEC-LAB-020` ditujukan kepada pemegang Mikrobiologi | `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK | Konfirmasi |
| `LAB-DEC-175` penyetuju nilai kritis | Menetapkan peran pada Kepala Instalasi; `DEC-LAB-012` milik pihak klinis | dr. Bima Prasetya, Sp.PK, Kepala Instalasi | Konfirmasi menerima peran dan bentuk delegasinya |
| `LAB-DEC-176`, `177` alur nilai kritis | Ambang, SLA, dan eskalasi menyangkut keselamatan pasien | `DR-LAB-001`, `DR-LAB-002`, `DR-LAB-003` + Clinical Governance | Konfirmasi, bersama isi `LAB-OPEN-041`, `046`, `047` |
| `LAB-DEC-178`..`180` koreksi | Pertanyaan asal `DEC-LAB-014` ditujukan kepada ketiganya. Aturannya **lebih ketat** dari `LAB-DEC-007`, sehingga tidak melonggarkan apa pun | `DR-LAB-001`, `DR-LAB-002`, `DR-LAB-003` | Konfirmasi |
| `LAB-DEC-182` hasil normal tanpa persetujuan tambahan | `LAB-DEC-121`: bukan wewenang pemilik modul sendirian | Kepala Instalasi | Konfirmasi, bersama isi `LAB-OPEN-048` |
| `LAB-DEC-168`..`170` atribut wadah PA | Pemilik asal `DEC-LAB-022`/`028`/`029` termasuk `DR-LAB-003` | `DR-LAB-003` | Konfirmasi |

**Bila salah satu ditolak**, keputusannya dibuka kembali lewat amendment pass, dan task yang bergantung
padanya ditahan — **bukan** dipakai dengan tafsiran sendiri.

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| — | 14 butir teks `LAB-EVD-012` | Diterima tertulis | ✅ `LAB-DEC-168`..`LAB-DEC-187` |
| Q1 | `DEC-LAB-012` | Pengusul sekaligus penyetuju? | ✅ *Pengusul ≠ penyetuju* — `LAB-DEC-175` |
| Q2 | `LAB-P0-004` | Arti acknowledgement | ✅ *= laporan baca ulang* — `LAB-DEC-177` |
| Q3 | `DEC-LAB-019` | Batal atau koreksi sesudah rilis | ✅ *Selalu lewat koreksi* — `LAB-DEC-179` |
| Q4 | `DEC-LAB-014` | Versi koreksi ke pasien | ✅ *Antrean kirim otomatis* — `LAB-DEC-180` |
| Q5 | `DEC-LAB-020` | Order Selesai saat `Sementara` | ✅ *Belum Selesai* — `LAB-DEC-172` |
| Q6 | `DEC-LAB-020` | Pemberitahuan saat `Definitif` | ✅ *Ya, bila Sementara sudah dirilis* — `LAB-DEC-173` |

> **Hasil putaran:** `DEC-LAB-012`, `DEC-LAB-014`, `DEC-LAB-016`, `DEC-LAB-019`, `DEC-LAB-020`, `DEC-LAB-021`,
> `DEC-LAB-022`, `DEC-LAB-028`, `DEC-LAB-029`, `LAB-P0-003`, `LAB-P0-004`, `LAB-OPEN-014`, dan `LAB-OPEN-029`
> **tertutup**; `DEC-LAB-025` **ditunda ke Phase 2**. `LAB-COORD-011`, `LAB-COORD-015`, dan `LAB-COORD-018`
> **menyempit** ke penyediaan atau persetujuan pemilik lain. Keputusan terkunci yang **diamandemen**:
> `LAB-DEC-003` (PA saja), `LAB-DEC-067`/`139` (syarat bergelar), `LAB-DEC-117`/`162` (seri nomor), dan
> `LAB-DEC-163` (satu titik); yang **dipertajam**: `LAB-DEC-007`, `023`, `153` (PA), dan `154`. Dibuka satu
> ratifikasi klinis `LAB-SIGN-002`, satu koordinasi `LAB-COORD-019`, dan lima konten go-live
> `LAB-OPEN-046`..`050`. `AC-257`..`AC-275` ditambahkan. **Kesiapan `S2b-1`, `S2b-2`, `S2b-3`, `S4d-2`, `S4e`,
> `S5`, `S6`, `S17`, dan `S18` perlu dinilai ulang gerbang** — bukan diputuskan di sini.

---

## Amendment Pass Putaran 22 — Menu dan aksi alur kerja FE v1 pada daftar pasien lab (2026-10-06)

**Sumber:** permintaan langsung pemilik modul pada sesi 2026-10-06 — *menu dan alur proses Laboratorium
FE v1 diterapkan ke V2, dengan tampilan tetap memakai tema UI V2*. FE v1 adalah repository terpisah
`C:\Users\Admin\QuilvianSystemFrontendDev` di luar workspace V2, dibaca baca-saja: menu
`src/utils/menuSidebar/menuItems.jsx` baris 786–1088 dan daftar
`src/components/view/laboratorium/daftar-pasien-lab-patologi-klinik/DaftarPasienLabPatologiKlinik.jsx`.
Lima pertanyaan pilihan pada sesi yang sama; **kelimanya dijawab dengan pilihan yang direkomendasikan**.
Fakta kode dibaca pada backend `171dc314` dan frontend `5427ddbe4`.

**FE v1 bukan evidence tingkat artifact.** Ia diperlakukan sebagai **rujukan alur yang ditunjuk pemilik
modul**, tidak diberi nomor `LAB-EVD-`, dan isinya tidak disalin ke `evidence/`. Pemetaan v1 ke V2 di bawah
adalah bacaan source, bukan keputusan.

**Capability map revision 6 (2026-09-25) tidak dipindai ulang.** Putaran ini bersandar pada fakta kode yang
diperiksa langsung 2026-10-06 (tabel di bawah); verifikasi rincinya diteruskan ke
`/trace-existing-capabilities` (bagian *Bahan audit*).

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F22-1 | 27 berkas halaman Lab v1 hanya berisi *"Fitur dalam tahap pengembangan"*: seluruh 10 butir menu Laporan (Laporan Order Lab berupa tab di atas tiga sub-halaman placeholder), Parameter ×3, Tarif ×2, Rujukan ×4, Nilai Rujukan Mikrobiologi ×4, Template Hasil, dan Pembatalan. Alur v1 yang hidup: Beranda, Penerimaan Sampling atau Specimen, Daftar Pasien OTC, Daftar Pasien Lab PK/PA/Mikro, Hasil dan Riwayat PK/PA/Mikro | v1 `src/components/view/laboratorium/**` |
| F22-2 | Aksi baris daftar v1: Konfirmasi, Terima Sampling, Proses Pemeriksaan, Batalkan Pemeriksaan, Print. Terima Sampling dan Proses Pemeriksaan **nonaktif bila pembayaran belum Lunas**; Konfirmasi tetap aktif. Status v1: Menunggu → Dalam Proses (sesudah konfirmasi) → Sedang Pemeriksaan → Selesai/Batal; *Selesai* **diturunkan dari keberadaan hasil**, bukan tindakan petugas | `DaftarPasienLabPatologiKlinik.jsx` ±1340–1470 dan ±3455–3520 |
| F22-3 | Aksi baris tiga daftar Pemeriksaan V2: Buka Hasil, Konfirmasi, Batalkan, Nota, Label Lab, Label Goldar. Penyaring Kategori Periode (Tanggal Order/Sampling/Pemeriksaan), Jenis Kunjungan, Kamar, dan Penjamin **sudah sama** dengan v1 | `lab-monitoring-table-columns.jsx`; `lab-monitoring-constants.jsx` |
| F22-4 | `PUT /lab-orders/{id}/start-process` (`Accepted` → `InProcess`) dan `PUT /{id}/complete` memakai `LabOrder : Process`; `PUT /{id}/hold` (alasan wajib) dan `resume` memakai `LabOrder : Hold`. **Nol pemanggil di frontend** untuk keempatnya | `LabOrderController.cs:380-460`; pencarian frontend nol hasil |
| F22-5 | Backend **tidak** memeriksa status pembayaran pada `start-process`. Baris daftar membawa `paymentType` (Tunai/Asuransi/Perusahaan), **tidak** membawa status lunas | `LabOrderService.StartProcessAsync`; `LabMonitoringItemResponse` |
| F22-6 | `resultProgress` dikirim bagi Patologi Klinik dan Mikrobiologi (`LabReleasableDisciplines`), **kosong** bagi Patologi Anatomi sampai `S4e`. Layar baru menampilkan kolom Keadaan Order pada daftar Mikrobiologi | `LabMonitoringService.cs:147-164`; `lab-monitoring-view.jsx:170` |
| F22-7 | Nol endpoint satu klik untuk penerimaan sampel. Langkah wadah terpisah: rencana (`POST /lab-specimens/by-order/{labOrderId}`, jenis specimen wajib — `VAL-51`), `collect`, `receive`, `accept`; kelayakan tagih terbentuk saat `accept`. Pesanan dari dokter belum punya wadah sampai direncanakan di layar Wadah | `LabSpecimenController.cs`; `LAB-DEC-057` |
| F22-8 | Konfirmasi pada pesanan `Accepted` ditolak `409` — urutan v1 *Terima Sampling lalu Konfirmasi* tidak dapat dijalankan di V2 tanpa amandemen `LAB-STATE-v1` | `contracts/state-transition-matrix.md` bagian 1a |

### Batas scope putaran ini — `LAB-DEC-188`

| Di dalam scope (cukup frontend, backend sudah ada) | Di luar scope — ditunda |
|---|---|
| Aksi **Proses Pemeriksaan** dan **Selesaikan** pada daftar pasien lab | Urutan v1 *Terima Sampling sebelum Konfirmasi* — menuntut amandemen `LAB-STATE-v1` (F22-8) |
| Aksi **Terima Sampling** per baris | Opsi *langsung proses* pada pop-up Konfirmasi — `start-process` mensyaratkan `Accepted` |
| Label, urutan, dan pengelompokan menu Laboratorium mengikuti kosakata v1 | Kunci tombol sampai `Lunas` — tetap menunggu `LAB-REQ-008`/`LAB-COORD-010` |
| | Menu Hasil dan Riwayat, kirim hasil ke pasien lewat WhatsApp, penghitung *Terkirim ke Pasien* — `S17` |
| | Daftar Pasien OTC dan grafik Beranda versi v1; Tahan/Lanjutkan pesanan |
| | 27 halaman placeholder v1 — tidak ada perilaku untuk diterapkan (F22-1) |

### Keputusan diuji terhadap blueprint

| Pilihan pemilik | Bersinggungan dengan | Penyelesaian |
|---|---|---|
| Q1 — Proses Pemeriksaan aktif bagi semua penjamin | `LAB-DEC-062` (`approved` 2026-09-15): kunci Proses bagi Mandiri/tunai sampai `Lunas` | **Diamandemen, bukan dicabut**: kunci ditangguhkan sebagai pengecualian sementara — `LAB-DEC-189` |
| Q2, Q3 — Selesaikan hanya bila semua dirilis | `LAB-DEC-154`: Selesai adalah tindakan manual yang diperiksa sistem | Sejalan; layar mempersempit, backend tetap pengaman — `LAB-DEC-190` |
| Q4 — Terima Sampling membuka layar Wadah | `LAB-DEC-060` (`approved` 2026-09-15): jalan pintas **satu klik** | **Diamandemen**: satu klik diganti satu navigasi sampai ada endpoint atomik — `LAB-DEC-191` |
| Q5 — Label *Daftar Pasien Lab …* | `LAB-DEC-048` (2026-09-14): tiga menu Monitoring dinamai *Pemeriksaan* | **Diamandemen pada labelnya saja**; tiga menu terpisah, penyaring identik, dan jalan ke detail pesanan (`LAB-FE-012`) tetap — `LAB-DEC-192` |

### BR-136 — Aksi alur kerja pada daftar pasien lab (`LAB-DEC-189`..`LAB-DEC-191`)

**Aturan:**

1. Ketiga daftar pasien lab menawarkan aksi **Proses Pemeriksaan** yang memanggil
   `PUT /lab-orders/{id}/start-process`. Aksi hanya sah bila status pesanan `Accepted`; pemegang izinnya
   `LabOrder : Process`.
2. Selama `LAB-COORD-010` belum terjawab, Proses Pemeriksaan **tidak** dikunci status pembayaran untuk
   penjamin mana pun. Kepastian pembayaran pasien Tunai sebelum diproses dipegang petugas secara manual
   (`LAB-OPEN-051`). Ketika jalur baca Billing tersedia, kunci `LAB-DEC-062` dipasang tanpa keputusan baru.
3. Daftar Patologi Klinik dan Mikrobiologi menawarkan aksi **Selesaikan** yang memanggil
   `PUT /lab-orders/{id}/complete`, dan aksi itu dapat ditekan hanya bila status pesanan `InProcess`
   **dan** `resultProgress` = `AllReleased`. Daftar Patologi Anatomi **tidak** menampilkannya sampai `S4e`
   berdiri.
4. Penolakan `409` dari `complete` (`VAL-146`) ditampilkan beserta pemeriksaan yang menahan. Layar tidak
   mengubah status baris tanpa jawaban `200`, baik untuk Proses Pemeriksaan maupun Selesaikan.
5. Aksi **Terima Sampling** membuka layar Wadah pesanan baris tersebut. Daftar **tidak** memanggil satu pun
   endpoint wadah; siklus `Planned` → `Collected` → `Received` → `Accepted`/`Rejected` tetap dijalankan di
   layar Wadah, termasuk `LAB-FE-009` dan `LAB-FE-010`.
6. Tahan/Lanjutkan pesanan **tidak** ditawarkan. Endpoint `hold`/`resume` tetap ada tanpa pemanggil.

**Contoh:** pesanan Patologi Klinik pasien Tunai berisi Hemoglobin dan Glukosa. Petugas menekan Terima
Sampling, layar Wadah terbuka, dan wadahnya dinyatakan layak → pesanan `Accepted`. Petugas memastikan pasien
sudah membayar di kasir, lalu menekan Proses Pemeriksaan → `InProcess`. Hemoglobin dirilis, Glukosa masih
*Menunggu Validasi*: Selesaikan **nonaktif**. Sesudah Glukosa dirilis, Keadaan Order menjadi *Selesai*,
Selesaikan dapat ditekan → `Completed`.

### BR-137 — Susunan menu Laboratorium mengikuti FE v1 (`LAB-DEC-192`)

**Aturan:**

1. Urutan dan label menu: Beranda; Penerimaan Sampling/Specimen; Pendaftaran Pasien Laboratorium; Daftar
   Pasien Lab Patologi Klinik; Daftar Pasien Lab Patologi Anatomi; Daftar Pasien Lab Mikrobiologi; Daftar
   Kerja Laboratorium; Pantau Keterlambatan Cito; Antrean Validasi; **Laporan** (berisi Laporan Penerimaan
   dan Laporan Operasional); Tarif Laboratorium; Master Data (isi tetap).
2. Route, key menu, dan izin **tidak berubah**. Laporan Operasional tetap berizin
   `LabOperationalReport : Read` di dalam grup Laporan.
3. Judul halaman Beranda dan ketiga daftar pasien lab mengikuti label menunya.
4. Ketiga daftar per disiplin tetap tiga menu terpisah dengan penyaring identik (BR-43, `FE-LAB-09`).
5. Ejaan disiplin tetap **Mikrobiologi**, bukan *Microbiologi* seperti v1.

### Bahan audit untuk `/trace-existing-capabilities`

Pertanyaan berikut dapat dijawab dari source, sehingga **tidak** ditanyakan kepada pemilik modul:

1. Apakah baris daftar membawa penunjuk pesanan yang cukup untuk `buildLabSpecimenRoute`, dan bagaimana aksi
   *Buka Hasil* membangun rutenya.
2. Pola keadaan aksi yang belum sah pada `RowActionMenu` modul ini — Konfirmasi nonaktif beralasan, Batalkan
   disembunyikan (`FE-LAB-15`/`FE-LAB-16`) — dan apakah layar membaca izin `LabOrder : Process` untuk
   menampilkan aksi.
3. Uji unit dan e2e yang mengunci label menu (*Ringkasan Laboratorium*, *Pemeriksaan …*) serta judul halaman
   — dampak penggantian label.
4. Komentar kode pada `menu-items.jsx` dan `lab-monitoring-constants.jsx` merujuk `LAB-DEC-025` untuk tiga
   menu, sedangkan decision log mencatatnya pada `LAB-DEC-048`/BR-43 — rujukannya perlu diselaraskan.
5. Bentuk `errors.details` penolakan `409` `complete` (`VAL-146`) untuk ditampilkan per pemeriksaan.
6. Apakah respons `LabOrderDetailResponse` dari `start-process`/`complete` cukup untuk memperbarui baris,
   atau daftar dimuat ulang.
7. Apakah penyaring menu membaca `requiredPermission` pada `subItems` — dipakai Laporan Operasional di grup
   Laporan; butir Master Data sudah memakainya.
8. Apakah peran analis pada `SysAccessPolicies` memegang `LabOrder : Process` — prasyarat uji nyata.
9. Letak judul halaman ketiga daftar (`LAB_MONITORING_DISCIPLINES`) dan Ringkasan
   (`laboratory-overview-view.jsx`) — satu tempat ganti atau tersebar.

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| Q1 | `LAB-DEC-062`, F22-5 | Proses Pemeriksaan tanpa jalur baca Lunas | ✅ *Aktif untuk semua, kunci menyusul* — `LAB-DEC-189` |
| Q2 | F22-4 | Tindakan status pesanan selain Proses | ✅ *Proses + Selesaikan* — `LAB-DEC-190` |
| Q3 | `LAB-DEC-154`, F22-6 | Kapan Selesaikan muncul | ✅ *PK & Mikro, bila semua dirilis* — `LAB-DEC-190` |
| Q4 | `LAB-DEC-060`, F22-7 | Perilaku Terima Sampling | ✅ *Buka layar Wadah* — `LAB-DEC-191` |
| Q5 | `LAB-DEC-048` | Susunan menu | ✅ *Urutan v1 + grup Laporan* — `LAB-DEC-192` |

> **Hasil putaran:** lima keputusan `LAB-DEC-188`..`LAB-DEC-192`, dua butir kewenangan UI
> `LAB-FE-030`/`LAB-FE-031`, BR-136, BR-137, dan `AC-276`..`AC-282`. Keputusan terkunci yang
> **diamandemen**: `LAB-DEC-048` (label menu), `LAB-DEC-060` (satu klik menjadi satu navigasi), dan
> `LAB-DEC-062` (kunci Lunas ditangguhkan). **Nol** perubahan `LAB-API-v1`, `LAB-VAL-v1`, `LAB-PERM-v1`,
> maupun `LAB-STATE-v1`, dan nol migration. Dibuka `LAB-OPEN-051`, yang menahan go-live aksi Proses
> Pemeriksaan, bukan roadmap. Langkah berikutnya: `/trace-existing-capabilities` atas bahan audit di atas,
> lalu task frontend lewat `plan-module-delivery`.

## Amendment Pass Putaran 23 — Urutan Terima Sampling dan Konfirmasi mengikuti FE v1 (2026-10-07)

**Sumber:** permintaan langsung pemilik modul 2026-10-07 — *"untuk urutan sampling dan konfirmasi ikut v1"* —
atas butir yang ditunda `LAB-DEC-188` (F22-8). Lima pertanyaan pilihan pada sesi yang sama; **kelimanya
dijawab dengan pilihan yang direkomendasikan**. Putaran ini juga **mencatat jawaban** pemilik modul atas
closure question capability map revision 7 (`Q-P22-01`..`Q-P22-03`), yang diberikan 2026-10-07 saat
memerintahkan *"langsung ke plan-module-delivery memakai rekomendasi di atas"* dan sudah dipakai
`frontend-roadmap.md` revisi 65. Fakta kode dibaca pada backend `f17cb984` dan frontend `70dd4d4e0` (commit
2026-10-07 10:39, memuat `FE-LAB-46`..`FE-LAB-49`); FE v1 dibaca baca-saja.

### Batas scope putaran ini

| Di dalam scope | Di luar scope |
|---|---|
| Kapan Konfirmasi sah, relatif terhadap penerimaan sampel | Opsi *langsung proses* pada pop-up Konfirmasi v1 — tetap ditunda `LAB-DEC-188` |
| Apakah Konfirmasi menjadi syarat Proses Pemeriksaan | Pembatalan sesudah sampel diterima — v1 mengizinkannya sampai pemeriksaan dimulai, V2 melarangnya (`LAB-DEC-063`); tidak diminta |
| Status pesanan sesudah Konfirmasi yang terlambat | Kunci Lunas pada Terima Sampling dan Proses — tetap `LAB-COORD-010` |
| Siapa memegang hak Konfirmasi | Hak Batalkan bagi analis — sisa `Q-P22-03`, tetap terbuka (`LAB-OPEN-052`) |
| Pencatatan jawaban `Q-P22-01`..`Q-P22-03` | |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F23-1 | Di v1, **Konfirmasi tersedia sejak pesanan dibuat sampai pemeriksaan dimulai** — sebelum maupun sesudah Terima Sampling. Komentar kodenya: *"Setelah Terima Sampling, konfirmasi harus tetap tersedia"* | v1 `DaftarPasienLabPatologiKlinik.jsx#isPendingConfirmation`, `#renderActions` (`canConfirm = confirmationPending`) |
| F23-2 | Di v1, **Proses Pemeriksaan hanya aktif pada status *Dalam Proses***, yaitu status sesudah konfirmasi (`canProcess = paymentIsLunas && isDalamProses`). Terima Sampling hanya mensyaratkan Lunas dan sampel belum diterima | v1 `#renderActions` |
| F23-3 | Di V2, Konfirmasi hanya sah dari `Requested`: `ConfirmAsync` menolak bila sudah pernah dikonfirmasi (`VAL-70`) atau status bukan `Requested` (`VAL-71`), lalu memindahkan status ke `Confirmed` | BE `LabOrderService.cs#ConfirmAsync`; `LAB-STATE-v1` `r3` bagian 1a |
| F23-4 | Di V2, `Requested` → `Accepted` sah tanpa konfirmasi (keputusan sadar `LAB-DEC-061`); apakah konfirmasi kelak wajib dicatat `LAB-OPEN-027` | `contracts/state-transition-matrix.md` bagian 1a |
| F23-5 | `POST /lab-orders/{id}/confirm` dijaga `LabOrder : Update` — izin yang juga membuka Batalkan dan penulisan konteks klinis pesanan (`LAB-DEC-091`). Jabatan *Analis Laboratorium* tidak memegang `LabOrder : Update` sejak 2026-10-01 | `LabOrderController.cs#Confirm`; capability map rev 7 `CAP-P22-15` |
| F23-6 | Data dev: `LAB-RSMMC-000001` dan `000002` sudah `InProcess` tanpa pernah dikonfirmasi; `000003` `Accepted` belum dikonfirmasi | Capability map rev 7; uji `FE-LAB-47`/`48` |

### BR-138 — Konfirmasi dan Proses Pemeriksaan mengikuti urutan v1 (`LAB-DEC-193`..`LAB-DEC-197`)

**Aturan:**

1. Konfirmasi sah pada pesanan berstatus `Requested` **atau** `Accepted`, selama pesanan itu **belum pernah
   dikonfirmasi**. Pada `InProcess`, `Completed`, `Cancelled`, dan `OnHold` tetap ditolak. Konfirmasi tetap
   hanya sah sekali.
2. Konfirmasi pada `Requested` memindahkan status ke `Confirmed`, seperti hari ini. Konfirmasi pada `Accepted`
   **tidak mengubah status** — pesanan tetap *Diterima*; yang tercatat adalah konfirmator, waktu, dan dokter
   pemeriksa. Status tidak pernah mundur.
3. Proses Pemeriksaan sah hanya bila pesanan `Accepted` **dan sudah dikonfirmasi** (jejak konfirmasi terisi),
   bukan dibaca dari status.
4. Syarat konfirmasi **hanya** berlaku pada Proses Pemeriksaan. Selesaikan tidak memeriksanya, sehingga
   pesanan yang sudah *Sedang Dikerjakan* tanpa konfirmasi tetap dapat diselesaikan.
5. Hak Konfirmasi dipisah menjadi aksi izin tersendiri dan diberikan kepada jabatan Analis Laboratorium. Hak
   itu tidak membuka Batalkan maupun penulisan konteks klinis pesanan.

**Contoh — urutan v1 di V2:** pesanan Hemoglobin pasien rawat jalan masuk berstatus *Diminta*. Analis menekan
Terima Sampling, wadah dinyatakan layak → *Diterima*. Proses Pemeriksaan masih redup: *belum dikonfirmasi*.
Analis menekan Konfirmasi dan memilih dokter pemeriksa → status tetap *Diterima*, kolom Konfirmasi kini berisi
namanya, waktu, dan dokter pemeriksa. Proses Pemeriksaan kini dapat ditekan → *Sedang Dikerjakan*.

**Contoh — urutan V2 lama tetap sah:** pesanan lain dikonfirmasi lebih dulu → *Dikonfirmasi*; wadahnya
dinyatakan layak → *Diterima*; Proses Pemeriksaan langsung dapat ditekan karena jejak konfirmasi sudah ada.

**Contoh — pesanan yang sudah berjalan:** `LAB-RSMMC-000002` sudah *Sedang Dikerjakan* tanpa konfirmasi.
Konfirmasi tidak lagi sah padanya, tetapi Selesaikan tetap dapat ditekan begitu seluruh hasilnya dirilis.

**Perubahan status:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| `Requested` | Konfirmasi | `Confirmed` | Pemegang hak Konfirmasi | Belum pernah dikonfirmasi; dokter pemeriksa dipilih |
| `Accepted` | Konfirmasi | `Accepted` (tetap) | Pemegang hak Konfirmasi | Belum pernah dikonfirmasi; dokter pemeriksa dipilih |
| `Accepted` | Proses Pemeriksaan | `InProcess` | `LabOrder : Process` | **Sudah dikonfirmasi** |
| `Accepted` belum dikonfirmasi | Proses Pemeriksaan | — | — | Ditolak |
| `InProcess` | Selesaikan | `Completed` | `LabOrder : Process` | `VAL-146`; konfirmasi **tidak** diperiksa |

**Dampak kontrak — belum diamandemen, diteruskan ke `/design-business-module`:**

| Kontrak | Yang perlu berubah |
|---|---|
| `LAB-STATE-v1` `r7` | Bagian 1a: baris `Accepted` → Konfirmasi dipindah dari "tidak sah" ke "sah" (status tetap); baris baru `Accepted` belum dikonfirmasi → Proses ditolak |
| `LAB-VAL-v1` `r16` | `VAL-71` dipersempit (status selain `Requested`/`Accepted`); aturan baru *Proses wajib terkonfirmasi* beserta kode penolakannya |
| `LAB-PERM-v1` revision 12 | Aksi izin baru untuk Konfirmasi pada resource `LabOrder` (nama diputuskan desain, mis. `Confirm`); `POST …/confirm` berpindah dari `Update` ke aksi itu |
| `LAB-API-v1` `r39` | Hak akses `confirm`; penolakan baru `start-process` |

### Jawaban closure question capability map revision 7 — dicatat

| Pertanyaan | Jawaban pemilik modul 2026-10-07 | Dicatat sebagai |
|---|---|---|
| `Q-P22-01` — Selesaikan pada `InProcess` dengan `resultProgress` kosong | **Boleh**, menyamai backend 22.7 butir 3; bunyi `AC-278` diamandemen | `LAB-DEC-198`; **`LAB-CONFLICT-017` ditutup** |
| `Q-P22-02` — `start-process` `400` lawan kontrak `409` | Layar menampung keduanya; penyelarasan backend `BE-LAB-89` tidak memblokir | `LAB-DEC-199` |
| `Q-P22-03` — analis tanpa `LabOrder : Update` | **Hak Konfirmasi** terjawab putaran ini (`LAB-DEC-197`); **hak Batalkan** tetap terbuka | `LAB-OPEN-052` |
| Rekomendasi subjudul Nota | Diterima: tetap `Pemeriksaan <disiplin>`, dilepas dari judul halaman | `LAB-FE-032` |

### Bahan audit untuk `/trace-existing-capabilities`

1. Pemanggil `POST /lab-orders/{id}/confirm` di luar Laboratorium (mis. layar dokter) yang akan terdampak
   pemindahan izin dari `Update` ke aksi baru.
2. Jabatan mana saja yang hari ini memegang `LabOrder : Update` dan memakainya untuk Konfirmasi.
3. Jumlah pesanan `Accepted` belum dikonfirmasi di dev dan produksi — yang akan redup Proses-nya sesudah
   penjaga baru berlaku.
4. Apakah `LabOrderDetailResponse` dan baris daftar sudah membawa `confirmedAt` untuk menegakkan aturan 3 di
   layar (daftar: ya, `FE-LAB-15`; detail: diperiksa).

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| Q1 | F23-1, F23-3 | Kapan Konfirmasi sah | ✅ *Sebelum atau sesudah sampling, sampai pesanan diproses* — `LAB-DEC-193` |
| Q2 | F23-2, `LAB-OPEN-027` | Konfirmasi syarat Proses? | ✅ *Wajib, seperti v1* — `LAB-DEC-194` |
| Q3 | Q1 + Q2 | Status sesudah Konfirmasi pada `Accepted` | ✅ *Tetap Diterima* — `LAB-DEC-195` |
| Q4 | F23-6 | Cakupan syarat konfirmasi | ✅ *Hanya saat Proses* — `LAB-DEC-196` |
| Q5 | F23-5, `Q-P22-03` | Hak Konfirmasi bagi analis | ✅ *Izin Konfirmasi sendiri* — `LAB-DEC-197` |

> **Hasil putaran:** lima keputusan `LAB-DEC-193`..`LAB-DEC-197` beserta BR-138; dua keputusan pencatatan
> `LAB-DEC-198`, `LAB-DEC-199`; `LAB-FE-032`, `LAB-FE-033`; `AC-283`..`AC-288` dan amandemen bunyi `AC-278`.
> **Diamandemen:** `LAB-DEC-188` (butir urutan v1 kini diputuskan), `LAB-DEC-189` (Proses kini wajib
> terkonfirmasi), `LAB-DEC-061` (Konfirmasi juga sah pada `Accepted`). **Ditutup:** `LAB-OPEN-027` dalam
> bentuk lain — konfirmasi **tidak** wajib sebelum `Accepted`, tetapi wajib sebelum `InProcess`;
> `LAB-CONFLICT-017`. **Dibuka:** `LAB-OPEN-052`. Berbeda dari putaran 22, putaran ini **menuntut amandemen
> kontrak** `LAB-STATE-v1`, `LAB-VAL-v1`, `LAB-PERM-v1`, dan `LAB-API-v1` beserta task backend — nol migration
> skema diperkirakan. Langkah berikutnya: `/design-business-module` untuk amandemen kontrak.

## Amendment Pass Putaran 24 — Dokter pemeriksa bagi pengonfirmasi dan jawaban detail konfirmasi (2026-10-07)

**Sumber:** dua temuan verifikasi `FE-LAB-50` dengan akun analis asli, 2026-10-07
([`FE-LAB-50.md`](task/report/frontend/FE-LAB-50.md) bagian 8). Pemilik modul memerintahkan *"lanjutkan langkah
selanjutnya"* sesudah laporan itu; batas scope dan empat pertanyaan pilihan dijawab pada sesi yang sama —
**seluruhnya dengan pilihan yang direkomendasikan**. Fakta dibaca pada backend `f17cb984` + working tree
`BE-LAB-90`, frontend `70dd4d4e0` + working tree `FE-LAB-50`; FE v1 dibaca baca-saja.

### Batas scope putaran ini

Disetujui pemilik modul 2026-10-07 (*Setuju*).

| Di dalam scope | Di luar scope |
|---|---|
| Siapa yang boleh dipilih sebagai dokter pemeriksa saat Konfirmasi pesanan lab | Aturan akses master dokter milik modul SDM/HR — kebijakan `KioskRead` **tidak** diubah dari modul ini |
| Siapa yang boleh memuat daftar pilihan itu, dan apa isinya | Jadwal jaga dokter (`TrxOnCallAssignment`) |
| Nilai awal pilihan dokter pemeriksa | Hak Batalkan bagi analis — tetap `LAB-OPEN-052` |
| Pencatatan T1 sebagai perbaikan kesesuaian kontrak `r13` | |
| Urutan rilis `MVP-12c` akibat kedua temuan | |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F24-1 | **T1.** `LabOrderService.GetDetailAsync` — dipakai `GET /lab-orders/{id}` serta jawaban `confirm` dan `start-process` — **tidak memetakan** `ConfirmedAt`, `ConfirmedByName`, `ExaminerDoctorName`, `ConfirmedByUserId`, `ExaminerDoctorId`. Sesudah Konfirmasi sungguhan `LAB-RSMMC-000003`, kelimanya dijawab `null` walau DB terisi. `LAB-API-v1` `r13` (`approved` 2026-09-16) menjanjikan kelimanya | HTTP asli 2026-10-07; `LabOrderService.cs#GetDetailAsync`, `#MapDetailResponse` |
| F24-2 | T1 tertutup selama Konfirmasi selalu memindahkan status ke `Confirmed`, karena layar juga membaca status itu. Sejak `LAB-DEC-195` status tetap `Accepted`, sehingga kolom Konfirmasi dan Proses baru benar sesudah daftar dimuat ulang | `FE-LAB-50` S4 `FAIL`, S6a `PASS` |
| F24-3 | **T2.** Pemilih dokter pemeriksa dialog Konfirmasi (`FE-LAB-15`) memanggil `GET /v1/corporate/human-resource/master-data/doctors/options`, yang dijaga kebijakan Identity `KioskRead`: hanya peran SuperAdmin, Administrator, Kiosk, atau akun kios. Kebijakan itu **tidak dapat dibuka lewat Akses Role**. Analis pemegang `LabOrder : Confirm` mendapat `403`, sehingga tidak dapat mengonfirmasi lewat layar | `DoctorController.cs#GetDoctorOptionsForKiosk`; `Program.cs` `AuthorizationPolicies.KioskRead`; HTTP asli `403` |
| F24-4 | T2 sudah ada sejak `FE-LAB-15`, tetapi tidak pernah terlihat karena Konfirmasi selama ini diuji dengan superadmin | Laporan `FE-LAB-15`; memori uji |
| F24-5 | Aturan dokter pemeriksa yang berlaku: wajib dipilih (`VAL-72`), wajib ada dan aktif (`VAL-73`). Nol aturan tentang siapa yang **boleh** dipilih atau siapa yang **boleh melihat** daftarnya | BR-48; `LAB-VAL-v1` |
| F24-6 | FE v1 memberi label *"Pilih Dokter Pemeriksa Lab"*: dokter berelasi laboratorium didahulukan, semua dokter sebagai cadangan; nilai awal dari dokter data pendaftaran pasien lab | v1 `DaftarPasienLabMicrobiologi.jsx` — `doctors` (`isLabExaminingDoctor`), `getRowDokterPemeriksaId` |
| F24-7 | Preseden di modul ini: daftar dokter konfirmator hasil milik Laboratorium (BR-65, `GET /lab-examinations/{id}/confirming-doctor-options`), dijaga `LabExamination : Read`, dengan cadangan semua dokter aktif | `LabExaminationController.cs#GetConfirmingDoctorOptions` |
| F24-8 | Data dev sesudah verifikasi: `LAB-RSMMC-000003` dikonfirmasi analis lalu `InProcess`; **nol pesanan `Accepted` belum dikonfirmasi** tersisa untuk uji ulang | Kueri baca-saja 2026-10-07 |

### BR-139 — Dokter pemeriksa dapat dipilih oleh setiap pengonfirmasi (`LAB-DEC-200`..`LAB-DEC-203`, `LAB-FE-034`)

**Aturan:**

1. **Setiap dokter aktif boleh dipilih sebagai dokter pemeriksa.** `VAL-73` tidak berubah; tidak ada
   penyaring dokter laboratorium, spesialisasi, disiplin, atau jadwal jaga.
2. **Daftar pilihan disediakan modul Laboratorium sendiri** dan dapat dimuat oleh **setiap pemegang
   `LabOrder : Confirm`** — orang yang boleh mengonfirmasi otomatis boleh melihat pilihannya, tanpa izin baru
   dan tanpa izin master dokter HR.
3. **Isi daftar terbatas pada yang dibutuhkan memilih:** nama, kode, dan spesialisasi dokter aktif, dapat dicari.
   Nomor telepon, alamat, dan data pribadi lain **tidak** ikut.
4. **Dialog Konfirmasi dibuka dengan pilihan kosong;** petugas wajib memilih sendiri (`AC-95` tetap).
5. **Jawaban detail pesanan membawa jejak konfirmasi** — `confirmedAt`, `confirmedByName`,
   `examinerDoctorName`, `confirmedByUserId`, `examinerDoctorId` — sesuai `r13`. Ini perbaikan kesesuaian, bukan
   aturan baru.
6. **Urutan v1 dirilis serempak.** `BE-LAB-90`, perbaikan butir 5, daftar dokter butir 2, dan `FE-LAB-50` beserta
   pemakaian daftar baru dideploy dalam **satu** gelombang. Tidak boleh ada lingkungan di mana Proses wajib
   terkonfirmasi tetapi analis tidak dapat memuat daftar dokter.

**Contoh.** Analis Gilang membuka Konfirmasi untuk pesanan Ureum *Diterima*. Kotak *Dokter Pemeriksa* kosong;
ia mengetik "Adit", memilih *dr. Aditya Pranata, Sp.PK*, lalu Simpan. Baris tetap *Diterima*, kolom Konfirmasi
langsung berisi namanya, waktu, dan dokter pemeriksa, dan Proses Pemeriksaan langsung dapat ditekan — tanpa muat
ulang. Petugas pendaftaran yang tidak memegang `LabOrder : Confirm` tidak dapat memuat daftar itu (`403`).

**Perubahan status.** Tidak ada — BR-139 tidak menyentuh `LAB-STATE-v1`.

### Bahan audit untuk `/trace-existing-capabilities` atau desain

1. Apakah sudah ada daftar dokter yang dijaga Akses Role (bukan `KioskRead`) dan dapat dipakai ulang tanpa
   menduplikasi — sebelum membuat daftar baru di modul Lab.
2. Ruas spesialisasi dan kode pada `MstDoctor` yang dapat dipakai butir 3.
3. Pemanggil lain `GetDetailAsync`/`LabOrderDetailResponse` yang ikut menerima lima ruas butir 5 (aditif).
4. Pesanan uji `Accepted` belum dikonfirmasi untuk mengulang `FE-LAB-50` S4 (F24-8).

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| Q0 | — | Batas scope | ✅ *Setuju* |
| Q1 | F24-5, F24-6 | Siapa yang boleh dipilih sebagai dokter pemeriksa | ✅ *Semua dokter aktif* — `LAB-DEC-200` |
| Q2 | F24-3, F24-7 | Siapa yang boleh memuat daftar, dan isinya | ✅ *Pemegang izin Konfirmasi* — `LAB-DEC-201` |
| Q3 | F24-6 | Nilai awal pilihan | ✅ *Kosong, wajib dipilih* — `LAB-FE-034` |
| Q4 | F24-1..F24-3 | Urutan rilis `MVP-12c` | ✅ *Tahan, rilis serempak* — `LAB-DEC-202` |

> **Hasil putaran:** `LAB-DEC-200`..`LAB-DEC-203` beserta BR-139; `LAB-FE-034`; `AC-289`..`AC-293`.
> **Diamandemen:** urutan langkah rilis `MVP-12c` (`backend-roadmap.md` 6ar.2) — dari bertahap menjadi serempak.
> **Tidak diamandemen:** `VAL-72`, `VAL-73`, `LAB-STATE-v1`, `LAB-PERM-v1` (nol aksi izin baru). **Menuntut
> amandemen `LAB-API-v1`** (daftar dokter pemeriksa milik Lab; penegasan lima ruas `r13` pada jawaban detail)
> beserta task backend dan frontend baru — nol migration diperkirakan. Langkah berikutnya:
> `/design-business-module` untuk amandemen kontrak.

## Amendment Pass Putaran 25 — Beranda Lab mengikuti FE v1 (2026-10-07)

**Sumber:** permintaan langsung pemilik modul 2026-10-07 — *"untuk beranda menu lab ubah yaa versi yang sekarang agar
sama seperti versi v1 sesuaikan dengan tema yang sekarang ada"* — atas butir *grafik Beranda versi v1* yang ditunda
`LAB-DEC-188`. Batas scope dan sebelas pertanyaan pilihan dijawab pada sesi yang sama — **seluruhnya dengan pilihan
yang direkomendasikan**. Fakta dibaca pada backend `f17cb984` + working tree (`BE-LAB-90`, `BE-LAB-91`), frontend
`70dd4d4e0` + working tree (`FE-LAB-50`, `FE-LAB-51`), dan FE v1 `dashboard-laboratorium.jsx` (baca-saja).

### Batas scope putaran ini

Disetujui pemilik modul 2026-10-07 (*Setuju*).

| Di dalam scope | Di luar scope |
|---|---|
| Isi dan susunan halaman **Beranda** Lab (menu yang sudah ada, `/health-services/laboratory-management/overview`) mengikuti Beranda v1 dengan komponen dan tema V2 | Menu Hasil & Riwayat + WhatsApp (`S17`), Daftar Pasien OTC, opsi *langsung proses*, kunci *Lunas*, Tahan/Lanjutkan, Batalkan oleh analis — putaran lain |
| Data ringkasan yang dibutuhkan halaman itu | Dasbor beranda utama aplikasi (`/`) — bukan modul Lab |
| Siapa yang boleh membuka Beranda | Pembaruan realtime (SignalR) — ditunda `LAB-DEC-210` |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F25-1 | Beranda v1: hero *"Beranda Laboratorium Rumah Sakit"* dengan status koneksi, waktu pembaruan, dan tombol *Perbarui Data*; empat kartu *hari ini* — Booking, Menunggu Pemeriksaan, Pemeriksaan Selesai (dengan persentase), Prioritas CITO; *Fokus utama* — Total Booking Tercatat, Petugas Laboratorium, Jenis Laboratorium, Tingkat Penyelesaian; grafik batang *Jumlah pemeriksaan per laboratorium*; grafik lingkar *Sebaran*; grafik *Tren booking* bulanan tahun terpilih vs tahun sebelumnya; tabel *Data booking* berhalaman; panel *Kesiapan operasional* | v1 `src/components/view/laboratorium/dashboard-laboratorium.jsx` |
| F25-2 | Di v1, *Menunggu* = seluruh booking hari ini dikurangi yang selesai (yang batal ikut terhitung); *CITO* = booking hari ini bertanda CITO; *Petugas* = pengguna aktif yang dikenali sebagai petugas lab dari daftar pengguna; grafik per laboratorium mengambil sejumlah baris pemeriksaan **terbaru** tanpa rentang tanggal | v1 `#summaryToday`, `#getIsCito`, `#fetchPetugasLab`, `#fetchDetailData` (`MAX_DASHBOARD_ROWS`) |
| F25-3 | Beranda V2: `Hero` *"Beranda"*, `SummaryCards`, tabel *Rekap Status Pesanan* per rentang tanggal (bawaan 30 hari) dari `GET /lab-orders/summary` — per status dan per disiplin, **tanpa** kesegeraan, **tanpa** hitungan pemeriksaan, **tanpa** tren bulanan | `laboratory-overview-view.jsx`, `use-laboratory-overview.jsx`; `LabOrderSummaryResponse` |
| F25-4 | Beranda V2 tidak punya penjaga izin sendiri: ia menampilkan *Akses Ditolak* bila `summary` (`LabOrder : Read`) menolak | `laboratory-overview-view.jsx` (`AccessDeniedGate`) |
| F25-5 | V2 sudah punya klien SignalR umum; backend Lab belum menyiarkan perubahan pesanan | `src/lib/signalr/signalrHubClient.jsx` |

### BR-140 — Beranda Lab mengikuti susunan v1 (`LAB-DEC-204`..`LAB-DEC-211`, `LAB-FE-035`)

**Aturan:**

1. **Susunan v1, tema V2.** Hero; empat kartu *hari ini*; empat kartu *fokus operasional*; grafik pemeriksaan per
   disiplin; grafik sebaran; grafik tren bulanan; tabel pesanan terbaru; panel kesiapan operasional; lalu **rekap status
   per rentang tanggal yang sudah ada tetap di bagian paling bawah**.
2. **"Hari ini" = 00.00–23.59 WIB menurut waktu pesanan diminta.** *Pesanan hari ini* = seluruh pesanan hari ini.
   *Menunggu* = pesanan hari ini yang belum *Selesai* **dan** tidak *Dibatalkan*. *Selesai* = pesanan hari ini berstatus
   *Selesai*, beserta persentasenya terhadap pesanan hari ini. *CITO* = pesanan hari ini berkesegeraan CITO.
3. **Fokus operasional:** *Total pesanan tercatat*; *Hasil menunggu validasi* (pemeriksaan yang hasilnya sudah final
   diisi tetapi belum divalidasi — **menggantikan** *Petugas Laboratorium* v1); *Jenis laboratorium* (jumlah disiplin
   yang punya pemeriksaan pada tahun terpilih); *Tingkat penyelesaian* hari ini.
4. **Grafik per disiplin dan sebaran menghitung pemeriksaan**, bukan pesanan — pemeriksaan tidak batal pada pesanan yang
   diminta di **tahun terpilih**. Satu pemilih tahun berlaku untuk ketiga grafik.
5. **Tren bulanan menghitung pesanan per bulan** (WIB, menurut waktu diminta) di tahun terpilih dan tahun sebelumnya,
   **tanpa** *Dibatalkan* dan *Draft*.
6. **Pesanan terbaru:** 10 pesanan terakhir diminta dari ketiga disiplin — No. RM, nama pasien, pemeriksaan, disiplin,
   status, waktu diminta; klik baris membuka halaman pesanan yang sudah ada.
7. **Kesiapan operasional:** waktu data terakhir dimuat, ringkasan progres hari ini, dan tombol *Perbarui Data*.
   Pembaruan otomatis realtime **ditunda**.
8. **Akses:** siapa pun pemegang `LabOrder : Read`; data ringkasan baru ikut dijaga izin itu. Nol izin baru.

**Contoh.** Pukul 09.15 WIB ada 12 pesanan sejak tengah malam: 3 *Selesai*, 1 *Dibatalkan*, 2 CITO. Kartu berbunyi
*Pesanan hari ini 12*, *Menunggu 8*, *Selesai 3 (25%)*, *CITO 2*. Grafik tahun 2026 memperlihatkan Patologi Klinik 1.240
pemeriksaan, Mikrobiologi 310, Patologi Anatomi 95; garis tren Mei 2026 berada di atas Mei 2025.

**Perubahan status.** Tidak ada — Beranda adalah layar baca.

### Bahan audit untuk `/trace-existing-capabilities` atau desain

1. Sumber kesegeraan CITO pada pesanan (`LabOrder` atau per pemeriksaan) dan apakah daftar pantau sudah membacanya.
2. Keadaan pemeriksaan yang berarti *hasil final, belum divalidasi* — selaras dengan antrean validasi (`FE-LAB-42`).
3. Apakah `summary` dapat diperluas, atau ringkasan Beranda menjadi endpoint baca tersendiri.
4. Konversi WIB ↔ UTC pada `RequestedAt` (jebakan tanggal yang sudah tercatat pada modul SDM).

### Urutan pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| Q0 | — | Batas scope | ✅ *Setuju* |
| Q1 | F25-1, F25-3 | Bagian v1 yang dibangun | ✅ *Semua bagian v1* — `LAB-DEC-204` |
| Q2 | F25-2 | Kartu *Petugas Laboratorium* | ✅ *Ganti metrik lab* — `LAB-DEC-208` |
| Q3 | F25-3 | Rekap status lama | ✅ *Pertahankan di bawah* — `LAB-DEC-204` |
| Q4 | F25-2 | Arti *hari ini* dan *Menunggu* | ✅ *WIB; Menunggu tanpa batal* — `LAB-DEC-205` |
| Q5 | F25-2 | Satuan grafik per disiplin | ✅ *Pemeriksaan* — `LAB-DEC-206` |
| Q6 | F25-1 | Isi tren bulanan | ✅ *Pesanan non-batal per bulan* — `LAB-DEC-207` |
| Q7 | Q2 | Metrik pengganti | ✅ *Hasil menunggu validasi* — `LAB-DEC-208` |
| Q8 | F25-1 | Tabel pesanan terbaru | ✅ *10 terbaru, klik buka pesanan* — `LAB-DEC-209` |
| Q9 | F25-5 | Kesiapan dan pembaruan | ✅ *Waktu muat + tombol Perbarui* — `LAB-DEC-210` |
| Q10 | F25-4 | Akses | ✅ *Pemegang LabOrder : Read* — `LAB-DEC-211` |
| Q11 | F25-2 | Rentang grafik per disiplin | ✅ *Ikut tahun di grafik tren* — `LAB-DEC-206` |

> **Hasil putaran:** `LAB-DEC-204`..`LAB-DEC-211` beserta BR-140; `LAB-FE-035`; `AC-294`..`AC-301`. **Diputuskan:**
> butir *grafik Beranda versi v1* `LAB-DEC-188`. **Berbeda dari v1 secara sadar:** *Menunggu* tanpa pesanan batal,
> metrik petugas diganti antrean validasi, rentang grafik jelas (tahun terpilih), tabel terbaru tidak berhalaman,
> tanpa realtime. **Menuntut amandemen `LAB-API-v1`** (data ringkasan Beranda) beserta task backend dan frontend —
> nol migration diperkirakan. Langkah berikutnya: `/design-business-module`.

## Amendment Pass Putaran 26 — Alur Lab dari kiosk sampai hasil mengikuti FE v1 (2026-10-08)

**Sumber:** permintaan langsung pemilik modul 2026-10-07 — *"tes alur modul lab dari kiosk sampai selesai [di v1]
lalu terapkan pada QuilvianV2 … pastikan sama proses alur UI dan UX-nya, tetapi dengan tetap mempertahankan tema
tampilan seperti sekarang"*. Enam keputusan diberikan pada sesi 2026-10-07/08 sebagai pertanyaan pilihan dan dicatat
apa adanya pada bukti [`LAB-EVD-013`](evidence/2026-10-08-keputusan-paritas-alur-v1.md). Putaran ini **mencatat**
keenamnya, menandai keputusan yang tergeser, mengoreksi satu klaim keliru pada bukti itu, dan menutup tiga
pertanyaan lanjutan yang muncul dari implementasinya — **seluruhnya dijawab dengan pilihan yang direkomendasikan**.

Alur v1 ditelusuri pada aplikasi tayang `v1.quilvian-mmchospital.com` (akun yang diberikan pemilik modul) dan
kode v1 lokal (baca-saja). Fakta V2 dibaca pada backend `6564e04d` (branch `yoga`) + working tree dan frontend
`f6f23352e` (branch `YogaV2`). **Implementasi mendahului putaran ini** atas arahan langsung pemilik modul;
putaran ini membukukannya, bukan memberi izin baru.

### Batas scope putaran ini — `LAB-DEC-212`

Disetujui pemilik modul 2026-10-08 (*Setuju*).

| Di dalam scope | Di luar scope |
|---|---|
| Mencatat enam keputusan `LAB-EVD-013` ke decision log | Kirim hasil ke pasien lewat WhatsApp (`S17`) — tetap ditunda |
| Menandai keputusan tergeser: `LAB-DEC-044`, `045`, `052`, `060`, `062`, `188`, `189`, `191`; `LAB-FE-010`, `011`, `030`; `LAB-OPEN-051`; layar serah-terima `FE-LAB-13` | Status kunjungan kiosk Lab tertulis *Menunggu Dokter* dan prefiks antrean `Q` — milik `registration-management` |
| Koreksi klaim `AC-45` pada `LAB-EVD-013` | Aturan internal Billing (rumus tagihan, pelunasan di kasir) |
| Dua konflik implementasi: jalur baca status Lunas dan `LAB-FE-011` | Tahan/Lanjutkan pesanan — tetap ditunda `LAB-DEC-188` |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F26-1 | **`AC-45` tidak tergeser.** Bunyinya: *kode Laboratorium tidak menulis ke tabel kunjungan maupun tabel pasien*. Kunjungan kiosk Lab dibentuk lewat `POST /patient-encounters/kiosk` milik Registrasi — pelaksanaan `LAB-DEC-053` yang sudah disetujui 2026-09-15 dan jalurnya dibuktikan `BE-EXT-05`. Klaim *"AC-45 tergeser"* pada `LAB-EVD-013` bagian B **keliru** dan dikoreksi di sana | `PatientEncounterController.cs` route `kiosk`; `task/report/backend/BE-EXT-05.md` bagian 1.1 |
| F26-2 | Kiosk v1 jalur Lab: identifikasi No. RM/NIK/NIP → bila ada booking Lab: data booking + daftar pemeriksaan → *Konfirmasi Kehadiran* (**nol tulis ke API**); bila tidak: Data Pasien → Tipe Pasien → Pembayaran → Konfirmasi → kunjungan `OPLab` dibentuk | v1 `add-layanan-lab/index.jsx` (`handleScreeningSubmit`, `handleConfirmKehadiran`, `handleCreateKunjungan`) |
| F26-3 | Penerimaan Sampling v1: **satu Simpan** untuk pasien, data pemeriksaan (pembayaran, tanggal sampling, kategori Lab, perujuk, dokter pemeriksa, diagnosa awal), daftar pemeriksaan (+CITO, harga, Grand Total), specimen (+mL), kelayakan, keterangan. Daftar Pasien OTC v1: kunjungan kiosk tanpa booking Lab, tab Umum/Rujukan, aksi Proses/Hapus | v1 `Laboratorium/pendaftaran-pasien-lab`, `daftar-pasien-otc` (tayang) |
| F26-4 | Daftar Pasien v1: Terima Sampling dan Proses Pemeriksaan **wajib Lunas**; pasien asuransi/penjamin dianggap lolos; pop-up Konfirmasi memuat centang *sampling diterima* dan *langsung proses*. Hasil dan Riwayat v1: Lihat Hasil, Nota Lab, Label Lab, Kirim Hasil ke Pasien (WhatsApp), Label Goldar | v1 `DaftarPasienLabPatologiKlinik.jsx` (`buildPaymentSummary`, `handleConfirm`); `HasilRiwayatLabPatologiKlinik.jsx` |
| F26-5 | Tagihan Lab V2 terbit saat wadah **Layak** (`LabSpecimenService` → `BillingSourceContract.LaboratorySourceContext`, status tagih `ACCEPTED`). Sebelum itu tidak ada baris tagihan yang dapat dilunasi | `BillingClinicalChargeBridgeService.MapBillableStatus` |
| F26-6 | **`VAL-09`** menolak `403` ketika pengambil sampel menetapkan kelayakan wadahnya sendiri. Satu Simpan v1 oleh satu petugas karena itu **pasti ditolak** di V2 — ditemukan pada uji tulis sungguhan 2026-10-08 (order `LAB-RSMMC-000022`) | `LabSpecimenService.AcceptAsync`; `contracts/validation-matrix.md` `VAL-09` |
| F26-7 | Status Lunas kini dihitung `LabPaymentClearanceRules` dengan **membaca tabel Billing langsung** (`BilInvoices`, `BilCalculationVersions`, `BilInvoiceItems`, `BilSettlements`, `BilTenders`), memakai rumus `BillingInvoiceService.GetPaymentHistoryAsync`. `LAB-REQ-008` kepada pemilik Billing **belum dijawab** | `LaboratoryManagement/Services/LabPaymentClearanceRules.cs`; `approval-requests/2026-09-17-permintaan-jalur-baca-status-pembayaran.md` |
| F26-8 | Layar Penerimaan baru memuat **pilihan** metode pembayaran Tunai/Asuransi saat kunjungan baru dibentuk — sama dengan form Pendaftaran Pasien Laboratorium V2 yang sudah ada — padahal `LAB-FE-011` mewajibkan baca-saja | `lab-specimen-reception-view.jsx`; `lab-patient-registration-form-view.jsx` |
| F26-9 | Endpoint dan ruas baru **belum tercatat** pada `LAB-API-v1`: `GET /lab-orders/kiosk/pending-by-patient/{patientId}` (`KioskRead`), `GET /lab-patient-registrations/kiosk-encounters`, ruas `paymentStatus`/`isPaymentCleared`/`outstandingAmount` daftar pantau, `orderedProcedures[].procedureId` detail pesanan (sebelumnya ruas itu **selalu kosong**), `collectedByUserId`/`collectedByName` wadah | commit BE `6564e04d` + working tree |

### BR-141 — Alur Lab dari kiosk sampai hasil mengikuti FE v1 (`LAB-DEC-213`..`LAB-DEC-221`, `LAB-FE-036`)

**Tujuan.** Petugas dan pasien menjalani urutan kerja yang sama dengan v1 — kiosk, Daftar Pasien OTC, Penerimaan
Sampling, Daftar Pasien Lab, Hasil dan Riwayat — dengan tema tampilan V2 dan tanpa melonggarkan pengaman V2
(empat mata wadah `VAL-09`, validasi/rilis empat mata hasil, kepemilikan kunjungan oleh Registrasi).

**Pelaku.**

| Pelaku | Mengerjakan | Berwenang menyetujui |
|---|---|---|
| Pasien | Kiosk: identifikasi, memilih Laboratorium, Konfirmasi Kehadiran atau mendaftar | — |
| Analis/petugas penerima (petugas A) | Daftar Pasien OTC → Penerimaan Sampling → Simpan; Terima Sampling (catat diterima) | — |
| Analis lain (petugas B, **bukan** pengambil sampel) | Terima Sampling → Layak/Tidak Layak | Kelayakan wadah (`VAL-09`) |
| Pemegang `LabOrder : Confirm` | Konfirmasi + dokter pemeriksa | — |
| Pemegang `LabOrder : Process` | Proses Pemeriksaan, Selesaikan | — |
| Kasir (Billing) | Pelunasan tagihan pasien Tunai | Di luar scope |

**Pemicu.** Pasien datang ke kiosk atau langsung ke loket Laboratorium.

**Prasyarat.** Pasien sudah punya rekam medis; unit layanan bertipe Laboratorium tersedia untuk pendaftaran
(`SU-LAB-001`).

**Langkah utama.**

1. **Kiosk.** Pasien memilih *Laboratorium*. Kiosk membaca pesanan Lab aktif pasien (diminta, diterima,
   dikerjakan, atau ditahan; 30 hari terakhir).
   - **Ada pesanan** → layar *Konfirmasi Kehadiran* menampilkan tanggal, asal poli, dokter, dan daftar
     pemeriksaan → pasien menekan *Konfirmasi Kehadiran*. Satu-satunya yang tercatat adalah sesi kiosk bertujuan
     Laboratorium dengan penanda *membawa permintaan dokter* — panel petugas membacanya seperti sebelumnya.
   - **Tidak ada pesanan** → Jenis Kunjungan (Umum/Rujukan) → Pembayaran → Konfirmasi → kunjungan dibentuk route
     kiosk Registrasi ke unit Laboratorium tanpa dokter → tiket antrean.
2. **Daftar Pasien OTC.** Kunjungan kiosk Laboratorium yang belum punya pesanan tampil pada tab Umum atau
   Rujukan. Petugas A menekan *Proses*.
3. **Penerimaan Sampling.** Petugas A melengkapi (atau, bagi pasien datang langsung, mendaftarkan) pasien, memilih
   Kategori Lab, pemeriksaan beserta CITO, dan jenis specimen, lalu menekan **satu** *Simpan*: kunjungan (bila belum
   ada) → satu pesanan per kategori → wadah → diambil → diterima. Kelayakan **tidak** ditetapkan di sini.
4. **Terima Sampling (Daftar Pasien Lab).** Petugas B membuka dialog yang sama: bila wadah sudah *Diterima* oleh
   orang lain, ia menetapkan *Layak* (tagihan terbit) atau *Tidak Layak* beserta alasan. Bila pesanan dokter belum
   punya wadah, dialog meminta jenis specimen dan mencatat diambil → diterima lebih dulu.
5. **Konfirmasi** dengan dokter pemeriksa; centang *Sampling diterima* hanya mencatat penerimaan, centang
   *Langsung proses* hanya aktif bila wadah sudah layak dan pembayaran lolos.
6. **Proses Pemeriksaan** — pasien Tunai wajib sudah lunas; asuransi/penjamin lolos.
7. **Hasil dan Riwayat** — halaman hasil per disiplin, Nota Lab, Label Lab, Label Goldar; validasi dan rilis
   empat mata tetap berlaku sebelum hasil resmi.

**Aturan bisnis.**

1. Kode Laboratorium tetap **nol** menulis kunjungan maupun pasien (`AC-45`); pendaftaran diteruskan ke Registrasi.
2. Satu Simpan Penerimaan = satu kategori Lab, sehingga tepat satu pesanan. Simpan ulang sesudah gagal di tengah
   melanjutkan dari langkah yang gagal, tidak membentuk kunjungan atau pesanan kedua.
3. Pengambil sampel tidak boleh menetapkan kelayakan wadahnya sendiri (`VAL-09`).
4. Kunci Lunas berlaku pada **Proses Pemeriksaan saja**, satuannya **kunjungan** (invoice per kunjungan).
   Terima Sampling tidak memeriksa pembayaran — tagihan baru terbit saat wadah layak (F26-5).
5. Metode pembayaran dipilih hanya ketika kunjungan baru dibentuk di layar Penerimaan; kunjungan kiosk/OTC
   menampilkannya baca-saja.

**Contoh.** Pasien Tunai Indra (No. RM samaran 00-00-00-16) mendaftar lewat kiosk tanpa pesanan → tiket `Q002`,
kunjungan `ENC-RSMMC-00178`. Petugas A memproses dari OTC, memilih Patologi Klinik → Hemoglobin (Rp 35.000), specimen
Blood, Simpan → pesanan `LAB-RSMMC-000023`, wadah *Diterima*. Petugas A membuka Terima Sampling → ditolak: *"Specimen ini
diambil oleh Anda sendiri"*. Petugas B menyatakan Layak → kolom Pembayaran *Belum Lunas · Rp 35.000*; Proses
Pemeriksaan nonaktif; panggilan langsung `start-process` dijawab `409` *"pembayaran belum lunas (sisa Rp 35.000)"*.
Sesudah kasir menerima Rp 35.000, kolom menjadi *Lunas* dan Proses dapat ditekan.

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Simpan Penerimaan / Terima Sampling (pesanan dokter tanpa wadah) | Wadah `Planned` → `Collected` → `Received` | Pemegang `LabSpecimen : Plan/Collect/Receive` | Jenis specimen dipilih |
| Wadah `Received` | Layak | Wadah `Accepted`; pesanan `Requested`/`Confirmed` → `Accepted` | Pemegang `LabSpecimen : Accept` | Bukan pengambil sampel (`VAL-09`) |
| Wadah `Received` | Tidak Layak | Wadah `Rejected`; pemeriksaannya gugur | Pemegang `LabSpecimen : Accept` | Bukan pengambil sampel; alasan dari daftar terkendali |
| Pesanan `Requested`/`Accepted` | Konfirmasi | `Confirmed` / tetap `Accepted` | `LabOrder : Confirm` | Dokter pemeriksa dipilih; sekali saja |
| Pesanan `Accepted` terkonfirmasi | Proses Pemeriksaan | `InProcess` | `LabOrder : Process` | Tunai: lunas; asuransi/penjamin: lolos |

**Jalur tidak normal.**

| Keadaan | Perilaku |
|---|---|
| Kiosk gagal membaca pesanan Lab | Diperlakukan *tidak ada pesanan* → pendaftaran; pasien tidak tertahan |
| Unit Laboratorium tidak ditemukan | Kiosk berhenti di Tujuan Layanan dengan pesan meminta bantuan petugas; Penerimaan menampilkan pemilih unit |
| Simpan gagal di langkah wadah | Kunjungan/pesanan yang sudah terbentuk dipertahankan; Simpan ulang melanjutkan |
| Tagihan belum terbit | Proses nonaktif: *"Tagihan pemeriksaan belum terbit"* |
| Billing mengubah rumus pelunasan | Risiko simpangan jalur sementara `LAB-DEC-220` — tercatat `LAB-OPEN-053` |

**Hasil akhir.** Pesanan berstatus `InProcess` dengan wadah layak, tagihan lunas atau dijamin, dan hasil yang
kemudian divalidasi dan dirilis empat mata; kunjungan kiosk yang tidak pernah dipesan tetap ditutup otomatis
Registrasi (`LAB-DEC-054`).

### Pertanyaan putaran ini

| Urutan | Butir | Pertanyaan | Keadaan |
|---|---|---|---|
| Q0 | — | Batas scope | ✅ *Setuju* — `LAB-DEC-212` |
| Q1 | F26-7, `LAB-REQ-008` | Jalur baca Lunas sebelum Billing menjawab | ✅ *Sementara, lapor Billing* — `LAB-DEC-220` |
| Q2 | F26-8, `LAB-FE-011` | Metode pembayaran di layar Penerimaan | ✅ *Pilihan saat daftar* — `LAB-DEC-221` |
| Q3 | `LAB-DEC-052`, `LAB-REQ-006` | Status persetujuan amandemen kiosk milik Registrasi | ✅ *Approved + kabari Andry* — `LAB-DEC-213` |

Enam keputusan `LAB-EVD-013` (2026-10-07/08) dibukukan sebagai `LAB-DEC-213`..`LAB-DEC-219`.

### Bahan audit untuk `/trace-existing-capabilities` atau desain

1. Amandemen `LAB-API-v1`, `LAB-PERM-v1`, dan `LAB-STATE-v1` atas F26-9 — endpoint dan ruas baru sudah berjalan
   sebelum tercatat di kontrak.
2. Apakah panel *Menunggu dari Kiosk* (`FE-LAB-14`) masih diperlukan: pasien jalur pendaftaran kini pindah ke Daftar
   Pasien OTC karena sesinya terpakai kunjungan; yang tersisa hanya pasien Konfirmasi Kehadiran.
3. Layar Penerimaan menyembunyikan pembayaran pasien OTC, padahal `LAB-DEC-221` meminta tampilan baca-saja.
4. Uji Proses Pemeriksaan sesudah pelunasan kasir — belum dijalankan sungguhan.

> **Hasil putaran:** `LAB-DEC-212`..`LAB-DEC-221` beserta BR-141; `LAB-FE-036`; `AC-302`..`AC-313`. **Diamandemen:**
> `LAB-DEC-044`, `LAB-DEC-045`, `LAB-DEC-052`, `LAB-DEC-060`, `LAB-DEC-062`, `LAB-DEC-189`, `LAB-FE-010`, `LAB-FE-011`,
> `LAB-FE-030`. **Digantikan (`superseded`):** `LAB-DEC-191`. **Ditutup:** `LAB-OPEN-051`; butir tunda `LAB-DEC-188`
> *Daftar Pasien OTC*, *langsung proses*, *kunci Lunas*, *menu Hasil dan Riwayat*. **Tetap berlaku:** `AC-45`.
> **Dibuka:** `LAB-OPEN-053`, `LAB-REQ-018`. Langkah berikutnya: `/design-business-module` untuk amandemen kontrak
> (F26-9).

## Amendment Pass Putaran 27 — Lingkup kunci Lunas Proses Pemeriksaan (2026-10-08)

**Sumber:** closure question `Q-LAB-P26-01` dari capability map revision 8 (`LAB-CONFLICT-018`). Kunci Lunas
`LAB-DEC-218` berbunyi "pasien Tunai wajib lunas" tanpa batas jenis kunjungan. Impact scan 2026-10-08 menemukan
akibatnya bagi dua jenis kunjungan yang belum pernah dibahas putaran 26: rawat inap dan IGD. Tiga pertanyaan
diajukan kepada pemilik modul, **seluruhnya dijawab dengan pilihan yang direkomendasikan**.

Fakta dibaca pada backend `f4e79c4e` (branch `yoga`) + working tree dan frontend `f6f23352e` (branch `YogaV2`),
capability map revision 8.

### Batas scope putaran ini — `LAB-DEC-222`

Disetujui pemilik modul 2026-10-08 (*Sesuai*).

| Di dalam scope | Di luar scope |
|---|---|
| Jenis kunjungan mana yang wajib lunas sebelum Proses Pemeriksaan | Cara Billing menagih pemeriksaan Lab pasien IGD — milik `billing-kasir`, diteruskan sebagai `LAB-COORD-020` |
| Tampilan kolom Pembayaran bagi pasien yang dikecualikan | Kapan pasien rawat inap melunasi tagihan — milik Rawat Inap/Billing (`RWI-DEC-192`, `RWI-DEC-195`) |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F27-1 | `start-process` menilai Lunas dari `RegPatientEncounter.PaymentType` saja, **tanpa membedakan jenis kunjungan** | `LabOrderService.cs#StartProcessAsync:1356-1374` |
| F27-2 | Sejak `BE-RWI-155`, tagihan Lab pasien **rawat inap** terbit saat wadah Layak ke invoice kunjungan itu sendiri (RANAP), yang lazimnya dilunasi saat pasien pulang | `BillingClinicalChargeBridgeService.cs:135-140` |
| F27-3 | Jembatan Billing menolak kunjungan **IGD** (`NotApplicable`, *"bukan Rawat Jalan maupun Rawat Inap"*), sehingga tagihan Lab IGD tidak pernah terbit dan statusnya *Belum Ditagih* selamanya. Penolakan ini sudah ada sebelum `BE-RWI-155` | Sama |
| F27-4 | Jenis kunjungan di sistem: `Unknown` (0), `Outpatient` (1), `Emergency` (2), `Inpatient` (3), `MedicalCheckup` (4), `Telemedicine` (5). Kunjungan kiosk tercatat `Outpatient` | `RegistrationManagement/Enums/EncounterType.cs`; `RegPatientEncounter.cs:83` |
| F27-5 | Di devYoga belum ada pesanan Lab rawat inap atau IGD: 23 pesanan, 21 baris daftar pantau, seluruhnya rawat jalan Tunai | Capability map revision 8, bagian C |

### Pertanyaan putaran ini

| Urutan | Pertanyaan | Jawaban | Keputusan |
|---|---|---|---|
| Q0 | Batas scope | *Sesuai* | `LAB-DEC-222` |
| Q1 | Kunci Lunas berlaku untuk siapa | *Rawat jalan & kiosk saja* — rawat inap dan IGD dikecualikan | `LAB-DEC-223` |
| Q2 | MCU, Telemedicine, dan Unknown | *Tetap dikunci* | `LAB-DEC-223` |
| Q3 | Tampilan baris yang dikecualikan | *Status baru 'Ditagih Kemudian'* | `LAB-DEC-224` |

### Aturan yang berlaku sesudah putaran ini

| Jenis kunjungan | Pasien berpenjamin (asuransi/perusahaan) | Pasien Tunai |
|---|---|---|
| Rawat jalan (termasuk kiosk), MCU, Telemedicine, `Unknown`, dan jenis baru di masa depan | *Asuransi / Penjamin* — lolos | **Wajib lunas** — *Lunas* lolos; *Belum Lunas* `VAL-152`; *Belum Ditagih* `VAL-153` |
| **Rawat inap** | *Asuransi / Penjamin* — lolos | ***Ditagih Kemudian*** — lolos; sisa tagihan tetap ditampilkan bila tagihannya sudah terbit |
| **IGD** | *Asuransi / Penjamin* — lolos | ***Ditagih Kemudian*** — lolos; tanpa nominal selama Billing belum menagih Lab IGD (`LAB-COORD-020`) |

**Contoh.**

> Pasien Umum dirawat di bangsal. Dokter bangsal memesan Darah Lengkap; sampel dinilai Layak dan Billing menerbitkan
> Rp 85.000 ke invoice RANAP. Di Daftar Pasien Patologi Klinik kolom Pembayaran berbunyi *Ditagih Kemudian — sisa
> Rp 85.000*, tombol Proses Pemeriksaan aktif, dan `start-process` menjawab `200`. Pasien melunasi seluruh tagihan
> saat pulang. Sebaliknya, pasien Umum yang datang lewat kiosk dengan tagihan yang sama tetap ditolak `409` sampai
> membayar di kasir.

### Bahan audit untuk desain

1. Amandemen `LAB-API-v1` — nilai baru `Deferred` pada `paymentStatus` daftar pantau; `isPaymentCleared` = `true`.
2. Amandemen `LAB-VAL-v1` — `VAL-152` dan `VAL-153` hanya berlaku bagi kunjungan selain `Inpatient` dan `Emergency`.
3. Task backend kecil pada `LabPaymentClearanceRules` (satu penjaga jenis kunjungan) dan task frontend kecil (label
   *Ditagih Kemudian* pada kolom Pembayaran dan petunjuk tombol Proses).

> **Hasil putaran:** `LAB-DEC-222`..`LAB-DEC-224`; `AC-314`..`AC-317`. **Diamandemen:** `LAB-DEC-218`.
> **Ditutup sisi Laboratorium:** `LAB-CONFLICT-018`. **Dibuka:** `LAB-COORD-020`. Langkah berikutnya:
> `/design-business-module` untuk amandemen kontrak.

## Amendment Pass Putaran 28 — CITO dari pemesanan hilang saat wadah direncanakan (2026-10-09)

**Sumber:** cacat as-is yang dicatat `02-backend-architecture.md` 28.8 saat merancang Beranda (usulan
`LAB-CONFLICT-019`, belum pernah dibukukan). Dokter atau kiosk memilih CITO per pemeriksaan saat memesan, tetapi
tanda itu tidak pernah sampai ke pemeriksaan yang dikerjakan petugas. Karena menyentuh prioritas klinis dan perilaku
lima layar, perbaikannya wajib diputuskan pemilik modul — tidak boleh dikerjakan diam-diam di task Beranda. Batas
scope dan lima pertanyaan diajukan, **seluruhnya dijawab dengan pilihan yang direkomendasikan**.

Fakta dibaca pada backend `e6c0e451` dan frontend `8f3d74d18` (keduanya bersih, sudah di-commit pengguna).
Capability map revision 8 diaudit pada BE `f4e79c4e`/FE `f6f23352e` — **berpotensi basi**; fakta putaran ini
dibaca langsung dari source sehingga tidak bergantung pada map.

### Batas scope putaran ini — `LAB-DEC-225`

Disetujui pemilik modul 2026-10-09 (*Setuju*).

| Di dalam scope | Di luar scope |
|---|---|
| Membawa tanda CITO dari permintaan ke pemeriksaan saat wadah direncanakan — wadah pertama dan wadah pengganti | Siapa yang boleh *Tandai Cito* atau mencabutnya — tetap `VAL-03`/`VAL-04`, tidak dibuka ulang |
| Jejak siapa/kapan pada pemeriksaan yang lahir CITO | Batas waktu cito per pemeriksaan (`LabCitoTurnaroundPolicy`) |
| Perbaikan data lama yang terlanjur Routine | Aturan pemesanan di modul dokter/kiosk — titik sentuhnya hanya `citoExaminations` |
| | Billing — kesegeraan tidak mengubah tagihan |

### Fakta yang dibaca (bukan keputusan)

| ID | Fakta | Bukti |
|---|---|---|
| F28-1 | Saat wadah direncanakan, setiap pemeriksaan dibuat dengan `Urgency = Routine` **tanpa membaca** permintaan yang dipenuhinya | `LabSpecimenService.cs#CreateExaminationsAsync:224-264` (baris 249) |
| F28-2 | Pilihan CITO saat memesan tersimpan pada permintaan: `LabOrderedProcedure.Urgency = Cito` bila prosedurnya ada di `citoExaminations` | `LabOrderService.cs:1080`, `:1171-1172` |
| F28-3 | Wadah pengganti (ambil ulang) menyalin seluruh pemeriksaan wadah lama lewat method yang sama, sehingga CITO hasil *Tandai Cito* dokter juga **kembali Routine** | `LabSpecimenService.cs` ±897-930 (`RequestRecollection`) |
| F28-4 | *Tandai Cito* hanya boleh oleh dokter pemesan (`VAL-03`), tidak pada pesanan *Selesai*/*Dibatalkan* (`VAL-04`), dan tidak pada pemeriksaan *Gugur*/*Dibatalkan*; menulis `UrgencyMarkedAt`/`UrgencyMarkedByUserId` serta riwayat `Examination.SetUrgency` | `LabExaminationService.cs#SetUrgencyAsync:353-410`, `:514-527` |
| F28-5 | Jam keterlambatan cito dihitung sejak wadah *Layak* (`ChargeEligibleAt`) + batas waktu prosedur — **bukan** sejak tanda dipasang | `LabWorklistService.cs#GetCitoOverdueAsync:108-170` |
| F28-6 | Pembaca `LabExamination.Urgency`: lencana dan saringan *Hanya cito* daftar pantau, Daftar Kerja, Pantau Keterlambatan Cito, laporan TAT cito, label cetak. Hanya kartu CITO Beranda yang membaca dua sumber | `LabMonitoringService.cs:124,359`; `LabWorklistService.cs:57`; `LabOperationalReportService.cs:320,385`; `lab-print-rules.js:95`; `LabDashboardService.cs:334-340` |
| F28-7 | Sesudah wadah dibuat, permintaan ditautkan ke pemeriksaan terbaru lewat `FulfilledExaminationId` — termasuk pemeriksaan di wadah pengganti | `LabSpecimenService.cs#MarkOrderedProceduresFulfilledAsync:347-379` |
| F28-8 | DB dev (kueri baca-saja 2026-10-09): 13 permintaan, **seluruhnya Routine**; 2 pemeriksaan bertanda CITO lewat *Tandai Cito*; **0 baris** memenuhi syarat perbaikan data lama. Jumlah di produksi belum diketahui | Konsol Npgsql `default_transaction_read_only=on` |

### Pertanyaan putaran ini

| Urutan | Pertanyaan | Jawaban | Keputusan |
|---|---|---|---|
| Q0 | Batas scope | *Setuju* | `LAB-DEC-225` |
| Q1 | Pemeriksaan yang terbentuk dari permintaan CITO | *Ikut CITO otomatis* | `LAB-DEC-226` |
| Q2 | Kesegeraan pemeriksaan di wadah pengganti | *Keadaan terakhir pemeriksaan lama* | `LAB-DEC-227` |
| Q3 | Penanda (oleh siapa, kapan) pada pemeriksaan yang lahir CITO | *Dibiarkan kosong* | `LAB-DEC-226` |
| Q4 | Data lama yang diperbaiki saat rilis | *Hanya pesanan yang masih berjalan* | `LAB-DEC-228` |
| Q5 | Jejak perbaikan data lama | *Satu baris riwayat per pemeriksaan* | `LAB-DEC-228` |

### Aturan yang berlaku sesudah putaran ini

| Peristiwa | Kesegeraan pemeriksaan | Penanda (`UrgencyMarkedAt`/`ByUserId`) |
|---|---|---|
| Wadah pertama direncanakan; permintaannya CITO | **Cito** | Kosong — asal CITO ditelusuri lewat permintaan yang tertaut |
| Wadah pertama direncanakan; permintaannya biasa, atau pesanan lama tanpa baris permintaan | Routine (seperti hari ini) | Kosong |
| Dokter menekan *Tandai Cito* / mencabutnya | Mengikuti pilihan dokter (tidak berubah) | Dokter dan waktunya (tidak berubah) |
| Sampel diambil ulang | **Sama dengan pemeriksaan yang digantikan, keadaan terakhirnya** | **Disalin** dari pemeriksaan yang digantikan |
| Rilis — pemeriksaan lama yang terlanjur Routine | Lihat `LAB-DEC-228` | Tetap kosong; satu baris riwayat |

**Contoh.**

> dr. Arif memesan Hemoglobin **CITO** dan Leukosit biasa untuk pasien IGD. Analis Gilang merencanakan wadah:
> Hemoglobin lahir **Cito** dengan penanda kosong, Leukosit Routine. Di Daftar Pasien Patologi Klinik baris pasien
> berlencana CITO dan muncul pada saringan *Hanya cito*; label tabung Hemoglobin tercetak CITO; Daftar Kerja
> menaruh Hemoglobin di atas pemeriksaan biasa. Wadah dinilai *Layak* pukul 10.00 dengan batas cito 60 menit —
> pukul 11.05 Hemoglobin muncul di Pantau Keterlambatan Cito.
>
> Sore harinya sampel ditolak karena hemolisis dan diambil ulang. Sebelumnya dr. Arif menandai Leukosit CITO lewat
> *Tandai Cito*. Di wadah pengganti **keduanya** Cito; penanda Leukosit (dr. Arif, 13.20) ikut tersalin, penanda
> Hemoglobin tetap kosong.

### Bahan audit untuk desain

1. Amandemen `02-backend-architecture.md` 28.8 dari *belum diputuskan* menjadi to-be, dan catatan perilaku pada
   kontrak (`LAB-API-v1`): nilai `urgency` pemeriksaan kini dapat lahir Cito tanpa `urgencyMarkedAt`. Tidak ada
   ruas atau endpoint baru.
2. Task backend kecil: `CreateExaminationsAsync` membaca `LabOrderedProcedure.Urgency` (wadah pertama) atau
   pemeriksaan yang digantikan (wadah pengganti); skrip perbaikan data lama yang idempoten beserta hitung kering
   sebelum dijalankan di produksi.
3. **Nol task frontend wajib** — kelima layar membaca `LabExamination.Urgency` (F28-6). Verifikasi peramban tetap
   dibutuhkan untuk `AC-318`..`AC-320`.

> **Hasil putaran:** `LAB-DEC-225`..`LAB-DEC-228`; `AC-318`..`AC-322`. **Dibuka dan ditutup:**
> `LAB-CONFLICT-019`. Langkah berikutnya: `/design-business-module` untuk amandemen 28.8 dan catatan kontrak.

## Amendment Pass Putaran 29 — Rencana ulang sesudah wadah dibatalkan (temuan T1 `BE-LAB-94`, 2026-10-09)

**Sumber:** temuan T1 laporan [`BE-LAB-94`](task/report/backend/BE-LAB-94.md). Wadah yang **dibatalkan** lalu direncanakan
ulang lewat `PlanAsync` membaca kesegeraan dari **permintaan** (rancangan 29.2), bukan dari pemeriksaan pada wadah yang
dibatalkan. *Tandai Cito* atau pencabutan dokter sebelum wadahnya dibatalkan tidak terbawa — berbeda dari ambil ulang
(`LAB-DEC-227`). Satu pertanyaan, dijawab dengan pilihan yang direkomendasikan. Batas scope sama dengan `LAB-DEC-225`.

| ID | Fakta | Bukti |
|---|---|---|
| F29-1 | Membatalkan wadah **tidak** membatalkan pemeriksaan di dalamnya; permintaan tetap `Fulfilled`; `VAL-69` membuka perencanaan ulang begitu wadahnya `Cancelled` | `LabSpecimenService.cs#EnsureOrderedProcedureGuardAsync` |
| F29-2 | Pemeriksaan yang lahir **sebelum** `BE-LAB-94` bisa `Routine` tanpa penanda walau permintaannya Cito (cacat `LAB-CONFLICT-019`) | F28-1 |

| Urutan | Pertanyaan | Jawaban | Keputusan |
|---|---|---|---|
| Q1 | Rencana ulang sesudah wadah dibatalkan — kesegeraan dari mana | *Samakan: keadaan terakhir* | `LAB-DEC-229` |

**Contoh.** dr. Arif menandai Leukosit CITO pukul 13.20. Wadahnya dibatalkan karena salah tabung, lalu analis
merencanakan wadah baru: Leukosit lahir **Cito** dengan penanda dr. Arif 13.20 — sama seperti bila sampelnya diambil ulang.

> **Hasil putaran:** `LAB-DEC-229`; `AC-323`..`AC-325`. Tafsir untuk pemeriksaan pendahulu **tanpa penanda** diajukan
> sebagai butir persetujuan `LAB-REQ-023` (bukan keputusan putaran ini). Langkah berikutnya: `/design-business-module`.

## State dan Transition

Kerangka mengikuti `LAB-INH-001`, `LAB-INH-002`, dan `LAB-INH-003`. Bagian yang ditambahkan
sesi ini adalah kewenangan pada alur **hasil**.

### Alur hasil pemeriksaan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Pemeriksaan mulai dikerjakan | `Pending` | Sistem | Sampel sudah `Accepted` |
| `Pending` | Mulai mengerjakan | `InProcess` | Analis | Sampel masih layak |
| `InProcess` | Menyimpan angka hasil | `Completed` | Analis | Seluruh nilai wajib terisi |
| `Completed` | Memvalidasi | `Validated` | Petugas berwenang validasi, **bukan** pengisi hasil | BR-01 terpenuhi atau pengecualian tercatat |
| `Validated` | Merilis | `Released` | Petugas berwenang rilis | Bila nilai kritis, formulir pelaporan BR-02 wajib muncul |
| `Released` | Mengoreksi | `Corrected/Amended` | Petugas berwenang validasi atau rilis. Analis biasa **tidak boleh** | Riwayat rilis lama tetap disimpan (`LAB-INH-003`, BR-05) |
| `Corrected/Amended` | Memvalidasi ulang | `Revalidated` | Petugas berwenang validasi | BR-01 tetap berlaku |
| `Revalidated` | Merilis ulang | `Released` | Petugas berwenang rilis | Versi lama tetap terlihat bertanda "sudah diperbaiki", dan dokter pemesan otomatis diberi tahu (BR-05) |

Status hasil di atas berlaku **per pemeriksaan**, bukan per pesanan (BR-06). Dalam satu
pesanan, Kalium boleh sudah `Released` sementara Hemoglobin masih `InProcess`.

---

## Skenario Normal dan Exception

### Skenario normal — pemeriksaan Hemoglobin rawat jalan

1. **Tujuan:** dokter memperoleh angka Hemoglobin pasien untuk menentukan terapi.
2. **Pelaku:** dr. Rina (pemesan), perawat Dewi (pengambil sampel), petugas penerimaan lab
   Budi, analis Sari (pengisi hasil), penanggung jawab teknis Tono (validator dan perilis).
3. **Pemicu:** dr. Rina memesan pemeriksaan Hemoglobin saat pasien Andi berkunjung.
4. **Prasyarat:** pasien Andi sudah punya kunjungan (*encounter*) aktif dari Registrasi.
5. **Langkah utama:**
   1. dr. Rina membuat pesanan lab. Status pesanan `Requested`.
   2. Perawat Dewi mengambil darah dan memindai barcode tabung. Sampel `Collected`.
   3. Sampel diantar ke lab. Petugas Budi memindai barcode. Sampel `Received`.
   4. Budi memeriksa kelayakan sampel dan menyatakan layak. Sampel `Accepted`.
      **Pada titik ini pemeriksaan sah untuk ditagihkan** (`LAB-INH-009`).
   5. Analis Sari mengerjakan pemeriksaan, lalu mengetik hasil 9,4 g/dL. Hasil `Completed`.
   6. Tono memvalidasi hasil. Hasil `Validated`. BR-01 terpenuhi karena Tono bukan Sari.
   7. Tono merilis hasil. Hasil `Released`. dr. Rina dapat membacanya.
6. **Hasil akhir:** dr. Rina melihat Hemoglobin 9,4 g/dL, Billing menerima fakta bahwa
   pemeriksaan Hemoglobin sah ditagihkan sejak langkah 4.

### Jalur tidak normal

| Kejadian | Yang terjadi di sistem | Acuan |
|---|---|---|
| Sampel darah menggumpal saat diperiksa kelayakannya | Budi menolak sampel dengan alasan terkendali. Sampel `Rejected`. Secara *default* tidak ada tagihan pemeriksaan | `LAB-INH-009` |
| Sampel harus diambil ulang karena tabung pecah oleh petugas lab | Sebab diisi `InternalHospitalError`. Sampel baru dibuat, sampel lama tetap terlihat dan tertaut. Pasien **tidak** ditagih dua kali | `LAB-INH-005`, `LAB-INH-011` |
| Analis shift malam sendirian dan harus memvalidasi hasilnya sendiri | Diizinkan dengan alasan tercatat dan penanda permanen pada lembar hasil | BR-01 |
| Hasil Kalium 7,2 mmol/L (kritis) | Hasil tetap dirilis, formulir pelaporan wajib diisi, masuk daftar pantau bila belum dilaporkan | BR-02 |
| dr. Rina ingin membatalkan pesanan setelah sampel `Accepted` | Dokter hanya bisa **mengajukan** pembatalan. Lab yang memproses. Tagihan tidak otomatis hilang; Billing yang memutuskan | `LAB-INH-006`, `LAB-INH-012` |
| Hasil sudah dirilis lalu ketahuan salah | Hanya petugas berwenang validasi/rilis yang boleh mengoreksi. Versi lama tetap terlihat bertanda "sudah diperbaiki", dokter pemesan otomatis diberi tahu | BR-05 |
| Kalium selesai lebih dulu, Hemoglobin masih diproses | Kalium langsung dirilis. Lembar hasil menampilkan peringatan "hasil belum lengkap" | BR-06 |
| Hasil di luar batas normal tetapi belum kritis, misalnya Kalium 5,3 mmol/L | Ditandai "di atas nilai rujukan". Formulir pelaporan nilai kritis **tidak** muncul | BR-04 |

---

## Frontend Decision Authority

Urutan wewenang yang berlaku, dari yang paling kuat:
keamanan/privasi/invariant → arahan produk/UI yang disetujui → konvensi project → keleluasaan
developer (`DEV_DISCRETION`).

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `LAB-FE-001` | Letak menu dan penamaan route | Konvensi project | `decided` | Wajib mengikuti pola modul Health Services yang sudah ada, misalnya `pharmacy-management` dan `inpatient-management`. Tidak boleh membuat pola penamaan baru | `LAB-DEC-010`; F5 menunjukkan frontend Laboratorium masih nol |
| `LAB-FE-002` | Tata letak layar, pemilihan tab/modal/drawer, warna, komponen | Developer | `DEV_DISCRETION` | Bebas selama mengikuti komponen dan gaya yang sudah dipakai modul lain | `LAB-DEC-010` |
| `LAB-FE-003` | Peringatan "hasil belum lengkap" pada lembar hasil | Invariant keselamatan | `decided` | **Wajib ada.** Bukan `DEV_DISCRETION`. Bentuk visualnya boleh dipilih developer, keberadaannya tidak boleh dihapus | BR-06 |
| `LAB-FE-004` | Penanda "divalidasi oleh pengisi sendiri" pada lembar hasil | Invariant keselamatan | `decided` | **Wajib terlihat** di layar dan di cetakan. Bentuk visualnya boleh dipilih developer | BR-01 |
| `LAB-FE-005` | Penandaan nilai kritis dan formulir pelaporannya | Invariant keselamatan | `decided` | **Wajib ada dan wajib menonjol.** Formulir pelaporan tidak boleh bisa dilewati begitu saja | BR-02 |
| `LAB-FE-006` | Urutan daftar kerja: pesanan cito di atas pesanan biasa | Invariant keselamatan | `decided` | **Wajib.** Aturan urutannya tidak boleh diserahkan pada selera tampilan | BR-07, BR-09 |
| `LAB-FE-007` | Kotak pemberitahuan dokter berisi nilai kritis dan koreksi hasil | Invariant keselamatan | `decided` | **Wajib ada.** Pemberitahuan yang belum dibaca harus terlihat jelas. Bentuk visual dan letaknya boleh dipilih developer | BR-08 |
| `LAB-FE-008` | Tombol atau penanda cito pada layar pembuatan pesanan | Konvensi project | `DEV_DISCRETION` | Bebas, asalkan hanya dokter pemesan yang bisa menandainya | BR-09 |
| `LAB-FE-009` | Pembedaan dua jalur masuk: menu `Penerimaan Sampling/Specimen` versus layar pesanan dokter | Arahan produk | `decided` — **diamandemen `LAB-DEC-048`** | **Wajib terbaca jelas** siapa memakai jalur yang mana, lewat penamaan menu dan dokumen pelatihan. Bentuk visualnya boleh dipilih developer. **Sejak 2026-09-14 layar pesanan dokter tidak lagi punya butir menu sendiri**; pembedaannya kini antara menu `Penerimaan Sampling/Specimen` dan ketiga menu `Pemeriksaan` per disiplin | BR-40, BR-43, `LAB-DEC-045`, `LAB-DEC-048` |
| `LAB-FE-012` | Cara membuka pesanan dari ketiga layar `Pemeriksaan` | Invariant keterjangkauan | `decided` | **Wajib ada dan wajib ditemukan pengguna.** Setelah butir menu Pesanan Laboratorium dicabut, inilah satu-satunya jalan ke detail pesanan. Bentuk aksinya boleh dipilih developer, tetapi bila memakai aksi yang tidak terlihat sebagai tombol, petunjuknya **wajib tertulis** pada layar | BR-43, `LAB-DEC-048` |
| `LAB-FE-013` | Tampilan disiplin pada formulir pesanan | Arahan produk | `decided` | **Wajib baca-saja** dan mengikuti pemeriksaan yang dipilih. Tidak boleh ada kotak pilihan disiplin. Wajib memberi tahu petugas menu Pemeriksaan mana yang akan memuat pasien ini | BR-43, `LAB-DEC-036` |
| `LAB-FE-010` | Kedudukan daftar pemeriksaan terhadap wadah pada layar penerimaan | Invariant operasional | `decided` — **diamandemen `LAB-DEC-214`/`LAB-DEC-219` 2026-10-08:** kelayakan tidak lagi ditetapkan di layar Penerimaan; wadah yang dinilai tampil di dialog Terima Sampling | **Wajib terlihat berdampingan** sebelum kelayakan ditetapkan, karena penguncian `LAB-DEC-039` jatuh di situ. Bentuk visualnya boleh dipilih developer | BR-40, BR-34 |
| `LAB-FE-011` | Tampilan metode pembayaran pada layar penerimaan | Batas wewenang | `decided` — **diamandemen `LAB-DEC-221` 2026-10-08:** pilihan Tunai/Asuransi saat kunjungan baru dibentuk; kunjungan yang sudah ada tetap baca-saja | **Wajib baca-saja.** Tidak boleh ada kotak pilihan, dan tidak boleh ada cara menimpanya. Saat Billing tidak terjawab, wajib menulis *belum dapat ditentukan*, bukan mengosongkannya | BR-39, `LAB-DEC-044` |
| `LAB-FE-015` | Penanda hasil rendah, tinggi, dan kritis — di layar dan di cetakan | Invariant keselamatan + arahan produk | `decided` — **mempersempit `LAB-FE-002`** untuk penanda ini | **Maknanya dikunci:** tiga tingkat dibedakan jelas, kritis paling menonjol (`LAB-FE-005`), dan penanda **wajib berupa huruf atau teks** — `L`, `H`, `KRITIS` — **tidak boleh hanya warna**. Warna persisnya mengikuti token desain (`--color-warning`, `--color-danger`) dan tetap `DEV_DISCRETION` | `LAB-DEC-145` butir 2; cetakan Patologi Klinik lapangan memakai huruf `H` (`LAB-EVD-005`) |
| `LAB-FE-017` | Letak isian hasil Patologi Klinik | Arahan produk | `decided` — **mempersempit `LAB-FE-002`** untuk isian hasil Patologi Klinik | **Halaman detail per order** berisi seluruh pemeriksaan Patologi Klinik yang tidak batal dalam satu tabel isian; Daftar Kerja tetap sebagai antrean dan membuka halaman itu; **nol** dialog modal pengisian hasil. Nama route mengikuti `LAB-FE-001`; tata letak di dalam halaman `DEV_DISCRETION` dalam batas `LAB-FE-015` dan `LAB-FE-016` | `LAB-DEC-149`; PRD `FR-PK-001`; capability map revision 5 `CAP-P14-18` |
| `LAB-FE-016` | Isian hasil utama pada halaman hasil Patologi Klinik dan Mikrobiologi | Arahan produk | `decided` — **mempersempit `LAB-FE-002`** untuk halaman ini | Nilai hasil, isolat, dan antibiogram diisi **di halaman**, **tidak** di jendela modal. Modal tetap boleh untuk konfirmasi dan isian pendek, misalnya alasan *Kembalikan ke analis*. Tata letak selebihnya tetap `DEV_DISCRETION` | `LAB-DEC-145` butir 3; PRD bagian 12 |
| `LAB-FE-030` | Keadaan aksi Proses Pemeriksaan, Selesaikan, dan Terima Sampling yang belum sah; dialog konfirmasi sebelum memanggil; bentuk pesan penolakan | Developer | `DEV_DISCRETION` — **dibatasi BR-136; diamandemen `LAB-DEC-216` 2026-10-08:** daftar kini memanggil endpoint wadah lewat dialog Terima Sampling | Mengikuti pola `FE-LAB-15`/`FE-LAB-16` pada daftar yang sama. Yang **tidak** boleh: Selesaikan tampil pada daftar Patologi Anatomi; Selesaikan dapat ditekan sebelum `AllReleased`; status baris berubah tanpa jawaban `200`; daftar memanggil endpoint wadah; aksi Tahan/Lanjutkan muncul | `LAB-DEC-189`..`LAB-DEC-191` |
| `LAB-FE-031` | Label, urutan, dan pengelompokan menu Laboratorium serta judul halaman yang dibukanya | Arahan produk | `decided` — **mengamandemen penamaan `LAB-DEC-048`** | Wajib persis BR-137. Route, key, dan izin menu tidak boleh berubah. Ikon tetap `DEV_DISCRETION` | `LAB-DEC-192` |
| `LAB-FE-032` | Subjudul dokumen Nota pada ketiga daftar pasien lab | Arahan produk | `decided` — rekomendasi capability map rev 7 diterima 2026-10-07 | Tetap `Pemeriksaan <disiplin>` dan **tidak** mengikuti judul halaman; dokumen cetak berada di luar `LAB-DEC-188` | Sudah ditegakkan `FE-LAB-49` |
| `LAB-FE-033` | Keadaan aksi Konfirmasi dan Proses Pemeriksaan sesudah BR-138 | Developer | `DEV_DISCRETION` — **dibatasi BR-138** | Mengikuti pola `FE-LAB-15`/`FE-LAB-47`: nonaktif beralasan. Yang **tidak** boleh: Konfirmasi dapat ditekan pada pesanan yang sudah dikonfirmasi atau sudah `InProcess`; Proses dapat ditekan pada pesanan yang belum dikonfirmasi; status baris berubah ke `Confirmed` saat mengonfirmasi pesanan `Accepted`; layar menebak status sebelum jawaban `200` | `LAB-DEC-193`..`LAB-DEC-197` |
| `LAB-FE-034` | Nilai awal pilihan dokter pemeriksa pada dialog Konfirmasi | Arahan produk | `decided` — Yoga Aji Pratama 2026-10-07, Q3 putaran 24 *Kosong, wajib dipilih* | Dialog dibuka dengan pilihan **kosong**; tidak diisi dokter pemberi instruksi, dokter pendaftaran, maupun pilihan terakhir. Bentuk kotak pilih dan pencarian tetap `DEV_DISCRETION` | BR-139 butir 4; `AC-291` |
| `LAB-FE-035` | Tata letak Beranda Lab mengikuti susunan v1 dengan komponen dan tema V2 | Arahan produk | `decided` — Yoga Aji Pratama 2026-10-07, putaran 25 (*"sesuaikan dengan tema yang sekarang ada"*) | **Urutan bagian wajib BR-140 butir 1.** Komponen dasar V2 (`Hero`, `SummaryCards`, `DataTable`, panel keadaan) dan token tema V2 dipakai; animasi, warna kartu, ikon, dan jenis grafik `DEV_DISCRETION` dalam tema V2. **Tidak** boleh: meniru gaya CSS v1 apa adanya, membuang rekap status, atau menampilkan data pasien selain BR-140 butir 6 | BR-140; `AC-294`, `AC-300` |
| `LAB-FE-036` | Susunan halaman Penerimaan Sampling, Daftar Pasien OTC, dialog Terima Sampling, dan menu Hasil dan Riwayat mengikuti v1 dengan komponen dan tema V2 | Arahan produk | `decided` — Yoga Aji Pratama 2026-10-07 (*"pastikan sama proses alur UI dan UX-nya, tetapi … tema tampilan seperti sekarang"*) | **Urutan wilayah dan aksi wajib BR-141.** Komponen dasar V2 (`Hero`, `DataFilter`, `DataTable`, `RowActionMenu`, `ConfirmModal`, `BaseCheckboxCard`). Bentuk visual lain `DEV_DISCRETION`. Yang **tidak** boleh: menetapkan kelayakan di layar Penerimaan; mengunci Batal pada dialog | `LAB-DEC-213`..`LAB-DEC-219` |

Catatan: butir bertanda `DEV_DISCRETION` boleh diputuskan developer. Butir bertanda `decided`
dengan alasan keselamatan **tidak boleh** dihapus atau diperlemah oleh keputusan tampilan.

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `LAB-FACT-001` | Fact | Backend Laboratorium sudah punya pesanan, sampel, alasan penolakan, dan riwayat transisi | — | `fact` | — | F1, F2, F3 pada SHA `c87d9c0` |
| `LAB-FACT-002` | Fact | Hasil pemeriksaan, nilai kritis, nilai rujukan, panel, worklist, integrasi alat, dan TAT belum ada di backend | — | `fact` | — | F4 |
| `LAB-FACT-003` | Fact | Frontend belum punya modul Laboratorium sama sekali | — | `fact` | — | F5 pada SHA `c79bb6ee4` |
| `LAB-FACT-004` | Fact | Katalog pemeriksaan lab masih menumpang `MstProcedure.IsLaboratory` tanpa atribut khas lab | — | `fact` | — | F6 |
| `LAB-FACT-005` | Fact | Siklus hidup pesanan, sampel, hasil, dan titik kelayakan tagih sudah dikunci di `RJ-BIL-GATE-DEC-003` | Billing + Clinical Governance | `locked-draft` | Approval SHA-256 `4d4447...`; tanda tangan formal Lab masih `OPEN` | `rawat-jalan/00-interview-decisions.md` |
| `LAB-DEC-001` | Decision | **Rilis 1 = menyelesaikan rantai sampai hasil dirilis.** Isinya: pengisian hasil, verifikasi, validasi, rilis hasil, nilai kritis, daftar kerja petugas, penyajian hasil ke dokter dan pasien, serta seluruh tampilan frontend dari nol. Katalog pemeriksaan sementara tetap memakai `MstProcedure`. Katalog lab mandiri ditunda ke Rilis 2 | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Jawaban wawancara sesi ini; alasan: rantai pesanan-sampel sudah berjalan tetapi mati di ujung, sehingga modul belum berguna bagi dokter maupun pasien |
| `LAB-DEC-002` | Decision | **Cakupan modul dibatasi pada Patologi Klinik saja** — darah, urin, feses, kimia klinik, hematologi, imunologi. Mikrobiologi, Patologi Anatomi, dan Bank Darah dikeluarkan menjadi modul atau slice terpisah | Pemilik proses Laboratorium | `superseded` oleh `LAB-DEC-025` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Jawaban wawancara sesi ini; alasan: alur sampel yang sudah dikodekan memang cocok untuk pola hasil sekali jadi |
| `LAB-SCOPE-001` | Open Question | Apa batas resmi modul Laboratorium untuk rilis pertama? | Pemilik proses Laboratorium | `superseded` | Digantikan `LAB-DEC-001` dan `LAB-DEC-002` | Pertanyaan pembuka sesi ini |
| `LAB-DEC-003` | Decision | **Prinsip empat mata pada validasi hasil.** Pengisi hasil tidak boleh memvalidasi dan merilis hasil yang sama. Pengecualian diizinkan dengan alasan terkendali, penanda permanen "divalidasi oleh pengisi sendiri", dan jejak audit | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **DITANDATANGANI KLINIS 2026-09-17** — `LAB-DEC-079`, per disiplin oleh `DR-LAB-001`, `DR-LAB-002`, dan `DR-LAB-003` | Lihat BR-01. **Bunyi aturannya TIDAK berubah**: ketiganya menyetujui naskah apa adanya, nol perubahan dinyatakan. Yang per disiplin adalah **wewenangnya**, bukan isinya — lihat `LAB-DEC-079` dan `LAB-OPEN-034` |
| `LAB-DEC-004` | Decision | **Nilai kritis tetap dirilis, pelaporan wajib tercatat.** Pemeriksaan belum tuntas sampai catatan pelapor, penerima, waktu, sarana, dan bukti pembacaan ulang terisi. Ada daftar pantau nilai kritis yang belum dilaporkan | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **DITANDATANGANI KLINIS 2026-09-17** — `LAB-DEC-079`, per disiplin oleh `DR-LAB-001`, `DR-LAB-002`, dan `DR-LAB-003` | Lihat BR-02. **Bunyi aturannya TIDAK berubah.** Tanda tangan ini mengesahkan **kerangkanya** — nilai kritis tetap dirilis dan pelaporannya wajib tercatat. Ia **TIDAK** mengisi daftar nilai kritisnya: `LAB-P0-004` (alur lengkap, batas waktu, eskalasi) dan `LAB-OPEN-014` (nilai kritis mikrobiologi dan patologi anatomi) **tetap terbuka**, dan keduanya memang diajukan terpisah pada `LAB-REQ-004` bagian 6 |
| `LAB-DEC-005` | Decision | **Hasil Rilis 1 diketik manual oleh analis.** Sambungan otomatis ke alat laboratorium ditunda, dan struktur data Rilis 1 tidak menyiapkan tempat untuk hasil dari alat | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-03 |
| `LAB-RISK-001` | Assumption | Karena `LAB-DEC-005` tidak menyiapkan tempat data asal hasil, penyambungan alat di kemudian hari akan memerlukan perubahan struktur data hasil, dan riwayat hasil lama tidak dapat membedakan hasil ketikan dari hasil kiriman alat | Pemilik proses Laboratorium | `draft` | Diberitahukan pada sesi ini, pengguna tetap memilih pengetikan manual murni | Konsekuensi langsung `LAB-DEC-005` |
| `LAB-OPEN-001` | Open Question | Siapa pemilik modul Laboratorium yang berwenang menyetujui keputusan klinis dan operasional? | Manajemen rumah sakit | `closed` | Ditutup 2026-09-01: **Yoga Aji Pratama** ditetapkan sebagai pemilik modul | Pernyataan pengguna pada sesi 2026-09-01 |
| `LAB-FACT-006` | Fact | Platform belum punya sarana notifikasi umum maupun pemberitahuan tersimpan. Yang ada hanya SignalR `QueueHub` khusus antrean | — | `fact` | — | F7 pada SHA `c87d9c0` |
| `LAB-FACT-007` | Fact | **Kedua dokumen tata kelola backend ditemukan dan masih berlaku.** Sumber canonical lintas vendor: `QuilvianEngineeringSkills/agents/rules/backend/engineering/`. Edisi Claude: `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/`. Kedua salinan identik byte-per-byte (md5 `ad549762…` untuk kontrak, `6d11c0de…` untuk registry), tercommit pada `59bd3e2` di `DevBenari/QuilvianEngineeringSkills` | — | `fact` | — | Menutup `LAB-OPEN-002` |
| `LAB-FACT-008` | Fact | **`AGENTS.md` backend bertentangan dengan dirinya sendiri.** Baris 11 dan 20 masih menunjuk `docs/engineering/…`, sedangkan baris 40 menunjuk `rules/backend/engineering/…` dan menyatakan repository ini “tidak lagi memiliki folder `agents/rules/`”. Folder `agents/rules/` tetap ada di working tree berisi 7 berkas — persis peninggalan tercabut yang `AGENTS.md` perintahkan untuk dilaporkan | Pemilik repository backend | `fact` | — | `AGENTS.md:11`, `AGENTS.md:20`, `AGENTS.md:40`, `AGENTS.md:53` |
| `LAB-FACT-009` | Fact | **Rules root yang benar-benar terpasang tidak memuat kedua dokumen itu.** `AGENTS.md` menetapkan rules root Claude Code di `${CLAUDE_PLUGIN_ROOT}/.claude/rules/`. Plugin terpasang `quilvian-engineering-skills@quilvian` versi `0.1.0` hanya punya `rules/backend/` tanpa subfolder `engineering/`, dan tanpa `GLOBAL_RULES.md`. Marketplace terpasang menunjuk `MHamzah1/QuilvianEngineeringSkillsClaude` pada `f0136df` — repository yang **berbeda** dari sumber canonical `DevBenari/QuilvianEngineeringSkills` | Pemilik repository backend | `fact` | — | `~/.claude/plugins/installed_plugins.json`, `~/.claude/plugins/known_marketplaces.json`, `git ls-tree` pada klon marketplace |
| `LAB-FACT-010` | Fact | **Registry mencatat `HealthServices / LaboratoryManagement / Laboratory`, prefix `Lab`, lifecycle `PLANNED`.** Prefix `Lab` = *Laboratory* sudah terdaftar sehingga hak penamaan sudah ada, tetapi registry secara eksplisit menyatakan persetujuan registry **tidak** memberi wewenang implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul berstatus `PLANNED` | — | `fact` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris tabel Laboratorium dan paragraf 3 |
| `LAB-CONFLICT-001` | Conflict | `LAB-DEC-001` menunda katalog lab ke Rilis 2, tetapi `LAB-DEC-004` mewajibkan pengenalan nilai kritis yang butuh batas nilai. `MstProcedure` tidak punya kolomnya | Pemilik proses Laboratorium | `resolved` | Diselesaikan 2026-09-01 oleh `LAB-DEC-006` | F6 dan tabrakan antara `LAB-DEC-001` dan `LAB-DEC-004` |
| `LAB-DEC-006` | Decision | **Tabel batas nilai ditarik maju ke Rilis 1.** Isinya satuan hasil, batas normal bawah/atas, batas kritis bawah/atas, serta pembeda jenis kelamin dan kelompok umur. Sisa katalog lab tetap Rilis 2 | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-04. Menutup `LAB-CONFLICT-001` |
| `LAB-DEC-007` | Decision | **Koreksi hasil setelah rilis hanya oleh petugas berwenang validasi/rilis, dan dokter pemesan otomatis diberi tahu.** Versi lama tetap terlihat bertanda "sudah diperbaiki" | Pemilik proses Laboratorium + Clinical Governance | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01. **DITANDATANGANI KLINIS 2026-09-17** — `LAB-DEC-079`, per disiplin oleh `DR-LAB-001`, `DR-LAB-002`, dan `DR-LAB-003` | Lihat BR-05. Menutup `LAB-OPEN-005`. **Bunyi aturannya TIDAK berubah.** Frasa *"petugas berwenang validasi/rilis"* kini mewarisi dimensi disiplin dari `LAB-DEC-079`, dan apakah kewenangan itu melintasi disiplin **belum dinyatakan** — dibuka `LAB-OPEN-034` |
| `LAB-DEC-008` | Decision | **Hasil boleh dirilis sebagian per pemeriksaan.** Status hasil melekat pada pemeriksaan, bukan pesanan. Lembar hasil wajib memberi peringatan "hasil belum lengkap" | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-06 |
| `LAB-OPEN-005` | Open Question | Siapa yang berwenang mengoreksi hasil yang sudah dirilis, dan apakah dokter pemesan wajib diberi tahu? | Pemilik proses Laboratorium + Clinical Governance | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-007` | `LAB-INH-003` mengunci alur statusnya, tetapi tidak kewenangan dan pemberitahuannya |
| `LAB-CONFLICT-002` | Conflict | `LAB-INH-001` mengunci alur dimulai dari `Draft`, tetapi `Services/LabOrderService.cs:136@c87d9c0` selalu membuat pesanan berstatus `Requested` dan tidak ada kode yang menetapkan `Draft`. `LAB-INH-006` menjadi kosong | Yoga Aji Pratama | `resolved` | Diselesaikan 2026-09-01 oleh `LAB-DEC-015` | `01-existing-capability-map.md#CONF-01` |
| `LAB-DEC-015` | Decision | **Status `Draft` dihapus.** Pesanan tetap dibuat langsung `Requested`, tetapi dokter pemesan boleh menyunting pesanannya selama belum ada sampel berstatus `Collected`. Setelah itu pesanan terkunci dan dokter hanya boleh mengajukan pembatalan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-11. Menutup `LAB-CONFLICT-002` |
| `LAB-DEC-016` | Decision | **Pemberitahuan tersimpan dibangun sebagai kemampuan platform bersama**, bukan milik Laboratorium. Laboratorium menjadi pemakai pertama. Dokter punya satu kotak masuk untuk seluruh modul | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-12. Menutup `Q-LAB-02` dan `01-existing-capability-map.md#UNK-02` |
| `LAB-COORD-001` | Open Question | Kesepakatan dengan pemilik platform mengenai bentuk data, lokasi kode, dan urutan pengerjaan kemampuan pemberitahuan bersama | Yoga Aji Pratama + pemilik platform | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-016`; kemampuan ini di luar kepemilikan modul Laboratorium |
| `DEC-LAB-008` | Open Question | Apakah satu wadah fisik dapat melayani beberapa pemeriksaan? Model sekarang menyatukan wadah dan pemeriksaan menjadi satu konsep, sehingga dua pemeriksaan dari satu tabung serum memaksa dua barcode, dan penolakan tabung yang keruh dapat dilakukan sebagian — sesuatu yang tidak mungkin secara fisik | Yoga Aji Pratama + kepala instalasi laboratorium | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-024` | Ditemukan `03-domain-architecture.md#DEC-LAB-008`. Bukti: `TrxLabSpecimen.ProcedureId@c87d9c0` dan pengujian `#DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu@c87d9c0` |
| `LAB-DEC-017` | Decision | **Hasil lab didaftarkan ke rekam medis sebagai jenis dokumen klinis baru.** Isi hasil tetap di tabel Laboratorium, tidak digandakan. Rekam medis mencatat penulis, penandatanganan, dan penguncian | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-13. Menutup `Q-LAB-03` dan `01-existing-capability-map.md#UNK-01` |
| `LAB-COORD-002` | Open Question | Kesepakatan dengan pemilik modul `rekam-medis` untuk menambah nilai baru pada `ClinicalDocumentKind` | Yoga Aji Pratama + pemilik `rekam-medis` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-017`; enum itu milik modul lain |
| `LAB-DEC-025` | Decision | **Cakupan diperluas menjadi tiga disiplin**: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Bank Darah tetap di luar scope. Menggantikan `LAB-DEC-002` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-21. Bukti: `Analisis_Konsolidasi_Modul_Laboratorium.md` bagian 3.1 dan 13.1; menutup `REC-CONF-001` |
| `LAB-DEC-026` | Decision | **Cito dan Duplo melekat pada pemeriksaan terpesan, bukan pesanan.** Satu pesanan boleh memuat pemeriksaan cito dan biasa sekaligus | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-22. Bukti: `LAB-RULE-006`; menutup `REC-CONF-002` |
| `LAB-DEC-027` | Decision | **Hasil punya empat bentuk**: angka bersatuan, pilihan terbatas, mikrobiologi berstruktur, dan narasi Patologi Anatomi. Menggantikan `LAB-DEC-021` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-23. Bukti: `LAB-CAP-016`, `LAB-CAP-017`; menutup `REC-CONF-003` |
| `LAB-DEC-028` | Decision | **Laboratorium memiliki jalur pendaftaran pasien datang langsung dan rujukan luar.** Identitas pasien dan kunjungan tetap milik modul lain | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-24. Bukti: `LAB-CAP-002`; menutup `REC-CONF-004` |
| `LAB-DEC-032` | Decision | **Layar pendaftaran milik Laboratorium, pembuatan kunjungan tetap milik Registrasi.** Laboratorium memanggil Registrasi lalu menyimpan penunjuk kunjungan yang dikembalikan. `INV-01` tetap utuh | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-28. Bukti: `EncounterRegistrationSource.WalkIn`, `TrxPatientEncounter.IsWalkIn`, `IsReferral@c87d9c0`. Menutup `LAB-OPEN-015` |
| `LAB-DEC-036` | Decision | **Satu kolom penanda disiplin ditambahkan pada `MstProcedure`**, hanya bermakna bila `IsLaboratory` benar. Satu-satunya tambahan Laboratorium pada tabel itu; atribut operasional tetap dilarang. Mengamandemen AC-25 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-32. Menutup `DEC-LAB-010` |
| `LAB-COORD-005` | Open Question | Izin pemilik `master-data` untuk menambah kolom disiplin pada `MstProcedure`, dan pengisian nilainya untuk pemeriksaan lab yang sudah ada | Yoga Aji Pratama + pemilik `master-data` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-036` |
| `DEC-LAB-010` | Open Question | Bagaimana disiplin melekat pada jenis pemeriksaan? | Yoga Aji Pratama + pemilik `master-data` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-036` | `03-domain-architecture.md#DEC-LAB-010` |
| `LAB-DEC-035` | Decision | **Instansi dan dokter perujuk menjadi data induk global milik Master Data.** Kunjungan menunjuk ke sana; nama tidak disimpan sebagai teks bebas. Laboratorium hanya memilih dari daftar | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-31. Menutup `DEC-LAB-009`, membuka `S13b` |
| `LAB-COORD-004` | Open Question | Kesepakatan dengan pemilik `master-data` dan `registration-management`: dua data induk baru, kolom penunjuk pada kunjungan, dan pengisian daftar instansi perujuk | Yoga Aji Pratama + pemilik `master-data` + pemilik `registration-management` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-035` |
| `DEC-LAB-009` | Open Question | Di mana identitas dokter dan instansi perujuk disimpan? | Yoga Aji Pratama + pemilik `registration-management` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-035` | `03-domain-architecture.md#DEC-LAB-009` |
| `LAB-DEC-034` | Decision | **Penempatan data induk mengikuti cakupan pemakaiannya — backend saja.** Khusus Laboratorium diletakkan di folder Laboratorium; global diletakkan di folder Master Data. **Frontend tidak mengikutinya**; menu data induk frontend tetap di `health-services/master-data/` sesuai konvensi yang sudah ada | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-30. Bukti: 20 data induk khusus modul sudah berada di folder modulnya pada `c87d9c0` |
| `LAB-DEC-033` | Decision | **Tarif tetap milik Master Data.** Menu `Tarif Laboratorium` menjadi tampilan tersaring yang bersifat baca saja. Laboratorium tidak membuat tabel tarif sendiri | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-29. Bukti: `MstTariff@c87d9c0` adalah tabel bersama; `ResolveTariffAsync@c87d9c0`. Menutup `LAB-OPEN-016` |
| `LAB-FACT-007` | Fact | **Dokumen tata kelola canonical ditemukan.** `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` ada pada commit `c9692d0` "Repair QBE canonical governance paths". Checkout lokal berada di cabang `yoga` pada `c87d9c0`, **7 commit tertinggal** dari `origin/yoga`. Ketujuh commit itu **tidak menyentuh Laboratorium** | — | `fact` | — | `git log HEAD..c9692d0`; `git diff --name-only HEAD..c9692d0 \| grep -i lab` kosong |
| `LAB-OPEN-002` | Open Question | Di mana `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`? | Pemilik repository backend | `closed` | Ditutup 2026-09-01 oleh `LAB-FACT-007` — dokumennya ada, checkout lokal yang tertinggal | Folder `docs/engineering/` tidak ada pada `c87d9c0`, tetapi ada pada `c9692d0` |
| `LAB-OPEN-018` | Open Question | Prefix mana yang berlaku untuk data induk milik Laboratorium: `Mst` mengikuti baris Master/Reference, atau `Lab` mengikuti aturan `<PrefixPemilik><Konsep>`? Menyentuh penamaan `LabValueBound` dan `LabValueOption` | Pemilik registry prefix + Yoga Aji Pratama | `open` | — | `QBE-NAM-002` mewajibkan prefix registry; `QBE-NAM-004` melarang menyimpulkannya sendiri |
| `LAB-OPEN-019` | Open Question | Lifecycle `LaboratoryManagement` pada registry masih **`PLANNED`**, yang menurut registry **tidak memberi wewenang implementasi, migration, maupun deployment**. Padahal `LabOrder` dan siklus hidup wadah sudah berjalan di produksi. Perlu dinaikkan ke `ACTIVE` atau dijelaskan dasar pekerjaan yang sudah berjalan | Pemilik registry prefix | `open` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md@c9692d0` baris `LaboratoryManagement / Laboratory \| Lab \| PLANNED` |
| `LAB-COORD-003` | Open Question | Kesepakatan kontrak pemanggilan antarmodul dengan pemilik `registration-management`: bentuk permintaan, bentuk jawaban, dan perilaku saat gagal | Yoga Aji Pratama + pemilik `registration-management` | `closed` | `andryzainhome` dan `sukmagp`, 2026-09-01 lewat `LAB-REQ-001` | Konsekuensi `LAB-DEC-032` |
| `LAB-DEC-029` | Decision | **Tarif dan cakupan ditampilkan saat memesan; keputusan uang tetap milik Billing.** Menegaskan `LAB-INH-010` dan `LAB-INH-012` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-25. Bukti: `LAB-CAP-004`; menutup `REC-CONF-005` |
| `LAB-DEC-030` | Decision | *Diamandemen 2026-09-25 pada dua baris: penautan PDF eksternal kini menyimpan datanya di dokumen klinis pasien Clinical Management dengan pintu unggah di Laboratorium (`LAB-DEC-157`); order dari MCU memakai jalur umum dan perlakuan khususnya milik layanan MCU (`LAB-DEC-164`).* **Sebelas kemampuan tambahan masuk scope modul** dengan pembagian Rilis 1, 2, dan 3. Bank Darah, Quality Control, dan pemantauan beban alat tetap di luar | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-26 |
| `LAB-DEC-031` | Decision | **Baseline alur ujung ke ujung diadopsi** mengikuti kesimpulan analisis konsolidasi, disertai peringatan bahwa delapan hal `LAB-P0-*` belum diputuskan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-27 |
| `LAB-DEC-024` | Decision | **Wadah fisik dipisahkan dari pemeriksaan terpesan.** Satu wadah = satu barcode = satu keputusan layak atau tolak, dan dapat melayani beberapa pemeriksaan. Kelayakan tagih tetap terbit per pemeriksaan. Penolakan berlaku serentak bagi seluruh pemeriksaan pada wadah itu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-20. Menutup `DEC-LAB-008` dari `03-domain-architecture.md` |
| `LAB-OPEN-013` | Open Question | Apakah penanda Cito dan Duplo berdampak pada tarif? Bukti menempatkan keduanya sebaris dengan harga, tetapi dampaknya tidak diperagakan | Yoga Aji Pratama + Billing | `open` | — | Konsekuensi `LAB-DEC-026` |
| `LAB-OPEN-014` | Open Question | Bagaimana hasil mikrobiologi dan patologi anatomi masuk alur nilai kritis? Keduanya tidak dapat dinilai dengan perbandingan batas angka | Yoga Aji Pratama + Clinical Governance | `closed` | Arsitektur ditutup 2026-10-06 oleh `LAB-DEC-176`; isi klinis `LAB-OPEN-041`/`046` sebelum go-live | Konsekuensi `LAB-DEC-027` |
| `LAB-OPEN-015` | Open Question | Bagaimana Laboratorium membuat kunjungan untuk pasien datang langsung dan rujukan luar tanpa mengambil alih kepemilikan kunjungan dari modul Registrasi? | Yoga Aji Pratama + pemilik `registration-management` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-032` | Konsekuensi `LAB-DEC-028` |
| `LAB-OPEN-016` | Open Question | Apakah daftar tarif laboratorium dimiliki modul Laboratorium atau tetap milik Master Data? | Yoga Aji Pratama + pemilik `master-data` | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-033` | Konsekuensi `LAB-DEC-029` |
| `LAB-OPEN-017` | Open Question | Apa makna penanda Definitif pada hasil mikrobiologi, dan kapan ia wajib diisi? | Yoga Aji Pratama | `open` | — | Konsekuensi `LAB-DEC-030`. **Turun dari `BLOCKING` menjadi `LATER SLICE` pada 2026-09-18 lewat `LAB-DEC-081`** — penandanya dikeluarkan dari `S4b` Rilis 1, sehingga ia tidak lagi menahan slice mana pun. Bila kelak dijawab *"laporan akhir lawan laporan sementara"*, ia berubah menjadi perkara klinis milik `DR-LAB-002`, bukan lagi perkara pemilik modul |
| `LAB-P0-001` | Open Question | Matriks kewenangan per peran: siapa boleh menerima wadah, memproses, membatalkan, mengisi hasil, memvalidasi, mengotorisasi, menghapus pesanan, dan mengoreksi hasil yang sudah diotorisasi | Yoga Aji Pratama + Clinical Governance | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-1. Sebagian sudah dijawab `LAB-DEC-022`, sisanya terbuka |
| `LAB-P0-002` | Open Question | Urutan status resmi lintas tiga aplikasi, termasuk posisi `Confirmed` yang belum punya padanan pada rancangan | Yoga Aji Pratama | `closed` | Ditutup 2026-09-18 oleh `LAB-DEC-080`; posisi `Confirmed` sudah lebih dulu ditutup `LAB-DEC-061` pada 2026-09-15. **Bagian *lintas tiga aplikasi* TIDAK ikut tertutup** — ia memang milik `LAB-P0-008` dan tetap terbuka di sana | `Analisis_Konsolidasi` bagian 7 dan 14 P0-2 **Terjawab sebagian 2026-09-15 oleh `LAB-DEC-061`:** posisi `Confirmed` ditetapkan — antara `Requested` dan `Accepted`. Yang **tetap terbuka**: penyelarasan urutan status lintas tiga aplikasi |
| `LAB-P0-003` | Open Question | Aturan pembatalan dan koreksi: alasan wajib, jejak audit, dampak tagihan, dampak wadah, dampak hasil yang sudah masuk | Yoga Aji Pratama + Billing | `closed` | **Ditutup 2026-10-06** — sisa klinisnya `DEC-LAB-014` dijawab `LAB-DEC-178`..`180`. Sebelumnya: **Sebagian ditutup 2026-09-18 oleh `LAB-DEC-082`** — alasan koreksi dari daftar terkendali dan versi bernomor. Pembatalan pesanan sudah lebih dulu ditutup `LAB-DEC-063` dan `LAB-DEC-049`. **Sisanya bersifat klinis dan dipindahkan ke `DEC-LAB-014`** | — | `Analisis_Konsolidasi` bagian 14 P0-3 **Terjawab sebagian 2026-09-15 oleh `LAB-DEC-063`:** alasan pembatalan wajib dan batas statusnya ditetapkan. Yang **tetap terbuka**: aturan **koreksi** hasil, jejak auditnya, dan dampak tagihan |
| `LAB-P0-004` | Open Question | Alur nilai kritis: ambang, batas waktu tanggap, eskalasi, bukti penerimaan, dan tindakan bila penerima tidak dapat dihubungi | Yoga Aji Pratama + Clinical Governance | `closed` | Arsitektur ditutup 2026-10-06 oleh `LAB-DEC-176`/`177`; isi SLA `LAB-OPEN-047` sebelum go-live | `Analisis_Konsolidasi` bagian 14 P0-4; memperluas `LAB-DEC-004`. **2026-09-24:** `LAB-DEC-136` mengunci WhatsApp sebagai saluran pengantar, dan `PRD1-CLIN-01` menambahkan satu pertanyaan bukti penerimaan — sahkah balasan WhatsApp sebagai bukti baca ulang |
| `LAB-P0-005` | Open Question | Integrasi alat laboratorium: pemetaan kode pemeriksaan, pemetaan wadah, penerimaan hasil, pengulangan, pencegahan ganda, antrean kesalahan, dan arah penyelarasan | Yoga Aji Pratama + pemilik platform | `open` | — | `Analisis_Konsolidasi` bagian 14 P0-5. Bertentangan arah dengan `LAB-DEC-005` yang menunda integrasi alat |
| `LAB-P0-006` | Open Question | Kebijakan jejak audit resmi laboratorium | Yoga Aji Pratama | `open` | — | `Analisis_Konsolidasi` bagian 15 |
| `LAB-P0-007` | Open Question | Aturan tagihan dan cakupan penjamin, termasuk arti `Tidak Tercover` dan perubahan penjamin setelah pesanan dibuat | Billing | `open` | — | `Analisis_Konsolidasi` bagian 14 P1-9 dan P1-10 |
| `LAB-P0-008` | Open Question | Mekanisme penyelarasan data antara aplikasi Laboratorium baru, HiSys, HCLAB, dan RS MMC App | Yoga Aji Pratama + pemilik platform | `open` | — | `Analisis_Konsolidasi` bagian 13.2 dan 18 |
| `LAB-OPEN-012` | Open Question | Berapa banyak data laboratorium yang sudah terisi di basis data produksi? Menentukan biaya dan risiko pemindahan data akibat `LAB-DEC-024` | Pemilik repository backend + DBA | `open` | — | Frontend Laboratorium nol (`CAP-21`) menjadi dugaan kuat bahwa data sungguhan belum ada, tetapi belum diverifikasi |
| `LAB-DEC-023` | Decision | **Batas normal bebas diubah kepala instalasi; batas kritis memerlukan persetujuan klinis.** Seluruh perubahan batas nilai disimpan sebagai riwayat lengkap. Mempersempit `LAB-DEC-018`, tidak membatalkannya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-19. Menutup `DEC-LAB-003` dari `02-requirement-completeness-assessment.md` |
| `LAB-DEC-022` | Decision | **Kewenangan validasi dan rilis tetap terpisah dan diberikan per orang, bukan per jabatan.** Setiap shift wajib punya minimal dua pemegang kewenangan validasi; sistem memperingatkan kepala instalasi bila hanya ada satu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-18. Menutup `DEC-LAB-001` dari `02-requirement-completeness-assessment.md`. **2026-09-24 — butir 2 SUPERSEDED oleh `LAB-DEC-150`:** kewenangan validasi hanya bagi dokter berkewenangan laboratorium, bukan analis senior. **Butir 3 dan 4 — minimal dua pemegang per shift dan peringatannya — TETAP BERLAKU** |
| `LAB-DEC-021` | Decision | **Hasil punya dua bentuk: angka dan pilihan terbatas.** Pemeriksaan berhasil pilihan menyimpan daftar pilihan sah beserta penanda mana yang di luar rujukan dan mana yang kritis. Analis memilih, tidak mengetik bebas. `LAB-DEC-004` berlaku untuk kedua bentuk | Yoga Aji Pratama | `superseded` oleh `LAB-DEC-027` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-17. Menutup `DEC-LAB-002` dari `02-requirement-completeness-assessment.md` |
| `LAB-DEC-020` | Decision | **Koreksi hasil setelah kunjungan ditutup memakai mekanisme addendum rekam medis yang sudah ada.** Dokumen asli tetap terkunci dan tidak diubah; hasil perbaikan menempel sebagai addendum bertanda tangan dengan alasan koreksi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-16. Menutup `LAB-OPEN-011`. Dasar bukti: `MrcClinicalNoteAddendum.cs@c87d9c0` |
| `LAB-DEC-019` | Decision | **Alasan penolakan sampel dikelola kepala instalasi lewat layar pengelolaan, kecuali penanda kesalahan internal dan penanda wajib catatan** yang hanya dapat disetel admin sistem. Data awal wajib disiapkan pada Rilis 1 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-15. Menutup `Q-LAB-04` |
| `LAB-DEC-018` | Decision | **Batas nilai menjadi tabel tersendiri milik Laboratorium** yang menunjuk ke `MstProcedure`. Satu pemeriksaan boleh punya beberapa baris batas menurut jenis kelamin dan kelompok umur. `MstProcedure` tidak diubah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-14. Menutup `Q-LAB-05` |
| `LAB-OPEN-011` | Open Question | Apa yang terjadi bila hasil perlu dikoreksi setelah kunjungan pasien ditutup dan dokumennya sudah terkunci oleh `ClinicalDocumentLockTrigger.EncounterClosed`? | Yoga Aji Pratama + pemilik `rekam-medis` + Clinical Governance | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-020` | Pertemuan antara `LAB-DEC-007` dan aturan penguncian rekam medis |
| `LAB-AMD-001` | Open Question | `LAB-INH-001` dan `LAB-INH-006` pada `RJ-BIL-GATE-DEC-003` perlu diamandemen: `Draft` dihapus, dan batas kewenangan dokter diubah menjadi "sampai sampel pertama diambil" | Pemilik blueprint `rawat-jalan` + Billing | `open` | — | Konsekuensi `LAB-DEC-015`; keputusan aslinya milik blueprint lain sehingga tidak boleh diubah dari sini |
| `LAB-OPEN-002` | Open Question | Di mana `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` yang disebut `AGENTS.md`? | Pemilik repository backend | `closed` | Ditutup 2026-09-01 oleh `LAB-FACT-007`: keduanya **masih berlaku**, berada di `QuilvianEngineeringSkills/agents/rules/backend/engineering/`; path `docs/engineering/` usang | Membuka `LAB-OPEN-018` dan `LAB-OPEN-019` |
| `LAB-OPEN-018` | Open Question | Kapan suite Skill yang memuat `rules/backend/engineering/` dipublikasikan ke marketplace yang benar-benar terpasang, sehingga rules root runtime memenuhi `AGENTS.md`? Selama belum, setiap task backend wajib berhenti dengan `BLOCKED — canonical governance unavailable` menurut gerbang kegagalan `AGENTS.md` sendiri | Pemilik repository backend + pemilik suite Skill | `open` | — | `LAB-FACT-009`. Dua repo berbeda: terpasang `MHamzah1/QuilvianEngineeringSkillsClaude@f0136df`, canonical `DevBenari/QuilvianEngineeringSkills@59bd3e2` |
| `LAB-OPEN-019` | Open Question | Apakah lifecycle `LaboratoryManagement` pada registry dinaikkan dari `PLANNED` menjadi `ACTIVE`? Tanpa itu `QBE-MOD-002` dan `QBE-MOD-003` menahan pembuatan entity operasional `Lab*` pertama | Pemilik repository backend | `open` | — | `LAB-FACT-010`. Preseden: `RWI-DEC-068` menaikkan `InPatientManagement` dari `PLANNED` ke `ACTIVE` |
| `LAB-OPEN-003` | Open Question | Apakah bank darah dan transfusi masuk modul Laboratorium? | Pemilik proses Laboratorium | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-002`: **tidak masuk** | Belum ada modul bank darah di backend |
| `LAB-DEC-009` | Decision | **Laboratorium melayani Rawat Jalan, Rawat Inap, dan IGD sekaligus sejak Rilis 1.** Konsekuensinya pesanan wajib punya penanda tingkat kesegeraan minimal "biasa" dan "cito" | Pemilik proses Laboratorium | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-07 |
| `LAB-DEC-010` | Decision | **Wewenang UI:** letak menu dan penamaan route mengikuti pola modul Health Services yang sudah ada. Rincian tata letak, tab/modal/drawer, dan warna menjadi `DEV_DISCRETION`. Penanda keselamatan pada BR-01, BR-02, BR-06, dan BR-07 tetap wajib dan tidak boleh diperlemah | Pemilik produk/UI | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat tabel Frontend Decision Authority |
| `LAB-OPEN-004` | Open Question | Apakah pemakaian dan stok reagen dikelola Laboratorium atau Farmasi/Inventory? | Laboratorium + Farmasi | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-014` | Belum ada tabel stok reagen |
| `LAB-DEC-011` | Decision | **Wewenang klinis terpisah.** Yoga Aji Pratama mengesahkan sisi produk dan operasional. `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` tetap memerlukan tanda tangan dokter penanggung jawab laboratorium atau Komite Medis sebelum desain final | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Ketiga keputusan itu menentukan perilaku sistem saat hasil salah atau pasien dalam bahaya. **TERPENUHI 2026-09-17** lewat `LAB-DEC-079`: ketiganya ditandatangani per disiplin. Syarat yang dipasang keputusan ini sendiri kini sudah dibayar |
| `LAB-DEC-012` | Decision | **Pemberitahuan tersimpan dibangun di Rilis 1.** Setiap pemberitahuan nilai kritis dan koreksi hasil disimpan sebagai data milik dokter tujuan, lengkap dengan status sudah dibaca. SignalR hanya pelengkap agar muncul seketika | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-08. Dasar bukti: F7 |
| `LAB-DEC-013` | Decision | **Cito ditandai dokter pemesan, dengan batas waktu penyelesaian per jenis pemeriksaan** yang disimpan bersama tabel batas nilai. Ada daftar pantau pesanan cito yang lewat batas | Yoga Aji Pratama | `amended` oleh `LAB-DEC-026` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-09. Menutup `LAB-OPEN-007` |
| `LAB-DEC-014` | Decision | **Stok, pembelian, dan pencatatan pemakaian reagen berada di luar modul Laboratorium**, diserahkan ke Farmasi/Inventory | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-01 | Lihat BR-10. Menutup `LAB-OPEN-004` |
| `LAB-OPEN-007` | Open Question | Apa definisi resmi "cito", siapa yang boleh menandai pesanan sebagai cito, dan berapa batas waktu penyelesaiannya? | Yoga Aji Pratama | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-013` | Konsekuensi langsung `LAB-DEC-009`; belum ada kolom kesegeraan pada `LabOrder` (F1) |
| `LAB-OPEN-008` | Open Question | Lewat sarana apa pemberitahuan koreksi hasil dan pemberitahuan nilai kritis sampai ke dokter? | Yoga Aji Pratama + pemilik platform | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-012` | Konsekuensi `LAB-DEC-004` dan `LAB-DEC-007`. Bukti F7: platform belum punya sarana notifikasi tersimpan |
| `LAB-OPEN-009` | Open Question | Apakah Yoga Aji Pratama sebagai pemilik modul juga memegang wewenang Clinical Governance untuk mengesahkan `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007`, atau ketiganya masih memerlukan tanda tangan pihak klinis terpisah? | Manajemen rumah sakit | `closed` | Ditutup 2026-09-01 oleh `LAB-DEC-011` | Ketiga keputusan itu menyangkut keselamatan pasien, bukan sekadar operasional |
| `LAB-EVD-001` | Fact | Artifact **`Penerimaan Sampling Specimen Lab.md`** dilampirkan pemilik modul pada sesi 2026-09-14. Memuat `CAP-001`..`CAP-016` dan `RULE-001`..`RULE-029`, hasil 14 klarifikasi bisnis tertanggal 2026-09-12. Berkasnya dikutip dengan nama, tidak disalin ke folder blueprint, mengikuti konvensi `LAB-DEC-025` | — | `fact` | — | Lampiran sesi 2026-09-14 |
| `LAB-GAP-001` | Conflict | **`BR-25` menyebut Laboratorium boleh menampilkan "jumlah", tetapi `LabExamination` tidak punya kolom Qty sama sekali** — yang ada hanya `IsDuplo`. Keputusan dan model tidak sejalan sejak 2026-09-01 | Yoga Aji Pratama | `closed` | Ditutup 2026-09-14 oleh `LAB-DEC-038` | `LabExamination.cs@466a7127`; `BR-25`; `AC-43`; diperkuat `RULE-025` pada `LAB-EVD-001` |
| `LAB-DEC-050` | Decision | **Kolom Jumlah/Qty tidak dibuat; `LAB-DEC-038` dicabut.** `LabExamination` punya index unik `(SpecimenId, ProcedureId)` atas dasar `BR-20` dan `AC-35`, sehingga `Quantity` > 1 untuk satu jenis pemeriksaan pada satu wadah mustahil. Contoh Glukosa pada `BR-33` keliru: Glukosa Puasa dan Glukosa 2 Jam PP adalah dua `MstProcedure` berbeda. Petugas memilih dua butir katalog; `IsDuplo` tetap satu-satunya cara menyatakan pengerjaan ganda | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-45. Menutup `LAB-CONFLICT-005`. Bukti: `LabExaminationConfiguration.cs@9067fa73`; [`BE-LAB-23.md`](task/report/backend/BE-LAB-23.md) |
| `LAB-DEC-051` | Decision | **Kiosk menjadi titik masuk pendaftaran laboratorium, dan pasien memilih layanannya sendiri.** Sesi kiosk perlu membawa tujuan layanan, yang hari ini tidak dimilikinya. Hasil sesi ditampung layar pendaftaran pasien laboratorium | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul) + Andry Zain lewat `LAB-REQ-006`, 2026-09-15 | Lihat BR-46. Disetujui Andry Zain lewat `LAB-REQ-006`; `LAB-COORD-008` **ditutup** |
| `LAB-DEC-052` | Decision | *Diamandemen 2026-10-08 oleh `LAB-DEC-213` — kiosk tidak lagi menanyakan surat dokter; jalur ditentukan dari ada/tidaknya pesanan Lab aktif, dan pasien bersurat dokter luar mendaftar lewat Jenis Kunjungan Rujukan.* **Dua jalur dibedakan sejak kiosk:** pasien yang membawa permintaan dokter, dan pasien yang memeriksakan diri sendiri. Keduanya berbeda perlakuan sampai ke penjamin dan tagihan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul) + Andry Zain lewat `LAB-REQ-006`, 2026-09-15 | Lihat BR-46 butir 2. Disetujui lewat `LAB-REQ-006`. **Batas:** jalur bawa surat dokter **luar** belum dapat dipakai penuh sampai `LAB-COORD-006` dijawab |
| `LAB-DEC-053` | Decision | **Kunjungan terbentuk begitu pasien selesai di kiosk**, bukan menunggu petugas lab memproses. Pasien langsung terlihat pada antrean dan laporan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul) + Andry Zain lewat `LAB-REQ-006`, 2026-09-15 | Lihat BR-46 butir 3 beserta risikonya. Disetujui lewat `LAB-REQ-006`. Pembentukannya tetap dikerjakan Registrasi; `AC-45` **tidak dicabut** |
| `LAB-DEC-054` | Decision | **Kunjungan kiosk yang tidak dilanjutkan ditutup otomatis oleh Registrasi** saat batas waktunya lewat, dengan sebab "tidak dilanjutkan". Laboratorium tidak ikut menutupnya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul) + Andry Zain lewat `LAB-REQ-006`, 2026-09-15 | Lihat BR-46 butir 4. Disetujui lewat `LAB-REQ-006`; batas waktu dan nasib biaya pendaftaran ditetapkan `LAB-DEC-058` |
| `LAB-DEC-055` | Decision | **Pesanan dipecah otomatis satu per disiplin.** Petugas memilih pemeriksaan sekali; sistem membentuk satu pesanan per disiplin. Pemeriksaan yang belum digolongkan berkumpul menjadi satu pesanan tanpa disiplin, dan itu tetap sah. `INV-21`, `VAL-46`, dan penyaring ketiga layar Pemeriksaan **tidak disentuh**. Mengamandemen `LAB-DEC-048` butir 6 dan `AC-83` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Lihat BR-47. **Murni Laboratorium** — tidak bergantung modul lain, dapat dijadwalkan sekarang |
| `LAB-DEC-056` | Decision | **Satu endpoint baru sekali tekan.** Pendaftaran laboratorium dari sesi kiosk beserta daftar pemeriksaan diproses dalam satu transaksi yang membentuk pesanan-pesanan terpecah. **`POST /lab-orders` yang sudah ada tidak disentuh**, sehingga pemanggil lama tidak berubah perilakunya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Lihat BR-47 butir 4. Menghindari pengulangan kejadian `specimenTypeId` pada `BE-LAB-21` |
| `LAB-DEC-057` | Decision | **Pemeriksaan yang dipesan disimpan pada tabel tersendiri, bukan pada `LabExamination`.** Entity baru `LabOrderedProcedure` memuat **apa yang diminta**; `LabExamination` tetap menjadi **satuan kerja yang lahir dari sebuah wadah**. Diputuskan setelah ditemukan bahwa `LabExamination.SpecimenId` wajib dan bagian unique index `(SpecimenId, ProcedureId)`, sehingga permintaan pemeriksaan tidak dapat hidup sebelum wadah fisiknya ada | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Lihat BR-47 dan `02-backend-architecture.md` 12.1-12.2. Menjadikan `LAB-DEC-056` dapat dilaksanakan; **`LabExamination` tidak disentuh sama sekali** |
| `LAB-DEC-058` | Decision | **Kunjungan kiosk yang tidak dilanjutkan ditutup saat hari layanan berakhir, dan biaya pendaftarannya gugur.** Penutupannya otomatis oleh Registrasi dengan sebab "tidak dilanjutkan". Pasien yang batal tidak menanggung apa pun, dan tidak pernah menghasilkan tagihan pemeriksaan laboratorium | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul) + Andry Zain lewat `LAB-REQ-006`, 2026-09-15 | Lihat BR-46 butir 4. Menutup `LAB-COORD-009` |
| `LAB-DEC-059` | Decision | **Hari layanan berakhir pukul 21:00 WIB untuk keperluan penutupan otomatis kunjungan kiosk yang tidak dilanjutkan**, ditulis sebagai konfigurasi sehingga dapat diubah tanpa rilis ulang. Angka ini ditetapkan pemilik modul Laboratorium pada 2026-09-15 saat `BE-EXT-05` ditinjau; **ia belum dikonfirmasi terhadap jam operasional resmi rumah sakit maupun terhadap pemilik `registration-management`**, dan itu ditulis di sini supaya tidak terbaca sebagai SOP | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Menurunkan `LAB-DEC-058`, yang menetapkan **kapan** tetapi tidak menetapkan **pukul berapa**. Dipakai `BE-EXT-05` |
| `LAB-DEC-060` | Decision | *Dipulihkan 2026-10-08 oleh `LAB-DEC-216` — Terima Sampling kembali satu aksi di daftar, kini sebagai dialog; kelayakan ditetapkan petugas yang bukan pengambil sampel (`LAB-DEC-219`).* *Diamandemen 2026-10-06 oleh `LAB-DEC-191` — jalan pintas satu klik diganti satu navigasi ke layar Wadah sampai ada endpoint penerimaan atomik; siklus hidup wadah tetap.* **Siklus hidup wadah tetap berlaku; centang `Sampling diterima` adalah jalan pintas satu klik ke jalur itu, bukan penggantinya.** Artifact 2026-09-15 memotret layar yang berjalan hari ini, ketika penerimaan sampling memang hanya satu centang di pop-up Proses Pemeriksaan. Blueprint memperluasnya menjadi `Planned` → `Collected` → `Received` → `Accepted`/`Rejected` karena bukti lapangan putaran 1 menunjukkan **penolakan wadah dan pengambilan ulang benar-benar terjadi**. Petugas tetap melihat satu centang; di belakangnya wadah berpindah status sungguhan. Menutup `REC2-CONF-001` dan `REC2-CONF-002` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Menjaga `BE-LAB-21` sampai `BE-LAB-28` tetap berlaku, termasuk `VAL-68`/`VAL-69`. Status `Accepted` tetap diturunkan otomatis saat wadah pertama dinyatakan layak |
| `LAB-DEC-061` | Decision | *Diamandemen 2026-10-07 oleh `LAB-DEC-193`/`LAB-DEC-195` — Konfirmasi juga sah pada `Accepted` yang belum dikonfirmasi, dan di sana status tidak berubah; `Confirmed` tetap berarti dikonfirmasi sebelum wadah layak.* **Status `Confirmed` diadopsi sebagai status pesanan antara `Requested` dan `Accepted`, beserta konfirmator, waktu konfirmasi, dan dokter pemeriksa.** Konfirmasi hanya boleh sekali; sesudahnya aksinya nonaktif. Nama konfirmator mengikuti pengguna yang login. Dokter pemeriksa dipilih atau diganti sebelum konfirmasi disimpan, dan tampil pada daftar serta ringkasan cetak. **Menjawab `LAB-P0-002` untuk bagian yang selama ini paling sering ditanyakan** — posisi `Confirmed` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | Menuntut amandemen `LAB-STATE-v1` dan satu migration: `Confirmed` pada `LabOrderStatus`, ditambah kolom konfirmator, waktu konfirmasi, dan dokter pemeriksa pada `LabOrder`. Hubungan `Confirmed` dengan urutan status dua aplikasi lain **belum** dijawab; `LAB-P0-002` tetap terbuka untuk bagian itu |
| `LAB-DEC-062` | Decision | *Dipasang kembali 2026-10-08 oleh `LAB-DEC-218` untuk Proses Pemeriksaan saja, lewat jalur baca sementara `LAB-DEC-220`; Terima Sampling tidak dikunci.* *Diamandemen 2026-10-06 oleh `LAB-DEC-189` — kunci Proses Pemeriksaan ditangguhkan sebagai pengecualian sementara sampai `LAB-COORD-010` tersedia; sesudah itu dipasang tanpa keputusan baru.* **Status pembayaran tampil di layar Laboratorium dan mengunci aksi Proses Pemeriksaan bagi pasien Mandiri/tunai sampai `Lunas` — tetapi nilainya dibaca dari Billing, bukan disimpan Laboratorium.** Laboratorium **tidak** menambah kolom pembayaran apa pun pada `LabOrder`. `RJ-BIL-GATE-DEC-003` tetap berlaku penuh: keputusan uang milik Billing, dan Laboratorium hanya membaca serta menampilkannya. Menutup `REC2-CONF-003` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | **Jalur bacanya belum ada.** Dibuka sebagai `LAB-COORD-010`: Laboratorium memerlukan endpoint milik Billing yang menyatakan status pembayaran satu kunjungan atau satu pesanan. Sampai ada, penguncian tombol tidak dapat ditegakkan backend |
| `LAB-DEC-063` | Decision | **Alasan pembatalan pesanan wajib, disimpan backend, dan pembatalan hanya sah saat status masih `Requested` atau `Confirmed`.** Pembatalan pada `InProcess`, `Completed`, atau `Cancelled` ditolak. Pop-up alert konfirmasi akhir sesudah `Lanjut Pembatalan` adalah **kewenangan UI**, bukan aturan backend — backend menuntut alasannya ada, bukan menuntut berapa kali petugas ditanya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-15 | **Dikoreksi 2026-09-15 saat amandemen kontrak: kolom baru TIDAK diperlukan.** `LabOrderService.CancelAsync` sudah menyimpan alasannya sebagai `ReasonNote` pada `LabTransitionHistory` — jejak audit, tempat yang memang seharusnya, dan terbaca kembali per pesanan lewat `LabOrderId`. Artifact pun tidak menampilkan alasan pembatalan pada satu pun dari 14 kolomnya. Yang dibutuhkan hanya aturan baru pada `LAB-VAL-v1`: mewajibkan ruas yang hari ini opsional. **Menjawab sebagian `LAB-P0-003`** — aturan pembatalan ditetapkan, aturan **koreksi hasil** belum disinggung sama sekali dan tetap terbuka |
| `LAB-EVD-002` | Fact | Artifact **`Laboratorium (4).md`** dilampirkan pemilik modul pada sesi 2026-09-16. Memuat `CAP-001`..`CAP-012`, `RULE-001`..`RULE-028`, dan `BP-001`..`BP-008` atas satu menu: *Hasil Patologi Klinik, Patologi Anatomi dan Mikrobiologi*. Menyatakan dirinya pembaruan `Laboratorium (3).md` ditambah sepuluh jawaban klarifikasi bertanggal sama. **Disimpan verbatim** pada [`evidence/2026-09-16-menu-hasil-patologi-klinik-anatomi-mikrobiologi.md`](evidence/2026-09-16-menu-hasil-patologi-klinik-anatomi-mikrobiologi.md) — putaran 1 dan 2 hanya menyebut nama berkas buktinya dan berkasnya kini tidak dapat ditemukan lagi; putaran ini tidak mengulanginya. **Kedua baseline yang dirujuk artifact sendiri tidak ikut diserahkan**, sehingga rujukan `Sumber baris ...` di dalamnya tidak dapat diverifikasi dari sini | Yoga Aji Pratama | `accepted` | Diterima 2026-09-16 sebagai bukti tingkat pemilik atas klarifikasinya, tingkat pengamatan atas sisanya | Rekonsiliasinya pada `05-evidence-reconciliation.md` bagian 11. **Seluruh isinya slice `S17`**, yang berdiri di atas hasil pemeriksaan yang belum ada |
| `LAB-DEC-064` | Decision | **Aturan rentang tanggal penyaring: hari ini boleh, masa depan tidak, tanpa batas maksimum panjang rentang, dan wajib animasi loading selama pemuatan.** Berapa pun panjang rentang yang diminta petugas, sistem tidak menolaknya — yang wajib adalah memberi tahu bahwa data sedang diproses | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Arah pembandingnya — `Tgl Awal < Tgl Akhir` atau `<=` — **tidak** ikut diputuskan; lihat `LAB-OPEN-028`. Penyaring yang berjalan hari ini inklusif pada kedua ujung |
| `LAB-DEC-065` | Decision | **Keyword Search mengikuti kolom, relasi, dan pola query order pemeriksaan yang sudah ada di backend; mengarang nama kolom baru dilarang.** Ini keputusan yang sengaja menahan diri: pemilik menolak menetapkan ruas pencarian di dokumen dan menyerahkannya pada implementasi yang sudah terbukti | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Pencarian Laboratorium hari ini menjangkau `EncounterNumber`, `FullName`, dan `MedicalRecordNumber`. **NIK nol dijangkau** walaupun `MstPatient.IdentityNumber` ada, dan **nomor order belum ada kolomnya** — lihat `REC3-NEW-003` dan `LAB-OPEN-033` |
| `LAB-DEC-066` | Decision | **`Terkirim ke Pasien` adalah counter integer, bukan rasio.** Setiap pengiriman berhasil menambah `+1`; kegagalan tidak menambah apa pun. Tombol kirim ulang tetap tersedia sesudah fungsi pengiriman dapat dipakai, termasuk ketika pengiriman sebelumnya gagal, dan setiap pengiriman ulang yang berhasil menambah `+1` lagi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | **Bentuk penyimpanannya belum dirancang dan tidak diputuskan di sini.** Counter telanjang menyimpan angka tanpa riwayat: siapa mengirim, kapan, ke nomor mana, dan mana yang gagal tidak terjawab. Perancangannya milik `hospital-domain-architect` |
| `LAB-DEC-067` | Decision | **Satu nomor order menghasilkan satu dokumen hasil final, dan hasil hanya boleh dikirim sesudah disetujui Profesor dan Dokter Lab.** Seluruh pemeriksaan pada order wajib sudah mempunyai hasil lebih dulu. Nomor tujuan diambil dari nomor WhatsApp pasien pada data induk pasien. Berkas yang dikirim PDF atau laporan final yang **tidak dapat diedit pasien** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | **Tertahan tiga hal sekaligus.** `LAB-SIGN-001` — siapa yang berwenang menyetujui secara klinis belum ditetapkan, lihat `LAB-OPEN-029`. `LAB-COORD-011` — gerbang WhatsApp dan pembangkit PDF keduanya nol pada platform. Dan izin privasi pengiriman data klinis ke kanal pihak ketiga, yang `LAB-DEC-030` tandai sejak 2026-09-01 dan **belum** diberikan artifact ini. Nomor tujuannya sendiri **sudah punya tempat**: `MstPatient.WhatsAppNumber` |
| `LAB-DEC-068` | Decision | **Seluruh tombol aksi pada menu Hasil dipegang Petugas Lab dan/atau Admin** — Nota Lab, Label Lab, Label Goldar, Kirim Hasil ke Pasien, dan Kirim Ulang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Menjawab `LAB-P0-001` **hanya untuk menu ini**. Peran lain pada matriks kewenangan tetap terbuka. Peran penyetuju Profesor dan Dokter Lab **bukan** bagian keputusan ini |
| `LAB-DEC-069` | Decision | **Perilaku datatable: pagination kelipatan 5, urutan default data terbaru paling atas, alert ketika data kosong, dan alasan error ketika datatable gagal dimuat.** Petugas tidak boleh dibiarkan menatap tabel kosong tanpa tahu apakah datanya memang tidak ada atau pemuatannya gagal | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | **Dua dari empat sudah terpenuhi source hari ini**: pengurutan menurun sudah berjalan pada `LabMonitoringService`, dan keempat `pageSizeOptions` frontend (`10`, `25`, `50`, `100`) semuanya kelipatan 5. Pola alert kosong dan alert galat pun sudah dipakai layar pantau |
| `LAB-CONFLICT-009` | Conflict | **Artifact menyatukan hasil ketiga disiplin dalam satu datatable; `LAB-DEC-025` menetapkan tiga daftar sejajar sebagai tiga menu, dan `LabMonitoringQuery` sengaja nol ruas disiplin** | Yoga Aji Pratama | `closed` | Ditutup 2026-09-16 oleh `LAB-DEC-070` | Dibuka dan ditutup pada hari yang sama. **Blueprint bertahan:** tiga daftar sejajar, disiplin ditentukan jalur yang dipanggil. Artifact tidak diadopsi apa adanya pada butir ini |
| `LAB-DEC-070` | Decision | **Menu Hasil mengikuti pola tiga daftar sejajar per disiplin, bukan satu datatable gabungan.** Disiplin tetap ditentukan jalur yang dipanggil, bukan ruas yang dikirim — persis seperti ketiga menu Pemeriksaan. **Artifact tidak diadopsi apa adanya pada butir ini**, dan itu disengaja: `LAB-DEC-025` menolak layar berpenyaring disiplin dengan alasan yang masih berlaku, yaitu memaksa petugas memilih hal yang bagi dirinya tidak pernah berubah. Menutup `LAB-CONFLICT-009` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | `LabMonitoringQuery` dipakai ulang apa adanya — nol ruas disiplin ditambahkan. Bentuk endpoint daftar hasil mengikuti `GET /lab-orders/by-discipline/{discipline}` yang sudah ada, bukan satu jalur lintas disiplin |
| `LAB-DEC-071` | Decision | **Pembanding rentang tanggal inklusif: `Tgl Awal <= Tgl Akhir`.** Petugas dapat mencari satu hari saja dengan mengisi tanggal yang sama pada kedua ruas. **Mengamandemen `LAB-DEC-064`** dan mengoreksi `RULE-004` artifact, yang bila dibaca tegas justru menutup pencarian satu hari — padahal `RULE-003` di sebelahnya mengizinkan tanggal hari ini pada kedua ruas. Menutup `LAB-OPEN-028` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | **Nol perubahan perilaku pada source.** Penyaring yang berjalan hari ini sudah inklusif pada kedua ujung (`>= mulai`, `<= sampai`). Yang ditetapkan di sini aturan validasinya, supaya frontend tidak menolak apa yang backend terima |
| `LAB-DEC-072` | Decision | **`LabOrder` memperoleh kolom nomor order baru berupa nomor urut yang terbaca manusia**, mengikuti pola `PatientEncounterNumberService` yang sudah berjalan: awalan tetap, nomor urut berpadding, dan `pg_advisory_xact_lock` sebagai penjaga konkurensi. Nomor ini menjadi isi kolom `No. Order` sekaligus sumber barcode Label Lab. **Pola `LSP-{Guid:N}` milik barcode wadah sengaja tidak dipakai** — nomor yang dicetak pada amplop pasien harus dapat disebut lewat telepon. Menutup `LAB-OPEN-033` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Menuntut **satu kolom baru beserta migration**, satu layanan alokasi, dan satu index unik. **Satu peringatan dibawa serta dari pola acuannya:** `AllocateEncounterNumberAsync` memuat **seluruh** nomor terpakai ke memori lalu memindai celah pertama — biayanya tumbuh seiring jumlah baris. Meniru polanya **tanpa** meniru kelemahan itu adalah bagian dari perancangan, bukan detail implementasi |
| `LAB-DEC-073` | Decision | **Menu Hasil hanya memuat pesanan Laboratorium.** Nilai Unit Layanan `Radiologi` pada `RULE-008` artifact terbawa dari tabel unit layanan yang dipakai bersama modul lain, dan **tidak akan pernah muncul** sebagai baris pada menu ini. Cakupan menu tidak melebar ke luar modul. Menutup `LAB-OPEN-031` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Nol koordinasi dengan pemilik Radiologi yang perlu dibuka. Nilai `Laboratorium` pada aturan yang sama tetap sah dan tetap berlaku |
| `LAB-EVD-008` | Fact | PRD **`Modul Laboratorium - Hasil Pemeriksaan Patologi Klinik & Mikrobiologi`** berstatus `Draft Requirement` ditempel pemilik modul pada sesi 2026-09-24. Memuat `FR-PK-001`..`FR-PK-005`, `FR-MB-001`..`FR-MB-005`, `BP-001`..`BP-003`, tabel lima status, dan acceptance criteria dua disiplin. **Disimpan verbatim** pada [`evidence/2026-09-24-prd-hasil-patologi-klinik-mikrobiologi.md`](evidence/2026-09-24-prd-hasil-patologi-klinik-mikrobiologi.md). Direkonsiliasi pada amendment pass putaran 14: **14 pertentangan** `PRD1-CONF-01`..`PRD1-CONF-14` dan **6 butir baru** `PRD1-NEW-01`..`PRD1-NEW-06` | Yoga Aji Pratama | `diterima sebagai bukti` | Diterima 2026-09-24; kedudukannya ditetapkan `LAB-DEC-133` | **Penulis PRD belum disebutkan.** Empat pertentangannya sudah pernah ditolak pada putaran 9 (`LAB-DEC-095`, `LAB-DEC-098`, `LAB-DEC-102`, `LAB-DEC-105`) |
| `LAB-EVD-009` | Fact | Jawaban **dr. Bima Prasetya, Sp.PK**, Kepala Instalasi Laboratorium, atas `DEC-LAB-011`, `LAB-OPEN-029`, `LAB-COORD-011`/`015`, `PRD1-CLIN-01`, dan `PRD1-OPEN-01`, disampaikan pemilik modul pada sesi 2026-09-24. Isinya dicatat **verbatim** pada bagian *Amendment Pass Putaran 16* | Yoga Aji Pratama | `diterima sebagai bukti` | Diterima 2026-09-24 | **Bukti tertulis dari dr. Bima belum dilampirkan.** Jawaban `DEC-LAB-011` dicatat sebagai jawaban **sebagian** atas pilihan A pemilik modul. **2026-09-25: dilengkapi bukti tertulis `LAB-EVD-010`** |
| `LAB-EVD-010` | Fact | **Surat tertulis dr. Bima Prasetya, Sp.PK**, Kepala Instalasi Laboratorium, atas `LAB-REQ-014`: konfirmasi *"Ya"* atas catatan `LAB-DEC-150`; validasi di luar jam kerja oleh dokter lain yang ditetapkan pada disiplin terkait; pemegang Mikrobiologi dr. Nabila Rahmawati, Sp.MK dan Patologi Anatomi dr. Citra Maharani, Sp.PA; rilis oleh *"pejabat/dokter yang memiliki kewenangan otorisasi sesuai aturan laboratorium"*. **Disimpan verbatim** pada [`evidence/2026-09-25-jawaban-tertulis-dr-bima-lab-req-014.md`](evidence/2026-09-25-jawaban-tertulis-dr-bima-lab-req-014.md) | Yoga Aji Pratama | `diterima sebagai bukti` | Diterima 2026-09-25 | **Isi surat tidak bertanggal** — tanggal kirim ada pada kepala surel aslinya, yang wajib disimpan bersama berkas bukti. Ditafsirkan pada Amendment Pass Putaran 17 |
| `LAB-DEC-150` | Decision | **dr. Bima Prasetya, Sp.PK adalah PEMEGANG PERTAMA kewenangan validasi Patologi Klinik sekaligus PENETAP pemegang lain; validasi HANYA oleh DOKTER berkewenangan laboratorium.** Setiap validasi mencatat nama, **snapshot peran/jabatan** validator saat itu, waktu, dan jejak perubahan. Menjawab sebagian `DEC-LAB-011`; **menggantikan butir 2 `LAB-DEC-022`**; mengamandemen butir 1 `LAB-DEC-142` | dr. Bima Prasetya, Sp.PK | `approved` | dr. Bima Prasetya, Sp.PK — disampaikan Yoga Aji Pratama, 2026-09-24; **dikonfirmasi tertulis 2026-09-25, `LAB-EVD-010`** | Lihat BR-101. **Butir 3-4 `LAB-DEC-022` tetap berlaku** — pemegang kedua per shift dan pemegang Mikrobiologi/Patologi Anatomi diajukan `LAB-REQ-014`. Akibat: desain `S4` Patologi Klinik terbuka; rilisnya dan desain `S4d`/`S4e` tetap tertahan. Snapshot peran adalah kebutuhan **baru** bagi desain `S4`. `AC-238`, `AC-239` |
| `LAB-DEC-151` | Decision | **Balasan WhatsApp dokter hanya KONFIRMASI KOMUNIKASI, bukan bukti pembacaan ulang `LAB-DEC-004`.** Boleh dicatat, tetapi tidak menutup kewajiban pelaporan; baca ulang tetap lewat percakapan langsung. Menutup `PRD1-CLIN-01` | dr. Bima Prasetya, Sp.PK | `approved` | dr. Bima Prasetya, Sp.PK — disampaikan Yoga Aji Pratama, 2026-09-24 | Lihat BR-102. Menjadikan aturan sementara `LAB-DEC-136` butir 6 aturan tetap. **Mempertahankan aturan yang lebih ketat**, sehingga menutupnya tanpa `DR-LAB-001`/`DR-LAB-002` tidak melonggarkan wewenang klinis siapa pun. `AC-240` |
| `LAB-DEC-152` | Decision | **Di luar jam dokter utama bertugas, hasil divalidasi DOKTER LAIN YANG DITETAPKAN pada DISIPLIN YANG SAMA; kewenangan validasi TIDAK melintasi disiplin.** Pemegang yang disebut: Patologi Klinik dr. Bima Prasetya, Sp.PK; Mikrobiologi dr. Nabila Rahmawati, Sp.MK; Patologi Anatomi dr. Citra Maharani, Sp.PA. Pemegang tambahan menurut **daftar penetapan** pada kredensial Human Resource — **data, bukan keputusan**. `LAB-DEC-022` butir 3 menjadi **syarat rilis**: dua pemegang validasi per disiplin tercatat sebelum disiplin itu dipakai nyata. Menutup sisa `DEC-LAB-011` dan `LAB-OPEN-034` | dr. Bima Prasetya, Sp.PK | `approved` | dr. Bima Prasetya, Sp.PK — tertulis, `LAB-EVD-010`; kedudukannya ditetapkan Yoga Aji Pratama (Q1 pilihan A), 2026-09-25 | Lihat BR-103. **Mempertahankan aturan yang lebih ketat** — tidak lintas disiplin — sehingga menutup `LAB-OPEN-034` tanpa pernyataan tersendiri `DR-LAB-001`..`003` tidak melonggarkan wewenang siapa pun. Akibat: penahan `S4d`/`S4e` tertutup; **gerbangnya perlu dinilai ulang**. `AC-241` |
| `LAB-DEC-153` | Decision | **Perilis TIDAK WAJIB DOKTER.** Hasil tervalidasi dirilis pemegang kewenangan otorisasi yang ditetapkan — dokter atau pejabat non-dokter — lewat kode rilis per disiplin pada kredensial Human Resource. Jabatan calon perilis mengikuti *aturan laboratorium* (`LAB-OPEN-044`). Perilis ≠ pemvalidasi tetap berlaku (`LAB-DEC-120`). Menutup `DEC-LAB-018` | dr. Bima Prasetya, Sp.PK | `approved` | dr. Bima Prasetya, Sp.PK — tertulis, `LAB-EVD-010`; tafsiran *"pejabat/dokter"* = *atau* ditetapkan Yoga Aji Pratama (Q2 pilihan A), 2026-09-25 | Lihat BR-104. **Tafsiran dicatat terbuka**: surat memakai kata *dokter* hanya untuk validasi. Nol perubahan desain `S4` — rancangan bagian 20 sengaja netral atas pertanyaan ini. `AC-242` |
| `LAB-EVD-011` | Fact | **Keputusan pemilik modul dalam dua tangkapan layar** atas `LAB-CONFLICT-014` dan pertanyaan *pemakaian sebelum koreksi `S6`*, beserta empat klarifikasi pilihan pada sesi yang sama. **Disimpan verbatim** pada [`evidence/2026-09-25-keputusan-penyelesaian-order-dan-pemakaian-sebelum-s6.md`](evidence/2026-09-25-keputusan-penyelesaian-order-dan-pemakaian-sebelum-s6.md) | Yoga Aji Pratama | `diterima sebagai bukti` | Diterima 2026-09-25 | **Tangkapan kedua tampak terpotong** sesudah contoh JSON. Ditafsirkan pada Amendment Pass Putaran 18 |
| `LAB-DEC-154` | Decision | **Order hanya dapat `Completed` bila SELURUH pemeriksaan yang tidak batal dan tidak gugur sudah DIRILIS.** `PUT /lab-orders/{id}/complete` tetap tindakan manual tetapi diperiksa sistem; satu saja yang belum dirilis → `409` beserta rincian setiap pemeriksaan yang menahan dan label keadaannya. Pemeriksaan tanpa jalur validasi — Patologi Anatomi sampai `S4e`, Mikrobiologi sampai `MVP-10`, hasil `Sementara` — **menahan** order. Menutup `LAB-CONFLICT-014` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — `LAB-EVD-011`; Q3 *dirilis*, Q4 *tertahan* | Lihat BR-105. Syarat validasi tangkapan 2 butir 2 ikut terpenuhi lewat `VAL-133`. Batal/gugur tidak menahan — anggapan dari `AC-199`, dicatat terbuka. Hari ini nol layar memanggil endpoint itu. `AC-243`..`AC-245` |
| `LAB-DEC-155` | Decision | **Hasil resmi pasien = hasil yang Tervalidasi DAN Dirilis.** Hanya hasil resmi yang dipakai untuk keputusan klinis, cetak hasil final, pengiriman kepada pasien, dan integrasi di luar Laboratorium; hasil yang belum dirilis hanya untuk pemeriksaan internal, review analis, dan verifikasi teknis. **Validasi dan rilis boleh dipakai sebelum koreksi `S6` berdiri.** Menjawab bagian pertama pertanyaan `04-prd-to-mvp.md` 21.7; bagian keduanya dibuka `LAB-OPEN-045` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — `LAB-EVD-011`; Q1 *rilis sudah resmi* | Lihat BR-106. Pertanyaan induknya semula juga ditujukan kepada `DR-LAB-001`; konfirmasi klinis atas pemakaian tanpa jalur koreksi **diusulkan** bersama `LAB-OPEN-045`. Nilai kritis sebelum rilis tetap `DEC-LAB-017`. Nol perubahan source. `AC-246` |
| `LAB-DEC-156` | Decision | **Label keadaan pemeriksaan di layar:** *Menunggu Hasil* (`NotEntered`), *Draft*, *Menunggu Validasi* (`Final`), *Tervalidasi*, *Dirilis*. Tindakan Draft → Final bernama **Pemeriksaan Selesai** — nama tombol, bukan keadaan; Final tetap langsung masuk antrean validasi. *Dalam Pemeriksaan*/*Selesai* tetap label order | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — `LAB-EVD-011`; Q2 *Final = Menunggu Validasi* | Lihat BR-107. **Mengamandemen tabel label BR-88 butir 5 (`LAB-DEC-135`)** — label *Final* menjadi *Menunggu Validasi*. `LAB-DEC-080` utuh: nol status tersimpan. Patologi Anatomi tidak disentuh (`LAB-FE-029`, `LAB-FE-024`). Frontend saja, nol kontrak. `AC-247` |
| `LAB-CONFLICT-014` | Conflict | **`PUT /lab-orders/{id}/complete` memindahkan order `InProcess` → `Completed` tanpa memeriksa hasil**, sedangkan `LAB-DEC-135`/`AC-199` menyatakan *Selesai* hanya bila seluruh pemeriksaan tidak batal sudah dirilis — dua *Selesai* berbeda arti sesudah `S4`. Ditemukan desain `S4` (`02-backend-architecture.md` 20.12) | Yoga Aji Pratama | `closed` | Ditutup 2026-09-25 oleh `LAB-DEC-154` | `LabOrderService.cs:1051-1059`; `LabFilterMetadataFactory.cs:387`. **Dicatat di decision log baru saat ditutup** — sebelumnya hanya tercatat di arsitektur backend dan manifest |
| `LAB-DEC-157` | Decision | **Penautan PDF hasil laboratorium eksternal: DATANYA SATU di dokumen klinis pasien milik Clinical Management** (`TrxPatientClinicalDocument`, jenis `LaboratoryResult`, sumber `ExternalHospital`), **PINTU UNGGAHNYA di Laboratorium.** Laboratorium tidak menyimpan salinan. Penyimpanan berkas tetap `DEC-LAB-016`. Menjawab bagian pemilik `DEC-LAB-026`; membuka `LAB-COORD-018` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q1 | Lihat BR-108. **Mengamandemen `LAB-DEC-030`** pada tempat data penautan PDF; pemilik alur kerjanya tetap Laboratorium. Bukti lapangan: `Bagian Ketiga` `CAP-014`, `BP-003`. Kemampuan Clinical Management belum ber-`CAP` |
| `LAB-DEC-158` | Decision | **PDF eksternal ditautkan ke PASIEN DAN KUNJUNGAN; pengunggah wajib mencocokkan DUA identitas pasien sebelum menyimpan; berkas salah pasien ditandai SALAH INPUT beralasan (`EnteredInError`) oleh pengunggah atau kepala instalasi, tidak dihapus.** Berkas eksternal bukan hasil resmi laboratorium ini. Menutup `DEC-LAB-026` bersama `LAB-DEC-157` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q2, Q3 | Lihat BR-109. Status `EnteredInError` sudah ada pada dokumen klinis pasien. `AC-248`, `AC-249` |
| `LAB-DEC-159` | Decision | **`S16` dipecah; `S16a` berisi tiga laporan dengan definisi tetap:** jumlah pemeriksaan **pada tanggal rilis** (batal tidak dihitung); angka penolakan **wadah tidak layak ÷ wadah yang diputuskan**, menurut `DecidedAt`, per disiplin dan alasan; TAT **dari wadah layak sampai hasil dirilis**, per disiplin, cito dan rutin terpisah — sama dengan `AC-17`. Delapan laporan lain tetap `DEC-LAB-025` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q4-Q7 | Lihat BR-110. **Mengesahkan `DEC-LAB-007`**, usulan sejak 2026-09-01. Seluruh data milik Laboratorium; nol kontrak lintas modul. `AC-250`..`AC-252` |
| `LAB-DEC-160` | Decision | **Laporan `S16a` dibuka lewat HAK AKSES TERSENDIRI** yang diberikan admin kepada jabatan kepala instalasi dan manajemen; hak baca daftar Laboratorium tidak otomatis membukanya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q8 | Lihat BR-111. `AC-253` |
| `LAB-DEC-161` | Decision | **Lokasi, pola, dan metode pengambilan specimen MELEKAT PADA WADAH, diisi di halaman hasil oleh PENGISI HASIL disiplinnya (PA: Dokter Lab; Mikrobiologi: analis) sampai Final, berjejak.** Lokasi dan metode dari daftar terkendali dengan `Lainnya` + keterangan wajib — nilai baru hanya lewat kepala instalasi. Pola: Tunggal/Serial/Hormonal. Kewajiban disetel kepala instalasi per kategori atau jenis pemeriksaan, bawaan opsional. Menutup `DEC-LAB-023` dan `LAB-DEC-145` butir 4 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q9-Q11 | Lihat BR-112. **Pertentangan dengan `LAB-EVD-003` `CAP-006`** (daftar tumbuh otomatis) diselesaikan ke arah blueprint. Pola `LAB-DEC-107`, `LAB-DEC-112`, `LAB-DEC-099`, `VAL-95`. `AC-254`..`AC-256` |
| `LAB-DEC-162` | Decision | **Blok parafin dan slide TIDAK dilacak sebagai benda fisik** sampai ada kebutuhan tertulis; jumlahnya, bila perlu tercetak, menjadi parameter laporan PA (`LAB-DEC-086`). **Seri nomor Sitologi dan FNAB DITAHAN** sampai cetakannya diserahkan; sampai itu seri PA berlaku | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q12, Q13 | Lihat BR-113. `DEC-LAB-022` menyempit ke seri nomor saja, menunggu `LAB-OPEN-039` |
| `LAB-DEC-163` | Decision | **Di luar nilai kritis dan koreksi, TIDAK ADA kejadian lab yang wajib memberi tahu dokter; `S8` dilebur menjadi dependency `S5` dan `S6`** atas `LAB-COORD-017`. Hasil dirilis tidak mengirim pemberitahuan. Menutup `DEC-LAB-024` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q14 | Lihat BR-114. Nilai kritis tetap `LAB-DEC-004`; koreksi tetap `DEC-LAB-014` |
| `LAB-DEC-164` | Decision | **Order dari MCU memakai JALUR PEMESANAN BIASA.** Paket, harga paket, rekap perusahaan, dan penerima hasil selain pasien milik layanan MCU kelak. **`S19` ditutup dari modul Laboratorium sesudah pemesanan dari kunjungan `MedicalCheckup` terbukti berjalan.** Menutup `DEC-LAB-027` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-25 — putaran 19 Q15 | Lihat BR-115. **Mengamandemen `LAB-DEC-030`** pada baris *order dari MCU*. Verifikasi berjalan diteruskan ke `/trace-existing-capabilities` |
| `LAB-DEC-165` | Decision | **Hasil PILIHAN yang di luar rujukan ditulis dengan TEKS di belakang nilainya: `+2 — Di luar rujukan`.** Bukan huruf `H`/`L` (hasil pilihan tidak punya arah naik/turun) dan bukan simbol seperti `*` (tidak menjelaskan dirinya sendiri); teks utuh tetap terbaca di cetak hitam-putih. Warna boleh menyertai, tidak pernah menjadi satu-satunya penanda. Menutup `FR-14.9` dan `02-backend-architecture.md` 19.10 butir 2 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-01 — *"rekomendasikan keputusan terbaiknya saya setujui keputusan tersebut langsung"*; rekomendasi = usulan rancangan `r33` 28.3 | Turunan `LAB-FE-015`. Diterapkan `FE-LAB-36` (konstanta `LAB_REFERENCE_FLAG_SUFFIX`). Nol perubahan backend — `referenceFlag = OutOfReference` sudah dikirim `BE-LAB-69` |
| `LAB-DEC-166` | Decision | **Rincian order (`GET /lab-orders/{id}`) membawa identitas pasien** — nama, No. RM, jenis kelamin, tanggal lahir, No. kunjungan, tipe kunjungan, dan unit layanan — supaya Halaman Hasil per order menampilkan *Informasi pasien* dan hasil tidak diisi tanpa melihat pasiennya. Aditif; umur dihitung layar dari tanggal lahir | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-01 — *"Informasi pasien … tambahkan di backend"* | `LAB-API-v1` `r38` bagian 33; `BE-LAB-87`; `FE-LAB-36`. Ruas dibaca dengan jalur yang sama dengan daftar pantau, sehingga satu pasien terbaca sama di semua layar. Laporan Patologi Anatomi ikut terisi |
| `LAB-DEC-167` | Decision | **Respons specimen menyebut Spesifik Specimen yang tercatat, dan koreksi menggantinya berdasar selisih.** (1) `LabSpecimenResponse.specimenDetails` = daftar `{labSpecimenDetailTypeId, detailName, isActive}` — `detailName` adalah **nama snapshot** saat dipilih, `isActive` keadaan data induk hari ini; `null` = tidak dimuat, `[]` = nol rincian. (2) Dibawa `GET /lab-specimens/by-order/{labOrderId}` yang sudah ada — nol endpoint baru. (3) `PATCH /lab-specimens/{id}/correction`: rincian tercatat yang dikirim ulang **dibiarkan** (baris, snapshot, dan keadaan nonaktifnya); yang tidak dikirim dihapus; yang **baru** wajib aktif (`VAL-150`) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — *"yaaa saya setujui"* atas `LAB-REQ-015`, pilihan A ketiga butir | `LAB-API-v1` `r39` bagian 34; `LAB-VAL-v1` `r16` `VAL-150`; `BE-LAB-88`; sisa `FE-LAB-32`. Menutup empat cacat yang ditemukan verifikasi `FE-LAB-32`: formulir koreksi kosong, centang mengganti seluruh pilihan, pilihan yang kini nonaktif menggagalkan koreksi apa pun, dan nama snapshot tertimpa nama data induk terbaru |
| `LAB-EVD-012` | Fact | **Keputusan tertulis pemilik modul *Keputusan untuk Membuka Roadmap FE/BE Laboratorium yang masih tertahan*** — 14 butir dan penggolongan ulang penahan — beserta enam klarifikasi pilihan pada sesi yang sama. **Disimpan verbatim** pada [`evidence/2026-10-06-keputusan-pembuka-roadmap-tertahan.md`](evidence/2026-10-06-keputusan-pembuka-roadmap-tertahan.md) | Yoga Aji Pratama | `diterima sebagai bukti` | Diterima 2026-10-06 | Teks lengkap, tidak terpotong. Ditafsirkan pada Amendment Pass Putaran 21 |
| `LAB-DEC-168` | Decision | **Satu wadah specimen mewakili TEPAT SATU lokasi anatomi/sumber; beberapa lokasi = beberapa wadah bernomor sendiri.** Satu order tetap boleh banyak wadah; Spesifik Specimen `LAB-DEC-098` tidak berubah. Menutup `DEC-LAB-029` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 1 | Lihat BR-116. Usulan baseline gerbang `LAB-RCG-001-r11` diterima apa adanya. Konfirmasi `DR-LAB-003` lewat `LAB-SIGN-002`. `AC-257` |
| `LAB-DEC-169` | Decision | **Fiksasi dicatat PER WADAH bila pemeriksaannya membutuhkan** — jenis dari data induk terkendali, tanggal dan jam mulai, petugas, catatan; atau `Tidak Memerlukan Fiksasi`. Dicatat petugas yang memfiksasi atau menerima informasinya. Menutup `DEC-LAB-028` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 3 | Lihat BR-117. Setelan *pemeriksaan mana yang membutuhkan* mengikuti pola `LAB-DEC-161` butir 4 — arah turunan. Isi daftar dan setelannya `LAB-OPEN-050`. `AC-258` |
| `LAB-DEC-170` | Decision | **Sitologi dan FNAB memakai SERI NOMOR CETAK SENDIRI**, terpisah dari Histopatologi; nomor order tetap satu identitas global; bentuk nomor tiap seri disetel, tidak tertanam di kode. Tidak lagi menunggu cetakan `LAB-OPEN-039`. Menutup `DEC-LAB-022` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 2 | Lihat BR-118. **Mengamandemen `LAB-DEC-162` butir 3** (seri ditahan) dan **`LAB-DEC-117`** (alokasi PA per seri, bukan per disiplin). Kepala Instalasi dapat mempertahankan pola lama lewat setelan. Setelan `LAB-OPEN-049`, wajib final sebelum nomor pertama terbit (peringatan `LAB-OPEN-043`). `AC-259` |
| `LAB-DEC-171` | Decision | **Hasil Mikrobiologi `Sementara` BOLEH divalidasi dan dirilis**, tampil dan tercetak `HASIL SEMENTARA`; **`Definitif` adalah TAHAP BERIKUTNYA pemeriksaan yang sama, BUKAN koreksi `S6`**. Lintasan `Draft` → `Sementara` → `Definitif`; seluruh versi tersimpan. Hasil tanpa kualifikasi tetap bukan sementara. Menutup `DEC-LAB-020` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 4 | Lihat BR-119. Melengkapi `LAB-DEC-114`; `LAB-DEC-106` utuh. Konfirmasi `DR-LAB-002` lewat `LAB-SIGN-002`. `AC-260` |
| `LAB-DEC-172` | Decision | **Order BELUM Selesai selama ada pemeriksaan Mikrobiologi yang baru dirilis `Sementara`**; Selesai bila setiap pemeriksaan Mikrobiologi dirilis `Definitif` atau tanpa kualifikasi. Pasien menerima dokumen final sekali | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — klarifikasi Q5, pilihan yang direkomendasikan | Lihat BR-120. Menajamkan `LAB-DEC-154`; gerbang `LAB-DEC-139` mengikutinya. `AC-261` |
| `LAB-DEC-173` | Decision | **Dokter pemesan DIBERI TAHU ketika `Definitif` dirilis atas pemeriksaan yang `Sementara`-nya sudah dirilis**; di luar kasus itu rilis tetap tanpa pemberitahuan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — klarifikasi Q6, pilihan yang direkomendasikan | Lihat BR-121. **Mengamandemen `LAB-DEC-163` di satu titik**, dengan alasan `LAB-DEC-007`. Sarana `LAB-COORD-017`. `AC-262` |
| `LAB-DEC-174` | Decision | **Validasi PA: penulis Sp.PA BOLEH memvalidasi laporannya sendiri; RILIS oleh Sp.PA LAIN pemegang kewenangan rilis PA; analis tidak pernah memvalidasi maupun merilis PA.** Empat mata PA pindah ke rilis; disiplin lain tidak berubah. Menutup `DEC-LAB-021` — pilihan (c) | Yoga Aji Pratama | `approved` — **pemakaian nyata menunggu `DR-LAB-003`** | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 5 | Lihat BR-122. **Mengamandemen `LAB-DEC-003` bagi PA saja** dan **menyempitkan `LAB-DEC-153` bagi PA**. Melonggarkan aturan yang diteken `DR-LAB-003` → tanda tangannya **wajib** sebelum go-live (`LAB-SIGN-002`). Rilis PA butuh ≥ dua Sp.PA berkewenangan. `AC-263` |
| `LAB-DEC-175` | Decision | **Penyetuju akhir perubahan batas/penanda kritis adalah KEPALA INSTALASI**, dapat didelegasikan resmi kepada dokter penanggung jawab disiplin; **PENGUSUL ≠ PENYETUJU**; nilai baru tidak aktif sebelum disetujui dan berlaku pada tanggal mulai berlakunya; riwayat menyimpan delapan ruas. Menutup `DEC-LAB-012` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 6 + klarifikasi Q1 | Lihat BR-123. Menajamkan `LAB-DEC-023`: pengusul ≠ penyetuju menjaga contoh BR-19 tetap tertangkap. Konfirmasi Kepala Instalasi lewat `LAB-SIGN-002`. `AC-264` |
| `LAB-DEC-176` | Decision | **Alur nilai kritis: aturan terpicu → status kritis → pemberitahuan kepada dokter pemesan dan DPJP/unit bila episode aktif → acknowledgement → eskalasi bila lewat batas `Critical Notification SLA`** yang disetel per disiplin; seluruh percobaan, eskalasi, dan hasilnya tersimpan. Aturan kritis Mikrobiologi dan PA = data induk disiplinnya. Menutup sisi arsitektur `LAB-P0-004` dan `LAB-OPEN-014` | Yoga Aji Pratama | `approved` — arsitektur; isi klinis sebelum go-live | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 7 | Lihat BR-124. `LAB-DEC-004`, `136`, `137` utuh. Penerima eskalasi pada konfigurasi dan pemicu PA lewat `Status Hasil PA` = Kritis adalah **arah turunan**, bukan keputusan baru. Isi: `LAB-OPEN-041`, `046`, `047`. `AC-265`, `AC-266` |
| `LAB-DEC-177` | Decision | **Acknowledgement = laporan baca ulang `LAB-DEC-004` yang lengkap.** Eskalasi berhenti hanya bila itu tercatat; status dibaca di aplikasi dan balasan WhatsApp hanya bukti komunikasi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — klarifikasi Q2, pilihan yang direkomendasikan | Lihat BR-125. Menegakkan `LAB-DEC-004` dan `LAB-DEC-151` tanpa mengubah bunyinya — tidak butuh tanda tangan ulang. `AC-267` |
| `LAB-DEC-178` | Decision | **Koreksi sesudah rilis: diajukan pemegang kewenangan VALIDASI, disetujui PENGGUNA LAIN pemegang kewenangan RILIS disiplin itu; alasan wajib; versi lama `Superseded`; dokter pemesan DAN DPJP/unit aktif diberi tahu; TANPA batas waktu.** Menutup `DEC-LAB-014` dan sisa `LAB-P0-003` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 8 | Lihat BR-126. Menajamkan `LAB-DEC-007` — lebih ketat (dua orang) dan pemberitahuan diperluas. `LAB-DEC-082`, `LAB-DEC-020` utuh. Konfirmasi `DR-LAB-001`..`003` lewat `LAB-SIGN-002`. `AC-268` |
| `LAB-DEC-179` | Decision | **Hasil terrilis TIDAK DAPAT DIBATALKAN; `S6` punya dua jenis koreksi — GANTI NILAI dan TARIK HASIL (salah pasien) — keduanya lewat alur `LAB-DEC-178`.** Versi yang ditarik bertanda `Invalidated` dan tetap terlihat. Menutup `DEC-LAB-019` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — klarifikasi Q3, pilihan yang direkomendasikan | Lihat BR-127. Mengukuhkan `ARCH-GAP-LAB-09` dan `LAB-DEC-138`; `LAB-DEC-049` berlaku sampai rilis. Dampak tagihan diperiksa saat desain bersama `S10`. `AC-269` |
| `LAB-DEC-180` | Decision | **Versi koreksi atas hasil yang pernah dikirim ke pasien MASUK ANTREAN KIRIM `Pending` otomatis**; dokumen lama `Superseded`/`Invalidated` dan token QR-nya dicabut; pengiriman tetap lewat gerbang `S17` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — klarifikasi Q4, pilihan yang direkomendasikan | Lihat BR-128. `AC-270` |
| `LAB-DEC-181` | Decision | **Delapan laporan `S16b` DITUNDA ke `PHASE 2 — DEFERRED PENDING REPORT CATALOG`**; tidak ada laporan yang dikarang; satu laporan menjadi task hanya sesudah sebelas butir definisinya diserahkan. `S16a` tetap. Mengeluarkan `DEC-LAB-025` dari penahan roadmap | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 9 | Lihat BR-129. Nol AC baru — tidak ada yang dibangun |
| `LAB-DEC-182` | Decision | **Persetujuan kirim ke pasien BERBASIS PERAN (`LabResultPatientDeliveryApprover`, ditunjuk Kepala Instalasi), BUKAN GELAR**; hasil normal Released tanpa persetujuan tambahan; persetujuan kedua dapat disetel bagi kategori sensitif; seluruh pengiriman teraudit. Menutup `LAB-OPEN-029` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 10 | Lihat BR-130. **Mengamandemen `LAB-DEC-067` dan `LAB-DEC-139`** pada syarat *Profesor dan Dokter Lab*; sisa `LAB-DEC-139` utuh. `LAB-DEC-121` menyatakan ini bukan wewenang pemilik modul sendirian → konfirmasi Kepala Instalasi lewat `LAB-SIGN-002`; isi `LAB-OPEN-048`. `AC-271` |
| `LAB-DEC-183` | Decision | **Hasil Released menghasilkan PDF bernomor, berversi, berwaktu terbit, menyebut perilis, ber-QR, dan tertaut order/hasil; WhatsApp lewat layanan platform terpisah mengirim TAUTAN AMAN, bukan URL berkas publik; status kirim `Pending` → `Sent` → `Delivered`/`Failed` terpisah, dan kegagalannya tidak membatalkan Released.** Menutup sisi arsitektur `LAB-COORD-011` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 11 | Lihat BR-131. Penyediaan PDF dan WhatsApp tetap platform. Model tautan aman menunggu pemilik keamanan (`LAB-COORD-015`). `AC-272` |
| `LAB-DEC-184` | Decision | **Verifikasi QR tanpa login memakai TOKEN OPAQUE/BERTANDA TANGAN yang DAPAT DICABUT**; QR tanpa Patient ID, Order ID berurutan, maupun URL berkas; informasi minimum; tanpa pencarian pasien. Nama pemeriksaan tetap tidak tampil sampai kebijakan privasi menyetujui | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 12 | Lihat BR-132. Melengkapi `LAB-DEC-140`. Task verifikasi publik dibuat sesudah pemilik keamanan/platform menyetujui (`LAB-COORD-015`). `AC-273` |
| `LAB-DEC-185` | Decision | **Berkas klinis tinggal di PENYIMPANAN PRIVAT, bukan berkas publik web server**; metadata dan relasi di basis data; akses lewat endpoint terautentikasi atau tautan bertanda tangan sementara; izin, audit akses, validasi jenis dan ukuran; berkas lama `Superseded`/`Invalidated`, tidak dihapus. Menutup `DEC-LAB-016` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 13 | Lihat BR-133. Backend belum punya layanan penyimpanan bersama (diperiksa pada `6924b689`) → `LAB-COORD-019`. `AC-274` |
| `LAB-DEC-186` | Decision | **`Verified` = verifikasi administratif/lab (terbaca, pasien benar, terkait order/kunjungan, sumber tercatat), BUKAN persetujuan klinis; `Approved` = penerimaan klinis oleh tenaga medis berwenang.** Lintasan `Uploaded` → `Verified` → `Approved`; Laboratorium mengunggah dan memverifikasi, Clinical Management menyetujui bila perlu | Yoga Aji Pratama | `approved` — sisi Laboratorium | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` butir 14 | Lihat BR-134. Menjawab butir (3) `LAB-COORD-018` dari sisi Laboratorium; konfirmasi pemilik `clinical-management` tetap dibutuhkan. `AC-275` |
| `LAB-DEC-187` | Decision | **Penahan roadmap dipisahkan dari penahan go-live.** Butir yang sudah ditutup dan `LAB-OPEN-025`/`026` tidak lagi disebut menahan roadmap; `DEC-LAB-017`, `LAB-OPEN-044`, `LAB-OPEN-045`, `UNK-P14-03`, validator kedua, dan kewenangan HR menjadi penahan go-live; dependency penyediaan platform menahan implementasi, bukan roadmap | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — `LAB-EVD-012` bagian penutup | Lihat BR-135. Nol AC baru |
| `LAB-DEC-188` | Decision | *Butir Daftar Pasien OTC, langsung proses, kunci Lunas, dan menu Hasil dan Riwayat diputuskan 2026-10-08 oleh `LAB-DEC-215`..`LAB-DEC-218` (putaran 26); WhatsApp (`S17`) dan Tahan/Lanjutkan tetap ditunda.* *Butir "urutan Terima Sampling sebelum Konfirmasi" diputuskan 2026-10-07 oleh `LAB-DEC-193`..`LAB-DEC-197` (putaran 23); butir *grafik Beranda versi v1* diputuskan `LAB-DEC-204`..`LAB-DEC-211` (putaran 25); butir tunda lainnya tetap.* **Batas scope Amendment Pass putaran 22: hanya yang cukup di frontend.** Aksi Proses Pemeriksaan dan Selesaikan, aksi Terima Sampling, serta label, urutan, dan pengelompokan menu mengikuti FE v1. **Ditunda:** urutan Terima Sampling sebelum Konfirmasi, opsi *langsung proses*, kunci `Lunas`, menu Hasil dan Riwayat beserta WhatsApp (`S17`), Daftar Pasien OTC dan grafik Beranda versi v1, serta Tahan/Lanjutkan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — pembukaan putaran 22 | FE v1 `C:\Users\Admin\QuilvianSystemFrontendDev` sebagai rujukan alur, bukan evidence; F22-1..F22-8 |
| `LAB-DEC-189` | Decision | *Diamandemen 2026-10-08 oleh `LAB-DEC-218` — penangguhan kunci Lunas dicabut: pasien Tunai wajib lunas sebelum Proses; asuransi/penjamin lolos. Aksi Proses sendiri tetap.* *Diamandemen 2026-10-07 oleh `LAB-DEC-194` — Proses Pemeriksaan kini juga mensyaratkan pesanan sudah dikonfirmasi; ketiadaan kunci Lunas tetap.* **Aksi Proses Pemeriksaan pada ketiga daftar pasien lab, aktif bagi semua penjamin.** Memanggil `start-process` (`Accepted` → `InProcess`). **Mengamandemen `LAB-DEC-062`:** kunci Proses bagi Mandiri/tunai sampai `Lunas` **ditangguhkan** sebagai pengecualian sementara sampai jalur baca Billing (`LAB-COORD-010`, `LAB-REQ-008`) tersedia; sesudah itu kunci dipasang tanpa keputusan baru. Selama penangguhan, kepastian pembayaran pasien Tunai dipegang petugas secara manual | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — Q1 *Aktif untuk semua* | Lihat BR-136 butir 1–2. F22-4, F22-5. Nol perubahan kontrak. `LAB-OPEN-051`. `AC-276`, `AC-277` |
| `LAB-DEC-190` | Decision | *Diamandemen 2026-10-07 oleh `LAB-DEC-198` — Selesaikan juga dapat ditekan bila `resultProgress` kosong (nol pemeriksaan terhitung).* **Aksi Selesaikan hanya pada daftar Patologi Klinik dan Mikrobiologi, dapat ditekan hanya bila pesanan `InProcess` DAN keadaan order `AllReleased`.** Disembunyikan pada daftar Patologi Anatomi sampai `S4e`. `VAL-146` di backend tetap pengaman. **Tahan/Lanjutkan tidak ditawarkan** — tidak ada di v1 dan nol bukti lapangan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — Q2 *Proses + Selesaikan*, Q3 *PK & Mikro, bila semua dirilis* | Lihat BR-136 butir 3, 4, 6. Menegakkan `LAB-DEC-154` di layar, yang sampai hari ini nol pemanggil. F22-4, F22-6. `AC-278`, `AC-279`, `AC-281` |
| `LAB-DEC-191` | Decision | *Digantikan 2026-10-08 oleh `LAB-DEC-216` — Terima Sampling dijalankan lewat dialog di daftar, tidak lagi navigasi ke layar Wadah.* **Aksi Terima Sampling per baris membuka layar Wadah pesanan itu.** **Mengamandemen `LAB-DEC-060`:** jalan pintas *satu klik* diganti *satu navigasi* sampai ada endpoint penerimaan atomik; siklus hidup wadah dan kedua invariant penolakan (`LAB-FE-009`, `LAB-FE-010`) tetap. Merangkai `collect`/`receive`/`accept` dari frontend **ditolak**: tidak atomik dan langsung membentuk kelayakan tagih | Yoga Aji Pratama | **`superseded` oleh `LAB-DEC-216`** pada 2026-10-08 | Yoga Aji Pratama (pemilik modul), 2026-10-06 — Q4 *Buka layar Wadah* | Lihat BR-136 butir 5. F22-7. Endpoint atomik bila kelak diminta = amandemen `LAB-API-v1` + task backend. `AC-280` |
| `LAB-DEC-192` | Decision | **Susunan menu Laboratorium mengikuti urutan FE v1 beserta grup Laporan.** Tiga menu *Pemeriksaan …* dinamai *Daftar Pasien Lab Patologi Klinik/Patologi Anatomi/Mikrobiologi*; *Ringkasan Laboratorium* menjadi *Beranda*; Laporan Penerimaan dan Laporan Operasional masuk grup *Laporan*. Judul halaman mengikuti label menu. **Mengamandemen `LAB-DEC-048` pada label saja**; route, key, izin, tiga menu terpisah, dan penyaring identik tetap | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-06 — Q5 *Urutan v1 + grup Laporan* | Lihat BR-137. `LAB-FE-031`. `AC-282` |
| `LAB-DEC-193` | Decision | **Konfirmasi sah sebelum maupun sesudah wadah dinyatakan layak, sampai pesanan diproses** — pada `Requested` atau `Accepted` yang belum pernah dikonfirmasi; tetap hanya sekali; tetap ditolak pada `InProcess`, `Completed`, `Cancelled`, `OnHold`. Mengikuti FE v1 (F23-1). **Mengamandemen `LAB-DEC-061`** dan bagian 1a `LAB-STATE-v1` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q1 *Sebelum atau sesudah sampling* | Lihat BR-138 butir 1. F23-1, F23-3. Menuntut amandemen `LAB-STATE-v1` dan `LAB-VAL-v1` (`VAL-71`). `AC-283`, `AC-284` |
| `LAB-DEC-194` | Decision | **Proses Pemeriksaan wajib didahului Konfirmasi.** `start-process` sah hanya pada `Accepted` **yang sudah dikonfirmasi**, dibaca dari jejak konfirmasi, bukan status. Mengikuti FE v1 (F23-2). **Mengamandemen `LAB-DEC-189`**. **Menutup `LAB-OPEN-027` dalam bentuk lain:** konfirmasi tidak wajib sebelum `Accepted` — jalur `Requested` → `Accepted` tetap sah — tetapi wajib sebelum `InProcess` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q2 *Wajib, seperti v1* | Lihat BR-138 butir 3. Menuntut aturan validasi baru dan task backend. `AC-285`; `AC-276` dibaca bersama aturan ini |
| `LAB-DEC-195` | Decision | **Konfirmasi pada pesanan `Accepted` tidak mengubah status** — pesanan tetap *Diterima*; konfirmator, waktu, dan dokter pemeriksa tercatat. Status tidak pernah mundur; `Confirmed` tetap berarti dikonfirmasi sebelum wadah layak. Nol status baru | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q3 *Tetap Diterima* | Lihat BR-138 butir 2. `AC-283` |
| `LAB-DEC-196` | Decision | **Syarat konfirmasi hanya berlaku pada Proses Pemeriksaan.** Selesaikan tidak memeriksanya; pesanan yang sudah `InProcess` tanpa konfirmasi tetap dapat diselesaikan. Konfirmasi pada `InProcess` tetap tidak sah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q4 *Hanya saat Proses* | Lihat BR-138 butir 4. F23-6. `AC-286` |
| `LAB-DEC-197` | Decision | **Hak Konfirmasi dipisah menjadi aksi izin tersendiri pada resource `LabOrder` dan diberikan kepada jabatan Analis Laboratorium.** `POST /lab-orders/{id}/confirm` tidak lagi dijaga `LabOrder : Update`. Hak itu tidak membuka Batalkan maupun penulisan konteks klinis (`LAB-DEC-091`). Nama aksi diputuskan pada desain kontrak | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q5 *Izin Konfirmasi sendiri* | Lihat BR-138 butir 5. F23-5. Menjawab sebagian `Q-P22-03`; sisa Batalkan → `LAB-OPEN-052`. Menuntut amandemen `LAB-PERM-v1`, `LAB-API-v1`, task backend, dan langkah rilis pemberian izin. `AC-288` |
| `LAB-DEC-198` | Decision | **Selesaikan dapat ditekan pada pesanan `InProcess` yang `resultProgress`-nya kosong** (nol pemeriksaan terhitung — seluruhnya batal atau gugur), menyamai backend rancangan 22.7 butir 3. **Mengamandemen bunyi `AC-278` dan `LAB-DEC-190`; menutup `LAB-CONFLICT-017`** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — jawaban `Q-P22-01` capability map rev 7, *"memakai rekomendasi di atas"* | Sudah ditegakkan `FE-LAB-48` (⚠). Nol perubahan kontrak |
| `LAB-DEC-199` | Decision | **Layar membaca `400` maupun `409` dari `start-process` sebagai *status sudah berubah*; penyelarasan backend ke `409` (`BE-LAB-89`) menyusul dan tidak memblokir** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — jawaban `Q-P22-02` capability map rev 7 | Sudah ditegakkan `FE-LAB-47` (✅). `LAB-STATE-v1` `r7` bagian 1 tetap menulis `409` |
| `LAB-DEC-200` | Decision | **Setiap dokter aktif boleh dipilih sebagai dokter pemeriksa.** `VAL-73` tidak berubah; tidak ada penyaring dokter laboratorium, spesialisasi, disiplin, atau jadwal jaga. Berbeda dari FE v1 (F24-6) secara sadar: dokter lab tidak didahulukan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q1 putaran 24 *Semua dokter aktif* | BR-139 butir 1. Nol aturan validasi baru |
| `LAB-DEC-201` | Decision | **Daftar pilihan dokter pemeriksa disediakan modul Laboratorium dan dijaga `LabOrder : Confirm`.** Isinya hanya nama, kode, dan spesialisasi dokter aktif, dapat dicari; tanpa data kontak atau data pribadi lain. Tanpa izin baru dan tanpa izin master dokter HR; kebijakan `KioskRead` milik HR tidak diubah. Pola BR-65 (F24-7) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q2 putaran 24 *Pemegang izin Konfirmasi* | BR-139 butir 2–3. Menuntut amandemen `LAB-API-v1` — bentuk dan nama endpoint diputuskan pada desain kontrak; wajib memeriksa daftar yang sudah ada lebih dulu (bahan audit 1). `AC-289`, `AC-290` |
| `LAB-DEC-202` | Decision | **Urutan v1 dirilis serempak.** `BE-LAB-90`, perbaikan `LAB-DEC-203`, daftar dokter `LAB-DEC-201`, dan `FE-LAB-50` beserta pemakaian daftar baru dideploy dalam satu gelombang `MVP-12c`. Tidak ada Konfirmasi sementara lewat akun Administrator/SuperAdmin | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q4 putaran 24 *Tahan, rilis serempak* | BR-139 butir 6. Mengamandemen urutan 6ar.2 `backend-roadmap.md`. `AC-293` |
| `LAB-DEC-203` | Decision | **Jawaban detail pesanan wajib membawa lima ruas jejak konfirmasi** (`confirmedAt`, `confirmedByName`, `examinerDoctorName`, `confirmedByUserId`, `examinerDoctorId`) sesuai `LAB-API-v1` `r13`. Dicatat sebagai **perbaikan kesesuaian kontrak**, bukan aturan baru (F24-1, F24-2) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — butir 4 batas scope putaran 24 | BR-139 butir 5. Membuka AC tambahan (a) `FE-LAB-50`. `AC-292` |
| `LAB-DEC-204` | Decision | **Beranda Lab mengikuti seluruh susunan Beranda v1** — hero, empat kartu hari ini, empat kartu fokus operasional, grafik pemeriksaan per disiplin, sebaran, tren bulanan, pesanan terbaru, kesiapan operasional — **dengan komponen dan tema V2**; rekap status per rentang tanggal yang sudah ada **dipertahankan** di bagian paling bawah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q1 *Semua bagian v1* dan Q3 *Pertahankan di bawah* putaran 25 | BR-140 butir 1. Memutuskan butir *grafik Beranda versi v1* `LAB-DEC-188`. `AC-294` |
| `LAB-DEC-205` | Decision | **Kartu hari ini memakai hari kalender WIB (00.00–23.59) menurut waktu pesanan diminta.** *Menunggu* = belum *Selesai* dan tidak *Dibatalkan*; *Selesai* = berstatus *Selesai* beserta persentasenya; *CITO* = berkesegeraan CITO | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q4 *WIB; Menunggu tanpa batal* putaran 25 | BR-140 butir 2. Berbeda dari v1 secara sadar (v1 menghitung pesanan batal sebagai menunggu). Sumber kesegeraan diperiksa pada desain (bahan audit 1). `AC-295` |
| `LAB-DEC-206` | Decision | **Grafik per disiplin dan sebaran menghitung pemeriksaan tidak batal** pada pesanan yang diminta di **tahun terpilih**; satu pemilih tahun berlaku bagi ketiga grafik | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q5 *Pemeriksaan* dan Q11 *Ikut tahun di grafik tren* putaran 25 | BR-140 butir 4. v1 tidak punya rentang yang jelas (F25-2). `AC-297` |
| `LAB-DEC-207` | Decision | **Tren bulanan menghitung pesanan per bulan** (WIB, menurut waktu diminta) pada tahun terpilih dan tahun sebelumnya, **tanpa** *Dibatalkan* dan *Draft* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q6 *Pesanan non-batal per bulan* putaran 25 | BR-140 butir 5. `AC-298` |
| `LAB-DEC-208` | Decision | **Kartu *Petugas Laboratorium* v1 diganti *Hasil menunggu validasi*** — jumlah pemeriksaan yang hasilnya sudah final diisi tetapi belum divalidasi. Hanya angka; Laboratorium tidak membaca data pengguna/SDM untuk Beranda | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q2 *Ganti metrik lab* dan Q7 *Hasil menunggu validasi* putaran 25 | BR-140 butir 3. Selaras dengan antrean validasi (`FE-LAB-42`) — keadaan pastinya ditetapkan desain (bahan audit 2). `AC-296` |
| `LAB-DEC-209` | Decision | **Tabel pesanan terbaru berisi 10 pesanan terakhir diminta** dari ketiga disiplin — No. RM, nama pasien, pemeriksaan, disiplin, status, waktu diminta; klik baris membuka halaman pesanan yang sudah ada; tanpa halaman | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q8 *10 terbaru, klik buka pesanan* putaran 25 | BR-140 butir 6. Data pasien setara daftar pasien lab. `AC-299` |
| `LAB-DEC-210` | Decision | **Kesiapan operasional = waktu data terakhir dimuat, ringkasan progres hari ini, dan tombol *Perbarui Data*.** Pembaruan otomatis realtime (SignalR seperti v1) **ditunda** — backend Lab belum menyiarkan perubahan pesanan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q9 *Waktu muat + tombol Perbarui* putaran 25 | BR-140 butir 7. `AC-300` |
| `LAB-DEC-211` | Decision | **Beranda dapat dibuka setiap pemegang `LabOrder : Read`**; data ringkasan baru ikut dijaga izin itu. Nol aksi izin baru, nol pemberian izin saat rilis | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — Q10 *Pemegang LabOrder : Read* putaran 25 | BR-140 butir 8. `AC-301` |
| `LAB-DEC-212` | Decision | **Batas scope Amendment Pass putaran 26:** mencatat enam keputusan `LAB-EVD-013`, menandai keputusan yang tergeser, mengoreksi klaim `AC-45`, dan menutup dua konflik implementasi (jalur baca Lunas, `LAB-FE-011`). **Di luar:** WhatsApp `S17`, status/prefiks antrean kunjungan kiosk Lab milik Registrasi, aturan internal Billing, Tahan/Lanjutkan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — Q0 *Setuju* putaran 26 | Putaran 26 bagian *Batas scope*. F26-1 |
| `LAB-DEC-213` | Decision | **Kiosk jalur Laboratorium mengikuti v1.** Sesudah pasien memilih Laboratorium, kiosk membaca pesanan Lab aktif pasien: **ada** → *Konfirmasi Kehadiran* (data booking + daftar pemeriksaan; nol tulis selain sesi kiosk); **tidak ada** → Jenis Kunjungan → Pembayaran → Konfirmasi → kunjungan dibentuk route kiosk Registrasi (`LAB-DEC-053`) ke unit Laboratorium tanpa dokter → tiket antrean. **Mengamandemen `LAB-DEC-052`:** pertanyaan *membawa surat dokter* dicabut; penanda sesi `hasPhysicianRequest` = ada pesanan Lab aktif; pasien bersurat dokter luar mendaftar lewat Jenis Kunjungan *Rujukan*. Layar serah-terima `FE-LAB-13` dicabut. **`AC-45` tetap berlaku** (F26-1) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 1; status persetujuan 2026-10-08 Q3 putaran 26 *Approved + kabari Andry* | BR-141 langkah 1. Pemberitahuan kepada Andry Zain (`registration-management`): `LAB-REQ-018`. `AC-302`..`AC-304` |
| `LAB-DEC-214` | Decision | **Penerimaan Sampling/Specimen satu halaman dengan satu Simpan, untuk pasien Umum maupun Rujukan**, susunan v1: Data Pasien (Tipe Pasien), Data Pemeriksaan (pembayaran, Kategori Lab, tanggal sampling, perujuk, diagnosa), Pemeriksaan Lab (+CITO, Grand Total), Informasi Specimen. Rangkaian: kunjungan (bila belum ada) → satu pesanan per kategori → wadah → diambil → diterima; dapat dilanjutkan bila gagal di tengah. **Mengamandemen `LAB-DEC-045`:** layar tidak lagi menetapkan kelayakan (`LAB-DEC-219`) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 2 | BR-141 langkah 3. `AC-306`, `AC-307` |
| `LAB-DEC-215` | Decision | **Menu Daftar Pasien OTC.** Isinya kunjungan dari kiosk ke unit Laboratorium yang belum memiliki pesanan Lab dan tidak *Dibatalkan*/*Tidak Hadir*; tab Umum/Rujukan menurut penanda rujukan kunjungan; pilihan periode v1. Aksi **Proses** membuka Penerimaan Sampling dengan kunjungan terisi. Aksi *Hapus* v1 **tidak** ditiru — membatalkan kunjungan wewenang Registrasi. Memutuskan butir tunda `LAB-DEC-188` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 2 | BR-141 langkah 2. `AC-305` |
| `LAB-DEC-216` | Decision | **Terima Sampling dijalankan dari Daftar Pasien Lab lewat dialog**, menggantikan navigasi ke layar Wadah: wadah `Planned`/`Collected` dicatat sampai `Received` (pesanan dokter tanpa wadah meminta jenis specimen); wadah `Received` yang diambil orang lain dinilai Layak/Tidak Layak. Dialog Konfirmasi memuat centang *Sampling diterima* (hanya mencatat penerimaan) dan *Langsung proses* (hanya bila wadah sudah layak dan pembayaran lolos). **Menggantikan `LAB-DEC-191`** — risiko tidak atomik yang dulu menjadi alasan penolakan ditangani dengan melanjutkan dari status wadah; **memulihkan `LAB-DEC-060`** dalam bentuk dialog; memutuskan butir tunda *langsung proses* `LAB-DEC-188` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 3, dipersempit butir 6 | BR-141 langkah 4–5. `AC-308`, `AC-309`, `AC-312` |
| `LAB-DEC-217` | Decision | **Menu Hasil dan Riwayat Lab Patologi Klinik/Patologi Anatomi/Mikrobiologi** mengikuti v1: data sama dengan daftar pantau; kolom Order, Registrasi, Pasien, Status Pemeriksaan, Dokter Konfirmator, Selesai Pemeriksaan, Pembayaran; aksi Lihat Hasil, Nota Lab, Label Lab, Label Goldar. *Kirim Hasil ke Pasien* tampil **nonaktif beralasan** sampai `S17`. Validasi dan rilis empat mata V2 tetap berlaku. Memutuskan butir tunda *menu Hasil dan Riwayat* `LAB-DEC-188` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 4 | BR-141 langkah 7. `AC-313` |
| `LAB-DEC-218` | Decision | *Diamandemen 2026-10-08 oleh `LAB-DEC-223` — kunci dikecualikan bagi kunjungan Rawat Inap dan IGD; pasien Tunai keduanya tampil *Ditagih Kemudian* (`LAB-DEC-224`). Jenis kunjungan lain tetap dikunci.* **Kunci Lunas berlaku pada Proses Pemeriksaan saja.** Pasien Tunai wajib lunas; asuransi/penjamin perusahaan lolos (v1). Satuannya **kunjungan**: tanggungan pasien pada kalkulasi terakhir invoice dibanding tender sukses. Sebelum wadah layak statusnya *Belum Ditagih* dan Proses tetap nonaktif. Terima Sampling **tidak** memeriksa pembayaran (F26-5). Backend menolak `start-process` `409`. **Mengamandemen `LAB-DEC-189`** (penangguhan kunci dicabut) dan **memasang kembali `LAB-DEC-062`** untuk Proses; **menutup `LAB-OPEN-051`** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-07 — `LAB-EVD-013` butir 5 | BR-141 aturan 4. Jalur bacanya `LAB-DEC-220`. `AC-310`, `AC-311` |
| `LAB-DEC-219` | Decision | **`VAL-09` dipertahankan.** Pengambil sampel tidak boleh menetapkan kelayakan wadahnya sendiri; Penerimaan Sampling dan Terima Sampling oleh petugas yang sama berhenti di `Received`, dan Layak/Tidak Layak ditetapkan petugas lain lewat dialog Terima Sampling. Berbeda dari v1 (satu petugas) secara sadar | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — `LAB-EVD-013` butir 6 | F26-6. `AC-309` |
| `LAB-DEC-220` | Decision | **Jalur baca status Lunas sementara milik Laboratorium.** Laboratorium membaca tabel Billing **baca-saja** dengan rumus yang sama dengan riwayat pembayaran Billing, sampai pemilik Billing menyediakan jalur resmi lewat `LAB-REQ-008`; sesudah itu Laboratorium beralih tanpa keputusan baru. `LAB-REQ-008` diperbarui dengan pemberitahuan jalur sementara ini. Risiko simpangan rumus dicatat `LAB-OPEN-053` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — Q1 putaran 26 *Sementara, lapor Billing* | F26-7. `LAB-COORD-010` berubah sifat: bukan lagi penahan, tetapi penggantian jalur |
| `LAB-DEC-221` | Decision | **Metode pembayaran dipilih petugas hanya ketika kunjungan baru dibentuk di layar Penerimaan** (Tunai/Asuransi, jalur yang sama dengan form Pendaftaran Pasien Laboratorium); kunjungan yang sudah ada (kiosk/OTC) menampilkannya **baca-saja**. **Mengamandemen `LAB-FE-011`** dan bagian *baca-saja untuk jalur rujukan* `LAB-DEC-044`; Laboratorium tetap tidak menjalankan aturan uang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — Q2 putaran 26 *Pilihan saat daftar* | F26-8. `AC-307` |
| `LAB-DEC-222` | Decision | **Batas scope Amendment Pass putaran 27:** lingkup kunci Lunas Proses Pemeriksaan menurut jenis kunjungan, dan tampilan kolom Pembayaran bagi yang dikecualikan. Penagihan Lab IGD (milik Billing) dan waktu pelunasan rawat inap (milik Rawat Inap/Billing) **di luar scope** — dicatat sebagai titik koordinasi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — *Sesuai* | `Q-LAB-P26-01`, capability map revision 8 |
| `LAB-DEC-223` | Decision | **Kunci Lunas Proses Pemeriksaan dikecualikan HANYA bagi kunjungan Rawat Inap (`Inpatient`) dan IGD (`Emergency`).** Kunjungan jenis lain — rawat jalan termasuk kiosk, MCU, Telemedicine, `Unknown`, dan jenis baru di masa depan — **tetap wajib lunas** (fail-closed). Pasien rawat inap dan IGD membayar di akhir perawatan, sehingga pemeriksaannya tidak boleh tertahan oleh tagihan yang memang belum jatuh tempo. **Mengamandemen `LAB-DEC-218`**; menyelesaikan `LAB-CONFLICT-018` dari sisi Laboratorium | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — Q1 *Rawat jalan & kiosk saja*, Q2 *Tetap dikunci* | F27-1..F27-4. `VAL-152`/`VAL-153` dipersempit. `AC-314`..`AC-316` |
| `LAB-DEC-224` | Decision | **Pasien Tunai rawat inap dan IGD tampil *Ditagih Kemudian* pada kolom Pembayaran** — status baru `Deferred`, `isPaymentCleared` = `true`, sisa tagihan tetap ditampilkan bila tagihannya sudah terbit (0 bila belum). Pasien berpenjamin pada kedua jenis kunjungan itu tetap *Asuransi / Penjamin*. Label *Asuransi / Penjamin* **tidak** dipakai untuk pasien Tunai agar tidak terbaca seolah dijamin | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 — Q3 *Status baru 'Ditagih Kemudian'* | Menambah satu nilai `paymentStatus` pada `LAB-API-v1`. `AC-314`, `AC-315`, `AC-317` |
| `LAB-DEC-225` | Decision | **Batas scope Amendment Pass putaran 28:** membawa tanda CITO dari permintaan ke pemeriksaan saat wadah direncanakan (wadah pertama dan pengganti), jejaknya, dan perbaikan data lama. Wewenang *Tandai Cito* (`VAL-03`/`VAL-04`), batas waktu cito, aturan pemesanan dokter/kiosk, dan Billing **di luar scope** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-09 — *Setuju* | `02-backend-architecture.md` 28.8 (usulan `LAB-CONFLICT-019`) |
| `LAB-DEC-226` | Decision | **Pemeriksaan yang dibuat saat wadah direncanakan mewarisi kesegeraan permintaan yang dipenuhinya:** permintaan `Cito` → pemeriksaan `Cito`. Penanda `UrgencyMarkedAt`/`UrgencyMarkedByUserId` **dibiarkan kosong** — penanda tetap berarti *ditandai susulan lewat Tandai Cito*; asal CITO ditelusuri lewat permintaan yang tertaut (`FulfilledExaminationId`). Pesanan lama tanpa baris permintaan tetap `Routine` (aditif, seperti `VAL-68`/`VAL-69`). Menutup `LAB-CONFLICT-019` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-09 — Q1 *Ikut CITO otomatis*, Q3 *Dibiarkan kosong* | F28-1, F28-2, F28-6, F28-7. `AC-318`, `AC-319`, `AC-322` |
| `LAB-DEC-227` | Decision | **Pemeriksaan di wadah pengganti (ambil ulang) mewarisi kesegeraan TERAKHIR pemeriksaan yang digantikannya** — termasuk *Tandai Cito* maupun pencabutan susulan oleh dokter — **beserta penandanya** (turunan langsung: penanda menjelaskan keadaan yang diwarisi). Tanda awal permintaan tidak dipakai lagi pada jalur ini | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-09 — Q2 *Keadaan terakhir pemeriksaan lama* | F28-3, F28-4. `AC-320` |
| `LAB-DEC-228` | Decision | **Perbaikan data lama saat rilis hanya bagi pekerjaan yang masih berjalan:** permintaan `Cito` berstatus `Fulfilled` yang pemeriksaan tertautnya `Routine`, **penandanya kosong**, pemeriksaannya bukan *Gugur*/*Dibatalkan*, dan pesanannya bukan *Selesai*/*Dibatalkan* → diubah `Cito`. Setiap pemeriksaan yang diubah mendapat **satu baris riwayat** `Routine → Cito` berketerangan *perbaikan data `LAB-CONFLICT-019`* dengan pelaku sistem; penanda tetap kosong. Pesanan selesai **tidak disentuh** (sejalan `VAL-04`; laporan TAT masa lalu tidak bergeser). Pemeriksaan yang penandanya terisi (dokter pernah memutuskan) tidak disentuh. Perbaikan wajib idempoten dan didahului hitung kering di produksi. **Diperluas 2026-10-09 lewat `LAB-REQ-022` butir 4:** pemeriksaan di wadah pengganti yang kehilangan CITO dari pemeriksaan yang digantikannya ikut dipulihkan dengan syarat berjalan yang sama, mewarisi kesegeraan dan penanda pemeriksaan lama (`02-backend-architecture.md` 29.6 langkah 3) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-09 — Q4 *Hanya pesanan yang masih berjalan*, Q5 *Satu baris riwayat per pemeriksaan* | F28-4, F28-8 (dev: 0 baris). `AC-321` |
| `LAB-DEC-229` | Decision | **Rencana ulang sesudah wadah dibatalkan mewarisi keadaan terakhir pemeriksaan pendahulu** (pesanan dan prosedur yang sama) beserta penandanya — sama dengan aturan ambil ulang `LAB-DEC-227`. Tanpa pemeriksaan pendahulu → kesegeraan permintaan (`LAB-DEC-226`). Menutup temuan T1 `BE-LAB-94` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-09 — *Samakan: keadaan terakhir* | F29-1, F29-2. `AC-323`..`AC-325`. Tafsir pendahulu tanpa penanda: `LAB-REQ-023` |
| `LAB-CONFLICT-018` | Conflict | **Kunci Lunas menahan Proses Pemeriksaan pasien rawat inap dan IGD Tunai** — rawat inap tertahan *Belum Lunas* sampai pulang, IGD tertahan *Belum Ditagih* selamanya. Ditemukan capability map revision 8. **Diselesaikan sisi Laboratorium oleh `LAB-DEC-223`/`LAB-DEC-224` 2026-10-08**; penagihan Lab IGD diteruskan `LAB-COORD-020` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-10-08 | F27-1..F27-3 |
| `LAB-DEC-149` | Decision | **Hasil Patologi Klinik diisi pada HALAMAN DETAIL PER ORDER**, seluruh pemeriksaan yang tidak batal dalam satu tabel isian — parameter, hasil, satuan, nilai rujukan, penanda `L`/`H`. Final tetap per pemeriksaan; Daftar Kerja tetap sebagai antrean dan membuka halaman ini; dialog modal pengisian hasil dicabut. Menutup `LAB-CLOSE-016` dan `LAB-CONFLICT-013` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-100 dan `LAB-FE-017`. Sejajar halaman Mikrobiologi yang juga per order (`lab-monitoring/microbiology/[slug]` menerima `labOrderId`). Memenuhi PRD `FR-PK-001`. Bukti keadaan lama: capability map revision 5 `CAP-P14-18`. `AC-234`..`AC-237` |
| `LAB-CONFLICT-013` | Conflict | **Isian hasil Patologi Klinik berada di dialog modal Daftar Kerja**, dan nol halaman detail hasil per order, bertentangan dengan `LAB-FE-016`. Ditemukan capability map revision 5 | Yoga Aji Pratama | `closed` | Ditutup 2026-09-24 oleh `LAB-DEC-149` | `01-existing-capability-map.md` revision 5 bagian C |
| `LAB-DEC-148` | Decision | **Penunjukan pemvalidasi dan perilis disimpan pada KREDENSIAL HUMAN RESOURCE (`WfpClinicalPrivilege`); Laboratorium HANYA MEMBACA.** Enam kode kewenangan — validasi dan rilis untuk Patologi Klinik, Mikrobiologi, dan Patologi Anatomi — ditambahkan ke katalog Human Resource. Ditunjuk berarti memegang kode yang sesuai, berstatus aktif, dalam masa berlaku; suspend, revoke, atau kedaluwarsa menolak seketika. Lapis jabatan `LAB-DEC-142` tetap. Menutup `LAB-CLOSE-015` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-99. Menetapkan wadah `LAB-DEC-142` butir 2 dan `LAB-DEC-143`. Bukti: capability map revision 5 `CAP-P14-09`; preseden `OperatingRoomCredentialResolver.cs:15-42`; katalog dan kewenangan sama-sama **punya jalan tulis**. Membuka `LAB-COORD-016`. **Tidak menjawab `DEC-LAB-011`** — hanya menyediakan usulan konkret. `AC-229`..`AC-233` |
| `LAB-DEC-147` | Decision | **Hasil yang sudah FINAL DITOLAK (`409`) bila isinya disimpan ulang** — nilai Patologi Klinik; status temuan, isolat, antibiogram, kualifikasi, jenis biakan, dan metode uji Mikrobiologi; serta catatan konsultasi. Reopen beralasan adalah satu-satunya jalan. **Mikrobiologi diperbaiki sekarang**, sebelum `S4b` masuk rilis. Menutup `LAB-CLOSE-014` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-98. Pola `VAL-96` Patologi Anatomi. Bukti celahnya: capability map revision 5 `CAP-P14-04`. Alasan kuncinya: `FinalizedAt` dicetak sebagai Tanggal Selesai (`LAB-DEC-118`) dan Waktu Issued (`LAB-DEC-096`). Menuntut amandemen `LAB-VAL-v1` dan **task perbaikan** `S4b` backend dan frontend. `AC-225`..`AC-228` |
| `LAB-DEC-146` | Decision | **Tindakan atas hasil memakai RESOURCE HAK AKSES TERSENDIRI.** Mengisi hasil Patologi Klinik dan Mikrobiologi, Simpan Final, Reopen, dan mencatat konsultasi pindah dari `LabExamination : Update` ke izin hasil; batal, cito, dan duplo tetap. Izin hasil diberikan kepada jabatan analis, bukan dokter pemesan maupun Petugas Lab administrasi. Laporan Patologi Anatomi tidak disentuh; validasi dan rilis tidak termasuk. Data kebijakan analis wajib terpasang dalam rilis yang sama. Menutup `LAB-CLOSE-013` dan `LAB-CONFLICT-012` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-97. Bukti pertentangannya: capability map revision 5 `CAP-P14-02` — `AccessPermissionService.cs:117-132` mencocokkan nama aksi, dan delapan endpoint berbagi `Update`. Menuntut amandemen `LAB-PERM-v1` dan **task perbaikan** atas lima endpoint `S4a`/`S4b`. `AC-221`..`AC-224` |
| `LAB-CONFLICT-012` | Conflict | **Satu kode aksi `LabExamination : Update` membuka seluruh tindakan atas pemeriksaan**, sehingga dokter pemesan — yang memegangnya untuk menandai cito — secara teknis dapat mengisi hasil laboratorium. Ditemukan capability map revision 5 | Yoga Aji Pratama | `closed` | Ditutup 2026-09-24 oleh `LAB-DEC-146` | `01-existing-capability-map.md` revision 5 bagian C |
| `LAB-DEC-145` | Decision | *Butir 4 terjawab 2026-09-25 oleh `LAB-DEC-161` — lokasi dan metode pengambilan diputuskan bentuk, pengisi, dan kewajibannya.* **Paket empat butir kecil PRD.** (1) Email pasien **dihapus** dari PRD. (2) Penanda rendah/tinggi/kritis: **makna dikunci**, penanda wajib huruf atau teks `L`/`H`/`KRITIS`, warna mengikuti token desain — `LAB-FE-015`. (3) Larangan modal **diterima** untuk isian hasil utama halaman hasil PK dan Mikrobiologi — `LAB-FE-016`. (4) Lokasi dan metode pengambilan specimen **tetap `S2b`**. Menutup `PRD1-NEW-03`..`PRD1-NEW-06` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Diajukan sebagai satu paket karena berisiko rendah dan saling lepas; setiap butir tetap berpilihan dan berekomendasi sendiri. `LAB-FE-015` dan `LAB-FE-016` **mempersempit** `LAB-FE-002`. `AC-219`, `AC-220` |
| `LAB-DEC-144` | Decision | **Empat penolakan Mikrobiologi putaran 9 DITEGASKAN ULANG:** hasil per pemeriksaan (`LAB-DEC-095`), `Lainnya` ke daftar pantau (`LAB-DEC-098`), nama analis diturunkan (`LAB-DEC-105`), dan subbakteri tidak dibangun (`LAB-DEC-102`). PRD mengajukannya kembali tanpa alasan atau bukti baru. Menutup `PRD1-CONF-07`..`PRD1-CONF-10` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Dicatat sebagai keputusan karena ini **kali kedua** usulan yang sama datang lewat dokumen berbeda. Nol perubahan source — keempatnya sudah dibangun sesuai keputusan asal pada `S4b`. Koreksi PRD 36..39 |
| `LAB-DEC-143` | Decision | **Penunjukan validasi dan rilis dicatat PER DISIPLIN.** Setiap penunjukan menyebut orang, jenis kewenangan (validasi atau rilis), dan disiplin; penunjukan pada satu disiplin tidak berlaku pada disiplin lain. Menutup `PRD1-FOLLOW-03` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-96. Selaras dengan kewenangan klinis per disiplin (`LAB-DEC-078`) dan slice per disiplin (`LAB-DEC-083`). **Bentuk ini tetap dapat mewakili kebijakan satu orang untuk semua disiplin**, sehingga jawaban `DEC-LAB-011` apa pun tidak menuntut data dipecah ulang. `AC-218` |
| `LAB-DEC-142` | Decision | **Kewenangan validasi dan rilis BERLAPIS DUA: lapis JABATAN menentukan siapa yang boleh menjadi calon, lapis ORANG — daftar penunjukan — menentukan siapa yang benar-benar berwenang. Keduanya wajib lolos.** Penunjukan validasi dan penunjukan rilis terpisah (`LAB-INH-007`); lapis orang ditegakkan di dalam service. "Dokter Lab" pada PRD bukan jaminan kewenangan. Menutup `PRD1-CONF-03` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-95. Menegakkan `LAB-DEC-022` di atas model hak akses per departemen dan jabatan yang dipakai aplikasi, yang bekerja per aksi dan bukan per orang. **Pengisi daftar penunjukan tetap `DEC-LAB-011`** — keputusan ini tidak menjawabnya. Membuka `PRD1-FOLLOW-03`. `AC-215`..`AC-217`. **2026-09-24 — butir 1 diamandemen `LAB-DEC-150`:** jabatan calon dibatasi pada dokter berkewenangan laboratorium |
| `LAB-DEC-141` | Decision | **Konsultasi pada hasil Patologi Klinik dicatat sebagai FAKTA OPSIONAL — siapa, kepada siapa, kapan — memakai pola Mikrobiologi `LAB-DEC-106`.** Bukan syarat Final, bukan status, bukan izin; Patologi Klinik tidak memperoleh kualifikasi `Definitif`. *Kepada siapa* berupa nama tertulis sehingga konsultan dari luar rumah sakit tetap dapat dicatat. Menutup `PRD1-NEW-02` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-94. **Nol kolom baru** — ketiga kolom konsultasi sudah ada pada `LabExamination`. Konsultasi wajib per pemeriksaan tidak dibangun; bila kelak dibutuhkan, isi daftarnya milik `DR-LAB-001`. `AC-214` |
| `LAB-DEC-140` | Decision | **Kode QR pada dokumen final MEMERIKSA KEASLIAN DAN STATUS DOKUMEN, bukan membuka hasil.** Halaman verifikasi publik hanya menampilkan nama rumah sakit, nomor cetak, tanggal rilis, dan status — *asli dan berlaku*, *sudah digantikan* beserta versi penggantinya, atau *tidak dikenal*. Nol nilai hasil, nama pemeriksaan, nama pasien, No. RM, maupun NIK. QR berisi token acak per versi dokumen yang tidak dapat diturunkan dari nomor tercetak mana pun. Menutup `PRD1-NEW-01` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-93. **Batasnya dinyatakan:** QR membuktikan dokumen pernah terbit dan statusnya, **bukan** keutuhan angka di kertas — itu menuntut tanda tangan digital PDF (`LAB-COORD-011`). Halaman publik tanpa login menunggu `LAB-COORD-015`, dan hanya menahan implementasi halaman itu. `QRCoder` sudah ada di backend. `AC-211`..`AC-213` |
| `LAB-DEC-139` | Decision | **SATU GERBANG untuk seluruh penyerahan dokumen final kepada pasien — kirim WhatsApp, cetak untuk pasien, dan unduh.** Gerbang terbuka hanya bila order berlabel Selesai **dan** persetujuan Profesor dan Dokter Lab `LAB-DEC-067` sudah ada; ditegakkan backend pada ketiga jalur; layar menyebut syarat yang belum terpenuhi. Nota Lab, Label Lab, Label Goldar, dan lembar hasil dokter tidak terpengaruh. Berlaku Patologi Klinik dan Mikrobiologi. Menutup `PRD1-CONF-13` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-92. **Memperluas cakupan** `LAB-DEC-067`, tidak mengubah syaratnya. Bentuk persetujuan tetap `LAB-OPEN-029` (diajukan lewat `LAB-DEC-121`) — **bukan wewenang pemilik modul**. Nol penundaan tambahan hari ini karena rilis masih tertahan `DEC-LAB-011`. `AC-208`..`AC-210` |
| `LAB-DEC-138` | Decision | **Hasil yang sudah DIVALIDASI tetapi BELUM DIRILIS dapat dikembalikan kepada analis** oleh pemegang kewenangan validasi atau rilis, dengan alasan wajib dari **daftar terkendali yang sama dengan `LAB-DEC-082`**. Validasi dibatalkan dan hasil kembali Draft; fakta pernah divalidasi tetap tercatat di riwayat. Validasi ulang tunduk `LAB-DEC-003`. **Tanpa** versi bernomor dan **tanpa** pemberitahuan dokter; hasil yang sudah dirilis tetap hanya lewat koreksi. Menutup `PRD1-FOLLOW-01`. Bersamaan, `PRD1-CONF-12` ditutup ke arah blueprint karena tidak dibuka ulang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-91. Mengisi celah antara Reopen `LAB-DEC-135` dan koreksi `LAB-DEC-007`. Nol perubahan source hari ini — `S4` belum dibangun. `AC-205`..`AC-207` |
| `LAB-DEC-137` | Decision | **Pesan WhatsApp hasil kritis kepada dokter TIDAK MEMUAT DATA KLINIS.** Isinya hanya: ada hasil kritis, unit/ruang perawatan, waktu, dan nomor kontak Laboratorium. Nama pasien, No. RM, NIK, nama pemeriksaan, dan nilai hasil dilarang, dan larangan itu ditegakkan pada pembentuk pesan — nol ruas pengganti untuk data pasien. Menutup `PRD1-FOLLOW-02` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-90. Jalur kritis karena itu **tidak menunggu izin privasi** siapa pun; yang tetap ditunggu hanya gerbang WhatsApp (`LAB-COORD-011`). Menambah pertimbangan pada `PRD1-CLIN-01`. Dapat diamandemen bila pemilik platform kelak memberi izin privasi. `AC-203`, `AC-204` |
| `LAB-DEC-136` | Decision | **WhatsApp kepada Dokter Konfirmator adalah SALURAN PENGANTAR hasil kritis, BUKAN bukti pelaporan.** Waktu kirim WhatsApp dan waktu dilaporkan disimpan terpisah; pelaporan tuntas hanya bila kelima isian `LAB-DEC-004` — termasuk bukti pembacaan ulang — tercatat; alur kritis tidak bergantung pada gerbang WhatsApp. Berlaku Patologi Klinik dan Mikrobiologi. Menutup `PRD1-CONF-11` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-89. Menegakkan `LAB-DEC-004` yang diteken `DR-LAB-001`/`DR-LAB-002`; memakai `MstDoctor.WhatsAppNumber` yang sudah dibaca `LAB-DEC-111`. Sah tidaknya balasan WhatsApp sebagai bukti baca ulang **bukan wewenang pemilik modul** → `PRD1-CLIN-01`. Membuka `PRD1-FOLLOW-02` (isi pesan). `AC-200`..`AC-202` |
| `LAB-DEC-135` | Decision | **Patologi Klinik memakai DRAFT dan FINAL per pemeriksaan, dengan arti yang sama seperti Mikrobiologi dan Patologi Anatomi: Final = penulis selesai menulis, BUKAN rilis.** Hanya hasil Final yang masuk antrean validasi; Reopen boleh selama belum divalidasi; hasil terkunci sesudah dirilis. Kelima status PRD menjadi **label tampilan turunan** tanpa status tersimpan baru; Definitif tidak berlaku bagi Patologi Klinik. Menutup `PRD1-CONF-04`, `PRD1-CONF-05`, `PRD1-CONF-06` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-88. Menyalin `LAB-DEC-088`/`LAB-DEC-097`; menegakkan `LAB-DEC-080` dan `LAB-DEC-008`. **Perluasan kecil `S4a`**: `FinalizedAt`/`FinalizedByUserId` sudah ada pada `LabExamination`; tindakan Final/Reopen PK dan penyaring antrean validasi belum ada. Membuka `PRD1-FOLLOW-01`. `AC-196`..`AC-199` |
| `LAB-DEC-134` | Decision | **Hasil Patologi Klinik tetap DIKETIK ANALIS; "Dokter Lab" pada PRD dibaca sebagai PEMVALIDASI/PENGOTORISASI, bukan pengisi.** Menegakkan `LAB-DEC-005`. "Petugas Lab" pada PRD dibaca sebagai petugas administrasi (`LAB-DEC-068`), bukan analis, sehingga larangan mengubah hasil PK tetap berlaku. Empat mata `LAB-DEC-003` tidak disentuh. Menutup `PRD1-CONF-01` dan `PRD1-CONF-02` ke arah blueprint | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Lihat BR-87. **Nol perubahan source** — pengisi sudah tercatat dari pengguna yang menyimpan. Menetapkan **peran pada alur**, bukan pemegangnya: kewenangan tetap per orang (`LAB-DEC-022`, `DEC-LAB-011`), dan apakah setiap Dokter Lab otomatis memegangnya tetap Q9 (`PRD1-CONF-03`). Enam koreksi PRD dicatat pada bagian *Koreksi yang wajib masuk revisi PRD* |
| `LAB-DEC-133` | Decision | **PRD `LAB-EVD-008` adalah BUKTI UNTUK DIREKONSILIASI BUTIR PER BUTIR, bukan baseline baru.** Keputusan yang sudah `approved` tetap berlaku kecuali pemilik modul membukanya ulang secara eksplisit per butir, dan PRD direvisi mengikuti hasil rekonsiliasi. Batas scope amendment pass putaran 14 dikonfirmasi bersamaan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-24 | Menjadikan PRD baseline akan menandai `superseded` belasan keputusan sekaligus, termasuk `LAB-DEC-003` yang **hanya dapat diubah `DR-LAB-001`/`DR-LAB-002`** (`LAB-DEC-079`), serta membongkar `S4a` dan `S4b` yang sudah berdiri. Pola yang sama dipakai putaran 8 dan 9 |
| `LAB-DEC-132` | Decision | **`DetailTypeCode` tetap MILIK LABORATORIUM; kode SNOMED CT disimpan pada KOLOM TERSENDIRI yang boleh kosong.** Melengkapi `LAB-DEC-098` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-86. Nilai yang kelak ditambahkan lewat jalan keluar `Lainnya` **nol punya kode SNOMED**; memaksakannya sebagai kode utama berarti baris lokal diberi **kode karangan di dalam kolom yang dianggap standar internasional** — kesalahan yang sangat sulit ditemukan kemudian. SNOMED tetap disimpan sebab datanya ada hari ini, dan pemetaan 1.767 baris tidak perlu diulang dari nol ketika rumah sakit bertukar data dengan sistem luar |
| `LAB-DEC-131` | Decision | **Setiap baris punya nama Indonesia DAN nama Inggris; nama Indonesia BOLEH KOSONG dan layar menampilkan Inggris selama belum terisi.** Pencarian menerima keduanya; pemeriksaan duplikasi membandingkan keduanya (`RULE-007`). Harus ada cara melihat mana yang belum diterjemahkan. Melaksanakan `LAB-DEC-008` bertahap | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-85. **Seluruh 1.767 nilai `display` berbahasa Inggris SNOMED CT**, diverifikasi pada `LAB-EVD-007`. Menerjemahkan lebih dulu menahan `BE-LAB-55` dan `FE-LAB-32` berminggu-minggu; dan yang lebih memberatkan, **terjemahan anatomi yang keliru lebih berbahaya daripada istilah Inggris yang dibiarkan** — petugas yang ragu pada istilah asing akan bertanya, sedangkan terjemahan salah tampak meyakinkan |
| `LAB-DEC-130` | Decision | **Seluruh 1.767 baris diimpor: 1.601 berkonfidensi `Tinggi`/`Sedang` AKTIF, dan 166 berkonfidensi `Rendah` NONAKTIF.** Kepala instalasi **mengaktifkan** yang dibutuhkan, bukan menambah dari nol. Menjalankan maksud `LAB-DEC-099` dengan dasar yang kini terlihat | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-84. Ke-166 baris `Rendah` seluruhnya menyebut **lokasi tanpa menyebut bahan** — 158 dari `Spesimen Anatomi - Material Tidak Disebutkan`, 8 dari `Lainnya`. Dibuang berarti kehilangan kode SNOMED-nya. Menunggu sesi penyaringan lebih dulu berarti `BE-LAB-55` membangun tabel yang **nol dapat diuji dengan data nyata** — pola yang sudah dua kali menahan modul ini lewat `LAB-COORD-006` dan `MST-POS-WRITE` |
| `LAB-DEC-129` | Decision | **Specimen TETAP DUA TINGKAT, tetapi dari kolom yang berbeda. `LabSpecimenType` diperluas dari 7 menjadi 31 nilai `jenis_specimen`; `LabSpecimenDetailType` tetap dari `display` (1.767). `subjenis_specimen` (85) turun menjadi ATRIBUT pada baris detail, bukan tingkat pilihan.** Mengoreksi `LAB-DEC-098` pada pilihan kolomnya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-83. **`LAB-EVD-007` membuktikan datanya bertingkat TIGA**, dan tingkat yang dipilih artifact ternyata yang paling sedikit gunanya: **21 dari 31 kelompok hanya punya satu subjenis**, sehingga pada dua pertiga data ia layar yang nol punya alternatif. Perluasan 7 → 31 **aman** sebab ketujuh nilai ter-seed seluruhnya punya padanan — penambahan 24 baris beserta penggantian nama, bukan pembongkaran. `subjenis` tetap disimpan sebab pelaporan *"berapa banyak bahan dari kelompok cairan pleura"* hanya dapat dijawab darinya |
| `LAB-DEC-128` | Decision | **Zona bernilai `0` adalah PENGUKURAN SAH yang berarti nol zona hambat; ruas kosong berarti belum diukur.** Keduanya disimpan berbeda. Validasi **tidak boleh menolak** `0` dan **tidak boleh mewajibkan** zona terisi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-82. Bukti `LAB-EVD-006`: **sebelas dari 22 baris bernilai `0` dan seluruhnya berhasil `R`** — ia temuan terkuat bahwa kuman kebal, bukan ketiadaan data. Mewajibkan zona terisi ditolak sebab metode dilusi nol menghasilkan zona sama sekali, sehingga hasil jamur `LAB-EVD-005` menjadi mustahil disimpan |
| `LAB-DEC-127` | Decision | **Kalimat `LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI.` adalah TEKS BAKU** yang tercetak otomatis pada setiap hasil ber-set-bakteri, disimpan sebagai pengaturan dan dapat diubah kepala instalasi. **Catatan bebas analis tetap ada, terpisah, di bawahnya** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-81. Ia penjelasan klinis yang berlaku bagi semua hasil difusi cakram. Diketik ulang setiap kali, suatu hari ia **terlupa pada hasil yang justru membutuhkannya** — dan salah ketik pada kalimat klinis nol akan tertangkap siapa pun |
| `LAB-DEC-126` | Decision | **Isolat BOLEH berdiri tanpa satu pun baris kepekaan**, dan memperoleh **penanda apakah kepekaannya diuji**. Kuman yang ditemukan tetapi tidak diuji **tetap tersimpan sebagai isolat**, bukan hanya sebagai kalimat pada Catatan. Cetakan nol berubah: hanya isolat berantibiogram yang mendapat tabel `IDENTITAS` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-80. Bukti `LAB-EVD-006`: Catatan menyebut *Streptococcus Alfa Haemolyticus* **dan** *Branhamella catarrhalis*, tetapi hanya Branhamella yang bertabel. Bentuk 1 : 0..n **sudah diizinkan ERD sejak awal**. Ditulis sebagai kalimat, pertanyaan "berapa kali kuman ini tumbuh tahun ini" hanya dapat dijawab dengan membaca ribuan catatan satu per satu |
| `LAB-DEC-125` | Decision | **Penanda set bakteri melekat pada DATA INDUK PEMETAAN KATALOG milik Laboratorium**, sejajar pola `LAB-DEC-087`, dikelola kepala instalasi. Pemeriksaan tanpa set bakteri **tidak menampilkan** bagian isolat dan antibiogram sama sekali | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-79. Pemilik modul menyatakan **tidak semua pemeriksaan Mikrobiologi memakai set bakteri**; yang `MO KUL` memakainya. Menurunkannya dari awalan nama **ditolak** sebab `LAB-DEC-087` sudah menolak hal yang persis sama. Kolom pada `MstProcedure` ditolak sebab tabel itu milik `master-data`, dan modul ini sudah dua kali tertahan pola itu lewat `LAB-COORD-006` dan `MST-POS-WRITE` |
| `LAB-DEC-124` | Decision | **DUA penanda, bukan satu. Memperluas `LAB-DEC-116`.** Penanda **jenis biakan** menentukan kata pada label — bakteri `IDENTITAS`, jamur `HASIL BIAKAN JAMUR`. Penanda **metode uji** menentukan **bentuk tabel dan kolom mana yang berlaku** — difusi cakram memakai `UG`/rentang/zona, dilusi memakai MIC beserta satuannya. Keduanya **bebas satu sama lain** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-78. **Mengoreksi `LAB-DEC-116` yang berumur kurang dari satu jam:** putaran 11 menduga perbedaannya hanya label, dan `LAB-EVD-006` membuktikan bentuknya berbeda total. Dugaan itu diambil dari satu contoh cetak; contoh kedua membatalkannya. Satu penanda saja ditolak sebab itu mengunci bakteri selalu difusi dan jamur selalu dilusi, dan **bukti nol menyatakan itu** |
| `LAB-DEC-123` | Decision | **Interpretasi `S`/`I`/`R` DIHITUNG sistem dari zona terhadap breakpoint, dan boleh ditimpa analis dengan alasan tercatat.** Bila breakpoint untuk kombinasi itu belum terisi, ruasnya kembali manual. **Mengubah `r26`** yang mewajibkan analis mengetiknya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-77. **Seluruh 22 baris `LAB-EVD-006` konsisten:** zona di bawah batas bawah → `R`, di dalam rentang → `I`, di atas batas atas → `S`. Penimpaan tetap dibuka sebab **resistensi intrinsik** menuntut penilaian di luar rumus; menutupnya berarti melaporkan `Sensitive` pada kuman yang pasti tidak merespons. Peringatan-saja ditolak: peringatan yang muncul 22 kali pada satu layar berhenti dibaca |
| `LAB-DEC-122` | Decision | **`UG` dan rentang `R-S` berasal dari DATA INDUK lalu di-snapshot ke baris hasil.** `UG` menjadi ruas baru pada `LabAntibiotic` — kandungan cakram. Rentang `R-S` menjadi data induk breakpoint **per kombinasi organisme dan antibiotik**. **Menambal kekosongan `r26`** yang nol mengenal keduanya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-76. Breakpoint **per organisme** karena CLSI menetapkannya begitu dan `LAB-DEC-039` sudah mewajibkannya configurable. Di-snapshot dengan alasan yang sama seperti `OrganismNameSnapshot`: breakpoint berubah saat versi CLSI berganti, dan hasil tahun lalu tidak boleh berubah arti. **`LAB-DEC-115` tetap berlaku dan `UG` bukan penggantinya** — `UG` kandungan cakram pada difusi, MIC nilai hasil pada dilusi |
| `LAB-DEC-121` | Decision | **Temuan Profesor DIAJUKAN kepada wewenang klinis, BUKAN diputuskan sendiri.** Disusun permintaan baru berlampir `LAB-EVD-005` yang menanyakan apakah Konsultan Mikrobiologi Klinik adalah Profesor yang dimaksud `LAB-DEC-067`, dan bagaimana peran itu masuk `LAB-PERM-v1`. **`LAB-OPEN-029` TIDAK ditutup** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-75. `LAB-OPEN-029` mencatat tegas ini **bukan wewenang pemilik modul**; menutupnya di sini berarti menetapkan siapa berhak menyetujui pengiriman hasil kepada pasien tanpa satu pun pihak klinis menyatakannya. Yang berubah hari ini bukan jawabannya, melainkan bahwa pertanyaannya **kini punya kandidat** — dan kandidat itu **bukan** `DR-LAB-002` |
| `LAB-DEC-120` | Decision | **`Petugas Otorisasi` adalah pihak yang MERILIS; `Validasi oleh` adalah pemvalidasi.** Keduanya tidak boleh orang yang sama kecuali pengecualian `LAB-DEC-003` tercatat. Ruasnya dibangun sekarang dan **tampil kosong** selama rilis belum ada | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-74. **Cetakan Patologi Klinik mencetak DUA baris terpisah** — `Otorisasi oleh :` dan `Validasi oleh :` — sehingga laboratorium ini **sudah** membedakan keduanya hari ini. Bukti lapangan langsung bagi empat mata `LAB-DEC-003`, dan penguat `LAB-DEC-097`. **AKIBAT YANG WAJIB DIBACA: cetakan Mikrobiologi belum dapat lengkap sampai `S4d` dibuka `DEC-LAB-011`** — dan mengisinya dari pencetak atau penulis hasil ditolak, sebab dokumen akan menyebut pihak yang salah sebagai pengesah |
| `LAB-DEC-119` | Decision | **Nama konsultan adalah DATA INDUK PENGATURAN per disiplin**, diubah kepala instalasi. Ia **bukan** diturunkan dari pemegang wewenang klinis. Labelnya pun berbeda per disiplin: `Konsultan Mikrobiologi Klinik`, `Spesialis Patologi Anatomi`, dan `Konsultan` pada Patologi Klinik | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-73. Bukti: footer mencetak `Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.`, sedangkan `DR-LAB-002` adalah `dr. Nabila Rahmawati, Sp.MK`. **Dua peran berbeda yang kebetulan sama-sama Mikrobiologi Klinik**; menurunkan satu dari yang lain akan mencetak nama yang salah |
| `LAB-DEC-118` | Decision | **Tanggal pada cetakan punya pemetaannya SENDIRI.** `Tanggal Terima` dari waktu penerimaan fisik specimen, `Tanggal Selesai` dari `FinalizedAt`, `Tgl. Cetak` dinamis. Ruas `Waktu Efektif`/`Waktu Issued` pada layar **tetap** seperti `LAB-DEC-096` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-72. **Keduanya menjawab pertanyaan berbeda:** cetakan mencatat lama pengerjaan laboratorium (3 → 7 Agustus), sedangkan Waktu Efektif menjawab kapan bahan meninggalkan tubuh pasien — yang bermakna klinis dan yang diminta ruas HL7. Menyamakannya membuat bahan yang diambil Senin dan diterima Rabu tercatat efektif hari Rabu. Nol kolom baru |
| `LAB-DEC-117` | Decision | **Nomor cetak berdiri TERPISAH, dialokasikan per disiplin per tahun.** `OrderNumber` **tetap** menjadi identitas internal dan sumber barcode Label Lab; `LAB-DEC-072` utuh | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-71. Bukti: Mikro `26-1129`, PA `26.0919`, PK `25039254`, sedangkan `OrderNumber` yang **sudah dibangun** berformat `LAB-RSMMC-000000123`. Mengubah formatnya berarti membongkar layanan alokasi, index unik, dan sumber barcode yang sudah berjalan — serta meninggalkan pesanan lama berformat berbeda selamanya |
| `LAB-DEC-116` | Decision | **Mikrobiologi MENCAKUP biakan jamur.** `LabOrganism` dan `LabAntibiotic` tetap satu tabel masing-masing; yang ditambahkan **penanda jenis biakan** pada pemeriksaan, dan penanda itu memilih label cetak `BIAKAN JAMUR`/`RESISTENSI ANTIJAMUR` atau `BIAKAN BAKTERI`/`RESISTENSI ANTIBIOTIKA`. Dokumentasi `LabAntibiotic` **wajib diperbaiki** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-70. **Bukti pertama yang menunjukkannya:** contoh cetak ternyata `HASIL BIAKAN JAMUR : Candida albicans` dengan Nystatin dan Amphotericin, sedangkan **jamur nol kemunculan di seluruh backend** pada `981e002c`. Dua model terpisah ditolak karena strukturnya identik — perbedaannya hanya label. Tanpa penanda ditolak karena cetakan akan menyebut Nystatin sebagai antibiotik di hadapan dokter |
| `LAB-DEC-115` | Decision | **Nilai MIC selalu membawa satuannya** — tersimpan sebagai angka + satuan, daftar awal `ug/mL` dan `mg/L`, dapat diperluas. **Memperbaiki cacat `LAB-API-v1` `r26`** yang menyimpan `Concentration` tanpa satuan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-69. Bukti: cetakan menulis `Nystatin 1,25 ug/mL`. Alasannya **sama persis** dengan `LAB-DEC-041` pada volume: angka `1,25` sendirian nol dapat dibaca ulang siapa pun, dan pola resistensi tidak dapat dihitung dari nilai bersatuan berbeda yang tidak tercatat. Mengunci ke `ug/mL` ditolak sebab `mg/L` juga lazim, dan nilai `mg/L` kelak tersimpan salah **tanpa satu pun galat terlihat** |
| `LAB-DEC-114` | Decision | **`Definitif` adalah NILAI KUALIFIKASI HASIL yang dicetak** — daftar awal `Definitif` dan `Sementara`, dapat diperluas — **dan ketiga kolom fakta konsultasi `LAB-DEC-106` TETAP ADA** sebagai dasarnya. Memperluas `LAB-DEC-106`, tidak mencabutnya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-68. Bukti: cetakan menulis `HASIL YANG DIPEROLEH : DEFINITIF` sebagai baris tersendiri yang menonjol. Menurunkannya dari ada tidaknya konsultasi ditolak: itu menyimpulkan *dikonsultasikan* sama dengan *definitif*, dan **bukti nol menyatakan itu** — pada biakan yang dibaca bertahap berhari-hari, hasil `Sementara` yang sudah dikonsultasikan sangat mungkin ada. Ini **memperbaiki keputusan berumur satu hari**, dan sebabnya bukan kecerobohan melainkan artifact yang tidak memuat bentuk cetak |
| `LAB-DEC-113` | Decision | **Status temuan Mikrobiologi memakai DAFTAR NILAI TERSENDIRI** — `Normal`, `Positif`, `Negatif` — pada tingkat isolat. `LabPathologyFindingStatus` (`Normal`/`NeedsAttention`/`Critical`) **tidak disentuh** dan tetap milik Patologi Anatomi. Menutup `LAB-CLOSE-012` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-67. **Keduanya menjawab pertanyaan berbeda:** `Positif` berarti ada pertumbuhan kuman — itu temuan; `NeedsAttention` adalah penilaian kegawatan. Disatukan, PA mendapat `Positif` yang nol artinya bagi laporan jaringan, dan Mikrobiologi mendapat `NeedsAttention` yang **bertabrakan dengan penanda kritis `LAB-DEC-103`** — dua tempat menyatakan hal yang sama, nol aturan menentukan pemenangnya. Daftar gabungan lima nilai juga ditolak: penyaringan yang terlewat sekali membuat patolog dapat memilih `Positif` pada jaringan |
| `LAB-DEC-112` | Decision | **Jejak perubahan ruas berdiri sebagai TABEL TERSENDIRI milik Laboratorium** — baris apa, ruas apa, nilai lama, nilai baru, siapa, kapan. `LabTransitionHistory` **tidak disentuh** dan tetap mencatat perpindahan status saja. Melaksanakan `LAB-DEC-107`, menutup `LAB-CLOSE-011` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-66. Alasannya **sama persis** dengan `LAB-DEC-080` menolak menambahkan `Validated` ke `LabExaminationStatus`: **satu tabel, dua sumbu**. Ditumpangkan, ketiga kolom baru kosong pada seluruh baris status lama dan setiap laporan riwayat status harus menyaring baris yang bukan miliknya — selamanya |
| `LAB-DEC-111` | Decision | **Dokter bertugas dibaca dari `TrxOnCallAssignment` yang aktif pada jam itu, lewat rantai `WorkforceProfileId` → `MstDoctor.WorkforceProfileId` → nama + `MstDoctor.WhatsAppNumber`; bila nol baris, layar OTOMATIS jatuh ke daftar dokter aktif yang dapat dicari disertai keterangan.** Jalur jatuh itu **permanen**, bukan sementara. `MstDoctorSchedule` **tidak dipakai**. Pilihan DPJP nol berubah. **Menggantikan butir 2 `LAB-DEC-108`**; menutup `LAB-CLOSE-010` dan `LAB-CONFLICT-010` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-65. Seluruh rantainya **sudah berdiri pada satu `ApplicationDbContext`** — nol tabel baru, nol jembatan baru. **`TrxOnCallAssignment` nol punya controller**, sehingga pada hari pertama halaman ini hidup jadwal jaganya **pasti kosong**; tanpa jalur jatuh, kolom Dokter Konfirmator lahir tidak dapat dipakai dan `LAB-DEC-004` gagal sejak hari pertama. Pola `LAB-COORD-006`/`MST-POS-WRITE` — daftarnya ada, cara mengisinya tidak — **tidak diulang**: Laboratorium menyediakan jalan lain sambil tetap membaca sumber yang benar begitu terisi. Membuka `LAB-COORD-014` |
| `LAB-DEC-110` | Decision | **Template cetak Mikrobiologi dicatat dari URAIANNYA, bukan dari gambarnya.** Kop `LABORATORIUM MIKROBIOLOGI` beserta identitas rumah sakit dan akreditasi, pengulangan kop + informasi pasien tiap lembar, dan footer berisi anjuran menghubungi dokter peminta, area Catatan, Konsultan Mikrobiologi Klinik, Tgl. Cetak, serta Petugas Otorisasi. **Tata letak persisnya menunggu berkas screenshot-nya.** Membuka `LAB-OPEN-039` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-64. **Kedua screenshot yang disebut artifact tidak ikut diserahkan ke sesi ini** — yang dibaca hanya uraiannya. Menaikkan uraian menjadi tata letak final berarti mengarang bukti. Ukuran cetaknya tetap `LAB-OPEN-032` |
| `LAB-DEC-109` | Decision | **Ruas `HL7` TIDAK dibangun pada putaran ini.** Value set sementara `P`/`F`/`C`/`X` yang diusulkan artifact ditolak; ruasnya ditambahkan sesudah pemilik platform menyatakan profil HL7 yang dipakai | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-63. **`HL7` nol kemunculan di SELURUH backend**, diverifikasi ulang 2026-09-21 pada `981e002c`. Nilai sementara yang sudah dipakai berbulan-bulan tetap tersimpan sebagai data lama ketika nilai aslinya datang, dan pemetaan ulangnya melebihi biaya menunggu. Tetap `LAB-COORD-012`, milik pemilik platform |
| `LAB-DEC-108` | Decision | **Dokter Konfirmator punya DUA CARA MEMILIH, bukan dua jabatan.** DPJP diambil dari pesanan; alternatifnya dibaca dari `MstDoctorSchedule` yang sudah berdiri. **`Dokter Lantai` adalah label layar, bukan peran** — nol tambahan pada matriks kewenangan, nol tabel baru. Menutup `LAB-OPEN-030` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-62. Mendirikannya sebagai peran menambah baris ke `LAB-PERM-v1` yang masih menunggu `LAB-P0-001`, dan menuntut proses baru menetapkan siapa dokter lantai hari ini — **nol bukti lapangan menjelaskan penetapnya**. **Kecocokan `MstDoctorSchedule` belum diaudit**; yang dipastikan sesi ini baru keberadaan tabelnya |
| `LAB-DEC-107` | Decision | **Informasi Specimen DAPAT DISUNTING pada halaman hasil sampai `Simpan Final`, dan seluruh perubahannya berjejak.** Termasuk jenis dan Spesifik Specimen. Sesudah Final baca-saja. Tetap bukan area wajib. Menerima `CAP-024` artifact | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-61. Menutupnya mengulang jalan buntu yang persis dihindari `LAB-DEC-040`: satu salah pilih saat penerimaan menahan pengisian hasil sampai petugas penerimaan tersedia, dan pada shift malam ia mungkin tidak ada. **Jejak perubahan specimen belum ada hari ini** — itu pekerjaan baru |
| `LAB-DEC-106` | Decision | **`Definitif` adalah FAKTA konsultasi — siapa, kepada siapa, kapan — BUKAN status dan BUKAN izin.** Hasil tetap dapat disunting sesudahnya sampai Final. **`CAP-023` artifact TIDAK diadopsi**: `Definitif` nol membuka pembagian hasil ke dokter lain sebelum rilis. Menutup `LAB-OPEN-017` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-60. Mengikuti `LAB-DEC-080` — mencatat apa yang terjadi, bukan menjanjikan apa berikutnya. **Menyamakannya dengan "persetujuan Profesor" `LAB-DEC-067` sengaja TIDAK dilakukan**: itu persis `LAB-OPEN-029`, dan penetapan wewenang klinis 2026-09-17 nol memuat nama bergelar Profesor |
| `LAB-DEC-105` | Decision | **Ruas `Analis` DITURUNKAN dari pengguna yang menekan simpan, baca-saja.** Memakai `ResultEnteredByUserId` yang sudah dicatat. `Penanggung Jawab Analis` tetap ruas terpisah yang dipilih (`LAB-DEC-093`). Menolak artifact yang menjadikannya pilihan wajib | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-59. Bila dapat dipilih, petugas A dapat menyimpan hasil atas nama analis B — dan **empat mata `LAB-DEC-003` ditegakkan justru dengan membandingkan pengisi terhadap pemvalidasi**. Alasan sama dengan `LAB-DEC-092`: satu kejadian, satu jawaban |
| `LAB-DEC-104` | Decision | **Kewajiban ruas bergantung ISI, bukan daftar nama kolom.** Wajib: hasil pemeriksaan terisi; organisme bila ada isolat; antibiotik + `S`/`I`/`R` bila ada baris kepekaan. MIC, zona mm, dan keterangan boleh kosong. Menolak `RULE-011` artifact | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-58. **Kultur steril adalah hasil yang sah dan nol isolat** — mewajibkan organisme membuatnya mustahil disimpan, dan petugas akan memilih organisme sembarang supaya lolos. Mengikuti pola `VAL-95`; `VAL-88` dicabut minggu ini justru karena mengunci nama ruas yang dihardcode |
| `LAB-DEC-103` | Decision | **Aturan kritis Mikrobiologi berdiri sebagai DATA INDUK terkendali milik Laboratorium** — kombinasi organisme, antibiotik, dan interpretasi — dan dapat diubah tanpa mengubah kode. **Isinya diisi `DR-LAB-002`**, bukan pemilik modul dan bukan implementer. Selama kosong, penanda nol menyala dan layar wajib menyatakannya terbaca | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-57. **Bentuk dan isi adalah dua wewenang berbeda**; memisahkannya membuat `S4b` jalan sekarang. "Semua `R` kritis" ditolak: resistensi terhadap satu antibiotik cadangan bukan kegawatan, sedangkan MRSA yang masih peka justru wajib dikabarkan. Menjawab **bagian** `LAB-OPEN-014`; alur pelaporannya tetap `LAB-P0-004` dan `S5` |
| `LAB-DEC-102` | Decision | **`Subbakteri` TIDAK dibangun.** Spesies dan subtipe menjadi baris tersendiri pada `LabOrganism` yang sudah berdiri — `Escherichia coli` dan `Escherichia coli ESBL` masing-masing satu baris. Nol tabel baru, nol migration | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-56. Nama kuman sudah memuat genus dan spesies dalam satu nama, dan **`Escherichia` sendirian bukan temuan yang dapat dilaporkan**. **Artifact nol memberi satu pun contoh pasangan bakteri dan subbakteri.** `LAB-DEC-084` tidak diperluas |
| `LAB-DEC-101` | Decision | **Baris kepekaan antibiotik mempertahankan BR-23.** Wajib: antibiotik dari `LabAntibiotic` dan interpretasi `S`/`I`/`R`. Opsional dan **keduanya boleh kosong**: nilai MIC dan zona hambat dalam mm. Status `Normal`/`Positif`/`Negatif` tetap pada tingkat **isolat**. Menolak penyempitan artifact yang membuang zona mm | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-56. **Difusi cakram menghasilkan mm, dilusi menghasilkan MIC**, dan laboratorium memakai keduanya. Mewajibkan salah satunya memaksa analis mengisi angka karangan yang tersimpan selamanya sebagai kalau-kalau data yang benar — alasan yang sama dipakai `LAB-DEC-041` menolak memaksakan mililiter |
| `LAB-DEC-100` | Decision | **Volume specimen tetap ANGKA + SATUAN; daftar satuannya diperpanjang.** Tambahan: `swab`, `preparat`, `potong`, `item`, `isolat`, `vial`. **Ukuran berdimensi P × L × T TIDAK ditampung sebagai volume** — ia masuk `Keterangan Specimen`. Menolak format teks bebas bagian 5.5.4 artifact | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-55. Menegakkan `LAB-DEC-041`: **angka yang tersimpan sebagai kalimat tidak dapat dijumlahkan, dibandingkan, atau dilaporkan**. `2 swab` tersimpan persis sepola dengan `5 mL` — satu angka, satu satuan. Dimensi adalah tiga angka, bukan satu |
| `LAB-DEC-099` | Decision | **Isi awal data induk Spesifik Specimen BUKAN 1.767 entri mentah**, melainkan hasil penyaringan kepala instalasi atas nilai yang benar-benar dipakai Mikrobiologi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-54. **Dataset 1.767 entri nol kemunculan di seluruh blueprint sebelum 2026-09-21**, dan berkasnya tidak ikut diserahkan ke sesi ini. Daftar pilihan berisi 1.767 baris bukan bantuan bagi petugas melainkan hambatan. Penyaringannya dicatat `LAB-OPEN-040` |
| `LAB-DEC-098` | Decision | **Spesifik Specimen berdiri sebagai DATA INDUK BARU milik Laboratorium yang menunjuk induknya pada `LabSpecimenType`; satu specimen boleh menunjuk lebih dari satu.** `LabSpecimenType` **tetap satu tingkat dan tidak diubah**. Penambahan nilai baru tetap tunduk `LAB-DEC-040` — jalan keluar `Lainnya`, daftar pantau, dan hanya kepala instalasi yang menaikkannya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-54. **Pemeriksaan duplikat Indonesia/Inggris yang diusulkan artifact tidak cukup**: `cairan kista`, `Cairan Kista`, `c. kista`, dan `kista` lolos sebagai empat nilai. Alasannya sama persis dengan `LAB-DEC-040` tujuh hari lalu. `LabSpecimen.SpecimenTypeId` hari ini **tunggal** — relasi banyak ini pekerjaan baru |
| `LAB-DEC-097` | Decision | **`Simpan Final` pada hasil Mikrobiologi berarti PENULIS SELESAI MENULIS — ia BUKAN rilis.** Dicatat sebagai fakta: `FinalizedAt` + `FinalizedByUserId`; `Draft` berarti `FinalizedAt` kosong; `Reopen` mengosongkannya beserta jejaknya. **Hasil Final tetap BELUM boleh dikirim ke pasien** — pengiriman menunggu rilis, dan rilis Mikrobiologi adalah `S4d` yang tertahan `DEC-LAB-011` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-53. Menyalin pola `LAB-DEC-088`. **Tafsir artifact — Final = hasil pasien, hanya Dokter Lab — ditolak** karena ia membuat pengisi hasil sekaligus pengesahnya, melanggar empat mata `LAB-DEC-003` yang baru ditandatangani `DR-LAB-002` 2026-09-17. **`BP-007` artifact tidak diadopsi apa adanya**: baca-saja berlaku sesudah RILIS, bukan sesudah Final |
| `LAB-DEC-096` | Decision | **`Waktu Issued` dan `Waktu Efektif` Mikrobiologi DITURUNKAN, baca-saja.** Waktu Efektif dari `LabSpecimen.CollectedAt`, Waktu Issued dari `FinalizedAt`. Nol kolom baru, nol ketikan. Koreksi waktu pengambilan dilakukan pada data specimen. Menolak `RULE-021` dan `RULE-022` artifact | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-52. **Memperluas `LAB-DEC-092` dari Patologi Anatomi ke Mikrobiologi**, dengan alasan yang identik: isian manual berarti dua sumber kebenaran untuk satu kejadian, dan yang diketik **nol akan pernah ketahuan melenceng** |
| `LAB-DEC-095` | Decision | **Hasil Mikrobiologi TETAP melekat pada PEMERIKSAAN, bukan pada order.** Yang berjumlah satu per order adalah **dokumen final cetaknya** (`LAB-DEC-067`), bukan tempat hasilnya. Layar bertingkat dua: pilih baris pemeriksaan, baru isi hasilnya. Menolak `RULE-001` artifact | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-21 | Lihat BR-51. Menegakkan `LAB-DEC-085` yang berumur 3 hari. **Kultur Darah dan Kultur Urin dalam satu order adalah dua bahan dengan dua pola kuman**; dipaksa satu kolom, laporan "kuman apa yang paling sering tumbuh dari darah" berhenti dapat dijawab — dan justru itu dasar pemilihan antibiotik empiris |
| `LAB-DEC-094` | Decision | **`Status Hasil PA` — Normal / Perlu Perhatian / Kritis — dibangun pada `S4c` sebagai NILAI, dan alur pelaporan kritisnya TETAP `S5`.** Ruasnya tersimpan, tampil, dan tercetak. Yang **tidak** dibangun: tombol Konfirmasi DPJP/Dokter Lantai, pengiriman WhatsApp, dan daftar pantau keterlambatan. Menutup `LAB-OPEN-038` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Bentuknya nilai, sederajat `MicrobiologyFinding`**, sehingga ia **nol melanggar `LAB-DEC-080`** — sama seperti status temuan Mikrobiologi yang sudah diputuskan. **Ini menjaga `S4c` tetap dapat dikirim** tanpa menunggu `LAB-P0-004` dan `LAB-OPEN-014` yang keduanya wewenang klinis. **`LAB-OPEN-014` MENYEMPIT, tidak tertutup:** artifact membuktikan Patologi Anatomi **mengenal** temuan yang wajib diperhatikan segera, dan penandanya **manual** — tetapi **apa** yang tergolong kritis, berapa batas waktu tanggapnya, dan bagaimana eskalasinya tetap milik `DR-LAB-003` lewat `LAB-P0-004` |
| `LAB-DEC-093` | Decision | **`Penanggung Jawab Analis` adalah ruas BARU pada hasil Patologi Anatomi, bukan `ExaminerDoctorId` yang sudah ada.** Dipilih saat mengisi hasil, dari pemegang jabatan `Analis Laboratorium`. `ExaminerDoctorId` tetap berarti **Penanggung Jawab Lab** dan tetap dipilih saat konfirmasi pesanan sesuai `LAB-DEC-061`. Menutup `LAB-OPEN-037` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dua orang, dua pekerjaan.** Analis **menyiapkan jaringan** — memotong, memblok, mewarnai — dan itu pekerjaan yang berbeda dari **membaca slide**. Artifact menampilkan keduanya sebagai dua baris berdampingan pada informasi pemeriksaan, dan menyatukannya akan membuat satu baris selalu menduplikasi baris di atasnya. **Menurunkannya dari pelaku pengisian juga ditolak**, dan alasannya lahir dari `LAB-DEC-090` yang baru diambil pada sesi yang sama: pengisi hasil PA adalah **Dokter Lab**, sehingga ruas itu akan selalu berisi nama dokter dan **tidak pernah** nama analis |
| `LAB-DEC-092` | Decision | **`Waktu Issued` dan `Waktu Efektif` DITURUNKAN, bukan diisi manual.** Waktu Efektif diambil dari `LabSpecimen.CollectedAt` yang sudah tercatat sejak `S2`; Waktu Issued diambil dari `FinalizedAt` yang ditetapkan `LAB-DEC-088`. Layar menampilkannya **baca-saja**. Nol kolom baru. **Mengoreksi Klarifikasi Q8 artifact** yang menyebut keduanya diisi manual. Menutup `LAB-OPEN-036` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Keduanya ruas HL7 `DiagnosticReport`** — *effective* adalah waktu yang bermakna klinis, *issued* adalah waktu laporan diterbitkan — dan **sistem sudah mencatat keduanya**. Menerimanya sebagai isian manual berarti **dua sumber kebenaran untuk satu kejadian**: Waktu Efektif yang diketik berbeda dari `CollectedAt` nol akan terdeteksi siapa pun, dan pertanyaan *"kapan bahan ini sebenarnya diambil"* berhenti punya satu jawaban. Alasan yang sama dipakai `LAB-DEC-066` menolak counter telanjang |
| `LAB-DEC-091` | Decision | **Empat ruas konteks klinis Patologi Anatomi — Diagnosa Awal, Riwayat Penyakit Relevan, Masa Terakhir Haid, dan Keterangan Klinis — melekat pada PESANAN sebagai kelompok ruas khas PA, dan diisi DOKTER PEMESAN.** Bukan kolom pada `LabOrder` yang dipakai bersama tiga disiplin. Menutup `LAB-OPEN-035` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dokter pemesan yang mengisinya karena ia yang melihat pasiennya**; patolog membacanya untuk menafsirkan jaringan. Menyerahkan pengisiannya kepada patolog berarti memintanya mengetik ulang konteks yang **tidak ia amati** — riwayat penyakit dan masa terakhir haid ada di hadapan dokter pemesan, bukan di hadapan patolog. **Kelompok terpisah dipilih di atas kolom pada `LabOrder`** dengan alasan yang sama seperti `LAB-DEC-085`: `LabOrder` dipakai tiga disiplin, dan `Masa Terakhir Haid` bahkan hanya bermakna bagi sitologi ginekologi pada pasien perempuan. **`Diagnosa Awal` menjadi sumber auto-fill `Diagnosa Klinis` pada IHK**, sehingga tanpa keputusan ini auto-fill itu nol punya asal |
| `LAB-DEC-090` | Decision | **Pengisi hasil Patologi Anatomi adalah Dokter Lab, bukan analis. `LAB-DEC-068` tetap utuh.** Keduanya mengatur layar yang berbeda: `LAB-DEC-068` mengatur tombol pada **menu Hasil** — Nota Lab, Label, Kirim ke Pasien — dan itu tetap dipegang Petugas Lab dan/atau Admin; halaman **Detail Hasil PA** diisi Dokter Lab. Petugas Lab tetap boleh membuka dan mencetak. Menutup `REC4-CONF-005` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dasarnya sifat pekerjaannya:** makroskopik, mikroskopik, dan kesimpulan adalah **diagnosis**, bukan pencatatan. **MEMPERSEMPIT `LAB-DEC-005`** yang menyebut hasil diketik analis — benar untuk Patologi Klinik, tidak untuk Patologi Anatomi. **Satu hal ikut terbawa dan belum terjawab:** bila Dokter Lab yang mengisi, siapa yang kelak memvalidasi hasil PA? `LAB-DEC-003` melarang pengisi memvalidasi hasil yang sama. Itu perkara `S4e` dan tetap tertahan `DEC-LAB-011` |
| `LAB-DEC-089` | Decision | **`Diplo` pada artifact adalah `Duplo` yang sudah ada. Nol kolom baru.** Penanda `IsDuplo` pada `LabExamination` dipakai apa adanya; `LAB-DEC-026` tidak berubah. Menutup `REC4-CONF-007` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Ejaannya berbeda satu huruf, maknanya ternyata tidak.** Artifact `RULE-016` menyebut Diplo *"hanya menandai atribut; tidak memengaruhi alur atau output"*, dan `IsDuplo` yang sudah dibangun memang persis begitu. Dua penanda berejaan nyaris sama pada satu layar dihindari — petugas yang salah memilih di antara keduanya **nol akan tahu ia salah** |
| `LAB-DEC-088` | Decision | **`Simpan Final` pada halaman hasil PA berarti PATOLOG SELESAI MENULIS — ia BUKAN rilis.** Dicatat sebagai **fakta** sesuai `LAB-DEC-080`: `FinalizedAt` dan `FinalizedByUserId`. `Draft` berarti `FinalizedAt` masih kosong; `Reopen` mengosongkannya kembali beserta jejaknya. **Hasil yang sudah Final tetap BELUM boleh dikirim ke pasien** — pengiriman menunggu rilis, dan rilis Patologi Anatomi adalah `S4e` yang tertahan `DEC-LAB-011`. Menutup `REC4-CONF-001` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Mempertahankan `LAB-DEC-080` DAN `LAB-DEC-003` sekaligus, dan itu yang menentukan pilihannya.** Bila `Final` diartikan rilis, maka artifact memberi Dokter Lab hak mengisi **dan** merilis hasil yang sama — dan itu melanggar prinsip empat mata yang baru ditandatangani `DR-LAB-003` pada 2026-09-17. **`REC4-CONF-006` LARUT oleh keputusan ini:** `Reopen` sebelum rilis adalah penyuntingan biasa, bukan koreksi hasil yang sudah dirilis, sehingga ia **bukan** `S6` dan **tidak** menyentuh `DEC-LAB-014`. Nol keputusan berumur satu hari yang dibatalkan |
| `LAB-DEC-087` | Decision | **Kategori Patologi Anatomi ditentukan data induk pemetaan milik Laboratorium**, bukan dari teks nama pemeriksaan. Satu tabel pemetaan `procedure → kategori PA`, berprefix `Lab`. Keenam keyword artifact — `HISTO`, `PAPSMEAR`, `LBC`, `HPV`, `NON GINEKOLOGI`, `IHK` — tetap dipakai, **tetapi sebagai alat bantu pengisian awal SEKALI**, bukan logika runtime. Menutup `REC4-CONF-004` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dua alasan, dan keduanya berasal dari modul ini sendiri.** Pertama, kepemilikan Laboratorium dipilih di atas `master-data` atas alasan yang sama dengan `LAB-DEC-084`: `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan data induk di modul lain dapat nol punya endpoint tulis. Kedua, pencocokan keyword saat runtime membuat **penggantian nama pemeriksaan diam-diam mengubah bentuk hasil** — dan hasil lama ikut berubah artinya. `RULE-036` artifact tetap berlaku: pemeriksaan tanpa pemetaan **tidak diberi kategori**, dan sistem nol menebak |
| `LAB-DEC-086` | Decision | **Bentuk hasil Patologi Anatomi dimodelkan sebagai DATA INDUK PARAMETER beserta nilai per baris**, bukan kolom tetap. Satu data induk memuat ke-15 ruas dari ketiga bentuk — Makroskopik, Mikroskopik, Kesimpulan, Kondisi, Kategori, Anjuran, Diagnosa Klinis, Diagnosa PA, ER, PR, HER2, Ki-67, Status ER, Status PR, HER2 IHK — beserta kategori yang mewajibkan masing-masing. Menutup `REC4-CONF-002` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Alasan terkuatnya ada di dalam artifact sendiri:** ruas `Anjuran` muncul pada Sitologi Ginekologi **dan** pada IHK. `RULE-013` menuntut penggabungan tanpa duplikasi ketika satu order memuat beberapa kategori — dan itu **hanya mungkin bila parameter punya identitas**, bukan sekadar nama kolom. Bentuk hasil kelima kelak cukup menambah baris data induk, nol migration. Pola yang sama dengan `LabValueBound`/`LabValueOption` |
| `LAB-DEC-085` | Decision | **Tempat hasil melekat BERBEDA PER DISIPLIN.** Patologi Klinik dan Mikrobiologi tetap **per pemeriksaan** pada `LabExamination`; Patologi Anatomi menjadi **per order**. Menutup `REC4-CONF-003` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dasarnya bahan yang diperiksa, bukan kerapian model.** Hemoglobin dan Kalium adalah dua angka berbeda dari satu tabung — dua hasil. Makroskopik dan mikroskopik menguraikan **satu jaringan**: bila satu order PA memuat Histologi dan IHK pada blok parafin yang sama, mengisinya dua kali berarti **dua uraian atas jaringan yang sama, dan keduanya dapat melenceng**. Modul ini sudah menerima perbedaan per disiplin sebagai prinsip lewat `LAB-DEC-079` dan `LAB-DEC-083`. **`S4a` yang sudah berjalan sejak 2026-09-17 nol perlu dirombak** |
| `LAB-EVD-003` | Fact | Artifact **`Detail Hasil Patologi Anatomi`** dilampirkan pemilik modul pada 2026-09-18. Memuat `CAP-001`..`CAP-020`, `RULE-001`..`RULE-036`, dan `BP-001`..`BP-006` atas **satu halaman**: detail hasil Patologi Anatomi, beserta 14 klarifikasi pemilik bertanggal 2026-09-17. **Disimpan verbatim** pada [`evidence/2026-09-18-detail-hasil-patologi-anatomi.md`](evidence/2026-09-18-detail-hasil-patologi-anatomi.md) | Yoga Aji Pratama | `diterima sebagai bukti` | Bukti analis dan klarifikasi pemilik proses — tingkat 4 | **Verdict rekonsiliasi `NOT_RECONCILED`** (putaran 4, `05-evidence-reconciliation.md` bagian 12). **Nol butirnya diadopsi sebagai keputusan**, dan itu disengaja: 13 butir baru dicatat, 7 pertentangan dibuka, dan **tiga di antaranya membantah keputusan yang berumur kurang dari 48 jam**. `BE-LAB-46` dan `FE-LAB-25` **dibekukan** sebelum satu baris kode ditulis |
| `LAB-DEC-084` | Decision | **Organisme dan antibiotik menjadi DUA data induk terkendali milik Laboratorium**, berprefix `Lab` sesuai `LAB-OPEN-021`. Isolat menunjuk organisme yang dikenal; baris kepekaan menunjuk antibiotik yang dikenal. Pengetikan bebas tidak diterima pada kedua ruas itu. Menutup `DEC-LAB-015` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Dasarnya domain, bukan kerapian:** panel antibiotik yang diuji kepekaannya adalah keputusan **laboratorium** — kuman apa yang dilaporkan dan antibiotik apa yang dipanel — dan ia **bukan** formularium farmasi. **Kepemilikan Laboratorium dipilih di atas `master-data` atas dasar bukti dari modul ini sendiri:** dua penahan yang masih terbuka hari ini berbentuk persis itu — `LAB-COORD-006`, instansi perujuk **nol punya endpoint tulis sama sekali**; dan `MST-POS-WRITE`, `MstPosition` juga nol sampai barisnya harus disisipkan lewat SQL langsung. Menaruh dua daftar baru di sana berisiko mengulang keduanya: **daftarnya ada, cara mengisinya tidak.** **Yang dibeli keputusan ini:** antibiogram rumah sakit menjadi mungkin disusun. Teks bebas menulis `E. coli`, `E.coli`, `Escherichia coli`, dan `eschericia coli` sebagai empat hal padahal satu, dan pola resistensi tidak dapat dihitung dari itu — alasan yang **sama persis** dengan `LAB-DEC-082` pada alasan koreksi. **Membuka `S4b`**: arsitektur domain menaikkannya menjadi `DOMAIN_ARCHITECTURE_READY` pada revision 6 |
| `LAB-DEC-083` | Decision | **Validasi dan rilis Mikrobiologi serta Patologi Anatomi menjadi slice tersendiri, mencerminkan pemecahan `LAB-DEC-076`: `S4d` validasi dan rilis Mikrobiologi, `S4e` validasi dan rilis Patologi Anatomi.** Tiga disiplin karena itu punya tiga pasang sejajar — `S4a`+`S4`, `S4b`+`S4d`, `S4c`+`S4e` — konsisten dengan pola tiga daftar sejajar `LAB-DEC-025` dan `LAB-DEC-070`. Menutup `DEC-LAB-013` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Alasannya bukan kerapian, melainkan kemerdekaan rilis:** ketiga disiplin punya penahan sisa yang berbeda, sehingga masing-masing dapat maju tanpa menunggu dua lainnya. **Ini TIDAK mendahului `LAB-OPEN-034`** — tiga slice terpisah tetap boleh berbagi satu implementasi; yang dipisah adalah unit perencanaan, bukan kodenya. **Nama `S4d` dan `S4e` dipilih karena `S4b` dan `S4c` sudah menjadi milik pengisian**, mengikuti kehati-hatian penamaan yang sama yang membuat `LAB-DEC-076` menolak memakai ulang nama `S4b` |
| `LAB-DEC-082` | Decision | **Koreksi hasil memakai alasan dari daftar terkendali dan versi bernomor.** Setiap koreksi menghasilkan versi baru bernomor; versi lama tetap terlihat bertanda *"sudah diperbaiki"* sesuai `LAB-DEC-007` butir 3. Alasan koreksi diambil dari data induk terkendali, mengikuti pola `LAB-DEC-019` (master alasan penolakan sampel) | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Alasan daftar terkendali dipilih supaya koreksi dapat DIHITUNG** — berapa kali sampel tertukar, berapa kali salah ketik. Teks bebas menulis *"salah ketik"*, *"salah input"*, *"keliru"*, dan *"tertukar"* sebagai empat hal padahal satu, dan pola kesalahan yang berulang tidak akan pernah terlihat sebagai angka. **INI MENUTUP `LAB-P0-003` HANYA SEBAGIAN, dan itu koreksi atas penilaian gerbang `LAB-RCG-001-r6` sendiri**, yang menyatakan `LAB-P0-003` dapat ditutup pemilik modul sendirian. Wawancara membuktikan tidak: `LAB-REQ-004` bagian 4.5 memuat tiga pertanyaan yang **bersifat klinis** dan belum terjawab. Dibuka `DEC-LAB-014`. `S6` karena itu **tetap tertahan**, dengan penahan yang kini bernama tepat |
| `LAB-DEC-081` | Decision | **`superseded` 2026-09-21 oleh `LAB-DEC-106` + persetujuan pemilik modul pada sesi perancangan `S4b`.** Penanda `Definitif` **masuk kembali** ke `S4b` Rilis 1, sebab alasan pengeluarannya — maknanya belum diputuskan — sudah hilang. Bentuknya tiga kolom fakta pada `LabExamination`, nol tabel baru, nol status baru. Isi aslinya: **Penanda `Definitif` dikeluarkan dari `S4b` Rilis 1.** Pengisian hasil Mikrobiologi Rilis 1 memuat status Normal/Positif/Negatif, organisme per bakteri, antibiotik, kadar, zona dalam mm, dan hasil `R`/`I`/`S` — **tanpa** penanda `Definitif`. `LAB-OPEN-017` tetap terbuka tetapi **turun dari `BLOCKING` menjadi `LATER SLICE`** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Keputusan ini nol mengubah apa pun; ia hanya membaca yang sudah tertulis.** Blueprint sudah menempatkan penanda `Definitif` pada **Rilis 2** sejak semula, justru karena maknanya belum diputuskan. Gerbang `LAB-RCG-001-r6` menandainya `BLOCKING` bagi `S4b` atas dasar ia ruas di dalam struktur hasil Mikrobiologi — benar sebagai pernyataan struktur, **keliru sebagai penahan rilis**, sebab ruas itu memang tidak diikutkan Rilis 1. **Penahan tunggal `S4b` karena itu lenyap tanpa satu pun keputusan klinis baru.** Dua tafsir yang ditawarkan dan TIDAK diambil dicatat supaya tidak ditemukan ulang: *laporan akhir lawan laporan sementara* — yang akan menjadikannya alur pelaporan bertahap dan itu wewenang `DR-LAB-002`; dan *hasil tidak berubah lagi* — yang bertabrakan dengan `ReleasedAt` pada `LAB-DEC-080` |
| `LAB-DEC-080` | Decision | **Validasi dan rilis dicatat sebagai FAKTA, bukan status. Nol status hasil diperkenalkan.** `LabExamination` memperoleh `ValidatedAt`/`ValidatedByUserId` dan `ReleasedAt`/`ReleasedByUserId`; `LabExaminationStatus` **tidak disentuh** dan tetap `Ordered`/`ChargeEligible`/`Voided`/`Cancelled`. API tetap boleh menyajikan ruas `resultStatus` **turunan** untuk ketiga aplikasi. Menutup `LAB-P0-002` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-18 | **Meneruskan disiplin yang sudah dipegang `S4a`**: kolom mencatat APA YANG TERJADI, bukan MENJANJIKAN apa berikutnya — sama seperti `ResultEnteredAt != null` yang sudah dipakai hari ini, dan sama seperti `LabResultDelivery` yang memilih log berturunan counter ketimbang counter telanjang. **Empat mata `LAB-DEC-003` justru lebih mudah ditegakkan pada bentuk ini**: bandingkan `ResultEnteredByUserId` dengan `ValidatedByUserId`, dan pengecualiannya tinggal ditandai. **Satu jalur yang ditolak dan alasannya perlu disimpan:** menambahkan `Validated`/`Released` ke `LabExaminationStatus` **cacat**, sebab enum itu memuat `ChargeEligible` dan `Voided` yang bersumbu KELAYAKAN TAGIH — pemeriksaan yang `ChargeEligible` lalu menjadi `Validated` akan kehilangan informasi tagihannya. Satu kolom, dua sumbu. **Penyelarasan antaraplikasi tetap `LAB-P0-008`**; keputusan ini menutup urutan status milik Laboratorium, bukan kesepakatan lintas aplikasi |
| `LAB-DEC-079` | Decision | **`LAB-SIGN-001` DITUTUP — tanda tangan klinis diberikan 2026-09-17, dan bentuknya PER DISIPLIN.** `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` disetujui **apa adanya**, masing-masing oleh pemegang wewenang disiplinnya sendiri: `DR-LAB-001` dr. Aditya Pranata, Sp.PK untuk Patologi Klinik; `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK untuk Mikrobiologi Klinik; `DR-LAB-003` dr. Citra Maharani, Sp.PA untuk Patologi Anatomi. Penahan tertua modul ini — terbuka sejak 2026-09-01, diajukan 2026-09-09 lewat `LAB-REQ-004` — berumur 16 hari | `DR-LAB-001`, `DR-LAB-002`, `DR-LAB-003`, disampaikan Yoga Aji Pratama | `approved` | Ketiga pemegang wewenang klinis per disiplin, 2026-09-17 | **NOL PERUBAHAN BUNYI DINYATAKAN, dan itu perlu dibaca tepat.** Jalur "disetujui dengan perubahan" pada `LAB-REQ-004` bagian 8 **tidak** ditempuh; ketiga naskah berlaku sebagaimana tertulis. Maka **hari ini isi aturannya identik bagi ketiga disiplin** — yang per disiplin adalah **wewenangnya**, bukan isinya. Konsekuensinya nyata dan baru: amandemen aturan mikrobiologi kelak cukup ditandatangani `DR-LAB-002` sendiri, dan ketiga disiplin **boleh** menyimpang satu sama lain tanpa membuka ulang keputusan pusat. **Menyimpulkan ada perbedaan hari ini adalah mengarang** — nol perbedaan disampaikan. **Satu pertanyaan lahir dari bentuk ini dan SENGAJA tidak dijawab sendiri:** apakah kewenangan validasi/rilis melintasi disiplin — bolehkah validator Patologi Klinik memvalidasi hasil Mikrobiologi. Dibuka `LAB-OPEN-034`; ia menyentuh `LAB-DEC-003` empat mata, `LAB-DEC-022` jaminan dua pemegang kewenangan per shift, dan `LAB-PERM-v1`. **YANG TIDAK IKUT TERJAWAB, dan `LAB-REQ-004` bagian 7.2 sudah memperingatkannya sejak 2026-09-09:** `LAB-P0-004` (alur nilai kritis lengkap), `LAB-OPEN-014` (nilai kritis mikrobiologi dan patologi anatomi), `LAB-P0-001` (sisa matriks kewenangan), dan kedua penetapan bagian 5 — `LAB-DEC-022` butir 3 dan `LAB-DEC-023`. Keempatnya akan kembali menahan pekerjaan beberapa minggu kemudian bila dibiarkan |
| `LAB-DEC-078` | Decision | **Pemegang wewenang Clinical Governance modul Laboratorium ditetapkan: TIGA orang, satu per disiplin.** `DR-LAB-001` dr. Aditya Pranata, Sp.PK — Patologi Klinik; `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK — Mikrobiologi Klinik; `DR-LAB-003` dr. Citra Maharani, Sp.PA — Patologi Anatomi. Ketiganya menutup **persis** ketiga disiplin scope `LAB-DEC-025`. Menjawab `LAB-REQ-009`, mengisi `owners.clinical_governance` yang kosong sejak 2026-09-01, dan memberi `LAB-REQ-004` alamat yang sah sesudah delapan hari menggantung | Manajemen rumah sakit, disampaikan Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul, sebagai penerus jawaban), 2026-09-17 | **INI TIDAK MENUTUP `LAB-SIGN-001`,** dan itu butir terpenting baris ini. Yang berpindah tepat satu hal: dokumen permintaannya kini punya tujuan. Ketiga keputusan `LAB-DEC-003`/`004`/`007` masih **nol ditandatangani**, dan `S4`, `S4b`, `S4c`, `S5`, `S6` tetap tertahan. **Bentuknya perorangan PER DISIPLIN — bentuk keempat**, di luar ketiga yang ditawarkan `LAB-REQ-009` bagian 4, sedangkan ketiga keputusan yang ditahan dirumuskan LINTAS disiplin; siapa menandatangani apa karena itu **belum tertentu** dan ditanyakan `LAB-REQ-004` bagian 9.2. **Empat ruas penetapan tidak disampaikan**: siapa yang menetapkan, tanggal penetapan, masa berlaku, dan pengganti bila berhalangan — sehingga yang tercatat adalah NAMA, belum sepenuhnya PENETAPAN. **Nol di antara ketiganya bergelar Profesor**, sehingga `RULE-017` yang mensyaratkan *"Profesor dan Dokter Lab"* tetap tidak terpetakan. **`LAB-GOV-SEAT` menyempit, bukan tertutup**: penelusuran source dan seeder 2026-09-17 menemukan nol kemunculan ketiga nama maupun ketiga kode; basis data tidak diperiksa karena nol klien SQL tersedia, dan `MstDoctor` menuntut tujuh ruas wajib yang tidak satu pun disampaikan |
| `LAB-DEC-077` | Decision | **Jabatan `Dokter Penanggung Jawab Laboratorium` didirikan pada data induk**, di bawah departemen `Penunjang Medis`, kode `POS-PMJ-004`. Menurunkan `LAB-DEC-011` yang sudah menamai pemegang wewenang klinis laboratorium, sehingga penetapannya kelak punya tempat dicatat. **`Komite Medis` sengaja TIDAK ikut dibuat** — ia sebuah lembaga, bukan jabatan seseorang, dan memodelkannya sebagai `MstPosition` adalah pertimbangan `master-data` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-17 | **Ini TIDAK membuka `LAB-SIGN-001`.** Penelusuran source menemukan nol kode yang membaca konsep Clinical Governance; tanda tangan itu tindakan manusia atas dokumen, dan yang dibutuhkannya sebuah NAMA. Jabatan ini hari ini ber-pemegang NOL. Jabatan validator dan pengisi untuk aturan empat mata SENGAJA belum dibuat — apa peran itu persis adalah yang justru diputuskan tanda tangannya |
| `LAB-DEC-076` | Decision | **Slice `S4` dipecah menjadi `S4a` pengisian hasil, sementara `S4` dipersempit menjadi validasi dan rilis saja — nama `S4b` TIDAK dipakai karena sudah menjadi milik Mikrobiologi.** `LAB-SIGN-001` ditelusuri sampai akarnya pada 2026-09-17 dan terbukti menahan **tepat tiga** keputusan — `LAB-DEC-003` (empat mata pada validasi dan rilis), `LAB-DEC-004` (nilai kritis), dan `LAB-DEC-007` (koreksi setelah rilis) — seluruhnya soal *perilaku sistem saat hasil salah atau pasien dalam bahaya*. **`LAB-DEC-005`, yang menetapkan hasil diketik manual oleh analis, berdiri TANPA caveat tanda tangan** — setara persis dengan `LAB-DEC-006` yang sudah dibangun penuh sebagai `LabValueBound`. `S4a` karena itu **tidak** tertahan `LAB-SIGN-001` dan masuk scope; `S4b` tetap tertahan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-17 | **`S4a` nol mendirikan status hasil.** Komentar `LabExaminationStatus` sudah menyatakan `Pending`/`InProcess`/`Completed`/`Validated`/`Released` sengaja ditahan, dan disiplin itu **dipertahankan**: pengisian mencatat APA YANG TERJADI lewat waktu dan pelakunya, bukan MENJANJIKAN apa yang terjadi berikutnya. Membuka juga `Tanggal Pemeriksaan` pada `REC3-NEW-002` |
| `LAB-DEC-075` | Decision | **Nota Lab, Label Lab, dan Label Golongan Darah dipindahkan dari Rilis 2 ke Rilis 1**, mengamandemen penempatannya pada `LAB-DEC-030`/BR-26. Alasan pemindahan: seluruh datanya ternyata **sudah berdiri** — `OrderNumber` sejak `BE-LAB-36`, `orderedProcedures` sejak `BE-LAB-35`, `MstPatient.BloodType` sejak sebelum modul ini, dan pustaka barcode sudah ada di frontend — sehingga alasan penundaannya ("kemampuan cetak, tidak memblokir alur kerja") tidak lagi sebanding dengan biayanya yang mendekati nol. **Label Goldar menampilkan golongan darah pasien**, melengkapi artifact yang hanya menulis "informasi utama pasien" padahal namanya menyebut golongan darah | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-17 | Menurunkan `LAB-API-v1` `r20`. **Ukuran cetaknya TIDAK ikut diadopsi** — `LAB-OPEN-032` tetap terbuka, dan angkanya hidup sebagai konstanta berlabel "belum dikonfirmasi" |
| `LAB-DEC-074` | Decision | **Aturan tanggal `LAB-DEC-064` dan `LAB-DEC-071` berlaku untuk SELURUH penyaring tanggal modul Laboratorium, bukan hanya Menu Hasil.** Ketiga menu Pemeriksaan yang sudah dipakai petugas ikut terikat: tanggal masa depan tidak boleh dipilih, dan `Tgl Awal <= Tgl Akhir`. **Dasarnya konsistensi yang dirasakan petugas** — dua layar Laboratorium yang berperilaku berbeda untuk penyaring yang bentuknya sama adalah cacat yang dilaporkan sebagai bug, bukan sebagai perbedaan cakupan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-16 | Menutup `LAB-RDY-C02`. **Perbaikannya menyentuh komponen base milik bersama:** `FilterDatePicker` dipakai 133 berkas lintas modul. Jalur yang ditempuh sengaja aditif — prop **opsional** `max` dengan default tidak berubah, sehingga 132 pemakai lain nol terdampak. Diturunkan menjadi `FE-LAB-18` |
| `LAB-DEC-049` | Decision | **Penguncian daftar pemeriksaan hanya berlaku pada penambahan, bukan pembatalan.** Pembatalan tetap terbuka sesudah kelayakan ditetapkan dan koreksi tagihannya wewenang Billing. `VAL-18` tidak berubah bunyinya dan tidak diperluas. Mengamandemen `AC-55` dan `AC-57` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-44. Menutup `LAB-CONFLICT-004` tanpa menyentuh `RJ-BIL-GATE-DEC-003`; [`BE-LAB-24.md`](task/report/backend/BE-LAB-24.md) |
| `LAB-CONFLICT-005` | Conflict | **`AC-52` bertentangan dengan index unik `(SpecimenId, ProcedureId)`** yang dipasang atas dasar `BR-20` dan `AC-35` | Yoga Aji Pratama | `closed` | Ditutup 2026-09-14 oleh `LAB-DEC-050` | Ditemukan `BE-LAB-23` |
| `LAB-CONFLICT-004` | Conflict | **`AC-55` dan `AC-57` bertentangan dengan `LAB-INH-001`, `LAB-INH-006`, dan `LAB-INH-010`** yang mengizinkan pembatalan terkendali sesudah titik tagih | Yoga Aji Pratama | `closed` | Ditutup 2026-09-14 oleh `LAB-DEC-049` | Ditemukan `BE-LAB-24` |
| `LAB-DEC-048` | Decision | *Label diamandemen 2026-10-06 oleh `LAB-DEC-192` — tiga menu Pemeriksaan menjadi Daftar Pasien Lab; selebihnya tetap.* **Susunan menu Laboratorium dirapikan, disiplin berhenti ditanyakan.** Butir menu Pesanan Laboratorium dicabut; tiga menu Monitoring dinamai ulang menjadi Pemeriksaan; empat penyaring dicabut dan Kesegeraan disederhanakan menjadi `Semua Data`/`Cito`; dua tombol pindah disiplin dicabut; **disiplin pesanan diturunkan dari pemeriksaan yang dipilih**, tidak lagi dipilih petugas; baris pada ketiga layar Pemeriksaan dapat dibuka ke detail pesanannya. Layar `lab-orders` beserta seluruh route-nya **tidak dihapus**. Mengamandemen `LAB-DEC-045`, `AC-76`, dan `LAB-FE-009` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-43. Dikerjakan lebih dulu di frontend atas arahan langsung, di luar jalur task roadmap; menegakkan `LAB-DEC-036` |
| `LAB-DEC-047` | Decision | **Metode pembayaran diturunkan untuk rujukan luar, dinyatakan petugas untuk datang langsung.** `PaymentType` dan `PaymentMethodId` tetap ada pada permintaan pendaftaran tetapi hanya sah untuk jalur datang langsung; pada jalur rujukan luar keduanya diabaikan dan kesimpulannya diturunkan Registrasi dari status PKS instansi perujuk. Laboratorium tidak membaca penanda PKS — `AC-73` tetap berlaku. **Mengamandemen `LAB-DEC-044`** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-42. Menjawab `Q-LAB-07`; menutup sisa `LAB-CONFLICT-003`. Menegakkan `BR-25` dan pola `LAB-DEC-032` |
| `LAB-DEC-046` | Decision | **Piutang mitra menjadi penjamin kunjungan tersendiri.** `EncounterPaymentType` bertambah satu nilai baru secara aditif; `Cash`, `Insurance`, dan `CompanyGuarantor` tidak bergeser. Rumah sakit perujuk ber-PKS **tidak** dititipkan pada `CompanyGuarantor`, karena yang terakhir berarti tempat pasien bekerja. Enum tetap milik Registrasi; Laboratorium hanya memakainya. Penetapan status PKS tetap di luar scope | Yoga Aji Pratama + pemilik `registration-management` + pemilik `billing-kasir` | `draft` — posisi Laboratorium ditetapkan pemilik modul; pemilik enum **belum** | Yoga Aji Pratama (pemilik modul), 2026-09-14, **hanya untuk posisi Laboratorium** | Lihat BR-41. Menjawab `Q-LAB-06`; menutup sebagian `LAB-CONFLICT-003`. Preseden: `CompanyGuarantor = 3` ditambahkan aditif lewat `BE-RWI-035` |
| `LAB-DEC-045` | Decision | *Diamandemen 2026-10-08 oleh `LAB-DEC-214` — satu Simpan sampai wadah diterima; kelayakan tidak lagi ditetapkan di layar ini (`LAB-DEC-219`).* **`Penerimaan Sampling/Specimen` berdiri sebagai menu tersendiri** untuk pasien rujukan luar dan datang langsung, memuat satu rangkaian dari identifikasi pasien sampai penetapan kelayakan. Layar `lab-orders` dan specimen per pesanan **tetap dipertahankan** untuk pesanan dokter. Keduanya memakai data dan aturan yang sama; yang berbeda urutan penyajian dan titik kuncinya. Tata letak tetap `DEV_DISCRETION` | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-40. Alasan: titik kunci `LAB-DEC-039` mensyaratkan daftar pemeriksaan dan wadah terlihat berdampingan. Membuka `LAB-FE-009` sampai `LAB-FE-011` |
| `LAB-DEC-044` | Decision | **Metode pembayaran dibaca dari Billing dan ditampilkan baca-saja.** Laboratorium memanggil Billing dengan penunjuk kunjungan dan menampilkan jawabannya beserta alasan; tidak menyimpulkan sendiri dari penanda PKS, dan tidak mengirim apa pun yang baru. Bila Billing tidak dapat dihubungi, layar menulis *belum dapat ditentukan* dan penerimaan specimen **tetap berjalan**. Penerimaan uang tunai tetap di luar scope | Yoga Aji Pratama + pemilik `billing-kasir` | **`amended` oleh `LAB-DEC-047`** pada 2026-09-14; **diamandemen lagi oleh `LAB-DEC-221`** pada 2026-10-08 (metode pembayaran dipilih saat kunjungan baru dibentuk di layar Penerimaan) | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-39. Yang **tetap berlaku**: Laboratorium tidak menjalankan aturan uang, tampilan baca-saja untuk jalur rujukan, dan fail-open saat layanan penjamin tidak terjawab. Yang **dicabut**: anggapan bahwa jawabannya datang dari Billing lewat endpoint baru, dan bahwa Laboratorium tidak mengirim apa pun |
| `LAB-COORD-007` | Open Question | ~~Endpoint baca metode pembayaran kepada pemilik `billing-kasir`~~. **Dirumuskan ulang 2026-09-14:** kesepakatan dengan pemilik `registration-management` untuk menambah satu nilai `EncounterPaymentType` bagi piutang mitra secara aditif, beserta penurunannya dari status PKS instansi perujuk. Pemilik `billing-kasir` diperlukan untuk cara menagihkannya | Yoga Aji Pratama + pemilik `registration-management` + pemilik `billing-kasir` | `open` — **diajukan 2026-09-14** | — | Konsekuensi `LAB-DEC-046` dan `LAB-DEC-047`. Diajukan sebagai `LAB-REQ-005` butir 4-7 pada [`approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md`](approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md). Memblokir bagian metode pembayaran pada layar penerimaan |
| `LAB-DEC-043` | Decision | **Laboratorium mengusulkan instansi perujuk baru, Master Data yang mengesahkan.** Usulan langsung menjadi satu baris data induk berstatus menunggu persetujuan yang dapat ditunjuk kunjungan, sehingga penerimaan specimen tidak tertahan. Pencarian daftar wajib diperlihatkan sebelum usulan dikirim. Laboratorium tetap tidak dapat menyetujui usulannya sendiri maupun menetapkan status PKS. `LAB-DEC-035` butir 4 dan `AC-50` tidak dicabut | Yoga Aji Pratama + pemilik `master-data` | `draft` — bagian Laboratorium disetujui pemilik modul; bagian Master Data **belum** | Yoga Aji Pratama (pemilik modul), 2026-09-14, **hanya untuk bagian Laboratorium** | Lihat BR-38. Menjawab `CAP-008` dan `RULE-014` pada `LAB-EVD-001`. Membuka `LAB-COORD-006` dan `LAB-OPEN-024` |
| `LAB-COORD-006` | Open Question | Kesepakatan dengan pemilik `master-data`: status menunggu persetujuan pada data induk instansi perujuk, layar persetujuan, dan kemampuan menggabungkan dua baris. **Diperbesar 2026-09-14:** impact scan menemukan `MstReferralInstitution` **tidak punya endpoint tulis sama sekali** — `ReferralInstitutionController@466a7127` hanya `GET /options`, dan satu-satunya pengisi tabelnya adalah `LabDummyDataSeeder`. Yang kurang bukan sekadar status menunggu, melainkan seluruh kemampuan pengelolaannya | Yoga Aji Pratama + pemilik `master-data` | `open` — **diajukan 2026-09-14** | — | Konsekuensi `LAB-DEC-043`; diperbesar capability map revision 3. Diajukan sebagai `LAB-REQ-005` butir 1-3 pada [`approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md`](approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md). Memblokir bagian pendaftaran rujukan pada layar penerimaan |
| `LAB-DEBT-001` | Fact | Data induk **global** instansi perujuk diisi oleh `Areas/HealthServices/LaboratoryManagement/Seeders/LabDummyDataSeeder.cs:498,513@466a7127` — seeder milik Laboratorium mengisi data induk milik Master Data. Utang teknis terhadap `AC-49` | Yoga Aji Pratama + pemilik `master-data` | `fact` | — | Ditemukan capability map revision 3. Dicatat, tidak diperbaiki — audit bersifat read-only |
| `LAB-OPEN-024` | Open Question | Berapa lama usulan instansi perujuk boleh menggantung, apa yang terjadi pada kunjungan bila usulan **ditolak**, dan siapa yang berhak menggabungkan dua baris? | Pemilik `master-data` | `open` | — | Konsekuensi `LAB-DEC-043`; sengaja tidak diputuskan Laboratorium karena bukan wilayahnya |
| `LAB-DEC-042` | Decision | **Waktu sistem dan waktu nyata disimpan berdampingan.** `ReceivedAt` tetap diisi server dan tidak dapat diubah; ditambahkan waktu penerimaan fisik yang diisi petugas, tidak boleh di masa depan dan tidak boleh mendahului pengambilan. Laporan penerimaan dan lama tunggu memakai waktu nyata; perhitungan keterlambatan cito `AC-17` tidak berubah. Hanya bagian specimen dari `RULE-019` yang diterima — Tanggal Registrasi tetap milik Registrasi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-37. Menegakkan `LAB-DEC-032` dan `LAB-DEC-037` |
| `LAB-DEC-041` | Decision | **Volume specimen tersimpan sebagai angka beserta satuannya**, dipilih dari daftar pendek `mL`, `µL`, `gram`, `blok`, `slide`. Ketiadaan batas minimum dan maksimum pada `RULE-021` diterima apa adanya; ketiadaan satuan ditolak. Penilaian kecukupan sampel tetap lewat penetapan kelayakan, bukan perbandingan angka oleh sistem. Volume minimal per jenis pemeriksaan tetap di Rilis 2 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-36. Alasan: `LAB-DEC-025` dan `LAB-DEC-040` mengesahkan Jaringan, yang tidak terukur dalam mililiter |
| `LAB-DEC-040` | Decision | **Jenis specimen menjadi data induk terkendali milik Laboratorium** dengan tujuh nilai awal. Pilihan `Lainnya` tetap ada, wajib berketerangan, dan setiap pemakaiannya masuk daftar pantau kepala instalasi yang dapat menaikkannya menjadi nilai tetap. `SpecimenDescription` turun pangkat menjadi keterangan operasional, bukan tempat menyimpan jenis. Tingkat transaksi, bukan katalog — `LAB-DEC-001` Rilis 2 tidak ditarik maju | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-35. Menerapkan `LAB-DEC-034`; menegakkan alasan `LAB-DEC-035` tanpa mengulang jalan buntu `VAL-43` |
| `LAB-DEC-039` | Decision | **Titik kunci daftar pemeriksaan berbeda menurut jalur masuknya.** Pesanan dokter tetap terkunci saat sampel pertama `Collected`; penerimaan sampling langsung di laboratorium terkunci saat **kelayakan wadah ditetapkan**. Aksi `Pemeriksaan Diproses` pada `LAB-EVD-001` dipetakan ke aksi penetapan kelayakan yang sudah ada — tidak ada tombol ketiga. Mengamandemen `AC-20`. **Diverifikasi 2026-09-14: kode sudah berperilaku begini sejak sebelum keputusan ini** — `LabExaminationService.cs:123@466a7127` mengunci pada `Accepted or Rejected` sebagai `VAL-18`, dan tidak ada satu pun rujukan `Collected` di `LabOrderService` maupun `LabExaminationService`. Keputusan ini karena itu **tidak mengubah perilaku**; ia menyamakan `AC-20` dengan kenyataan yang sudah berjalan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-34. Alasan: `AC-37` menerbitkan kelayakan tagih tepat pada penetapan kelayakan. Diperkuat capability map revision 3 — status `Ready to reuse`, bukan `Extend` |
| `LAB-DEC-038` | Decision | **Qty adalah alat bantu layar, bukan kolom penyimpanan.** Qty bernilai 3 memecah diri menjadi tiga baris `LabExamination`, masing-masing dengan salinan tarif, status, dan tempat hasilnya sendiri. Tidak ada kolom Qty pada tabel mana pun. Penguncian `RULE-025` berarti baris tidak dapat ditambah atau dihapus. `IsDuplo` tidak digantikan Qty | Yoga Aji Pratama | **`superseded` oleh `LAB-DEC-050`** pada 2026-09-14 | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Lihat BR-33, dicabut BR-45. **Terbukti tidak dapat dilaksanakan** `BE-LAB-23`: index unik `(SpecimenId, ProcedureId)` atas dasar `BR-20` dan `AC-35` menolak baris kedua. `LAB-GAP-001` tetap tertutup — celahnya nyata, hanya jawabannya yang keliru |
| `LAB-DEC-037` | Decision | **Batas scope Amendment Pass putaran 2 dikunci.** Delapan butir tetap di Laboratorium: jenis specimen terkendali, volume specimen, Qty baris pemeriksaan, penguncian Qty setelah `Pemeriksaan Diproses`, tanggal penerimaan specimen yang dapat diisi petugas, bentuk layar, serta dua titik sentuh ke Billing dan Master Data. Enam butir `LAB-EVD-001` diserahkan ke modul pemiliknya: pendaftaran RS perujuk baru, penentuan metode bayar dan penerimaan tunai, promo, status PKS, penyimpanan pasien baru beserta tanggal registrasi dan kategori usia, serta kewajiban pemindaian KTP se-rumah sakit. **Tidak satu pun keputusan terkunci dicabut** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama (pemilik modul), 2026-09-14 | Bukti: `LAB-EVD-001`. Menegakkan `LAB-DEC-029`, `LAB-DEC-032`, `LAB-DEC-033`, `LAB-DEC-035`, `AC-45`, `AC-50` |
| `LAB-OPEN-022` | Open Question | Capability map dikunci pada BE `c87d9c0` + FE `688daff90`, sementara `HEAD` pada 2026-09-14 adalah BE `466a7127` + FE `9cd4cd03f`. Selisihnya belum dipindai, sehingga status kemampuan existing berpotensi basi | Yoga Aji Pratama | `closed` | Ditutup 2026-09-14 oleh impact scan capability map revision 3 | Hasilnya: 7 dari 9 keputusan amendment terbukti; `LAB-DEC-039` ternyata sudah dikerjakan kode; `LAB-DEC-044` dibuka ulang sebagai `CONF-02` |
| `LAB-OPEN-023` | Open Question | `blueprint-manifest.md` mengunci `input_revisions.decisions: 21` dan `backend_commit_sha`/`frontend_commit_sha` lama. **Dipersempit 2026-09-14:** hash `02-requirement-completeness-assessment.md` dan `03-domain-architecture.md` terverifikasi **tidak berubah**; yang perlu diperbarui hanya baris `decisions`, hash decision log, hash capability map, dan kedua `*_commit_sha` | Yoga Aji Pratama | `open` | — | Memblokir `DESIGN`; pembukuan, bukan keputusan bisnis. Bukti: capability map revision 3 bagian pembukuan manifest |
| `LAB-CONFLICT-003` | Conflict | **Ditutup 2026-09-14** oleh `LAB-DEC-046` dan `LAB-DEC-047`. **`LAB-DEC-044` berdiri di atas fakta yang salah.** Impact scan menemukan: metode pembayaran per kunjungan **sudah ada** pada `RegPatientEncounterGuarantor@466a7127` milik **Registrasi**, bukan Billing; Laboratorium hari ini justru **mengirimkannya** lewat `LabPatientRegistrationDtos.cs:80,83,120,122`, bukan membacanya; dan **`Piutang Mitra` tidak ada** pada `EncounterPaymentType` yang hanya mengenal `Cash`, `Insurance`, `CompanyGuarantor` | Yoga Aji Pratama | `closed` | Ditutup 2026-09-14 oleh `LAB-DEC-046` (`Q-LAB-06`) dan `LAB-DEC-047` (`Q-LAB-07`) | Capability map revision 3 `CONF-02` |

---

## Acceptance Criteria

Kriteria berikut sudah dapat diuji. Nomor acuannya dipakai kembali saat menyusun roadmap dan
pengujian.

| No | Kriteria yang dapat diuji | Acuan |
|---:|---|---|
| AC-01 | Petugas yang mengisi hasil, ketika menekan Validasi pada hasil yang sama, ditolak sistem kecuali ia mengisi alasan pengecualian dari daftar terkendali | BR-01 |
| AC-02 | Hasil yang divalidasi lewat jalur pengecualian menampilkan penanda "divalidasi oleh pengisi sendiri" di layar, di cetakan, dan di riwayat | BR-01 |
| AC-03 | Hasil dengan nilai melewati batas kritis tetap berpindah ke status `Released`, dan sekaligus memunculkan formulir pelaporan wajib | BR-02, BR-04 |
| AC-04 | Selama formulir pelaporan nilai kritis belum terisi lengkap, pemeriksaan itu muncul pada daftar pantau "nilai kritis belum dilaporkan" | BR-02 |
| AC-05 | Hasil dengan nilai di luar batas normal tetapi belum melewati batas kritis ditandai "di atas/di bawah nilai rujukan" **tanpa** memunculkan formulir pelaporan | BR-04 |
| AC-06 | Analis biasa yang mencoba mengoreksi hasil berstatus `Released` ditolak sistem | BR-05 |
| AC-07 | Setelah hasil koreksi dirilis ulang, versi lama tetap dapat dilihat dengan tanda "sudah diperbaiki", dan dokter pemesan menerima pemberitahuan | BR-05 |
| AC-08 | Dalam satu pesanan berisi beberapa pemeriksaan, satu pemeriksaan dapat berstatus `Released` sementara pemeriksaan lain masih `InProcess` | BR-06 |
| AC-09 | Lembar hasil yang belum lengkap menampilkan peringatan "hasil belum lengkap" | BR-06 |
| AC-10 | Pesanan bertanda cito muncul di urutan lebih atas daripada pesanan biasa pada daftar kerja petugas, tanpa memandang jam masuk | BR-07 |
| AC-11 | Pesanan lab dapat dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD dengan alur kerja yang sama | BR-07 |
| AC-12 | Fakta kelayakan tagih diterbitkan tepat pada transisi sampel ke `Accepted`, dan tidak bergantung pada dirilisnya hasil | `LAB-INH-009` |
| AC-13 | Tidak ada satu pun endpoint atau kolom Laboratorium yang menghitung, mengubah, membatalkan, atau mengembalikan uang | `LAB-INH-012` |
| AC-14 | Pemberitahuan nilai kritis tetap tersimpan dan terbaca oleh dokter meskipun dokter itu tidak sedang membuka aplikasi saat hasil keluar | BR-08 |
| AC-15 | Setiap pemberitahuan menyimpan status sudah dibaca atau belum, beserta waktu dibacanya | BR-08 |
| AC-16 | Pemberitahuan koreksi hasil terkirim ke dokter pemesan setiap kali hasil perbaikan dirilis ulang | BR-05, BR-08 |
| AC-17 | Pesanan cito yang melewati batas waktu penyelesaiannya muncul pada daftar pantau keterlambatan, dihitung sejak sampel `Accepted` sampai hasil `Released` | BR-09 |
| AC-18 | Penandaan cito hanya dapat dilakukan dokter pemesan saat membuat pesanan, bukan otomatis berdasarkan unit asal | BR-09 |
| AC-19 | Tidak ada satu pun tabel atau endpoint Laboratorium yang menyimpan stok, pembelian, atau pemakaian reagen | BR-10 |
| AC-20 | **Diamandemen `LAB-DEC-039`.** Untuk pesanan dokter dari poliklinik, rawat inap, dan IGD: dokter pemesan dapat menyunting daftar pemeriksaan selama belum ada sampel berstatus `Collected`, dan ditolak sistem setelah sampel pertama diambil. Untuk penerimaan sampling langsung di laboratorium, titik kuncinya adalah penetapan kelayakan wadah — lihat AC-55 | BR-11, BR-34 |
| AC-21 | Tidak ada satu pun pesanan yang pernah berstatus `Draft`; nilai itu tidak lagi ada pada siklus hidup pesanan | BR-11 |
| AC-22 | Struktur data pemberitahuan tidak memuat satu pun istilah khas laboratorium, sehingga modul lain dapat memakainya tanpa perubahan | BR-12 |
| AC-23 | Setiap hasil yang dirilis memiliki satu baris pencatatan keutuhan di rekam medis, dan angka hasilnya tidak digandakan ke tabel rekam medis mana pun | BR-13 |
| AC-24 | Satu jenis pemeriksaan dapat memiliki lebih dari satu baris batas nilai yang dibedakan menurut jenis kelamin dan kelompok umur | BR-14 |
| AC-25 | `MstProcedure` tidak bertambah satu pun kolom **operasional** laboratorium — satuan, batas nilai, jenis wadah. Satu-satunya tambahan yang diizinkan adalah kolom klasifikasi disiplin | BR-14, BR-32 |
| AC-26 | Kepala instalasi dapat menambah dan menonaktifkan alasan penolakan, tetapi percobaan mengubah penanda kesalahan internal atau penanda wajib catatan ditolak sistem | BR-15 |
| AC-27 | Koreksi hasil pada kunjungan yang sudah ditutup menghasilkan addendum bertanda tangan beralasan, sementara dokumen hasil aslinya tetap terkunci dan isinya tidak berubah | BR-16 |
| AC-28 | Pemeriksaan berhasil pilihan hanya menerima nilai dari daftar pilihan yang sah; pengetikan bebas ditolak sistem | BR-17 |
| AC-29 | Protein urin bernilai `+4` memicu formulir pelaporan nilai kritis persis seperti Kalium 7,2 mmol/L memicunya | BR-17, BR-02 |
| AC-30 | Pemeriksaan berhasil pilihan tanpa nilai kritis, seperti golongan darah, tidak pernah memunculkan formulir pelaporan | BR-17 |
| AC-31 | Kewenangan validasi dan kewenangan rilis dapat diberikan kepada orang yang berbeda, dan pemberiannya tidak mengikuti jabatan | BR-18 |
| AC-32 | Kepala instalasi menerima peringatan bila suatu shift hanya memiliki satu pemegang kewenangan validasi | BR-18 |
| AC-33 | Perubahan batas normal oleh kepala instalasi langsung berlaku, sedangkan perubahan batas kritis tertahan sebagai pengajuan sampai disetujui pihak klinis | BR-19 |
| AC-34 | Setiap perubahan batas nilai menyimpan kolom yang berubah, nilai lama, nilai baru, pelaku, penyetuju, waktu, dan alasan | BR-19 |
| AC-35 | Satu wadah fisik dapat menopang lebih dari satu pemeriksaan terpesan, dan hanya memiliki satu barcode | BR-20 |
| AC-36 | Menolak sebuah wadah menggugurkan seluruh pemeriksaan yang ditopangnya; percobaan menolak sebagian ditolak sistem | BR-20 |
| AC-37 | Menyatakan satu wadah layak menerbitkan kelayakan tagih sebanyak pemeriksaan yang ditopangnya, masing-masing dengan tarifnya sendiri | BR-20, `LAB-INH-009` |
| AC-38 | Ambil ulang menciptakan wadah baru yang menampung seluruh pemeriksaan dari wadah lama, sementara wadah lama tetap terlihat beserta tautan sebabnya | BR-20, `LAB-INH-005` |
| AC-39 | Satu pesanan dapat memuat pemeriksaan bertanda cito dan pemeriksaan biasa sekaligus; hanya yang bertanda cito naik ke urutan atas daftar kerja | BR-22 |
| AC-40 | Penanda cito dan duplo hanya dapat disetel pada baris pemeriksaan; percobaan menyetelnya pada pesanan ditolak sistem | BR-22 |
| AC-41 | Pesanan menyimpan disiplinnya, dan daftar pantau dapat disaring per disiplin: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi | BR-21 |
| AC-42 | Tidak ada satu pun tabel atau endpoint Laboratorium yang melayani Bank Darah | BR-21 |
| AC-43 | Layar pemesanan menampilkan harga satuan, subtotal, total, dan status cakupan, tanpa membentuk tagihan apa pun | BR-25 |
| AC-44 | Pendaftaran pasien datang langsung dari layar Laboratorium menghasilkan kunjungan yang dibuat modul Registrasi, bertanda datang langsung, dan pesanan lab menempel padanya | BR-28 |
| AC-45 | Tidak ada satu pun kode Laboratorium yang menulis ke tabel kunjungan maupun tabel pasien | BR-28 |
| AC-46 | Pendaftaran pasien rujukan luar menyimpan dokter perujuk, instansi perujuk, dan nomor surat rujukan pada kunjungan yang dibuat Registrasi | BR-28 |
| AC-47 | Modul Laboratorium tidak memiliki tabel tarif sendiri; harga selalu berasal dari `MstTariff` | BR-29 |
| AC-48 | Menu `Tarif Laboratorium` bersifat baca saja; percobaan mengubah tarif dari modul Laboratorium ditolak sistem | BR-29 |
| AC-49 | Pada **backend**, data induk khusus Laboratorium berada di folder Laboratorium dan data induk global tidak disalin ke sana. Pada **frontend**, seluruh menu data induk berada di `health-services/master-data/` | BR-30 |
| AC-50 | Nama instansi dan dokter perujuk dipilih dari daftar terkendali, bukan diketik bebas; kunjungan menyimpan penunjuk, bukan teks | BR-31 |
| AC-51 | Menambahkan pemeriksaan yang disiplinnya tidak sesuai disiplin pesanan ditolak sistem | BR-32, `INV-22` |
| ~~AC-52~~ | **Dicabut `LAB-DEC-050`.** Semula: Qty 3 menghasilkan tiga baris pemeriksaan. Terbukti tidak dapat dipenuhi — index unik `(SpecimenId, ProcedureId)` menolak baris kedua | ~~BR-33~~, BR-45 |
| ~~AC-53~~ | **Dicabut `LAB-DEC-050`.** Bergantung pada AC-52 | ~~BR-33~~, BR-45 |
| ~~AC-54~~ | **Dicabut `LAB-DEC-050`.** Bergantung pada AC-52. Kelayakan tagih per baris tetap berlaku lewat `AC-37` yang tidak tersentuh | ~~BR-33~~, BR-45, `AC-37` |
| AC-55 | **Diamandemen `LAB-DEC-049`.** Pada penerimaan sampling langsung, **menambah** baris pemeriksaan diizinkan selama kelayakan wadah belum ditetapkan dan ditolak sistem setelahnya. **Pembatalan tidak ikut terkunci** — ia tetap terbuka lewat `POST /{id}/cancel`, dan koreksi tagihannya wewenang Billing | BR-34, BR-44 |
| AC-56 | Tidak ada aksi `Pemeriksaan Diproses` yang berdiri sendiri; penguncian daftar selalu melekat pada aksi penetapan kelayakan yang sudah ada | BR-34 |
| AC-57 | **Diamandemen `LAB-DEC-049`.** Tidak ada jalan **menambah** baris pemeriksaan setelah kelayakan tagih terbit. Pembatalan tetap ada sebagai jalur sah menurut `LAB-INH-006`, dan layar wajib memberi tahu bahwa tagihannya baru gugur setelah Billing mengoreksi | BR-34, BR-44, `AC-37` |
| AC-58 | Jenis specimen dipilih dari daftar terkendali; mengirim jenis sebagai teks bebas di luar jalur `Lainnya` ditolak sistem | BR-35 |
| AC-59 | Memilih `Lainnya` tanpa mengisi keterangan ditolak sistem, dan penerimaan specimen berjenis `Lainnya` **tidak pernah** dihalangi hanya karena jenisnya belum terdaftar | BR-35 |
| AC-60 | Setiap pemakaian `Lainnya` terlihat pada daftar pantau kepala instalasi beserta keterangan dan jumlah pemakaiannya | BR-35 |
| AC-61 | `SpecimenDescription` tidak lagi menjadi satu-satunya tempat jenis specimen tersimpan; data lama yang hanya punya teks bebas tetap terbaca dan tidak dihapus | BR-35 |
| AC-62 | Volume specimen tidak dapat disimpan tanpa satuan; mengirim angka saja ditolak sistem | BR-36 |
| AC-63 | Volume bernilai berapa pun diterima sistem tanpa peringatan maupun penolakan, termasuk nilai yang sangat kecil; yang menyatakan sampel tidak cukup adalah petugas lewat penetapan kelayakan | BR-36, `RULE-021` |
| AC-64 | Specimen berjenis Jaringan dapat menyimpan volumenya dalam satuan `gram`, `blok`, atau `slide` tanpa dipaksa memakai satuan cairan | BR-36, BR-35 |
| AC-65 | Tidak ada satu pun endpoint Laboratorium yang dapat mengubah `ReceivedAt`; percobaan mengirimnya dari luar diabaikan sistem | BR-37 |
| AC-66 | Waktu penerimaan fisik yang berada di masa depan, atau yang mendahului waktu pengambilan specimen, ditolak sistem | BR-37 |
| AC-67 | Laporan penerimaan harian menempatkan specimen menurut waktu nyatanya, dan selisih terhadap waktu sistem dapat dilihat kepala instalasi | BR-37 |
| AC-68 | Mengirim usulan instansi perujuk tanpa lebih dulu memperlihatkan hasil pencarian daftar yang sudah ada ditolak sistem | BR-38 |
| AC-69 | Usulan instansi perujuk tersimpan sebagai **satu baris data induk** berstatus menunggu persetujuan; tidak ada satu pun tabel Laboratorium yang menyimpan nama instansi perujuk sebagai teks | BR-38, `AC-50` |
| AC-70 | Penerimaan specimen dengan instansi perujuk berstatus menunggu persetujuan **tetap berhasil**; tidak ada `422` yang menahannya | BR-38 |
| AC-71 | Percobaan menyetujui usulan, mengubah baris yang sudah disetujui, atau menetapkan status PKS dari modul Laboratorium ditolak sistem | BR-38, `AC-48` |
| AC-72 | Metode pembayaran pada layar penerimaan tidak dapat diubah dari sisi Laboratorium, dan tidak ada satu pun endpoint Laboratorium yang menerimanya sebagai masukan | BR-39, BR-25 |
| AC-73 | Tidak ada satu pun baris kode Laboratorium yang membaca penanda PKS untuk menyimpulkan metode pembayaran | BR-39 |
| AC-74 | Saat layanan penjamin tidak dapat dihubungi, penerimaan specimen **tetap berhasil disimpan** dan metode pembayaran ditampilkan sebagai *belum dapat ditentukan* | BR-39, BR-42 |
| AC-78 | Pada pendaftaran **rujukan luar**, `PaymentType` dan `PaymentMethodId` yang dikirim Laboratorium **diabaikan**; kunjungan yang terbentuk memakai penjamin yang diturunkan dari status PKS instansi perujuk | BR-42 |
| AC-79 | Pada pendaftaran **datang langsung**, petugas dapat menyatakan penjamin, dan penjamin yang tidak sah **ditolak Registrasi** dengan pesannya sendiri sesuai `VAL-40` | BR-42, BR-28 |
| AC-80 | Ruas metode pembayaran pada formulir rujukan luar bersifat **baca-saja**; tidak ada kotak pilihan padanya | BR-42, `LAB-FE-011` |
| AC-81 | Nilai `Cash`, `Insurance`, dan `CompanyGuarantor` pada `EncounterPaymentType` **tidak bergeser** setelah nilai piutang mitra ditambahkan | BR-41, `RWI-ENC-PAYER-001` |
| AC-82 | Rumah sakit perujuk **tidak pernah** tercatat sebagai `CompanyGuarantor`; laporan penjamin perusahaan tidak memuat satu pun instansi perujuk | BR-41 |
| AC-75 | Satu pasien rujukan luar dapat diselesaikan dari identifikasi sampai penetapan kelayakan **tanpa berpindah menu** | BR-40 |
| AC-76 | **Diamandemen `LAB-DEC-048`.** Layar `lab-orders` dan layar specimen per pesanan tetap berfungsi seperti sebelumnya; tidak ada satu pun **perilakunya** yang berubah. Butir menunya di sidebar dicabut, dan jalan masuknya berpindah: pembuatan pesanan dari pendaftaran pasien, detail pesanan dari ketiga layar Pemeriksaan | BR-40, BR-43 |
| AC-83 | **Diamandemen `LAB-DEC-055`.** Pesanan yang dibuat lewat pendaftaran pasien laboratorium **selalu** membawa disiplin, diturunkan dari pemeriksaan yang dipilih; tidak ada jalan menyimpan pesanan berdisiplin kosong ketika pemeriksaannya sudah digolongkan. **Ditegakkan di backend, bukan hanya di layar** — lihat `LAB-CONFLICT-007`. Pemeriksaan lintas disiplin menghasilkan beberapa pesanan, masing-masing berdisiplin tunggal (`AC-86`) | BR-43, BR-32, BR-47 |
| AC-84 | Pasien yang pesanannya berhasil dibuat muncul pada menu Pemeriksaan yang **sesuai disiplin pemeriksaannya**, dan barisnya dapat dibuka ke detail pesanan | BR-43 |
| AC-85 | Pemeriksaan yang **belum digolongkan** disiplinnya tetap dapat dipesan, dan pesanannya tersimpan tanpa disiplin — keadaan ini sah, bukan data rusak | BR-43, BR-32 |
| AC-86 | Pemeriksaan lintas disiplin yang dipilih dalam satu tindakan menghasilkan **satu pesanan per disiplin**, dan setiap pesanan muncul pada menu Pemeriksaan yang sesuai | BR-47, BR-43 |
| AC-87 | Pemeriksaan yang belum digolongkan disiplinnya berkumpul menjadi **satu pesanan tanpa disiplin** pada tindakan yang sama; keadaan ini tetap sah dan tidak menggagalkan pemesanan pemeriksaan lainnya | BR-47, `AC-85` |
| AC-88 | `POST /lab-orders` yang sudah ada **tidak berubah bentuk maupun perilakunya**; pemanggil lama tetap menerima tepat satu pesanan | BR-47 |
| AC-89 | Satu sesi kiosk **tidak dapat diproses menjadi pendaftaran dua kali**; percobaan kedua tidak membentuk kunjungan maupun pesanan baru | BR-46 |
| AC-90 | Pasien yang memilih Laboratorium di kiosk lalu pergi tanpa diperiksa **tidak menghasilkan satu pun tagihan pemeriksaan laboratorium** | BR-46, `AC-37` |
| AC-91 | Setiap pemeriksaan yang dipesan dapat ditelusuri ke baris pemeriksaan yang memenuhinya; yang belum masuk wadah terbaca sebagai **menunggu wadah**, bukan hilang | BR-47, `LAB-DEC-057` |
| AC-92 | Pasien yang memilih Laboratorium di kiosk lalu pergi tanpa diperiksa: kunjungannya **ditutup otomatis saat hari layanan berakhir** dengan sebab "tidak dilanjutkan", dan **biaya pendaftarannya gugur** | BR-46, `LAB-DEC-058` |
| AC-93 | Sesi kiosk yang bertambah ruas tujuan layanan dan jalur permintaan **tidak mengubah satu pun perilaku sesi kiosk yang sudah ada**; 16 sesi yang sudah tersimpan tetap terbaca | BR-46, `LAB-REQ-006`; tafsir dicatat Amendment Pass putaran 20 (`KSK-DEC-002`/`014`) |
| AC-94 | Konfirmasi pesanan hanya sah **sekali**; sesudahnya nama konfirmator dan tanggal/waktu konfirmasi terekam dan terbaca pada daftar | BR-48, `LAB-DEC-061` |
| AC-95 | Konfirmasi **menolak** bila dokter pemeriksa belum dipilih, dan dokter yang dipilih tampil pada daftar serta ringkasan cetak | BR-48, `LAB-DEC-061` |
| AC-96 | Pembatalan pesanan **tanpa alasan ditolak**; alasan yang diterima tersimpan pada jejak audit dan terbaca kembali per pesanan | BR-49, `LAB-DEC-063` |
| AC-97 | Pembatalan **ditolak** ketika pesanan sudah `Accepted`, `InProcess`, `Completed`, atau `Cancelled`; **pesanan `Requested` dan `Confirmed` tetap dapat dibatalkan seperti sebelumnya** | BR-49, `LAB-DEC-063` |
| AC-98 | Penyaring tanggal Laboratorium **menolak tanggal masa depan**: hari sesudah hari ini tidak dapat dipilih pada `Tgl Awal` maupun `Tgl Akhir`, dan hari ini sendiri **tetap** dapat dipilih | BR-50, `LAB-DEC-064`, `LAB-DEC-074` |
| AC-99 | Rentang terbalik (`Tgl Awal` > `Tgl Akhir`) **ditolak beserta sebabnya** dan pencariannya tidak dijalankan; `Tgl Awal` **sama dengan** `Tgl Akhir` tetap sah, sehingga pencarian satu hari tetap mungkin | BR-50, `LAB-DEC-071`, `LAB-DEC-074` |
| AC-77 | Daftar pemeriksaan dan wadahnya terlihat berdampingan sebelum kelayakan ditetapkan | BR-40, BR-34 |
| **Penomoran melompat dari `AC-99` ke `AC-156`, dan itu disengaja.** | Roadmap backend dan frontend sudah terlanjur mengalokasikan `AC-100` sampai `AC-155` secara lokal tanpa mendaftarkannya di tabel ini — `BE-LAB-44` misalnya mencatat `AC-98`..`AC-100` **terbukti pada database**. Penomoran putaran 9-10 sempat ditulis mulai `AC-100` lalu **digeser +56** supaya tidak menimpa nomor yang sudah dipakai pekerjaan selesai. Nomor roadmap tidak diubah satu pun | — |
| AC-156 | Satu pesanan Mikrobiologi berisi dua pemeriksaan menghasilkan **dua tempat hasil yang terpisah**; mengisi hasil Kultur Darah tidak mengubah satu pun ruas pada Kultur Urin | BR-51 |
| AC-157 | `Waktu Efektif` dan `Waktu Issued` pada halaman hasil Mikrobiologi **tidak dapat diketik**; keduanya berubah hanya ketika `LabSpecimen.CollectedAt` atau `FinalizedAt` berubah | BR-52 |
| AC-158 | Menekan `Simpan Final` mengisi `FinalizedAt` dan `FinalizedByUserId`, dan hasil itu **tetap ditolak** ketika dicoba dikirim ke pasien, dengan sebab yang menyebut hasil belum dirilis | BR-53 |
| AC-159 | `Reopen` pada hasil yang sudah Final tetapi **belum dirilis** mengosongkan `FinalizedAt`, mengembalikan hasil ke keadaan dapat disunting, dan **tidak** tercatat sebagai koreksi hasil terrilis | BR-53 |
| AC-160 | Satu specimen dapat menunjuk **lebih dari satu** Spesifik Specimen, dan seluruh pilihannya berasal dari data induk — nilai yang diketik bebas ditolak | BR-54 |
| AC-161 | Petugas yang memakai jalan keluar `Lainnya` **tidak** membuat nilai tetap baru; pemakaiannya muncul pada daftar pantau kepala instalasi, dan nilai itu baru menjadi pilihan tetap sesudah kepala instalasi menaikkannya | BR-54 |
| AC-162 | Volume `2 swab` tersimpan sebagai angka `2` dengan satuan `swab`, dan dapat dijumlahkan bersama baris lain bersatuan sama | BR-55 |
| AC-163 | Baris kepekaan yang **hanya** berisi antibiotik dan interpretasi `S` — nol MIC, nol zona mm — tersimpan tanpa penolakan | BR-56 |
| AC-164 | Hasil kultur **tanpa satu pun isolat** tersimpan tanpa penolakan, dan sistem **tidak** meminta organisme diisi | BR-58 |
| AC-165 | Baris kepekaan yang berisi antibiotik tetapi **nol interpretasi** ditolak beserta sebabnya | BR-58 |
| AC-166 | Penanda kritis Mikrobiologi **tidak menyala** ketika data induk aturan kritis masih kosong, dan layar menyatakan keadaan itu secara terbaca — bukan diam | BR-57 |
| AC-167 | Hasil dengan interpretasi `R` yang **tidak** cocok dengan satu pun baris aturan kritis **tidak** menyalakan penanda kritis | BR-57 |
| AC-168 | Ruas `Analis` pada form hasil **tidak dapat dipilih**, dan nilainya sama persis dengan pengguna yang menekan simpan | BR-59 |
| AC-169 | Menyalakan `Definitif` menyimpan siapa, kepada siapa, dan kapan; hasilnya **tetap dapat disunting** sesudahnya, dan **tidak** membuka satu pun tombol pengiriman atau pembagian | BR-60 |
| AC-170 | Mengubah jenis specimen dari halaman hasil tersimpan beserta nilai lamanya, nama pengubah, dan waktunya; sesudah `Simpan Final`, ruas yang sama **tidak dapat diubah** | BR-61 |
| AC-171 | Kolom Dokter Konfirmator menawarkan DPJP pesanan dan dokter bertugas dari jadwal; **nol peran baru** muncul pada matriks kewenangan | BR-62 |
| AC-172 | Halaman hasil Mikrobiologi **tidak menampilkan** ruas HL7 dalam bentuk apa pun, termasuk pilihan kosong yang tidak dapat dipilih | BR-63 |
| AC-173 | Ketika `TrxOnCallAssignment` memuat penugasan aktif pada jam permintaan, pilihan Dokter Konfirmator menampilkan **hanya** dokter tersebut, beserta nomor WhatsApp dari `MstDoctor.WhatsAppNumber` | BR-65 |
| AC-174 | Ketika `TrxOnCallAssignment` **nol baris aktif**, pilihan Dokter Konfirmator tetap dapat dipakai: daftar dokter aktif yang dapat dicari muncul, **disertai keterangan** bahwa jadwal jaga belum tersedia | BR-65 |
| AC-175 | Mengubah jenis specimen dari halaman hasil menambah **satu baris** pada tabel jejak perubahan ruas berisi nilai lama dan nilai baru; `LabTransitionHistory` **tidak bertambah** satu baris pun | BR-66 |
| AC-176 | Daftar status temuan pada layar isolat Mikrobiologi menawarkan **tepat tiga** nilai — `Normal`, `Positif`, `Negatif` — dan **tidak** menawarkan `NeedsAttention` maupun `Critical` | BR-67 |
| AC-177 | Cetakan Mikrobiologi menampilkan baris `HASIL YANG DIPEROLEH` berisi nilai kualifikasi yang tersimpan; nilai itu **tidak** disimpulkan dari ada tidaknya catatan konsultasi | BR-68 |
| AC-178 | Menyimpan kadar `1,25` tanpa memilih satuan **ditolak beserta sebabnya**; nilai bersatuan `mg/L` dan `ug/mL` tersimpan berdampingan tanpa saling menimpa | BR-69 |
| AC-179 | Pemeriksaan bertanda biakan jamur mencetak `HASIL BIAKAN JAMUR` dan `HASIL RESISTENSI ANTIJAMUR`; pemeriksaan bertanda biakan bakteri mencetak `BIAKAN BAKTERI` dan `RESISTENSI ANTIBIOTIKA` | BR-70 |
| AC-180 | Satu pesanan memiliki **dua nomor yang berbeda dan keduanya terbaca**: nomor cetak per disiplin per tahun pada kolom `No. Lab`, dan `OrderNumber` sebagai sumber barcode | BR-71 |
| AC-181 | `Tanggal Terima` pada cetakan sama dengan waktu penerimaan fisik specimen, dan **berbeda** dari `Waktu Efektif` pada layar ketika bahan diambil pada hari yang lain | BR-72 |
| AC-182 | Mengubah nama konsultan pada pengaturan mengubah footer seluruh cetakan disiplin itu, dan **tidak** mengubah pemegang wewenang klinis yang terdaftar | BR-73 |
| AC-183 | Selama hasil belum dirilis, kolom `Petugas Otorisasi` pada cetakan **kosong** — dan **tidak** terisi nama pencetak maupun nama penulis hasil | BR-74 |
| AC-184 | `LAB-OPEN-029` tetap berstatus terbuka sesudah `LAB-DEC-121`, dan permintaan kepada wewenang klinis tercatat beserta lampiran buktinya | BR-75 |
| AC-185 | Kolom `UG` dan rentang `R-S` pada baris kepekaan **tidak dapat diketik**; keduanya terisi dari data induk dan **tetap seperti semula** setelah data induknya diubah | BR-76 |
| AC-186 | Zona `13` pada rentang `12 - 15` menghasilkan `I`; zona `11` pada rentang `12 - 16` menghasilkan `R`; zona `32` pada rentang `13 - 16` menghasilkan `S` — ketiganya tanpa satu pun ketikan analis | BR-77 |
| AC-187 | Menimpa interpretasi hasil hitungan **ditolak** bila alasannya kosong; hasil timpaan tersimpan beserta nilai hitungan aslinya | BR-77 |
| AC-188 | Pemeriksaan bertanda difusi cakram menampilkan kolom `UG`, rentang, dan zona; bertanda dilusi menampilkan MIC beserta satuannya — dan **penanda jenis biakan tidak memaksa salah satunya** | BR-78 |
| AC-189 | Pemeriksaan yang **tidak** terpetakan memakai set bakteri **nol menampilkan** bagian isolat maupun antibiogram | BR-79 |
| AC-190 | Isolat tanpa satu pun baris kepekaan **tersimpan tanpa penolakan** dan tetap terhitung pada laporan pola kuman | BR-80 |
| AC-191 | Zona `0` tersimpan sebagai angka dan menghasilkan `R`; ruas zona yang **dikosongkan** tersimpan sebagai belum diukur dan **tidak** menghasilkan interpretasi apa pun | BR-82 |
| AC-192 | Memilih Specimen `Cairan Tubuh` menampilkan pilihan Spesifik Specimen **hanya** dari 175 baris kelompok itu; `subjenis_specimen` ikut tersimpan **tanpa** pernah dipilih petugas | BR-83 |
| AC-193 | Sesudah impor, **1.601** baris dapat dipilih dan **166** baris tidak — dan yang 166 itu **tetap ada** serta dapat diaktifkan kepala instalasi | BR-84 |
| AC-194 | Baris yang nama Indonesianya kosong tampil dengan nama Inggrisnya, **dapat ditemukan lewat pencarian kedua bahasa**, dan muncul pada daftar yang belum diterjemahkan | BR-85 |
| AC-195 | Baris hasil impor membawa kode SNOMED CT-nya; baris yang ditambahkan lewat `Lainnya` tersimpan dengan kode SNOMED **kosong** dan tetap sah | BR-86 |
| AC-196 | Hasil Patologi Klinik yang masih Draft **tidak muncul** pada antrean validasi, dan percobaan memvalidasinya **ditolak** | BR-88 |
| AC-197 | Analis dapat membuka kembali (Reopen) hasil Patologi Klinik yang sudah Final **selama belum divalidasi**; `FinalizedAt` kosong kembali, dan riwayat mencatat siapa serta kapan membukanya | BR-88 |
| AC-198 | Pada satu order berisi Kalium cito dan Hemoglobin, Kalium dapat Final, divalidasi, dan dirilis **tanpa menunggu** Hemoglobin; order tetap berlabel *Dalam Pemeriksaan* sampai Hemoglobin juga dirilis | BR-88, BR-06 |
| AC-199 | Order berlabel *Selesai* hanya bila seluruh pemeriksaan yang tidak batal sudah dirilis; pemeriksaan yang dibatalkan atau gugur **tidak menahan** label itu, dan **nol kolom status order baru** tersimpan | BR-88 |
| AC-200 | Sesudah WhatsApp hasil kritis **berhasil terkirim**, hasil **tetap** berada pada daftar pantau *nilai kritis belum dilaporkan* sampai kelima isian `LAB-DEC-004`, termasuk bukti pembacaan ulang, tercatat | BR-89 |
| AC-201 | Waktu kirim WhatsApp dan waktu dilaporkan tersimpan sebagai **dua nilai berbeda**; pengiriman WhatsApp yang **gagal** tercatat sebagai gagal dan tidak mengubah kewajiban pelaporan | BR-89 |
| AC-202 | Pelaporan hasil kritis dapat **dituntaskan lewat telepon** tanpa satu pun pengiriman WhatsApp, termasuk ketika gerbang WhatsApp belum tersedia | BR-89 |
| AC-203 | Isi pesan WhatsApp hasil kritis yang diserahkan ke gerbang **tidak memuat** nama pasien, No. RM, NIK, nama pemeriksaan, maupun nilai hasil — hanya penanda adanya hasil kritis, unit/ruang, waktu, dan nomor kontak Laboratorium | BR-90 |
| AC-204 | Pembentuk pesan WhatsApp kritis **tidak menyediakan** ruas pengganti untuk data pasien atau data klinis, sehingga mengubah teks pesan pun tidak dapat memasukkannya | BR-90 |
| AC-205 | Pemegang kewenangan validasi atau rilis dapat mengembalikan hasil yang **sudah divalidasi tetapi belum dirilis** menjadi Draft; tanpa alasan dari daftar terkendali, pengembalian **ditolak** | BR-91 |
| AC-206 | Sesudah pengembalian, riwayat hasil **tetap memuat** siapa yang pernah memvalidasi, kapan, serta alasan pengembaliannya; **nol versi bernomor** terbit dan **nol pemberitahuan** dikirim ke dokter pemesan | BR-91 |
| AC-207 | Tombol *Kembalikan ke analis* **ditolak** pada hasil yang sudah dirilis; hasil semacam itu hanya dapat diubah lewat koreksi | BR-91 |
| AC-208 | Pada order berlabel Selesai **tanpa** persetujuan `LAB-DEC-067`, kirim WhatsApp, cetak dokumen final untuk pasien, dan unduh **ketiganya ditolak**, dan layar menyebut syarat yang belum terpenuhi | BR-92 |
| AC-209 | Nota Lab, Label Lab, dan Label Golongan Darah **tetap dapat dicetak** pada order yang gerbangnya masih tertutup | BR-92 |
| AC-210 | Memanggil langsung jalur backend cetak, unduh, atau kirim untuk order yang gerbangnya tertutup **ditolak** — gerbang tidak hanya ditegakkan dengan menonaktifkan tombol di layar | BR-92 |
| AC-211 | Memindai QR dokumen final membuka halaman yang menampilkan nama rumah sakit, nomor cetak, tanggal rilis, dan status dokumen — **tanpa** nilai hasil, nama pemeriksaan, nama pasien, No. RM, maupun NIK | BR-93 |
| AC-212 | QR dokumen yang kemudian dikoreksi menampilkan status **sudah digantikan** beserta nomor dan tanggal versi penggantinya; QR versi pengganti menampilkan **asli dan berlaku** | BR-93 |
| AC-213 | Token QR **tidak sama dan tidak dapat diturunkan** dari nomor order maupun nomor cetak; token yang diubah satu karakter menghasilkan status **tidak dikenal** | BR-93 |
| AC-214 | Hasil Patologi Klinik **dapat Final tanpa** catatan konsultasi; bila konsultasi dicatat, siapa, kepada siapa, dan kapan tersimpan dan tampil; pilihan kualifikasi `Definitif` **tidak tersedia** pada Patologi Klinik | BR-94 |
| AC-215 | Pengguna berjabatan calon tetapi **belum ditunjuk** yang menekan Validasi **ditolak**, dengan pesan yang menyebut penunjukannya belum ada | BR-95 |
| AC-216 | Pengguna yang tercantum pada daftar penunjukan tetapi jabatannya **bukan** jabatan calon juga **ditolak** — kedua lapis wajib lolos | BR-95 |
| AC-217 | Penunjukan validasi **tidak** memberi kewenangan rilis, dan penunjukan rilis **tidak** memberi kewenangan validasi | BR-95 |
| AC-218 | Pengguna yang ditunjuk memvalidasi **Patologi Klinik** **ditolak** saat memvalidasi hasil **Mikrobiologi**, dan pesan penolakannya menyebut disiplin yang belum ditunjuk | BR-96 |
| AC-219 | Penanda hasil rendah, tinggi, dan kritis pada layar **dan** pada cetakan memuat huruf atau teks `L`, `H`, `KRITIS`; cetakan hitam-putih tetap membedakan ketiganya | `LAB-FE-015` |
| AC-220 | Isian nilai hasil, isolat, dan antibiogram pada halaman hasil Patologi Klinik dan Mikrobiologi **tidak** berada di jendela modal | `LAB-FE-016` |
| AC-221 | Pengguna yang hanya memegang `LabExamination : Update` — misalnya dokter pemesan untuk menandai cito — **ditolak `403`** saat mengisi hasil Patologi Klinik atau Mikrobiologi, menekan Final, Reopen, atau mencatat konsultasi | BR-97 |
| AC-222 | Pengguna yang memegang izin hasil **tanpa** `LabExamination : Update` dapat mengisi hasil, tetapi **ditolak** saat membatalkan pemeriksaan atau menandai cito dan duplo | BR-97 |
| AC-223 | Sesudah perubahan dirilis, analis yang sebelumnya dapat mengisi hasil **tetap dapat** mengisinya tanpa campur tangan admin — data kebijakan izin hasil bagi jabatan analis terpasang dalam rilis yang sama | BR-97 |
| AC-224 | Tindakan laporan Patologi Anatomi — isi, Final, Reopen — **tetap berjalan** dengan izinnya sekarang dan tidak terdampak perubahan ini | BR-97 |
| AC-225 | Menyimpan hasil Patologi Klinik atau Mikrobiologi yang sudah Final **ditolak `409`** dengan pesan yang menyuruh membuka kembali lebih dulu, dan isi yang tersimpan **tidak berubah** | BR-98 |
| AC-226 | Mencatat konsultasi pada hasil yang sudah Final **ditolak `409`** | BR-98 |
| AC-227 | Sesudah Reopen, penyimpanan **diterima kembali**; `ReopenCount` naik satu, riwayat memuat alasannya, dan Final berikutnya menggantikan Tanggal Selesai serta Waktu Issued | BR-98 |
| AC-228 | Halaman hasil Mikrobiologi menampilkan penolakan `409` sebagai pesan yang terbaca, dan isian yang sedang diketik **tidak hilang** | BR-98 |
| AC-229 | Pengguna yang memegang kode kewenangan *validasi Patologi Klinik* berstatus aktif dalam masa berlakunya — dan lolos lapis jabatan — **dapat** memvalidasi hasil Patologi Klinik | BR-99 |
| AC-230 | Kewenangan berstatus *suspended*, *revoked*, kedaluwarsa, atau di luar masa berlaku **menolak** validasi dan rilis seketika, tanpa satu pun perubahan pada data Laboratorium | BR-99 |
| AC-231 | Kode *validasi Patologi Klinik* **tidak** memberi validasi Mikrobiologi, dan kode *validasi* **tidak** memberi *rilis* | BR-99, BR-96 |
| AC-232 | Laboratorium **nol menulis** ke tabel kredensial Human Resource; seluruh aksesnya baca-saja | BR-99 |
| AC-233 | Pesan penolakan validasi atau rilis menyebut sebabnya — **belum ditunjuk**, **masa berlaku habis**, atau **sedang ditangguhkan** | BR-99 |
| AC-234 | Membuka satu order Patologi Klinik menampilkan **seluruh** pemeriksaan yang tidak batal dalam **satu tabel isian** berkolom parameter, hasil, satuan, nilai rujukan, dan penanda `L`/`H` | BR-100 |
| AC-235 | Daftar Kerja **membuka halaman detail order**; tidak ada satu pun dialog modal untuk mengisi hasil Patologi Klinik | BR-100, `LAB-FE-017` |
| AC-236 | Final pada satu pemeriksaan di halaman itu **tidak mengunci** pemeriksaan lain pada order yang sama — Kalium cito dapat Final lebih dulu sementara Hematologi masih diisi | BR-100, BR-88 |
| AC-237 | Menyimpan atau Final pada satu baris **tidak menghapus** isian baris lain yang belum disimpan | BR-100 |
| AC-238 | Validasi hasil Patologi Klinik oleh pengguna berjabatan analis — termasuk analis senior — **ditolak**, walaupun namanya pernah ditunjuk; validasi oleh dokter berkewenangan laboratorium yang ditunjuk **diterima** | BR-101 |
| AC-239 | Setiap validasi menyimpan nama validator, **snapshot peran/jabatannya saat itu**, dan waktu validasi; mengubah jabatan orang itu kemudian **tidak** mengubah snapshot pada validasi lama | BR-101 |
| AC-240 | Balasan WhatsApp dokter dapat dicatat sebagai konfirmasi komunikasi, tetapi hasil **tetap** berada pada daftar pantau *nilai kritis belum dilaporkan* sampai bukti baca ulang dari percakapan langsung tercatat | BR-102 |
| AC-241 | Pemegang validasi Patologi Klinik yang **tidak** ditunjuk validasi Mikrobiologi **ditolak** saat memvalidasi hasil Mikrobiologi — sekalipun ia dokter dan bertugas sendirian pada malam itu; pemegang validasi Mikrobiologi yang ditunjuk **diterima** | BR-103 |
| AC-242 | Pejabat **non-dokter** yang memegang kode rilis Patologi Klinik dan jabatannya memegang aksi rilis **dapat** merilis hasil yang divalidasi dokter lain; pejabat yang sama **tanpa** kode rilis **ditolak** | BR-104 |
| AC-243 | `PUT /lab-orders/{id}/complete` pada order `InProcess` yang masih memuat satu pemeriksaan tidak batal yang **belum dirilis** **ditolak `409`**; order tetap `InProcess`; respons menyebut **setiap** pemeriksaan yang menahan, dengan nama dan label keadaannya | BR-105 |
| AC-244 | Order yang seluruh pemeriksaan tidak batalnya sudah dirilis **dapat** diselesaikan, dan pemeriksaan batal atau gugur **tidak menahannya**; nol order `Completed` yang masih memuat pemeriksaan tidak batal yang belum dirilis | BR-105 |
| AC-245 | Order yang memuat pemeriksaan Patologi Anatomi, pemeriksaan Mikrobiologi yang belum dapat divalidasi, atau hasil `Sementara` **tidak dapat** diselesaikan; rinciannya menyebut pemeriksaan itu | BR-105 |
| AC-246 | Sebelum `ReleasedAt` terisi, hasil **tidak** tersedia sebagai hasil resmi: nol dokumen rekam medis, nol pengiriman kepada pasien, nol cetak hasil final | BR-106 |
| AC-247 | Layar hasil Patologi Klinik dan Mikrobiologi menampilkan keadaan pemeriksaan sebagai *Menunggu Hasil*, *Draft*, *Menunggu Validasi*, *Tervalidasi*, atau *Dirilis* sesuai `resultStatus`, dan tombol Final bertuliskan *Pemeriksaan Selesai* | BR-107 |
| AC-248 | PDF hasil eksternal yang ditautkan dari layar Laboratorium tersimpan sebagai **satu** dokumen klinis pasien berjenis `LaboratoryResult` dan bersumber `ExternalHospital`, tertaut pasien dan kunjungan; **nol** salinan di data Laboratorium, dan dokumen yang sama terbaca dari layar Clinical Management | BR-108, BR-109 |
| AC-249 | Penautan **ditolak** bila pengunggah belum mengonfirmasi kecocokan dua identitas pasien; berkas salah pasien dapat ditandai *salah input* beralasan oleh pengunggah atau kepala instalasi, **tidak terhapus**, dan alasannya tetap terbaca | BR-109 |
| AC-250 | Laporan jumlah pemeriksaan menghitung pemeriksaan pada **tanggal rilisnya**; pemeriksaan dipesan 30 September dan dirilis 1 Oktober masuk hitungan Oktober; pemeriksaan batal tidak dihitung | BR-110 |
| AC-251 | Angka penolakan = wadah tidak layak ÷ wadah yang diputuskan kelayakannya pada periode itu, menurut waktu keputusan, dirinci per disiplin dan per alasan; 12 ditolak dari 400 diputuskan terbaca **3,0%** | BR-110 |
| AC-252 | TAT dihitung dari wadah dinyatakan layak sampai hasil dirilis, cito dan rutin terpisah; pemeriksaan cito yang terlambat menurut laporan **juga** muncul pada daftar pantau keterlambatan (`AC-17`), dan sebaliknya | BR-110 |
| AC-253 | Pengguna yang memegang hak baca daftar Laboratorium **tanpa** izin laporan **ditolak** membuka laporan `S16a` | BR-111 |
| AC-254 | Lokasi, pola, dan metode pengambilan tersimpan per wadah, dapat diubah pengisi hasil sampai Final dengan jejak nilai lama dan baru, dan **baca-saja** sesudah Final | BR-112 |
| AC-255 | Memilih `Lainnya` pada lokasi atau metode **wajib** disertai keterangan, dan **tidak** menambah daftar; nilai baru hanya muncul sesudah kepala instalasi menambahkannya | BR-112 |
| AC-256 | Final **ditolak** bila ruas pengambilan yang disetel wajib bagi kategori pemeriksaan itu masih kosong, dan pesannya menyebut ruasnya; ruas yang tidak disetel wajib tidak menahan Final | BR-112 |
| AC-257 | Satu wadah PA menyimpan tepat **satu** lokasi; menyimpan dua lokasi pada satu wadah **ditolak**. Order `LAB-001` dengan Lambung, Duodenum, dan Ileum memiliki tiga wadah bernomor sendiri, dan cetakan PA menomori ketiga lokasi dari wadahnya masing-masing | BR-116 |
| AC-258 | Wadah yang pemeriksaannya disetel membutuhkan fiksasi menyimpan jenis fiksasi dari data induk, tanggal dan jam mulai, dan petugas — atau `Tidak Memerlukan Fiksasi`; jenis fiksasi di luar data induk aktif **ditolak** | BR-117 |
| AC-259 | Nomor cetak Sitologi dan FNAB dialokasikan dari seri masing-masing: biopsi dan FNAB pada hari yang sama memperoleh nomor dari dua seri berbeda, sedangkan nomor order keduanya tetap berformat global. Awalan, pemisah, dan panjang dibaca dari setelan; mengubah setelan tidak menuntut perubahan kode | BR-118 |
| AC-260 | Hasil Mikrobiologi berkualifikasi `Sementara` dapat divalidasi dan dirilis, serta tampil dan tercetak `HASIL SEMENTARA`. Rilis tahap `Definitif` sesudahnya **tidak** meminta alasan maupun persetujuan koreksi, **tidak** menandai versi `Sementara` *sudah diperbaiki*, dan kedua versi terbaca pada riwayat | BR-119 |
| AC-261 | Order yang memuat pemeriksaan Mikrobiologi terrilis `Sementara` **ditolak** berpindah ke `Completed` dan gerbang penyerahan ke pasien tetap tertutup; sesudah rilis `Definitif` atau rilis tanpa kualifikasi, order dapat `Completed` | BR-120 |
| AC-262 | Rilis `Definitif` atas pemeriksaan yang `Sementara`-nya sudah dirilis membuat **tepat satu** pemberitahuan tersimpan bagi dokter pemesan; rilis `Definitif` tanpa `Sementara` terrilis sebelumnya membuat **nol** pemberitahuan | BR-121 |
| AC-263 | Pada laporan PA, penulis dapat memvalidasi laporannya sendiri **tanpa** penanda pengecualian; rilis oleh penulis yang sama **ditolak**; rilis oleh Sp.PA lain pemegang kewenangan rilis PA berhasil; analis **ditolak** memvalidasi maupun merilis PA. Pada Patologi Klinik dan Mikrobiologi, pengisi tetap **ditolak** memvalidasi hasilnya sendiri | BR-122 |
| AC-264 | Usulan perubahan batas kritis **tidak** dipakai menilai hasil sebelum disetujui dan sebelum tanggal mulai berlakunya; persetujuan oleh pengusulnya sendiri **ditolak**; persetujuan oleh dokter tanpa delegasi tercatat **ditolak**; riwayat menyimpan nilai lama, nilai baru, alasan, pengusul, penyetuju, waktu persetujuan, tanggal mulai berlaku, dan nomor versi | BR-123 |
| AC-265 | Hasil yang memenuhi aturan kritis berstatus kritis dan membuat pemberitahuan tersimpan bagi dokter pemesan serta — bila episode perawatan masih aktif — DPJP/unit pelayanan; setiap percobaan kirim menyimpan penerima, waktu, saluran, dan hasilnya | BR-124 |
| AC-266 | Bila batas waktu `Critical Notification SLA` disiplin itu lewat tanpa laporan baca ulang, sistem mencatat eskalasi ke tingkat berikutnya pada konfigurasi beserta hasilnya; mengubah batas waktu SLA tidak menuntut rilis aplikasi | BR-124 |
| AC-267 | Status *sudah dibaca* pada kotak pemberitahuan dan balasan WhatsApp tercatat sebagai bukti komunikasi tetapi **tidak** menghentikan eskalasi; eskalasi berhenti tepat saat laporan baca ulang `LAB-DEC-004` tercatat lengkap | BR-125 |
| AC-268 | Pengajuan koreksi tanpa alasan dari daftar terkendali **ditolak**; persetujuan oleh pengajunya sendiri atau oleh pengguna tanpa kewenangan rilis disiplin itu **ditolak**; sesudah disetujui, versi lama `Superseded` bertanda *sudah diperbaiki*, versi koreksi aktif, dan dokter pemesan serta DPJP/unit aktif menerima pemberitahuan. Hasil yang dirilis berbulan-bulan lalu tetap dapat dikoreksi | BR-126 |
| AC-269 | Pembatalan pemeriksaan yang hasilnya sudah dirilis **ditolak**; koreksi jenis *tarik hasil* yang disetujui membuat hasil itu `Invalidated` — tidak lagi aktif, tetapi tetap terbaca pada riwayat beserta alasannya | BR-127 |
| AC-270 | Koreksi atau tarik hasil atas hasil yang pernah dikirim ke pasien membuat dokumen lama `Superseded`/`Invalidated` dan mencabut token QR-nya; koreksi membuat **tepat satu** tugas kirim `Pending` bagi pasien itu, dan tugas itu **tidak** terkirim tanpa tindakan petugas lewat gerbang `S17` | BR-128 |
| AC-271 | Order Selesai berisi hasil normal dapat dikirim ke pasien **tanpa** persetujuan tambahan; hasil dari kategori yang disetel sensitif **ditolak** dikirim sebelum pemegang izin `LabResultPatientDeliveryApprover` menyetujuinya; nol pemeriksaan gelar pada jalur mana pun | BR-130 |
| AC-272 | Kegagalan WhatsApp mengubah status kirim menjadi `Failed` tetapi **tidak** mengubah status Released; pesan berisi tautan aman, bukan URL berkas publik; PDF memuat nomor dokumen, versi, waktu terbit, perilis, dan QR | BR-131 |
| AC-273 | Isi QR tidak memuat Patient ID, nomor order berurutan, maupun URL berkas; halaman verifikasi publik tidak menyediakan pencarian dan tidak menampilkan nama pasien maupun nama pemeriksaan; token yang dicabut menampilkan status *tidak berlaku* atau *digantikan* | BR-132 |
| AC-274 | Berkas klinis tidak dapat dibuka lewat URL statis web server; akses hanya lewat endpoint terautentikasi atau tautan bertanda tangan yang kedaluwarsa, dan setiap akses tercatat; jenis dan ukuran di luar batas **ditolak**; mengganti berkas membuat berkas lama `Superseded`, tidak terhapus | BR-133 |
| AC-275 | `Verified` dan `Approved` tersimpan sebagai dua tindakan terpisah dengan pelaku dan waktu masing-masing; petugas lab dapat `Verified` tetapi **ditolak** `Approved`; `Approved` sebelum `Verified` **ditolak** | BR-134 |
| AC-276 | Ketiga daftar pasien lab menampilkan aksi *Proses Pemeriksaan*; aksi dapat ditekan **hanya** pada pesanan berstatus `Accepted`; satu tekanan yang dikonfirmasi mengirim **tepat satu** `PUT /lab-orders/{id}/start-process`, dan status baris menjadi *Sedang Dikerjakan* hanya sesudah jawaban `200` | BR-136 butir 1, 4 |
| AC-277 | Pada pesanan `Accepted` berpenjamin Tunai, Asuransi, maupun Perusahaan, aksi *Proses Pemeriksaan* sama-sama dapat ditekan; layar tidak membaca status lunas untuk mengunci aksi ini | BR-136 butir 2 |
| AC-278 | *Bunyi diamandemen 2026-10-07 oleh `LAB-DEC-198`:* Aksi *Selesaikan* tampil pada daftar Patologi Klinik dan Mikrobiologi dan **tidak** tampil pada daftar Patologi Anatomi; aksi dapat ditekan hanya bila status `InProcess` **dan** `resultProgress` = `AllReleased` **atau kosong** (nol pemeriksaan terhitung) — pesanan `InProcess` dengan satu saja pemeriksaan belum dirilis menampilkannya **nonaktif** | BR-136 butir 3; `LAB-DEC-198` |
| AC-279 | Penolakan `409` dari `complete` menampilkan pemeriksaan yang menahan; status baris tidak berubah | BR-136 butir 4 |
| AC-280 | Aksi *Terima Sampling* membuka layar Wadah pesanan baris itu; daftar mengirim **nol** permintaan ke `/lab-specimens/**` saat aksi ditekan | BR-136 butir 5 |
| AC-281 | Nol aksi *Tahan* maupun *Lanjutkan* pada ketiga daftar pasien lab | BR-136 butir 6 |
| AC-282 | Menu Laboratorium tampil dengan urutan dan label BR-137 butir 1; seluruh `pathname` dan `key` sama dengan sebelum perubahan; Laporan Operasional tetap tersembunyi bagi pengguna tanpa `LabOperationalReport : Read`; judul halaman Beranda dan ketiga daftar sama dengan label menunya | BR-137 |
| AC-283 | Konfirmasi pada pesanan `Accepted` yang belum pernah dikonfirmasi berhasil; status **tetap** `Accepted`; konfirmator, waktu konfirmasi, dan dokter pemeriksa terisi dan tampil di kolom Konfirmasi | BR-138 butir 1–2 |
| AC-284 | Konfirmasi kedua pada pesanan yang sudah pernah dikonfirmasi ditolak `409`; Konfirmasi pada `InProcess`, `Completed`, `Cancelled`, atau `OnHold` ditolak `409`; Konfirmasi pada `Requested` tetap memindahkan ke `Confirmed` | BR-138 butir 1–2 |
| AC-285 | `start-process` pada `Accepted` yang belum dikonfirmasi ditolak dengan pesan yang menyebut konfirmasi; pada `Accepted` yang sudah dikonfirmasi — baik dikonfirmasi sebelum maupun sesudah wadah layak — berhasil | BR-138 butir 3 |
| AC-286 | Selesaikan pada pesanan `InProcess` tanpa konfirmasi yang seluruh pemeriksaannya dirilis berhasil — syarat konfirmasi tidak diperiksa | BR-138 butir 4 |
| AC-287 | Di ketiga daftar pasien lab: Konfirmasi dapat ditekan pada `Requested` dan `Accepted` yang belum dikonfirmasi; Proses Pemeriksaan redup beralasan pada `Accepted` yang belum dikonfirmasi; status baris tidak berubah sebelum jawaban `200` | BR-138; `LAB-FE-033` |
| AC-288 | Pengguna yang memegang hak Konfirmasi tanpa `LabOrder : Update` dapat mengonfirmasi, tetapi tidak dapat membatalkan pesanan maupun menulis konteks klinis; pengguna yang hanya memegang `LabOrder : Update` tidak lagi dapat mengonfirmasi | BR-138 butir 5 |
| AC-289 | Pengguna yang memegang `LabOrder : Confirm` tanpa izin master dokter HR dapat memuat dan mencari daftar dokter pemeriksa di dialog Konfirmasi, lalu menyimpan Konfirmasi; daftar hanya memuat dokter aktif | BR-139 butir 1–2 |
| AC-290 | Pengguna tanpa `LabOrder : Confirm` ditolak `403` saat memuat daftar dokter pemeriksa; setiap butir daftar hanya memuat nama, kode, dan spesialisasi — tanpa nomor telepon, alamat, atau data pribadi lain | BR-139 butir 2–3 |
| AC-291 | Dialog Konfirmasi dibuka dengan pilihan dokter pemeriksa kosong; Simpan tetap tidak dapat dilakukan sampai dokter dipilih | BR-139 butir 4; `LAB-FE-034` |
| AC-292 | Sesudah Konfirmasi `200`, jawaban `confirm` dan `GET /lab-orders/{id}` membawa kelima ruas jejak konfirmasi terisi; di daftar pasien lab baris *Diterima* langsung menampilkan kolom Konfirmasi dan Proses Pemeriksaan dapat ditekan tanpa muat ulang | BR-139 butir 5 |
| AC-293 | Pada setiap lingkungan yang menjalankan penjaga `VAL-151`, pemegang `LabOrder : Confirm` dapat memuat daftar dokter pemeriksa — dibuktikan pada langkah rilis `MVP-12c` | BR-139 butir 6 |
| AC-294 | Beranda Lab menampilkan, berurutan: hero, empat kartu hari ini, empat kartu fokus operasional, grafik pemeriksaan per disiplin, sebaran, tren bulanan, pesanan terbaru, kesiapan operasional, lalu rekap status per rentang tanggal; memakai komponen dan tema V2 | BR-140 butir 1; `LAB-FE-035` |
| AC-295 | Dengan 12 pesanan diminta hari ini WIB (3 *Selesai*, 1 *Dibatalkan*, 2 CITO), kartu berbunyi 12 / Menunggu 8 / Selesai 3 (25%) / CITO 2; pesanan yang diminta 23.30 WIB kemarin tidak terhitung | BR-140 butir 2 |
| AC-296 | Kartu *Hasil menunggu validasi* menghitung pemeriksaan berhasil final belum divalidasi; Beranda nol memanggil data pengguna/SDM | BR-140 butir 3 |
| AC-297 | Grafik per disiplin dan sebaran menghitung pemeriksaan tidak batal pada pesanan tahun terpilih; mengganti tahun mengubah ketiga grafik | BR-140 butir 4 |
| AC-298 | Tren bulanan menampilkan 12 bulan tahun terpilih dan tahun sebelumnya; pesanan *Dibatalkan* dan *Draft* tidak terhitung | BR-140 butir 5 |
| AC-299 | Tabel pesanan terbaru berisi paling banyak 10 baris urut waktu diminta terbaru; klik baris membuka halaman pesanan | BR-140 butir 6 |
| AC-300 | Beranda menampilkan waktu data terakhir dimuat; *Perbarui Data* memuat ulang seluruh bagian; galat satu bagian tidak mengosongkan bagian lain | BR-140 butir 7 |
| AC-301 | Pengguna tanpa `LabOrder : Read` melihat *Akses Ditolak*; data ringkasan menjawab `403` | BR-140 butir 8 |
| AC-302 | Pasien dengan pesanan Lab aktif yang memilih Laboratorium di kiosk melihat *Konfirmasi Kehadiran* berisi tanggal, asal poli, dokter, dan daftar pemeriksaan; menekan *Konfirmasi Kehadiran* hanya menghasilkan satu sesi kiosk bertujuan Laboratorium dengan penanda membawa permintaan dokter — **nol** kunjungan baru | BR-141 |
| AC-303 | Pasien tanpa pesanan Lab aktif melewati Jenis Kunjungan, Pembayaran, Konfirmasi, dan Cetak Antrean; kunjungan terbentuk pada unit Laboratorium **tanpa** dokter; nol kode Laboratorium menulis kunjungan (`AC-45`) | BR-141 |
| AC-304 | Bila pembacaan pesanan Lab di kiosk gagal, pasien diarahkan ke jalur pendaftaran, bukan tertahan; bila unit Laboratorium tidak ditemukan, kiosk berhenti di Tujuan Layanan dengan pesan meminta bantuan petugas | BR-141 |
| AC-305 | Kunjungan kiosk unit Laboratorium tanpa pesanan tampil di Daftar Pasien OTC pada tab menurut penanda rujukannya; sesudah pesanan pertamanya terbit, kunjungan itu **hilang** dari daftar | BR-141 |
| AC-306 | Satu Simpan Penerimaan membentuk kunjungan (bila belum ada), tepat satu pesanan untuk Kategori Lab terpilih, dan wadah berstatus `Received`; Simpan ulang sesudah gagal di langkah wadah **tidak** membentuk kunjungan maupun pesanan kedua; pemeriksaan bertanda CITO terkirim sebagai CITO | BR-141 |
| AC-307 | Pada kunjungan baru, petugas memilih Tunai atau Asuransi (Asuransi mewajibkan kartu asuransi pasien); pada kunjungan kiosk/OTC metode pembayaran tampil baca-saja tanpa kotak pilihan | BR-141, `LAB-DEC-221` |
| AC-308 | Terima Sampling pada wadah `Planned`/`Collected` mencatat diambil lalu diterima; pada pesanan dokter tanpa wadah, dialog mewajibkan jenis specimen dan merencanakan satu wadah untuk seluruh pemeriksaan yang masih dipesan | BR-141 |
| AC-309 | Wadah `Received` yang diambil pengguna sendiri → dialog menampilkan larangan empat mata dan **nol** permintaan terkirim; petugas lain dapat menyatakan Layak, atau Tidak Layak dengan alasan wajib dari daftar terkendali | BR-141, `VAL-09` |
| AC-310 | Pasien Tunai dengan sisa tagihan Rp 35.000 → Proses Pemeriksaan nonaktif dengan petunjuk belum lunas, dan `PUT start-process` langsung dijawab `409` *"… belum lunas (sisa Rp 35.000)"*; pasien asuransi/penjamin perusahaan tidak dikunci; sebelum wadah layak petunjuknya *tagihan belum terbit* | BR-141 |
| AC-311 | Kolom Pembayaran Daftar Pasien Lab dan Hasil dan Riwayat berbunyi *Lunas*, *Belum Lunas* beserta sisanya, *Asuransi / Penjamin*, atau *Belum Ditagih* | BR-141 |
| AC-312 | Centang *Langsung proses* pada dialog Konfirmasi nonaktif selama wadah belum layak atau pembayaran belum lolos; centang *Sampling diterima* tidak pernah menetapkan kelayakan; tombol Batal dialog selalu dapat ditekan | BR-141 |
| AC-313 | Tiga menu Hasil dan Riwayat tampil sesudah ketiga Daftar Pasien Lab dengan aksi Lihat Hasil, Nota Lab, Label Lab, Label Goldar; *Kirim Hasil ke Pasien* nonaktif beralasan; hasil resmi tetap hanya sesudah rilis (`AC-246`) | BR-141 |
| AC-314 | Pesanan Lab pasien **rawat inap** Tunai dengan tagihan Lab belum dilunasi → kolom Pembayaran *Ditagih Kemudian* beserta sisanya, Proses Pemeriksaan aktif, dan `PUT start-process` menjawab `200` |
| AC-315 | Pesanan Lab pasien **IGD** Tunai yang tagihannya tidak pernah terbit → kolom Pembayaran *Ditagih Kemudian* tanpa nominal, Proses Pemeriksaan aktif, dan `start-process` menjawab `200` (bukan `409` `VAL-153`) |
| AC-316 | Pesanan Lab kunjungan **MCU, Telemedicine, atau `Unknown`** Tunai yang belum lunas → tetap ditolak `409` (`VAL-152`/`VAL-153`) persis seperti rawat jalan |
| AC-317 | Pesanan Lab pasien rawat inap atau IGD **berpenjamin** → kolom Pembayaran tetap *Asuransi / Penjamin*, bukan *Ditagih Kemudian* |
| AC-318 | Dokter memesan Hemoglobin **CITO** dan Leukosit biasa; analis merencanakan wadah → pemeriksaan Hemoglobin `Cito` dengan `urgencyMarkedAt` kosong, Leukosit `Routine`. Daftar Pasien Lab berlencana CITO dan memuat baris itu pada saringan *Hanya cito*; label tabung Hemoglobin tercetak CITO; Daftar Kerja menaruh Hemoglobin di atas pemeriksaan biasa |
| AC-319 | Wadah pemeriksaan pada AC-318 dinilai *Layak*; sesudah batas waktu cito prosedurnya lewat, Hemoglobin muncul di Pantau Keterlambatan Cito, dan laporan TAT menghitungnya sebagai cito |
| AC-320 | Sampel diambil ulang: pemeriksaan yang digantikan `Cito` (dari pemesanan maupun *Tandai Cito*) → pemeriksaan pengganti `Cito` dengan penanda yang sama; pemeriksaan yang cito-nya sudah dicabut dokter → pengganti `Routine` dengan penanda pencabutan itu |
| AC-321 | Perbaikan data lama: pesanan berjalan + permintaan `Cito` `Fulfilled` + pemeriksaan `Routine` berpenanda kosong yang bukan *Gugur*/*Dibatalkan* → `Cito` dan tepat satu baris riwayat `Routine → Cito`. Pesanan *Selesai*/*Dibatalkan*, pemeriksaan *Gugur*/*Dibatalkan*, dan pemeriksaan berpenanda terisi **tidak berubah**. Dijalankan dua kali → jalan kedua mengubah 0 baris |
| AC-322 | Pesanan lama tanpa baris permintaan → wadah tetap `Routine` seperti sebelumnya. Kartu CITO Beranda menghasilkan angka yang sama sebelum dan sesudah perbaikan (rumus dua sumber `r44` 39.6) |
| AC-323 | Dokter menandai Leukosit CITO; wadahnya **dibatalkan**; analis merencanakan wadah baru → Leukosit `Cito` dengan penanda dokter dan waktu yang sama |
| AC-324 | Dokter mencabut cito Hemoglobin (permintaan Cito); wadahnya dibatalkan; rencana ulang → Hemoglobin `Routine` dengan penanda pencabutan — permintaan tidak dibaca ulang |
| AC-325 | Rencana ulang tanpa pemeriksaan pendahulu → mengikuti permintaan (`AC-318`); perilaku pemeriksaan pendahulu **tanpa penanda** mengikuti `LAB-REQ-023` |

---

## Open Questions dan Blocker

### Dibuka Amendment Pass putaran 2 — 2026-09-14, diperbarui impact scan hari yang sama

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-CONFLICT-003`~~ | **Ditutup 2026-09-14** oleh `LAB-DEC-046` dan `LAB-DEC-047` | — | — |
| `LAB-COORD-006` | Data induk instansi perujuk **tidak punya endpoint tulis sama sekali**; yang kurang bukan sekadar status menunggu persetujuan, melainkan seluruh kemampuan pengelolaannya | Yoga Aji Pratama + pemilik `master-data` | `DESIGN` — bagian pendaftaran rujukan pada menu `Penerimaan Sampling/Specimen` |
| `LAB-COORD-007` | Dirumuskan ulang: menambah satu nilai `EncounterPaymentType` untuk piutang mitra secara aditif, beserta penurunannya dari status PKS. Bukan lagi permintaan endpoint ke Billing | Yoga Aji Pratama + pemilik `registration-management` + `billing-kasir` | `DESIGN` — bagian metode pembayaran pada menu yang sama |
| ~~`LAB-OPEN-022`~~ | **Ditutup 2026-09-14** oleh impact scan capability map revision 3 | — | — |
| `LAB-OPEN-023` | Dipersempit: hanya baris `decisions`, hash decision log, hash capability map, dan kedua `*_commit_sha` pada manifest. Dua masukan lain terverifikasi tidak berubah | Yoga Aji Pratama | `DESIGN` — pembukuan, bukan keputusan bisnis |
| `LAB-OPEN-024` | Berapa lama usulan instansi perujuk boleh menggantung, apa yang terjadi pada kunjungan bila usulan **ditolak**, dan siapa yang berhak menggabungkan dua baris | Pemilik `master-data` | `IMPLEMENTATION` — jalur penolakan usulan |
| `LAB-DEBT-001` | Seeder Laboratorium mengisi data induk global instansi perujuk — utang teknis terhadap `AC-49` | Yoga Aji Pratama + pemilik `master-data` | Tidak memblokir; dicatat agar tidak hilang |

### Dibuka Amendment Pass putaran 5 — Menu Hasil, 2026-09-16

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-COORD-011` | 🟡 **ARSITEKTUR DIPUTUSKAN 2026-10-06 — `LAB-DEC-183`**: PDF bernomor dan berversi ber-QR, WhatsApp lewat layanan platform terpisah, tautan aman alih-alih URL publik, status kirim terpisah dari rilis. **Yang tersisa hanya penyediaan** pembangkit PDF dan gerbang WhatsApp oleh platform — dependency implementasi, bukan penahan roadmap (`LAB-DEC-187`). Isi asli: **Gerbang pengiriman pesan (WhatsApp) dan pembangkit berkas PDF, keduanya nol pada platform.** `F7` diverifikasi ulang 2026-09-16 dan masih benar: nol `Twilio`, `Fonnte`, `SendMessageAsync`, `IWhatsAppService`, `SmtpClient`, maupun `MailKit` di seluruh backend; nol pustaka PDF pada `.csproj`. `QRCoder 1.8.0` sudah ada dan menutup kebutuhan barcode, tetapi tidak menutup kebutuhan PDF. Laboratorium tidak berwenang mengadakan keduanya sendiri. **Diverifikasi ulang 2026-09-23 dan masih benar seluruhnya** — nol `Twilio`, `Fonnte`, `IWhatsAppService`, `SendMessageAsync`, `SmtpClient`, maupun `MailKit` di seluruh backend; `QuilvianSystemBackend.csproj` memuat **satu** paket yang relevan, `QRCoder 1.8.0`, dan nol pustaka PDF. Tujuh hari sejak verifikasi 2026-09-16, **nol bergerak** | Pemilik platform + Yoga Aji Pratama | Seluruh `CAP-009`/`CAP-010` artifact — pengiriman dan pengiriman ulang hasil |
| ~~`LAB-OPEN-028`~~ | **Ditutup 2026-09-16** oleh `LAB-DEC-071` — pembandingnya inklusif, `Tgl Awal <= Tgl Akhir` | — | — |
| ~~`LAB-OPEN-029`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-182`** — gelar tidak menjadi izin; izin berbasis peran `LabResultPatientDeliveryApprover` bagi yang ditunjuk Kepala Instalasi; hasil normal Released tanpa persetujuan tambahan; persetujuan kedua dapat disetel bagi kategori sensitif (`LAB-OPEN-048`). Konfirmasi Kepala Instalasi `LAB-SIGN-002`. Isi aslinya: **MENYEMPIT 2026-09-17 — satu dari tiga pertanyaannya terjawab.** ✅ *Siapa pemegang wewenang Clinical Governance-nya* dijawab `LAB-DEC-078`: tiga dokter, satu per disiplin. ❌ Masih terbuka: **apakah persetujuan Profesor dan Dokter Lab yang disebut artifact adalah tanda tangan klinis `LAB-DEC-011`**, dan bagaimana keduanya menjadi peran pada `LAB-PERM-v1`. Penetapan justru **menajamkan** pertanyaan pertamanya: nol di antara ketiga nama bergelar Profesor, sehingga `RULE-017` menyebut penyetuju yang tidak ada padanannya pada penetapan. **2026-09-24 — MENYEMPIT LAGI (`LAB-EVD-009`):** bentuk persetujuannya **elektronik dengan audit trail**, bukan tanda tangan kertas. Siapa Profesor yang dimaksud, perannya pada `LAB-PERM-v1`, dan mekanismenya tetap **menunggu pihak klinis** | Yoga Aji Pratama + wewenang klinis | Memblokir penyerahan hasil kepada pasien (`LAB-DEC-139`) |
| `LAB-OPEN-030` | **Jabatan `Dokter Lantai` nol kemunculan** di seluruh blueprint, sedangkan artifact memakainya bersama `Dokter DPJP` pada kolom Dokter Konfirmator angka kritis | Yoga Aji Pratama | Bertaut `LAB-P0-004` dan `LAB-OPEN-014`. Memblokir kolom Dokter Konfirmator |
| ~~`LAB-OPEN-031`~~ | **Ditutup 2026-09-16** oleh `LAB-DEC-073` — menu Hasil hanya memuat pesanan Laboratorium; nilai `Radiologi` terbawa dari tabel unit layanan bersama | — | — |
| `LAB-OPEN-032` | **Ukuran cetak Nota Lab, Label Lab, dan Label Goldar.** Artifact menandainya sendiri `Confidence: Medium` dan menyebutnya default implementasi, bukan bukti — sehingga **tidak diadopsi** sebagai keputusan. Perlu profil printer dan media nyata | Yoga Aji Pratama + operasional laboratorium | Bentuk akhir ketiga dokumen cetak; tidak memblokir slice |
| ~~`LAB-OPEN-033`~~ | **Ditutup 2026-09-16** oleh `LAB-DEC-072` — kolom baru berisi nomor urut terbaca, pola `PatientEncounterNumberService`. **Pekerjaannya tetap ada**: satu kolom, satu layanan alokasi, satu index unik, satu migration | — | — |

### Dibuka penandatanganan klinis — 2026-09-17

Satu butir, dan ia lahir justru dari **bentuk** tanda tangannya, bukan dari isinya.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-OPEN-034`~~ | ✅ **DITUTUP 2026-09-25** oleh **`LAB-DEC-152`**: kewenangan validasi **tidak** melintasi disiplin (`LAB-EVD-010` — *"sesuai disiplin masing-masing"*); rilis per disiplin lewat `LAB-DEC-143`. Aturan yang dipertahankan lebih ketat, sehingga penutupannya tidak melonggarkan wewenang `DR-LAB-001`..`003`. Isi aslinya: **Apakah kewenangan validasi dan rilis melintasi disiplin.** `LAB-DEC-079` menandatangani ketiga keputusan **per disiplin**, sehingga wewenangnya kini berdimensi disiplin — tetapi apakah seorang validator Patologi Klinik boleh memvalidasi hasil Mikrobiologi **belum dinyatakan sama sekali**. Pertanyaan ini tidak pernah ada sebelum tanda tangannya berbentuk begini. Menyentuh tiga hal sekaligus: `LAB-DEC-003` prinsip empat mata, `LAB-DEC-022` jaminan dua pemegang kewenangan validasi per shift — yang menjadi **jauh lebih mahal** bila jaminannya harus dipenuhi per disiplin, bukan per laboratorium — dan bentuk resource/permission pada `LAB-PERM-v1` | `DR-LAB-001` + `DR-LAB-002` + `DR-LAB-003`, bersama Yoga Aji Pratama | Bertaut `LAB-P0-001`. **Tidak** memblokir `S4`; memblokir bentuk akhir penegakan kewenangan pada `S4b` dan `S4c` |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r6` — 2026-09-17

Tiga butir, ditemukan saat gerbang dijalankan ulang sesudah `LAB-SIGN-001` ditutup. **Dua di
antaranya bukan hal baru** — keduanya sudah tertulis di `LAB-REQ-004` bagian 5 sejak 2026-09-09
dan tidak ikut dijawab saat penandatanganan; gerbang hanya memberi keduanya identitas supaya
tidak hilang lagi. **Yang ketiga benar-benar baru, dan ia temuan peta, bukan temuan
requirement.**

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-011`~~ | ✅ **DITUTUP 2026-09-25** oleh `LAB-DEC-150` (sebagian, 2026-09-24) dan **`LAB-DEC-152`** — jawaban tertulis dr. Bima `LAB-EVD-010` atas `LAB-REQ-014`. Pemegang validasi per disiplin bernama; validasi di luar jam kerja oleh dokter lain yang ditetapkan pada disiplin yang sama; `LAB-DEC-022` butir 3 menjadi **syarat rilis**. **Tidak lagi memblokir apa pun**. Isi aslinya: **DIAJUKAN 2026-09-18 lewat `LAB-REQ-013`** kepada **dr. Bima Prasetya, Sp.PK** *(alamat dikoreksi 2026-09-23; nota semula salah ditujukan kepada dr. Arya Wicaksana, Sp.Rad — itu sebabnya ia tak berjawab lima hari)* selaku kepala instalasi — lihat [`approval-requests/2026-09-18-nota-penetapan-pemegang-kewenangan-validasi.md`](approval-requests/2026-09-18-nota-penetapan-pemegang-kewenangan-validasi.md). **Penahan termahal seluruh modul**: ia menahan `S4`, `S4d`, dan `S4e` sekaligus. Nota itu juga menanyakan **terbuka** dua hal yang belum jelas — instalasi mana yang dipimpin, dan apakah wewenang menetapkan ada pada kepala instalasi sendiri, pada ketiga pemegang wewenang klinis per disiplin, atau berlapis — sebab `Sp.Rad` adalah spesialis radiologi sedangkan modul ini melayani tiga disiplin laboratorium, dan menetapkan siapa boleh menyatakan sebuah angka hasil benar adalah penilaian kompetensi atas pekerjaan laboratorium. Isi aslinya: **Siapa yang berwenang MENETAPKAN seseorang sebagai pemegang kewenangan validasi, dan dapatkah rumah sakit menjamin minimal dua pemegang per shift.** `LAB-DEC-022` menetapkan kewenangan diberikan **per orang, bukan per jabatan** — maka harus ada pemberinya, dan pemberinya belum ada. Bahayanya sudah ditulis `LAB-REQ-004` bagian 5.1 sendiri: bila sebuah shift hanya punya satu pemegang, jalur pengecualian empat mata berubah menjadi **jalur utama** — pengujian tetap lulus, nol aturan dilanggar, dan prinsipnya berhenti berarti apa pun. **Diperiksa 2026-09-23: masih belum dijawab, lima hari sejak diajukan.** Nol jawaban tercatat pada nota `LAB-REQ-013` maupun di mana pun pada decision log ini. **Ia nol dapat dijawab dari sisi rekayasa** — yang ditanyakan adalah siapa berwenang menilai kompetensi seseorang menyatakan sebuah angka hasil laboratorium benar, dan apakah rumah sakit sanggup menjamin dua pemegang per shift; keduanya kebijakan rumah sakit, bukan pilihan rancangan. **2026-09-24 — wadahnya kini ditetapkan, jawabannya belum:** `LAB-DEC-148` menyimpan penunjukan pada kredensial Human Resource, sehingga dr. Bima kini dapat diberi **usulan konkret** — penetapan pemegang kewenangan lewat proses kredensial rumah sakit (grant, suspend, revoke, bermasa berlaku). Usulan itu **bukan jawaban**; butir ini tetap terbuka sampai dr. Bima menyatakannya. **2026-09-24 — DIJAWAB SEBAGIAN oleh dr. Bima (`LAB-EVD-009`, `LAB-DEC-150`):** ia pemegang pertama kewenangan validasi Patologi Klinik sekaligus penetap pemegang lain, dan validasi hanya oleh dokter berkewenangan laboratorium. **Sisa terbuka**, diajukan `LAB-REQ-014`: pemegang **kedua** per shift, dan pemegang Mikrobiologi serta Patologi Anatomi | Kepala instalasi laboratorium + manajemen rumah sakit | **Tidak lagi memblokir DESAIN `S4` Patologi Klinik.** Tetap **`BLOCKING`** bagi **rilis** `S4` ke pemakaian nyata, serta bagi desain `S4d` dan `S4e` |
| ~~`DEC-LAB-012`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-175`** — Kepala Instalasi penyetuju akhir, delegasi resmi per disiplin, pengusul ≠ penyetuju, nilai baru tidak aktif sebelum disetujui. Konfirmasi Kepala Instalasi `LAB-SIGN-002`. Isi aslinya: **Siapa pemberi "persetujuan klinis" atas perubahan batas kritis** yang disyaratkan `LAB-DEC-023`, bolehkah didelegasikan, dan cukupkah satu orang atau perlu rapat. `LAB-DEC-078` kini menyediakan **kandidat yang sebelumnya tidak ada** — ketiga pemegang wewenang klinis — tetapi itu `PROPOSED`: menyetujui aturan keselamatan dan menyetujui perubahan **angka** batas adalah dua wewenang berbeda, dan nol orang menyatakan keduanya melekat pada orang yang sama | Pihak klinis; kemungkinan `DR-LAB-001`/`002`/`003`, **perlu dinyatakan** | **`BLOCKING`** bagi `AC-33` dan `S5` |
| ~~`DEC-LAB-014`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-178`**..**`180`** — (1) pengaju berkewenangan validasi, penyetuju pengguna lain berkewenangan rilis; (2) dokter pemesan **dan** DPJP/unit aktif diberi tahu; (3) **tanpa** batas waktu. Konfirmasi `DR-LAB-001`..`003` `LAB-SIGN-002`. Isi aslinya: **Sisa klinis `LAB-P0-003` — tiga pertanyaan `LAB-REQ-004` bagian 4.5 yang tidak ikut dijawab saat penandatanganan.** (1) Apakah pemegang kewenangan validasi/rilis **cukup** untuk mengoreksi, atau koreksi menuntut wewenang lebih tinggi? (2) Apakah **hanya dokter pemesan** yang diberi tahu, atau juga DPJP dan unit perawatan? (3) Adakah **batas waktu** setelahnya hasil tidak boleh dikoreksi lagi? Dua di antaranya menyertakan usulan bertanda *(usulan)* pada dokumennya — *cukup*, dan *tidak ada batas* — tetapi **menganggap tanda tangan `apa adanya` ikut menyetujui usulan bagian 4.5 adalah lompatan**, sebab yang ditandatangani bagian 4.1 | `DR-LAB-001` + `DR-LAB-002` + `DR-LAB-003` | **`BLOCKING`** bagi `S6`. Bagian pemilik modul sudah ditutup `LAB-DEC-082` |
| ~~`DEC-LAB-013`~~ | ✅ **Ditutup 2026-09-18** oleh `LAB-DEC-083` — tiga pasang sejajar; `S4d` validasi/rilis Mikrobiologi dan `S4e` validasi/rilis Patologi Anatomi didirikan. Catatan asal: **Validasi dan rilis untuk Mikrobiologi dan Patologi Anatomi tidak punya slice sama sekali.** `S4b` dan `S4c` bernama *pengisian hasil*; `LAB-DEC-076` memecah `S4` menjadi `S4a` + `S4` **hanya untuk Patologi Klinik**. Celah ini tidak terlihat selama `LAB-SIGN-001` menahan kelimanya sekaligus. **Tanda tangan per disiplin membuatnya terlihat sekaligus mendesak:** `DR-LAB-002` dan `DR-LAB-003` kini memegang wewenang klinis atas disiplin yang **tidak punya tempat menjalankan wewenang itu**. Kelas kesalahan yang sama dengan `BE-EXT-04` — satu sisi berdiri tanpa sisi lainnya — hanya terbalik arahnya | Yoga Aji Pratama + ketiga pemegang wewenang klinis | **Tidak** memblokir bagian **pengisian** `S4b`/`S4c`; memblokir bagian validasi dan rilis keduanya. Bertaut erat `LAB-OPEN-034` |

### Dibuka rekonsiliasi bukti putaran 4 — `LAB-EVD-003`, 2026-09-18

Tujuh pertentangan, dan **nol dapat ditutup tanpa pemilik modul**. Rinciannya beserta sisi
blueprint dan sisi artifact ada pada `05-evidence-reconciliation.md` bagian 12.3.

> ### ✅ KETUJUHNYA TERTUTUP — 2026-09-18, amendment pass putaran 8
>
> | ID | Ditutup oleh |
> |---|---|
> | `REC4-CONF-001` | `LAB-DEC-088` — `Final` = patolog selesai menulis, **bukan** rilis. `LAB-DEC-080` utuh |
> | `REC4-CONF-002` | `LAB-DEC-086` — data induk parameter + nilai per baris |
> | `REC4-CONF-003` | `LAB-DEC-085` — per disiplin: PK/Mikro per pemeriksaan, PA per order |
> | `REC4-CONF-004` | `LAB-DEC-087` — data induk pemetaan milik Laboratorium; keyword hanya alat bantu pengisian awal |
> | `REC4-CONF-005` | `LAB-DEC-090` — pengisi hasil PA adalah Dokter Lab; `LAB-DEC-068` utuh |
> | `REC4-CONF-006` | **LARUT**, bukan diputuskan. `LAB-DEC-088` membuat `Reopen` sebelum rilis menjadi penyuntingan biasa — ia **bukan** koreksi hasil terrilis, sehingga **tidak** menyentuh `S6` maupun `DEC-LAB-014` |
> | `REC4-CONF-007` | `LAB-DEC-089` — `Diplo` = `Duplo`; pakai `IsDuplo` yang sudah berdiri |
>
> **Nol keputusan berumur kurang dari 48 jam yang dibatalkan.** `LAB-DEC-080`, `LAB-DEC-003`,
> `LAB-DEC-068`, `LAB-DEC-026`, dan `LAB-DEC-036` seluruhnya bertahan; yang berubah adalah
> cakupan `LAB-DEC-005`, dan itu dipersempit bukan dibatalkan.

| ID | Pertentangan | Pemilik | Memblokir |
|---|---|---|---|
| `REC4-CONF-001` | **`Draft`/`Final`/`Reopen` adalah status hasil**, sedangkan `LAB-DEC-080` yang berumur satu hari menetapkan **nol status hasil** | Yoga Aji Pratama | **`BE-LAB-46`**, `FE-LAB-25`; menyentuh `LAB-DEC-080` dan `INV-29` |
| `REC4-CONF-002` | **Bentuk hasil PA bergantung kategori** — tiga bentuk berbeda, dan IHK sendirian punya **10 ruas**; BR-23 hanya mengenal satu bentuk tiga ruas | Yoga Aji Pratama | **`BE-LAB-46`**, `FE-LAB-25`; menyentuh BR-23 dan `LAB-API-v1` `r24` |
| `REC4-CONF-003` | **Hasil melekat pada ORDER, bukan pada PEMERIKSAAN** (`RULE-014` artifact) | Yoga Aji Pratama | **Struktural.** Menyentuh `S4a` yang sudah berjalan, `S4b`, dan `S4c` sekaligus |
| `REC4-CONF-004` | **Kategori diturunkan dari keyword pada NAMA pemeriksaan**, sedangkan `LAB-DEC-036` menetapkannya melekat pada kolom katalog dan `LAB-DEC-048` butir 6 sudah mencabut penanyaannya | Yoga Aji Pratama + pemilik `master-data` | Bentuk hasil dinamis pada `S4c` |
| `REC4-CONF-005` | **Hak akses**: artifact memberi Dokter Lab penuh dan Petugas Lab hanya baca+cetak; `LAB-DEC-068` menetapkan seluruh tombol menu Hasil dipegang Petugas Lab dan/atau Admin | Yoga Aji Pratama + wewenang klinis | `LAB-PERM-v1`; bertaut `LAB-P0-001` dan `LAB-DEC-079` |
| `REC4-CONF-006` | **`Reopen` sesudah Final adalah koreksi hasil** — itu `S6`, dan `DEC-LAB-014` sedang ditanyakan kepada pihak klinis. Artifact menjawabnya sendiri: cukup Dokter Lab | `DR-LAB-001`/`002`/`003` | `S6`; **jawaban artifact tidak sah menutup `DEC-LAB-014`** |
| `REC4-CONF-007` | **`Diplo` versus `Duplo`** — beda satu huruf, dan maknanya beda lebih dari satu huruf: artifact menyebutnya indikator saja, `LAB-DEC-026` menyebutnya pengerjaan ganda | Yoga Aji Pratama | `LAB-DEC-026` dan `IsDuplo` yang sudah berdiri |

### Dibuka amendment pass putaran 8 — sisa `LAB-EVD-003` yang bukan pertentangan, 2026-09-18

Ketujuh pertentangan tertutup, tetapi **tiga belas butir baru** artifact tidak seluruhnya
terjawab olehnya. Yang tersisa dibuka di sini, dan dua di antaranya **bukan milik Laboratorium**.

> ### ✅ EMPAT DARI ENAM TERTUTUP — 2026-09-18, lanjutan sesi yang sama
>
> | ID | Ditutup oleh |
> |---|---|
> | `LAB-OPEN-035` | `LAB-DEC-091` — kelompok ruas khas PA pada pesanan, diisi **dokter pemesan** |
> | `LAB-OPEN-036` | `LAB-DEC-092` — keduanya **diturunkan**, bukan manual. **Mengoreksi Klarifikasi Q8 artifact** |
> | `LAB-OPEN-037` | `LAB-DEC-093` — ruas **baru**; dua orang, dua pekerjaan |
> | `LAB-OPEN-038` | `LAB-DEC-094` — **nilai saja** pada `S4c`; alur pelaporan kritis tetap `S5` |
>
> **Yang tersisa dua, dan keduanya BUKAN milik Laboratorium:** `LAB-COORD-012` sumber data HL7,
> dan `LAB-COORD-013` layanan terjemahan otomatis beserta izin privasinya. Keduanya memblokir
> **satu ruas** dan **satu bentuk cetak**, bukan seluruh halaman.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-COORD-012` | **Sumber data `HL7`.** Artifact menuntut pilihan HL7 pada halaman hasil PA (`REC4-NEW-006`). Penelusuran 2026-09-18 menemukan **`HL7` nol kemunculan di SELURUH backend** — bukan hanya modul Laboratorium. Artifact sendiri tidak merinci jenis object maupun profil HL7 yang dimaksud | Pemilik platform | Ruas HL7 pada `S4c`. **Tidak** memblokir bagian lain halaman |
| `LAB-COORD-013` | **Layanan terjemahan otomatis untuk cetak bahasa Inggris** (`REC4-NEW-011`), beserta **izin privasinya**. Nol pustaka, nol layanan pada platform. **Dan ini yang lebih berat:** terjemahan otomatis mengirim **diagnosis pasien** ke luar sistem — sekelas dengan penandaan `LAB-DEC-030` atas pengiriman hasil ke kanal pihak ketiga, dan **belum pernah ditandai** | Pemilik platform + Yoga Aji Pratama | Cetak bilingual. **Tidak** memblokir cetak Bahasa Indonesia |
| `LAB-OPEN-035` | **Empat ruas konteks klinis** — Diagnosa Awal, Riwayat Penyakit Relevan, Masa Terakhir Haid, Keterangan Klinis (`REC4-NEW-008`) — nol kolom pada `LabOrder`. `LAB-API-v1` `r11` justru **mencabut** `clinicalNote` sebelum sempat dibangun karena tidak ada tempatnya; kini requirement menuntut **empat** ruas. Siapa yang mengisinya, dan pada langkah mana | Yoga Aji Pratama | `S4c`. `Diagnosa Klinis` IHK terisi otomatis dari `Diagnosa Awal`, sehingga auto-fill-nya **nol punya sumber** sampai ini dijawab |
| `LAB-OPEN-036` | **`Waktu Issued` dan `Waktu Efektif`** (`REC4-NEW-007`) — keduanya diisi manual dan **bukan** `ExaminedAt` maupun `ResultEnteredAt` yang sudah ada. Apa beda keduanya, dan apa akibatnya bila berbeda dari waktu pengetikan | Yoga Aji Pratama | Ruas diagnostik pada `S4c` |
| `LAB-OPEN-037` | **`Penanggung Jawab Analis`** (`REC4-NEW-010`) — `LabOrder` punya `ExaminerDoctorId`, bukan analis. Apakah ini ruas baru, atau `ExaminerDoctorId` yang dimaksud | Yoga Aji Pratama | Ruas informasi pemeriksaan pada `S4c` |
| `LAB-OPEN-038` | **`Status Hasil PA`: Normal / Perlu Perhatian / Kritis** (`REC4-NEW-002`). Bentuknya **nilai**, sederajat `MicrobiologyFinding`, sehingga ia **tidak** melanggar `LAB-DEC-080`. Yang belum jelas: apakah `Kritis` di sini memicu alur nilai kritis `S5` — dan `S5` masih tertahan `LAB-P0-004` serta `LAB-OPEN-014` | Yoga Aji Pratama + `DR-LAB-003` | Bertaut `S5`. Ruas statusnya sendiri tidak memblokir pengisian |

**Tiga butir lain dari artifact sudah punya rumahnya dan tidak dibuka sebagai butir baru:**
`REC4-NEW-003`, `004`, dan `005` — Lokasi, Pola, dan Metode Pengambilan Specimen — seluruhnya
atribut wadah khas Patologi Anatomi, yaitu **`S2b`** yang memang masih
`BUSINESS_DECISION_REQUIRED`. `REC4-NEW-012` retry WhatsApp tetap di bawah `LAB-COORD-011`.
`REC4-NEW-013` harga pada daftar pemeriksaan tunduk pada `LAB-DEC-037`.

### Dibuka amendment pass putaran 9 — halaman Hasil Mikrobiologi, 2026-09-21

Enam belas keputusan diambil dan **enam pertentangan diselesaikan seluruhnya ke arah
blueprint** — nol keputusan terkunci dicabut. Yang tersisa tiga butir, dan **dua di antaranya
adalah celah bukti, bukan keputusan bisnis yang belum diambil**.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-039` | 🟡 **SEBAGIAN DITUTUP 2026-09-21** oleh `LAB-EVD-005` — cetakan Mikrobiologi, Patologi Anatomi kategori *Histological*, dan Patologi Klinik *Rutin* seluruhnya sudah diterima dalam bentuk berkas, bukan lagi uraian. **Yang masih kurang, dan seluruhnya varian:** Mikrobiologi versi **bakteri**, versi ber-**zona hambat mm**, versi **lebih dari satu isolat**, versi **kultur steril**, dan versi **halaman kedua**; Patologi Anatomi kategori **Sitologi**, **IHK**, dan **FNAB** beserta cara **gambar** dicetak. Isi aslinya: **Kedua screenshot template cetak Mikrobiologi tidak pernah diserahkan.** Artifact menyebut berkas kop dan footer sebagai bukti, tetapi yang sampai ke sesi 2026-09-21 hanya **uraiannya**. `LAB-DEC-110` membukukan uraian itu apa adanya dan **sengaja tidak** menaikkannya menjadi tata letak final. Yang dibutuhkan: kedua berkas gambarnya. **Diperiksa ulang 2026-09-23: nol berkas gambar berformat apa pun ada di seluruh folder blueprint modul ini** — `evidence/` memuat empat berkas, keempatnya `.md` berisi uraian. Yang kurang karena itu tetap utuh, dan bentuknya **penyerahan berkas**, bukan keputusan | Yoga Aji Pratama | Bentuk akhir cetak Mikrobiologi. **Tidak** memblokir pengisian hasil. Bertaut `LAB-OPEN-032` |
| ~~`LAB-OPEN-040`~~ | ✅ **DITUTUP 2026-09-21** oleh `LAB-EVD-007` dan `LAB-DEC-130`. Datasetnya diserahkan, dibaca, dan diverifikasi: **1.767 baris, jumlahnya benar apa adanya**. Dasar penyaringan kini terlihat — 1.601 baris berkonfidensi `Tinggi`/`Sedang` aktif, 166 baris `Rendah` nonaktif. **Sesi penyaringan kepala instalasi tetap diperlukan**, tetapi ia kini memangkas daftar yang sudah jalan, bukan menghadapi halaman kosong. Isi aslinya: **Penyaringan isi awal data induk Spesifik Specimen.** `LAB-DEC-099` menolak impor 1.767 entri mentah dan menyerahkan penyaringannya kepada kepala instalasi — tetapi **dataset-nya sendiri belum pernah dibaca blueprint**, dan berkasnya tidak ikut diserahkan. Yang dibutuhkan: berkas dataset, lalu satu sesi penyaringan | Kepala instalasi laboratorium + Yoga Aji Pratama | `IMPLEMENTATION` — pengisian data induk Spesifik Specimen. **Tidak** memblokir bentuk tabelnya |
> ### ✅ KEEMPATNYA TERTUTUP — 2026-09-21, amendment pass putaran 10
>
> | ID | Ditutup oleh |
> |---|---|
> | `LAB-CONFLICT-010` | `LAB-DEC-111` — `TrxOnCallAssignment` beserta jalur jatuh permanen; butir 2 `LAB-DEC-108` digantikan |
> | `LAB-CLOSE-010` | `LAB-DEC-111` — rantainya sudah berdiri pada satu `ApplicationDbContext`, nol tabel baru |
> | `LAB-CLOSE-011` | `LAB-DEC-112` — tabel jejak ruas tersendiri; satu tabel, satu sumbu |
> | `LAB-CLOSE-012` | `LAB-DEC-113` — daftar nilai Mikrobiologi tersendiri; `LabPathologyFindingStatus` tidak disentuh |
>
> **Satu butir baru lahir dan ia bukan milik Laboratorium:** `LAB-COORD-014`, endpoint pengisi
> `TrxOnCallAssignment`. Ia **nol memblokir** apa pun di sini justru karena jalur jatuh
> `LAB-DEC-111` dibangun lebih dulu.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-CONFLICT-011`~~ | ✅ **DITUTUP 2026-09-22 oleh pemilik modul — jalan B dipilih.** `AC-115` dipersempit menjadi *"pilih ulang antibiotik yang sama berhasil"*; klausa *"baris lama tetap ada bertanda `IsDelete = true`"* **dicabut**, sebab koleksi anak pada tabel ini memang diganti utuh secara fisik. Komentar index diperbaiki agar menyatakan kebenarannya: filternya **jaring pengaman, bukan kebutuhan**. **Filter parsialnya TETAP DIPERTAHANKAN** — bila gaya hapus tabel ini kelak berubah menjadi penandaan, index penuh akan mulai menolak antibiotik yang sama dipilih ulang, dan kegagalannya muncul di tangan analis, bukan di pipeline. **Alasan jalan A ditolak:** menyimpan generasi baris mati membuat **dua tempat menjawab pertanyaan riwayat** — antibiogram 22 baris yang disunting lima kali meninggalkan 110 baris yang nol pernah dibaca — dan itu pola yang sudah ditolak dua kali lewat `LAB-DEC-080` dan `LAB-DEC-112`. Jejak perubahan sudah punya rumahnya sendiri: `LabFieldChangeLog`. Uraian aslinya disimpan sebagai jejak: **Premis index unik parsial bertentangan dengan kode yang berjalan.** `LabIsolateSusceptibilityConfiguration` menulis alasan filternya apa adanya — *"penghapusan berupa penandaan (IsDelete)"* — tetapi `LabMicrobiologyResultService` baris 165–166 memakai `RemoveRange`/`Remove`, dan **nol tempat di codebase mengubah `EntityState.Deleted` menjadi penandaan** (nol interseptor `SaveChanges`, nol `HasQueryFilter` global). Dibuktikan `BE-LAB-49` 2026-09-22: baris kepekaan yang dihapus **lenyap secara fisik**, `0` baris bertanda terhapus. Akibatnya filter parsial pada index itu **nol pernah teruji** — index unik penuh akan berperilaku persis sama pada jalur ini, dan `AC-115` separuhnya terbantah. **Modul ini memang memakai dua gaya hapus dan keduanya disengaja** — penandaan untuk data induk dan dokumen, hapus fisik untuk koleksi anak yang diganti utuh (`LabMicrobiologyResultService`, `LabSpecimenCorrectionService`, `LabValueBoundService`) — sehingga yang salah bukan kodenya melainkan **komentar index beserta `AC-115`**, atau sebaliknya. Dua jalan keluar: **(A)** kode diubah menjadi penandaan — `AC-115` lolos utuh tetapi antibiogram 22 baris yang disunting lima kali meninggalkan 110 baris mati yang nol pernah dibaca; **(B)** `AC-115` dan komentar index diperbaiki — mengakui koleksi anak memang diganti utuh, sebab `LAB-DEC-112` sudah menempatkan jejak perubahan pada `LabFieldChangeLog` yang bersumbu berbeda. Filter parsialnya **tetap dipertahankan** pada kedua jalan: mencabutnya sebelum gaya hapusnya diputuskan adalah urutan yang terbalik | Yoga Aji Pratama (pemilik modul) | **Tidak memblokir `S4b`** — perilaku yang dialami analis sudah benar dan terbukti. Memblokir **kejelasan**: dibiarkan seperti sekarang, komentar index itu menyesatkan orang berikutnya yang membacanya |
| ~~`LAB-OPEN-043`~~ | ✅ **DITUTUP 2026-09-22 oleh pemilik modul — jalan A dipilih.** Dua kolom ditambahkan pada `LabDisciplineSetting`: `ReportNumberSeparator` (maks 5, **teks kosong = sengaja tanpa pemisah**, `null` = belum disetel) dan `ReportNumberLength` (1..12, default 4). Diamandemenkan sebagai `LAB-API-v1` `r29` bagian 24, `approved` hari yang sama, **ADITIF penuh**. Migration `20260922064824_AddLabReportNumberShape` ikut **mengisi sekali jalan** ketiga baris yang sudah ada dari `LAB-EVD-005` — itu **bukan** pelanggaran prinsip "seeder nol menimpa", sebab kolomnya baru lahir pada migration itu juga sehingga nol mungkin ada manusia yang pernah menyetelnya. **Terbukti pada tiga pesanan nyata:** Patologi Anatomi `26.0001`, Patologi Klinik `26000001`, Mikrobiologi `26-0004`. **Jalan B ditolak** — template teks bebas ber-`{yy}.{0000}` hanya gagal ketika lembarnya sudah tercetak, sedangkan dua kolom bertipe tegas dapat divalidasi saat disimpan. **AKIBAT YANG WAJIB DIINGAT, dan ia terbukti saat pengujian:** mengubah pemisah MENGULANG penghitung dari satu, sebab pencarian nomor tertinggi mencocokkan awalan tetap — Patologi Anatomi yang sudah punya `26-0001` memperoleh `26.0001`, bukan `26.0002`. Index unik tetap menjaga nol ada dua lembar bernomor sama, tetapi urutannya patah. **Anjuran: ubah bentuk nomor hanya pada pergantian tahun.** Uraian aslinya disimpan sebagai jejak: **Bentuk nomor cetak berbeda pada ketiga disiplin, dan satu ruas awalan tidak dapat menyatakannya.** `LAB-EVD-005` memperlihatkan Mikrobiologi `26-1129` berpemisah `-` dan empat digit, Patologi Anatomi `26.0919` berpemisah `.`, serta Patologi Klinik `25039254` yang **nol berpemisah** dan berurut **enam** digit. `r27` bagian 22.7 menyetujui satu ruas `reportNumberPrefix` saja; pemisah dan lebar nomor **tidak dapat** dinyatakan olehnya. `BE-LAB-63` karena itu memakai `-` dan empat digit bagi ketiganya — **cocok dengan Mikrobiologi, dan tidak dengan dua lainnya**. Ditulis terbuka, bukan ditutup diam-diam: membangun format yang salah bagi dua dari tiga disiplin sambil berdiam diri adalah cara termurah membuat cacat ini ditemukan oleh pasien yang memegang lembarnya. Tiga jalan keluar sudah dinilai dan ketiganya ditolak sementara — menambah kolom pemisah dan lebar (mengubah kontrak `approved` secara sepihak), menjadikan awalan sebuah template ber-`{yy}` (namanya akan berbohong tentang isinya), dan menaruh tahun di dalam awalan lalu meminta penggantian tiap Januari (salahnya baru terlihat pada lembar yang sudah tercetak) | Yoga Aji Pratama (pemilik modul) — menuntut amandemen `LAB-API-v1` | **Tidak memblokir `S4b`.** Alokasi, index, dan jalur bacanya sudah berjalan; yang berubah kelak hanya bentuk teksnya. **Memblokir cetakan Patologi Anatomi dan Patologi Klinik** ketika layarnya dibangun |
| `LAB-OPEN-042` | **Bolehkah isolat dicatat ketika status temuan `Negatif`?** `INV-26` menyebut *"isolat hanya boleh ada bila status temuannya memungkinkan pertumbuhan"*, tetapi arsitektur sendiri menandainya **usulan arsitektur**, bukan keputusan pemilik modul. Dibuka saat perancangan `S4b` 2026-09-21 daripada ditegakkan diam-diam sebagai aturan validasi | Yoga Aji Pratama + `DR-LAB-002` | **Tidak memblokir.** `S4b` dibangun tanpa aturan ini; ia ditambahkan bila jawabannya menuntut |
| `LAB-COORD-014` | **`TrxOnCallAssignment` nol punya controller** — tabel penugasan jaga dapat dibaca tetapi tidak ada satu pun cara mengisinya. Diverifikasi 2026-09-21 pada `981e002c`. Laboratorium **tidak** berwenang mengadakannya sendiri | Pemilik `human-resource` | **Tidak memblokir.** Selama kosong, `LAB-DEC-111` jatuh ke daftar dokter aktif. Yang tertunda hanya ketepatan daftarnya, bukan kemampuannya |

**Butir putaran 9 yang kini tertutup, disimpan untuk riwayat:**

| ID | Pertentangan | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-CONFLICT-010`~~ | **`MstDoctorSchedule` yang dipakai `LAB-DEC-108` ternyata jadwal praktik poliklinik, bukan daftar dokter jaga.** Ditemukan impact scan `/qv-trace` revision 4 pada `981e002c`: `ClinicId` wajib, ada kuota pasien dan penanda kiosk/telemedicine, dan `ScheduleType` nol memuat nilai yang berarti "sedang jaga". **Butir DPJP tetap sahih**; yang terbantah hanya sumber pilihan kedua. Kandidatnya `TrxOnCallAssignment` milik Human Resource, tetapi ia menunjuk `WorkforceProfileId` bukan `DoctorId` | Yoga Aji Pratama | Kolom Dokter Konfirmator. Membuka ulang **butir 2** `LAB-DEC-108`; `LAB-OPEN-030` **tidak** dibuka kembali karena bentuk jawabannya — dua cara memilih, bukan dua jabatan — tetap berlaku |
| ~~`LAB-CLOSE-010`~~ | ✅ **Ditutup 2026-09-21** oleh `LAB-DEC-111`. Isi aslinya: **Dari mana daftar dokter yang sedang bertugas diambil?** Pilihan yang terlihat dari source: menumpang `TrxOnCallAssignment` beserta jembatan profil-ke-dokter, kembali ke DPJP saja, atau mendirikan penugasan jaga milik Laboratorium sendiri | Yoga Aji Pratama, bersama pemilik `human-resource` bila kandidat pertama dipilih | Sama dengan `LAB-CONFLICT-010`. Diajukan ke `/grill-me` putaran berikutnya |
| ~~`LAB-CLOSE-011`~~ | ✅ **Ditutup 2026-09-21** oleh `LAB-DEC-112`. Isi aslinya: **Jejak perubahan ruas specimen disimpan di mana?** `LabTransitionHistory` mencatat perpindahan **status** — `FromStatus`, `ToStatus`, `Action` — sedangkan `LAB-DEC-107` menuntut **nilai lama sebuah ruas** tetap terbaca. Ditumpangkan dengan penyesuaian, atau berdiri sendiri | Yoga Aji Pratama | Bentuk jejak `LAB-DEC-107`. **Tidak** memblokir penyuntingannya |
| ~~`LAB-CLOSE-012`~~ | ✅ **Ditutup 2026-09-21** oleh `LAB-DEC-113`. Isi aslinya: **Status temuan Mikrobiologi memakai daftar nilai yang mana?** `LabPathologyFindingStatus` yang sudah berdiri berisi `Normal`/`NeedsAttention`/`Critical`; BR-56 menetapkan `Normal`/`Positif`/`Negatif` untuk tingkat isolat. Dipakai ulang dengan penyesuaian, atau daftar tersendiri | Yoga Aji Pratama + `DR-LAB-002` | Bentuk isolat pada `S4b` |
| `LAB-OPEN-041` | **Isi baris aturan kritis Mikrobiologi.** `LAB-DEC-103` mengunci **bentuknya** — data induk terkendali berisi kombinasi organisme, antibiotik, dan interpretasi — dan menyerahkan **isinya** kepada `DR-LAB-002`. Selama barisnya kosong, penanda kritis nol menyala. Bertaut erat `LAB-OPEN-014` yang menanyakan hal yang sama untuk dua disiplin sekaligus | `DR-LAB-002` | Penanda kritis Mikrobiologi menyala. **Tidak** memblokir `S4b`; memblokir kegunaan `S5` bagi Mikrobiologi |

**Yang tetap di tempatnya dan tidak dibuka ulang sebagai butir baru:** pengiriman WhatsApp
beserta status *delivered* yang diminta `CAP-020` artifact tetap `LAB-COORD-011`; preview dan
cetak dwibahasa `CAP-021` tetap `LAB-COORD-013`; ruas HL7 tetap `LAB-COORD-012` dan kini
disertai keputusan tegas `LAB-DEC-109` untuk tidak membangunnya lebih dulu; serta Lokasi dan
Metode Pengambilan Specimen yang diminta `CAP-010` tetap `S2b`.

### Dibuka amendment pass putaran 14 — PRD Hasil Pemeriksaan PK & Mikrobiologi, 2026-09-24

PRD `LAB-EVD-008` membuka **14 pertentangan dan 6 butir baru**. Tabel lengkapnya beserta status
per butir ada pada bagian *Amendment Pass Putaran 14* di atas; di sini hanya yang memblokir.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`PRD1-CONF-01`, `PRD1-CONF-02`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-134`** — analis tetap mengisi hasil Patologi Klinik; Dokter Lab dibaca sebagai pemvalidasi/pengotorisasi. Nol perubahan source | Yoga Aji Pratama | — |
| ~~`PRD1-CONF-04`..`PRD1-CONF-06`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-135`** — Patologi Klinik memakai Draft/Final per pemeriksaan; status PRD menjadi label turunan | Yoga Aji Pratama | — |
| ~~`PRD1-FOLLOW-01`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-138`** — *Kembalikan ke analis* beralasan dari daftar `LAB-DEC-082`, riwayat validasi tetap tercatat, tanpa versi bernomor dan tanpa pemberitahuan dokter | Yoga Aji Pratama | — |
| ~~`PRD1-CONF-11`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-136`** — WhatsApp saluran pengantar; pelaporan tuntas hanya dengan kelima isian `LAB-DEC-004` | Yoga Aji Pratama | — |
| `PRD1-CLIN-01` | **Bolehkah balasan WhatsApp dokter yang menyebut ulang nilai kritis diterima sebagai bukti pembacaan ulang `LAB-DEC-004`?** Sampai dijawab, bukti baca ulang harus dari percakapan langsung. **Pertimbangan tambahan sejak `LAB-DEC-137`:** pesan keluar tidak memuat nilai, sehingga balasan yang menyebut ulang angkanya berarti **dokter sendiri** memasukkan data klinis ke WhatsApp | `DR-LAB-001` (Patologi Klinik), `DR-LAB-002` (Mikrobiologi); bertaut `LAB-P0-004` | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-151`** — balasan WhatsApp hanya konfirmasi komunikasi, **bukan** bukti baca ulang. Jawaban dr. Bima mempertahankan aturan yang lebih ketat, sehingga nol wewenang klinis dilonggarkan |
| ~~`PRD1-FOLLOW-02`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-137`** — pesan WhatsApp kritis tanpa data klinis; jalur kritis tidak menunggu izin privasi | Yoga Aji Pratama | — |
| ~~`PRD1-CONF-12`~~ | ✅ **Ditutup 2026-09-24** — tetap blueprint (bawaan `LAB-DEC-133`); sisa klinisnya tetap `DEC-LAB-014` | Yoga Aji Pratama | — |
| ~~`PRD1-CONF-13`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-139`** — satu gerbang untuk kirim, cetak, dan unduh dokumen final pasien. Bentuk persetujuannya tetap `LAB-OPEN-029`, dan kini menjadi penahan berikutnya bagi `S17` begitu `DEC-LAB-011` terjawab | Yoga Aji Pratama | — |
| ~~`PRD1-NEW-01`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-140`** — QR verifikasi keaslian dan status dokumen; nol data kesehatan pada halaman publik | Yoga Aji Pratama | — |
| `PRD1-OPEN-01` | **Arti dan sumber data Nomor Transaksi dan Nomor Mutasi** (PRD FR-PK-001). Keduanya tercetak pada cetakan Patologi Klinik lapangan, tetapi belum pernah dipastikan apa isinya dan dari mana datanya. **2026-09-24:** dr. Bima bertanya balik apakah keduanya billing atau inventory; **bukti belum cukup untuk keduanya** — lihat putaran 16. Penutupnya satu contoh cetakan berisi nilai (disamarkan) dan konfirmasi admin sistem lama tentang ruas asalnya | Yoga Aji Pratama | **Tidak memblokir** — hanya tampilan identitas pada halaman dan cetakan `S4a` |
| `LAB-COORD-015` | 🟡 **MODEL DIPUTUSKAN 2026-10-06 — `LAB-DEC-184`**: token opaque/bertanda tangan yang dapat dicabut, informasi minimum, tanpa pencarian pasien. **Diperluas** dengan model tautan aman hasil pasien (`LAB-DEC-183` butir 3). Yang tersisa: **persetujuan pemilik keamanan/platform** atas kedua model; task verifikasi publik dan tautan aman dibuat sesudahnya. Isi asli: **Halaman verifikasi dokumen publik tanpa login.** `LAB-DEC-140` menuntut satu jalur backend yang dapat dibuka tanpa autentikasi, memuat nol data kesehatan, dengan token acak yang tidak dapat ditebak. Jalur tanpa login adalah keputusan keamanan platform; `owners.security` pada manifest masih *belum ditetapkan* | Pemilik keamanan/platform — **belum ditetapkan** | `IMPLEMENTATION` — halaman verifikasi saja. Desain dan pencetakan QR **tidak** tertahan |
| ~~`PRD1-NEW-02`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-141`** — konsultasi Patologi Klinik dicatat sebagai fakta opsional, nol kolom baru | Yoga Aji Pratama | — |
| ~~`PRD1-CONF-03`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-142`** — kewenangan validasi dan rilis berlapis dua: jabatan calon dan penunjukan per orang. Pengisi daftar penunjukan tetap `DEC-LAB-011` | Yoga Aji Pratama | — |
| ~~`PRD1-FOLLOW-03`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-143`** — penunjukan dicatat per orang, per jenis kewenangan, per disiplin | Yoga Aji Pratama | — |

### Dibuka capability map revision 5 — closure pass putaran 15, 2026-09-24

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-CLOSE-013`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-146`** — tindakan atas hasil memakai izin tersendiri | Yoga Aji Pratama | — |
| ~~`LAB-CLOSE-014`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-147`** — hasil Final ditolak bila disimpan ulang; `S4b` diperbaiki sekarang | Yoga Aji Pratama | — |
| ~~`LAB-CLOSE-015`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-148`** — penunjukan disimpan pada kredensial Human Resource; Laboratorium hanya membaca | Yoga Aji Pratama | — |
| `LAB-COORD-016` | **Kesepakatan dengan pemilik `human-resource`:** (1) enam kode kewenangan Laboratorium — validasi dan rilis untuk tiga disiplin — ditambahkan ke `MstClinicalPrivilegeCatalog`, beserta nama dan formatnya; (2) Laboratorium membaca `WfpClinicalPrivilege` lewat `ApplicationUser.WorkforceProfileId`, sejajar preseden Kamar Operasi, tanpa menulis. Katalog dan kewenangan **sudah punya jalan tulis**, sehingga yang diminta persetujuan, bukan pembangunan | Pemilik `human-resource` | `IMPLEMENTATION` — `S4`/`S4d`/`S4e`. **Tidak** menahan desain |
| ~~`LAB-CLOSE-016`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-149`** — halaman detail hasil Patologi Klinik per order; dialog modal Daftar Kerja dicabut | Yoga Aji Pratama | — |
| ~~`LAB-CONFLICT-013`~~ | ✅ **Ditutup 2026-09-24 oleh `LAB-DEC-149`** | Yoga Aji Pratama | — |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r9` — 2026-09-25

Gerbang menilai ulang `S4d` dan `S4e` sesudah putaran 17 menutup penahan lama keduanya. Dua
keputusan **klinis** yang belum pernah ditanyakan ditemukan, satu per disiplin. Rinciannya:
`02-requirement-completeness-assessment.md` bagian 0D.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-020`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-171`**..**`173`** — `Sementara` boleh divalidasi dan dirilis; `Definitif` tahap berikutnya, bukan `S6`; order belum Selesai selama `Sementara`; dokter pemesan diberi tahu saat `Definitif` menyusul `Sementara` terrilis. Hasil tanpa kualifikasi tetap bukan sementara. Konfirmasi `DR-LAB-002` `LAB-SIGN-002`. Isi aslinya: **Bolehkah hasil Mikrobiologi berkualifikasi `Sementara` divalidasi dan dirilis? Bila boleh, bagaimana hasil `Definitif` menggantikannya** — sebagai koreksi `S6` (versi lama bertanda *sudah diperbaiki*, dokter pemesan diberi tahu), atau sebagai **rilis bertahap** yang sah tanpa dianggap koreksi? `LAB-DEC-114` sudah memutuskan kualifikasi itu **dicetak**, dan BR-68 menyatakan biakan dibaca bertahap berhari-hari. **Contoh:** pewarnaan Gram darah pasien sepsis hari pertama menunjukkan kokus Gram positif; biakan baru tumbuh hari ketiga. Bila hasil hari pertama tidak dapat dirilis, dokter memulai antibiotik tanpa dasar tertulis; bila dapat, hasil hari ketiga harus punya cara sah untuk menggantikannya. **Titipan arsitektur `LAB-DA-001` rev 9 (`ARCH-GAP-LAB-10`): tanyakan sekaligus apakah hasil *tanpa* kualifikasi dianggap sementara.** Sampai dijawab, hasil tanpa kualifikasi diperlakukan **bukan** sementara — ruas itu sengaja opsional (`LAB-VAL-v1` `r10` 12.2) | `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK + Yoga Aji Pratama | **Desain `S4d-2`** — hasil `Sementara`. **Tidak** memblokir `S4d-1` |
| ~~`DEC-LAB-021`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-174`** — pilihan (c): penulis memvalidasi, Sp.PA lain merilis, analis tidak pernah. **Pemakaian nyata menunggu tanda tangan `DR-LAB-003`** (`LAB-SIGN-002`). Isi aslinya: **Siapa memvalidasi laporan Patologi Anatomi yang ditulis patolog?** (a) patolog **kedua** yang ditunjuk — empat mata sungguhan; (b) penulisnya sendiri lewat **jalur pengecualian** beralasan; atau (c) **aturan PA tersendiri** — penulis sekaligus pemvalidasi, dan empat mata dipenuhi pada **rilis** oleh orang lain. `LAB-DEC-090` menetapkan pengisi laporan PA adalah patolog dan **mencatat sendiri** pertanyaan ini belum terjawab; `LAB-DEC-003` melarang pengisi memvalidasi hasilnya sendiri; `LAB-DEC-079` mengizinkan aturan PA berbeda dari disiplin lain. **Bahaya pilihan (b) bila patolognya hanya satu:** setiap laporan selamanya bertanda pengecualian, dan penanda itu berhenti berarti apa pun | **`DR-LAB-003` dr. Citra Maharani, Sp.PA** — penanda tangan `LAB-DEC-003` bagi PA — + Yoga Aji Pratama | **Desain `S4e` seluruhnya** |

**`DEC-LAB-017` berlaku sejenis bagi `S4d` dan `S4e`**, dengan pemilik tambahan `DR-LAB-002` dan
`DR-LAB-003` untuk disiplinnya masing-masing. Ia menahan pemakaian nyata, bukan desain.

### Dibuka capability map revision 6 — 2026-09-25

Impact scan terbatas atas dokumen klinis pasien dan kunjungan MCU (`01-existing-capability-map.md`
Impact Scan Revision 6). Kedua pertentangan **bukan milik Laboratorium untuk diselesaikan** — keduanya
bahan `LAB-COORD-018`.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-CONFLICT-015` | **Jalan keluar berkas salah pasien.** Dokumen klinis pasien punya tiga — `PUT` ke `EnteredInError` **tanpa** alasan, batal beralasan (`Cancelled`), dan hapus (`IsDelete`) — sedangkan `LAB-DEC-158` meminta satu: *salah input beralasan, tidak dihapus* | Pemilik `clinical-management`, lewat `LAB-COORD-018` | **Desain `S18`** bagian penarikan berkas |
| `LAB-CONFLICT-016` | **Kewenangan atas keadaan dokumen.** Review, verify, approve, arsip, dan batal memakai **satu** kode izin `PatientClinicalDocument : Update`; pembuat dapat mengirim `IsVerified`/`IsApproved` benar saat membuat — memverifikasi unggahannya sendiri. `LAB-DEC-158` membatasi penandaan salah input pada *pengunggah atau kepala instalasi*. **Contoh:** perawat poli pemegang `Update` dapat membatalkan PDF HbA1c unggahan petugas lab. Pola yang sama dengan `LAB-CONFLICT-012` | Pemilik `clinical-management` + pemilik platform (izin), lewat `LAB-COORD-018` | **Desain `S18`** bagian kewenangan |
| `UNK-P19-01` | **Pemesanan dari kunjungan MCU belum dapat dibuktikan berjalan.** Jalur pesanan Lab siap dan backend menerima kunjungan `MedicalCheckup`, tetapi **nol layar** membuat kunjungan MCU. Membuktikannya lewat API berarti menulis ke basis data pengembangan yang dipakai bersama. Pertanyaan penutup `Q-P19-01`: cukupkah bukti kode dan satu panggilan API untuk menutup `S19` (`LAB-DEC-164`)? | Yoga Aji Pratama | **Penutupan `S19`** saja — tidak ada yang dirancang |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r11` — 2026-09-25

Gerbang menilai ulang kelima slice sesudah putaran 19. **`S16a` naik `READY_FOR_DOMAIN_DESIGN`.**
Pembacaan ulang cetakan PA `LAB-EVD-005` untuk `S2b` menemukan dua hal yang belum pernah diputuskan.
Rinciannya: `02-requirement-completeness-assessment.md` bagian 0F.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-028`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-169`** — per wadah, data induk terkendali, `Tidak Memerlukan Fiksasi`, dicatat yang memfiksasi atau menerima informasinya. Isi daftar `LAB-OPEN-050`. Isi aslinya: **Apakah fiksasi specimen PA dicatat**, siapa mencatatnya — ruang tindakan saat pengambilan atau laboratorium saat penerimaan — dan dari daftar apa. Cetakan PA memuat ruas `Fiksasi` (*Formalin buffer 10%*), tetapi ruas itu **tidak pernah** diadopsi ketika cetakan diterima putaran 11. **Usulan baseline:** dicatat per wadah dari daftar terkendali, oleh pihak yang memfiksasi | Yoga Aji Pratama + `DR-LAB-003` | **Desain `S2b-3`** saja |
| ~~`DEC-LAB-029`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-168`** — satu wadah satu lokasi; beberapa lokasi = beberapa wadah. Isi aslinya: **Satu wadah satu lokasi, atau satu wadah boleh memuat beberapa lokasi bernomor?** Cetakan PA menomori `Lokasi Spesimen` dan memuat lebih dari satu; `LAB-DEC-161` menyatakan *melekat pada wadah* tanpa kardinalitas, dan `LAB-DEC-098` sudah mengizinkan lebih dari satu Spesifik Specimen per wadah. **Contoh:** mastektomi kiri dalam dua wadah — (1) payudara kiri, (2) kelenjar getah bening aksila kiri — cetakannya sama, bentuk datanya berbeda. **Usulan baseline:** satu wadah satu lokasi | Yoga Aji Pratama + `DR-LAB-003` | **Desain `S2b-1`** — satu-satunya penahannya |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r10` — 2026-09-25

Gerbang menilai ulang lima slice yang belum disentuh sejak revision 3 — `S2b`, `S8`, `S16`, `S18`,
`S19` — atas permintaan pemilik modul. **Nol slice naik.** Dua penahan lama yang `closed` —
`LAB-COORD-001` dan `LAB-COORD-002` — ternyata hanya menjawab **siapa pemilik**, bukan **apa yang
dibangun**. Rinciannya: `02-requirement-completeness-assessment.md` bagian 0E.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-022`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-170`** — Sitologi dan FNAB berseri sendiri, nomor order tetap global, bentuk nomor disetel; tidak lagi menunggu cetakan. Setelan `LAB-OPEN-049`. Sebelumnya: 🟡 **MENYEMPIT 2026-09-25 — putaran 19.** Separuh blok dan slide dijawab `LAB-DEC-162`: **tidak dilacak**. **Yang tersisa hanya seri nomor Sitologi dan FNAB**, ditahan pemilik modul sampai cetakannya diserahkan (`LAB-OPEN-039`); sampai itu seri PA berlaku. Isi asli: **Apakah Sitologi dan FNAB punya seri nomor cetak sendiri** atau ikut seri PA (`LAB-DEC-117`), dan **apakah blok parafin serta slide dicatat dan dilacak** sebagai benda fisik — nomor, jumlah, arsip, peminjaman. **Contoh:** biopsi payudara dan FNAB tiroid hari yang sama bernomor `26.0919`/`26.0920` bila seri bersama, atau `26.0919`/`F26.0112` bila FNAB berseri sendiri — dan alokator yang hari ini berkunci disiplin harus berkunci kategori | Yoga Aji Pratama + `DR-LAB-003` | **Desain `S2b`** bagian penomoran dan turunan fisik. Penutup buktinya: cetakan Sitologi/IHK/FNAB (`LAB-OPEN-039`) |
| ~~`DEC-LAB-023`~~ | ✅ **DITUTUP 2026-09-25 oleh `LAB-DEC-161`** — melekat pada wadah, diisi pengisi hasil sampai Final, `Lainnya` + keterangan, kewajiban per kategori. **Tidak lagi memblokir.** Isi asli: **Lokasi, pola, dan metode pengambilan specimen** melekat di mana (wadah atau pesanan), diisi siapa (dokter pemesan atau petugas penerima), dan wajib bagi kategori apa. Kebutuhannya sudah `CONFIRMED` (`REC4-NEW-003`..`005`, `PRD1-NEW-06`); daftar nilainya `CONFIGURABLE_DEFAULT` | Yoga Aji Pratama + `DR-LAB-002`, `DR-LAB-003` | **Desain `S2b`** bagian atribut pengambilan |
| ~~`DEC-LAB-024`~~ | ✅ **DITUTUP 2026-09-25 oleh `LAB-DEC-163`** — tidak ada kejadian lain; `S8` dilebur menjadi dependency `S5`/`S6`. **Tidak lagi memblokir.** Isi asli: **Kejadian laboratorium apa, selain nilai kritis dan koreksi, yang wajib memberi tahu siapa**, lewat saluran apa, dan apakah penerima wajib menandai sudah membaca — misalnya *hasil dirilis* kepada dokter pemesan atau DPJP. Bila jawabannya *tidak ada*, `S8` diusulkan dilebur sebagai dependency `S5`/`S6` | Yoga Aji Pratama + `DR-LAB-001` | **Desain `S8`** |
| ~~`DEC-LAB-025`~~ | ⏸ **DITUNDA KE PHASE 2 — 2026-10-06** oleh **`LAB-DEC-181`**: delapan laporan `S16b` menjadi `PHASE 2 — DEFERRED PENDING REPORT CATALOG` dan tidak lagi menahan roadmap lain. Sebelumnya: 🟡 **MENYEMPIT 2026-09-25 — putaran 19.** Tiga laporan `S16a` dan pembacanya diputuskan `LAB-DEC-159`, `LAB-DEC-160`. **Yang tersisa: delapan laporan lainnya** beserta data lintas modulnya (biaya, penjamin, diagnosis) — menunggu daftar dan definisi dari kepala instalasi. Isi asli: **Nama dan definisi kesebelas laporan operasional** — rumus, titik mulai dan akhir waktu penyelesaian, perlakuan pemeriksaan batal, periode, penyaring, pembaca, format ekspor — serta data lintas modul mana yang boleh dibaca (biaya, penjamin, diagnosis). **Contoh:** Kalium cito diterima 08.00, Final 08.40, dirilis 09.10 — 40 atau 70 menit tergantung titik akhirnya, *tepat waktu* atau *terlambat* pada batas 60 menit. Dokumen sumbernya tidak tersimpan di repository | Yoga Aji Pratama + kepala instalasi; `billing-kasir` untuk biaya | **Desain `S16`**. Tiga laporan `DEC-LAB-007` dapat dipisah lebih dulu bila definisinya disahkan |
| ~~`DEC-LAB-026`~~ | ✅ **DITUTUP 2026-09-25 oleh `LAB-DEC-157` dan `LAB-DEC-158`** — satu data di dokumen klinis pasien, pintu unggah di Laboratorium, tertaut pasien dan kunjungan, cek dua identitas. **Yang tersisa bukan keputusan Laboratorium:** penyimpanan berkas (`DEC-LAB-016`) dan kesepakatan dengan `clinical-management` (`LAB-COORD-018`). Isi asli: **Siapa pemilik penautan berkas hasil laboratorium eksternal** — dokumen klinis pasien milik Clinical Management yang **sudah ada** (`TrxPatientClinicalDocument`: jenis `LaboratoryResult`, sumber `ExternalHospital`, status verifikasi; sejak `9d38d30a` 2026-06-02 — *dikoreksi capability map revision 6; semula tertulis `58c61a5b`, yang hanya mengganti nama tabel*), atau jalur milik Laboratorium seperti bunyi `LAB-DEC-030`? Siapa mengunggah dan memverifikasi, ditautkan ke apa, dan bagaimana berkas salah pasien ditarik? **Bertentangan** dengan `LAB-DEC-030` soal pemilik. Penyimpanan berkasnya mengikuti **`DEC-LAB-016`, yang kini berlaku juga bagi PDF** | Yoga Aji Pratama + pemilik `clinical-management` | **Desain `S18`** |
| ~~`DEC-LAB-027`~~ | ✅ **DITUTUP 2026-09-25 oleh `LAB-DEC-164`** — jalur umum; perlakuan khusus milik layanan MCU kelak. `S19` **menunggu verifikasi berjalan**, bukan keputusan. Isi asli: **Apakah order dari MCU berbeda dari order kunjungan biasa** — paket dan harga paket, klien perusahaan, pemesan non-dokter, hasil gabungan, penerima hasil selain pasien — atau cukup lewat jalur umum, yang hari ini **sudah** menerima kunjungan `MedicalCheckup` tanpa pembatasan | Yoga Aji Pratama + pemilik layanan MCU (belum ditetapkan) + `billing-kasir` | **Desain `S19`**. Bila *cukup jalur umum*, `S19` dapat ditutup sesudah verifikasi berjalan |
| `LAB-COORD-017` | **Penyediaan kemampuan pemberitahuan tersimpan oleh pemilik platform.** `LAB-COORD-001` (2026-09-01) menyepakati pemberitahuan sebagai kemampuan platform, tetapi pada `cfafad8d` platform masih **nol** sarana pemberitahuan — sama dengan temuan 2026-09-01 | Pemilik platform | **Dependency** `S8`, `S5`, `S6`. Bukan keputusan bisnis Laboratorium. *Sejak `LAB-DEC-163`, `S8` dilebur ke `S5`/`S6` — penahan ini kini melekat pada keduanya* |
| `LAB-COORD-018` | 🟡 **MENYEMPIT 2026-10-06** — butir (3) dijawab dari sisi Laboratorium oleh **`LAB-DEC-186`** (`Verified` = verifikasi administratif/lab, `Approved` = penerimaan klinis); butir (2) diarahkan **`LAB-DEC-185`** (penyimpanan privat, penyediaannya `LAB-COORD-019`). Yang tersisa: **konfirmasi pemilik `clinical-management`** atas ketiga butir, serta `LAB-CONFLICT-015`/`016`. Isi asli: **Kesepakatan dengan pemilik `clinical-management`** — dibuka putaran 19 oleh `LAB-DEC-157`: (1) Laboratorium menjadi salah satu pintu unggah yang **menulis** dokumen klinis pasien berjenis `LaboratoryResult` dan bersumber `ExternalHospital`; (2) kemampuan itu hari ini **tidak mengunggah berkas** — `POST` hanya menerima `FilePath` teks — sehingga cara berkas masuk perlu disepakati bersama `DEC-LAB-016`; (3) arti status `Verified`/`Approved` bagi berkas eksternal yang diunggah petugas lab | Pemilik `clinical-management` + Yoga Aji Pratama | ~~Desain `S18` bagian penulisan dan unggah~~ — sejak `LAB-DEC-187`: **implementasi** bagian `S18` yang menulis dokumen klinis pasien. Bukan keputusan bisnis Laboratorium sendirian |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r12` — 2026-10-06

Gerbang menilai ulang sembilan slice sesudah putaran 21. **Delapan slice atau bagian naik
`READY_FOR_DOMAIN_DESIGN`.** Dua penahan baru ditemukan dari pembacaan kode dan alur; tiga butir lain
dikunci sebelum kontrak tetapi tidak menahan desain. Rinciannya: `02-requirement-completeness-assessment.md`
bagian 0G.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `DEC-LAB-030` | **Bila satu order Patologi Anatomi memuat pemeriksaan dari seri nomor berbeda — misalnya histopatologi dan FNAB — seri mana yang dipakai?** Nomor cetak hari ini tersimpan **satu per order** (`LabOrder.LabReportNumber`), order hanya dipecah per disiplin (`LAB-DEC-055`), dan laporan PA satu per order (`LAB-DEC-085`). **Usulan baseline:** pemecahan otomatis `LAB-DEC-055` diperluas sehingga order PA dipecah per seri — satu order, satu laporan, satu nomor | Yoga Aji Pratama; konfirmasi `DR-LAB-003` | **Desain `S2b-2`** |
| `DEC-LAB-031` | **Bolehkah lebih dari satu tahap `Sementara` dirilis sebelum `Definitif`?** BR-68: biakan dibaca bertahap berhari-hari; lintasan `LAB-DEC-171` menyebut satu. **Usulan baseline:** boleh; setiap tahap dirilis dengan empat mata, `Definitif` menutup tahapan | Yoga Aji Pratama + `DR-LAB-002` | **Kontrak `S4d-2`.** Tidak menahan desain |
| `DEC-LAB-032` | **Bagi pemeriksaan yang dirilis `Sementara` lalu `Definitif`, rilis mana yang dipakai jumlah pemeriksaan dan TAT `S16a` serta keterlambatan cito `S7`?** Keduanya sudah dibangun dan membaca satu waktu rilis. **Usulan baseline:** rilis pertama — hasil resmi pertama (`LAB-DEC-155`) | Yoga Aji Pratama + Kepala Instalasi | **Kontrak `S4d-2`.** Tidak menahan desain; mengubah perilaku `S7`/`S16a` yang berjalan |
| `DEC-LAB-033` | **Apakah perubahan `Critical Notification SLA` dan aturan kritis Mikrobiologi/PA melewati persetujuan pengusul ≠ penyetuju yang sama dengan batas kritis (`LAB-DEC-175`)?** Memanjangkan SLA sama berbahayanya dengan menaikkan batas. **Usulan baseline:** ya | Yoga Aji Pratama + Kepala Instalasi | **Kontrak `S5`.** Tidak menahan desain |
| `DEC-LAB-034` | **Sesudah tarik hasil: pemeriksaannya berhenti dan pemeriksaan ulang dipesan baru, atau dibuka kembali? Order yang sudah *Selesai* tetap Selesai? Fakta apa yang diterbitkan untuk Billing, dan siapa menanggung pemeriksaan ulang bila kesalahannya internal?** **Usulan baseline:** pemeriksaan berhenti dan pemeriksaan ulang adalah pemeriksaan baru; order lama tetap Selesai; penarikan tidak menerbitkan fakta tagih baru — alasan koreksi membawa penanda kesalahan internal (pola `LAB-DEC-019`) yang dibaca Billing, dan koreksi uangnya tetap wewenang Billing (`LAB-INH-010`) | Yoga Aji Pratama + `billing-kasir` | **Desain `S6-2`** — tarik hasil. `S6-1` ganti nilai tidak tertahan |

### Dibuka amendment pass putaran 29 — 2026-10-09

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-REQ-023` | Bentuk kontrak `r46` dan tafsir pemeriksaan pendahulu tanpa penanda | Yoga Aji Pratama | **IMPLEMENTATION** `BE-LAB-95` |

### Dibuka amendment pass putaran 28 — 2026-10-09

**Dibuka dan ditutup putaran ini:** `LAB-CONFLICT-019` (`LAB-DEC-226`).

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| — | **Amandemen `02-backend-architecture.md` 28.8** menjadi to-be dan catatan perilaku `urgency` pada `LAB-API-v1` (tanpa ruas baru) | `/design-business-module`, disetujui Yoga Aji Pratama | **IMPLEMENTATION** perubahan `CreateExaminationsAsync` |
| — | **Hitung kering perbaikan data lama di produksi** sebelum dijalankan (dev: 0 baris) | Developer backend + Yoga Aji Pratama | Langkah rilis, bukan kode |
| — | Verifikasi peramban `AC-318`..`AC-320` butuh pesanan uji ber-CITO dari pemesanan — dev belum punya satu pun (F28-8) | Yoga Aji Pratama (izin tulis data uji) | Tidak — verifikasi |

### Dibuka amendment pass putaran 27 — 2026-10-08

**Ditutup putaran ini (sisi Laboratorium):** `LAB-CONFLICT-018`.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-COORD-020` | **Tagihan pemeriksaan Lab pasien IGD tidak pernah terbit.** Jembatan tagihan klinis Billing menolak kunjungan `Emergency` (`BillingClinicalChargeBridgeService.cs:135-140`), sehingga Lab IGD tidak tertagih sama sekali. Sejak `LAB-DEC-223` hal ini tidak lagi menahan Proses Pemeriksaan, tetapi pendapatan pemeriksaan Lab IGD hilang sampai Billing menjembatani IGD | Pemilik `billing-kasir` | Tidak bagi Laboratorium — kebocoran pendapatan milik Billing |
| — | **Amandemen kontrak** `LAB-API-v1` (nilai `Deferred`) dan `LAB-VAL-v1` (`VAL-152`/`VAL-153` dipersempit) | `/design-business-module`, disetujui Yoga Aji Pratama | **IMPLEMENTATION** penjaga jenis kunjungan |
| — | Task backend `LabPaymentClearanceRules` dan task frontend label *Ditagih Kemudian* | Developer BE/FE sesudah kontrak | Tidak bagi rawat jalan/kiosk |

### Dibuka amendment pass putaran 26 — 2026-10-08

**Ditutup putaran ini:** `LAB-OPEN-051` (prosedur manual pembayaran — kunci kini ditegakkan `LAB-DEC-218`); butir
tunda `LAB-DEC-188` *Daftar Pasien OTC*, *langsung proses*, *kunci Lunas*, *menu Hasil dan Riwayat*.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-053` | **Simpangan jalur baca Lunas sementara** (`LAB-DEC-220`): rumus disalin dari Billing; bila Billing mengubah aturan pelunasan, kolom Pembayaran dan kunci Proses dapat berbeda dari kasir. Berakhir saat `LAB-REQ-008` dijawab dan Laboratorium beralih | Pemilik `billing-kasir` + Yoga Aji Pratama | Tidak — `LATER SLICE` |
| `LAB-REQ-018` | Pemberitahuan kepada Andry Zain (`registration-management`) bahwa bagian Laboratorium pada kiosk berubah (`LAB-DEC-213`) | Yoga Aji Pratama | Tidak |
| — | **Amandemen kontrak** `LAB-API-v1`/`LAB-PERM-v1`/`LAB-STATE-v1` atas F26-9 — implementasi sudah berjalan sebelum kontrak | `/design-business-module`, disetujui Yoga Aji Pratama | Kesiapan rilis (dokumen), bukan kode |
| — | Uji Proses Pemeriksaan sesudah pelunasan di kasir — data uji `LAB-RSMMC-000023` menunggu Rp 35.000 | Yoga Aji Pratama | Tidak — verifikasi |
| — | Pembayaran pasien OTC disembunyikan, belum ditampilkan baca-saja (`AC-307`) | Developer frontend | Tidak |
| — | **Di luar scope, untuk `registration-management`:** kunjungan kiosk unit Laboratorium berstatus *Menunggu Dokter* dan berprefiks antrean `Q` | Andry Zain | Tidak |

### Dibuka amendment pass putaran 25 — 2026-10-07

**Ditutup putaran ini:** butir *grafik Beranda versi v1* `LAB-DEC-188`.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| — | **Amandemen kontrak BR-140** — `LAB-API-v1` (data ringkasan Beranda: kartu hari ini, CITO, menunggu validasi, pemeriksaan per disiplin per tahun, tren bulanan, pesanan terbaru) | `/design-business-module`, disetujui Yoga Aji Pratama | **Roadmap dan implementasi BR-140** |
| — | Pembaruan realtime Beranda (SignalR) | Yoga Aji Pratama | Tidak — ditunda `LAB-DEC-210` |

### Dibuka amendment pass putaran 24 — 2026-10-07

**Ditutup putaran ini:** nol. T1 dan T2 laporan `FE-LAB-50` diputuskan `LAB-DEC-200`..`LAB-DEC-203`.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| — | **Amandemen kontrak BR-139** — `LAB-API-v1` (daftar dokter pemeriksa milik Lab; penegasan lima ruas `r13`) | `/design-business-module`, disetujui Yoga Aji Pratama | **Roadmap, implementasi BR-139, dan seluruh rilis `MVP-12c`** (`LAB-DEC-202`) |
| — | Pesanan uji `Accepted` belum dikonfirmasi untuk mengulang `FE-LAB-50` S4 dan `AC-292` (F24-8) | Yoga Aji Pratama — izin tulis per sesi | Verifikasi akhir, bukan desain |

### Dibuka amendment pass putaran 23 — 2026-10-07

**Ditutup putaran ini:** `LAB-OPEN-027` (apakah konfirmasi wajib sebelum `Accepted`) — dijawab `LAB-DEC-194`
dalam bentuk lain: tidak wajib sebelum `Accepted`, wajib sebelum `InProcess`. `LAB-CONFLICT-017` (capability
map rev 7) — ditutup `LAB-DEC-198`.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-052` | **Hak Batalkan pesanan bagi Analis Laboratorium.** Sisa `Q-P22-03`: baris `LabOrder : Update` Analis terhapus 2026-10-01; Konfirmasi kini lewat izin tersendiri (`LAB-DEC-197`), tetapi Batalkan tetap `LabOrder : Update`. Apakah analis memang tidak boleh membatalkan, atau perlu izin Batalkan tersendiri? | Yoga Aji Pratama | Nol — Batalkan tetap dapat dilakukan pemegang `LabOrder : Update` |
| — | **Amandemen kontrak BR-138** — `LAB-STATE-v1`, `LAB-VAL-v1`, `LAB-PERM-v1`, `LAB-API-v1` | `/design-business-module`, disetujui Yoga Aji Pratama | **Roadmap dan implementasi** BR-138 |

### Dibuka amendment pass putaran 22 — 2026-10-06

Nol yang menahan penyusunan roadmap maupun implementasi frontend putaran ini (`LAB-DEC-187`).

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-051` | **Prosedur manual memastikan pembayaran pasien Tunai sebelum Proses Pemeriksaan**, selama kunci `LAB-DEC-062` ditangguhkan `LAB-DEC-189`: siapa memeriksa, di mana dicatat, dan apa yang dilakukan bila pasien ternyata belum membayar. Berakhir sendiri ketika `LAB-COORD-010` terjawab dan kuncinya dipasang | Kepala Instalasi + Yoga Aji Pratama | ✅ **Ditutup 2026-10-08** oleh `LAB-DEC-218` — kunci Lunas ditegakkan backend dan layar |

### Dibuka amendment pass putaran 21 — 2026-10-06

Seluruhnya **penahan go-live atau dependency penyediaan** — nol yang menahan penyusunan roadmap
(`LAB-DEC-187`).

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-SIGN-002` | **Ratifikasi klinis keputusan putaran 21** — tabel *Kedudukan klinis* pada Amendment Pass Putaran 21. Satu yang **wajib**: `DR-LAB-003` atas `LAB-DEC-174`, sebab validasi PA oleh penulisnya melonggarkan `LAB-DEC-003` yang ia teken. Sisanya konfirmasi | `DR-LAB-001`, `DR-LAB-002`, `DR-LAB-003`, Kepala Instalasi, Clinical Governance | **Go-live** slice yang bersangkutan. Tidak menahan desain maupun roadmap |
| `LAB-OPEN-046` | Isi daftar **kondisi kritis Patologi Anatomi** yang dipilih patolog ketika `Status Hasil PA` = Kritis (`LAB-DEC-176` butir 6) | `DR-LAB-003` | **Go-live** `S5` bagi PA |
| `LAB-OPEN-047` | Isi **`Critical Notification SLA`**: batas waktu tanggap per disiplin, tingkat eskalasi, dan penerima tiap tingkat (`LAB-DEC-176` butir 3–4) | Kepala Instalasi + Clinical Governance | **Go-live** `S5` |
| `LAB-OPEN-048` | Daftar **kategori pemeriksaan sensitif** yang membutuhkan persetujuan kedua sebelum dikirim ke pasien, dan **siapa** pemegang izin `LabResultPatientDeliveryApprover` (`LAB-DEC-182`) | Kepala Instalasi | **Go-live** `S17` bagi kategori sensitif. Hasil normal tidak tertahan |
| `LAB-OPEN-049` | Setelan **seri nomor cetak PA**: awalan, pemisah, dan panjang tiap seri, serta pemetaan kategori PA — termasuk IHK — ke seri (`LAB-DEC-170`). **Wajib final sebelum nomor Sitologi/FNAB pertama terbit** | Kepala Instalasi + `DR-LAB-003` | **Go-live** `S2b-2` |
| `LAB-OPEN-050` | Isi data induk **jenis fiksasi** dan setelan pemeriksaan mana yang membutuhkan fiksasi (`LAB-DEC-169`) | `DR-LAB-003` + Kepala Instalasi | **Go-live** `S2b-3` |
| `LAB-COORD-019` | **Penyediaan penyimpanan berkas klinis privat oleh platform** (`LAB-DEC-185`): penyimpanan di luar direktori yang dilayani web, akses terautentikasi atau tautan bertanda tangan sementara, dan audit akses. Diperiksa 2026-10-06 pada `6924b689`: nol layanan bersama | Pemilik platform | **Implementasi** bagian gambar `S4c`, berkas `S18`, dan PDF `S17`. Tidak menahan desain maupun roadmap |

### Dibuka amendment pass putaran 18 — 2026-09-25

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-045` | **Prosedur bila hasil yang SUDAH DIRILIS ternyata keliru, selama koreksi `S6` belum berdiri.** Sistem tidak dapat mengubah maupun menarik hasil terrilis — hasil terkunci sesudah rilis, dan pembatalan pemeriksaan ditolak `VAL-143`. Yang perlu ditetapkan: siapa memberi tahu dokter pemesan, dengan cara apa, bagaimana kekeliruan dicatat sampai `S6` ada, dan siapa berwenang menyatakannya keliru. **Contoh:** Kalium 6,4 dirilis pukul 10.00; pukul 11.00 ketahuan tabungnya milik pasien lain — angka itu tetap tercatat resmi di rekam medis pasien yang salah sampai ada prosedur. Sisa pertanyaan `04-prd-to-mvp.md` 21.7 sesudah `LAB-DEC-155` | Yoga Aji Pratama + `DR-LAB-001`; `DR-LAB-002` untuk Mikrobiologi | **Diusulkan** menahan langkah 4 `MVP-9d` dan langkah 3 `MVP-10c` — sama dengan usulan semula atas pertanyaan induknya. **Tidak** menahan pengembangan |

### Dibuka amendment pass putaran 17 — 2026-09-25

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-OPEN-044` | **Isi *aturan laboratorium* tentang kewenangan otorisasi (rilis)** yang dirujuk surat dr. Bima (`LAB-EVD-010` butir 3): jabatan mana yang menjadi **calon perilis**, dan siapa yang **menetapkan** perilis per disiplin. Keputusan bahwa perilis tidak wajib dokter sudah diambil (`LAB-DEC-153`); yang belum ada adalah **daftarnya**. **Contoh kenapa perlu:** admin yang akan memberi aksi rilis tidak tahu apakah *penanggung jawab mutu laboratorium* termasuk; menebaknya berarti memberi kewenangan mengesahkan hasil pasien atas dugaan | dr. Bima Prasetya, Sp.PK | **Langkah rilis `MVP-9d` saja** — pemberian aksi rilis dan pencatatan kode rilis (`UNK-P14-03`). **Tidak** memblokir desain maupun pengembangan |

### Dibuka arsitektur domain `LAB-DA-001` revision 8 — 2026-09-24, diperiksa 2026-09-25

Satu butir. Ia muncul ketika perjalanan pemeriksaan sesudah rilis dimodelkan: arsitektur harus
menjawab apa yang terjadi bila pemeriksaan yang sudah dirilis dibatalkan, dan ternyata dua
keputusan lama memberi arah yang berbeda.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-019`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-179`** — pembatalan sesudah rilis ditolak; salah pasien ditarik lewat koreksi `S6` jenis *tarik hasil*. Isi aslinya: **Bolehkah pemeriksaan yang hasilnya SUDAH DIRILIS dibatalkan, atau penarikannya selalu lewat koreksi?** `LAB-DEC-138` menyatakan hasil yang sudah dirilis *"tetap hanya lewat koreksi"*, dan `LAB-DEC-063` menolak pembatalan **pesanan** pada `InProcess` dan `Completed`. Tetapi `LAB-DEC-049` membiarkan pembatalan **pemeriksaan** terbuka *"sesudah kelayakan ditetapkan"* tanpa batas atas, karena ditulis sebelum rilis dirancang. **Contoh:** Kalium pasien Andi dirilis pukul 10.00, lalu pukul 11.00 ketahuan tabungnya milik pasien lain. Apakah pemeriksaan itu dibatalkan (hasilnya hilang dari rekam medis Andi), atau dikoreksi `S6` (versi lama tetap terlihat bertanda *"sudah diperbaiki"*)? **Arah yang berlaku sampai dijawab:** pembatalan sesudah rilis **ditolak** (`ARCH-GAP-LAB-09`). Arah ini diturunkan dari `LAB-DEC-138`, bukan pilihan baru | Yoga Aji Pratama | **Desain `S6`.** Tidak memblokir `S4` karena arah yang aman sudah berlaku |

### Dibuka gerbang kelengkapan requirement `LAB-RCG-001-r8` — 2026-09-24

Gerbang menilai ulang `S4` sesudah `LAB-DEC-150` dan menaikkannya
**`READY_FOR_DOMAIN_DESIGN` untuk desain saja**. Pemakaian nyatanya tertahan dua butir baru di
bawah, ditambah sisa `DEC-LAB-011`. Rinciannya: `02-requirement-completeness-assessment.md`
bagian 0C.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `DEC-LAB-017` | **Bolehkah `S4` dipakai sebelum `S5` pelaporan nilai kritis berdiri**, dan bila boleh, prosedur manual apa yang menggantikan formulir pelaporan serta daftar pantau selama itu? `LAB-DEC-004` — diteken pihak klinis — mengandaikan keduanya ada saat hasil kritis dirilis | Yoga Aji Pratama + `DR-LAB-001` | **Pemakaian nyata `S4`.** Tidak memblokir desainnya |
| ~~`DEC-LAB-018`~~ | ✅ **DITUTUP 2026-09-25** oleh **`LAB-DEC-153`** — perilis **tidak wajib dokter**; pemegang kewenangan otorisasi yang ditetapkan lewat kode rilis di kredensial Human Resource. Jabatan calonnya menunggu `LAB-OPEN-044`, yang menahan **langkah rilis** saja. Isi aslinya: **Siapa pemegang kewenangan rilis (otorisasi) hasil Patologi Klinik, dan apakah wajib dokter?** `LAB-DEC-150` menjawab validasi saja, padahal `LAB-DEC-120` mewajibkan perilis berbeda dari pemvalidasi. Ditambahkan ke `LAB-REQ-014` | dr. Bima Prasetya, Sp.PK | **Pemakaian nyata `S4`.** Tidak memblokir desainnya |

### Dibuka arsitektur domain `LAB-DA-001` revision 5 — 2026-09-18

Dua butir, dan keduanya hanya muncul ketika konsepnya benar-benar dimodelkan — bukan ketika
keputusannya dinilai. Itu memang urutan yang dirancang; arsitektur berdiri sesudah gerbang
justru untuk menangkap yang seperti ini.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-LAB-015`~~ | ✅ **Ditutup 2026-09-18** oleh `LAB-DEC-084` — keduanya menjadi data induk terkendali **milik Laboratorium**, berprefix `Lab`. `S4b` terbuka. Isi aslinya: **Apakah organisme dan antibiotik merupakan data induk terkendali, dan siapa pemiliknya?** BR-23 memerinci bentuk hasil Mikrobiologi berstruktur sampai ke zona hambat dalam milimeter, tetapi **tidak pernah menyatakan** apakah nama kuman dan nama antibiotik diambil dari daftar yang dikelola atau diketik bebas. Bila bebas, satu kuman yang sama akan tertulis `E. coli`, `E.coli`, `Escherichia coli`, dan `eschericia coli` — **empat tulisan, satu kuman** — dan **antibiogram rumah sakit mustahil disusun**. Alasannya **sama persis** dengan yang dipakai `LAB-DEC-082` menutup bentuk alasan koreksi. Pemiliknya pun belum jelas: panel antibiotik uji kepekaan **bukan** formularium farmasi | Yoga Aji Pratama + pemilik `master-data`; panel antibiotiknya kemungkinan `DR-LAB-002` | **`BLOCKING`** bagi `S4b`. Menentukan identitas konsep, invariant, bentuk jejak audit, dan mungkin-tidaknya pelaporan |
| ~~`DEC-LAB-016`~~ | ✅ **DITUTUP 2026-10-06** oleh **`LAB-DEC-185`** — penyimpanan berkas klinis privat, metadata di basis data, akses terautentikasi atau tautan bertanda tangan sementara, berkas lama tidak dihapus. **Penyediaannya `LAB-COORD-019`**, milik platform. Isi aslinya: **Diperluas 2026-09-25 oleh `LAB-RCG-001-r10`: kini juga menahan penyimpanan PDF hasil laboratorium eksternal (`S18`, `DEC-LAB-026`).** **Di mana gambar hasil Patologi Anatomi disimpan, dan bolehkah ia terlayani web?** Penelusuran 2026-09-18 menemukan unggahan berkas **sudah berjalan** di beberapa area — `WfpDocumentController`, `ProfileController`, `LeaveRequestController` — tetapi masing-masing memakai `SaveFileAsync` privatnya sendiri di atas `IWebHostEnvironment`; **nol layanan penyimpanan bersama**. Gambar patologi adalah **data klinis yang dapat mengidentifikasi pasien**; menyimpannya di bawah direktori yang dilayani web adalah keputusan **privasi**, bukan keputusan teknis. Sekelas dengan penandaan `LAB-DEC-030` atas pengiriman hasil ke kanal pihak ketiga | Pemilik platform + Yoga Aji Pratama | **`BLOCKING`** bagi bagian **gambar** `S4c`. **Tidak** memblokir laporan narasinya |

**Yang sengaja ditinggalkan di luar scope** oleh `LAB-DEC-037`, dan **bukan** merupakan open
question modul ini: pendaftaran RS perujuk sebagai data induk yang disahkan sendiri oleh
Laboratorium, penerimaan uang tunai di laboratorium, promo, status PKS, penyimpanan pasien
baru beserta Tanggal Registrasi dan Kategori/Title dari usia, serta kewajiban pemindaian KTP
di seluruh titik pendaftaran rumah sakit. Keenamnya diserahkan ke modul pemiliknya.

---

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
> **Perubahan besar sejak revision 13.** Analisis konsolidasi bukti lapangan diadopsi sebagai
> baseline requirement (`LAB-DEC-025` sampai `LAB-DEC-031`). Cakupan modul **bertambah tiga
> kali lipat**, dan delapan hal tata kelola yang sebelumnya tidak terlihat kini terbuka sebagai
> `LAB-P0-001` sampai `LAB-P0-008`. Daftar di bawah sudah memperhitungkannya.

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-P0-001` | Matriks kewenangan per peran | Yoga Aji Pratama + Clinical Governance | `DESIGN` — seluruh slice hasil dan pembatalan |
| ~~`LAB-P0-002`~~ | Urutan status resmi, termasuk posisi `Confirmed` | Yoga Aji Pratama | ✅ **Ditutup 2026-09-18** oleh `LAB-DEC-080` — validasi dan rilis dicatat sebagai fakta, nol status hasil. Bagian lintas aplikasi tetap `LAB-P0-008` |
| ~~`LAB-P0-003`~~ | Aturan pembatalan dan koreksi | Yoga Aji Pratama + Billing | ✅ **Ditutup 2026-10-06** — sebagian oleh `LAB-DEC-082` (2026-09-18), sisanya oleh `LAB-DEC-178`..`180` yang menutup `DEC-LAB-014` |
| ~~`LAB-P0-004`~~ | Alur nilai kritis lengkap | Yoga Aji Pratama + Clinical Governance | ✅ **Arsitektur ditutup 2026-10-06** oleh `LAB-DEC-176`/`177`. Isi klinis — SLA dan eskalasi — `LAB-OPEN-047`, penahan go-live |
| `LAB-P0-005` | Integrasi alat laboratorium | Yoga Aji Pratama + pemilik platform | `DESIGN` — slice hasil Patologi Klinik |
| `LAB-P0-006` | Kebijakan jejak audit | Yoga Aji Pratama | `IMPLEMENTATION` |

### Koordinasi lintas modul — disetujui 2026-09-01

Lima butir ditutup lewat `LAB-REQ-001`, disetujui `andryzainhome` (`andryzain01@gmail.com`) dan
`sukmagp` — Sukma Giri Pratama (`sukmagiri11@gmail.com`) selaku pemilik repository.

| ID | Isi | Membuka |
|---|---|---|
| `LAB-COORD-001` | Pemberitahuan sebagai kemampuan platform | Prasyarat `S5` dan `S8` |
| `LAB-COORD-002` | Jenis dokumen klinis baru pada `rekam-medis` | Prasyarat `S6`, `S9`, `S18` |
| `LAB-COORD-003` | Kontrak pemanggilan Registrasi | `MVP-1` |
| `LAB-COORD-004` | Data induk perujuk + kolom penunjuk pada kunjungan | `MVP-1` |
| `LAB-COORD-005` | Kolom disiplin pada `MstProcedure` | `MVP-0` |

**Yang tetap terbuka meski permintaannya disetujui:**

| ID | Kenapa persetujuan belum cukup |
|---|---|
| `LAB-OPEN-012` | Yang dibutuhkan adalah **satu angka** — jumlah baris `TrxLabSpecimen` di produksi. Persetujuan tidak memberi tahu berapa |
| `LAB-OPEN-018` | Lokasi dokumen sudah diketahui, tetapi rules root yang **terpasang** belum memuatnya. Yang dibutuhkan adalah publikasi/pembaruan suite Skill, bukan persetujuan |
| `LAB-OPEN-019` | Yang dibutuhkan adalah **kenaikan lifecycle registry** `PLANNED` → `ACTIVE` untuk `LaboratoryManagement`, dan itu wewenang pemilik repository backend |
| ~~`LAB-SIGN-001`~~ | ✅ **DITUTUP 2026-09-17** oleh `LAB-DEC-079` — ditandatangani per disiplin oleh `DR-LAB-001`, `DR-LAB-002`, dan `DR-LAB-003`, sesudah `LAB-DEC-078` menetapkan ketiganya pada hari yang sama. Catatan asal, dipertahankan karena menjelaskan kenapa ia bertahan 16 hari: pemilik repository **bukan** wewenang klinis, dan `LAB-DEC-011` yang disetujui pemilik modul sendiri mensyaratkan tanda tangan dokter penanggung jawab laboratorium atau Komite Medis — jabatan yang saat itu belum pernah diisi |
| `LAB-P0-007` | Aturan tagihan dan cakupan | Billing | `DESIGN` — tampilan tarif dan cakupan |
| `LAB-P0-008` | Penyelarasan antaraplikasi | Yoga Aji Pratama + pemilik platform | `DESIGN` — batas integrasi |
| `LAB-OPEN-013` | Dampak Cito dan Duplo pada tarif | Yoga Aji Pratama + Billing | `DESIGN` — bagian penandaan pemeriksaan |
| ~~`LAB-OPEN-014`~~ | Nilai kritis untuk hasil mikrobiologi dan patologi anatomi | Yoga Aji Pratama + Clinical Governance | ✅ **Arsitektur ditutup 2026-10-06** oleh `LAB-DEC-176` — data induk aturan per disiplin. Isi: Mikrobiologi `LAB-OPEN-041`, PA `LAB-OPEN-046` — penahan go-live |
| `LAB-COORD-003` | Kesepakatan kontrak pemanggilan Registrasi dari Laboratorium | Yoga Aji Pratama + pemilik `registration-management` | `DESIGN` — slice pendaftaran `S13` |
| `LAB-OPEN-017` | Makna penanda Definitif | Yoga Aji Pratama | `LATER SLICE` |
| `DEC-LAB-009` | Di mana identitas dokter dan instansi perujuk disimpan? | Yoga Aji Pratama + pemilik `registration-management` | `DESIGN` — memblokir `S13b` pendaftaran rujukan luar |
| `DEC-LAB-010` | Bagaimana disiplin melekat pada jenis pemeriksaan? `MstProcedure` hanya punya `IsLaboratory` tanpa pembeda disiplin | Yoga Aji Pratama + pemilik `master-data` | `DESIGN` — memblokir penegakan `INV-22` |
| `DEC-LAB-008` | Apakah satu wadah fisik dapat melayani beberapa pemeriksaan? | Yoga Aji Pratama + kepala instalasi laboratorium | **Ditutup** `LAB-DEC-024` |
| ~~`LAB-SIGN-001`~~ | Tanda tangan klinis untuk `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007`, sesuai `LAB-DEC-011` | `DR-LAB-001` dr. Aditya Pranata, Sp.PK (Patologi Klinik), `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK (Mikrobiologi Klinik), `DR-LAB-003` dr. Citra Maharani, Sp.PA (Patologi Anatomi) — ditetapkan `LAB-DEC-078` | ✅ **DITUTUP 2026-09-17** oleh `LAB-DEC-079`, ditandatangani **per disiplin**, nol perubahan bunyi. Penahan tertua modul ini, terbuka sejak 2026-09-01. **Penutupannya tidak membuat kelima slice langsung dapat dibangun** — `LAB-REQ-004` bagian 7.2 sudah memperingatkan empat hal yang tetap tertahan: `LAB-P0-004`, `LAB-OPEN-014`, `LAB-P0-001`, dan kedua penetapan bagian 5. Ditambah `LAB-OPEN-034` yang lahir dari bentuk tanda tangannya |
| `LAB-AMD-001` | Amandemen `LAB-INH-001` dan `LAB-INH-006` pada blueprint `rawat-jalan`: `Draft` dihapus, batas kewenangan dokter menjadi "sampai sampel pertama diambil" | Pemilik blueprint `rawat-jalan` + Billing | `DESIGN` — **hanya bagian pembuatan dan penyuntingan pesanan** |
| `LAB-COORD-001` | Kesepakatan dengan pemilik platform soal kemampuan pemberitahuan bersama | Yoga Aji Pratama + pemilik platform | `DESIGN` — **hanya bagian pemberitahuan** |
| `LAB-COORD-002` | Kesepakatan dengan pemilik `rekam-medis` untuk menambah jenis dokumen klinis baru | Yoga Aji Pratama + pemilik `rekam-medis` | `DESIGN` — **hanya bagian penyajian hasil ke rekam medis** |
| `LAB-OPEN-018` | Rules root terpasang belum memuat `rules/backend/engineering/`; gerbang `AGENTS.md` memaksa `BLOCKED — canonical governance unavailable` | Pemilik repository backend + pemilik suite Skill | `IMPLEMENTATION` |
| `LAB-OPEN-019` | Lifecycle registry `LaboratoryManagement` masih `PLANNED`; `QBE-MOD-002`/`QBE-MOD-003` menahan entity `Lab*` pertama | Pemilik repository backend | `IMPLEMENTATION` |

Keempat butir `DESIGN` di atas bersifat **sebagian**: masing-masing hanya menahan bagian yang
disebutkan, bukan seluruh desain. Bagian lain — siklus hidup sampel, batas nilai, cito, daftar
kerja, dan batas kewenangan finansial — sudah bebas hambatan.

Seluruh open question wawancara sudah tertutup pada sesi ini:

| ID | Cara ditutup |
|---|---|
| `LAB-SCOPE-001` | Digantikan `LAB-DEC-001` dan `LAB-DEC-002` |
| `LAB-OPEN-001` | Pemilik modul ditetapkan: Yoga Aji Pratama |
| `LAB-OPEN-003` | Ditutup `LAB-DEC-002` — bank darah di luar scope |
| `LAB-OPEN-004` | Ditutup `LAB-DEC-014` — reagen di luar scope |
| `LAB-OPEN-005` | Ditutup `LAB-DEC-007` — kewenangan koreksi hasil |
| `LAB-OPEN-007` | Ditutup `LAB-DEC-013` — aturan cito |
| `LAB-OPEN-008` | Ditutup `LAB-DEC-012` — pemberitahuan tersimpan |
| `LAB-OPEN-009` | Ditutup `LAB-DEC-011` — wewenang klinis terpisah |
| `LAB-CONFLICT-001` | Diselesaikan `LAB-DEC-006` — tabel batas nilai ditarik ke Rilis 1 |
| `LAB-OPEN-011` | Ditutup `LAB-DEC-020` — koreksi setelah kunjungan ditutup memakai addendum |
| `LAB-CONFLICT-002` | Diselesaikan `LAB-DEC-015` — status `Draft` dihapus, dokter menyunting sampai sampel diambil |
| `Q-LAB-01` sampai `Q-LAB-05` | Pertanyaan penutup dari capability map, ditutup `LAB-DEC-015` sampai `LAB-DEC-019` |

Yang tersisa bukan pertanyaan wawancara, melainkan dua hal administratif: tanda tangan klinis
dan kelengkapan dokumen tata kelola repository.

### Catatan approval

**Pemilik modul:** Yoga Aji Pratama (`yogaaji452@gmail.com`), ditetapkan 2026-09-01.

Keputusan `LAB-DEC-001` sampai `LAB-DEC-036` berstatus `approved` oleh pemilik modul, kecuali `LAB-DEC-002` dan `LAB-DEC-021` yang `superseded` serta `LAB-DEC-013` yang `amended`
pada 2026-09-01. Yang perlu dicatat jujur tentang persetujuan ini:

1. Persetujuan diberikan lisan dalam sesi wawancara, bukan lewat dokumen bertanda tangan.
   Bila rumah sakit memerlukan bukti tertulis untuk keperluan audit atau akreditasi, dokumen
   ini perlu dicetak dan ditandatangani.
2. Pemilik modul menyatakan lewat `LAB-DEC-011` bahwa **wewenang klinis berada di pihak lain**.
   Karena itu tiga keputusan berikut sudah disetujui dari sisi produk dan operasional, tetapi
   masih **menunggu tanda tangan dokter penanggung jawab laboratorium atau Komite Medis**:

   | Keputusan | Isi | Kenapa perlu tanda tangan klinis |
   |---|---|---|
   | `LAB-DEC-003` | Prinsip empat mata pada validasi hasil | Menentukan siapa yang boleh menyatakan sebuah angka hasil benar |
   | `LAB-DEC-004` | Nilai kritis tetap dirilis, pelaporan wajib tercatat | Menentukan apa yang terjadi ketika pasien dalam bahaya |
   | `LAB-DEC-007` | Kewenangan koreksi hasil dan pemberitahuan ke dokter | Menentukan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah |

   Dicatat sebagai `LAB-SIGN-001`. Ini **tidak** memblokir seluruh desain — hanya bagian
   validasi hasil, nilai kritis, dan koreksi hasil yang harus menunggu. Permintaan tanda tangannya
   diajukan 2026-09-09 sebagai `LAB-REQ-004` pada `approval-requests/2026-09-09-permintaan-tanda-tangan-klinis.md`,
   yang sekaligus meminta penetapan **siapa** pemegang wewenang klinis — sampai hari ini
   `blueprint-manifest.md` masih mencatatnya `belum ditetapkan`.
3. Keputusan warisan `RJ-BIL-GATE-DEC-003` berstatus `locked-draft` dengan tata kelola formal
   `OPEN` di blueprint `rawat-jalan`. Statusnya tidak berubah oleh sesi ini.

---


### Dibuka Amendment Pass putaran 4 — 2026-09-15, pendaftaran lewat kiosk

| ID | Hal yang belum selesai | Pemilik | Memblokir |
|---|---|---|---|
| ~~`LAB-COORD-008`~~ | ✅ **Ditutup 2026-09-15** oleh `LAB-REQ-006`. Andry Zain menyetujui kiosk bertambah bagian Laboratorium: sesi kiosk boleh membawa tujuan layanan dan jalur permintaan dokter, secara **aditif** | — | — |
| ~~`LAB-COORD-009`~~ | ✅ **Ditutup 2026-09-15** oleh `LAB-REQ-006` dan `LAB-DEC-058`. Kunjungan kiosk yang tidak dilanjutkan ditutup **saat hari layanan berakhir** oleh Registrasi, dan **biaya pendaftarannya gugur**. `AC-45` tidak dicabut | — | — |
| `LAB-CONFLICT-007` | **`AC-83` hanya ditegakkan frontend.** `LabOrderService` menyalin `request.Discipline` apa adanya tanpa menurunkannya dari katalog, dan ruas itu tidak wajib — sehingga pemanggil mana pun di luar layar tersebut masih dapat membuat pesanan tanpa disiplin. Terbukti pada data: **2 dari 5 pesanan tidak berdisiplin** dan hilang dari ketiga menu | Yoga Aji Pratama | `IMPLEMENTATION` — bukan gap desain; `LAB-DEC-055` sudah menjawab arahnya |

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 94 | 2026-10-09 | **Amendment pass putaran 29 — rencana ulang sesudah wadah dibatalkan.** Menutup temuan T1 `BE-LAB-94`: `LAB-DEC-229` rencana ulang mewarisi keadaan terakhir pemeriksaan pendahulu beserta penanda (sama dengan `LAB-DEC-227`), tanpa pendahulu → permintaan. `AC-323`..`AC-325`. Tafsir pendahulu tanpa penanda diajukan lewat `LAB-REQ-023` | `draft` |
| 93 | 2026-10-09 | **Amendment pass putaran 28 — CITO dari pemesanan hilang saat wadah direncanakan.** Membukukan dan menutup `LAB-CONFLICT-019` (`02-backend-architecture.md` 28.8). Fakta F28-1..F28-8 dibaca pada BE `e6c0e451`/FE `8f3d74d18`, termasuk cacat kedua: wadah pengganti juga menghapus *Tandai Cito* (F28-3). Batas scope dan lima pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-225` batas scope; `LAB-DEC-226` pemeriksaan mewarisi CITO permintaan, penanda kosong; `LAB-DEC-227` wadah pengganti mewarisi keadaan terakhir beserta penanda; `LAB-DEC-228` perbaikan data lama hanya pekerjaan berjalan, satu baris riwayat, idempoten. `AC-318`..`AC-322` | `draft` |
| 92 | 2026-10-08 | **Amendment pass putaran 27 — lingkup kunci Lunas Proses Pemeriksaan.** Menjawab closure question `Q-LAB-P26-01` (capability map revision 8, `LAB-CONFLICT-018`). Batas scope dan tiga pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-222` batas scope; `LAB-DEC-223` kunci Lunas dikecualikan hanya bagi Rawat Inap dan IGD, jenis lain termasuk MCU/Telemedicine/`Unknown` tetap dikunci (fail-closed) — **mengamandemen `LAB-DEC-218`**; `LAB-DEC-224` status baru `Deferred` *Ditagih Kemudian* bagi pasien Tunai rawat inap/IGD. `AC-314`..`AC-317`. `LAB-CONFLICT-018` diselesaikan sisi Laboratorium; `LAB-COORD-020` dibuka (tagihan Lab IGD tidak terbit, milik Billing). **Menuntut amandemen `LAB-API-v1` dan `LAB-VAL-v1`** | `draft` |
| 91 | 2026-10-08 | **Amendment pass putaran 26 — alur Lab dari kiosk sampai hasil mengikuti FE v1.** Mencatat enam keputusan pemilik modul `LAB-EVD-013` (2026-10-07/08) yang sudah diimplementasikan atas arahan langsung: `LAB-DEC-213` kiosk v1 (Konfirmasi Kehadiran atau pendaftaran kunjungan) — **`LAB-DEC-052` diamandemen**, layar `FE-LAB-13` dicabut; `LAB-DEC-214` Penerimaan satu Simpan — **`LAB-DEC-045` diamandemen**; `LAB-DEC-215` Daftar Pasien OTC; `LAB-DEC-216` Terima Sampling dari daftar — **`LAB-DEC-191` digantikan**, `LAB-DEC-060` dipulihkan; `LAB-DEC-217` Hasil dan Riwayat; `LAB-DEC-218` kunci Lunas pada Proses — **`LAB-DEC-189` diamandemen, `LAB-DEC-062` dipasang kembali, `LAB-OPEN-051` ditutup**; `LAB-DEC-219` `VAL-09` dipertahankan. Batas scope dan tiga pertanyaan lanjutan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-212`; `LAB-DEC-220` jalur baca Lunas sementara milik Lab; `LAB-DEC-221` pembayaran dipilih saat kunjungan baru — **`LAB-FE-011` dan `LAB-DEC-044` diamandemen**. **Koreksi:** `AC-45` tidak tergeser (F26-1) — klaim pada `LAB-EVD-013` diperbaiki. BR-141, `LAB-FE-036`, `AC-302`..`AC-313`; dibuka `LAB-OPEN-053`, `LAB-REQ-018`. Kontrak belum diamandemen (F26-9) | `draft` |
| 90 | 2026-10-07 | **Amendment pass putaran 25 — Beranda Lab mengikuti FE v1.** Permintaan langsung pemilik modul atas butir *grafik Beranda versi v1* `LAB-DEC-188`. Batas scope dan sebelas pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-204` seluruh susunan v1 dengan tema V2, rekap status tetap; `LAB-DEC-205` hari ini WIB, *Menunggu* tanpa batal; `LAB-DEC-206` grafik menghitung pemeriksaan pada tahun terpilih; `LAB-DEC-207` tren pesanan non-batal per bulan; `LAB-DEC-208` *Petugas* diganti *Hasil menunggu validasi*; `LAB-DEC-209` 10 pesanan terbaru; `LAB-DEC-210` tanpa realtime; `LAB-DEC-211` akses `LabOrder : Read`. BR-140, `LAB-FE-035`, `AC-294`..`AC-301` ditulis. **Menuntut amandemen `LAB-API-v1`** — belum dilakukan. Fakta dibaca pada BE `f17cb984` + working tree, FE `70dd4d4e0` + working tree, dan FE v1 | `draft` |
| 89 | 2026-10-07 | **Amendment pass putaran 24 — dokter pemeriksa bagi pengonfirmasi dan jawaban detail konfirmasi.** Dua temuan verifikasi `FE-LAB-50` dengan akun analis asli: T1 jawaban detail tanpa jejak konfirmasi `r13`, T2 pemilih dokter dijaga `KioskRead` → `403` bagi analis. Batas scope dan empat pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-200` semua dokter aktif; `LAB-DEC-201` daftar milik Lab dijaga `LabOrder : Confirm`, isi terbatas; `LAB-FE-034` nilai awal kosong; `LAB-DEC-202` rilis serempak `MVP-12c`; `LAB-DEC-203` kesesuaian `r13`. BR-139 dan `AC-289`..`AC-293` ditulis. **Menuntut amandemen `LAB-API-v1`** — belum dilakukan. Fakta dibaca pada BE `f17cb984` + working tree `BE-LAB-90`, FE `70dd4d4e0` + working tree `FE-LAB-50`, dan FE v1 | `draft` |
| 88 | 2026-10-07 | **Amendment pass putaran 23 — urutan Terima Sampling dan Konfirmasi mengikuti FE v1.** Permintaan langsung pemilik modul atas butir yang ditunda `LAB-DEC-188`. Lima pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-193` Konfirmasi sah sebelum maupun sesudah wadah layak sampai pesanan diproses — **`LAB-DEC-061` diamandemen**; `LAB-DEC-194` Proses Pemeriksaan wajib didahului Konfirmasi — **`LAB-DEC-189` diamandemen, `LAB-OPEN-027` ditutup**; `LAB-DEC-195` Konfirmasi pada `Accepted` tidak mengubah status; `LAB-DEC-196` syarat konfirmasi hanya saat Proses; `LAB-DEC-197` hak Konfirmasi menjadi aksi izin tersendiri bagi Analis. **Dicatat** jawaban 2026-10-07 atas closure question capability map rev 7: `LAB-DEC-198` (`Q-P22-01`, bunyi `AC-278` diamandemen, `LAB-DEC-190` diamandemen, `LAB-CONFLICT-017` ditutup), `LAB-DEC-199` (`Q-P22-02`), `LAB-FE-032` (subjudul Nota); sisa `Q-P22-03` dibuka sebagai `LAB-OPEN-052`. BR-138, `LAB-FE-033`, `AC-283`..`AC-288` ditulis; `LAB-DEC-188` diberi catatan. **Menuntut amandemen `LAB-STATE-v1`, `LAB-VAL-v1`, `LAB-PERM-v1`, `LAB-API-v1`** — belum dilakukan. Fakta dibaca pada BE `f17cb984` + FE `70dd4d4e0` dan FE v1 | `draft` |
| 87 | 2026-10-06 | **Amendment pass putaran 22 — menu dan aksi alur kerja FE v1 pada daftar pasien lab.** Permintaan langsung pemilik modul; FE v1 (`C:\Users\Admin\QuilvianSystemFrontendDev`) dibaca sebagai rujukan alur, bukan evidence. Lima pertanyaan, seluruhnya pilihan yang direkomendasikan: `LAB-DEC-188` batas scope (hanya frontend); `LAB-DEC-189` Proses Pemeriksaan aktif bagi semua penjamin — **`LAB-DEC-062` diamandemen**, kunci Lunas ditangguhkan; `LAB-DEC-190` Selesaikan hanya PK/Mikro bila semua dirilis, Tahan/Lanjutkan tidak ditawarkan; `LAB-DEC-191` Terima Sampling membuka layar Wadah — **`LAB-DEC-060` diamandemen**; `LAB-DEC-192` susunan menu mengikuti v1 — **`LAB-DEC-048` diamandemen pada label**. `LAB-FE-030`, `LAB-FE-031`, BR-136, BR-137, `AC-276`..`AC-282` ditulis; `LAB-OPEN-051` dibuka; sembilan butir bahan audit diteruskan ke `/trace-existing-capabilities`. Nol perubahan kontrak dan nol migration. Fakta dibaca pada BE `171dc314` + FE `5427ddbe4` | `draft` |
| 86 | 2026-10-06 | **Lima decision ID baru dari gerbang `LAB-RCG-001-r12`, dicatat tanpa keputusan baru.** `DEC-LAB-030` — order PA campuran seri nomor (nomor cetak satu per order), menahan `S2b-2`; `DEC-LAB-034` — akibat tarik hasil pada pemeriksaan, order, dan tagihan, menahan `S6-2`; `DEC-LAB-031`, `032`, `033` — dikunci sebelum kontrak `S4d-2`/`S5`, tidak menahan desain. Gerbang yang sama menaikkan delapan slice atau bagian ke `READY_FOR_DOMAIN_DESIGN` | `DRAFT` |
| 85 | 2026-10-06 | **Amendment pass putaran 21 — keputusan pembuka roadmap yang tertahan (`LAB-EVD-012`).** Teks tertulis pemilik modul 14 butir ditambah enam klarifikasi pilihan, seluruhnya pilihan yang direkomendasikan. `LAB-DEC-168`..`187` menutup `DEC-LAB-012`, `014`, `016`, `019`, `020`, `021`, `022`, `028`, `029`, `LAB-P0-003`, `LAB-P0-004`, `LAB-OPEN-014`, dan `LAB-OPEN-029`; `DEC-LAB-025` ditunda ke Phase 2; `LAB-COORD-011`/`015`/`018` menyempit. **Empat persinggungan dengan keputusan terkunci diselesaikan lewat klarifikasi:** pengusul ≠ penyetuju nilai kritis (BR-19 tetap tegak), acknowledgement = baca ulang (`LAB-DEC-004` utuh), order belum Selesai selama `Sementara`, dan pemberitahuan saat `Definitif`. **Amandemen yang disengaja:** `LAB-DEC-003` bagi PA (empat mata pindah ke rilis), `LAB-DEC-067`/`139` (syarat bergelar), `LAB-DEC-117`/`162` (seri nomor), `LAB-DEC-163` (satu titik). Dibuka `LAB-SIGN-002` (ratifikasi klinis — **wajib** bagi `LAB-DEC-174`), `LAB-COORD-019` (penyimpanan privat), dan `LAB-OPEN-046`..`050` (konten go-live). `AC-257`..`AC-275` | `DRAFT` |
| 84 | 2026-10-06 | **`LAB-DEC-167` disetujui** atas [`LAB-REQ-015`](approval-requests/2026-10-06-permintaan-spesifik-specimen-pada-respons-specimen.md) — pilihan A ketiga butir: Spesifik Specimen pada respons specimen (`by-order`), penggantian berdasar selisih, dan `VAL-150`. `LAB-API-v1` `r39` bagian 34; `LAB-VAL-v1` `r16`; `BE-LAB-88` | `DRAFT` |
| 83 | 2026-10-01 | **Dua keputusan pemilik modul sesudah `FE-LAB-36`.** `LAB-DEC-165` menutup `FR-14.9`: hasil pilihan di luar rujukan ditulis `+2 — Di luar rujukan` (rekomendasi disetujui langsung). `LAB-DEC-166`: rincian order membawa identitas pasien — `LAB-API-v1` `r38` bagian 33, `BE-LAB-87` | `DRAFT` |
| 82 | 2026-09-30 | **Amendment pass putaran 20 — urutan langkah kiosk Pasien Lama, menutup `KSK-OQ-004` blueprint `kiosk`.** `KSK-DEC-002`/`014` (Sukma Giri Pratama, `approved` 30 Sep 2026) memindahkan pilihan Tujuan Layanan ke langkah ketiga (sesudah Identifikasi dan Review Data) dan membentuk sesi kiosk tepat sekali saat tujuan dipilih. Bunyi lama perilaku `FE-LAB-13` dicatat berdampingan dengan bunyi baru; BR-46, `LAB-DEC-051`/`052`, tiga jawaban pemilik modul `FE-LAB-13`, kontrak `scan-result`, dan `FE-LAB-14` diperiksa **tidak berubah**. `AC-93` tidak diubah bunyinya; tafsirnya dicatat. Pemilik modul Laboratorium perlu diberi tahu | `draft` |
| 81 | 2026-09-25 | **Dua pertentangan dan satu unknown dari capability map revision 6, dicatat tanpa keputusan baru.** `LAB-CONFLICT-015` — tiga jalan keluar berkas salah pasien pada dokumen klinis pasien lawan `LAB-DEC-158`; `LAB-CONFLICT-016` — satu kode izin `Update` untuk seluruh tindakan keadaan dan verifikasi-sendiri saat membuat. Keduanya bahan `LAB-COORD-018`, bukan keputusan Laboratorium. `UNK-P19-01` — kunjungan MCU tidak dapat dibuat dari layar mana pun, sehingga syarat penutupan `S19` belum dapat dibuktikan lewat layar. **Satu koreksi faktual:** asal dokumen klinis pasien `9d38d30a` (2026-06-02), bukan `58c61a5b` | `draft` |
| 80 | 2026-09-25 | **Dua decision ID baru dari gerbang `LAB-RCG-001-r11`, dicatat tanpa keputusan baru.** `DEC-LAB-028` — fiksasi specimen PA, ruas cetakan `LAB-EVD-005` yang tidak pernah diadopsi putaran 11; `DEC-LAB-029` — kardinalitas lokasi per wadah, satu-satunya penahan `S2b-1`. Gerbang yang sama menaikkan `S16a` ke `READY_FOR_DOMAIN_DESIGN`, meleburkan `S8`, dan mengeluarkan `S19` dari desain | `draft` |
| 79 | 2026-09-25 | **Amendment pass putaran 19 — enam penahan gerbang `LAB-RCG-001-r10`, dengan scope yang dikonfirmasi pemilik modul.** **`LAB-DEC-157`/`158`** menutup `DEC-LAB-026`: PDF hasil eksternal **satu data** di dokumen klinis pasien Clinical Management, **pintu unggah di Laboratorium**, tertaut pasien dan kunjungan, cek dua identitas, salah pasien ditandai *salah input* — `LAB-COORD-018` dibuka. **`LAB-DEC-159`/`160`**: `S16a` tiga laporan dengan definisi tetap — jumlah pada tanggal rilis, penolakan per wadah, TAT layak sampai rilis — dan hak akses tersendiri; `DEC-LAB-007` disahkan, `DEC-LAB-025` menyempit ke delapan laporan lain. **`LAB-DEC-161`** menutup `DEC-LAB-023` dan `LAB-DEC-145` butir 4 — pertentangan dengan `CAP-006` (daftar lokasi tumbuh otomatis) diselesaikan **ke arah blueprint**. **`LAB-DEC-162`**: blok/slide tidak dilacak; seri Sitologi/FNAB ditahan — `DEC-LAB-022` menyempit. **`LAB-DEC-163`** menutup `DEC-LAB-024`: `S8` dilebur ke `S5`/`S6`. **`LAB-DEC-164`** menutup `DEC-LAB-027`: MCU lewat jalur umum, `S19` menunggu verifikasi berjalan. `LAB-DEC-030` diamandemen pada dua baris. `AC-248`..`AC-256`. **Nol keputusan terkunci dicabut** | `draft` |
| 78 | 2026-09-25 | **Enam decision ID dan satu koordinasi baru dari gerbang `LAB-RCG-001-r10`, dicatat tanpa keputusan baru.** `DEC-LAB-022` seri Sitologi/FNAB dan pelacakan blok/slide, `DEC-LAB-023` lokasi dan metode pengambilan specimen (`S2b`); `DEC-LAB-024` kejadian yang wajib diberitahukan (`S8`); `DEC-LAB-025` definisi kesebelas laporan (`S16`); `DEC-LAB-026` pemilik penautan berkas hasil eksternal — **bertentangan** dengan `LAB-DEC-030` sesudah kemampuan dokumen klinis pasien milik Clinical Management ditemukan (`S18`); `DEC-LAB-027` perlakuan order MCU (`S19`); `LAB-COORD-017` penyediaan pemberitahuan oleh platform. `DEC-LAB-016` diperluas ke berkas PDF. Juga pada hari yang sama: kontrak `r36`/`r14`/`r7` disetujui dan `BE-LAB-81` direncanakan | `draft` |
| 77 | 2026-09-25 | **Amendment pass putaran 18 — keputusan pemilik modul `LAB-EVD-011` (dua tangkapan layar, disimpan verbatim di `evidence/`) beserta empat klarifikasi pilihan.** **`LAB-DEC-154`** menutup `LAB-CONFLICT-014`: order hanya `Completed` bila seluruh pemeriksaan tidak batal sudah **dirilis**; `PUT complete` menjawab `409` beserta rincian; pemeriksaan tanpa jalur validasi menahan order. **`LAB-DEC-155`**: hasil resmi = Tervalidasi dan Dirilis; validasi dan rilis boleh dipakai sebelum `S6` — bagian kedua pertanyaannya dibuka sebagai **`LAB-OPEN-045`**. **`LAB-DEC-156`**: label keadaan pemeriksaan *Menunggu Hasil*/*Draft*/*Menunggu Validasi*/*Tervalidasi*/*Dirilis*, tombol Final bernama *Pemeriksaan Selesai* — **mengamandemen tabel label BR-88 (`LAB-DEC-135`)** pada tingkat tampilan saja; `LAB-DEC-080` utuh. `AC-243`..`AC-247`. `LAB-CONFLICT-014` dicatat di decision log untuk pertama kalinya, saat ditutup | `draft` |
| 76 | 2026-09-25 | **Dua decision ID baru dari gerbang `LAB-RCG-001-r9`, dicatat tanpa keputusan baru.** `DEC-LAB-020` — bolehkah hasil Mikrobiologi `Sementara` dirilis, dan bagaimana `Definitif` menggantikannya — menahan desain `S4d-2`. `DEC-LAB-021` — siapa memvalidasi laporan Patologi Anatomi yang ditulis patolog — menahan desain `S4e` seluruhnya. Keduanya wewenang klinis per disiplin (`LAB-DEC-079`). `DEC-LAB-017` dinyatakan berlaku sejenis bagi kedua disiplin | `draft` |
| 75 | 2026-09-25 | **Amendment pass putaran 17 — jawaban tertulis dr. Bima Prasetya, Sp.PK atas `LAB-REQ-014` (`LAB-EVD-010`, disimpan verbatim di `evidence/`).** Dua pertanyaan kepada pemilik modul, keduanya pilihan A. **`LAB-DEC-152`** menutup sisa `DEC-LAB-011` dan `LAB-OPEN-034`: validasi di luar jam kerja oleh dokter lain yang ditetapkan pada disiplin yang sama, tidak lintas disiplin; pemegang Mikrobiologi dr. Nabila Rahmawati, Sp.MK dan Patologi Anatomi dr. Citra Maharani, Sp.PA; nama pemegang tambahan adalah **data** di kredensial Human Resource, dan `LAB-DEC-022` butir 3 menjadi **syarat rilis**. **`LAB-DEC-153`** menutup `DEC-LAB-018`: perilis **tidak wajib dokter** — tafsiran *"pejabat/dokter"* = *atau* dicatat terbuka beserta dasarnya. `LAB-DEC-150` kini **berbukti tertulis**. Dibuka **`LAB-OPEN-044`** — isi *aturan laboratorium* tentang calon perilis — yang menahan langkah rilis saja. `AC-241`, `AC-242`. **Nol keputusan terkunci dicabut.** Penahan `S4d`/`S4e` tertutup; gerbangnya perlu dinilai ulang | `draft` |
| 74 | 2026-09-25 | **Satu decision ID baru dari arsitektur domain `LAB-DA-001` rev 8, dicatat tanpa keputusan baru.** `DEC-LAB-019` — bolehkah pemeriksaan yang sudah dirilis dibatalkan, atau penarikannya selalu lewat koreksi. Pertanyaan ini muncul dari ketegangan `LAB-DEC-049` dengan `LAB-DEC-138`/`LAB-DEC-063`. Arah yang berlaku sampai dijawab adalah **ditolak**, diturunkan dari `LAB-DEC-138`. Memblokir desain `S6`, tidak `S4` | `draft` |
| 73 | 2026-09-24 | **Dua decision ID baru dari gerbang `LAB-RCG-001-r8`, dicatat di sini tanpa keputusan baru.** `DEC-LAB-017` — bolehkah `S4` dipakai sebelum `S5` pelaporan kritis berdiri — dan `DEC-LAB-018` — siapa pemegang kewenangan rilis Patologi Klinik. Keduanya menahan **pemakaian nyata** `S4`, bukan desainnya. `DEC-LAB-018` ditambahkan ke nota `LAB-REQ-014`. Juga pada hari yang sama: keempat kontrak `EPIC-LAB-14` disetujui pemilik modul dan roadmap `MVP-8` disusun (`BE-LAB-67`..`69`, `FE-LAB-35`..`37`) | `draft` |
| 72 | 2026-09-24 | **Amendment pass putaran 16 — jawaban dr. Bima Prasetya, Sp.PK (`LAB-EVD-009`), disampaikan pemilik modul; bukti tertulis belum dilampirkan.** **`LAB-DEC-150`** mencatat jawaban `DEC-LAB-011` sebagai **jawaban sebagian** (pilihan A): dr. Bima pemegang pertama kewenangan validasi Patologi Klinik sekaligus penetap pemegang lain; **validasi hanya oleh dokter berkewenangan laboratorium**, sehingga **butir 2 `LAB-DEC-022` SUPERSEDED** dan butir 1 `LAB-DEC-142` diamandemen; setiap validasi menyimpan **snapshot peran** validator. **Butir 3-4 `LAB-DEC-022` tetap berlaku** — pemegang kedua per shift serta pemegang Mikrobiologi dan Patologi Anatomi diajukan lewat **`LAB-REQ-014`**. `DEC-LAB-011` **tidak lagi memblokir desain `S4` Patologi Klinik**, tetapi tetap memblokir rilisnya serta desain `S4d`/`S4e`. **`LAB-DEC-151`** menutup `PRD1-CLIN-01`: balasan WhatsApp hanya konfirmasi komunikasi, bukan bukti baca ulang. `LAB-OPEN-029` menyempit — persetujuan Profesor berbentuk elektronik beraudit, mekanismenya menunggu pihak klinis. `LAB-COORD-011`/`015` tidak berubah. `PRD1-OPEN-01` tetap terbuka; pertanyaan balik dr. Bima dijawab dari bukti. Lima contoh lama yang memakai analis sebagai pemvalidasi (BR-01, BR-18, BR-95, BR-96, koreksi PRD 31) **diberi catatan, tidak dihapus**. BR-101, BR-102, `AC-238`..`AC-240` ditulis | `draft` |
| 71 | 2026-09-24 | **Closure pass putaran 15 SELESAI — Q4 dijawab, `LAB-DEC-149`.** Hasil Patologi Klinik diisi pada **halaman detail per order** — seluruh pemeriksaan dalam satu tabel isian, Final tetap per pemeriksaan, Daftar Kerja tetap sebagai antrean, dialog modal dicabut; `LAB-FE-017` dicatat. `LAB-CLOSE-016` dan `LAB-CONFLICT-013` ditutup; `AC-234`..`AC-237` ditulis. **Hasil putaran:** keempat closure question dan kedua conflict capability map revision 5 tertutup lewat `LAB-DEC-146`..`LAB-DEC-149` serta `LAB-FE-017`; `AC-221`..`AC-237`; **nol keputusan terkunci dicabut**. Menuntut amandemen `LAB-PERM-v1` dan `LAB-VAL-v1` serta task perbaikan atas `S4a`/`S4b`. Terbuka di luar wewenang pemilik modul: `LAB-COORD-016` (pemilik `human-resource`) dan `DEC-LAB-011` (dr. Bima, kini dengan usulan konkret) | `draft` |
| 70 | 2026-09-24 | **Closure pass putaran 15, Q3 dijawab — `LAB-DEC-148`.** Penunjukan pemvalidasi dan perilis disimpan pada **kredensial Human Resource** (`WfpClinicalPrivilege`) dengan enam kode kewenangan Laboratorium; Laboratorium **hanya membaca** — status aktif dan masa berlaku menentukan, suspend dan revoke menolak seketika. `LAB-CLOSE-015` ditutup. **`LAB-COORD-016` dibuka** kepada pemilik `human-resource` — menahan implementasi, bukan desain. **`DEC-LAB-011` tetap terbuka**, tetapi kini dilengkapi usulan konkret: penetapan lewat proses kredensial rumah sakit. BR-99 dan `AC-229`..`AC-233` ditulis. Q4 (`LAB-CLOSE-016`) ditanyakan | `draft` |
| 69 | 2026-09-24 | **Closure pass putaran 15, Q2 dijawab — `LAB-DEC-147`.** Hasil yang sudah Final **ditolak `409`** bila isinya disimpan ulang — Patologi Klinik, Mikrobiologi, dan catatan konsultasi — dengan Reopen beralasan sebagai satu-satunya jalan; **Mikrobiologi diperbaiki sekarang** sebelum `S4b` masuk rilis. `LAB-CLOSE-014` ditutup. BR-98 dan `AC-225`..`AC-228` ditulis. Menuntut amandemen `LAB-VAL-v1` dan task perbaikan `S4b` backend serta frontend. Q3 (`LAB-CLOSE-015`) ditanyakan | `draft` |
| 68 | 2026-09-24 | **Closure pass putaran 15 dibuka atas capability map revision 5; Q1 dijawab — `LAB-DEC-146`.** Tindakan atas hasil — isi hasil Patologi Klinik dan Mikrobiologi, Final, Reopen, konsultasi — memakai **resource hak akses tersendiri**; batal, cito, dan duplo tetap pada `LabExamination : Update`; izin hasil hanya untuk jabatan analis; laporan Patologi Anatomi tidak disentuh; data kebijakan analis wajib terpasang dalam rilis yang sama. `LAB-CLOSE-013` dan `LAB-CONFLICT-012` ditutup. BR-97 dan `AC-221`..`AC-224` ditulis. Menuntut amandemen `LAB-PERM-v1` dan task perbaikan lima endpoint `S4a`/`S4b`. Header *Capability map* diperbarui ke revision 5. Q2 (`LAB-CLOSE-014`) ditanyakan | `draft` |
| 67 | 2026-09-24 | **Amendment pass putaran 14 SELESAI — Q11 dijawab, `LAB-DEC-145`.** Email pasien dihapus dari PRD; penanda rendah/tinggi/kritis dikunci maknanya dan wajib berupa huruf atau teks (`LAB-FE-015`); larangan modal diterima untuk isian hasil utama (`LAB-FE-016`); lokasi dan metode pengambilan specimen tetap `S2b`. `AC-219`, `AC-220` ditulis; koreksi PRD menjadi **45**, termasuk dua tambahan penyisiran akhir (preview bilingual dan Diagnostic Report). Satu open question kecil dicatat: `PRD1-OPEN-01` — arti Nomor Transaksi dan Nomor Mutasi, tidak memblokir. **Hasil putaran:** keempat belas pertentangan dan keenam butir baru tertutup lewat `LAB-DEC-133`..`LAB-DEC-145` beserta `LAB-FE-015`/`LAB-FE-016`; `AC-196`..`AC-220` ditambahkan; **nol keputusan terkunci dicabut**. Yang tersisa di luar wewenang pemilik modul: `PRD1-CLIN-01` (`DR-LAB-001`/`DR-LAB-002`) dan `LAB-COORD-015` (pemilik keamanan/platform, belum ditetapkan) | `draft` |
| 66 | 2026-09-24 | **Amendment pass putaran 14, Q10 dijawab — `LAB-DEC-144`.** Keempat penolakan Mikrobiologi putaran 9 ditegaskan ulang: hasil per pemeriksaan, `Lainnya` ke daftar pantau, nama analis diturunkan, subbakteri tidak dibangun. `PRD1-CONF-07`..`PRD1-CONF-10` ditutup — **keempat belas pertentangan PRD kini seluruhnya tertutup, dan nol keputusan terkunci dicabut**. Koreksi PRD menjadi 39. Q11 (empat butir kecil: email pasien, warna flag, larangan modal, lokasi dan metode specimen) ditanyakan sebagai satu paket rekomendasi | `draft` |
| 65 | 2026-09-24 | **Amendment pass putaran 14, Q9b dijawab — `LAB-DEC-143`.** Penunjukan validasi dan rilis dicatat **per disiplin**; bentuk ini tetap dapat mewakili kebijakan satu orang untuk semua disiplin, sehingga jawaban `DEC-LAB-011` apa pun tidak menuntut data dipecah ulang. `PRD1-FOLLOW-03` ditutup. **`PRD1-CONF-14` ditutup tanpa pertanyaan** — AC Mikrobiologi nilai kritis bukan wewenang pemilik modul; dikoreksi mengikuti `LAB-DEC-103`. **Sepuluh dari empat belas pertentangan kini tertutup.** BR-96 dan `AC-218` ditulis; koreksi PRD menjadi 35. Q10 (empat butir Mikrobiologi yang pernah ditolak) ditanyakan | `draft` |
| 64 | 2026-09-24 | **Amendment pass putaran 14, Q9 dijawab — `LAB-DEC-142`.** Kewenangan validasi dan rilis **berlapis dua**: jabatan menentukan calon, daftar penunjukan per orang menentukan yang berwenang; keduanya wajib lolos, dan validasi terpisah dari rilis. `LAB-DEC-022` ditegakkan, bukan diganti. `PRD1-CONF-03` ditutup — **sembilan dari empat belas pertentangan kini tertutup**. BR-95 dan `AC-215`..`AC-217` ditulis. Koreksi PRD menjadi 33, termasuk satu yang terlewat pada tabel pertentangan: BP-002 langkah 7 dan AC Mikrobiologi *"Finalisasi → Hanya Dokter Lab"* bertentangan dengan `LAB-DEC-097`. **Satu butir dibuka:** `PRD1-FOLLOW-03` — apakah penunjukan berlaku per disiplin, ditanyakan sebagai Q9b | `draft` |
| 63 | 2026-09-24 | **Amendment pass putaran 14, Q8 dijawab — `LAB-DEC-141`.** Konsultasi pada hasil Patologi Klinik dicatat sebagai **fakta opsional** — siapa, kepada siapa, kapan — memakai pola Mikrobiologi; bukan syarat Final, dan tanpa kualifikasi `Definitif`. Nol kolom baru. `PRD1-NEW-02` ditutup. BR-94 dan `AC-214` ditulis; koreksi PRD menjadi 30. Q9 (kewenangan validasi per orang atau per jabatan) ditanyakan | `draft` |
| 62 | 2026-09-24 | **Amendment pass putaran 14, Q7 dijawab — `LAB-DEC-140`.** *QR Result Viewer* PRD menjadi **QR verifikasi dokumen**: halaman publik hanya menampilkan nama rumah sakit, nomor cetak, tanggal rilis, dan status dokumen — asli, sudah digantikan, atau tidak dikenal — tanpa satu pun data kesehatan; token acak per versi dokumen. **Batasnya dinyatakan terus terang:** QR tidak membuktikan keutuhan angka di kertas. `PRD1-NEW-01` ditutup. **`LAB-COORD-015` dibuka** — halaman publik tanpa login menunggu pemilik keamanan/platform yang belum ditetapkan; menahan implementasi halaman itu saja. BR-93 dan `AC-211`..`AC-213` ditulis; koreksi PRD menjadi 29. Q8 (konsultasi pada Patologi Klinik) ditanyakan | `draft` |
| 61 | 2026-09-24 | **Amendment pass putaran 14, Q6 dijawab — `LAB-DEC-139`.** Satu gerbang untuk seluruh penyerahan dokumen final kepada pasien — kirim WhatsApp, cetak, dan unduh — terbuka hanya bila order Selesai **dan** persetujuan `LAB-DEC-067` ada; ditegakkan backend, dan layar menyebut syarat yang kurang. Nota Lab, Label Lab, Label Goldar, dan lembar hasil dokter tidak terpengaruh. Berlaku Patologi Klinik dan Mikrobiologi. `PRD1-CONF-13` ditutup. BR-92 dan `AC-208`..`AC-210` ditulis; koreksi PRD menjadi 25. Dicatat bahwa `LAB-OPEN-029` menjadi penahan berikutnya bagi `S17` begitu `DEC-LAB-011` terjawab. Q7 (QR Result Viewer) ditanyakan | `draft` |
| 60 | 2026-09-24 | **Amendment pass putaran 14, Q5 dijawab — `LAB-DEC-138`.** Hasil yang sudah divalidasi tetapi belum dirilis dapat **dikembalikan kepada analis** oleh pemegang kewenangan validasi atau rilis, beralasan dari daftar `LAB-DEC-082`; riwayat validasi tetap tercatat; tanpa versi bernomor dan tanpa pemberitahuan dokter. `PRD1-FOLLOW-01` ditutup. `PRD1-CONF-12` ditutup ke arah blueprint karena tidak dibuka ulang (bawaan `LAB-DEC-133`). BR-91 dan `AC-205`..`AC-207` ditulis; koreksi PRD menjadi 22. Q6 (cakupan gerbang persetujuan `LAB-DEC-067` atas cetak dan unduh) ditanyakan | `draft` |
| 59 | 2026-09-24 | **Amendment pass putaran 14, Q4b dijawab — `LAB-DEC-137`.** Pesan WhatsApp hasil kritis kepada dokter **tidak memuat data klinis**: hanya adanya hasil kritis, unit/ruang, waktu, dan nomor kontak Laboratorium; larangannya ditegakkan pada pembentuk pesan. Jalur kritis karena itu **tidak menunggu izin privasi**. `PRD1-FOLLOW-02` ditutup. BR-90 dan `AC-203`, `AC-204` ditulis; koreksi PRD menjadi 17. `PRD1-CLIN-01` memperoleh pertimbangan tambahan. Q5 (amendment dan pengembalian hasil tervalidasi belum dirilis) ditanyakan | `draft` |
| 58 | 2026-09-24 | **Amendment pass putaran 14, Q4 dijawab — `LAB-DEC-136`.** WhatsApp kepada Dokter Konfirmator menjadi **saluran pengantar** hasil kritis, bukan bukti pelaporan: waktu kirim dan waktu dilaporkan disimpan terpisah, dan pelaporan tuntas hanya bila kelima isian `LAB-DEC-004` termasuk bukti baca ulang tercatat. Alur kritis tidak bergantung pada gerbang WhatsApp. `PRD1-CONF-11` ditutup. BR-89 dan `AC-200`..`AC-202` ditulis; koreksi PRD menjadi 16. **Dua butir dibuka:** `PRD1-CLIN-01` — sahkah balasan WhatsApp sebagai bukti baca ulang, milik `DR-LAB-001`/`DR-LAB-002` dan ditautkan ke `LAB-P0-004`, tidak memblokir; `PRD1-FOLLOW-02` — isi pesan WhatsApp kritis, ditanyakan sebagai Q4b | `draft` |
| 57 | 2026-09-24 | **Amendment pass putaran 14, Q3 dijawab — `LAB-DEC-135`.** Patologi Klinik memakai Draft dan Final **per pemeriksaan** dengan arti yang sama seperti Mikrobiologi dan Patologi Anatomi — Final = penulis selesai menulis, bukan rilis. Hanya hasil Final yang masuk antrean validasi; Reopen boleh selama belum divalidasi; hasil terkunci sesudah dirilis. Kelima status PRD menjadi **label tampilan turunan**, nol status tersimpan baru (`LAB-DEC-080`); Definitif tidak berlaku bagi Patologi Klinik. `PRD1-CONF-04`..`PRD1-CONF-06` ditutup. BR-88 dan `AC-196`..`AC-199` ditulis; koreksi PRD bertambah menjadi 12. **Satu butir turunan dibuka:** `PRD1-FOLLOW-01` — pengembalian hasil yang sudah divalidasi tetapi belum dirilis, digabung ke Q5. Q4 (WhatsApp sebagai bukti pelaporan nilai kritis) ditanyakan | `draft` |
| 56 | 2026-09-24 | **Amendment pass putaran 14, Q2 dijawab — `LAB-DEC-134`.** Hasil Patologi Klinik tetap diketik analis (`LAB-DEC-005` utuh); "Dokter Lab" pada PRD dibaca sebagai pemvalidasi/pengotorisasi dan "Petugas Lab" sebagai petugas administrasi. `PRD1-CONF-01` dan `PRD1-CONF-02` ditutup ke arah blueprint, **nol perubahan source**. BR-87 ditulis. Bagian baru *Koreksi yang wajib masuk revisi PRD* dibuka dengan enam koreksi pertama. Q3 (Draft/Final pada Patologi Klinik dan pemetaan status PRD) ditanyakan | `draft` |
| 55 | 2026-09-24 | **Amendment pass putaran 14 dibuka — PRD *Hasil Pemeriksaan Patologi Klinik & Mikrobiologi* `LAB-EVD-008`.** PRD disimpan verbatim pada `evidence/2026-09-24-prd-hasil-patologi-klinik-mikrobiologi.md`. **`LAB-DEC-133`**: PRD adalah bukti untuk direkonsiliasi butir per butir, bukan baseline baru; batas scope putaran dikonfirmasi pemilik modul tanpa koreksi. Rekonsiliasi menemukan **14 pertentangan** `PRD1-CONF-01`..`PRD1-CONF-14` — empat di antaranya sudah pernah ditolak pada putaran 9, dua menyentuh `LAB-DEC-003` yang diteken pihak klinis — dan **6 butir baru** `PRD1-NEW-01`..`PRD1-NEW-06`, termasuk QR Result Viewer yang nol pernah diputuskan. Q2 (pengisi hasil Patologi Klinik) ditanyakan. **Pembukuan dirapikan:** header dokumen masih tertulis revision `53` padahal revision 54 sudah tercatat; kini disamakan menjadi `55`, dan baris *Capability map* pada header yang masih menyebut revision 3 diperbarui | `draft` |
| 54 | 2026-09-24 | **`LAB-FE-014` DIAMANDEMEN atas arahan pemilik modul: data induk yang KHUSUS Laboratorium pindah ke modul Laboratorium di frontend.** Keputusan revisi 17 (2026-09-01) menempatkan seluruh menu data induk di `health-services/master-data/`; kini sepuluh data induk yang terbukti hanya dipakai Laboratorium — batas nilai, alasan penolakan, jenis specimen, organisme, antibiotik, breakpoint, profil Mikrobiologi katalog, parameter dan golongan PA, pengaturan disiplin — berada di `health-services/laboratory-management/master-data/…` beserta route, view, hook, konstanta, util, dan CSS-nya, mengikuti pola `billing-management/master-data/`. Menunya menjadi sub-grup **Master Data** di menu Laboratorium. **Yang tetap global:** Prosedur, Tarif, Satuan Ukur, dan Konversi Satuan — dipakai modul lain. Slice Redux dan URL API backend tidak berubah. Alamat lama diarahkan ke alamat baru lewat `redirects()` di `next.config.js`. Bunyi `AC-49` dan `AC-116` ("berada di `master-data/`, bukan di folder Laboratorium") dengan ini **digantikan** oleh letak baru; yang tetap mengikat dari keduanya adalah isi layarnya, bukan foldernya. Dasar pemindahan: pemeriksaan pemakaian menunjukkan nol layar, hook, slice, maupun sumber pilihan di luar Laboratorium yang memakai kesepuluhnya | `approved` |
| 53 | 2026-09-21 | **Amendment pass putaran 13 — dataset specimen SNOMED CT `LAB-EVD-007`. `LAB-DEC-129`..`LAB-DEC-132`. `LAB-OPEN-040` DITUTUP.** Berkas `snomedct_specimen_dikelompokan_berdasarkan_jenis.xlsx` diserahkan, dibaca langsung dari XML di dalamnya, dan diverifikasi. **JUMLAHNYA BENAR APA ADANYA: 1.767 baris**, persis seperti klaim artifact sejak awal. **YANG TIDAK BENAR ADALAH BENTUKNYA.** Datasetnya bertingkat **TIGA** — `jenis_specimen` 31, `subjenis_specimen` 85, `display` 1.767 — sedangkan `LAB-DEC-098` merancang dua. Dan tingkat yang dipilih artifact ternyata **yang paling sedikit gunanya**: **21 dari 31 kelompok hanya punya satu subjenis**, sehingga pada dua pertiga data memilihnya berarti melewati layar yang nol punya alternatif. `LAB-DEC-129` mempertahankan bentuk dua tingkat tetapi **mengganti kolom sumbernya** — `LabSpecimenType` diperluas dari 7 menjadi 31, dan `subjenis` turun menjadi atribut yang tetap disimpan demi pelaporan. **Perluasan itu aman**, dan itu diperiksa bukan diasumsikan: ketujuh nilai yang sudah ter-seed **seluruhnya punya padanan** di antara 31 kelompok. **`LAB-DEC-130` menjalankan maksud `LAB-DEC-099` dengan dasar yang kini terlihat:** ke-166 baris berkonfidensi `Rendah` ternyata terpusat pada dua kelompok yang menyebut **lokasi tanpa menyebut bahan**, sehingga ia diimpor **nonaktif** — kepala instalasi memangkas daftar yang sudah jalan alih-alih menghadapi halaman kosong, dan `BE-LAB-55` punya data nyata untuk diuji. **`LAB-DEC-131` menolak menerjemahkan 1.767 nama lebih dulu**, dengan alasan yang lebih berat daripada kecepatan: terjemahan anatomi yang keliru **lebih berbahaya** daripada istilah Inggris yang dibiarkan, sebab petugas yang ragu pada istilah asing akan bertanya sedangkan terjemahan salah tampak meyakinkan. **`LAB-DEC-132` menolak memakai kode SNOMED sebagai kode utama** sebab baris lokal yang ditambahkan lewat `Lainnya` nol punya kode SNOMED, dan kode karangan di dalam kolom yang dianggap standar internasional adalah kesalahan yang sangat sulit ditemukan kemudian. Empat AC baru `AC-192`..`AC-195` | `draft` |
| 52 | 2026-09-21 | **Amendment pass putaran 12 — cetakan Mikrobiologi varian bakteri `LAB-EVD-006`. `LAB-DEC-122`..`LAB-DEC-128`. Putaran ini MENGOREKSI `LAB-DEC-116` YANG BERUMUR KURANG DARI SATU JAM**, dan pelajarannya perlu disimpan: putaran 11 menduga perbedaan bakteri dan jamur **hanya label**, dan dugaan itu diambil dari **satu** contoh cetak. Contoh kedua membatalkannya. **Bentuknya berbeda total:** bakteri memakai `IDENTITAS : <kuman>` plus tabel lima kolom `ANTIBIOTIK | UG | R-S | Zona/mm | RESULT`; jamur memakai `HASIL BIAKAN JAMUR` plus daftar butir ber-MIC. `LAB-DEC-124` menyelesaikannya dengan **dua penanda bebas** — jenis biakan menentukan label, metode uji menentukan bentuk — sebab satu penanda akan mengunci bakteri selalu difusi dan jamur selalu dilusi, dan bukti nol menyatakan itu. **TEMUAN TERBESAR PUTARAN INI:** dua kolom yang `r26` nol kenal sama sekali — `UG` kandungan cakram dan rentang `R-S` breakpoint — dan `LAB-DEC-122` menempatkan keduanya pada data induk lalu di-snapshot, breakpoint **per kombinasi organisme dan antibiotik** karena CLSI menetapkannya begitu dan `LAB-DEC-039` sudah mewajibkannya configurable. **YANG MENGUBAH CARA KERJA ANALIS:** pemeriksaan atas **seluruh 22 baris** menunjukkan interpretasi `S`/`I`/`R` konsisten dapat dihitung dari zona terhadap rentang — zona di bawah batas bawah `R`, di dalam rentang `I`, di atas batas atas `S` — sehingga `LAB-DEC-123` menjadikannya terhitung, dengan penimpaan beralasan tetap dibuka bagi **resistensi intrinsik**. Peringatan-saja ditolak sebab peringatan yang muncul 22 kali pada satu layar berhenti dibaca. **TIGA HAL YANG BELUM PERNAH DIMODELKAN kini punya tempat:** `LAB-DEC-125` penanda set bakteri pada pemetaan katalog — pemilik modul menyatakan tidak semua pemeriksaan memakainya; `LAB-DEC-126` isolat boleh berdiri tanpa baris kepekaan sebab cetakan menyebut dua kuman tetapi hanya satu bertabel; dan `LAB-DEC-127` kalimat baku `LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI` sebagai pengaturan, bukan ketikan. `LAB-DEC-128` menetapkan zona `0` sebagai **pengukuran sah** — sebelas dari 22 baris bernilai `0` dan seluruhnya `R`. Tujuh AC baru `AC-185`..`AC-191` | `draft` |
| 51 | 2026-09-21 | **Amendment pass putaran 11 — bukti cetak tiga disiplin `LAB-EVD-005`. `LAB-DEC-114`..`LAB-DEC-121`. Putaran ini MEMPERBAIKI EMPAT KEPUTUSAN YANG BERUMUR BEBERAPA JAM, dan itu perlu dinyatakan terus terang.** Bukti cetak datang **sesudah** `LAB-API-v1` `r26` disetujui pada hari yang sama. Nol di antara keempatnya salah karena ceroboh — seluruhnya diambil dari uraian artifact, dan uraian itu memang tidak memuat bentuk cetak sebenarnya. **TEMUAN TERBESAR, dan ia mengubah cakupan:** contoh cetak Mikrobiologi yang diserahkan ternyata **KULTUR JAMUR** — `HASIL BIAKAN JAMUR : Candida albicans`, `HASIL RESISTENSI ANTIJAMUR` berisi Nystatin dan Amphotericin — sedangkan seluruh `S4b` dimodelkan untuk bakteri dan **jamur nol kemunculan di seluruh backend**. `LAB-DEC-116` menyelesaikannya dengan satu penanda jenis biakan, bukan dua model: strukturnya identik, perbedaannya hanya label; membiarkannya tanpa penanda berarti mencetak Nystatin sebagai antibiotik di hadapan dokter. **SATU CACAT RANCANGAN DITEMUKAN:** `r26` menyimpan `Concentration` tanpa satuan, padahal cetakan menulis `1,25 ug/mL` — `LAB-DEC-115` memperbaikinya dengan alasan yang **sama persis** dengan `LAB-DEC-041` pada volume specimen. **`LAB-DEC-114` memperluas `LAB-DEC-106` yang berumur satu hari:** `DEFINITIF` ternyata dicetak sebagai kualifikasi hasil, bukan sekadar fakta konsultasi; menurunkannya dari ada tidaknya konsultasi ditolak karena itu menyimpulkan sesuatu yang bukti nol menyatakannya. **DUA BUKTI YANG JUSTRU MENGUATKAN keputusan sebelumnya:** cetakan Patologi Klinik mencetak **dua baris terpisah** `Otorisasi oleh` dan `Validasi oleh`, sehingga laboratorium ini **sudah** membedakan pemvalidasi dari pengotorisasi hari ini — bukti lapangan langsung bagi empat mata `LAB-DEC-003` dan penguat `LAB-DEC-097` bahwa Simpan Final bukan rilis. **AKIBAT YANG HARUS DIBAWA KE PERENCANAAN:** `LAB-DEC-120` berarti cetakan Mikrobiologi **belum dapat lengkap sampai `S4d` dibuka `DEC-LAB-011`**, dan mengisinya dari pencetak atau penulis hasil ditolak. **SATU HAL SENGAJA TIDAK DITUTUP:** konsultan pada cetakan bergelar **Profesor** dan itu kandidat yang selama ini tidak ada bagi `LAB-OPEN-029` — tetapi `LAB-DEC-121` hanya **mengajukannya**, sebab butir itu tegas bukan wewenang pemilik modul. `LAB-OPEN-039` sebagian ditutup; sisanya seluruhnya **varian**, bukan hal baru. Delapan AC baru `AC-177`..`AC-184` | `draft` |
| 50 | 2026-09-21 | **Amendment pass putaran 10 — menutup ketiga closure question yang dibuka impact scan capability map revision 4 pada hari yang sama. `LAB-DEC-111`..`LAB-DEC-113`. Putaran ini memperbaiki, bukan memperluas: nol scope bertambah.** **Yang paling perlu dicatat adalah bahwa putaran ini ADA.** Impact scan menemukan `LAB-DEC-108` butir 2 berdiri di atas fakta yang terbantah — `MstDoctorSchedule` ternyata jadwal praktik poliklinik, lengkap dengan `ClinicId` wajib, kuota pasien, dan penanda kiosk/telemedicine, sedangkan `ScheduleType` nol memuat nilai yang berarti "sedang jaga". Keputusan itu berumur **kurang dari satu hari** ketika terbantah, dan **tetap tidak dicabut oleh audit** — mencabut keputusan bukan wewenang audit; yang mencabutnya pemilik modul lewat `LAB-DEC-111`. **`LAB-DEC-111` menemukan rantai yang ternyata SUDAH LENGKAP:** `TrxOnCallAssignment` → `WorkforceProfileId` → `MstDoctor.WorkforceProfileId` → nama + `MstDoctor.WhatsAppNumber`, seluruhnya pada satu `ApplicationDbContext`, nol tabel baru dan nol jembatan baru. **Tetapi `TrxOnCallAssignment` nol punya controller**, sehingga pada hari pertama halaman ini hidup jadwal jaganya pasti kosong — dan di situlah keputusan ini berbeda dari dua pendahulunya yang gagal: alih-alih menunggu modul lain seperti `LAB-COORD-006` dan `MST-POS-WRITE` yang **keduanya masih terbuka sampai hari ini**, Laboratorium membangun **jalur jatuh permanen** ke daftar dokter aktif, sehingga `LAB-DEC-004` terpenuhi sejak hari pertama dan sumber yang benar tetap terbaca begitu terisi. **`LAB-DEC-112` menolak menumpangkan jejak perubahan ruas ke `LabTransitionHistory`** dengan alasan yang sama persis dengan `LAB-DEC-080`: satu tabel, dua sumbu — ditumpangkan, ketiga kolom baru kosong pada seluruh baris status lama dan setiap laporan riwayat status menyaring baris yang bukan miliknya selamanya. **`LAB-DEC-113` menolak memakai ulang `LabPathologyFindingStatus`** karena `Positif` adalah temuan sedangkan `NeedsAttention` adalah penilaian kegawatan; disatukan, `NeedsAttention` **bertabrakan dengan penanda kritis `LAB-DEC-103`** — dua tempat menyatakan hal yang sama, nol aturan menentukan pemenangnya. **Satu butir baru, dan ia bukan milik Laboratorium:** `LAB-COORD-014`, endpoint pengisi `TrxOnCallAssignment`, **nol memblokir** justru karena jalur jatuhnya dibangun lebih dulu. **Empat AC baru:** `AC-173`..`AC-176`, dan `AC-174` menguji jalur jatuh itu secara langsung | `draft` |
| 49 | 2026-09-21 | **Amendment pass putaran 9 — halaman Hasil Pemeriksaan Mikrobiologi. Enam belas keputusan `LAB-DEC-095`..`LAB-DEC-110`, dan yang paling perlu dicatat adalah polanya: ENAM pertentangan dengan keputusan terkunci, dan KEENAMNYA diselesaikan ke arah blueprint. Nol keputusan terkunci dicabut.** Sumbernya artifact `Laboratorium - Hasil Pemeriksaan Mikrobiologi`, dibukukan `LAB-EVD-004`. **Tiga penolakan yang paling menentukan.** `LAB-DEC-095` menolak `RULE-001` yang memindahkan hasil ke tingkat order: Kultur Darah dan Kultur Urin dalam satu pesanan adalah **dua bahan dengan dua pola kuman**, dan menyatukannya membuat laporan pola kuman per jenis bahan — dasar pemilihan antibiotik empiris — berhenti dapat dihitung; `LAB-DEC-085` yang berumur 3 hari **bertahan utuh**. `LAB-DEC-097` menolak tafsir artifact bahwa `Simpan Final` berarti hasil sah untuk pasien: bila diterima, pengisi hasil sekaligus pengesahnya, dan **empat mata `LAB-DEC-003` yang baru ditandatangani `DR-LAB-002` pada 2026-09-17 berhenti berlaku untuk Mikrobiologi saja tanpa satu pun pihak klinis menyatakannya**. `LAB-DEC-096` memperluas `LAB-DEC-092` ke Mikrobiologi dengan alasan identik — isian manual berarti dua sumber kebenaran untuk satu kejadian. **Dua keputusan yang menerima artifact dengan syarat.** `LAB-DEC-098` menerima kebutuhan tingkat kedua specimen tetapi **menolak tata kelolanya**: pemeriksaan duplikat Indonesia/Inggris yang diusulkan artifact meloloskan `cairan kista`, `Cairan Kista`, `c. kista`, dan `kista` sebagai empat nilai, sehingga `LAB-DEC-040` tetap berlaku dan hanya kepala instalasi yang menaikkan nilai. `LAB-DEC-107` menerima penyuntingan specimen di halaman hasil, dengan syarat berjejak — menutupnya mengulang jalan buntu yang persis dihindari `LAB-DEC-040` pada shift malam. **Dua open question lama ditutup:** `LAB-OPEN-017` arti `Definitif` oleh `LAB-DEC-106` — fakta konsultasi, bukan status dan bukan izin, dan **sengaja tidak** disamakan dengan persetujuan Profesor `LAB-DEC-067` karena itu `LAB-OPEN-029`; dan `LAB-OPEN-030` oleh `LAB-DEC-108` — `Dokter Lantai` menjadi label layar di atas `MstDoctorSchedule` yang sudah berdiri, bukan peran baru pada `LAB-PERM-v1`. **Satu keputusan memisahkan bentuk dari isi supaya slice tidak tertahan:** `LAB-DEC-103` mengunci bentuk aturan kritis Mikrobiologi sebagai data induk terkendali dan menyerahkan isinya kepada `DR-LAB-002`, sehingga `S4b` dapat maju sekarang. **Tiga butir baru dibuka, dan dua di antaranya celah bukti bukan keputusan yang belum diambil:** `LAB-OPEN-039` kedua screenshot cetak tidak pernah diserahkan, `LAB-OPEN-040` dataset 1.767 entri juga tidak, dan `LAB-OPEN-041` isi baris aturan kritis. **Peringatan yang dibawa putaran ini:** capability map revision 3 dipindai pada BE `466a7127` + FE `9cd4cd03f`, sedangkan wawancara ini berjalan pada BE `981e002c` + FE `ebef7ebe5` — **keduanya sudah bergeser**, sehingga status kemampuan existing berpotensi basi dan `/qv-trace` mode impact scan perlu dijalankan sebelum butir mana pun turun menjadi task | `draft` |
| 48 | 2026-09-18 | **Amendment pass putaran 8 dilanjutkan — empat dari enam butir sisa `LAB-EVD-003` ditutup, dan dua di antaranya MENGOREKSI artifact-nya sendiri.** `LAB-DEC-091` sampai `LAB-DEC-094`. **`LAB-DEC-092` adalah koreksi paling tegas atas artifact:** Klarifikasi Q8 menyatakan `Waktu Issued` dan `Waktu Efektif` diisi **manual**, dan itu ditolak — keduanya ruas HL7 `DiagnosticReport` yang **sistemnya sudah mencatat**: *effective* dari `LabSpecimen.CollectedAt` sejak `S2`, *issued* dari `FinalizedAt` yang baru ditetapkan `LAB-DEC-088`. Menerimanya sebagai isian manual berarti **dua sumber kebenaran untuk satu kejadian**, dan yang diketik nol akan pernah ketahuan melenceng — alasan yang sama dipakai `LAB-DEC-066` menolak counter telanjang. **`LAB-DEC-091` menempatkan keempat ruas konteks klinis pada PESANAN, diisi DOKTER PEMESAN**, bukan pada hasil dan bukan oleh patolog: riwayat penyakit dan masa terakhir haid ada di hadapan dokter pemesan, **bukan di hadapan patolog**, dan memintanya mengetik ulang berarti meminta ia mengarang atau mengosongkan. Kelompok terpisah dipilih di atas kolom `LabOrder` dengan alasan `LAB-DEC-085`: tabel itu dipakai tiga disiplin, dan `Masa Terakhir Haid` bahkan hanya bermakna bagi sitologi ginekologi pada pasien perempuan. **`LAB-DEC-093` menolak dua jalan pintas sekaligus** — `ExaminerDoctorId` bukan analis, dan menurunkannya dari pelaku pengisian akan **selalu** menghasilkan nama dokter sebab `LAB-DEC-090` baru menetapkan pengisi hasil PA adalah Dokter Lab. **`LAB-DEC-094` menjaga `S4c` tetap dapat dikirim:** `Status Hasil PA` dibangun sebagai **nilai**, alur pelaporan kritisnya tetap `S5`. **`LAB-OPEN-014` ikut MENYEMPIT tetapi tidak tertutup** — terbukti Patologi Anatomi mengenal temuan yang wajib diperhatikan segera dan penandanya manual, sedangkan **apa** yang tergolong kritis beserta batas waktu dan eskalasinya tetap milik `DR-LAB-003`. **Sisa dua butir bukan milik Laboratorium**: `LAB-COORD-012` HL7 dan `LAB-COORD-013` terjemahan otomatis, dan keduanya memblokir **satu ruas** serta **satu bentuk cetak**, bukan seluruh halaman | `draft` |
| 47 | 2026-09-18 | **Amendment pass putaran 8 — ketujuh pertentangan `LAB-EVD-003` ditutup dalam satu sesi, dan nol keputusan berumur kurang dari 48 jam dibatalkan.** `LAB-DEC-085` sampai `LAB-DEC-090`; `REC4-CONF-006` **larut sendiri**, bukan diputuskan. **Yang paling menentukan bentuk kode kelak `LAB-DEC-085`:** tempat hasil melekat **berbeda per disiplin** — Patologi Klinik dan Mikrobiologi tetap per pemeriksaan, Patologi Anatomi menjadi **per order**. Dasarnya bahan yang diperiksa, bukan kerapian model: Hemoglobin dan Kalium adalah dua angka dari satu tabung, sedangkan makroskopik dan mikroskopik menguraikan **satu jaringan** — satu order PA yang memuat Histologi dan IHK pada blok parafin yang sama akan meminta uraian jaringan itu dua kali, dan keduanya dapat melenceng. **`S4a` yang sudah berjalan nol perlu dirombak.** **`LAB-DEC-086` memilih data induk parameter + nilai per baris**, dan alasannya ada di dalam artifact sendiri: ruas `Anjuran` muncul pada Sitologi Ginekologi **dan** IHK, sehingga penggabungan tanpa duplikasi yang dituntut `RULE-013` hanya mungkin bila parameter punya **identitas**, bukan sekadar nama kolom. **`LAB-DEC-087` menolak pencocokan keyword saat runtime** — mengganti nama pemeriksaan di katalog akan diam-diam mengubah bentuk hasil, dan hasil lama ikut berubah artinya; keenam keyword tetap dipakai tetapi sebagai alat bantu pengisian awal sekali. **`LAB-DEC-088` adalah yang paling saya khawatirkan sebelum ditanyakan, dan jawabannya menyelamatkan dua keputusan sekaligus:** `Simpan Final` berarti patolog selesai menulis, **bukan** rilis. Bila ia diartikan rilis, artifact memberi Dokter Lab hak mengisi **dan** merilis hasil yang sama — dan itu melanggar prinsip empat mata yang baru ditandatangani `DR-LAB-003` kemarin. Dengan jawaban ini `LAB-DEC-080` dan `LAB-DEC-003` sama-sama bertahan, **dan `REC4-CONF-006` larut**: `Reopen` sebelum rilis adalah penyuntingan biasa, bukan koreksi hasil terrilis, sehingga ia tidak menyentuh `S6` maupun `DEC-LAB-014`. **`LAB-DEC-089`** menutup `Diplo` versus `Duplo` sebagai satu huruf, bukan satu konsep. **`LAB-DEC-090`** menetapkan pengisi hasil PA adalah Dokter Lab dan **mempersempit `LAB-DEC-005`** — benar untuk Patologi Klinik, tidak untuk Patologi Anatomi; `LAB-DEC-068` tetap utuh sebab ia mengatur menu Hasil, bukan halaman Detail Hasil. **Enam butir sisa artifact dibuka**, dua di antaranya bukan milik Laboratorium: `LAB-COORD-012` sumber data HL7 yang **nol kemunculan di seluruh backend**, dan `LAB-COORD-013` layanan terjemahan otomatis beserta **izin privasinya** — sebab menerjemahkan laporan patologi berarti mengirim **diagnosis pasien** ke luar sistem, sekelas `LAB-DEC-030` dan belum pernah ditandai | `draft` |
| 46 | 2026-09-18 | **Amendment pass putaran 8 — bukti lapangan baru `LAB-EVD-003` diterima, dan verdict-nya `NOT_RECONCILED`.** Artifact *Detail Hasil Patologi Anatomi* memuat `CAP-001`..`CAP-020`, `RULE-001`..`RULE-036`, dan `BP-001`..`BP-006`, beserta 14 klarifikasi pemilik bertanggal 2026-09-17. **Tiga belas butir baru dicatat dan NOL diadopsi sebagai keputusan** — itu disengaja, sebab tujuh pertentangan dibuka dan **tidak satu pun dapat ditutup tanpa pemilik modul**. **Tiga pertentangan membantah keputusan yang berumur kurang dari 48 jam**, dan itu yang membuat putaran ini berbeda dari tiga putaran sebelumnya: `REC4-CONF-001` — `Draft`/`Final`/`Reopen` adalah **status hasil**, sedangkan `LAB-DEC-080` kemarin menetapkan nol status hasil; `REC4-CONF-002` — bentuk hasil Patologi Anatomi **bergantung kategori** dan IHK sendirian punya **sepuluh** ruas, sedangkan BR-23 hanya mengenal satu bentuk tiga ruas; `REC4-CONF-003` — hasil mungkin melekat pada **order**, bukan pada pemeriksaan, dan itu menyentuh `S4a` yang sudah berjalan sekaligus `S4b` dan `S4c`. **Empat pertentangan lain:** kategori diturunkan dari **teks nama** pemeriksaan (`REC4-CONF-004`, membalik `LAB-DEC-036` dan `LAB-DEC-048` butir 6), hak akses yang meniadakan `LAB-DEC-068` (`REC4-CONF-005`), `Reopen` yang menjawab sendiri pertanyaan `DEC-LAB-014` yang sedang diajukan kepada pihak klinis (`REC4-CONF-006`), dan `Diplo` versus `Duplo` yang beda satu huruf tetapi beda makna lebih dari satu huruf (`REC4-CONF-007`). **`BE-LAB-46` dan `FE-LAB-25` DIBEKUKAN**, dan itu keputusan yang murah justru karena **nol baris kode sudah ditulis** — bila artifact ini datang seminggu lagi, ia datang sesudah tabelnya berdiri. **Task Mikrobiologi dan kedua data induk TIDAK ikut dibekukan**: artifact ini nol membahas Mikrobiologi. **Enam butir baru yang nol ada di mana pun pada sistem juga dicatat**, termasuk `HL7` yang nol kemunculan di **seluruh** backend, layanan terjemahan otomatis, dan empat ruas konteks klinis pada `LabOrder` yang justru pernah **dicabut** `LAB-API-v1` `r11` karena tidak ada tempatnya | `draft` |
| 45 | 2026-09-18 | **Tiga amandemen kontrak disetujui pemilik modul, dan gerbang perencanaan `EPIC-LAB-13` terbuka.** `LAB-API-v1` `r24` (empat jalur hasil Mikrobiologi dan Patologi Anatomi, delapan jalur data induk `LabOrganism`/`LabAntibiotic`), `LAB-VAL-v1` `r7` (`VAL-83`..`VAL-91`), dan `LAB-PERM-v1` revision 6 (dua resource baru, nol `Delete`). **Ketiganya aditif** — nol endpoint, ruas, aturan, atau permission yang sudah ada berubah. **Nol keputusan bisnis baru dibuat pada baris ini**; yang terjadi adalah persetujuan atas rancangan yang keputusannya sudah diambil `LAB-DEC-027`, `LAB-DEC-080`, `LAB-DEC-081`, dan `LAB-DEC-084`. **Satu hal perlu dicatat supaya tidak salah baca di kemudian hari:** label *"Rencana (belum tersedia)"* pada tabel endpoint **tetap melekat**, dan itu benar — ia menyatakan endpointnya belum ada **di kode**, bukan bahwa kontraknya belum disetujui. Keduanya hal yang berbeda, dan label itu baru dicabut ketika task pembangunannya selesai. **Yang tetap terbuka dan bukan penahan epic ini:** `DEC-LAB-016` (lampiran gambar Patologi Anatomi) dan `DEC-LAB-011` (validasi/rilis, di luar scope). **Yang tetap memblokir gelombang `MVP-6c` dan bukan pekerjaan programmer:** siapa mengisi daftar organisme dan antibiotik | `draft` |
| 44 | 2026-09-18 | **Amendment pass putaran 7 — satu pertanyaan, dan `S4b` terbuka.** `LAB-DEC-084` menutup `DEC-LAB-015` yang baru dibuka arsitektur beberapa jam sebelumnya: organisme dan antibiotik menjadi **dua data induk terkendali milik Laboratorium**, berprefix `Lab` sesuai `LAB-OPEN-021` yang sudah dijawab Muhammad Hamzah dan melahirkan `LabValueBound` serta `LabValueOption`. **Kepemilikan Laboratorium dipilih di atas `master-data`, dan dasarnya bukti dari modul ini sendiri** — bukan preferensi: `LAB-COORD-006` membuktikan instansi perujuk nol punya endpoint tulis sama sekali, dan `MST-POS-WRITE` membuktikannya lagi pada `MstPosition` sampai barisnya harus disisipkan lewat SQL langsung. Dua kali modul ini menemukan pola yang sama, dan yang ketiga tidak perlu dicoba. **Dasar domainnya terpisah dan berdiri sendiri:** panel antibiotik yang diuji kepekaannya adalah keputusan laboratorium, bukan formularium farmasi. **Yang dibeli:** antibiogram rumah sakit menjadi mungkin disusun. **Akibatnya pada kesiapan:** `S4b` naik menjadi `DOMAIN_ARCHITECTURE_READY`, sehingga kedua slice yang dirancang kemarin kini siap diserahkan ke penyusunan blueprint — dan penilaian gerbang `r7` yang sempat saya cabut pada 0B.6 **pulih dengan sebab yang sah**, bukan karena dicabut ulang | `draft` |
| 43 | 2026-09-18 | **Arsitektur domain `LAB-DA-001` revision 5 disusun untuk `S4b` dan `S4c`, dan ia membuka dua butir sekaligus mencabut satu penilaian gerbang.** `DEC-LAB-015`: BR-23 memerinci hasil Mikrobiologi sampai ke zona hambat dalam milimeter, tetapi **tidak pernah menyatakan** apakah nama kuman dan nama antibiotik diambil dari daftar terkendali atau diketik bebas — dan tanpa itu identitas konsep intinya hanya dapat ditebak. **`S4b` karena itu `DOMAIN_ARCHITECTURE_BLOCKED`**, dan penilaian gerbang `r7` yang menyatakannya `READY` dengan nol penahan **dicabut** pada dokumen gerbang bagian 0B.6. `DEC-LAB-016`: gambar Patologi Anatomi adalah data klinis yang dapat mengidentifikasi pasien, sedangkan pola penyimpanan berkas yang ada di platform berupa `SaveFileAsync` privat per area di atas `IWebHostEnvironment` — nol layanan bersama — sehingga menaruhnya di bawah direktori yang dilayani web adalah keputusan **privasi**, bukan teknis. **`S4c` lolos dan dikirim ke penyusunan blueprint, tanpa bagian gambarnya**, sebab narasi Patologi Anatomi **tidak menunjuk data induk apa pun**. **Nol keputusan bisnis dibuat pada baris ini; arsitektur memodelkan, ia tidak memutuskan.** Catatan yang perlu disimpan dari sesi itu: ini **koreksi ketiga** atas gerbang dalam dua hari, dan ketiganya lahir dari sumber yang sama — gerbang menilai kelengkapan **keputusan**, sedangkan sebagian gap hanya muncul ketika konsepnya benar-benar **dimodelkan** | `draft` |
| 42 | 2026-09-18 | **`DEC-LAB-011` diajukan resmi sebagai `LAB-REQ-013`** pada [`approval-requests/2026-09-18-nota-penetapan-pemegang-kewenangan-validasi.md`](approval-requests/2026-09-18-nota-penetapan-pemegang-kewenangan-validasi.md), ditujukan kepada **dr. Bima Prasetya, Sp.PK** *(dikoreksi 2026-09-23)* selaku kepala instalasi. Memuat **dua pertanyaan saja** — jaminan dua pemegang kewenangan validasi per shift, dan siapa yang berwenang menetapkannya — keduanya turunan `LAB-REQ-004` bagian 5.1 yang tidak ikut dijawab saat penandatanganan 2026-09-17. **Notanya sengaja dibuat tunggal isinya, dan itu keputusan sadar:** `LAB-REQ-004` membundel tiga tanda tangan, dua penetapan, dan tiga pertanyaan klinis; ketiga tanda tangannya turun dan **kelima butir lainnya tidak**, termasuk pertanyaan yang kini diajukan ulang. `LAB-REQ-009` yang meminta **satu hal** dijawab dalam hitungan jam sesudah menggantung delapan hari. Pola itu diikuti. **Satu hal ditanyakan terbuka di dalam nota, dan ia perlu dicatat di sini supaya tidak terbaca sebagai kelalaian:** `Sp.Rad` adalah spesialis radiologi, sedangkan modul ini melayani tiga disiplin **laboratorium** dan rumah sakit baru saja menetapkan tiga pemegang wewenang klinis per disiplin. Menetapkan siapa boleh menyatakan sebuah angka hasil benar adalah penilaian kompetensi atas pekerjaan laboratorium. Nota karena itu menanyakan instalasi mana yang dipimpin, dan apakah wewenang menetapkan ada pada kepala instalasi sendiri, pada ketiga pemegang wewenang klinis, atau berlapis — **ketiganya dinyatakan sama-sama sah**, dan pertanyaannya diajukan sebagai perkara batas wewenang, bukan perkara orang. **Nol keputusan dibuat pada baris ini** | `draft` |
| 41 | 2026-09-18 | **Amendment pass putaran 6 — empat pertanyaan yang gerbang `r6` tunjuk sebagai milik pemilik modul, ditanyakan dan dijawab.** `LAB-DEC-080` sampai `LAB-DEC-083`. **`LAB-DEC-080` menutup `LAB-P0-002`**: validasi dan rilis dicatat sebagai **fakta** — `ValidatedAt`/`ValidatedByUserId`, `ReleasedAt`/`ReleasedByUserId` — dan **nol status hasil** diperkenalkan; `LabExaminationStatus` tidak disentuh. Meneruskan disiplin `S4a`, dan jalur yang ditolak disimpan alasannya: menambahkan `Validated`/`Released` ke enum itu **cacat**, sebab ia memuat `ChargeEligible` dan `Voided` yang bersumbu kelayakan tagih — satu kolom, dua sumbu. **`LAB-DEC-081` mencabut penahan tunggal `S4b` tanpa satu pun keputusan klinis baru**, dan caranya justru dengan membaca yang sudah tertulis: penanda `Definitif` memang sudah ditempatkan Rilis 2 sejak semula. Gerbang `r6` menandainya `BLOCKING` — benar sebagai pernyataan struktur, **keliru sebagai penahan rilis**. **`LAB-DEC-082` menutup `LAB-P0-003` HANYA SEBAGIAN, dan ini koreksi atas gerbang `r6` sendiri**: gerbang menyatakan `LAB-P0-003` dapat ditutup pemilik modul sendirian, wawancara membuktikan tidak — tiga pertanyaan `LAB-REQ-004` bagian 4.5 bersifat klinis dan belum terjawab, dibuka `DEC-LAB-014`. Yang ditutup: alasan koreksi dari **daftar terkendali** mengikuti pola `LAB-DEC-019`, dan **versi bernomor**; alasannya supaya koreksi dapat **dihitung**, sebab teks bebas menulis *"salah ketik"*, *"salah input"*, *"keliru"*, dan *"tertukar"* sebagai empat hal padahal satu. **`LAB-DEC-083` menutup `DEC-LAB-013`**: `S4d` dan `S4e` didirikan, sehingga ketiga disiplin punya tiga pasang sejajar — alasannya kemerdekaan rilis, bukan kerapian, dan ia **tidak** mendahului `LAB-OPEN-034`. **Hasil bersihnya pada kesiapan:** `S4b` naik `READY_FOR_DOMAIN_DESIGN` menemani `S4c`; `S4` turun menjadi **satu** penahan; `S6` tetap tertahan tetapi penahannya kini bernama tepat; `S4d` dan `S4e` lahir langsung dengan dua penahan. **Dan satu butir menjadi yang paling mahal di seluruh modul hari ini:** `DEC-LAB-011` kini menahan **ketiga** slice validasi sekaligus — `S4`, `S4d`, `S4e` — sehingga ia sendirian menahan kemampuan merilis hasil bagi seluruh disiplin | `draft` |
| 40 | 2026-09-17 | **Gerbang kelengkapan requirement dijalankan ulang (`LAB-RCG-001-r6`), dan tiga open question baru masuk ke decision log ini.** `DEC-LAB-011` dan `DEC-LAB-012` **bukan hal baru** — keduanya sudah tertulis pada `LAB-REQ-004` bagian 5.1 dan 5.2 sejak 2026-09-09 sebagai *"dua penetapan yang membuat ketiga aturan itu dapat dijalankan"*, dan keduanya tidak ikut dijawab saat penandatanganan revision 39. Gerbang memberi keduanya **identitas** supaya tidak hilang untuk kedua kalinya; pencatatannya sebagai prosa di dalam satu dokumen permintaan terbukti tidak cukup. `DEC-LAB-013` **benar-benar baru, dan ia temuan peta bukan temuan requirement**: `S4b` dan `S4c` bernama *pengisian hasil*, dan **nol slice** memuat validasi serta rilis untuk Mikrobiologi dan Patologi Anatomi — `LAB-DEC-076` memecah `S4` hanya untuk Patologi Klinik. Selama `LAB-SIGN-001` menahan kelima slice sekaligus, celah itu tidak punya cara terlihat; tanda tangan **per disiplin** kemarin sore membuatnya terlihat sekaligus mendesak, sebab `DR-LAB-002` dan `DR-LAB-003` kini memegang wewenang klinis atas disiplin yang tidak punya tempat menjalankannya. **Nol keputusan baru dibuat pada baris ini** — gerbang menilai, ia tidak memutuskan, dan ketiga butir di atas sengaja dibiarkan terbuka atas nama pemiliknya masing-masing. Hasil verdict-nya sendiri ada pada `02-requirement-completeness-assessment.md` bagian 0A: `S4c` naik `READY_FOR_DOMAIN_DESIGN`, empat slice lain menyempit penahannya, nol mundur | `draft` |
| 39 | 2026-09-17 | **`LAB-SIGN-001` ditutup. Penahan tertua modul ini — terbuka 2026-09-01, diajukan 2026-09-09, berumur 16 hari — akhirnya dibayar.** `LAB-DEC-079`: `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` ditandatangani **apa adanya** oleh ketiga pemegang wewenang yang ditetapkan revision 38, **masing-masing untuk disiplinnya sendiri**. Jalur *"disetujui dengan perubahan"* pada `LAB-REQ-004` bagian 8 tidak ditempuh, dan itu perlu dibaca tepat: **hari ini isi aturannya identik bagi ketiga disiplin** — yang per disiplin adalah **wewenangnya**, bukan isinya. Menyimpulkan ada perbedaan hari ini berarti mengarang; nol perbedaan disampaikan. **Konsekuensi yang nyata dan baru:** amandemen aturan Mikrobiologi kelak cukup ditandatangani `DR-LAB-002` sendiri, dan ketiga disiplin **boleh** menyimpang tanpa membuka ulang keputusan pusat. **Satu open question lahir dari bentuk ini, dan saya tidak menjawabnya sendiri:** `LAB-OPEN-034` — apakah kewenangan validasi/rilis melintasi disiplin. Ia menyentuh `LAB-DEC-003` empat mata, `LAB-PERM-v1`, dan terutama `LAB-DEC-022` jaminan dua pemegang kewenangan validasi per shift, yang menjadi **jauh lebih mahal** bila jaminannya harus dipenuhi per disiplin alih-alih per laboratorium. **YANG TIDAK IKUT TERJAWAB, dan ini yang paling mudah salah harap:** `LAB-REQ-004` bagian 7.2 sudah memperingatkan sejak 2026-09-09 bahwa menandatangani bagian 2–4 saja menyisakan empat hal — `LAB-P0-004` (alur nilai kritis lengkap dengan batas waktu dan eskalasi), `LAB-OPEN-014` (nilai kritis mikrobiologi dan patologi anatomi), `LAB-P0-001` (sisa matriks kewenangan), dan kedua penetapan bagian 5 (`LAB-DEC-022` butir 3 dan `LAB-DEC-023`). Keempatnya **tetap terbuka**, dan peringatan itu terbukti tepat pada hari pertama. **Nol task dibuka, nol kontrak naik revisi, nol baris kode disentuh pada baris ini** — yang berubah adalah izin merancang, bukan rancangannya. Gerbang kelengkapan requirement `02-requirement-completeness-assessment.md` **sengaja belum disunting**: dokumen itu menyatakan sendiri ia dipanggil ulang ketika penahannya dijawab, dan menyuntingnya sepotong berarti mengaku sudah dijalankan ulang padahal belum | `draft` |
| 38 | 2026-09-17 | **Kursi wewenang klinis akhirnya terisi — `LAB-REQ-009` dijawab, dan `LAB-DEC-078` mencatat tiga nama, bukan satu.** `DR-LAB-001` dr. Aditya Pranata, Sp.PK (Patologi Klinik), `DR-LAB-002` dr. Nabila Rahmawati, Sp.MK (Mikrobiologi Klinik), dan `DR-LAB-003` dr. Citra Maharani, Sp.PA (Patologi Anatomi) — menutup **persis** ketiga disiplin scope `LAB-DEC-025`. Nota itu meminta satu nama dan menerima tiga; itu lebih baik daripada yang diminta, dan ia juga membawa pertanyaan yang tidak ada sebelumnya. **Yang berpindah tepat satu baris, dan itu perlu dinyatakan terang supaya tidak ada yang menyimpulkan lebih:** `LAB-REQ-004` kini punya alamat yang sah sesudah menggantung delapan hari — bukan karena isinya sulit, melainkan karena ia ditujukan kepada jabatan yang belum pernah diisi. **`LAB-SIGN-001` TIDAK tertutup.** Ketiga keputusan `LAB-DEC-003`/`004`/`007` masih nol ditandatangani, dan `S4`, `S4b`, `S4c`, `S5`, `S6` tetap tertahan persis seperti kemarin. **Tiga hal ikut terbawa jawaban ini.** Pertama, **bentuknya perorangan per disiplin — bentuk keempat** yang tidak ditawarkan `LAB-REQ-009` bagian 4, sedangkan ketiga keputusan yang ditahan dirumuskan **lintas** disiplin; siapa menandatangani apa karena itu belum tertentu, dan pertanyaannya ditulis langsung ke dalam `LAB-REQ-004` bagian 9.2 sebagai dua pilihan bercentang, bukan ditebak. Kedua, **empat ruas penetapan tidak disampaikan** — siapa yang menetapkan, tanggal, masa berlaku, dan pengganti bila berhalangan — sehingga yang tercatat hari ini adalah **nama**, belum sepenuhnya **penetapan**; ruasnya dibiarkan kosong dan didaftar pada `LAB-REQ-009` bagian 7.1. Ketiga, **nol di antara ketiganya bergelar Profesor**, sehingga `LAB-OPEN-029` **menyempit tetapi justru menajam**: siapa pemegang wewenangnya terjawab, sedangkan `RULE-017` yang mensyaratkan *"Profesor dan Dokter Lab"* kini terbukti menyebut penyetuju yang tidak punya padanan pada penetapan. **`LAB-GOV-SEAT` menyempit, bukan tertutup** — kursinya ada sejak `LAB-DEC-077`, namanya ada sejak hari ini, **barisnya pada data induk belum**: penelusuran source dan seeder menemukan nol kemunculan ketiga nama maupun ketiga kode, dan basis data tidak diperiksa karena nol klien SQL tersedia pada lingkungan sesi ini. **Satu catatan pembukuan:** `LAB-DEC-074` sampai `LAB-DEC-077` masuk pada 2026-09-17 tanpa barisnya sendiri di sini; alih-alih menyusun ulang empat baris dari ingatan, ketiadaannya dicatat apa adanya di baris ini. **DISUSUL PADA SESI YANG SAMA — lihat revision 39.** Pernyataan *"`LAB-SIGN-001` TIDAK tertutup"* di atas benar pada saat ditulis dan berumur pendek: tanda tangannya menyusul beberapa saat kemudian. Baris ini **tidak** disunting supaya urutan kejadiannya tetap terbaca | `draft` |
| 37 | 2026-09-16 | **Aturan tanggal dinyatakan berlaku module-wide lewat `LAB-DEC-074`, menutup `LAB-RDY-C02`.** Audit kesiapan `LAB-RDY-001` menemukan tiga menu Pemeriksaan yang sudah dipakai petugas menerima tanggal masa depan sampai 20 tahun ke depan dan rentang terbalik, tanpa penolakan apa pun — keduanya menghasilkan daftar kosong yang tidak menjelaskan sebabnya. `LAB-DEC-064` dan `LAB-DEC-071` semula lahir dalam konteks `BR-50` (Menu Hasil, `S17`); pemilik modul menetapkan keduanya mengikat seluruh penyaring tanggal Laboratorium. Alasannya konsistensi yang dirasakan petugas, bukan kerapian dokumen. **Perbaikannya menyentuh komponen base yang dipakai 133 berkas lintas modul**, sehingga jalurnya dibuat aditif: prop opsional dengan default tidak berubah. Diturunkan menjadi `FE-LAB-18` | `draft` |
| 36 | 2026-09-16 | **Riwayat revisi yang bolong dirapikan.** Baris `32` disusun ulang dari `blueprint-manifest.md` dan `05-evidence-reconciliation.md` revision 2, keduanya menyebut isinya secara langsung. Baris `31` **ditandai tidak tercatat dan tidak dapat dipulihkan** — nol berkas pada blueprint menyebut isinya, dan mengisinya dengan tebakan akan menghasilkan riwayat yang salah tetapi dipercaya. Dengan ini seluruh selisih pembukuan yang ditemukan saat memproses bukti putaran 3 sudah ditangani: `BR-48`/`BR-49` ditulis pada revision 35, dan riwayat revisi pada revision ini | `draft` |
| 35 | 2026-09-16 | **Dua business rule yang hilang ditulis, menutup selisih pembukuan yang ditemukan saat menulis `BR-50`.** `AC-94` sampai `AC-97` merujuk `BR-48` dan `BR-49` sejak 2026-09-15, tetapi kedua bagiannya tidak pernah ada — putaran 2 menambahkan acceptance criteria tanpa business rule-nya. Keduanya kini ditulis: `BR-48` konfirmasi pesanan dan dokter pemeriksa, `BR-49` pembatalan wajib beralasan beserta batas statusnya. **Seluruh isinya disusun ulang dari `LAB-DEC-061`, `LAB-DEC-063`, `LAB-STATE-v1` `r3`, `LAB-VAL-v1` `r6` (`VAL-70`..`VAL-75`), dan `LAB-API-v1` `r12`..`r14` — nol aturan baru diperkenalkan**, dan itu dinyatakan terang di kepala kedua bagian supaya pembaca berikutnya tidak memperlakukannya sebagai sumber. Dua hal yang sengaja tetap tidak ditetapkan ikut ditulis apa adanya: `LAB-OPEN-027` (konfirmasi belum wajib sebelum `Accepted`) dan koreksi hasil yang tetap terbuka pada `LAB-P0-003` | `draft` |
| 34 | 2026-09-16 | **Empat dari tujuh hal yang dibuka putaran 5 ditutup pada hari yang sama.** `LAB-DEC-070` menolak satu datatable gabungan dan mempertahankan pola tiga daftar sejajar — **artifact tidak diadopsi apa adanya pada butir ini**, karena alasan `LAB-DEC-025` masih berlaku. `LAB-DEC-071` menjadikan pembanding tanggal inklusif dan **mengoreksi `RULE-004` artifact**, yang bila dibaca tegas justru menutup pencarian satu hari. `LAB-DEC-072` memberi `LabOrder` kolom nomor order berupa nomor urut terbaca mengikuti `PatientEncounterNumberService` — beserta peringatan yang dibawa serta dari pola acuannya, yang memuat seluruh nomor terpakai ke memori. `LAB-DEC-073` mengunci cakupan baris menu Hasil pada pesanan Laboratorium saja. `LAB-CONFLICT-009`, `LAB-OPEN-028`, `LAB-OPEN-031`, dan `LAB-OPEN-033` ditutup. **Tiga tetap terbuka dan bukan wewenang pemilik modul sendiri:** `LAB-OPEN-029` persetujuan klinis, `LAB-OPEN-030` jabatan `Dokter Lantai`, dan `LAB-OPEN-032` ukuran cetak — ketiganya diajukan lewat `LAB-REQ-007` bersama `LAB-COORD-011` | `draft` |
| 33 | 2026-09-16 | **Amendment pass putaran 5 — Menu Hasil tiga disiplin, pencetakan, dan pengiriman hasil ke pasien.** Artifact `Laboratorium (4).md` diterima sebagai `LAB-EVD-002`. Enam klarifikasi pemilik diadopsi `LAB-DEC-064` sampai `LAB-DEC-069` dan ditulis sebagai `BR-50`. **Temuan yang paling mahal justru yang paling sederhana:** `LabOrder` **nol kolom nomor order**, padahal `No. Order` dipakai sebagai kolom daftar sekaligus sumber barcode Label Lab. Menyusul di belakangnya tanggal pemeriksaan yang belum punya kolom mana pun, counter pengiriman yang belum punya tabel, dan gerbang WhatsApp beserta pembangkit PDF yang keduanya nol pada platform — dibuka sebagai `LAB-COORD-011`. **Tiga aturan justru sudah terpenuhi source hari ini**: urutan terbaru di atas, pagination kelipatan 5, dan `MstPatient.WhatsAppNumber` sebagai sumber nomor tujuan. **Lima hal dibuka dan sengaja tidak diputuskan:** `LAB-CONFLICT-009` satu datatable gabungan versus `LAB-DEC-025`, `LAB-OPEN-028` arah pembanding tanggal, `LAB-OPEN-029` persetujuan Profesor/Dokter Lab yang justru perkara `LAB-SIGN-001`, `LAB-OPEN-030` jabatan `Dokter Lantai`, dan `LAB-OPEN-031` Unit Layanan `Radiologi`. Ukuran cetak **tidak diadopsi** — artifact menandainya sendiri `Medium` — dan dicatat `LAB-OPEN-032`. **Seluruh BR ini slice `S17` dan tetap tertahan `LAB-SIGN-001`** | `draft` |
| 32 | 2026-09-15 | **Disusun ulang 2026-09-16 dari sumber tertulis, bukan dari ingatan.** Baris ini tidak pernah ditulis pada waktunya. Isinya diambil dari dua sumber yang menyebutkannya secara langsung: `blueprint-manifest.md` bagian `input_revisions` (*"rev 32 — `LAB-DEC-060`..`LAB-DEC-063` menutup rekonsiliasi bukti putaran 2"*) dan `05-evidence-reconciliation.md` revision 2 bagian 10.5, yang mencatat keempat keputusan itu beserta arahnya pada tanggal yang sama. **Isi revision 32:** `LAB-DEC-060` mempertahankan siklus hidup wadah dan menjadikan centang `Sampling diterima` sebagai jalan pintasnya; `LAB-DEC-061` mengadopsi status `Confirmed` beserta konfirmator, waktu konfirmasi, dan dokter pemeriksa; `LAB-DEC-062` menampilkan status pembayaran yang mengunci tombol Proses tetapi dibaca dari Billing; `LAB-DEC-063` mewajibkan alasan pembatalan dan mempersempit batas statusnya. `REC2-CONF-001` sampai `REC2-CONF-003` ditutup, dan `LAB-COORD-010` dibuka | `draft` |
| 31 | — | **Tidak tercatat, dan tidak dapat dipulihkan.** Header dokumen ini sempat menunjuk revision 31, tetapi baris riwayatnya tidak pernah ditulis dan **tidak satu pun berkas pada blueprint menyebut isinya** — manifest melompat dari rev 30 langsung ke rev 32 pada catatan `input_revisions`-nya. Baris ini sengaja ditinggalkan kosong dan ditandai, **bukan diisi tebakan**: riwayat revisi yang salah lebih berbahaya daripada riwayat revisi yang mengaku bolong, karena yang pertama akan dipercaya. Bila pemilik modul mengingat apa yang berubah pada revision 31, baris ini dapat diisi kemudian | `tidak tercatat` |
| 30 | 2026-09-15 | **Persetujuan lintas modul turun, dan gelombang kiosk terbuka penuh.** Pemilik modul menyampaikan persetujuan **Andry Zain** atas penambahan bagian Laboratorium pada kiosk, dibukukan sebagai `LAB-REQ-006`. **Kedua penahan ditutup pada hari yang sama ia dibuka:** `LAB-COORD-008` oleh persetujuan itu, dan `LAB-COORD-009` oleh `LAB-DEC-058`. Keempat keputusan kiosk naik dari `draft` menjadi **`approved`**. `LAB-DEC-058` menetapkan butir yang paling berkonsekuensi dan belum pernah dijawab siapa pun: kunjungan kiosk yang tidak dilanjutkan ditutup **saat hari layanan berakhir**, dan **biaya pendaftarannya gugur** — pasien yang batal tidak menanggung apa pun. Alasannya ditulis terang pada `LAB-REQ-006` bagian 1.3: menagih orang yang tidak menerima satu pun layanan klinis paling sering terbongkar di meja kasir, di depan pasien lain. `LAB-DEC-057` ikut dicatat pada revisi ini setelah ditemukan **belum pernah masuk tabel keputusan** walaupun sudah dirujuk tiga dokumen arsitektur — kelalaian pembukuan, dicatat apa adanya. `AC-92` dan `AC-93` ditambahkan; `AC-93` menjaga agar penambahan ruas pada sesi kiosk bersifat aditif, karena **16 sesi nyata sudah tersimpan** pada tabel itu. **Cara pembukuan yang harus jujur:** persetujuan Andry disampaikan **lisan dan diteruskan pemilik modul**, bukan tertulis langsung. `LAB-REQ-006` menyatakan itu apa adanya, beserta ketentuan bahwa bila pemilik `registration-management` kelak menemukan cakupannya berbeda, dokumen itulah yang keliru — bukan pekerjaannya. **Yang tetap tertahan:** `LAB-COORD-006` data induk perujuk, sehingga jalur bawa surat dokter **luar** belum dapat dipakai penuh; dan `LAB-COORD-007` beserta `FR-11.9`/`FR-11.10` di bawah `LAB-REQ-005` | `draft` |
| 29 | 2026-09-15 | **Amendment pass putaran 4 — pendaftaran lewat kiosk dan pemecahan pesanan per disiplin.** Enam keputusan baru dari wawancara pemilik modul. **Temuan pertama justru memperkecil pekerjaannya:** kiosk **sudah ada dan sudah dipakai** — `TrxKioskScanSession`, `KioskScanSessionController`, dan `MstKioskDevice` milik `registration-management`, dengan **16 sesi nyata dan 15 di antaranya cocok ke pasien**. Yang belum ada hanya sambungannya ke pendaftaran laboratorium, dan pengetahuan kiosk tentang layanan yang dituju pasien. `LAB-DEC-051` menetapkan pasien memilih Laboratorium sendiri di kiosk; `LAB-DEC-052` membedakan dua jalur sejak kiosk — bawa permintaan dokter dan periksa mandiri; `LAB-DEC-053` membentuk kunjungan begitu pasien selesai di kiosk; `LAB-DEC-054` menyerahkan penutupan kunjungan terbengkalai kepada Registrasi. **Keempatnya berstatus `draft`** karena seluruhnya menyentuh layar, tabel, dan kewenangan milik `registration-management` — diajukan sebagai `LAB-COORD-008` dan `LAB-COORD-009`. Risiko `LAB-DEC-053` ditulis apa adanya dan dipilih pemilik modul **setelah** diberi tahu: pasien yang pergi meninggalkan kunjungan tanpa pemeriksaan, dan selama `LAB-DEC-054` belum berjalan, angka kunjungan harian akan melebihi jumlah pasien yang benar-benar diperiksa. **Dua keputusan lain murni Laboratorium dan `approved`.** `LAB-DEC-055` **mengamandemen `LAB-DEC-048` butir 6**: keputusan itu sudah menurunkan disiplin dari pemeriksaan, tetapi tidak pernah menjawab apa yang terjadi bila pemeriksaan berasal dari dua disiplin sekaligus — jawabannya **pecah otomatis, satu pesanan per disiplin**, sehingga `INV-21`, `VAL-46`, dan penyaring ketiga layar Pemeriksaan tidak perlu disentuh sama sekali. `LAB-DEC-056` menetapkan **satu endpoint baru**, bukan memperluas `POST /lab-orders`, dengan alasan yang baru saja terbukti mahal pada `BE-LAB-21`: ruas wajib yang ditambahkan ke endpoint berjalan membuat layar yang sudah dipakai menjawab `422` seketika. BR-46 dan BR-47 ditulis; `AC-86` sampai `AC-90` ditambahkan; `AC-83` diamandemen. **Satu pertentangan dibuka sebagai `LAB-CONFLICT-007`:** `AC-83` menjanjikan tidak ada jalan menyimpan pesanan berdisiplin kosong, tetapi janji itu **hanya ditegakkan frontend** — `LabOrderService` menyalin `request.Discipline` apa adanya dan ruas itu tidak wajib. Terbukti pada data: **2 dari 5 pesanan tidak berdisiplin** dan hilang dari ketiga menu. Ini pola yang sama dengan `VAL-62` dan `T-M3`: aturan yang hanya dijaga satu lapis akan bocor lewat lapis lain, dan bocornya diam-diam | `draft` |
| 28 | 2026-09-14 | **Dua pertentangan ditutup, dan keduanya ditemukan saat implementasi — bukan saat perancangan.** `LAB-DEC-049` mempersempit `AC-55`/`AC-57` ke **penambahan saja**: pembatalan tetap terbuka sesudah kelayakan dan koreksi tagihannya wewenang Billing, sehingga `RJ-BIL-GATE-DEC-003` milik `rawat-jalan` tidak perlu disentuh. `LAB-DEC-050` **mencabut `LAB-DEC-038`**: kolom Jumlah tidak dibuat sama sekali, karena index unik `(SpecimenId, ProcedureId)` atas dasar `BR-20` dan `AC-35` membuat Qty > 1 mustahil — dan contoh Glukosa pada `BR-33` sendiri keliru, sebab Glukosa Puasa dan Glukosa 2 Jam PP adalah dua `MstProcedure` berbeda. BR-44 dan BR-45 ditulis; `AC-52`..`AC-54` dicabut; `AC-55` dan `AC-57` diamandemen; `VAL-60` dicabut; `LAB-API-v1` naik ke `r9` mencabut ruas `Quantity` sebelum sempat dibangun. **Akar keduanya sama:** acceptance criteria revision 23 diturunkan dari `LAB-EVD-001` tanpa pernah diadu dengan model data yang dikunci `LAB-DEC-024`. Dicatat sebagai pelajaran proses pada `roadmap/traceability.md` bagian 5 | `draft` |
| 27 | 2026-09-14 | **`LAB-DEC-048` susunan menu Laboratorium dirapikan**, atas arahan langsung pemilik modul setelah melihat layar yang sudah berjalan. Tujuh perubahan: butir menu Pesanan Laboratorium dicabut, tiga menu Monitoring dinamai ulang menjadi **Pemeriksaan**, empat penyaring dicabut, Kesegeraan disederhanakan menjadi `Semua Data`/`Cito`, dua tombol pindah disiplin dicabut, **disiplin pesanan diturunkan dari pemeriksaan yang dipilih**, dan baris pada ketiga layar Pemeriksaan dapat dibuka ke detail pesanannya. Butir keenam menutup cacat yang selama ini tidak terlihat: disiplin berupa kotak pilihan yang boleh dikosongkan, sehingga pesanan tanpa disiplin **tidak pernah muncul** di menu disiplin mana pun. Butir ketujuh wajib menyertai butir pertama — tanpanya pasien muncul di menu yang benar tetapi tidak dapat dibuka. BR-43 ditulis; `AC-76` diamandemen; `AC-83` sampai `AC-85` ditambahkan; `LAB-FE-009` diamandemen; `LAB-FE-012` dan `LAB-FE-013` ditetapkan. **Dikerjakan lebih dulu di frontend, di luar jalur task roadmap** — dicatat apa adanya. Verifikasi frontend: eslint bersih, 921 uji unit lulus, build produksi exit 0 | `draft` |
| 26 | 2026-09-14 | **`LAB-COORD-006` dan `LAB-COORD-007` diajukan resmi** sebagai `LAB-REQ-005` pada [`approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md`](approval-requests/2026-09-14-permintaan-penerimaan-sampling-specimen.md), ditujukan kepada pemilik `master-data`, `registration-management`, dan `billing-kasir`. Tujuh butir diminta: tiga kepada Master Data (pengelolaan data induk perujuk yang **tidak punya endpoint tulis sama sekali**, status menunggu persetujuan, penggabungan baris), dua kepada Registrasi (nilai `EncounterPaymentType` baru secara aditif, dan penurunannya dari status PKS), dua konfirmasi kepada Billing. Ditambah satu konfirmasi perlakuan `PaymentType` per jalur, tiga pertanyaan yang harus diputuskan Master Data sendiri (`LAB-OPEN-024`), dan laporan `LAB-DEBT-001`. Dicatat pula bahwa penerima belum ditetapkan — blueprint belum mencatat nama pemilik ketiga modul itu | `draft` |
| 25 | 2026-09-14 | **Amendment pass putaran 3 selesai — `LAB-CONFLICT-003` ditutup.** `LAB-DEC-046` menjawab `Q-LAB-06`: piutang mitra menjadi **penjamin kunjungan tersendiri** lewat satu nilai baru `EncounterPaymentType` yang ditambahkan aditif, bukan dititipkan pada `CompanyGuarantor` yang berarti tempat pasien bekerja. Presedennya `CompanyGuarantor = 3` yang ditambahkan begitu lewat `BE-RWI-035`. `LAB-DEC-047` menjawab `Q-LAB-07` dan **mengamandemen `LAB-DEC-044`**: metode pembayaran **diturunkan** dari status PKS untuk rujukan luar dan ditampilkan baca-saja, tetapi tetap **dinyatakan petugas** untuk pasien datang langsung — karena tanpa itu, `LAB-DEC-028` hanya berguna bagi separuh pasiennya. `PaymentType` dan `PaymentMethodId` pada DTO pendaftaran tidak dipangkas, maknanya dipersempit. BR-41 dan BR-42 ditulis; AC-78 sampai AC-82 ditambahkan; AC-74 diperluas. `LAB-COORD-007` dirumuskan ulang dan dialihkan ke `registration-management`. `LAB-DEC-046` berstatus `draft` karena enumnya milik modul lain | `draft` |
| 24 | 2026-09-14 | **Hasil impact scan diserap.** Capability map naik ke revision 3 atas BE `466a7127` + FE `9cd4cd03f`; `LAB-OPEN-022` **ditutup**. Tujuh dari sembilan keputusan amendment terbukti berdiri di atas fakta yang masih benar. Dua dikoreksi: `LAB-DEC-039` ternyata **sudah dikerjakan kode** sebagai `VAL-18` pada `LabExaminationService.cs:123` — ia tidak mengubah perilaku apa pun, hanya menyamakan `AC-20` dengan kenyataan; dan `LAB-DEC-044` **dasar faktualnya dibantah**, dicatat sebagai `LAB-CONFLICT-003`. `LAB-COORD-006` diperbesar — data induk instansi perujuk tidak punya endpoint tulis sama sekali, sehingga `LAB-DEC-043` berstatus `Missing`, bukan `Extend`. `LAB-COORD-007` dirumuskan ulang dan dialihkan ke `registration-management`. `LAB-OPEN-023` dipersempit. `LAB-DEBT-001` dicatat: seeder Laboratorium mengisi data induk global. Empat entity berganti nama sejak `c87d9c0`, termasuk `TrxPatientEncounter` menjadi `RegPatientEncounter` | `draft` |
| 23 | 2026-09-14 | **Amendment pass putaran 2 selesai** atas bukti `LAB-EVD-001`, artifact `Penerimaan Sampling Specimen Lab.md`. Batas scope dikunci `LAB-DEC-037`: delapan butir tetap di Laboratorium, enam butir diserahkan ke modul pemiliknya. **Tidak satu pun keputusan terkunci dicabut.** Sembilan keputusan baru: `LAB-DEC-037` batas scope, `LAB-DEC-038` Qty sebagai alat bantu layar yang memecah diri menjadi baris, `LAB-DEC-039` titik kunci berbeda menurut jalur masuk, `LAB-DEC-040` jenis specimen jadi data induk terkendali dengan jalan keluar terpantau, `LAB-DEC-041` volume selalu membawa satuan, `LAB-DEC-042` waktu sistem dan waktu nyata berdampingan, `LAB-DEC-043` Laboratorium mengusulkan instansi perujuk dan Master Data mengesahkan, `LAB-DEC-044` metode pembayaran dibaca dari Billing dan ditampilkan baca-saja, `LAB-DEC-045` menu tersendiri dengan layar lama dipertahankan. BR-33 sampai BR-40 ditulis; AC-52 sampai AC-77 ditambahkan; `AC-20` diamandemen `LAB-DEC-039`; `LAB-FE-009` sampai `LAB-FE-011` ditetapkan. Pembacaan ulang `BR-25` menggugurkan satu dugaan pertentangan — menampilkan subtotal dan grand total **memang sudah diizinkan** sejak `AC-43` — dan membongkar `LAB-GAP-001`, yang ditutup `LAB-DEC-038`. Lima penahan dibuka: `LAB-COORD-006`, `LAB-COORD-007`, `LAB-OPEN-022`, `LAB-OPEN-023`, `LAB-OPEN-024`. Dua keputusan berstatus `draft` karena bagiannya jatuh di modul lain: `LAB-DEC-043` dan `LAB-DEC-044` | `draft` |
| 22 | 2026-09-09 | **`LAB-SIGN-001` diajukan resmi** sebagai `LAB-REQ-004` pada `approval-requests/2026-09-09-permintaan-tanda-tangan-klinis.md`, ditujukan ke dokter penanggung jawab laboratorium atau Komite Medis. Memuat `LAB-DEC-003`, `LAB-DEC-004`, dan `LAB-DEC-007` untuk ditandatangani; dua penetapan rumah sakit yang menurunkan `LAB-DEC-022` butir 3 dan `LAB-DEC-023`; serta tiga pertanyaan klinis terbuka `LAB-P0-004`, `LAB-OPEN-014`, dan `LAB-P0-001`. Penelusuran ulang menemukan `LAB-COORD-001` dan `LAB-COORD-002` sudah ditutup 2026-09-01, sehingga `LAB-SIGN-001` kini **satu-satunya** penahan `S4`, `S4b`, `S4c`, `S5`, dan `S6`. Dicatat pula bahwa penanda tangannya sendiri belum ditetapkan — `clinical_governance` masih kosong | `draft` |
| 1 | 2026-09-01 | Scope pass dibuka. Fakta source code dicatat, keputusan warisan dari `RJ-BIL-GATE-DEC-003` dikutip, batas scope diajukan untuk dikonfirmasi | `draft` |
| 2 | 2026-09-01 | Batas scope dikunci lewat `LAB-DEC-001` (rilis 1 sampai hasil dirilis) dan `LAB-DEC-002` (Patologi Klinik saja). `LAB-SCOPE-001` dan `LAB-OPEN-003` ditutup | `draft` |
| 3 | 2026-09-01 | Invariant hasil dikunci: `LAB-DEC-003` prinsip empat mata, `LAB-DEC-004` nilai kritis, `LAB-DEC-005` hasil diketik manual. Risiko `LAB-RISK-001` dicatat | `draft` |
| 4 | 2026-09-01 | `LAB-CONFLICT-001` ditemukan dan diselesaikan `LAB-DEC-006` (tabel batas nilai ditarik ke Rilis 1). `LAB-DEC-007` koreksi hasil dan `LAB-DEC-008` rilis sebagian ditambahkan | `draft` |
| 5 | 2026-09-01 | `LAB-DEC-009` cakupan unit dan `LAB-DEC-010` wewenang UI ditambahkan. Acceptance criteria AC-01 sampai AC-13 ditulis. Scope pass ditutup | `draft` |
| 6 | 2026-09-01 | Yoga Aji Pratama ditetapkan sebagai pemilik modul, `LAB-OPEN-001` ditutup. `LAB-DEC-001` sampai `LAB-DEC-010` naik status menjadi `approved`. Fakta F7 tentang ketiadaan sarana notifikasi ditambahkan. `LAB-OPEN-009` dibuka | `draft` |
| 7 | 2026-09-01 | `LAB-DEC-011` wewenang klinis terpisah, `LAB-DEC-012` pemberitahuan tersimpan, `LAB-DEC-013` aturan cito, dan `LAB-DEC-014` reagen di luar scope ditambahkan. BR-08 sampai BR-10 dan AC-14 sampai AC-19 ditulis. Scope pass ditutup | `draft` |
| 8 | 2026-09-01 | **Closure pass dibuka** setelah `01-existing-capability-map.md` revision 1 terbit. Frontend SHA diperbarui dari `c79bb6ee4` menjadi `688daff90` | `draft` |
| 20 | 2026-09-01 | Checkout ditarik ke `c87d9c0`. Impact scan menemukan empat perubahan yang menyentuh blueprint: `ClinicalBillingIntegration` pindah ke `ClinicalManagement`, `TrxClinicalMilestoneFact` menjadi `CliClinicalMilestoneFact`, configuration Laboratorium pindah ke lokasi standar, dan berkas uji pindah ke `Tests/`. Seluruh rujukan diperbarui. `docs/engineering/` kini ada di working copy | `draft` |
| 19 | 2026-09-01 | **`LAB-OPEN-002` ditutup lewat temuan faktual**, ditulis sebagai `LAB-FACT-007` sampai `LAB-FACT-010`. Pembacaan kedua dokumen tata kelola membongkar pelanggaran `QBE-NAM-001` pada rancangan sendiri — tiga entity baru berawalan `Trx*` diganti menjadi `LabExamination`, `LabValueBoundChangeRequest`, dan `LabValueBoundHistory`. Penutupan ini membuka dua penghambat implementasi baru: `LAB-OPEN-018` rules root terpasang belum memuat kedua dokumen, dan `LAB-OPEN-019` lifecycle registry Laboratorium masih `PLANNED` | `draft` |
| 21 | 2026-09-02 | **Pembukuan dirapikan.** Baris revision 19 yang tercatat dua kali digabungkan menjadi satu; kedua salinannya sempat bertentangan soal lokasi canonical dokumen tata kelola dan soal arti `LAB-OPEN-018`. Yang berlaku: lokasi canonical adalah rules root terpasang menurut `AGENTS.md` baris 13, dan `LAB-OPEN-018` berarti rules root belum lengkap. Pertanyaan prefix data induk dipisahkan menjadi `LAB-OPEN-021`. Dua penghambat baru dicatat: `LAB-OPEN-020` checker QBE gagal `TOOL ERROR`, dan koreksi jumlah pengujian dari 31 menjadi 30 | `draft` |
| 18 | 2026-09-01 | Lima koordinasi lintas modul `LAB-COORD-001` sampai `LAB-COORD-005` ditutup, disetujui `andryzainhome` dan `sukmagp` lewat `LAB-REQ-001`. `LAB-OPEN-012`, `LAB-OPEN-002`, dan `LAB-SIGN-001` tetap terbuka karena memerlukan jawaban faktual atau wewenang klinis | `draft` |
| 17 | 2026-09-01 | `LAB-DEC-034` dipersempit menjadi **backend saja** atas arahan pemilik modul. Frontend tetap memakai konvensi yang sudah ada: seluruh menu data induk di `health-services/master-data/`. AC-49 dan `LAB-FE-014` disesuaikan | `draft` |
| 16 | 2026-09-01 | `DEC-LAB-009` ditutup `LAB-DEC-035` (sumber rujukan jadi data induk global) dan `DEC-LAB-010` ditutup `LAB-DEC-036` (kolom disiplin pada `MstProcedure`). BR-31 dan BR-32 ditulis; AC-25 diamandemen; AC-49 sampai AC-51 ditambahkan. `LAB-COORD-004` dan `LAB-COORD-005` dibuka | `draft` |
| 15 | 2026-09-01 | `LAB-DEC-034` penempatan data induk menurut cakupan pemakaian, berlaku backend dan frontend. BR-30 ditulis. Catatan utang teknis "master di dalam folder submodul" dicabut karena penempatan itu justru yang benar | `draft` |
| 14 | 2026-09-01 | `LAB-OPEN-015` ditutup `LAB-DEC-032`: layar pendaftaran milik Laboratorium, pembuatan kunjungan tetap milik Registrasi. `LAB-OPEN-016` ditutup `LAB-DEC-033`: tarif tetap milik Master Data, menu `Tarif Laboratorium` menjadi tampilan baca saja. BR-28 dan BR-29 serta AC-44 sampai AC-48 ditulis. `LAB-COORD-003` dibuka | `draft` |
| 13 | 2026-09-01 | **Amendment pass.** Analisis konsolidasi bukti lapangan diadopsi sebagai baseline requirement. `LAB-DEC-025` sampai `LAB-DEC-031` ditambahkan; `LAB-DEC-002` dan `LAB-DEC-021` ditandai `superseded`, `LAB-DEC-013` ditandai `amended`. BR-21 sampai BR-27 ditulis. Tiga belas open question baru dibuka, delapan di antaranya tata kelola `LAB-P0-*` | `draft` |
| 12 | 2026-09-01 | `DEC-LAB-008` ditutup `LAB-DEC-024`: wadah fisik dipisahkan dari pemeriksaan terpesan. BR-20 dan AC-35 sampai AC-38 ditulis. `LAB-OPEN-012` dibuka untuk memverifikasi jumlah data lab yang sudah terisi | `draft` |
| 11 | 2026-09-01 | `DEC-LAB-008` dibuka dari `03-domain-architecture.md`: wadah fisik dan pemeriksaan terpesan menyatu dalam satu konsep. Memblokir arsitektur target `S2`, `S7`, `S10`, dan menentukan bentuk data hasil `S4` | `draft` |
| 10 | 2026-09-01 | Closure pass putaran kedua, menutup tiga gap yang ditemukan `02-requirement-completeness-assessment.md`. `LAB-DEC-021` bentuk hasil dua macam, `LAB-DEC-022` pemegang kewenangan validasi, `LAB-DEC-023` perlindungan batas kritis. BR-17 sampai BR-19 dan AC-28 sampai AC-34 ditulis | `draft` |
| 9 | 2026-09-01 | Closure pass selesai. `LAB-CONFLICT-002` diselesaikan `LAB-DEC-015`. `LAB-DEC-016` sampai `LAB-DEC-020` menutup `Q-LAB-01` sampai `Q-LAB-05` dan `LAB-OPEN-011`. BR-11 sampai BR-16 dan AC-20 sampai AC-27 ditulis. Empat butir koordinasi lintas modul dibuka: `LAB-AMD-001`, `LAB-COORD-001`, `LAB-COORD-002`, dan `LAB-SIGN-001` | `draft` |
