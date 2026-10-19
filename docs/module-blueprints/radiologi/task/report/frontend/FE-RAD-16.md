# Laporan Perubahan Frontend — `FE-RAD-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-16` |
| Judul | Beranda Radiologi |
| Slice | Paritas v1, Fase 3 — `RAD-EVD-001` bagian 2.1 |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 5c |
| Contract | `GET /rad-orders/dashboard` — `RAD-API-001` revision 14 (`BE-RAD-19`) |
| Task mode | `FRONTEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `478f08aa9`, branch `YogaV2` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** Layar terhadap API tiruan lulus; terhadap devYoga menunggu migration `BE-RAD-18` |

---

## 1. Yang tampil

Rute `/health-services/radiology-management/overview` (`RADIOLOGY_ROUTES.overview`, sudah terdefinisi sejak awal
tetapi belum punya halaman). Menu **Beranda** di urutan teratas grup Radiologi, seperti v1.

Susunan mengikuti v1 dengan komponen V2 (`Hero`, `BaseDetailSection`, `SummaryCards`, `DataTable`, ApexCharts tanpa
SSR — pola Beranda Lab `FE-LAB-53`):

1. **Ringkasan** — Total Booking (keterangan jumlah IGD), Rawat Jalan, Rawat Inap, Pemeriksaan Lunas (keterangan CITO).
2. **Status booking yang perlu dipantau** — Menunggu, Diproses, Selesai, CITO.
3. **Tren kunjungan 30 hari** (area) dan **komposisi pasien** (pie) berdampingan; legenda pie berangka dan
   berpersen, tren membawa total dan tabel tersembunyi untuk pembaca layar.
4. **Booking Terbaru** — lima pesanan: pasien + No. RM, order + CITO, jenis kunjungan, waktu pesan WIB, status.

Layar hanya membaca. Galat `401/403` ditampilkan `AccessDeniedGate`.

## 2. Selisih sengaja dari v1

| v1 | V2 | Alasan |
| --- | --- | --- |
| Komposisi Rajal/Ranap/Rujukan | + Gawat Darurat | Sama dengan tab IGD `FE-RAD-14` |
| Muat ulang otomatis tiap 30 menit | Tombol *Muat ulang* + keterangan "Diperbarui pukul …" | Angka yang berganti diam-diam menyesatkan |
| Hitungan atas ≤ 2.000 booking yang dimuat browser | 30 hari WIB, dihitung server | `BE-RAD-19` bagian 1 |

## 3. Berkas

Baru: `app/.../overview/{page,rad-overview-client}.jsx`, `components/view/.../rad-dashboard/rad-dashboard-{view,charts}.jsx`,
`lib/hooks/.../rad-dashboard-rules.js`, `lib/hooks/.../use-rad-dashboard.jsx`,
`style/.../rad-dashboard/rad-dashboard.module.css` (kelas grafik disalin dari Beranda Lab),
`tests/unit/rad-dashboard-rules.test.mjs`. Diubah: `menu-items.jsx`, `rad-order.service.js` (`getRadOrderDashboard`).

## 4. Validasi

| Bukti | Hasil |
| --- | --- |
| Uji unit `rad-dashboard-rules` + `rad-patient-list-rules` | 16/16 lulus (7 baru) |
| ESLint berkas yang disentuh | 0 error, 0 warning |
| `npm run build` | Lulus; rute `overview` terbentuk |
| Layar standalone 3710, API tiruan penuh (gabungan `FE-RAD-14` + `FE-RAD-16`) | **43/43 lulus** |

Butir Beranda yang dibuktikan: judul; empat kartu dan empat status v1; angka dari backend; total tren 30 hari;
legenda komposisi berangka (6 (50%), 1 (8%)); lima baris terbaru; tepat satu permintaan saat dibuka dan satu lagi
saat *Muat ulang*; nol tulis; `403` → *Akses Ditolak*; menu Beranda di atas Daftar Pasien Radiologi.

## 5. Batas verifikasi

Belum dibuka terhadap backend yang tersambung ke devYoga, karena migration `BE-RAD-18` belum diterapkan.
