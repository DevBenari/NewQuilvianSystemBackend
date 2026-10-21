# Matriks Validasi — Modul MCU

| Field | Nilai |
| --- | --- |
| Contract | `MCU-VAL-v1` |
| contract_version | `v1` — status `approved` |
| Owner | Yoga Aji Pratama |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21 |
| input_revision | Decision log r5; `state-transition-matrix.md` `MCU-STATE-v1` |

Kolom *Asal* menunjuk keputusan pemilik. Aturan yang semula berlabel **DES** (usulan desain) disahkan
seluruhnya oleh `MCU-DEC-041` pada 2026-10-21; keterangan dalam kurung menyebut alasan desain asalnya.

## 1. Katalog

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode | Asal |
| --- | --- | --- | --- | --- | --- | --- |
| `VAL-MCU-01` | Kode paket unik | Buat/ubah paket | Kode sudah dipakai paket lain yang tidak terhapus | *"Kode paket MCU-BASIC sudah dipakai."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-02` | Paket minimal satu komponen | Buat/ubah paket | Daftar komponen kosong | *"Paket harus berisi sekurang-kurangnya satu pemeriksaan."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-03` | Tarif harus tarif paket aktif | Paket, harga perusahaan | `MstTariff.IsPackageTariff = false` atau tidak aktif | *"Tarif yang dipilih bukan tarif paket aktif."* | `400` | `MCU-DEC-025` |
| `VAL-MCU-04` | Σ nilai pengembalian ≤ harga umum | Buat/ubah paket | Jumlah nilai pengembalian > `NormalPrice` tarif umum | *"Total nilai pengembalian Rp 390.000 melebihi harga paket Rp 350.000."* | `400` | `MCU-DEC-015` |
| `VAL-MCU-05` | Harga perusahaan tidak tumpang tindih | Tambah harga perusahaan | Rentang tanggal beririsan dengan harga lain untuk paket + perusahaan sama | *"PT Contoh sudah punya harga untuk paket ini pada 1–31 Desember 2026."* | `400` | `MCU-DEC-020` |
| `VAL-MCU-06` | Isian komponen sesuai jenis | Komponen paket | Lab tanpa prosedur ber-`IsLaboratory`; Rad tanpa prosedur ber-`IsRadiology` atau tanpa modalitas; Pemeriksaan MCU tanpa jenis | *"Komponen Radiologi wajib memilih prosedur dan modalitas."* | `400` | `MCU-DEC-007` |
| `VAL-MCU-07` | Komponen tidak ganda dalam satu paket | Komponen paket | Prosedur/jenis yang sama dua kali | *"Gula Darah Puasa sudah ada di paket ini."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-08` | Nilai pengembalian & jasa ≥ 0 | Komponen paket | Nilai negatif | *"Nilai tidak boleh negatif."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-09` | Isian pilihan wajib punya daftar pilihan | Jenis Pemeriksaan MCU | `FieldType = Option` tanpa `OptionValues` | *"Isian pilihan wajib berisi daftar pilihan."* | `400` | `MCU-DEC-041` |

## 2. Jadwal dan kuota

| ID | Aturan | Berlaku pada | Kondisi | Pesan | Kode | Asal |
| --- | --- | --- | --- | --- | --- | --- |
| `VAL-MCU-10` | Tanggal harus buka | Booking, rekomendasi, *walk-in*, jadwal ulang | Tidak ada jadwal atau `IsOpen = false` | *"MCU tutup pada 26-10-2026."* | `409` | `MCU-DEC-012` |
| `VAL-MCU-11` | Kuota tersedia | Sama | Terpakai ≥ kuota + pelampauan | *"Kuota MCU tanggal 27-10-2026 sudah penuh (30/30). Minta persetujuan kepala unit untuk melampaui."* | `409` | `MCU-DEC-012`, `029`, `037` |
| `VAL-MCU-12` | Konkurensi kuota | Sama | Dua petugas mengambil sisa kuota terakhir bersamaan | Yang kalah: *"Kuota baru saja terpakai petugas lain. Muat ulang."* | `409` | `MCU-DEC-041` |
| `VAL-MCU-13` | Pelampauan beralasan oleh kepala unit | Pelampauan | Alasan kosong atau pengguna tanpa `ApproveQuotaOverride` | *"Alasan pelampauan wajib diisi."* / `403` | `400`/`403` | `MCU-DEC-029` |
| `VAL-MCU-14` | Kuota tidak di bawah terpakai | Atur jadwal | Kuota baru < `UsedQuota` | *"Sudah ada 26 peserta terjadwal; kuota tidak boleh di bawah 26."* | `409` | `MCU-DEC-041` |
| `VAL-MCU-15` | Tanggal tidak boleh lampau | Booking, rekomendasi, jadwal ulang | Tanggal < hari ini (WIB) | *"Tanggal pemeriksaan tidak boleh sebelum hari ini."* | `400` | `MCU-DEC-041` |

## 3. Booking, rekomendasi, impor

| ID | Aturan | Berlaku pada | Kondisi | Pesan | Kode | Asal |
| --- | --- | --- | --- | --- | --- | --- |
| `VAL-MCU-16` | Peserta = pasien atau identitas sementara lengkap | Booking, impor | Tanpa `patientId` dan tanpa nama/NIK/tanggal lahir | *"Isi data pasien atau nama, NIK, dan tanggal lahir peserta."* | `400` | `MCU-DEC-033` |
| `VAL-MCU-17` | NIK 16 digit angka | Booking, impor | Format salah | *"NIK harus 16 digit angka."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-18` | Tidak ada order ganda | Booking, impor | Peserta (pasien atau NIK) sudah punya order *Terjadwal*/*Hadir*/*Dalam Pemeriksaan* pada tanggal yang sama | *"Peserta ini sudah terjadwal MCU pada 27-10-2026 (MCU-RSMMC-000123)."* | `409` | `MCU-DEC-041` |
| `VAL-MCU-19` | Rekomendasi wajib tanggal & kunjungan asal | Rekomendasi | Tanpa tanggal; kunjungan asal bukan milik pasien | *"Tanggal pemeriksaan MCU wajib diisi."* | `400` | `MCU-DEC-037` |
| `VAL-MCU-20` | Paket aktif | Booking, rekomendasi, impor, *walk-in* | Paket nonaktif | *"Paket MCU sudah tidak aktif."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-21` | Peserta perusahaan wajib perusahaan aktif | Booking, impor | Perusahaan nonaktif atau kontrak berakhir sebelum tanggal pemeriksaan | *"Kerja sama PT Contoh berakhir 31-12-2026."* | `400` | `MCU-DEC-004` |
| `VAL-MCU-22` | Impor diproses per baris | Impor | Baris gagal tidak menggagalkan baris lain; kuota dicek per baris | Hasil: *"38 peserta terjadwal, 2 gagal: baris 7 NIK tidak valid; baris 15 kuota penuh."* | `200` | `MCU-DEC-041` |
| `VAL-MCU-23` | Batal/jadwal ulang beralasan | Batal | Alasan kosong | *"Alasan pembatalan wajib diisi."* | `400` | `MCU-DEC-038` |

## 4. Check-in

| ID | Aturan | Berlaku pada | Kondisi | Pesan | Kode | Asal |
| --- | --- | --- | --- | --- | --- | --- |
| `VAL-MCU-24` | Check-in hanya pada tanggal pemeriksaan | Check-in | Tanggal order ≠ hari ini (WIB) | *"Order ini dijadwalkan 28-10-2026, bukan hari ini."* | `409` | `MCU-DEC-041` (turunan `MCU-DEC-014`) |
| `VAL-MCU-25` | Pencocokan NIK + tanggal lahir | Check-in | NIK sama, tanggal lahir beda, tanpa konfirmasi petugas | *"NIK cocok dengan pasien RM 00012345 tetapi tanggal lahir berbeda. Periksa KTP lalu konfirmasi."* | `409` | `MCU-DEC-039` |
| `VAL-MCU-26` | Dokter MCU wajib | Check-in | Tanpa dokter MCU | *"Pilih dokter MCU pemeriksa."* | `400` | `MCU-DEC-032` |
| `VAL-MCU-27` | Semua persyaratan ditandai | Check-in | Ada persyaratan wajib tanpa hasil penandaan | *"Tandai persyaratan: Puasa 10–12 jam (Gula Darah Puasa)."* | `400` | `MCU-DEC-013` |
| `VAL-MCU-28` | Catatan wajib bila lanjut dengan catatan | Check-in | `ProceedWithNote` tanpa catatan | *"Tulis catatan untuk persyaratan yang tidak terpenuhi."* | `400` | `MCU-DEC-013` |
| `VAL-MCU-29` | Tarif berlaku tersedia | Check-in | Tarif umum/perusahaan tidak aktif pada tanggal check-in | *"Tarif paket tidak aktif. Hubungi admin MCU."* | `409` | `MCU-DEC-020`, `025` |
| `VAL-MCU-30` | Satu kunjungan per order | Check-in | Order sudah punya kunjungan (kirim ulang) | Mengembalikan hasil check-in yang sama, tanpa kunjungan/baris kedua | `200` | `MCU-DEC-041` (idempotensi) |
| `VAL-MCU-47` | Check-in/*walk-in* kiosk butuh dokter jaga | Check-in kiosk, *walk-in* kiosk | Jadwal tanggal itu tanpa `DutyMcuDoctorId` | *"Dokter MCU hari ini belum ditetapkan. Silakan menuju admisi MCU."* | `409` | `MCU-DEC-042` |

## 5. Pemeriksaan, resume, penyerahan

| ID | Aturan | Berlaku pada | Kondisi | Pesan | Kode | Asal |
| --- | --- | --- | --- | --- | --- | --- |
| `VAL-MCU-31` | Ringkasan untuk perusahaan; lengkap butuh persetujuan | Penyerahan | Penerima perusahaan + isi lengkap tanpa persetujuan berlaku | *"Peserta belum menyetujui pelepasan hasil lengkap kepada PT Contoh. Serahkan ringkasan kelayakan."* | `409` | `MCU-DEC-010` |
| `VAL-MCU-32` | Penyerahan hanya versi terverifikasi | Penyerahan | Versi berlaku belum *Verified* | *"Hasil belum diverifikasi."* | `409` | `MCU-DEC-008` |
| `VAL-MCU-33` | Tujuan wajib untuk WhatsApp/email/telepon | Penyerahan | Tujuan kosong; format email salah | *"Isi nomor WhatsApp tujuan."* | `400` | `MCU-DEC-027` |
| `VAL-MCU-34` | Mulai pemeriksaan butuh lunas/terjamin | Mulai pemeriksaan | Perorangan belum lunas | *"Tagihan paket belum lunas (sisa Rp 350.000)."* | `409` | `MCU-DEC-004`, `024` |
| `VAL-MCU-35` | Approve butuh semua komponen tuntas | Approve | Ada komponen belum `Completed`/`Cancelled` | *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* | `409` | `MCU-DEC-009`, `036` |
| `VAL-MCU-36` | Approve butuh skrining selesai | Approve, isi Pemeriksaan MCU | Antrean skrining kunjungan belum selesai | *"Skrining awal peserta belum selesai."* | `409` | `MCU-DEC-018` |
| `VAL-MCU-37` | Approve butuh isi minimum | Approve | Kategori kelayakan, kondisi umum, atau rekomendasi kosong | *"Lengkapi kategori kelayakan sebelum menyetujui."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-38` | Tindakan dokter hanya dokter yang ditetapkan | Approve, buka ulang, batalkan/tambah komponen, simpan resume | Akun tidak tertaut ke `McuDoctorId` | *"Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini."* | `403` | `MCU-DEC-032` |
| `VAL-MCU-39` | Pemverifikasi bukan pemberi approve | Verifikasi | Akun = dokter pemberi approve | *"Verifikasi tidak boleh dilakukan oleh dokter yang menyetujui resume."* | `403` | `MCU-DEC-008` |
| `VAL-MCU-40` | Alasan wajib | Kembalikan, buka ulang, batalkan komponen, ganti dokter | Alasan kosong | *"Alasan wajib diisi."* | `400` | `MCU-DEC-011`, `015`, `040` |
| `VAL-MCU-41` | Batal penuh hanya bila belum ada komponen dimulai | Batal penuh | Ada komponen dimulai (definisi `state-transition-matrix.md` 2.3) | *"Sudah ada pemeriksaan yang dimulai. Gunakan pembatalan per komponen."* | `409` | `MCU-DEC-021` |
| `VAL-MCU-42` | Isian wajib terisi saat komponen diselesaikan | Simpan hasil `complete=true` | Isian `IsRequired` kosong | *"Isi Tekanan darah sistolik."* | `400` | `MCU-DEC-041` |
| `VAL-MCU-43` | Angka berformat desimal titik | Simpan hasil | `FieldType = Number` bukan angka | *"Nilai harus berupa angka, contoh 6.4."* | `400` | `MCU-DEC-041` (pelajaran `BaseTextField` membuang desimal) |
| `VAL-MCU-44` | Isian terkunci sesudah approve | Simpan hasil | Resume berlaku `Approved`/`Verified` | *"Hasil terkunci. Minta dokter membuka ulang resume."* | `409` | `MCU-DEC-011` |
| `VAL-MCU-45` | Ganti dokter hanya kepala unit, beralasan | Ganti dokter | Tanpa `ReassignDoctor`; alasan kosong; order sudah *Diserahkan*/*Dibatalkan* | *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."* | `403`/`400`/`409` | `MCU-DEC-040` |
| `VAL-MCU-46` | Konkurensi order | Semua tindakan ubah | `Version` request ≠ `Version` data | *"Data order sudah diubah petugas lain. Muat ulang lalu coba lagi."* | `409` | `MCU-DEC-041` |
