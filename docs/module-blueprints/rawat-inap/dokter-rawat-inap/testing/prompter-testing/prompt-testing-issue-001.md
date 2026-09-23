# Prompt Pengujian — Verifikasi Perbaikan `ISSUE-DOK-001`

Dokumen ini adalah **instruksi untuk agent AI penguji**. Salin bagian
[Prompt](#prompt-yang-disalin) apa adanya ke agent penguji. Bagian lain adalah konteks bagi manusia
yang menugaskannya.

| Hal | Isi |
| --- | --- |
| Tujuan | Memverifikasi perbaikan `BE-RWI-127` (backend) dan `FE-RWI-095` (frontend) |
| Sumber masalah | `roadmap/issues/issue-001-perbaikan-hasil-testing-dokter-rawat-inap.md` |
| Status sebelum diuji | `dotnet build` PASS 23 September 2026; **verifikasi runtime belum pernah dijalankan** |
| Yang menugaskan | Muhammad Hamzah |

---

## Prompt yang disalin

> Kamu adalah agent penguji untuk sistem rumah sakit Quilvian. Tugasmu **hanya menguji dan
> melaporkan** — kamu **tidak boleh mengubah satu baris source pun**, baik backend maupun frontend.
> Kalau kamu menemukan cacat, laporkan; jangan perbaiki.
>
> ### Konteks
>
> Pada 22 September 2026 pengujian menemukan fitur resep dokter rawat inap tidak dapat dipakai.
> Perbaikannya sudah dikerjakan pada 23 September 2026 lewat dua task: `BE-RWI-127` (backend) dan
> `FE-RWI-095` (frontend). Backend sudah di-build ulang dan hijau. **Belum ada satu pun verifikasi
> runtime.** Itu tugasmu sekarang.
>
> Baca lebih dulu, berurutan:
>
> 1. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/issues/issue-001-perbaikan-hasil-testing-dokter-rawat-inap.md`
> 2. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/backend/BE-RWI-127.md`
> 3. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/frontend/FE-RWI-095.md`
>
> ### Lingkungan
>
> | Hal | Nilai |
> | --- | --- |
> | Frontend | `http://localhost:3000` |
> | Backend | `https://localhost:7184` |
> | Database | PostgreSQL `QuilvianNewDevHamzah` |
> | Akun dokter | `rendi@admin.com` (dr. Rendy Pangalila, DPJP) |
> | Kata sandi | Pakai kata sandi akun uji yang sudah dipakai tim. **Jangan menuliskannya ke dalam laporan.** |
>
> **Locale server wajib `id-ID`.** Seluruh bug kelompok pertama hanya muncul pada locale itu. Kalau
> mesin yang kamu pakai berbahasa Inggris, sebutkan itu di laporan — hasil "lulus" pada locale yang
> salah tidak membuktikan apa pun.
>
> Tentukan sendiri pasien dan episode rawat inap yang aktif dari data yang ada, lalu **catat apa
> adanya** nomor rekam medis, nomor episode, ruang, bed, dan penjaminnya. Jangan menyalin konteks
> pasien dari laporan pengujian lama — sebagian di antaranya terbukti keliru.
>
> ---
>
> ### Bagian A — Perbaikan yang harus dibuktikan
>
> #### A1. `ISS-01` — penyimpanan obat tidak lagi galat server
>
> Sebelumnya setiap penyimpanan butir resep dijawab `HTTP 500` karena batas pecahan `"0.0001"` pada
> atribut validasi gagal dibaca di locale Indonesia.
>
> Uji:
>
> 1. `POST /api/v1/health-services/pharmacy-management/prescription-items` dengan payload sah.
> 2. `PATCH /api/v1/health-services/pharmacy-management/prescription-workspaces/{id}/autosave`.
>
> Lulus bila: keduanya menjawab 2xx, **dan** log backend tidak lagi memuat `FormatException` maupun
> `RangeAttribute.SetupConversion`. Periksa berkas log-nya, jangan hanya melihat respons.
>
> Karena perbaikannya mengunci culture seluruh aplikasi, periksa juga **satu** endpoint di luar
> Farmasi yang mengembalikan angka atau tanggal berformat — misalnya Billing atau Master Data Tarif —
> dan pastikan keluarannya tidak berubah bentuk. Laporkan apa yang kamu periksa.
>
> #### A2. `ISS-03` — isi resep benar-benar tersimpan
>
> Sebelumnya `items` dan `compounds` dibuang diam-diam, sehingga dokter melihat "berhasil" untuk
> resep yang isinya kosong.
>
> Uji:
>
> 1. Buat resep lewat `POST /prescriptions` dengan **2 obat** dan **1 racikan berisi 2 bahan**.
> 2. Periksa responsnya, lalu periksa langsung ke database.
>
> Lulus bila: `TotalItemCount` sesuai jumlah yang dikirim, dan baris obat serta racikannya benar-benar
> ada di `PhmPrescriptionItem`, `PhmPrescriptionCompound`, dan `PhmPrescriptionCompoundItem`.
>
> **Uji juga sifat satu transaksinya.** Kirim satu permintaan yang isinya sengaja salah — misalnya
> `drugId` yang tidak ada. Lulus bila **tidak ada** kepala resep yang tertinggal di database. Kepala
> resep yang terbit tanpa isinya adalah kegagalan, sekalipun responsnya terlihat wajar.
>
> #### A3. `ISS-04` — jenis resep tersimpan sesuai pilihan dokter
>
> Sebelumnya frontend mengirim `orderType`, sedangkan kontrak menamainya `prescriptionOrderType`,
> sehingga jenis resep selalu jatuh ke `Routine`.
>
> Uji lewat **antarmuka**, bukan hanya API: buat resep bertipe **obat pulang** dari layar dokter, lalu
> periksa nilai `PrescriptionOrderType` yang tersimpan di database. Lulus bila nilainya obat pulang,
> bukan `Routine`.
>
> Lalu periksa apakah layar Farmasi dapat menyaring resep obat pulang dan hasilnya cocok.
>
> #### A4. `ISS-02` — dokter dapat mencari dan memilih obat
>
> Uji lewat antarmuka: buka tab Resep → "Cari dan Tambah Obat" → ketik nama obat yang ada di
> formularium.
>
> Lulus bila: daftar obat muncul, **dan** tidak ada respons `400` pada
> `GET /api/v1/health-services/clinical-management/prescribing-drugs` di log jaringan. Rekam log
> jaringannya sebagai bukti.
>
> #### A5. `ISS-06` — status code create seragam
>
> Periksa **HTTP status** ketiga endpoint ini:
>
> | Endpoint | Diharapkan |
> | --- | --- |
> | `POST /pharmacy-management/prescriptions` | `201` |
> | `POST /radiology-management/rad-orders` | `201` |
> | `POST /clinical-management/patient-assessments` | `201` |
>
> Periksa juga bahwa pengiriman ulang `POST /prescriptions` dengan `idempotencyKey` yang sama
> menjawab `200`, bukan `201`, dan **tidak** melahirkan resep kedua.
>
> > **Jangan salah lapor di sini.** Pada sistem ini badan jawaban selalu memuat `"statusCode": 200`
> > walaupun HTTP status-nya `201`. Itu sifat pembungkus `ApiResponse` yang sudah ada sejak lama dan
> > berlaku untuk seluruh endpoint `201`, termasuk `physician-visits`. **Itu bukan cacat, dan bukan
> > bagian dari pengujian ini.** Yang kamu periksa adalah HTTP status-nya, bukan angka di dalam badan.
>
> ---
>
> ### Bagian B — Regresi yang harus tetap hijau
>
> Perbaikan `ISS-01` mengubah setelan culture **seluruh aplikasi**, dan `ISS-06` mengubah status code
> tiga endpoint. Pastikan enam alur yang 22 September lalu dinyatakan lulus tetap bekerja:
>
> 1. Visite dokter — catat dan batalkan beralasan
> 2. Tindakan medis — pesan, tarif terhitung, batalkan beralasan
> 3. Resume medis — keputusan pulang, prefill, simpan draf, tanda tangan
> 4. Penunjang medis — pesan laboratorium dan radiologi
> 5. CPPT — buat catatan langsung
> 6. Kajian pasien — buat dan selesaikan
>
> Cukup satu putaran per alur. Yang dicari adalah kerusakan baru, bukan pengujian ulang menyeluruh.
>
> ---
>
> ### Bagian C — Tiga anomali yang harus direproduksi
>
> Ketiganya terbaca dari laporan pengujian lama yang justru berstatus "BERHASIL 100%", tetapi belum
> pernah ditelusuri. **Ini bukan bug yang sudah dipastikan** — tugasmu memastikan ada atau tidaknya.
>
> **C1.** Buat CPPT lewat `POST /patient-integrated-progress-notes/from-consultation/{consultationId}`.
> Laporan lama mencatat dokumen terbit dengan `200 OK`, tetapi kartunya tidak pernah muncul di lini
> masa dan counter tetap `1 catatan` padahal dua CPPT dibuat. Pastikan: apakah baris yang ditulis
> endpoint itu benar-benar terbaca oleh `GET /patient-integrated-progress-notes/episodes/{episodeId}`,
> atau ada penyaringan yang menyembunyikannya?
>
> **C2.** Buat kajian medis, lalu lihat tabel Riwayat Kajian Medis. Laporan lama menunjukkan kolom
> **Dokter / Penulis** berisi `-`. Pastikan: apakah backend tidak mengirim nama penulis, atau frontend
> tidak memetakannya? Untuk dokumen rekam medis, penulis yang kosong adalah cacat medikolegal.
>
> **C3.** Buka satu episode rawat inap yang sama dari tab Visite, Tindakan, dan Penunjang. Laporan
> lama mencatat ruang dan kelas yang berbeda-beda untuk episode yang identik — salah satunya
> menyebut kelas `UNIQUE`. Pastikan: apakah konteks pasien yang ditampilkan konsisten antar tab?
>
> ---
>
> ### Aturan pelaporan — baca ini sebelum menulis
>
> Laporan pengujian sebelumnya punya cacat yang tidak boleh terulang. Empat aturan berikut mengikat:
>
> 1. **Jangan menulis "BERHASIL 100%" bila ada satu pun pengamatan yang belum kamu jelaskan.** Laporan
>    lama menyatakan CPPT "berfungsi 100% sempurna" sambil memuat counter yang tidak cocok dengan
>    jumlah dokumen yang dibuat. Bila ada yang janggal, itu temuan — bukan detail yang boleh dilewati.
> 2. **Tempel respons JSON yang benar-benar kamu terima.** Jangan menyusun ulang dari ingatan atau
>    dari dokumen kontrak. Laporan lama mencantumkan `"statusCode": 201` pada badan jawaban
>    `physician-visits`, padahal keluaran sebenarnya `200` — angka itu tidak pernah diterima, hanya
>    disimpulkan.
> 3. **Catat konteks pasien dari layar yang kamu buka**, bukan dari laporan lama. Bedakan `userId` dan
>    `doctorId`; keduanya berbeda dan pernah tertukar.
> 4. **Yang tidak sempat diuji ditulis `NOT RUN` apa adanya**, beserta alasannya. Itu jawaban yang
>    sah. Menyimpulkan lulus dari perintah yang tidak dijalankan tidak.
>
> ### Bentuk dan lokasi laporan
>
> Tulis **satu** laporan berbahasa Indonesia ke:
>
> ```text
> NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/laporan-verifikasi-issue-001.md
> ```
>
> Isinya:
>
> 1. **Metadata** — tanggal, lingkungan, locale server, akun, pasien dan episode uji, commit yang diuji.
> 2. **Ringkasan hasil** — tabel `ISS-01` s.d. `ISS-06`, `PASS` / `FAIL` / `NOT RUN`, satu baris bukti tiap butir.
> 3. **Rincian per butir** — langkah, permintaan, respons sebenarnya, bukti database bila relevan.
> 4. **Hasil regresi** — enam alur Bagian B.
> 5. **Hasil tiga anomali** — Bagian C, masing-masing: terbukti / tidak terbukti / belum dapat dipastikan, beserta buktinya.
> 6. **Cacat baru yang ditemukan** — bila ada, lengkap dengan langkah reproduksi. Jangan diperbaiki.
> 7. **Yang tidak dijalankan** — beserta alasannya.
>
> Simpan tangkapan layar dan log jaringan di repository frontend pada folder uji yang sudah diabaikan
> Git, lalu tautkan dari laporan.
>
> Jangan melakukan `git add`, `commit`, `push`, atau `merge`. Jangan menyentuh source aplikasi.

---

## Catatan untuk yang menugaskan

- Prompt ini **tidak** meminta penguji memperbaiki apa pun. Kalau ada cacat baru, perbaikannya
  dialokasikan sebagai task terpisah.
- `ISS-05` sengaja **tidak** ada dalam daftar uji. Butir itu tidak dikerjakan karena kedua berkas
  service ternyata bukan duplikat, dan menyatukannya menunggu keputusan Anda — rinciannya di bagian 7
  laporan `FE-RWI-095`.
- Sesudah laporan penguji masuk dan hasilnya hijau, status `BE-RWI-127` dan `FE-RWI-095` pada roadmap
  dinaikkan dari 🟡 ke ✅.
