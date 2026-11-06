# PRD → MVP — Nurse View

> **Revision 2 (`approved` `NV-DEC-055`, kontrak `0.2.0`) — 2026-11-05.** Gelombang MVP untuk tablet bed dan
> panggilan pasien ada di [Revision 2](#revision-2--tablet-bed-dan-panggilan-pasien).
> Bagian 1–20 di bawah tetap revision 1 yang sudah `approved`.

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

## Revision 2 — Tablet bed dan panggilan pasien

### R2.1 Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian SIMRS |
| Modul | Nurse View — tablet bed dan panggilan pasien (Amendment revision 5) |
| Status | `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan |
| Repository target | `NewQuilvianSystemBackend` (`rizkiG`), `QuilvianSystemFrontendDev` (`RizkiV2`) |
| Commit baseline | BE `0c6eab34`, FE `91a11b08f` |
| Contract version | `0.2.0` `approved` |
| Masukan | `02-backend-architecture.md` Revision 2, `contracts/*` `0.2.0`, `00-interview-decisions.md` rev `5`, `01-existing-capability-map.md` rev `2`, `02-requirement-completeness-assessment.md` rev `2` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| Ringkasan cakupan | Pasien memanggil perawat dari tablet bed; panggilan tampil berurutan di strip TV dan Konsol Panggilan; perawat mengklaim lalu menutup panggilan di bed atau dari konsol; bel kabel tetap jalur utama |

### R2.2 Ringkasan eksekutif

Saat ini pasien rawat inap hanya punya bel kabel: perawat mendengar bunyi, tetapi tidak tahu
siapa yang memanggil lebih dulu, siapa yang sudah menuju, dan berapa lama pasien menunggu.
Saat banyak pasien memanggil bersamaan, ada yang didatangi dua perawat dan ada yang terlewat.
Revision 2 menambah tablet di setiap bed, strip panggilan di TV nurse station, dan Konsol
Panggilan di PC nurse station, sehingga setiap panggilan terlihat, berurutan, punya satu
penanggung jawab, dan tercatat waktu tanggapnya.

### R2.3 Masalah produk

| Sudah ada (bukti) | Belum ada |
|---|---|
| TV per lantai dan akun perangkat (`NvwDisplayDevice`, `NV-CAP-12`, `13`) | Perangkat per bed (`NV-CAP-20`) |
| Penempatan aktif per bed dan akhir penempatan (`InpBedPlacement`, `NV-CAP-18`, `19`) | Data panggilan pasien (`NV-CAP-24`) |
| Grup realtime lantai dan pola join berizin (`QueueHub`, `NV-CAP-23`) | Grup konsol dan grup tablet |
| Izin otomatis dari atribut endpoint (`NV-CAP-22`) | Izin Panggilan Pasien |
| Pola bunyi di layar staf (`NV-CAP-27`) | Bunyi dengan pengingat dan nada Darurat |
| Prototipe `nurseCallContext.jsx` (`NV-CAP-29`) — `localStorage`, tidak dipakai | Pengganti yang tersimpan di server |

### R2.4 Visi produk

1. Pasien menekan "Panggil perawat" atau "Darurat" di tablet bed.
2. Panggilan langsung muncul di strip TV dan di konsol nurse station, Darurat paling atas.
3. Konsol berbunyi sampai ada perawat yang menekan "Saya tangani".
4. TV menampilkan siapa yang menuju, sehingga perawat lain mengambil panggilan berikutnya.
5. Perawat tiba dan menahan "Perawat hadir"; panggilan selesai dan waktu tanggap tercatat.
6. Bila pasien pindah atau pulang, panggilannya tertutup sendiri paling lambat 60 detik.

### R2.5 Batas MVP

**Titik mulai:**
1. Blueprint revision 2 disetujui owner, dan restu lead `NV-OQ-028` untuk berkas bersama.
2. Admin IT mendaftarkan satu tablet untuk satu bed berpenghuni di satu lantai yang sudah punya TV Nurse View.

**Titik akhir:**
1. Pasien di bed itu dapat memanggil biasa dan Darurat; panggilan tampil di strip TV dan konsol.
2. Perawat dapat mengklaim, mengambil alih, melepas, menutup di bed, dan menutup dari konsol.
3. Pasien dapat membatalkan panggilan biasa yang belum diklaim.
4. Panggilan tertutup sendiri saat pasien pindah atau pulang.
5. Setiap kejadian tercatat sebagai data waktu tanggap.
6. Admin dapat memasukkan ambang warna yang disahkan Komite; tanpa ambang, tampil tanpa warna.

### R2.6 Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
|---|---|
| Pasien atau keluarga | Memanggil, menaikkan ke Darurat, membatalkan panggilan biasa |
| Perawat rawat inap | Mengklaim, mengambil alih, melepas, hadir di bed, menutup dari konsol |
| Kepala ruang | Sama dengan perawat; edukasi pasien tentang Darurat (`NV-DEC-042`) |
| Admin IT | Mengelola tablet bed, memberi izin, memasukkan ambang |
| Komite Keperawatan | Mengesahkan angka ambang di luar sistem |
| Akun tablet bed, akun TV | Menjalankan layar masing-masing |

### R2.7 Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Bed → kamar → lantai | `NV-CAP-17` | Wajib; menentukan TV dan konsol mana yang menerima panggilan |
| Penghuni aktif per bed | `NV-CAP-18` | Wajib; bed kosong tidak boleh memanggil (`NV-DEC-046`) |
| Deteksi pindah/pulang | `NV-CAP-19` | Wajib; tanpa ini panggilan menggantung |
| Akun tablet bed | `NV-CAP-20` | Wajib; tanpa ini tidak ada yang bisa memanggil |
| Halaman tablet tanpa sidebar | `NV-CAP-21` | Wajib |
| Izin konsol | `NV-CAP-22` | Wajib |
| Grup realtime | `NV-CAP-23` | Wajib; tanpa ini panggilan terlambat sampai 60 detik |
| Data panggilan, satu aktif per bed, klaim, kirim ulang | `NV-CAP-24` | Wajib; inti modul |
| Jejak dan nama pengklaim | `NV-CAP-25` | Wajib; dasar waktu tanggap (`NV-DEC-049`) |
| Inisial di konsol | `NV-CAP-26` | Wajib (`NV-DEC-044`) |
| Bunyi konsol | `NV-CAP-27` | Wajib; tanpa bunyi panggilan bisa terlewat |
| Menu konsol berizin | `NV-CAP-28` | Wajib |
| Hapus prototipe | `NV-CAP-29` | Wajib (`NV-DEC-051`) |

### R2.8 Kemampuan yang ditunda

| Kemampuan | ID asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Layar laporan waktu tanggap | — | Keputusan owner `NV-DEC-049`; datanya sudah dicatat sejak MVP | Data jejak dapat ditarik tim IT bila diperlukan |
| Penilaian "Darurat tidak sesuai" | — | Ditolak untuk MVP (`NV-DEC-042`) | Edukasi pasien saat orientasi kamar |
| Notifikasi HP perawat, eskalasi ke pihak lain | — | Di luar scope (`NV-DEC-035`, `G-11`) | Bunyi dan pengingat konsol; bel kabel |
| Integrasi bel kabel | — | Di luar scope (`NV-DEC-037`) | Bel kabel tetap berjalan sendiri |
| Ambang per lantai | — | Belum ada kebutuhan; `ScopeCode` disiapkan | Satu ambang `GLOBAL` |
| Pemesanan makan, jadwal visite, data klinis di tablet | — | Di luar scope (`NV-DEC-035`) | Alur yang sudah ada |
| Angka ambang sah | `NV-OQ-022` | Kebijakan Komite Keperawatan | Tanpa warna; lama menunggu tetap tampil |

### R2.9 Alur bisnis target

`FLOW-NV-MVP-002`:
1. Admin IT mendaftarkan tablet untuk bed 305-A dan membuat login; tablet dipasang.
2. Pukul 10.00.00 pasien 305-A menekan "Panggil perawat".
3. Strip TV lantai 3 dan konsol lantai 3 menampilkan 305-A; konsol berbunyi.
4. Pukul 10.01.00 Sari menekan "Saya tangani"; TV menampilkan "305-A · Sari menuju"; pengingat berhenti.
5. Pukul 10.02.30 Sari tiba dan menahan "Perawat hadir" 3 detik.
6. Panggilan selesai sebagai `Handled`, dikonfirmasi di bed; waktu tanggap 2 menit 30 detik tercatat.

### R2.10 Epic dan functional requirement

#### EPIC NV-05 — Tablet bed · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-033` | Admin mendaftarkan tablet untuk bed yang sah; kode dari number series; satu bed paling banyak satu tablet yang belum nonaktif (`AD-01`) | Bed 305-A → `NVT-000001`; tablet kedua untuk 305-A → `409` `VAL-NV-19` |
| `FR-NV-034` | Buat login sekali dengan kata sandi tampil sekali; atur ulang membatalkan login lama; nonaktif wajib beralasan dan langsung ditolak runtime | Nonaktif 14.00 → permintaan tablet 14.00.30 `403` |
| `FR-NV-035` | Tablet dipindah ke bed lain hanya bila bed lama tanpa panggilan aktif (`AD-04`) | 305-A masih punya panggilan → `409` `VAL-NV-20` |
| `FR-NV-036` | Runtime tablet: bed dari akun; bed kosong → terkunci; layar tanpa identitas pasien; tablet ditolak papan TV dan TV ditolak runtime tablet | Akun tablet membuka papan → `403` |

#### EPIC NV-06 — Siklus panggilan · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-037` | Tekan panggil membuat panggilan `Waiting`; satu bed satu panggilan aktif; tekan ulang menambah hitungan; jam awal tetap; kunci permintaan mencegah dobel | Kunci sama dikirim dua kali → satu jejak |
| `FR-NV-038` | "Darurat" menaikkan panggilan yang sama; prioritas tidak pernah turun | Biasa 10.00.00, Darurat 10.00.50 → satu item Darurat sejak 10.00.00 |
| `FR-NV-039` | Klaim tunggal, ambil alih oleh perawat lain, lepas oleh pengklaim; klaim bersamaan → satu `409` | Sari dan Dewi mengklaim 10.01.00 → satu berhasil |
| `FR-NV-040` | "Perawat hadir" ditahan 3 detik menutup panggilan sebagai `Handled` dikonfirmasi di bed; tetap bisa saat bed kini kosong | Dilepas 2 detik → panggilan tetap aktif |
| `FR-NV-041` | "Tutup panggilan" dari konsol dengan alasan wajib; tercatat tidak dikonfirmasi di bed | Tanpa alasan → `400`; "Lainnya" tanpa keterangan → `400` |
| `FR-NV-042` | Pasien membatalkan hanya panggilan biasa yang masih `Waiting` | Diklaim atau Darurat → `409` `VAL-NV-31` |
| `FR-NV-043` | Pindah atau pulang menutup panggilan paling lambat 60 detik; koreksi dan pembatalan penempatan tidak | Pindah 10.05.00 → `ClosedBySystem` paling lambat 10.06.00 |
| `FR-NV-044` | Setiap kejadian menulis tepat satu jejak beserta pelaku dan jamnya | Satu panggilan lengkap → jejak Pressed, EscalatedToEmergency, Claimed, TakenOver, NurseArrived |
| `FR-NV-045` | Tablet yang gagal mengirim menampilkan pesan bel dinding dan mengirim ulang otomatis tanpa membuat panggilan ganda | Wi-Fi pulih → tepat satu panggilan |

#### EPIC NV-07 — Strip Panggilan Aktif di TV · disposisi `EXTEND`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-046` | Strip tetap di atas papan, tidak ikut berganti halaman, berurutan, tanpa identitas pasien; kelebihan ditandai "+k lainnya" | 331-C di halaman 3 tampil saat halaman 1 aktif |
| `FR-NV-047` | Kartu bed yang memanggil diberi penanda; TV tetap tanpa suara | Kartu 305-A bertanda saat halaman 1 tampil |
| `FR-NV-048` | Strip diperbarui lewat sinyal tanpa menunggu muat ulang 60 detik; cadangan muat ulang tetap ada | Panggilan 10.00.00 tampil sebelum 10.00.05 di jaringan normal |

#### EPIC NV-08 — Konsol Panggilan · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-049` | Konsol dibuka dengan akun sendiri, memilih lantai yang diingat PC; klaim berdasarkan izin, tanpa roster | Roster kosong → Sari tetap bisa mengklaim |
| `FR-NV-050` | Daftar berurutan dengan inisial · jenis kelamin · umur, lama menunggu, pengklaim, dan aksi sesuai status | 305-A diklaim Sari → di layar Sari tombol Lepas, di layar Dewi tombol Ambil alih |
| `FR-NV-051` | Bunyi per panggilan baru, nada Darurat berbeda, pengingat sampai semua diklaim, tunda pengingat; tombol Aktifkan suara bila browser menahan | Semua diklaim → senyap |
| `FR-NV-052` | Prototipe `nurseCallContext.jsx` dihapus | Pencarian source → nol hasil |

#### EPIC NV-09 — Ambang lama menunggu · disposisi `MISSING / NEW`

| FR | Requirement | Contoh |
|---|---|---|
| `FR-NV-053` | Ambang kuning dan merah diisi admin dengan rujukan SK Komite; kosong → tanpa warna; merah harus lebih besar dari kuning | Kuning 3, merah 5 → 305-A merah pada menit ke-5; merah 2 → `400` |

### R2.11 Model status yang diusulkan

Tablet bed: `PendingLogin` → `Active` ↔ `Inactive` (sama dengan perangkat display). Panggilan:
`Waiting` → `Claimed` → `Handled`; `Waiting` → `Cancelled`; `Waiting`/`Claimed` →
`ClosedBySystem`. Invariant utama: satu panggilan aktif per bed, jam awal tetap, prioritas hanya
naik, satu pengklaim, panggilan selesai tidak dibuka ulang. Rincian di
`contracts/state-transition-matrix.md` §4–§5.

### R2.12 Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| Penempatan bed Rawat Inap (baca), master bed/kamar, pola akun perangkat, `NumberSeriesAllocator`, `NvwPatientMasking`, mesin izin Akses Role, pola bunyi layar staf | `NvwRealtimeService`, `NvwFloorBoardQueryService`, `NvwDisplayRuntimeController`, `QueueHub`, `queue-realtime-client.js`, berkas login bersama | `NvwBedTablet`, `NvwPatientCall`, `NvwPatientCallEvent`, `NvwPatientCallWaitThreshold`, enam service, empat controller, pemeriksaan berkala |

### R2.13 Sasaran kemampuan API

Seluruhnya bagian dari `contracts/api-contract.md` R2 dan berstatus **Rencana (belum tersedia)**.

#### Health Services / Nurse View Management / Bed Tablet

Base URL: `api/v1/health-services/nurse-view-management/bed-tablets`

| Method | Path | Kegunaan | Hak akses | Epic | Status |
|---|---|---|---|---|---|
| `GET` | `/`, `/floors`, `/bed-options`, `/{id}` | Daftar, pilihan, rincian | `NurseViewBedTablet : Read` | `EPIC NV-05` | **Rencana (belum tersedia)** |
| `POST` | `/` | Daftarkan tablet | `NurseViewBedTablet : Create` | `EPIC NV-05` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah tablet | `NurseViewBedTablet : Update` | `EPIC NV-05` | **Rencana (belum tersedia)** |
| `POST`, `PATCH` | `/{id}/login`, `/{id}/login/reset`, `/{id}/deactivate` | Login dan nonaktif | `NurseViewBedTablet : Update` | `EPIC NV-05` | **Rencana (belum tersedia)** |

#### Health Services / Nurse View Management / Bed Tablet Runtime

Base URL: `api/v1/health-services/nurse-view-management/bed-tablet-runtime`

| Method | Path | Kegunaan | Hak akses | Epic | Status |
|---|---|---|---|---|---|
| `GET` | `/current` | Keadaan tablet | policy `NurseViewBedTabletAccess` | `EPIC NV-05`, `06` | **Rencana (belum tersedia)** |
| `POST` | `/calls` | Panggil, tekan ulang, naik Darurat | policy `NurseViewBedTabletAccess` | `EPIC NV-06` | **Rencana (belum tersedia)** |
| `POST` | `/calls/current/cancel`, `/calls/current/nurse-arrived` | Batal, perawat hadir | policy `NurseViewBedTabletAccess` | `EPIC NV-06` | **Rencana (belum tersedia)** |

#### Health Services / Nurse View Management / Patient Call

Base URL: `api/v1/health-services/nurse-view-management/patient-calls`

| Method | Path | Kegunaan | Hak akses | Epic | Status |
|---|---|---|---|---|---|
| `GET` | `/floors`, `/active` | Pilihan lantai, daftar konsol | `NurseViewPatientCall : Read` | `EPIC NV-08` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/claim`, `/take-over`, `/release`, `/close` | Aksi perawat | `NurseViewPatientCall : Update` | `EPIC NV-06`, `08` | **Rencana (belum tersedia)** |

#### Health Services / Nurse View Management / Patient Call Wait Threshold

Base URL: `api/v1/health-services/nurse-view-management/patient-call-wait-thresholds`

| Method | Path | Kegunaan | Hak akses | Epic | Status |
|---|---|---|---|---|---|
| `GET` | `/` | Ambang aktif | `NurseViewPatientCallWaitThreshold : Read` | `EPIC NV-09` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Ganti-seluruh ambang | `NurseViewPatientCallWaitThreshold : Update` | `EPIC NV-09` | **Rencana (belum tersedia)** |

#### Health Services / Nurse View Management / Display Runtime

Base URL: `api/v1/health-services/nurse-view-management/display-runtime`

| Method | Path | Kegunaan | Hak akses | Epic | Status |
|---|---|---|---|---|---|
| `GET` | `/active-calls` | Strip TV | policy `NurseViewDisplayRead` | `EPIC NV-07` | **Rencana (belum tersedia)** |

### R2.14 Matriks kewenangan

| Peran | Kewenangan |
|---|---|
| Admin IT | `[AccessPermission("NurseViewBedTablet", "Read")]`, `("NurseViewBedTablet", "Create")`, `("NurseViewBedTablet", "Update")`, `("NurseViewPatientCallWaitThreshold", "Read")`, `("NurseViewPatientCallWaitThreshold", "Update")` |
| Perawat rawat inap, kepala ruang | `("NurseViewPatientCall", "Read")`, `("NurseViewPatientCall", "Update")`; kepala ruang opsional `("NurseViewPatientCallWaitThreshold", "Read")` |
| Akun tablet bed | Policy `NurseViewBedTabletAccess` saja |
| Akun TV | Policy `NurseViewDisplayRead` saja |

### R2.15 Batas integrasi dan billing

Nurse View **MUST NOT**: menulis ke tabel Rawat Inap, Clinical, Patient, atau Master Data;
menambah titik terbit di kode transfer atau pemulangan Rawat Inap (`NV-DEC-052`); mengintegrasi
bel kabel; mengirim notifikasi ke HP; memanggil tim kegawatan (`NV-DEC-039`); menerbitkan tagihan.
Tidak ada dampak billing.

### R2.16 Guardrail regulasi

| Kewajiban | Penerapan di MVP |
|---|---|
| Pelindungan data pribadi (data minimal) | Tablet dan strip TV tanpa identitas pasien; konsol hanya inisial · jenis kelamin · umur; disamarkan di backend (`NV-DEC-044`, `G-09`) |
| Sarana panggil pasien | Tablet **melengkapi** bel kabel; bel kabel tetap jalur utama (`NV-DEC-037`). Ketersediaan bel di setiap bed diperiksa sebelum go-live (`NV-UNK-005`) |
| Keselamatan pasien | Darurat bukan pengganti prosedur kegawatan rumah sakit (`NV-DEC-039`); tablet gagal kirim selalu menunjuk bel dinding |
| Jejak audit | Setiap kejadian panggilan bernama dan berjam; kehadiran tidak terverifikasi identitas disebut di setiap laporan (`NV-DEC-041`, `049`) |

### R2.17 Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-008` | Panggilan baru tampil di strip TV dan konsol dalam ≤ 5 detik pada jaringan dev normal (target uji, tanpa menunggu muat ulang 60 detik) |
| `NFR-009` | Satu panggilan aktif per bed dijamin index unik berfilter, bukan hanya oleh kode |
| `NFR-010` | Seluruh `POST` runtime tablet idempoten lewat `Idempotency-Key` |
| `NFR-011` | Perintah panggilan dan tablet memakai concurrency `Version`; konflik → `409` |
| `NFR-012` | Pemeriksaan pindah/pulang tiap 30 detik; galat satu putaran tidak menghentikan host maupun putaran berikutnya |
| `NFR-013` | Daftar konsol, strip TV, dan papan memakai jumlah query tetap per permintaan, tidak bertambah per panggilan |
| `NFR-014` | Waktu disimpan UTC, tampil Asia/Jakarta; lama menunggu dihitung dari jam server |
| `NFR-015` | Kata sandi tablet dan `CloseNote` tidak pernah masuk log maupun sinyal realtime |

### R2.18 Skenario UAT

| UAT | Epic | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|
| `UAT-12` | NV-05 berhasil | Bed 305-A berpenghuni | Daftarkan tablet, buat login, login di tablet | Layar `Siap` dengan dua tombol; tanpa nama pasien |
| `UAT-13` | NV-05 gagal | 305-A sudah punya tablet aktif | Daftarkan tablet kedua untuk 305-A | Ditolak: bed sudah punya tablet |
| `UAT-14` | NV-05 gagal (privasi) | Tablet 305-A aktif | Dengan akun tablet, buka papan TV dan strip | Ditolak `403` |
| `UAT-15` | NV-06 berhasil | Tablet, TV, konsol lantai 3 aktif | Panggil → Saya tangani → tahan Perawat hadir | `Handled`, dikonfirmasi di bed, waktu tanggap tercatat |
| `UAT-16` | NV-06 berhasil (banyak panggilan) | Tiga bed berpenghuni | 305-A biasa, 318-B biasa, 331-C Darurat, 305-A tekan lagi | Urutan 331-C, 305-A (×2), 318-B; tidak ada panggilan ganda |
| `UAT-17` | NV-06 gagal | 305-A `Waiting` | Sari dan Dewi mengklaim bersamaan; lalu pasien menekan Batal | Satu klaim ditolak `409`; Batal tidak tersedia |
| `UAT-18` | NV-06 gagal | 312-B diklaim, tablet dimatikan | Tutup dari konsol tanpa alasan, lalu dengan alasan "Tablet tidak dapat dipakai" | Pertama ditolak; kedua selesai, "tidak dikonfirmasi di bed" |
| `UAT-19` | NV-06 berhasil | 331-C `Waiting` | Pindahkan pasien ke 402-B; di bed lain lakukan koreksi salah catat | 331-C `ClosedBySystem` ≤ 60 detik; bed koreksi tetap aktif |
| `UAT-20` | NV-06 gagal | Tablet 305-A aktif | Putuskan Wi-Fi, tekan panggil, sambungkan lagi | Pesan bel dinding; tepat satu panggilan sesudah tersambung |
| `UAT-21` | NV-07 berhasil | TV lantai 3 berisi 3 halaman | 331-C (halaman 3) memanggil saat halaman 1 tampil | Strip langsung menampilkan 331-C, tanpa identitas; kartu bertanda |
| `UAT-22` | NV-07 gagal | Lebih banyak panggilan daripada kapasitas strip | Tambah panggilan, lalu putuskan jaringan TV 3 menit | "+k lainnya"; pita data basi; strip kembali saat tersambung |
| `UAT-23` | NV-08 berhasil | Konsol lantai 3 terbuka, suara aktif | Dua panggilan masuk; klaim satu; tunda pengingat; panggilan ketiga masuk | Bunyi, pengingat, tetap berbunyi untuk panggilan baru, senyap saat semua diklaim |
| `UAT-24` | NV-08 gagal | Pengguna tanpa izin; browser menahan bunyi | Buka konsol; buka konsol dengan pengguna berizin di browser baru | Pertama ditolak; kedua menampilkan Aktifkan suara |
| `UAT-25` | NV-09 berhasil dan gagal | Ambang kosong | Isi merah 2 dan kuning 3; lalu kuning 3, merah 5 + SK | Pertama ditolak; kedua diterima dan warna tampil sesuai menit |
| `UAT-26` | NV-05 gagal (regresi login) | Akun perawat, Kiosk, display antrean, TV Nurse View | Login keempatnya sesudah tablet terpasang | Perilaku sama dengan `UAT-11`; tidak ada yang ke `/nurse-view-bed-tablet` |

### R2.19 Definition of Done

| Butir | Bukti |
|---|---|
| Restu lead untuk berkas bersama tercatat dan syaratnya terbukti | `NV-OQ-028` ditutup; `NV-AC-041`, `UAT-26` lulus |
| Satu tablet bed memanggil dan panggilan ditutup di bed | `UAT-12`, `UAT-15` |
| Banyak panggilan berurutan tanpa ganda | `UAT-16`, `NV-AC-029`, `030` |
| Klaim tunggal terbukti saat bersamaan | `UAT-17`, `VAL-NV-38` |
| Panggilan tidak pernah tertinggal tanpa jalan tutup | `UAT-18`, `UAT-19`, `NV-AC-042`, `044` |
| Gagal kirim tidak membuat ganda dan selalu menunjuk bel dinding | `UAT-20`, `NV-AC-039` |
| Identitas pasien tidak bocor di tablet dan TV | `UAT-14`, `UAT-21`, `NV-AC-037` |
| Konsol berbunyi dan berhenti sesuai aturan | `UAT-23`, `NV-AC-035` |
| Tiga butir menu terdaftar dan terjangkau | `03-frontend-architecture.md` R2.2 + tangkapan layar sidebar |
| Hak akses diberikan tanpa menimpa policy lain | Tangkapan layar Akses Role |
| Prototipe lama terhapus | `NV-AC-043` |
| Daftar periksa go-live `G-16` terpenuhi | Hasil cek bel kabel, uji tablet contoh, praktik pencatatan transfer |

### R2.20 Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-5` | Model, enum, configuration, 4 DbSet, migration `AddNurseViewPatientCall` (oleh Rizki) | Blueprint revision 2 disetujui — **terpenuhi** (`NV-DEC-055`) |
| `MVP-6` | `EPIC NV-05` backend + `FE-NVW-05`/`06` + login tablet di berkas bersama | `MVP-5` dan restu lead `NV-OQ-028` — restu **terpenuhi 2026-11-05** (`NV-DEC-056`) |
| `MVP-7` | `EPIC NV-06` backend: runtime tablet, endpoint konsol, pemeriksaan berkala, realtime, method hub | `MVP-6` |
| `MVP-8` | `EPIC NV-06`/`08` frontend: `FE-NVW-07`, `FE-NVW-08` beserta bunyi dan penghapusan prototipe; `EPIC NV-07` strip TV | `MVP-7` |
| `MVP-9` | `EPIC NV-09` ambang backend + `FE-NVW-09` | `MVP-5`; tidak menahan `MVP-8` karena aman-gagal |
| `POST-MVP` | R2.8 | — |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| `NV-OQ-028` restu berkas bersama: login tablet, policy, route tablet, method hub, menu | Lead | `MVP-6`–`MVP-8` tidak dapat diimplementasikan | **Ditutup 2026-11-05** (`NV-DEC-056`): restu lead dengan syarat `NV-DEC-030`; sebelumnya **Ya** |
| Usulan `AD-01`–`06`, `G-09`–`G-17`, `NV-FE-008`–`010` | Rizki Gunawan | — | **Terjawab 2026-11-05** (`NV-DEC-055`): seluruhnya disahkan |
| `NV-OQ-022` angka ambang | Komite Keperawatan | Tampil tanpa warna | Tidak — aman-gagal |
| `NV-UNK-005`–`007` bel kabel, praktik transfer, uji tablet | Pengelola sarana, kepala ruang, Admin IT | Go-live tertahan | Tidak untuk desain; ya untuk go-live |

Blueprint revision 2 disetujui owner (`NV-DEC-055`). Dokumen ini **belum boleh** diteruskan ke
`plan-module-delivery` selama `NV-OQ-028` belum dijawab lead.

**Pembaruan 2026-11-05.** `NV-OQ-028` dijawab lead (`NV-DEC-056`, dilaporkan Rizki Gunawan), sehingga
dokumen ini diteruskan ke `plan-module-delivery`. Hasilnya bagian "Revision 2" pada
`roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`, dan `roadmap/requirement-traceability.md`.
