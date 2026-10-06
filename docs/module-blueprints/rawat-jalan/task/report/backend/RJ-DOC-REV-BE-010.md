# Laporan Perubahan Backend — `RJ-DOC-REV-BE-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-010` |
| Judul | Pembatalan kunjungan dari Daftar Pasien Rawat Jalan |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `11.1` |
| Trace | `RJ-DOC-DEC-015`, `016`, `017`, `018`, `020`, `021`; desain `02-backend-architecture.md` DP.3.4, DP.8; `contracts/validation-matrix.md` `RJDP-VAL-002`..`006`; `contracts/state-transition-matrix.md` DP-A |
| Kontrak | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — `PATCH /outpatient-encounters/{id}/cancel` |
| Dependency | `RJ-DOC-REV-BE-009` ✅ |
| Wewenang | `RJ-DOC-DEC-025`; akun uji `UJI-RJDP` dari `BE-009` |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI — AT-DP-07..14 terbukti runtime |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (prefix `Reg`) |
| Keberlakuan | `NEW CODE` (tiga berkas `BE-009` yang diperluas) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-MOD-001`, Conformance Strict |
| Arketipe endpoint | Aksi `cancel` pada worklist. **Catatan standar:** pedoman transaksi menganjurkan `POST /{id}/<aksi>`; kontrak yang disetujui (`RJ-DOC-ENCLIST-001@1.0.0`) menetapkan `PATCH /{id}/cancel`, sama dengan `PATCH /patient-encounters/{id}/cancel` yang ada. Kontrak yang disetujui diikuti; tidak diubah sepihak |
| Hak akses | `[AccessAction("Cancel", "Cancel Outpatient Encounter", AccessType = AccessTypes.Update)]` + `[AccessPermission("OutpatientEncounter", "Cancel")]`. Tanpa hardcode role |
| Migration / DB | Tidak ada |

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientEncounterController.cs` | Endpoint `Cancel`; `OutpatientEncounterNotFoundException` → `404` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterListService.cs` | `CancelAsync`: cakupan → validasi alasan (1–250 setelah dipangkas) → cek terlihat dalam cakupan (`404` bila tidak) → transaksi: kunci baris `SELECT … FOR UPDATE` (pola `LeaveAccrualProcessorService`), periksa ulang `GetCancelBlockedReason` termasuk konsultasi aktif, isi kolom batal sama dengan endpoint lama, batalkan antrean yang belum selesai/batal → commit → notifikasi antrean (kegagalan hanya dicatat) → audit log `EntityId`/controller/action/status tanpa alasan dan data pasien. Dependency baru: `QueueRealtimeService`, `LoggerService` |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientEncounterDtos.cs` | `OutpatientEncounterCancelRequest` (tanpa data annotation agar pesan `RJDP-VAL-002` tepat), `OutpatientEncounterCancelResponse` |

`EncounterStatus` tidak diubah saat batal, sesuai DP-A. `PATCH /patient-encounters/{id}/cancel` lama
tidak disentuh (`RJ-DOC-DEC-017`).

## 3. Validasi

| Perintah / bukti | Hasil | Status |
| --- | --- | --- |
| `dotnet build … -c Release` | `0 Error(s)`, `244 Warning(s)` (sama dengan baseline) | `PASS` |
| QBE Strict × 3 berkas | `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah | — |

Hak `OutpatientEncounter : Cancel` ditambahkan (digabung, bukan ditimpa) ke jabatan Perawat Rawat
Jalan (12 → 13) dan Petugas Pendaftaran (2 → 3). Dokter Umum sengaja **tidak** diberi `Cancel`
untuk menguji AT-DP-12.

### Uji runtime (`rt_be010.log`, `rt_be010_status6.log`)

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| U1 | **AT-DP-07** — perawat uji membatalkan `ENC-RSMMC-00186` (status 3, 1 antrean) | `200`; `IsCancel true`, status tetap 3, antrean aktif 0, `cancelledQueueCount 1` | `PASS` |
| U2 | **AT-DP-07** — pasien yang sama didaftarkan lagi | `200` (`00187`) | `PASS` |
| U3 | **AT-DP-14** — dua `PATCH` bersamaan (perawat + pendaftaran) pada `00187` | `[200, 400]` | `PASS` |
| U4 | Batal lagi | `400` "Kunjungan sudah dibatalkan." | `PASS` |
| U6–U8 | **AT-DP-11** — alasan kosong, spasi saja, 251 karakter | `400` "Alasan pembatalan wajib diisi, maksimal 250 karakter." | `PASS` |
| U9 | **AT-DP-12** — dokter uji (tanpa `Cancel`) pada kunjungannya sendiri | `403` | `PASS` |
| U10 | **AT-DP-13** — perawat uji pada kunjungan nyata di poli lain | `404` "Kunjungan tidak ditemukan."; data tidak berubah | `PASS` |
| U11 | Id tidak ada | `404` | `PASS` |
| V1–V2 | **AT-DP-08** — `00190` dipindah ke status 6 tanpa konsultasi; daftar `canCancel true`; batal | `200` | `PASS` |
| V3–V4 | **AT-DP-09** — kunjungan nyata `ENC-RSMMC-00117` (status 6, konsultasi aktif): daftar menyatakan terblokir, lalu `PATCH` | `400` "Konsultasi masih aktif. Selesaikan atau batalkan konsultasi lewat workspace dokter."; `IsCancel` tetap `false` | `PASS` |
| U18 | **AT-DP-10** — status 7 | `400` "Kunjungan dengan status Konsultasi Selesai tidak dapat dibatalkan." | `PASS` |
| U19 | Tanpa autentikasi | `401` | `PASS` |
| U20 | Registry | `Cancel`, `Read`, `ReadAll` terdaftar | `PASS` |

Ringkasan: AT-DP-07..14 seluruhnya `PASS`. Skrip pertama `15/21`: enam kegagalan (U12–U17) berasal
dari rancangan uji — `POST /doctor-consultations` menolak konsultasi poliklinik di luar antrean
("Konsultasi untuk pasien poliklinik tetap harus lewat antrean."), sehingga `00188` tidak pernah
punya konsultasi dan pembatalannya di U13 justru benar (`200`). Skenario status 6 diulang dengan
skrip kedua (`4/4 PASS`). Untuk AT-DP-09 dipakai kunjungan nyata karena konsultasi uji memerlukan
alur skrining dan antrean dokter lengkap; `PATCH` hanya dikirim setelah daftar menyatakan
kunjungan itu terblokir, dan keadaannya diperiksa ulang sesudahnya (tidak berubah).

Keadaan data sesudah uji: `00186`–`00190` milik `KSKTEST-RM-07` seluruhnya `IsCancel = true`.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-DP-07` batal 0–5, antrean batal, daftar ulang | Terbukti | U1, U2 |
| `AT-DP-08` status 6 tanpa konsultasi aktif | Terbukti | V2 |
| `AT-DP-09` status 6 dengan konsultasi aktif ditolak | Terbukti | V4 |
| `AT-DP-10` status 7 ditolak | Terbukti | U18 |
| `AT-DP-11` alasan tidak sah | Terbukti | U6–U8 |
| `AT-DP-12` tanpa butir `Cancel` | Terbukti | U9 |
| `AT-DP-13` di luar cakupan | Terbukti | U10 |
| `AT-DP-14` batal ganda bersamaan | Terbukti | U3 |

## 5. Risiko tersisa

1. `R-DP-1` tetap: panggilan dokter pada milidetik yang sama dengan pembatalan. `start-consultation`
   tidak membaca `IsCancel` kunjungan; antrean yang batal mengecilkan peluangnya.
2. Logika pembatalan antrean kini ada di dua tempat (controller lama dan service baru).
3. Hak `Cancel` untuk jabatan Perawat Rawat Jalan dan Petugas Pendaftaran sudah aktif di DB dev.

## 6. Task berikutnya

`RJ-DOC-REV-FE-010` — layar Daftar Pasien Rawat Jalan dan butir menu.
