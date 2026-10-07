# Traceability Requirement — Keperawatan Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Berkas | `keperawatan/roadmap/requirement-traceability-finishing.md` — revision `1` |
| Status | **`DRAFT`**, mengikuti status kedua roadmap pendamping |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `keperawatan`, kontrak `0.6.0` `approved` 2026-10-02 (`RWI-DEC-221`) |
| Roadmap | `backend-roadmap-finishing.md` revision `1` (`BE-RWI-165` s.d. `171`); `frontend-roadmap-finishing.md` revision `1` (`FE-RWI-180` s.d. `191`) |
| Sumber requirement | `PRD-RWI-FINISHING-001` v`0.4` (rumusan FR); `04-prd-to-mvp.md` bagian 23; decision log revision `32` |
| Masukan dan hash | Seperti metadata roadmap; hash lengkap pada `../blueprint-manifest.md` bagian 9 |
| Source SHA | Backend `bf5c6bde`; frontend `f74758af5` |
| Gate | `evidence/02-requirement-completeness-gate.md` revision `1.10` bagian 19 |

**Cara membaca bukti.** Backend: verifikasi API/kontrak, pencarian kode, verifikasi proses bisnis, dan runtime bila tersedia — bukan automated test (`rules/backend/TEST_POLICY.md`). Frontend: lint, build, dan verifikasi manual kontrol interaktif. Butir yang tidak dijalankan ditulis `NOT RUN`.

**Pembaruan bukti 5 Oktober 2026.** Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini. Penerapan mencakup enam task pemilik perubahan skema, bukan semua task Finishing. Requirement dengan frontend/runtime tertunda tetap sebagian. `BE-RWI-149` mempertahankan catatan pengemasan `I1`/`I2` menjadi satu migration. SHA metadata tetap snapshot perencanaan; approval roadmap tetap `DRAFT`. [Bukti penerapan](../../episode-rawat-inap/task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

**Pembaruan bukti 5 Oktober 2026 (penyelesaian).** Ketujuh task backend `BE-RWI-165`–`171` ✅: build akhir `dotnet build` `0 Error(s)`; migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` diterapkan ke database development (`Done.`, nol `Pending`). Requirement yang masih punya task frontend tetap 🟡. Uji API dan UAT runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026.

## 1. Matriks requirement → desain → kontrak → task → bukti

| Requirement | Keputusan | Desain | Kontrak `0.6.0` | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| `FR-RWF-020` s.d. `025` Tagihan Pasien per kelompok, tanpa rupiah, `NOT_FORMED` apa adanya | `RWI-DEC-170`, `108` | FE 11.1, 11.5 | `integrasi-billing` API 3.7 | `integrasi-billing` `BE-RWI-148`, `BE-RWI-156` | `FE-RWI-185` | Akun perawat tanpa request `/amounts`; akun admisi subtotal tanpa harga per item (`AC-RWF-020` s.d. `023`, `UAT-RWF-10`) | Belum dikerjakan |
| `RWI-AC-330` (tampilan) Baris operasi kunjungan asal | `RWI-DEC-207` | FE 11.7 | `integrasi-billing` API 3.11 | `integrasi-billing` `BE-RWI-159` | `FE-RWI-186` | `UAT-RWF-35`; satu kwitansi di luar cakupan (G-27) | Belum dikerjakan |
| `FR-RWF-050` Enam sub-menu Catatan Keperawatan | `RWI-DEC-172` (1) | FE 11.1 | API 8.1 | — | `FE-RWI-180` | Manual urutan V1 (`AC-RWF-050`) | Belum dikerjakan |
| `FR-RWF-051` Sub-menu hanya jendela; satu data | `RWI-DEC-172` (2) | Backend 12.3 (`INV-RWF-10`) | API 8.1, 8.4 | `BE-RWI-165` | `FE-RWI-180`, `FE-RWI-183` | Data cairan sama di dua layar (`AC-RWF-051`); review diff tanpa tabel salinan | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-165](../task/report/backend/BE-RWI-165.md)); frontend `FE-RWI-180`, `FE-RWI-183` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-052` DPO empat bagian | `RWI-DEC-172` (1d) | FE 11.1 | — | — | `FE-RWI-181` | Manual (`AC-RWF-053`) | Belum dikerjakan |
| `FR-RWF-053` Efek Samping lewat ADR | `RWI-DEC-172` | FE 11.1 | API 8.1 | — (`EXISTING / REUSE`) | `FE-RWI-181` | Efek samping muncul di riwayat alergi (`AC-RWF-054`) | Belum dikerjakan |
| `FR-RWF-054` WSD per selang tiap shift | `RWI-DEC-200` | Backend 12.6.2 (`INV-RWF-11`, `12`) | API 8.4 | `BE-RWI-165` | `FE-RWI-183` | Contoh berangka `UAT-RWF-09`, `22` (`AC-RWF-052`, `057`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-165](../task/report/backend/BE-RWI-165.md)); frontend `FE-RWI-183` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-055` Diet Medis atas instruksi | `RWI-DEC-178`, `188`, `191` | Backend 12.7 (`INV-RWF-19`); integrasi 9.5 | API 8.8, 8.9 | `BE-RWI-166` | `FE-RWI-184`; verifikasi dokter: `dokter-rawat-inap` `FE-RWI-175` | `UAT-RWF-26` (`AC-RWF-055`); regresi `RWI-AC-303` | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-166](../task/report/backend/BE-RWI-166.md)); frontend `FE-RWI-184`, `FE-RWI-175` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-056` Narasi perawat tetap CPPT | `RWI-DEC-172` | Backend 12.2 | API 8.1 (`noteKind`) | — (`EXISTING / REUSE`) | `FE-RWI-180` | Manual saringan Naratif Keperawatan (`AC-RWF-056`) | Belum dikerjakan |
| `FR-RWF-057` Obat & Alkes empat sub-tab | `RWI-DEC-172` (3) | FE 11.1 | — | — | `FE-RWI-182` | Manual | Belum dikerjakan |
| `FR-RWF-058` Selang WSD terdaftar | `RWI-DEC-200` | Backend 12.6.2 | API 8.4 | `BE-RWI-165` | `FE-RWI-183` | Sisa awal saat pasang atau 0 ml (`AC-RWF-058`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-165](../task/report/backend/BE-RWI-165.md)); frontend `FE-RWI-183` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-060` Master jenis alat | `RWI-DEC-179` (1) | Backend 12.7 | API 8.2 | `BE-RWI-172` (`episode-rawat-inap`, model), `BE-RWI-167` | `FE-RWI-187` | API dan manual (`AC-RWF-060`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-167](../task/report/backend/BE-RWI-167.md)) (`BE-RWI-172` ✅ `episode-rawat-inap`); frontend `FE-RWI-187` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-061` Tarif alat di master tarif yang sama | `RWI-DEC-179`, `193` | Backend 12.10 | API 8.2 (field tarif) | `BE-RWI-172` (`episode-rawat-inap`, kolom dan isian tarif), `BE-RWI-167` | `FE-RWI-187` | Regresi tarif tindakan dan obat | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-167](../task/report/backend/BE-RWI-167.md)) (`BE-RWI-172` ✅ `episode-rawat-inap`); frontend `FE-RWI-187` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-062` Dua tab Pemakaian Alat | `RWI-DEC-179` (3) | FE 11.1 | — | — | `FE-RWI-188` | Manual | Belum dikerjakan |
| `FR-RWF-063` Unit dihitung server dari waktu | `RWI-DEC-179` (4) | Backend 12.3 (`INV-RWF-13`) | API 8.3 | `BE-RWI-168` | `FE-RWI-188` | Contoh ventilator 3 unit (`UAT-RWF-06`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-168](../task/report/backend/BE-RWI-168.md)); frontend `FE-RWI-188` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-064` Dokter berpenugasan; perawat dari login | `RWI-DEC-179` (4) | Backend 12.7 | API 8.3 | `BE-RWI-168` | `FE-RWI-188` | Dokter tanpa penugasan 403 | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-168](../task/report/backend/BE-RWI-168.md)); frontend `FE-RWI-188` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-065` Pemakaian masuk invoice lewat jalur tindakan | `RWI-DEC-179` (7), `192` (4) | Integrasi 9.2 | API 8.3 | `BE-RWI-168` (bergantung `integrasi-billing` `BE-RWI-155`) | — | Proses bisnis dengan Billing sungguhan (`AC-RWF-061`) | ✅ Backend selesai 5 Oktober 2026 ([BE-RWI-168](../task/report/backend/BE-RWI-168.md)); verifikasi runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026 |
| `FR-RWF-066` Koreksi/pembatalan hanya charge sendiri, selama `OPEN` | `RWI-DEC-179` (5, 7) | Backend 12.3 (`INV-RWF-14`) | API 8.3 | `BE-RWI-168` | `FE-RWI-188` | `UAT-RWF-27`; charge Farmasi tak tersentuh | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-168](../task/report/backend/BE-RWI-168.md)); frontend `FE-RWI-188` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-067` Alkes kecil tetap di Farmasi | `RWI-DEC-179` (9) | — | — | — | `FE-RWI-182` | Manual sub-tab Alat Kesehatan | Belum dikerjakan |
| `FR-RWF-068` Kepemilikan data alat | `RWI-DEC-180` | Backend 12.4 | — | `BE-RWI-167`, `BE-RWI-168` | — | Review diff: master di `MasterData`, pemakaian di `ClinicalManagement` | ✅ Backend selesai 5 Oktober 2026 ([BE-RWI-167](../task/report/backend/BE-RWI-167.md), [BE-RWI-168](../task/report/backend/BE-RWI-168.md)); verifikasi runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026 |
| `FR-RWF-069` Pemakaian berjalan ditutup saat keluar ruangan | `RWI-DEC-179` (6) | Integrasi `INT-RWF-06` | API 8.3; `integrasi-billing` API 3.2 | `BE-RWI-168` (bergantung `BE-RWI-153`) | `FE-RWI-188` | Proses bisnis keluar ruangan dengan alat `Running` | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-168](../task/report/backend/BE-RWI-168.md)); frontend `FE-RWI-188` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-083` Surveilans per kasus operasi | `RWI-DEC-202` | Backend 12.6.3 (`INV-RWF-15`, `16`) | API 8.5; integrasi 9.3 | `BE-RWI-169` | `FE-RWI-189` | `UAT-RWF-18`, `28` (`AC-RWF-083`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-169](../task/report/backend/BE-RWI-169.md)); frontend `FE-RWI-189` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-084` Hari ke-1 dan suhu dari tanda vital | `RWI-DEC-202` (3) | Integrasi 9.3 | API 8.5 | `BE-RWI-169` | `FE-RWI-189` | Contoh tanggal dan suhu 38,5 °C | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-169](../task/report/backend/BE-RWI-169.md)); frontend `FE-RWI-189` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-091` Pengisi dan peninjau surveilans | `RWI-DEC-202` (4) | Backend 12.7 | API 8.5 (`Review`) | `BE-RWI-169` | `FE-RWI-189` | Tanda dicurigai hanya oleh pemegang `Review` (`AC-RWF-095`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-169](../task/report/backend/BE-RWI-169.md)); frontend `FE-RWI-189` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-092` Surveilans berhenti saat keluar ruangan | `RWI-DEC-202` | Integrasi 9.3 | API 8.5 | `BE-RWI-169` | `FE-RWI-189` | Pasien keluar hari ke-5 (`AC-RWF-096`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-169](../task/report/backend/BE-RWI-169.md)); frontend `FE-RWI-189` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-085` Monitoring transfusi per kantong | `RWI-DEC-203`, `209` | Backend 12.6.4 (`INV-RWF-17`) | API 8.6, 8.7 | `BE-RWI-170`, `BE-RWI-171` | `FE-RWI-190`, `FE-RWI-191` | `UAT-RWF-19` disaksikan Bank Darah; `UAT-RWF-29` (`AC-RWF-084`, `097`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-170](../task/report/backend/BE-RWI-170.md), [BE-RWI-171](../task/report/backend/BE-RWI-171.md)); frontend `FE-RWI-190`, `FE-RWI-191` belum dikerjakan; UAT runtime `NOT RUN` |
| `FR-RWF-093` Ketertiban titik ukur | `RWI-DEC-203` | Backend 12.3 (`INV-RWF-18`) | API 8.6 | `BE-RWI-170` | `FE-RWI-190` | Titik terlambat tanpa keterangan ditolak (`AC-RWF-098`) | 🟡 Backend ✅ 5 Oktober 2026 ([BE-RWI-170](../task/report/backend/BE-RWI-170.md)); frontend `FE-RWI-190` belum dikerjakan; UAT runtime `NOT RUN` |
| `RWI-DEC-211`, `212`, `216` Letak layar | — | FE 11.7 | — | — | `FE-RWI-189`, `FE-RWI-190` | `UAT-RWF-34` (`RWI-AC-339`) | Belum dikerjakan |

## 2. Definition of Done PRD → bukti

| Butir DoD (`04-prd-to-mvp.md` 23.19) | Task | Bukti |
|---|---|---|
| Delapan menu keperawatan tanpa *placeholder* kecuali Rehab Medik | `FE-RWI-180`, `182`, `185`, `188`; Pemesanan Ruangan Bedah `episode-rawat-inap` `FE-RWI-193`; Gizi dan Bank Darah `dokter-rawat-inap` `FE-RWI-174` | Manual delapan menu (`AC-RWF-050`, `056`) |
| Satu data cairan untuk Spooling Cairan, Pengawasan Harian, dan WSD | `BE-RWI-165`, `FE-RWI-183` | `AC-RWF-051`, `INV-RWF-10` lewat verifikasi API dan review diff; Backend ✅ [BE-RWI-165](../task/report/backend/BE-RWI-165.md); `FE-RWI-183` belum |
| Pemakaian alat tertagih dari master tarif dan tidak menyentuh tagihan Farmasi | `BE-RWI-168` | `UAT-RWF-06`; `AC-RWF-061`; Backend ✅ [BE-RWI-168](../task/report/backend/BE-RWI-168.md); `UAT-RWF-06` `NOT RUN` |
| Master alat dan tarifnya terisi di lingkungan uji | `BE-RWI-167`, `FE-RWI-187` + pengisian data | `02-backend-architecture.md` 12.12; prasyarat lingkungan; Backend ✅ [BE-RWI-167](../task/report/backend/BE-RWI-167.md); `FE-RWI-187` dan pengisian data belum |
| Surveilans terbentuk, berhenti, dan menandai dicurigai lewat register nosokomial | `BE-RWI-169`, `FE-RWI-189` | `UAT-RWF-18`, `UAT-RWF-28`; Backend ✅ [BE-RWI-169](../task/report/backend/BE-RWI-169.md); UAT `NOT RUN` |
| Formulir surveilans disahkan sebelum dipakai pasien sungguhan | Bukan task kode | Versi instrumen `Approved` beserta nama pengesah (gate G-21) |
| Monitoring transfusi dan pemberitahuan reaksi terbukti di Bank Darah | `BE-RWI-170`, `171`; `FE-RWI-190`, `191` | `UAT-RWF-19` disaksikan petugas Bank Darah; Backend ✅ [BE-RWI-170](../task/report/backend/BE-RWI-170.md), [BE-RWI-171](../task/report/backend/BE-RWI-171.md); UAT `NOT RUN` |
| Diet atas instruksi terverifikasi dokter | `BE-RWI-166`, `FE-RWI-184`; `dokter-rawat-inap` `FE-RWI-175` | `UAT-RWF-26`; Backend ✅ [BE-RWI-166](../task/report/backend/BE-RWI-166.md); UAT `NOT RUN` |
| Regresi alur Gizi dan Bank Darah yang sudah ada nol | `BE-RWI-166`, `BE-RWI-171` | Verifikasi proses bisnis regresi `RWI-AC-303`; Penelusuran source ✅ [BE-RWI-166](../task/report/backend/BE-RWI-166.md), [BE-RWI-171](../task/report/backend/BE-RWI-171.md); regresi runtime `NOT RUN` |

## 3. Gap dan catatan

| ID | Gap | Penanganan | Pemilik |
|---|---|---|---|
| TRC-RWF-02 | Empat isian baru formulir master tarif — jenis alat (API 8.2) dan tiga isian komponen operasi (`episode-rawat-inap` API 11.9) — tidak disebut kontrak frontend mana pun, padahal tarif alat dan tarif komponen operasi hanya dapat diisi lewat formulir itu | Dicakup `FE-RWI-187` sebagai akibat langsung kontrak; disebut pada revisi kontrak frontend berikutnya | Muhammad Hamzah |
| G-21, G-22 | Isi formulir surveilans, titik ukur, toleransi terlambat, dan pemilik PPI belum disahkan klinis | Gerbang produksi, bukan task; desain dan build tetap berjalan | Pemilik klinis / komite PPI |
| G-27 | Satu kwitansi untuk kunjungan asal dan rawat inap | Dependency luar `billing-kasir` | Yasmina |
| — | Pemilihan kantong maju dari `MVP-4` ke `MVP-3` (ikut `BE-RWI-170`) | Catatan perencanaan; pemilik dapat menolak saat approval roadmap | Muhammad Hamzah |
| — | Satuan dan pembulatan setiap jenis alat | Disahkan pemilik tarif sebelum master diisi | Pemilik tarif |

Tidak adanya automated test backend **bukan** gap (`rules/backend/TEST_POLICY.md`).
