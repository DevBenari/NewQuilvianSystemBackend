# MCU (Medical Check-Up) — Arsitektur Domain

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` |
| Architecture ID | `MCU-HDA-001` |
| Revision | `1` |
| Status | `draft` — belum disetujui pemilik |
| Scope | Slice `MCU-S1`..`MCU-S8`. **`MCU-S9` Laporan di luar scope** (tertahan `DEC-MCU-005`) |
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN` untuk S1–S8 — [02-requirement-completeness-assessment.md](02-requirement-completeness-assessment.md) revision 2 (`MCU-RCG-001`) |
| Keputusan yang dipakai | `MCU-DEC-001`..`MCU-DEC-035` — [00-interview-decisions.md](00-interview-decisions.md) revision 3 |
| Bukti keadaan saat ini | [01-existing-capability-map.md](01-existing-capability-map.md) revision 1 (`MCU-CAP-01`..`38`) |
| Source SHA | Backend `4d3e4f98`, frontend `7fb1e1d3e` — **sama** dengan capability map, sehingga tidak perlu impact scan |
| Baseline rujukan | `ID-LAB-CAP-011`, `ID-LAB-CAP-012`, `ID-LAB-CAP-013`, `ID-LAB-REG-002` (`REFERENCE_ONLY`) |
| Kesiapan arsitektur | **`DOMAIN_ARCHITECTURE_READY`** untuk S1–S8 (bagian N) |

> **Cara membaca dokumen ini.** Dokumen ini menjelaskan *konsep bisnis* MCU, siapa pemiliknya,
> bagaimana konsep itu berhubungan dengan modul lain, dan bagaimana statusnya berubah. Dokumen
> ini **bukan** rancangan tabel, endpoint, atau layar. Satu konsep di sini belum tentu menjadi
> satu tabel; keputusan itu dibuat pada tahap `/design-business-module`.

---

## A. Ringkasan untuk pembaca umum

MCU dibangun di sekitar **satu catatan utama: Order Paket MCU**. Satu order = satu peserta yang
menjalani satu paket pada satu tanggal. Seluruh perjalanan peserta — booking, check-in, bayar,
pemeriksaan, resume, verifikasi, penyerahan — adalah perubahan status pada order itu.

MCU **memiliki** tiga hal sendiri: **katalog paket**, **jadwal kuota harian**, dan **order paket**
beserta hasil pemeriksaan MCU dan resume di dalamnya. Semua hal lain **dipinjam** dari pemiliknya:
data pasien dari Patient Management, kunjungan dari Registrasi, tarif dari Master Tarif, tagihan
dan refund dari Billing, pemeriksaan Lab/Radiologi dari modulnya, dokter dari master tenaga kerja,
skrining dari Rawat Jalan, dan perhitungan jasa dokter dari Medical Fee.

**Contoh satu perjalanan:**

> 23 Okt: admisi mencatat booking Budi, Paket Basic, untuk 27 Okt → order *Terjadwal*, kuota 27 Okt
> berkurang satu. Budi belum punya No. RM, jadi order menyimpan identitas sementara (nama, NIK).
>
> 27 Okt 07.05: Budi check-in di kiosk → NIK dicocokkan, data pasien dibuat lewat Patient
> Management, kunjungan *Medical Check-Up* dibuat lewat Registrasi, dr. Rina ditetapkan sebagai
> dokter MCU, satu baris *Paket Basic Rp 350.000* masuk tagihan → order *Hadir*.
>
> 07.10: Budi membayar → order *Dalam Pemeriksaan*; MCU membuat pesanan Lab (DL, GDP) dan
> Radiologi (Thorax) bertanda *dicakup paket MCU* atas tanggung jawab dr. Rina. Lab/Rad tidak
> menagih lagi.
>
> 28 Okt: Thorax diekspertise, Pemeriksaan Fisik diisi dr. Rina → order *Menunggu Resume* → dr. Rina
> approve resume → Dewi memverifikasi → Dewi menyerahkan lewat WhatsApp dan mencatatnya → *Diserahkan*.
> Medical Fee menerima catatan jasa: Pemeriksaan Fisik dr. Rina, Thorax dr. Andi.

---

## B. Ubiquitous language

| Istilah | Arti tunggal dalam MCU | Bukan |
| --- | --- | --- |
| **Paket MCU** | Kumpulan komponen yang dijual dengan satu nama dan satu harga umum | Bukan baris tarif; baris tarif hanya *harga* paket |
| **Komponen paket** | Satu pemeriksaan di dalam paket, berjenis *Laboratorium*, *Radiologi*, atau *Pemeriksaan MCU* (`MCU-DEC-007`) | Bukan pesanan Lab/Rad |
| **Pemeriksaan MCU** | Komponen yang hasilnya diisi di MCU (fisik, tanda vital, visus, EKG, gigi, audiometri, spirometri) | Bukan pengkajian rawat jalan; bukan pemeriksaan Lab |
| **Order Paket MCU** | Satu peserta × satu paket × satu tanggal pemeriksaan. Lahir saat booking, rekomendasi dokter, atau pendaftaran *walk-in* | Bukan kunjungan; kunjungan baru lahir saat check-in |
| **Komponen order** | Salinan satu komponen paket di dalam order, beserta nasibnya (dipesan, selesai, dijadwal ulang, dibatalkan) | — |
| **Peserta** | Orang yang menjalani order. Bisa sudah pasien (punya No. RM) atau masih **identitas sementara** | Bukan master pasien |
| **Identitas sementara** | Nama, NIK, tanggal lahir, HP peserta yang belum menjadi pasien (`MCU-DEC-033`) | Bukan pasien bayangan; tidak punya No. RM |
| **Dokter MCU** | Dokter yang ditetapkan saat check-in sebagai penanggung jawab pesanan, penerima nilai kritis, dan penyusun resume (`MCU-DEC-032`) | Bukan dokter yang merekomendasikan MCU |
| **Resume MCU** | Kesimpulan akhir peserta: kondisi umum, temuan penting, kategori kelayakan, rekomendasi. Berversi | Bukan laporan rekap (butir 14) |
| **Ringkasan kelayakan** | Potongan resume untuk perusahaan: kategori kelayakan + rekomendasi kerja, tanpa nilai dan diagnosis (`MCU-DEC-010`) | Bukan resume lengkap |
| **Terjamin** | Peserta perusahaan dengan penjamin aktif; boleh diperiksa tanpa bayar di tempat (`MCU-DEC-004`) | Bukan *lunas* |
| **Tuntas** (komponen) | Selesai/tervalidasi di modulnya, terisi (Pemeriksaan MCU), **atau** dibatalkan resmi oleh dokter MCU (`MCU-DEC-009`) | Bukan sekadar *sudah dikerjakan* |
| **Dicakup paket** | Tanda pada pesanan Lab/Rad bahwa pemeriksaannya sudah dibayar lewat baris paket, sehingga tidak boleh menerbitkan tagihan (`MCU-DEC-022`) | — |
| **Penyerahan** | Satu kejadian hasil sampai ke penerima: saluran, waktu, alamat tujuan, petugas | Bukan *pengiriman otomatis*; rilis pertama dicatat manual (`MCU-DEC-027`) |
| **Kuota harian** | Batas jumlah **peserta baru** yang dijadwalkan pada satu tanggal (`MCU-DEC-012`, `029`) | Tidak menghitung komponen yang dijadwal ulang |

---

## C. Peta bounded context

| ID | Context | Tanggung jawab | Konsep yang dimiliki | Pemilik |
| --- | --- | --- | --- | --- |
| `BC-MCU-01` | **Katalog MCU** | Mendefinisikan apa yang dijual dan aturannya | Paket, komponen paket, persyaratan, nilai pengembalian, nilai jasa medis, pemetaan harga | MCU — **New** |
| `BC-MCU-02` | **Jadwal MCU** | Hari buka dan kuota harian peserta | Jadwal harian, pelampauan kuota | MCU — **New** |
| `BC-MCU-03` | **Layanan MCU** | Perjalanan satu peserta dari booking sampai hasil diserahkan | Order paket, komponen order, hasil Pemeriksaan MCU, resume berversi, persetujuan pelepasan, penyerahan, impor rombongan | MCU — **New** |

**Context milik modul lain yang dipakai MCU:**

| Context | Pemilik | Hubungan dengan MCU | Arah |
| --- | --- | --- | --- |
| Patient Management | `patient-management` | MCU mencari pasien dengan NIK dan meminta pasien baru dibuat saat check-in | MCU hilir (*customer*) |
| Registrasi | `registration-management` | MCU meminta kunjungan `MedicalCheckup` dibuat saat check-in; membaca status skrining | MCU hilir |
| Penjamin perusahaan | Administrator Master Data + Patient Management + Registrasi | MCU membaca perusahaan, masa kontrak, dan kepesertaan karyawan | MCU hilir (*conformist*) |
| Master Tarif & Prosedur | Master Data | Komponen menunjuk prosedur; harga paket menunjuk baris tarif | MCU hilir (*conformist*) |
| Laboratorium | `laboratorium` | MCU meminta pesanan dibuat (bertanda *dicakup paket*); Lab memberi status selesai | Kemitraan — **Lab berubah** (`MCU-COORD-002`, `008`) |
| Radiologi | `radiologi` | Sama dengan Laboratorium | Kemitraan — **Rad berubah** |
| Billing | `billing-kasir` | MCU menambah baris paket; Billing memberi status lunas; refund lewat Billing | MCU hilir |
| Medical Fee | `medical-fee` | MCU menyerahkan catatan jasa per komponen | MCU hulu — **Medical Fee berubah** (`MCU-COORD-010`) |
| Kiosk | `kiosk` | Kiosk menyerahkan check-in/pendaftaran ke MCU | MCU hulu — **Kiosk berubah** (`MCU-COORD-006`) |
| Tenaga kerja (dokter) | `human-resource` / Master Data | MCU menunjuk dokter | MCU hilir |
| Rekam Medis | `rekam-medis` | Kelak membaca resume MCU (`MCU-GAP-04`) | MCU hulu — kelak |
| Notifikasi platform | `platform-notification` | Kelak mengirim hasil (`MCU-COORD-005`) | MCU hulu — kelak |

---

## D. Katalog konsep domain

| ID | Konsep | Klasifikasi | Context | Pemilik | Identitas | Peran lifecycle | Invariant penting | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `CON-MCU-01` | Paket MCU | `AGGREGATE_ROOT` | BC-01 | MCU — New | Kode paket unik | Aktif / nonaktif | Minimal satu komponen; jumlah nilai pengembalian ≤ harga umum paket | `MCU-DEC-005`, `015` |
| `CON-MCU-02` | Komponen paket | `ENTITY` (dalam Paket) | BC-01 | MCU — New | Paket + urutan | — | Menunjuk tepat satu prosedur (Lab/Rad) **atau** satu jenis Pemeriksaan MCU; tidak boleh dua komponen menunjuk prosedur yang sama dalam satu paket | `MCU-DEC-007`, `MCU-CAP-15` |
| `CON-MCU-03` | Persyaratan pemeriksaan | `VALUE_OBJECT` (pada komponen) | BC-01 | MCU — New | — | — | Teks persyaratan dan wajib-dikonfirmasi | `MCU-DEC-013`, `MCU-GAP-01` |
| `CON-MCU-04` | Nilai pengembalian & nilai jasa medis per komponen | `VALUE_OBJECT` (pada komponen) | BC-01 | MCU — New | — | — | Nilai ≥ 0 | `MCU-DEC-015`, `034` |
| `CON-MCU-05` | Harga paket | `REFERENCE_DATA` | Master Tarif | Master Data — **Existing** | Baris `MstTariff` ber-`IsPackageTariff` | — | Harga umum = satu baris tarif paket | `MCU-DEC-025`, `MCU-CAP-13` |
| `CON-MCU-06` | Pemetaan harga perusahaan | `ENTITY` (dalam Paket) | BC-01 | MCU — New | Paket + perusahaan + masa berlaku | Berlaku / kedaluwarsa | Tidak ada dua pemetaan untuk paket + perusahaan yang sama dengan masa berlaku tumpang tindih | `MCU-DEC-020`, `025` |
| `CON-MCU-07` | Jenis Pemeriksaan MCU | `REFERENCE_DATA` | BC-01 | MCU — New | Kode jenis | Aktif / nonaktif | Isiannya dikonfigurasi (`MCU-OQ-002`) | `MCU-DEC-007` |
| `CON-MCU-08` | Kategori kelayakan | `REFERENCE_DATA` | BC-01 | MCU — New | Kode kategori | Aktif / nonaktif | Daftar diisi dokter MCU (`MCU-OQ-003`) | `MCU-DEC-010` |
| `CON-MCU-09` | Jadwal harian MCU | `AGGREGATE_ROOT` | BC-02 | MCU — New | Tanggal | Buka / tutup | Peserta baru terjadwal ≤ kuota, kecuali pelampauan oleh kepala unit | `MCU-DEC-012`, `029` |
| `CON-MCU-10` | Pelampauan kuota | `ENTITY` (dalam Jadwal) | BC-02 | MCU — New | — | — | Wajib alasan dan kepala unit MCU | `MCU-DEC-029` |
| `CON-MCU-11` | **Order Paket MCU** | `AGGREGATE_ROOT` | BC-03 | MCU — New | Nomor order MCU | Sembilan status (bagian G) | Lihat `AGG-MCU-03` | `MCU-DEC-003`, `031` |
| `CON-MCU-12` | Peserta order | `VALUE_OBJECT` (dalam Order) | BC-03 | MCU — New; menunjuk pasien **Existing** | — | Identitas sementara → tertaut pasien | Sejak tertaut, menunjuk tepat satu pasien; NIK wajib bila belum tertaut | `MCU-DEC-033` |
| `CON-MCU-13` | Komponen order | `ENTITY` (dalam Order) | BC-03 | MCU — New | Order + komponen | Menunggu → Dipesan/Diisi → Tuntas; atau Dijadwal ulang; atau Dibatalkan | Potret isi paket saat order dibuat (`MCU-GAP-05`); pembatalan hanya oleh dokter MCU dengan alasan | `MCU-DEC-009`, `015`, `013` |
| `CON-MCU-14` | Hasil Pemeriksaan MCU | `ENTITY` (pada komponen order berjenis Pemeriksaan MCU) | BC-03 | MCU — New | Komponen order | Kosong → Terisi → (dikoreksi) | Hanya dapat diisi sesudah skrining selesai; pengisi tercatat | `MCU-DEC-007`, `018`, `035` |
| `CON-MCU-15` | Konfirmasi persyaratan | `VALUE_OBJECT` (dalam Order) | BC-03 | MCU — New | — | Dicatat saat check-in | Setiap persyaratan komponen ditandai terpenuhi/tidak sebelum check-in selesai | `MCU-DEC-013` |
| `CON-MCU-16` | Resume MCU (berversi) | `ENTITY` (dalam Order) | BC-03 | MCU — New | Order + nomor versi | Draf → Disetujui → Terverifikasi → (Dikoreksi) | Approve hanya bila semua komponen tuntas; versi lama tidak pernah hilang | `MCU-DEC-008`, `009`, `011` |
| `CON-MCU-17` | Persetujuan pelepasan hasil lengkap | `ENTITY` (dalam Order) | BC-03 | MCU — New | — | Ada / dicabut | Tanpa persetujuan, perusahaan hanya menerima ringkasan kelayakan | `MCU-DEC-010`, `MCU-OQ-003` |
| `CON-MCU-18` | Penyerahan hasil | `ENTITY` (dalam Order) | BC-03 | MCU — New | — | Dicatat | Hanya untuk versi resume yang terverifikasi; mencatat versi mana yang diserahkan | `MCU-DEC-019`, `027` |
| `CON-MCU-19` | Impor booking rombongan | `ENTITY` | BC-03 | MCU — New | Nomor impor | Diunggah → Diproses | Mencatat pengunggah, perusahaan, jumlah baris, hasil per baris — **usulan** (`ARC-OPEN-04`) | `MCU-DEC-033` |
| `CON-MCU-20` | Kunjungan Medical Check-Up | `EXTERNAL_CONTRACT` | Registrasi | **Existing** | `RegPatientEncounter`, `EncounterType.MedicalCheckup` | Dibuat saat check-in | Satu order punya paling banyak satu kunjungan | `MCU-CAP-01`, `02` |
| `CON-MCU-21` | Pasien | `EXTERNAL_CONTRACT` | Patient Management | **Existing** | `MstPatient` | — | MCU tidak menyalin master pasien | `MCU-DEC-033`, `MCU-COORD-009` |
| `CON-MCU-22` | Penjamin perusahaan | `EXTERNAL_CONTRACT` | Administrator / Patient Mgmt / Registrasi | **Existing** | `MstCompanyGuarantor`, `MstPatientCompanyGuarantor`, `RegPatientEncounterGuarantor` | — | — | `MCU-CAP-08`..`10` |
| `CON-MCU-23` | Pesanan Lab/Rad dicakup paket | `EXTERNAL_CONTRACT` | Lab / Rad | **Extend** | `LabOrder`/`LabOrderedProcedure`, `RadOrder` + tanda *dicakup paket* | Mengikuti lifecycle Lab/Rad | Pesanan bertanda tidak menerbitkan fakta tagih; dokter penanggung jawab = dokter MCU | `MCU-DEC-022`, `032` |
| `CON-MCU-24` | Baris tagihan paket | `EXTERNAL_CONTRACT` | Billing | **Existing** | `BilInvoiceItem` dari tarif paket | Ditambah saat check-in | Tepat satu baris paket per order | `MCU-DEC-005`, `MCU-CAP-25` |
| `CON-MCU-25` | Catatan jasa medis komponen | `EXTERNAL_CONTRACT` | Medical Fee | **Extend** | Komponen order + dokter | Diterbitkan saat komponen tuntas karena selesai | Satu catatan per komponen selesai; tidak diterbitkan bila komponen dibatalkan | `MCU-DEC-034` |
| `CON-MCU-26` | Dokter | `EXTERNAL_CONTRACT` | Tenaga kerja | **Existing** | `MstDoctor` | — | — | `MCU-DEC-032` |

**Konsep yang sengaja tidak dibuat:**

| Konsep yang dipertimbangkan | Alasan ditolak |
| --- | --- |
| Booking MCU sebagai konsep terpisah dari order | Booking **adalah** order berstatus *Terjadwal* (`MCU-DEC-003`); memisahkannya membuat dua catatan untuk satu peserta |
| Pasien MCU / master peserta MCU | Duplikasi master pasien; identitas sementara cukup sebagai nilai di dalam order |
| Tarif paket milik MCU | Harga tetap bersumber tunggal di Master Tarif (`MCU-DEC-025`) |
| Memakai dokumen klinis pasien (`TrxPatientClinicalDocument`) sebagai resume | Satu izin `Update` untuk semua tindakan dan verifikasi-sendiri bertentangan dengan `MCU-DEC-008` (`MCU-CAP-30`) |
| Memakai perjanjian rawat jalan sebagai booking | Perjanjian rawat jalan membuat kunjungan saat booking (`MCU-CONFLICT-005`) |
| Satu konsep per status (mis. *booking*, *kunjungan MCU*, *hasil MCU*) | Status adalah perjalanan satu order, bukan konsep berbeda |

---

## E. Model aggregate

### `AGG-MCU-01` Paket MCU

| Bagian | Isi |
| --- | --- |
| Root | `CON-MCU-01` Paket MCU |
| Isi | Komponen paket (+ persyaratan, nilai pengembalian, nilai jasa), pemetaan harga perusahaan, rujukan baris tarif harga umum |
| Invariant | (1) minimal satu komponen; (2) Σ nilai pengembalian ≤ harga umum; (3) pemetaan harga perusahaan tidak tumpang tindih; (4) setiap baris tarif yang dirujuk bertanda paket |
| Tindakan | Buat, ubah isi, nonaktifkan paket; tambah/akhiri pemetaan harga perusahaan |
| Catatan | Perubahan paket **tidak** mengubah order yang sudah ada — order menyimpan potretnya (`MCU-GAP-05`) |

**Contoh invariant (2):** Paket Basic Rp 350.000; nilai pengembalian DL Rp 100.000 + GDP Rp 40.000 +
Thorax Rp 150.000 = Rp 290.000 → sah. Admin mengubah Thorax menjadi Rp 250.000 → total Rp 390.000 →
ditolak.

### `AGG-MCU-02` Jadwal harian MCU

| Bagian | Isi |
| --- | --- |
| Root | `CON-MCU-09` Jadwal harian (satu per tanggal) |
| Isi | Status buka/tutup, kuota, jumlah terpakai, pelampauan |
| Invariant | Terpakai ≤ kuota + pelampauan yang disetujui kepala unit |
| Tindakan | Atur hari buka dan kuota; pakai kuota (booking, *walk-in* — lihat `ARC-OPEN-02`); lepas kuota (*Tidak Hadir*, *Dibatalkan*); setujui pelampauan |
| Konkurensi | **Wajib dijaga.** Dua petugas admisi yang mengisi sisa kuota terakhir pada saat yang hampir bersamaan tidak boleh sama-sama berhasil |

### `AGG-MCU-03` Order Paket MCU

| Bagian | Isi |
| --- | --- |
| Root | `CON-MCU-11` Order Paket MCU |
| Isi | Peserta, pintu asal, tanggal pemeriksaan, potret paket dan harga, komponen order (+ hasil Pemeriksaan MCU), konfirmasi persyaratan, dokter MCU, rujukan kunjungan, resume berversi, persetujuan pelepasan, penyerahan, riwayat status |
| Invariant | (1) paling banyak satu kunjungan; (2) tidak ada pesanan Lab/Rad sebelum *Dalam Pemeriksaan* (`MCU-DEC-024`); (3) approve resume hanya bila semua komponen tuntas **dan** skrining selesai (`MCU-DEC-009`, `018`); (4) verifikasi oleh petugas yang **bukan** penyusun isi klinis, tanpa mengubah isi (`MCU-DEC-008`); (5) penyerahan hanya untuk versi terverifikasi; (6) batal penuh hanya selama belum ada komponen dimulai (`MCU-DEC-021`); (7) ringkasan perusahaan tanpa nilai dan diagnosis kecuali ada persetujuan (`MCU-DEC-010`) |
| Tindakan | Booking, rekomendasikan, daftar *walk-in*, check-in, tetapkan/ganti dokter MCU, mulai pemeriksaan, jadwal ulang komponen, isi Pemeriksaan MCU, batalkan komponen, susun & approve resume, kembalikan ke dokter, verifikasi, serahkan, buka ulang resume, batalkan order, tandai tidak hadir, jadwal ulang order |
| Konkurensi | Beberapa pelaku menulis order yang sama di hari yang sama (perawat, dokter, petugas). Perubahan status order wajib dijaga dari tulis-menimpa; isian per komponen boleh dijaga per komponen |

**Kenapa resume, hasil Pemeriksaan MCU, dan penyerahan berada di dalam order, bukan aggregate
sendiri:** ketiganya hanya bermakna untuk satu order, dan invariant terpentingnya — *approve hanya
bila komponen tuntas*, *serahkan hanya versi terverifikasi*, *buka ulang mengembalikan order ke
Menunggu Resume* — melintasi status order. Memisahkannya akan memecah satu batas konsistensi.

---

## F. Model relasi

| ID | Dari | Ke | Makna | Kardinalitas | Wajib? | Pemilik relasi | Hapus/arsip |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `REL-MCU-01` | Paket | Komponen paket | Paket berisi komponen | 1 : 1..n | Wajib | MCU | Komponen ikut paket; paket hanya dinonaktifkan |
| `REL-MCU-02` | Komponen paket | Prosedur (Master) | Komponen Lab/Rad menunjuk prosedur | n : 1 | Wajib untuk Lab/Rad | MCU | Prosedur tidak dihapus |
| `REL-MCU-03` | Komponen paket | Jenis Pemeriksaan MCU | Komponen Pemeriksaan MCU menunjuk jenisnya | n : 1 | Wajib untuk Pemeriksaan MCU | MCU | — |
| `REL-MCU-04` | Paket | Baris tarif (Master Tarif) | Harga umum paket | n : 1 | Wajib | MCU | — |
| `REL-MCU-05` | Pemetaan harga | Perusahaan penjamin + baris tarif | Harga khusus perusahaan | n : 1 masing-masing | Wajib | MCU | Diakhiri lewat masa berlaku |
| `REL-MCU-06` | Order | Paket | Order menjalani paket (dengan potret) | n : 1 | Wajib | MCU | Paket nonaktif tidak memengaruhi order lama |
| `REL-MCU-07` | Order | Pasien | Peserta tertaut | n : 0..1 | Opsional sampai check-in; wajib sejak *Hadir* | MCU | — |
| `REL-MCU-08` | Order | Kunjungan `MedicalCheckup` | Kunjungan peserta | 1 : 0..1 | Wajib sejak *Hadir* | MCU menyimpan rujukan; Registrasi pemilik kunjungan | Order batal → kunjungan dibatalkan lewat Registrasi |
| `REL-MCU-09` | Order | Perusahaan penjamin | Peserta perusahaan | n : 0..1 | Opsional | MCU | — |
| `REL-MCU-10` | Order | Jadwal harian | Tanggal pemeriksaan memakai kuota | n : 1 | Wajib | MCU | — |
| `REL-MCU-11` | Order | Dokter MCU | Penanggung jawab | n : 0..1 | Wajib sejak *Hadir* | MCU | Penggantian tercatat (`MCU-ASM-004`) |
| `REL-MCU-12` | Komponen order (Lab/Rad) | Pesanan Lab/Rad | Komponen dikerjakan lewat pesanan | n : 1 (beberapa komponen boleh berada pada satu pesanan Lab per disiplin) | Wajib sejak *Dalam Pemeriksaan* | **Dua sisi:** MCU memiliki tautan komponen → pesanan; Lab/Rad memiliki tanda *dicakup paket* pada pesanannya sendiri | Pembatalan komponen membatalkan pemeriksaan di Lab/Rad (`ARC-TECH-01`) |
| `REL-MCU-13` | Order | Baris tagihan paket | Tagihan paket | 1 : 1 | Wajib sejak *Hadir* | Billing memiliki baris; MCU menyimpan rujukan | Batal → refund lewat Billing |
| `REL-MCU-14` | Komponen order (selesai) | Catatan jasa medis | Jasa dokter pelaksana/pembaca | 1 : 0..1 | Wajib untuk komponen bernilai jasa > 0 yang selesai | MCU menerbitkan; Medical Fee memiliki perhitungannya | — |
| `REL-MCU-15` | Order | Order rekomendasi asal | Pintu rekomendasi dokter RJ/RI/IGD menunjuk kunjungan asalnya | n : 0..1 | Opsional | MCU | — |
| `REL-MCU-16` | Order | Impor rombongan | Order lahir dari unggahan daftar | n : 0..1 | Opsional | MCU | — |

**Kenapa tautan Lab/Rad dimiliki dua sisi (`REL-MCU-12`):** keputusan *jangan menagih* harus dapat
diambil Laboratorium saat spesimen diterima **tanpa** bertanya ke MCU, sehingga tanda itu milik
Lab/Rad. Sebaliknya, MCU perlu tahu pesanan mana milik komponen mana untuk menghitung kelengkapan.
Kedua fakta berbeda pemiliknya, dan desain wajib menyediakan pemeriksaan rekonsiliasi agar
keduanya tidak menyimpang.

---

## G. Model lifecycle/status

### G.1 Order Paket MCU (`MCU-DEC-031`)

| Dari | Ke | Pemicu | Pelaku | Prasyarat / invariant | Kejadian audit |
| --- | --- | --- | --- | --- | --- |
| — | **Terjadwal** | Booking / rekomendasi dokter | Admisi / dokter RJ/RI/IGD | Kuota tanggal tersedia atau pelampauan disetujui; peserta = pasien atau identitas sementara ber-NIK; tanggal wajib (`ARC-OPEN-03`) | Dibuat, pintu asal, pembuat |
| — | **Hadir** | Pendaftaran *walk-in* (admisi/kiosk) | Admisi / kiosk | Sama dengan check-in; memakai kuota hari itu (`MCU-DEC-037`) | Dibuat + check-in |
| Terjadwal | **Hadir** | Check-in fisik | Admisi / kiosk | Peserta tertaut ke pasien (cocokkan NIK atau buat pasien); kunjungan dibuat; dokter MCU ditetapkan; persyaratan ditandai; baris paket ditambahkan ke tagihan | Check-in, dokter MCU, hasil konfirmasi persyaratan |
| Terjadwal | **Tidak Hadir** | Tutup hari | Sistem | Belum check-in; konfirmasi WhatsApp tidak mengubah apa pun (`MCU-DEC-030`) | Otomatis, waktu |
| Tidak Hadir | **Terjadwal** | Jadwal ulang | Admisi | Kuota tanggal baru tersedia | Tanggal lama → baru |
| Terjadwal | **Dibatalkan** | Pembatalan sebelum datang | Admisi | Alasan wajib; kuota dilepas; tanpa refund (`MCU-DEC-038`) | Alasan, pelaku |
| Hadir | **Dalam Pemeriksaan** | Lunas (perorangan) atau terjamin (perusahaan) | Sistem, dari fakta Billing / penjaminan | Pesanan Lab/Rad dibuat **saat ini**, bertanda *dicakup paket*, atas nama dokter MCU | Waktu, dasar (lunas/terjamin), pesanan yang dibuat |
| Hadir, Dalam Pemeriksaan | **Dibatalkan** | Batal penuh | Petugas admisi | Belum ada komponen dimulai; alasan wajib; pesanan turunan dibatalkan; refund utuh lewat Billing; kunjungan dibatalkan lewat Registrasi | Alasan, refund yang diajukan |
| Dalam Pemeriksaan | **Menunggu Resume** | Komponen terakhir tuntas | Sistem | Semua komponen tuntas | — |
| Menunggu Resume | **Disetujui Dokter** | Approve resume | Dokter MCU yang ditetapkan | Skrining selesai; resume lengkap (kategori kelayakan terisi) | Versi, dokter, waktu |
| Disetujui Dokter | **Menunggu Resume** | Dikembalikan karena salah isi | Petugas MCU | Alasan wajib | Alasan |
| Disetujui Dokter | **Terverifikasi** | Verifikasi kelengkapan administratif | Petugas MCU | Bukan penyusun isi klinis; identitas, kelengkapan komponen, lampiran hasil Lab/Rad diperiksa | Petugas, waktu |
| Terverifikasi | **Diserahkan** | Penyerahan pertama dicatat | Petugas MCU | Versi terverifikasi; penerima perusahaan hanya ringkasan kecuali ada persetujuan | Saluran, alamat, versi, petugas |
| Disetujui Dokter, Terverifikasi, Diserahkan | **Menunggu Resume** (versi baru) | Buka ulang | Dokter MCU | Alasan wajib; versi lama bertanda *dikoreksi*; penerima versi lama ditandai *perlu diberi tahu* | Alasan, versi lama/baru |

**Status akhir:** *Diserahkan* (dapat dibuka ulang), *Dibatalkan*, *Tidak Hadir* (dapat dijadwal ulang).

**Perpindahan terlarang (contoh):** *Terjadwal → Dalam Pemeriksaan* (melewati check-in);
*Dalam Pemeriksaan → Disetujui Dokter* (melewati kelengkapan); *Disetujui Dokter → Diserahkan*
(melewati verifikasi); *Dibatalkan → apa pun*.

### G.2 Komponen order

| Dari | Ke | Pemicu | Pelaku |
| --- | --- | --- | --- |
| Menunggu | Dipesan (Lab/Rad) | Order masuk *Dalam Pemeriksaan* | Sistem |
| Menunggu | Siap diisi (Pemeriksaan MCU) | Order *Dalam Pemeriksaan* dan skrining selesai | Sistem |
| Menunggu | Dijadwal ulang | Persyaratan tidak terpenuhi saat check-in (`MCU-DEC-013`) | Admisi |
| Dijadwal ulang | Menunggu | Tanggal susulan tiba | Sistem / admisi |
| Dipesan | Tuntas — selesai | Pesanan Lab/Rad selesai di modulnya | Lab / Rad |
| Siap diisi | Tuntas — selesai | Hasil diisi | Dokter/petugas MCU |
| Menunggu, Dipesan, Siap diisi, Dijadwal ulang | Tuntas — dibatalkan | Pembatalan komponen; refund sebesar nilai pengembalian | Dokter MCU, alasan wajib |

Komponen yang **dijadwal ulang** tidak memakai kuota (`MCU-DEC-029`) dan tetap milik order yang sama;
order tidak dapat masuk *Menunggu Resume* sampai komponen susulan tuntas.

### G.3 Resume MCU (versi)

*Draf* → *Disetujui* → *Terverifikasi* → (*Dikoreksi* bila dibuka ulang). Satu order boleh punya
banyak versi; hanya satu versi yang *berlaku*. Versi *dikoreksi* tetap dapat dibaca.

---

## H. Tanggung jawab authorization

Peran dan pembagiannya mengikuti keputusan pemilik (decision log bagian 6). **Jabatan pemegang
peran tidak ditetapkan di sini** (`MCU-GAP-02`, `CONFIGURABLE_DEFAULT`).

| Tindakan | Peran | Keputusan |
| --- | --- | --- |
| Kelola paket, harga perusahaan, nilai pengembalian, nilai jasa, persyaratan, kuota | Admin MCU | `MCU-DEC-001` |
| Setujui pelampauan kuota | Kepala unit MCU | `MCU-DEC-029` |
| Booking, *walk-in*, check-in, jadwal ulang, batal penuh | Petugas admisi MCU (+ kiosk untuk check-in/*walk-in*) | `MCU-DEC-003`, `021`, `023` |
| Rekomendasikan paket | Dokter RJ/RI/IGD (titik sentuh di modulnya) | `MCU-DEC-006`, `017` |
| Isi Pemeriksaan MCU | Dokter/petugas MCU | `MCU-DEC-007` |
| Batalkan komponen, susun & approve resume, buka ulang | **Dokter MCU yang ditetapkan** | `MCU-DEC-008`, `011`, `015`, `032` |
| Ganti dokter MCU yang ditetapkan | Kepala unit MCU | `MCU-DEC-040` |
| Verifikasi, kembalikan ke dokter, serahkan, catat telepon | Petugas MCU | `MCU-DEC-008`, `019`, `027` |
| Lihat hasil lengkap | Peserta; petugas internal berwenang | `MCU-DEC-010` |
| Terima ringkasan kelayakan | Perusahaan penjamin | `MCU-DEC-010` |

**Pemisahan tugas yang wajib dijaga:** penyusun isi klinis (dokter) ≠ pemverifikasi (petugas).

---

## I. Model audit/histori

| Kejadian | Yang wajib tersimpan |
| --- | --- |
| Setiap perpindahan status order | Status lama/baru, pelaku, waktu, alasan bila ada |
| Check-in | Cara (admisi/kiosk), dokter MCU, hasil pencocokan NIK (tertaut ke pasien lama / pasien baru dibuat), konfirmasi persyaratan |
| Penetapan/penggantian dokter MCU | Dokter lama/baru, pelaku, alasan |
| Pembatalan komponen | Komponen, dokter, alasan, nilai pengembalian yang diajukan |
| Isian Pemeriksaan MCU | Pengisi, waktu; nilai sebelumnya bila diubah |
| Resume | Seluruh versi, penyusun, pemberi approve, pemverifikasi, alasan buka ulang |
| Penyerahan | Versi, saluran, alamat tujuan, penerima (peserta/perusahaan), jenis isi (lengkap/ringkasan), petugas |
| Persetujuan pelepasan | Pemberi, waktu, bukti, pencabutan |
| Pelampauan kuota | Tanggal, jumlah, kepala unit, alasan |
| Impor rombongan | Pengunggah, perusahaan, jumlah baris, baris gagal beserta sebabnya |

**Privasi log.** NIK, nomor HP, nilai pemeriksaan, diagnosis, dan isi resume **tidak boleh** masuk
catatan log teks bebas; log cukup memuat nomor order dan jenis kejadian.

---

## J. Model integrasi

| ID | Produsen → konsumen | Tujuan | Sumber kebenaran | Waktu | Idempotency / kegagalan |
| --- | --- | --- | --- | --- | --- |
| `INT-MCU-01` | MCU → Patient Management | Cari pasien per NIK; buat pasien baru saat check-in | Patient Management | Sinkron, saat check-in | Pencocokan NIK sebelum membuat; bila gagal, check-in tidak selesai dan order tetap *Terjadwal* |
| `INT-MCU-02` | MCU → Registrasi | Buat kunjungan `MedicalCheckup` | Registrasi | Sinkron, saat check-in | Kunci per order: check-in ulang tidak membuat kunjungan kedua |
| `INT-MCU-03` | MCU → Billing | Tambah satu baris paket (tarif umum atau tarif perusahaan) | Billing | Sinkron, saat check-in | Kunci per order (`CorrelationId`); baris kedua tidak boleh terbentuk |
| `INT-MCU-04` | Billing → MCU | Status lunas kunjungan | Billing | Dibaca saat dibutuhkan | Aturan lunas yang sama dengan Lab (`MCU-CAP-17`) |
| `INT-MCU-05` | MCU → Lab / Rad | Buat pesanan bertanda *dicakup paket*, dokter penanggung jawab = dokter MCU | Lab / Rad | Sinkron, saat *Dalam Pemeriksaan* | Kunci per komponen; kegagalan sebagian harus terlihat dan dapat diulang tanpa pesanan ganda |
| `INT-MCU-06` | Lab / Rad → MCU | Status selesai/batal pesanan | Lab / Rad | Dibaca saat dibutuhkan atau diberitahukan | Rekonsiliasi berkala antara tautan MCU dan tanda Lab/Rad |
| `INT-MCU-07` | MCU → Lab / Rad | Batalkan pemeriksaan komponen / seluruh pesanan | Lab / Rad | Sinkron | Lihat `ARC-TECH-01` |
| `INT-MCU-08` | MCU → Billing | Refund nominal sebagian (komponen) atau utuh (batal penuh) | Billing | Sinkron (pengajuan); persetujuan di Billing | Satu pengajuan per pembatalan; `MCU-UNK-04` |
| `INT-MCU-09` | MCU → Medical Fee | Catatan jasa per komponen selesai | Medical Fee | Asinkron diperbolehkan | Satu catatan per komponen; dapat diulang tanpa ganda |
| `INT-MCU-10` | Kiosk → MCU | Check-in peserta booking / pendaftaran *walk-in* | MCU | Sinkron | Sama dengan check-in admisi |
| `INT-MCU-11` | Registrasi/Rawat Jalan → MCU | Status skrining selesai | Registrasi (antrean perawat) | Dibaca saat dibutuhkan | `MCU-UNK-05` dibuktikan pada task pertama (`MCU-DEC-028`) |
| `INT-MCU-12` | RJ/RI/IGD → MCU | Rekomendasi paket menjadi order *Terjadwal* | MCU | Sinkron | Kunci per rekomendasi |
| `INT-MCU-13` | MCU → Platform notifikasi | Kirim hasil | Platform | Kelak (`MCU-COORD-005`) | Di luar rilis pertama |

---

## K. Dampak billing

**Klasifikasi: berdampak pada charge.**

| Kejadian MCU | Dampak | Keputusan |
| --- | --- | --- |
| Check-in | Satu baris paket ditambahkan (harga umum atau harga perusahaan yang berlaku pada tanggal check-in); kena PPN; tanpa biaya administrasi | `MCU-DEC-005`, `020`, `025`, `026`, `BKC-DEC-083` |
| Pesanan Lab/Rad dari paket | **Tidak** menerbitkan tagihan | `MCU-DEC-022` |
| Pemeriksaan tambahan di luar paket | Ditagih seperti biasa oleh modul pelaksana | `MCU-DEC-005`, `ARC-OPEN-01` |
| Pembatalan komponen | Refund sebesar nilai pengembalian komponen | `MCU-DEC-015` |
| Batal penuh | Refund utuh; pesanan turunan dibatalkan | `MCU-DEC-021` |
| Peserta perusahaan | Tagihan kolektif ke perusahaan lewat jalur Billing yang ada | `MCU-DEC-004`, `MCU-CAP-28` |
| Komponen selesai | Catatan jasa medis ke Medical Fee | `MCU-DEC-034` |

**Dependency billing yang belum tuntas (tidak memblokir arsitektur):** refund nominal sebagian dari
satu baris (`MCU-UNK-04`, `MCU-COORD-003`); sisi penerima Medical Fee (`MCU-COORD-010`).

---

## L. Dampak keselamatan klinis

**Klasifikasi: relevan terhadap keselamatan.**

| Titik | Risiko | Kendali yang ditetapkan |
| --- | --- | --- |
| Pencocokan NIK saat check-in | Order tertaut ke pasien yang salah → hasil masuk rekam pasien lain | NIK + tanggal lahir; bila tanggal lahir berbeda, tidak ditautkan otomatis dan petugas wajib mengonfirmasi (`MCU-DEC-039`) |
| Nilai kritis sebelum resume | Peserta pulang tanpa tahu hasil berbahaya | Dokter MCU penerima, eskalasi kepala unit (`MCU-DEC-032`); mekanisme pemberitahuan milik Lab (`MCU-COORD-008`, `LAB-COORD-017`) |
| Resume disetujui tanpa data | Kesimpulan tanpa dasar | Kunci approve (`MCU-DEC-009`, `018`) |
| Koreksi sesudah diserahkan | Penerima memegang versi salah | Versi berlapis, alasan wajib, daftar *perlu diberi tahu* (`MCU-DEC-011`; `ID-LAB-CAP-013`) |
| Komponen dibatalkan karena kondisi (mis. hamil) | Paparan radiasi | Pembatalan oleh dokter; pemeriksaan keselamatan Radiologi tetap berlaku (`MCU-GAP-06`) |
| Privasi hasil ke perusahaan | Diagnosis bocor ke pemberi kerja | Ringkasan tanpa nilai/diagnosis, persetujuan tercatat (`MCU-DEC-010`) |

---

## M. Gap arsitektur

Tidak ada gap yang memblokir. **Pembaruan 2026-10-20 (putaran 4 decision log):** `ARC-OPEN-01`, `02`, `03`, `05`, `06` dan `MCU-ASM-004` sudah diputuskan pemilik sesuai usulan di bawah — `MCU-DEC-036`..`040`. Yang tersisa: `ARC-OPEN-04` (keputusan desain) dan `ARC-TECH-01` (teknis).

| ID | Gap | Jenis | Usulan arsitektur | Pemilik |
| --- | --- | --- | --- | --- |
| `ARC-OPEN-01` | Pemeriksaan tambahan di luar paket: apakah menjadi komponen order (ikut kunci approve) atau pesanan Lab/Rad biasa di luar order | Bisnis → `/grill-me` | Menjadi komponen order bertanda *di luar paket* (tidak bertanda *dicakup paket*, ditagih biasa), agar resume tidak di-approve sebelum hasilnya keluar | Yoga |
| `ARC-OPEN-02` | Apakah *walk-in* memakai kuota dan ditolak bila penuh | Bisnis → `/grill-me` | *Walk-in* memakai kuota; bila penuh perlu pelampauan kepala unit, sama dengan booking | Yoga |
| `ARC-OPEN-03` | Rekomendasi dokter RJ/RI/IGD wajib menyertakan tanggal pemeriksaan (karena *Terjadwal* butuh tanggal untuk *Tidak Hadir*) | Bisnis → `/grill-me` | Wajib tanggal, dipilih dokter bersama pasien, dan memakai kuota | Yoga |
| `ARC-OPEN-04` | Perlu tidaknya catatan impor rombongan tersendiri untuk jejak unggahan | Desain | Usulan `CON-MCU-19`; boleh dilebur ke riwayat order bila pemilik tidak membutuhkan jejak unggahan | Yoga |
| `ARC-OPEN-05` | Pembatalan order *Terjadwal* sebelum datang (peserta mengabarkan batal) | Bisnis → `/grill-me` | Admisi membatalkan dengan alasan; kuota dilepas; tanpa dampak tagihan | Yoga |
| `ARC-OPEN-06` | Identitas kedua saat pencocokan NIK | Keselamatan → `/grill-me` | NIK + tanggal lahir; beda → konfirmasi petugas | Yoga (+ pemilik `patient-management`) |
| `ARC-TECH-01` | Laboratorium mengelompokkan pemeriksaan per disiplin dalam satu pesanan; pembatalan **satu** komponen di pesanan yang berisi beberapa komponen perlu dibuktikan didukung Lab | Teknis → impact scan saat desain | Pembatalan pada tingkat pemeriksaan yang diminta (`LabOrderedProcedure`), bukan seluruh pesanan. **Terjawab 2026-10-21:** didukung jalur yang ada, `LabExaminationService.CancelAsync` (`MCU-EVD-002` 2.1) | Pemilik `laboratorium` |
| `MCU-ASM-004` | Penggantian dokter MCU oleh kepala unit | Asumsi → konfirmasi | Seperti tertulis di decision log | Yoga |

---

## N. Kesiapan arsitektur

**`DOMAIN_ARCHITECTURE_READY`** untuk scope `MCU-S1`..`MCU-S8`.

| Syarat | Terpenuhi | Dasar |
| --- | --- | --- |
| Slice layak domain design | Ya | `MCU-RCG-001` r2 |
| Bounded context dapat dipertahankan | Ya | Tiga context MCU; seluruh master bersama dipinjam |
| Ownership material terselesaikan | Ya | Bagian C, D, F; tautan Lab/Rad dua sisi dijelaskan |
| Lifecycle dan invariant terwakili | Ya | Bagian E, G |
| Keputusan pemblokir terselesaikan | Ya | `DEC-MCU-001`..`004` ditutup; `DEC-MCU-005` hanya S9 |
| Billing dan keselamatan klinis eksplisit | Ya | Bagian K, L |

`MCU-S9` Laporan **tidak** termasuk dan tetap `BUSINESS_DECISION_REQUIRED` (`DEC-MCU-005`). Laporan
hanya membaca order, sehingga ketidakhadirannya tidak mengubah arsitektur di atas — asalkan order
menyimpan waktu setiap perpindahan status (sudah tercakup bagian I).

## O. Handoff ke `/design-business-module`

| Butir | Nilai |
| --- | --- |
| Scope | `MCU-S1`..`MCU-S8` |
| Revision | `MCU-HDA-001` r1, `draft` |
| Kesiapan | `DOMAIN_ARCHITECTURE_READY` |
| Masukan | Decision log r3, capability map r1, penilaian kelengkapan r2 |
| Wajib dibawa | `ARC-OPEN-01`..`06`, `ARC-TECH-01`, `MCU-ASM-004`, `MCU-OQ-002`, `MCU-OQ-003`, `MCU-GAP-01`..`07`, `MCU-COORD-001`..`010`, `MCU-GOV-001` |
| Yang tidak boleh diubah diam-diam di blueprint | Order sebagai satu aggregate; booking = order *Terjadwal*; harga di Master Tarif; tanda *dicakup paket* milik Lab/Rad; tidak ada salinan master pasien |
| Source SHA | Backend `4d3e4f98`, frontend `7fb1e1d3e` |
