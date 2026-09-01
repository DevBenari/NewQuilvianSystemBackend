# Bank Darah — Prerequisite Readiness

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `3` |
| Status | `DRAFT` |
| Diperbarui | `2026-09-02` — setelah closure pass wawancara selesai |
| Sumber bukti | `02-existing-capability-map.md` revisi 2 dan `00-interview-decisions.md` revisi 2 |
| Backend SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |

Dokumen ini mencatat apa saja yang harus sudah tersedia sebelum Bank Darah bisa dikerjakan, dan
sejauh mana masing-masing sudah siap.

Nilai `capability_status` hanya boleh memakai salah satu dari taksonomi baku berikut:
`READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`.

Nilai `dependency_type` hanya boleh memakai `MODULE_FOUNDATION`, `PHASE`, `INTEGRATION`, atau
`EXTERNAL`.

> **Perubahan pada revisi 3.** Dua `CONFLICT` yang muncul pada revisi 2 sudah ditutup keputusan
> pemilik: `BD-DEP-002` oleh `DEC-BD-014`, dan `BD-DEP-007` oleh `DEC-BD-015`. Tidak ada lagi
> dependency berstatus `CONFLICT`.

## Catatan dependency

| dependency_id | capability_or_module | dependency_type | owner | evidence | capability_status | required_by | blocking_impact | independent_continuation | source_sha | next_owner_or_action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BD-DEP-001` | Data pasien | `MODULE_FOUNDATION` | PatientManagement | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#MstPatient` — lihat `BD-CAP-001` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Simpan `PatientId` sebagai rujukan |
| `BD-DEP-002` | Kunjungan pasien beserta sinyal penutupannya | `MODULE_FOUNDATION` | RegistrationManagement dan InPatientManagement | `TrxPatientEncounter.cs`, `EncounterStatus.cs`, `InpEpisode.cs` — lihat `BD-CAP-002` dan `BD-CAP-003` | `REUSE WITH ADAPTER` | `BD-PH-005` | Tidak memblokir. `DEC-BD-014` menetapkan dua penyesuai: status akhir kunjungan untuk rawat jalan dan IGD, waktu pasien meninggalkan rumah sakit untuk rawat inap | — | `9522caa` | Bank Darah hanya membaca kedua sumber; dilarang mengubah status kunjungan atau episode |
| `BD-DEP-003` | Data dokter | `MODULE_FOUNDATION` | HR — Master Data Workforce | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs#MstDoctor` — lihat `BD-CAP-004` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Simpan `DoctorId` sebagai rujukan |
| `BD-DEP-004` | Kewenangan unit pelayanan memesan darah | `MODULE_FOUNDATION` | HealthServices — Master Data | `MstServiceUnit.cs#MstServiceUnit` sudah memakai pola tanda kemampuan per unit — lihat `BD-CAP-005` | `EXTEND` | `BD-PH-005` | Tidak memblokir perancangan. Perlu satu tanda kemampuan baru bergaya sama agar `DEC-BD-012` terpenuhi tanpa mengunci daftar unit di kode | — | `9522caa` | Ajukan penambahan tanda kemampuan kepada pemilik Master Data |
| `BD-DEP-005` | Katalog komponen darah — PRC, TC, FFP | `MODULE_FOUNDATION` | belum ada pemilik | Tidak ditemukan katalog komponen darah pada `Areas/HealthServices/MasterData/Models/` — lihat `BD-CAP-018` | `MISSING` | `BD-PH-005` | `DEC-BD-005` memakai komponen darah sebagai penanda order ganda, sehingga komponen tidak boleh berupa ketikan bebas | Alur order dan permintaan tetap dapat dirancang | `9522caa` | Tetapkan katalog ini sebagai data induk baru milik Bank Darah pada fase perancangan |
| `BD-DEP-006` | Penyerahan fakta biaya ke Billing | `INTEGRATION` | BillingManagement | `BillingSourceContract.cs` memuat daftar sumber tertutup tanpa Bank Darah; `ClinicalMilestoneFactProducer.cs#EmitChargeEligibilityAsync` — lihat `BD-CAP-015` | `EXTEND` | `BD-PH-005` | BR-BD-004 tidak dapat mengirim biaya ke Billing sebelum konteks sumber Bank Darah ditambahkan | Seluruh alur order dan kantong tetap dapat dirancang | `9522caa` | Minta persetujuan pemilik Billing untuk menambah konteks sumber dan jenis efek biaya Bank Darah |
| `BD-DEP-007` | Sumber sah golongan darah dan Rhesus | `MODULE_FOUNDATION` | Bank Darah sendiri, sesuai `DEC-BD-015` | `MstPatient.BloodType` adalah data induk administratif, bukan hasil pemeriksaan tervalidasi. Tidak ditemukan entity hasil pemeriksaan golongan darah di `LaboratoryManagement` — lihat `BD-CAP-017` | `MISSING` | `BD-PH-005` | Tidak memblokir rancangan. `DEC-BD-015` menetapkan sumber sah berupa hasil pemeriksaan tersendiri milik Bank Darah, dan `DEC-BD-018` menetapkan sampelnya juga milik Bank Darah. Kemampuannya dibangun baru | Seluruh alur inti tetap dapat dirancang | `9522caa` | Siapa yang berhak memvalidasi hasil masih `DEF-BD-004`; mekanik label masih `OQ-BD-011` |
| `BD-DEP-008` | Entri registry kepemilikan modul dan prefix untuk Bank Darah | `EXTERNAL` | Pemilik registry engineering | `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tidak memuat entri Blood Bank maupun Bank Darah | `MISSING` | `BD-PH-007` | Pembuatan entity operasional baru berstatus `BLOCKED` menurut `QBE-MOD-002` dan `QBE-MOD-003`. Prefix tidak boleh disimpulkan dari nama folder menurut `QBE-NAM-004` | Seluruh fase perancangan sampai `BD-PH-006` tetap berjalan | `9522caa` | Daftarkan Area, Module/pemilik, Category, Prefix, dan Lifecycle sebelum berkas model pertama dibuat |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD | `EXTERNAL` | Pemilik kebutuhan | `Bank Darah(1).md`, `Artifact_Bank_Darah_Bagian_Kedua(1).md`, `Bank_Darah_Bagian_Ketiga(1).md` tidak ditemukan di kedua repository | `MISSING` | `BD-PH-003` | Penelusuran dari bukti ke kebutuhan tidak dapat diverifikasi, sehingga gerbang kelengkapan requirement tidak dapat lulus penuh | Perancangan tetap berjalan | `9522caa` | Sediakan berkas bukti atau tunjukkan lokasinya |
| `BD-DEP-010` | Pola otorisasi tingkat controller dan tindakan | `MODULE_FOUNDATION` | Platform backend | `Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`; contoh pemakaian `LabOrderController.cs` dengan `[AccessPermission("LabOrder", "Read")]` — lihat `BD-CAP-013` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Petakan kelompok kewenangan BRD §14 ke pola ini |
| `BD-DEP-011` | Kontrak response dan pagination bersama | `MODULE_FOUNDATION` | Platform backend | `Responses/ApiResponse.cs`, `Responses/PagedResult.cs` — lihat `BD-CAP-012` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Pakai apa adanya saat kontrak API disusun |
| `BD-DEP-012` | Pemisahan data per fasilitas kesehatan | `MODULE_FOUNDATION` | Platform backend | `MstHospitalSite.cs` hanya dipakai Corporate; tidak ada rujukan `HospitalSite` di seluruh `Areas/HealthServices/`; `TrxPatientEncounter` tidak punya kolom fasilitas — lihat `BD-CAP-022` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir. Layanan kesehatan Quilvian saat ini tidak memisahkan data per fasilitas | — | `9522caa` | Bank Darah mengikuti pola yang sama; jangan menambah pemisahan baru |
| `BD-DEP-013` | Pola pesanan klinis, riwayat perpindahan status, dan kunci konkurensi | `MODULE_FOUNDATION` | LaboratoryManagement | `LabOrder.cs`, `TrxLabSpecimen.cs`, `TrxLabTransitionHistory.cs` — lihat `BD-CAP-007`, `BD-CAP-008`, `BD-CAP-009`, `BD-CAP-010` | `REUSE WITH ADAPTER` | `BD-PH-005` | Tidak memblokir. Dipakai sebagai pola, bukan sebagai entity bersama | — | `9522caa` | Ikuti bentuknya saat merancang entity Bank Darah |
| `BD-DEP-014` | Komponen dasar tampilan frontend | `MODULE_FOUNDATION` | Frontend V2 | `src/components/features/base-features/` memuat `hero.jsx`, `data-table.jsx`, `data-filter.jsx`, `filter-select.jsx`, `filter-date-picker.jsx`, `base-button.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx` — lihat `BD-CAP-021` | `READY TO REUSE` | `BD-PH-008` | Tidak memblokir | — | `afbb8ab` | Dilarang membuat komponen dasar tandingan |
| `BD-DEP-015` | Integrasi HCLAB — workstation `BANK DARAH`, kode `BBW`, Lab Sec `GL` | `EXTERNAL` | belum diketahui | Tidak ditemukan rujukan HCLAB, `BBW`, maupun `GL` pada `Areas/HealthServices/LaboratoryManagement/` — lihat `BD-CAP-024` | `UNKNOWN` | `BD-PH-005` | Hanya memblokir BR-BD-014, yang memang baru menuntut dokumen penelusuran dan bukan implementasi | Seluruh scope lain berjalan | `9522caa` | Sediakan bukti integrasi dari luar repository |

## Ringkasan kesiapan

| Status | Jumlah | Dependency |
| --- | --- | --- |
| `READY TO REUSE` | 6 | `BD-DEP-001`, `003`, `010`, `011`, `012`, `014` |
| `REUSE WITH ADAPTER` | 2 | `BD-DEP-002`, `013` |
| `EXTEND` | 2 | `BD-DEP-004`, `006` |
| `CONFLICT` | 0 | keduanya ditutup pada closure pass 2026-09-02 |
| `MISSING` | 4 | `BD-DEP-005`, `007`, `008`, `009` |
| `UNKNOWN` | 1 | `BD-DEP-015` |
| `REPAIR` | 0 | — |

## Apa artinya bagi fase

| Fase | Terpengaruh? | Alasan |
| --- | --- | --- |
| `BD-PH-002` audit kemampuan | Selesai | Delapan `UNKNOWN` sudah berubah menjadi status berbukti |
| `BD-PH-003` gerbang kelengkapan | Sebagian | `BD-DEP-009` menghalangi kelulusan penuh, tetapi penilaian tetap boleh dijalankan dan mencatat kekurangannya |
| `BD-PH-005` perancangan blueprint | Hampir penuh | Tidak ada lagi `CONFLICT`. Yang tersisa hanya `BD-DEP-006` yang menunggu persetujuan pemilik Billing (`DEC-BD-016`), sehingga penyerahan biaya belum dapat dikontrakkan. Seluruh alur order, permintaan PMI, penerimaan, alokasi, pemberian, kedaluwarsa, dan penyelesaian kantong dapat dirancang penuh |
| `BD-PH-007` implementasi backend | **Terblokir** | `BD-DEP-008` membuat pembuatan entity operasional baru berstatus `BLOCKED` |

Sesuai aturan gerbang dependency, `MISSING` dan `UNKNOWN` hanya memblokir fase yang membutuhkannya.
Tiga `MISSING` yang tersisa — katalog komponen darah, hasil golongan darah Bank Darah, dan seluruh
kapabilitas Bank Darah — memang kemampuan yang akan dibangun modul ini sendiri, jadi keberadaannya
wajar dan bukan penghalang. Dua `MISSING` yang benar-benar menahan adalah `BD-DEP-008` entri registry
dan `BD-DEP-009` berkas bukti, dan keduanya menahan implementasi serta penelusuran bukti, bukan
perancangan. Status modul tetap `PARTIAL`.
