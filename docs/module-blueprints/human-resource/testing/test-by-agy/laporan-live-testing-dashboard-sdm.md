# Laporan Live Testing — Dashboard SDM

| Field | Nilai |
| --- | --- |
| Menu | Sumber Daya Manusia → **Dashboard SDM** (`/hr/dashboard`) |
| Pemilik menu | Manajer SDM |
| Task | `FE-HRD-409` (verifikasi Paket B), layar `FE-HRD-407`, menu `FE-HRD-408` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi di `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev bersama `QuilvianNewDevHamzah` |
| Peramban | Chromium headed (Playwright), `slowMo` 450 ms, desktop 1440 × 900 dan ponsel 390 × 844 |
| Akun uji | `uat.pkb.manajer@rsmmc.local` (Manajer SDM), `uat.pkb.recruiter@rsmmc.local` dan `uat.pkb.atasan@rsmmc.local` (tanpa hak) |
| Hasil | **10 dari 10 skenario lulus**; 0 galat server; 0 panggilan ke alamat usang |

## 1. Ringkasan

Dashboard SDM bekerja dengan data nyata. Manajer SDM melihat 40 pegawai aktif, distribusi profesi, status aktif/nonaktif, serta pesan kosong untuk kontrak dan dokumen karena database dev belum berisi kontrak atau lisensi. Angka Pegawai Aktif dapat diklik dan membuka daftar nama yang bisa dicari; baris daftar membuka Employee 360. Akun tanpa hak `HrdWorkforceDashboard : Read` tidak melihat butir menu dan mendapat pesan hak bila membuka alamatnya langsung, tanpa ringkasan dimuat.

## 2. Alur proses bisnis yang diuji

Contoh: Bu Maya (Manajer SDM) ingin tahu siapa saja pegawai aktif bulan ini.

1. Bu Maya masuk aplikasi dan memilih **Sumber Daya Manusia → Dashboard SDM**.
2. Layar memanggil ringkasan dan menampilkan "Pegawai Aktif 40", "Masuk Bulan Ini 0", "Keluar Bulan Ini 0", distribusi profesi, serta panel kontrak dan dokumen.
3. Bu Maya mengklik kartu **Pegawai Aktif**. Di bawah panel muncul "Daftar Nama — Ringkasan Pegawai · Pegawai Aktif" berisi 40 pegawai.
4. Bu Maya mengetik "UAT PKB Tono" di kolom cari; daftar menyisakan satu pegawai.
5. Bu Maya mengklik baris itu dan berpindah ke Employee 360 pegawai tersebut.

Jalur gagal: Pak Dodi (Recruiter) tidak memegang hak dashboard. Butir Dashboard SDM tidak tampil di menunya, dan bila ia membuka `/hr/dashboard` langsung, layar menampilkan "Anda belum punya hak untuk melihat Dashboard SDM." tanpa memanggil ringkasan.

## 3. Data uji

| Data | Asal |
| --- | --- |
| 40 pegawai aktif (34 pegawai dev + 6 pegawai uji "UAT PKB") | Database dev; pegawai uji dibuat lewat `POST master-data/employees` |
| 0 kontrak berjalan, 0 lisensi/SPK/sertifikat | Kondisi database dev apa adanya |

## 4. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| M01 | P1 | HRD-130 | Menu Manajer SDM | Ada Dashboard SDM; tidak ada pintu Paket B lain | Sesuai | ✅ PASS | — |
| M02 | P0 | HRD-125 | Ringkasan nyata dari `/summary` | Pegawai Aktif > 0, empat panel tampil | Pegawai Aktif 40; panel tampil | ✅ PASS | `fe-hrd-401/manajer-01-dashboard-desktop.png` |
| M03 | P2 | HRD-125 | Panel kontrak dan dokumen tanpa data | Pesan kosong, bukan angka nol palsu | "Tidak ada kontrak yang berakhir dalam 60 hari." dan "Tidak ada dokumen yang kedaluwarsa dalam 90 hari." | ✅ PASS | idem |
| M04 | P0 | HRD-125 | Klik Pegawai Aktif | `drilldown?panel=EMPLOYEE_OVERVIEW&bucket=ACTIVE` 200, daftar tampil | HTTP 200, 40 baris | ✅ PASS | `fe-hrd-401/manajer-02-drilldown-pegawai-aktif.png` |
| M05 | P2 | HRD-125 | Cari di daftar nama | `search` terkirim, sisa pegawai uji | HTTP 200, 1 baris | ✅ PASS | — |
| M06 | P0 | HRD-125 | Klik baris | Pindah ke `/hr/employee-360/{id}` | Sesuai | ✅ PASS | — |
| M08 | P4 | HRD-125 | Tampilan ponsel | Tanpa gulir mendatar | Kelebihan lebar 0 px | ✅ PASS | `fe-hrd-401/manajer-04-dashboard-ponsel.png` |
| R06 | P1 | HRD-125 | Recruiter membuka alamat langsung | Pesan hak; `/summary` tidak dipanggil | Pesan tampil; 0 panggilan | ✅ PASS | `fe-hrd-401/recruiter-03-dashboard-tanpa-hak.png` |
| T04 | P1 | HRD-125 | Atasan membuka alamat langsung | Pesan hak | Pesan tampil | ✅ PASS | — |
| SA-01 | P1 | DoD 43.11 | Hak di Manajemen Role | Posisi "UAT Paket B - Manajer SDM" memegang `Hrd Workforce Dashboard : Read` | Tercentang | ✅ PASS | `fe-hrd-401/superadmin-01-akses-role-manajer-sdm.png` |

Seluruh screenshot berada di `QuilvianSystemFrontendDev/test-with-agy/screenshots/`.

## 5. Endpoint yang diuji

### `[Tags("Corporate / Human Resource / Workforce Core / Workforce Dashboard")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Lima panel Dashboard SDM | `HrdWorkforceDashboard : Read` | Tanpa parameter | `200`, `ApiResponse<WorkforceDashboardSummaryResponse>` |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/drilldown` | Daftar nama di balik angka | `HrdWorkforceDashboard : Read` | `panel`, `bucket`, `professionId`, `documentType`, `search`, `pageNumber`, `pageSize` | `200`, `ApiResponse<PagedResult<WorkforceDashboardDrilldownItem>>` |

## 6. Temuan

Tidak ada temuan pada menu ini.

## 7. Batasan

Klik angka kontrak dan dokumen dengan data nyata tidak dapat diuji karena database dev tidak memiliki kontrak, lisensi, SPK, atau sertifikat. Perilaku dengan data terisi (contoh "SIP/STR Sudah Habis 2" → dr. Dodi dan Sari) dibuktikan lewat uji Playwright dengan response tiruan pada `FE-HRD-407`.
