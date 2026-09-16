# Rawat Inap — Blueprint Manifest

Manifest **tingkat modul**. Pada bentuk `COMPOSITE` berkas ini memegang identitas modul, snapshot
SHA, hash masukan hulu, dan **registry sub-modul**. Status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency masing-masing sub-modul dipegang manifest **di dalam**
sub-modul itu sendiri.

| Field | Value |
|---|---|
| `blueprint_id` | `RWI-BP-001` |
| `revision` | **`7`** — naik 2026-09-15 menyerap penyelarasan `PRD-RWI-V2-001` fase `RLN-PH-06`: `dokter-rawat-inap` kontrak `0.6.0`, `keperawatan` kontrak `0.5.0`, `episode-rawat-inap` amandemen terbatas kontrak `0.9.0`, `02-module-map.md` revision `2`. **Revisi material:** tabel baru di `ClinicalManagement` dan `PharmacyManagement`, perubahan perilaku penutupan episode, pesanan tindakan, dan template resep. Sebelumnya: `6` — naik 2026-09-11 menyerap `Gelombang 1A`, keputusan `RWI-DEC-097` s.d. `RWI-DEC-104`. **Revisi material:** `RWI-DEC-101` mencabut sebagian aturan keras `RWI-RULE-012`, dan `RWI-DEC-099` menambah satu kolom beserta perubahan filter index unik. Lihat bagian 0-A |
| `blueprint_shape` | **`COMPOSITE`** — ditetapkan `RWI-DEC-082` 2026-09-02 |
| `shape_decided_by` | **`USER_CONFIRMED`** — Muhammad Hamzah; agent menyarankan, pemilik memutuskan |
| `status` | **`draft`** — **diturunkan, bukan ditulis tangan.** Ketiga sub-modul `draft` pada revision `7` (bagian 1). Isi revision `6` yang tidak diubah tetap `approved` dan task `✅` tetap sah; task baru hanya dari revision `7` setelah disetujui. Sebelumnya: **`approved`** — **diturunkan, bukan ditulis tangan.** Ketiga sub-modul `approved` pada revision `6`, disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105`. Sebelumnya `partial` selama amandemen `Gelombang 1A` berstatus `draft`. Lihat bagian 1.1 dan 0-A |
| `module` | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| `registry_lifecycle` | `ACTIVE` — dinaikkan dari `PLANNED` 2026-08-24 lewat `RWI-DEC-068`. Wewenang eksekusi database dan deployment tetap terpisah |
| `design_snapshot_at` | `2026-09-15` untuk revision `7`; `2026-09-11` untuk revision `6`; `2026-09-02` untuk revision `5`; `2026-08-24` untuk revision `4`; `2026-08-21` untuk revision `3` |
| `backend_commit_sha` | **`df3679c0d5b2f08106702153eb242d3a6cb2929b`** untuk revision `7` (`01-existing-capability-map.md` revision `1.4` bagian 17). Sebelumnya `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| `frontend_commit_sha` | **`1ce219b40f8e411f3c4e66975626ab33ae81616a`** untuk revision `7`. Sebelumnya `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| `last_focused_impact_scan` | `2026-09-02`, hanya slice `dokter-rawat-inap`; backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`, frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `focused_scan_result` | Capability map `CURRENT` untuk slice dokter. `02-module-map.md` bagian dokter dan seluruh artefak `dokter-rawat-inap/` sudah **diamendemen 2026-09-02** dan kini `CURRENT` terhadap `BE@93b3227` serta `FE@863f24b` |
| `last_focused_requirement_gate` | `2026-09-02`, revision `1.3`; seluruh tujuh capability Dokter Rawat Inap `READY_FOR_DOMAIN_DESIGN`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085`. Overall modul tetap `PARTIALLY_READY` karena slice lain tidak dinilai ulang |
| `baseline_requirement` | **`PRD-RWI-FINAL-001` v1.0.0** — `docs/Modul-RS/Rawat-Inap/PRD_Final_Rawat_Inap_100_Persen.md`. **Menggantikan batas scope revision `4`** lewat `RWI-DEC-080`. Scope modul menjadi **28 kemampuan** `CAP-001` s.d. `CAP-028`. **Peringatan 2026-09-14:** berkas ini **terhapus dari working tree** dan penghapusannya belum di-commit; isinya masih terbaca dari `HEAD` `4f79e998`. Temuan `RLN-01` bagian 0-B |
| `upstream_input_v2` | **`PRD-to-MVP-Rawat-Inap-V2` v`1.0.0`** — **berkas terhapus dari working tree per 2026-09-14, belum di-commit; temuan `RLN-01`.** `docs/Modul-RS/Rawat-Inap/PRD-to-MVP-Rawat-Inap-V2.md`, **diterima sebagai masukan hulu 2026-09-11** lewat `RWI-DEC-097`. Ia **belum** menggantikan `baseline_requirement`; penguncian itu menunggu amandemen `design-business-module`. Menuntut empat koreksi P0, mencabut `RWI-DEC-066`, menjawab `RWI-OQ-047`, dan membalikkan status MAR dari luar-MVP menjadi P0/P1 |
| `upstream_input_v2_final` | **`PRD-RWI-V2-001` v`2.0` FINAL** — `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md`, tanggal dokumen 14 September 2026, SHA-256 `2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f`. **Didaftarkan sebagai masukan hulu 2026-09-14** oleh `manage-module-blueprint` atas instruksi pengguna, untuk **penyelarasan ulang** sub-modul `dokter-rawat-inap` dan `keperawatan`, dengan `episode-rawat-inap` sebagai jangkar konteks. **Pendaftaran ini bukan approval desain dan belum mengganti `baseline_requirement`**; keputusan formalnya milik `grill-me`. Berkasnya masih `untracked` di Git. Rincian, temuan, dan rencana fase: bagian 0-B |
| `realignment_phase` | **Diperbarui 2026-09-15 malam: `RLN-PH-06` `DRAFT_WRITTEN` — menunggu approval pemilik.** Amandemen revision `7` ditulis untuk ketiga sub-modul dan `02-module-map.md` revision `2`; fase ini **belum `DONE`** sampai Muhammad Hamzah menyetujui. `RLN-PH-07` tetap `NOT_STARTED`. Sebelumnya: **Diperbarui 2026-09-15: `RLN-PH-04` `DONE`, `RLN-PH-06` `READY`.** Requirement gate revision `1.6` bagian 15 menyatakan seluruh slice penyelarasan `INP-S17` s.d. `INP-S21` `READY_FOR_DOMAIN_DESIGN`, handover shift dan transfusi `DEFERRED`. `RLN-PH-05` tidak diperlukan untuk slice aktif. Sebelumnya: `RLN-PH-01` `DONE` 2026-09-14; `RLN-PH-02` dan `RLN-PH-03` `DONE` 2026-09-15. Lihat bagian 0-B.4 |
| `last_focused_impact_scan_v2` | **`2026-09-11`** — scan terfokus Rawat Inap V2, `01-existing-capability-map.md` bagian 16. Terikat `BE@201de753` dan `FE@7f6b9356`. Menilai **14** kemampuan: 1 `Conflict`, 3 `Repair`, 3 `Extend`, 1 `Reuse with adapter`, dan 6 `Missing` |
| `focused_scan_v2_result` | Keempat temuan `P0` PRD V2 **terbukti**, tetapi **tiga lebih luas** dari yang tertulis: jalur delete ada pada **10** controller bukan 2; penulis klinis tidak ditegakkan pada **8 dari 9** titik panggil resolver bukan 1 controller; aturan gender kamar menyentuh **dua** aturan bukan satu. **Dua kemampuan ternyata sudah tersedia sebagian** dan berstatus `Extend`, bukan `Missing`: bukti consent sudah punya hash, path, nama, ukuran berkas, hubungan penanda tangan, dan riwayat pencabutan; mesin keutuhan dokumen sudah ada dan sudah terpasang pada controller CPPT, hanya tidak dipanggil pada jalur delete. Tiga `Unknown` tercatat sebagai `V2-UNK-01` s.d. `V2-UNK-03` |
| `evidence_staleness` | **Diperbarui 2026-09-14 — `STALE` lagi.** `HEAD` backend kini `4f79e9985940f2e333a9e6ea6e159f1076a18844` (**119** commit sejak `201de753`; **3** menyentuh `Areas/HealthServices/InPatientManagement`, **7** menyentuh `Areas/HealthServices/ClinicalManagement`) dan frontend `147355f505e875148b8416866ada6cf8b2f1ad99` (**1** commit sejak `7f6b9356`). Selain itu artefak `dokter-rawat-inap` dan `keperawatan` kini `STALE_AGAINST_UPSTREAM` terhadap `PRD-RWI-V2-001`, bagian 0-B. Catatan sebelumnya: **`STALE` sejak 2026-09-11**, **sebagian terjawab hari itu juga** oleh scan terfokus V2 di atas untuk scope empat koreksi `P0` dan sembilan kemampuan baru. Scope di luar itu tetap `STALE`. `HEAD` backend kini `201de7535d4ca00fa9ede2395d4fb769f023b9e6` dan frontend `7f6b9356f6349d516570d6d603ca026f2c7f4ec2`, keduanya sama dengan snapshot audit PRD V2. Jarak dari `backend_commit_sha` adalah **517 commit**, dari `impact_scan_source_sha` slice dokter **243 commit**. **Cakupan nyatanya sempit:** hanya **8** commit menyentuh `Areas/HealthServices/InPatientManagement` dan **26** menyentuh `Areas/HealthServices/ClinicalManagement` sejak `5afb54bd`. Karena itu yang dibutuhkan adalah **impact assessment terbatas** lewat `trace-existing-capabilities`, **bukan** perancangan ulang arsitektur |
| `task_id_integrity` | **`RESOLVED` 2026-09-11.** ~~`CONFLICT` — sebelas task ID dipakai dua pekerjaan berbeda~~. `RWI-DEC-103` dieksekusi `plan-module-delivery` pada tanggal itu: sisi `episode-rawat-inap` dinomori ulang menjadi `BE-RWI-070` s.d. `BE-RWI-072` dan `FE-RWI-058` s.d. `FE-RWI-061`; tiga berkas laporan diganti nama; **108 rujukan** diperbarui tanpa satu pun hilang. Empat nomor `BE-RWI-037` s.d. `BE-RWI-040` **tidak** dinomori ulang karena task episode-nya sudah dibatalkan atau dipindah ke Billing; barisnya kini bertanda coretan dan menyebut nomornya milik `dokter-rawat-inap`. Pemeriksaan judul kartu enam roadmap mengembalikan **nol** tabrakan. **Nomor lama dipensiunkan permanen.** ~~ID bebas berikutnya: `BE-RWI-073` dan `FE-RWI-062`~~ — **basi.** Keduanya sudah dipakai `Gelombang 1A` beserta laporannya. **ID bebas berikutnya per 2026-09-14: `BE-RWI-079` dan `FE-RWI-063`**, dibaca dari roadmap dan pohon `task/report/` |
| `owners` | Product/Domain: **Muhammad Hamzah**, ditunjuk `RWI-DEC-061`; jabatan formal belum diisi. Clinical governance: **sebagian terisi** — keputusan isolasi dan jenis kelamin diambil pemilik yang sama lewat `RWI-DEC-064`, cakupan peran selebihnya belum dinyatakan. Security/Privacy: `OPEN`. API dan Frontend authority: sesuai decision log |
| `approved_by` | **Per sub-modul.** `episode-rawat-inap` 2026-08-24, `dokter-rawat-inap` 2026-09-03, `keperawatan` 2026-09-03 — seluruhnya Muhammad Hamzah | Revision `7`: **belum** untuk ketiganya |
| `approved_at` | — lihat registry bagian 1 |
| `requirement_readiness` | `PARTIALLY_READY` |
| `domain_architecture_revision` | `0.2` — amendment Dokter Rawat Inap, 2026-09-02 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` untuk modul. **`DOMAIN_ARCHITECTURE_READY`** untuk scope `dokter-rawat-inap`, yaitu `CAP-015` dan `CAP-020` s.d. `CAP-025`. **Revision `7`:** isi baru `DOMAIN_ARCHITECTURE_NOT_RUN` — gate `1.6` bagian 15.15, kepemilikan ditetapkan `RWI-DEC-117`, `118`, `132`, `147` s.d. `149` |
| `compatibility_impact` | **Revision `7`:** nol tabel baru `InPatientManagement` (tiga tabel diperbarui); **enam belas** tabel baru `keperawatan` (sebelas `ClinicalManagement`, lima `PharmacyManagement`) dan **tujuh** tabel baru `dokter-rawat-inap` (`PharmacyManagement`); kolom baru pada `TrxPatientAssessment`, `TrxPatientVitalSign`, `TrxPatientAllergy`, `TrxPatientIntegratedProgressNote`, `TrxPatientProcedure`, `PhmPrescriptionItem`, `LabOrder`, `RadOrder`. Perubahan perilaku poliklinik: `R7`, `R9`, `K2`. Sebelumnya: **Tiga belas** tabel baru, seluruhnya milik `episode-rawat-inap`. **Nol tabel baru** dari `keperawatan` dan `dokter-rawat-inap` — `RWI-DEC-081` menaruh seluruh tabel dokumentasi klinis pada `ClinicalManagement`. **Nol perubahan kolom pada tabel modul lain oleh task Rawat Inap**; janji itu tetap utuh dan tetap diuji lewat `BE-RWI-003` kriteria 5. `RWI-RULE-029` aturan 2 menuntut kolom `OriginEncounterId` pada `TrxPatientEncounter`, dan kolom itu **dikerjakan modul IGD** lewat `IGD-DEC-075`, bukan blueprint ini — `RWI-DEC-073`. **Dua** perubahan perilaku: `PATCH /beds/{id}/availability`, dan penempatan jalur IGD yang menunggu event `Tiba` milik IGD sesuai `RWI-DEC-072` |

---

## 0-B. Pendaftaran `PRD-RWI-V2-001` v`2.0` — fase penyelarasan ulang ★ 14 September 2026

Bagian ini mencatat **masuknya satu dokumen hulu baru** dan **rencana kerja** untuk menyelaraskan
blueprint dengannya. Isi desain belum berubah sama sekali. Karena itu `revision` tetap `6`.

### 0-B.1 Yang didaftarkan

| Hal | Isi |
|---|---|
| Dokumen | `PRD TO MVP FINAL — RAWAT INAP V2`, ID `PRD-RWI-V2-001`, versi `2.0`, status menurut dokumennya sendiri `FINAL — AUTHORITY FOR BLUEPRINT REALIGNMENT` |
| Letak | `NewQuilvianSystemBackend/docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md` — 2.310 baris, **`untracked`** di Git |
| SHA-256 saat didaftarkan | `2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f` |
| Cakupan | Tiga hal. (1) **Dokter Rawat Inap:** layout wajib **sama persis** dengan ruang kerja Dokter Rawat Jalan V2, dengan delapan tab SOAP, CPPT, Kajian Pasien, Resep, Tindakan, Resume Medis, Visit, dan Penunjang Medis. (2) **Keperawatan:** layout V2 yang ada dipertahankan, tetapi isinya mengikuti kemampuan V1 dalam delapan menu. (3) **Episode** tetap menjadi jangkar konteks |
| Pembanding yang disebut dokumen | Frontend V1 `QuilvianSystemFrontendDev@MHamzah`, frontend V2 `@HamzahV2`, backend V1 `QuilvianSystemBackendDev@QuilvianSta`, backend V2 `NewQuilvianSystemBackend@MHamzah` |
| Didaftarkan oleh | `manage-module-blueprint`, atas instruksi pengguna pada sesi 14 September 2026 |

### 0-B.2 Arti pendaftaran ini — dan apa yang **bukan** artinya

**Artinya:** setiap skill hilir yang menyentuh `dokter-rawat-inap` atau `keperawatan` **wajib
membaca dokumen ini** sebagai masukan hulu. Artefak desain kedua sub-modul itu ditandai
**`STALE_AGAINST_UPSTREAM`**, yaitu masih sah sebagai desain yang disetujui, tetapi diketahui
tertinggal dari arah produk terbaru.

**Bukan artinya:**

| Yang tidak terjadi | Kenapa |
|---|---|
| Approval desain | Permintaan pengguna tidak diperlakukan sebagai approval. Bagian 72 dokumen PRD juga tidak memuat nama pemberi persetujuan |
| `baseline_requirement` diganti | Mengganti baseline berarti memutuskan nasib 28 kemampuan `CAP-001` s.d. `CAP-028`, keputusan `MVP-RWI-D-001` s.d. `012`, dan butir `OPEN-MVP-001` s.d. `010`. PRD baru **tidak menyebut satu pun** dari ketiganya. Itu keputusan pemilik lewat `grill-me`, temuan `RLN-01` |
| `revision` naik | Arsitektur target, kontrak, dan dependency belum berubah. Revision naik saat `design-business-module` benar-benar menyerap PRD ini |
| Status sub-modul turun | Ketiganya tetap `approved`, sehingga status modul yang **diturunkan** tetap `approved`. Status baru turun menjadi `partial` saat amandemen desain mulai ditulis berstatus `draft`, sama seperti pola revision `6` |
| Task yang sudah ✅ batal | Tidak ada task yang dibatalkan |
| Izin menulis source, migration, atau database | Tetap wewenang terpisah per task |

**Contoh supaya jelas.** `FE-RWI-051` Ruang Kerja Keperawatan sudah ✅ di atas kontrak
keperawatan `0.4.0`. Hasil itu **tetap sah**. Yang berubah hanya ini: sebelum task keperawatan
**baru** diturunkan, misalnya untuk menu Pengawasan Harian, blueprint keperawatan wajib diamendemen
dan disetujui lebih dulu mengikuti PRD ini. Task baru tidak boleh diturunkan dari artefak yang
sudah diketahui tertinggal.

### 0-B.3 Temuan yang wajib diselesaikan sebelum amandemen desain

Temuan berikut dibaca dengan **membandingkan dokumen**: PRD baru terhadap artefak blueprint yang
`approved`. Temuan yang menyangkut isi source **belum** dibuktikan di sini dan diarahkan ke
`trace-existing-capabilities`.

| ID | Temuan | Isi PRD baru | Isi blueprint `approved` | Jenis | Diarahkan ke |
|---|---|---|---|---|---|
| `RLN-01` | **Tiga dokumen hulu lama terhapus dari working tree**, belum di-commit | Bagian 0: "dokumen lama yang bertentangan harus direvisi". PRD **tidak** menyatakan dirinya menggantikan `PRD-RWI-FINAL-001` atau `PRD-to-MVP-Rawat-Inap-V2` | `baseline_requirement` dan `upstream_input_v2` menunjuk `PRD_Final_Rawat_Inap_100_Persen.md` dan `PRD-to-MVP-Rawat-Inap-V2.md`. `RWI-DEC-080` dan `RWI-DEC-097` bersumber dari keduanya. Berkas ketiga `04-prd-to-mvp-deposit.md` sudah `SUPERSEDED` sejak 2026-09-08 | Keputusan dokumen | `grill-me`: diganti penuh atau berlapis; di mana berkas lama disimpan |
| `RLN-02` | **Jalan masuk ruang kerja dokter** | Bagian 9 dan 11: panel **Daftar Pasien Rawat Inap** sendiri di kiri, identik dengan Dokter Rawat Jalan. Artinya halaman berdiri sendiri | `dokter-rawat-inap/03-frontend-architecture.md` bagian 2: **nol butir menu**; `FE-DOK-01` layar anak dari Census `FE-INP-01` dan Detail Episode `FE-INP-04`; butir "Dokter → Rawat Inap" **wajib dicabut**. `IA-INP-05` kuota sembilan butir sudah penuh | Konflik keputusan | `grill-me` |
| `RLN-03` | **Susunan tab dokter** | Delapan tab. Resep dan Tindakan **terpisah**. Resume Medis ada **di ruang kerja dokter**, termasuk Resume ODC | `FE-DOK-02` s.d. `FE-DOK-07`. Resep dan Tindakan **satu layar** `FE-DOK-06`. Resume pulang `CAP-026` **milik `episode-rawat-inap`**, layarnya `FE-INP-06` | Konflik keputusan | `grill-me`: tampil di ruang kerja dokter sebagai permukaan saja, atau pindah pemilik |
| `RLN-04` | **Penunjang Medis melebar** | Enam layanan untuk dokter dan perawat: Radiologi, Laboratorium, Gizi, Hemodialisa, Bank Darah, Rehab Medik | `CAP-015` hanya Laboratorium dan Radiologi. Gizi `CAP-027` `DEFERRED` ke `POST-MVP`. **Hemodialisa, Bank Darah, dan Rehab Medik tidak punya `CAP` sama sekali**, sehingga menjadi **kemampuan yatim** menurut `bentuk-blueprint.md` bagian 5 | Gap kemampuan | `grill-me`, lalu `requirement-completeness-gate` |
| `RLN-05` | **Kemampuan resep baru** | Template Resep, Resep Harian, History Resep; minimum sepuluh isian per item | Resep Harian sudah disebut di enam berkas dokter; **Template Resep tidak ada**. Definisi keduanya terbuka pada PRD sendiri: `OD-RWI-006`, `OD-RWI-007` | Gap + keputusan | `grill-me` |
| `RLN-06` | **Menu keperawatan menyentuh kemampuan sub-modul lain** | Delapan menu, termasuk Transfer Pasien, Pemesanan Ruangan Bedah, Tagihan Pasien, Pemakaian Alat, dan Penunjang Medis | Keperawatan **nol butir menu**, enam layar anak `FE-KEP-01` s.d. `FE-KEP-06`. Transfer `CAP-017`, pesan kamar operasi `CAP-018` `DEFERRED`, tagihan `CAP-019` `DEFERRED` **milik `episode-rawat-inap`**. Pemakaian alat `CAP-016` **`DEFERRED` oleh `RWI-DEC-089`**. Penunjang `CAP-015` milik `dokter-rawat-inap` | Konflik keputusan | `grill-me`. Memindahkan kemampuan antar sub-modul adalah keputusan pemilik, bagian 7 butir 3 |
| `RLN-07` | **Tujuh isi Pengkajian Pasien beserta progres** | Kajian Umum sepuluh bagian V1, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Pengawasan Harian (intake, output, balance cairan), Evaluasi Awal, Perencanaan Pulang; progres "5 dari 7 bagian" | `CAP-012` merancang pengkajian awal dan ulang secara umum. Pengawasan Harian dan Evaluasi Awal **tidak disebut** di blueprint keperawatan. Pemilik Evaluasi Awal/MPP terbuka: `OD-RWI-008` | Gap kemampuan + bukti V1 | `trace-existing-capabilities` untuk isian V1, lalu `grill-me` dan `requirement-completeness-gate` |
| `RLN-08` | **Asuhan Keperawatan** | Enam sub-isi: Vital Sign, SOAP Keperawatan "jika digunakan oleh policy RS", Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, Catatan Keperawatan | Obat & Alkes menyentuh MAR yang `Missing` dan dijadikan `P0/P1` oleh `RWI-DEC-097`. SOAP perawat belum pernah diputuskan | Keputusan | `grill-me`: SOAP perawat dipakai atau tidak; `OD-RWI-005` |
| `RLN-09` | **Butir MVP-0 mungkin sudah sebagian selesai** | Bagian 56: semantik finalisasi SOAP, verifikasi DPJP pada CPPT, idempotency, header-item resep, enum dan skor risiko jatuh, pembeda "belum dikaji" dari "tidak ada", dan lain-lain | `RWI-DEC-086` sudah mengunci kapan catatan dokter final. `Gelombang 1A` sudah menurunkan penulis dari pengguna terautentikasi. Dokter 22/22 dan keperawatan 14/14 task backend ✅ | Bukti source | `trace-existing-capabilities`: klasifikasi `Done`/`Repair`/`Missing` per butir |
| `RLN-10` | **Kesamaan layout dengan Dokter Rawat Jalan** | `UI-AC-DOK-001` s.d. `012`: bila teks disamarkan, kedua layout harus terlihat sama | Blueprint dokter mengunci bentuk ruang kerja sendiri di `03-frontend-architecture.md` bagian 0 dan 3.1.1, serta `skema-tampilan-dokter-rawat-inap.md` | Bukti source + konflik | `trace-existing-capabilities` untuk komponen Rawat Jalan yang sudah "FIX"; hasilnya dibawa ke `grill-me` |
| `RLN-11` | **Sepuluh open decision PRD belum punya nomor register** | `OD-RWI-001` s.d. `OD-RWI-010` | Beberapa beririsan dengan butir yang sudah ada: `RWI-RULE-021`, `RWI-DEC-088`, `RWI-DEC-089`, `RWI-DEC-102`, `OPEN-MVP-*` | Register | `grill-me` mendaftarkannya sebagai `RWI-OQ-###` berikutnya beserta alias `OD-RWI-###` |
| `RLN-12` | **Letak berkas dan daftar berkas yang disebut PRD** | Bagian 0 menyebut target `docs/module-blueprints/rawat-inap/04-prd-to-mvp-final.md`. Bagian 69 menyebut `keperawatan/02-module-map.md` dan `api-contract.md` di akar sub-modul | Berkas sebenarnya ada di `docs/Modul-RS/Rawat-Inap/`. `02-module-map.md` hanya ada di tingkat modul. Kontrak ada di `contracts/`. Bagian 69 **tidak menyebut** state-transition, permission-audit, integration, acceptance-test, dan flowcharts | Dokumen | Letak: `grill-me`. Pemetaan ke himpunan berkas canonical: `design-business-module` |
| `RLN-13` | **Nama gelombang bertabrakan** | `MVP-0` s.d. `MVP-4` | Nama gelombang yang sudah dipakai: `DOK-MVP-0`, `DOK-MVP-0b`, `DOK-MVP-FE`, `Gelombang 1A` | Perencanaan | `plan-module-delivery`: petakan tanpa memakai ulang nama |
| `RLN-14` | **Bukti source basi dan satu repository tidak tersedia** | Pembanding V1 backend `QuilvianSystemBackendDev@QuilvianSta` | Repository itu **tidak ada** di workspace lokal. Frontend V1 tersedia sebagai branch lokal `MHamzah`. `HEAD` kedua repository V2 sudah bergerak, field `evidence_staleness` | Bukti source | `trace-existing-capabilities`. Isian V1 backend yang tidak dapat dibaca dicatat `Unknown`, bukan ditebak |

### 0-B.4 Rencana fase penyelarasan

| Fase | Isi | Skill pemilik | Status | Butuh | Menutup temuan |
|---|---|---|---|---|---|
| `RLN-PH-01` | Mendaftarkan PRD sebagai masukan hulu, menandai artefak basi, dan menyusun rencana ini | `manage-module-blueprint` | **`DONE`** 2026-09-14 | — | — |
| `RLN-PH-02` | Amendment Pass: nasib dokumen hulu lama, jalan masuk dan tab dokter, pemindahan kemampuan antar sub-modul, SOAP perawat, dan pendaftaran `OD-RWI-001` s.d. `010` | `grill-me` | **`DONE`** 2026-09-15 — Amendment Pass `PRD-RWI-V2-001` tuntas, `00-interview-decisions.md` Riwayat Pass | — | `RLN-01`, `02`, `03`, `05`, `06`, `08`, `11`, dan letak berkas `RLN-12` |
| `RLN-PH-03` | Impact scan terfokus: isian V1 keperawatan dan dokter, komponen Dokter Rawat Jalan V2, butir MVP-0, dan selisih SHA | `trace-existing-capabilities` | **`DONE`** 2026-09-15 — `01-existing-capability-map.md` revision `1.4` bagian 17; temuannya ditutup Amendment Pass `RLN-PH-03` | — | `RLN-07` bagian V1, `09`, `10`, `14` |
| `RLN-PH-04` | Requirement gate terfokus untuk kemampuan baru dan kemampuan yatim | `requirement-completeness-gate` | **`DONE`** 2026-09-15 — `evidence/02-requirement-completeness-gate.md` revision `1.5` bagian 14, lalu penutupan keputusan revision `1.6` bagian 15 setelah Amendment Pass penutupan gate. `INP-S17` s.d. `INP-S21` `READY_FOR_DOMAIN_DESIGN`; handover shift dan transfusi `DEFERRED` | `RLN-PH-02` dan `RLN-PH-03` selesai | `RLN-04`, `RLN-07` |
| `RLN-PH-05` | Arsitektur domain, **opsional** | `hospital-domain-architect` | `NOT_STARTED` — **tidak diperlukan** untuk slice aktif menurut gate `1.6` bagian 15.15: kepemilikan data lintas modul sudah ditetapkan `RWI-DEC-147` s.d. `149`. Baru relevan saat transfusi dijadwalkan | Hanya bila `RLN-PH-04` menemukan Evaluasi Awal/MPP, transfer klinis, atau pemesanan kamar operasi melintasi bounded context | — |
| `RLN-PH-06` | Amandemen blueprint `dokter-rawat-inap` dan `keperawatan`, termasuk `02-module-map.md` bagian 3 dan 4; revision naik ke `7`; approval pemilik | `design-business-module` | **`DRAFT_WRITTEN`** 2026-09-15 — revision `7` ditulis, **menunggu approval**; bukan `DONE` | `RLN-PH-04` selesai; `RLN-PH-05` bila dijalankan | Bagian pemetaan `RLN-12` |
| `RLN-PH-07` | Task backend dan frontend untuk MVP-0 s.d. MVP-4, mulai `BE-RWI-079` dan `FE-RWI-063` | `plan-module-delivery` | `NOT_STARTED` | `RLN-PH-06` **disetujui** | `RLN-13` |
| `RLN-PH-08` | Satu task per pemanggilan | `build-module-backend` / `build-module-frontend` | `NOT_STARTED` | Task disetujui + wewenang tulis eksplisit | — |
| `RLN-PH-09` | Kesiapan per gelombang, lalu UAT | `verify-module-readiness` | `NOT_STARTED` | Gelombang selesai | — |

**Yang tetap dapat berjalan tanpa menunggu fase ini:** enam task terbuka `episode-rawat-inap`
pada `Sisa-Pekerjaan-Rawat-Inap.md` bagian 4. Lima di antaranya memang tertahan endpoint Billing,
**bukan** oleh PRD ini.

**Yang tertahan:** penurunan task **baru** untuk `dokter-rawat-inap` dan `keperawatan` sampai
`RLN-PH-06` disetujui. Hari ini kedua sub-modul itu tidak punya task terbuka, jadi tidak ada
pekerjaan berjalan yang berhenti.

### 0-B.5 Handoff untuk dua fase yang `READY`

```yaml
# RLN-PH-02 → grill-me (Amendment Pass)
blueprint_id: RWI-BP-001
blueprint_revision: 6
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
upstream_input: PRD-RWI-V2-001 v2.0 — docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md
backend_source_sha: 4f79e9985940f2e333a9e6ea6e159f1076a18844
frontend_source_sha: 147355f505e875148b8416866ada6cf8b2f1ad99
current_phase: RLN-PH-02
capability_scope: dokter-rawat-inap (CAP-015, CAP-020..025), keperawatan (CAP-012..014, CAP-016, CAP-027),
  episode-rawat-inap hanya permukaan CAP-017, CAP-018, CAP-019, CAP-026
decision_revision: 00-interview-decisions.md revision 18; RWI-DEC terakhir 105; RWI-OQ terakhir 055
blocking_decision_ids: [RLN-01, RLN-02, RLN-03, RLN-05, RLN-06, RLN-08, RLN-11, RLN-12]
contract_versions: {episode-rawat-inap: 0.8.0, dokter-rawat-inap: 0.5.0, keperawatan: 0.4.0}  # RWI-DEC-105
wewenang_tulis: docs/module-blueprints/rawat-inap/00-interview-decisions.md saja
keluaran: keputusan RWI-DEC-106 dst. beserta pemberi approval bernama; RWI-OQ-056 dst. untuk OD-RWI-001..010
```

```yaml
# RLN-PH-03 → trace-existing-capabilities (impact scan terfokus, read-only)
blueprint_id: RWI-BP-001
blueprint_revision: 6
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
backend_source_sha: 4f79e9985940f2e333a9e6ea6e159f1076a18844   # V2 MHamzah
frontend_source_sha: 147355f505e875148b8416866ada6cf8b2f1ad99  # V2 HamzahV2
pembanding_v1: frontend branch lokal MHamzah; backend QuilvianSystemBackendDev@QuilvianSta TIDAK TERSEDIA → Unknown
current_phase: RLN-PH-03
capability_scope: ruang kerja Dokter Rawat Jalan V2 vs Dokter Rawat Inap V2; tujuh isi Pengkajian
  Pasien dan enam sub-isi Asuhan Keperawatan V1 vs V2; butir MVP-0 PRD bagian 56
taksonomi: Ready to reuse | Reuse with adapter | Extend | Repair | Missing | Conflict | Unknown
keluaran: 01-existing-capability-map.md bagian 17; menutup RLN-07 (bagian V1), RLN-09, RLN-10, RLN-14
```

---


## 0-A. Yang diserap revision `6` — Gelombang 1A Rawat Inap Safety Corrections

Revision `6` adalah **revisi material isi desain**, berbeda dari revision `5` yang hanya memindahkan
berkas. Satu aturan keras dicabut sebagian, satu kolom lahir, dan satu index unik berubah filternya.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-097` PRD V2 diterima sebagai masukan hulu | Field `upstream_input_v2` |
| `RWI-DEC-098` jalur hapus ditutup pada dua controller saja | `dokter-rawat-inap/contracts/api-contract.md` `0.5.0` bagian 0.A.1; `keperawatan/contracts/api-contract.md` `0.4.0` bagian 0.A.1 |
| `RWI-DEC-099` peran pada penugasan dokter | `episode-rawat-inap/02-backend-architecture.md` bagian 0.2 s.d. 0.4; `data/data-dictionary.md` bagian 2 dan 2.1; `dokter-rawat-inap/contracts/api-contract.md` bagian 0.A.2 |
| `RWI-DEC-100` kewenangan menulis perawat ditentukan unit | `keperawatan/contracts/api-contract.md` bagian 0.A.2 |
| `RWI-DEC-101` aturan jenis kelamin tingkat kamar dicabut | `episode-rawat-inap/02-backend-architecture.md` bagian 0.1 dan bagian 1; `contracts/api-contract.md` `0.8.0`; `contracts/validation-matrix.md` `0.8.0`; `03-frontend-architecture.md` bagian 4.3A.1 |
| `RWI-DEC-102` kelayakan keuangan tetap `P0` tetapi terblokir Billing | **Sengaja belum diserap ke artefak desain.** Ia menetapkan batas gelombang, bukan bentuk desain. Diserap saat gelombang kelayakan keuangan dibuka |
| `RWI-DEC-103` sisi episode dinomori ulang | **Belum dikerjakan.** Milik `plan-module-delivery`; lihat field `task_id_integrity` |
| `RWI-DEC-104` `DEC-INP-004` ditutup, `INP-S11` naik `READY_FOR_DOMAIN_DESIGN` | `evidence/02-requirement-completeness-gate.md` baris 453, 560, 561, 618, dan 674, dibetulkan dengan coretan |

### 0-A.1 Satu temuan desain yang tidak diminta siapa pun

`RWI-DEC-099` menambah peran pada penugasan dokter. Saat merancangnya ditemukan bahwa index unik
`IX_InpDoctorAssignment_EpisodeId_Active` memfilter hanya `"EndDateTime" IS NULL`, sehingga
**satu episode hanya boleh punya satu penugasan terbuka**. Selama tabel itu hanya menyimpan DPJP,
filter itu benar. Begitu konsulen dan dokter jaga masuk, baris kedua **ditolak database**, dan
keputusan pemilik tidak akan pernah dapat dijalankan.

Penyelesaiannya menyempitkan filter menjadi `"EndDateTime" IS NULL AND "AssignmentRole" = 1`.
`INV-INP-03` berbunyi "tepat satu DPJP aktif", sehingga filter baru justru menegakkan bunyi
invariant **apa adanya**, sedangkan filter lama menegakkan sesuatu yang lebih ketat daripada yang
pernah diminta. Rinciannya beserta urutan migration yang mengikat ada pada
`episode-rawat-inap/data/data-dictionary.md` bagian 2.1.

### 0-A.2 Yang sengaja belum dikerjakan pada revision `6`

| Yang belum | Alasan |
|---|---|
| `04-prd-to-mvp.md` ketiga sub-modul | Kontrak `Gelombang 1A` baru berstatus `draft`. PRD ditulis paling akhir dan menurunkan dari kontrak yang sudah berdiri |
| `contracts/permission-audit-matrix.md`, `state-transition-matrix.md`, `testing/acceptance-test-matrix.md` | Menunggu approval kontrak `0.8.0`, `0.5.0`, dan `0.4.0`. Menaikkannya sekarang berarti menulis matriks di atas kontrak yang masih dapat berubah |
| Sembilan kemampuan `Missing` dan `Extend` PRD V2 | Di luar `Gelombang 1A`. Sebagiannya menunggu sepuluh butir `OPEN-MVP` yang `RWI-DEC-097` **tidak** buka |
| Kelayakan keuangan | `RWI-DEC-102` menetapkannya `P0 — external dependency`, menunggu `BE-BKC-040` |

---
## 0. Yang diserap revision `5` — migrasi bentuk `SINGLE` → `COMPOSITE`

Revision `5` adalah **revisi material struktur**, bukan revisi isi desain. Tidak ada tabel baru,
kolom baru, endpoint baru, aturan baru, maupun kontrak yang naik versi. Yang berubah adalah letak
berkas dan granularitas approval.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-080` `PRD-RWI-FINAL-001` menggantikan batas scope revision `4`; modul menjadi 28 kemampuan | Field `baseline_requirement`; `02-module-map.md` bagian 4; koreksi **enam** keterangan basi pada `episode-rawat-inap/04-prd-to-mvp.md`, tersebar di bagian 7, 8, 14, dan 16 |
| `RWI-DEC-081` `ClinicalManagement` pemilik tabel dokumentasi klinis; Rawat Inap hanya workspace dan kontrak | `02-module-map.md` bagian 2.3; manifest kedua sub-modul baru bagian 2, lengkap dengan larangan membuat tabel tandingan |
| `RWI-DEC-082` `blueprint_shape: COMPOSITE`, tiga sub-modul, `shape_decided_by: USER_CONFIRMED` | Field `blueprint_shape` dan `shape_decided_by`; registry bagian 1; seluruh struktur folder |
| `RWI-DEC-083` pemetaan 28 kemampuan, nol yatim | `02-module-map.md` bagian 4, lengkap dengan pemeriksaan kemampuan yatim bagian 4.4 |
| ~~`RWI-OQ-047` **terbuka**~~ — **DITUTUP 2026-09-11** oleh `RWI-DEC-097` dan `RWI-DEC-102`. Billing menjadi sumber kebenaran kelayakan keuangan | `02-module-map.md` bagian 2.4 dan bagian 6, ditulis **apa adanya** sebagai "belum diputuskan"; `episode-rawat-inap/02-backend-architecture.md` bagian 2 |

### 0.1 Tiga dokumen basi yang diperbaiki

Ketiganya masih menyatakan `DEC-INP-001` terbuka, padahal `RWI-DEC-062` sudah menutupnya
2026-08-21 — dan sejak `RWI-DEC-080` keterangan itu bertabrakan langsung dengan scope baru.

| Dokumen | Yang diperbaiki |
|---|---|
| `episode-rawat-inap/04-prd-to-mvp.md` bagian 7, 8, 14, 16 | **Enam** keterangan pada empat bagian. Dokumentasi klinis keluar dari MVP sub-modul ini **karena berpindah pemilik ke sub-modul lain yang belum dirancang**, bukan karena `DEC-INP-001` |
| `evidence/02-requirement-completeness-gate.md` bagian 4.10 | Dua baris `Belum ada` → **`SUDAH ADA`**, beserta pemberi dan tanggalnya |
| `evidence/02-requirement-completeness-gate.md` bagian 5.2 dan 6 | Butir 1 `MISSING`/`BLOCKING` → `CONFIRMED`/tidak memblokir; blok `DEC-INP-001` diberi kepala **TERTUTUP** |

Mentahnya dipertahankan dengan coretan, bukan dihapus, supaya jejak pertanyaan aslinya tetap
terbaca.

### 0.2 Perubahan struktur

| Gerakan | Yang dilakukan |
|---|---|
| ① Ekstrak | `02-module-map.md` lahir di tingkat modul: registry sub-modul, tabel kepemilikan data seluruh modul, peta butir menu + urutan migration, dan pemetaan kemampuan ke sub-modul |
| ② Pindahkan | Seluruh artefak revision `4` pindah ke `episode-rawat-inap/`, **termasuk `roadmap/` dan `task/`**. `erd/data-dictionary.md` → `data/data-dictionary.md` memenuhi `blueprint-output-contract.md`; 13 rujukan ke path lama diperbaiki |
| ③ Buat | `keperawatan/` dan `dokter-rawat-inap/`, masing-masing manifest + himpunan 11 berkas berisi satu baris alasan bersebab |

### 0.3 Yang diserap revision `4` — Amendment Pass 2026-08-24

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-070` pelonggaran mesin klinis meluas ke `Emergency` | Decision log: `RWI-RULE-026` aturan 3 s.d. 6 |
| `RWI-DEC-071` justifikasi `RWI-DEC-041` ditulis ulang | Decision log: `RWI-RULE-029` |
| `RWI-DEC-072` waktu tiba milik IGD | `episode-rawat-inap/` — `02-backend-architecture.md` §1.7 aturan 9 dan §4.5; `erd/00-context-erd.md`; `data/data-dictionary.md`; keempat kontrak; `testing/` bagian 2 dan 14 |
| `RWI-DEC-073` `OriginEncounterId` dikerjakan IGD | Field `compatibility_impact`; `episode-rawat-inap/02-backend-architecture.md`; `erd/00-context-erd.md` §2; `contracts/integration-contract.md` (`INT-INP-07`) |

Riwayat penyerapan revision `1` s.d. `3` ada pada bagian 10.

### 0.4 Satu artefak hulu yang masih tertinggal

[`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md)
masih revision `0.1` dan belum memuat perubahan Amendment Pass 2026-08-21 maupun 2026-09-02.

| Bagian | Yang perlu diperbarui |
|---|---|
| `INV-INP-01` | Dilonggarkan untuk episode `DischargePending` yang kepergiannya sudah dicatat |
| Invariant baru `INV-INP-10` | Satu pasien satu episode yang benar-benar hadir |
| `CMD-INP-15` | Perintah bisnis baru: catat pasien sudah meninggalkan ruangan |
| `ARCH-GAP-002` s.d. `ARCH-GAP-005` | Keempatnya sudah tertutup `RWI-DEC-054` s.d. `RWI-DEC-057` |
| `CTX-CLI` dan `CTX-PHM` | Keduanya masih "Belum ditentukan, lihat `DEC-INP-001`". Sejak `RWI-DEC-081` pemiliknya **sudah** ditentukan: `ClinicalManagement` |

**Ini sengaja tidak diselesaikan di sini.** Bounded context, batas aggregate, invariant, dan
lifecycle adalah wewenang `/qv-domain`; skill penyusun blueprint dilarang merancangnya ulang.
Selisih ini **tidak memblokir** pemakaian blueprint, karena isi blueprint dan decision log sudah
sejalan.

---

## 1. Registry sub-modul

```yaml
blueprint_shape: COMPOSITE
shape_decided_by: USER_CONFIRMED
submodules:
  - slug: episode-rawat-inap
    prefix: BE-RWI / FE-RWI
    kemampuan: 16
    uji_pemecahan: 5/5
    status: draft            # revision 7 amandemen terbatas, 2026-09-15; sebelumnya approved
    approved_by: Muhammad Hamzah   # untuk revision 6 / kontrak 0.8.0
    approved_at: 2026-08-24
    contract_versions: 0.9.0   # draft; 0.8.0 approved 2026-09-11 lewat RWI-DEC-105
  - slug: keperawatan
    prefix: BE-RWI / FE-RWI
    kemampuan: 5
    uji_pemecahan: 3/5
    status: draft            # revision 7, 2026-09-15; sebelumnya approved
    approved_by: Muhammad Hamzah   # untuk revision 5 / kontrak 0.4.0
    approved_at: 2026-09-03
    contract_versions: 0.5.0   # draft; 0.4.0 approved 2026-09-11
    upstream_realignment: REALIGNED_DRAFT sejak 2026-09-15; sebelumnya STALE_AGAINST_UPSTREAM
    designed_at: 2026-09-02
    amended_at: 2026-09-02
    catatan: dirancang lalu diamandemen menyerap RWI-DEC-089; CAP-016 kini DEFERRED dan RWI-OQ-048
      tertutup. Roadmap DRAFT/FORWARD_TEST sudah ditulis 2026-09-02: 11 task backend dan 6 task
      frontend, seluruhnya BLOCKED menunggu approval blueprint sub-modul ini.
      Butir konsistensi mesin koreksi ditutup RWI-DEC-091. Disetujui RWI-DEC-092 pada 2026-09-03;
      roadmap kini DRAFT_STALE dan wajib ditulis ulang /qv-plan menjadi revision 2 berstatus APPROVED
      sebelum satu pun task dikirim ke builder
  - slug: dokter-rawat-inap
    prefix: BE-RWI / FE-RWI
    kemampuan: 7
    uji_pemecahan: 3/5
    status: draft            # revision 7, 2026-09-15; sebelumnya approved
    approved_by: Muhammad Hamzah   # untuk kontrak 0.5.0
    approved_at: 2026-09-03
    contract_versions: 0.6.0   # draft; 0.5.0 approved 2026-09-11
    upstream_realignment: REALIGNED_DRAFT sejak 2026-09-15; sebelumnya STALE_AGAINST_UPSTREAM
    pending_amendment: revision 7 / kontrak 0.6.0, manifest sub-modul bagian 9
    designed_at: 2026-09-02
    catatan: disetujui 2026-09-03 untuk 13 artefak revision 0.3 / kontrak 0.3.0; domain architecture READY; nol pertanyaan memblokir; approval desain BUKAN izin implementasi, migration, maupun deployment.
      Amendment 0.4.0 pada 2026-09-09 menaikkan tujuh artefak untuk membuka BE-RWI-068 (grup Patient Diagnosis, INT-DOK-10) dan DISETUJUI Muhammad Hamzah hari itu juga.
      Status sub-modul tidak pernah diturunkan: baseline 0.3.0 tidak tersentuh, dan amendment hanya menahan BE-RWI-068 sampai disetujui
```

| Sub-modul | Rumpun kemampuan | Kemampuan | Status | Manifest sub-modul |
|---|---|:---:|---|---|
| [`episode-rawat-inap/`](./episode-rawat-inap/) | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan | 16 | `approved` | [manifest](./episode-rawat-inap/blueprint-manifest.md) |
| [`keperawatan/`](./keperawatan/) | Pengkajian, asuhan, tindakan keperawatan, gizi, pemakaian alat | 5 | **`approved`** — Muhammad Hamzah, 2026-09-03, lewat `RWI-DEC-092`. `CAP-016` `DEFERRED`; roadmap wajib ditulis ulang `/qv-plan` revision `2` | [manifest](./keperawatan/blueprint-manifest.md) |
| [`dokter-rawat-inap/`](./dokter-rawat-inap/) | SOAP, CPPT, kajian medis, resep, tindakan, visite, penunjang | 7 | **`approved`** — revision `0.4`, kontrak **`0.4.0`** disetujui Muhammad Hamzah 2026-09-09; kontrak `0.3.0` disetujui 2026-09-03. Siap `plan-module-delivery` | [manifest](./dokter-rawat-inap/blueprint-manifest.md) |

### 1.1 Cara `status` modul diturunkan

`bentuk-blueprint.md` bagian 7 menyatakan status modul pada bentuk `COMPOSITE` **MUST NOT** ditulis
tangan. Aturannya:

| Keadaan baris registry | Status modul |
|---|---|
| Semua `approved` | `approved` |
| **Campur** | **`partial`** |
| Belum ada yang jalan | `draft` |

Hari ini: **tiga `approved` + nol `draft`** = **`approved`**. Menuliskannya tangan akan membuat modul
dapat terlihat `approved` sementara dua sub-modulnya belum dirancang sama sekali.

### 1.2 Dua sub-modul baru `draft`, bukan `BLOCKED`

`bentuk-blueprint.md` gerakan ③ menyatakan sub-modul yang batas kepemilikan datanya belum diputuskan
lahir `BLOCKED`. Kedua sub-modul baru **tidak** dalam keadaan itu: `RWI-DEC-081` sudah menetapkan
pemilik tabelnya (`ClinicalManagement`), `RWI-DEC-062` sudah memberikan persetujuannya, dan
`RWI-DEC-083` sudah memetakan kemampuannya. Yang tersisa adalah **pekerjaan desain**, ditambah satu
penghalang teknis — *shared inpatient clinical context resolver*, `PRD-RWI-FINAL-001` bagian 30.3.

`BLOCKED` berarti menunggu orang; `draft` berarti menunggu pekerjaan.

---

> ★ **Revision `7` — 15 September 2026.** Kolom Status tabel di atas adalah jejak revision `6`. Yang berlaku: ketiga
> sub-modul **`draft`** — `episode-rawat-inap` kontrak `0.9.0` (manifest bagian 10), `keperawatan` kontrak `0.5.0`
> (manifest bagian 8), `dokter-rawat-inap` kontrak `0.6.0` (manifest bagian 9). Status modul: **`draft`**.

## 2. Daftar artefak tingkat modul dan hash

Hanya empat berkas ini yang hidup di tingkat modul. Artefak desain masing-masing sub-modul dicatat
pada manifest sub-modulnya.

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`00-interview-decisions.md`](./00-interview-decisions.md) | **`21`** | `approved decisions / Amendment Pass RLN-PH-04 tuntas` | `1c55c80a50aee11ef005ccde6315c2935cbe21504e8596798b89bf7f2d45102a` |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | **`1.4`** | `source-audited / impact scan RLN-PH-03 bagian 17` | `337a10f09d6e91b06395405098bad09623452de062e10a720a637bd22daa543a` |
| [`02-module-map.md`](./02-module-map.md) | **`2`** | `draft` — revision `7`: kepemilikan data baru 2.5, urutan migration 3.4.1, peta menu 3.5, pemecahan kemampuan 4.5, registry tiga `draft` | `242ece500fa9c3a4fb6b5e644d2524f9c00d73f8d3da33a14f32b3cb4a5d7c80` |
| [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) | `1.6` | `CURRENT / penutupan keputusan RLN-PH-04, INP-S17 s.d. INP-S21` | `f31d207ae0cac120b0821d4474a3d952e109293c2b517aa630370396e49b5300` |
| [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) | `0.2` | `draft / amendment Dokter Rawat Inap` | `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |

`evidence/02-requirement-completeness-gate.md` naik dari `1.0` ke `1.1` karena tiga keterangan basi
`DEC-INP-001` diperbaiki. Isinya selebihnya tidak disentuh.

**Amandemen 2026-09-02 sore — penyerapan `RWI-DEC-089`.** `00-interview-decisions.md` naik ke revision `11` (Amendment Pass pemakaian alat), dan `02-module-map.md` berubah pada bagian 2.4, 4, serta 6 karena `CAP-016` berpindah dari `OPEN DECISION` menjadi `DEFERRED`. Delapan artefak `keperawatan/` ikut berubah dan `contract_versions` sub-modul itu naik ke `0.2.0`; tiga kontrak yang isinya tidak bergerak tetap `last_changed_in: 0.1.0`. Perubahan ini **tidak** menaikkan `revision` blueprint tingkat modul, karena tidak ada arsitektur target, kontrak lintas modul, atau kepemilikan data yang berubah — yang berubah adalah disposisi satu kemampuan yang sudah punya sub-modul pemilik.

Hash `02-module-map.md` dinormalkan pada validasi lifecycle 2026-09-02 dari nilai metadata lama
`62be6c…` ke SHA-256 isi committed `6be5e5…`. Pada hari yang sama berkasnya **benar-benar berubah**
karena amendment slice dokter, sehingga hashnya kini `d94b70…`. Perubahan itu menyentuh peta butir
menu dan urutan migration — dua hal yang memang hanya boleh hidup di berkas ini — dan tidak
menaikkan revision blueprint tingkat modul.

Hash dipakai mendeteksi perubahan yang tidak tercatat. Bila salah satu berubah tanpa revision naik,
blueprint dianggap tidak konsisten.

---

## 3. Struktur berkas

```text
rawat-inap/                              ◄── TINGKAT MODUL
├── blueprint-manifest.md                     berkas ini
├── 00-interview-decisions.md                 satu wawancara untuk seluruh modul
├── 01-existing-capability-map.md             satu audit untuk seluruh modul
├── 02-module-map.md                          hanya lahir pada COMPOSITE
├── evidence/                                 keluaran skill hulu
│
├── episode-rawat-inap/                  ◄── <blueprint-root> #1  approved
├── keperawatan/                         ◄── <blueprint-root> #2  draft
└── dokter-rawat-inap/                   ◄── <blueprint-root> #3  draft
```

Folder yang memuat `blueprint-manifest.md` adalah sub-modul; `evidence/` bukan.

| Hitungan | Nilai |
|---|---|
| Berkas pasti menurut kontrak | 4 tingkat modul + (11 × 3 sub-modul) = **37** |
| Yang ada hari ini | 4 tingkat modul + 3 manifest sub-modul + 33 himpunan + berkas di luar himpunan canonical |
| Berkas di luar himpunan canonical | `episode-rawat-inap/` — `05-skema-tampilan.md`, `erd/` (3), dua kontrak turunan, `archive/`, `roadmap/`, `task/report/` (77 laporan). Kehadirannya **bukan** penyimpangan struktur |
| Penyimpangan struktur yang diketahui | `episode-rawat-inap/flowcharts/` belum ada — dicatat pada manifest sub-modul itu bagian 4 |

---

## 4. Rantai masukan

```text
00-interview-decisions.md  rev 8  (grill-me: Scope + Closure + Amendment terdahulu + Amendment CAP-025 tuntas)
        |
01-existing-capability-map.md  (trace-existing-capabilities)
        |
evidence/02-requirement-completeness-gate.md  (PARTIALLY_READY)
        |
evidence/03-hospital-domain-architecture.md  (modul PARTIAL; scope dokter READY sejak revision 0.2)
        |
02-module-map.md  ◄── bentuk COMPOSITE; membagi 28 kemampuan ke tiga sub-modul
        |
        +--> episode-rawat-inap/  arsitektur + kontrak + PRD  →  roadmap  →  task
        +--> keperawatan/         dirancang, draft
        +--> dokter-rawat-inap/   domain architecture READY; artefak desain CURRENT rev 0.2, menunggu approval
```

Baseline requirement `PRD-RWI-FINAL-001` masuk pada tahap `00-interview-decisions.md` lewat
`RWI-DEC-080`, menggantikan batas scope revision `4`.

---

## 5. Design gate

Blueprint ini adalah desain target, bukan spesifikasi implementasi yang disetujui.

### 5.1 Gerbang implementasi

| Gate | Keadaannya | Menahan sub-modul |
|---|---|---|
| Persetujuan pemilik modul tetangga | **TERBUKA SEBAGIAN.** Dicabut 2026-08-21 oleh `RWI-DEC-062` untuk `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` HealthServices. Bagian `EmergencyInstallationManagement` terbuka kembali 2026-08-24 lewat `RWI-DEC-069`; pemiliknya **Rizki Gunawan**, persetujuan formalnya belum tercatat | `episode-rawat-inap`, hanya `INP-S09` |
| Kesiapan data master | **MASIH TERBUKA.** Penanggung jawabnya `RWI-DEC-063`. Gerbang tertutup begitu datanya benar-benar terisi. Sejak revision `3` syaratnya bertambah: penanda jenis kelamin, isolasi, dan boks bayi harus **benar** | `episode-rawat-inap` |
| *Shared inpatient clinical context resolver* | Impact scan mengklasifikasikan `INT-DOK-01` sebagai **`Missing`** dan defect null-queue sebagai **`Repair`**. `INT-KEP-01` dan `INT-DOK-01` wajib dikerjakan bersama; amendment desain boleh berjalan, planning/build dokter belum boleh | `keperawatan`, `dokter-rawat-inap` |
| Pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif | Impact scan mengklasifikasikan `INT-DOK-02` sebagai **`Extend`**. Scope wajib terbatas pada `Inpatient`/`Emergency` dan menjaga regresi `Outpatient`/`MCU` | `dokter-rawat-inap` |
| Consumer frontend Dokter Rawat Inap | **`Conflict`.** Route/menu ter-commit memakai hook, service, state, dan aksi antrean dokter rawat jalan; tidak membaca episode/census/DPJP dan melanggar keputusan layar anak tanpa menu tingkat dua | `dokter-rawat-inap`; menahan sign-off, planning frontend, dan rilis |
| Perbaikan tombol tempat tidur | Hari ini selalu gagal 404. `RWI-DEC-049`. Pekerjaan perbaikan, bukan keputusan | `episode-rawat-inap` |
| Test regresi modul tetangga | Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi. `RWI-DEC-051`. Pekerjaan uji, bukan keputusan | Ketiganya |
| ~~Registry lifecycle~~ | **DICABUT** 2026-08-24 oleh `RWI-DEC-068` | — |

### 5.2 Gerbang sebelum produksi — klinis dan privasi

| Gate | Keadaannya |
|---|---|
| Clinical governance owner | **Sebagian terisi.** Keputusan isolasi dan jenis kelamin diambil Muhammad Hamzah lewat `RWI-DEC-064`; belum dinyatakan apakah penunjukan itu mencakup seluruh peran clinical governance |
| Security/privacy owner | Belum ditunjuk |
| `RWI-RULE-021` batas waktu klinis | Gerbang keras. Masih menunggu pemilik klinis. Kini menahan `keperawatan` dan `dokter-rawat-inap` |
| `RWI-RULE-025` persetujuan umum | Gerbang keras. `DEC-INP-003` |
| Masa simpan riwayat | `RWI-OQ-035`, keputusan hukum. Sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum |
| ~~`RWI-RULE-012` isolasi dan jenis kelamin~~ | **BERUBAH BENTUK** 2026-08-21. Aturannya final lewat `RWI-DEC-064` s.d. `RWI-DEC-066`, dirancang sebagai `EPIC RI-34` |

---

## 6. Butir terbuka tingkat modul

| Butir | Isinya | Yang ditahannya |
|---|---|---|
| `RWI-OQ-047` | Sumber kebenaran *Financial Clearance*: `PRD-RWI-FINAL-001` bagian 23.1 menaruhnya pada Billing Management, sedangkan `RWI-RULE-028` aturan 7 memilikinya **sementara** lewat `InpFinancialClearance` | **Satu baris** pada `02-module-map.md` bagian 2.4. Tidak menahan desain, tidak menahan task berjalan |
| `RWI-OQ-034` | Persetujuan pemilik `EmergencyInstallationManagement` | `INP-S09` saja |
| `RWI-OQ-035`, `038`, `039` | Sudah dijawab, menunggu pemilik klinis atau hukum | Gerbang produksi |
| `RWI-OQ-045`, `046` | Keputusan implementasi nonblocking | Tidak menahan apa pun |
| Butir menu dua sub-modul baru | Kuota sembilan butir `IA-INP-05` sudah penuh dipakai `episode-rawat-inap` | Ditetapkan saat kedua sub-modul dirancang |

---

## 7. Yang tidak boleh diubah blueprint hilir

Perubahan pada butir berikut wajib kembali ke skill hulu, bukan diselesaikan pada tahap perencanaan
atau implementasi:

1. Kepemilikan data pada [`02-module-map.md`](./02-module-map.md) bagian 2.
2. **Baru revision `5`:** larangan `keperawatan` dan `dokter-rawat-inap` membuat tabel tandingan
   untuk dokumentasi klinis — `RWI-DEC-081`.
3. **Baru revision `5`:** pemetaan 28 kemampuan ke sub-modul — `RWI-DEC-083`. Memindahkan sebuah
   kemampuan antar sub-modul adalah keputusan pemilik, bukan kerapian struktur.
4. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
5. Sepuluh invariant `INV-INP-01` sampai `INV-INP-10` beserta cara menjaganya.
6. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
7. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.
8. Kebutuhan isolasi sebagai **atribut episode**, bukan atribut pasien dan bukan status.
9. Aturan pencampuran kamar diperiksa dari **penghuni yang sedang ada**, bukan dari penanda pada
   `MstRoom`.

---

## 8. Pemicu impact scan

| Yang berubah | Yang harus ditinjau ulang |
|---|---|
| `backend_commit_sha` atau `frontend_commit_sha` | Capability map lebih dulu, lalu seluruh kontrak setiap sub-modul |
| `Areas/HealthServices/MasterData/` tempat tidur, kamar, unit layanan, kelas | `episode-rawat-inap/` — `erd/`, `contracts/api-contract.md`, `EPIC RI-22`, `RI-23`, `RI-32`, `RI-34` |
| `Areas/HealthServices/RegistrationManagement/` | `INV-INP-04`, `EPIC RI-21` |
| `Areas/HealthServices/ClinicalManagement/` atau `PharmacyManagement/` | **`keperawatan/` dan `dokter-rawat-inap/` seluruhnya.** Sejak `RWI-DEC-081` kedua modul itu adalah **pemilik tabel** kedua sub-modul tersebut, bukan lagi sekadar tetangga yang menahan slice |
| `Areas/HealthServices/BillingManagement/` | `RWI-RULE-028` aturan 7 dan **`RWI-OQ-047`**; sumber kelayakan keuangan mungkin berpindah |
| `Repositories/ApplicationDbContext.cs` | Rencana migration, `02-module-map.md` bagian 3.4 |
| `agents/rules/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix dan lifecycle modul |
| Munculnya berkas berawalan `Inp` | Seluruh status `Baru` wajib dinilai ulang |
| `PRD_Final_Rawat_Inap_100_Persen.md` | Pemetaan 28 kemampuan pada `02-module-map.md` bagian 4 |
| `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md` — SHA-256 berbeda dari `2b3b2f29…0a679f` | Bagian 0-B seluruhnya; temuan `RLN-01` s.d. `RLN-14` dinilai ulang sebelum fase `RLN-PH-02` s.d. `RLN-PH-06` memakainya |

### 8.1 Hasil impact scan terfokus terbaru

Impact scan 2026-09-02 pada backend `93b3227c431401d8f586dec4e1fb25fbf41766e3` dan frontend
`863f24b0d1617069310c04e5770b47fd1b518b5b` selesai **hanya untuk** Dokter Rawat Inap. Snapshot
revision `5` di metadata sengaja tidak diganti karena sub-modul lain belum dipindai ulang; SHA
terbaru dicatat terpisah agar snapshot desain dan bukti audit tidak tercampur.

| Scope | Keadaan | Konsekuensi lifecycle |
|---|---|---|
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) bagian 15 | `CURRENT` untuk `CAP-015`, `CAP-020`–`CAP-025`, `INT-DOK-01`, `INT-DOK-02`, frontend, authorization, dan test | Menjadi input kanonis amendment |
| `02-module-map.md` bagian Dokter Rawat Inap | **`CURRENT`** sejak 2026-09-02 | Pernyataan Radiologi, nama entity visite, dan urutan migration sudah diperbaiki |
| Seluruh artefak `dokter-rawat-inap/` | **`approved`** pada revision `0.3` | Boleh dipakai `plan-module-delivery`. Perubahan sesudah ini membuat revision baru dan memicu impact scan kedua repository |
| Status sub-modul / modul | Tetap `draft` / `partial` | Staleness artefak tidak mengubah aturan derivasi status komposit |

---

## 9. Riwayat revision

| Revision | Tanggal | Ringkasan |
|---|---|---|
| `7` | 2026-09-15 | **Penyelarasan `PRD-RWI-V2-001`, fase `RLN-PH-06`, `draft`.** Menyerap `RWI-DEC-106` s.d. `149` dan gate `1.6`. `dokter-rawat-inap` kontrak `0.6.0`: ruang kerja satu halaman, registrasi sejak konsep dan penguncian, jenis catatan CPPT, pesanan dengan pemberi instruksi, Resep Harian, rekonsiliasi, template dan order sliding scale. `keperawatan` kontrak `0.5.0`: delapan menu, konfigurasi klinis berversi, progres, Evaluasi Awal MPP, Pengawasan Harian, MAR, pelaksanaan sliding scale. `episode-rawat-inap` kontrak `0.9.0` amandemen terbatas: census dokter, penugasan pendukung, resume delapan bagian, akibat penutupan. `02-module-map.md` revision `2`. Handover shift dan transfusi `DEFERRED` |
| `6` | 2026-09-11 | Gelombang 1A Rawat Inap Safety Corrections; disetujui lewat `RWI-DEC-105` — lihat bagian 0-A |
| `5` | 2026-09-02 | **Migrasi bentuk `SINGLE` → `COMPOSITE`.** Menyerap `RWI-DEC-080` s.d. `RWI-DEC-083`. `PRD-RWI-FINAL-001` menggantikan batas scope revision `4`; modul menjadi 28 kemampuan. Tiga sub-modul lahir: `episode-rawat-inap` (16 kemampuan, `approved`), `keperawatan` (5, `draft`), `dokter-rawat-inap` (7, `draft`). `02-module-map.md` lahir; tabel kepemilikan data, peta butir menu, dan urutan migration lintas sub-modul naik ke sana. Kamus data pindah `erd/` → `data/`. Tiga dokumen basi `DEC-INP-001` diperbaiki. Status modul **diturunkan** menjadi `partial`. **Nol tabel baru, nol kolom baru, nol endpoint baru, nol kontrak naik versi** |
| `4` | 2026-08-24 | **Disetujui Muhammad Hamzah lewat `RWI-DEC-074`.** Menyerap empat keputusan Amendment Pass dari tiga usulan lintas modul milik blueprint IGD. Kelayakan Penempatan tumbuh menjadi sembilan aturan; dua integrasi arah baca baru; `InpBedPlacement.StartDateTime` berubah asal nilainya untuk jalur serah terima; sepuluh acceptance criteria baru. Nol tabel, kolom, dan endpoint baru |
| `3` | 2026-08-21 | Menyerap tiga keputusan penutupan butir organisasi. Satu epic baru `EPIC RI-34` beserta 9 functional requirement, 5 skenario UAT, 26 skenario acceptance test. Enam kolom baru pada `InpEpisode`, satu enum, dua endpoint, satu penjaga service, satu daftar pantau, satu layar. `INP-S11` berpindah dari slice yang dihentikan menjadi slice yang dirancang |
| `2` | 2026-08-21 | Menyerap empat keputusan Amendment Pass. Satu tabel baru `InpDischargeSummaryRevision`, tiga kolom baru, satu nilai enum baru, satu endpoint baru, satu invariant baru `INV-INP-10`, `INV-INP-01` dilonggarkan |
| `1` | 2026-08-21 | Blueprint pertama. Dua bounded context, satu aggregate root, dua belas tabel baru, nol perubahan kolom pada tabel modul lain. Sembilan slice dirancang, delapan sengaja dihentikan. 13 epic, 47 functional requirement, 23 skenario UAT, 82 skenario acceptance test |

---

## 10. Langkah berikutnya

> ★ **Diperbarui 2026-09-15 malam — revision `7` `draft`.** Hasil `design-business-module` masih `draft`; approval
> manusia belum tergantikan.
>
> | Kondisi | Skill | Untuk |
> |---|---|---|
> | **Pemilik menyetujui revision `7`** ketiga sub-modul, termasuk mengonfirmasi usulan non-blocking (`dokter` 22.20 nomor 7–8; `keperawatan` 22.20 nomor 5–9; `episode` 22.7) | `plan-module-delivery` | `DOK-V2-0`…`4`, `KEP-V2-0`…`4`, `RI-V2-1`…`3` dengan urutan `02-module-map.md` 3.4.1 |
> | Pemilik ingin menutup temuan non-blocking lebih dulu — integrasi Gizi/Bank Darah/Kamar Operasi yang ternyata ada di source; nasib pesanan tertagih; dosis `Due` saat penutupan | `grill-me` Amendment Pass | Tingkat modul |
> | Pemberitahuan lintas pemilik: Yoga Aji Pratama (penguncian, `my-authored`, jenis dokumen `14`), pemilik `rawat-jalan` (`R7`, `R9`, `K2`, ekstraksi komponen), pemilik Lab/Rad, pemilik Billing | Bukan skill — tindakan pemilik | Sebelum rilis gelombang terkait |
> | SHA backend/frontend berubah sebelum approval | `trace-existing-capabilities` impact scan | Area terdampak |
>
> Baris tabel lama di bawah dipertahankan sebagai jejak.

> **Diperbarui 2026-09-14.** Langkah paling depan kini penyelarasan ulang terhadap
> `PRD-RWI-V2-001`. Dua baris pertama di bawah boleh dijalankan bersamaan; urutan lengkapnya pada
> bagian 0-B.4.

| Kondisi | Skill | Untuk sub-modul |
|---|---|---|
| **`RLN-PH-02` `READY`** — keputusan dokumen hulu lama dan konflik PRD baru terhadap blueprint | `/qv-grill` Amendment Pass | Tingkat modul, `dokter-rawat-inap`, `keperawatan` |
| **`RLN-PH-03` `READY`** — isian V1, kesamaan layout Dokter Rawat Jalan, butir MVP-0, SHA baru | `/qv-trace` impact scan terfokus | `dokter-rawat-inap`, `keperawatan` |
| `RLN-PH-02` dan `RLN-PH-03` selesai | `/qv-gate` terfokus | Kemampuan baru dan yatim, `RLN-04`, `RLN-07` |
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 terjawab dan owner menyetujui | `/qv-plan` | `episode-rawat-inap` |
| ~~Focused requirement gate Dokter Rawat Inap selesai~~ | ~~`hospital-domain-architect` amendment~~ | **SELESAI 2026-09-02.** Hasilnya `DOMAIN_ARCHITECTURE_READY` untuk ketujuh capability, pada `evidence/03-hospital-domain-architecture.md` Bagian Kedua |
| ~~Domain amendment ketujuh capability siap~~ | ~~`design-business-module` amendment~~ | **SELESAI 2026-09-02**, dua putaran. Revision `0.2` menyerap arsitektur domain; revision `0.3` menyerap `RWI-DEC-086` s.d. `RWI-DEC-088` |
| **Owner menyetujui revision `0.2` dan pertanyaan memblokir dijawab** | `plan-module-delivery` | `dokter-rawat-inap`; pecah menjadi task backend dan frontend berbasis vertical slice |
| `RWI-OQ-047` ingin ditutup | `/qv-grill` Amendment Pass | Tingkat modul |
| Salah satu SHA berubah | `/qv-trace` impact scan | Seluruhnya |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) | `keperawatan`, `dokter-rawat-inap` |

**Sub-modul `dokter-rawat-inap` sudah `approved` sejak 2026-09-03** dan boleh diteruskan ke
`plan-module-delivery`. `keperawatan` tetap `draft` dan dinilai dari manifestnya sendiri; status
modul karena itu tetap `partial`, diturunkan dari baris sub-modul dan bukan ditulis tangan.

**Approval itu menyetujui desain, bukan memberi izin menulis source.** Wewenang implementasi,
migration, dan deployment tetap terpisah, sebagaimana registry kepemilikan prefix juga hanya memberi
hak penamaan. Gerbang produksi pada `dokter-rawat-inap/blueprint-manifest.md` bagian 6 tetap
berlaku apa adanya.
