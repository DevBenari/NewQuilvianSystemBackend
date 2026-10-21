# Keputusan Paritas Radiologi V2 dengan v1 Tayang — `RAD-EVD-001`

| Field | Nilai |
| --- | --- |
| ID bukti | `RAD-EVD-001` |
| Tanggal | 2026-10-14 |
| Pemilik keputusan | Yoga Aji Pratama, pemilik modul Radiologi |
| Sumber | (1) v1 tayang `https://v1.quilvian-mmchospital.com` — ditelusuri baca-saja dengan akun yang diberikan pemilik modul; seluruh permintaan non-GET diblokir. (2) Source FE v1 `C:\Users\Admin\QuilvianSystemFrontendDev` commit `7210f5f4d` (18 Agustus 2026) |
| Pola rujukan | Paritas Lab `LAB-EVD-013` (2026-10-08) |
| Status | `approved` — keputusan diberikan lewat pertanyaan terstruktur pada sesi 2026-10-14 |

---

## 1. Keputusan pemilik modul

| No | Pertanyaan | Keputusan |
| ---: | --- | --- |
| `RAD-DEC-018` | Arah paritas | **Layar ikut v1, pengaman V2 tetap.** Menu, layar daftar, kolom, tab, dan aksi mengikuti v1. Gerbang keselamatan (`RAD-DEC-005`) dan pengesahan dokter radiolog (`RAD-DEC-003`) tetap berjalan di dalam alur itu |
| `RAD-DEC-019` | Gerbang keselamatan selama 12 aturan masih Draft | **Tetap fail-closed.** Alur v1 di V2 baru dapat dipakai penuh setelah aturan disahkan (`DEC-RAD-005`) |
| `RAD-DEC-020` | Cakupan fitur v1 | **Seluruhnya**: Beranda + Daftar Pasien + Riwayat; status pembayaran dan kunci Lunas; kirim hasil via WhatsApp; unggah gambar, jumlah film, PACS, dan AI |

`RAD-DEC-020` butir PACS **mengubah** `RAD-DEC-001` (PACS/DICOM di luar scope). Bentuk integrasinya belum ditetapkan — lihat bagian 5.

---

## 2. Yang hidup di v1

Hanya tiga layar Radiologi v1 yang hidup. Sembilan butir menu lainnya — Laporan (Statistik Pemeriksaan, Dokter
Pengirim, Pasien Perusahaan), Pemeriksaan (Harian, Per Item), Setup (Kategori, Parameter, Tarif Tindakan, Template
Hasil) — dibuka di v1 tayang dan **seluruhnya menampilkan "Coming Soon"**. Tidak ada logika yang perlu diport dari
sana. *Pengajuan Rekrutmen* bukan fitur Radiologi.

### 2.1 Beranda Radiologi

Kartu ringkasan (Total Booking, Rawat Jalan, Rawat Inap, Pemeriksaan Lunas), status yang perlu dipantau (Menunggu,
Diproses, Selesai, CITO), tren kunjungan 30 hari, komposisi pasien Rajal/Ranap/Rujukan (pie), lima booking terbaru.

### 2.2 Daftar Pasien Radiologi

| Tab | Isi (dari source v1) |
| --- | --- |
| Rawat Jalan | Jenis kunjungan `OP`, tanggal pemeriksaan **hari ini**, bukan batal |
| Rawat Inap | Jenis kunjungan `IP`, tanggal pemeriksaan hari ini, bukan batal |
| Pasien Luar | **Data palsu** (`makeRows`, "Pasien Luar 1…") — bukan alur hidup |
| Pasien Persiapan | Booking bertanda `isPasienPersiapan` |
| Pasien Perjanjian | Tanggal pemeriksaan **bukan** hari ini; lencana jumlah belum terkonfirmasi |

Penyaring: cari pasien/No. RM/dokter, periode, tanggal mulai–akhir, cito. Tombol **Tambah** (booking).

Kolom: **Registrasi** · **Pasien** (nama, No. RM, jenis kelamin) · **Order** (tanggal, jam, nomor) · **Pemeriksaan**
(bertumpuk) · **No. Foto** (bertumpuk) · **Diagnosa** · **Pemesan** · **Konfirmasi** (nama + waktu, atau *Belum
terkonfirmasi*) · **Asuransi / Penjamin** · **Pembayaran** (Lunas / Belum Lunas).

**Variasi kolom per tab (dibaca dari v1 tayang, 2026-10-14):** tab *Pasien Persiapan* menambah **Tipe Kunjungan** sesudah Registrasi; tab *Pasien Perjanjian* memakai Registrasi · Tipe Kunjungan · Pasien · Order · **Tgl Pemeriksaan** · Pemeriksaan · No. Foto · Diagnosa · Konfirmasi · Asuransi / Penjamin · Pembayaran (tanpa Pemesan).

Aksi baris: **Konfirmasi** (membuka Detail Pasien Radiologi; *Sudah Dikonfirmasi* bila sudah) · **Batalkan Order** ·
**Kirim Hasil** (WhatsApp) · **Print Hasil** · **Rincian Pasien** · **Print Label**.

### 2.3 Detail Pasien Radiologi (dari aksi Konfirmasi)

*Informasi Pasien*: No. Registrasi, No. RM, Nama, Jenis Kelamin, Umur, Kelas, Jenis Kunjungan, Asuransi/Penjamin,
Diagnosa. *Informasi Order*: No. Order, Tanggal Pemeriksaan, Dokter Perujuk, Konfirmator/Tgl Konfirmasi, Status
Pembayaran, Status Pemeriksaan/Tgl Selesai, **Petugas Radiologi** (pilihan). *Tabel pemeriksaan*: Cito, Pemeriksaan,
No. Photo, Foto Radiologi (PACS), **Dokter Pemeriksa** (pilihan), Hasil, Cetak Film, Aksi.

Form hasil: Informasi Pemesanan (No Order, Dokter Perujuk, Dokter Pemeriksa, Tanggal Selesai), Informasi Tindakan
(Kelas, daftar tindakan + harga), Petugas Radiologi, **Unggah Gambar**, **Jumlah Film** (ukuran Kecil/Sedang/Besar,
Hitam/Berwarna), **Gunakan Hasil AI**, Hasil Pemeriksaan, Catatan Tambahan (kesimpulan).

### 2.4 Riwayat Pemeriksaan

Tab sama dengan Daftar Pasien, tanpa batas "hari ini", berhalaman 10 baris. Kolom sama dengan Daftar Pasien RJ ditambah **Status Pemeriksaan** sesudah Konfirmasi — seragam di kelima tab. Rekap Billing per pasien.

---

## 3. Peta v1 → V2

| v1 | V2 sebelum paritas | Status | Task |
| --- | --- | --- | --- |
| Menu Beranda / Daftar Pasien / Riwayat | Antrian Pesanan, Daftar Kerja, Hasil Bacaan | `EXTEND` | `FE-RAD-14` |
| Tab RJ/RI hari ini, Perjanjian | `GET /rad-orders` sudah punya `EncounterType`, `OnlyScheduled`, rentang tanggal | `REUSE WITH ADAPTER` | `BE-RAD-18`, `FE-RAD-14` |
| Tab Persiapan | Tidak ada penanda persiapan | `MISSING` — kolom baru | `BE-RAD-18` |
| Kolom Registrasi, Pasien, Order, Pemeriksaan | Sudah ada (konteks pasien `RAD-CONF-001`) | `READY TO REUSE` | — |
| No. Foto | Nomor study ada di tabel study, tidak di daftar | `EXTEND` | `BE-RAD-18` |
| Diagnosa | `TrxPatientDiagnosis` per kunjungan | `EXTEND` (baca) | `BE-RAD-18` |
| Pemesan, Konfirmasi (nama) | `RequestedByUserId`, `ConfirmedByUserId` tanpa nama | `EXTEND` | `BE-RAD-18` |
| Asuransi / Penjamin | `RegPatientEncounterGuarantors` (pola Lab `r47`) | `EXTEND` | `BE-RAD-18` |
| Pembayaran | Aturan baca pembayaran per kunjungan Lab (`LabPaymentClearanceRules`, `r43`) | `REUSE WITH ADAPTER` | `BE-RAD-18` |
| Riwayat Pemeriksaan | Hasil Bacaan (per bacaan, bukan per pesanan) | `EXTEND` | `FE-RAD-15` |
| Beranda | Rekap pesanan dan study (angka saja) | `EXTEND` | `BE-RAD-19`, `FE-RAD-16` |
| Detail Pasien: Petugas Radiologi, Dokter Pemeriksa | Tidak ada penugasan | `MISSING` | `BE-RAD-20`, `FE-RAD-17` |
| Print Hasil, Print Label, Rincian Pasien | Tidak ada | `MISSING` | `FE-RAD-17` |
| Kunci Lunas sebelum proses | Tidak ada | `MISSING` | `BE-RAD-21` |
| Unggah gambar, jumlah film | Tidak ada | `MISSING` — penyimpanan berkas | `BE-RAD-22` |
| Kirim hasil WhatsApp | Tidak ada | `MISSING` — gerbang pesan sama dengan Lab `LAB-COORD-011` | `BE-RAD-23` |
| PACS | Di luar scope `RAD-DEC-001` | `CONFLICT` → dibuka `RAD-DEC-020` | perlu keputusan bentuk |
| AI | v1 sendiri masih *dummy* | `UNKNOWN` | perlu keputusan bentuk |
| Pasien Luar | v1 data palsu | Tidak diport | — |

---

## 4. Yang dipertahankan V2 (tidak mengikuti v1)

- **Gerbang keselamatan fail-closed** (`RAD-DEC-005`, `RAD-DEC-019`). v1 tidak punya gerbang.
- **Pengesahan dokter radiolog dan koreksi berversi** (`RAD-DEC-003`). Form hasil v1 langsung "Selesai"; di V2
  hasil yang diisi dari form v1 menjadi draf bacaan yang tetap disahkan dan dirilis.
- **Pembatalan wajib beralasan**, riwayat transisi berpelaku.

---

## 5. Yang masih perlu keputusan sebelum task-nya dapat dikerjakan

> **Seluruh butir bagian ini terjawab 2026-10-15** oleh `RAD-DEC-021` (ikut v1) — lihat bagian 7.1. Tabel di bawah
> dipertahankan sebagai jejak pertanyaan semula.

| ID | Pertanyaan | Menahan |
| --- | --- | --- |
| `RAD-OPEN-013` | PACS: sistem PACS mana, protokolnya (DICOMweb/HL7/berkas), dan siapa pemilik integrasinya | Butir PACS `RAD-DEC-020` |
| `RAD-OPEN-014` | AI: v1 masih tiruan. Penyedia, data yang boleh dikirim keluar, dan tanggung jawab klinisnya | Butir AI `RAD-DEC-020` |
| `RAD-OPEN-015` | Kirim WhatsApp: tautan `wa.me` dari peramban seperti v1, atau gerbang pesan rumah sakit (`LAB-COORD-011`) | `BE-RAD-23` |
| `RAD-OPEN-016` | Kunci Lunas: tagihan Radiologi terbit per study **sesudah** mutu citra diputuskan (`RWI-DEC-195`), sehingga sebelum pemeriksaan pasien tunai selalu `NotBilled`. Tagihan diterbitkan lebih awal (milik Billing), atau Radiologi memakai bentuk kunci lain? *(ditemukan 2026-10-15)* | `BE-RAD-21` |
| `RAD-OPEN-017` | Rekap Billing v1 menampilkan tarif per pemeriksaan; Billing V2 sengaja tidak membuka harga per item di luar kasir (`BE-RJE-014`). Bolehkah petugas Radiologi melihatnya? Keputusan pemilik Billing *(ditemukan 2026-10-15)* | `FE-RAD-15` |
| `RAD-DEC-012` (ditinjau ulang) | Detail Pasien v1 memilih Petugas Radiologi dan Dokter Pemeriksa; `RAD-DEC-012` menolak penugasan per orang. Apakah `RAD-DEC-020` mengubahnya? *(ditemukan 2026-10-15)* | `BE-RAD-20`, `FE-RAD-17` |

~~Unggah gambar (`BE-RAD-22`) ikut menunggu `RAD-OPEN-013`, supaya tidak lahir gudang citra kedua di samping PACS.~~
Diganti `RAD-DEC-021`: unggah gambar dibangun mengikuti v1 (penyimpanan privat, bukan PACS). Jumlah film tidak
menunggu apa pun: bahan terpakai jenis `Film` sudah ada.

---

## 6. Urutan pengerjaan

> **Digantikan bagian 7.2** (2026-10-15). Fase 5 (kunci Lunas) dibatalkan karena v1 tidak punya kunci.

1. **Fase 1** — `BE-RAD-18` + `FE-RAD-14`: Daftar Pasien Radiologi (tab, kolom, penyaring) dan menu v1.
2. **Fase 2** — `FE-RAD-15`: Riwayat Pemeriksaan.
3. **Fase 3** — `BE-RAD-19` + `FE-RAD-16`: Beranda Radiologi.
4. **Fase 4** — `BE-RAD-20` + `FE-RAD-17`: Detail Pasien Radiologi, petugas/dokter pemeriksa, Print Hasil/Label, Rincian.
5. **Fase 5** — `BE-RAD-21`: kunci Lunas.
6. **Fase 6** — `BE-RAD-22`: unggah gambar dan jumlah film. `BE-RAD-23` WhatsApp, PACS, dan AI menunggu bagian 5.

---

## 7. Keputusan lanjutan — 2026-10-15

Pemilik modul, 2026-10-15: *"semua keputusan mengikuti alur yang ada pada v1, ikuti saja polanya, sesuaikan dengan
yang sudah ada sekarang terbentuk. Untuk bagian atau alur yang belum ada di V2 tapi di v1 ada, catat alurnya,
terapkan, tapi beri tanda bahwasanya fiturnya belum terbuat."*

| No | Keputusan |
| ---: | --- |
| `RAD-DEC-021` | **Setiap pertanyaan terbuka paritas dijawab oleh perilaku v1 tayang/source v1**, disesuaikan dengan komponen dan pengaman V2 yang sudah ada (`RAD-DEC-018`, `RAD-DEC-019` tetap) |
| `RAD-DEC-022` | **Alur v1 yang belum punya fitur di V2 tetap dipasang di layar** (menu, tab, tombol, kolom) **dengan penanda "Fitur belum tersedia"** beserta catatan alur v1-nya. Penanda dicabut begitu fiturnya dibangun |

### 7.1 Jawaban atas pertanyaan terbuka, dibaca dari source v1

| Butir | Perilaku v1 (bukti) | Penerapan V2 |
| --- | --- | --- |
| `RAD-OPEN-013` PACS | Kolom *Foto Radiologi (PACS)* dan tombol *Kirim ulang ke PACS* hanya menampilkan "Fitur kirim ulang ke PACS belum tersedia" (`handleKirimUlangPacsDetail`) | Kolom dan tombol dipasang **bertanda belum tersedia** — sama dengan v1 |
| `RAD-OPEN-014` AI | *Eksekusi AI* = `Swal` "Eksekusi AI (dummy)" lalu mengisi teks contoh (`handleEksekusiAI`) | Tombol dipasang **bertanda belum tersedia**; teks contoh tidak ditiru (bukan hasil klinis) |
| `RAD-OPEN-015` WhatsApp | `wa.me/<62…>?text=…` dari peramban, nomor pasien, pesan "Hasil … sudah tersedia" (`handleKirimHasilWhatsapp`) | Diikuti apa adanya; nomor dari `MstPatient.WhatsAppNumber` ?? `PhoneNumber` |
| `RAD-OPEN-016` Kunci Lunas | **Tidak ada kunci.** `ubahStatusPembayaran` didefinisikan tetapi tidak pernah dipanggil; status pembayaran hanya kolom tampilan | `BE-RAD-21` **dibatalkan** — kolom Pembayaran (`BE-RAD-18`) sudah setara v1 |
| `RAD-OPEN-017` Tarif | *Rincian Pasien* mencetak harga per pemeriksaan + TOTAL, tanda tangan Petugas Radiologi dan Kasir (`handleRincianPasien`). Halaman *Rekap Billing* v1 **yatim** — tidak ada tautan dari alur hidup | Tarif ditampilkan pada Rincian Pasien, dibaca dari `MstTariff` (preseden `LabExaminationService`). Rekap Billing tidak dibangun (bukan alur hidup) |
| `RAD-DEC-012` vs penugasan | Detail Pasien memilih **Petugas Radiologi** (pesanan) dan **Dokter Pemeriksa** (per pemeriksaan) | Diikuti: dua isian pada pesanan (satu pesanan V2 = satu pemeriksaan). Daftar kerja per alat `RAD-DEC-012` tetap |
| Unggah gambar | Multipart ke server (`photoLabFiles`, `/uploads/HasilLabPhoto`) dari Form Hasil | Diikuti dengan pola penyimpanan `ReferralDocumentStorageService`. **Bertanda belum tersedia sampai dibangun** |
| Jumlah film | Teks bebas pada Form Hasil | Bahan terpakai `Film` yang sudah ada |
| Riwayat — aksi | *Detail*, *Cetak Hasil* | Diikuti |
| Tab Pasien Luar | Data palsu (`makeRows`) | Tab dipasang **bertanda belum tersedia** |
| Tab Medical Check-Up | Dikomentari di `tabsPemesananRadiologi.jsx` — tidak tampil | Tidak dipasang |
| 9 menu Laporan/Pemeriksaan/Setup | "Coming Soon" di v1 tayang | Menu dipasang, halaman **bertanda belum tersedia** |
| Konfirmasi | Membuka *Detail Pasien Radiologi*; tombol konfirmasi ada di dalamnya | Diikuti — `FE-RAD-17` |

### 7.2 Urutan pengerjaan baru

1. **Fase 4a** — penanda belum tersedia: menu Laporan/Pemeriksaan/Setup, tab Pasien Luar.
2. **Fase 4b** — `BE-RAD-20` + `FE-RAD-17`: Detail Pasien Radiologi (Petugas Radiologi, Dokter Pemeriksa, Konfirmasi,
   kolom PACS bertanda), aksi baris Kirim Hasil, Print Hasil, Rincian Pasien (tarif), Print Label; aksi Riwayat.
3. **Fase 6** — `BE-RAD-22`: unggah gambar (sampai selesai tetap bertanda belum tersedia). Status akhir: bagian 7.3.

### 7.3 Keadaan akhir paritas — 2026-10-15

| Fase | Keadaan |
| --- | --- |
| 1 — Daftar Pasien + menu v1 (`BE-RAD-18`, `FE-RAD-14`) | **Selesai** — migration diterapkan ke devYoga |
| 2 — Riwayat (`FE-RAD-15`) | **Selesai** — layar terbawa `FE-RAD-14`; Rekap Billing v1 yatim, tidak dibangun |
| 3 — Beranda (`BE-RAD-19`, `FE-RAD-16`) | **Selesai** |
| 4a — penanda belum tersedia | **Selesai** — 9 menu, tab Pasien Luar, PACS, Cetak Film, AI, Ubah Pemeriksaan (sebagian dicabut 2026-10-15 — bagian 7.4) |
| 4b — Detail Pasien + aksi baris (`BE-RAD-20`, `FE-RAD-17`) | **Selesai** — migration diterapkan; layar data devYoga 17/17 |
| 5 — kunci Lunas (`BE-RAD-21`) | **Dibatalkan** — v1 tanpa kunci |
| 6 — unggah gambar (`BE-RAD-22`) | **Selesai** — migration diterapkan; penanda dicabut |
| WhatsApp (`BE-RAD-23`) | **Selesai tanpa backend** — `wa.me` seperti v1 |

Tiga migration Radiologi diterapkan ke devYoga 2026-10-15 atas instruksi pemilik modul (riwayat 318 → 321). Yang
tersisa bukan pekerjaan kode — lihat `roadmap/backend-roadmap.md` bagian 0.3.

### 7.4 Fitur bertanda yang dibangun — 2026-10-15

Pemilik modul memilih membangun fitur bertanda yang tidak bergantung pada sistem luar. Alur v1 tetap acuan;
karena di v1 tayang menu-menu ini "Coming Soon", isinya diturunkan dari data pesanan V2.

| Fitur v1 | V2 | Task |
| --- | --- | --- |
| Laporan → Statistik Pemeriksaan, Dokter Pengirim, Pasien Perusahaan | Laporan rentang tanggal WIB, CSV, cetak | `BE-RAD-24`, `FE-RAD-18` |
| Pemeriksaan → Harian, Per Item | Sama | `BE-RAD-24`, `FE-RAD-18` |
| Setup → Tarif Tindakan | Baca-saja dari `MstTariff`; pengisian tetap milik master tarif | `BE-RAD-24`, `FE-RAD-18` |
| Setup → Template Hasil | Kelola template (dokter radiolog); pemilih di draf bacaan mengisi ruas kosong | `BE-RAD-26`, `FE-RAD-18` |
| Detail → Ubah Pemeriksaan | Sebelum study, alasan wajib, tercatat | `BE-RAD-25`, `FE-RAD-18` |
| Detail → Cetak Film | Centang `PrintFilm`, tercatat | `BE-RAD-25`, `FE-RAD-18` |

**Tetap bertanda:** Setup → Kategori dan Parameter ("Coming Soon" di v1, kategori V2 ada di master pemeriksaan),
tab Pasien Luar (data contoh di v1), Foto PACS (`RAD-OPEN-013`), AI (`RAD-OPEN-014`). Migration
`AddRadReportTemplateAndFilmPrint` menunggu instruksi untuk devYoga.

---

## 8. Keputusan lanjutan — 2026-10-19: Form Hasil tanpa konsol pemeriksaan

Pemilik modul, 2026-10-19, atas pertanyaan apakah hasil boleh disimpan tanpa konsol seperti v1 (mengubah
`RAD-DEC-018`): *"yaaa saya ingin seperti ini. proses yang di foto sepertinya tidak diperlukan hapus saja yaa
pastikan aman jadi pada form hasil langsung munculkan textbox seperti yang v1 samakan saja konsepnya dan ada
tombol simpan yaaa seperti v1 yang ketika sudah diklik langsung redirect lalu datanya tampil di tabel"*.

| No | Keputusan |
| ---: | --- |
| `RAD-DEC-023` | **Form Hasil mengikuti v1: hasil diisi langsung dan langsung berlaku setelah Simpan**, tanpa konsol pemeriksaan (verifikasi pasien, gerbang keselamatan, akuisisi, penilaian mutu) dan tanpa pengesahan/rilis terpisah. Mengubah `RAD-DEC-018` (gerbang keselamatan dan pengesahan radiolog) untuk jalur ini; `RAD-DEC-019` tidak lagi menahan pengisian hasil |

Yang tetap dijaga agar penghapusan konsol aman:

| Hal | Sebelumnya (konsol) | Sekarang (Form Hasil) |
| --- | --- | --- |
| Tagihan | Fakta kelayakan tagih terbit saat mutu citra diterima | Terbit saat hasil pertama disimpan, sekali per study (`BillingFactSubmitted`); gagal kirim dicoba lagi pada Simpan berikutnya |
| Study | Dibuat dan dinaikkan statusnya di konsol | Dibuat atau dilompati ke `QualityAccepted` oleh Simpan; riwayat bertanda `RAD-DEC-023` |
| Status pesanan | Selesai setelah bacaan dirilis | Setiap Simpan menyelesaikan pesanan yang masih terbuka — termasuk pesanan yang hasilnya sudah dirilis lewat alur lama tetapi statusnya masih Accepted |
| Isi hasil | Draf → disahkan → dirilis | Langsung `Released`; simpan ulang dengan isi berbeda menjadi versi koreksi, versi lama `Superseded` |
| Pesanan belum dikonfirmasi, ditahan, batal, ditolak | — | Ditolak backend dan diterangkan di form |

Kontrak: `PUT rad-reports/by-order/{radOrderId}/result` dengan `{ findings, notes }` — `notes` ("Catatan Tambahan"
v1) opsional; kosong berarti hasil itu sendiri menjadi kesimpulan. Hak akses `RadReport : Create`. Detail Pasien
tidak lagi punya tombol *Buka Pemeriksaan*; kolom Hasil menampilkan isi hasil yang tersimpan.
