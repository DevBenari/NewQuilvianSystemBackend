# Requirement Traceability — Modul Radiologi

| Field | Value |
|---|---|
| Traceability ID | `RAD-TRACE-001` |
| Revision | `1` |
| Status | `approved` |
| Blueprint | `RAD-BP-001` revision 9, status `approved` |
| Backend SHA baseline | `0e2eb105` |
| Frontend SHA baseline | `f66ed1885` |
| Tanggal | 2026-09-10 |

Dokumen ini menjawab satu pertanyaan: **setiap keputusan bisnis berakhir di task yang mana, dan
dibuktikan uji yang mana?**

Bila sebuah keputusan tidak punya task, ia belum akan terwujud. Bila sebuah task tidak punya
keputusan asal, ia lahir dari selera, bukan kebutuhan.

---

## 1. Dari Keputusan ke Task

### Keputusan warisan

| Decision | Isinya | Task backend | Task frontend | Uji |
|---|---|---|---|---|
| `RJ-BIL-GATE-DEC-004` | Siklus hidup pesanan, study, dan laporan; invariant tagih | `BE-RAD-07` s/d `BE-RAD-10` | `FE-RAD-10` s/d `FE-RAD-12` | `RAD-TEST-001` bagian 3, 4, 7 |
| `RJ-BIL-DEC-014` | Sifat fail-closed; aturan keselamatan sebagai data yang dapat diubah | `BE-RAD-01` **selesai sebagian 2026-09-10**, `BE-RAD-06` **selesai 2026-09-10**, `BE-RAD-15` | `FE-RAD-04` | `RAD-TEST-001` bagian 1, 2; bukti `BE-RAD-06`: `RadiologySafetyGateTests` 20 lulus |
| `RJ-BIL-GATE-DEC-005` | Bahan terpakai dicatat sebagai jumlah, bukan nominal | Sudah ada di source | `FE-RAD-09` | `RAD-TEST-001` bagian 7 |

### Keputusan modul Radiologi

| Decision | Isinya | Task backend | Task frontend | Uji |
|---|---|---|---|---|
| `RAD-DEC-001` | Batas scope; Rilis 1 sampai hasil bacaan dirilis | Seluruh roadmap | Seluruh roadmap | — |
| `RAD-DEC-002` | Enam modalitas; butir keselamatan berbeda per alat | `BE-RAD-04`, `BE-RAD-15` | `FE-RAD-02`, `FE-RAD-08` | AC-6, `UAT-10` |
| `RAD-DEC-003` | Radiolog boleh mengesahkan drafnya sendiri; bukan-radiolog tidak | `BE-RAD-08` | `FE-RAD-11` | AC-1 s/d AC-4, `UAT-04`, `UAT-05` |
| `RAD-DEC-004` | Temuan kritis ditandai, dikirim, diakui | **Tidak ada task** | **Tidak ada task** | `S11` tertahan `DEC-RAD-002` |
| `RAD-DEC-005` | Aturan keselamatan disusun admin, disahkan penanggung jawab klinis | `BE-RAD-01` **selesai sebagian 2026-09-10**, `BE-RAD-06` **selesai 2026-09-10** — AC-12 terpenuhi, `BE-RAD-02` **selesai 2026-09-10** — AC-13 s/d AC-15 terpenuhi, `BE-RAD-03` **selesai 2026-09-10** — AC-17 terpenuhi di backend | `FE-RAD-04` | AC-12 s/d AC-17, `UAT-03`; AC-12 dibuktikan `RadiologySafetyGateTests`; AC-13 s/d AC-15 dibuktikan `RadSafetyPolicyServiceTests`; AC-17 dibuktikan `RadSafetyRuleCatalogTests` — 42 uji radiologi lulus |
| `RAD-DEC-006` | Rekam medis membaca langsung, tanpa salinan | `BE-RAD-11` | `FE-RAD-13` | AC-19 s/d AC-21, `UAT-08`, `UAT-09` |
| `RAD-DEC-007` | Cara menyelesaikan konflik registry | **Selesai 2026-09-10** | — | Entri riwayat registry |
| `RAD-DEC-008` | Pelewatan gerbang darurat | **Tidak ada task** | **Tidak ada task** | `S5` tertahan `DEC-RAD-001` |
| `RAD-DEC-009` | Titik sentuh IGD | **Selesai sebagian 2026-09-10** — teks diperbaiki | Teks layar IGD diperbaiki | Penyambungan endpoint milik modul IGD |
| `RAD-DEC-010` | Pemberitahuan temuan kritis saat ganti jaga | **Tidak ada task** | **Tidak ada task** | Mengikuti `S11` |
| `RAD-DEC-011` | Status `Draft` pesanan tidak dipakai | Tidak ada pekerjaan — nilainya dibiarkan | — | AC-34, AC-35 |
| `RAD-DEC-012` | Daftar kerja per alat, tanpa tabel baru | `BE-RAD-13` | `FE-RAD-07` | AC-36 s/d AC-38, `UAT-13`, `UAT-14` |
| `RAD-DEC-013` | Penanda cito pada pesanan | `BE-RAD-12` | `FE-RAD-05`, `FE-RAD-07` | AC-39 s/d AC-42 |
| `RAD-DEC-014` | Kepemilikan modul pada Yoga Aji Pratama | Tata kelola, bukan task | — | Manifest revision 9 |
| `RAD-DEC-015` | Peran dikenali lewat hak akses penanda | `BE-RAD-08`, `BE-RAD-14` | `FE-RAD-11` | `RAD-PERM-001` bagian 9 |

---

## 2. Dari Kemampuan ke Task

Menghubungkan `01-existing-capability-map.md` dengan pekerjaan yang direncanakan.

| ID kemampuan | Status audit | Task | Perubahan status setelah task |
|---|---|---|---|
| `RAD-CAP-001` Kelola alat | `Repair` | `BE-RAD-04`, `FE-RAD-02` | Menjadi lengkap |
| `RAD-CAP-002` Kelola butir keselamatan | `Repair` | `BE-RAD-05`, `FE-RAD-03` | Menjadi lengkap |
| `RAD-CAP-003` Kelola aturan keselamatan | `Repair` — **risiko tertinggi** | `BE-RAD-01` **selesai sebagian**, `BE-RAD-02` dan `BE-RAD-03` **selesai 2026-09-10**, `FE-RAD-04` | **Backend lengkap 2026-09-10** — aturan dapat disusun, disahkan, ditolak, dihentikan, dan dilihat cakupannya lewat sebelas endpoint. Sisa: layar `FE-RAD-04` dan pengisian data awal `BE-RAD-15` |
| `RAD-CAP-007` Pesanan radiologi | `Ready to reuse` | `FE-RAD-05`, `FE-RAD-06` | Tetap; ditambah layar |
| `RAD-CAP-008` Status `Draft` pesanan | `Extend` | Tidak dikerjakan — `RAD-DEC-011` | Tetap tidak dipakai |
| `RAD-CAP-009` Study dan pengambilan citra | `Ready to reuse` | `FE-RAD-08`, `FE-RAD-09` | Tetap; ditambah layar |
| `RAD-CAP-010` Hasil bacaan | `Missing` | `BE-RAD-07` s/d `BE-RAD-09`, `FE-RAD-10`, `FE-RAD-11` | Menjadi ada |
| `RAD-CAP-011` Koreksi berversi | `Missing` | `BE-RAD-10`, `FE-RAD-12` | Menjadi ada |
| `RAD-CAP-012` Temuan kritis | `Missing` | **Tidak ada task** | Tetap `Missing` — `POST-MVP` |
| `RAD-CAP-013` Daftar kerja petugas | `Missing` | `BE-RAD-12`, `BE-RAD-13`, `FE-RAD-07` | Menjadi ada |
| `RAD-CAP-015` Gerbang keselamatan | `Ready to reuse` | `BE-RAD-06` **selesai 2026-09-10** | **Sudah berpindah** ke `RuleStatus`; sifat fail-closed tidak berubah |
| `RAD-CAP-018` Slot dokumen rekam medis | `Reuse with adapter` | `BE-RAD-11`, `FE-RAD-13` | Tetap slot berkas luar; hasil dibaca langsung |
| `RAD-CAP-020` Fakta kelayakan tagih | `Ready to reuse` | Tidak ada pekerjaan | Tetap |
| `RAD-CAP-025` Uji kontrak hak akses | `Missing` | `BE-RAD-14` | Menjadi ada |
| `RAD-CAP-027` Registry `Rad` | `Conflict` | **Selesai 2026-09-10** | Menjadi `ACTIVE` |
| `RAD-CAP-029` Frontend Radiologi | `Missing` | `FE-RAD-01` s/d `FE-RAD-13` | Menjadi ada |
| `RAD-CAP-030` Pola frontend Laboratorium | `Reuse with adapter` | `FE-RAD-01` | Ditiru, bukan diubah |
| `RAD-CAP-031` Wadah menu sidebar | `Reuse with adapter` | `FE-RAD-01` | Terisi |
| `RAD-CAP-033` Titik sentuh IGD | `Conflict` | **Selesai sebagian 2026-09-10** | Teks benar; penyambungan menunggu |

---

## 3. Dari Slice ke Task

| Slice | Nama | Task backend | Task frontend |
|---|---|---|---|
| `S1` | Pesanan radiologi | `BE-RAD-12` | `FE-RAD-05`, `FE-RAD-06` |
| `S2` | Study dan pengambilan citra | — | `FE-RAD-08`, `FE-RAD-09` |
| `S3` | Penilaian gerbang keselamatan | `BE-RAD-06` **selesai 2026-09-10** | `FE-RAD-08` |
| `S4` | Pengelolaan aturan keselamatan | `BE-RAD-01` s/d `BE-RAD-03` **selesai 2026-09-10**, `BE-RAD-15` | `FE-RAD-04` |
| `S6` | Mutu, pengulangan, penghentian | — | `FE-RAD-09` |
| `S7` | Pencatatan pemakaian bahan | — | `FE-RAD-09` |
| `S8` | Fakta kelayakan tagih | — | — |
| `S9` | Hasil bacaan radiolog | `BE-RAD-07` s/d `BE-RAD-09` | `FE-RAD-10`, `FE-RAD-11` |
| `S10` | Koreksi hasil berversi | `BE-RAD-10` | `FE-RAD-12` |
| `S12` | Daftar kerja petugas | `BE-RAD-13` | `FE-RAD-07` |
| `S13` | Data induk alat pencitraan | `BE-RAD-04`, `BE-RAD-05` | `FE-RAD-02`, `FE-RAD-03` |
| `S14` | Penyajian hasil ke rekam medis | `BE-RAD-11` | `FE-RAD-13` |
| `S15` | Tampilan frontend | — | Seluruh task frontend |
| ~~`S5`~~ | ~~Pelewatan gerbang darurat~~ | **Tidak direncanakan** | **Tidak direncanakan** |
| ~~`S11`~~ | ~~Temuan kritis~~ | **Tidak direncanakan** | **Tidak direncanakan** |

---

## 4. Dari Skenario UAT ke Task

| UAT | Yang dibuktikan | Task yang wajib selesai |
|---|---|---|
| `UAT-01` | Satu pemeriksaan dari pesanan sampai hasil dibaca | Seluruh `MVP-0` s/d `MVP-3` |
| `UAT-02` | Alat belum punya aturan keselamatan | `BE-RAD-06`, `FE-RAD-04`, `FE-RAD-08` |
| `UAT-03` | Admin mengesahkan aturannya sendiri | `BE-RAD-02`, `FE-RAD-04` |
| `UAT-04` | Radiolog mengesahkan bacaannya sendiri | `BE-RAD-08`, `FE-RAD-11` |
| `UAT-05` | Residen mengesahkan drafnya sendiri | `BE-RAD-08`, `FE-RAD-11` |
| `UAT-06` | Koreksi bacaan yang sudah dirilis | `BE-RAD-10`, `FE-RAD-12` |
| `UAT-07` | Mengubah bacaan yang sudah dirilis | `BE-RAD-10` |
| `UAT-08` | Rekam medis saat Radiologi bermasalah | `FE-RAD-13` |
| `UAT-09` | Koreksi langsung terlihat di rekam medis | `BE-RAD-11`, `FE-RAD-13` |
| `UAT-10` | Butir keselamatan berbeda antar alat | `BE-RAD-15`, `FE-RAD-08` |
| `UAT-11` | Dua radiolog mengesahkan bersamaan | `BE-RAD-08`, `FE-RAD-11` |
| `UAT-12` | Menonaktifkan alat yang masih dipakai | `BE-RAD-04`, `FE-RAD-02` |
| `UAT-13` | Pesanan cito didahulukan | `BE-RAD-13`, `FE-RAD-07` |
| `UAT-14` | Daftar kerja tanpa memilih alat | `BE-RAD-13`, `FE-RAD-07` |

Seluruh 14 skenario punya task penanggung. Tidak ada UAT yang menggantung.

---

## 5. Definition of Done Modul ke Task

Dari `04-prd-to-mvp.md` bagian 19.

| Butir Definition of Done | Task yang membuktikannya | Keadaan |
|---|---|---|
| Registry mencatat `Rad` `ACTIVE` beserta entri riwayat | — | **Selesai 2026-09-10** |
| Aturan keselamatan dapat disusun, diajukan, disahkan, ditolak | `BE-RAD-02` dan `BE-RAD-03` **selesai 2026-09-10**, `FE-RAD-04` | **Backend selesai** — sebelas endpoint tersedia dan terbukti 42 uji. Sisa hanya layarnya |
| Setiap alat punya sekurang-kurangnya satu aturan aktif | `BE-RAD-15` | Direncanakan |
| Seluruh tabel master MVP terisi | `BE-RAD-15` | Direncanakan |
| Satu pasien berjalan dari pesanan sampai hasil dibaca | Seluruh `MVP-0` s/d `MVP-3` | Direncanakan |
| Bacaan non-spesialis tidak dapat dirilis tanpa diperiksa spesialis | `BE-RAD-08`, `BE-RAD-14` | Direncanakan |
| Bacaan yang sudah dirilis tidak dapat diubah | `BE-RAD-10` | Direncanakan |
| Versi lama tetap dapat dibaca | `BE-RAD-10`, `FE-RAD-12` | Direncanakan |
| Rekam medis menampilkan versi berlaku tanpa penyalinan | `BE-RAD-11`, `FE-RAD-13` | Direncanakan |
| Gangguan dibedakan dari kekosongan | `FE-RAD-13` | Direncanakan |
| Fakta tagih terbit tepat satu kali per study layak | Sudah ada di source | **Sudah berjalan** |
| Hasil bacaan dirilis tidak menerbitkan fakta tagih | Sudah ada di source | **Sudah berjalan** |
| Uji kontrak hak akses ada dan lulus | `BE-RAD-14` | Direncanakan |
| Kolom sensitif tidak muncul di log | `BE-RAD-08`, `BE-RAD-14` | Direncanakan |
| Migration 1 tidak mematikan pemeriksaan berjalan | `BE-RAD-01` | Direncanakan |
| Dua orang tidak dapat sama-sama berhasil | `BE-RAD-08`, `FE-RAD-11` | Direncanakan |

---

## 6. Yang Sengaja Tidak Punya Task

| Yang ditinggalkan | Alasan | Pemilik keputusan |
|---|---|---|
| `S5` pelewatan gerbang darurat | `DEC-RAD-001` — tanda tangan klinis belum ada | Tata kelola klinis, lewat `RAD-REQ-002` |
| `S11` temuan kritis | `DEC-RAD-002` — daftar temuan kritis belum ada | Tata kelola klinis, lewat `RAD-REQ-002` |
| Pemantauan keterlambatan cito | `RAD-OPEN-009` — batas waktu belum ditetapkan | Tata kelola klinis |
| Penyambungan pemesanan IGD | Menunggu frontend Radiologi Rilis 1 | Pemilik modul IGD |
| Integrasi PACS dan DICOM | Di luar scope `RAD-DEC-001` | — |
| Penghapusan dua endpoint data induk lama | Task tersendiri setelah konsumen berpindah | Pemilik modul |
| Penyegaran plugin cache registry | `RAD-OPEN-010` | Pemegang suite Skill |

---

## 7. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-10 | Traceability pertama. 15 keputusan, 19 kemampuan, 14 slice, 14 UAT, dan 16 butir Definition of Done dipetakan ke 28 task. | `draft` |
