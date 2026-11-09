# MVP Penuntasan Rawat Inap

| Atribut | Keterangan |
|---|---|
| Modul | Rawat Inap (`InPatientManagement`), blueprint `RWI-BP-001` bentuk `COMPOSITE` |
| Jenis dokumen | Paket kerja lintas sub-modul. Dokumen ini **tidak membuat task baru**; ia mengumpulkan task yang sudah ada di roadmap masing-masing sub-modul menjadi satu paket yang dikerjakan sampai tuntas |
| Tanggal | 9 November 2026 (Asia/Jakarta) |
| Disetujui | Muhammad Hamzah, 9 November 2026 ("oke sekarang kerjakan semua task tersebut"), termasuk izin menulis source backend `MHamzah`, source frontend `HamzahV2`, dokumen blueprint, dan data uji di database development `QuilvianNewDevHamzah` |
| Snapshot source | Backend `53418d372b6e37fa76aeabea9cf3b395c5534641`; frontend `bb7e93dc839d70c389d1aa5b98ff5ab1342ff79e` |
| Pengganti ringkasan lama | [Daftar-Pekerjaan-Belum-Selesai.md](Daftar-Pekerjaan-Belum-Selesai.md) (23 Oktober 2026) sudah usang karena belum memuat repair 015, 043, dan 044 tanggal 2 November 2026 |

## 1. Tujuan

Menuntaskan **semua pekerjaan rawat inap yang bisa dikerjakan tanpa menunggu keputusan pihak
lain**. Setelah paket ini selesai, sisa pekerjaan modul tinggal empat task yang menunggu
keputusan Registrasi, Billing, dan manajemen tarif, ditambah rencana Kajian Pasien yang belum
disetujui.

Contoh hasil yang dirasakan rumah sakit: perawat jaga malam langsung melihat daftar pasien
dengan skor peringatan dini (*Early Warning Score*, EWS) kritis, dokter mencetak resume medis
yang memuat jadwal kontrol, dan perawat dapat memesan pemeriksaan darah atas instruksi dokter
tanpa harus menunggu dokter membuka layarnya sendiri.

## 2. Bukti awal yang sudah diperiksa (9 November 2026)

Pemeriksaan dilakukan **hanya membaca** pada database development `QuilvianNewDevHamzah`.

| Yang diperiksa | Hasil | Artinya |
|---|---|---|
| Migration `20261102050639_AddStructuredFollowUpToInpDischargeSummary` (milik `BE-RWI-256`) | Sudah tercatat di `__EFMigrationsHistory` | Kolom rencana kontrol sudah ada di database; tidak perlu izin migration lagi |
| Migration `20261012041938_AddAdmissionTransferOrigin` (milik `FIX-EPS-006-07`) | Sudah tercatat | Relasi asal transfer IGD/Poli sudah ada di database |
| Migration `20260909065125_AddInpatientEpisodeContextToPatientDiagnosis` (milik `BE-RWI-068`) | Sudah tercatat | Catatan "migration belum diterapkan" pada ringkasan 23 Oktober tidak berlaku lagi untuk database ini |
| Data master | 189 tempat tidur, 154 kamar, 5 penjamin, 49 episode (7 di antaranya sudah `Closed`) | Hambatan "data master kosong" (`RWI-UI-GAP-007`) kemungkinan besar sudah gugur |
| Source repair 015 dan 043 | Sudah masuk commit backend `da737fb9` dan frontend `66b7e5600` (2 November 2026), tetapi roadmap masih menulis "Belum dikerjakan" | Pekerjaannya adalah membuktikan, memperbaiki bila kurang, lalu membuat laporan |

## 3. Isi paket

Tanda status mengikuti roadmap asal: ✅ selesai dan terbukti, 🟡 sebagian, ⛔ terblokir, tanpa
tanda belum dikerjakan. Kolom status di bawah diperbarui setiap kali satu item selesai.

| Slice | Item | Roadmap asal | Ringkasan | Status |
|---|---|---|---|---|
| 1. Keselamatan pasien di bangsal | `BE-RWI-255` | [roadmap-repair-015](keperawatan/roadmap/roadmap-repair-015.md) | Endpoint daftar pasien EWS berisiko per bangsal | ✅ 9 Nov — [laporan](keperawatan/task/report/backend/BE-RWI-255.md) |
| | `FE-RWI-283` | roadmap-repair-015 | Warna dan golongan EWS mengikuti hitungan server | |
| | `FE-RWI-284` | roadmap-repair-015 | Tab Monitoring "EWS Perlu Lapor Dokter" | |
| | `FE-RWI-285` | roadmap-repair-015 | Tombol "Ajukan Retur" di e-MAR | |
| 2. Resume medis dan berkas admisi | `BE-RWI-256` | [roadmap-repair-043](episode-rawat-inap/roadmap/roadmap-repair-043.md) | Rencana kontrol terstruktur pada resume medis (server) | ✅ 9 Nov — [laporan](episode-rawat-inap/task/report/backend/BE-RWI-256.md) |
| | `FE-RWI-286` | roadmap-repair-043 | Rencana kontrol pada form, detail, dan cetakan resume | |
| | `BE-RWI-257` | roadmap-repair-043 | Lembar Data Dasar Rawat Inap (IPD) memakai nama perujuk luar yang dicatat admisi | ✅ 9 Nov — [laporan](episode-rawat-inap/task/report/backend/BE-RWI-257.md) |
| 3. Pesanan penunjang oleh perawat | `FE-RWI-172` | [frontend-roadmap-finishing dokter](dokter-rawat-inap/roadmap/frontend-roadmap-finishing.md) | Perawat memesan Lab/Radiologi atas instruksi dokter; diawali uji runtime `BE-RWI-104` | 🟡 |
| 4. Admisi transfer IGD/Poli | `FIX-EPS-006-02` s.d. `08` (7 item) | [frontend-roadmap-admisi-transfer-igd](episode-rawat-inap/roadmap/frontend-roadmap-admisi-transfer-igd.md) | Bukti transaksi nyata dari antrean transfer sampai pasien dirawat | 🟡 |
| 5. Alur utama ujung ke ujung | `FE-RWI-035` (memayungi `FE-RWI-003`, `004`, `012`, `013`, `015`, `019`) | [frontend-roadmap episode](episode-rawat-inap/roadmap/frontend-roadmap.md) | Pasien baru dari admisi sampai episode ditutup; layar tertutup bagi peran tak berhak | 🟡 |
| 6. Merapikan dokumen | Roadmap, laporan, ringkasan sisa pekerjaan | — | Menyelaraskan tanda status yang bertentangan (`BE-RWI-127`, `128`, `147` s.d. `159`, `FE-RWI-143` s.d. `146`) dengan bukti | |

## 4. Proses bisnis per slice

### Slice 1 — Keselamatan pasien di bangsal

| Unsur | Isi |
|---|---|
| Tujuan | Pasien yang tanda vitalnya memburuk tidak terlewat oleh perawat jaga |
| Pelaku | Perawat bangsal mencatat tanda vital; kepala ruangan memantau; dokter menerima laporan; apoteker memeriksa retur obat |
| Pemicu | Perawat menyimpan tanda vital yang menghasilkan EWS risiko tinggi atau kritis; atau obat batal diberikan |
| Prasyarat | Pasien berstatus `Admitted` di bangsal; tanda vital tersimpan di deret tanda vital pasien |

Langkah utama:

1. Perawat menyimpan tanda vital. Server menghitung EWS dan golongan risikonya.
2. Tab Vital Sign dan Pengawasan Harian menampilkan warna dan anjuran **apa adanya dari server**.
3. Kepala ruangan membuka Monitoring → tab "EWS Perlu Lapor Dokter" dan menyaring per bangsal.
4. Perawat melapor ke dokter untuk pasien yang tampil di daftar.
5. Bila obat batal diberikan, perawat menekan "Ajukan Retur" di e-MAR; sistem membuat draf retur Farmasi dan mengajukannya; apoteker memeriksa.

Contoh: Tn. B (samaran) di Ruang Mawar Bed 301-A tercatat nadi 128, napas 26, saturasi 91%.
Server memberi EWS 7 golongan kritis. Pasien ini muncul paling atas di tab "EWS Perlu Lapor
Dokter" sehingga perawat jaga langsung menghubungi dokter jaga.

Jalur tidak normal: tanda vital lama (bukan yang terakhir) tidak membuat pasien tampil;
pengguna tanpa hak `InpatientMonitoring : Read` ditolak (kode 403, artinya tidak punya hak
akses).

### Slice 2 — Resume medis dan berkas admisi

| Unsur | Isi |
|---|---|
| Tujuan | Pasien pulang membawa jadwal kontrol yang jelas; lembar IPD tidak kosong bila admisi sudah mencatat perujuk luar |
| Pelaku | DPJP (dokter penanggung jawab pelayanan) mengisi resume; petugas admisi mencatat perujuk luar; petugas PPRI mencetak IPD |
| Pemicu | DPJP menulis resume medis sebelum pasien pulang; petugas mencetak lembar IPD |

Langkah utama:

1. DPJP mengisi tanggal kontrol, poli kontrol, dan dokter kontrol (boleh dikosongkan bila tidak perlu kontrol).
2. Server memeriksa isian: tanggal kontrol tidak boleh sebelum tanggal pulang.
3. Detail dan cetakan resume menampilkan rencana kontrol; revisi resume menyimpan salinannya.
4. Saat mencetak IPD, nama perujuk diambil berurutan: surat pengantar → perujuk luar Registrasi → perujuk luar yang dicatat admisi → garis kosong.

Contoh: DPJP menulis "Kontrol 16 November 2026, Poli Penyakit Dalam, dr. S (samaran)". Bila
DPJP mengisi tanggal 5 November padahal pasien pulang 9 November, server menolak karena
tanggal kontrol tidak mungkin sebelum pasien pulang.

### Slice 3 — Pesanan penunjang oleh perawat

| Unsur | Isi |
|---|---|
| Tujuan | Instruksi lisan atau tertulis dokter dapat langsung diteruskan perawat menjadi pesanan Lab/Radiologi |
| Pelaku | Perawat memesan; dokter berpenugasan aktif menjadi pemberi instruksi; dokter memverifikasi; petugas Lab menerima di worklist |
| Prasyarat | Pasien punya dokter berpenugasan aktif; katalog pemeriksaan tersedia di modul Lab/Radiologi |

Langkah utama:

1. Perawat membuka Penunjang Medis di ruang kerja perawat dan memilih pemeriksaan.
2. Perawat memilih "Dokter pemberi instruksi" dari daftar dokter yang sedang menangani pasien.
3. Sistem menampilkan status tanggungan dan perkiraan harga bila perawat berhak melihatnya.
4. Pesanan terkirim dan masuk worklist Laboratorium dengan status menunggu verifikasi instruksi.

Contoh: atas instruksi dr. A, perawat memesan "Darah Lengkap" untuk Ny. C. Pesanan muncul di
worklist Lab. Bila perawat lupa memilih dokter, pesanan ditolak dengan pesan dari modul Lab.

Syarat pembuka kunci (`RWI-DEC-168`): tombol "Pesan" hanya dibuka bila uji nyata membuktikan
pesanan perawat sampai ke worklist Laboratorium. Bila gagal, tombol tetap terkunci.

### Slice 4 — Admisi transfer IGD/Poli

| Unsur | Isi |
|---|---|
| Tujuan | Pasien IGD atau poliklinik yang diputuskan dirawat inap dapat diterima tanpa mengetik ulang data |
| Pelaku | Dokter IGD/poli membuat permintaan rawat inap; petugas admisi memproses antrean |
| Prasyarat | Disposisi IGD rawat inap dengan permintaan admisi bertanda tangan, atau surat pengantar rawat inap dari poli |

Langkah utama: antrean transfer → "Proses Admisi" → identitas, diagnosis awal, dan DPJP terisi
otomatis → pilih kamar → admisi tersimpan → pasien berstatus `Admitted` dengan penjamin yang
dipilih, bukan tunai bawaan.

### Slice 5 — Alur utama ujung ke ujung

Satu uji nyata menjalankan: admisi pasien baru → penempatan tempat tidur → catatan dokter dan
perawat → DPJP mengizinkan pulang → kasir menyatakan kelayakan keuangan → pasien keluar ruangan
→ episode `Closed`. Uji yang sama menjadi bukti UAT bagi integrasi Billing (`BE-RWI-147` s.d.
`159`).

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| `Draft` | Admisi disimpan | `Admitted` | Petugas admisi | Tempat tidur dipilih, penjamin dipilih |
| `Admitted` | Keputusan pulang | `DischargePending` | DPJP aktif | Resume medis ditandatangani |
| `DischargePending` | Tutup episode | `Closed` | Petugas berwenang | Lima syarat penutupan terpenuhi, termasuk izin kasir |

Kriteria "layar tertutup bagi peran tak berhak" ditelusuri lebih dulu. Bila ternyata
membutuhkan endpoint backend baru, kriteria itu dipisah menjadi task baru dan tidak dipaksakan
di paket ini.

## 5. Urutan pengerjaan

```text
Slice 1 ─┐
         ├─> Slice 3 ─┐
Slice 2 ─┘   Slice 4 ─┼─> Slice 5 ─> Slice 6
```

## 6. Di luar paket

| Item | Alasan |
|---|---|
| `BE-RWI-190`, `BE-RWI-191`, `BE-RWI-203`, `FE-RWI-221` | Menunggu keputusan Registrasi (`RWI-OQ-128`), Billing (`RWI-OQ-129`), dan manajemen tarif (`DEC-INP-020`) |
| Rencana kerja Kajian Pasien dokter | Belum disetujui pemilik; usulan ID `FE-RWI-145` bentrok dengan task yang sudah ada |
| Sembilan kapabilitas masa depan (MAR lanjutan, handover shift, dan lainnya) | Belum menjadi task; harus melewati gerbang kelengkapan requirement |

## 7. Definisi selesai paket

1. Seluruh item slice 1 sampai 5 berstatus ✅ dengan laporan task, atau berstatus 🟡/⛔ dengan alasan dan pemilik yang masih berlaku.
2. Roadmap asal dan dokumen ini menunjukkan status yang sama.
3. Ringkasan sisa pekerjaan modul ditulis ulang sesuai kenyataan.

## 8. Catatan pelaksanaan

Bagian ini diisi selama pengerjaan.
