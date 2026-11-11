# Laporan Perubahan Frontend — `FE-RWI-285`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-285` |
| Judul | Ajukan Retur dari e-MAR |
| Slice | Repair 015 keperawatan; Slice 1 [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-015.md](../../../roadmap/roadmap-repair-015.md), kartu `FE-RWI-285` |
| Trace | `FIX-KEP-015-03` (`ISS-KEP-015-03`); keputusan K-04 (2026-11-02): perawat mengajukan, apoteker memeriksa, depo dipilih di form |
| Contract version | Source backend `53418d37`: `POST /drug-returns` (`CreateDrugReturnRequest`, termasuk `SourceDrugUsageId` wajib sejak `BUG-PHA-BE-003`, 2 Oktober 2026) dan `POST /drug-returns/{id}/submit` |
| Wewenang UI | Aksi "Ajukan Retur" per baris e-MAR; form memakai layar Retur Obat Farmasi yang ada |
| Dependency | Tidak ada task; API Farmasi sudah ada |
| Klasifikasi | `MEDIUM` — e-MAR, hook dan form Retur Obat Farmasi, satu resource select |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Tombol e-MAR di `66b7e5600`; pengajuan otomatis + kunci klik ganda di-commit pemilik `6053e347c`; isian Penyerahan Sumber dan perbaikan gerbang CSS masih di working tree |
| Commit backend yang dijadikan rujukan | `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai — AC 1, 2, 4, 5 dan sisi stok AC 3 terbukti di peramban; sisi tagihan AC 3 terbukti di source `BE-RWI-158`, tidak dapat dibuktikan di DB dev (alasan di bagian 6) |

---

## 1. Keadaan yang ditemukan di awal

Tombol "Ajukan Retur" per obat di e-MAR (Daftar Pemberian Obat) sudah ada dan membuka form Retur Obat
Farmasi dengan kunjungan, obat, dan alasan terisi. Verifikasi menemukan tiga celah:

| No | Celah | Akibat |
| ---: | --- | --- |
| 1 | Form hanya menyimpan **draf**; perawat harus membuka detail lalu menekan "Ajukan" lagi | Langkah 4 plan (create lalu submit) dan AC 1 ("mengirim → berstatus diajukan") belum terpenuhi |
| 2 | Kunci idempotensi dibuat ulang tiap klik, tanpa pengaman klik ganda di hook | AC 4 hanya bergantung pada tombol `disabled` |
| 3 | **Ditemukan saat uji langsung:** sejak `BUG-PHA-BE-003` (2 Okt 2026) backend mewajibkan `SourceDrugUsageId` (penyerahan obat asal), tetapi form Farmasi tidak pernah punya isiannya | **Semua** retur dari form ini — dari e-MAR maupun menu Farmasi — ditolak 422 `PHM080` "Penyerahan sumber wajib disebut" |

---

## 2. Proses bisnis dari sisi pengguna

| Unsur | Isi |
| --- | --- |
| Tujuan | Sisa obat bangsal kembali ke stok Farmasi, dan tagihan pasien terkoreksi setelah apoteker memeriksa |
| Pelaku | Perawat mengajukan; apoteker memeriksa (K-04) |
| Pemicu | Terapi dihentikan DPJP atau obat tidak jadi diberikan |

Langkah:

1. Perawat membuka Catatan Keperawatan → Daftar Pemberian Obat, lalu menekan "Ajukan Retur" pada baris obat.
2. Form "Ajukan Retur Obat" terbuka dengan kunjungan, obat, dan alasan terisi dari e-MAR.
3. Perawat memilih **Penyerahan Sumber** (daftar penyerahan obat pada kunjungan itu), lokasi penerima, petugas pengaju, batch, satuan, dan jumlah.
4. Perawat menekan **Kirim Retur**. Sistem membuat draf lalu langsung mengajukannya; layar pindah ke detail retur berstatus Diajukan.
5. Apoteker menekan **Periksa**, mengisi jumlah diterima dan keadaannya, lalu menyimpan. Stok bertambah.

Contoh dari uji: Tn. F (samaran) menerima 5 botol Paracetamol infus (penyerahan USG-20261109-9A494A). Perawat mengembalikan 3 botol. Sebelum diperiksa, stok batch tetap 45. Setelah apoteker menerima 3 botol "Siap Pakai", stok menjadi 48.

Jalur tidak normal:

| Keadaan | Hasil |
| --- | --- |
| Penyerahan sumber belum dipilih | Ditahan di layar: "Penyerahan sumber wajib dipilih." |
| Kunjungan diganti | Pilihan penyerahan sumber dikosongkan |
| Kunjungan belum punya penyerahan | Daftar menampilkan "Belum ada penyerahan obat pada kunjungan ini." |
| Pengajuan gagal setelah draf dibuat | Layar tetap pindah ke detail draf; tombol "Ajukan" di detail tersedia untuk mencoba lagi (draf tidak digandakan) |
| Perawat tanpa hak `DrugReturn : Create` dan `Update` | Tombol "Ajukan Retur" tidak tampil |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`medication-administration-chart-panel.jsx`, `drug-return-prefill-utils.js`, `use-drug-return.jsx`, `drug-return-form-view.jsx`, `stock-request-resource-select.jsx`, `health-service-select-resources.js`; backend `DrugReturnService.cs` (`EnsureReturnableAgainstSourceAsync`, PHM074–PHM081), `DrugReturnDtos.cs`, `DrugUsageController.cs`, `DrugUsageDtos.cs`; dokumen `pharmacy/bug-pha-be-003-retur-tanpa-batas-serah.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/pharmacy-management/use-drug-return.jsx` | Bila berasal dari e-MAR, draf langsung diajukan (`submitDrugReturn`); kunci `useRef` menolak kiriman kedua selama kiriman pertama berjalan; isian `sourceDrugUsageId` divalidasi wajib dan ikut dikirim |
| `src/components/view/health-services/pharmacy-management/drug-return-view/drug-return-form-view.jsx` | Isian "Penyerahan Sumber *" (dikosongkan bila kunjungan berganti); label tombol "Kirim Retur" bila dari e-MAR; teks informasi e-MAR diperbarui |
| `src/lib/hooks/select/health-service/health-service-select-resources.js` | Resource `drugUsages` → `GET /pharmacy-management/drug-usages` dengan saringan `encounterId`/`drugId` |
| `.../nursing-workspace/sections/medication/panels/medication-administration-chart-panel.jsx` | Gerbang CSS: empat `BaseButton variant="outline"` (tidak sah) → `secondary` |

**Catatan lintas modul.** Form dan hook Retur Obat milik modul Farmasi. Perubahannya membuat form mengikuti kontrak backend Farmasi yang sudah berlaku sejak 2 Oktober 2026, jadi memperbaiki form untuk petugas Farmasi juga. Pengajuan otomatis hanya aktif bila form dibuka dari e-MAR (`source=emar`). Dari menu Farmasi, perilakunya tetap simpan draf.

### 3.3 Kepatuhan arsitektur frontend

Pilihan penyerahan memakai `StockRequestResourceSelect` → `useSelectResource` → registry resource yang sama dengan isian lokasi, petugas, dan batch. Tidak ada service atau komponen baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** form Retur Obat Farmasi itu sendiri.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Isian Penyerahan Sumber | `StockRequestResourceSelect` (`FilterSelect`) | `REUSE` | Pola isian lain di form yang sama |
| Tombol Kirim Retur | `BaseButton` (label kondisional) | `REUSE` | — |
| Tombol tanggal e-MAR | `BaseButton variant="secondary"` | `REUSE` | Mengganti `outline` yang tidak sah |

`UI GATE: PASS — 3 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs ... drug-return-form-view.jsx medication-administration-chart-panel.jsx ...
OK — 6 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| e-MAR dengan tombol Ajukan Retur | `test-with-agy/screenshots/rawat-inap-mvp-1109/06-emar-retur/v3-farhan-dpo-atas.png` | Tombol tanggal kini `secondary` |
| Form terisi dari e-MAR | `.../06-emar-retur/paracetamol-2-lengkap.png` | Penyerahan Sumber USG-20261109-9A494A |
| Detail sesudah Kirim | `.../06-emar-retur/paracetamol-2-sesudah-kirim.png` | Status Diajukan, "Dari pemakaian USG-…" |
| Pemeriksaan apoteker | `.../06-emar-retur/periksa-lengkap.png`, `periksa-tersimpan.png` | Diterima 3, Siap Pakai |
| Penolakan 422 sebelum perbaikan | `.../06-emar-retur/paracetamol-sesudah-kirim.png` | PHM080 |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner pada `FilterSelect` dan tombol Kirim (`loading`) |
| Kosong | "Belum ada penyerahan obat pada kunjungan ini." |
| Gagal | `InformationAlert` "Retur tidak dapat disimpan" berisi pesan server |
| Tanpa hak akses | Tombol e-MAR tidak tampil; `AccessDeniedGate` pada form |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Pharmacy Management / Drug Return

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/pharmacy-management/drug-returns` | Membuat draf retur | `DrugReturn : Create` |
| `POST` | `/v1/health-services/pharmacy-management/drug-returns/{id}/submit` | Mengajukan draf (otomatis dari e-MAR) | `DrugReturn : Update` |

#### Health Services / Pharmacy Management / Drug Usage

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/pharmacy-management/drug-usages?encounterId=` | Daftar penyerahan sumber | `DrugUsage : Read` |

Kode 422 `PHM080` berarti penyerahan sumber belum disebut. `PHM081` berarti penyerahan bukan milik kunjungan ini atau belum terjadi.

---

## 6. Verifikasi

Data uji di DB dev (lewat endpoint resmi Farmasi, berlabel "Data uji MVP rawat inap 9 Nov 2026"): saldo awal batch `UJI-MVP-1109` Paracetamol infus 50 botol di FARMASI RAWAT INAP; penyerahan `USG-20261109-9A494A` 5 botol untuk Tn. F (stok → 45).

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors`, `npm run build` | exit 0 | `PASS` | Run MVP |
| `npm run test:unit` | 4144 lulus / 56 gagal lama; `pharmacy-screen-acceptance.test.mjs` 23/23 lulus | `EXISTING / ENVIRONMENT ISSUE` untuk kegagalan lama | Perbandingan worktree |
| Sebelum perbaikan: kirim retur | 422 `PHM080` — cacat lama form Farmasi sejak 2 Okt 2026, bukan akibat diff ini; diperbaiki pada task ini | `EXISTING / ENVIRONMENT ISSUE` (diperbaiki) | `paracetamol-sesudah-kirim.png` |
| AC 1 — kirim → berstatus diajukan | Satu klik "Kirim Retur" → `POST /drug-returns` 200 lalu `POST /{id}/submit` 200; `RTN-20261109-F80F01` status 2 (Diajukan) | `PASS` | Network log, DB |
| AC 2 — sebelum diperiksa stok belum bertambah | Stok tetap 45 | `PASS` | Query `PhmDrugStockBalance` |
| AC 3 — sesudah diperiksa stok +3 | 45 → 48; status 3 (Diperiksa) | `PASS` | `periksa-tersimpan.png`, DB |
| AC 3 — tagihan berkurang | `NOT FEASIBLE`: penyerahan jalur Pemakaian Obat berstatus "belum ditagih", dan status "Billed" di modul Farmasi belum dapat dicapai (menunggu kontrak integrasi). Pembatalan tagihan retur pada jalur resep rawat inap dimiliki `BE-RWI-158` (✅ di roadmap integrasi-billing) | `NOT RUN` | `DrugStockEnums.cs` `DrugUsageStatus` |
| AC 4 — klik ganda hanya satu retur | `dblclick` → satu `POST /drug-returns`; satu baris di `PhmDrugReturn` | `PASS` | Network log, DB |
| AC 5 — tanpa hak tidak melihat aksi | Positif: superadmin melihat tombol. Negatif: `canSubmitReturn` mensyaratkan `DrugReturn` Create **dan** Update `allowed` | `PASS` (positif di layar, negatif di source) | `medication-administration-chart-panel.jsx:37-44` |

`AUTOMATED TEST: npm run test:unit — FAIL (56 kegagalan lama di luar cakupan; 0 kegagalan baru)`

`MANUAL TEST: PASS` (kecuali sisi tagihan AC 3: `NOT FEASIBLE`)

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1 | Terpenuhi | Bagian 6 |
| AC 2 | Terpenuhi | Bagian 6 |
| AC 3 | Terpenuhi untuk stok; sisi tagihan hanya terpetakan ke source `BE-RWI-158` | Bagian 6 |
| AC 4 | Terpenuhi | Bagian 6 |
| AC 5 | Terpenuhi | Bagian 6 |
| DoD lint, unit, build, laporan | Terpenuhi | Bagian 6, berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Superadmin tidak tertaut ke profil petugas, sehingga data uji penyerahan memakai petugas pencatat Amanda Fitriani (akun uji HR) |
| Masalah yang diketahui | Revisi `keperawatan/05-skema-tampilan.md` untuk e-MAR (langkah 2 plan) masuk Slice 6 MVP |
| Dependency backend | Integrasi tagihan Pemakaian Obat ke Billing (status Billed) belum aktif — milik Farmasi/Billing |
| Perubahan sampingan | `NONE`. Data uji tetap tersimpan di DB dev: batch `UJI-MVP-1109`, penyerahan `USG-20261109-9A494A`, retur `RTN-20261109-F80F01` |
| Interupsi | `NONE` |
| Status Git | Berubah (belum di-commit): `use-drug-return.jsx`, `drug-return-form-view.jsx`, `health-service-select-resources.js`, `medication-administration-chart-panel.jsx` |
| Langkah berikutnya | Beri tahu pemilik Farmasi bahwa form Retur Obat kini mengirim penyerahan sumber |
