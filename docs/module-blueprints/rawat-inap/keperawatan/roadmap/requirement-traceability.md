# Requirement Traceability — Sub-modul Keperawatan Rawat Inap

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 5
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
traceability_revision: 2
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah (RWI-DEC-092)"
approved_at: "2026-09-03"
contract_versions: 0.3.0
planning_source_sha:
  backend: 7d4bf2b91d39265eab4453a230ba324f95866962
  frontend: eb505a9ea20d99505a69ff0e6ea428ec9a551dc5
requirement_readiness: INP-S16 PARTIALLY_READY
requirement_gate_revision: 1.4
task_count_backend: 12
task_count_frontend: 6
```

---

## 0. Apa yang dijaga dokumen ini

Pada bentuk `COMPOSITE`, berkas traceability menjadi milik **masing-masing sub-modul**, padahal
requirement lahir dari **satu** wawancara di tingkat modul. Akibatnya sudah dicatat
`bentuk-blueprint.md` bagian 5: kemampuan yang tidak ditugaskan ke sub-modul mana pun **tidak
diperiksa siapa pun**.

Dokumen ini menjaga jatah `keperawatan` saja: lima kemampuan sesuai `RWI-DEC-083`. Pemeriksaan
kemampuan yatim untuk seluruh 28 kemampuan tetap dipegang
[`../../02-module-map.md`](../../02-module-map.md) bagian 4.

**Yang berubah pada revision `2`:** gerbang approval sudah dicabut, sehingga kolom Status tidak
lagi berbunyi `BLOCKED` di seluruh baris; satu task backend bertambah (`BE-RWI-065`); dan tiga
prasyarat yang dulu tercatat sebagai penghalang ternyata sudah mendarat di source.

---

## 1. Kemampuan ke epic ke task

| Kemampuan | Epic | Requirement | Task backend | Task frontend | Status |
| --- | --- | --- | --- | --- | --- |
| `CAP-012` Nursing Assessment | `EPIC KEP-01` | `FR-KEP-001` s.d. `FR-KEP-004` | `BE-RWI-054` ✅ | `FE-RWI-051` ✅ | Backend ✅ selesai 6 September 2026; frontend ✅ selesai 7 September 2026 [laporan](../task/report/frontend/FE-RWI-051.md) |
| `CAP-012` Nursing Assessment | `EPIC KEP-02` | `FR-KEP-005` s.d. `FR-KEP-011` | `BE-RWI-055` ✅, `BE-RWI-056` ✅, `BE-RWI-065` ✅, `BE-RWI-057` ✅, `BE-RWI-058` ✅ | `FE-RWI-052` ✅, `FE-RWI-053` ✅ | Backend ✅ — kelimanya selesai, `BE-RWI-056` ditutup 8 September 2026 [laporan](../task/report/backend/BE-RWI-056.md); frontend FE-RWI-052 ✅ selesai 7 September 2026 [laporan](../task/report/frontend/FE-RWI-052.md), FE-RWI-053 ✅ selesai 8 September 2026 [laporan](../task/report/frontend/FE-RWI-053.md) |
| `CAP-013` Nursing Care | `EPIC KEP-03` | `FR-KEP-012` s.d. `FR-KEP-017` | `BE-RWI-059`, `BE-RWI-060` | `FE-RWI-054` ✅ | Backend menunggu implementasi; frontend FE-RWI-054 ✅ selesai 8 September 2026 [laporan](../task/report/frontend/FE-RWI-054.md) |
| `CAP-014` Nursing Interventions | `EPIC KEP-04` | `FR-KEP-018` s.d. `FR-KEP-023` | `BE-RWI-061` ✅, `BE-RWI-062` ✅, `BE-RWI-063` ✅ | `FE-RWI-055` ✅ | Backend ✅ — ketiganya selesai, `BE-RWI-061` ditutup 8 September 2026 sesudah uji PostgreSQL hijau [laporan](../task/report/backend/BE-RWI-061.md); frontend FE-RWI-055 ✅ selesai 8 September 2026 [laporan](../task/report/frontend/FE-RWI-055.md) |
| `CAP-012` kepatuhan | `EPIC KEP-05` | `FR-KEP-024` s.d. `FR-KEP-026` | `BE-RWI-064` ✅ | `FE-RWI-056` ✅ | Backend ✅ selesai 6 September 2026; frontend ✅ selesai 8 September 2026 [laporan](../task/report/frontend/FE-RWI-056.md) |
| `CAP-016` Equipment Usage | `EPIC KEP-06` | `FR-KEP-027`, `FR-KEP-028` | **Nol task, disengaja** | **Nol task, disengaja** | `DEFERRED` |
| `CAP-027` Nutrition Care | — | Skrining ikut `EPIC KEP-01`/`KEP-02`; rujukan `INT-KEP-04` | Sebagian `BE-RWI-056` ✅ | Sebagian `FE-RWI-052` ✅ | Sebagian; skrining gizi kini **wajib terisi** sebelum pengkajian rawat inap diselesaikan [laporan](../task/report/frontend/FE-RWI-052.md); rujukan menunggu modul Gizi |

**Nol kemampuan tanpa epic pemilik.** `CAP-016` punya epic dan punya sub-modul pemilik; ia
`DEFERRED`, bukan yatim — perbedaan itu diuji `RWI-AC-169`.

---

## 2. Requirement ke keputusan ke kontrak ke bukti

| Requirement | Keputusan / aturan | Kontrak `0.3.0` | Acceptance test | Task |
| --- | --- | --- | --- | --- |
| `FR-KEP-001` | `RWI-DEC-062`, `RWI-RULE-026`; PRD 16.2 aturan 2 | Integration `INT-KEP-01` | `AC-CAP012-01` — **terbukti** `RawatInapTanpaAntreanDanTanpaIgd_PengkajianTersimpan` | `BE-RWI-054` ✅ [laporan](../task/report/backend/BE-RWI-054.md) |
| `FR-KEP-002` | PRD 16.2 aturan 1 | Validation `VAL-KEP-01` s.d. `VAL-KEP-03` | **Terbukti** — tiga uji `..._Ditolak422` | `BE-RWI-054` ✅ [laporan](../task/report/backend/BE-RWI-054.md) |
| `FR-KEP-003` | `RWI-DEC-051`, `RWI-DEC-070` | Validation `VAL-KEP-04` | **Terbukti** — empat uji regresi poliklinik, MCU, IGD, dan jalur berantre; utang uji `RWI-DEC-051` tertutup | `BE-RWI-054` ✅ [laporan](../task/report/backend/BE-RWI-054.md) |
| `FR-KEP-004` | `RWI-DEC-081`; PRD 16.2 aturan 1 | API grup Patient Assessment | **Terbukti** — pengkajian tersimpan beserta `InpEpisodeId` dan terbaca per perawatan | `BE-RWI-054` ✅ [laporan](../task/report/backend/BE-RWI-054.md) |
| `FR-KEP-005` | PRD 16.2 aturan 3 | State transition bagian 1 | `AC-CAP012-02` — **terbukti** `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` | `BE-RWI-056` ✅ [laporan](../task/report/backend/BE-RWI-056.md) |
| `FR-KEP-006` | PRD 16.2 aturan 3 | Validation `VAL-KEP-11` | **Terbukti** — `409` beserta kalimat `VAL-KEP-11` apa adanya di backend; deteksi proaktif dan alihkan pengkajian ulang di frontend | `BE-RWI-056` ✅ [laporan](../task/report/backend/BE-RWI-056.md), `FE-RWI-052` ✅ [laporan](../task/report/frontend/FE-RWI-052.md) |
| `FR-KEP-007` | PRD 16.2 aturan 6 | API `GET /episodes/{episodeId}/timeline` | `AC-CAP012-02` — **terbukti** tiga pengukuran nyeri berurutan tampil seluruhnya | `BE-RWI-058` ✅ [laporan](../task/report/backend/BE-RWI-058.md), `FE-RWI-053` ✅ [laporan](../task/report/frontend/FE-RWI-053.md) |
| `FR-KEP-008` | **`RWI-DEC-091`**; PRD 16.2 aturan 13, 27.3 aturan 7 | Integration `INT-KEP-06`; API `POST /{id}/addendums` | `AC-CAP012-05`, `RWI-AC-175` — **terbukti** koreksi bernomor tersimpan, isi asli utuh, status tetap `Completed`; form terkunci dan koreksi via addendum di frontend | **`BE-RWI-065`** ✅ [laporan](../task/report/backend/BE-RWI-065.md), `BE-RWI-057` ✅ [laporan](../task/report/backend/BE-RWI-057.md), `FE-RWI-052` ✅ [laporan](../task/report/frontend/FE-RWI-052.md) |
| `FR-KEP-009` | PRD 16.2 aturan 12 | State transition bagian 1.1 | **Terbukti** — penyuntingan dokumen terkunci ditolak `400`; grup ini tidak punya satu pun endpoint `DELETE` | **`BE-RWI-065`** ✅ [laporan](../task/report/backend/BE-RWI-065.md), `BE-RWI-057` ✅ [laporan](../task/report/backend/BE-RWI-057.md) |
| `FR-KEP-010` | `RWI-RULE-021`; PRD 16.2 aturan 11 | Validation `VAL-KEP-18` | `AC-CAP012-04` — **terbukti** mengubah kebijakan tidak mengubah penilaian pengkajian yang lalu | `BE-RWI-054` ✅, `BE-RWI-055` ✅, `BE-RWI-058` ✅ [laporan](../task/report/backend/BE-RWI-058.md) |
| `FR-KEP-011` | PRD 16.2 aturan 11 | Validation `VAL-KEP-17` | **Terbukti** — master kosong menghasilkan `NotMonitored` dan kalimat "batas waktu belum ditetapkan", bukan "terlambat" | `BE-RWI-055` ✅ [laporan](../task/report/backend/BE-RWI-055.md), `FE-RWI-053` ✅ [laporan](../task/report/frontend/FE-RWI-053.md) |
| `FR-KEP-012` | PRD `CAP-013` aturan 1; `RWI-DEC-083`, `RWI-DEC-090` | API grup Nursing Care Plan | `AC-CAP013-01` | `BE-RWI-059` |
| `FR-KEP-013` | PRD `CAP-013` aturan 2 | API grup Nursing Care Plan | `AC-CAP013-01` | `BE-RWI-059`, `FE-RWI-054` |
| `FR-KEP-014` | PRD `CAP-013` aturan 5; **`RWI-DEC-091`** | API `GET /items/{itemId}/revisions` | `AC-CAP013-02`, **`RWI-AC-177`** | `BE-RWI-060`, `FE-RWI-054` |
| `FR-KEP-015` | PRD `CAP-013` aturan 2 | State transition bagian 2; Validation `VAL-KEP-16` | Tutup butir tanpa evaluasi ditolak | `BE-RWI-059`, `FE-RWI-054` |
| `FR-KEP-016` | PRD `CAP-013` aturan 6 | State transition bagian 2.1 | `AC-CAP013-03` | `BE-RWI-060` |
| `FR-KEP-017` | `INV-KEP-02`; PRD `CAP-013` AC-03 | State transition bagian 2 | `AC-CAP013-03` | `BE-RWI-060`, `FE-RWI-054` |
| `FR-KEP-018` | PRD `CAP-014` aturan 1, 2 | API grup Nursing Intervention | **Terbukti** — `Tindakan_MenyimpanApaKapanSiapaDanHasilnya` | `BE-RWI-061` ✅ [laporan](../task/report/backend/BE-RWI-061.md), `FE-RWI-055` ✅ |
| `FR-KEP-019` | PRD `CAP-014` aturan 3 | API grup Nursing Intervention | **Terbukti** — `TindakanMendadak_DapatDicatatTanpaRencanaAsuhan` | `BE-RWI-061` ✅ [laporan](../task/report/backend/BE-RWI-061.md), `FE-RWI-055` ✅ |
| `FR-KEP-020` | PRD `CAP-014` aturan 1 | API `Idempotency-Key`; Validation `VAL-KEP-15` | `AC-CAP014-01` — **terbukti terhadap PostgreSQL 16 sungguhan** 8 September 2026, `Failed: 0, Passed: 3, Total: 3`; yang menolak kiriman kembar adalah **database**, bukan pemeriksaan di dalam aplikasi | `BE-RWI-061` ✅ [laporan](../task/report/backend/BE-RWI-061.md), `FE-RWI-055` ✅ |
| `FR-KEP-021` | PRD `CAP-014` aturan 5 | Integration `INT-KEP-05` | `AC-CAP014-02` | `BE-RWI-062`, `FE-RWI-055` |
| `FR-KEP-022` | PRD `CAP-014` AC-03; **`RWI-DEC-091`** | Integration `INT-KEP-06`; Permission `NursingIntervention : Amend`; State transition bagian 3 | `AC-CAP014-03`, **`RWI-AC-176`** | `BE-RWI-062`, `FE-RWI-055` |
| `FR-KEP-023` | PRD `CAP-014` aturan 4; `RWI-RULE-026` | Integration `INT-KEP-03` | Migration kosong; nol tabel baru | `BE-RWI-063` |
| `FR-KEP-024` | `RWI-RULE-023`, `RWI-DEC-032` | API daftar pantau kepatuhan | **Terbukti** — daftar memuat perawatan tanpa pengkajian awal dan yang terlambat; terpasang di layar FE-INP-09 | `BE-RWI-064` ✅ [laporan](../task/report/backend/BE-RWI-064.md), `FE-RWI-056` ✅ [laporan](../task/report/frontend/FE-RWI-056.md) |
| `FR-KEP-025` | `RWI-DEC-032` | — | **Terbukti** — pembedaan mutlak State A (tepat waktu) dan State B (kebijakan kosong) di frontend dan backend | `BE-RWI-064` ✅ [laporan](../task/report/backend/BE-RWI-064.md), `FE-RWI-056` ✅ [laporan](../task/report/frontend/FE-RWI-056.md) |
| `FR-KEP-026` | PRD 16.2 aturan 11; `INV-KEP-03` | Validation `VAL-KEP-18` | **Terbukti** — daftar pantau murni membaca (`GET`) dan tidak menahan tindakan klinis apa pun; tautan langsung ke ruang kerja | `BE-RWI-064` ✅ [laporan](../task/report/backend/BE-RWI-064.md), `FE-RWI-056` ✅ [laporan](../task/report/frontend/FE-RWI-056.md) |
| `FR-KEP-027` | `RWI-DEC-089` | — | **Tidak diuji** | **Nol task** |
| `FR-KEP-028` | `RWI-DEC-089` | — | **Tidak diuji** | **Nol task** |

---

## 3. Prasyarat yang sudah terpenuhi di source

Baris di bawah dibaca langsung dari `BE@7d4bf2b` pada 5 September 2026. Ketiganya mengubah bentuk
roadmap, dan karena itu ikut dicatat di sini — bukan hanya di roadmap backend.

| Prasyarat | Diminta oleh | Dipenuhi oleh | Bukti source |
| --- | --- | --- | --- |
| `INT-KEP-01` resolver konteks klinis rawat inap | `FR-KEP-001`, `FR-KEP-002` | `BE-RWI-039` ✅ pada `dokter-rawat-inap`, 3 September 2026 | `PatientAssessmentController.cs:1040-1064`; `InpatientClinicalContextService.cs` |
| Kolom `InpEpisodeId` dan `AssessmentType` pada tabel pengkajian | `FR-KEP-004`, `FR-KEP-005` | `BE-RWI-040` 🟡 pada `dokter-rawat-inap` | `TrxPatientAssessment.cs:62,73`; `PatientAssessmentType.cs` |
| Penegakan keutuhan bagi jenis `Assessment` dan `Procedure` — substansi `RWI-OQ-051` | `FR-KEP-008`, `FR-KEP-009`, `FR-KEP-022` | `BE-RWI-038` ✅ pada `dokter-rawat-inap` | `ClinicalDocumentIntegrityService.cs:74-87` |

Ketiganya sah karena `INT-DOK-09` memang menetapkan pekerjaan bersama dibuat **sekali**: sub-modul
yang mendarat lebih dulu membuatnya, yang kedua memakainya apa adanya dan menerima baris
dependency — bukan salinan task.

---

## 4. Coverage gap yang diakui

| Gap | Sebabnya | Kapan tertutup |
| --- | --- | --- |
| `FR-KEP-027`, `FR-KEP-028` tanpa task dan tanpa test | `RWI-DEC-089` mengeluarkan `EPIC KEP-06` dari scope rilis pertama secara tertulis | Setelah modul persediaan/aset ada dan `RWI-OQ-048` dibuka kembali — `RWI-AC-171` |
| `AC-CAP027-01` rujukan gizi hanya sebagian | Modul Gizi berstatus `PLANNED`; PRD 23.1 menaruh Nutrition Assessment/Care di sana | Setelah modul Gizi berdiri |
| `AC-CAP027-02` kewenangan ahli gizi | Sama seperti di atas; di luar kendali sub-modul ini | Setelah modul Gizi berdiri |
| Nilai batas waktu klinis belum ada | `RWI-RULE-021` menunggu pemilik klinis; PRD 16.2 aturan 11 menjadikannya konfigurasi | Mekanismenya tetap diuji sekarang; angkanya menyusul tanpa mengubah kode |
| Katalog SDKI/SLKI/SIKI | PRD `CAP-013` aturan 3 bersyarat; `OQ-RI-011` terbuka kembali lewat `RWI-DEC-090` | Setelah Clinical governance menyatakan pemakaiannya |
| Uji ujung ke ujung yang bermakna | `RWI-UI-GAP-007` data master rawat inap belum layak | Setelah data master dirapikan; menahan bukti e2e, bukan pembangunan layar |

**Satu gap tertutup pada revision `2`.** `testing/acceptance-test-matrix.md` bagian 8 memperingatkan
bahwa jalur gagal `FR-KEP-009` "akan lolos padahal seharusnya gagal" selama `Assessment` belum
ditegakkan. Peringatan itu sudah gugur: jenis `Assessment` **sudah** masuk daftar yang ditegakkan,
sehingga skenario itu kini benar-benar dapat membuktikan sesuatu.

Keenam gap sisanya **tercatat**, bukan tersembunyi. Tidak satu pun dari keenamnya menahan 26
functional requirement yang aktif.

---

## 5. Keputusan yang masih terbuka dan pengaruhnya

| Butir | Status | Pengaruh pada roadmap ini |
| --- | --- | --- |
| Approval blueprint `keperawatan` | **Tertutup** — `RWI-DEC-092`, 3 September 2026 | Gerbang yang menahan seluruh 18 task **sudah dicabut** |
| `INT-KEP-01` resolver konteks klinis | **Tertutup** — `BE-RWI-039` ✅ | Tidak lagi menahan `BE-RWI-054` maupun task lain |
| Butir konsistensi mesin koreksi, `04-prd-to-mvp.md` bagian 20.1 | **Tertutup** — `RWI-DEC-091` | Bentuk koreksi terkunci: addendum untuk pengkajian dan tindakan, versi untuk rencana asuhan |
| `DEC-INP-009` supersession `RWI-DEC-004` dan `RWI-DEC-034` | **Tertutup** — `RWI-DEC-090` | `CAP-013` resmi di dalam scope; `EPIC KEP-03` beserta `BE-RWI-059`, `BE-RWI-060`, dan `FE-RWI-054` tetap berdiri |
| `RWI-OQ-051` perluasan penegakan keutuhan | **`open` pada decision log; substansinya sudah terpenuhi di source** | **Tidak memblokir.** Yang tersisa adalah menutup catatannya lewat `/qv-grill`, dengan `BE-RWI-038` sebagai buktinya |
| Bentuk pintu endpoint koreksi | Terbuka, non-blocking | Menyentuh `BE-RWI-057` dan `BE-RWI-062`. Endpoint kontrak **wajib** meneruskan ke mesin yang sudah ada, bukan menulis barisnya sendiri. Diselesaikan `/qv-design` |
| Kepala dokumen empat berkas hulu | Terbuka, non-blocking | Isinya sudah benar; hanya baris `Status: draft` dan nomor revision yang tertinggal. Diselesaikan `/qv-design` |
| `RWI-RULE-021` batas waktu klinis | Belum final | Tidak menahan desain maupun pembangunan; menahan **produksi** |
| `OQ-RI-011` pemakaian katalog SDKI | Terbuka, non-blocking | Menyentuh `BE-RWI-059` dan `FE-RWI-054`; masalah keperawatan ditulis sebagai teks sampai diputuskan |
| Urutan daftar di dalam `FE-INP-09` | **Tertutup** — `FE-RWI-056` | Dipasang sebagai section terintegrasi pada layar `FE-INP-09` existing tanpa mengubah urutan 4 daftar monitoring backend |

---

## 6. Ringkasan hitungan

| Hal | Jumlah |
| --- | ---: |
| Kemampuan dalam scope sub-modul | 5 |
| Kemampuan aktif | 4 |
| Kemampuan `DEFERRED` | 1 |
| Epic aktif | 5 |
| Epic `DEFERRED` | 1 |
| Functional requirement aktif | 26 |
| Functional requirement `DEFERRED` | 2 |
| Task backend | **12** — bertambah satu dari revision `1` |
| Task frontend | 6 |
| Task berstatus `BLOCKED` hari ini | **0 dari 18** — turun dari 17 dari 17 pada revision `1` |
| Task berstatus selesai hari ini | **18 dari 18** — dua belas task backend `BE-RWI-054` s.d. `BE-RWI-065` dan enam task frontend `FE-RWI-051` s.d. `FE-RWI-056`, seluruhnya ✅ per 8 September 2026. Yang terakhir ditutup: `BE-RWI-056` dan `BE-RWI-061` |
| Task berstatus sebagian hari ini | **0 dari 18** — `BE-RWI-056` dan `BE-RWI-061` ditutup ✅ pada 8 September 2026 |
| Tabel baru milik Rawat Inap | **0** |
| Entity baru yang diminta kepada `ClinicalManagement` | 5 — **seluruhnya sudah mendarat**: `MstClinicalAssessmentPolicy` (`BE-RWI-055`) beserta empat tabel `Cli*` dari `BE-RWI-059` s.d. `BE-RWI-061`. Migration-nya **belum diterapkan** ke database bersama, dev, staging, atau production mana pun |
| Butir menu baru | **0** |
| Nilai enum baru pada `ClinicalDocumentKind` | **0** — dibuktikan uji `NolNilaiEnumBaru_PadaJenisDokumenKlinis` |

## Gelombang 1A — traceability koreksi keselamatan

**Ditambahkan 11 September 2026.** Menyerap `RWI-DEC-098` dan `RWI-DEC-100`.

| Kemampuan | Epic | Requirement | Task backend | Task frontend | Status |
| --- | --- | --- | --- | --- | --- |
| `V2-CAP-02` Catatan klinis final tidak dapat disembunyikan | `EPIC KEP-07` | `FR-KEP-029`, `FR-KEP-030` | `BE-RWI-077` ⛔ | **Nol task** — frontend tidak pernah memanggil endpoint itu | ⛔ menunggu approval kontrak `0.4.0` |
| `V2-CAP-03` Penulis klinis dari identitas terautentikasi | `EPIC KEP-08` | `FR-KEP-031` s.d. `FR-KEP-034` | `BE-RWI-078` ⛔ | **Nol task** — perubahan tidak mengubah rupa layar | ⛔ menunggu approval kontrak `0.4.0` |

**Kenapa nol task frontend, dan ini bukan kelalaian.** Pencarian pada `src/` repository frontend
pada `FE@7f6b9356` membuktikan layar **tidak pernah** memanggil `DELETE` tanda vital. Menutup route
itu karenanya tidak menghilangkan tombol mana pun dari layar. Penegakan penulis juga bekerja di
server dan tidak mengubah rupa layar; yang berubah hanya bahwa permintaan tertentu kini ditolak.

| Requirement | Keputusan / aturan | Kontrak `0.4.0` | Acceptance test | Task |
| --- | --- | --- | --- | --- |
| `FR-KEP-029` | `RWI-DEC-098` | `api-contract.md` bagian 0.A.1 | `AC-KEP-040` | `BE-RWI-077` |
| `FR-KEP-030` | `RWI-DEC-098` | `state-transition-matrix.md` bagian 3A.1 | `AC-KEP-042` | `BE-RWI-077` |
| `FR-KEP-031` | `RWI-DEC-100` | `api-contract.md` bagian 0.A.2 | `AC-KEP-046`, `AC-KEP-047` | `BE-RWI-078` |
| `FR-KEP-032` | `RWI-DEC-100` | `permission-audit-matrix.md` bagian 3A.2 | `AC-KEP-044`, `AC-KEP-045` | `BE-RWI-078` |
| `FR-KEP-033` | `RWI-DEC-100` | `permission-audit-matrix.md` bagian 3A.4 | `AC-KEP-048` | `BE-RWI-078` |
| `FR-KEP-034` | `RWI-DEC-100` | `permission-audit-matrix.md` bagian 3A.5 | `AC-KEP-049` | `BE-RWI-078` |

### Coverage gap yang dinyatakan terbuka

| Requirement | Yang belum tercakup test | Sebabnya |
| --- | --- | --- |
| Pembatalan tanda vital final ditolak dan diarahkan ke addendum | Nol acceptance test | `ClinicalDocumentKind.VitalSign` belum termasuk jenis yang ditegakkan mesin keutuhan dokumen, sehingga keadaan "final" belum ada untuk diuji. Dilacak `V2-UNK-01`, pemiliknya `MedicalRecordManagement`. **Ditulis `NOT RUN` apa adanya** pada laporan task, bukan dihilangkan |
