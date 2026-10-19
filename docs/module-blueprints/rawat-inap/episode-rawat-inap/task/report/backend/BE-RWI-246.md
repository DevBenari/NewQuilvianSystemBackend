# Laporan Perubahan Backend — `BE-RWI-246`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Snapshot penjamin bayi mengikuti ibu |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source dan migration**; verifikasi runtime menunggu migration |

## Hasil

Pembuatan encounter bayi menerima `InheritedPaymentSourceId` dan versi sumber yang dilihat
frontend. Backend memvalidasi rantai bayi–ibu–kasus–encounter, mengunci sumber dalam transaksi,
menolak sumber basi dengan HTTP 409, lalu membuat snapshot penjamin baru milik encounter bayi.
FK kartu asuransi/perusahaan ibu tidak dipasang sebagai kartu milik bayi.

## Bukti perubahan

- `NewbornEncounterPaymentInheritanceService.cs`: validasi, version check, lock sumber, snapshot.
- `PatientEncounterController.cs`: preview sebelum validasi dan revalidasi di dalam transaksi.
- `RegPatientEncounterGuarantor`: provenance `InheritedFromPaymentSourceId/At`.
- `PatientEncounterDtos.cs`: request dan response provenance.
- Migration `20261016080745_ReviseNewbornAdmissionFlow`.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Tunai/asuransi/perusahaan | Semua memakai snapshot source; FK kartu patient-specific selalu null |
| Sumber berubah | Ditolak 409 sebelum encounter disimpan |
| Jalur non-bayi | Tetap memakai builder penjamin existing |
| Build backend Release | Lulus, 0 error |

Tidak ada penerapan database atau uji API runtime pada task ini.
