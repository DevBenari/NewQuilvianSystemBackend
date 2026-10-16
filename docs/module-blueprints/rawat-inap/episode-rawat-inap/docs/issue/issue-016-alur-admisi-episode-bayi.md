# ISSUE-016 — Alur admisi bayi belum menyambungkan identitas, perawatan, penempatan, dan tagihan

~~~yaml
issue_id: ISSUE-EPS-016
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Admisi Rawat Inap — FE-INP-03, kategori Bayi Baru Lahir"
sumber_laporan: "Laporan pengguna, satu screenshot, klarifikasi dua invoice, dan rekap meeting yang diserahkan 14 Oktober 2026"
tanggal_issue: "2026-10-14"
status: SEBAGIAN_DIPERBAIKI
keparahan_tertinggi: High
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah), perubahan lokal eksternal muncul selama analisis"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2), ditambah perubahan lokal yang sudah ada"
rencana_perbaikan: ../plan-repair/plan-repair-016-alur-admisi-episode-bayi.md
mode: MODULE BLUEPRINT MODE
~~~

## 1. Ringkasan

Pilihan Bayi Baru Lahir sudah dapat dipilih dan daftar episode ibu sudah mempunyai sumber data, tetapi pendaftaran berikutnya masih memakai formulir umum serta mengirim penanda bayi bernilai salah. Alur juga mewajibkan episode ibu aktif untuk semua bayi, sehingga belum mewakili bayi rujukan dari luar atau ibu yang tidak sedang dirawat. Pemeriksaan penempatan belum mewajibkan kecocokan tempat tidur dengan kebutuhan bayi. Lima butir laporan dan tiga temuan tambahan dibahas di bawah; perubahan alur serta penggabungan penagihan merupakan usulan desain, sedangkan penguncian data bayi menjadi bukan bayi merupakan cacat implementasi yang terbukti. Source aplikasi dan database tidak diubah; hasil pekerjaan ini adalah analisis dan rencana perbaikan.

**Keputusan pengguna yang sudah tersedia, 14 Oktober 2026:** ibu dan bayi mempunyai dua invoice berbeda yang dapat disatukan. Jawaban sebelumnya memilih dua tagihan tetap terpisah yang dapat dibayar bersama. Analisis menggunakan dua invoice sumber dan satu kelompok penagihan/pembayaran. Mengganti keduanya menjadi satu invoice pengganti belum ditetapkan pengguna.

### 1.1 Batas bukti dan lokasi pemilik

- Route frontend: QuilvianSystemFrontendDev/src/app/health-services/inpatient-management/admissions/page.jsx:1 mengimpor InpatientAdmissionView.
- Pemilik layar FE-INP-03 adalah episode-rawat-inap, berdasarkan NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/02-module-map.md:230.
- BillingManagement memiliki invoice, tarif, dan pembayaran; kepemilikan ini tercantum pada module map yang sama:124. Perubahan billing perlu handoff ke pemilik BillingManagement.
- Tidak ada capture database. Data master boks, ruangan, tarif, penjamin, ataupun invoice yang sebenarnya belum diperiksa.
- Tidak dilakukan uji browser, pengiriman API, build, pengujian otomatis, ataupun query database. Klaim source berarti dibaca langsung, bukan bukti keberhasilan runtime.
- Backend mempunyai dokumen issue 014–015 belum tracked; frontend mempunyai perubahan lokal yang sudah ada sebelum analisis. Seluruhnya dipertahankan. Referensi baris frontend mengacu pada working tree tanggal analisis, bukan hanya commit HEAD.
- Saat verifikasi akhir, pekerjaan lain mengubah PatientController.cs, konstanta admisi, dan beberapa berkas pencarian pasien. Analisis ini tidak menulis source tersebut. Validasi newborn diperiksa ulang dan perilakunya tetap sama; nomor baris PatientController di bawah diperbarui ke snapshot akhir.

Sidik SHA-256 beberapa berkas sumber penting untuk membedakan working tree dari HEAD:

| Berkas frontend, relatif ke QuilvianSystemFrontendDev | SHA-256 saat analisis |
| --- | --- |
| src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx | 15818A6F6017C6A2DF2349A6095D91F13591E0728035C6AAEDFEFDCE226503A8 |
| src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx | 2B3C0B7BD1EE91BBFE5FFA88E6A2D6115EB7B5853BEA3D15F177C2AA4B7D3A06 |
| src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx | 930B8A192D4A91E168C73FF78D760CDABE94BC36E23A73CCF48F3F13120D2C14 |
| src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx | 621DE87A0112BA06A3A4833972BF404CA47E7EF127834A8DFF94EB7F9297DB2D |

Snapshot akhir konstanta admisi: B5E6706CAEC5BFEC08DECDCF4D7F26B8D38DD972241111BC313E71B7F152FDF3. Posisi daftar langkah dan kategori yang dikutip tetap sama. Snapshot akhir backend Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs: F5C541E934927564B770A6F717054AB7B1A21F1FB10091DA442F84B70A3E6642. Hash adalah penanda isi saat dibaca, bukan klaim bahwa working tree akan berhenti berubah.

### 1.2 Rujukan resmi Indonesia

Seluruh tautan berikut diperiksa pada 14 Oktober 2026. Rujukan identitas, interoperabilitas, klinis, dan klaim mempunyai kegunaan berbeda; tidak satu pun menetapkan jumlah stepper aplikasi.

| ID | Rujukan | Temuan yang digunakan |
| --- | --- | --- |
| R1 | [SATUSEHAT — Pasien Bayi Baru Lahir](https://satusehat.kemkes.go.id/platform/docs/id/master-data/master-patient-index/pasien-bayi/) | Identitas bayi tersendiri; tersedia penggunaan NIK ibu sebagai identifier dengan sistem khusus, nama sementara, tanggal lahir, jenis kelamin, urutan kelahiran kembar, dan alamat orang tua. NIK ibu tidak diperlakukan sebagai NIK pribadi bayi. |
| R2 | [SATUSEHAT — Intranatal Care](https://satusehat.kemkes.go.id/platform/docs/id/interoperability/inc/) | Ibu dan bayi mempunyai Encounter masing-masing; riwayat perpindahan lokasi bayi dicatat. |
| R3 | [SATUSEHAT — Data Kelahiran](https://satusehat.kemkes.go.id/platform/docs/id/interoperability/data-kelahiran/) | Pendaftaran bayi setelah persalinan menghasilkan ID bayi sendiri; data kelahiran tersambung dengan data ibu dan kunjungan persalinan. |
| R4 | [Kemenkes — Perawatan Bayi Baru Lahir](https://keslan.kemkes.go.id/view_artikel/2777/perawatan-bayi-baru-lahir) dan [Kemenkes — Pemulihan Pasca Seksio Sesarea](https://keslan.kemkes.go.id/view_artikel/596/pemulihan-kondisi-kesehatan-pasca-seksio-sesarea) | Rawat gabung merupakan jalur yang perlu didukung saat kondisi ibu dan bayi memungkinkan. Pemilihan perawatan harus berdasarkan penilaian tenaga klinis. |
| R5 | [JDIH — Permenkes 26 Tahun 2021](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-26-tahun-2021), [naskah PDF, halaman tercetak 44](https://jdih.kemkes.go.id/storage/documents/pdfs/2021permenkes026.pdf) | Pada ketentuan bayi lahir dengan tindakan persalinan, bayi sehat yang mendapat pelayanan neonatal esensial dibayar dalam paket persalinan ibu; bayi sakit yang memerlukan pelayanan lain dibayar terpisah. Ini aturan klaim, bukan perintah menggabungkan identitas pasien atau seluruh invoice internal. JDIH mencatat peraturan berstatus berlaku saat diperiksa. |
| R6 | [JDIH — Permenkes 24 Tahun 2022](https://jdih.kemkes.go.id/documents/peraturan-menteri-kesehatan-nomor-24-tahun-2022) | Dasar penyelenggaraan rekam medis; bukan dasar untuk mengklaim suatu susunan stepper wajib secara nasional. |

**Implikasi desain, bukan kutipan peraturan:** Quilvian perlu memisahkan identitas bayi, episode bayi, riwayat lokasi, penjamin bayi, serta kelompok penagihan keluarga. Besaran tarif, klasifikasi level neonatal, batas umur operasional, penanganan ibu tidak dikenal, dan kewenangan persetujuan mengikuti keputusan serta SOP rumah sakit yang perlu dikunci.

### 1.3 Bukti tambahan dari rekap meeting admisi

**Diterima:** 14 Oktober 2026. **Tanggal meeting:** tidak disebutkan dalam lampiran. **Sumber:** [Teks tempel.txt — rekap meeting Admisi Rawat Inap](<C:/Users/Admin/.codex/attachments/711065c0-1579-4c81-95eb-5eb19cf0d392/Teks tempel.txt>).

Pengguna meminta analisis meeting sebelum penyusunan skema tiap stepper dilanjutkan. Pada pembaruan pertama hanya bukti dan dampaknya pada analisis yang dicatat. Setelah instruksi lanjutan “oke update stepper admisi episode bayi” pada 14 Oktober 2026, skema tiap tahap diperinci pada plan repair bagian 3, termasuk rencana deposit versus pembayaran kasir, identitas ibu pada gelang, dan pencatatan waktu. Implementasi aplikasi belum dilakukan.

**Batas bukti:** lampiran berupa rekap cepat/ringkasan rapat, bukan transkrip percakapan lengkap dengan timestamp atau notulen bertanda persetujuan. Isi teks sudah dibaca langsung, tetapi ketepatannya terhadap rekaman belum diverifikasi. Pernyataan “tim mengklarifikasi” dilaporkan sebagaimana tercantum dalam rekap; tidak otomatis menjadi approval seluruh desain, kontrak API, atau sembilan tahap yang direkomendasikan.

#### Pernyataan yang secara langsung berkaitan dengan bayi

| ID bukti | Lokasi dalam rekap | Isi yang dinyatakan | Arti bagi kebutuhan | Batas penafsiran |
| --- | --- | --- | --- | --- |
| MTG-BAYI-01 | Rekap cepat; Diskusi Proses Pendaftaran Rumah Sakit | Pembahasan mencakup bayi yang lahir di rumah sakit dan ibunya | Kasus lahir lokal perlu didukung oleh admisi | Tidak membuktikan bahwa bayi rujukan atau rawat ulang dikeluarkan dari scope |
| MTG-BAYI-02 | Langkah selanjutnya — Mahda; Diskusi Proses Pendaftaran Rumah Sakit | Pendaftaran bayi dihubungkan dengan catatan/informasi ibu | Pilihan bayi harus meneruskan hubungan ibu sampai data pasien tersimpan | Tidak menyebut bahwa ibu selalu wajib mempunyai episode aktif |
| MTG-BAYI-03 | Diskusi Proses Pendaftaran Rumah Sakit | Nama bayi awalnya ditampilkan sebagai “Bayi [Nama Ibu]” | Form dan ringkasan perlu menyarankan nama awal berdasarkan ibu yang diverifikasi | Prefix/sapaan, kembar, perubahan nama resmi dan bentuk cetak belum dirinci |
| MTG-BAYI-04 | Langkah selanjutnya — Mahda | Gelang memberi label bayi dengan nama ibu | Gelang bayi merupakan keluaran yang secara eksplisit diminta | Tidak menyatakan nama ibu sebagai satu-satunya pengenal pada gelang |
| MTG-BAYI-05 | Diskusi Proses Pendaftaran Rumah Sakit | Penagihan dan pelacakan ibu–bayi dapat tetap terhubung atau catatan dipisahkan jika diperlukan | Hubungan ibu–bayi dan fleksibilitas penagihan perlu dirancang | Tidak menjelaskan apakah yang dipisah adalah invoice, tampilan, penanggung biaya, atau hubungan episode |
| MTG-BAYI-06 | Langkah selanjutnya — Mahda; paragraf penutup | Waktu respons tiap tahap, dari masuk sampai serah terima/perawatan, perlu dicatat | Alur bayi perlu ikut pelacakan waktu admisi dan serah terima | Event, titik awal/akhir durasi, target waktu, dan pengecualian belum ditentukan |

Kutipan penting dari lampiran:

> “sistem pendaftaran menghubungkan bayi dengan informasi ibu, dengan nama bayi awalnya ditampilkan sebagai ‘Bayi [Nama Ibu].’”

> “memastikan gelang memberi label bayi dengan nama ibu.”

> “kemampuan untuk menjaga mereka tetap terhubung atau memisahkan catatan jika diperlukan.”

Tanda kutip pada kutipan pertama dinormalisasi untuk keterbacaan. Tidak ada penggabungan dua pasien yang dapat disimpulkan dari kalimat tentang “catatan”.

#### Keterkaitan dengan keputusan yang sudah diberikan pengguna

| Topik | Bukti meeting | Bukti pengguna dalam chat | Kesimpulan analisis |
| --- | --- | --- | --- |
| Hubungan ibu–bayi | Eksplisit, tetapi bentuk teknis tidak dijelaskan | Pengguna menunjukkan pemilih episode ibu pada screenshot | Pertahankan hubungan identitas ibu; bentuk hubungan episode perlu ditetapkan sesuai konteks |
| Dua invoice dapat disatukan | Rekap hanya menyebut penagihan/catatan terhubung atau dipisah | Pengguna secara spesifik menyatakan “2 invoice berbeda yang bisa di satukan”, setelah memilih pembayaran bersama | Kebutuhan dua invoice sumber berasal dari klarifikasi pengguna; jangan mengatasnamakannya sebagai detail yang sudah diputuskan meeting |
| Rawat gabung dan bed bayi | Tidak dirinci pada bagian bayi | Pengguna meminta filter tempat untuk kebutuhan bayi | Rawat gabung/NICU dan aturan kemampuan tempat tetap rekomendasi analisis serta keputusan terbuka |
| Sembilan tahap bayi | Tidak disebutkan | Pengguna meminta skema per stepper, lalu menunda untuk membaca meeting | Sembilan tahap tetap usulan; belum dianggap disahkan oleh meeting |

#### Pembahasan umum admisi yang relevan tetapi belum spesifik untuk bayi

| Topik umum | Isi rekap | Pengaruh yang perlu dinilai untuk bayi |
| --- | --- | --- |
| Deposit | Dicatat saat pendaftaran, dibayar terpisah di kasir; rekap menyebut minimum Rp10.000.000 | Pisahkan nominal rencana deposit, tagihan deposit, dan pembayaran yang benar-benar diterima. Belum ada dasar menerapkan minimum itu otomatis pada setiap bayi atau menggandakan deposit ibu–bayi |
| Dokter | Langkah selanjutnya menyebut dua DPJP dengan maksimal lima; uraian lain menyebut ideal satu DPJP dan maksimal lima dokter pada kasus tertentu | Bedakan DPJP utama dan dokter tambahan; jumlah serta peran dokter bayi belum dapat dikunci dari rekap yang tidak konsisten ini |
| Penilaian ketergantungan | Tingkat ketergantungan dinilai dahulu dan didokumentasikan keperawatan sebelum penugasan kamar | Penempatan bayi menggunakan kebutuhan dari tim klinis/keperawatan, bukan klasifikasi yang ditebak petugas admisi |
| Lokasi kamar | Tampilan diminta berurutan lantai, sayap A/B, nomor kamar, nomor tempat tidur | Daftar boks/tempat neonatal dapat memakai susunan lokasi yang sama jika master tersedia |
| Estimasi biaya | Diminta pada proses penerimaan; format belum diimplementasikan | Belum ada keputusan tentang estimasi gabungan ibu–bayi, tarif boks, paket neonatal, atau penjamin bayi |

#### Dampak pada analisis dan skema yang kelak dilanjutkan

1. **Identitas bayi:** sumber hubungan ibu harus terus dibawa dari pemilihan sampai penyimpanan. Nama awal disarankan dari ibu, dengan verifikasi petugas. Ini memperkuat ISS-EPS-016-02; builder yang mengirim isNewborn=false tidak mewakili kebutuhan yang tertulis di meeting.
2. **Dokumen akhir:** gelang bayi dengan nama ibu menjadi keluaran eksplisit MTG-BAYI-04. Rincian pengenal bayi, urutan kembar dan format nama perlu disepakati; tidak cukup menyimpulkan semua bayi satu ibu boleh mempunyai label identik.
3. **Billing:** keputusan dua invoice yang dapat disatukan tetap mengikuti klarifikasi pengguna. Dalam admisi, rencana penagihan keluarga dibedakan dari pelaksanaan pembayaran oleh kasir.
4. **Deposit:** mencatat nominal deposit di admisi tidak boleh membuat layar menampilkan “sudah dibayar”. Status pembayaran mengikuti transaksi kasir yang benar-benar berhasil.
5. **Penempatan:** rekap tidak menetapkan bahwa semua bayi harus di satu tipe “Ruang Bayi”, tidak menjelaskan boks kamar ibu, dan tidak menetapkan NICU. Aturan tersebut tetap perlu keputusan pemilik proses/klinis.
6. **Waktu layanan:** rancangan berikutnya perlu menampilkan atau mencatat event admisi dan serah terima sesuai proses; sekadar persentase stepper tidak membuktikan waktu respons tercatat. Waktu lahir, waktu input, waktu aktivasi, dan waktu serah terima tidak disamakan.

Hal yang **belum disebut secara eksplisit** dalam rekap: nomor RM bayi sendiri, nomor episode bayi sendiri, aturan ibu sudah pulang, bayi rujukan luar, rawat ulang, bayi kembar, ibu tidak dikenal, NIK bayi belum terbit, DPJP bayi, detail persetujuan wali, kelayakan bed neonatal, tarif/paket bayi, deposit bayi, serta cara teknis menggabungkan dua invoice dan melakukan refund.

Ketiadaan pembahasan pada rekap tidak berarti kebutuhan tersebut ditolak. Dasar identitas/kunjungan terpisah pada bagian 1.2 dan keputusan repository lama tetap mempunyai sumbernya sendiri; tidak dipindahkan menjadi klaim hasil meeting ini.

## 2. Laporan asli

Pengguna tidak memberikan nomor butir. Nomor analisis berikut dibuat untuk pelacakan; kolom nomor asli tetap ditulis “Tidak bernomor”.

| No. analisis | No. asli | Keluhan atau permintaan | Lampiran |
| --- | --- | --- | --- |
| 1 | Tidak bernomor | Admisi belum sepenuhnya mendukung kebutuhan episode bayi | Screenshot langkah 2 |
| 2 | Tidak bernomor | Setelah memilih Bayi Baru Lahir, tidak jelas apa yang harus diisi pada pendaftaran | Screenshot langkah 2; langkah berikutnya diperiksa dari source |
| 3 | Tidak bernomor | Stepper ambigu dan pengaruh setiap langkah belum jelas | Screenshot stepper 12 langkah |
| 4 | Tidak bernomor | Bed harus terfilter untuk kebutuhan bayi | Permintaan perilaku; tidak ada screenshot hasil bed |
| 5 | Tidak bernomor | Client membutuhkan dua invoice ibu–bayi yang dapat disatukan | Pernyataan dan dua klarifikasi pengguna |

Screenshot tunggal mendukung butir 1–3; tidak ada lampiran yang tidak terpetakan. Nama, nomor RM, episode, dan kode tempat tidur pada screenshot tidak disalin.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis utama | Area | Keparahan | Bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ISS-EPS-016-01 | 1 | Semua bayi dipaksa mempunyai episode ibu aktif | DESIGN_CHANGE | FE + kontrak | Medium | SUDAH-VERIFIKASI | FIX-EPS-016-01, 06 |
| ISS-EPS-016-02 | 2 | Pendaftaran mengunci bayi menjadi bukan newborn | BUG; CONTRACT_MISMATCH | FE + PatientManagement | High | SUDAH-VERIFIKASI | FIX-EPS-016-02 |
| ISS-EPS-016-03 | 3 | Stepper umum tidak menjelaskan konteks bayi | DESIGN_CHANGE | FE + desain | Medium | DARI-CAPTURE; SUDAH-VERIFIKASI | FIX-EPS-016-01, 06 |
| ISS-EPS-016-04 | 4 | Kecocokan bed bayi belum dijaga di server | RULE_VIOLATION; DESIGN_CHANGE | FE + BE | High | SUDAH-VERIFIKASI; isi master UNKNOWN | FIX-EPS-016-04, 05 |
| ISS-EPS-016-05 | 5 | Dua invoice belum mempunyai kontrak pembayaran gabungan lintas pasien | DESIGN_CHANGE | Billing + integrasi | High | SUDAH-VERIFIKASI pada settlement yang diperiksa | FIX-EPS-016-09 |
| ISS-EPS-016-T1 | Tambahan | Hubungan pasien ibu belum dicocokkan dengan episode ibu | BUG; CONTRACT_MISMATCH | BE | High | SUDAH-VERIFIKASI | FIX-EPS-016-03 |
| ISS-EPS-016-T2 | Tambahan | Gagal mencari ibu tampil seperti tidak ada data | BUG | FE | Medium | SUDAH-VERIFIKASI | FIX-EPS-016-07 |
| ISS-EPS-016-T3 | Tambahan | Jalur pasien lama menawarkan bayi tanpa alur ibu yang setara | CONTRACT_MISMATCH; DESIGN_CHANGE | FE + desain | High | SUDAH-VERIFIKASI | FIX-EPS-016-08 |

## 4. Rincian per temuan

Semua path FE di bawah relatif ke QuilvianSystemFrontendDev; path BE relatif ke NewQuilvianSystemBackend. SHA dan status working tree mengikuti metadata bagian 1.

### ISS-EPS-016-01 — Episode ibu aktif dijadikan syarat untuk semua bayi

**No. laporan:** 1. **Jenis:** DESIGN_CHANGE. **Area:** FE dan kontrak. **Keparahan:** Medium. **Bukti:** SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-01 dan 06.

**Apa yang terjadi.** Sesudah memilih bayi, petugas wajib memilih episode ibu yang sedang dirawat. Belum tersedia percabangan untuk bayi lahir di luar RS, ibu sudah pulang, atau bayi kembali dirawat.

**Kenapa terjadi.** FE src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx:429–432 mensyaratkan motherEpisodeId untuk melanjutkan kategori bayi. BE Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs:478–484 hanya mencari episode Admitted milik pasien perempuan. Ini adalah daftar calon berdasarkan kriteria pencarian, bukan bukti ibu kandung atau episode persalinan yang benar.

**Terhadap desain.** 05-skema-tampilan.md:270–274 memang mewajibkan MotherEpisodeId untuk kategori bayi. Perluasan jalur adalah DESIGN_CHANGE, bukan sekadar menghilangkan validasi. Keputusan induk rawat-inap/00-interview-decisions.md:1265–1269 sudah memisahkan episode bayi dan ibu untuk rawat gabung.

**Dampak.** Bayi samaran B lahir di fasyankes lain dan dirujuk ke NICU; ibunya tidak mempunyai episode di RS ini. Petugas tidak mempunyai pilihan yang benar pada langkah 2.

**Rekomendasi.** Pisahkan “asal kelahiran”, “hubungan dengan ibu”, dan “rencana perawatan”. Hubungan pasien ibu tetap disimpan jika diketahui; episode ibu hanya dipilih saat ada konteks perawatan yang relevan. Untuk rawat gabung, ibu harus mempunyai penempatan aktif yang sesuai. Jangan membuat episode rawat inap fiktif untuk ibu demi membuka alur bayi.

### ISS-EPS-016-02 — Formulir umum dan payload bertentangan dengan pilihan bayi

**No. laporan:** 2. **Jenis:** BUG dan CONTRACT_MISMATCH. **Area:** FE + PatientManagement. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-02.

**Apa yang terjadi.** Langkah Pendaftaran tetap menampilkan pemindai KTP dan formulir pasien umum; pemilihan ibu belum menyiapkan formulir bayi.

**Kenapa terjadi.**

1. FE src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:609–612 memasang hook pasien tanpa konteks kategori atau ibu.
2. FE src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx:309–320 selalu memasang ScanPanel dan NewPatientForm.
3. FE src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx:34–39 secara eksplisit mengirim isNewborn=false dan mengosongkan motherPatientId. Komentar:29–32 masih mengacu penundaan bayi pada rilis lama.
4. FE src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx:213–234 memakai builder itu, kemudian selalu membuat dokumen identitas pasien.
5. BE Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs:2481–2518 pada snapshot akhir sebenarnya sudah memvalidasi hubungan bayi–pasien ibu; DTO PatientDtos.cs:392–403 menyediakan IsNewborn, MotherPatientId, BirthOrder, berat, panjang, dan jam lahir.

**Rantai sebab.** Formulir terasa salah konteks ← reuse formulir umum tanpa mode bayi ← pembatasan rilis lama pada builder belum diselaraskan ketika pilihan bayi aktif kembali.

**Terhadap desain.** 05-skema-tampilan.md:278–320 masih menggambarkan form umum. Dokumen lama ikut perlu diamendemen. Namun representasi bayi sebagai bukan bayi bertentangan dengan kategori terpilih dan dukungan kontrak master pasien; ini bukan sekadar perubahan label.

**Dampak.** Bayi samaran B dapat terdaftar dengan penanda master bukan newborn meskipun episode ditautkan ke ibu. Risiko turunannya adalah informasi profil, pengenalan bayi, pencarian keluarga, dan cetakan menjadi tidak konsisten. Kejadian data sebenarnya belum dibuktikan dengan database.

**Rekomendasi.** Gunakan mode pendaftaran bayi pada komponen bersama: data bayi miliknya sendiri; identitas ibu/wali dalam wilayah terpisah. Isi otomatis hubungan ibu, alamat dan kontak dengan konfirmasi. Jangan menyalin NIK ibu ke nomor identitas pribadi bayi dan jangan mewajibkan KTP bayi. Penggunaan identifier NIK-ibu SATUSEHAT membutuhkan pemetaan identifier khusus, bukan menyamakan NIK kedua pasien.

### ISS-EPS-016-03 — Stepper menjelaskan urutan umum tanpa keputusan khusus bayi

**No. laporan:** 3. **Jenis:** DESIGN_CHANGE. **Area:** FE/desain. **Keparahan:** Medium. **Bukti:** DARI-CAPTURE dan SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-01 dan 06.

**Apa yang terjadi.** “Pembayaran” tidak membedakan pilihan penjamin dari transaksi kasir; “Dokter” tidak menjelaskan DPJP bayi; “Pilih Bed” belum menjelaskan rawat gabung atau neonatal. Pilih Bed dan Booking Bed tampil sebagai dua keputusan petugas.

**Kenapa terjadi.** FE src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:89–114 menetapkan satu daftar 12 langkah bagi pasien baru, tanpa cabang newborn. Pemilihan kategori hanya memunculkan pemilih episode ibu di view:259.

**Terhadap desain.** 03-frontend-architecture.md:317–325 dan 05-skema-tampilan.md menetapkan alur tersebut. Penyederhanaan dan percabangan stepper perlu revisi desain, bukan langsung mengubah urutan source.

**Dampak.** Petugas dapat mengira DPJP bayi mengikuti dokter ibu atau penjamin bayi otomatis mengikuti ibu. Angka persentase selesai tidak menjelaskan keluaran tiap langkah.

**Rekomendasi.** Sembilan tahap semantik dalam plan repair, dengan deposit kondisional dan dokumen cetak sebagai aksi penutup. Nomor diturunkan dari tahap yang berlaku; status selesai berdasarkan data tersimpan dan validasi, bukan sekadar pernah dikunjungi. Pemesanan dan penempatan tetap transaksi terpisah meskipun berada dalam satu langkah tampilan.

### ISS-EPS-016-04 — Filter bed tidak menjadi aturan kelayakan bayi

**No. laporan:** 4. **Jenis:** RULE_VIOLATION dan DESIGN_CHANGE. **Area:** FE/BE. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI; master aktual UNKNOWN. **Perbaikan:** FIX-EPS-016-04 dan 05.

**Apa yang terjadi.** Belum ada pembatasan otomatis yang menjamin bayi hanya ditempatkan pada sumber daya perawatan bayi yang sesuai.

**Kenapa terjadi.** FE src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx:360–392 tidak meneruskan isForNewborn. BE Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:143–145 menyediakan filter tersebut hanya jika diminta. Pemeriksaan bersama :1602–1728 mencakup kesiapan, pemesanan, penghunian, jenis kelamin, dan isolasi; IsForNewborn pada :1676 dipakai untuk pengecualian aturan jenis kelamin, bukan untuk mengharuskan bayi memakai bed bayi. DTO pencarian :21–36 juga belum mempunyai konteks pasien bayi sebelum episode tersedia.

**Terhadap desain.** Keputusan 00-interview-decisions.md:1267 mendukung boks tersendiri di kamar ibu. Permintaan “ruang bayi saja” diterima sebagai larangan menawarkan bed dewasa; penerjemahan menjadi satu RoomType=BabyRoom untuk semua bayi akan bertentangan dengan jalur rawat gabung.

**Dampak.** Bayi laki-laki dapat lolos aturan jenis kelamin pada bed dewasa yang menerima laki-laki, walaupun bed itu tidak sesuai kebutuhan neonatal. Ini kemungkinan berdasarkan source; bukan bukti penempatan nyata.

**Rekomendasi.** Validasi server pada pencarian, booking, pembukaan admisi dengan SelectedBedId, penempatan, dan transfer: jenis sumber daya bayi, kemampuan unit, kebutuhan isolasi, kesiapan, kapasitas, serta konteks perawatan. Rawat gabung memakai boks bayi di kamar ibu; perawatan terpisah memakai tempat neonatal yang sesuai. Kemampuan unit jangan ditebak hanya dari nama kamar atau boolean IsForNewborn.

### ISS-EPS-016-05 — Dua invoice dapat disatukan untuk penagihan, tanpa menyatukan pasien

**No. laporan:** 5. **Jenis:** DESIGN_CHANGE. **Area:** BillingManagement dan integrasi. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI pada kontrak yang diperiksa. **Perbaikan:** FIX-EPS-016-09, handoff.

**Apa yang dibutuhkan.** Dua invoice sumber, satu untuk ibu dan satu untuk bayi, dapat dipilih bersama untuk penagihan/pembayaran. Keduanya tetap dapat dibayar terpisah.

**Bukti kemampuan saat ini.** BE Areas/HealthServices/BillingManagement/Billing/DTOs/BillingSettlementDtos.cs:6–20 hanya menerima satu InvoiceId atau DepositAccountId. BillingSettlementService.cs:125–140 menyimpan settlement terhadap satu target. Pencarian pada folder Billing tidak menemukan kontrak khusus pembayaran bersama ibu–bayi. Ini membuktikan keterbatasan jalur settlement yang diperiksa, bukan audit lengkap seluruh mekanisme piutang atau provider.

**Terhadap desain.** Invoice dan ledger dimiliki BillingManagement. Kebutuhan ini adalah perluasan lintas pasien; penggabungan kunjungan IGD ke rawat inap pasien yang sama bukan bukti aman untuk menggabungkan dua pasien berbeda.

**Solusi yang direkomendasikan.** Satu kelompok penagihan mereferensikan kedua invoice dan satu pembayaran dialokasikan secara eksplisit ke masing-masing invoice. Contoh simulasi pasien umum: sisa ibu Rp4.000.000 dan bayi Rp1.000.000, total kelompok Rp5.000.000. Bila hanya Rp3.000.000 dibayar, alokasi yang dikonfirmasi kasir bisa ibu Rp2.500.000 dan bayi Rp500.000; sisa masing-masing Rp1.500.000 dan Rp500.000. Kebijakan alokasi otomatis belum diputuskan.

Jangan menggandakan biaya, menghapus invoice asal, memindahkan obat bayi menjadi milik ibu, menyamakan penjamin, otomatis memindahkan deposit, atau menutup kedua episode sekaligus. Satu bukti pembayaran dapat memuat kedua nomor invoice dan alokasinya. Jika yang kelak dimaksud adalah satu invoice pengganti, perlu kontrak koreksi akuntansi serta referensi penggantian yang berbeda.

**Pembedaan JKN.** R5 mengatur paket klaim bayi sehat versus bayi sakit dalam lingkup yang disebut peraturan. Dua catatan biaya internal tidak otomatis berarti dua klaim JKN ataupun dua utang pasien. Biaya yang sudah masuk paket ibu tidak boleh kembali dijadikan tagihan bayi kepada keluarga tanpa dasar yang sah. Hubungan klaim ditentukan koder/tim penjamin berdasarkan pelayanan dan ketentuan, bukan sakelar “gabung” milik admisi.

## 5. Tanya-jawab pelapor

### Seperti apa pendaftaran episode bayi di rumah sakit Indonesia?

Bayi dicatat sebagai pasien tersendiri dan mempunyai kunjungan sendiri; pada Quilvian rawat inap bayi memakai episode sendiri yang ditautkan ke ibu bila relevan. R1–R3 mendukung pemisahan identitas dan kunjungan. “Rawat gabung” menjelaskan lokasi/perawatan; “tagihan gabungan” menjelaskan penagihan. Keduanya bukan satu pilihan yang saling menentukan.

| Kasus | Hubungan ibu | Episode bayi | Lokasi yang direkomendasikan | Perlakuan billing |
| --- | --- | --- | --- | --- |
| Lahir di RS, ibu dan bayi memenuhi syarat rawat gabung | Pasien ibu + episode persalinan/perawatan terkait | Tersendiri | Boks bayi di kamar ibu; keputusan klinis dikonfirmasi | Biaya tetap dapat ditelusuri per subjek; aturan paket/penjamin diperiksa |
| Lahir di RS, perlu neonatal terpisah | Tetap terkait ibu; tidak harus sekamar | Tersendiri | Perinatologi/NICU sesuai keputusan klinis dan kemampuan unit | Invoice bayi tersendiri bila memang berlaku; dapat ditagihkan bersama |
| Lahir di luar RS/rujukan | Identitas ibu jika diketahui; tidak mensyaratkan episode ibu di RS ini | Tersendiri | Lokasi yang ditetapkan tenaga klinis | Penjamin bayi diverifikasi sendiri |
| Bayi datang kembali sesudah pulang | Reuse RM bayi; hubungan ibu tetap | Episode baru | Sesuai kebutuhan saat kunjungan baru | Invoice episode baru; tidak otomatis membuka ulang paket persalinan lama |

Jumlah level perinatologi dan batas umur tidak ditetapkan di analisis ini. Rumah sakit perlu memetakan kemampuan unit serta SOP sendiri.

### Setelah memilih bayi, apa yang seharusnya diisi?

Petugas mengisi identitas bayi dan memverifikasi hubungan keluarga, bukan melengkapi seluruh identitas orang dewasa atas nama bayi.

| Data | Cara isi yang direkomendasikan | Contoh samaran |
| --- | --- | --- |
| Bayi sudah mempunyai RM atau belum | Cari duplikasi dahulu; gunakan pasien yang ada bila cocok | Bayi yang dirujuk kembali memakai RM sebelumnya |
| Nama bayi | Nama resmi jika ada; nama sementara mengikuti SOP RS | Bayi Ny. Rani — Kembar 1 |
| Tanggal, jam, tempat lahir | Dari catatan persalinan atau dokumen rujukan; jam tidak ditebak | 14-10-2026, 03:20 WIB |
| Jenis kelamin | Dari data kelahiran terverifikasi; ketidakpastian masuk jalur koreksi SOP | Perempuan |
| Kelahiran tunggal/kembar dan urutan | Bedakan urutan lahir dalam persalinan ini dari anak keberapa dalam keluarga | Kembar 2 dari 2; pemetaan multipleBirthInteger R1 diperiksa |
| Ibu dan episode ibu | Pasien ibu adalah hubungan identitas; episode ibu adalah konteks perawatan | Ibu diketahui, tanpa episode lokal untuk bayi rujukan |
| NIK bayi | Opsional jika belum terbit; jangan membuat angka fiktif | Belum tersedia |
| NIK ibu/wali | Wilayah identitas terkait, dengan tipe identifier yang benar | Tidak ditampilkan utuh di daftar umum |
| Alamat dan kontak | Saran dari ibu/orang tua, dikonfirmasi petugas | Kontak ayah bila ibu tidak dapat dihubungi |
| Wali sah dan penanggung biaya | Dicatat terpisah; orangnya dapat berbeda | Ayah penanda tangan; perusahaan penjamin |
| Berat, panjang, usia gestasi, metode lahir | Ambil dari catatan klinis jika ada; jangan dijadikan seluruhnya isian wajib admisi | Berat 2.950 gram; usia gestasi dari catatan bidan |
| Penjamin bayi dan rencana perawatan | Verifikasi mandiri; penempatan berasal dari keputusan tenaga klinis | Bayi ditanggung asuransi berbeda dari ibu |

R1 menetapkan kebutuhan tertentu untuk integrasi SATUSEHAT. Kebutuhan integrasi tidak boleh diterjemahkan menjadi larangan pelayanan ketika data kependudukan sementara belum tersedia. Jalur identitas sementara memerlukan SOP dan dukungan kontrak yang eksplisit.

### Bagaimana stepper berpengaruh?

Pilihan awal harus mengubah label, field, validasi, daftar tempat, dan dokumen berikutnya. Persentase selesai hanya membantu navigasi. Penjamin, pembayaran, consent, episode, serta penempatan mempunyai status tersimpan masing-masing.

Usulan urutan: konteks kelahiran → hubungan ibu dan identitas bayi → rencana perawatan/DPJP bayi → penjamin serta preferensi penagihan → deposit bila berlaku → penempatan dan reservasi → persetujuan wali → tinjau dan aktifkan → dokumen. Rinciannya dan pemetaan dari 12 langkah ada pada plan repair.

### Apakah bed hanya boleh ruang bayi?

Bed dewasa umum tidak ditawarkan untuk bayi. Namun rawat gabung perlu mengizinkan boks bayi di kamar ibu. Untuk perawatan terpisah, daftar dibatasi tempat neonatal dengan kemampuan yang sesuai. Isolasi neonatal memerlukan kombinasi kemampuan neonatal dan isolasi; label “isolasi” saja tidak cukup. Keputusan klinis tetap milik dokter/tim klinis.

### Apakah dua invoice bisa disatukan?

Bisa dijadikan satu kelompok penagihan/pembayaran sebagai target desain yang sudah diminta pengguna. Dua pasien, dua episode, dan rincian kedua invoice sumber tetap dapat ditelusuri. Dukungan pembayaran bersama belum terbukti tersedia pada kontrak settlement saat ini.

### Proses bisnis dan perubahan status yang direkomendasikan

**Tujuan:** admisi bayi yang identitasnya benar, perawatannya tepat, serta biaya dan hubungannya dengan ibu dapat ditelusuri.

**Pemicu:** kelahiran hidup terlapor oleh unit persalinan, permintaan rawat neonatal, rujukan luar, atau keputusan rawat ulang. Kelompok lahir mati tidak otomatis diproses sebagai pasien bayi hidup; jalur dokumentasinya harus mengikuti SOP klinis.

**Pelaku:** bidan/perawat/dokter mencatat dan memverifikasi kelahiran serta kebutuhan klinis; admisi memverifikasi identitas, kunjungan, penjamin dan tempat; wali memberikan persetujuan yang relevan; kasir menangani pembayaran; koder/tim penjamin menentukan klaim.

**Prasyarat:** fakta kelahiran/rujukan, identitas minimum atau jalur identitas sementara yang disetujui, serta kebutuhan perawatan dari tenaga klinis. Ketidaklengkapan administrasi ditangani dengan status dan tindak lanjut yang terlihat.

1. Terima laporan kelahiran atau permintaan rawat; jangan membuat pasien sebelum ada fakta kelahiran hidup.
2. Cari bayi yang sudah mempunyai RM untuk mencegah duplikasi.
3. Verifikasi ibu dan sumber kelahiran; ambil data lahir dari sumber klinis.
4. Simpan identitas bayi dan kontak wali, beserta hubungan pasien ibu yang diketahui.
5. Tentukan rencana lokasi dan DPJP bayi dari keputusan klinis.
6. Verifikasi penjamin bayi; rekam preferensi penagihan keluarga tanpa menjanjikan cakupan.
7. Reservasi sumber daya bayi yang memenuhi syarat; periksa ulang saat aktivasi/penempatan.
8. Rekam persetujuan wali sesuai SOP dan tinjau ringkasan sebelum aktivasi.
9. Aktifkan episode/penempatan sesuai kontrak; sediakan identitas dan dokumen bayi.
10. Kasir menagihkan terpisah atau mengelompokkan invoice yang layak ketika invoice tersedia.

Status episode yang sudah ada adalah Draft, Admitted, DischargePending, Closed, dan Cancelled pada BE Areas/HealthServices/InPatientManagement/Enums/InpEpisodeStatus.cs:5–11. Tabel ini merupakan target alur dan bukan transisi baru yang sudah diterapkan:

| Dari | Tindakan | Ke | Pelaku | Syarat dan contoh |
| --- | --- | --- | --- | --- |
| Belum ada episode | Siapkan admisi bayi | Draft | Admisi | Identitas dan DPJP sesuai kontrak; waktu lahir 03:20 dibedakan dari waktu input 04:05 |
| Draft | Aktifkan dan tempatkan | Admitted | Petugas berwenang | Kelayakan bed diperiksa ulang; boks yang sudah dipegang pasien lain ditolak |
| Draft | Batalkan | Cancelled | Petugas berwenang | Reservasi dilepas; RM tidak otomatis dihapus |
| Admitted | Transfer rawat gabung ke NICU | Admitted | Petugas sesuai kewenangan klinis/bed | Episode bayi tetap, lokasi historis ditutup dan lokasi baru dibuka |
| Admitted | Mulai proses pulang | DischargePending | Petugas berwenang | Berdasarkan bayi sendiri; ibu pulang tidak memicu ini |
| DischargePending | Tutup episode | Closed | Petugas berwenang | Mengikuti clearance dan syarat episode bayi; pembayaran kelompok tidak menggantikan keputusan pulang |

**Jalur tidak normal:** perubahan ibu setelah data tersimpan memerlukan pemeriksaan hubungan dan audit; bayi kembar mendapat RM/episode masing-masing; ibu pindah kamar tidak otomatis memindahkan bayi yang sedang di NICU; tagihan baru setelah pembayaran membutuhkan rekonsiliasi; kegagalan pembayaran sebagian menampilkan hasil per invoice dan tidak diulang membuta. Bayi tanpa ibu diketahui perlu jalur identitas sementara yang disetujui; backend saat ini mewajibkan MotherPatientId untuk IsNewborn=true sehingga jalur tersebut belum dapat dianggap tersedia.

**Hasil akhir:** subjek bayi dan riwayat perawatannya utuh; hubungan ibu bersifat dapat ditelusuri; kasir mengetahui saldo per invoice; pulang dan perubahan lokasi ibu tidak diam-diam mengubah episode bayi.

## 6. Temuan tambahan

### ISS-EPS-016-T1 — Hubungan pasien ibu dan episode ibu belum divalidasi bersama

**Jenis:** BUG/CONTRACT_MISMATCH. **Area:** BE. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-03. **Kenapa dicantumkan:** penautan ibu adalah dasar keselamatan identitas dan gejala utama.

BE InpEpisodeService.Corrections.cs:239–276 hanya memeriksa referensi episode, status Closed/Cancelled, diri sendiri, dan pasien berbeda. Tidak ada pencocokan terhadap MstPatient.MotherPatientId pada metode itu. Sementara daftar active-mothers di InpEpisodeService.Reads.cs:478–484 menerima seluruh pasien perempuan Admitted yang lolos query, tanpa fakta persalinan. Daftar calon boleh luas untuk pencarian, tetapi penyimpanan hubungan memerlukan konfirmasi dan pemeriksaan konsistensi. Contoh: bayi B yang master ibunya pasien A ditautkan ke episode pasien C harus ditolak atau melalui alur koreksi hubungan berwenang, bukan diterima diam-diam.

Metode yang sama juga dipanggil saat update admisi pada InpEpisodeService.cs:501–509. Karena Closed ditolak tanpa membedakan tautan baru dari referensi lama yang tidak berubah, memperbarui admisi bayi dengan referensi ibu yang sudah Closed berpotensi ditolak. Ini kesimpulan dari alur source; kejadian runtime belum diuji. Rekomendasi: pertahankan hubungan historis yang sah, dan periksa kelayakan ibu saat ini hanya untuk operasi yang memang memerlukannya, seperti rawat gabung baru.

### ISS-EPS-016-T2 — Kesalahan pencarian ibu disamarkan sebagai hasil kosong

**Jenis:** BUG. **Area:** FE. **Keparahan:** Medium. **Bukti:** SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-07. **Kenapa dicantumkan:** petugas tidak dapat membedakan “ibu tidak ditemukan” dari “server gagal”.

FE inpatient-admission-view.jsx:111–115 selalu meminta halaman 1 ukuran 50; :132–135 mencatat console error dan mengosongkan daftar. Tidak tersedia keadaan gagal yang terlihat pada blok tersebut. Respons lama setelah pencarian baru juga perlu diuji karena pembatalan permintaan tidak terlihat pada fungsi ini. Rekomendasi: tampilkan gagal/muat ulang, pemulihan pilihan tersimpan, pencarian dan pagination yang konsisten; uji respons terbalik sebelum menyebut perlombaan respons sebagai bug runtime.

### ISS-EPS-016-T3 — Pasien lama: source menawarkan bayi, desain mengatakan dinonaktifkan

**Jenis:** CONTRACT_MISMATCH/DESIGN_CHANGE. **Area:** FE/desain. **Keparahan:** High. **Bukti:** SUDAH-VERIFIKASI. **Perbaikan:** FIX-EPS-016-08. **Kenapa dicantumkan:** bayi rujukan dan rawat ulang dapat sudah mempunyai RM.

03-frontend-architecture.md:336 dan 05-skema-tampilan.md:398 menyebut bayi dinonaktifkan pada pasien lama. Source FE inpatient-admission-visit-category-step.jsx:233–246 justru merender seluruh INPATIENT_PATIENT_TYPE_OPTIONS tanpa disabled; daftar tersebut memuat newborn pada constants:240–243. Jalur ini tidak memakai MotherEpisodeSelector yang dipasang khusus PatientTypeStep pada view:259. Jangan menganggap mengikuti label bayi pada state sudah mengubah identitas pasien tersimpan. Rekomendasi: kategori/identitas diturunkan dari profil bayi yang dipilih, dengan konteks admisi neonatal terpisah dan kebijakan rawat ulang yang eksplisit.

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor

| ID | Hal yang belum terbukti | Kenapa dibutuhkan | Dampak |
| --- | --- | --- | --- |
| P-01 | Belum ada capture layar Pendaftaran dan hasil Pilih Bed sesudah memilih bayi | Source membuktikan logika, screenshot lanjutan akan memperjelas gejala runtime | Tidak menahan analisis; diperlukan saat UAT |
| P-02 | Apakah “ruang bayi saja” juga dimaksudkan melarang boks di kamar ibu | Aturan literal akan menutup rawat gabung | Rekomendasi saat ini: tempat bayi sesuai perawatan, termasuk boks kamar ibu |

### Untuk pemilik proses dan modul

| ID | Keputusan | Rekomendasi | Menahan |
| --- | --- | --- | --- |
| K-01 | Ruang lingkup awal: bayi lahir lokal, rujukan luar, dan rawat ulang | Desain mencakup semuanya; implementasi dapat bertahap dengan cabang yang jujur | FIX-01, 06, 08 |
| K-02 | Sumber fakta lahir, detail format nama/gelang, batas usia neonatal, dan urutan kembar | Nama awal “Bayi [Nama Ibu]” dan label gelang dengan nama ibu didukung rekap meeting; detail format, sumber klinis, kembar dan batas usia masih perlu ditetapkan | FIX-01, 02 |
| K-03 | Ibu tidak dikenal atau tidak mempunyai master pasien | Jalur identitas sementara dan rekonsiliasi berwenang; jangan buat episode ibu fiktif | FIX-03 untuk cabang khusus tersebut |
| K-04 | Kemampuan unit neonatal dan model boks rawat gabung | Boks sebagai bed tersendiri; kemampuan neonatal serta isolasi diverifikasi | FIX-04, 05 |
| K-05 | Urutan persetujuan terhadap aktivasi dan jalur kedaruratan | Persetujuan/pengecualian sesuai SOP, dokumen cetak menjadi aksi penutup | FIX-01, 06 |
| K-06 | Kebijakan paket/tarif boks, deposit, penjamin berbeda, alokasi parsial dan refund | Biaya tetap per subjek; aturan klaim dipisahkan dari pengelompokan pembayaran | FIX-09 |
| K-07 | Kontrak kelompok pembayaran lintas pasien dan pemilik implementasi Billing | Audit Billing lebih lanjut sebelum kontrak final; pertahankan referensi kedua invoice | FIX-09 |

**Tidak ditanyakan ulang:** kebutuhan dua invoice yang dapat disatukan sudah diberikan pengguna. Detail akuntansi dan alokasi yang belum diberikan tetap menjadi keputusan terbuka.

## 8. Catatan pola

ISSUE-EPS-002 menonaktifkan bayi pada rilis September karena pemilih ibu belum berfungsi. Daftar ibu kini tersedia, tetapi builder pasien masih membawa pembatasan lama. Pola akar masalahnya adalah mengaktifkan pilihan UI tanpa meninjau seluruh rantai identitas → episode → penempatan → dokumen → billing.

Kemampuan existing yang layak diperluas: master pasien newborn dan field lahir, hubungan MotherEpisodeId, filter IsForNewborn, engine kelayakan bed, histori penempatan, persetujuan wali, serta settlement dan alokasi satu invoice. Rekomendasi ini tidak memerlukan modul bayi baru hanya karena ada layar khusus; ownership tetap PatientManagement, InPatientManagement, ClinicalManagement, dan BillingManagement.

Tidak ditemukan bukti yang cukup untuk menyatakan fitur end-to-end bayi sudah siap. Hasil lama “lulus semua tipe pasien” tidak dipakai sebagai pengganti pembuktian ulang.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-14 | Analisis 5 butir dan 3 temuan tambahan; rujukan resmi diperiksa; dua invoice sumber mengikuti klarifikasi pengguna; source dan database tetap baca-saja | diagnose-module-issue |
| 2026-10-14 | Verifikasi akhir mencatat perubahan source dari pekerjaan lain, memperbarui hash/baris, dan menambahkan dampak referensi ibu Closed terhadap update bayi | diagnose-module-issue |
| 2026-10-14 | Rekap meeting pengguna dianalisis: enam bukti bayi, lima keterkaitan umum, batas penafsiran billing dan dampak pada desain. Penyusunan skema per stepper ditunda sesuai arahan pengguna; plan repair tidak diubah pada pembaruan ini | diagnose-module-issue |
| 2026-10-14 | Pengguna melanjutkan pembaruan stepper; plan repair bagian 3 diperinci untuk BAYI-S01–BAYI-S09. Status issue dan implementasi tetap terbuka; kebijakan yang belum diputuskan tidak dianggap disetujui | diagnose-module-issue |

## Pembaruan implementasi 14 Oktober 2026

Pengguna menyetujui perbaikan untuk admisi bayi pasien baru dan pasien lama. Source BE/FE serta migration sudah disiapkan. Lint dan lima skenario browser API tiruan lulus; packaging standalone tertahan ENOSPC dan migration belum diterapkan. Lihat [register rencana](../plan-repair/plan-repair-016-alur-admisi-episode-bayi.md), [laporan BE-RWI-231](../../task/report/backend/BE-RWI-231.md) dan [laporan FE-RWI-245](../../task/report/frontend/FE-RWI-245.md). Pernyataan source belum berubah pada uraian sebelumnya adalah keadaan saat diagnosis awal, bukan status pekerjaan terbaru.
