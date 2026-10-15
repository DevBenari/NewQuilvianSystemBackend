# Roadmap Backend — Modul Gizi

Register task modul Gizi, disusun 15 Oktober 2026.

## Mengapa register ini baru ada sekarang

Modul Gizi dikerjakan tanpa roadmap task: yang terdokumentasi hanya keputusan
(`GIZ-DEC-001`…`014`), pertanyaan terbuka (`GIZ-OQ-001`…`007`), desain domain, dan
`MODULE-STATUS.md`. Akibatnya Gizi tidak pernah dapat dihitung seperti modul lain, dan hanya
terbaca sebagai estimasi source.

Register ini **tidak mengarang pekerjaan**. Setiap task diturunkan dari keputusan yang sudah
disahkan, berkas uji yang sudah ada, dan bukti runtime yang sudah dijalankan. Kolom *Diturunkan
dari* menyebut jangkarnya supaya tiap baris dapat ditelusuri.

| Hal | Isi |
|---|---|
| Cakupan | V1 modul Gizi |
| Uji | `Tests/QuilvianSystemBackend.NutritionTests` — **219 uji, 219 lulus** |
| Bukti runtime | 14 Oktober 2026, `localhost/QuilvianNewDevIkbalFr` |
| Register gabungan | [`../../MODULE-COMPLETION-NORMALIZATION.md`](../../MODULE-COMPLETION-NORMALIZATION.md) |

## Task

| Task ID | Outcome | Diturunkan dari | Status | Bukti |
|---|---|---|---|---|
| `GIZ-BE-001` | Order konsultasi gizi | `GIZ-DEC-001`, `GIZ-DEC-007` | **Selesai** | `OrderRuleTests` (24); runtime: order `GZ-1e2d56e6…` `200` |
| `GIZ-BE-002` | Catatan asuhan per kunjungan | `GIZ-DEC-005`, `GIZ-DEC-010` | **Selesai** | `OrderRuleTests`, `DiagnosisRuleTests`, `DietRuleTests`; runtime: BB/TB, asesmen, intervensi, recall, evaluasi `200` |
| `GIZ-BE-003` | Diagnosis gizi dari master berkode | `GIZ-DEC-011` (mengganti `GIZ-DEC-009`) | **Selesai** | `DiagnosisRuleTests` (10); runtime: `NI-1.4` primary + `NC-1.1` sekunder `200` |
| `GIZ-BE-004` | Kebutuhan nutrisi V1 lima parameter | `GIZ-DEC-012` | **Selesai** | `RequirementRuleTests` (19); runtime: gerbang *"seluruh parameter aktif wajib terisi"* terbukti |
| `GIZ-BE-005` | Revisi kebutuhan beralasan | `GIZ-DEC-012` | **Selesai** | `RequirementRuleTests`; runtime: revisi 2 `isCurrent=true`, revisi 1 `false`, alasan tersimpan |
| `GIZ-BE-006` | Diet pasien | `GIZ-DEC-013` (mengganti `GIZ-DEC-004`) | **Selesai** | `DietRuleTests` (21). Runtime menunggu kunjungan rawat inap — lihat catatan |
| `GIZ-BE-007` | Produksi dan distribusi makanan | `GIZ-DEC-013` | **Selesai** | `ProductionBatchRuleTests` (30). Runtime menunggu kunjungan rawat inap |
| `GIZ-BE-008` | Master gizi: jenis diet, bentuk makanan, jadwal makan | `GIZ-OQ-002` (ditutup `GIZ-DEC-011`) | **Selesai** | `MasterControllerTests` (26); runtime: ketiganya dibuat `200` |
| `GIZ-BE-009` | Laporan gizi | `MODULE-STATUS.md`, `03-desain-domain.md` | **Selesai** | `ReportRuleTests` (19); runtime: `summary`, `services`, `diets`, `requirements` `200` |
| `GIZ-BE-010` | Otorisasi seluruh controller Gizi | matriks izin modul | **Selesai** | `PermissionMatrixTests` (16); runtime: **`401`/`403`/`200` selektif terbukti** |
| `GIZ-BE-011` | Penutupan asuhan gizi | `GIZ-DEC-008` | **Selesai** | `OrderRuleTests`; runtime: order ditutup, status 2 → 3 |
| `GIZ-BE-012` | Verifikasi instruksi oleh dokter pemberi order | `GIZ-DEC-007` | **Selesai** | tercakup `OrderRuleTests`, `DietRuleTests`; runtime: `403` *"Hanya dokter yang memberi instruksi…"* |

**Rekap: 12 task, 12 selesai, 0 sedang proses, 0 blocked, 0 belum mulai — 100%.**

## Bukti otorisasi runtime (`GIZ-BE-010`)

Diambil 14 Oktober 2026 dalam satu sesi login yang sama, dengan
`Security:Authorization:Enabled=true` dan `EnforceClinicalPolicyForSuperAdmin=true` sehingga
bypass SuperAdmin mati.

| Endpoint | Permission | Tanpa login | Izin ditahan | Izin diberikan |
|---|---|---|---|---|
| `…/nutrition-management/orders` | `NutritionOrder:Read` | `401` | — | **`200`** |
| `…/orders/{id}/requirements` | `NutritionRequirement:Read` | `401` | **`403`** | — |
| `…/masters/diet-types` | `NutritionMaster:Read` | `401` | **`403`** | — |
| `…/reports/summary` | `NutritionReport:Read` | `401` | — | **`200`** |

Penegakannya di `Filters/AccessPermissionFilter.cs`, bukan logika controller.

## Yang tidak dihitung sebagai task

| Hal | Alasan |
|---|---|
| `GIZ-DEC-004` | **Superseded** `GIZ-DEC-013` — scope diperluas ke diet, produksi, distribusi |
| `GIZ-DEC-009` | **Superseded** `GIZ-DEC-011` — master diagnosis milik Gizi sendiri |
| `GIZ-OQ-007` | **DEFERRED** 25 Sep 2026: rumus kalkulasi tidak dibuat pada V1 atas keputusan pemilik proses. Nilai diinput ahli gizi; registry rumus berdiri dan kosong. Bukan coding yang belum selesai |
| `GIZ-OQ-003` | **Eksternal** — isi `MstProfession` untuk ahli gizi, milik Human Resource |
| `GIZ-OQ-005` | **Di luar V1** — interval skrining ulang; tertulis *"Tidak untuk versi pertama"* |
| Skrining gizi | **Bukan scope Gizi** — `GIZ-DEC-003` menetapkannya dikerjakan perawat sebelum order; nol endpoint Gizi |

## Catatan runtime diet dan produksi

`GIZ-BE-006` dan `GIZ-BE-007` berstatus selesai atas source lengkap dan 51 uji hijau. Runtime-nya
belum dijalankan karena `GIZ-DEC-002` menetapkan **V1 hanya melayani pasien rawat inap**,
sedangkan basis data pengembangan memuat 24 kunjungan dan seluruhnya rawat jalan. Ini
keterbatasan data uji, bukan cacat modul — perilakunya justru sesuai keputusan yang disahkan.

## Dependency eksternal

| Hal | Pemilik |
|---|---|
| Kunjungan rawat inap untuk menguji diet dan produksi | Rawat Inap |
| Akun dokter pemberi order untuk menguji verifikasi instruksi | akun/role |
| `GIZ-OQ-003` isi `MstProfession` | Human Resource |
