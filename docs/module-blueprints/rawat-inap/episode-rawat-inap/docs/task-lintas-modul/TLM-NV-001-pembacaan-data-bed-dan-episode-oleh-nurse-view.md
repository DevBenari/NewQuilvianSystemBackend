# TLM-NV-001 — Pembacaan Data Bed dan Episode oleh Nurse View

```yaml
dokumen_id: TLM-NV-001
jenis: pemberitahuan ketergantungan baca lintas modul
modul_peminta: nurse-view
pemilik_peminta: Rizki Gunawan
modul_tujuan: rawat-inap
submodul_tujuan: episode-rawat-inap
pemilik_tujuan: Muhammad Hamzah
tanggal: "2026-10-28"
status: DISETUJUI — Muhammad Hamzah, 2026-10-28 (disampaikan Rizki Gunawan); tidak ada perubahan kode
keputusan_asal: [NV-DEC-006, NV-DEC-012, NV-DEC-015, NV-DEC-020, NV-DEC-025, NV-DEC-026]
source_sha_backend: "a6f977a3 (rizkiG)"
source_sha_frontend: "76b0c6e4b (RizkiV2)"
rujukan:
  - ../../../../nurse-view/00-interview-decisions.md
  - ../../../../nurse-view/01-existing-capability-map.md
```

---

## 1. Ringkasan

Modul **Nurse View** milik Rizki Gunawan adalah TV pasif di nurse station, satu per lantai, yang
menampilkan seluruh bed rawat inap di lantai itu beserta status dan penghuninya dalam bentuk
samaran, misalnya "B.S. · L · 64 th".

Nurse View **membaca** data milik `episode-rawat-inap`, tetapi **tidak mengubah kode, tabel, maupun
data** sub-modul ini. Dokumen ini memberi tahu apa saja yang dibaca, agar perubahan di sisi Rawat
Inap yang menyentuhnya bisa dikabarkan ke Rizki lebih dulu.

## 2. Data yang dibaca

| Data | Sumber | Dipakai untuk |
|---|---|---|
| Status bed enam kode (`Available`, `Occupied`, `Reserved`, `WaitingCleaning`, `Cleaning`, `Unavailable`) | `InpBedManagementQueryService.ProjectBatchAsync` (`internal`) dan `StatusLabels` | Kartu bed: Kosong / Terisi / Dipesan / Dibersihkan / Tidak tersedia (`NV-DEC-015`) |
| Penempatan dan reservasi aktif | `BedProjectionBatch.Placements`, `.Reservations` hasil metode di atas | Penghuni bed, lalu disamarkan di backend Nurse View |
| Perawat penanggung jawab | `InpNurseAssignment` aktif (`IsActive`, `EndDateTime == null`) | Nama perawat PJ, atau tanda "Belum ada PJ" (`NV-DEC-012`) |
| Kebutuhan isolasi | `InpEpisode.RequiresIsolation` dibandingkan `MstBed.IsIsolationBed` — aturan yang sama dengan `InpCensusQueryService.GetIsolationMismatchAsync` | Ikon isolasi dan tanda "Bed tidak sesuai isolasi" (`NV-DEC-020`) |
| Lantai dan bangsal kamar | `MstRoom.FloorCode`, `MstRoom.ServiceUnitId` (Master Data) | Pengelompokan papan per lantai dan per bangsal (`NV-DEC-006`, `025`) |

**Contoh.** Bed 301-A berstatus `Occupied` dari `ProjectBatchAsync`. Penempatannya menunjuk
episode Budi Santoso dengan `RequiresIsolation = true`, padahal 301-A bukan bed isolasi. TV lantai
3 menampilkan "301-A · B.S. · L · 64 th · Perawat Sari · ikon isolasi · Bed tidak sesuai isolasi".

## 3. Yang sengaja TIDAK dipakai Nurse View

| Tidak dipakai | Alasan |
|---|---|
| `HoldingContext` hasil proyeksi bed | Memuat nama lengkap dan No. RM; identitas di TV wajib samaran (`NV-DEC-008`) |
| `InpBedAccessService` untuk cakupan unit | Bersandar pada pengguna (`ActorId`); TV memakai akun perangkat per lantai |
| Saringan kamar `InpFloorPlanQueryService.LoadAsync` | Mensyaratkan `FloorPlanRoomKey`; papan Nurse View cukup `FloorCode` (`NV-DEC-025`) |
| Event `InpatientBedSelectionChanged` dan grup `inpatient-bed-selection:{unit}` | Payload memuat `PatientName` (`InpBedRealtimeService.cs` baris 78). Akun perangkat Nurse View **dilarang** bergabung ke grup itu (`NV-DEC-026`). Perubahan bed tertangkap lewat muat ulang ≤ 60 detik |

## 4. Dampak bagi episode-rawat-inap

| Aspek | Dampak |
|---|---|
| Kode, tabel, migration, endpoint | **Tidak ada perubahan** |
| Perilaku reservasi, penempatan, pembersihan, pulang | Tidak berubah |
| Hak akses Rawat Inap | Tidak berubah |
| Beban database | Satu pemuatan per lantai per TV setiap 60 detik, memakai `ProjectBatchAsync` yang jumlah query-nya tetap per permintaan |

## 5. Yang dibutuhkan dari Muhammad Hamzah

| No | Hal | Keterangan |
|---:|---|---|
| 1 | **Pertanyaan:** bolehkah Nurse View memanggil `InpBedManagementQueryService.ProjectBatchAsync` yang berstatus `internal`, atau Hamzah lebih suka menyediakan satu metode baca publik khusus? | Pilihan kedua berarti perubahan kode di sub-modul ini dan akan dibuatkan dokumen lanjutan |
| 2 | **Mohon kabar lebih dulu** bila salah satu berikut berubah | `ProjectBatchAsync` atau `StatusLabels`; arti `InpNurseAssignment` aktif; `InpEpisode.RequiresIsolation` atau aturan isolation-mismatch; isi payload event bed |

## 6. Status dan tindak lanjut

| Tanggal | Status | Catatan |
|---|---|---|
| 2026-10-28 | `PEMBERITAHUAN` | Ditulis dari closure pass Nurse View, sebelum `design-business-module`. Bila desain ternyata menuntut perubahan kode di sub-modul ini, dokumen baru ditambahkan di folder ini |
| 2026-10-28 | `DISETUJUI` | Muhammad Hamzah menyetujui usulan dokumen ini, disampaikan Rizki Gunawan: Nurse View **memanggil `ProjectBatchAsync` yang `internal` secara langsung** (pertanyaan 1 bagian 5), dan Hamzah akan mengabari bila data di bagian 2 berubah. Desain Nurse View tidak menuntut perubahan kode di sub-modul ini |

`integrasi-billing` dan `dokter-rawat-inap` **tidak tersentuh** Nurse View, sehingga tidak diberi
dokumen.
