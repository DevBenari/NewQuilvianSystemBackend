# Laporan Perubahan Backend — `BE-RAD-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-18` |
| Judul | Kolom, tab, dan penyaring Daftar Pasien v1 pada `GET /rad-orders` |
| Slice | Paritas v1, Fase 1 — `RAD-EVD-001` bagian 6 |
| Roadmap | `roadmap/backend-roadmap.md` bagian 6c. Dibuka atas instruksi pemilik modul Yoga Aji Pratama, 2026-10-14 |
| Trace | `RAD-DEC-018`, `RAD-DEC-020`; `RAD-EVD-001` bagian 2.2 dan 3 |
| Contract version | `RAD-API-001` revision 13 — amandemen ditulis task ini |
| Dependency | Aturan baca pembayaran Lab `LabPaymentClearanceRules` (`LAB-API-v1` r43), dipakai ulang apa adanya |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Migrations/`, `Tests/QuilvianSystemBackend.RadiologyTests/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a6920`, branch `yoga` |
| Tanggal | 2026-10-14 (kode), 2026-10-15 (uji, dokumen) |
| Status | **Selesai — batas verifikasi.** Build lulus; 41/41 uji Radiologi lulus. Migration belum diterapkan ke devYoga |

---

## 1. Masalah

Daftar Pasien Radiologi v1 punya lima tab dan sepuluh kolom yang tidak dapat dibentuk dari `GET /rad-orders`:
tidak ada penanda persiapan, tidak ada penyaring "hari ini" maupun "tanpa batal", dan kolom No. Foto, Diagnosa,
Pemesan, Konfirmasi (nama), serta Pembayaran tidak ada di baris daftar.

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Models/RadOrder.cs` | Kolom `RequiresPreparation` |
| `DTOs/RadiologyDtos.cs` | `CreateRadOrderRequest.RequiresPreparation`; `RadOrderListQuery.OnlyPreparation`, `ExaminationDay`, `ExcludeCancelled`; sembilan field baru pada `RadOrderListResponse`; `ConfirmedAt` dipindah dari rincian ke baris daftar (rincian mewarisinya, JSON tidak berubah) |
| `Services/RadOrderService.cs` | Tiga penyaring di `TerapkanPenyaring`; `ExaminationDate`/`RequiresPreparation` pada proyeksi; `LengkapiKolomV1Async` — satu kueri berkelompok per kolom untuk seluruh halaman; metadata penyaring |
| `Migrations/20261014094349_AddRadOrderRequiresPreparation.cs` | `AddColumn<bool>` bawaan `false`; snapshot +3 baris |
| `Tests/.../RadOrderListV1ColumnsTests.cs` | 7 uji baru |

Aturan yang dipilih:

- **"Hari ini" pada kalender WIB.** Waktu di database UTC; petugas dan v1 membaca tanggal lokal.
- **Tanggal pemeriksaan** = `ScheduledAt` ?? `RequestedAt` ?? `CreateDateTime`.
- **Konfirmasi** = baris `Order.Accept` pertama pada `RadTransitionHistory` — sumber yang sama dengan rincian.
- **Pemesan** = `RequestedByUserId`, cadangan pembuat baris (pola Lab r47).
- **Pembayaran** memanggil `LabPaymentClearanceRules.ReadAsync` agar satu kunjungan terbaca sama di Lab dan Radiologi.

## 3. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet ef migrations add` (membangun proyek) | Lulus, sesudah satu perbaikan nullability `Guid` |
| `dotnet test` RadiologyTests | **41/41 lulus** (7 baru + 34 lama) |
| `dotnet ef migrations list` devYoga | `AddRadOrderRequiresPreparation` satu-satunya yang **Pending** |

**Satu jebakan uji.** Percobaan pertama 6 dari 7 uji baru gagal dengan daftar kosong. Penyebabnya data uji:
`ProcedureId`, `ModalityId`, dan `ServiceUnitId` adalah FK non-nullable, sehingga proyeksi daftar menggabungkan
master dengan **INNER JOIN** dan pesanan tanpa baris master ikut hilang. Uji kini menyiapkan satu tindakan, satu
modalitas, dan satu unit layanan. Di PostgreSQL FK ditegakkan, jadi perilaku produksi tidak terdampak.

## 4. Acceptance criteria

| AC | Bukti | Status |
| --- | --- | --- |
| Tab RJ/RI = jenis kunjungan + hari ini WIB | `HariPemeriksaan_HariIniLawanPerjanjian`, `TabRawatInap_MemakaiJenisKunjungan` | Terpenuhi |
| Tab Persiapan | `TabPersiapan_HanyaPesananBertandaPersiapan` | Terpenuhi |
| Tab Perjanjian = bukan hari ini | `HariPemeriksaan_HariIniLawanPerjanjian` | Terpenuhi |
| Daftar Pasien tanpa batal/ditolak, Riwayat tetap | `DaftarPasien_MenyembunyikanPesananBatalDanDitolak` | Terpenuhi |
| Kolom v1 dari tabel pemiliknya | `KolomV1_TerisiDariTabelPemiliknya`, `KolomV1_KosongBilaDatanyaMemangTidakAda` | Terpenuhi |
| Penyaring terbit di metadata | `Metadata_MenyebutKetigaPenyaringBaru` | Terpenuhi |
| Berjalan di devYoga | — | **Batas verifikasi**: migration belum diterapkan |

## 5. Yang terbuka

- **Migration belum diterapkan ke devYoga** — tanpa instruksi tidak ada eksekusi DB. Sebelum diterapkan, backend
  dengan kode ini akan gagal membaca `RadOrder` di devYoga (`42703`, kolom tidak ada). Cara aman: skrip
  `--idempotent` untuk satu migration ini.
- `GET /rad-orders` tetap berbatas `limit` 200 (bawaan) — Riwayat tanpa rentang tanggal akan terpotong di sana
  sampai perpindahan ke `PagedResult`.
