# Laporan Perubahan Frontend — `FE-HRD-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-026` |
| Judul | Layar Batch Inisialisasi Penempatan Awal Pegawai (55 Pegawai Eksisting) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-054`, `BE-HRD-027` |
| Contract version | Initial Placement Batch API v1 (`HrdInitialPlacementBatchController.cs`) |
| Dependency | `BE-HRD-027` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/initial-batches/page.jsx`, `src/app/hr/initial-placement-batch/page.jsx`, `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`, `src/lib/services/hr/initial-placement-batch-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | 🟡 Sebagian — dikoreksi 3 November 2026 (lihat "Pengerjaan Ulang — PLAN-REPAIR-HRD-002"); semula Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Saat sistem HR Quilvian beralih ke tata kelola terstruktur, terdapat 55 staf eksisting rumah sakit yang belum memiliki data penempatan organisasi, jabatan, atau relasi atasan resmi. Memasukkan satu per satu via form SK kepegawaian akan sangat memakan waktu.

Task ini menyediakan antarmuka **Batch Inisialisasi Penempatan Awal**:
1. **Manajemen Wadah Batch**: Petugas HR dapat melihat daftar batch inisialisasi atau membuat batch baru.
2. **Validasi Otomatis (Pre-Flight Integrity Check)**: Tombol validasi memeriksa apakah seluruh 55 baris usulan penempatan memiliki unit kerja, departemen, dan formasi jabatan yang valid di master data serta tidak ada duplikasi penugasan primer.
3. **Penerapan Serentak (Batch Apply)**: Setelah status tervalidasi siap (*Validated*), HR dapat mengesahkan seluruh penempatan awal secara serentak dalam satu transaksi database atomik (`HRD-DEC-054`).

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-026: Initial Placement Batch Service dan View tersedia (0.3755ms)
```
Status: **PASS (100%)**

---

## Pengerjaan Ulang — `PLAN-REPAIR-HRD-002` (3 November 2026)

| Field | Nilai |
| --- | --- |
| Pemicu | Laporan pemilik 03-11-2026 (lampiran 1) dan laporan live testing `LBT-WFC-03`, `-19`, `-23` |
| Issue / rencana | [issue-002](../../../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) · [plan-repair-002](../../../docs/plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md) — `FIX-HRD-002-06` (tampilan); `FIX-HRD-002-09` (kontrak, ⛔) |
| Branch frontend | `HamzahV2`, basis `66b7e5600`; perubahan belum di-commit |

### Keadaan yang ditemukan

Layar ditulis dengan 43 `className` Tailwind, tombol pemilih batch berupa `<button>` mentah, tabel
berupa `<table>` mentah, dan dialog Buat Batch berupa overlay Tailwind yang tampil menempel di halaman.
Galat 404 tampil sebagai "Request failed with status code 404". Status batch memakai nilai yang tidak
dikirim backend (`Applied`, `ValidationFailed`), dan baris memakai `rowNumber`/`validationStatus` yang
tidak ada di DTO. Isian "Catatan" di dialog tidak pernah disimpan backend. Service memanggil
`/initial-placement-batches` dan aksi `/{id}/validate`, `/{id}/apply` yang tidak ada di backend.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` | Shell standar, `FilterSelect` pemilih batch, `SummaryGrid`, `DataTable`, `ConfirmModal` untuk Buat Batch dan Eksekusi, field DTO yang benar, isian Catatan dihapus, Eksekusi hanya untuk batch Validated tanpa baris gagal |

Service **tidak** diubah; penyambungan ke kontrak backend adalah `FIX-HRD-002-09`.

### 3.4 Gerbang tampilan

**Modul referensi visual:** layar rekap Administrasi Kepegawaian (shell dan tabel).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header + Buat Batch | `Hero` (`actions`) | `REUSE` | — |
| Pemilih batch | `FilterSelect` | `REUSE` | Menggantikan deretan tombol mentah |
| Ringkasan | `SummaryGrid` | `REUSE` | — |
| Panel batch aktif | `.panel` CSS Module domain + `StatusBadge` | `COMPOSE` | — |
| Baris batch | `DataTable` + kelas sel standar | `REUSE` | — |
| Dialog Buat Batch / Eksekusi | `ConfirmModal` (+ `BaseInputField` di `children`) | `REUSE` | — |
| Notifikasi | `ToastStack` | `REUSE` | — |

`UI GATE: PASS — 6 elemen REUSE, 1 COMPOSE, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view Administrasi Kepegawaian>
OK — 7 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Keadaan nyata (layanan belum tersambung) | `16-batch-nyata.png` | "Data atau layanan yang diminta tidak ditemukan." |
| Batch berisi (respons dicegat) | `17-batch-berisi.png`, `27-batch-baris-gagal.png` | Eksekusi mati bila ada baris gagal |
| Dialog Buat Batch | `18-batch-dialog-buat.png` | Dialog sungguhan dengan latar gelap |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` | 0 error | `PASS` |
| `npm run build` | Rute `/hr/workforce-core/initial-batches` terbangun | `PASS` |
| Batch Validated dengan 1 baris gagal | Tombol Eksekusi mati, catatan "1 baris belum valid" | `PASS` |

Uji manual: `PASS` untuk tampilan (Chrome headed, build produksi port 3001). Alur data nyata
`NOT FEASIBLE` — endpoint yang dipanggil service tidak ada di backend.

### Acceptance criteria (status sesudah pengerjaan ulang)

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Layar menampilkan 55 pegawai eksisting beserta usulan penempatan | Belum terpenuhi | Service belum memanggil `GET …/initial-batches/template` (`FIX-HRD-002-09`) |
| 2. Status validasi per baris; eksekusi mati bila ada baris tidak valid | Terpenuhi (tampilan) | `27-batch-baris-gagal.png` |
| 3. Progres dan hasil eksekusi tampil | Belum terpenuhi | Aksi eksekusi memanggil `/{id}/apply` yang tidak ada |
| 4. Batch `Executed` menjadi baca saja | Sebagian | Tombol Validasi/Eksekusi mati untuk `Executed`; belum terbukti dengan data nyata |

### Status task

🟡 **SEBAGIAN** — 1 dari 4 acceptance criteria terbukti. Status ✅ sebelumnya (2 November 2026) tidak
didukung bukti: layar tidak pernah dapat memuat data. Blocker: `FIX-HRD-002-09` ⛔ (menunggu K-02 dan
perbaikan backend LBT-WFC-09, LBT-WFC-10).
