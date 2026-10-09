# PRD to MVP — Finishing Rawat Inap (Paritas Bisnis V1)

| Field | Nilai |
|---|---|
| PRD ID | `PRD-RWI-FINISHING-001` |
| Versi | `0.4` |
| Status | **`DRAFT v0.4` — seluruh isi sudah diputuskan lewat decision log.** Versi ini menyalin keputusan `RWI-DEC-163` s.d. `RWI-DEC-205` pada `docs/module-blueprints/rawat-inap/00-interview-decisions.md` revision `30`, termasuk klaster Pasca Operasi (`CAP-RWF-18` s.d. `CAP-RWF-23`) yang di v`0.3` masih usulan. v`0.4` sudah dinilai ulang oleh `requirement-completeness-gate` revision `1.9` (bagian 18 dokumen gate): 14 dari 15 slice Finishing `READY_FOR_DOMAIN_DESIGN`; admisi dari kamar pulih (`INP-S32`) `PARTIALLY_READY` karena satu aturan Billing, yaitu invoice tujuan biaya operasi dari kunjungan poliklinik atau ODC (`DEC-INP-018`, alias `RWI-OQ-114` butir b). Tiga butir terbuka hanya menahan implementasi, bukan desain: `RWI-OQ-108`, `RWI-OQ-114`, dan `RWI-OQ-115` (bagian 11.2). Dokumen ini belum ditinjau ulang sebagai dokumen utuh oleh pemilik dan **tidak** mengganti `baseline_requirement` blueprint (`RWI-DEC-165`) |
| Tanggal | v`0.1`: 1 Oktober 2026 (pagi). v`0.2` s.d. v`0.4`: 1 Oktober 2026 (malam) |
| Modul | Rawat Inap (`InPatientManagement`), dengan modul pendukung: Billing, Kamar Operasi, Gizi, Bank Darah, Laboratorium, Radiologi, Farmasi, Clinical Management, dan Master Data |
| Pemilik produk | **Muhammad Hamzah**, Product/Domain Owner Rawat Inap (`RWI-DEC-061`) |
| Pemilik modul tetangga | Billing: **Yasmina** (`RWI-DEC-154`). Kamar Operasi dan Gizi: **Ikbal Yulianto** (`RWI-DEC-190`). Bank Darah: **Sukma Giri Pratama** (`RWI-DEC-190`). Laboratorium dan Radiologi: **Yoga Aji** (`RWI-DEC-153`). Master Data: **seluruh tim**, tanpa satu pemilik (`RWI-DEC-193`) Farmasi: **belum tercatat** (`RWI-OQ-108`) |
| Dasar dokumen | (1) Audit paritas V1 vs Final 30 September 2026. (2) Impact scan `01-existing-capability-map.md` revision `1.6` bagian 19. (3) Dua Amendment Pass `grill-me` tanggal 1 Oktober 2026 dan catatan persetujuan pemilik modul tetangga. (4) Untuk klaster Pasca Operasi: rekaman layar sistem HiSys `docs/Modul-RS/Rawat-Inap/Pasca-Operasi-ke-Rawat-Inap.md` (bagian 2.3). (5) Amendment Pass penutupan gate `1.8`, sebelas pertanyaan (`RWI-DEC-194` s.d. `RWI-DEC-205`) |
| Hubungan dokumen | **Melengkapi**, tidak menggantikan, `PRD-RWI-V2-001` (`04-prd-to-mvp-final.md`) dan PRD Integrasi Rawat Inap ↔ Billing. PRD V2 mengatur tata letak ruang kerja, PRD Integrasi mengatur sambungan dasar ke Billing, sedangkan PRD ini mengatur kemampuan bisnis yang belum berfungsi. **Bila dokumen ini berbeda dari decision log, decision log yang berlaku** (`RWI-DEC-165`) |
| Jalur pengesahan | `grill-me` ✅ → `requirement-completeness-gate` revision `1.8` ✅ (Finishing `CAP-RWF-01` s.d. `16`) → Amendment Pass penutupan gate `1.8` ✅ → `requirement-completeness-gate` revision `1.9` ✅ (v`0.4`) → **amandemen blueprint lewat `design-business-module` (langkah berikutnya)** → `plan-module-delivery` → `build-module-backend` / `build-module-frontend` |

---

## 0. Cara membaca dokumen ini

Dokumen ini menjawab satu pertanyaan: **apa yang harus selesai supaya Rawat Inap QuilvianFinal siap dipakai rumah sakit dengan bisnis yang sama dengan V1, tetapi tanpa kelemahan V1.**

Semua nama pasien dan petugas di dokumen ini adalah **samaran**.

| Istilah | Arti |
|---|---|
| V1 | QuilvianV1, sistem lama yang alur bisnisnya sudah disetujui client. **Hanya dibaca**, tidak diubah |
| Final | QuilvianFinal, sistem baru yang akan dipakai rumah sakit. Satu-satunya tempat perubahan kode |
| Bangsal | Ruang rawat inap tempat perawat dan dokter bekerja |
| OK | Kamar operasi (instalasi bedah) |
| Folio | Catatan tagihan berjalan milik Billing per kunjungan, sebelum menjadi invoice |
| Invoice | Tagihan resmi yang dibayar pasien atau penjamin di kasir |
| Izin kasir | Status dari kasir bahwa tagihan pasien sudah beres atau dijamin. Di sistem disebut *financial clearance*, dengan nilai `PENDING`, `BLOCKED`, `CLEARED`, atau `REVOKED` |
| Keluar ruangan | Tindakan "catat pasien meninggalkan ruangan": pasien secara fisik meninggalkan bed, sehingga bed langsung kosong (`RWI-RULE-036`) |
| Penutupan episode | Tindakan administrasi terakhir yang membuat episode rawat inap berstatus `Closed` (`RWI-RULE-010`) |
| *Placeholder* | Layar yang hanya bertuliskan "Integrasi belum tersedia" dan tidak menyimpan apa pun |

Label status kemampuan memakai taksonomi baku Quilvian:

| Label | Arti untuk pembaca umum |
|---|---|
| `READY TO REUSE` | Sudah ada dan bisa dipakai apa adanya |
| `REUSE WITH ADAPTER` | Sudah ada di modul lain, tinggal disambungkan |
| `EXTEND` | Sudah ada sebagian, perlu ditambah |
| `REPAIR` | Sudah ada tetapi salah atau rusak |
| `MISSING` | Belum ada sama sekali |
| `CONFLICT` | Dua bagian sistem atau dua keputusan saling bertentangan |
| `UNKNOWN` | Belum bisa dipastikan tanpa uji di aplikasi berjalan |

### 0.1 Yang berubah dari v`0.1`

| Hal | v`0.1` | v`0.2` | Dasar |
|---|---|---|---|
| Tujuan produk nomor 3 | Pasien hanya bisa **pulang fisik** bila kasir memberi izin | **Episode hanya bisa ditutup** bila kasir memberi izin, atau supervisor menutup dengan alasan. Pasien boleh keluar ruangan setelah keputusan pulang, dengan peringatan bila kasir belum memberi izin | `RWI-DEC-186` |
| Arah data tagihan | Usulan: Billing menarik, outbox hanya jejak audit | Outbox tetap dipakai sebagai "ketukan pintu" tanpa data tarif. Billing membaca fakta sendiri, dan pesan baru "Published" setelah Billing mengonfirmasi | `RWI-DEC-166` |
| Sumber izin kasir | Usulan: milik Billing | **Diputuskan:** milik Billing, dibaca langsung, tanpa salinan. Webhook dihapus | `RWI-DEC-167` |
| Supervisor override | Alasan 20 karakter dan PIN, untuk pulang fisik | Hanya untuk **penutupan episode**: cukup permission dan alasan. **PIN dihapus** | `RWI-DEC-187` |
| Rupiah di Tagihan Pasien | Usulan: rupiah per item untuk pemegang izin | Subtotal per kelompok dan total berjalan. **Tidak pernah harga per item** | `RWI-DEC-170` |
| Catatan Keperawatan | Usulan susunan V1 | **Diputuskan** susunan V1, dengan narasi tetap entri CPPT | `RWI-DEC-172` |
| Pemakaian Alat | Usulan master alat | **Diputuskan**: master jenis alat di Master Data, pemakaian di Clinical Management | `RWI-DEC-179`, `RWI-DEC-180` |
| Penunjang Gizi dan Bank Darah | Pemesanan lewat modul pemilik | Ditambah pola instruksi dokter dan kolom verifikasi di modul pemilik | `RWI-DEC-171`, `RWI-DEC-188` |
| Pasien operasi | Usulan | **Diputuskan**: pra-operasi di OK, penandaan gambar tubuh, Obstetri sebagai jenis kasus, bed tetap selama operasi, dan penerima serah terima yang sah | `RWI-DEC-173` s.d. `RWI-DEC-177`, `RWI-DEC-189` |
| Serah terima transfer (`P2`) | Butuh pembukaan ulang `RWI-DEC-113` | **Diputuskan**: wajib, tetapi tidak menahan perpindahan bed | `RWI-DEC-182` |
| Resume Medis ODC | `CAP-RWF-17`, `FR-RWF-072`, `OD-RWF-09` | **Dihapus dari PRD ini.** Resume ODC tetap *placeholder* menurut `RWI-DEC-123`. Nomor lamanya tidak dipakai ulang | `RWI-DEC-183` |
| Keputusan terbuka `OD-RWF-01` s.d. `09` | Terbuka | Seluruhnya diputuskan atau dihapus (bagian 11) | `RWI-DEC-163` s.d. `RWI-DEC-192` |
| Temuan baru dari impact scan | — | Enam temuan baru masuk tabel 2.1 (nomor 30 s.d. 35) | Capability map bagian 19 |


### 0.2 Yang berubah dari v`0.2`

| Hal | v`0.2` | v`0.3` | Dasar |
|---|---|---|---|
| Titik tagih obat dan retur | Tafsiran agent, menunggu konfirmasi | **Diputuskan**: obat ditagih saat diserahkan; retur yang lolos pemeriksaan membatalkan tagihan obat itu | `RWI-DEC-195` |
| Isi biaya operasi | Belum ditetapkan | **Diputuskan**: tindakan ditagih lewat order tindakan yang ditandai selesai oleh OK; OK mengirim anestesi, sewa kamar operasi, serta bahan dan implan | `RWI-DEC-196` |
| Klaster Pasca Operasi | — | **Ditambahkan sebagai usulan**: enam kemampuan `CAP-RWF-18` s.d. `CAP-RWF-23`, `EPIC-RWF-09`, `BP-RWF-08`, dan keputusan terbuka `OD-RWF-10` s.d. `OD-RWF-14` | `RWI-DEC-197`, bukti HiSys |
| Temuan audit | 35 temuan | Tujuh temuan baru (nomor 36 s.d. 42) dari perbandingan HiSys dengan Final | `RWI-FACT-056` s.d. `RWI-FACT-058` |
| Keputusan yang masih terbuka | `RWI-OQ-102` dan dua tafsiran | Tafsiran titik tagih tertutup. Terbuka: pra-operasi saat ditunda (`DEC-INP-016`), WSD lebih dari satu selang (`DEC-INP-017`), pemilik Farmasi (`RWI-OQ-108`), dan `OD-RWF-10` s.d. `14` | Bagian 11.2 dan 11.3 |


### 0.3 Yang berubah dari v`0.3`

| Hal | v`0.3` | v`0.4` | Dasar |
|---|---|---|---|
| Klaster Pasca Operasi | Usulan, menunggu `OD-RWF-10` s.d. `14` | **Diputuskan seluruhnya.** `CAP-RWF-18` s.d. `22` masuk `P1`, `CAP-RWF-23` masuk `P2`. `EPIC-RWF-09` dan `BP-RWF-08` berlaku | `RWI-DEC-201` s.d. `RWI-DEC-205` |
| Admisi dari kamar pulih | Usulan pilihan A | Kamar pulih mengirim permintaan admisi; petugas admisi tetap menyelesaikan admisi berlangkah; **tidak ada admisi otomatis** | `RWI-DEC-201` |
| Surveilans infeksi luka operasi | Usulan, `P1` atau `P2` | `P1`, milik `ClinicalManagement`, diisi perawat sampai hari ke-15, ditinjau tim PPI, berhenti saat pasien keluar ruangan | `RWI-DEC-202` |
| Monitoring transfusi | `DEFERRED`, diusulkan dibuka | `P1`, per kantong, empat titik ukur, reaksi diberitahukan ke Bank Darah. Bagian alur transfusi lain tetap `DEFERRED` | `RWI-DEC-203` |
| Verifikasi order operasi | Usulan "Disetujui/Ditolak" | Status akhir `Rejected` (Ditolak) dari `Requested` dengan alasan wajib. Menyetujui = menjadwalkan, tanpa status "Disetujui". Bangsal memesan ulang sebagai kasus baru | `RWI-DEC-204` |
| Laporan transfer ruangan | Usulan `P2` | `P2`, permission khusus laporan, ekspor Excel tercatat di audit, koreksi salah catat ditandai | `RWI-DEC-205` |
| Pra-operasi saat kasus Ditunda | Terbuka (`DEC-INP-016`) | Wajib dikirim ulang dengan tanda vital terbaru dan dikonfirmasi ulang sebelum "Siap" (`FR-RWF-090`) | `RWI-DEC-199` |
| WSD lebih dari satu selang | Terbuka (`DEC-INP-017`) | Dicatat per selang dengan lokasi; pembacaan pertama dari sisa awal atau 0 ml (`FR-RWF-054`, `FR-RWF-058`) | `RWI-DEC-200` |
| Contoh dan UAT admisi dari kamar pulih | "Biaya operasi masuk invoice `RANAP`" | Invoice tujuan biaya operasi dari kunjungan poliklinik atau ODC **belum diputuskan** dan menunggu Yasmina | `RWI-AC-318`, `RWI-OQ-114` butir (b) |
| Bagian 11.3 | Lima keputusan terbuka | Tabel jawaban; tidak ada lagi keputusan desain yang terbuka | `RWI-DEC-198` |
| Yang masih terbuka | Empat butir gate dan lima `OD-RWF` | Hanya penahan implementasi (`RWI-OQ-108`, `114`, `115`), gerbang produksi klinis, dan konfirmasi prioritas `CAP-RWF-18`, `19`, `22` | Bagian 11.2 |
| Kriteria penerimaan | `AC-RWF-080` s.d. `087` berlaku bersyarat | Berlaku penuh, dan ditambah padanan `RWI-AC-307` s.d. `RWI-AC-329` | Decision log revision `30` |

---

## 1. Latar belakang

### 1.1 Masalah

| Sistem | Kelebihan | Kelemahan |
|---|---|---|
| V1 | Alur bisnis sudah cocok dengan kebutuhan operasional dan sudah dipresentasikan ke client | Backend rapuh. Contoh nyata: tagihan pemakaian alat dibuat tanpa harga (bagian 13.2) |
| Final | Backend lebih kuat: audit, versi data, hak akses, idempotensi | Banyak alur bisnis V1 belum berfungsi: menu masih *placeholder*, dan tagihan rawat inap tidak masuk kasir |

Roadmap Rawat Inap di Final sudah menandai banyak task backend ✅, tetapi audit menunjukkan tanda ✅ itu belum berarti fungsi bisnisnya berjalan. Contohnya, task pengiriman event ke Billing ditandai ✅ karena status pesan berubah "Published", padahal pesannya tidak pernah sampai ke Billing. Karena itu PRD ini memakai aturan ✅ yang baru (`RWI-DEC-168`, prinsip `PR-RWF-09`).

### 1.2 Versi source yang diaudit

| Repository | Branch | Audit v`0.1` | Impact scan v`0.2` (bagian 19) | HEAD saat v`0.2` ditulis |
|---|---|---|---|---|
| `NewQuilvianSystemBackend` (Final) | `MHamzah` | `c8e99ce5` | `c8e99ce5` | `425cfeae` — hanya dokumen, tidak menyentuh source |
| `QuilvianSystemFrontendDev` (Final) | `HamzahV2` | `7a83e8574` | `22ad67330` | `ee75e055b` — hanya styling, tidak menyentuh berkas yang diaudit |
| `QuilvianSystemBackendDev` (V1) | `QuilvianSta` | `4be1499c` | `4be1499c` | — |
| `QuilvianSystemFrontendDev` (V1) | `MHamzah` | `86408f245` | `86408f245` | — |

Bila SHA saat implementasi berbeda, temuan di bagian 2 wajib dicek ulang sebelum dijadikan task. Pemicu cek ulangnya ada di capability map bagian 19.9.

### 1.3 Tujuan produk

1. Perawat dan dokter bisa menjalankan **seluruh delapan menu keperawatan V1** tanpa menu kosong, kecuali Rehab Medik yang modulnya belum ada.
2. **Setiap layanan yang diberikan kepada pasien rawat inap muncul di tagihan kasir secara otomatis**, dengan harga dari master tarif, bukan diketik ulang.
3. **Episode rawat inap hanya bisa ditutup bila kasir sudah memberi izin, atau supervisor menutup dengan alasan tercatat.** Pasien boleh meninggalkan ruangan setelah dokter memutuskan pulang. Bila kasir belum memberi izin, perawat diperingatkan dan status kasir saat itu tercatat untuk ditindaklanjuti.
4. Kelemahan V1 yang sudah terbukti **tidak ikut disalin**.
5. **Pasien pasca operasi diterima dan dirawat di bangsal dengan informasi operasi yang lengkap**: ringkasan operasi terbaca di bangsal, infeksi luka operasi dipantau sampai hari ke-15, transfusi dipantau per kantong, dan pasien yang baru diputuskan rawat inap dari kamar pulih mendapat admisi yang benar.

**Contoh tujuan nomor 3.** Tn. Budi boleh pulang pukul 08.30. Pukul 09.05 perawat mencatat Budi keluar ruangan walaupun kasir belum memberi izin. Layar memperingatkan, perawat mengonfirmasi, bed langsung kosong, dan tarif kamar berhenti di jam itu. Keluarga melunasi pukul 09.40. Barulah pukul 09.45 petugas admisi dapat menutup episode.

---

## 2. Ringkasan hasil audit

### 2.1 Temuan per kemampuan

Status di bawah adalah keadaan source saat diaudit, **bukan** target. Kolom terakhir menyebut keputusan yang kini mengatur perbaikannya.

| No | Kemampuan | V1 | Final saat diaudit | Status | Diatur oleh |
|---:|---|---|---|---|---|
| 1 | Webhook izin pulang dari kasir | — | Bisa dipanggil **tanpa login** (`[AllowAnonymous]`), tanpa tanda tangan atau kunci rahasia | `REPAIR` — keamanan | `RWI-DEC-167`: dihapus |
| 2 | Sumber status izin kasir | Satu penanda `IsFinishedKasir` | Tiga tempat: `InpFinancialClearance`, `InpEpisode.ClearanceStatus`, `BilInpatientClearanceHandoff` | `CONFLICT` | `RWI-DEC-167`: satu sumber di Billing |
| 3 | Sinyal izin kasir dan jalur kepulangan | Kasir menandai selesai; daftar perawat dan kasir mengikuti penanda itu. Kepulangan fisik **tidak dikunci** status kasir (`RWI-FACT-051`) | Billing tidak pernah mengirim sinyal. Ada **dua jalur** keluar ruangan: `record-departure` tanpa syarat kasir dan `confirm-physical-discharge` dengan salinan status | `CONFLICT` | `RWI-DEC-186`: satu jalur keluar ruangan, gerbang kasir di penutupan |
| 4 | Invoice rawat inap terbentuk | Otomatis dari setiap layanan | Tidak pernah terbentuk otomatis. Hanya terbentuk bila kasir menambah biaya manual | `MISSING` | `RWI-DEC-166`, `RWI-DEC-192` |
| 5 | Tindakan, lab, radiologi, obat, dan konsul ke invoice | Otomatis | Tercatat di folio (produsen tidak membedakan jenis kunjungan), tetapi jembatan ke invoice menolak semua kunjungan selain rawat jalan (`NotOutpatient`) | `EXTEND` | `RWI-DEC-192` butir (a) |
| 6 | Tarif kamar | Kamar × jumlah hari | **Sudah ada** di Billing (`BKC-DEC-043`): dihitung dari data penempatan bed dan master kebijakan tarif kamar. Baru tampil bila invoice rawat inap sudah ada | `READY TO REUSE` setelah nomor 4 | `RWI-DEC-166` |
| 7 | Hitungan tarif kamar kedua | — | Layanan `InpatientRoomChargeCalculationService` dengan jam potong tertanam dan tarif cadangan Rp 500.000 / Rp 1.500.000. Hanya dipakai satu endpoint yang tidak dipanggil siapa pun | `CONFLICT` | `RWI-DEC-192` butir (e): dipensiunkan |
| 8 | Biaya administrasi | Tampil di Tagihan | Ada di Billing, tetapi **hanya dihitung bila invoice punya minimal satu item**. Tarif kamar bukan item, sehingga invoice yang baru berisi tarif kamar tidak mendapat biaya admin | `EXTEND` | `RWI-DEC-192` butir (c) |
| 9 | Pengiriman event Rawat Inap ke Billing (outbox) | — | Pengirim hanya menulis log lalu menandai "Published". Isi pesan membawa salinan data penempatan. Pesan yang tertinggal berstatus `Processing` tidak pernah diambil ulang | `REPAIR` | `RWI-DEC-166` |
| 10 | Tagihan Pasien di layar perawat | Rincian per item dengan rupiah | Empat jalur paralel, semuanya dari folio, tanpa tarif kamar dan biaya admin. Menampilkan rupiah kepada semua pemegang hak baca | `CONFLICT` | `RWI-DEC-170` |
| 11 | Penunjang: Lab dan Radiologi oleh perawat | Pesan + hasil | Backend sudah menerima pesanan perawat beserta dokter pemberi instruksi. Tombol pesan di layar perawat masih dikunci "menunggu BE-RWI-104" | `REPAIR` | `RWI-DEC-114`, `RWI-DEC-171` |
| 12 | Penunjang: Gizi | Pesan | *Placeholder*, padahal modul Gizi Final sudah lengkap. Penginput sudah tersimpan terpisah dari dokter peminta, tetapi belum ada tempat status verifikasi | `REUSE WITH ADAPTER` | `RWI-DEC-171`, `RWI-DEC-188` |
| 13 | Penunjang: Bank Darah | Pesan | *Placeholder*, padahal modul Bank Darah Final sudah lengkap. Kondisinya sama dengan nomor 12 | `REUSE WITH ADAPTER` | `RWI-DEC-171`, `RWI-DEC-188` |
| 14 | Penunjang: Hemodialisa | Pesan | Sudah berfungsi untuk dokter dan perawat | `READY TO REUSE` | — |
| 15 | Penunjang: Rehab Medik | Pesan | *Placeholder*; tidak ada modul backend | `MISSING` — ditunda | `RWI-DEC-108` |
| 16 | Pemesanan Ruangan Bedah (Bedah Operasi, Bedah Obgyn) | Ada | *Placeholder* di bangsal. Modul OK sudah bisa menerima kasus yang merujuk order tindakan pasien, tetapi belum mengenal jenis anestesi dan jenis layanan Obstetri | `EXTEND` | `RWI-DEC-175`, `RWI-DEC-176` |
| 17 | Catatan Pra-Operasi di bangsal | Ada | Tidak ada. OK hanya punya checklist keselamatan WHO | `MISSING` | `RWI-DEC-173`, `RWI-DEC-174` |
| 18 | Serah terima pasien dari OK kembali ke bangsal | Lewat form Transfer | OK bisa mengirim serah terima, tetapi Rawat Inap tidak membacanya. Penerimaan tidak memeriksa penerimanya, sehingga pengirim bisa menerima kirimannya sendiri | `EXTEND` | `RWI-DEC-177`, `RWI-DEC-189` |
| 19 | Biaya operasi ke tagihan | Dibuat saat booking | Pengiriman OK → Billing ditahan: "kontraknya belum tersedia" | `MISSING` | `RWI-DEC-191` butir (f), `RWI-DEC-192` |
| 20 | Catatan Keperawatan (6 sub-menu) | Ada | Arti berbeda: Final hanya catatan naratif CPPT | `CONFLICT` | `RWI-DEC-172` |
| 21 | Observasi WSD per shift | Ada | Hanya kategori cairan "Drain/WSD" berisi volume | `EXTEND` | `RWI-DEC-172` |
| 22 | Efek Samping Obat (ESO) | Ada | Backend ada, layar tidak ada | `REUSE WITH ADAPTER` | `RWI-DEC-172` |
| 23 | Diet Medis di bangsal | Ada | Backend Gizi punya diet per pasien, bangsal tidak punya layar | `REUSE WITH ADAPTER` | `RWI-DEC-178`, `RWI-DEC-188` |
| 24 | Pemakaian Alat (alat besar) | Ada, dengan tagihan | *Placeholder*; tidak ada master alat medis; master tarif belum bisa merujuk alat | `MISSING` | `RWI-DEC-179`, `RWI-DEC-180` |
| 25 | Katalog tindakan rawat inap | — | Memakai saringan rawat jalan (`IsAvailableForOutpatient`), sehingga tindakan khusus rawat inap tidak muncul | `REPAIR` | `FR-RWF-070` |
| 26 | Hak lihat rupiah di ringkasan Billing | — | Ditentukan dari nama peran (`Contains("Admin")`, `"Cashier"`) | `REPAIR` | `RWI-DEC-170`, `RWI-DEC-192` butir (f) |
| 27 | Serah terima klinis saat transfer | 9 bagian klinis | Hanya pindah bed; bagian klinis *placeholder* | `MISSING` | `RWI-DEC-182`, `RWI-DEC-189` |
| 28 | Resume Medis ODC | Ada | *Placeholder* | Di luar PRD ini | `RWI-DEC-183`: tetap *placeholder* menurut `RWI-DEC-123` |
| 29 | Transfusi darah | Komponen ada, tetapi dimatikan dari menu V1 | Tidak ada | `MISSING` — monitoring dibuka `P1`, sisanya ditunda | `RWI-DEC-203`; bagian 5.4 |
| 30 | Label jenis layanan invoice rawat inap | — | Invoice dibuat berlabel `"RANAP"`, tetapi empat titik penguncian ulang memeriksa `"INPATIENT"`, sehingga pencabutan izin kasir otomatis karena tagihan susulan tidak pernah terjadi | `REPAIR` | `RWI-DEC-192` butir (h) |
| 31 | PIN supervisor override | — | PIN hanya diperiksa tidak kosong, tidak pernah dicocokkan. Wewenang juga dibaca dari nama peran `"SuperAdmin"` | `REPAIR` | `RWI-DEC-187`: PIN dihapus, wewenang dari permission |
| 32 | Koreksi salah catat penempatan bed | — | Belum ada. Event `OCCUPANCY_CORRECTED` justru terbit pada setiap transfer biasa | `MISSING` | `RWI-DEC-157`, `RWI-DEC-192` butir (g) |
| 33 | Rawat Inap membaca tabel Billing langsung | — | Rincian tagihan bangsal (`billing-details`) membaca `BilFolio` dan `BilChargeLine` milik Billing | `CONFLICT` — batas modul | `RWI-DEC-102` butir (e), `RWI-DEC-170` butir (5) |
| 34 | Tagihan kosong tampil "Rp 0" | — | Layar Tagihan Pasien menampilkan "Rp 0" saat data tidak ada | `REPAIR` | `RWI-DEC-108` |
| 35 | Putar ulang episode aktif saat rilis | — | Tidak ada mekanisme putar ulang di Rawat Inap | `MISSING` | `RWI-DEC-169` |
| 36 | Ringkasan operasi untuk perawatan pasca bedah | HiSys: "Keterangan Tentang Operasi" lengkap (tim, anestesi, ASA, kamar pulih, detail tindakan) | Data sudah ada di modul OK: laporan operasi (diagnosis pasca bedah, temuan, komplikasi, perdarahan, drain/implan, rencana pasca bedah), catatan anestesi, dan kamar pulih. Bangsal tidak membacanya | `REUSE WITH ADAPTER` | `CAP-RWF-19`, `RWI-DEC-197` |
| 37 | Admisi rawat inap dari kamar pulih | HiSys: perpindahan dari OK ke bed **tidak diperlihatkan** | Keputusan kamar pulih "Inpatient" tidak membuat admisi apa pun | `MISSING` | `CAP-RWF-18`, `RWI-DEC-201` |
| 38 | Surveilans infeksi luka operasi | HiSys: Formulir A nosokomial (lama dan baru), hari ke-1 s.d. ke-15, kultur, serologi, tanda infeksi per lokasi | Tidak ada | `MISSING` | `CAP-RWF-20`, `RWI-DEC-202` |
| 39 | Monitoring transfusi darah | HiSys: tanda vital sebelum, 15 menit, 1 jam, 4 jam setelah darah masuk, dan reaksi | Tidak ada; transfusi hanya muncul sebagai jenis consent | `MISSING` | `CAP-RWF-21`, `RWI-DEC-203` |
| 40 | Verifikasi order operasi (Approve/Reject) | HiSys: verifikasi dengan keterangan, dan order ditolak bertanda merah | Tidak ada langkah verifikasi; kasus langsung dijadwalkan atau dibatalkan | `MISSING` (modul OK) | `CAP-RWF-22`, `RWI-DEC-204` |
| 41 | Laporan transfer ruangan | HiSys: per periode, kelas dan bed asal-tujuan, pencatat, ekspor Excel | Hanya riwayat penempatan per episode | `MISSING` | `CAP-RWF-23`, `RWI-DEC-205` |
| 42 | Kasus OK selesai bergantung pada bangsal | — | Kasus baru `Completed` setelah laporan operasi final, pasien keluar kamar pulih, **dan** serah terima diterima unit tujuan, sehingga biaya operasi ikut menunggu | `READY TO REUSE` — dengan konsekuensi | `RWI-DEC-196`, `FR-RWF-088`, `FR-RWF-089` |

### 2.2 Bukti source

Format bukti: `repository/path:baris@SHA`. Bukti untuk nomor 30 s.d. 35 dan pembaruan nomor 3, 8, 9, 10, 11, 12, 13, 16, dan 18 berasal dari capability map bagian 19 (`FIN-CAP-01` s.d. `FIN-CAP-33`).

| No | Bukti |
|---:|---|
| 1 | `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs:49-53@c8e99ce5` |
| 2 | `.../InPatientManagement/Models/InpEpisode.cs:68@c8e99ce5`; `.../InPatientManagement/Models/InpFinancialClearance.cs:19@c8e99ce5`; `.../BillingManagement/Billing/Models/BilInpatientClearanceHandoff.cs:12-81@c8e99ce5` |
| 3 | Jalur 1: `.../InPatientManagement/Controllers/InpatientDischargeController.cs:639-652@c8e99ce5` → `Services/InpDischargeService.Closure.cs:574-630`. Jalur 2: `InpatientDischargeClearanceController.cs:132-147@c8e99ce5` → `Services/InpatientClearanceGateService.cs:139-235`. V1: `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/Pendaftaran/Controllers/KunjunganController.cs:1837-1862@4be1499c`; `.../RawatInap/Controllers/BookingBedRanapController.cs:454-517@4be1499c` |
| 4 | `.../BillingManagement/Billing/Services/BillingInvoiceService.cs:1528-1547@c8e99ce5` — satu-satunya tempat invoice dibuat |
| 5 | `.../BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs:111-112@c8e99ce5`; produsen: `LaboratoryManagement/Services/LabSpecimenService.cs:554`, `RadiologyManagement/Services/RadStudyService.cs:490`, `PharmacyManagement/Services/PrescriptionDispensingService.cs:276@c8e99ce5` |
| 6 | `.../BillingManagement/Billing/Services/BillingCalculationService.cs:196-198, 607-741@c8e99ce5` |
| 7 | `.../BillingManagement/Billing/Services/InpatientRoomChargeCalculationService.cs:16-19, 242, 449@c8e99ce5` |
| 8 | `.../BillingManagement/Billing/Services/BillingCalculationService.cs:189-195, 515-540@c8e99ce5` |
| 9 | `.../InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs:67-70, 85-104, 129-145@c8e99ce5`; `Services/InpatientClearanceGateService.cs:215-224`; `Services/InpBedOccupancyService.cs:1156-1167` |
| 10 | `.../BillingManagement/Operational/Services/PatientBillingSummaryService.cs:95-189@c8e99ce5`; `.../InPatientManagement/Services/InpatientBillingQueryService.cs:56-175@c8e99ce5`; `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx:218@22ad67330` |
| 11 | `.../LaboratoryManagement/Models/LabOrder.cs:150-157@c8e99ce5`; `.../RadiologyManagement/Models/RadOrder.cs:120-127@c8e99ce5`; `.../nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:189-205@22ad67330` |
| 12–15 | `.../NutritionManagement/Services/NutritionOrderService.cs:146-190, 471-490@c8e99ce5`; `.../BloodBankManagement/Services/BbkBloodOrderService.cs:742, 834, 918-929@c8e99ce5`; `.../nursing-ancillary-section.jsx:35-39@22ad67330`; `.../physician-workspace/tabs/supporting-service/supporting-service-tab.jsx:164-167@22ad67330` |
| 16 | `.../OperatingRoomManagement/DTOs/OperatingRoomCaseDtos.cs:24-42@c8e99ce5`; `Enums/OperatingRoomEnums.cs:3-4`; `.../nursing-workspace/components/nursing-workspace-sections.jsx:90-99@22ad67330` |
| 17 | `.../OperatingRoomManagement/Enums/OperatingRoomEnums.cs:9-10@c8e99ce5` |
| 18 | `.../OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs:317-360@c8e99ce5`; `Controllers/OperatingRoomRecoveryController.cs:99-144` |
| 19 | `.../OperatingRoomManagement/Services/OperatingRoomIntegrationService.cs:28-40@c8e99ce5` |
| 20 | `.../nursing-workspace/sections/nursing-care/tabs/nursing-narrative-tab.jsx:81@22ad67330`; `src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js:61-72@22ad67330` |
| 21 | `.../ClinicalManagement/Enums/FluidSourceCategory.cs:35-36@c8e99ce5` |
| 22 | `.../ClinicalManagement/Controllers/PatientAllergyController.cs:455-463@c8e99ce5` |
| 23 | `.../NutritionManagement/Services/NutritionDietService.cs:180-217@c8e99ce5` |
| 24 | `.../nursing-workspace/components/nursing-workspace-sections.jsx:75-84@22ad67330`; `.../MasterData/Models/MstTariff.cs:19-42@c8e99ce5`; harga per penjamin `.../MasterData/Models/MstInsuranceTariff.cs:11-44@c8e99ce5` |
| 25 | `.../ClinicalManagement/Controllers/PatientProcedureController.cs:128-136@c8e99ce5`; `.../MasterData/Models/MstProcedure.cs:65, 67@c8e99ce5` |
| 26 | `.../BillingManagement/Billing/Controllers/InpatientClearanceController.cs:131-133@c8e99ce5` |
| 27 | `.../nursing-workspace/sections/transfer/nursing-transfer-form-panel.jsx:33@22ad67330` |
| 30 | `.../BillingManagement/Billing/Services/InpatientClearanceService.cs:263`, `BilConsumerHandoffService.cs:753`, `BillingInvoiceService.cs:1622, 1674, 2068@c8e99ce5` |
| 31 | `.../InPatientManagement/Services/InpatientClearanceGateService.cs:108-131@c8e99ce5`; `Controllers/InpatientDischargeClearanceController.cs:93-101` |
| 32 | `.../InPatientManagement/Services/InpBedOccupancyService.cs:1086-1100, 1156-1170@c8e99ce5` |
| 33 | `.../InPatientManagement/Services/InpatientBillingQueryService.cs:120-175@c8e99ce5` |
| 34 | `.../nursing-workspace/sections/billing/nursing-billing-section.jsx:41, 218, 279@22ad67330` |
| 35 | Pencarian `replay`, `redispatch`, `requeue` di `Areas/HealthServices/InPatientManagement` mengembalikan nol hasil `@c8e99ce5` |
| 36 | `.../OperatingRoomManagement/Models/OprExecutionRecord.cs:8-26@c8e99ce5`; `Controllers/OperatingRoomExecutionController.cs:25-29`; `Controllers/OperatingRoomRecoveryController.cs:25-66` |
| 37 | `.../OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs:146-213@c8e99ce5`; `Enums/OperatingRoomEnums.cs:16` |
| 38 | Pencarian surveilans/infeksi di `Areas` nol hasil `@c8e99ce5`; HiSys `01-Operasi-ke-Ranap.mp4` 01:37–04:12 |
| 39 | `.../ClinicalManagement/Enums/PatientConsentType.cs@c8e99ce5`; `.../BloodBankManagement/Models/*@c8e99ce5`; HiSys 13:15–14:32 |
| 40 | `.../OperatingRoomManagement/Enums/OperatingRoomEnums.cs:3@c8e99ce5`; HiSys 15:11, 17:08 |
| 41 | `.../InPatientManagement/Controllers/InpatientBedOccupancyController.cs:294@c8e99ce5`; HiSys 18:25 |
| 42 | `.../OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs:395-430@c8e99ce5` (`OPS-DEC-025`) |


### 2.3 Bukti HiSys untuk klaster Pasca Operasi

Klaster Pasca Operasi (nomor 36 s.d. 42) bersumber dari `docs/Modul-RS/Rawat-Inap/Pasca-Operasi-ke-Rawat-Inap.md`, yaitu ringkasan rekaman layar **sistem HiSys** (satu video berdurasi 23 menit 16 detik).

| Hal | Keterangan |
|---|---|
| Apa buktinya | Rekaman layar HiSys, bukan V1 dan bukan keputusan pemilik. Audio tidak ditranskripsikan, sehingga hanya isi layar yang terekam |
| Yang terlihat jelas | Surveilans infeksi pasca operasi, SOAP dokter dan perawat, histori tanda vital, monitoring transfusi, verifikasi order operasi, keterangan operasi, daftar verifikasi OK, laporan kunjungan OK, laporan transfer ruangan, katalog laporan rawat inap |
| Yang **tidak** terlihat | Perpindahan pasien dari kamar operasi atau kamar pulih ke bed rawat inap; pesan validasi; aturan wajib isi; hak akses tiap peran |
| Kedudukannya | Bukti praktik sistem lain. Kemampuan yang diturunkan darinya semula usulan (`RWI-DEC-197`), lalu diputuskan pemilik pada Amendment Pass penutupan gate `1.8` (`RWI-DEC-201` s.d. `RWI-DEC-205`). Yang berlaku adalah keputusan itu, bukan isi layar HiSys |
| Yang sengaja tidak dibawa | Katalog laporan rawat inap umum (BOR, sensus, kematian, register) karena bukan pasca operasi; pembagian jasa medis (*share*) dan diskon operasi karena milik modul jasa medis dan Billing; laporan kunjungan OK karena sudah ada di modul Kamar Operasi (`GET reports/operations`) |
| SHA-256 berkas | `ce43e811cf35b074bedd3d5d6bbc0c8fc8941dc5dbf49afa2991d3f96fc853c7` (`untracked` saat v`0.3` ditulis) |

---

## 3. Batas MVP Finishing

| Batas | Isi |
|---|---|
| **Titik mulai** | Pasien rawat inap berstatus `Admitted` dan menempati bed di bangsal |
| **Titik akhir** | Pasien sudah keluar ruangan, seluruh layanan selama dirawat tercantum di invoice kasir, kasir memberi izin, dan episode ditutup |
| **Di dalam MVP** | Kemampuan `P0` dan `P1` di bagian 5 |
| **Boleh menyusul dalam MVP bila waktu cukup** | Kemampuan `P2` |
| **Di luar MVP** | Kemampuan `DEFERRED` di bagian 5.4 |

**Definisi "selesai" untuk pemilik produk.** Satu pasien uji dirawat tiga hari dan menerima tindakan, lab, obat, pemakaian alat, serta operasi. Setelah dokter memutuskan pulang, perawat mencatat pasien keluar ruangan. Kasir lalu melihat semuanya di satu invoice tanpa mengetik ulang, memberi izin, dan petugas admisi menutup episode. Skenarionya ada di `UAT-RWF-01`.

---

## 4. Prinsip produk

| ID | Prinsip | Contoh penerapan |
|---|---|---|
| `PR-RWF-01` | Kemampuan bisnis V1 dipertahankan, cara simpan V1 tidak disalin | Menu Pemakaian Alat V1 dipertahankan, tetapi tagihannya wajib berharga dan tidak menghapus tagihan modul lain |
| `PR-RWF-02` | Pakai modul Final yang sudah ada sebelum membuat yang baru | Pemesanan Gizi memakai modul Gizi Final, bukan tabel baru di Rawat Inap |
| `PR-RWF-03` | Tagih saat layanan benar-benar terjadi | Operasi ditagih saat kasus OK berstatus `Completed`, bukan saat dipesan seperti V1 |
| `PR-RWF-04` | Harga selalu dari master tarif di server | Layar tidak pernah mengirim harga; tidak ada angka tarif tertanam di kode |
| `PR-RWF-05` | Satu fakta, satu tempat simpan | Status izin kasir hanya disimpan Billing. Rawat Inap membacanya langsung, tanpa salinan |
| `PR-RWF-06` | Pelaku diambil dari akun login | Nama perawat pengirim pra-operasi tidak diketik manual |
| `PR-RWF-07` | Hak akses lewat permission, bukan nama peran | `InpatientDischarge : CloseOverride`, bukan "peran bernama Supervisor" |
| `PR-RWF-08` | Tidak ada data tiruan | Menu yang belum siap tetap *placeholder*, tanpa contoh data, tanpa "Rp 0", dan tanpa pesan berhasil palsu (`RWI-DEC-108`) |
| `PR-RWF-09` | ✅ hanya bila terbukti di modul penerima | Task "kirim pesanan gizi" baru ✅ bila pesanan benar-benar muncul di layar Gizi, bukan sekadar respons 200 (`RWI-DEC-168`) |
| `PR-RWF-10` | **Izin kasir menjaga penutupan episode, bukan pintu kamar** | Pasien keluar → tarif kamar terkunci → kasir menghitung tagihan final → izin → episode ditutup (`RWI-DEC-186`) |
| `PR-RWF-11` | Penginput dan pemberi instruksi dicatat terpisah | Perawat yang menginput pesanan darah tercatat sebagai penginput, dan dokter yang menginstruksikan tercatat sebagai peminta yang wajib memverifikasi (`RWI-DEC-114`, `RWI-DEC-171`, `RWI-DEC-188`) |

**Kenapa `PR-RWF-10`.** Tarif kamar berjalan sampai pasien benar-benar keluar (`RWI-DEC-159`). Kalau izin kasir dijadikan syarat keluar ruangan, tagihan belum final saat kasir memberi izin. Contohnya, kasir menghitung pukul 09.30 (71 jam 30 menit = 3 hari = Rp 2.250.000), tetapi pasien baru keluar pukul 10.05 (72 jam 5 menit = 4 hari = Rp 3.000.000), sehingga tagihan naik setelah lunas (`RWI-FACT-050`). Dengan urutan "keluar dulu, baru tutup", tagihan kamar sudah final saat kasir menghitung.

---

## 5. Kemampuan dalam MVP

### 5.1 `P0` — menahan go-live

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-01` | Gerbang penutupan episode yang aman, dengan satu sumber izin kasir | `REPAIR` + `CONFLICT` | 1, 2, 3, 31 |
| `CAP-RWF-02` | Invoice rawat inap terbentuk otomatis dan menerima seluruh layanan klinis | `MISSING` + `EXTEND` | 4, 5, 6, 8, 30 |
| `CAP-RWF-03` | Status izin kasir dibaca langsung dari Billing, termasuk pencabutan otomatis | `REPAIR` | 3, 30 |
| `CAP-RWF-04` | Pembersihan jalur tagihan ganda dan pengiriman palsu, koreksi penempatan, dan putar ulang | `CONFLICT` + `REPAIR` + `MISSING` | 7, 9, 32, 35 |

### 5.2 `P1` — wajib dalam MVP Finishing

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-05` | Tagihan Pasien di bangsal dengan susunan rincian seperti V1 | `EXTEND` + `CONFLICT` | 10, 33, 34 |
| `CAP-RWF-06` | Penunjang Medis lengkap dari bangsal: Lab, Radiologi, Gizi, Bank Darah | `REPAIR` + `REUSE WITH ADAPTER` | 11, 12, 13 |
| `CAP-RWF-07` | Pemesanan Ruangan Bedah: Bedah Operasi dan Bedah Obgyn | `EXTEND` | 16 |
| `CAP-RWF-08` | Pasien operasi terhubung ke bangsal: status, pra-operasi, serah terima, tagihan | `MISSING` + `EXTEND` | 17, 18, 19 |
| `CAP-RWF-09` | Catatan Keperawatan dengan susunan 6 sub-menu V1 | `CONFLICT` | 20 |
| `CAP-RWF-10` | Observasi WSD per shift | `EXTEND` | 21 |
| `CAP-RWF-11` | Layar Efek Samping Obat | `REUSE WITH ADAPTER` | 22 |
| `CAP-RWF-12` | Diet Medis di bangsal | `REUSE WITH ADAPTER` | 23 |
| `CAP-RWF-13` | Pemakaian Alat (alat medis besar) dengan tagihan | `MISSING` | 24 |
| `CAP-RWF-14` | Katalog tindakan khusus rawat inap | `REPAIR` | 25 |
| `CAP-RWF-15` | Hak lihat rupiah berbasis permission | `REPAIR` | 26 |
| `CAP-RWF-18` | Admisi rawat inap dari kamar pulih lewat permintaan admisi dari OK | `MISSING` | 37 |
| `CAP-RWF-19` | Ringkasan operasi untuk perawatan pasca bedah di bangsal | `REUSE WITH ADAPTER` | 36 |
| `CAP-RWF-20` | Surveilans infeksi luka operasi hari ke-1 s.d. ke-15 | `MISSING` | 38 |
| `CAP-RWF-21` | Monitoring transfusi darah per kantong | `MISSING` | 29, 39 |
| `CAP-RWF-22` | Penolakan order operasi oleh OK, tampil di bangsal | `MISSING` (modul OK) | 40 |

Prioritas `P1` untuk `CAP-RWF-20` dan `CAP-RWF-21` dinyatakan `RWI-DEC-202` dan `RWI-DEC-203`. Untuk `CAP-RWF-18`, `19`, dan `22`, keputusan pemilik tidak menyebut prioritas; `P1` di sini mengikuti usulan v`0.3` yang tidak diubah pada pass penutupan gate, dan dikonfirmasi pemilik saat meninjau PRD (bagian 11.2 butir 13).

### 5.3 `P2` — sebaiknya ada, boleh menyusul

| ID | Kemampuan | Status awal | Temuan |
|---|---|---|---|
| `CAP-RWF-16` | Serah terima klinis saat transfer antarunit | `MISSING` | 27 |
| `CAP-RWF-23` | Laporan transfer ruangan (`RWI-DEC-205`) | `MISSING` | 41 |

`CAP-RWF-17` (Resume Medis ODC) **dihapus** dari PRD ini oleh `RWI-DEC-183`, dan nomornya tidak dipakai ulang.

### 5.4 `DEFERRED` — di luar MVP

| Kemampuan | Alasan | Perilaku selama MVP |
|---|---|---|
| Rehab Medik | Modul backend belum ada | Tetap *placeholder* sesuai `RWI-DEC-108` |
| Transfusi darah, selain monitoring | V1 sendiri mematikan menunya; pemilik alur klinis selebihnya belum ditetapkan (`DEC-INP-012`) | Tidak tampil. **Monitoring transfusi dibuka sebagai `P1`** (`CAP-RWF-21`, `RWI-DEC-203`). Yang tetap ditunda: permintaan transfusi oleh dokter di luar pesanan darah yang sudah ada, verifikasi dua petugas di samping tempat tidur, dan pengembalian kantong yang tidak terpakai |
| Notifikasi seketika (WebSocket) | Tidak menahan alur bisnis | Penyegaran berkala seperti sekarang |

### 5.5 Pemetaan ke sub-modul blueprint

Blueprint Rawat Inap tetap `COMPOSITE` empat sub-modul (`RWI-DEC-164`). Setiap kemampuan punya satu sub-modul pemilik, sehingga tidak ada kemampuan tanpa pemilik.

| Sub-modul | Kemampuan |
|---|---|
| `integrasi-billing` | `CAP-RWF-01`, `02`, `03`, `04`, `15` |
| `keperawatan` | `CAP-RWF-05`, `09`, `10`, `11`, `12`, `13`, `20`, `21` |
| `dokter-rawat-inap` | `CAP-RWF-06`, `14` |
| `episode-rawat-inap` | `CAP-RWF-07`, `08`, `16`, `18`, `19`, `22`, `23` |

Pemetaan klaster Pasca Operasi mengikuti `RWI-DEC-197` dan tidak diubah pass penutupan gate (`RWI-DEC-198`). Tidak ada sub-modul baru, karena datanya dimiliki Kamar Operasi, Clinical Management, Bank Darah, atau episode.


### 5.6 Klaster Pasca Operasi — keputusan dan persetujuan modul lain

Keenam kemampuan berasal dari bukti HiSys (bagian 2.3) dan sudah diputuskan pemilik. Kolom terakhir menyebut persetujuan modul lain yang masih ditunggu. Butir itu menahan implementasi bagian modul tersebut, bukan desain.

| ID | Kemampuan | Prioritas | Keputusan | Persetujuan modul lain |
|---|---|---|---|---|
| `CAP-RWF-18` | Admisi dari kamar pulih | `P1` (usulan v`0.3`) | `RWI-DEC-201` | OK dan Billing: `RWI-OQ-114` butir (a) dan (b) |
| `CAP-RWF-19` | Ringkasan operasi di bangsal | `P1` (usulan v`0.3`) | `RWI-DEC-197`; aturannya usulan standar (`FR-RWF-081`, `082`) yang dikonfirmasi saat desain (`RWI-DEC-194`) | Tidak perlu; hanya membaca endpoint OK yang sudah ada |
| `CAP-RWF-20` | Surveilans infeksi luka operasi | `P1` | `RWI-DEC-202` | Tidak ada. Isi formulir dan pemilik PPI menjadi gerbang produksi |
| `CAP-RWF-21` | Monitoring transfusi | `P1` | `RWI-DEC-203` | Bank Darah: `RWI-OQ-115` |
| `CAP-RWF-22` | Penolakan order operasi | `P1` (usulan v`0.3`) | `RWI-DEC-204` | OK: `RWI-OQ-114` butir (c) |
| `CAP-RWF-23` | Laporan transfer ruangan | `P2` | `RWI-DEC-205` | Tidak perlu |

---

## 6. Epic dan kebutuhan fungsional

Setiap kebutuhan diberi kriteria penerimaan (`AC-RWF-###`) yang bisa diuji di aplikasi berjalan. Nomor `FR` dan `AC` dari v`0.1` dipertahankan bila artinya masih sama, supaya keterlacakan tidak putus. Kolom "Keputusan" menunjuk baris decision log yang mengaturnya. Nomor baru v`0.4` memakai nomor kosong terdekat di dekade epiknya; bila dekade itu penuh, nomor diambil dari deret `090` ke atas sesuai urutan kemunculan. Kriteria penerimaan baru menyebut baris `RWI-AC` padanannya di decision log.

### EPIC-RWF-01 — Gerbang penutupan episode dan izin kasir (`CAP-RWF-01`, `CAP-RWF-03`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-001` | Setiap endpoint yang mengubah status kepulangan atau izin kasir **wajib** memerlukan login dan permission. Webhook `discharge-clearance/webhook` **dihapus**, karena Rawat Inap tidak lagi menunggu kiriman dari Billing | `RWI-DEC-167` butir 3 |
| `FR-RWF-002` | Status izin kasir **hanya disimpan Billing** (`BilInpatientClearanceHandoff`). Rawat Inap membacanya langsung setiap kali dibutuhkan dan tidak menyimpan salinan. `InpEpisode.ClearanceStatus` dipensiunkan | `RWI-DEC-167` butir 1, 2, dan 4 |
| `FR-RWF-003` | Layar kepulangan dan layar penutupan episode menampilkan status izin kasir terbaru paling lambat pada penyegaran berikutnya, yaitu 10 detik. Perawat hanya melihat status dan kendala tanpa rupiah | `RWI-DEC-160`, `RWI-DEC-167` butir 2 |
| `FR-RWF-004` | Tanda keuangan manual `InpFinancialClearance` dibekukan sebagai riwayat baca-saja dan **tidak pernah** membuka gerbang penutupan | `RWI-DEC-102` butir (d), `RWI-DEC-167` butir 4 |
| `FR-RWF-005` | **Penutupan tanpa izin kasir** hanya oleh pemegang permission `InpatientDischarge : CloseOverride`, dengan alasan yang tidak kosong dan tidak hanya tanda baca. Tanpa PIN dan tanpa pemeriksaan nama peran. Nama, waktu, alasan, dan status kasir saat itu tersimpan, lalu episode masuk laporan "ditutup tanpa kelayakan keuangan" | `RWI-DEC-187`, `RWI-RULE-009` |
| `FR-RWF-006` | **Keluar ruangan tidak ditahan kasir.** "Catat pasien meninggalkan ruangan" adalah satu-satunya tindakan keluar ruangan. Syaratnya hanya episode berstatus `DischargePending`. Pencatatnya petugas admisi, perawat pelaksana, kepala ruangan, atau supervisor. Bed dilepas seketika, dan akhir masa hunian = waktu keluar | `RWI-DEC-186` butir 1, `RWI-RULE-036`, `RWI-DEC-159` |
| `FR-RWF-007` | **Peringatan dan jejak.** Bila izin kasir bukan `CLEARED`, atau Billing tidak dapat dibaca, layar keluar ruangan menampilkan "Kasir belum memberi izin pulang" dan meminta konfirmasi sekali klik, tanpa alasan dan tanpa supervisor. Status kasir saat itu, pencatat, dan waktunya tersimpan. Episode semacam ini masuk daftar "pulang sebelum izin kasir" untuk kasir dan admisi | `RWI-DEC-186` butir 2 dan 3 |
| `FR-RWF-008` | **Gerbang penutupan.** Penutupan normal hanya bila izin kasir `CLEARED`, dibaca langsung dari Billing saat tombol ditekan. Bila Billing tidak dapat dibaca, penutupan normal ditolak. Bila kasir mencabut izin, penutupan ditolak sampai izin kembali `CLEARED` (penguncian ulang otomatis) | `RWI-DEC-186` butir 4 dan 5, `RWI-DEC-158`, `RWI-RULE-009` |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-001` | Endpoint lama `POST episodes/{episodeId}/discharge-clearance/webhook` tidak ada lagi (kode 404). Memanggil endpoint pengubah status kepulangan lain tanpa token ditolak dengan kode 401 |
| `AC-RWF-002` | Memanggil endpoint pengubah status kepulangan dengan akun tanpa permission ditolak dengan kode 403 |
| `AC-RWF-003` | Kasir menyetujui izin pulang pukul 10.00.00. Paling lambat pukul 10.00.10, layar penutupan menampilkan "Disetujui" dan tombol Tutup Episode aktif |
| `AC-RWF-004` | Kasir mencabut izin setelah pasien keluar ruangan. Tombol Tutup Episode terkunci kembali, banner merah menampilkan kendalanya, dan penutupan normal yang tetap dikirim ditolak |
| `AC-RWF-005` | Pencarian kode tidak menemukan lagi dua tempat simpan status izin kasir yang aktif bersamaan |
| `AC-RWF-006` | Tn. Contoh A berstatus `DischargePending`, izin kasir `PENDING`. Perawat mencatat keluar ruangan pukul 09.05. Layar memperingatkan, perawat mengonfirmasi, bed langsung `Available`, dan status kasir `PENDING` tersimpan atas nama perawat itu |
| `AC-RWF-007` | Saat Billing tidak dapat dibaca, pencatatan keluar ruangan tetap bisa dengan peringatan, sedangkan penutupan normal ditolak dengan pesan "Status kasir tidak dapat dibaca" |
| `AC-RWF-008` | Supervisor tanpa permission `CloseOverride` ditolak 403. Pemegang permission dengan alasan "..." ditolak. Dengan alasan yang berisi huruf, episode menjadi `Closed` dan muncul di laporan "ditutup tanpa kelayakan keuangan". Kontrak API tidak memuat isian PIN |
| `AC-RWF-009` | Pasien yang menempati bed sejak 1 Okt 10.00 dan dicatat keluar 4 Okt 09.05 mendapat tarif kamar 3 hari (kebijakan contoh pada 7.1.6). Hitung ulang sesudahnya tidak mengubah jumlah itu |

### EPIC-RWF-02 — Tagihan rawat inap masuk kasir (`CAP-RWF-02`, `CAP-RWF-04`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-010` | Invoice rawat inap (jenis layanan `RANAP`) **terbentuk otomatis** saat episode menjadi `Admitted`, lewat event `ADMISSION_CONFIRMED`, tanpa menunggu kasir menambah biaya manual | `RWI-DEC-156`, `RWI-DEC-166`, `RWI-DEC-192` |
| `FR-RWF-011` | Tindakan, pemeriksaan lab, radiologi, obat, dan konsultasi untuk kunjungan rawat inap **masuk ke invoice** lewat jembatan folio yang sama dengan rawat jalan, yang diperluas untuk `RANAP`. Titik tagihnya sama dengan rawat jalan: tindakan saat `Completed`, Lab saat spesimen diterima, Radiologi saat kualitas citra diputuskan, dan obat saat diserahkan farmasi. MAR tidak menjadi sumber tagihan. **Retur obat** yang lolos pemeriksaan farmasi membatalkan tagihan obat itu sebanyak jumlah yang kembali, tanpa menghapus baris aslinya; retur yang dinilai tidak layak tetap tertagih | `RWI-DEC-166` butir 5, `RWI-DEC-192` butir (a), `RWI-DEC-195` |
| `FR-RWF-012` | Tarif kamar dihitung Billing dari data penempatan bed dan master kebijakan tarif kamar (`BKC-DEC-043`). Billing menghitung ulang setiap menerima `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, atau `BED_RELEASED`. Rawat Inap tidak menghitung tarif sendiri | `RWI-DEC-166` butir 3 |
| `FR-RWF-013` | `InpatientRoomChargeCalculationService` beserta endpoint `occupancy-charges` dipensiunkan, termasuk seluruh tarif dan jam potong yang tertanam di kode | `RWI-DEC-192` butir (e) |
| `FR-RWF-014` | **Outbox jujur.** Event hanya berisi jenis kejadian, `EpisodeId`, `EncounterId`, ID sumber, nomor versi, waktu kejadian, dan `IdempotencyKey`. Event dilarang membawa tarif, rupiah, harga kelas, atau salinan data penempatan. Pesan baru "Published" setelah Billing mengonfirmasi terima. Pesan yang tertinggal berstatus `Processing` wajib dapat diambil ulang | `RWI-DEC-166` butir 2 dan 4, `RWI-DEC-161` |
| `FR-RWF-015` | Biaya administrasi rawat inap dihitung menurut kebijakan Billing, **termasuk** untuk invoice yang baru berisi tarif kamar | `RWI-DEC-192` butir (c) |
| `FR-RWF-016` | Kegagalan Billing tidak boleh menggagalkan penyimpanan klinis, admisi, penempatan bed, koreksi penempatan, maupun keluar ruangan. Pesan yang gagal tercatat "gagal, dicoba ulang" dan dikirim ulang tanpa dobel | `RWI-DEC-161`, `RWI-DEC-166` butir 1 |
| `FR-RWF-017` | **Putar ulang terkontrol** saat rilis. Setiap episode `Admitted` atau `DischargePending` yang punya bed aktif diantrekan ulang (`ADMISSION_CONFIRMED`, lalu `BED_OCCUPIED`) dengan `IdempotencyKey` stabil dan waktu hunian asli. Kasir mendapat daftar hasilnya, dan invoice yang sudah memuat biaya kamar manual ditandai "perlu diperiksa". Putar ulang hanya dijalankan dengan wewenang tertulis tersendiri | `RWI-DEC-169`, `RWI-DEC-192` |
| `FR-RWF-018` | Label jenis layanan invoice rawat inap diseragamkan, supaya tagihan susulan setelah izin `CLEARED` benar-benar mencabut izin secara otomatis | `RWI-DEC-192` butir (h) |
| `FR-RWF-019` | **Koreksi salah catat penempatan** (kamar, bed, kelas, atau waktu) hanya oleh kepala ruangan atau petugas admisi berwenang, selama invoice masih `OPEN`, dengan alasan wajib dan versi lama tersimpan. Koreksi memicu `OCCUPANCY_CORRECTED`. Uji pindah kamar dan koreksi salah catat terhadap penanda `IsSuperseded` wajib lulus sebelum `RWF-W1` dinyatakan selesai | `RWI-DEC-157`, `RWI-DEC-166`, `RWI-DEC-192` butir (g) |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-010` | Tn. Contoh A dinyatakan `Admitted` → invoice `RANAP` terbuka di kasir dalam waktu kurang dari 1 menit, tanpa kasir menambah biaya apa pun lebih dulu |
| `AC-RWF-011` | Perawat menandai tindakan "Pasang infus" `Completed` → baris tindakan muncul di invoice dengan harga dari master tarif |
| `AC-RWF-012` | Tindakan yang dibatalkan → barisnya dibatalkan (`Voided`) di invoice, tidak dihapus fisik. Farmasi menyerahkan 3 vial, lalu 1 vial diretur dan lolos pemeriksaan → tagihan obat itu menjadi 2 vial, dan pembatalannya merujuk nomor retur (`RWI-AC-308`) |
| `AC-RWF-013` | Pasien menempati kamar kelas 2 selama 2 hari 5 jam (contoh 7.1.6) → tarif kamar di invoice sama dengan hasil kebijakan tarif kamar yang aktif. Invoice yang hanya berisi tarif kamar tetap memuat biaya administrasi |
| `AC-RWF-014` | Tidak ada lagi angka tarif atau jam potong tertanam di kode Billing maupun Rawat Inap |
| `AC-RWF-015` | Billing dimatikan sementara, perawat tetap bisa menyimpan tindakan. Setelah Billing hidup lagi, baris tagihan muncul tanpa dobel |
| `AC-RWF-016` | Billing dimatikan, lalu pasien dinyatakan `Admitted`. Admisi tersimpan, dan pesan `ADMISSION_CONFIRMED` berstatus "gagal, dicoba ulang", **bukan** "Published". Setelah Billing hidup, tepat satu invoice `RANAP` terbuka, lalu pesan menjadi "Published" |
| `AC-RWF-017` | Isi setiap pesan outbox tidak memuat field tarif, rupiah, harga kelas, maupun salinan data penempatan |
| `AC-RWF-018` | Di lingkungan uji ada dua episode aktif: satu tanpa invoice, satu dengan biaya kamar manual. Setelah putar ulang, keduanya punya tepat satu invoice `RANAP` dengan tarif kamar sejak waktu masuk bed asli, dan hanya episode kedua yang ditandai "perlu diperiksa". Putar ulang kedua kali tidak mengubah apa pun |
| `AC-RWF-019` | Izin kasir sudah `CLEARED`, lalu farmasi menyerahkan obat pulang susulan. Izin otomatis menjadi `REVOKED`, dan penutupan episode ditolak sampai kasir menyetujui ulang |
| `AC-RWF-090` | Farmasi menyerahkan 3 vial obat untuk pasien rawat inap samaran → invoice `RANAP` memuat 3 vial pada saat penyerahan, tanpa input kasir (`RWI-AC-307`) |
| `AC-RWF-091` | Retur yang dinilai tidak layak kembali tidak mengubah tagihan. Perawat mencatat pemberian dosis di MAR, dan tidak ada baris tagihan baru yang muncul karenanya (`RWI-AC-309`) |

### EPIC-RWF-03 — Tagihan Pasien di bangsal (`CAP-RWF-05`, `CAP-RWF-15`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-020` | Menu Tagihan Pasien menampilkan **rincian per kelompok seperti V1**, urut: Kamar Rawat Inap, Tindakan, Penunjang Medis, Obat & Alkes, Pemakaian Alat, Operasi, dan Biaya Administrasi. Kelompok yang belum punya sumber data tidak tampil dan tidak diisi data contoh | `RWI-DEC-170` butir 1 |
| `FR-RWF-021` | Setiap baris tanpa rupiah memuat nama layanan, periode atau tanggal, dan jumlah unit. Contoh: "Kamar Melati 2 — 1 s.d. 4 Okt 2026 — 3 hari" atau "Nebulizer × 2" | `RWI-DEC-170` butir 2 |
| `FR-RWF-022` | Pemegang **izin rupiah** melihat subtotal rupiah per kelompok dan total berjalan. Harga per item dan perkalian "unit × tarif" **tidak pernah** ditampilkan di bangsal. Harga per item tetap tersedia di layar kasir | `RWI-DEC-170` butir 3, `RWI-DEC-137` |
| `FR-RWF-023` | Sumber rincian adalah **hitungan invoice Billing**. Rawat Inap tidak menghitung ulang dan tidak membaca tabel Billing secara langsung | `RWI-DEC-170` butir 5, `RWI-DEC-102` butir (e) |
| `FR-RWF-024` | Rupiah disaring di server: bagi pengguna tanpa izin, respons API tidak memuat satu pun field rupiah. Hak baca rincian dan hak lihat rupiah adalah dua permission terpisah, bukan nama peran. Hal yang sama berlaku pada endpoint ringkasan milik Billing | `RWI-DEC-170` butir 4 dan 6, `RWI-DEC-192` butir (f) |
| `FR-RWF-025` | Bila tagihan belum terbentuk atau data tidak dapat dibaca, layar menampilkan keadaan itu apa adanya, misalnya "Tagihan belum terbentuk". Layar **tidak** menampilkan "Rp 0" | `RWI-DEC-108` |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-020` | Perawat tanpa izin rupiah melihat "Kamar Melati 2 — 1 s.d. 4 Okt 2026 — 3 hari" tanpa angka rupiah, baik di layar maupun di respons API |
| `AC-RWF-021` | Petugas admisi pemegang izin rupiah melihat "Kamar: Rp 2.250.000", "Tindakan: Rp 320.000", dan "Total berjalan: Rp 2.570.000" (angka contoh). Tidak ada harga per item maupun "3 hari × Rp 750.000" |
| `AC-RWF-022` | Mengganti nama peran dari "Cashier" menjadi "Kasir Shift Pagi" tidak mengubah siapa yang boleh melihat rupiah |
| `AC-RWF-023` | Untuk episode yang invoice-nya belum terbentuk, layar menampilkan "Tagihan belum terbentuk", bukan "Rp 0" |

### EPIC-RWF-04 — Penunjang Medis lengkap (`CAP-RWF-06`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-030` | Perawat dapat memesan Lab dan Radiologi atas instruksi dokter. Tombol pesan perawat dibuka setelah `BE-RWI-104` terbukti berjalan | `RWI-DEC-114`, `RWI-DEC-153`, `RWI-DEC-171` |
| `FR-RWF-031` | Dokter dan perawat dapat memesan Konsultasi Gizi lewat modul Gizi Final. Rawat Inap tidak membuat tabel gizi | `RWI-DEC-171` |
| `FR-RWF-032` | Dokter dan perawat dapat memesan darah lewat modul Bank Darah Final. Rawat Inap tidak membuat tabel darah | `RWI-DEC-171` |
| `FR-RWF-033` | Setiap layanan penunjang menampilkan status pesanan dan hasil dari modul pemiliknya. Bangsal hanya membaca hasil | `RWI-DEC-171` butir 7 |
| `FR-RWF-034` | Status tanggungan penjamin per pemeriksaan ("Ditanggung" atau "Tidak Di-cover") tampil saat memilih pemeriksaan, seperti V1. **Harga** hanya tampil bagi pemegang izin rupiah | Tafsiran v`0.2`, lihat 11.2 butir 3 |
| `FR-RWF-035` | Rehab Medik tetap *placeholder*. Hemodialisa tetap seperti sekarang | `RWI-DEC-108` |
| `FR-RWF-036` | **Aturan pemesan seragam** untuk Lab, Radiologi, Gizi, dan Bank Darah. Dokter berpenugasan aktif (DPJP, konsulen, atau dokter jaga) memesan atas namanya sendiri, dan namanya diambil dari akun login. Perawat menginput atas instruksi dokter berpenugasan aktif. Rawat Inap memeriksa penugasan itu sebelum meneruskan pesanan; dokter tanpa penugasan ditolak dengan kode 403. Penginput selalu dari akun login | `RWI-DEC-114`, `RWI-DEC-171` butir 1–3 |
| `FR-RWF-037` | **Verifikasi dokter.** Pesanan yang diinput perawat berstatus "menunggu verifikasi" dan muncul di daftar "perlu diverifikasi" dokter pemberi instruksi. Pesanan yang dibuat dokter sendiri berstatus "tidak perlu". Untuk Gizi dan Bank Darah, status itu disimpan sebagai kolom di tabel modul pemiliknya, sama seperti Lab dan Radiologi | `RWI-DEC-171` butir 4, `RWI-DEC-188`, `RWI-DEC-191` |
| `FR-RWF-038` | Pesanan bukan tagihan. Tagihan muncul saat layanan dikerjakan atau diserahkan menurut modul pemiliknya. Pesanan yang dibatalkan sebelum diproses tidak menimbulkan tagihan. Pesanan ganda mengikuti modul pemilik, misalnya `confirm-duplicate` pada Bank Darah | `RWI-DEC-171` butir 5 dan 6 |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-030` | Perawat memesan "Darah Lengkap" dan memilih dokter pemberi instruksi → pesanan masuk worklist Laboratorium |
| `AC-RWF-031` | Perawat tidak bisa menyimpan pesanan Lab tanpa dokter pemberi instruksi; pesan: "Dokter pemberi instruksi wajib dipilih" |
| `AC-RWF-032` | Dokter memesan konsultasi gizi dari bangsal → pesanan tampil di layar Gizi `nutrition-management/orders`, dengan dokter peminta dari akun login dan status verifikasi "tidak perlu" |
| `AC-RWF-033` | Perawat memesan 2 kantong PRC atas instruksi dokter jaga berpenugasan aktif → pesanan tampil di layar Bank Darah `blood-bank-management/blood-orders` dengan status "menunggu verifikasi" |
| `AC-RWF-034` | Perawat memilih dokter tanpa penugasan pada pasien sebagai dokter peminta Gizi atau Bank Darah → ditolak dengan kode 403, dan pesanan tidak sampai ke modul tujuan |
| `AC-RWF-035` | Dokter membuka daftar "perlu diverifikasi" lalu memverifikasi pesanan darah → status, waktu, dan nama pemverifikasi tersimpan pada pesanan itu di modul Bank Darah |
| `AC-RWF-036` | Alur pesanan gizi, pesanan darah, dan diet yang sudah ada sebelum perubahan, misalnya dari poliklinik, menghasilkan data dan status yang sama seperti sebelumnya |

### EPIC-RWF-05 — Pemesanan ruang bedah dan pasien operasi (`CAP-RWF-07`, `CAP-RWF-08`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-040` | Menu Pemesanan Ruangan Bedah punya dua tab seperti V1: **Bedah Operasi** dan **Bedah Obgyn (SC/Caesar)** | `RWI-DEC-175` |
| `FR-RWF-041` | Perawat atau dokter bangsal memesan ruang bedah dengan merujuk **tepat satu order tindakan operasi** yang masih aktif pada episode itu. Pesanan membuat kasus di modul Kamar Operasi dengan konteks pasien dan kunjungan terisi otomatis. Tanpa order, pesanan ditolak dengan pesan "Tindakan operasi belum dipesan dokter" | `RWI-DEC-176` butir 1 |
| `FR-RWF-042` | Isian pemesanan minimal: tanggal dan jam yang diinginkan, tindakan operasi (dari order), dokter operator (dari order), jenis anestesi, prioritas, indikasi, sisi tubuh bila relevan, dan keterangan. Penginput diambil dari akun login | `RWI-DEC-176` butir 2 dan 3 |
| `FR-RWF-043` | Tab Bedah Obgyn membuat kasus OK dengan jenis layanan bedah **"Obstetri"** yang terisi otomatis dan tidak bisa diubah dari tab itu. Status, pra-operasi, serah terima, dan penagihannya sama dengan bedah umum. Ruang bersalin hanya dipakai bila modul OK mendukung lokasi itu | `RWI-DEC-175` |
| `FR-RWF-044` | Bangsal melihat daftar kasus operasi pasiennya dengan label: Diminta, Terjadwal, Siap, Sedang Operasi, Selesai, Ditunda, Dibatalkan, dan **Ditolak**. Untuk Ditunda, Dibatalkan, dan Ditolak, alasannya ikut tampil; untuk Ditolak juga penolak dan waktunya. Status "Diminta" belum menjamin ruang | `RWI-DEC-176` butir 4 dan 7, `RWI-DEC-204` butir 3 |
| `FR-RWF-045` | **Catatan Pra-Operasi** adalah fase persiapan baru milik kasus OK. Perawat bangsal mengisinya sebagai pengirim lewat Catatan Keperawatan → Catatan Pra-Operasi, lalu perawat OK mengonfirmasinya sebagai penerima. Isinya tanda vital, nyeri, checklist persiapan (verifikasi pasien, persiapan fisik, hasil pemeriksaan, persiapan lain), dan penandaan area operasi. Setiap butir punya konfirmasi pengirim dan penerima dari **dua akun berbeda**. Tanda vital dan nyeri dirujuk dari pencatatan yang sudah ada, lalu dibekukan sebagai potret saat dikirim. Kasus baru boleh "Siap" bila butir wajib lengkap di kedua sisi **dan** persetujuan tindakan serta anestesi sudah ada | `RWI-DEC-173`, `RWI-DEC-176` butir 5 |
| `FR-RWF-046` | **Serah terima pasca operasi.** Bangsal membaca serah terima yang dikirim OK untuk pasiennya. Penerima yang sah memegang permission "terima serah terima" yang terpisah dari permission kirim, akunnya berbeda dari pengirim, dan pasien sudah menempati bed aktif di unit tujuan. Bila unit tujuan berbeda dari unit bed sekarang, tombol Terima terkunci sampai pasien dipindahkan lewat Transfer Pasien. Penolakan wajib beralasan, lalu OK melengkapi dan mengirim ulang | `RWI-DEC-177`, `RWI-DEC-189` |
| `FR-RWF-047` | **Biaya operasi** masuk invoice saat kasus `Completed`, bukan saat dipesan, lewat dua sumber yang tidak tumpang tindih: (1) tindakan operasi ditagih sekali lewat order tindakan yang dirujuk kasus, yang ditandai `Completed` oleh OK; (2) OK mengirim komponen miliknya sendiri, yaitu jasa anestesi bila ada catatan anestesi, sewa kamar operasi, serta bahan dan implan yang terpakai, masing-masing dengan tarif dari master tarif. Bahan yang tertagih lewat OK tidak ditagih lagi lewat Farmasi. Kasus yang dibatalkan sebelum `Completed` tidak menimbulkan biaya | `RWI-DEC-176` butir 6, `RWI-DEC-191` butir (f), `RWI-DEC-196` |
| `FR-RWF-048` | **Penandaan area operasi** berupa satu atau lebih titik pada gambar tubuh, sisi (Kiri, Kanan, Bilateral, atau Tidak berlaku), dan keterangan lokasi. Sisi wajib cocok dengan sisi pada pesanan operasi. Foto tubuh pasien tidak disimpan | `RWI-DEC-174` |
| `FR-RWF-049` | **Bed selama operasi.** Selama di OK, pasien tetap tercatat menempati bed asal, dan tarif kamarnya tetap berjalan. Menerima serah terima tidak pernah memindahkan bed. Satu episode selalu punya tepat satu bed aktif | `RWI-DEC-177` butir 1 dan 6 |
| `FR-RWF-090` | **Pra-operasi setelah penundaan.** Saat kasus menjadi Ditunda, catatan pra-operasi yang sudah dikirim atau dikonfirmasi berstatus "perlu diperbarui"; versi lamanya tetap tersimpan sebagai riwayat. Saat kasus dijadwalkan ulang, perawat bangsal mengirim versi baru: tanda vital dan nyeri dirujuk ulang otomatis dari pencatatan terbaru lalu dibekukan sebagai potret baru, sedangkan butir checklist versi lama boleh dikonfirmasi ulang sekali klik atau diubah. Perawat OK dengan akun berbeda mengonfirmasi ulang versi baru, termasuk penandaan area operasi yang tetap wajib cocok dengan sisi pada pesanan. Gerbang "Siap" hanya membaca versi terbaru yang sudah dikonfirmasi kedua sisi. Berlaku untuk setiap penundaan, tanpa batas jam | `RWI-DEC-199` |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-040` | Perawat memesan bedah untuk pasien samaran yang sudah punya order tindakan operasi → kasus baru berstatus `Requested` tampil di daftar kasus OK, dengan dokter operator dari order dan penginput dari akun perawat |
| `AC-RWF-041` | OK menjadwalkan kasus → status di bangsal berubah menjadi "Terjadwal" beserta tanggal dan jamnya pada penyegaran berikutnya |
| `AC-RWF-042` | Catatan pra-operasi dengan butir wajib yang belum dikonfirmasi penerima → OK menampilkan kendala kesiapan, dan kasus tidak bisa "Siap" |
| `AC-RWF-043` | OK mengirim serah terima → perawat bangsal pemegang permission terima menerimanya → status `Accepted`, dengan penerima dan waktu dari akun login |
| `AC-RWF-044` | Kasus dibatalkan sebelum `Completed` → tidak ada biaya operasi di invoice. Kasus `Completed` → invoice memuat tepat satu baris tindakan operasi dari order tindakan, ditambah baris jasa anestesi, sewa kamar operasi, serta bahan atau implan, tanpa tindakan yang sama dua kali (`RWI-AC-310`, `RWI-AC-311`) |
| `AC-RWF-045` | Pesanan operasi bersisi "Kanan", tetapi penandaan memilih "Kiri" → butir penandaan tidak bisa dikonfirmasi, dengan pesan "Sisi penandaan berbeda dengan sisi pada pesanan operasi" |
| `AC-RWF-046` | Akun yang sama mencoba menjadi pengirim sekaligus penerima, baik pada butir pra-operasi maupun pada serah terima → ditolak |
| `AC-RWF-047` | OK mengirim serah terima ke ICU padahal pasien masih di bangsal → tombol Terima di ICU terkunci dengan pesan "Pindahkan pasien ke tempat tidur di unit ini lewat Transfer Pasien sebelum menerima serah terima". Setelah transfer berhasil, serah terima dapat diterima |
| `AC-RWF-048` | Pesan ruang bedah tanpa order tindakan operasi → ditolak dengan pesan "Tindakan operasi belum dipesan dokter" |
| `AC-RWF-049` | Selama kasus berstatus Sedang Operasi, bed asal tetap "Terisi", dan tarif kamar tetap dihitung untuk jam-jam operasi |
| `AC-RWF-092` | Bahan OK yang sudah tertagih lewat kasus operasi tidak muncul lagi sebagai tagihan Farmasi. Retur bahan itu yang lolos pemeriksaan membatalkan tagihannya (`RWI-AC-312`) |
| `AC-RWF-093` | Kasus dengan pra-operasi yang sudah dikonfirmasi kedua sisi diubah menjadi Ditunda → catatan berstatus "perlu diperbarui", versi lamanya tetap terbaca, dan kasus tidak dapat "Siap" dengan versi itu (`RWI-AC-313`) |
| `AC-RWF-094` | Setelah kasus dijadwalkan ulang, tanda vital dan nyeri terbaru terisi dari pencatatan terakhir, bukan dari versi lama. Setelah perawat OK dengan akun berbeda mengonfirmasi versi baru, kasus dapat "Siap" (`RWI-AC-314`) |

### EPIC-RWF-06 — Catatan Keperawatan susunan V1 (`CAP-RWF-09` s.d. `CAP-RWF-12`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-050` | Sub-tab Catatan Keperawatan berisi enam sub-menu V1, urut: **Spooling Cairan, Observasi Pengeluaran Cairan WSD, Sliding Scale, Daftar Pemberian Obat, Catatan Pra-Operasi, Diet Medis** | `RWI-DEC-172` butir 1 |
| `FR-RWF-051` | Setiap sub-menu hanya jendela ke data pemiliknya. Dilarang membuat salinan data (tabel 8.2). Data yang dicatat lewat satu jendela tampil sama di jendela lain | `RWI-DEC-172` butir 2 |
| `FR-RWF-052` | Daftar Pemberian Obat punya empat bagian seperti V1: Pemberian Obat (MAR), Riwayat Pemberian, Efek Samping, dan Rekonsiliasi Obat | `RWI-DEC-172` butir 1d |
| `FR-RWF-053` | Efek Samping memakai endpoint ADR yang sudah ada, dan hasilnya tampil di riwayat alergi atau reaksi pasien | `RWI-DEC-172` |
| `FR-RWF-054` | Observasi WSD dicatat **per selang** setiap shift: selang mana, jam awal, jam akhir, sisa cairan di tabung sekarang, dan volume yang dibuang bila tabung dikosongkan atau diganti. Sisa shift lalu **selang yang sama** dan jumlah bertambah **dihitung server**; hasil negatif ditolak. Setiap jumlah bertambah dicatat sebagai output "Drain/WSD" di Pengawasan Harian dengan rujukan ke selangnya, sehingga total harian tetap satu angka sedangkan rinciannya per selang | `RWI-DEC-172` butir 1b, `RWI-DEC-200` butir 2 dan 4 |
| `FR-RWF-055` | **Diet Medis** membaca dan menulis diet pasien di modul Gizi: tanggal, penetap, penginput, jenis diet, bentuk makanan, status diet, diagnosa, keterangan atau instruksi, dan riwayat. Penetap diet adalah dokter berpenugasan aktif atau ahli gizi. Perawat boleh menginput atas instruksi dokter berpenugasan aktif, lalu diet masuk daftar "perlu diverifikasi". Menghentikan diet wajib beralasan. Semua perawat bangsal dapat membaca diet aktif dan riwayatnya | `RWI-DEC-178`, `RWI-DEC-188` |
| `FR-RWF-056` | Catatan naratif perawat tetap entri CPPT berprofesi Perawat dengan penanda jenis catatan. Catatan itu ditulis dan dibaca lewat Catatan Terintegrasi atau SOAP Keperawatan, dengan saringan "Naratif Keperawatan". Tidak ada tabel baru | `RWI-DEC-172` butir 4, `RWI-DEC-140` |
| `FR-RWF-057` | Obat & Alkes kembali ke empat sub-tab V1: Resep, Resep Harian, Alat Kesehatan, dan Summary. MAR, Sliding Scale, dan Obat Bawaan pindah ke Catatan Keperawatan. Riwayat BMHP digabung ke Summary | `RWI-DEC-172` butir 3 |
| `FR-RWF-058` | **Selang WSD terdaftar** pada episode dengan lokasi atau label (misalnya "WSD kanan") dan waktu pasang; satu selang tetap kasus yang paling umum. Pembacaan pertama memakai sisa awal yang dicatat saat pemasangan, atau 0 ml bila tidak diisi. Selang yang dilepas ditutup dengan waktu lepas, dan pembacaan baru untuknya ditolak. Catatan drain pada laporan operasi hanya petunjuk dan tidak membuat selang otomatis. Data milik `ClinicalManagement` | `RWI-DEC-200` butir 1, 3, 5, dan 6 |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-050` | Enam sub-menu Catatan Keperawatan tampil berurutan seperti V1, tanpa *placeholder* |
| `AC-RWF-051` | Intake 500 ml yang dicatat lewat Spooling Cairan juga tampil di Pengawasan Harian, karena datanya satu. Pencarian database tidak menemukan tabel cairan kedua |
| `AC-RWF-052` | Observasi WSD satu selang: sisa shift lalu 200 ml, sisa shift ini 350 ml, tanpa pengosongan → jumlah bertambah 150 ml, dihitung server |
| `AC-RWF-053` | Efek samping yang dicatat dari dosis MAR tampil sebagai reaksi obat di riwayat alergi pasien |
| `AC-RWF-054` | Catatan naratif perawat yang ditulis sebelum perubahan menu tetap dapat dibaca di Catatan Terintegrasi dengan saringan "Naratif Keperawatan" |
| `AC-RWF-055` | Perawat menetapkan diet atas instruksi dokter berpenugasan aktif → layar Gizi menampilkan diet itu dengan penetap sang dokter dan penginput sang perawat, dan diet itu muncul di daftar "perlu diverifikasi" sang dokter. Penghentian diet tanpa alasan ditolak |
| `AC-RWF-056` | Obat & Alkes menampilkan empat sub-tab: Resep, Resep Harian, Alat Kesehatan, dan Summary |
| `AC-RWF-057` | Pasien samaran dengan WSD kanan dan kiri. Sisa shift lalu kanan 200 ml dan kiri 50 ml, sisa sekarang 350 ml dan 80 ml → server mencatat output 150 ml dan 30 ml, masing-masing merujuk selangnya, dan balance cairan bertambah 180 ml (`RWI-AC-315`) |
| `AC-RWF-058` | Pembacaan pertama selang baru tanpa sisa awal memakai 0 ml. Pembacaan untuk selang yang sudah dilepas ditolak (`RWI-AC-316`) |

### EPIC-RWF-07 — Pemakaian Alat medis besar (`CAP-RWF-13`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-060` | Ada master **Alat Medis** yang mencatat **jenis** alat, bukan unit aset bernomor seri. Contohnya ventilator, *high flow nasal cannula*, *syringe pump*, dan *infusion pump*. Isinya kode, nama, kategori, satuan tagih (per pemakaian, per jam, atau per hari), aturan pembulatan unit, dan status aktif | `RWI-DEC-179` butir 1 |
| `FR-RWF-061` | Tarif alat diatur di master tarif yang sama dengan tindakan dan obat, ditambah rujukan ke jenis alat. Harga per kelas memakai master tarif, sedangkan harga kontrak per penjamin memakai mekanisme `MstInsuranceTariff` yang sudah ada. Tidak ada tabel tarif kedua, dan layar tidak pernah mengirim harga | `RWI-DEC-179` butir 2, `RWI-DEC-180` butir 2, `RWI-FACT-049` |
| `FR-RWF-062` | Menu Pemakaian Alat punya dua tab seperti V1: **Order Alat Kesehatan** dan **History Alat Kesehatan** | `RWI-DEC-179` butir 3 |
| `FR-RWF-063` | Alat bersatuan waktu dicatat dengan **waktu mulai dan waktu selesai**, lalu jumlah unitnya dihitung server. Alat per pemakaian dicatat dengan jumlah | `RWI-DEC-179` butir 4 |
| `FR-RWF-064` | Dokter penanggung jawab dipilih dari dokter berpenugasan aktif, dan perawat pelaksana diambil dari akun login | `RWI-DEC-179` butir 4 |
| `FR-RWF-065` | Pemakaian masuk invoice rawat inap lewat jalur tagihan yang sama dengan tindakan | `RWI-DEC-179` butir 7 |
| `FR-RWF-066` | Koreksi atau pembatalan pemakaian hanya mengubah baris tagihan milik pemakaian itu sendiri, wajib beralasan, dan hanya selama invoice `OPEN`. Koreksi waktu menyimpan versi lama | `RWI-DEC-179` butir 5 dan 7 |
| `FR-RWF-067` | Alkes kecil dan BMHP tetap di Obat & Alkes → Alat Kesehatan (Farmasi). Pemakaian Alat khusus alat besar | `RWI-DEC-179` butir 9 |
| `FR-RWF-068` | **Kepemilikan data:** master jenis alat milik `MasterData` (prefix `Mst`); catatan pemakaian per pasien milik `ClinicalManagement` (prefix `Cli`); Rawat Inap hanya menyediakan layar, konteks episode, dan pemeriksaan penugasan dokter | `RWI-DEC-180` |
| `FR-RWF-069` | Pemakaian yang masih **Berjalan** saat pasien dicatat keluar ruangan ditutup otomatis pada waktu keluar, lalu ditandai "perlu diperiksa perawat" | `RWI-DEC-179` butir 6 |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-060` | Ventilator dipakai 1 Okt 08.00 s.d. 3 Okt 11.00, satuan per hari dengan pembulatan ke atas → invoice memuat 3 hari, dengan harga dari master tarif kelas pasien (contoh 7.4) |
| `AC-RWF-061` | Membatalkan satu pemakaian ventilator tidak mengubah baris tagihan alkes farmasi pasien yang sama |
| `AC-RWF-062` | Alat tanpa tarif untuk kelas pasien → pemakaian tetap tersimpan, baris tagihannya berstatus "tarif belum ada", kasir melihat peringatan, dan tidak ada harga karangan |
| `AC-RWF-063` | Pemakaian yang masih Berjalan saat pasien dicatat keluar ruangan tertutup otomatis pada waktu keluar, lalu ditandai "perlu diperiksa perawat" |
| `AC-RWF-064` | Pencarian source tidak menemukan tabel tarif alat yang terpisah dari master tarif, dan tidak menemukan tabel pemakaian alat di `InPatientManagement` |

### EPIC-RWF-08 — Perbaikan pendukung (`CAP-RWF-14`, `CAP-RWF-16`)

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-070` | Katalog tindakan di ruang kerja rawat inap memakai penanda `IsAvailableForInpatient`, bukan `IsAvailableForOutpatient`. Catatan untuk desain: saringan `IsDoctorAction` saat ini juga menyembunyikan tindakan khusus perawat, dan perlu ditinjau saat desain | Masukan desain (`RWI-DEC-165` butir 1) |
| `FR-RWF-071` | (`P2`) **Serah terima klinis saat transfer antarunit.** Setiap perpindahan bed ke unit lain otomatis membuat satu dokumen serah terima berstatus "Belum dikirim"; pindah bed di unit yang sama tidak. Isinya sembilan bagian V1: kondisi pasien (SOAP), GCS, tanda vital, nyeri dan risiko jatuh, balance cairan, barang yang diserahkan, instruksi khusus, pelaksana, dan penerima. Nilai klinis dirujuk lalu dibekukan sebagai potret. Dokumen tidak menahan perpindahan bed. Selama belum diterima, kedua unit melihat penanda "Serah terima tertunda". Penerima sah mengikuti `FR-RWF-046`. Data milik `ClinicalManagement` | `RWI-DEC-182`, `RWI-DEC-189` |

`FR-RWF-072` (Resume Medis ODC) **dihapus** oleh `RWI-DEC-183`, dan nomornya tidak dipakai ulang.

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-070` | Tindakan bertanda "hanya rawat inap" muncul di katalog bangsal dan ruang kerja dokter rawat inap, tetapi tidak muncul di poliklinik |
| `AC-RWF-071` | Pasien dipindahkan dari bangsal ke ICU. Bed langsung pindah, satu dokumen serah terima "Belum dikirim" lahir, dan kedua unit melihat "Serah terima tertunda" sampai perawat ICU menerimanya. Pindah bed di dalam unit yang sama tidak melahirkan dokumen |
| `AC-RWF-072` | Perawat ICU menolak serah terima tanpa alasan → ditolak. Dengan alasan, dokumen menjadi Ditolak, lalu pengirim dapat mengirim ulang |


### EPIC-RWF-09 — Pasien pasca operasi di bangsal (`CAP-RWF-18` s.d. `CAP-RWF-23`)

Kebutuhan di bawah sudah diputuskan (`RWI-DEC-201` s.d. `RWI-DEC-205`). `FR-RWF-081`, `082`, dan `088` tetap usulan standar dari `RWI-DEC-197` dan dikonfirmasi saat desain (`RWI-DEC-194`). Bagian yang mengubah modul OK atau Bank Darah baru boleh diimplementasikan setelah `RWI-OQ-114` dan `RWI-OQ-115` dijawab.

| ID | Kebutuhan | Keputusan |
|---|---|---|
| `FR-RWF-080` | **Permintaan admisi dari kamar pulih.** Bila kamar pulih menyimpan keputusan "rawat inap" atau "ICU" untuk pasien yang belum punya episode rawat inap aktif (operasi elektif dari poliklinik, IGD, atau ODC), pasien itu muncul di daftar permintaan admisi lengkap dengan kunjungan asal, kasus OK, dan dokter operator. **Tidak ada admisi otomatis.** Petugas admisi menyelesaikan admisi berlangkah yang sudah ada: penjamin, kelas, DPJP, deposit, dan bed, dengan aturan jenis kelamin dan isolasi tetap berlaku. Kunjungan asal dirujuk seperti pola alih IGD. Tarif kamar mulai saat bed ditempati | `RWI-DEC-201` butir 1 s.d. 3, `RWI-RULE-029`, `RWI-DEC-156` |
| `FR-RWF-089` | **Batas permintaan admisi.** Permintaan untuk pasien yang sudah punya episode rawat inap aktif ditolak dan diarahkan ke serah terima biasa (satu pasien satu episode aktif). OK dapat membatalkan permintaan dengan alasan selama admisi belum selesai, misalnya bila pasien ternyata boleh pulang. Permintaan yang belum ditindaklanjuti tampil di daftar pantau admisi beserta lamanya menunggu, karena selama itu kasus OK belum `Completed`. Serah terima pasca operasi baru dapat diterima setelah pasien menempati bed di unit tujuan (`FR-RWF-046`) | `RWI-DEC-201` butir 4 s.d. 7, `RWI-RULE-035` |
| `FR-RWF-081` | **Ringkasan operasi di bangsal.** Ruang kerja dokter dan perawat menampilkan ringkasan operasi pasien dari modul OK secara baca-saja: tindakan, diagnosis pasca bedah, temuan, komplikasi, perdarahan, catatan drain dan implan, rencana pasca bedah, jenis anestesi, skor dan keputusan kamar pulih, serta instruksi serah terima. Rawat Inap tidak menyimpan salinan (`PR-RWF-05`) | Usulan standar (`RWI-DEC-197`) |
| `FR-RWF-082` | Pengguna yang boleh membaca ringkasan operasi adalah pengguna yang berhak atas episode pasien itu, lewat permission baca kasus OK. Ringkasan hanya tampil bila laporan operasi sudah final; laporan yang masih draft tampil "Laporan operasi belum final" | Usulan standar (`RWI-DEC-197`) |
| `FR-RWF-083` | **Surveilans infeksi luka operasi.** Satu formulir per kasus operasi pasien rawat inap, lahir saat kasus OK `Completed`. Satu template berversi menggantikan "Formulir A lama" dan "Formulir A baru" HiSys. Isi minimal: indikator harian hari ke-1 s.d. ke-15 (suhu ≥ 38 °C, drainase, pus, perforasi, fistula), kultur (ya/tidak, tanggal, hasil), serologi (HBsAg, Anti HCV: positif/negatif), dan tanda infeksi per lokasi (nyeri, merah, bengkak, pus, menggigil, suhu tinggi, hari ke, keterangan). Data milik `ClinicalManagement` | `RWI-DEC-202` butir 1 dan 2 |
| `FR-RWF-084` | Hari ke-1 surveilans dihitung dari tanggal operasi selesai: operasi selesai 1 Okt berarti hari ke-1 jatuh pada 2 Okt. Suhu harian dibaca dari tanda vital yang sudah dicatat, tidak diketik ulang | `RWI-DEC-202` butir 3 |
| `FR-RWF-091` | **Pengisi dan peninjau surveilans.** Perawat bangsal yang merawat pasien mengisi setiap hari dari akun login. Tim PPI pemegang permission tinjau tersendiri dapat menandai "dicurigai infeksi luka operasi" beserta catatan; perawat tanpa permission itu tidak dapat. Koreksi isian berversi dengan alasan | `RWI-DEC-202` butir 4 dan 6 |
| `FR-RWF-092` | **Surveilans berhenti saat pasien keluar ruangan** sebelum hari ke-15, dengan status "berhenti — pasien pulang hari ke-N". Isian baru ditolak, dan formulir tetap tampil di daftar PPI. Tindak lanjut setelah pulang berada di luar PRD ini | `RWI-DEC-202` butir 5 |
| `FR-RWF-085` | **Monitoring transfusi,** satu catatan per kantong darah yang sudah diserahkan Bank Darah kepada pasien itu. Kantong dirujuk dari data penyerahan Bank Darah (nomor kantong, jenis komponen) dan tidak diketik; kantong yang belum diserahkan tidak dapat dipilih. Isinya jam darah diterima di bangsal; tekanan darah, suhu, dan nadi sebelum transfusi serta 15 menit, 1 jam, dan 4 jam setelah darah masuk; reaksi transfusi; dan perawat pelaksana dari akun login. Reaksi yang dicatat otomatis menjadi pemberitahuan di Bank Darah berisi pasien, kantong, jenis reaksi, dan waktu. Data milik `ClinicalManagement` | `RWI-DEC-203` butir 1, 2, dan 4 |
| `FR-RWF-093` | **Ketertiban titik ukur transfusi.** Titik ukur yang terlewat tetap kosong dan ditandai terlambat; nilai tidak boleh diisi mundur tanpa keterangan. Volume darah yang masuk tetap dicatat sebagai intake di Pengawasan Harian dan tidak disalin. Koreksi berversi dengan alasan | `RWI-DEC-203` butir 3, 5, dan 6, `RWI-DEC-149` |
| `FR-RWF-086` | **Order operasi ditolak.** Petugas OK memverifikasi order bangsal sebelum menjadwalkannya. Menyetujui berarti menjadwalkan (`Requested` → `Scheduled`); tidak ada status "Disetujui". Menolak menghasilkan status akhir `Rejected` (Ditolak), hanya dari `Requested`, dengan alasan wajib dan penolak dari akun login. Bangsal melihat status, alasan, penolak, dan waktunya. Kasus Ditolak tidak dapat dijadwalkan, diubah, atau dihidupkan lagi; bangsal memesan ulang sebagai kasus baru yang merujuk order tindakan yang sama. Kasus Ditolak tidak menimbulkan biaya, dan laporan OK dapat memisahkannya dari kasus yang dibatalkan | `RWI-DEC-204` |
| `FR-RWF-087` | **Laporan transfer ruangan** (`P2`) per periode dari linimasa penempatan bed, tanpa tabel baru. Isi per baris: waktu transfer, No. RM, nama pasien, kelas dan bed asal, kelas dan bed tujuan, alasan, dan pencatat. Saringan: periode (wajib), unit asal atau tujuan, dan kelas. Koreksi salah catat (`FR-RWF-019`) ditandai berbeda dari transfer. Laporan hanya untuk pemegang permission khusus laporan rawat inap; ekspor Excel adalah aksi dengan permission tersendiri dan tercatat di audit | `RWI-DEC-205` |
| `FR-RWF-088` | **Serah terima pasca operasi yang tertunda** tampil di daftar pantau bangsal dan OK, karena kasus OK dan biaya operasinya baru selesai setelah bangsal menerima serah terima (temuan 42). Batas jam tampilnya dapat diatur | Usulan standar (`RWI-DEC-197`), sejalan `RWI-DEC-201` butir 6 |

| ID | Kriteria penerimaan |
|---|---|
| `AC-RWF-080` | Kamar pulih memutuskan rawat inap untuk pasien samaran dari poliklinik → pasien muncul di daftar permintaan admisi dengan rujukan kunjungan asal dan kasus OK, tetapi tidak ada episode yang terbentuk sebelum petugas admisi menyelesaikannya (`RWI-AC-317`) |
| `AC-RWF-088` | Setelah admisi selesai dan pasien menempati bed, serah terima dapat diterima, kasus OK menjadi `Completed`, dan tarif kamar dihitung sejak bed ditempati (`RWI-AC-318`) |
| `AC-RWF-089` | Permintaan admisi untuk pasien yang sudah punya episode rawat inap aktif ditolak. Permintaan yang dibatalkan OK dengan alasan hilang dari daftar (`RWI-AC-319`) |
| `AC-RWF-081` | Perawat bangsal membuka ringkasan operasi pasien pasca bedah dan melihat diagnosis pasca bedah, komplikasi, drain, dan rencana pasca bedah dari laporan OK, tanpa tombol ubah |
| `AC-RWF-082` | Laporan operasi masih draft → ringkasan menampilkan "Laporan operasi belum final" |
| `AC-RWF-095` | Kasus OK pasien samaran menjadi `Completed` pada 1 Okt → satu formulir surveilans lahir, dan hari ke-1 jatuh pada 2 Okt (`RWI-AC-320`) |
| `AC-RWF-083` | Suhu 38,5 °C yang dicatat di tanda vital pada hari ke-2 menandai indikator suhu hari ke-2 tanpa diketik ulang. Tim PPI dapat menandai "dicurigai infeksi luka operasi", sedangkan perawat tanpa permission tinjau tidak dapat (`RWI-AC-321`) |
| `AC-RWF-096` | Pasien dicatat keluar ruangan pada hari ke-5 → surveilans berhenti dengan status "berhenti — pasien pulang hari ke-5", isian baru ditolak, dan formulir tetap tampil di daftar PPI (`RWI-AC-322`) |
| `AC-RWF-084` | Perawat membuka monitoring transfusi untuk pasien samaran. Hanya kantong yang sudah diserahkan Bank Darah kepada pasien itu yang dapat dipilih. Nilai sebelum, 15 menit, 1 jam, dan 4 jam tersimpan bersama nama perawat dari akun login (`RWI-AC-323`) |
| `AC-RWF-097` | Perawat mencatat reaksi "menggigil, demam" pada titik 15 menit → pemberitahuan berisi pasien, kantong, reaksi, dan waktu muncul di Bank Darah (`RWI-AC-324`) |
| `AC-RWF-098` | Titik ukur 1 jam yang tidak diisi tampil terlambat. Mengisinya belakangan tanpa keterangan ditolak (`RWI-AC-325`) |
| `AC-RWF-085` | Petugas OK menolak kasus berstatus Diminta tanpa alasan → penolakan ditolak. Dengan alasan "Hasil lab pra-operasi belum ada", kasus menjadi Ditolak, dan bangsal melihat status, alasan, penolak, serta waktunya (`RWI-AC-326`) |
| `AC-RWF-099` | Kasus Ditolak tidak dapat dijadwalkan atau diubah. Bangsal dapat memesan kasus baru yang merujuk order tindakan yang sama, dan invoice tidak memuat biaya dari kasus yang ditolak (`RWI-AC-327`) |
| `AC-RWF-086` | Pemegang permission laporan memilih periode 1–7 Okt → laporan menampilkan setiap transfer dengan kelas dan bed asal-tujuan, alasan, dan pencatat; koreksi salah catat bertanda "koreksi"; dan ekspor Excel tercatat di audit (`RWI-AC-328`) |
| `AC-RWF-100` | Pengguna tanpa permission laporan rawat inap ditolak dengan kode 403, apa pun nama perannya (`RWI-AC-329`) |
| `AC-RWF-087` | Serah terima pasca operasi yang belum diterima lewat batas jam yang diatur tampil di daftar pantau bangsal dan OK |

---

## 7. Proses bisnis

### 7.1 `BP-RWF-01` — Tagihan rawat inap terbentuk otomatis

1. **Tujuan.** Semua layanan pasien rawat inap tercatat di satu invoice tanpa diketik ulang.
2. **Pelaku.** Petugas admisi mengesahkan admisi. Dokter dan perawat memberi layanan. Farmasi menyerahkan obat. Kasir memeriksa dan memfinalkan invoice. Billing menghitung secara otomatis.
3. **Pemicu.** Episode berstatus `Admitted`.
4. **Prasyarat.** Master tarif kamar, tindakan, pemeriksaan, obat, dan alat sudah terisi. Ada satu kebijakan tarif kamar yang aktif.
5. **Langkah utama.**
   1. Petugas admisi mengesahkan admisi. Rawat Inap menyimpan admisi dan mengantrekan `ADMISSION_CONFIRMED`.
   2. Billing menerima pesan itu, membuka invoice `RANAP`, lalu mengonfirmasi terima. Barulah pesan berstatus "Published".
   3. Perawat mengonfirmasi pasien menempati bed (`BED_OCCUPIED`). Masa hunian mulai dihitung.
   4. Setiap tindakan selesai, spesimen lab diterima, kualitas citra radiologi diputuskan, obat diserahkan, pemakaian alat dicatat, dan operasi `Completed`, baris tagihan masuk invoice lewat jembatan folio.
   5. Setiap kali invoice dihitung, Billing menghitung ulang tarif kamar dari linimasa bed dan biaya administrasi, termasuk untuk invoice yang baru berisi tarif kamar.
   6. Perawat mencatat pasien keluar ruangan (`BED_RELEASED`). Masa hunian berakhir, dan tarif kamar menjadi final.
   7. Kasir memeriksa invoice, menerima pembayaran atau jaminan, memfinalkan invoice, lalu memberi izin kasir (`BP-RWF-02`).
6. **Aturan bisnis.**
   - Harga dari master tarif sesuai kelas, dan harga kontrak per penjamin dari `MstInsuranceTariff`.
   - Layanan yang hanya dipesan belum ditagih.
   - Tarif kamar mengikuti kebijakan aktif: menit minimum, panjang periode, cara pembulatan sisa, dan saat tarif dibaca.
   - Event outbox hanya berisi penanda kejadian, tanpa tarif atau rupiah.
7. **Perubahan status.** Lihat tabel di bawah.
8. **Jalur tidak normal.**
   - Tarif belum ada → baris tetap tercatat dengan penanda "tarif belum ada", dan invoice tidak bisa difinalkan sampai tarifnya diisi.
   - Billing gangguan → pesan "gagal, dicoba ulang", dikirim ulang tanpa dobel. Pencatatan klinis tidak terganggu.
   - Layanan dibatalkan → baris dibatalkan, tidak dihapus.
   - Obat diretur dan lolos pemeriksaan farmasi → tagihan obat itu dibatalkan sebanyak jumlah yang kembali, merujuk nomor retur, tanpa menghapus baris aslinya. Retur yang dinilai tidak layak tetap tertagih (`RWI-DEC-195`).
   - Penempatan salah catat → kepala ruangan atau admisi mengoreksi selama invoice `OPEN`; Billing menghitung ulang (`FR-RWF-019`).
   - Episode yang sudah aktif saat rilis → putar ulang terkontrol (`FR-RWF-017`).
9. **Hasil akhir.** Invoice `RANAP` berisi semua layanan dengan tarif kamar final dan siap dibayar.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Admisi disahkan | Invoice `OPEN` | Sistem (Billing menerima `ADMISSION_CONFIRMED`) | Episode `Admitted` |
| Pesan outbox "menunggu" | Billing mengonfirmasi terima | "Published" | Sistem | Billing benar-benar memproses |
| Pesan outbox "menunggu" | Billing tidak menjawab | "Gagal, dicoba ulang" | Sistem | — |
| Baris tagihan "menunggu sinkron" | Billing menerima | Baris `Recognized` | Sistem | Tarif ditemukan |
| Baris `Recognized` | Layanan dibatalkan | Baris `Voided` | Sistem, mengikuti pembatalan klinis | Invoice masih `OPEN` |
| Invoice `OPEN` | Kasir memfinalkan | Invoice final | Kasir | Tidak ada baris "tarif belum ada" |
| Invoice final | Butuh koreksi | Adjustment | Supervisor kasir | Alasan wajib |

#### 7.1.6 Contoh hitungan tarif kamar

Kebijakan aktif (contoh angka, bukan kebijakan rumah sakit): periode 1.440 menit (24 jam), minimum 1.440 menit, pembulatan sisa **ke atas**. Tarif kamar kelas 2 Rp 750.000 per periode.

Pasien menempati bed 1 Okt 2026 10.00 sampai dicatat keluar ruangan 3 Okt 2026 15.00. Masa hunian 2 hari 5 jam = 3.180 menit.

Unit tertagih = pembulatan ke atas dari 3.180 ÷ 1.440 = pembulatan ke atas dari 2,21 = **3 unit**.

Tarif kamar = 3 × Rp 750.000 = **Rp 2.250.000**.

Bila kebijakan memakai pembulatan proporsional, unitnya 2,21 dan tarifnya 2,21 × Rp 750.000 = Rp 1.656.250. Angka pastinya selalu mengikuti master kebijakan yang aktif. Tidak ada aturan jam yang ditulis di kode.

### 7.2 `BP-RWF-02` — Keputusan pulang, keluar ruangan, izin kasir, dan penutupan episode

1. **Tujuan.** Bed cepat kosong begitu pasien pergi, tagihan kamar final saat kasir menghitung, dan episode baru ditutup setelah urusan kasir beres, kecuali supervisor memutuskan lain dengan alasan.
2. **Pelaku.** Dokter DPJP memutuskan pulang. Perawat pelaksana, kepala ruangan, petugas admisi, atau supervisor mencatat pasien keluar ruangan. Kasir memberi atau mencabut izin. Petugas admisi atau supervisor menutup episode. Supervisor pemegang `CloseOverride` menutup tanpa izin kasir bila perlu.
3. **Pemicu.** Dokter DPJP memutuskan pasien boleh pulang.
4. **Prasyarat.** Invoice `RANAP` ada (`BP-RWF-01`).
5. **Langkah utama.**
   1. Dokter mencatat keputusan pulang. Episode menjadi `DischargePending`.
   2. Keluarga datang menjemput. Perawat mencatat pasien keluar ruangan. Bila izin kasir belum `CLEARED`, layar memperingatkan "Kasir belum memberi izin pulang" dan perawat mengonfirmasi. Status kasir saat itu tersimpan.
   3. Bed langsung `Available`. Masa hunian dan tarif kamar terkunci di jam keluar (`BED_RELEASED`).
   4. Kasir menghitung tagihan final, menerima pembayaran atau jaminan, lalu menyetujui izin kasir di Billing.
   5. Layar penutupan membaca izin itu langsung dari Billing. Tombol Tutup Episode aktif.
   6. Petugas admisi menutup episode. Sistem memeriksa izin kasir sekali lagi saat tombol ditekan, beserta syarat penutupan lain (`RWI-RULE-010`).
6. **Aturan bisnis.**
   - Satu sumber status izin kasir, yaitu Billing. Rawat Inap tidak menyimpan salinan.
   - Keluar ruangan tidak pernah ditahan kasir; penutupan episode selalu ditahan kasir.
   - Endpoint pengubah status wajib login dan permission.
   - Akhir masa hunian kamar = waktu keluar ruangan (`RWI-DEC-159`).
   - Penutupan tanpa izin kasir hanya lewat permission `InpatientDischarge : CloseOverride` dan alasan, tanpa PIN.
7. **Perubahan status.** Lihat tabel di bawah.
8. **Jalur tidak normal.**
   - Kasir mencabut izin, misalnya karena obat pulang susulan → tombol Tutup Episode terkunci lagi dan banner merah tampil.
   - Billing tidak dapat dibaca → keluar ruangan tetap bisa dengan peringatan, sedangkan penutupan normal ditolak dengan pesan "Status kasir tidak dapat dibaca".
   - Keluarga pergi tanpa membayar → episode tetap `DischargePending`, muncul di daftar pantau (`RWI-RULE-023`) dan daftar "pulang sebelum izin kasir". Hanya supervisor yang dapat menutupnya, dengan alasan. Piutangnya ditindaklanjuti Billing.
   - Pasien meninggal atau kabur → mengikuti `RWI-RULE-037`, yang belum final.
9. **Hasil akhir.** Pasien sudah keluar, bed sudah kosong, tagihan final dan beres (atau tercatat sebagai pengecualian supervisor), dan episode `Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| Episode `Admitted` | Keputusan pulang | `DischargePending` | DPJP | — |
| `DischargePending`, bed terisi | Catat keluar ruangan | `DischargePending`, bed `Available`, status kasir saat itu tersimpan | Petugas admisi, perawat pelaksana, kepala ruangan, supervisor | Peringatan dan konfirmasi bila izin kasir bukan `CLEARED` |
| Izin kasir `PENDING` | Kasir menyetujui | `CLEARED` | Kasir, menurut aturan Billing | Tagihan lunas atau dijamin |
| `CLEARED` | Kasir mencabut, atau tagihan susulan masuk | `REVOKED` | Kasir, atau sistem Billing | Alasan dicatat Billing |
| `REVOKED` atau `BLOCKED` | Kasir menyetujui ulang | `CLEARED` | Kasir | Kendala selesai |
| `DischargePending` | Tutup episode | `Closed` | Petugas admisi, supervisor | Izin kasir `CLEARED` dibaca langsung, dan syarat `RWI-RULE-010` lain terpenuhi |
| `DischargePending` | Tutup tanpa izin kasir | `Closed`, ditandai "ditutup tanpa kelayakan keuangan" | Pemegang `InpatientDischarge : CloseOverride` | Alasan wajib |

**Contoh lengkap.** Tn. Budi masuk bed Melati 2 Bed 3 pada 1 Okt 10.00.

| Waktu (4 Okt) | Yang terjadi | Status episode | Bed | Izin kasir |
|---|---|---|---|---|
| 08.30 | dr. Andi memutuskan Budi boleh pulang | `DischargePending` | Terisi | `PENDING` |
| 09.05 | Ns. Siti mencatat Budi keluar ruangan dan mengonfirmasi peringatan | `DischargePending` | **`Available`** | `PENDING` (tercatat atas nama Siti) |
| 09.15 | Kasir menghitung tagihan final: kamar 3 hari Rp 2.250.000 | `DischargePending` | — | `PENDING` |
| 09.40 | Keluarga melunasi | `DischargePending` | — | `CLEARED` |
| 09.45 | Petugas admisi menutup episode | `Closed` | — | `CLEARED` |

**Contoh pengecualian.** Pak Joko pulang atas permintaan sendiri pukul 22.00 tanpa melunasi. Esok pagi pukul 07.30, supervisor Ns. Rina menutup episode lewat "Tutup tanpa izin kasir" dengan alasan "APS malam hari, keluarga berjanji melunasi hari ini, sudah dihubungi kasir". Laporan memuat nama Rina, pukul 07.30, alasannya, dan status kasir `PENDING`.

### 7.3 `BP-RWF-03` — Pasien operasi di rawat inap

1. **Tujuan.** Pasien rawat inap yang perlu operasi dipesankan ruang bedah dari bangsal, dioperasi di OK, kembali dengan serah terima, dan biayanya masuk tagihan rawat inap.
2. **Pelaku.** Dokter operator membuat order tindakan operasi. Perawat atau dokter bangsal memesan ruang dan menyiapkan pasien. Petugas OK menjadwalkan dan melaksanakan. Perawat OK menerima catatan pra-operasi dan mengirim serah terima balik. Perawat unit tujuan pemegang permission terima menerima serah terima.
3. **Pemicu.** Dokter membuat order tindakan operasi.
4. **Prasyarat.** Order tindakan operasi aktif pada episode. Pasien punya episode aktif.
5. **Langkah utama.**
   1. Perawat membuka Pemesanan Ruangan Bedah, lalu memilih tab Bedah Operasi atau Bedah Obgyn.
   2. Perawat memilih order tindakan operasi dan mengisi pemesanan. Kasus OK terbentuk berstatus Diminta, dengan jenis Obstetri bila dari tab Bedah Obgyn.
   3. OK menjadwalkan kasus. Status di bangsal menjadi Terjadwal.
   4. Perawat bangsal mengisi Catatan Pra-Operasi dan penandaan area operasi, lalu mengonfirmasi sebagai pengirim.
   5. Perawat OK mengonfirmasi butir pra-operasi sebagai penerima. Setelah persetujuan tindakan dan anestesi ada, kasus menjadi Siap.
   6. OK menjalankan checklist WHO, melaksanakan operasi, lalu memantau pemulihan. Selama itu bed asal tetap terisi.
   7. OK mengirim serah terima ke unit tujuan: ringkasan kondisi dan instruksi pasca bedah.
   8. Bila unit tujuan berbeda dari unit bed sekarang, pasien dipindahkan lebih dulu lewat Transfer Pasien.
   9. Perawat unit tujuan pemegang permission terima menerima serah terima.
   10. Kasus `Completed`, dan biaya operasi masuk invoice rawat inap.
6. **Aturan bisnis.**
   - Pesanan bukan jadwal: status Diminta belum menjamin ruang.
   - Biaya ditagih saat `Completed`.
   - Pengirim dan penerima, baik pra-operasi maupun serah terima, adalah dua akun berbeda.
   - Sisi penandaan wajib cocok dengan sisi pada pesanan operasi.
   - Satu episode selalu punya tepat satu bed aktif.
7. **Perubahan status.** Lihat tabel.
8. **Jalur tidak normal.**
   - Operasi ditunda → status Ditunda, dan bangsal melihat alasannya. Catatan pra-operasi menjadi "perlu diperbarui". Saat dijadwalkan ulang, perawat bangsal mengirim versi baru dengan tanda vital terbaru, dan perawat OK mengonfirmasi ulang sebelum kasus boleh "Siap" (`FR-RWF-090`).
   - Order ditolak OK → status Ditolak beserta alasan, penolak, dan waktunya; bangsal memesan ulang sebagai kasus baru (`FR-RWF-086`).
   - Operasi dibatalkan → tidak ada biaya.
   - Serah terima ditolak bangsal → OK wajib melengkapi lalu mengirim ulang.
   - Operasi darurat → jalur bypass checklist OK yang sudah ada, dengan alasan dan penanggung jawab.
9. **Hasil akhir.** Pasien kembali ke unit tujuan dengan instruksi pasca bedah tercatat, dan biaya operasi ada di invoice.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Pesan dari bangsal | `Requested` (Diminta) | Perawat atau dokter bangsal | Merujuk satu order tindakan operasi aktif |
| `Requested` | Jadwalkan | `Scheduled` (Terjadwal) | Petugas OK | Ruang dan tim tersedia |
| `Scheduled` | Kesiapan lengkap | `Ready` (Siap) | Petugas OK | Pra-operasi **versi terbaru** lengkap di kedua sisi, serta persetujuan tindakan dan anestesi ada |
| `Ready` | Mulai operasi | `InProgress` | Tim OK | Checklist sign-in |
| `InProgress` | Selesai | `Completed` | Sistem OK (`OPS-DEC-025`) | Laporan operasi final, pasien keluar kamar pulih, dan serah terima diterima unit tujuan (temuan 42) |
| `Requested`/`Scheduled` | Tunda | `Postponed` | Petugas OK | Alasan wajib. Pra-operasi yang sudah dikirim menjadi "perlu diperbarui" (`RWI-DEC-199`) |
| `Postponed` | Jadwalkan ulang | `Scheduled` | Petugas OK | Kasus baru boleh "Siap" setelah pra-operasi versi baru dikonfirmasi kedua sisi (`RWI-DEC-199`) |
| `Requested` | Tolak | `Rejected` (Ditolak), status akhir | Petugas OK | Alasan wajib; penolak dari akun login (`RWI-DEC-204`) |
| Belum `Completed` | Batalkan | `Cancelled` | Petugas OK | Alasan wajib |

| Serah terima: dari | Tindakan | Ke | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| `Draft` | Kirim | `Sent` | Perawat OK (permission kirim) | — |
| `Sent` | Terima | `Accepted` | Pemegang permission terima, bukan pengirim | Pasien menempati bed aktif di unit tujuan |
| `Sent` | Tolak | `Rejected` | Pemegang permission terima, bukan pengirim | Alasan wajib |

**Contoh penundaan.** Pra-operasi Tn. Budi dikirim 1 Okt 08.30 (TD 120/80) dan dikonfirmasi perawat OK pukul 09.20. Pukul 09.40 operasi ditunda karena ruang penuh, sehingga catatan itu menjadi "perlu diperbarui". Pada 2 Okt pukul 07.00 kasus dijadwalkan ulang pukul 10.00. Pukul 08.00 Ns. Siti membuka Catatan Pra-Operasi; TD terbaru 160/100 dari Pengawasan Harian terisi otomatis. Siti mengonfirmasi ulang butir "Puasa sejak 22.00" dan "Gelang identitas terpasang", lalu mengirim. Pukul 09.00 Ns. Dewi di OK mengonfirmasi versi baru, lalu kasus dapat "Siap". Versi 1 Okt tetap tersimpan sebagai riwayat.

### 7.4 `BP-RWF-04` — Pemakaian alat medis besar

1. **Tujuan.** Pemakaian alat besar tercatat dan tertagih sesuai lama pemakaian.
2. **Pelaku.** Dokter penanggung jawab berpenugasan aktif menginstruksikan. Perawat memasang dan mencatat. Kasir menagih.
3. **Pemicu.** Alat mulai dipasang ke pasien.
4. **Prasyarat.** Jenis alat terdaftar di master Alat Medis (Master Data) dan punya tarif untuk kelas pasien.
5. **Langkah utama.**
   1. Perawat membuka Pemakaian Alat → Order Alat Kesehatan.
   2. Perawat memilih jenis alat dan dokter penanggung jawab, lalu mencatat waktu mulai.
   3. Saat alat dilepas, perawat mencatat waktu selesai.
   4. Server menghitung unit tertagih dan mengirim baris tagihan.
   5. Riwayat tampil di History Alat Kesehatan.
6. **Aturan bisnis.**
   - Satuan dan pembulatan unit mengikuti master alat.
   - Pemakaian yang masih berjalan saat pasien dicatat keluar ruangan ditutup otomatis pada waktu keluar, lalu ditandai untuk diperiksa perawat.
   - Data pemakaian milik Clinical Management; Rawat Inap hanya menyediakan layar.
7. **Perubahan status.** Lihat tabel.
8. **Jalur tidak normal.**
   - Salah pilih alat → batalkan dengan alasan; hanya tagihan pemakaian itu yang batal.
   - Waktu salah → koreksi berversi dengan alasan.
   - Tarif belum ada → pemakaian tetap tersimpan, baris tagihan "tarif belum ada".
9. **Hasil akhir.** Pemakaian tercatat, dan baris tagihan alat ada di invoice.

**Contoh.** Ventilator bersatuan per hari dengan pembulatan ke atas, bertarif kelas 2 Rp 1.200.000 per hari (contoh angka). Dipasang 1 Okt 08.00, dilepas 3 Okt 11.00 = 2 hari 3 jam → dibulatkan **3 hari** → Rp 3.600.000.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Catat mulai | Berjalan | Perawat | Episode aktif; alat bertarif |
| Berjalan | Catat selesai | Selesai | Perawat | Waktu selesai ≥ waktu mulai |
| Berjalan | Pasien keluar ruangan | Selesai, "perlu diperiksa" | Sistem | Waktu selesai = waktu keluar |
| Berjalan/Selesai | Batalkan | Dibatalkan | Perawat penanggung jawab atau kepala ruangan | Alasan wajib; invoice `OPEN` |
| Selesai | Koreksi waktu | Selesai (versi baru) | Kepala ruangan | Alasan wajib; invoice `OPEN` |

### 7.5 `BP-RWF-05` — Pesan penunjang medis dari bangsal

1. **Tujuan.** Pemeriksaan penunjang dapat dipesan dari bangsal seperti V1, dengan pemberi instruksi yang jelas dan terverifikasi.
2. **Pelaku.** Dokter berpenugasan aktif memesan langsung. Perawat menginput atas instruksi dokter tersebut. Unit penunjang memproses dan menerbitkan hasil. Dokter pemberi instruksi memverifikasi.
3. **Pemicu.** Instruksi pemeriksaan dari dokter.
4. **Prasyarat.** Episode aktif; katalog pemeriksaan tersedia.
5. **Langkah utama.**
   1. Pengguna memilih layanan: Lab, Radiologi, Gizi, Bank Darah, atau Hemodialisa.
   2. Pengguna memilih pemeriksaan dan melihat status tanggungan penjamin.
   3. Bila yang memesan perawat, perawat memilih dokter pemberi instruksi dari dokter berpenugasan aktif. Rawat Inap memeriksa penugasan itu.
   4. Pesanan dikirim ke modul pemilik. Pesanan dari perawat berstatus "menunggu verifikasi".
   5. Dokter pemberi instruksi memverifikasi pesanan dari daftar "perlu diverifikasi".
   6. Hasil tampil di bangsal setelah unit penunjang memfinalkan.
6. **Aturan bisnis.**
   - Pesanan perawat tanpa dokter pemberi instruksi ditolak; dokter tanpa penugasan ditolak dengan kode 403.
   - Penginput selalu dari akun login.
   - Hasil hanya dibaca dari modul pemilik.
   - Tagihan muncul saat layanan diterima atau dikerjakan, bukan saat dipesan.
7. **Perubahan status.** Mengikuti modul pemilik. Status verifikasi terpisah dari status pesanan.
8. **Jalur tidak normal.**
   - Pesanan ganda dalam waktu dekat → modul pemilik meminta konfirmasi, misalnya `confirm-duplicate` pada Bank Darah.
   - Pesanan dibatalkan sebelum diproses → tidak ada tagihan.
   - Penugasan dokter berakhir sebelum verifikasi → jejak pesanan tetap utuh (`RWI-DEC-114`).
9. **Hasil akhir.** Pesanan dan hasil terhubung ke episode rawat inap, dan setiap pesanan perawat punya dokter yang memverifikasi.

**Contoh.** Pukul 23.00 dr. Yoga, dokter jaga berpenugasan aktif pada Budi, menelepon Ns. Siti dan menginstruksikan 2 kantong PRC. Siti membacakan ulang instruksinya, membuka Penunjang Medis → Bank Darah, memilih dr. Yoga sebagai dokter peminta, lalu menyimpan. Pesanan tampil di layar Bank Darah dengan status "menunggu verifikasi". Esok pagi dr. Yoga memverifikasinya dari ruang kerjanya.

### 7.6 `BP-RWF-06` — Observasi pengeluaran cairan WSD per selang per shift

1. **Tujuan.** Jumlah cairan yang keluar dari WSD per shift tercatat dan ikut balance cairan.
2. **Pelaku.** Perawat shift.
3. **Pemicu.** Akhir shift atau saat observasi terjadwal.
4. **Prasyarat.** Pasien terpasang minimal satu selang WSD yang terdaftar dengan lokasi dan waktu pasang (`FR-RWF-058`).
5. **Langkah utama.**
   1. Perawat membuka Catatan Keperawatan → Observasi Pengeluaran Cairan WSD.
   2. Perawat memilih selang. Server menampilkan sisa cairan di tabung selang itu dari shift sebelumnya; untuk pembacaan pertama, sisa awal saat pemasangan atau 0 ml bila tidak diisi.
   3. Perawat mengisi jam awal, jam akhir, sisa cairan di tabung sekarang, dan volume yang dibuang bila tabung dikosongkan.
   4. Server menghitung jumlah bertambah dan mencatatnya sebagai output "Drain/WSD" di Pengawasan Harian, dengan rujukan ke selangnya.
   5. Saat selang dilepas, perawat mencatat waktu lepas, dan pencatatan selang itu ditutup.
6. **Aturan bisnis.**
   - Rumus berlaku per selang: jumlah bertambah = sisa sekarang − sisa shift lalu **selang yang sama**.
   - Bila tabung dikosongkan atau diganti: (volume dibuang + sisa sekarang) − sisa shift lalu.
   - Hasil negatif ditolak dengan pesan yang jelas.
   - Pembacaan untuk selang yang sudah dilepas ditolak.
7. **Perubahan status.** Mengikuti pola koreksi dan pembatalan Pengawasan Harian yang sudah ada.
8. **Jalur tidak normal.** Salah ketik → koreksi berversi dengan alasan.
9. **Hasil akhir.** Output WSD masuk balance cairan harian.

**Contoh.** Sisa shift lalu 200 ml. Pukul 14.00 tabung dikosongkan 300 ml. Sisa sekarang 150 ml. Jumlah bertambah = (300 + 150) − 200 = **250 ml**.

**Contoh dua selang.** Ny. Ani terpasang WSD kanan dan WSD kiri sejak 1 Okt, masing-masing dengan sisa awal 0 ml. Pada 2 Okt pukul 06.00 sisa kanan 200 ml dan kiri 50 ml. Pukul 14.00 sisa kanan 350 ml dan kiri 80 ml, tanpa pengosongan. Server mencatat output kanan 150 ml dan kiri 30 ml, sehingga balance cairan bertambah 180 ml dengan rincian dua selang. Pada 4 Okt WSD kiri dilepas pukul 10.00; pembacaan WSD kiri setelah itu ditolak, sedangkan WSD kanan tetap dicatat.

### 7.7 `BP-RWF-07` — Serah terima klinis saat transfer antarunit (`P2`)

1. **Tujuan.** Kondisi pasien berpindah utuh bersama pasiennya saat pindah unit, tanpa menunda perpindahan bed.
2. **Pelaku.** Perawat unit asal mengirim. Perawat unit tujuan pemegang permission terima menerima atau menolak.
3. **Pemicu.** Perpindahan bed ke unit lain lewat Transfer Pasien.
4. **Prasyarat.** Transfer berhasil menurut `RWI-RULE-006` dan `RWI-RULE-016`.
5. **Langkah utama.**
   1. Transfer berhasil. Bed langsung pindah, dan satu dokumen serah terima lahir berstatus "Belum dikirim".
   2. Daftar pasien di kedua unit menampilkan "Serah terima tertunda".
   3. Perawat unit asal mengisi sembilan bagian. GCS, tanda vital, nyeri, risiko jatuh, dan balance cairan dirujuk dari pencatatan yang ada, lalu dibekukan sebagai potret.
   4. Perawat unit asal mengirim dokumen.
   5. Perawat unit tujuan menerima dokumen, dan penanda tertunda hilang.
6. **Aturan bisnis.**
   - Pindah bed di dalam unit yang sama tidak membuat dokumen.
   - Dokumen tidak pernah menahan perpindahan bed.
   - Penerima berbeda akun dari pengirim, dan pasien sudah menempati bed di unit tujuan.
7. **Perubahan status.** Belum dikirim → Dikirim → Diterima, atau Dikirim → Ditolak (wajib beralasan) → Dikirim ulang.
8. **Jalur tidak normal.** Ditolak karena isi kurang → pengirim melengkapi dan mengirim ulang.
9. **Hasil akhir.** Unit tujuan memegang potret kondisi pasien saat diserahkan, beserta nama pengirim dan penerima.

**Contoh.** Pukul 02.10 Budi memburuk di Melati 2. Ns. Siti memindahkannya ke ICU Bed 1. Dokumen lahir, dan kedua unit melihat "Serah terima tertunda". Pukul 02.30 Siti mengisi SOAP singkat dan barang yang diserahkan ("1 botol infus, 1 kateter terpasang"), lalu mengirim. GCS 13 dan TTV pukul 02.00 ikut terlampir. Pukul 02.35 Ns. Rani di ICU menerimanya.


### 7.8 `BP-RWF-08` — Dari kamar pulih ke bangsal dan pemantauan pasca operasi

1. **Tujuan.** Pasien pasca operasi pindah dari kamar pulih ke bed rawat inap dengan aman, bangsal memegang informasi operasinya, dan komplikasi seperti infeksi luka operasi serta reaksi transfusi terpantau.
2. **Pelaku.** Tim OK dan perawat kamar pulih memutuskan tujuan pasien, membatalkan permintaan admisi bila keputusan berubah, dan mengirim serah terima. Petugas admisi menyelesaikan admisi pasien yang belum dirawat inap. Perawat bangsal menerima serah terima, membaca ringkasan operasi, mengisi surveilans, dan memantau transfusi. Tim PPI meninjau surveilans. Bank Darah menerima pemberitahuan reaksi transfusi.
3. **Pemicu.** Kamar pulih menyimpan keputusan "rawat inap" atau "ICU".
4. **Prasyarat.** Laporan operasi final. Untuk pasien yang belum dirawat inap: permintaan admisi dari kamar pulih diselesaikan petugas admisi (`FR-RWF-080`).
5. **Langkah utama.**
   1. Kamar pulih menyimpan keputusan tujuan pasien.
   2. Bila pasien belum punya episode rawat inap aktif, pasien muncul di daftar permintaan admisi. Petugas admisi menyelesaikan admisi berlangkah dan memilih bed; tidak ada admisi otomatis (`FR-RWF-080`).
   3. Pasien menempati bed, dan tarif kamar mulai berjalan. Untuk pasien yang sudah dirawat inap dan unit tujuannya berbeda dari bed sekarang, pasien dipindahkan lewat Transfer Pasien (`RWI-DEC-177`).
   4. Kamar pulih menyatakan pasien keluar, dan OK mengirim serah terima ke unit tujuan.
   5. Perawat unit tujuan pemegang permission terima menerima serah terima (`RWI-DEC-189`). Kasus OK menjadi `Completed`, order tindakan ditandai selesai, dan biaya operasi terkirim ke Billing (`RWI-DEC-196`).
   6. Formulir surveilans infeksi luka operasi lahir. Hari ke-1 jatuh pada hari setelah operasi selesai (`FR-RWF-083`, `FR-RWF-084`).
   7. Dokter dan perawat bangsal membaca ringkasan operasi (`FR-RWF-081`).
   8. Perawat mengisi surveilans setiap hari sampai hari ke-15, dan tim PPI meninjaunya (`FR-RWF-091`). Bila pasien mendapat darah, perawat memantau transfusi per kantong (`FR-RWF-085`).
6. **Aturan bisnis.**
   - Serah terima tidak pernah memindahkan bed (`RWI-DEC-177`), dan penerimanya berbeda akun dari pengirim.
   - Satu pasien satu episode rawat inap aktif: permintaan admisi untuk pasien yang sudah dirawat inap ditolak (`RWI-RULE-035`).
   - Ringkasan operasi, data kantong darah, dan tanda vital tidak disalin ke Rawat Inap.
   - Titik ukur transfusi tidak boleh diisi mundur tanpa keterangan.
7. **Perubahan status.** Permintaan admisi: menunggu → selesai saat admisi dibuat, atau dibatalkan OK dengan alasan; permintaan untuk pasien yang sudah punya episode aktif ditolak sejak awal. Nama statusnya ditetapkan saat desain. Kamar pulih: Pemantauan → Siap keluar → Keluar. Serah terima: `Draft` → `Sent` → `Accepted` atau `Rejected`. Kasus OK: `InProgress` → `Completed` setelah ketiga syarat terpenuhi (temuan 42). Surveilans: berjalan sampai hari ke-15, atau berhenti saat pasien keluar ruangan dengan status "berhenti — pasien pulang hari ke-N".
8. **Jalur tidak normal.**
   - Serah terima ditolak → OK melengkapi dan mengirim ulang; kasus belum `Completed`, dan biaya operasi belum terkirim.
   - Serah terima lama tidak diterima → tampil di daftar pantau bangsal dan OK (`FR-RWF-088`).
   - Permintaan admisi lama tidak ditindaklanjuti → tampil di daftar pantau admisi beserta lamanya menunggu (`FR-RWF-089`).
   - Keputusan kamar pulih berubah sebelum admisi selesai → OK membatalkan permintaan dengan alasan.
   - Pasien keluar ruangan sebelum hari ke-15 → surveilans berhenti otomatis (`FR-RWF-092`).
   - Reaksi transfusi → pemberitahuan otomatis ke Bank Darah; penanganan lanjutnya mengikuti modul Bank Darah.
9. **Hasil akhir.** Pasien menempati bed rawat inap, kasus OK `Completed`, biaya operasi terkirim ke Billing, dan surveilans serta monitoring transfusi berjalan.

**Contoh admisi dari kamar pulih.** Ny. Ani (samaran) menjalani herniorafi elektif dari poliklinik bedah dan selesai pukul 11.00. Pukul 12.00 kamar pulih memutuskan rawat inap semalam karena nyeri belum terkendali, dan Ani muncul di daftar permintaan admisi dengan rujukan kunjungan poliklinik dan kasus OK-nya. Pukul 12.10 petugas admisi menyelesaikan admisi: BPJS kelas 2, DPJP dr. Rina, bed Melati 2 Bed 1. Ani menempati bed pukul 12.20, sehingga tarif kamar mulai berjalan. Pukul 12.25 Ns. Siti menerima serah terima, lalu kasus OK `Completed`. Siti membaca ringkasan operasi: "Herniorafi kanan, tanpa komplikasi, drain tidak terpasang, mobilisasi bertahap". Esok paginya, hari ke-1 surveilans, Siti mencatat tanpa tanda infeksi. Apakah biaya operasi Ani digabung ke invoice `RANAP` atau tetap di invoice kunjungan poliklinik menunggu jawaban Yasmina (`RWI-OQ-114` butir b).

**Contoh monitoring transfusi.** Bank Darah menyerahkan kantong PRC nomor 2026-0101 untuk Tn. Budi pukul 09.00. Ns. Siti memilih kantong itu dan mencatat TD 120/80, suhu 36,8 °C, dan nadi 82 sebelum transfusi pukul 09.10. Pada titik 15 menit pukul 09.25, Budi menggigil dan suhunya 38,2 °C. Siti mencatat reaksi "menggigil, demam", dan pemberitahuan langsung muncul di Bank Darah. Titik 1 jam dan 4 jam tetap dicatat, atau ditandai berhenti bila transfusi dihentikan.

---

## 8. Susunan menu target

### 8.1 Delapan menu keperawatan

| Menu (urutan V1) | Sub-menu target | Sumber data Final |
|---|---|---|
| Pengkajian Pasien | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Pengawasan Harian, Evaluasi Awal, Perencanaan Pulang | Sudah ada; tidak berubah |
| Asuhan Keperawatan | Vital Sign, SOAP, Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, Catatan Keperawatan, **Rencana Asuhan** | Lihat 8.2 dan 8.3. Rencana Asuhan dipertahankan (`RWI-DEC-172` butir 5) |
| Tindakan | Order Tindakan, History Tindakan | Sudah ada; katalog diperbaiki (`FR-RWF-070`) |
| Penunjang Medis | Radiologi, Laboratorium, Rehab Medik, Konsultasi Gizi, Hemodialisa, Bank Darah | EPIC-RWF-04; Rehab Medik tetap *placeholder* |
| Pemakaian Alat | Order Alat Kesehatan, History Alat Kesehatan | EPIC-RWF-07 |
| Transfer Pasien | Form Transfer, History Transfer | Sudah ada; serah terima klinis `P2` (`FR-RWF-071`) |
| Pemesanan Ruangan Bedah | Bedah Operasi, Bedah Obgyn | EPIC-RWF-05 |
| Tagihan Pasien | Rincian per kelompok | EPIC-RWF-03 |

**Klaster Pasca Operasi.** HiSys menempatkan Transfusi Darah dan formulir surveilans nosokomial pada menu perawat. Letak monitoring transfusi dan surveilans di delapan menu V1, letak ringkasan operasi di ruang kerja dokter dan perawat, letak daftar permintaan admisi, dan letak laporan transfer ruangan ditetapkan saat desain menurut wewenang frontend yang berlaku. Agent tidak menetapkannya.

### 8.2 Catatan Keperawatan: enam sub-menu V1

| Sub-menu | Isi V1 | Data yang dipakai di Final | Pemilik data |
|---|---|---|---|
| Spooling Cairan | Intake, output, cairan masuk/keluar/sisa, urin, keterangan, cetak laporan | Pengawasan Harian — cairan (`fluid-balance-entries`) | `ClinicalManagement` |
| Observasi Pengeluaran Cairan WSD | Jam awal/akhir, sisa di tabung, sisa shift lalu, bertambah | Perluasan Pengawasan Harian per selang (`BP-RWF-06`, `FR-RWF-058`) | `ClinicalManagement` |
| Sliding Scale | GDS, insulin, insulin drip, catatan | Order dan pelaksanaan sliding scale Farmasi; GDS dari Pengawasan Harian (`RWI-DEC-147`, `RWI-DEC-148`) | `PharmacyManagement` |
| Daftar Pemberian Obat | Pemberian, Riwayat, Efek Samping, Rekonsiliasi | MAR Farmasi (`RWI-DEC-117`), ADR (`PatientAllergy`), rekonsiliasi Farmasi (`RWI-DEC-132`) | `PharmacyManagement`, `ClinicalManagement` |
| Catatan Pra-Operasi | Tanda vital, nyeri, checklist persiapan pengirim/penerima, informasi operasi, kondisi, penandaan area | Fase persiapan kasus OK (`FR-RWF-045`, `FR-RWF-048`), berversi setelah penundaan (`FR-RWF-090`) | `OperatingRoomManagement` |
| Diet Medis | Tanggal, perawat, diet, status diet, diagnosa, keterangan, riwayat | Diet pasien modul Gizi (`nutrition-management/diets`, `FR-RWF-055`) | `NutritionManagement` |

Catatan naratif perawat **tidak** berada di sini. Narasi ditulis dan dibaca lewat Catatan Terintegrasi atau SOAP dengan jenis "Naratif Keperawatan" (`FR-RWF-056`).

### 8.3 Obat & Alkes

Susunan target sama dengan V1: **Resep, Resep Harian, Alat Kesehatan, Summary** (`FR-RWF-057`).

Final saat diaudit punya delapan sub-tab. MAR, Sliding Scale, dan Obat Bawaan dipindah ke Catatan Keperawatan → Daftar Pemberian Obat dan Sliding Scale. "Pemakaian Alkes" dan riwayat BMHP digabung ke Summary.

---

## 9. Kontrak API

Endpoint dikelompokkan per nilai `[Tags(...)]` di controller, apa adanya. Kolom "Status v0.2" memakai empat label: **Tetap** (tidak berubah), **Diubah** (perilakunya berubah menurut PRD ini), **Dihapus** atau **Dipensiunkan** (tidak dipakai lagi), dan **Rencana** (belum ada). Bentuk respons sukses selalu `ApiResponse<T>`. Nama endpoint baru dan bentuk penyatuannya ditetapkan `design-business-module`.

### 9.1 Inpatient Discharge Clearance

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `POST` | `/{episodeId}/discharge-clearance/webhook` | Dulu: menerima sinyal izin kasir tanpa login | `[AllowAnonymous]` | **Dihapus** (`FR-RWF-001`) |
| `POST` | `/{episodeId}/supervisor-override` | Dulu: membuka pulang fisik darurat dengan PIN | `InpatientDischargeClearance : SupervisorOverride` | **Dipensiunkan**. Pengecualian dipindah ke penutupan (`FR-RWF-005`) |
| `POST` | `/{episodeId}/confirm-physical-discharge` | Dulu: pulang fisik dengan syarat salinan izin kasir | `InpatientDischargeClearance : ConfirmPhysicalDischarge` | **Disatukan** dengan `record-departure` menjadi satu tindakan keluar ruangan (`FR-RWF-006`) |

### 9.2 Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status v0.2 |
|---|---|---|---|---|---|---|
| `POST` | `/{episodeId}/decide` | Keputusan pulang DPJP | `InpatientDischarge : Update` | Keputusan pulang | Episode | Tetap |
| `POST` | `/{episodeId}/record-departure` | Catat pasien keluar ruangan; melepas bed | `InpatientDischarge : RecordDeparture` | Waktu keluar, konfirmasi peringatan | Episode, dengan status kasir yang tercatat | **Diubah**: membaca izin kasir dari Billing untuk peringatan dan menyimpan jejaknya (`FR-RWF-006`, `FR-RWF-007`) |
| `GET` | `/{episodeId}/closure-readiness` | Syarat penutupan dan kendalanya | `InpatientDischarge : Read` | - | Daftar syarat | **Diubah**: syarat keuangan dibaca langsung dari Billing (`FR-RWF-008`) |
| `POST` | `/{episodeId}/close` | Penutupan normal | `InpatientDischarge : Close` | - | Episode | **Diubah**: wajib izin kasir `CLEARED` dari Billing; Billing tidak terbaca = ditolak (`FR-RWF-008`) |
| `POST` | `/{episodeId}/close-with-override` | Penutupan tanpa izin kasir | `InpatientDischarge : CloseOverride` saja | Alasan | Episode | **Diubah**: tanpa pemeriksaan nama peran; status kasir saat itu tersimpan (`FR-RWF-005`) |
| `POST` | `/{episodeId}/financial-clearance` | Tanda keuangan manual | `InpatientDischarge : MarkFinancialClearance` | Status | Riwayat | **Dibekukan**: tidak lagi membuka gerbang (`FR-RWF-004`) |
| `GET` | `/{episodeId}/financial-clearance` | Riwayat tanda manual | `InpatientDischarge : ReadFinancialClearance` | - | Riwayat | Tetap, baca-saja |
| `GET` | Daftar "pulang sebelum izin kasir" | Episode yang keluar dengan status kasir selain `CLEARED` | Diputuskan saat desain | Filter | Daftar | **Rencana** (`FR-RWF-007`) |

Kode status:

| Kode | Arti bagi pengguna |
|---|---|
| `400` | Isian tidak lengkap atau formatnya salah |
| `401` | Belum login |
| `403` | Akun tidak punya hak untuk tindakan ini |
| `404` | Episode rawat inap tidak ditemukan |
| `409` | Data sudah diubah pengguna lain; muat ulang lalu coba lagi |
| `422` | Ditolak aturan bisnis. Contoh: "Episode belum dapat ditutup: kasir belum memberi izin" atau "Status kasir tidak dapat dibaca" |

### 9.3 Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/{episodeId}/billing-status` | Status izin kasir dan kendala tanpa rupiah | `InpatientBillingOperational : Read` | **Diubah**: dibaca langsung dari Billing, bukan dari salinan episode (`FR-RWF-002`, `FR-RWF-003`) |
| `GET` | `/{episodeId}/billing-details` | Rincian dengan rupiah | `InpatientBillingOperational : ViewBillingDetails` | **Diubah**: tidak lagi membaca tabel Billing langsung. Digantikan atau dilebur dengan rincian 9.5 (`FR-RWF-023`) |

### 9.4 BillingInpatientIntegration

Base URL: `api/v1/health-services/billing-management/billing`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `POST` | `/invoices/occupancy-charges` | Hitungan tarif kamar kedua | `BillingInpatient : Create` | **Dipensiunkan** (`FR-RWF-013`) |
| `GET` | `/invoices/encounter/{encounterId}/inpatient-summary` | Ringkasan tagihan rawat inap | `BillingInpatient : Read` | **Diubah**: hak lihat rupiah dari permission (`FR-RWF-024`) |
| `POST` | `/inpatient-clearance/reevaluate` | Kasir menilai ulang izin kasir | `BillingInpatient : Clearance` | Tetap |
| `GET` | `/inpatient-clearance/encounter/{encounterId}/latest` | Status izin kasir terakhir | `BillingInpatient : ReadLatest` | **Diubah**: menjadi satu-satunya sumber yang dibaca Rawat Inap; bacaan untuk perawat tanpa rupiah (`FR-RWF-002`) |
| — | Penerima event outbox Rawat Inap | Membuka invoice `RANAP` dan menghitung ulang tarif kamar, lalu mengonfirmasi terima | Sistem | **Rencana** — proses di dalam aplikasi, tanpa endpoint publik baru (`FR-RWF-010`, `FR-RWF-014`) |

### 9.5 Health Services / Billing Management / Patient Billing Summary

Base URL: `api/v1/health-services/billing-management/patient-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Ringkasan tagihan untuk bangsal | `PatientBillingSummary : Read` | **Diubah**: sumber hitungan invoice; rupiah disaring di server; tanpa "Rp 0" |
| `GET` | `/episodes/{episodeId}/breakdown` | Rincian per kelompok seperti V1; subtotal rupiah hanya untuk pemegang izin | Permission baca rincian dan permission lihat rupiah, terpisah | **Rencana** (`FR-RWF-020` s.d. `025`) |

### 9.6 Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `POST` | `/from-source` | Biaya dari modul lain | `BillingInvoice : Create` | Tetap |
| `POST` | `/catalog-charges` | Kasir menambah biaya; harga dari master tarif | `BillingInvoice : Create` | Tetap |

### 9.7 Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/` | Daftar kasus; bangsal menyaring dengan `EncounterId` | `OperatingRoomCase : Read` | Tetap |
| `GET` | `/{id}` | Detail kasus | `OperatingRoomCase : Read` | Tetap |
| `POST` | `/` | Membuat kasus dari bangsal dengan merujuk order tindakan | `OperatingRoomCase : Create` | **Diubah**: tambah jenis anestesi dan jenis layanan Obstetri (`FR-RWF-042`, `FR-RWF-043`) |
| `PATCH` | `/{id}/cancel` | Membatalkan kasus | `OperatingRoomCase : Cancel` | Tetap |
| `PATCH` | `/{id}/reject` (nama usulan) | Menolak kasus berstatus Diminta dengan alasan wajib | Diputuskan saat desain | **Rencana** (`FR-RWF-086`); implementasi menunggu `RWI-OQ-114` butir (c) |

### 9.8 Health Services / Operating Room Management / Execution

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/handovers` | Serah terima pasca operasi | `OperatingRoomHandover : Read` | Tetap; dibaca juga oleh bangsal |
| `POST` | `/handovers` | OK mengirim serah terima | Permission **kirim** (dipisah dari `Update`) | **Diubah** (`FR-RWF-046`) |
| `PATCH` | `/handovers/{handoverId}/accept` | Menerima atau menolak | Permission **terima** (dipisah dari `Update`) | **Diubah**: penerima ≠ pengirim; pasien harus menempati bed di unit tujuan (`FR-RWF-046`) |

### 9.9 Health Services / Operating Room Management / Preparation

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `PUT` | `/ward-pre-op` (nama usulan) | Catatan Pra-Operasi bangsal dengan konfirmasi pengirim dan penerima per butir, penandaan area, dan versi baru setelah penundaan | Diputuskan saat desain | **Rencana** (`FR-RWF-045`, `FR-RWF-048`, `FR-RWF-090`) |

### 9.10 Gizi

**Health Services / Nutrition Management / Nutrition Order** — base URL `api/v1/health-services/nutrition-management/orders`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan gizi | `NutritionOrder : Read` | Tetap |
| `POST` | `/` | Pesan konsultasi gizi dari bangsal | `NutritionOrder : Create` | **Diubah**: tambah dokter pemberi instruksi dan status verifikasi (`FR-RWF-037`) |
| `POST` | `/{id}/cancel` | Membatalkan pesanan | `NutritionOrder : Cancel` | Tetap |
| `GET` | Daftar "perlu diverifikasi" | Pesanan dari perawat yang menunggu verifikasi dokter | Diputuskan saat desain | **Rencana** |

**Health Services / Nutrition Management / Patient Diet** — base URL `api/v1/health-services/nutrition-management/diets`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/history/{encounterId}` | Riwayat diet pasien (Diet Medis) | `NutritionPatientDiet : Read` | Tetap |
| `POST` | `/` | Menetapkan diet | `NutritionPatientDiet : Update` | **Diubah**: tambah status verifikasi untuk input perawat (`FR-RWF-055`) |
| `POST` | `/{dietId}/stop` | Menghentikan diet | `NutritionPatientDiet : Update` | Tetap; alasan wajib |

### 9.11 Health Services / Blood Bank Management / Blood Order

Base URL: `api/v1/health-services/blood-bank-management/blood-orders`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan darah | `BloodOrder : Read` | Tetap |
| `POST` | `/` | Pesan darah dari bangsal | `BloodOrder : Create` | **Diubah**: tambah status verifikasi (`FR-RWF-037`) |
| `POST` | `/confirm-duplicate` | Konfirmasi pesanan yang mirip pesanan sebelumnya | `BloodOrder : Create` | Tetap |
| `POST` | `/{id}/cancel` | Membatalkan pesanan | `BloodOrder : Cancel` | Tetap |
| `GET` | Daftar "perlu diverifikasi" | Pesanan dari perawat yang menunggu verifikasi dokter | Diputuskan saat desain | **Rencana** |

### 9.12 Laboratorium dan Radiologi

| Tag | Base URL | Method dan path | Kegunaan | Status v0.2 |
|---|---|---|---|---|
| `Health Services / Laboratory Management / Lab Order` | `api/v1/health-services/laboratory-management/lab-orders` | `POST /` | Pesan lab; perawat wajib membawa dokter pemberi instruksi | Tetap (backend siap); tombol perawat dibuka |
| | | `GET /instruction-verification-worklist` | Daftar "perlu diverifikasi" | Tetap |
| `Health Services / Radiology Management / Rad Order` | `api/v1/health-services/radiology-management/rad-orders` | `POST /`, `GET /instruction-verification-worklist` | Sama dengan Lab | Tetap |

### 9.13 Health Services / Clinical Management / Fluid Balance

Base URL: `api/v1/health-services/clinical-management/fluid-balance-entries`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/episodes/{episodeId}` | Entri cairan (Spooling Cairan) | `FluidBalance : Read` | Tetap |
| `GET` | `/episodes/{episodeId}/totals` | Total balance cairan | `FluidBalance : Read` | Tetap |
| `POST` | `/` | Mencatat entri cairan | `FluidBalance : Create` | Tetap |
| `PUT` | `/{id}/correct` | Koreksi berversi | `FluidBalance : Update` | Tetap |
| `POST` | `/wsd-readings` (nama usulan) | Pembacaan tabung WSD per selang per shift; server menghitung jumlah bertambah | `FluidBalance : Create` | **Rencana** (`FR-RWF-054`) |
| `POST`, `PATCH` | `/wsd-drains` (nama usulan) | Mendaftarkan selang WSD (lokasi, waktu pasang, sisa awal) dan mencatat pelepasannya | Diputuskan saat desain | **Rencana** (`FR-RWF-058`) |

### 9.14 Health Services / Clinical Management / Patient Allergy

Base URL: `api/v1/health-services/clinical-management/patient-allergies`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `POST` | `/from-medication-administration` | Mencatat dugaan efek samping obat dari dosis MAR | `PatientAllergy : Create` | Tetap; layarnya baru (`FR-RWF-053`) |

### 9.15 Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Status v0.2 |
|---|---|---|---|---|
| `GET` | `/master-options` | Katalog tindakan | `PatientProcedure : Read` | **Diubah**: memakai penanda rawat inap (`FR-RWF-070`) |
| `POST` | `/inpatient-orders` | Pesan tindakan rawat inap | `PatientProcedure : Create` | Tetap |
| `GET` | `/instruction-verification-worklist` | Daftar "perlu diverifikasi" tindakan | Lihat controller | Tetap |

### 9.16 Pemakaian Alat — **Rencana (belum tersedia)**

Pemilik data sudah diputuskan (`FR-RWF-068`): master di `MasterData`, pemakaian di `ClinicalManagement`. Nama tag dan path final ditetapkan saat desain.

| Method | Path usulan | Kegunaan |
|---|---|---|
| `GET` | `api/v1/health-services/master-data/medical-equipments` | Daftar jenis alat medis besar beserta satuan tagih |
| `GET` | `api/v1/health-services/clinical-management/equipment-usages?episodeId={episodeId}` | History Alat Kesehatan |
| `POST` | `api/v1/health-services/clinical-management/equipment-usages` | Mulai pemakaian |
| `PATCH` | `api/v1/health-services/clinical-management/equipment-usages/{id}/finish` | Selesai pemakaian |
| `PATCH` | `api/v1/health-services/clinical-management/equipment-usages/{id}/cancel` | Batal pemakaian dengan alasan |

### 9.17 Serah terima transfer — **Rencana (belum tersedia)**

Data milik `ClinicalManagement` (`FR-RWF-071`). Endpoint mencakup membaca dokumen per episode, mengisi dan mengirim, serta menerima atau menolak dengan alasan. Nama dan path ditetapkan saat desain.


### 9.18 Health Services / Operating Room Management / Execution — bacaan pasca bedah

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses | Status v0.3 |
|---|---|---|---|---|
| `GET` | `/operation-record` | Laporan operasi: diagnosis pasca bedah, temuan, teknik, komplikasi, perdarahan, drain/implan, rencana pasca bedah | `OperatingRoomCase : Read` | Tetap; dibaca bangsal untuk `FR-RWF-081` |
| `GET` | `/anesthesia-record` | Catatan anestesi | `OperatingRoomAnesthesia : Read` | Tetap; dibaca bangsal |
| `GET` | `/recovery` | Skor, observasi, dan keputusan kamar pulih | `OperatingRoomAnesthesia : Read` | Tetap; dibaca bangsal |

Laporan kunjungan OK sudah tersedia di `GET api/v1/health-services/operating-room-management/reports/operations` (`OperatingRoomCase : Read`) dan tidak dibangun ulang.

### 9.19 Klaster Pasca Operasi — **Rencana (belum tersedia)**

Pemilik datanya sudah diputuskan. Nama tag, path, dan bentuk endpoint ditetapkan `design-business-module`.

| Kegunaan | Pemilik | Keputusan | Penahan implementasi |
|---|---|---|---|
| Daftar permintaan admisi dari kamar pulih, pembatalan oleh OK, dan daftar pantau lama menunggu | `InPatientManagement` menerima; `OperatingRoomManagement` mengirim dan membatalkan | `RWI-DEC-201` | `RWI-OQ-114` butir (a) |
| Formulir surveilans infeksi luka operasi per kasus, termasuk tinjauan PPI | `ClinicalManagement` | `RWI-DEC-202` | Tidak ada; isi formulir disahkan klinis sebelum produksi |
| Monitoring transfusi per kantong dan pemberitahuan reaksi | `ClinicalManagement`, merujuk data penyerahan `BloodBankManagement` | `RWI-DEC-203` | `RWI-OQ-115` |
| Penolakan kasus OK (`Rejected`) | `OperatingRoomManagement` | `RWI-DEC-204` | `RWI-OQ-114` butir (c) |
| Laporan transfer ruangan per periode dan ekspor Excel | `InPatientManagement` | `RWI-DEC-205` | Tidak ada |

---

## 10. Data dan kepemilikan

| Data | Pemilik | Status | Catatan |
|---|---|---|---|
| Invoice, baris tagihan, tarif kamar, biaya administrasi | `BillingManagement` | Ada | Disambungkan untuk `RANAP` (`RWI-DEC-192`) |
| Status izin kasir | `BillingManagement` | **Diputuskan** | Satu sumber, dibaca langsung (`RWI-DEC-167`) |
| Jejak status kasir saat keluar ruangan | `InPatientManagement` | Rencana | Bagian dari pencatatan keluar ruangan (`FR-RWF-007`) |
| Outbox integrasi | `InPatientManagement` | Ada | Isi dipangkas, status jujur (`RWI-DEC-166`) |
| Kasus operasi, checklist, serah terima, pra-operasi bangsal | `OperatingRoomManagement` | Ada sebagian | Tambah jenis anestesi, Obstetri, fase pra-operasi, permission kirim/terima (`RWI-DEC-191`); pra-operasi berversi setelah penundaan (`RWI-DEC-199`); status `Rejected` (`RWI-DEC-204`, menunggu `RWI-OQ-114` butir c) |
| Pesanan gizi, diet pasien | `NutritionManagement` | Ada | Tambah kolom verifikasi (`RWI-DEC-188`, `RWI-DEC-191`). Rawat Inap tidak membuat tabel |
| Pesanan darah | `BloodBankManagement` | Ada | Tambah kolom verifikasi (`RWI-DEC-188`, `RWI-DEC-191`). Rawat Inap tidak membuat tabel |
| Cairan, WSD, selang WSD | `ClinicalManagement` | Ada sebagian | WSD per selang per shift perlu perluasan (`RWI-DEC-200`) |
| Serah terima klinis transfer | `ClinicalManagement` | Rencana | `RWI-DEC-182` |
| MAR, sliding scale, rekonsiliasi | `PharmacyManagement` | Ada | `RWI-DEC-117`, `132`, `147` |
| Master jenis alat medis, rujukan alat pada master tarif | `MasterData` (`Mst`) | Rencana | Diizinkan; master data milik seluruh tim (`RWI-DEC-193`) |
| Pemakaian alat per pasien | `ClinicalManagement` (`Cli`) | Rencana | `RWI-DEC-180` |
| Laporan operasi, catatan anestesi, kamar pulih | `OperatingRoomManagement` | Ada | Dibaca bangsal, tanpa salinan (`FR-RWF-081`) |
| Surveilans infeksi luka operasi | `ClinicalManagement` | Rencana | `RWI-DEC-202` |
| Monitoring transfusi | `ClinicalManagement`, merujuk data penyerahan Bank Darah | Rencana | `RWI-DEC-203`; sisi Bank Darah menunggu `RWI-OQ-115` |
| Permintaan admisi dari kamar pulih | `InPatientManagement` menerima; `OperatingRoomManagement` mengirim | Rencana | `RWI-DEC-201`; sisi OK menunggu `RWI-OQ-114` butir (a) |
| Laporan transfer ruangan | `InPatientManagement`, bacaan linimasa penempatan bed | Rencana | `RWI-DEC-205`; tanpa tabel baru |
| Retur obat yang membatalkan tagihan | `PharmacyManagement` memberi tahu; `BillingManagement` membatalkan | Rencana | `RWI-DEC-195`; sisi Farmasi menunggu `RWI-OQ-108` |

`Mst` dan `Cli` sudah `ACTIVE` pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, sehingga tidak perlu pendaftaran modul baru. Setiap perubahan tabel, termasuk migration, butuh wewenang terpisah sesuai `AGENTS.md`.

---

## 11. Keputusan

### 11.1 Keputusan terbuka v`0.1` dan jawabannya

| Alias v`0.1` | Pertanyaan | Jawaban | Keputusan |
|---|---|---|---|
| `OD-RWF-01` | Arah integrasi tagihan dan sumber izin pulang | Outbox sebagai ketukan pintu dengan konfirmasi terima; izin kasir milik Billing, dibaca langsung; gerbang di penutupan episode | `RWI-DEC-166`, `RWI-DEC-167`, `RWI-DEC-186`, `RWI-DEC-192` |
| `OD-RWF-02` | Pemakaian Alat | Master jenis alat sederhana; data di Master Data dan Clinical Management | `RWI-DEC-179`, `RWI-DEC-180` |
| `OD-RWF-03` | Susunan Asuhan Keperawatan | Pilihan A: susunan V1, Rencana Asuhan dipertahankan | `RWI-DEC-172` |
| `OD-RWF-04` | Rupiah di Tagihan Pasien | Rincian tanpa rupiah; subtotal per kelompok untuk pemegang izin; tanpa harga per item | `RWI-DEC-170` |
| `OD-RWF-05` | Pemilik Catatan Pra-Operasi | Pilihan A: fase di Kamar Operasi; penandaan gambar tubuh tanpa foto | `RWI-DEC-173`, `RWI-DEC-174` |
| `OD-RWF-06` | Bedah Obgyn | Pilihan A: kasus OK berjenis Obstetri | `RWI-DEC-175` |
| `OD-RWF-07` | Siapa boleh menulis Diet Medis | Dokter berpenugasan aktif atau ahli gizi; perawat atas instruksi dengan verifikasi | `RWI-DEC-178`, `RWI-DEC-188` |
| `OD-RWF-08` | Serah terima klinis saat transfer | Wajib, tidak menahan transfer | `RWI-DEC-181`, `RWI-DEC-182`, `RWI-DEC-189` |
| `OD-RWF-09` | Resume ODC | **Dihapus dari PRD ini** | `RWI-DEC-183` |

Keputusan tambahan yang tidak punya alias v`0.1`: bentuk blueprint (`RWI-DEC-164`), kedudukan PRD (`RWI-DEC-165`), arti ✅ (`RWI-DEC-168`), putar ulang (`RWI-DEC-169`), pemesan Gizi dan darah (`RWI-DEC-171`), pemesan ruang bedah (`RWI-DEC-176`), bed selama operasi (`RWI-DEC-177`), penutupan tanpa izin kasir (`RWI-DEC-187`), pemilik dan persetujuan modul tetangga (`RWI-DEC-190` s.d. `RWI-DEC-193`), titik tagih obat dan retur (`RWI-DEC-195`), komponen biaya operasi (`RWI-DEC-196`), perluasan klaster Pasca Operasi (`RWI-DEC-197`), serta scope pass penutupan gate `1.8` (`RWI-DEC-194`, `RWI-DEC-198`).

### 11.2 Yang masih terbuka

| No | Butir | Pemilik | Memblokir |
|---:|---|---|---|
| 1 | ~~Persetujuan pemilik `MasterData`~~ — **tertutup**: master data milik seluruh tim dan perubahannya sudah diizinkan (`RWI-DEC-193`) | — | Tidak lagi memblokir |
| 2 | ~~Tafsiran titik tagih rawat inap~~ — **tertutup** oleh `RWI-DEC-195`: obat saat diserahkan, retur membatalkan tagihan | — | Tidak lagi memblokir |
| 3 | **Tafsiran v`0.2` yang wajib dikonfirmasi:** v`0.1` `FR-RWF-034` menampilkan harga pemeriksaan saat memilih. Agar sejalan dengan `RWI-DEC-160` dan `RWI-DEC-170`, v`0.2` hanya menampilkan **status tanggungan** kepada perawat, sedangkan harga hanya bagi pemegang izin rupiah | Muhammad Hamzah | Desain `CAP-RWF-06` |
| 4 | Kecukupan klinis pesanan darah yang diinput perawat dan isi minimal serah terima transfer | Pemilik clinical governance (belum ditunjuk penuh) | Kesiapan produksi, bukan desain |
| 5 | Aturan pasien meninggal dan kabur (`RWI-RULE-037`) | Pemilik klinis | Kesiapan produksi |
| 6 | Pemetaan role ke permission di lingkungan target, dan kesiapan lingkungan uji (worker aktif, master kebijakan tarif kamar dan tarif per kelas terisi) | Admin Akses Role; tim QA dan penanggung jawab data master | Implementasi dan UAT |
| 7 | ~~Catatan pra-operasi saat kasus Ditunda~~ — **tertutup** oleh `RWI-DEC-199`: wajib dikirim ulang dan dikonfirmasi ulang | — | Tidak lagi memblokir |
| 8 | ~~WSD dengan lebih dari satu selang~~ — **tertutup** oleh `RWI-DEC-200`: dicatat per selang dengan lokasi | — | Tidak lagi memblokir |
| 9 | Pemilik `PharmacyManagement` dan persetujuannya agar retur memberi tahu Billing (`RWI-OQ-108`) | Muhammad Hamzah (penunjukan) | Implementasi retur yang membatalkan tagihan |
| 10 | Persetujuan Ikbal Yulianto: (a) kamar pulih mengirim dan membatalkan permintaan admisi; (c) status `Rejected` di modul OK. **Keputusan** Yasmina (gate `1.9`: `DEC-INP-018`, menahan desain aturan Billing itu saja): (b) biaya operasi dari kunjungan poliklinik atau ODC digabung ke invoice `RANAP` seperti alih IGD (`BKC-DEC-117`), atau tetap di invoice kunjungan asal (`RWI-OQ-114`) | Ikbal Yulianto; Yasmina; lewat Muhammad Hamzah | Implementasi `CAP-RWF-18` dan `CAP-RWF-22`; desain aturan Billing butir (b) |
| 11 | Persetujuan Sukma Giri Pratama: Bank Darah menyediakan data kantong yang sudah diserahkan per pasien dan menerima pemberitahuan reaksi transfusi (`RWI-OQ-115`) | Sukma Giri Pratama, lewat Muhammad Hamzah | Implementasi `CAP-RWF-21` |
| 12 | Pengesahan klinis isi formulir surveilans (daftar indikator dan definisinya) dan penunjukan pemilik PPI; kecukupan klinis titik ukur transfusi; konfirmasi clinical governance atas pra-operasi setelah penundaan | Pemilik klinis atau komite PPI (belum tercatat) | Kesiapan produksi, bukan desain |
| 13 | Prioritas `CAP-RWF-18`, `19`, dan `22` mengikuti usulan v`0.3` (`P1`), karena keputusan pemilik tidak menyebut prioritas | Muhammad Hamzah | Peninjauan PRD; tidak menahan desain |


### 11.3 Keputusan v`0.3` dan jawabannya — klaster Pasca Operasi

| Alias v`0.3` | Pertanyaan | Jawaban | Keputusan |
|---|---|---|---|
| `OD-RWF-10` (`RWI-OQ-109`) | Admisi pasien yang diputuskan rawat inap dari kamar pulih | Pilihan A: permintaan admisi dari OK; petugas admisi menyelesaikan admisi berlangkah biasa dengan merujuk kunjungan asal; tanpa admisi otomatis | `RWI-DEC-201` |
| `OD-RWF-11` (`RWI-OQ-110`) | Surveilans infeksi luka operasi | Pilihan A: `ClinicalManagement`, diisi perawat, ditinjau PPI, satu formulir berversi, `P1`. Sub-keputusan: berhenti saat pasien keluar ruangan | `RWI-DEC-202` |
| `OD-RWF-12` (`RWI-OQ-111`) | Monitoring transfusi | Pilihan A: dibuka dari `DEFERRED` sebagai `P1`, data `ClinicalManagement` per kantong, reaksi diberitahukan ke Bank Darah; alur transfusi lain tetap `DEFERRED` | `RWI-DEC-203` |
| `OD-RWF-13` (`RWI-OQ-112`) | Verifikasi order operasi | Pilihan A: status `Rejected` sendiri dari `Requested` dengan alasan wajib; menyetujui berarti menjadwalkan; kasus ditolak dipesan ulang sebagai kasus baru | `RWI-DEC-204` |
| `OD-RWF-14` (`RWI-OQ-113`) | Laporan transfer ruangan | Pilihan A: `P2`, dari linimasa penempatan bed, permission khusus laporan, ekspor Excel tercatat di audit | `RWI-DEC-205` |

Dua butir gate `1.8` yang tersisa diputuskan pada pass yang sama: pra-operasi saat Ditunda (`DEC-INP-016`, `RWI-DEC-199`) dan WSD per selang (`DEC-INP-017`, `RWI-DEC-200`). Seluruhnya `approved` oleh Muhammad Hamzah pada 1 Oktober 2026. Bagian yang mengubah modul lain menunggu butir 9 s.d. 11 pada bagian 11.2.

---

## 12. Larangan

| No | Larangan | Alasan |
|---:|---|---|
| 1 | Endpoint yang mengubah data tanpa login | Celah keamanan nyata pada webhook izin pulang |
| 2 | Angka tarif atau jam potong tertanam di kode | Menimbulkan tagihan karangan |
| 3 | Menandai pesan terkirim padahal penerima belum mengonfirmasi | Menyesatkan status task dan audit |
| 4 | Menentukan hak akses dari nama peran | Melanggar aturan hak akses Quilvian |
| 5 | Menagih saat dipesan | Pasien bisa tertagih layanan yang tidak terjadi |
| 6 | Menghapus tagihan milik modul lain saat mengoreksi data sendiri | Bug V1 Pemakaian Alat |
| 7 | Membuat tabel salinan untuk MAR, cairan, gizi, darah, operasi, atau pemakaian alat di Rawat Inap | Melanggar `RWI-DEC-081` dan prinsip satu sumber |
| 8 | Data contoh, "Rp 0" untuk data kosong, atau pesan berhasil palsu | `RWI-DEC-108` |
| 9 | Menyalin nama, nomor RM, atau data pasien asli dari V1 atau capture ke dokumen dan test | Privasi |
| 10 | Rawat Inap membaca atau menulis tabel milik Billing secara langsung | `RWI-DEC-102` butir (e) |
| 11 | Event outbox yang membawa tarif, rupiah, harga kelas, atau salinan data penempatan | `RWI-DEC-166` |
| 12 | Isian otorisasi yang tidak pernah diverifikasi, seperti PIN yang hanya dicek tidak kosong | Memberi rasa aman palsu (`RWI-DEC-187`) |
| 13 | Menyimpan foto tubuh pasien untuk penandaan area operasi | Data sangat sensitif; pemilik privasi belum ditunjuk (`RWI-DEC-174`) |
| 14 | Menagih tindakan operasi lebih dari sekali, atau menagih bahan OK lewat dua jalur | `RWI-DEC-196` |
| 15 | Membuat episode rawat inap otomatis dari keputusan kamar pulih | `RWI-DEC-201` butir 2 |
| 16 | Menjadwalkan, mengubah, atau menghidupkan kembali kasus OK yang Ditolak | `RWI-DEC-204` butir 4 |
| 17 | Mengetik nomor kantong darah, atau mengisi titik ukur transfusi secara mundur tanpa keterangan | `RWI-DEC-203` butir 1 dan 3 |
| 18 | Meloloskan gerbang "Siap" dengan catatan pra-operasi berstatus "perlu diperbarui" | `RWI-DEC-199` butir 4 |

---

## 13. Kelemahan V1 yang tidak boleh ditiru

### 13.1 Pemesanan Ruangan Bedah V1

Tagihan tindakan operasi dibuat saat pemesanan, sebelum operasi terjadi.
Bukti: `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/OperasiOK/Controllers/RuangBedahBookingController.cs:283-423@4be1499c`.

### 13.2 Pemakaian Alat V1

| Kelemahan | Bukti |
|---|---|
| Harga baris tagihan dimatikan, sehingga tagihan tanpa harga | `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/Alkes/Controllers/AlatPemakaianController.cs:292-294@4be1499c` |
| Mengubah pemakaian menghapus semua tagihan "Alkes" kunjungan, termasuk alkes farmasi | `AlatPemakaianController.cs:417-424@4be1499c` |
| Dokter wajib dipilih di form, tetapi tidak dikirim ke server | `QuilvianSystemFrontendDev/src/components/view/Rawat-Inap/perawat/perawatan-pasien/Pemakaian-Alat/order-peralatan-kesehatan.jsx:349-388@86408f245` |
| Jenis tagihan "Alkes" dipakai bersama alkes kecil farmasi | `AlatPemakaianController.cs:296@4be1499c` |

### 13.3 Kepulangan V1

V1 mengurutkan pekerjaan lewat daftar: pasien yang sudah selesai klinis muncul di daftar kasir, dan kasir menandai selesai. Namun V1 **tidak** mengunci kepulangan maupun penutupan dengan status kasir (`RWI-FACT-051`). Final mempertahankan urutan kerja itu, lalu menambah gerbang kasir pada **penutupan episode** (`PR-RWF-10`), bukan pada pintu kamar.

---

## 14. Gelombang pengiriman

Nama gelombang memakai awalan `RWF-W` agar tidak bertabrakan dengan `MVP-0..4`, `DOK-MVP-*`, dan `Gelombang 1A`. Nomor task diberikan `plan-module-delivery` dari deret bebas yang berlaku. Persetujuan pemilik modul sudah ada untuk semua gelombang; master data milik seluruh tim (`RWI-DEC-193`). **Wewenang tulis dan migration tetap terpisah per task.**

| Gelombang | Isi | Prioritas |
|---|---|---|
| `RWF-W0` — Keamanan dan kebenaran data | Hapus webhook; hapus PIN dan pemeriksaan nama peran pada override; pensiunkan hitungan tarif kedua dan tarif karangan (Billing); seragamkan label jenis layanan invoice (Billing); katalog tindakan rawat inap; hak lihat rupiah berbasis permission; buka tombol pesan Lab/Rad perawat | `P0`/`P1` |
| `RWF-W1` — Tagihan inti dan penutupan | Penerima event di Billing dan invoice `RANAP` otomatis; outbox jujur; jembatan klinis untuk `RANAP`; biaya admin untuk invoice berisi tarif kamar; satu sumber izin kasir; satu tindakan keluar ruangan dengan peringatan dan jejak; gerbang penutupan; koreksi penempatan dan uji `IsSuperseded`; putar ulang (eksekusi dengan wewenang terpisah); retur obat yang membatalkan tagihan (`RWI-DEC-195`, implementasi menunggu `RWI-OQ-108`) | `P0` |
| `RWF-W2` — Menu yang modulnya sudah ada | Gizi dan Bank Darah beserta kolom verifikasi; Diet Medis; Efek Samping; Catatan Keperawatan susunan V1; Obat & Alkes empat sub-tab; WSD per selang per shift; Tagihan Pasien rincian | `P1` |
| `RWF-W3` — Pasien operasi | Pemesanan dari bangsal termasuk Obgyn; status operasi di bangsal; pra-operasi dan penandaan, termasuk versi baru setelah penundaan (`RWI-DEC-199`); permission kirim/terima serah terima; terima serah terima; biaya operasi: OK menandai order tindakan selesai dan mengirim anestesi, sewa kamar operasi, serta bahan (`RWI-DEC-196`) | `P1` |
| `RWF-W4` — Pemakaian Alat | Master jenis alat; rujukan alat pada master tarif; pemakaian; tagihan | `P1` |
| `RWF-W5` — Menyusul | Serah terima klinis transfer | `P2` |
| `RWF-W6` — UAT bersama kasir | `UAT-RWF-01` s.d. `UAT-RWF-15` dan `UAT-RWF-21` s.d. `23` | Wajib |
| `RWF-W7` — Pasca operasi | Permintaan admisi dari kamar pulih; ringkasan operasi di bangsal; surveilans infeksi luka operasi dengan tinjauan PPI; monitoring transfusi dengan pemberitahuan reaksi; status Ditolak pada kasus OK; laporan transfer ruangan; daftar pantau serah terima dan permintaan admisi yang tertunda; `UAT-RWF-16` s.d. `20` dan `UAT-RWF-24` | `P1`; laporan transfer ruangan `P2`. Bagian OK dan Bank Darah menunggu `RWI-OQ-114` dan `RWI-OQ-115` |

Urutan ketergantungan:

```text
RWF-W0 (keamanan & kebenaran data)
└──► RWF-W1 (tagihan inti & penutupan)
     ├──► RWF-W2 (menu modul yang sudah ada)
     ├──► RWF-W3 (pasien operasi) ── biaya operasi butuh kontrak OK → Billing
     └──► RWF-W4 (pemakaian alat)
          └──► RWF-W6 (UAT bersama kasir)
RWF-W5 (P2) ── boleh paralel setelah RWF-W1
RWF-W7 (pasca operasi) ── setelah RWF-W3; ringkasan operasi (FR-RWF-081) boleh lebih dulu; laporan transfer (FR-RWF-087) setelah koreksi penempatan RWF-W1; bagian OK dan Bank Darah setelah RWI-OQ-114 dan RWI-OQ-115 dijawab
```

Bagian `RWF-W2` yang tidak menyentuh tagihan, seperti Diet Medis, Efek Samping, WSD, dan susunan menu, boleh dikerjakan paralel dengan `RWF-W1`.

---

## 15. Skenario UAT

Seluruh data di bawah adalah data samaran. Setiap skenario dijalankan **di aplikasi berjalan** dan dianggap lulus hanya bila akibatnya terlihat di modul penerima (`PR-RWF-09`).

| ID | Skenario | Hasil yang diharapkan |
|---|---|---|
| `UAT-RWF-01` | "Tn. Contoh A", kelas 2, masuk 1 Okt 10.00. Menerima darah lengkap, pasang infus, ceftriaxone 3 dosis, ventilator 2 hari, dan satu operasi. Dokter memutuskan pulang 4 Okt 08.30, perawat mencatat keluar ruangan 09.00, kasir memberi izin, lalu admisi menutup episode | Invoice `RANAP` berisi kamar, tindakan, lab, obat, alat, operasi, dan biaya admin tanpa input manual kasir. Tarif kamar berhenti di 09.00. Episode `Closed` setelah izin kasir |
| `UAT-RWF-02` | Panggil endpoint lama webhook izin pulang, lalu panggil `close-with-override` tanpa token | Webhook tidak ada (404). `close-with-override` ditolak 401. Status episode tidak berubah |
| `UAT-RWF-03` | Perawat mencatat pasien keluar sebelum kasir memberi izin. Kasir lalu menyetujui, kemudian mencabut karena resep susulan | Peringatan tampil dan bed langsung kosong. Tombol Tutup Episode aktif setelah disetujui, lalu terkunci lagi dengan banner merah setelah dicabut |
| `UAT-RWF-04` | Perawat memesan lab tanpa memilih dokter pemberi instruksi | Ditolak dengan pesan yang jelas |
| `UAT-RWF-05` | Pesan Bedah Obgyn → dijadwalkan → pra-operasi lengkap di kedua sisi → operasi selesai → serah terima diterima bangsal | Status terlihat di bangsal di setiap langkah; kasus berjenis Obstetri; biaya operasi muncul setelah `Completed` |
| `UAT-RWF-06` | Ventilator 1 Okt 08.00 s.d. 3 Okt 11.00, satuan per hari, pembulatan ke atas | Tertagih 3 hari |
| `UAT-RWF-07` | Dokter memesan konsultasi gizi dari bangsal | Pesanan tampil di layar Gizi dengan status verifikasi "tidak perlu" |
| `UAT-RWF-08` | Perawat memesan 2 kantong PRC atas instruksi dokter jaga, lalu dokter memverifikasi | Pesanan tampil di layar Bank Darah "menunggu verifikasi", lalu terverifikasi atas nama dokter |
| `UAT-RWF-09` | WSD: sisa lalu 200 ml, dibuang 300 ml, sisa sekarang 150 ml | Tercatat output 250 ml dan masuk balance cairan |
| `UAT-RWF-10` | Perawat biasa membuka Tagihan Pasien, lalu petugas pemegang izin rupiah membuka layar yang sama | Perawat melihat rincian tanpa rupiah. Petugas melihat subtotal per kelompok dan total, tanpa harga per item |
| `UAT-RWF-11` | Billing dimatikan saat pasien dinyatakan `Admitted`, lalu dihidupkan lagi | Admisi tersimpan; pesan "gagal, dicoba ulang"; setelah Billing hidup, tepat satu invoice terbuka |
| `UAT-RWF-12` | Supervisor menutup episode tanpa izin kasir: pertama dengan akun tanpa permission, kedua dengan alasan "...", ketiga dengan alasan yang jelas | Pertama ditolak 403, kedua ditolak, ketiga berhasil dan muncul di laporan "ditutup tanpa kelayakan keuangan" |
| `UAT-RWF-13` | OK mengirim serah terima ke ICU saat pasien masih di bangsal, lalu perawat OK mencoba menerimanya sendiri | Perawat OK ditolak. Tombol Terima di ICU terkunci sampai transfer selesai, lalu perawat ICU dapat menerima |
| `UAT-RWF-14` | Pasien dipindahkan dari bangsal ke ICU | Bed langsung pindah, dokumen serah terima lahir, penanda "Serah terima tertunda" tampil di kedua unit sampai diterima |
| `UAT-RWF-15` | Di lingkungan uji, jalankan putar ulang untuk dua episode aktif, salah satunya punya biaya kamar manual | Masing-masing punya satu invoice `RANAP` dengan tarif kamar sejak waktu masuk asli; episode berbiaya manual ditandai "perlu diperiksa"; putar ulang kedua tidak mengubah apa pun |
| `UAT-RWF-16` | Pasien samaran dari poliklinik selesai operasi elektif, lalu kamar pulih memutuskan rawat inap | Pasien muncul di daftar permintaan admisi tanpa episode otomatis. Setelah admisi dan bed ditempati, serah terima diterima, kasus OK `Completed`, dan tarif kamar dihitung sejak bed ditempati. Invoice tujuan biaya operasi mengikuti jawaban `RWI-OQ-114` butir (b) |
| `UAT-RWF-17` | Perawat bangsal membuka ringkasan operasi pasien pasca bedah | Diagnosis pasca bedah, komplikasi, drain, dan rencana pasca bedah tampil baca-saja |
| `UAT-RWF-18` | Surveilans infeksi luka operasi hari ke-1 s.d. ke-3 dengan suhu 38,5 °C pada hari ke-2, lalu pasien pulang hari ke-5 | Suhu hari ke-2 terbaca dari tanda vital; tanda infeksi tercatat per lokasi; tim PPI dapat menandai dicurigai; surveilans berhenti "pasien pulang hari ke-5" dan tetap tampil di daftar PPI |
| `UAT-RWF-19` | Monitoring transfusi satu kantong PRC dengan reaksi menggigil, dan titik 1 jam terlewat | Kantong dipilih dari data Bank Darah; titik ukur yang diisi tersimpan; titik 1 jam bertanda terlambat; Bank Darah menerima pemberitahuan reaksi |
| `UAT-RWF-20` | Laporan transfer ruangan satu minggu, dibuka pemegang permission laporan lalu pengguna tanpa permission | Setiap transfer tampil dengan asal-tujuan, alasan, dan pencatat; koreksi bertanda "koreksi"; ekspor Excel tercatat di audit; pengguna tanpa permission ditolak 403 |
| `UAT-RWF-21` | Kasus OK dengan pra-operasi terkonfirmasi ditunda, lalu dijadwalkan ulang esok hari dengan TD terbaru yang berbeda | Catatan lama "perlu diperbarui" dan tetap terbaca; versi baru memuat TD terbaru; kasus baru "Siap" setelah perawat OK berakun lain mengonfirmasi versi baru |
| `UAT-RWF-22` | Pasien dengan WSD kanan dan kiri, dua kali pembacaan, lalu WSD kiri dilepas | Output per selang benar dan balance cairan bertambah sebesar jumlahnya; pembacaan WSD kiri setelah dilepas ditolak |
| `UAT-RWF-23` | Farmasi menyerahkan 3 vial; perawat meretur 1 vial yang lolos pemeriksaan, lalu 1 vial lain yang dinilai rusak | Invoice memuat 3 vial saat penyerahan, lalu 2 vial dengan pembatalan yang merujuk nomor retur; retur vial rusak tidak mengubah tagihan. Dijalankan setelah `RWI-OQ-108` dijawab |
| `UAT-RWF-24` | Petugas OK menolak order operasi tanpa alasan, lalu dengan alasan; bangsal memesan ulang | Penolakan tanpa alasan ditolak; kasus Ditolak tampil di bangsal dengan alasan, penolak, dan waktu; kasus baru berstatus Diminta; tidak ada biaya dari kasus yang ditolak |

---

## 16. Definition of Done

1. Seluruh kemampuan `P0` dan `P1` berstatus selesai di roadmap, dengan laporan task tracked.
2. Tidak ada menu keperawatan *placeholder* kecuali Rehab Medik.
3. `UAT-RWF-01` s.d. `UAT-RWF-15` dan `UAT-RWF-21` s.d. `UAT-RWF-23` lulus **di aplikasi berjalan**, disaksikan pemilik Rawat Inap dan pemilik Billing.
4. Tidak ada endpoint pengubah data tanpa login di modul Rawat Inap dan Billing rawat inap.
5. Tidak ada angka tarif atau aturan jam tertanam di kode.
6. Status izin kasir hanya punya satu sumber, dan episode tidak bisa ditutup normal tanpa izin itu.
7. Setiap baris tagihan bisa ditelusuri ke layanan asalnya.
8. Task yang kriterianya menyebut akibat di modul lain hanya ✅ bila akibat itu terbukti di modul penerima. Butir verifikasi yang tidak dijalankan ditulis `NOT RUN` apa adanya (`RWI-DEC-168`).
9. Migration dan putar ulang diterapkan hanya dengan wewenang terpisah dan tercatat.
10. Kolom baru pada tabel modul lain (Gizi, Bank Darah, Kamar Operasi, Master Data) disertai bukti regresi bahwa alur modul itu tidak berubah hasilnya.
11. Kemampuan klaster Pasca Operasi `P1` (`CAP-RWF-18` s.d. `22`) masuk Definition of Done sejak v`0.4`: `UAT-RWF-16` s.d. `19` dan `UAT-RWF-24` lulus di aplikasi berjalan, dan skenario yang menyentuh Kamar Operasi atau Bank Darah disaksikan juga pemilik modul itu. `UAT-RWF-20` berlaku bila laporan transfer ruangan (`P2`) dikirim. Bagian yang menyentuh modul OK, Bank Darah, dan Farmasi baru dapat ✅ setelah `RWI-OQ-108`, `RWI-OQ-114`, dan `RWI-OQ-115` dijawab dan akibatnya terbukti di modul penerima.
12. Formulir surveilans dan titik ukur transfusi tidak dipakai untuk pasien sungguhan sebelum disahkan pemilik klinis (bagian 11.2 butir 12).

---

## 17. Risiko

| ID | Risiko | Dampak | Mitigasi |
|---|---|---|---|
| `RSK-RWF-01` | Pekerjaan Billing untuk `RWF-W1` tertunda walaupun sudah disetujui | Tagihan inti dan gerbang penutupan tertahan | Persetujuan sudah ada (`RWI-DEC-192`); jadwalkan task Billing lebih dulu di `RWF-W0`/`W1` |
| `RSK-RWF-02` | Penanda `IsSuperseded` dipakai untuk transfer dan kelak untuk koreksi. Billing menghitung semua penempatan yang tidak dihapus | Tarif kamar bisa dobel atau kurang setelah koreksi | Uji wajib pindah kamar vs koreksi salah catat sebelum `RWF-W1` selesai (`FR-RWF-019`) |
| `RSK-RWF-03` | Master tarif kamar, alat, dan kebijakan tarif kamar belum terisi di lingkungan target (`RWI-UI-GAP-007`) | UAT gagal walau kode benar | Persiapan data master sebelum UAT; dilarang menanam data tiruan |
| `RSK-RWF-04` | Biaya admin tidak terhitung untuk invoice yang hanya berisi tarif kamar | Tagihan kurang | Diputuskan diperbaiki (`RWI-DEC-192` butir c, `AC-RWF-013`) |
| `RSK-RWF-05` | Episode yang aktif saat rilis tidak punya invoice `RANAP` | Pasien yang sedang dirawat tidak punya tagihan | Putar ulang terkontrol (`FR-RWF-017`) |
| `RSK-RWF-06` | Penyesuaian modul Kamar Operasi menyentuh modul milik tim lain | Jadwal bergeser | Pemilik OK sudah setuju (`RWI-DEC-191`); koordinasi jadwal dengan Ikbal Yulianto |
| `RSK-RWF-07` | **Pasien pulang sebelum membayar**, karena keluar ruangan tidak ditahan kasir | Piutang | Deposit admisi (`RWI-DEC-093` s.d. `096`), gerbang penutupan, daftar "pulang sebelum izin kasir", daftar pantau, dan laporan penutupan tanpa kelayakan keuangan. Ukur jumlahnya per bulan setelah rilis |
| `RSK-RWF-08` | Migration pada tabel modul lain mengubah alur modul itu tanpa disadari | Regresi di poliklinik atau unit penunjang | DoD nomor 10; kewajiban regresi seperti `RWI-DEC-153` |
| `RSK-RWF-09` | ~~Tafsiran titik tagih berbeda dari kehendak Billing~~ | — | **Tertutup** oleh `RWI-DEC-195` |
| `RSK-RWF-10` | Biaya operasi tertahan karena serah terima pasca operasi tidak segera diterima bangsal (temuan 42) | Tagihan operasi terlambat masuk invoice | Daftar pantau serah terima tertunda (`FR-RWF-088`); daftar pantau permintaan admisi (`FR-RWF-089`) |
| `RSK-RWF-11` | ~~Bukti HiSys tanpa audio menjadi dasar desain~~ | — | **Tertutup**: aturan klaster diputuskan pemilik (`RWI-DEC-201` s.d. `RWI-DEC-205`). Sisa risikonya ada di `RSK-RWF-12` |
| `RSK-RWF-12` | Isi formulir surveilans dan titik ukur transfusi berasal dari layar HiSys, belum disahkan klinis, dan pemilik PPI belum tercatat | Formulir tidak sesuai kebutuhan PPI rumah sakit | Template berversi (`RWI-DEC-202` butir 1); pengesahan klinis menjadi syarat produksi (DoD nomor 12) |
| `RSK-RWF-13` | Persetujuan pemilik Farmasi, OK, dan Bank Darah (`RWI-OQ-108`, `114`, `115`) belum tercatat | Sebagian `RWF-W1`, `RWF-W3`, dan `RWF-W7` tertahan di implementasi | Desain berjalan lebih dulu; task yang menyentuh modul itu dijadwalkan setelah jawaban tercatat |

---

## 18. Riwayat dan persetujuan

| Versi | Tanggal | Perubahan | Oleh |
|---|---|---|---|
| `0.1` | 1 Oktober 2026 | Draf pertama dari audit paritas 30 September 2026 | Disusun agent atas permintaan pemilik |
| `0.2` | 1 Oktober 2026 | Diselaraskan dengan decision log revision `29` (`RWI-DEC-163` s.d. `RWI-DEC-193`) dan impact scan bagian 19. Perubahan utama ada di bagian 0.1. `CAP-RWF-17`, `FR-RWF-072`, dan `OD-RWF-09` dihapus. Enam temuan baru (nomor 30 s.d. 35), `BP-RWF-07`, `PR-RWF-10`, `PR-RWF-11`, dan `UAT-RWF-11` s.d. `15` ditambahkan | Disusun agent atas permintaan pemilik |
| `0.3` | 1 Oktober 2026 | Menyerap `RWI-DEC-195` (titik tagih obat dan retur) dan `RWI-DEC-196` (komponen biaya operasi). Menambah klaster Pasca Operasi sebagai usulan dari bukti HiSys (`RWI-DEC-197`): temuan 36 s.d. 42, bagian 2.3, 5.6, `EPIC-RWF-09`, `BP-RWF-08`, 9.18, 9.19, 11.3, `RWF-W7`, dan `UAT-RWF-16` s.d. `20`. Perubahan utama ada di bagian 0.2 | Disusun agent atas permintaan pemilik |
| `0.4` | 1 Oktober 2026 | Menyerap Amendment Pass penutupan gate `1.8` (`RWI-DEC-194` s.d. `RWI-DEC-205`, decision log revision `30`). Klaster Pasca Operasi tidak lagi usulan: `CAP-RWF-18` s.d. `22` masuk `P1` dan `CAP-RWF-23` masuk `P2`. Pra-operasi setelah penundaan (`FR-RWF-090`) dan WSD per selang (`FR-RWF-054`, `FR-RWF-058`) ditambahkan. `FR-RWF-089`, `091` s.d. `093`, `AC-RWF-057`, `058`, `088` s.d. `100`, dan `UAT-RWF-21` s.d. `24` ditambahkan. Bagian 11.3 menjadi tabel jawaban. Perubahan utama ada di bagian 0.3 | Disusun agent atas permintaan pemilik |

Persetujuan atas **isi keputusan** yang disalin dokumen ini. Persetujuan modul tetangga disampaikan tidak langsung lewat Muhammad Hamzah, sesuai catatan pada decision log.

| Persetujuan | Nama | Tanggal | Cakupan |
|---|---|---|---|
| Product/Domain Owner Rawat Inap | Muhammad Hamzah | 1 Oktober 2026 | `RWI-DEC-163` s.d. `RWI-DEC-189`; `RWI-DEC-194`, `RWI-DEC-197` s.d. `RWI-DEC-205` (sisi Rawat Inap dan Clinical) |
| Pemilik modul Billing | Yasmina | 1 Oktober 2026 | `RWI-DEC-192`: penerima event, invoice `RANAP`, satu sumber izin kasir, dan butir internal Billing (a) s.d. (h); `RWI-DEC-195` (titik tagih obat dan retur) dan `RWI-DEC-196` (komponen biaya operasi). **Belum:** invoice tujuan biaya operasi dari kunjungan poliklinik atau ODC (`RWI-OQ-114` butir b) |
| Pemilik modul Kamar Operasi | Ikbal Yulianto | 1 Oktober 2026 | `RWI-DEC-191`: pra-operasi, gerbang "Siap", jenis anestesi, Obstetri, pemesanan dari bangsal, biaya operasi, dan permission kirim/terima serah terima; `RWI-DEC-196` (komponen biaya operasi). **Belum:** permintaan admisi dari kamar pulih dan status `Rejected` (`RWI-OQ-114` butir a dan c) |
| Pemilik modul Gizi | Ikbal Yulianto | 1 Oktober 2026 | `RWI-DEC-191`: kolom verifikasi pesanan gizi dan diet, serta input perawat atas instruksi |
| Pemilik modul Bank Darah | Sukma Giri Pratama | 1 Oktober 2026 | `RWI-DEC-191`: kolom verifikasi pesanan darah dan input perawat atas instruksi. **Belum:** data kantong yang sudah diserahkan dan pemberitahuan reaksi transfusi (`RWI-OQ-115`) |
| Pemilik modul Laboratorium dan Radiologi | Yoga Aji | 16 September 2026 | `RWI-DEC-153`: kolom instruksi dan verifikasi pada pesanan Lab dan Radiologi |
| Master Data | Seluruh tim (tanpa satu pemilik) | 1 Oktober 2026 | `RWI-DEC-193`: master jenis alat dan rujukan alat pada master tarif, disampaikan Muhammad Hamzah |
| Pemilik modul Farmasi | Belum tercatat | — | **Belum.** Retur obat memberi tahu Billing (`RWI-OQ-108`) |
| Klaster Pasca Operasi (`CAP-RWF-18` s.d. `23`) | Muhammad Hamzah | 1 Oktober 2026 | `RWI-DEC-201` s.d. `RWI-DEC-205` untuk sisi Rawat Inap dan Clinical; sisi OK dan Bank Darah lihat baris di atas |
| Dokumen v`0.4` sebagai satu kesatuan | — | — | **Belum ditinjau** |
