# Laporan Live Testing — Seleksi Administrasi

| Field | Nilai |
| --- | --- |
| Menu | Rekrutmen & Seleksi → **Seleksi Administrasi** (`/hr/recruitment/screenings`) |
| Pemilik menu | Recruiter |
| Task | `FE-HRD-409`, layar `FE-HRD-404`, menu `FE-HRD-408` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev `QuilvianNewDevHamzah` |
| Peramban | Chromium headed, `slowMo` 450 ms, desktop dan ponsel 390 px |
| Akun uji | `uat.pkb.recruiter@rsmmc.local` (Recruiter), `uat.pkb.asesor@rsmmc.local` (tanpa hak penapisan) |
| Hasil | **5 dari 5 skenario lulus**; 0 galat server |

## 1. Ringkasan

Layar terbuka dengan saringan bawaan **Menunggu** dan memanggil backend nyata. Database dev belum memiliki lowongan, kandidat, maupun lamaran, sehingga layar menampilkan "Tidak ada lamaran yang menunggu penapisan." beserta tombol **Lihat semua**, yang memuat ulang daftar tanpa saringan status. Asesor yang tidak memegang `CandidateScreening : Read` tidak melihat butir menu dan mendapat pesan hak bila membuka alamatnya.

## 2. Alur proses bisnis yang diuji

Contoh: Pak Dodi (Recruiter) memulai hari dengan memeriksa lamaran yang belum ditapis.

1. Pak Dodi memilih **Rekrutmen & Seleksi → Seleksi Administrasi**.
2. Layar meminta daftar penapisan berstatus Menunggu. Karena belum ada lamaran, muncul "Tidak ada lamaran yang menunggu penapisan."
3. Pak Dodi menekan **Lihat semua** untuk melihat semua status; daftar tetap kosong ("Belum ada data pada saringan ini.").

Jalur gagal: Bu Sari (asesor klinis) membuka alamat layar ini langsung dan melihat "Anda belum punya hak untuk melihat seleksi administrasi."; daftar penapisan tidak diminta ke server.

## 3. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R01 | P1 | HRD-130 | Menu Recruiter | Ada Seleksi Administrasi dan Tes/Wawancara; tidak ada Dashboard SDM | Sesuai | ✅ PASS | — |
| R02 | P0 | HRD-126 | Bawaan saringan Menunggu | `GET screenings?status=Pending` 200; pesan kosong | HTTP 200; pesan tampil | ✅ PASS | `fe-hrd-401/recruiter-01-seleksi-administrasi-kosong.png` |
| R03 | P2 | HRD-126 | Tombol Lihat semua | Request ulang tanpa `status` | HTTP 200 | ✅ PASS | — |
| R07 | P4 | HRD-126 | Tampilan ponsel | Tanpa gulir mendatar | 0 px | ✅ PASS | `fe-hrd-401/recruiter-04-seleksi-ponsel.png` |
| A03 | P1 | HRD-126 | Asesor membuka alamat langsung | Pesan hak; tanpa request daftar | Pesan tampil; 0 panggilan | ✅ PASS | `fe-hrd-401/asesor-02-seleksi-tanpa-hak.png` |

## 4. Endpoint yang diuji

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate Screening")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/screenings` | Daftar penapisan per lamaran | `CandidateScreening : Read` | `status`, `vacancyId`, `clinicalResult`, `search`, `pageNumber`, `pageSize` | `200`, `PagedResult` kosong |

## 5. Temuan

Tidak ada temuan pada layar.

## 6. Batasan

Jalur dengan data (lamaran IGD Sari berstatus Menunggu, aksi **Periksa**, hasil STR "Gugur") belum dapat diuji dengan data nyata. Database dev tidak memiliki satu pun pengajuan kebutuhan tenaga, permintaan rekrutmen, lowongan, kandidat, atau lamaran. Membuat lowongan mensyaratkan pengajuan headcount yang disetujui melalui workflow Perencanaan SDM bertahap. Perilaku dengan data dibuktikan lewat uji Playwright dengan response tiruan pada `FE-HRD-404`. Uji ulang dengan data nyata dapat dilakukan setelah satu lowongan uji tersedia.
