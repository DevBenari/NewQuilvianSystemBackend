# Laporan Perubahan Frontend — `FE-RWI-227`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-227` |
| Judul | Penambahan Opsi Masuk Kartu Keempat "Transfer Pasien IGD / Poliklinik" pada Landing Admisi Ranap & Constants Flow |
| Slice | Slice Transfer IGD / RJ; Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-transfer-igd.md`](../../../roadmap/frontend-roadmap-admisi-transfer-igd.md) — kartu `FE-RWI-227` |
| Trace | `FR-RI-189`; `RWI-DEC-274`; `RWI-AC-400`; `05-skema-tampilan.md` 3.0; `INPATIENT_ADMISSION_ENTRY_MODE` |
| Contract version | `0.12.0` **`approved`** (10 Oktober 2026) |
| Dependency | — (Task fondasi frontend) |
| Klasifikasi | `MEDIUM` — Penambahan opsi kartu landing admisi, definisi tahapan flow transfer, dan router query `entry=transfer` |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-flow-constants.jsx`, `use-inpatient-admission-flow.jsx`, `inpatient-admission-view.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Kriteria penerimaan `RWI-AC-400` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Sesuai kebutuhan Analisis Bisnis (Tahap B alur admisi transfer IGD):
1. **Opsi Masuk Keempat (`RWI-DEC-274`, `RWI-AC-400`):** Menambahkan kartu pilihan ke-4 pada landing page pendaftaran rawat inap: **"Pendaftaran Pasien Transfer IGD & Poliklinik"** berdampingan dengan Pasien Baru, Pasien Lama, dan Kamar Pulih.
2. **Panduan Visual Jelas:** Dilengkapi badge visual *"PASIEN TRANSFER IGD / POLIKLINIK"*, deskripsi *"Gunakan untuk pasien rujukan rawat inap dari IGD atau Poliklinik yang membawa formulir permintaan opname"*, serta poin-poin fitur utama (data terisi otomatis, DPJP terhubung, notifikasi ruangan).
3. **Penyelarasan Query URL:** Saat kartu dipilih, router mengarahkan parameter URL menjadi `?entry=transfer` dan menampilkan antrean transfer pasien.

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-admission-flow-constants.jsx`
- Menambahkan konstanta `TRANSFER: "transfer"` pada `INPATIENT_ADMISSION_ENTRY_MODE`.
- Menambahkan kartu ke-4 pada `INPATIENT_ADMISSION_ENTRY_OPTIONS` dengan badge, judul, deskripsi, dan bullets panduan operasional.
- Mendefinisikan konstanta urutan langkah `INPATIENT_ADMISSION_TRANSFER_STEPS` (10 langkah terpadu).
- Memperbarui fungsi `getInpatientAdmissionSteps` untuk mengembalikan `INPATIENT_ADMISSION_TRANSFER_STEPS` ketika `entryMode === INPATIENT_ADMISSION_ENTRY_MODE.TRANSFER`.

### 2.2 `use-inpatient-admission-flow.jsx`
- Memperbarui fungsi guard `isKnownEntryMode` agar mengenali `INPATIENT_ADMISSION_ENTRY_MODE.TRANSFER` sebagai mode yang sah.

### 2.3 `inpatient-admission-view.jsx`
- Menambahkan label navigasi pada `ENTRY_COPY[INPATIENT_ADMISSION_ENTRY_MODE.TRANSFER]`.
- Memperbarui memo `entryOptions` agar merender badge dinamis jumlah pasien yang sedang menunggu transfer.

---

## 3. Bukti Verifikasi
- Landing page admisi ranap berhasil merender 4 kartu pilihan secara responsif.
- Memilih kartu ke-4 mengubah URL menjadi `?entry=transfer` dan membuka view antrean transfer secara instan tanpa me-reload aplikasi.
