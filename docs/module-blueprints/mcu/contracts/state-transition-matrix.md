# Matriks Perpindahan Status — Modul MCU

| Field | Nilai |
| --- | --- |
| Contract | `MCU-STATE-v1` |
| contract_version | `v1` — status `approved` |
| Owner | Yoga Aji Pratama |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21 |
| input_revision | Decision log r5 (`MCU-DEC-031` label status); `MCU-HDA-001` bagian G; `MCU-EVD-002` 2.1 |

Kolom *Bila dilanggar* menyebut kode status dan pesan bagi pengguna. Setiap perpindahan yang sah
menulis satu baris `McuOrderStatusHistory`.

## 1. Order paket MCU (`McuOrderStatus`)

### 1.1 Perpindahan yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Booking / impor | Terjadwal | Admisi (`McuOrder : Create`/`Import`) | Paket aktif; tanggal buka; kuota tersisa atau pelampauan; pasien **atau** identitas sementara ber-NIK + tanggal lahir | `409` *"Kuota MCU tanggal 27-10-2026 sudah penuh (30/30)."* |
| — | Rekomendasi | Terjadwal | Dokter RJ/RI/IGD (`McuOrder : Recommend`) | Kunjungan asal milik pasien yang sama; tanggal wajib (`MCU-DEC-037`); kuota | `400` *"Tanggal pemeriksaan MCU wajib diisi."* |
| — | *Walk-in* admisi | Hadir | Admisi (`Create` + `CheckIn`) | Sama dengan check-in; memakai kuota hari ini | Seperti check-in |
| Terjadwal | Check-in | Hadir | Admisi (`McuOrder : CheckIn`) | Tanggal = hari ini; pasien tertaut (NIK + tanggal lahir, atau dikonfirmasi petugas); dokter MCU dipilih; semua persyaratan ditandai; tarif berlaku; satu transaksi dengan kunjungan + baris paket | `409` *"Order ini dijadwalkan 28-10-2026, bukan hari ini."* |
| Terjadwal | Tutup hari | Tidak Hadir | Sistem | Tanggal sudah lewat, belum check-in | — |
| Terjadwal, Tidak Hadir | Jadwal ulang | Terjadwal | Admisi (`McuOrder : Update`) | Tanggal baru buka + kuota; kuota tanggal lama dilepas | `409` kuota penuh |
| Terjadwal | Batal sebelum datang | Dibatalkan | Admisi (`McuOrder : Cancel`) | Alasan wajib; kuota dilepas; tanpa refund (`MCU-DEC-038`) | `400` alasan kosong |
| Hadir | Mulai pemeriksaan | Dalam Pemeriksaan | Petugas MCU (`McuExamination : Start`) | Perorangan **lunas** atau perusahaan **terjamin** (`LabPaymentClearanceRules`); pesanan Lab/Rad dibuat dalam transaksi yang sama | `409` *"Tagihan paket belum lunas (sisa Rp 350.000)."* |
| Hadir, Dalam Pemeriksaan | Batal penuh | Dibatalkan | Admisi (`McuOrder : Cancel`) | **Belum ada komponen dimulai** (lihat 2.3); alasan wajib; pesanan turunan dibatalkan; refund utuh diajukan; kunjungan dibatalkan | `409` *"Sudah ada pemeriksaan yang dimulai. Gunakan pembatalan per komponen."* |
| Dalam Pemeriksaan | Komponen terakhir tuntas | Menunggu Resume | Sistem | Semua komponen `Completed` atau `Cancelled` | — |
| Menunggu Resume | Tambah komponen di luar paket | Dalam Pemeriksaan | Dokter MCU yang ditetapkan | `MCU-DEC-036` — order kembali menunggu komponen baru | `403` bukan dokter yang ditetapkan |
| Menunggu Resume | Approve resume | Disetujui Dokter | Dokter MCU yang ditetapkan (`McuResume : Approve`) | Skrining selesai; kategori kelayakan, kondisi umum, rekomendasi terisi | `409` *"Skrining awal peserta belum selesai."* |
| Disetujui Dokter | Kembalikan | Menunggu Resume | Petugas MCU (`McuResume : Verify`) | Alasan wajib | `400` alasan kosong |
| Disetujui Dokter | Verifikasi | Terverifikasi | Petugas MCU (`McuResume : Verify`) | Bukan dokter yang approve; seluruh komponen Lab/Rad yang tuntas-selesai punya hasil tervalidasi | `403` *"Verifikasi tidak boleh dilakukan oleh dokter yang menyetujui resume."* |
| Terverifikasi | Penyerahan pertama | Diserahkan | Petugas MCU (`McuDelivery : Create`) | Lihat validasi penyerahan | `409` lihat `VAL-MCU-31` |
| Diserahkan | Penyerahan berikutnya | Diserahkan | Petugas MCU | — | — |
| Disetujui Dokter, Terverifikasi, Diserahkan | Buka ulang | Menunggu Resume | Dokter MCU yang ditetapkan (`McuResume : Reopen`) | Alasan wajib; versi lama → *Dikoreksi*; versi baru *Draf*; penyerahan versi lama ditandai perlu pemberitahuan | `400` alasan kosong |

### 1.2 Perpindahan yang tidak sah

| Dari | Ke | Contoh percobaan | Respons |
| --- | --- | --- | --- |
| Terjadwal | Dalam Pemeriksaan | Petugas menekan *Mulai pemeriksaan* sebelum check-in | `409` *"Peserta belum check-in."* |
| Hadir | Menunggu Resume / Disetujui Dokter | Mencoba approve sebelum pemeriksaan dimulai | `409` *"Pemeriksaan belum dimulai."* |
| Dalam Pemeriksaan | Disetujui Dokter | Approve saat Thorax belum diekspertise | `409` *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* |
| Disetujui Dokter | Diserahkan | Menyerahkan sebelum verifikasi | `409` *"Hasil belum diverifikasi."* |
| Tidak Hadir | Hadir | Check-in order kemarin | `409` *"Order sudah Tidak Hadir. Jadwalkan ulang lebih dulu."* |
| Dibatalkan | apa pun | — | `409` *"Order sudah dibatalkan."* |
| Terjadwal | Terjadwal (tanggal sama) | Jadwal ulang ke tanggal yang sama | `400` *"Tanggal baru sama dengan tanggal sekarang."* |
| apa pun | apa pun | Dua petugas mengubah order yang sama hampir bersamaan | `409` *"Data order sudah diubah petugas lain. Muat ulang lalu coba lagi."* (`Version`) |

## 2. Komponen order (`McuOrderComponentStatus`)

### 2.1 Perpindahan yang sah

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Order dibuat / komponen ditambah | Pending | Sistem / dokter MCU | — | — |
| Pending | Persyaratan tidak terpenuhi → jadwal ulang | Rescheduled | Admisi saat check-in | Tanggal susulan buka (tanpa memakai kuota) | `400` tanggal tutup |
| Rescheduled | Tanggal susulan tiba | Pending | Sistem | — | — |
| Pending (Lab/Rad) | Order *Dalam Pemeriksaan* / komponen tambahan saat order sudah berjalan | Ordered | Sistem | Pesanan Lab/Rad berhasil dibuat | Seluruh transaksi gagal |
| Pending (Pemeriksaan MCU) | Order *Dalam Pemeriksaan* | ReadyToFill | Sistem | — | — |
| Ordered | Pesanan Lab/Rad selesai di modulnya | Completed | Sistem (sinkron) | Lab `Completed`; Rad selesai dengan laporan final | — |
| ReadyToFill | Simpan hasil dengan `complete = true` | Completed | Dokter/petugas MCU | Skrining selesai; isian wajib terisi | `409` *"Skrining awal peserta belum selesai."* |
| Pending, Ordered, ReadyToFill, Rescheduled | Batalkan komponen | Cancelled | Dokter MCU yang ditetapkan | Alasan wajib; pemeriksaan Lab komponen dibatalkan satu per satu lewat `LabExaminationService.CancelAsync`, pesanan Rad dibatalkan (`ARC-TECH-01`, terbukti `MCU-EVD-002` 2.1). MCU menolak lebih dulu bila pemeriksaan Lab sudah tervalidasi, karena Lab sendiri hanya menolak yang sudah dirilis; penolakan `422` Lab diterjemahkan ke `409` di kolom kanan. Refund sebesar `RefundAmountSnapshot` bila > 0 dan sudah dibayar, lewat Billing yang diperluas (`MCU-DEC-043`) | `409` *"Pemeriksaan Lab sudah divalidasi dan tidak dapat dibatalkan."* |

### 2.2 Tidak sah

| Percobaan | Respons |
| --- | --- |
| Membatalkan komponen `Completed` | `409` *"Komponen sudah selesai."* |
| Mengisi hasil komponen Lab/Rad lewat MCU | `400` *"Hasil Laboratorium/Radiologi diisi di modulnya."* |
| Mengubah isian Pemeriksaan MCU saat resume berstatus Disetujui/Terverifikasi | `409` *"Hasil terkunci. Minta dokter membuka ulang resume."* |

### 2.3 Arti "komponen sudah dimulai" (`MCU-DEC-021`)

| Jenis | Dianggap dimulai bila |
| --- | --- |
| Laboratorium | Pesanan Lab berstatus lebih lanjut dari `Requested` (spesimen sudah diterima/diproses) |
| Radiologi | Pesanan Rad sudah `start` (akuisisi dimulai) |
| Pemeriksaan MCU | Sudah ada `McuExaminationResult` tersimpan |

## 3. Resume (`McuResumeStatus`)

| Dari | Tindakan | Ke | Siapa | Bila dilanggar |
| --- | --- | --- | --- | --- |
| — | Simpan draf pertama | Draft | Dokter MCU yang ditetapkan | `403` |
| Draft | Approve | Approved | Dokter MCU yang ditetapkan | `409` komponen/skrining belum tuntas |
| Approved | Kembalikan | Draft (versi sama) | Petugas MCU | `400` alasan kosong |
| Approved | Verifikasi | Verified | Petugas MCU, bukan pemberi approve | `403` |
| Approved, Verified | Buka ulang | versi lama **Corrected** (`IsCurrent=false`) + versi baru **Draft** | Dokter MCU yang ditetapkan | `400` alasan kosong |
| Corrected | apa pun | — | — | `409` *"Versi ini sudah dikoreksi dan hanya dapat dibaca."* |

## 4. Jadwal harian

| Tindakan | Syarat | Bila dilanggar |
| --- | --- | --- |
| Pakai kuota | `IsOpen` dan `UsedQuota < Quota + ApprovedOverrideQuota` | `409` kuota penuh / `409` *"MCU tutup pada 26-10-2026."* |
| Turunkan kuota | Kuota baru ≥ `UsedQuota` | `409` *"Sudah ada 26 peserta terjadwal; kuota tidak boleh di bawah 26."* |
| Tutup tanggal | Tidak ada order *Terjadwal* pada tanggal itu | `409` *"Masih ada 12 peserta terjadwal pada tanggal ini."* |
