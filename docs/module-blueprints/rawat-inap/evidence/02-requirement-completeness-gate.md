# Rawat Inap — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Assessment revision | **`1.12`** — evaluasi Bed Management 10 Oktober 2026, bagian 21. Sebelumnya: **`1.11`** — gerbang kelengkapan Workspace PPRI `PRD-RWI-ADMISI-001`, bagian 20. Sebelumnya: **`1.10`** — evaluasi ulang Finishing sesudah Amendment Pass dan approval desain 2 Oktober 2026, bagian 19. Sebelumnya: **`1.9`** — evaluasi ulang gerbang Finishing Rawat Inap `PRD-RWI-FINISHING-001` v`0.4`, bagian 18. Sebelumnya `1.8` (Finishing v`0.2`, bagian 17) dan `1.7` |
| Assessment date | **Bed Management 10 Oktober 2026 (`Asia/Jakarta`, bagian 21);** **Workspace PPRI 7 Oktober 2026 (bagian 20);** 21 Agustus 2026 (`Asia/Jakarta`); focused reassessment Dokter Rawat Inap dan Keperawatan, 2 September 2026; focused reassessment penyelarasan `PRD-RWI-V2-001`, 15 September 2026; penutupan keputusan `RLN-PH-04`, 15 September 2026; **evaluasi gerbang kelengkapan requirement Integrasi Rawat Inap ↔ Billing (`INP-S22`), 17 September 2026**; evaluasi gerbang Finishing Rawat Inap, 1 Oktober 2026 (revision `1.8`); **evaluasi ulang Finishing v`0.4`, 1 Oktober 2026 malam (revision `1.9`)**; **evaluasi ulang Finishing pasca-desain, 2 Oktober 2026 (bagian 19)** |
| Assessment status | `CURRENT` |
| Koreksi `1.1` | Tiga keterangan yang menyatakan `DEC-INP-001` masih terbuka diperbaiki; kesiapan belum dinilai ulang pada revision itu |
| Focused reassessment `1.2` | Menilai ulang `INP-S05` bagian dokter, `INP-S06`, serta `CAP-015` berdasarkan decision log revision `7`, PRD final, dan capability map revision `1.3`. Hasil kanonisnya ada pada bagian 11 |
| Decision closure `1.3` | Menyerap hasil Amendment Pass `CAP-025` pada decision log revision `8`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085`; hasil kanonis terbaru untuk Dokter Rawat Inap ada pada bagian 12 |
| Focused reassessment `1.4` | Menilai **lima kemampuan Keperawatan** yang tidak pernah punya slice sendiri: `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, dan `CAP-027`. Slice baru `INP-S16`. Hasilnya pada bagian 13 |
| Focused reassessment `1.5` | Fase `RLN-PH-04`: kemampuan baru dan yatim dari `PRD-RWI-V2-001`. Slice baru `INP-S17` s.d. `INP-S21`. Decision ID baru `DEC-INP-010` s.d. `DEC-INP-012`. Menutup temuan manifest `RLN-04` dan `RLN-07`. Hasilnya pada bagian 14 |
| Decision closure `1.6` | Menyerap Amendment Pass penutupan gate `RLN-PH-04`: `RWI-DEC-145` s.d. `RWI-DEC-149` dan `RWI-AC-219` s.d. `RWI-AC-231`. `DEC-INP-010` dan `DEC-INP-011` **`CLOSED`**, `DEC-INP-012` **`DEFERRED`**. `INP-S17` naik ke `READY_FOR_DOMAIN_DESIGN`; `INP-S19` `READY_FOR_DOMAIN_DESIGN` terbatas pada sliding scale, handover shift dan transfusi `DEFERRED`. Hasil kanonis terbaru untuk `INP-S17` dan `INP-S19` ada pada bagian 15 |
| **Focused reassessment `1.7`** | **Evaluasi gerbang kelengkapan requirement untuk integrasi Rawat Inap ↔ Kasir / Billing (slice `INP-S22`) berbasis `PRD Integrasi-Rawat-Inap-dengan-Billing.md`, keputusan wawancara `RWI-DEC-156` s.d. `RWI-DEC-161` dan kriteria penerimaan `RWI-AC-236` s.d. `RWI-AC-241` pada `00-interview-decisions.md` revision 25, serta audit kemampuan pada `01-existing-capability-map.md` revision 1.5 Bagian 18. Hasil kanonisnya ada pada Bagian 16** |
| **Focused reassessment `1.8`** | **Evaluasi gerbang Finishing Rawat Inap, 1 Oktober 2026.** Menilai `CAP-RWF-01` s.d. `CAP-RWF-16` dari `PRD-RWI-FINISHING-001` v`0.2` dalam sembilan slice baru `INP-S23` s.d. `INP-S31`, berdasarkan decision log revision `29` (`RWI-DEC-163` s.d. `RWI-DEC-193`) dan capability map revision `1.6` bagian 19. Enam slice `READY_FOR_DOMAIN_DESIGN`; `INP-S24`, `INP-S27`, dan `INP-S28` `PARTIALLY_READY`. Decision ID baru `DEC-INP-014` s.d. `DEC-INP-017`, seluruhnya `OPEN`. Bagian 16.8 butir 4 dan batas auto-reblock 16.9 digantikan `RWI-DEC-186`. Hasilnya pada bagian 17 |
| **Focused reassessment `1.9`** | **Evaluasi ulang gerbang Finishing Rawat Inap, 1 Oktober 2026 malam.** Menyerap Amendment Pass penutupan gate `1.8` (`RWI-DEC-194` s.d. `RWI-DEC-205`, decision log revision `30`) dan `PRD-RWI-FINISHING-001` v`0.4`. `DEC-INP-014` s.d. `DEC-INP-017` **`CLOSED`**, sehingga `INP-S24`, `INP-S27`, dan `INP-S28` naik ke `READY_FOR_DOMAIN_DESIGN`. Klaster Pasca Operasi `CAP-RWF-18` s.d. `23` dinilai sebagai slice baru `INP-S32` s.d. `INP-S37`: lima `READY_FOR_DOMAIN_DESIGN`, `INP-S32` `PARTIALLY_READY` karena satu aturan Billing (`DEC-INP-018`, alias `RWI-OQ-114` butir b). Gerbang implementasi: `RWI-OQ-108`, `RWI-OQ-114` butir (a) dan (c), `RWI-OQ-115`. Hasilnya pada bagian 18 |
| **Focused reassessment `1.10`** | **Evaluasi ulang Finishing pasca-desain, 2 Oktober 2026.** Menyerap Amendment Pass penutupan butir terbuka desain Finishing (`RWI-DEC-206` s.d. `RWI-DEC-220`) dan approval desain revision `8` (`RWI-DEC-221`), decision log revision `32`. `DEC-INP-018` **`CLOSED`** (`RWI-DEC-207`), sehingga `INP-S32` naik ke `READY_FOR_DOMAIN_DESIGN` dan **seluruh 15 slice Finishing siap**. Gerbang persetujuan `RWI-OQ-108`, `114`, `115` tertutup. G-05, G-17, G-18, G-26 tertutup; dua belas usulan dikonfirmasi lewat approval kontrak. Gap baru G-27 (penyelesaian multi-invoice Billing) dan G-28 (PRD tertinggal), keduanya non-blocking. Empat temuan implementasi frontend `f74758af5` (IMP-RWF-01 s.d. 04) menjadi gerbang rilis, bukan gap requirement. Hasilnya pada bagian 19; koreksi perencanaan 19.12 menambah `DEC-INP-019` (Rehab Medik, di luar slice Finishing) dan IMP-RWF-05 |
| **Focused reassessment `1.11`** | **Gerbang kelengkapan Workspace PPRI (Ruang Kerja Admisi), 7 Oktober 2026.** Menilai `PRD-RWI-ADMISI-001` v`0.2` dalam sebelas slice baru `INP-S38` s.d. `INP-S48`, berdasarkan decision log revision `36` (`RWI-DEC-225` s.d. `RWI-DEC-262`) dan capability map revision `1.7` bagian 20. Sembilan slice `READY_FOR_DOMAIN_DESIGN`; `INP-S46` Estimasi Biaya `PARTIALLY_READY` karena `DEC-INP-020` (baru); `INP-S48` tanda tangan digital `BUSINESS_DECISION_REQUIRED` karena `DEC-INP-003`, yang cakupannya diperluas. Penyimpanan persetujuan umum tetap slice lama `INP-S10`. Gap `G-30` s.d. `G-51`. Hasilnya pada bagian 20 |
| **Focused reassessment `1.12`** | **Bed Management, 10 Oktober 2026.** Enam kemampuan `BM-CG-01` s.d. `BM-CG-06` cukup untuk desain produk berdasarkan decision log revision `46` dan audit `BM-AUD-20261010-01`. Tidak ada keputusan bisnis pemblokir yang terbuka dalam batas ini. `BM-G01` s.d. `BM-G04` tetap syarat penerapan; bukan bukti SOP, akses, data atau runtime sudah tersedia. Hasil kanonisnya pada bagian 21 |
| **Overall readiness** | **`PARTIALLY_READY`** |
| **Bed Management readiness** | **`READY_FOR_DOMAIN_DESIGN` untuk batas produk bagian 21.** Kesiapan penerapan belum terbukti; status keseluruhan Rawat Inap tetap seperti baris di atas |
| Ready destination | **Bagian 21:** enam kemampuan Bed Management `READY_FOR_DOMAIN_DESIGN` dalam batas produk → `design-business-module`; empat gate penerapan tetap terbuka. **Bagian 20:** sembilan slice Workspace PPRI `READY_FOR_DOMAIN_DESIGN` dan bagian siap `INP-S46` → langsung `design-business-module` (amandemen `episode-rawat-inap` `0.11.0`); `hospital-domain-architect` tidak disarankan. **Bagian 19:** seluruh 15 slice Finishing `READY_FOR_DOMAIN_DESIGN`, dan desain revision `8` sudah disetujui (`RWI-DEC-221`) → `plan-module-delivery`. `hospital-domain-architect` atau langsung `design-business-module`. Ketujuh capability Dokter Rawat Inap siap sesuai bagian 12; empat kemampuan Keperawatan aktif siap sesuai bagian 13; slice penyelarasan V2 `INP-S17`, `S18`, `S19` (sliding scale), `S20`, dan `S21` siap sesuai bagian 14 dan 15; **slice integrasi Rawat Inap ↔ Billing `INP-S22` siap untuk domain design sesuai bagian 16**; **slice Finishing `INP-S23` s.d. `INP-S37` siap sesuai bagian 18, kecuali aturan Billing penggabungan biaya operasi kunjungan asal pada `INP-S32` (`DEC-INP-018`)**; handover shift dan transfusi selain monitoring `DEFERRED` |
| Business evidence | **Bagian 21:** decision log revision `46`, SHA-256 `41dd035d62e7804dff7e796712e25410152fae8e67b9444257ba73e0a6a30b3d`; `RWI-DEC-274` s.d. `294`, `RWI-FACT-068` s.d. `070`, `RWI-AC-396` s.d. `426`. **Bagian 20:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `36`, SHA-256 `618f684b707fdb60f88d194623a265844faaf61fb02061e11eb61a4104f746df` (`RWI-DEC-225` s.d. `RWI-DEC-262`, `RWI-AC-343` s.d. `RWI-AC-383`). **Bagian 19:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `32`, SHA-256 `2102ed1da2a4748157bb3e6960212905e20a704b2a265c6abda4a97ffed43b25` (`RWI-DEC-206` s.d. `RWI-DEC-221`, `RWI-AC-330` s.d. `RWI-AC-340`); `billing-kasir` `BKC-DEC-118`. **Bagian 18:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `30`, SHA-256 `aa92c5ddd217b0bd95abf628ae484a1c0caddcdf386e7834f0715ed216e2a439` (`RWI-DEC-194` s.d. `RWI-DEC-205`, `RWI-AC-307` s.d. `RWI-AC-329`). **Bagian 17:** revision `29`, SHA-256 `f6fed60809321687d850306dfd2830d9394e1b2e7e7ab0a66b01cbc5068ce471`. **Bagian 16:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `25` (Pass A — Muhammad Hamzah, 17 September 2026), memuat keputusan `RWI-DEC-156` s.d. `RWI-DEC-161` dan kriteria penerimaan `RWI-AC-236` s.d. `RWI-AC-241`. **Bagian 15:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `21`, SHA-256 `1c55c80a50aee11ef005ccde6315c2935cbe21504e8596798b89bf7f2d45102a`. **Bagian 14:** [`00-interview-decisions.md`](../00-interview-decisions.md) revision `20`, SHA-256 `b278013547dfa3c8f1bfa21fdd442cdaa416a8015939f1628794ab7e03db0fb7`. Sebelumnya revision `11` dan revision `8` |
| Capability evidence | **Bagian 21:** audit `BM-AUD-20261010-01` revision `1`, SHA-256 `50e0e1525804260331d3a830fb379532f802a3ff31e43cd3b4df4416fa42f38b`; fingerprint working tree diperiksa ulang. **Bagian 20:** capability map revision `1.7` bagian 20, SHA-256 `e8e454cbaed3f0bfd39793bbb4b82860ed9eb921b9154c24554b2bf06fb6e3da`, ditambah `RWI-FACT-065` dan `RWI-FACT-066`. **Bagian 19:** capability map revision `1.6` bagian 19 ditambah pembacaan source frontend `f74758af5` dan backend Billing pada 19.1; daftar pemicu 19.9 tidak lengkap (IMP-RWF-04). **Bagian 17 dan 18:** [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.6` bagian 19; bagian 18 ditambah `RWI-FACT-057`, `RWI-FACT-058`, dan pembacaan source pada 18.1. **Bagian 16:** [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.5` Bagian 18 (Audit Kemampuan Integrasi Rawat Inap ↔ Billing, 17 September 2026). **Bagian 14 dan 15:** [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.4`. Sebelumnya revision `1.3` |
| Primary business source | **Bagian 21:** instruksi BA Bed Management butir 1–5 yang diteruskan pengguna, dilengkapi keputusan produk revision `46`; rekomendasi draft dalam audit tidak menjadi approval sendiri. **Bagian 20:** `PRD-RWI-ADMISI-001` v`0.2` `DRAFT`, SHA-256 `f1fd336f562d6936629d50f8f849fa7767e2dbea2b24678da9596de60dcc1192`, sebagian tertinggal dari decision log (G-50). **Bagian 19:** `PRD-RWI-FINISHING-001` v`0.4`, SHA-256 `0f658455b98658974865fda4fb917b2ccdb9a9bce5db5723f9b919327cfabe1a`, sebagian tertinggal dari decision log (G-28). **Bagian 17 dan 18:** `docs/Modul-RS/Rawat-Inap/05-prd-to-mvp-finishing-rawat-inap.md` (`PRD-RWI-FINISHING-001` v`0.2` untuk bagian 17; v`0.4`, SHA-256 `aee2afdb03e62c2bcdbd2e8fc832bb634df40686079c1b5e4f7f6f4d1ae46d29`, untuk bagian 18). Klaster Pasca Operasi juga merujuk bukti HiSys `Pasca-Operasi-ke-Rawat-Inap.md` sebagai praktik sistem lain. **Bagian 16:** `docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md` (2.282 baris). **Bagian 14 dan 15:** `PRD-RWI-V2-001` v`2.0` dan `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0`. Baseline: `docs/Modul-RS/Rawat-Inap/PRD_Final_Rawat_Inap_100_Persen.md` |
| Baseline rujukan | **Bagian 21:** tidak memakai baseline rumah sakit atau regulasi baru. **Bagian terdahulu:** `indonesia-hospital-domain-reference`, berkas `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruh observasi berstatus `REFERENCE_ONLY` |
| Backend snapshot | **Bagian 21:** `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, branch `MHamzah`, ditambah fingerprint working tree. **Bagian 20:** audit `671191eb`, HEAD `fdf85a07` (enam berkas harga penjamin, dinilai `RWI-FACT-065`). **Bagian 19:** HEAD `bf5c6bde` — sejak `8d96a978` hanya dokumen; sejak audit `c8e99ce5` perubahan kode hanya saringan pencarian census. **Bagian 17 dan 18:** audit `c8e99ce5`, HEAD `425cfeae` (hanya dokumen). **Bagian 16: `fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344`** (branch `MHamzah`); Bagian 14: `df3679c0d5b2f08106702153eb242d3a6cb2929b`; sebelumnya `93b3227c431401d8f586dec4e1fb25fbf41766e3` |
| Frontend snapshot | **Bagian 21:** `969acfcc04cdf31074a1911e9827c31d25ddadd0`, branch `HamzahV2`, ditambah fingerprint working tree. **Bagian 20:** audit dan HEAD `27889662a`; perubahan lokal belum di-commit hanya menyentuh menu penunjang dan pemesanan bedah Workspace Keperawatan. **Bagian 19:** HEAD `f74758af5` — form Penunjang Medis ruang kerja dokter (19.5). **Bagian 17 dan 18:** audit `22ad67330`, HEAD `ee75e055b`; bagian 18 mencatat perubahan lokal belum di-commit yang tidak menyentuh berkas pemicu 19.9. **Bagian 16: `2c00758832f834cff0288bef4f0d2fcf1161fb52`** (branch `HamzahV2`); Bagian 14: `147355f505e875148b8416866ada6cf8b2f1ad99`; sebelumnya `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Write boundary | Dokumen evidence ini dan sinkronisasi metadata/hash blueprint. Tidak ada source aplikasi, migration, entity, endpoint, UI, task, database, atau ClickUp yang diubah |

> **Apa gunanya dokumen ini.** Dokumen ini tidak merancang apa pun. Tugasnya satu: memeriksa
> apakah kebutuhan bisnis Rawat Inap sudah cukup lengkap dan cukup berbukti untuk mulai dirancang
> arsitektur domainnya. Hasilnya berupa daftar bagian mana yang boleh maju dan bagian mana yang
> harus berhenti dulu, beserta alasannya.
>
> Dokumen ini juga **tidak menjawab** keputusan bisnis yang belum diputuskan pemiliknya. Butir
> semacam itu dicatat sebagai Decision ID lalu dikembalikan ke `/grill-me`.

---

## 1. Scope penilaian

### 1.1 Modul dan menu

| Hal | Nilai |
| --- | --- |
| Area | `HEALTH_SERVICES` |
| Modul | `InPatientManagement` / Rawat Inap, prefix `Inp`, lifecycle registry `PLANNED` |
| Batas scope bisnis | Satu episode perawatan pasien menginap, dari pasien diterima masuk sampai episode ditutup dan tempat tidur kembali kosong, sesuai `RWI-DEC-004` |

### 1.2 Slice yang dinilai

Penilaian dilakukan **per slice**, bukan per modul. Ini penting: satu slice yang terhambat tidak
boleh menghentikan slice lain yang sebenarnya sudah siap.

| Slice ID | Nama slice | Kemampuan PRD yang dicakup | Aturan bisnis utama |
| --- | --- | --- | --- |
| `INP-S01` | Admisi dan pemesanan tempat tidur | CAP-002, CAP-003, CAP-004, CAP-005, CAP-006 | `RWI-RULE-001` s.d. `005`, `013`, `015`, `022` |
| `INP-S02` | Penempatan tempat tidur, census, dan lama dirawat | CAP-006, CAP-008 | `RWI-RULE-019`, `RWI-RULE-027` |
| `INP-S03` | Perpindahan pasien dan pindah kelas | CAP-017 | `RWI-RULE-006`, `007`, `008`, `016`, `030` |
| `INP-S04` | Penugasan perawat penanggung jawab | CAP-011 | `RWI-RULE-033` |
| `INP-S05` | Dokumentasi klinis rawat inap, visite, dan penunjang dari workspace dokter | CAP-012, CAP-014, **CAP-015**, CAP-020, CAP-021, CAP-022, CAP-024, CAP-025 | `RWI-RULE-017`, `021`, `026`; `RWI-DEC-080`, `081`, `083` |
| `INP-S06` | Resep rawat inap dan obat pulang | CAP-023 | `RWI-RULE-024`, `RWI-RULE-026` |
| `INP-S07` | Keputusan pulang, cara pulang, dan resume pulang | CAP-026 | `RWI-RULE-011`, `RWI-RULE-032` |
| `INP-S08` | Daftar periksa administrasi, kelayakan keuangan, dan penutupan episode | CAP-028 | `RWI-RULE-009`, `010`, `018`, `020`, `028` |
| `INP-S09` | Serah terima IGD ke rawat inap | Titik sentuh IGD | `RWI-RULE-029` |
| `INP-S10` | Persetujuan umum rawat inap | Bagian CAP-009 yang tidak ditunda | `RWI-RULE-025` |
| `INP-S11` | Penempatan menurut jenis kelamin dan isolasi | Bagian CAP-005 dan CAP-006 | `RWI-RULE-012` |
| `INP-S12` | Bayi baru lahir dan boks bayi | Bagian CAP-002 dan CAP-006 | `RWI-RULE-014` |
| `INP-S13` | Riwayat status, audit, dan daftar pantau kepatuhan | NFR-003 | `RWI-RULE-023`, `RWI-RULE-031` |
| `INP-S14` | Pengaturan yang dapat diubah admin | Pendukung | `RWI-RULE-034` |
| `INP-S15` | Interoperabilitas SATUSEHAT dan pelaporan | **Belum ada di daftar kemampuan** | **Belum ada aturannya** |
| `INP-S16` | Keperawatan rawat inap (revision `1.4`) | CAP-012, CAP-013, CAP-014, CAP-016, CAP-027 | `RWI-RULE-021`, `RWI-RULE-026`, `RWI-RULE-033` |
| `INP-S17` | Pengkajian Keperawatan Lanjutan & Pengawasan Harian (revision `1.5` & `1.6`) | V2-CAP-01 s.d. 06, 08, RLN3-CAP-15 | `RWI-DEC-110` s.d. `113`, `149` |
| `INP-S18` | MAR & Rekonsiliasi Obat Admisi (revision `1.5`) | V2-CAP-07, V2-CAP-09 | `RWI-DEC-114` s.d. `122` |
| `INP-S19` | Sliding Scale Insulin, Handover Shift, Transfusi Darah (revision `1.5` & `1.6`) | V2-CAP-10, V2-CAP-11, V2-CAP-12 | `RWI-DEC-123`, `145` s.d. `148` |
| `INP-S20` | Integrasi Enam Layanan Penunjang Diagnostik V2 (revision `1.5`) | RLN3-CAP-01 s.d. 06 | `RWI-DEC-124` s.d. `133` |
| `INP-S21` | CPPT & Ruang Kerja Dokter Rawat Inap V2 (revision `1.5`) | RLN3-CAP-07, 08, 10 s.d. 14 | `RWI-DEC-134` s.d. `144` |
| **`INP-S22`** | **Integrasi Rawat Inap ↔ Kasir / Billing (revision `1.7`)** | `INT-CAP-01` s.d. `06` (`RANAP-INT-001` s.d. `006`) | `RWI-DEC-156` s.d. `RWI-DEC-161`, `RWI-AC-236` s.d. `RWI-AC-241` |

`INP-S15` **tidak** berasal dari dokumen keputusan. Slice ini muncul dari pembandingan dengan
baseline rumah sakit Indonesia, dan penjelasannya ada di bagian 4.11. Slice `INP-S22` ditambahkan
pada assessment revision `1.7` untuk mengevaluasi kelengkapan integrasi operasional dan keuangan
antara Modul Rawat Inap (`InPatientManagement`) dan Modul Kasir/Billing (`BillingManagement`).

### 1.3 Yang sengaja tidak dinilai

Daftar di luar scope pada assessment awal tetap historis. Sejak `RWI-DEC-080`, `CAP-015` dan
`CAP-023` masuk scope Rawat Inap sebagai **workspace dan kontrak integrasi**, bukan sebagai mesin
Laboratorium, Radiologi, atau Farmasi tandingan. Mesin pemrosesan internal, stok/dispensing,
validasi hasil, dan buku besar tetap berada di modul pemiliknya.

---

## 2. Bukti yang dipakai dan wewenangnya

### 2.1 Urutan wewenang yang dipakai

| Urutan | Jenis bukti | Tersedia untuk modul ini | Keterangan |
| ---: | --- | --- | --- |
| 1 | Requirement eksplisit terkini dari rumah sakit/user | **Ada** | Decision log revision `7`; Muhammad Hamzah dinyatakan sebagai owner lewat `RWI-DEC-061`, dan `PRD-RWI-FINAL-001` diterima sebagai baseline lewat `RWI-DEC-080` |
| 2 | SOP atau kebijakan rumah sakit yang disahkan | **Tidak ada** | Tidak ada satu pun SOP yang dilampirkan atau dirujuk |
| 3 | Keputusan rapat yang dikonfirmasi | **Tidak ada** | Tidak ada notulen yang dirujuk |
| 4 | Bukti bisnis analis atau ClickUp yang disetujui | **Ada untuk target produk** | `PRD-RWI-FINAL-001` v1.0.0 menjadi baseline requirement; nilai kebijakan klinis/legal tertentu tetap menunggu owner terkait sebelum produksi |
| 5 | Baseline rumah sakit Indonesia | **Ada** | `references/inpatient.md`, `Reference coverage: PARTIAL`, seluruhnya `REFERENCE_ONLY` |
| 6 | Bukti implementasi Quilvian V2 | **Ada dan kuat** | Focused impact scan pada `01-existing-capability-map.md` revision `1.3`, backend `93b3227`, frontend `863f24b` |
| 7 | Bukti legacy Quilvian V1 | **Tidak dipakai** | Tidak ada lampiran legacy untuk modul ini |

### 2.2 Catatan penting tentang wewenang bukti

Ini yang paling menentukan hasil penilaian, dan harus dibaca sebelum tabel mana pun:

Pada assessment awal, owner masih dicatat sebagai “pemegang sementara”. Keadaan itu sudah
`superseded`: `RWI-DEC-061` menetapkan Muhammad Hamzah sebagai owner Rawat Inap, dan
`RWI-DEC-062` memberi persetujuan lintas `ClinicalManagement`, `PharmacyManagement`, serta
`MasterData`. Karena itu `DEC-INP-001` tidak lagi menjadi blocker bisnis.

Clinical Governance, Security/Privacy, Pharmacy, Laboratory, dan Radiology tetap membutuhkan
sign-off kebijakan atau kontrak final sebelum produksi. Ketiadaan sign-off produksi tersebut
tidak otomatis memblokir domain design selama bentuk targetnya sudah dikunci dan nilai policy
yang belum final tetap configurable serta tidak dipalsukan.

Ketiadaan SOP yang disahkan tidak dipakai sebagai alasan memblokir, karena bukti tingkat 1 dan 4
sudah menjawab sebagian besar pertanyaan. Tetapi ketiadaan itu dicatat sebagai keterbatasan pada
bagian 9.

---

## 3. Ringkasan hasil

| Hal | Jumlah |
| --- | ---: |
| Slice yang dinilai | 15 |
| Slice yang dinilai ulang pada revision `1.2` | 2 — `INP-S05` bagian dokter dan `INP-S06` |
| Slice `READY_FOR_DOMAIN_DESIGN` | 8 |
| Slice `PARTIALLY_READY` | 3 |
| Slice `BUSINESS_DECISION_REQUIRED` | 4 |
| Capability focused scope `READY_FOR_DOMAIN_DESIGN` | 6 |
| Capability focused scope `BUSINESS_DECISION_REQUIRED` | 1 — `CAP-025` |
| Dimensi kelengkapan focused scope yang dinilai | 18 |
| Butir focused `PROPOSED`/`MISSING` nonblocking | 4 |
| Butir focused `CONFLICT` / Decision ID pemblokir | 1 / 1 — `DEC-INP-008` |

Jumlah blocker global di luar focused scope tidak dihitung ulang pada revision `1.2`; statusnya
tetap historis sampai slice terkait dinilai ulang dengan decision log terbaru.

**Kalimat pendeknya:** `INP-S06` dan enam capability Dokter Rawat Inap cukup lengkap untuk domain
design. Hanya `CAP-025 Physician Visit` yang berhenti karena definisi visite lama dan PRD final
bertentangan secara material; blocker lain pada source adalah pekerjaan teknis, bukan keputusan
bisnis.

---

## 4. Temuan kelengkapan pada 18 dimensi

### 4.1 Dimensi 01 — Tujuan

**Status: `CONFIRMED`.**

Hasil bisnis yang dituju dinyatakan satu kalimat pada batas scope, dan diperinci menjadi 18
kemampuan MUST. Kalimat batasnya: mengelola satu episode perawatan pasien menginap, dari pasien
diterima masuk sampai episode ditutup dan tempat tidur kembali kosong.

Bukti: `00-interview-decisions.md` bagian Scope dan Outcome; `RWI-DEC-004`.

### 4.2 Dimensi 02 — Aktor

**Status: `CONFIRMED`.**

| Aktor | Perannya | Bukti |
| --- | --- | --- |
| Petugas admisi | Membuka admisi, memesan bed, menempatkan pasien, menandai daftar periksa, menutup episode | `RWI-RULE-004`, `010`, `018` |
| DPJP | Memutuskan pasien boleh pulang, meminta perpindahan, menandatangani resume | `RWI-RULE-010`, `016`, `030`, `032` |
| Kepala ruangan | Menugaskan perawat, memindahkan pasien, menindaklanjuti daftar pantau kepatuhan | `RWI-RULE-006`, `023`, `033` |
| Perawat pelaksana | Menulis pengkajian dan catatan, memindahkan pasien | `RWI-RULE-006`, `021` |
| Supervisor | Membatalkan admisi setelah `Admitted`, menutup episode, menembus gerbang keuangan, membuka kembali episode | `RWI-RULE-004`, `009`, `010`, `020` |
| Petugas kasir atau billing | Menandai kelayakan keuangan | `RWI-RULE-028` |
| Admin master data | Mengatur parameter, mengisi master, menyetel keadaan bed non-pasien | `RWI-RULE-027`, `034` |

Baseline `ID-INP-CAP-006` mengingatkan agar wewenang profesional tidak disimpulkan dari nama
jabatan. Di sini wewenang memang ditulis eksplisit per tindakan, bukan disimpulkan.

### 4.3 Dimensi 03 — Pemicu dan prasyarat

**Status: `CONFIRMED` untuk dua dari tiga jalur masuk.**

| Jalur masuk | Pemicu | Status |
| --- | --- | --- |
| Pasien datang langsung | Petugas admisi membuka admisi | `CONFIRMED` — `RWI-DEC-011` |
| Pasien dari poliklinik | Kunjungan poliklinik yang sudah ada dipakai | `CONFIRMED` — `RWI-DEC-011` |
| Pasien dari IGD | Disposisi `RANAP` dijalankan | `CONFIRMED` arahnya lewat `RWI-DEC-041`, tetapi **terblokir** pada `DEC-INP-002` |

Baseline `ID-INP-CAP-001` menanyakan "keputusan merawat" yang mendahului admission, termasuk
tingkat kegawatan dan prasyarat payer. Tingkat kegawatan tidak dipakai sebagai prasyarat mana pun
pada modul ini, dan prasyarat payer sengaja ditunda bersama CAP-010. Keduanya deferral yang
disadari, bukan lubang.

### 4.4 Dimensi 04 — Alur utama

**Status: `CONFIRMED`.**

Alur utama tertulis urut dan lengkap:

`Admisi → pemesanan bed → penempatan bed → episode Admitted → census → penugasan perawat →
pengkajian awal → dokumentasi harian → resep → perpindahan bila perlu → keputusan pulang →
DischargePending → resume pulang → daftar periksa administrasi → kelayakan keuangan → Closed →
bed kembali Available.`

Setiap langkah punya pelaku, syarat, dan hasil akhir yang tertulis. Contoh berangka juga tersedia
pada hampir setiap aturan.

### 4.5 Dimensi 05 — Alur alternatif dan exception

**Status: `CONFIRMED` untuk sebagian besar, dengan tiga gap.**

Yang sudah tertutup:

| Exception | Aturan |
| --- | --- |
| Pembatalan admisi | `RWI-RULE-004` |
| Pemesanan bed gugur, lalu bed diambil pasien lain | `RWI-RULE-002`, `RWI-RULE-015` |
| Episode `Draft` telantar | `RWI-RULE-022` |
| Perpindahan gagal di tengah jalan | `RWI-RULE-008` |
| Lima cara pulang | `RWI-RULE-011` |
| Penutupan tanpa kelayakan keuangan | `RWI-RULE-009` |
| Pembukaan kembali episode | `RWI-RULE-020` |
| Serah terima IGD gagal | `RWI-RULE-029` aturan 5 |

Yang belum tertutup, ketiganya diangkat baseline pasal 10:

1. **Kepergian fisik pasien terpisah dari penutupan administratif.** Baseline pasal 9 secara tegas
   memisahkan "kepergian pasien", "pembebasan tempat tidur", dan "penyelesaian encounter" sebagai
   tiga kejadian yang belum tentu bersamaan. Dokumen keputusan menggabungkan pembebasan tempat
   tidur ke dalam penutupan episode, sehingga tempat tidur tetap terbaca terisi selama pasien
   sudah pulang tetapi episodenya belum ditutup. Baris daftar pantau "penutupan tertunda" dengan
   ambang 4 jam justru membuktikan jeda itu memang diperkirakan terjadi. Klasifikasi:
   `PROPOSED` / `NON_BLOCKING_STANDARD`, lihat bagian 5.
2. **Episode rawat inap aktif ganda untuk satu pasien.** Tidak ada aturan yang melarang satu
   pasien punya dua episode aktif sekaligus. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.
3. **Perpindahan yang dicatat sebelum pasien benar-benar berpindah.** Sistem hanya mengenal satu
   waktu perpindahan. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.

### 4.6 Dimensi 06 — Data minimum

**Status: `CONFIRMED` untuk slice yang siap; `MISSING` untuk `INP-S15`.**

Data minimum untuk admisi, pemesanan, penempatan, perpindahan, penugasan, resume, dan penutupan
sudah disebut satu per satu di dalam aturannya masing-masing, termasuk kolom wajib dan alasan
wajib. Contoh: `RWI-RULE-030` menyebut catatan DPJP wajib memuat dokter, masa berlaku, pengalih,
dan alasan.

Yang belum: data minimum untuk pengiriman interoperabilitas, karena topik itu memang belum pernah
dibahas. Lihat bagian 4.11.

### 4.7 Dimensi 07 — Aturan bisnis dan validation

**Status: `CONFIRMED`.**

34 aturan bisnis tertulis, seluruhnya disertai contoh berangka, dan diturunkan menjadi 115
acceptance criteria yang dapat diuji. Ini jauh di atas kelengkapan minimum yang dituntut gerbang
ini.

### 4.8 Dimensi 08 — Status dan perubahan status

**Status: `CONFIRMED`.**

Model status episode dikunci lima nilai: `Draft`, `Admitted`, `DischargePending`, `Closed`,
`Cancelled`, dengan tabel perpindahan yang menyebut siapa boleh memicu dan syaratnya
(`RWI-RULE-003`). Status tempat tidur memakai enum yang sudah ada di source. Perpindahan status
wajib lewat satu pintu dan meninggalkan riwayat (`RWI-RULE-031`).

Baseline pasal 5 mengingatkan bahwa status klinis, okupansi tempat tidur, administratif,
finansial, dan interoperabilitas dapat berjalan mandiri dan perlu direkonsiliasi. Dokumen
keputusan memang memisahkan status episode dari status tempat tidur dan dari status kelayakan
keuangan. Yang belum dipisahkan adalah status interoperabilitas, karena `INP-S15` belum ada.

### 4.9 Dimensi 09 — Peran dan authorization

**Status: `CONFIRMED`.**

Setiap tindakan material punya peran yang berwenang, dan yang paling penting: kewenangan per
pasien sudah dipikirkan, bukan hanya kewenangan per peran. `RWI-RULE-030` menetapkan hanya DPJP
aktif episode itu yang boleh meminta perpindahan, dan menyadari bahwa mesin hak akses yang ada
tidak dapat menegakkannya sehingga penjaganya ditulis di dalam service.

### 4.10 Dimensi 10 — Dependency antarmodul

**Status: `CONFIRMED` isinya, tetapi persetujuannya belum ada.**

| Modul tetangga | Yang dibutuhkan | Status persetujuan |
| --- | --- | --- |
| `RegistrationManagement` | Kunjungan sebagai jangkar episode | Belum diminta secara eksplisit |
| `ClinicalManagement` | Pelonggaran keharusan antrean dan konsultasi | **SUDAH ADA** — diberikan Muhammad Hamzah 2026-08-21 lewat `RWI-DEC-062`, yang menutup `RWI-OQ-032` sekaligus `DEC-INP-001`. Baris ini semula berbunyi "Belum ada"; **dikoreksi 2026-09-02** |
| `PharmacyManagement` | Pelonggaran resep dan penanda obat pulang | **SUDAH ADA** — sumber, pemberi, dan tanggalnya sama dengan baris di atas. **Dikoreksi 2026-09-02** |
| `MasterData` | Pembatasan endpoint ketersediaan tempat tidur | **Belum ada** — tidak memblokir, lihat bagian 5 |
| `EmergencyInstallationManagement` | Serah terima disposisi `RANAP` | **Belum ada** — `DEC-INP-002` |
| `BillingManagement` | Status kelayakan keuangan | Tidak dibutuhkan pada MVP, diganti penandaan manual `RWI-RULE-028` |

Baseline pasal 11 juga menyebut Notification, Medical Record, Credentialing, Nutrition, dan
Rehabilitation. Empat yang terakhir sudah dinyatakan di luar scope. Notification belum pernah
dibahas, lihat dimensi 15.

### 4.11 Dimensi 11 — Integrasi internal dan eksternal

**Status: `MISSING`. Ini gap terbesar yang ditemukan gerbang ini.**

Integrasi internal sudah jelas: Farmasi menerima resep dengan konteks encounter dan status
penyerahannya dibaca balik; Billing diganti penandaan manual sementara; IGD lewat disposisi.

Integrasi eksternal **tidak dibahas sama sekali**. Kata "SATUSEHAT" tidak muncul satu kali pun di
dalam 2.163 baris dokumen keputusan. Padahal PRD sendiri menyebutnya:

> "Playbook SATUSEHAT Rawat Inap mendefinisikan satu rangkaian rawat inap sebagai `Encounter`,
> termasuk timeline lokasi, diagnosis, observation, procedure dan discharge-related data.
> Dokumentasi juga menunjukkan perubahan lokasi/bed perlu direpresentasikan sebagai histori
> location dalam encounter."
>
> — `docs/Modul-RS/PRD-Modul-Rawat-Inap.md` baris 814

Baseline juga menandai topik ini dengan lima observasi terpisah — `ID-INP-INT-001` sampai
`ID-INP-INT-005` — seluruhnya dengan `integration_relevance: HIGH`, `audit_relevance: HIGH`, dan
`billing_relevance: HIGH`.

**Kenapa ini penting dan bukan sekadar pekerjaan susulan.** Baris PRD di atas menyebut riwayat
lokasi harus terwakili **di dalam encounter**. Sementara `RWI-DEC-039` menempatkan riwayat lokasi
pada catatan penempatan milik Rawat Inap, dan capability map membuktikan kunjungan hari ini hanya
punya satu kolom `RoomId` tanpa riwayat. Keduanya bisa saja tetap sejalan bila catatan penempatan
dipakai sebagai sumber yang dibaca saat pengiriman. Tetapi itu **belum diputuskan**, dan bila
jawabannya ternyata "riwayat lokasi harus tersimpan pada kunjungan", maka pemilik datanya berpindah
dari Rawat Inap ke Registrasi. Perpindahan pemilik data adalah perubahan yang mahal bila baru
ketahuan setelah desain jadi.

Klasifikasi: `MISSING` / `BLOCKING`, dicatat sebagai `DEC-INP-005`. Memblokir `INP-S15`, dan
**tidak** memblokir `INP-S01` maupun `INP-S02` dengan syarat catatan penempatan dirancang sebagai
sumber yang dapat dibaca ulang, bukan sekadar penanda keadaan terakhir.

### 4.12 Dimensi 12 — Hasil akhir

**Status: `CONFIRMED`.**

Hasil akhir yang dapat diamati: episode berstatus `Closed`, tempat tidur kembali `Available`,
resume pulang tertandatangani, daftar periksa administrasi tertutup, dan riwayat status lengkap.
Seluruhnya punya acceptance criteria.

### 4.13 Dimensi 13 — Pembatalan dan koreksi

**Status: `CONFIRMED`.**

Pembatalan admisi, pembatalan pemesanan, pembatalan perpindahan, dan pembukaan kembali episode
semuanya punya aturan beserta wewenangnya. `RWI-RULE-020` bahkan tegas bahwa reopen hanya untuk
membetulkan catatan, tidak mengembalikan tempat tidur, dan tidak menambah lama dirawat.

Satu hal yang belum ada: koreksi resume pulang setelah episode ditutup hanya bisa lewat reopen,
dan tidak ada versi resume yang tersimpan. Baseline `ID-INP-CAP-019` menanyakan riwayat versi
resume. Klasifikasi: `MISSING` / `NON_BLOCKING_STANDARD`.

### 4.14 Dimensi 14 — Audit dan histori

**Status: `CONFIRMED`.**

`RWI-RULE-031` menetapkan tabel riwayat status tersendiri yang ditulis dalam transaksi yang sama,
lewat satu pintu, tidak dapat diubah, dan mencatat pelaku, waktu, alasan, serta perubahan yang
dilakukan sistem secara terpisah dari yang dilakukan orang. `RWI-RULE-030` dan `RWI-RULE-033`
menambahkan riwayat DPJP dan riwayat perawat.

Yang belum: masa simpan riwayat sebelum boleh diarsipkan (`RWI-OQ-035`). Klasifikasi: `MISSING` /
`NON_BLOCKING_STANDARD`, karena bentuk tabelnya tidak berubah oleh keputusan itu.

### 4.15 Dimensi 15 — Notifikasi

**Status: `MISSING`, tidak material untuk MVP.**

Modul ini memakai pendekatan tarik, bukan dorong: tiga daftar pantau pada `RWI-RULE-023` yang
dibuka sendiri oleh penanggung jawabnya. Tidak ada notifikasi yang dikirim ke siapa pun.

Ini masuk akal untuk MVP dan tidak menimbulkan risiko keselamatan langsung, karena tidak ada
aturan yang menuntut seseorang bertindak dalam hitungan menit. Tetapi keputusan "tidak ada
notifikasi" itu **tidak pernah dinyatakan**; ia hanya tidak dibahas. Klasifikasi: `PROPOSED` /
`NON_BLOCKING_STANDARD` — usulkan menyatakannya eksplisit supaya pembaca berikutnya tahu itu
pilihan sadar, bukan kelupaan.

### 4.16 Dimensi 16 — Dampak billing dan charge

**Status: `CONFIRMED` sebagai deferral yang disadari, dengan satu catatan.**

Yang sudah diputuskan: kelas yang ditagihkan selalu mengikuti kamar yang ditempati
(`RWI-RULE-007`); perubahan kelas disimpan sebagai riwayat; pasien titipan dikeluarkan dari MVP
sehingga tidak ada kelas hak yang terpisah; kelayakan keuangan memblokir penutupan dan ditandai
manual sementara (`RWI-RULE-028`).

Yang ditunda dengan alasan yang jelas: tagihan berjalan, deposit, estimasi biaya, cek manfaat
penjamin, dan klaim, semuanya menunggu `BillingManagement` operasional.

Catatan yang perlu diketahui pemilik: baseline pasal 12 menyebut charge kamar per hari sebagai
kepedulian utama rawat inap, dan `MstPatientClass` di source sudah punya kolom
`DefaultDailyRoomRate`. Karena tagihan berjalan ditunda, **tidak ada satu pun charge kamar yang
tercatat selama MVP**. Konsekuensinya: data lama dirawat dan riwayat kelas yang dihasilkan MVP
harus cukup untuk merekonstruksi charge kamar di kemudian hari. `RWI-RULE-007` dan `RWI-RULE-019`
sudah menyediakan keduanya, jadi rekonstruksi itu mungkin dilakukan. Klasifikasi: `CONFIRMED`
dengan catatan, tidak memblokir.

### 4.17 Dimensi 17 — Dampak keselamatan klinis

**Status: `CONFLICT` pada satu butir, `MISSING` pada satu butir lain.**

**Butir `CONFLICT` — isolasi dan pemisahan jenis kelamin.** `RWI-DEC-018` memilih keduanya tetap
berupa penyaring pencarian, bukan aturan yang menolak penempatan. Artinya sistem mengizinkan
pasien yang butuh isolasi ditempatkan di kamar biasa berisi pasien lain, dan mengizinkan pasien
laki-laki dan perempuan sekamar. Dokumen keputusan sendiri menandai ini sebagai gerbang keras dan
menolak menaikkannya ke `approved`.

Pertentangannya: `RWI-FACT-009` menunjukkan PRD memang menulisnya sebagai penyaring opsional,
sementara baseline `ID-INP-CAP-003` dan pasal 13 memperlakukan kebutuhan isolasi sebagai kendala
penempatan, bukan preferensi pencarian. Keduanya sumber yang berbeda wewenangnya, dan pertentangan
ini tidak dapat diselesaikan tanpa pemilik klinis. Klasifikasi: `CONFLICT` / `BLOCKING`, dicatat
sebagai `DEC-INP-004`.

**Butir `MISSING` — serah terima klinis antar shift.** Baseline `ID-INP-CAP-016` menandai serah
terima tim perawatan sebagai `SAFETY_CHECK`: siapa menyerahkan, siapa menerima, apa isinya, apakah
penerimaan dikonfirmasi, dan tugas apa yang belum tuntas. Dokumen keputusan hanya mengenal serah
terima IGD ke rawat inap, dan sama sekali tidak membahas pergantian jaga perawat, padahal pasien
menginap berhari-hari dan berganti perawat berkali-kali. `RWI-RULE-033` mencatat siapa perawat
penanggung jawab, tetapi tidak mencatat apa yang diserahkan saat berganti. Klasifikasi: `MISSING`
/ `BLOCKING` untuk slice serah terima klinis, dicatat sebagai `DEC-INP-006`. Tidak memblokir
`INP-S04`, karena penugasan perawat tetap dapat dirancang tanpa isi serah terima.

Butir keselamatan lain yang **sudah** tertutup: kepastian identitas pasien lewat kunjungan,
informasi alergi tersedia dan bebas antrean di source, tanggung jawab klinis lewat `RWI-RULE-030`,
dan keselamatan saat perpindahan lewat `RWI-RULE-008` yang mensyaratkan pasien tidak pernah
tercatat tanpa tempat tidur.

### 4.18 Dimensi 18 — Pelaporan dan traceability

**Status: `MISSING` sebagian.**

Yang sudah ada: laporan penutupan tanpa kelayakan keuangan, tiga daftar pantau kepatuhan, laporan
selisih tempat tidur, dan riwayat status yang dapat ditelusuri.

Yang belum: pelaporan wajib ke luar rumah sakit. Baseline `ID-INP-REG-001` menyebut informasi
klinis rawat inap menjadi bagian rekam medis elektronik yang diatur regulasi, dan `ID-INP-INT-005`
menyebut pengiriman resume medis. Keduanya belum pernah dibahas. Ini bagian dari `DEC-INP-005`.

---

## 5. Klasifikasi bukti dan dampak gap

### 5.1 Butir `CONFIRMED` yang menopang kesiapan

| Butir | Bukti |
| --- | --- |
| Batas scope dan 18 kemampuan MUST | `RWI-DEC-004`, `RWI-DEC-005` |
| Model status episode dan tabel perpindahannya | `RWI-RULE-003` |
| Pemesanan tempat tidur, batas waktu, dan kedaluwarsa saat dibaca | `RWI-RULE-001`, `RWI-RULE-002` |
| Sumber kebenaran penghunian tempat tidur | `RWI-RULE-027` |
| Perpindahan utuh dan kewenangannya | `RWI-RULE-006`, `008`, `016`, `030` |
| Lima cara pulang | `RWI-RULE-011` |
| Gerbang keuangan dan sumber sementaranya | `RWI-RULE-009`, `RWI-RULE-028` |
| Daftar periksa administrasi yang diatur admin | `RWI-RULE-018` |
| Riwayat status yang tidak dapat diubah | `RWI-RULE-031` |
| Pemakaian ulang tanpa duplikasi entity | `01-existing-capability-map.md` bagian 14.4 |

### 5.2 Butir `PROPOSED`, `MISSING`, dan `CONFLICT` beserta dampaknya

| No | Butir | Status | Dampak | Slice terdampak | Decision ID |
| ---: | --- | --- | --- | --- | --- |
| 1 | ~~Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` atas pelonggaran antrean dan konsultasi~~ | ~~`MISSING`~~ → **`CONFIRMED`** | ~~`BLOCKING`~~ → **tidak memblokir** | `INP-S05`, `INP-S06` | `DEC-INP-001` **TERTUTUP** 2026-08-21 lewat `RWI-DEC-062`; dicatat di sini 2026-09-02 |
| 2 | Persetujuan pemilik `EmergencyInstallationManagement` atas serah terima disposisi `RANAP` | `MISSING` | `BLOCKING` | `INP-S09` | `DEC-INP-002` |
| 3 | Persetujuan pemilik privasi dan hukum atas persetujuan umum yang tidak menahan admisi | `PROPOSED` | `BLOCKING` | `INP-S10` | `DEC-INP-003` |
| 4 | ~~Isolasi dan pemisahan jenis kelamin sebagai penyaring, bukan penolak penempatan~~ **TERTUTUP 2026-09-11** | ~~`CONFLICT`~~ `RESOLVED` | ~~`BLOCKING`~~ tidak memblokir | `INP-S11` | `DEC-INP-004` **`CLOSED`** oleh `RWI-DEC-064`, `RWI-DEC-065`, dan `RWI-DEC-101`; dicatat `RWI-DEC-104` |
| 5 | Kepemilikan, isi, dan pemicu pengiriman SATUSEHAT rawat inap | `MISSING` | `BLOCKING` | `INP-S15` | `DEC-INP-005` |
| 6 | Isi dan konfirmasi serah terima klinis antar shift keperawatan | `MISSING` | `BLOCKING` | Slice serah terima klinis, belum masuk daftar kemampuan | `DEC-INP-006` |
| 7 | Aturan klinis pasien meninggal dan pasien kabur | `PROPOSED` | `BLOCKING` | `INP-S07` sebagian | `DEC-INP-007` |
| 8 | Persetujuan pemilik `MasterData` atas pembatasan endpoint ketersediaan bed | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S02` | — |
| 9 | Kepergian fisik pasien sebagai kejadian tersendiri, terpisah dari penutupan | `PROPOSED` | `NON_BLOCKING_STANDARD` | `INP-S02`, `INP-S08` | — |
| 10 | Larangan dua episode rawat inap aktif untuk satu pasien | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S01` | — |
| 11 | Perpindahan yang dicatat sebelum pasien benar-benar berpindah | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S03` | — |
| 12 | Riwayat versi resume pulang | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S07` | — |
| 13 | Masa simpan riwayat status | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S13` | `RWI-OQ-035` |
| 14 | Pernyataan eksplisit bahwa modul ini tidak mengirim notifikasi | `PROPOSED` | `NON_BLOCKING_STANDARD` | Seluruh modul | — |
| 15 | Frekuensi observasi keperawatan dan ambang eskalasi | `MISSING` | `CONFIGURABLE_DEFAULT` | `INP-S05` | — |
| 16 | Hasil penunjang yang masih tertunda saat pasien pulang | `MISSING` | `CONFIGURABLE_DEFAULT` | `INP-S08` | — |
| 17 | Instruksi medis dan keperawatan sebagai order yang ditelusuri | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S05` | — |
| 18 | Obat dan instruksi yang masih aktif setelah pasien pulang | `MISSING` | `NON_BLOCKING_STANDARD` | `INP-S07` | — |
| 19 | Penanggung jawab pengisian data master | `MISSING` | `NON_BLOCKING_STANDARD` | Implementasi | `RWI-OQ-036` |

### 5.3 Kenapa butir nomor 8 sampai 19 tidak memblokir

Alasan singkat masing-masing, supaya keputusan ini dapat diperiksa dan tidak sekadar diterima:

- **Nomor 8.** Yang belum ada hanyalah persetujuan atas pembatasan endpoint milik modul lain.
  Bentuk data Rawat Inap sendiri tidak berubah: catatan penempatan tetap menjadi sumber kebenaran.
  Bila persetujuan tidak didapat, jalan mundurnya jelas — status tempat tidur dihitung dari catatan
  penempatan setiap kali dibaca, bukan disalin. Ini pilihan B yang sudah pernah ditimbang pada
  Closure Pass pertanyaan 2.
- **Nomor 9, 11, 12, 17, 18.** Semuanya menambah kolom atau kejadian baru pada aggregate yang
  pemiliknya sudah jelas. Penambahan semacam itu tidak memindahkan ownership dan tidak mengubah
  makna klinis.
- **Nomor 10.** Berupa invariant yang wajar dan dapat dinyatakan eksplisit tanpa mengubah struktur.
- **Nomor 13.** Masa simpan mengubah kebijakan pengarsipan, bukan bentuk tabelnya.
- **Nomor 14.** Menyatakan ketiadaan notifikasi tidak mengubah apa pun; ia hanya membuat pilihan
  itu terbaca.
- **Nomor 15 dan 16.** Keduanya memang wajar berbeda antar rumah sakit dan antar unit, dan modul
  ini sudah punya dua tempat untuk menampungnya: tabel pengaturan `RWI-RULE-034` dan daftar periksa
  administrasi yang butirnya diatur admin `RWI-RULE-018`.
- **Nomor 19.** Tindakan organisasi pada tahap implementasi, tidak menyentuh desain.

---

## 6. Decision Log

Tujuh Decision ID berikut berasal dari assessment awal. Status current dibaca per entri:
`DEC-INP-001` sudah tertutup; Decision ID lain di luar focused scope revision `1.2` tidak dinilai
ulang. Konflik baru focused scope dicatat sebagai `DEC-INP-008` pada bagian 11.8.

### `DEC-INP-001`

> **TERTUTUP 2026-08-21 lewat `RWI-DEC-062`. Koreksi dokumen ini dicatat 2026-09-02.**
>
> Pemilik `ClinicalManagement` dan `PharmacyManagement` adalah Muhammad Hamzah — pemilik yang sama
> dengan `RWI-DEC-061` — dan persetujuannya **sudah diberikan**. `RWI-OQ-032` ikut tertutup pada
> tanggal yang sama. Tabel di bawah sudah dinormalisasi ke keadaan current; jejak pertanyaan
> aslinya tetap dipertahankan pada baris Pertanyaan.
>
> Akibat hilir yang perlu diketahui pembaca: sejak `RWI-DEC-080` (2026-09-02) dokumentasi klinis
> rawat inap **masuk scope modul**, dan `RWI-DEC-083` memetakannya ke sub-modul `keperawatan/`
> serta `dokter-rawat-inap/`. Yang menahannya sekarang bersifat teknis — *shared inpatient
> clinical context resolver* pada `PRD-RWI-FINAL-001` bagian 30.3 — **bukan** keputusan bisnis.

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik `ClinicalManagement` dan `PharmacyManagement` menyetujui pelonggaran keharusan antrean dan konsultasi, serta pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi, khusus untuk kunjungan bertipe rawat inap? |
| Kemampuan terdampak | `INP-S05` dokumentasi klinis dan visite; `INP-S06` resep dan obat pulang |
| Bukti saat ini | `RWI-DEC-038` dan `RWI-RULE-026` memilih arah pelonggaran; `RWI-DEC-062` memberi persetujuan owner; `RWI-DEC-080` memasukkan capability ke scope; capability map revision `1.3` membuktikan pembatas source masih ada |
| Usulan baseline | Baseline `ID-INP-INT-004` dan `ID-INP-CAP-011` menyatakan ownership Farmasi dan domain klinis lain tidak boleh diduplikasi ke dalam Inpatient. Ini mendukung arah pelonggaran, bukan arah membuat entity tandingan |
| Dampak | Risiko duplikasi ownership sudah ditutup: Rawat Inap dilarang membangun entity dokumentasi atau mesin resep tandingan |
| Pemilik yang dibutuhkan | Muhammad Hamzah, melalui `RWI-DEC-061` dan `RWI-DEC-062` |
| Status | **`CLOSED`** — 2026-08-21 |
| Dampak implementasi atau domain | Tidak lagi menahan `INP-S05/S06`; gap resolver, multiplicity, dan test adalah pekerjaan teknis |

### `DEC-INP-002`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik `EmergencyInstallationManagement` menyetujui bahwa disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap baru, serta menyetujui penanda `ClosesEmergencyVisit` mulai benar-benar dijalankan? |
| Kemampuan terdampak | `INP-S09` serah terima IGD ke rawat inap |
| Bukti saat ini | `RWI-DEC-041` dan `RWI-RULE-029` sudah memilih arahnya. `RWI-TF-017` membuktikan penanda `ClosesEmergencyVisit` selama ini tidak pernah dibaca satu pun alur kerja |
| Usulan baseline | Baseline pasal 8 menyatakan perpindahan internal tidak otomatis berarti encounter baru, dan kebijakan rumah sakit yang menentukan batas episodenya. Ini justru menegaskan keputusan itu memang milik rumah sakit, bukan milik agent |
| Dampak | Bila persetujuan tidak didapat, jangkar episode berpindah ke kunjungan IGD, dan syarat pelonggaran `RWI-RULE-026` harus diperluas menjadi majemuk |
| Pemilik yang dibutuhkan | Pemilik modul `EmergencyInstallationManagement`: **Rizki Gunawan**, ditetapkan `RWI-DEC-069` 2026-08-24. Persetujuan formalnya belum tercatat; jawabannya sudah tersedia pada `IGD-DEC-067` yang masih `draft` |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S09` berhenti. `INP-S01` tetap boleh berjalan untuk jalur pasien datang langsung dan poliklinik |

### `DEC-INP-003`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik keamanan, privasi, dan hukum menerima bahwa persetujuan umum rawat inap wajib ada tetapi **tidak** menahan admisi, sehingga ada jeda ketika pasien sudah dirawat tanpa persetujuan tertulis? |
| Kemampuan terdampak | `INP-S10` persetujuan umum rawat inap |
| Bukti saat ini | `RWI-DEC-035` dan `RWI-RULE-025` sudah menuliskan pilihannya, dan dokumen keputusan sendiri menolak menaikkannya ke `approved` karena berada di area privasi dan hukum |
| Usulan baseline | Baseline `ID-INP-CAP-002` menempatkan persetujuan sebagai prasyarat pra-admission yang perlu diverifikasi, bukan sebagai syarat penutupan |
| Dampak | Menyentuh kewajiban hukum dan perlindungan data pasien. Bila jawabannya berubah menjadi "menahan admisi", alur admisi ikut berubah |
| Pemilik yang dibutuhkan | Pemilik keamanan dan privasi; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S10` berhenti. Butir persetujuan pada daftar periksa administrasi dapat dinonaktifkan admin, sehingga `INP-S08` tetap boleh berjalan |

### `DEC-INP-004`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah kebutuhan isolasi dan pemisahan jenis kelamin hanya menjadi penyaring pencarian tempat tidur, atau menjadi aturan yang menolak penempatan? |
| Kemampuan terdampak | `INP-S11` penempatan menurut jenis kelamin dan isolasi |
| Bukti saat ini | `RWI-DEC-018` memilih "penyaring saja" dan tidak dapat naik ke `approved`. `RWI-FACT-009` menunjukkan PRD memang menulisnya sebagai penyaring opsional |
| Usulan baseline | Baseline `ID-INP-CAP-003` menempatkan kebutuhan isolasi dan kendala jenis kelamin sebagai pembatas penempatan, dan pasal 13 memasukkan isolasi ke dalam daftar kepedulian keselamatan klinis. Observasi ini `REFERENCE_ONLY` dan bukan kebijakan rumah sakit |
| Dampak | Menyentuh pengendalian infeksi dan privasi pasien. Bila menjadi aturan keras, penempatan mendapat validasi baru yang dapat menolak |
| Pemilik yang dibutuhkan | Pemilik klinis untuk isolasi; pemilik privasi untuk jenis kelamin. Keduanya belum ditunjuk |
| Status | ~~`OPEN`~~ **`CLOSED` 2026-09-11.** Dijawab `RWI-DEC-064` dan `RWI-DEC-065` yang keduanya `approved` 21 Agustus 2026, lalu dipersempit `RWI-DEC-101` 11 September 2026 yang mencabut bagian kamar tanpa mengembalikannya menjadi penyaring. Penutupannya dicatat `RWI-DEC-104` |
| Dampak implementasi atau domain | ~~`INP-S11` berhenti.~~ **Tidak lagi berlaku.** `INP-S11` naik menjadi `READY_FOR_DOMAIN_DESIGN` sejak 2026-09-11. `INP-S01` dan `INP-S02` tetap berjalan sebagaimana sebelumnya |

### `DEC-INP-005`

| Field | Isi |
| --- | --- |
| Pertanyaan | Siapa pemilik pengiriman data rawat inap ke SATUSEHAT, data apa yang wajib dikirim, kapan pengiriman dipicu, dan **di mana riwayat lokasi pasien disimpan** — pada catatan penempatan milik Rawat Inap, atau pada kunjungan milik Registrasi? |
| Kemampuan terdampak | `INP-S15` interoperabilitas dan pelaporan. Berpotensi menyentuh `INP-S02` |
| Bukti saat ini | Kata SATUSEHAT tidak muncul sama sekali pada 2.163 baris dokumen keputusan. PRD baris 814 menyebutnya dan menyatakan perubahan lokasi perlu direpresentasikan sebagai histori location di dalam encounter. `RWI-DEC-039` menempatkan riwayat lokasi pada catatan penempatan milik Rawat Inap. Capability map membuktikan kunjungan hari ini hanya punya satu kolom `RoomId` tanpa riwayat |
| Usulan baseline | Baseline `ID-INP-INT-001`, `ID-INP-INT-002`, dan `ID-INP-INT-005` seluruhnya menandai topik ini `integration_relevance: HIGH` dan `audit_relevance: HIGH`. Baseline juga memperingatkan agar Encounter tidak dipetakan satu lawan satu ke satu tabel setempat hanya karena FHIR merepresentasikannya sebagai satu resource |
| Dampak | Menentukan pemilik data riwayat lokasi. Bila jawabannya "pada kunjungan", ownership berpindah dari Rawat Inap ke Registrasi, dan itu perubahan mahal bila baru ketahuan setelah desain jadi |
| Pemilik yang dibutuhkan | Pemilik produk bersama pemilik integrasi dan pemilik rekam medis |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S15` berhenti. `INP-S01` dan `INP-S02` boleh berjalan **dengan syarat** catatan penempatan dirancang sebagai riwayat yang dapat dibaca ulang, bukan sekadar penanda keadaan terakhir |

### `DEC-INP-006`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apakah serah terima klinis antar shift keperawatan wajib direkam sistem? Bila ya: siapa menyerahkan, siapa menerima, apa isi minimalnya, apakah penerimaan harus dikonfirmasi, dan bagaimana tugas yang belum tuntas diteruskan? |
| Kemampuan terdampak | Slice serah terima klinis yang belum masuk daftar 18 kemampuan MUST |
| Bukti saat ini | Dokumen keputusan hanya mengenal serah terima IGD ke rawat inap. Pergantian jaga perawat tidak dibahas sama sekali, padahal `RWI-RULE-033` mengakui perawat penanggung jawab bisa berganti di tengah episode |
| Usulan baseline | Baseline `ID-INP-CAP-016` menandainya `SAFETY_CHECK` dan meminta verifikasi pihak penyerah, pihak penerima, isi, konfirmasi penerimaan, tugas yang belum tuntas, informasi kritis, dan waktu berlakunya |
| Dampak | Menyentuh keselamatan pasien. Informasi kritis yang tidak diserahkan adalah penyebab insiden yang lazim di bangsal |
| Pemilik yang dibutuhkan | Pemilik klinis dan pemilik keperawatan; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | Slice serah terima klinis berhenti. `INP-S04` penugasan perawat tetap boleh berjalan, karena mencatat siapa perawatnya tidak bergantung pada isi serah terima |

### `DEC-INP-007`

| Field | Isi |
| --- | --- |
| Pertanyaan | Apa aturan klinis untuk pasien meninggal dan pasien kabur: siapa yang mencatat, dokumen apa yang wajib, apakah resume pulang tetap wajib, kapan tempat tidur dilepas, dan pelaporan apa yang mengikutinya? |
| Kemampuan terdampak | `INP-S07` untuk dua dari lima cara pulang |
| Bukti saat ini | `RWI-DEC-017` mengakui lima cara pulang dan `approved` untuk keputusan produknya, tetapi baris meninggal dan kabur secara tegas dinyatakan **tetap terbuka secara klinis** |
| Usulan baseline | Baseline `ID-INP-CAP-018` menyebut pulang atas permintaan sendiri, transfer keluar, dan meninggal sebagai jalur yang wewenangnya harus eksplisit, dan memperingatkan agar wewenang pemulangan tidak disimpulkan dari praktik umum |
| Dampak | Menyentuh rekam medis, pelaporan wajib, dan dokumen hukum. Pasien meninggal juga memicu surat keterangan kematian yang bentuknya berbeda |
| Pemilik yang dibutuhkan | Pemilik klinis; belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi atau domain | `INP-S07` berjalan hanya untuk tiga cara pulang: atas izin DPJP, atas permintaan sendiri, dan dirujuk. Dua sisanya berhenti |

---

## 7. Kesiapan per slice

| Slice | Kesiapan | Decision ID pemblokir | Catatan |
| --- | --- | --- | --- |
| `INP-S01` Admisi dan pemesanan bed | `READY_FOR_DOMAIN_DESIGN` | — | Hanya untuk jalur pasien datang langsung dan poliklinik. Jalur IGD menunggu `DEC-INP-002` |
| `INP-S02` Penempatan, census, lama dirawat | `READY_FOR_DOMAIN_DESIGN` | — | Dengan syarat catatan penempatan dirancang sebagai riwayat yang dapat dibaca ulang, lihat `DEC-INP-005` |
| `INP-S03` Perpindahan dan pindah kelas | `READY_FOR_DOMAIN_DESIGN` | — | Lengkap termasuk kewenangan per pasien |
| `INP-S04` Penugasan perawat | `READY_FOR_DOMAIN_DESIGN` | — | Isi serah terima antar shift terpisah, lihat `DEC-INP-006` |
| `INP-S05` Dokumentasi klinis, penunjang, dan visite | `PARTIALLY_READY` | `DEC-INP-008`, hanya `CAP-025` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, dan `CAP-024` siap. Definisi visite masih konflik; lihat bagian 11 |
| `INP-S06` Resep dan obat pulang | `READY_FOR_DOMAIN_DESIGN` | — | Ownership dan kontrak target sudah dikunci; gap source `Extend` tidak menjadi keputusan bisnis |
| `INP-S07` Keputusan pulang dan resume | `PARTIALLY_READY` | `DEC-INP-007` | Tiga cara pulang siap; meninggal dan kabur berhenti |
| `INP-S08` Clearance dan penutupan | `PARTIALLY_READY` | `DEC-INP-007` lewat `INP-S07` | Mesin penutupan siap. Yang menunggu hanya syarat penutupan untuk dua cara pulang yang terblokir |
| `INP-S09` Serah terima IGD | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-002` | — |
| `INP-S10` Persetujuan umum | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-003` | — |
| `INP-S11` Jenis kelamin dan isolasi | ~~`BUSINESS_DECISION_REQUIRED`~~ **`READY_FOR_DOMAIN_DESIGN`** sejak 2026-09-11 | ~~`DEC-INP-004`~~ — | ~~Satu-satunya butir berstatus `CONFLICT`~~ Pemblokirnya tertutup; lihat `RWI-DEC-104`. Aturan yang berlaku kini `RWI-DEC-101`: jenis kelamin dinilai hanya terhadap penanda tempat tidur |
| `INP-S12` Bayi baru lahir dan boks bayi | `READY_FOR_DOMAIN_DESIGN` | — | Master sudah punya seluruh penanda yang dibutuhkan |
| `INP-S13` Riwayat status, audit, daftar pantau | `READY_FOR_DOMAIN_DESIGN` | — | Dua dari tiga daftar pantau siap; daftar pantau kepatuhan pengkajian dan CPPT menunggu `INP-S05` |
| `INP-S14` Pengaturan admin | `READY_FOR_DOMAIN_DESIGN` | — | — |
| `INP-S15` Interoperabilitas dan pelaporan | `BUSINESS_DECISION_REQUIRED` | `DEC-INP-005` | Slice ini belum pernah masuk daftar kemampuan mana pun |
| **`INP-S16` Keperawatan rawat inap** | **`PARTIALLY_READY`** | — | **Dinilai revision `1.4`, lihat bagian 13.** `CAP-012`, `CAP-013`, dan `CAP-014` siap; `CAP-027` siap hanya pada bagian skrining/rujukan; `CAP-016` `DEFERRED` oleh `RWI-DEC-089`. Nol blocker keputusan bisnis |

### 7.1 Dependency antar slice

| Slice | Bergantung pada | Sifat ketergantungan |
| --- | --- | --- |
| `INP-S02` | `INP-S01` | Penempatan hanya terjadi setelah admisi diaktifkan |
| `INP-S03` | `INP-S02` | Perpindahan menutup satu penempatan dan membuka penempatan lain |
| `INP-S07` | `INP-S05` | Isi resume merujuk diagnosis dan tindakan. Struktur resume tetap dapat dirancang lebih dulu karena resume menyimpan salinannya sendiri |
| `INP-S08` | `INP-S07` | Penutupan menuntut resume tertandatangani |
| `INP-S08` | `INP-S06` | Butir "obat pulang sudah diserahkan" dapat dinonaktifkan admin, sehingga ketergantungan ini **tidak** memblokir |
| `INP-S12` | `INP-S01`, `INP-S02` | Bayi mendapat episode dan penempatan sendiri |
| `INP-S13` | `INP-S05` | Hanya untuk satu dari tiga daftar pantau |
| `INP-S15` | `INP-S02` | Riwayat lokasi menjadi bahan pengiriman |

---

## 8. Apa yang boleh berjalan dan apa yang harus berhenti

### 8.1 Boleh diteruskan ke `hospital-domain-architect`

Delapan slice penuh berikut dinyatakan **independen** dari penilaian `PARTIALLY_READY` dan boleh
dirancang arsitektur domainnya sekarang:

`INP-S01`, `INP-S02`, `INP-S03`, `INP-S04`, `INP-S06`, `INP-S12`, `INP-S13`, `INP-S14`

ditambah bagian siap `INP-S05` (`CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024`) serta
bagian `INP-S07` dan `INP-S08` yang menyangkut tiga cara pulang: atas izin DPJP, atas permintaan
sendiri, dan dirujuk.

Slice operasional lama tetap membentuk perjalanan episode yang utuh:

`admisi → pesan bed → tempatkan → census → tugaskan perawat → pindah bila perlu → putuskan pulang
→ resume → daftar periksa → kelayakan keuangan → tutup episode → bed kembali kosong`

Focused reassessment menambahkan perjalanan klinis independen yang juga siap dirancang:

`census/episode dokter → kajian medis/SOAP/CPPT → resep/tindakan/penunjang → timeline dan status`

`Physician Visit` sengaja tidak dimasukkan ke rantai kedua sampai `DEC-INP-008` selesai.

### 8.2 Harus berhenti

Baris di luar `CAP-025` dipertahankan dari assessment awal dan tidak dinilai ulang oleh revision
`1.2`; status current-nya wajib diperiksa terhadap decision log terbaru sebelum dipakai.

| Yang berhenti | Alasan |
| --- | --- |
| `INP-S05`, hanya `CAP-025 Physician Visit` | `DEC-INP-008`: keputusan lama menurunkan visite dari SOAP/CPPT, sedangkan PRD final menuntut event mandiri; keduanya menghasilkan persistence, lifecycle, dan hitungan berbeda |
| `INP-S09` | `DEC-INP-002` menentukan kunjungan mana yang menjadi jangkar episode |
| `INP-S10` | `DEC-INP-003` adalah keputusan hukum dan privasi |
| `INP-S11` | ~~`DEC-INP-004` adalah satu-satunya `CONFLICT`, dan menyentuh pengendalian infeksi~~ **Tertutup 2026-09-11** lewat `RWI-DEC-104`. Pengendalian infeksi tetap dijaga `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED`, yang **tidak** tersentuh pencabutan aturan kamar |
| `INP-S15` | `DEC-INP-005` menentukan pemilik data riwayat lokasi |
| Serah terima klinis antar shift | `DEC-INP-006`, kemampuan ini bahkan belum masuk daftar 18 MUST |
| Cara pulang meninggal dan kabur | `DEC-INP-007` |

### 8.3 Syarat yang harus dibawa ke arsitektur domain

Dua syarat berikut wajib dipatuhi arsitek domain supaya slice yang berjalan tidak perlu dibongkar
ketika keputusan yang terbuka akhirnya turun:

1. **Catatan penempatan tempat tidur dirancang sebagai riwayat yang dapat dibaca ulang**, lengkap
   dengan waktu mulai dan waktu berakhir setiap penempatan, bukan sekadar penanda keadaan
   terakhir. Ini menjaga agar `DEC-INP-005` dapat dijawab ke arah mana pun tanpa membongkar
   `INP-S02`.
2. **Pemeriksaan kelayakan penempatan dirancang sebagai satu titik yang dapat diisi aturan
   tambahan**, bukan sebagai daftar syarat yang ditanam mati. Ini menjaga agar `DEC-INP-004` dapat
   berubah dari penyaring menjadi penolak tanpa membongkar `INP-S01` dan `INP-S02`.

---

## 9. Keterbatasan penilaian ini

1. **Tidak ada SOP rumah sakit yang disahkan.** Aturan bisnis berasal dari keputusan owner dan
   `PRD-RWI-FINAL-001`. Gerbang ini menilai kelengkapan untuk desain, bukan sign-off kelembagaan
   atau klinis untuk produksi.
2. **Owner produk sudah bernama, owner governance belum lengkap.** Muhammad Hamzah memegang
   Product/Domain dan tiga modul tetangga yang disebut `RWI-DEC-062`; Clinical Governance,
   Security/Privacy, serta owner kontrak penunjang masih menjadi gerbang produksi.
3. **Baseline rumah sakit Indonesia berstatus `Reference coverage: PARTIAL`.** Ketiadaan sebuah
   topik di dalam baseline **tidak** boleh dibaca sebagai bukti bahwa topik itu tidak dibutuhkan.
4. **Gerbang ini tidak membuka database dan tidak menjalankan aplikasi.** Focused facts memakai
   capability map revision `1.3`; fakta slice lain yang tidak dipindai ulang tetap historis.
5. **Regulasi tidak diverifikasi ulang.** Observasi `ID-INP-REG-001` berstatus
   `VERIFY_CURRENT_REGULATION`, dan gerbang ini tidak memeriksa apakah regulasi yang dirujuk masih
   berlaku.

---

## 10. Handoff

### 10.1 Yang dikirim ke `hospital-domain-architect`

| Field | Nilai |
| --- | --- |
| Modul | `InPatientManagement` / Rawat Inap, prefix `Inp` |
| Slice yang dikirim | Slice lama yang sudah siap, ditambah `INP-S06` seluruhnya dan bagian `INP-S05` untuk `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| Klasifikasi kesiapan | `PARTIALLY_READY` dengan slice siap yang dinyatakan eksplisit independen |
| Revision bukti | Decision log revision `7`; capability map revision `1.3`; focused reassessment revision `1.2` |
| Source SHA | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| Decision ID scope dokter | `DEC-INP-001` **CLOSED**; `DEC-INP-008` **OPEN**, hanya `CAP-025` |
| Baseline observation ID yang dipakai | `ID-INP-INT-001` s.d. `005`, `ID-INP-REG-001`, `ID-INP-CAP-001` s.d. `020`, seluruhnya `REFERENCE_ONLY` |
| Syarat yang wajib dipatuhi | Ownership `ClinicalManagement`/`PharmacyManagement`/modul penunjang; konteks episode tanpa antrean; tidak membuat engine atau tabel tandingan; policy klinis yang belum final tetap configurable |
| Keluaran hilir yang diharapkan | Amendment arsitektur domain untuk capability siap: ownership konsep, relasi episode, lifecycle, authorization, audit, integrasi, billing, dan keselamatan klinis; `CAP-025` dikecualikan sampai `DEC-INP-008` selesai |

### 10.2 Yang dikembalikan ke `/grill-me`

Untuk focused scope Dokter Rawat Inap, hanya `DEC-INP-008` yang perlu ditutup: apakah visite tetap
diturunkan dari catatan perkembangan dan dihitung satu per dokter per tanggal, atau menjadi event
mandiri yang dapat dicatat tanpa SOAP sebagaimana `PRD-RWI-FINAL-001`.

`DEC-INP-002` s.d. `DEC-INP-007` berada di luar focused reassessment ini. Status historisnya tidak
diubah oleh revision `1.2`; masing-masing harus dibaca bersama keputusan yang lebih baru sebelum
slice terkait dilanjutkan.

### 10.3 Skill berikutnya

| Urutan | Skill | Untuk apa |
| ---: | --- | --- |
| 1 | `/hospital-domain-architect` amendment | Merancang batas domain untuk enam capability dokter yang siap, tanpa `CAP-025` |
| 2 | `/grill-me` Amendment Pass | Menutup `DEC-INP-008` definisi dan perhitungan visite |
| 3 | `/hospital-domain-architect` amendment lanjutan | Menyerap `CAP-025` setelah keputusan visite turun |
| 4 | `/design-business-module` | Setelah arsitektur domain slice target berstatus siap |

Urutan 1 dan 2 dapat berjalan bersamaan. Enam capability siap tidak bergantung pada bentuk
`Physician Visit`; `design-business-module` tetap menunggu arsitektur domain amendment agar tidak
mengarang batas aggregate dan ownership yang sebelumnya sengaja tidak dirancang.

---

## 11. Focused reassessment Dokter Rawat Inap — revision `1.2`

Bagian ini adalah hasil kanonis terbaru **hanya untuk** `INP-S05` bagian dokter, `INP-S06`,
`CAP-015`, `CAP-020` s.d. `CAP-025`, serta dependency langsungnya. Bagian 1–10 tetap menjadi
jejak assessment awal; bila ada perbedaan pada scope ini, bagian 11 yang berlaku.

### 11.1 Scope dan pertanyaan gerbang

| Field | Nilai |
|---|---|
| Modul / sub-modul | `InPatientManagement` / `dokter-rawat-inap` |
| Slice | `INP-S05` bagian dokter dan `INP-S06` |
| Capability | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024`, `CAP-025` |
| Pertanyaan | Apakah requirement bisnisnya cukup lengkap untuk amendment arsitektur domain tanpa mengarang ownership, lifecycle, authorization, integrasi, billing, atau keselamatan klinis? |
| Tidak dinilai ulang | Slice keperawatan pada `INP-S05`, slice Rawat Inap lain, kesiapan implementasi/runtime, dan kesiapan produksi |

### 11.2 Bukti yang dipakai

| Bukti | Wewenang | Pemakaian |
|---|---|---|
| `00-interview-decisions.md` revision `7`, hash `e9f2c957…` | Keputusan owner | `RWI-DEC-038`, `062`, `070`, `080`–`083`, serta keputusan visite lama `025` dan `031` |
| `PRD-RWI-FINAL-001` v1.0.0, hash `fb5e75d7…` | Baseline requirement terkini yang diterima lewat `RWI-DEC-080` | Shared Physician Workspace, ketujuh capability, source of truth, RBAC, audit, integrasi, dan production gates |
| Capability map revision `1.3`, hash `0155b345…` | Fakta implementasi saat ini | Status `Ready to reuse`, `Reuse with adapter`, `Extend`, `Repair`, `Missing`, dan `Conflict` pada source |
| Requirement gate revision `1.1` | Jejak assessment lama | Membuktikan `DEC-INP-001` semula menahan `INP-S05/S06` dan kemudian sudah ditutup |
| Hospital domain architecture revision `0.1`, hash `721268f1…` | Arsitektur domain existing | Membuktikan `INP-S05/S06` sengaja belum dirancang; bukan sumber untuk mengarang batas baru |
| Baseline Indonesia pada assessment awal | `REFERENCE_ONLY` | Tidak ada observasi baru yang dinaikkan menjadi requirement pada focused reassessment ini |

### 11.3 Hasil penilaian 18 dimensi

| ID | Dimensi | Status bukti | Hasil focused assessment | Dampak gap |
|---:|---|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Dokter bekerja dari satu konteks episode untuk dokumentasi, resep, tindakan, visite, dan penunjang tanpa antrean semu | — |
| 02 | Aktor | `CONFIRMED` | Physician dan DPJP memiliki kewenangan klinis; profession lain hanya pada CPPT sesuai scope. Administrative attestation visite tidak berlaku sampai ada policy eksplisit | Sign-off peran rinci tetap gerbang produksi |
| 03 | Pemicu/prasyarat | `CONFIRMED` | Episode aktif, patient/encounter cocok, lokasi dan DPJP/assignment authority tersedia | Resolver source masih `Missing`, tetapi ini gap teknis |
| 04 | Alur utama | `CONFIRMED` | Buka census/episode → lihat konteks → tulis/finalkan dokumen atau order → baca status/hasil dari owner → tampilkan timeline | — |
| 05 | Alternatif/exception | `CONFLICT` hanya `CAP-025` | Mismatch episode, episode tertutup, duplicate submit, downstream gagal, dan unauthorized sudah dijelaskan. Bentuk visite sendiri bertentangan antar bukti | `BLOCKING` untuk `CAP-025`; capability lain tidak tertahan |
| 06 | Data minimum | `CONFIRMED` | Patient, encounter, episode, author/role, waktu klinis, isi capability, status, dan correlation/idempotency tersedia per requirement | — |
| 07 | Aturan/validation | `CONFIRMED` kecuali `CAP-025` | Tanpa queue/active IGD; banyak SOAP/konsultasi/resep; isolasi episode; hasil final read-only; clinical commit tidak hilang karena billing failure | Konflik visite `BLOCKING` |
| 08 | Status/lifecycle | `CONFIRMED` + `PROPOSED`, kecuali `CAP-025` | Draft/final/amendment dokumen, verifikasi CPPT, fulfillment Farmasi, dan lifecycle order penunjang cukup. Planned/performed tindakan dipertahankan sebagai capability opsional existing, bukan policy wajib | Lifecycle visite menunggu `DEC-INP-008` |
| 09 | Peran/authorization | `CONFIRMED` | RBAC final PRD membedakan Physician, DPJP, profession CPPT, read-only lintas peran, dan controlled override; backend tetap security boundary | Clinical/Security sign-off sebelum produksi |
| 10 | Dependency antarmodul | `CONFIRMED` | Episode milik Rawat Inap; dokumentasi/tindakan milik Clinical; fulfillment milik Pharmacy; hasil milik Lab/Radiology; charge milik Billing | Tidak boleh membuat tabel/engine tandingan |
| 11 | Integrasi | `CONFIRMED` untuk bentuk minimum | Context episode, correlation id, idempotent retry, status failure, dan source of truth downstream dijelaskan | Kontrak API/status final pemilik modul adalah gerbang produksi, bukan domain design |
| 12 | Hasil akhir | `CONFIRMED` | Catatan final/timeline, prescription identifier dan fulfillment, performed procedure/charge reference, serta order/result reference dapat diamati | — |
| 13 | Pembatalan/koreksi | `CONFIRMED` kecuali `CAP-025` | Dokumen final dikoreksi melalui amendment; cancel/reject order tetap mengikuti owner; silent overwrite/hard delete dilarang | Koreksi visite mengikuti keputusan bentuk visite |
| 14 | Audit/histori | `CONFIRMED` | Author, profession, authored/clinical time, finalize/verify/amend, submit/cancel, correlation, dan actor/reason wajib ditelusuri | Payload medis tidak boleh masuk custom logger |
| 15 | Notifikasi | `MISSING` | Requirement hanya mewajibkan daftar pantau CPPT overdue dan integration failure; push notification tidak ditetapkan | `NON_BLOCKING_STANDARD`; jangan mengarang kanal notifikasi |
| 16 | Billing/charge | `CONFIRMED` | Tindakan billable mengirim trigger idempotent; failure billing tidak menghapus record klinis; Rawat Inap hanya membaca status finansial yang diizinkan | Rekonsiliasi menjadi tanggung jawab owner Billing |
| 17 | Keselamatan klinis | `CONFIRMED` untuk desain | Identitas episode, mismatch A/B, allergy/context header, author, finalization, verified result, dan authority merupakan guard wajib | Nilai SLA serta sign-off governance menahan produksi, bukan desain |
| 18 | Pelaporan/traceability | `CONFIRMED` | Timeline, history visite, monitoring CPPT, failure integration, dan audit actor/time/correlation ditetapkan | Bentuk laporan visite menunggu `DEC-INP-008` |

### 11.4 Butir `CONFIRMED`

1. `RWI-DEC-080` memasukkan tujuh capability dokter ke scope; `RWI-DEC-083` memetakannya tanpa
   capability yatim.
2. `RWI-DEC-081` mengunci ownership lintas modul dan melarang tabel dokumentasi `Inp*` tandingan.
3. `DEC-INP-001` tertutup oleh `RWI-DEC-062`; pemilik `ClinicalManagement` dan
   `PharmacyManagement` sudah menyetujui perubahan yang dibutuhkan.
4. Konteks target adalah `PatientId + EncounterId + InpatientEpisodeId + Physician/Assignment
   Authority`; `QueueId` dan active IGD visit bukan prasyarat rawat inap.
5. Konflik frontend antrean rawat jalan adalah ketidaksesuaian implementasi terhadap target yang
   sudah jelas, bukan pilihan bisnis yang masih terbuka.

### 11.5 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Butir | Status | Dampak | Scope |
|---|---|---|---|---|
| `RWI-DOK-RQG-001` | Tidak ada push notification; daftar pantau dan retry list menjadi mekanisme operasional MVP | `PROPOSED` | `NON_BLOCKING_STANDARD` | `CAP-021`, integrasi |
| `RWI-DOK-RQG-002` | Nilai SLA verifikasi CPPT dan policy lintas profesi belum mendapat sign-off Clinical Governance | `MISSING` | `CONFIGURABLE_DEFAULT`; menahan produksi, tidak menahan domain design | `CAP-021` |
| `RWI-DOK-RQG-003` | Kontrak API/status final Pharmacy, Laboratory, dan Radiology belum disetujui owner masing-masing | `MISSING` | `NON_BLOCKING_STANDARD`; target minimum sudah ada, sign-off menahan produksi | `CAP-015`, `CAP-023` |
| `RWI-DOK-RQG-004` | Keputusan lama: visite berasal dari SOAP/CPPT, tanpa event/form tersendiri, satu per dokter per tanggal. PRD final: visite adalah event eksplisit yang dapat ada tanpa SOAP dan memakai idempotency | `CONFLICT` | **`BLOCKING`**; mengubah persistence, lifecycle, audit, hitungan, UI, billing potensial, dan acceptance | `CAP-025` |
| `RWI-DOK-RQG-005` | Apakah tindakan dokter selalu direncanakan lebih dulu tidak dinyatakan; source sudah mendukung planned lalu performed maupun pencatatan langsung performed | `PROPOSED` | `NON_BLOCKING_STANDARD`; pertahankan kedua jalur sebagai kemampuan opsional, jangan hard-code kewajiban planning | `CAP-024` |

### 11.6 Gap teknis bukan keputusan bisnis

| Evidence | Status capability source | Makna bagi requirement gate |
|---|---|---|
| `DOK-TRC-CTX-01` | `Ready to reuse` | Fondasi episode/census/DPJP tersedia |
| `DOK-TRC-INT-01`, `DOK-TRC-VER-01` | `Missing` | Resolver dan bukti otomatis harus dibangun; requirement targetnya tidak ambigu |
| `DOK-TRC-DEF-01`, `DOK-TRC-CAP020` | `Repair` | Defect no-queue perlu repair dan regression test, bukan keputusan owner |
| `DOK-TRC-INT-02`, `CAP015`, `CAP021`, `CAP023`, `CAP024`, `AUTH-01` | `Extend` | Scope extension sudah dibatasi target dan keputusan |
| `DOK-TRC-CAP022`, `DOK-TRC-FE-BASE` | `Reuse with adapter` | Semantik target jelas; bentuk adapter menjadi wewenang desain hilir |
| `DOK-TRC-FE-01` | `Conflict` | Consumer existing harus dirework ke episode/census; tidak boleh mengubah requirement agar cocok dengan antrean rawat jalan |

### 11.7 Penutupan tiga pertanyaan audit sebelumnya

| Pertanyaan | Keputusan gate |
|---|---|
| `RWI-DOK-TRQ-001` — reuse `TrxPatientAssessment` atau tabel lain | **Bukan blocker requirement.** Requirement mengunci Medical Assessment sebagai record/lifecycle berbeda dari SOAP; bentuk persistence adalah keputusan arsitektur hilir dengan ownership tetap `ClinicalManagement` |
| `RWI-DOK-TRQ-002` — owner Clinical Governance | **Gerbang produksi.** Domain design boleh memakai controlled configurable policy tanpa mengarang angka atau approval |
| `RWI-DOK-TRQ-003` — rework atau karantina frontend | **Bukan keputusan requirement.** Target mewajibkan rework ke episode/census; consumer sekarang tidak boleh di-sign-off atau dirilis sebelum sesuai |

### 11.8 Decision Log baru

#### `DEC-INP-008` — Definisi dan perhitungan Physician Visit

| Field | Isi |
|---|---|
| Pertanyaan | Apakah Physician Visit tetap diturunkan dari SOAP/CPPT dan dihitung maksimal satu per dokter per tanggal, atau menjadi event mandiri yang dapat dicatat tanpa SOAP serta memakai idempotency per submission? |
| Kemampuan terdampak | `CAP-025 Physician Visit`; history/timeline dokter dan kemungkinan charge/report visite |
| Bukti saat ini | `RWI-DEC-025`, `RWI-RULE-017`, dan `RWI-DEC-031` memilih turunan catatan; `PRD-RWI-FINAL-001` CAP-025 serta keputusan final nomor 12 memilih event eksplisit |
| Usulan baseline | Gunakan event eksplisit seperti PRD final agar visite tanpa SOAP tetap auditable; keputusan ini **PROPOSED**, bukan jawaban owner |
| Dampak | Menentukan apakah persistence/event baru diperlukan, hubungan dengan SOAP/CPPT, uniqueness/idempotency, lifecycle koreksi, hitungan laporan, dan bentuk UI |
| Pemilik yang dibutuhkan | Muhammad Hamzah sebagai Product/Domain owner; Clinical Governance dan Billing perlu meninjau bila hitungan visite dipakai klinis atau finansial |
| Status | **`OPEN`** |
| Dampak implementasi/domain | `CAP-025` berhenti. Enam capability dokter lain boleh berjalan secara independen |

### 11.9 Kesiapan per capability

| Capability | Slice | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `CAP-015` Supporting Services | `INP-S05` bagian dokter | `READY_FOR_DOMAIN_DESIGN` | — | Owner processing dan source of truth hasil jelas; source perlu extension |
| `CAP-020` SOAP | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Multiple entry, finalization, amendment, dan episode context jelas |
| `CAP-021` CPPT | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Policy value configurable; sign-off menahan produksi |
| `CAP-022` Medical Assessment | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Semantik/lifecycle terpisah dari SOAP jelas; persistence diputuskan di arsitektur |
| `CAP-023` Medication Management | `INP-S06` | `READY_FOR_DOMAIN_DESIGN` | — | Ownership Pharmacy dan discharge medication type jelas |
| `CAP-024` Physician Procedures | `INP-S05` | `READY_FOR_DOMAIN_DESIGN` | — | Konteks, performer, planned/performed, billing trigger, dan idempotency cukup |
| `CAP-025` Physician Visit | `INP-S05` | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-008` | Jangan merancang persistence, lifecycle, endpoint, atau UI visite sebelum keputusan turun |

Hasil turunan: `INP-S06` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S05`
**`PARTIALLY_READY`**; sub-modul `dokter-rawat-inap` secara keseluruhan **`PARTIALLY_READY`**.

### 11.10 Apa yang boleh berjalan dan harus berhenti

**Boleh berjalan:** amendment `hospital-domain-architect` untuk `CAP-015`, `CAP-020`, `CAP-021`,
`CAP-022`, `CAP-023`, dan `CAP-024`, dengan dependency dan production gate tetap terbuka secara
eksplisit.

**Harus berhenti:** domain design dan seluruh artefak hilir khusus `CAP-025`, sampai
`DEC-INP-008` ditutup. Tidak boleh memakai blueprint lama sebagai jawaban karena blueprint itu
sendiri memilih salah satu sisi konflik tanpa Decision ID yang menyelesaikannya.

### 11.11 Handoff terfokus

| Field | Nilai |
|---|---|
| `next_owner_ready_slice` | `hospital-domain-architect` amendment |
| `next_owner_blocked_slice` | `grill-me` Amendment Pass untuk `DEC-INP-008` |
| `requirement_readiness` | `PARTIALLY_READY` |
| `ready_capabilities` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024` |
| `blocked_capability` | `CAP-025` |
| `decision_ids` | `DEC-INP-001 CLOSED`; `DEC-INP-008 OPEN` |
| `source_sha` | Backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`; frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `expected_output` | Amendment domain architecture yang menambah slice physician siap tanpa mengubah konsep episode existing; CAP-025 tetap ditandai belum dirancang |

---

## 12. Penutupan keputusan Physician Visit — revision `1.3`

Bagian ini adalah hasil kanonis terbaru untuk `CAP-025` dan menggantikan status `OPEN`/
`BUSINESS_DECISION_REQUIRED` pada bagian 10.2, 11.5, 11.8, 11.9, 11.10, dan 11.11. Bagian lama
dipertahankan agar pembaca dapat menelusuri alasan keputusan.

### 12.1 Keputusan yang turun

| Decision ID | Keputusan | Status |
|---|---|---|
| `RWI-DEC-084` | Physician Visit adalah event klinis eksplisit. Event dapat ada tanpa SOAP/CPPT, memiliki tautan dokumen opsional, dan duplicate submission dicegah melalui `request id/idempotency key` | `approved` |
| `RWI-DEC-085` | Setiap visite nyata yang dicatat sebagai event berbeda dihitung satu pada riwayat klinis dan laporan operasional. Dua visite pada hari yang sama tetap dua | `approved` |
| `RWI-DEC-025` | Visite diturunkan dari SOAP/CPPT | `superseded` |
| `RWI-DEC-031` | Visite digabung maksimal satu per dokter per tanggal berdasarkan catatan pertama | `superseded` |
| `RWI-OQ-049` | Hitungan dua event visite aktual pada hari yang sama | `closed` oleh `RWI-DEC-085` |
| `DEC-INP-008` | Definisi dan perhitungan Physician Visit | **`CLOSED`** |

### 12.2 Aturan yang dapat diuji

1. Event visite pukul 07:40 tetap muncul pada history walaupun SOAP baru dibuat pukul 07:52 atau
   belum dibuat.
2. SOAP/CPPT tanpa event Physician Visit tidak otomatis menambah visite.
3. Dua visite nyata oleh dokter yang sama pukul 07:40 dan 16:10 menghasilkan dua event dan
   hitungan klinis/operasional dua.
4. Retry dengan `idempotency key` yang sama menghasilkan event yang sama, bukan visite kedua.
5. Billing boleh mengagregasikan dua event menjadi satu tagihan harian hanya melalui kebijakan
   owner Billing yang disetujui terpisah. Agregasi tidak boleh menghapus atau mengubah dua event
   klinis.
6. Koreksi mengikuti prinsip PRD final: tidak ada hard delete atau silent overwrite; actor, waktu,
   alasan, dan perubahan harus dapat diaudit. Detail lifecycle diturunkan pada arsitektur domain,
   bukan dikarang oleh requirement gate.

Bukti acceptance detail: `RWI-AC-150` s.d. `RWI-AC-156` pada decision log revision `8`.

### 12.3 Kesiapan capability setelah keputusan

| Capability | Kesiapan current | Blocker bisnis |
|---|---|---|
| `CAP-015` Supporting Services | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-020` SOAP | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-021` CPPT | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-022` Medical Assessment | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-023` Medication Management | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-024` Physician Procedures | `READY_FOR_DOMAIN_DESIGN` | — |
| `CAP-025` Physician Visit | **`READY_FOR_DOMAIN_DESIGN`** | —; gap source `Missing` tetap pekerjaan teknis |

Hasil turunan:

- sub-modul `dokter-rawat-inap` berstatus **`READY_FOR_DOMAIN_DESIGN`** untuk seluruh tujuh
  capability;
- bagian dokter pada `INP-S05` siap; bagian keperawatan pada slice yang sama tidak dinilai ulang;
- `INP-S06` tetap siap;
- overall Rawat Inap tetap **`PARTIALLY_READY`** karena slice modul lain berada di luar keputusan
  ini dan tidak dinilai ulang.

### 12.4 Handoff current

| Field | Nilai |
|---|---|
| `next_owner` | `hospital-domain-architect` amendment |
| `ready_capabilities` | `CAP-015`, `CAP-020`, `CAP-021`, `CAP-022`, `CAP-023`, `CAP-024`, `CAP-025` |
| `blocked_capability` | — untuk scope Dokter Rawat Inap |
| `decision_ids` | `RWI-DEC-084`, `RWI-DEC-085`; `DEC-INP-008 CLOSED` |
| `required_boundary` | Dokumentasi dan visite tetap milik `ClinicalManagement`; jangan membuat tabel/engine `Inp*` tandingan. Billing hanya menerima dampak/agregasi melalui kontrak terpisah |
| `expected_output` | Amendment arsitektur domain ketujuh capability dokter, termasuk ownership event Physician Visit, relasi episode, lifecycle koreksi, authorization, audit, idempotency, integrasi Billing, dan traceability |

---

## 13. Focused reassessment Keperawatan — revision `1.4`

### 13.1 Kenapa penilaian ini dijalankan

Taksonomi slice `INP-S01` s.d. `INP-S15` pada bagian 4 disusun ketika scope modul masih **18 kemampuan**.
Setelah `RWI-DEC-080` mengangkat `PRD-RWI-FINAL-001` menjadi baseline dan scope menjadi **28 kemampuan**,
lalu `RWI-DEC-082` memecah modul menjadi tiga sub-modul, lima kemampuan Keperawatan tidak pernah mendapat
slice sendiri. Akibatnya terbaca dari dokumen ini sendiri:

| Kemampuan | Berapa kali disebut dokumen ini sebelum revision `1.4` | Akibatnya |
|---|:---:|---|
| `CAP-012` Nursing Assessment | 1 | Masuk daftar `INP-S05`, tetapi **tidak pernah dinyatakan siap** — bagian 7 hanya menyebut `CAP-015`, `020`, `021`, `022`, dan `024` |
| `CAP-013` Nursing Care | **0** | Tidak ada slice mana pun yang mencakupnya |
| `CAP-014` Nursing Interventions | 1 | Sama seperti `CAP-012` |
| `CAP-016` Equipment Usage | 2 | Hanya muncul sebagai catatan, bukan sebagai penilaian |
| `CAP-027` Nutrition Care | **0** | Tidak ada slice mana pun yang mencakupnya |

Bagian 12.3 dokumen ini sudah mencatatnya apa adanya: *"bagian keperawatan pada slice yang sama tidak
dinilai ulang"*. Penilaian `1.4` menutup lubang itu, dan **tidak** menilai ulang slice modul lain.

### 13.2 Scope penilaian

| Field | Nilai |
|---|---|
| Slice baru | **`INP-S16`** — Keperawatan rawat inap |
| Sub-modul | [`keperawatan/`](../keperawatan/), `RWI-BP-001` revision `5` |
| Kemampuan | `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, `CAP-027` — sesuai `RWI-DEC-083` |
| Aturan bisnis terkait | `RWI-RULE-021`, `RWI-RULE-026`, `RWI-RULE-033` |
| Yang **tidak** dinilai | Seluruh slice `INP-S01` s.d. `INP-S15`. Hasilnya tetap sebagaimana revision `1.3` |

### 13.3 Bukti yang dipakai

| Bukti | Revision | Wewenang |
|---|---|---|
| `PRD-RWI-FINAL-001` bagian 16, 17, 20, 22, 26, 27, 29 | v1.0.0 | Requirement rumah sakit terkini — wewenang tertinggi untuk "apa yang seharusnya dibangun" |
| [`00-interview-decisions.md`](../00-interview-decisions.md) | `11` | Keputusan pemilik yang dikonfirmasi |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | `1.3` | Bukti implementasi — **bagian 1–14 stale** terhadap SHA terbaru; dipakai hanya untuk pertanyaan "apa yang ada sekarang" |
| [`../keperawatan/`](../keperawatan/) sebelas artefak desain | `0.2` / `0.2.0` | Bukan bukti requirement; dipakai untuk memeriksa apakah requirement sudah cukup untuk dirancang |

> **Batas yang dijaga.** Keberadaan desain `keperawatan` **bukan** bukti bahwa requirement-nya lengkap.
> Penilaian ini menilai buktinya, bukan dokumen turunannya.

### 13.4 Hasil penilaian 18 dimensi

| ID | Dimensi | Status bukti | Hasil focused assessment | Dampak gap |
|---:|---|---|---|---|
| 01 | Tujuan | `CONFIRMED` | Catatan klinis perawat yang awal dan berkelanjutan sepanjang episode, beserta rencana asuhan, tindakan nyata, dan skrining gizi | — |
| 02 | Aktor | `CONFIRMED` | RBAC PRD bagian 26: Perawat `C/U/F`, Kepala Ruangan `R/O*`, Supervisor `O*`, Dokter dan DPJP `R` | — |
| 03 | Pemicu/prasyarat | `CONFIRMED` | Episode `Admitted` ditambah penugasan/kewenangan perawat — PRD 16.3 | Resolver `INT-KEP-01` masih `Missing`, tetapi itu **gap teknis** |
| 04 | Alur utama | `CONFIRMED` | PRD 16.3 menulis alurnya utuh: `Admitted` → penugasan → pengkajian awal → identifikasi risiko → asuhan/tindakan → pengkajian ulang harian → evaluasi → perencanaan pulang | — |
| 05 | Alternatif/exception | `CONFIRMED` | Pengkajian ulang tidak menimpa pengkajian awal (16.2 aturan 3); nilai nyeri lama tidak ditimpa (aturan 6); pasien salah dibatalkan Kepala Ruangan dengan alasan (PRD 29); kegagalan Billing tidak menghapus catatan klinis (`CAP-014` aturan 5) | — |
| 06 | Data minimum | `CONFIRMED` | PRD 16.2 aturan 4 dan 5 merinci isi General Assessment dan Fall Risk; `CAP-014` aturan 2 merinci tindakan: aksi, waktu, pelaku, hasil, konteks episode | — |
| 07 | Aturan/validation | `CONFIRMED` | Tiga belas aturan `CAP-012`, enam aturan `CAP-013`, lima aturan `CAP-014` | Katalog SDKI bersyarat — lihat 13.6 |
| 08 | Status/lifecycle | `CONFIRMED` | PRD 16.2 aturan 10 menyebut `NotStarted`, `Draft`, `Completed`, `Amended`, dan **melarang** status itu diturunkan dari status episode | — |
| 09 | Peran/authorization | `CONFIRMED` | RBAC bagian 26 ditambah `CAP-014` aturan AC-03: bukan penulis dan bukan supervisor tidak dapat menyunting diam-diam catatan final | — |
| 10 | Dependency antarmodul | `CONFIRMED` | Tabel milik `ClinicalManagement` (`RWI-DEC-081`, PRD 23.1); asuhan gizi milik modul Gizi; tagihan milik Billing | Modul Gizi **belum berwujud** — lihat 13.6 |
| 11 | Integrasi | `CONFIRMED` untuk bentuk minimum | Pemicu tagihan idempotent (`CAP-014` aturan 5); rujukan gizi tanpa menduplikasi konteks pasien (`CAP-027` AC-01) | — |
| 12 | Hasil akhir | `CONFIRMED` | AC-CAP012-03: pengkajian `Completed` tampil pada census/workspace **tanpa** menambah status episode baru | — |
| 13 | Pembatalan/koreksi | `CONFIRMED` | PRD 16.2 aturan 12 dan 13: dilarang hard-delete dan timpa diam-diam; amandemen wajib menyimpan pelaku, waktu, alasan, dan perubahan. PRD 27.3 aturan 7 menyatakan koreksi dokumen klinis **mengikuti aturan amandemen/versi masing-masing jenis dokumen** | Konsistensi dengan mesin keutuhan dokumen `NON_BLOCKING_STANDARD` — lihat 13.6 |
| 14 | Audit/histori | `CONFIRMED` | PRD 27.1 mewajibkan jejak finalisasi/verifikasi/amandemen dokumen klinis; 27.2 merinci isinya termasuk alasan dan nilai sebelum/sesudah | — |
| 15 | Notifikasi | `MISSING` | Requirement hanya menuntut **daftar pantau kepatuhan**, bukan pemberitahuan dorong | `NON_BLOCKING_STANDARD` |
| 16 | Billing/charge | `CONFIRMED` | `CAP-014` aturan 5 dan AC-02: tindakan billable mengirim pemicu beridentitas idempotency; kegagalan Billing **tidak** menghapus catatan klinis | — |
| 17 | Keselamatan klinis | `CONFIRMED` untuk desain | Pemisahan pengkajian awal dan ulang, riwayat nyeri longitudinal, larangan hard-delete, dan kewenangan penyuntingan menutup risiko utama | Nilai batas waktu klinis `CONFIGURABLE_DEFAULT` — lihat 13.6 |
| 18 | Pelaporan/traceability | `CONFIRMED` | Daftar pantau kepatuhan pengkajian, `DueAt`/`CompletedAt`/overdue berdasarkan konfigurasi aktif (16.2 aturan 11) | — |

### 13.5 Butir `CONFIRMED` yang menopang kesiapan

| No | Butir | Bukti |
|---:|---|---|
| 1 | Pengkajian terikat Patient + Encounter + Inpatient Episode, dan **tidak boleh** menuntut `QueueId` rawat jalan maupun kunjungan IGD aktif | PRD 16.2 aturan 1 dan 2; `AC-CAP012-01` |
| 2 | Pengkajian awal dan pengkajian ulang adalah record **terpisah** | PRD 16.2 aturan 3; `AC-CAP012-02` |
| 3 | Status pengkajian diturunkan dari record nyata, bukan dari status episode | PRD 16.2 aturan 10; `AC-CAP012-03` |
| 4 | Rencana asuhan diturunkan dari temuan pengkajian, punya masalah/tujuan/rencana/evaluasi dan lifecycle sendiri, serta menutup butir tanpa menghapus riwayat | PRD `CAP-013` aturan 1, 2, 6; `AC-CAP013-01` s.d. `03` |
| 5 | Tindakan mencatat apa yang **benar-benar dilakukan**, boleh ad-hoc tanpa rencana lebih dulu | PRD `CAP-014` aturan 1 dan 3 |
| 6 | Skrining gizi menghasilkan pemicu rujukan **tanpa** menjadikan perawat pemilik asuhan gizi profesional | PRD 16.2 aturan 7; `CAP-027` aturan 1 dan 3 |
| 7 | Kewenangan per peran lengkap, termasuk larangan penyuntingan diam-diam oleh selain penulis/supervisor | PRD bagian 26; `AC-CAP014-03` |

### 13.6 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Butir | Status | Dampak | Alasannya |
|---|---|---|---|---|
| K-01 | Nilai batas waktu klinis pengkajian (`RWI-RULE-021`) | `MISSING` | **`CONFIGURABLE_DEFAULT`** | PRD 16.2 aturan 11 **secara eksplisit** mewajibkan SLA klinis dapat dikonfigurasi Clinical Governance dan **melarang PRD men-hard-code angka yang belum disetujui**. Mekanismenya wajib ada; angkanya memang konfigurasi. Karena itu butir ini **tidak memblokir desain** |
| K-02 | Katalog terminologi SDKI/SLKI/SIKI | `PROPOSED` | **`CONFIGURABLE_DEFAULT`** | PRD `CAP-013` aturan 3 bersyarat: *"Jika terminology SDKI/SLKI/SIKI digunakan rumah sakit"*. Pemakaiannya belum dinyatakan, dan struktur rencana asuhan tetap dapat dirancang tanpa katalognya |
| K-03 | Konsistensi mesin koreksi dokumen keperawatan terhadap mesin keutuhan dokumen `ClinicalManagement` | `PROPOSED` | **`NON_BLOCKING_STANDARD`** | `RWI-DEC-086`/`087` bercakupan **catatan dokter**; dokumen keperawatan tidak disebut. **PRD 27.3 aturan 7 menyelesaikannya**: koreksi dokumen klinis mengikuti aturan amandemen/versi **masing-masing jenis dokumen**. Model amandemen sendiri karenanya **sah**. Yang tersisa adalah pilihan keseragaman, bukan pertentangan |
| K-04 | Cakupan `CAP-013` terhadap scope MVP | **`CONFLICT`** | **`NON_BLOCKING_STANDARD`** | `RWI-DEC-034` (`approved`, 2026-08-20) menyatakan `CAP-013` berada di **luar scope** dan ditunda setelah MVP, turunan `RWI-DEC-004`. `RWI-DEC-080` (2026-09-02) **menggantikan batas scope itu**, dan `RWI-DEC-083` menugaskan `CAP-013` ke `keperawatan`. Keputusan yang lebih baru dan lebih spesifik menang, tetapi `RWI-DEC-004` dan `RWI-DEC-034` **belum ditandai `superseded`** — lihat 13.8 |
| K-05 | Asuhan gizi ujung ke ujung (`CAP-027` bagian modul Gizi) | `MISSING` | **`BLOCKING` hanya untuk bagian itu** | PRD 23.1 menaruh Nutrition Assessment/Care pada modul Gizi, dan modul itu **belum berwujud** di `Areas/`. Bagian milik Keperawatan — skrining dan rujukan — tidak ikut terblokir |
| K-06 | Pemberitahuan dorong | `MISSING` | `NON_BLOCKING_STANDARD` | Requirement hanya menuntut daftar pantau kepatuhan |

### 13.7 Gap teknis, bukan keputusan bisnis

Dua butir berikut **bukan** bahan gerbang ini dan tidak boleh dipakai untuk menahan kesiapan requirement:

| Butir | Sifatnya | Pemilik |
|---|---|---|
| `INT-KEP-01` *shared inpatient clinical context resolver* — `TrxPatientAssessment` masih menuntut `QueueId` | **Teknis.** Requirement-nya justru sudah tegas: PRD 16.2 aturan 2 melarang `QueueId` diwajibkan | `ClinicalManagement` |
| Modul Gizi berstatus `PLANNED` | **Ketersediaan modul**, bukan keputusan bisnis yang belum diambil | Roadmap Quilvian |

### 13.8 Decision Log

Tidak ada Decision ID **baru** yang memblokir. Satu butir kebersihan decision log perlu ditutup pemilik:

Decision ID: `DEC-INP-009`

| Field | Isi |
|---|---|
| Pertanyaan | Apakah `RWI-DEC-004` dan `RWI-DEC-034` dinyatakan `superseded` oleh `RWI-DEC-080` dan `RWI-DEC-083`, sehingga `CAP-013` resmi berada **di dalam** scope? |
| Kemampuan terdampak | `CAP-013` Nursing Care |
| Bukti saat ini | `RWI-DEC-034` `approved` menyatakan di luar scope; `RWI-DEC-083` `approved` dan lebih baru menugaskannya ke `keperawatan`; `keperawatan/04-prd-to-mvp.md` menulisnya `MUST HAVE` `EPIC KEP-03` |
| Usulan baseline | Keduanya ditandai `superseded` dengan rujukan ke `RWI-DEC-080` dan `RWI-DEC-083`, sesuai disiplin yang sudah dipakai pada `RWI-DEC-018`, `025`, dan `031` |
| Dampak | Hasil bisnis: apakah `EPIC KEP-03` dibangun. Tidak mengubah model domain, lifecycle, maupun authorization |
| Pemilik | Muhammad Hamzah, Product/Domain |
| Status | **`CLOSED` 2026-09-02** oleh `RWI-DEC-090`: `RWI-DEC-004` dan `RWI-DEC-034` dinyatakan `superseded`, `CAP-013` resmi di dalam scope. Ekornya, `OQ-RI-011` terbuka kembali sebagai butir non-blocking |
| Dampak implementasi | Nihil bila ditutup sesuai usulan. Bila pemilik justru menegaskan `CAP-013` tetap di luar scope, `EPIC KEP-03` dicabut dari `keperawatan/04-prd-to-mvp.md` |

### 13.9 Kesiapan per capability

| Capability | Slice | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `CAP-012` Nursing Assessment | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Tiga belas aturan, lima acceptance criteria, status lifecycle, dan kewenangan lengkap. SLA `CONFIGURABLE_DEFAULT` |
| `CAP-013` Nursing Care | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Enam aturan dan tiga acceptance criteria cukup. Katalog SDKI bersyarat dan tidak menahan struktur. `DEC-INP-009` bersifat kebersihan catatan |
| `CAP-014` Nursing Interventions | `INP-S16` | **`READY_FOR_DOMAIN_DESIGN`** | — | Konteks, pelaku, waktu, idempotency, dan pemisahan kegagalan Billing tegas |
| `CAP-016` Equipment Usage | `INP-S16` | **`DEFERRED`** — tidak dinilai | — | Dikeluarkan dari scope rilis pertama secara tertulis oleh `RWI-DEC-089`. Dinilai ulang saat `RWI-OQ-048` dibuka kembali |
| `CAP-027` Nutrition Care | `INP-S16` | **`PARTIALLY_READY`** | — | **Skrining dan rujukan siap** — itulah bagian milik Keperawatan. **Asuhan gizi ujung ke ujung berhenti** karena modul Gizi belum berwujud, dan itu ketersediaan modul, bukan keputusan bisnis |

Hasil turunan: slice **`INP-S16` `PARTIALLY_READY`**; sub-modul `keperawatan` **siap dirancang dan siap
direncanakan untuk empat kemampuan aktifnya**. Overall Rawat Inap tetap **`PARTIALLY_READY`** karena slice
modul lain berada di luar penilaian ini.

### 13.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

- perencanaan delivery `keperawatan` untuk `CAP-012`, `CAP-013`, `CAP-014`, dan bagian skrining/rujukan `CAP-027`;
- desain lanjutan bila diperlukan, dengan atau tanpa `hospital-domain-architect` — sub-modul ini sudah mencatat `DOMAIN_ARCHITECTURE_NOT_RUN` beserta alasannya.

**Harus berhenti:**

- pekerjaan asuhan gizi ujung ke ujung, sampai modul Gizi berdiri;
- pekerjaan `CAP-016`, sampai `RWI-OQ-048` dibuka kembali;
- **pemakaian** kelima kemampuan untuk pasien sungguhan, sampai `INT-KEP-01` dikerjakan. Ini menahan rilis, **bukan** menahan desain maupun perencanaan.

### 13.11 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `CAP-012`, `CAP-013`, `CAP-014`, `CAP-027` bagian skrining/rujukan |
| `requirement_readiness` | `INP-S16` `PARTIALLY_READY`; empat kemampuan aktif `READY_FOR_DOMAIN_DESIGN` |
| `requirement_evidence_status` | `CONFIRMED` untuk 16 dari 18 dimensi; `MISSING` pada notifikasi dan nilai SLA; satu `CONFLICT` kebersihan catatan |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas konteks dan kepemilikan data sudah ditetapkan `RWI-DEC-081` dan PRD 23.1, sehingga tidak ada batas domain yang perlu diturunkan ulang |
| `decision_ids` | `DEC-INP-009` `OPEN` non-blocking; `RWI-OQ-048` `CLOSED` oleh `RWI-DEC-089` |
| `dependency_ids` | `INT-KEP-01` teknis; modul Gizi `PLANNED` |
| `next_owner` | `plan-module-delivery` untuk sub-modul `keperawatan` |
| `required_boundary` | Dokumentasi keperawatan tetap milik `ClinicalManagement` (`RWI-DEC-081`). Sub-modul ini **MUST NOT** membuat tabel tandingan, termasuk tabel pemakaian alat |
| `expected_output` | Roadmap backend dan frontend `keperawatan` beserta traceability requirement, tanpa satu pun task untuk `EPIC KEP-06` |

---

## 14. Focused reassessment penyelarasan `PRD-RWI-V2-001` — revision `1.5`

> **Catatan revision `1.6`.** Status `INP-S17`, `INP-S19`, gap `G-08`, `G-19`, `G-20`, serta
> `DEC-INP-010` s.d. `DEC-INP-012` pada bagian ini **digantikan bagian 15**. Isinya dipertahankan agar
> pembaca dapat menelusuri alasan keputusan. Hasil `INP-S18`, `INP-S20`, dan `INP-S21` tetap berlaku.

### 14.1 Kenapa penilaian ini dijalankan

Fase `RLN-PH-04` pada `blueprint-manifest.md` bagian 0-B.4 mewajibkan gerbang terfokus untuk
**kemampuan baru dan kemampuan yatim** yang dibawa `PRD-RWI-V2-001`, setelah `RLN-PH-02`
(wawancara) dan `RLN-PH-03` (impact scan) selesai. Dua keputusan juga mewajibkannya secara tertulis:

- `RWI-DEC-116` konsekuensi (2): MAR **wajib** melewati gerbang ini sebelum dirancang;
- `RWI-DEC-133` butir (2): rekonsiliasi obat ikut melewati gerbang bersama MAR.

Penilaian ini menutup temuan manifest `RLN-04` (Penunjang Medis melebar) dan `RLN-07` (tujuh isi
Pengkajian Pasien). Slice `INP-S01` s.d. `INP-S16` **tidak** dinilai ulang; hasilnya tetap
sebagaimana revision sebelumnya.

### 14.2 Scope penilaian

| Slice baru | Nama | Sub-modul | Kemampuan dan isi yang dinilai | Keputusan yang mengikat |
|---|---|---|---|---|
| **`INP-S17`** | Pengkajian Pasien V2 | `keperawatan` | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Pengawasan Harian Pasien, Evaluasi Awal (MPP), Perencanaan Pulang, progres | `RWI-DEC-118` s.d. `120`, `124`, `131`, `136`, `141` |
| **`INP-S18`** | Obat keperawatan | `keperawatan` + `PharmacyManagement` | Pemberian obat per dosis (MAR) dan rekonsiliasi obat saat admisi | `RWI-DEC-116`, `117`, `132` s.d. `134` |
| **`INP-S19`** | Kemampuan keselamatan `P1` PRD-to-MVP-V2 | `keperawatan` | Handover shift (`FR-MVP-KEP-015`, `016`), sliding scale (`FR-MVP-KEP-018`), transfusi (`FR-MVP-KEP-019`) | `RWI-DEC-097`, `RWI-DEC-110` |
| **`INP-S20`** | Penunjang Medis enam layanan | `dokter-rawat-inap` + `keperawatan` | Laboratorium, Radiologi, Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-108`, `RWI-DEC-113` |
| **`INP-S21`** | Dokumentasi klinis dan ruang kerja dokter V2 | `dokter-rawat-inap` + `keperawatan` | Kesamaan layout, Catatan Saya, jenis catatan CPPT, koreksi dan penguncian konsep, penulis dan pembatalan tindakan, verifikasi CPPT, Resep Harian, Template Resep, tab Resume Medis dan ODC | `RWI-DEC-107`, `112`, `115`, `121` s.d. `123`, `125` s.d. `130`, `135`, `137` s.d. `144` |

**Yang tidak dinilai:** seluruh isi sub-modul `episode-rawat-inap`, termasuk serah terima klinis
transfer antarunit (`FR-MVP-EP-016`, `017`), karena `RWI-DEC-113` menampilkannya "Integrasi belum
tersedia" dan kepemilikannya ada di sub-modul itu; `CAP-016` Pemakaian Alat (`RWI-DEC-089`,
`RWI-DEC-108`); lima cara keluar (`MVP-RWI-D-011`).

### 14.3 Bukti yang dipakai

| Bukti | Revision / hash | Wewenang dan penggunaannya |
|---|---|---|
| `PRD-RWI-V2-001` v`2.0`, `docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md` bagian 5, 16–23, 26–47, 56 | SHA-256 `2b3b2f29…0a679f` | Requirement rumah sakit terkini untuk ruang kerja dokter dan keperawatan — menang bila bertentangan, sesuai `RWI-DEC-110` |
| `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` bagian 13 dan 16 (`FR-MVP-KEP-001` s.d. `020`, `AC-MVP-025` s.d. `034`, `OPEN-MVP-001` s.d. `010`) | SHA-256 `9804b968…3ce141` | Requirement yang tetap berlaku lewat `RWI-DEC-097` dan `RWI-DEC-110` |
| `PRD-RWI-FINAL-001` v1.0.0 | SHA-256 `fb5e75d7…bddc55` | Baseline 28 kemampuan |
| [`00-interview-decisions.md`](../00-interview-decisions.md) | revision `20`, SHA-256 `b2780135…03db0fb7`, keputusan terakhir `RWI-DEC-144` | Keputusan pemilik yang dikonfirmasi |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | revision `1.4`, SHA-256 `337a10f0…d22daa543a`, bagian 17 current pada `BE@df3679c0` dan `FE@147355f5` | Bukti "apa yang ada sekarang" saja |
| Frontend V1 `QuilvianSystemFrontendDev@13c3a96b` | commit lokal | Bukti legacy isi formulir V1 — wewenang terendah, dipakai sebagai usulan isian |
| Pembacaan source tambahan pada `BE@df3679c0` | — | `TrxPatientAllergy.cs` (data alergi terstruktur), `TrxPatientProcedure.IsBillingGenerated` (tidak pernah disetel `true` di mana pun) |
| Blueprint `bank-darah` | `00-interview-decisions.md`, manifest status `IN_PROGRESS` | Bukti titik sentuh transfusi; tidak dinilai isinya |

> **Batas yang dijaga.** Keputusan pemilik yang lahir dari opsi yang disusun agent tetap
> `CONFIRMED` karena pemilik memilihnya secara tertulis. Usulan agent yang **tidak** dipilih
> pemilik tetap `PROPOSED` pada penilaian ini.

### 14.4 Hasil penilaian 18 dimensi per slice

Kode: `C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `X` = `CONFLICT`. Nomor `G-##` merujuk
tabel 14.6.

| ID | Dimensi | `INP-S17` Pengkajian | `INP-S18` Obat | `INP-S19` P1 keselamatan | `INP-S20` Penunjang | `INP-S21` Dokumentasi dokter |
|---:|---|---|---|---|---|---|
| 01 | Tujuan | `C` PRD 26–34 | `C` PRD 40; `FR-MVP-KEP-009` | `C` `FR-MVP-KEP-015` s.d. `019` | `C` PRD 23, 43 | `C` PRD 7–22 |
| 02 | Aktor | `C` perawat `RWI-DEC-100`; MPP `RWI-DEC-118`, `131` | `C` perawat unit, dokter pemutus rekonsiliasi `RWI-DEC-132` | `C` pemberi/penerima handover; `X` transfusi G-20 | `C` `RWI-DEC-113` | `C` `RWI-DEC-111`, `125`, `139` |
| 03 | Pemicu/prasyarat | `C` episode `Admitted` | `C` resep aktif `RWI-DEC-117`; admisi untuk rekonsiliasi | `M` G-19 | `C` order `CAP-015` | `C` penugasan `RWI-DEC-099`, `128` |
| 04 | Alur utama | `C` PRD 27–34 | `C` `RWI-DEC-116`, `132` | `C` pada PRD-to-MVP-V2; `M` penempatan rilis G-19 | `C` | `C` |
| 05 | Alternatif/exception | `C` `BR-RWI-004`, `007`; `RWI-DEC-119` | `C` `Held`/`Refused`/`Missed` beralasan; obat di luar master `RWI-DEC-134` | `C` `AC-MVP-032`, `033` | `C` "Integrasi belum tersedia" `RWI-DEC-108` | `C` `RWI-DEC-129`, `138`, `143` |
| 06 | Data minimum | `C` bagian dan kelompok PRD; `P` isian rinci G-01; `M` intake obat/darah G-08 | `C` `FR-MVP-KEP-009` s.d. `011`; isian V1 rekonsiliasi | `C` `FR-MVP-EP-017`, `FR-MVP-KEP-015` s.d. `019` | `C` | `C` `RWI-DEC-140`, `144` |
| 07 | Aturan/validation | `C` `RWI-DEC-124`, `136`, `141`; `M` isi klinis G-02 | `C`; `M` jam standar dan jendela `Missed` G-12 | `C`; `M` protokol sliding scale G-19 | `C` | `C` |
| 08 | Status/lifecycle | `C` `RWI-DEC-119`, `120`; `M` status rencana pulang G-10 | `C` enam status dosis `FR-MVP-KEP-010` | `C` snapshot handover immutable `FR-MVP-KEP-016` | `C` | `C` `RWI-DEC-138`, `143`, `144` |
| 09 | Peran/authorization | `C` `RWI-DEC-100`, `118`, `131`, `136`; `P` penanda MPP G-11 | `C` `RWI-DEC-117` butir (3); cek ganda pengguna kedua | `X` transfusi G-20 | `C` | `C` `RWI-DEC-125`, `127`, `128`, `135`, `139`, `143` |
| 10 | Dependency antarmodul | `C` `ClinicalManagement` `RWI-DEC-081`, `118` | `C` `PharmacyManagement` `RWI-DEC-117`, `132`; `MasterData` `RWI-DEC-134` | `X` Bank Darah G-20 | `C` modul penunjang masing-masing | `C` `MedicalRecordManagement` `RWI-DEC-138`, `142`, `144` |
| 11 | Integrasi | `C` internal saja | `C` Farmasi internal | `M` sumber hasil gula darah untuk sliding scale G-19 | `C` | `C` |
| 12 | Hasil akhir | `C` `RWI-DEC-120` | `C` riwayat dosis; draft resep dari rekonsiliasi | `C` | `C` | `C` |
| 13 | Pembatalan/koreksi | `C` addendum `RWI-DEC-091` | `C` koreksi tidak menimpa `RWI-DEC-116` (c) | `C` `FR-MVP-KEP-016`, `017` | `C` | `C` `RWI-DEC-127`, `138`, `143` |
| 14 | Audit/histori | `C` versi instrumen `RWI-DEC-124` | `C` pelaksana, waktu rencana/aktual | `C` | `C` | `C` `RWI-DEC-144` |
| 15 | Notifikasi | Tidak material — daftar pantau `RWI-DEC-029` | `M` penerima notifikasi dugaan reaksi obat G-15 | `C` acknowledgment handover | Tidak material | Tidak material — daftar pantau `RWI-DEC-126`, `129` |
| 16 | Billing/charge | Tidak material — pengkajian tidak menagih | `C` resep dan pemberian bukan tagihan `BR-RWI-013` | `M` transfusi dan sliding scale G-19 | `C` modul penunjang | `C` biaya tindakan lahir dari pelaksanaan, lihat 14.5 butir 9 |
| 17 | Keselamatan klinis | `C` untuk desain; `M` intake obat/darah G-08 | `C` high-alert, idempotency; `M` isi daftar G-13 | `X` G-20 | `C` | `C` alergi saat pakai template `RWI-DEC-122`, lihat 14.7 |
| 18 | Pelaporan/traceability | `C` progres dan kepatuhan | `C` riwayat per dosis | `C` | `C` | `C` |

### 14.5 Butir `CONFIRMED` yang menopang kesiapan

| No | Butir | Bukti |
|---:|---|---|
| 1 | Kajian Umum delapan bagian; Psikososial, Alat Bantu, dan Catatan Relevan sebagai isian; satu fakta satu tempat. Daftar sepuluh bagian PRD bagian 28 dan kemampuan instrumen `FR-MVP-KEP-003` (termasuk sirkulasi dan neurologi) dibaca lewat lapisan `RWI-DEC-110`: isi form mengikuti `RWI-DEC-141` | `RWI-DEC-141`; `RWI-DEC-110` |
| 2 | Resiko Jatuh dan isian wajib berupa konfigurasi berversi, dihitung server, disahkan terpisah dari pengubahnya, tanpa angka tertanam di source | `RWI-DEC-124`, `RWI-DEC-136`; `FR-MVP-KEP-004`, `005`; `AC-MVP-034` |
| 3 | Monitoring Nyeri membedakan Belum dinilai, Tidak nyeri, Ada nyeri, dan Tidak dapat dinilai; riwayat longitudinal tidak menimpa nilai lama | PRD bagian 30; `BR-RWI-007`; `PRD-RWI-FINAL-001` 16.2 aturan 6 |
| 4 | Assesment Edukasi punya penerima, kebutuhan, hambatan, materi, metode, dan evaluasi pemahaman — **bukan** satu catatan teks | PRD bagian 31 |
| 5 | Pengawasan Harian mencatat tanda vital, nyeri, intake, output, balance, gula darah, diet, mobilisasi; total dihitung dari data terstruktur per shift dan rolling 24 jam; koreksi mempertahankan nilai lama | PRD bagian 32; `FR-MVP-KEP-017`; `AC-MVP-031`; `RWI-DEC-120` butir (1) |
| 6 | Evaluasi Awal milik MPP dengan delapan bagian, jangkauan per unit penempatan, dan tidak menahan progres perawat | `RWI-DEC-118`, `RWI-DEC-120`, `RWI-DEC-131` |
| 7 | Perencanaan Pulang dengan delapan kelompok isi dan tidak menutup episode | PRD bagian 34; `BR-RWI-016` |
| 8 | MAR `P0`: enam status dosis beralasan, pelaksana dari akun login, idempoten, cek ganda high-alert oleh pengguna kedua, dugaan reaksi obat tertaut dosis; tabel milik `PharmacyManagement`; rekonsiliasi obat `P0` bersama MAR dari master obat | `RWI-DEC-116`, `117`, `132` s.d. `134`; `FR-MVP-KEP-009` s.d. `014`; `AC-MVP-025` s.d. `029` |
| 9 | Pesanan tindakan yang belum dilaksanakan tidak punya tagihan, sehingga pembatalan otomatis saat episode ditutup (`RWI-DEC-143`) tidak bersinggungan dengan Billing. Source juga tidak pernah menyetel `TrxPatientProcedure.IsBillingGenerated = true` | `BR-RWI-013`; `PRD-RWI-FINAL-001` `CAP-014` aturan 5; `BE@df3679c0` pencarian seluruh `*.cs` |
| 10 | Keenam layanan penunjang adalah perluasan permukaan `CAP-015`, **bukan kemampuan yatim**; Gizi, Hemodialisa, Bank Darah, dan Rehab Medik tampil "Integrasi belum tersedia" tanpa backend baru | `RWI-DEC-108`, `RWI-DEC-113` |
| 11 | Dokumentasi dokter V2 terkunci keputusan: layout mengikuti PRD, Catatan Saya dari satu sumber, jenis catatan CPPT, penulis dan penguncian konsep, penulis dan pembatalan tindakan, verifikasi CPPT hanya DPJP, Template Resep pribadi dengan pemilik dari akun login | `RWI-DEC-107`, `121` s.d. `130`, `135`, `137` s.d. `144` |

### 14.6 Butir `PROPOSED`, `MISSING`, dan `CONFLICT`

| ID | Slice | Butir | Status | Dampak | Alasannya |
|---|---|---|---|---|---|
| G-01 | `S17` | Isian rinci setiap bagian Kajian Umum, Assesment Edukasi, dan Perencanaan Pulang | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD memberi bagian dan kelompok; isian rinci diusulkan dari label frontend V1 (`01-existing-capability-map.md` bagian 17 `RLN3-CAP-05`, `08`, `11`). Struktur dokumen tidak berubah oleh jumlah isian |
| G-02 | `S17` | Isi klinis instrumen risiko jatuh per kelompok usia dan isian wajib setiap jenis pengkajian | `MISSING` | `CONFIGURABLE_DEFAULT` | `RWI-OQ-056`, `RWI-OQ-057`, `OPEN-MVP-003`. `RWI-DEC-124` sudah menetapkan mekanisme konfigurasi; menahan pemakaian untuk pasien sungguhan, bukan desain |
| G-03 | `S17` | Satu rangkaian data tanda vital dan nyeri per episode yang ditampilkan di Kondisi Umum, Pengawasan Harian, Vital Sign Keperawatan, dan Monitoring Nyeri | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan prinsip yang sudah dikonfirmasi pemilik: data terstruktur tidak diduplikasi (`RWI-DEC-140` butir 4) dan satu fakta satu tempat (`RWI-DEC-141` butir 4). Desain wajib menghormatinya; bentuk penyimpanannya milik desain |
| G-04 | `S17` | Instrumen nyeri untuk pasien yang tidak dapat menilai sendiri (bayi, anak, tidak sadar) dan interval kajian ulang setelah intervensi | `MISSING` | `CONFIGURABLE_DEFAULT` | PRD menyediakan keadaan "Tidak dapat dinilai" dan skala 0–10, tanpa instrumen alternatif maupun interval. Wajar berbeda antar rumah sakit dan dapat mengikuti pola konfigurasi berversi `RWI-DEC-124` |
| G-05 | `S17` | Tanda tangan penerima edukasi dan tanda tangan pasien/keluarga pada perencanaan pulang, yang ada di V1 | `MISSING` | `NON_BLOCKING_STANDARD` | PRD v`2.0` tidak menyebutnya. Bila kelak diminta, dapat ditambahkan sebagai bukti pelengkap tanpa mengubah dokumen inti |
| G-06 | `S17` | Batas jam shift untuk total intake/output | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-017` menuntut total per shift, sedangkan sistem tidak punya konsep shift perawat (`RWI-DEC-100`). Jam shift wajar berbeda per unit |
| G-07 | `S17` | Isi checklist setiap bagian Evaluasi Awal dan pengesahnya | `MISSING` | `CONFIGURABLE_DEFAULT` | PRD hanya memberi delapan judul; V1 memakai master `/ChecklistItem` yang dinamis. Menahan pemakaian untuk pasien sungguhan, bukan struktur dokumen |
| G-08 | `S17` | **Sumber volume intake obat dan darah** pada Pengawasan Harian: diketik perawat, atau diturunkan dari MAR dan catatan transfusi | `MISSING` | **`BLOCKING` hanya untuk sub-bagian intake obat dan darah** | PRD bagian 32 mencantumkan intake Obat dan Darah. MAR `FR-MVP-KEP-011` menyimpan dosis dan rute, bukan volume cairan pelarut. Pilihannya mengubah integrasi MAR–intake/output dan berisiko menghitung ganda balance cairan — `DEC-INP-010` |
| G-09 | `S17` | Frekuensi Evaluasi Awal: sekali per episode atau berulang | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: satu dokumen per episode yang dilengkapi lewat addendum, sesuai namanya "Evaluasi Awal". Tetap terlihat sebagai usulan, bukan kebijakan |
| G-10 | `S17` | Nilai "Status Rencana" pada Perencanaan Pulang | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: isian informatif yang **tidak** memicu perubahan status episode (`BR-RWI-016`); status dokumennya mengikuti `RWI-DEC-119` |
| G-11 | `S17` | Cara sistem mengenali pengguna berperan MPP | `PROPOSED` | `NON_BLOCKING_STANDARD` | Konvensi proyek: hak akses pada layar Akses Role, bukan nama peran di kode (`RWI-FACT-031`, `RWI-DEC-118` konsekuensi a) |
| G-12 | `S18` | Jam standar pemberian per frekuensi dan jendela waktu sebelum dosis dianggap `Missed` | `MISSING` | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar rumah sakit dan unit; mekanisme status dosis sudah dikonfirmasi `FR-MVP-KEP-010` |
| G-13 | `S18` | Isi daftar obat high-alert dan aturan cek gandanya | `MISSING` | `CONFIGURABLE_DEFAULT` | `RWI-DEC-116` butir (3) menjadikannya keputusan klinis/Farmasi dan **gerbang sebelum produksi** |
| G-14 | `S18` | Interval evaluasi obat PRN | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-013` `P1` menyebut "interval yang ditetapkan" tanpa angka |
| G-15 | `S18` | Penerima notifikasi dugaan reaksi obat | `MISSING` | `CONFIGURABLE_DEFAULT` | `FR-MVP-KEP-014` mewajibkan notifikasi klinis tanpa menyebut penerima. Usulan bawaan: DPJP aktif dan apoteker. Rute notifikasi tidak mengubah catatan pemberian |
| G-16 | `S18` | Kepemilikan catatan dugaan reaksi obat dan kontraknya dengan `ClinicalManagement` | `PROPOSED` | `NON_BLOCKING_STANDARD` | `RWI-DEC-117` butir (a) sudah menetapkannya sebagai titik sentuh yang dikontrakkan desain |
| G-17 | `S18` | Rekonsiliasi obat saat **transfer** dan saat **pulang** | `MISSING` | `NON_BLOCKING_STANDARD` untuk rekonsiliasi admisi | `RWI-DEC-132` hanya menyebut admisi. Obat pulang sudah menjadi jenis resep `RWI-DEC-046`. Perluasan ke transfer dan pulang adalah pertanyaan scope, tidak menahan rekonsiliasi admisi |
| G-18 | `S18` | Batas waktu rekonsiliasi sejak admisi | `MISSING` | `CONFIGURABLE_DEFAULT` | Pola sama dengan `RWI-RULE-021` |
| G-19 | `S19` | **Penempatan rilis handover shift, sliding scale, dan transfusi** | `MISSING` | **`BLOCKING` untuk `INP-S19`** | Ketiganya `P1` pada PRD-to-MVP-V2, tidak muncul sebagai menu pada PRD v`2.0`, dan blueprint `keperawatan` menundanya (`keperawatan/04-prd-to-mvp.md` bagian 21.6). `RWI-DEC-116` hanya menarik MAR. Belum ada keputusan apakah ketiganya masuk penyelarasan ini. Sliding scale juga membutuhkan pemilik protokol berversi dan sumber hasil gula darah — `DEC-INP-011` |
| G-20 | `S19` | **Kepemilikan alur transfusi di bangsal** | **`CONFLICT`** | **`BLOCKING` untuk transfusi** | `FR-MVP-KEP-019` menaruh workflow transfusi pada Keperawatan Rawat Inap, sedangkan blueprint `bank-darah` (`IN_PROGRESS`) menyatakan penanganan reaksi dan pemantauan pasca-transfusi sebagai lingkupnya. Tidak dapat diselesaikan dari urutan wewenang karena keduanya modul berbeda — `DEC-INP-012` |
| G-21 | `S20` | Nomor `CAP` final Gizi, Hemodialisa, Bank Darah, dan Rehab Medik | `PROPOSED` | `NON_BLOCKING_STANDARD` | `RWI-DEC-113` menugaskannya ke `design-business-module` |

### 14.7 Gap teknis, bukan keputusan bisnis

Butir berikut **bukan** bahan gerbang ini dan tidak boleh dipakai untuk menahan kesiapan requirement:

| Butir | Sifatnya | Rujukan |
|---|---|---|
| Risiko jatuh tersimpan dengan arti berbeda, enum pengkajian tidak cocok, daftar pengkajian keperawatan salah bentuk, isian belum dikaji terkirim sebagai normal | **Teknis — perbaikan keselamatan.** Arahnya sudah dikunci `RWI-DEC-124`, `136`, `BR-RWI-007` | `RWI-FACT-038`; capability map bagian 17 `RLN3-CON-01` s.d. `04` |
| Pemeriksaan ulang alergi saat memakai template belum ada | **Teknis.** Requirement sudah tegas (`RWI-DEC-122` butir 4) dan sumber data alergi terstruktur **sudah ada**: `TrxPatientAllergy` punya `DrugId`, kode/nama/kelompok alergen, tingkat keparahan, dan status | `RLN3-CAP-42`; `BE@df3679c0 ClinicalManagement/Models/TrxPatientAllergy.cs` baris 42–73 |
| Penjaga penulis pada jalur ubah dan final, verifikasi CPPT menerima peran apa pun, tindakan baru pada episode tertutup | **Teknis.** Arah dikunci `RWI-DEC-125`, `128`, `139`, `143`, `144` | `RLN3-CAP-25`, `33` s.d. `36` |
| MAR, rekonsiliasi obat, intake/output, isian terstruktur lima bagian Kajian Umum belum ada di source | **Ketersediaan implementasi**, bukan keputusan yang belum diambil | `RLN3-CAP-05`, `09`, `15` |
| Persetujuan pemilik `MedicalRecordManagement` atas daftar catatan terkunci milik penulis | **Dependency persetujuan modul tetangga**, menahan implementasi bagian itu | `RWI-DEC-142` butir (3) |
| Komponen layout Dokter Rawat Jalan milik pemilik `rawat-jalan` | **Dependency modul tetangga** untuk kesamaan layout | `RLN3-CAP-03` |

### 14.8 Decision Log

Decision ID: `DEC-INP-010`

| Field | Isi |
|---|---|
| Pertanyaan | Dari mana volume **intake obat** dan **intake darah** pada Pengawasan Harian Pasien diambil: diketik perawat, diturunkan dari MAR dan catatan transfusi, atau gabungan dengan aturan tertentu? Termasuk apakah volume cairan pelarut obat intravena dihitung |
| Kemampuan terdampak | `INP-S17` Pengawasan Harian — sub-bagian intake obat dan darah; `INP-S18` MAR sebagai sumber potensial |
| Bukti saat ini | PRD v`2.0` bagian 32 mencantumkan Intake Infus, Oral, NGT, **Darah**, **Obat**. `FR-MVP-KEP-011` menyimpan dosis dan rute aktual, bukan volume. `FR-MVP-KEP-017` menuntut total per shift dari data terstruktur. `RWI-DEC-140` butir (4) melarang duplikasi data terstruktur ke catatan naratif, tetapi tidak mengatur hubungan MAR dengan intake/output |
| Usulan baseline | Intake obat intravena dan darah tidak diketik ulang bila sumbernya sudah tercatat terstruktur; perhitungan volume pelarut mengikuti kebijakan klinis yang disahkan. Usulan ini **belum** kebijakan rumah sakit |
| Dampak | Integrasi MAR–intake/output, integritas data balance cairan, dan keselamatan klinis karena risiko hitung ganda atau terlewat |
| Pemilik | Muhammad Hamzah bersama pemilik klinis/keperawatan yang belum ditunjuk |
| Status | `OPEN` |
| Dampak implementasi/domain | Sub-bagian intake obat dan darah **berhenti**. Bagian lain Pengawasan Harian — tanda vital, nyeri, intake infus/oral/NGT, output, diet, mobilisasi — **boleh berjalan** |

Decision ID: `DEC-INP-011`

| Field | Isi |
|---|---|
| Pertanyaan | Apakah handover shift (`FR-MVP-KEP-015`, `016`), sliding scale (`FR-MVP-KEP-018`), dan transfusi (`FR-MVP-KEP-019`) masuk penyelarasan `PRD-RWI-V2-001` ini seperti MAR, atau tetap ditunda ke irisan berikutnya? Bila masuk: siapa pemilik dan pengesah protokol sliding scale berversi, dan dari mana hasil gula darah diambil |
| Kemampuan terdampak | `INP-S19` seluruhnya |
| Bukti saat ini | Ketiganya `P1` pada PRD-to-MVP-V2 yang tetap berlaku (`RWI-DEC-110`). PRD v`2.0` tidak menampilkannya sebagai menu. Blueprint `keperawatan` yang disetujui menundanya bersama MAR (`04-prd-to-mvp.md` bagian 21.6), lalu `RWI-DEC-116` hanya menarik MAR. Capability map `V2-CAP-09`, `10`, `12`: `Missing` |
| Usulan baseline | Tidak diberikan. Ini keputusan scope dan prioritas milik pemilik |
| Dampak | Batas rilis gelombang keperawatan, model domain protokol berversi, integrasi dengan Bank Darah dan sumber hasil laboratorium/POCT |
| Pemilik | Muhammad Hamzah, Product/Domain; protokol sliding scale memerlukan pemilik klinis |
| Status | `OPEN` |
| Dampak implementasi/domain | `INP-S19` **berhenti**. Tidak menahan `INP-S17`, `S18`, `S20`, maupun `S21` |

Decision ID: `DEC-INP-012`

| Field | Isi |
|---|---|
| Pertanyaan | Siapa pemilik alur transfusi di bangsal — pre-check, tanda vital awal, mulai/berhenti, observasi berkala, dan penanganan reaksi: Keperawatan Rawat Inap sesuai `FR-MVP-KEP-019`, atau modul Bank Darah sesuai blueprint `bank-darah`? |
| Kemampuan terdampak | `INP-S19` bagian transfusi; `INP-S17` intake darah melalui `DEC-INP-010` |
| Bukti saat ini | `FR-MVP-KEP-019` dan `AC-MVP-033` pada PRD-to-MVP-V2; blueprint `bank-darah` `00-interview-decisions.md` menyebut penanganan reaksi transfusi dan pemantauan pasca-transfusi sebagai lingkupnya, dengan pemilik proses yang belum bernama; `RWI-DEC-108` menampilkan menu Bank Darah sebagai "Integrasi belum tersedia" |
| Usulan baseline | Satu pemilik data observasi transfusi, dengan modul lain membaca lewat kontrak; bukan dua catatan paralel |
| Dampak | Kepemilikan data, authorization pemeriksa kedua, keselamatan klinis reaksi transfusi, dan integrasi antarmodul |
| Pemilik | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Status | `OPEN` — hanya relevan bila `DEC-INP-011` memasukkan transfusi ke penyelarasan ini |
| Dampak implementasi/domain | Transfusi **berhenti**. Bila diputuskan milik Bank Darah, Rawat Inap hanya menyediakan permukaan dan konteks episode |

### 14.9 Kesiapan per slice dan kemampuan

| Slice | Kemampuan/isi | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `INP-S17` | Kajian Umum | **`READY_FOR_DOMAIN_DESIGN`** | — | G-01 usulan isian; G-02 isian wajib `CONFIGURABLE_DEFAULT` |
| `INP-S17` | Resiko Jatuh | **`READY_FOR_DOMAIN_DESIGN`** | — | Mekanisme dikunci; isi klinis gerbang produksi |
| `INP-S17` | Monitoring Nyeri | **`READY_FOR_DOMAIN_DESIGN`** | — | G-03, G-04 |
| `INP-S17` | Assesment Edukasi | **`READY_FOR_DOMAIN_DESIGN`** | — | G-01, G-05 |
| `INP-S17` | Pengawasan Harian Pasien | **`PARTIALLY_READY`** | `DEC-INP-010` | Boleh: tanda vital, nyeri, intake infus/oral/NGT, output, balance dari data terstruktur, diet, mobilisasi. Berhenti: intake obat dan darah |
| `INP-S17` | Evaluasi Awal (MPP) | **`READY_FOR_DOMAIN_DESIGN`** | — | G-07, G-09, G-11 |
| `INP-S17` | Perencanaan Pulang | **`READY_FOR_DOMAIN_DESIGN`** | — | G-05, G-10 |
| `INP-S17` | Progres Pengkajian | **`READY_FOR_DOMAIN_DESIGN`** | — | `RWI-DEC-119`, `120` |
| `INP-S18` | MAR | **`READY_FOR_DOMAIN_DESIGN`** | — | G-12 s.d. G-16; high-alert gerbang produksi |
| `INP-S18` | Rekonsiliasi obat saat admisi | **`READY_FOR_DOMAIN_DESIGN`** | — | G-17 scope transfer/pulang non-blocking; G-18 |
| `INP-S19` | Handover shift, sliding scale | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-011` | — |
| `INP-S19` | Transfusi | **`BUSINESS_DECISION_REQUIRED`** | `DEC-INP-011`, `DEC-INP-012` | — |
| `INP-S20` | Laboratorium dan Radiologi | **`READY_FOR_DOMAIN_DESIGN`** | — | Sudah siap sejak revision `1.3` sebagai `CAP-015` |
| `INP-S20` | Gizi, Hemodialisa, Bank Darah, Rehab Medik | **`READY_FOR_DOMAIN_DESIGN`** untuk permukaan "Integrasi belum tersedia" saja | — | Backend **`DEFERRED`** sampai modul pemiliknya tersedia, sesuai `RWI-DEC-108` |
| `INP-S21` | Dokumentasi klinis dan ruang kerja dokter V2 | **`READY_FOR_DOMAIN_DESIGN`** | — | Dependency persetujuan `MedicalRecordManagement` menahan implementasi daftar catatan terkunci, bukan desain |

**Hasil turunan:**

- `INP-S17` **`PARTIALLY_READY`**; `INP-S18` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S19`
  **`BUSINESS_DECISION_REQUIRED`**; `INP-S20` **`READY_FOR_DOMAIN_DESIGN`**; `INP-S21`
  **`READY_FOR_DOMAIN_DESIGN`**.
- **`RLN-04` tertutup:** tidak ada kemampuan penunjang yang yatim (`RWI-DEC-113`).
- **`RLN-07` tertutup:** ketujuh isi Pengkajian Pasien dan progresnya dinilai; hanya sub-bagian
  intake obat dan darah yang berhenti.
- Overall Rawat Inap tetap **`PARTIALLY_READY`**.

### 14.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke amandemen desain `RLN-PH-06`:**

- `INP-S17` kecuali sub-bagian intake obat dan darah;
- `INP-S18` MAR dan rekonsiliasi obat saat admisi;
- `INP-S20` seluruhnya, dengan empat layanan non-laboratorium/radiologi sebatas permukaan;
- `INP-S21` seluruhnya.

**Harus berhenti:**

- sub-bagian intake obat dan darah Pengawasan Harian, sampai `DEC-INP-010` diputuskan;
- handover shift, sliding scale, dan transfusi, sampai `DEC-INP-011` diputuskan; transfusi juga
  menunggu `DEC-INP-012`;
- **pemakaian untuk pasien sungguhan** atas Resiko Jatuh, isian wajib pengkajian, checklist Evaluasi
  Awal, dan daftar high-alert, sampai isi klinisnya disahkan. Ini menahan rilis, **bukan** desain.

**Dependency di antara keduanya:** `DEC-INP-010` bergantung pada `DEC-INP-012` untuk intake darah.
MAR tidak bergantung pada `DEC-INP-010`; yang bergantung justru intake obat pada MAR.

### 14.11 Keputusan pemilik yang dibutuhkan

| Prioritas | Keputusan | Memblokir |
|---|---|---|
| 1 | `DEC-INP-011` — penempatan rilis handover shift, sliding scale, transfusi | `INP-S19` |
| 2 | `DEC-INP-010` — sumber intake obat dan darah | Sub-bagian Pengawasan Harian |
| 3 | `DEC-INP-012` — pemilik alur transfusi di bangsal | Transfusi, bila masuk |
| Non-blocking | G-17 rekonsiliasi saat transfer dan pulang; G-05 tanda tangan penerima edukasi dan rencana pulang; G-09 frekuensi Evaluasi Awal | — |
| Gerbang produksi | `RWI-OQ-056`, `RWI-OQ-057`, isi high-alert, checklist Evaluasi Awal, penunjukan pemilik klinis | Rilis |

### 14.12 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `INP-S17` (kecuali intake obat dan darah), `INP-S18`, `INP-S20`, `INP-S21` |
| `requirement_readiness` | `S17` `PARTIALLY_READY`; `S18`, `S20`, `S21` `READY_FOR_DOMAIN_DESIGN`; `S19` `BUSINESS_DECISION_REQUIRED` |
| `requirement_evidence_status` | Mayoritas `CONFIRMED`; 21 butir gap: 5 `PROPOSED`, 15 `MISSING`, 1 `CONFLICT` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice yang siap, dan tidak diperlukan: batas konteks dan kepemilikan datanya sudah ditetapkan `RWI-DEC-081`, `113`, `117`, `118`, `132`, `142`, `144`. `RLN-PH-05` baru relevan untuk transfusi setelah `DEC-INP-012` |
| `decision_ids` | `DEC-INP-010`, `DEC-INP-011`, `DEC-INP-012` — ketiganya `OPEN` |
| `dependency_ids` | Persetujuan `MedicalRecordManagement` (`RWI-DEC-142`); komponen `rawat-jalan` (`RLN3-CAP-03`); modul Bank Darah (`DEC-INP-012`); gerbang produksi isi klinis |
| `next_owner` | `design-business-module` untuk amandemen `dokter-rawat-inap` dan `keperawatan`, fase `RLN-PH-06`; `grill-me` untuk `DEC-INP-010` s.d. `012` |
| `required_boundary` | Dokumentasi keperawatan dan dokter tetap milik `ClinicalManagement`; MAR dan rekonsiliasi milik `PharmacyManagement`; metadata keutuhan milik `MedicalRecordManagement`. Desain **MUST NOT** membuat tabel intake obat/darah maupun tabel transfusi sebelum keputusannya turun |
| `expected_output` | Amandemen blueprint kedua sub-modul revision `7` untuk slice yang siap, dengan sub-bagian dan slice yang berhenti ditandai eksplisit |

---

## 15. Penutupan keputusan penyelarasan `PRD-RWI-V2-001` — revision `1.6`

Bagian ini adalah **hasil kanonis terbaru** untuk `INP-S17` dan `INP-S19`. Ia menggantikan status pada
14.4 (kolom `INP-S19` dan baris intake obat/darah kolom `INP-S17`), 14.6 butir `G-08`, `G-19`, `G-20`,
serta 14.8 s.d. 14.12. Hasil `INP-S18`, `INP-S20`, dan `INP-S21` tetap sebagaimana bagian 14.

### 15.1 Kenapa penutupan ini dijalankan

Revision `1.5` menahan dua hal dengan tiga Decision ID:

- sub-bagian **intake obat dan darah** Pengawasan Harian, oleh `DEC-INP-010`;
- **handover shift, sliding scale, dan transfusi**, oleh `DEC-INP-011`, dengan transfusi juga menunggu
  `DEC-INP-012`.

Pemilik menjawab ketiganya pada Amendment Pass penutupan gate `RLN-PH-04`, 15 September 2026, lima
pertanyaan, seluruhnya pilihan A. Gate ini menyerap jawabannya, menilai ulang kemampuan yang terbuka
oleh jawaban itu, lalu menetapkan kesiapan baru. Gate **tidak** menambah keputusan atas nama pemilik.

### 15.2 Scope penilaian

| Slice | Kemampuan atau isi | Cara dinilai | Keputusan yang mengikat |
|---|---|---|---|
| `INP-S17` | Pengawasan Harian — sub-bagian **intake obat dan intake darah** | Penuh, 18 dimensi | `RWI-DEC-149` |
| `INP-S17` | Pengawasan Harian — **pencatatan gula darah** | Sebatas peran barunya sebagai satu-satunya sumber dosis sliding scale | `RWI-DEC-148` |
| `INP-S19` | **Sliding scale** — template protokol, order per pasien, pelaksanaan | Penuh, 18 dimensi | `RWI-DEC-145` s.d. `RWI-DEC-148` |
| `INP-S19` | Handover shift (`FR-MVP-KEP-015`, `016`) | **`DEFERRED` — tidak dinilai** | `RWI-DEC-145` butir (4) |
| `INP-S19` | Transfusi (`FR-MVP-KEP-019`) | **`DEFERRED` — tidak dinilai** | `RWI-DEC-145` butir (4) dan (5) |
| `INP-S18` | MAR | Hanya dampak silang: MAR mendapat dua pengguna baru, yaitu tautan entri intake obat dan dosis sliding scale | `RWI-DEC-145` butir (2), `RWI-DEC-149` butir (3) dan (4) |

**Arti `DEFERRED` di sini.** Handover shift dan transfusi **tidak terblokir** oleh keputusan yang belum
diambil. Pemilik sudah memutuskan mengeluarkan keduanya dari batas rilis penyelarasan ini, tanpa mencabut
requirement-nya. Pola ini sama dengan `CAP-016` pada bagian 13.9. Keduanya dinilai ulang saat dijadwalkan.

**Yang tidak dinilai:** slice `INP-S01` s.d. `INP-S16`, serta isi `INP-S17`, `S18`, `S20`, dan `S21` di
luar baris di atas.

### 15.3 Bukti yang dipakai

| Bukti | Revision / hash | Wewenang dan penggunaannya |
|---|---|---|
| [`00-interview-decisions.md`](../00-interview-decisions.md) | revision `21`, SHA-256 `1c55c80a…2d45102a`, keputusan terakhir `RWI-DEC-149`, acceptance criteria terakhir `RWI-AC-231` | Keputusan pemilik yang dikonfirmasi — wewenang tertinggi untuk bagian ini |
| `PRD-to-MVP-Rawat-Inap-V2` v`1.0.0` bagian 13.2, 13.3, dan 16 (`FR-MVP-KEP-009` s.d. `019`, `AC-MVP-031`, `AC-MVP-032`) | SHA-256 `9804b968…3ce141`, diperiksa ulang tidak berubah | Requirement yang tetap berlaku lewat `RWI-DEC-110` |
| `PRD-RWI-V2-001` v`2.0` bagian 32 dan `BR-RWI-013` | SHA-256 `2b3b2f29…0a679f`, diperiksa ulang tidak berubah | Daftar isi Pengawasan Harian; aturan "order ≠ pelaksanaan ≠ hasil ≠ tagihan" |
| [`01-existing-capability-map.md`](../01-existing-capability-map.md) | revision `1.4`, SHA-256 `337a10f0…d22daa543a`, tidak berubah | "Apa yang ada sekarang": `V2-CAP-10` sliding scale `Missing`, `RLN3-CAP-09` Pengawasan Harian `Missing`, `RLN3-CAP-15` MAR `Missing` |
| Frontend V1 `13c3a96b` `catatan-keperawatan/sliding-scale/` | dibaca pada Amendment Pass, dikutip `RWI-DEC-145` | Bukti legacy: V1 hanya catatan bebas tanggal, GDS, insulin, insulin drip, dan catatan, tanpa protokol |

> **Batas yang dijaga.** Kelima keputusan lahir dari opsi yang disusun agent, lalu dipilih pemilik
> secara tertulis, sehingga berstatus `CONFIRMED`. Usulan pada tabel 15.9 **tidak** dipilih pemilik dan
> tetap `PROPOSED` atau `MISSING`.

### 15.4 Keputusan yang turun

| ID | Isi singkat | Status |
|---|---|---|
| `RWI-DEC-145` | Sliding scale masuk batas rilis bersama MAR dan rekonsiliasi obat. Handover shift dan transfusi ditunda ke irisan berikutnya, tanpa dicabut | `approved` |
| `RWI-DEC-146` | Protokol sliding scale dua lapis: template standar berversi yang disahkan terpisah dari pengubahnya, lalu order per pasien yang boleh disesuaikan dengan alasan wajib | `approved` |
| `RWI-DEC-147` | Template dan order sliding scale milik `PharmacyManagement`, bersama resep dan MAR | `approved` |
| `RWI-DEC-148` | Dosis insulin hanya dihitung dari GDS bangsal, yang tersimpan satu kali sebagai gula darah Pengawasan Harian. Hasil laboratorium hanya informasi | `approved` |
| `RWI-DEC-149` | Intake obat dan darah dicatat perawat sebagai volume aktual, termasuk pelarut. Entri obat tertaut ke satu dosis MAR `Administered`; MAR tidak mendapat isian volume | `approved` |
| `RWI-OQ-091`, `092`, `094`, `095`, `096` | Pertanyaan pass | `TERTUTUP` |
| `RWI-OQ-093` | Pemilik alur transfusi di bangsal | `DITUNDA` bersama transfusi |
| `DEC-INP-010` | Sumber intake obat dan darah | **`CLOSED`** oleh `RWI-DEC-149` |
| `DEC-INP-011` | Penempatan rilis handover shift, sliding scale, transfusi | **`CLOSED`** oleh `RWI-DEC-145`, dirinci `RWI-DEC-146` s.d. `148` |
| `DEC-INP-012` | Pemilik alur transfusi di bangsal | **`DEFERRED`** bersama transfusi |

### 15.5 Hasil penilaian 18 dimensi

Kode: `C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `X` = `CONFLICT`. Nomor `G-##` merujuk tabel
15.8 dan 15.9.

| ID | Dimensi | `INP-S17` Intake obat dan darah | `INP-S19` Sliding scale |
|---:|---|---|---|
| 01 | Tujuan | `C` balance cairan memuat obat dan darah, PRD bagian 32, `FR-MVP-KEP-017` | `C` dosis insulin mengikuti protokol aktif, `FR-MVP-KEP-018`, `RWI-DEC-145` (1) |
| 02 | Aktor | `C` perawat pencatat dari akun login, `RWI-DEC-149` (1) | `C` dokter pemesan `RWI-DEC-146` (3); perawat pelaksana; pengubah dan pengesah template terpisah `RWI-DEC-146` (1), `147` (4); apoteker membaca `RWI-DEC-147`. Pengesah isi klinis belum ditunjuk — gerbang produksi, bukan desain |
| 03 | Pemicu/prasyarat | `C` obat: dosis MAR sudah `Administered` `RWI-AC-230`; darah: tanpa prasyarat transfusi `RWI-DEC-149` (5) | `C` order aktif dari versi template yang disahkan `RWI-AC-219`, `222`; GDS bangsal tercatat `RWI-DEC-148`; `M` jadwal pemeriksaan G-22 |
| 04 | Alur utama | `C` contoh `RWI-DEC-149` | `C` contoh `RWI-DEC-145`, `146`, `148` |
| 05 | Alternatif/exception | `C` entri kedua, tanpa tautan, dosis `Held`, koreksi volume `RWI-DEC-149` (a)–(d); `M` koreksi dosis MAR yang sudah tertaut G-26 | `C` tanpa order aktif, template draft, penyesuaian tanpa alasan, rentang bertumpuk, order dihentikan, hasil lab dipilih, kontrak baca gagal, dosis berbeda sebagai pengecualian, cek ganda high-alert |
| 06 | Data minimum | `C` sumber, volume, satuan, waktu, pelaksana, tautan dosis `RWI-DEC-149` (1), (3) | `C` template: versi, rentang, dosis; order: versi template, penyesuaian, alasan, dokter, waktu; pelaksanaan: rujukan GDS, aturan yang cocok, dosis, pengecualian, versi order `RWI-DEC-146` (2), (4); `M` satuan GDS G-25 |
| 07 | Aturan/validation | `C` volume aktual termasuk pelarut; satu dosis satu entri `RWI-AC-229`; `M` dosis bervolume tanpa entri G-27 | `C` rentang tidak bertumpuk dan tidak berlubang `RWI-DEC-146` (1); dosis hanya dari GDS bangsal `RWI-AC-228`; `P` cakupan rentang terbuka G-23 |
| 08 | Status/lifecycle | `C` entri aktif dan terkoreksi, nilai lama tetap `FR-MVP-KEP-017`, `RWI-AC-231` | `C` template draft → disahkan; order aktif → dihentikan; penyesuaian menjadi versi order baru `RWI-DEC-146` (6); dosis memakai enam status MAR; `M` letak dosis sliding scale dalam lifecycle MAR G-22 |
| 09 | Peran/authorization | `C` mengikuti kewenangan Pengawasan Harian `RWI-DEC-100` | `C` pemesan mengikuti penulis resep `RWI-DEC-099`, `128`; pemisahan pengesahan `RWI-DEC-136`, `147` (4); cek ganda pengguna kedua `FR-MVP-KEP-012` |
| 10 | Dependency antarmodul | `C` entri milik `ClinicalManagement` merujuk dosis MAR milik `PharmacyManagement` `RWI-DEC-149` konsekuensi (2) | `C` `PharmacyManagement` pemilik `RWI-DEC-147`; `ClinicalManagement` pemilik gula darah `RWI-DEC-148`; MAR `INP-S18` |
| 11 | Integrasi | `C` internal saja; tanpa Bank Darah `RWI-DEC-149` (5) | `C` internal: baca gula darah `ClinicalManagement`, tampil hasil lab sebagai informasi `RWI-DEC-148` (3). Tanpa integrasi glukometer — angka diketik perawat |
| 12 | Hasil akhir | `C` total per shift dan rolling 24 jam dari lima sumber `RWI-AC-231` | `C` dosis tercatat satu kali di MAR dengan rujukan GDS dan aturan `RWI-AC-220`, `227` |
| 13 | Pembatalan/koreksi | `C` koreksi mempertahankan nilai lama; `M` G-26 | `C` penghentian order `RWI-AC-226`; versi order baru; koreksi GDS mempertahankan nilai asli `RWI-DEC-148` (b); koreksi pemberian mengikuti `RWI-DEC-116` (c) |
| 14 | Audit/histori | `C` pelaksana, waktu, nilai lama | `C` versi template, versi order, alasan penyesuaian, rujukan GDS `RWI-DEC-146` (2), `148` (4) |
| 15 | Notifikasi | Tidak material — pencatatan volume tidak diminta memberi tahu siapa pun | `M` instruksi "lapor dokter" pada rentang G-24 |
| 16 | Billing/charge | Tidak material — pencatatan volume bukan kejadian tagihan; pelaksanaan bukan tagihan `BR-RWI-013` | `C` pemberian insulin bukan tagihan `BR-RWI-013`; `M` tagihan pemeriksaan GDS bangsal G-28 |
| 17 | Keselamatan klinis | `C` hitung ganda dicegah satu dosis satu entri; `M` entri terlewat G-27 | `C` satu sumber dosis, validasi rentang, cek ganda high-alert; isi template dan daftar high-alert menjadi gerbang produksi; `M` satuan GDS G-25 |
| 18 | Pelaporan/traceability | `C` entri obat tertelusur ke dosis MAR | `C` dosis tertelusur ke angka dan waktu GDS `RWI-DEC-148` (4); `M` catatan sliding scale V1 G-29 |

### 15.6 Aturan yang dapat diuji

Seluruh contoh memakai data samaran.

**Sliding scale**

1. Template "Sliding Scale Insulin Dewasa v2" masih draft. dr. Rina memilihnya untuk Budi → **ditolak**.
   Setelah v2 disahkan oleh pengguna lain yang bukan pengubah terakhirnya, dr. Rina dapat memesannya
   (`RWI-AC-222`).
2. v2 berisi GDS 150–199 → 2 unit, 200–249 → 4 unit, 250–299 → 6 unit, ≥ 300 → 8 unit. dr. Rina membuat
   dosis setiap rentang separuh dengan alasan "pasien sensitif insulin". Order Budi menjadi 1, 2, 3, dan 4
   unit. Penyesuaian yang sama **tanpa** alasan ditolak (`RWI-AC-223`).
3. dr. Rina mengubah rentang menjadi 200–260 dan 250–299. Nilai 255 jatuh ke dua rentang → order
   **ditolak** saat disimpan (`RWI-AC-223`).
4. Pukul 06.00 hasil GDS laboratorium Budi 190. Pukul 11.00 perawat mengisi GDS glukometer 280 dari layar
   sliding scale. Angka 280 tersimpan **satu kali** sebagai gula darah Pengawasan Harian pukul 11.00,
   pelaksanaan merujuknya, dan sistem menampilkan 3 unit. Angka 190 tampil bertanda "informasi" dan
   **tidak dapat dipilih** sebagai dasar dosis (`RWI-AC-227`, `228`).
5. Perawat mencatat pemberian 3 unit. Riwayat MAR Budi menampilkan pemberian itu **tepat satu kali**,
   bersama rujukan GDS 280 dan aturan 250–299 (`RWI-AC-220`).
6. Rabu template v3 disahkan dengan rentang berbeda. Order Budi tetap memakai v2 beserta penyesuaiannya,
   dan pelaksanaan Selasa tetap terbaca dengan v2 (`RWI-AC-224`).
7. Pukul 15.00 dr. Rina menghentikan order. Pukul 17.00 perawat mencoba mencatat pelaksanaan dari order
   itu → **ditolak** dengan keterangan order sudah dihentikan (`RWI-AC-226`).
8. Tidak ada satu pun tabel sliding scale di `ClinicalManagement` maupun `InPatientManagement`, dan tidak
   ada menu, tabel, atau endpoint handover shift dan transfusi pada penyelarasan ini (`RWI-AC-221`, `225`).

**Intake obat dan darah**

9. Pukul 08.00 Budi mendapat Ceftriaxone 1 g dalam NaCl 100 ml, dan dosisnya `Administered` di MAR.
   Perawat mencatat intake "Obat — 100 ml" tertaut ke dosis itu. Perawat lain mencoba mencatat entri
   kedua untuk dosis yang sama → **ditolak**. Entri "Obat" tanpa tautan dosis → **ditolak** (`RWI-AC-229`).
10. Dosis Ceftriaxone 20.00 berstatus `Held`. Dosis itu **tidak dapat** ditautkan ke entri intake
    (`RWI-AC-230`).
11. Transfusi PRC 250 ml berhenti di 200 ml karena Budi menggigil. Perawat mencatat intake "Darah —
    200 ml" tanpa tautan ke catatan transfusi, karena transfusi ditunda.
12. Shift pagi Budi: Infus 500 ml, Oral 200 ml, Obat 100 ml, Darah 200 ml → total intake **1.000 ml**.
    Entri Obat ternyata hanya 80 ml dan dikoreksi → total dihitung ulang menjadi **980 ml**, dan nilai
    100 ml tetap terbaca pada riwayat (`RWI-AC-231`).

### 15.7 Butir `CONFIRMED` baru yang menopang kesiapan

Melanjutkan penomoran tabel 14.5.

| No | Butir | Bukti |
|---:|---|---|
| 12 | Sliding scale masuk batas rilis yang sama dengan MAR; label `P1` hanya asal requirement | `RWI-DEC-145` (1) |
| 13 | Dua lapis data berversi: template standar dan order per pasien. Pelaksanaan selalu memakai order, bukan template langsung | `RWI-DEC-146` (1), (2), (4) |
| 14 | Template, order, dan pelaksanaan milik `PharmacyManagement`; tabel baru di sana wajib dicatat eksplisit pada amandemen desain | `RWI-DEC-147`; `RWI-DEC-062` |
| 15 | Satu sumber dosis dan satu tempat simpan: GDS bangsal di Pengawasan Harian, dirujuk oleh pelaksanaan; hasil laboratorium tidak pernah menjadi sumber dosis | `RWI-DEC-148` |
| 16 | Pengawasan Harian wajib menyediakan pencatatan gula darah pada rilis yang sama dengan sliding scale | `RWI-DEC-148` konsekuensi (2) |
| 17 | Intake obat dan darah adalah entri intake terstruktur, volume aktual termasuk pelarut; entri obat tertaut satu dosis MAR `Administered`; MAR tidak mendapat isian volume | `RWI-DEC-149` (1)–(4) |
| 18 | Handover shift dan transfusi tetap `P1` dan tidak dicabut, tetapi tanpa menu, tabel, maupun endpoint pada penyelarasan ini | `RWI-DEC-145` (4); `RWI-AC-221` |

### 15.8 Status gap lama setelah keputusan

| ID | Semula (revision `1.5`) | Kini | Alasannya |
|---|---|---|---|
| `G-08` | `MISSING` / `BLOCKING` untuk intake obat dan darah | **`CONFIRMED`** — tertutup | `RWI-DEC-149` menjawab sumber, volume pelarut, dan hubungan dengan MAR |
| `G-19` | `MISSING` / `BLOCKING` untuk `INP-S19` | **`CONFIRMED`** — tertutup | `RWI-DEC-145` menetapkan penempatan rilis; `RWI-DEC-146` s.d. `148` menjawab pemilik protokol, pengesah, dan sumber gula darah |
| `G-20` | `CONFLICT` / `BLOCKING` untuk transfusi | **`CONFLICT` tetap, tetapi transfusi `DEFERRED`** | Pertentangan `FR-MVP-KEP-019` dengan blueprint `bank-darah` belum diselesaikan. Karena transfusi keluar dari batas rilis, butir ini **tidak menahan slice aktif mana pun**. Dibuka kembali lewat `RWI-OQ-093` saat transfusi dijadwalkan |

Butir `G-01` s.d. `G-07`, `G-09` s.d. `G-18`, dan `G-21` tidak berubah.

### 15.9 Gap baru

Seluruh gap baru **tidak memblokir**. Setiap usulan tetap usulan sampai pemilik memilihnya.

| ID | Slice | Butir | Status | Dampak | Alasan dan usulan |
|---|---|---|---|---|---|
| G-22 | `S19` | **Jadwal pemeriksaan GDS pada order, dan letak dosis sliding scale dalam lifecycle MAR** | `MISSING` | `NON_BLOCKING_STANDARD` | `FR-MVP-KEP-010` hanya mengenal dosis terjadwal dengan enam status, `FR-MVP-KEP-013` mengenal PRN, dan `RWI-DEC-145` s.d. `148` tidak menyebut jadwal. **Usulan:** order memuat jadwal pemeriksaan yang ditulis dokter, seperti frekuensi resep. Setiap jadwal menjadi dosis terjadwal MAR yang dosisnya baru ditentukan saat GDS tercatat. Hasil pada rentang tanpa insulin dicatat `Held` dengan alasan dari aturan skala. **Contoh:** order "cek GDS 06.00, 11.00, 17.00" menghasilkan tiga dosis `Due`; pukul 17.00 GDS 130 jatuh pada "< 150 → 0 unit", sehingga dosis itu `Held` beralasan "GDS di bawah rentang pemberian". Usulan ini tidak menambah status dan tidak mengubah kepemilikan. **Disarankan dikonfirmasi pemilik saat approval `RLN-PH-06`** |
| G-23 | `S19` | Cakupan rentang template | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan `RWI-DEC-146` (1) "setiap hasil jatuh ke tepat satu rentang": template wajib mencakup seluruh kemungkinan nilai GDS, termasuk rentang terbuka di bawah dan di atas. **Contoh:** v2 pada contoh keputusan mulai dari 150, sehingga GDS 120 tidak jatuh ke rentang mana pun; versi itu belum boleh disahkan sampai ada rentang "< 150". Isi tindak lanjut hipoglikemia, misalnya GDS < 70, adalah isi klinis template dan ikut gerbang produksi |
| G-24 | `S19` | Instruksi tindak lanjut pada rentang, misalnya "≥ 300 → 8 unit dan lapor dokter" | `MISSING` | `CONFIGURABLE_DEFAULT` | Belum ditetapkan apakah instruksi itu hanya tampil ke perawat atau juga mengirim notifikasi ke dokter. Rute notifikasi tidak mengubah catatan pelaksanaan maupun dosis, pola yang sama dengan `G-15`. **Usulan bawaan:** instruksi tersimpan sebagai isi rentang dan tampil di layar pelaksanaan; notifikasi aktif mengikuti keputusan penerima pada `G-15` |
| G-25 | `S19`, `S17` | Satuan GDS: mg/dL atau mmol/L | `MISSING` | `NON_BLOCKING_STANDARD` | Pilihan satuan tidak mengubah struktur, tetapi salah satuan berbahaya: 280 mg/dL setara sekitar 15,6 mmol/L. **Usulan:** satuan disimpan eksplisit bersama nilai pada catatan gula darah dan pada rentang template; pencocokan skala **menolak** bila satuannya berbeda, bukan mengonversi diam-diam |
| G-26 | `S17`, `S18` | Koreksi dosis MAR yang sudah tertaut entri intake | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-149` mengatur tautan saat dibuat, dan `RWI-DEC-116` (c) mengatur koreksi MAR tanpa menimpa. Belum ada yang mengatur nasib entri intake bila dosisnya dikoreksi kemudian. **Usulan yang tidak mengubah data diam-diam:** entri intake tidak dihapus atau diubah otomatis; entri itu ditandai "dosis tertaut dikoreksi", lalu perawat mengoreksinya lewat koreksi intake beralasan. **Contoh:** dosis Ceftriaxone 08.00 dikoreksi karena hanya separuh yang masuk; entri "Obat — 100 ml" tetap terhitung sampai perawat mengoreksinya menjadi 50 ml. Bila pemilik ingin total langsung mengeluarkan entri itu, yang berubah hanya aturan hitung, bukan struktur. **Disarankan dikonfirmasi pemilik saat approval `RLN-PH-06`** |
| G-27 | `S17` | Dosis `Administered` bervolume yang belum punya entri intake | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-149` mencegah hitung ganda, tetapi tidak mewajibkan entri untuk setiap dosis, sehingga risiko "terlewat" pada `DEC-INP-010` masih ada. **Usulan:** entri tetap tidak diwajibkan, sesuai keputusan. Desain boleh menampilkan penanda informatif yang tidak menahan apa pun. **Contoh:** hari ini Budi mendapat tiga dosis intravena `Administered`, dua sudah punya entri intake; Pengawasan Harian menampilkan "1 dosis intravena belum dicatat volumenya" |
| G-28 | `S19`, `S17` | Tagihan pemeriksaan GDS bangsal, misalnya strip glukometer | `MISSING` | `NON_BLOCKING_STANDARD` | Pengawasan Harian tidak menagih (14.4 dimensi 16), dan pelaksanaan bukan tagihan `BR-RWI-013`. Bila rumah sakit menagih pemeriksaan GDS, jalurnya tindakan keperawatan `CAP-014` yang sudah siap sejak revision `1.4`, atau pemakaian alat `CAP-016` yang `DEFERRED` — **bukan** sliding scale. Struktur sliding scale tidak berubah. Pilihannya dikonfirmasi pemilik Billing sebelum rilis |
| G-29 | `S19` | Nasib catatan sliding scale V1 yang berupa catatan bebas tanpa protokol | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-145` konsekuensi (3) memasukkannya ke cakupan migrasi `OPEN-MVP-010`, sedangkan `RWI-AC-219` menolak pelaksanaan tanpa protokol. **Usulan:** catatan V1 dibawa sebagai riwayat baca-saja, bukan pelaksanaan sliding scale V2 dan bukan dosis MAR. Keputusan cutover tetap milik Product + Data Owner |

**Rekap status gap revision `1.6`:** 29 butir tercatat (`G-01` s.d. `G-29`). 2 kini `CONFIRMED`
(`G-08`, `G-19`); 6 `PROPOSED`; 20 `MISSING`; 1 `CONFLICT` (`G-20`, transfusi `DEFERRED`). **Nol butir
`BLOCKING` untuk slice aktif.**

### 15.10 Gap teknis, bukan keputusan bisnis

| Butir | Sifatnya | Rujukan |
|---|---|---|
| Sliding scale, template, order, Pengawasan Harian termasuk gula darah dan intake/output, serta MAR belum punya model di source | **Ketersediaan implementasi** | `V2-CAP-10`, `RLN3-CAP-09`, `RLN3-CAP-15` — ketiganya `Missing` |
| Kolom pemilik `V2-CAP-10` pada capability map masih `ClinicalManagement` | **Catatan basi pada peta "apa yang ada"**. Untuk "apa yang harus dibangun", `RWI-DEC-147` menang: `PharmacyManagement`. Status `Missing` tetap benar. Diperbarui pada impact scan berikutnya | `01-existing-capability-map.md` bagian 16 |
| Dua kontrak rujukan lintas modul: `PharmacyManagement` membaca gula darah `ClinicalManagement`, dan entri intake `ClinicalManagement` merujuk dosis MAR `PharmacyManagement` beserta aturan satu dosis satu entri | **Pekerjaan desain kontrak**, milik `design-business-module` | `RWI-DEC-148` konsekuensi (1); `RWI-DEC-149` konsekuensi (2) |
| Baris kepemilikan baru `PharmacyManagement` pada `02-module-map.md`, dan pencatatan eksplisit tabel baru di modul itu | **Pekerjaan desain** | `RWI-DEC-147` konsekuensi (1), (3) |
| `keperawatan/04-prd-to-mvp.md` bagian 21.6 masih mencatat sliding scale tertunda | **Dokumen basi**, diperbarui pada amandemen `RLN-PH-06`. Handover shift dan transfusi tetap tertunda di sana | `RWI-DEC-145` konsekuensi (2) |
| Persetujuan pemilik `MedicalRecordManagement` atas daftar catatan terkunci milik penulis | **Dependency persetujuan modul tetangga**, tidak berubah dari 14.7. Menahan implementasi bagian itu, bukan desain | `RWI-DEC-142` butir (3); Yoga Aji Pratama |

### 15.11 Decision Log — status akhir

Decision ID: `DEC-INP-010`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Jawaban | Volume aktual dicatat perawat sebagai entri intake terstruktur bersumber "Obat" atau "Darah", termasuk pelarut. Entri obat tertaut ke satu dosis MAR `Administered`, satu dosis satu entri. MAR tidak mendapat isian volume. Intake darah tanpa tautan transfusi |
| Ditutup oleh | `RWI-DEC-149`; `RWI-OQ-092` |
| Status | **`CLOSED`** |
| Dampak implementasi/domain | Sub-bagian intake obat dan darah **boleh dirancang**. Kontrak rujukan ke dosis MAR wajib dirancang. Gap tersisa `G-26`, `G-27` tidak memblokir |

Decision ID: `DEC-INP-011`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Jawaban | Sliding scale masuk bersama MAR; handover shift dan transfusi ditunda. Protokol dua lapis berversi, milik `PharmacyManagement`, disahkan pemilik klinis. Sumber dosis hanya GDS bangsal di Pengawasan Harian |
| Ditutup oleh | `RWI-DEC-145` s.d. `RWI-DEC-148`; `RWI-OQ-091`, `094`, `095`, `096` |
| Status | **`CLOSED`** |
| Dampak implementasi/domain | Sliding scale **boleh dirancang** bersama MAR. Handover shift dan transfusi **`DEFERRED`**. Pemakaian untuk pasien sungguhan menunggu pengesahan isi template |

Decision ID: `DEC-INP-012`

| Field | Isi |
|---|---|
| Pertanyaan | Tetap seperti 14.8 |
| Bukti saat ini | Tetap seperti 14.8; `G-20` masih `CONFLICT` |
| Pemilik | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Status | **`DEFERRED`** oleh `RWI-DEC-145` butir (5); alias `RWI-OQ-093` `DITUNDA` |
| Pemicu dibuka kembali | Transfusi dijadwalkan masuk irisan rilis. Saat itu juga dibahas cara intake darah diturunkan dari catatan transfusi, `RWI-DEC-149` butir (5) |
| Dampak implementasi/domain | Tidak menahan slice aktif. Desain **MUST NOT** membuat tabel, menu, atau endpoint transfusi |

### 15.12 Kesiapan per slice dan kemampuan

Baris yang berubah dari 14.9 dicetak tebal.

| Slice | Kemampuan/isi | Kesiapan | Blocker bisnis | Catatan |
|---|---|---|---|---|
| `INP-S17` | Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Evaluasi Awal (MPP), Perencanaan Pulang, Progres Pengkajian | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah dari 14.9 |
| **`INP-S17`** | **Pengawasan Harian Pasien, termasuk intake obat dan darah** | **`READY_FOR_DOMAIN_DESIGN`** | — | Naik dari `PARTIALLY_READY`. Pencatatan gula darah wajib hadir pada rilis sliding scale. `G-25` s.d. `G-28` |
| `INP-S18` | MAR | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah. Dua pengguna baru: tautan intake obat dan dosis sliding scale. MAR **tidak** mendapat isian volume |
| `INP-S18` | Rekonsiliasi obat saat admisi | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah |
| **`INP-S19`** | **Sliding scale** | **`READY_FOR_DOMAIN_DESIGN`** | — | Naik dari `BUSINESS_DECISION_REQUIRED`. Dirancang bersama MAR `INP-S18`. `G-22` s.d. `G-25`, `G-28`, `G-29`. Isi template gerbang produksi |
| **`INP-S19`** | **Handover shift** | **`DEFERRED`** — tidak dinilai | — | Dikeluarkan dari batas rilis oleh `RWI-DEC-145` (4); requirement tetap `P1` |
| **`INP-S19`** | **Transfusi** | **`DEFERRED`** — tidak dinilai | — | `RWI-DEC-145` (4), (5). `DEC-INP-012` dan `G-20` ikut ditunda |
| `INP-S20` | Enam layanan penunjang | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah; empat layanan non-laboratorium/radiologi sebatas permukaan |
| `INP-S21` | Dokumentasi klinis dan ruang kerja dokter V2 | `READY_FOR_DOMAIN_DESIGN` | — | Tidak berubah |

**Hasil turunan:**

- `INP-S17` **`READY_FOR_DOMAIN_DESIGN`** — naik dari `PARTIALLY_READY`.
- `INP-S18` `READY_FOR_DOMAIN_DESIGN` — tetap.
- `INP-S19` **`READY_FOR_DOMAIN_DESIGN` terbatas pada sliding scale** — naik dari
  `BUSINESS_DECISION_REQUIRED`; handover shift dan transfusi `DEFERRED`.
- `INP-S20` dan `INP-S21` `READY_FOR_DOMAIN_DESIGN` — tetap.
- Seluruh slice penyelarasan `PRD-RWI-V2-001` kini **tanpa blocker keputusan bisnis** di dalam batas rilisnya.
- Overall Rawat Inap tetap **`PARTIALLY_READY`**, karena `INP-S07`, `S08`, `S09`, `S10`, `S15`, dan `S16`
  berada di luar penilaian ini dan tidak dinilai ulang.

### 15.13 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke amandemen desain `RLN-PH-06`:**

- `INP-S17` seluruhnya, termasuk intake obat dan darah serta pencatatan gula darah;
- `INP-S18` MAR dan rekonsiliasi obat saat admisi;
- `INP-S19` sliding scale — template, order per pasien, dan pelaksanaan;
- `INP-S20` seluruhnya, dengan empat layanan non-laboratorium/radiologi sebatas permukaan;
- `INP-S21` seluruhnya.

**Harus berhenti:**

- **handover shift dan transfusi**, sampai dijadwalkan ke irisan berikutnya. Tidak ada menu, tabel,
  maupun endpoint untuk keduanya (`RWI-AC-221`);
- tautan intake darah ke catatan transfusi, sampai transfusi dibangun;
- **pemakaian untuk pasien sungguhan** atas sliding scale sampai isi template disahkan pemilik klinis
  yang belum ditunjuk; atas insulin dan obat high-alert lain sampai daftarnya disahkan; serta atas
  Resiko Jatuh, isian wajib pengkajian, dan checklist Evaluasi Awal. Semua ini menahan **rilis**, bukan
  desain;
- **implementasi** daftar catatan terkunci "Catatan Saya", sampai Yoga Aji Pratama menyetujui
  (`RWI-DEC-142`). Ini tidak menahan desain.

**Dependency di antara slice yang boleh berjalan:**

```text
INP-S18 MAR ───────────────┬──► INP-S17 intake obat   (entri merujuk dosis Administered)
                           │
INP-S17 gula darah ────────┴──► INP-S19 sliding scale (dosis dicatat di MAR, GDS dirujuk dari Pengawasan Harian)

INP-S17 intake darah  ──  berdiri sendiri (tanpa tautan transfusi)
```

Karena itu MAR, pencatatan gula darah, intake obat, dan sliding scale sebaiknya dirancang dalam satu
amandemen `keperawatan` dan satu gelombang rilis. Urutan pembangunannya ditetapkan `plan-module-delivery`.

### 15.14 Keputusan pemilik yang dibutuhkan

| Jenis | Butir | Menahan | Pemilik |
|---|---|---|---|
| Blocker desain | **Tidak ada** | — | — |
| Non-blocking, disarankan dikonfirmasi saat approval `RLN-PH-06` | `G-22` jadwal dan lifecycle dosis sliding scale; `G-26` koreksi dosis MAR yang tertaut intake | — | Muhammad Hamzah |
| Non-blocking lain | `G-24` notifikasi "lapor dokter"; `G-25` satuan GDS; `G-27` penanda dosis tanpa entri intake; `G-28` tagihan GDS; `G-29` catatan V1; serta `G-05`, `G-09`, `G-17` dari bagian 14 | — | Muhammad Hamzah; `G-28` pemilik Billing; `G-29` Product + Data Owner |
| Gerbang produksi — **bertambah** | Pengesahan isi template protokol sliding scale (`RWI-DEC-146`) | Rilis sliding scale | Pemilik klinis — **belum ditunjuk** |
| Gerbang produksi — tetap | `RWI-OQ-056`, `RWI-OQ-057`, daftar high-alert termasuk insulin, checklist Evaluasi Awal | Rilis | Pemilik klinis/Farmasi — belum ditunjuk |
| Tidak dapat dijawab lewat wawancara | `RWI-OQ-064` Pemakaian Alat | `CAP-016` | Menunggu modul persediaan/aset |
| Ditunda | `RWI-OQ-093` pemilik alur transfusi | Transfusi, saat dijadwalkan | Muhammad Hamzah bersama pemilik modul Bank Darah |
| Dependency implementasi | Daftar catatan terkunci milik penulis (`RWI-DEC-142`) | Implementasi bagian itu | Yoga Aji Pratama |

### 15.15 Handoff

| Field | Nilai |
|---|---|
| `capability_scope` | `INP-S17` seluruhnya, `INP-S18`, `INP-S19` sliding scale, `INP-S20`, `INP-S21` |
| `requirement_readiness` | `S17`, `S18`, `S19` (sliding scale), `S20`, `S21` `READY_FOR_DOMAIN_DESIGN`; handover shift dan transfusi `DEFERRED` |
| `requirement_evidence_status` | Mayoritas `CONFIRMED`; 29 butir gap: 2 tertutup `CONFIRMED`, 6 `PROPOSED`, 20 `MISSING`, 1 `CONFLICT` pada transfusi yang `DEFERRED`. Nol `BLOCKING` untuk slice aktif |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN`, dan tidak diperlukan. Sliding scale dan intake obat memang melintasi `ClinicalManagement` dan `PharmacyManagement`, tetapi kepemilikan data dan arah rujukannya sudah ditetapkan pemilik lewat `RWI-DEC-147` s.d. `149`; yang tersisa desain kontrak. `RLN-PH-05` baru relevan saat transfusi dijadwalkan bersama `DEC-INP-012` |
| `decision_ids` | `DEC-INP-010` `CLOSED`, `DEC-INP-011` `CLOSED`, `DEC-INP-012` `DEFERRED`; `RWI-DEC-145` s.d. `149`; `RWI-AC-219` s.d. `231` |
| `dependency_ids` | Persetujuan `MedicalRecordManagement` `RWI-DEC-142`; komponen `rawat-jalan` `RLN3-CAP-03`; gerbang produksi isi template sliding scale, daftar high-alert, `RWI-OQ-056`, `RWI-OQ-057`, checklist Evaluasi Awal |
| `next_owner` | `design-business-module` untuk amandemen `dokter-rawat-inap` dan `keperawatan`, fase `RLN-PH-06` |
| `required_boundary` | Dokumentasi keperawatan dan dokter, termasuk entri intake dan gula darah, tetap milik `ClinicalManagement`. MAR, rekonsiliasi, template, order, dan pelaksanaan sliding scale milik `PharmacyManagement`. Desain **MUST NOT**: membuat tabel sliding scale di `ClinicalManagement` atau `InPatientManagement` (`RWI-AC-225`); menyimpan salinan GDS pada pelaksanaan sliding scale (`RWI-DEC-148` (2)); menambah isian volume pada MAR (`RWI-DEC-149` (4)); membuat menu, tabel, atau endpoint handover shift dan transfusi (`RWI-AC-221`); memakai hasil laboratorium sebagai sumber dosis (`RWI-AC-228`) |
| `expected_output` | Amandemen blueprint kedua sub-modul revision `7`: sliding scale dan intake obat/darah dirancang penuh bersama MAR, termasuk dua kontrak rujukan lintas modul dan baris kepemilikan `PharmacyManagement` pada `02-module-map.md`; handover shift dan transfusi ditandai `DEFERRED`; `G-22` dan `G-26` diajukan untuk dikonfirmasi saat approval; `keperawatan/04-prd-to-mvp.md` bagian 21.6 diperbarui |

---

## 16. Evaluasi Gerbang Kelengkapan Requirement — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

Evaluasi ini dilakukan pada **17 September 2026** menyusul penuntasan wawancara keputusan bisnis
(Pass A — Muhammad Hamzah) dan audit kemampuan eksisting. Tujuannya adalah memastikan bahwa seluruh
dimensi requirement untuk integrasi operasional dan keuangan antara Rawat Inap (`InPatientManagement`)
dan Kasir/Billing (`BillingManagement`) telah lengkap, berbukti, bebas dari kontradiksi bisnis, dan siap
melangkah ke tahap perancangan domain arsitektur (`hospital-domain-architect`) serta blueprint modul
(`design-business-module`).

### 16.1 Scope, Identitas Slice, dan Bukti Acuan

| Atribut | Rincian |
|---|---|
| **Slice ID** | **`INP-S22`** |
| **Nama Slice** | **Integrasi Rawat Inap ↔ Kasir / Billing (Inpatient to Billing Integration)** |
| **Domain Modul** | `InPatientManagement` (Rawat Inap) bertukar data dengan `BillingManagement` (Kasir & Tagihan Rumah Sakit) |
| **Kemampuan yang Dicakup** | 6 Kemampuan Integrasi Kanonik: `INT-CAP-01` s.d. `INT-CAP-06` (alias `RANAP-INT-001` s.d. `006` pada PRD Integrasi) |
| **Dokumen Sumber Bisnis Primer** | `docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md` (2.282 baris, v1.0.0) |
| **Bukti Keputusan Bisnis Terkunci** | [`00-interview-decisions.md`](../00-interview-decisions.md) revision `25`, memuat keputusan `RWI-DEC-156` s.d. `RWI-DEC-161` dan kriteria penerimaan `RWI-AC-236` s.d. `RWI-AC-241` |
| **Bukti Kemampuan Eksisting** | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revision `1.5` Bagian 18 (`INT-CAP-01` Extend, `INT-CAP-02` Extend, `INT-CAP-03` Missing, `INT-CAP-04` Extend, `INT-CAP-05` Repair & Extend, `INT-CAP-06` Missing & Reuse with Adapter) |
| **Penanggung Jawab / Domain Owner** | Muhammad Hamzah (Product & Domain Owner Modul Rawat Inap) |

---

### 16.2 Evaluasi 18 Dimensi Kelengkapan Minimum

Sesuai piagam rekayasa Quilvian, setiap slice kemampuan wajib dievaluasi terhadap 18 dimensi
kelengkapan sebelum arsitektur target disusun.

| No | Dimensi Kelengkapan | Status | Evaluasi Bukti & Penjelasan Detail |
|---:|---|:---:|---|
| 1 | **Tujuan Bisnis** | `CONFIRMED` | Menjamin sinkronisasi otomatis dan nir-desinkronisasi antara status perawatan rawat inap pasien dengan tagihan di kasir rumah sakit. Mencegah kebocoran pendapatan (*revenue leakage*), sengketa jam sewa kamar (*room charge*), ketidaksesuaian kelas tarif saat mutasi pasien, serta mencegah pasien pulang tanpa pelunasan kasir (*billing clearance*). |
| 2 | **Aktor & Pemangku Kepentingan** | `CONFIRMED` | Teridentifikasi dengan tegas: (1) **Staf Admisi Rawat Inap** (menerima pasien & penempatan awal); (2) **Perawat Bangsal / Kepala Ruangan** (konfirmasi penempatan bed fisik, pantau status pembayaran tanpa rupiah, konfirmasi kepulangan fisik); (3) **Staf Kasir / Billing** (verifikasi komponen biaya, terima pembayaran, terbitkan/cabut clearance); (4) **Supervisor Bangsal / Supervisor Kasir** (otorisasi mutasi/koreksi saat billing `OPEN`, supervisor override pemulangan saat darurat); (5) **Dokter DPJP** (memberi instruksi izin pulang klinis / `DischargeRequested`); (6) **Pasien / Penjamin** (pihak yang menyelesaikan administrasi); (7) **Sistem Otomasi / Worker** (outbox processor pengirim event). |
| 3 | **Pemicu & Prasyarat (Triggers/Preconditions)** | `CONFIRMED` | **Prasyarat & Pemicu Terkunci:**<br>• *Sinkronisasi Tagihan:* Dipicu saat status admisi pasien mencapai status resmi `Admitted` (`RWI-DEC-156`). Pasien berstatus *Booking* atau *Draft* tidak boleh men-generate tagihan aktif.<br>• *Room Charge:* Dipicu sejak tempat tidur berstatus `Bed Occupied` secara fisik (`RWI-DEC-156`).<br>• *Koreksi/Mutasi:* Dipicu oleh aksi mutasi kamar atau koreksi admisi oleh Supervisor, dengan prasyarat status folio kasir masih `OPEN` (`RWI-DEC-157`).<br>• *Pelepasan Pasien:* Dipicu setelah adanya izin pulang DPJP (`DischargeRequested`) DAN clearance kasir (`PaymentCleared` / `ClearanceApproved`, `RWI-DEC-158`, `RWI-DEC-159`). |
| 4 | **Alur Utama (Happy Path)** | `CONFIRMED` | Alur proses bisnis 6 tahap berjalan runtut:<br>1. *Admisi Masuk:* Pasien diterima di bangsal → Status `Admitted` → Rawat Inap menerbitkan event `ADMISSION_CONFIRMED` via Outbox → Billing membuat `BillingFolio` berstatus `OPEN`.<br>2. *Penempatan Bed & Charge:* Perawat konfirmasi `Bed Occupied` → Event `BED_OCCUPIED` dikirim → Billing mulai mencatat kalkulasi harian kamar.<br>3. *Pemantauan di Bangsal:* UI Rawat Inap menampilkan lencana tagihan operasional kasir (misal: "Tagihan Berjalan", "Menunggu Kasir") tanpa memuat angka rupiah (`RWI-DEC-160`).<br>4. *Instruksi Pulang:* DPJP menerbitkan `DischargeRequested` → Kasir menerima notifikasi untuk finalisasi biaya.<br>5. *Pelunasan Kasir:* Pasien melunasi tagihan → Kasir terbitkan `ClearanceApproved` → Event clearance masuk ke Rawat Inap → Tombol kepulangan fisik diaktifkan.<br>6. *Kepulangan Fisik Pasien:* Pasien meninggalkan ruangan secara nyata → Perawat klik `Pasien Pulang Fisik` (`PhysicallyLeftAt`) → Event `BED_RELEASED` terbit → Durasi hunian final dihitung (`OccupancyEndAt = PhysicallyLeftAt`, `RWI-DEC-159`) → Billing menutup tagihan (`CLOSED`). |
| 5 | **Alur Alternatif & Exception** | `CONFIRMED` | Ditutup lengkap oleh keputusan wawancara:<br>• *Koreksi Kamar/Kelas saat OPEN:* Supervisor dapat mengoreksi data dengan alasan wajib; sistem membuat versi baru (tanpa hard-delete) dan menerbitkan event `OCCUPANCY_CORRECTED` (`RWI-DEC-157`).<br>• *Koreksi Kamar saat CLOSED/LOCKED:* Sistem menolak mutasi kamar langsung; wajib melalui proses un-finalizing di Kasir (`RWI-DEC-157`).<br>• *Pencabutan Clearance (Auto-Reblock):* Jika ada tagihan susulan (misal obat/lab darurat menit akhir) dan Kasir mencabut clearance (`ClearanceRevoked`), Rawat Inap seketika mengunci kembali tombol pelepasan pasien (*Auto-Reblock*, `RWI-DEC-158`).<br>• *Supervisor Override:* Jika terjadi kondisi kedaruratan klinis / rujukan kritis yang tidak boleh tertahan administrasi kasir, Supervisor Bangsal dapat melakukan override beralasan wajib (`RWI-DEC-158`, `RWI-DEC-015`).<br>• *Gangguan Jaringan / Downtime:* Event tersimpan aman di outbox lokal dan di-retry otomatis dengan exponential backoff (`RWI-DEC-161`). |
| 6 | **Data Minimum** | `CONFIRMED` | Terdefinisi lengkap pada level payload integrasi:<br>• Entitas Admisi: `AdmissionId`, `PatientId`, `MedicalRecordNumber`, `AdmissionDateTime`, `AdmissionStatus`.<br>• Entitas Hunian Bed: `OccupancyId`, `BedId`, `BedCode`, `RoomId`, `RoomName`, `ClassCategory`, `ClassDailyRate`, `OccupancyStartAt`, `OccupancyEndAt`, `PhysicallyLeftAt`, `Version`, `ChangeReason`.<br>• Entitas Outbox: `OutboxMessageId`, `IdempotencyKey` (`SourceDomain:SourceType:SourceDetailId:Version`), `EventType`, `PayloadJson`, `CreatedAtUtc`, `Status`, `RetryCount`, `LastAttemptAtUtc`, `LastError`.<br>• Kontrak Status Kasir: `FolioId`, `FolioStatus`, `ClearanceStatus`, `BlockerReasons[]` (string informatif tanpa saldo). |
| 7 | **Aturan Bisnis & Validasi** | `CONFIRMED` | Terkunci oleh 6 aturan utama:<br>• `RWI-RULE-INT-001`: Tidak ada tagihan aktif sebelum admisi `Admitted`.<br>• `RWI-RULE-INT-002`: Room charge dihitung murni sejak `Bed Occupied`.<br>• `RWI-RULE-INT-003`: Koreksi kamar hanya saat billing `OPEN`, otorisasi Supervisor, wajib mencatat alasan, immutable history.<br>• `RWI-RULE-INT-004`: Pelepasan fisik pasien dilarang tanpa clearance kasir aktif atau supervisor override.<br>• `RWI-RULE-INT-005`: Auto-reblock seketika saat clearance revoked.<br>• `RWI-RULE-INT-006`: `OccupancyEndAt` identik dengan `PhysicallyLeftAt`.<br>• `RWI-RULE-INT-007`: Layar bangsal tanpa nominal rupiah (`InpatientBilling:View` diperlukan untuk melihat rupiah).<br>• `RWI-RULE-INT-008`: Outbox idempoten dengan compound key unik. |
| 8 | **Status & Perubahan Status (Lifecycle)** | `CONFIRMED` | Siklus hidup terdefinisi jelas di kedua modul:<br>• Siklus Admisi: `Draft` → `Admitted` → `Active` → `DischargeRequested` → `PaymentCleared` → `Discharged` → `Closed`.<br>• Siklus Hunian Bed: `Assigned` → `Occupied` → `Vacating` → `Released` (status koreksi: `Superseded`).<br>• Siklus Clearance Kasir: `None` → `Pending` → `Cleared` → (`Revoked` / `Overridden`) → `Finalized`.<br>• Siklus Outbox: `Pending` → `Processing` → `Published` / `Failed` (retry exponential backoff). |
| 9 | **Peran & Otorisasi (Authorization)** | `CONFIRMED` | Pembagian wewenang kedap dan jelas:<br>• `InpatientAdmission:Write`: Membuat admisi dan penempatan kamar awal.<br>• `InpatientNurse:Write`: Konfirmasi bed occupied, request discharge, konfirmasi fisik pulang.<br>• `InpatientSupervisor:Override`: Koreksi kamar/kelas saat billing OPEN, supervisor override discharge saat status clearance revoked.<br>• `InpatientBilling:View`: Hak khusus melihat rincian angka rupiah tagihan rawat inap.<br>• `BillingStaff:Write`: Kasir pelunasan dan penerbitan/pencabutan clearance tagihan. |
| 10 | **Dependency Antarmodul** | `CONFIRMED` | Teridentifikasi secara tegas:<br>• Modul `BillingManagement`: penyedia siklus hidup `BillingFolio`, kalkulasi room rate, clearance status, dan penutupan tagihan.<br>• Modul `BedManagement` / `MasterData`: penyedia master tempat tidur, kelas kamar, dan tarif acuan.<br>• Modul `MedicalRecordManagement`: sinkronisasi nomor rekam medis dan resume medis pemulangan.<br>• Modul `UserManagement`: otorisasi peran perawat, supervisor, dan staf kasir. |
| 11 | **Integrasi Internal / Eksternal** | `CONFIRMED` | • Integrasi Internal: Asinkronus berbasis Transactional Outbox (`InpIntegrationOutbox`) untuk pengiriman event dari Rawat Inap ke Kasir/Billing, serta REST API internal terproteksi JWT untuk kueri status kasir secara langsung.<br>• Integrasi Eksternal: Tidak ada integrasi eksternal langsung pada slice `INP-S22`. Modul Billing bertanggung jawab atas jembatan BPJS/Klaim Eksternal. |
| 12 | **Hasil Akhir (Outcome)** | `CONFIRMED` | Terwujudnya integrasi yang akurat, real-time, dan taat audit. Pasien pulang dengan administrasi beres, kamar langsung siap dibersihkan dan dialokasikan ke pasien baru, dan tidak terjadi kehilangan pendapatan sewa kamar bagi rumah sakit. |
| 13 | **Pembatalan & Koreksi** | `CONFIRMED` | Mekanisme koreksi terdefinisi utuh: pembatalan admisi menerbitkan event pembatalan tagihan jika belum ada charge; koreksi kamar/waktu dilakukan melalui penambahan record versi baru (`Superseded`) dan penerbitan event `OCCUPANCY_CORRECTED` tanpa menghapus baris lama. |
| 14 | **Audit & Histori** | `CONFIRMED` | Seluruh mutasi kamar, perubahan jam hunian, penerbitan clearance, pembatalan clearance, dan supervisor override wajib mencatat jejak audit immutable: `UserId`, `TimestampUtc`, `IpAddress`, `ActionType`, `PreviousValueJson`, `NewValueJson`, dan `Reason` (`RWI-AC-237`, `RWI-AC-238`). |
| 15 | **Notifikasi & Sinyal Sistem** | `CONFIRMED` | Sinyal terkirim otomatis antar-staf: notifikasi visual ke bangsal saat clearance terbit atau dicabut; notifikasi ke kasir saat dokter menyetujui pemulangan (`DischargeRequested`) atau terjadi mutasi kamar pasien. |
| 16 | **Dampak Billing & Biaya (Charge Impact)** | `CONFIRMED` | Menjadi inti fungsionalitas slice ini: penentuan trigger awal pembuatan invoice, akumulasi sewa kamar harian (*room charge*), perhitungan selisih kelas kamar, dan validasi pelunasan kasir. |
| 17 | **Dampak Keselamatan Klinis** | `CONFIRMED` | Sangat terlindungi: Perawat bangsal terbebas dari sengketa uang dengan keluarga pasien karena UI tidak menampilkan angka rupiah (`RWI-DEC-160`). Mekanisme *Supervisor Override* menjamin pasien dalam ancaman klinis gawat/rujukan tidak tertahan di ruangan akibat kendala kasir (`RWI-DEC-158`, `RWI-DEC-015`). |
| 18 | **Pelaporan & Keterlacakan (Traceability)** | `CONFIRMED` | Setiap event integrasi membawa ID korelasi unik (`CorrelationId`, `TraceId`). Memungkinkan rekonsiliasi otomatis harian antara data sensus tempat tidur bangsal dengan daftar tagihan kamar di kasir. |

---

### 16.3 Klasifikasi Status Bukti

Dari evaluasi 18 dimensi di atas, seluruh komponen arsitektural inti berstatus **`CONFIRMED`**
berkat penutupan keputusan `RWI-DEC-156` s.d. `RWI-DEC-161` pada `00-interview-decisions.md` revision 25.

Ditemukan 5 (lima) butir gap teknis/konfigurasi, yang seluruhnya bersifat non-blocking:

| Gap ID | Dimensi Terkait | Status Bukti | Klasifikasi Dampak | Deskripsi Gap & Rekomendasi Penanganan |
|---|---|:---:|:---:|---|
| **`G-INT-01`** | Aturan Validasi / Billing | `PROPOSED` | `CONFIGURABLE_DEFAULT` | **Toleransi Jam Cut-Off Sewa Kamar Harian:** Rumah sakit umumnya menerapkan aturan cut-off (misal: checkout lewat pkl 12.00 dikenakan charge setengah hari, lewat pkl 18.00 satu hari penuh).<br>*Rekomendasi:* Diatur melalui parameter konfigurasi rumah sakit di modul Master/Billing. Modul Rawat Inap hanya wajib mengirimkan stempel waktu presisi `PhysicallyLeftAt`. |
| **`G-INT-02`** | Aturan Validasi / Billing | `PROPOSED` | `NON_BLOCKING_STANDARD` | **Perhitungan Biaya Pindah Kelas di Tengah Hari:** Jika pasien pindah kelas kamar (misal pkl 10.00 pindah dari Kelas 2 ke VIP), penentuan apakah hari itu dihitung tarif VIP penuh atau pro-rata durasi adalah domain kalkulasi `BillingManagement`.<br>*Rekomendasi:* Rawat Inap mencatat riwayat occupancy per segmen jam dengan akurat; formula perhitungan biaya diserahkan ke mesin kalkulasi billing. |
| **`G-INT-03`** | Integrasi Kontrak | `PROPOSED` | `NON_BLOCKING_STANDARD` | **Format Skema Kontrak JSON & Nama Topic / Queue Broker:** Spesifikasi struktur JSON detail untuk event outbox `ADMISSION_CONFIRMED`, `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED`.<br>*Rekomendasi:* Ditetapkan pada tahap perancangan kontrak API & integrasi di `design-business-module`. |
| **`G-INT-04`** | Notifikasi / Frontend | `PROPOSED` | `NON_BLOCKING_STANDARD` | **Metode Pembaruan Status Kasir di UI Bangsal (Polling vs WebSocket):** Apakah status clearance di UI perawat diperbarui via polling berkala (misal tiap 30 detik) atau via WebSocket/SignalR push.<br>*Rekomendasi:* Polling berkala ringan sebagai baseline yang andal, dapat ditingkatkan ke SignalR pada fase delivery frontend. |
| **`G-INT-05`** | Audit & Outbox | `PROPOSED` | `CONFIGURABLE_DEFAULT` | **Masa Retensi Riwayat Tabel Outbox:** Batas waktu penyimpanan log event outbox yang berstatus `Published` sebelum diarsipkan.<br>*Rekomendasi:* Nilai bawaan sistem 30 hari sebelum pengarsipan berkala oleh worker. |

---

### 16.4 Dampak Gap dan Ketiadaan Blocker Bisnis

Berdasarkan analisis klasifikasi dampak:
- Jumlah gap berstatus **`BLOCKING`**: **`0 (NOL)`**.
- Jumlah gap berstatus **`NON_BLOCKING_STANDARD`**: **`3 (Tiga)`** (`G-INT-02`, `G-INT-03`, `G-INT-04`).
- Jumlah gap berstatus **`CONFIGURABLE_DEFAULT`**: **`2 (Dua)`** (`G-INT-01`, `G-INT-05`).

> **Kesimpulan Ketiadaan Blocker:** Tidak terdapat satu pun ketidakpastian bisnis, pertentangan
> kewenangan klinis, atau kebuntuan regulasi rumah sakit yang menahan slice `INP-S22`. Seluruh pertanyaan
> esensial telah dijawab secara konsisten oleh Product Owner (Muhammad Hamzah). Dengan demikian,
> slice `INP-S22` dinyatakan **MEMENUHI SYARAT KELENGKAPAN REQUIREMENT**.

---

### 16.5 Skenario Contoh Konkret Rumah Sakit

Untuk menjamin pemahaman yang utuh bagi pemangku kepentingan rumah sakit (perawat, kasir, dokter, dan staf IT),
berikut adalah skenario alur terpadu yang menggambarkan penerapan aturan di lapangan:

#### Skenario A: Admisi Masuk, Penempatan Tempat Tidur, dan Pembuatan Tagihan Kasir
1. **Pasien Masuk:** Pasien bernama Budi Santoso (No. RM `RM-2026-08891`) dirujuk dari IGD ke Rawat Inap.
2. **Konfirmasi Admisi:** Petugas Admisi Rawat Inap mengonfirmasi admisi pukul 08.00 WIB → Status berubah menjadi `Admitted`.
3. **Penerbitan Event:** Sistem Rawat Inap menyimpan pesan outbox `ADMISSION_CONFIRMED` dengan kunci idempoten `INP:ADMISSION:ADM-08891:1`. Worker outbox mengirimkan event ke Modul Billing.
4. **Pembentukan Folio:** Modul Billing menerima event dan otomatis membuat tagihan terbuka `BillingFolio` dengan status `OPEN`.
5. **Penempatan Bed Fisik:** Pukul 09.30 WIB, Perawat Siti Aminah mengantar Budi ke Bangsal Melati Kamar 02 (Bed Melati-02A, Kelas 2) dan menekan tombol `Konfirmasi Penempatan Fisik`.
6. **Mulai Room Charge:** Status hunian bed berubah menjadi `Bed Occupied` (`OccupancyStartAt = 09:30`). Event `BED_OCCUPIED` dikirim via outbox ke Kasir. Mulai detik ini, perhitungan tarif kamar harian Melati Kelas 2 aktif dicatat di folio kasir.

#### Skenario B: Pindah Kamar (Mutasi) dan Koreksi Kamar saat Billing `OPEN`
1. **Permintaan Pindah:** Pada hari kedua pukul 14.00 WIB, keluarga Budi mengajukan kenaikan kelas ke Ruang VIP Aster 01.
2. **Validasi Status Kasir:** Sistem memverifikasi bahwa tagihan Budi di Modul Billing masih berstatus `OPEN`.
3. **Pencatatan Segmen Hunian:** Perawat mengonfirmasi kepindahan:
   - Segmen Bed Melati-02A ditutup pukul 14.00 WIB (`OccupancyEndAt = 14:00`).
   - Segmen Bed VIP Aster-01A dibuka pukul 14.00 WIB (`OccupancyStartAt = 14:00`).
4. **Event Mutasi:** Event `OCCUPANCY_CORRECTED` diterbitkan ke Kasir. Kasir memperbarui rincian sewa kamar tanpa menghapus data historis hari sebelumnya (`RWI-DEC-157`).
5. **Penolakan saat CLOSED:** Seandainya folio tagihan pasien telah ditutup/dikunci (`CLOSED`), sistem Rawat Inap akan langsung menolak mutasi dengan pesan peringatan: *"Tagihan pasien sudah berstatus CLOSED. Hubungi bagian Kasir/Keuangan untuk pembukaan kembali tagihan."*

#### Skenario C: Keputusan Pulang DPJP, Pelunasan Kasir, dan Kepulangan Fisik Pasien
1. **Izin Pulang DPJP:** Pada hari kelima pukul 10.00 WIB, dr. Anwar Sp.PD melakukan visite dan mengesahkan rencana pulang medis (`DischargeRequested`).
2. **Pemberitahuan ke Kasir:** Status rencana pulang tampil di dasbor Kasir Hendra. Kasir memeriksa seluruh komponen biaya (sewa kamar Melati 2 hari, VIP 2 hari, obat farmasi, jasa visite dokter, laboratorium).
3. **Tampilan di Bangsal:** Perawat Siti melihat layar bangsal Budi: tertera lencana oranye bertuliskan *"Menunggu Penyelesaian Kasir"* beserta catatan kendala *"Belum Clearance Kasir"*. Sesuai aturan `RWI-DEC-160`, **tidak ada angka saldo rupiah** yang tampil pada layar perawat.
4. **Pembayaran di Kasir:** Keluarga Budi membayar lunas di loket kasir pukul 11.30 WIB. Kasir Hendra menerbitkan tanda lunas dan menekan tombol `Setujui Clearance Kepulangan` (`PaymentCleared`).
5. **Sinyal Clearance Masuk ke Bangsal:** Layar bangsal Budi seketika berganti menjadi hijau bertuliskan *"Clearance Kasir Disetujui"*. Tombol `Pasien Pulang Fisik` pada aplikasi perawat kini aktif (tidak lagi disabled).
6. **Pasien Meninggalkan Kamar:** Pukul 12.15 WIB, setelah keluarga selesai berkemas dan menerima obat pulang, Budi meninggalkan ruangan secara fisik. Perawat Siti mengklik tombol `Pasien Pulang Fisik`.
7. **Finalisasi Jam Kamar:** Sistem mencatat `PhysicallyLeftAt = 12:15 WIB` dan secara otomatis mengunci `OccupancyEndAt = 12:15 WIB` (`RWI-DEC-159`). Event `BED_RELEASED` dikirim ke Kasir. Kasir memfinalisasi durasi sewa kamar dan mengunci invoice menjadi `CLOSED`. Tempat tidur VIP Aster-01A berubah status menjadi `Vacant / Needs Cleaning`.

#### Skenario D: Tagihan Susulan, Pencabutan Clearance (*Auto-Reblock*), dan Eksekusi *Supervisor Override*
1. **Pencabutan Clearance Kasir:** Pukul 11.45 WIB (sebelum Budi pulang fisik), bagian Farmasi mendadak menginput resep obat injeksi darurat yang tertinggal. Kasir Hendra menemukan tagihan baru dan segera menekan tombol `Cabut Clearance Tagihan` (`ClearanceRevoked`).
2. **Reaksi Auto-Reblock di Bangsal:** Secara seketika (real-time), modul Rawat Inap mendeteksi status pencabutan tersebut. Sistem mengunci kembali (*Auto-Reblock*) tombol kepulangan fisik di layar perawat dan memunculkan peringatan merah: *"Clearance kasir telah dibatalkan: Ada tagihan tambahan farmasi."* (`RWI-DEC-158`).
3. **Kondisi Kedaruratan & Kebutuhan Rujukan:** Namun, pada saat yang sama kondisi Budi mendadak mengalami komplikasi pernapasan akut dan dokter DPJP memerintahkan Budi segera dirujuk ke RS Jantung rujukan dengan ambulans siaga.
4. **Eksekusi Supervisor Override:** Menghadapi kondisi kritis tersebut, Supervisor Bangsal Ns. Dewi menggunakan wewenang darurat dengan menekan tombol `Supervisor Override Pelepasan Pasien`. Ns. Dewi memasukkan PIN otorisasi dan mengetik alasan wajib: *"Pasien darurat rujukan kritis ke RS Harapan Kita via ambulans siaga, penyelesaian administrasi kasir dilanjutkan oleh penjamin keluarga di kasir."* (`RWI-DEC-158`, `RWI-DEC-015`).
5. **Pelepasan Terlaksana & Jejak Audit:** Sistem mengizinkan Budi keluar fisik, mencatat kepulangan dengan penanda khusus `Discharged via Supervisor Override`, serta mencatat jejak audit lengkap (nama Ns. Dewi, timestamp, alasan darurat). Event `BED_RELEASED` tetap terkirim ke Billing untuk penghentian sewa kamar fisik.

---

### 16.6 Decision Log dan Keterkaitan Keputusan

Evaluasi requirement untuk slice `INP-S22` secara resmi menutup Decision ID kanonik:

| Decision ID | Status | Pertanyaan Keputusan | Keputusan Penutup & Bukti |
|---|:---:|---|---|
| **`DEC-INP-013`** | **`CLOSED`** | Bagaimana batasan dan mekanisme integrasi antara Rawat Inap dan Billing/Kasir terkait sinkronisasi admisi, room charge, koreksi hunian, auto-reblock clearance, dan privasi nominal uang? | Ditutup secara tuntas oleh keputusan wawancara `RWI-DEC-156` s.d. `RWI-DEC-161` dan kriteria penerimaan `RWI-AC-236` s.d. `RWI-AC-241` pada [`00-interview-decisions.md`](../00-interview-decisions.md) revision `25`. Tidak ada isu wewenang atau fungsionalitas yang menggantung. |

---

### 16.7 Kesiapan Per Slice dan Status Modul

| Slice ID | Nama Slice | Status Kesiapan | Catatan Evaluasi & Blocker |
|---|---|:---:|---|
| **`INP-S22`** | **Integrasi Rawat Inap ↔ Kasir / Billing** | **`READY_FOR_DOMAIN_DESIGN`** | **Siap penuh.** Tidak ada keputusan bisnis pemblokir. 18 dimensi lengkap. 5 gap teknis non-blocking. |
| `INP-S01` s.d. `INP-S06` | Admisi, Penempatan, Mutasi, Perawat, Dokter, Resep | `READY_FOR_DOMAIN_DESIGN` | Mempertahankan status kesiapan sebelumnya. |
| `INP-S09` | Serah terima IGD ke Rawat Inap | `BUSINESS_DECISION_REQUIRED` | Masih menunggu kesepakatan formal dari domain owner IGD (Rizki Gunawan). |
| `INP-S17` s.d. `INP-S21` | Pengkajian Keperawatan, MAR, Sliding Scale, Penunjang, CPPT V2 | `READY_FOR_DOMAIN_DESIGN` | Mempertahankan status kesiapan penutupan gate revision `1.6`. |
| **Keseluruhan Modul Rawat Inap** | **`InPatientManagement`** | **`PARTIALLY_READY`** | Modul secara keseluruhan tetap *Partially Ready* karena ketergantungan historis pada slice `INP-S09` (IGD) yang masih menunggu approval owner luar. Namun, hal ini **TIDAK MENGHALANGI** kemajuan slice `INP-S22`. |

---

### 16.8 Apa yang Boleh Berjalan dan Apa yang Harus Berhenti

#### Apa yang Boleh Berjalan:
1. **Perancangan Domain Arsitektur (`hospital-domain-architect`):** Pemetaan bounded context antara `InPatientManagement` dan `BillingManagement`, pemisahan aggregate root (`InpatientAdmission` vs `BillingFolio`), dan pendefinisian domain events integrasi.
2. **Penyusunan Blueprint Bisnis (`design-business-module`):** Perancangan flowchart alur data, kontrak API endpoint Swagger, kamus data tabel outbox `InpIntegrationOutbox`, serta perancangan komponen UI operasional bangsal (tanpa nominal rupiah).
3. **Penyusunan Delivery Plan (`plan-module-delivery`):** Pemecahan task implementasi berukuran kecil berbasis vertical slice setelah blueprint disetujui.

#### Apa yang Harus Berhenti / Dilarang Keras:
1. **DILARANG** menulis kode implementasi backend (C#) atau frontend (Next.js) sebelum blueprint modul dan roadmap delivery disetujui.
2. **DILARANG** membuat tabel tagihan baru atau memproses posting akun buku besar keuangan di dalam modul Rawat Inap (`RWI-DEC-156`). Seluruh tagihan dan keuangan adalah domain eksklusif `BillingManagement`.
3. **DILARANG** menampilkan angka nominal rupiah pada layar operasional keperawatan rawat inap, kecuali pengguna memiliki izin khusus `InpatientBilling:View` (`RWI-DEC-160`).
4. **DILARANG** memulangkan pasien secara fisik tanpa clearance kasir aktif atau otorisasi resmi *Supervisor Override* beralasan wajib (`RWI-DEC-158`).
5. **DILARANG** melakukan koreksi penempatan kamar langsung di Rawat Inap apabila status tagihan di Kasir telah `CLOSED` (`RWI-DEC-157`).

---

### 16.9 Handoff ke Tahap Berikutnya

| Parameter Handoff | Rincian Spesifikasi |
|---|---|
| **`capability_scope`** | Slice `INP-S22` (Kemampuan `INT-CAP-01` s.d. `INT-CAP-06` / `RANAP-INT-001` s.d. `006`) |
| **`requirement_readiness`** | **`READY_FOR_DOMAIN_DESIGN`** |
| **`requirement_evidence_status`** | Mayoritas mutlak **`CONFIRMED`**; 5 gap non-blocking (`G-INT-01` s.d. `G-INT-05`); **`0 BLOCKING`** |
| **`domain_architecture_readiness`** | **`READY`**. Dapat dilanjutkan ke `hospital-domain-architect` untuk pemetaan formal relasi aggregate lintas modul, atau langsung ke `design-business-module` bila pola aggregate integrasi dipandang cukup lugas. |
| **`decision_ids`** | `DEC-INP-013` (`CLOSED`); `RWI-DEC-156` s.d. `RWI-DEC-161`; `RWI-AC-236` s.d. `RWI-AC-241` |
| **`dependency_ids`** | Kontrak API modul `BillingManagement` (pembuatan folio, verifikasi status `OPEN`, penerbitan/pencabutan clearance) |
| **`next_owner`** | `hospital-domain-architect` atau `design-business-module` |
| **`required_boundary`** | • Modul Rawat Inap **TIDAK BOLEH** membuat tabel invoice/folio tandingan.<br>• Room charge hanya boleh aktif saat `Bed Occupied`.<br>• Waktu hunian kamar berakhir saat `PhysicallyLeftAt`.<br>• UI bangsal steril dari nominal rupiah.<br>• Mekanisme *Auto-Reblock* dan *Supervisor Override* wajib dipertahankan secara transaksional. |
| **`expected_output`** | Blueprint integrasi lengkap memuat: arsitektur event-driven outbox, kontrak API Swagger untuk kueri status kasir, kamus data tabel outbox, state-machine pemulangan pasien, flowchart alur mutasi/auto-reblock, serta spesifikasi izin peran `InpatientSupervisor:Override` dan `InpatientBilling:View`. |


---

## 17. Evaluasi Gerbang Kelengkapan Requirement — Finishing Rawat Inap (`PRD-RWI-FINISHING-001`) — revision `1.8`

### 17.1 Scope, identitas slice, dan bukti acuan

**Yang dinilai:** 16 kemampuan `CAP-RWF-01` s.d. `CAP-RWF-16` pada `PRD-RWI-FINISHING-001` v`0.2`. `CAP-RWF-17` (Resume ODC) sudah dihapus oleh `RWI-DEC-183`, sehingga tidak dinilai. Kemampuan dikelompokkan menjadi sembilan slice baru `INP-S23` s.d. `INP-S31`. Pengelompokan mengikuti kepemilikan data dan siklus hidup, bukan menu.

| Slice | Nama | Kemampuan PRD | Sub-modul pemilik (`RWI-DEC-164`) |
|---|---|---|---|
| `INP-S23` | Gerbang penutupan episode dan izin kasir | `CAP-RWF-01`, `CAP-RWF-03` | `integrasi-billing` |
| `INP-S24` | Tagihan inti rawat inap | `CAP-RWF-02`, `CAP-RWF-04` | `integrasi-billing` |
| `INP-S25` | Tagihan Pasien di bangsal dan hak lihat rupiah | `CAP-RWF-05`, `CAP-RWF-15` | `keperawatan`, `integrasi-billing` |
| `INP-S26` | Penunjang dari bangsal | `CAP-RWF-06` | `dokter-rawat-inap` |
| `INP-S27` | Pasien operasi dari bangsal | `CAP-RWF-07`, `CAP-RWF-08` | `episode-rawat-inap` |
| `INP-S28` | Catatan Keperawatan susunan V1, WSD, Efek Samping Obat, Diet Medis | `CAP-RWF-09` s.d. `CAP-RWF-12` | `keperawatan` |
| `INP-S29` | Pemakaian Alat medis besar | `CAP-RWF-13` | `keperawatan` |
| `INP-S30` | Katalog tindakan rawat inap | `CAP-RWF-14` | `dokter-rawat-inap` |
| `INP-S31` | Serah terima klinis transfer antarunit (`P2`) | `CAP-RWF-16` | `episode-rawat-inap` |

**Bukti yang dipakai:**

| Jenis bukti | Sumber | Revision / hash |
|---|---|---|
| Requirement eksplisit pemilik | `docs/module-blueprints/rawat-inap/00-interview-decisions.md`, `RWI-DEC-163` s.d. `RWI-DEC-193`, `RWI-AC-242` s.d. `RWI-AC-306` | Revision `29`, SHA-256 `f6fed608…68ce471` |
| Persetujuan pemilik modul tetangga | `RWI-DEC-190` s.d. `RWI-DEC-193` (Billing, Kamar Operasi, Gizi, Bank Darah, Master Data), disampaikan tidak langsung lewat Muhammad Hamzah | Decision log revision `29` |
| Dokumen produk | `docs/Modul-RS/Rawat-Inap/05-prd-to-mvp-finishing-rawat-inap.md` (`PRD-RWI-FINISHING-001` v`0.2`) | SHA-256 `01f4479d…5f6ca` |
| Implementasi V2 terverifikasi | `01-existing-capability-map.md` revision `1.6` bagian 19 (`FIN-CAP-01` s.d. `FIN-CAP-33`) | Backend `c8e99ce5`, frontend `22ad67330`; SHA-256 `2f78b74e…30eccd2` |
| HEAD saat gate dijalankan | Backend `425cfeae` (hanya dokumen), frontend `ee75e055b` (hanya styling, tidak menyentuh berkas pemicu bagian 19.9) | Bagian 19 tetap `CURRENT` |
| Legacy V1 | `RWI-FACT-051` (kepulangan V1), PRD v`0.2` bagian 13 | Backend V1 `4be1499c`, frontend V1 `86408f245` |
| Baseline rujukan | `indonesia-hospital-domain-reference` `references/inpatient.md`: `ID-INP-CAP-016` (serah terima tim perawatan), bagian 9 (discharge dan penutupan), bagian 10 (exception dan koreksi) | `REFERENCE_ONLY`, `Reference coverage: PARTIAL` |

**Wewenang bukti yang diterapkan.** Untuk apa yang **seharusnya dibangun**, decision log menang atas PRD (`RWI-DEC-165`), dan PRD v`0.2` menyalinnya. Untuk apa yang **sudah ada**, capability map bagian 19 yang dipakai. Baseline rujukan hanya dipakai untuk mendeteksi gap dan tidak pernah dijadikan kebijakan rumah sakit.

### 17.2 Ringkasan untuk pembaca umum

Hampir semua aturan bisnis Finishing sudah diputuskan pemiliknya, dan pemilik modul tetangga sudah setuju. Gate ini menemukan **empat pertanyaan kecil** yang masih terbuka, tetapi pertanyaan-pertanyaan itu **menyentuh bentuk data atau tagihan**, sehingga tidak boleh ditebak oleh perancang:

1. **Kapan layanan rawat inap ditagih**, terutama obat: saat diserahkan farmasi atau saat diberikan perawat, dan bagaimana obat yang dikembalikan (`DEC-INP-014`).
2. **Apa saja yang masuk biaya operasi** saat kasus selesai: tarif tindakan saja, atau juga anestesi, sewa kamar operasi, dan bahan atau implan (`DEC-INP-015`).
3. **Catatan pra-operasi yang sudah dikirim, lalu operasinya ditunda**: tetap berlaku atau wajib dikirim ulang (`DEC-INP-016`).
4. **Pasien dengan dua selang WSD** (misalnya kiri dan kanan): dicatat per selang atau digabung (`DEC-INP-017`).

Masing-masing hanya menahan **bagian kecil** dari slice-nya. Sisanya boleh langsung dirancang.

### 17.3 Matriks 18 dimensi kelengkapan

Kode isi sel: **C** = `CONFIRMED`; **P** = `PROPOSED`; **M** = `MISSING`; **X** = `CONFLICT`; **–** = tidak material untuk slice itu, dengan alasan di 17.4. Angka dalam kurung merujuk gap pada 17.5.

| No | Dimensi | `S23` | `S24` | `S25` | `S26` | `S27` | `S28` | `S29` | `S30` | `S31` |
|---:|---|---|---|---|---|---|---|---|---|---|
| 01 | Tujuan | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | C | C | C | C | C |
| 03 | Pemicu / prasyarat | C | C | C | C | C | C | C | C | C |
| 04 | Alur utama | C | C | C | C | C | C | C | C | C |
| 05 | Alur alternatif / exception | C, M (G-01) | C | C | C | C, M (G-07) | C, M (G-09) | C | C | C |
| 06 | Data minimum | C | C | C, P (G-04) | C | C, P (G-06) | C, M (G-08) | C | C | C |
| 07 | Aturan bisnis / validation | C | C, P (G-03) | C | C, P (G-05) | C | C, P (G-10) | C, P (G-12) | C, P (G-13) | C |
| 08 | Status / perubahan status | C | C | – | C | C, M (G-07) | C | C | – | C |
| 09 | Peran / authorization | C | C | C | C | C | C | C | C | C |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | C | C |
| 11 | Integrasi internal / eksternal | C | C | C | C | C | C | C | – | – |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | C |
| 13 | Pembatalan / koreksi | C | C | – | C | C | C | C | – | C |
| 14 | Audit / histori | C | C | C | C | C | C | C | – | C |
| 15 | Notifikasi | P (G-02) | – | – | P (G-02) | P (G-02) | – | – | – | P (G-02) |
| 16 | Dampak billing / charge | C | C, P (G-03) | C | C | C, M (G-06) | – | C, P (G-12) | – | – |
| 17 | Dampak keselamatan klinis | – | – | – | C, P (G-11) | C, M (G-07) | C, M (G-08) | – | – | C, P (G-11) |
| 18 | Pelaporan / traceability | C | C | C | C | C | C | C | – | C |

### 17.4 Temuan per slice

#### `INP-S23` — Gerbang penutupan episode dan izin kasir

Seluruh dimensi inti `CONFIRMED` oleh `RWI-DEC-167`, `RWI-DEC-186`, `RWI-DEC-187`, dan `RWI-DEC-192` butir (2), serta `RWI-RULE-009`, `RWI-RULE-010`, dan `RWI-RULE-036`. Alurnya: keputusan pulang → keluar ruangan dengan peringatan dan jejak → kasir menyetujui di Billing → penutupan normal yang membaca izin langsung, atau penutupan supervisor dengan permission dan alasan. Dimensi 17 tidak material, karena gerbang ini administratif dan keputusan klinis pulang tetap milik DPJP. **Gap:** G-01 dan G-02, keduanya non-blocking.

**Catatan sinkronisasi.** Bagian 16.8 "Apa yang harus berhenti" butir 4, *"DILARANG memulangkan pasien secara fisik tanpa clearance kasir"*, dan batas auto-reblock pada 16.9 **tidak berlaku lagi** sejak `RWI-DEC-186`. Gerbang kasir kini berada di penutupan episode.

#### `INP-S24` — Tagihan inti rawat inap

Pembukaan invoice `RANAP` lewat `ADMISSION_CONFIRMED`, outbox jujur dengan konfirmasi terima, tarif kamar dari linimasa bed, biaya admin untuk invoice berisi tarif kamar, larangan finalisasi bila ada "tarif belum ada", pensiun hitungan kedua, label jenis layanan seragam, koreksi penempatan, dan putar ulang: semuanya `CONFIRMED` (`RWI-DEC-157`, `RWI-DEC-166`, `RWI-DEC-169`, `RWI-DEC-192`). Dimensi 17 tidak material. Dimensi 15 tidak material, karena kasir bekerja dari invoice yang terbuka sendiri. **Gap:** G-03 **memblokir** jembatan layanan klinis.

**Catatan untuk desain (bukan gap bisnis).** Aturan bisnis koreksi penempatan sudah jelas: tarif dihitung dari linimasa yang sudah dikoreksi (`RWI-DEC-166` butir 3). Namun penanda `IsSuperseded` kini dipakai transfer biasa (`FIN-CAP-15`), sedangkan Billing menghitung semua penempatan yang tidak dihapus. Cara menandai penempatan yang dikoreksi agar tidak ikut dihitung adalah keputusan desain data, dan wajib dibuktikan lewat uji `RWI-DEC-192` butir (g).

#### `INP-S25` — Tagihan Pasien di bangsal

Kelompok V1, isi baris tanpa rupiah, subtotal bagi pemegang izin, penyaringan rupiah di server, dua permission terpisah, dan larangan "Rp 0": `CONFIRMED` (`RWI-DEC-170`, `RWI-DEC-108`). Dimensi 08 dan 13 tidak material, karena layar ini hanya membaca. **Gap:** G-04 (pemetaan kategori tarif ke kelompok V1), non-blocking.

#### `INP-S26` — Penunjang dari bangsal

Aturan pemesan seragam, pemeriksaan penugasan oleh Rawat Inap, kolom verifikasi di modul pemilik, dan pembacaan status serta hasil: `CONFIRMED` (`RWI-DEC-114`, `RWI-DEC-171`, `RWI-DEC-188`, `RWI-DEC-191`). Fakta source mendukung tanpa perubahan modul selain kolom verifikasi (`01-existing-capability-map.md` 19.6 jawaban `RWI-OQ-101`). **Gap:** G-02, G-05, dan G-11, semuanya non-blocking untuk desain.

#### `INP-S27` — Pasien operasi dari bangsal

Pemesanan yang merujuk order tindakan, Obstetri sebagai jenis kasus, status di bangsal, pra-operasi milik OK dengan dua akun, penandaan gambar tubuh dengan kecocokan sisi, syarat consent sebelum "Siap", bed tetap selama operasi, dan penerima serah terima yang sah: `CONFIRMED` (`RWI-DEC-173` s.d. `RWI-DEC-177`, `RWI-DEC-189`, `RWI-DEC-191`). **Gap:** G-06 **memblokir** biaya operasi; G-07 **memblokir** perilaku pra-operasi saat kasus ditunda.

#### `INP-S28` — Catatan Keperawatan susunan V1, WSD, Efek Samping Obat, Diet Medis

Susunan enam sub-menu, jendela tanpa salinan, empat sub-tab Obat & Alkes, narasi sebagai entri CPPT, Efek Samping lewat endpoint ADR, dan Diet Medis dengan penetap serta verifikasi: `CONFIRMED` (`RWI-DEC-172`, `RWI-DEC-178`, `RWI-DEC-188`). **Gap:** G-08 **memblokir** Observasi WSD; G-09 dan G-10 non-blocking.

#### `INP-S29` — Pemakaian Alat medis besar

Master jenis alat di `MasterData`, tarif lewat `MstTariff` dan `MstInsuranceTariff`, pemakaian di `ClinicalManagement`, perhitungan unit oleh server, penutupan otomatis saat keluar ruangan, dan koreksi yang hanya menyentuh baris sendiri: `CONFIRMED` (`RWI-DEC-179`, `RWI-DEC-180`, `RWI-DEC-192` butir 4, `RWI-DEC-193`). Dimensi 17 tidak material, karena yang dicatat adalah fakta pemakaian untuk penagihan, bukan instruksi klinis. **Gap:** G-12, non-blocking.

#### `INP-S30` — Katalog tindakan rawat inap

Penanda `IsAvailableForInpatient` `CONFIRMED` sebagai masukan desain (`RWI-DEC-165` butir 1, `FR-RWF-070`). Dimensi 08, 11, 13, 14, dan 16 s.d. 18 tidak material, karena ini perbaikan saringan katalog. **Gap:** G-13, non-blocking.

#### `INP-S31` — Serah terima klinis transfer antarunit

Kapan dokumen lahir, sembilan bagian V1, potret beku, status, penanda tertunda, penerima sah, dan kepemilikan `ClinicalManagement`: `CONFIRMED` (`RWI-DEC-182`, `RWI-DEC-189`). Dibandingkan baseline `ID-INP-CAP-016` (`REFERENCE_ONLY`), kepedulian "pihak yang menyerahkan, penerima, isi, konfirmasi penerimaan, informasi kritis" sudah tercakup. "Order atau tugas yang belum tuntas" tercakup sebagian lewat bagian "instruksi khusus". **Gap:** G-02 dan G-11, non-blocking.

### 17.5 Daftar gap dan dampaknya

| Gap | Slice | Pernyataan | Status bukti | Dampak | Penjelasan dan usulan |
|---|---|---|---|---|---|
| G-01 | `S23` | Perlakuan gerbang untuk pasien **meninggal** dan **kabur** | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-RULE-037` belum final dan sengaja di luar MVP. Cara pulang yang dimodelkan hanya tiga (`RWI-FACT-046`). Sampai difinalkan, penutupan memakai jalur supervisor dengan alasan (`RWI-DEC-185` butir 5 yang kini dibawa `RWI-DEC-186` butir 7). Tidak mengubah desain slice ini |
| G-02 | `S23`, `S26`, `S27`, `S31` | Pemberitahuan aktif kepada kasir, dokter pemverifikasi, bangsal, atau unit tujuan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Seluruh pemberitahuan berbentuk **daftar yang disegarkan berkala**, yaitu daftar "pulang sebelum izin kasir", daftar "perlu diverifikasi", status kasus OK, dan penanda "Serah terima tertunda". Notifikasi seketika `DEFERRED` (PRD 5.4) |
| G-03 | `S24` | **Titik tagih layanan klinis rawat inap**, khususnya obat (diserahkan vs diberikan menurut MAR) dan **retur obat** yang diserahkan tetapi tidak diberikan | `PROPOSED` | **`BLOCKING`** — hanya jembatan layanan klinis `RANAP` (`FR-RWF-011`) | Persetujuan Yasmina pada `RWI-DEC-192` umum dan tidak memilih titik. Titik pada PRD v`0.2` adalah **tafsiran agent** dari perilaku rawat jalan as-is (`FIN-FACT-09`). Ada juga beda kata: PRD v`0.1` `BP-RWF-01` menulis "hasil lab/radiologi diterima", sedangkan source menagih lab saat **spesimen** diterima. Pilihan ini mengubah kapan dan berapa pasien ditagih. **`DEC-INP-014`** |
| G-04 | `S25` | Pemetaan kategori tarif Billing ke tujuh kelompok V1. Contoh: tagihan Gizi, Bank Darah, dan Hemodialisa masuk "Penunjang Medis"? Material operasi masuk "Operasi" atau "Obat & Alkes"? | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Usulan bawaan: kelompok diturunkan dari penanda kategori tarif (`MstTariffCategory.IsRoomCharge`, `IsProcedure`, `IsLaboratory`, `IsRadiology`, `IsPharmacy`, `IsAdministrationFee`), ditambah sumber domain untuk Pemakaian Alat dan Operasi. Penunjang Medis memuat Lab, Radiologi, Gizi, Bank Darah, dan Hemodialisa. Pemetaan dapat dikonfigurasi tanpa mengubah tagihan |
| G-05 | `S26` | Perawat **hanya melihat status tanggungan, tanpa harga**, saat memilih pemeriksaan (PRD v`0.2` `FR-RWF-034`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tafsiran agent agar sejalan dengan `RWI-DEC-160` dan `RWI-DEC-170` yang sudah `CONFIRMED`. Pilihan bawaan ini aman karena lebih ketat. Bila pemilik ingin harga tampil bagi perawat, `RWI-DEC-160` harus diamendemen lebih dulu |
| G-06 | `S27` | **Komponen biaya operasi** yang masuk invoice saat kasus `Completed`: tarif tindakan operator, jasa anestesi, sewa kamar operasi, bahan atau implan, dan asisten. Juga dari mana tarifnya diambil | `MISSING` | **`BLOCKING`** — hanya biaya operasi (`FR-RWF-047`) | `RWI-DEC-191` butir (f) dan `RWI-DEC-192` menyetujui **adanya** kontrak OK → Billing, tetapi tidak menyebut isinya. Source: integrasi OK → Billing ditahan (`OperatingRoomIntegrationService.cs:28-40`); material OK dibukukan ke stok Farmasi. Pilihan ini menentukan baris tagihan dan risiko tertagih ganda dengan tindakan yang sudah lewat folio. **`DEC-INP-015`** |
| G-07 | `S27` | Catatan pra-operasi yang sudah dikirim dan dikonfirmasi, lalu kasus **Ditunda** dan dijadwalkan ulang | `MISSING` | **`BLOCKING`** — hanya lifecycle pra-operasi saat penundaan | Tanda vital dan nyeri dibekukan saat dikirim (`RWI-DEC-173` butir 5). Bila operasi bergeser sehari, potret itu basi. Pilihannya mengubah lifecycle dokumen (tetap berlaku, kedaluwarsa, atau wajib dikirim ulang) dan menyangkut keselamatan klinis. **`DEC-INP-016`** |
| G-08 | `S28` | Pasien dengan **lebih dari satu selang WSD**, misalnya kiri dan kanan: pembacaan dan "sisa shift lalu" dicatat per selang atau digabung | `MISSING` | **`BLOCKING`** — hanya Observasi WSD | Rumus `BP-RWF-06` mengandaikan satu tabung. Bila ada dua selang dan sistem hanya mengenal satu, jumlah bertambah salah dihitung. Pilihannya mengubah struktur data pembacaan dan makna klinis output. **`DEC-INP-017`** |
| G-09 | `S28` | Pembacaan WSD **pertama** (selang baru dipasang, belum ada sisa shift lalu) | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: pembacaan pertama memakai sisa awal 0 ml, atau sisa awal yang diisi perawat saat pemasangan. Tidak mengubah struktur bila G-08 sudah diputuskan |
| G-10 | `S28` | Batas waktu dokter memverifikasi diet, pesanan gizi, dan pesanan darah yang diinput perawat | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Keputusan lama tidak menetapkan batas (`RWI-DEC-114`; `RWI-RULE-021` untuk CPPT pun belum final). Usulan: batas jam dapat dikonfigurasi, dan pesanan yang melewatinya tampil "terlambat diverifikasi" pada daftar. Tidak mengubah lifecycle pesanan |
| G-11 | `S26`, `S31` | Kecukupan klinis pesanan darah yang diinput perawat, dan isi minimal serah terima transfer | `PROPOSED` | `NON_BLOCKING_STANDARD` untuk desain; **gerbang produksi** | Keputusan produknya `approved` (`RWI-DEC-171`, `RWI-DEC-182`). Konfirmasi pemilik clinical governance wajib sebelum produksi, sama seperti butir klinis lain pada Gate Sebelum Produksi |
| G-12 | `S29` | Tarif kelas yang berlaku bila pasien **pindah kelas** saat alat bersatuan waktu masih berjalan | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Usulan bawaan: mengikuti aturan "saat tarif dibaca" pada kebijakan tarif kamar (`TariffMoment`), sehingga perilaku alat dan kamar seragam. Dapat dikonfigurasi per jenis alat |
| G-13 | `S30` | Apakah tindakan khusus perawat ikut tampil di katalog Order Tindakan bangsal (saringan `IsDoctorAction`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Inferensi dari `FIN-CAP-32`. Usulan: katalog bangsal memakai penanda rawat inap, dan saringan `IsDoctorAction` ditinjau saat desain. Tidak mengubah model data |

**Pertentangan (`CONFLICT`).** Tidak ada pertentangan bisnis yang terbuka. `RWI-CON-012` s.d. `RWI-CON-015` sudah tertutup. Beda kata pada G-03 dicatat sebagai bagian `DEC-INP-014`, bukan konflik tersendiri, karena sumber yang lebih baru (PRD v`0.2`) menyatakannya sebagai tafsiran yang menunggu konfirmasi.

### 17.6 Decision Log

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status | Dampak implementasi / domain |
|---|---|---|---|---|---|---|---|---|
| **`DEC-INP-014`** | Kapan layanan klinis rawat inap ditagih? Khususnya: (a) obat saat **diserahkan farmasi** atau saat **diberikan menurut MAR**; (b) bagaimana obat yang diserahkan tetapi tidak diberikan dibatalkan tagihannya (retur); (c) apakah lab tetap saat spesimen diterima dan radiologi saat kualitas citra diputuskan, seperti rawat jalan | `INP-S24` — jembatan layanan klinis `RANAP` (`FR-RWF-011`); berimbas ke `INP-S25` kelompok Obat & Alkes | `RWI-DEC-192` menyetujui butir (b) `RWI-OQ-103` secara umum tanpa memilih. Tafsiran agent di PRD v`0.2`: sama dengan rawat jalan. Source: `FIN-FACT-09` | Sama dengan rawat jalan untuk lab dan radiologi. Untuk obat: saat diserahkan, dengan retur yang membatalkan baris tagihan milik obat itu sendiri | Konsekuensi billing, rekonsiliasi farmasi, dan kepercayaan tagihan pasien | Yasmina (Billing), diteruskan Muhammad Hamzah | `OPEN` | Jembatan klinis `RANAP` **berhenti**. Invoice otomatis, tarif kamar, biaya admin, outbox, koreksi, dan putar ulang **boleh jalan** |
| **`DEC-INP-015`** | Apa saja komponen biaya operasi yang dikirim ke invoice saat kasus `Completed`, dari mana tarifnya, dan bagaimana mencegah tagihan ganda dengan tindakan operasi yang juga tercatat sebagai tindakan pasien? | `INP-S27` — biaya operasi (`FR-RWF-047`) | `RWI-DEC-191` butir (f) dan `RWI-DEC-192` menyetujui adanya kontrak. Isi kontrak tidak ada. OK → Billing ditahan di source | Satu baris tindakan operasi dari order tindakan yang dirujuk kasus (tarif master tindakan), ditambah komponen yang berasal dari OK (anestesi, kamar operasi, bahan atau implan) masing-masing satu sumber | Konsekuensi billing dan integritas data (risiko dobel) | Ikbal Yulianto (OK) bersama Yasmina (Billing) | `OPEN` | Biaya operasi **berhenti**. Pemesanan, status, pra-operasi, penandaan, dan serah terima **boleh jalan** |
| **`DEC-INP-016`** | Bila kasus operasi **Ditunda** setelah catatan pra-operasi dikirim dan dikonfirmasi, apakah catatan itu tetap berlaku, kedaluwarsa setelah batas waktu tertentu, atau wajib dikirim ulang dan dikonfirmasi ulang? | `INP-S27` — lifecycle pra-operasi (`FR-RWF-045`) | `RWI-DEC-173` butir 5 membekukan potret tanda vital saat dikirim. Penundaan tidak dibahas | Penundaan membuat catatan pra-operasi wajib dikirim ulang dan dikonfirmasi ulang sebelum kasus kembali "Siap" | Lifecycle dokumen, keselamatan klinis | Muhammad Hamzah bersama Ikbal Yulianto (OK); konfirmasi klinis sebelum produksi | `OPEN` | Hanya perilaku pra-operasi saat penundaan yang **berhenti**. Jalur normal pra-operasi **boleh jalan** |
| **`DEC-INP-017`** | Bila pasien terpasang lebih dari satu selang WSD, apakah observasi dicatat **per selang** (dengan penanda lokasi, misalnya kanan/kiri) atau digabung? | `INP-S28` — Observasi WSD (`FR-RWF-054`) | `BP-RWF-06` dan `RWI-DEC-172` mengandaikan satu tabung. V1 tidak dibaca ulang untuk butir ini | Per selang, dengan penanda lokasi. Satu selang tetap menjadi kasus paling umum | Struktur data, makna klinis output, ketepatan balance cairan | Muhammad Hamzah (pemilik produk, dengan konfirmasi klinis) | `OPEN` | Observasi WSD **berhenti**. Sub-menu lain di Catatan Keperawatan **boleh jalan** |

Keempatnya bergantung pada pemilik, sehingga **tidak dijawab di sini**. Penutupannya lewat `grill-me`, didaftarkan sebagai `RWI-OQ-104` s.d. `RWI-OQ-107`.

### 17.7 Contoh konkret untuk setiap blocker

**`DEC-INP-014` — obat yang tidak jadi diberikan.** Farmasi menyerahkan ceftriaxone 3 vial untuk Tn. Budi pada 2 Okt. Dosis ketiga batal karena Budi dipulangkan. Bila obat ditagih saat **diserahkan**, invoice memuat 3 vial, dan baris itu harus dikurangi lewat retur 1 vial. Bila ditagih saat **diberikan** menurut MAR, invoice hanya memuat 2 vial, dan tidak perlu retur. Kedua pilihan menghasilkan tagihan berbeda pada hari pulang, dan alur retur farmasi berbeda.

**`DEC-INP-015` — operasi laparotomi.** Order tindakan "Laparotomi" milik dr. Rina sudah punya tarif. Bila tindakan itu juga lewat folio saat ditandai selesai, **dan** OK mengirim "biaya operasi" saat kasus `Completed`, invoice bisa memuat tindakan yang sama dua kali. Yang perlu ditetapkan: siapa mengirim apa, misalnya tindakan lewat folio, sedangkan OK hanya mengirim anestesi dan kamar operasi.

**`DEC-INP-016` — operasi ditunda sehari.** Catatan pra-operasi Budi dikirim 1 Okt 08.30 dengan TD 120/80, lalu operasi ditunda ke 2 Okt karena ruang penuh. Pada 2 Okt TD Budi 160/100. Bila catatan lama tetap berlaku, OK membaca TD yang sudah tidak benar.

**`DEC-INP-017` — dua selang.** Ny. Ani terpasang WSD kanan (sisa lalu 200 ml) dan kiri (sisa lalu 50 ml). Pukul 14.00 sisa kanan 350 ml dan kiri 80 ml. Per selang, bertambahnya 150 ml dan 30 ml. Bila digabung tanpa penanda, sistem tidak bisa membedakan bahwa selang kanan yang aktif mengeluarkan cairan, padahal itu informasi klinis yang dibaca dokter.

### 17.8 Kesiapan per slice

> **Catatan revision `1.9`.** Kesiapan `INP-S24`, `INP-S27`, dan `INP-S28` pada tabel ini, serta `open_decisions` pada 17.10, digantikan bagian 18.10 dan 18.12. `DEC-INP-014` s.d. `DEC-INP-017` sudah `CLOSED`.

| Slice | Kesiapan | Yang boleh jalan | Yang berhenti | Decision ID |
|---|---|---|---|---|
| `INP-S23` Gerbang penutupan dan izin kasir | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| `INP-S24` Tagihan inti | **`PARTIALLY_READY`** | Invoice `RANAP` otomatis, penerima event, outbox jujur, tarif kamar, biaya admin, finalisasi dengan "tarif belum ada", pensiun hitungan kedua, label seragam, koreksi penempatan, putar ulang | Jembatan layanan klinis `RANAP` | `DEC-INP-014` |
| `INP-S25` Tagihan Pasien bangsal | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice. Isi kelompok Obat & Alkes mengikuti `DEC-INP-014` saat datanya ada, tetapi bentuk layar dan kontraknya tidak bergantung padanya | — | — |
| `INP-S26` Penunjang dari bangsal | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| `INP-S27` Pasien operasi | **`PARTIALLY_READY`** | Pemesanan dari bangsal termasuk Obgyn, status di bangsal, pra-operasi jalur normal, penandaan, syarat consent, bed selama operasi, serah terima pasca operasi | Biaya operasi; perilaku pra-operasi saat kasus ditunda | `DEC-INP-015`, `DEC-INP-016` |
| `INP-S28` Catatan Keperawatan | **`PARTIALLY_READY`** | Susunan enam sub-menu, Spooling Cairan, Sliding Scale, Daftar Pemberian Obat, Efek Samping, Catatan Pra-Operasi (jendela), Diet Medis, Obat & Alkes empat sub-tab, narasi CPPT | Observasi WSD | `DEC-INP-017` |
| `INP-S29` Pemakaian Alat | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| `INP-S30` Katalog tindakan | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| `INP-S31` Serah terima transfer (`P2`) | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| **Finishing secara keseluruhan** | **`PARTIALLY_READY`** | Enam slice penuh dan tiga slice sebagian | Empat bagian kecil | `DEC-INP-014` s.d. `DEC-INP-017` |

**Dependency antar-slice:** `INP-S25` membaca hasil hitungan invoice dari `INP-S24`. `INP-S29` mengirim tagihan lewat jalur yang sama dengan tindakan, sehingga ikut menunggu jalur tagihan `INP-S24`, tetapi desainnya tidak terblokir. `INP-S28` jendela Catatan Pra-Operasi memakai fase pra-operasi `INP-S27`. `UAT-RWF-01` baru bisa lulus setelah keempat Decision ID tertutup dan terimplementasi.

**Status modul Rawat Inap** tetap **`PARTIALLY_READY`**: selain empat Decision ID ini, `INP-S09` (serah terima IGD) masih menunggu pemilik IGD, dan transfusi serta handover shift tetap `DEFERRED`.

### 17.9 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

1. `design-business-module` untuk amandemen keempat sub-modul pada bagian yang siap (17.8). Amandemen `integrasi-billing` wajib mencabut webhook, salinan status izin di episode, `confirm-physical-discharge` sebagai jalur kedua, dan supervisor override pulang fisik dengan PIN, karena semuanya digantikan `RWI-DEC-166`, `RWI-DEC-167`, `RWI-DEC-186`, dan `RWI-DEC-187`.
2. `grill-me` lanjutan yang singkat untuk `DEC-INP-014` s.d. `DEC-INP-017`, paralel dengan desain bagian yang siap.
3. `manage-module-blueprint` atau `plan-module-delivery` untuk menurunkan status `BE-RWI-129`, `BE-RWI-130`, dan `BE-RWI-133` ke `PARTIAL` (`RWI-DEC-168`). Butir ini administratif dan tidak bergantung gate.

**Harus berhenti:**

1. Desain dan task untuk jembatan layanan klinis `RANAP`, biaya operasi, perilaku pra-operasi saat penundaan, dan Observasi WSD, sampai Decision ID masing-masing tertutup.
2. Implementasi apa pun sebelum blueprint amandemen disetujui dan task diturunkan. Wewenang tulis, migration, dan putar ulang tetap terpisah per task.
3. Memakai bagian 16.8 butir 4 dan batas auto-reblock 16.9 sebagai aturan, karena keduanya sudah digantikan `RWI-DEC-186`.

### 17.10 Handoff

```yaml
gate_revision: 1.8
blueprint_id: RWI-BP-001
assessed_scope: PRD-RWI-FINISHING-001 v0.2 — CAP-RWF-01..16 (CAP-RWF-17 dihapus RWI-DEC-183)
slices: INP-S23..INP-S31
evidence:
  decision_log: 00-interview-decisions.md revision 29, sha256 f6fed60809321687d850306dfd2830d9394e1b2e7e7ab0a66b01cbc5068ce471
  prd: 05-prd-to-mvp-finishing-rawat-inap.md v0.2, sha256 01f4479de05a6e2faecb9db535fd965f0392f42519a525b944afd102e5d5f6ca
  capability_map: 01-existing-capability-map.md revision 1.6 bagian 19, sha256 2f78b74e37c983ffa81cbd510e129906243634536c04df105811c827930eccd2
  backend_sha: c8e99ce5 (audit); HEAD 425cfeae docs-only
  frontend_sha: 22ad67330 (audit); HEAD ee75e055b styling-only
  baseline_reference: indonesia-hospital-domain-reference inpatient.md — ID-INP-CAP-016, bagian 9, bagian 10 (REFERENCE_ONLY)
readiness:
  READY_FOR_DOMAIN_DESIGN: [INP-S23, INP-S25, INP-S26, INP-S29, INP-S30, INP-S31]
  PARTIALLY_READY:
    INP-S24: {stop: "jembatan layanan klinis RANAP", decision: DEC-INP-014}
    INP-S27: {stop: ["biaya operasi", "pra-operasi saat kasus ditunda"], decision: [DEC-INP-015, DEC-INP-016]}
    INP-S28: {stop: "Observasi WSD", decision: DEC-INP-017}
open_decisions:
  - DEC-INP-014: titik tagih layanan klinis rawat inap dan retur obat — owner Yasmina
  - DEC-INP-015: komponen biaya operasi — owner Ikbal Yulianto + Yasmina
  - DEC-INP-016: pra-operasi saat kasus ditunda — owner Muhammad Hamzah + Ikbal Yulianto
  - DEC-INP-017: WSD lebih dari satu selang — owner Muhammad Hamzah
non_blocking: [G-01, G-02, G-04, G-05, G-09, G-10, G-11, G-12, G-13]
production_gate: [G-11 clinical governance, RWI-RULE-037 meninggal/kabur, FIN-UNK-05 role-permission, FIN-UNK-06 lingkungan uji]
domain_architecture: opsional; tidak disarankan — kepemilikan data lintas modul sudah ditetapkan RWI-DEC-164, 167, 173, 180, 182, 188, 193
next_skill:
  - grill-me (DEC-INP-014..017 sebagai RWI-OQ-104..107), paralel dengan
  - design-business-module untuk slice dan bagian yang siap
superseded_in_this_document: bagian 16.8 butir 4 dan batas auto-reblock 16.9 (oleh RWI-DEC-186)
```

---

## 18. Evaluasi Ulang Gerbang Kelengkapan Requirement — Finishing Rawat Inap `PRD-RWI-FINISHING-001` v`0.4` — revision `1.9`

### 18.1 Scope, identitas slice, dan bukti acuan

**Yang dinilai ulang.** Gate ini melanjutkan bagian 17, bukan mengulangnya dari awal.

1. **Tiga slice yang tadinya `PARTIALLY_READY`:** `INP-S24`, `INP-S27`, dan `INP-S28`. Keempat Decision ID penahannya (`DEC-INP-014` s.d. `DEC-INP-017`) sudah dijawab pemilik pada Amendment Pass penutupan gate `1.8`.
2. **Klaster Pasca Operasi** `CAP-RWF-18` s.d. `CAP-RWF-23`, yang di PRD v`0.3` masih usulan dan di v`0.4` sudah diputuskan (`RWI-DEC-201` s.d. `RWI-DEC-205`). Klaster ini dipecah menjadi enam slice baru `INP-S32` s.d. `INP-S37`. Seperti bagian 17, pengelompokannya mengikuti kepemilikan data dan siklus hidup, bukan menu.

Enam slice lain di bagian 17 (`INP-S23`, `S25`, `S26`, `S29`, `S30`, `S31`) **tidak dinilai ulang**, karena tidak ada keputusan baru yang mengubahnya. Kesiapannya tetap seperti 17.8.

| Slice | Nama | Kemampuan PRD | Sub-modul pemilik (PRD 5.5) |
|---|---|---|---|
| `INP-S24` | Tagihan inti rawat inap (dinilai ulang) | `CAP-RWF-02`, `CAP-RWF-04` | `integrasi-billing` |
| `INP-S27` | Pasien operasi dari bangsal (dinilai ulang) | `CAP-RWF-07`, `CAP-RWF-08` | `episode-rawat-inap` |
| `INP-S28` | Catatan Keperawatan susunan V1, WSD, Efek Samping Obat, Diet Medis (dinilai ulang) | `CAP-RWF-09` s.d. `CAP-RWF-12` | `keperawatan` |
| `INP-S32` | Admisi rawat inap dari kamar pulih | `CAP-RWF-18` | `episode-rawat-inap` |
| `INP-S33` | Ringkasan operasi di bangsal dan daftar pantau serah terima pasca operasi | `CAP-RWF-19` (`FR-RWF-081`, `082`, `088`) | `episode-rawat-inap` |
| `INP-S34` | Surveilans infeksi luka operasi | `CAP-RWF-20` | `keperawatan` |
| `INP-S35` | Monitoring transfusi darah | `CAP-RWF-21` | `keperawatan` |
| `INP-S36` | Penolakan order operasi oleh OK | `CAP-RWF-22` | `episode-rawat-inap` |
| `INP-S37` | Laporan transfer ruangan (`P2`) | `CAP-RWF-23` | `episode-rawat-inap` |

**Bukti yang dipakai:**

| Jenis bukti | Sumber | Revision / hash |
|---|---|---|
| Requirement eksplisit pemilik | `docs/module-blueprints/rawat-inap/00-interview-decisions.md`, `RWI-DEC-194` s.d. `RWI-DEC-205`, `RWI-OQ-104` s.d. `RWI-OQ-115`, `RWI-AC-307` s.d. `RWI-AC-329` | Revision `30`, SHA-256 `aa92c5dd…ed216e2a439` |
| Dokumen produk | `docs/Modul-RS/Rawat-Inap/05-prd-to-mvp-finishing-rawat-inap.md` (`PRD-RWI-FINISHING-001` v`0.4`) | SHA-256 `aee2afdb…d1ae46d29` saat dinilai. Setelah gate, hanya baris Status, Jalur pengesahan, dan 11.2 butir 10 yang diperbarui untuk mencatat hasil gate ini: SHA-256 `0f658455…27cfabe1a` |
| Bukti praktik sistem lain | `docs/Modul-RS/Rawat-Inap/Pasca-Operasi-ke-Rawat-Inap.md`, rekaman layar HiSys tanpa audio (`RWI-FACT-056`) | SHA-256 `ce43e811…96fc853c7`, `untracked` |
| Implementasi V2 terverifikasi | `01-existing-capability-map.md` revision `1.6` bagian 19; `RWI-FACT-057` dan `RWI-FACT-058` (backend `c8e99ce5`); pembacaan source gate ini, dicatat di bawah | SHA-256 capability map `2f78b74e…30eccd2` |
| HEAD saat gate dijalankan | Backend `425cfeae` (sama dengan gate `1.8`). Frontend `ee75e055b` (sama), dengan perubahan lokal belum di-commit pada dashboard, grafik tanda vital, asesmen medis, styling, dan label menu "Beranda Rawat Inap" → "Dashboard". Tidak satu pun menyentuh berkas pemicu capability map 19.9 | Bagian 19 tetap `CURRENT` |
| Baseline rujukan | `indonesia-hospital-domain-reference` `references/inpatient.md`: `ID-INP-CAP-001` (konteks permintaan admisi), `ID-INP-CAP-015` (transfer internal). Berkas Operating Theatre, Blood Bank / Transfusion, dan Infection Prevention & Control masih `PLANNED` | `REFERENCE_ONLY`; `Reference coverage: PARTIAL` untuk rawat inap, **tidak tersedia** untuk OK, transfusi, dan PPI |

**Pembacaan source tambahan pada gate ini** (backend `425cfeae`, sama dengan `c8e99ce5` untuk berkas berikut):

| Fakta | Bukti | Dipakai untuk |
|---|---|---|
| Kasus OK `Postponed` hanya punya tindakan `Reschedule` (kembali ke `Scheduled`). Penundaan hanya dari `Requested` atau `Scheduled`; kasus `Ready` hanya dapat `Start` atau `Cancel` | `OperatingRoomManagement/Services/OperatingRoomCommandSupport.cs:77-88`; `OperatingRoomSchedulingService.cs:199-201` | `INP-S27`, gap G-14 |
| Penempatan bed menyimpan `TransferReason`, `ChangeReason`, `IsSuperseded`, `EndReason`, `PlacedByUserId`, dan `EndedByUserId` | `InPatientManagement/Models/InpBedPlacement.cs:39-58` | `INP-S37`: alasan dan pencatat tersedia tanpa tabel baru |
| Kantong darah menyimpan `IssuedToPatientId`, `IssuedAt`, `IssuedByUserId`, dan `IssuedViaEmergency` | `BloodBankManagement/Models/BbkBloodUnit.cs:93-114` | `INP-S35`: kantong yang sudah diserahkan per pasien tersedia; cakupannya per pasien, bukan per episode (G-24) |

**Wewenang bukti yang diterapkan.** Untuk apa yang **seharusnya dibangun**, decision log menang atas PRD (`RWI-DEC-165`), dan PRD v`0.4` menyalinnya. Bukti HiSys hanya bukti praktik sistem lain: aturan yang berlaku adalah keputusan pemilik, bukan isi layar HiSys. Untuk apa yang **sudah ada**, capability map bagian 19 dan pembacaan source di atas yang dipakai.

**Persetujuan pemilik modul lain diperlakukan sebagai gerbang implementasi**, mengikuti preseden decision log ("Gerbang implementasi: tidak menghalangi penyusunan desain, tetapi menghalangi penulisan source code"). Keputusannya sudah dibuat dan `approved` untuk sisi Rawat Inap dan Clinical. Yang menunggu hanyalah persetujuan pemilik modul yang berubah. Bila pemilik itu menjawab lain, keputusan terkait dibuka ulang lewat `grill-me`, lalu desain bagian itu diamandemen. Ini berbeda dari **keputusan yang belum dibuat sama sekali**, yang tetap dinilai `BLOCKING` bila memenuhi kriteria kontrak (lihat `DEC-INP-018`).

### 18.2 Ringkasan untuk pembaca umum

Keempat pertanyaan yang menahan gate sebelumnya sudah dijawab:

| Pertanyaan gate `1.8` | Jawaban pemilik |
|---|---|
| Kapan obat rawat inap ditagih (`DEC-INP-014`) | Saat diserahkan farmasi; retur yang lolos pemeriksaan membatalkan tagihannya (`RWI-DEC-195`) |
| Apa isi biaya operasi (`DEC-INP-015`) | Tindakan lewat order tindakan yang ditandai selesai oleh OK; OK mengirim anestesi, sewa kamar operasi, serta bahan dan implan, tanpa dobel (`RWI-DEC-196`) |
| Pra-operasi saat operasi ditunda (`DEC-INP-016`) | Wajib dikirim ulang dengan tanda vital terbaru dan dikonfirmasi ulang (`RWI-DEC-199`) |
| Pasien dengan dua selang WSD (`DEC-INP-017`) | Dicatat per selang dengan lokasi (`RWI-DEC-200`) |

Karena itu **`INP-S24`, `INP-S27`, dan `INP-S28` naik menjadi siap penuh** untuk dirancang.

Untuk klaster Pasca Operasi, **lima dari enam slice siap penuh**. Satu slice, admisi dari kamar pulih (`INP-S32`), siap untuk seluruh sisi Rawat Inap dan OK. Yang tertahan hanya satu aturan milik Billing: bila pasien dioperasi dari poliklinik atau ODC lalu dirawat inap, **biaya operasinya masuk invoice rawat inap atau tetap di invoice poliklinik** (`DEC-INP-018`, sama dengan `RWI-OQ-114` butir b). Pertanyaan ini mengubah tagihan yang diterima pasien atau penjamin, sehingga tidak boleh ditebak perancang.

Tiga persetujuan pemilik modul lain masih ditunggu, tetapi **hanya menahan implementasi**: pemilik Farmasi untuk retur obat (`RWI-OQ-108`), Ikbal Yulianto untuk permintaan admisi dan status Ditolak (`RWI-OQ-114` butir a dan c), serta Sukma Giri Pratama untuk data kantong dan pemberitahuan reaksi transfusi (`RWI-OQ-115`).

### 18.3 Penutupan Decision ID gate `1.8`

| Decision ID | Ditutup oleh | Isi jawaban | Gap yang tertutup | Sisa yang dibawa |
|---|---|---|---|---|
| `DEC-INP-014` | `RWI-DEC-195` | Obat saat diserahkan; MAR bukan sumber tagihan; retur yang lolos pemeriksaan membatalkan sebanyak jumlah yang kembali; lab saat spesimen diterima dan radiologi saat kualitas citra diputuskan, sama dengan rawat jalan; retur setelah invoice final lewat adjustment Billing | G-03 | Gerbang implementasi `RWI-OQ-108` (perubahan `DrugReturnService` di Farmasi) |
| `DEC-INP-015` | `RWI-DEC-196` | Dua sumber yang tidak tumpang tindih; satu baris per komponen; tarif dari master tarif; kasus batal tanpa biaya; "tarif belum ada" menahan finalisasi; tarif bed tetap berjalan | G-06 | — (disetujui OK dan Billing lewat Muhammad Hamzah) |
| `DEC-INP-016` | `RWI-DEC-199` | Penundaan membuat pra-operasi "perlu diperbarui"; versi baru dengan tanda vital dirujuk ulang; konfirmasi ulang dua akun; gerbang "Siap" hanya membaca versi terbaru; tanpa batas jam | G-07 | Konfirmasi clinical governance sebelum produksi |
| `DEC-INP-017` | `RWI-DEC-200` | Per selang dengan lokasi dan waktu pasang; rumus per selang; pembacaan pertama dari sisa awal atau 0 ml; selang dilepas menutup pencatatan; data `ClinicalManagement` | G-08, G-09 | G-15 (pelaku pendaftaran selang), non-blocking |

Keempatnya kini **`CLOSED`**.

### 18.4 Matriks 18 dimensi kelengkapan

Kode isi sel sama dengan 17.3: **C** = `CONFIRMED`; **P** = `PROPOSED`; **M** = `MISSING`; **X** = `CONFLICT`; **–** = tidak material, dengan alasan di 18.5. Angka dalam kurung merujuk gap pada 18.6 (G-14 dan seterusnya) atau 17.5 (G-01 s.d. G-13).

| No | Dimensi | `S24` | `S27` | `S28` | `S32` | `S33` | `S34` | `S35` | `S36` | `S37` |
|---:|---|---|---|---|---|---|---|---|---|---|
| 01 | Tujuan | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | C | C | C | C | C |
| 03 | Pemicu / prasyarat | C | C | C | C | C | C | C | C | C |
| 04 | Alur utama | C | C | C | C | C | C | C | C | C |
| 05 | Alur alternatif / exception | C | C, M (G-14) | C | C | C | C | C, P (G-22) | C | C |
| 06 | Data minimum | C | C | C | C | C | C, P (G-21) | C, P (G-22, G-24) | C | C |
| 07 | Aturan bisnis / validation | C | C | C, P (G-10) | C | C, P (G-19) | C, P (G-20) | C | C | C |
| 08 | Status / perubahan status | C | C, M (G-14) | C | C, P (G-16) | – | C, P (G-20) | P (G-22) | C | – |
| 09 | Peran / authorization | C | C | C, M (G-15) | C, P (G-16) | P (G-19) | C, P (G-20) | C | C, P (G-25) | C |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | C | C |
| 11 | Integrasi internal / eksternal | C | C | C | C | C | C | C | C | – |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | C |
| 13 | Pembatalan / koreksi | C | C | C | C | – | C | C | C | – |
| 14 | Audit / histori | C | C | C | C | – | C | C | C | C |
| 15 | Notifikasi | – | P (G-02) | – | P (G-02, G-18) | P (G-18) | C | C, P (G-23) | P (G-02) | – |
| 16 | Dampak billing / charge | C | C | – | C, M (G-17) | – | – | – | C | – |
| 17 | Dampak keselamatan klinis | – | C | C | – | C, P (G-19) | C, P (G-21) | C, P (G-21, G-23) | – | – |
| 18 | Pelaporan / traceability | C | C | C | C | C | C | C | C | C |

Prioritas `P1` untuk `CAP-RWF-18`, `19`, dan `22` (G-26) tidak masuk matriks, karena menyangkut batas MVP, bukan kelengkapan requirement slice.

### 18.5 Temuan per slice

#### `INP-S24` — Tagihan inti rawat inap (dinilai ulang)

Bagian yang di gate `1.8` berhenti, yaitu jembatan layanan klinis `RANAP`, kini `CONFIRMED`: titik tagih per jenis layanan, MAR yang bukan sumber tagihan, retur yang membatalkan tagihan secara terlacak tanpa menghapus baris asli, retur bahan OK, dan koreksi setelah invoice final lewat adjustment (`RWI-DEC-195` butir 1 s.d. 5, `AC-RWF-090`, `091`, `012`). Dimensi 13 (pembatalan) kini lengkap karena retur sudah punya aturan. Dimensi 15 dan 17 tetap tidak material, dengan alasan yang sama seperti 17.4. **Gap baru: tidak ada.** Perubahan `DrugReturnService` di Farmasi adalah **gerbang implementasi** `RWI-OQ-108`, bukan gap desain.

#### `INP-S27` — Pasien operasi dari bangsal (dinilai ulang)

Biaya operasi (`RWI-DEC-196`) dan pra-operasi setelah penundaan (`RWI-DEC-199`, `FR-RWF-090`) kini `CONFIRMED`. Source mendukung jalur penjadwalan ulang: kasus `Postponed` hanya punya tindakan `Reschedule`, sehingga titik "dijadwalkan ulang" pada `RWI-DEC-199` butir 2 punya kejadian yang jelas. Daftar komponen biaya operasi tertutup: pembagian jasa medis dan asisten berada di modul jasa medis (`RWI-DEC-197`), bukan baris OK. Status Ditolak pada daftar bangsal (`FR-RWF-044`) ikut slice ini sebagai tampilan, sedangkan aturan penolakannya dinilai di `INP-S36`. **Gap baru:** G-14, non-blocking.

#### `INP-S28` — Catatan Keperawatan (dinilai ulang)

Observasi WSD kini `CONFIRMED` per selang (`RWI-DEC-200`, `FR-RWF-054`, `FR-RWF-058`, `AC-RWF-057`, `058`). Satu angka total harian tetap terjaga, karena setiap jumlah bertambah masuk balance cairan dengan rujukan selang. Catatan drain di laporan operasi hanya petunjuk, sehingga tidak ada tulisan otomatis lintas modul. **Gap baru:** G-15, non-blocking. G-10 tetap `CONFIGURABLE_DEFAULT`.

#### `INP-S32` — Admisi rawat inap dari kamar pulih

Pemicu, daftar permintaan beserta kunjungan asal, kasus OK, dan dokter operator, larangan admisi otomatis, admisi berlangkah dengan aturan jenis kelamin dan isolasi, rujukan kunjungan asal seperti alih IGD, penolakan bila sudah ada episode aktif, pembatalan oleh OK dengan alasan, daftar pantau lama menunggu, urutan admisi → bed → serah terima → `Completed`, dan tarif kamar sejak bed ditempati: semuanya `CONFIRMED` (`RWI-DEC-201`, `RWI-RULE-029`, `RWI-RULE-035`, `RWI-DEC-156`, `RWI-DEC-177`, `RWI-DEC-189`). Source menegaskan kebutuhannya: keputusan kamar pulih `Inpatient` hari ini tidak membuat admisi apa pun (`RWI-FACT-057` butir 5).

Dibandingkan baseline `ID-INP-CAP-001` (`REFERENCE_ONLY`): asal admisi, keputusan merawat, tenaga profesional bertanggung jawab, kegawatan (rawat inap atau ICU), penjamin saat admisi, dan pembatalan sebelum admisi sudah tercakup. Diagnosis atau alasan tersedia lewat rujukan kasus OK (diagnosis pasca bedah), tanpa salinan. Dimensi 17 tidak material, karena keputusan klinis rawat inap tetap dibuat kamar pulih dan dokter, sedangkan slice ini mengatur jalur administrasinya.

**Gap:** G-16 dan G-18 non-blocking; **G-17 memblokir** satu aturan Billing (`DEC-INP-018`). Pengiriman dan pembatalan permintaan oleh OK menunggu **gerbang implementasi** `RWI-OQ-114` butir (a).

#### `INP-S33` — Ringkasan operasi dan daftar pantau serah terima

Kemampuannya `CONFIRMED` oleh `RWI-DEC-197`. Datanya sudah ada dan dapat dibaca lewat endpoint OK yang tersedia: laporan operasi, catatan anestesi, dan kamar pulih (`RWI-FACT-057` butir 1 dan 2). Aturan bacanya, yaitu siapa yang boleh membaca dan larangan menampilkan laporan draft, adalah usulan standar (`FR-RWF-081`, `082`) yang dikonfirmasi saat desain (`RWI-DEC-194`). Dimensi 08, 13, dan 14 tidak material, karena slice ini hanya membaca data milik OK, dan OK sudah mengatur versi serta audit laporannya. Dimensi 16 tidak material. **Gap:** G-18 dan G-19, non-blocking.

#### `INP-S34` — Surveilans infeksi luka operasi

Kepemilikan `ClinicalManagement`, satu formulir berversi per kasus, lahir saat kasus `Completed`, isi minimal, hari ke-1 dari tanggal operasi selesai, suhu dibaca dari tanda vital, pengisi perawat dari akun login, peninjau PPI dengan permission tersendiri, berhenti otomatis saat pasien keluar ruangan, dan koreksi berversi: `CONFIRMED` (`RWI-DEC-202`). Dimensi 15 `CONFIRMED` dalam bentuk daftar PPI. Dimensi 16 tidak material. Dimensi 18: daftar PPI `CONFIRMED`; rekap angka infeksi luka operasi tidak diminta dan tidak dijadikan requirement. Tidak ada baseline rujukan PPI yang tersedia, dan ketiadaannya tidak dipakai sebagai bukti apa pun. **Gap:** G-20 non-blocking; G-21 non-blocking untuk desain tetapi **gerbang produksi**.

#### `INP-S35` — Monitoring transfusi darah

Pembukaan dari `DEFERRED` hanya untuk monitoring, satu catatan per kantong yang sudah diserahkan, kantong dirujuk dan tidak diketik, empat titik ukur, reaksi menjadi pemberitahuan di Bank Darah, titik terlewat ditandai terlambat, larangan isi mundur tanpa keterangan, volume tetap di Pengawasan Harian, koreksi berversi, dan daftar bagian yang tetap `DEFERRED`: `CONFIRMED` (`RWI-DEC-203`). Source mendukung pemilihan kantong: `BbkBloodUnit` menyimpan penerima, waktu, dan petugas penyerahan. Dimensi 16 tidak material, karena penagihan darah mengikuti penyerahan oleh Bank Darah, bukan monitoring. **Gap:** G-22, G-23, dan G-24 non-blocking; G-21 gerbang produksi. Pembacaan data kantong dan penerimaan pemberitahuan di Bank Darah menunggu **gerbang implementasi** `RWI-OQ-115`.

**Catatan keselamatan yang disadari pemilik.** Verifikasi dua petugas di samping tempat tidur tetap `DEFERRED` (`RWI-DEC-203` butir 7). Artinya, sampai bagian itu dibuka, sistem mencatat pemantauan tetapi tidak menegakkan identifikasi kantong-pasien di samping tempat tidur. Hal ini diteruskan ke pemilik clinical governance bersama G-21.

#### `INP-S36` — Penolakan order operasi

Status akhir `Rejected` hanya dari `Requested`, alasan wajib, penolak dari akun login, "menyetujui" berarti menjadwalkan tanpa status tersendiri, tampilan di bangsal, larangan menghidupkan kembali, pesan ulang sebagai kasus baru yang merujuk order sama, tanpa biaya, dan laporan OK yang memisahkan ditolak dari dibatalkan: `CONFIRMED` (`RWI-DEC-204`). Source: `Requested` hari ini punya tindakan `Update`, `Schedule`, `Postpone`, dan `Cancel`; `Reject` adalah tambahan pada lifecycle OK. Dimensi 17 tidak material, karena penolakan terjadi sebelum tindakan apa pun pada pasien. **Gap:** G-25 non-blocking. Status baru di OK menunggu **gerbang implementasi** `RWI-OQ-114` butir (c).

#### `INP-S37` — Laporan transfer ruangan (`P2`)

Sumber linimasa penempatan bed tanpa tabel baru, isi per baris, saringan, penanda koreksi, permission laporan, dan ekspor Excel teraudit: `CONFIRMED` (`RWI-DEC-205`). Source mendukung seluruh kolom: alasan transfer, alasan perubahan, penanda supersede, dan pencatat tersedia di `InpBedPlacement`. Dibandingkan baseline `ID-INP-CAP-015` (`REFERENCE_ONLY`), kepedulian "waktu perpindahan, perubahan kelas, dan tanggung jawab pengirim" tercakup lewat kolom laporan. Dimensi 08, 11, 13, 15, 16, dan 17 tidak material, karena laporan hanya membaca. **Gap baru: tidak ada.** Ada satu **dependency desain**: cara membedakan koreksi dari transfer bergantung pada desain penanda koreksi penempatan di `INP-S24` (catatan 17.4).

### 18.6 Daftar gap dan dampaknya

**Gap gate `1.8` yang tertutup:** G-03, G-06, G-07, G-08, dan G-09 (lihat 18.3). **Gap gate `1.8` yang tetap berlaku tanpa perubahan:** G-01, G-02, G-04, G-05, G-10, G-11, G-12, dan G-13.

**Gap baru:**

| Gap | Slice | Pernyataan | Status bukti | Dampak | Penjelasan dan usulan |
|---|---|---|---|---|---|
| G-14 | `S27` | Kasus OK berstatus `Ready` (Siap) **tidak dapat ditunda** di source; hanya dapat dimulai atau dibatalkan | `MISSING` (kehendak bisnis belum dinyatakan) | `NON_BLOCKING_STANDARD` | Usulan: ikuti source, sesuai PRD 7.3 yang hanya mengizinkan penundaan dari Diminta atau Terjadwal. Aturan pra-operasi `RWI-DEC-199` tidak bergantung pada status asal penundaan. Bila OK ingin menunda kasus yang sudah Siap, itu perubahan lifecycle OK milik Ikbal Yulianto, dan aturan `RWI-DEC-199` berlaku sama |
| G-15 | `S28` | Siapa yang mendaftarkan, mengoreksi, dan melepas selang WSD | `MISSING` | `NON_BLOCKING_STANDARD` | `RWI-DEC-200` menetapkan isi dan pemilik data, tetapi tidak menyebut pelakunya. Usulan: perawat bangsal dari akun login, dengan permission yang sama dengan pencatatan cairan. Koreksi label, lokasi, atau waktu pasang berversi dengan alasan, dan selang tidak pernah dihapus fisik |
| G-16 | `S32` | Kosakata status permintaan admisi (menunggu, selesai, dibatalkan, ditolak), permission OK untuk mengirim dan membatalkan, serta permission admisi untuk membaca daftar | `PROPOSED` | `NON_BLOCKING_STANDARD` | Perilakunya sudah diputuskan (`RWI-DEC-201` butir 1, 4, 6, 7); yang belum hanya nama. Usulan: nama status ditetapkan `design-business-module`; hak akses lewat permission, bukan nama peran (`PR-RWF-07`) |
| G-17 | `S32` | **Invoice tujuan biaya operasi** dari kunjungan poliklinik atau ODC bila pasien kemudian dirawat inap: digabung ke invoice `RANAP` seperti alih IGD (`BKC-DEC-117`), atau tetap di invoice kunjungan asal | `MISSING` | **`BLOCKING`** — hanya aturan Billing penggabungan itu | `RWI-DEC-201` menyatakan ini aturan Billing dan mencatatnya sebagai `RWI-OQ-114` butir (b) untuk Yasmina. Pilihannya menentukan berapa invoice yang diterima pasien atau penjamin dan ke mana biaya operasi dibebankan. Desain sisi Rawat Inap dan OK **tidak** bergantung padanya, karena episode cukup merujuk kunjungan asal (`RWI-DEC-201` butir 3). **`DEC-INP-018`** |
| G-18 | `S32`, `S33` | Batas jam tampil pada daftar pantau permintaan admisi dan serah terima pasca operasi yang tertunda | `PROPOSED` | `CONFIGURABLE_DEFAULT` | `FR-RWF-088` sudah menyebut "dapat diatur". Usulan: satu pengaturan per daftar pada Pengaturan Rawat Inap (`RWI-RULE-034`), tanpa angka tertanam di kode |
| G-19 | `S33` | Aturan baca ringkasan operasi: siapa yang boleh membaca, dan larangan menampilkan laporan draft | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan standar `FR-RWF-081`, `082`: pengguna yang berhak atas episode, lewat permission baca kasus OK; hanya laporan final yang tampil. Menampilkan hanya laporan final mengurangi risiko klinis membaca isi yang masih berubah. Pemetaan role ke permission tetap `FIN-UNK-05` |
| G-20 | `S34` | Aturan turunan surveilans: (a) indikator suhu bila sehari ada beberapa pencatatan; (b) status setelah hari ke-15; (c) cakupan "perawat yang merawat" | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: (a) indikator suhu hari ke-N bernilai "ya" bila minimal satu pencatatan pada hari kalender itu (zona `Asia/Jakarta`) ≥ 38 °C; (b) formulir berstatus selesai setelah hari ke-15; (c) perawat pemegang permission isi surveilans pada unit tempat pasien dirawat. Tidak mengubah kepemilikan maupun lifecycle utama |
| G-21 | `S34`, `S35` | Isi formulir surveilans dan titik ukur transfusi berasal dari layar HiSys, belum disahkan klinis, dan pemilik PPI belum tercatat | `PROPOSED` | `NON_BLOCKING_STANDARD` untuk desain; **gerbang produksi** | Keputusan produknya `approved` (`RWI-DEC-202`, `RWI-DEC-203`), dan keduanya sendiri menyebut pengesahan klinis sebagai syarat produksi. Template berversi membuat isi dapat disesuaikan tanpa mengubah struktur. Sama polanya dengan G-11 |
| G-22 | `S35` | Acuan waktu titik ukur dan status catatan monitoring: (a) waktu mulai transfusi; (b) status berjalan, selesai, atau dihentikan; (c) toleransi sebelum titik ukur dianggap terlambat | `PROPOSED` | (a), (b) `NON_BLOCKING_STANDARD`; (c) `CONFIGURABLE_DEFAULT` | Titik 15 menit, 1 jam, dan 4 jam diukur "setelah darah masuk", sehingga waktu mulai transfusi wajib dicatat; tanpa itu keterlambatan tidak dapat dihitung. Contoh pada `RWI-DEC-203` menyebut titik "ditandai berhenti bila transfusi dihentikan". Usulan: waktu mulai wajib; transfusi yang dihentikan dicatat dengan waktu dan alasan, dan titik sesudahnya bertanda "dihentikan"; toleransi dapat dikonfigurasi |
| G-23 | `S35` | Bentuk dan kecepatan pemberitahuan reaksi transfusi di Bank Darah | `PROPOSED` | `NON_BLOCKING_STANDARD` | Notifikasi seketika `DEFERRED` (PRD 5.4). Usulan: reaksi tampil sebagai butir pada daftar reaksi Bank Darah yang disegarkan berkala. Komunikasi darurat di luar sistem adalah prosedur klinis rumah sakit, bukan requirement sistem, dan diteruskan ke pemilik clinical governance |
| G-24 | `S35` | (a) Kantong diserahkan per **pasien**, bukan per episode; (b) letak penyimpanan nilai tanda vital pada titik ukur | `PROPOSED` | `NON_BLOCKING_STANDARD` | (a) Usulan: kantong yang dapat dipilih adalah kantong yang diserahkan kepada pasien itu sejak episode aktif dimulai dan belum punya catatan monitoring. (b) Usulan: nilai titik ukur disimpan sekali sebagai bagian catatan monitoring; tren tanda vital boleh membacanya tanpa salinan (`PR-RWF-05`). Keduanya keputusan desain di dalam `ClinicalManagement` |
| G-25 | `S36` | Permission untuk menolak kasus OK | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: permission tersendiri, dipisah dari permission batal, agar laporan dan audit membedakan keduanya (`RWI-DEC-204` butir 6). Tidak memakai nama peran |
| G-26 | `S32`, `S33`, `S36` | Prioritas `P1` untuk `CAP-RWF-18`, `19`, dan `22` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Keputusan pemilik tidak menyebut prioritas; PRD v`0.4` memakai usulan v`0.3` dan mencatatnya di bagian 11.2 butir 13. Prioritas memengaruhi batas MVP dan Definition of Done, bukan bentuk domain. Dikonfirmasi pemilik saat meninjau PRD |

**Pertentangan (`CONFLICT`).** Tidak ada pertentangan bisnis yang terbuka. Satu ketidaksesuaian **di dalam PRD v`0.3`** sudah dibetulkan di v`0.4` sebelum gate ini: contoh `BP-RWF-08` dan `UAT-RWF-16` v`0.3` menulis biaya operasi pasien dari poliklinik "masuk invoice `RANAP`", padahal `RWI-DEC-201` menyerahkan aturan itu ke Billing dan `RWI-AC-318` sengaja tidak menyebut invoice. Karena PRD v`0.4` sudah mengikuti decision log (`RWI-DEC-165`), butir ini tidak dicatat sebagai konflik.

### 18.7 Decision Log

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status | Dampak implementasi / domain |
|---|---|---|---|---|---|---|---|---|
| `DEC-INP-014` | Titik tagih layanan klinis rawat inap dan retur obat | `INP-S24` | `RWI-DEC-195` | — | — | Yasmina | **`CLOSED`** | Jembatan klinis `RANAP` boleh dirancang. Retur menunggu gerbang implementasi `RWI-OQ-108` |
| `DEC-INP-015` | Komponen biaya operasi | `INP-S27` | `RWI-DEC-196` | — | — | Ikbal Yulianto, Yasmina | **`CLOSED`** | Biaya operasi boleh dirancang |
| `DEC-INP-016` | Pra-operasi saat kasus ditunda | `INP-S27` | `RWI-DEC-199` | — | — | Muhammad Hamzah, Ikbal Yulianto | **`CLOSED`** | Lifecycle pra-operasi berversi boleh dirancang; konfirmasi klinis sebelum produksi |
| `DEC-INP-017` | WSD lebih dari satu selang | `INP-S28` | `RWI-DEC-200` | — | — | Muhammad Hamzah | **`CLOSED`** | Observasi WSD per selang boleh dirancang |
| **`DEC-INP-018`** | Bila pasien dioperasi pada kunjungan poliklinik atau ODC lalu diputuskan rawat inap dari kamar pulih, apakah biaya operasinya (tindakan lewat order tindakan dan komponen OK menurut `RWI-DEC-196`) **digabung ke invoice `RANAP`** seperti penggabungan alih IGD (`BKC-DEC-117`), atau **tetap di invoice kunjungan asal**? Alias `RWI-OQ-114` butir (b) | `INP-S32` — aturan Billing untuk biaya kunjungan asal; berimbas ke `INP-S25` (kelompok Operasi di Tagihan Pasien) dan `UAT-RWF-16` | `RWI-DEC-201` menyerahkannya ke Billing. Pola penggabungan alih IGD sudah ada di Billing (`BKC-DEC-117`). Belum ada pernyataan Billing untuk asal poliklinik atau ODC | Mengikuti pola alih IGD: biaya kunjungan asal yang belum difinalkan ikut ke invoice `RANAP`. Ini rujukan dari keputusan Billing yang ada, bukan keputusan | Konsekuensi billing: jumlah invoice, pembebanan ke penjamin, dan isi kelompok Operasi pada Tagihan Pasien | Yasmina (Billing), disampaikan lewat Muhammad Hamzah | `OPEN` | Hanya aturan Billing penggabungan itu yang **berhenti**. Permintaan admisi, admisi berlangkah, rujukan kunjungan asal, serah terima, tarif kamar, dan seluruh sisi OK **boleh jalan** |

`DEC-INP-018` bergantung pada pemilik, sehingga **tidak dijawab di sini**. Pertanyaannya sudah terdaftar di decision log sebagai `RWI-OQ-114` butir (b). Decision log mengklasifikasikan `RWI-OQ-114` sebagai penahan implementasi `CAP-RWF-18` dan menyatakan desain sisi Rawat Inap tidak terblokir. Gate ini **sejalan** dengan itu: yang dinilai `BLOCKING` hanya desain aturan Billing untuk butir (b). Butir (a) dan (c) tetap gerbang implementasi.

### 18.8 Contoh konkret untuk blocker

**`DEC-INP-018` — herniorafi dari poliklinik.** Ny. Ani (samaran) menjalani herniorafi elektif dari poliklinik bedah. Pukul 13.00 kasus OK `Completed`: satu baris tindakan "Herniorafi" lewat order tindakan, ditambah jasa anestesi, sewa kamar operasi, dan satu mesh dari OK. Pada saat itu Ani sudah dirawat inap sejak 12.20 dengan invoice `RANAP` yang terbuka.

- **Bila digabung**, kasir melihat satu invoice `RANAP` yang memuat kamar, biaya operasi, dan biaya rawat inap berikutnya. Kelompok Operasi pada Tagihan Pasien di bangsal berisi biaya itu.
- **Bila tetap di kunjungan asal**, Ani atau penjaminnya menerima dua invoice: invoice poliklinik berisi biaya operasi, dan invoice `RANAP` berisi kamar serta layanan sesudahnya. Kelompok Operasi pada Tagihan Pasien bangsal kosong untuk operasi ini.

Kedua pilihan sah secara teknis, tetapi menghasilkan tagihan dan pembebanan penjamin yang berbeda. Karena itu Billing yang memutuskan.

### 18.9 Gerbang implementasi dan gerbang produksi

Butir di bawah **tidak menahan desain**. Butir itu menahan penulisan source bagian terkait, atau pemakaian untuk pasien sungguhan.

| Jenis | Butir | Menahan | Pemilik |
|---|---|---|---|
| Gerbang implementasi | `RWI-OQ-108` — pemilik Farmasi dan persetujuannya agar retur terverifikasi memberi tahu Billing | Retur yang membatalkan tagihan (`INP-S24`, `AC-RWF-012`, `UAT-RWF-23`) | Muhammad Hamzah (penunjukan), lalu pemilik `PharmacyManagement` |
| Gerbang implementasi | `RWI-OQ-114` butir (a) — kamar pulih mengirim dan membatalkan permintaan admisi | Sisi OK pada `INP-S32` | Ikbal Yulianto |
| Gerbang implementasi | `RWI-OQ-114` butir (c) — status `Rejected` di modul OK | `INP-S36` | Ikbal Yulianto |
| Gerbang implementasi | `RWI-OQ-115` — data kantong yang sudah diserahkan dan penerimaan pemberitahuan reaksi | Sisi Bank Darah pada `INP-S35` | Sukma Giri Pratama |
| Gerbang produksi | G-21 — pengesahan isi formulir surveilans, titik ukur transfusi, dan penunjukan pemilik PPI | Pemakaian `INP-S34` dan `INP-S35` untuk pasien sungguhan | Pemilik klinis atau komite PPI (belum tercatat) |
| Gerbang produksi | Konfirmasi clinical governance atas pra-operasi setelah penundaan (`RWI-DEC-199`) | Pemakaian lifecycle baru pra-operasi untuk pasien sungguhan | Pemilik clinical governance |
| Gerbang produksi (dibawa) | G-11, `RWI-RULE-037`, `FIN-UNK-05` (role ke permission), `FIN-UNK-06` (lingkungan uji) | Seperti 17.10 | Seperti 17.10 |

### 18.10 Kesiapan per slice

| Slice | Kesiapan | Yang boleh jalan | Yang berhenti | Decision ID |
|---|---|---|---|---|
| `INP-S23` Gerbang penutupan dan izin kasir | `READY_FOR_DOMAIN_DESIGN` (tetap, 17.8) | Seluruh slice | — | — |
| `INP-S24` Tagihan inti | **`READY_FOR_DOMAIN_DESIGN`** (naik dari `PARTIALLY_READY`) | Seluruh slice, termasuk jembatan layanan klinis `RANAP` dan retur | — | — (`DEC-INP-014` `CLOSED`) |
| `INP-S25` Tagihan Pasien bangsal | `READY_FOR_DOMAIN_DESIGN` (tetap) | Seluruh slice. Isi kelompok Operasi untuk pasien dari poliklinik atau ODC mengikuti `DEC-INP-018` saat datanya ada; bentuk layar dan kontraknya tidak bergantung padanya | — | — |
| `INP-S26` Penunjang dari bangsal | `READY_FOR_DOMAIN_DESIGN` (tetap) | Seluruh slice | — | — |
| `INP-S27` Pasien operasi | **`READY_FOR_DOMAIN_DESIGN`** (naik dari `PARTIALLY_READY`) | Seluruh slice, termasuk biaya operasi dan pra-operasi setelah penundaan | — | — (`DEC-INP-015`, `016` `CLOSED`) |
| `INP-S28` Catatan Keperawatan | **`READY_FOR_DOMAIN_DESIGN`** (naik dari `PARTIALLY_READY`) | Seluruh slice, termasuk Observasi WSD per selang | — | — (`DEC-INP-017` `CLOSED`) |
| `INP-S29` Pemakaian Alat | `READY_FOR_DOMAIN_DESIGN` (tetap) | Seluruh slice | — | — |
| `INP-S30` Katalog tindakan | `READY_FOR_DOMAIN_DESIGN` (tetap) | Seluruh slice | — | — |
| `INP-S31` Serah terima transfer (`P2`) | `READY_FOR_DOMAIN_DESIGN` (tetap) | Seluruh slice | — | — |
| `INP-S32` Admisi dari kamar pulih | **`PARTIALLY_READY`** | Permintaan admisi dan daftarnya, admisi berlangkah, rujukan kunjungan asal, penolakan bila episode aktif, pembatalan oleh OK, daftar pantau, urutan dengan serah terima, tarif kamar | Aturan Billing penggabungan biaya operasi kunjungan asal ke invoice `RANAP` | `DEC-INP-018` |
| `INP-S33` Ringkasan operasi dan daftar pantau | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| `INP-S34` Surveilans infeksi luka operasi | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — (gerbang produksi G-21) | — |
| `INP-S35` Monitoring transfusi | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — (gerbang implementasi `RWI-OQ-115`; gerbang produksi G-21) | — |
| `INP-S36` Penolakan order operasi | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — (gerbang implementasi `RWI-OQ-114` butir c) | — |
| `INP-S37` Laporan transfer ruangan (`P2`) | **`READY_FOR_DOMAIN_DESIGN`** | Seluruh slice | — | — |
| **Finishing secara keseluruhan** | **`PARTIALLY_READY`** | 14 dari 15 slice penuh, dan `INP-S32` sebagian | Satu aturan Billing | `DEC-INP-018` |

**Dependency antar-slice:**

- `INP-S32` → `INP-S27`: serah terima pasca operasi baru dapat diterima setelah pasien menempati bed (`FR-RWF-046`).
- `INP-S32` → `INP-S24`: pasien baru mendapat invoice `RANAP` lewat admisi biasa (`FR-RWF-010`).
- `INP-S33` dan `INP-S34` → `INP-S27`: ringkasan hanya untuk laporan final; formulir surveilans lahir saat kasus `Completed`.
- `INP-S34` → `INP-S23`: surveilans berhenti pada waktu keluar ruangan (`FR-RWF-006`).
- `INP-S35` → data penyerahan Bank Darah (`BbkBloodUnit`), yang sudah ada.
- `INP-S36` → `INP-S27`: menambah satu status pada lifecycle kasus OK yang sama.
- `INP-S37` → `INP-S24`: penanda koreksi penempatan (`FR-RWF-019`) harus dirancang lebih dulu agar laporan membedakan koreksi dari transfer.
- `UAT-RWF-16` baru dapat menyatakan invoice tujuan setelah `DEC-INP-018` tertutup. `UAT-RWF-23` baru dapat dijalankan setelah `RWI-OQ-108` dijawab.

**Status modul Rawat Inap** tetap **`PARTIALLY_READY`**: selain `DEC-INP-018`, `INP-S09` (serah terima IGD) masih menunggu pemilik IGD, handover shift tetap `DEFERRED`, dan transfusi selain monitoring tetap `DEFERRED` (`DEC-INP-012`).

### 18.11 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

1. `design-business-module` untuk amandemen keempat sub-modul pada seluruh slice siap (18.10), termasuk ketiga slice yang naik (`INP-S24`, `S27`, `S28`) dan lima slice klaster Pasca Operasi. Amandemen `integrasi-billing` tetap wajib mencabut bagian yang digantikan, seperti dicatat 17.9 butir 1.
2. `design-business-module` untuk `INP-S32` pada seluruh sisi Rawat Inap dan OK. Desain mencatat titik sambung ke aturan Billing `DEC-INP-018` sebagai keputusan yang belum ada, bukan menebaknya.
3. Penutupan `DEC-INP-018` lewat `grill-me` atau jawaban langsung Yasmina yang diteruskan Muhammad Hamzah, paralel dengan desain.
4. Pengumpulan persetujuan gerbang implementasi `RWI-OQ-108`, `RWI-OQ-114` butir (a) dan (c), serta `RWI-OQ-115`, paralel dengan desain.
5. Konfirmasi prioritas `CAP-RWF-18`, `19`, dan `22` (G-26) saat pemilik meninjau PRD v`0.4`.

**Harus berhenti:**

1. Desain aturan Billing penggabungan biaya operasi kunjungan asal ke invoice `RANAP`, sampai `DEC-INP-018` tertutup.
2. Implementasi retur yang membatalkan tagihan, pengiriman permintaan admisi dari OK, status `Rejected` di OK, serta pembacaan kantong dan pemberitahuan reaksi di Bank Darah, sampai gerbang implementasi masing-masing dijawab.
3. Pemakaian formulir surveilans, titik ukur transfusi, dan lifecycle pra-operasi baru untuk pasien sungguhan, sampai disahkan klinis.
4. Implementasi apa pun sebelum blueprint amandemen disetujui dan task diturunkan. Wewenang tulis, migration, dan putar ulang tetap terpisah per task.

### 18.12 Handoff

```yaml
gate_revision: 1.9
blueprint_id: RWI-BP-001
assessed_scope: >
  PRD-RWI-FINISHING-001 v0.4 — re-gate INP-S24, INP-S27, INP-S28 (DEC-INP-014..017 ditutup)
  dan klaster Pasca Operasi CAP-RWF-18..23 sebagai slice baru INP-S32..INP-S37
not_reassessed: [INP-S23, INP-S25, INP-S26, INP-S29, INP-S30, INP-S31]  # tetap seperti 17.8
evidence:
  decision_log: 00-interview-decisions.md revision 30, sha256 aa92c5ddd217b0bd95abf628ae484a1c0caddcdf386e7834f0715ed216e2a439
  prd: 05-prd-to-mvp-finishing-rawat-inap.md v0.4, sha256 aee2afdb03e62c2bcdbd2e8fc832bb634df40686079c1b5e4f7f6f4d1ae46d29
  hisys_evidence: Pasca-Operasi-ke-Rawat-Inap.md, sha256 ce43e811cf35b074bedd3d5d6bbc0c8fc8941dc5dbf49afa2991d3f96fc853c7 (praktik sistem lain, bukan keputusan)
  capability_map: 01-existing-capability-map.md revision 1.6 bagian 19, sha256 2f78b74e37c983ffa81cbd510e129906243634536c04df105811c827930eccd2
  facts: [RWI-FACT-057, RWI-FACT-058]
  source_reads_this_gate:
    - OperatingRoomCommandSupport.cs:77-88 (Postponed -> Reschedule; Ready tanpa Postpone)
    - OperatingRoomSchedulingService.cs:199-201 (Postpone hanya dari Requested/Scheduled)
    - InpBedPlacement.cs:39-58 (TransferReason, ChangeReason, IsSuperseded, pencatat)
    - BbkBloodUnit.cs:93-114 (IssuedToPatientId, IssuedAt, IssuedByUserId)
  backend_sha: c8e99ce5 (audit); HEAD 425cfeae docs-only
  frontend_sha: 22ad67330 (audit); HEAD ee75e055b; perubahan lokal belum di-commit tidak menyentuh pemicu 19.9
  baseline_reference: indonesia-hospital-domain-reference inpatient.md — ID-INP-CAP-001, ID-INP-CAP-015 (REFERENCE_ONLY); operating-theatre, blood-bank-transfusion, infection-prevention-control PLANNED
readiness:
  READY_FOR_DOMAIN_DESIGN: [INP-S23, INP-S24, INP-S25, INP-S26, INP-S27, INP-S28, INP-S29, INP-S30, INP-S31, INP-S33, INP-S34, INP-S35, INP-S36, INP-S37]
  PARTIALLY_READY:
    INP-S32: {stop: "aturan Billing penggabungan biaya operasi kunjungan asal ke invoice RANAP", decision: DEC-INP-018}
closed_decisions:
  - DEC-INP-014: RWI-DEC-195
  - DEC-INP-015: RWI-DEC-196
  - DEC-INP-016: RWI-DEC-199
  - DEC-INP-017: RWI-DEC-200
open_decisions:
  - DEC-INP-018: invoice tujuan biaya operasi kunjungan poliklinik/ODC (alias RWI-OQ-114 butir b) — owner Yasmina
implementation_gates: [RWI-OQ-108, "RWI-OQ-114 butir a", "RWI-OQ-114 butir c", RWI-OQ-115]
non_blocking: [G-01, G-02, G-04, G-05, G-10, G-11, G-12, G-13, G-14, G-15, G-16, G-18, G-19, G-20, G-21, G-22, G-23, G-24, G-25, G-26]
production_gate: [G-21 isi formulir surveilans dan titik ukur transfusi + pemilik PPI, konfirmasi klinis RWI-DEC-199, G-11, RWI-RULE-037, FIN-UNK-05, FIN-UNK-06]
domain_architecture: >
  opsional; tidak disarankan — kepemilikan data klaster Pasca Operasi sudah ditetapkan
  (RWI-DEC-200 s.d. 205: ClinicalManagement, OperatingRoomManagement, InPatientManagement, rujukan BloodBankManagement)
next_skill:
  - design-business-module untuk seluruh slice siap dan sisi Rawat Inap/OK INP-S32
  - grill-me atau jawaban Yasmina untuk DEC-INP-018, paralel
superseded_in_this_document: kesiapan INP-S24, INP-S27, INP-S28 pada 17.8 dan 17.10 (oleh 18.10)
```

---

## 19. Evaluasi Ulang Gerbang Kelengkapan Requirement — Finishing Rawat Inap, penutupan butir terbuka pasca-desain — revision `1.10`

### 19.1 Scope, identitas slice, dan bukti acuan

**Yang dinilai ulang.** Gate ini melanjutkan bagian 18 setelah tiga langkah pada 2 Oktober 2026: Amendment Pass `grill-me` (decision log revision `31`, `RWI-DEC-206` s.d. `RWI-DEC-220`), penyelarasan desain revision `8`, dan approval desain itu oleh pemilik (`RWI-DEC-221`, decision log revision `32`).

1. **`INP-S32`** — satu-satunya slice Finishing yang di 18.10 masih `PARTIALLY_READY`. Penahannya, `DEC-INP-018`, sudah dijawab (`RWI-DEC-207`).
2. **Dampak keputusan baru pada slice lain.** `RWI-DEC-208` s.d. `RWI-DEC-221` menyentuh `INP-S24`, `S25`, `S26`, `S27`, `S28`, `S29`, `S30`, `S33`, `S34`, `S35`, `S36`, dan `S37`. Kesiapan slice-slice itu sudah `READY_FOR_DOMAIN_DESIGN`; yang diperbarui adalah status gap dan gerbangnya.
3. **Bukti implementasi baru** yang muncul sesudah gate `1.9`: backend `bf5c6bde` dan frontend `f74758af5`.

`INP-S23` dan `INP-S31` tidak tersentuh keputusan baru, sehingga tidak dinilai ulang.

**Urutan langkah yang perlu diketahui pembaca.** Biasanya gate berjalan sebelum desain. Kali ini desain sudah diselaraskan dan disetujui lebih dulu, karena semua keputusan penahannya sudah `approved` sebelum desain ditulis. Gate ini menegaskan kesiapan secara formal dan mencatat bukti baru. **Gate ini tidak mengubah desain yang disetujui.**

**Bukti yang dipakai:**

| Jenis bukti | Sumber | Revision / hash |
|---|---|---|
| Requirement eksplisit pemilik | `00-interview-decisions.md`, `RWI-DEC-206` s.d. `RWI-DEC-221`, `RWI-AC-330` s.d. `RWI-AC-340`, `RWI-FACT-059`, `RWI-FE-006` | Revision `32`, SHA-256 `2102ed1d…ffed43b25` |
| Keputusan Billing yang dirujuk | `billing-kasir/00-interview-decisions.md` `BKC-DEC-118` (konsolidasi alih IGD tanpa melebur, satu penyelesaian akhir) | `approved` 24 September 2026 |
| Dokumen produk | `PRD-RWI-FINISHING-001` v`0.4` | SHA-256 `0f658455…27cfabe1a`. **Sebagian tertinggal** dari decision log revision `31`/`32` (G-28) |
| Desain yang disetujui | Blueprint revision `8`: kontrak `integrasi-billing` `1.1.0`, `keperawatan` `0.6.0`, `dokter-rawat-inap` `0.7.0`, `episode-rawat-inap` `0.10.0`, `02-module-map.md` revision `4`; `approved` 2026-10-02 (`RWI-DEC-221`) | Hash approval pada manifest masing-masing sub-modul |
| Implementasi V2 terverifikasi | `01-existing-capability-map.md` revision `1.6` bagian 19, ditambah pembacaan source gate ini (di bawah) | SHA-256 capability map `2f78b74e…30eccd2` |
| HEAD saat gate dijalankan | Backend `bf5c6bde`: satu commit sesudah `8d96a978`, **hanya dokumen blueprint**. Sejak audit `c8e99ce5`, perubahan kode hanya saringan pencarian census dan daftar pantau. Frontend `f74758af5`: tiga commit sejak audit `22ad67330` — gaya tampilan, pencarian census, label menu "Dashboard", dan **form Penunjang Medis baru di ruang kerja dokter** (19.5) | — |
| Baseline rujukan | Tidak ada observasi baru. `ID-INP-CAP-001` pada 18.1 tetap dipakai untuk `INP-S32` | `REFERENCE_ONLY` |

**Pembacaan source tambahan pada gate ini:**

| Fakta | Bukti | Dipakai untuk |
|---|---|---|
| Pembayaran Billing hari ini **per invoice**: `CreateSettlementRequest` hanya memuat satu `InvoiceId` | `BillingManagement/Billing/Dtos/BillingSettlementDtos.cs:6-8`; `Controllers/BillingSettlementsController.cs:27` | G-27 |
| `BILL-INT-007` (`BKC-DEC-118`) baru berupa keputusan; belum ada desainnya di blueprint `billing-kasir` | Pencarian `BILL-INT-007` di `docs/module-blueprints/billing-kasir/` hanya menemukan `00-interview-decisions.md` | G-27 |
| Form Gizi dan Bank Darah di ruang kerja dokter mengirim pesanan **langsung** ke `nutrition-management/orders` dan `blood-bank-management/blood-orders`, dengan `requestingDoctorId = episode.admissionDoctorId`; layar menampilkan nama dokter aktif. Form Hemodialisa baru memakai sumber peminta yang sama | FE `f74758af5`: `physician-workspace/tabs/supporting-service/supporting-nutrition-form.jsx:66-71`, `supporting-blood-bank-form.jsx:80-95` dan `:182-183`, `supporting-hemodialysis-form.jsx:86`; `use-inpatient-supporting-service.jsx:585-660` | IMP-RWF-01 |
| Form Bank Darah memakai `bloodComponentId` tetap `00000000-0000-0000-0000-000000000001`, nama komponen dari konstanta frontend, dan cadangan `serviceUnitId` berupa GUID tetap | `supporting-blood-bank-form.jsx:83-90` | IMP-RWF-02 |
| Form Rehab Medik membuat **order tindakan rawat inap** dengan `procedureId` palsu `00000000-…-000000000101` s.d. `…107` dan label buatan | `supporting-rehab-form.jsx:20-26`, `:38-60`; `use-inpatient-supporting-service.jsx:672-695` (`createInpatientProcedureOrder`) | IMP-RWF-03 |
| Ruang kerja **perawat** tidak berubah (commit terakhir `a03676d1d`): Gizi, Bank Darah, Rehab, dan dialisis tetap "Integrasi belum tersedia" tanpa request jaringan | `nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx:72-75`, `:130` | Batas IMP-RWF-01 s.d. 03: hanya ruang kerja dokter |
| Daftar berkas pemicu capability map 19.9 tidak memuat berkas ruang kerja dokter di atas | `01-existing-capability-map.md` 19.9 | IMP-RWF-04 |

**Wewenang bukti yang diterapkan.** Untuk apa yang **seharusnya dibangun**, keputusan pemilik pada decision log menang atas PRD (`RWI-DEC-165`) dan atas implementasi yang sudah ada. Untuk apa yang **sudah ada**, pembacaan source di atas yang dipakai. Karena kedua pertanyaan itu berbeda, penyimpangan frontend pada 19.5 **bukan** `CONFLICT` requirement: requirement-nya jelas dan menang, sedangkan kodenya yang harus menyesuaikan.

### 19.2 Ringkasan untuk pembaca umum

**Seluruh 15 slice Finishing kini siap penuh untuk dirancang**, dan desainnya sudah disetujui.

| Yang ditunggu di gate `1.9` | Keadaan sekarang |
|---|---|
| Ke invoice mana biaya operasi pasien poli/ODC yang dirawat inap (`DEC-INP-018`) | **Diputuskan** (`RWI-DEC-207`): tetap di invoice kunjungan asal, ditautkan ke invoice rawat inap, dibayar sekali saat pulang |
| Persetujuan Farmasi, Kamar Operasi, dan Bank Darah | **Semua ada** (`RWI-DEC-208` s.d. `210`); tidak ada lagi gerbang persetujuan modul tetangga |
| Prioritas klaster Pasca Operasi | **`P1`** (`RWI-DEC-217`) |
| Perawat melihat harga atau tidak (G-05) | **Melihat perkiraan harga** di layar pemesanan (`RWI-DEC-218`, `219`), kebalikan dari usulan gate |

Satu hal baru yang **bukan** kekurangan requirement tetapi wajib ditindaklanjuti: kode frontend ruang kerja **dokter** yang dibuat 1–2 Oktober belum mengikuti desain yang disetujui. Pesanan darah dan gizi tercatat atas nama DPJP saat admisi, bukan dokter yang memesan, dan penugasan dokter tidak diperiksa. Form Bank Darah dan Rehab mengirim ID palsu. Ruang kerja perawat tidak terdampak (19.5).

### 19.3 Penutupan Decision ID dan gerbang implementasi gate `1.9`

| Butir | Ditutup oleh | Isi jawaban | Gap yang tertutup | Sisa yang dibawa |
|---|---|---|---|---|
| `DEC-INP-018` | `RWI-DEC-207` | Pola alih IGD `BKC-DEC-118`: baris operasi tetap pada kunjungan asal tanpa berpindah; Billing menautkan kunjungan asal ke invoice `RANAP` saat admisi dari permintaan; satu penyelesaian akhir dengan kwitansi per unit; Tagihan Pasien bangsal menampilkan kelompok Operasi kunjungan asal tanpa rupiah bagi perawat | G-17 | G-27: implementasi penyelesaian multi-invoice milik Billing |
| `RWI-OQ-108` | `RWI-DEC-210` | Pemilik Farmasi Ikbal Yulianto menyetujui retur terverifikasi membatalkan tagihan | — | — |
| `RWI-OQ-114` (a), (c) | `RWI-DEC-208` | Ikbal Yulianto menyetujui permintaan admisi dari kamar pulih dan status `Rejected` | — | — |
| `RWI-OQ-115` | `RWI-DEC-209` | Sukma Giri Pratama menyetujui data kantong per pasien dan pemberitahuan reaksi | — | Gerbang produksi klinis G-21 tetap |

`DEC-INP-018` kini **`CLOSED`**. Rujukan "`BKC-DEC-117`" pada 18.6 dan 18.7 keliru; pola alih IGD ada pada **`BKC-DEC-118`** (`RWI-FACT-059` butir 3).

### 19.4 Matriks 18 dimensi kelengkapan

Kode isi sel sama dengan 18.4.

**`INP-S32` — dinilai penuh:**

| No | Dimensi | `S32` | Dasar |
|---:|---|---|---|
| 01 | Tujuan | C | `RWI-DEC-201`; ditambah tagihan sekali bayar `RWI-DEC-207` |
| 02 | Aktor | C | Kamar pulih, petugas admisi, perawat unit tujuan, kasir (penyelesaian akhir) |
| 03 | Pemicu / prasyarat | C | Keputusan kamar pulih "rawat inap" atau "ICU" untuk pasien tanpa episode aktif |
| 04 | Alur utama | C | Permintaan → admisi berlangkah → bed → serah terima → kasus `Completed`; tautan kunjungan asal saat admisi; satu penyelesaian akhir saat pulang |
| 05 | Alur alternatif / exception | C | Pasien sudah dirawat → tidak ada permintaan; OK membatalkan beralasan; admisi biasa ditolak bila ada permintaan menunggu (`RWI-DEC-220` butir 1); tanpa permintaan → tanpa tautan. Invoice asal yang sudah final atau lunas → aturan internal Billing yang sengaja di luar scope (`RWI-DEC-207`) |
| 06 | Data minimum | C | Kunjungan asal, kasus OK, dokter operator, tingkat perawatan; tautan kunjungan–invoice `RANAP` |
| 07 | Aturan bisnis / validation | C | Tidak ada baris yang berpindah invoice; hanya baris operasi kunjungan asal yang tampil di Tagihan Pasien bangsal |
| 08 | Status / perubahan status | C (sebelumnya P, G-16) | Menunggu, selesai, dibatalkan — kontrak `episode-rawat-inap` `0.10.0` state 9.4, disetujui `RWI-DEC-221` |
| 09 | Peran / authorization | C (sebelumnya P, G-16) | Baca daftar lewat permission khusus; OK memanggil dalam proses; perawat tanpa rupiah (`RWI-DEC-160`, `170`) |
| 10 | Dependency antarmodul | C | Kamar Operasi (`RWI-DEC-208`), Billing (`RWI-DEC-207`); penyelesaian multi-invoice bergantung implementasi `BKC-DEC-118` (G-27) |
| 11 | Integrasi internal / eksternal | C | Seluruhnya internal; tidak ada pihak luar |
| 12 | Hasil akhir | C | Episode berjalan, kasus OK selesai, kunjungan asal tertaut, satu kwitansi saat pulang |
| 13 | Pembatalan / koreksi | C | Pembatalan permintaan oleh OK; koreksi tautan yang salah adalah aturan internal Billing (`RWI-DEC-207`) |
| 14 | Audit / histori | C | Riwayat permintaan; tautan menyimpan tanda terima pembuatnya |
| 15 | Notifikasi | C (sebelumnya P, G-02 dan G-18) | Daftar pantau dengan ambang 30 menit yang dapat diatur (`RWI-DEC-220` butir 6) |
| 16 | Dampak billing / charge | C (sebelumnya M, G-17) | `RWI-DEC-207` |
| 17 | Dampak keselamatan klinis | – | Alasan sama dengan 18.5: keputusan klinis tetap pada kamar pulih dan dokter |
| 18 | Pelaporan / traceability | C | Kwitansi berincian per unit; daftar pantau lama menunggu |

**Slice lain — hanya sel yang berubah:**

| Slice | Dimensi | Sebelum | Sesudah | Dasar |
|---|---|---|---|---|
| `S24` | 13 Pembatalan / koreksi (retur) | C, gerbang `RWI-OQ-108` | C, tanpa gerbang | `RWI-DEC-210` |
| `S25` | 16 Billing | C | C, kelompok Operasi memuat baris kunjungan asal yang tertaut | `RWI-DEC-207` |
| `S26` | 07 Aturan, 09 Peran | P (G-05) | C — perkiraan harga bagi setiap pemesan | `RWI-DEC-218`, `219` |
| `S27` | 07 Aturan | C | C, ditambah perkiraan tarif tindakan di Pemesanan Ruangan Bedah; pemesanan hanya untuk episode `Admitted` | `RWI-DEC-219`, `RWI-DEC-220` butir 2 |
| `S27` | 05, 08 | M (G-14) | C | Kontrak `0.10.0` state 9.1, disetujui `RWI-DEC-221` |
| `S28` | 09 Peran | M (G-15) | C | Kontrak `keperawatan` `0.6.0` API 8.4, disetujui `RWI-DEC-221` |
| `S29` | 16 Billing | C | C, Pemakaian Alat tidak menampilkan harga | `RWI-DEC-219` |
| `S33` | 07, 09, 15, 17 | P (G-18, G-19) | C | `RWI-DEC-220` butir 6; kontrak `0.10.0` API 11.5.1, disetujui `RWI-DEC-221` |
| `S34` | 07, 08, 09 | P (G-20) | C; isi formulir tetap gerbang produksi G-21 | Kontrak `keperawatan` `0.6.0` state 8 dan integrasi 9, disetujui `RWI-DEC-221` |
| `S35` | 05, 06, 08 | P (G-22, G-24) | C; nilai toleransi tetap `CONFIGURABLE_DEFAULT` | Kontrak `keperawatan` `0.6.0` API 8.6, kamus data 12.10, disetujui `RWI-DEC-221` |
| `S35` | 15 Notifikasi | C, P (G-23) | C | Kotak masuk reaksi Bank Darah, disetujui `RWI-DEC-221` |
| `S36` | 09 Peran | C, P (G-25) | C | `OperatingRoomCase : Reject`, disetujui `RWI-DEC-221` |
| `S37` | 07 Aturan | C | C, ditambah batas periode 31 hari dan butir menu ke-10 | `RWI-DEC-220` butir 4; `RWI-DEC-214`, `215` |

### 19.5 Temuan bukti implementasi baru — bukan gap requirement

Temuan di bawah berasal dari commit frontend `f74758af5`. Requirement-nya sudah `CONFIRMED`, sehingga temuan ini **tidak mengubah kesiapan slice**. Temuan ini adalah penyimpangan kode terhadap desain yang disetujui, dan wajib diperbaiki sebelum bagian itu dirilis.

| ID | Temuan | Bertentangan dengan | Akibat bila dibiarkan | Tindak lanjut |
|---|---|---|---|---|
| IMP-RWF-01 | Pesanan Gizi dan Bank Darah dari ruang kerja dokter dikirim langsung ke modul pemilik, tanpa adapter Rawat Inap. `requestingDoctorId` selalu DPJP saat admisi, padahal layar menampilkan nama dokter aktif | `RWI-DEC-171`, `RWI-DEC-188`; `INV-RWF-20` (penugasan diperiksa), `INV-RWF-21` (dokter peminta = akun login bila dokter memesan sendiri); `FR-RWF-031`, `032`, `036` | Pesanan darah konsulen atau dokter jaga tercatat atas nama DPJP. DPJP yang sudah dialihkan masih tercatat sebagai peminta. Penugasan dokter tidak diperiksa. **Risiko akuntabilitas dan keselamatan klinis**, terutama untuk produk darah | Task frontend: arahkan ke `…/ancillary-orders/*` (`dokter-rawat-inap` `0.7.0`) setelah adapter backend tersedia; peminta dari akun login. Sampai itu, bagian ini tidak dirilis |
| IMP-RWF-02 | Form Bank Darah mengirim `bloodComponentId` tetap `00000000-…-0001` dan nama komponen dari konstanta frontend, bukan master komponen darah | `RWI-DEC-108` (dilarang data contoh dan penyimpanan tiruan); integritas data pesanan darah | Pesanan merujuk komponen yang tidak ada di master, atau gagal disimpan; nama komponen tidak sama dengan yang diproses Bank Darah | Task frontend: pilihan komponen dari master Bank Darah |
| IMP-RWF-03 | Form Rehab Medik membuat order tindakan rawat inap dengan tujuh `procedureId` palsu | `RWI-DEC-108`, `FR-RWF-035`, PRD 5.4: Rehab Medik tetap *placeholder* "Integrasi belum tersedia" | Order tindakan berisi ID yang tidak ada di master (gagal atau data rusak); pesan berhasil palsu bagi dokter | Task frontend: kembalikan ke *placeholder*. Bila pemilik memang ingin membuka Rehab Medik lewat order tindakan, itu perluasan scope dan perlu keputusan `grill-me` lebih dulu — gate ini **tidak** menganggapnya diputuskan |
| IMP-RWF-04 | Daftar berkas pemicu capability map 19.9 tidak memuat berkas ruang kerja dokter yang berubah, sehingga bagian 19 tidak otomatis basi walaupun status as-is `FIN-CAP-17` dan `FIN-CAP-18` (dulu *placeholder*) sudah berubah di ruang kerja dokter | — (kelengkapan bukti) | Skill hilir dapat memakai gambaran as-is yang usang | `trace-existing-capabilities` impact scan terfokus pada `physician-workspace/tabs/supporting-service/` dan hook-nya, sekaligus melengkapi daftar pemicu |

Dua hal yang **bukan** temuan:

- **Perkiraan harga** belum tampil di form baru karena endpoint `coverage-status` yang membawanya belum dibangun.
- **Hemodialisa.** Form baru juga mengisi `requestingDoctorId` dengan DPJP saat admisi; modal lama memakai `episode.doctorId` (`hemodialysis-order-modal.jsx:46`, `:116`). Hemodialisa berstatus `READY TO REUSE` dan perilakunya tidak diatur ulang Finishing (`FR-RWF-035`), sehingga tidak dicatat sebagai penyimpangan. Bila pemilik menghendaki peminta Hemodialisa dari akun login, itu keputusan baru.

### 19.6 Daftar gap dan dampaknya

**Tertutup oleh keputusan pemilik:**

| Gap | Ditutup oleh | Catatan |
|---|---|---|
| G-05 | `RWI-DEC-218`, `RWI-DEC-219` | Isi keputusan **berlawanan** dengan usulan gate: perawat melihat perkiraan harga di semua layar pemesanan. `RWI-DEC-160` butir 2 diamendemen; Tagihan Pasien tetap tanpa rupiah |
| G-17 | `RWI-DEC-207` | `DEC-INP-018` `CLOSED` |
| G-18 | `RWI-DEC-220` butir 6 | Ambang 60 dan 30 menit yang dapat diatur admin |
| G-26 | `RWI-DEC-217` | `P1` untuk `CAP-RWF-18`, `19`, `22` |

**Dikonfirmasi lewat approval kontrak `RWI-DEC-221`.** Usulan berikut tertulis eksplisit di kontrak yang disetujui pemilik, sehingga statusnya naik dari `PROPOSED`/`MISSING` menjadi `CONFIRMED`. Kenaikan ini tidak diam-diam: dasarnya approval tertulis atas dokumen yang memuat aturannya.

| Gap | Isi yang kini `CONFIRMED` | Letak di kontrak yang disetujui |
|---|---|---|
| G-02 | Pemberitahuan berbentuk daftar yang disegarkan berkala; notifikasi seketika tetap `DEFERRED` | Daftar pantau `FE-INP-30`, `FE-INT-03`, `FE-KEP-30`, `FE-DOK-15` |
| G-13 | Katalog tindakan bangsal dengan saringan audiens perawat dan dokter | `dokter-rawat-inap` `0.7.0` API 13.5 (`careSetting`, `audience`) |
| G-14 | Kasus `Ready` tidak dapat ditunda, mengikuti source | `episode-rawat-inap` `0.10.0` state 9.1 |
| G-15 | Selang WSD didaftarkan dan dikoreksi perawat dengan permission cairan, koreksi berversi, tanpa hapus fisik | `keperawatan` `0.6.0` API 8.4 |
| G-16 | Status permintaan admisi menunggu, selesai, dibatalkan; permission baca daftar | `episode-rawat-inap` `0.10.0` state 9.4, permission 9.1 |
| G-19 | Ringkasan operasi lewat `OperatingRoomCase : Read`; laporan draft tidak ditampilkan | `episode-rawat-inap` `0.10.0` API 11.5.1 |
| G-20 | Indikator suhu "ya" bila ada nilai ≥ 38 °C pada hari kalender itu; formulir selesai setelah hari ke-15; berhenti saat keluar ruangan | `keperawatan` `0.6.0` integrasi 9, state 6 |
| G-22 (a), (b) | Waktu mulai transfusi wajib; penghentian dengan waktu dan alasan, titik sesudahnya bertanda dihentikan | `keperawatan` `0.6.0` API 8.6, kamus data 12.10 |
| G-23 | Reaksi sebagai butir kotak masuk Bank Darah | `keperawatan` `0.6.0` API 8.7 |
| G-24 | Kantong yang dapat dipilih adalah kantong yang diserahkan kepada pasien dan belum dipantau; nilai titik ukur disimpan sekali | `keperawatan` `0.6.0` API 8.6 `selectable-units` |
| G-25 | Permission tolak `OperatingRoomCase : Reject` terpisah dari batal | `episode-rawat-inap` `0.10.0` permission 9.1 |
| — | **Catatan pemilik:** harga obat dan harga order tindakan yang dirujuk pemesanan ruang bedah memakai endpoint lama, sehingga tampil bagi pemegang hak **baca** endpoint, bukan hanya pemegang hak membuat pesanan | `dokter-rawat-inap` `0.7.0` `02-backend-architecture.md` 12.13 |

**Tetap berlaku tanpa perubahan:** G-01, G-04 (`CONFIGURABLE_DEFAULT`), G-10 (`CONFIGURABLE_DEFAULT`), G-11 (gerbang produksi), G-12 (`CONFIGURABLE_DEFAULT`), G-21 (gerbang produksi), dan G-22 (c) — toleransi titik ukur tetap `CONFIGURABLE_DEFAULT` dengan bawaan 10 menit yang menunggu pengesahan klinis.

**Gap baru:**

| Gap | Slice | Pernyataan | Status bukti | Dampak | Penjelasan dan usulan |
|---|---|---|---|---|---|
| G-27 | `S25`, `S32` | **Penyelesaian satu kwitansi** untuk invoice kunjungan asal dan invoice `RANAP` yang tertaut. Requirement-nya `CONFIRMED` (`RWI-DEC-207` butir 3, `BKC-DEC-118`), tetapi pembayaran Billing hari ini per invoice, dan desain `BILL-INT-007` belum ada di blueprint `billing-kasir` | `MISSING` (desain dan implementasi milik Billing) | `NON_BLOCKING_STANDARD` untuk desain Rawat Inap; **dependency implementasi** bagian "satu transaksi" `RWI-AC-330` | Tautan dari `integrasi-billing` 9.14 adalah masukannya dan tidak bergantung pada bentuk penyelesaian. Usulan: Billing merancang `BILL-INT-007` sekali untuk alih IGD dan asal poli/ODC. Sampai tersedia, `RWI-AC-330` lulus untuk tautan dan Tagihan Pasien, tetapi belum untuk "satu transaksi" |
| G-28 | Seluruh Finishing | **PRD Finishing v`0.4` tertinggal** dari decision log revision `31`/`32`: `FR-RWF-034` (harga), 11.2 butir 3, 10 (rujukan `BKC-DEC-117`), dan 13, serta penyebutan `DEC-INP-018` dan `UI-RWF` sebagai butir terbuka | Ketidaksesuaian dokumen — diselesaikan oleh `RWI-DEC-165` (decision log menang), sehingga **bukan** `CONFLICT` | `NON_BLOCKING_STANDARD` | Usulan: PRD diperbarui ke v`0.5` agar pembaca yang hanya membuka PRD tidak salah tangkap. Tidak menahan langkah apa pun |

**Pertentangan (`CONFLICT`).** Tidak ada pertentangan requirement yang terbuka. Penyimpangan frontend pada 19.5 diselesaikan oleh wewenang bukti (requirement menang), sehingga tidak dicatat sebagai `CONFLICT`.

### 19.7 Decision Log

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Pemilik | Status | Dampak implementasi / domain |
|---|---|---|---|---|---|---|
| `DEC-INP-018` | Invoice tujuan biaya operasi kunjungan poli/ODC bila pasien kemudian dirawat inap | `INP-S32`, `INP-S25` | `RWI-DEC-207` | Yasmina, disampaikan lewat Muhammad Hamzah | **`CLOSED`** | Tautan non-destruktif dirancang di `integrasi-billing` 9.14 dan disetujui (`RWI-DEC-221`). Penyelesaian satu kwitansi: G-27 |

**Tidak ada Decision ID baru.** IMP-RWF-03 tidak memerlukan keputusan, karena `RWI-DEC-108` sudah jelas. Bila pemilik ingin membuka Rehab Medik, pertanyaannya dibawa ke `grill-me` dan baru saat itu diberi Decision ID.

### 19.8 Gerbang implementasi, rilis, dan produksi

| Jenis | Butir | Menahan | Pemilik |
|---|---|---|---|
| Gerbang persetujuan modul tetangga | **Tidak ada lagi** (`RWI-DEC-208` s.d. `210`) | — | — |
| Dependency implementasi | G-27 — penyelesaian multi-invoice `BKC-DEC-118` / `BILL-INT-007` | Bagian "satu transaksi, satu kwitansi" `RWI-AC-330` | Yasmina (Billing) |
| Gerbang rilis | IMP-RWF-01 s.d. 03 — form Penunjang Medis ruang kerja dokter | Rilis form Gizi, Bank Darah, dan Rehab Medik ruang kerja dokter | Pelaksana frontend; `plan-module-delivery` menurunkan task-nya |
| Gerbang produksi | G-21 isi formulir surveilans, titik ukur transfusi, pemilik PPI; G-22 (c) nilai toleransi; konfirmasi klinis pra-operasi setelah penundaan (`RWI-DEC-199`); isi awal butir persiapan bedah dan nilai jenis anestesi rencana; G-11; `RWI-RULE-037`; `FIN-UNK-05`; `FIN-UNK-06` | Pemakaian untuk pasien sungguhan | Pemilik klinis, komite PPI, pemilik OK |

### 19.9 Kesiapan per slice

| Slice | Kesiapan | Catatan |
|---|---|---|
| `INP-S23` | `READY_FOR_DOMAIN_DESIGN` (tetap) | — |
| `INP-S24` | `READY_FOR_DOMAIN_DESIGN` | Gerbang `RWI-OQ-108` tertutup |
| `INP-S25` | `READY_FOR_DOMAIN_DESIGN` | Kelompok Operasi memuat kunjungan asal tertaut (`RWI-DEC-207`); G-27 dependency |
| `INP-S26` | `READY_FOR_DOMAIN_DESIGN` | G-05 tertutup; gerbang rilis IMP-RWF-01 s.d. 03 |
| `INP-S27` | `READY_FOR_DOMAIN_DESIGN` | — |
| `INP-S28` | `READY_FOR_DOMAIN_DESIGN` | — |
| `INP-S29` | `READY_FOR_DOMAIN_DESIGN` | — |
| `INP-S30` | `READY_FOR_DOMAIN_DESIGN` | — |
| `INP-S31` | `READY_FOR_DOMAIN_DESIGN` (tetap) | — |
| `INP-S32` | **`READY_FOR_DOMAIN_DESIGN`** (naik dari `PARTIALLY_READY`) | `DEC-INP-018` `CLOSED`; G-27 dependency Billing |
| `INP-S33` | `READY_FOR_DOMAIN_DESIGN` | — |
| `INP-S34` | `READY_FOR_DOMAIN_DESIGN` | Gerbang produksi G-21 |
| `INP-S35` | `READY_FOR_DOMAIN_DESIGN` | Gerbang `RWI-OQ-115` tertutup; gerbang produksi G-21, G-22 (c) |
| `INP-S36` | `READY_FOR_DOMAIN_DESIGN` | Gerbang `RWI-OQ-114` (c) tertutup |
| `INP-S37` | `READY_FOR_DOMAIN_DESIGN` | — |
| **Finishing secara keseluruhan** | **`READY_FOR_DOMAIN_DESIGN`** — 15 dari 15 slice | Desain revision `8` sudah disetujui (`RWI-DEC-221`) |

Dependency antar-slice pada 18.10 tetap berlaku, kecuali butir terakhir: `UAT-RWF-16` kini dapat menyatakan invoice tujuan (`RWI-DEC-207`), dan `UAT-RWF-23` tidak lagi menunggu `RWI-OQ-108`.

**Status modul Rawat Inap** tetap **`PARTIALLY_READY`** karena slice di luar Finishing: `INP-S09` (serah terima IGD) masih menunggu pemilik IGD, handover shift `DEFERRED`, dan transfusi selain monitoring `DEFERRED` (`DEC-INP-012`).

### 19.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

1. `plan-module-delivery` untuk seluruh 15 slice Finishing, gelombang `RWF-W0` s.d. `RWF-W7` menurut `02-module-map.md` 7.4. Desain revision `8` sudah menyerap semua keputusan dan sudah disetujui, sehingga `design-business-module` **tidak** perlu dijalankan ulang.
2. Task perbaikan frontend IMP-RWF-01 s.d. 03 diturunkan bersama gelombang `dokter-rawat-inap`.
3. `trace-existing-capabilities` impact scan terfokus untuk IMP-RWF-04, paralel.
4. Penyusunan `BILL-INT-007` oleh pemilik Billing untuk G-27, paralel.

**Harus berhenti:**

1. Rilis form Gizi, Bank Darah, dan Rehab Medik di ruang kerja dokter sampai IMP-RWF-01 s.d. 03 diperbaiki.
2. Klaim "satu transaksi, satu kwitansi" pada `RWI-AC-330` sampai G-27 tersedia di Billing.
3. Pemakaian untuk pasien sungguhan sampai gerbang produksi 19.8 terpenuhi.
4. Implementasi di luar task yang diturunkan. Wewenang tulis, migration, dan deployment tetap terpisah per task.

### 19.11 Handoff

```yaml
gate_revision: "1.10"
blueprint_id: RWI-BP-001
assessed_scope: >
  Finishing Rawat Inap setelah grill-me 2026-10-02 dan approval desain revision 8:
  INP-S32 dinilai penuh; dampak RWI-DEC-206..221 pada INP-S24..S37; bukti implementasi baru FE f74758af5
not_reassessed: [INP-S23, INP-S31]
evidence:
  decision_log: 00-interview-decisions.md revision 32, sha256 2102ed1da2a4748157bb3e6960212905e20a704b2a265c6abda4a97ffed43b25
  billing_decision: billing-kasir BKC-DEC-118 (approved 2026-09-24)
  prd: 05-prd-to-mvp-finishing-rawat-inap.md v0.4, sha256 0f658455b98658974865fda4fb917b2ccdb9a9bce5db5723f9b919327cfabe1a (sebagian tertinggal, G-28)
  approved_design: blueprint revision 8 — integrasi-billing 1.1.0, keperawatan 0.6.0, dokter-rawat-inap 0.7.0, episode-rawat-inap 0.10.0, 02-module-map revision 4 (RWI-DEC-221)
  capability_map: 01-existing-capability-map.md revision 1.6 bagian 19, sha256 2f78b74e37c983ffa81cbd510e129906243634536c04df105811c827930eccd2 (pemicu 19.9 tidak lengkap, IMP-RWF-04)
  facts: [RWI-FACT-059]
  source_reads_this_gate:
    - BillingSettlementDtos.cs:6-8 (pembayaran per invoice)
    - supporting-nutrition-form.jsx:66-71, supporting-blood-bank-form.jsx:80-95, supporting-hemodialysis-form.jsx:86 (requestingDoctorId = admissionDoctorId)
    - supporting-blood-bank-form.jsx:83-90 (bloodComponentId tetap)
    - supporting-rehab-form.jsx:20-60 (procedureId palsu)
    - nursing-ancillary-section.jsx:72-75 (ruang kerja perawat tidak berubah)
  backend_sha: bf5c6bde (HEAD; sejak 8d96a978 hanya dokumen)
  frontend_sha: f74758af5 (HEAD; form Penunjang Medis ruang kerja dokter)
readiness:
  READY_FOR_DOMAIN_DESIGN: [INP-S23, INP-S24, INP-S25, INP-S26, INP-S27, INP-S28, INP-S29, INP-S30, INP-S31, INP-S32, INP-S33, INP-S34, INP-S35, INP-S36, INP-S37]
  PARTIALLY_READY: []
closed_decisions:
  - DEC-INP-018: RWI-DEC-207
closed_implementation_gates:
  - RWI-OQ-108: RWI-DEC-210
  - "RWI-OQ-114 butir a dan c": RWI-DEC-208
  - RWI-OQ-115: RWI-DEC-209
open_decisions: []
gaps_closed: [G-05, G-17, G-18, G-26]
gaps_confirmed_by_contract_approval: [G-02, G-13, G-14, G-15, G-16, G-19, G-20, "G-22 a", "G-22 b", G-23, G-24, G-25]
new_gaps:
  - G-27: penyelesaian multi-invoice BKC-DEC-118 / BILL-INT-007 belum ada (NON_BLOCKING; dependency RWI-AC-330)
  - G-28: PRD Finishing v0.4 tertinggal dari decision log (NON_BLOCKING; usulan v0.5)
implementation_findings: [IMP-RWF-01, IMP-RWF-02, IMP-RWF-03, IMP-RWF-04]
release_gate: [IMP-RWF-01, IMP-RWF-02, IMP-RWF-03]
production_gate: [G-21, "G-22 c", "konfirmasi klinis RWI-DEC-199", "isi butir persiapan bedah", "nilai jenis anestesi rencana", G-11, RWI-RULE-037, FIN-UNK-05, FIN-UNK-06]
domain_architecture: opsional; tidak diperlukan — kepemilikan data sudah ditetapkan dan desain disetujui
next_skill:
  - plan-module-delivery untuk 15 slice Finishing (design-business-module sudah menyerap dan disetujui RWI-DEC-221)
  - trace-existing-capabilities impact scan terfokus ruang kerja dokter Penunjang Medis (IMP-RWF-04), paralel
superseded_in_this_document: kesiapan INP-S32 dan status DEC-INP-018 pada 18.6, 18.7, 18.10, 18.12 (oleh 19.3, 19.7, 19.9); rujukan BKC-DEC-117 pada 18.6 dan 18.7 (RWI-FACT-059)
```

### 19.12 Koreksi sesudah gate — bukti baru saat perencanaan delivery, 2 Oktober 2026

`plan-module-delivery` menemukan tiga bukti yang belum terbaca saat 19.1 s.d. 19.11 ditulis. Kesiapan slice Finishing **tidak berubah** (15 dari 15 `READY_FOR_DOMAIN_DESIGN`), tetapi dua pernyataan di atas dibetulkan.

**1. Keputusan KK-3 pada rencana kerja Penunjang Medis dokter.** `dokter-rawat-inap/roadmap/rencana-kerja/penunjang-medis/penunjang-medis.md` rev 2.0, disetujui pengguna 1 Oktober 2026, memuat KK-3 pilihan B: *"Buka formulir pemesanan terstruktur [Gizi, Bank Darah, Rehab Medik] mengikuti alur bisnis V1 dan hubungkan ke backend masing-masing."* Task `FE-RWI-145` dan `FE-RWI-146` mengerjakannya.

| Bagian KK-3 | Dibandingkan dengan | Penilaian |
|---|---|---|
| Gizi dan Bank Darah dibuka | `RWI-DEC-171`, `RWI-DEC-188`; kontrak `dokter-rawat-inap` `0.7.0` yang disetujui 2 Oktober (`RWI-DEC-221`) | **Sejalan.** Keduanya membuka formulir. Jalur lewat adapter Rawat Inap ditetapkan kontrak yang disetujui belakangan, sehingga IMP-RWF-01 dan 02 tetap berlaku sebagai cacat implementasi |
| Rehab Medik dibuka dan dihubungkan ke backend | `RWI-DEC-108`, `FR-RWF-035`, dan `dokter-rawat-inap` `04-prd-to-mvp.md` 23.8 (Rehab Medik ditunda sebagai *placeholder*), yang ikut disetujui ulang lewat `RWI-DEC-221` | **`CONFLICT`.** Dua persetujuan pemilik berselisih satu hari dan saling bertentangan. Urutan waktu memenangkan *placeholder*, tetapi persetujuan KK-3 yang eksplisit menunjukkan pemilik mungkin memang menghendaki Rehab dibuka. Agent tidak memilih atas nama pemilik |

Karena itu pernyataan 19.5 IMP-RWF-03 ("tidak memerlukan keputusan") dan 19.7 ("tidak ada Decision ID baru") **dibetulkan** menjadi:

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status | Dampak implementasi / domain |
|---|---|---|---|---|---|---|---|---|
| **`DEC-INP-019`** | Apakah Rehab Medik dari ruang kerja dokter **dibuka sekarang** sebagai pesanan (misalnya order tindakan dari katalog prosedur fisioterapi yang ada di master), atau **tetap *placeholder*** sampai modul Rehab Medik ada? | Pesanan Rehab Medik dari bangsal — kemampuan yang ditunda, **bukan** salah satu slice Finishing | KK-3 B (1 Oktober) vs `RWI-DEC-108`, `FR-RWF-035`, dan approval `RWI-DEC-221` (2 Oktober). Source `FE-RWI-146` memakai tujuh `procedureId` palsu, sehingga apa pun jawabannya kodenya wajib diubah | Tetap *placeholder* mengikuti keputusan yang disetujui paling akhir | Lingkup layanan, konsekuensi billing (order tindakan menghasilkan tagihan), integritas data katalog | Muhammad Hamzah | **`CLOSED`** — disetujui 2 Oktober 2026 lewat `RWI-DEC-222` (tetap *placeholder*) | Task perbaikan Rehab (`FE-RWI-179`) **unblocked** (Bebas Blokir). Seluruh slice Finishing dan perbaikan Gizi, Bank Darah, Lab, serta Radiologi **tidak** tertahan |

**2. IMP-RWF-05 — form Laboratorium dan Radiologi ruang kerja dokter (`FE-RWI-144`).** Bila katalog API berisi, setiap pemeriksaan diberi harga tetap **120.000** (`supporting-laboratory-form.jsx:81-96`). Bila katalog kosong atau gagal dimuat, form memakai katalog contoh `DEFAULT_LAB_EXAMS` dan `DEFAULT_RAD_PROCEDURES` berisi ID dan harga buatan (`supporting-laboratory-form.jsx:27-40`, `supporting-radiology-form.jsx:21-25`, `:103`). Ini bertentangan dengan `RWI-DEC-108` (dilarang data contoh) dan `RWI-DEC-218` (harga berupa perkiraan dari resolver penjamin, atau "tarif belum tersedia"). Statusnya **gerbang rilis**, sama seperti IMP-RWF-01 s.d. 03.

**3. Integritas ID task.** `BE-RWI-128` dipakai dua task berbeda: di `dokter-rawat-inap/roadmap/backend-roadmap-v2.md` dan di `integrasi-billing/roadmap/backend-roadmap.md`. Deret ID juga sudah terpakai sampai `BE-RWI-145` dan `FE-RWI-165` oleh rencana kerja paralel, jauh melewati catatan manifest (`BE-RWI-135`, `FE-RWI-101`). Ini bukan gap requirement; dicatat untuk pemilik peta modul (`02-module-map.md` 1.3).

**Handoff yang dibetulkan:** `open_decisions: []` (seluruh keputusan terbuka termasuk `DEC-INP-019` telah tertutup oleh `RWI-DEC-222`); `release_gate: [IMP-RWF-01, IMP-RWF-02, IMP-RWF-03, IMP-RWF-05]`; seluruh task Finishing siap dieksekusi per gelombang setelah approval roadmap.

---

## 20. Evaluasi Gerbang Kelengkapan Requirement — Workspace PPRI (`PRD-RWI-ADMISI-001`) — revision `1.11`

### 20.1 Scope, identitas slice, dan bukti acuan

**Yang dinilai:** kemampuan Workspace PPRI (Ruang Kerja Admisi) pada `PRD-RWI-ADMISI-001` v`0.2`, sesudah Amendment Pass (`RWI-DEC-225` s.d. `RWI-DEC-248`) dan Closure Pass (`RWI-DEC-249` s.d. `RWI-DEC-262`). Tiga kemampuan **tidak** dinilai: `CAP-RWA-04` Asesmen Edukasi (dibatalkan `RWI-DEC-226`), serta `CAP-RWA-10` Estimasi Biaya Rinci dan `CAP-RWA-12` MP Benefit (tetap ditunda, `RWI-OQ-119`, `RWI-OQ-120`).

Kemampuan dikelompokkan menjadi sebelas slice baru `INP-S38` s.d. `INP-S48`. Pengelompokan mengikuti kepemilikan data dan siklus hidup dokumen, bukan menu. Seluruh slice dirancang di sub-modul `episode-rawat-inap` (`RWI-DEC-227`), dan seluruh tabel dokumen admisi milik `InPatientManagement` (`RWI-DEC-228`).

| Slice | Nama | Kemampuan PRD | Keputusan utama |
|---|---|---|---|
| `INP-S38` | Ruang kerja PPRI, header, dan kelengkapan | `CAP-RWA-01` | `RWI-DEC-234`, `245`, `246`, `250`, `256`, `257` |
| `INP-S39` | Fondasi dokumen admisi: siklus, tanda tangan kertas, atestasi, cetak, hak akses | `CAP-RWA-14` (kertas), `15`, `16`, `17` | `RWI-DEC-228`, `229`, `230`, `237`, `238`, `239`, `240`, `247`, `257`, `258` |
| `INP-S40` | Serah Terima Pasien Baru | `CAP-RWA-03` | `RWI-DEC-239`, `241`, `255`, `262` |
| `INP-S41` | Gelang dan label identitas | `CAP-RWA-05` | `RWI-DEC-243`, `253`, `259` |
| `INP-S42` | Data Dasar Rawat Inap (IPD) | `CAP-RWA-07` | `RWI-DEC-244`, `254`, `258` |
| `INP-S43` | Hak pasien: Permintaan Privasi dan Nilai Kepercayaan | `CAP-RWA-06`, `CAP-RWA-13` | `RWI-DEC-228`, `238`, `242` |
| `INP-S44` | Selisih Biaya | `CAP-RWA-11` | `RWI-DEC-234`, `256` |
| `INP-S45` | Pelunasan Deposit | `CAP-RWA-08` | `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261` |
| `INP-S46` | Estimasi Biaya Rekap | `CAP-RWA-09` | `RWI-DEC-232`, `250`, `258` |
| `INP-S47` | General Consent cetak saja selama *fail-closed* | `CAP-RWA-02` (bagian cetak) | `RWI-DEC-230`, `233`, `246`, `251`, `252` |
| `INP-S48` | Tanda tangan digital pasien/keluarga | `CAP-RWA-14` (bagian digital) | `RWI-DEC-230`, `235` (`draft`) |

**Hubungan dengan slice lama.** Penyimpanan General Consent (`RWI-DEC-236`, `draft`) **tidak** dijadikan slice baru. Kemampuannya sama dengan slice lama `INP-S10` "Persetujuan umum rawat inap", yang sejak revision `1.0` berstatus `BUSINESS_DECISION_REQUIRED` karena `DEC-INP-003`. Gate ini hanya memperluas cakupan `DEC-INP-003` (20.6).

**Bukti yang dipakai:**

| Jenis bukti | Sumber | Revision / hash |
|---|---|---|
| Requirement eksplisit pemilik | `00-interview-decisions.md`, `RWI-DEC-225` s.d. `RWI-DEC-262`, `RWI-AC-343` s.d. `RWI-AC-383` | Revision `36`, SHA-256 `618f684b707fdb60f88d194623a265844faaf61fb02061e11eb61a4104f746df` |
| Brief pemilik 7 Oktober 2026 | PRD bagian 1 baris "Brief UI dari pemilik", dicatat ulang pada sub-bagian scope `RWI-DEC-225`: tombol sesudah Workspace Dokter, template Workspace Keperawatan, isian form tetap mengikuti V1, V1 hanya acuan bisnis | Sama dengan baris di atas |
| Jawaban Billing lewat pemilik | `RWI-DEC-231`, `232`, `248`, `256`, `260`, `261` (Yasmina, disampaikan Muhammad Hamzah) | Decision log revision `36` |
| Dokumen produk | `docs/Modul-RS/Rawat-Inap/06-prd-to-mvp-workspace-admisi-rawat-inap.md` (`PRD-RWI-ADMISI-001` v`0.2`, `DRAFT`) | SHA-256 `f1fd336f562d6936629d50f8f849fa7767e2dbea2b24678da9596de60dcc1192` |
| Implementasi V2 terverifikasi | `01-existing-capability-map.md` revision `1.7` bagian 20 (`PPRI-CAP-01` s.d. `PPRI-CAP-54`), ditambah `RWI-FACT-065` dan `RWI-FACT-066` | Backend `671191eb` dan frontend `27889662a`; SHA-256 `e8e454cbaed3f0bfd39793bbb4b82860ed9eb921b9154c24554b2bf06fb6e3da` |
| HEAD saat gate dijalankan | Backend `fdf85a07` (enam berkas harga penjamin yang sudah dinilai `RWI-FACT-065`); frontend `27889662a`, dengan perubahan lokal belum di-commit yang hanya menyentuh menu penunjang dan pemesanan bedah Workspace Keperawatan | Bagian 20 capability map tetap `CURRENT` |
| Legacy V1 | PRD Lampiran A (isian form V1) dan bagian 3.4 (kelemahan V1) | Backend V1 `4be1499cc`, frontend V1 `86408f245` |
| Baseline rujukan | `indonesia-hospital-domain-reference` `references/inpatient.md`: `ID-INP-CAP-001` (konteks dan diagnosis admisi), `ID-INP-CAP-002` (verifikasi pra-admisi: persetujuan, penjamin), `ID-INP-CAP-016` (serah terima), `ID-INP-REG-001` (rekam medis elektronik), bagian 10 (koreksi), 12 (selisih biaya), 13 (identitas pasien), 14 (audit) | `REFERENCE_ONLY`, `Reference coverage: PARTIAL` |

**Wewenang bukti yang diterapkan.** Untuk apa yang **seharusnya dibangun**, decision log menang atas PRD v`0.2` yang masih `DRAFT`. PRD sendiri menyatakan hal itu (bagian 1), dan beberapa isinya sudah tertinggal (G-50). Isian form V1 berstatus `CONFIRMED` karena pemilik memerintahkan isian tetap mengikuti V1, sedangkan perbaikan atas V1 yang hanya tertulis di PRD berstatus `PROPOSED` sampai ada keputusan. Untuk apa yang **sudah ada**, capability map bagian 20 yang dipakai. Baseline rujukan hanya dipakai untuk mendeteksi gap.

### 20.2 Ringkasan untuk pembaca umum

Hampir seluruh aturan Workspace PPRI sudah diputuskan pemilik dalam dua pass wawancara hari ini. Gate ini hanya menemukan **dua hal yang masih menahan**, dan keduanya hanya menahan bagian kecil:

1. **Tarif visit dokter dan catatan aturan biaya bedah pada Estimasi Biaya.** Billing belum punya konsep tarif visit dokter rawat inap, dan belum punya pengaturan catatan cito, lembur, *standby*, maupun anestesi. Keputusannya milik Yasmina (`DEC-INP-020`). Bagian Estimasi lain, seperti tarif tindakan, kamar, baris manual, dan catatan biaya admin, boleh dirancang.
2. **Pemilik privasi dan hukum belum ditunjuk.** Karena itu General Consent belum boleh disimpan dan tanda tangan digital di tablet belum boleh dibangun (`DEC-INP-003`, sudah ada sejak gate pertama). Cetakan General Consent tanpa simpan tetap boleh dirancang.

Sembilan dari sebelas slice baru siap dirancang penuh. Beberapa usulan kecil tetap tercatat sebagai gap tidak memblokir, misalnya alasan cetak ulang gelang dan batas tiga baris kerabat pada Permintaan Privasi. Ada pula tiga gerbang produksi, misalnya verifikasi aturan gelang oleh tim keselamatan pasien, yang tidak menahan desain.

### 20.3 Matriks 18 dimensi kelengkapan

Kode isi sel: **C** = `CONFIRMED`; **P** = `PROPOSED`; **M** = `MISSING`; **X** = `CONFLICT`; **–** = tidak material untuk slice itu, dengan alasan di 20.4. Angka dalam kurung merujuk gap pada 20.5.

| No | Dimensi | `S38` | `S39` | `S40` | `S41` | `S42` | `S43` | `S44` | `S45` | `S46` | `S47` | `S48` |
|---:|---|---|---|---|---|---|---|---|---|---|---|---|
| 01 | Tujuan | C | C | C | C | C | C | C | C | C | C | C |
| 02 | Aktor | C | C | C | C | C | C | C | C | C | C | C |
| 03 | Pemicu / prasyarat | C, P (G-30) | C | C | C | C | C | C | C | C | C | P (G-49) |
| 04 | Alur utama | C | C | C | C | C | C | C | C | C | C | P (G-49) |
| 05 | Alur alternatif / exception | C | C | C | C | C | C | C, P (G-43) | C, P (G-45) | C | C | M (G-49) |
| 06 | Data minimum | C | C | C | C, P (G-36) | C, P (G-39) | C, P (G-40) | C, P (G-44) | C | C, M (G-47), P (G-48) | C | P (G-49) |
| 07 | Aturan bisnis / validation | C | C | C | C | C | C | C | C | C, M (G-47) | C | P (G-49) |
| 08 | Status / perubahan status | C | C | C | – | – | C | C | C | C | – | P (G-49) |
| 09 | Peran / authorization | C, P (G-32) | C, P (G-32) | C | C | C | C, P (G-41) | C | C | C | C | M (G-49) |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | C | C | C | C |
| 11 | Integrasi internal / eksternal | C | C | – | C, M (G-37) | – | – | – | C | C, M (G-47) | – | P (G-49) |
| 12 | Hasil akhir | C | C | C | C | C | C | C | C | C | C | P (G-49) |
| 13 | Pembatalan / koreksi | – | C | C | C, P (G-33) | – | C | C | C | C | – | P (G-49) |
| 14 | Audit / histori | – | C, M (G-35) | C | C | C | C | C | C | C | C | M (G-49) |
| 15 | Notifikasi | C | P (G-34) | P (G-34) | – | – | M (G-42) | – | C, P (G-46) | – | – | – |
| 16 | Dampak billing / charge | – | – | – | – | – | – | C | C | C, M (G-47) | – | – |
| 17 | Dampak keselamatan klinis | – | – | – | C, P (G-38) | – | C, M (G-42) | – | – | – | – | – |
| 18 | Pelaporan / traceability | C | C, M (G-35) | C | C | C | C | C | C | C | – | M (G-35) |

Gap lintas slice G-50 (PRD tertinggal) dan G-51 (capability map belum memuat dua fakta baru) tidak dimasukkan ke sel, karena keduanya menyangkut dokumen bukti, bukan dimensi requirement.

### 20.4 Temuan per slice

#### `INP-S38` — Ruang kerja PPRI, header, dan kelengkapan

Tombol "Workspace PPRI" tepat sesudah Workspace Dokter (`RWI-DEC-245`, brief pemilik), template empat wilayah Workspace Keperawatan (brief pemilik; `PPRI-CAP-03` siap pakai), header yang disusun server tanpa hak modul lain (`RWI-DEC-257`), status deposit hanya bagi pemegang `ViewAmount` (`RWI-DEC-258`), aturan dokumen wajib (`RWI-DEC-234`, `250`, `256`), peringatan di Detail Episode (`RWI-DEC-234`), dan hanya-baca pada episode `Closed`/`Cancelled` (`RWI-DEC-240` butir 7): `CONFIRMED`. Dimensi 13 dan 14 tidak material, karena ruang kerja hanya merangkum; pembatalan dan jejak audit melekat pada dokumen di `INP-S39`. Dimensi 16 tidak material karena ruang kerja tidak membuat tagihan. Dimensi 17 tidak material karena alergi pada header hanya dibaca dari sumber yang sudah ada. **Gap:** G-30, G-31, G-32, semuanya tidak memblokir.

#### `INP-S39` — Fondasi dokumen admisi

Siklus `Draft` → `AwaitingSignature` → `Completed`, dengan `Superseded` dan `Cancelled`, transisi terlarang, koreksi lewat versi, pembatalan supervisor dengan alasan minimal 10 karakter, tanpa hapus permanen, dan satu dokumen aktif per jenis per episode (`RWI-DEC-240`): `CONFIRMED`. Penanda tangan kertas (`RWI-DEC-230`), atestasi petugas (`RWI-DEC-237`), Kepala Ruangan khusus `SignAsHeadNurse` (`RWI-DEC-238`), satu akun satu slot (`RWI-DEC-239`), kop dan kode formulir dari pengaturan (`RWI-DEC-247`), kepemilikan `InPatientManagement` tanpa mesin keutuhan Rekam Medis (`RWI-DEC-228`, `229`), dan jejak setiap buat, ubah, kunci, tanda tangan, batal, serta cetak (`RWI-DEC-229`) juga `CONFIRMED`. Dimensi 16 dan 17 tidak material: fondasi tidak membuat tagihan, sedangkan salinan beku identitas pasien sudah diputuskan (`RWI-DEC-228`). **Gap:** G-32, G-33, G-34, G-35, tidak memblokir.

**Catatan untuk desain (bukan gap bisnis).** Pola teknis yang sudah ada dan cocok ditiru dicatat di capability map `PPRI-CAP-42` s.d. `45`: registry `Inp` `ACTIVE`, konkurensi `RowVersion` dengan indeks unik bersaring, `Idempotency-Key`, dan salinan beku seperti `CliTransferHandover.SnapshotJson`.

#### `INP-S40` — Serah Terima Pasien Baru

Lima belas butir dan tiga sub-butir V1 dari master (brief pemilik), setiap butir wajib dipilih, butir Belum wajib berketerangan, nama butir dibekukan, saran sistem untuk butir 1, 4, 7, 9, dan 13 (`RWI-DEC-241`, `262`), tiga penanda tangan berbeda (`RWI-DEC-239`), dan Perawat penerima hanya setelah pasien menempati bed (`RWI-DEC-255`): `CONFIRMED`. Perluasan master butir administrasi beserta saringan jenis pada penutupan episode juga `CONFIRMED` (`RWI-DEC-241` butir 3), dengan gerbang MasterData `RWI-DEC-193`. Dimensi 11 tidak material, karena master butir adalah dependency internal dan tidak ada sistem luar. Dimensi 16 tidak material. Dimensi 17 tidak material, karena checklist ini administratif. Serah terima klinis tetap milik serah terima IGD (`INP-S09`) dan transfer (`INP-S31`); baseline `ID-INP-CAP-016` dipakai hanya untuk memastikan pihak penyerah, pihak penerima, dan konfirmasi penerimaan tercakup, dan ketiganya tercakup. **Gap:** G-34, tidak memblokir.

**Catatan untuk desain (bukan gap bisnis).** `MstInpatientClearanceItem` memakai `SortOrder` presentasi generik, pola legacy yang dilarang untuk kode baru (`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md:48`). Perluasan tabel ini berstatus `TOUCHED LEGACY`, sehingga desain perlu menetapkan apakah urutan butir serah terima memakai kolom yang ada atau field semantik baru.

#### `INP-S41` — Gelang dan label identitas

Jenis gelang dewasa atau bayi dengan batas umur yang dapat diatur, aturan sapaan dari umur, jenis kelamin, dan status nikah (`RWI-DEC-243`), "No. Kartu" hanya dari `CardNumberSnapshot` (`RWI-DEC-253`), QR berisi No. RM yang dirender ulang setiap cetak (`RWI-DEC-259`), dan log cetak (`RWI-DEC-240` butir 7, `RWI-DEC-228`): `CONFIRMED`. Baseline bagian 13 "kepastian identitas pasien" tercakup oleh aturan ini. Dimensi 08 tidak material karena gelang hanya punya log cetak, bukan status dokumen. Dimensi 15 dan 16 tidak material. **Gap:** G-33, G-36, G-37, G-38, tidak memblokir. G-38 menjadi gerbang produksi.

#### `INP-S42` — Data Dasar Rawat Inap (IPD)

Tata letak dan isian V1 (brief pemilik), sumber tiap isian (capability map `PPRI-CAP-24` s.d. `28`), isian tanpa sumber berupa garis kosong (`RWI-DEC-244`), diagnosis masuk dan dokter perujuk dari Surat Pengantar Rawat Inap atau dokter perujuk luar (`RWI-DEC-254`, didukung baseline `ID-INP-CAP-001`), dan tarif kamar hanya bagi pemegang `ViewAmount` (`RWI-DEC-258`): `CONFIRMED`. Dimensi 08 dan 13 tidak material, karena IPD hanya dicetak dan dicatat log cetaknya. Dimensi 11, 15, 16, dan 17 tidak material: tidak ada sistem luar, tidak ada pemberitahuan, tarif kamar hanya ditampilkan tanpa ditagih, dan nilai kepercayaan yang dicetak berasal dari `INP-S43`. **Gap:** G-39, tidak memblokir.

#### `INP-S43` — Hak pasien: Permintaan Privasi dan Nilai Kepercayaan

Isian V1 kedua formulir (brief pemilik), penyimpanan per baris (`RWI-DEC-228` dan `RWI-DEC-242`), Nilai Kepercayaan per episode dengan isian dari episode lalu dan 1–5 butir (`RWI-DEC-242`), Kepala Ruangan pada Permintaan Privasi (`RWI-DEC-238`), siklus dokumen (`RWI-DEC-240`), dan endpoint ringkasan hak pasien untuk modul lain (`RWI-DEC-228`): `CONFIRMED`. Dimensi 11 dan 16 tidak material. **Gap:** G-40 dan G-41 tidak memblokir. G-42 **tidak memblokir desain slice ini**, tetapi menjadi gerbang produksi keselamatan klinis.

#### `INP-S44` — Selisih Biaya

Berlaku hanya untuk penjamin utama asuransi atau perusahaan (`RWI-DEC-234`, didukung capability map `PPRI-CAP-34`), tetap wajib walaupun penjamin melarang selisih dibebankan ke pasien (`RWI-DEC-256`), isian deklarer V1 (brief pemilik), dan data pasien yang hanya dibaca dari pemiliknya lalu dibekukan (`RWI-DEC-228`): `CONFIRMED`. Baseline bagian 12 "selisih biaya/urun bayar" tercakup sebagai pernyataan kesediaan; penagihannya tetap milik Billing (`RWI-DEC-256`). Dimensi 11, 15, dan 17 tidak material. **Gap:** G-43, G-44, tidak memblokir. Penyimpanan nomor identitas deklarer termasuk gerbang produksi privasi (G-35).

#### `INP-S45` — Pelunasan Deposit

Angka dari `PolicyShortfallAmount` Billing, batas jatuh tempo dari interval kebijakan, pemotongan ke batas, dan hari kerja Senin–Jumat (`RWI-DEC-231`, `248`, `261`); angka dibekukan saat `Completed`; data wali dari relasi atau kontak darurat yang dipilih (`RWI-DEC-252`); cetak hanya bagi pemegang `ViewAmount` (`RWI-DEC-258`); peringatan jatuh tempo terlewati dari tanggal surat (`RWI-DEC-260`): `CONFIRMED`. Dimensi 17 tidak material. **Gap:** G-45, G-46, tidak memblokir.

#### `INP-S46` — Estimasi Biaya Rekap

Isian dan cetakan V1 (brief pemilik), tarif tindakan dan kamar dari layanan harga Billing yang dihitung server untuk pemegang `ViewAmount` (`RWI-DEC-232`, `258`), baris "Tarif belum tersedia" yang menahan penguncian, catatan biaya admin dari kebijakan Billing, dan kewajiban bila ada kasus OK yang tidak batal atau ditolak (`RWI-DEC-250`): `CONFIRMED`. Dimensi 17 tidak material. **Gap:** G-47 **memblokir** baris visit dokter dan catatan aturan biaya bedah saja. G-48 tidak memblokir.

#### `INP-S47` — General Consent cetak saja selama *fail-closed*

Dua cetakan tanpa simpan, yaitu Surat Persetujuan 12 butir dan Formulir GC V1 (`RWI-DEC-233`), tipe kamar dari isolasi episode atau penanda kamar dan bed (`RWI-DEC-251`), isi otomatis hanya dari relasi terstruktur (`RWI-DEC-252`), dan tombol Cetak Persetujuan yang diarahkan ke Workspace PPRI (`RWI-DEC-246`): `CONFIRMED`. Kop cetakan 12 butir yang menanam identitas rumah sakit harus diperbaiki agar sesuai `RWI-DEC-247` (capability map `PPRI-CAP-09`, status `Repair`); ini konsekuensi keputusan, bukan gap. Dimensi 08, 13, dan 18 tidak material, karena cetakan ini sengaja tidak meninggalkan rekaman apa pun. Dimensi 14 `CONFIRMED` dalam arti tidak ada jejak cetak, sesuai keputusan `RWI-DEC-233`. **Gap:** tidak ada.

#### `INP-S48` — Tanda tangan digital pasien/keluarga

Sisi produk sudah dipilih (`RWI-DEC-235`: digital sebagai mode utama dan kertas sebagai cadangan, mode diatur rumah sakit, berkas goresan ber-hash SHA-256), tetapi berstatus `draft`. Keabsahan hukumnya menunggu pemilik privasi/hukum (`RWI-DEC-230`, `RWI-OQ-116`). Seluruh dimensi material bergantung pada keputusan itu. **Gap:** G-49 **memblokir** seluruh slice, dan G-35 berlaku saat slice ini dibuka.

### 20.5 Daftar gap dan dampaknya

| Gap | Slice | Pernyataan | Status bukti | Dampak | Penjelasan dan usulan |
|---|---|---|---|---|---|
| G-30 | `S38` | Apakah Workspace PPRI terbuka untuk episode `Draft`, yaitu admisi yang belum dikonfirmasi | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD 5.1 menetapkan titik mulai `Admitted`/`DischargePending`, sedangkan `RWI-DEC-240` hanya mengatur `Closed`/`Cancelled`. Usulan: ruang kerja hanya terbuka sesudah admisi dikonfirmasi, dan episode `Draft` menampilkan "Admisi belum dikonfirmasi". Cetak persetujuan pada langkah 8 alur admisi tetap seperti hari ini |
| G-31 | `S38` | Judul dan subjudul halaman (`RWI-OQ-121`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan agent "Ruang Kerja PPRI" dan "Penerimaan Pasien Rawat Inap" dipakai sebagai `draft` sampai dijawab |
| G-32 | `S38`, `S39` | Pemberian aksi `InpatientAdmissionDocument` (`Read`, `Create`, `Update`, `Sign`, `SignAsCro`, `SignAsNurse`, `SignAsHeadNurse`, `Print`, `Cancel`, `ViewAmount`) per peran | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Aturannya `CONFIRMED` (hak menentukan tindakan, bukan nama peran). Pembagian per peran pada PRD bagian 14 hanyalah bawaan usulan, dan rumah sakit mengaturnya lewat Akses Role. Ketersediaan peran CRO dan supervisor di lingkungan target adalah gerbang implementasi (`RWI-OQ-124`) |
| G-33 | `S39`, `S41` | Alasan cetak ulang pada episode aktif (rusak, hilang, data berubah, lainnya) dan penanda "Cetak ulang ke-*n*" | `PROPOSED` | `NON_BLOCKING_STANDARD` | `RWI-DEC-240` butir 7 hanya mewajibkan alasan pada episode tertutup. PRD `FR-RWA-053` dan `FR-RWA-127` mengusulkan alasan untuk setiap cetak ulang. Usulan: ikuti PRD, karena hanya menambah isian log cetak |
| G-34 | `S39`, `S40` | Tanda tangan pihak lain tampil paling lambat 30 detik tanpa memuat ulang | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD `FR-RWA-035`. Usulan: penyegaran berkala memakai pola `use-inpatient-billing-status.js` (capability map `PPRI-CAP-52`). Hub SignalR khusus tidak dibutuhkan untuk MVP |
| G-35 | `S39`, `S44`, `S48` | Masa simpan dokumen admisi, serta tinjauan privasi atas data pribadi keluarga (nomor identitas, alamat, telepon) | `MISSING` | `NON_BLOCKING_STANDARD` untuk desain; **gerbang produksi** | Tanpa hapus permanen sudah `CONFIRMED`, sehingga masa simpan tidak mengubah model data. Tinjauan privasi diwajibkan `RWI-DEC-230` butir 5 dan menunggu `RWI-OQ-116`. Baseline `ID-INP-REG-001` dan PRD bagian 16 mendukung topik ini sebagai `REFERENCE_ONLY` |
| G-36 | `S41` | Kode singkat rumah sakit pada label diambil dari `MstHospitalSite.SiteCode` | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Inferensi capability map `PPRI-CAP-20`. Usulan: `SiteCode`, atau isian pengaturan kecil bila isinya tidak cocok dicetak |
| G-37 | `S41` | Ukuran kertas gelang dan label serta jenis printernya | `MISSING` | `CONFIGURABLE_DEFAULT` | V1 mencetak lewat peramban. Usulan: ukuran diatur lewat pengaturan cetak dan diuji dengan printer rumah sakit saat UAT. Tidak mengubah model data |
| G-38 | `S41` | Verifikasi tim keselamatan pasien atas batas umur gelang bayi, aturan sapaan, dan isi QR | `PROPOSED` | `NON_BLOCKING_STANDARD` untuk desain; **gerbang produksi** | Keputusan produknya `approved` (`RWI-DEC-243`, `259`). Konfirmasi keselamatan pasien sudah dicatat pada Gate Sebelum Produksi decision log |
| G-39 | `S42` | Tarif kamar yang dicetak bila ada lebih dari satu tarif kamar aktif untuk satu kelas | `PROPOSED` | `NON_BLOCKING_STANDARD` | Risiko dari capability map `PPRI-CAP-28`. Usulan: bila lebih dari satu, isian ditulis "lihat kasir" dan tidak memilih sendiri |
| G-40 | `S43` | Batas jumlah baris kerabat dan permintaan khusus | `PROPOSED` | `NON_BLOCKING_STANDARD` | V1 menyediakan tiga baris. Usulan: maksimal tiga baris masing-masing, mengikuti V1. Penyimpanan per baris membuat batas ini mudah diubah |
| G-41 | `S43` | Hak baca endpoint ringkasan hak pasien bagi modul lain | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD 13.1 mengusulkan `InpatientEpisode : Read`, karena isinya dibutuhkan klinisi untuk keselamatan. Konsumen di luar Workspace PPRI baru datang pada amandemen keperawatan dan dokter, sehingga pilihan ini dapat ditinjau saat itu. Masuk tinjauan privasi G-35 |
| G-42 | `S43` | Nilai kepercayaan yang menolak tindakan tertentu, misalnya transfusi darah, belum tampil di Workspace Keperawatan dan Workspace Dokter pada MVP | `MISSING` | `NON_BLOCKING_STANDARD` untuk desain `S43`; **gerbang produksi keselamatan klinis** | Peringatan di header klinis ditunda (PRD bagian 8), dan isian `SD_BELIEFS` perawat terpisah (`RWI-OQ-123`). Pada MVP, informasi itu hanya tampil di Workspace PPRI dan IPD. Pemilik clinical governance harus menerima risiko ini, atau meminta amandemen keperawatan dan dokter sebelum produksi. Baseline bagian 13 mendukung topik ini sebagai `REFERENCE_ONLY` |
| G-43 | `S44` | Penjamin utama berubah setelah Selisih Biaya `Completed`, misalnya dari asuransi menjadi tunai | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: dokumen tetap tersimpan apa adanya, dan kelengkapan dihitung ulang dari penjamin terkini. Bila berubah dari tunai menjadi asuransi, Selisih Biaya menjadi wajib sejak saat itu |
| G-44 | `S44` | Isian deklarer yang wajib diisi | `PROPOSED` | `NON_BLOCKING_STANDARD` | PRD Lampiran A.9 mengusulkan Nama, Alamat, Tipe ID, No. ID, dan Tanggal. Usulan: ikuti PRD |
| G-45 | `S45` | Billing tidak dapat dihubungi saat Pelunasan Deposit dibuka | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan PRD bagian 9: angka tidak ditampilkan, simpan dinonaktifkan, dan ada tombol Coba Lagi. Sejalan dengan prinsip "data tidak tersedia, bukan nol" yang sudah dipakai daftar pantau deposit |
| G-46 | `S45` | Pemberitahuan jatuh tempo kepada kasir di modul Billing | `PROPOSED` | `NON_BLOCKING_STANDARD` | Ditunda menurut PRD bagian 8. Pada MVP peringatan hanya tampil di Workspace PPRI dan Detail Episode (`RWI-DEC-260`) |
| G-47 | `S46` | Tarif visit dokter rawat inap, serta sumber catatan cito +25%, lebih dari 4 jam +25% per jam, *standby* 20%, dan anestesi 50% | `MISSING` | **`BLOCKING`** — hanya baris visit dokter dan catatan aturan biaya bedah | `RWI-DEC-232` menetapkan sumbernya Billing, tetapi capability map 20.5 membuktikan konsep tarif visit dan keempat aturan itu tidak ada. Pilihan Billing mengubah kontrak integrasi dan angka yang dijelaskan kepada pasien. **`DEC-INP-020`** (`RWI-OQ-122`) |
| G-48 | `S46` | Isian kepala estimasi (jenis tindakan, jadwal, dokter) diambil dari pemesanan bedah atau diisi petugas | `PROPOSED` | `NON_BLOCKING_STANDARD` | Usulan: terisi dari kasus OK aktif bila ada, dan boleh diubah atau diisi manual bila tidak ada |
| G-49 | `S48`, `INP-S10` | Siapa pemilik privasi/hukum, dan apakah persetujuan umum boleh disimpan serta tanda tangan goresan di tablet boleh menjadi bukti | `PROPOSED` (`RWI-DEC-235`, `RWI-DEC-236` berstatus `draft`) | **`BLOCKING`** — `S48` seluruhnya dan bagian simpan `INP-S10` | Keputusan hukum dan privasi yang dikecualikan `RWI-DEC-006`. **`DEC-INP-003`** (dipertahankan, cakupan diperluas), alias `RWI-OQ-116` |
| G-50 | Seluruh slice | PRD v`0.2` tertinggal dari decision log: label "Workspace PPRI", letak di `episode-rawat-inap`, tanpa mesin keutuhan Rekam Medis, General Consent *fail-closed*, sumber diagnosis IPD, nama aksi rupiah `ViewAmount`, dan data dirangkai server | `PROPOSED` | `NON_BLOCKING_STANDARD` | Bukan pertentangan, karena PRD menyatakan decision log yang berlaku. Desain wajib membaca decision log revision `36`; revisi PRD ke v`0.3` bersifat opsional |
| G-51 | Seluruh slice | Capability map bagian 20 belum memuat Surat Pengantar Rawat Inap (`RWI-FACT-066`), dan nomor baris `InpAncillaryOrderAdapter.cs` bergeser (`RWI-FACT-065`) | `PROPOSED` | `NON_BLOCKING_STANDARD` | Kedua fakta sudah tercatat di decision log dan dibaca desain dari sana. Impact scan capability map bersifat opsional |

**Pertentangan (`CONFLICT`).** Tidak ada pertentangan bisnis yang terbuka. Tiga pertentangan dari audit sudah diputuskan pada closure pass: tipe unit intensif (`RWI-DEC-251`), dua interval deposit (`RWI-DEC-260`), dan hak lihat harga (`RWI-DEC-258`). Perbedaan isi QR dengan kartu kiosk berada di modul lain dan dicatat sebagai `RWI-OQ-125`.

### 20.6 Decision Log

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status | Dampak implementasi / domain |
|---|---|---|---|---|---|---|---|---|
| **`DEC-INP-020`** | Bagaimana tarif visit dokter rawat inap ditetapkan, dan dari mana Estimasi Biaya membaca catatan cito +25%, tindakan lebih dari 4 jam +25% per jam, *standby* 20%, dan anestesi 50%? | `INP-S46` — baris visit dokter dan catatan aturan biaya bedah | `RWI-DEC-232` menetapkan sumbernya Billing. Capability map 20.5: tarif tindakan dan kamar dapat diperkirakan, biaya admin terbaca dari `MstAdministrationFeePolicy`, tetapi konsep tarif visit dan keempat aturan biaya bedah tidak ada. Alias `RWI-OQ-122` | Baseline bagian 12 melarang mengarang tarif dan aturan payer. Pilihan yang dapat diambil Billing: (a) Billing menambah konsep tarif visit dan aturan biaya bedah; (b) catatan disimpan sebagai teks pengaturan, bukan rumus, dan baris visit diisi manual; (c) tarif visit memakai tarif konsultasi per kelas | Kontrak integrasi Rawat Inap ↔ Billing dan angka yang dijelaskan kepada pasien | Yasmina, disampaikan lewat Muhammad Hamzah | `OPEN` | Baris visit dokter dan catatan aturan biaya bedah **berhenti**. Dokumen Estimasi, tarif tindakan dan kamar, baris manual, catatan biaya admin, dan aturan wajib **boleh jalan** |
| **`DEC-INP-003`** (dipertahankan, cakupan diperluas 2026-10-07) | Pertanyaan asal: apakah pemilik privasi dan hukum menerima persetujuan umum yang tidak menahan admisi. **Ditambah:** siapa pemilik privasi/hukum untuk dokumen admisi; apakah persetujuan umum boleh disimpan sebagai satu rekaman `TrxPatientConsent` jenis `Admission` (`RWI-DEC-236`); dan apakah tanda tangan goresan di tablet sah sebagai bukti (`RWI-DEC-235`) | `INP-S10` (penyimpanan persetujuan umum), `INP-S48` (tanda tangan digital) | `RWI-DEC-035` dan `RWI-RULE-025` tetap `draft`; `RWI-DEC-230` memilih *fail-closed*; sisi produk `RWI-DEC-235` dan `236` sudah dipilih tetapi `draft`. Alias `RWI-OQ-116` | Baseline `ID-INP-CAP-002` menempatkan persetujuan sebagai prasyarat yang diverifikasi menurut kebijakan setempat; `ID-INP-REG-001` menempatkan dokumen rawat inap dalam konteks rekam medis elektronik. Keduanya `REFERENCE_ONLY` | Kewajiban hukum, perlindungan data pribadi, keabsahan bukti tanda tangan | Pemilik privasi/hukum yang ditunjuk Muhammad Hamzah | `OPEN` | Penyimpanan persetujuan umum dan tanda tangan digital **berhenti**. Cetakan General Consent tanpa simpan (`INP-S47`) dan seluruh dokumen admisi lain dengan tanda tangan kertas **boleh jalan** |

Keduanya bergantung pada pemilik, sehingga **tidak dijawab di sini**. Penutupannya lewat `grill-me`: `DEC-INP-020` setelah jawaban Yasmina tersedia, dan `DEC-INP-003` setelah pemilik privasi ditunjuk.

### 20.7 Contoh konkret untuk setiap blocker

**`DEC-INP-020` — Estimasi apendektomi Tn. Budi.** Petugas admisi menyusun Estimasi Biaya: tarif apendektomi Rp 8.500.000 dan kamar kelas 2 tiga hari × Rp 750.000 terisi dari layanan harga. Baris "visit DPJP 3 hari" tidak punya sumber, sehingga tampil "Tarif belum tersedia" dan dokumen tidak bisa dikunci sampai petugas mengisi angka manual beserta alasannya. Bila Billing kelak memilih pilihan (a), baris itu terisi otomatis dari tarif visit. Bila memilih (b), baris itu tetap manual dan catatan cito dibaca dari teks pengaturan. Kedua pilihan menghasilkan kontrak integrasi yang berbeda, sehingga perancang tidak boleh menebaknya.

**`DEC-INP-003` — persetujuan umum Ny. Rina.** Ny. Rina menandatangani persetujuan umum suaminya di kertas pukul 09.40. Hari ini sistem hanya mencetak, tanpa menyimpan apa pun (`RWI-DEC-233`). Bila pemilik privasi kelak mengizinkan penyimpanan, persetujuan itu menjadi satu rekaman `Admission` yang otomatis menandai butir daftar periksa penutupan (`RWI-DEC-236`). Bila tidak diizinkan, butir itu tetap ditandai manual. Pilihan ini mengubah apakah ada rekaman persetujuan di rekam medis dan apakah penutupan episode bisa tertahan karenanya, sehingga keputusan hukum dan privasinya tidak boleh diambil perancang.

### 20.8 Gerbang implementasi dan gerbang produksi

| Gerbang | Jenis | Isi | Pemilik |
|---|---|---|---|
| `RWI-OQ-124` | Implementasi | Peran CRO, supervisor admisi, dan pemegang `SignAsHeadNurse` tersedia di lingkungan target, dan aksi `InpatientAdmissionDocument` diberikan per peran (G-32) | Admin Akses Role |
| `RWI-DEC-193` | Implementasi | Perluasan `MstInpatientClearanceItem` (jenis dan induk) dan `MstInpatientSetting` (kode formulir, kota, batas umur gelang bayi) disetujui tim MasterData | Seluruh tim MasterData |
| QBE `TOUCHED LEGACY` | Implementasi | Perluasan tabel ber-`SortOrder` legacy mengikuti kontrak backend (`INP-S40`) | Pelaksana backend |
| G-35 | Produksi | Masa simpan dokumen admisi dan tinjauan privasi data pribadi keluarga (`RWI-DEC-230` butir 5) | Pemilik privasi/hukum (`RWI-OQ-116`) |
| G-38 | Produksi | Verifikasi aturan gelang, sapaan, dan isi QR | Tim keselamatan pasien |
| G-42 | Produksi | Penerimaan risiko bahwa nilai kepercayaan belum tampil di ruang kerja klinis pada MVP, atau permintaan amandemen keperawatan dan dokter | Pemilik clinical governance |

### 20.9 Kesiapan per slice

| Slice | Kesiapan | Gelombang (PRD 20.1) | Yang boleh jalan | Yang berhenti | Decision ID |
|---|---|---|---|---|---|
| `INP-S38` Ruang kerja, header, kelengkapan | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-1` | Seluruh slice | — | — |
| `INP-S39` Fondasi dokumen admisi | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-0` | Seluruh slice, termasuk mode kertas dan atestasi | — | — |
| `INP-S40` Serah Terima Pasien Baru | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-2` | Seluruh slice, bersama saringan jenis pada penutupan | — | — |
| `INP-S41` Gelang dan label | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-1` | Seluruh slice; produksi menunggu G-38 | — | — |
| `INP-S42` IPD | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-1` | Seluruh slice. Isian penerima informasi tetap kosong selama `DEC-INP-003` terbuka (`RWI-DEC-244`) | — | — |
| `INP-S43` Privasi dan Nilai Kepercayaan | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-2` | Seluruh slice; produksi menunggu G-42 | — | — |
| `INP-S44` Selisih Biaya | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-2` | Seluruh slice; produksi menunggu G-35 | — | — |
| `INP-S45` Pelunasan Deposit | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-2` (`RWI-DEC-231`) | Seluruh slice | — | — |
| `INP-S46` Estimasi Biaya Rekap | **`PARTIALLY_READY`** | Sesudah `RWA-MVP-2`, setelah `DEC-INP-020` | Dokumen, tarif tindakan dan kamar, baris manual, catatan biaya admin, aturan wajib | Baris visit dokter, catatan aturan biaya bedah | `DEC-INP-020` |
| `INP-S47` General Consent cetak saja | **`READY_FOR_DOMAIN_DESIGN`** | `RWA-MVP-1` (`RWI-DEC-246`) | Seluruh slice, termasuk perbaikan kop cetakan 12 butir | — | — |
| `INP-S48` Tanda tangan digital | **`BUSINESS_DECISION_REQUIRED`** | Di luar gelombang | — | Seluruh slice | `DEC-INP-003` |
| `INP-S10` Persetujuan umum tersimpan (slice lama) | **`BUSINESS_DECISION_REQUIRED`**, tidak berubah | Di luar gelombang | — | Penyimpanan persetujuan dan penandaan otomatis butir penutupan | `DEC-INP-003` |
| **Workspace PPRI secara keseluruhan** | **`PARTIALLY_READY`** | — | Sembilan slice penuh dan satu slice sebagian | Satu slice dan dua bagian kecil | `DEC-INP-020`, `DEC-INP-003` |

**Dependency antar-slice.** Seluruh slice dokumen (`S40` s.d. `S46`) memakai fondasi `S39`, sehingga desainnya harus menetapkan `S39` lebih dulu. Kelengkapan `S38` membaca status dokumen dari `S39` dan aturan wajib dari `S44` s.d. `S46`. Saran sistem pada `S40` membaca log cetak `S41` dan `S42`, status `S45` dan `S46`, serta Surat Pengantar Rawat Inap (`RWI-DEC-262`). IPD `S42` membaca dokumen `Completed` dari `S43`. `S48` bila kelak dibuka mengubah slot tanda tangan `S39` pada semua dokumen, bukan hanya satu slice.

**Status modul Rawat Inap** tetap **`PARTIALLY_READY`**: selain dua Decision ID di atas, `INP-S09` (serah terima IGD) masih menunggu pemilik IGD, dan transfusi serta handover shift tetap `DEFERRED`.

### 20.10 Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan:**

1. `design-business-module` untuk amandemen `episode-rawat-inap` (usulan kontrak `0.11.0`, `RWI-DEC-227`) pada sembilan slice yang siap dan bagian siap `INP-S46`. Amandemen ditulis sebagai bagian tersendiri tanpa mengubah isi Finishing yang sudah `approved` (`RWI-DEC-227`). Desain membaca decision log revision `36` sebagai sumber utama, dan PRD v`0.2` hanya sebagai pelengkap (G-50).
2. `02-module-map.md` diperbarui oleh desain untuk tabel kepemilikan data, layar anak `FE-INP-04`, dan pemetaan `CAP-RWA-*` ke `episode-rawat-inap`.
3. Pertanyaan `DEC-INP-020` kepada Yasmina, paralel dengan desain.

**Harus berhenti:**

1. Desain dan task untuk baris visit dokter dan catatan aturan biaya bedah, sampai `DEC-INP-020` tertutup.
2. Desain dan task untuk penyimpanan persetujuan umum (`INP-S10`) dan tanda tangan digital (`INP-S48`), sampai `DEC-INP-003` tertutup.
3. Memakai PRD v`0.2` sebagai sumber utama pada butir yang sudah diputuskan lain oleh decision log (G-50).
4. Implementasi apa pun sebelum amandemen blueprint disetujui dan task diturunkan. Wewenang tulis, migration, dan eksekusi database tetap terpisah per task.

### 20.11 Handoff

```yaml
gate_revision: 1.11
blueprint_id: RWI-BP-001
assessed_scope: PRD-RWI-ADMISI-001 v0.2 — CAP-RWA-01..17 (CAP-RWA-04 dibatalkan RWI-DEC-226; CAP-RWA-10 dan CAP-RWA-12 tetap ditunda)
sub_module: episode-rawat-inap (RWI-DEC-227); tabel milik InPatientManagement (RWI-DEC-228)
slices: INP-S38..INP-S48; penyimpanan persetujuan umum tetap slice lama INP-S10
evidence:
  decision_log: 00-interview-decisions.md revision 36, sha256 618f684b707fdb60f88d194623a265844faaf61fb02061e11eb61a4104f746df
  prd: 06-prd-to-mvp-workspace-admisi-rawat-inap.md v0.2 DRAFT, sha256 f1fd336f562d6936629d50f8f849fa7767e2dbea2b24678da9596de60dcc1192
  capability_map: 01-existing-capability-map.md revision 1.7 bagian 20, sha256 e8e454cbaed3f0bfd39793bbb4b82860ed9eb921b9154c24554b2bf06fb6e3da
  backend_sha: 671191eb (audit); HEAD fdf85a07 (RWI-FACT-065)
  frontend_sha: 27889662a (audit dan HEAD)
  baseline_reference: indonesia-hospital-domain-reference inpatient.md — ID-INP-CAP-001, ID-INP-CAP-002, ID-INP-CAP-016, ID-INP-REG-001, bagian 10, 12, 13, 14 (REFERENCE_ONLY)
readiness:
  READY_FOR_DOMAIN_DESIGN: [INP-S38, INP-S39, INP-S40, INP-S41, INP-S42, INP-S43, INP-S44, INP-S45, INP-S47]
  PARTIALLY_READY:
    INP-S46: {stop: ["baris visit dokter", "catatan aturan biaya bedah"], decision: DEC-INP-020}
  BUSINESS_DECISION_REQUIRED:
    INP-S48: {decision: DEC-INP-003}
    INP-S10: {decision: DEC-INP-003, note: "tidak berubah sejak revision 1.0; cakupan DEC-INP-003 diperluas"}
open_decisions:
  - DEC-INP-020: tarif visit dokter dan catatan aturan biaya bedah — owner Yasmina (alias RWI-OQ-122)
  - DEC-INP-003: pemilik privasi/hukum, penyimpanan persetujuan umum, tanda tangan digital — owner ditunjuk Muhammad Hamzah (alias RWI-OQ-116)
non_blocking: [G-30, G-31, G-32, G-33, G-34, G-36, G-37, G-39, G-40, G-41, G-43, G-44, G-45, G-46, G-48, G-50, G-51]
implementation_gate: [RWI-OQ-124 role-permission, RWI-DEC-193 MasterData, QBE TOUCHED LEGACY SortOrder]
production_gate: [G-35 masa simpan dan privasi, G-38 keselamatan pasien gelang, G-42 clinical governance nilai kepercayaan]
domain_architecture: opsional; tidak disarankan — seluruh tabel baru milik InPatientManagement (RWI-DEC-228), data modul lain dibaca lewat service pemilik (RWI-DEC-257), dan tidak ada ownership lintas modul baru selain perluasan MasterData
next_skill:
  - design-business-module untuk amandemen episode-rawat-inap 0.11.0 pada slice yang siap
  - grill-me singkat untuk DEC-INP-020 setelah jawaban Yasmina, dan untuk DEC-INP-003 setelah pemilik privasi ditunjuk
```

---

## 21. Gerbang kelengkapan Bed Management — 10 Oktober 2026

### 21.1 Hasil dan batas penilaian

**Hasil: `READY_FOR_DOMAIN_DESIGN` untuk enam kemampuan produk Bed Management yang dibatasi pada bagian 21.3.** Tujuan, pelaku, alur, status, pengecualian, akses minimum, dan hasil bisnisnya sudah cukup untuk dirancang. Tidak ada keputusan produk pemblokir yang masih menunggu jawaban pengguna.

Hasil ini mengikuti penutupan `RWI-DEC-274` s.d. `294`, termasuk delegasi eksplisit `RWI-DEC-279`. Gate mempertahankan pilihan yang sudah disahkan pengguna. Persetujuan produk tidak membuktikan SOP klinis, hak akses produksi, isi data master, atau hasil pengujian. Bukti tersebut masih diperlukan pada `BM-G01` s.d. `BM-G04`.

| Field | Nilai |
|---|---|
| ID assessment / revision dokumen | `BM-RCG-20261010-01` / `1.12` |
| Modul / menu | `rawat-inap`, `RWI-BP-001` / Bed Management |
| Sub-modul acuan | `episode-rawat-inap`; batas ownership existing tetap menjadi masukan, bukan rancangan baru |
| Batas tulis | `MODULE BLUEPRINT MODE`; hanya dokumen gate ini |
| Baseline blueprint | Manifest induk revision `9`; manifest episode revision `10`, `approved`; kontrak `0.11.0` |
| Status blueprint target Bed Management | Belum dirancang/disetujui; hasil gate tidak menaikkan kontrak atau memberi wewenang implementasi |
| Satuan penilaian | `BM-CG-01` s.d. `BM-CG-06` adalah ID kemampuan lokal assessment, bukan task roadmap atau bounded context baru |
| Scope keseluruhan Rawat Inap | Tidak dinilai ulang. Status historis `PARTIALLY_READY` serta keputusan di luar Bed Management tetap dipertahankan |

**Di dalam batas siap:** monitoring enam status; pemesanan/pelepasan; transfer satu langkah dengan kategori manual; pencatatan pembersihan dan pengesahan kesiapan sebagai alur produk; penutupan/pemulihan operasional bed; riwayat penggunaan dengan akses minimum dan koreksi berversi.

**Di luar batas siap:** merancang atau mengesahkan prosedur disinfeksi/pemeriksaan klinis; memilih orang yang sah menjadi verifikator; memberi hak baru atau akses lintas pasien; kebijakan privasi/retensi hukum baru; mengubah tarif/claim/payment; merombak proses klinis pindah/pulang, MasterData, admisi, atau IGD; menambah kerja offline, pengingat/perpanjangan pesanan, atau transfer dengan penerimaan tujuan sebagai gerbang.

Batas ini menentukan arti `READY_FOR_DOMAIN_DESIGN`. Desain boleh menerjemahkan keputusan produk dengan tindakan yang ditolak saat prasyarat/hak belum sah. Desain tidak boleh mengisi kebijakan klinis atau keamanan yang belum ada dengan konfigurasi atau rekomendasi AI. Jika bukti SOP/privasi kelak menuntut perubahan pelaku, data wajib, atau lifecycle, nilai ulang kemampuan terkait sebelum mengunci perubahan tersebut.

### 21.2 Bukti, wewenang, dan kesegaran

| Ref | Bukti yang dipakai | Revision / snapshot | Wewenang dan batas |
|---|---|---|---|
| BM-EV-01 | Instruksi BA butir 1–5, diteruskan pengguna; dicatat `RWI-FACT-068` | Sesi 10 Oktober 2026 | Target bisnis: nama menu, tiga tab, enam status, transfer asal otomatis/tujuan tersedia/kategori manual, riwayat termasuk tanpa transfer |
| BM-EV-02 | [Decision log](../00-interview-decisions.md), bagian Amendment Pass Bed Management | Revision `46`; SHA-256 `41dd035d62e7804dff7e796712e25410152fae8e67b9444257ba73e0a6a30b3d` | Target produk `CONFIRMED` melalui pilihan langsung dan delegasi. `RWI-DEC-006` tetap membatasi approval klinis/keamanan/privasi |
| BM-EV-03 | [Audit existing](../../../../../artifacts/bed-management/01-existing-capability-map.md), bagian 3–8 dan 11 | `BM-AUD-20261010-01` revision `1`; SHA-256 `50e0e1525804260331d3a830fb379532f802a3ff31e43cd3b4df4416fa42f38b` | Bukti source as-is. Rekomendasi bisnis audit masih draft kecuali secara eksplisit dipilih BM-EV-02 |
| BM-EV-04 | [Fingerprint audit](../../../../../artifacts/bed-management/evidence-fingerprints.json) | 62 berkas; BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`; FE `969acfcc04cdf31074a1911e9827c31d25ddadd0` | HEAD cocok. Sebanyak 61 fingerprint masih sama; decision log berubah dari revision 40 menjadi 46, dibaca sebagai bukti target terbaru |
| BM-EV-05 | [Manifest induk](../blueprint-manifest.md) dan [manifest episode](../episode-rawat-inap/blueprint-manifest.md) | Revision `9` / `10`; kontrak `0.11.0` | Baseline desain terdahulu. SHA-256 induk `ecd50237393ed7255eccfcaaed7172429feffc12ce95d6db47149e748a30de2f`; episode `b661259ee1812418ab0e78bc03b7c0f6dc19dda05f5bfe18b6c847843d568a8c` |

Branch BE `MHamzah` dan FE `HamzahV2`. Baseline Git berisi 38 entri backend dan 28 frontend yang telah ada sebelum penulisan gate. SHA commit saja tidak mewakili seluruh isi working tree; fingerprint melengkapi bukti tersebut.

Audit menyediakan bukti source per repository, path, baris/simbol, dan SHA pada bagian 11. Rujukan `E01` s.d. `E24` di bawah mengikuti indeks itu. Contoh: `E09` = backend `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`, `ReserveBedAsync`, `TransferAsync`, `ReleaseBedStatusCopyAsync`, pada snapshot BE di atas. Perubahan rule pada decision log tidak mengubah source service itu.

Tidak ada SOP rumah sakit, daftar akun/role target, data master aktual, approval PPI/privasi baru, atau bukti runtime baru yang tersedia dalam masukan assessment. Tidak memakai rujukan domain/regulasi tambahan. Baseline `REFERENCE_ONLY` pada bagian terdahulu dokumen ini tidak dinaikkan menjadi kebijakan Bed Management.

### 21.3 Kemampuan, proses, dan traceability

| Kemampuan | Tujuan, pelaku, pemicu / prasyarat | Alur dan hasil yang telah diputuskan | Data minimum | Bukti produk / audit |
|---|---|---|---|---|
| `BM-CG-01` Monitoring | Petugas berhak melihat ketersediaan pada scope unit/bed sah ketika membuka Monitoring Bed | Baca kondisi aktual → tampilkan enam status dan ringkasan konsisten → pilih bed yang layak; identitas hanya sesuai hak existing | Bed/kamar/unit/kelas, status/tahap, alasan tertahan atau konflik, ketersediaan aktual; konteks pasien hanya bila sah | `RWI-DEC-285/287/292`; `RWI-AC-396..398/420/424`; `BM-CAP-01..03/15`, `E09/E19..E22` |
| `BM-CG-02` Pemesanan dan pelepasan | Admisi/petugas existing berhak; episode dan kelayakan bed sah | Pesan → tempatkan, atau batal/expiry server; pelepasan hunian yang benar-benar dipakai memicu kebutuhan pembersihan | Episode, bed, pemegang pesanan/hunian, waktu pesan/batas berlaku/penempatan/keluar, actor, alasan pembatalan | `RWI-DEC-281/283/288/289`; `RWI-AC-407/408/413/418/419/426`; `BM-CAP-04/05/08/14`, `E04/E09/E10/E18` |
| `BM-CG-03` Transfer | Petugas transfer existing; hunian asal aktif, tujuan tersedia, guard DPJP/folio existing sah | Asal otomatis → tujuan → kategori manual → periksa ulang → simpan utuh atau batal seluruhnya; asal masuk pembersihan, tujuan Terisi, riwayat bertambah | Episode, asal/tujuan, kelas/urutan resmi saat kejadian, kategori, waktu, actor, alasan existing | `RWI-DEC-275..277/284/289/291`; `RWI-AC-399..405/414/419/421/423`; `BM-CAP-11/12/17`, `E09/E11/E16/E23` |
| `BM-CG-04` Pembersihan dan pengesahan | Housekeeping mencatat; perawat ruangan yang ditunjuk mengesahkan. Bed bekas pasien sudah dilepas; hak/scope dan SOP sah diperlukan untuk penerapan | Menunggu → mulai → selesai fisik/menunggu verifikasi → siap, atau kembali menunggu beralasan; tidak ada kesiapan otomatis karena timer | Bed/siklus pekerjaan terkini, pelaksana/pencatat/verifikator, waktu mulai/selesai/pemeriksaan, hasil, alasan, jejak upaya | `RWI-DEC-278/280..282/287..290/293`; `RWI-AC-406/407/409..411/416/417/419/421/425`; `BM-CAP-07..09`, `E01/E05/E09/E10` |
| `BM-CG-05` Penutupan/pemulihan | Admin MasterData sesuai scope; alasan operasional. Penutupan langsung tidak boleh menimpa hunian/pesanan aktif | Periksa seluruh guard → tutup beralasan → buka setelah sebab ditangani → tetap tertahan bila kesiapan belum sah; verifikator mengesahkan kesiapan | Bed, sebab, actor/waktu, kondisi hunian/pesanan/kebersihan, jejak penutupan/pembukaan | `RWI-DEC-285/287..289`; `RWI-AC-412/417/419/424`; `BM-CAP-10`, `E02/E05/E09` |
| `BM-CG-06` Bed Usage History | Pembaca berhak; memilih bed/periode dalam scope baca sah | Baca semua segmen, termasuk tanpa transfer → konteks kamar/kelas saat kejadian → versi koreksi; hunian aktif belum punya akhir | Bed/kamar/unit/kelas historis, episode/identitas sesuai hak, awal/akhir, jenis kejadian, kategori bila transfer, actor/alasan, versi | `RWI-DEC-286..288`; `RWI-AC-402/415..418`; `BM-CAP-13/17`, `E04/E09/E12/E17/E23` |

Urutan proses produk yang diperiksa:

1. Petugas membaca Monitoring Bed; sistem menilai ketersediaan dari kondisi aktual, bukan hanya salinan enum master.
2. Admisi berwenang memesan bed yang layak. Penempatan mengubah pesanan menjadi hunian; batal atau kedaluwarsa sebelum dipakai tidak membuat pekerjaan pembersihan baru.
3. Transfer sah mengakhiri hunian asal dan membuka hunian tujuan bersama-sama. Kategori manual harus cocok dengan urutan kelas resmi. Serah terima klinis existing berjalan sesudah transfer tersimpan.
4. Kepergian fisik sah atau transfer melepaskan bed bekas pasien. Bed tertahan untuk pembersihan, sementara penutupan episode lama tidak boleh melepaskan hunian pasien berikutnya.
5. Housekeeping mulai dan selesai; perawat verifikator mengesahkan atau menolak kesiapan. Penutupan administratif tetap menghalangi pemesanan.
6. Riwayat mempertahankan segmen hunian dan koreksi. Pemesanan, pembersihan, dan penutupan mempunyai audit operasional, tanpa dihitung sebagai durasi pasien menggunakan bed.

| Dari | Kejadian | Ke / hasil | Pelaku dan syarat |
|---|---|---|---|
| Tersedia | Pesan / penempatan sah | Dipesan → Terisi | Hak existing; kelayakan dan pemegang aktual diperiksa server |
| Dipesan | Batal / expiry sebelum dipakai | Tersedia hanya jika masih layak | Actor berhak atau evaluasi server; bukan pemicu pembersihan baru |
| Terisi | Transfer / kepergian sah | Menunggu Pembersihan, atau Tidak Tersedia bila ditutup | Pelepasan sah existing; kebutuhan bersih tetap tercatat |
| Menunggu Pembersihan | Mulai | Dalam Pembersihan | Housekeeping berhak pada scope bed |
| Dalam Pembersihan | Selesai fisik | Dalam Pembersihan, tahap Menunggu verifikasi | Housekeeping; belum boleh dipesan |
| Dalam Pembersihan | Verifikasi siap / belum siap | Tersedia / Menunggu Pembersihan | Perawat verifikator; seluruh guard terkini lolos / alasan penolakan tersimpan |
| Bed tanpa hunian/pesanan | Tutup beralasan | Tidak Tersedia | Admin berhak; pekerjaan dan audit terdahulu dipertahankan |
| Terisi / Dipesan | Tutup langsung melalui master | Ditolak; fakta pasien/pesanan tetap | Seluruh pintu tulis master menjaga invariant yang sama |
| Tidak Tersedia | Buka | Menunggu Pembersihan bila belum terbukti siap; Tersedia hanya sesudah pengesahan sah | Admin membuka; verifikator mengesahkan. Bukti basi tidak membuka siklus/penutupan baru |

Tabel ini merangkum keputusan, bukan menetapkan enum, API, atau rancangan penyimpanan. Transisi lengkap tetap pada decision log revision 46.

### 21.4 Penilaian 18 dimensi per kemampuan

`C` pada tabel berarti `CONFIRMED` untuk kebutuhan produk dari BM-EV-01/02, **bukan** bukti bahwa kemampuan sudah terimplementasi atau approval klinis/privasi telah diberikan. Angka `DEC` merujuk `RWI-DEC`. Keterangan penerapan yang masih `MISSING` dirinci pada 21.5 dan 21.7.

| Dimensi | BM-CG-01 Monitoring | BM-CG-02 Pemesanan | BM-CG-03 Transfer | BM-CG-04 Pembersihan | BM-CG-05 Penutupan | BM-CG-06 Riwayat | Bukti utama |
|---|---|---|---|---|---|---|---|
| 01 Tujuan | C: ketersediaan terpercaya | C: bed dikuasai satu pihak sah | C: perpindahan tercatat utuh | C: bekas pasien belum siap tidak dipesan | C: bed tak layak ditahan | C: semua penggunaan dapat ditelusuri | BA; DEC-281/285/286/289/292 |
| 02 Aktor | C: pembaca sesuai hak | C: admisi/petugas existing | C: petugas transfer existing | C: HK dan perawat verifikator | C: admin dan verifikator | C: pembaca/auditor sesuai hak | DEC-278/280/284/285/287; matriks kewenangan revision 46 |
| 03 Pemicu/prasyarat | C: buka tab, scope sah | C: episode dan bed layak | C: hunian asal aktif, tujuan sah | C: hunian dilepas, hak dan SOP sah saat penerapan | C: alasan, tanpa hold aktif | C: bed/periode dan scope sah | DEC-281/283..287/293 |
| 04 Alur utama | C: baca → status/ringkasan | C: pesan → tempatkan/batal/expiry | C: asal → tujuan → kategori → simpan | C: mulai → selesai → verifikasi | C: tutup → buka → periksa siap | C: filter → segmen → versi | Alur revision 46; bagian 21.3 |
| 05 Alternatif/exception | C: data invalid, stale, akses ditolak | C: konflik pemegang, expiry, kegagalan | C: kelas tak sah, asal berubah, gagal seluruhnya | C: belum siap, verifikator belum ada, siklus lama | C: hold aktif, tutup saat pekerjaan | C: data gagal, identitas disamarkan, koreksi | DEC-282/285/287/289..291 |
| 06 Data minimum | C: lokasi/status/tahap/alasan | C: episode/bed/waktu/actor/alasan | C: asal/tujuan/kelas/kategori/waktu | C: pekerjaan/actor/waktu/hasil/alasan | C: bed/sebab/actor/waktu/kesiapan | C: segmen/waktu/konteks/versi | DEC-278/284..288; bagian 21.3 |
| 07 Validation | C: arti Tersedia konsisten | C: satu pemegang, server otoritatif | C: kategori cocok, guard existing | C: tidak siap otomatis, bukti siklus terkini | C: semua jalur master, tidak menimpa hold | C: hak baca dan versi efektif | DEC-276/281..285/287/289/291/292 |
| 08 Status/transisi | C: enam status BA | C: Tersedia/Dipesan/Terisi + release | C: asal menunggu, tujuan terisi | C: menunggu/dalam/siap; verifikasi subfase | C: Tidak Tersedia dan pemulihan terkendali | C: aktif/berakhir serta versi, tanpa hunian fiktif | DEC-281/282/285/286; tabel 21.3 |
| 09 Authorization | C: hak baca dan scope | C: hak existing, alasan batal | C: transfer/koreksi sesuai hak existing | C: HK tidak mengesahkan; perawat berhak | C: admin tidak melewati verifikator | C: identitas hanya dengan hak pasien/episode | DEC-280/285/287/288; BM-G03 bukti grant belum ada |
| 10 Dependency | C: master/lokasi, reservasi/hunian, akses | C: episode, bed, setting, akses | C: episode/master/clinical/Billing | C: pelepasan, bed, operasional/akses | C: MasterData dan hunian/pembersihan | C: linimasa hunian/koreksi/akses | Audit E02/E04/E09/E13/E16/E17; DEC-193/284 |
| 11 Integrasi | C: baca sumber existing | C: operasi admisi/placement existing | C: guard folio, handover sesudah commit | C: kejadian release dan hak internal | C: seluruh pintu master | C: histori existing dan titik koreksi Billing | Audit E05/E09/E11..E17/E23; DEC-284/290 |
| 12 Hasil akhir | C: status/total/pilihan selaras | C: satu pesanan/hunian sah atau lepas | C: lokasi baru dan histori utuh | C: siap disahkan atau tetap tertahan | C: sebab tertangani, kesiapan tidak diasumsikan | C: riwayat lengkap sesuai hak | DEC-281..289/292 |
| 13 Batal/koreksi | C: baca tidak mengubah data | C: batal beralasan/expiry | C: tidak hapus transfer sah; koreksi existing | C: penolakan beralasan, ulang tanpa hapus upaya | C: buka bukan pembatalan audit | C: koreksi berversi, tidak hapus histori | DEC-157/283/285/288..290 |
| 14 Audit/history | C: perubahan sumber ditelusuri | C: actor/waktu/alasan | C: kelas saat kejadian, alasan, versi | C: mulai/selesai/verifikasi/upaya | C: tutup/buka/sebab/actor | C: semua hunian termasuk tanpa transfer | DEC-278/284/286/288 |
| 15 Notifikasi | C: keadaan/error pada layar; kanal baru tidak diminta | C: countdown existing; reminder/extension dikecualikan | C: hasil aksi; handover existing bukan gerbang | C: tahap Menunggu verifikasi terlihat; eskalasi melalui operasional | C: alasan tertahan terlihat | C: hasil baca/error; push baru tidak diperlukan | DEC-282/283/284/290/292; tidak mengarang SLA/notifikasi baru |
| 16 Billing/charge | C: pembacaan tidak membuat charge | C: linimasa pemakaian existing tetap sumber | C: kelas aktual/waktu hunian tetap; label bukan tarif | C: waktu membersihkan bukan hunian pasien | C: penutupan administratif bukan charge pasien | C: koreksi tunduk guard dan integrasi existing | DEC-013/157/166/186/284/286/288; audit E17 |
| 17 Keselamatan klinis | C: tak layak tidak ditawarkan; fakta pasien tidak hilang | C: tidak ada dua pemegang; guard existing tetap | C: tidak memilih bed tak siap atau mengubah keputusan klinis | C: bekas pasien ditahan sampai pemeriksaan sah | C: tidak memalsukan pasien keluar | C: identitas dibatasi, konteks lama terjaga | DEC-006/280..282/285/287/289/293; SOP BM-G02 belum terbukti |
| 18 Pelaporan/traceability | C: ringkasan dengan arti status sama | C: pesanan dan hunian berbeda makna | C: perpindahan/kategori/konteks tersimpan | C: tindakan, waktu, hasil dapat ditelusuri | C: alasan dan jejak pemulihan | C: bed/periode/kamar/semua segmen/versi | DEC-284..288/292; AC-402/415/418/420 |

Dimensi kondisional 15–18 sudah dipertimbangkan. Tidak ada kebutuhan notifikasi WhatsApp/email/push, laporan statistik baru, atau tenggat pekerjaan pembersihan yang dikonfirmasi. Membaca board/history tidak membuat charge. Waktu pembersihan/bed ditutup tidak ditambahkan menjadi durasi hunian pasien. Perubahan penempatan/koreksi tetap berdampak ke Billing existing; gate ini tidak memvalidasi kalkulasi atau pengiriman runtime-nya.

### 21.5 Klasifikasi requirement dan gap

**`CONFIRMED`:** mandat BA dan keputusan produk `RWI-DEC-274..294`; pemisahan kewenangan `RWI-DEC-006`; guard koreksi/Billing existing. Persetujuan langsung/delegasi sudah tertulis, sehingga pilihan produk tidak kembali menjadi `PROPOSED` hanya karena awalnya rekomendasi AI.

**`CONFLICT`:** tidak ada pertentangan bisnis terbuka dalam batas produk ini. `RWI-CON-016` diselesaikan keputusan terbaru. Contoh lama pada `RWI-DEC-186` yang mengatakan bed langsung Available dibaca dengan amandemen `RWI-DEC-281`: pelepasan hunian tetap segera, tetapi bed bekas pasien tertahan untuk pembersihan. Guard kasir di penutupan episode tidak berubah. Ini perbedaan target dengan source/dokumen lama yang harus ditindaklanjuti, bukan keputusan produk yang belum dipilih.

| ID / rujukan | Pernyataan yang masih belum terbukti atau belum dipilih | Status bukti | Dampak pada desain dalam batas 21.1 | Batas penerapan / tindak lanjut |
|---|---|---|---|---|
| `BM-G01` | Arti/arah angka ClassLevel, kesetaraan sah dan isi master target belum diverifikasi | `MISSING` untuk fakta data; kebijakan urutan global dan menahan perbandingan tak sah `CONFIRMED` | `NON_BLOCKING_STANDARD`: perilaku produk sudah tegas, desain tidak menebak arah angka | Validasi/perbandingan terkait tidak diaktifkan sebelum bukti MasterData ada; seluruh tim MasterData + BA/Product |
| `BM-G02` | SOP pembersihan/pemeriksaan/downtime serta penugasan nyata belum tersedia | `MISSING` untuk bukti operasional; alur dan tanggung jawab produk `CONFIRMED` | `NON_BLOCKING_STANDARD` hanya untuk pencatatan/alur produk yang sudah dibatasi; desain prosedur klinis tidak termasuk hasil siap | Aktivasi workflow pada bed pasien nyata tertahan sampai SOP/penugasan sah. Jika SOP mengubah lifecycle/data/authority, assessment terkait harus diulang |
| `BM-G03` | Pemetaan permission/scope/akun dan approval privasi/keamanan target belum terbukti; owner privacy masih OPEN | `MISSING` untuk bukti penerapan; batas minimum/penolakan hak tak sah `CONFIRMED` | `NON_BLOCKING_STANDARD` untuk desain yang mempertahankan hak existing dan menolak grant tidak diketahui | Dilarang memberi role/hak baru atau membuka identitas lintas pasien sebelum otorisasi sah. Pilihan kebijakan akses baru tidak diloloskan lewat konfigurasi |
| `BM-G04` | Implementasi target, perbaikan `BM-F01..F07`, API/PG/integrasi/migration target belum dibuktikan | `MISSING` untuk bukti pemenuhan; perilaku wajibnya `CONFIRMED` | `NON_BLOCKING_STANDARD`: kekurangan implementasi tidak mengubah requirement menjadi belum diputuskan | Readiness/rilis tetap tertahan sesuai kemampuan terdampak sampai implementasi dan uji terbukti |
| `BM-RQG-01` | Penempatan kontrol/tab, warna nonkritis dan detail tata letak akan dipilih saat desain | `PROPOSED` untuk pilihan konkret; delegasi `DEV_DISCRETION` `CONFIRMED` | `NON_BLOCKING_STANDARD`; ikuti convention/token/component/accessibility existing | Tidak mengubah enam status, permission, scope data, atau alur |
| `BM-RQG-02` | Metadata ringkasan lama masih menyebut 273 keputusan/395 AC; contoh lama dan blueprint belum menyerap Bed Management | `CONFIRMED` sebagai selisih dokumen yang dibaca | `NON_BLOCKING_STANDARD`; bagian Bed Management revision 46 dan assessment ini menjadi acuan slice | Sinkronisasi oleh amandemen desain berikutnya; tidak menyatakan blueprint/kontrak target sudah approved |

Label `NON_BLOCKING_STANDARD` pada `BM-G01..04` berlaku **hanya terhadap desain kebutuhan produk yang telah dibatasi**, bukan terhadap izin mengaktifkan layanan, kebijakan klinis, keamanan, atau keselamatan pasien. Gate penerapan tersebut tetap wajib. Bukti yang belum ada tidak dianggap persetujuan tersirat.

Tidak ada gap `BLOCKING` yang bergantung keputusan produk tersisa pada enam kemampuan ini. Tidak ada gap kritis yang dialihkan ke `CONFIGURABLE_DEFAULT`. Default reservasi 120 menit beserta parameter resmi existing sudah `CONFIRMED` (`RWI-DEC-283`), bukan usulan default baru.

**Contoh batas yang tetap dihentikan:** bila SOP mensyaratkan pemeriksaan isolasi tertentu yang belum tercakup, perancang tidak boleh menciptakan checklist/hasil klinisnya. Bila rumah sakit meminta Housekeeping melihat diagnosis, permintaan itu mengubah batas akses `RWI-DEC-287` dan memerlukan keputusan keamanan/klinis yang sah sebelum desain bagian tersebut. Keduanya bukan alasan membuka ulang seluruh pertanyaan produk yang sudah ditutup.

### 21.6 Decision Log yang dipertahankan

| Decision ID | Pokok keputusan / ambiguitas asal | Kemampuan | Bukti / pemilik | Status dan akibat |
|---|---|---|---|---|
| `RWI-DEC-274/292/294` | Batas BA, menu/tiga tab/detail UI, penutupan pass | Semua | Pengguna sesi; Product/Domain owner | `CLOSED` pada produk; desain boleh memakai kebutuhan ini, belum ada approval desain |
| `RWI-DEC-275..277/291`; `RWI-OQ-130/135` | Same Grade, kategori manual cocok, satu urutan resmi, data belum sah ditahan | BM-CG-03 | Jawaban langsung pengguna; seluruh tim MasterData untuk BM-G01 | `CLOSED` pada pilihan produk; fakta master belum diverifikasi |
| `RWI-DEC-278/280..282`; `RWI-OQ-131` | HK mencatat, perawat mengesahkan, bed bekas pasien dibersihkan, enam status | BM-CG-02/04 | Pengguna/delegasi; operasional ruangan/HK/PPI untuk bukti nyata | `CLOSED` pada produk; SOP/penugasan tidak dikarang |
| `RWI-DEC-283/284/289/290` | Expiry, transfer satu langkah, benturan/retry/downtime | BM-CG-02/03 dan tindakan bed lain | Pengguna/delegasi; pemilik aplikasi/environment | `CLOSED` pada produk; guard existing dan bukti implementasi tetap diperlukan |
| `RWI-DEC-285/287`; `RWI-OQ-132/134` | Tidak Tersedia beralasan, guard semua pintu, hak/scope minimum | BM-CG-01/04/05/06 | Pengguna/delegasi; admin akses/MasterData dan owner privacy yang sah | `CLOSED` sebagai batas produk; bukan pemberian akses atau approval privacy |
| `RWI-DEC-286/288`; `RWI-OQ-133` | Semua penggunaan, versi koreksi, audit/alasan, identitas sesuai hak | BM-CG-06 | Pengguna/delegasi; petugas koreksi existing; Billing untuk guard existing | `CLOSED` pada produk; tidak menambah retensi hukum atau hapus histori |
| `RWI-DEC-279/293` bersama `RWI-DEC-006` | Batas delegasi dan bukti penerapan | Semua | Instruksi eksplisit pengguna dan keputusan governance existing | `CONFIRMED`; tidak mengubah keputusan produk menjadi approval klinis/keamanan |
| `DEC-INP-003` dan keputusan gate lain di luar Bed Management | Privasi/consent/tanda tangan dan gap modul lain | Scope terdahulu | Tetap mengikuti bagian terdahulu beserta amendment yang relevan | Tidak ditutup atau dinilai ulang di sini; bukan alasan memblokir desain Bed Management yang tidak bergantung penyimpanan consent baru |

Tidak membuat Decision ID `OPEN` baru untuk menanyakan ulang pilihan yang telah ditutup. `BM-G01..04` tetap identifier bukti yang stabil. Keputusan pemilik tambahan **tidak diperlukan untuk memulai desain dalam batas ini**. Persetujuan SOP/akses nyata tetap diminta dari pihak yang sah saat akan diterapkan; `grill-me` hanya diperlukan jika jawaban mereka mengubah kebutuhan atau membuka scope baru.

### 21.7 Gerbang penerapan dan bukti penutup

| Gate / dependency | Bagian terdampak | Bukti minimum untuk menutup gate | Pemilik berwenang |
|---|---|---|---|
| `BM-G01 / BM-DEP-01` MasterData | Ketersediaan/pemesanan dan kategori transfer yang bergantung master | Sumber/arti urutan kelas resmi dan arah ClassLevel, kesetaraan sah, validitas kelas/bed aktif-reservable pada data target; hasil pemeriksaan tercatat | Seluruh tim MasterData (`RWI-DEC-193`) + BA/Product |
| `BM-G02 / BM-DEP-02` Operasional | Pembersihan, pengesahan dan rekonsiliasi downtime | SOP sah yang berlaku, penugasan HK/verifikator per shift dan serah terima; bukti bahwa hasil kerja diperiksa sesuai kewenangan | Penanggung jawab HK/ruangan/PPI sesuai struktur rumah sakit; nama/approval belum tersedia |
| `BM-G03 / BM-DEP-03` Akses | Semua action serta baca identitas/history | Matriks hak/scope nyata, akun sah, pembatasan response; verifikasi penolakan akses tanpa hak; approval keamanan/privasi yang diperlukan | Admin akses/Platform dan owner keamanan/privasi yang sah; identitas owner belum dinyatakan |
| `BM-G04 / BM-DEP-04` Implementasi dan runtime | Semua kemampuan target | Perbaikan tujuh temuan source; uji transisi/penolakan/konkurensi/retry; uji API/PostgreSQL/integrasi; bukti kesesuaian migration/DB target bila desain membutuhkan perubahan | Pemilik aplikasi BE/FE dan environment, melalui task yang disetujui |

Ketergantungan operasional yang terkonfirmasi: MasterData menyediakan lokasi/kelas/kelayakan; Rawat Inap memegang pesanan dan hunian existing; ClinicalManagement memegang keputusan/serah terima klinis existing; Billing memakai linimasa dan guard existing; Platform mengendalikan akses; HK/ruangan/PPI membuktikan pekerjaan dan pemeriksaan. Assessment ini tidak menetapkan aggregate, tabel, field transport, atau kontrak integrasi target.

`BM-DEP-05` = integrasi klinis/Billing existing, khusus transfer, koreksi, dan pelepasan. Pertahankan `RWI-DEC-013/157/161/166/186` beserta amendment terbaru. Audit source bukan bukti pengiriman event atau kalkulasi Billing berhasil di environment target.

Contoh yang harus dipertahankan oleh desain dan kelak diuji:

1. **Pembersihan:** pasien A keluar 10.00; HK mulai 10.05, selesai 10.20. Pemesanan 10.22 ditolak. Perawat mengesahkan 10.25 setelah syarat sah; admin yang telah menutup bed 10.24 membuat pengesahan tersebut ditolak.
2. **Transfer/kelas:** kelas X sah berada di atas Y. Y → X dipilih Down Grade ditolak tanpa mengubah lokasi. Dua kelas berbeda sama-sama bernilai default ClassLevel 0 tidak otomatis dianggap Same Grade.
3. **Expiry/benturan:** pesanan belum dipakai pada 09.00 dengan default 120 menit dapat lepas ketika server mengevaluasi setelah 11.00. Bed yang ternyata ditutup/dikuasai pihak lain tidak menjadi Tersedia dari countdown browser. Dua permintaan bersamaan tidak boleh menghasilkan dua pemegang.
4. **Koreksi/histori:** pasien masuk bed A lalu pindah B tetap mempunyai dua segmen; pasien yang hanya memakai A tetap muncul. Koreksi salah waktu mempertahankan versi lama dan guard Billing. Waktu pembersihan tidak dihitung sebagai waktu hunian.
5. **Akses/siklus:** akun HK melihat bed 101-A dan tahap kerja, tanpa identitas/diagnosis pasien. Penutupan episode A pukul 13.00 tidak melepas bed yang sudah dibersihkan dan ditempati pasien B.
6. **Gangguan:** hasil simpan yang tidak pasti harus dibaca kembali sebelum ulang; tidak tampil sukses rekaan. Gagal callback setelah transfer tersimpan tidak otomatis membalik hunian. Rekonsiliasi mengikuti SOP yang sah.

### 21.8 Kesiapan per kemampuan dan dependency

| Kemampuan | Readiness | Yang boleh dirancang | Dependency / batas yang tetap berlaku |
|---|---|---|---|
| `BM-CG-01` Monitoring | `READY_FOR_DOMAIN_DESIGN` | Nama/tab/status, ringkasan/pilihan konsisten, alasan tertahan dan akses minimum | BM-DEP-01/03/04; tidak perlu menunggu SOP disinfeksi untuk mendesain pembacaan status |
| `BM-CG-02` Pemesanan/pelepasan | `READY_FOR_DOMAIN_DESIGN` | Expiry existing, guard, alasan batal, pelepasan bed bekas pasien dengan kebutuhan pembersihan | BM-DEP-01/03/04/05; penerapan release baru terhubung BM-CG-04 dan BM-G02 |
| `BM-CG-03` Transfer | `READY_FOR_DOMAIN_DESIGN` | Asal otomatis, tujuan layak, kategori manual tervalidasi, operasi utuh, konteks histori | BM-DEP-01/03/04/05; BM-CG-04 menahan bed asal; perbandingan tidak sah tetap ditolak |
| `BM-CG-04` Pembersihan/pengesahan | `READY_FOR_DOMAIN_DESIGN` dalam batas alur produk | Pelaku/tindakan/status/upaya/alasan, larangan kesiapan otomatis, penolakan tanpa hak/SOP sah | BM-DEP-02/03/04; tidak merancang prosedur klinis atau menyatakan akun nyata sudah ditunjuk |
| `BM-CG-05` Penutupan/pemulihan | `READY_FOR_DOMAIN_DESIGN` | Sebab, guard seluruh pintu master, pemulihan tanpa melewati pengesahan | BM-DEP-01/03/04; berhubungan BM-CG-02/04 untuk hold/kesiapan |
| `BM-CG-06` Riwayat penggunaan | `READY_FOR_DOMAIN_DESIGN` | Segmen per bed/periode, konteks historis, semua penggunaan, versi, identitas sesuai hak | BM-DEP-03/04/05; data hunian dari BM-CG-02/03, tanpa menunggu reminder/transfer bertahap |
| **Bed Management, batas produk 21.1** | **`READY_FOR_DOMAIN_DESIGN`** | Enam kemampuan di atas | Tidak sama dengan siap implementasi/aktivasi/produksi |
| **Rawat Inap keseluruhan** | **`PARTIALLY_READY` dipertahankan dari assessment terdahulu** | Hanya slice yang memang siap pada assessment masing-masing | Gate ini tidak menutup blocker consent/IGD/fitur lain di luar scope |

### 21.9 Yang boleh berjalan dan yang harus berhenti

**Boleh berjalan berikutnya:** `design-business-module` untuk amandemen Bed Management dengan masukan decision log revision 46, audit existing, dan assessment 1.12. Keluaran yang diharapkan: blueprint bounded yang memperinci alur/state/permission/audit, arsitektur dan data dictionary, kontrak serta acceptance evidence. Semua itu merupakan pekerjaan tahap berikut, belum dibuat/dikunci oleh gate ini.

`hospital-domain-architect` bersifat opsional. Gunakan bila tim memerlukan pendalaman batas tanggung jawab lintas MasterData, Rawat Inap, operasional pembersihan, Platform dan Billing sebelum blueprint. Skill tersebut tidak menjadi gerbang wajib tambahan.

**Tetap dihentikan:** menetapkan SOP/pemeriksaan klinis atau grant akses baru tanpa wewenang; mengaktifkan aksi/penyingkapan yang belum memenuhi BM-G01..04; menyatakan runtime atau produksi siap; memakai salinan status/master untuk melewati hold/cleaning; merancang perubahan billing, retention, offline, consent atau alur lain di luar scope. Implementasi aplikasi dan task delivery menunggu desain/kontrak yang disetujui serta wewenang task tersendiri.

Inventaris endpoint existing bergaya Swagger tetap pada [audit bagian 7](../../../../../artifacts/bed-management/01-existing-capability-map.md#7-kontrak-api-as-is). Gate ini tidak menambah endpoint atau menetapkan request/response target. Tujuh temuan `BM-F01..F07` tetap pekerjaan tindak lanjut, bukan perubahan yang sudah selesai.

### 21.10 Handoff

```yaml
gate_id: BM-RCG-20261010-01
gate_revision: "1.12"
blueprint_id: RWI-BP-001
sub_module: episode-rawat-inap
assessed_scope: Bed Management — batas produk bagian 21.1
evidence:
  decision_log: "revision 46; sha256 41dd035d62e7804dff7e796712e25410152fae8e67b9444257ba73e0a6a30b3d"
  audit: "BM-AUD-20261010-01 revision 1; source-audited"
  audit_sha256: 50e0e1525804260331d3a830fb379532f802a3ff31e43cd3b4df4416fa42f38b
  backend_sha: d4e1eca06fb28c05934c68c1e51a4dca01935a10
  frontend_sha: 969acfcc04cdf31074a1911e9827c31d25ddadd0
  working_tree: "61 fingerprint audit sama; decision log diperbarui ke revision 46"
  baseline_reference: "tidak dipakai pada assessment ini"
classification:
  confirmed: ["BA butir 1–5", "RWI-DEC-274..294", "RWI-FACT-068..070", "RWI-AC-396..426"]
  proposed: ["detail UI nonkritis dalam DEV_DISCRETION; bukan perubahan aturan"]
  missing: ["bukti aktual BM-G01", "bukti aktual BM-G02", "bukti aktual BM-G03", "bukti aktual BM-G04"]
  conflict: []
readiness:
  READY_FOR_DOMAIN_DESIGN: [BM-CG-01, BM-CG-02, BM-CG-03, BM-CG-04, BM-CG-05, BM-CG-06]
  open_blocking_product_decisions: []
  overall_rawat_inap: PARTIALLY_READY
dependencies: [BM-DEP-01, BM-DEP-02, BM-DEP-03, BM-DEP-04, BM-DEP-05]
application_gates_open: [BM-G01, BM-G02, BM-G03, BM-G04]
source_findings_open: [BM-F01, BM-F02, BM-F03, BM-F04, BM-F05, BM-F06, BM-F07]
decision_log_state: "RWI-OQ-130..135 ditutup pada produk atau dipindahkan eksplisit ke bukti; tidak dibuka ulang"
approval_boundary: "produk; tidak mencakup SOP klinis, privacy/security, grant akses atau kesiapan runtime"
design_baseline: "induk revision 9; episode revision 10 approved; kontrak 0.11.0 belum diamendemen untuk Bed Management"
next_skill: design-business-module
optional_skill: hospital-domain-architect
expected_output: "amandemen blueprint Bed Management dengan traceability ke keputusan, audit, AC dan gate penerapan"
```

### 21.11 Bukti validasi assessment

Pemeriksaan dokumentasi dilakukan terpisah dari pengujian aplikasi, pada 10 Oktober 2026:

| Pemeriksaan | Hasil | Bukti / batas |
|---|---|---|
| Scope perubahan | PASS | Hanya dokumen gate ini ditambah bagian 21 dan metadata terkait. Decision log, manifest, kontrak, roadmap dan source aplikasi tidak diubah |
| Histori assessment | PASS | Isi mulai penjelasan awal sampai akhir bagian 20 identik setelah normalisasi newline; SHA-256 histori `fe78d39695fac87fdf6a9d6c9a638ccff824fd34b416eb1b6a4874bd4cc6d739` tetap sama |
| Dimensi dan tabel | PASS | 18 dimensi menilai enam kemampuan; lebar kolom tabel konsisten; bagian 21 tercatat satu kali |
| Referensi keputusan | PASS | 25 ID eksplisit unik yang dirujuk ditemukan dalam decision log revision 46; rentang keputusan/AC mengikuti section Bed Management |
| Tautan bukti lokal | PASS | Enam tautan lokal bagian 21 mencapai berkas yang tersedia; inventaris Swagger merujuk bagian 7 audit |
| Pemeriksaan diff | PASS | `git diff --check -- docs/module-blueprints/rawat-inap/evidence/02-requirement-completeness-gate.md` selesai tanpa error |
| Kesegaran bukti source | PASS | Seluruh 62 fingerprint sama dengan baseline awal gate. Terhadap audit revision 1, hanya decision log telah diperbarui sebelumnya; 61 berkas lain sama. Kedua HEAD tidak berubah |
| Git status akhir | PASS | Backend 39 entri: 38 baseline dan satu tambahan dokumen gate ini. Frontend tetap 28 entri, tanpa perubahan status dari baseline |
| Approval dan klaim kesiapan | PASS | Produk tertutup, empat gate penerapan tetap terbuka; tidak ada SOP/grant/approval klinis-privasi atau runtime yang dibuat seolah tersedia |
| QBE/source/migration | NOT RUN | Tidak ada implementasi aplikasi atau perubahan schema pada task dokumentasi ini |
| Tes aplikasi/API/PostgreSQL/integrasi | NOT RUN | Hasil unit test pada audit terdahulu bukan bukti skenario target; pembuktian tetap BM-G04 |
