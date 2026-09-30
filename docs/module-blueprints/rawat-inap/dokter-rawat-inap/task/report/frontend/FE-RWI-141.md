# Laporan Perubahan Frontend — `FE-RWI-141`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-141` |
| Judul | Tata letak V1 + tampilan V2, Riwayat SOAP, Catatan Dokter |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 3 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.4; didaftarkan di [`frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) |
| Trace | Permintaan pemilik 3; cacat C12; `soap.md` bagian 3.3, 3.4, wireframe 6.1–6.3; capture V1 `02-soap/01-form-soap.png`, `02-riwayat-soap.png`, `03-catatan-dokter.png` |
| Contract version | `0.6.1` + delta `BE-RWI-142` (diagnosa dan peran di lini masa) |
| Wewenang UI | Rencana kerja `soap.md` Rev 2.1 yang disetujui pemilik 30-09-2026. Batasnya: tab SOAP ruang kerja dokter rawat inap; header episode di luar tab tidak disentuh |
| Dependency | `FE-RWI-139` ✅, `FE-RWI-140` ✅, `BE-RWI-142` ✅ — semuanya source 30-09-2026 |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2, berkas diubah 2 (10 berkas), logika 1, kontrak 1, database 0, keamanan 0, UI 1 |
| Task mode | `FRONTEND` (laporan di repository backend) |
| Target tulis | `QuilvianSystemFrontendDev/src/**` bagian SOAP rawat inap |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `82487a491` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b342ae46` + perubahan `BE-RWI-141`/`BE-RWI-142` di working tree |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. ESLint dan test PASS. `npm run build` dan runtime **NOT RUN**. Target CSS di bawah 400 baris **tidak tercapai** (599 baris; lihat bagian 8) |

---

## 1. Keadaan yang ditemukan di awal

Dari atas tab sampai kolom Subjective, dokter melewati **lima lapis**:

1. header episode + alergi (milik ruang kerja);
2. `DoctorSoapHeader` di dalam tab;
3. banner "Riwayat Catatan" beserta drawer-nya;
4. `PatientSummaryBadge`, yang mengulang identitas pasien;
5. kartu "Form SOAP Dokter".

Pesan "minimal satu diagnosa ICD-10" bisa muncul tiga kali: di alert seksi ICD, di ringkasan validasi, dan sebagai error simpan. Ada dua bar aksi. CSS modul berisi 1.850 baris dengan banyak warna hex lepas dan `!important`. Riwayat dan Catatan Dokter V1 tidak punya padanan layak di V2.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter rawat inap (DPJP, konsulen, dokter jaga).

**Sub-tab Form SOAP** (urutan V1, `soap.md` 6.1):

1. Tiga sub-tab berikon: **Form SOAP** (badge "Draf Baru" saat menulis), **Riwayat SOAP** (jumlah catatan), **Catatan Dokter** (jumlah dokter).
2. Kartu **Form SOAP** berkepala satu baris: badge "Nama – No. RM", status (Catatan Baru/Draf/Final/Tidak Ditandatangani), **Waktu Pemeriksaan**, dan kelengkapan **S ✓ O ✓ A ○ P ○**.
3. Isi berurutan: **Tanda Vital** (`FE-RWI-140`) → **S | O** berdampingan → **Pilih ICD-10** + tabel (`FE-RWI-139`) → **Planning per Diagnosa** → **A | P** berdampingan.
4. Ikon dan warna aksen per seksi mengikuti V1: S stetoskop (primer), O mata (hijau), A clipboard (kuning), P dokumen (merah). Warnanya dari token.
5. Di bawah Subjective ada chip **"+ Sisipkan dari catatan perawat: Nyeri 2/10 (Monitoring Nyeri 05.00)"**. Chip ini hanya menyisipkan bila dokter menekannya, karena S adalah anamnesis dokter.
6. **Satu banner kuning** di atas bar aksi mendaftar kekurangan yang bisa diklik untuk langsung menuju isiannya, misalnya "Plan belum diisi." atau "Silakan pilih minimal satu diagnosa ICD-10 sebelum menyelesaikan SOAP."
7. **Satu bar aksi sticky** di bawah kartu: status simpan · **Reset** · **Simpan Draf** · **Selesaikan & Kunci**. **Selesaikan & Kunci** membuka modal penegasan penguncian.
8. Setelah diselesaikan, layar **pindah ke Riwayat SOAP** dan kartu catatan itu disorot dalam keadaan terbuka.

**Catatan final atau Tidak Ditandatangani:** isi asli tampil read-only beserta meta (Penulis, Pemeriksaan, Tanda Tangan, Status, **Diagnosa ICD-10**, **Sumber Tanda Vital**), lalu riwayat koreksi. Bar aksinya berisi **Koreksi** (bila berwenang) dan **Catatan Baru**.

**Catatan draf milik dokter lain:** editor tampil nonaktif dengan alasan FR-DOK-075 (hanya penulis yang boleh menyunting).

**Sub-tab Riwayat SOAP** (`soap.md` 6.2):
- Kepala V1: judul, deskripsi "Nama (RM)", "Total SOAP: n catatan", **Refresh**, **Buat SOAP Baru**.
- Penyaring: pencarian teks (dokter, keluhan, diagnosa, instruksi), dokter (dengan perannya), dan rentang waktu (24 jam, 3 hari, 7 hari).
- Kelompok per **Hari rawat ke-n** beserta tanggalnya.
- Tiap kartu: **SOAP #n**, jam, "dicatat belakangan" bila backdated, dokter · peran, badge status, chip ICD dengan Utama ditebalkan, dan chip TTV ringkas. Isi S/O/A/P dapat dibuka; kartu terbaru terbuka otomatis. Tombol **Buka** membuka catatan di Form SOAP.
- Seluruh data dari **satu** request lini masa (`BE-RWI-142`), tanpa request per kartu.

**Sub-tab Catatan Dokter** (`soap.md` 6.3):
- Dikelompokkan per dokter: nama · peran pada episode (DPJP/Konsulen/Dokter Jaga) · jumlah catatan · waktu terakhir.
- **Instruksi terakhir (P)** tiap dokter tampil paling atas.
- Penyaring **Catatan saya** dan pencarian teks. Tombol **Detail** membuka catatan di Form SOAP.

**Jalur tidak normal.** Lini masa gagal: "Catatan SOAP tidak dapat dimuat" + Coba Lagi. Akses ditolak: "Akses catatan SOAP ditolak". Belum ada catatan: status kosong dengan tombol **Buat Catatan SOAP Baru** (bila berwenang). Penyaring tanpa hasil: "Tidak ada catatan yang cocok".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`rules/frontend/*`; seluruh isi `tabs/progress-note/`; `doctor-clinical-base` (`ClinicalSectionPanel`, `ClinicalSegmentedNav`, `ClinicalTimeline(Item)`, `ClinicalCompletionBar`, `ClinicalStateBoundary`, `ClinicalActionGuard`, `ClinicalEmptyState`, `ClinicalDocumentMeta`, `DoctorSoapField`, `DoctorSoapReadOnlyView`, `ClinicalStatusBadge`) beserta CSS-nya; base `DataFilter`, `FilterSelect`, `ConfirmModal`, `BaseButton`, `InformationAlert`; modal `complete-document-modal.jsx`, `correction-modal.jsx`; `globals.css` (token); test `inpatient-physician-workspace`, `inpatient-final-consistency`, `inpatient-physician-clinical-tabs`; source V1 `soap-history-pasien.jsx`, `soap-catatan-dokter.jsx`, `Soap-module/index.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../tabs/progress-note/physician-progress-tab.jsx` | Ditulis ulang: `DoctorSoapHeader`, banner/drawer Riwayat, dan kartu identitas ganda dibuang. `ClinicalSegmentedNav` tiga sub-tab berikon; modal penegasan, koreksi, dan konfirmasi salin |
| `src/components/view/.../tabs/progress-note/soap-editor.jsx` | Ditulis ulang: kartu Form SOAP, urutan V1, ikon/aksen seksi, satu banner, satu bar sticky |
| `src/components/view/.../tabs/progress-note/soap-history-panel.jsx` | Riwayat SOAP per hari rawat, penyaring, chip ICD/TTV, sorotan |
| `src/components/view/.../tabs/progress-note/doctor-notes-panel.jsx` | Catatan Dokter per dokter dan peran, instruksi terakhir, "Catatan saya" |
| `src/components/view/.../tabs/progress-note/progress-note-detail.jsx` | Meta **Diagnosa ICD-10** (chip) dan **Sumber Tanda Vital** |
| `src/components/view/.../tabs/progress-note/progress-note-timeline.jsx` | **Dihapus** — digantikan Riwayat SOAP; grep memastikan tidak ada importer |
| `src/components/view/.../tabs/progress-note/patient-summary-badge.jsx` | **Dihapus** (berkas Rev 1 yang belum pernah di-commit) — identitas ganda |
| `src/style/health-services/inpatient-management/physician-progress-note.module.css` | Ditulis ulang **token-only**: 599 baris (507 non-kosong), dari 1.850 baris di working tree dan 438 baris di `HEAD`. Tanpa hex/rgb, tanpa `!important`, tanpa dark mode |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-progress-note.jsx` | Status sub-tab, sorotan catatan yang baru diselesaikan, pindah ke Riwayat sesudah Selesaikan |
| `src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx` | `groupNotesByHospitalDay`, `groupNotesByDoctor`, `formatClock`, `formatShortDateTime`, `describeElapsed`, normalisasi lini masa baru |

### 3.3 Kepatuhan arsitektur frontend

Satu hook orkestrasi tab (`use-inpatient-progress-note`) memakai dua sub-hook (`use-progress-note-diagnoses`, `use-progress-note-vital-signs`). View hanya merangkai base component. Satu CSS module fitur untuk seluruh tab.

**Temuan base component.** `ClinicalSectionPanel` dengan `layout` bawaan (`default`) adalah **strip kepala**: anak-anaknya ikut berjajar horizontal di samping judul (`display: flex; align-items: center`). Kelima panel baru semula memakai layout bawaan sebagai wadah, dan hasilnya pasti rusak. Semuanya kini memakai `layout="document"`, wadah resmi base itu. Base component tidak diubah.

**Tabel keputusan base component**

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Tiga sub-tab | `REUSE` `ClinicalSegmentedNav` (label berupa node berikon) | Base klinis; grup radio di dalam tab, bukan tablist kedua |
| Kartu seksi | `REUSE` `ClinicalSectionPanel layout="document"` | Base klinis |
| Isian S/O/A/P | `REUSE` `DoctorSoapField` (label berupa node berikon) | SOAP poliklinik |
| Banner kekurangan | `COMPOSE` `InformationAlert variant="warning"` + daftar `BaseButton ghost` | Menggantikan `ClinicalValidationSummary` dan alert ganda |
| Bar aksi sticky | `REUSE` `ClinicalCompletionBar` + `className` | Base klinis; posisi sticky dari CSS fitur |
| Status kosong/memuat/gagal/ditolak | `REUSE` `ClinicalEmptyState`, `ClinicalStateBoundary` | Base klinis |
| Guard penulis | `REUSE` `ClinicalActionGuard mode="disable"` | Base klinis |
| Riwayat dan Catatan Dokter | `COMPOSE` `ClinicalTimeline` + `ClinicalTimelineItem` + `DataFilter` + `FilterSelect` + `DoctorSoapReadOnlyView` + `<details>` native | Base klinis dan base |
| Chip ICD/TTV/kelengkapan, penanda salin | `COMPOSE` `<span>` + CSS token, pola `codeBadge` katalog | `StatusBadge` khusus status, bukan kode |
| Meta dokumen final | `REUSE` `ClinicalDocumentMeta` (dua entri baru) | Base klinis |
| Modal penegasan dan koreksi | `REUSE` `CompleteDocumentModal`, `CorrectionModal` | Modal ruang kerja yang sudah ada |

`UI GATE: PASS — 0 NEW, 0 EXTEND yang mengubah perilaku default; seluruh elemen REUSE/COMPOSE.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membuka catatan SOAP..." hanya saat lini masa belum pernah termuat; pemuatan ulang tidak mengosongkan layar |
| Kosong | "Belum ada catatan yang dibuka." + **Buat Catatan SOAP Baru** / **Lihat Riwayat SOAP (n)**; Riwayat: "Belum ada catatan SOAP"; Catatan Dokter: "Belum Ada Catatan Dokter" |
| Gagal | "Catatan SOAP tidak dapat dimuat" + **Coba Lagi**; gagal simpan/selesaikan tampil sebagai alert merah di kartu |
| Tanpa hak akses | "Akses catatan SOAP ditolak — Akun ini tidak memiliki izin membaca catatan dokter pada perawatan ini."; tanpa hak tulis: editor nonaktif beserta alasan dari ruang kerja |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/doctor-consultations/episodes/{episodeId}/soap-timeline` | Riwayat SOAP, Catatan Dokter, detail catatan; `diagnoses[]` dan `doctorAssignmentRoleLabel` dari `BE-RWI-142` | `DoctorConsultation : Read` |
| `PATCH` | `/v1/health-services/clinical-management/doctor-consultations/{id}/complete` | Selesaikan & Kunci | `DoctorConsultation : Complete` |

Koreksi memakai alur addendum yang sudah ada (`clinical-note-addendum.service`) tanpa perubahan.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <15 berkas SOAP> --quiet` lalu tanpa `--quiet` | Exit 0; 0 error, 0 peringatan | `PASS` | Keluaran ESLint 30-09-2026 |
| `npx eslint src --quiet` | 1 error di Tab Tindakan yang tidak disentuh | `EXISTING / ENVIRONMENT ISSUE` | Lihat `FE-RWI-139` bagian 6 |
| Test tab: `inpatient-physician-workspace`, `inpatient-final-consistency`, `inpatient-physician-clinical-tabs`, `inpatient-soap-modernization` | 69/69 lulus (termasuk penjaga string `setShowCompleteModal(false)`, `[episode?.id]`, `ClinicalActionGuard`, tanpa aksi antrean) | `PASS` | Keluaran `node --test` |
| Suite unit penuh | 2106 / 2101 lulus / 5 gagal, sama dengan garis dasar dan tidak terkait SOAP | `EXISTING / ENVIRONMENT ISSUE` untuk 5 kegagalan | Lihat `FE-RWI-139` bagian 6 |
| Kompilasi Turbopack dev route dokter rawat inap | HTTP 200; chunk `progress-note` 291 KB tanpa stub galat; CSS module terkompilasi (kelas `stickyActionBar` ada di stylesheet) | `PASS` | Pengambilan chunk 30-09-2026 |
| Seluruh kelas yang dipakai komponen ada di CSS module | 0 kelas hilang | `PASS` | Perbandingan `styles.*` dengan selector |
| Grep konsistensi UI | Tanpa `<button>`/`<table>`/`<select>` mentah, `style={{`, hex/rgb, `!important`, dark mode, `window.confirm` | `PASS` | `ui-consistency-checklist.md` |
| Tata letak 1366 px dan 768 px di peramban | — | `NOT RUN` | Dibuktikan di tingkat CSS saja (bagian 7 kriteria 4) |
| `npm run build` | — | `NOT RUN` | `next dev` pemilik berjalan; build dijalankan pemilik |

MANUAL TEST: NOT FEASIBLE — tidak ada sesi peramban berakun dokter pada sesi ini.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test` pada empat berkas test tab — PASS (69/69). Test pencocok string source tidak dihitung sebagai bukti fungsi (`soap.md` DoD butir 6).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.4) | Status | Bukti |
| --- | --- | --- |
| 1. Dari atas tab sampai Subjective hanya ada sub-tab, header kartu, dan kartu Tanda Vital | Terpenuhi di tingkat source | `physician-progress-tab.jsx`: `ClinicalSegmentedNav` → `SoapEditor`. `soap-editor.jsx`: kepala kartu + baris meta (padanan baris kepala wireframe 6.1) → `VitalSignsCard` → S/O. Komponen lama sudah dihapus |
| 2. Pesan "minimal satu diagnosa ICD-10" tampil tepat satu kali | Terpenuhi di tingkat source | Hanya banner kekurangan yang memuat `icdRequired`. Seksi ICD tidak punya alert sendiri, dan `ClinicalValidationSummary` tidak dipakai lagi |
| 3. Tidak ada warna hex lepas di CSS modul | Terpenuhi | Grep bagian 6; seluruh warna dari token |
| 4. 1366 px: S\|O dan A\|P berdampingan; 768 px: satu kolom tanpa gulir horizontal | Terpenuhi di tingkat CSS; peramban `NOT RUN` | `.soapGrid` dua kolom `minmax(0, 1fr)`, satu kolom pada `max-width: 991px`; wadah `min-width: 0` |
| 5. Riwayat SOAP menampilkan chip ICD tanpa request tambahan per kartu | Terpenuhi | `soap-history-panel.jsx` membaca `item.diagnoses` dari lini masa; tidak ada pemanggilan API di panel |
| Isi — hapus `DoctorSoapHeader`, banner/drawer Riwayat, `PatientSummaryBadge` | Terpenuhi | Bagian 3.2 |
| Isi — setelah Selesaikan pindah ke Riwayat dan kartu disorot | Terpenuhi di tingkat source | `completeNote` → `setActiveSubTab(history)` + `setHighlightedNoteId` |
| Isi — CSS modul token-only **di bawah 400 baris** | **Sebagian** — token-only terpenuhi; 599 baris | Bagian 8 |
| DoD 2, 4, 5, 7 | Terpenuhi | Bagian 6; roadmap dan traceability diperbarui |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | (1) **CSS 599 baris**, di atas target 400 baris milik rencana kerja (bukan kriteria terima). Satu modul menata sepuluh komponen: editor, kartu tanda vital, ICD, planning, Riwayat, Catatan Dokter, detail, koreksi, waktu pemeriksaan, dan tab. Deklarasi yang sama sudah digabung. Memadatkan lebih jauh berarti menumpuk deklarasi pada satu baris atau memecah modul, dan keputusan itu diserahkan ke pemilik. (2) Wireframe 6.2 menaruh tombol **Koreksi** di tiap kartu Riwayat; koreksi kini dicapai lewat **Buka** → **Koreksi**, karena kewenangan addendum dimuat per catatan terbuka. (3) Catatan Dokter V1 memakai modal detail; di V2 **Detail** membuka catatan di Form SOAP beserta riwayat koreksinya. (4) Penanda "otomatis dari …" (`soap.md` 3.3 butir 7) baru ada untuk teks salinan ("Disalin dari SOAP …") dan blok diagnosa (petunjuk **Susun ulang blok diagnosa**); blok TTV di Objective belum berpenanda |
| Dependency backend | `BE-RWI-142` ✅ source. Tanpa backend baru, chip ICD dan peran dokter kosong, tetapi layar tetap berjalan |
| Perubahan sampingan | `progress-note-timeline.jsx` (tracked) dan `patient-summary-badge.jsx` (untracked Rev 1) dihapus sesuai isi task |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi |
| Status Git | Berkas SOAP frontend: `M` `physician-progress-tab.jsx`, `progress-note-detail.jsx`, `soap-editor.jsx`, `inpatient-progress-note-constants.jsx`, `use-inpatient-progress-note.jsx`, `physician-progress-note.module.css`, `inpatient-progress-note-utils.jsx`; `D` `progress-note-timeline.jsx`, `src/utils/icdData.jsx`; `??` `doctor-notes-panel.jsx`, `soap-history-panel.jsx`, `soap-icd-section.jsx`, `soap-planning-accordion.jsx`, `vital-signs-card.jsx`, `use-progress-note-diagnoses.jsx`, `use-progress-note-vital-signs.jsx`, `tests/unit/inpatient-soap-modernization.test.mjs`. Berkas resep, keperawatan, dan `daily-nursing-action*` milik pekerjaan lain dan tidak disentuh |
| Langkah berikutnya | Pemilik menjalankan `npm run build` dan memeriksa tampilan pada 1366 px dan 768 px. Pemilik memutuskan butir masalah (1)–(4) |
