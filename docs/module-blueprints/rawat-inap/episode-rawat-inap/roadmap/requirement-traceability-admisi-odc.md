# Traceability Delivery Pendaftaran Admisi Pasien One Day Care (ODC)

| Metadata | Nilai |
| --- | --- |
| blueprint_id / planning_id | `RWI-BP-001` / `ODC-DEL-20261017-01` |
| revision / tanggal / status | 1 / 2026-10-17 Asia/Jakarta / APPROVED_DESIGN_IMPLEMENTED; 2 task backend dan 3 task frontend selesai source code (100% complete) |
| approval | Desain disetujui pengguna 17 Oktober 2026 lewat `/plan-module-delivery` menyerap `RWI-DEC-327` s.d. `332`. Kontrak target `0.15.0` |
| kontrak | `0.15.0` (Pendaftaran Admisi Pasien ODC Rawat Inap) |
| masukan canonical | Decision log 53; manifest anak 14; flowcharts/20; contracts/api-contract.md; contracts/validation-matrix.md |
| source backend / frontend | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` / `fc9aea6d62418460493899cb669832f032e48328` |
| hash snapshot | [admisi-odc-delivery-lock.json](admisi-odc-delivery-lock.json) |
| roadmaps | [backend](backend-roadmap-admisi-odc.md) / [frontend](frontend-roadmap-admisi-odc.md) |

---

## 1. Matriks Keterlacakan Kebutuhan Fungsional (Requirement to Task)

Bukti backend memakai requirement-ke-bukti-verifikasi menurut `rules/backend/TEST_POLICY.md`; tidak adanya automated test backend bukan gap. Seluruh task backend dan frontend telah diimplementasikan pada source code.

| Functional Requirement | Perilaku Terukur | Desain / Kontrak | Backend Task | Frontend Task | Bukti Diharapkan / Status |
|---|---|---|:---:|:---:|---|
| `FR-ODC-001` | Opsi ke-4 `One Day Care (ODC)` tersedia pada pilihan tipe pasien Langkah 2 Wizard Admisi | Frontend 19.1; PRD 29.3; Flowchart 20 | — | `FE-RWI-260` ✅ | RWI-AC-463; UAT-ODC-01; TERVERIFIKASI FE ([FE-RWI-260](../task/report/frontend/FE-RWI-260.md)) |
| `FR-ODC-002` | Penetapan DPJP Utama rawat inap wajib saat mendaftarkan pasien tipe ODC | Backend 18.2; Frontend 19.1; PRD 29.3 | `BE-RWI-249` ✅ | `FE-RWI-260` ✅ | RWI-AC-464; VAL-ADM-13; UAT-ODC-01; TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)); TERVERIFIKASI FE ([FE-RWI-260](../task/report/frontend/FE-RWI-260.md)) |
| `FR-ODC-003` | Filter otomatis ketersediaan tempat tidur khusus ODC (`isOdcBed=true`) pada Langkah 6 Wizard | Frontend 19.2; API Contract 16; Flowchart 20 | — | `FE-RWI-261` ✅ | RWI-AC-465; UAT-ODC-01; TERVERIFIKASI FE ([FE-RWI-261](../task/report/frontend/FE-RWI-261.md)) |
| `FR-ODC-004` | Penolakan penempatan bed non-ODC untuk pasien ODC (`ODC_BED_REQUIRED`) dan penolakan bed ODC untuk pasien reguler (`ODC_BED_RESERVED`) | Backend 18.2; Validation Matrix (`VAL-ADM-10`, `11`) | `BE-RWI-249` ✅ | `FE-RWI-261` ✅ | RWI-AC-465, 466; VAL-ADM-10, 11; UAT-ODC-02; TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)); TERVERIFIKASI FE ([FE-RWI-261](../task/report/frontend/FE-RWI-261.md)) |
| `FR-ODC-005` | Pengecualian validasi penempatan bed ODC saat transfer bed management pasien ODC > 24 jam | Backend 18.2; State Transition Matrix; Flowchart 20 | `BE-RWI-249` ✅ | — | RWI-AC-467; VAL-ADM-12; UAT-ODC-03; TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)) |
| `FR-ODC-006` | Penyesuaian formulir digital & lembar cetak General Consent dengan judul dinamis ODC dan klausul edukasi 24 jam | Frontend 19.3; PRD 29.3; Validation Matrix (`VAL-ADM-14`) | — | `FE-RWI-262` ✅ | RWI-AC-468; VAL-ADM-14; UAT-ODC-01; TERVERIFIKASI FE ([FE-RWI-262](../task/report/frontend/FE-RWI-262.md)) |
| `FR-ODC-007` | Ekstensi endpoint `GET /bed-occupancies/available-beds` mendukung filter query `isOdcBed` / `isOdcRoom` dan field respon `isOdcBed` | Backend 18.1; API Contract Bagian 16 | `BE-RWI-248` ✅ | — | RWI-AC-465; VAL-ADM-10; UAT-ODC-01; TERVERIFIKASI BE ([BE-RWI-248](../task/report/backend/BE-RWI-248.md)) |

---

## 2. Matriks Kriteria Penerimaan (Acceptance Criteria Index)

| Kode AC | Pernyataan Kriteria Penerimaan | Task Penanggung Jawab | Status Bukti |
|---|---|:---:|:---:|
| `RWI-AC-463` | Wizard pendaftaran admisi menyajikan pilihan tipe pasien "One Day Care (ODC)" di samping Umum, BPJS, dan Asuransi pada Langkah 2 | `FE-RWI-260` ✅ | TERVERIFIKASI FE ([FE-RWI-260](../task/report/frontend/FE-RWI-260.md)) |
| `RWI-AC-464` | Admisi pasien ODC membentuk episode rawat inap aktif (< 24 jam) dan mewajibkan penetapan DPJP Utama penanggung jawab | `BE-RWI-249` ✅, `FE-RWI-260` ✅ | TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)); TERVERIFIKASI FE ([FE-RWI-260](../task/report/frontend/FE-RWI-260.md)) |
| `RWI-AC-465` | Langkah 6 wizard hanya menyajikan tempat tidur yang terdaftar sebagai bed ODC (`isOdcBed=true`) dan backend menolak penempatan bed reguler | `BE-RWI-248` ✅, `BE-RWI-249` ✅, `FE-RWI-261` ✅ | TERVERIFIKASI BE ([BE-RWI-248](../task/report/backend/BE-RWI-248.md), [BE-RWI-249](../task/report/backend/BE-RWI-249.md)); TERVERIFIKASI FE ([FE-RWI-261](../task/report/frontend/FE-RWI-261.md)) |
| `RWI-AC-466` | Tempat tidur bertanda khusus ODC dilindungi dan ditolak oleh backend jika dialokasikan untuk pasien rawat inap reguler (non-ODC) | `BE-RWI-249` ✅, `FE-RWI-261` ✅ | TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)); TERVERIFIKASI FE ([FE-RWI-261](../task/report/frontend/FE-RWI-261.md)) |
| `RWI-AC-467` | Pasien ODC yang mengalami perburukan kondisi atau dirawat melebihi 24 jam dapat dimutasi ke tempat tidur reguler via transfer bed management tanpa penolakan | `BE-RWI-249` ✅ | TERVERIFIKASI BE ([BE-RWI-249](../task/report/backend/BE-RWI-249.md)) |
| `RWI-AC-468` | Formulir persetujuan umum (General Consent) menampilkan klausul edukasi khusus batasan 24 jam dan ketentuan eskalasi mutasi rawat inap reguler | `FE-RWI-262` ✅ | TERVERIFIKASI FE ([FE-RWI-262](../task/report/frontend/FE-RWI-262.md)) |

---

## 3. Matriks Aturan Validasi Backend (Validation Matrix Mapping)

| Kode Validasi | Guard / Titik Validasi | Perilaku Validasi | Task Penanggung Jawab |
|---|---|---|:---:|
| `VAL-ADM-10` | `EvaluatePlacementEligibilityCore` & `OpenAdmissionAsync` | Admisi baru pasien ODC wajib memilih bed dengan `IsOdcBed == true`. Pelanggaran menghasilkan `422 Unprocessable Entity` (`ODC_BED_REQUIRED`) | `BE-RWI-248` ✅, `BE-RWI-249` ✅, `FE-RWI-261` ✅ |
| `VAL-ADM-11` | `EvaluatePlacementEligibilityCore` & `OpenAdmissionAsync` | Pasien non-ODC dilarang menempati bed dengan `IsOdcBed == true`. Pelanggaran menghasilkan `422 Unprocessable Entity` (`ODC_BED_RESERVED`) | `BE-RWI-249` ✅, `FE-RWI-261` ✅ |
| `VAL-ADM-12` | `EvaluatePlacementEligibilityCore` (`Transfer`) | Konteks `InpPlacementContext.Transfer` mengecualikan aturan `VAL-ADM-10`, meloloskan transfer pasien ODC ke bed reguler saat rawat > 24 jam | `BE-RWI-249` ✅ |
| `VAL-ADM-13` | `OpenAdmissionAsync` | Pembuatan admisi pasien bertipe ODC menolak request jika `DoctorId` kosong/null dengan `422 Unprocessable Entity` (`PRIMARY_DOCTOR_REQUIRED`) | `BE-RWI-249` ✅, `FE-RWI-260` ✅ |
| `VAL-ADM-14` | Formulir General Consent | Lembar General Consent wajib menampilkan teks klausul edukasi batas observasi 24 jam dan klausul peralihan rawat reguler | `FE-RWI-262` ✅ |

---

## 4. Matriks Skenario Pengujian UAT (UAT Scenarios Traceability)

| Skenario UAT | Deskripsi Skenario Uji | Task Terkait | Target Kriteria |
|---|---|---|---|
| `UAT-ODC-01` | **Admisi Pasien ODC Normal (< 24 Jam):** Petugas mendaftarkan admisi pasien Ny. Linda memilih tipe ODC, memilih DPJP dr. Herman Sp.OG, memilih bed ODC Flamboyan ODC-01, menandatangani General Consent ODC ber-klausul 24 jam, dan berhasil admisi | `BE-RWI-248`, `BE-RWI-249`, `FE-RWI-260`, `FE-RWI-261`, `FE-RWI-262` | RWI-AC-463, 464, 465, 468 |
| `UAT-ODC-02` | **Pencegahan Salah Alokasi Bed ODC:** Pasien umum reguler mencoba dialokasikan ke Bed ODC-01 ditolak sistem (`ODC_BED_RESERVED`); pasien ODC mencoba dialokasikan ke Bed Reguler ditolak sistem (`ODC_BED_REQUIRED`) | `BE-RWI-249`, `FE-RWI-261` | RWI-AC-465, 466 |
| `UAT-ODC-03` | **Eskalasi Mutasi Perawatan ODC (> 24 Jam):** Pasien ODC yang mengalami komplikasi dipindahkan oleh perawat via modul Transfer Bed ke Bed Reguler Kelas 1 Kamar 305 Bed A; transfer berhasil tanpa error 422 | `BE-RWI-249` | RWI-AC-467 |

---

## 5. Ringkasan Ketercakupan (Zero Coverage Gap)

- **Total Functional Requirements:** 7 (`FR-ODC-001` s.d. `FR-ODC-007`) — **Terpetakan 100% (7/7)**.
- **Total Kriteria Penerimaan:** 6 (`RWI-AC-463` s.d. `RWI-AC-468`) — **Terpetakan 100% (6/6)**.
- **Total Validasi Backend:** 5 (`VAL-ADM-10` s.d. `VAL-ADM-14`) — **Terpetakan 100% (5/5)**.
- **Total Skenario UAT:** 3 (`UAT-ODC-01` s.d. `UAT-ODC-03`) — **Terpetakan 100% (3/3)**.
- **Coverage Gap:** **0 (Nol Celah)**.
