# Laporan Verifikasi Frontend — `FE-IGD-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-012` |
| Judul | Penolakan `409` jalur triase tampil dengan pesan backend — kriteria 3 diuji ulang |
| Slice | `IGD-S01` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 1 |
| Requirement | `FR-IGD-013`, `FR-IGD-014` — sisi tampilan |
| Kontrak | Validation `0.3.0` bagian 2 aturan 4 dan 5 — **nol perubahan** |
| Klasifikasi | `VERIFY` — **nol perubahan source** |
| Source | Frontend `RizkiV2` `25a9403fa`, dilayani `npm run dev` pemilik di port 3000; backend `rizkiG` `cdffb114` di `https://localhost:7184` |
| Laporan sebelumnya | [fe-igd-012-018](fe-igd-012-018-penyelesaian-antarmuka.md); [penilaian lima putaran](../../../evidence/2026-10-26-penilaian-lima-putaran-d3-l1-l1r-e2e.md) bagian 2 |
| Tanggal | 27 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–4 terpenuhi** |

---

## 1. Yang tersisa sebelum laporan ini

Kriteria 3 — *tidak ada layar yang menampilkan status kunjungan sebagai `Triaged` setelah pasien
`InTreatment` dinilai ulang* — kurang satu layar. Putaran `L1-03` (22 Oktober 2026) membuktikan
penilaian ulang tersimpan (`isRetriage: true`) dan memeriksa *Daftar Kunjungan* serta *Detail
Assesmen*. *Ruang Kerja Dokter* tidak pernah terbuka karena skripnya menuju `/doctor-emergency`;
`L1-03-dokter.png` ternyata halaman *COMING SOON!*, dan kata *"Sesuai"* di `hasil.json` lahir justru
karena barisnya tidak ditemukan.

## 2. Uji ulang `T1-L1-03R`

Bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fe-igd-012-l1-03r-20261027/` (di-gitignore).
Playwright `msedge` headless, akun dokter `QUILVIAN_DOKTER_*` (dr. Jenah Ali). Setiap verdict dihitung
dari nilai DOM atau jaringan yang diukur dan menjadi konjungsi wajib; `hasil.json` dirakit
`skrip/rakit-hasil.mjs`. Ketiga PNG diperiksa langsung.

**Prasyarat, diukur lewat API (`00-sonde-kunjungan.json`).** Kunjungan `IGD-261018123252-672191`
(Pasien Uji K1-A 4287, kunjungan yang sama dengan `L1-03`) berstatus `visitStatus` **4**
(`InTreatment`). Riwayat triasenya tiga baris, dua di antaranya `isRetriage: true`; yang terakhir
22 Oktober 2026 02.32 UTC. Status kunjungan tidak berubah sejak `L1-03`, sehingga layar hari ini
memperlihatkan keadaan sesudah penilaian ulang yang sama.

| # | Pemeriksaan | Terukur | Hasil |
| ---: | --- | --- | :---: |
| 1 | Route benar, bukan *COMING SOON* | `/health-services/emergency-installation-management/doctor-emergency`; 0 teks *coming soon* | ✅ |
| 2 | Segmen *Semua pasien* tersedia | 1 — dipakai karena DPJP kunjungan ini dr. B4 IGD Utama, bukan akun penguji | ✅ |
| 3 | Kartu pasien sasaran ditemukan **tepat satu** — penjaga lolos-hampa | 1 | ✅ |
| 4 | Kartu memuat *Sedang ditangani* | *"Tiba 18 Okt 2026, 19.31 Sedang ditangani Pasien Uji K1-A 4287 …"* | ✅ |
| 5 | Kartu tidak memuat *Sudah ditriage*/*ditriase*/*Triaged* | — | ✅ |
| 6 | Header ruang kerja terbuka | 1 | ✅ |
| 7 | Header memuat nomor kunjungan sasaran | `IGD-261018123252-672191` | ✅ |
| 8 | Header memuat *Sedang ditangani* | *"… Instalasi Gawat Darurat • Sedang ditangani • Tiba 18 Okt 2026, 19.31"* | ✅ |
| 9 | Header tidak memuat label `Triaged` | — | ✅ |
| 10 | Respons daftar menjawab kunjungan sasaran berstatus 4 | 4 (`jaringan-T1.json`) | ✅ |

`T1-dokter-daftar.png` dan `T1-dokter-kartu.png` memperlihatkan kartu berlencana *Sedang ditangani*;
`T1-dokter-ruang-kerja.png` memperlihatkan ruang kerja terbuka pada pasien itu dengan baris konteks
*"… Darurat • Sedang ditangani • Tiba 18 Okt 2026, 19.31"*.

## 3. Acceptance

| # | Kriteria | Bukti | Keadaan |
| ---: | --- | --- | :---: |
| 1 | Pesan backend apa adanya pada triase kunjungan tertutup | `L1-01-akhir.png` (22 Oktober 2026) | ✅ |
| 2 | Penolakan transisi menyebut status kunjungan saat ini | `L1-02-akhir.png` (22 Oktober 2026) | ✅ |
| 3 | Tidak ada layar yang menampilkan `Triaged` sesudah pasien `InTreatment` dinilai ulang | *Daftar Kunjungan* dan *Detail Assesmen* — `L1-03` (22 Oktober 2026); **Ruang Kerja Dokter — `T1-L1-03R` 10/10 (27 Oktober 2026)** | ✅ |
| 4 | `normalizeErrorMessage` membaca `response.data.message` untuk `409` | Terpetakan 15 September 2026 | ✅ |

## 4. Validasi

| Perintah | Hasil |
| --- | --- |
| `node skrip/00-sonde-kunjungan.mjs` | `200`; `visitStatus` 4; 2 penilaian ulang |
| `node skrip/t1-layar-dokter.mjs` | `PASS` 10/10 |
| `node skrip/rakit-hasil.mjs` | `PASS` |
| ESLint, build | Tidak dijalankan — nol perubahan source |
| `git status` frontend | Bersih |

Nol data tercipta: uji ini hanya membaca.

## 5. Langkah berikutnya

Tidak ada untuk kartu ini.
