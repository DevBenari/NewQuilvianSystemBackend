# PRD to MVP — Workspace Admisi Rawat Inap (Ruang Kerja Penerimaan Pasien Rawat Inap)

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| PRD ID | `PRD-RWI-ADMISI-001` |
| Versi | `0.2` |
| Status | **`DRAFT v0.2` — hasil analisis, belum diputuskan seluruhnya.** v`0.2` mencabut Asesmen Edukasi dari Workspace Admisi atas keputusan pemilik (`DEC-RWA-003`, bagian 1.3). Dokumen ini adalah **PRD hulu** (masukan untuk `grill-me` dan `design-business-module`), sama kedudukannya dengan `PRD-RWI-FINISHING-001` v`0.1`. Dokumen ini **bukan** `04-prd-to-mvp.md` milik blueprint, dan **memuat pertanyaan yang memblokir** (bagian 20.3), sehingga **tidak boleh** diteruskan ke `plan-module-delivery` sebelum keputusannya turun |
| Tanggal | v`0.1` dan v`0.2`: 7 Oktober 2026 |
| Produk | QuilvianFinal — Health Services |
| Modul | Rawat Inap (`InPatientManagement`, prefix entity `Inp`). Modul pendukung: Clinical Management (persetujuan), Medical Record Management (keutuhan dokumen), Billing Management (deposit, tarif), Patient Management dan Registration Management (data pasien, wali, penjamin), Master Data (butir checklist, aturan tanggungan), Platform/Authorization (identitas dan tanda tangan petugas) |
| Repository target dan SHA baseline | Backend `NewQuilvianSystemBackend` branch `MHamzah` @ `671191eb1`. Frontend `QuilvianSystemFrontendDev` branch `HamzahV2` @ `1f889d67c` |
| Pembanding V1 (hanya dibaca) | Backend `QuilvianSystemBackendDev` branch `QuilvianSta` @ `4be1499cc`. Frontend `QuilvianSystemFrontendDev` branch `MHamzah` @ `86408f245` |
| Bukti tampilan V1 | 14 tangkapan layar `QuilvianV1/QuilvianSystemFrontendDev/captures/episode/RuangKerjaAdmisi/` (7 Oktober 2026, pukul 09.43–09.47) |
| Brief UI dari pemilik (7 Oktober 2026) | (1) Detail Episode mendapat tombol **Workspace Admisi** di samping **Workspace Dokter**. (2) Tampilan memakai **template Workspace Keperawatan**. (3) V1 hanya acuan bisnis; sistem Final harus lebih baik dan siap *go-live*. (4) **Isian form tetap mengikuti V1**, karena bisnisnya sudah berjalan dan sudah disepakati dengan client. (5) Keputusan susulan hari yang sama: **Assessment Edukasi Pasien tidak diperlukan di admisi** (`DEC-RWA-003`) |
| Ringkasan cakupan | Satu ruang kerja per episode rawat inap tempat petugas admisi menyelesaikan **dokumen penerimaan pasien dari sepuluh menu V1** — persetujuan umum, serah terima pasien baru, gelang dan label, privasi, data dasar rawat inap, pelunasan deposit, estimasi biaya, selisih biaya, benefit makanan pendamping, serta nilai dan kepercayaan — dengan data terisi otomatis, tersimpan, berversi, dan tercetak sama seperti V1. Menu ke-11 V1, Assessment Edukasi Pasien, **tidak dipakai di admisi** (`DEC-RWA-003`). MVP menampilkan sembilan menu; MP Benefit dan tab Estimasi Biaya Rinci ditunda (bagian 8) |
| Pemilik produk | **Muhammad Hamzah**, Product/Domain Owner Rawat Inap (`RWI-DEC-061`) |
| Pemilik modul tetangga | Billing: **Yasmina** (`RWI-DEC-154`). Gizi: **Ikbal Yulianto** (`RWI-DEC-190`). Master Data: seluruh tim (`RWI-DEC-193`). Pemilik privasi/hukum untuk persetujuan umum: **belum ditunjuk** (`DEC-INP-003`). Pemilik Platform/Authorization: belum tercatat di decision log Rawat Inap |
| Hubungan dokumen | **Melengkapi**, tidak menggantikan, `PRD-RWI-V2-001` (`04-prd-to-mvp-final.md`) dan `PRD-RWI-FINISHING-001` (`05-prd-to-mvp-finishing-rawat-inap.md`). Dokumen ini **menyentuh** keputusan yang sudah ada: `RWI-DEC-077` (persetujuan umum dicetak tanpa disimpan), `RWI-DEC-035`/`RWI-RULE-025` (persetujuan umum per episode, menunggu pemilik privasi), `RWI-RULE-018` (daftar periksa administrasi), `IA-INP-05` (kuota menu penuh), serta kemampuan yang ditunda `RWI-CAP-031` dan `RWI-CAP-027`. **Bila dokumen ini berbeda dari decision log, decision log yang berlaku** |
| Jalur pengesahan | `grill-me` (Amendment Pass, menjawab bagian 20.3) → `trace-existing-capabilities` (mendaftarkan `CAP-RWA-01` s.d. `CAP-RWA-17`, kecuali `CAP-RWA-04` yang dibatalkan, ke `01-existing-capability-map.md`) → `requirement-completeness-gate` → `design-business-module` (sub-modul baru atau amandemen, `OD-RWA-05`) → `plan-module-delivery` → `build-module-backend` / `build-module-frontend` |

### 1.1 Gerbang `design-business-module` yang belum terpenuhi untuk cakupan ini

Kontrak PRD → MVP mensyaratkan empat hal sebelum dokumen ini boleh menjadi `04-prd-to-mvp.md` blueprint. Keempatnya belum ada untuk Workspace Admisi, sehingga nama tabel, status, dan endpoint di dokumen ini berstatus **usulan** dan berlabel `Rencana (belum tersedia)`.

| Prasyarat | Keadaan 7 Oktober 2026 | Yang menutupnya |
|---|---|---|
| Tabel kepemilikan data di `02-backend-architecture.md` | Belum ada untuk dokumen-dokumen ini. Usulan di bagian 12.3 | `OD-RWA-05`, `OD-RWA-06`, lalu `design-business-module` |
| `contracts/state-transition-matrix.md` | Belum ada. Usulan model status di bagian 11 | `design-business-module` |
| ID kemampuan di `01-existing-capability-map.md` | Belum ada. Dokumen ini memberi ID usulan `CAP-RWA-01` s.d. `CAP-RWA-17`; `CAP-RWA-04` sudah dibatalkan dan nomornya tidak dipakai ulang | `trace-existing-capabilities` |
| Tidak ada keputusan `BLOCKING` terbuka | Ada enam (bagian 20.3) | `grill-me` |

### 1.2 Cara membaca dokumen ini

Dokumen ini menjawab satu pertanyaan: **apa yang harus dibangun supaya petugas admisi rawat inap di QuilvianFinal bisa menyelesaikan seluruh dokumen penerimaan pasien seperti di V1, tetapi tanpa kelemahan V1.**

Semua nama pasien, keluarga, dan petugas di dokumen ini adalah **samaran**. Nama pada tangkapan layar V1 tidak disalin.

| Istilah | Arti |
|---|---|
| V1 | QuilvianV1, sistem lama yang alur bisnisnya sudah disetujui client. **Hanya dibaca** |
| Final | QuilvianFinal, sistem baru yang akan dipakai rumah sakit. Satu-satunya tempat perubahan |
| Admisi / PPRI | Penerimaan Pasien Rawat Inap. Petugasnya disebut **petugas admisi** atau **petugas PPRI** — istilah yang tercetak di dokumen V1 sendiri ("Petugas PPRI / Admission") |
| CRO | *Customer Relation Officer*, petugas layanan pelanggan yang ikut menandatangani serah terima pasien baru di V1 |
| Episode | Satu masa rawat inap seorang pasien, dari admisi sampai episode ditutup (`InpEpisode`) |
| Dokumen admisi | Satu lembar formulir penerimaan pasien, misalnya Permintaan Privasi atau Surat Pernyataan Selisih Biaya |
| Konsep (*draft*) | Dokumen yang sudah disimpan tetapi belum dikunci dan belum lengkap tanda tangannya |
| Versi | Salinan baru dokumen yang dibuat ketika dokumen yang sudah ditandatangani perlu dikoreksi. Versi lama tetap tersimpan |
| Mode tanda tangan kertas | Formulir dicetak, ditandatangani basah di kertas, lalu petugas mencatat di sistem bahwa lembar itu sudah ditandatangani, oleh siapa, dan kapan |
| Mode tanda tangan digital | Pasien atau keluarga menandatangani langsung di layar (tablet), dan gambar tanda tangannya disimpan sebagai bukti |
| Atestasi petugas | Tanda tangan petugas berupa pernyataan elektronik "ditandatangani oleh *nama*, *waktu*" yang terikat ke akun yang sedang masuk, tanpa gambar goresan |
| *Placeholder* | Layar yang hanya bertuliskan "belum tersedia" dan tidak menyimpan apa pun |

Label status kemampuan memakai taksonomi baku Quilvian:

| Label | Arti untuk pembaca umum |
|---|---|
| `READY TO REUSE` / `EXISTING / REUSE` | Sudah ada dan bisa dipakai apa adanya |
| `REUSE WITH ADAPTER` | Sudah ada di modul lain, tinggal disambungkan |
| `EXTEND` | Sudah ada sebagian, perlu kolom, status, atau endpoint tambahan |
| `MISSING / NEW` | Belum ada sama sekali |
| `CONFLICT` | Dua bagian sistem atau dua keputusan saling bertentangan |
| `LEGACY REFERENCE` | Hanya rujukan sistem lama, tidak dipindahkan |
| `OPEN DECISION` | Belum diputuskan; tidak boleh masuk gelombang pengiriman |

Penomoran ID di dokumen ini memakai kode **`RWA`** (Rawat Inap — Workspace Admisi). Kode ini belum pernah dipakai di dokumen Quilvian mana pun per 7 Oktober 2026.

| Jenis | Pola | Contoh |
|---|---|---|
| Kemampuan | `CAP-RWA-nn` | `CAP-RWA-03` |
| Alur bisnis | `FLOW-RWA-MVP-nnn` | `FLOW-RWA-MVP-001` |
| Epic | `EPIC-RWA-nn` | `EPIC-RWA-03` |
| Functional requirement | `FR-RWA-nnn` | `FR-RWA-031` |
| Invariant | `INV-RWA-nn` | `INV-RWA-04` |
| Kebutuhan non-fungsional | `NFR-RWA-nn` | `NFR-RWA-02` |
| Skenario UAT | `UAT-RWA-nn` | `UAT-RWA-06` |
| Pertanyaan terbuka | `OD-RWA-nn` | `OD-RWA-07` |
| Usulan keputusan produk | `DEC-RWA-nnn` | `DEC-RWA-005` |

### 1.3 Yang berubah dari v`0.1`

| Hal | v`0.1` | v`0.2` | Dasar |
|---|---|---|---|
| Menu Assessment Edukasi Pasien | Dipakai ulang dari Workspace Keperawatan sebagai menu ke-3 Workspace Admisi (`CAP-RWA-04`, `EPIC-RWA-04`) | **Dicabut dari Workspace Admisi.** Asesmen edukasi tetap dikerjakan perawat di Workspace Keperawatan → Pengkajian Pasien → Asesmen Edukasi, tanpa perubahan apa pun | `DEC-RWA-003` — keputusan pemilik produk 7 Oktober 2026: "Assessment Edukasi Pasien tidak diperlukan di admisi". Wajib dicatat ke decision log Rawat Inap pada `grill-me` berikutnya |
| Jumlah menu Workspace Admisi | Sebelas menu V1; sepuluh tampil di MVP | **Sepuluh** menu; **sembilan** tampil di MVP (MP Benefit tetap ditunda) | `DEC-RWA-003` |
| Nomor yang dicabut | — | `CAP-RWA-04`, `EPIC-RWA-04`, `FR-RWA-040`, `FR-RWA-041`, `UAT-RWA-08`, `UAT-RWA-09` berstatus **Dibatalkan** dan **tidak dipakai ulang**, mengikuti aturan penomoran PRD → MVP | Kontrak PRD → MVP bagian 4 |
| Gelombang `RWA-MVP-1` | Memuat `EPIC-RWA-04` | Tanpa `EPIC-RWA-04` | `DEC-RWA-003` |

**Akibat yang perlu diketahui.** Petugas admisi tidak lagi punya jalan masuk ke asesmen edukasi dari Workspace Admisi. Bila petugas admisi tetap perlu **membaca** riwayat edukasi pasien, ia membukanya lewat Workspace Keperawatan dengan hak baca yang sudah ada. Checklist Serah Terima Pasien Baru tidak terdampak, karena tidak satu pun dari 18 butirnya menyangkut edukasi.

---

## 2. Ringkasan eksekutif

Di V1, petugas admisi menyelesaikan seluruh dokumen penerimaan pasien rawat inap dari satu halaman berisi sebelas menu. Halaman itu tidak ada di QuilvianFinal. Akibatnya, setelah pasien diterima di Final, persetujuan umum hanya bisa dicetak kosong, gelang identitas tidak bisa dicetak, dan delapan dokumen lain tidak punya tempat sama sekali. (Menu ke-11 V1, Assessment Edukasi Pasien, sudah ada di Workspace Keperawatan dan tidak dibawa ke admisi — `DEC-RWA-003`.)

PRD ini mengusulkan **Workspace Admisi**: satu ruang kerja per episode, dibuka dari tombol baru di Detail Episode tepat di samping Workspace Dokter, dengan tata letak yang sama dengan Workspace Keperawatan. Hasil bisnis yang dikejar:

1. **Petugas admisi menyelesaikan seluruh dokumen penerimaan pasien di satu tempat**, dengan urutan menu dan isian yang sama seperti V1 (tanpa Assessment Edukasi), sehingga petugas tidak perlu belajar ulang.
2. **Tidak ada pengetikan ulang.** Identitas pasien, wali, kontak darurat, penjamin, kelas, kamar, DPJP, dan kekurangan deposit terisi dari data yang sudah ada di Final.
3. **Setiap dokumen tersimpan, berversi, dan dapat dicetak ulang** dengan isi yang sama. Di V1 beberapa dokumen — misalnya Pelunasan Deposit — hanya dicetak dan tidak pernah tersimpan.
4. **Tidak ada data karangan.** V1 mencetak nomor kartu asuransi acak dan nilai benefit bawaan Rp 500.000 bila datanya kosong. Final menolak mencetak data yang tidak ada.
5. **Petugas tahu dokumen mana yang belum lengkap.** Ruang kerja menampilkan kelengkapan, misalnya "Dokumen admisi 5 dari 7 lengkap", beserta dokumen yang wajib untuk pasien tersebut.

**Contoh.** Tn. Budi Santoso masuk rawat inap kelas 2 dengan penjamin asuransi. Petugas admisi Sari membuka Workspace Admisi dari Detail Episode. Ruang kerja langsung menunjukkan tujuh dokumen wajib untuk Budi, termasuk Surat Pernyataan Selisih Biaya (karena penjaminnya asuransi) dan Pelunasan Deposit (karena Billing mencatat kekurangan deposit Rp 3.000.000). Sari mencetak gelang, mengisi persetujuan umum, lalu mengirim checklist serah terima ke CRO dan perawat ruangan. Pukul 10.40 perawat Andi menandatangani penerimaan, dan kelengkapan naik menjadi 7 dari 7.

---

## 3. Masalah produk (hasil analisis)

### 3.1 Ruang Kerja Admisi di V1

Di V1, petugas admisi membuka **Daftar Pasien Rawat Inap**, lalu mengklik dua kali satu baris untuk membuka halaman **Dokumen Pasien Ranap** (`/Admisi/list-pasien/dokumen-pasien-ranap/{slug}`). Halaman itu terdiri dari kartu **Informasi Pasien** di atas dan **sebelas menu di kiri**. Bukti: `QuilvianSystemFrontendDev/src/components/view/Admisi/list-pasien-admisi/list-pasien-ranap.jsx:283-287, 489-491@86408f245` dan `.../dokumen-pasien-ranap/detail-dokumen-tabs.jsx:130-247@86408f245`.

Kartu Informasi Pasien V1 memuat: No. RM, umur, kelas, tanggal masuk, diagnosa, nama pasien, kontak darurat, kamar, DPJP, alergi, jenis kelamin, tipe pembayaran, bed, dan status pengkajian.

### 3.2 Sebelas menu V1 dan fungsinya

| No | Menu V1 | Fungsi bisnis | Tersimpan di V1? | Penanda tangan V1 | Tangkapan layar |
|---:|---|---|---|---|---|
| 1 | General Consent | Persetujuan umum: hubungan penanda tangan dengan pasien, tipe dan kelas kamar, penerimaan panduan rawat inap | Ya (`GeneralConsent`) | Pasien/keluarga (digital), "Kepala Ruangan" (gambar TTD pengguna yang sedang masuk) | `094331` |
| 2 | Serah Terima Pasien | Checklist 15 butir (+3 sub-butir) serah terima pasien baru dari admisi ke ruangan, Sudah/Belum + keterangan | Ya (`HandoverPasien` + `HandoverPasienDetail`) | Admission, CRO, Perawat (gambar TTD master) | `094356`, `094424`, `094432` |
| 3 | Assessment Edukasi Pasien | Asesmen kebutuhan edukasi dan catatan pelaksanaan edukasi, dengan tab Riwayat | Ya (komponen yang sama dengan perawat) | Wali (digital), Perawat (gambar TTD) | `094448`, `094503` |
| 4 | Gelang & Label Pasien | Cetak gelang dewasa/bayi dan label pasien ber-QR | Tidak (hanya cetak) | — | `094518` |
| 5 | Permintaan Privasi | Kerabat yang boleh menjenguk, permintaan khusus, privasi selama transportasi | Ya (`PermintaanPrivasi`), hanya tambah | Pasien/keluarga (digital), "Kepala Ruangan" | `094543` |
| 6 | IPD | Cetak **Data Dasar Rawat Inap/ODC**: identitas, alamat, penjamin, rencana kelas, dan petugas | Tidak (hanya cetak) | — | `094603` |
| 7 | Pelunasan Deposit | Surat pernyataan kesediaan melunasi kekurangan deposit sampai tanggal jatuh tempo | **Tidak** — hanya cetak | Yang menyatakan (digital), petugas (gambar TTD) | `094620` |
| 8 | Estimasi Biaya | Tab Rekap: "Penjelasan Prakiraan Biaya Tindak Medik". Tab Rinci: "Prakiraan Biaya" per kelompok | Tidak (hanya cetak) | Garis kosong untuk tanda tangan basah | `094629` |
| 9 | Selisih Biaya | Surat pernyataan kesediaan menanggung biaya sendiri bila penjamin tidak menanggung | Ya (`SelisihBiaya`), hanya tambah | Deklarer (digital), petugas PPRI (gambar TTD) | `094640` |
| 10 | Mp benefit | Kartu "Benefit Makanan Pendamping" ber-QR dengan nilai benefit | Tidak (hanya cetak) | — | `094647` |
| 11 | Form Nilai Kepercayaan | Identifikasi nilai dan kepercayaan pasien, hal-hal yang bertentangan (1–5 butir) | Ya (`NilaiKepercayaan`), **per pasien, bukan per kunjungan** | Penanda tangan (digital) | `094701` |

Rincian isian setiap form ada di **Lampiran A**. Isian itu yang dipertahankan di Final.

### 3.3 Kondisi QuilvianFinal per menu

Status di bawah adalah keadaan source saat dianalisis, **bukan** target.

| No | Menu V1 | Yang sudah ada di Final | Status | Bukti utama |
|---:|---|---|---|---|
| 0 | Halaman ruang kerja dan jalan masuk | Detail Episode punya tombol Workspace Keperawatan dan Workspace Dokter, **tidak ada** Workspace Admisi. Template ruang kerja (kerangka 4 wilayah) sudah ada dan dipakai Workspace Keperawatan | `MISSING / NEW` (layar) + `EXISTING / REUSE` (template) | `inpatient-episode-detail-view.jsx:609-711@1f889d67c`; `nursing-workspace-view.jsx:262-401@1f889d67c`; `src/components/ui/clinical-workspace/*` |
| 1 | General Consent | (a) Tabel persetujuan pasien **sudah ada** di Clinical Management, lengkap dengan jenis `Admission`, tipe penanda tangan, jalur gambar tanda tangan, hash berkas, pencabutan, dan pembatalan. (b) Alur admisi Final mencetak **Surat Persetujuan Pasien Rawat Inap 12 butir** — lembar yang **berbeda** dari form General Consent V1 di ruang kerja — dan **sengaja tidak menyimpan** apa pun (`RWI-DEC-077`). (c) Penyimpanan persetujuan umum ditunda (`RWI-CAP-031`) karena pemilik privasi belum ditunjuk (`DEC-INP-003`) | `EXTEND` + `CONFLICT` dengan `RWI-DEC-077` → `OPEN DECISION` | `TrxPatientConsent.cs:13-321@671191eb1`; `PatientConsentType.cs` (`Admission = 9`); `inpatient-consent-print-view.jsx:30-119@1f889d67c`; `inpatient-admission-flow-constants.jsx:814-957@1f889d67c` |
| 2 | Serah Terima Pasien | Tidak ada checklist serah terima pasien baru. Yang ada: master **Butir Administrasi Rawat Inap** untuk daftar periksa **penutupan** episode (`RWI-RULE-018`), dengan kode, nama, wajib, dan urutan | `MISSING / NEW` + `EXTEND` (master) | `MstInpatientClearanceItem.cs@671191eb1`; `InpatientClearanceItemController.cs:36-290@671191eb1` |
| 3 | Assessment Edukasi | **Sudah ada** di Workspace Keperawatan → Pengkajian Pasien → Asesmen Edukasi, sebagai instrumen klinis `EDUCATION_ASSESSMENT` yang sudah diselaraskan dengan V1. **Tidak dibawa ke Workspace Admisi** (`DEC-RWA-003`, v`0.2`) | Di luar cakupan PRD ini | `inpatient-nursing-constants.js:54@1f889d67c`; `ClinicalInstrumentDraftSeeder.cs:634-728@671191eb1` |
| 4 | Gelang & Label | Tidak ada cetak gelang. Data yang dibutuhkan sudah ada: nama, No. RM, tanggal lahir, jenis kelamin, penanda bayi baru lahir, ibu, dan nomor kartu penjamin per kunjungan | `MISSING / NEW` (cetak + log) + `REUSE WITH ADAPTER` (data) | `MstPatient.cs@671191eb1`; `RegPatientEncounterGuarantor.cs@671191eb1` |
| 5 | Permintaan Privasi | Tidak ada | `MISSING / NEW` | Pencarian nama kelas `*Privacy*` pada `Areas/**/*.cs` backend Final `@671191eb1`: nol hasil |
| 6 | IPD | Data tersebar di master pasien, penjamin kunjungan, episode, dan penempatan bed. **Sebagian isian IPD tidak punya sumber**: pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat KTP vs domisili, alamat kantor | `REUSE WITH ADAPTER` + gap data (`OD-RWA-15`) | `MstPatient.cs@671191eb1` (tidak ada kolom pekerjaan/kewarganegaraan/RT-RW) |
| 7 | Pelunasan Deposit | Billing **sudah** menghitung kekurangan deposit per episode dari **kebijakan deposit** per penjamin dan kelas (`MinimumPolicyAmount`, `TotalReceived`, `PolicyShortfallAmount`, `FollowUpIntervalDays`). Surat pernyataannya tidak ada | `REUSE WITH ADAPTER` (Billing) + `MISSING / NEW` (surat) | `BillingPatientFundsController.cs:108-132@671191eb1`; `EpisodeDepositSummaryDtos.cs:3-24@671191eb1`; `MstDepositPolicy.cs@671191eb1` |
| 8 | Estimasi Biaya | Tidak ada estimasi biaya rawat inap. Yang ada hanya perkiraan harga per pesanan penunjang (`coverage-status`). Estimasi otomatis **ditunda** di blueprint (`RWI-CAP-027` sebagian) | `MISSING / NEW` + `OPEN DECISION` | `InpatientAncillaryOrderController.cs:22@671191eb1`; `episode-rawat-inap/04-prd-to-mvp.md:295` |
| 9 | Selisih Biaya | Tidak ada. Jenis penjamin per kunjungan (tunai/asuransi/perusahaan) sudah tersedia | `MISSING / NEW` | `RegPatientEncounterGuarantor.cs@671191eb1` |
| 10 | Mp benefit | Tidak ada. **Tidak ada sumber nilai benefit** makanan pendamping. Aturan tanggungan asuransi per penjamin, kelas, dan paket (`MstInsuranceCoverageRule`, ada `MaxCoverageAmount`) bisa menjadi sumbernya | `OPEN DECISION` | `MstInsuranceCoverageRule.cs@671191eb1` |
| 11 | Nilai Kepercayaan | Tidak ada. Agama pasien sudah ada di master pasien | `MISSING / NEW` | Pencarian nama kelas `*Belief*` pada `Areas/**/*.cs`: nol hasil; `MstPatient.cs:44@671191eb1` (`Religion`) |

### 3.4 Kelemahan V1 yang tidak boleh ditiru

Setiap butir di bawah dibuktikan dari source V1. Butir-butir ini menjadi aturan "jangan" pada epic di bagian 10.

| No | Kelemahan V1 | Akibat bila ditiru | Bukti `@86408f245` (frontend) / `@4be1499cc` (backend) |
|---:|---|---|---|
| 1 | **Nomor kartu asuransi acak** dicetak di label pasien dan kartu MP Benefit bila nomor kartu kosong | Label pasien memuat nomor kartu palsu; klaim penjamin bisa salah | `gelang-label-pasien.jsx:39-43, 433-435`; `mp-benefit-surat-pengantar.jsx:16-20, 37` |
| 2 | **Nilai benefit bawaan Rp 500.000** bila data benefit kosong | Pasien mendapat kupon dengan nilai yang tidak pernah dijanjikan penjamin | `mp-benefit-surat-pengantar.jsx:51` |
| 3 | **Kekurangan deposit dihitung dari angka tetap Rp 100.000.000** dikurangi deposit masuk | Pasien kelas 3 yang sudah membayar penuh deposit wajibnya Rp 2.000.000 tetap tercetak "kurang Rp 98.000.000" | `pelunasan-deposit-pasien.jsx:18, 113-117` |
| 4 | **Surat Pelunasan Deposit tidak tersimpan**; tanda tangan hanya ada di memori peramban | Bukti hukum hilang begitu halaman ditutup; tidak bisa dicetak ulang | `pelunasan-deposit-pasien.jsx:164, 1332-1347` (tombol hanya Cetak) |
| 5 | **Tanda tangan "Kepala Ruangan" diambil dari siapa pun yang sedang masuk**, dan nama petugas di cetakan diambil dari pengguna yang **mencetak**, bukan yang menandatangani | Lembar persetujuan memuat nama dan tanda tangan orang yang salah | `form-general-consent.jsx:413, 823-832`; `print-general-consent.jsx:466-468`; `permintaan-privasi-pasien.jsx:122-150, 444-454` |
| 6 | **Hapus permanen** dokumen bertanda tangan | Jejak hukum dan rekam medis hilang | `form-general-consent.jsx:301-325`; `form-handover-pasien.jsx:475-503`; `form-nilai-kepercayaan.jsx:355-378` |
| 7 | **Data pasien boleh diketik bebas** di Surat Pernyataan Selisih Biaya (nama, alamat, No. RM, kelas) | Surat bisa memuat identitas yang berbeda dari rekam medis | `selisih-biaya-pasien.jsx:154-157, 632-668` |
| 8 | **Nama penanda tangan Permintaan Privasi dikirim tetapi tidak punya kolom** di backend | Nama hilang diam-diam | Frontend `permintaan-privasi-pasien.jsx:204`; backend `Areas/ManajemenKesehatan/RawatInap/Models/PermintaanPrivasi.cs` |
| 9 | Daftar kerabat dan permintaan khusus **digabung dengan koma** dalam satu teks | Nama yang mengandung koma terpecah; tidak bisa dicari per kerabat | `permintaan-privasi-pasien.jsx:186-198` |
| 10 | Permintaan Privasi dan Selisih Biaya **hanya bisa ditambah**, tidak bisa dibuka lagi | Petugas membuat dokumen ganda setiap kali membuka menu | `permintaan-privasi-pasien.jsx:170-225`; `selisih-biaya-pasien.jsx:229-289` |
| 11 | **Identitas rumah sakit, kota "Jakarta", nomor telepon, email, dan kode formulir ditanam di kode** | Ganti alamat atau revisi formulir berarti ganti program | `gelang-label-pasien.jsx:315, 529-542`; `print-general-consent.jsx:474-475, 509`; `estimasi-biaya-rinci/index.jsx:411-425`; `selisih-biaya-pasien.jsx:502, 534` |
| 12 | Tipe kamar Umum/Khusus ditebak dari **nama kelas atau kamar yang mengandung "ICU"/"ISOLASI"** | Kamar isolasi bernama "Melati Khusus" terbaca Umum | `form-general-consent.jsx:170-192` |
| 13 | Peran ditentukan dari **teks nama tipe pengguna** ("Administrasi", "Customer Service", "Perawat") | Mengganti nama peran mematikan fitur; tidak sesuai pola hak akses Final | `form-handover-pasien.jsx:59-64, 126-143` |
| 14 | Template checklist dicari dengan **nama "Serah Terima Pasien"**, dan bila gagal **dicoba ulang tanpa batas** setiap 2 detik | Typo nama template membuat layar berputar terus | `form-handover-pasien.jsx:214-237` |
| 15 | Topik edukasi "Lainnya" dikenali dari **GUID yang ditanam di kode**; asesmen dan detailnya disimpan **dua langkah terpisah**. *Tidak lagi relevan untuk Workspace Admisi sejak v`0.2` (`DEC-RWA-003`); dicatat sebagai rujukan bagi keperawatan* | Data yatim bila langkah kedua gagal | `add-assesment-edukasi.jsx:260-279, 395-396` |
| 16 | Estimasi Biaya Rekap mengelompokkan biaya dengan **mencocokkan potongan kata** pada nama tindakan ("operasi", "alat", "kamar", "visit") dan membagi total dengan lama rawat | Tindakan "Konsultasi Pra Operasi" masuk kelompok operasi | `estimasi-biaya-rekap-pasien.jsx:49-94` |
| 17 | Estimasi Biaya Rinci sebenarnya **membaca tagihan berjalan**, bukan perkiraan | Judul "Prakiraan" menyesatkan pasien | Backend `Areas/ManajemenKesehatan/Kasir/Services/PerkiraanBillingRanapService.cs:13` |
| 18 | Label pasien pada Nilai Kepercayaan dibuat sebagai **gambar tangkapan layar** lalu diunggah | Berkas gambar tidak sinkron bila data pasien dikoreksi | `form-nilai-kepercayaan.jsx:282-308, 443-452` |
| 19 | QR gelang dan MP Benefit berisi **teks data pasien lengkap** ditambah ID acak yang berubah setiap halaman dibuka | Data pribadi terbaca pemindai mana pun; QR tidak bisa diverifikasi | `gelang-label-pasien.jsx:452-477`; `utils/useRandomQrCode.jsx:6-21, 60-63` |
| 20 | Isian IPD "Nilai Kepercayaan", "Permintaan Privat", dan "Orang yang diberi kewenangan mendapatkan informasi" **selalu kosong** | Dokumen ringkasan tidak meringkas apa pun | `ipd-surat-pengantar.jsx:548-551, 707-716` |

### 3.5 Celah lintas modul di Final yang ikut terungkap

| No | Temuan | Dampak ke Workspace Admisi | Bukti |
|---:|---|---|---|
| G-RWA-01 | **Tidak ada master tanda tangan petugas** di backend. Layar Pengaturan Pengguna memanggil `/v1/Auth/signature/register`, tetapi endpoint itu tidak ada di backend | Cetakan V1 memuat gambar tanda tangan petugas. Final belum bisa menyediakannya → usulan atestasi petugas (`OD-RWA-04`) | `src/components/view/settings/user/user-settings-client.jsx:233@1f889d67c`; `Models/ApplicationUser.cs@671191eb1` tanpa kolom tanda tangan; pencarian `signature` pada controller backend: nol hasil |
| G-RWA-02 | **Identitas rumah sakit juga ditanam di kode cetak Final**: kop surat bawaan, salinan formulir persetujuan, dan 13 berkas cetak lain | Dokumen admisi wajib memakai profil rumah sakit; perbaikan cetakan Final lain di luar PRD ini | `src/components/features/surat-component/kop-surat.jsx:4-6@1f889d67c`; `inpatient-admission-flow-constants.jsx:936-939@1f889d67c`; profil RS tersedia di `MstHospitalSite` |
| G-RWA-03 | String hak akses master butir administrasi di peta modul (`MstInpatientClearanceItem : Read`) **berbeda** dari source (`InpatientClearanceItem : Read`) | Dokumen ini memakai string source | `02-module-map.md:237`; `InpatientClearanceItemController.cs:66@671191eb1` |
| G-RWA-04 | Master pasien tidak punya pekerjaan, kewarganegaraan, sapaan (Tn./Ny./An.), RT/RW, kelurahan, alamat KTP terpisah dari domisili, dan alamat kantor | Isian IPD dan Selisih Biaya tertentu tidak punya sumber | `MstPatient.cs@671191eb1` |

### 3.6 Nama yang cocok untuk ruang kerja ini

Pemilik meminta usulan nama selain "Admisi". Empat pilihan dinilai:

| Pilihan | Dasar | Kelebihan | Kekurangan |
|---|---|---|---|
| **Workspace Admisi** | Istilah yang sudah dipakai Final: menu "Admisi Rawat Inap", status episode `Admitted`, tombol "Batalkan Admisi" | Singkat; sejajar dengan "Workspace Dokter" dan "Workspace Keperawatan" yang dinamai menurut **peran** pemakainya | Bisa disangka layar pendaftaran, padahal pendaftaran rawat inap sudah ada di menu Admisi Rawat Inap |
| **Workspace PPRI** (Penerimaan Pasien Rawat Inap) | Istilah yang **tercetak di dokumen V1** ("Petugas PPRI / Admission"; "bagian Penerimaan Pasien Rawat Inap (Admision)"). Di pedoman rekam medis rumah sakit Indonesia, unit ini lazim disebut TPPRI (Tempat Penerimaan Pasien Rawat Inap) | Sama dengan nama unit kerja di rumah sakit client; jelas siapa pemakainya | Singkatan asing bagi pengguna baru; harus selalu ditulis kepanjangannya |
| **Workspace Administrasi Pasien** | Isi ruang kerja memang dokumen administrasi | Menggambarkan dokumen biaya dan persetujuan | Terlalu umum; mudah tertukar dengan kasir/Billing |
| **Workspace Dokumen & Persetujuan Pasien** | Isi ruang kerja | Deskriptif | Panjang; tidak mewakili gelang dan serah terima |

**Usulan (`DEC-RWA-001`, menunggu `OD-RWA-01`):** label tombol **"Workspace Admisi"**, judul halaman **"Ruang Kerja Admisi Rawat Inap"**, dan subjudul **"Penerimaan Pasien Rawat Inap (PPRI)"**. Alasannya: ketiga ruang kerja di Detail Episode dinamai menurut peran (Dokter, Keperawatan, Admisi), kata "Admisi" sudah dikenal petugas, dan istilah PPRI dari dokumen V1 tetap tampil supaya petugas yang terbiasa dengan V1 langsung mengenalinya.

---

## 4. Visi produk

Rantai keterhubungan data yang ingin dicapai, dari pasien diterima sampai dokumen lengkap:

1. Petugas admisi mengunci admisi lewat alur Admisi Rawat Inap yang sudah ada. Episode berstatus `Admitted`.
2. Detail Episode menampilkan tombol **Workspace Admisi** di samping Workspace Dokter.
3. Ruang kerja membaca episode, penempatan bed, pasien, wali dan kontak darurat, penjamin kunjungan, alergi, DPJP, dan ringkasan deposit Billing **tanpa menyalinnya**.
4. Sistem menghitung dokumen mana yang **wajib** untuk pasien ini — misalnya Selisih Biaya hanya bila penjaminnya asuransi atau perusahaan — lalu menampilkan kelengkapannya.
5. Setiap form terisi otomatis dari data pada langkah 3. Petugas hanya mengisi yang memang belum diketahui sistem, misalnya hubungan penanda tangan dan daftar kerabat.
6. Penanda tangan menandatangani (mode kertas atau digital, `OD-RWA-02`). Petugas menandatangani lewat akunnya sendiri.
7. Dokumen dikunci. Identitas dan angka di dalamnya **dibekukan** saat ditandatangani, dan cetakan selalu sama dengan yang ditandatangani.
8. Dokumen yang perlu dikoreksi dibuat **versi baru**; versi lama tetap terbaca.
9. Checklist Serah Terima menyarankan butir yang sudah terbukti selesai (misalnya "Pasang Gelang" setelah gelang dicetak), lalu CRO dan perawat ruangan menandatanganinya.
10. Data Dasar Rawat Inap (IPD) merangkum seluruh dokumen di atas menjadi satu lembar.
11. Persetujuan umum yang sudah ditandatangani dapat menandai butir daftar periksa penutupan episode secara otomatis (bila `RWI-RULE-025` disetujui pemilik privasi).

---

## 5. Batas MVP

### 5.1 Titik mulai

1. Episode rawat inap sudah berstatus `Admitted` atau `DischargePending` (sudah melewati langkah Konfirmasi alur admisi).
2. Pengguna membuka Detail Episode dan memiliki hak baca dokumen admisi.
3. Pengguna menekan tombol **Workspace Admisi**.

### 5.2 Titik akhir

1. Seluruh dokumen yang **wajib** untuk pasien itu berstatus **Lengkap**, dan kelengkapan menunjukkan angka penuh, misalnya "7 dari 7".
2. Gelang identitas sudah dicetak minimal satu kali dan tercatat di log cetak.
3. Checklist Serah Terima Pasien Baru sudah ditandatangani petugas admisi, CRO, dan perawat penerima.
4. Setiap dokumen dapat dicetak ulang dengan isi yang sama seperti saat ditandatangani.
5. Saat episode `Closed` atau `Cancelled`, seluruh dokumen menjadi **hanya-baca**; cetak ulang tetap boleh dengan alasan.

### 5.3 Di luar batas MVP

Pendaftaran dan penempatan pasien (sudah ada di alur Admisi Rawat Inap), penerimaan dan pengembalian deposit (milik Billing, `EPIC RI-35`), persetujuan tindakan medis khusus per tindakan (`CAP-009`), dan kemampuan yang ditunda di bagian 8.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di dalam MVP | Pola hak akses (usulan, bagian 14) |
|---|---|---|
| Petugas admisi / PPRI | Membuka ruang kerja, mengisi dan menyimpan semua dokumen, merekam tanda tangan pasien/keluarga, menandatangani sebagai petugas admisi, mencetak gelang, label, IPD, dan surat | `InpatientAdmissionDocument : Read, Create, Update, Sign, Print` |
| CRO (*Customer Relation Officer*) | Menandatangani checklist Serah Terima sebagai CRO; membaca dan mencetak | `Read`, `SignAsCro`, `Print` |
| Perawat ruangan penerima | Menandatangani checklist Serah Terima sebagai perawat yang menerima pasien; membaca semua dokumen | `Read`, `SignAsNurse`, `Print` |
| Kepala ruangan | Menandatangani General Consent dan Permintaan Privasi sebagai Kepala Ruangan (label V1) | `Read`, `SignAsHeadNurse` |
| Kasir / petugas Billing | Membaca surat Pelunasan Deposit untuk tindak lanjut; mengatur kebijakan deposit (sudah ada) | `Read`, `ReadAmount`; `BillingDeposit : Read` |
| Supervisor admisi | Membatalkan dokumen yang sudah ditandatangani dengan alasan; mengizinkan cetak ulang tertentu | `Cancel` |
| Pasien / keluarga / penanggung jawab | Menandatangani (kertas atau digital) | Tidak punya akun |
| Admin rumah sakit | Mengatur butir checklist serah terima, kode formulir, dan profil kop surat | `InpatientClearanceItem : Create, Update` (sudah ada); pengaturan Rawat Inap |

---

## 7. Pemilihan kemampuan MVP

Setiap kemampuan diuji dengan dua pertanyaan kontrak PRD: (1) tanpa kemampuan ini, apakah satu admisi nyata bisa selesai dengan dokumen lengkap? (2) bila tidak, adakah jalan sementara yang aman dan dapat diaudit? Selain itu, client sudah menyepakati form V1, sehingga **hilangnya form yang sudah disepakati dihitung sebagai "tidak bisa selesai"** (`DEC-RWA-002`, usulan). Pengecualiannya hanya Assessment Edukasi, yang dicabut pemilik dari admisi (`DEC-RWA-003`).

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Ruang kerja admisi per episode + tombol di Detail Episode + kelengkapan dokumen | `CAP-RWA-01` (usulan) | **Wajib**; tanpa ini sembilan dokumen lain tidak punya tempat |
| General Consent (form V1 + Surat Persetujuan 12 butir) | `CAP-RWA-02` → `RWI-CAP-031` | **Wajib, tetapi `OPEN DECISION`** (`OD-RWA-02`, `OD-RWA-03`). Jalan sementara selama keputusan belum turun: cetak tanpa simpan yang sudah ada (`RWI-DEC-077`) |
| Serah Terima Pasien Baru (checklist + tiga tanda tangan petugas) | `CAP-RWA-03` | **Wajib**; satu-satunya bukti bahwa pasien benar-benar diserahkan ke ruangan |
| Gelang dan Label Pasien | `CAP-RWA-05` | **Wajib**; gelang adalah sarana identifikasi pasien (keselamatan pasien) |
| Permintaan Privasi | `CAP-RWA-06` | **Wajib**; hak pasien yang sudah disepakati client |
| Data Dasar Rawat Inap (IPD) | `CAP-RWA-07` | **Wajib**; butir 9 checklist Serah Terima |
| Pelunasan Deposit (surat pernyataan) | `CAP-RWA-08` | **Wajib, tetapi `OPEN DECISION`** (`OD-RWA-14`, konfirmasi Billing) |
| Estimasi Biaya — Rekap (Penjelasan Prakiraan Biaya) | `CAP-RWA-09` → `RWI-CAP-027` sebagian | **Wajib, tetapi `OPEN DECISION`** (`OD-RWA-07`) |
| Selisih Biaya (surat pernyataan) | `CAP-RWA-11` | **Wajib** untuk pasien penjamin asuransi/perusahaan |
| Nilai dan Kepercayaan Pasien | `CAP-RWA-13` | **Wajib**; hak pasien dan keselamatan (misalnya menolak transfusi) |
| Siklus hidup dokumen: konsep, kunci, versi, batal beralasan, log cetak | `CAP-RWA-17` | **Wajib**; tanpa ini kelemahan V1 nomor 4, 6, dan 10 terulang |
| Tanda tangan mode kertas + atestasi petugas | `CAP-RWA-14` (sebagian), `CAP-RWA-15` | **Wajib**; jalan aman selama tanda tangan digital belum diputuskan |
| Kop surat dan kode formulir dari pengaturan | `CAP-RWA-16` | **Wajib**; tanpa ini kelemahan V1 nomor 11 terulang |
| Tanda tangan digital pasien/keluarga di layar | `CAP-RWA-14` (sebagian) | **`OPEN DECISION`** (`OD-RWA-02`) — paritas V1, tetapi menyangkut hukum dan privasi |

**Dibatalkan:** `CAP-RWA-04` Assessment Edukasi Pasien — dicabut dari Workspace Admisi oleh pemilik produk pada 7 Oktober 2026 (`DEC-RWA-003`). Ini **bukan** kemampuan yang ditunda, sehingga tidak masuk bagian 8: asesmen edukasi tetap berjalan penuh di Workspace Keperawatan, dan tidak ada pekerjaan yang hilang dari petugas mana pun.

---

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Estimasi Biaya — Rinci (rincian tagihan berjalan per kelompok) | `CAP-RWA-10` | Isinya adalah tagihan berjalan milik Billing, bukan perkiraan (kelemahan V1 nomor 17). Hak melihat rupiah per butir bagi petugas admisi belum diputuskan; `RWI-DEC-170` hanya mengatur bangsal (`OD-RWA-08`) | Tab "Rinci" menampilkan pemberitahuan bahwa rincian tagihan berjalan dicetak kasir dari modul Billing, beserta tautannya bila pengguna berhak |
| MP Benefit (kartu Benefit Makanan Pendamping) | `CAP-RWA-12` | Tidak ada sumber nilai benefit di Final, dan alur penukaran kupon (kantin atau Gizi, sekali atau per hari) belum diputuskan (`OD-RWA-09`). Mencetak tanpa sumber berarti mengulang kelemahan V1 nomor 1 dan 2 | Menu **tidak ditampilkan** (bukan *placeholder*). Petugas mencatat benefit di butir 15 "Lain-lain" checklist Serah Terima dan memberi kupon manual sesuai polis |
| Tanda tangan elektronik tersertifikasi (TTE dari penyelenggara sertifikasi elektronik) | — | Butuh integrasi penyedia sertifikat dan keputusan hukum | Mode kertas atau tanda tangan goresan digital dengan hash (`OD-RWA-02`) |
| Peringatan Nilai Kepercayaan dan Privasi di header Workspace Keperawatan dan Workspace Dokter | — | Mengubah dua sub-modul lain yang sudah `approved`; perlu amandemen terpisah | Terbaca di Workspace Admisi, IPD, dan endpoint baca yang disediakan `FR-RWA-112` |
| Daftar tindak lanjut surat deposit jatuh tempo di modul Billing | — | Perubahan modul Billing | Peringatan "jatuh tempo terlewati" di Workspace Admisi dan Detail Episode (`FR-RWA-085`) |
| Pindai (*scan*) lembar kertas bertanda tangan | — | Kebijakan penyimpanan berkas belum diputuskan | Lembar kertas disimpan di berkas rekam medis seperti hari ini |
| Ruang kerja admisi untuk ODC/rawat jalan (judul IPD V1 "Rawat Inap/ODC") | — | Alur ODC di Final terpisah dari episode rawat inap | IPD hanya untuk episode rawat inap |
| Perluasan master pasien (pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat KTP/domisili, alamat kantor, sapaan) | — | Milik Patient Management; perlu persetujuan pemiliknya (`OD-RWA-15`) | Isian IPD tanpa sumber dicetak sebagai garis kosong untuk diisi tangan, sama seperti hasil cetak V1 hari ini |

---

## 9. Alur bisnis target

### 9.1 `FLOW-RWA-MVP-001` — Menyelesaikan dokumen penerimaan pasien

| Unsur | Isi |
|---|---|
| Tujuan | Seluruh dokumen penerimaan pasien rawat inap lengkap, tersimpan, dan tercetak pada hari pasien masuk |
| Pelaku | Petugas admisi (utama), CRO, perawat ruangan, kepala ruangan, pasien/keluarga |
| Pemicu | Admisi dikunci pada alur Admisi Rawat Inap |
| Prasyarat | Episode `Admitted`; bed sudah ditempati atau dipesan; penjamin kunjungan tercatat |

Langkah utama:

1. Petugas admisi membuka Detail Episode Tn. Budi Santoso, lalu menekan **Workspace Admisi**.
2. Sistem menampilkan header pasien dan **Kelengkapan Dokumen Admisi: 0 dari 7**.
3. Petugas membuka **Gelang & Label**, memeriksa pratinjau, lalu mencetak gelang dewasa. Log cetak mencatat cetakan ke-1.
4. Petugas membuka **General Consent**, memilih hubungan "Istri". Nama dan alamat Ny. Rina Santoso terisi dari data relasi pasien. Tipe kamar "Umum" dan "Kelas 2 — Melati 03 (Bed B)" terisi dari episode.
5. Ny. Rina menandatangani; kepala ruangan menandatangani. Dokumen **Lengkap**, lalu dicetak.
6. Karena Billing mencatat kekurangan deposit Rp 3.000.000, petugas membuka **Pelunasan Deposit**, memilih Ny. Rina sebagai yang menyatakan, menetapkan jatuh tempo, lalu merekam tanda tangan.
7. Karena penjamin Budi adalah asuransi, petugas membuka **Selisih Biaya** dan menyelesaikan surat pernyataan.
8. Petugas mengisi **Nilai Kepercayaan** ("Tidak menerima transfusi darah"), dan bila diminta, **Permintaan Privasi**.
9. Petugas mencetak **IPD**. Lembar itu sudah memuat nilai kepercayaan, penerima informasi, dan data penjamin.
10. Petugas membuka **Serah Terima Pasien**. Butir "Pasang Gelang", "Formulir IPD", dan "Pernyataan Pelunasan Deposit" sudah bertanda **saran sistem: Sudah**. Petugas mengonfirmasi seluruh butir, lalu menandatangani sebagai petugas admisi.
11. CRO Dewi membuka dokumen yang sama dan menandatangani sebagai CRO.
12. Perawat Andi di ruangan menerima pasien dan menandatangani sebagai perawat penerima. Serah Terima **Lengkap**.
13. Kelengkapan menjadi **7 dari 7**. Peringatan "dokumen admisi belum lengkap" di Detail Episode hilang.

Aturan bisnis yang berlaku: bagian 10 per epic dan invariant di bagian 11.

Perubahan status utama:

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Simpan pertama | `Draft` | Petugas admisi | Episode `Admitted`/`DischargePending` |
| `Draft` | Kunci dan minta tanda tangan | `AwaitingSignature` | Petugas admisi | Seluruh isian wajib terisi |
| `AwaitingSignature` | Tanda tangan terakhir yang wajib masuk | `Completed` | Penanda tangan yang sah | Semua slot tanda tangan wajib terisi |
| `Completed` | Koreksi | `Superseded` (versi lama) + `Draft` (versi baru) | Petugas admisi | Alasan koreksi wajib |
| `Draft`/`AwaitingSignature`/`Completed` | Batalkan | `Cancelled` | Pembuat (untuk `Draft`); supervisor (selain itu) | Alasan minimal 10 karakter |

Jalur tidak normal:

| Kejadian | Yang dilakukan sistem | Yang dilakukan petugas |
|---|---|---|
| Pasien datang tidak sadar tanpa keluarga | Admisi dan perawatan tetap berjalan; dokumen wajib tetap `Draft`/kosong dan tampil di kelengkapan | Melengkapi saat keluarga tiba, seperti contoh `RWI-RULE-025` |
| Data relasi pasien tidak cocok dengan hubungan yang dipilih | Isian nama dan alamat dibuka untuk diketik manual, dengan keterangan "tidak ditemukan di data wali/kontak darurat" | Mengisi manual |
| Billing tidak bisa dihubungi saat membuka Pelunasan Deposit | Angka kekurangan tidak ditampilkan; tombol simpan dinonaktifkan dengan pesan dan tombol **Coba Lagi** | Mencoba lagi; tidak mengetik angka sendiri |
| Dua petugas mengubah dokumen yang sama | Penyimpanan kedua ditolak (kode 409) dengan pesan "dokumen sudah diubah petugas lain, muat ulang" | Memuat ulang dan mengulang perubahan |
| Petugas salah memilih penanda tangan setelah dokumen `Completed` | Dokumen tidak bisa diubah | Membuat versi baru dengan alasan koreksi |
| Episode dibatalkan (Batalkan Admisi) | Semua dokumen menjadi hanya-baca; cetak ulang diberi tanda "ADMISI DIBATALKAN" | — |
| Gelang rusak atau hilang | Cetak ulang diminta alasan | Memilih alasan "rusak", "hilang", atau "data berubah" |

Hasil akhir: seluruh dokumen wajib berstatus `Completed`, tercetak, dan dapat ditelusuri siapa membuat, menandatangani, mencetak, dan mengoreksinya. Perawat ruangan, kasir, dan rekam medis membaca dokumen yang sama.

---

## 10. Epic dan functional requirement

Disposisi backend memakai lima istilah baku: `EXISTING / REUSE`, `EXTEND`, `MISSING / NEW`, `LEGACY REFERENCE`, `OPEN DECISION`.

### EPIC-RWA-01 — Ruang kerja admisi dan tombol di Detail Episode (`CAP-RWA-01`)

**Tujuan:** petugas admisi punya satu tempat kerja per episode yang tampil dan berperilaku seperti Workspace Keperawatan.

**Disposisi:** frontend `MISSING / NEW` di atas template `EXISTING / REUSE`; backend `MISSING / NEW` untuk ringkasan kelengkapan, `EXISTING / REUSE` untuk episode, penempatan, pasien, alergi, dan ringkasan deposit.

> **FR-RWA-001 — Tombol Workspace Admisi di Detail Episode**
>
> Detail Episode menampilkan tombol **Workspace Admisi** pada kelompok aksi utama, tepat sesudah tombol Workspace Dokter, hanya bagi pengguna yang memiliki `InpatientAdmissionDocument : Read`.
>
> **Contoh:** Perawat Andi dan petugas admisi Sari membuka Detail Episode `RI-261007-0001`. Keduanya melihat urutan tombol: Workspace Keperawatan, Workspace Dokter, **Workspace Admisi**, Penutupan Episode, dan seterusnya. Pengguna gizi tanpa hak baca dokumen admisi tidak melihat tombol itu.

> **FR-RWA-002 — Template Workspace Keperawatan**
>
> Ruang kerja memakai empat wilayah yang sama dengan Workspace Keperawatan: header halaman berisi judul dan tombol (Ringkasan Kelengkapan, Kembali ke Detail Episode, Segarkan); batas keadaan (memuat, gagal, ditolak, kosong, hanya-baca); header pasien; navigasi kiri; dan isi bagian dengan bilah tab kedua. Menu dan tab tersimpan di alamat halaman (`?section=` dan `?tab=`), sehingga tautan bisa dibagikan.
>
> **Contoh:** Sari menyalin alamat `…/episodes/{id}/admission?section=handover&tab=form` ke Dewi. Dewi yang membukanya langsung berada di form Serah Terima.

> **FR-RWA-003 — Header pasien versi admisi**
>
> Header menampilkan: nama, No. RM, jenis kelamin, umur, tanggal lahir; nomor episode, waktu admisi, kelas, kamar/bed, DPJP; jenis dan nama penjamin serta nomor kartu; kontak darurat utama; alergi; dan **status deposit** dari Billing ("Kurang Rp 3.000.000", "Cukup", atau "Tidak wajib"). Status deposit hanya tampil bagi pemegang `InpatientAdmissionDocument : ReadAmount`; yang lain melihat "Deposit: lihat kasir".
>
> **Contoh:** Header Tn. Budi menampilkan "Penjamin: Asuransi — PT Asuransi Sehat Sentosa · Kartu 7788-0012-3456", "Kontak darurat: Ny. Rina Santoso (istri)", dan "Deposit kurang Rp 3.000.000".

> **FR-RWA-004 — Navigasi kiri sepuluh menu berurutan V1**
>
> Navigasi kiri bertajuk "DOKUMEN ADMISI" memuat sepuluh menu dengan urutan V1, **tanpa** Assessment Edukasi Pasien (`DEC-RWA-003`): General Consent, Serah Terima Pasien, Gelang & Label Pasien, Permintaan Privasi, IPD, Pelunasan Deposit, Estimasi Biaya, Selisih Biaya, MP Benefit, Nilai Kepercayaan. Menu yang kemampuannya ditunda (MP Benefit) **tidak ditampilkan** sampai kemampuannya dikirim, sehingga MVP menampilkan sembilan menu. Setiap menu menampilkan lencana status: Lengkap, Konsep, Menunggu tanda tangan, Belum dibuat, atau Tidak diperlukan.
>
> **Contoh:** Untuk pasien tunai, menu Selisih Biaya berlencana "Tidak diperlukan" dan tidak dihitung ke kelengkapan.

> **FR-RWA-005 — Kelengkapan dokumen admisi**
>
> Sistem menghitung dokumen wajib untuk episode itu dengan aturan berikut, lalu menampilkan "Dokumen admisi *x* dari *y* lengkap" di header halaman dan sebagai daftar pada tombol Ringkasan Kelengkapan.
>
> | Dokumen | Wajib bila |
> |---|---|
> | General Consent | Selalu |
> | Serah Terima Pasien Baru | Selalu |
> | Gelang | Selalu (minimal satu kali dicetak) |
> | IPD | Selalu (minimal satu kali dicetak) |
> | Nilai Kepercayaan | Selalu |
> | Selisih Biaya | Penjamin utama kunjungan adalah asuransi atau perusahaan |
> | Pelunasan Deposit | Billing mencatat `PolicyShortfallAmount` lebih dari 0 |
> | Estimasi Biaya (Rekap) | Episode punya pemesanan ruang bedah yang aktif, atau petugas menandai "ada rencana tindakan/operasi" (`OD-RWA-11`) |
> | Permintaan Privasi, Label pasien | Tidak dihitung (opsional) |
>
> **Contoh:** Tn. Budi (asuransi, deposit kurang Rp 3.000.000, tanpa rencana operasi) → wajib 7 dokumen: General Consent, Serah Terima, Gelang, IPD, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit. Ny. Wati (tunai, deposit cukup) → wajib 5 dokumen.

> **FR-RWA-006 — Kelengkapan hanya memperingatkan**
>
> Dokumen admisi yang belum lengkap **tidak menahan** penempatan, perawatan, transfer, keputusan pulang, maupun keluar ruangan. Detail Episode menampilkan peringatan "Dokumen admisi belum lengkap: 2 (Selisih Biaya, Serah Terima)". Satu-satunya penahan adalah persetujuan umum pada daftar periksa penutupan, dan hanya bila `RWI-RULE-025` disetujui pemilik privasi (`OD-RWA-03`).
>
> **Contoh:** Pukul 14.00 dokter memutuskan Budi pulang walaupun Selisih Biaya belum ditandatangani. Keputusan pulang tetap tersimpan, dan peringatan tetap tampil.

> **FR-RWA-007 — Hanya-baca untuk episode tertutup**
>
> Bila episode `Closed` atau `Cancelled`, seluruh tombol tulis disembunyikan dan server menolak penulisan dengan kode 409. Cetak ulang tetap boleh, wajib beralasan.
>
> **Contoh:** Episode Budi ditutup 10 Oktober 2026. Pada 12 Oktober Sari membuka General Consent; tombol Ubah dan Batalkan tidak ada, tombol Cetak Ulang meminta alasan.

> **FR-RWA-008 — Kegagalan konteks memblokir penulisan**
>
> Bila identitas pasien, episode, atau penempatan gagal dimuat, ruang kerja menampilkan "DATA PASIEN TIDAK DAPAT DIMUAT" beserta tombol Coba Muat Ulang, dan **tidak menampilkan form apa pun**, sama seperti Workspace Keperawatan.
>
> **Contoh:** Server episode tidak menjawab. Sari tidak melihat form General Consent kosong yang bisa salah disimpan untuk pasien lain.

### EPIC-RWA-02 — General Consent (`CAP-RWA-02`) — **`OPEN DECISION`**

**Tujuan:** persetujuan umum rawat inap terekam per episode dengan isian form V1, tercetak bersama Surat Persetujuan Pasien Rawat Inap 12 butir yang sudah ada di Final.

**Disposisi:** `EXTEND` tabel persetujuan Clinical Management (jenis `Admission`); `CONFLICT` dengan `RWI-DEC-077` (cetak tanpa simpan) → **`OPEN DECISION`** (`OD-RWA-02`, `OD-RWA-03`). Epic ini **tidak masuk gelombang** sampai keputusan turun.

> **FR-RWA-020 — Isian form General Consent V1**
>
> Form memuat isian V1 apa adanya: Hubungan dengan Pasien (Diri Sendiri, Suami, Istri, Anak, Orang Tua, Lainnya), Nama dan Alamat penanda tangan, Tipe Kamar Rawat (Umum; Khusus ICU/Isolasi), Kelas Kamar/Kamar Rawat, Sudah Menerima Panduan Rawat Inap (Ya/Tidak), Keterangan, tanda tangan penanda tangan, dan tanda tangan Kepala Ruangan. Rincian di Lampiran A.1.

> **FR-RWA-021 — Pengisian otomatis penanda tangan**
>
> Bila hubungan "Diri Sendiri", nama dan alamat diambil dari master pasien. Bila Suami/Istri/Anak/Orang Tua, sistem mencari data relasi pasien (`MstPatientRelationship`) lalu kontak darurat (`MstPatientEmergencyContact`) dengan hubungan yang sama. Bila tidak ditemukan atau hubungan "Lainnya", isian dibuka untuk diketik manual dengan keterangan.
>
> **Contoh:** Hubungan "Istri" → ditemukan relasi `Spouse` bernama Rina Santoso beralamat "Jl. Kenanga No. 5, Bekasi". Bila Budi tidak punya data relasi istri, isian kosong dan terbuka.

> **FR-RWA-022 — Tipe kamar dari data episode, bukan dari nama**
>
> Tipe kamar "Khusus (ICU/Isolasi)" dipilih otomatis bila episode `RequiresIsolation = true` atau unit layanan penempatannya bertipe perawatan intensif; selain itu "Umum". Petugas boleh mengubahnya. Menebak dari nama kamar (kelemahan V1 nomor 12) dilarang.
>
> **Contoh:** Kamar "Melati Khusus" yang bukan isolasi tetap terisi "Umum".

> **FR-RWA-023 — Satu persetujuan umum aktif per episode**
>
> Hanya satu persetujuan umum berstatus aktif per episode. Koreksi membuat versi baru dan versi lama berstatus `Superseded`.

> **FR-RWA-024 — Surat 12 butir dan form V1 adalah satu persetujuan**
>
> Bagian General Consent punya dua tab: **Formulir General Consent** (form V1) dan **Surat Persetujuan Pasien Rawat Inap** (cetakan 12 butir yang sudah ada, `FE-INP-18`). Keduanya merujuk satu rekaman persetujuan, dan penerima informasi pasien (isi minimal `RWI-DEC-035`) diisi sekali lalu dipakai keduanya dan IPD.

> **FR-RWA-025 — Penanda daftar periksa penutupan**
>
> Bila `RWI-RULE-025` disetujui, persetujuan umum berstatus `Completed` menandai butir daftar periksa penutupan "persetujuan umum sudah ditandatangani" secara otomatis, dengan nama penanda tangan dan waktu.

### EPIC-RWA-03 — Serah Terima Pasien Baru (`CAP-RWA-03`)

**Tujuan:** penyerahan pasien dari admisi ke ruangan terbukti lengkap dan ditandatangani tiga pihak.

**Disposisi:** `MISSING / NEW` (dokumen dan butirnya); `EXTEND` master Butir Administrasi Rawat Inap supaya bisa menampung jenis "Serah Terima Pasien Baru" dan sub-butir — menambah master baru tidak mungkin tanpa menambah butir menu, padahal kuota `IA-INP-05` sudah habis.

> **FR-RWA-030 — Butir checklist dari master**
>
> Butir checklist diambil dari master berjenis "Serah Terima Pasien Baru", diurutkan menurut urutan master, dengan sub-butir tampil berpoin di bawah induknya. Data awal berisi 15 butir dan 3 sub-butir V1 (Lampiran A.2). Nama dan kode butir **dibekukan** di dokumen saat disimpan, sehingga perubahan master tidak mengubah dokumen lama.
>
> **Contoh:** Admin mengganti nama butir 14 menjadi "Input Kartu Parkir" pada 1 November 2026. Serah terima Budi bertanggal 7 Oktober tetap tercetak "INPUT PARKIR".

> **FR-RWA-031 — Sudah/Belum saling meniadakan, dan wajib dipilih**
>
> Setiap butir dipilih **Sudah** atau **Belum** (tidak bisa keduanya), dengan Keterangan. Dokumen tidak bisa dikunci selama ada butir yang belum dipilih, dan butir **Belum** wajib berketerangan (`OD-RWA-12`, usulan `DEC-RWA-012`; V1 membolehkan kosong).
>
> **Contoh:** Butir 11 "Informasi VIP (khusus VIP)" untuk pasien kelas 2 dipilih **Belum** dengan keterangan "bukan pasien VIP". Bila keterangannya kosong, sistem menolak dengan pesan "Butir 11 berstatus Belum wajib diberi keterangan".

> **FR-RWA-032 — Saran sistem dari dokumen lain**
>
> Sistem menandai **saran: Sudah** pada butir yang terbukti dari dokumen lain, dan petugas tetap wajib mengonfirmasi:
>
> | Butir | Saran Sudah bila |
> |---|---|
> | 4. Penjelasan prakiraan biaya tindakan & deposit | Estimasi Biaya (Rekap) `Completed` |
> | 7. Pernyataan pelunasan deposit | Pelunasan Deposit `Completed` |
> | 9. Formulir IPD | IPD sudah dicetak |
> | 12. Cetak label/stiker/general consent | Label dicetak **dan** General Consent `Completed` |
> | 13. Pasang gelang | Gelang sudah dicetak |
>
> **Contoh:** Pukul 09.50 Sari mencetak gelang Budi. Pukul 10.00 butir 13 tampil "Sudah — saran sistem (gelang dicetak 09.50 oleh Sari)".

> **FR-RWA-033 — Tiga tanda tangan petugas, tiga orang berbeda**
>
> Slot tanda tangan: **Admission** (petugas admisi pembuat), **CRO**, dan **Perawat** penerima. Masing-masing ditandatangani lewat akun penanda tangan sendiri (`Sign`, `SignAsCro`, `SignAsNurse`). Satu akun tidak boleh mengisi dua slot pada dokumen yang sama. Dokumen `Completed` saat ketiga slot terisi.
>
> **Contoh:** Sari (admisi) yang juga punya hak CRO mencoba menandatangani slot CRO setelah menandatangani slot Admission. Sistem menolak dengan kode 422: "Satu petugas tidak boleh menandatangani dua kolom pada serah terima yang sama".

> **FR-RWA-034 — CRO dan perawat hanya menandatangani**
>
> Pengguna dengan hak `SignAsCro` atau `SignAsNurse` tetapi tanpa `Update` melihat dokumen hanya-baca dengan tombol "Tandatangani sebagai CRO/Perawat". Sebelum dokumen dikunci petugas admisi, tombol itu tidak tersedia dan layar menulis "Data serah terima belum dikirim oleh petugas admisi".

> **FR-RWA-035 — Pembaruan tanpa memuat ulang**
>
> Tanda tangan CRO atau perawat tampil di layar petugas lain yang sedang membuka dokumen yang sama paling lambat 30 detik kemudian (SignalR yang sudah ada di Final, atau penyegaran berkala).

### EPIC-RWA-04 — Asesmen Edukasi Pasien (`CAP-RWA-04`) — **Dibatalkan v`0.2`**

**Dibatalkan** 7 Oktober 2026 oleh pemilik produk (`DEC-RWA-003`): Assessment Edukasi Pasien tidak diperlukan di admisi. Asesmen edukasi tetap dikerjakan di Workspace Keperawatan tanpa perubahan. Nomor `EPIC-RWA-04`, `FR-RWA-040`, dan `FR-RWA-041` tidak dipakai ulang untuk isi lain.

| ID | Isi semula (v`0.1`) | Status |
|---|---|---|
| `FR-RWA-040` | Menu Asesmen Edukasi di Workspace Admisi memakai komponen dan data asesmen keperawatan | **Dibatalkan** — `DEC-RWA-003` |
| `FR-RWA-041` | Hak menyimpan asesmen mengikuti `PatientAssessment : Create` | **Dibatalkan** — `DEC-RWA-003` |

### EPIC-RWA-05 — Gelang dan Label Pasien (`CAP-RWA-05`)

**Tujuan:** setiap pasien rawat inap memakai gelang identitas yang benar, dan setiap cetakan tercatat.

**Disposisi:** `REUSE WITH ADAPTER` (data pasien dan penjamin); `MISSING / NEW` (log cetak dan endpoint cetak).

> **FR-RWA-050 — Gelang dewasa atau bayi**
>
> Sistem memilih **Gelang Bayi** bila umur pasien ≤ 5 tahun (batas V1, dapat diatur — `OD-RWA-16`) atau pasien ditandai bayi baru lahir; selain itu **Gelang Dewasa**. Gelang Bayi mencetak satu gelang besar dan dua label kecil seperti V1.
>
> **Contoh:** Tn. Budi lahir 12 Maret 1981 → Gelang Dewasa "BUDI SANTOSO, Tn." · "12 Mar 1981 (45 th)" · "00-12-34-56". Bayi Ny. Rina umur 3 hari → Gelang Bayi "BY. NY. RINA" · "4 Okt 2026 (3 hari)" + dua label kecil.

> **FR-RWA-051 — Label pasien tanpa data karangan**
>
> Label pasien mencetak nama / kode singkat rumah sakit (dari pengaturan), tanggal lahir, jenis kelamin / umur, No. RM, dan **nomor kartu penjamin dari data kunjungan**. Bila nomor kartu tidak ada, baris itu **kosong**; nomor acak dilarang.
>
> **Contoh:** Ny. Wati pasien tunai. Label tercetak tanpa baris nomor kartu, bukan "4829103746".

> **FR-RWA-052 — Isi QR minimal**
>
> QR gelang dan label hanya memuat pengenal (No. RM dan nomor episode, atau kode yang diterbitkan server), bukan teks data pribadi lengkap. Pemindaian di aplikasi membuka pasien yang benar (`OD-RWA-16`).

> **FR-RWA-053 — Log cetak dan cetak ulang beralasan**
>
> Setiap cetak mencatat jenis cetakan (gelang dewasa, gelang bayi, label), jumlah, petugas, dan waktu. Cetak kedua dan seterusnya wajib memilih alasan: **rusak**, **hilang**, **data berubah**, atau **lainnya** (dengan keterangan).
>
> **Contoh:** Pukul 15.10 gelang Budi basah dan sobek. Andi mencetak ulang dengan alasan "rusak". Log menunjukkan "Cetakan ke-2, rusak, oleh Andi, 15.10".

### EPIC-RWA-06 — Permintaan Privasi (`CAP-RWA-06`)

**Tujuan:** permintaan privasi pasien tercatat, bisa dibuka lagi, dan terbaca oleh petugas lain.

**Disposisi:** `MISSING / NEW`.

> **FR-RWA-060 — Isian V1**
>
> Isian V1: Kerabat yang Diperbolehkan Menjenguk (3 baris), Permintaan Khusus untuk Pelayanan (3 baris), Privasi selama Transportasi (Ya/Tidak), Kota, Tanggal, Nama Penanda Tangan, tanda tangan pasien/keluarga, tanda tangan Kepala Ruangan, Keterangan. Tombol bahasa English/Indonesia mengganti label layar seperti V1. Rincian di Lampiran A.5.

> **FR-RWA-061 — Disimpan per baris dan bisa dibuka lagi**
>
> Setiap kerabat dan permintaan khusus disimpan sebagai baris tersendiri. Membuka menu ini menampilkan dokumen aktif episode itu, bukan form kosong.
>
> **Contoh:** Kerabat "Sdr. Dimas, Jr." tersimpan utuh sebagai satu nama, tidak terpecah menjadi "Sdr. Dimas" dan "Jr.".

> **FR-RWA-062 — Ringkasan privasi terbaca**
>
> Bila ada Permintaan Privasi `Completed`, header Workspace Admisi dan IPD menampilkan "Privasi khusus: hanya 2 kerabat; privasi transportasi: Ya". Endpoint baca disediakan untuk sub-modul lain (`FR-RWA-112`).

### EPIC-RWA-07 — Data Dasar Rawat Inap / IPD (`CAP-RWA-07`)

**Tujuan:** satu lembar ringkasan identitas, penjamin, dan dokumen admisi dicetak dari data yang sudah ada.

**Disposisi:** `REUSE WITH ADAPTER` — endpoint baca baru yang merangkai data milik modul lain; tidak ada tabel data baru selain log cetak.

> **FR-RWA-070 — Tata letak V1, isi dari sumber resmi**
>
> Dokumen IPD memakai tata letak V1 "DATA DASAR RAWAT INAP/ODC" dua kolom (Lampiran A.6). Setiap isian diambil dari sumbernya menurut tabel pemetaan Lampiran A.6. Isian tanpa sumber dicetak sebagai **garis kosong** untuk diisi tangan (`OD-RWA-15`).

> **FR-RWA-071 — IPD merangkum dokumen admisi**
>
> Isian "Nilai Kepercayaan", "Permintaan Privat", dan "Orang yang diberi kewenangan untuk mendapatkan informasi" diisi dari dokumen admisi yang `Completed` (V1 selalu kosong).
>
> **Contoh:** IPD Budi mencetak "Nilai Kepercayaan: Tidak menerima transfusi darah" dan "Penerima informasi: Ny. Rina Santoso (istri)".

> **FR-RWA-072 — Cetak ditahan bila data wajib gagal terbaca**
>
> Bila identitas pasien atau data episode gagal dimuat, tombol Cetak dinonaktifkan dengan pesan dan tombol **Coba Lagi**, mengikuti pola halaman Cetak Persetujuan yang sudah ada.

### EPIC-RWA-08 — Pelunasan Deposit (`CAP-RWA-08`) — **`OPEN DECISION`**

**Tujuan:** kesediaan melunasi kekurangan deposit tersimpan sebagai surat bertanda tangan, dengan angka dari Billing.

**Disposisi:** `REUSE WITH ADAPTER` (ringkasan deposit Billing); `MISSING / NEW` (surat). **`OPEN DECISION`** sampai Billing mengonfirmasi dasar angka dan batas jatuh tempo (`OD-RWA-14`).

> **FR-RWA-080 — Kekurangan deposit dari Billing**
>
> Angka "Kekurangan pembayaran deposit" diambil dari `PolicyShortfallAmount` ringkasan deposit episode milik Billing, dengan teks perhitungan "minimum deposit − deposit diterima". Angka tetap Rp 100.000.000 milik V1 tidak dipakai.
>
> **Contoh:** Kebijakan deposit asuransi kelas 2 minimum Rp 5.000.000. Deposit masuk Rp 2.000.000. Surat mencetak "Rp 3.000.000" dengan perhitungan "Rp 5.000.000 − Rp 2.000.000 = Rp 3.000.000".

> **FR-RWA-081 — Hanya bila ada kekurangan**
>
> Form hanya dapat disimpan bila `PolicyShortfallAmount` lebih dari 0. Bila 0, layar menulis "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan."

> **FR-RWA-082 — Data wali dari sumber yang dipilih**
>
> Bagian Data Wali punya pilihan Sumber Data: daftar kontak darurat dan relasi pasien (penanggung jawab, wali sah), atau isi manual — menggantikan pilihan tetap "Kontak Darurat / Wali 2 / Wali 3" V1. Tombol **Ambil Data Wali** dan **Reset** dipertahankan. Telepon hanya angka, maksimal 13 digit.
>
> **Contoh:** Petugas mengetik "0812-3456-7890". Sistem membuang tanda hubung dan menyimpan "081234567890" (12 digit). Bila petugas mengetik 14 digit "08123456789012", sistem menolak dengan pesan "Nomor telepon maksimal 13 digit".

> **FR-RWA-083 — Tanggal jatuh tempo**
>
> Tanggal jatuh tempo terisi bawaan **hari kerja berikutnya** setelah tanggal surat, pukul 11.00 WIB (teks V1), boleh diubah tetapi tidak boleh sebelum tanggal surat dan tidak boleh melewati tanggal surat + `FollowUpIntervalDays` kebijakan deposit (usulan `DEC-RWA-014`).
>
> **Contoh:** Surat bertanggal Jumat 9 Oktober 2026, `FollowUpIntervalDays` = 3. Bawaan jatuh tempo Senin 12 Oktober 2026 pukul 11.00 WIB. Tanggal 13 Oktober ditolak: "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit".

> **FR-RWA-084 — Angka dibekukan saat ditandatangani**
>
> Angka kekurangan, minimum, dan deposit diterima disimpan apa adanya saat surat `Completed`, berikut waktu pengambilan dari Billing. Cetak ulang mencetak angka yang sama walaupun deposit kemudian bertambah.

> **FR-RWA-085 — Jatuh tempo terlewati**
>
> Bila tanggal jatuh tempo lewat dan Billing masih mencatat kekurangan, Workspace Admisi dan Detail Episode menampilkan "Pelunasan deposit jatuh tempo 12 Okt 2026 11.00 terlewati — kurang Rp 3.000.000". Tindakan menurunkan kelas kamar tetap keputusan manual lewat transfer yang sudah ada.

### EPIC-RWA-09 — Estimasi Biaya: Penjelasan Prakiraan Biaya (`CAP-RWA-09`) — **`OPEN DECISION`**

**Tujuan:** pasien menerima penjelasan prakiraan biaya yang angkanya berasal dari tarif resmi, dan penjelasan itu tersimpan bertanda tangan.

**Disposisi:** `MISSING / NEW` (dokumen); `REUSE WITH ADAPTER` (tarif dan aturan tanggungan). **`OPEN DECISION`** — rumus, sumber tarif, dan catatan aturan (cito, durasi, *standby*, biaya admin, anestesi) adalah aturan Billing (`OD-RWA-07`).

> **FR-RWA-090 — Isian dan cetakan V1**
>
> Header: nama pasien, nomor MR, jenis tindakan, jadwal tindakan, dokter, ruang rawat. Baris biaya V1: prakiraan biaya tindakan operasi, tindakan rawat inap, harga alat/alkes khusus, prakiraan lama rawat (biaya kamar per hari × hari; biaya visit dokter per dokter per hari × hari), dan biaya lain-lain. Catatan V1 dan dua kolom tanda tangan (pasien/keluarga; petugas PPRI/Admission). Rincian di Lampiran A.8.

> **FR-RWA-091 — Angka dari tarif, bukan dari tebakan nama**
>
> Tarif tindakan, kamar per hari menurut kelas, dan visit dokter diambil dari master tarif untuk penjamin pasien lewat layanan perkiraan harga Billing yang sudah ada. Pengelompokan dengan mencocokkan potongan kata (kelemahan V1 nomor 16) dilarang. Baris "alat/alkes khusus" dan "lain-lain" boleh diisi manual dengan uraian.
>
> **Contoh:** Rencana appendektomi tarif Rp 8.500.000; lama rawat 3 hari × kamar kelas 2 Rp 750.000 = Rp 2.250.000; visit DPJP 3 hari × Rp 250.000 = Rp 750.000; lain-lain Rp 500.000 ("perlengkapan pasca operasi"). Total prakiraan Rp 12.000.000.

> **FR-RWA-092 — Tarif tidak ditemukan**
>
> Bila tarif sebuah baris tidak ditemukan untuk penjamin itu, baris tersebut ditandai "Tarif belum tersedia" dan dokumen tidak dapat dikunci sampai baris itu diisi manual dengan alasan atau dihapus.

> **FR-RWA-093 — Catatan dari pengaturan**
>
> Catatan aturan (misalnya "biaya admin 7% dari jumlah tagihan, maksimal Rp 6.000.000") dibaca dari pengaturan yang sama dengan yang dipakai Billing, bukan ditanam di layar.

### EPIC-RWA-10 — Selisih Biaya (`CAP-RWA-11`)

**Tujuan:** kesediaan pasien atau keluarga menanggung biaya yang tidak dijamin pihak ketiga tersimpan sebagai surat bertanda tangan.

**Disposisi:** `MISSING / NEW`.

> **FR-RWA-100 — Hanya untuk penjamin asuransi/perusahaan**
>
> Form tersedia bila penjamin utama kunjungan berjenis asuransi atau perusahaan (judul V1: "khusus pasien yang menggunakan jaminan Asuransi/Perusahaan"). Untuk pasien tunai, menu berlencana "Tidak diperlukan".

> **FR-RWA-101 — Data pasien dari master, tidak bisa diketik**
>
> Nama, alamat, No. RM, dan kelas pasien tampil **hanya-baca** dari master dan episode (memperbaiki kelemahan V1 nomor 7).

> **FR-RWA-102 — Isian deklarer V1**
>
> Subjek pernyataan (diri saya sendiri, istri, suami, anak, saudara kandung lainnya + keterangan); Nama, Alamat, Pekerjaan, Tipe ID (KTP, SIM, PASPOR, ID CARD), No. ID, No. Handphone (maksimal 13 digit), Telepon Kantor, Kota, Tanggal, Nama Penanda Tangan, tanda tangan deklarer, petugas PPRI, Keterangan. Subjek "diri saya sendiri" mengisi deklarer dari master pasien. Rincian di Lampiran A.9.
>
> **Contoh:** Subjek "istri saya" dipilih oleh Ny. Rina → deklarer Rina, ID KTP `3275•••••••••001` (disamarkan di layar daftar), HP `081234567890`.

> **FR-RWA-103 — Hubungan disimpan sebagai kode**
>
> Subjek pernyataan disimpan sebagai kode (misalnya `SPOUSE`), bukan teks tampilan seperti V1 (`"istri saya / my wife"`), sehingga bisa dilaporkan dan diterjemahkan.

### EPIC-RWA-11 — Nilai dan Kepercayaan Pasien (`CAP-RWA-13`)

**Tujuan:** nilai dan kepercayaan pasien yang bertentangan dengan pelayanan tercatat dan terbaca petugas.

**Disposisi:** `MISSING / NEW`.

> **FR-RWA-110 — Isian V1**
>
> Penanda tangan: Nama Lengkap, Tanggal Lahir, Umur (dihitung otomatis dari tanggal lahir), Jenis Kelamin, Hubungan dengan Pasien, Alamat. Data pasien hanya-baca (nama, No. RM, tanggal lahir, umur, jenis kelamin) dan agama dari master pasien. Hal-hal yang bertentangan: minimal 1, maksimal 5 butir. Tanda tangan penanda tangan. Rincian di Lampiran A.11.
>
> **Contoh:** Butir 1 "Tidak menerima transfusi darah", butir 2 "Hanya menerima obat berlabel halal". Menambah butir ke-6 ditolak: "Maksimal 5 butir".

> **FR-RWA-111 — Per episode, terisi dari dokumen sebelumnya**
>
> Dokumen disimpan per episode (V1: per pasien). Saat episode baru, form terisi dari dokumen `Completed` terakhir pasien itu dan tetap wajib dikonfirmasi serta ditandatangani ulang (`OD-RWA-10`, usulan `DEC-RWA-010`).
>
> **Contoh:** Budi dirawat lagi Januari 2027. Form terisi dua butir dari Oktober 2026; petugas menanyakan ulang, lalu Budi menandatangani.

> **FR-RWA-112 — Endpoint baca ringkasan hak pasien**
>
> Disediakan satu endpoint baca ringkas "nilai kepercayaan dan privasi aktif" per episode, supaya Workspace Keperawatan, Workspace Dokter, dan IPD dapat menampilkannya tanpa membaca tabel dokumen admisi.

> **FR-RWA-113 — Identitas pasien tidak berupa gambar**
>
> Blok identitas pasien dirender dari data, bukan gambar tangkapan layar yang diunggah (kelemahan V1 nomor 18).

### EPIC-RWA-12 — Fondasi dokumen admisi: siklus, versi, tanda tangan kertas, atestasi, cetak (`CAP-RWA-14` sebagian, `CAP-RWA-15`, `CAP-RWA-16`, `CAP-RWA-17`)

**Tujuan:** semua dokumen admisi berperilaku sama: tersimpan, terkunci, berversi, tidak terhapus, dan tercetak dengan identitas rumah sakit yang benar.

**Disposisi:** `MISSING / NEW` (siklus dan log); `EXTEND` (pengaturan Rawat Inap untuk kode formulir; mesin keutuhan dokumen Rekam Medis, `OD-RWA-06`); `REUSE WITH ADAPTER` (profil rumah sakit).

> **FR-RWA-120 — Siklus status seragam**
>
> Setiap dokumen admisi (kecuali IPD, Gelang, dan Label yang hanya punya log cetak) mengikuti status `Draft` → `AwaitingSignature` → `Completed`, ditambah `Superseded` dan `Cancelled` (bagian 11).

> **FR-RWA-121 — Tidak ada hapus permanen**
>
> Tidak ada endpoint hapus. Konsep yang tidak jadi dibuang dengan status `Cancelled`; dokumen yang sudah ditandatangani hanya dapat dibatalkan supervisor dengan alasan minimal 10 karakter.
>
> **Contoh:** Alasan "salah" (5 karakter) ditolak. Alasan "salah pasien, dokumen dibuat untuk RM 00-12-34-57" diterima.

> **FR-RWA-122 — Mode tanda tangan kertas**
>
> Selama mode digital belum diputuskan (`OD-RWA-02`), slot tanda tangan pasien/keluarga diisi dengan **"Lembar kertas sudah ditandatangani"**: nama penanda tangan, hubungan, waktu, dan petugas yang memverifikasi. Cetakan sebelum slot terisi diberi tanda "KONSEP — BELUM DITANDATANGANI".
>
> **Contoh:** Ny. Rina menandatangani Surat Selisih Biaya di kertas pukul 10.15. Sari mencatat "ditandatangani di kertas oleh Rina Santoso (istri), 10.15, diverifikasi Sari". Lembar kertas disimpan di berkas rekam medis.

> **FR-RWA-123 — Atestasi petugas**
>
> Slot tanda tangan petugas (Admission, CRO, Perawat, Kepala Ruangan, Petugas PPRI) diisi oleh akun yang **sedang menandatangani**, dan cetakan menampilkan "Ditandatangani secara elektronik oleh *nama lengkap*, *jabatan*, *tanggal jam*". Nama petugas di cetakan selalu nama penanda tangan, bukan pengguna yang mencetak (memperbaiki kelemahan V1 nomor 5). Bila master tanda tangan petugas kelak tersedia (`OD-RWA-04`), gambarnya ditambahkan tanpa mengubah aturan ini.

> **FR-RWA-124 — Identitas dibekukan saat ditandatangani**
>
> Identitas pasien, penjamin, kelas, kamar, dan angka rupiah di dalam dokumen disimpan sebagai salinan saat dokumen `Completed`. Bila master pasien dikoreksi kemudian, dokumen lama tetap mencetak data saat ditandatangani dan layar menampilkan catatan "Data pasien telah diperbarui sejak dokumen ini ditandatangani".

> **FR-RWA-125 — Koreksi lewat versi baru**
>
> Dokumen `Completed` tidak dapat diubah. Tombol **Buat Versi Koreksi** menyalin isi ke `Draft` baru dengan alasan wajib; versi lama berstatus `Superseded` dan tetap terbaca di tab Riwayat.
>
> **Contoh:** Alamat penanda tangan General Consent salah ketik. Sari membuat versi 2 dengan alasan "koreksi alamat penanda tangan". Riwayat menampilkan versi 1 (`Superseded`) dan versi 2 (`Completed` setelah ditandatangani ulang).

> **FR-RWA-126 — Kop surat dan kode formulir dari pengaturan**
>
> Kop surat (nama, alamat, telepon, email, logo) dibaca dari profil rumah sakit; kode formulir terkendali (misalnya `GC/ADM/001/Rev01/2024`, `005/NM/E/Rev01/XI/2016`) dan kota penandatanganan dibaca dari pengaturan, bukan ditanam di kode (`OD-RWA-18`). Nilai awal pengaturan = nilai V1 (Lampiran A.12).

> **FR-RWA-127 — Cetak dwibahasa dan log cetak**
>
> Cetakan memakai teks dwibahasa V1 (Indonesia/Inggris). Setiap cetak dan cetak ulang tercatat; cetakan ulang diberi tanda "Cetak ulang ke-*n*".

> **FR-RWA-128 — Penulisan atomik, aman dari klik ganda, dan aman dari tabrakan**
>
> Dokumen beserta butir dan tanda tangannya disimpan dalam satu transaksi. Permintaan simpan, tanda tangan, dan cetak membawa kunci idempoten; klik ganda menghasilkan satu rekaman. Perubahan membawa nomor versi terakhir; penyimpanan berdasar versi basi ditolak 409.
>
> **Contoh:** Sari menekan Simpan dua kali dalam 1 detik. Hanya satu Serah Terima tercatat, dan klik kedua mendapat hasil yang sama.

### EPIC-RWA-13 — Tanda tangan digital pasien/keluarga di layar (`CAP-RWA-14` sebagian) — **`OPEN DECISION`**

**Tujuan:** paritas V1 — penanda tangan menandatangani langsung di tablet, dan gambar tanda tangannya menjadi bukti.

**Disposisi:** **`OPEN DECISION`** (`OD-RWA-02`). Tidak masuk gelombang sampai pemilik privasi/hukum memutuskan.

> **FR-RWA-130 — Gambar tanda tangan sebagai bukti**
>
> Gambar tanda tangan (PNG/JPEG, maksimal 2 MB) disimpan sebagai berkas dengan hash SHA-256, waktu, perangkat, dan alamat IP, mengikuti pola kolom bukti yang sudah ada (`TrxPatientConsent` dan `MrcClinicalDocumentIntegrity`).

> **FR-RWA-131 — Mode dipilih per rumah sakit**
>
> Mode tanda tangan (kertas atau digital) adalah pengaturan, sehingga rumah sakit dapat berpindah tanpa mengubah program. Dokumen yang sudah ditandatangani tetap pada mode saat ditandatangani.

---

## 11. Model status yang diusulkan

### 11.1 Status dokumen admisi

Berlaku untuk General Consent (dipetakan ke status persetujuan yang sudah ada, lihat 11.3), Serah Terima, Permintaan Privasi, Pelunasan Deposit, Estimasi Biaya (Rekap), Selisih Biaya, dan Nilai Kepercayaan.

| Status | Arti | Isi dapat diubah? | Dapat dicetak final? |
|---|---|:---:|:---:|
| `Draft` (Konsep) | Disimpan, belum dikunci | Ya | Tidak — bertanda "KONSEP" |
| `AwaitingSignature` (Menunggu tanda tangan) | Isi dikunci, menunggu slot tanda tangan wajib | Tidak | Tidak — bertanda "BELUM DITANDATANGANI" |
| `Completed` (Lengkap) | Seluruh slot wajib terisi | Tidak | Ya |
| `Superseded` (Digantikan) | Versi lama setelah koreksi | Tidak | Ya, bertanda "DIGANTIKAN VERSI *n*" |
| `Cancelled` (Dibatalkan) | Dibatalkan beralasan | Tidak | Ya, bertanda "DIBATALKAN" |

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| — | Simpan | `Draft` | `Create` | Episode `Admitted`/`DischargePending`; tidak ada dokumen aktif sejenis |
| `Draft` | Ubah | `Draft` | `Update` | Nomor versi cocok |
| `Draft` | Kunci | `AwaitingSignature` | `Update` | Isian wajib lengkap; aturan per jenis terpenuhi |
| `AwaitingSignature` | Tanda tangan slot | `AwaitingSignature` | `Sign` / `SignAsCro` / `SignAsNurse` / `SignAsHeadNurse` sesuai slot | Satu akun satu slot |
| `AwaitingSignature` | Tanda tangan slot terakhir | `Completed` | idem | Semua slot wajib terisi |
| `AwaitingSignature` | Buka kunci (belum ada tanda tangan) | `Draft` | `Update` | Belum ada slot terisi |
| `Completed` | Buat versi koreksi | `Superseded` + versi baru `Draft` | `Update` | Alasan wajib |
| `Draft` | Buang konsep | `Cancelled` | Pembuat (`Update`) atau `Cancel` | Alasan wajib |
| `AwaitingSignature`/`Completed` | Batalkan | `Cancelled` | `Cancel` | Alasan ≥ 10 karakter |
| Apa pun | Episode `Closed`/`Cancelled` | Tidak berubah, menjadi hanya-baca | — | — |

Transisi terlarang yang wajib ditolak: `Completed` → `Draft` langsung; `Cancelled` → status apa pun; tanda tangan pada `Draft`; ubah pada `AwaitingSignature` yang sudah punya tanda tangan.

### 11.2 Invariant utama

| ID | Invariant | Contoh pelanggaran yang ditolak |
|---|---|---|
| `INV-RWA-01` | Paling banyak satu dokumen aktif (`Draft`, `AwaitingSignature`, atau `Completed`) per jenis per episode | Membuat Permintaan Privasi kedua saat yang pertama masih `Completed` → 409; gunakan versi koreksi |
| `INV-RWA-02` | Dokumen `Completed` tidak berubah | Mengubah keterangan Selisih Biaya `Completed` → 409 |
| `INV-RWA-03` | Tidak ada hapus permanen | Tidak ada endpoint `DELETE` |
| `INV-RWA-04` | Satu akun tidak mengisi dua slot petugas pada dokumen yang sama | Sari menandatangani Admission dan CRO → 422 |
| `INV-RWA-05` | Identitas pasien di dokumen berasal dari master, dibekukan saat `Completed` | Mengirim nama pasien lewat API diabaikan; server mengisi dari master |
| `INV-RWA-06` | Angka rupiah berasal dari Billing/master, dibekukan dengan waktu sumbernya | Mengirim angka kekurangan deposit dari layar ditolak |
| `INV-RWA-07` | Tidak ada data karangan | Nomor kartu kosong tetap kosong; benefit tanpa sumber tidak dicetak |
| `INV-RWA-08` | Penulisan hanya pada episode `Admitted`/`DischargePending` | Menyimpan pada episode `Closed` → 409 |
| `INV-RWA-09` | Cetak ulang selalu beralasan dan tercatat | Cetak ulang gelang tanpa alasan → 422 |

### 11.3 Pemetaan ke status persetujuan yang sudah ada

General Consent memakai tabel persetujuan Clinical Management yang sudah punya statusnya sendiri (`PatientConsentStatus`). Pemetaannya (usulan, final di `design-business-module`):

| Status dokumen admisi | `PatientConsentStatus` yang sudah ada |
|---|---|
| `Draft` | `Draft` |
| `AwaitingSignature` | `PendingSignature` |
| `Completed` | `Signed` |
| `Superseded` | `Cancelled` + rujukan ke versi pengganti (usulan) |
| `Cancelled` | `Cancelled` atau `EnteredInError` |

Status gelang, label, IPD, dan MP Benefit bukan status dokumen, melainkan **log cetak**: cetakan ke-1, ke-2, dan seterusnya beserta alasan.

---

## 12. Sasaran arsitektur

### 12.1 Frontend — template Workspace Keperawatan

| Bagian | Dipakai ulang | Diperluas | Baru |
|---|---|---|---|
| Kerangka halaman | `ClinicalPageHeader`, `ClinicalStateBoundary`, `ClinicalWorkspaceShell`, `ClinicalSectionNav` (`src/components/ui/clinical-workspace`) | — | `AdmissionWorkspaceView` meniru `nursing-workspace-view.jsx:73-401` |
| Header pasien | `PatientContextHeader` atau pola `NursingEpisodeHeader` | Tambah penjamin, kontak darurat, status deposit | — |
| Kelengkapan | `ClinicalCompletionBar`, `ClinicalStatusBadge`, `ClinicalQuickSummary` | — | Aturan kelengkapan (`FR-RWA-005`) dari backend |
| Riwayat versi | `ClinicalRevisionHistory`, `ClinicalDocumentMeta` | — | — |
| Validasi | `ClinicalValidationSummary` | — | — |
| Tab kedua | Pola `NursingSecondaryTabBar` | — | — |
| Cetak | `KopSurat`, `A4Document`, `signature-section`, `informasi-pasien-surat` (`src/components/features/surat-component`) | `KopSurat` membaca profil rumah sakit, bukan nilai bawaan | Sepuluh lembar cetak mengikuti V1 |
| Jalan masuk | Tombol di `inpatient-episode-detail-view.jsx` | Satu tombol baru sesudah Workspace Dokter | Builder route `buildInpatientAdmissionWorkspaceRoute` dan halaman `episodes/[id]/admission` (nama route `DEV_DISCRETION`) |

**Peta butir menu.** Workspace Admisi **tidak** mendapat butir menu sidebar, karena kuota `IA-INP-05` sudah habis (`02-module-map.md:546`) dan pekerjaannya berputar pada satu pasien. Ia menjadi **layar anak** Detail Episode `FE-INP-04`, sama seperti Workspace Keperawatan dan Workspace Dokter. ID layar diberikan `design-business-module`.

**Skema tampilan** (isi dan sumber data dikunci; warna, jarak, ikon tetap `DEV_DISCRETION`):

```text
Detail Episode — kelompok aksi utama (FR-RWA-001)
[Workspace Keperawatan] [Workspace Dokter] [Workspace Admisi] [Penutupan Episode]
[Keputusan Pulang dan Resume] [Riwayat Kelayakan Keuangan]   [Cetak Persetujuan]   [Batalkan Admisi]

Workspace Admisi
┌──────────────────────────────────────────────────────────────────────────────────────┐
│ Ruang Kerja Admisi Rawat Inap          [Ringkasan Kelengkapan] [Kembali ke Detail     │
│ Penerimaan Pasien Rawat Inap (PPRI)                                Episode] [Segarkan]│
├──────────────────────────────────────────────────────────────────────────────────────┤
│ Tn. Budi Santoso · RM 00-12-34-56 · L · 45 th      │ RI-261007-0001 · Masuk 07 Okt 08.15│
│ Kelas 2 · Melati 03 / Bed B · DPJP dr. Andika     │ Asuransi PT Asuransi Sehat Sentosa │
│ Kontak darurat Ny. Rina Santoso (istri)           │ Kartu 7788-0012-3456 · Alergi: -   │
│ Deposit: kurang Rp 3.000.000                      │ Dokumen admisi 5 dari 7 lengkap ▓▓▓░│
├──────────────────────────┬───────────────────────────────────────────────────────────┤
│ DOKUMEN ADMISI           │ [Formulir] [Riwayat Versi]                                 │
│ ✓ General Consent        │ Ceklist Serah Terima Pasien Baru        Menunggu tanda tangan│
│ ⏳ Serah Terima Pasien    │ No │ Uraian                     │ Sudah │ Belum │ Keterangan│
│ ✓ Gelang & Label         │  1 │ Surat pengantar rawat      │  ●    │       │           │
│ – Permintaan Privasi     │  … │ …                          │       │       │           │
│ ✓ IPD                    │ 13 │ Pasang gelang  (saran)     │  ●    │       │ 09.50     │
│ ✓ Pelunasan Deposit      │ Catatan: …                                                 │
│ ○ Estimasi Biaya         │ Admission: Sari ✓ 10.05 │ CRO: [Tandatangani] │ Perawat: – │
│ ✓ Selisih Biaya          │                                                            │
│ ● Nilai Kepercayaan      │ [Simpan Konsep] [Kunci & Minta Tanda Tangan] [Cetak]       │
└──────────────────────────┴───────────────────────────────────────────────────────────┘
Lencana: ✓ Lengkap · ⏳ Menunggu tanda tangan · ● Konsep · ○ Belum dibuat · – Tidak diperlukan
```

| Wilayah | Isi | Sumber data | Hak akses penjaga | Bunyi bila kosong | Bunyi bila gagal |
|---|---|---|---|---|---|
| Tombol di Detail Episode | "Workspace Admisi" | — | `InpatientAdmissionDocument : Read` | — | — |
| Header pasien | Identitas, episode, penjamin, kontak, alergi | Episode, pasien, penjamin kunjungan, alergi (sudah ada) | `InpatientEpisode : Read` | — | "DATA PASIEN TIDAK DAPAT DIMUAT" + Coba Muat Ulang |
| Status deposit | Kurang/Cukup/Tidak wajib | Ringkasan deposit episode Billing | `InpatientAdmissionDocument : ReadAmount` + `BillingDeposit : Read` | "Tidak wajib" | "Status deposit tidak dapat dimuat" (form lain tetap jalan) |
| Kelengkapan | *x* dari *y* + daftar | Ringkasan kelengkapan (`Rencana`) | `InpatientAdmissionDocument : Read` | "Belum ada dokumen" | "Kelengkapan tidak dapat dihitung" |
| Navigasi kiri | 9 menu di MVP (10 menu; MP Benefit tersembunyi sampai dikirim; Assessment Edukasi tidak ada) + lencana | Ringkasan kelengkapan | `InpatientAdmissionDocument : Read` | — | Lencana "?" |
| Isi dokumen | Form V1 per menu | Dokumen admisi (`Rencana`) | Per tombol: `Create`, `Update`, `Sign*`, `Print`, `Cancel` | "Belum ada *nama dokumen* untuk episode ini" + tombol Buat | Pesan server + Coba Lagi |

### 12.2 Backend — dipakai ulang, diperluas, baru

| Kebutuhan | Dipakai ulang (`EXISTING / REUSE`) | Diperluas (`EXTEND`) | Baru (`MISSING / NEW`, usulan) |
|---|---|---|---|
| Konteks episode | Endpoint episode, penempatan, alergi | — | — |
| Data pasien, wali, kontak darurat, agama | `MstPatient`, `MstPatientRelationship`, `MstPatientEmergencyContact` | Bila `OD-RWA-15` memilih memperluas master pasien (milik Patient Management) | — |
| Penjamin kunjungan | `RegPatientEncounterGuarantor` (snapshot kartu, peserta, polis, kelas) | — | — |
| Persetujuan umum | `TrxPatientConsent` + endpoint `patient-consents` (`/sign`, `/withdraw`, `/cancel`) | Isian V1 khusus admisi (tipe kamar, kamar, panduan, keterangan): **opsi A** kolom baru di tabel Clinical Management; **opsi B** tabel pendamping milik Rawat Inap (direkomendasikan, karena tidak mengubah tabel modul lain) | — |
| Deposit | `GET …/patient-funds/deposits/episodes/{episodeId}` | — | — |
| Butir checklist serah terima | Master `MstInpatientClearanceItem` + endpoint `inpatient-clearance-items` | Kolom jenis checklist (penutupan / serah terima pasien baru) dan induk sub-butir; saringan jenis di layar master `FE-INP-13` | Data awal 15 butir + 3 sub-butir |
| Keutuhan dan penguncian dokumen | `MrcClinicalDocumentIntegrity` (penanda tangan, waktu, perangkat, IP, penguncian) | Jenis dokumen baru pada `ClinicalDocumentKind` **dan** pada himpunan jenis yang ditegakkan layanan keutuhan (`OD-RWA-06`) | — |
| Dokumen admisi | — | — | Kelompok data "dokumen admisi": kepala dokumen (jenis, status, versi, pengganti, alasan), slot tanda tangan, butir serah terima, kerabat/permintaan privasi, butir nilai kepercayaan, isian surat deposit, isian selisih biaya, baris estimasi biaya. Nama tabel final di `design-business-module` |
| Log cetak | — | — | Log cetak gelang, label, IPD, dan dokumen |
| Kop surat | `MstHospitalSite` | Logo dan kode singkat RS bila belum ada | — |
| Kode formulir dan kota | `MstInpatientSetting` | Kolom kode formulir per jenis dokumen dan kota penandatanganan (atau tabel master kecil) | — |
| Tanda tangan petugas | Identitas akun dan jabatan dari profil pengguna | — | Master gambar tanda tangan petugas milik Platform, **bila** `OD-RWA-04` memilihnya |
| Tarif estimasi | Layanan perkiraan harga (`coverage-status`) dan master tarif | Bila `OD-RWA-07` memutuskan endpoint estimasi milik Billing | — |
| Benefit makanan pendamping | `MstInsuranceCoverageRule` (calon sumber) | Bila `OD-RWA-09` memutuskannya | — |
| Pembaruan *real time* | SignalR yang sudah terpasang di Final | Satu kanal untuk dokumen admisi | — |

### 12.3 Kepemilikan data (usulan untuk `design-business-module`)

| Kelompok data | Modul pemilik | Dipakai Workspace Admisi | Dibuat ulang? |
|---|---|---|---|
| Episode, penempatan bed, DPJP | Rawat Inap (`episode-rawat-inap`) | Baca | Tidak |
| Pasien, relasi, kontak darurat, agama | Patient Management | Baca | **Tidak boleh** |
| Penjamin kunjungan | Registration Management | Baca | **Tidak boleh** |
| Persetujuan pasien | Clinical Management (`TrxPatientConsent`) | Tulis lewat endpoint pemilik | **Tidak boleh** membuat tabel persetujuan kedua |
| Deposit dan kebijakan deposit | Billing | Baca ringkasan | **Tidak boleh** menghitung sendiri |
| Tarif dan aturan tanggungan | Master Data / Billing | Baca | Tidak |
| Butir checklist | Master Data (`MstInpatientClearanceItem`) | Baca | Tidak — diperluas |
| Dokumen admisi Rawat Inap | **Rawat Inap** (usulan; alternatif Clinical Management — `OD-RWA-06`) | Tulis | Ya, baru |
| Log cetak | **Rawat Inap** (usulan) | Tulis | Ya, baru |
| Keutuhan dokumen | Medical Record Management | Tulis lewat layanan pemilik | Tidak — diperluas |
| Profil rumah sakit | HR Master Data (`MstHospitalSite`) | Baca | Tidak |
| Tanda tangan petugas | Platform/Authorization (usulan) | Baca | Baru, di luar Rawat Inap (`OD-RWA-04`) |

---

## 13. Sasaran kemampuan API

Endpoint yang belum ada di kode berlabel **Rencana (belum tersedia)**. Judul grup memakai nilai `[Tags(...)]` apa adanya untuk endpoint yang sudah ada, dan usulan nilai `[Tags(...)]` untuk grup baru. Daftar ini wajib disalin ke `contracts/api-contract.md` oleh `design-business-module`; sebelum itu, tabel ini **belum** menjadi kontrak.

### 13.1 Health Services / Inpatient Management / Inpatient Admission Workspace — **Rencana (belum tersedia)**

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/summary` | Header admisi, kelengkapan, dan lencana per menu | `InpatientAdmissionDocument : Read` | - | `AdmissionWorkspaceSummaryResponse` | `EPIC-RWA-01` | **Rencana (belum tersedia)** |
| `GET` | `/summary/amounts` | Status deposit berupiah untuk header | `InpatientAdmissionDocument : ReadAmount` | - | `AdmissionAmountSummaryResponse` | `EPIC-RWA-01` | **Rencana (belum tersedia)** |
| `GET` | `/prefill/{documentType}` | Isian bawaan: pasien, calon penanda tangan dari relasi/kontak darurat, penjamin, kamar | `InpatientAdmissionDocument : Read` | `documentType` | `AdmissionDocumentPrefillResponse` | `EPIC-RWA-03`, `06`, `10`, `11` | **Rencana (belum tersedia)** |
| `GET` | `/documents` | Daftar dokumen per jenis, termasuk versi | `InpatientAdmissionDocument : Read` | query `type`, `includeHistory` | `List<AdmissionDocumentSummaryResponse>` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}` | Satu dokumen lengkap | `InpatientAdmissionDocument : Read` | - | `AdmissionDocumentResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `POST` | `/documents` | Simpan konsep baru | `InpatientAdmissionDocument : Create` | Header `Idempotency-Key`; `CreateAdmissionDocumentRequest` | `AdmissionDocumentResponse` | `EPIC-RWA-03`, `06`, `08`, `09`, `10`, `11` | **Rencana (belum tersedia)** |
| `PUT` | `/documents/{documentId}` | Ubah konsep | `InpatientAdmissionDocument : Update` | `UpdateAdmissionDocumentRequest` (+ `ExpectedVersion`) | `AdmissionDocumentResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/lock` | Kunci dan minta tanda tangan | `InpatientAdmissionDocument : Update` | `ExpectedVersion` | `AdmissionDocumentResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures` | Isi slot tanda tangan pasien/keluarga (mode kertas; mode digital bila `OD-RWA-02` disetujui) | `InpatientAdmissionDocument : Sign` | Header `Idempotency-Key`; `RecordSignerSignatureRequest` | `AdmissionDocumentResponse` | `EPIC-RWA-12`, `13` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/staff-signatures/{slot}` | Atestasi petugas pada slot `ADMISSION`, `CRO`, `NURSE`, `HEAD_NURSE`, `PPRI` | `InpatientAdmissionDocument : Sign` / `SignAsCro` / `SignAsNurse` / `SignAsHeadNurse` sesuai slot | Header `Idempotency-Key` | `AdmissionDocumentResponse` | `EPIC-RWA-03`, `12` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/revisions` | Buat versi koreksi | `InpatientAdmissionDocument : Update` | `ReviseAdmissionDocumentRequest` (alasan) | `AdmissionDocumentResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/cancel` | Batalkan beralasan | `InpatientAdmissionDocument : Cancel` | `CancelAdmissionDocumentRequest` | `AdmissionDocumentResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}/print` | Data cetak beku (snapshot) + kop + kode formulir | `InpatientAdmissionDocument : Print` | - | `AdmissionDocumentPrintResponse` | `EPIC-RWA-12` | **Rencana (belum tersedia)** |
| `POST` | `/print-logs` | Catat cetak/cetak ulang (gelang, label, IPD, dokumen) | `InpatientAdmissionDocument : Print` | Header `Idempotency-Key`; `RecordPrintRequest` | `PrintLogResponse` | `EPIC-RWA-05`, `07`, `12` | **Rencana (belum tersedia)** |
| `GET` | `/identity-labels` | Data gelang dan label (jenis, sapaan, QR) | `InpatientAdmissionDocument : Print` | - | `IdentityLabelResponse` | `EPIC-RWA-05` | **Rencana (belum tersedia)** |
| `GET` | `/ipd` | Data Dasar Rawat Inap terangkai | `InpatientAdmissionDocument : Read` | - | `InpatientBaseDataResponse` | `EPIC-RWA-07` | **Rencana (belum tersedia)** |
| `GET` | `/patient-rights` | Ringkasan nilai kepercayaan dan privasi aktif | `InpatientEpisode : Read` | - | `PatientRightsSummaryResponse` | `EPIC-RWA-06`, `11` | **Rencana (belum tersedia)** |

Kode status:

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Berhasil, termasuk pengulangan permintaan dengan kunci idempoten yang sama |
| `400` | Isian tidak lengkap atau formatnya salah, misalnya telepon lebih dari 13 digit |
| `403` | Pengguna tidak punya hak untuk tindakan ini, misalnya perawat mencoba membatalkan dokumen |
| `404` | Episode atau dokumen tidak ditemukan |
| `409` | Bertabrakan dengan keadaan data: dokumen sudah diubah petugas lain, sudah ada dokumen aktif sejenis, dokumen sudah `Completed`, atau episode sudah ditutup |
| `422` | Melanggar aturan bisnis: satu petugas dua slot, butir "Belum" tanpa keterangan, jatuh tempo melewati batas, tidak ada kekurangan deposit, pasien bukan penjamin asuransi/perusahaan |

### 13.2 Health Services / Clinical Management / Patient Consent — sudah ada, dipakai ulang

Base URL: `api/v1/health-services/clinical-management/patient-consents`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/` | Membuat persetujuan jenis `Admission` | `PatientConsent : Create` | DTO persetujuan yang sudah ada | DTO persetujuan | `EPIC-RWA-02` | Sudah ada; pemakaian menunggu `OD-RWA-03` |
| `PUT` | `/{id}` | Mengubah persetujuan konsep | `PatientConsent : Update` | idem | idem | `EPIC-RWA-02` | Sudah ada |
| `PATCH` | `/{id}/sign` | Menandai ditandatangani | `PatientConsent : Update` | idem | idem | `EPIC-RWA-02` | Sudah ada |
| `PATCH` | `/{id}/cancel` | Membatalkan | `PatientConsent : Update` | idem | idem | `EPIC-RWA-02` | Sudah ada |

### 13.3 Health Services / Clinical Management / Patient Assessment — tidak dipakai sejak v`0.2`

Base URL: `api/v1/health-services/clinical-management/patient-assessments`. Pada v`0.1` dipakai menu Asesmen Edukasi (`EPIC-RWA-04`). Menu itu dicabut (`DEC-RWA-003`), sehingga Workspace Admisi **tidak** memanggil grup endpoint ini. Nomor bagian dipertahankan supaya rujukan bagian 13.4 s.d. 13.6 tidak bergeser.

### 13.4 Health Services / Billing Management / Billing / Patient Funds — sudah ada, dipakai ulang

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/deposits/episodes/{episodeId}` | Kekurangan deposit menurut kebijakan, deposit diterima, interval tindak lanjut | `BillingDeposit : Read` | - | `EpisodeDepositSummaryResponse` | `EPIC-RWA-01`, `08` | Sudah ada |
| `GET` | `/deposit-policies` | Kebijakan deposit per penjamin dan kelas | `BillingDeposit : Read` | query `guarantorId`, `patientClassId` | `DepositPolicyResponse` | `EPIC-RWA-08` | Sudah ada |

### 13.5 Health Services / Master Data / Inpatient Clearance Item — sudah ada, diperluas

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`. Semua endpoint yang ada dipertahankan dengan hak `InpatientClearanceItem : Read/Create/Update/Delete`. Perluasan **Rencana (belum tersedia)**: saringan dan isian jenis checklist (`CLOSURE` / `NEW_PATIENT_HANDOVER`) serta induk sub-butir (`EPIC-RWA-03`).

### 13.6 Health Services / Inpatient Management / Inpatient Ancillary Order — sudah ada, calon sumber tarif

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders`. `GET /coverage-status` (`InpatientEpisode : Read`) sudah memberi status tanggungan dan perkiraan harga per butir. Pemakaiannya untuk Estimasi Biaya menunggu `OD-RWA-07`.

---

## 14. Matriks kewenangan

String permission di bawah adalah **usulan** untuk grup baru dan string **persis** untuk yang sudah ada. Registry permission Final lahir dari atribut endpoint, sehingga usulan ini baru berlaku setelah endpoint-nya dibuat.

| Tindakan | Permission | Petugas admisi | CRO | Perawat ruangan | Kepala ruangan | Kasir | Supervisor admisi |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Melihat tombol dan membuka ruang kerja | `InpatientAdmissionDocument : Read` (usulan) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Melihat rupiah (deposit, estimasi) | `InpatientAdmissionDocument : ReadAmount` (usulan) | ✓ | — | — | — | ✓ | ✓ |
| Membuat konsep | `InpatientAdmissionDocument : Create` (usulan) | ✓ | — | — | — | — | ✓ |
| Mengubah, mengunci, membuat versi koreksi, membuang konsep sendiri | `InpatientAdmissionDocument : Update` (usulan) | ✓ | — | — | — | — | ✓ |
| Merekam tanda tangan pasien/keluarga; atestasi slot Admission/PPRI | `InpatientAdmissionDocument : Sign` (usulan) | ✓ | — | — | — | — | ✓ |
| Atestasi slot CRO | `InpatientAdmissionDocument : SignAsCro` (usulan) | — | ✓ | — | — | — | — |
| Atestasi slot Perawat penerima | `InpatientAdmissionDocument : SignAsNurse` (usulan) | — | — | ✓ | ✓ | — | — |
| Atestasi slot Kepala Ruangan | `InpatientAdmissionDocument : SignAsHeadNurse` (usulan) | — | — | — | ✓ | — | — |
| Mencetak dan mencetak ulang | `InpatientAdmissionDocument : Print` (usulan) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Membatalkan dokumen yang sudah dikunci | `InpatientAdmissionDocument : Cancel` (usulan) | — | — | — | — | — | ✓ |
| Persetujuan umum | `PatientConsent : Create` / `PatientConsent : Update` (sudah ada) | ✓ | — | — | — | — | ✓ |
| Ringkasan deposit | `BillingDeposit : Read` (sudah ada) | ✓ | — | — | — | ✓ | ✓ |
| Butir checklist serah terima | `InpatientClearanceItem : Create` / `InpatientClearanceItem : Update` (sudah ada) | — | — | — | — | — | Admin RS |

Pembagian peran di atas adalah **bawaan usulan**; rumah sakit mengatur pemberian permission lewat Platform/Authorization. Yang dikunci adalah **aturan**: hak menentukan tindakan, bukan teks nama peran (kelemahan V1 nomor 13).

---

## 15. Batas integrasi dan billing

Workspace Admisi **MUST NOT**:

1. Menghitung kekurangan deposit, tarif, biaya admin, atau aturan cito sendiri. Semua angka berasal dari Billing/Master Data dan dibekukan saat ditandatangani.
2. Menulis ke tabel Billing mana pun, termasuk menerima deposit atau mengubah kebijakan deposit.
3. Membaca tabel Billing secara langsung (`BilFolio`, `BilChargeLine`, `BilDepositAccount`); hanya lewat endpoint Billing (`RWI-DEC-102` butir e).
4. Membuat tabel persetujuan kedua; persetujuan umum memakai tabel persetujuan Clinical Management.
5. Menyalin master pasien, wali, kontak darurat, atau penjamin ke tabel sendiri, kecuali sebagai **salinan beku** di dalam dokumen yang sudah ditandatangani (`INV-RWA-05`).
6. Mengubah kolom tabel modul lain tanpa persetujuan pemiliknya. Perluasan master butir administrasi, pengaturan Rawat Inap, jenis dokumen keutuhan, dan master pasien masing-masing butuh persetujuan pemiliknya.
7. Mencetak data karangan: nomor kartu acak, benefit bawaan, atau tarif cadangan.
8. Menahan admisi, penempatan, perawatan, transfer, atau keputusan pulang karena dokumen admisi belum lengkap (`FR-RWA-006`).
9. Menurunkan kelas kamar pasien secara otomatis saat jatuh tempo deposit terlewati; itu tetap tindakan transfer manual.

Integrasi masuk dan keluar:

| Arah | Dengan | Isi | Cara |
|---|---|---|---|
| Masuk | Billing | Ringkasan deposit episode | Panggilan baca sinkron; gagal → angka tidak tampil, form deposit tidak bisa dikunci |
| Masuk | Patient Management, Registration | Pasien, relasi, kontak darurat, penjamin | Panggilan baca / query modul pemilik |
| Keluar | Clinical Management | Persetujuan umum | Endpoint persetujuan yang sudah ada |
| Keluar | Medical Record Management | Penguncian dan keutuhan dokumen | Layanan keutuhan yang sudah ada (`OD-RWA-06`) |
| Keluar | Daftar periksa penutupan episode | Butir "persetujuan umum sudah ditandatangani" | Di dalam Rawat Inap, bila `RWI-RULE-025` disetujui |
| Keluar | Sub-modul keperawatan dan dokter | Ringkasan nilai kepercayaan dan privasi | Endpoint baca `FR-RWA-112` |

---

## 16. Guardrail regulasi

Rujukan berikut adalah **acuan untuk diverifikasi pemilik hukum/privasi dan tim akreditasi**, bukan tafsir final dokumen ini.

| Kewajiban | Rujukan | Dampak pada MVP |
|---|---|---|
| Persetujuan pasien atas pelayanan dan tindakan kedokteran | Permenkes No. 290/MENKES/PER/III/2008 tentang Persetujuan Tindakan Kedokteran; UU No. 17 Tahun 2023 tentang Kesehatan | General Consent tersimpan dan dapat dibuktikan; penanda tangan wali tercatat hubungannya. Keputusan penyimpanan menunggu `DEC-INP-003` |
| Rekam medis elektronik: kelengkapan, keutuhan, kerahasiaan, masa simpan | Permenkes No. 24 Tahun 2022 tentang Rekam Medis | Tidak ada hapus permanen; versi dan jejak audit; penguncian dokumen; masa simpan ditetapkan pemilik hukum |
| Pelindungan data pribadi pasien dan keluarga (nomor identitas, tanda tangan, alamat) | UU No. 27 Tahun 2022 tentang Pelindungan Data Pribadi | Kolom sensitif tidak masuk log; QR tidak memuat data pribadi lengkap; akses berdasar permission |
| Keabsahan tanda tangan elektronik | UU Informasi dan Transaksi Elektronik beserta perubahannya; PP No. 71 Tahun 2019 | Mode digital (`EPIC-RWA-13`) menunggu keputusan hukum; mode kertas tersedia sebagai jalan aman |
| Standar akreditasi: hak pasien dan keluarga (persetujuan umum, privasi, nilai dan kepercayaan) dan sasaran keselamatan pasien (identifikasi dengan gelang) | Standar Akreditasi Rumah Sakit Kementerian Kesehatan | Permintaan Privasi, Nilai Kepercayaan, dan Gelang masuk MVP |

---

## 17. Kebutuhan non-fungsional

| ID | Kategori | Kebutuhan | Contoh / ukuran |
|---|---|---|---|
| `NFR-RWA-01` | Atomicity | Dokumen, butir, dan slot tanda tangan tersimpan dalam satu transaksi | Simpan Serah Terima dengan 18 butir gagal di butir ke-12 → tidak ada satu butir pun tersimpan |
| `NFR-RWA-02` | Concurrency | Perubahan membawa nomor versi; versi basi ditolak 409 | Sari dan Dewi menyimpan pada detik yang sama → satu berhasil, satu diminta memuat ulang |
| `NFR-RWA-03` | Idempotency | Simpan, tanda tangan, dan log cetak menerima `Idempotency-Key` (pola yang sudah dipakai Billing) | Klik ganda → satu rekaman |
| `NFR-RWA-04` | Audit | Setiap buat, ubah, kunci, tanda tangan, batal, dan cetak tercatat dengan akun dan waktu | Riwayat menampilkan "Dikunci oleh Sari 10.05; CRO Dewi 10.20; Perawat Andi 10.40" |
| `NFR-RWA-05` | Keutuhan | Dokumen `Completed` tidak berubah; berkas tanda tangan digital ber-hash SHA-256 | Hash berkas berbeda dari catatan → layar menampilkan "bukti tanda tangan tidak utuh" |
| `NFR-RWA-06` | Privasi | Nomor identitas, gambar tanda tangan, dan alamat tidak masuk log aplikasi dan disamarkan di daftar | Daftar dokumen menampilkan `3275•••••••••001` |
| `NFR-RWA-07` | Otorisasi | Setiap endpoint dijaga `[AccessPermission]`; tombol disembunyikan **dan** server menolak | Memanggil `/cancel` tanpa hak → 403 walaupun tombolnya tidak terlihat |
| `NFR-RWA-08` | Waktu | Disimpan UTC, ditampilkan WIB menurut zona profil rumah sakit (`Asia/Jakarta`); "hari kerja" memakai Senin–Jumat | Surat 9 Oktober 2026 → jatuh tempo bawaan 12 Oktober 2026 |
| `NFR-RWA-09` | Kinerja | Ringkasan ruang kerja termuat ≤ 2 detik untuk satu episode; data cetak ≤ 2 detik | Diukur pada data uji 50 dokumen per episode |
| `NFR-RWA-10` | Tablet | Ruang kerja dapat dipakai di tablet berlayar ≥ 10 inci untuk tanda tangan | Navigasi kiri dapat dilipat |
| `NFR-RWA-11` | Konfigurasi | Tidak ada identitas RS, kota, telepon, email, kode formulir, atau angka aturan biaya yang ditanam di kode | Mengganti kode formulir lewat pengaturan langsung berlaku pada cetakan berikutnya |
| `NFR-RWA-12` | Koreksi | Koreksi hanya lewat versi baru beralasan | Lihat `FR-RWA-125` |
| `NFR-RWA-13` | *Real time* | Tanda tangan pihak lain tampil ≤ 30 detik tanpa memuat ulang | Lihat `FR-RWA-035` |
| `NFR-RWA-14` | Aksesibilitas | Lencana status tidak hanya dibedakan warna; ada teks atau ikon | "Lengkap", "Konsep" terbaca pembaca layar |

---

## 18. Skenario UAT

Data uji samaran: pasien **Tn. Budi Santoso** (RM `00-12-34-56`, lahir 12 Maret 1981, penjamin asuransi PT Asuransi Sehat Sentosa, kartu `7788-0012-3456`, kelas 2, Melati 03 Bed B), istri **Ny. Rina Santoso**; pasien tunai **Ny. Wati**; bayi **By. Ny. Rina** umur 3 hari. Petugas: **Sari** (admisi), **Dewi** (CRO), **Andi** (perawat ruangan), **Maya** (kepala ruangan), **Yudi** (kasir), **Hendra** (supervisor admisi).

> **UAT-RWA-01 — Membuka Workspace Admisi dari Detail Episode** (`EPIC-RWA-01`, berhasil)
>
> **Kondisi awal:** episode Budi `Admitted`; Sari punya hak baca dokumen admisi; Billing mencatat kekurangan deposit Rp 3.000.000.
>
> **Langkah:** buka Detail Episode Budi → tekan **Workspace Admisi**.
>
> **Hasil yang diharapkan:** tombol berada tepat sesudah Workspace Dokter; ruang kerja tampil dengan header Budi, "Deposit kurang Rp 3.000.000", navigasi sembilan menu urutan V1 (tanpa Assessment Edukasi; MP Benefit tidak tampil), dan "Dokumen admisi 0 dari 7 lengkap".

> **UAT-RWA-02 — Pengguna tanpa hak dan episode tertutup** (`EPIC-RWA-01`, gagal)
>
> **Kondisi awal:** pengguna Gizi tanpa `InpatientAdmissionDocument : Read`; episode Ny. Wati `Closed`.
>
> **Langkah:** (1) pengguna Gizi membuka Detail Episode Budi, lalu mengetik alamat ruang kerja langsung. (2) Sari membuka ruang kerja Wati dan mencoba mengubah Nilai Kepercayaan.
>
> **Hasil yang diharapkan:** (1) tombol tidak terlihat; alamat langsung menampilkan "Akses Tidak Tersedia". (2) Tidak ada tombol ubah; panggilan langsung ke server ditolak 409 "Episode sudah ditutup"; cetak ulang tetap bisa dengan alasan.

> **UAT-RWA-03 — General Consent dengan data istri** (`EPIC-RWA-02`, berhasil — dijalankan setelah `OD-RWA-02`/`03`)
>
> **Kondisi awal:** Budi punya relasi istri Rina Santoso beralamat "Jl. Kenanga No. 5, Bekasi".
>
> **Langkah:** pilih hubungan **Istri** → periksa isian → Panduan "Ya" → simpan → kunci → rekam tanda tangan Rina → Maya menandatangani sebagai Kepala Ruangan → cetak.
>
> **Hasil yang diharapkan:** nama dan alamat Rina terisi otomatis; tipe kamar "Umum"; dokumen `Completed`; cetakan memuat nama Maya sebagai Kepala Ruangan walaupun yang mencetak Sari; tab Surat Persetujuan 12 butir memakai penerima informasi yang sama.

> **UAT-RWA-04 — General Consent tanpa data wali** (`EPIC-RWA-02`, gagal)
>
> **Kondisi awal:** Wati tidak punya data relasi anak.
>
> **Langkah:** pilih hubungan **Anak** → coba kunci tanpa mengisi nama.
>
> **Hasil yang diharapkan:** isian nama/alamat terbuka dengan keterangan "tidak ditemukan di data wali/kontak darurat"; kunci ditolak dengan pesan "Nama penanda tangan wajib diisi".

> **UAT-RWA-05 — Serah Terima lengkap tiga pihak** (`EPIC-RWA-03`, berhasil)
>
> **Kondisi awal:** gelang Budi dicetak 09.50; IPD dicetak 09.55.
>
> **Langkah:** Sari membuka Serah Terima → butir 9 dan 13 bersaran Sudah → Sari mengonfirmasi semua 18 butir → kunci → tanda tangan Admission 10.05; Dewi tanda tangan CRO 10.20; Andi tanda tangan Perawat 10.40.
>
> **Hasil yang diharapkan:** setiap tanda tangan tampil di layar pihak lain ≤ 30 detik; pukul 10.40 status `Completed`; kelengkapan naik satu.

> **UAT-RWA-06 — Satu petugas dua slot** (`EPIC-RWA-03`, gagal)
>
> **Kondisi awal:** Sari punya hak `Sign` dan `SignAsCro`; Sari sudah menandatangani slot Admission.
>
> **Langkah:** Sari menekan Tandatangani sebagai CRO.
>
> **Hasil yang diharapkan:** ditolak 422 "Satu petugas tidak boleh menandatangani dua kolom pada serah terima yang sama"; slot CRO tetap kosong.

> **UAT-RWA-07 — Butir Belum tanpa keterangan** (`EPIC-RWA-03`, gagal)
>
> **Langkah:** butir 11 dipilih Belum tanpa keterangan, lalu Kunci.
>
> **Hasil yang diharapkan:** ditolak dengan pesan "Butir 11 berstatus Belum wajib diberi keterangan"; status tetap `Draft`.

> **UAT-RWA-08 — Edukasi dari admisi terbaca di keperawatan** — **Dibatalkan v`0.2`** (`DEC-RWA-003`). Asesmen Edukasi tidak lagi ada di Workspace Admisi. Nomor tidak dipakai ulang.

> **UAT-RWA-09 — Petugas tanpa hak asesmen** — **Dibatalkan v`0.2`** (`DEC-RWA-003`). Nomor tidak dipakai ulang.

> **Pemeriksaan pengganti di `UAT-RWA-01`:** navigasi kiri Workspace Admisi **tidak** memuat menu Assessment Edukasi, dan ruang kerja tidak memanggil endpoint asesmen pasien.

> **UAT-RWA-10 — Gelang dewasa dan label tanpa nomor kartu karangan** (`EPIC-RWA-05`, berhasil)
>
> **Langkah:** cetak Gelang Budi; cetak Label Ny. Wati (tunai).
>
> **Hasil yang diharapkan:** gelang "BUDI SANTOSO, Tn." · "12 Mar 1981 (45 th)" · "00-12-34-56"; label Wati tanpa baris nomor kartu; QR hanya berisi pengenal; log mencatat dua cetakan pertama.

> **UAT-RWA-11 — Gelang bayi** (`EPIC-RWA-05`, berhasil)
>
> **Langkah:** cetak gelang By. Ny. Rina.
>
> **Hasil yang diharapkan:** Gelang Bayi dengan dua label kecil; umur tercetak "3 hari".

> **UAT-RWA-12 — Cetak ulang tanpa alasan** (`EPIC-RWA-05`, gagal)
>
> **Langkah:** cetak ulang gelang Budi tanpa memilih alasan.
>
> **Hasil yang diharapkan:** ditolak dengan pesan "Pilih alasan cetak ulang"; setelah memilih "rusak", log mencatat "Cetakan ke-2, rusak".

> **UAT-RWA-13 — Permintaan Privasi tersimpan dan terbuka lagi** (`EPIC-RWA-06`, berhasil)
>
> **Langkah:** isi kerabat "Ny. Rina Santoso" dan "Sdr. Dimas, Jr.", permintaan khusus "Tidak menerima tamu dari kantor", privasi transportasi Ya → simpan → kunci → catat tanda tangan kertas Budi → Maya menandatangani → tutup menu → buka lagi.
>
> **Hasil yang diharapkan:** dokumen `Completed` tampil kembali, bukan form kosong; "Sdr. Dimas, Jr." tetap satu nama; header menampilkan ringkasan privasi.

> **UAT-RWA-14 — Cetak sebelum ditandatangani** (`EPIC-RWA-06`, gagal)
>
> **Langkah:** cetak Permintaan Privasi saat status `Draft`.
>
> **Hasil yang diharapkan:** cetakan bertanda "KONSEP — BELUM DITANDATANGANI"; tidak ada cetakan final; kelengkapan tidak berubah.

> **UAT-RWA-15 — IPD merangkum dokumen** (`EPIC-RWA-07`, berhasil)
>
> **Kondisi awal:** Nilai Kepercayaan dan Permintaan Privasi Budi `Completed`.
>
> **Langkah:** buka IPD → cetak.
>
> **Hasil yang diharapkan:** isian penjamin, kartu, kelas, DPJP terisi; "Nilai Kepercayaan" dan "Permintaan Privat" terisi; isian tanpa sumber berupa garis kosong; log cetak bertambah dan butir 9 Serah Terima bersaran Sudah.

> **UAT-RWA-16 — Data episode gagal dimuat** (`EPIC-RWA-07`, gagal)
>
> **Kondisi awal:** layanan episode dimatikan sementara di lingkungan uji.
>
> **Langkah:** buka IPD.
>
> **Hasil yang diharapkan:** tombol Cetak nonaktif dengan pesan "Cetak ditahan sampai data wajib terbaca lengkap" dan tombol Coba Lagi.

> **UAT-RWA-17 — Pelunasan deposit dengan angka Billing** (`EPIC-RWA-08`, berhasil — dijalankan setelah `OD-RWA-14`)
>
> **Kondisi awal:** kebijakan deposit asuransi kelas 2 Rp 5.000.000; deposit masuk Rp 2.000.000; `FollowUpIntervalDays` 3; tanggal surat Jumat 9 Oktober 2026.
>
> **Langkah:** pilih sumber wali Ny. Rina → Ambil Data Wali → periksa jatuh tempo → simpan → kunci → catat tanda tangan → Sari menandatangani sebagai petugas → cetak. Kemudian kasir Yudi menerima tambahan deposit Rp 1.000.000 → cetak ulang.
>
> **Hasil yang diharapkan:** kekurangan Rp 3.000.000 dengan perhitungan "Rp 5.000.000 − Rp 2.000.000"; jatuh tempo bawaan Senin 12 Oktober 2026 11.00 WIB; cetak ulang tetap Rp 3.000.000 (angka beku), sedangkan header menampilkan kekurangan terkini Rp 2.000.000.

> **UAT-RWA-18 — Tidak ada kekurangan dan jatuh tempo kelewat batas** (`EPIC-RWA-08`, gagal)
>
> **Langkah:** (1) buka Pelunasan Deposit untuk Wati yang depositnya cukup. (2) Untuk Budi, isi jatuh tempo 13 Oktober 2026.
>
> **Hasil yang diharapkan:** (1) "Surat pelunasan tidak diperlukan", tombol simpan nonaktif. (2) Ditolak 422 "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit".

> **UAT-RWA-19 — Penjelasan prakiraan biaya dari tarif** (`EPIC-RWA-09`, berhasil — dijalankan setelah `OD-RWA-07`)
>
> **Langkah:** isi rencana appendektomi, lama rawat 3 hari, DPJP; tambah lain-lain Rp 500.000 "perlengkapan pasca operasi" → simpan → kunci → tanda tangan → cetak.
>
> **Hasil yang diharapkan:** tindakan Rp 8.500.000, kamar 3 × Rp 750.000, visit 3 × Rp 250.000 terisi dari tarif; total Rp 12.000.000; catatan aturan berasal dari pengaturan Billing.

> **UAT-RWA-20 — Tarif tidak ditemukan** (`EPIC-RWA-09`, gagal)
>
> **Langkah:** pilih tindakan yang belum punya tarif untuk penjamin Budi → kunci.
>
> **Hasil yang diharapkan:** baris bertanda "Tarif belum tersedia"; kunci ditolak sampai baris diisi manual dengan alasan atau dihapus.

> **UAT-RWA-21 — Selisih Biaya pasien asuransi** (`EPIC-RWA-10`, berhasil)
>
> **Langkah:** subjek "istri saya" → deklarer Rina, KTP, HP `081234567890` → simpan → kunci → catat tanda tangan kertas → Sari menandatangani sebagai Petugas PPRI → cetak.
>
> **Hasil yang diharapkan:** data pasien hanya-baca dari master; subjek tersimpan sebagai kode; cetakan dwibahasa dengan nama RS dari profil, bukan nama yang ditanam di kode.

> **UAT-RWA-22 — Pasien tunai dan nomor HP terlalu panjang** (`EPIC-RWA-10`, gagal)
>
> **Langkah:** (1) buka Selisih Biaya untuk Wati (tunai). (2) Untuk Budi, isi HP `08123456789012` (14 digit).
>
> **Hasil yang diharapkan:** (1) menu berlencana "Tidak diperlukan" dan form tidak dapat dibuat. (2) Ditolak "Nomor telepon maksimal 13 digit".

> **UAT-RWA-23 — Nilai Kepercayaan terisi dari episode sebelumnya** (`EPIC-RWA-11`, berhasil)
>
> **Kondisi awal:** episode Budi Oktober 2026 punya dua butir; Budi masuk lagi Januari 2027.
>
> **Langkah:** buka Nilai Kepercayaan pada episode Januari.
>
> **Hasil yang diharapkan:** dua butir terisi sebagai konsep; status `Draft` sampai dikonfirmasi dan ditandatangani ulang.

> **UAT-RWA-24 — Butir ke-6 dan nol butir** (`EPIC-RWA-11`, gagal)
>
> **Langkah:** (1) tambah butir ke-6. (2) Hapus semua butir lalu kunci.
>
> **Hasil yang diharapkan:** (1) "Maksimal 5 butir". (2) "Minimal satu hal yang bertentangan wajib diisi".

> **UAT-RWA-25 — Koreksi lewat versi baru** (`EPIC-RWA-12`, berhasil)
>
> **Langkah:** General Consent atau Selisih Biaya `Completed` → Buat Versi Koreksi dengan alasan "koreksi alamat penanda tangan" → ubah → kunci → tanda tangan.
>
> **Hasil yang diharapkan:** versi 1 `Superseded`, versi 2 `Completed`; riwayat menampilkan keduanya; kelengkapan tetap terhitung satu dokumen.

> **UAT-RWA-26 — Dua petugas menyimpan bersamaan** (`EPIC-RWA-12`, gagal)
>
> **Langkah:** Sari dan Hendra membuka konsep yang sama; Sari menyimpan; Hendra menyimpan dari layar lama.
>
> **Hasil yang diharapkan:** simpanan Hendra ditolak 409 "Dokumen sudah diubah petugas lain, muat ulang"; perubahan Sari tidak tertimpa.

> **UAT-RWA-27 — Batal tanpa alasan cukup dan hapus** (`EPIC-RWA-12`, gagal)
>
> **Langkah:** Hendra membatalkan Serah Terima `Completed` dengan alasan "salah"; lalu mencari tombol hapus.
>
> **Hasil yang diharapkan:** ditolak "Alasan pembatalan minimal 10 karakter"; tidak ada tombol maupun endpoint hapus.

---

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Tombol Workspace Admisi tampil tepat sesudah Workspace Dokter hanya bagi pemegang hak baca | `UAT-RWA-01`, `UAT-RWA-02` |
| Ruang kerja memakai empat wilayah template Workspace Keperawatan dan menyimpan menu/tab di alamat halaman | `UAT-RWA-01`; tinjauan visual berdampingan dengan Workspace Keperawatan |
| Kelengkapan dokumen dihitung menurut aturan `FR-RWA-005` untuk pasien asuransi dan pasien tunai | `UAT-RWA-01`, `UAT-RWA-21`, `UAT-RWA-22` |
| Episode `Closed`/`Cancelled` hanya-baca, cetak ulang beralasan | `UAT-RWA-02` |
| Serah Terima ditandatangani tiga orang berbeda dan menolak satu orang dua slot | `UAT-RWA-05`, `UAT-RWA-06` |
| Butir checklist berasal dari master, dibekukan di dokumen | `UAT-RWA-05`; data awal 15 + 3 butir terpasang |
| Workspace Admisi tidak memuat menu Assessment Edukasi (`DEC-RWA-003`) | `UAT-RWA-01` |
| Gelang dewasa dan bayi tercetak benar, label tanpa nomor karangan, cetak ulang beralasan | `UAT-RWA-10`, `UAT-RWA-11`, `UAT-RWA-12` |
| Permintaan Privasi tersimpan per baris dan terbuka kembali | `UAT-RWA-13` |
| IPD merangkum dokumen admisi dan menahan cetak bila data wajib gagal | `UAT-RWA-15`, `UAT-RWA-16` |
| Selisih Biaya hanya untuk penjamin asuransi/perusahaan, data pasien hanya-baca | `UAT-RWA-21`, `UAT-RWA-22` |
| Nilai Kepercayaan per episode dengan isian dari episode sebelumnya, 1–5 butir | `UAT-RWA-23`, `UAT-RWA-24` |
| Koreksi hanya lewat versi; tidak ada hapus; batal beralasan | `UAT-RWA-25`, `UAT-RWA-27` |
| Penyimpanan bersamaan ditolak 409 | `UAT-RWA-26` |
| Tidak ada identitas RS, kota, telepon, email, atau kode formulir yang ditanam di kode cetak Workspace Admisi | Pencarian source atas nama RS client, "Jakarta", dan pola kode formulir pada berkas Workspace Admisi mengembalikan nol hasil |
| Tidak ada data karangan (nomor acak, benefit bawaan) | Pencarian `Math.random` dan angka benefit bawaan pada berkas Workspace Admisi mengembalikan nol hasil; `UAT-RWA-10` |
| Setiap endpoint baru dijaga `[AccessPermission]` dan muncul di registry permission | Daftar registry permission setelah build memuat `InpatientAdmissionDocument` beserta tindakannya |
| Data awal pengaturan (kode formulir V1, kota, butir checklist) terisi | Rencana data master awal `design-business-module` dan hasil seeder |
| Epic `OPEN DECISION` (`EPIC-RWA-02`, `08`, `09`, `13`) hanya dikerjakan setelah keputusannya tercatat | Decision log memuat keputusan bernama untuk `OD-RWA-02`, `03`, `07`, `14` |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang pengiriman

| Gelombang | Isi | Epic | Syarat mulai |
|---|---|---|---|
| `RWA-MVP-0` | Fondasi: tabel dokumen admisi, slot tanda tangan (mode kertas + atestasi), log cetak, perluasan master butir checklist dan data awalnya, pengaturan kode formulir/kota/kop, jenis dokumen keutuhan, registry permission | `EPIC-RWA-12` | `OD-RWA-05` dan `OD-RWA-06` dijawab; blueprint dan kontrak disetujui |
| `RWA-MVP-1` | Ruang kerja dan tombol, header, kelengkapan; Gelang & Label; IPD | `EPIC-RWA-01`, `EPIC-RWA-05`, `EPIC-RWA-07` | `RWA-MVP-0` selesai |
| `RWA-MVP-2` | Dokumen bertanda tangan inti dalam mode kertas | `EPIC-RWA-03`, `EPIC-RWA-06`, `EPIC-RWA-10`, `EPIC-RWA-11` | `RWA-MVP-1` selesai |
| `POST-MVP` | Seluruh kemampuan pada bagian 8 | — | Di luar rilis pertama |

Nama gelombang memakai awalan `RWA-` supaya tidak bertabrakan dengan nama gelombang Rawat Inap yang sudah ada (`DOK-MVP-0`, `Gelombang 1A`, `MVP-0` milik PRD lain).

### 20.2 Epic yang menunggu keputusan — tidak masuk gelombang mana pun

| Epic | Menunggu | Gelombang tujuan setelah diputuskan | Jalan sementara |
|---|---|---|---|
| `EPIC-RWA-02` General Consent | `OD-RWA-02`, `OD-RWA-03` | Sesudah `RWA-MVP-2` | Cetak tanpa simpan yang sudah ada (`RWI-DEC-077`) |
| `EPIC-RWA-08` Pelunasan Deposit | `OD-RWA-14` | Bersama `RWA-MVP-2` bila dijawab sebelum gelombang itu mulai | Surat kertas manual dengan angka dari kasir |
| `EPIC-RWA-09` Estimasi Biaya (Rekap) | `OD-RWA-07` | Sesudah `RWA-MVP-2` | Penjelasan lisan kasir/admisi dengan lembar kertas |
| `EPIC-RWA-13` Tanda tangan digital | `OD-RWA-02` | Sesudah `RWA-MVP-2`; mengaktifkan mode digital pada semua dokumen | Mode kertas `FR-RWA-122` |

### 20.3 Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Usulan jawaban | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|---|---|:---:|
| `OD-RWA-01` | Nama ruang kerja | `DEC-RWA-001`: tombol "Workspace Admisi", judul "Ruang Kerja Admisi Rawat Inap", subjudul "Penerimaan Pasien Rawat Inap (PPRI)" (bagian 3.6) | Muhammad Hamzah | Label memakai usulan | Tidak |
| `OD-RWA-02` | Tanda tangan pasien/keluarga: digital di layar (V1) atau kertas? | Digital dengan hash, waktu, perangkat, dan IP sebagai mode utama; kertas sebagai cadangan; keduanya pengaturan | Pemilik privasi/hukum (`DEC-INP-003`, belum ditunjuk) + Muhammad Hamzah | `EPIC-RWA-13` tidak dapat dimulai; MVP berjalan mode kertas | **Ya** (`EPIC-RWA-13`) |
| `OD-RWA-03` | General Consent disimpan per episode (membuka ulang `RWI-DEC-077` dan `RWI-CAP-031`); apakah form V1 dan surat 12 butir menjadi satu persetujuan; apakah `RWI-RULE-025` (menahan penutupan) disetujui | Disimpan di tabel persetujuan Clinical Management jenis `Admission`; form V1 + surat 12 butir = satu persetujuan; butir penutupan ditandai otomatis | Pemilik privasi/hukum + Muhammad Hamzah | `EPIC-RWA-02` tidak dapat dimulai | **Ya** (`EPIC-RWA-02`) |
| `OD-RWA-04` | Tanda tangan petugas: gambar dari master tanda tangan (V1) atau atestasi elektronik? | Atestasi untuk MVP (`FR-RWA-123`); master gambar dibangun Platform bila diinginkan, memperbaiki endpoint `/v1/Auth/signature/register` yang hilang | Muhammad Hamzah + pemilik Platform/Authorization | Cetakan memakai atestasi | Tidak |
| `OD-RWA-05` | Letak di blueprint: sub-modul ke-5 `admisi-rawat-inap` atau bagian `episode-rawat-inap`? | Sub-modul baru `admisi-rawat-inap`: pelaku, layar, dan tabelnya berbeda, approval dan roadmap terpisah, dan `episode-rawat-inap` sudah sangat besar | Muhammad Hamzah | `design-business-module` tidak dapat mulai | **Ya** (desain) |
| `OD-RWA-06` | Pemilik tabel dokumen admisi: Rawat Inap atau Clinical Management/Rekam Medis? Apakah memakai mesin keutuhan dokumen Rekam Medis? | Rawat Inap (cakupan dokumen rawat inap), dengan endpoint baca untuk modul lain; penguncian memakai mesin keutuhan dokumen dengan jenis baru | Muhammad Hamzah + pemilik Clinical Management/Rekam Medis | Model data tidak dapat ditetapkan | **Ya** (desain) |
| `OD-RWA-07` | Estimasi Biaya: rumus, sumber tarif, dan catatan aturan (cito +25%, > 4 jam +25%/jam, *standby* 20%, admin 7% maks. Rp 6.000.000, anestesi 50%) — milik siapa dan dibaca dari mana? | Tarif dari master tarif lewat layanan perkiraan harga; catatan dari pengaturan Billing; dokumen milik Rawat Inap | Yasmina (Billing) | `EPIC-RWA-09` tidak dapat dimulai | **Ya** (`EPIC-RWA-09`) |
| `OD-RWA-08` | Bolehkah petugas admisi melihat rupiah per butir tagihan berjalan (Estimasi Rinci)? | Tetap ditunda; kasir mencetak dari Billing | Yasmina + Muhammad Hamzah | `CAP-RWA-10` tetap ditunda | Tidak |
| `OD-RWA-09` | MP Benefit: sumber nilai, penukaran kupon (kantin/Gizi), sekali atau per hari? | Aturan tanggungan per penjamin/paket/kelas sebagai sumber; kode kupon diterbitkan server; penukaran dicatat Gizi | Yasmina + Ikbal Yulianto | `CAP-RWA-12` tetap ditunda | Tidak |
| `OD-RWA-10` | Nilai Kepercayaan per pasien (V1) atau per episode? | `DEC-RWA-010`: per episode dengan isian dari episode sebelumnya | Muhammad Hamzah | Memakai usulan | Tidak |
| `OD-RWA-11` | Dokumen mana yang wajib dan apa akibatnya bila belum lengkap? | Tabel `FR-RWA-005`; hanya peringatan (`FR-RWA-006`); Estimasi wajib hanya bila ada rencana tindakan | Muhammad Hamzah | Memakai usulan | Tidak |
| `OD-RWA-12` | Serah Terima: semua butir wajib dipilih dan butir Belum wajib berketerangan (V1 boleh kosong)? | `DEC-RWA-012`: ya | Muhammad Hamzah | Memakai usulan | Tidak |
| `OD-RWA-13` | Siapa yang sah menandatangani kolom "Kepala Ruangan" (V1: siapa pun yang masuk)? | Hanya pemegang `SignAsHeadNurse`; bila kepala ruangan tidak ada, label cetak menjadi "Petugas yang menerima" | Muhammad Hamzah + kepala keperawatan | Memakai usulan | Tidak |
| `OD-RWA-14` | Dasar kekurangan deposit = kebijakan deposit Billing (mengganti Rp 100.000.000 V1)? Batas jatuh tempo = tanggal surat + interval tindak lanjut kebijakan? | Ya untuk keduanya (`DEC-RWA-014`) | Yasmina | `EPIC-RWA-08` tidak dapat dimulai | **Ya** (`EPIC-RWA-08`) |
| `OD-RWA-15` | Isian IPD dan Selisih Biaya tanpa sumber (pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat KTP/domisili, alamat kantor, sapaan, nama sendiri, no. mutasi, persetujuan direktur, perhatian khusus) | Cetak garis kosong untuk MVP; perluasan master pasien diajukan ke Patient Management sebagai pekerjaan terpisah | Muhammad Hamzah + pemilik Patient Management | Garis kosong | Tidak |
| `OD-RWA-16` | Gelang: batas umur gelang bayi, sapaan (Tn./Ny. V1 vs An./Nn./By.), isi QR | Batas 5 tahun dapat diatur; sapaan dari aturan umur, jenis kelamin, status nikah; QR berisi pengenal saja | Muhammad Hamzah + tim keselamatan pasien | Memakai usulan | Tidak |
| `OD-RWA-17` | Tombol "Cetak Persetujuan" di Detail Episode | Tetap sampai `EPIC-RWA-02` dikirim, lalu mengarah ke tab Surat Persetujuan di Workspace Admisi | Muhammad Hamzah | Tetap seperti sekarang | Tidak |
| `OD-RWA-18` | Kop surat dan kode formulir dari pengaturan; apakah cetakan Final lain yang menanam identitas RS ikut diperbaiki? | Workspace Admisi wajib dari pengaturan; perbaikan cetakan lain diajukan sebagai issue terpisah (G-RWA-02) | Muhammad Hamzah + Master Data | Workspace Admisi tetap dari pengaturan | Tidak |

**Enam pertanyaan memblokir:** `OD-RWA-02`, `03`, `05`, `06`, `07`, `14`. Selama `OD-RWA-05` dan `OD-RWA-06` terbuka, dokumen ini **tidak boleh** diteruskan ke `design-business-module` maupun `plan-module-delivery`.

### 20.4 Langkah berikutnya

Dokumen ini masih `draft` dan **belum disetujui**; persetujuan tetap tindakan pemilik.

| Urutan | Skill | Alasan |
|---:|---|---|
| 1 | `grill-me` (Amendment Pass) | Mencatat `DEC-RWA-003` (Assessment Edukasi dicabut dari admisi) ke decision log, lalu menjawab `OD-RWA-01` s.d. `OD-RWA-18`, terutama enam yang memblokir, sebagai `RWI-DEC-225` dst. (ID terakhir di decision log: `RWI-DEC-224`, `RWI-OQ-115`) |
| 2 | `trace-existing-capabilities` | Mendaftarkan `CAP-RWA-01` s.d. `CAP-RWA-17` (tanpa `CAP-RWA-04` yang dibatalkan) ke capability map dan memeriksa ulang temuan bagian 3.3 terhadap HEAD terbaru |
| 3 | `requirement-completeness-gate` | Menilai kesiapan slice Workspace Admisi setelah keputusan turun |
| 4 | `design-business-module` | Merancang sub-modul (atau amandemen) beserta kontrak, kamus data, dan `04-prd-to-mvp.md` blueprint yang menurunkan dari dokumen ini |
| 5 | `plan-module-delivery` | Memecah gelombang `RWA-MVP-0` s.d. `RWA-MVP-2` menjadi task mulai `BE-RWI-185` dan `FE-RWI-202` (ID bebas berikutnya per manifest) |

---

## Lampiran A — Spesifikasi form V1 yang dipertahankan

Kolom "Perubahan Final" hanya berisi perbaikan kelemahan (bagian 3.4) atau keputusan yang sudah ada; **isian bisnisnya tidak berubah**.

### A.1 General Consent — "FORMULIR PERSETUJUAN UMUM (GENERAL CONSENT)", kode V1 `GC/ADM/001/Rev01/2024`

Sumber V1: `general-consent/form-general-consent.jsx:64-76, 498-516, 637-836`; `print-general-consent.jsx:182-510`; model `GeneralConsent.cs`.

| Bagian | Isian | Tipe / pilihan | Wajib | Isi otomatis V1 | Perubahan Final |
|---|---|---|:---:|---|---|
| Saya yang Bertanda Tangan | Hubungan dengan Pasien | Diri Sendiri, Suami, Istri, Anak, Orang Tua, Lainnya | Ya | — | Disimpan sebagai tipe penanda tangan persetujuan (`Patient`, `Spouse`, `Child`, `Parent`, `Other`) |
| | Nama | Teks | Ya | Dari pasien atau data wali sesuai hubungan; manual bila tidak cocok | Sumber: relasi pasien lalu kontak darurat (`FR-RWA-021`) |
| | Alamat | Teks | Ya | idem | idem |
| Informasi Kamar Rawat | Tipe Kamar Rawat | Umum; Khusus (ICU/Isolasi) | Ya | Ditebak dari nama kelas/kamar | Dari status isolasi dan tipe unit (`FR-RWA-022`) |
| | Kelas Kamar / Kamar Rawat | Teks hanya-baca "KELAS – KAMAR (BED)" | — | Dari kunjungan | Dari penempatan aktif; dibekukan saat ditandatangani |
| Penerimaan Panduan Rawat Inap | Sudah Menerima Panduan Rawat Inap? | Ya, Sudah Menerima; Tidak/Belum Menerima | Ya | — | — |
| | Keterangan | Teks | Tidak | — | — |
| Tanda Tangan | Tanda tangan yang bertanda tangan | Goresan | Ya (tambah) | — | Mode kertas/digital (`OD-RWA-02`) |
| | Tanda tangan Kepala Ruangan | Gambar TTD pengguna yang masuk | — | Pengguna yang masuk | Atestasi pemegang `SignAsHeadNurse` (`OD-RWA-13`) |
| Cetakan | Data Pasien: nama, No. RM, tanggal lahir, telepon, alamat; pernyataan dwibahasa; "Petugas Administrasi" dan "Yang Bertanda Tangan"; kota, tanggal | — | — | Nama petugas = pengguna yang mencetak | Nama petugas = penanda tangan; kota dan kode dari pengaturan |
| Tambahan dari keputusan Final | Penerima informasi pasien (nama, hubungan) | Teks | Ya | — (V1 tidak punya) | Isi minimal `RWI-DEC-035`; dipakai juga oleh surat 12 butir dan IPD |

### A.2 Serah Terima Pasien — "CEKLIST SERAH TERIMA PASIEN BARU", kode V1 `HP/ADM/001/Rev01/2024`

Sumber V1: `handover-pasien/form-handover-pasien.jsx:693-1305`; `print-handover-pasien.jsx:196, 343`; model `HandoverPasien.cs`, `HandoverPasienDetail.cs`, `ChecklistTemplate.cs`, `ChecklistItem.cs`; tangkapan layar `094356`, `094424`, `094432`.

Data awal butir (dari master V1 "Serah Terima Pasien", urutan apa adanya):

| No | Uraian |
|---:|---|
| 1 | SURAT PENGANTAR RAWAT |
| 2 | MENGHUBUNGI DOKTER (KHUSUS TINDAKAN) |
| • | HASIL PEMERIKSAAN PENUNJANG — Laboratorium |
| • | HASIL PEMERIKSAAN PENUNJANG — Radiologi |
| • | HASIL PEMERIKSAAN PENUNJANG — Lain-lain |
| 3 | PENJELASAN DILARANG MEMBAWA OBAT DARI LUAR |
| 4 | PENJELASAN PRAKIRAAN BIAYA TINDAKAN & DEPOSIT |
| 5 | PENJELASAN HARGA KAMAR |
| 6 | PENJELASAN TATA TERTIB |
| 7 | PERNYATAAN PELUNASAN DEPOSIT |
| 8 | DOKUMEN MEDIK RI / RJ |
| 9 | FORMULIR IPD |
| 10 | FORMULIR ASURANSI / JAMINAN |
| 11 | INFORMASI VIP (KHUSUS VIP) |
| 12 | CETAK LABEL / STIKER / GENERAL CONSENT (V1 tertulis "GENERAL CONCERN") |
| 13 | PASANG GELANG |
| 14 | INPUT PARKIR |
| 15 | LAIN-LAIN |

| Isian | Tipe | Wajib | Perubahan Final |
|---|---|:---:|---|
| Per butir: Sudah / Belum | Pilihan saling meniadakan | V1 tidak; Final ya (`OD-RWA-12`) | Saran sistem untuk butir 4, 7, 9, 12, 13 |
| Per butir: Keterangan | Teks | Wajib bila Belum (usulan) | — |
| Catatan | Teks panjang | Tidak | — |
| Tanda Tangan Admission | Gambar TTD master | Ya | Atestasi `Sign` |
| Tanda Tangan CRO (*Customers Relation Officer*) | Gambar TTD master, lewat tombol "Tandatangan CRO" | Ya | Atestasi `SignAsCro` |
| Tanda Tangan Perawat | Gambar TTD master, lewat tombol "Tandatangan Perawat" | Ya | Atestasi `SignAsNurse` |

### A.3 Assessment Edukasi Pasien — **tidak dibawa ke Workspace Admisi** (`DEC-RWA-003`, v`0.2`)

Menu ini ada di V1 (`Rawat-Inap/perawat/.../assestmen-edukasi/add-assesment-edukasi.jsx`, `tabs-assesment.jsx@86408f245`; tangkapan layar `094448`, `094503`), tetapi pemilik produk memutuskan pada 7 Oktober 2026 bahwa **Assessment Edukasi Pasien tidak diperlukan di admisi**. Asesmen edukasi tetap dikerjakan di Workspace Keperawatan → Pengkajian Pasien → Asesmen Edukasi, memakai instrumen `EDUCATION_ASSESSMENT` yang isiannya sudah diselaraskan dengan V1 (laporan `keperawatan/task/report/backend/BE-RWI-130-asesmen-edukasi-seeder-alignment.md`). Nomor lampiran dipertahankan supaya rujukan A.4 s.d. A.12 tidak bergeser.

### A.4 Gelang & Label Pasien

Sumber V1: `gelang-label-pasien.jsx:114-344, 346-634`; tangkapan layar `094518`.

| Cetakan | Isi V1 | Perubahan Final |
|---|---|---|
| Label Gelang Dewasa | QR; "NAMA, Tn./Ny."; "tgl lahir (umur th)"; No. RM | QR berisi pengenal; sapaan menurut `OD-RWA-16` |
| Label Gelang Bayi (umur ≤ 5 th) | QR; "NAMA BY. NY.,"; "tgl lahir (umur bln/hari)"; No. RM; + dua label kecil | Batas umur dapat diatur; penanda bayi baru lahir dari master pasien |
| Label Pasien | QR; "NAMA / kode RS"; tgl lahir pendek; JK / umur; No. RM; No. Kartu | Kode RS dari pengaturan; No. Kartu dari penjamin kunjungan, kosong bila tidak ada |
| Tombol | "Cetak (Gelang)" / "Cetak (Label)" | Log cetak; cetak ulang beralasan |

### A.5 Permintaan Privasi — "FORMULIR PERMINTAAN PRIVASI / FORMS OF PRIVACY APPLICATION", kode V1 `008/NM/E/Rev02/VI/2022`

Sumber V1: `permintaan-privasi/permintaan-privasi-pasien.jsx:87-110, 301-461, 525-781`; model `PermintaanPrivasi.cs`; tangkapan layar `094543`.

| Bagian | Isian | Tipe | Wajib | Perubahan Final |
|---|---|---|:---:|---|
| Data Pasien | Pasien, No. Rekam Medis, Tanggal Lahir | Hanya-baca | — | — |
| | Kerabat yang Diperbolehkan Menjenguk | 3 baris teks | Tidak | Disimpan per baris |
| | Permintaan Khusus untuk Pelayanan | 3 baris teks | Tidak | Disimpan per baris |
| | Privasi selama Transportasi | Ya / Tidak | Ya (bawaan Tidak) | — |
| Tanda Tangan & Detail | Kota | Teks (V1 bawaan "Jakarta") | Ya | Bawaan dari pengaturan |
| | Tanggal | Tanggal (bawaan hari ini) | Ya | — |
| | Nama Penanda Tangan | Teks | Ya | **Disimpan** (V1 hilang) |
| | Tanda Tangan Pasien/Keluarga | Goresan | Ya | Mode kertas/digital |
| | Tanda Tangan Kepala Ruangan | Nama + gambar TTD pengguna yang masuk | — | Atestasi `SignAsHeadNurse` |
| | Keterangan | Teks | Tidak | — |
| Tombol | Simpan, Cetak, Batal; English / Indonesia | — | — | Cetak final hanya setelah `Completed` |
| Cetakan | Bagian I kerabat, II permintaan khusus, III privasi transportasi; "Pasien / Keluarga Pasien" dan "Kepala Ruangan" | — | — | — |

### A.6 IPD — "DATA DASAR RAWAT INAP/ODC" ("Dibaca dan dimengerti sebelum ditanda-tangani")

Sumber V1: `ipd-surat-pengantar.jsx:139-743`; tangkapan layar `094603`.

| Kolom | Isian V1 | Sumber di Final |
|---|---|---|
| Kiri | Pasien / Patient | `MstPatient.FullName` |
| | Rekam Medis No. | `MstPatient.MedicalRecordNumber` |
| | Nama sendiri / Own name | `MstPatient.NickName` (usulan; `OD-RWA-15`) |
| | Nama Lengkap | `MstPatient.FullName` |
| | KTP / SIM / Paspor | `MstPatient.IdentityType` + `IdentityNumber` |
| | Tanggal Lahir | `MstPatient.BirthDate` |
| | Alamat di KTP; RT/RW; Kelurahan; Kecamatan; Kota; Kode Pos | `Address`, `DistrictId`, `CityId`, `PostalCodeId`; **RT/RW dan kelurahan tidak ada** |
| | Alamat Sekarang; RT/RW; Kelurahan; Kecamatan; Kota; Kode Pos | **Alamat domisili terpisah tidak ada** — garis kosong |
| | Email | `MstPatient.Email` |
| | Pekerjaan | **Tidak ada** — garis kosong |
| | Alamat Kantor/Tel.; Kota; Kode Pos | **Tidak ada** — garis kosong |
| | Warga Negara; Agama | Kewarganegaraan **tidak ada**; Agama `MstPatient.Religion` |
| | No. Mutasi | **Tidak ada** — garis kosong |
| | Tanggal / Jam | `InpEpisode.AdmittedAt` |
| | Kamar rawat No.; Kelas | Penempatan aktif; `InpEpisode.PatientClassId` |
| | Dokter perujuk | Permintaan admisi / rujukan kunjungan asal |
| | Dokter yang merawat | DPJP aktif episode |
| | Orang yang diberi kewenangan mendapatkan informasi | Penerima informasi pada General Consent |
| Kanan | Penjamin / Guarantor | `RegPatientEncounterGuarantor.PaymentSourceNameSnapshot` |
| | Perorangan; Perusahaan; Asuransi | Jenis penjamin dan nama penjamin perusahaan/asuransi |
| | No. Peserta; No. Polis | `MemberNumberSnapshot`; `PolicyNumberSnapshot` |
| | Perorangan (penanggung); KTP/SIM/Paspor; Alamat Lengkap; Alamat di Jakarta; Telp/Hp/Faks (2 baris) | Penanggung jawab dari relasi/kontak darurat (`IsResponsiblePerson`); "Alamat di Jakarta" ditampilkan sebagai "Alamat domisili" (`OD-RWA-15`) |
| | Diagnosa masuk & rencana tindak medis | Diagnosis masuk dari permintaan admisi / episode |
| | Rencana Kelas; Rencana @ Kamar (Rp) | Kelas episode; tarif kamar kelas itu (hanya bagi `ReadAmount`) |
| | Metode Pembayaran | `RegPatientEncounter.PaymentType` |
| | Persetujuan Direktur | **Tidak ada** — garis kosong |
| | Kasir; Administrasi Rawat Inap; Perawat Lantai | Kasir: garis kosong; Administrasi: petugas admisi yang mengunci admisi; Perawat lantai: perawat penanggung jawab aktif |
| | Tanggal Pindah Ruangan; Lantai / Kelas / Nomor | Riwayat penempatan |
| | Perhatian Khusus | **Tidak ada** — garis kosong (usulan: kebutuhan isolasi bila ada) |
| | Nilai Kepercayaan | Dokumen Nilai Kepercayaan `Completed` |
| | Permintaan Privat | Dokumen Permintaan Privasi `Completed` |
| Kaki | "quilvian-mmchospital"; No. Surat; tanggal | Kode RS dari pengaturan; nomor episode; tanggal cetak |

### A.7 Pelunasan Deposit — "PERNYATAAN KESEDIAAN MELUNASKAN DEPOSIT / STATEMENT OF WILLINGNESS TO SETTLE DEPOSIT", kode V1 `005/NM/E/Rev01/XI/2016`

Sumber V1: `pelunasan-deposit/pelunasan-deposit-pasien.jsx:150-284, 536-741, 1090-1347`; tangkapan layar `094620`.

| Bagian | Isian | Tipe | Wajib | Perubahan Final |
|---|---|---|:---:|---|
| Data Wali | Sumber Data | Kontak Darurat / Wali 2 / Wali 3 | — | Daftar kontak darurat dan relasi pasien + manual (`FR-RWA-082`) |
| | Ambil Data Wali, Reset | Tombol | — | — |
| | Nama; Alamat; Telp (angka, maks. 13 digit) | Teks | Ya | — |
| Form Pernyataan | Pasien; No Rekam Medis; Kelas – Kamar | Hanya-baca | — | — |
| | Kekurangan pembayaran deposit; Perhitungan | Angka hanya-baca | — | Dari Billing (`FR-RWA-080`) |
| | Tanggal Jatuh Tempo (teks cetak "Pk. 11.00 WIB") | Tanggal | Ya | Bawaan hari kerja berikutnya, batas kebijakan (`FR-RWA-083`) |
| | Tanda Tangan yang Menyatakan (nama) | Teks (bawaan nama wali) | Ya | — |
| | Tanda Tangan yang Menyatakan | Goresan | Ya | Mode kertas/digital |
| | Tanda Tangan Petugas | Nama + gambar TTD pengguna yang masuk | — | Atestasi petugas |
| | Kota; Tanggal Surat | Teks; tanggal | Ya | Kota dari pengaturan |
| Tombol | Cetak saja; English / Indonesia | — | — | **Simpan** + Cetak; cetak final setelah `Completed` |
| Cetakan | Pernyataan sebagai penanggung biaya; kekurangan Rp …; janji melunasi pada hari kerja pertama tanggal … pukul 11.00 WIB; "pasien dapat diturunkan ke ruang yang sesuai dengan deposit"; pernyataan sadar tanpa paksaan; "Yang menyatakan" dan "Yang menyetujui" | — | — | Teks dipertahankan |

### A.8 Estimasi Biaya

Sumber V1: `rekap-pembayaran-pasien/estimasi-tabs.jsx:21-55`, `estimasi-biaya-rekap-pasien.jsx:141-323`, `estimasi-biaya-rinci/index.jsx:161-494`; tangkapan layar `094629`.

**Tab Rekap — "PENJELASAN PRAKIRAAN BIAYA TINDAK MEDIK"** (`EPIC-RWA-09`):

| Bagian | Isi V1 |
|---|---|
| Pembuka | "Dengan ini kami menyatakan bahwa telah menerima penjelasan tentang prakiraan biaya tindakan dari petugas PPRI / Admission, kepada:" |
| Data | Nama pasien, Nomor MR, Jenis tindakan, Jadwal tindakan, Dokter, Ruang rawat |
| Biaya | Prakiraan biaya tindakan operasi; Prakiraan biaya tindakan rawat inap; Prakiraan harga alat/alkes khusus; Prakiraan Lama Rawat → Biaya Kamar Rawat (per hari), Biaya Visit Dokter (per dokter per hari); Prakiraan biaya lain-lain |
| Catatan | Nilai hanya prakiraan (bukan paket), tergantung obat, penunjang, kesulitan, konsultasi, ICU; operasi cito +25% honor dokter dan ruang operasi; operasi/tindakan > 4 jam +25% per jam ruang operasi/cathlab; honor dokter *standby* 20% dari honor operator; biaya admin 7% dari tagihan, maksimal Rp 6.000.000; tindakan di CCVC/Cath Lab dengan anestesi: jasa anestesi 50% dari operator |
| Tanda tangan | Pasien / keluarga pasien (nama jelas); Petugas PPRI / Admission (nama jelas) |

Perubahan Final: isian rencana (tindakan, jadwal, dokter, lama rawat, kelas) diisi petugas; angka dari tarif; catatan dari pengaturan; dokumen tersimpan dan bertanda tangan.

**Tab Rinci — "PRAKIRAAN BIAYA"** (ditunda, `CAP-RWA-10`): header (nama, MRN, asuransi, tindakan/rencana, kamar perawatan, DPJP); kelompok Biaya Operasi, Rawat Inap, Kamar Rawat (segmen masuk–keluar, hari, harga per hari × hari), Lain-lain, Tindakan, Obat, Laboratorium, Radiologi, masing-masing dengan subtotal dwibahasa; total; keterangan perubahan tarif; kontak admisi/billing; tanda tangan pasien/keluarga dan Admission/Billing.

### A.9 Selisih Biaya — "SURAT PERNYATAAN KESEDIAAN PASIEN MENANGGUNG BIAYA SENDIRI DI RAWAT INAP", kode V1 `006/NM/E/Rev03/VI/2022`

Sumber V1: `selisih-biaya/selisih-biaya-pasien.jsx:139-289, 293-598, 600-930`; model `SelisihBiaya.cs`; tangkapan layar `094640`.

| Bagian | Isian | Tipe / pilihan | Wajib | Perubahan Final |
|---|---|---|:---:|---|
| Data Pasien | Nama Pasien; Alamat Pasien; No. RM; Kelas | V1 dapat diketik | — | **Hanya-baca** dari master/episode |
| Subjek Pernyataan | Diri saya sendiri; Istri saya; Suami saya; Anak saya; Saudara kandung lainnya (+ keterangan) | Radio | Ya | Disimpan sebagai kode |
| Data Deklarer | Nama; Alamat; Pekerjaan; Tipe ID (KTP, SIM, PASPOR, ID CARD); No. ID; No. Handphone (maks. 13 digit); Telepon Kantor; Kota; Tanggal; Nama Penanda Tangan | Teks/pilihan/tanggal | Nama, Alamat, Tipe ID, No. ID, Tanggal wajib (usulan) | "Diri saya sendiri" mengisi dari master pasien |
| Tanda tangan | Tanda Tangan Deklarer | Goresan | Ya | Mode kertas/digital |
| | Petugas PPRI | Nama + gambar TTD pengguna yang masuk | — | Atestasi petugas |
| | Keterangan | Teks | Tidak | — |
| Cetakan | Pernyataan bersedia: (1) membayar bila pihak ketiga tidak menanggung; (2) bila penjamin membatalkan/menanggung sebagian karena masa tunggu, diagnosa, plafon, pengecualian penyakit; (3) membayar selisih akibat pindah kelas lebih tinggi; pernyataan tidak dapat dicabut tanpa persetujuan tertulis manajemen RS; "Mengetahui, Petugas PPRI" dan "Yang Membuat Pernyataan" | — | — | Nama RS dari profil |

### A.10 MP Benefit — "Benefit Makanan Pendamping" (ditunda, `CAP-RWA-12`)

Sumber V1: `mp-benefit-surat-pengantar.jsx:30-189`; tangkapan layar `094647`. Isi kartu: logo dan nama RS; pita judul "Benefit Makanan Pendamping"; QR; Nama Pasien; No. Reg; Asuransi; No. Kartu; No. Kamar; Benefit (Rp); tombol Cetak. Dikirim hanya setelah `OD-RWA-09`, tanpa nomor kartu acak dan tanpa nilai bawaan.

### A.11 Nilai Kepercayaan — "FORMULIR IDENTIFIKASI NILAI-NILAI DAN KEPERCAYAAN PASIEN", kode V1 `009/NM/E/Rev01/VI/2022`

Sumber V1: `nilai-kepercayaan/form-nilai-kepercayaan.jsx:126-137, 380-530, 640-960`; `print-nilai-kepercayaan.jsx:123-418`; model `NilaiKepercayaan.cs`; tangkapan layar `094701`.

| Bagian | Isian | Tipe / pilihan | Wajib | Perubahan Final |
|---|---|---|:---:|---|
| Yang Bertanda Tangan | Nama Lengkap | Teks | Ya | — |
| | Tanggal Lahir | Tanggal | Tidak | — |
| | Umur | Otomatis dari tanggal lahir ("x tahun y bulan z hari") | Ya | — |
| | Jenis Kelamin | Laki-laki / Perempuan | Ya | — |
| | Hubungan dengan Pasien | Teks (contoh: Orang Tua, Suami) | Ya | — |
| | Alamat | Teks | Ya | — |
| Data Pasien | Nama, No. Rekam Medis, Tanggal Lahir, Umur, Jenis Kelamin | Hanya-baca | — | — |
| | "Generate Label Pasien" | Gambar tangkapan layar diunggah | Ya (V1) | Diganti blok identitas dari data (`FR-RWA-113`) |
| | Agama / Kepercayaan | Hanya-baca dari master | — | — |
| Hal-hal yang bertentangan | Butir 1–5 | Teks per butir | Minimal 1 | Disimpan per baris |
| Tanda tangan | Tanda Tangan Penanda Tangan | Goresan | Ya | Mode kertas/digital |
| Cetakan | Pernyataan agar RS tidak melakukan tindakan yang bertentangan dengan nilai dan kepercayaan; daftar butir; kota, tanggal; tanda tangan | — | — | Kota dan kode dari pengaturan |

### A.12 Nilai awal pengaturan cetak (dari V1)

| Dokumen | Kode formulir V1 | Kota bawaan V1 |
|---|---|---|
| General Consent | `GC/ADM/001/Rev01/2024` | Jakarta |
| Serah Terima Pasien Baru | `HP/ADM/001/Rev01/2024` | — |
| Pelunasan Deposit | `005/NM/E/Rev01/XI/2016` | Jakarta |
| Selisih Biaya | `006/NM/E/Rev03/VI/2022` | Jakarta |
| Permintaan Privasi | `008/NM/E/Rev02/VI/2022` | Jakarta |
| Nilai Kepercayaan | `009/NM/E/Rev01/VI/2022` | Jakarta |
| Estimasi Biaya, IPD, Gelang, Label, MP Benefit | Tidak ada di V1 | Jakarta (Estimasi) |

---

## Lampiran B — Bukti source

Format: `repository/path:baris@SHA`. Frontend V1 `@86408f245`, backend V1 `@4be1499cc`, frontend Final `@1f889d67c`, backend Final `@671191eb1`.

| Klaim | Bukti |
|---|---|
| Halaman dan sebelas menu V1 | `QuilvianSystemFrontendDev/src/components/view/Admisi/list-pasien-admisi/dokumen-pasien-ranap/detail-dokumen-tabs.jsx:130-247@86408f245`; `src/app/Admisi/list-pasien/dokumen-pasien-ranap/[slug]/page.jsx:66-76@86408f245` |
| Jalan masuk V1 dari daftar pasien | `src/components/view/Admisi/list-pasien-admisi/list-pasien-ranap.jsx:283-287, 489-491@86408f245` |
| Model V1 | `QuilvianSystemBackendDev/Areas/ManajemenKesehatan/RawatInap/Models/{GeneralConsent,HandoverPasien,HandoverPasienDetail,PermintaanPrivasi,SelisihBiaya,NilaiKepercayaan,ChecklistTemplate,ChecklistItem}.cs@4be1499cc` |
| Tombol Detail Episode Final | `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx:609-711@1f889d67c` |
| Template Workspace Keperawatan | `.../inpatient-management/nursing-workspace/nursing-workspace-view.jsx:73-401@1f889d67c`; `src/components/ui/clinical-workspace/index.js@1f889d67c` |
| Route ruang kerja keperawatan | `src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js:8@1f889d67c` |
| Asesmen Edukasi Final (tetap di keperawatan; tidak dipakai Workspace Admisi sejak v`0.2`) | `inpatient-nursing-constants.js:54@1f889d67c`; `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs:634-728@671191eb1` |
| Tabel dan endpoint persetujuan | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientConsent.cs:13-321@671191eb1`; `Enums/PatientConsentType.cs@671191eb1`; `Controllers/PatientConsentController.cs:25, 35, 295-299, 595-599, 780-784@671191eb1` |
| Cetak persetujuan tanpa simpan | `src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx:30-119@1f889d67c`; `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx:814-957@1f889d67c`; decision log `RWI-DEC-077` |
| Persetujuan umum belum final | `docs/module-blueprints/rawat-inap/00-interview-decisions.md` `RWI-RULE-025`, `RWI-DEC-035`; `episode-rawat-inap/04-prd-to-mvp.md:290` (`RWI-CAP-031`) |
| Daftar periksa penutupan | `00-interview-decisions.md` `RWI-RULE-018`; `Areas/HealthServices/MasterData/Models/MstInpatientClearanceItem.cs@671191eb1`; `Controllers/InpatientClearanceItemController.cs:36-290@671191eb1` |
| Kuota menu penuh | `docs/module-blueprints/rawat-inap/02-module-map.md:239, 546` |
| Ringkasan deposit Billing | `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs:14-17, 93-132@671191eb1`; `Dtos/EpisodeDepositSummaryDtos.cs:3-24@671191eb1`; `MasterData/Models/MstDepositPolicy.cs@671191eb1` |
| Perkiraan harga penunjang | `Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs:12-33@671191eb1` |
| Estimasi otomatis ditunda | `docs/module-blueprints/rawat-inap/episode-rawat-inap/04-prd-to-mvp.md:295` |
| Data pasien, relasi, kontak darurat | `Areas/HealthServices/PatientManagement/MasterData/Models/{MstPatient,MstPatientRelationship,MstPatientEmergencyContact}.cs@671191eb1` |
| Penjamin kunjungan | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs@671191eb1` |
| Aturan tanggungan asuransi | `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs@671191eb1` |
| Profil rumah sakit | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs@671191eb1` |
| Identitas RS ditanam di cetak Final | `src/components/features/surat-component/kop-surat.jsx:4-6@1f889d67c`; `inpatient-admission-flow-constants.jsx:936-939@1f889d67c` |
| Master tanda tangan petugas tidak ada | `src/components/view/settings/user/user-settings-client.jsx:233@1f889d67c`; `Models/ApplicationUser.cs@671191eb1` |
| Mesin keutuhan dokumen | `Areas/HealthServices/MedicalRecordManagement/Models/MrcClinicalDocumentIntegrity.cs:25-103@671191eb1`; `Enums/ClinicalDocumentKind.cs:11-34@671191eb1` |
| Status episode | `Areas/HealthServices/InPatientManagement/Enums/InpEpisodeStatus.cs:4-11@671191eb1` |
| Pola permission Rawat Inap | `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs:42-52, 79@671191eb1` |
| SignalR tersedia | `NewQuilvianSystemBackend/Program.cs@671191eb1`; `src/lib/signalr/signalrHubClient.jsx@1f889d67c` |
| Bukti kelemahan V1 | Kolom "Bukti" tabel 3.4 |

---

## Lampiran C — Riwayat dokumen

| Versi | Tanggal | Perubahan | Oleh |
|---|---|---|---|
| `0.1` | 7 Oktober 2026 | Draf pertama dari analisis 14 tangkapan layar V1, source V1, dan source Final. Belum ditinjau pemilik | Disusun agent atas permintaan pemilik |
| `0.2` | 7 Oktober 2026 | Assessment Edukasi Pasien dicabut dari Workspace Admisi (`DEC-RWA-003`): `CAP-RWA-04`, `EPIC-RWA-04`, `FR-RWA-040`, `FR-RWA-041`, `UAT-RWA-08`, `UAT-RWA-09` ditandai Dibatalkan; menu menjadi sepuluh (sembilan tampil di MVP); gelombang `RWA-MVP-1`, skema tampilan, matriks kewenangan, arsitektur, dan DoD disesuaikan. Rincian bagian 1.3 | Keputusan pemilik produk; disunting agent |
