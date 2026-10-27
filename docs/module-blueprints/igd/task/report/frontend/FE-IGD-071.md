# Laporan Verifikasi Frontend — `FE-IGD-071`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-071` |
| Judul | Tab Tagihan Pasien IGD: monitoring baca-saja dan estimasi biaya |
| Slice | `IGD-DEC-349` · `SCR-IGD-P01` tab Tagihan Pasien |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.18 |
| Keputusan | `IGD-DEC-349`, `IGD-DEC-350` (syarat dua akun berbeda izin); pemberian izin uji `IGD-DEC-358` |
| Contract version | **Nol perubahan** — `encounter-billing-summaries` dan `charge-summary` di-reuse |
| Klasifikasi | `VERIFY` — **nol perubahan source**; konfigurasi role-access atas perintah pemilik |
| Source | Frontend `RizkiV2` `25a9403fa` + perubahan pass ini yang tidak menyentuh tab Tagihan; backend `rizkiG` `cdffb114` |
| Tanggal | 27 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–9 terpenuhi** |

---

## 1. Yang tersisa sebelum laporan ini

Kartu dibuat menyusul 26 Oktober 2026 dengan delapan dari sembilan acceptance terbukti oleh live
browser testing 23 Oktober (`UAT-IGD-BILLING-20261023`, 9 skenario `PASS`). Acceptance 5 — angka nominal
disamarkan bagi pengguna tanpa izin finansial, sementara rincian item tetap terbaca — belum pernah
diuji: seluruh skenario memakai `superadmin@admin.com`, dan `IGD-DEC-350` mensyaratkan **dua akun
berbeda izin, bukan SuperAdmin**.

## 2. Cara layar memutuskan penyamaran

`emergency-assessment-billing-tab.jsx` membaca tiga izin lewat `usePermission`, yang bersumber dari
`GET /v1/auth/permissions`:

| Izin | Gunanya |
| --- | --- |
| `EncounterBillingSummary : Read` | Membaca rincian tagihan (`encounter-billing-summaries`). Tanpa ini tab menolak dengan *"Izin yang dibutuhkan…"* — bukan menyamarkan |
| `BillingInvoice : Read` **atau** `PatientBillingSummary : ViewAmount` | Membuka angka nominal (`canViewAmount`). Tanpa keduanya, kartu nominal tertulis *Terproteksi* |

## 3. Mengapa izin harus diberikan lebih dulu

Sonde 27 Oktober (`00-sonde-izin-finansial.json`) atas keempat akun environment:

| Akun | Peran | `BillingInvoice:Read` | `ViewAmount` | `EncounterBillingSummary:Read` | Keadaan |
| --- | --- | :---: | :---: | :---: | --- |
| Loket — Rendy Saputra | — | ✔ | ✘ | ✘ | Ruang kerja IGD tidak terbuka (`GET emergency-visits` `403`) |
| Penerima — Siti Nurhaliza | — | ✘ | ✘ | ✘ | Tab menolak `403` |
| Dokter — dr. Jenah Ali | — | ✘ | ✘ | ✘ | Tab menolak `403` |
| Perawat — Dimas Kurniawan | **SuperAdmin** | ✔ | ✔ | ✔ | Tidak sah (`IGD-DEC-350`, `IGD-FACT-241`) |

Sonde layar sebelum pemberian izin (`hasil-S0-*.json`) membuktikan kedua akun non-SuperAdmin
**dapat** membuka ruang kerja pasien uji, dan yang menolak hanya `encounter-billing-summaries` `403`.
Daftar izin minimal karena itu pasti, dan pemilik memerintahkan pemberiannya (`IGD-DEC-358`).

| Posisi | Ditambah | Policy sebelum → sesudah | Policy lama hilang |
| --- | --- | :---: | :---: |
| Dokter IGD (dept `676f2aa7…` / pos `cd1cd442…`) | `EncounterBillingSummary:Read` | 233 → 234 | **0** |
| Akun penerima (dept `3aa08cc0…` / pos `20f0bbda…`) | `EncounterBillingSummary:Read`, `PatientBillingSummary:ViewAmount` | 20 → 22 | **0** |

Dikerjakan `skrip/10-beri-izin.mjs` mengikuti resep `10-beri-izin-dokter.mjs`: policy lama dibaca dan
dicadangkan (`10-policy-*-sebelum.json`), dikirim gabungan, lalu dibaca ulang dan dibandingkan.

## 4. Uji layar acceptance 5

Bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fe-igd-071-20261027/` (di-gitignore).
`npm run dev` pemilik di port 3000; Playwright `msedge` headless. Pasien uji: Aldo Barbeto `00-00-02-80`,
kunjungan `IGD-261023035629-36B0C8` *Sedang ditangani*, invoice `BIL-20261023-00000001` `OPEN`,
tagihan bruto `150000`, satu item *Jasa Kunjungan IGD*.

| Berkas | Akun | Harapan | Verdict | Catatan |
| --- | --- | --- | :---: | --- |
| `hasil-T3-dokter-tanpa-izin.json` | dokter | tersamar | FAIL 0/5 | **Salah rancang**: pemilih `input[placeholder*="Cari"]` mengenai kotak *Cari menu* di sidebar; ruang kerja tak pernah terbuka. Verdict tidak diubah |
| `hasil-T3-penerima-berizin.json` | penerima | terbuka | FAIL 4/5 | **Salah rancang**: nama item dibaca dari ruas yang salah; nama item ada di `services[].description` |
| `hasil-T3b-dokter-tanpa-izin.json` | dokter | tersamar | **PASS 5/5** | Ukur ulang dengan pemilih *Cari No. RM* |
| `hasil-T3b-penerima-berizin.json` | penerima | terbuka | **PASS 5/5** | Ukur ulang dengan ruas `description` |

| Pemeriksaan | dr. Jenah Ali (tanpa izin finansial) | Siti Nurhaliza (berizin finansial) |
| --- | --- | --- |
| Peran SuperAdmin | Tidak | Tidak |
| Ruang kerja dan tab Tagihan terbuka, tidak ditolak | ✔ | ✔ |
| `encounter-billing-summaries` | `200` | `200` |
| Item *Jasa Kunjungan IGD* tampil | ✔ | ✔ |
| Kartu nominal | **4× *Terproteksi*, nol nilai rupiah** | **`Rp 150.000,00`, `Rp 0,00`, `Rp 150.000,00`, `Rp 0,00`**, nol *Terproteksi* |
| PNG | `T3b-dokter-tanpa-izin-tab-tagihan.png` | `T3b-penerima-berizin-tab-tagihan.png` |

Kedua PNG diperiksa langsung: bilah fakta sama (No. RM, penjamin *Tunai*, invoice, status kasir
*Berjalan*), *Total Pelayanan 1* tampil pada keduanya, dan hanya kartu nominal yang berbeda.
`hasil.json` dirakit `skrip/rakit-hasil.mjs`: `PASS`.

## 5. Acceptance

| # | Kriteria | Keadaan | Bukti |
| ---: | --- | :---: | --- |
| 1 | Tab terbuka tanpa lencana *Belum tersambung* | ✅ | `P1-NAV-AVAILABLE`, `P0-TAB-LOAD` (23 Oktober) |
| 2 | Bilah fakta | ✅ | `P1-HEADER-FACTS`; terlihat lagi pada kedua PNG 27 Oktober |
| 3 | Lima kartu dan tabel rekapitulasi | ✅ | `P2-SUMMARY-CARDS`, `P2-CONTENT-PRESENT` |
| 4 | Tombol *Segarkan* | ✅ | `P3-REFRESH-BUTTON` |
| 5 | Nominal disamarkan tanpa izin finansial; rincian item tetap terbaca | ✅ | Bagian 4 — dua akun non-SuperAdmin berbeda izin |
| 6 | Nol tombol bayar/ubah; kotak regulasi kasir | ✅ | `P5-REGULATORY-ALERT` |
| 7 | Nol migration, nol perubahan kontrak | ✅ | `IGD-DEC-349` |
| 8 | `globals.css` dan komponen bersama tidak disentuh; nol komentar baru | ✅ | Nol perubahan source pada pass ini |
| 9 | ESLint 0 error | ✅ | Dilaporkan 23 Oktober; tab tidak berubah sejak itu |

## 6. Catatan, tidak dikartukan

- Izin uji berlaku bagi **seluruh pengguna** pada kedua posisi, termasuk posisi akun penerima yang
  bukan milik IGD (`IGD-DEC-358`). Bila tidak lagi dibutuhkan, kembalikan dari cadangan
  `10-policy-*-sebelum.json` dengan cara yang sama.
- Pada layar dokter, kartu DPJP tertulis *"dr. Jenah Ali • Di luar jadwal jaga — alasan: testing"*,
  sedangkan pada layar penerima *"dr. Jenah Ali"*. Penyajian berbeda per penampil, bukan bagian acceptance.

## 7. Validasi

| Perintah | Hasil |
| --- | --- |
| `node skrip/00-sonde-izin-finansial.mjs` | Tidak ada pasangan sah sebelum izin |
| `node skrip/10-beri-izin.mjs <akun> --terapkan` × 2 | `200` *"Policy role access berhasil disimpan."*; nol policy lama hilang |
| `node skrip/t3b-tab-tagihan.mjs` × 2 | `PASS` 5/5 dan 5/5 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — aturan pemilik |
| `MANUAL TEST` | `PASS` |
| ESLint, build | Tidak dijalankan untuk task ini — nol perubahan source |

## 8. Langkah berikutnya

Tidak ada untuk kartu ini.
