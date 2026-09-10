# Radiologi — Hospital Domain Architecture

## A. Identitas Arsitektur

| Field | Value |
|---|---|
| Architecture ID | `RAD-DA-001` |
| Revision | `2` |
| Status | `draft` |
| Blueprint ID | `RAD-BP-001` |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** |
| Kesiapan requirement | `RAD-RCG-001-r2` — `PARTIALLY_READY` |
| Slice yang dirancang | `S1`, `S2`, `S3`, `S4`, `S6`, `S7`, `S8`, `S9`, `S10`, **`S12`**, `S13`, `S14` |
| Slice yang **tidak** dirancang | `S5`, `S11` |
| Perubahan revision 2 | `S12` daftar kerja masuk setelah `DEC-RAD-003` ditutup |
| Backend SHA | `64da911` |
| Frontend SHA | `f66ed1885` |
| Decision ID mengikat | `RJ-BIL-GATE-DEC-004`, `RJ-BIL-DEC-014`, `RAD-DEC-001` s/d `RAD-DEC-013` |
| Decision ID belum selesai | `DEC-RAD-001` s/d `DEC-RAD-006` |
| Baseline rujukan | Tidak dipakai. Seluruh arsitektur bersandar pada keputusan pemilik dan bukti source |
| Tanggal | 2026-09-09 |

> **Apa yang dikerjakan dokumen ini.** Dokumen ini memodelkan **makna bisnis** modul Radiologi:
> konsep apa yang ada, siapa pemiliknya, bagaimana siklus hidupnya, dan aturan apa yang tidak
> boleh dilanggar.
>
> Dokumen ini **bukan** rancangan tabel database, **bukan** kontrak API, dan **bukan** izin
> menulis kode. Bentuk fisiknya dirancang tahap berikutnya.

---

## B. Ubiquitous Language

Istilah di bawah dipakai dengan satu makna di seluruh dokumen modul ini. Bila sebuah kata
dipakai berbeda oleh unit lain, perbedaannya dipertahankan, tidak disatukan.

| Istilah | Makna bisnis dalam modul ini |
|---|---|
| **Pesanan** (*order*) | Permintaan seorang dokter agar pasien diperiksa dengan alat pencitraan tertentu. Satu permintaan, satu pesanan |
| **Study** | Satu kali tindakan pengambilan citra yang benar-benar dikerjakan pada pasien. Bukan permintaannya, melainkan pengerjaannya |
| **Acquisition** | Proses pengambilan citranya itu sendiri, bagian dari study |
| **Modalitas** (*modality*) | Jenis alat pencitraan: X-Ray, CT-Scan, MRI, USG, mamografi, fluoroskopi |
| **Gerbang keselamatan** | Sekumpulan pertanyaan wajib yang harus tuntas sebelum pasien boleh diperiksa |
| **Butir keselamatan** | Satu pertanyaan di dalam gerbang itu, misalnya "sedang hamil?" |
| **Aturan keselamatan** | Ketetapan bahwa butir tertentu wajib atau tidak wajib untuk alat tertentu |
| **Citra layak** (*usable*) | Citra yang cukup baik untuk dibaca dokter. Citra yang sudah diambil belum tentu layak |
| **Hasil bacaan** / **ekspertise** | Kesimpulan tertulis dokter radiolog atas citra |
| **Draf bacaan** | Hasil bacaan yang belum disahkan, belum boleh dipakai dokter pengirim |
| **Pengesahan** (*validation*) | Pernyataan dokter radiolog bahwa isi bacaan benar dan boleh dirilis |
| **Rilis** (*release*) | Titik saat hasil bacaan resmi dapat dibaca dokter pengirim |
| **Amandemen** | Koreksi hasil bacaan setelah dirilis, dibuat sebagai versi baru tanpa menghapus versi lama |
| **Fakta kelayakan tagih** | Kabar dari Radiologi ke Billing bahwa suatu pemeriksaan benar-benar dikerjakan dan hasilnya dapat dipakai. Bukan nominal uang |
| **Pengulangan** (*repeat*) | Study baru yang menggantikan study sebelumnya karena suatu sebab, tanpa menghapus yang lama |
| **Penghentian** (*abort*) | Acquisition yang dihentikan di tengah jalan |

### Kata yang sengaja dibedakan

| Pasangan | Bedanya | Mengapa penting |
|---|---|---|
| Pesanan **selesai** vs hasil **dirilis** | Pesanan selesai berarti pemeriksaannya sudah dikerjakan. Hasil dirilis berarti bacaannya sudah sah dibaca | Keduanya kejadian berbeda dengan waktu berbeda. Dikunci `RJ-BIL-GATE-DEC-004` |
| **Diambil** (*acquired*) vs **layak** (*usable*) | Citra sudah diambil belum tentu bisa dibaca | Yang menentukan kelayakan tagih adalah "layak", bukan "diambil" |
| **Lolos** (*passed*) vs **tidak berlaku** (*not applicable*) | Pertanyaan berlaku dan jawabannya aman, versus pertanyaan memang tidak berlaku | Keduanya meloloskan pemeriksaan, tetapi jejak auditnya harus dapat dibedakan |

---

## C. Peta Bounded Context

Modul Radiologi dipecah menjadi empat batas tanggung jawab. Pemecahannya mengikuti **siapa
yang berwenang dan kapan**, bukan mengikuti bentuk layar.

| ID | Nama | Tanggung jawab bisnis | Konsep yang dimiliki |
|---|---|---|---|
| `BC-RAD-01` | Radiology Ordering | Menerima permintaan pemeriksaan dari klinisi, menerima atau menolaknya, menjadwalkan, dan menutupnya | `RadOrder` |
| `BC-RAD-02` | Radiology Acquisition | Memastikan identitas pasien benar, memastikan aman diperiksa, mengambil citra, menilai mutunya, mencatat bahan terpakai | `RadStudy`, `RadStudySafetyCheck`, `RadAcquisitionConsumption` |
| `BC-RAD-03` | Radiology Reporting | Menulis bacaan, mengesahkan, merilis, dan mengamandemen | `RadReport`, `RadReportVersion` |
| `BC-RAD-04` | Radiology Safety Policy | Menetapkan alat apa saja yang ada, butir keselamatan apa saja yang berlaku, dan mengesahkan perubahannya | `RadModality`, `RadSafetyRequirement`, `RadModalitySafetyRule` |

### Mengapa dipecah empat, bukan satu

Alasannya wewenang, bukan kerapian.

- `BC-RAD-01` dikendalikan **dokter pengirim dan petugas pendaftaran radiologi**.
- `BC-RAD-02` dikendalikan **radiografer** — orang yang berdiri di samping alat.
- `BC-RAD-03` dikendalikan **dokter radiolog** — yang membaca citra.
- `BC-RAD-04` dikendalikan **admin Radiologi dan penanggung jawab klinis** — yang menetapkan
  kebijakan, bukan yang mengerjakan pasien.

> **Contoh mengapa `BC-RAD-04` harus terpisah.** Seorang radiografer boleh menjawab butir
> keselamatan pada seorang pasien. Ia **tidak** boleh mengubah butir mana yang wajib untuk
> seluruh pasien. Menyatukan keduanya dalam satu batas akan membuat wewenang "menjawab" dan
> wewenang "menetapkan aturan" sulit dipisahkan.

### Hubungan antar context

| Dari | Ke | Sifat | Keterangan |
|---|---|---|---|
| `BC-RAD-01` | `BC-RAD-02` | Upstream → downstream | Pesanan melahirkan study. Study tidak dapat berdiri tanpa pesanan |
| `BC-RAD-02` | `BC-RAD-03` | Upstream → downstream | Bacaan hanya dapat ditulis atas study yang citranya layak |
| `BC-RAD-04` | `BC-RAD-02` | Upstream → downstream | Aturan keselamatan dipakai saat study dinilai, versinya dibekukan |
| Registration Management | `BC-RAD-01` | Upstream, **Existing** | Kunjungan pasien. Radiologi menempel, tidak memiliki |
| MasterData | `BC-RAD-01`, `BC-RAD-02` | Upstream, **Existing** | Katalog prosedur dan tarif |
| InPatient Management | `BC-RAD-01` | Upstream, **Existing** | Konteks perawatan rawat inap, boleh kosong |
| `BC-RAD-02` | Billing Management | Downstream | Fakta kelayakan tagih. Radiologi mengirim fakta, Billing memutuskan uangnya |
| `BC-RAD-03` | Clinical Management | Downstream, **baca langsung** | Rekam medis membaca hasil bacaan tanpa menyalinnya (`RAD-DEC-006`) |

---

## D. Katalog Konsep Domain

Klasifikasi menggambarkan tanggung jawab domain, bukan bentuk tabel.

### `DC-RAD-01` — RadOrder

| Field | Isi |
|---|---|
| Klasifikasi | `AGGREGATE_ROOT` |
| Context | `BC-RAD-01` |
| Ownership | **Existing** |
| Tujuan | Mewakili satu permintaan pemeriksaan dari seorang dokter |
| Identitas | Identitas sendiri; menempel pada kunjungan pasien, tidak menggantikannya |
| Peran lifecycle | Enam status jalur normal, empat status pengecualian |
| Invariant utama | Tidak memiliki satu pun kolom finansial. Pesanan tidak pernah menerbitkan fakta tagih |
| Bukti | `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs@64da911` |
| Decision | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-009`, `RAD-DEC-011` |
| Keyakinan | Tinggi — sudah rilis dan teruji |

### `DC-RAD-02` — RadStudy

| Field | Isi |
|---|---|
| Klasifikasi | `AGGREGATE_ROOT` |
| Context | `BC-RAD-02` |
| Ownership | **Existing** |
| Tujuan | Mewakili satu tindakan pengambilan citra yang benar-benar dikerjakan |
| Identitas | Identitas dan nomor urut sendiri dalam satu pesanan, termasuk pengulangannya |
| Peran lifecycle | Enam status jalur normal, lima status pengecualian |
| Invariant utama | Pengulangan **tidak pernah** menimpa study aslinya. Study baru menunjuk study yang diulang beserta sebabnya |
| Invariant kedua | Versi aturan keselamatan dibekukan saat study dinyatakan lolos |
| Bukti | `Models/RadStudy.cs@64da911` |
| Decision | `RJ-BIL-GATE-DEC-004` |
| Keyakinan | Tinggi |

> **Mengapa study, bukan pesanan, yang menjadi satuan penentu.** Satu pesanan dapat melahirkan
> beberapa study ketika terjadi pengulangan, dan masing-masing punya sebab serta kelayakan
> tagihnya sendiri. Pertanyaan "berapa kali pasien ini sebenarnya disinari" hanya bisa dijawab
> bila study yang menjadi satuannya.

### `DC-RAD-03` — RadStudySafetyCheck

| Field | Isi |
|---|---|
| Klasifikasi | `ENTITY` di dalam aggregate `RadStudy` |
| Context | `BC-RAD-02` |
| Ownership | **Existing** |
| Tujuan | Jawaban atas satu butir keselamatan pada satu study |
| Identitas | Tidak berdiri sendiri di luar study-nya |
| Invariant | Ketiadaan jawaban **bukan** jawaban. Butir wajib tanpa baris jawaban diperlakukan belum dijawab |
| Bukti | `Models/RadStudySafetyCheck.cs@64da911`; `Services/RadSafetyGateEvaluator.cs@64da911` |
| Keyakinan | Tinggi |

### `DC-RAD-04` — RadAcquisitionConsumption

| Field | Isi |
|---|---|
| Klasifikasi | `ENTITY` di dalam aggregate `RadStudy` |
| Context | `BC-RAD-02` |
| Ownership | **Existing** |
| Tujuan | Mencatat bahan yang benar-benar terpakai — kontras, film, BHP, obat |
| Invariant | Dicatat sebagai **jumlah**, bukan nominal rupiah. Penilaian finansialnya milik Billing |
| Bukti | `Models/RadAcquisitionConsumption.cs@64da911` |
| Decision | `RJ-BIL-GATE-DEC-005` |
| Keyakinan | Tinggi |

### `DC-RAD-05` — RadTransitionHistory

| Field | Isi |
|---|---|
| Klasifikasi | `ENTITY` — jejak audit |
| Context | `BC-RAD-01` dan `BC-RAD-02` |
| Ownership | **Existing** |
| Tujuan | Merekam setiap perpindahan status beserta pelaku, waktu, dan alasannya |
| Invariant | Hanya bertambah. Tidak pernah diubah maupun dihapus |
| Bukti | `Models/RadTransitionHistory.cs@64da911` |
| Keyakinan | Tinggi |

### `DC-RAD-06` — RadModality

| Field | Isi |
|---|---|
| Klasifikasi | `REFERENCE_DATA` |
| Context | `BC-RAD-04` |
| Ownership | **Extend** — data sudah ada, kemampuan mengelolanya belum |
| Tujuan | Daftar alat pencitraan yang dimiliki rumah sakit |
| Peran lifecycle | Aktif atau nonaktif |
| Gap | Belum ada cara menambah, mengubah, atau menonaktifkan selain lewat database langsung |
| Bukti | `Models/MstRadModality.cs@64da911`; hanya dibaca lewat `Controllers/RadStudyController.cs#GetModalities:44@64da911` |
| Decision | `RAD-DEC-001` butir 14, `RAD-DEC-002` |
| Keyakinan | Tinggi |

### `DC-RAD-07` — RadSafetyRequirement

| Field | Isi |
|---|---|
| Klasifikasi | `REFERENCE_DATA` |
| Context | `BC-RAD-04` |
| Ownership | **Extend** |
| Tujuan | Daftar butir pertanyaan keselamatan yang mungkin dipakai |
| Gap | Sama seperti `DC-RAD-06` |
| Bukti | `Models/MstRadSafetyRequirement.cs@64da911` |
| Keyakinan | Tinggi |

### `DC-RAD-08` — RadModalitySafetyRule

| Field | Isi |
|---|---|
| Klasifikasi | `AGGREGATE_ROOT` — **naik kelas dari reference data biasa** |
| Context | `BC-RAD-04` |
| Ownership | **Extend** |
| Tujuan | Menetapkan butir keselamatan mana yang wajib untuk alat mana |
| Identitas | Identitas sendiri, berversi |
| Peran lifecycle | Draf → Menunggu persetujuan → Aktif → Nonaktif |
| Invariant utama | Hanya aturan berstatus **Aktif** yang ikut dinilai gerbang keselamatan |
| Invariant kedua | Setiap pengesahan menaikkan nomor versi tepat satu kali |
| Bukti | `Models/MstRadModalitySafetyRule.cs@64da911`; `Repositories/Configurations/HealthServices/RadiologyManagement/MstRadModalitySafetyRuleConfiguration.cs@64da911` |
| Decision | `RAD-DEC-005`, `RJ-BIL-DEC-014` |
| Keyakinan | Tinggi |

> **Mengapa ini aggregate root, bukan sekadar data induk.** Data induk biasa hanya punya aktif
> dan nonaktif. Aturan keselamatan punya **siklus pengesahan** dengan wewenang yang berbeda di
> tiap langkah, dan punya **versi** yang dibekukan pada study. Konsep dengan siklus hidup dan
> invariant sendiri adalah aggregate, bukan tabel rujukan.

### `DC-RAD-09` — RadReport

| Field | Isi |
|---|---|
| Klasifikasi | `AGGREGATE_ROOT` |
| Context | `BC-RAD-03` |
| Ownership | **New** |
| Tujuan | Mewakili bacaan dokter radiolog atas satu study |
| Identitas | Identitas sendiri, terpisah dari study. Rantai lengkapnya `Patient → Encounter → Order → Study → Report` |
| Peran lifecycle | Pending → Drafted → Validated → Released, lalu amandemen berversi |
| Invariant utama | Hasil yang sudah dirilis **tidak pernah** ditimpa. Koreksi selalu menjadi versi baru |
| Invariant kedua | Pengesah wajib dokter radiolog. Draf yang ditulis bukan-radiolog tidak boleh disahkan penulisnya sendiri |
| Invariant ketiga | Peran penulis dibekukan pada draf, tidak ikut berubah bila peran orangnya berubah kemudian |
| Bukti | **Belum ada di source.** Lifecycle berasal dari `RJ-BIL-GATE-DEC-004` |
| Decision | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003`, `RAD-DEC-006` |
| Keyakinan | Tinggi pada lifecycle dan kewenangan; lihat gap `GAP-RAD-02` untuk pertanyaan yang tersisa |

### `DC-RAD-10` — RadReportVersion

| Field | Isi |
|---|---|
| Klasifikasi | `ENTITY` di dalam aggregate `RadReport` |
| Context | `BC-RAD-03` |
| Ownership | **New** |
| Tujuan | Menyimpan satu versi isi bacaan beserta penulis, pengesah, waktu, dan alasan perubahannya |
| Identitas | Bernomor urut di dalam satu `RadReport` |
| Invariant utama | Versi yang sudah dirilis bersifat tetap. Tidak ada perubahan di tempat |
| Invariant kedua | Setiap versi menunjuk versi sebelumnya, sehingga riwayat koreksi dapat ditelusuri mundur |
| Bukti | Belum ada di source. Berasal dari `RJ-BIL-GATE-DEC-004` |
| Keyakinan | Tinggi |

> **Mengapa versi menjadi entity, bukan status.** Kontrak arsitektur melarang membuat entity
> hanya untuk mewakili status. Versi bukan status — ia adalah **catatan sejarah yang berbeda
> isinya**. Bacaan versi 1 dan versi 2 memuat kesimpulan klinis yang berlainan, dan keduanya
> harus tetap dapat dibaca. Status hanya menerangkan di mana satu versi berada dalam
> perjalanannya.

### `DC-RAD-11` — Fakta Kelayakan Tagih

| Field | Isi |
|---|---|
| Klasifikasi | `DOMAIN_EVENT` yang menyeberang ke `EXTERNAL_CONTRACT` |
| Context | Diterbitkan `BC-RAD-02`, dimiliki Billing Management |
| Ownership | **Adapter/View** — Radiologi menerbitkan, tidak memiliki akibatnya |
| Tujuan | Memberi tahu Billing bahwa satu pemeriksaan benar-benar dikerjakan dan citranya layak |
| Invariant | Satu fakta per study, bukan per pesanan. Hanya terbit saat citra dinyatakan layak |
| Bukti | `Services/RadStudyService.cs#EmitChargeEligibilityAsync:930-960@64da911`; kontrak `BIL-INTEGRATION-0.4` |
| Keyakinan | Tinggi |

### `DC-RAD-12` — Daftar Kerja Alat

| Field | Isi |
|---|---|
| Klasifikasi | **`ADAPTER/VIEW`** — bukan entity, bukan aggregate |
| Context | `BC-RAD-01` dan `BC-RAD-02` |
| Ownership | **Adapter/View** — tidak memiliki data apa pun |
| Tujuan | Menyajikan pekerjaan hari itu pada satu alat, sehingga petugas tahu apa yang harus dikerjakan |
| Identitas | **Tidak punya identitas.** Ia adalah cara memandang data, bukan data |
| Sumber data | `RadOrder` dan `RadStudy` yang sudah ada, disaring menurut alat, status, dan tanggal |
| Invariant | Tidak menyimpan apa pun. Tidak dapat menjadi basi karena selalu dihitung saat diminta |
| Bukti | Belum ada di source. Bentuknya ditetapkan `RAD-DEC-012` |
| Decision | `RAD-DEC-012`, `RAD-DEC-013` |
| Keyakinan | Tinggi |

> **Mengapa ini bukan aggregate dan bukan entity.** Kontrak arsitektur melarang membuat entity
> hanya karena ada layar yang menampilkannya. Daftar kerja tidak punya siklus hidup, tidak punya
> invariant yang perlu dijaga, dan tidak ada yang "membuat" atau "menutup" sebuah daftar kerja.
> Ia hanya pertanyaan: "pekerjaan apa yang ada pada alat ini hari ini?"
>
> Konsekuensi praktisnya besar: **`S12` tidak membutuhkan tabel baru**, sehingga tidak ikut
> tertahan `RAD-CONFLICT-001` seperti hasil bacaan.

### `DC-RAD-13` — Penanda Mendesak pada Pesanan

| Field | Isi |
|---|---|
| Klasifikasi | `VALUE_OBJECT` yang melekat pada `RadOrder` |
| Context | `BC-RAD-01` |
| Ownership | **Extend** — menambah sifat pada aggregate yang sudah ada |
| Tujuan | Menyatakan bahwa pemeriksaan ini harus didahulukan |
| Siapa yang menetapkan | Dokter pengirim, saat memesan |
| Invariant | Penanda melekat pada pesanan dan ikut terbawa ke seluruh study yang lahir darinya |
| Bukti | Belum ada di source |
| Decision | `RAD-DEC-013` |
| Keyakinan | Tinggi |

> **Mengapa melekat pada pesanan, bukan pada study.** Cito adalah keputusan klinis dokter
> pengirim. Study lahir kemudian, dibuat petugas radiologi. Menaruh penandanya pada study
> berarti memindahkan keputusan klinis ke tangan yang bukan pemiliknya.

### Konsep milik modul lain yang dipakai, bukan dimiliki

| Konsep | Pemilik otoritatif | Cara dipakai | Larangan |
|---|---|---|---|
| Pasien | Patient Management | Lewat kunjungan | **Jangan** membuat `RadPatient` |
| Kunjungan (*encounter*) | Registration Management | Rujukan identitas | **Jangan** membuat salinan kunjungan |
| Perawatan rawat inap | InPatient Management | Rujukan opsional | **Jangan** menyalin data perawatan |
| Prosedur dan tarif | MasterData | Rujukan identitas | **Jangan** membuat katalog prosedur radiologi tersendiri |
| Dokter dan petugas | Human Resource / Identity | Rujukan identitas pelaku | **Jangan** membuat `RadDoctor` |
| Dokumen rekam medis | Clinical Management | Slot `Radiology` hanya untuk berkas unggahan dari luar | **Jangan** menyalin isi hasil bacaan ke sana |

---

## E. Model Aggregate

### `AGG-RAD-01` — Pesanan Radiologi

| Field | Isi |
|---|---|
| Root | `RadOrder` |
| Batas | Pesanan itu sendiri beserta riwayat perpindahan statusnya |
| Invariant | Perpindahan status hanya lewat jalur yang sah; pembatalan dan penolakan wajib beralasan; tidak ada kolom finansial |
| Tindakan bisnis | Buat, terima, jadwalkan, mulai, selesaikan, tahan, lanjutkan, tolak, batalkan |
| Domain event | Pesanan dibuat; pesanan diterima; pesanan dijadwalkan; pesanan ditutup |
| Konkurensi | Token versi. Dua petugas yang memindahkan status pesanan yang sama tidak boleh sama-sama berhasil |

### `AGG-RAD-02` — Study Radiologi

| Field | Isi |
|---|---|
| Root | `RadStudy` |
| Batas | Study, jawaban butir keselamatannya, dan catatan bahan terpakainya |
| Invariant | Acquisition ditolak sebelum identitas pasien terverifikasi dan seluruh butir wajib tuntas; pengulangan tidak menimpa study asli; kelayakan tagih hanya terbit untuk citra yang layak |
| Tindakan bisnis | Rencanakan, verifikasi pasien, jawab butir keselamatan, nyatakan lolos, mulai acquisition, selesaikan, hentikan, nilai mutu, ulangi, catat bahan |
| Domain event | Study dinyatakan lolos keselamatan; acquisition dimulai; acquisition selesai; mutu diputuskan; study diulang; **fakta kelayakan tagih diterbitkan** |
| Konkurensi | Token versi |

> **Mengapa jawaban butir keselamatan berada di dalam aggregate study, bukan berdiri sendiri.**
> Karena invariantnya harus dijaga bersama: "acquisition tidak boleh dimulai selama butir wajib
> belum tuntas". Aturan itu hanya dapat ditegakkan bila status study dan jawaban butirnya
> berubah dalam satu batas konsistensi yang sama.

### `AGG-RAD-03` — Hasil Bacaan

| Field | Isi |
|---|---|
| Root | `RadReport` |
| Batas | Hasil bacaan beserta seluruh versinya |
| Invariant | Versi yang sudah dirilis tidak pernah berubah; koreksi selalu versi baru; pengesah wajib dokter radiolog; peran penulis dibekukan pada draf |
| Prasyarat lahir | Hanya atas study yang citranya dinyatakan layak |
| Tindakan bisnis | Tulis draf, sahkan, rilis, tulis draf amandemen, sahkan amandemen, rilis amandemen |
| Domain event | Bacaan dirilis; amandemen dirilis |
| Konkurensi | Token versi pada root, agar dua radiolog tidak mengesahkan draf yang sama bersamaan |

### `AGG-RAD-04` — Aturan Keselamatan

| Field | Isi |
|---|---|
| Root | `RadModalitySafetyRule` |
| Batas | Aturan itu sendiri beserta riwayat pengesahannya |
| Invariant | Hanya aturan Aktif yang dinilai; setiap pengesahan menaikkan versi tepat satu kali; penolakan wajib beralasan |
| Tindakan bisnis | Susun draf, ajukan pengesahan, sahkan, tolak, nonaktifkan |
| Domain event | Aturan disahkan, dengan nomor versi baru |
| Catatan | `RadModality` dan `RadSafetyRequirement` tetap data rujukan biasa di context yang sama, bukan bagian aggregate ini |

---

## F. Model Relasi

Relasi logis beserta pembenaran bisnisnya. Bentuk kunci fisiknya dirancang tahap berikutnya.

| Sumber | Tujuan | Makna | Kardinalitas | Wajib | Ketergantungan lifecycle |
|---|---|---|---|---|---|
| `RadOrder` | Kunjungan pasien | Pesanan dibuat dalam konteks satu kunjungan | Banyak ke satu | Ya | Pesanan tidak dapat berdiri tanpa kunjungan |
| `RadOrder` | Perawatan rawat inap | Pesanan dapat dinaungi satu perawatan | Banyak ke satu | **Tidak** | Kosong untuk pasien rawat jalan |
| `RadOrder` | Prosedur | Pemeriksaan apa yang dipesan | Banyak ke satu | Ya | — |
| `RadOrder` | Modalitas | Alat apa yang diminta; menentukan aturan keselamatan mana yang berlaku | Banyak ke satu | Ya | — |
| `RadStudy` | `RadOrder` | Study mengerjakan satu pesanan | Banyak ke satu | Ya | Study tidak dapat berdiri tanpa pesanan |
| `RadStudy` | `RadStudy` | Study ini mengulang study sebelumnya | Banyak ke satu | **Tidak** | Kosong berarti bukan pengulangan. Study asli **tetap ada** |
| `RadStudySafetyCheck` | `RadStudy` | Jawaban satu butir pada satu study | Banyak ke satu | Ya | Ikut mati bila study dibatalkan |
| `RadStudySafetyCheck` | `RadSafetyRequirement` | Butir apa yang dijawab | Banyak ke satu | Ya | — |
| `RadAcquisitionConsumption` | `RadStudy` | Bahan terpakai pada satu study | Banyak ke satu | Ya | — |
| `RadModalitySafetyRule` | `RadModality` | Aturan berlaku untuk alat mana | Banyak ke satu | Ya | — |
| `RadModalitySafetyRule` | `RadSafetyRequirement` | Butir mana yang diatur | Banyak ke satu | Ya | — |
| `RadReport` | `RadStudy` | Bacaan atas satu study | **Satu ke paling banyak satu** | Ya | Bacaan tidak dapat berdiri tanpa study |
| `RadReportVersion` | `RadReport` | Satu versi isi bacaan | Banyak ke satu | Ya | — |
| `RadReportVersion` | `RadReportVersion` | Versi ini menggantikan versi sebelumnya | Satu ke paling banyak satu | **Tidak** | Kosong berarti versi pertama |
| `RadTransitionHistory` | `RadOrder` / `RadStudy` | Jejak perpindahan status | Banyak ke satu | Ya | Tidak pernah dihapus |

### Catatan pada relasi `RadReport` ke `RadStudy`

Dimodelkan **satu study paling banyak satu bacaan**. Alasannya: `RJ-BIL-GATE-DEC-004` menyebut
rantai `Order → Study → Report`, dan pengulangan sudah ditangani dengan membuat **study baru**,
bukan bacaan baru pada study lama.

> **Contoh.** CT-Scan Tn. B diulang karena citra pertama kabur. Yang terjadi: study pertama
> berakhir `QualityRejected` dan **tidak pernah punya bacaan**, karena bacaan hanya lahir atas
> study yang citranya layak. Study kedua berhasil dan punya satu bacaan. Rantainya tetap jelas.

Kemungkinan satu study punya **bacaan sementara** lalu **bacaan final** — praktik yang ada di
sebagian rumah sakit — tidak terdapat dalam bukti mana pun. Dicatat sebagai `GAP-RAD-02`, tidak
dirancang.

---

## G. Model Lifecycle dan Status

### G.1 Pesanan Radiologi

| Dari | Tindakan | Ke | Wewenang | Prasyarat |
|---|---|---|---|---|
| — | Buat pesanan | `Requested` | Dokter pengirim | Kunjungan, prosedur, dan alat terisi |
| `Requested` | Terima | `Accepted` | Petugas Radiologi | — |
| `Requested` | Tolak | `Rejected` | Petugas Radiologi | Alasan wajib |
| `Accepted` | Jadwalkan | `Scheduled` | Petugas Radiologi | — |
| `Scheduled` | Mulai | `InProgress` | Petugas Radiologi | — |
| `InProgress` | Selesaikan | `Completed` | Petugas Radiologi | — |
| Beberapa status | Tahan | `OnHold` | Petugas Radiologi | Status sebelumnya disimpan |
| `OnHold` | Lanjutkan | Status sebelum ditahan | Petugas Radiologi | — |
| Beberapa status | Batalkan | `Cancelled` | Sesuai wewenang | Alasan wajib |
| Setelah diserahkan | Minta batal | `CancelRequested` | Dokter pengirim | Dokter tidak lagi membatalkan langsung |

**Status `Draft` ada tetapi tidak dipakai** (`RAD-DEC-011`). Nilainya dipertahankan supaya
angka status lain tidak bergeser.

**Status terminal:** `Completed`, `Cancelled`, `Rejected`.

### G.2 Study Radiologi

| Dari | Tindakan | Ke | Wewenang | Prasyarat |
|---|---|---|---|---|
| — | Rencanakan | `Planned` | Petugas Radiologi | Pesanan sudah diterima |
| `Planned` | Verifikasi pasien | `PatientVerified` | Radiografer | Identitas cocok dengan pesanan |
| `PatientVerified` | Nyatakan lolos keselamatan | `SafetyCleared` | Radiografer | **Seluruh butir wajib tuntas dan ada aturan aktif** |
| `SafetyCleared` | Mulai acquisition | `AcquisitionStarted` | Radiografer | — |
| `AcquisitionStarted` | Selesaikan | `Acquired` | Radiografer | — |
| `AcquisitionStarted` | Hentikan | `Aborted` | Radiografer | Sebab dan alasan wajib |
| `Acquired` | Nilai mutu — layak | `QualityAccepted` | Radiografer atau radiolog | **Menerbitkan fakta kelayakan tagih** |
| `Acquired` | Nilai mutu — tidak layak | `QualityRejected` | Radiografer atau radiolog | Tidak menerbitkan fakta tagih |
| `QualityRejected` | Tandai perlu ulang | `RepeatRequired` | Petugas Radiologi | — |
| `RepeatRequired` | Buat study pengulangan | Study **baru** `Planned` | Petugas Radiologi | Sebab pengulangan wajib. Study lama tetap ada |

**Perpindahan yang tidak sah dan wajib ditolak:**

| Percobaan | Mengapa ditolak |
|---|---|
| `Planned` langsung ke `AcquisitionStarted` | Identitas dan keselamatan belum diperiksa |
| `PatientVerified` langsung ke `AcquisitionStarted` | Gerbang keselamatan dilewati |
| Menilai mutu pada study yang belum `Acquired` | Belum ada citra untuk dinilai |
| Mengubah study yang sudah `QualityAccepted` | Fakta tagih sudah terbit |

### G.3 Hasil Bacaan

| Dari | Tindakan | Ke | Wewenang | Prasyarat |
|---|---|---|---|---|
| — | (otomatis saat citra layak) | `Pending` | Sistem | Study `QualityAccepted` |
| `Pending` | Tulis draf | `Drafted` | Radiolog, residen, radiografer, atau bantuan AI | Peran penulis dibekukan |
| `Drafted` | Sahkan | `Validated` | **Dokter radiolog** | Bila penulisnya bukan radiolog, pengesah wajib orang lain |
| `Validated` | Rilis | `Released` | Dokter radiolog | — |
| `Released` | Tulis draf amandemen | `AmendmentDrafted` | Sama seperti penulisan draf | **Versi lama tetap dapat dibaca** |
| `AmendmentDrafted` | Sahkan amandemen | `AmendmentValidated` | Dokter radiolog | Aturan pengesahan sama |
| `AmendmentValidated` | Rilis amandemen | `AmendmentReleased` | Dokter radiolog | Menjadi versi berlaku; versi sebelumnya tetap tersimpan |

**Perpindahan yang tidak sah:**

| Percobaan | Mengapa ditolak |
|---|---|
| Penulis bukan-radiolog mengesahkan drafnya sendiri | `RAD-DEC-003` |
| Mengubah isi versi yang sudah `Released` | Koreksi wajib lewat amandemen berversi |
| Menghapus versi mana pun | Riwayat klinis tidak boleh hilang |
| Menulis bacaan atas study yang citranya tidak layak | Tidak ada citra yang sah untuk dibaca |

**Status terminal:** tidak ada. Hasil yang sudah dirilis selalu dapat diamandemen.

### G.4 Aturan Keselamatan

| Dari | Tindakan | Ke | Wewenang | Prasyarat |
|---|---|---|---|---|
| — | Susun draf | `Draf` | Admin Radiologi | Alat dan butir sudah ada |
| `Draf` | Ajukan pengesahan | `Menunggu persetujuan` | Admin Radiologi | Isi lengkap |
| `Menunggu persetujuan` | Sahkan | `Aktif`, versi naik | **Penanggung jawab klinis** | — |
| `Menunggu persetujuan` | Tolak | `Draf` | Penanggung jawab klinis | Alasan wajib |
| `Aktif` | Nonaktifkan | `Nonaktif` | Penanggung jawab klinis | — |

---

## H. Tanggung Jawab Authorization

Batas wewenang **bisnis**. Pemetaan ke peran Quilvian belum ada — itu `DEC-RAD-004`, dan tidak
menahan arsitektur ini.

| Kemampuan | Memulai | Melihat | Mengubah | Mengesahkan | Membatalkan / mengoreksi |
|---|---|---|---|---|---|
| Pesanan | Dokter pengirim | Dokter pengirim, Radiologi | Radiologi setelah diserahkan | — | Dokter langsung sampai `Requested`; sesudahnya berupa permintaan |
| Study dan keselamatan | Radiografer | Radiologi, dokter pengirim | Radiografer | — | Radiografer, dengan sebab tercatat |
| Mutu citra | Radiografer atau radiolog | Radiologi | — | — | Melalui pengulangan, bukan penghapusan |
| Hasil bacaan | Radiolog, residen, radiografer, bantuan AI | Dokter pengirim setelah dirilis | Penulis, selama masih draf | **Dokter radiolog** | Melalui amandemen berversi |
| Aturan keselamatan | Admin Radiologi | Radiologi | Admin Radiologi | **Penanggung jawab klinis** | Penanggung jawab klinis |
| Data induk alat | Admin Radiologi | Radiologi | Admin Radiologi | — | Nonaktifkan, bukan hapus |

**Batas yang tidak boleh dilanggar:** Radiologi **tidak memiliki** wewenang finansial apa pun.
Tidak dapat mengubah status terbayar, persetujuan penjamin, pembebasan, pelunasan, pembatalan
tagihan, pengembalian dana, maupun pembalikan.

---

## I. Model Audit dan Histori

| Kejadian | Yang wajib terekam | Boleh diubah? |
|---|---|---|
| Setiap perpindahan status pesanan dan study | Pelaku, waktu, status asal, status tujuan, alasan | Tidak |
| Jawaban butir keselamatan | Pelaku, waktu, keadaan jawaban, butir mana | Tidak |
| Pernyataan lolos keselamatan | Pelaku, waktu, **versi aturan yang berlaku saat itu** | Tidak |
| Penilaian mutu citra | Pelaku, waktu, layak atau tidak, catatan | Tidak |
| Pengulangan study | Study asal, sebab, alasan, pemberi wewenang | Tidak |
| Bahan terpakai | Jenis, jumlah, satuan, pelaku, waktu | Tidak |
| Penerbitan fakta kelayakan tagih | Waktu, penanda sudah terkirim | Tidak |
| Setiap versi hasil bacaan | Penulis, **peran penulis saat menulis**, pengesah, waktu, alasan perubahan, versi sebelumnya | Tidak |
| Pengesahan aturan keselamatan | Pengesah, waktu, nomor versi baru | Tidak |

> **Mengapa peran penulis ikut dibekukan.** Peran seseorang berubah seiring waktu. Seorang
> residen yang menulis draf hari ini dapat lulus menjadi spesialis enam bulan lagi. Draf lamanya
> harus tetap dinilai memakai peran saat draf itu ditulis. Menyimpan hanya nama penulis akan
> membuat aturan pengesahan berubah arti di kemudian hari.

---

## J. Model Integrasi

### J.1 Radiologi → Billing Management

| Field | Isi |
|---|---|
| Produsen | `BC-RAD-02` |
| Konsumen | Billing Management |
| Sumber kebenaran | Radiologi untuk **faktanya**; Billing untuk **akibat finansialnya** |
| Tujuan bisnis | Memberi tahu bahwa satu pemeriksaan dikerjakan dan citranya layak |
| Pemicu | Study berpindah ke `QualityAccepted` |
| Satuan | Satu fakta per study |
| Arah | Satu arah, Radiologi ke Billing |
| Idempotency | Penanda "fakta sudah terkirim" pada study mencegah pengiriman ganda |
| Kontrak | `BIL-INTEGRATION-0.4` |
| Bila gagal | Study tetap `QualityAccepted`; penanda tidak diset, sehingga dapat dikirim ulang |
| Bukti | `Services/RadStudyService.cs:517-524@64da911` |

**Yang bukan pemicu tagihan:** pesanan dibuat, pesanan diterima, pesanan dijadwalkan, dan
**hasil bacaan dirilis**. Dikunci `RJ-BIL-GATE-DEC-004`.

### J.2 Radiologi → Clinical Management

| Field | Isi |
|---|---|
| Produsen | `BC-RAD-03` |
| Konsumen | Clinical Management, rekam medis, CPPT |
| Sumber kebenaran | **Radiologi**, selalu |
| Arah | Clinical Management **membaca**; tidak menyalin |
| Alasan | Koreksi berversi harus langsung terlihat. Salinan berisiko menampilkan versi yang sudah digantikan |
| Bila tidak dapat dihubungi | Rekam medis wajib menampilkan pesan gangguan, **bukan** daftar kosong |
| Decision | `RAD-DEC-006` |

> **Bahaya yang dicegah.** dr. Andi membaca hasil pukul 08.00. Pukul 09.00 hasil diamandemen.
> Pukul 10.00 dr. Andi membuka lagi. Dengan baca langsung, ia melihat versi terbaru. Dengan
> salinan yang lupa diperbarui, ia masih melihat versi pukul 08.00 tanpa tahu ada koreksi.

### J.3 Modul IGD dan Rawat Inap → Radiologi

| Field | Isi |
|---|---|
| Produsen | IGD, Rawat Inap, Rawat Jalan |
| Konsumen | `BC-RAD-01` |
| Tujuan | Memesan pemeriksaan lewat jalur resmi yang sama |
| Catatan | Pesanan sudah mendukung konteks rawat inap; IGD belum tersambung — lihat `RAD-CONFLICT-002` |
| Decision | `RAD-DEC-009` |

### J.4 Integrasi eksternal

**Tidak ada.** RIS, PACS, dan DICOM sengaja tidak diaktifkan (`RJ-BIL-GATE-DEC-004`,
`RAD-DEC-001`). Tempat penampung pengenal study eksternal sudah ada di `RadStudy` tetapi tidak
dipakai. Tidak ada kontrak pihak ketiga yang dirancang di sini.

---

## K. Dampak Billing

**Klasifikasi: berdampak pada charge.**

| Kejadian | Akibat finansial | Pemutus |
|---|---|---|
| Pesanan dibuat, diterima, dijadwalkan | Tidak ada | — |
| Acquisition dimulai lalu dibatalkan | **Tidak otomatis** batal penuh maupun tagih penuh | Billing, berdasarkan bagian yang dikerjakan dan bahan terpakai |
| Citra dinyatakan **layak** | Fakta kelayakan tagih terbit | Billing menentukan nominalnya |
| Citra dinyatakan **tidak layak** | Tidak ada fakta tagih | Billing menilai bahan yang terlanjur terpakai |
| Pengulangan karena kesalahan internal | **Tidak otomatis** menambah tagihan pasien | Billing, lewat mekanisme penghapusan biaya internal |
| Pengulangan karena kebutuhan klinis baru | Perlu pesanan yang sah | Billing |
| Hasil bacaan dirilis | **Tidak ada** | — |

Radiologi **tidak menghitung** tarif, tidak menyimpan nominal, dan tidak menentukan penjamin.

---

## L. Dampak Keselamatan Klinis

**Klasifikasi: relevan terhadap keselamatan.**

| Kemampuan | Risiko bila ambigu | Batas yang ditegakkan |
|---|---|---|
| Verifikasi identitas pasien | Pasien salah disinari | Acquisition ditolak sebelum identitas diverifikasi |
| Gerbang keselamatan | Pasien hamil disinari; pasien berimplan logam masuk MRI; pasien alergi diberi kontras | Menolak bila aturan belum ada, dan menolak bila butir wajib belum tuntas |
| Versi aturan dibekukan | Penilaian lama tertulis ulang oleh aturan baru | Versi aturan dicatat pada study |
| Pengesahan hasil bacaan | Bacaan non-spesialis sampai ke dokter pengirim | Pengesah wajib dokter radiolog |
| Amandemen berversi | Dokter membaca versi yang sudah salah | Versi lama tersimpan, versi berlaku selalu yang terbaru |
| Rekam medis baca langsung | Dokter membaca salinan basi | Tidak ada salinan |

### Sifat fail-closed dan akibatnya sekarang

Gerbang keselamatan menolak ketika **belum ada aturan aktif** untuk sebuah alat. Ini perilaku
yang benar: tidak adanya aturan berarti belum ada yang menetapkan apa yang aman, bukan berarti
semuanya aman.

Akibatnya nyata dan harus disampaikan: **selama aturan keselamatan belum dapat dikelola dan
disahkan, modul ini tidak dapat menjalankan satu pun pemeriksaan.** Slice `S4` karena itu
bukan pelengkap, melainkan syarat hidup modul.

### Yang sengaja tidak dirancang di sini

Pelewatan gerbang keselamatan darurat (`S5`) **tidak** dirancang. Rumusannya sudah ada di
`RAD-DEC-008`, tetapi tanda tangan tata kelola klinis belum ada (`DEC-RAD-001`). Merancangnya
sekarang berarti mengarang wewenang klinis.

---

## M. Gap Arsitektur

| ID | Gap | Sifat | Memblokir |
|---|---|---|---|
| `GAP-RAD-01` | Ruang bagi penanda temuan kritis pada hasil bacaan belum berbentuk | Struktural, menunggu `DEC-RAD-002` | `S11` saja |
| `GAP-RAD-02` | Kemungkinan satu study punya bacaan sementara lalu bacaan final tidak ada dalam bukti mana pun | Perluasan, bukan perubahan | Tidak memblokir |
| `GAP-RAD-03` | Pemetaan empat sebutan peran ke peran Quilvian | Implementasi | Tidak memblokir arsitektur |
| `GAP-RAD-04` | Registry `Rad` masih `PLANNED` | Tata kelola | Implementasi `AGG-RAD-03`, bukan perancangannya |
| `GAP-RAD-05` | Isi awal aturan keselamatan belum ditetapkan | Data | Kesiapan pakai, bukan struktur |

### Catatan pada `GAP-RAD-01` — pesan untuk tahap desain

Saat merancang `RadReport`, **sediakan ruang** bagi penanda temuan kritis tanpa menetapkan
bentuknya.

Alasannya: bila `DEC-RAD-002` kelak memutuskan ada daftar resmi temuan kritis, dibutuhkan data
induk tersendiri dan hasil bacaan menunjuk ke butir daftar itu. Bila diputuskan tidak ada
daftar, cukup satu penanda. Dua jawaban itu menghasilkan bentuk berbeda.

Yang **tidak boleh** dilakukan: menebak salah satunya sekarang, lalu membongkarnya nanti.
Yang **boleh**: merancang `RadReport` sedemikian rupa sehingga penambahan penanda kelak tidak
mengubah identitas, lifecycle, maupun aturan pengesahannya.

---

## N. Kesiapan Arsitektur

### `DOMAIN_ARCHITECTURE_PARTIAL`

**Slice yang siap diserahkan ke `design-business-module`:**

| Slice | Nama | Konsep domain terkait |
|---|---|---|
| `S1` | Pesanan radiologi | `AGG-RAD-01` |
| `S2` | Study dan pengambilan citra | `AGG-RAD-02` |
| `S3` | Penilaian gerbang keselamatan | `AGG-RAD-02`, `AGG-RAD-04` |
| `S4` | Pengelolaan aturan keselamatan | `AGG-RAD-04` |
| `S6` | Mutu, pengulangan, penghentian | `AGG-RAD-02` |
| `S7` | Pencatatan pemakaian bahan | `AGG-RAD-02` |
| `S8` | Fakta kelayakan tagih | `DC-RAD-11` |
| `S9` | Hasil bacaan radiolog | `AGG-RAD-03` |
| `S10` | Koreksi hasil berversi | `AGG-RAD-03` |
| `S12` | Daftar kerja petugas | `DC-RAD-12`, `DC-RAD-13` |
| `S13` | Data induk alat pencitraan | `DC-RAD-06`, `DC-RAD-07` |
| `S14` | Penyajian hasil ke rekam medis | Integrasi `J.2` |

Kedua belasnya dinyatakan **berdiri sendiri** dari slice yang tertahan. Tidak satu pun
bergantung pada `S5` maupun `S11`.

**Slice yang harus berhenti:**

| Slice | Blocker | Pemilik |
|---|---|---|
| `S5` Pelewatan gerbang darurat | `DEC-RAD-001` | Tata kelola klinis — belum ditunjuk |
| `S11` Temuan kritis | `DEC-RAD-002` | Tata kelola klinis — belum ditunjuk |

**Catatan urutan pengerjaan yang penting.** Walaupun `S9` hasil bacaan adalah inti Rilis 1,
`S4` pengelolaan aturan keselamatan **harus dikerjakan lebih dulu atau bersamaan**. Tanpa `S4`,
tidak satu pun pemeriksaan dapat berjalan, sehingga tidak akan pernah ada citra layak untuk
dibaca.

---

## O. Jejak Requirement ke Domain

| Decision | Konsep atau aturan domain yang lahir darinya |
|---|---|
| `RJ-BIL-GATE-DEC-004` | Lifecycle ketiga aggregate; pemisahan selesai pesanan dan rilis hasil; amandemen berversi; invariant tagih |
| `RJ-BIL-DEC-014` | Sifat fail-closed gerbang keselamatan; aturan keselamatan sebagai data yang dapat diubah |
| `RJ-BIL-GATE-DEC-005` | Bahan terpakai dicatat sebagai jumlah, bukan nominal |
| `RAD-DEC-001` | Batas scope; `S9` dan `S10` masuk Rilis 1 |
| `RAD-DEC-002` | Enam modalitas; butir keselamatan berbeda per alat |
| `RAD-DEC-003` | Invariant pengesahan `AGG-RAD-03`; pembekuan peran penulis |
| `RAD-DEC-005` | `RadModalitySafetyRule` naik menjadi aggregate root berversi |
| `RAD-DEC-006` | Integrasi `J.2` baca langsung; larangan salinan |
| `RAD-DEC-009` | Integrasi `J.3` jalur pemesanan seragam |
| `RAD-DEC-011` | Status `Draft` dipertahankan tetapi tidak dipakai |
| `RAD-DEC-012` | `DC-RAD-12` daftar kerja sebagai `ADAPTER/VIEW`, bukan tabel baru |
| `RAD-DEC-013` | `DC-RAD-13` penanda cito melekat pada `RadOrder` |

---

## P. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Arsitektur domain pertama untuk 11 slice siap. Empat bounded context, empat aggregate, 11 konsep domain. Lima gap arsitektur dicatat. | `draft` |
| 2 | 2026-09-09 | `S12` daftar kerja masuk setelah `DEC-RAD-003` ditutup. Dua konsep ditambahkan: `DC-RAD-12` daftar kerja sebagai `ADAPTER/VIEW` dan `DC-RAD-13` penanda cito. Tidak ada aggregate baru. Slice siap menjadi 12. | `draft` |
