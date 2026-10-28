# PRD → MVP — Nurse View

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian SIMRS |
| Modul | Nurse View — display pemantauan perawat per lantai |
| Status | `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan |
| Repository target | `NewQuilvianSystemBackend` (`rizkiG`), `QuilvianSystemFrontendDev` (`RizkiV2`) |
| Commit baseline | BE `a6f977a3`, FE `76b0c6e4b` |
| Contract version | `0.1.0` |
| Masukan | `02-backend-architecture.md` rev `1`, `contracts/*` `0.1.0`, `00-interview-decisions.md` rev `2`, `01-existing-capability-map.md` rev `1`, `02-requirement-completeness-assessment.md` rev `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| Ringkasan cakupan | TV pasif per lantai yang menampilkan seluruh bed beserta pasien samaran, perawat, TTV/EWS, dan ikon keselamatan; ditambah layar admin perangkat dan interval |

## 2. Ringkasan eksekutif

Perawat jaga perlu tahu, sekilas dari nurse station, pasien mana yang sedang memburuk, mana
yang TTV-nya terlewat, dan bed mana yang siap menerima pasien. Hari ini informasi itu tersebar
di beberapa layar dan harus dibuka satu per satu. Nurse View menaruhnya di satu TV per lantai,
diperbarui sendiri, tanpa membuka identitas pasien kepada pengunjung.

## 3. Masalah produk

| Sudah ada (bukti) | Belum ada |
|---|---|
| Status bed enam kode — `InpBedManagementQueryService.ProjectBatchAsync` | Papan satu lantai yang menyatukan semuanya |
| TTV + EWS dihitung saat simpan — `PatientVitalSignController` | Tanda "TTV lewat jadwal" (`NV-CAP-08`) |
| Perawat PJ — `InpNurseAssignment` | Akun perangkat untuk TV Nurse View (`NV-CAP-12`, `13`) |
| Risiko jatuh, alergi, isolasi — Clinical & Rawat Inap | Identitas samaran yang dijaga backend (`NV-CAP-03`) |
| Pola TV pasif — `queue-display` | Sinyal realtime saat TTV disimpan (`NV-CAP-14`) |

## 4. Visi produk

1. Perawat menyimpan TTV di form yang sudah ada.
2. EWS dihitung oleh Clinical seperti biasa.
3. TV lantai pasien menerima sinyal dan memuat ulang kartunya.
4. Kartu berubah warna bila kritis dan naik ke atas.
5. Perawat di nurse station melihatnya dan mendatangi pasien.
6. Perubahan bed (masuk, pindah, pulang, dibersihkan) tertangkap paling lambat satu menit.

## 5. Batas MVP

**Titik mulai:**
1. Registry `Nvw` disetujui pemilik modul (`NV-DEC-029`) dan berkas bersama direstui lead dengan syarat (`NV-DEC-030`).
2. Admin IT mendaftarkan satu TV untuk satu lantai.

**Titik akhir:**
1. TV menampilkan papan lantai lengkap sesuai `NV-DEC-007`, `015`, `018`–`020`.
2. Papan memperbarui diri ≤ 60 detik dan menampilkan pita data basi saat putus.
3. Admin dapat membuat login, mengatur ulang, dan menonaktifkan perangkat.
4. Admin dapat memasukkan interval yang disahkan Komite; tanpa interval, papan aman-gagal.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
|---|---|
| Admin IT | Mengelola perangkat dan memasukkan interval |
| Perawat, kepala ruang | Melihat papan |
| Komite Keperawatan | Mengesahkan angka interval di luar sistem |
| Akun perangkat | Membaca papan lantainya |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Bed per lantai dikelompokkan per bangsal | `NV-CAP-01` | Wajib; tanpa ini tidak ada papan |
| Status bed | `NV-CAP-02` | Wajib |
| Penghuni samaran | `NV-CAP-03` | Wajib; tanpa samaran papan tidak boleh dipasang di ruang publik |
| Perawat PJ | `NV-CAP-04` | Wajib |
| TTV/EWS terakhir | `NV-CAP-06` | Wajib; inti pemantauan |
| Peringatan kritis visual | `NV-CAP-07` | Wajib |
| Interval ukur ulang (struktur + aman-gagal) | `NV-CAP-08` | Wajib strukturnya; angkanya menyusul |
| Ikon risiko jatuh, alergi, isolasi | `NV-CAP-09`, `10`, `11` | Wajib |
| Master perangkat + login | `NV-CAP-12`, `13` | Wajib; tanpa ini TV tidak bisa masuk |
| Halaman TV, data basi | `NV-CAP-15` | Wajib |
| Header perawat jaga dari roster | `NV-CAP-05` | Wajib dengan aman-gagal |

## 8. Kemampuan yang ditunda

| Kemampuan | ID asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Push TTV seketika | `NV-CAP-14` | Gelombang terakhir (`MVP-4`) karena menyentuh kode keperawatan; disetujui Hamzah 2026-10-28 | Muat ulang penuh ≤ 60 detik sampai `MVP-4` selesai |
| Angka interval sah | `NV-CAP-08` | Kebijakan klinis Komite (`NV-OQ-010`) | Kartu menampilkan umur pengukuran; perawat menilai sendiri |
| Laporan kamar tanpa `FloorCode` | `NV-CAP-16` | Cukup langkah operasional | Daftar periksa go-live `G-06` |
| Tablet pasien, panggilan pasien, pesan makan, jadwal visite | — | Di luar scope `NV-DEC-004` | Bel kabel dan alur yang sudah ada |
| Integrasi monitor pasien | — | `NV-DEC-011`, butuh gateway vendor | TTV input perawat |
| Jenis isolasi | — | Data milik Rawat Inap belum ada (`NV-OQ-013`) | Ikon isolasi tanpa jenis |

## 9. Alur bisnis target

`FLOW-NV-MVP-001`:
1. Admin IT mendaftarkan TV untuk lantai 3.
2. Admin membuat login; kata sandi tampil sekali.
3. Admin login di TV; papan lantai 3 tampil.
4. Perawat menyimpan TTV pasien bed 305-A dengan EWS 8.
5. Paling lambat 60 detik (seketika setelah `NV-OQ-016`), kartu 305-A merah dan teratas.
6. Perawat mendatangi pasien dan membuka rekam medis untuk detail.

## 10. Epic dan functional requirement

### EPIC NV-01 — Perangkat display · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-001` | Admin mendaftarkan perangkat dengan lantai yang dikenal; kode dari number series | Lantai "3" diterima → `NVW-000001`; lantai "99" → `422` |
| `FR-NV-002` | Buat login sekali; kata sandi hanya di response itu | Buat login kedua → `409` |
| `FR-NV-003` | Atur ulang login membatalkan login lama dan mengaktifkan kembali perangkat nonaktif | Token lama dipakai sesudah reset → ditolak |
| `FR-NV-004` | Nonaktifkan wajib beralasan; perangkat langsung ditolak runtime | Nonaktif 10.00 → permintaan 10.00.30 `403` |
| `FR-NV-005` | Daftar admin menampilkan online/offline dari `LastOnlineAt` | Terakhir online 21.00, sekarang 21.10 → "Offline sejak 21.00" |

### EPIC NV-02 — Papan lantai · disposisi `EXTEND` (adapter atas data yang ada)

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-010` | Papan memuat semua bed aktif di kamar aktif ber-`FloorCode` lantai perangkat, per bangsal | Lantai 3: Anak 8 + Bedah 12 = 20 kartu |
| `FR-NV-011` | Identitas samaran dihitung backend | "Budi Santoso", L, lahir 1962 → "B.S. · L · 64 th" |
| `FR-NV-012` | Perawat PJ atau "Belum ada PJ" | — |
| `FR-NV-013` | TTV sah terakhir per episode; TTV batal diabaikan | TTV 10.05 batal → tampil TTV 06.00 |
| `FR-NV-014` | Kartu kritis bila `IsCritical` atau EWS `Critical`; teratas | — |
| `FR-NV-015` | Ikon risiko jatuh hanya pita `HIGH`; alergi hanya ada/tidak; isolasi + tidak cocok | Morse 30 tanpa ikon, 50 dengan ikon |
| `FR-NV-016` | Bed tak berpenghuni berlabel ringkas | `Cleaning` → "Dibersihkan" |
| `FR-NV-017` | Header perawat jaga dari roster `Published` shift berjalan; kosong → "Roster shift ini belum tersedia" | — |
| `FR-NV-018` | Perangkat hanya membaca lantainya sendiri | — |

### EPIC NV-03 — Interval ukur ulang TTV · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-020` | Ganti-seluruh dengan rujukan pengesahan wajib; riwayat tidak ditimpa | Menit 0 → `400` |
| `FR-NV-021` | Tanda lewat jadwal hanya untuk tingkat yang punya interval | `Medium` 240, TTV 08.00 → 12.30 "lewat 30 menit" |
| `FR-NV-022` | Tanpa interval → aman-gagal | — |

### EPIC NV-04 — Pembaruan & data basi · disposisi `EXTEND`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-030` | Muat ulang penuh tiap 60 detik | Pasien pulang 14.25 → kartu berubah ≤ 14.26 |
| `FR-NV-031` | Pita data basi bila > 120 detik tanpa papan baru | Putus 10.42 → pita 10.44 |
| `FR-NV-032` | Push TTV lewat `NurseViewBoardChanged` tanpa identitas | **Bergantung `NV-OQ-016`**; tanpa itu `FR-NV-030` menggantikan |

## 11. Model status yang diusulkan

`NvwDisplayDevice.DeviceStatus`: `PendingLogin` → `Active` ⇄ `Inactive`. Invariant: perangkat
`Inactive` tidak pernah lolos runtime; satu perangkat satu akun login. Rincian:
`contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| `ProjectBatchAsync`, `InpNurseAssignment`, `TrxPatientVitalSign`, instrumen risiko jatuh, `TrxPatientAllergy`, `TrxShiftAssignment`, `NumberSeriesAllocator`, pola `queue-display` | `QueueHub` (+`JoinNurseViewFloor`), `AuthController`, `proxy.js`, `use-base-login.jsx`, `PatientVitalSignController` (titik terbit) | `NvwDisplayDevice`, `NvwVitalSignInterval`, 5 service, 3 controller, 4 layar |

## 13. Sasaran kemampuan API

Bagian dari `contracts/api-contract.md`; tidak melebihinya. Seluruhnya **Rencana (belum tersedia)**.

### Health Services / Nurse View Management / Display Device

Base URL: `api/v1/health-services/nurse-view-management/display-devices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar perangkat | `NurseViewDisplayDevice : Read` | `NvwDisplayDevicePagedQuery` | `PagedResult<NvwDisplayDeviceResponse>` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `GET` | `/floors` | Pilihan lantai | `NurseViewDisplayDevice : Read` | — | `List<NvwFloorOptionResponse>` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail | `NurseViewDisplayDevice : Read` | — | `NvwDisplayDeviceDetailResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `POST` | `/` | Daftarkan | `NurseViewDisplayDevice : Create` | `NvwDisplayDeviceCreateRequest` | `NvwDisplayDeviceDetailResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceUpdateRequest` | `NvwDisplayDeviceDetailResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login` | Buat login | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceLoginRequest` | `NvwDisplayDeviceLoginResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login/reset` | Atur ulang login | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceLoginRequest` | `NvwDisplayDeviceLoginResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceDeactivateRequest` | `NvwDisplayDeviceDetailResponse` | `EPIC NV-01` | **Rencana (belum tersedia)** |

### Health Services / Nurse View Management / Vital Sign Interval

Base URL: `api/v1/health-services/nurse-view-management/vital-sign-intervals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Himpunan aktif | `NurseViewVitalSignInterval : Read` | — | `NvwVitalSignIntervalSetResponse` | `EPIC NV-03` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Ganti seluruh | `NurseViewVitalSignInterval : Update` | `NvwVitalSignIntervalReplaceRequest` | `NvwVitalSignIntervalSetResponse` | `EPIC NV-03` | **Rencana (belum tersedia)** |

### Health Services / Nurse View Management / Display Runtime

Base URL: `api/v1/health-services/nurse-view-management/display-runtime`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/current` | Identitas perangkat | policy `NurseViewDisplayRead` | — | `NvwDisplayRuntimeCurrentResponse` | `EPIC NV-04` | **Rencana (belum tersedia)** |
| `GET` | `/board` | Papan lantai | policy `NurseViewDisplayRead` | — | `NvwFloorBoardResponse` | `EPIC NV-02` | **Rencana (belum tersedia)** |

## 14. Matriks kewenangan

| Peran | Kewenangan |
|---|---|
| Admin IT | `[AccessPermission("NurseViewDisplayDevice", "Read")]`, `("NurseViewDisplayDevice", "Create")`, `("NurseViewDisplayDevice", "Update")`, `("NurseViewVitalSignInterval", "Read")`, `("NurseViewVitalSignInterval", "Update")` |
| Kepala ruang | Opsional `("NurseViewVitalSignInterval", "Read")` |
| Akun perangkat | Policy `NurseViewDisplayRead` saja |

## 15. Batas integrasi dan billing

Nurse View **MUST NOT**: menulis ke tabel Rawat Inap, Clinical, HR, atau Patient; menyalin
penghuni bed; menghitung EWS sendiri; menetapkan ambang risiko jatuh sendiri; menerbitkan
tagihan. Tidak ada dampak billing.

## 16. Guardrail regulasi

| Kewajiban | Penerapan di MVP |
|---|---|
| Kerahasiaan rekam medis & pelindungan data pribadi (prinsip data minimal) | Identitas samaran di backend; tanpa nama lengkap, No. RM, alergen, diagnosis (`NV-DEC-008`, `019`) |
| Sasaran keselamatan pasien — risiko jatuh | Ikon mengikuti pita `HIGH` instrumen keperawatan; penanda fisik tetap gelang kuning (`G-05`) |
| Papan sebagai alat bantu | Keputusan klinis tetap berdasar rekam medis (`G-02`), dicantumkan di materi UAT dan pelatihan |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-001` | Jumlah query papan tetap per permintaan, tidak bertambah per bed |
| `NFR-002` | Papan satu lantai 40 bed tersusun ≤ 2 detik di server dev |
| `NFR-003` | Update perangkat memakai concurrency `Version`; konflik → `409` |
| `NFR-004` | `LastOnlineAt` ditulis paling sering 1×/60 detik per perangkat |
| `NFR-005` | Seluruh waktu tampil memakai jam operasional Asia/Jakarta |
| `NFR-006` | Kata sandi tidak tersimpan dan tidak tercatat di log mana pun |
| `NFR-007` | Sinyal realtime best-effort; kegagalannya tidak menggagalkan simpan TTV |

## 18. Skenario UAT

| UAT | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-01` | NV-01 berhasil | Lantai 3 punya kamar aktif | Daftarkan TV, buat login, login di TV | Papan lantai 3 tampil |
| `UAT-02` | NV-01 gagal | TV `Active` tampil | Admin menonaktifkan | TV kembali ke layar login; login ulang ditolak |
| `UAT-03` | NV-01 gagal | Perangkat sudah punya login | Tekan Buat Login lagi | Ditolak: gunakan Atur Ulang Login |
| `UAT-04` | NV-02 berhasil | Pasien "Budi Santoso" di 301-A, alergi aktif, Morse 50 | Buka papan | "B.S. · L · 64 th", ikon J dan A |
| `UAT-05` | NV-02 gagal (privasi) | Sama | Simpan response papan, cari "Budi" dan No. RM | Nol hasil |
| `UAT-06` | NV-02 gagal (akses) | Akun Administrator manusia | Buka `GET /board` | Ditolak `403` |
| `UAT-07` | NV-03 berhasil | Interval kosong | Isi `Medium` 240 + SK; TTV `Medium` 08.00 | 12.30 kartu "lewat jadwal 30 menit" |
| `UAT-08` | NV-03 gagal | — | Isi `Critical` 0 | Ditolak 1–1440 menit |
| `UAT-09` | NV-04 berhasil | Papan tampil | Pulangkan pasien 303-B | Kartu berubah ≤ 60 detik |
| `UAT-11` | NV-01 gagal (regresi login) | Akun perawat, Kiosk, dan display antrean tersedia | Login dengan ketiganya sesudah Nurse View terpasang | Perawat ke dashboard, Kiosk ke `/kiosk`, display antrean ke `/queue-display`; tidak ada yang ke `/nurse-view-display` |
| `UAT-10` | NV-04 gagal | Papan tampil | Cabut jaringan TV 3 menit | Pita data basi muncul; hilang saat tersambung |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Registry `Nvw` disetujui sebelum model pertama dibuat | Baris di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Restu berkas bersama tercatat dan syaratnya terbukti | `NV-DEC-030`; `NV-AC-023`–`027` dan `UAT-11` lulus |
| Satu TV menampilkan papan lantai nyata | `UAT-01`, `UAT-04` |
| Identitas tidak bocor di response | `UAT-05`, `NV-AC-002`, `012` |
| Perangkat nonaktif tidak bisa membaca papan | `UAT-02`, `NV-AC-016` |
| Akun manusia tidak bisa membaca papan | `UAT-06` |
| Interval aman-gagal dan bekerja setelah diisi | `UAT-07`, `UAT-08`, `NV-AC-004` |
| Papan memperbarui diri dan menandai data basi | `UAT-09`, `UAT-10` |
| Dua butir menu terdaftar dan terjangkau | `03-frontend-architecture.md` §2 + tangkapan layar sidebar |
| Hak akses Admin IT diberikan tanpa menimpa policy lain | Tangkapan layar Akses Role |
| Daftar periksa data go-live `G-06` terpenuhi | Hasil cek `FloorCode`, versi instrumen, `OrganizationUnitId` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | Registry, model `NvwDisplayDevice` + `NvwVitalSignInterval`, configuration, migration (oleh Rizki) | Blueprint disetujui (terpenuhi); langkah pertama: tulis baris registry `Nvw` (`NV-DEC-029`) |
| `MVP-1` | `EPIC NV-01` backend + `FE-NVW-01`/`02` + berkas bersama login perangkat | `MVP-0` (berkas bersama direstui, `NV-DEC-030`) |
| `MVP-2` | `EPIC NV-02` papan + `FE-NVW-04` dengan muat ulang 60 detik dan data basi (`FR-NV-030`, `031`) | `MVP-1` |
| `MVP-3` | `EPIC NV-03` interval + `FE-NVW-03` | `MVP-0` |
| `MVP-4` | `FR-NV-032` push TTV | `MVP-2` (`NV-OQ-016` disetujui Hamzah 2026-10-28) |
| `POST-MVP` | Bagian 8 | — |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| `NV-OQ-017` registry `Nvw` | Rizki Gunawan | — | **Terjawab 2026-10-28** (`NV-DEC-029`): ditulis pemilik modul di MVP-0 |
| `NV-OQ-015` berkas bersama login | Lead | — | **Terjawab 2026-10-28** (`NV-DEC-030`): direstui dengan syarat tidak mengganggu login lain dan meniru pola display antrean |
| `TLM-NV-001` episode: `internal` atau metode publik | Muhammad Hamzah | — | **Terjawab 2026-10-28:** `internal`, disetujui |
| `TLM-NV-001` keperawatan permintaan 2: pemetaan profesi | Muhammad Hamzah | — | **Terjawab 2026-10-28:** disetujui |
| `NV-OQ-016` titik terbit TTV | Muhammad Hamzah | — | **Terjawab 2026-10-28:** disetujui; `MVP-4` tinggal menunggu `MVP-2` |
| `NV-OQ-010` angka interval | Komite Keperawatan | Tanda lewat jadwal tidak tampil | Tidak — aman-gagal |

Seluruh pertanyaan memblokir sudah terjawab: blueprint disetujui (`NV-DEC-028`), registry (`NV-DEC-029`), dan berkas bersama (`NV-DEC-030`). Dokumen ini **boleh** diteruskan ke `plan-module-delivery`. `NV-OQ-010` tetap terbuka tetapi tidak memblokir (aman-gagal).
