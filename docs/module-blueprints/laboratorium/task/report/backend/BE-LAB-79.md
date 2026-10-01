# Laporan Perubahan Backend — `BE-LAB-79`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-79` |
| Judul | Pengesah pada respons hasil Mikrobiologi dan label order Mikrobiologi |
| Slice | Gelombang `MVP-10a` — `EPIC-LAB-16`, jalur baca `S4d-1` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6al.2** |
| Trace | `FR-16.4`, `FR-16.7`; `LAB-DEC-097`, `LAB-DEC-120`, `LAB-DEC-135` |
| Contract version | `LAB-API-v1` **`r35`** 30.3 dan 30.5; bentuk ruas `r34` 29.3 dan 29.5 — **`approved` 2026-09-25** |
| Dependency | `BE-LAB-78` ⚠ (rilis Mikrobiologi berjalan pada kode), `BE-LAB-76` ✅ (bentuk ruas, turunan `resultStatus`, `LabOrderResultProgressRules`) |
| Klasifikasi | `MEDIUM` — skor 7: berkas diubah 2 (5), logika bisnis 1, kontrak API 2 (ruas baru; ruas lama berubah nilai), database 1 (kueri baca), keamanan 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7ff35b8c` (branch `yoga`), di atas perubahan `BE-LAB-78` dan `BE-LAB-84`..`86` yang belum ter-commit |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — ruas pengesah dan keadaan pada `GET /{id}/result/microbiology` terbukti pada **tiga titik baca lewat tindakan sungguhan** (harness 13/13), nama pelaku **satu kueri** (6 = 6 = 6), label order Mikrobiologi benar pada tiga keadaan, `orderStatus` tak tersentuh. **HTTP terhadap PostgreSQL** pada keempat jalur baca: ruas hadir, pengesah kosong sebelum validasi (`AC-183`), LAB-RSMMC-000014 `InProgress`, Patologi Anatomi tetap kosong, Patologi Klinik tidak berubah. Build 0 error; nol migration; nol kolom tersimpan. Titik baca *sesudah validasi* dan *sesudah rilis* lewat HTTP menunggu rilis Mikrobiologi pertama (`MVP-10c`) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabMicrobiologyResultService`, `LabMicrobiologyResultDtos`, `LabMonitoringService`, `LabOrderService.GetDetailAsync` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` (ruas baru pada DTO), `QBE-ENT-003` (nol kolom presentasi — `resultStatus` dan `resultProgress` turunan). **Tidak berlaku:** `QBE-PERM-*` (hak baca yang sudah ada), `QBE-LOG-001` (baca saja), `QBE-DB-*` |
| Governance yang dibaca | Sama dengan `BE-LAB-78` pada sesi yang sama |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-78`, hasil Mikrobiologi dapat divalidasi dan dirilis — tetapi respons hasil Mikrobiologi
masih **sengaja** menulis pengesah kosong dan `isReleased = false`, dan daftar Pemeriksaan Mikrobiologi
tidak punya label order. Layar Halaman Hasil Mikrobiologi tidak dapat menampilkan *Validasi oleh* dan
*Petugas Otorisasi*, dan tidak tahu apakah hasil sudah boleh dikirim.

---

## 2. Proses bisnis

**Halaman Hasil Mikrobiologi** kini membaca pengesah **sebenarnya**:

| Titik | *Validasi oleh* | *Petugas Otorisasi* | Keadaan |
| --- | --- | --- | --- |
| Sebelum validasi (`AC-183`) | kosong | kosong | `Final` |
| Sesudah validasi | nama pemvalidasi (dr. Nabila) | kosong | `Validated` |
| Sesudah rilis | nama pemvalidasi | nama **perilis** | `Released`, `isReleased = true` |

Pengesah **tidak pernah** diisi dari pencetak atau penulis hasil (`LAB-DEC-120`). Bila dokter tunggal
memvalidasi dan merilis sendiri, kedua penanda pengecualian tampil dengan bunyi yang sama persis dengan
halaman hasil Patologi Klinik.

**Label order Mikrobiologi** — daftar Pemeriksaan Mikrobiologi dan detail order:

| Keadaan | Label |
| --- | --- |
| Kultur urin dirilis, kultur darah masih *Sementara* (`AC-199`) | `InProgress` — hasil sementara menahan *Selesai* |
| Seluruh pemeriksaan dirilis | `AllReleased` |
| Order tanpa pemeriksaan | kosong |

Patologi Anatomi tetap tanpa label — rilisnya belum ada. `orderStatus` tidak berubah oleh rilis mana pun:
*Selesai* order tetap tindakan manual (`LAB-DEC-154`, `BE-LAB-81`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6al.2 | Cakupan, tiga jebakan, verifikasi, DoD |
| `contracts/api-contract.md` 30.3, 30.5; 29.3, 29.5 | Daftar ruas dan artinya |
| `testing/acceptance-test-matrix.md` amandemen `S4d-1` | Baris *30.3 — ruas pengesah*, `AC-199` Mikrobiologi |
| `Services/LabMicrobiologyResultService.cs` — `GetResultAsync`, `ReadAnalystNameAsync` | Ruas yang sengaja kosong; pembacaan nama |
| `Services/LabExaminationService.cs` — `DeriveResultStatus`, penanda pengecualian, `BuildResultFormsAsync` | Rumus dan bunyi yang wajib dipakai ulang; rumus nama pelaku `BE-LAB-76` |
| `Services/LabMonitoringService.cs`, `LabOrderService.cs` | Titik `resultProgress` yang dikunci Patologi Klinik |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabMicrobiologyResultDtos.cs` | Sembilan ruas baru persis `r35` 30.3 — `ResultStatus`, `ValidatedAt`, `ValidatedByUserId`, `ValidatedByPositionName`, `ValidationExceptionMarker`, `ReleasedAt`, `ReleasedByUserId`, `ReleasedByPositionName`, `ReleaseExceptionMarker` (`ResultEnteredByUserId` sudah ada). Dokumentasi `AuthorizingOfficerName`, `ValidatedByName`, `IsReleased` diperbarui |
| `Services/LabMicrobiologyResultService.cs` | Blok *"SENGAJA dibiarkan kosong… S4d"* diganti: keadaan dari `LabExaminationService.DeriveResultStatus`; `IsReleased` dari `ReleasedAt`; pengesah dari kolom pemeriksaan; penanda dari pembantu bersama `BE-LAB-76`. `ReadAnalystNameAsync` → **`ReadActorNamesAsync`** — analis, pemvalidasi, dan perilis dalam **satu** kueri, dilewati bila tak ada pelaku tercatat |
| `Services/LabMonitoringService.cs` | `resultProgress` bagi disiplin di `LabReleasableDisciplines` — PK dan Mikrobiologi |
| `Services/LabOrderService.cs` | Detail order: sama; komentar merujuk `LAB-DEC-154` |

**Nol endpoint, nol entity, nol migration, nol kolom tersimpan.** Registri tetap 1577.

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Pengesah dari pencetak atau penulis hasil | Nama dari `ValidatedByUserId`/`ReleasedByUserId`, hanya bila `ValidatedAt`/`ReleasedAt` terisi | Pemvalidasi = dr. Nabila, bukan analis; sebelum validasi kosong |
| Satu kueri nama per ruas | `ReadActorNamesAsync` membaca ketiganya sekaligus | Jumlah kueri baca sama pada tiga titik (6 = 6 = 6); tanpa pelaku 5 |
| Menyetel `Completed` saat seluruhnya dirilis | `resultProgress` turunan; nol penulisan `OrderStatus` | Order `AllReleased` tetap `InProcess` |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Gerbang `resultProgress` | `LabReleasableDisciplines.Contains`, bukan menambah Mikrobiologi sebagai konstanta kedua — satu jawaban dengan penjaga validasi dan laporan. Patologi Anatomi otomatis ikut saat `S4e` menambahkannya ke himpunan |
| Rumus nama | Analis tetap `DisplayName` (perilaku lama); pemvalidasi dan perilis `DisplayName ?? UserName ?? Email ?? UserCode` — sama dengan lembar hasil Patologi Klinik |
| `deliveryBlockedReason` | **Tidak** ditambahkan — tidak ada di daftar `r35` 30.3; layar memakai `isReleased` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `r35` 30.3 dan 30.5. **Ruas lama berubah nilai**: `validatedByName`, `authorizingOfficerName`, `isReleased` — sebelumnya selalu kosong/salah; nol konsumen mengandalkannya (30.6). `resultProgress` kini terisi pada `GET /lab-monitoring/microbiology` dan detail order Mikrobiologi |
| Database | **Baca saja**; satu kueri nama per respons hasil; satu kueri berkelompok per halaman daftar |
| Keamanan/Auth | Hak baca yang sudah ada. Nama dan jabatan pengesah memang untuk ditampilkan; nol data pasien baru |

---

## 4. Dokumentasi endpoint

Nol endpoint baru; tiga endpoint lama bertambah atau berubah ruas.

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-examinations/{id}/result/microbiology` | Pengesah terisi; sembilan ruas `r35` 30.3 | `LabExamination : Read` |
| `GET` | `/lab-monitoring/microbiology` | Setiap item bertambah `resultProgress` | Tetap |
| `GET` | `/lab-orders/{id}` | `resultProgress` kini terisi bagi order Mikrobiologi | Tetap |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` | **0 error, 230 warning**; nol dari berkas yang disentuh | `PASS` | Keluaran build |
| Harness `BE-LAB-79` | **13 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `77` 21/21, `78` 29/29, `81` 25/25, `84` 15/15, `86` 19/19 | `PASS` | Harness yang sama |
| **Regresi yang berubah — disengaja** | `BE-LAB-76`: tepat **dua** baris — *"Jalur Mikrobiologi — resultProgress kosong"* dan *"Detail order Mikrobiologi → kosong"* — kini terisi, sesuai `r35` 30.5 | `PASS` — perubahan yang disetujui | Harness `BE-LAB-76` |
| Startup | Registri tetap 1577; nol galat tak dikenal | `PASS` | Log |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |

**Rincian HTTP** — superadmin, baca saja.

| Panggilan | Hasil |
| --- | --- |
| `GET /lab-examinations/{Pewarnaan BTA Sputum}/result/microbiology` (Final, belum divalidasi) | `200` — `resultStatus = Final`; `validatedByName` dan `authorizingOfficerName` **kosong** (`AC-183`); `isReleased = false`; kesembilan ruas baru hadir (kosong); `resultEnteredByUserId` dan `analystName` terisi |
| `GET /lab-monitoring/microbiology` | `200` — LAB-RSMMC-000014 (dua BTA belum dirilis) → `InProgress`; empat order tanpa pemeriksaan → kosong; `orderStatus` tetap `Requested` |
| `GET /lab-orders/{LAB-RSMMC-000014}` | `200` — `resultProgress = InProgress` |
| `GET /lab-monitoring/anatomic-pathology` | `200` — `resultProgress` kosong pada setiap item |
| Regresi `GET /lab-monitoring/clinical-pathology` | `200` — tiga `InProgress`, tiga kosong, sama dengan `BE-LAB-76` |

**Rincian harness** — tindakan validasi dan rilis **sungguhan** (`LabResultValidationService`), pembacaan lewat
`LabMicrobiologyResultService` yang dibangun DI.

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-183`** — sebelum validasi | Kedua pengesah kosong; `Final`; `isReleased = false`; analis terisi |
| Sesudah validasi dr. Nabila | `validatedByName = dr. Nabila, Sp.MK` (bukan analis); jabatan snapshot; `authorizingOfficerName` kosong; `Validated` |
| Sesudah rilis | `authorizingOfficerName = Perilis Mikro`; `isReleased = true`; `Released`; jabatan perilis |
| Penanda tanpa pengecualian | Keduanya kosong |
| **Jumlah kueri** pada tiga titik | **6 = 6 = 6** — nama pelaku satu kueri |
| Dokter tunggal dengan alasan pengecualian | *"Divalidasi oleh pengisi sendiri — dr. Tunggal, Sp.MK — Shift tunggal"* dan *"Dirilis oleh pemvalidasi sendiri — …"* |
| **`AC-199` Mikrobiologi** — urin dirilis, darah Sementara | `InProgress` pada daftar **dan** detail |
| Seluruh dirilis; tanpa pemeriksaan; Patologi Anatomi | `AllReleased`; kosong; kosong |
| **`orderStatus`** sesudah rilis | Tetap `InProcess` |
| Tanpa pelaku tercatat | Nama kosong; kueri nama dilewati (5 kueri) |

**Tidak dijalankan:**

- Titik *sesudah validasi* dan *sesudah rilis* lewat HTTP — menunggu rilis Mikrobiologi pertama
  (`MVP-10c`: kode `LAB-COORD-016`, penunjukan, kebijakan jabatan). Terbukti pada harness dengan tindakan
  sungguhan.
- Log SQL PostgreSQL untuk jumlah kueri — dihitung pada tingkat EF, seperti `BE-LAB-76`.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-183` — kedua ruas kosong sebelum pengesahan | ✅ **Terpenuhi** — harness dan HTTP | — |
| Baris *30.3 — ruas pengesah* pada tiga titik baca | ✅ **Terpenuhi** pada harness; titik pertama juga lewat HTTP | — |
| `AC-199` Mikrobiologi | ✅ **Terpenuhi** pada harness; `InProgress` lewat HTTP pada order dev | — |
| Verifikasi roadmap — nama pelaku satu kueri; `orderStatus` tak berubah | ✅ **Terpenuhi** | — |
| DoD — pengesah sebenarnya; `resultProgress` Mikrobiologi benar pada tiga keadaan; `orderStatus` tak tersentuh; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | Dua baris harness `BE-LAB-76` kini sengaja gagal (5) — disimpan apa adanya sebagai bukti perubahan perilaku |
| Risiko tersisa | **Rendah.** Seluruhnya jalur baca |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M` `DTOs/LabMicrobiologyResultDtos.cs`, `Services/LabMicrobiologyResultService.cs`, `Services/LabMonitoringService.cs`, `Services/LabOrderService.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-78`, `84`..`86` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-80` (antrean validasi dua disiplin, `VAL-145`) tetap `SIAP DIKERJAKAN` — penutup `MVP-10a`. **2.** Langkah rilis `MVP-10c` tetap `BLOCKED` (6al.5) |
