# Requirement Traceability — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 4
status: FORWARD-TEST / DRAFT
contract_version: v4 (approved)
backend_source_sha: 55ac6ab
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
decision_revision: 11
acceptance_criteria_range: AC-BD-001 .. AC-BD-097
task_range_backend: BE-BD-001 .. BE-BD-016
task_range_frontend: FE-BD-001 .. FE-BD-012
```

---

## 0. Apa yang dijaga dokumen ini

Dokumen ini menjawab satu pertanyaan: **apakah setiap kebutuhan yang sudah diputuskan punya task yang
mengerjakannya, dan punya cara membuktikan bahwa ia benar-benar dikerjakan.**

Ia **tidak** menilai kesiapan modul, **tidak** menyetujui apa pun, dan **tidak** menggantikan
`verify-module-readiness`. Kalau sebuah baris di sini menunjuk task yang belum jalan, itu berarti
penelusurannya utuh tetapi buktinya belum ada — dua hal yang berbeda.

---

## 1. Kebutuhan bisnis ke task

| Kebutuhan | Keputusan | Task backend | Task frontend | Keadaan |
| --- | --- | --- | --- | --- |
| Katalog komponen darah terkendali | `DEC-BD-024`, `DEC-BD-032` | ✅ `BE-BD-001` | 🟡 `FE-BD-001` | Backend **terbukti**, frontend siap |
| Daftar alasan berkategori | `DEC-BD-044`, `DEC-BD-024` | ✅ `BE-BD-001` | 🟡 `FE-BD-001` | Backend **terbukti** |
| Kewenangan unit memesan darah dari konfigurasi | `DEC-BD-012` | ✅ `BE-BD-002` | — | Penegakan diteruskan ke ⛔ `BE-BD-003` |
| Lokasi penyimpanan darah dikelola | `DEC-BD-035`, `DEC-BD-037` | ✅ `BE-BD-014` | 🟡 `FE-BD-011` | Backend **terbukti**, frontend siap |
| Hak akses per tindakan | `DEC-BD-039`..`047` | 🟡 `BE-BD-016` 12/39 | — | Sisa lahir bersama controller pemakainya |
| **Pemeriksaan golongan darah** | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026` | 🟡 `BE-BD-005` | ⛔ `FE-BD-005` sebagian | **Jalur terbuka** |
| **Penyelesaian konflik golongan darah** | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` | 🟡 `BE-BD-011` | ⛔ `FE-BD-009` | **Jalur terbuka** |
| Order darah dan pembatalannya | `DEC-BD-004/005/006/044` | ⛔ `BE-BD-003` | ⛔ `FE-BD-002` | Tertahan `G4` |
| Permintaan PMI dan penerimaan | `DEC-BD-002/003/008/020` | ⛔ `BE-BD-004` | ⛔ `FE-BD-003` | Tertahan `G4` |
| Penyimpanan dan perpindahan kantong | `DEC-BD-036`, `DEC-BD-037` | ⛔ `BE-BD-015` | ⛔ `FE-BD-012` | Tertahan lewat `BE-BD-004` |
| Alokasi kantong | `DEC-BD-003/007/029` | ⛔ `BE-BD-006` | ⛔ `FE-BD-004` | Tertahan lewat `BE-BD-015` |
| Bukti kecocokan dan pemberian | `DEC-BD-013/027/028/038/042` | ⛔ `BE-BD-007` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-006` |
| Pemberian jalur darurat | `DEC-BD-017/038/040` | ⛔ `BE-BD-008` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-007` |
| Penyelesaian kantong `PendingReview` | `DEC-BD-019/028/043/045` | ⛔ `BE-BD-009` | ⛔ `FE-BD-007` | Tertahan lewat `BE-BD-006/007` |
| Koreksi pencatatan pemberian | `DEC-BD-030/034/041` | ⛔ `BE-BD-010` | ⛔ `FE-BD-008` | Tertahan lewat `BE-BD-007` |
| Tindakan Bank Darah tercatat | `DEC-BD-021`, `DEC-BD-034` | ⛔ `BE-BD-012` | ⛔ `FE-BD-010` | Tertahan `G4` |
| Layar terjangkau dari menu | — | — | 🟡 `FE-BD-006` | Siap |
| Penyaluran biaya ke Billing | `DEC-BD-016` **OPEN** | — `BE-BD-013` | — | **Future scope** |

---

## 2. Acceptance criteria ke task

| Rentang AC | Task | Keadaan bukti |
| --- | --- | --- |
| `AC-BD-055`, `AC-BD-056` | ✅ `BE-BD-001` | **Terbukti** — laporan tracked, 56 test lulus |
| `AC-BD-015`, `AC-BD-016` | ✅ `BE-BD-002` | **Terbukti** — 8 test lulus |
| `AC-BD-064` | ✅ `BE-BD-014` | **Terbukti** — 25 test lulus |
| `AC-BD-013` | ⛔ `BE-BD-003` | Diteruskan dari `BE-BD-002`; menunggu jalur order |
| `AC-BD-030/034/035/077/078` | 🟡 `BE-BD-005` | **Belum ada bukti** — task siap dijadwalkan |
| `AC-BD-036/037/051/053/054/079/080` | 🟡 `BE-BD-011` | Belum ada bukti |
| `AC-BD-001/002/003/004/010/011/017/095/096/097` | ⛔ `BE-BD-003` | Tertahan `G4` |
| `AC-BD-005/006/009/022/023/031/032/033/059` | ⛔ `BE-BD-004` | Tertahan `G4` |
| `AC-BD-060/061/063/066/067/068/069/070` | ⛔ `BE-BD-015` | Tertahan |
| `AC-BD-062`, `AC-BD-065` | ⛔ `BE-BD-015` | Diteruskan dari `BE-BD-014` |
| `AC-BD-043/044/045/046/071` | ⛔ `BE-BD-006` | Tertahan |
| `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` | ⛔ `BE-BD-007` | Tertahan |
| `AC-BD-020/021/074/075/081/082/083/084/085` | ⛔ `BE-BD-008` | Tertahan |
| `AC-BD-007/008/024/025/029/092/093/094` | ⛔ `BE-BD-009` | Tertahan |
| `AC-BD-047/048/049/050/086/087/088` | ⛔ `BE-BD-010` | Tertahan |
| `AC-BD-026/058` | ⛔ `BE-BD-012` | Tertahan `G4` |
| `AC-BD-027` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka |

**Hitungan bukti.** Dari 97 acceptance criteria, **5 sudah terbukti** (`AC-BD-015/016/055/056/064`),
**1 tidak dapat diuji** karena keputusan terbuka (`AC-BD-027`), dan **91 sisanya belum diuji** —
12 di antaranya menunggu task yang sudah siap dijadwalkan, 79 menunggu `G4`.

---

## 3. Coverage gap yang diakui

| Gap | Keadaan | Akibat |
| --- | --- | --- |
| **Provider number-series** (`BD-DEP-017` / `G4`) | `DEC-PLT-007` `draft`; `PLT-SLICE-01` `BUSINESS_DECISION_REQUIRED`; `OQ-PLT-007` belum ada pemiliknya | **79 acceptance criteria belum dapat diuji.** Gap terbesar modul ini |
| **Asal `SampleIdentifier`** | `data/data-dictionary.md:250` menetapkannya `string(50)`, wajib dan unik, tetapi **tidak menyebut dari mana nilainya datang**. Bukan salah satu dari tiga field number-series | Dicatat sebagai gap penelusuran, **bukan blocker**. Bila ternyata harus dibuat server, `BE-BD-005` ikut terkena `G4` — builder wajib berhenti dan melapor |
| Penyaluran biaya Billing | `DEC-BD-016` `OPEN DECISION` | `AC-BD-027` tidak dapat diuji; `BE-BD-013` di luar gelombang mana pun |
| Jam masa berlaku bukti per komponen | `OQ-BD-012` | **Tidak** menahan task; nilainya dari konfigurasi master. Selama kosong, gerbang menolak |
| Keadaan kantong setelah dikoreksi | `OQ-BD-014` | Menahan detail implementasi `BE-BD-010`, bukan bentuknya |
| Rumah slice resmi `BR-BD-020` | Penilaian kelengkapan requirement masih revisi 2 | Tidak menahan task; diperlakukan sebagai perluasan `BD-SLICE-03/04/10` |
| Dua master baru tanpa baris `BD-CAP-*` | `MstBloodStorageLocation`, `MstBloodBankReason` | Audit penuh peta kemampuan disarankan sebelum `MVP-2` |
| Rujukan pola Laboratorium bergeser | `BD-CAP-008` kini `LabSpecimen.cs` + `LabExamination.cs`; `BD-CAP-009` kini `LabTransitionHistory.cs` | Sudah diperbarui pada peta kemampuan revisi 5. Builder `BE-BD-003`/`004` wajib memakai nama baru |

---

## 4. Keputusan yang masih terbuka dan pengaruhnya

| ID | Ringkasan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| `OQ-PLT-007` | Backend Engineering Contract Owner belum ditunjuk | **`G4`** → 9 task backend, 8 task frontend | Authority engineering/architecture |
| `DEC-BD-016` | Persetujuan pemilik Billing atas konteks sumber biaya | `BE-BD-013` future scope | Pemilik BillingManagement |
| `OQ-BD-011` | Mekanik label golongan darah | Slice label | Pemilik proses klinis |
| `DEF-BD-003` | Apakah semua komponen menuntut bukti kecocokan sama | Aturan per komponen saat implementasi | Pemilik proses klinis |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong | Kegunaan `RETURNED_TO_PROVIDER` | Pemilik proses BDRS |
| `OQ-BD-012` | Jam masa berlaku bukti kecocokan per komponen | Implementasi gerbang pemberian | Pemilik proses klinis |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru setelah dikoreksi | Implementasi jalur koreksi | Pemilik proses BDRS |
| `OQ-BD-016` | Apakah bukti pendukung koreksi menuntut lampiran | Bentuk kolom bukti pendukung | Pemilik proses BDRS |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada | Penelusuran bukti ke kebutuhan | Pemilik kebutuhan |

**Hanya `OQ-PLT-007` yang memblokir task.** Delapan lainnya menyangkut scope di luar rilis pertama,
detail implementasi yang nilainya datang dari konfigurasi, atau satu baris seeder — dipertahankan
supaya tidak hilang, bukan sebagai penahan.

---

## 5. Ringkasan hitungan

| Dimensi | Angka |
| --- | ---: |
| Task backend seluruhnya | 16 |
| — ✅ selesai | 3 |
| — 🟡 selesai sebagian | 1 |
| — 🟡 pending, siap dijadwalkan | 2 |
| — ⛔ blocked | 9 |
| — future scope | 1 |
| Task frontend seluruhnya | 12 |
| — 🟡 pending, siap dijadwalkan | 3 |
| — ⛔ blocked | 9 |
| **Total task** | **28** (27 dalam gelombang + 1 future scope) |
| **Dapat dijadwalkan hari ini** | **5** — `BE-BD-005`, `BE-BD-011`, `FE-BD-001`, `FE-BD-006`, `FE-BD-011` |
| Acceptance criteria seluruhnya | 97 |
| — terbukti | 5 |
| — tidak dapat diuji (keputusan terbuka) | 1 |
| — belum diuji | 91 |
| Keputusan bisnis | `DEC-BD-001`..`047` |
| Gerbang tertutup | `G1`, `G2a`, `G2b` |
| Gerbang terbuka | **`G4`** |

**Penelusuran utuh.** Setiap kebutuhan yang sudah diputuskan punya task pemilik, dan setiap task punya
acceptance criteria. Nol requirement yatim. Yang kurang bukan penelusurannya, melainkan **bukti** —
dan sebagian besar bukti itu menunggu satu gerbang yang belum ada pemiliknya.
