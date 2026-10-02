# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-011` |
| Judul | Pembatalan kunjungan dari layar Daftar Pasien Rawat Jalan |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `11.2` |
| Trace | `RJ-DOC-FE-008`; `RJ-DOC-DEC-015`, `016`, `018`, `020`, `021`; desain `03-frontend-architecture.md` DP-FE.3, DP-FE.5 |
| Kontrak | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — `PATCH /outpatient-encounters/{id}/cancel` |
| Dependency | `RJ-DOC-REV-FE-010` ✅, `[BE] RJ-DOC-REV-BE-010` ✅ |
| Wewenang | `RJ-DOC-DEC-025` |
| Repository / branch | `V2QuilvianSystemFrontendDev` @ `sukmagpV2` (lanjutan `FE-010`, belum di-commit) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Keputusan base component

| Elemen | Base component | Status | Bukti |
| --- | --- | --- | --- |
| Aksi baris "Batalkan Kunjungan" | `RowActionMenu` (pola kolom Aksi `lab-monitoring`) | `REUSE` | `lab-monitoring-table-columns.jsx:258-305` |
| Modal konfirmasi + alasan wajib, batas 250 | `ConfirmModal` (`requireReason`, `maxReasonLength`, `loading`) | `REUSE` | `confirm-modal.jsx:58-132` |
| Ringkasan kunjungan di modal | Kelas bawaan `region-confirm-message` | `REUSE` | `confirm-modal.jsx:181-184` |
| Galat server di modal | `InformationAlert` (`danger`) sebagai `children` | `REUSE` | — |
| Notifikasi berhasil | `ToastStack` | `REUSE` | — |
| Keterangan konsultasi aktif | `StatusBadge` + kelas `supportingCell` di dalam `regionNameCell` | `REUSE` | — |

`UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

**Temuan perilaku base component:** pada `ConfirmModal`, `children` **menggantikan** `message`
(`children ?? message`). Karena galat server dikirim lewat `children`, kalimat ringkasan ditulis di
dalam `children` memakai kelas `region-confirm-message`. Base component tidak diubah.

## 2. Berkas

| Berkas | Status |
| --- | --- |
| `src/lib/hooks/health-services/registration-management/outpatient-encounters/use-outpatient-encounter-cancel.jsx` | Baru |
| `src/lib/services/health-services/registration-management/outpatient-encounter.service.js` | Diperbarui — `cancelOutpatientEncounter` |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-table-columns.jsx` | Diperbarui — kolom Aksi bersyarat, keterangan konsultasi aktif |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-list-view.jsx` | Diperbarui — `ConfirmModal`, `ToastStack` |

**Perilaku utama:**
- **Kolom Aksi** hanya ada bila `usePermission("OutpatientEncounter", "Cancel")` mengizinkan.
- **Aksi Batalkan** hanya muncul pada baris dengan `canCancel` dari server.
- **Konsultasi aktif:** baris yang punya konsultasi aktif menampilkan `cancelBlockedReason` di bawah status. Baris itu tidak mendapat tombol.
- **Kirim ganda:** dicegah lewat `submittingRef`, dan tombol konfirmasi masuk keadaan `loading`.
- **Berhasil:** modal ditutup, toast tampil, lalu tabel dan ringkasan dimuat ulang.
- **Ditolak `400`/`404`:** pesan server tampil di modal yang tetap terbuka, dan tabel dimuat ulang saat modal ditutup.

## 3. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| ESLint pada seluruh berkas fitur + `menu-items.jsx` | `0 error, 0 warning` |
| `npm run build` (final, `.env` asli) | `PASS` — `✓ Compiled successfully in 47s` |
| Grep anti-regresi | Tidak ada `<button>`/`btn-*`, `<table>`, `fw-*`/`fs-*`; tidak ada berkas style baru |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — perilaku diverifikasi lewat uji layar |

### Verifikasi layar (Playwright headless, `ui_fe011.log`)

Lingkungannya sama dengan `FE-010`: backend Release di 7185 dan frontend `next start` di 3001 dengan override API. Data uji memakai pasien `KSKTEST-RM-07`. Kunjungan nyata ENC-RSMMC-00146 sengaja **tidak** dibatalkan.

| ID | Skenario | Hasil |
| --- | --- | --- |
| Y1 | Akun pendaftaran (memegang `Cancel`): kolom Aksi tampil | PASS |
| Y2 | Modal memuat nomor kunjungan, pasien, klinik, status, dan isian "Alasan Pembatalan" | PASS (run kedua; run pertama FAIL — lihat bawah) |
| Y3 | Tombol konfirmasi nonaktif sebelum alasan diisi | PASS |
| Y4 | Alasan 260 karakter terpotong menjadi 250 | PASS |
| Y5 | **AT-DP-24** — batal `ENC-RSMMC-00193` lewat layar: `PATCH 200`, toast "…berhasil dibatalkan…", modal tertutup | PASS |
| Y6 | Tabel dimuat ulang; kunjungan hilang dari tampilan Aktif | PASS |
| Y7 | **AT-DP-24** — pasien yang sama didaftarkan ulang | PASS (`200`) |
| Y8 | Data basi: kunjungan dibatalkan pihak lain saat modal terbuka → "Kunjungan sudah dibatalkan." tampil di modal yang tetap terbuka | PASS |
| Y9 | Tutup modal → tabel dimuat ulang | PASS |
| Y10 | **AT-DP-22** — `ENC-RSMMC-00117` (status 6, konsultasi aktif): keterangan "Konsultasi masih aktif…" tampil, tanpa tombol aksi | PASS |
| Y11 | **AT-DP-22** — dokter uji (tanpa `Cancel`): tanpa kolom Aksi maupun tombol | PASS |

Ringkasan: `12/12 PASS`.
- Run pertama `11/12`: Y2 gagal karena kalimat ringkasan tidak tampil (temuan `children ?? message` di atas). Cacat ini diperbaiki dan dibuktikan pada run kedua.
- Sesudah uji, pasien uji tidak punya kunjungan aktif. `ENC-RSMMC-00117` dan `00146` tetap `IsCancel = false`.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-DP-22` tombol bersyarat; keterangan `cancelBlockedReason` | Terbukti | Y1, Y10, Y11 |
| `AT-DP-24` kasus pemicu (padanan pada data uji): batal lalu daftar ulang | Terbukti | Y5–Y7 |
| DP-FE.5 kirim ganda, data basi | Terbukti | Y3, Y8, Y9; `submittingRef` |

## 5. Risiko dan catatan

1. ENC-RSMMC-00146 masih aktif. Penutupannya keputusan pemilik lewat layar ini (`RJ-DOC-OQ-011`).
2. Server dev pemilik di port 7184/3000 perlu di-restart atau di-build ulang untuk memuat perubahan backend dan frontend.
3. Build produksi `.next` sudah dikembalikan ke `.env` asli. Override port uji yang dicatat sebagai risiko di laporan `FE-010` tidak lagi berlaku.

## 6. Penutup Amendment DP

Kelima task (`BE-008`..`010`, `FE-010`..`011`) selesai.

Atas keputusan pemilik (2 Okt 2026):
- Akun uji `UJI-RJDP` sudah dinonaktifkan.
- Hak jabatan sudah dipulihkan ke daftar asli. Rinciannya ada di laporan `BE-009` §4.
- Hak `OutpatientEncounter` untuk jabatan nyata diberikan admin lewat layar Akses Role saat fitur dipakai.
