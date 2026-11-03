# Laporan Perubahan Frontend — `FE-HRD-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-025` |
| Judul | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-053`, `HRD-DEC-033`, `BE-HRD-026` |
| Contract version | Employee 360 API v1 (`HrdEmployee360Controller.cs`) |
| Dependency | `BE-HRD-026` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/employee-360/[id]/page.jsx`, `src/app/hr/employee-360/[id]/page.jsx`, `src/components/view/hr/workforce-core/employee-360-view.jsx`, `src/lib/services/hr/employee-360-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Rumah sakit membutuhkan satu layar terpadu 360 derajat untuk meninjau seluruh aspek staf medis dan karyawan (kedinasan, riwayat SK, sertifikasi STR/SIP, kompensasi, dan catatan disiplin), namun data konfidensial wajib dilindungi agar tidak bocor ke pihak yang tidak berhak.

Task ini mengimplementasikan layar **Employee 360 Dual-View**:
1. **Mode HR Administrator (Akses Penuh)**: Menampilkan seluruh tab informasi secara komprehensif, mencakup struktur gaji pokok, grade kompensasi, nomor rekening payroll, NIK KTP, tanggal lahir, susunan keluarga, dan riwayat tindakan disiplin.
2. **Mode Atasan Langsung (Akses Terbatas Kedinasan)**: Jika layar dibuka oleh manajer/kepala ruangan bawahan, sistem secara otomatis menyembunyikan informasi gaji, rekening, identitas sensitif, dan catatan sanksi, menggantinya dengan banner edukasi privasi (`HRD-DEC-053` & `HRD-DEC-033`). Atasan hanya berhak melihat unit penugasan, jabatan, atasan, histori SK, dan masa berlaku sertifikasi kompetensi.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-025: Employee 360 Dual-View Service dan View membatasi data sensitif (1.1043ms)
```
Status: **PASS (100%)**

---

## Pengerjaan Ulang — `PLAN-REPAIR-HRD-002` (3 November 2026)

| Field | Nilai |
| --- | --- |
| Pemicu | Laporan pemilik 03-11-2026 dan laporan live testing `LBT-WFC-04`, `-19`, `-23` |
| Issue / rencana | [issue-002](../../../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) · [plan-repair-002](../../../docs/plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md) — `FIX-HRD-002-05` |
| Branch frontend | `HamzahV2`, basis `66b7e5600`; perubahan belum di-commit |

### Keadaan yang ditemukan

Layar ditulis dengan 115 `className` Tailwind, tab berupa `<button>` mentah, riwayat SK berupa `<table>`
mentah, dan UUID atasan tampil sebagai "ID Atasan". Service memanggil
`/v1/corporate/human-resource/employee-360/{id}`, alamat yang tidak ada (404). `[Route]`
`HrdEmployee360Controller` yang benar adalah `…/workforce-profiles/{workforceProfileId}/employee-360`.
Test unit lama justru menuntut alamat yang salah.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/employee-360-view.jsx` | `BaseDetailView` (kartu profil + identitas), tab dari `BaseButton role="tab"`, `BaseDetailSection` per tab, `DataTable` riwayat SK, label tipe tenaga Bahasa Indonesia, tanpa UUID |
| `src/lib/services/hr/employee-360-service.js` | Alamat sesuai `[Route]` backend |
| `tests/unit/workforce-core-target-state.test.mjs` | Menuntut alamat `workforce-profiles/{id}/employee-360` dan menolak alamat lama |

### 3.4 Gerbang tampilan

**Modul referensi visual:** layar detail berbasis `BaseDetailView` (Bank Darah; Kiosk Device untuk aksi
hero kustom).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Halaman + kartu profil | `BaseDetailView` + `BaseDetailCard` (`showHeader`) | `REUSE` | — |
| Tab | `BaseButton` (`primary` aktif, `secondary` lainnya) dalam `.tabList` CSS Module | `COMPOSE` | Belum ada base component tab halaman |
| Isi tab | `BaseDetailSection` + grid info CSS Module | `COMPOSE` | — |
| Riwayat SK | `DataTable` (`pagination={false}`) | `REUSE` | — |
| Data rahasia mode atasan | `InformationAlert warning` | `REUSE` | — |

`UI GATE: PASS — 3 elemen REUSE, 2 COMPOSE, 0 NEW.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view Administrasi Kepegawaian>
OK — 7 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Employee 360 mode HR (data nyata) | `11-employee360-kedinasan.png`, `12`–`14`, `26-employee360-tipe-tenaga.png` | `GET 200 …/workforce-profiles/{id}/employee-360` |
| Mode Atasan Langsung (respons dicegat) | `15-employee360-mode-atasan-finansial.png` | Data rahasia diganti peringatan "Akses dibatasi" |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` | 0 error | `PASS` |
| Test unit `FE-HRD-025` (alamat baru) | Lulus | `PASS` |
| `npm run build` | Rute `/hr/workforce-core/employee-360/[id]` dan alias `/hr/employee-360/[id]` terbangun | `PASS` |
| Buka profil pegawai nyata sebagai HR | Seluruh tab tampil, tidak ada UUID, tipe tenaga "Karyawan" | `PASS` |

Uji manual: `PASS` — Chrome headed pada build produksi port 3001 (lihat catatan dev server pada
`FE-HRD-022`). Mode Atasan Langsung dengan **akun atasan sungguhan** belum diuji karena password akun
atasan uji tidak diketahui (laporan live testing bagian 7). Yang diuji adalah cara layar menampilkan
respons `DirectManager`, lewat respons yang dicegat di peramban.

### Status task

✅ tetap berlaku untuk lingkup layar. Penyaringan data rahasia ditegakkan backend (`BE-HRD-026`).
