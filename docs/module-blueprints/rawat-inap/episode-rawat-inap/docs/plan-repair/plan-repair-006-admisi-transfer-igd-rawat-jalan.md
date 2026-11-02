# PLAN-REPAIR-006 — Menuntaskan pendaftaran ranap dari transfer IGD dan rawat jalan

```yaml
plan_id: PLAN-REPAIR-EPS-006
issue: ../issue/issue-006-admisi-transfer-igd-rawat-jalan.md
status_rencana: IMPLEMENTASI_SELESAI_VERIFIKASI_RUNTIME_SEBAGIAN
tanggal_rencana: "2026-10-12"
diputuskan_oleh: "Pengguna, instruksi implementasi seluruh rencana pada sesi ini"
tanggal_keputusan: "2026-10-12"
basis_source_backend: "b3957e57707b8ee0cd96691454f3b830c1e1c283 (MHamzah)"
basis_source_frontend: "eb04e206f6481bd94f90c3afad8a8a7d7e2e8387 (HamzahV2), dengan perubahan working tree pekerjaan lain"
blueprint_id: RWI-BP-001
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| FIX-EPS-006-01 | ISS-EPS-006-T4 | Selaraskan route, titik tulis dan referensi keputusan | Dokumen/kontrak | 1 | FIX-EPS-006-01 | ✅ SELESAI - kontrak/dokumen | [Laporan](../../task/report/backend/FIX-EPS-006-01.md) |
| FIX-EPS-006-02 | ISS-EPS-006-03 | Terapkan penjamin dan deposit melalui pemiliknya | BE/Registrasi/Billing | 1 | FIX-EPS-006-02 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/backend/FIX-EPS-006-02.md) |
| FIX-EPS-006-03 | ISS-EPS-006-02 | Lengkapi pembaca/penulis sumber rawat jalan | BE/Clinical | 1 | FIX-EPS-006-03 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/backend/FIX-EPS-006-03.md) |
| FIX-EPS-006-04 | ISS-EPS-006-01 | Hubungkan satu jalur submit transfer, tanpa membuat episode dua kali | FE | 1 | FIX-EPS-006-04 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/frontend/FIX-EPS-006-04.md) |
| FIX-EPS-006-05 | ISS-EPS-006-T1 | Pulihkan rujukan dan isi awal dokter/unit dari detail server | FE | 2 | FIX-EPS-006-05 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/frontend/FIX-EPS-006-05.md) |
| FIX-EPS-006-06 | ISS-EPS-006-T3; tampilan ISS-EPS-006-01 | Betulkan asal, status, paginasi, jumlah, dan keadaan hak akses | FE | 2 | FIX-EPS-006-06 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/frontend/FIX-EPS-006-06.md) |
| FIX-EPS-006-07 | ISS-EPS-006-T2 | Simpan relasi kunjungan asal melalui pemilik Registrasi | BE/Registrasi | 1 | FIX-EPS-006-07 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/backend/FIX-EPS-006-07.md) |
| FIX-EPS-006-08 | ISS-EPS-006-T5; seluruh temuan | Buktikan alur A–D dan kegagalannya secara menyeluruh | Verifikasi | 1 | FIX-EPS-006-08 | 🟡 SEBAGIAN - source selesai; verifikasi runtime belum lengkap | [Laporan](../../task/report/frontend/FIX-EPS-006-08.md) |

**Ringkasan 12 Oktober 2026:** seluruh perubahan source yang direncanakan sudah diterapkan; FIX-01 selesai dan FIX-02 sampai FIX-08 dicatat sebagian sampai bukti runtime relevan terpenuhi. Backend MHamzah, frontend HamzahV2. [Kontrak implementasi yang disetujui](../../contracts/admission-transfer-repair-006.md). Migration disiapkan tanpa database update. Perubahan UI user sebelumnya dipertahankan.

API pada `MHamzah` sudah ada; rencana ini tidak meminta pembangunan ulang API antrean maupun coordinator transaksi/idempotensi. Daftar bukti E01–E18 ada pada [issue pasangan](../issue/issue-006-admisi-transfer-igd-rawat-jalan.md#daftar-bukti-source).

## 2. Solusi terpilih per temuan

| Perbaikan | Opsi yang dipertimbangkan | Solusi terpilih dan alasan |
| --- | --- | --- |
| FIX-01 | Memakai label roadmap lama; atau menyelaraskan route dan trace dengan source/decision current | Selaraskan acuan lebih dahulu. `/admission-transfers/admit` terbukti ada; `/episodes/open-from-transfer` pada roadmap bukan route source current. Jangan menyalin ID keputusan Bed Management menjadi persetujuan admisi |
| FIX-02 | Mengisi teks skema pembayaran saja; atau memakai service Registrasi/Billing | Pakai pemilik Registrasi/Billing dan kontrak identitas penjamin/receipt yang sah. Teks “BPJS” tidak boleh berakhir sebagai kunjungan Cash secara diam-diam |
| FIX-03 | Memperlakukan semua sumber sebagai disposisi IGD; atau membedakan sumber dengan reuse dokumen pemilik | Pembaca dan penulis mengikuti `SourceType`. Kandidat rawat jalan ialah surat pengantar ranap existing; keputusan K-02 menentukan kelayakan/pemakaian ulang |
| FIX-04 | Memanggil transfer sesudah alur umum selesai; atau menyusun jalur transfer dengan satu penciptaan episode | Jalur transfer memakai orchestrator transfer existing. Jangan menambah panggilan di belakang alur yang sudah membuat episode dan reservasi. K-01 mengunci tahap tulis/consent yang final |
| FIX-05 | Menahan seluruh detail hanya pada state React; atau menyimpan referensi lalu membaca detail server | Simpan referensi transfer dan jenis sumber, lalu muat ulang detail server. Identitas pasien dan status sumber diverifikasi kembali; data sensitif tidak dimasukkan ke URL |
| FIX-06 | Mengganti style saja; atau memperbaiki data/keadaan pada komponen existing | Reuse komponen yang sedang distandardisasi, perbaiki perilakunya: asal dari server, status nyata, pencarian/paginasi server dan error yang jelas |
| FIX-07 | Menuliskan nomor asal di Notes; atau menyimpan relasi terstruktur | Relasi terstruktur melalui Registrasi. Notes tetap untuk catatan petugas, bukan pengganti identitas sumber; schema final mengikuti K-03 |
| FIX-08 | Menganggap unit test struktur cukup; atau menguji perjalanan pengguna serta transaksi | Uji perjalanan A–D, kegagalan, hak akses, dan replay. Coordinator yang sudah ada dipertahankan dan dibuktikan, bukan diganti |

Pemilihan K-01 sampai K-03 disetujui melalui instruksi implementasi pengguna pada sesi ini. Kontrak EPS-006 mencatat batasnya; tidak menambahkan kebijakan klinis atau cara bayar baru.

## 3. Skema tampilan sebelum → sesudah

Rangka SEBELUM berasal dari source, bukan screenshot runtime. Susunan SESUDAH adalah usulan yang perlu dicocokkan dengan hasil FIX-01. Batas waktu deposit dan persetujuan mengikuti kontrak yang diselaraskan; diagram ini tidak menetapkan status klinis baru.

### Sebelum

```text
Admisi Rawat Inap
[ Transfer IGD / Poliklinik ]

Antrean: halaman pertama, filter lokal, asal selalu IGD
[ Proses Admisi ]
  -> Review pasien; dokter/diagnosis terlihat
  -> Pembayaran / Deposit
  -> Dokter: buat kunjungan + episode lewat alur umum
  -> Pilih Bed / Booking Bed terpisah
  -> Konfirmasi: simpan perubahan episode
     "Disposisi IGD akan otomatis menjadi Executed"
  -> Form Persetujuan / Cetak

Helper POST transfer belum dipanggil oleh UI.
Refresh: referensi transfer tidak dipulihkan.
```

### Sesudah

```text
Admisi Rawat Inap — Transfer
Asal [ Semua ▾ ]  Cari [ Nama / RM / No. kunjungan ]
Pasien     Asal       Permintaan       Menunggu     Aksi
Tn. Budi   IGD        Rawat inap        12 menit    [ Proses Admisi ]
Ny. Sari   Poli       Surat pengantar   25 menit    [ Proses Admisi ]
[ Sebelumnya ]  Halaman 1 dari ...  [ Berikutnya ]

Review rujukan yang diverifikasi server
Pasien / sumber / dokter / tujuan / indikasi
  -> Lengkapi penjamin, deposit yang berlaku, dokter dan bed
  -> SATU jalur penyimpanan transfer sesuai titik tulis FIX-01
     API existing + versi bed + intent operasi yang tetap
  -> Persetujuan dan cetak memakai episode hasil server
  -> Hasil pendaftaran dan status sumber dari server
     Episode: ...   Kamar/bed dipesan: ...   Dokumen: ...

Refresh: detail rujukan/hasil operasi dimuat kembali.
Pendaftaran selesai dan pasien tiba di bangsal tampil sesuai fakta masing-masing.
```

| Wilayah | Isi | Sumber data | Komponen/pola yang dipakai ulang |
| --- | --- | --- | --- |
| Antrean | Pasien, asal, nomor permintaan, waktu tunggu | API antrean, termasuk metadata paginasi | Tabel transfer existing, `StatusBadge`, komponen filter/paginasi repo |
| Review | Identitas dan rujukan yang dipilih | API detail berdasarkan referensi transfer | Transfer review existing, `InformationAlert` |
| Data administrasi | Penjamin, kelas, DPJP, unit, deposit yang berlaku | Form existing dan service pemilik | Langkah pembayaran, dokter, deposit existing |
| Bed | Pilihan bed dan versi terakhir | Pembaca bed existing; server memeriksa lagi saat submit | Komposisi pemilih bed existing; sesuaikan aktivasi tanpa episode untuk jalur ini bila K-01 memilihnya |
| Hasil | Episode, reservasi, sumber, status dokumen | Hasil submit dan pembacaan server | Konfirmasi existing, ringkasan, cetak/consent existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Proses Admisi | Utama | Memiliki hak; rujukan masih layak | Muat detail, ikat sumber dan pasien |
| Lanjut | Utama | Tidak ada request berlangsung | Validasi isian; bila kurang, tampilkan sebab dan arahkan ke field |
| Simpan/Selesaikan sesuai tahap | Utama | Berhak; tidak ada request berlangsung | Jalankan satu intent transfer; sukses ditampilkan setelah respons terverifikasi |
| Muat ulang / Coba lagi | Sekunder | Keadaan gagal atau data berubah | Baca ulang; timeout mutasi memeriksa hasil sebelum mengulang maksud yang sama |
| Cetak | Sekunder | Dokumen/episode yang diperlukan tersedia | Cetak berkas pasien/episode hasil server yang sama |

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Tampilkan pemuatan wilayah terkait; jangan tampilkan angka nol sebagai hasil |
| Kosong | Nyatakan tidak ada permintaan yang cocok pada filter yang berlaku |
| Gagal | Tampilkan sebab yang dapat dipahami dan aksi coba lagi; pertahankan isian |
| Tidak berhak | Jangan memanggil pembaca tanpa izin; jelaskan akses ditolak saat URL dibuka langsung |
| Rujukan berubah/dibatalkan | Hentikan submit dan muat status terbaru; jangan membuat episode |
| Bed berubah/dipegang pihak lain | Tampilkan konflik, baca ulang bed; jangan menaikkan expected version sendiri |
| Timeout | Hasil belum diketahui; periksa own outcome/replay dengan key yang sama sebelum menyatakan berhasil atau gagal |
| Sukses | Gunakan episode/reservasi/status sumber hasil server; reservasi bukan bukti pasien sudah tiba atau bed sudah occupied |

> **Revisi 27-10-2026 (`PLAN-REPAIR-EPS-041`).** Skema langkah Verifikasi transfer kini mengacu ke
> [plan-repair-041 skema 3.1](plan-repair-041-temuan-uji-admisi-lama-bayi-transfer.md#31-verifikasi-pasien-jalur-transfer--fix-eps-041-02-fix-eps-041-06):
> kartu rujukan memisahkan DPJP usulan dari "Diputuskan oleh", menampilkan chip Kebutuhan, Kelas Diminta, Diagnosa, dan
> label "Alasan Rawat Inap", serta pemeriksaan kelayakan pasien sebelum "Lanjut ke Pembayaran".

## 4. Rincian perbaikan

### FIX-EPS-006-01 — Selaraskan kontrak dan keterlacakan

**Menutup:** ISS-EPS-006-T4. **Area:** dokumen. **Jenis perubahan:** amandemen blueprint/trace melalui jalur berwenang. **Berkas sasaran:** roadmap admisi transfer BE/FE, requirement-traceability terkait, bagian admisi pada arsitektur/skema, API/state/integration/validation/permission contract. **Radius dampak:** admisi IGD/poli dan konsumsi operasi bed. **Dependency:** tidak ada untuk analisis penyelarasan; keputusan K-01–K-03 untuk penguncian hasil.

1. Cocokkan referensi keputusan/AC lama terhadap decision log current; jangan menambah atau mengambil nomor task/keputusan pada skill ini.
2. Nyatakan route current `/admission-transfers/admit`, HTTP 201, response episode dengan `OperationMeta`, versi bed dan header idempotensi.
3. Tentukan kapan episode/reservasi dibentuk, kapan consent disimpan, serta arti status sumber dan “pendaftaran selesai”. Pertahankan perbedaan dari kepergian/tiba fisik.

**Acceptance criteria:** (1) satu route submit yang sama dipakai source, kontrak dan consumer; (2) referensi keputusan admisi tidak menunjuk keputusan kategori kelas Bed Management; (3) contoh satu pasien dari pilihan rujukan sampai selesai menyebut setiap titik tulis tanpa menciptakan episode dua kali; (4) approval lama dicatat sesuai scope, dan konflik yang belum diputuskan tetap terlihat.

**Verifikasi:** review dokumen terhadap E01–E18 dan pencarian route/ID secara terbatas. **Risiko:** menganggap dokumen lama sebagai otorisasi perubahan lifecycle; kendalikan dengan catatan keputusan pemilik yang eksplisit.

### FIX-EPS-006-02 — Terapkan penjamin dan deposit pada transaksi transfer

**Menutup:** ISS-EPS-006-03. **Area:** BE/Registrasi/Billing. **Jenis:** source/kontrak. **Berkas sasaran:** DTO dan core `InpAdmissionTransferService`; service Registrasi/Billing pemilik sesuai kontrak K-03. **Radius dampak:** kunjungan ranap hasil transfer, penjamin dan receipt deposit; builder tidak mengubah semua alur Cash. **Dependency:** FIX-01/K-03.

1. Bentuk request pembayaran dengan identitas penjamin yang benar, bukan hanya nama skema.
2. Terapkan penjamin melalui pemilik Registrasi dan proses deposit melalui Billing; definisikan pemulihan bila salah satu tahap gagal.
3. Tolak field/skema yang belum didukung dengan pesan jelas; jangan menerima lalu mengabaikannya. Pertahankan idempotensi masing-masing transaksi keuangan.

**Acceptance criteria:** (1) fixture asuransi mempertahankan penjamin yang dipilih dan tidak diam-diam menjadi Cash; (2) pasien umum tetap mengikuti kebijakan deposit existing; (3) contoh deposit Rp500.000 menghasilkan satu receipt yang dapat ditelusuri ketika diwajibkan dan tidak mengganda setelah replay; (4) kegagalan penjamin/deposit tidak tampil sebagai pendaftaran selesai, dengan hasil/pemulihan yang didefinisikan kontrak.

**Verifikasi:** uji integrasi terbatas pada database uji yang diotorisasi; baca ulang konteks pembayaran dan receipt lewat API pemilik. **Risiko:** penagihan salah atau receipt ganda; wajib diuji sebelum UI memakai API transfer untuk pembayaran tersebut. Schema/migration hanya disiapkan oleh task yang diberi wewenang, tidak dijalankan oleh rencana ini.

### FIX-EPS-006-03 — Lengkapi sumber rawat jalan dan validasi sumber

**Menutup:** ISS-EPS-006-02. **Area:** BE/Clinical. **Jenis:** source/kontrak. **Berkas sasaran:** `InpAdmissionTransferService.cs`, DTO/controller terkait dan service baca surat dokter pemilik. **Radius dampak:** pembacaan dan eksekusi IGD/poli; rujukan kamar pulih tetap mempunyai kontraknya sendiri. **Dependency:** FIX-01/K-02.

1. Terapkan `SourceType` pada daftar, detail dan eksekusi. Sumber tidak dikenal ditolak, bukan diperlakukan sebagai IGD.
2. Reuse surat pengantar ranap existing untuk sumber poli bila K-02 menyetujuinya; tetapkan identitas sumber, status layak, pembatalan dan penghubung hasil episode.
3. Validasi bahwa pasien, jenis permintaan, status sumber dan unit tujuan benar saat submit; daftar dan penulis tidak memakai aturan yang berbeda.

**Acceptance criteria:** (1) filter `Emergency` dan `Outpatient` mengembalikan asal yang benar; (2) satu surat poli layak milik Ny. Sari dapat diproses tanpa ID disposisi IGD fiktif; (3) surat dibatalkan atau pasien tidak cocok ditolak tanpa episode/reservasi baru; (4) request berjenis tidak dikenal ditolak; (5) pasien IGD yang sah tetap dapat diproses dan request yang tidak memenuhi jenis ranap tidak lolos hanya karena mempunyai unit tujuan.

**Verifikasi:** uji positif/negatif tiap sumber, pembatalan di antara pembacaan dan submit, dan pemeriksaan hasil antrean. **Risiko:** duplikasi konsep permintaan atau pemakaian ulang surat; gunakan owner service dan kontrak status yang disahkan.

### FIX-EPS-006-04 — Hubungkan jalur submit transfer secara utuh

**Menutup:** ISS-EPS-006-01. **Area:** FE. **Jenis:** source/komposisi langkah. **Berkas sasaran:** `inpatient-admission-view.jsx`, hook flow/dokter/bed/konfirmasi yang relevan, service transfer existing. **Radius dampak:** hanya cabang transfer; pasien baru/lama/kamar pulih menjadi pemeriksaan regresi. **Dependency:** FIX-01/K-01/FIX-02; jalur poli juga FIX-03.

1. Ikat rujukan, jenis sumber, pasien, form administrasi dan bed ke satu jalur transfer.
2. Sesuaikan pilihan bed/titik tulis: hook saat ini tidak aktif tanpa episode. Jangan membuat episode umum semata-mata untuk membuka selector lalu memanggil endpoint pencipta episode kedua. Pembaca bed backend existing mendukung `EpisodeId` kosong; pilih komposisi yang sesuai keputusan FIX-01.
3. Panggil helper `admitFromTransfer()` dengan payload sah, `expectedBedVersion` terbaru dan objek `operationIntent` stabil; pertahankan key/body ketika timeout.
4. Pakai episode hasil server untuk langkah consent/cetak dan pembacaan ulang. Hilangkan janji sukses sebelum operasi sumber benar-benar terkonfirmasi.

**Acceptance criteria:** (1) klik proses sampai tahap simpan menghasilkan request ke API transfer existing; (2) satu intent menghasilkan tepat satu episode dan satu reservasi yang sah; (3) tidak ada `POST /episodes` umum tambahan untuk cabang transfer; (4) klik ganda/timeout/replay tidak menghasilkan hasil kedua; (5) source yang berubah atau bed versi lama menampilkan penolakan dan mempertahankan isian; (6) langkah consent/cetak memakai episode yang sama; (7) status pendaftaran selesai mengikuti kriteria FIX-01, bukan sekadar keberadaan banner.

**Verifikasi:** HTTP spy/mock untuk urutan request dan replay; setelah kontrak/backend siap, pengujian browser jaringan nyata. **Risiko:** episode ganda dan reservasi yang saling menghalangi bila hanya menambahkan panggilan baru di akhir alur lama.

### FIX-EPS-006-05 — Pulihkan rujukan dan pengisian awal

**Menutup:** ISS-EPS-006-T1. **Area:** FE. **Jenis:** source. **Berkas sasaran:** hook flow, admission view, review dan hook input dokter. **Radius dampak:** cabang transfer dan pemulihan sesinya. **Dependency:** API detail IGD sudah ada; detail poli mengikuti FIX-03.

1. Simpan referensi sumber/transfer di URL atau state pemulihan sesuai pola repo, tanpa nama/diagnosis/NIK dalam URL.
2. Panggil detail server ketika masuk atau refresh; cocokkan pasien/sumber/status. Pulihkan episode hasil yang sudah tersimpan sesuai kontrak.
3. Isi awal DPJP, unit dan indikasi dari rujukan sah hanya bila nilai belum dipilih; jangan menimpa perubahan petugas atau menetapkan dokter yang sudah tidak layak.

**Acceptance criteria:** (1) refresh pada review maupun langkah berikutnya tidak menghilangkan sumber; (2) contoh rujukan dr. Andi/unit Melati mengisi kontrol operasional, bukan hanya teks review; (3) rujukan dibatalkan setelah refresh menghentikan submit; (4) ganti pasien membersihkan konteks sebelumnya; (5) nilai pilihan petugas yang sah tidak berubah karena pemuatan ulang.

**Verifikasi:** uji refresh, navigasi kembali, ganti pasien dan sumber dibatalkan. **Risiko:** rujukan milik pasien sebelumnya terbawa; identitas selalu diperiksa server.

### FIX-EPS-006-06 — Betulkan informasi dan jangkauan antrean

**Menutup:** ISS-EPS-006-T3 dan pesan menyesatkan ISS-EPS-006-01. **Area:** FE. **Jenis:** source. **Berkas sasaran:** transfer list/review, landing counter, confirmation. **Radius dampak:** tampilan antrean transfer; pertahankan perubahan style dari pekerjaan lain. **Dependency:** DTO/paginasi IGD existing dapat dipakai; variasi poli dan status akhir mengikuti FIX-03/FIX-04.

1. Gunakan asal server untuk label, judul, nama dokter fallback dan nomor kunjungan; jangan menetapkan semua pasien sebagai IGD.
2. Teruskan search/filter/halaman ke server dan gunakan metadata paginasi. Nyatakan cakupan jumlah overdue; bila perlu angka global, definisikan kontrak ringkasannya daripada menghitung halaman pertama sebagai seluruh antrean.
3. Periksa izin baca transfer sebelum menghitung landing; bedakan kosong, gagal dan tidak berhak. Hapus penelanan error yang menghasilkan kesan jumlah nol.
4. Tampilkan hasil penyimpanan/status server; label bed dipesan tidak menyatakan pasien sudah menempatinya.

**Acceptance criteria:** (1) dengan 65 permintaan dan page size 50, seluruh 65 dapat ditemukan/dijangkau; (2) pencarian pasien pada halaman kedua tetap berhasil; (3) pasien poli berlabel Poli, IGD berlabel IGD; (4) akun tanpa Read tidak melakukan fetch terlarang; (5) error server tidak ditampilkan sebagai antrean kosong; (6) tidak muncul janji sinkronisasi/selesai bila server belum membuktikannya.

**Verifikasi:** fixture multipage, izin berbeda, HTTP gagal, dan hasil mutasi. **Risiko:** angka tampak lengkap padahal hanya satu halaman; jelaskan cakupan dan gunakan data server yang sesuai.

### FIX-EPS-006-07 — Tautkan kunjungan asal dengan hasil ranap

**Menutup:** ISS-EPS-006-T2. **Area:** BE/Registrasi. **Jenis:** kontrak/source; schema bila diputuskan diperlukan. **Berkas sasaran:** core transfer, DTO hasil dan model/service milik Registrasi yang ditetapkan K-03. **Radius dampak:** riwayat kunjungan pasien IGD/poli, pembatalan/resume dan pembaca source surat PPRI. **Dependency:** FIX-01/K-01/K-03.

1. Tetapkan relasi sumber–hasil yang terstruktur melalui pemilik Registrasi; gunakan referensi kanonik bila sudah tersedia pada hasil pemeriksaan lanjutan.
2. Simpan relasi dalam komposisi transaksi yang sah; perubahan Notes tidak boleh memutus riwayat asal.
3. Pertahankan catatan klinis pada kunjungan asal. Perubahan status fisik/penutupan IGD mengikuti keputusan yang sudah diselaraskan, bukan dilakukan karena reservasi saja.

**Acceptance criteria:** (1) dari episode hasil dapat ditelusuri tepat ke kunjungan/sumber yang diproses; (2) Notes kosong maupun diisi petugas tidak mengubah relasi; (3) rollback operasi tidak meninggalkan relasi hasil palsu; (4) catatan klinis IGD tetap pada kunjungan IGD; (5) alur pasien langsung tidak wajib mempunyai sumber yang tidak ada.

**Verifikasi:** baca ulang riwayat/sumber lewat service/API dan uji rollback pada lingkungan berwenang. **Risiko:** perubahan owner/schema lintas modul; task terpisah wajib membawa kewenangan yang sesuai. Rencana ini tidak membuat migration atau menjalankannya.

### FIX-EPS-006-08 — Verifikasi alur lengkap dan perbarui bukti

**Menutup:** ISS-EPS-006-T5 dan membuktikan seluruh perbaikan. **Area:** pengujian/dokumentasi hasil. **Berkas sasaran:** test yang bermakna, laporan task dan laporan live testing menurut aturan skill terkait. **Radius dampak:** A–D, integrasi Registrasi/Billing/IGD/bed serta regresi jalur admisi existing. **Dependency:** perbaikan yang diuji, akun/fixture dan lingkungan yang diberi wewenang.

1. Uji alur positif dan negatif pada bagian 8; nilai efek transaksi, bukan hanya teks UI.
2. Pertahankan coordinator idempotensi/lock/rollback existing. Buktikan receipt replay, versi bed dan sumber tidak menghasilkan episode/reservasi kedua.
3. Catat hasil PASS/FAIL/NOT_RUN dan tautkan laporan tracked ketika builder menyelesaikan task; centang selesai hanya bila acceptance terkait terbukti.

**Acceptance criteria:** (1) A–D IGD dan rawat jalan masing-masing dibuktikan; (2) nominal/penjamin/sumber/episode sesuai hasil server; (3) failure dan replay aman; (4) akun operasional diuji, bukan hanya bypass admin; (5) tidak ada klaim runtime atas skenario NOT_RUN.

**Verifikasi:** frontend mengikuti lint/unit/build repository pada task implementasi kecuali dibatasi pengguna saat itu; backend memakai check/build/test proporsional sesuai task. Live browser testing memakai browser visual dan lokasi laporan skill yang berlaku. Tidak menjalankan server, query database atau simulasi mutasi melalui rencana ini.

**Risiko:** test struktur yang lulus disalahartikan sebagai bukti bisnis. Setiap bukti harus menyebut trigger, request, hasil server dan efek yang dapat diperiksa.

## 5. Urutan pengerjaan

```text
FIX-EPS-006-01  SELESAI - kontrak/trace disahkan
  -> FIX-EPS-006-02  SOURCE SIAP - penjamin/deposit; integrasi DB tertunda
  -> FIX-EPS-006-03  SOURCE SIAP - IGD dan surat poli; integrasi DB tertunda
  -> FIX-EPS-006-07  SOURCE SIAP - relasi/lifecycle; migration belum diterapkan
  -> FIX-EPS-006-04  SOURCE SIAP - satu submit transfer pada booking
  -> FIX-EPS-006-05  SOURCE SIAP - URL, draft dan pengisian awal
  -> FIX-EPS-006-06  SOURCE SIAP - antrean server dan hak akses
  -> FIX-EPS-006-08  SEBAGIAN - bukti source/UI tersedia; transaksi DB tertunda
```


Perbaikan pembayaran harus siap sebelum UI dipindahkan ke API transfer untuk skema tersebut. Pemulihan konteks dan perbaikan pembaca antrean dapat ditangani lebih awal tanpa mengubah aturan transaksi. Urutan ini bukan alokasi task; rencana delivery kelak memecah pekerjaan lintas owner bila diperlukan.

## 6. Keputusan pemilik yang dibutuhkan

| No. | Keputusan | Rekomendasi | Menahan |
| --- | --- | --- | --- |
| K-01 | Tetapkan komposisi langkah transfer, timing consent dan batas selesai administratif versus kedatangan fisik | Satu orchestrator existing; satu episode; status fisik terpisah dari reservasi/administrasi | FIX-04 dan lifecycle FIX-07 |
| K-02 | Tetapkan sumber permintaan poli serta aturan terbit/batal/koreksi/pemakaian ulang | Reuse surat pengantar ranap existing dari Clinical Management | FIX-03 |
| K-03 | Tetapkan kontrak penjamin, receipt deposit dan relasi sumber melalui owner | Reuse Registrasi/Billing; tidak membentuk master atau ledger kedua | FIX-02/FIX-07 |

K-01 sampai K-03 telah ditetapkan sesuai rekomendasi yang disetujui instruksi implementasi pengguna 12 Oktober 2026; rinciannya ada pada amandemen EPS-006. Tabel di atas menyimpan alasan dependency saat perencanaan. Tidak mengulang permintaan persetujuan `IGD-DEC-067` yang sudah tercatat di blueprint IGD. Pengguna pada sesi ini telah menetapkan branch backend `MHamzah`; rencana tidak meminta penetapan itu lagi.

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| Roadmap BE admisi transfer | Metadata/BE-RWI-207/endpoint | ID keputusan lama dan `/episodes/open-from-transfer` | Referensi keputusan yang benar serta route source `/admission-transfers/admit` |
| Roadmap FE/requirement-traceability admisi transfer | FE-RWI-229/230, klaim proven | Centang selesai tanpa caller/runtime yang cocok | Acceptance dan bukti sesuai implementasi final; skenario belum berjalan tetap terlihat |
| Arsitektur FE/skema admisi | Tahap Dokter–Bed–Konfirmasi–Consent | Transfer memakai core alur umum yang sudah membuat episode | Cabang transfer mempunyai satu penciptaan episode dan tahap consent yang konsisten |
| API/validation/permission contract | API transfer dan penjamin/sumber | Field ditawarkan tetapi sebagian tidak diterapkan | Bentuk request/response, sumber, versi bed, izin dan error cocok dengan source final |
| Integration/state-transition contract dan decision log | Sumber dan selesai/kepergian | Aturan historis dan current belum sepenuhnya selaras | Relasi asal, arti Executed, timing persetujuan dan kepergian/tiba dinyatakan eksplisit |

Dokumen hulu tersebut telah diperbarui melalui amandemen EPS-006, termasuk K-01 sampai K-03, route dan batas pembuktian. Nomor keputusan Bed Management tidak dipakai untuk keputusan admisi, dan versi global tidak dinaikkan.

## 8. Verifikasi menyeluruh

| Skenario | Hasil yang harus dibuktikan |
| --- | --- |
| A–D IGD | Permintaan sah muncul; wali melengkapi administrasi; submit memakai rujukan yang dipilih; episode/reservasi dan status sumber dapat dibaca ulang; dokumen memakai hasil yang sama |
| A–D poli | Surat/sumber sah muncul sebagai Poli; dapat selesai tanpa disposisi IGD fiktif; sumber tertaut ke hasil |
| Asuransi dan umum | Data penjamin tidak berubah diam-diam menjadi Cash; deposit mengikuti kebijakan dan receipt pemilik |
| Sumber dibatalkan/salah pasien/jenis tidak layak | Submit ditolak sebelum ada hasil transaksi baru |
| Dua petugas dan bed berubah | Satu hasil sah, pihak yang kalah menerima konflik; tidak ada bed/episode/disposisi setengah berhasil |
| Klik ganda, timeout dan replay | Intent/key/body yang sama mengembalikan hasil yang sama, tanpa episode atau deposit ganda |
| Key sama dengan body berbeda | Server menolak konflik key; UI tidak menyembunyikannya sebagai sukses |
| Refresh, mundur dan ganti pasien | Referensi sah pulih, pilihan yang boleh berubah bertahan, konteks pasien lama tidak terbawa |
| 65 permintaan | Semua halaman dapat dijangkau dan pencarian pasien di luar halaman pertama berhasil |
| Hak akses dan scope | Baca/submit sesuai izin; akses langsung tanpa hak ditolak; error berbeda dari data kosong |
| Dokumen dan status fisik | Persetujuan/cetak memenuhi kriteria selesai; reservasi tidak ditulis sebagai pasien sudah tiba/bed occupied |
| Regresi pasien baru/lama/kamar pulih | Alur existing tetap sesuai kontraknya; perbaikan cabang transfer tidak membuat episode tambahan pada cabang lain |

**Bukti aktual saat rencana ditulis:** audit source pada branch yang dikoreksi; API/DI/coordinator ditemukan; lima unit test struktur transfer lulus pada audit sebelumnya. **Belum dijalankan:** integrasi/API/browser/database untuk branch pasangan ini, build ulang, serta acceptance seluruh FIX di atas. Tidak ada query atau perubahan data pasien pada pekerjaan ini.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Rencana 8 perbaikan dibuat berdasarkan audit ulang `MHamzah`; pembangunan ulang API yang sudah ada tidak direkomendasikan; belum mengubah source/roadmap/database | diagnose-module-issue |

## 10. Hasil implementasi 12 Oktober 2026

Satu submit transfer pada booking membuat kunjungan, penjamin, episode, relasi asal, reservasi dan deposit positif melalui owner dalam satu outer transaction. Surat poli existing didukung; source/pasien/status/bed/version/izin diperiksa server. URL/draft memulihkan sumber dan isian operator. Antrean memakai paging/search server dan jumlah seluruh hasil filter; izin baca ditunggu sebelum fetch. Persetujuan harus tersimpan untuk episode hasil yang sama sebelum tombol Selesai aktif.

Backend build Repair006: PASS, 0 error, 270 warning. QBE strict: PASS, 0 violation. Frontend build dan lint: PASS; unit terarah: 28/28 PASS; browser headed HTTP mock: 6/6 PASS. Lint dan hasil browser headed serta kegagalan suite penuh dicatat pada [FIX-08](../../task/report/frontend/FIX-EPS-006-08.md). Belum ada pengujian transaksi pada database nyata: migration belum diterapkan, belum ada fixture PostgreSQL berwenang untuk commit/rollback, concurrency, stale bed dan penerimaan kasir. Tidak mengklaim rilis/operasional tuntas hanya dari build dan mock UI.

| Tanggal | Perubahan lanjutan |
| --- | --- |
| 2026-10-12 | Instruksi pengguna mengesahkan seluruh rencana; kontrak K-01/K-02/K-03, backend/FE/migration dan delapan laporan FIX diterapkan. Penerapan migration dan bukti integrasi DB tetap tertunda. |
