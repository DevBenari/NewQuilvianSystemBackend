# ISSUE-006 — Admisi transfer IGD dan rawat jalan belum terhubung sampai pendaftaran selesai

```yaml
issue_id: ISSUE-EPS-006
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Transfer IGD/Poliklinik, FE-INP-03"
sumber_laporan: "Permintaan pengguna mengenai penerapan alur 1.A–1.D dan daftar perbaikannya; koreksi branch backend ke MHamzah"
tanggal_issue: "2026-10-12"
status: DIPERBAIKI_MENUNGGU_VERIFIKASI_RUNTIME
keparahan_tertinggi: Blocker
source_sha_backend: "b3957e57707b8ee0cd96691454f3b830c1e1c283 (MHamzah)"
source_sha_frontend: "eb04e206f6481bd94f90c3afad8a8a7d7e2e8387 (HamzahV2), dengan perubahan working tree milik pekerjaan lain"
rencana_perbaikan: ../plan-repair/plan-repair-006-admisi-transfer-igd-rawat-jalan.md
```

## 1. Ringkasan

API antrean, detail, dan penyelesaian transfer **sudah tersedia pada backend branch `MHamzah`**. Kesimpulan audit chat sebelumnya bahwa API tersebut belum ada berasal dari pembacaan branch `QuilvianIntegrationBackend` dan dicabut untuk scope `MHamzah`. Masalah utama yang tetap terbukti ialah alur frontend memakai penyimpanan admisi umum, sementara helper penyelesaian transfer tidak mempunyai pemanggil UI. API transfer yang tersedia juga belum menangani sumber rawat jalan serta belum memakai isian penjamin dan deposit yang diterimanya. Dokumen ini membedakan fakta source dari risiko yang masih perlu pengujian runtime; belum ada klaim bahwa database pasien nyata telah mengalami kesalahan.

### Batas dan versi pemeriksaan

- Backend: `MHamzah`, SHA pada metadata; Git backend bersih sebelum penulisan kedua dokumen ini. Tidak melakukan checkout, fetch, commit, query database, atau perubahan source.
- Frontend: `HamzahV2`, SHA pada metadata. Tiga komponen admisi dan beberapa berkas style/stepper sedang berubah di working tree dari pekerjaan lain. Perubahan itu dipertahankan. Rujukan baris frontend di bawah mengacu pada versi commit tersebut; simbol dan perilaku inti juga diperiksa pada working tree.
- Blueprint pemilik: `rawat-inap/episode-rawat-inap`, sebagaimana `../blueprint-manifest.md` dan `../../02-module-map.md`. Manifest child revision `11` memuat target Bed Management `0.12.0` berstatus draft, sedangkan approval terdahulu berlaku sesuai scope masing-masing. Label versi yang sama pada laporan admisi lama tidak diperlakukan sebagai approval atas seluruh target baru.
- Tidak menerima screenshot atau capture database. Bukti runtime/UI/API/database untuk audit ini belum tersedia. Lima unit test transfer pada audit awal lulus, tetapi hanya membuktikan struktur opsi, langkah, dan keberadaan fungsi service.

### Daftar bukti source

Path backend relatif terhadap `NewQuilvianSystemBackend`; path frontend relatif terhadap `QuilvianSystemFrontendDev`. Semua kutipan memakai SHA masing-masing pada metadata, kecuali bila ditandai working tree.

| Bukti | Repository dan lokasi | Fakta yang dapat diperiksa |
| --- | --- | --- |
| E01 | Backend `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferController.cs:18`, `:41`, `:57` | Route antrean/detail tersedia dan dijaga `InpatientAdmissionTransfer : Read` |
| E02 | Backend `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferAdmitController.cs:14`, `:21`, `:25` | `POST /admission-transfers/admit` tersedia, dijaga `InpatientEpisode : Create`, sukses HTTP 201 |
| E03 | Backend `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs:45`, `:52`, `:124`, `:160`, `:184` | Daftar/detail hanya membaca `EmgDisposition` dan mengisi asal `Emergency` |
| E04 | Backend `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs:210`, `:220`, `:247`, `:281`, `:294` | Eksekusi mengunci disposisi, membuat kunjungan baru, memesan bed, lalu mengubah disposisi ke `Executed` |
| E05 | Backend `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.BedManagement.cs:11`, `:18`, `:25` | Wrapper memeriksa versi/scope bed, memakai coordinator, hasilnya reservasi dan episode |
| E06 | Backend `Areas/HealthServices/InPatientManagement/Services/InpBedMutationCoordinator.cs:18`, `:41`, `:49`, `:114` | Kunci idempotensi, transaksi, pemeriksaan receipt, dan rollback sudah ada; bukan kemampuan yang harus dibangun ulang |
| E07 | Backend `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs:12`, `:95`, `:100`, `:121`, `:123`; `Services/InpEpisodeService.cs:1163`, `:1182` | DTO menerima sumber, versi bed, skema pembayaran dan deposit; pembuat kunjungan internal menetapkan `PaymentType = Cash` |
| E08 | Frontend `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:534`, `:598`, `:604`, `:790` | Rujukan transfer hanya disimpan di state; hook Dokter mendapat rujukan kamar pulih, bukan rujukan transfer |
| E09 | Frontend `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx:56`, `:435`, `:467` | Hook aktif membuat kunjungan umum lalu episode; tidak menerima identitas transfer |
| E10 | Frontend `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js:55`, `:72`, `:88` | Helper detail dan submit tersedia; pencarian seluruh `src` tidak menemukan pemanggil UI kedua method itu |
| E11 | Frontend `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx:612`, `:658`, `:662`; `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-confirmation.jsx:166` | Konfirmasi hanya mengunci/memperbarui episode, tetapi banner menjanjikan sinkronisasi `Executed` |
| E12 | Frontend `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx:90`, `:104`; `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:14` | URL menyimpan pasien/langkah/episode, belum menyimpan identitas rujukan transfer |
| E13 | Frontend `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx:45`, `:61`, `:235`; service transfer `:40`; view admisi `:679`, `:693` | Query awal dibatasi 50 item, filter dilakukan lokal, label asal tetap IGD, penghitung tidak memeriksa izin transfer dan menelan error |
| E14 | Backend `Areas/HealthServices/ClinicalManagement/Models/CliDoctorCertificate.cs:17`, `:21`, `:23`, `:76`; `Services/DoctorCertificateService.cs:127`; `InPatientManagement/Services/InpAdmissionSourceReader.cs:358` | Surat pengantar ranap sudah ada; ringkasannya sudah dibaca Workspace PPRI, tetapi belum menjadi sumber antrean transfer |
| E15 | Backend `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`; service transfer E04 | Tidak ditemukan `OriginEncounterId` pada model; jalur transfer tidak menyimpan penghubung terstruktur sumber dengan kunjungan baru |
| E16 | Backend `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/frontend/FE-RWI-237.md`, bagian 1–2; laporan `BE-RWI-221.md`, bagian penerimaan wrapper | Laporan terbaru mengakui helper belum mempunyai caller UI dan runtime transfer belum terbukti penuh |
| E17 | Backend `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap-admisi-transfer-igd.md:10`, `:139`; `contracts/api-contract.md`, bagian 13.8; `../../00-interview-decisions.md:3648` | Roadmap lama menyebut route berbeda dan ID keputusan yang kini menunjuk Bed Management; acuan perlu diselaraskan |
| E18 | Frontend `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx:56`; Backend `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs:10`, `:23` | Hook pilihan bed sekarang baru aktif setelah episode ada, sedangkan endpoint daftar bed mendukung pembacaan tanpa episode |

## 2. Laporan asli

| No. laporan | Isi pengguna | Lampiran |
| --- | --- | --- |
| 1.A | Admisi menerima form permintaan rawat inap dari IGD | Tidak ada |
| 1.B | Wali ke admisi ranap untuk pendaftaran pasien | Tidak ada |
| 1.C | Admisi melanjutkan pengisian pendaftaran pasien IGD untuk ranap | Tidak ada |
| 1.D | Pendaftaran selesai | Tidak ada |
| Lanjutan | Apa saja yang harus diperbaiki dari hasil audit? | Tidak ada |
| Koreksi | Backend yang harus dibaca adalah branch `MHamzah`; lanjutkan pekerjaan | Tidak ada |

Kedatangan fisik wali pada 1.B merupakan langkah operasional. Pengguna belum meminta pencatatan check-in wali; dokumen ini tidak menambah status atau layar check-in baru.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ISS-EPS-006-01 | 1.C–1.D | Penyimpanan frontend tidak menjalankan transfer | BUG, CONTRACT_MISMATCH | FE | Blocker | SUDAH-VERIFIKASI E08–E11 | FIX-EPS-006-04 |
| ISS-EPS-006-02 | 1.A; asal rawat jalan | SourceType belum dijalankan backend | CONTRACT_MISMATCH | BE | High | SUDAH-VERIFIKASI E03–E07 | FIX-EPS-006-03 |
| ISS-EPS-006-03 | 1.C–1.D | Penjamin/deposit belum diterapkan pada API transfer | CONTRACT_MISMATCH | BE, Billing | High | SUDAH-VERIFIKASI E04, E07; dampak runtime belum diuji | FIX-EPS-006-02 |
| ISS-EPS-006-T1 | Tambahan | Konteks rujukan dan pengisian awal belum utuh | BUG | FE | High | SUDAH-VERIFIKASI E08–E12 | FIX-EPS-006-05 |
| ISS-EPS-006-T2 | Tambahan | Kunjungan asal belum tertaut terstruktur | CONTRACT_MISMATCH | BE, Registrasi | High | SUDAH-VERIFIKASI E04, E15 | FIX-EPS-006-07 |
| ISS-EPS-006-T3 | Tambahan | Label, jumlah antrean, pencarian dan akses membingungkan | BUG | FE | Medium | SUDAH-VERIFIKASI E13 | FIX-EPS-006-06 |
| ISS-EPS-006-T4 | Tambahan | Referensi kontrak dan keputusan bertabrakan | CONTRACT_MISMATCH | Dokumen | High | SUDAH-VERIFIKASI E17 | FIX-EPS-006-01 |
| ISS-EPS-006-T5 | Tambahan | Bukti struktur belum membuktikan alur lengkap | BUG pada klaim keterbuktian | Verifikasi | High | SUDAH-VERIFIKASI E16 dan isi unit test | FIX-EPS-006-08 |

## 4. Rincian per temuan

### ISS-EPS-006-01 — Penyelesaian transfer tidak dipanggil

**No. laporan:** 1.C–1.D. **Jenis:** BUG, CONTRACT_MISMATCH. **Area:** frontend. **Keparahan:** Blocker. **Bukti:** SUDAH-VERIFIKASI E08–E11. **Perbaikan:** FIX-EPS-006-04.

**Apa yang terjadi.** Petugas dapat memilih pasien transfer dan melanjutkan formulir, tetapi penyimpanan memakai alur pasien umum. Banner menyebut disposisi akan menjadi `Executed`, padahal tombol konfirmasi tidak memanggil API yang mengerjakan perubahan tersebut.

**Kenapa terjadi.** Pemilihan rujukan hanya mengisi `transferReferral`. Identitas disposisi tidak diteruskan ke hook penyimpanan. `admitFromTransfer()` tersedia di service, tetapi belum dihubungkan dengan UI. Hook bed juga membutuhkan episode yang sudah dibuat, sehingga menambahkan satu panggilan transfer di akhir alur lama bukan perbaikan yang memadai: endpoint transfer membuat episode dan reservasinya sendiri (E04, E18).

**Penyimpangan desain.** Hasil bisnis pada roadmap `FE-RWI-230` menuntut penyelesaian transfer nyata; menampilkan banner saja tidak memenuhinya. Route dan rincian tahap tulis pada dokumen lama perlu dibetulkan dahulu melalui FIX-01.

**Dampak nyata.** Contoh samaran: petugas menyelesaikan formulir Tn. Budi, tetapi disposisi IGD belum diproses melalui endpoint transfer. **DUGAAN runtime:** ia masih terlihat menunggu pada antrean dan dapat diproses kembali. Buktikan melalui UI/API dan pemeriksaan data uji yang berwenang.

**Rekomendasi.** Gunakan satu jalur penyimpanan transfer yang memanggil API existing tepat sekali per maksud operasi. Hindari menjalankan penciptaan episode umum lalu membuat episode kedua lewat API transfer.

### ISS-EPS-006-02 — Sumber rawat jalan belum diimplementasikan

**No. laporan:** 1.A dan asal rawat jalan. **Jenis:** CONTRACT_MISMATCH. **Area:** backend. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI E03–E07. **Perbaikan:** FIX-EPS-006-03.

**Apa yang terjadi.** DTO menerima `SourceType = Outpatient`, tetapi daftar/detail selalu membaca disposisi IGD, dan eksekusi selalu mencari `EmgDisposition`.

**Kenapa terjadi.** Kontrak menawarkan dua sumber, sedangkan pembaca dan penulis hanya memiliki implementasi sumber `Emergency`. Parameter `SourceType` belum menentukan sumber kueri maupun validasi eksekusi.

**Penyimpangan desain.** Judul dan tujuan roadmap menawarkan IGD/Poliklinik, sehingga ini selisih kontrak, bukan sekadar masalah teks.

**Dampak nyata.** Surat pengantar ranap Ny. Sari dari poli tidak dapat ditemukan melalui pembaca antrean tersebut. Mengirim sumber `Outpatient` tidak membuat server membaca surat poli.

**Rekomendasi.** Reuse surat pengantar dokter dan service pemiliknya (E14), dengan aturan kelayakan yang disepakati. Validasi sumber pada query, detail dan submit; tolak nilai tidak dikenal. Jangan menganggap rujukan kamar pulih sama dengan seluruh rujukan poli: model `InpAdmissionReferral` mensyaratkan `OprCaseId`.

### ISS-EPS-006-03 — Pembayaran yang diterima DTO belum dipakai

**No. laporan:** 1.C–1.D. **Jenis:** CONTRACT_MISMATCH. **Area:** backend/Billing. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI E04, E07. **Perbaikan:** FIX-EPS-006-02.

**Apa yang terjadi.** Request transfer mempunyai `PaymentScheme` dan `DepositAmount`, tetapi core transfer tidak menggunakan keduanya. Core membuat kunjungan melalui `OpenAdmissionAsync` tanpa encounter existing; pembuat kunjungan internal menetapkan `Cash`.

**Kenapa terjadi.** DTO dan penulis transaksi tidak terhubung dengan pemilik penjamin dan deposit. Mengganti jalur frontend langsung ke helper transfer dapat mengekspos selisih ini, meskipun alur umum mempunyai penyimpanan penjaminnya sendiri.

**Penyimpangan desain.** Roadmap/laporan BE-RWI-207 menyebut pengikatan penjamin biaya. Isian yang diterima tetapi diabaikan belum memenuhi hasil tersebut.

**Dampak nyata.** **Inferensi dari source, belum bukti database:** request berlabel BPJS tetap mengikuti pembuat kunjungan `Cash`, dan nominal deposit tidak diteruskan oleh service ini. Verifikasi dengan fixture asuransi dan receipt Billing sebelum aktivasi.

**Rekomendasi.** Gunakan service Registrasi untuk data penjamin yang sah dan service Billing untuk deposit; tetapkan kontrak identitas penjamin, cara bayar, hasil receipt, dan pemulihan kegagalan. Jangan cukup menyalin teks `PaymentScheme` atau membuat tabel deposit tandingan.

## 5. Tanya-jawab pelapor

**T: Apakah alur A–D sudah diterapkan?** Sebagian. Pada `MHamzah`, fondasi API IGD sudah ada; penyelesaian UI dan dukungan rawat jalan belum lengkap. Temuan awal API hilang tidak berlaku untuk branch ini.

**T: Apa yang harus diperbaiki?** Sambungan submit transfer, penjamin/deposit, sumber rawat jalan, pemulihan/pengisian awal data, penautan kunjungan, ketepatan tampilan antrean, dan pembuktian ujung-ke-ujung. Urutannya ada pada dokumen rencana pasangan.

**Terkait 1.B:** layar mendukung administrasi bersama wali. Tidak ada bukti bahwa pengguna meminta perubahan berupa status kedatangan wali.

## 6. Temuan tambahan

### ISS-EPS-006-T1 — Rujukan hilang setelah refresh; data dokter baru ditampilkan

**Jenis:** BUG. **Area:** FE. **Keparahan:** High. **Status bukti:** SUDAH-VERIFIKASI E08–E12. **Perbaikan:** FIX-05. **Kenapa dicantumkan:** konteks rujukan diperlukan untuk menuntaskan 1.C–1.D.

State `transferReferral` tidak dipulihkan dari identitas rujukan pada URL/server. Data dokter dan tujuan terlihat pada review, tetapi tidak diserahkan sebagai seed ke hook Dokter. Setelah refresh pada langkah review, state diinisialisasi kembali kosong. Rancangan alur mengharuskan posisi URL dapat dilanjutkan; konteks sumbernya juga perlu pulih. Contoh: review menampilkan dr. Andi, tetapi pilihan DPJP operasional tidak terisi dari rujukan itu. Solusi: muat detail server berdasarkan referensi transfer, seed nilai yang masih kosong, dan pertahankan pilihan petugas yang sudah sah.

### ISS-EPS-006-T2 — Riwayat asal baru berupa keterangan

**Jenis:** CONTRACT_MISMATCH. **Area:** BE/Registrasi. **Keparahan:** High. **Status bukti:** SUDAH-VERIFIKASI E04, E15. **Perbaikan:** FIX-07. **Kenapa dicantumkan:** alur berangkat dari kunjungan yang sudah ada.

Core membuat kunjungan baru tanpa penghubung terstruktur ke kunjungan asal; nomor IGD hanya menjadi teks catatan bila catatan tidak diisi petugas. Ini belum memenuhi tujuan penautan pada roadmap BE-RWI-207 dan `RWI-RULE-029`. Contoh: catatan diganti menjadi “sesuai instruksi dokter”; nomor asal tidak lagi hadir pada catatan otomatis. Solusi: simpan relasi yang disahkan melalui pemilik Registrasi; catatan klinis tetap milik kunjungan asal. Timing penutupan kunjungan dan makna `Executed` harus mengikuti kontrak yang diselaraskan, bukan ditetapkan dari banner.

### ISS-EPS-006-T3 — Antrean sebagian terlihat sebagai antrean keseluruhan

**Jenis:** BUG. **Area:** FE. **Keparahan:** Medium. **Status bukti:** SUDAH-VERIFIKASI E13 dan working tree. **Perbaikan:** FIX-06. **Kenapa dicantumkan:** memengaruhi kemampuan menemukan permintaan pada 1.A.

Service meminta 50 item, lalu layar mencari dan menghitung overdue dari daftar lokal tanpa navigasi seluruh halaman. Label asal masih tetap IGD pada versi commit dan working tree yang diperiksa. Penghitung landing memanggil API tanpa pemeriksaan izin transfer dan menelan error. Contoh: bila ada 65 permintaan, 15 pada halaman berikutnya tidak ikut pencarian lokal. Solusi: gunakan pencarian/filter/paginasi server, jumlah yang didefinisikan jelas, asal dari response, serta keadaan gagal/tidak berhak yang dapat dipahami. Perubahan style yang sedang masuk tidak dianggap bukti perbaikan perilaku ini.

### ISS-EPS-006-T4 — Acuan dokumen lama tidak cocok dengan source dan keputusan current

**Jenis:** CONTRACT_MISMATCH. **Area:** dokumen. **Keparahan:** High. **Status bukti:** SUDAH-VERIFIKASI E17. **Perbaikan:** FIX-01. **Kenapa dicantumkan:** menentukan API dan urutan tulis yang akan dipakai builder.

Roadmap lama menyebut `/episodes/open-from-transfer`; source dan kontrak bagian 13.8 memakai `/admission-transfers/admit`. Roadmap mengaitkan keputusan `RWI-DEC-274`–`276` dengan antrean/eksekusi/60 menit, tetapi decision log current memakai ID itu untuk cakupan Bed Management dan kategori transfer kelas. Rujukan API 11.7 pada roadmap juga tidak sama dengan bagian 13.8 current. Solusi: selaraskan referensi dan keterlacakan dengan bukti keputusan yang benar; jangan mengarang ID baru atau menganggap centang lama membuktikan alur current.

### ISS-EPS-006-T5 — Klaim selesai melampaui bukti yang tersedia

**Jenis:** BUG pada klaim keterbuktian. **Area:** verifikasi. **Keparahan:** High. **Status bukti:** SUDAH-VERIFIKASI E16 dan `tests/unit/inpatient-admission-transfer.test.mjs`. **Perbaikan:** FIX-08. **Kenapa dicantumkan:** jawaban penerapan A–D memerlukan hasil yang benar-benar berjalan.

Lima unit test hanya menguji mode, langkah dan method service. Laporan FE-RWI-237 justru mengakui helper tidak mempunyai caller dan runtime belum diuji. Contoh: test method bertipe fungsi tetap lulus walaupun tidak ada tombol yang memanggilnya. Solusi: uji perjalanan pengguna, request yang dikirim, respons server dan hasil transaksi dengan fixture yang diotorisasi; jangan hanya memeriksa tulisan sukses.

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

Tidak ada pertanyaan yang menahan penyusunan rencana. Backend yang diminta sudah ditetapkan pengguna: `MHamzah`. Frontend yang diperiksa tetap `HamzahV2` beserta working tree; pengguna belum mengarahkan pemeriksaan ke branch frontend lain.

### Untuk pemilik saat kontrak perbaikan ditetapkan

| No. | Keputusan/penyelarasan | Pilihan yang nyata | Rekomendasi | Menahan |
| --- | --- | --- | --- | --- |
| K-01 | Titik tulis transfer dan batas “pendaftaran selesai” terhadap persetujuan/booking/kepergian fisik | Pakai komposisi transfer existing dengan tahap UI yang sesuai; atau ubah kontrak agar menyelesaikan episode draft existing | Pertahankan satu orchestrator transfer existing; sesuaikan tahap UI, penjamin dan consent secara eksplisit; jangan membuat episode dua kali | FIX-04 dan aturan lifecycle pada FIX-07 |
| K-02 | Sumber sah permintaan rawat jalan dan kapan dianggap sudah dipakai | Surat pengantar dokter yang terbit; atau sumber bisnis lain yang sudah disahkan | Reuse `CliDoctorCertificate.InpatientReferral`; tetapkan pembatalan/koreksi/pemakaian ulang melalui owner | FIX-03 |
| K-03 | Kontrak identitas penjamin, receipt deposit dan relasi kunjungan asal | Perluas operasi existing melalui pemiliknya; atau gunakan adaptor service pemilik dengan kontrak yang sama | Reuse Registrasi/Billing, tanpa ledger atau master pasien kedua; konfirmasi ownership perubahan schema bila diperlukan | FIX-02, FIX-07 |

Persetujuan sisi IGD atas `IGD-DEC-067` telah tercatat pada decision log IGD bagian “Approval 2026-08-24”. Catatan Rawat Inap lama yang masih menyebut belum disetujui tidak menjadi alasan meminta persetujuan yang sama lagi. Penyelarasan di atas menyangkut selisih kontrak/tahap current, bukan pengulangan persetujuan lama.

## 8. Catatan pola

- Helper yang tersedia belum berarti fitur dapat dipakai; telusuri tombol sampai efek transaksi.
- Centang roadmap harus mengikuti acceptance dan bukti, bukan hanya tersedianya file.
- Nama sumber yang sudah ada di DTO harus diterapkan pada pembaca dan penulis, bukan hanya judul layar.
- ISSUE-EPS-003 dan ISSUE-EPS-005 tetap terpisah: keduanya tidak ditutup atau dianggap berulang tanpa pengujian masing-masing.
- Rencana ini tidak mengalokasikan task ID, mengubah roadmap, atau memperbaiki source. Tugas perbaikan didaftarkan setelah kontrak dan rencana diputuskan melalui jalur yang berwenang.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-12 | Audit diulang pada `MHamzah` atas koreksi pengguna; temuan API hilang dicabut; issue dan rencana dibuat dari source pasangan yang benar | diagnose-module-issue |


## Tindak lanjut implementasi 12 Oktober 2026

Perubahan source dan kontrak seluruh FIX diterapkan melalui instruksi pengguna. Audit source historis di atas tetap sebagai dasar temuan, bukan gambaran source setelah perbaikan. [Register perbaikan](../plan-repair/plan-repair-006-admisi-transfer-igd-rawat-jalan.md) dan [laporan verifikasi](../../task/report/frontend/FIX-EPS-006-08.md) membedakan bukti build/unit/browser mock dari bukti transaksi PostgreSQL yang belum tersedia. Issue belum ditutup sebagai PROVEN operasional.
