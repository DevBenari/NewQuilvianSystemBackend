# FLOW-RI-MVP-021 — Notifikasi Realtime Pemilihan Bed

| Atribut | Nilai |
|---|---|
| Flow ID | `FLOW-RI-MVP-021` |
| Contract | `0.16.0` (`approved_design_planned`, `RWI-DEC-337`) |
| Trigger | Reservasi, penempatan, atau target transfer berhasil commit |
| Actor utama | Petugas yang melakukan mutasi dan petugas lain pada selection unit sama |
| Source of truth | State bed committed dan GET floor plan |

## Alur utama

```mermaid
flowchart TD
    A[User 1 membuka layar pemilihan bed] --> B[Client autentikasi ke /hubs/queues]
    B --> C{Join unit diizinkan?}
    C -- Tidak --> D[Tolak join; tidak ada payload sensitif]
    C -- Ya --> E[Join inpatient-bed-selection:serviceUnitId]

    F[User 2 reserve/place/transfer] --> G[Backend validasi permission eligibility version]
    G --> H{Mutasi berhasil commit?}
    H -- Tidak --> I[Return error/409; tidak publish]
    H -- Ya --> J{Idempotency replay?}
    J -- Ya --> K[Return receipt existing; tidak publish]
    J -- Tidak --> L[Bentuk payload minimum dari state committed]
    L --> M[Publish InpatientBedSelectionChanged ke grup unit target]
    M --> N{Publish berhasil?}
    N -- Tidak --> O[Log teknis tanpa nama; transaksi tetap sukses]
    N -- Ya --> P[Client User 1 menerima event]
    O --> Q[Return HTTP sukses existing]
    P --> R{Actor event = User 1?}
    R -- Ya --> S[Tekan alert sendiri; refetch boleh dilakukan]
    R -- Tidak --> T[Tampilkan alert bed pasien actor]
    T --> U[GET floor-plan terkini]
    S --> U
    U --> V{Bed terpilih masih eligible?}
    V -- Tidak --> W[Bersihkan selected bed]
    V -- Ya --> X[Pertahankan pilihan]
    W --> Y[User melanjutkan dengan state terkini]
    X --> Y
```

## Reconnect dan recovery

```mermaid
flowchart LR
    A[Koneksi putus] --> B[Automatic reconnect]
    B --> C[Rejoin seluruh unit aktif]
    C --> D[Refetch floor plan segera]
    D --> E[Render state authoritative]
    E --> F[Tidak replay alert lama]
```

## Failure path penting

| Kondisi | Hasil pengguna/sistem |
|---|---|
| User lain tidak berwenang pada unit | Tidak dapat join dan tidak menerima nama pasien |
| SignalR gagal setelah commit | Mutasi User 2 tetap sukses; User 1 akan pulih melalui refresh/reconnect |
| User 1 submit sebelum event selesai | Backend menentukan pemenang; yang kalah menerima `409` dan refresh |
| Refetch gagal | Alert tetap tampil, error loading existing tampil, payload tidak dipakai sebagai state |
| Popup kamar tertutup | Alert tetap diterima karena koneksi dimiliki selection screen |

## Output selesai

- Pengguna lain pada unit yang sama mengetahui bed telah dikuasai dan oleh siapa.
- Denah serta eligibility dimuat ulang dari server.
- Pilihan yang usang dibersihkan.
- Tidak ada PHI yang disebar lintas unit dan tidak ada transaksi yang bergantung pada delivery event.
