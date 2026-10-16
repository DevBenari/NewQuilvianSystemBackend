# Traceability Delivery Integrasi Denah ↔ Bed Management

| Metadata | Nilai |
| --- | --- |
| blueprint_id / planning_id | RWI-BP-001 / `DNH-DEL-20261016-01` |
| revision / tanggal / status | 1 / 2026-10-16 Asia/Jakarta / APPROVED_DESIGN; 9 task backend ✅ selesai 16 Oktober 2026 (`BE-RWI-232` s.d. `240`, laporan `task/report/backend/`); 8 task frontend belum dikerjakan |
| approval | Desain revision 12 / kontrak `0.13.0` — Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`). Aturan keras frontend `RWI-DEC-325` |
| kontrak | `0.13.0` (isi Integrasi Denah); isi Bed Management `0.12.0` mengikuti delivery `BM-DEL-20261011-01` |
| masukan canonical | Decision log 51; gate 1.13; module map 7; manifest anak 12; impact scan `BM-IMP-20261015-01` rev 1 |
| source backend / frontend | `ca09cae909597442385ce4513be9c803549be10e` / `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| hash snapshot | [integrasi-denah-delivery-lock.json](integrasi-denah-delivery-lock.json); 22 masukan + 2 manifest sebelum registrasi roadmap |
| roadmaps | [backend](backend-roadmap-integrasi-denah.md) / [frontend](frontend-roadmap-integrasi-denah.md) |

## Identitas Scope dan Bukti Hulu

Semua ID keputusan dan AC pada dokumen ini berarti **Integrasi Denah ↔ Bed Management, decision log revision 51**. Kunci lengkapnya `episode-rawat-inap/integrasi-denah@51@<sha256 decision log>:<ID>`. Hash manifest dicatat **sebelum registrasi roadmap** supaya tidak terjadi hash yang merujuk dirinya sendiri. Masukan wajib tetap sama sampai relock; perubahan sesudah approval membuat revision baru dan memicu impact scan.

| Masukan relatif dari roadmap | Revision / kegunaan | SHA256 |
| --- | --- | --- |
| ../../00-interview-decisions.md | 51; RWI-DEC-298–325, RWI-AC-436–461 | 739b01c27cd41df0187336e2d8858722588631bb6ba630a047a0d3539bfb57e1 |
| ../../evidence/02-requirement-completeness-gate.md | 1.13; DNH-RCG-20261015-01 | 54da1ceefb103b691ea87390b841b20fef866d130edb099e86e7c4fa8879f738 |
| ../../02-module-map.md | 7 approved; kepemilikan, menu, urutan migration | 8ba242ec25e42fda4da8697197c6c657faebb555f9fee938bc448140fd1ca620 |
| ../../evidence/denah-bed-impact-scan-20261015.md | BM-IMP-20261015-01 rev 1 | 5ec075d8fe708c5c575bcc5b52715450de60aa9282f70d548f211f4f2b9f0669 |
| ../../evidence/denah-bed-source-snapshot-20261015.json | snapshot source BE/FE | 05d997365ca75ad2d3c53689d911422871eba0ae5aaf8aab97c8af2db8ab58f1 |
| ../docs/denah-map-ruangan-rs-mmc/rencana-integrasi-denah-bed.md | rencana integrasi; register Fase 0 | d1b0ce8d9a0db73b38f32dd904b442590b54f21de6f92c68a6acfad729fa5aa3 |
| ../docs/denah-map-ruangan-rs-mmc/rencana-kelas-kamar.md | kelas kamar denah | 2b8f86ce0525b532eb86748544ecf78b498350dea5b8e09594cb56da49832609 |
| ../02-backend-architecture.md | approved 0.13.0; bagian 15 | e15de1bb2cb1bb4375f8d5719e5d487cb5d1169da24bf3c67fe9f759b8d3f6ae |
| ../data/data-dictionary.md | approved 0.13.0; bagian 22 | eed76fe1e3252bf8e7eea1437dfdbeaa3b552ea7cec2c90220390713bd83b6b7 |
| ../contracts/api-contract.md | approved 0.13.0; bagian 14 | d94a0ea46ddfb0e18392d547480f9d0f19ccb0c4527d588faaa5b85e7039f075 |
| ../contracts/permission-audit-matrix.md | approved 0.13.0; bagian 12 | 3ddc6c84ef3aa3da39cdd277e302b77219c86444810556c3a1ed4a665f3fcf7d |
| ../contracts/validation-matrix.md | approved 0.13.0; bagian 17 | 637d3d54781dbeafb6312e2a8a6a30cd49f41ca34d8a646e59ad5c5b84f77c20 |
| ../contracts/state-transition-matrix.md | approved 0.13.0; bagian 12 | f8b3f20d3c28bd38135e8b405d78b3c432a69dffda8d01fcfa757cb53ef690b2 |
| ../contracts/integration-contract.md | approved 0.13.0; bagian 12 | a317131a351ee74306a59d8f6e94ab9c36cd333008ef116cf4b9d1d981fe6acb |
| ../03-frontend-architecture.md | approved 0.13.0; bagian 16 + aturan keras 16.7 | 590f6b2871462da36da123d8509c8f125c89014fdaf3fde6422d012def90ec57 |
| ../testing/acceptance-test-matrix.md | approved 0.13.0; bagian 23 | 7c0cd9f50e7355955658b32e878ef8aac7b79853e5d49b5ff07751e047494d9a |
| ../04-prd-to-mvp.md | approved 0.13.0; bagian 27 | e7c41612db6248bf19fab384fdd780b93b8f28fd2d779c3ae36dde9f106d78d0 |
| ../flowcharts/00-alur-utama.md | approved 0.13.0 | 46f12d164ca387b1890c495a7b4a035cae5906f41b0c54af2489bad1fdba79be |
| ../flowcharts/15-denah-pilih-bed.md | approved 0.13.0 | 806f143b8cc7977641f0aa05623d1083a1bc258974a2f58d1a97cede60817693 |
| ../flowcharts/16-denah-master-bed.md | approved 0.13.0 | a2bfeeed93b7cb8c60024131de25032dd8d65df85822d0056cfb4bb6462647ce |
| ../flowcharts/17-impor-denah.md | approved 0.13.0 | 6bd9114be0c548a860c0ac1e0588117914d672924fd94c376bb7656520703a68 |
| ../flowcharts/18-ketergantungan-pasien.md | approved 0.13.0 | cfb57c3d0b9ee01ecc97e0bc222aca1ac5323e69726d2335abeda07aa0af6285 |
| ../blueprint-manifest.md | manifest anak revision 12 approved, sebelum registrasi roadmap | 4b9355371f586d44916bbda651710dfafd2c935708cf17c1dcbc0d6845a8cb4d |
| ../../blueprint-manifest.md | manifest induk revision 11, sebelum registrasi roadmap | 2397a0bcd3a48e189f53bc676ed3f6f11e9e144ff60cf1a20cc37c360e45cb67 |

## Requirement Fungsional ke Task dan Bukti

Bukti backend memakai requirement-ke-bukti-verifikasi menurut `rules/backend/TEST_POLICY.md`; tidak adanya automated test backend bukan gap. Seluruh status bukti **NOT_RUN** sampai task dikerjakan.

| Requirement | Perilaku terukur | Desain / kontrak | Backend | Frontend | Bukti diharapkan / status |
| --- | --- | --- | --- | --- | --- |
| FR-DNH-001 | Monitoring menampilkan denah per lantai; status kamar, badge, dan outline dihitung dari bed; kamar penuh tanpa merah dan tetap dapat diklik | BE 15.7; FE 16; API 14 | `BE-RWI-238` | `FE-RWI-246`, `FE-RWI-247` | DNH-AT-442, 453; UAT-801; backend PASS (source + build 16 Oktober 2026: [BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend/UAT NOT_RUN |
| FR-DNH-002 | Pop up kamar memuat bed, status, penghuni sesuai hak, sisa waktu pesanan, dan aksi pembersihan | BE 15.7; FE 16.3 | `BE-RWI-238` | `FE-RWI-247` | DNH-AT-443, 904; UAT-801, 802; backend PASS (source + build 16 Oktober 2026: [BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend/UAT NOT_RUN |
| FR-DNH-003 | Kartu dan tabel bed dihapus; endpoint `bed-board` tetap untuk daftar pasien | FE 16.4 | — | `FE-RWI-247`, `FE-RWI-253` | DNH-AT-436; NOT_RUN |
| FR-DNH-004 | Hitungan boks rawat gabung pada badge dan outline Monitoring | PRD 27.10 | — | — | **OPEN DECISION** RWI-OQ-144; belum ada task |
| FR-DNH-005 | Pilih Bed admisi (termasuk bayi) dan tujuan transfer lewat denah dengan kelayakan per pasien | BE 15.7; API 14 | `BE-RWI-239` | `FE-RWI-248`, `FE-RWI-249` | DNH-AT-449, 453, 454, 907; UAT-803, 804; backend PASS (source + build 16 Oktober 2026: [BE-RWI-239](../task/report/backend/BE-RWI-239.md)); frontend/UAT NOT_RUN |
| FR-DNH-006 | Tidak ada pemeriksaan, field, maupun tampilan jenis kelamin pada bed | BE 15.2; API 14.1 | `BE-RWI-240` | `FE-RWI-253` | DNH-AT-439, 452, 912; backend PASS (source + build 16 Oktober 2026: [BE-RWI-240](../task/report/backend/BE-RWI-240.md)); frontend/UAT NOT_RUN |
| FR-DNH-007 | Transfer yang melibatkan kelas khusus menerima kategori apa pun | BE 15.7 | `BE-RWI-233` | `FE-RWI-249` | DNH-AT-450, 458; UAT-804, 811; backend PASS (source + build 16 Oktober 2026: [BE-RWI-233](../task/report/backend/BE-RWI-233.md)); frontend/UAT NOT_RUN |
| FR-DNH-008 | Kamar menyimpan lantai, wing, kunci; nama otomatis; kode sistem; label bed | BE 15.6; data 22 | `BE-RWI-232`, `BE-RWI-235` | `FE-RWI-250` | DNH-AT-451, 456, 901; backend PASS (source + build 16 Oktober 2026: [BE-RWI-232](../task/report/backend/BE-RWI-232.md), [BE-RWI-235](../task/report/backend/BE-RWI-235.md)); frontend/UAT NOT_RUN |
| FR-DNH-009 | Editor master: ubah kelas kamar kosong, tambah/kurangi bed atau boks, wing, label, ruang cadangan | BE 15.1, 15.4 | `BE-RWI-235`, `BE-RWI-236` | `FE-RWI-250` | DNH-AT-444, 454, 457, 902, 903, 913; UAT-805, 806; backend PASS (source + build 16 Oktober 2026: [BE-RWI-235](../task/report/backend/BE-RWI-235.md), [BE-RWI-236](../task/report/backend/BE-RWI-236.md)); frontend/UAT NOT_RUN |
| FR-DNH-010 | Bed baru dan bed sesudah ubah kelas Tidak Tersedia sampai disahkan perawat | BE 15.2 | `BE-RWI-235`, `BE-RWI-236` | `FE-RWI-250` | DNH-AT-455; backend PASS (source + build 16 Oktober 2026: [BE-RWI-235](../task/report/backend/BE-RWI-235.md), [BE-RWI-236](../task/report/backend/BE-RWI-236.md)); frontend/UAT NOT_RUN |
| FR-DNH-011 | Impor dengan pemeriksaan, pemetaan unit wajib, idempoten, laporan per kamar; kamar existing tidak diubah | BE 15.4; API 14 | `BE-RWI-237` | `FE-RWI-251` | DNH-AT-446, 461, 905; UAT-807, 808; backend PASS (source + build 16 Oktober 2026: [BE-RWI-237](../task/report/backend/BE-RWI-237.md)); frontend/UAT NOT_RUN |
| FR-DNH-012 | Kesiapan bed hasil impor per lingkungan; produksi tidak dapat menetapkan siap | BE 15.4; transisi 12.4 | `BE-RWI-237` | `FE-RWI-251` | DNH-AT-459, 906; backend PASS (source + build 16 Oktober 2026: [BE-RWI-237](../task/report/backend/BE-RWI-237.md)); frontend/UAT NOT_RUN |
| FR-DNH-013 | Catat atau ubah HD/LD dengan sumber menurut peran; riwayat tidak ditimpa; nilai basi ditolak | BE 15.4; hak akses 12 | `BE-RWI-234` | `FE-RWI-252` | DNH-AT-440, 908; UAT-809, 810; backend PASS (source + build 16 Oktober 2026: [BE-RWI-234](../task/report/backend/BE-RWI-234.md)); frontend/UAT NOT_RUN |
| FR-DNH-014 | HD/LD tampil per bed dan ringkasan kamar bagi pembaca berhak; tidak membatasi penempatan | BE 15.7 | `BE-RWI-234`, `BE-RWI-238` | `FE-RWI-247`, `FE-RWI-252` | DNH-AT-441, 443; backend PASS (source + build 16 Oktober 2026: [BE-RWI-234](../task/report/backend/BE-RWI-234.md), [BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend/UAT NOT_RUN |
| FR-DNH-015 | Kelas resmi diterapkan dengan Id tetap bersama konfigurasi urutan kelas | BE 15.8 (D-DNH-1) | D-DNH-1 (operasional); `BE-RWI-233` | — | DNH-AT-447, 448, 449, 458, 460; UAT-811, 812; backend PASS (source + build 16 Oktober 2026: [BE-RWI-233](../task/report/backend/BE-RWI-233.md)); frontend/UAT NOT_RUN |
| FR-DNH-016 | Nama kamar berkunci mengikuti nama kelas; snapshot lama tetap | BE 15.6 | `BE-RWI-235` | — | DNH-AT-909; backend PASS (source + build 16 Oktober 2026: [BE-RWI-235](../task/report/backend/BE-RWI-235.md)); frontend/UAT NOT_RUN |

## Acceptance Criteria ke Task

| AC | Kriteria | Decision | Backend / langkah | Frontend | Skenario | Status |
| --- | --- | --- | --- | --- | --- | --- |
| RWI-AC-436 | Satu denah di empat tempat; kartu bed tidak ada lagi | RWI-DEC-299, 320 | `BE-RWI-238`, `BE-RWI-239` (data) | `FE-RWI-246`–`250`, `FE-RWI-253` | DNH-AT-436 | Backend PASS ([BE-RWI-238](../task/report/backend/BE-RWI-238.md), [BE-RWI-239](../task/report/backend/BE-RWI-239.md)); frontend NOT_RUN |
| RWI-AC-437 | Superseded oleh RWI-AC-452 | RWI-DEC-301 | — | — | — | SUPERSEDED |
| RWI-AC-438 | Superseded oleh RWI-AC-452 | RWI-DEC-301 | — | — | — | SUPERSEDED |
| RWI-AC-439 | Master tanpa penanda jenis kelamin | RWI-DEC-315 | `BE-RWI-240` | `FE-RWI-253` | DNH-AT-439 | Backend PASS ([BE-RWI-240](../task/report/backend/BE-RWI-240.md)); frontend NOT_RUN |
| RWI-AC-440 | Ubah HD/LD dengan pelaku, waktu, sumber, riwayat; tanpa hak 403 | RWI-DEC-302 | `BE-RWI-234` | `FE-RWI-252` | DNH-AT-440 | Backend PASS ([BE-RWI-234](../task/report/backend/BE-RWI-234.md)); frontend NOT_RUN |
| RWI-AC-441 | Pasien HD dapat dipesankan bed layak mana pun | RWI-DEC-302 | `BE-RWI-234`, `BE-RWI-239` | `FE-RWI-252` | DNH-AT-441 | Backend PASS ([BE-RWI-234](../task/report/backend/BE-RWI-234.md), [BE-RWI-239](../task/report/backend/BE-RWI-239.md)); frontend NOT_RUN |
| RWI-AC-442 | Status kamar dan badge; kamar penuh tanpa merah tetap diklik | RWI-DEC-303 | `BE-RWI-238` | `FE-RWI-247` | DNH-AT-442 | Backend PASS ([BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend NOT_RUN |
| RWI-AC-443 | Ringkasan kamar "High Dependency (1 HD · 1 LD)" | RWI-DEC-303 | `BE-RWI-238` | `FE-RWI-247`, `FE-RWI-252` | DNH-AT-443 | Backend PASS ([BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend NOT_RUN |
| RWI-AC-444 | Ubah kelas kamar berpenghuni ditolak dengan jumlah penghuni | RWI-DEC-304 | `BE-RWI-235` | `FE-RWI-250` | DNH-AT-444 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)); frontend NOT_RUN |
| RWI-AC-445 | Pembersihan DB Hamzah dengan cadangan, dry run, konfirmasi | RWI-DEC-305 | D-DNH-2 (operasional) | — | DNH-AT-445 | PASS — eksekusi atomik tuntas 16 Oktober 2026, backup 1,47 MB, 35 episode bersih |
| RWI-AC-446 | Impor dua kali: run kedua 0 baris baru | RWI-DEC-306 | `BE-RWI-237` | `FE-RWI-251` | DNH-AT-446 | Backend PASS ([BE-RWI-237](../task/report/backend/BE-RWI-237.md)); runtime D-DNH-3 PASS (106 kamar & 168 bed terbuat, run kedua 106 kamar & 168 bed unchanged, 0 baru); frontend NOT_RUN |
| RWI-AC-447 | 12 kelas resmi + nama kamar baru | RWI-DEC-307, 308 | D-DNH-1; `BE-RWI-235` | — | DNH-AT-447 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)); D-DNH-1 NOT_RUN |
| RWI-AC-448 | SVIP 318/401/418 menjadi VIP Superior | RWI-DEC-310 | D-DNH-1, D-DNH-3 | — | DNH-AT-448 | NOT_RUN |
| RWI-AC-449 | Perinatologi untuk Ruang Bayi; boks dapat dipesan episode bayi | RWI-DEC-311 | D-DNH-1; `BE-RWI-239` | `FE-RWI-248` | DNH-AT-449 | Backend PASS ([BE-RWI-239](../task/report/backend/BE-RWI-239.md)); D-DNH-1 NOT_RUN; frontend NOT_RUN |
| RWI-AC-450 | Kelas 3 → ICU kategori apa pun; Kelas 2 → Kelas 1 Down ditolak | RWI-DEC-312 | `BE-RWI-233` | `FE-RWI-249` | DNH-AT-450 | Backend PASS ([BE-RWI-233](../task/report/backend/BE-RWI-233.md)); frontend NOT_RUN |
| RWI-AC-451 | Delapan arah wing; arah bawaan; admin mengubah | RWI-DEC-313, 314 | `BE-RWI-232`, `BE-RWI-236` | `FE-RWI-250`, `FE-RWI-251` | DNH-AT-451 | Backend PASS ([BE-RWI-232](../task/report/backend/BE-RWI-232.md), [BE-RWI-236](../task/report/backend/BE-RWI-236.md)); frontend NOT_RUN |
| RWI-AC-452 | Kamar campur tanpa peringatan; tidak ada tampilan jenis kelamin | RWI-DEC-315 | `BE-RWI-240` | `FE-RWI-253` | DNH-AT-452 | Backend PASS ([BE-RWI-240](../task/report/backend/BE-RWI-240.md)); frontend NOT_RUN |
| RWI-AC-453 | Outline hijau Monitoring dan per pasien saat admisi | RWI-DEC-316 | `BE-RWI-238`, `BE-RWI-239` | `FE-RWI-247`, `FE-RWI-248`, `FE-RWI-249` | DNH-AT-453 | Backend PASS ([BE-RWI-238](../task/report/backend/BE-RWI-238.md), [BE-RWI-239](../task/report/backend/BE-RWI-239.md)); frontend NOT_RUN |
| RWI-AC-454 | Boks bayi dari editor; hanya pasien bayi | RWI-DEC-299; RWI-RULE-014 | `BE-RWI-235`, `BE-RWI-239` | `FE-RWI-248`, `FE-RWI-250` | DNH-AT-454 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md), [BE-RWI-239](../task/report/backend/BE-RWI-239.md)); frontend NOT_RUN |
| RWI-AC-455 | Bed sesudah ubah kelas Tidak Tersedia sampai disahkan | RWI-DEC-282, 285 | `BE-RWI-235` | `FE-RWI-250` | DNH-AT-455 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)); frontend NOT_RUN |
| RWI-AC-456 | Kode sistem `BD-RSMMC-#####`, label "329-D" | RWI-DEC-317 | `BE-RWI-235` | `FE-RWI-250` | DNH-AT-456 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)); frontend NOT_RUN |
| RWI-AC-457 | Tambah bed di kamar berpenghuni: `3/2` menjadi `4/2` | RWI-DEC-318 | `BE-RWI-235`, `BE-RWI-238` | `FE-RWI-250` | DNH-AT-457 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md), [BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend NOT_RUN |
| RWI-AC-458 | Kelas dan konfigurasi urutan diterapkan bersamaan | RWI-DEC-312, 319 | `BE-RWI-233`; D-DNH-1 | — | DNH-AT-458 | Backend PASS ([BE-RWI-233](../task/report/backend/BE-RWI-233.md)); D-DNH-1 NOT_RUN |
| RWI-AC-459 | Kesiapan bed hasil impor per lingkungan | RWI-DEC-321 | `BE-RWI-237` | `FE-RWI-251` | DNH-AT-459 | Backend PASS ([BE-RWI-237](../task/report/backend/BE-RWI-237.md)); frontend NOT_RUN |
| RWI-AC-460 | "Kelas I" menjadi "Kelas 1" dengan Id sama; tarif tetap | RWI-DEC-322 | D-DNH-1; `BE-RWI-235` | — | DNH-AT-460 | Backend PASS ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)); D-DNH-1 NOT_RUN |
| RWI-AC-461 | Kamar tanpa unit ditolak; denah lantai menampilkan semua kamar lintas unit | RWI-DEC-323 | `BE-RWI-237`, `BE-RWI-238` | `FE-RWI-251` | DNH-AT-461 | Backend PASS ([BE-RWI-237](../task/report/backend/BE-RWI-237.md), [BE-RWI-238](../task/report/backend/BE-RWI-238.md)); frontend NOT_RUN |

## Skenario Aturan Desain

| Skenario | Kasus | Task | Status |
| --- | --- | --- | --- |
| DNH-AT-901 | `DNH-DES-01` kode kamar sesudah hapus lunak | `BE-RWI-232` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-232](../task/report/backend/BE-RWI-232.md)) |
| DNH-AT-902 | Label unik per lantai | `BE-RWI-235` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)) |
| DNH-AT-903 | Hapus bed hanya tanpa riwayat | `BE-RWI-235` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)) |
| DNH-AT-904 | Privasi kedua endpoint denah | `BE-RWI-238`, `BE-RWI-239` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-238](../task/report/backend/BE-RWI-238.md), [BE-RWI-239](../task/report/backend/BE-RWI-239.md)) |
| DNH-AT-905 | Impor tidak mengubah kamar existing | `BE-RWI-237` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-237](../task/report/backend/BE-RWI-237.md)) |
| DNH-AT-906 | Validator startup impor | `BE-RWI-237` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-237](../task/report/backend/BE-RWI-237.md)) |
| DNH-AT-907 | Tujuan denah vs status episode | `BE-RWI-239` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-239](../task/report/backend/BE-RWI-239.md)) |
| DNH-AT-908 | HD/LD bersamaan | `BE-RWI-234` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-234](../task/report/backend/BE-RWI-234.md)) |
| DNH-AT-909 | Ganti nama kelas → nama kamar | `BE-RWI-235` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-235](../task/report/backend/BE-RWI-235.md)) |
| DNH-AT-910 | Beban query denah per lantai | `BE-RWI-238` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-238](../task/report/backend/BE-RWI-238.md)) |
| DNH-AT-911 | Tampilan denah tidak berubah | `FE-RWI-246` | NOT_RUN |
| DNH-AT-912 | Gelombang jenis kelamin satu rilis | `BE-RWI-240`, `FE-RWI-253` | Backend PASS ([BE-RWI-240](../task/report/backend/BE-RWI-240.md)); rilis bersama `FE-RWI-253` NOT_RUN |
| DNH-AT-913 | Aktivasi ruang cadangan atomik | `BE-RWI-236` | PASS — penelusuran source + build 16 Oktober 2026 ([BE-RWI-236](../task/report/backend/BE-RWI-236.md)) |

## Non-Fungsional dan UAT

| NFR | Kebutuhan | Task |
| --- | --- | --- |
| NFR-801 | Tampilan denah tidak berubah | `FE-RWI-246` (dan seluruh task frontend) |
| NFR-802 | Privasi penghuni | `BE-RWI-238`, `BE-RWI-239`, `FE-RWI-247` |
| NFR-803 | Beban baca per lantai | `BE-RWI-238`, `BE-RWI-239` |
| NFR-804 | Atomik dan aman diulang | `BE-RWI-234`, `BE-RWI-236`, `BE-RWI-237` |
| NFR-805 | Kompatibilitas gelombang jenis kelamin | `BE-RWI-240`, `FE-RWI-253` |
| NFR-806 | Migration aman | `BE-RWI-232`, `BE-RWI-234`, `BE-RWI-237`, `BE-RWI-240` |
| NFR-807 | Aksesibilitas denah | `FE-RWI-246`, `FE-RWI-247` |

| UAT | Epic | Task / langkah |
| --- | --- | --- |
| UAT-801, 802 | EPIC DNH-01 | `FE-RWI-247` + `BE-RWI-238` |
| UAT-803, 804 | EPIC DNH-02 | `FE-RWI-248`, `FE-RWI-249` + `BE-RWI-239`, `BE-RWI-233` |
| UAT-805, 806 | EPIC DNH-03 | `FE-RWI-250` + `BE-RWI-235`, `BE-RWI-236` |
| UAT-807, 808 | EPIC DNH-04 | `FE-RWI-251` + `BE-RWI-237`; D-DNH-2, D-DNH-3 |
| UAT-809, 810 | EPIC DNH-05 | `FE-RWI-252` + `BE-RWI-234` |
| UAT-811, 812 | EPIC DNH-06 | D-DNH-1 + `BE-RWI-233`, `BE-RWI-235`; tarif RWI-OQ-143 |

## Kepemilikan Endpoint

Base URL mengikuti API contract 14.2 (`api/v1/health-services/inpatient-management/...` dan `api/v1/health-services/master-data/...`). Endpoint existing yang dipakai tanpa perubahan kontrak — pembersihan, verifikasi kesiapan, tutup/buka bed, status bed, `transfer-context`, dan `bed-board` — tidak dimiliki task mana pun.

| Method | Path | Tag | Task | Perubahan |
| --- | --- | --- | --- | --- |
| GET | `bed-management/floor-plan` | Bed Management | `BE-RWI-238` | baru |
| GET | `bed-management/monitoring` | Bed Management | `BE-RWI-238` | saring lantai, No. RM, HD/LD |
| GET | `bed-occupancies/floor-plan` | Bed Occupancy | `BE-RWI-239` | baru |
| GET | `bed-occupancies/available-beds` | Bed Occupancy | `BE-RWI-239` | saring lantai; field jenis kelamin dihapus oleh BE-RWI-240 |
| POST | `bed-occupancies/placements/transfer` | Bed Occupancy | `BE-RWI-233` | kelas khusus; aturan jenis kelamin dihapus oleh BE-RWI-240 |
| POST | `bed-occupancies/reservations` | Bed Occupancy | `BE-RWI-240` | aturan jenis kelamin dihapus |
| POST | `bed-occupancies/placements` | Bed Occupancy | `BE-RWI-240` | aturan jenis kelamin dihapus |
| PATCH | `episodes/{id}/care-dependency` | Inpatient Episode | `BE-RWI-234` | baru |
| GET | `episodes/{id}/care-dependency-history` | Inpatient Episode | `BE-RWI-234` | baru |
| GET | `episodes/{id}` | Inpatient Episode | `BE-RWI-234` | lima field HD/LD |
| GET | `rooms/floor-plan-layout` | Room | `BE-RWI-236` | baru |
| POST | `rooms/floor-plan-layout/rooms` | Room | `BE-RWI-236` | baru |
| POST | `rooms/floor-plan-layout/import` | Room | `BE-RWI-237` | baru |
| GET | `rooms` | Room | `BE-RWI-232` | saring lantai dan kunci |
| POST | `rooms` | Room | `BE-RWI-232` | tiga field, pengalokasi kode |
| PUT | `rooms/{id}` | Room | `BE-RWI-232` | wing, kunci tetap; turunan ubah kelas oleh BE-RWI-235 |
| POST | `beds` | Bed | `BE-RWI-235` | label server |
| PUT | `beds/{id}` | Bed | `BE-RWI-235` | ubah label |
| DELETE | `beds/{id}` | Bed | `BE-RWI-235` | hanya tanpa riwayat |
| PUT | `patient-classes/{id}` | Patient Class | `BE-RWI-235` | nama kamar ikut nama kelas |

## Gap dan Gerbang

| Butir | Jenis | Dampak | Pemilik |
| --- | --- | --- | --- |
| FR-DNH-004 boks rawat gabung | OPEN DECISION RWI-OQ-144 | Belum ada task; badge memakai `Counts` server apa adanya | Product owner |
| FR-DNH-015 kelas resmi | Langkah operasional D-DNH-1, bukan task source | Bukti runtime kelas khusus, nama kamar, impor | Seluruh tim MasterData + pemilik konfigurasi; `DNH-G01` |
| Data uji denah | D-DNH-2, D-DNH-3 | Verifikasi dengan data nyata seluruh task frontend | Pemilik DB pribadi; izin eksekusi terpisah; `DNH-UNK-01`, `02` |
| Tarif kelas/unit baru | RWI-OQ-143 | UAT-812 | Billing (Yasmina); `DNH-G02` |
| Produksi | `DNH-G03`, `G04`, `G05` | Impor produksi, HD/LD produksi, pembukaan bangsal | RS, pemilik klinis/privasi, perawat verifikator |

## Validasi Dokumen Planning

| Pemeriksaan | Hasil |
| --- | --- |
| Grafik vs kolom Dependency | Backend 9 pasangan = 9 entri; frontend 13 pasangan = 13 entri; diurai otomatis mengikuti arah aliran |
| Siklus dan kemunculan | Tanpa siklus; setiap task muncul tepat satu kali; node luar berlabel `[BE]` |
| Keterbacaan | Backend 9 node, lebar 74; frontend 15 node, lebar 88; setiap garis masukan berakhir di sudut, tanpa garis tegak antar-simpul |
| Ruang ID | Dipindai global 2026-10-16: terpakai sampai `BE-RWI-231`/`FE-RWI-245`; dialokasikan `BE-RWI-232`–`240`, `FE-RWI-246`–`253` |
| Source/database/Git | Tidak disentuh oleh planning ini |
