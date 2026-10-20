# Frontend Roadmap — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Roadmap ID | `RJ-PRI-RM-FE-001` |
| Revision | `1` |
| Status | `approved` — Sukma Giri, 2026-10-16 (`RJ-PRI-DEC-031`). Wewenang implementasi `GRANTED` untuk seluruh task |
| Blueprint | `rawat-jalan` revisi `33`, `approved` (Sukma Giri, 2026-10-16, `RJ-PRI-DEC-030`) |
| Cakupan | `MVP-0` s.d. `MVP-2`, sisi frontend |
| SHA baseline | FE `1fd858322` (`sukmagpV2`), BE `551e1cbb` (`sukmagp`) |
| Kontrak masukan | `RJ-PRI-RESV-001@1.0.0` `approved`, `api-contract.md` `7932aaf7dc6c`. Kontrak sudah approved dan terkunci hash, sehingga frontend boleh dikerjakan paralel dengan API tiruan. Acceptance terhadap backend lokal tetap menunggu task `[BE]` |
| Masukan desain | `03-frontend-architecture.md` RP-FE.1–7 (`3274f27fa597`) |
| Owner | Sukma Giri (Frontend authority) |

> **Batas dokumen.**
> 1. Bukan izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan approval task eksplisit.
> 2. Tata letak, bentuk dialog/drawer, posisi menu, warna, ikon, bunyi, dan kalimat persis pesan WA adalah `DEV_DISCRETION` (`RJ-PRI-FE-002`, `003`, `004`). Isi halaman publik dan isi pesan WA mengikat (`RJ-PRI-FE-001`, `002`).
> 3. Uji unit: `node --import ./tests/helpers/register.mjs --test tests/unit/`. Uji layar lewat dev `3000` dengan route `7184 → 7185`.

## Legenda

| Tanda | Arti |
|:---:|---|
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-RJP-004 ✅ [BE] ─> FE-RJP-001 ✅

FE-RJP-002 ✅

BE-RJP-006 ✅ [BE] ─> FE-RJP-003 ✅ ─> FE-RJP-004 ✅ ─┬────────────────────┐
                                                      │                    │
BE-RJP-007 ✅ [BE] ─┐                                 │                    │
                    │                                 │                    │
BE-RJP-010 ✅ [BE] ─┴─────────────────────────────────┴─> FE-RJP-005 ✅    │
                                                                           │
BE-RJP-011 ✅ [BE] ─┬─> FE-RJP-007 ✅                                      │
                    │                                                      │
BE-RJP-008 ✅ [BE] ─┴──────────────────────────────────────────────────────┴─> FE-RJP-006 ✅

BE-RJP-012 ✅ [BE] ─> FE-RJP-008 ✅
```

`[BE]` = task backend pada `priority-reservation-backend-roadmap.md`, cermin baca-saja.

Pasangan prasyarat → task: 11 (`BE-004→FE-001`, `BE-006→FE-003`, `FE-003→FE-004`, `FE-004→FE-005`, `BE-007→FE-005`,
`BE-010→FE-005`, `FE-004→FE-006`, `BE-011→FE-006`, `BE-008→FE-006`, `BE-011→FE-007`, `BE-012→FE-008`), sama dengan
isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — (acceptance menunggu `[BE]` masing-masing) | `FE-RJP-001` (`BE-RJP-004`), `FE-RJP-002` (tanpa prasyarat), `FE-RJP-003` (`BE-RJP-006`), `FE-RJP-007` (`BE-RJP-011`), `FE-RJP-008` (`BE-RJP-012`) |
| 2 | `FE-RJP-003` | `FE-RJP-004` |
| 3 | `FE-RJP-004` (+ `[BE]`) | `FE-RJP-005` (`BE-RJP-007`, `BE-RJP-010`), `FE-RJP-006` (`BE-RJP-008`, `BE-RJP-011`) |

| Rilis | Task | Catatan |
|---|---|---|
| `MVP-0` | `FE-RJP-001`, `FE-RJP-002` | `FE-RJP-001` **wajib** dirilis bersama `BE-RJP-004` |
| `MVP-1` | `FE-RJP-003` s.d. `007` | — |
| `MVP-2` | `FE-RJP-008` | Go-live menunggu `RJ-PRI-OQ-003` |

## Ringkasan Task

| Task ID | Outcome | Layar / FR | Dependency | Rilis | Status |
|---|---|---|---|---|---|
| `FE-RJP-001` | Layar display memakai group realtime display dan mengabaikan event prioritas | `FE-RP-05` / `FR-RP-013` | `BE-RJP-004` [BE] | `MVP-0` | ✅ 2026-10-16 — lint/build PASS, unit 4/4 PASS, uji layar 12/12 PASS (UAT-RP-03); rilis bersama BE-RJP-004 — [laporan](../task/report/frontend/FE-RJP-001.md) |
| `FE-RJP-002` | Label member di Pendaftaran Pasien | `FE-RP-04` / `FR-RP-070` | — | `MVP-0` | ✅ 2026-10-16 — lint/build PASS, unit 5/5 PASS, uji layar 4/4 PASS (R-RP-28, UAT-RP-18/19); deviasi: label hanya di panel identitas, belum di kartu hasil pencarian — [laporan](../task/report/frontend/FE-RJP-002.md) |
| `FE-RJP-003` | Lapisan data reservasi dan klien realtime board | `FE-RP-01` (data) | `BE-RJP-006` [BE] | `MVP-1` | ✅ 2026-10-19 — lint/build PASS, unit task 7/7 PASS (thunk tiap endpoint, 409 VAL-RP-14, link tidak disimpan di Redux), unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-003.md) |
| `FE-RJP-004` | Daftar Reservasi Prioritas, butir menu, buat reservasi, dialog kirim WA | `FE-RP-01` / `FR-RP-030`, `062` | `FE-RJP-003` | `MVP-1` | ✅ 2026-10-19 — lint/build PASS, uji layar 15/15 PASS (UAT-RP-07/08/09, butir menu per izin Read, empat keadaan, link hilang sesudah dialog ditutup). Rework 2026-10-19 (RJ-PRI-DEC-032, kontrak 1.1.0): tombol Buat di kartu saringan, Tanggal Awal/Akhir/Periode atas tanggal kunjungan, kolom audit, tanpa kolom Aksi — uji layar 12/12 + regresi 15/15 PASS, unit task 8/8 PASS, unit penuh 2977/3004 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-004.md) |
| `FE-RJP-005` | Detail dan aksi reservasi, serta realtime berbunyi | `FE-RP-01` / `FR-RP-033` s.d. `035`, `038`, `052` | `FE-RJP-004`, `BE-RJP-007` [BE], `BE-RJP-010` [BE] | `MVP-1` | ✅ 2026-10-19 — lint/build PASS, uji layar 11/11 PASS dua akun (R-RP-14 akun Read+Screen, 409 toast + muat ulang, tombol sesuai can* dan izin, cadangan muat ulang 60 detik). Rework 2026-10-19 (RJ-PRI-DEC-032): Tandai Tiba primer, Lengkapi hanya dari detail — regresi admin 7/7 PASS, unit penuh 2977/3004 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-005.md) |
| `FE-RJP-006` | Pendaftaran Pasien mode reservasi dan layar selesai | `FE-RP-03`, `FE-RP-06` / `FR-RP-040`, `061` | `FE-RJP-004`, `BE-RJP-008` [BE], `BE-RJP-011` [BE] | `MVP-1` | ✅ 2026-10-19 — lint/build PASS, uji layar 7/7 PASS (UAT-RP-11, UAT-RP-12, pasien dan tanggal terkunci, regresi pendaftaran biasa), unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-006.md) |
| `FE-RJP-007` | Tiket kiosk dan kartu antrean menampilkan ruang tunggu prioritas | `FE-RP-06`, `FE-RP-07` / `FR-RP-060`, `061` | `BE-RJP-011` [BE] | `MVP-1` | ✅ 2026-10-19 — lint/build PASS, uji layar 4/4 PASS (R-RP-21 tiket kiosk, UAT-RP-16, UAT-RP-17, R-RP-29 Klinis Dokter); R-RP-21 tanpa reservasi dibuktikan di BE-RJP-011, unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-007.md) |
| `FE-RJP-008` | Halaman publik Konfirmasi Kedatangan | `FE-RP-02` / `FR-RP-051` | `BE-RJP-012` [BE] | `MVP-2` | ✅ 2026-10-19 — lint/build PASS, uji layar 16/16 PASS tanpa cookie lebar 360 px (R-RP-11 s.d. 13, UAT-RP-14, UAT-RP-15, 429, tanpa panggilan auth); DoD go-live dikecualikan: menunggu RJ-PRI-OQ-003, unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-008.md) |

## Rincian Task

### `FE-RJP-001` ✅ — Layar display

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — lint/build PASS, unit 4/4 PASS, uji layar 12/12 PASS (UAT-RP-03); rilis bersama BE-RJP-004 — [laporan](../task/report/frontend/FE-RJP-001.md) |
| Outcome | `queue-realtime-client` mendapat `syncQueueDisplayClusters`. `use-queue-display-screen` memakainya sebagai pengganti `syncNurseStationClusters`, menangani event `QueueDisplayChanged`, dan membuang kandidat `isPriorityQueue = true` sebagai lapis kedua |
| Requirement/decision | `FR-RP-013`; `RJ-PRI-DEC-005`, `025` |
| Kontrak | `api-contract.md` hub `/hubs/queues` |
| Reuse | `lib/realtime/queue-realtime-client.js`, `lib/hooks/queue-display/use-queue-display-screen.jsx`, `utils/queue-display/queue-display-call-coordinator-utils.jsx` |
| Cakupan | Klien realtime dan hook layar display |
| Dependency | `BE-RJP-004` [BE] |
| Acceptance criteria | `UAT-RP-03` lewat layar: nomor prioritas yang dipanggil tidak muncul sesaat pun; nomor Reguler muncul dan bersuara |
| Verifikasi | Uji unit utilitas penyaring + uji layar Playwright akun display |
| Risiko/pemilik | **Rilis bersama `BE-RJP-004`.** Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `FE-RJP-002` ✅ — Label member di Pendaftaran Pasien

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-16 — lint/build PASS, unit 5/5 PASS, uji layar 4/4 PASS (R-RP-28, UAT-RP-18/19); deviasi: label hanya di panel identitas, belum di kartu hasil pencarian — [laporan](../task/report/frontend/FE-RJP-002.md) |
| Outcome | Hasil pencarian dan panel identitas Pendaftaran Pasien menampilkan tingkat member Aktif dari `isMember` dan `defaultMembershipTierName`. Tampilan saja |
| Requirement/decision | `FR-RP-070`; PDF 2C; `RJ-PRI-FE-004` (`DEV_DISCRETION` bentuk) |
| Kontrak | Respons pencarian pasien yang sudah ada (`CAP-RP-21`) |
| Reuse | `outpatient-identity-panel.jsx`, pencarian `searchOutpatientPatients` |
| Cakupan | Komponen Pendaftaran Pasien |
| Dependency | — |
| Acceptance criteria | `R-RP-28`, `UAT-RP-18`, `UAT-RP-19` |
| Verifikasi | Uji unit pemetaan label + uji layar |
| Risiko/pemilik | Proyeksi bisa tertinggal antara 00.00 dan 00.05 WIB. Dapat diterima untuk tampilan. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `FE-RJP-003` ✅ — Lapisan data reservasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, unit task 7/7 PASS (thunk tiap endpoint, 409 VAL-RP-14, link tidak disimpan di Redux), unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-003.md) |
| Outcome | Service dan slice/thunk untuk seluruh endpoint Outpatient Priority Reservation, serta `joinPriorityReservationBoard` dan handler `PriorityReservationChanged` di klien realtime |
| Requirement/decision | `FR-RP-030` s.d. `038` (data) |
| Kontrak | `api-contract.md` Outpatient Priority Reservation, hub |
| Reuse | `InstanceAxios`, pola slice Redux yang ada, `queue-realtime-client` |
| Cakupan | `lib/services/...`, `lib/state/slice/...`, konstanta endpoint |
| Dependency | `BE-RJP-006` [BE] |
| Acceptance criteria | Uji unit thunk untuk setiap endpoint, termasuk penanganan 409 `VAL-RP-14` |
| Verifikasi | Uji unit |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-RJP-004` ✅ — Daftar Reservasi Prioritas

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, uji layar 15/15 PASS (UAT-RP-07/08/09, butir menu per izin Read, empat keadaan, link hilang sesudah dialog ditutup). Rework 2026-10-19 (RJ-PRI-DEC-032, kontrak 1.1.0): tombol Buat di kartu saringan, Tanggal Awal/Akhir/Periode atas tanggal kunjungan, kolom audit, tanpa kolom Aksi — uji layar 12/12 + regresi 15/15 PASS, unit task 8/8 PASS, unit penuh 2977/3004 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-004.md) |
| Outcome | Butir menu "Reservasi Prioritas" di bawah Rawat Jalan (`OutpatientPriorityReservation : Read`), route `/health-services/registration-management/priority-reservations`, daftar dengan saringan, ringkasan status, dan dialog buat reservasi (cari pasien, kelayakan, tanggal). Setelah simpan: link, teks pesan, tombol "Kirim via WhatsApp" (`wa.me/62…?text=…`), dan tombol salin |
| Requirement/decision | `FR-RP-030`, `062`; `RJ-PRI-DEC-008`, `011`, `021`; `RJ-PRI-FE-002`, `003` |
| Kontrak | `api-contract.md` `GET /`, `/summary`, `/eligibility`, `POST /` |
| Reuse | Base component (hero, data-filter, data-table, status-badge), `usePermission`, pola `wa.me` di `use-dokumen-kasir.js` |
| Cakupan | Page, client, view, `menu-items.jsx` |
| Dependency | `FE-RJP-003` |
| Acceptance criteria | Butir menu tampil hanya untuk pemegang `Read`. `UAT-RP-07` s.d. `09` lewat layar. Keempat keadaan (memuat, kosong, gagal, berisi) sesuai skema. Link hilang dari state saat dialog ditutup |
| Verifikasi | Uji unit penormalan No. HP ke format 62 + uji layar Playwright |
| Risiko/pemilik | Salah normalisasi No. HP mengirim ke nomor yang salah. Petugas tetap melihat tujuan di WA. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `FE-RJP-005` ✅ — Detail dan aksi reservasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, uji layar 11/11 PASS dua akun (R-RP-14 akun Read+Screen, 409 toast + muat ulang, tombol sesuai can* dan izin, cadangan muat ulang 60 detik). Rework 2026-10-19 (RJ-PRI-DEC-032): Tandai Tiba primer, Lengkapi hanya dari detail — regresi admin 7/7 PASS, unit penuh 2977/3004 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-005.md) |
| Outcome | Detail berisi kepala, checklist skrining, dan riwayat. Tombol sesuai `can*` dan izin: Tandai tiba, Kembalikan (alasan), Skrining (tiga isian), Kirim ulang, Batalkan (alasan). Bergabung ke board realtime; baris berubah dan berbunyi saat pasien tiba; muat ulang berkala 60 detik bila koneksi putus |
| Requirement/decision | `FR-RP-033` s.d. `035`, `038`, `052`; `RJ-PRI-DEC-012`, `014`, `019`, `022`, `023`, `026`, `028`, `029` |
| Kontrak | `api-contract.md` perintah petugas, hub |
| Reuse | `confirm-modal`, `row-action-menu`, `toast-stack` |
| Cakupan | Komponen detail dan dialog aksi |
| Dependency | `FE-RJP-004`, `BE-RJP-007` [BE], `BE-RJP-010` [BE] |
| Acceptance criteria | `R-RP-14` lewat layar dengan akun petugas skrining. 409 menampilkan "baru saja diubah petugas lain" lalu memuat ulang detail. Tombol tanpa hak tidak tampil |
| Verifikasi | Uji layar dua akun (admisi, skrining) |
| Risiko/pemilik | Bunyi pemberitahuan butuh interaksi pengguna dulu di browser. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `FE-RJP-006` ✅ — Pendaftaran Pasien mode reservasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, uji layar 7/7 PASS (UAT-RP-11, UAT-RP-12, pasien dan tanggal terkunci, regresi pendaftaran biasa), unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-006.md) |
| Outcome | Tombol "Lengkapi" (butuh `Complete` dan `PatientEncounter : Create`) membuka wizard Pendaftaran Pasien dengan `?priorityReservationId=`. Pasien dan tanggal terkunci. Simpan mengirim `priorityReservationId`. Layar selesai mengarahkan ke ruang tunggu prioritas |
| Requirement/decision | `FR-RP-040`, `061`; PDF 2B; `RJ-PRI-DEC-030` usulan (1) |
| Kontrak | `api-contract.md` Patient Encounter (diperbarui) |
| Reuse | Wizard `outpatient-registration`, `outpatient-completed-step.jsx` |
| Cakupan | Hook dan step wizard, layar selesai |
| Dependency | `FE-RJP-004`, `BE-RJP-008` [BE], `BE-RJP-011` [BE] |
| Acceptance criteria | `UAT-RP-11`, `UAT-RP-12` lewat layar. Membuka mode reservasi untuk reservasi yang bukan Sudah Tiba menampilkan pesan dan tombol kembali. Pendaftaran biasa tetap sama |
| Verifikasi | Uji layar + regresi pendaftaran biasa |
| Risiko/pemilik | Menyentuh wizard yang dipakai setiap hari. Pemilik: Sukma Giri |
| DoD | Kriteria terbukti tanpa regresi |

### `FE-RJP-007` ✅ — Tiket kiosk dan kartu antrean

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, uji layar 4/4 PASS (R-RP-21 tiket kiosk, UAT-RP-16, UAT-RP-17, R-RP-29 Klinis Dokter); R-RP-21 tanpa reservasi dibuktikan di BE-RJP-011, unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-007.md) |
| Outcome | Tiket kiosk menampilkan pemberitahuan ruang tunggu prioritas (pola `KioskReferralTicketNotice`). Kartu antrean Nurse Station dan Klinis Dokter menampilkan `priorityWaitingLocationName` di samping lencana "Prioritas" |
| Requirement/decision | `FR-RP-060`, `061`; `RJ-PRI-DEC-017`, `018` |
| Kontrak | `api-contract.md` Nurse Station Queue, Doctor Queue, Patient Encounter |
| Reuse | `kiosk-old-patient-step-ticket.jsx`, `QueuePatientCard.jsx` (nurse dan doctor) |
| Cakupan | Tiga komponen |
| Dependency | `BE-RJP-011` [BE] |
| Acceptance criteria | `R-RP-21` (tiket), `R-RP-29`, `UAT-RP-16`, `UAT-RP-17` |
| Verifikasi | Uji layar kiosk dan Nurse Station |
| Risiko/pemilik | Menyentuh layar kiosk (pemberitahuan ke owner kiosk). Pemilik: Sukma Giri |
| DoD | Kriteria terbukti |

### `FE-RJP-008` ✅ — Halaman Konfirmasi Kedatangan

| Field | Isi |
|---|---|
| Status | ✅ 2026-10-19 — lint/build PASS, uji layar 16/16 PASS tanpa cookie lebar 360 px (R-RP-11 s.d. 13, UAT-RP-14, UAT-RP-15, 429, tanpa panggilan auth); DoD go-live dikecualikan: menunggu RJ-PRI-OQ-003, unit penuh 2976/3003 (27 gagal di modul lain, sudah gagal di HEAD) — [laporan](../task/report/frontend/FE-RJP-008.md) |
| Outcome | Route publik `/priority-arrival/[code]` terdaftar di `PUBLIC_PATH_PREFIXES` pada `src/proxy.js` **dan** `src/components/ui/auth-wrapper.jsx`. Panggilan memakai `_skipAuthRefresh: true`. Keadaan `NOT_YET_OPEN`, `READY`, `CONFIRMED`, 404, dan 429 sesuai skema. Ramah HP 360 px |
| Requirement/decision | `FR-RP-051`; `RJ-PRI-DEC-012`; `RJ-PRI-FE-001`; `NFR-RP-008` |
| Kontrak | `api-contract.md` Priority Reservation Arrival |
| Reuse | Pola route publik kiosk/queue-display, `InstanceAxios` |
| Cakupan | Page publik, dua daftar prefix publik |
| Dependency | `BE-RJP-012` [BE] |
| Acceptance criteria | `R-RP-11` s.d. `13` lewat layar tanpa login. Tidak ada sidebar, menu, atau panggilan API yang butuh login. `UAT-RP-14`, `UAT-RP-15` |
| Verifikasi | Uji layar Playwright di jendela tanpa cookie, lebar 360 px |
| Risiko/pemilik | **Go-live menunggu `RJ-PRI-OQ-003`.** Pemilik: Sukma Giri |
| DoD | Kriteria terbukti. Go-live tercatat terpisah |
