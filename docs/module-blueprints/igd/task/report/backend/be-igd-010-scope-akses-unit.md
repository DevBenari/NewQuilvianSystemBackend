# Laporan Perubahan Backend — `BE-IGD-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-010` |
| Judul | Pemeriksaan akses mengenal unit pelayanan dan sumber daya |
| Slice | S4 — Hak akses sesuai kewenangan |
| Trace | `IGD-DEC-026`; `IGD-GAP-006`; permission matrix bagian 3 kebutuhan nomor 2; `AT-IGD-041` |
| Commit backend | `21c609f2853574532f74dd2b1489b8d2e502abd1` |
| Tanggal | 18 Agustus 2026 |
| **Status** | **Terhalang desain — hanya `AT-IGD-041` yang dapat ditegakkan** |

---

## 1. Temuan utama: relasi pengguna ke unit pelayanan tidak ada

Kriteria 1 dan 3 pada roadmap menuntut pemeriksaan akses mengenal unit pelayanan, sehingga
petugas unit A ditolak saat menerima perpindahan ke unit B.

Penelusuran source membuktikan **tidak ada satu pun jalur data dari pengguna ke unit
pelayanan**:

| Yang diperiksa | Hasil |
| --- | --- |
| `MstServiceUnit` | Tidak punya `DepartmentId` maupun kolom penghubung ke organisasi |
| `ApplicationUserOrganization` | Hanya `UserId`, `DepartmentId`, `PositionId` |
| `MstDepartment` | Hanya punya koleksi `Positions`; tidak mengenal unit pelayanan |
| Pencarian `ServiceUnitId` pada `Models/` dan `Areas/Corporate/` | Nol hasil |
| Pencarian berkas `*ServiceUnit*` | Hanya model, DTO, controller, enum, dan konfigurasi master-nya sendiri; tidak ada tabel penghubung |

`SysAccessPolicy` menautkan kebijakan ke pasangan Department dan Position. Tidak ada dimensi
unit pelayanan di mana pun dalam rantai itu.

### 1.1 Mengapa ini tidak saya kerjakan sendiri

Menegakkan kriteria 3 menuntut relasi baru antara pengguna dan unit pelayanan. Itu berarti
tabel atau kolom baru, migration, dan — yang paling menentukan — **keputusan pengisian data
lama**: setiap penugasan organisasi yang sudah ada harus ditetapkan miliknya unit mana.

Menebak pemetaan itu berarti mengarang kewenangan akses. Pada modul yang menyimpan data klinis,
menebak ke arah lebih longgar membuka akses yang tidak sah, dan menebak ke arah lebih ketat
menghentikan pekerjaan sah petugas di tengah pelayanan darurat.

Roadmap sendiri mencatat `IGD-GAP-006` berstatus `Missing` untuk scope resource dan unit. Yang
dibutuhkan adalah `/design-business-module`, bukan pengetikan kode.

## 2. Yang tetap dikerjakan: `AT-IGD-041`

Satu bagian dari kebutuhan ini dapat ditegakkan tanpa relasi yang hilang, yaitu larangan pengaju
transfer menerima transfernya sendiri. Aturan itu hanya membandingkan pelaku dengan
`RequestedByUserId` pada transfer yang bersangkutan.

| File | Perubahan |
| --- | --- |
| `Controller/EmergencyTransferController.cs` | `PATCH /{id}/transfer-status` ke `Accepted` menolak 403 bila pelaku sama dengan pengaju |

Pesan dan kode statusnya mengikuti validation matrix bagian 4 apa adanya: 403 dengan pesan
"Perpindahan harus diterima oleh petugas unit tujuan."

Perlu jujur soal batasnya: pemeriksaan ini menutup kasus pengaju menerima transfernya sendiri,
**tetapi tidak** menutup kasus petugas unit lain yang bukan pengaju ikut menerima. Kasus kedua
menunggu relasi pengguna ke unit.

## 3. Yang sengaja tidak dikerjakan

**Parameter konteks pada `HasAccessAsync` tidak saya tambahkan.** Menambah parameter yang tidak
punya data untuk dievaluasi berarti menaruh kode mati di jalur otorisasi seluruh aplikasi —
menambah permukaan tinjauan tanpa menambah perlindungan, dan memberi kesan keliru bahwa scope
unit sudah tertangani.

Bentuk parameter itu sebaiknya ditentukan bersamaan dengan desain relasi pengguna ke unit,
supaya bentuknya cocok dengan data yang akhirnya tersedia.

## 4. Verifikasi

**Belum ada verifikasi berjalan.** Build tidak dijalankan; solution tidak memiliki test project.

| Kriteria | Status |
| --- | --- |
| 1. Pemeriksaan akses menerima unit pelayanan sebagai konteks | **Terhalang** — tidak ada relasi pengguna ke unit |
| 2. Endpoint tanpa konteks berperilaku persis seperti sekarang | **Terpenuhi secara struktur** — `AccessPermissionService` dan `AccessPermissionFilter` tidak disentuh sama sekali |
| 3. Petugas unit A ditolak saat menerima perpindahan ke unit B | **Terhalang** — alasan sama dengan kriteria 1 |
| 4. `AT-IGD-041` lulus | Ada di kode — **belum terbukti** |

Kriteria 2 justru terpenuhi paling kuat: karena berkas otorisasi tidak diubah satu baris pun,
tidak ada modul lain yang mungkin berubah perilakunya.

## 5. Langkah yang tepat berikutnya

1. `/design-business-module` bersama security/privacy owner untuk merancang relasi pengguna ke
   unit pelayanan, termasuk aturan pengisian data lama.
2. Setelah relasi itu ada, barulah parameter konteks pada `HasAccessAsync` dirancang dan
   kriteria 1 dan 3 dikerjakan sebagai task tersendiri.
3. `BE-IGD-011` dan `BE-IGD-012` bergantung pada task ini menurut roadmap, sehingga keduanya
   ikut tertahan sampai langkah 1 selesai.

## 6. Risiko tersisa

| No | Risiko | Penanganan |
| ---: | --- | --- |
| 1 | Kewenangan penerimaan transfer masih lebih longgar dari yang dirancang | Tercatat di sini dan pada traceability; jangan dianggap tertutup |
| 2 | `BE-IGD-012` dinyalakan sebelum scope unit ada | Roadmap sudah menahannya di belakang `BE-IGD-011`, yang juga tertahan |
| 3 | Dianggap selesai karena `AT-IGD-041` lulus | Kriteria 1 dan 3 ditulis terhalang secara eksplisit, bukan dihitung lulus |
