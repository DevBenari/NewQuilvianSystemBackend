# Integration Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | — belum |
| Tanggal | 2 September 2026 |

---

## 0. Kenapa dokumen ini menentukan

Sub-modul ini tidak memiliki satu tabel pun dan menyentuh **empat** modul lain:
`ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, dan `BillingManagement`.
Hampir seluruh wujudnya adalah integrasi.

---

## 1. `INT-DOK-01` — Pelonggaran konteks klinis pada **konsultasi** ★ penghalang utama

| Field | Isinya |
| --- | --- |
| Arah | Rawat Inap **meminta** perubahan pada `ClinicalManagement` |
| Bentuk | Sinkron, di dalam proses yang sama |
| Pemilik perubahan | `ClinicalManagement` — Muhammad Hamzah, disetujui `RWI-DEC-062` |
| Yang diminta | Validasi tanpa antrean pada `DoctorConsultationController` menerima encounter yang punya `InpEpisode` berstatus `Admitted`, setara cara `EmgVisit` dipakai untuk IGD |
| Hubungan dengan `keperawatan` | **Kembaran.** `keperawatan/contracts/integration-contract.md` `INT-KEP-01` meminta hal yang sama pada `PatientAssessmentController`. Keduanya **wajib dikerjakan bersama** — lihat 1.1 |
| Yang **tidak** berubah | Perilaku rawat jalan dan medical check-up; nol kolom |
| Bila gagal | Konsultasi ditolak `422`; tidak ada keadaan setengah jadi |
| Traceability | PRD 30.3; `RWI-DEC-062`, `RWI-DEC-070`, `RWI-DEC-080` |

### 1.1 Kenapa keduanya wajib bersama

| Bila hanya pengkajian dibuka | Bila hanya konsultasi dibuka |
| --- | --- |
| Perawat dapat mencatat; **dokter tidak dapat sama sekali** — SOAP, diagnosis, resep, dan tindakan seluruhnya lahir dari konsultasi | Dokter dapat mencatat; perawat tidak. Pengkajian awal keperawatan tetap di kertas |

Keduanya adalah satu pekerjaan yang kebetulan berada di dua berkas. Memecahnya menjadi dua
gelombang menghasilkan setengah ruang kerja klinis yang tidak dapat dipakai siapa pun.

---

## 2. `INT-DOK-02` — Pelonggaran batas jumlah konsultasi dan resep

| Field | Isinya |
| --- | --- |
| Arah | Rawat Inap **meminta** perubahan pada `ClinicalManagement` dan `PharmacyManagement` |
| Yang diminta | Untuk kunjungan bertipe rawat inap: batas **satu konsultasi per kunjungan** dan **satu resep aktif per konsultasi** tidak berlaku |
| Dasarnya | `RWI-RULE-026` aturan 4 dan 5; `RWI-DEC-038`, diperluas `RWI-DEC-070` |
| Keadaan keputusan | **Sudah `approved` sejak 2026-08-21.** Yang belum ada kodenya |
| Kenapa wajib | Pasien dirawat berhari-hari. Tanpa pelonggaran ini, dokter hanya dapat menulis **satu** SOAP dan **satu** resep untuk seluruh masa perawatan |
| Yang **tidak** berubah | Rawat jalan dan medical check-up tetap dibatasi seperti sekarang |

---

## 3. `INT-DOK-03` — Konteks episode dan kewenangan DPJP

| Field | Isinya |
| --- | --- |
| Arah | **Baca** dari `episode-rawat-inap` |
| Yang dibaca | Pasien, lokasi, status episode, dan **`InpDoctorAssignment` yang berlaku pada tanggal itu** |
| Kenapa berperiode | DPJP dapat berganti di tengah perawatan. Kewenangan menulis pada tanggal tertentu ditentukan penugasan yang berlaku **pada tanggal itu**, bukan penugasan terkini |
| Arah tulis | **Tidak ada.** Sub-modul ini tidak pernah mengubah episode maupun penugasan |
| Bila gagal | Ruang kerja menampilkan keadaan gagal; seluruh tombol tulis nonaktif |

---

## 4. `INT-DOK-04` — Berbagi enum dan tabel dengan `keperawatan`

Bukan integrasi antar modul, melainkan **koordinasi antar sub-modul**. Dicatat di sini karena
tidak ada berkas lain yang memergokinya.

| Yang dibagi | Diminta lebih dulu oleh | Yang harus dilakukan sub-modul kedua |
| --- | --- | --- |
| `PatientAssessmentType` | `keperawatan` — `Initial`, `Reassessment`, `DailyReassessment`, `DischargePlanning` | Menambah `MedicalInitial` dan `MedicalReassessment`, **bukan** membuat enum kedua |
| Kolom `InpEpisodeId`, `DueAt`, `PolicyId`, `AmendedAt`, `AmendedByUserId` pada `TrxPatientAssessment` | `keperawatan` | Memakai apa adanya. **Tidak meminta duplikatnya** |
| `MstClinicalAssessmentPolicy` | `keperawatan` | Menambah baris kebijakan untuk jenis kajian medis |
| Enum status pengiriman tagihan | `keperawatan` sebagai `NursingBillingDispatchStatus` | **Diusulkan berganti nama** menjadi `ClinicalBillingDispatchStatus` karena kini dipakai dua profesi |
| `TrxPatientIntegratedProgressNote` | Dipakai keduanya | **Kontraknya milik sub-modul ini** (`CAP-021`). `keperawatan` menulis sebagai penulis, bukan pemilik kontrak |

> **Siapa pun yang mendarat lebih dulu membuat, yang kedua menambah.** Bila keduanya dikerjakan
> berbarengan, `INT-DOK-04` wajib dibaca kedua pelaksana supaya tidak lahir dua enum kembar.

---

## 5. `INT-DOK-05` — Resep ke Farmasi

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** pesanan resep; **baca** status pemenuhannya |
| Pemilik | `PharmacyManagement` |
| Keadaan modul tujuan | **Lengkap dan berjalan** — resep, item, racikan, review, penyiapan, template, ruang kerja |
| Idempotency | Wajib — PRD `CAP-023` aturan 8 |
| Yang **dilarang** | Menulis status pemenuhan apa pun. `INV-DOK-04`, PRD aturan 6 |
| Obat pulang | Dikirim sebagai `PrescriptionOrderType = Discharge`, bukan sebagai daftar terpisah — `RWI-DEC-046` |
| Bila gagal | Resep tidak terbentuk; dokter melihat penolakan dan dapat mengulang dengan kunci yang sama |

---

## 6. `INT-DOK-06` — Pesanan dan hasil laboratorium

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** pesanan; **baca** status dan hasil terverifikasi |
| Pemilik | `LaboratoryManagement` |
| Keadaan modul tujuan | **Ada dan berjalan** — `LabOrder`, spesimen, riwayat transisi, dua controller |
| **Temuan** | `LabOrder` terikat `EncounterId` saja; **tidak ada gerbang antrean**. Pemesanan lab rawat inap sudah mungkin hari ini |
| Yang diminta | Satu kolom `InpEpisodeId` supaya `AC-CAP015-01` dapat dibuktikan |
| Yang **dilarang** | Menulis maupun menyalin hasil. `INV-DOK-05`, `AC-CAP015-02` |

---

## 7. `INT-DOK-07` — Radiologi

| Field | Isinya |
| --- | --- |
| Keadaan modul tujuan | **Tidak ada.** Pencarian `Areas/HealthServices/*Radiolog*` nihil |
| Yang berlaku sementara | Pemeriksaan radiologi dipesan di luar sistem, sebagaimana hari ini |
| Kapan dibuat | Setelah modul Radiologi berdiri |
| Akibatnya | `CAP-015` masuk MVP **sebagian**: laboratorium ya, radiologi ditunda |

---

## 8. `INT-DOK-08` — Pemicu tagihan tindakan

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** ke `BillingManagement` |
| Idempotency | Wajib — PRD `CAP-024` aturan 5 |
| Bila gagal | `BillingDispatchStatus` menjadi `Failed`; **catatan klinisnya tetap tersimpan** |
| Keadaan modul tujuan | Belum punya kemampuan transaksi. Sampai ada, status tetap `Pending` dan tidak ada yang hilang |

---

## 9. Integrasi yang **tidak** dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Penulisan status penyerahan obat | `INV-DOK-04` |
| Penyalinan hasil laboratorium ke tabel Rawat Inap | `INV-DOK-05`, `AC-CAP015-02` |
| Penghitungan visite dari catatan SOAP | `INV-DOK-03` |
| Integrasi radiologi | Modulnya belum ada |
