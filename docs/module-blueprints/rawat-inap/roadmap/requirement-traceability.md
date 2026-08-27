# Requirement Traceability — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
roadmap_revision: 2
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), lewat RWI-DEC-067; sinkronisasi revision 2 lewat RWI-DEC-074"
approved_at: "2026-08-24"
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 6
  04-prd-to-mvp.md: 0.4.0
  testing/acceptance-test-matrix.md: 0.4.0
contract_versions:
  - "API 0.4.0"
  - "State transition 0.4.0"
  - "Validation 0.4.0"
  - "Integration 0.4.0"
  - "Permission/Audit 0.4.0"
  - "Acceptance test 0.4.0"
  - "PRD ke MVP 0.4.0"
counts:
  epic: 14
  functional_requirement: 62
  acceptance_criteria: 139
  uat_scenario: 33
  backend_task: 33
  frontend_task: 19
  api_endpoint_baru: 49
  api_endpoint_perubahan_perilaku: 1
```

---

## 0. Cara memakai dokumen ini

Dokumen ini menjawab satu pertanyaan: **apakah ada requirement yang tidak dikerjakan siapa pun, dan
apakah ada task yang tidak menjawab requirement apa pun?** Keduanya sama berbahayanya — yang pertama
adalah lubang cakupan, yang kedua adalah pekerjaan yang tidak ada yang memintanya.

Arah bacanya dua:

```text
EPIC → FR → task → acceptance criteria → skenario test        (bagian 1 dan 2)
task → EPIC                                                    (bagian 3, arah balik)
```

Baris yang berbunyi "menyusul" **tidak diperbolehkan** di dokumen ini. Bila sesuatu belum dapat
ditelusuri, tulis alasannya dan Decision ID yang menahannya.

---

## 1. Epic → task

| Epic | Isinya | Gelombang | Task backend | Task frontend |
| --- | --- | --- | --- | --- |
| `EPIC RI-21` | Fondasi episode dan data master | `MVP-0` | `BE-RWI-001`, `BE-RWI-002`, `BE-RWI-003`, `BE-RWI-004`, `BE-RWI-007`, `BE-RWI-008` | `FE-RWI-002`, `FE-RWI-006` |
| `EPIC RI-22` | Pencarian dan pemesanan tempat tidur | `MVP-1` | `BE-RWI-010` | `FE-RWI-005` |
| `EPIC RI-23` | Penempatan pasien dan pengaktifan episode | `MVP-1` | `BE-RWI-011`, `BE-RWI-012` | `FE-RWI-007` |
| `EPIC RI-24` | Census dan lama dirawat | `MVP-1` | `BE-RWI-016` | `FE-RWI-008` |
| `EPIC RI-25` | Penanggung jawab episode | `MVP-2` | `BE-RWI-017`, `BE-RWI-018` | `FE-RWI-011` |
| `EPIC RI-26` | Perpindahan pasien dan pindah kelas | `MVP-2` | `BE-RWI-019` | `FE-RWI-010` |
| `EPIC RI-27` | Keputusan pulang dan resume | `MVP-3` | `BE-RWI-020`, `BE-RWI-021`, `BE-RWI-022` | `FE-RWI-012` |
| `EPIC RI-28` | Daftar periksa, kelayakan keuangan, dan penutupan | `MVP-3` | `BE-RWI-023`, `BE-RWI-024`, `BE-RWI-025`, `BE-RWI-026`, `BE-RWI-027` | `FE-RWI-013`, `FE-RWI-014`, `FE-RWI-015` |
| `EPIC RI-29` | Riwayat status dan daftar pantau | `MVP-4` | `BE-RWI-028`, `BE-RWI-029` | `FE-RWI-016`, `FE-RWI-017` |
| `EPIC RI-30` | Sesi koreksi episode | `MVP-4` | `BE-RWI-030` | `FE-RWI-018` |
| `EPIC RI-31` | Pengaturan yang dapat diubah admin | `MVP-0` | `BE-RWI-005` | `FE-RWI-003`, `FE-RWI-004` |
| `EPIC RI-32` | Perbaikan tempat tidur dan pembatasan wewenang status | `MVP-0` | `BE-RWI-006`, `BE-RWI-032` | `FE-RWI-001` |
| `EPIC RI-33` | Bayi baru lahir dan boks bayi | `MVP-4` | `BE-RWI-031` | Tidak ada layar khusus — tercakup census dan penempatan |
| `EPIC RI-34` | Kelayakan penempatan menurut jenis kelamin dan isolasi | `MVP-1` | `BE-RWI-013`, `BE-RWI-014`, `BE-RWI-015` | `FE-RWI-006`, `FE-RWI-007`, `FE-RWI-009`, `FE-RWI-016` |

**Empat belas epic, nol tanpa task.**

Dua task tidak menempel pada epic mana pun, dan itu disengaja: `BE-RWI-033` bukti penerimaan dan
`FE-RWI-019` kesiapan e2e. Keduanya menjawab `NFR-008` dan `RWI-DEC-051`, bukan satu epic tertentu.

---

## 1b. Progres delivery per 26 Agustus 2026

Bagian ini memisahkan dua hal yang sering dicampur: **kode yang selesai**, versus **DoD yang
benar-benar terpenuhi** menurut aturan roadmap sendiri.

| Task | Requirement yang dijawab | Bukti | Status |
| --- | --- | --- | :---: |
| `BE-RWI-001` | Angka batas waktu dan butir administrasi punya tempat tinggal di master, bukan di kode (`RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`) | Build Release lulus; `has-pending-model-changes` bersih; migration **maju dan mundur lulus** pada PostgreSQL 16 lokal sekali pakai; bentuk kolom cocok kolom demi kolom dengan `erd/data-dictionary.md` bagian 12 dan 13; unique `ItemCode` terbukti menolak duplikat di database sungguhan ([laporan](../task/report/backend/be-rwi-001-tabel-master-rawat-inap.md)) | ✅ **Selesai** |
| `BE-RWI-003` | Fondasi data seluruh modul berdiri; empat keadaan mustahil dijadikan mustahil oleh database (`INV-INP-02`, `INV-INP-03`, `INV-INP-10`; `RWI-DEC-054`, `RWI-DEC-055`, `RWI-DEC-065`) | Build Release lulus; `has-pending-model-changes` bersih; migration **maju dan mundur lulus** pada PostgreSQL 16 lokal sekali pakai; 251 kolom cocok kolom demi kolom dengan kamus data; **enam** unique index parsial terbentuk dan **terbukti menolak** pada database sungguhan (sepuluh uji, tujuh penolakan tiga penerimaan); enam test enum lulus ([laporan](../task/report/backend/be-rwi-003-tabel-transaksi-rawat-inap.md)) | ✅ **Selesai** |
| `BE-RWI-002` | Data master awal terisi tanpa seeder mengarang kamar dan tempat tidur, dan seeder menolak berjalan di produksi (`RWI-DEC-048`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: tabel DoD pada laporannya tidak berformat baku, sehingga harus dinilai manual ([laporan](../task/report/backend/be-rwi-002-seeder-master-rawat-inap.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-004` | Angka batas waktu dibaca dari master, bukan ditanam di kode; enam service dapat dibentuk container; nomor episode tidak pernah kembar (`RWI-DEC-008`, `RWI-AC-003`, `RWI-AC-110`, `QBE-CODE-003`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: tabel DoD pada laporannya tidak berformat baku, sehingga harus dinilai manual ([laporan](../task/report/backend/be-rwi-004-enam-service-dan-nomor-episode.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-005` | Pengaturan dan butir administrasi dapat diubah admin lewat layar, dan menonaktifkan butir tidak menghapus penandaan pada episode lama (`RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: tabel DoD pada laporannya tidak berformat baku, sehingga harus dinilai manual ([laporan](../task/report/backend/be-rwi-005-controller-master-rawat-inap.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-007` | Episode lahir bernomor, menempel pada tepat satu kunjungan, dan punya DPJP sejak detik pertama (`RWI-DEC-009`, `RWI-DEC-011`; `INV-INP-03`, `INV-INP-04`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-007-buka-admisi-episode-bernomor.md)) | ✅ **Selesai** |
| `BE-RWI-008` | Isian admisi dapat dibetulkan, admisi dapat dibatalkan beserta pemesanannya, dan `Draft` telantar gugur sendiri saat dibaca (`RWI-DEC-010`, `RWI-DEC-030`; `RWI-RULE-004`, `RWI-RULE-022`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: batas "belum ada catatan klinis" — jalur baca ke `ClinicalManagement` belum ada pada integration contract ([laporan](../task/report/backend/be-rwi-008-ubah-batal-kedaluwarsa-draft.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-006` | Status terisi dan dipesan hanya lahir dari modul Rawat Inap (`RWI-DEC-039`, `RWI-RULE-027`, `RWI-DEC-062`) | **Masih tidak dikerjakan, tetapi alasannya menyempit.** Per 26 Agustus 2026 `FE-RWI-001` sudah selesai dan terbukti di browser sungguhan, dan laporannya ada. Yang belum: perubahan frontend itu **masih lokal, belum di-commit dan belum rilis**. `BedController.cs` tetap tidak disentuh ([laporan blokir](../task/report/backend/be-rwi-006-terblokir-prasyarat-fe-rwi-001.md); [laporan FE-RWI-001](../task/report/frontend/FE-RWI-001.md)) | ⛔ **Terblokir** |
| `BE-RWI-009` | Petugas dapat menemukan episode tanpa menebak; lokasi selalu dibaca dari catatan penempatan, bukan dari kolom pada episode | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: kriteria **403**. Yang terbukti baru **401** tanpa token; membuktikan 403 butuh akun yang login tetapi tidak punya butir hak aksesnya ([laporan](../task/report/backend/be-rwi-009-daftar-dan-detail-episode.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-010` | Tempat tidur terkunci 2 jam lalu bebas sendiri tanpa penjadwal; batas waktunya milik admin (`RWI-DEC-007`, `RWI-DEC-008`; `RWI-RULE-001`, `RWI-RULE-002`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-010-pencarian-dan-pemesanan-tempat-tidur.md)) | ✅ **Selesai** |
| `BE-RWI-011` | Pasien punya lokasi dan tempat tidur ganda mustahil (`INV-INP-01`, `INV-INP-02`; `RWI-DEC-039`, `RWI-AC-147`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: test tabrakan dua transaksi terhadap PostgreSQL belum dijalankan. Index-nya sudah ada, perilakunya belum diuji ([laporan](../task/report/backend/be-rwi-011-penempatan-pasien-dan-inv-inp-02.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-012` | Satu pasien paling banyak satu episode yang benar-benar hadir (`INV-INP-10`; `RWI-DEC-054`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-012-satu-pasien-satu-episode-hadir.md)) | ✅ **Selesai** |
| `BE-RWI-013` | Kamar tidak pernah menjadi campur; boks bayi dikecualikan dua arah (`RWI-DEC-064`, `RWI-DEC-066`; `RWI-RULE-012` B) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-013-aturan-jenis-kelamin-dan-boks-bayi.md)) | ✅ **Selesai** |
| `BE-RWI-014` | Kebutuhan isolasi tercatat dengan pemiliknya jelas; `GUARD-INP-04` membedakan catatan awal dari keputusan klinis (`RWI-DEC-065`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: kriteria **403**. Yang terbukti baru **401** tanpa token; membuktikan 403 butuh akun yang login tetapi tidak punya butir hak aksesnya ([laporan](../task/report/backend/be-rwi-014-kebutuhan-isolasi-dan-guard-inp-04.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-015` | Kapasitas isolasi terjaga dua arah, dan pencatatan klinis tidak pernah ditahan (`RWI-DEC-065` aturan 5–7) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-015-kapasitas-isolasi-dan-daftar-pantau.md)) | ✅ **Selesai** |
| `BE-RWI-016` | Census dihitung dari penempatan aktif; lama dirawat dari selisih tanggal, minimum 1 hari (`RWI-DEC-027`; `RWI-RULE-019`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-016-census-dan-lama-dirawat.md)) | ✅ **Selesai** |
| `BE-RWI-017` | DPJP berbentuk riwayat berperiode, bukan kolom yang ditimpa (`RWI-DEC-022`, `RWI-DEC-024`; `INV-INP-03`; `GUARD-INP-01`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-017-penugasan-dpjp-berperiode.md)) | ✅ **Selesai** |
| `BE-RWI-018` | Perawat penanggung jawab berperiode, dan ketiadaannya tidak menahan apa pun (`RWI-DEC-032`; `RWI-RULE-023`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: kriteria 3 baru terbukti di tingkat service, belum lewat endpoint ([laporan](../task/report/backend/be-rwi-018-penugasan-perawat-penanggung-jawab.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-019` | Perpindahan utuh dalam satu transaksi; kelas tagihan mengikuti kamar (`INV-INP-07`; `RWI-DEC-012`, `RWI-DEC-013`; `GUARD-INP-01`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-019-perpindahan-pasien-satu-transaksi.md)) | ✅ **Selesai** |
| `BE-RWI-020` | Keputusan pulang milik DPJP aktif; tempat tidur belum dilepas (`RWI-DEC-016`, `RWI-DEC-017`; `GUARD-INP-02`) | Build dan test hijau, endpoint terbukti menjawab saat aplikasi menyala. Yang masih menahan: RWI-OQ-039 — roadmap menyebut lima cara pulang, sedangkan enum baru menyediakan tiga. Menunggu pemilik klinis ([laporan](../task/report/backend/be-rwi-020-keputusan-pasien-boleh-pulang.md)) | 🟡 **Hampir selesai** |
| `BE-RWI-021` | Resume tertandatangani hanya oleh DPJP aktif; isi klinis tidak bocor ke daftar mana pun (`RWI-DEC-016`; `GUARD-INP-03`; `INV-INP-05`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-021-resume-pulang-dan-tanda-tangan.md)) | ✅ **Selesai** |
| `BE-RWI-022` | Amandemen resume tertandatangani menyimpan versi sebelumnya; versi tidak dapat diubah maupun dihapus (`RWI-DEC-057`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-022-versi-resume-pulang.md)) | ✅ **Selesai** |
| `BE-RWI-023` | Butir wajib administrasi menahan penutupan; butir yang dinonaktifkan tidak lagi menahan tanpa menghapus penandaan lama (`RWI-DEC-026`, `RWI-DEC-033`; `RWI-RULE-018`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-023-daftar-periksa-administrasi.md)) | ✅ **Selesai** |
| `BE-RWI-024` | Gerbang keuangan punya sumber data yang jelas; setiap penandaan meninggalkan pelaku, waktu, dan catatan (`RWI-DEC-015`, `RWI-DEC-040`; `RWI-RULE-028`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-024-kelayakan-keuangan.md)) | ✅ **Selesai** |
| `BE-RWI-025` | Kelima syarat penutupan diperiksa dan dilaporkan satu per satu; penutupan melepas tempat tidur dalam satu transaksi (`RWI-RULE-010`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-025-lima-syarat-penutupan.md)) | ✅ **Selesai** |
| `BE-RWI-026` | Jalan keluar supervisor menembus **hanya** syarat keuangan, dan selalu meninggalkan jejak (`RWI-DEC-015`; `RWI-RULE-009`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-026-jalan-keluar-supervisor.md)) | ✅ **Selesai** |
| `BE-RWI-027` | Tempat tidur bebas sejak pasien meninggalkan kamar, tanpa menutup episode dan tanpa menulis riwayat status (`RWI-DEC-055`; `RWI-RULE-036`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-027-kepergian-fisik-pasien.md)) | ✅ **Selesai** |
| `BE-RWI-028` | Riwayat status terbaca lengkap; kedaluwarsa tercatat sebagai tindakan sistem, bukan menuduh pembaca layar (`RWI-DEC-009`; `NFR-003`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-028-riwayat-status-tidak-dapat-dihapus.md)) | ✅ **Selesai** |
| `BE-RWI-029` | Empat daftar pantau dan laporan selisih salinan status tempat tidur (`RWI-DEC-032`, `RWI-DEC-039`; `RWI-RULE-023`, `RWI-RULE-027`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-029-daftar-pantau-dan-laporan-selisih.md)) | ✅ **Selesai** |
| `BE-RWI-030` | Sesi koreksi tanpa membongkar episode: status tetap `Closed`, tempat tidur tidak kembali, hari rawat tidak bertambah (`RWI-DEC-028`; `RWI-RULE-020`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-030-sesi-koreksi.md)) | ✅ **Selesai** |
| `BE-RWI-031` | Bayi punya episode dan kunjungan sendiri di boks kamar ibunya; menutup episode ibu tidak menutup episode bayi (`RWI-DEC-020`, `RWI-DEC-056`; `RWI-RULE-014`) | Build Debug dan Release hijau; `dotnet test` **255/255** hijau; migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`; aplikasi menyala dengan `GET /health` → **200**; 49 operasi HTTP `inpatient` terbaca pada dokumen Swagger. Api contract sudah dinaikkan menjadi `Tersedia`, sehingga butir DoD terakhir ikut terpenuhi ([laporan](../task/report/backend/be-rwi-031-episode-bayi-baru-lahir.md)) | ✅ **Selesai** |
| `BE-RWI-032`, `BE-RWI-033` | — | Belum dikerjakan | Planned |

**21 dari 33 task backend selesai (64%).** Sembilan task berstatus 🟡 — build dan test-nya
**sudah hijau**, endpoint-nya **sudah terbukti menjawab**, dan yang menahan tinggal butir spesifik
per task yang tercantum pada kolom Bukti. Satu task berstatus ⛔ dan tidak boleh dimulai. Dua task
terakhir, `BE-RWI-032` dan `BE-RWI-033`, menunggu keduanya.

Narasi lengkap per task ada pada [backend-roadmap.md](backend-roadmap.md) bagian 0. Bagian 1b ini
merangkumnya; bila keduanya berbeda, backend-roadmap yang berlaku.

> **Gerbang build sudah ditutup pada 26 Agustus 2026.** Sampai 25 Agustus, 28 task berisi kode
> yang belum pernah dikompilasi sekali pun, dan itu adalah risiko terbesar yang tersisa. Risiko
> itu **sudah hilang**: `dotnet build` hijau pada Debug maupun Release, `dotnet test` hijau
> **255 dari 255**, migration diterapkan ke PostgreSQL `QuilvianNewDevTim01`, aplikasi menyala
> dengan `GET /health` menjawab **200**, dan dokumen Swagger memuat **49 operasi HTTP**
> `inpatient` — cocok persis dengan 49 baris api contract.

> **Kenapa masih ada status 🟡 sesudah build hijau.** Sembilan task tersisa, dan **tidak satu pun**
> di antaranya tertahan oleh ketiadaan build atau test. Yang menahannya:
>
> | Task | Yang masih menahannya |
> | --- | --- |
> | `BE-RWI-002`, `BE-RWI-004`, `BE-RWI-005` | Tabel DoD pada laporannya tidak berformat baku, sehingga harus dinilai manual |
> | `BE-RWI-008` | Batas "belum ada catatan klinis" — jalur baca ke `ClinicalManagement` belum ada pada integration contract |
> | `BE-RWI-009`, `BE-RWI-014` | Kriteria **403**. Yang terbukti baru **401** tanpa token; membuktikan 403 butuh akun yang login tetapi tidak punya butir hak aksesnya |
> | `BE-RWI-011` | Test tabrakan dua transaksi terhadap PostgreSQL belum dijalankan. Index-nya sudah ada, perilakunya belum diuji |
> | `BE-RWI-018` | Kriteria 3 baru terbukti di tingkat service, belum lewat endpoint |
> | `BE-RWI-020` | `RWI-OQ-039` — roadmap menyebut lima cara pulang, sedangkan enum baru menyediakan tiga. Menunggu pemilik klinis |
>
> Sisanya adalah satu test yang belum dijalankan, dua keputusan yang belum turun, tiga pengujian
> otorisasi runtime, dan tiga penilaian DoD manual.
>
> **Satu hal yang berubah sifatnya sejak 25 Agustus 2026.** Sebelum `BE-RWI-011`, episode tidak
> pernah dapat mencapai `Admitted`, sehingga beberapa celah yang tercatat hanya teoretis. Sejak
> penempatan pasien dibuka, celah "belum ada catatan klinis" pada pembatalan **sudah punya jalur
> yang benar-benar terpakai**. Ia berhenti menjadi catatan tambahan dan menjadi gerbang.
>
> **Kenapa `BE-RWI-006` berstatus ⛔.** Roadmap menyebut prasyaratnya di tiga tempat: bagian 0,
> baris **Dependency**, dan baris **DoD**. Ketiganya menuntut `FE-RWI-001` terbukti rilis lebih
> dulu. Mengerjakannya lebih dulu berarti mencabut satu-satunya cara admin menutup tempat tidur
> rusak sebelum penggantinya berfungsi.
>
> **Yang berubah pada 26 Agustus 2026.** `FE-RWI-001` dikerjakan dan dibuktikan. Penelusuran
> layarnya menemukan sesuatu yang tidak pernah tercatat: tombol aktifkan dan nonaktifkan pada
> layar tempat tidur **memang belum pernah ada**. Perbaikan URL yang sudah dilakukan sebelumnya
> hanya membetulkan alamat panggilan, bukan menghadirkan tombolnya. Ini justru memperkuat alasan
> `BE-RWI-006` ditahan: sebelum 26 Agustus, jalan keluar yang hendak dilindungi itu sama sekali
> tidak ada. Sekarang tombolnya ada dan terbukti bekerja, tetapi **belum di-commit**, jadi belum
> ada di lingkungan mana pun. Gerbangnya tetap tertutup sampai perubahan itu rilis.

### Yang perlu diputuskan pemilik pekerjaan

| Butir | Sifat | Terdampak |
| --- | --- | --- |
| Tidak ada connection string lokal — `dotnet ef database update` polos mengenai database dev bersama `QuilvianNewDevTim01` | Operasional, dapat dikerjakan | Setiap task bermigration berikutnya — jebakan ini **terbukti relevan kembali** pada `BE-RWI-003` |
| Berkas `BE-RWI-001` dan `BE-RWI-003` belum di-commit | Operasional | Rekan yang menarik branch `MHamzah` |
| Letak konfigurasi EF master: `HealthServices/MasterData/` versus `HealthServices/` | Konvensi, tanpa akibat teknis | Konsistensi folder, sejalan `BE-IGD-013` |
| Folder konfigurasi EF transaksi: roadmap menulis `HealthService/` (tanpa `s`), kenyataan `HealthServices/` | Salah ketik pada roadmap, **bukan** keputusan desain — penyimpangan tercatat pada [laporan BE-RWI-003](../task/report/backend/be-rwi-003-tabel-transaksi-rawat-inap.md) bagian 5.3 | Konsistensi roadmap |
| `RWI-RULE-021` belum final secara klinis — nilai `24` jam terpasang sebagai bawaan | Klinis | Menahan pemakaian untuk pasien sungguhan, bukan MVP |
| `IX_InpNurseAssignment_EpisodeId_Active` membatasi satu perawat aktif per episode — perlu dipastikan cocok kenyataan ruangan | Domain | `BE-RWI-018`, keputusan kembali ke `/qv-design` |
| Perbaikan `Program.cs` di luar scope `BE-RWI-003` | Operasional | Commit strategy — lihat [laporan](../task/report/backend/be-rwi-003-tabel-transaksi-rawat-inap.md) bagian 5.2 |
| Project Tests di dalam folder project web — `MSB3030` berulang | Struktural, di luar scope modul ini | Build stability — lihat [laporan](../task/report/backend/be-rwi-003-tabel-transaksi-rawat-inap.md) bagian 6.2 |
| ~~Build dan test `BE-RWI-002` s.d. `BE-RWI-031` belum dijalankan~~ | **SELESAI 26 Agustus 2026.** Build hijau, `dotnet test` 255/255 hijau, dan endpoint terbukti menjawab saat aplikasi menyala. 21 task naik menjadi ✅ | Backend/API |
| **Penanggung jawab pembaca laporan selisih tempat tidur belum ditetapkan** | `GET /monitoring/bed-drift` adalah satu-satunya pengawas atas satu-satunya arah tulis lintas modul. Kode-nya ada; yang belum ada adalah orang yang membacanya berkala | Backend/API bersama Product/Domain — lihat [laporan BE-RWI-029](../task/report/backend/be-rwi-029-daftar-pantau-dan-laporan-selisih.md) bagian 2 |
| Interpretasi syarat kelima penutupan | `RWI-RULE-010` menulis "tempat tidur aktif ditemukan"; sejak `RWI-DEC-055` episode yang kepergiannya sudah dicatat tidak lagi memegang tempat tidur dan tetap harus dapat ditutup | Product/Domain — lihat [laporan BE-RWI-025](../task/report/backend/be-rwi-025-lima-syarat-penutupan.md) bagian 2 |
| `InpDischargeService` kini juga memakai `InpBedOccupancyService` | Class diagram `02-backend-architecture.md` §3.4 kurang satu panah. Arahnya tidak melingkar | Pemilik arsitektur backend — lihat [laporan BE-RWI-025](../task/report/backend/be-rwi-025-lima-syarat-penutupan.md) bagian 4.1 |
| Cara pulang belum dapat dikoreksi lewat sesi koreksi | State matrix §6.1 mengizinkannya, tetapi tidak ada endpoint yang menyediakannya. Kesalahan cara pulang pada episode tertutup tidak dapat dibetulkan | Product/Domain — lihat [laporan BE-RWI-030](../task/report/backend/be-rwi-030-sesi-koreksi.md) bagian 6.1 |
| Sumber kolom pelaku pada daftar penutupan menembus gerbang | Dibaca dari `InpEpisode.UpdateBy`, yang ikut berubah bila episode disentuh lagi. Sumber tahan lamanya adalah `InpStatusHistory` | Backend/API — lihat [laporan BE-RWI-029](../task/report/backend/be-rwi-029-daftar-pantau-dan-laporan-selisih.md) bagian 6.1 |
| Rujukan episode ibu tidak dapat dibetulkan setelah bayi ditempatkan | Bayi kembar yang tertukar rujukannya tidak dapat diperbaiki lewat jalur mana pun | Product/Domain — lihat [laporan BE-RWI-031](../task/report/backend/be-rwi-031-episode-bayi-baru-lahir.md) bagian 6.2 |
| Nama peran kasir dan billing adalah asumsi | Bila keliru, kelayakan keuangan tidak pernah menjadi `Cleared` dan **pasien ikut tertahan**, bukan hanya petugas. **Kini menahan dua task sekaligus:** `FE-RWI-013` menyalin daftar peran yang sama, sehingga layar dan server akan salah bersamaan | Product/Domain — lihat [laporan BE-RWI-024](../task/report/backend/be-rwi-024-kelayakan-keuangan.md) bagian 5.1 dan [laporan FE-RWI-013](../task/report/frontend/FE-RWI-013.md) |
| **Test tabrakan dua transaksi terhadap PostgreSQL belum dijalankan** | Penguncian baris `MstBed` dan kedua unique index parsial tidak dapat diuji provider InMemory. Ini pertahanan sesungguhnya terhadap tempat tidur ganda, dan ia belum terbukti sama sekali | Backend/API — lihat [laporan BE-RWI-011](../task/report/backend/be-rwi-011-penempatan-pasien-dan-inv-inp-02.md) bagian 2 |
| Arah dependency `InpEpisodeService` ↔ `InpBedOccupancyService` dibalik | Class diagram `02-backend-architecture.md` §3.4 menggambar arah lama. Mempertahankan kedua arah menghasilkan dependency melingkar yang membuat aplikasi tidak menyala. Diagramnya perlu dikoreksi | Pemilik arsitektur backend — lihat [laporan BE-RWI-011](../task/report/backend/be-rwi-011-penempatan-pasien-dan-inv-inp-02.md) bagian 3.1 |
| **Dua cara pulang — meninggal dan kabur — aturan klinisnya belum disahkan** | Roadmap `BE-RWI-020` menyebut lima cara pulang; enum dan validation matrix menyediakan tiga. Pasien yang meninggal atau kabur belum dapat dicatat cara pulangnya sama sekali. **Kini menahan dua task sekaligus:** kriteria 2 `FE-RWI-012` ikut tertahan butir yang sama | Product/Domain bersama Clinical governance — `RWI-OQ-039`, `RWI-DEC-059` |
| **Tidak ada endpoint baca kelayakan keuangan** | `GetFinancialClearanceAsync` sudah ditulis pada `InpDischargeService.Closure.cs:222` tetapi tidak pernah dipasang sebagai aksi controller, dan api contract `0.4.0` tidak memuatnya. Akibatnya kasir tidak dapat membaca penandaannya sendiri dari hari sebelumnya, dan kriteria 3 `FE-RWI-013` tertahan. `MarkedByUserId` juga berupa `Guid` tanpa nama | Backend/API — lihat [laporan FE-RWI-013](../task/report/frontend/FE-RWI-013.md) bagian 1 |
| **Delta jumlah nilai kelayakan keuangan** | Roadmap `FE-RWI-013` kriteria 4 menyebut tiga nilai tersedia; `state-transition-matrix` §4 hanya mengenal dua tindakan dan tidak punya perpindahan kembali ke `Pending`; backend menerima ketiganya. Layar mengikuti matriks. Salah satu dokumen perlu dikoreksi | Product/Domain bersama pemilik kontrak — lihat [laporan FE-RWI-013](../task/report/frontend/FE-RWI-013.md) bagian 2 |
| **Penandaan butir administrasi tidak dapat dipisahkan dari penyusunan resume** | `MarkClearanceItem` dijaga `InpatientDischarge : Update`, butir hak akses yang sama persis dengan `UpsertSummary` — dan resume memang milik DPJP. Akibatnya server **tidak dapat** menolak DPJP yang menandai butir administrasi, walaupun `03-frontend-architecture.md` bagian 3 tidak memberinya aksi itu. Pemisahannya hari ini hanya ada di layar | Backend/API bersama pemilik keamanan — lihat [laporan FE-RWI-014](../task/report/frontend/FE-RWI-014.md) bagian 4 |
| **Tidak ada daftar nama peran "petugas admisi"** | `InpatientActorClaims` menyediakan daftar untuk supervisor, kepala ruangan, dan kasir, tetapi tidak untuk petugas admisi. Akibatnya layar tidak dapat menyembunyikan tombol tutup episode dari perawat pelaksana, walaupun bagian 3 tidak memberinya wewenang menutup. Yang menahannya tetap `InpatientEpisode : Close` di server | Backend/API bersama pemilik keamanan — lihat [laporan FE-RWI-014](../task/report/frontend/FE-RWI-014.md) bagian 4 |
| **Delta `state-transition-matrix` §5 vs roadmap `BE-RWI-021` kriteria 3** | Matriks mengizinkan DPJP aktif mengubah resume tertandatangani lewat endpoint biasa; roadmap melarangnya. Implementasi mengikuti roadmap. Salah satu dokumen perlu dikoreksi | Product/Domain bersama pemilik kontrak — lihat [laporan BE-RWI-021](../task/report/backend/be-rwi-021-resume-pulang-dan-tanda-tangan.md) bagian 5.1 |
| Kolom penyaring rentang tanggal daftar episode | Roadmap `BE-RWI-009` menyebut "rentang tanggal" tanpa menetapkan kolomnya. Yang dipakai `InpEpisode.CreateDateTime` | Product/Domain — lihat [laporan BE-RWI-009](../task/report/backend/be-rwi-009-daftar-dan-detail-episode.md) bagian 5.1 |
| Kalimat daftar pantau penempatan tidak sesuai belum dikunci kontrak | Api contract menetapkan endpoint dan bentuk jawabannya, bukan kalimatnya. Kalimat yang dipakai ditulis apa adanya pada laporan | Product/Domain — lihat [laporan BE-RWI-015](../task/report/backend/be-rwi-015-kapasitas-isolasi-dan-daftar-pantau.md) bagian 6.2 |
| Perilaku lama dirawat untuk episode `DischargePending` | Angkanya terus bertambah sampai episode ditutup atau kepergiannya dicatat. Bila seharusnya berhenti pada `DischargeDecidedAt`, perubahannya satu baris | Product/Domain — lihat [laporan BE-RWI-016](../task/report/backend/be-rwi-016-census-dan-lama-dirawat.md) bagian 5.1 |
| Selisih arti unit layanan antara daftar episode dan census | Daftar episode menyaring terhadap unit **admisi**; census menyaring terhadap unit **penempatan saat ini**. Benar secara semantik, tetapi perlu diketahui perancang layar | Product/Domain — lihat [laporan BE-RWI-019](../task/report/backend/be-rwi-019-perpindahan-pasien-satu-transaksi.md) bagian 6.2 |
| DPJP tidak dapat membetulkan resumenya sendiri setelah ditandatangani | Amandemen hanya oleh supervisor di dalam sesi koreksi, sesuai state matrix §5. Alur kerjanya perlu dipastikan dapat dijalankan ruangan | Product/Domain — lihat [laporan BE-RWI-022](../task/report/backend/be-rwi-022-versi-resume-pulang.md) bagian 7.1 |
| Tanda tangan tidak diperbarui setelah amandemen resume | Resume hasil koreksi beredar dengan tanda tangan yang mendahului isinya. Bila seharusnya ditandatangani ulang, perilakunya berbeda | Product/Domain — lihat [laporan BE-RWI-022](../task/report/backend/be-rwi-022-versi-resume-pulang.md) bagian 7.2 |
| **`FE-RWI-001` sudah selesai dan terbukti, tetapi belum di-commit** | Menahan `BE-RWI-006`, dan lewat dependency-nya `BE-RWI-032`. Yang tersisa satu langkah: commit dan push repository frontend. Sesudah itu gerbangnya boleh dibuka | Frontend bersama Backend/API — lihat [laporan FE-RWI-001](../task/report/frontend/FE-RWI-001.md) |
| **Delta kontrak baru: `PUT /beds/{id}` kini menerima `isActive` opsional** | Diperbaiki 26 Agustus 2026 atas izin pemilik pekerjaan. Sebelumnya `UpdateBedRequest.IsActive` bertipe `bool` berbawaan `true`, sehingga consumer yang tidak mengirim field itu diam-diam mengaktifkan kembali tempat tidur yang sudah dinonaktifkan. Sekarang bertipe `bool?` dan controller mempertahankan nilai lama bila tidak dikirim. `api-contract.md` bagian 7 masih menyatakan "seluruh endpoint lain pada grup ini" tidak berubah, jadi kalimat itu perlu dikoreksi dan kontraknya diberi versi baru | Pemilik kontrak bersama pemilik modul `MasterData` — lihat [laporan FE-RWI-001](../task/report/frontend/FE-RWI-001.md) |
| Jalur kunjungan poliklinik pada `RWI-RULE-005` tidak dapat berjalan | Validation matrix `0.4.0` bagian 1 mewajibkan kunjungan bertipe rawat inap, sehingga kunjungan poliklinik ditolak 422. Salah satu dokumen perlu dikoreksi | Product/Domain — lihat [laporan BE-RWI-007](../task/report/backend/be-rwi-007-buka-admisi-episode-bernomor.md) bagian 5.1 |
| Bentuk nomor kunjungan yang dibuat modul Rawat Inap berbeda dari nomor pendaftaran | Alokator lama milik Registrasi melanggar `QBE-CODE-003` dan tidak boleh ditiru kode baru. Perlu diputuskan: terima dua bentuk, atau sediakan alokator bersama | Pemilik `RegistrationManagement` — lihat [laporan BE-RWI-007](../task/report/backend/be-rwi-007-buka-admisi-episode-bernomor.md) bagian 5.2 |
| Integration contract bagian 2 menyebut `INT-INP-03` "satu-satunya arah tulis", padahal modul kini juga menulis `TrxPatientEncounter` | Kalimat kontraknya sudah tidak akurat. Perlu diperbarui menjadi dua arah tulis, atau diberi pengecualian tertulis | Product/Domain bersama pemilik `RegistrationManagement` — lihat [laporan BE-RWI-007](../task/report/backend/be-rwi-007-buka-admisi-episode-bernomor.md) bagian 5.3 |
| Batas "belum ada catatan klinis" pada pembatalan **belum diperiksa** | Tidak berakibat hari ini karena episode belum dapat mencapai `Admitted`. **Wajib ditutup sebelum `BE-RWI-011` ditandai selesai**, atau supervisor akan dapat membatalkan episode yang sudah punya pengkajian dan tanda vital | Backend/API bersama Product/Domain — lihat [laporan BE-RWI-008](../task/report/backend/be-rwi-008-ubah-batal-kedaluwarsa-draft.md) bagian 5.1 |
| Daftar nama peran supervisor dan kepala ruangan adalah asumsi | Nama peran adalah data yang disiapkan admin, dan tidak ada kontrak modul ini yang menyebutkan nama sesungguhnya. Bila keliru, supervisor yang sah ditolak 403 | Product/Domain — lihat [laporan BE-RWI-008](../task/report/backend/be-rwi-008-ubah-batal-kedaluwarsa-draft.md) bagian 5.3 |
| Blueprint §4.20 dan roadmap `BE-RWI-005` menyatakan controller master memakai `ApplicationDbContext` langsung; kode mematuhi `QBE-SVC-001` dan memakai service | Dokumentasi, tanpa akibat teknis. Perlu diputuskan: sesuaikan blueprint, atau catat pengecualian pada `QBE_EXCEPTIONS.json` | Pemilik arsitektur backend — lihat [laporan BE-RWI-005](../task/report/backend/be-rwi-005-controller-master-rawat-inap.md) bagian 5.1 |
| `contracts/validation-matrix.md` `0.4.0` tidak punya bagian pengaturan, padahal roadmap `BE-RWI-005` merujuknya | Aturan validasi kedua layar master tidak terkunci kontrak. Aturan yang dipakai ditulis apa adanya pada laporan untuk ditinjau | Product/Domain — lihat [laporan BE-RWI-005](../task/report/backend/be-rwi-005-controller-master-rawat-inap.md) bagian 5.3 |
| Aturan baru "pengaturan terakhir tidak boleh dinonaktifkan" belum disahkan | Menutup satu jalan yang mungkin dibutuhkan admin. Belum ada dasarnya pada kontrak | Product/Domain |
| Letak seeder: roadmap menulis `MasterData/Seeders/`, arsitektur §5 menulis `InPatientManagement/Seeders/` | Roadmap yang diikuti. Perbedaan perlu dirapikan pada salah satu dokumen | Pemilik arsitektur backend |
| Nama lingkungan produksi yang sebenarnya belum diperiksa | Penjagaan produksi seeder membandingkan dengan `Production`. Bila lingkungan produksi memakai nama lain, penjagaan itu tidak berlaku | Backend/API bersama pemilik deployment |

---

## 2. Functional requirement → task → acceptance criteria → test

Kolom **AC** merujuk `00-interview-decisions.md` revision `5`. Kolom **Test** merujuk
`testing/acceptance-test-matrix.md` `0.3.0` dan skenario `UAT` pada `04-prd-to-mvp.md`.

### `EPIC RI-21` — Fondasi episode dan data master

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-101` s.d. `FR-RI-104` | Episode, nomor, jangkar kunjungan, DPJP wajib | `BE-RWI-007` ✅ | `RWI-AC-004` s.d. `RWI-AC-006`, `RWI-AC-009`, `RWI-AC-010` | Bagian 1; `UAT-01`. Kode: `InpEpisodeOpenAdmissionTests.cs`, 14 test **belum dijalankan** |
| `FR-RI-105` s.d. `FR-RI-108` | Ubah isian, pembatalan, kedaluwarsa `Draft` | `BE-RWI-008` 🟡 | `RWI-AC-007` s.d. `RWI-AC-010`, `RWI-AC-044` s.d. `RWI-AC-046`, `RWI-AC-090` s.d. `RWI-AC-092` | Bagian 6; `UAT-23`. Kode: `InpEpisodeDraftLifecycleTests.cs`, 16 test **belum dijalankan**. `RWI-AC-006` dan `RWI-AC-007` **belum tercakup penuh** — lihat [laporan](../task/report/backend/be-rwi-008-ubah-batal-kedaluwarsa-draft.md) bagian 5.1 dan 5.2 |
| `FR-RI-148` | Satu pasien satu episode yang hadir | `BE-RWI-012` ✅ | `RWI-AC-116`, `RWI-AC-117` | Bagian 1; `UAT-26` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-22` — Pencarian dan pemesanan tempat tidur

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-106` s.d. `FR-RI-108` | Pemesanan 2 jam, gugur saat dibaca, dapat diubah admin | `BE-RWI-010` ✅ | `RWI-AC-001` s.d. `RWI-AC-003` | Bagian 1; `UAT-03` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-23` — Penempatan pasien

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-109` | Pencegahan tempat tidur ganda | `BE-RWI-011` 🟡 | `RWI-AC-059` | Bagian 2 skenario tabrakan; `UAT-02`, `UAT-04` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-110` | Penempatan dan salinan status dalam satu transaksi | `BE-RWI-011` 🟡 | `RWI-AC-062` | Bagian 2 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-111` | Pemesanan gugur tidak menghalangi penempatan | `BE-RWI-011` 🟡 | `RWI-AC-002` | Bagian 1 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-112` | Penolakan tidak menghapus isian admisi | `BE-RWI-011` 🟡, `FE-RWI-007` | `RWI-AC-010` | Bagian 2 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-148` | Satu pasien satu episode | `BE-RWI-012` ✅ | `RWI-AC-116`, `RWI-AC-117` | Bagian 1; `UAT-26` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-24` — Census dan lama dirawat

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-113` | Census hanya `Admitted` dan `DischargePending` | `BE-RWI-016` ✅ | — | Bagian 5; `UAT-06` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-114` | Lama dirawat dari selisih tanggal, minimum 1 hari | `BE-RWI-016` ✅ | — | Bagian 5 unit test; `UAT-05` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-115` | Bertambah pada pergantian tanggal | `BE-RWI-016` ✅ | — | Bagian 5 unit test — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-25` — Penanggung jawab episode

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-116` | DPJP berbentuk riwayat berperiode | `BE-RWI-017` ✅ | — | Bagian 4; `UAT-07` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-117` | Tepat satu DPJP aktif | `BE-RWI-017` ✅ | — | Bagian 4 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-118` | Pengalihan wajib beralasan | `BE-RWI-017` ✅, `FE-RWI-011` | — | Bagian 4 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-119` | Episode boleh tanpa perawat | `BE-RWI-018` 🟡, `FE-RWI-011` | — | Bagian 4 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-26` — Perpindahan pasien

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-120` | Perpindahan bersifat utuh | `BE-RWI-019` ✅ | — | Bagian 3; `UAT-09` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-121` | Kelas mengikuti kamar | `BE-RWI-019` ✅ | — | Bagian 3 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-122` | Dokter bukan DPJP tidak dapat memindahkan | `BE-RWI-019` ✅, `FE-RWI-010` | — | Bagian 3; `UAT-08` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-123` | Perpindahan wajib beralasan medis | `BE-RWI-019` ✅ | — | Bagian 3 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-162` | Aturan penempatan berlaku pada perpindahan | `BE-RWI-019` ✅ | `RWI-AC-133` | Bagian 2A.1 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-27` — Keputusan pulang dan resume

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-124` s.d. `FR-RI-128` | Keputusan pulang, lima cara pulang, resume, tanda tangan | `BE-RWI-020` 🟡, `BE-RWI-021` ✅, `FE-RWI-012` 🟡 | — | Bagian 7; `UAT-10` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. **Lima cara pulang baru terpenuhi tiga** — meninggal dan kabur menunggu `RWI-OQ-039` dan `RWI-DEC-059`. Layar `FE-RWI-012` mengonsumsi ketiganya, menolak permintaan tanpa cara pulang sebelum dialog konfirmasi tampil, dan menyebut kedua cara pulang yang belum tersedia beserta jalan keluar supervisornya ([laporan](../task/report/frontend/FE-RWI-012.md)) |
| `FR-RI-153` | Versi resume pulang | `BE-RWI-022` ✅, `FE-RWI-012` ✅ | `RWI-AC-124` s.d. `RWI-AC-126` | Bagian 7; `UAT-27` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Jalur amandemennya dapat dijalankan sepenuhnya sejak `BE-RWI-030` membuka endpoint sesi koreksi. Layar membacanya lewat `includeRevisions=true`, dan e2e membuktikan dua versi yang dikirim terbalik terbaca urut beserta nama penandatangan tiap versi ([laporan](../task/report/frontend/FE-RWI-012.md)) |

### `EPIC RI-28` — Daftar periksa, kelayakan keuangan, dan penutupan

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-129` s.d. `FR-RI-133` | Daftar periksa, kelayakan keuangan, lima syarat, penutupan | `BE-RWI-023` ✅, `BE-RWI-024` ✅, `BE-RWI-025` ✅, `FE-RWI-013` 🟡 | `RWI-AC-064` | Bagian 8, 9; `UAT-11` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Layar `FE-RWI-013` menandai kelayakan keuangan dengan catatan wajib dan menyembunyikan aksinya dari peran selain kasir dan billing. **Riwayat penandaannya belum dapat dibaca ulang** karena kontrak tidak menyediakan `GET .../financial-clearance` ([laporan](../task/report/frontend/FE-RWI-013.md)) |
| `FR-RI-134` | Jalan keluar supervisor | `BE-RWI-026` ✅, `FE-RWI-014` ✅ | — | Bagian 8; `UAT-12`, `UAT-13` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Layar `FE-RWI-014` merender kelima syarat sejak dibuka dan **tidak merender** jalan keluar supervisor sampai penutupan biasa ditolak karena kelayakan keuangan; e2e membuktikan supervisor tidak melihatnya sebelum penolakan, dan tiga peran lain tidak melihatnya bahkan sesudahnya ([laporan](../task/report/frontend/FE-RWI-014.md)) |
| `FR-RI-149` s.d. `FR-RI-151` | Kepergian fisik pasien | `BE-RWI-027` ✅, `FE-RWI-015` 🟡 | `RWI-AC-118` s.d. `RWI-AC-121` | Bagian 4A; `UAT-24`, `UAT-25` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Aksi `FE-RWI-015` menempel pada detail episode — bukan layar penutupan — karena perawat pelaksana tidak punya `InpatientDischarge : Read`; konfirmasinya menyebut tindakan tidak dapat dibatalkan, dan sesudah pencatatan status episode tetap `DischargePending` sementara tempat tidurnya terbaca kosong. **Kriteria 4 kini dapat ditutup penuh** sejak [`FE-RWI-016`](../task/report/frontend/FE-RWI-016.md) selesai: daftar pantau penutupan tertunda sudah ada, dan episode yang kepergiannya tercatat muncul di sana bertanda tempat tidur "Sudah bebas" ([laporan](../task/report/frontend/FE-RWI-015.md)) |

### `EPIC RI-29` — Riwayat status dan daftar pantau

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-135` s.d. `FR-RI-138` | Riwayat status, tiga daftar pantau, laporan selisih | `BE-RWI-028` ✅, `BE-RWI-029` ✅, `FE-RWI-016` ✅, `FE-RWI-017` ✅ | `RWI-AC-063` | Bagian 10; `UAT-17`, `UAT-21` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Keempat daftar pantau berdiri pada satu halaman bertab dan tidak menahan tindakan apa pun — e2e membuktikan nol permintaan selain `GET` dari halaman itu, lalu alur perpindahan pada detail episode berjalan penuh sesudahnya ([laporan](../task/report/frontend/FE-RWI-016.md)). Laporan selisih menyebut **kedua** nilai yang berselisih pada setiap baris dan menutup halamannya bagi peran selain supervisor dan `SuperAdmin`; **penyempitan itu hanya berlaku di layar**, karena server menjaga kelima endpoint daftar pantau dengan butir `InpatientMonitoring : Read` yang sama ([laporan](../task/report/frontend/FE-RWI-017.md)) |

### `EPIC RI-30` — Sesi koreksi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-139` s.d. `FR-RI-141` | Sesi koreksi supervisor, tidak mengganggu tempat tidur | `BE-RWI-030` ✅, `FE-RWI-018` ✅ | — | Bagian 10; `UAT-14`, `UAT-15`, `UAT-16` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Layar sesi koreksi berdiri dan seluruh aksinya tidak dirender bagi peran selain supervisor, termasuk bagi DPJP aktif. Status episode dibaca ULANG dari server sesudah sesi dibuka dan terbaca tetap `Closed`; koreksi resume tertandatangani memperingatkan versi lama akan disimpan, dan versi lamanya terbaca sesudah tersimpan. **Sesi terbuka milik supervisor lain tidak dapat dibaca layar** karena kontrak `0.4.0` tidak menyediakan `GET .../correction-sessions` ([laporan](../task/report/frontend/FE-RWI-018.md)) |

### `EPIC RI-31` — Pengaturan admin

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-142` s.d. `FR-RI-144` | Pengaturan dan butir administrasi dapat diubah admin | `BE-RWI-005`, `FE-RWI-003`, `FE-RWI-004` | `RWI-AC-003`, `RWI-AC-105` s.d. `RWI-AC-107` | Bagian 11; `UAT-18`, `UAT-19` |

### `EPIC RI-32` — Perbaikan tempat tidur dan pembatasan wewenang status

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-145` | Status terisi dan dipesan hanya dari Rawat Inap | `BE-RWI-006` ⛔ | `RWI-AC-060`, `RWI-AC-061` | Bagian 12; `UAT-20`, `UAT-21`. **Belum dikerjakan** — terblokir `FE-RWI-001` |
| — | Tombol tempat tidur tidak lagi 404 | `FE-RWI-001` ✅ | `RWI-AC-114` | Bagian 12; e2e `tests/e2e/bed-status-toggle.spec.mjs` dan enam unit test tempat tidur ([laporan](../task/report/frontend/FE-RWI-001.md)) |
| — | Modul tetangga terbukti tidak rusak | `BE-RWI-032` | `RWI-AC-114` | Bagian 12 |

### `EPIC RI-33` — Bayi baru lahir dan boks bayi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-146` | Boks bayi sebagai tempat tidur | `BE-RWI-031` ✅ | — | Bagian 12A; `UAT-22` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-147` | Episode ibu dan bayi terpisah | `BE-RWI-031` ✅ | `RWI-AC-123` | Bagian 12A — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-152` | Penanda rawat gabung | `BE-RWI-031` ✅ | `RWI-AC-122` | Bagian 12A; `UAT-28` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

### `EPIC RI-34` — Kelayakan penempatan menurut jenis kelamin dan isolasi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-154` | Penanda tempat tidur menolak jenis kelamin | `BE-RWI-013` ✅ | `RWI-AC-128` | Bagian 2A.1 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-155` | Kamar tidak boleh campur | `BE-RWI-013` ✅, `FE-RWI-007` | `RWI-AC-130` | Bagian 2A.1; `UAT-29` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-156` | Boks bayi dikecualikan dua arah | `BE-RWI-013` ✅ | `RWI-AC-131`, `RWI-AC-132` | Bagian 2A.2; `UAT-30` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-157` | Jenis kelamin belum tercatat | `BE-RWI-013` ✅ | `RWI-AC-129` | Bagian 2A.1 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-158` | Isolasi atribut episode | `BE-RWI-014` 🟡 | `RWI-AC-136` | Bagian 2A.4 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-159` | Catatan awal vs keputusan klinis | `BE-RWI-014` 🟡, `FE-RWI-006`, `FE-RWI-009` | `RWI-AC-136`, `RWI-AC-137`, `RWI-AC-139` | Bagian 2A.4; `UAT-32` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-160` | Tempat tidur isolasi dijaga dua arah | `BE-RWI-015` ✅ | `RWI-AC-134`, `RWI-AC-135` | Bagian 2A.3; `UAT-31` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |
| `FR-RI-161` | Perubahan isolasi tidak ditahan; daftar pantau | `BE-RWI-015` ✅, `FE-RWI-016` ✅ | `RWI-AC-138` | Bagian 2A.5; `UAT-33` — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026**. Daftar penempatan tidak sesuai bernada berbeda dari tiga daftar lain: tanpa kolom keterlambatan sama sekali, membawa tindakan berikutnya "Pindahkan Pasien", dan menyangkal nada tuduhan dengan kalimat yang dikunci test ([laporan](../task/report/frontend/FE-RWI-016.md)) |
| `FR-RI-162` | Berlaku pada perpindahan | `BE-RWI-019` ✅ | `RWI-AC-133` | Bagian 2A.1 — kode ditulis 25 Agustus 2026; **build dan test hijau 26 Agustus 2026** |

**Enam puluh dua functional requirement, nol tanpa task.**

---

## 3. Arah balik — task → epic

Bagian ini memeriksa kebalikannya: adakah task yang tidak ada yang memintanya?

| Task | Epic yang dilayani | Bila tidak menempel epic, apa dasarnya |
| --- | --- | --- |
| `BE-RWI-001` ✅, `BE-RWI-002` 🟡, `BE-RWI-003` ✅, `BE-RWI-004` 🟡 | `EPIC RI-21` | — |
| `BE-RWI-005` 🟡 | `EPIC RI-31` | — |
| `BE-RWI-006` ⛔, `BE-RWI-032` | `EPIC RI-32` | — |
| `BE-RWI-007` ✅, `BE-RWI-008` 🟡, `BE-RWI-009` | `EPIC RI-21` | — |
| `BE-RWI-010` | `EPIC RI-22` | — |
| `BE-RWI-011`, `BE-RWI-012` | `EPIC RI-23` | — |
| `BE-RWI-013` s.d. `BE-RWI-015` | `EPIC RI-34` | — |
| `BE-RWI-016` | `EPIC RI-24` | — |
| `BE-RWI-017`, `BE-RWI-018` | `EPIC RI-25` | — |
| `BE-RWI-019` | `EPIC RI-26` | — |
| `BE-RWI-020` s.d. `BE-RWI-022` | `EPIC RI-27` | — |
| `BE-RWI-023` s.d. `BE-RWI-027` | `EPIC RI-28` | — |
| `BE-RWI-028`, `BE-RWI-029` | `EPIC RI-29` | — |
| `BE-RWI-030` | `EPIC RI-30` | — |
| `BE-RWI-031` | `EPIC RI-33` | — |
| `BE-RWI-033` | **Tidak ada** | `NFR-008`, `RWI-DEC-051`. Bukti penerimaan lintas epic |
| `FE-RWI-001` | `EPIC RI-32` | — |
| `FE-RWI-002` | **Tidak ada** | Kerangka lintas layar. `03-frontend-architecture.md` bagian 8 |
| `FE-RWI-003`, `FE-RWI-004` | `EPIC RI-31` | — |
| `FE-RWI-005` | `EPIC RI-22`, `EPIC RI-34` | — |
| `FE-RWI-006`, `FE-RWI-007` | `EPIC RI-21`, `EPIC RI-23`, `EPIC RI-34` | — |
| `FE-RWI-008` | `EPIC RI-24` | — |
| `FE-RWI-009` | `EPIC RI-21`, `EPIC RI-34` | — |
| `FE-RWI-010` | `EPIC RI-26` | — |
| `FE-RWI-011` | `EPIC RI-25` | — |
| `FE-RWI-012` | `EPIC RI-27` | — |
| `FE-RWI-013` | `EPIC RI-28` | — |
| `FE-RWI-014`, `FE-RWI-015` | `EPIC RI-27`, `EPIC RI-28` | — |
| `FE-RWI-016`, `FE-RWI-017` | `EPIC RI-29`, `EPIC RI-34` | — |
| `FE-RWI-018` | `EPIC RI-30` | — |
| `FE-RWI-019` | **Tidak ada** | `RWI-DEC-051`, `03-frontend-architecture.md` bagian 10 |

**Tiga task tanpa epic, ketiganya beralasan tertulis.** Tidak ada task yatim.

---

## 4. Decision ID → task

Hanya keputusan yang **mengikat implementasi** yang didaftar. Keputusan yang hanya menutup scope
ada pada bagian 6.

| Decision | Isinya | Task yang menegakkannya |
| --- | --- | --- |
| `RWI-DEC-008` | Pemesanan 2 jam, dapat diubah admin | `BE-RWI-005`, `BE-RWI-010` |
| `RWI-DEC-009` | Lima status episode, `InCare` dibuang | `BE-RWI-003`, `BE-RWI-007` |
| `RWI-DEC-010` | Batas pembatalan admisi | `BE-RWI-008` |
| `RWI-DEC-011`, `RWI-DEC-041` | Episode selalu menempel kunjungan | `BE-RWI-007` |
| `RWI-DEC-012` s.d. `RWI-DEC-014` | Kewenangan dan keutuhan perpindahan | `BE-RWI-019` |
| `RWI-DEC-015` | Gerbang keuangan dan jalan keluar supervisor | `BE-RWI-024`, `BE-RWI-026` |
| `RWI-DEC-016`, `RWI-DEC-017` | Keputusan pulang, lima cara pulang | `BE-RWI-020`, `BE-RWI-025` |
| `RWI-DEC-019` | Pasien titipan tidak dikenali; kelas mengikuti kamar | `BE-RWI-019` |
| `RWI-DEC-020` | Bayi punya episode sendiri | `BE-RWI-031` |
| `RWI-DEC-021` | Keadaan tempat tidur diperiksa ulang saat penempatan | `BE-RWI-011` |
| `RWI-DEC-022` s.d. `RWI-DEC-024` | Kewenangan DPJP | `BE-RWI-017`, `BE-RWI-019` |
| `RWI-DEC-026` | Daftar periksa administrasi menahan | `BE-RWI-023` |
| `RWI-DEC-027` | Lama dirawat dari selisih tanggal | `BE-RWI-016` |
| `RWI-DEC-028` | Sesi koreksi | `BE-RWI-030` |
| `RWI-DEC-030` | Kedaluwarsa `Draft` | `BE-RWI-008` |
| `RWI-DEC-032` | Daftar pantau berpenanggung jawab | `BE-RWI-029` |
| `RWI-DEC-033` | Obat pulang sebagai butir daftar periksa | `BE-RWI-023` |
| `RWI-DEC-039` | `MstBed.BedStatus` turun jadi salinan | `BE-RWI-006`, `BE-RWI-011`, `BE-RWI-029` |
| `RWI-DEC-040` | Kelayakan keuangan ditandai manual | `BE-RWI-024` |
| `RWI-DEC-048` | Seeder menolak produksi | `BE-RWI-002` |
| `RWI-DEC-049` | Perbaikan tombol tempat tidur | `FE-RWI-001` |
| `RWI-DEC-051` | Test menempel pada tiap task | Seluruh task; `BE-RWI-032`, `BE-RWI-033`, `FE-RWI-019` |
| `RWI-DEC-053` | Riwayat lokasi milik Rawat Inap | `BE-RWI-009`, `BE-RWI-011` |
| `RWI-DEC-054` | Satu pasien satu episode hadir | `BE-RWI-003`, `BE-RWI-012` |
| `RWI-DEC-055` | Kepergian fisik pasien | `BE-RWI-027`, `FE-RWI-015` |
| `RWI-DEC-056` | Penanda rawat gabung bayi | `BE-RWI-031` |
| `RWI-DEC-057` | Versi resume pulang | `BE-RWI-022` |
| `RWI-DEC-062` | Persetujuan pemilik modul tetangga | `BE-RWI-006` |
| `RWI-DEC-063` | Penanggung jawab data master | Gerbang bagi `BE-RWI-010` ke atas |
| `RWI-DEC-064` | Jenis kelamin dan isolasi **menolak** | `BE-RWI-013`, `BE-RWI-015`, `BE-RWI-019` |
| `RWI-DEC-065` | Isolasi atribut episode | `BE-RWI-003`, `BE-RWI-014`, `FE-RWI-006`, `FE-RWI-009` |
| `RWI-DEC-066` | Seluruh kamar tidak boleh campur, tanpa kolom baru | `BE-RWI-013` |
| `RWI-DEC-069` | Pemilik `EmergencyInstallationManagement` bernama: Rizki Gunawan | Gerbang bagi `INP-S09`; tidak ada task MVP |
| `RWI-DEC-070` | Pelonggaran mesin klinis meluas ke kunjungan `Emergency` | Tidak ada task modul ini — pelaksananya modul IGD lewat `IGD-DEC-068` |
| `RWI-DEC-071` | Justifikasi `RWI-DEC-041` ditulis ulang | Tidak ada task — keputusannya tidak berubah |
| `RWI-DEC-072` | Waktu tiba milik IGD; penempatan menunggu event `Tiba` | `BE-RWI-011` kriteria 7 sebagai penjaga; aturan penuhnya menunggu `INP-S09` |
| `RWI-DEC-073` | `OriginEncounterId` dikerjakan modul IGD | `BE-RWI-003` — menegaskan kriteria 5 tetap utuh; tidak ada pekerjaan kolom di modul ini |
| `RWI-DEC-074` | Blueprint revision `4` disetujui | Gerbang `BLUEPRINT_APPROVED` bagi `roadmap_revision` `2` |

---

## 5. Invariant dan penjaga → task

| Penjaga | Isinya | Ditegakkan oleh | Dibuktikan test |
| --- | --- | --- | --- |
| `INV-INP-01` | Episode aktif punya tepat satu penempatan aktif; dilonggarkan setelah kepergian dicatat | `BE-RWI-011`, `BE-RWI-027` | Bagian 2, 4A |
| `INV-INP-02` | Satu tempat tidur paling banyak satu penempatan aktif | `BE-RWI-003` index parsial, `BE-RWI-011` | Bagian 2 skenario tabrakan |
| `INV-INP-03` | Episode wajib punya DPJP | `BE-RWI-007` | Bagian 1 |
| `INV-INP-04` | Satu kunjungan satu episode | `BE-RWI-007` | Bagian 1 |
| `INV-INP-07` | Perpindahan utuh | `BE-RWI-019` | Bagian 3 |
| `INV-INP-10` | Satu pasien satu episode yang hadir | `BE-RWI-003` index parsial, `BE-RWI-012` | Bagian 1 |
| `GUARD-INP-01` | Perpindahan oleh DPJP aktif | `BE-RWI-017`, `BE-RWI-019` | Bagian 3; `FE-RWI-010`, `FE-RWI-019` |
| `GUARD-INP-02` | Keputusan pulang oleh DPJP aktif | `BE-RWI-020` | Bagian 7 |
| `GUARD-INP-03` | Penandatanganan resume oleh DPJP aktif | `BE-RWI-021` | Bagian 7; `UAT-10` |
| `GUARD-INP-04` | Perubahan isolasi setelah episode aktif hanya DPJP | `BE-RWI-014` | Bagian 2A.4; `FE-RWI-009`, `FE-RWI-019` |

**Empat penjaga tidak dapat dikerjakan mesin hak akses** dan ditulis di dalam service. Keempatnya
punya pasangan test di frontend juga, karena tombol yang pasti ditolak server tidak boleh tampil
aktif di layar.

---

## 6. Yang sengaja tidak ditelusuri, beserta dasarnya

| Yang tidak ada task-nya | Alasan | Decision ID |
| --- | --- | --- |
| Pengkajian, catatan dokter, CPPT, tindakan, visite | Slice di luar scope MVP | `DEC-INP-001` |
| Resep rawat inap dan obat pulang | Terikat konsultasi; di luar scope | `DEC-INP-001` |
| Serah terima IGD ke rawat inap | Di luar scope | `DEC-INP-002` |
| Persetujuan umum rawat inap | Di luar scope, menunggu pemilik hukum | `DEC-INP-003` |
| Pengiriman SATUSEHAT | Di luar scope | `DEC-INP-005` |
| Serah terima klinis antar shift | Di luar scope; isinya menunggu pemilik klinis | `DEC-INP-006`, `RWI-OQ-038` |
| Aturan klinis pasien meninggal dan kabur | Cara pulangnya dikenali sistem, aturan klinisnya menunggu pemilik klinis | `DEC-INP-007`, `RWI-OQ-039`, `RWI-DEC-059` |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice di luar scope | `DEC-INP-001` |
| Masa simpan riwayat status | Sudah dijawab, menunggu pemilik hukum | `RWI-OQ-035`, `RWI-DEC-060` |
| Tabel riwayat kebutuhan isolasi | Isolasi adalah **atribut**, bukan riwayat | `RWI-DEC-065` |
| Kolom "boleh campur" pada `MstRoom` | Ditolak tegas; diperiksa dari penghuni yang sedang ada | `RWI-DEC-066` |

Sebelas butir, **seluruhnya beralasan tertulis**. Tidak ada satu pun yang berbunyi "menyusul".

---

## 7. Ringkasan kelengkapan

| Yang diperiksa | Jumlah | Tertelusur | Lubang |
| --- | ---: | ---: | ---: |
| Epic | 14 | 14 | **0** |
| Functional requirement | 62 | 62 | **0** |
| Skenario UAT | 33 | 33 | **0** |
| Invariant dan penjaga | 10 | 10 | **0** |
| Decision yang mengikat implementasi | 32 | 32 | **0** |
| Task backend | 33 | 33 | **0** |
| Task frontend | 19 | 19 | **0** |

### Yang **belum** dapat diperiksa sekarang

| Butir | Kenapa belum | Kapan dapat diperiksa |
| --- | --- | --- |
| 139 acceptance criteria → berkas test yang benar-benar ada | Seluruh berkas test Rawat Inap **sudah dijalankan** pada 26 Agustus 2026: `dotnet test` hijau 255 dari 255. Yang belum tertutup tinggal test tabrakan dua transaksi terhadap PostgreSQL milik `BE-RWI-011` — provider InMemory tidak dapat mengujinya | `BE-RWI-033` |
| 49 endpoint baru → status tersedia pada api contract | **Sudah ditutup 26 Agustus 2026.** Dokumen Swagger memuat 49 operasi HTTP `inpatient`, cocok persis dengan 49 baris kontrak, dan lima endpoint yang dipanggil tanpa token menjawab 401 — bukti `[Authorize]` tegak saat runtime. Ke-49 baris karena itu dinaikkan menjadi `Tersedia`. Baris ke-50, perubahan perilaku `PATCH /beds/{id}/availability`, dinilai terpisah dan **masih terblokir** oleh `BE-RWI-006` | `BE-RWI-033` |
| Cakupan e2e frontend | Bertambah sejak 26 Agustus 2026: kini ada empat unit test dan tiga e2e, dengan `tests/unit/inpatient-bed-status.test.mjs`, `tests/unit/inpatient-foundation.test.mjs`, dan `tests/e2e/bed-status-toggle.spec.mjs` menyentuh Rawat Inap. Layar operasional modul ini sendiri belum punya cakupan | `FE-RWI-019` |

Ketiganya sudah sangat menyusut sejak 26 Agustus 2026. Yang tersisa adalah satu test tabrakan
transaksi, satu baris kontrak yang menunggu `BE-RWI-006`, dan cakupan e2e untuk layar operasional
yang memang belum dibuat. Ketiganya punya task penutup yang memeriksanya.

---

## 8. Gerbang sebelum roadmap ini boleh dijalankan

| Gerbang | Keadaannya |
| --- | --- |
| ~~**Approval blueprint**~~ | **DICABUT 24 Agustus 2026** oleh `RWI-DEC-067`. Disetujui Muhammad Hamzah; `approved_by` pada metadata sudah terisi |
| Kesiapan data master beserta penanda yang benar | `RWI-DEC-063`, target 22 Agustus 2026. Menahan `BE-RWI-010` ke atas, **tidak** menahan `BE-RWI-001` s.d. `BE-RWI-004` |
| `FE-RWI-001` sebelum `BE-RWI-006` | Lintas repository, wajib diurutkan. **Diperiksa ulang 26 Agustus 2026 dan hampir tertutup** — `FE-RWI-001` selesai dan terbukti, tetapi perubahannya belum di-commit. `BE-RWI-006` tetap ⛔ sampai perubahan frontend itu rilis |
| ~~Registry lifecycle `PLANNED`~~ | **DICABUT 24 Agustus 2026** oleh `RWI-DEC-068`. Modul naik `PLANNED` → `ACTIVE` |
| Tidak ada connection string lokal | Baru — ditemukan saat `BE-RWI-001`. Menahan **cara aman** menjalankan migration, bukan penulisan kodenya |

Ketiga gerbang produksi pada `blueprint-manifest.md` bagian 7.2 — masa simpan data,
interoperabilitas nasional, dan persetujuan pasien — **tidak** menahan pengerjaan MVP. Yang
tertahan olehnya hanya kesiapan melayani pasien sungguhan.
