# Impact Scan Realtime Pemilihan Bed — 17 Oktober 2026

## 1. Metadata

| Atribut | Nilai |
|---|---|
| Evidence ID | `RTB-IMP-20261017-01` |
| Scope | Notifikasi SignalR setelah reservasi, penempatan, atau transfer pasien berhasil menguasai bed |
| Pemilik bounded context | `episode-rawat-inap` / Inpatient Bed Occupancy |
| Backend snapshot | branch `MHamzah`, HEAD `d420c2fbe094a2d70776c44955233bce9c87bda8` |
| Frontend snapshot | branch `HamzahV2`, HEAD `0accf387138a3fcc1ff40fbb5bdeb960076b4d59` |
| Metode | Inspeksi source read-only; tidak menjalankan aplikasi, database, atau browser |
| Keputusan kebutuhan | `RWI-DEC-333`–`RWI-DEC-336`, `RWI-AC-469`–`RWI-AC-480` |

Working tree frontend telah berisi perubahan pengguna pada saat audit. Perubahan tersebut tidak dibuat,
diubah, atau dinilai sebagai bukti implementasi oleh assessment ini.

## 2. Ringkasan hasil

| Capability | Status | Bukti utama | Keputusan desain |
|---|---|---|---|
| Infrastruktur SignalR terautentikasi | Ready to reuse | `Program.cs:247,338,362-366,2191`; `QueueHub.cs:17-18` | Gunakan hub existing `/hubs/queues`; tidak membuat endpoint hub kedua |
| Grup realtime per unit layanan untuk pemilihan bed | Extend | `QueueHub.cs:43-213` sudah memiliki pola join/leave grup, tetapi belum ada grup rawat inap | Tambah join/leave yang memvalidasi akses unit dan permission identitas pasien |
| Titik post-commit pada mutasi bed | Extend | `InpBedOccupancyService.BedManagement.cs:12-69`; `InpBedMutationCoordinator.cs:28-103` | Publikasi hanya setelah coordinator selesai commit dan `meta.IsReplay == false` |
| Publisher dan payload event bed | Missing | Tidak ditemukan event/publisher rawat inap pada source snapshot | Tambah service realtime dan DTO payload nonpersisten |
| Scope akses unit dan identitas pasien | Reuse with adapter | `InpBedAccessService.cs:58-137` | Reuse aturan `UnitsAsync`, `InpatientBedOccupancy:Read`, dan `InpatientEpisode:Read` untuk otorisasi join |
| Klien SignalR, reconnect, dan rejoin | Reuse with adapter | `signalrHubClient.jsx:114-149`; `queue-realtime-client.js:128-248` | Extend factory klien queue agar memahami grup/event bed dan rejoin setelah reconnect |
| Refresh denah dan pembersihan pilihan usang | Ready to reuse | `use-floor-plan.js:359`; `floor-plan-selection.jsx:47-54`; guard 409 pada hook admisi/transfer | Event memanggil refresh; effect existing menghapus bed yang tidak lagi eligible |
| Alert realtime pada konteks pemilihan bed | Missing | Tidak ditemukan consumer event bed | Tambah hook consumer dan alert pada `FloorPlanSelection`, bukan pada popup kamar |

Tidak ada capability berstatus `Conflict` atau `Unknown`. Status di atas adalah hasil inspeksi statis,
bukan bukti runtime.

## 3. Bukti backend

### 3.1 Infrastruktur dan keamanan koneksi

- SignalR telah diregistrasikan melalui `AddSignalR` dan `QueueHub` telah dipetakan ke
  `/hubs/queues`.
- Token pada query string hanya diterima untuk path hub yang telah ditetapkan, termasuk
  `/hubs/queues`.
- `QueueHub` memakai `[Authorize]` dan telah memiliki pola group join/leave untuk nurse station,
  dokter, klinik, dan IGD.
- Belum ada method atau nama grup untuk konteks pemilihan bed rawat inap.

### 3.2 Boundary transaksi bed

- Mutasi publik existing adalah `ReserveBedAsync`, `PlacePatientAsync`, dan `TransferAsync`.
- Ketiganya memakai `InpBedMutationCoordinator.ExecuteAsync`; coordinator melakukan commit
  sebelum mengembalikan `BedOperationMeta`.
- `BedOperationMeta.IsReplay` membedakan hasil commit baru dari replay idempotency.
- Pola post-commit best-effort sudah ada pada transfer handover: kegagalan side effect setelah
  commit dicatat tanpa membatalkan transaksi bed.
- Endpoint HTTP existing tetap menjadi source of truth:
  `POST /reservations`, `POST /placements`, dan `POST /placements/transfer`.

### 3.3 Otorisasi dan data payload

- `InpBedAccessService.UnitsAsync` menghasilkan unit layanan yang dapat diakses actor untuk suatu
  operation.
- Pembacaan identitas pasien telah mensyaratkan `InpatientEpisode:Read` dan akses
  `InpatientBedOccupancy:Read` pada unit yang sama.
- Model existing menyediakan episode/patient, bed/room/service unit, serta actor user untuk
  membentuk payload tanpa tabel baru.
- Nama lengkap pasien dan display name actor bersifat sensitif; keduanya hanya boleh dikirim ke
  koneksi yang lolos akses unit dan permission identitas.

## 4. Bukti frontend

- Dependensi `@microsoft/signalr` versi `^8.0.17` sudah tersedia; tidak perlu package baru.
- Factory `createQueueRealtimeConnection` sudah menangani automatic reconnect dan rejoin grup
  existing sehingga menjadi titik extension terdekat.
- `useFloorPlan.refresh()` sudah menjadi jalur kanonis untuk memuat ulang kondisi dan eligibility
  bed.
- `FloorPlanSelection` sudah menghapus pilihan saat hasil refresh menyatakan bed terpilih tidak
  eligible.
- Hook admisi dan transfer telah menangani HTTP `409` dengan refresh; SignalR tidak menggantikan
  guard atomik ini, hanya memperkecil jendela konflik pengguna.

## 5. Gap yang harus ditutup

1. Method hub `JoinInpatientBedSelection(serviceUnitId)` dan
   `LeaveInpatientBedSelection(serviceUnitId)` beserta validasi scope.
2. Grup bernama deterministik `inpatient-bed-selection:{serviceUnitId}`.
3. Publisher best-effort post-commit untuk tiga mutasi yang menguasai bed.
4. Event `InpatientBedSelectionChanged` dengan payload minimum yang telah dikunci.
5. Consumer frontend yang aktif hanya selama layar pemilihan bed terpasang, menekan alert actor
   sendiri, melakukan deduplikasi sesi, refresh segera, dan rejoin/refetch setelah reconnect.
6. Pengujian otorisasi lintas unit, kegagalan publish, idempotency replay, reconnect tanpa replay,
   serta race yang tetap diselesaikan oleh conflict `409`.

## 6. Hal yang tidak dibuat

- Tidak ada webhook eksternal.
- Tidak ada tabel, kolom, migration, outbox, inbox notifikasi, atau riwayat alert persisten.
- Tidak ada permission string baru; aturan akses existing digunakan kembali.
- Tidak ada perubahan respons endpoint HTTP existing.
- Tidak ada soft lock ketika user hanya menyorot atau membuka detail bed.
- Tidak ada replay alert lama setelah reconnect.

## 7. Kesimpulan audit

Solusi diklasifikasikan sebagai **extension kecil lintas backend–frontend di bounded context yang
sama**. Infrastruktur koneksi, keamanan dasar, transaksi bed, refresh denah, dan conflict guard
sudah tersedia. Gap utamanya adalah kontrak grup/event, publisher post-commit, dan consumer alert.
Tidak diperlukan `hospital-domain-architect` karena tidak ada aggregate, lifecycle persisted,
billing, atau ownership lintas bounded context baru.
