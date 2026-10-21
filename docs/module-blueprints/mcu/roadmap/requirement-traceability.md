# Traceability Requirement — MCU (Medical Check-Up)

| Field | Nilai |
|---|---|
| Revision | `1`, status `approved` (Yoga Aji Pratama, 2026-10-21) |
| Roadmap | `MCU-RM-BE-001` r1, `MCU-RM-FE-001` r1 |
| Blueprint | `MCU-BP-001` r1 `approved` (2026-10-21) |
| SHA | BE `4d3e4f98`, FE `7fb1e1d3e` |
| Kontrak | `MCU-API-v1`, `MCU-STATE-v1`, `MCU-VAL-v1`, `MCU-INT-v1`, `MCU-PERM-v1` — `approved` |
| Input hash | Lihat [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Input revisions* dan *Artifact hashes* |

Kolom *Status* hanya berisi keadaan rencana: **Direncanakan**, **⛔ Tertahan**, **Selesai** (dengan
bukti), atau **Di luar roadmap**. Tidak ada task implementasi yang selesai pada revisi ini.

## 1. Requirement → Task → Bukti

| Requirement | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti yang diharapkan | Status |
|---|---|---|---|---|---|---|---|
| `FR-MCU-001` | `MCU-GOV-001` | BE §6 | Registry | `BE-MCU-000` ✅, `BE-MCU-002` | — | Baris registry + checker tanpa QBE-MOD-002 (`MCU-EVD-002` §1); checker `PASS` atas model nyata | Registry selesai; model direncanakan |
| `FR-MCU-002` | `MCU-DEC-007`, `015`, `025` | BE §4.1 | `VAL-MCU-01`..`08` | `BE-MCU-005` | `FE-MCU-002`, `003` | `AT-MCU-01`, `03`; `UAT-01`, `02` | Direncanakan |
| `FR-MCU-003` | `MCU-DEC-020`, `025` | BE §4.1 | `VAL-MCU-05` | `BE-MCU-005` | `FE-MCU-002`, `003` | `AT-MCU-02` | Direncanakan |
| `FR-MCU-004` | `MCU-DEC-007`; `MCU-OQ-002`, `003` | BE §9 | `VAL-MCU-09` | `BE-MCU-004`, `006` | `FE-MCU-002`, `004` | Uji unit `BE-MCU-006`; seeder idempotent | Direncanakan |
| `FR-MCU-005` | `MCU-DEC-012`, `042` | ERD jadwal | `VAL-MCU-10`, `14` | `BE-MCU-007` | `FE-MCU-002`, `005` | Uji unit `BE-MCU-007` | Direncanakan |
| `FR-MCU-006` | `MCU-DEC-029` | BE §5.5 | `VAL-MCU-13` | `BE-MCU-007` | `FE-MCU-005` | `AT-MCU-06` | Direncanakan |
| `FR-MCU-010` | `MCU-DEC-003` | BE §2.1 | `MCU-STATE-v1` | `BE-MCU-011` | `FE-MCU-006`, `008` | `AT-MCU-08`; `UAT-03` | Direncanakan |
| `FR-MCU-011` | `MCU-DEC-012` | BE §2 | `VAL-MCU-11`, `12` | `BE-MCU-007`, `011` | `FE-MCU-008` | `AT-MCU-04`, `05`; `UAT-04` | Direncanakan |
| `FR-MCU-012` | `MCU-DEC-033` | ERD layanan | `VAL-MCU-16`, `17` | `BE-MCU-011` | `FE-MCU-008` | `AT-MCU-08`, `09` | Direncanakan |
| `FR-MCU-013` | `MCU-DEC-041` | — | `VAL-MCU-18` | `BE-MCU-011` | `FE-MCU-008` | `AT-MCU-10` | Direncanakan |
| `FR-MCU-014` | `MCU-DEC-014`, `038` | — | `VAL-MCU-23` | `BE-MCU-011` | `FE-MCU-008` | `AT-MCU-14`, `15` | Direncanakan |
| `FR-MCU-015` | `MCU-DEC-014`, `030` | BE §13 | `MCU-STATE-v1` | `BE-MCU-012` | — | `AT-MCU-13`; `UAT-05` | Direncanakan |
| `FR-MCU-020` | `MCU-DEC-039` | — | `VAL-MCU-25` | `BE-MCU-013` | `FE-MCU-006`, `009` | `AT-MCU-18`, `19`; `UAT-07` | Direncanakan |
| `FR-MCU-021` | `MCU-DEC-003`, `028`, `032`, `042` | BE §2.1 | `VAL-MCU-26`, `27`; `MCU-INT-v1` | `BE-MCU-001` ✅, `013` | `FE-MCU-009` | `AT-MCU-16`, `20`, `21`, `24`; `AT-MCU-RUN-01`..`03` ✅ ([`BE-MCU-001`](../task/report/backend/BE-MCU-001.md)); `UAT-06` | Direncanakan — bagian kunjungan selesai (`BE-MCU-001`), check-in menunggu `BE-MCU-013` |
| `FR-MCU-022` | `MCU-DEC-020`, `025` | — | `VAL-MCU-29` | `BE-MCU-013` | `FE-MCU-009` | `AT-MCU-22`; `UAT-08` | Direncanakan |
| `FR-MCU-023` | `MCU-DEC-013`, `029` | — | `VAL-MCU-27`, `28` | `BE-MCU-013`, `022` | `FE-MCU-009` | `AT-MCU-20`, `07` | Direncanakan |
| `FR-MCU-024` | `MCU-DEC-041` | — | `VAL-MCU-30` | `BE-MCU-013` | `FE-MCU-009` | `AT-MCU-17` | Direncanakan |
| `FR-MCU-025` | `MCU-DEC-017`, `037` | — | `VAL-MCU-10`, `11` | `BE-MCU-014` | `FE-MCU-009` | Uji integrasi `BE-MCU-014` | Direncanakan |
| `FR-MCU-030` | `MCU-DEC-004`, `024` | BE §2.1 | `VAL-MCU-34` | `BE-MCU-021` | `FE-MCU-011` | `AT-MCU-26`, `27`; `UAT-09` | Direncanakan |
| `FR-MCU-031` | `MCU-DEC-024`, `032` | BE §5.4 | `MCU-INT-v1` `INT-MCU-05`, `06` | `BE-MCU-020`, `021` | `FE-MCU-011` | `AT-MCU-26` | Direncanakan |
| `FR-MCU-032` | `MCU-DEC-005`, `022` | BE §5.4 | `MCU-INT-v1` | `BE-MCU-020` | `FE-MCU-015` | `AT-MCU-28`; `UAT-10` | Direncanakan |
| `FR-MCU-033` | `MCU-DEC-036` | — | `MCU-STATE-v1` | `BE-MCU-022` | `FE-MCU-011` | `AT-MCU-29` | Direncanakan |
| `FR-MCU-034` | `MCU-DEC-015`, `043`; `ARC-TECH-01` ✅ | BE §5.5 | `INT-MCU-08`, `09`; `VAL-MCU-38`, `40` | `BE-MCU-024`, `BE-MCU-026` | `FE-MCU-011` (aksi batal) | `AT-MCU-30`, `31`, `37`; `UAT-11`; sisi Lab `MCU-EVD-002` §2.1 ✅ | Direncanakan (Billing disetujui 2026-10-21) |
| `FR-MCU-035` | `MCU-DEC-021` | BE §2.1 | `VAL-MCU-41` | `BE-MCU-025` | `FE-MCU-011` | `AT-MCU-32`; `UAT-12`; refund utuh didukung jalur yang ada (`MCU-EVD-002` §2.2) | Direncanakan |
| `FR-MCU-036` | `MCU-DEC-018`, `035` | — | `VAL-MCU-36`, `42`, `43`, `44` | `BE-MCU-023` | `FE-MCU-001`, `011` | `AT-MCU-33`, `34`, `35`; `UAT-13` | Direncanakan |
| `FR-MCU-037` | `MCU-DEC-007`, `009` | — | `MCU-API-v1` | `BE-MCU-010`, `022` | `FE-MCU-007`, `011` | Uji integrasi `BE-MCU-010` (tampilan *3 dari 4*) | Direncanakan |
| `FR-MCU-040` | `MCU-DEC-009`, `018` | — | `VAL-MCU-35`, `36` | `BE-MCU-030` | `FE-MCU-012` | `AT-MCU-38`; `UAT-14` | Direncanakan |
| `FR-MCU-041` | `MCU-DEC-032` | BE §11 | `VAL-MCU-38` | `BE-MCU-030` | `FE-MCU-012` | `AT-MCU-39`; `UAT-16` | Direncanakan |
| `FR-MCU-042` | `MCU-DEC-008` | BE §11 | `VAL-MCU-39` | `BE-MCU-030` | `FE-MCU-013` | `AT-MCU-40`; `UAT-15` | Direncanakan |
| `FR-MCU-043` | `MCU-DEC-008` | — | `VAL-MCU-40` | `BE-MCU-030` | `FE-MCU-013` | Uji integrasi `BE-MCU-030` | Direncanakan |
| `FR-MCU-044` | `MCU-DEC-011` | — | `MCU-STATE-v1` | `BE-MCU-031`, `034` | `FE-MCU-012` | `AT-MCU-43`; `UAT-17` | Direncanakan |
| `FR-MCU-045` | `MCU-DEC-040` | — | `VAL-MCU-45` | `BE-MCU-032` | `FE-MCU-014` | `AT-MCU-39` bagian ketiga | Direncanakan |
| `FR-MCU-050` | `MCU-DEC-010` | BE §11 | `MCU-API-v1` Mcu Delivery | `BE-MCU-033` | `FE-MCU-010`, `013` | `AT-MCU-42` (refleksi DTO) | Direncanakan |
| `FR-MCU-051` | `MCU-DEC-010` | — | `VAL-MCU-31` | `BE-MCU-034` | `FE-MCU-013` | `AT-MCU-42`; `UAT-19` | Direncanakan |
| `FR-MCU-052` | `MCU-DEC-019`, `027` | — | `VAL-MCU-32`, `33` | `BE-MCU-034` | `FE-MCU-013` | `AT-MCU-41`, `44`; `UAT-18` | Direncanakan |
| `FR-MCU-053` | `MCU-DEC-010` | — | `MCU-PERM-v1` | `BE-MCU-034` | `FE-MCU-013` | Uji integrasi `BE-MCU-034` | Direncanakan |

## 2. Kebutuhan non-fungsional → Task

| NFR | Isi singkat | Task | Bukti |
|---|---|---|---|
| `NFR-001` | Atomicity check-in, mulai pemeriksaan, batal penuh | `BE-MCU-013`, `021`, `025` | `AT-MCU-24`; uji interceptor gagal-simpan |
| `NFR-002` | Konkurensi kuota dan `Version` order | `BE-MCU-007`, `011`, seluruh task ubah | `AT-MCU-05`; `VAL-MCU-46` per task |
| `NFR-003` | Idempotensi check-in | `BE-MCU-013` | `AT-MCU-17` |
| `NFR-004` | Audit tahan lama | Seluruh task ubah (riwayat status/dokter, versi resume, penyerahan) | Baris riwayat pada setiap AT — lihat celah `GAP-MCU-05` |
| `NFR-005` | Izin **dan** kecocokan dokter yang ditetapkan | `BE-MCU-024`, `030`, `031`, `032` | `AT-MCU-31`, `39` |
| `NFR-006` | Log tanpa data sensitif | `BE-MCU-035` (dan DoD setiap task) | `AT-MCU-45` |
| `NFR-007` | Tanggal kalender WIB, jam tutup dapat dikonfigurasi | `BE-MCU-012`, `013` | `AT-MCU-13`; `VAL-MCU-24` |
| `NFR-008` | Isian terkunci sesudah approve; revisi tercatat | `BE-MCU-023`, `031` | `AT-MCU-43`; `VAL-MCU-44` |

## 3. Uji penerimaan → Task

| Uji | Task | | Uji | Task |
|---|---|---|---|---|
| `AT-MCU-01`..`03` | `BE-MCU-005` | | `AT-MCU-26`, `27` | `BE-MCU-021` |
| `AT-MCU-04`, `05` | `BE-MCU-007`, `011` | | `AT-MCU-28` | `BE-MCU-020`, `021` |
| `AT-MCU-06` | `BE-MCU-007` | | `AT-MCU-29`, `07` | `BE-MCU-022` |
| `AT-MCU-08`..`10`, `14`, `15` | `BE-MCU-011` | | `AT-MCU-30`, `31`, `37` | `BE-MCU-024` |
| `AT-MCU-11` | — (`EPIC MCU-09`) | | `AT-MCU-32` | `BE-MCU-025` |
| `AT-MCU-12` | — (`EPIC MCU-07`) | | `AT-MCU-33`..`35` | `BE-MCU-023` |
| `AT-MCU-13` | `BE-MCU-012` | | `AT-MCU-36` | — (`EPIC MCU-10`) |
| `AT-MCU-16`..`24` | `BE-MCU-013` | | `AT-MCU-38`..`40` | `BE-MCU-030`, `032` |
| `AT-MCU-25` | — (`EPIC MCU-08`) | | `AT-MCU-41`..`44` | `BE-MCU-031`, `033`, `034` |
| `AT-MCU-RUN-01`..`03` | `BE-MCU-001` ✅ `PASS` 2026-10-21 | | `AT-MCU-45` | `BE-MCU-035` |

## 4. UAT → Task

| UAT | Task FE | Task BE |
|---|---|---|
| `UAT-01`, `02` | `FE-MCU-003` | `BE-MCU-005` |
| `UAT-03`, `04` | `FE-MCU-008` | `BE-MCU-011` |
| `UAT-05` | — (tanpa layar; diamati di `FE-MCU-007`) | `BE-MCU-012` |
| `UAT-06`..`08` | `FE-MCU-009` | `BE-MCU-013` |
| `UAT-09`, `10`, `12`, `13` | `FE-MCU-011` | `BE-MCU-020`..`023`, `025` |
| `UAT-11` | `FE-MCU-011` | `BE-MCU-024`, `026` |
| `UAT-14`, `16`, `17` | `FE-MCU-012` | `BE-MCU-030`, `031` |
| `UAT-15`, `18`, `19` | `FE-MCU-013` | `BE-MCU-030`, `033`, `034` |

## 5. Celah cakupan (coverage gap)

| ID | Celah | Akibat | Tindak lanjut |
|---|---|---|---|
| `GAP-MCU-01` | `AT-MCU-11`, `12`, `25`, `36` tidak punya task | Rekomendasi, impor, kiosk, dan sumber jasa medis tidak teruji di MVP | Sengaja — epic `POST-MVP`/menunggu koordinasi; dibawa ke roadmap revisi berikutnya |
| `GAP-MCU-02` | ~~`FR-MCU-034` tertahan Billing~~ — **ditutup 2026-10-21** | — | `MCU-COORD-003` disetujui Yasmina, pemilik `billing-kasir` |
| `GAP-MCU-03` | ~~Seluruh layar MCU tertahan `MCU-FE-002`~~ — **ditutup 2026-10-21** | — | `FE-MCU-000` ✅ — `05-screen-brief.md` |
| `GAP-MCU-04` | Master klinis usulan belum diratifikasi (`MCU-OQ-002`, `003`) | Development dan UAT boleh; **go-live tidak** | Jawaban dokter MCU dan pemilik privasi/legal sebelum go-live |
| `GAP-MCU-05` | `NFR-004` (audit) tidak punya uji penerimaan khusus | Riwayat bisa saja tidak lengkap pada satu tindakan tanpa ketahuan | Setiap task ubah mencantumkan baris riwayat yang terbentuk di laporan; `BE-MCU-035` memeriksa ulang daftar tindakan ubah |
| `GAP-MCU-06` | Aksesibilitas dan layar 1366 px (`03-frontend-architecture.md` §4) tidak punya uji | Kekurangan baru terlihat saat UAT | Diperiksa di DoD setiap task layar sesudah `FE-MCU-000` |
| `GAP-MCU-07` | Cacat refund ganda Billing (`MCU-EVD-002` §2.2) berlaku untuk **semua** refund, bukan hanya MCU | Satu baris tagihan dapat direfund melebihi nilainya hari ini | Penutupannya termasuk dalam persetujuan Yasmina 2026-10-21; ditutup oleh `BE-MCU-026` |
