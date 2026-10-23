# Matriks Uji Penerimaan — Modul MCU

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21 |
| Sumber | Acceptance criteria `MCU-AC-001`..`029` (decision log), `MCU-VAL-v1`, `MCU-STATE-v1` |

Jenis uji: **Unit** (service, tanpa basis data nyata), **Integrasi** (harness InMemory/SQLite seperti
`PatientManagementTests`), **devYoga** (berjalan di basis data dev pemilik), **UAT** (dijalankan pengguna
lewat layar). Seluruh contoh memakai data samaran.

## 1. Katalog dan jadwal

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-MCU-01` | `VAL-MCU-04` | Paket Basic Rp 350.000 dengan total nilai pengembalian Rp 390.000 | Unit | `400`, pesan menyebut Rp 390.000 dan Rp 350.000; paket tidak tersimpan |
| `AT-MCU-02` | `VAL-MCU-05` | Tambah harga PT Contoh 1–31 Des padahal sudah ada 1 Okt–31 Des | Unit | `400`; harga lama tetap |
| `AT-MCU-03` | `VAL-MCU-03` | Paket memakai tarif yang bukan tarif paket | Unit | `400` |
| `AT-MCU-04` | `MCU-AC-003`, `VAL-MCU-11` | Booking ke-31 pada tanggal berkuota 30 | Integrasi | `409` menyebut 30/30; `UsedQuota` tetap 30 |
| `AT-MCU-05` | `VAL-MCU-12` | Dua booking bersamaan untuk sisa kuota terakhir | Integrasi | Tepat satu berhasil; satu `409`; `UsedQuota` = kuota |
| `AT-MCU-06` | `MCU-AC-022` | Kepala unit menyetujui pelampauan 5 → booking ke-31..35 diterima; petugas admisi tanpa izin mencoba melampaui | Integrasi | Pelampauan tercatat beralasan; tanpa izin `403` |
| `AT-MCU-07` | `MCU-AC-022` | Gula Darah Puasa dijadwal ulang ke Selasa yang kuotanya penuh | Integrasi | Jadwal ulang komponen berhasil; `UsedQuota` Selasa tidak berubah |

## 2. Booking, rekomendasi, impor, tidak hadir, batal

| ID | Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- | --- |
| `AT-MCU-08` | `MCU-AC-001`, `MCU-AC-026` | Booking peserta baru dengan identitas sementara | Integrasi | Satu order *Terjadwal*; `PatientId` kosong; nol kunjungan; nol baris tagihan; nol pasien baru |
| `AT-MCU-09` | `VAL-MCU-16`, `VAL-MCU-17` | Booking tanpa NIK; NIK 15 digit | Unit | `400` masing-masing |
| `AT-MCU-10` | `VAL-MCU-18` | Booking kedua untuk NIK yang sama pada tanggal sama | Integrasi | `409` menyebut nomor order pertama |
| `AT-MCU-11` | `MCU-AC-002`, `VAL-MCU-19` | Dokter poli merekomendasikan dengan tanggal; lalu tanpa tanggal | Integrasi | Order *Terjadwal* + kuota terpakai; tagihan kunjungan poli tidak bertambah. Tanpa tanggal `400` |
| `AT-MCU-12` | `VAL-MCU-22` | Impor 40 baris: 2 NIK salah, 1 kuota penuh | Integrasi | 37 order dibuat; ringkasan 3 gagal beserta nomor baris; berkas tidak tersimpan |
| `AT-MCU-13` | `MCU-AC-004`, `AC-023` | Order *Terjadwal* lewat tutup hari, dengan dan tanpa konfirmasi WhatsApp | Integrasi | Keduanya *Tidak Hadir*; kuota dilepas; nol kunjungan |
| `AT-MCU-14` | `MCU-DEC-038` | Batal sebelum datang dengan alasan; tanpa alasan | Integrasi | *Dibatalkan*, kuota dilepas, nol refund; tanpa alasan `400` |
| `AT-MCU-15` | `MCU-STATE-v1` 1.2 | Check-in order *Tidak Hadir* | Integrasi | `409` |

## 3. Check-in

| ID | Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- | --- |
| `AT-MCU-16` | `MCU-AC-001` | Check-in order Budi | Integrasi | Order yang **sama** *Hadir*; tepat satu kunjungan `MedicalCheckup`; tepat satu baris paket; satu riwayat dokter |
| `AT-MCU-17` | `VAL-MCU-30` | Kirim ulang check-in yang sama | Integrasi | Tidak ada kunjungan/baris kedua |
| `AT-MCU-18` | `MCU-AC-027` | Identitas sementara dengan NIK + tanggal lahir yang sama dengan pasien lama | Integrasi | Tertaut ke pasien lama; nol pasien baru |
| `AT-MCU-19` | `VAL-MCU-25` | NIK sama, tanggal lahir beda, tanpa konfirmasi; lalu dengan konfirmasi | Integrasi | Pertama `409`; kedua berhasil dengan `ConfirmedByStaff` dan petugas tercatat |
| `AT-MCU-20` | `MCU-AC-005`, `VAL-MCU-27` | Persyaratan puasa tidak ditandai; lalu ditandai *jadwal ulang komponen* | Integrasi | Pertama `400`; kedua GDP *Dijadwal ulang*, komponen lain berjalan |
| `AT-MCU-21` | `MCU-AC-024`, `VAL-MCU-26` | Check-in tanpa dokter MCU | Unit | `400` |
| `AT-MCU-22` | `MCU-AC-007` | Peserta PT Contoh check-in 27 Okt (harga khusus berlaku) dan 5 Jan (berakhir) | Integrasi | Baris paket Rp 300.000 lalu Rp 350.000 |
| `AT-MCU-23` | `MCU-AC-020` | Check-in kunjungan MCU | Integrasi | Tagihan tanpa baris biaya administrasi |
| `AT-MCU-24` | Transaksi 2.1 | Tarif paket nonaktif saat check-in | Integrasi | `409`; order tetap *Terjadwal*; nol kunjungan |
| `AT-MCU-25` | `MCU-AC-017`, `MCU-AC-030` | Check-in dari kiosk: dengan dokter jaga → dokter MCU = dokter jaga; tanpa dokter jaga → `409` (`VAL-MCU-47`) | — | Di luar MVP (`EPIC MCU-08`); keputusan `MCU-DEC-042` |

## 4. Pemeriksaan dan komponen

| ID | Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- | --- |
| `AT-MCU-26` | `MCU-AC-018`, `VAL-MCU-34` | *Mulai pemeriksaan* perorangan belum lunas; lalu sesudah lunas | Integrasi | `409` tanpa pesanan; sesudah lunas pesanan Lab per disiplin + Rad per komponen dibuat, bertanda `CoveringMcuOrderId`, `InstructingDoctorId` = dokter MCU |
| `AT-MCU-27` | `MCU-AC-008` | Peserta perusahaan terjamin | Integrasi | *Mulai pemeriksaan* berhasil tanpa pembayaran |
| `AT-MCU-28` | `MCU-AC-006`, `MCU-AC-016` | Spesimen pesanan bertanda diterima Lab; Thorax diakuisisi | Integrasi | Nol fakta `LaboratoryCharge`/`RadiologyCharge`; tagihan tetap satu baris paket; kunjungan tetap lunas |
| `AT-MCU-29` | `MCU-DEC-036` | Dokter MCU menambah Kolesterol Total | Integrasi | Komponen di luar paket; pesanan Lab **tanpa** tanda; saat spesimen diterima tagihan bertambah Rp 75.000; kelengkapan *4 dari 5* |
| `AT-MCU-30` | `MCU-AC-009`, `MCU-DEC-015` | Dokter MCU membatalkan Thorax (nilai Rp 150.000) | Integrasi + devYoga | Komponen *Dibatalkan*, pesanan Rad batal, pengajuan refund **tercatat** Rp 150.000 (bukan Rp 350.000); pembatalan komponen kedua pada baris yang sama menghasilkan refund kedua, total ≤ nilai baris (`MCU-AC-031`). Bergantung perluasan Billing `MCU-DEC-043` |
| `AT-MCU-31` | `VAL-MCU-38` | Dokter lain mencoba membatalkan komponen | Unit | `403` |
| `AT-MCU-32` | `MCU-AC-014`, `VAL-MCU-41` | Batal penuh sebelum dan sesudah spesimen diterima | Integrasi | Sebelum: *Dibatalkan*, refund utuh, kunjungan batal; sesudah: `409` |
| `AT-MCU-33` | `VAL-MCU-36` | Isi Pemeriksaan Fisik sebelum skrining selesai | Integrasi + devYoga | `409`; sesudah skrining berhasil — membuktikan `MCU-UNK-05` |
| `AT-MCU-34` | `VAL-MCU-43` | Isian angka `6,4` dan `6.4` | Unit | `6.4` tersimpan utuh, tidak menjadi `64` |
| `AT-MCU-35` | `MCU-AC-029` | EKG diselesaikan tanpa lampiran | Unit | Komponen *Selesai* |
| `AT-MCU-36` | `MCU-AC-028` | Thorax selesai dibaca dr. Andi; komponen batal | Integrasi | Sumber jasa memuat Thorax–dr. Andi–Rp 30.000; komponen batal tidak muncul |
| `AT-MCU-37` | `ARC-TECH-01` | Batalkan satu dari dua pemeriksaan Hematologi dalam satu pesanan Lab | Integrasi | Hanya pemeriksaan itu batal; pemeriksaan lain tetap berjalan. **Sisi Lab sudah terbukti** 2026-10-21 (`MCU-EVD-002` 2.1); yang diuji di task MCU adalah pemanggilannya dari pembatalan komponen |

## 5. Resume, verifikasi, penyerahan

| ID | Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- | --- |
| `AT-MCU-38` | `MCU-AC-010`, `VAL-MCU-35` | Approve saat Thorax belum diekspertise | Integrasi | `409` menyebut Thorax |
| `AT-MCU-39` | `MCU-AC-025` | Approve oleh dokter yang ditetapkan; oleh dokter lain; sesudah kepala unit mengganti dokter | Integrasi | Berhasil / `403` / dokter pengganti berhasil, dokter lama `403` |
| `AT-MCU-40` | `MCU-AC-011`, `VAL-MCU-39` | Verifikasi oleh dokter pemberi approve; oleh petugas | Integrasi | `403` / berhasil tanpa mengubah isi |
| `AT-MCU-41` | `VAL-MCU-32` | Serahkan sebelum verifikasi | Unit | `409` |
| `AT-MCU-42` | `MCU-AC-012`, `VAL-MCU-31` | Serahkan isi lengkap ke perusahaan tanpa persetujuan; serahkan ringkasan | Integrasi | `409`; ringkasan berisi kategori + rekomendasi kerja **saja** (diperiksa: nol ruas nilai/diagnosis di DTO) |
| `AT-MCU-43` | `MCU-AC-013` | Buka ulang sesudah diserahkan | Integrasi | Versi 1 *Dikoreksi* tetap terbaca; versi 2 *Draf*; penyerahan versi 1 `RequiresCorrectionNotice = true`; isian terbuka, revisi nilai tercatat |
| `AT-MCU-44` | `MCU-AC-015`, `MCU-AC-021` | Catat penyerahan WhatsApp tanpa tujuan; dengan tujuan | Unit | `400` / tersimpan dengan petugas dan waktu |
| `AT-MCU-45` | Privasi log | Seluruh tindakan ubah pada uji di atas | Integrasi | Log aplikasi tidak memuat NIK, nilai isian, isi resume, atau tujuan penyerahan |

## 6. Uji berjalan devYoga (task backend pertama, `MCU-DEC-028`)

| ID | Membuktikan | Langkah |
| --- | --- | --- |
| `AT-MCU-RUN-01` | `MCU-UNK-01` | Check-in satu order uji → kunjungan `MedicalCheckup` di klinik MCU terbentuk dengan antrean |
| `AT-MCU-RUN-02` | `MCU-UNK-05` | Perawat menyelesaikan skrining lewat nurse station → MCU membaca *skrining selesai* |
| `AT-MCU-RUN-03` | `MCU-UNK-06` | Kueri baca-saja: klinik/unit MCU ada dan `IsQueueRequired = true` |
