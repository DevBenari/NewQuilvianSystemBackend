# Requirement Traceability — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## Metadata

```yaml
module_id: rawat-inap
submodule: dokter-rawat-inap
traceability_revision: 3
status: APPROVED
blueprint_shape: COMPOSITE
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), approval desain 2026-09-03"
approved_at: "2026-09-03"
contract_versions: "0.4.0 (approved 2026-09-09) — task BE-RWI-037 s.d. BE-RWI-067 tetap terikat 0.3.0"
source_sha:
  backend: "93b3227c431401d8f586dec4e1fb25fbf41766e3"
  frontend: "863f24b0d1617069310c04e5770b47fd1b518b5b"
replan_source_sha:
  backend: "c82e69f327b01702b6bef595c685b10086ab7bd9"
  frontend: "e194509dc695aaa43264eab3ca7065762b14d2ee"
replan_date: "2026-09-08"
roadmaps:
  backend: "roadmap/backend-roadmap.md revision 3"
  frontend: "roadmap/frontend-roadmap.md revision 3"
task_range:
  backend: "BE-RWI-037 s.d. BE-RWI-053, ditambah BE-RWI-066 s.d. BE-RWI-068"
  frontend: "FE-RWI-042 s.d. FE-RWI-050 — nol ID baru pada revision 2"
```

---

## 0. Cara memakai dokumen ini

Dokumen ini menjawab empat pertanyaan yang sering ditanyakan saat pekerjaan berjalan:

| Pertanyaan | Bagian |
| --- | --- |
| Kemampuan ini dikerjakan task yang mana? | 1 |
| Task ini membuktikan requirement yang mana, dan diuji apa? | 2 |
| Keputusan pemilik ini diwujudkan di mana? | 3 |
| Apa yang **belum** punya task atau belum punya test? | 4 |

Kolom bukti diisi saat laporan task ditulis, bukan oleh dokumen ini.

> ⚠️ **Alinea berikut adalah keadaan 8 September 2026 dan sudah disusul.** Status yang berlaku ada
> pada tabel 9 September 2026 di bawahnya: tujuh ✅ dan dua 🟡.

**Status frontend per 8 September 2026:** **kesembilan task frontend sudah dikerjakan**, dan
tidak ada lagi yang berstatus `BELUM DIKERJAKAN`. Enam ✅ `SELESAI` — `FE-RWI-042`,
`FE-RWI-043`, `FE-RWI-045`, `FE-RWI-047`, `FE-RWI-048`, dan `FE-RWI-049`. Tiga
🟡 `SEBAGIAN` — `FE-RWI-044` (tombol Tambah Diagnosis tertahan `ConsultationId`
yang `[Required]`), `FE-RWI-046` (nama verifikator tidak dikembalikan kontrak baca CPPT), dan
`FE-RWI-050` (urutan daftar menunggu ketetapan `02-module-map.md`, dan nama penulis tidak ada
pada daftar pantau backend). Ketiganya tertahan kontrak backend serta keputusan pemilik,
**bukan** kekurangan source. Bukti ada pada sembilan laporan di
[`task/report/frontend/`](../task/report/frontend/). Status task backend mengikuti
[roadmap backend](backend-roadmap.md) dan laporannya masing-masing.

**Diperbarui revision 2, 8 September 2026.** Ketiga celah kontrak yang menahan tiga task 🟡 itu kini
**punya task pemiliknya sendiri**: `BE-RWI-066` untuk identitas verifikator, `BE-RWI-067` untuk nama
penulis pada daftar pantau, dan `BE-RWI-068` ⛔ untuk diagnosis tanpa nomor konsultasi. Sebelum revision
ini, ketiga celah itu hanya tercatat sebagai keluhan pada laporan task frontend — tercatat, tetapi tidak
ada seorang pun yang memilikinya. Perbedaannya penting: keluhan tidak pernah dikerjakan siapa-siapa,
sedangkan task punya pemilik, acceptance criteria, dan tempat pada register.

**Diperbarui 9 September 2026 — tujuh ✅ dan dua 🟡.** Dua dari ketiga task penutup itu sudah
selesai, dan builder frontend dijalankan ulang pada task yang menunggunya:

| Task | Status baru | Yang berubah |
| --- | :---: | --- |
| `FE-RWI-046` | 🟡 → **✅ `SELESAI`** | `BE-RWI-066` ✅ melengkapi balasan baca, sehingga nama verifikator benar-benar tampil. **5 dari 5** kriteria terpenuhi, dan gerbang §4.1 butir 10 terbukti penuh |
| `FE-RWI-050` | 🟡 → **🟡, 3 → 4 dari 5** | `BE-RWI-067` ✅ menutup kolom Penulis. Yang tersisa hanya kriteria 5, yaitu **keputusan** urutan daftar milik pemilik `02-module-map.md` — bukan pekerjaan |
| `FE-RWI-044` | 🟡 → **🟡 tetap** | Menunggu `BE-RWI-068`, yang pada 9 September 2026 **dibangun** dan berhenti di 🟡: source-nya terkompilasi, bukti ujinya belum dijalankan. `FE-RWI-044` baru dapat dijalankan ulang setelah task itu benar-benar **selesai** — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |

Satu temuan baru ikut tercatat: uji peramban 9 September 2026 menemukan seluruh sel tabel daftar
pantau verifikasi salah dirender karena kontrak `render` yang tertukar antara `DataTable` dan
`ClinicalDataTable`. Cacat itu diperbaiki dan dijaga dua uji unit —
[FE-RWI-050](../task/report/frontend/FE-RWI-050.md) bagian 3.4.

---

## 1. Kemampuan → epic → task

| Kemampuan | Nama | Epic | Task backend | Task frontend |
| --- | --- | --- | --- | --- |
| `CAP-015` | Pemeriksaan penunjang laboratorium dan radiologi | `EPIC DOK-06` | `BE-RWI-042`, `BE-RWI-052` | `FE-RWI-049` ✅ — [laporan](../task/report/frontend/FE-RWI-049.md) |
| `CAP-020` | Dokumentasi SOAP | `EPIC DOK-01`, `EPIC DOK-03` | `BE-RWI-037`, `BE-RWI-043`, `BE-RWI-044`, `BE-RWI-046`, `BE-RWI-047` | `FE-RWI-045` ✅ — [laporan](../task/report/frontend/FE-RWI-045.md) |
| `CAP-021` | Catatan terpadu beserta verifikasi | `EPIC DOK-04` | `BE-RWI-040` ✅, `BE-RWI-053` ✅, **`BE-RWI-066`** ✅, **`BE-RWI-067`** ✅ | **Celah kontrak bacanya ditutup 8 September 2026** — [BE-RWI-066](../task/report/backend/BE-RWI-066.md), [BE-RWI-067](../task/report/backend/BE-RWI-067.md). **Keduanya sudah dijalankan ulang 9 September 2026:** `FE-RWI-046` kini ✅ `SELESAI` dengan nama verifikator benar-benar tampil, dan `FE-RWI-050` naik menjadi **4 dari 5** kriteria dengan kolom Penulis bernama — tetap 🟡 karena kriteria 5-nya menunggu keputusan urutan daftar, bukan pekerjaan; [FE-RWI-046](../task/report/frontend/FE-RWI-046.md), [FE-RWI-050](../task/report/frontend/FE-RWI-050.md) |
| `CAP-022` | Kajian medis awal | `EPIC DOK-02` | `BE-RWI-040`, `BE-RWI-045`, **`BE-RWI-068`** 🟡 | `FE-RWI-044` 🟡 — tetap tertahan. `BE-RWI-068` **dibangun 9 September 2026** dan source-nya terkompilasi, tetapi berhenti di 🟡 karena bukti ujinya belum dijalankan. **Aturan 5 `CAP-022`** — daftar masalah berbentuk objek terstruktur — sudah punya source dan menunggu pembuktian — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |
| `CAP-023` | Resep rawat inap dan obat pulang | `EPIC DOK-06` | `BE-RWI-042`, `BE-RWI-043`, `BE-RWI-050` | `FE-RWI-048` ✅ — [laporan](../task/report/frontend/FE-RWI-048.md) |
| `CAP-024` | Tindakan dokter | `EPIC DOK-06` | `BE-RWI-051` | `FE-RWI-048` ✅ — [laporan](../task/report/frontend/FE-RWI-048.md) |
| `CAP-025` | Pencatatan visite dokter | `EPIC DOK-05` | `BE-RWI-041`, `BE-RWI-048`, `BE-RWI-049` | `FE-RWI-047` ✅ — [laporan](../task/report/frontend/FE-RWI-047.md) |

**Nol kemampuan tanpa task.** Ketujuhnya punya sekurang-kurangnya satu task backend dan satu task
frontend.

### 1.1 Task fondasi yang melayani lebih dari satu kemampuan

| Task | Melayani | Kenapa tidak dipecah per kemampuan |
| --- | --- | --- |
| `BE-RWI-037` | Seluruhnya | Jalur yang diperbaiki adalah pintu masuk semua dokumentasi dokter |
| `BE-RWI-038` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` | Satu mekanisme koreksi untuk empat jenis dokumen; memecahnya melahirkan empat jalur koreksi |
| `BE-RWI-039` | Seluruhnya | Satu service konteks dipakai seluruh perintah klinis, **dan dipakai bersama `keperawatan`** |
| `BE-RWI-040` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` | Satu migration lebih aman daripada empat migration berurutan pada tabel bertetangga |
| `FE-RWI-043` ✅ | Seluruh layar dokter | Kepala konteks dan penjaga kewenangan dipakai kedelapan layar. Shell, konteks keselamatan, enam tab, dan penjaga tulis tersedia sejak 7 September 2026 — [FE-RWI-043](../task/report/frontend/FE-RWI-043.md) |

---

## 2. Functional requirement → task → acceptance criteria → test

| FR | Bunyi singkat | Task | Acceptance criteria | Jenis test | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-DOK-001` | Catatan dibuat tanpa antrean pada perawatan berjalan | `BE-RWI-044` | `AC-CAP022-01`, `AC-CAP023-01` | Integration | ✅ Catatan dan pengkajian tersimpan tanpa antrean dan tanpa kunjungan IGD, beserta penanda perawatannya — [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-002` | Catatan kedua pada satu kunjungan rawat inap | `BE-RWI-043`, `BE-RWI-044` | `RWI-RULE-026` aturan 4 | Integration | ✅ Terbukti lewat endpoint 4 September 2026: dua catatan berturut-turut pada satu perawatan keduanya dijawab `200` — [BE-RWI-043](../task/report/backend/BE-RWI-043.md), [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-003` | Resep kedua sepanjang perawatan | `BE-RWI-043`, `BE-RWI-050` | `RWI-RULE-026` aturan 5 | Integration | ✅ Terbukti lewat endpoint. 4 September 2026: lima resep berturut-turut pada satu perawatan lima hari tersimpan seluruhnya — [BE-RWI-050](../task/report/backend/BE-RWI-050.md). 5 September 2026 dilengkapi uji berpasangan `ResepKedua_DiterimaLewatEndpointRawatInap_DitolakLewatEndpointRawatJalan`: resep kedua rawat inap `200` dan tersimpan `2`; resep aktif kedua tanpa konteks perawatan `400` dengan kalimat dibandingkan **utuh** dan hitungan tetap `1` — [BE-RWI-043](../task/report/backend/BE-RWI-043.md) bagian 8.1 |
| `FR-DOK-004` | Perilaku rawat jalan dan MCU tidak berubah | `BE-RWI-043` | **`RWI-AC-143`** | **Regression** | ✅ Test regresi rawat jalan dan medical check-up hijau; kalimat penolakan dibandingkan **utuh** — [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `FR-DOK-005` | Jalur IGD tidak rusak | `BE-RWI-037`, `BE-RWI-043` | `RWI-DEC-051` | **Regression** | ✅ Test regresi IGD hijau pada kedua task — [BE-RWI-037](../task/report/backend/BE-RWI-037.md), [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `FR-DOK-006` | Kajian medis pada perawatan berjalan | `BE-RWI-045` | `AC-CAP022-01` | Integration | ✅ Kajian medis hanya lahir di atas perawatan berjalan; kunjungan tanpa perawatan ditolak `422` — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-007` | Kajian medis dan catatan harian berbeda record | `BE-RWI-045` | `AC-CAP022-02` | Integration | ✅ Dua record pada dua tabel; menyelesaikan salah satunya tidak menggerakkan status yang lain — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `CAP-022` aturan 5 ★ `0.4.0` | Daftar masalah berbentuk **objek terstruktur berkode**, bukan teks | `BE-RWI-068` 🟡 | `VAL-DOK-36`, `VAL-DOK-37`, `VAL-DOK-39`, `VAL-DOK-40` | Integration | 🟡 Diagnosis terstruktur kini dapat lahir dari kajian medis: `ConsultationId` dilonggarkan dan `InpEpisodeId` ditambahkan pada `TrxPatientDiagnosis`, kelima aturan validasi ditegakkan `PatientDiagnosisController`. Kesembilan skenario bagian 11 **sudah ditulis tetapi belum dijalankan**. Source ada dan terkompilasi; **bukti ujinya belum dijalankan** — dihentikan atas instruksi pengguna 9 September 2026 — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |
| `CAP-022` aturan 2 ★ `0.4.0` | Daftar masalah menjadi bagian kajian medis, terbaca dari layarnya | `BE-RWI-068` 🟡 | Penyaring `inpEpisodeId` pada `api-contract.md` bagian 2.1 | Integration | 🟡 Penyaring `inpEpisodeId` terpasang pada `GET /` dan `GET /options`, dan `VAL-DOK-11` dipertajam sehingga kajian medis lolos bila daftar terstrukturnya terisi walau teks bebasnya kosong. Source ada dan terkompilasi; **bukti ujinya belum dijalankan** — dihentikan atas instruksi pengguna 9 September 2026 — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |
| `RWI-AC-143` ★ `0.4.0` | Rawat jalan dan MCU **tetap** menuntut nomor konsultasi pada diagnosis | `BE-RWI-068` 🟡 | `VAL-DOK-38` | **Regression** | 🟡 Jenis kunjungan diperiksa **lebih dulu**, sehingga rawat jalan, MCU, dan IGD tidak pernah masuk jalur baru; kalimat penolakannya dikunci sebagai konstanta. Uji regresinya membandingkan kalimat **utuh** tetapi **belum dijalankan**. Source ada dan terkompilasi; **bukti ujinya belum dijalankan** — dihentikan atas instruksi pengguna 9 September 2026 — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |
| `FR-DOK-008` | Catatan harian tidak menimpa kajian medis | `BE-RWI-045` | PRD `CAP-022` aturan 3 | Integration | ✅ Tiga catatan harian ditulis; isi, status, dan waktu kajian medis identik sebelum dan sesudahnya — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-009` | Diagnosis tersimpan terstruktur | `BE-RWI-045` | PRD `CAP-022` aturan 5 | Integration | ✅ **Blocker dicabut 5 September 2026.** Product/Domain memilih pilihan 1: `TrxPatientAssessment` memperoleh `WorkingDiagnosis` beserta `PhysicalExamination` dan `TherapyPlan`, dan kamus data bagian 3 direvisi `0.2` → `0.3`. Kriteria dibuktikan dua arah oleh `KajianMedisTanpaDiagnosis_DitolakDanHanyaDiagnosisYangDisebut` — menyebut "diagnosis kerja" dan **tidak** menyebut empat bagian yang sudah diisi — beserta kendali positif `KajianMedisYangLengkap_DapatDiselesaikanDanIsianMedisnyaTersimpan`. `WorkingDiagnosis` **bukan pengganti** `TrxPatientDiagnosis`, yang tetap memegang diagnosis berkode ICD — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) bagian 8 |
| `FR-DOK-010` | Koreksi kajian medis mempertahankan versi asli | `BE-RWI-038`, `BE-RWI-047` | `RWI-AC-158`, `RWI-AC-162` | Integration | ✅ Kajian medis yang selesai terdaftar tertanda tangan lalu menerima koreksi; isi aslinya tidak berubah karena koreksi menempel sebagai addendum bernomor urut — [BE-RWI-038](../task/report/backend/BE-RWI-038.md), [BE-RWI-047](../task/report/backend/BE-RWI-047.md) |
| `FR-DOK-011` | Perawat tidak dapat membuat kajian medis | `BE-RWI-045` | `VAL-DOK-05` | Integration | ✅ Pengguna yang tidak terhubung ke data dokter ditolak `403`, dan tetap boleh membuat pengkajian keperawatan — [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `FR-DOK-012` | Beberapa catatan sebagai lini masa | `BE-RWI-046` | `AC-CAP020-01` | Integration | ✅ `GET /doctor-consultations/episodes/{episodeId}/soap-timeline` mengembalikan catatan satu perawatan terurut waktu pemeriksaan — [BE-RWI-046](../task/report/backend/BE-RWI-046.md) |
| `FR-DOK-013` | Waktu pemeriksaan terpisah dari waktu penulisan | `BE-RWI-046` | PRD `CAP-020` aturan 2 | Integration | ✅ Urutan penulisan sengaja dibalik dari urutan pemeriksaan; lini masa mengikuti waktu pemeriksaan. Batas `VAL-DOK-13` dan `VAL-DOK-14` diuji terpisah — [BE-RWI-046](../task/report/backend/BE-RWI-046.md) |
| `FR-DOK-014` | Catatan dibuat walaupun pengkajian perawat belum selesai | `BE-RWI-044` | `AC-CAP020-02` | Integration | ✅ Pengkajian keperawatan berstatus `InProgress` tidak menahan pembuatan catatan — [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-015` | Perawatan tertutup menolak catatan baru, menerima koreksi | `BE-RWI-047` | `AC-CAP020-03`, `RWI-AC-161` | Integration | ✅ Catatan baru ditolak `422` beserta arahan bahwa koreksi tetap bisa; koreksi atas catatan lama diterima `201` — [BE-RWI-047](../task/report/backend/BE-RWI-047.md) |
| `FR-DOK-016` | Koreksi tidak mengaktifkan kembali perawatan | `BE-RWI-047` | `RWI-AC-161` | Integration | ✅ Status tetap tertutup; waktu masuk, waktu keluar, waktu tutup, dan tempat tidurnya identik sebelum dan sesudah koreksi — [BE-RWI-047](../task/report/backend/BE-RWI-047.md) |
| `FR-DOK-017` | Catatan lintas profesi tampil terpisah | `BE-RWI-053` | `AC-CAP021-01` | Integration | ✅ `GET /patient-integrated-progress-notes/episodes/{episodeId}` mengembalikan lini masa lintas profesi satu perawatan, dapat disaring jenis profesi — [BE-RWI-053](../task/report/backend/BE-RWI-053.md) |
| `FR-DOK-018` | Verifikasi tidak mengubah penulis asli | `BE-RWI-053`, `BE-RWI-066` | `AC-CAP021-03` | Integration | ✅ Penulis tetap perawat, verifikator DPJP; keduanya tersimpan pada kolom berbeda dan terbukti berbeda — [BE-RWI-053](../task/report/backend/BE-RWI-053.md). **Ditambah 8 September 2026:** keduanya kini juga **terbaca** pada dua kolom balasan yang berbeda beserta namanya masing-masing, dan enam kolom penulis dibandingkan sebelum-sesudah verifikasi — [BE-RWI-066](../task/report/backend/BE-RWI-066.md) |
| `FR-DOK-019` | Verifikasi hanya oleh DPJP aktif saat itu | `BE-RWI-053` | `VAL-DOK-07`, `INV-DOK-11` | Integration | ✅ Setelah pergantian DPJP, DPJP lama ditolak `403` dan DPJP baru diterima, walaupun catatannya ditulis pada masa DPJP lama — [BE-RWI-053](../task/report/backend/BE-RWI-053.md) |
| `FR-DOK-020` | Keterlambatan terpantau tanpa menahan | `BE-RWI-053` | `AC-CAP021-02`, `VAL-DOK-25` | Integration | ✅ Catatan lewat batas muncul pada daftar pantau bertanda terlambat, dan catatan berikutnya tetap dapat ditulis — [BE-RWI-053](../task/report/backend/BE-RWI-053.md) |
| `FR-DOK-021` | Kebijakan kosong berarti nol yang menunggu | `BE-RWI-053` | `VAL-DOK-24` | Integration | ✅ Tiga catatan lahir berstatus tidak-diwajibkan tanpa batas waktu; daftar pantau kosong; **nol angka batas waktu ditanam di kode** — [BE-RWI-053](../task/report/backend/BE-RWI-053.md) |
| `FR-DOK-022` | Koreksi mengembalikan ke menunggu verifikasi | `BE-RWI-053` | PRD `CAP-021` aturan 6 | Integration | ✅ Catatan terverifikasi kembali menunggu setelah dikoreksi; catatan tidak-diwajibkan **tidak** ikut dinaikkan — [BE-RWI-053](../task/report/backend/BE-RWI-053.md) |
| `FR-DOK-023` | Visite tercatat beserta identitasnya | `BE-RWI-048` | `RWI-AC-153` | Integration | ✅ Riwayat memuat perawatan, dokter, peran, waktu kedatangan, waktu pencatatan, dan pencatatnya — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-024` | Catatan tanpa event tidak menambah hitungan | `BE-RWI-048` | `RWI-AC-151` | Integration | ✅ Tiga catatan dokter tanpa satu pun kejadian visite menghasilkan hitungan **nol** — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-025` | Visite muncul walaupun catatannya menyusul | `BE-RWI-048` | `RWI-AC-150` | Integration | ✅ Kejadian tanpa satu pun catatan tetap muncul; waktu kedatangan tidak bergeser saat catatannya menyusul — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-026` | Kiriman berulang tidak melahirkan kejadian ganda | `BE-RWI-048` | `RWI-AC-152`, `RWI-AC-155` | **Integration PostgreSQL** | ✅ **8 September 2026.** Kiriman ulang biasa terbukti pada SQLite, dan dua permintaan yang tiba **benar-benar bersamaan** kini terbukti pada PostgreSQL 15.15: satu kejadian, identitas sama, yang kalah dijawab `200` — `DuaPermintaanBersamaan_KunciSama_HanyaSatuKejadian`. Menutupnya menyingkap celah nyata pada `PhysicianVisitService`, yang ikut diperbaiki — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-027` | Visite berdekatan diperingatkan, bukan ditolak | `BE-RWI-048` | `VAL-DOK-18` | Integration | 🟡 Yang terbukti adalah **tidak ditolaknya**: dua visite nyata pada tanggal yang sama menghasilkan dua baris dan hitungan dua. Penyampaian peringatannya belum dibuat; bentuk peringatan pada balasan pencatatan belum ada polanya — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-028` | Hanya dokter yang dapat mencatat visite | `BE-RWI-048` | `VAL-DOK-08` | Integration | ✅ Pengguna yang tidak terhubung ke data dokter ditolak `403` dengan kalimat `VAL-DOK-08` persis — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-029` | Resep dari konteks rawat inap | `BE-RWI-050` | `AC-CAP023-01` | Integration | ✅ Resep lahir dari catatan dokter yang berkonteks perawatan, dan mewarisi penanda perawatannya — [BE-RWI-050](../task/report/backend/BE-RWI-050.md) |
| `FR-DOK-030` | Obat pulang sebagai jenis resep eksplisit | `BE-RWI-042`, `BE-RWI-050` | `AC-CAP023-03` | Integration | ✅ Jenis resep dikirim layar dan tersimpan; penyaring obat pulang mengembalikan **satu** baris dari lima resep satu perawatan — [BE-RWI-050](../task/report/backend/BE-RWI-050.md) |
| `FR-DOK-031` | Status pemenuhan hanya dibaca | `BE-RWI-050` | `VAL-DOK-21` | Integration + **Architecture** | ✅ Keadaan pemenuhan terbaca dari konteks perawatan; dua uji arsitektur membuktikan **nol jalur tulis** dan nol aksi penyerahan obat — [BE-RWI-050](../task/report/backend/BE-RWI-050.md) |
| `FR-DOK-032` | Resep berulang tidak ganda | `BE-RWI-050` | `VAL-DOK-19` | Integration | ✅ Dua pengiriman berkunci sama mengembalikan resep yang sama; satu baris tersimpan — [BE-RWI-050](../task/report/backend/BE-RWI-050.md) |
| `FR-DOK-033` | Tindakan membedakan rencana dan pelaksanaan | `BE-RWI-051` | `RWI-DOK-RQG-005` | Integration | ✅ Jalur rencana menghasilkan `Planned`; jalur langsung menghasilkan `Completed`; keduanya tetap berjalan — [BE-RWI-051](../task/report/backend/BE-RWI-051.md) |
| `FR-DOK-034` | Kegagalan tagihan tidak menghapus catatan | `BE-RWI-051` | PRD `CAP-024` aturan 5 | Integration | ✅ Jalur Billing diputus; tindakan tetap `Completed` dan tetap ditandai dikerjakan, sedangkan baris fakta menyimpan keadaan pengiriman yang tidak berhasil — [BE-RWI-051](../task/report/backend/BE-RWI-051.md) |
| `FR-DOK-035` | Pesanan lab membawa konteks perawatan | `BE-RWI-042`, `BE-RWI-052` | `AC-CAP015-01` | Integration | ✅ Penanda perawatan dikirim saat pemesanan dan tersimpan; penanda milik perawatan lain ditolak `400` — [BE-RWI-052](../task/report/backend/BE-RWI-052.md) |
| `FR-DOK-036` | Hasil lab terbaca tanpa salinan | `BE-RWI-052` | `AC-CAP015-02` | Integration + **Architecture** | ✅ Dibaca dari baris milik Laboratorium apa adanya; uji arsitektur membuktikan **nol tabel salinan hasil** di bawah Rawat Inap — [BE-RWI-052](../task/report/backend/BE-RWI-052.md) |
| `FR-DOK-037` | Jalur tanpa antrean tidak gagal | `BE-RWI-037` | `DOK-TRC-DEF-01` | Integration + **Regression** | ✅ Enam test hijau, termasuk uji hitungan baris antrean sebelum dan sesudah — [BE-RWI-037](../task/report/backend/BE-RWI-037.md) |
| `FR-DOK-038` | Penanda perawatan tidak cocok ditolak | `BE-RWI-039`, `BE-RWI-044` | `VAL-DOK-26` | Integration | ✅ Terpasang pada jalur pembuatan catatan dan pengkajian, **pada kedua cabang** — berantre maupun tanpa antrean; ditolak `400` — [BE-RWI-039](../task/report/backend/BE-RWI-039.md), [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `FR-DOK-039` | Dua visite pada tanggal sama dihitung dua | `BE-RWI-048` | `RWI-AC-154` | Integration | ✅ Dua kejadian nyata pada tanggal yang sama menghasilkan dua baris dan hitungan dua — [BE-RWI-048](../task/report/backend/BE-RWI-048.md) |
| `FR-DOK-040` | Kejadian batal tetap tersimpan, tidak dihitung | `BE-RWI-049` | `INV-DOK-08`, `VAL-DOK-28`, `VAL-DOK-29` | Integration | ✅ Kejadian batal tetap tersimpan dan tetap tampil beserta alasannya; hitungan berlaku nol dari total satu; pembatalan tanpa alasan `400`, pembatalan kedua `409` — [BE-RWI-049](../task/report/backend/BE-RWI-049.md) |
| `FR-DOK-041` | Agregasi tagihan tidak mengubah kejadian klinis | `BE-RWI-049` | `RWI-AC-156` | Integration + **Architecture** | ✅ Dibuktikan pada tingkat arsitektur: **nol tipe di luar `ClinicalManagement`** yang menyentuh kejadian visite, sehingga Billing tidak dapat menggabungkan maupun menghapusnya — [BE-RWI-049](../task/report/backend/BE-RWI-049.md) |
| `FR-DOK-042` | Pesanan radiologi membawa konteks perawatan | `BE-RWI-042`, `BE-RWI-052` | `AC-CAP015-01` | Integration | ✅ Penanda perawatan dikirim saat pemesanan dan tersimpan; penanda milik perawatan lain ditolak `400` — [BE-RWI-052](../task/report/backend/BE-RWI-052.md) |
| `FR-DOK-043` | Hasil belum final diberi penanda | `BE-RWI-052` | `VAL-DOK-30` | Integration | ✅ Setiap baris membawa penanda hasil final beserta kalimat siap tampil; radiologi menuntut study lolos mutu sebelum dinyatakan final — [BE-RWI-052](../task/report/backend/BE-RWI-052.md) |
| `FR-DOK-044` | Finalisasi sekaligus mendaftarkan ke mesin keutuhan | `BE-RWI-038` | `RWI-AC-157` | Integration | ✅ Tiga jenis dokumen terdaftar tertanda tangan saat difinalkan, dengan **penulis dokumen** sebagai penanda tangan; kegagalan pendaftaran membatalkan finalisasi — [BE-RWI-038](../task/report/backend/BE-RWI-038.md) |
| `FR-DOK-045` | Koreksi pada catatan belum final ditolak | `BE-RWI-038` | `RWI-AC-159`, `VAL-DOK-32` | Integration | ✅ Ditolak `400` beserta arahan menyunting langsung pada catatannya — [BE-RWI-038](../task/report/backend/BE-RWI-038.md) |
| `FR-DOK-046` | Kajian medis dan tindakan ikut dapat dikoreksi | `BE-RWI-038` | `RWI-AC-162` | Integration | ✅ Keduanya terdaftar tertanda tangan saat diselesaikan, sehingga berada di keadaan yang menerima koreksi — [BE-RWI-038](../task/report/backend/BE-RWI-038.md) |
| `FR-DOK-047` | Koreksi atas nama dokter berhalangan hanya DPJP aktif | `BE-RWI-047` | `RWI-AC-163`, `RWI-AC-167`, `VAL-DOK-35` | Integration | ✅ Dokter yang bukan DPJP perawatan itu ditolak `403` **walaupun hak akses dan penetapannya lolos**; DPJP terakhir tetap dapat mengoreksi setelah pasien pulang — [BE-RWI-047](../task/report/backend/BE-RWI-047.md) |
| `FR-DOK-048` | Koreksi atas nama lain tidak mengubah penulis asli | `BE-RWI-047` | `RWI-AC-164` | Integration | ✅ Penulis catatan tetap dokter yang berhalangan; penulis koreksi tersimpan terpisah beserta penetapan yang mendasarinya — [BE-RWI-047](../task/report/backend/BE-RWI-047.md) |

---

> **Nol `FR-DOK-*` baru dibuat pada revision 3.** Ketiga baris bertanda ★ `0.4.0` di atas
> ditambatkan pada aturan kemampuan `CAP-022` dan acceptance `RWI-AC-143` yang **sudah ada**, bukan
> pada nomor requirement baru. Menerbitkan `FR-DOK-*` adalah pekerjaan `04-prd-to-mvp.md`, bukan
> wewenang roadmap; deret itu pun sudah terpakai sampai `FR-DOK-048`.

---

## 3. Decision ID → task

| Decision | Isinya | Task yang mewujudkan |
| --- | --- | --- |
| `RWI-DEC-038`, `RWI-DEC-070` | Pelonggaran antrean dan batas jumlah, terbatas rawat inap dan IGD | `BE-RWI-037`, `BE-RWI-039`, `BE-RWI-043` |
| `RWI-DEC-046` | Obat pulang sebagai jenis resep milik Farmasi | `BE-RWI-042`, `BE-RWI-050` |
| `RWI-DEC-062` | Persetujuan pemilik modul lintas modul | Prasyarat seluruh task; bukan task tersendiri |
| Approval kontrak `0.4.0`, 2026-09-09 ★ | Pelonggaran nomor konsultasi pada diagnosis — `INT-DOK-10`. Bukan keputusan bernomor tersendiri: `RWI-DEC-062` sudah memberi persetujuan lintas modul, dan `0.4.0` inilah yang menjadikannya dituntut blueprint | `BE-RWI-068` |
| `RWI-DEC-081` | Nol tabel dokumentasi klinis milik Rawat Inap | Dijaga architecture test pada `BE-RWI-041` dan matriks acceptance §7 |
| `RWI-DEC-083` | Pemetaan tujuh kemampuan ke sub-modul ini | Batas scope roadmap |
| `RWI-DEC-084` | Visite adalah kejadian klinis eksplisit | `BE-RWI-041`, `BE-RWI-048`, `FE-RWI-047` ✅ — keadaan kosong berbunyi “Belum ada visite tercatat” walau catatan perkembangan sudah ada; [laporan](../task/report/frontend/FE-RWI-047.md) |
| `RWI-DEC-085` | Setiap visite nyata satu hitungan; agregasi Billing terpisah | `BE-RWI-048`, `BE-RWI-049`, `FE-RWI-047` ✅ — visite berdekatan hanya diperingatkan dan tetap dapat dilanjutkan; kunci permintaan menjaga kiriman ulang tidak melahirkan kejadian kedua; [laporan](../task/report/frontend/FE-RWI-047.md) |
| `RWI-DEC-086` | Selesai sama dengan tertanda tangan sama dengan terkunci | `BE-RWI-038`, `FE-RWI-045` ✅ — peringatan penguncian tampil sebelum tombol ditekan, dan catatan final kehilangan editornya sama sekali; [laporan](../task/report/frontend/FE-RWI-045.md) |
| `RWI-DEC-087` | Tiga jenis dokumen didaftarkan ke mesin keutuhan | `BE-RWI-038` |
| `RWI-DEC-088` | Koreksi atas nama dokter berhalangan hanya DPJP aktif | `BE-RWI-047`, `FE-RWI-045` ✅ — tombol Koreksi hanya muncul bila endpoint authority server mengizinkan, dan penulis asli tidak pernah tergantikan pengoreksi; [laporan](../task/report/frontend/FE-RWI-045.md) |
| `RWI-DEC-051` | Kewajiban test regresi pada setiap perubahan mesin klinis | `BE-RWI-037`, `BE-RWI-043`, dan setiap task yang menyentuh mesin klinis |
| `RWI-RULE-021` | Batas waktu klinis — **belum final** | `BE-RWI-053` membangun mekanismenya tanpa angka |
| `RWI-RULE-026` | Tidak ada tabel tandingan dan tidak ada antrean semu | `BE-RWI-039` acceptance nomor 7; `FE-RWI-042` acceptance nomor 4 ✅ — sisi frontend terbukti 7 September 2026: pemindaian import/dependency transitif jalur rawat inap nol antrean, dan skenario peramban memeriksa seluruh URL yang diminta halaman tanpa menemukan request antrean — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md) |
| `RWI-DEC-089` | `CAP-016` pemakaian alat ditunda | **Di luar scope sub-modul ini** — milik `keperawatan` |

---

## 4. Coverage gap — yang belum tercakup

Bagian ini sengaja ditulis supaya lubangnya terlihat, bukan supaya dokumen terlihat rapi.

### 4.1 Requirement yang punya task tetapi belum punya test otomatis apa pun hari ini

| Keadaan | Buktinya | Akibatnya |
| --- | --- | --- |
| ~~**Nol** test untuk konsultasi, pengkajian, catatan terpadu, tindakan, resep, dan radiologi rawat inap~~ **sebagian tertutup, diperbarui 4 September 2026** | `DOK-TRC-VER-01`; penutupan sebagian oleh `BE-RWI-037` s.d. `BE-RWI-046` | Jaring pengamannya kini **86 test** pada `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/` ditambah **19 test hak akses peran non-SuperAdmin** pada `Tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/ClinicalManagement/`. Cakupannya: jalur tanpa antrean, service konteks klinis, bentuk kolom dan index, kejadian visite, penyaring pesanan laboratorium, **pintu masuk rawat inap**, **kajian medis**, **lini masa catatan harian**, dan **regresi poliklinik, medical check-up, serta IGD**. Yang masih kosong: catatan terpadu, tindakan, dan radiologi rawat inap. Setiap task berikutnya tetap **membawa test-nya sendiri** |
| ~~Test concurrency di atas PostgreSQL belum berjalan~~ **tertutup 8 September 2026** | Uji migration dijalankan 5 September 2026; test concurrency dan percobaan ulang dijalankan 8 September 2026 terhadap PostgreSQL **15.15** sekali pakai, database `quilvian_rwi_test`, 148 migration dan 555 tabel dari nol | **Seluruhnya tertutup.** Ketujuh migration `DOK-MVP-1` beserta migration `BE-RWI-045` lulus uji maju-mundur-maju, dan ujinya menemukan satu cacat rollback yang sudah diperbaiki. Ditambah **7 test PostgreSQL hijau** 8 September 2026: tiga `PhysicianVisitUniquenessTests` milik `BE-RWI-041` dan `BE-RWI-048`, empat `PatientProcedureRetryTests` milik `BE-RWI-051`. Penegakan unique index di bawah **dua permintaan serentak** kini terbukti pada kedua tabel — penuh pada `CliPhysicianVisit`, parsial pada `TrxPatientProcedure`. Penjagaan `BillingTestDatabaseFixture` **tidak dilemahkan**; database sekali pakai memenuhi tuntutannya apa adanya. **Yang masih terbuka:** CI belum menyediakan PostgreSQL, sehingga ketujuh test itu `NOT RUN` di sana — terhalang konfigurasi, bukan gagal |
| ~~**Nol** test frontend untuk ruang kerja dokter dan komponen dasar klinis~~ **tertutup untuk keenam tab klinis, diperbarui 8 September 2026** | `DOK-TRC-VER-01`; penutupan oleh `FE-RWI-042` s.d. `FE-RWI-050` | Jaring pengamannya kini **60 test source-level** — 8 pada `tests/unit/inpatient-physician-entry.test.mjs`, 16 pada `tests/unit/inpatient-physician-workspace.test.mjs`, 15 pada `tests/unit/inpatient-medical-assessment.test.mjs`, dan **21 baru** pada `tests/unit/inpatient-physician-clinical-tabs.test.mjs` — ditambah **35 skenario peramban** pada tiga spec `tests/e2e/inpatient-*.spec.mjs`. Seluruh 542 unit test lulus, 0 gagal. Cakupan yang bertambah 8 September 2026: urutan lini masa menurut waktu pemeriksaan beserta pemisahan dua waktu, penolakan waktu masa depan dan sebelum pasien masuk kamar, kecukupan satu bagian S/O/A/P, kestabilan kunci permintaan visite dan resep, peringatan visite berdekatan yang tidak menolak, kejadian batal yang tetap terbaca, pembedaan tiga jenis resep, keadaan Farmasi tak dikenal yang tidak dipalsukan, kegagalan tagihan yang tidak menurunkan status klinis, kefinalan hasil tiga arah, isolasi pesanan antar-perawatan, kelayakan tombol Verifikasi lima jalur, dan status verifikasi ambigu yang tidak pernah dinaikkan menjadi Diverifikasi. Ditambah lima uji pemindaian arsitektur yang membuktikan **nol** kemunculan tombol Sunting visite, “Tandai Diserahkan”, “Input Hasil”, dan kalimat “pemeriksaan radiologi belum tersedia di sistem”. **Diperbarui 9 September 2026:** jaring pengamannya kini **66 test source-level** — enam uji baru pada `tests/unit/inpatient-physician-clinical-tabs.test.mjs`, dua di antaranya menjaga kontrak `render` `DataTable` yang sempat tertukar — dan seluruh **563** unit test lulus, 0 gagal. **Verifikasi interaktif di peramban tidak lagi kosong bagi `FE-RWI-046` dan `FE-RWI-050`:** tujuh skenario dijalankan di Microsoft Edge terhadap `.next/standalone/server.js`, dan skenario itulah yang **menemukan** cacat render daftar pantau. Konfigurasi dan spec-nya sementara, lalu dihapus kembali; perlindungan permanennya dipindahkan ke uji unit. Yang masih kosong: verifikasi interaktif di peramban bagi empat task 8 September 2026 lainnya, tercatat `NOT RUN` pada laporannya masing-masing — [FE-RWI-045](../task/report/frontend/FE-RWI-045.md), [FE-RWI-046](../task/report/frontend/FE-RWI-046.md), [FE-RWI-047](../task/report/frontend/FE-RWI-047.md), [FE-RWI-048](../task/report/frontend/FE-RWI-048.md), [FE-RWI-049](../task/report/frontend/FE-RWI-049.md), [FE-RWI-050](../task/report/frontend/FE-RWI-050.md) |

### 4.2 Requirement yang sengaja tidak punya task pada rilis pertama

| Requirement | Kenapa tidak punya task | Penggantinya selama MVP |
| --- | --- | --- |
| Nilai batas waktu kajian medis dan verifikasi | `RWI-RULE-021` belum disahkan; pemilik klinis belum ditunjuk | Mekanismenya dibangun `BE-RWI-053` dengan kebijakan kosong |
| Pencatatan visite atas nama dokter | Kebijakannya belum ada | Bawaan aman: hanya dokter, dijaga `VAL-DOK-08` |
| Penagihan dan agregasi tarif visite | Kebijakan milik pemilik Billing belum ada | Kejadian klinis tetap dicatat lengkap sehingga aturan apa pun dapat dijalankan mundur |
| Pembacaan balik penyerahan obat pulang | Kontrak status final Farmasi belum disetujui — `RWI-DOK-RQG-003` | Butir daftar periksa ditandai manual petugas admisi |
| Pemberitahuan otomatis | Tidak ada requirement-nya — `RWI-DOK-RQG-001` | Daftar pantau dan daftar percobaan ulang |

### 4.3 Batas yang tidak dapat dijaga mesin mana pun

| Batas | Kenapa tidak dapat dijaga | Di mana dijaganya | Test penjaganya |
| --- | --- | --- | --- |
| Dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya | Mesin hak akses hanya mengenal peran terhadap endpoint | `BE-RWI-039`, `BE-RWI-044` | `VAL-DOK-06` — ⛔ **belum ditegakkan** per 4 September 2026. Service konteks sudah mampu memeriksanya, tetapi menyalakannya berarti menolak dokter konsulen dan dokter jaga yang bukan DPJP, dan kebijakan itu tidak disebut satu pun acceptance criteria `BE-RWI-044`. Menunggu keputusan pemilik |
| Kajian medis hanya oleh dokter, pengkajian keperawatan hanya oleh perawat | Keduanya berbagi satu sumber daya hak akses | `BE-RWI-045` | `VAL-DOK-05` — ✅ **ditegakkan** 4 September 2026, diturunkan dari penautan pengguna ke data dokter dan bukan dari nama peran |
| **Koreksi atas nama dokter lain hanya oleh DPJP aktif** | Penetapan berhalangan bersifat milik penulis, tidak menyebut penggantinya | `BE-RWI-047` | **`RWI-AC-167`** |
| Hasil yang dibaca milik perawatan yang sedang dibuka | Mesin hak akses tidak mengenal perawatan | `BE-RWI-052` | `VAL-DOK-31` |

> **Baris ketiga adalah yang paling mudah lolos.** Seluruh pemeriksaan hak aksesnya **berhasil**;
> yang menolak adalah aturan bisnis. Test yang hanya menguji hak akses tidak akan pernah
> menangkapnya, dan karena itu `RWI-AC-167` ditulis dengan catatan tegas.

### 4.4 Dependency lintas sub-modul yang belum punya roadmap penerima

| Butir | Keadaan | Yang harus terjadi |
| --- | --- | --- |
| Service konteks klinis bersama — `INT-DOK-01` dan `INT-KEP-01` | **Sudah dibuat `BE-RWI-039`** pada 3 September 2026 — `InpatientClinicalContextService`, terdaftar pada dependency injection | Roadmap `keperawatan` menerima **baris dependency**, bukan salinan task. Bukti: [BE-RWI-039](../task/report/backend/BE-RWI-039.md) |
| Kolom konteks pada tabel pengkajian | **Sudah dibuat `BE-RWI-040`** pada 3 September 2026 — `InpEpisodeId` dan `AssessmentType` beserta enum `PatientAssessmentType` | Roadmap `keperawatan` **memakainya apa adanya** dan tidak membuat ulang. Kolom `DueAt` dan `PolicyId` sengaja **tidak** dibuat: keduanya bergantung pada master kebijakan milik `keperawatan` yang belum ada. Bukti: [BE-RWI-040](../task/report/backend/BE-RWI-040.md) |
| Balasan verifikasi dan nama penulis pada daftar pantau ★ revision 2 | **Kini punya task**: `BE-RWI-066` dan `BE-RWI-067`. Keduanya menyentuh berkas milik `ClinicalManagement`, sehingga **penjadwalannya tetap wewenang pemilik modul itu** walaupun persetujuan menyentuhnya sudah ada lewat `RWI-DEC-062` | `FE-RWI-046` dan `FE-RWI-050` ditutup pada ID-nya sendiri setelah keduanya selesai |
| Urutan daftar di dalam daftar pantau | Ditetapkan `02-module-map.md` | `FE-RWI-050` mengikuti ketetapan itu, tidak memutuskan sendiri. **Per 8 September 2026 ketetapannya belum ada**, sehingga bagian verifikasi ditempatkan sementara di bawah keempat daftar existing tanpa mengubah urutannya, dan acceptance criteria nomor 5 `FE-RWI-050` dinyatakan **belum terpenuhi** — [FE-RWI-050](../task/report/frontend/FE-RWI-050.md). **Diperbarui revision 2:** ketetapan urutannya tetap belum ada dan tetap milik pemilik peta modul; yang berubah hanya bahwa **kolom Penulis** kini punya task pemiliknya, yaitu `BE-RWI-067`. Kedua hal itu sengaja tidak dicampur — satu dapat diselesaikan dengan bekerja, satu lagi hanya dapat diselesaikan dengan keputusan. **Diperbarui 9 September 2026:** yang dapat diselesaikan dengan bekerja **sudah selesai** — `BE-RWI-067` ✅ dan kolom Penulis kini bernama. Yang hanya dapat diselesaikan dengan keputusan **masih persis sama**: `02-module-map.md` baris 371 diperiksa ulang dan belum berubah, sehingga acceptance criteria nomor 5 `FE-RWI-050` tetap **belum terpenuhi** dan task itu tetap 🟡 |

---

### 4.5 Celah kontrak yang baru ketahuan setelah layarnya dibuat — ★ ditambahkan revision 2

Ketiga baris ini adalah jenis lubang yang **tidak mungkin terlihat saat perencanaan**. Kontrak
menyebut endpoint, hak akses, dan jenis balasannya, tetapi tidak pernah merinci kolom apa saja yang
harus ada di dalam balasan itu. Kekurangan kolom karena itu baru ketahuan ketika ada layar yang
benar-benar mencoba menampilkannya.

Ini bukan kegagalan perencanaan, dan bukan pula alasan untuk memperinci setiap kolom pada kontrak
berikutnya. Ini konsekuensi wajar dari mengerjakan backend lebih dulu: yang membuktikan sebuah
balasan sudah cukup adalah pemakainya, bukan penulisnya.

| Celah | Ditemukan oleh | Akibatnya bagi pengguna | Task pemiliknya | Status |
| --- | --- | --- | --- | --- |
| Balasan baca catatan terpadu tidak memuat kolom verifikasi apa pun, padahal keempat kolomnya sudah ada dan terisi di tabel | `FE-RWI-046` | DPJP tidak dapat melihat siapa yang memverifikasi sebuah catatan. Layar menampilkan keadaan kelima **"belum dapat dipastikan"** alih-alih menebak "sudah diverifikasi" — pilihan yang benar, tetapi bukan yang dijanjikan `AC-CAP021-03` | **`BE-RWI-066`** | ✅ **Ditutup 8 September 2026.** Balasan `GET /timeline`, `GET /episodes/{episodeId}`, `GET /`, `GET /{id}`, dan `PATCH /{id}/verify` kini membawa `verificationStatus`, `verifiedAt`, `verifiedByUserId`, `verifiedByUserName`, dan `verificationDueAt`. `dotnet test` `Failed: 0, Passed: 470` — [BE-RWI-066](../task/report/backend/BE-RWI-066.md) |
| Butir daftar pantau verifikasi hanya membawa nomor pengguna penulis | `FE-RWI-050` | Supervisor melihat kolom Penulis berbunyi "Nama penulis belum tersedia", lalu harus membuka catatannya satu per satu untuk tahu siapa yang perlu diingatkan | **`BE-RWI-067`** | ✅ **Ditutup 8 September 2026.** Butir daftar pantau kini membawa `providerName`, diambil snapshot lebih dulu sehingga akun yang berganti nama tidak menulis ulang riwayat. Nol isi klinis bocor, dibuktikan test — [BE-RWI-067](../task/report/backend/BE-RWI-067.md) |
| Kontrak API tidak memuat satu pun grup diagnosis, dan diagnosis terstruktur wajib menyebut nomor konsultasi | `FE-RWI-044` | DPJP tidak dapat menambah diagnosis kerja dari layar kajian medis awal, padahal di situlah diagnosis kerja sebenarnya lahir | **`BE-RWI-068`** | 🟡 **Celah kontraknya tertutup, dan source-nya dibangun 9 September 2026.** `ConsultationId` dilonggarkan, `InpEpisodeId` ditambahkan, dan kewenangan per pasien ditegakkan. Yang tersisa hanya bukti uji, yang belum dijalankan — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |

### 4.6 Selisih dokumen yang ditemukan saat perencanaan ulang — ★ ditambahkan revision 2

Dicatat supaya tidak hilang. Tidak satu pun menahan task, dan tidak satu pun diperbaiki dari
roadmap sub-modul karena berkasnya milik tingkat modul.

| Selisih | Keadaan sebenarnya | Pemilik perbaikannya |
| --- | --- | --- |
| `02-module-map.md` bagian 1 masih mencatat `dokter-rawat-inap` berstatus `draft` dengan approval "Belum" | Manifest sub-modulnya `approved` sejak 3 September 2026, dan `RWI-DEC-092` menyatakan **ketiga** sub-modul sudah disetujui sehingga status modul turun menjadi `approved` | Pemilik `02-module-map.md` |
| `02-module-map.md` bagian 3.3 memberi `keperawatan` kata "daftar **ketiga**" tetapi memberi dokter hanya "daftar **tambahan**" | Layar `FE-INP-09` hari ini sudah memuat **empat** daftar existing pada `INPATIENT_MONITORING_LIST_KEYS`, sehingga kata "ketiga" pun sudah tidak cocok. Urutan bagi kedua sub-modul perlu dinyatakan ulang sekaligus | Pemilik `02-module-map.md` bersama Frontend authority |

---

## 5. Arah balik — task → kemampuan

| Task | Kemampuan yang dilayani |
| --- | --- |
| `BE-RWI-037` | Seluruhnya — pintu masuk dokumentasi |
| `BE-RWI-038` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-039` | Seluruhnya |
| `BE-RWI-040` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-041` | `CAP-025` |
| `BE-RWI-042` | `CAP-015`, `CAP-023` |
| `BE-RWI-043` | `CAP-020`, `CAP-023` |
| `BE-RWI-044` | `CAP-020`, `CAP-022` |
| `BE-RWI-045` | `CAP-022` |
| `BE-RWI-046` | `CAP-020` |
| `BE-RWI-047` | `CAP-020`, `CAP-021`, `CAP-022`, `CAP-024` |
| `BE-RWI-048` | `CAP-025` |
| `BE-RWI-049` | `CAP-025` |
| `BE-RWI-050` | `CAP-023` |
| `BE-RWI-051` | `CAP-024` |
| `BE-RWI-052` | `CAP-015` |
| `BE-RWI-053` | `CAP-021` |
| `BE-RWI-066` ★ ✅ | `CAP-021` — melengkapi balasan verifikasi supaya `FE-RWI-046` dapat ditutup |
| `BE-RWI-067` ★ ✅ | `CAP-021` — melengkapi daftar pantau supaya `FE-RWI-050` dapat ditutup |
| `BE-RWI-068` ★ 🟡 | `CAP-022` aturan 2 dan 5 — membuka diagnosis kerja terstruktur dari kajian medis. Source dibangun 9 September 2026 dan terkompilasi; bukti ujinya belum dijalankan — [BE-RWI-068](../task/report/backend/BE-RWI-068.md) |
| `FE-RWI-042` ✅ | Seluruhnya — keterjangkauan |
| `FE-RWI-043` ✅ | Seluruhnya — konteks pasien |
| `FE-RWI-044` 🟡 | `CAP-022` |
| `FE-RWI-045` ✅ | `CAP-020` |
| `FE-RWI-046` ✅ | `CAP-021` |
| `FE-RWI-047` ✅ | `CAP-025` |
| `FE-RWI-048` ✅ | `CAP-023`, `CAP-024` |
| `FE-RWI-049` ✅ | `CAP-015` |
| `FE-RWI-050` 🟡 | `CAP-021` |

---

## 6. Definition of Done tingkat sub-modul

Diturunkan dari `04-prd-to-mvp.md` bagian 19. Sub-modul dianggap selesai untuk rilis pertama hanya
bila **seluruh** butir terjawab "ya".

| No | Butir | Task pembuktinya |
| ---: | --- | --- |
| 1 | Jalur tanpa antrean tidak lagi gagal dan tidak menyentuh data antrean | `BE-RWI-037` |
| 2 | Jalur IGD dan poliklinik terbukti tidak rusak | `BE-RWI-037`, `BE-RWI-043` |
| 3 | Catatan rawat inap dapat dibuat tanpa antrean | `BE-RWI-044` ✅ |
| 4 | Catatan dan resep kedua diterima rawat inap, tetap ditolak rawat jalan | `BE-RWI-043` |
| 5 | Kajian medis dan catatan harian terbukti berbeda record | `BE-RWI-045` ✅ |
| 6 | Waktu pemeriksaan terpisah dan lini masa terurut benar | `BE-RWI-046` ✅ |
| 7 | Perawatan tertutup menolak catatan baru dan menerima koreksi | `BE-RWI-047` |
| 8 | Verifikasi tidak mengubah penulis asli | `BE-RWI-053` |
| 9 | Verifikasi hanya oleh DPJP aktif saat itu | `BE-RWI-053` |
| 10 | Catatan tanpa kejadian visite tidak menambah hitungan | `BE-RWI-048` |
| 11 | Dua visite nyata pada tanggal sama menghasilkan hitungan dua | `BE-RWI-048` |
| 12 | Kiriman ulang tetap satu kejadian, terbukti pada PostgreSQL sungguhan | `BE-RWI-048` |
| 13 | Kejadian yang dibatalkan tetap tersimpan dan tidak dihitung | `BE-RWI-049` |
| 14 | Agregasi tagihan tidak mengubah riwayat klinis | `BE-RWI-049` |
| 15 | Kegagalan penagihan tidak menghilangkan catatan tindakan | `BE-RWI-051` |
| 16 | Obat pulang terbedakan dari resep harian | `BE-RWI-050` |
| 17 | Pesanan lab dan radiologi tidak dapat dipakai lintas perawatan | `BE-RWI-052` |
| 18 | Hasil terbaca tanpa salinan, yang belum final ditandai | `BE-RWI-052` |
| 19 | Nol jalur tulis menuju status pemenuhan dan hasil penunjang | `BE-RWI-050`, `BE-RWI-052` |
| 20 | Nol tabel `Inp*` untuk dokumentasi dokter | `BE-RWI-041` |
| 21 | Nol entity baru berawalan `Trx*` | `BE-RWI-041` |
| 22 | Nol baris antrean dibuat untuk pasien rawat inap | `BE-RWI-039`, `FE-RWI-042` ✅ sisi frontend terbukti 7 September 2026 — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md) |
| 23 | Butir hak akses baru berfungsi bagi peran non-SuperAdmin | `BE-RWI-044` ✅ — nol butir baru diperlukan; keenam butir yang dipakai sudah ada dan terbukti dapat diberikan kepada peran non-SuperAdmin |
| 24 | Ruang kerja membaca daftar pasien dirawat, tanpa aksi antrean | `FE-RWI-042` ✅, `FE-RWI-043` ✅ — daftar pasien terkunci pada `doctorId` sesi, dan ruang kerja membaca episode, penempatan, penugasan DPJP, alergi, serta diagnosis tanpa satu pun permintaan antrean; dibuktikan pemindaian source dan skenario peramban — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md), [FE-RWI-043](../task/report/frontend/FE-RWI-043.md) |
| 25 | Delapan layar terjangkau sesuai `IA-INP-01` dan `IA-INP-05` | `FE-RWI-042` ✅ s.d. `FE-RWI-050` 🟡 — jalur masuknya sudah ada dan terbukti dalam batas tiga klik lewat menu, Census, dan detail episode. **Diperbarui 8 September 2026:** keenam tab klinis kini berisi, dan daftar pantau verifikasi menambahkan tautan langsung ke tab Catatan Terpadu lewat `?tab=`; nol route dan nol butir menu baru ditambahkan, sehingga kuota `IA-INP-05` tidak tersentuh. **Diperbarui 9 September 2026:** tautan itu kini memakai `?tab=` **dan** `?note=` sekaligus, sehingga supervisor mendarat pada catatan yang dituju dan bukan pada awal lini masa; nol route baru tetap ditambahkan — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md), [FE-RWI-045](../task/report/frontend/FE-RWI-045.md), [FE-RWI-050](../task/report/frontend/FE-RWI-050.md) |
| 26 | Kolom sensitif tidak muncul di logger | Seluruh task backend |
| 27 | Baris registry `Rad` sudah `ACTIVE` | `BE-RWI-042` |
| 28 | Finalisasi sekaligus mendaftarkan; kegagalannya membatalkan finalisasi | `BE-RWI-038` |
| 29 | Catatan final dapat dikoreksi; catatan konsep menolak koreksi | `BE-RWI-038` |
| 30 | Kajian medis dan tindakan ikut dapat dikoreksi | `BE-RWI-038` |
| 31 | Koreksi atas nama dokter lain tidak mengubah penulis aslinya | `BE-RWI-047` |
| 32 | Penetapan berhalangan tanpa masa berlaku ditolak | `BE-RWI-047` |
| 33 | Hanya DPJP aktif yang dapat mengoreksi atas nama dokter lain | `BE-RWI-047` |

**Tiga puluh tiga butir, seluruhnya punya task pembukti.** Tidak ada butir yang menggantung tanpa
pemilik pekerjaan.

> **Catatan revision 2, 8 September 2026.** Butir 8 dan 9 dibuktikan `BE-RWI-053` **di sisi backend**, dan
> pembuktian itu tetap sah: verifikasi memang tidak mengubah penulis asli, dan memang hanya dapat dilakukan
> DPJP aktif. Yang belum ada adalah **cara pengguna melihatnya** — balasan bacanya tidak memuat kolom
> verifikasi sama sekali. Selama `BE-RWI-066` belum dikerjakan, kedua butir itu terbukti pada database
> tetapi tidak terbukti pada layar, dan `FE-RWI-046` karena itu tetap 🟡. Definition of Done tingkat
> sub-modul **belum** dapat dinyatakan lengkap, dan gelombang `DOK-MVP-FE` belum boleh ditutup.
>
> **Diperbarui 9 September 2026.** Butir 8 dan 9 kini terbukti **pada layar juga**, bukan hanya pada
> database: `BE-RWI-066` ✅ melengkapi balasan bacanya, dan `FE-RWI-046` ✅ menampilkannya sebagai dua
> baris terpisah dengan nama masing-masing — diperiksa di peramban, bukan hanya pada source.
> `FE-RWI-046` karena itu naik menjadi ✅ `SELESAI`.
>
> **Yang tidak berubah:** Definition of Done tingkat sub-modul **tetap belum lengkap**, dan gelombang
> `DOK-MVP-FE` **tetap belum boleh ditutup**. Dua task masih 🟡 — `FE-RWI-050` menunggu ketetapan
> urutan daftar dari pemilik `02-module-map.md`, dan `FE-RWI-044` menunggu `BE-RWI-068` yang sendirinya
> ⛔. Keduanya tertahan hal yang bukan pekerjaan frontend.
