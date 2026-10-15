# Laporan Perubahan Frontend — `FE-BKC-FIX-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-012` |
| Judul | Keranjang Alkes/BMHP perawat memakai kontrak Drug Usage yang sah: draft → catat pemakaian → status tagih, tanpa resep pengganti |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-02`; temuan `ISS-BKC-001-02` |
| Area | Workspace Keperawatan Rawat Inap → Obat & Alkes → Order Alkes (laporan ditempatkan di billing-kasir karena register pemicunya di sini) |
| Dependency | `BE-BKC-FIX-017` (depo/petugas opsional, `chargeState` pada detail) — wajib terpasang bersama |
| Wewenang UI | `REUSE`: `BaseButton`, `ClinicalStateBoundary`, `ClinicalStatusBadge`, `StockRequestResourceSelect` (Farmasi), `usePermission`. Tidak ada base component baru |
| Target tulis | `QuilvianSystemFrontendDev` |
| Commit saat dikerjakan | `7d44526ff` (`HamzahV2`) + working tree; belum di-commit. Working tree juga memuat perubahan admisi/kelas perawatan milik pekerjaan lain |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**; lint 0 error; build lulus; uji unit baru lulus. Uji layar dengan backend berjalan `NOT RUN` |

## 1. Masalah

`order-alat-kesehatan-panel.jsx` mengirim `storageLocationId` dan `recordedByWorkforceId` berisi GUID kosong (dan `measurementId` kosong bila katalog tidak punya satuan), sehingga backend menolak draft. Saat gagal, panel diam-diam membuat **resep** pengganti tanpa `consultationId` dan dengan field yang tidak dikenal kontrak resep. Pesan sukses tidak membedakan draft, pemakaian tercatat, dan masuk invoice. Panel tidak pernah memanggil `recordDrugUsage`.

## 2. Alur sesudah perbaikan

```text
Katalog Alkes → Keranjang 3 spuit
Lokasi Stok: [ Otomatis: depo unit pasien ▾ ]        (opsional, master lokasi; tanpa input UUID)
[ 💾 Simpan Draft Order Alkes ]
  ✔ Draft pemakaian USG-… tersimpan — belum dicatat sebagai pemakaian. Stok belum berkurang dan belum ditagihkan. Depo: …
  ┌ Draft belum mengurangi stok dan belum ditagihkan. Catat pemakaian bila alat sudah benar-benar dipakai.  [ Catat Pemakaian ] ┐   ← hanya hak DrugUsage : Record
  └ (tanpa hak) Pencatatan pemakaian dilakukan petugas yang berwenang, lewat menu Pemakaian Obat Farmasi.
[ Catat Pemakaian ]
  ✔ Pemakaian USG-… tercatat (stok berkurang) — Masuk invoice. Pemakaian tercatat dan sudah masuk invoice.
```

| Keadaan | Perilaku |
| --- | --- |
| Gagal simpan draft | Pesan sebab dari server (`normalizeErrorMessage`); keranjang tetap; kirim ulang memakai kunci yang sama sehingga tidak menggandakan draft |
| Satuan katalog kosong | Ditolak di layar dengan nama item; tidak mengirim GUID kosong |
| Gagal catat (stok kurang, versi berubah) | Pesan sebab; kunci pencatatan dipertahankan untuk kirim ulang |
| Tanpa hak `Record` | Tombol tidak tampil; kalimat serah tugas ditampilkan |
| Keranjang/lokasi berubah | Kunci draft baru (draft berbeda) |

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/medication/panels/order-alat-kesehatan-panel.jsx` | Fallback `createPrescription` dan GUID kosong dihapus; payload draft hanya membawa lokasi bila dipilih; kunci idempoten stabil (`createKey`/`recordKey`); `handleRecordUsage` di balik `usePermission("DrugUsage","Record")`; kalimat hasil `describeUsageResult`; pilihan "Lokasi Stok" opsional; varian tombol `outline` (tidak dikenal `BaseButton`) diganti `secondary` |
| `src/lib/constants/health-services/pharmacy-management/drug-usage-constant.jsx` | `DRUG_USAGE_CHARGE_STATE_LABEL` |
| `tests/unit/nursing-alkes-checkout.test.mjs` | **Baru** — 4 uji kontrak source/label |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint <berkas yang diubah>` | exit 0, 0 error, 0 warning |
| `npx eslint src --quiet` | exit 0 (0 error). `npm run lint:errors` tidak dipakai — rusak oleh lingkungan (`test-with-agy`), `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build` | **Lulus** (exit 0, termasuk `postbuild` standalone) |
| `node --import ./tests/helpers/register.mjs --test tests/unit/nursing-alkes-checkout.test.mjs` (bersama lima berkas billing terkait) | 49 uji, **48 lulus**, 1 gagal: `FE-RWI-094` (mismatch lama switcher section tagihan perawat, sudah tercatat di issue bagian 6.1, bukan akibat task ini). Keempat uji baru lulus |
| Uji layar dengan backend `BE-BKC-FIX-017` berjalan (draft, catat, retry) | `NOT RUN` — backend lokal berjalan `--no-build` dengan DLL lama; dikecualikan sesuai keputusan pemilik bahwa uji manual bukan gerbang selesai |

## 5. Acceptance criteria (`FIX-BKC-001-02`)

| AC | Bukti |
| --- | --- |
| 1. Tiga spuit membuat draft dengan master sah, tanpa GUID kosong atau `consultationId` palsu | Payload tanpa GUID kosong; server menentukan depo/petugas (`BE-BKC-FIX-017`); uji AC1 |
| 2. Gagal tidak membuat resep alternatif; retry tidak menggandakan draft | Fallback dihapus; `createKey` stabil sampai berhasil; backend `DeterministicId(idempotencyKey)`; uji AC2 |
| 3. Draft tidak diklaim pemakaian/tagihan; Record hanya untuk pemegang hak | Kalimat hasil per keadaan; tombol di balik `usePermission`; uji AC3 |
