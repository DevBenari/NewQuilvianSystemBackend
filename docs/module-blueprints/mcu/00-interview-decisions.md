# MCU (Medical Check-Up) — Keputusan Wawancara (Interview Decisions)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` |
| Revision | `5` |
| Pass | Scope pass (r1), closure pass (r2), putaran 3 sesudah penilaian kelengkapan (r3), dan putaran 4 (r4) — 20 Oktober 2026; putaran 5 sesudah persetujuan lintas modul dan pembuktian teknis (r5) — 21 Oktober 2026 |
| Capability map | [01-existing-capability-map.md](01-existing-capability-map.md) r1 (2026-10-20) — disusun **sesudah** scope pass; empat pertentangan baru `MCU-CONFLICT-005`..`008` dan pertanyaan penutup `MCU-Q-01`..`07` menunggu closure pass |
| Status | `approved` untuk `MCU-DEC-001`..`043`; butir terbuka tercatat di bagian 10 |
| Pemilik produk/domain & approver | Yoga Aji Pratama — `MCU-DEC-002` |
| SHA backend | `4d3e4f98` (dibaca untuk fakta awal, bukan audit) |
| SHA frontend | `7fb1e1d3e` (dibaca untuk fakta awal, bukan audit) |
| Bukti input | [evidence/2026-10-19-kebutuhan-mcu.md](evidence/2026-10-19-kebutuhan-mcu.md) (`MCU-EVD-001`); [evidence/2026-10-21-persetujuan-dan-pembuktian.md](evidence/2026-10-21-persetujuan-dan-pembuktian.md) (`MCU-EVD-002`, putaran 5) |

> **Catatan audit.** Scope pada pass ini dikunci **sebelum** capability map resmi disusun.
> Fakta kode di bawah hanya hasil pembacaan cepat read-only agar pertanyaan tidak salah arah.
> Kemungkinan duplikasi dengan kemampuan yang sudah ada belum diperiksa menyeluruh dan menjadi
> tugas `/trace-existing-capabilities`.

## 1. Scope dan hasil yang diharapkan

**Batas scope (disahkan `MCU-DEC-001`):** layanan Medical Check-Up pasien —
dari master paket, pemesanan, jadwal, pendaftaran, dan check-in peserta, sampai penampungan order
paket, pemantauan kelengkapan hasil, persetujuan dokter, resume hasil MCU, penyerahan hasil, dan
laporan MCU. Pelaksanaan pemeriksaan Lab/Radiologi, pembayaran, kanal pesan, dan layar kiosk
tetap milik modulnya masing-masing.

**Hasil yang diharapkan:** setiap peserta MCU tercatat dalam satu "order paket MCU" yang dapat
dilacak dari pemesanan sampai hasil diterima, dan setiap komponen di dalamnya tetap dikerjakan
oleh unit pelaksananya.

> **Contoh.** Budi memesan Paket MCU Basic (darah lengkap, gula darah puasa, rontgen dada) lewat
> WhatsApp untuk 22 Oktober. Admisi MCU mencatat pesanannya. Pada 22 Oktober Budi check-in,
> membayar, menjalani skrining, lalu sistem membuat pesanan Laboratorium (2 pemeriksaan) dan
> pesanan Radiologi (1 pemeriksaan). Daftar Pasien MCU menunjukkan "2 dari 3 komponen selesai"
> sampai rontgen tervalidasi. Dokter MCU menyusun kesimpulan, hasil diverifikasi, lalu petugas
> MCU mengirim berkas hasil ke WhatsApp Budi.

### 1.1 Di dalam scope

| No | Kemampuan | Butir sumber |
| ---: | --- | --- |
| 1 | Master Paket MCU, komponen paket, tarif paket, dan persyaratan pemeriksaan | 1A–1D |
| 2 | Pemesanan (booking) MCU yang dicatat admisi dari telepon/WhatsApp | 2 |
| 3 | Jadwal MCU | 3 |
| 4 | Pendaftaran peserta MCU oleh admisi MCU | 4 |
| 5 | Check-in peserta oleh admisi (pencatatan statusnya) | 5 |
| 6 | Daftar Pasien MCU sebagai penampung seluruh order paket dari semua pintu | 6, 13F |
| 7 | Penerimaan order paket dari lima pintu: MCU langsung, Rawat Jalan, Rawat Inap, IGD, kiosk | 13A–13E |
| 8 | Pemecahan paket menjadi pesanan Lab/Radiologi sesuai layanannya | 13F |
| 9 | Pemantauan kelengkapan hasil komponen | 8 |
| 10 | Persetujuan dokter dan verifikasi hasil | 9 |
| 11 | Laporan hasil MCU per peserta (kesimpulan, temuan penting, rekomendasi) | 10 |
| 12 | Pencatatan penyerahan hasil oleh petugas MCU | 12 |
| 13 | Laporan MCU: rekap kunjungan, rekap per metode pembayaran, rekap pemeriksaan & paket, statistik | 14A–14D |

### 1.2 Di luar scope — untuk modul lain (hanya titik sentuh yang dibahas)

| No | Kemampuan | Modul pemilik | Titik sentuh yang tetap dibahas di MCU |
| ---: | --- | --- | --- |
| 1 | Layar pendaftaran dan check-in di kiosk | `kiosk` | Data apa yang kiosk kirim ke MCU, dan kapan peserta dianggap check-in |
| 2 | Kanal WhatsApp, email, dan telepon otomatis | `platform-notification` | Kejadian MCU apa yang memicu pesan dan siapa penerimanya |
| 3 | Formulir skrining awal (dipakai bersama rawat jalan) | `rawat-jalan` | Apakah skrining wajib sebelum komponen MCU dikerjakan |
| 4 | Tagihan, pembayaran, gerbang lunas, PPN | `billing-kasir` | Paket ditagih sebagai apa; siapa pembayarnya; kapan dianggap lunas |
| 5 | Pelaksanaan, hasil, dan validasi pemeriksaan Lab | `laboratorium` | Bentuk pesanan Lab dari paket dan status hasil yang dibaca MCU |
| 6 | Pelaksanaan, hasil, dan ekspertise Radiologi | `radiologi` | Sama dengan Lab |
| 7 | Tombol "rekomendasikan MCU" milik dokter RJ/RI/IGD | `rawat-jalan`, `rawat-inap`, `igd` | Isi rekomendasi yang diterima MCU |
| 8 | MCU pra-kerja dan berkala karyawan rumah sakit | `human-resource` (`TrxEmployeeMedicalExamination`) | Tidak ada, kecuali diputuskan lain |
| 9 | Data identitas pasien | `patient-management` | MCU memakai pasien yang sudah ada |

## 2. Glossary (usulan)

| Istilah | Arti dalam dokumen ini |
| --- | --- |
| Paket MCU | Kumpulan komponen pemeriksaan yang dijual dengan satu nama dan satu tarif, misalnya *Paket Basic* |
| Komponen MCU | Satu pemeriksaan di dalam paket, misalnya *Gula Darah Puasa* (Lab) atau *Thorax PA* (Radiologi) |
| Pemesanan (booking) | Janji MCU yang dicatat sebelum peserta datang |
| Peserta MCU | Pasien yang menjalani paket MCU |
| Order paket MCU | Satu catatan per peserta per paket di Daftar Pasien MCU; induk dari pesanan Lab/Rad turunannya |
| Pintu order | Asal order paket: MCU langsung, rekomendasi dokter RJ/RI/IGD, atau kiosk |
| Laporan hasil MCU | Resume akhir per peserta: kondisi umum, temuan penting, rekomendasi. **Bukan** laporan rekap (butir 14) |

## 3. Fakta yang sudah ada

| ID | Fakta | Bukti |
| --- | --- | --- |
| `MCU-FACT-001` | Jenis kunjungan `MedicalCheckup` sudah ada di sistem | `EncounterType.MedicalCheckup = 4`; `ClinicType`/`ServiceUnitType` `MedicalCheckup = 7`; `DoctorServiceRuleType.MedicalCheckup = 6` |
| `MCU-FACT-002` | Belum ada modul, area backend, maupun blueprint MCU | Nol folder `Areas/**/Mcu*`; nol folder `docs/module-blueprints/mcu` sebelum dokumen ini |
| `MCU-FACT-003` | Laboratorium memutuskan pesanan dari kunjungan MCU memakai **jalur pemesanan biasa**; paket, harga paket, rekap perusahaan, dan penerima hasil selain pasien diserahkan ke layanan MCU | `LAB-DEC-164`, BR-115 (`laboratorium/00-interview-decisions.md`) |
| `MCU-FACT-004` | Gerbang Lunas Laboratorium sudah mengunci kunjungan MCU yang belum lunas (kode 409) | `AC-316` (`laboratorium/testing/acceptance-test-matrix.md`) |
| `MCU-FACT-005` | Tagihan MCU berjenis layanan `MCU` dan dikenai PPN seperti rawat jalan | `BKC-DEC-083` (`billing-kasir`); `BillingInvoiceService.MapServiceType` |
| `MCU-FACT-006` | Biaya administrasi belum punya jenis layanan `MCU` | `AdministrationFeeServiceTypes` hanya `RAJAL`, `IGD`, `OTC`, `RANAP` (`BE-BKC-026`) |
| `MCU-FACT-007` | Kiosk v2 sudah menampilkan "Klinik Medical Check Up" sebagai pilihan poliklinik | `kiosk/task/report/frontend/FE-KSK-001.md` |
| `MCU-FACT-008` | Frontend v1 punya menu MCU: dashboard, Daftar Pasien MCU (detail per kunjungan, modal billing, item, upload surat jaminan), Order MCU tambah/ubah, Pendaftaran Pasien MCU. Ada isian *Nama Perusahaan (Penjamin)* dan *Paket MCU*. Datanya diambil lewat pemesanan Lab (`fetchLabBookingMCUWithFilters`) | `C:\Users\Admin\QuilvianSystemFrontendDev\src\app\Medical-check-up\`, `src\components\view\MCU\` (5.124 baris) |
| `MCU-FACT-009` | Human Resource punya pemeriksaan kesehatan pegawai sendiri (kesehatan kerja), bukan layanan MCU pasien | `TrxEmployeeMedicalExamination` |

## 4. Pertentangan di dalam bukti

| ID | Pertentangan | Contoh dampak |
| --- | --- | --- |
| `MCU-CONFLICT-001` (**ditutup** `MCU-DEC-003`) | Butir 2 menyatakan pasien yang booking lewat telepon/WhatsApp **tidak perlu mendaftar lagi**, tetapi butir 4 dan 5 menyediakan pendaftaran **dan** check-in lewat kiosk/admisi | Bila booking sudah sama dengan pendaftaran, Budi yang datang ke kiosk hanya perlu check-in; bila tidak, Budi terdaftar dua kali dan muncul dua baris di Daftar Pasien MCU |
| `MCU-CONFLICT-002` (**ditutup** `MCU-DEC-004`) | Butir 11 mewajibkan **bayar dulu**, sedangkan v1 menyediakan *Penjamin Perusahaan* dan *surat jaminan* — perusahaan lazimnya membayar sesudah tagihan kolektif | 40 karyawan PT Contoh datang; bila bayar-dulu berlaku mutlak, tidak satu pun komponen dapat dikerjakan sebelum PT Contoh membayar |
| `MCU-CONFLICT-003` (**ditutup** `MCU-DEC-006`) | Butir 13C/13D membuka order MCU dari **Rawat Inap dan IGD**, padahal pasien rawat inap membayar saat pulang dan Gerbang Lunas Lab mengunci MCU yang belum lunas (`MCU-FACT-004`) | Pasien rawat inap yang direkomendasikan MCU tertahan di Lab sampai pulang |
| `MCU-CONFLICT-004` (**ditutup** `MCU-DEC-005`; sisi teknisnya `MCU-DEC-022`) | Butir 1C menetapkan **tarif paket**, sedangkan butir 13F membuat **pesanan Lab/Rad biasa** yang secara bawaan ditagih per pemeriksaan | Paket Rp 350.000 berisi tiga pemeriksaan bertarif satuan Rp 150.000, Rp 60.000, dan Rp 200.000; tanpa aturan, pasien bisa ditagih Rp 350.000 + Rp 410.000 = Rp 760.000 |
| `MCU-CONFLICT-005` (**ditutup** — booking MCU tidak memakai perjanjian rawat jalan) | Booking rawat jalan membuat kunjungan saat booking (`MCU-CAP-04`) | Lihat capability map |
| `MCU-CONFLICT-006` (**ditutup** `MCU-DEC-023`) | Kiosk mendaftarkan MCU sebagai kunjungan rawat jalan (`MCU-CAP-06`) | Lihat capability map |
| `MCU-CONFLICT-007` (**ditutup** `MCU-DEC-022`) | Lab/Rad menagih per pemeriksaan otomatis (`MCU-CAP-22`) | Lihat capability map |
| `MCU-CONFLICT-008` (**ditutup** `MCU-DEC-027`) | Pengiriman WhatsApp v1 dari browser ke server luar (`MCU-CAP-33`) | Lihat capability map |

## 5. Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `MCU-DEC-001` | Decision | Batas scope, daftar *di dalam* dan *di luar* scope pada bagian 1 disahkan apa adanya. Layar kiosk, kanal pesan, formulir skrining, tagihan, pelaksanaan Lab/Rad, tombol rekomendasi dokter, dan MCU karyawan rumah sakit tetap milik modul masing-masing | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-002` | Decision | Menutup `MCU-OQ-001`. Pemilik produk/domain modul MCU dan approver wawancara ini adalah **Yoga Aji Pratama** | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Mengisi "pemilik layanan MCU (belum ditetapkan)" pada `LAB-DEC-164` |
| `MCU-ASM-001` | Assumption | Nama folder modul `mcu` (kebab-case dari singkatan yang dipakai rumah sakit) | Agent | `draft` | — | — |
| `MCU-DEC-003` | Decision | Menutup `MCU-CONFLICT-001`. **Booking sama dengan pendaftaran.** Saat admisi mencatat booking (dari telepon/WhatsApp), order paket MCU langsung dibuat berstatus *Terjadwal*. Pada hari pemeriksaan peserta **cukup check-in** lewat kiosk, WhatsApp, atau admisi; check-in mengubah status menjadi *Hadir* dan **baru saat itu kunjungan MCU dibuat**. Pendaftaran butir 4 hanya untuk peserta yang datang tanpa booking (*walk-in*), dan pendaftaran itu langsung berstatus *Hadir*. **Contoh:** Budi booking Paket Basic via WhatsApp untuk 22 Oktober → satu baris *Terjadwal* di Daftar Pasien MCU, belum ada kunjungan maupun tagihan. 22 Oktober Budi check-in di kiosk → baris yang sama menjadi *Hadir* dan kunjungan `MedicalCheckup` terbentuk; tidak ada baris kedua | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Aturan booking yang tidak datang dibahas di pertanyaan antre no. 11 |
| `MCU-DEC-004` | Decision | Menutup `MCU-CONFLICT-002`. **Dua jenis peserta.** (1) **Perorangan** wajib membayar lebih dulu sebelum komponen dikerjakan (butir 11). (2) **Peserta perusahaan** yang datang dengan surat jaminan atau kerja sama perusahaan yang masih berlaku dianggap *terjamin*: komponen boleh dikerjakan tanpa bayar di tempat, dan tagihannya ditagihkan kolektif ke perusahaan oleh Billing. **Contoh:** Budi (perorangan) belum membayar → pesanan Lab dan Radiologinya tetap terkunci. Siti, karyawan PT Contoh dengan surat jaminan terunggah → pesanannya langsung dapat dikerjakan, dan Rp 350.000 masuk tagihan PT Contoh bulan itu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan; selaras v1 (`MCU-FACT-008`). Menimbulkan `MCU-COORD-001` |
| `MCU-COORD-001` | Open Question | Titik sentuh peserta perusahaan: (a) data perusahaan penjamin dan masa berlaku kerja samanya dibaca dari modul pemilik penjamin (dugaan `insurance-management`/`billing-kasir`, dipastikan lewat trace); (b) Gerbang Lunas Lab (`AC-316`) dan Radiologi harus mengenali peserta *terjamin* sebagai boleh dikerjakan; (c) tagihan kolektif ke perusahaan dikerjakan Billing | Pemilik `billing-kasir`, `insurance-management`, `laboratorium`, `radiologi` | `draft` | — | Koordinasi, bukan keputusan bisnis MCU. Memblokir `IMPLEMENTATION` jalur perusahaan, bukan jalur perorangan |
| `MCU-DEC-005` | Decision | Menutup `MCU-CONFLICT-004`. **Paket ditagih satu baris seharga paket.** Pesanan Lab/Radiologi yang lahir dari paket ditandai *bagian paket MCU* dan **tidak ditagih lagi** per pemeriksaan. Pemeriksaan tambahan di luar paket ditagih satuan seperti biasa. **Contoh:** Paket Basic Rp 350.000 berisi Darah Lengkap, Gula Darah Puasa, dan Thorax. Tagihan Budi hanya memuat satu baris *Paket Basic Rp 350.000*, bukan Rp 410.000. Bila dokter menambah Kolesterol Total Rp 75.000, tagihannya menjadi dua baris, total Rp 425.000 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Mempersempit `LAB-DEC-164` (jalur umum tetap, tetapi harga pesanan turunan paket nol/tak ditagih). Menimbulkan `MCU-COORD-002` |
| `MCU-COORD-002` | Coordination (**ditutup** putaran 5) | Tanda *bagian paket MCU* pada pesanan Lab dan Radiologi: Lab/Rad harus menyimpan tanda itu dan tidak mengirim baris tagihan per pemeriksaan; Billing harus menerima satu baris paket dari MCU. Bentuk teknisnya: kolom `CoveringMcuOrderId` pada `LabOrder`/`RadOrder` dan penahanan fakta tagih (`MCU-DEC-022`) | Pemilik `laboratorium`, `radiologi`, `billing-kasir` | `approved` | Pemilik `laboratorium` dan `radiologi`, disampaikan Yoga Aji Pratama 2026-10-21 | `MCU-EVD-002` bagian 1. Sisi Billing tidak berubah (`MCU-DEC-022`) |
| `MCU-DEC-006` | Decision | Menutup `MCU-CONFLICT-003`. **Rekomendasi MCU dari dokter Rawat Inap dan IGD menjadi booking MCU terpisah.** Rekomendasi membuat order paket berstatus *Terjadwal*, sama seperti booking (`MCU-DEC-003`). Saat peserta check-in MCU — sesudah pulang, atau saat masih dirawat bila memungkinkan — kunjungan `MedicalCheckup` baru dibuat dengan tagihan, PPN, dan aturan bayar MCU sendiri (`MCU-DEC-004`). Order MCU **tidak pernah** menumpang kunjungan rawat inap atau IGD. **Contoh:** Pak Andi dirawat karena tifus; dokter merekomendasikan Paket Lengkap. Daftar Pasien MCU memuat satu baris *Terjadwal* atas nama Pak Andi. Tagihan rawat inapnya tetap bebas PPN dan tidak memuat paket. Seminggu kemudian Pak Andi check-in MCU → kunjungan MCU terbentuk, ia membayar Paket Lengkap di kasir, lalu pemeriksaan dimulai | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-ASM-003` | Assumption (**disahkan** `MCU-DEC-031`) | Label status order paket MCU pada bagian 7 | Yoga Aji Pratama | `superseded` | — | Diturunkan dari keputusan scope pass |
| `MCU-ASM-002` | Assumption (**superseded** oleh `MCU-DEC-017`) | Pintu 13B (rekomendasi dokter Rawat Jalan) mengikuti pola yang sama dengan `MCU-DEC-006`: rekomendasi menjadi booking *Terjadwal*, bukan menumpang kunjungan poliklinik. Pintu 13E (kiosk tanpa booking) diperlakukan sebagai pendaftaran *walk-in* yang langsung *Hadir* (`MCU-DEC-003`) | Yoga Aji Pratama | `superseded` | — | Turunan langsung `MCU-DEC-003` dan `MCU-DEC-006`; disahkan `MCU-DEC-017` |
| `MCU-DEC-007` | Decision | **Tiga jenis komponen:** *Laboratorium*, *Radiologi*, dan *Pemeriksaan MCU*. Komponen Lab dan Radiologi menjadi pesanan di modulnya (`MCU-DEC-005`). Komponen *Pemeriksaan MCU* — misalnya pemeriksaan fisik dokter, tanda vital dan BMI, EKG, visus dan buta warna, gigi, audiometri, spirometri — **hasilnya diisi langsung di modul MCU** oleh petugas atau dokter MCU; inilah isi butir 8. **Contoh:** Paket Karyawan berisi Darah Lengkap, Thorax, Pemeriksaan Fisik, dan Visus. Sistem membuat satu pesanan Lab dan satu pesanan Radiologi; Pemeriksaan Fisik dan Visus muncul sebagai dua isian di layar hasil MCU. Daftar Pasien MCU menghitung keempatnya sebagai komponen: *2 dari 4 selesai* sampai semuanya terisi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Memperluas butir 1B. Isi formulir per jenis pemeriksaan menjadi `MCU-OQ-002` |
| `MCU-OQ-002` | Open Question | Daftar jenis *Pemeriksaan MCU* yang dipakai rumah sakit beserta isian tiap jenis (contoh: Visus = mata kanan/kiri, dengan/tanpa kacamata; Buta warna = normal/parsial/total) | Yoga Aji Pratama + dokter MCU | `draft` | — | Memblokir `IMPLEMENTATION` layar isi hasil, bukan desain domain. Bukti yang dibutuhkan: contoh formulir/cetakan MCU yang dipakai sekarang |
| `MCU-DEC-008` | Decision | **Pembagian tugas butir 9.** (1) **Dokter MCU** menyusun laporan hasil MCU (kesimpulan, temuan penting, rekomendasi) lalu **menyetujui (approve)** — ini tanggung jawab klinis. (2) **Petugas MCU** lalu **memverifikasi kelengkapan administratif**: identitas peserta benar, seluruh komponen ada, lampiran hasil Lab/Radiologi terlampir. Petugas **tidak boleh mengubah isi klinis**; bila menemukan salah isi, ia mengembalikan ke dokter. (3) Hanya hasil yang sudah terverifikasi yang boleh diserahkan. **Contoh:** dr. Rina approve resume Budi *Fit dengan catatan: kolesterol tinggi*. Petugas Dewi memeriksa dan mendapati lampiran Thorax belum ada → hasil belum bisa diserahkan sampai lampiran masuk. Dewi tidak dapat mengganti kata *Fit* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-009` | Decision | **Approve hanya sesudah semua komponen tuntas.** Tombol approve dokter terkunci sampai setiap komponen paket berstatus selesai — Lab dan Radiologi tervalidasi di modulnya, *Pemeriksaan MCU* terisi — **atau** komponen itu dibatalkan secara resmi. Resume tidak pernah disetujui dengan data yang belum ada. **Contoh:** Lab Budi sudah tervalidasi, tetapi Thorax belum diekspertise → Daftar Pasien MCU menampilkan *3 dari 4 selesai* dan dr. Rina belum bisa approve | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Arti *komponen dibatalkan resmi* dibahas di pertanyaan antre no. 11 |
| `MCU-DEC-010` | Decision | **Penerima hasil.** Hasil lengkap hanya diserahkan kepada **peserta**. Untuk peserta perusahaan, **perusahaan penjamin hanya menerima ringkasan kelayakan** — kategori kelayakan dan rekomendasi kerja — tanpa nilai pemeriksaan maupun diagnosis, **kecuali** peserta memberi persetujuan tertulis yang tercatat di sistem untuk melepas hasil lengkap kepada perusahaan. **Contoh:** Siti, karyawan PT Contoh, hasilnya *Unfit sementara: hipertensi, HBsAg reaktif*. Tanpa persetujuan Siti, PT Contoh hanya menerima *Unfit sementara — disarankan tidak bertugas di bagian pengolahan makanan sampai pemeriksaan ulang*. Kata *HBsAg reaktif* tidak pernah sampai ke PT Contoh | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menyentuh privasi data kesehatan; lihat `MCU-OQ-003` |
| `MCU-DEC-011` | Decision | **Koreksi hasil sesudah diserahkan.** Hanya **dokter MCU** yang boleh membuka ulang laporan hasil, dengan **alasan wajib diisi**. Versi lama tidak dihapus; ia disimpan dan ditandai *dikoreksi*. Versi baru wajib melewati verifikasi petugas (`MCU-DEC-008`) dan diserahkan ulang, dan setiap penerima versi lama ditandai *perlu diberi tahu*. **Contoh:** resume Budi terkirim 25 Oktober dengan kesimpulan *Fit*. 27 Oktober dr. Rina mendapati seharusnya *Unfit sementara*; ia membuka ulang dengan alasan *salah baca nilai tekanan darah*. Versi 1 tetap tersimpan bertanda *dikoreksi*, versi 2 diverifikasi Dewi lalu dikirim ulang ke Budi, dan PT Contoh — penerima ringkasan versi 1 — masuk daftar yang wajib diberi tahu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-OQ-003` | Open Question | Daftar **kategori kelayakan** (usulan: *Fit*, *Fit dengan catatan*, *Unfit sementara*, *Unfit*) dan bentuk **persetujuan pelepasan hasil** (formulir kertas yang diunggah, atau centang di pendaftaran) | Yoga Aji Pratama + dokter MCU (kategori); pemilik privasi/legal rumah sakit (persetujuan) | `draft` | — | Memblokir `IMPLEMENTATION` resume dan penyerahan ke perusahaan |
| `MCU-DEC-012` | Decision | **Jadwal MCU = kuota peserta per hari.** Admin MCU mengatur hari buka MCU dan kuota peserta per hari. Booking untuk hari yang kuotanya penuh ditolak. Rombongan perusahaan dicatat sekaligus dan setiap pesertanya memakan satu kuota. Jadwal **tidak** terikat dokter tertentu. **Contoh:** Senin 27 Oktober berkuota 30. Booking Budi masuk (1/30). Admisi lalu mencatat rombongan PT Contoh 25 orang (26/30). Rombongan kedua berisi 10 orang ditolak untuk hari itu karena hanya tersisa 4 kuota | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Pelampauan kuota dibahas `MCU-OQ-004` |
| `MCU-DEC-013` | Decision | **Persyaratan pemeriksaan dikonfirmasi saat check-in.** Persyaratan melekat pada komponen (misalnya *puasa 10–12 jam* pada Gula Darah Puasa) dan ikut tampil pada paket. Persyaratan ditampilkan saat booking dan diteruskan kepada peserta. Saat check-in petugas **wajib** menandai setiap persyaratan *terpenuhi* atau *tidak terpenuhi*. Bila tidak terpenuhi, petugas memilih: **lanjut dengan catatan** (catatannya terbawa ke pesanan dan resume), atau **jadwal ulang komponen yang terdampak saja**. **Contoh:** Budi check-in dan mengaku sudah sarapan. Petugas menandai *puasa: tidak terpenuhi* lalu memilih jadwal ulang Gula Darah Puasa ke 28 Oktober; Darah Lengkap, Thorax, dan Pemeriksaan Fisik tetap dikerjakan hari itu | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-OQ-004` | Open Question | Apakah kuota harian boleh dilampaui (misalnya oleh kepala unit MCU dengan alasan) untuk rombongan perusahaan, dan apakah jadwal ulang komponen (`MCU-DEC-013`) memakan kuota hari tujuannya | Yoga Aji Pratama | `draft` | — | Tidak memblokir desain; memblokir `IMPLEMENTATION` aturan booking |
| `MCU-DEC-014` | Decision | **Peserta yang tidak datang.** Order *Terjadwal* yang belum check-in sampai tutup hari otomatis berubah menjadi *Tidak Hadir*, dan kuotanya dilepas. Petugas boleh menjadwalkan ulang **order yang sama** ke tanggal baru (kembali *Terjadwal*); tanggal lama tetap tercatat di riwayat. Karena kunjungan dan tagihan baru dibuat saat check-in (`MCU-DEC-003`), tidak ada kunjungan atau tagihan yang perlu dibatalkan. **Contoh:** Budi booking 27 Oktober dan tidak datang. Pukul 23.59 ordernya menjadi *Tidak Hadir* dan kuota 27 Oktober kembali 29/30. 28 Oktober Budi menelepon; petugas menjadwal ulang ke 30 Oktober, dan riwayat order mencatat *27 Okt — Tidak Hadir; dijadwal ulang ke 30 Okt* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Jam *tutup hari* ditetapkan saat desain (`DEV_DISCRETION` tidak berlaku; usulan 23.59 WIB) |
| `MCU-DEC-015` | Decision | **Pembatalan sebagian komponen sesudah bayar.** Master paket memuat **nilai pengembalian** tiap komponen yang diisi admin; jumlah seluruh nilai pengembalian **tidak boleh melebihi harga paket**. Hanya **dokter MCU** yang boleh membatalkan komponen, dengan alasan wajib. Pembatalan memicu pengembalian sebesar nilai itu lewat Billing, dan komponen yang dibatalkan dianggap tuntas bagi kunci approve (`MCU-DEC-009`). **Contoh:** Paket Basic Rp 350.000 dengan nilai pengembalian Darah Lengkap Rp 100.000, Gula Darah Puasa Rp 40.000, Thorax Rp 150.000 (total Rp 290.000 ≤ Rp 350.000 — sah). Siti ternyata hamil; dr. Rina membatalkan Thorax dengan alasan *kehamilan*. Siti menerima pengembalian Rp 150.000, bukan Rp 200.000 tarif satuan Thorax. Bila admin mencoba mengisi total Rp 400.000, master menolak karena melebihi harga paket | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-003` |
| `MCU-COORD-003` | Open Question | Pengembalian dana komponen MCU (`MCU-DEC-015`) dan perlakuannya untuk peserta perusahaan — potong tagihan kolektif, bukan uang tunai — dikerjakan Billing. **Putaran 5:** jalur refund yang ada terbukti **tidak** menerima nominal sebagian (`MCU-EVD-002` 2.2); bentuk perluasannya ditetapkan `MCU-DEC-043`. **Disetujui 2026-10-21** oleh Yasmina, pemilik `billing-kasir`, termasuk penutupan celah refund ganda. Bentuk teknis akumulasi per baris dan perlakuan peserta perusahaan (potong tagihan kolektif) ditetapkan pemilik Billing saat `BE-MCU-026` | Pemilik `billing-kasir` | `approved` | Yasmina (pemilik `billing-kasir`), disampaikan Yoga Aji Pratama 2026-10-21 | `MCU-EVD-002` bagian 4. Tidak lagi memblokir |
| `MCU-DEC-016` | Decision | **Acuan alur v2 adalah keputusan wawancara ini.** Menu MCU frontend v1 (`MCU-FACT-008`) hanya **referensi tampilan dan isian** bila tidak bertentangan dengan keputusan di sini. Alasan: v1 menumpang pemesanan Lab sehingga tidak dapat menampung *Pemeriksaan MCU* (`MCU-DEC-007`), resume dan verifikasi (`MCU-DEC-008`), maupun tagihan satu baris (`MCU-DEC-005`). **Contoh:** isian *Nama Perusahaan (Penjamin)* dan modal unggah surat jaminan v1 boleh ditiru; tetapi daftar v1 yang mengambil data dari pemesanan Lab tidak ditiru, karena Daftar Pasien MCU v2 membaca order paket MCU | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. **Berbeda** dari pola Radiologi yang ikut v1 persis |
| `MCU-DEC-017` | Decision | Mengesahkan `MCU-ASM-002`. **Pintu 13B** — rekomendasi dokter Rawat Jalan membuat order paket *Terjadwal*, tidak menumpang kunjungan poliklinik. **Pintu 13E** — pendaftaran MCU di kiosk tanpa booking adalah *walk-in* yang langsung *Hadir* dan langsung membentuk kunjungan MCU. Dengan `MCU-DEC-003` dan `MCU-DEC-006`, **kelima pintu berakhir pada satu bentuk order paket MCU**. **Contoh:** dr. Hadi di Poli Penyakit Dalam merekomendasikan Paket Jantung untuk Bu Wati → Daftar Pasien MCU memuat baris *Terjadwal*; kunjungan poli Bu Wati selesai tanpa paket di tagihannya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-018` | Decision | **Skrining awal wajib sebelum komponen *Pemeriksaan MCU*, tidak sebelum Lab/Radiologi.** Formulir skrining memakai milik rawat jalan (`MCU-DEC-001`). Komponen Lab dan Radiologi boleh dikerjakan begitu peserta lunas atau terjamin (`MCU-DEC-004`). Komponen *Pemeriksaan MCU* dan approve resume menunggu skrining selesai. **Contoh:** rombongan PT Contoh check-in pukul 07.00. Siti langsung diambil darahnya di Lab sambil menunggu giliran perawat; Pemeriksaan Fisik Siti oleh dr. Rina baru dapat dibuka sesudah perawat menyelesaikan skrining (tanda vital, berat, tinggi) pukul 07.40 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-004` |
| `MCU-COORD-004` | Open Question | Skrining rawat jalan harus dapat dipakai untuk kunjungan `MedicalCheckup`, dan MCU harus dapat membaca status *skrining selesai* | Pemilik `rawat-jalan` | `draft` | — | Memblokir `IMPLEMENTATION` kunci skrining; dipastikan lewat trace |
| `MCU-DEC-019` | Decision | **Pengiriman hasil oleh petugas lewat sistem.** Sesudah hasil terverifikasi, petugas MCU memilih saluran — WhatsApp, email, atau ambil langsung — lalu menekan **Kirim**, per peserta atau sekaligus untuk satu rombongan. Sistem mengirim berkas hasil beserta pemberitahuan *hasil sudah keluar* lewat `platform-notification`, lalu mencatat siapa mengirim, kapan, ke saluran dan alamat mana. Pemberitahuan lewat **telepon dicatat manual** oleh petugas. **Tidak ada pengiriman otomatis tanpa tindakan petugas.** Ringkasan untuk perusahaan mengikuti `MCU-DEC-010`. **Contoh:** Dewi memilih 25 peserta PT Contoh yang terverifikasi lalu menekan Kirim. Tiap peserta menerima hasil lengkap di WhatsApp masing-masing; HRD PT Contoh menerima satu berkas ringkasan kelayakan lewat email. Riwayat Budi mencatat *25 Okt 10.12 — WhatsApp 0812xxxx — oleh Dewi*, ditambah catatan manual *25 Okt 10.30 — telepon, diterima* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-005` |
| `MCU-COORD-005` | Open Question | Kesiapan `platform-notification` mengirim berkas lewat WhatsApp dan email beserta status terkirim/gagal. Sejalan dengan `LAB-COORD-017` yang juga masih terbuka | Pemilik platform | `draft` | — | Memblokir `IMPLEMENTATION` tombol Kirim; pencatatan *ambil langsung* dan telepon tidak terblokir |
| `MCU-DEC-020` | Decision | **Harga umum dan harga khusus per perusahaan.** Setiap paket punya **harga umum**. Admin boleh menambah **harga khusus untuk perusahaan tertentu** lengkap dengan tanggal mulai dan berakhir. Peserta perusahaan memakai harga khusus yang berlaku pada **tanggal check-in**; di luar masa itu, harga umum. Isi komponen paket tetap sama untuk semua pembayar. **Contoh:** Paket Basic berharga umum Rp 350.000; PT Contoh punya harga Rp 300.000 berlaku 1 Oktober–31 Desember 2026. Siti (PT Contoh) check-in 27 Oktober → Rp 300.000. Budi (perorangan) pada hari yang sama → Rp 350.000. Karyawan PT Contoh yang check-in 5 Januari 2027 → Rp 350.000 bila kontrak belum diperpanjang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Paket berisi komponen khusus per perusahaan **tidak** termasuk |
| `MCU-DEC-021` | Decision | **Pembatalan seluruh order sesudah bayar.** Selama **belum ada satu komponen pun yang dimulai** — sampel belum diambil, pasien belum difoto, pemeriksaan belum diisi — petugas MCU boleh membatalkan seluruh order dengan alasan wajib. Semua pesanan Lab/Radiologi turunannya ikut batal, dan Billing mengembalikan harga paket **utuh**. Begitu satu komponen sudah dimulai, yang berlaku hanya pembatalan per komponen oleh dokter (`MCU-DEC-015`). **Contoh:** Budi membayar Rp 350.000 pukul 07.10, lalu pukul 07.20 harus pulang karena urusan keluarga; belum ada sampel diambil. Dewi membatalkan order dengan alasan *permintaan peserta*, dan Budi menerima Rp 350.000 kembali. Bila darah Budi sudah diambil pukul 07.15, tombol batal penuh tidak tersedia | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Pengembaliannya bagian dari `MCU-COORD-003` |
| `MCU-DEC-022` | Decision | Closure pass. Menutup `MCU-Q-01` dan `MCU-CONFLICT-007`. **Pesanan Lab/Radiologi bertanda *bagian paket MCU* tidak menerbitkan tagihan sama sekali.** Pencegahannya di sumber: Lab tidak menerbitkan `LaboratoryCharge` saat spesimen diterima, dan Radiologi tidak menerbitkan `RadiologyCharge` saat akuisisi, untuk pesanan yang bertanda paket. Pemeriksaan tambahan di luar paket tetap ditagih seperti biasa. Rekap pemeriksaan MCU (butir 14C) dibaca dari data MCU, bukan dari baris tagihan. **Contoh:** Budi membayar Paket Basic Rp 350.000. Spesimen Darah Lengkap dan Gula Darah Puasa diterima Lab, Thorax diambil Radiologi — tagihan Budi tetap Rp 350.000 dan kunjungannya tetap *lunas*. Dokter lalu menambah Kolesterol Total di luar paket; saat spesimennya diterima, Lab menerbitkan Rp 75.000 seperti biasa | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Mempersempit `MCU-COORD-002`: kini jelas Lab dan Radiologi yang berubah, Billing tidak |
| `MCU-DEC-023` | Decision | Closure pass. Menutup `MCU-Q-02` dan `MCU-CONFLICT-006`. **Kiosk hanya menyerahkan data ke MCU; MCU yang membuat kunjungan `MedicalCheckup`.** Untuk peserta yang sudah booking, kiosk menyerahkan permintaan check-in; untuk *walk-in*, kiosk menyerahkan pendaftaran baru. Kedua jalur membuat kunjungan lewat proses yang **sama** dengan check-in di admisi, sehingga aturan pembuatan kunjungan MCU hanya ada di satu tempat. Kiosk tidak lagi mendaftarkan MCU sebagai kunjungan rawat jalan di Klinik Medical Check Up. **Contoh:** Siti datang tanpa booking dan memilih MCU di kiosk → kiosk menyerahkan identitas Siti dan Paket Karyawan ke MCU → MCU membuat order *Hadir* dan kunjungan `MedicalCheckup` → Siti muncul di Daftar Pasien MCU, bukan di daftar poliklinik | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-006` |
| `MCU-COORD-006` | Open Question | Perubahan di modul `kiosk`: pilihan MCU di kiosk diarahkan ke penyerahan data MCU, bukan pendaftaran rawat jalan Klinik Medical Check Up | Pemilik `kiosk` (Sukma Giri Pratama, `KSK-DEC-005`) | `draft` | — | Memblokir `IMPLEMENTATION` pintu 13E dan check-in kiosk |
| `MCU-DEC-024` | Decision | Closure pass. Menutup `MCU-Q-03`. **MCU membuat pesanan Lab/Radiologi dari paket hanya sesudah order berstatus *Dalam Pemeriksaan*** — yaitu sesudah peserta perorangan lunas atau peserta perusahaan terjamin (`MCU-DEC-004`). Dengan begitu butir 11 ditegakkan di MCU, dan **Radiologi tidak perlu diubah** untuk komponen paket. Pemeriksaan Radiologi **tambahan** di luar paket tetap mengikuti perilaku Radiologi hari ini (tanpa gerbang lunas) sampai pemilik Radiologi memutuskan lain. **Contoh:** Budi check-in pukul 07.05 → order *Hadir*, belum ada pesanan Lab/Rad. Pukul 07.10 Budi membayar → order *Dalam Pemeriksaan* dan saat itu pesanan Lab (2 pemeriksaan) serta pesanan Radiologi (Thorax) dibuat. Bila Budi tidak membayar, petugas Radiologi tidak akan pernah melihat pesanan Thorax-nya | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Celah pemeriksaan tambahan Radiologi dicatat sebagai `MCU-COORD-007` |
| `MCU-COORD-007` | Open Question | Radiologi tidak punya gerbang lunas (`MCU-CAP-18`), sehingga pemeriksaan Radiologi **tambahan** untuk peserta MCU perorangan dapat dikerjakan sebelum dibayar. Diteruskan sebagai informasi kepada pemilik Radiologi | Pemilik `radiologi` | `draft` | — | **Tidak** memblokir MCU |
| `MCU-DEC-025` | Decision | Closure pass. Menutup `MCU-Q-04`. **Harga khusus perusahaan disimpan sebagai baris master tarif tersendiri, dan MCU menyimpan pemetaannya.** Setiap harga khusus menjadi satu baris `MstTariff` bertanda paket. Master MCU mencatat **paket × perusahaan × masa berlaku → baris tarif mana**. Harga umum paket juga satu baris `MstTariff` bertanda paket. Billing tidak berubah: baris paket ditambahkan lewat jalur katalog tarif yang sudah ada, dan harga tetap bersumber tunggal dari master tarif. **Contoh:** master tarif memuat *Paket Basic* Rp 350.000 dan *Paket Basic – PT Contoh* Rp 300.000. Master MCU mencatat: *Paket Basic + PT Contoh, 1 Okt–31 Des 2026 → tarif Paket Basic – PT Contoh*. Siti (PT Contoh) check-in 27 Oktober → MCU menambahkan tarif Rp 300.000 ke tagihan; Budi (perorangan) → tarif Rp 350.000 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Melengkapi `MCU-DEC-020`; memakai `MCU-CAP-13` dan `MCU-CAP-25` |
| `MCU-DEC-026` | Decision | Closure pass. Menutup `MCU-Q-05`. **Kunjungan MCU tidak dikenai biaya administrasi terpisah; biaya itu sudah termasuk harga paket.** Harga paket adalah harga final yang dijanjikan kepada peserta dan perusahaan. Billing tidak perlu menambah `MCU` ke daftar jenis layanan biaya administrasi. **Contoh:** Paket Basic Rp 350.000 dibayar tepat Rp 350.000 — tidak ada baris *Biaya Administrasi* di tagihan Budi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menutup `MCU-CAP-26` |
| `MCU-DEC-027` | Decision | Closure pass. Menutup `MCU-Q-06` dan mengamandemen urutan rilis `MCU-DEC-019` (isi keputusannya tetap). **Rilis pertama MCU tidak menunggu pengiriman otomatis.** Pada rilis pertama hasil dicetak atau diunduh lalu diserahkan langsung, dan petugas mencatat manual setiap penyerahan — saluran (*ambil langsung*, WhatsApp, email, telepon), waktu, dan tujuan. Tombol **Kirim** lewat sistem diaktifkan sesudah `platform-notification` menyediakan pengiriman (`MCU-COORD-005`). Pola v1 — mengirim dari browser ke server di luar backend — **dilarang** (`MCU-CONFLICT-008`). **Contoh:** rilis pertama: Dewi mengunduh PDF hasil Budi, mengirimnya lewat WhatsApp resmi unit, lalu mencatat *25 Okt 10.25 — WhatsApp 0812xxxx — oleh Dewi*. Sesudah platform siap, catatan yang sama terbentuk otomatis saat Dewi menekan Kirim | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. `MCU-COORD-005` turun dari pemblokir rilis menjadi pemblokir fitur Kirim |
| `MCU-DEC-028` | Decision | Closure pass. Menutup `MCU-Q-07`. **Bukti berjalan pembuatan kunjungan `MedicalCheckup` dan skrining perawatnya (`MCU-UNK-01`, `MCU-UNK-05`) dikumpulkan saat task backend pertama**, lewat uji otomatis ditambah satu uji di basis data devYoga. Tidak ada penulisan basis data pada tahap blueprint. Kedua unknown tidak menahan desain | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menjadi syarat penerimaan task backend pertama |
| `MCU-DEC-029` | Decision | Closure pass. Menutup `MCU-OQ-004`. **Kuota harian boleh dilampaui hanya oleh kepala unit MCU, dengan alasan tercatat.** Komponen yang dijadwal ulang (`MCU-DEC-013`) **tidak memakan kuota** hari tujuannya, karena kuota mengukur kedatangan peserta baru, bukan komponen susulan. **Contoh:** Senin berkuota 30 sudah terisi 26. PT Contoh membawa rombongan 9 orang → admisi ditolak (sisa 4). Kepala unit menyetujui pelampauan dengan alasan *kontrak tahunan PT Contoh*, sehingga hari itu berisi 35 peserta. Gula Darah Puasa Budi yang dipindah ke Selasa tidak mengurangi kuota Selasa | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-030` | Decision | Closure pass. Menutup `MCU-OQ-006`. **Check-in WhatsApp hanya konfirmasi rencana datang, bukan check-in.** Rilis pertama: check-in hanya lewat kiosk dan admisi. Sesudah kanal WhatsApp tersedia, balasan WhatsApp peserta hanya menandai order *Terjadwal* sebagai *sudah konfirmasi datang*; kunjungan tetap dibuat saat peserta **hadir secara fisik** dan di-check-in di kiosk atau admisi (`MCU-DEC-003`, `MCU-DEC-023`). **Contoh:** H-1 Budi membalas *YA* pada pengingat WhatsApp → ordernya bertanda *konfirmasi datang*, tetap *Terjadwal*, belum ada kunjungan maupun tagihan. Bila Budi ternyata tidak datang, ordernya menjadi *Tidak Hadir* pada tutup hari tanpa ada kunjungan yang harus dibatalkan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Mempersempit butir 5 sumber |
| `MCU-DEC-031` | Decision | Closure pass. Mengesahkan `MCU-ASM-003`. **Sembilan label status order paket MCU:** *Terjadwal*, *Tidak Hadir*, *Hadir*, *Dalam Pemeriksaan*, *Menunggu Resume*, *Disetujui Dokter*, *Terverifikasi*, *Diserahkan*, *Dibatalkan*. Urutan dan aturan perpindahannya mengikuti bagian 7 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-032` | Decision | Putaran 3 (sesudah penilaian kelengkapan `MCU-RCG-001`). Menutup `DEC-MCU-001`. **Dokter MCU pemeriksa ditetapkan saat check-in.** Dokter itu menjadi (1) dokter penanggung jawab seluruh pesanan Lab/Radiologi dari paket peserta tersebut, (2) penerima nilai kritis dari Lab/Radiologi atas pesanan itu, dan (3) penyusun serta pemberi approve resume (`MCU-DEC-008`). Bila dokter itu tidak dapat dihubungi saat ada nilai kritis, pemberitahuan dieskalasikan ke **kepala unit MCU**. **Contoh:** Siti check-in pukul 07.00 dan admisi menetapkan dr. Rina. Pesanan Gula Darah Puasa dan Thorax Siti tercatat atas tanggung jawab dr. Rina. Pukul 09.00 GDP Siti 450 mg/dL → analis Lab menghubungi dr. Rina; dr. Rina sedang tidak dapat dihubungi → analis menghubungi kepala unit MCU, yang memanggil Siti sebelum pulang | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-008` dan `MCU-ASM-004` |
| `MCU-ASM-004` | Assumption (**disahkan** `MCU-DEC-040`) | Kepala unit MCU boleh **mengganti** dokter MCU yang ditetapkan pada satu peserta, dengan alasan tercatat; dokter pengganti mewarisi seluruh tanggung jawab `MCU-DEC-032` sejak penggantian. Contoh: dr. Rina pulang pukul 14.00 sebelum resume Siti disusun → kepala unit mengganti ke dr. Hadi | Yoga Aji Pratama | `superseded` | — | Turunan `MCU-DEC-032`; disahkan `MCU-DEC-040` |
| `MCU-COORD-008` | Coordination (**ditutup** putaran 5) | Laboratorium dan Radiologi mengisi dokter penanggung jawab (`InstructingDoctorId`) pesanan dari paket dengan dokter MCU yang ditetapkan, dan memakai dokter itu sebagai penerima nilai kritis. Pemberitahuan nilai kritis Lab sendiri masih menunggu kemampuan pemberitahuan platform (`LAB-COORD-017`) | Pemilik `laboratorium`, `radiologi` | `approved` | Pemilik `laboratorium` dan `radiologi`, disampaikan Yoga Aji Pratama 2026-10-21 | `MCU-EVD-002` bagian 1. `LAB-COORD-017` tetap terbuka dan bukan bagian persetujuan ini |
| `MCU-DEC-033` | Decision | Menutup `DEC-MCU-004`. **Peserta baru boleh dibooking dengan identitas sementara.** Peserta yang sudah menjadi pasien langsung ditautkan ke data pasiennya saat booking. Peserta yang belum punya No. RM disimpan sebagai identitas sementara — nama, **NIK wajib**, tanggal lahir, nomor HP — dan boleh dimasukkan sekaligus dari daftar (misalnya berkas Excel dari HRD perusahaan). Saat check-in, sistem mencocokkan NIK ke data pasien: bila sudah ada, order ditautkan ke pasien itu; bila belum, data pasien dibuat dari identitas yang sudah terisi, sehingga peserta tidak mengisi ulang (butir 2 sumber). Peserta yang tidak pernah datang **tidak pernah mendapat No. RM**. **Contoh:** PT Contoh mengunggah 40 nama; 10 cocok dengan pasien lama, 30 menjadi identitas sementara. Pada hari H 27 orang datang: Siti (NIK cocok dengan pasien yang ternyata terdaftar kemarin di poli) ditautkan ke pasien itu, 26 lainnya dibuatkan data pasien saat check-in. Tiga yang tidak datang tetap identitas sementara dan berstatus *Tidak Hadir* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Menimbulkan `MCU-COORD-009` |
| `MCU-COORD-009` | Open Question | Pembuatan dan pencocokan data pasien saat check-in memakai jalur `patient-management` yang ada (pencarian NIK, pembuatan pasien baru); MCU tidak membuat salinan master pasien | Pemilik `patient-management` | `draft` | — | Memblokir `IMPLEMENTATION` check-in peserta baru; dipastikan lewat impact scan |
| `MCU-DEC-034` | Decision | Menutup `DEC-MCU-002`. **Nilai jasa medis diatur per komponen di master paket.** Admin mengisi nilai jasa medis setiap komponen paket. Saat komponen selesai, MCU menyerahkan kepada Medical Fee: komponen apa, dikerjakan atau dibaca dokter siapa, dan nilainya. Medical Fee yang menghitung pembayarannya. **Contoh:** Paket Basic Rp 350.000 memuat nilai jasa Pemeriksaan Fisik Rp 40.000 dan Thorax Rp 30.000. dr. Rina memeriksa fisik Budi dan dr. Andi, Sp.Rad membaca Thorax-nya → Medical Fee menerima dua catatan: *Pemeriksaan Fisik — dr. Rina — Rp 40.000* dan *Thorax — dr. Andi — Rp 30.000* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Sisi Medical Fee menunggu `MCU-COORD-010` |
| `MCU-COORD-010` | Open Question | Medical Fee menerima catatan jasa medis per komponen paket MCU sebagai sumber perhitungan, sebagai pengganti baris tagihan per pemeriksaan yang ditahan (`MCU-DEC-022`). Batas jumlah nilai jasa terhadap harga paket ditetapkan bersama Keuangan | Pemilik `medical-fee` + Keuangan | `draft` | — | Memblokir `IMPLEMENTATION` S4b; tidak memblokir tagihan paket |
| `MCU-DEC-035` | Decision | Menutup `DEC-MCU-003`. **Rilis pertama tanpa lampiran berkas.** Hasil EKG, audiometri, dan spirometri dicatat sebagai isian angka dan kesimpulan dokter; lembar asli dari alat disimpan secara fisik seperti saat ini. Lampiran berkas diaktifkan sesudah penyimpanan berkas di platform tersedia. **Contoh:** dr. Rina mengisi *EKG: irama sinus, 78 x/menit, kesan normal* dan komponen EKG dinyatakan selesai; kertas rekaman EKG disimpan di map peserta | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-036` | Decision | Putaran 4 (sesudah arsitektur domain `MCU-HDA-001`). Menutup `ARC-OPEN-01`. **Pemeriksaan tambahan di luar paket menjadi komponen order bertanda *di luar paket*.** Komponen itu **tidak** bertanda *dicakup paket*, sehingga ditagih satuan seperti biasa oleh modul pelaksananya, tetapi **ikut dihitung kelengkapan**: resume tidak dapat di-approve sebelum hasilnya tuntas, dan hasilnya ikut dibahas dalam resume. **Contoh:** dr. Rina menambah Kolesterol Total untuk Budi → order Budi kini 5 komponen (4 paket + 1 di luar paket); tagihan bertambah Rp 75.000; Daftar Pasien MCU menampilkan *4 dari 5 selesai* sampai Kolesterol tervalidasi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-037` | Decision | Menutup `ARC-OPEN-02` dan `ARC-OPEN-03`. **Walk-in dan rekomendasi dokter sama-sama memakai kuota harian.** Peserta *walk-in* memakai kuota tanggal hari itu; bila penuh, dibutuhkan pelampauan oleh kepala unit MCU (`MCU-DEC-029`), sama seperti booking. Rekomendasi dokter RJ/RI/IGD **wajib menyertakan tanggal pemeriksaan** yang dipilih bersama pasien dan memakai kuota tanggal itu, sehingga aturan *Tidak Hadir* (`MCU-DEC-014`) berlaku. **Contoh:** Senin berkuota 30 sudah penuh; Siti datang tanpa booking → admisi meminta pelampauan kepala unit. dr. Hadi merekomendasikan MCU untuk Bu Wati pada 3 November → kuota 3 November berkurang satu; bila Bu Wati tidak datang, ordernya menjadi *Tidak Hadir* pada tutup hari 3 November | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-038` | Decision | Menutup `ARC-OPEN-05`. **Pembatalan booking sebelum hari pemeriksaan dilakukan admisi dengan alasan wajib.** Order *Terjadwal* menjadi *Dibatalkan* dan kuotanya dilepas. Karena belum ada kunjungan maupun tagihan, tidak ada refund. Bila peserta hanya ingin pindah tanggal, admisi menjadwal ulang order yang sama, bukan membatalkan. **Contoh:** Budi menelepon 25 Oktober membatalkan booking 27 Oktober → order *Dibatalkan*, alasan *peserta berhalangan*, kuota 27 Oktober kembali 29/30 | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-039` | Decision | Menutup `ARC-OPEN-06`. **Pencocokan peserta saat check-in memakai NIK ditambah tanggal lahir.** Bila keduanya sama, order ditautkan ke pasien itu. Bila NIK sama tetapi tanggal lahir berbeda, sistem **tidak** menautkan otomatis; petugas wajib memeriksa dan mengonfirmasi pasien yang benar sebelum check-in selesai, dan konfirmasi itu tercatat. **Contoh:** NIK Budi cocok dengan pasien lama, tetapi tanggal lahir di booking 12-03-1990 sedangkan di data pasien 21-03-1990 → check-in berhenti; petugas mengecek KTP Budi, memperbaiki salah ketik di booking, lalu mengonfirmasi tautan | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan. Kendali keselamatan identifikasi pasien |
| `MCU-DEC-040` | Decision | Mengesahkan `MCU-ASM-004`. **Kepala unit MCU boleh mengganti dokter MCU yang ditetapkan, dengan alasan tercatat.** Dokter pengganti mewarisi seluruh tanggung jawab `MCU-DEC-032` — penanggung jawab pesanan, penerima nilai kritis, penyusun dan pemberi approve resume — sejak penggantian. **Contoh:** dr. Rina pulang pukul 14.00 sebelum resume Siti disusun → kepala unit mengganti ke dr. Hadi dengan alasan *dokter selesai jaga*; pesanan Lab/Rad Siti yang masih berjalan kini atas tanggung jawab dr. Hadi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-20 | Pilihan yang direkomendasikan |
| `MCU-DEC-041` | Decision | Putaran 5. **Ke-18 aturan berlabel DES di `contracts/validation-matrix.md` disahkan apa adanya** — `VAL-MCU-01`, `02`, `07`, `08`, `09`, `12`, `14`, `15`, `17`, `18`, `20`, `22`, `24`, `30`, `37`, `42`, `43`, `46` — termasuk check-in hanya pada tanggal pemeriksaan (`VAL-MCU-24`), larangan order ganda per peserta per tanggal (`VAL-MCU-18`), dan tanggal pemeriksaan tidak boleh lampau (`VAL-MCU-15`). Kolom *Asal* aturan-aturan itu kini menunjuk keputusan ini. **Contoh:** Budi sudah *Terjadwal* 27 Oktober (`MCU-RSMMC-000123`); admisi mencoba membooking Budi lagi untuk 27 Oktober → ditolak `409` *"Peserta ini sudah terjadwal MCU pada 27-10-2026 (MCU-RSMMC-000123)."* | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-21 | Pilihan yang direkomendasikan |
| `MCU-DEC-042` | Decision | Putaran 5. Menutup `DES-MCU-001`. **Dokter MCU jaga harian untuk kiosk.** Admin MCU menetapkan satu dokter MCU jaga pada setiap tanggal di jadwal harian. Check-in atau *walk-in* lewat kiosk otomatis menetapkan dokter jaga tanggal itu sebagai dokter MCU pemeriksa (`MCU-DEC-032`), sehingga dokter tetap ditetapkan **saat check-in**. Bila tanggal itu belum punya dokter jaga, kiosk menolak dan mengarahkan peserta ke admisi. Check-in di admisi tetap memilih dokter secara eksplisit, dengan dokter jaga sebagai isian bawaan. Kepala unit tetap boleh mengganti dokter (`MCU-DEC-040`). **Contoh:** 27 Oktober dokter jaga MCU dr. Rina. Siti check-in di kiosk pukul 07.00 → order *Hadir* atas tanggung jawab dr. Rina. 28 Oktober belum ada dokter jaga → Budi yang check-in di kiosk diarahkan ke admisi | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-21 | Pilihan yang direkomendasikan. Epic kiosk (`EPIC MCU-08`) tetap di luar MVP; keputusan ini membuatnya dapat dijadwalkan |
| `MCU-DEC-043` | Decision | Putaran 5. Menutup `MCU-UNK-04`. **Refund komponen lewat perluasan Billing.** Pembuktian `MCU-EVD-002` 2.2 menunjukkan jalur refund yang ada mengabaikan nominal yang diminta, mengizinkan satu case aktif per invoice, dan dapat merefund satu baris lebih dari nilainya. Billing diperluas agar (1) menerima nominal sebagian ≤ sisa yang dapat direfund pada satu baris, (2) mencatat akumulasi refund per baris sehingga beberapa komponen pada baris paket yang sama dapat direfund berturut-turut, dan (3) menolak refund yang melampaui nilai baris. Perluasan ini milik Billing dan butuh persetujuan pemilik `billing-kasir`; `MVP-2` tertahan sampai tersedia. **Contoh:** baris Paket Basic Rp 350.000 lunas. Thorax dibatalkan → refund Rp 150.000, sisa dapat direfund Rp 200.000. Audiometri dibatalkan kemudian → refund Rp 50.000, sisa Rp 150.000. Pengajuan ketiga sebesar Rp 200.000 ditolak karena melebihi sisa | Yoga Aji Pratama | `approved` | Yoga Aji Pratama, 2026-10-21 | Pilihan yang direkomendasikan. Melanjutkan `MCU-COORD-003`; butir (3) sekaligus menutup cacat Billing yang berdiri sendiri |

## 6. Aktor dan tanggung jawab

| Aktor | Boleh melakukan | Tidak boleh |
| --- | --- | --- |
| Admin MCU | Mengelola master paket, komponen, harga umum dan harga per perusahaan, nilai pengembalian, persyaratan, hari buka, dan kuota | Mengisi hasil klinis |
| Petugas admisi MCU | Mencatat booking, mendaftarkan *walk-in*, check-in, menandai persyaratan, menjadwal ulang, membatalkan seluruh order sebelum komponen dimulai | Membatalkan komponen; mengubah resume |
| Perawat (skrining) | Mengisi skrining awal (formulir rawat jalan) | — |
| Dokter MCU | Mengisi *Pemeriksaan MCU*, menyusun dan menyetujui resume, membatalkan komponen, membuka ulang resume | Memverifikasi kelengkapan administratif hasilnya sendiri |
| Petugas MCU (verifikasi & penyerahan) | Memverifikasi kelengkapan administratif, mengirim hasil, mencatat telepon | Mengubah isi klinis |
| Dokter RJ/RI/IGD | Merekomendasikan paket MCU (titik sentuh di modulnya) | — |
| Peserta | Booking lewat telepon/WhatsApp, check-in lewat kiosk/WhatsApp/admisi, menerima hasil, memberi persetujuan pelepasan hasil | — |
| Perusahaan penjamin | Menerima ringkasan kelayakan; menerima hasil lengkap hanya dengan persetujuan peserta | Melihat nilai pemeriksaan/diagnosis tanpa persetujuan |

Nama izin dan pemetaan departemen/posisi ditetapkan saat desain mengikuti pola izin yang ada.

## 7. Status order paket MCU (`MCU-DEC-031`)

Label dan urutan di bawah sudah disahkan.

| Dari | Ke | Pemicu | Pelaku | Keputusan |
| --- | --- | --- | --- | --- |
| — | Terjadwal | Booking, rekomendasi dokter RJ/RI/IGD | Admisi / dokter pemberi rekomendasi | `MCU-DEC-003`, `006`, `017` |
| — | Hadir | Pendaftaran *walk-in* (admisi atau kiosk) | Admisi / peserta | `MCU-DEC-003`, `017` |
| Terjadwal | Hadir | Check-in fisik; kunjungan `MedicalCheckup` dibuat oleh MCU | Admisi / kiosk (WhatsApp hanya konfirmasi datang) | `MCU-DEC-003`, `023`, `030` |
| Terjadwal | Tidak Hadir | Tutup hari tanpa check-in fisik (konfirmasi WhatsApp tidak menghitung) | Sistem | `MCU-DEC-014`, `030` |
| Tidak Hadir | Terjadwal | Jadwal ulang ke tanggal baru | Admisi | `MCU-DEC-014` |
| Hadir | Dalam Pemeriksaan | Lunas (perorangan) atau terjamin (perusahaan); **saat ini pesanan Lab/Rad dari paket dibuat** | Billing / sistem | `MCU-DEC-004`, `024` |
| Hadir, Dalam Pemeriksaan | Dibatalkan | Batal penuh sebelum komponen dimulai; refund utuh | Petugas admisi | `MCU-DEC-021` |
| Dalam Pemeriksaan | Menunggu Resume | Semua komponen tuntas (selesai atau dibatalkan resmi) | Sistem | `MCU-DEC-009` |
| Menunggu Resume | Disetujui Dokter | Approve resume (skrining wajib selesai) | Dokter MCU | `MCU-DEC-008`, `018` |
| Disetujui Dokter | Menunggu Resume | Dikembalikan karena salah isi | Petugas MCU | `MCU-DEC-008` |
| Disetujui Dokter | Terverifikasi | Verifikasi kelengkapan administratif | Petugas MCU | `MCU-DEC-008` |
| Terverifikasi | Diserahkan | Kirim lewat WhatsApp/email atau ambil langsung | Petugas MCU | `MCU-DEC-019` |
| Disetujui Dokter, Terverifikasi, Diserahkan | Menunggu Resume (versi baru) | Buka ulang dengan alasan; versi lama bertanda *dikoreksi* | Dokter MCU | `MCU-DEC-011` |

**Contoh perjalanan lengkap:** Budi — Terjadwal (23 Okt, booking WhatsApp) → Hadir (27 Okt
07.05, check-in kiosk) → Dalam Pemeriksaan (07.10, lunas) → Menunggu Resume (28 Okt 09.00,
Thorax diekspertise) → Disetujui Dokter (28 Okt 10.00, dr. Rina) → Terverifikasi (28 Okt
10.20, Dewi) → Diserahkan (28 Okt 10.25, WhatsApp).

## 8. Acceptance criteria yang sudah dapat diuji

| ID | Kriteria | Keputusan |
| --- | --- | --- |
| `MCU-AC-001` | Booking membuat satu order *Terjadwal* tanpa kunjungan dan tanpa tagihan; check-in mengubah order **yang sama** menjadi *Hadir* dan membuat tepat satu kunjungan `MedicalCheckup` | `MCU-DEC-003` |
| `MCU-AC-002` | Rekomendasi dari RJ/RI/IGD tidak menambah baris apa pun ke tagihan kunjungan asalnya | `MCU-DEC-006`, `017` |
| `MCU-AC-003` | Booking ke hari yang kuotanya penuh ditolak dengan pesan yang menyebut sisa kuota | `MCU-DEC-012` |
| `MCU-AC-004` | Order *Terjadwal* tanpa check-in berubah *Tidak Hadir* sesudah tutup hari dan kuotanya kembali | `MCU-DEC-014` |
| `MCU-AC-005` | Check-in tidak dapat diselesaikan sebelum setiap persyaratan ditandai; komponen yang dijadwal ulang tidak ikut dipesan hari itu | `MCU-DEC-013` |
| `MCU-AC-006` | Tagihan paket berisi satu baris seharga paket; pesanan Lab/Rad turunan tidak menghasilkan baris tagihan sendiri; pemeriksaan tambahan ditagih satuan | `MCU-DEC-005` |
| `MCU-AC-007` | Peserta perusahaan dengan harga khusus berlaku pada tanggal check-in ditagih harga khusus; di luar masa berlaku ditagih harga umum | `MCU-DEC-020` |
| `MCU-AC-008` | Pesanan Lab/Rad peserta perorangan yang belum lunas terkunci; peserta perusahaan terjamin tidak terkunci | `MCU-DEC-004` |
| `MCU-AC-009` | Master menolak nilai pengembalian yang jumlahnya melebihi harga paket | `MCU-DEC-015` |
| `MCU-AC-010` | Approve resume ditolak selama ada komponen belum tuntas atau skrining belum selesai | `MCU-DEC-009`, `018` |
| `MCU-AC-011` | Petugas MCU tidak dapat mengubah isi klinis resume; hasil yang belum terverifikasi tidak dapat dikirim | `MCU-DEC-008` |
| `MCU-AC-012` | Berkas untuk perusahaan tanpa persetujuan peserta tidak memuat nilai pemeriksaan maupun diagnosis | `MCU-DEC-010` |
| `MCU-AC-013` | Buka ulang resume hanya oleh dokter MCU dengan alasan; versi lama tetap terbaca bertanda *dikoreksi*; penerima versi lama tercatat *perlu diberi tahu* | `MCU-DEC-011` |
| `MCU-AC-014` | Batal penuh tidak tersedia begitu satu komponen sudah dimulai | `MCU-DEC-021` |
| `MCU-AC-015` | Setiap pengiriman hasil tercatat: pengirim, waktu, saluran, alamat tujuan | `MCU-DEC-019` |
| `MCU-AC-016` | Pesanan Lab/Rad bertanda paket tidak menerbitkan fakta tagih saat spesimen diterima atau gambar diambil; pemeriksaan tambahan tetap menerbitkan | `MCU-DEC-022` |
| `MCU-AC-017` | Pendaftaran/check-in dari kiosk menghasilkan kunjungan `MedicalCheckup` lewat proses MCU, bukan kunjungan `Outpatient` | `MCU-DEC-023` |
| `MCU-AC-018` | Tidak ada pesanan Lab/Rad dari paket sebelum order berstatus *Dalam Pemeriksaan* | `MCU-DEC-024` |
| `MCU-AC-019` | Peserta perusahaan dengan pemetaan harga berlaku ditagih baris tarif khusus; tanpa pemetaan berlaku ditagih tarif umum | `MCU-DEC-025` |
| `MCU-AC-020` | Tagihan kunjungan MCU tidak memuat biaya administrasi | `MCU-DEC-026` |
| `MCU-AC-021` | Penyerahan hasil dapat dicatat manual (saluran, waktu, tujuan, petugas) tanpa tombol Kirim | `MCU-DEC-027` |
| `MCU-AC-022` | Pelampauan kuota hanya oleh kepala unit MCU dengan alasan; komponen yang dijadwal ulang tidak mengurangi kuota | `MCU-DEC-029` |
| `MCU-AC-023` | Konfirmasi WhatsApp tidak membuat kunjungan dan tidak mencegah status *Tidak Hadir* | `MCU-DEC-030` |
| `MCU-AC-024` | Check-in tidak dapat diselesaikan tanpa dokter MCU pemeriksa; pesanan Lab/Rad paket tercatat atas tanggung jawab dokter itu | `MCU-DEC-032` |
| `MCU-AC-025` | Approve resume hanya oleh dokter MCU yang ditetapkan (atau penggantinya) | `MCU-DEC-032`, `MCU-ASM-004` |
| `MCU-AC-026` | Booking peserta baru menyimpan identitas sementara dengan NIK wajib; tidak ada No. RM terbentuk sebelum check-in | `MCU-DEC-033` |
| `MCU-AC-027` | Check-in identitas sementara dengan NIK yang sudah dimiliki pasien menautkan ke pasien itu, bukan membuat pasien kedua | `MCU-DEC-033` |
| `MCU-AC-028` | Komponen selesai menghasilkan satu catatan jasa medis (komponen, dokter, nilai) untuk Medical Fee | `MCU-DEC-034` |
| `MCU-AC-029` | Komponen EKG/audiometri/spirometri dapat dinyatakan selesai tanpa lampiran berkas | `MCU-DEC-035` |
| `MCU-AC-030` | Check-in kiosk menetapkan dokter jaga tanggal itu sebagai dokter MCU; tanpa dokter jaga, kiosk menolak dan mengarahkan ke admisi | `MCU-DEC-042` |
| `MCU-AC-031` | Dua pembatalan komponen pada satu baris paket menghasilkan dua refund sebesar nilai pengembalian masing-masing; total refund satu baris tidak pernah melebihi nilai baris | `MCU-DEC-043` |

## 9. Kewenangan keputusan frontend

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MCU-FE-001` | Acuan alur layar | Yoga Aji Pratama | `approved` | Keputusan wawancara ini; v1 hanya referensi tampilan | `MCU-DEC-016` |
| `MCU-FE-002` | Menu, route, tab/modal, tata letak layar MCU | Yoga Aji Pratama | `approved` | Menurut [05-screen-brief.md](05-screen-brief.md): menu ringkas tanpa menu v1, detail order halaman bertab, formulir panjang di halaman tersendiri, jadwal sebagai tabel per tanggal. Rentang `DEV_DISCRETION` di brief bagian 7 | Yoga Aji Pratama, 2026-10-21 |
| `MCU-FE-003` | Privasi tampilan ringkasan perusahaan | Yoga Aji Pratama | `approved` | Tidak boleh menampilkan nilai/diagnosis tanpa persetujuan | `MCU-DEC-010` |

## 10. Open question, koordinasi, dan blocker

### 10.1 Masih terbuka

| ID | Butir | Owner | Memblokir |
| --- | --- | --- | --- |
| `MCU-OQ-002` | Daftar jenis *Pemeriksaan MCU* dan isian tiap jenis. Bukti yang dibutuhkan: contoh formulir/cetakan MCU yang dipakai sekarang | Yoga + dokter MCU | `IMPLEMENTATION` layar isi hasil |
| `MCU-OQ-003` | Kategori kelayakan; bentuk persetujuan pelepasan hasil kepada perusahaan | Yoga + dokter MCU; pemilik privasi/legal | `IMPLEMENTATION` resume dan ringkasan perusahaan |
| `MCU-OQ-005` | Definisi keempat laporan MCU (butir 14) | Yoga | `LATER SLICE` |
| `MCU-COORD-001` | Penjamin perusahaan: gerbang lunas Lab sudah meloloskan `CompanyGuarantor` (`MCU-CAP-17`); yang tersisa apakah status `NeedGuaranteeLetter` harus menahan (`MCU-UNK-02`) | Billing, Registrasi | `IMPLEMENTATION` jalur perusahaan |
| `MCU-COORD-004` | Skrining rawat jalan untuk kunjungan MCU — dibuktikan saat task backend pertama (`MCU-DEC-028`) | Rawat Jalan | `IMPLEMENTATION` kunci skrining |
| `MCU-COORD-005` | Pengiriman berkas WhatsApp/email lewat platform | Platform | Fitur **Kirim** saja (`MCU-DEC-027`); tidak memblokir rilis |
| `MCU-COORD-006` | Kiosk menyerahkan data MCU, tidak membuat kunjungan rawat jalan Klinik MCU | Kiosk | `IMPLEMENTATION` pintu 13E |
| `MCU-COORD-007` | Radiologi tanpa gerbang lunas untuk pemeriksaan tambahan | Radiologi | Tidak memblokir MCU |
| `MCU-COORD-009` | Pencocokan NIK dan pembuatan pasien saat check-in lewat `patient-management` | Patient Management | `IMPLEMENTATION` check-in peserta baru |
| `MCU-COORD-010` | Medical Fee menerima catatan jasa per komponen paket | Medical Fee + Keuangan | `IMPLEMENTATION` S4b |
| `DEC-MCU-005` | Definisi laporan MCU (sama dengan `MCU-OQ-005`) | Yoga | `DESIGN` slice S9 |

### 10.2 Ditutup pada closure pass

| ID | Ditutup oleh |
| --- | --- |
| `MCU-Q-01` / `MCU-CONFLICT-007` | `MCU-DEC-022` |
| `MCU-Q-02` / `MCU-CONFLICT-006` | `MCU-DEC-023` |
| `MCU-Q-03` | `MCU-DEC-024` |
| `MCU-Q-04` | `MCU-DEC-025` |
| `MCU-Q-05` | `MCU-DEC-026` |
| `MCU-Q-06` / `MCU-CONFLICT-008` | `MCU-DEC-027` |
| `MCU-Q-07` | `MCU-DEC-028` |
| `MCU-OQ-004` | `MCU-DEC-029` |
| `MCU-OQ-006` | `MCU-DEC-030` |
| `MCU-ASM-003` | `MCU-DEC-031` |
| `DEC-MCU-001` | `MCU-DEC-032` (putaran 3) |
| `DEC-MCU-002` | `MCU-DEC-034` (putaran 3) |
| `DEC-MCU-003` | `MCU-DEC-035` (putaran 3) |
| `DEC-MCU-004` | `MCU-DEC-033` (putaran 3) |
| `MCU-CONFLICT-005` | Tidak butuh keputusan baru: booking MCU tidak memakai perjanjian rawat jalan (`MCU-DEC-003`) |
| `MCU-GOV-001` | Putaran 5 — baris registry `Mcu` / `ACTIVE` disetujui Muhammad Hamzah dan ditambahkan 2026-10-21; checker terbukti tidak lagi melaporkan QBE-MOD-002 (`MCU-EVD-002` bagian 1) |
| `MCU-COORD-002`, `MCU-COORD-008` | Putaran 5 — disetujui pemilik `laboratorium` dan `radiologi` (`MCU-EVD-002` bagian 1) |
| Perluasan `EncounterIntakeService` | Putaran 5 — disetujui pemilik Registrasi (`MCU-EVD-002` bagian 1) |
| `ARC-TECH-01` | Putaran 5 — terbukti didukung jalur `LabExaminationService.CancelAsync` yang sudah ada (`MCU-EVD-002` 2.1) |
| `MCU-UNK-04` | Putaran 5 — terbukti **tidak** didukung; diputuskan `MCU-DEC-043` (perluasan Billing, berlanjut sebagai `MCU-COORD-003`) |
| `DES-MCU-001` | `MCU-DEC-042` (putaran 5) |
| Label **DES** pada `validation-matrix.md` | `MCU-DEC-041` (putaran 5) |
| `MCU-COORD-003` | Putaran 5 — perluasan refund (`MCU-DEC-043`) dan penutupan celah refund ganda disetujui Yasmina, pemilik `billing-kasir` (`MCU-EVD-002` bagian 4) |

Seluruh pertentangan `MCU-CONFLICT-001`..`008` sudah ditutup. **Koreksi 2026-10-20:** penilaian
kelengkapan `MCU-RCG-001` menemukan lima pertanyaan baru `DEC-MCU-001`..`005` yang **memblokir desain
domain** slice tertentu (lihat 10.1). Pernyataan sebelumnya bahwa tidak ada pemblokir desain tidak
lagi berlaku. Empat di antaranya (`001`..`004`) ditutup pada putaran 3; tinggal `DEC-MCU-005` (laporan).

## 11. Ringkasan pass

### Scope pass (r1)

- **Keputusan:** `MCU-DEC-001`..`MCU-DEC-021`, seluruhnya `approved` oleh Yoga Aji Pratama pada
  20 Oktober 2026, seluruhnya pilihan yang direkomendasikan.
- **Digantikan:** `MCU-ASM-002` → `MCU-DEC-017`.

### Closure pass (r2)

- **Masukan:** [01-existing-capability-map.md](01-existing-capability-map.md) r1.
- **Keputusan:** `MCU-DEC-022`..`MCU-DEC-031`, seluruhnya `approved` oleh Yoga Aji Pratama pada
  20 Oktober 2026, seluruhnya pilihan yang direkomendasikan.
- **Diamandemen:** urutan rilis `MCU-DEC-019` oleh `MCU-DEC-027` (isinya tetap).
- **Disahkan:** `MCU-ASM-003` → `MCU-DEC-031`.
- **Bersinggungan dengan modul lain:** Laboratorium dan Radiologi berhenti menerbitkan tagihan
  untuk pesanan bertanda paket (`MCU-DEC-022`); kiosk berhenti mendaftarkan MCU sebagai rawat jalan
  (`MCU-DEC-023`); `LAB-DEC-164` dipersempit — perlu dicatat di blueprint Laboratorium.
- **Di luar scope, sengaja ditinggalkan:** gerbang lunas Radiologi untuk pemeriksaan tambahan
  (`MCU-COORD-007`), paket berisi komponen khusus per perusahaan, harga kontrak perusahaan untuk
  layanan selain paket MCU.
- **Langkah berikutnya yang tepat:** `/design-business-module` untuk modul `mcu`. Syaratnya
  terpenuhi: pertentangan tertutup, unknown tersisa tidak menahan desain, dan approval tercatat.
  `MCU-OQ-002`/`003` boleh dijawab paralel karena hanya menahan implementasi layar hasil.

### Putaran 3 (r3)

- **Masukan:** [02-requirement-completeness-assessment.md](02-requirement-completeness-assessment.md) r1 (`MCU-RCG-001`).
- **Keputusan:** `MCU-DEC-032`..`MCU-DEC-035`, seluruhnya `approved` oleh Yoga Aji Pratama pada
  20 Oktober 2026, seluruhnya pilihan yang direkomendasikan. Menutup `DEC-MCU-001`..`004`.
- **Dibuka:** `MCU-ASM-004` (penggantian dokter MCU), `MCU-COORD-008`..`010`.
- **Masih terbuka dan memblokir desain:** hanya `DEC-MCU-005` (laporan, slice S9).
- **Langkah berikutnya yang tepat:** `/hospital-domain-architect` untuk slice S1–S8.

### Putaran 4 (r4)

- **Masukan:** [03-domain-architecture.md](03-domain-architecture.md) r1 (`MCU-HDA-001`), bagian M.
- **Keputusan:** `MCU-DEC-036`..`MCU-DEC-040`, seluruhnya `approved` oleh Yoga Aji Pratama pada 20 Oktober 2026, seluruhnya pilihan yang direkomendasikan. Menutup `ARC-OPEN-01`, `02`, `03`, `05`, `06`; mengesahkan `MCU-ASM-004`.
- **Tersisa:** `ARC-OPEN-04` (catatan impor rombongan — keputusan desain), `ARC-TECH-01` (teknis Lab), `DEC-MCU-005` (laporan).
- **Langkah berikutnya yang tepat:** `/design-business-module` untuk S1–S8.

### Putaran 5 (r5)

- **Masukan:** blueprint `MCU-BP-001` r1 (draft) bagian *Pertanyaan sebelum development lock*, dan
  [evidence/2026-10-21-persetujuan-dan-pembuktian.md](evidence/2026-10-21-persetujuan-dan-pembuktian.md) (`MCU-EVD-002`).
- **Persetujuan lintas modul yang disampaikan pemilik produk:** registry `Mcu` (Muhammad Hamzah), Lab/Rad
  (`MCU-COORD-002`, `008`), Registrasi (perluasan `EncounterIntakeService`).
- **Pembuktian:** `ARC-TECH-01` terbukti didukung; `MCU-UNK-04` terbukti tidak didukung, beserta satu cacat
  Billing yang berdiri sendiri (refund ganda atas satu baris).
- **Keputusan:** `MCU-DEC-041`..`MCU-DEC-043`, seluruhnya `approved` oleh Yoga Aji Pratama pada
  21 Oktober 2026, seluruhnya pilihan yang direkomendasikan. Blueprint dan kontrak `v1` disetujui.
- **Tersisa:** `MCU-OQ-002`/`003` (go-live), `DEC-MCU-005` (laporan), `ARC-OPEN-04`, serta
  koordinasi non-pemblokir MVP lainnya di 10.1. `MCU-FE-002` disetujui 2026-10-21 lewat brief layar.
- **Langkah berikutnya yang tepat:** `/plan-module-delivery` untuk `MVP-0`..`MVP-3`.
- **Tambahan sesudah roadmap (2026-10-21):** roadmap `MCU-RM-BE-001`/`MCU-RM-FE-001` r1 disetujui Yoga
  Aji Pratama; `MCU-COORD-003` disetujui Yasmina, pemilik `billing-kasir`, sehingga `BE-MCU-024` dan
  `BE-MCU-026` tidak lagi tertahan.
