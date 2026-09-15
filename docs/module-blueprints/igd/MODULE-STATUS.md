# IGD — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Module name | `IGD` / `EmergencyInstallationManagement` |
| Revision | `6` — `draft`. Irisan kontrak yang dibutuhkan `MVP-0`…`MVP-6` sudah `approved` lewat `IGD-DEC-093` dan `IGD-DEC-108`; blueprint secara keseluruhan belum disetujui |
| Module status | `PARTIAL` — pekerjaan berarti masih dapat berjalan (lihat *Next recommended task*), sementara `MVP-6` terblokir |
| Current phase | Gelombang `MVP-3`, `MVP-4`, dan `MVP-5` sedang dituntaskan. Modul IGD memakai penomoran gelombang `MVP-0`…`MVP-6`, bukan ID `IGD-PH-*` |
| Last verified at | `15 September 2026` — pemetaan ulang acceptance criteria ke source. **Tanpa** build, test, maupun query basis data. Bukti: [evidence/2026-09-15-pemeriksaan-status.md](evidence/2026-09-15-pemeriksaan-status.md) |
| Backend source SHA | `e89907c5` (branch `rizkiG`) — tempat pemeriksaan. SHA desain revisi 6 tetap `300922c` |
| Frontend source SHA | `43adae648` (branch `RizkiV2`) — tempat pemeriksaan. SHA desain revisi 6 tetap `96a91201` |

Dokumen ini ringkasan keadaan. Sumber kebenaran status per task tetap
[roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) dan
[roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md).

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `MVP-1`, `MVP-2`, R3.7 | `MVP-0`, `MVP-3`, `MVP-4`, `MVP-5` | `MVP-6` |

| Gelombang | Isi | Status | Yang menahan |
| --- | --- | --- | --- |
| `MVP-0` | Status kunjungan tidak dapat mundur (`BE-IGD-017`…`022`) | `IN_PROGRESS` 🟡 | `BE-IGD-017` tidak punya laporan tracked |
| `MVP-1` | Pendaftaran & encounter `Emergency` (`BE-IGD-023`, `024`) | `DONE` ✅ | — |
| `MVP-2` | Satu pasien satu episode (`BE-IGD-025`) | `DONE` ✅ | — |
| `MVP-3` | Pengkajian IGD tanpa antrean (`BE-IGD-026`…`030`) | `IN_PROGRESS` 🟡 | `BE-IGD-026`: uji langkah mundur migration belum |
| `MVP-4` | Kepergian pasien (`BE-IGD-031`…`034`) | `IN_PROGRESS` 🟡 | `BE-IGD-031`: uji `RENAME` balik belum |
| `MVP-5` | Riwayat dokter & serah terima (`BE-IGD-035`; `EPIC IGD-04`) | `IN_PROGRESS` 🟡 | `BE-IGD-035` kriteria 2; `EPIC IGD-04` belum punya task (`IGD-DEC-114`) |
| R3.7 | Migration, master data pindah modul, kolom respons (`BE-IGD-036`…`038`) | `DONE` ✅ | — |
| `MVP-6` | Kewenangan unit (`BE-IGD-039`) | `BLOCKED` ⛔ | Security/Privacy owner; pemetaan unit 0 dari 18 |

`DONE` di sini berarti seluruh task gelombangnya ✅ menurut roadmap. Status UAT **terpisah** dan
**tidak** ditulis lulus.

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — 18 ✅, 4 🟡, 1 ⛔ dari 23 task | `IN_PROGRESS` — 5 ✅, 5 🟡, 1 belum dikerjakan dari 11 task | `PARTIAL` — radiologi ditahan `IGD-DEC-111`; laboratorium tersambung tetapi tab-nya cacat (`IGD-EV-112`); billing IGD belum direncanakan | `NOT_STARTED` untuk uji lewat layar; alur simpan lewat layar belum pernah dijalankan sejak roadmap revision `1` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `BE-IGD-039` | Kewenangan unit membandingkan `DepartmentId` dengan `OrganizationUnitId` — tidak pernah benar | Security/Privacy owner (belum ditunjuk) | `MVP-6`; route tulis `order-items`, `arrive`, `accept-handover` | Ya — seluruh gelombang lain dan layar resusitasi tidak tertahan |
| `IGD-DEC-092` (sementara) | Fail-closed + jalan keluar beralasan; jalan keluarnya belum ada di kode | Security/Privacy owner | `MVP-6` | Ya |
| Pemetaan unit | `MstServiceUnit.OrganizationUnitId` 0 dari 18 unit terisi | Master Data (belum ditunjuk) | `MVP-6` | Ya |
| `ActAsRadiologist` | Hasil bacaan radiologi belum dapat dirilis siapa pun (`FE-RAD-11`) | Yoga Aji Pratama — pemilik Radiologi | Penyambungan pemesanan radiologi IGD (`IGD-DEC-111`) | Ya — perbaikan teks layar tidak menunggu |
| `IGD-DEC-100`…`102` | Sikap pesanan, pesanan lab manual, penerimaan per pesanan masih `draft` | Clinical Governance, Nursing authority, pemilik Laboratorium | Butir 10 DoD `EPIC IGD-07` | Ya |
| `IGD-OQ-083` | Tempat menyimpan alasan pembatalan observasi | Product/Domain Owner IGD | Bagian `Cancelled` dari `IGD-DEC-115` | Ya — bagian `Completed` tidak tertahan |
| `EPIC IGD-04` | Riwayat penugasan dokter belum punya task | `plan-module-delivery` atas persetujuan Rizki | `MVP-5` | Ya |

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` revision `3` + suplemen `3.1` | `f69e9e48` / `300922c` | `e89907c5` | Wajib sebelum gelombang berikutnya menyentuh modul lain; tim Registrasi, Laboratorium, dan Radiologi mengubah source sejak itu |
| `blueprint-manifest.md` `artifact_hashes` | 24 Agustus 2026 | — | Dihitung ulang pada pass desain berikutnya |
| Angka test pada laporan `BE-IGD-018`…`038` | `761 total, 759 lulus` (27 Agt) | Proyek test dihapus 11 Sep | Tidak dapat diulang; sah sebagai bukti historis (`IGD-DEC-110`) |
| Penerapan migration `20260910031500` (rename encounter/resep, milik tim Registrasi) | — | — | Belum diketahui apakah sudah diterapkan ke basis data; Rizki memastikannya sendiri |

## Next recommended task

Urutan yang tidak menunggu pihak lain. ID task masih usulan dan ditetapkan lewat
`plan-module-delivery`.

1. **Perbaikan tab Penunjang Medis** — kirim `encounterId` dan paging ke `lab-orders`, dan ganti
   teks *"modul Radiologi belum ada"* (`IGD-EV-112`, `IGD-EV-115`, `IGD-DEC-111`). Usulan
   `FE-IGD-023`.
2. **Catatan penutupan observasi** — `Completed` menulis `CompletionSummary` (`IGD-DEC-115`).
   Usulan `BE-IGD-040`.
3. **Pesan penolakan penutupan menyebut pesanan** — sambungkan
   `ValidatePesananSebelumPenutupanAsync` (`IGD-EV-122`), membuka `BE-IGD-035` kriteria 2.
4. **Pencabutan `Outpatient`** setelah Rizki memastikan jumlah baris (`IGD-EV-121`).
5. **Laporan tracked susulan** untuk `BE-IGD-017`, `f76ebaab`, `bd1d94a8a` (termasuk temuan
   privasi `IGD-EV-117`), dan `c8613d88c`.
6. **Uji langkah mundur migration** `20260826090500` di basis data terpisah — membuka
   `BE-IGD-026` dan `BE-IGD-031`. Butuh basis data terpisah milik Rizki.

## Optional deterministic delivery progress

| Lapisan | Rumus | Hasil |
| --- | --- | --- |
| Backend | task ✅ / seluruh task roadmap | **18 / 23 = 78%**. Tidak dikecualikan: `BE-IGD-039` (⛔) tetap dihitung di penyebut |
| Frontend | task ✅ / seluruh task roadmap | **5 / 11 = 45%**. `FE-IGD-019` tidak dihitung karena belum punya kartu roadmap |

`EPIC IGD-04` dan tiga area R3.5 (penunjang medis, pemakaian alat, billing IGD) **tidak** masuk
penyebut karena belum punya task. Persentase ini bukan ukuran kesiapan produksi.

## Status contract

`DRAFT` has identity but incomplete intake. `DISCOVERY` is collecting decisions/evidence. `READY` means planned phases may start. `PARTIAL` means at least one phase is ready while another is blocked or unknown. `BLOCKED` means no material phase can safely proceed. `IN_PROGRESS` has authorized active work. `VERIFYING` awaits readiness evidence. `DONE` requires appropriate verification evidence. `SUPERSEDED` records the successor blueprint.

Phase statuses are `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A phase becomes `DONE` only when its acceptance/readiness evidence is recorded; file existence is insufficient.
