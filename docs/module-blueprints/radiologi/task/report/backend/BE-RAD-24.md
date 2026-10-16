# Laporan Perubahan Backend — `BE-RAD-24`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-24` |
| Judul | Laporan operasional Radiologi dan Tarif Tindakan (menu v1 Laporan, Pemeriksaan, Setup) |
| Trace | `RAD-DEC-022` (fitur bertanda), `RAD-EVD-001` bagian 7.4 |
| Contract version | `RAD-API-001` revision 17; `RAD-PERM-001` — tanpa string izin baru |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a692`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai.** 71/71 uji Radiologi; 2026-10-15 migration `BE-RAD-25` diterapkan dan keenam laporan 200 terhadap devYoga |

---

## 1. Alur v1 dan penerapannya

Di v1 tayang menu Laporan (Statistik Pemeriksaan, Dokter Pengirim, Pasien Perusahaan), Pemeriksaan (Harian, Per Item),
dan Setup → Tarif Tindakan masih "Coming Soon". Pemilik modul meminta fitur bertanda yang dapat dibangun dikerjakan
(2026-10-15), sehingga isinya diturunkan dari pesanan V2:

| Endpoint (`rad-orders/reports/…`) | Isi |
| --- | --- |
| `examination-statistics` | Per pemeriksaan + alat: total, rawat jalan/inap/IGD, CITO, selesai, batal |
| `referrers` | Per dokter pemberi instruksi; bila kosong, per akun pemesan (`ReferrerSource`) |
| `company-patients` | Baris pesanan berpenjamin perusahaan (`PaymentType` `CompanyGuarantor`) dengan nama perusahaan |
| `daily` | Satu baris per tanggal WIB, termasuk hari tanpa pesanan |
| `per-item` | Rincian pesanan; ≤ 5.000 baris, `IsTruncated` bila terpotong |
| `procedure-tariffs` | Pemeriksaan `IsRadiology` beserta `MstTariff`-nya (baca-saja; pengisian milik master tarif) |

Rentang `startDate`/`endDate` (`yyyy-MM-dd`) dibaca sebagai tanggal WIB; kosong = 30 hari terakhir; lebih dari 366
hari atau akhir sebelum awal → `400`. Seluruhnya `RadOrder : Read`; tidak ada tabel baru.

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet build` (tanpa analyzer) | Lulus |
| `dotnet test` RadiologyTests | **71/71 lulus** — 4 uji laporan di `RadV1FeatureTests.cs` |

## 3. Batas verifikasi

Belum diuji HTTP terhadap devYoga: kode yang sama memetakan `RadOrder.PrintFilm` (`BE-RAD-25`), sehingga backend ini
baru boleh dijalankan terhadap devYoga sesudah migration `AddRadReportTemplateAndFilmPrint` diterapkan.
