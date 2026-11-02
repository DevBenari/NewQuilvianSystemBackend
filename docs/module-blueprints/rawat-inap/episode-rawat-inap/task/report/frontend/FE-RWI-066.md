# Laporan Perubahan Frontend — `FE-RWI-066`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-066` |
| Judul | Pesanan tertagih yang tidak dibatalkan punya tempat untuk ditindaklanjuti |
| Slice | Gelombang 1 — `PRD-RWI-V2-001`, `EPIC RI-41` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) bagian 4, kartu `FE-RWI-066` |
| Trace | `FR-RI-199`; `RWI-DEC-143` (c), `RWI-DEC-216`; `contracts/api-contract.md` `0.9.0` bagian 10.4; `03-frontend-architecture.md` 12.3.4; `data/data-dictionary.md` 18.5 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Wewenang UI | `FE-INP-24` — daftar baru pada Daftar Pantau Rawat Inap `FE-INP-09` |
| Dependency | `BE-RWI-083` [BE] ✅ selesai 17 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 6, berkas diubah 5 (source frontend) + 1 berkas test unit baru, kontrak API 1, UI/workflow 1 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Branch frontend | `HamzahV2` |
| Branch backend | `MHamzah` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ **Selesai 22 Oktober 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-4) terbukti pada source code, komponen visual, dan suite unit test otomatis (`37/37 passing`). `npx eslint` bersih (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Ketika episode rawat inap ditutup secara resmi, sistem melakukan pembatalan otomatis terhadap pesanan tindakan medis yang tertunda dan belum ditagih (`PendingProcedureOrders`). Namun, untuk pesanan tindakan medis yang **sudah ditagih ke pasien** (`IsBillingGenerated = true`), pembatalan otomatis **tidak boleh dilakukan** karena akan menghapus dasar pencatatan keuangan/tagihan kasir tanpa keputusan administratif (`RWI-DEC-143` jalur tidak normal c).

Sebelum task ini diselesaikan:
1. Pesanan tindakan yang tertunda dan sudah ditagih tersebut dibiarkan tetap ada di database backend, tetapi **tidak memiliki wadah antarmuka visual** untuk dipantau.
2. Tanpa layar daftar pantau, staf rumah sakit (supervisor ruangan, admisi, maupun petugas billing) tidak mengetahui bahwa ada tindakan yang belum pernah dilaksanakan padahal tagihannya sudah masuk ke rincian invoice pasien yang telah pulang.
3. Potensi sengketa atau keluhan pasien pasca-pulang menjadi tinggi karena tagihan tindakan tidak terlacak penyelesaiannya.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Supervisor Ruangan, Petugas Admisi, atau Staf Kasir/Billing yang memiliki hak akses membaca pantauan rawat inap (`InpatientMonitoring : Read`).

**Kapan layar dibuka.** Secara berkala saat meninjau operasional harian rawat inap pada menu **Daftar Pantau Rawat Inap (`FE-INP-09`)**.

### Langkah Berurutan — Supervisor Menindaklanjuti Pesanan Tertunda Tertagih:

1. **Membuka Menu Daftar Pantau:**
   Petugas membuka menu `Pelayanan Kesehatan → Rawat Inap → Daftar Pantau` (`FE-INP-09`).
2. **Memilih Tab "Pesanan Tertagih":**
   Pada kelompok kategori tab episode rawat inap, petugas melihat tab baru bertuliskan **"Pesanan Tertagih"** lengkap dengan badge jumlah data (count pill).
3. **Membaca Informasi Transparan:**
   Tabel menyajikan daftar pesanan tindakan yang belum terlaksana pada episode yang sudah ditutup, memuat:
   - Identitas pasien (Nama & No. Rekam Medis).
   - Episode rawat inap terkait dan tanggal penutupan episode (`episodeClosedAt`).
   - Rincian tindakan (Nama tindakan, kode tindakan, dokter pemesan, dan waktu order).
   - Lokasi bangsal / unit layanan asal.
   - Status tagihan bertanda *"Tertagih (Belum Dikerjakan)"* beserta waktu pembentukan butir tagihan kasir (`billingGeneratedAt`) dan ID butir tagihan.
4. **Membuka Detail Episode untuk Investigasi (AC-4):**
   Petugas menekan tombol **"Detail Episode"** pada kolom Tindak Lanjut. Sistem mengarahkan petugas langsung ke layar Detail Episode pasien terkait (`/health-services/inpatient-management/episodes/[id]`) untuk berkoordinasi dengan DPJP atau unit penunjang.
5. **Kondisi Daftar Kosong (AC-3):**
   Bila seluruh tindakan tertagih telah selesai dilaksanakan atau diselesaikan pembukuannya, layar menampilkan pesan keadaan kosong yang wajar (*"Tidak ada pesanan tindakan tertagih yang tertunda. Seluruh pesanan tindakan pada episode yang ditutup telah diselesaikan atau dibatalkan."*), bukan kondisi galat (*error*).

> **Batasan Wewenang Layar (`04-prd-to-mvp.md` 22.7 nomor 1):**
> Layar ini **menampilkan**, bukan menyelesaikan. Tindak lanjut administratif atas tagihan tersebut (misal refund, pembatalan invoice kasir, atau rescheduling) merupakan domain produk bersama modul Billing dan berada di luar wewenang task ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Jenis | Perubahan |
| --- | --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx` | Source Frontend | Menambahkan kunci list `BILLED_PENDING_PROCEDURE_ORDER: "billed-pending-procedure-orders"` pada `INPATIENT_MONITORING_LIST_KEYS`. Mendaftarkan entri konfigurasi tab di `INPATIENT_MONITORING_LISTS` pada urutan ketiga kelompok episode rawat inap sesuai `RWI-DEC-216`. |
| `src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx` | Source Frontend | Menambahkan normalizer `normalizeBilledPendingProcedureOrderItem` dan `normalizeBilledPendingProcedureOrderList`. Memperbarui `buildMonitoringQuery` untuk mendukung filter `closedFrom` dan `closedTo`. Memperbarui `getMonitoringRowKey` untuk memprioritaskan `procedureId`. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-monitoring.jsx` | Source Frontend | Mendaftarkan normalizer `normalizeBilledPendingProcedureOrderList` pada `NORMALIZERS` dan pesan galat pada `ERROR_FALLBACKS`. |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-table-columns.jsx` | Source Frontend | Menambahkan fungsi `buildBilledPendingProcedureOrderColumns` yang menyusun 7 kolom: No, Pasien, Episode, Pesanan Tindakan, Lokasi, Status Tagihan, dan Tindak Lanjut (tombol `DetailAction` dengan tautan ke detail episode). |
| `src/components/view/health-services/inpatient-management/monitoring/components/monitoring-tabs.jsx` | Source Frontend | Menambahkan label Bahasa Indonesia `"billed-pending-procedure-orders": "Pesanan Tertagih"` pada `TAB_LABELS`. |
| `tests/unit/inpatient-monitoring-billed-orders.test.mjs` | Test Unit | Membuat suite pengujian unit native `node --test` untuk memvalidasi AC-1 s.d. AC-4, query filter, penanganan data kosong, dan tautan aksi detail episode. |
| `tests/unit/inpatient-monitoring.test.mjs` | Test Unit | Menyelaraskan array ekspektasi kunci monitoring kriteria 1 dengan menyertakan `billed-pending-procedure-orders`. |

---

## 4. Bukti Verifikasi dan Pengujian

### 4.1 Verifikasi Unit Test Otomatis (`node --test`)

```bash
node --test tests/unit/inpatient-monitoring-billed-orders.test.mjs tests/unit/inpatient-monitoring.test.mjs
```

**Hasil:**
```text
✔ FE-RWI-066 AC-1: Normalisasi pesanan tindakan tertagih memetakan seluruh field data riil (1.0362ms)
✔ FE-RWI-066 AC-1: Normalisasi paged result menangani paging dan array items (0.2554ms)
✔ FE-RWI-066 AC-2: Urutan tab pada FE-INP-09 menempatkan FE-INP-24 sesuai ketetapan RWI-DEC-216 (0.2159ms)
✔ FE-RWI-066 AC-3: Jawaban daftar kosong menghasilkan empty state yang wajar tanpa galat (0.7027ms)
✔ FE-RWI-066 AC-4: Kolom tabel FE-INP-24 menyediakan kolom lengkap dan tautan aksi ke Detail Episode (5.552ms)
✔ FE-RWI-066 Query: buildMonitoringQuery menyertakan filter serviceUnitId dan closedFrom/closedTo (0.3762ms)
... [seluruh 37 test suite monitoring lulus 100%]
ℹ tests 37
ℹ suites 0
ℹ pass 37
ℹ fail 0
ℹ duration_ms 201.1003
```

Pengujian gabungan monitoring dan closure (71 tests): **71/71 PASS (0 gagal)**.

### 4.2 Verifikasi Linter (`eslint`)

```bash
npx eslint src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx src/lib/hooks/health-services/inpatient-management/use-inpatient-monitoring.jsx src/components/view/health-services/inpatient-management/inpatient-monitoring-table-columns.jsx src/components/view/health-services/inpatient-management/monitoring/components/monitoring-tabs.jsx src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx
```
**Hasil:** Bersih (`Exit code 0`, 0 error, 0 warning).

---

## 5. Pemenuhan Acceptance Criteria & Definition of Done

| Acceptance Criteria | Status | Bukti di Kode |
| --- | :---: | --- |
| **AC-1:** Daftar memuat persis pesanan tertagih yang tidak dibatalkan saat penutupan. | ✅ LULUS | Mengonsumsi endpoint `GET /monitoring/billed-pending-procedure-orders` (`BE-RWI-083`), dinormalisasi via `normalizeBilledPendingProcedureOrderList` mencakup `procedureId`, `procedureName`, `procedureCode`, `billingItemId`, `billingGeneratedAt`, dan `episodeClosedAt`. |
| **AC-2:** Letaknya pada `FE-INP-09` sesuai urutan yang ditetapkan, bukan disisipkan sembarang. | ✅ LULUS | Ditempatkan pada `INPATIENT_MONITORING_LISTS` sesuai urutan `RWI-DEC-216`: di dalam kelompok episode rawat inap sesudah `closures-without-financial-clearance` dan sebelum `departures-before-clearance`. |
| **AC-3:** Daftar kosong menampilkan keadaan kosong yang wajar, bukan galat. | ✅ LULUS | Bila items kosong, `totalData: 0` dikembalikan tanpa throw, memicu `emptyTitle: "Tidak ada pesanan tindakan tertagih yang tertunda."` dan deskripsi yang wajar. |
| **AC-4:** Setiap baris membuka episode yang bersangkutan. | ✅ LULUS | Kolom aksi menyematkan `<DetailAction item={item} />` yang membuka route `/health-services/inpatient-management/episodes/[id]`. |

---

## 6. Kesimpulan

Task `FE-RWI-066` (`FE-INP-24`) telah **selesai 100%**. Seluruh task pada **Gelombang 1 Roadmap V2 (`frontend-roadmap-v2.md`)** kini telah rampung secara penuh (`FE-RWI-063`, `FE-RWI-064`, `FE-RWI-065`, dan `FE-RWI-066`).
