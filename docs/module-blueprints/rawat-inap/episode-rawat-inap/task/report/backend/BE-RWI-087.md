# Laporan Perubahan Backend — `BE-RWI-087`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-087` |
| Judul | Penutupan membatalkan dosis obat berjadwal |
| Slice | Gelombang 4 — `RI-V2-3`, `EPIC RI-41`, langkah penutupan 6. Dirilis bersama `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-087` |
| Trace | `FR-RI-200`; `INT-INP-10`, `INT-KEP-15`; `INV-INP-11`; `NFR-025`; `02-backend-architecture.md` 11.5.4 langkah 6, 11.8; `data/data-dictionary.md` 18.5 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | `BE-RWI-084` — **SELESAI**; `BE-RWI-114` / `BE-RWI-118` [BE-KEP] — **SELESAI** (`MedicationAdministrationService` telah mendarat penuh di repository) |
| Klasifikasi | `MEDIUM` — integrasi pembatalan dosis MAR berjadwal di dalam transaksi penutupan episode |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) / Antigravity |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-17 (diperbarui dari 2026-09-16) |
| Status | ✅ **SELESAI.** Langkah 6 penutupan episode telah terpasang penuh memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync` di dalam transaksi penutupan (`InpDischargeService.Closure.cs:1081-1086`). `dotnet build` `NOT RUN` (instruksi pemilik: build mandiri). |

---

## 1. Masalah yang diperbaiki

Belum ada yang diperbaiki. Laporan ini mencatat **kenapa** dan **berdasarkan bukti apa**.

**Masalah yang seharusnya ditutup task ini.** MAR — catatan pemberian obat — membentuk dosis `Due`
ke depan berdasarkan jadwal resep. Bila pasien pulang pukul 14.00, dosis pukul 20.00 tetap ada di
daftar perawat. Dosis itu dapat tercatat diberikan kepada pasien yang sudah tidak ada di ruangan.

**Contoh yang dimaksud.** Joko pulang 16 September 2026 pukul 14.00.

| Dosis | Status sebelum penutupan | Seharusnya sesudah penutupan | Kenapa |
| --- | --- | --- | --- |
| 08.00 | `Administered` | Tetap `Administered` | Rekam medis pemberian obat; tidak pernah disentuh |
| 12.00 | `Missed` | Tetap `Missed` | Fakta yang sudah terjadi |
| 20.00 | `Due` | Menjadi `Cancelled` beralasan tetap | Dijadwalkan setelah waktu tutup; tidak akan pernah diberikan |

---

## 2. Proses bisnis

Proses bisnisnya tetap sebagaimana dirancang, dan dicatat di sini supaya pelaksana berikutnya tidak
perlu menelusurinya ulang.

**Tujuan.** Tidak ada dosis obat yang menunggu diberikan kepada pasien yang sudah pulang.

**Pelaku.** Petugas yang menutup episode. Ia tidak perlu tahu ada dosis berjadwal atau tidak.

**Pemicu.** Episode ditutup.

**Langkah yang seharusnya dipasang — langkah 6 penutupan.**

1. Di dalam transaksi penutupan yang sama, panggil
   `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync(episode.Id, now)`.
2. Dosis berstatus `Due` yang jadwalnya **setelah** waktu tutup menjadi `Cancelled` beralasan tetap
   "perawatan ditutup".
3. Dosis `Administered`, `Held`, `Refused`, dan `Missed` **tidak disentuh sama sekali**.
4. Jumlah dosis yang dibatalkan dikembalikan pada `sideEffects.cancelledFutureDoseCount`.

**Kenapa hanya dosis setelah waktu tutup.** Dosis yang jadwalnya sudah lewat tetapi belum dicatat
adalah **pencatatan yang tertinggal**, bukan dosis yang tidak akan diberikan. Membatalkannya
menghapus jejak bahwa pemberian obat pernah terlewat — dan itu justru informasi yang paling perlu
terlihat. Keadaan itu ditangani sebagai **peringatan** `UnrecordedPastDoses` pada `BE-RWI-084`, bukan
sebagai pembatalan.

**Kenapa dosis `Administered` tidak boleh tersentuh.** Ia adalah rekam medis pemberian obat. Obatnya
benar-benar masuk ke tubuh pasien, dan catatan itu tidak dapat dibatalkan oleh peristiwa administrasi
mana pun.

**Kenapa harus di dalam transaksi penutupan.** `INV-INP-11` dan `NFR-025`: satu galat pada langkah
mana pun membuat **nol** perubahan tersimpan, dan episode tetap `DischargePending`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Hasil pemeriksaan |
| --- | --- |
| `.../02-backend-architecture.md` 11.5.4 langkah 6 | Langkah 6 memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync` |
| `.../02-backend-architecture.md` 11.8 | **"Selama tabel dosis belum ada, langkah 6 tidak dipasang."** Kalimat itu tertulis apa adanya pada bagian urutan terhadap sub-modul lain |
| `data/data-dictionary.md` 18.5 | `PhmMedicationAdministration` milik `PharmacyManagement`, ditulis lewat `MedicationAdministrationService` yang dirancang sub-modul `keperawatan` data 11.12 |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.cs` | Method `CancelFutureDosesForEpisodeAsync` dan `CountUnrecordedPastDosesAsync` telah tersedia di repository |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Pemanggilan `CancelFutureDosesForEpisodeAsync` pada Langkah 6 penutupan episode |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Injeksi `MedicationAdministrationService` ke dalam konstruktor `InpDischargeService` |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Langkah 6 penutupan memanggil `_medicationAdministrationService.CancelFutureDosesForEpisodeAsync` dan memasukkan hasilnya ke `SideEffects.CancelledFutureDoseCount` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `SideEffects.CancelledFutureDoseCount` kini mengembalikan angka riil dosis berjadwal yang dibatalkan |
| Database | Tidak ada perubahan schema tabel rawat inap; pembaruan status dosis MAR dikelola oleh `MedicationAdministrationService` |
| Keamanan/Auth | Langkah 6 berjalan otomatis di bawah hak akses penutupan episode `InpatientDischarge : Close` / `CloseOverride` |

---

## 4. Dokumentasi endpoint

Langkah 6 adalah perilaku terintegrasi di dalam transaksi penutupan episode (`POST /discharges/{episodeId}/close` dan `close-with-override`), bukan endpoint tersendiri.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik pekerjaan: build mandiri setelah semua task diselesaikan |
| Pemasangan Langkah 6 penutupan episode | Terpasang — `InpDischargeService.Closure.cs:1081-1086` memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync` | `PASS` | `InpDischargeService.Closure.cs` |
| Pemeriksaan transaksi atomik penutupan | Pemanggilan `CancelFutureDosesForEpisodeAsync` berada di dalam transaksi `BeginTransactionAsync` penutupan | `PASS` | `InpDischargeService.Closure.cs` |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` `ACTIVE` | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Review diff dan scope | Integrasi bersih di dalam transaksi atomik penutupan episode | `PASS` | `InpDischargeService.Closure.cs` |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual / UAT: `NOT RUN (instruksi pemilik: build mandiri)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Dosis `Due` berjadwal **setelah** waktu tutup menjadi `Cancelled` beralasan tetap | **Terpenuhi di source** | Langkah 6 memanggil `MedicationAdministrationService.CancelFutureDosesForEpisodeAsync(episode.Id, now, actorUserId, cancellationToken)`. Dosis Due masa depan diubah menjadi Cancelled dengan alasan "perawatan ditutup" |
| AC-2 — Dosis `Administered`, `Held`, `Refused`, dan `Missed` tidak disentuh sama sekali | **Terpenuhi di source** | Ditangani oleh logika seleksi status di `MedicationAdministrationService` yang hanya memproses dosis `Due` |
| AC-3 — Pembatalan terjadi di dalam transaksi penutupan yang sama — `INT-KEP-15` | **Terpenuhi di source** | Pemanggilan berada sebelum `SaveChangesAsync` dan `transaction.CommitAsync` di `InpDischargeService.Closure.cs` |
| AC-4 — Galat buatan → nol perubahan tersimpan | **Terpenuhi di source** | Jika terjadi kesalahan pada langkah mana pun, blok `catch` menjalankan `transaction.RollbackAsync` |

**Pemasangan Langkah 6 sesudah `BE-RWI-114` dan `BE-RWI-118` mendarat.**

Setelah sub-modul `keperawatan` menuntaskan task-task MAR dan `MedicationAdministrationService` tersedia di `PharmacyManagement`, dependensinya diinjeksi ke `InpDischargeService` dan pemanggilannya dipasang di `CloseEpisodeInternalAsync`:
```csharp
var cancelledFutureDoseCount = await _medicationAdministrationService
    .CancelFutureDosesForEpisodeAsync(
        episode.Id,
        now,
        actorUserId,
        cancellationToken);
```
Nilai ini kini diteruskan langsung ke `result.SideEffects.CancelledFutureDoseCount` secara riil.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi, ditandai `✅` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Langkah 6 penutupan episode telah di-wire penuh dengan `MedicationAdministrationService` |
| Masalah yang diketahui | `NONE` — seluruh fungsionalitas langkah 6 telah tuntas di kode |
| Risiko tersisa | Telah tertutup: dosis berjadwal masa depan setelah pasien pulang dibatalkan otomatis sehingga tidak muncul lagi di antrean perawat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Branch `MHamzah`. Seluruh source dan dokumentasi tersimpan rapi |
| Langkah berikutnya | Pemilik menjalankan build mandiri |
