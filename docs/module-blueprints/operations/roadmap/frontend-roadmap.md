# Frontend Roadmap — Modul Operasi

Semua task frontend memakai `opr-api-v1` dan tidak boleh mengubah kontrak backend. Layout detail adalah `DEV_DISCRETION` mengikuti design system existing. Amandemen 23 Oktober 2026: `OPS-DEC-038` s.d. `OPS-DEC-042`.

## Grafik Urutan Dependency

```text
FE-OPR-001 ✅ ─┬─> FE-OPR-002 ✅
               ├─> FE-OPR-003 ✅
               ├─> FE-OPR-004 ✅ ─┬─> FE-OPR-005 ✅ ─┬─> FE-OPR-009 ✅
               │                  │                  │
               │                  │                  ├─> FE-OPR-006 ✅
               │                  │                  │
               │                  │                  ├─> FE-OPR-007 ✅
               │                  │                  │
               │                  │                  └─> FE-OPR-008 ✅
               │                  │
               │                  └─> [BE: BE-OPR-014] ✅ ───> FE-OPR-013 ✅
               │
               └─> FE-OPR-010 ⏳

[BE: BE-OPR-012] ✅ ─────────────────────────────────────────> FE-OPR-012 ✅
```

### Tabel Gelombang Eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | Kontrak backend `BE-OPR-012` siap | `FE-OPR-012` (Visualisasi Kalender Grid Timeline Ruang OK Lantai 2) |
| 1 | `FE-OPR-004` ✅ & Kontrak `BE-OPR-014` siap | `FE-OPR-013` (Form Pencatatan Kelahiran Bayi Intraoperatif SC) |

> **Status dimutakhirkan 15 Oktober 2026.** Kolom `Dependency` lama menandai seluruh task
> `BLOCKED BY BE-OPR-00x`; kesebelas task backend itu kini **Selesai**, sehingga penandaan itu
> tidak lagi berlaku dan diganti kolom Status beserta bukti acceptance-nya. Kriteria acceptance
> aslinya tetap utuh di `testing/fe-acceptance-evidence.md`.

| Task ID | Outcome | Trace | Status | Bukti acceptance |
|---|---|---|---|---|
| `FE-OPR-001` | Shell Modul Operasi dan daftar/detail kasus | `OPS-REQ-001/002` | **SELESAI** | `operating-room-screen-acceptance` AC-1..4; `operating-room-reject-case` (4 AC); `operating-room-hardening` (10) |
| `FE-OPR-002` | Workspace jadwal ruang dan tim | `OPS-REQ-003/004` | **SELESAI** | `operating-room-screen-acceptance`: `OPR003`, Riwayat Jadwal, `disabled={submitting}` |
| `FE-OPR-003` | Workspace persiapan/checklist | `OPS-REQ-005` | **SELESAI** | `operating-room-screen-acceptance`; `ward-pre-op` (3 AC) |
| `FE-OPR-004` | Workspace pelaksanaan dan addendum | `OPS-REQ-006/009` | **SELESAI** | `operating-room-execution-addendum` (7 AC) |
| `FE-OPR-005` | Workspace anestesi, recovery, handover | `OPS-REQ-006/008` | **SELESAI** | `operating-room-screen-acceptance`; `post-op-summary` (5 AC) |
| `FE-OPR-006` | Pencatatan material/implant | `OPS-REQ-007` | **SELESAI** | `operating-room-screen-acceptance`: batch/serial wajib untuk implan |
| `FE-OPR-007` | Monitoring integrasi | `OPS-REQ-007/010` | **SELESAI** | `operating-room-screen-acceptance`: payload tidak ditampilkan |
| `FE-OPR-008` | Laporan Operasi | `OPS-REQ-011` | **SELESAI** | `operating-room-screen-acceptance`: filter, paging, kosong, galat |
| `FE-OPR-009` | E2E dan hardening frontend | Semua | **SELESAI** | hardening 10 lulus; **E2E Playwright 11/11 lulus** 15 Okt — `testing/fe-acceptance-evidence.md` |
| `FE-OPR-010` | Borang permintaan operasi baru | `OPS-REQ-001` | **SEDANG PROSES** | 4 AC lulus: pasien/kunjungan/dokter, urutan "pilih kunjungan dulu", indikasi klinis, kegagalan simpan. **AC-5 belum terpenuhi: nol penjaga hak akses** |
| `FE-OPR-011` | Pemetaan depo sumber stok kamar operasi | `PHA-DEPOT-ROUTING-v1` | **SELESAI** | 4 AC: kamar operasi tidak menyimpan saldo sendiri, hasil simpan vs gagal dibedakan, kegagalan muat, gerbang hak akses |
| `FE-OPR-012` | Visualisasi Kalender Grid Timeline Ruang OK Lantai 2 & Booking Slot | `OPS-DEC-038`, `OPS-DEC-039` | **RENCANA** | Timeline grid ruangan OK 1, OK 2, OK 3; blok buffer sterilisasi 45 menit abu-abu terkunci; modal booking saat klik area kosong |
| `FE-OPR-013` | Form Pencatatan Kelahiran Bayi Intraoperatif SC | `OPS-DEC-042` | **RENCANA** | Modal input kelahiran bayi (Jam, BB, PB, Kelamin, APGAR 1/5/10) aktif saat status SC In Progress; banner konfirmasi MRN bayi |

Dua task terakhir ditambahkan 16 Oktober 2026. Register ini sebelumnya mencatat sembilan task
frontend, padahal modul memiliki **sebelas area layar**; borang permintaan operasi dan pemetaan
depo sumber stok tidak pernah ber-ID dan nol uji menyentuhnya.

Uji: `tests/unit/operating-room-remaining-screens.test.mjs` — **10 uji, 10 lulus**.

> **`FE-OPR-010` belum selesai, dan itu temuan nyata.** Borang permintaan operasi tidak memiliki
> penjaga hak akses sama sekali — nol `AccessDeniedGate` pada view, pada
> `operating-room-case-form-client.jsx`, maupun pada `page.jsx` route `cases/create`. Sembilan
> layar Operasi lainnya memilikinya.
>
> Source **tidak diubah** untuk menutupnya: menambahkan penjaga adalah perubahan perilaku
> otorisasi, bukan pekerjaan sambilan saat menulis uji. Satu uji mencatat keadaan ini apa adanya
> dan akan **gagal begitu penjaga ditambahkan**, yang menjadi penanda untuk menaikkan status
> task ini.

Setiap task frontend memerlukan `TASK MODE: FRONTEND` dan backend dibaca sebagai source of truth.
