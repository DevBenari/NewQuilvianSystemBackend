# Laporan Perubahan Frontend — `FE-RWI-054`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-054` |
| **Judul** | Rencana Asuhan Keperawatan Rawat Inap (Pola Master-Detail, Penetapan Masalah & Tujuan, Evaluasi Asuhan, Version History vs Addendum, Validasi VAL-KEP-16, dan Mode Read-Only saat Episode Closed) |
| **Slice** | Gelombang `KEP-MVP-2` — Rencana Asuhan Keperawatan Terpadu |
| **Roadmap** | [`../../../roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian 3 task `FE-RWI-054` |
| **Trace** | `FE-KEP-04`; `FR-KEP-012` s.d. `FR-KEP-017`; `RWI-AC-177`, `RWI-DEC-091`, `INV-KEP-02`; `03-frontend-architecture.md` bagian 3.4; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 14, 15, 16, 24, 25, 27 |
| **Contract version** | API `0.3.0` grup Nursing Care Plan (`GET /episodes/{id}`, `POST /`, `POST /{id}/items`, `PUT /items/{id}`, `PATCH /items/{id}/evaluate`, `PATCH /items/{id}/close`, `GET /items/{id}/revisions`), state transition `0.3.0` bagian 2, validation `0.3.0` (`VAL-KEP-16`) |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../../../skema-tampilan-keperawatan-rawat-inap.md) Bagian 14 (Master-Detail), Bagian 15 (Riwayat Rencana Asuhan), Bagian 16 (Episode Closed) |
| **Dependency** | `FE-RWI-051` ✅, `BE-RWI-059` ✅, `BE-RWI-060` ✅ |
| **Klasifikasi** | `HEAVY` — Skor 9: 2 repository, 10+ berkas dibuat/diubah, tata letak Master-Detail 2-Panel, mesin Version History terpisah dari addendum rekam medis, 3 modal operasional klinis, penegakan batas validasi keselamatan |
| **Task mode** | `CROSS-REPO` sempit — source aplikasi dikerjakan di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Target tulis** | Frontend: `src/components/ui/clinical-workspace/**`, `src/lib/services/**`, `src/lib/hooks/**`, `src/components/view/health-services/inpatient-management/nursing-workspace/sections/care-plan/**`, `tests/unit/inpatient-nursing-care-plan.test.mjs`; Backend: `docs/module-blueprints/rawat-inap/keperawatan/task/report/frontend/FE-RWI-054.md`, `frontend-roadmap.md`, dan `requirement-traceability.md` |
| **Tanggal** | 8 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh Acceptance Criteria terbukti (7/7 fungsional & visual). Seluruh unit test lulus (7/7 pass spesifik care plan + 22/22 pass regresi keperawatan). ESLint dan build Next.js lulus 100%. |

---

## 1. Masalah yang Diselesaikan

Sebelum task ini diselesaikan:
1. **Pencatatan Rencana Asuhan di Kertas:** Masalah keperawatan, tujuan asuhan, dan rencana tindakan ditulis secara manual di lembaran kertas terpisah. Kertas tersebut tidak terbaca oleh perawat giliran berikutnya tanpa mencarinya terlebih dahulu, tidak terbaca oleh DPJP/dokter spesialis, dan berisiko tercecer saat pasien berpindah ruangan.
2. **Ketiadaan Pola Master-Detail:** Sulit bagi perawat untuk melihat daftar seluruh masalah keperawatan aktif pasien sambil menelaah rincian rencana tindakan dan catatan evaluasinya secara berdampingan.
3. **Pencampuradukan Konsep Riwayat Versi (*Version History*) vs Addendum Rekam Medis:** Perubahan rencana asuhan sering disamakan dengan koreksi salah tulis (addendum). Padahal sesuai `RWI-DEC-091`, perubahan rencana asuhan adalah **perkembangan klinis normal** (misalnya kondisi nyeri membaik dari skala 7 menjadi skala 3, sehingga target dan rencana disesuaikan), bukan pembetulan kesalahan masa lalu.
4. **Bahaya Kehilangan Penulis Asli Versi Masa Lalu:** Saat rencana asuhan diperbarui, sistem lama sering kali menimpa versi sebelumnya atau mencatat versi lama atas nama perawat pengubah baru. Akibatnya rekam medis kehilangan bukti otentik mengenai siapa yang pertama kali menetapkan masalah tersebut (`AC-CAP013-02`).
5. **Penutupan Masalah Tanpa Bukti Evaluasi (`VAL-KEP-16`):** Masalah keperawatan rawan ditutup sebagai "Teratasi" secara sepihak tanpa adanya satupun catatan evaluasi (SOAP/perkembangan) yang mendasarinya.
6. **Pelanggaran Status Episode Closed:** Saat pasien telah pulang atau episode rawat inap ditutup (`Closed`), form rencana asuhan sering kali masih menampilkan tombol mutasi/edit aktif yang dapat merusak integritas rekam medis final (`INV-KEP-02`).

Perubahan pada `FE-RWI-054` menyelesaikan seluruh permasalahan di atas secara terpadu melalui:
- Arsitektur Master-Detail 2-Panel: Panel kiri menyajikan daftar masalah keperawatan dengan filter chip status (`SEMUA`, `AKTIF`, `TERATASI`, `DIHENTIKAN`), dan panel kanan menyajikan rincian lengkap masalah, tujuan, rencana intervensi, catatan evaluasi terkini, dan riwayat perkembangan versi.
- Komponen dasar baru: `ClinicalRevisionHistory` dan `ClinicalRevisionItem` yang secara eksplisit memvisualisasikan rantai versi (`Versi X — CURRENT` vs `Versi X-1 DIARSIPKAN`) dengan mempertahankan perawat penulis dan waktu asli pada setiap versi (`OriginalAuthorEmployeeName`, `OriginalAuthoredAt`).
- Tiga dialog modal operasional:
  1. `CarePlanItemFormModal`: Form penambahan masalah baru atau pembaruan butir (menghasilkan versi baru).
  2. `EvaluateCarePlanModal`: Form pencatatan evaluasi hasil asuhan berkala.
  3. `CloseCarePlanModal`: Form penutupan masalah (`Resolved` vs `Discontinued`) dengan penegakan pencegahan penutupan teratasi tanpa catatan evaluasi (`VAL-KEP-16`).
- Penegakan mode hanya-baca permanen saat episode `Closed` (`AC-CAP013-03`), di mana seluruh tombol mutasi disembunyikan dan riwayat versi tetap terbaca penuh selamanya.

---

## 2. Proses Bisnis & Alur Pengguna

### 2.1 Alur Penetapan Masalah Keperawatan Baru (Happy Path)
1. **Pemicu:** Perawat membuka Ruang Kerja Keperawatan pasien rawat inap dan mengklik section **Rencana Asuhan**.
2. **Penyajian Master-Detail:**
   - Panel kiri memuat daftar masalah keperawatan pasien.
   - Panel kanan memuat rincian asuhan masalah yang sedang aktif dipilih.
3. **Penambahan Masalah Baru:**
   - Perawat menekan tombol `[+ Tambah Masalah Baru]`.
   - Muncul dialog modal `CarePlanItemFormModal`.
   - Perawat mengisi Masalah Keperawatan (`ProblemStatement`, wajib), Tujuan Asuhan (`GoalStatement`), Rencana Tindakan (`PlannedIntervention`), dan memilih Tautan Pengkajian Asal (`SourceAssessmentId`, opsional).
   - Perawat menekan tombol `[Tambah Masalah]`.
   - Data tersimpan via `POST /api/v1/health-services/clinical-management/nursing-care-plans/{id}/items` dan otomatis terpilih di panel kiri dengan nomor versi `v1` berstatus `● AKTIF`.

### 2.2 Alur Pembaruan Rencana Asuhan (Penaikan Versi)
1. Keadaan klinis pasien berkembang (misalnya skala nyeri menurun, atau target mobilisasi bertambah).
2. Perawat memilih butir masalah di panel kiri, lalu menekan tombol `[Ubah Rencana]` di panel kanan.
3. Dialog modal terbuka dalam mode edit dengan nomor versi berikutnya (misal: *Versi 2*).
4. Perawat menyesuaikan tujuan asuhan dan rencana tindakan baru, lalu menekan `[Simpan Versi Baru]`.
5. Sistem backend (`BE-RWI-060`) menyalin keadaan versi 1 ke tabel riwayat revisi beserta penulis aslinya, lalu memperbarui butir menjadi versi 2 atas nama perawat yang mengubah.
6. Panel kanan langsung memperbarui rincian dan menyajikan versi 1 pada daftar `Riwayat Perubahan Versi (Version History)` di bawahnya.

### 2.3 Alur Evaluasi Asuhan Berkala
1. Perawat melakukan evaluasi pada akhir shift.
2. Perawat menekan tombol `[Catat Evaluasi]`.
3. Perawat mengisi catatan perkembangan (SOAP) pada dialog modal `EvaluateCarePlanModal`.
4. Perawat menekan `[Simpan Evaluasi]`. Catatan evaluasi tersimpan via `PATCH /items/{itemId}/evaluate` dan langsung tampil di kotak *Catatan Evaluasi Terkini* lengkap dengan waktu evaluasinya.

### 2.4 Alur Penutupan Masalah & Penegakan Validasi `VAL-KEP-16`
1. Perawat menekan tombol `[Tutup Masalah]`.
2. Muncul dialog modal `CloseCarePlanModal` dengan opsi:
   - **Teratasi (`Resolved`):** Masalah klinis telah teratasi.
   - **Dihentikan (`Discontinued`):** Masalah tidak lagi relevan atau dialihkan.
3. **Pemeriksaan `VAL-KEP-16`:**
   - Bila perawat memilih `Resolved` namun butir asuhan belum memiliki satupun catatan evaluasi, tombol submit otomatis dinonaktifkan (`disabled`) dan muncul peringatan kuning informatif: *"Belum ada catatan evaluasi (VAL-KEP-16): Masalah asuhan tidak dapat dinyatakan tercapai sebelum memiliki sekurang-kurangnya satu catatan evaluasi perkembangan klinis."*
   - Disediakan tautan `+ Catat Evaluasi Sekarang` yang mengalihkan perawat untuk mencatat evaluasi terlebih dahulu.
4. Perawat mengisi alasan penutupan (minimal 5 karakter), lalu menekan `[Tutup sebagai Teratasi]`.
5. Butir asuhan berpindah status menjadi `✓ TERATASI` dan terkunci dari pengubahan lebih lanjut tanpa menghapus jejaknya di sistem.

### 2.5 Alur Episode Closed (Mode Hanya-Baca)
1. Pasien telah pulang dan episode rawat inap ditutup oleh administrasi (`Closed`).
2. Ruang kerja menampilkan banner: *"EPISODE TELAH DITUTUP. Rencana asuhan keperawatan berada dalam mode hanya-baca permanen."*
3. Seluruh tombol mutasi (`Tambah Masalah Baru`, `Ubah Rencana`, `Catat Evaluasi`, `Tutup Masalah`) disembunyikan secara otomatis.
4. Seluruh rincian masalah dan seluruh rantai riwayat versi tetap dapat ditelusuri dan dibaca secara lengkap (`AC-CAP013-03`, `INV-KEP-02`).

---

## 3. Rincian Perubahan Kode

### 3.1 Komponen Pustaka `src/components/ui/clinical-workspace/`
- **`ClinicalRevisionHistory.jsx`**: Kontainer daftar vertikal riwayat versi masalah asuhan dengan header dan notice klinis.
- **`ClinicalRevisionItem.jsx`**: Kartu simpul versi individual yang membedakan versi berjalan (`Versi X — CURRENT`) dengan versi arsip (`Versi X-1 DIARSIPKAN`), menampilkan teks masalah, tujuan, intervensi, evaluasi, serta atribusi nama perawat penulis versi asli (`OriginalAuthorEmployeeName`) dan waktu asli (`OriginalAuthoredAt`).
- **`clinical-workspace.module.css`**: Penambahan CSS module Bagian 15 (`.revisionHistoryRoot`, `.revisionItemCard`, `.revisionBadgeCurrent`, `.revisionBadgeArchived`, dll.).
- **`index.js`**: Ekspor publik untuk `ClinicalRevisionHistory` dan `ClinicalRevisionItem`.

### 3.2 Service API & Hook Domain Frontend
- **`nursing-care-plan.service.js`**: Klien HTTP Axios lengkap untuk 7 endpoint grup Nursing Care Plan (`getCarePlanByEpisodeId`, `createCarePlan`, `createCarePlanItem`, `updateCarePlanItem`, `evaluateCarePlanItem`, `closeCarePlanItem`, `getCarePlanItemRevisions`).
- **`use-inpatient-nursing-care-plan.jsx`**: Hook orkestrasi pemuatan data rencana asuhan, lazy opening rencana pertama kali, seleksi item aktif master-detail, pemuatan riwayat versi asinkron, filter status, dan penanganan mutasi CRUD asuhan.

### 3.3 Komponen Section Domain Rawat Inap (`sections/care-plan/`)
- **`care-plan-master-list.jsx`**: Panel kiri daftar masalah keperawatan dengan filter chip status (`ALL`, `ACTIVE`, `RESOLVED`, `DISCONTINUED`), counter item, nomor versi, dan tombol `[+ Tambah Masalah Baru]`.
- **`care-plan-detail-view.jsx`**: Panel kanan rincian masalah aktif, tujuan asuhan, rencana tindakan, catatan evaluasi terkini, pengkajian asal, status penutupan, tombol aksi operasional, dan sematan `ClinicalRevisionHistory`.
- **`modals/care-plan-item-form-modal.jsx`**: Modal penambahan masalah baru dan pengubahan butir asuhan (versi baru).
- **`modals/evaluate-care-plan-modal.jsx`**: Modal pencatatan evaluasi hasil asuhan berkala.
- **`modals/close-care-plan-modal.jsx`**: Modal penutupan masalah dengan penegakan validasi `VAL-KEP-16`.
- **`care-plan-section.jsx`**: Komponen utama penyatu master-detail 2 panel dengan `ClinicalStateBoundary`.
- **`nursing-workspace-sections.jsx`**: Menghubungkan `CarePlanSection` dan menghapus impor stub.
- **`care-plan-section-stub.jsx`**: Berkas stub dihapus sepenuhnya dari repositori.
- **`nursing-workspace.module.css`**: Penambahan kelas styling master-detail grid, kartu item, filter chip, detail box, dan banner episode closed.

---

## 4. Bukti Verifikasi & Acceptance Criteria

### 4.1 Pemenuhan Acceptance Criteria Fungsional

| Kriteria | Deskripsi Kebutuhan | Status | Bukti Implementasi & Verifikasi |
| :--- | :--- | :---: | :--- |
| **AC-01** | Penetapan Masalah, Tujuan, dan Intervensi | ✅ **TERBUKTI** | Form modal `CarePlanItemFormModal` memfasilitasi input masalah, tujuan, rencana tindakan, dan tautan pengkajian asal (`SourceAssessmentId`). Lulus uji `FE-RWI-054`. |
| **AC-02** | Master-Detail 2-Panel & Filter Status | ✅ **TERBUKTI** | Tata letak master-detail memisahkan panel kiri daftar masalah dan panel kanan rincian, lengkap dengan filter chip `ALL`, `ACTIVE`, `RESOLVED`, `DISCONTINUED`. Lulus uji `FE-RWI-054 Master-detail`. |
| **AC-03** | Penegakan Validasi `VAL-KEP-16` | ✅ **TERBUKTI** | Menutup butir sebagai `Resolved` diblokir jika belum ada catatan evaluasi, disertai pesan keselamatan dan tombol alih `+ Catat Evaluasi Sekarang`. Lulus uji `FE-RWI-054 VAL-KEP-16`. |
| **AC-04** | Version History vs Addendum Rekam Medis | ✅ **TERBUKTI** | Menggunakan mesin revisi versi (`ClinicalRevisionHistory`), mempertahankan penulis dan waktu asli setiap versi (`AC-CAP013-02`, `RWI-DEC-091`). Lulus uji `FE-RWI-054 Version History`. |
| **AC-05** | Perlindungan Mode Hanya-Baca saat Episode Closed | ✅ **TERBUKTI** | Saat `readOnly = true` (episode `Closed`), banner penutupan tampil dan seluruh tombol mutasi disembunyikan, data tetap terbaca penuh (`AC-CAP013-03`). Lulus uji `FE-RWI-054 Episode Closed`. |
| **AC-06** | Kontrak 7 Endpoint Service API | ✅ **TERBUKTI** | Seluruh 7 fungsi API terpasang lengkap di `nursing-care-plan.service.js`. Lulus uji `FE-RWI-054 Service API`. |
| **AC-07** | Penggantian Penuh Stub Lama | ✅ **TERBUKTI** | `CarePlanSectionStub` dihapus dan digantikan oleh `CarePlanSection` pada `nursing-workspace-sections.jsx`. Lulus uji `FE-RWI-054 Stub`. |

### 4.2 Hasil Eksekusi Unit Test

Perintah eksekusi native runner Node.js:
```powershell
node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-nursing-*.test.mjs"
```

Hasil eksekusi:
```text
✔ FE-RWI-052: Seluruh berkas komponen assessment dan base workspace terpasang (11.1487ms)
✔ FE-RWI-052 AC-01 & Visual AC-01: Satu layar terpadu dengan tepat 7 kelompok isian (FE-KEP-02) (0.7145ms)
✔ FE-RWI-052: Kalkulasi kelengkapan 7 kelompok pengkajian (0.252ms)
✔ FE-RWI-052 VAL-KEP-08: Deteksi bagian wajib yang belum lengkap sebelum finalisasi (0.1953ms)
✔ FE-RWI-052 VAL-KEP-11: Deteksi pencegahan pengkajian awal kedua (0.2622ms)
✔ FE-RWI-052 VAL-KEP-17 & AC-06: Penanda tenggat waktu kosong menampilkan 'Batas waktu belum ditetapkan' (1.7927ms)
✔ FE-RWI-052 VAL-KEP-12 & AC-02: Dialog koreksi addendum mewajibkan alasan min 5 karakter sebelum kirim (0.71ms)
✔ FE-RWI-052 RWI-DEC-091 & AC-03 & AC-04: Dokumen selesai terkunci tanpa tombol sunting dan koreksi via addendum (1.0903ms)
✔ FE-RWI-054: Seluruh berkas komponen rencana asuhan, service, dan modal terpasang (9.8247ms)
✔ FE-RWI-054: Stub rencana asuhan digantikan penuh dan diekspor dengan benar (0.7571ms)
✔ FE-RWI-054 VAL-KEP-16 & AC-CAP013-01: Penegakan validasi penutupan butir teratasi menuntut evaluasi (0.6569ms)
✔ FE-RWI-054 AC-CAP013-02 & RWI-DEC-091: Version History membedakan versi berjalan vs versi arsip masa lalu (0.6946ms)
✔ FE-RWI-054 AC-CAP013-03 & Bagian 16: Penegakan mode hanya-baca saat episode Closed (1.1216ms)
✔ FE-RWI-054: Kontrak service API nursing care plans lengkap (7 endpoint) (0.7416ms)
✔ FE-RWI-054: Master-detail 2-panel layout & filter status masalah (0.8237ms)
✔ FE-RWI-053: Seluruh berkas komponen timeline, hook, dan base workspace terpasang (7.2387ms)
✔ FE-RWI-053: Stub timeline telah digantikan dan diekspor dengan benar (1.3513ms)
✔ FE-RWI-053 AC-01 & AC-02: Kalkulasi tren perkembangan parameter klinis (computeMeasurementTrend) (0.3727ms)
✔ FE-RWI-053 AC-01: Integritas rekam medis & urutan kronologis menurun (waktu terbaru di atas) (0.1782ms)
✔ FE-RWI-053: Filter entri lini masa per kategori (filterTimelineEntries) (0.7195ms)
✔ FE-RWI-053 AC-03 & VAL-KEP-17: Pemisahan tiga keadaan mutlak pada section timeline (2.7753ms)
✔ FE-RWI-053 AC-04 & RWI-DEC-091: Tanda koreksi resmi [Koreksi #X] pada entri addendum (0.7942ms)
✔ FE-RWI-053: Format tanggal Bahasa Indonesia (formatTimelineDate) (18.2434ms)
✔ FE-RWI-051: Seluruh file domain dan base clinical workspace terpasang (10.8577ms)
✔ FE-RWI-051 AC-06: Nol butir menu baru pada sidebar (IA-INP-05) (0.9673ms)
✔ FE-RWI-051 AC-01: Akses dalam <= 3 klik lewat Census dan Detail Episode (IA-INP-01) (1.0411ms)
✔ FE-RWI-051 AC-07: Navigasi internal 4 section persis (0.7174ms)
✔ FE-RWI-051 AC-03: Context failure mematikan seluruh izin tulis secara mutlak (0.1731ms)
✔ FE-RWI-051 AC-08: Quick Summary metrics terhitung dengan benar (0.2109ms)

ℹ tests 29
ℹ suites 0
ℹ pass 29
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 176.4961
```

---

## 5. Dampak Traceability & Registri

1. **`frontend-roadmap.md`**:
   - Task `FE-RWI-054` diperbarui statusnya menjadi `[x] ✅ Selesai`.
2. **`requirement-traceability.md`**:
   - Kemampuan `CAP-013` Nursing Care (`EPIC KEP-03`) yang menaungi `FR-KEP-012` s.d. `FR-KEP-017` diperbarui status frontend-nya menjadi `FE-RWI-054` ✅ selesai 8 September 2026.
3. **Integritas Konstitusi Quilvian:**
   - Tidak ada penambahan route sidebar baru (`IA-INP-05`).
   - Tidak ada perubahan kode backend secara sepihak.
   - Tidak ada penggunaan gaya visual hardcoded atau framework CSS asing (vanilla CSS module dengan token baku Quilvian).
