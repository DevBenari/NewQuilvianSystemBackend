# Laporan Perubahan Backend — `BE-RWI-247`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Konteks episode, bed, dan handoff Billing |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source**; live integration menunggu migration/runtime |

## Hasil

Episode bayi menyimpan `DeliveryCaseId` dan hanya menerima konteks kelahiran di rumah sakit.
Kasus harus sama dengan identitas bayi, obstetri, dan masih `InProgress`/`Completed`. Alur baru
tidak lagi menulis preferensi invoice atau rencana deposit. Field lama tetap nullable untuk baca
data historis. Validasi bed bayi dan predikat kelayakan service unit existing tetap digunakan,
sehingga encounter memakai unit dari room/bed yang benar.

## Bukti perubahan

- `InpNewbornAdmissionRules.cs`: konteks, kasus, care mode, bed bayi/rawat gabung/intensif.
- `InpEpisodeService.cs`: validasi ulang kasus sebelum open/update episode.
- `InpEpisode.cs`, konfigurasi EF, dan DTO konteks: `DeliveryCaseId`.
- `PatientEncounterController.cs` dan `ServiceUnitRegistrationRules.cs`: kelayakan unit rawat inap.
- Event layanan Billing existing tidak diubah; tidak ada mutasi deposit dari Admisi.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Build backend Release | Lulus, 0 error |
| Model EF vs migration | Sinkron, tidak ada pending model changes |
| Migration database | Tidak dijalankan, sesuai batas wewenang |
| Live encounter/bed/Billing | Belum dijalankan karena schema runtime belum diperbarui |

Dependency `DEP-BIL-01` untuk alokasi saldo deposit ibu tetap berada di Billing/Kasir/AR.
